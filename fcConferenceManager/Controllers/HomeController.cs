using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using fcConferenceManager.Models;
using MAGI_API.Models;
using MAGI_API.Security;

namespace fcConferenceManager.Controllers
{
    [CheckActiveEventAttribute]
    public class HomeController : Controller
    {
        static SqlOperation repository = new SqlOperation();
        // GET: Home
        internal static string ReadConnectionString()
        {
            string connString = string.Format("Data Source={0};", ConfigurationManager.AppSettings["AppS"].ToString());
            connString += string.Format("Uid={0};", ConfigurationManager.AppSettings["AppL"].ToString());
            connString += string.Format("Pwd={0};", ConfigurationManager.AppSettings["AppP"].ToString());
            connString += string.Format("Database={0};", ConfigurationManager.AppSettings["AppDB"].ToString());
            connString += string.Format("Connect Timeout={0};", ConfigurationManager.AppSettings["AppT"].ToString());
            connString += string.Format("MultipleActiveResultSets={0};", "true");
            return connString;
        }
        private DataTable GetRefreshFilteredLinks(DataTable RefreshLinks, string strPartnerAlias, int Group)
        {
            DataTable Filter = RefreshLinks.Select("GroupID=" + Group.ToString()).CopyToDataTable();
            DataRow dr = Filter.Select("LinkText='Partners'").FirstOrDefault();
            if (dr != null)
                dr["LinkText"] = dr["LinkText"].ToString().Replace("Partner", strPartnerAlias);

            dr = Filter.Select("LinkText='Become a Partner'").FirstOrDefault();
            if (dr != null)
            {
                Regex exp = new Regex(@"\b[aeiou]\w*");
                if (dr["LinkText"].ToString().Contains("a Partner") && exp.IsMatch(strPartnerAlias.ToLower()))
                    dr["LinkText"] = dr["LinkText"].ToString().Replace("a Partner", "an " + strPartnerAlias);
                else
                    dr["LinkText"] = dr["LinkText"].ToString().Replace("Partner", strPartnerAlias);
            }

            return Filter;
        }
        private void InitializeEventsCollection(clsLastUsed cLast, clsSettings cSettings)
        {
            DataTable dt = new DataTable();
            dt = repository.InitializeEventsCollection();
            if (dt != null)
            {
                ViewData["ddHomeEvent"] = dt;
                cLast.colEventControlpKeys.Clear();
                foreach (DataRow dr in dt.Rows)
                {
                    clsOneEvent c = new clsOneEvent();
                    c.intEventPkey = Convert.ToInt32(dr["pKey"]);
                    c.strEventID = dr["EventID"].ToString();
                    if (!Convert.IsDBNull(dr["EventType_PKey"])) c.intEventType_PKey = Convert.ToInt32(dr["EventType_PKey"]);
                    if (!Convert.IsDBNull(dr["HomelinkDisable"])) c.bHomelinkDisable = Convert.ToBoolean(dr["HomelinkDisable"]);
                    c.strVenueID = dr["Venue"].ToString();
                    c.strLocationCity = dr["LocationCity"].ToString();
                    c.strLocationState = dr["LocationState"].ToString();
                    c.strEventFullname = dr["EventFullname"].ToString();
                    c.strVenueSmall = dr["VenueSmall"].ToString();
                    c.strEventHomeBanner = dr["HomeBanner"].ToString();
                    if (!Convert.IsDBNull(dr["StartDate"])) c.dtStartDate = Convert.ToDateTime(dr["StartDate"]);
                    if (!Convert.IsDBNull(dr["EndDate"])) c.dtEndDate = Convert.ToDateTime(dr["EndDate"]);
                    cLast.colEventControlpKeys.Add(c, "E" + c.intEventPkey.ToString());
                    if (!Convert.IsDBNull(dr["RowIndex"]))
                    {
                        if (cLast.intActiveEventPkey == Convert.ToInt32(dr["pKey"]))
                            cLast.intEventControlCurrentMVC = Convert.ToInt32(dr["RowIndex"]);
                    }
                    c = null;
                }
                int intEventPkey = 0;
                if (cLast.intEventControlCurrentMVC > 1)
                {
                    clsOneEvent cOneEvent = (clsOneEvent)cLast.colEventControlpKeys[cLast.intEventControlCurrentMVC];
                    intEventPkey = cOneEvent.intEventPkey;
                }
                cLast.SetEventControl(intEventPkey > 0 ? intEventPkey : (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey));
            }
            Session["cLastUsed"] =cLast;
        }
        #region NavMenu 
        private bool IsAccountSpeaker(int account_pkey, int Event_pkey)
        {
            bool result = false;
            try
            {
                string qry = "Select Count(t1.pkey) as SpkCount From eventsession_staff t1 inner join account_List t2 On t2.pKey = t1.Account_pKey LEFT JOIN Event_Sessions ES ON t1.EventSession_pkey=ES.pKey  LEFT JOIN Session_List SL ON ES.Session_pKey=SL.pKey  INNER JOIN SYS_SessionTypes ST ON SL.SessionType_pkey =ST.pKey Cross Apply dbo.getEventSession_Leader_Moderator(13299,SL.NumLeaders,SL.NumModerators) as PLM";
                qry = qry + " Where t1.Account_pKey=" + account_pkey.ToString() + " ANd ES.Event_pKey=" + Event_pkey.ToString();
                System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                DataTable dt = new DataTable();
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(qry);
                if (clsUtility.GetDataTable(sqlConn, cmd, ref dt))
                    result = (dt!=null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["SpkCount"]) > 0);
            }
            catch (Exception ex)
            {

            }
            return result;
        }
        private string getRolesForEvent(int account_pkey, int Event_pkey)
        {
            string result = "None";
            try
            {
                string qry = "Select dbo.getAccountRolesAtEvent(" + account_pkey.ToString() + "," + Event_pkey.ToString() + ",', ') as EventRoles";
                System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                DataTable dt = new DataTable();
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(qry);
                if (clsUtility.GetDataTable(sqlConn, cmd, ref dt))
                    result = dt.Rows[0]["EventRoles"].ToString();
            }
            catch (Exception ex)
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 0, "Error checking For event roles: " + ex.Message);
            }
            if (string.IsNullOrEmpty(result))
                result="None";
            return result;
        }
        private bool isAllowedByPriv(bool blueRibbonMode, bool Admin, DataTable dt, string strSection, string strNode, int intPriv)
        {
            bool b = Admin;
            if (!blueRibbonMode)
            {
                if (dt != null && dt.Rows.Count>0)
                    b = (Admin || dt.Select("ColKey = '" + "P" + intPriv.ToString() + "'").Count() >0);
            }

            if ((ConfigurationManager.AppSettings["QAMode"] == "1") && strSection == "Support" && strNode == "Issue Tracker")
                b= false;
            return b;
        }
        private bool MASTER_CheckExhibitor(int intOrganizationPkey = 0, int intEventPkey = 0, int intEventOrganizationpKey = 0, int Account_pkey = 0)
        {
            bool result = false;
            try
            {

                System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                DataTable dt = new DataTable();
                string qry = "EXEC ISValidORG @OrganizationPkey ,@Event_pkey ,@EventOrganizationpKey , @Account_pkey";
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(qry);
                cmd.Parameters.AddWithValue("@OrganizationPkey", intOrganizationPkey);
                cmd.Parameters.AddWithValue("@Event_pkey", intEventPkey);
                cmd.Parameters.AddWithValue("@EventOrganizationpKey", intEventOrganizationpKey);
                cmd.Parameters.AddWithValue("@Account_pkey", Account_pkey);
                if (clsUtility.GetDataTable(sqlConn, cmd, ref dt))
                    result = (dt!=null && dt.Rows.Count > 0);
            }
            catch (Exception ex)
            {

            }
            return result;
        }
        private bool CheckExhibitor(int intOrganizationPkey, int intEventPkey, int intEventOrganizationpKey = 0, int Account_pkey = 0, bool IsDemo = false)
        {
            bool result = false;
            try
            {
                System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                DataTable dt = new DataTable();
                string qry = "select t0.Pkey from Event_Organizations t0 where isnull(ShowOnEventPartner,0)=1 and Event_pKey=" + intEventPkey.ToString() + " and Organization_pKey=" + intOrganizationPkey.ToString();
                if (intEventOrganizationpKey > 0)
                    qry = qry + " and t0.pKey =" + intEventOrganizationpKey.ToString();
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(qry);
                if (clsUtility.GetDataTable(sqlConn, cmd, ref dt))
                    result = (dt!=null && dt.Rows.Count > 0);
            }
            catch (Exception ex)
            {

            }
            return result;
        }
        private EventFeatures LoadFeatures(string Features, string AttendeeAccess)
        {
            EventFeatures featureModel = new EventFeatures();
            if (!string.IsNullOrEmpty(Features))
            {
                string[] ArrFeatures = Features.Split(',');
                featureModel.bSch = ((ArrFeatures.ElementAt(0) == "1") ? true : false);
                featureModel.bNet =  ((ArrFeatures.ElementAt(2) == "1") ? true : false);
                featureModel.bDnCoupons = ((ArrFeatures.ElementAt(3) == "1") ? true : false);
                featureModel.bEdBadge = ((ArrFeatures.ElementAt(4) == "1") ? true : false);
                featureModel.bLunch = ((ArrFeatures.ElementAt(5) == "1") ? true : false);
                featureModel.bReferrer = ((ArrFeatures.ElementAt(6) == "1") ? true : false);
                featureModel.bBuy =  ((ArrFeatures.ElementAt(7) == "1") ? true : false);
                featureModel.bAttendees =((ArrFeatures.ElementAt(8) == "1") ? true : false);
                featureModel.bActivity = false;
                featureModel.bOptions = false;
                if (ArrFeatures.Length>9)
                {
                    featureModel.bActivity = ((ArrFeatures.ElementAt(9) == "1") ? true : false);
                    featureModel.bOptions = ((ArrFeatures.ElementAt(10) == "1") ? true : false);
                }
            }
            if (!string.IsNullOrEmpty(AttendeeAccess))
            {
                featureModel.intAttActivity = 0;
                featureModel.intAttOptions = 0;
                string[] ArrAttendee = AttendeeAccess.Split(',');
                featureModel.intAttSch = (string.IsNullOrEmpty(ArrAttendee[0])) ? 0 : Convert.ToInt32(ArrAttendee[0]);
                featureModel.intAttNet = (string.IsNullOrEmpty(ArrAttendee[2])) ? 0 : Convert.ToInt32(ArrAttendee[2]);
                featureModel.intAttDnCoupons = (string.IsNullOrEmpty(ArrAttendee[3])) ? 0 : Convert.ToInt32(ArrAttendee[3]);
                featureModel.intAttEdBadge = (string.IsNullOrEmpty(ArrAttendee[4])) ? 0 : Convert.ToInt32(ArrAttendee[4]);
                featureModel.intAttLunch = (string.IsNullOrEmpty(ArrAttendee[5])) ? 0 : Convert.ToInt32(ArrAttendee[5]);
                featureModel.intAttReferrer = (string.IsNullOrEmpty(ArrAttendee[6])) ? 0 : Convert.ToInt32(ArrAttendee[6]);
                featureModel.intAttBuy = (string.IsNullOrEmpty(ArrAttendee[7])) ? 0 : Convert.ToInt32(ArrAttendee[7]);
                featureModel.intAttAttendees = (string.IsNullOrEmpty(ArrAttendee[8])) ? 0 : Convert.ToInt32(ArrAttendee[8]);
                if (ArrAttendee.Length>9)
                {
                    featureModel.intAttActivity = (string.IsNullOrEmpty(ArrAttendee[9])) ? 0 : Convert.ToInt32(ArrAttendee[9]);
                    featureModel.intAttOptions = (string.IsNullOrEmpty(ArrAttendee[10])) ? 0 : Convert.ToInt32(ArrAttendee[10]);
                }
            }
            return featureModel;
        }
        public PartialViewResult _PageHeader()
        {
            LoadMenuRendering();

            return PartialView();
        }
        public PartialViewResult _NavMenu()
        {
            LoadMenuRendering();
            return PartialView();
        }
        private void UpComingSession(int AccountId, int EventId, clsSettings cSettings, DateTime dtCurrentTime)
        {
            ViewBag.MyScheduleNextSessionInfo =false;
            ViewBag.MyScheduleNextSpeakingInfo =false;
            DateTime CurrentTime = dtCurrentTime;
            DataTable dt = new SqlOperation().GetUpComingSession(AccountId, EventId, dtCurrentTime);
            if (dt != null && dt.Rows.Count>0)
            {
                System.Text.StringBuilder strSessions = new System.Text.StringBuilder("");
                System.Text.StringBuilder strSpeakerSessions = new System.Text.StringBuilder("");
                bool bAvailable = false, bSpeakerAvailable = false, isSpeaker = false, bWebinar = false;
                double intStartMinut = 0, intStartMinutUTC = 0;
                TimeSpan timeSpanMin = new TimeSpan(), timeSpanUtcMin = new TimeSpan();
                string SessionString = "", strLinkType = "", strSession = "", encString = "", sessionID = "";
                foreach (DataRow dr in dt.Rows)
                {
                    isSpeaker  = (dr["bSpeaker"] != System.DBNull.Value) ? Convert.ToBoolean(dr["bSpeaker"]) : false;
                    timeSpanMin = (CurrentTime - Convert.ToDateTime(dr["StartTime"].ToString()));
                    timeSpanUtcMin = DateTime.UtcNow.AddHours(-4) - Convert.ToDateTime(dr["StartTime"].ToString());
                    bWebinar = (isSpeaker &&
                                ((intStartMinut <= cSettings.intWebinarLinkShowSpeakerBefor || intStartMinutUTC <= cSettings.intWebinarLinkShowSpeakerBefor) &&
                                 CurrentTime <= Convert.ToDateTime(dr["EndTime"])))  ||
                                 ((intStartMinut <= cSettings.intWebinarLinkShowBefor || intStartMinutUTC <= cSettings.intWebinarLinkShowBefor) && CurrentTime <= Convert.ToDateTime(dr["EndTime"]));
                    SessionString = "";
                    if (bWebinar)
                    {
                        strLinkType =((dr["LinkType"] != System.DBNull.Value) ? dr["LinkType"].ToString() : "");
                        strSession =((dr["EvtSessionPKey"] != System.DBNull.Value) ? dr["EvtSessionPKey"].ToString() : "");
                        encString = "";
                        sessionID =((dr["SessionID"] != System.DBNull.Value) ? dr["SessionID"].ToString() : "");
                        switch (strLinkType)
                        {
                            case "Virtual":
                                encString = clsUtility.Encrypt(strSession);
                                SessionString = $", <a href='/Virtual/ZoomSession?ESPK={encString}' target='_blank' title='Attend session' >{sessionID}</a>";
                                break;
                            case "Schedule":
                                SessionString = $", <a href='/MyMAGI/MySchedule' target='_blank' title='Attend session' >{sessionID}</a>";
                                break;
                            case "Virtual Disable":
                                encString = clsUtility.Encrypt(strSession);
                                SessionString = $", <a href='/Virtual/ZoomSession?ESPK={encString}' target='_blank' title='Attend session' disabled='disabled' style='color:gray;pointer-events: none;'>{sessionID}</a>";
                                break;
                            case "Schedule Disable":
                            case "Disable":
                                SessionString = $", <a href='/MyMAGI/MySchedule' target='_blank' title='Attend session' disabled='disabled'  style='color:gray;pointer-events: none;' >{sessionID}</a>";
                                break;
                        }
                        if (isSpeaker)
                        {
                            bSpeakerAvailable= true;
                            strSpeakerSessions.Append(SessionString);
                        }
                        else
                        {
                            bAvailable=true;
                            strSessions.Append(SessionString);
                        }
                    }
                }
                ViewBag.MyScheduleNextSessionInfo =false;
                ViewBag.MyScheduleNextSpeakingInfo =false;
                if (bAvailable)
                {
                    ViewBag.lblMyScheduleNextSessionInfo ="My next session:" + strSessions.ToString().Trim(',');
                    ViewBag.MyScheduleNextSessionInfo= true;
                }
                if (bSpeakerAvailable)
                {
                    ViewBag.lblMyScheduleNextSpeakingSpot ="My next speaking spot:" + strSpeakerSessions.ToString().Trim(',');
                    ViewBag.MyScheduleNextSpeakingInfo= true;
                }
            }
        }
        #endregion NavMenu
        private void LoadMenuRendering()
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            clsSurrogate cSurr = ((clsSurrogate)Session["Surrogate"]);
            ViewBag.MyEventSection =false;
            ViewBag.MyStaffConsole =false;
            bool bSurrogateMode = false;
            if (cSurr != null)
                bSurrogateMode = cSurr.hasSurrogate;
            User_Login data = new User_Login();
            string strSection = "";
            bool blueRibbonMode = false;
            if (Session["BlueRibbonMode"]!= null)
                blueRibbonMode = ((bool)Session["BlueRibbonMode"]);
            bool bAnyVisibleInMenu = false, bIsPartner = false, bAnyVisibleInSec = false, bNewMessages = false, bOptions = false;
            bool LoggedIn = User.Identity.IsAuthenticated, bParticipatingInCurrentEvent = false, bAttendeeAtCurrEvent = false, bStaffForCurEvent = false, bDinnerPlanned = false, bShow = false;
            int NumTimesSpeakingCurEvent = 0, intNumNewContacts = 0, intRefrenceORG_pkey = 0;
            DateTime dtOneDayRegDate = new DateTime();
            if (LoggedIn && User.Identity.AuthenticationType == "Forms")
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(((FormsIdentity)User.Identity).Ticket.UserData);
            else
            {
                data.EventId = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
                data.Id = 0;
                data.GlobalAdmin =false;
                data.StaffMember= false;
            }

            ViewBag.LoggedIn = LoggedIn;
            ViewBag.ID =  data.Id;
            bool Admin = data.GlobalAdmin;
            int AcountID = data.Id, EventID = data.EventId;
            bool Speaker = IsAccountSpeaker(AcountID, EventID);

            if (data.Id > 0)
            {
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                DataTable AccountSettings = repository.getMenuAccountSettings(data.EventId, data.Id);
                if (AccountSettings != null && AccountSettings.Rows.Count > 0)
                {
                    bIsPartner =(AccountSettings.Rows[0]["IsPartner"] == DBNull.Value) ? false : Convert.ToBoolean(AccountSettings.Rows[0]["IsPartner"]);
                    bDinnerPlanned =(AccountSettings.Rows[0]["SpeakerDinnerActive"] == DBNull.Value) ? false : Convert.ToBoolean(AccountSettings.Rows[0]["SpeakerDinnerActive"]);
                    bAttendeeAtCurrEvent = (AccountSettings.Rows[0]["AttendeeAtCurrEvent"] == DBNull.Value) ? false : Convert.ToBoolean(AccountSettings.Rows[0]["AttendeeAtCurrEvent"]);
                    bStaffForCurEvent = (AccountSettings.Rows[0]["StaffForCurEvent"] == DBNull.Value) ? false : Convert.ToBoolean(AccountSettings.Rows[0]["StaffForCurEvent"]);
                    bParticipatingInCurrentEvent = (AccountSettings.Rows[0]["ParticipatingInCurrentEvent"] == DBNull.Value) ? false : Convert.ToBoolean(AccountSettings.Rows[0]["ParticipatingInCurrentEvent"]);
                    intNumNewContacts = (AccountSettings.Rows[0]["NewContacts"] == DBNull.Value) ? 0 : Convert.ToInt32(AccountSettings.Rows[0]["NewContacts"]);
                    intRefrenceORG_pkey = (AccountSettings.Rows[0]["RefrenceORG_pkey"] == DBNull.Value) ? 0 : Convert.ToInt32(AccountSettings.Rows[0]["RefrenceORG_pkey"].ToString());
                    NumTimesSpeakingCurEvent = (AccountSettings.Rows[0]["NumTimesSpeakingCurEvent"] == DBNull.Value) ? 0 : Convert.ToInt32(AccountSettings.Rows[0]["NumTimesSpeakingCurEvent"].ToString());
                    dtOneDayRegDate = (AccountSettings.Rows[0]["OneDayRegistrationDate"] == DBNull.Value) ? new DateTime() : Convert.ToDateTime(AccountSettings.Rows[0]["OneDayRegistrationDate"].ToString());
                }
            }
            DataTable ddMyEvent = (DataTable)TempData["dataEventsTable"];
            bShow  =   (data.StaffMember || data.GlobalAdmin || (ddMyEvent != null && ddMyEvent.Rows.Count>0));
            bNewMessages = (intNumNewContacts > 0);
            DataTable dt = new SqlOperation().LoadAccountPrivilages(EventID, AcountID);
            List<MVCNavMenu> menuList = new List<MVCNavMenu>();
            ViewBag.MENU_ADMINOPS = Admin;
            ViewBag.MENU_ADMINFinance = Admin;
            ViewBag.MENU_ADMINSYSTEM = Admin;
            ViewBag.PhStaff= (Admin || bSurrogateMode);
            ViewBag.SystemMenu = false;
            ViewBag.FinanceMenu = false;
            ViewBag.OperationsMenu = false;
            ViewBag.ConfigSection = false;
            ViewBag.SupportSection = false;
            ViewBag.LibrariesSection = false;
            ViewBag.AccountsSection = false;
            ViewBag.DiscountsSection = false;
            ViewBag.PartnersSection = false;
            ViewBag.AccountsOpSection = false;
            ViewBag.ProgramSection = false;
            ViewBag.OtherSection = false;

            //Administration Menu & Staff Member Is Looged ID, Appply Privilages
            bAnyVisibleInSec = false;
            if (!bAnyVisibleInSec)
            {
                strSection = "Configuration";
                ViewBag.PAGE_SecurityGroups  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Security groups", clsPrivileges.PAGE_SecurityGroups);
                ViewBag.PAGE_Configuration  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Configuration", clsPrivileges.PAGE_Configuration);
                ViewBag.PAGE_Exams  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Exams", clsPrivileges.PAGE_Exams);
                ViewBag.PAGE_Equipment  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Equipment", clsPrivileges.PAGE_Equipment);
                ViewBag.PAGE_TestimonialsList  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Testimonials", clsPrivileges.PAGE_TestimonialsList);
                ViewBag.PAGE_BadgeDesign  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Badge designs", clsPrivileges.PAGE_BadgeDesign);
                ViewBag.PAGE_BadgeRules  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Badge rules", clsPrivileges.PAGE_BadgeRules);
                ViewBag.PAGE_AttMetrics  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Feedback metrics", clsPrivileges.PAGE_AttMetrics);
                ViewBag.Page_MobileSettings  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Mobile admin", clsPrivileges.Page_MobileSettings);
                ViewBag.Page_ReminderManagement  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Reminder Management", clsPrivileges.Page_ReminderManagement);
                ViewBag.PAGE_Sentinels  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Sentinels", clsPrivileges.PAGE_Sentinels);
                ViewBag.Page_ReviewTooltipText  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Tooltips & Messages", clsPrivileges.Page_ReviewTooltipText);

                if (ViewBag.PAGE_SecurityGroups || ViewBag.PAGE_Configuration || ViewBag.PAGE_Exams || ViewBag.PAGE_Equipment ||
                    ViewBag.PAGE_TestimonialsList ||  ViewBag.PAGE_BadgeDesign || ViewBag.PAGE_BadgeRules  || ViewBag.PAGE_AttMetrics  ||
                    ViewBag.Page_MobileSettings ||  ViewBag.Page_ReminderManagement || ViewBag.PAGE_Sentinels  || ViewBag.Page_ReviewTooltipText)
                    bAnyVisibleInSec = true;
            }
            ViewBag.ConfigSection = bAnyVisibleInSec;

            bAnyVisibleInSec = false;
            if (!bAnyVisibleInSec)
            {
                strSection = "Support";
                ViewBag.PAGE_Issues  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Issue tracker", clsPrivileges.PAGE_Issues);
                ViewBag.PAGE_SendGridList  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Emails sent log", clsPrivileges.PAGE_SendGridList);
                ViewBag.Page_ChatLog  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Chat log", clsPrivileges.Page_ChatLog);
                ViewBag.Page_SMSLog  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "SMS log", clsPrivileges.Page_SMSLog);
                ViewBag.PAGE_SendGridList  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Jobs", clsPrivileges.PAGE_SendGridList);
                ViewBag.PAGE_ErrorLog  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Error log", clsPrivileges.PAGE_ErrorLog);
                ViewBag.PAGE_ErrorCodes  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Error codes", clsPrivileges.PAGE_ErrorCodes);
                ViewBag.PAGE_WebSiteMonitoring  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Website activity", clsPrivileges.PAGE_WebSiteMonitoring);
                ViewBag.PAGE_FAQs  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "FAQs", clsPrivileges.PAGE_FAQs);
                ViewBag.PAGE_NotificationSetting  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Notifications", clsPrivileges.PAGE_NotificationSetting);
                ViewBag.PAGE_Audit  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Audit trail", clsPrivileges.PAGE_Audit);

                if (ViewBag.PAGE_Issues || ViewBag.PAGE_SendGridList || ViewBag.Page_ChatLog || ViewBag.Page_SMSLog ||
                    ViewBag.PAGE_SendGridList ||  ViewBag.PAGE_ErrorLog || ViewBag.PAGE_ErrorCodes  || ViewBag.PAGE_WebSiteMonitoring  ||
                    ViewBag.PAGE_FAQs ||  ViewBag.PAGE_NotificationSetting || ViewBag.PAGE_Audit)
                    bAnyVisibleInSec = true;
            }
            ViewBag.SupportSection =bAnyVisibleInSec;

            bAnyVisibleInSec = false;
            if (!bAnyVisibleInSec)
            {
                strSection = "Libraries";
                ViewBag.PAGE_SessionList  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Activities", clsPrivileges.PAGE_SessionList);
                ViewBag.Page_TopicLibrary  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Topics", clsPrivileges.Page_TopicLibrary);
                ViewBag.PAGE_Venues  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Venues", clsPrivileges.PAGE_Venues);
                ViewBag.PAGE_Certifications  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Certificates", clsPrivileges.PAGE_Certifications);
                ViewBag.PAGE_GeoTags  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Geo tags", clsPrivileges.PAGE_GeoTags);
                ViewBag.PAGE_ExhFeedback  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Questionnaires", clsPrivileges.PAGE_ExhFeedback);
                ViewBag.PAGE_AppHelp  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Help Q&As", clsPrivileges.PAGE_AppHelp);
                ViewBag.PAGE_UrlRedirect  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Redirects", clsPrivileges.PAGE_UrlRedirect);
                ViewBag.PAGE_SendGridList  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Snippets", clsPrivileges.PAGE_SendGridList);
                ViewBag.PAGE_Announcements  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Announcements", clsPrivileges.PAGE_Announcements);
                ViewBag.Page_ArticleLibrary  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Standard files", clsPrivileges.Page_ArticleLibrary);
                ViewBag.PAGE_UploadImages  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Images", clsPrivileges.PAGE_UploadImages);
                ViewBag.Page_GamesAndRides  = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Games and rides", clsPrivileges.Page_GamesAndRides);

                if (ViewBag.PAGE_SessionList || ViewBag.Page_TopicLibrary || ViewBag.PAGE_Venues || ViewBag.PAGE_Certifications || ViewBag.Page_GamesAndRides  ||
                    ViewBag.PAGE_GeoTags ||  ViewBag.PAGE_ExhFeedback || ViewBag.PAGE_AppHelp  || ViewBag.PAGE_UrlRedirect || ViewBag.PAGE_UploadImages  ||
                    ViewBag.PAGE_SendGridList ||  ViewBag.PAGE_Announcements || ViewBag.Page_ArticleLibrary)
                    bAnyVisibleInSec = true;
            }
            ViewBag.LibrariesSection = bAnyVisibleInSec;

            if (ViewBag.ConfigSection || ViewBag.SupportSection || ViewBag.LibrariesSection)
                ViewBag.SystemMenu = true;

            //Finance Menu
            bAnyVisibleInSec = false;
            if (!bAnyVisibleInSec)
            {
                strSection = "Accounts";
                ViewBag.PAGE_Ledger = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Payments received", clsPrivileges.PAGE_Ledger);
                ViewBag.Page_PendingPayments = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Payments due", clsPrivileges.Page_PendingPayments);
                ViewBag.PAGE_LedgerByAcct = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Account ledger", clsPrivileges.PAGE_LedgerByAcct);
                ViewBag.Page_MiscellaneousPayments = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Payments", clsPrivileges.Page_MiscellaneousPayments);
                ViewBag.Page_DeferredRevenue = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Deferred revenue", clsPrivileges.Page_DeferredRevenue);
                ViewBag.PAGE_PastDue = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Attendee balances", clsPrivileges.PAGE_PastDue);
                ViewBag.PAGE_AcctReceipt = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Invoices & receipts", clsPrivileges.PAGE_AcctReceipt);
                ViewBag.PAGE_W9Requests = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "W9 requests", clsPrivileges.PAGE_W9Requests);

                if (ViewBag.PAGE_Ledger || ViewBag.Page_PendingPayments ||  ViewBag.PAGE_LedgerByAcct || ViewBag.Page_MiscellaneousPayments
                    || ViewBag.Page_DeferredRevenue ||  ViewBag.PAGE_PastDue || ViewBag.PAGE_AcctReceipt || ViewBag.PAGE_W9Requests)
                    bAnyVisibleInSec = true;
            }
            ViewBag.AccountsSection =bAnyVisibleInSec;

            bAnyVisibleInSec = false;
            if (!bAnyVisibleInSec)
            {
                strSection = "Discounts & Vouchers";
                ViewBag.PAGE_AcctDiscounts = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Discount code usage", clsPrivileges.PAGE_AcctDiscounts);
                ViewBag.PAGE_Discounts = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Discount codes", clsPrivileges.PAGE_Discounts);
                ViewBag.PAGE_VoucherList = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Vouchers", clsPrivileges.PAGE_VoucherList);
                ViewBag.Page_RegDiscountAuditor = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Registration discounts", clsPrivileges.Page_RegDiscountAuditor);
                ViewBag.Page_Passes = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Attendee Passes", clsPrivileges.Page_Passes);
                ViewBag.Page_Points = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Points", clsPrivileges.Page_Points);
                if (ViewBag.PAGE_AcctDiscounts || ViewBag.PAGE_Discounts ||  ViewBag.PAGE_VoucherList || ViewBag.Page_RegDiscountAuditor
                    || ViewBag.Page_Passes ||  ViewBag.Page_Points)
                    bAnyVisibleInSec = true;
            }
            ViewBag.DiscountsSection =bAnyVisibleInSec;

            bAnyVisibleInSec = false;
            if (!bAnyVisibleInSec)
            {
                strSection = "Partners";
                ViewBag.page_OrganizationLedger = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Partner payments", clsPrivileges.page_OrganizationLedger);
                ViewBag.Page_GeneralLedgerByOrg = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Partner ledger", clsPrivileges.Page_GeneralLedgerByOrg);
                ViewBag.Page_EventOrgBalanceDue = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Partner balances", clsPrivileges.Page_EventOrgBalanceDue);
                ViewBag.PAGE_PartnerPaymentSchedule = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Partner payment schedules", clsPrivileges.PAGE_PartnerPaymentSchedule);
                ViewBag.PAGE_PartnerPaymentSchedule = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Partner history", clsPrivileges.PAGE_PartnerPaymentSchedule);

                if (ViewBag.page_OrganizationLedger || ViewBag.Page_GeneralLedgerByOrg ||  ViewBag.Page_EventOrgBalanceDue
                        || ViewBag.PAGE_PartnerPaymentSchedule ||  ViewBag.PAGE_PartnerPaymentSchedule)
                    bAnyVisibleInSec = true;
            }
            ViewBag.PartnersSection = bAnyVisibleInSec;

            if (ViewBag.AccountsSection || ViewBag.DiscountsSection || ViewBag.PartnersSection)
                ViewBag.FinanceMenu = true;

            //Operations Menu
            bAnyVisibleInSec = false;
            if (!bAnyVisibleInSec)
            {
                strSection = "Accounts";
                ViewBag.PAGE_AccountList =  isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Accounts", clsPrivileges.PAGE_AccountList);
                ViewBag.Page_UserPhones =  isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "User phones", clsPrivileges.Page_UserPhones);
                ViewBag.PAGE_OrganizationList =  isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Organizations", clsPrivileges.PAGE_OrganizationList);
                ViewBag.PAGE_AccountGroups =  isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Account groups", clsPrivileges.PAGE_AccountGroups);
                ViewBag.PAGE_Invitations =  isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Referrals", clsPrivileges.PAGE_Invitations);
                ViewBag.PAGE_EventCommunications =  isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Communications", clsPrivileges.PAGE_EventCommunications);
                ViewBag.Page_PartialRegistration =  isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Partial registrations", clsPrivileges.Page_PartialRegistration);
                ViewBag.PAGE_EventAttendees =  isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Participants", clsPrivileges.PAGE_EventAttendees);
                ViewBag.PAGE_Data_Quality =  isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Data quality", clsPrivileges.PAGE_Data_Quality);
                ViewBag.Page_InterestedWritingArticle =  isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Authors", clsPrivileges.Page_InterestedWritingArticle);
                ViewBag.Page_ReviewHeadshots =  isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Headshots & Bios", clsPrivileges.Page_ReviewHeadshots);
                ViewBag.Page_ReviewAccountPhoto =  isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Review account photos", clsPrivileges.Page_ReviewAccountPhoto);
                ViewBag.Page_HeadshotSubstitueImages =  isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Headshot substitute images", clsPrivileges.Page_HeadshotSubstitueImages);

                if (ViewBag.PAGE_AccountList || ViewBag.Page_UserPhones ||  ViewBag.PAGE_OrganizationList || ViewBag.PAGE_EventCommunications ||  ViewBag.Page_PartialRegistration
                    || ViewBag.PAGE_EventAttendees ||  ViewBag.PAGE_Data_Quality || ViewBag.Page_InterestedWritingArticle ||  ViewBag.Page_ReviewHeadshots
                    || ViewBag.Page_ReviewAccountPhoto ||  ViewBag.Page_HeadshotSubstitueImages  || ViewBag.PAGE_AccountGroups ||  ViewBag.PAGE_Invitations)
                    bAnyVisibleInSec = true;
            }
            ViewBag.AccountsOpSection = bAnyVisibleInSec;

            bAnyVisibleInSec = false;
            if (!bAnyVisibleInSec)
            {
                strSection = "Program and Faculty";
                ViewBag.PAGE_EventList = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Event specification", clsPrivileges.PAGE_EventList);
                ViewBag.PAGE_EventList = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Events", clsPrivileges.PAGE_EventList);
                ViewBag.PAGE_EventProgram = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Program", clsPrivileges.PAGE_EventProgram);
                ViewBag.Page_EventSeries = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Event series", clsPrivileges.Page_EventSeries);
                ViewBag.PAGE_SpeakerList = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Faculty", clsPrivileges.PAGE_SpeakerList);
                ViewBag.Page_EventStaff = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Event staff", clsPrivileges.Page_EventStaff);
                ViewBag.Page_EventURLs = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Event URLs", clsPrivileges.Page_EventURLs);
                ViewBag.PAGE_SpeakerMgt = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Speaker management", clsPrivileges.PAGE_SpeakerMgt);
                ViewBag.PAGE_SessionChairs = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Session chairs", clsPrivileges.PAGE_SessionChairs);
                ViewBag.PAGE_OtherLinks = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Quick links", clsPrivileges.PAGE_OtherLinks);
                ViewBag.Page_UserReports = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Allow reports", clsPrivileges.Page_UserReports);
                ViewBag.Page_ActivityQuestions = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Polling questions", clsPrivileges.Page_ActivityQuestions);
                ViewBag.Page_RTDiscussion = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Public Events", clsPrivileges.Page_RTDiscussion);
                ViewBag.Page_StaffSupport = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Help desk", clsPrivileges.Page_StaffSupport);
                ViewBag.Page_EventToteBag = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Tote bag", clsPrivileges.Page_EventToteBag);
                ViewBag.PRIV_ManageTeams = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Manage interest groups", clsPrivileges.PRIV_ManageTeams);

                bool AdvancedSearch = false;
                if (isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Advanced searches", clsPrivileges.PAGE_EventAttendees))
                    AdvancedSearch = true;
                if (isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Advanced searches", clsPrivileges.PAGE_AccountList))
                    AdvancedSearch = true;
                if (isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Advanced searches", clsPrivileges.PAGE_EventCommunications))
                    AdvancedSearch = true;
                if (isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Advanced searches", clsPrivileges.PAGE_SpeakerList))
                    AdvancedSearch = true;

                ViewBag.AdvancedSearch = AdvancedSearch;
                if (ViewBag.PAGE_EventList || ViewBag.AdvancedSearch ||  ViewBag.PAGE_EventProgram || ViewBag.Page_EventSeries
                   || ViewBag.PAGE_SpeakerList ||  ViewBag.Page_EventStaff || ViewBag.Page_EventURLs ||  ViewBag.PAGE_SpeakerMgt
                   || ViewBag.PAGE_SessionChairs ||  ViewBag.PAGE_OtherLinks  || ViewBag.Page_UserReports ||  ViewBag.Page_ActivityQuestions
                   || ViewBag.Page_RTDiscussion ||  ViewBag.Page_StaffSupport  || ViewBag.Page_EventToteBag ||  ViewBag.PRIV_ManageTeams)
                    bAnyVisibleInSec = true;
            }
            ViewBag.ProgramSection  = bAnyVisibleInSec;

            bAnyVisibleInSec = false;
            if (!bAnyVisibleInSec)
            {
                strSection = "Other";
                ViewBag.PAGE_Badges = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Badges", clsPrivileges.PAGE_Badges);
                ViewBag.PAGE_Partners = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Partner management", clsPrivileges.PAGE_Partners);
                ViewBag.PAGE_TreasureHunt = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Treasure hunt", clsPrivileges.PAGE_TreasureHunt);
                ViewBag.PAGE_TaskList = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Tasks", clsPrivileges.PAGE_TaskList);
                ViewBag.PAGE_Feedback = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Attendance & feedback", clsPrivileges.PAGE_Feedback);
                ViewBag.PAGE_Certifier = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Certifications", clsPrivileges.PAGE_Certifier);
                ViewBag.PAGE_CRCP = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "CRCP certifications", clsPrivileges.PAGE_CRCP);
                ViewBag.PAGE_AV = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Audiovisual", clsPrivileges.PAGE_AV);
                ViewBag.PAGE_Reports = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Reports", clsPrivileges.PAGE_Reports);
                ViewBag.Page_Food_and_Beverage = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Food & beverage", clsPrivileges.Page_Food_and_Beverage);
                ViewBag.Page_StaffFiles = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Staff files", clsPrivileges.Page_StaffFiles);
                ViewBag.Page_Recording = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Online activities", clsPrivileges.Page_Recording);
                ViewBag.Page_CommunityShowcase = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Community showcase", clsPrivileges.Page_CommunityShowcase);
                ViewBag.Page_TrainingResources = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Training resources", clsPrivileges.Page_TrainingResources);
                ViewBag.Page_SessionRecordings = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Session Recordings", clsPrivileges.Page_SessionRecordings);
                ViewBag.Page_EventPlan = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Event plan", clsPrivileges.Page_EventPlan);
                ViewBag.Page_ChatModerator = isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Chat moderator", clsPrivileges.Page_ChatModerator);

                if (ViewBag.PAGE_Badges || ViewBag.PAGE_Partners ||  ViewBag.PAGE_TreasureHunt || ViewBag.PAGE_TaskList
                   || ViewBag.PAGE_Feedback ||  ViewBag.PAGE_Certifier || ViewBag.PAGE_CRCP ||  ViewBag.PAGE_AV
                   ||  ViewBag.PAGE_Reports  || ViewBag.Page_Food_and_Beverage ||  ViewBag.Page_CommunityShowcase || ViewBag.Page_TrainingResources
                   || ViewBag.Page_EventPlan)
                    bAnyVisibleInSec = true;

            }
            ViewBag.OtherSection = bAnyVisibleInSec;

            if (ViewBag.AccountsOpSection || ViewBag.ProgramSection || ViewBag.OtherSection)
                ViewBag.OperationsMenu = true;

            bAnyVisibleInSec = false;
            ViewBag.ResourceSM_Menu = false;
            ViewBag.Page_Journal = false;
            ViewBag.PAGE_Standards = false;
            ViewBag.Page_FCRDirectories = false;
            ViewBag.Page_RegDoc = false;
            ViewBag.Page_FdaGcp = false;
            ViewBag.Page_Glossary = false;
            ViewBag.Page_IcfGlossary = false;
            ViewBag.Page_Milestones = false;
            ViewBag.Page_MagiNews = false;
            ViewBag.MyEventOptions =false;
            if (!bAnyVisibleInSec && LoggedIn)
            {
                strSection = "Administration";
                ViewBag.Page_Journal        = (Admin || isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Journal of Clinical Research Best Practices", clsPrivileges.Page_Journal));
                ViewBag.PAGE_Standards      = (Admin ||isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "MAGI best practice standards", clsPrivileges.PAGE_Standards));
                ViewBag.Page_FCRDirectories = (Admin || isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Directories", clsPrivileges.Page_FCRDirectories));
                ViewBag.Page_RegDoc         = (Admin || isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Laws, regulations & guidelines", clsPrivileges.Page_RegDoc));
                ViewBag.Page_FdaGcp         = (Admin ||isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "FDA good clinical practice Q&A", clsPrivileges.Page_FdaGcp));
                ViewBag.Page_Glossary       = (Admin ||isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Glossary for clinical research", clsPrivileges.Page_Glossary));
                ViewBag.Page_IcfGlossary    = (Admin || isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Glossary for informed consent", clsPrivileges.Page_IcfGlossary));
                ViewBag.Page_Milestones     = (Admin ||isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "Clinical research milestones", clsPrivileges.Page_Milestones));
                ViewBag.Page_MagiNews       = (Admin || isAllowedByPriv(blueRibbonMode, Admin, dt, strSection, "MAGI news", clsPrivileges.Page_MagiNews));

                if (ViewBag.Page_Journal || ViewBag.PAGE_Standards || ViewBag.Page_FCRDirectories || ViewBag.Page_Milestones || ViewBag.Page_MagiNews
                 || ViewBag.Page_RegDoc || ViewBag.Page_FdaGcp || ViewBag.Page_Glossary || ViewBag.Page_IcfGlossary)
                    bAnyVisibleInSec = true;
            }
            ViewBag.ResourceSM_Menu =bAnyVisibleInSec;

            string directoryPath = Server.MapPath(string.Format("~/Documents/MAGI_First_Clinical_Research_Stock_Index.pdf"));
            ViewBag.Resources_FCRStockIdx = (System.IO.File.Exists(directoryPath));

            string FeatureSettings = "EventType_PKey,EventStatus_PKey,MenuSelection,EventID,PartnerAlias,IsNextSession";
            if (LoggedIn)
                FeatureSettings = " EventType_PKey,EventStatus_PKey,MenuSelection,PartnerAlias,EventID,IsNextSession,ISNULL(IsDemo,0) as IsDemo,ISNULL(IsNetworkingMsgPnlOn,0) as IsNetworkingMsgPnlOninfo,IsEventOpenForStaff,IsEventOpenForEventSponsors,IsEventOpenForSpeaker,ShowVirtaulBigButton,IsEventClosedForAttendees,ShowEventPages,isnull(FeatureAccess,'') as FeatureAccess,isnull(AttendeeAccess,'') as AttendeeAccess,ShowRemindersPanel,BookEndDate,BookStartDate";

            FeatureSettings += ",EventFullname,Venue_pkey,BannerMessage,intBackcolor,StandardTime_Pkey,PublicPageStartDate,PublicPageEndDate,LeftBlockImage,MiddleBlockImage,RightBlcokImage,StartDate,EndDate";
            DataTable EventInfo = repository.getDyamicEventSettings(data.EventId, FeatureSettings);
            bool bEventOpenStaff = false, bEventOpenEventSponsors = false, bEventOpenSpeakers = false, bShowEvtPages = false, bShowVirtaulBigButton = false,
                 bEventClosedAttendees = false, bShowRemindersPanel = false, bNetworkingMsgPnlOninfo = false, bEventDemo = false, bNextSession = false;
            int intEventType_PKey = 0, intEventStatus_PKey = 0;
            string strEventPages = "", FeatureAccess = "", AttendeeAccess = "", strEventID = "", strPartnerAlias = "", BannerImage = "", SmallBannerImage = "", strLocationCity = "", sVenueID = "";
            int VenuepKey = 0, intBackcolor = 0, StandardTime_pKey = 0; string BannerMessage = "", sLeftBlockImage = "", sMiddleBlockImage = "", sRightBlcokImage = "", lblcnfName = "";
            string imgLeftSrc = "", imgMiddleSrc = "", imgRightSrc = "", lblcnfLocation = "", lblcnfDate = "";
            bool imgRightVisible = true, EventInfoVisible = false;
            DateTime dtPublicPageStartDate = System.DateTime.Now, dtPublicPageEndDate = System.DateTime.Now, dtStartDate = System.DateTime.Now, dtEndDate = System.DateTime.Now;
            DateTime dtBookStartDate = new DateTime(), dtBookEndDate = new DateTime();
            if (EventInfo != null && EventInfo.Rows.Count > 0)
            {
                if (LoggedIn)
                {
                    bShowEvtPages = (EventInfo.Rows[0]["ShowEventPages"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["ShowEventPages"].ToString());
                    bEventOpenStaff = (EventInfo.Rows[0]["IsEventOpenForStaff"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["ShowEventPages"].ToString());
                    bEventOpenEventSponsors = (EventInfo.Rows[0]["IsEventOpenForEventSponsors"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["ShowEventPages"].ToString());
                    bEventOpenSpeakers = (EventInfo.Rows[0]["IsEventOpenForSpeaker"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["ShowEventPages"].ToString());
                    bEventClosedAttendees = (EventInfo.Rows[0]["IsEventClosedForAttendees"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["ShowEventPages"].ToString());
                    bShowRemindersPanel = (EventInfo.Rows[0]["ShowRemindersPanel"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["ShowRemindersPanel"].ToString());

                    FeatureAccess =  (EventInfo.Rows[0]["FeatureAccess"] == DBNull.Value) ? "" : EventInfo.Rows[0]["FeatureAccess"].ToString();
                    AttendeeAccess =  (EventInfo.Rows[0]["AttendeeAccess"] == DBNull.Value) ? "" : EventInfo.Rows[0]["AttendeeAccess"].ToString();
                    strEventPages =  (EventInfo.Rows[0]["MenuSelection"] == DBNull.Value) ? "" : EventInfo.Rows[0]["MenuSelection"].ToString();
                    bShowVirtaulBigButton = (EventInfo.Rows[0]["ShowVirtaulBigButton"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["ShowVirtaulBigButton"].ToString());
                    dtBookStartDate = (EventInfo.Rows[0]["BookStartDate"] == DBNull.Value) ? new DateTime() : Convert.ToDateTime(EventInfo.Rows[0]["BookStartDate"].ToString());
                    dtBookEndDate= (EventInfo.Rows[0]["BookEndDate"] == DBNull.Value) ? new DateTime() : Convert.ToDateTime(EventInfo.Rows[0]["BookEndDate"].ToString());
                    bNetworkingMsgPnlOninfo = (EventInfo.Rows[0]["IsNetworkingMsgPnlOninfo"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["IsNetworkingMsgPnlOninfo"].ToString());
                    bEventDemo = (EventInfo.Rows[0]["IsDemo"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["IsDemo"].ToString());
                }

                if(bNetworkingMsgPnlOninfo)
                {
                    clsEvent Event = new clsEvent();
                    Event.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                    bNetworkingMsgPnlOninfo = Event.CheckValiditityOfModule(data.EventId, "IsNetworkingMsgPnlOn");
                }

                bNextSession = (EventInfo.Rows[0]["IsNextSession"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["IsNextSession"].ToString());
                intEventType_PKey =  (EventInfo.Rows[0]["EventType_PKey"] == DBNull.Value) ? 0 : Convert.ToInt32(EventInfo.Rows[0]["EventType_PKey"].ToString());
                intEventStatus_PKey =  (EventInfo.Rows[0]["EventStatus_PKey"] == DBNull.Value) ? 0 : Convert.ToInt32(EventInfo.Rows[0]["EventStatus_PKey"].ToString());
                strEventPages =  (EventInfo.Rows[0]["MenuSelection"] == DBNull.Value) ? "" : EventInfo.Rows[0]["MenuSelection"].ToString();
                strEventID =  (EventInfo.Rows[0]["EventID"] == DBNull.Value) ? "" : EventInfo.Rows[0]["EventID"].ToString();
                strPartnerAlias =  (EventInfo.Rows[0]["PartnerAlias"] == DBNull.Value) ? "" : EventInfo.Rows[0]["PartnerAlias"].ToString();
                data.EventName = (EventInfo.Rows[0]["EventFullname"] == DBNull.Value) ? "" : EventInfo.Rows[0]["EventFullname"].ToString();
                BannerMessage = (EventInfo.Rows[0]["BannerMessage"] == DBNull.Value) ? "" : EventInfo.Rows[0]["BannerMessage"].ToString();
                sLeftBlockImage = (EventInfo.Rows[0]["LeftBlockImage"] == DBNull.Value) ? "" : EventInfo.Rows[0]["LeftBlockImage"].ToString();
                sMiddleBlockImage = (EventInfo.Rows[0]["MiddleBlockImage"] == DBNull.Value) ? "" : EventInfo.Rows[0]["MiddleBlockImage"].ToString();
                sRightBlcokImage = (EventInfo.Rows[0]["RightBlcokImage"] == DBNull.Value) ? "" : EventInfo.Rows[0]["RightBlcokImage"].ToString();
                VenuepKey =(EventInfo.Rows[0]["Venue_pkey"] == DBNull.Value) ? 0 : Convert.ToInt32(EventInfo.Rows[0]["Venue_pkey"]);
                intBackcolor =(EventInfo.Rows[0]["intBackcolor"] == DBNull.Value) ? 0 : Convert.ToInt32(EventInfo.Rows[0]["intBackcolor"]);
                StandardTime_pKey=(EventInfo.Rows[0]["StandardTime_Pkey"] == DBNull.Value) ? 0 : Convert.ToInt32(EventInfo.Rows[0]["StandardTime_Pkey"]);
                dtPublicPageStartDate =(EventInfo.Rows[0]["PublicPageStartDate"] == DBNull.Value) ? System.DateTime.Now : Convert.ToDateTime(EventInfo.Rows[0]["PublicPageStartDate"]);
                dtPublicPageEndDate =(EventInfo.Rows[0]["PublicPageEndDate"] == DBNull.Value) ? System.DateTime.Now : Convert.ToDateTime(EventInfo.Rows[0]["PublicPageEndDate"]);
                dtStartDate =(EventInfo.Rows[0]["StartDate"] == DBNull.Value) ? System.DateTime.Now : Convert.ToDateTime(EventInfo.Rows[0]["StartDate"]);
                dtEndDate =(EventInfo.Rows[0]["EndDate"] == DBNull.Value) ? System.DateTime.Now : Convert.ToDateTime(EventInfo.Rows[0]["EndDate"]);
            }
            string standardRegion = new SqlOperation().getStandardRegion(StandardTime_pKey);
            string BannerLinkText = data.EventName;
            string BannkerLinkNavigateUrl = "/Events/EventInfo?EVT=" + data.EventId.ToString();
            string BackColorRGB = System.Drawing.Color.FromArgb(intBackcolor).R + "," + System.Drawing.Color.FromArgb(intBackcolor).G + "," +System.Drawing.Color.FromArgb(intBackcolor).B;
            DataTable VenueImage = new SqlOperation().getVenueBannerImage(VenuepKey);


            if (VenueImage != null && VenueImage.Rows.Count > 0)
            {
                BannerImage = (VenueImage.Rows[0]["VenueBanner"] == DBNull.Value) ? "" : VenueImage.Rows[0]["VenueBanner"].ToString();
                strLocationCity = (VenueImage.Rows[0]["LocationCity"] == DBNull.Value) ? "" : VenueImage.Rows[0]["LocationCity"].ToString();
                SmallBannerImage= (VenueImage.Rows[0]["VenueNarrowBanner"] == DBNull.Value) ? "" : VenueImage.Rows[0]["VenueNarrowBanner"].ToString();
                sVenueID = (VenueImage.Rows[0]["VenueID"] == DBNull.Value) ? "" : VenueImage.Rows[0]["VenueID"].ToString();
            }
            BannerMessage = (string.IsNullOrEmpty(strLocationCity) ? "" : strLocationCity + ". ") +  BannerMessage;
            SmallBannerImage = "/venuedocuments/" + SmallBannerImage;
            bool Banner_imgVisible = (SmallBannerImage != "");
            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = data.EventId;
            cEvent.strStandardRegion = standardRegion;
            DateTime CurrentDate = clsEvent.getEventVenueTime();
            bool bShowImageLink = (CurrentDate >= dtPublicPageStartDate && CurrentDate <= dtPublicPageEndDate.AddDays(1).Date);
            bool bShowImage = true;
            if (System.Configuration.ConfigurationManager.AppSettings["QAMode"] == "1")
            {
                string strMiddle = Server.MapPath("~/images/Homepage/" + data.EventId.ToString() + "MiddleBlock.jpg");
                bShowImage = (System.IO.File.Exists(strMiddle));
            }
            Dictionary<int, clsImg> dct = ((Dictionary<int, clsImg>)System.Web.HttpContext.Current.Application["cImages"]);
            string imgLogo = "/Images/HomePage/magilogo.jpg",
                   MagiMenuImage = "/images/menu/confmenu1light.png",
                   EventMenuImage = "/images/menu/confmenu2light.png",
                   ResourcesMenuImage = "/images/menu/Resources.jpg",
                   MyMagiMenuImage = "/images/menu/confmenu5light.png";
            if (dct.ContainsKey(clsImages.IMG_1))
                imgLogo = dct[clsImages.IMG_1].strPath.Replace("~", "");

            if (dct.ContainsKey(clsImages.IMG_13))
                MagiMenuImage = dct[clsImages.IMG_13].strPath.Replace("~", "");

            if (dct.ContainsKey(clsImages.IMG_14))
                EventMenuImage = dct[clsImages.IMG_14].strPath.Replace("~", "");

            if (dct.ContainsKey(clsImages.IMG_15))
                ResourcesMenuImage = dct[clsImages.IMG_15].strPath.Replace("~", "");

            if (dct.ContainsKey(clsImages.IMG_16))
                MyMagiMenuImage = dct[clsImages.IMG_16].strPath.Replace("~", "");

            if (sLeftBlockImage != "" && sMiddleBlockImage != "")
            {
                imgLeftSrc = "/images/BannerImages/" + sLeftBlockImage;
                imgMiddleSrc = "/images/BannerImages/" + sMiddleBlockImage;
                if (sRightBlcokImage != "")
                    imgRightSrc= "/images/BannerImages/" + sRightBlcokImage;
                else
                    imgRightVisible = false;
            }
            else if (bShowImage && SmallBannerImage != "")
            {
                imgLeftSrc = "/venuedocuments/" + SmallBannerImage;
                imgMiddleSrc = "/images/Homepage/" +  data.EventId.ToString() + "MiddleBlock.jpg";
                imgRightSrc = "/images/Homepage/" +data.EventId.ToString()  + "RightBlock.jpg";
            }
            else
            {
                EventInfoVisible= true;
                if (data.EventId >0)
                {
                    lblcnfName = data.EventName.Replace("MAGI's Clinical Research Conference - ", " ").ToUpper();
                    lblcnfLocation =  sVenueID;
                    lblcnfDate =dtStartDate.ToString("MMMM") + " " + dtStartDate.Day.ToString() + "-" +
                                ((dtEndDate.ToString("MMMM") != dtStartDate.ToString("MMMM")) ? (cEvent.dtEndDate.ToString("MMMM") + " ") : "") +
                                dtEndDate.Day.ToString() + ", " + cEvent.dtEndDate.Year.ToString();
                }
                imgLeftSrc = "/images/Homepage/LeftBlock.gif";
                imgMiddleSrc = "/images/Homepage/MiddleBlock.jpg";
                if (dct.ContainsKey(clsImages.IMG_17))
                    imgMiddleSrc = dct[clsImages.IMG_17].strPath.Replace("~", "");
                imgRightSrc = "/images/Homepage/RightBlock.jpg";
                imgRightVisible = false;
                bShowImageLink =false;
            }

            ViewBag.lblCurRoles = getRolesForEvent(data.Id, data.EventId);
            ViewBag.cmdCurEvent = strEventID;
            ViewBag.cmdSwitchEvent = data.StaffMember;
            ViewBag.CurrEventURL= "/ViewEvent?PK=" + data.EventId.ToString();
            bool bEvent = (intEventStatus_PKey != clsEvent.STATUS_Completed);
            string[] strPages = strEventPages.Split(',');
            ViewBag.ContinuingEducationHr = (!(Array.IndexOf(strPages, "9") >= 0));
            ViewBag.CRCPcert = (!(Array.IndexOf(strPages, "1") >= 0));
            ViewBag.IsMAGIRight = (!(Array.IndexOf(strPages, "3") >= 0));
            ViewBag.AdvisoryBoard = (!(Array.IndexOf(strPages, "4") >= 0));
            ViewBag.Testimonials = (!(Array.IndexOf(strPages, "5") >= 0));
            ViewBag.UpcomingEvts = (!(Array.IndexOf(strPages, "6") >= 0));
            ViewBag.ContactMAGI = (!(Array.IndexOf(strPages, "7") >= 0));
            ViewBag.HavingQuestion = (!(Array.IndexOf(strPages, "8") >= 0));
            ViewBag.EvtTermsCond = (!(Array.IndexOf(strPages, "19") >= 0));
            ViewBag.MyInternetSpeed =  (Speaker || bAttendeeAtCurrEvent || Admin);

            ViewBag.Overview = (!(Array.IndexOf(strPages, "10") >= 0));
            ViewBag.ProgramAgenda = (!(Array.IndexOf(strPages, "11") >= 0));
            ViewBag.EventSpeakers = (!(Array.IndexOf(strPages, "12") >= 0));
            ViewBag.BecomeAspeaker = (!(Array.IndexOf(strPages, "13") >= 0));
            ViewBag.Eventpartners = (!(Array.IndexOf(strPages, "14") >= 0));
            ViewBag.Becomeapartner = (!(Array.IndexOf(strPages, "15") >= 0));
            if (intEventType_PKey == clsEvent.EventType_CloudConference || intEventType_PKey == clsEvent.EventType_HybridConference)
                ViewBag.Venuelodging = false;
            ViewBag.Venuelodging = (!(Array.IndexOf(strPages, "16") >= 0));
            ViewBag.ParticipatingOrg = (!(Array.IndexOf(strPages, "2") >= 0));
            ViewBag.PricingRegistration = (!(Array.IndexOf(strPages, "17") >= 0));
            ViewBag.SignIntoEvent = (!(Array.IndexOf(strPages, "18") >= 0) && !LoggedIn);
            ViewBag.IconNetwork = "";
            
            if (LoggedIn)
            {
                strSection="My Event";
                DateTime dCalTime = clsEvent.getCaliforniaTime();
                bool Enable = bShow;
                EventFeatures features = LoadFeatures(FeatureAccess, AttendeeAccess);
                cEvent.sqlConn=new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                cEvent.intEvent_PKey  = data.EventId;
                cEvent.GetBasicEventInfo(data.EventId);
                bAnyVisibleInMenu =false;
                DateTime CurrentTime = clsEvent.getEventVenueTime();
                if (strSection == "My Event")
                {
                    string intRegistrationLevel_pKey = "";
                    int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);

                    ViewBag.MyEventSummary = (bEvent && (intAttendeeStatus == 1 || intAttendeeStatus == 3) &&
                    (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_FullReg.ToString() ||
                     intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_FullRegVirtual.ToString() ||
                     intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_StudentOnly.ToString() ||
                     intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDay.ToString() ||
                     intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayPhysical.ToString()||
                     intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayVirtual.ToString() ||
                     intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_SingleTrackOnly.ToString() ||
                     intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_ExhibitOnly.ToString()) || data.GlobalAdmin || (intAttendeeStatus == 1 && data.StaffMember));

                    ViewBag.MyEventOptions = (!(Array.IndexOf(strPages, "19") >= 0));
                    if (ViewBag.MyEventOptions)
                        ViewBag.MyEventOptions = ((intAttendeeStatus == 1 || (features.intAttOptions == 0 && intAttendeeStatus == 3)) && features.bOptions &&
                           (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_FullReg.ToString() ||
                            intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_FullRegVirtual.ToString() ||
                            intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_StudentOnly.ToString() ||
                            intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDay.ToString() ||
                            intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayPhysical.ToString() ||
                            intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayVirtual.ToString() ||
                            intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_SingleTrackOnly.ToString() ||
                            intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_ExhibitOnly.ToString())
                            || data.GlobalAdmin || (intAttendeeStatus == 1 && data.StaffMember));

                    ViewBag.MyReminders = (!(Array.IndexOf(strPages, "20") >= 0));
                    if (ViewBag.MyReminders)
                        ViewBag.MyReminders =(bShowRemindersPanel && (data.GlobalAdmin || (intAttendeeStatus ==  1 &&  data.StaffMember)));

                    ViewBag.MySchedule = (!(Array.IndexOf(strPages, "23") >= 0));
                    if (ViewBag.MySchedule)
                        ViewBag.MySchedule = (features.bSch && (intAttendeeStatus == 1  || (features.intAttSch == 0 && intAttendeeStatus == 3)) &&
                                              (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_FullReg.ToString() ||
                                               intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_FullRegVirtual.ToString() ||
                                               intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_StudentOnly.ToString() ||
                                               intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDay.ToString() ||
                                               intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayPhysical.ToString() ||
                                               intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayVirtual.ToString() ||
                                               intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_SingleTrackOnly.ToString() ||
                                               intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_ExhibitOnly.ToString()) || data.GlobalAdmin  || (intAttendeeStatus == 1 && data.StaffMember));

                    ViewBag.MyBookMaterials = (!(Array.IndexOf(strPages, "22") >= 0));
                    if (ViewBag.MyBookMaterials)
                    {
                        ViewBag.MyBookMaterials =  ((intAttendeeStatus == 1) &&
                                                    (dCalTime >= dtBookStartDate &&  dCalTime < dtBookEndDate.AddDays(1).Date) &&
                                                    (intRegistrationLevel_pKey != clsEventAccount.REGISTRATION_ExhibitOnly.ToString()) &&
                                                    (intRegistrationLevel_pKey != clsEventAccount.REGISTRATION_SingleSessionOnly.ToString()) ||
                                                    data.GlobalAdmin  || (intAttendeeStatus == 1 && data.StaffMember));

                        if (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDay.ToString() || intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayPhysical.ToString() || intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayVirtual.ToString())
                            ViewBag.MyBookMaterials = (dtOneDayRegDate.ToString("MM/dd/yyyy") == DateTime.Now.ToString("MM/dd/yyyy"));
                    }
                    ViewBag.MYFAQs = (!(Array.IndexOf(strPages, "29") >= 0));
                    if (ViewBag.MYFAQs)
                        ViewBag.MYFAQs = ((intAttendeeStatus == 1) || bAttendeeAtCurrEvent || data.GlobalAdmin);

                    ViewBag.MYConsole =false;
                    bool bSponsor = MASTER_CheckExhibitor(intRefrenceORG_pkey, data.EventId, Account_pkey: data.Id);
                    if (!bIsPartner)
                        bIsPartner= bSponsor;
                    if (cEvent.bShowConsole && bSponsor && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "IsShowConsole"))
                        ViewBag.MYConsole = true;

                    ViewBag.MySessions = (bEvent && (intAttendeeStatus == 1 || intAttendeeStatus == 3) && (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_SingleSessionOnly.ToString()));
                    ViewBag.MyInterestGroup = ((data.GlobalAdmin || data.StaffMember  || (intAttendeeStatus == 1 && intRegistrationLevel_pKey != clsEventAccount.REGISTRATION_SingleSessionOnly.ToString())) && HaveAnyInterestGroup(data.Id, data.EventId));


                    ViewBag.EnterMyEventEnable = true;
                    ViewBag.EnterMyEvent =false;
                    if ((bEventOpenStaff && (bStaffForCurEvent || data.GlobalAdmin || data.StaffMember)) || (bEventOpenEventSponsors  && CheckExhibitor(data.ParentOrganization_pKey, data.EventId)))
                        ViewBag.EnterMyEvent = true;
                    else if (bEventClosedAttendees && bAttendeeAtCurrEvent && !bStaffForCurEvent)
                        ViewBag.EnterMyEvent =false;
                    else
                        ViewBag.EnterMyEvent = (bShowVirtaulBigButton && bShowEvtPages);

                    ViewBag.MyNetwork = false;
                    bool IsShow = true, bNet = features.bNet;
                    if (data.GlobalAdmin && (cEvent.dtEndDate<CurrentTime || cEvent.dtStartDate > CurrentTime))
                    {
                        if ((bEventDemo || bNetworkingMsgPnlOninfo) && MASTER_CheckExhibitor(data.ParentOrganization_pKey, data.EventId, Account_pkey: data.Id))
                        { }
                        else if ((bEventOpenSpeakers || bNetworkingMsgPnlOninfo) && NumTimesSpeakingCurEvent > 0)
                        { }
                        else if (bEventOpenStaff && (bStaffForCurEvent || data.GlobalAdmin || data.StaffMember))
                        { }
                        else
                            IsShow = (intAttendeeStatus == 1 ? true : false);
                    }
                    if (!data.GlobalAdmin)
                        bNet = (bNet || ((bEventDemo || bNetworkingMsgPnlOninfo) && MASTER_CheckExhibitor(data.ParentOrganization_pKey, data.EventId, Account_pkey: data.Id)));
                    else
                        bNet =true;
                    ViewBag.MyNetwork = IsShow && bNet && (((intAttendeeStatus == 1 || (features.intAttNet == 0 && intAttendeeStatus == 3)) &&
                                        (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_FullReg.ToString()
                                      || intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_FullRegVirtual.ToString()
                                      || intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_StudentOnly.ToString()
                                      || intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDay.ToString()
                                      || intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayPhysical.ToString()
                                      || intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayVirtual.ToString()
                                      || intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_SingleTrackOnly.ToString()
                                      || intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_ExhibitOnly.ToString()))
                                      || data.GlobalAdmin  || (intAttendeeStatus == 1 && data.StaffMember));

                    ViewBag.IconNetwork = ((bNewMessages && false) ? "~/images/icons/icon-new.png" : "");
                    ViewBag.Myspeakerdinner = (bParticipatingInCurrentEvent && bDinnerPlanned);


                    if (ViewBag.MYFAQs  || ViewBag.MyNetwork || ViewBag.Myspeakerdinner || ViewBag.EnterMyEvent || ViewBag.MyInterestGroup ||
                        ViewBag.MyBookMaterials || ViewBag.MySchedule || ViewBag.MyReminders || ViewBag.MyEventOptions || ViewBag.MyEventSummary ||
                        ViewBag.MySessions || ViewBag.MYConsole ||  ViewBag.EnterMyEvent)
                        bAnyVisibleInMenu=true;
                }

                if (ViewBag.MySchedule && bNextSession && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "IsNextSession"))
                    UpComingSession(data.Id, data.EventId, cSettings, CurrentTime);

                if (bAnyVisibleInMenu)
                    ViewBag.MyEventSection= true;

                ViewBag.HelpDesk= true;
                ViewBag.FacultyResourceCenter = (data.StaffMember || data.GlobalAdmin || (ddMyEvent != null && ddMyEvent.Rows.Count>0)) && (NumTimesSpeakingCurEvent > 0);
                bShow =bStaffForCurEvent  || data.GlobalAdmin;
                ViewBag.MyStaffConsole = bShow;
                ViewBag.MyPartnerChargesPayments= (clsEventOrganization.strOrganizationRole(data.ParentOrganization_pKey, data.EventId, data.Id, clsEventOrganization.PARTICIPATIONRole_PaymentMgr.ToString()) || data.GlobalAdmin);
                ViewBag.MyPartnerPaymentsDue=ViewBag.MyPartnerChargesPayments;
                ViewBag.MyPartnerAgreement=ViewBag.MyPartnerChargesPayments;
                ViewBag.MyPartnerOrganizationRole = (clsEventOrganization.strOrganizationRole(data.ParentOrganization_pKey, data.EventId, data.Id, clsEventOrganization.PARTICIPATIONRole_PartnerAdmin.ToString()));

                if (ViewBag.MyEventSection == null)
                    ViewBag.MyEventSection =false;
                if (ViewBag.MyStaffConsole == null)
                    ViewBag.MyStaffConsole =false;

                if (ViewBag.MyEventSection || ViewBag.MyStaffConsole)
                    ViewBag.MyEventSection= true;
            }
            ViewBag.MyBoothText = (data.EventId != 54) ? "My Console" : "My Booth";
            ViewBag.MyBoothURL = (data.EventId != 54) ? "/Console" : "/MyBooth";

            ViewBag.cmdSurrVisible = false;
            if (cSurr != null && cSurr.hasSurrogate)
            {
                ViewBag.cmdSurrText = "Return login to: " + cSurr.strOriginatorName;
                ViewBag.cmdSurrVisible = (cSurr.intOriginatorAcct_pKey != data.Id);
            }
            ViewBag.VisibleRepTrail = (LoggedIn || data.StaffMember);
            if (ViewBag.VisibleRepTrail)
            {
                clsFormList cFormlist = (clsFormList)Session["cFormlist"];
                ViewBag.RepTrailList=  cFormlist.MVC_getRepeaterList(5, strPartnerAlias);
            }
            ViewBag.ColorBG =  ((ConfigurationManager.AppSettings["QAMode"]== "1") ? "bgColorQA" : "");
            ViewBag.ColorLogoBG =  ((ConfigurationManager.AppSettings["QAMode"]== "1") ? "logoMagi" : "logoMagiImage");
            ViewBag.bannerMessage = BannerMessage;
            ViewBag.smallBannerImage = SmallBannerImage;
            ViewBag.backColorRGB = BackColorRGB;
            ViewBag.bannerLinkText = BannerLinkText;
            ViewBag.bannkerLinkNavigateUrl = BannkerLinkNavigateUrl;
            ViewBag.tblimageVisible = cSettings.bShowHeaderBanner;
            ViewBag.enableLink = bShowImageLink;
            ViewBag.imgRightVisible = imgRightVisible;
            ViewBag.imgLeftSrc = (string.IsNullOrEmpty(imgLeftSrc) ? "/images/Homepage/LeftBlock.gif" : imgLeftSrc);
            ViewBag.imgMiddleSrc = (string.IsNullOrEmpty(imgMiddleSrc) ? "/images/Homepage/MiddleBlock.jpg" : imgMiddleSrc);
            ViewBag.imgRightSrc = (string.IsNullOrEmpty(imgRightSrc) ? "/images/Homepage/MiddleBlock.jpg" : imgRightSrc);
            ViewBag.imgLogo =(string.IsNullOrEmpty(imgLogo) ? "/Images/HomePage2/magiLogo.png" : imgLogo);
            ViewBag.MagiMenuImage = (string.IsNullOrEmpty(MagiMenuImage) ? "/images/menu/confmenu1light.png" : MagiMenuImage);
            ViewBag.EventMenuImage =(string.IsNullOrEmpty(EventMenuImage) ? "/images/menu/confmenu2light.png" : EventMenuImage);
            ViewBag.ResourcesMenuImage = (string.IsNullOrEmpty(ResourcesMenuImage) ? "/Images/CommonImages/200515_153841confmenu1light.png" : ResourcesMenuImage);
            ViewBag.MyMagiMenuImage =(string.IsNullOrEmpty(MyMagiMenuImage) ? "/Images/CommonImages/200411_130851Resources.jpg" : MyMagiMenuImage);
            ViewBag.bShowImage = bShowImage;
            ViewBag.EventInfoVisible = EventInfoVisible;
            ViewBag.lblcnfName = lblcnfName;
            ViewBag.lblcnfLocation = lblcnfLocation;
            ViewBag.lblcnfDate = lblcnfDate;
            int AccountpKey = data.Id;
            int surrogateKey = cLast.intOriginatorAcct_pKey;
            if (surrogateKey != 0) { AccountpKey = surrogateKey; }
            ViewBag.LogoutAccount = AccountpKey;

        }
        public ActionResult Index()
        {
            ViewData["Title"] = "MAGI";
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            if (Request.QueryString["Redir"] != null)
            {
                cLast.bRedirPending = true;
                cLast.intRedirLink = Convert.ToInt32(Request.QueryString["Redir"]);
                if (cLast.intRedirLink == clsUtility.REDIR_MyConfBook | cLast.intRedirLink == clsUtility.REDIR_MyConfBookPrior)
                {
                    if (Request.QueryString["EID"] != null)
                    {
                        int eID = Convert.ToInt32(Request.QueryString["EID"]);
                        if (Request.QueryString["PR"] == null)
                            eID = (eID == 48 ? 47 : eID);
                        cLast.intEventSelector = eID;
                    }
                }
                if (Request.QueryString["EVT"] != null)
                {
                    int EVT = Convert.ToInt32(Request.QueryString["EVT"]);
                    cLast.intSwitchEventPkey = EVT;
                }

                Session["cLastUsed"] = cLast;
                return Redirect("/Home/Login" + Request.Url.Query);
            }



            User_Login data = new User_Login();
            DataSet infoTables = new DataSet();
            ViewData["lblSpkr"] = cSettings.strHomePageTitle1.ToString();
            ViewData["lblSpkr1"] = cSettings.strHomePageSubTitle1.ToString();
            ViewData["lblCtn"] = cSettings.strHomePageTitle2.ToString();
            ViewData["lblCtn1"] = cSettings.strHomePageSubTitle2.ToString();
            string strInfo = "", strNews = "", strFuture = "";
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
            }
            else
            {
                data.EventId = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
                data.Id = 0;
            }

            string strPartnerAlias = "Event Sponsor";
            System.Data.DataTable EventInfo = repository.getDyamicEventSettings(data.EventId, "EventFullname,PartnerAlias,IsEventOpenForStaff,IsEventOpenForEventSponsors,IsEventOpenForSpeaker,IsEventClosedForAttendees,ShowVirtaulBigButton,ShowEventPages,ShowHomeRegisterButton,StartDate,EndDate");
            bool bEventOpenStaff = false, bEventOpenEventSponsors = false, bEventOpenSpeakers = false, bShowVirtaulBigButton = false, bShowEvtPages = false, bEventClosedAttendees = false, bShowRegisterButton = false;
            DateTime dtStartDate = System.DateTime.Now, dtEndDate = System.DateTime.Now;
            if (EventInfo != null && EventInfo.Rows.Count > 0)
            {
                data.EventName = (EventInfo.Rows[0]["EventFullname"] == DBNull.Value) ? "" : EventInfo.Rows[0]["EventFullname"].ToString();
                bEventOpenStaff = (EventInfo.Rows[0]["IsEventOpenForStaff"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["IsEventOpenForStaff"].ToString());
                bEventOpenEventSponsors = (EventInfo.Rows[0]["IsEventOpenForEventSponsors"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["IsEventOpenForEventSponsors"].ToString());
                bEventOpenSpeakers = (EventInfo.Rows[0]["IsEventOpenForSpeaker"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["IsEventOpenForSpeaker"].ToString());
                bEventClosedAttendees = (EventInfo.Rows[0]["IsEventClosedForAttendees"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["IsEventClosedForAttendees"].ToString());
                bShowVirtaulBigButton = (EventInfo.Rows[0]["ShowVirtaulBigButton"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["ShowVirtaulBigButton"].ToString());
                bShowEvtPages = (EventInfo.Rows[0]["ShowEventPages"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["ShowEventPages"].ToString());
                bShowRegisterButton = (EventInfo.Rows[0]["ShowHomeRegisterButton"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["ShowHomeRegisterButton"].ToString());
                if(bShowRegisterButton)
                {
                    try
                    {
                        clsEvent cEvent = new clsEvent();
                        cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(Session["sqlConn"].ToString());
                        bShowRegisterButton = cEvent.CheckValiditityOfModule(cLast.intActiveEventPkey, "ShowHomeRegisterButton");
                    }catch { }
                }
                strPartnerAlias =  (EventInfo.Rows[0]["PartnerAlias"] == DBNull.Value) ? "" : EventInfo.Rows[0]["PartnerAlias"].ToString();
                dtStartDate =(EventInfo.Rows[0]["StartDate"] == DBNull.Value) ? System.DateTime.Now : Convert.ToDateTime(EventInfo.Rows[0]["StartDate"]);
                dtEndDate =(EventInfo.Rows[0]["EndDate"] == DBNull.Value) ? System.DateTime.Now : Convert.ToDateTime(EventInfo.Rows[0]["EndDate"]);
            }

            if (ConfigurationManager.AppSettings["QAMode"] == "1" && data.Id == 0)
            {
                if (Request.Cookies["Accountpkey"] != null && !string.IsNullOrEmpty(Request.Cookies["Accountpkey"].Value) && Request.Cookies["Accountpkey"].Value !="0"  && cLast.intBrowserClose == 0)
                {
                    DateTime currentDate = clsEvent.getEventVenueTime();
                    if ((currentDate >= dtStartDate && currentDate <= dtEndDate) || ConfigurationManager.AppSettings["QAMode"] == "1")
                    {
                        int Account_pKey = Convert.ToInt32(Request.Cookies["Accountpkey"].Value);

                        Request.Cookies["Accountpkey"].Value = Account_pKey.ToString();
                        Request.Cookies["Accountpkey"].Expires = DateTime.Now.AddDays(15);
                        // Login To MVC Panel 
                        data =  new SqlOperation().LoginByAccountIDFromCookie(Account_pKey.ToString(), true);
                        LoginAccountQA(true, data.Email, Account_pKey);
                        cLast.intBrowserClose = 1;
                        Session["cLastUsed"] = cLast;
                        ViewBag.ID = data.Id;
                        ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                        if (!string.IsNullOrEmpty(Request.QueryString["LastPage_URL"]))
                            return Redirect("/" + Request.QueryString["LastPage_URL"].Trim());
                    }
                }
            }

            InitializeEventsCollection(cLast, cSettings);

            bool bAttendeeAtCurrEvent = false, bStaffForCurEvent = false;
            int NumTimesSpeakingCurEvent = 0;
            if (data.Id > 0)
            {
                System.Data.DataTable AccountSettings = repository.getMenuAccountSettings(data.EventId, data.Id);
                if (AccountSettings != null && AccountSettings.Rows.Count > 0)
                {
                    bAttendeeAtCurrEvent = (AccountSettings.Rows[0]["AttendeeAtCurrEvent"] == DBNull.Value) ? false : Convert.ToBoolean(AccountSettings.Rows[0]["AttendeeAtCurrEvent"]);
                    bStaffForCurEvent = (AccountSettings.Rows[0]["StaffForCurEvent"] == DBNull.Value) ? false : Convert.ToBoolean(AccountSettings.Rows[0]["StaffForCurEvent"]);
                    NumTimesSpeakingCurEvent = (AccountSettings.Rows[0]["NumTimesSpeakingCurEvent"] == DBNull.Value) ? 0 : Convert.ToInt32(AccountSettings.Rows[0]["NumTimesSpeakingCurEvent"].ToString());
                }
            }

            ViewData["EnterEventTitle"] = (data.Id == 0) ? "Sign in and then click to enter the " + data.EventName : "Enter the " + data.EventName;
            ViewData["ForeColor"] = cSettings.strBlockTopColor;
            ViewData["lblBlk1"] = cSettings.strBlockTitle1;
            ViewData["lblBlk2"] = cSettings.strBlockTitle2;
            ViewData["cmdEnterEvent_Visible"] = true;
            ViewData["btnEnterEvent_Visible"] = true;
            if (data.Id != 0)
            {
                if (bEventOpenStaff && (bStaffForCurEvent || data.GlobalAdmin || data.StaffMember))
                {
                    repository.ReplaceReservedWordsIndex(cSettings, data.EventId, cSettings.getText(clsSettings.TEXT_HomePage1).Replace("Sign in and then click to enter", data.Id == 0 ? "Sign in and then click to enter" : "Enter"), ref strInfo, cSettings.getText(clsSettings.TEXT_HomePage2), ref strNews, cSettings.getText(clsSettings.TEXT_HomePage3), ref strFuture);
                }
                else if (bEventOpenEventSponsors && clsEventOrganization.CheckExhibitor(data.ParentOrganization_pKey, data.EventId))
                {
                    repository.ReplaceReservedWordsIndex(cSettings, data.EventId, cSettings.getText(clsSettings.TEXT_HomePage1).Replace("Sign in and then click to enter", data.Id == 0 ? "Sign in and then click to enter" : "Enter"), ref strInfo, cSettings.getText(clsSettings.TEXT_HomePage2), ref strNews, cSettings.getText(clsSettings.TEXT_HomePage3), ref strFuture);
                }
                else if (bEventOpenSpeakers && NumTimesSpeakingCurEvent > 0)
                {
                    repository.ReplaceReservedWordsIndex(cSettings, data.EventId, cSettings.getText(clsSettings.TEXT_HomePage1).Replace("Sign in and then click to enter", data.Id == 0 ? "Sign in and then click to enter" : "Enter"), ref strInfo, cSettings.getText(clsSettings.TEXT_HomePage2), ref strNews, cSettings.getText(clsSettings.TEXT_HomePage3), ref strFuture);
                }
                else if (bEventClosedAttendees && bAttendeeAtCurrEvent && !(bStaffForCurEvent || data.GlobalAdmin || data.StaffMember))
                {
                    repository.ReplaceReservedWordsIndex(cSettings, data.EventId, cSettings.getText(clsSettings.Text_HomePage1Info).Replace("Sign in and then click to enter", data.Id == 0 ? "Sign in and then click to enter" : "Enter"), ref strInfo, cSettings.getText(clsSettings.TEXT_HomePage2), ref strNews, cSettings.getText(clsSettings.TEXT_HomePage3), ref strFuture);
                    ViewData["cmdEnterEvent_Visible"] = false;
                    ViewData["btnEnterEvent_Visible"] = false;
                }
                else
                {
                    repository.ReplaceReservedWordsIndex(cSettings, data.EventId, (bShowVirtaulBigButton && bShowEvtPages ? cSettings.getText(clsSettings.TEXT_HomePage1).Replace("Sign in and then click to enter", data.Id == 0 ? "Sign in and then click to enter" : "Enter") : cSettings.getText(clsSettings.Text_HomePage1Info)), ref strInfo, cSettings.getText(clsSettings.TEXT_HomePage2), ref strNews, cSettings.getText(clsSettings.TEXT_HomePage3), ref strFuture);
                    if (!(bShowVirtaulBigButton && bShowEvtPages))
                    {
                        ViewData["cmdEnterEvent_Visible"] = false;
                        ViewData["btnEnterEvent_Visible"] = false;
                    }
                }
            }
            else
            {
                repository.ReplaceReservedWordsIndex(cSettings, data.EventId, (bShowVirtaulBigButton && bShowEvtPages ? cSettings.getText(clsSettings.TEXT_HomePage1).Replace("Sign in and then click to enter", data.Id == 0 ? "Sign in and then click to enter" : "Enter") : cSettings.getText(clsSettings.Text_HomePage1Info)), ref strInfo, cSettings.getText(clsSettings.TEXT_HomePage2), ref strNews, cSettings.getText(clsSettings.TEXT_HomePage3), ref strFuture);
                if (!(bShowVirtaulBigButton && bShowEvtPages))
                {
                    ViewData["cmdEnterEvent_Visible"] = false;
                    ViewData["btnEnterEvent_Visible"] = false;
                }
            }
            ViewData["RegisterBtn_Visible"] = bShowRegisterButton && !(bShowVirtaulBigButton && bShowEvtPages);
            // --replace conditionals
            clsReservedWords.ReplaceConditionals(ref strInfo, ref strNews, ref strFuture);
            ViewData["Advertisement_Visible"] = cSettings.bShowAdvertisement;
            if (cSettings.bShowAdvertisement)
                ViewData["lblAdvertisement"] = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.TEXT_Advertisement), null, intEvtPKey: data.EventId);

            ViewData["lblInfo"] = strInfo.Replace("[EventFullName]", data.EventName);
            ViewData["lblNews"] = strNews;
            ViewData["lblEvent"] = data.EventName;
            ViewData["lblEventDetail"] = "";
            ViewData["ToolTip"] = "";
            ViewData["ImageUrl"] = "";
            ViewData["phNextConfb_Visible"] = false;
            ViewData["cmdNextb_Visible"] = false;
            ViewData["cmdPriorb_Visible"] = false;

            if (cLast != null && cLast.colEventControlpKeys.Count > 0)
            {
                DataTable dtHomeEvent = (DataTable)ViewData["ddHomeEvent"];
                ViewData["phNextConfb_Visible"] = (cLast.colEventControlpKeys.Count > 1);
                ViewData["cmdNextb_Visible"] = (cLast.intEventControlCurrent < cLast.colEventControlpKeys.Count);
                ViewData["cmdPriorb_Visible"] = (cLast.intEventControlCurrent > 1);
                clsOneEvent cOneEvent = (clsOneEvent)cLast.colEventControlpKeys[cLast.intEventControlCurrentMVC];
                if (cOneEvent.intEventPkey != cLast.intActiveEventPkey && dtHomeEvent != null && dtHomeEvent.Rows.Count>0)
                {
                    DataRow dr = (dtHomeEvent.Select("pKey =" + cLast.intActiveEventPkey).FirstOrDefault());
                    if (dr!= null)
                    {
                        cOneEvent.intEventPkey = Convert.ToInt32(dr["pKey"]);
                        cOneEvent.strEventID = (dr["EventID"] != DBNull.Value) ? dr["EventID"].ToString() : "";
                        if (!Convert.IsDBNull(dr["EventType_PKey"])) ViewData["intEventType_PKey"] = Convert.ToInt32(dr["EventType_PKey"]);
                        if (!Convert.IsDBNull(dr["HomelinkDisable"])) cOneEvent.bHomelinkDisable = Convert.ToBoolean(dr["HomelinkDisable"]);
                        cOneEvent.strVenueID = (dr["Venue"] != DBNull.Value) ? dr["Venue"].ToString() : "";
                        cOneEvent.strLocationCity =  (dr["LocationCity"] != DBNull.Value) ? dr["LocationCity"].ToString() : "";
                        cOneEvent.strLocationState = (dr["LocationState"] != DBNull.Value) ? dr["LocationState"].ToString() : "";
                        cOneEvent.strEventFullname = (dr["EventFullname"] != DBNull.Value) ? dr["EventFullname"].ToString() : "";
                        cOneEvent.strVenueSmall = (dr["VenueSmall"] != DBNull.Value) ? dr["VenueSmall"].ToString() : "";
                        cOneEvent.strEventHomeBanner = (dr["HomeBanner"] != DBNull.Value) ? dr["HomeBanner"].ToString() : "";
                        if (!Convert.IsDBNull(dr["StartDate"])) cOneEvent.dtStartDate = Convert.ToDateTime(dr["StartDate"]);
                        if (!Convert.IsDBNull(dr["EndDate"])) cOneEvent.dtEndDate = Convert.ToDateTime(dr["EndDate"]);
                        ViewData["intNextEvent"] = cOneEvent.intEventPkey;
                        ViewData["OneEventPKey"] = cOneEvent.intEventPkey;
                        if (cOneEvent.strEventID != "")
                        {
                            ViewData["intEventType_PKey"] = cOneEvent.intEventType_PKey;
                            ViewData["ImageUrl"] = (cOneEvent.strEventHomeBanner == "" ? "/venuedocuments/" + cOneEvent.strVenueSmall : "/images/BannerImages/" + cOneEvent.strEventHomeBanner + "?" + DateTime.Now.Ticks.ToString());
                            ViewData["ToolTip"] = cOneEvent.strLocationCity;
                            ViewData["lblEvent"] = cOneEvent.strEventFullname;
                            ViewData["lblEventDetail"] = (cOneEvent.strLocationCity == "" ? cOneEvent.strVenueID : cOneEvent.strLocationCity + ", ") + cOneEvent.strLocationState + " - " + cOneEvent.dtStartDate.ToString("MMMM") + " " + cOneEvent.dtStartDate.Day.ToString() + "-" + (cOneEvent.dtEndDate.ToString("MMMM") != cOneEvent.dtStartDate.ToString("MMMM") ? cOneEvent.dtEndDate.ToString("MMMM") + " " : "") + cOneEvent.dtEndDate.Day.ToString();
                            ViewData["bdisable"] = (cOneEvent.bHomelinkDisable ? false : true);
                            ViewData["imgConf_Enabled"] = (cOneEvent.bHomelinkDisable ? true : false);
                        }
                    }
                }
                ViewData["intNextEvent"] = cOneEvent.intEventPkey;
                ViewData["OneEventPKey"] = cOneEvent.intEventPkey;
                if (cOneEvent.strEventID != "")
                {
                    ViewData["intEventType_PKey"] = cOneEvent.intEventType_PKey;
                    ViewData["ImageUrl"] = (cOneEvent.strEventHomeBanner == "" ? "/venuedocuments/" + cOneEvent.strVenueSmall : "/images/BannerImages/" + cOneEvent.strEventHomeBanner + "?" + DateTime.Now.Ticks.ToString());
                    ViewData["ToolTip"] = cOneEvent.strLocationCity;
                    ViewData["lblEvent"] = cOneEvent.strEventFullname;
                    ViewData["lblEventDetail"] = (cOneEvent.strLocationCity == "" ? cOneEvent.strVenueID : cOneEvent.strLocationCity + ", ") + cOneEvent.strLocationState + " - " + cOneEvent.dtStartDate.ToString("MMMM") + " " + cOneEvent.dtStartDate.Day.ToString() + "-" + (cOneEvent.dtEndDate.ToString("MMMM") != cOneEvent.dtStartDate.ToString("MMMM") ? cOneEvent.dtEndDate.ToString("MMMM") + " " : "") + cOneEvent.dtEndDate.Day.ToString();
                    ViewData["bdisable"] = (cOneEvent.bHomelinkDisable ? false : true); // c.bHomelinkDisable
                    ViewData["imgConf_Enabled"] = (cOneEvent.bHomelinkDisable ? true : false);
                }
                else
                    ViewData["lblEvent"] = "Not Found";
                DataTable RefreshLinks = new DataTable();
                ViewBag.SelectedEventHome= cOneEvent.intEventPkey;
                bool cached = false;
                if (HttpContext.Cache["RefreshQLinks"] != null && HttpContext.Cache["HomeEventPkey"].ToString() == cOneEvent.intEventPkey.ToString())
                {
                    RefreshLinks = (DataTable)HttpContext.Cache["RefreshQLinks"];
                    if (RefreshLinks.Columns.Contains("LinkPageMvc"))
                        cached =true;
                }

                if (!cached)
                {
                    RefreshLinks = repository.refreshQLinks(cOneEvent.intEventPkey, cOneEvent.intEventType_PKey);
                    HttpContext.Cache.Insert("RefreshQLinks", RefreshLinks, null, DateTime.MaxValue, TimeSpan.FromMinutes(300));
                    HttpContext.Cache.Insert("HomeEventPkey", cOneEvent.intEventPkey.ToString(), null, DateTime.MaxValue, TimeSpan.FromMinutes(300));
                }
                ViewData["RefreshLinks"] = RefreshLinks;
                if (RefreshLinks != null)
                {
                    infoTables.Tables.Add(GetRefreshFilteredLinks(RefreshLinks, strPartnerAlias, 1));
                    infoTables.Tables.Add(GetRefreshFilteredLinks(RefreshLinks, strPartnerAlias, 2));
                    infoTables.Tables.Add(GetRefreshFilteredLinks(RefreshLinks, strPartnerAlias, 3));
                }
            }
            return View(infoTables);
        }
        private int BoothSetting_pkey(clsLastUsed cLast, int ParentOrg)
        {
            try
            {
                System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                int ResBoothSetting_pkey = 0;
                string qry = "select isnull(BoothSetting_pkey,0) as BoothSetting_pkey  From   Event_Organizations where Event_Pkey=" + cLast.intActiveEventPkey.ToString();
                qry = qry + Environment.NewLine + "and Organization_pKey = " + ParentOrg.ToString();
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(qry);
                DataTable dt = new DataTable();
                if (clsUtility.GetDataTable(conn, cmd, ref dt))
                {
                    if (dt != null && dt.Rows.Count > 0)
                        ResBoothSetting_pkey = Convert.ToInt32(dt.Rows[0]["BoothSetting_pkey"]);
                }
                return ResBoothSetting_pkey;
            }
            catch
            {
                return 0;
            }
        }
        private string CloseForm(User_Login Model, bool bSuccess = false)
        {
            try
            {
                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                if (bSuccess)
                {
                    if (Request.QueryString["ESPK"] != null)
                        return "/MySpeakerPage?ESPK=" + Request.QueryString["ESPK"].ToString();
                    if (Request.QueryString["Reg"] != null && Request.QueryString["PReg"] != null)
                        return "/Registration?Reg=" + Request.QueryString["Reg"].ToString() + "&PReg=" + Request.QueryString["PReg"].ToString();
                    if (Request.QueryString["APK"] != null && Request.QueryString["ST"] != null)
                        return "/SpeakerRegistrationFeedback?APK=" + Request.QueryString["APK"].ToString() + "&ST=" + Request.QueryString["ST"].ToString();
                    if (Request.QueryString["APK"] != null && Request.QueryString["EPK"] != null)
                        return "/EventTasks?APK=" + Request.QueryString["APK"].ToString() + "&EPK=" + Request.QueryString["EPK"].ToString();
                    if (Request.QueryString["TPK"] != null)
                        return "/EditTask?TPK=" + Request.QueryString["TPK"].ToString();

                    string strPageID = Request.QueryString["Spkr"];
                    if (!string.IsNullOrEmpty(strPageID))
                    {
                        int PageID = Convert.ToInt32(strPageID);
                        switch (PageID)
                        {
                            case 1: cLast.intAccountLastTab = 4; return "/EditAccount";
                            case 2: cLast.intAccountLastTab = 0; return "/EditAccount";
                            case 3: Response.Redirect("/MyMagi"); break;
                            case 4: Response.Redirect("/MyConference"); break;
                            case 5:
                                if (Model.GlobalAdmin)
                                    return "/SpeakingInterest";
                                else
                                    return "/EditAccount?Spkr=1";
                        }
                    }
                    // --if success, remove self from stack
                    ((clsFormList)Session["cFormlist"]).RemoveTopForm();

                    // --redirect mode?
                    if (cLast.bRedirPending)
                    {
                        cLast.bRedirPending = false;
                        cLast.bRedirectJournal = true;
                        if (cLast.intSwitchEventPkey > 0 && cLast.intSwitchEventPkey != cLast.intActiveEventPkey)
                        {
                            DataTable dt = new DataTable();
                            DataTable EventInfo = repository.getDyamicEventSettings(cLast.intSwitchEventPkey, "pKey,EventID,EventType_PKey");
                            if (dt != null && dt.Rows.Count>0)
                            {
                                clsAccount cAccount = new clsAccount();
                                cAccount.sqlConn =new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                                cAccount.intAccount_PKey = Model.Id;
                                if (!cAccount.ChooseEvent(Convert.ToString(dt.Rows[0]["EventID"]), cLast.intSwitchEventPkey))
                                    return "";
                                cSettings.intHighlightedEvent = cLast.intSwitchEventPkey;
                                cLast.intEventSelector = -1;
                            }
                            cLast.intSwitchEventPkey = -1;
                        }
                        switch (cLast.intRedirLink)
                        {
                            case clsUtility.REDIR_MyCert:
                                return "/MyCertifications";

                            case clsUtility.REDIR_MyConf:
                                return "/MyConference";

                            case clsUtility.REDIR_MyProf:
                                return "/ViewAccount";

                            case clsUtility.REDIR_BRPartnerReport:
                                return "/BRPartnerReports";

                            case clsUtility.REDIR_SpkrFeedback:
                                return "/MyFeedback";

                            case clsUtility.REDIR_SpkrDinner:
                                return "/MySpeakerDinner";

                            case clsUtility.REDIR_MyConfBookPrior:
                                return "/MyConferenceBook?P=1";

                            case clsUtility.REDIR_MyConfBook:
                                return "/MyConferenceBook";

                            case clsUtility.REDIR_AccountList:
                                return "/EditAccount?Subscription=1";

                            case clsUtility.REDIR_AccListChngInfo:
                                return "/EditAccount?PK=" + User.Identity.Name;

                            case clsUtility.REDIR_Journal:
                                return "/Journal?S=1";

                            case clsUtility.REDIR_CreateSch:
                                return "MySessions";

                            case clsUtility.REDIR_ViewSch:
                                return "/MySchedule";

                            case clsUtility.REDIR_MyOption:
                                return "/MyOptions";

                            case clsUtility.REDIR_ChairmanLetter:
                                return "/Overview?M=31";

                            case clsUtility.REDIR_ContactHours:
                                return "/Overview?M=32";

                            case clsUtility.REDIR_Speakers:
                                return "/Speakers";

                            case clsUtility.REDIR_Venue:
                                return "/VenueInfo";

                            case clsUtility.REDIR_Overview:
                                return "/Overview";

                            case clsUtility.REDIR_BeAPartner:
                                return "/Overview?M=30";

                            case clsUtility.REDIR_ContinueEducation:
                                return "/Overview?M=32";

                            case clsUtility.REDIR_FounderBio:
                                return "/Overview?M=39";

                            case clsUtility.REDIR_MoreInfo:
                                return "/Overview?M=40";

                            case clsUtility.REDIR_BeASpeaker:
                                return "/BeASpeaker";

                            case clsUtility.REDIR_Conference:
                                return "/EventInfo";

                            case clsUtility.REDIR_Partners:
                                return "/Partners";

                            case clsUtility.REDIR_EventContacts:
                                return "/EventContacts?M=1";

                            case clsUtility.REDIR_EventFeedback:
                                return "/MyConferenceFeedback";

                            case clsUtility.REDIR_ExhibitorSurvey:
                                return "/Survey?EFPK=" + clsUtility.Encrypt("7");

                            case clsUtility.REDIR_AttendeeSurvey:
                                return "/Survey?EFPK=" + clsUtility.Encrypt("8");

                            case clsUtility.REDIR_NotAttendedSurvey:
                                return "/Survey?EFPK=" + clsUtility.Encrypt("9");

                            case clsUtility.REDIR_SpeakerSurvey:
                                return "/Survey?EFPK=" + clsUtility.Encrypt("10");

                            case clsUtility.REDIR_VisitedExhibitHall:
                                int intBoothId = BoothSetting_pkey(cLast, Model.ParentOrganization_pKey);
                                if (intBoothId > 0)
                                    return "ExhibitHall?PK=" + intBoothId.ToString();
                                else
                                    return "ExhibitHall";
                            case clsUtility.REDIR_SessionFeedback:
                                return "/MySchedule";
                            case clsUtility.REDIR_MyCertificate:
                                return "/MyCertifications";

                            case clsUtility.REDIR_Register:
                                if (Request.QueryString["Reg"] != null && clsUtility.Decrypt(Request.QueryString["Reg"].ToString()) == Model.Email)
                                    return "/registration.aspx?Reg=" + clsUtility.Encrypt(Model.Email) + (Request.QueryString["EvtPkey"] != null ? "&EVT=" + Request.QueryString["EvtPkey"].ToString() : "");
                                break;
                        }
                    }

                    // --if still here, go to default pages
                    string s = Request.QueryString["LastPage_URL"];
                    if (!String.IsNullOrEmpty(s) && Request.QueryString["IsApproval"] != null)
                        s = s + "&IsApproval=1";
                    else
                        s = "/Home/Index";
                    return s;
                }
                else
                    return ((clsFormList)Session["cFormlist"]).getPriorForm();
            }
            catch
            {
                return "/Home/Index";
            }
        }
        public ActionResult Login()
        {
            if (!User.Identity.IsAuthenticated)
            {
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                User_Login data = new User_Login();

                if (ConfigurationManager.AppSettings["QAMode"] == "1" && data.Id == 0 && Request.Cookies["Accountpkey"] != null && !string.IsNullOrEmpty(Request.Cookies["Accountpkey"].Value)  && Request.Cookies["Accountpkey"].Value !="0"  && cLast.intBrowserClose == 0)
                {
                    System.Data.DataTable EventInfo = repository.getDyamicEventSettings(data.EventId, "StartDate,EndDate");
                    DateTime dtStartDate = System.DateTime.Now, dtEndDate = System.DateTime.Now;
                    if (EventInfo != null && EventInfo.Rows.Count > 0)
                    {
                        dtStartDate =(EventInfo.Rows[0]["StartDate"] == DBNull.Value) ? System.DateTime.Now : Convert.ToDateTime(EventInfo.Rows[0]["StartDate"]);
                        dtEndDate =(EventInfo.Rows[0]["EndDate"] == DBNull.Value) ? System.DateTime.Now : Convert.ToDateTime(EventInfo.Rows[0]["EndDate"]);
                    }
                    DateTime currentDate = clsEvent.getEventVenueTime();
                    if ((currentDate >= dtStartDate && currentDate <= dtEndDate) || ConfigurationManager.AppSettings["QAMode"] == "1")
                    {
                        int Account_pKey = Convert.ToInt32(Request.Cookies["Accountpkey"].Value);
                        Request.Cookies["Accountpkey"].Value = Account_pKey.ToString();
                        Request.Cookies["Accountpkey"].Expires = DateTime.Now.AddDays(15);
                        // Login To MVC Panel 
                        data =  new SqlOperation().LoginByAccountIDFromCookie(Account_pKey.ToString(), true);
                        LoginAccountQA(true, data.UserId, Account_pKey);
                        cLast.intBrowserClose = 1;
                        Session["cLastUsed"] = cLast;

                        if (!string.IsNullOrEmpty(Request.QueryString["LastPage_URL"]))
                            Response.Redirect("/" + Request.QueryString["LastPage_URL"].Trim());
                        else
                            return Redirect("/Home/Index");
                    }
                }
                ViewData["Title"] = "Login";
                HttpContext.Session["intRetryCount"] = 0;
                ViewData["Instruct"] = cSettings.getText(clsSettings.TEXT_PassRequest);
                ViewBag.UserName   ="";
                if (Request.QueryString["Reg"] != null)
                    ViewBag.UserName   = clsUtility.Decrypt(Request.QueryString["Reg"]);
                else if (Request.Cookies["UserName"] != null)
                    ViewBag.UserName = Request.Cookies["UserName"].Value;
                return View();
            }
            else
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                string RedirectURL = CloseForm(data, true);
                return Redirect(RedirectURL);
            }
        }
        public ActionResult _PartialLogin()
        {
            ViewData["Title"] = "Login";
            HttpContext.Session["intRetryCount"] = 0;
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            ViewData["Instruct"] = cSettings.getText(clsSettings.TEXT_PassRequest);
            ViewBag.UserName   ="";
            if (Request.QueryString["Reg"] != null)
                ViewBag.UserName   = clsUtility.Decrypt(Request.QueryString["Reg"]);
            else if (Request.Cookies["UserName"] != null)
                ViewBag.UserName = Request.Cookies["UserName"].Value;
            return PartialView();
        }
        protected string GeneratePasscode()
        {
            string alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string small_alphabets = "abcdefghijklmnopqrstuvwxyz";
            string numbers = "1234567890";

            string characters = numbers;

            characters += Convert.ToString(alphabets + small_alphabets) + numbers;

            int length = 10;
            string otp = string.Empty;
            for (int i = 0; i <= length - 1; i++)
            {
                string character = string.Empty;
                do
                {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                }
                while (otp.IndexOf(character) != -1);
                otp += character;
            }
            return otp;
        }
        private string SendOneTimeLogin(string strEMail, int Account_pKey, string ID)
        {
            // --identify person
            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            clsAccount c = new clsAccount();
            c.sqlConn = sqlConn;
            c.lblMsg = null;
            c.intAccount_PKey = Account_pKey;
            c.strEmail = strEMail;
            if (clsUtility.CheckEmailFormat(strEMail))
                c.LoadAccountByEmail();
            else if (Account_pKey>0)
                c.LoadAccount();
            if (c.intAccount_PKey > 0)
            {
                clsAnnouncement Ann = clsAnnouncement.getAnnouncementInfo(sqlConn, null, clsAnnouncement.Text_LoginOneTIme);
                string strSubject = Ann.strTitle;
                string strContent = Ann.strHTMLText;
                strContent = strContent.Replace("[Url]", clsSettings.APP_URL().Replace("/forms", "") + "/Login?ULID=" + ID);
                strContent = strContent.Replace("[MAGI]", "MAGI");
                // --send email
                clsEmail cEmail = new clsEmail();
                cEmail.lblMsg = null;
                cEmail.strSubjectLine = strSubject;
                cEmail.strHTMLContent = strContent;
                if (!cEmail.SendEmailToAccount(c, bPlainText: false))
                    return "Error Occurred While Seding Help Mail";
                cEmail = null;
            }
            else
                return "No Account Found Related To Email To Send Login one Time";
            c = null;
            return "OK";
        }
        private bool SendPassCodeSMS(string SMSBody, string Number, int AccountPkey, int AddedBy)
        {
            clsSMS obj = new clsSMS();
            try
            {
                if (!string.IsNullOrEmpty(SMSBody) && !string.IsNullOrEmpty(Number))
                {
                    obj.SendMessage(SMSBody, Number, AccountPkey, AddedBy);
                    return true;
                }
            }
            catch (Exception ex)
            {

            }
            return false;
        }
        protected string GenerateNumericPasscode(int length)
        {
            string numbers = "1234567890";
            string characters = numbers;
            string otp = string.Empty;
            for (int i = 0; i <= length - 1; i++)
            {
                string character = string.Empty;
                do
                {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                }
                while (otp.IndexOf(character) != -1);
                otp += character;
            }
            return otp;
        }

        private bool HaveAnyInterestGroup(int accKey, int eventKey)
        {
            bool haveAnyGroup = false;
            string qry = string.Empty;
            qry = "select c.ID from sys_chats c inner join [InterestBasedChatGroups] ib on 'admingrp' + cast(ib.pkey as varchar) = cast(c.ID as varchar) and ib.eventKey = c.Event_pkey inner join Event_Accounts ea on ea.Account_pKey = c.myID and ea.ParticipationStatus_pKey=1 and ea.Event_pKey=c.Event_pkey where c.myID = " + accKey.ToString() + " and c.Event_pkey = " + eventKey.ToString() + " and upper(c.ID) like 'ADMINGRP%'";
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(qry);
            DataTable dt = new DataTable();

            System.Data.SqlClient.SqlParameter[] sqlParameters =
            {
                new System.Data.SqlClient.SqlParameter("@myID", accKey),
                new System.Data.SqlClient.SqlParameter("@ParamEventKey", eventKey)
            };
            dt = SqlHelper.ExecuteTable(qry, CommandType.Text, sqlParameters);
            if (dt.Rows.Count > 0)
            {
                haveAnyGroup = true;
            }
            return haveAnyGroup;
        }
        private string SendPassCodeEmail(string title, string text, string passcode, int PasscodeExpiry, clsAccount c, string ErrorMsg)
        {
            string strSubject = title;
            string strContent = text;
            strContent = strContent.Replace("[Url]", clsSettings.APP_URL().Replace("/forms", "") + "/login?bPasscode=1");
            strContent= c.ReplaceReservedWords(strContent);
            strContent = strContent.Replace("[Name]", (string.IsNullOrEmpty(c.strNickname.Trim()) ? c.strFirstname.Trim() : c.strNickname.Trim()));
            strContent = strContent.Replace("[AcctNickname]", (string.IsNullOrEmpty(c.strNickname.Trim()) ? c.strFirstname.Trim() : c.strNickname.Trim()));
            strContent = strContent.Replace("[Passcode]", passcode);
            strContent = strContent.Replace("[PasscodeExpire]", PasscodeExpiry.ToString() + ((PasscodeExpiry > 1) ? " minutes" : " minute"));
            // --send email
            clsEmail cEmail = new clsEmail();
            cEmail.strSubjectLine = strSubject;
            cEmail.strHTMLContent = strContent;
            if (!String.IsNullOrEmpty(c.strEmail2))
                cEmail.strEmailCC= c.strEmail2;
            if (!cEmail.SendEmailToAccount(c, bPlainText: false))
                return "Error Occurred While Seding Help Mail";
            cEmail = null;

            return ErrorMsg;
        }
        private string SendPasscode(string strEMail, int Account_pKey, string strPasscode, bool SMS = false, string ErrorMsg = "")
        {
            // --identify person
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            clsAccount c = new clsAccount();
            c.sqlConn = sqlConn;
            c.lblMsg = null;
            c.strEmail = strEMail;
            c.intAccount_PKey = Account_pKey;
            if (clsUtility.CheckEmailFormat(strEMail))
                c.LoadAccountByEmail();
            else if (Account_pKey>0)
                c.LoadAccount();

            if (string.IsNullOrEmpty(ErrorMsg))
                ErrorMsg = "Passcode email has been sent";

            if (c.intAccount_PKey > 0)
            {
                clsAnnouncement Ann = clsAnnouncement.getAnnouncementInfo(sqlConn, null, clsAnnouncement.TEXT_Passcode);
                if (!SMS)
                    return SendPassCodeEmail(Ann.strTitle, Ann.strHTMLText, strPasscode, cSettings.PasscodeExpiry, c, ErrorMsg: ErrorMsg);

                if (SMS) //&& (c.bGlobalAdministrator || c.bStaffMember)
                {
                    string strSmsText = (string.IsNullOrEmpty(Ann.strSmstext)) ? "" : Ann.strSmstext;
                    if (!string.IsNullOrEmpty(strSmsText))
                    {
                        strSmsText = strSmsText.Replace("[Url]", clsSettings.APP_URL().Replace("/forms", "") + "/login?bPasscode=1");
                        strSmsText = strSmsText.Replace("[Name]", (string.IsNullOrEmpty(c.strNickname.Trim()) ? c.strFirstname.Trim() : c.strNickname.Trim()));
                        strSmsText = strSmsText.Replace("[AcctNickname]", (string.IsNullOrEmpty(c.strNickname.Trim()) ? c.strFirstname.Trim() : c.strNickname.Trim()));
                        strSmsText = strSmsText.Replace("[Passcode]", strPasscode);
                        strSmsText = strSmsText.Replace("[PasscodeExpire]", cSettings.PasscodeExpiry.ToString() + ((cSettings.PasscodeExpiry>1) ? " minutes" : " minute"));
                        string MobileNumber = "";
                        if (string.IsNullOrEmpty(c.strPhone) && string.IsNullOrEmpty(c.strPhone2))
                            return SendPassCodeEmail(Ann.strTitle, Ann.strHTMLText, strPasscode, cSettings.PasscodeExpiry, c, "There is no mobile number in your profile so passcode has been sent to your email address(es).");
                        else
                            MobileNumber = clsAccount.getAccountMobileNumber(c.intAccount_PKey);

                        if (!string.IsNullOrEmpty(MobileNumber))
                        {
                            if (SendPassCodeSMS(strSmsText.Replace("<br />", System.Environment.NewLine), "+" + MobileNumber, c.intAccount_PKey, c.intAccount_PKey))
                                return "Text message with passcode sent to the mobile number in your profile";
                            else
                                return "Unable to send message to mobile number in profile.";
                        }
                        else
                            return "Both";
                    }
                    else
                        return SendPassCodeEmail(Ann.strTitle, Ann.strHTMLText, strPasscode, cSettings.PasscodeExpiry, c, "There is no mobile number in your profile so passcode has been sent to your email address(es).");
                }
                //if (SMS && !(c.bGlobalAdministrator || c.bStaffMember))
                //    return SendPassCodeEmail(Ann.strTitle, Ann.strHTMLText, strPasscode, cSettings.PasscodeExpiry, c, "There is no mobile number in your profile so passcode has been sent to your email address(es).");
                return "Error Occurred While Sending Passcode";
            }
            else
                return clsUtility.getErrorMessage(224);
            c = null;
        }
        public JsonResult NeedHelpSignIn(string strEMail, string SelectedValue)
        {
            string strToShow = "";
            clsAccount cAccount = new clsAccount();
            cAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            string strResult = "OK", redirectString = "";
            int resultID = 0;
            int AccountBeingChecked = 0;
            int ResultInfo = 0;

            if (string.IsNullOrEmpty(strEMail))
                return Json(new { msg = "No MAGI account found. Enter valid email address or user ID." }, JsonRequestBehavior.AllowGet);

            switch (SelectedValue)
            {
                case "0":
                    ResultInfo = cAccount.CheckUserLoginAccount(strEMail, ref AccountBeingChecked);
                    //resultID = cAccount.InsertOneTimeLogin(strEMail, AccountBeingChecked);
                    if (ResultInfo>0) // && resultID>0
                    {
                        //String EncryptID = clsUtility.Encrypt(resultID.ToString());
                        //strResult = SendOneTimeLogin(strEMail, AccountBeingChecked, EncryptID);
                        strResult = sendPWResetEmail(strEMail, AccountBeingChecked);
                    }
                    else
                        strResult ="No MAGI account found. Enter valid email address or user ID.";
                    break;
                case "1":
                    ResultInfo = cAccount.CheckUserLoginAccount(strEMail, ref AccountBeingChecked);
                    if (ResultInfo>0)
                    {
                        string strPasscode = GenerateNumericPasscode(6);
                        string result = cAccount.InsertPasscode(strEMail, strPasscode);
                        if (result.Contains("invalid"))
                            return Json(new { msg = "No MAGI account found. Enter valid email address or user ID." }, JsonRequestBehavior.AllowGet);
                        strResult =   SendPasscode(strEMail, AccountBeingChecked, strPasscode);
                    }
                    else
                    {
                        strToShow = "No MAGI account found. Enter valid email address or user ID.";
                        return Json(new { msg = strToShow, redirect = redirectString }, JsonRequestBehavior.AllowGet);
                    }
                    break;
                case "3":
                    ResultInfo = cAccount.CheckUserLoginAccount(strEMail, ref AccountBeingChecked);
                    if (ResultInfo>0)
                    {
                        string strPasscode = GenerateNumericPasscode(6);
                        string result = cAccount.InsertPasscode(strEMail, strPasscode);
                        if (result.Contains("invalid"))
                            return Json(new { msg = "No MAGI account found. Enter valid email address or user ID." }, JsonRequestBehavior.AllowGet);
                        strResult =   SendPasscode(strEMail, AccountBeingChecked, strPasscode, true);
                    }
                    else
                    {
                        strToShow ="No MAGI account found. Enter valid email address or user ID.";
                        return Json(new { msg = strToShow, redirect = redirectString }, JsonRequestBehavior.AllowGet);
                    }
                    break;
                case "2":
                    string strSubject = "Need help signing in";
                    clsLastUsed cLast = (clsLastUsed)Session["cLastUsed"];
                    cLast.colNotifications.Clear();
                    clsOneNotify c = new clsOneNotify();
                    c.strEmailAddress = cSettings.strSupportEmail;
                    cLast.colNotifications.Add(c);
                    c = null;
                    redirectString = "/SendEmail?S=" + HttpUtility.UrlEncode(strSubject);
                    break;
            }
            return Json(new { msg = strResult, redirect = redirectString }, JsonRequestBehavior.AllowGet);
        }
        private string sendPWResetEmail(string strEMail, int Account_pKey)
        {
            //if ((strEMail == "") | (!clsEmail.IsValidEmail(strEMail)))
            //    return clsUtility.getErrorMessage(223);  

            clsAccount c = new clsAccount();
            c.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            c.lblMsg = null;
            c.intAccount_PKey = Account_pKey;
            c.strEmail = strEMail;
            if (clsUtility.CheckEmailFormat(strEMail))
                c.LoadAccountByEmail();
            else if (Account_pKey > 0)
                c.LoadAccount();

            if (c.intAccount_PKey > 0)
            {
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                clsAnnouncement Ann = clsAnnouncement.getAnnouncementInfo(new System.Data.SqlClient.SqlConnection(ReadConnectionString()), null, clsAnnouncement.TEXT_PassReset);
                string strSubject = Ann.strTitle;
                string strContent = Ann.strHTMLText;
                strContent = strContent.Replace("[Name]", c.strFirstname);
                strContent = strContent.Replace("[Url]", clsSettings.APP_URL() + "/frmResetPassword.aspx?credential=" + clsUtility.Encrypt(c.strEmail));
                strContent= c.ReplaceReservedWords(strContent);
                strContent = strContent.Replace("[Support Email]", cSettings.strSupportEmail);
                strContent = strContent.Replace("[LinkName]", "click here");
                // --send email
                clsEmail cEmail = new clsEmail();
                cEmail.lblMsg = null;
                cEmail.strSubjectLine = strSubject;
                cEmail.strHTMLContent = strContent;
                if (!String.IsNullOrEmpty(c.strEmail2))
                    cEmail.strEmailCC= c.strEmail2;
                cEmail.strEmailBCC = "#";
                cEmail.intAnnouncement_pKey = clsAnnouncement.TEXT_PassReset;
                if (!cEmail.SendEmailToAccount(c, bPlainText: false))
                    return "Error Occurred While Sending Email";
                clsAccount.SetPasswordSentDate(c.intAccount_PKey, null);
                cEmail = null;
                return "Password email has been sent";
            }
            else
                return clsUtility.getErrorMessage(224);
            c = null;
        }
        private string SendPasscodeToBothNumbers(string strEMail, int Account_pKey, string strPasscode, bool SMS = false)
        {
            // --identify person
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            clsAccount c = new clsAccount();
            c.sqlConn = sqlConn;
            c.lblMsg = null;
            c.strEmail = strEMail;
            c.intAccount_PKey = Account_pKey;
            if (clsUtility.CheckEmailFormat(strEMail))
                c.LoadAccountByEmail();
            else if (Account_pKey>0)
                c.LoadAccount();

            if (c.intAccount_PKey > 0)
            {
                clsAnnouncement Ann = clsAnnouncement.getAnnouncementInfo(sqlConn, null, clsAnnouncement.TEXT_Passcode);
                if (SMS) //&& (c.bGlobalAdministrator || c.bStaffMember)
                {
                    string strSmsText = (string.IsNullOrEmpty(Ann.strSmstext)) ? "" : Ann.strSmstext;
                    if (!string.IsNullOrEmpty(strSmsText))
                    {
                        strSmsText = strSmsText.Replace("[Url]", clsSettings.APP_URL().Replace("/forms", "") + "/login?bPasscode=1");
                        strSmsText = strSmsText.Replace("[Name]", (string.IsNullOrEmpty(c.strNickname.Trim()) ? c.strFirstname.Trim() : c.strNickname.Trim()));
                        strSmsText = strSmsText.Replace("[AcctNickname]", (string.IsNullOrEmpty(c.strNickname.Trim()) ? c.strFirstname.Trim() : c.strNickname.Trim()));
                        strSmsText = strSmsText.Replace("[Passcode]", strPasscode);
                        strSmsText = strSmsText.Replace("[PasscodeExpire]", cSettings.PasscodeExpiry.ToString() + ((cSettings.PasscodeExpiry>1) ? " minutes" : " minute"));
                        string MobileNumber = clsAccount.getAccountMobileNumber(c.intAccount_PKey);
                        if (!string.IsNullOrEmpty(MobileNumber))
                        {
                            if (SendPassCodeSMS(strSmsText.Replace("<br />", System.Environment.NewLine), "+" + MobileNumber, c.intAccount_PKey, c.intAccount_PKey))
                                return "Text message with passcode sent to the mobile number in your profile";
                            else
                                return "Unable to send message to mobile number in profile";
                        }
                        else
                        {
                            DataTable dt = clsAccount.getAccountNumbers(c.intAccount_PKey);
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                string PhoneNumber = dt.Rows[0]["PhoneNumber1"].ToString();
                                string PhoneNumber2 = dt.Rows[0]["PhoneNumber2"].ToString();
                                if (!string.IsNullOrEmpty(PhoneNumber) && SendPassCodeSMS(strSmsText.Replace("<br />", System.Environment.NewLine), "+" +  PhoneNumber, Account_pKey, Account_pKey))
                                {
                                    clsAccount.UpdateAccountNumberTypes(1, Account_pKey, null);
                                    return "Text message with passcode sent to the mobile number in your profile";
                                }
                                else if (!string.IsNullOrEmpty(PhoneNumber2) && SendPassCodeSMS(strSmsText.Replace("<br />", System.Environment.NewLine), "+" +  PhoneNumber2, Account_pKey, Account_pKey))
                                {
                                    clsAccount.UpdateAccountNumberTypes(2, Account_pKey, null);
                                    return "Text message with passcode sent to the mobile number in your profile";
                                }
                                else
                                {
                                    string strNumbers = (!string.IsNullOrEmpty(PhoneNumber) && !string.IsNullOrEmpty(PhoneNumber2)) ? "numbers" : "number";
                                    return "Unable to send message to " + strNumbers + " in profile";
                                }
                            }
                            else
                                return SendPassCodeEmail(Ann.strTitle, Ann.strHTMLText, strPasscode, cSettings.PasscodeExpiry, c, "Unable to send message to numbers in profile. <br /> There is no mobile number in your profile so passcode has been sent to your email address(es).");
                        }
                    }
                    else
                        return "Send Passcode option is not configured yet, try using : Send passcode by email message";
                }
                //if (SMS && !(c.bGlobalAdministrator || c.bStaffMember))
                //    return "Send Passcode option is not configured yet, try using : Send passcode by email message";
                return "Error Occurred While Sending Passcode";
            }
            else
                return clsUtility.getErrorMessage(224);
            c = null;
        }
        public JsonResult SendPasscodeToBoth(string strEMail)
        {
            string strToShow = "";
            clsAccount cAccount = new clsAccount();
            cAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            string strResult = "", redirectString = "";
            int AccountBeingChecked = 0, ResultInfo = 0;
            ResultInfo = cAccount.CheckUserLoginAccount(strEMail, ref AccountBeingChecked);
            if (ResultInfo>0)
            {
                string strPasscode = GenerateNumericPasscode(6);
                string result = cAccount.InsertPasscode(strEMail, strPasscode);
                if (result.Contains("invalid"))
                    return Json(new { msg = "No MAGI account found. Enter valid email address or user ID." }, JsonRequestBehavior.AllowGet);
                strResult =   SendPasscodeToBothNumbers(strEMail, AccountBeingChecked, strPasscode, true);
                return Json(new { msg = strResult, redirect = redirectString }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                strToShow = "No MAGI account found. Enter valid email address or user ID.";
                return Json(new { msg = strToShow, redirect = redirectString }, JsonRequestBehavior.AllowGet);
            }
        }

        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult EventChange_Index(string ID, int SelectedValue = 0, int Current = 0)
        {
            try
            {
                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                switch (ID)
                {
                    case "selected":
                        cLast.intEventDropDown = SelectedValue;
                        cLast.intEventControlCurrentMVC = Current;
                        break;
                    case "prior":
                        if (cLast.intEventControlCurrentMVC > 1)
                            cLast.intEventControlCurrentMVC = cLast.intEventControlCurrentMVC - 1;
                        break;
                    case "next":
                        if (cLast.intEventControlCurrentMVC < cLast.colEventControlpKeys.Count)
                            cLast.intEventControlCurrentMVC = cLast.intEventControlCurrentMVC + 1;
                        break;
                }
                clsOneEvent cOneEvent = (clsOneEvent)cLast.colEventControlpKeys[cLast.intEventControlCurrentMVC];
                cLast.intEventDropDown = cOneEvent.intEventPkey;

                int intNextEvent = cOneEvent.intEventPkey, intEventType_PKey = 0;
                string ImageUrl = "/images/conferences/defaultsmall.jpg", ToolTip = "", EventFullName = "Not Found", EventDetail = "";
                bool HomeLinkDisabled = false, ImgConfEnabled = false;
                if (cOneEvent.strEventID != "")
                {
                    intEventType_PKey = cOneEvent.intEventType_PKey;
                    ImageUrl = (cOneEvent.strEventHomeBanner == "" ? "/venuedocuments/" + cOneEvent.strVenueSmall : "/images/BannerImages/" + cOneEvent.strEventHomeBanner + "?" + DateTime.Now.Ticks.ToString());
                    ToolTip = cOneEvent.strLocationCity;
                    EventFullName = cOneEvent.strEventFullname;
                    EventDetail = (cOneEvent.strLocationCity == "" ? cOneEvent.strVenueID : cOneEvent.strLocationCity + ", ") + cOneEvent.strLocationState + " - " + cOneEvent.dtStartDate.ToString("MMMM") + " " + cOneEvent.dtStartDate.Day.ToString() + "-" + (cOneEvent.dtEndDate.ToString("MMMM") != cOneEvent.dtStartDate.ToString("MMMM") ? cOneEvent.dtEndDate.ToString("MMMM") + " " : "") + cOneEvent.dtEndDate.Day.ToString();
                    HomeLinkDisabled = (cOneEvent.bHomelinkDisable ? false : true); // c.bHomelinkDisable
                    ImgConfEnabled = (cOneEvent.bHomelinkDisable ? true : false);
                }
                bool cached = false;
                DataTable RefreshLinks = new DataTable();
                if (HttpContext.Cache["RefreshQLinks"] != null && HttpContext.Cache["HomeEventPkey"].ToString() == cOneEvent.intEventPkey.ToString())
                {
                    RefreshLinks = (DataTable)HttpContext.Cache["RefreshQLinks"];
                    if (RefreshLinks.Columns.Contains("LinkPageMvc"))
                        cached =true;
                }

                if (!cached)
                {
                    RefreshLinks = repository.refreshQLinks(cOneEvent.intEventPkey, cOneEvent.intEventType_PKey);
                    HttpContext.Cache.Insert("RefreshQLinks", RefreshLinks, null, DateTime.MaxValue, TimeSpan.FromMinutes(300));
                    HttpContext.Cache.Insert("HomeEventPkey", cOneEvent.intEventPkey.ToString(), null, DateTime.MaxValue, TimeSpan.FromMinutes(300));
                }

                string strPartnerAlias = "Event Sponsor";
                DataTable LinksRefreshed = new DataTable();
                if (RefreshLinks != null)
                    LinksRefreshed = GetRefreshFilteredLinks(RefreshLinks, strPartnerAlias, 1);

                bool VisibleControlMain = (cLast.colEventControlpKeys.Count > 1);
                bool NextVisible = (cLast.intEventControlCurrentMVC < cLast.colEventControlpKeys.Count);
                bool PrevVisible = (cLast.intEventControlCurrentMVC > 1);
                return Json(new
                {
                    msg = "OK",
                    EventType = intEventType_PKey,
                    ImgURL = ImageUrl,
                    ToolTip = ToolTip,
                    EventFullName = EventFullName,
                    EventDetail = EventDetail,
                    ImgConfEnabled = ImgConfEnabled,
                    HomeLinkDisabled = HomeLinkDisabled,
                    NextVisible = NextVisible,
                    PrevVisible = PrevVisible,
                    SelectedEvent = cOneEvent.intEventPkey,
                    LinksRefreshed = Newtonsoft.Json.JsonConvert.SerializeObject(LinksRefreshed)
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {

            }
            return Json(new { msg = "Error occurred while updating event information" }, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult EnterEventInfo()
        {

            clsEvent cEvent = new clsEvent();
            bool Redirect = false, bPopUp = false, bVirtual = false;
            string lblEventUpdate = "", lblVirtualInstruction = "", RedirectURL = "";
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
            {
                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                int AccountID = Convert.ToInt32(User.Identity.Name);
                clsAccount cAccount = new clsAccount();
                cAccount.sqlConn = conn;
                cAccount.intAccount_PKey = AccountID;
                cAccount.LoadAccount();

                cEvent.intEvent_PKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
                cEvent.sqlConn = conn;
                cEvent.LoadEvent();
                DateTime d = clsEvent.getEventVenueTime();
                DateTime mindateTime = new DateTime(cEvent.dtStartDate.Year, cEvent.dtStartDate.Month, cEvent.dtStartDate.Day, 7, 0, 0);
                lblEventUpdate = cSettings.getText(clsSettings.Text_EventUpdate);

                cEvent.bMAGIUpdate = cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "IsMAGIUpdate");

                if ((cEvent.intEventStatus_PKey == clsEvent.STATUS_Pending && d < mindateTime && (!cAccount.bGlobalAdministrator || !cAccount.bStaffMember)))
                {
                    if (((cEvent.bEventOpenStaff && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "IsEventOpenForStaff")) && (cAccount.bStaffForCurEvent || cAccount.bGlobalAdministrator || cAccount.bStaffMember)))
                    {
                        bPopUp = cEvent.bMAGIUpdate;
                        Redirect = (!cEvent.bMAGIUpdate);
                    }
                    else if (cEvent.bEventOpenEventSponsors && clsEventOrganization.CheckExhibitor(cAccount.intParentOrganization_pKey, cLast.intActiveEventPkey) && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "IsEventOpenForEventSponsors"))
                    {
                        bPopUp = cEvent.bMAGIUpdate;
                        Redirect = (!cEvent.bMAGIUpdate);
                    }
                    else if (cEvent.bEventOpenSpeakers && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "IsEventOpenForSpeaker") && cAccount.intNumTimesSpeakingCurEvent > 0)
                    {
                        bPopUp = cEvent.bMAGIUpdate;
                        Redirect = (!cEvent.bMAGIUpdate);
                    }
                    else if (cEvent.bEventClosedAttendees && cAccount.bAttendeeAtCurrEvent && !(cAccount.bStaffForCurEvent || cAccount.bGlobalAdministrator || cAccount.bStaffMember))
                    {
                        bPopUp = cEvent.bMAGIUpdate;
                        Redirect = (!cEvent.bMAGIUpdate);
                    }
                    else
                    {
                        lblVirtualInstruction = cEvent.strBigButtonPopupText;
                        bVirtual = true;
                    }
                }
                else if (((cEvent.intEventStatus_PKey == clsEvent.STATUS_Completed || d.Date > cEvent.dtEndDate.Date) && (!cAccount.bGlobalAdministrator || !cAccount.bStaffMember)))
                {
                    lblVirtualInstruction = cEvent.strEventFullname + " has closed. " + (char)(34) + "See" + (char)(34) + " you next time!";
                    bVirtual = true;
                }
                else
                {
                    if (cEvent.bMAGIUpdate && ((cAccount.intAccount_PKey > 0 && cAccount.intRegistrationLevel_pKey == 1) || cAccount.bGlobalAdministrator || cAccount.bStaffMember))
                    {
                        bPopUp = true;
                    }
                    else
                        Redirect = true;
                }


            }
            else
            {
                Redirect = true;
                RedirectURL = "Login";
            }


            return Json(new { msg = "OK", Redirect = Redirect, bPopUp = bPopUp, bVirtual = bVirtual, lblEventUpdate = lblEventUpdate, lblVirtualInstruction = lblVirtualInstruction, RedirectURL = RedirectURL }, JsonRequestBehavior.AllowGet);
        }


        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public string SendNewPassword(string Email)
        {
            try
            {
                string strEMail = Email.Trim();
                if ((strEMail == "") | (!clsEmail.IsValidEmail(strEMail)))
                    return "No MAGI account found. Enter valid email address or user ID.";

                System.Data.SqlClient.SqlConnection Conn = new System.Data.SqlClient.SqlConnection(Session["sqlConn"].ToString());
                clsAccount c = new clsAccount();
                c.sqlConn = Conn;
                c.lblMsg = null;
                c.strEmail = strEMail;
                c.LoadAccountByEmail();
                if (c.intAccount_PKey > 0)
                {
                    clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                    clsAnnouncement Ann = clsAnnouncement.getAnnouncementInfo(Conn, null, clsAnnouncement.TEXT_PassReset);
                    string strSubject = Ann.strTitle;
                    string strContent = Ann.strHTMLText;
                    strContent = strContent.Replace("[Name]", c.strFirstname);
                    strContent = strContent.Replace("[Url]", clsSettings.APP_URL() + "/frmResetPassword.aspx?credential=" + clsUtility.Encrypt(c.strEmail));
                    strContent = strContent.Replace("[Support Email]", cSettings.strSupportEmail);
                    strContent = strContent.Replace("[LinkName]", "click here");
                    c.ReplaceReservedWords(strContent);
                    // --send email
                    clsEmail cEmail = new clsEmail();
                    cEmail.lblMsg = null;
                    cEmail.strSubjectLine = strSubject;
                    cEmail.strHTMLContent = strContent;
                    cEmail.intAnnouncement_pKey = clsAnnouncement.TEXT_PassReset;
                    if (!cEmail.SendEmailToAccount(c, bPlainText: false))
                        return "Error while sending email";
                    clsAccount.SetPasswordSentDate(c.intAccount_PKey, null);
                    cEmail = null;
                    return "OK";
                }
                else
                    return (clsUtility.getErrorMessage(224) + " (Code " + 224.ToString() + ")");
                c = null;
            }
            catch
            {
            }
            return "Error while sending passcode email";
        }

        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult Login(Identitylogger US)
        {
            if (US.Email != null || US.Password != null)
            {
                IdentityUser user = repository.GetUserbyNameAndPassword(US.Email, US.Password).Result;
                if (user == null)
                {
                    OwinError oobj = new OwinError();
                    oobj.error = "invalid_grant";
                    oobj.error_description = "The user name or password is incorrect.";
                    // return Ok(oobj);

                    ModelState.AddModelError("", oobj.error_description);
                }

                else if (!user.EmailConfirmed)
                {
                    OwinError oobj = new OwinError();
                    oobj.error = "invalid_grant";
                    oobj.error_description = "User did not confirm email.";
                    ModelState.AddModelError("", oobj.error_description);
                }
                else
                {
                    OwinResult obj = new OwinResult();
                    obj.access_token = Guid.NewGuid().ToString();
                    obj.token_type = "bearer";
                    obj.expires_in = "86399";
                    obj.userid = user.CustomerId;
                    obj.username = user.FirstName + " " + user.LastName;
                    obj.eventid = user.EventId;
                    obj.eventname = user.EventName;
                    obj.eventtypeid = user.EventTypeId;
                    obj.eventuserid = user.EventAccount_pkey;
                    obj.eventuserstatusid = user.ParticipationStatus_pKey;
                    obj.role = "KeyUser";
                    obj.issued = DateTime.Now.ToString("ddd, dd MMM yyyy HH:mm:ss GMT");
                    obj.expires = DateTime.Now.AddDays(1).ToString("ddd, dd MMM yyyy HH:mm:ss GMT");
                    // return Ok(obj);
                    ///Context.SaveChanges();
                }
                return View();
            }
            else
            {
                ModelState.AddModelError("", "");
                ViewBag.UserName   ="";
                if (Request.QueryString["Reg"] != null)
                    ViewBag.UserName   = clsUtility.Decrypt(Request.QueryString["Reg"]);
                else if (Request.Cookies["UserName"] != null)
                    ViewBag.UserName = Request.Cookies["UserName"].Value;

                return View();
            }
            //return View();
            // return RedirectToAction("Index", "Shop");
        }
        public ActionResult Log_Out()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }
        #region Login 

        [System.Web.Mvc.HttpPost]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public string VefifyLogin(string EmailID, string Password, bool RememberMe)
        {
            System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;
            int Account_pKey = 0;
            string resultMessage = repository.verifyLogin(EmailID, Password, RememberMe, request, out Account_pKey);
            if (resultMessage == "OK")
                LoginAccountQA(RememberMe, EmailID, Account_pKey);
            else if (resultMessage == "SendCode")
            {
                clsAccount cAccount = new clsAccount();
                cAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                int ResultInfo = cAccount.CheckUserLoginAccount(EmailID, ref Account_pKey);
                if (ResultInfo>0)
                {
                    string strPasscode = GenerateNumericPasscode(6);
                    string result = cAccount.InsertPasscode(EmailID, strPasscode);
                    if (result.Contains("invalid"))
                        return "No MAGI account found. Enter valid email address or user ID.";
                    resultMessage =   SendPasscode(EmailID, Account_pKey, strPasscode, ErrorMsg: "Sign-in unsuccessful. We have sent a passcode to the email address(es) in your profile.");
                }
            }
            return resultMessage;
        }

        [System.Web.Mvc.HttpPost]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public string VefifyLoginPasscode(string EmailID, string Passcode)
        {
            System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;
            int Account_pKey = 0;
            string resultMessage = repository.VerifyPasscodeLogin(EmailID, Passcode, request, out Account_pKey);
            if (resultMessage == "OK")
                LoginAccountQA(true, EmailID, Account_pKey);
            return resultMessage;
        }

        private void LoginAccountQA(bool RememberMe, string UserID, int Account_pKey)
        {
            if (Account_pKey>0)
            {
                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                clsAccount cAccount = new clsAccount();
                cAccount.intAccount_PKey = Account_pKey;
                cAccount.sqlConn=new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                if (!cAccount.LoadAccount())
                    return;

                cAccount.LogAuditMessage("Logged in", clsAudit.LOG_Login);
                Session["cAccount"] = cAccount;
                // --save id in cookie
                Response.Cookies["UserName"].Value = UserID;
                Response.Cookies["UserName"].Expires =  DateTime.Now.AddDays((RememberMe ? 30 : -1));
                // --initialize default event (this references lastlogin - so do this prior to setting last login date)
                cAccount.getDefaultEvent();
                // --log last login
                cAccount.SetLastLogin(cAccount.strChatID, cAccount.strChatSessionID);
                if (cAccount.intAccountStatus_pKey != 1)
                {
                    cAccount.intAccountStatus_pKey = 1;
                    cAccount.bGetJournal = true;
                    clsAccount.ListrackSubscription(cAccount, false, "");
                }
                if (!cLast.bRedirPending)
                    cLast.intEventSelector = -1;

                Session["cLastUsed"] = cLast;
            }

        }
        private void LogoutFromQA()
        {
            string SID = Session.SessionID;
            new SqlOperation().LogoutUpdateOnline(SID);

            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            clsSurrogate c = ((clsSurrogate)Session["Surrogate"]);
            clsAccount cAccount = new clsAccount();
            if (c  != null && c.hasSurrogate)
                c.Reset();

            cAccount.intAccount_PKey = Convert.ToInt32(User.Identity.Name);
            cAccount.sqlConn=new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cLast.strEditAccountVisitEmail = "";
            cLast.intShowSpkrIntrstAlrt = 0;
            cLast.intOriginatorAcct_pKey = 0;
            cAccount.LogAuditMessage("Logged out", clsAudit.LOG_LogOut);
            cAccount.Logout();

            if (Response.Cookies["Accountpkey"]!= null)
            {
                Response.Cookies["Accountpkey"].Value = null;
                Response.Cookies["Accountpkey"].Expires = DateTime.Now;
            }
        }
        public ActionResult Logout()
        {
            if (User.Identity.IsAuthenticated == true)
            {
                LogoutFromQA();
                FormsAuthentication.SignOut();
                //Removing Session
                Session.Abandon();
                Session.Clear();
                Session.RemoveAll();
                //Removing ASP.NET_SessionId Cookie
                if (Request.Cookies["ASP.NET_SessionId"] != null)
                {
                    Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                    Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-10);
                }

                if (Request.Cookies["AuthenticationToken"] != null)
                {
                    Response.Cookies["AuthenticationToken"].Value = string.Empty;
                    Response.Cookies["AuthenticationToken"].Expires = DateTime.Now.AddMonths(-10);
                }

            }
            return Redirect("/Home/Index");
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult SignInHelp()
        {
            string strSubject = "Need Help with Logging In";
            clsLastUsed cLast = (clsLastUsed)Session["cLastUsed"];
            cLast.colNotifications.Clear();
            clsOneNotify c = new clsOneNotify();
            c.strEmailAddress = ((clsSettings)Session["cSettings"]).strSupportEmail;
            cLast.colNotifications.Add(c);
            c = null;
            string s = "/SendEmail.aspx?S=" + HttpUtility.UrlEncode(strSubject);
            return Redirect(s);
        }

        [CustomizedAuthorize]
        public ActionResult LogoutCurrogate()
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsSurrogate c = ((clsSurrogate)Session["Surrogate"]);
            if (c != null &&  c.hasSurrogate)
            {
                FormsAuthentication.SignOut();
                clsAccount cAccount = ((clsAccount)Session["cAccount"]);
                cAccount.sqlConn=new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                cAccount.Logout();
                cAccount.intAccount_PKey = c.intOriginatorAcct_pKey;
                if (cAccount.LoadAccount())
                {
                    Session["cAccount"] = cAccount;
                    Session["Surrogate"] = c;
                }
                new SqlOperation().LoginByAccountID(c.intOriginatorAcct_pKey.ToString(), true);
                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                if (cLast.bActiveEvents)
                    return RedirectToAction("MyStaffPage", "MyMAGI");
            }
            return RedirectToAction("Index", "Home");
        }
        #endregion Login

        [HttpPost]
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult UpdateHelpIconLog(int type, string fileName, bool bAutoplay)
        {
            try
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                int Result = repository.HelpIcon_Save(data.Id, data.EventId, type, bAutoplay, fileName);
                if (Result > 0)
                    return Json(new { msg = "OK", LogResult = Result }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
            }
            return Json(new { msg = "Error While Updating Help Icon Log" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public string UpdateHelpLogTime(int ID)
        {
            try
            {
                string result = repository.HelpIcon_Update(ID);
                return result;
            }
            catch
            {
            }
            return "Error While Updating Help Icon Log Time";
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        public JsonResult VirtualDropdownSelected(int DropDownValue)
        {
            User_Login data = new User_Login();
            string evtString = Request.QueryString["EVT"] == null ? "" : "?EVT=" + Request.QueryString["EVT"].ToString();
            string URL = "", actionType = "Redirect";
            switch (DropDownValue)
            {
                case 0: URL = "/Virtual/EventOnCloud"; break;
                case 1: URL = "/Virtual/EducationCenter"; break;
                case 2: URL = "/ExhibitorDirectory"; break;
                case 3: URL = "/Virtual/NetworkingLounge"; break;
                case 4: URL = "/Virtual/ResourceSupportCenter"; break;
                //case 5:
                //    {
                //        string script = "function f(){ ExhibitHaalConfirm(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);";
                //        ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, true);
                //        break;
                //    }
                //case 6:
                //    {
                //        if (cAccount.intAccount_PKey > 0)
                //        {
                //            ctlTreasure.InitControl(cAccount.intAccount_PKey, cLast.intActiveEventPkey, cAccount.strFullName, "");
                //            string OpenThWindow = "function f(){ OpenTHWindow() ; Sys.Application.remove_load(f);}Sys.Application.add_load(f);";
                //            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", OpenThWindow, true);
                //        }
                //        else
                //        {
                //            clsUtility.InjectAlert(ScriptManager.GetCurrent(this.Page), this.Page, "No Treasure Hunt");
                //            return;
                //        }
                //        break;
                //    }

                case 7: URL = "/Virtual/ScheduledEvent"; break;

                case 8:
                    data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(((FormsIdentity)User.Identity).Ticket.UserData);
                    if (((clsSettings)Session["cSettings"]).bLogAttendeeDetails)
                        new clsAccount().Attendee_EnterEvent(clsEvent.EnterEvent_MyMAGI, data.Id, data.EventId);
                    URL = "/Home/Index"; break;
                case 9: break;
                case 10:
                    data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(((FormsIdentity)User.Identity).Ticket.UserData);
                    if (((clsSettings)Session["cSettings"]).bLogAttendeeDetails)
                        new clsAccount().Attendee_EnterEvent(clsEvent.EnterEvent_Exit, data.Id, data.EventId);
                    URL = "/Home/Index"; break;

                case 11: URL = "/CommunityShowcase"; break;
                case 12: URL = "/PhotoWall"; break;
                case 23: URL = "/PhotoWall"; break;
                case 14: URL = "/MyMAGI/MyConference"; break;
                case 15: URL = "/MyMAGI/MyOptions"; break;
                case 16: URL = "/MyMagi/MySchedule"; break;
                case 18: URL = "/MyNetworking"; break;
                case 19: URL = "/MyConsole"; break;
                case 20: URL = "/MyMAGI/MyConferenceBook"; break;
                case 21:
                    //    string script = "function f(){ opentheChat()(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);";
                    //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, true);
                    break;
                case 22: URL = "/SingleSession"; break;
                case 24: URL = "/MyReminders"; break;
                case 25: URL = "/Virtual/VirtualSession" + (Request.QueryString["ESPK"] == null ? "" : "?ESPK=" + Request.QueryString["ESPK"].ToString()); break;
                case 26: URL = "/MyMAGI/MyFAQs"; break;
                case 27: break;
                case 28:
                    clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                    if (cLast.MySessionLastURL == "")
                        URL = "/Virtual/VirtualSession?1=1&" + (Request.QueryString["ESPK"] == null ? "" : "?ESPK=" + Request.QueryString["ESPK"].ToString());
                    else
                        URL =  cLast.MySessionLastURL;
                    break;
                case 29: URL="/Virtual/ShowNews"; break;
                //case 27:
                //    {
                //        lblEventUpdatePopUPViurtual.Text = cSettings.getText(clsSettings.Text_EventUpdate);
                //        string Script = "function f(){OpenUpdatePopupVirtual(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);";
                //        ScriptManager.RegisterStartupScript(Page, Page.GetType(), "keyTypeOpenVirtual", Script, true);
                //        //clsUtility.PopupRadWindow(ScriptManager.GetCurrent(this.Page), this.Page, this.rwEventUpdateViurtual);
                //        break;
                //    }
                default: URL = "/Home/Index"; break;
            }
            return Json(new { msg = "OK", RedirectionUrl = URL, ActionType = actionType }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        [AjaxValidateAntiForgeryToken]
        public JsonResult GetNavigationInstructions()
        {
            try
            {
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                if (((clsSettings)Session["cSettings"]).bLogAttendeeDetails)
                    new clsAccount().Attendee_EnterEvent(clsEvent.EnterEvent_NavigationInstructions, data.Id, data.EventId);

                DataTable navInstrData = repository.GetNavigationInstructionInfo(data.EventId);
                if (navInstrData != null && navInstrData.Rows.Count > 0)
                {
                    string Content = clsReservedWords.ReplaceCurrent(null, clsSettings.ReplaceTermsGeneral(navInstrData.Rows[0]["SectionText"].ToString()));
                    return Json(new { msg = "OK", Content = Content }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { msg = "OK", Content = "Navigation Instructions" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { msg = "Error accessing text block data." }, JsonRequestBehavior.AllowGet);
            }
        }
        [CustomizedAuthorize]
        [AjaxValidateAntiForgeryToken]
        public JsonResult GetEventUpdatesContent()
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            string HtmlContent = cSettings.getText(clsSettings.Text_EventUpdate);
            return Json(new { msg = "OK", Content = HtmlContent }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult EventInfo()
        {
            return View();
        }

        public ActionResult Program()
        {
            return View();
        }

        public ActionResult Speakers()
        {
            return View();
        }

        public ActionResult BeASpeaker()
        {
            return View();
        }

        public ActionResult BeAEventSponsor()
        {
            return View();
        }

        public ActionResult EventSponsors()
        {
            return View();
        }

        public ActionResult EventContacts()
        {
            return View();
        }
        public PartialViewResult _PartialEventInfo()
        {
            User_Login data = new User_Login();
            int EvtPKey = 0;
            if (Request.QueryString["EVT"]!= null)
                int.TryParse(Request.QueryString["EVT"].ToString(), out EvtPKey);

            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            }
            else
            {
                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                data.EventId =(cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
                data.Id = 0;
            }
            if (EvtPKey>0)
                data.EventId =EvtPKey;

            DataTable EventInfo = repository.getDyamicEventSettings(data.EventId, "EventFullname,Venue_pkey,BannerMessage,intBackcolor");
            int VenuepKey = 0, intBackcolor = 0; string BannerMessage = "";
            if (EventInfo != null && EventInfo.Rows.Count > 0)
            {
                data.EventName = (EventInfo.Rows[0]["EventFullname"] == DBNull.Value) ? "" : EventInfo.Rows[0]["EventFullname"].ToString();
                BannerMessage = (EventInfo.Rows[0]["BannerMessage"] == DBNull.Value) ? "" : EventInfo.Rows[0]["BannerMessage"].ToString();
                VenuepKey =(EventInfo.Rows[0]["Venue_pkey"] == DBNull.Value) ? 0 : Convert.ToInt32(EventInfo.Rows[0]["Venue_pkey"]);
                intBackcolor =(EventInfo.Rows[0]["intBackcolor"] == DBNull.Value) ? 0 : Convert.ToInt32(EventInfo.Rows[0]["intBackcolor"]);
            }

            ViewBag.BannerLinkText = data.EventName;
            ViewBag.BannkerLinkNavigateUrl = "/Events/EventInfo?EVT=" + data.EventId.ToString();
            ViewBag.BackColor = "#"+ System.Drawing.Color.FromArgb(intBackcolor).Name;
            ViewBag.BackColorRGB =  System.Drawing.Color.FromArgb(intBackcolor).R + "," + System.Drawing.Color.FromArgb(intBackcolor).G + "," +System.Drawing.Color.FromArgb(intBackcolor).B;
            DataTable VenueImage = new SqlOperation().getVenueBannerImage(VenuepKey);
            string BannerImage = "", SmallBannerImage = "", strLocationCity = "";
            if (VenueImage != null && VenueImage.Rows.Count > 0)
            {
                BannerImage = (VenueImage.Rows[0]["VenueBanner"] == DBNull.Value) ? "" : VenueImage.Rows[0]["VenueBanner"].ToString();
                strLocationCity = (VenueImage.Rows[0]["LocationCity"] == DBNull.Value) ? "" : VenueImage.Rows[0]["LocationCity"].ToString();
                SmallBannerImage= (VenueImage.Rows[0]["VenueNarrowBanner"] == DBNull.Value) ? "" : VenueImage.Rows[0]["VenueNarrowBanner"].ToString();
            }

            ViewBag.BannerMessage= (string.IsNullOrEmpty(strLocationCity) ? "" : strLocationCity + ". ") +  BannerMessage;
            ViewBag.Banner_imgSrc = "/venuedocuments/" + SmallBannerImage;
            ViewBag.Banner_imgVisible = (SmallBannerImage != "");

            return PartialView();
        }

        public PartialViewResult _FillEventDropdown(bool EventDropdown = false, bool MyEventDropDown = false)
        {
            string intRegistraionLevelKey = "";
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            ViewData["lblConference"] = "";
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
            }
            else
            {
                data.EventId = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
                data.Id = 0;
            }
            ViewBag.EventDropdown = EventDropdown;
            ViewBag.MyEventDropDown = MyEventDropDown;
            DataTable EventInfo = repository.getDyamicEventSettings(data.EventId, "EventStatus_pKey");
            int intEventStatus_PKey = 0;
            if (EventInfo != null && EventInfo.Rows.Count > 0)
                intEventStatus_PKey = (EventInfo.Rows[0]["EventStatus_pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(EventInfo.Rows[0]["EventStatus_pKey"].ToString());

            bool bEvent = (intEventStatus_PKey != clsEvent.STATUS_Completed);
            ViewData["ddEventitemSelected"] =  cLast.intActiveEventPkey;
            int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistraionLevelKey);

            if (TempData["ddEvent"] != null && TempData["ddMyEvent"] != null)
                return PartialView();

            DataTable dt = new DataTable();
            if (TempData["dataEventsTable"]== null)
                dt = new SqlOperation().FillEventDropdown(data.Id, data.EventId, (bEvent && (intAttendeeStatus == 1 || intAttendeeStatus == 3)), false);
            else
                dt = (DataTable)TempData["dataEventsTable"];
            ViewData["ddEventVisible"]=false;
            ViewData["lblConferenceVisible"]=false;
            ViewData["ddEventVisible"]=false;
            ViewData["lblConferenceVisible"]=((ViewData["lblConference"].ToString()=="") ? false : true);

            if (!TempData.ContainsKey("ddEvent"))
                TempData.Add("ddEvent", null);
            if (!TempData.ContainsKey("ddMyEvent"))
                TempData.Add("ddMyEvent", null);

            if (!TempData.ContainsKey("dataEventsTable"))
                TempData.Add("dataEventsTable", dt);
            else
                TempData["dataEventsTable"] =dt;

            if (dt != null && dt.Rows.Count>0)
            {
                bool aPresent = true, bPresent = true, addMe = false;
                DateTime dCalTime = clsEvent.getEventVenueTime();
                DataRow[] results = dt.Select("pKey = " + data.EventId.ToString() + " AND EventStatus_pKey = 1");
                if (results.Length==0)
                    aPresent =false;
                else
                {
                    ViewData["lblConference"] = Convert.ToString(results[0].ItemArray[1]);
                    ViewData["lblConferenceToolTip"] = Convert.ToString(results[0].ItemArray[2]);
                }
                results = dt.Select("pKey = " + data.EventId.ToString()  + ((data.GlobalAdmin == true) ? "" : " AND Account_pKey > 0"));
                if (results.Length==0)
                {
                    results = dt.Select("pKey = " + data.EventId.ToString()  + ((data.GlobalAdmin == true) ? "" : " AND Account_pKey > 0"));
                    if (results.Length > 0)
                        cLast.intEventSelector = Convert.ToInt32(results[0][0]);
                    else
                        bPresent = false;
                }

                DataTable ddEvent = new DataTable();
                ddEvent.Columns.Add("strText", typeof(string));
                ddEvent.Columns.Add("strDataText", typeof(string));
                ddEvent.Columns.Add("pKey", typeof(string));

                DataTable ddMyEvent = new DataTable();
                ddMyEvent.Columns.Add("strText", typeof(string));
                ddMyEvent.Columns.Add("strDataText", typeof(string));
                ddMyEvent.Columns.Add("pKey", typeof(string));

                foreach (DataRow dRow in dt.Rows)
                {
                    if (Convert.ToInt32(dRow[3].ToString()) == 1 && Convert.ToBoolean(dRow[9].ToString()) == false)
                    {
                        if (cLast.intActiveEventPkey ==Convert.ToInt32(dRow[0].ToString()) || aPresent ==false)
                        {
                            ViewData["lblConference"]  = dRow[1];
                            ViewData["lblConferenceToolTip"]  = dRow[2];
                            aPresent = true;
                        }
                        ddEvent.Rows.Add(dRow[2], dRow["EventID"], dRow[0]);
                    }
                    addMe =false;
                    if ((Convert.ToInt32(dRow[3]) ==1 && Convert.ToInt32(dRow[4]) >0) || data.GlobalAdmin)
                        addMe = true;
                    else if (Convert.ToInt32(dRow[4]) >0 && Convert.ToBoolean(dRow[6]) == true)
                    {
                        DateTime dateTime1 = Convert.ToDateTime(dRow[8].ToString());
                        DateTime dateTime2 = dateTime1.AddDays(Convert.ToInt32(dRow[7]) + 1);
                        if (dateTime1 <= dCalTime && dCalTime < dateTime2)
                        {
                            addMe=true;
                            bPresent =false;
                            cLast.bSpeacialAccount = true;
                        }
                    }
                    if (addMe)
                    {
                        if (cLast.intEventSelector  ==Convert.ToInt32(dRow[0]) && !bPresent)
                        {
                            ViewData["lblConference"]  = dRow[1];
                            ViewData["lblConferenceToolTip"]  = dRow[2];
                            cLast.intEventSelector  =Convert.ToInt32(dRow[0]);
                            bPresent = true;
                        }
                        ddMyEvent.Rows.Add(dRow[2], dRow["EventID"], dRow[0]);
                    }
                }

                if (ddEvent.Rows.Count>1)
                {
                    ViewData["lblConferenceVisible"]=true;
                    ViewData["ddEventVisible"]=true;
                }
                else if (ddMyEvent.Rows.Count>0)
                {
                    ViewData["lblConferenceVisible"]=true;
                    ViewData["ddEventVisible"]=false;
                }
                else
                {
                    ViewData["lblConferenceVisible"]=((ViewData["lblConference"].ToString()=="") ? false : true);
                    ViewData["ddEventVisible"]=false;
                }

                if (ddMyEvent.Rows.Count>1)
                {
                    ViewData["lblConferenceVisible"]=true;
                    ViewData["ddMyEventVisible"]=true;
                }
                else if (ddMyEvent.Rows.Count>0)
                {
                    ViewData["lblConferenceVisible"]=true;
                    ViewData["ddMyEventVisible"]=false;
                }
                else
                {
                    ViewData["lblConferenceVisible"]=((ViewData["lblConference"].ToString()=="") ? false : true);
                    ViewData["ddMyEventVisible"]=false;
                }

                TempData["ddEvent"] = ddEvent;
                TempData["ddMyEvent"] = ddMyEvent;
            }
            return PartialView();
        }
        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public string EventChangeByDropDown(int EventValue)
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
            }
            else
            {
                data.EventId = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
                data.Id = 0;
            }

            if (EventValue == cSettings.intPrimaryEvent_pkey)
                cSettings.intHighlightedEvent = 0;
            else if (EventValue == cSettings.intNextEvent_pKey)
                cSettings.intHighlightedEvent = 1;
            else if (EventValue == cSettings.intPriorEvent_pKey)
                cSettings.intHighlightedEvent = -1;
            else if (EventValue == cSettings.intNext2Event_pKey)
                cSettings.intHighlightedEvent = 2;
            else
                cSettings.intHighlightedEvent = EventValue;

            clsAccount cAccount = new clsAccount();
            cAccount.intAccount_PKey = data.Id;
            cAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cAccount.SetLastViewedEvent(EventValue);

            int intEventType = 0;
            string strEvt = "";
            DataTable dt = repository.GetEventRelatedData(EventValue);
            if (dt!=null && dt.Rows.Count>0)
            {
                strEvt = (dt.Rows[0]["EventID"] != System.DBNull.Value) ? dt.Rows[0]["EventID"].ToString() : "";
                intEventType = (dt.Rows[0]["EventType_PKey"] != System.DBNull.Value) ? Convert.ToInt32(dt.Rows[0]["EventType_PKey"]) : 0;
            }

            cLast.strActiveEvent = strEvt;
            cLast.intActiveEventPkey = EventValue;
            cLast.intEventType_PKey = intEventType;
            cLast.interestedEventID_Management = EventValue;
            cLast.strActiveRoles = cAccount.getRolesForEvent(cLast.intActiveEventPkey);
            cLast.intGLEvent = EventValue;
            if (cLast.strActiveRoles == "")
                cLast.strActiveRoles = "None";

            Session["cLastUsed"] = cLast;
            Session["cSettings"] = cSettings;
            repository.UpdateAuthEvent();

            clsFormList cFormList = ((clsFormList)Session["cFormlist"]);
            cFormList.ResetRepeaterList();

            return "OK";
        }

        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult LoadImageData()
        {
            try
            {
                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                User_Login data = new User_Login();
                int EvtPKey = 0;
                if (Request.QueryString["EVT"]!= null)
                    int.TryParse(Request.QueryString["EVT"].ToString(), out EvtPKey);

                if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
                {
                    FormsIdentity identity = (FormsIdentity)User.Identity;
                    data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                }
                else
                {

                    data.EventId =(cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
                    data.Id = 0;
                }

                if (EvtPKey>0)
                    data.EventId =EvtPKey;

                DataTable EventInfo = repository.getDyamicEventSettings(data.EventId,
                    "EventFullname,Venue_pkey,BannerMessage,intBackcolor,StandardTime_Pkey," +
                    "PublicPageStartDate,PublicPageEndDate,LeftBlockImage,MiddleBlockImage," +
                    "RightBlcokImage,StartDate,EndDate");

                int VenuepKey = 0, intBackcolor = 0, StandardTime_pKey = 0; string BannerMessage = "", sLeftBlockImage = "", sMiddleBlockImage = "", sRightBlcokImage = "";
                DateTime dtPublicPageStartDate = System.DateTime.Now, dtPublicPageEndDate = System.DateTime.Now, dtStartDate = System.DateTime.Now, dtEndDate = System.DateTime.Now;
                if (EventInfo != null && EventInfo.Rows.Count > 0)
                {
                    data.EventName = (EventInfo.Rows[0]["EventFullname"] == DBNull.Value) ? "" : EventInfo.Rows[0]["EventFullname"].ToString();
                    BannerMessage = (EventInfo.Rows[0]["BannerMessage"] == DBNull.Value) ? "" : EventInfo.Rows[0]["BannerMessage"].ToString();
                    sLeftBlockImage = (EventInfo.Rows[0]["LeftBlockImage"] == DBNull.Value) ? "" : EventInfo.Rows[0]["LeftBlockImage"].ToString();
                    sMiddleBlockImage = (EventInfo.Rows[0]["MiddleBlockImage"] == DBNull.Value) ? "" : EventInfo.Rows[0]["MiddleBlockImage"].ToString();
                    sRightBlcokImage = (EventInfo.Rows[0]["RightBlcokImage"] == DBNull.Value) ? "" : EventInfo.Rows[0]["RightBlcokImage"].ToString();
                    VenuepKey =(EventInfo.Rows[0]["Venue_pkey"] == DBNull.Value) ? 0 : Convert.ToInt32(EventInfo.Rows[0]["Venue_pkey"]);
                    intBackcolor =(EventInfo.Rows[0]["intBackcolor"] == DBNull.Value) ? 0 : Convert.ToInt32(EventInfo.Rows[0]["intBackcolor"]);
                    StandardTime_pKey=(EventInfo.Rows[0]["StandardTime_Pkey"] == DBNull.Value) ? 0 : Convert.ToInt32(EventInfo.Rows[0]["StandardTime_Pkey"]);
                    dtPublicPageStartDate =(EventInfo.Rows[0]["PublicPageStartDate"] == DBNull.Value) ? System.DateTime.Now : Convert.ToDateTime(EventInfo.Rows[0]["PublicPageStartDate"]);
                    dtPublicPageEndDate =(EventInfo.Rows[0]["PublicPageEndDate"] == DBNull.Value) ? System.DateTime.Now : Convert.ToDateTime(EventInfo.Rows[0]["PublicPageEndDate"]);
                    dtStartDate =(EventInfo.Rows[0]["StartDate"] == DBNull.Value) ? System.DateTime.Now : Convert.ToDateTime(EventInfo.Rows[0]["StartDate"]);
                    dtEndDate =(EventInfo.Rows[0]["EndDate"] == DBNull.Value) ? System.DateTime.Now : Convert.ToDateTime(EventInfo.Rows[0]["EndDate"]);
                }
                string standardRegion = new SqlOperation().getStandardRegion(StandardTime_pKey);
                string BannerLinkText = data.EventName;
                string BannkerLinkNavigateUrl = "/Events/EventInfo?EVT=" + data.EventId.ToString();
                string BackColorRGB = System.Drawing.Color.FromArgb(intBackcolor).R + "," + System.Drawing.Color.FromArgb(intBackcolor).G + "," +System.Drawing.Color.FromArgb(intBackcolor).B;
                DataTable VenueImage = new SqlOperation().getVenueBannerImage(VenuepKey);
                string BannerImage = "", SmallBannerImage = "", strLocationCity = "", sVenueID = "";
                if (VenueImage != null && VenueImage.Rows.Count > 0)
                {
                    BannerImage = (VenueImage.Rows[0]["VenueBanner"] == DBNull.Value) ? "" : VenueImage.Rows[0]["VenueBanner"].ToString();
                    strLocationCity = (VenueImage.Rows[0]["LocationCity"] == DBNull.Value) ? "" : VenueImage.Rows[0]["LocationCity"].ToString();
                    SmallBannerImage= (VenueImage.Rows[0]["VenueNarrowBanner"] == DBNull.Value) ? "" : VenueImage.Rows[0]["VenueNarrowBanner"].ToString();
                    sVenueID = (VenueImage.Rows[0]["VenueID"] == DBNull.Value) ? "" : VenueImage.Rows[0]["VenueID"].ToString();
                }

                BannerMessage = (string.IsNullOrEmpty(strLocationCity) ? "" : strLocationCity + ". ") +  BannerMessage;
                SmallBannerImage = "/venuedocuments/" + SmallBannerImage;
                bool Banner_imgVisible = (SmallBannerImage != "");
                string lblcnfName = "";
                clsEvent cEvent = new clsEvent();
                cEvent.intEvent_PKey = data.EventId;
                cEvent.strStandardRegion = standardRegion;
                DateTime CurrentDate = clsEvent.getEventVenueTime();
                bool bShowImageLink = (CurrentDate >= dtPublicPageStartDate && CurrentDate <= dtPublicPageEndDate.AddDays(1).Date);
                bool bShowImage = true;
                if (System.Configuration.ConfigurationManager.AppSettings["QAMode"] == "1")
                {
                    string strMiddle = Server.MapPath("~/images/Homepage/" + data.EventId.ToString() + "MiddleBlock.jpg");
                    bShowImage = (System.IO.File.Exists(strMiddle));
                }
                Dictionary<int, clsImg> dct = ((Dictionary<int, clsImg>)System.Web.HttpContext.Current.Application["cImages"]);
                string imgLogo = "/Images/HomePage/magilogo.jpg",
                       MagiMenuImage = "/images/menu/confmenu1light.png",
                       EventMenuImage = "/images/menu/confmenu2light.png",
                       ResourcesMenuImage = "/images/menu/Resources.jpg",
                       MyMagiMenuImage = "/images/menu/confmenu5light.png";



                if (dct.ContainsKey(clsImages.IMG_1))
                    imgLogo = dct[clsImages.IMG_1].strPath.Replace("~", "");

                if (dct.ContainsKey(clsImages.IMG_13))
                    MagiMenuImage = dct[clsImages.IMG_13].strPath.Replace("~", "");

                if (dct.ContainsKey(clsImages.IMG_14))
                    EventMenuImage = dct[clsImages.IMG_14].strPath.Replace("~", "");

                if (dct.ContainsKey(clsImages.IMG_15))
                    ResourcesMenuImage = dct[clsImages.IMG_15].strPath.Replace("~", "");

                if (dct.ContainsKey(clsImages.IMG_16))
                    MyMagiMenuImage = dct[clsImages.IMG_16].strPath.Replace("~", "");


                string imgLeftSrc = "", imgMiddleSrc = "", imgRightSrc = "", lblcnfLocation = "", lblcnfDate = "";
                bool imgRightVisible = true, EventInfoVisible = false;

                if (sLeftBlockImage != "" && sMiddleBlockImage != "")
                {
                    imgLeftSrc = "/images/BannerImages/" + sLeftBlockImage;
                    imgMiddleSrc = "/images/BannerImages/" + sMiddleBlockImage;
                    if (sRightBlcokImage != "")
                        imgRightSrc= "/images/BannerImages/" + sRightBlcokImage;
                    else
                        imgRightVisible = false;
                }
                else if (bShowImage && SmallBannerImage != "")
                {
                    imgLeftSrc = "/venuedocuments/" + SmallBannerImage;
                    imgMiddleSrc = "/images/Homepage/" +  data.EventId.ToString() + "MiddleBlock.jpg";
                    imgRightSrc = "/images/Homepage/" +data.EventId.ToString()  + "RightBlock.jpg";
                }
                else
                {
                    EventInfoVisible= true;
                    if (data.EventId >0)
                    {
                        lblcnfName = data.EventName.Replace("MAGI's Clinical Research Conference - ", " ").ToUpper();
                        lblcnfLocation =  sVenueID;
                        lblcnfDate =dtStartDate.ToString("MMMM") + " " + dtStartDate.Day.ToString() + "-" +
                                    ((dtEndDate.ToString("MMMM") != dtStartDate.ToString("MMMM")) ? (cEvent.dtEndDate.ToString("MMMM") + " ") : "") +
                                    dtEndDate.Day.ToString() + ", " + cEvent.dtEndDate.Year.ToString();
                    }
                    imgLeftSrc = "/images/Homepage/LeftBlock.gif";
                    imgMiddleSrc = "/images/Homepage/MiddleBlock.jpg";
                    if (dct.ContainsKey(clsImages.IMG_17))
                        imgMiddleSrc = dct[clsImages.IMG_17].strPath.Replace("~", "");
                    imgRightSrc = "/images/Homepage/RightBlock.jpg";
                    imgRightVisible = false;
                    bShowImageLink =false;
                }

                return Json(new
                {
                    msg = "OK",
                    bannerMessage = BannerMessage,
                    smallBannerImage = SmallBannerImage,
                    backColorRGB = BackColorRGB,
                    bannerLinkText = BannerLinkText,
                    bannkerLinkNavigateUrl = BannkerLinkNavigateUrl,
                    tblimageVisible = cSettings.bShowHeaderBanner,
                    enableLink = bShowImageLink,
                    imgRightVisible = imgRightVisible,
                    imgLeftSrc = imgLeftSrc,
                    imgMiddleSrc = imgMiddleSrc,
                    imgRightSrc = imgRightSrc,
                    imgLogo = imgLogo,
                    MagiMenuImage = MagiMenuImage,
                    EventMenuImage = EventMenuImage,
                    ResourcesMenuImage = ResourcesMenuImage,
                    MyMagiMenuImage = MyMagiMenuImage,
                    bShowImage = bShowImage,
                    EventInfoVisible = EventInfoVisible,
                    lblcnfName = lblcnfName,
                    lblcnfLocation = lblcnfLocation,
                    lblcnfDate = lblcnfDate
                }, JsonRequestBehavior.AllowGet);

            }
            catch
            {

            }
            return Json(new { msg = "Error Occurred While Loading Images", JsonRequestBehavior.AllowGet });
        }

        public RedirectResult Switch(string URLInfo)
        {
            return Redirect("/Switch?LastPage_URL=" + URLInfo);
        }
    }
}

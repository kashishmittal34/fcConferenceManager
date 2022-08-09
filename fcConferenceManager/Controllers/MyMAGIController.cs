using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using Aspose.Pdf;
using fcConferenceManager.Models;
using MAGI_API.Models;
using Newtonsoft.Json;
using System.Collections;

namespace fcConferenceManager.Controllers
{
    [CheckActiveEventAttribute]
    public class MyMAGIController : Controller
    {
        // GET: MyMAGI
        DBAccessLayer dba = new DBAccessLayer();
        static SqlOperation repository = new SqlOperation();
        public ActionResult Index()
        {
            return View();
        }
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

        #region MySession
        private System.Data.DataTable LoadSessionData(int id, int eventpKey, string Audience = null, string Days = null, string Tracks = null, string Levels = null, string Topics = null, string CertpKey = null)
        {
            try
            {
                System.Data.DataTable SessionData = null;
                SessionData = repository.getMySessionsDataByID(id, eventpKey, true, Audience, Days, Tracks, Levels, Topics, CertpKey);
                return SessionData;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        private void CheckStringTitle(int ChargeType, int intAccount_pKey, int EventpKey, int EventAccountpKey, string CalcType)
        {
            string title = "Attendance";
            switch (ChargeType)
            {
                case clsPrice.CHARGE_CME: title = "CME"; break;
                case clsPrice.CHARGE_CNE: title = "CNE"; break;
                case clsPrice.CHARGE_CLE: title = "CLE Backup"; break;
                case clsPrice.CHARGE_CCB: title = "CCB"; break;
                case clsPrice.CHARGE_CMEMD: title = "CME-MD"; break;
                case clsPrice.CHARGE_CRCPExam: title = "CRCP"; break;
                case clsPrice.CHARGE_ConflictResolutionModes: title = "CRM"; break;
                case clsPrice.CHARGE_CLECompactStates: title = "CLE (WV)"; break;
                case clsPrice.CHARGE_CLEPA: title = "CLE (PA)"; break;
            }
            ViewBag.TextContactHours = ((CalcType == "Single") ? title + " Contact Hours:" : (title != "Attendance") ? " Contact Hours: " : "Attendance Certificate Hours: ");
            if (!string.IsNullOrEmpty(title))
            {
                int CertpKey = repository.FindCertPKey(title);
                ViewBag.lblContactHrs = repository.RecomputeOneAccount(EventAccountpKey, intAccount_pKey, CertpKey, EventpKey, title);
            }
        }
        private GenericListItem UpdateItemInDropDown(int ChargeType)
        {
            GenericListItem charges = new GenericListItem();
            switch (ChargeType)
            {
                case clsPrice.CHARGE_CME: charges.strText = "CME"; break;
                case clsPrice.CHARGE_CNE: charges.strText = "CNE"; break;
                case clsPrice.CHARGE_CLE: charges.strText = "CLE Backup"; break;
                case clsPrice.CHARGE_CCB: charges.strText = "CCB"; break;
                case clsPrice.CHARGE_CMEMD: charges.strText = "CME-MD"; break;
                case clsPrice.CHARGE_CRCPExam: charges.strText = "CRCP"; break;
                case clsPrice.CHARGE_ConflictResolutionModes: charges.strText = "CRM"; break;
                case clsPrice.CHARGE_CLECompactStates: charges.strText = "CLE (WV)"; break;
                case clsPrice.CHARGE_CLEPA: charges.strText = "CLE (PA)"; break;
            }
            charges.pKey = ChargeType;
            return charges;
        }
        private List<GenericListItem> RefreshContactHours(int EventPKey, int AccountPKey, int EventAccountpKey)
        {
            ViewBag.TextContactHours = "Attendance Certificate Hours: ";
            ViewBag.TypeSingle = "Multiple";
            ViewBag.lblContactHrs = "0.0";
            List<GenericListItem> chargeList = new List<GenericListItem>();
            try
            {
                System.Data.DataTable dt = repository.FetchSessionFilters(EventPKey, 7);
                if (dt != null)
                {
                    int ChargeTypePKey = 0;
                    ViewBag.ContactHourPkey = 0;
                    switch (dt.Rows.Count)
                    {
                        case 0:
                            ViewBag.lblContactHrs = repository.UpdateContactHours(EventPKey, AccountPKey);
                            break;
                        case 1:
                            ViewBag.TypeSingle = "Single";
                            if (dt.Rows[0]["chargeType_pkey"] != System.DBNull.Value)
                            {
                                int.TryParse(dt.Rows[0]["chargeType_pkey"].ToString(), out ChargeTypePKey);
                                ViewBag.ContactHourPkey = ChargeTypePKey;
                                GenericListItem itemData = UpdateItemInDropDown(ChargeTypePKey);
                                if (itemData.strText != "")
                                    chargeList.Add(itemData);
                                CheckStringTitle(ChargeTypePKey, AccountPKey, EventPKey, EventAccountpKey, "Single");
                            }
                            break;
                        default:
                            ViewBag.TypeSingle = "Multiple";
                            if (dt.Rows[0]["chargeType_pkey"] != System.DBNull.Value)
                            {
                                int.TryParse(dt.Rows[0]["chargeType_pkey"].ToString(), out ChargeTypePKey);
                                ViewBag.ContactHourPkey = ChargeTypePKey;
                                foreach (System.Data.DataRow dr in dt.Rows)
                                {
                                    if (dr["chargeType_pkey"] != System.DBNull.Value)
                                    {
                                        GenericListItem item = UpdateItemInDropDown(Convert.ToInt32(dr["chargeType_pkey"].ToString()));
                                        if (item.strText != "")
                                            chargeList.Add(item);
                                    }
                                }
                                CheckStringTitle(ChargeTypePKey, AccountPKey, EventPKey, EventAccountpKey, "");
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return chargeList;
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult UpdateContactHours(string typeSingle, string contHourpKey, string CertType)
        {
            ViewBag.TextContactHours = " Contact Hours:";
            ViewBag.lblContactHrs = 0.0;
            if (User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                if (CertType == "0" || CertType == "")
                {
                    if (string.IsNullOrEmpty(contHourpKey))
                    {
                        ViewBag.lblContactHrs = repository.UpdateContactHours(data.EventId, data.Id);
                        ViewBag.TextContactHours = "Attendance Certificate Hours: ";
                    }
                    else if (typeSingle == "Single")
                    {
                        CheckStringTitle(Convert.ToInt32(contHourpKey), data.Id, data.EventId, data.EventAccount_pkey, "Single");
                    }
                    else
                    {
                        CheckStringTitle(Convert.ToInt32(contHourpKey), data.Id, data.EventId, data.EventAccount_pkey, typeSingle);
                    }
                }
                else
                {
                    CheckStringTitle(Convert.ToInt32(CertType), data.Id, data.EventId, data.EventAccount_pkey, "");
                }

                return Json(new { msg = "OK", lblContactHrs = ViewBag.lblContactHrs, TextContactHours = ViewBag.TextContactHours }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { msg = "Error", lblContactHrs = ViewBag.lblContactHrs, TextContactHours = ViewBag.TextContactHours }, JsonRequestBehavior.AllowGet);
            }
        }
        private void LoadSessionListData(int EventPKey, bool cbDay, bool cbTrack, out MySessionsPage SessionData)
        {
            SessionData = new MySessionsPage();
            SessionData.ddPrimaryAudience = repository.FetchSessionFilters(EventPKey, 1);
            SessionData.cbTracks = ((cbTrack) ? repository.FetchSessionFilters(EventPKey, 4) : null);
            SessionData.cbTopics = repository.FetchSessionFilters(EventPKey, 3);
            SessionData.cbLevels = repository.FetchSessionFilters(EventPKey, 5);
            SessionData.cbDays = ((cbDay) ? repository.FetchSessionFilters(EventPKey, 6) : null);
            SessionData.ddCertificateHours = repository.FetchSessionFilters(EventPKey, 2);   //Contact Hours Filters 
        }
        private void LoadRegistrationQuestions(User_Login data, int intAttendeeStatus, bool bShowSurveyQuestion, bool bSponsor, string intRegistrationLevel_pKey)
        {
            ViewBag.lblRegText = "";
            ViewBag.OpenSurveyRadWindow=false;
            if (bShowSurveyQuestion && (intRegistrationLevel_pKey != clsEventAccount.REGISTRATION_SingleSessionOnly.ToString()) && !(data.StaffMember || data.GlobalAdmin || bSponsor) && intAttendeeStatus == 1)
            {
                bool Registered = repository.GetIsRegistered(data.EventId, data.Id);
                int CheckedCount = repository.CheckFeedbackClickCount(data.EventId, data.Id);
                if (CheckedCount < 3 && (data.Id > 0 && Registered && (bSponsor == false || (data.GlobalAdmin || data.StaffMember) == false)))
                {
                    bool show = repository.RegistrationFeedback(data.Id, data.EventId);
                    if (show)
                    {
                        ViewBag.lblRegText = "Please answer the following quick questions that we ask all event participants.<br/>";
                        ViewBag.OpenSurveyRadWindow=true;
                    }
                }
            }
        }

        [CustomizedAuthorize]
        public ActionResult MySession()
        {

            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            ViewData["Title"] = "CreateMySchedule";
            MySessionsPage SessionData = null;
            ViewBag.SelectedDropDown = 0;
            if (User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                int EventPKey = data.EventId;  // User for Changed Event Values.... : ((clsLastUsed)Session["cLastUsed"]).intActiveEventPkey; 
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.LblTitle = data.EventName + ": My Schedule - Create";
                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;


                string intRegistrationLevel_pKey = "";
                int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, EventPKey, ref intRegistrationLevel_pKey);
                bool bSingleTrackEvent = (data.EventTypeId == 4);

                ViewBag.SelectAll_Visible = (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDay.ToString() || intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayPhysical.ToString()|| intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayVirtual.ToString() || intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_SingleTrackOnly.ToString() || bSingleTrackEvent);
                ViewBag.DeSelectAll_Visible = (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDay.ToString()|| intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayPhysical.ToString() || intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayVirtual.ToString() || intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_SingleTrackOnly.ToString() || bSingleTrackEvent);
                bool cbDay = false, cbTrack = false;
                cbDay = (!(intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDay.ToString() || intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayPhysical.ToString() || intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayVirtual.ToString()));
                cbTrack = (!(intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_SingleTrackOnly.ToString()));

                LoadSessionListData(EventPKey, cbDay, cbTrack, out SessionData);
                ViewData["ListData"] = RefreshContactHours(EventPKey, data.Id, data.EventAccount_pkey);
                ViewBag.IsHybrid= (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_FullReg.ToString());

                SessionData.SessionList = LoadSessionData(data.Id, data.EventId, "");
                int EventStatusPKey = 0; bool bShowRemindersPanel = false; bool bEnableChatPanel = false; DateTime endDate = DateTime.Now;
                System.Data.DataTable EventSettings = repository.getEventSettingsForSessionByID(data.EventId);
                int AttSCH = 0; double dblAccountAmount = 0;
                bool bSch = false, bShowSurveyQuestion = false;
                DataTable EventFeatures = repository.getDyamicEventSettings(data.EventId, "ISNULL(FeatureAccess,'') as FeatureAccess,ISNULL(AttendeeAccess,'') as AttendeeAccess,ISNULL(ShowSurveyQuestion,'0') as ShowSurveyQuestion");
                if (EventFeatures != null && EventFeatures.Rows.Count > 0)
                {
                    string FeatureAccess = EventFeatures.Rows[0]["FeatureAccess"].ToString();
                    string AttendeeAccess = EventFeatures.Rows[0]["AttendeeAccess"].ToString();
                    if (!string.IsNullOrEmpty(AttendeeAccess))
                        AttSCH = Convert.ToInt32(AttendeeAccess.Split(',')[0]);
                    if (!string.IsNullOrEmpty(FeatureAccess))
                        bSch = (Convert.ToInt32(FeatureAccess.Split(',')[0]) == 1);

                    bShowSurveyQuestion = (EventFeatures.Rows[0]["ShowSurveyQuestion"]!= System.DBNull.Value) ? Convert.ToBoolean(EventFeatures.Rows[0]["ShowSurveyQuestion"]) : false;
                }
                ViewBag.NotSchedule = false;
                ViewBag.ShowAlert = false;
                ViewBag.PayNow = false;
                ViewData["NotAttendeeText"] = "You are not Attendee of this event.";
                dblAccountAmount = repository.GetAccountBalance(data.EventId, data.Id);
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                if (!data.GlobalAdmin)
                {
                    if (bSch)
                    {
                        if (!(intAttendeeStatus == clsEventAccount.PARTICIPATION_Attending) && AttSCH == clsEvent.intAttAttending)
                        {
                            ViewBag.NotSchedule = true;
                            ViewBag.ShowAlert = true;
                        }
                        if ((AttSCH == clsEvent.intAttPaid || AttSCH == clsEvent.intAttAsIfPaid) && dblAccountAmount <= cSettings.intAttAccessBal)
                        {
                            ViewBag.PayNow = true;
                            ViewData["NotAttendeeText"] = "To access this feature, please pay your balance due of " + String.Format("{0:c}", dblAccountAmount);
                        }
                    }
                    else
                    {
                        ViewData["NotAttendeeText"] = "This feature is not yet available.";
                        ViewBag.NotSchedule = true;
                    }
                }
                if (ViewBag.NotSchedule)
                    return View(SessionData);

                if (EventSettings != null && EventSettings.Rows.Count > 0)
                {
                    ViewBag.StandardRegion = EventSettings.Rows[0]["StandardRegion"];
                    ViewBag.StandardRegionCode = EventSettings.Rows[0]["StandardRegionCode"];

                    int.TryParse(EventSettings.Rows[0]["EventStatuspKey"].ToString(), out EventStatusPKey);
                    bool.TryParse(EventSettings.Rows[0]["ShowRemindersPanel"].ToString(), out bShowRemindersPanel);
                    bool.TryParse(EventSettings.Rows[0]["IsChatPanelOn"].ToString(), out bEnableChatPanel);
                    DateTime.TryParse(EventSettings.Rows[0]["EvtEndTime"].ToString(), out endDate);
                }
                clsEvent cEvent = new clsEvent();                
                cEvent.sqlConn = new SqlConnection(ReadConnectionString());
                
                if (bEnableChatPanel)
                {
                    bEnableChatPanel = cEvent.CheckValiditityOfModule(data.EventId, "IsChatPanelOn");
                }

                ViewBag.ShowTimer = bShowRemindersPanel;
                ViewBag.leftPanel_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
                ViewBag.VirtualDropdown_Visible = ViewBag.leftPanel_Visible;
                int intRegistrationLevelpKey = 0;
                if (!string.IsNullOrEmpty(intRegistrationLevel_pKey))
                    intRegistrationLevelpKey = Convert.ToInt32(intRegistrationLevel_pKey);

                bool bEvent = (EventStatusPKey != clsEvent.STATUS_Completed), showPanelReminders = false, Notificationtips = false;

                if (bEvent && (intAttendeeStatus == 1 || intAttendeeStatus == 3) && (intRegistrationLevelpKey == clsEventAccount.REGISTRATION_SingleSessionOnly))
                    showPanelReminders = false;
                else
                    showPanelReminders = bShowRemindersPanel;


                if (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_FullReg.ToString() || (data.GlobalAdmin))
                    Notificationtips = true;

                ViewBag.NotificationTips = Notificationtips;

                DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                if (bEvent && endDate > dtCurrentTime)
                {
                    ViewBag.ChatPanel_Visible = bEnableChatPanel;
                }
                else { ViewBag.ChatPanel_Visible = false; }
                ViewBag.Reminder_Visible = showPanelReminders;
                if (!ViewBag.ChatPanel_Visible && !ViewBag.Reminder_Visible)
                {
                    ViewBag.leftPanel_Visible = false;
                }

                cEvent.intEvent_PKey = data.EventId;
                string strName = (data.LastName + " " + data.FirstName).Replace("  ", " ");
                string str = new clsSettings().getText(clsSettings.TEXT_MyActivities).Replace("[Name]", strName);
                ViewBag.LblInstruct = clsReservedWords.ReplaceMyPageText(null, str, cEvent: cEvent, intEvtPKey: EventPKey);

                string ScheduleTypes = ((clsSettings)Session["cSettings"]).strScheduleSessionTypes;
                ViewBag.CalendarButtonVisible = repository.CheckCalnderIfExists(data.EventId, dtCurrentTime, data.Id, ScheduleTypes);

                if (ViewBag.VirtualDropdown_Visible)
                {
                    DateTime dtCalTime = clsEvent.getCaliforniaTime();
                    string Host = HttpContext.Request.Path.ToUpper().Replace("/MYMAGI", "");
                    SessionData.ddEventVirtualData = repository.GetVirtualEventDropDownInfo(Host, data.Id, data.EventId, dtCurrentTime, dtCalTime, data.ParentOrganization_pKey, intAttendeeStatus, intRegistrationLevelpKey, data.GlobalAdmin, data.StaffMember);
                    ViewBag.SelectedDropDown = GetSelectedVirtualDropDown(Host);
                }
                ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).bMediaPlaying = false;
                SessionData.HelpIconInfo = repository.PageLoadResourceData(data, "", "22");
                ViewBag.lblOverlap_Visible = false;
                System.Data.DataTable overlap = repository.GetOverLappingSessions(data.EventId, data.Id);
                if (overlap != null)
                {
                    ViewBag.lblOverlap_Visible = (overlap.Rows.Count > 0);
                    ViewBag.strOverLappingSession = ((overlap.Rows[0]["OverlappingSession"] == System.DBNull.Value) ? "" : overlap.Rows[0]["OverlappingSession"].ToString());
                }
                string[] strPages = cEvent.strEventPages.Split(',');
                int selectedEvent = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).intEventSelector;
                bool bSponsor = clsEventOrganization.CheckExhibitor(data.ParentOrganization_pKey, selectedEvent);
                ViewBag.lblRegText = "";
                ViewBag.OpenSurveyRadWindow=false;
                LoadRegistrationQuestions(data, intAttendeeStatus, bShowSurveyQuestion, bSponsor, intRegistrationLevel_pKey);
            }
            return View(SessionData);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        public ActionResult _PartialMySession(string Audience = null, string Days = null, string Tracks = null, string Levels = null, string Topics = null, string Cert = null)
        {
            ViewBag.IsHybrid= false;
            MySessionsPage SessionData = new MySessionsPage();
            if (User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                ViewBag.ID = data.Id;
                SessionData.SessionList = LoadSessionData(data.Id, data.EventId, Audience, Days, Tracks, Levels, Topics, Cert);

                string intRegistrationLevel_pKey = "";
                int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);
                ViewBag.IsHybrid= (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_FullReg.ToString());
            }
            return PartialView(SessionData.SessionList);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        public JsonResult GetOverLappingSessions()
        {
            try
            {
                bool lblOverLapVisible = false;
                string strOverLappingSession = "";
                if (User.Identity.AuthenticationType == "Forms")
                {
                    FormsIdentity identity = (FormsIdentity)User.Identity;
                    User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                    ViewBag.ID = data.Id;
                    System.Data.DataTable overlap = repository.GetOverLappingSessions(data.EventId, data.Id);
                    if (overlap != null)
                    {
                        lblOverLapVisible = (overlap.Rows.Count > 0);
                        strOverLappingSession = ((overlap.Rows[0]["OverlappingSession"] == System.DBNull.Value) ? "" : overlap.Rows[0]["OverlappingSession"].ToString());
                    }
                }

                return Json(new { msg = "OK", OverLapVisible = lblOverLapVisible, OverLappingSession = strOverLappingSession }, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return Json(new { msg = "Error" }, JsonRequestBehavior.AllowGet);
            }
        }

        private string ToStringAsXml(DataTable dt)
        {
            try
            {
                StringBuilder sbSql = new StringBuilder();
                StringWriter sw = new StringWriter(sbSql);
                string xmlformat = "";
                foreach (DataColumn col in dt.Columns)
                {
                    col.ColumnMapping = MappingType.Attribute;
                }
                dt.WriteXml(sw, XmlWriteMode.WriteSchema);
                xmlformat = sbSql.ToString();
                return xmlformat;
            }
            catch
            {
                return "";
            }
        }
        private void SendAnnouncement(User_Login data, int intStatus_pkey, int intPendingEvtSesPKey)
        {
            string strContent = "", StrSubject = "", strEmail = "";
            clsAnnouncement c = new clsAnnouncement();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            c.sqlConn = new SqlConnection(ReadConnectionString());
            c.lblMsg=null;
            c.intAnnouncement_PKey = intStatus_pkey;
            c.LoadAnnouncement();
            strContent = c.strHTMLText;
            StrSubject = c.strTitle;
            DataTable dt2 = new DataTable();
            dt2.Columns.Add("Email");
            dt2.Columns.Add("Name");
            dt2.Columns.Add("Subject");
            dt2.Columns.Add("Message");
            dt2.Columns.Add("Caption");
            dt2.Columns.Add("Account_pkey");
            dt2.Columns.Add("Certificate_pkey");
            dt2.Columns.Add("Session_pkey");
            dt2.Columns.Add("Degrees");
            dt2.Columns.Add("CCEmail");
            dt2.Columns.Add("BCCEmail");
            string contactName = data.LastName + ", " +data.FirstName;
            DataRow row = dt2.NewRow();
            strEmail = cSettings.strSupportEmail.ToString();
            row["Email"]= strEmail.ToString();
            row["Name"] = contactName;
            row["Subject"] = StrSubject.ToString();
            row["Message"] = strContent.ToString();
            row["Caption"] = StrSubject;
            row["Account_pkey"] = data.Id;
            row["Certificate_pkey"] = 0;
            row["Session_pkey"]= intPendingEvtSesPKey.ToString();
            row["CCEmail"] = "#";
            row["BCCEmail"] = "#";
            dt2.Rows.Add(row);
            dt2.TableName = "dt";
            string xmlString = ToStringAsXml(dt2);

            SqlCommand sqlCmd = new SqlCommand("Bulk_Email_save", new SqlConnection(ReadConnectionString()));
            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.CommandTimeout = 30;
            clsUtility.AddParameter(ref sqlCmd, "@XMLString", SqlDbType.VarChar, ParameterDirection.Input, xmlString);
            clsUtility.AddParameter(ref sqlCmd, "@IsLimit", SqlDbType.Bit, ParameterDirection.Input, 0);
            clsUtility.AddParameter(ref sqlCmd, "@Event_pkey", SqlDbType.Int, ParameterDirection.Input, data.EventId);
            clsUtility.AddParameter(ref sqlCmd, "@LatestStatus", SqlDbType.VarChar, ParameterDirection.Input, "0");
            clsUtility.AddParameter(ref sqlCmd, "@Sender_ID", SqlDbType.Int, ParameterDirection.Input, intStatus_pkey);
            clsUtility.AddParameter(ref sqlCmd, "@SenderEmailID", SqlDbType.VarChar, ParameterDirection.Input, "");
            clsUtility.AddParameter(ref sqlCmd, "@SenderName", SqlDbType.VarChar, ParameterDirection.Input, contactName);
            clsUtility.AddParameter(ref sqlCmd, "@Captiontitle", SqlDbType.VarChar, ParameterDirection.Input, StrSubject);
            clsUtility.AddParameter(ref sqlCmd, "@Account_pkey", SqlDbType.Int, ParameterDirection.Input, data.Id);
            clsUtility.AddParameter(ref sqlCmd, "@Date", SqlDbType.DateTime, ParameterDirection.Input, DateTime.Now);
            clsUtility.AddParameter(ref sqlCmd, "@Error", SqlDbType.VarChar, ParameterDirection.Output, "", 1000);
            clsUtility.AddParameter(ref sqlCmd, "@Count", SqlDbType.VarChar, ParameterDirection.Output, "", 1000);
            clsUtility.ExecuteStoredProc(sqlCmd, null, "Error Bulk Email Save");
        }
        [System.Web.Mvc.HttpPost]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public string SetAttend(string Type, int pKey, int sessionkey, string sessionID, bool CheckChanged, bool bAttend, bool bSlide, bool bWatch, DateTime ActDate, DateTime EndTime, int PrivateActivity, int IsInvitation, bool bAttendRemote = false)
        {
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

            string result = "OK";
            result = repository.UpdatecheckedValue(pKey, sessionID, CheckChanged, bAttend, bSlide, bWatch, ActDate, EndTime, bAttendRemote);
            if (PrivateActivity == 2 && IsInvitation != 2)
            {
                switch (Type)
                {
                    case "watch":
                    case "attend":
                        clsEventSession c = new clsEventSession();
                        c.sqlConn = new SqlConnection(ReadConnectionString());
                        c.lblMsg=null;
                        c.strSessionID = sessionID;
                        c.intEvent_PKey = data.EventId;
                        c.intEventSession_PKey =pKey;
                        c.RequertForAttend(data.Id, bAttend, bSlide, pKey, data.Id, data.EventId, sessionkey);
                        SendAnnouncement(data, 137, pKey);
                        break;
                }
            }
            return result;
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        public ActionResult _PartialQuestionsGrid(int? EventSessionPKey = null)
        {
            if (EventSessionPKey > 0)
            {
                System.Data.DataTable QuestionsData = repository.QuestionExecutions("RefreshQuestion", EventSessionPKey, Convert.ToInt32(User.Identity.Name));
                return PartialView(QuestionsData);
            }
            else
            {
                return PartialView(null);
            }
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult UpdateQuestions(string Question, int? EventSessionPKey = 0, int? QuestionID = 0, bool bSchedule = false)
        {
            try
            {
                if (string.IsNullOrEmpty(Question) || string.IsNullOrWhiteSpace(Question))
                    return Json(new { status = "Error", msg = "Enter the question." }, JsonRequestBehavior.AllowGet);

                string result = (QuestionID > 0) ? repository.SaveQuestionExecute(Question, QuestionID: QuestionID) : repository.SaveQuestionExecute(Question, EventSessionPKey, Convert.ToInt32(User.Identity.Name), Schedule: bSchedule);
                return (result != "OK") ? Json(new { status = "Error", msg = result }, JsonRequestBehavior.AllowGet) : (Json(new { status = "OK", msg = "OK" }, JsonRequestBehavior.AllowGet));
            }
            catch
            {
                return Json(new { status = "Error", msg = "Error Occurred While Updating Question" }, JsonRequestBehavior.AllowGet);
            }
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public void UpdateQuestionLog(int EventSessionPKey = 0)
        {
            if (EventSessionPKey > 0)
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                new clsEventSession().UpdateAccessLog(EventSessionPKey, data.Id, data.EventId, clsEvent.EnterEvent_Question, "");
            }
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult GetQuestionData(int QuestionID = 0)
        {
            try
            {
                if (QuestionID > 0)
                {
                    System.Data.DataTable Data = repository.QuestionExecutions("EditQuestion", QuestionID: QuestionID);
                    if (Data != null)
                        if (Data.Rows.Count > 0)
                            return Json(new { msg = "OK", Question = Data.Rows[0][0] }, JsonRequestBehavior.AllowGet);
                        else
                            return Json(new { msg = "Error: No Question Found", Question = "" }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { msg = "Error: No Question Found", Question = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { msg = "Error: No Question Found", Question = "" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { msg = "Error Occurred While Fetching Data" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion MySession

        #region MySchedule 

        [CustomizedAuthorize]
        public ActionResult MySchedule()
        {
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? ((Request.UrlReferrer.PathAndQuery == "/MyMAGI/MySession") ? "/Home/Index" : Request.UrlReferrer.PathAndQuery) : "/Home/Index";
            ViewBag.LabelTitle = "My Schedule - View";
            ViewData["Title"] = "ViewMySchedule";
            MySchedulePage ScheduleData = new MySchedulePage();
            //ScheduleData.chatViewModel = new Models.ViewModels.ChatViewModel();

            int offSet = 0;
            if (Request.Cookies["yjnf"] != null)
                int.TryParse(Request.Cookies["yjnf"].Value, out offSet);
            ViewBag.htmlPDF = "";
            ViewBag.bSurrogateStaff =false;
            ViewBag.OffsetVal = (-1) * offSet;
            ViewBag.OffsetVenue =0;
            if (User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.LblTitle = data.EventName + ": My Schedule - View";
                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);
                ViewBag.trCloudICons = (data.EventTypeId == clsEvent.EventType_CloudConference || data.EventTypeId == clsEvent.EventType_HybridConference);
                DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                ViewBag.CurrentTime = dtCurrentTime;
                ScheduleData.SessionList = repository.getMyScheduleDataByID(data.Id, data.EventId, true, dtCurrentTime);

                ViewBag.MyEventsButtonText = repository.GetCountsOnEventbutton(data.EventId, data.Id, dtCurrentTime);

                ViewBag.OffsetVenue = 60* (repository.OffsetVenue(data.EventId));
                int EventStatusPKey = 0; bool bShowRemindersPanel = false; DateTime endDate = DateTime.Now;
                System.Data.DataTable EventSettings = repository.getEventSettingsForSessionByID(data.EventId);
                string SkipRegDate = "";
                DateTime dtCurEventStart = DateTime.Now, dtCurEventEnd = DateTime.Now;
                if (EventSettings != null && EventSettings.Rows.Count > 0)
                {
                    ViewBag.StandardRegion = EventSettings.Rows[0]["StandardRegion"];
                    ViewBag.StandardRegionCode = EventSettings.Rows[0]["StandardRegionCode"];
                    ViewBag.TimeOffset = EventSettings.Rows[0]["TimeOffset"].ToString();
                    SkipRegDate = (EventSettings.Rows[0]["SkipRegDate"] != System.DBNull.Value) ? EventSettings.Rows[0]["SkipRegDate"].ToString() : "";
                    dtCurEventStart = (EventSettings.Rows[0]["EvtStartTime"] != System.DBNull.Value) ? Convert.ToDateTime(EventSettings.Rows[0]["EvtStartTime"]) : DateTime.Now;
                    dtCurEventEnd = (EventSettings.Rows[0]["dtCurEventEnd"] != System.DBNull.Value) ? Convert.ToDateTime(EventSettings.Rows[0]["dtCurEventEnd"]) : DateTime.Now;

                    int.TryParse(EventSettings.Rows[0]["EventStatuspKey"].ToString(), out EventStatusPKey);
                    bool.TryParse(EventSettings.Rows[0]["ShowRemindersPanel"].ToString(), out bShowRemindersPanel);
                    //bool.TryParse(EventSettings.Rows[0]["IsChatPanelOn"].ToString(), out bEnableChatPanel);
                    DateTime.TryParse(EventSettings.Rows[0]["EvtEndTime"].ToString(), out endDate);
                }

                ViewBag.ShowTimer = bShowRemindersPanel;
                string intRegistrationLevel_pKey = "";
                int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);
                ViewBag.leftPanel_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
                ViewBag.VirtualDropdown_Visible = ViewBag.leftPanel_Visible;

                clsSurrogate c1 = (clsSurrogate)Session["Surrogate"];
                if (c1 != null)
                    ViewBag.bSurrogateStaff = c1.bPrevAdmin;

                int intRegistrationLevelpKey = 0;
                if (!string.IsNullOrEmpty(intRegistrationLevel_pKey))
                    intRegistrationLevelpKey = Convert.ToInt32(intRegistrationLevel_pKey);


                int AttSCH = 0; double dblAccountAmount = 0;
                bool bSch = false, bShowSurveyQuestion = false;
                DataTable EventFeatures = repository.getDyamicEventSettings(data.EventId, "ISNULL(FeatureAccess,'') as FeatureAccess,ISNULL(AttendeeAccess,'') as AttendeeAccess,ISNULL(ShowSurveyQuestion,'0') as ShowSurveyQuestion");
                if (EventFeatures != null && EventFeatures.Rows.Count > 0)
                {
                    string FeatureAccess = EventFeatures.Rows[0]["FeatureAccess"].ToString();
                    string AttendeeAccess = EventFeatures.Rows[0]["AttendeeAccess"].ToString();
                    if (!string.IsNullOrEmpty(AttendeeAccess))
                        AttSCH = Convert.ToInt32(AttendeeAccess.Split(',')[0]);
                    if (!string.IsNullOrEmpty(FeatureAccess))
                        bSch = (Convert.ToInt32(FeatureAccess.Split(',')[0]) == 1);

                    bShowSurveyQuestion = (EventFeatures.Rows[0]["ShowSurveyQuestion"] != System.DBNull.Value) ? Convert.ToBoolean(EventFeatures.Rows[0]["ShowSurveyQuestion"]) : false;
                }
                ViewBag.NotSchedule = false;
                ViewBag.ShowAlert = false;
                ViewBag.PayNow = false;
                ViewData["NotAttendeeText"] = "You are not Attendee of this event.";
                dblAccountAmount = repository.GetAccountBalance(data.EventId, data.Id);
                clsSettings cSettings = (clsSettings)Session["cSettings"];
                if (!data.GlobalAdmin)
                {
                    if (bSch)
                    {
                        if (!(intAttendeeStatus == clsEventAccount.PARTICIPATION_Attending) && AttSCH == clsEvent.intAttAttending)
                        {
                            ViewBag.NotSchedule = true;
                            ViewBag.ShowAlert = true;
                        }
                        if ((AttSCH == clsEvent.intAttPaid || AttSCH == clsEvent.intAttAsIfPaid) && dblAccountAmount <= cSettings.intAttAccessBal)
                        {
                            ViewBag.PayNow = true;
                            ViewData["NotAttendeeText"] = "To access this feature, please pay your balance due of " + String.Format("{0:c}", dblAccountAmount);
                        }
                    }
                    else
                    {
                        ViewData["NotAttendeeText"] = "This feature is not yet available.";
                        ViewBag.NotSchedule = true;
                    }
                }
                if (ViewBag.NotSchedule)
                    return View(ScheduleData);

                bool bEvent = (EventStatusPKey != clsEvent.STATUS_Completed), showPanelReminders = false;

                if (bEvent && (intAttendeeStatus == 1 || intAttendeeStatus == 3) && (intRegistrationLevelpKey == clsEventAccount.REGISTRATION_SingleSessionOnly))
                    showPanelReminders = false;
                else
                    showPanelReminders = bShowRemindersPanel;

                ViewBag.NotificationTips = true;
                ViewBag.Reminder_Visible = showPanelReminders;

                if (!Convert.ToBoolean(ViewBag.Reminder_Visible))
                {
                    ViewBag.leftPanel_Visible = false;
                }

                string str = "Room assignments are subject to change so be sure to check the signs.For assistance, contact&nbsp;[MAGI Support].<br />";
                clsEvent cEvent = new clsEvent();
                cEvent.intEvent_PKey = data.EventId;
                str = (data.EventTypeId == clsEvent.EventType_CloudConference || data.EventTypeId == clsEvent.EventType_HybridConference) ? clsReservedWords.ReplaceMyPageText(null, str, cEvent: cEvent, intEvtPKey: data.EventId).Replace("Room assignments are subject to change so be sure to check the signs.", "") : clsReservedWords.ReplaceMyPageText(null, str, cEvent: cEvent, intEvtPKey: data.EventId);
                ViewBag.LblInstruct = str;
                ViewBag.EventTime = dtCurrentTime.ToString("hh:mm tt") + " " + ViewBag.StandardRegionCode;

                ViewBag.cmdConfFeedback = ((data.EventTypeId == clsEvent.EventType_CloudConference || data.EventTypeId == clsEvent.EventType_HybridConference) && (data.GlobalAdmin || data.StaffMember || (dtCurrentTime >= ViewBag.dtEventStart)));
                ViewBag.cmdConfFeedbackColor = (clsFeedback.CheckFeedbackSubmitted(data.EventId, data.Id) ? "btn" : "btn btnYellow");

                string ScheduleTypes = cSettings.strScheduleSessionTypes;
                ViewBag.CalendarButtonVisible = repository.CheckCalnderIfExists(data.EventId, dtCurrentTime, data.Id, ScheduleTypes);

                System.Data.DataTable dtSetting = repository.GetMySessionsSettingsInfo(data.EventId, data.Id);
                if (dtSetting != null)
                {
                    if (dtSetting.Rows.Count > 0)
                    {
                        ViewBag.SessionAlerts = dtSetting.Rows[0]["IsSessionAlerts"];
                        ViewBag.ShowSessionReminder = dtSetting.Rows[0]["SendSessionReminders"];
                    }
                }
                if (ViewBag.VirtualDropdown_Visible)
                {
                    DateTime dtCalTime = clsEvent.getCaliforniaTime();
                    string Host = HttpContext.Request.Path.ToUpper().Replace("/MYMAGI", "");
                    ScheduleData.ddEventVirtualData = repository.GetVirtualEventDropDownInfo(Host, data.Id, data.EventId, dtCurrentTime, dtCalTime, data.ParentOrganization_pKey, intAttendeeStatus, intRegistrationLevelpKey, data.GlobalAdmin, data.StaffMember);
                    ViewBag.SelectedDropDown = GetSelectedVirtualDropDown(Host);
                }
                ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).bMediaPlaying = false;
                ScheduleData.HelpIconInfo = repository.PageLoadResourceData(data, "", "16");

                ViewData["LinkShowSpeakerBefore"] = cSettings.intWebinarLinkShowSpeakerBefor;
                ViewData["LinkShowBefore"] = cSettings.intWebinarLinkShowBefor;
                ViewData["intWebinarLinkShowAfter"] = cSettings.intWebinarLinkShowAfter;
                ViewData["InstructionSpeakerFeedback"] = new clsSettings().getText(clsSettings.Text_SpeakerFeedbackInstruct);
                ScheduleData.ddPhonetype = repository.FetchSessionFilters(0, 8);

                List<GenericListItem> values = new List<GenericListItem>();
                DateTime d = dtCurEventStart;
                while (d <= dtCurEventEnd)
                {
                    if (SkipRegDate.Contains(d.ToString("yyyy-MM-dd")))
                        d = d.AddDays(1);
                    else
                    {
                        values.Add(new GenericListItem() { strText = d.ToString("MM/dd/yy"), value = d.ToShortDateString() });
                        d = d.AddDays(1);
                    }
                }
                ViewBag.ddStartSchedule = values;
                ViewBag.Alert=false;
                ViewBag.AlertZoomSessionMessage = "";
                string strSessionMessage = "";
                clsLastUsed cLast = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]);
                if (Request.QueryString["IsWait"]!= null && !cLast.bWaitPopup)
                {
                    cLast.bWaitPopup = true;
                    string type = Request.QueryString["Type"];
                    if (type != null && !string.IsNullOrEmpty(type))
                    {
                        if (type =="1")
                            strSessionMessage = "<b>Please Return Later</b><br /><br /> " + cLast.WaitPopUpString;
                        if (type =="2")
                            strSessionMessage = "<b>Session Has Ended</b><br /><br /> " + cLast.WaitPopUpString;
                        ViewBag.Alert=true;
                        ViewBag.AlertMessage =strSessionMessage;
                    }
                    else
                        strSessionMessage =  "Session is not ready to start or has ended. If the former, try again a few minutes before the session is to start.";
                }
                ViewBag.AlertMessage =strSessionMessage;

                string htmlPDF = "<div style='margin-top:10px;'><div Class='pageTitle'>" +  "<b>" + data.EventName + "</b>"
                    + "<br/>" + "<b> Schedule For " + data.FirstName + " " + data.LastName + "" + " As Of " + DateTime.Now.ToString("MM/dd/yy") + " -- " + ViewBag.StandardRegion + "</b>"
                    + "<br/><b>\"My time\" is based on the time zone in your browser.</b>"
                    + "</div></div>" + "<br/>";
                ViewBag.htmlPDF = htmlPDF;

                int selectedEvent = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).intEventSelector;
                bool bSponsor = clsEventOrganization.CheckExhibitor(data.ParentOrganization_pKey, selectedEvent);
                ViewBag.lblRegText = "";
                ViewBag.OpenSurveyRadWindow=false;
                LoadRegistrationQuestions(data, intAttendeeStatus, bShowSurveyQuestion, bSponsor, intRegistrationLevel_pKey);
            }


            return View(ScheduleData);
        }

        private string GetSelectedVirtualDropDown(string Host)
        {
            string SelectedValue = "0";
            switch (Host)
            {
                case "/EVENTONCLOUD":
                    SelectedValue = "0";
                    break;
                case "/EDUCATIONCENTER":
                    SelectedValue = "1";
                    break;
                case "/EXHIBITHALL":
                    SelectedValue = "5";
                    break;
                case "/NETWORKINGLOUNGE":
                    SelectedValue = "3";
                    break;
                case "/RESOURCESUPPORTCENTER":
                    SelectedValue = "4";
                    break;
                case "/EXHIBITORDIRECTORY":
                case "/EVENTSPONSORSDIRECTORY":
                    SelectedValue = "2";
                    break;
                case "/SCHEDULEDEVENT":
                    SelectedValue = "7";
                    break;
                case "/NAVIGATIONINSTRUCTION":
                    SelectedValue = "9";
                    break;
                case "/VIEWMYSCHEDULE":
                case "/CREATEMYSCHEDULE":
                case "/MYSESSION":
                case "/MYSCHEDULE":
                    SelectedValue = "16";
                    break;
                case "/COMMUNITYSHOWCASE":
                    {
                        SelectedValue = "11";
                        break;
                    }
                case "/NETWORKINGMODEL":
                    {
                        SelectedValue = "12";
                        break;
                    }
                case "/PHOTOWALL":
                    {
                        SelectedValue = "12";
                        break;
                    }
                case "/MYREMINDERS":
                    SelectedValue = "24";
                    break;
                case "/MYOPTIONS":
                    SelectedValue = "15";
                    break;
                case "/MYCONFERENCE":
                    SelectedValue = "14";
                    break;
                case "/MYNETWORKING":
                    SelectedValue = "18";
                    break;
                case "/VIRTUALSESSION":
                case "/VIRTUALEVENT":
                    SelectedValue = "25";
                    break;
                case "/MYCONSOLE":
                case "/SPONSORCHECKLIST":
                    SelectedValue = "19";
                    break;
                case "/MYCONFERENCEBOOK":
                    SelectedValue = "20";
                    break;
                case "/MYFAQS":
                    SelectedValue = "26";
                    break;
                case "/ZOOMSESSION":
                    SelectedValue = "28";
                    break;
                case "/SPEAKERLEFTPANEL":
                    SelectedValue = "28";
                    break;
                case "/SHOWNEWS":
                    SelectedValue  = "29";
                    break;
                default:
                    {
                        SelectedValue = "0";
                        break;
                    }
            }
            return SelectedValue;
        }


        [CustomizedAuthorize]
        [ValidateInput(true)]
        public void DownloadCalendarFile(string Download = "All")
        {
            try
            {
                if (User.Identity.AuthenticationType == "Forms")
                {
                    FormsIdentity identity = (FormsIdentity)User.Identity;
                    User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                    DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                    string ScheduleTypes = ((clsSettings)Session["cSettings"]).strScheduleSessionTypes;
                    System.Data.DataTable dt = repository.GetCalnderData(data.EventId, data.Id, dtCurrentTime, ScheduleTypes, Download);
                    if (dt != null)
                    {
                        if (dt.Rows.Count > 0)
                        {
                            string pKeys = "";
                            string fileName = (dt.Rows[0]["EventFullName"].ToString()).Replace(" ", "%20");
                            string EventTitle = dt.Rows[0]["EventID"].ToString().Replace(" ", "%20");
                            string Description = "", Summary = "";
                            DateTime startDate, endDate;
                            System.Text.StringBuilder icalSB = new System.Text.StringBuilder();
                            icalSB.AppendLine("BEGIN:VCALENDAR");
                            icalSB.AppendLine("PRODID:-//" + EventTitle + "//EN");
                            icalSB.AppendLine("VERSION:2.0");
                            icalSB.AppendLine("METHOD:PUBLISH");
                            icalSB.AppendLine("BEGIN:VTIMEZONE");
                            icalSB.AppendLine("TZID:GMT");
                            icalSB.AppendLine("END:VTIMEZONE");
                            foreach (System.Data.DataRow RowValue in dt.Rows)
                            {
                                pKeys += ((RowValue["EeventSession_pKey"] == System.DBNull.Value) ? "" : "," + RowValue["EeventSession_pKey"].ToString());
                                Summary = RowValue["Title"].ToString();
                                startDate = (DateTime)RowValue["StartTime"];
                                endDate = (DateTime)RowValue["EndTime"];
                                Description = (String.IsNullOrEmpty(RowValue["EventSpecificDescription"].ToString()) ? Summary : RowValue["EventSpecificDescription"].ToString());
                                icalSB.AppendLine("BEGIN:VEVENT");
                                icalSB.AppendLine("SUMMARY;LANGUAGE=en-us:" + Summary);
                                icalSB.AppendLine("CLASS:PUBLIC");
                                icalSB.AppendLine("DESCRIPTION:" + Description);
                                icalSB.AppendLine("UID:" + Guid.NewGuid().ToString());
                                icalSB.AppendLine("SEQUENCE:0");
                                icalSB.AppendLine(String.Format("CREATED:{0:yyyyMMddTHHmmssZ}", DateTime.UtcNow));
                                icalSB.AppendLine(String.Format("DTSTART;TZID=GMT:{0:yyyyMMddTHHmmssZ}", startDate));
                                icalSB.AppendLine(String.Format("DTEND;TZID=GMT:{0:yyyyMMddTHHmmssZ}", endDate));
                                icalSB.AppendLine("END:VEVENT");
                            }
                            icalSB.AppendLine("END:VCALENDAR");
                            if (!string.IsNullOrEmpty(pKeys))
                                repository.UpdateScheduleCalendar(pKeys, data.Id);

                            Response.AddHeader("content-disposition", "attachment; filename=" + fileName + ".ics");
                            Response.ContentType = "text/x-vCalendar";
                            Response.Write(icalSB.ToString());
                            Response.End();

                        }
                        else
                        {
                            Response.AddHeader("content-disposition", "attachment; filename= info.txt");
                            Response.ContentType = "text/plain";
                            Response.Write("No " + ((Download == "New") ? "new " : "") + "activities found for ICS calendar file download");
                            Response.End();
                        }
                    }
                    else
                    {
                        Response.AddHeader("content-disposition", "attachment; filename= info.txt");
                        Response.ContentType = "text/plain";
                        Response.Write("No " + ((Download == "New") ? "new " : "") + "activities found for ICS calendar file download");
                        Response.End();
                    }
                }
            }
            catch
            {
                Response.AddHeader("content-disposition", "attachment; filename= error.txt");
                Response.ContentType = "text/plain";
                Response.Write("Error Downloading ICS File");
                Response.End();
            }
        }

        private long ConvertToUnixTimeStamp(DateTime dtTime)
        {
            DateTime originDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = dtTime.ToUniversalTime() - originDate;
            return (long)Math.Floor(diff.TotalSeconds);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        public ActionResult _PartialMySchedule(string ViewType = "List")
        {
            ViewBag.ListType = ViewType;
            System.Data.DataTable ScheduleData = null;
            ViewBag.JDataSource = null;
            ViewBag.bSurrogateStaff =false;
            clsSurrogate c1 = (clsSurrogate)Session["Surrogate"];
            if (c1 != null)
                ViewBag.bSurrogateStaff = c1.bPrevAdmin;
            int offSet = 0;
            if (Request.Cookies["yjnf"] != null)
                int.TryParse(Request.Cookies["yjnf"].Value, out offSet);
            ViewBag.OffsetVal =   (-1) * offSet;
            ViewBag.OffsetVenue =0;
            ViewBag.trCloudICons = false;
            if (User.Identity.AuthenticationType == "Forms")
            {
                DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);
                ViewBag.trCloudICons = data.EventTypeId == clsEvent.EventType_CloudConference || data.EventTypeId == clsEvent.EventType_HybridConference;
                ViewBag.OffsetVenue = 60 * (repository.OffsetVenue(data.EventId));
                ScheduleData = repository.getMyScheduleDataByID(data.Id, data.EventId, true, dtCurrentTime);
                if (ViewType == "Calendar" && ScheduleData != null)
                {
                    ViewBag.JDataSource = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(ScheduleData.AsEnumerable().Select(p => new
                    {
                        category = p.Field<string>("TrackID"),
                        start = p.Field<DateTime>("StartTime"),
                        startStr = p.Field<DateTime>("StartTime").ToString("yyyy-MM-dd HH:mm:ss.fffffffK"),
                        end = p.Field<DateTime>("EndTime"),
                        endStr = p.Field<DateTime>("EndTime").ToString("yyyy-MM-dd HH:mm:ss.fffffffK"),
                        SessionTitle = p.Field<string>("SessionTitle"),
                        title = p.Field<string>("SessionID"),
                        content = p.Field<string>("Subject"),
                        EvtStartDate = p.Field<DateTime>("EvtStartDate"),
                        EvtStartstr = p.Field<DateTime>("EvtStartDate").ToString("yyyy-MM-dd HH:mm:ss.fffffffK"),
                        EvtEndDate = p.Field<DateTime>("EvtEndDate"),
                    }));
                }
            }
            return PartialView(ScheduleData);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult SendScheduledEmail(EmailContent ContentData)
        {
            clsLastUsed cLast = (clsLastUsed)Session["cLastUsed"];

            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<div style='margin-top:10px;'><div Class='pageTitle'>");
            sb.Append("<b>" + data.EventName + "</b>");
            sb.Append("<br/>" + "<b> Schedule For " + data.FirstName + " " + data.LastName + "" + " As Of " + DateTime.Now.ToString("MM/dd/yy") + " -- " + ContentData.StandardRegion + "</b>");
            sb.Append("</div></div>" + "<br/>");

            clsOneNotify c = new clsOneNotify();
            c.strName = "My Schedule";
            c.strEmailAddress = data.Email;

            cLast.colNotifications.Clear();
            cLast.strNotificationContent = "<br/>" + sb + "<br/>" + ContentData.eContent;
            cLast.strNotificationSubject = "MAGI Schedule for " + data.FirstName + " " + data.LastName;

            cLast.colNotifications.Add(c);
            c = null;
            return Json(new { msg = "OK", RedirectionUrl = "/SendEmail?F=1&NS=1" }, JsonRequestBehavior.AllowGet);
        }

        private string strFormatter(string str)
        {
            str = str.Replace("<span style=\"color:white;\">.</span>", "");
            str = str.Replace("<table class=\"pageSetup\">", "<table class=\"pageSetter\" style='font-size:0.7em;'>");
            str = str.Replace("<td style=\"width: 160px;min-width:160px;vertical-align:top;\">", "<td style='padding-top:5px;padding-bottom:5px;width:120px;font-size:12px;vertical-align: top;'>");
            str = str.Replace("<td style=\"width: 160px;min-width:160px;\">", "<td style='padding-top:5px;padding-bottom:5px;width:100px;font-size:12px;vertical-align: top;'>");
            str = str.Replace("<td style=\"width: 50px;min-width: 50px;vertical-align:top;\">", "<td style='padding-top:5px;padding-bottom:5px;width:50px;font-size:12px; vertical-align:top'>");
            str = str.Replace("<td class=\"magiActionIconGap\" style=\"width:145px;vertical-align: top;padding-left: 5px;\">", "<td style='padding-top:5px;padding-bottom:5px;width:30px; vertical-align:top'>");
            str = str.Replace("<td style=\"width: 160px;min-width:160px;vertical-align:top\">", "<td style='padding-top:5px;padding-bottom:5px;width: 160px;min-width:160px; vertical-align:top'>");
            str = str.Replace("<td style=\"vertical-align: top;\">", "<td style='padding-top:5px;padding-bottom:5px;font-size:12px;width: 600px;min-width: 600px;vertical-align:top'>");
            str = str.Replace("<span>", "<span style='font-size:12px;width:50px;vertical-align:top'>");
            str = str.Replace("&nbsp;&nbsp;", "");
            return str;
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult PrepareSchedulePDF(EmailContent ContentData)
        {
            if (User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                string ScheduleTypes = ((clsSettings)Session["cSettings"]).strScheduleSessionTypes;
                System.Data.DataTable dt = repository.GetCalnderData(data.EventId, data.Id, dtCurrentTime, ScheduleTypes, "All");

                clsLastUsed cLast = (clsLastUsed)Session["cLastUsed"];

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("<div style='margin-top:10px;'><div Class='pageTitle'>");
                sb.Append("<b>" + data.EventName + "</b>");
                sb.Append("<br/>" + "<b> Schedule For " + data.FirstName + " " + data.LastName + "" + " As Of " + DateTime.Now.ToString("MM/dd/yy") + " -- " + ContentData.StandardRegion + "</b>");
                sb.Append("<br/><b>\"My time\" is based on the time zone in your browser.</b>");
                sb.Append("</div></div>" + "<br/>");
                Aspose.Pdf.Document pdfnew = new Aspose.Pdf.Document();
                pdfnew.PageInfo.Width = Aspose.Pdf.PageSize.PageLetter.Width;
                pdfnew.PageInfo.Height = Aspose.Pdf.PageSize.PageLetter.Height;

                Aspose.Pdf.Page sec1 = pdfnew.Pages.Add();
                sec1.PageInfo.Margin.Left = 10;
                sec1.PageInfo.Margin.Right = 10;
                sec1.PageInfo.Margin.Top = 10;
                sec1.PageInfo.Margin.Bottom = 10;
                sec1.Paragraphs.Add(clsUtility.getPDFText(strFormatter("<br/>" + sb + "<br/>" + ContentData.eContent.Replace("font-size:11pt", "font-size:9pt")), "Verdana", 10, true, false, true, false));

                try
                {
                    string strScheduleFilename = data.EventName + "_" + data.Id.ToString() + "_Sched.pdf";
                    string strScheduleTargetFile = Server.MapPath("~/app_data/BookPrepTemp/" + strScheduleFilename);
                    pdfnew.Save(strScheduleTargetFile);
                    return Json(new { msg = "OK", FileName = strScheduleFilename }, JsonRequestBehavior.AllowGet);
                }
                catch
                {
                    return Json(new { msg = "Error Occurred While Processing PDF", FileName = "" }, JsonRequestBehavior.AllowGet);
                    //clsUtility.LogErrorMessage(Me.lblMsg, Me.Request, Me.GetType().Name, 0, "Error creating Schedule file")
                }
            }
            else
            {
                return Json(new { msg = "You are not authorized to download this file", FileName = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpGet]
        [CustomizedAuthorize]
        public FileResult DownloadScheduleFile(string FileName)
        {
            string strScheduleTargetFile = Server.MapPath("~/app_data/BookPrepTemp/" + FileName.Trim());
            if (System.IO.File.Exists(strScheduleTargetFile))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(strScheduleTargetFile);
                return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, FileName);
            }
            return null;
        }

        [HttpPost]
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult PreperareSynopsisFile(int ESPKey, string SessionID, string SessionTitle, string SessionDescription, DateTime StartTime, DateTime EndTime)
        {
            try
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                string strPhysicalPath = Server.MapPath("~/SynopsisDocuments");
                if (!System.IO.Directory.Exists(strPhysicalPath))
                    System.IO.Directory.CreateDirectory(strPhysicalPath);

                string strFileName = ESPKey.ToString() + "_Synopsis_" + DateTime.Now.ToString("yyMMdd") + ".pdf";
                strPhysicalPath = System.IO.Path.Combine(strPhysicalPath, User.Identity.Name + ESPKey.ToString() + "_Synopsis.pdf");
                Aspose.Pdf.Document pdfnew = clsUtility.CreateNewPDF(ESPKey.ToString(), false);
                int intAttendeeSpeakerCount = repository.getSpeakerAttendeeCount(ESPKey, data.EventId);
                clsEventSession cEventSession = new clsEventSession();
                cEventSession.sqlConn = new System.Data.SqlClient.SqlConnection(Session["sqlConn"].ToString());
                if (!cEventSession.CreateSessionSpeakerPage(pdfnew, strPhysicalPath, false, ESPKey.ToString(), intAttendeeSpeakerCount, strSessionID: SessionID, strSessionTitle: SessionTitle, strDescription: SessionDescription, dtStart: StartTime, dtEnd: EndTime))
                    return Json(new { msg = "Error While Processing Synopsis File" }, JsonRequestBehavior.AllowGet);

                if (System.IO.File.Exists(strPhysicalPath))
                {
                    return Json(new { msg = "OK", FileName = strFileName }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {

            }
            return Json(new { msg = "Error While Processing Synopsis File" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CustomizedAuthorize]
        public FileResult DownloadSynopsis(int ESPKey, string FileName)
        {
            string strScheduleTargetFile = System.IO.Path.Combine(Server.MapPath("~/SynopsisDocuments"), User.Identity.Name + ESPKey.ToString() + "_Synopsis.pdf");
            if (System.IO.File.Exists(strScheduleTargetFile))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(strScheduleTargetFile);
                return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, FileName);
            }
            return null;
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult UpdateSessionAlert(bool IsSessionAlert)
        {
            try
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                string result = repository.HearSessionAlertUpdate(IsSessionAlert, data.Id, data.EventId);
                if (result == "OK")
                    return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {

            }
            return Json(new { msg = "Error While Updating Hear Session Alert" }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult UpdateTextReminder(bool IsReminder)
        {
            try
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                string result = "";
                if (!IsReminder)
                {
                    result = repository.UpdateNotification(IsReminder, data.Id, data.EventId);
                    return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    System.Data.DataTable UserData = repository.CheckIfMobileAvailable(data.Id);
                    bool Available = false;
                    if (UserData != null)
                    {
                        if (UserData.Rows.Count > 0)
                        {
                            string bAvailable = (UserData.Rows[0]["Phone1Available"] == System.DBNull.Value) ? "0" : UserData.Rows[0]["Phone1Available"].ToString();
                            Available = (bAvailable == "1") ? true : false;
                            if (Available)
                            {
                                result = repository.UpdateNotification(IsReminder, data.Id, data.EventId);
                                return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                string MPhone1 = (UserData.Rows[0]["Phone"] == System.DBNull.Value) ? "" : UserData.Rows[0]["Phone"].ToString();
                                string MPhone1CC = (UserData.Rows[0]["Phone1CC"] == System.DBNull.Value) ? "" : UserData.Rows[0]["Phone1CC"].ToString();
                                string MPhone1Ext = (UserData.Rows[0]["Phone1Ext"] == System.DBNull.Value) ? "" : UserData.Rows[0]["Phone1Ext"].ToString();
                                string MPhoneType1 = (UserData.Rows[0]["PhoneType_pKey"] == System.DBNull.Value) ? "" : UserData.Rows[0]["PhoneType_pKey"].ToString();
                                string MPhone2 = (UserData.Rows[0]["Phone2"] == System.DBNull.Value) ? "" : UserData.Rows[0]["Phone2"].ToString();
                                string MPhone2CC = (UserData.Rows[0]["Phone2CC"] == System.DBNull.Value) ? "" : UserData.Rows[0]["Phone2CC"].ToString();
                                string MPhone2Ext = (UserData.Rows[0]["Phone2Ext"] == System.DBNull.Value) ? "" : UserData.Rows[0]["Phone2Ext"].ToString();
                                string MPhoneType2 = (UserData.Rows[0]["PhoneType2_pKey"] == System.DBNull.Value) ? "" : UserData.Rows[0]["PhoneType2_pKey"].ToString();
                                return Json(new { msg = "POPUP", Phone1 = MPhone1, Phone1CC = MPhone1CC, Phone1Ext = MPhone1Ext, PhoneType1 = MPhoneType1, Phone2 = MPhone2, Phone2CC = MPhone2CC, Phone2Ext = MPhone2Ext, PhoneType2 = MPhoneType2 }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
            }
            catch
            {

            }
            return Json(new { msg = "Error Occurred While Updating Text Reminder" }, JsonRequestBehavior.AllowGet);
        }

        private bool ValidateText(string Value, string strMsg, int? intmode = 0, int intMinLength = 1)
        {
            bool bvalid = true;

            switch (intmode)
            {

                case 9:
                    if (!System.Text.RegularExpressions.Regex.IsMatch(Value, @"^\(?([0-9]{3})\)?[-._ ]?([0-9]{3})[-._ ]?([0-9]{4})$"))
                        bvalid = false;
                    else if (System.Text.RegularExpressions.Regex.IsMatch(Value, @"[',']+|['?']+|['*']+|['<']+|['>']+|['[']+|['\]']+|['!']+|['@']+|['$']+|['%']+|['^']+|['&']+|[';']+|[':']+|['""']+|['\'']+|['/']+|['\\']+|['_']+|['~']+|['`']+|[a-z]+|[A-Z]") || Value.Length < 5)
                        bvalid = false;
                    break;
                case 10: bvalid = (Value != "" && clsUtility.isDigitsOnly(Value)); break;
                case 12:
                    if (string.IsNullOrEmpty(Value))
                        bvalid = true;
                    else
                        bvalid = clsUtility.isDigitsOnly(Value);
                    break;
                case 17: bvalid = (Value != "" && clsUtility.isDigitsOnly(Value)); break;
            }
            return bvalid;
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult UpdteContactNumber(bool IsReminder, string Phone1 = "", string Phone1CC = "", string Phone1Ext = "", string PhoneType1 = "", string Phone2 = "", string Phone2CC = "", string Phone2Ext = "", string PhoneType2 = "")
        {
            try
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                string ErrorMessage = "";
                string Phonenumber = Phone1.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Replace(".", "").Replace("_", "");
                string Phonenumber2 = Phone2.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Replace(".", "").Replace("_", "");

                if (!ValidateText(Phone1CC, "Country code must be a number for telephone1", intmode: 10))
                    ErrorMessage = ErrorMessage + "<br />Country code must be a number for telephone1";
                if (!ValidateText(Phonenumber, "Enter a valid phone number", intmode: 17))
                    ErrorMessage = ErrorMessage + "<br />Enter a valid phone number";
                if (!string.IsNullOrEmpty(Phone2))
                {
                    if (!ValidateText(Phonenumber2, "Enter a valid second phone number", intmode: 17))
                        ErrorMessage = ErrorMessage + "<br />Enter a valid second phone number";
                }
                if (!ValidateText(Phone1Ext, "Remove letter(s) from your first extention number.", intmode: 12))
                    ErrorMessage = ErrorMessage + "<br />Remove letter(s) from your first extention number.";
                if (!ValidateText(Phone2Ext, "Remove letter(s) from your first extention number.", intmode: 12))
                    ErrorMessage = ErrorMessage + "<br />Remove letter(s) from your first extention number.";

                if (ErrorMessage != "")
                    return Json(new { msg = ErrorMessage }, JsonRequestBehavior.AllowGet);
                else
                {
                    System.Data.DataTable infoData = repository.UpdateContactwithNotification(data.Id, data.EventId, IsReminder, Phone1, Phone1CC, Phone1Ext, PhoneType1, Phone2, Phone2CC, Phone2Ext, PhoneType2);
                    if (infoData != null)
                    {
                        if (infoData.Rows.Count > 0)
                        {
                            return Json(new { msg = "OK", ReminderValue = infoData.Rows[0]["NotificationValue"], UpdateResult = infoData.Rows[0]["Result"] }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch
            {

            }
            return Json(new { msg = "Error Occured While Updating Text Reminder" }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult GetTPInformation(int EventSessionPKey)
        {
            if (EventSessionPKey > 0)
            {
                System.Data.DataTable dataTPInfo = repository.getTpInfo(EventSessionPKey);
                if (dataTPInfo != null)
                {
                    if (dataTPInfo.Rows.Count > 0)
                    {
                        string TPMail = ((dataTPInfo.Rows[0]["TPMail"] != System.DBNull.Value) ? dataTPInfo.Rows[0]["TPMail"].ToString() : "");
                        string TPContact = ((dataTPInfo.Rows[0]["TPContact"] != System.DBNull.Value) ? dataTPInfo.Rows[0]["TPContact"].ToString() : "");
                        string TPAltContact = ((dataTPInfo.Rows[0]["TPAltContact"] != System.DBNull.Value) ? dataTPInfo.Rows[0]["TPAltContact"].ToString() : "");
                        string TPName = ((dataTPInfo.Rows[0]["TPName"] != System.DBNull.Value) ? dataTPInfo.Rows[0]["TPName"].ToString() : "");
                        return Json(new { msg = "OK", lblMail = TPMail, lblName = TPName, lblContact = TPContact, lblTPAltContact = TPAltContact }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(new { msg = "Error Loading Tp Info" }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult GetFeedbackInformation(int EventSessionPKey, int AttendPercent)
        {
            if (EventSessionPKey > 0)
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                DataTable dtFeedback = repository.GetFeebdackInformation(EventSessionPKey, data.EventId, data.Id);
                DataTable dtAdivces = repository.GetSpeakerAdvices();

                if (dtFeedback != null)
                {
                    if (dtFeedback.Rows.Count > 0)
                    {
                        string Title = ((dtFeedback.Rows[0]["SessionTitle"] != System.DBNull.Value) ? dtFeedback.Rows[0]["SessionTitle"].ToString() : "");
                        if (EventSessionPKey > 0)
                        {
                            clsEventSession cEventSession = new clsEventSession();
                            cEventSession.sqlConn = new System.Data.SqlClient.SqlConnection(Session["sqlConn"].ToString());
                            cEventSession.UpdateAccessLog(EventSessionPKey, data.Id, data.EventId, clsEvent.EnterEvent_Feedback, "");
                        }
                        return Json(new
                        {
                            msg = "OK",
                            lblSessionTitle = Title,
                            IsHaveSlides = dtFeedback.Rows[0]["IsHaveSlides"],
                            IsHowLOS = dtFeedback.Rows[0]["IsShowLOS"],
                            IsshowUnbiased = dtFeedback.Rows[0]["IsshowUnbiased"],
                            Comment = dtFeedback.Rows[0]["GComment"],
                            MAGISuggestions = dtFeedback.Rows[0]["MAGISuggestions"],
                            ddAttendPercent = dtFeedback.Rows[0]["AttendedPercent"],
                            CMDWithday_Visible = (AttendPercent >= 0),
                            ZoomVerificationMail = dtFeedback.Rows[0]["ZoomVerificationMail"],
                            ZoomVerification = dtFeedback.Rows[0]["IsZoomVerification"],
                            UserEmail = data.Email,
                            MaxLength = dtFeedback.Rows[0]["Maxlength"],
                            Source = JsonConvert.SerializeObject(dtFeedback),
                            Advices = JsonConvert.SerializeObject(dtAdivces)
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(new { msg = "Error Loading Feedback" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public string WithdrawFeebdack(int EventSessionPKey)
        {
            if (EventSessionPKey > 0)
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                clsFeedback c = new clsFeedback();
                c.sqlConn = new System.Data.SqlClient.SqlConnection(Session["sqlConn"].ToString());
                string FeedbackResult = repository.DeleteSpeakerFeedBack(data.Id, EventSessionPKey);
                if (FeedbackResult == "OK")
                {
                    clsAccount clsAccount = new clsAccount();
                    clsAccount.intAccount_PKey = data.Id;
                    clsAccount.LogAuditMessage("withdraw speaker feedback comment:" + EventSessionPKey.ToString(), clsAudit.LOG_FeedbackAdd);

                    string result = repository.UpdateScheduledHrsForCloud(data.Id, data.EventId);
                    return result;
                }
            }
            return "Error";
        }

        [HttpPost]
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult SaveFeedback(int EventSessionPKey, string AttendPercent, int ddScheduleFeedback, string FeedbackMail, string FeedbackComment, string MAGISuggestions, string FeedbackList)
        {
            if (EventSessionPKey <= 0)
                return Json(new { msg = "Error While Updating Feedback" }, JsonRequestBehavior.AllowGet);

            if (ddScheduleFeedback == 1)
            {
                if (string.IsNullOrEmpty(FeedbackMail))
                    return Json(new { msg = "<b>Invalid Email Address</b><br /> Enter email, address in valid format" }, JsonRequestBehavior.AllowGet);
                if (!clsUtility.CheckEmailFormat(FeedbackMail))
                    return Json(new { msg = "<b>Invalid Email Address</b><br /> Enter email, address in valid format" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(AttendPercent))
                return Json(new { msg = "Select percentage of activity attended" }, JsonRequestBehavior.AllowGet);
            try
            {
                int AttendPercentage = Convert.ToInt32(AttendPercent);
                int Count = 0;
                System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(Session["sqlConn"].ToString());


                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                if (ddScheduleFeedback == 0)
                    FeedbackMail = data.Email;

                List<SpeakerFeedback> speakerDynamic = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<List<SpeakerFeedback>>(FeedbackList);
                clsFeedback c = new clsFeedback();
                c.sqlConn = con;
                c.intEventSession_PKey = EventSessionPKey;
                foreach (SpeakerFeedback info in speakerDynamic)
                {
                    c.intAccount_PKey = Convert.ToInt32(info.speakerHidden);
                    c.strScore1 = Convert.ToString(info.ddTopic);
                    c.strScore2 = Convert.ToString(info.ddContent);
                    c.strScore3 = Convert.ToString(info.ddPresentation);
                    c.strScore4 = (string.IsNullOrEmpty(info.ddSlides) ? "-1" : Convert.ToString(info.ddSlides));
                    c.strScore5 = (string.IsNullOrEmpty(info.ddValue) ? "-1" : Convert.ToString(info.ddValue));
                    c.strLearn = (string.IsNullOrEmpty(info.ddLearning) ? "-1" : Convert.ToString(info.ddLearning));
                    c.strBias = (string.IsNullOrEmpty(info.ddUnbiased) ? "-1" : Convert.ToString(info.ddUnbiased));
                    string SpeakerAdvices = (string.IsNullOrEmpty(info.speakerAdvices) ? "" : Convert.ToString(info.speakerAdvices));
                    if (c.UpdateSpeakerFeedBack(data.Id, info.txtAreaSpeakFeedback, SpeakerAdvices) > 0)
                    {
                        if (Count == 0)
                        {
                            c.strEmailFeedback = FeedbackMail;
                            c.UpdateSpeakerFeedBackComments(EventSessionPKey, 0, data.Id, FeedbackComment.Trim(), AttendPercentage, MAGISuggestions: MAGISuggestions);
                        }
                        Count = Count + 1;
                    }
                    else
                    {
                        return Json(new { msg = "Error Occurred While Updating Feedback" }, JsonRequestBehavior.AllowGet);
                    }
                }
                string result = repository.UpdateScheduledHrsForCloud(data.Id, data.EventId);
                return Json(new { msg = result }, JsonRequestBehavior.AllowGet);
            }
            catch
            {

            }
            return Json(new { msg = "Error While Updating Feedback" }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        public ActionResult _PartialPersonalEvents()
        {
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = data.EventId;
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();
            DateTime dtCurrentTime = clsEvent.getEventVenueTime();
            DataTable dt = repository.GetPersonalEvents(data.Id, data.EventId, dtCurrentTime);
            return PartialView(dt);
        }
        [HttpPost]
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public string DeletePersonalSchedule(int PersonalSchedulePKey)
        {
            return repository.DeletePersonalSchedule(PersonalSchedulePKey);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult GetTimeList(int AMPM = 0)
        {
            List<object> valList = new List<object>();
            if (AMPM == 0)
                AMPM = 1;

            int d = ((AMPM == 1) ? 9 : 1);
            while (d < 12)
            {
                if (AMPM == 2)
                {
                    if (d < 8 || d > 12)
                    {
                        if (d == 1)
                            valList.Add(new { strText = "12", id = 12 });

                        valList.Add(new { strText = d.ToString(), id = d });
                    }
                }
                else
                    valList.Add(new { strText = d.ToString(), id = d });
                d = d + 1;
            }
            return Json(new { msg = "OK", List = valList }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult GetScheduleDataByID(int PersonalSchedulePKey)
        {
            try
            {
                int UserId = Convert.ToInt32(User.Identity.Name);
                DataTable data = repository.GetPersonalScheduleData(PersonalSchedulePKey, UserId);
                if (data != null && data.Rows.Count > 0)
                {
                    string startDate = "";
                    if (data.Rows[0]["DiscussionStart"] != System.DBNull.Value)
                        startDate = Convert.ToDateTime(data.Rows[0]["DiscussionStart"]).ToShortDateString();
                    return Json(new
                    {
                        msg = "OK",
                        StartDate = startDate,
                        Title = data.Rows[0]["Title"],
                        Description = data.Rows[0]["Description"],
                        Link = data.Rows[0]["Link"],
                        HostName = data.Rows[0]["HostName"],
                        RTDAmPm = data.Rows[0]["AMPM"],
                        EditEventDate = data.Rows[0]["DiscussionStart"],
                        ScheduleHour = data.Rows[0]["ScheduleHour"],
                        ScheduleMin = data.Rows[0]["ScheduleMin"],
                        Duration = data.Rows[0]["Duration"],
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

            }
            return Json(new { msg = "Error Occurred While Fetching Schedule Data" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult UpdatePersonalSchedule(string Title, string Description, string StartDate, string StartRTD, string ScheduleMin, string RTDAmPm, string Duration, int PersonalSchedulePKey = 0)
        {
            try
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                if (StartDate == "")
                    return Json(new { msg = "Choose any date and try again" }, JsonRequestBehavior.AllowGet);

                DateTime dtTargetDateStart = new DateTime();
                DateTime dtTargetDateEnd = new DateTime();

                if (RTDAmPm == "" || ScheduleMin =="" || StartRTD == "")
                    return Json(new { msg = "Choose valid time and try again" }, JsonRequestBehavior.AllowGet);

                string strStartDate = string.Format("{0:d}", StartDate) + " " + string.Format("{0:t}", StartRTD + ":" + ScheduleMin + "" + ((RTDAmPm == "1") ? "AM" : "PM"));
                dtTargetDateStart = Convert.ToDateTime(strStartDate);

                if (string.IsNullOrEmpty(Duration.Trim()))
                    return Json(new { msg = "Please enter time duration." }, JsonRequestBehavior.AllowGet);

                int valDuration = Convert.ToInt32(Duration.Trim());

                if (valDuration <= 0)
                    return Json(new { msg = "Please enter time duration." }, JsonRequestBehavior.AllowGet);

                dtTargetDateEnd = clsUtility.DateAdd_Interval(valDuration, dtTargetDateStart);

                TimeSpan t = dtTargetDateEnd - dtTargetDateStart;
                if (t.TotalMinutes < 15)
                    return Json(new { msg = "Events must be during the event (9am-7pm) and at least 15 minutes long." }, JsonRequestBehavior.AllowGet);

                string message = repository.CheckOverlappingPersonalEvent(data.Id, data.EventId, dtTargetDateStart, dtTargetDateEnd, PersonalSchedulePKey);
                if (message != "OK")
                    return Json(new { msg = message }, JsonRequestBehavior.AllowGet);


                string result = repository.UpdatePersonalSchedule(data.Id, data.EventId, Title, Description, dtTargetDateStart, dtTargetDateEnd, PersonalSchedulePKey);
                return Json(new { msg = result }, JsonRequestBehavior.AllowGet);
            }
            catch
            {

            }
            return Json(new { msg = "Error While Updating Personal Schedule" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public string CancelPersonalSchedule(int ID, string strSponsor, int account, int pkey, string Type)
        {
            if (Type == "MYPEOPLE" || Type == "MINGLE")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                clsEvent cEvent = new clsEvent();
                cEvent.intEvent_PKey = data.EventId;
                cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                cEvent.LoadEvent();
                DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                return repository.CancelPersonalScheduleUpdate(ID.ToString(), Type, account, data.EventId, dtCurrentTime);
            }
            else
            {
                return repository.CancelPersonalSchedule(ID, strSponsor, account, pkey);
            }
        }

        [HttpPost]
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult UpdteBtnPlay(string URL, int EspKey, string FileName)
        {
            if (!string.IsNullOrEmpty(URL) && URL.Contains("EventVideo"))
            {
                bool bExists = System.IO.File.Exists(Server.MapPath(URL));
                if (bExists)
                    return Json(new { msg = "Video not available" }, JsonRequestBehavior.AllowGet);
            }
            int UserID = Convert.ToInt32(User.Identity.Name);
            int EventID = (new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(((FormsIdentity)User.Identity).Ticket.UserData)).EventId;
            if (EspKey > 0)
            {
                string ipAddress2 = "", ipaddress = "";
                GetIPAddress(ref ipAddress2, ref ipaddress);
                clsEventSession cEventSesoion = new clsEventSession();
                cEventSesoion.sqlConn = new SqlConnection(ReadConnectionString());
                cEventSesoion.UpdateAccessLogRecording(EspKey, UserID, EventID, 2, FileName, ipaddress, ipAddress2);
            }

            clsSettings cSettings = (clsSettings)Session["cSettings"];
            string result = repository.CheckSessionCountMessage(EspKey, UserID, cSettings.intMaxRecordingAlertVal);

            new clsReminders().UserReminderStatusUpdate(EventID, UserID, clsReminders.R_SessionRecording);
            return Json(new { msg = "OK", CountAlert = result }, JsonRequestBehavior.AllowGet);
        }
        private void GetIPAddress(ref string IpAddress1, ref string IpAddress2)
        {
            string ipaddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ipaddress))
                ipaddress = Request.ServerVariables["REMOTE_ADDR"];

            ipaddress = ((ipaddress == "::1") ? ipaddress : ipaddress.Split(':').First().Trim());
            IpAddress1 = ipaddress;
            if ((ipaddress.Contains(",")))
            {
                IpAddress2 = ipaddress.Split(',').First();
                IpAddress1 = ipaddress.Split(',').Last();
            }
        }
        [HttpPost]
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public string UpdteBtnPromo(int EspKey)
        {
            int UserID = Convert.ToInt32(User.Identity.Name);
            if (EspKey > 0)
            {
                int EventID = (new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(((FormsIdentity)User.Identity).Ticket.UserData)).EventId;
                new clsEventSession().UpdateAccessLog(EspKey, UserID, EventID, 2, "");
            }
            return "OK";
        }
        #endregion MySchedule

        #region LeftPanel
        private DataTable LoadReminderInformation(User_Login data)
        {
            System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;
            string Host = HttpContext.Request.Path.ToUpper().Replace("/MYMAGI", "");

            try
            {
                DataTable EventInfo = repository.getDyamicEventSettings(data.EventId, "IsDemo,IsDemoEvent,StartDate,EndDate,StandardTime_Pkey");
                bool IsDemoAccount = false, IsDemoEvent = false;
                DateTime StartDate = new DateTime(), EndDate = new DateTime(); DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                int StandardTime_Pkey = 0, intSessionSpeakerMinutes = 15;
                string RegionCode = "", StandardTimeZone = "", CurrentEventDuration = "before";
                if (EventInfo != null)
                {
                    if (EventInfo.Rows.Count > 0)
                    {
                        IsDemoAccount = (EventInfo.Rows[0]["IsDemo"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["IsDemo"].ToString());
                        IsDemoEvent = (EventInfo.Rows[0]["IsDemoEvent"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["IsDemoEvent"].ToString());
                        if (EventInfo.Rows[0]["StartDate"] != DBNull.Value) { StartDate = Convert.ToDateTime(EventInfo.Rows[0]["StartDate"].ToString()); }
                        if (EventInfo.Rows[0]["EndDate"] != DBNull.Value) { EndDate = Convert.ToDateTime(EventInfo.Rows[0]["EndDate"].ToString()); }
                        StandardTime_Pkey = (EventInfo.Rows[0]["StandardTime_Pkey"] == DBNull.Value) ? 0 : Convert.ToInt32(EventInfo.Rows[0]["StandardTime_Pkey"].ToString());
                        DataTable S_Timezone = repository.GetTimeZoneDataByKey(StandardTime_Pkey);
                        if (S_Timezone != null)
                        {
                            if (S_Timezone.Rows.Count > 0)
                            {
                                StandardTimeZone = (S_Timezone.Rows[0]["Region"] == DBNull.Value) ? "" : S_Timezone.Rows[0]["Region"].ToString();
                                RegionCode = (S_Timezone.Rows[0]["RegionCode"] == DBNull.Value) ? "" : S_Timezone.Rows[0]["RegionCode"].ToString();

                                ViewData["RegionCode"] = RegionCode;
                                ViewData["TimeOffset"] = (S_Timezone.Rows[0]["TimeOffset"] == DBNull.Value) ? "4" : S_Timezone.Rows[0]["TimeOffset"].ToString();
                            }
                        }
                    }
                }


                bool IsDemo = (IsDemoAccount && (data.GlobalAdmin || IsDemoEvent)), NewTab = (Host == "/VIRTUALEVENT" || Host == "/VIRTUALSESSION");

                if (dtCurrentTime < StartDate)
                    CurrentEventDuration = "before";
                else if (dtCurrentTime > EndDate)
                    CurrentEventDuration = "after";
                else
                    CurrentEventDuration = "during";

                if ((clsSettings)Session["cSettings"] != null)
                    intSessionSpeakerMinutes = ((clsSettings)Session["cSettings"]).intSessionSpeakerMinutes;

                ViewData["CurrentTime"] = dtCurrentTime.ToString("hh:mm tt");
                DataTable dt = repository.LoadListRemindersUpdated(data.Id, data.EventId, StandardTimeZone, "All", NewTab, dtCurrentTime, IsDemo, RegionCode, CurrentEventDuration, EndDate, StartDate, intSessionSpeakerMinutes);
                if (dt != null && dt.Rows.Count > 0)
                    return dt;
            }
            catch
            {
                clsUtility.LogErrorMessage(null, request, this.GetType().Name, 0, "Error Accessing Notifications.");
            }
            return null;
        }

        private void LoadNotificationTips(int EventPKey)
        {
            DataTable dt = new SqlOperation().GetReminderNotificationTips(EventPKey);
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    Random r = new Random();
                    DataRow dr = dt.AsEnumerable().OrderBy(x => r.Next()).FirstOrDefault();
                    if ((dr != null))
                    {
                        ViewData["VisibleTips"] = false; // True
                        ViewData["ReminderTip"] = "Tip: " + dr["Tip"].ToString();
                    }
                }
            }
        }

        public string ReplaceSentinels(string hdnmsg)
        {
            string msg = hdnmsg;
            SqlConnection con = new SqlConnection(ReadConnectionString());


            clsBR cBr = new clsBR();
            clsAnnouncement cAnnouncement = new clsAnnouncement();
            clsEvent cEvent = new clsEvent() { sqlConn = con };
            clsEventAccount cEventAccount = new clsEventAccount() { sqlConn = con };
            clsEventSession cEventSession = new clsEventSession() { sqlConn = con };
            clsSession cSession = new clsSession() { sqlConn = con };
            clsVenue cVenue = new clsVenue() { sqlConn = con };
            clsOrganization cOrganization = new clsOrganization() { sqlConn = con };
            clsAccount cAccount = new clsAccount() { sqlConn = con };

            msg = cBr.ReplaceReservedWords(msg);
            msg = cEvent.ReplaceReservedWords(msg);
            msg = cEventAccount.ReplaceReservedWords(msg);
            msg = cEventSession.ReplaceReservedWords(msg);
            msg = cVenue.ReplaceReservedWords(msg);
            msg = cAccount.ReplaceReservedWords(msg);
            msg = cOrganization.ReplaceReservedWords(msg);
            msg = clsSettings.ReplaceTermsGeneral(msg);

            return msg;
        }

        public void SendTH_StampMail(int intaccount, int eventAccount, int RequestingAccount, int RequestingTo, string strhdnThAccount)
        {
            if (!string.IsNullOrEmpty(strhdnThAccount))
                RequestingTo = Convert.ToInt32(RequestingAccount);
            new SqlOperation().SendRequestStampNotificationMail(intaccount, eventAccount, RequestingTo);
        }

        public PartialViewResult GetMyChat()
        {
            Models.ViewModels.ChatViewModel chatViewModel = new Models.ViewModels.ChatViewModel();
            if (User.Identity.IsAuthenticated)
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                string AbsolutePath = Request.Url.AbsolutePath.ToUpper();
                bool IsOnMyConsole = AbsolutePath.Contains("MYCONSOLE");

                var result = repository.IsChatOn(data.Id, data.EventId, IsOnMyConsole);
                chatViewModel.ChatEnabled = result.Item1;
                chatViewModel.MyID = data.Id;
                chatViewModel.EventID = data.EventId;
                chatViewModel.ActiveEventID = data.EventId;
                chatViewModel.strActiveEventName = data.EventCodeName;
                chatViewModel.IsReceiverPartner = false;
                chatViewModel.MyNickName = data.NickName;
                chatViewModel.MyFirstName = data.FirstName;
                chatViewModel.MyOrganization = data.Organization;
                chatViewModel.Organization_key = data.Organization_Key;
                chatViewModel.LastDateOfEvent = data.EventLastDate.AddDays(1).Date;
                chatViewModel.EventOrganizationkey = 0;
                chatViewModel.IsDemo = false;
                chatViewModel.Contactname = data.LastName + ", " + data.FirstName;
                chatViewModel.ChatTypes = null;
                DataSet ds = new DataSet();
                ds = result.Item2;

                if (ds.Tables.Count > 3 && ds.Tables[3].Rows.Count > 0)
                {
                    chatViewModel.StandardTimeCode = ds.Tables[3].Rows[0]["RegionCode"].ToString();
                    chatViewModel.TimeZoneDiff = Convert.ToDecimal(ds.Tables[3].Rows[0]["timeOffset"]);
                    chatViewModel.zoomSessionSilentMode = Convert.ToBoolean(ds.Tables[3].Rows[0]["IsShowChatNotification"]);
                    chatViewModel.IsDemo =
                        (
                            (Convert.ToBoolean(ds.Tables[2].Rows[0]["IsPartner"]) || Convert.ToBoolean(ds.Tables[2].Rows[0]["GlobalAdministrator"]))
                            &&
                            Convert.ToBoolean(ds.Tables[1].Rows[0]["isDemoMode"])
                        );
                }
                chatViewModel.NetworkingLevelDetails = repository.GeNetworkingLevel(data.Id, data.EventId);
            }
            else
                chatViewModel.ChatEnabled = false;

            return PartialView("_MyChat", chatViewModel);
        }

        [CustomizedAuthorize]
        public ActionResult ChatPanelAction()
        {
            Models.ViewModels.ChatViewModel chatViewModel = new Models.ViewModels.ChatViewModel();
            ViewBag.ChatPanel_Visible = false;
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

            string RegistrationLevel_pKey = string.Empty;
            int intRegistrationLevel_pKey = 0;
            int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref RegistrationLevel_pKey);
            int.TryParse(RegistrationLevel_pKey, out intRegistrationLevel_pKey);

            if (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1)
            {
                if (intRegistrationLevel_pKey != clsEventAccount.REGISTRATION_SingleSessionOnly)
                {
                    bool bEnableChatPanel = false;

                    var result = repository.IsChatPanelON(data.Id, data.EventId, Request.Url.AbsoluteUri, false);
                    bEnableChatPanel = Convert.ToBoolean(result.Item1);

                    clsEvent cEvent = new clsEvent();
                    cEvent.sqlConn = new SqlConnection(ReadConnectionString());

                    if (bEnableChatPanel && cEvent.CheckValiditityOfModule(data.EventId, "IsChatPanelOn"))
                    {
                        string groupName = result.Item2;
                        DataSet dataForListPanel = new DataSet();
                        dataForListPanel = result.Item3;

                        Dictionary<int, string> chatTypes = new Dictionary<int, string>()
                        {
                            {2,"1:1 My Open Chats"},
                            {4,"1:1 Initiate New Chats"},
                            {5,"Topic Chat"},
                            {1,"Event Chat"}
                            //,{8,"Rapid Fire Chat"}
                        };

                        if (!string.IsNullOrEmpty(groupName))
                        {
                            chatTypes.Add(7, "Interest Group");
                        }

                        chatViewModel.MyID = data.Id;
                        chatViewModel.EventID = data.EventId;
                        chatViewModel.ActiveEventID = data.EventId;
                        chatViewModel.strActiveEventName = data.EventCodeName;
                        chatViewModel.IsReceiverPartner = false;
                        chatViewModel.MyNickName = data.NickName;
                        chatViewModel.MyFirstName = data.FirstName;
                        chatViewModel.MyOrganization = data.Organization;
                        chatViewModel.Organization_key = data.Organization_Key;
                        chatViewModel.LastDateOfEvent = data.EventLastDate.AddDays(1).Date;
                        chatViewModel.EventOrganizationkey = 0;
                        chatViewModel.IsDemo = false;
                        chatViewModel.InterestBasedGroups = groupName;
                        chatViewModel.Contactname = data.LastName + ", " + data.FirstName;
                        chatViewModel.ChatTypes = chatTypes;
                        chatViewModel.PanelSet = dataForListPanel;

                        if (dataForListPanel.Tables.Count > 3 && dataForListPanel.Tables[3].Rows.Count > 0)
                        {
                            chatViewModel.StandardTimeCode = dataForListPanel.Tables[3].Rows[0]["RegionCode"].ToString();
                            chatViewModel.TimeZoneDiff = Convert.ToDecimal(dataForListPanel.Tables[3].Rows[0]["timeOffset"]);
                        }

                        if (dataForListPanel.Tables.Count > 4 && dataForListPanel.Tables[4].Rows.Count > 0)
                        {
                            chatViewModel.zoomSessionSilentMode = Convert.ToBoolean(dataForListPanel.Tables[4].Rows[0]["IsShowChatNotification"]);
                        }

                        ViewBag.ChatPanel_Visible = bEnableChatPanel;
                    }
                }
            }

            return PartialView("_PartialChatPanel", chatViewModel);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        public ActionResult _PartialLeftPanel(bool? Reminders = null, bool? NotificationTips = null, bool? Timer = null)
        {
            ViewData["VisibleTips"] = false;
            ViewData["ReminderTip"] = false;
            ViewData["Reminders"] = null;
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            if (Reminders == null || NotificationTips == null)
            {
                int intRegistrationLevelpKey = 0; bool bShowRemindersPanel = false; int EventStatusPKey = 0; string intRegistrationLevel_pKey = "";
                int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);
                if (!string.IsNullOrEmpty(intRegistrationLevel_pKey))
                    intRegistrationLevelpKey = Convert.ToInt32(intRegistrationLevel_pKey);

                if (NotificationTips == null)
                {
                    if (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_FullReg.ToString() || (data.GlobalAdmin))
                        NotificationTips = true;
                }

                if (Reminders == null)
                {
                    DataTable EventSettings = repository.getDyamicEventSettings(data.EventId, "EventStatus_pKey,ISNULL(ShowRemindersPanel,0) as ShowRemindersPanel");

                    if (EventSettings != null && EventSettings.Rows.Count > 0)
                    {
                        int.TryParse(EventSettings.Rows[0]["EventStatus_pKey"].ToString(), out EventStatusPKey);
                        bool.TryParse(EventSettings.Rows[0]["ShowRemindersPanel"].ToString(), out bShowRemindersPanel);
                    }


                    bool bEvent = (EventStatusPKey != clsEvent.STATUS_Completed);
                    if (bEvent && (intAttendeeStatus == 1 || intAttendeeStatus == 3) && (intRegistrationLevelpKey == clsEventAccount.REGISTRATION_SingleSessionOnly))
                        Reminders = false;
                    else
                        Reminders = bShowRemindersPanel;

                    Timer = bShowRemindersPanel;
                }
            }
            if (Reminders == true)
                ViewData["Reminders"] = LoadReminderInformation(data);
            if (NotificationTips == true)
                LoadNotificationTips(data.EventId);

            ViewBag.TimerVisible = Timer;
            ViewBag.Reminder_Visible = Reminders;

            return PartialView();
        }

        [AjaxValidateAntiForgeryToken]
        [CustomizedAuthorize]
        [ValidateInput(true)]
        public string NotificationCheck(string ValueOne, string ValuePKey, bool itemState)
        {
            try
            {
                HttpRequest request = System.Web.HttpContext.Current.Request;
                int Value = 0;
                string SettingType = ValueOne;
                if (!string.IsNullOrEmpty(ValuePKey)) { Value = Convert.ToInt32(ValuePKey); }
                if (!itemState)
                {
                    FormsIdentity identity = (FormsIdentity)User.Identity;
                    User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                    string result = repository.UpdateNotificationVisibility(data.EventId, Value, data.Id, SettingType);
                    if (result != "OK")
                        clsUtility.LogErrorMessage(null, request, this.GetType().Name, 0, "Error Updating Notification Visibility.");
                    return result;
                }
            }
            catch
            {

            }
            return "Error";
        }

        #endregion LeftPanel

        #region MyOrganization

        [CustomizedAuthorize]
        public ActionResult MyOrganization()
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            ViewBag.cmdBoothUpdateVisible =false;
            ViewBag.intEventOrgPKey =0;
            MySchedulePage ScheduleData = new MySchedulePage();
            clsEvent cEvent = new clsEvent();

            if (User.Identity.AuthenticationType == "Forms")
            {
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.LblTitle = data.EventName + ": My Schedule - View";
                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);

                DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                ViewBag.CurrentTime = dtCurrentTime;
                ScheduleData.SessionList = repository.getMyScheduleDataByID(data.Id, data.EventId, true, dtCurrentTime);

                int EventStatusPKey = 0; bool bShowRemindersPanel = false; bool bEnableChatPanel = false; DateTime endDate = DateTime.Now;
                System.Data.DataTable EventSettings = repository.getEventSettingsForSessionByID(data.EventId);
                string SkipRegDate = "";
                DateTime dtCurEventStart = DateTime.Now, dtCurEventEnd = DateTime.Now;
                if (EventSettings != null && EventSettings.Rows.Count > 0)
                {
                    ViewBag.StandardRegion = EventSettings.Rows[0]["StandardRegion"];
                    ViewBag.StandardRegionCode = EventSettings.Rows[0]["StandardRegionCode"];
                    SkipRegDate = (EventSettings.Rows[0]["SkipRegDate"] != System.DBNull.Value) ? EventSettings.Rows[0]["SkipRegDate"].ToString() : "";
                    dtCurEventStart = (EventSettings.Rows[0]["EvtStartTime"] != System.DBNull.Value) ? Convert.ToDateTime(EventSettings.Rows[0]["EvtStartTime"]) : DateTime.Now;
                    dtCurEventEnd = (EventSettings.Rows[0]["dtCurEventEnd"] != System.DBNull.Value) ? Convert.ToDateTime(EventSettings.Rows[0]["dtCurEventEnd"]) : DateTime.Now;

                    int.TryParse(EventSettings.Rows[0]["EventStatuspKey"].ToString(), out EventStatusPKey);
                    bool.TryParse(EventSettings.Rows[0]["ShowRemindersPanel"].ToString(), out bShowRemindersPanel);
                    bool.TryParse(EventSettings.Rows[0]["IsChatPanelOn"].ToString(), out bEnableChatPanel);
                    DateTime.TryParse(EventSettings.Rows[0]["EvtEndTime"].ToString(), out endDate);
                }

                cEvent.sqlConn = new SqlConnection(ReadConnectionString());

                if (bEnableChatPanel)
                {
                    bEnableChatPanel = cEvent.CheckValiditityOfModule(data.EventId, "IsChatPanelOn");
                }

                string intRegistrationLevel_pKey = "";
                int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);
                ViewBag.leftPanel_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
                ViewBag.VirtualDropdown_Visible = ViewBag.leftPanel_Visible;

                int intRegistrationLevelpKey = 0;
                if (!string.IsNullOrEmpty(intRegistrationLevel_pKey))
                    intRegistrationLevelpKey = Convert.ToInt32(intRegistrationLevel_pKey);

                bool bEvent = (EventStatusPKey != clsEvent.STATUS_Completed), showPanelReminders = false;

                if (bEvent && (intAttendeeStatus == 1 || intAttendeeStatus == 3) && (intRegistrationLevelpKey == clsEventAccount.REGISTRATION_SingleSessionOnly))
                    showPanelReminders = false;
                else
                    showPanelReminders = bShowRemindersPanel;

                if (bEvent && endDate > dtCurrentTime)
                {
                    ViewBag.ChatPanel_Visible = bEnableChatPanel;
                }
                else { ViewBag.ChatPanel_Visible = false; }
                ViewBag.Reminder_Visible = showPanelReminders;
                if (!ViewBag.ChatPanel_Visible && !ViewBag.Reminder_Visible)
                {
                    ViewBag.leftPanel_Visible = false;
                }

                string str = "Room assignments are subject to change so be sure to check the signs.For assistance, contact&nbsp;[MAGI Support].<br />";

                cEvent.intEvent_PKey = data.EventId;
                str = (data.EventTypeId == clsEvent.EventType_CloudConference || data.EventTypeId == clsEvent.EventType_HybridConference) ? clsReservedWords.ReplaceMyPageText(null, str, cEvent: cEvent, intEvtPKey: data.EventId).Replace("Room assignments are subject to change so be sure to check the signs.", "") : clsReservedWords.ReplaceMyPageText(null, str, cEvent: cEvent, intEvtPKey: data.EventId);
                ViewBag.LblInstruct = str;
                ViewBag.EventTime = dtCurrentTime.ToString("hh:mm tt") + " " + ViewBag.StandardRegionCode;


                string ScheduleTypes = cSettings.strScheduleSessionTypes;
                ViewBag.CalendarButtonVisible = repository.CheckCalnderIfExists(data.EventId, dtCurrentTime, data.Id, ScheduleTypes);

                System.Data.DataTable dtSetting = repository.GetMySessionsSettingsInfo(data.EventId, data.Id);
                if (dtSetting != null)
                {
                    if (dtSetting.Rows.Count > 0)
                    {
                        ViewBag.SessionAlerts = dtSetting.Rows[0]["IsSessionAlerts"];
                        ViewBag.ShowSessionReminder = dtSetting.Rows[0]["SendSessionReminders"];
                    }
                }
                if (ViewBag.VirtualDropdown_Visible)
                {
                    DateTime dtCalTime = clsEvent.getCaliforniaTime();
                    string Host = HttpContext.Request.Path.Replace("/c", "").ToUpper();
                    ScheduleData.ddEventVirtualData = repository.GetVirtualEventDropDownInfo(Host, data.Id, data.EventId, dtCurrentTime, dtCalTime, data.ParentOrganization_pKey, intAttendeeStatus, intRegistrationLevelpKey, data.GlobalAdmin, data.StaffMember);
                    ViewBag.SelectedDropDown = GetSelectedVirtualDropDown(Host);
                }
                ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).bMediaPlaying = false;
                ScheduleData.HelpIconInfo = repository.PageLoadResourceData(data, "", "16");

                ViewData["LinkShowSpeakerBefore"] = cSettings.intWebinarLinkShowSpeakerBefor;
                ViewData["LinkShowBefore"] = cSettings.intWebinarLinkShowBefor;
                ViewData["InstructionSpeakerFeedback"] = new clsSettings().getText(clsSettings.Text_SpeakerFeedbackInstruct);
                ScheduleData.ddPhonetype = repository.FetchSessionFilters(0, 8);

                List<GenericListItem> values = new List<GenericListItem>();
                DateTime d = dtCurEventStart;
                while (d <= dtCurEventEnd)
                {
                    if (SkipRegDate.Contains(d.ToString("yyyy-MM-dd")))
                        d = d.AddDays(1);
                    else
                    {
                        values.Add(new GenericListItem() { strText = d.ToString("MM/dd/yy"), value = d.ToShortDateString() });
                        d = d.AddDays(1);
                    }
                }

                ViewBag.ddStartSchedule = values;
                int intEventOrgPKey = 0;
                DataTable dt = repository.RefreshHasEventOrg(data.EventId, data.Id);
                if (dt!= null && dt.Rows.Count>0)
                {
                    ViewBag.intEventOrgPKey = ((dt.Rows[0][0]!= System.DBNull.Value) ? Convert.ToInt32(dt.Rows[0][0]) : 0);
                    ViewBag.cmdBoothUpdateVisible = (intEventOrgPKey>0);
                }

                if (ViewBag.cmdBoothUpdateVisible)
                {
                    cEvent.sqlConn= new SqlConnection(ReadConnectionString());
                    cEvent.LoadEvent();
                }
            }
            clsAccount cAccount = new clsAccount();
            cAccount.sqlConn = new SqlConnection(ReadConnectionString());
            cAccount.intAccount_PKey = data.Id;
            cAccount.LoadAccount();

            clsOrganization cOrganization = new clsOrganization();
            cOrganization.sqlConn = new SqlConnection(ReadConnectionString());
            cOrganization.intOrganization_pKey = cAccount.intParentOrganization_pKey;
            cOrganization.LoadOrganization();

            string orgtext = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.TEXT_HasOrg), null, null, cAccount);
            string resulttext = cOrganization.ReplaceReservedWords(orgtext);

            ViewBag.OrgText = resulttext;
            var _siteType = dba.BindSiteTypes();
            var _sites = dba.BindSites();
            var _country = dba.BindCountry();
            var _state = dba.BindStates();
            var _timezone = dba.BindTimeZones(cOrganization.intCountry_Pkey.ToString());

            ViewBag.cmdBoothUpdateVisible = (cEvent.bShowConsole && clsEventOrganization.MASTER_CheckExhibitor(cAccount.intREFOrganization_pKey, (cLast.intEventSelector > 0 ? cLast.intEventSelector : cLast.intActiveEventPkey), Account_pkey: cAccount.intAccount_PKey) && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "IsShowConsole"));
            ViewBag.SiteTypeTable = _siteType;
            ViewBag.SiteTable = _sites;
            ViewBag.CountryTable = _country;
            ViewBag.StateTable = _state;
            ViewBag.TimeZoneTable = _timezone;
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";

            return View(cOrganization);
        }

        [HttpPost]
        [ValidateInput(true)]
        [CustomizedAuthorize]
        [AjaxValidateAntiForgeryToken]
        public JsonResult UpdateOrganisation(FormCollection updateorg)
        {
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

            clsOrganization cOrganization = new clsOrganization();
            cOrganization.sqlConn = new SqlConnection(ReadConnectionString());
            cOrganization.intOrganization_pKey = data.ParentOrganization_pKey;
            cOrganization.LoadOrganization();
            cOrganization.strOrganizationID = updateorg["txtOrgName"].ToString();
            cOrganization.intOrganizationType_pKey = Convert.ToInt32(updateorg["cbSiteType"].ToString());
            cOrganization.strOrganizationTypeID = updateorg["hiddencbSiteType"].ToString();
            cOrganization.strOrgSiteType = updateorg["hiddencbSite"].ToString();
            cOrganization.intSiteOrgType_pkey = Convert.ToInt32(updateorg["cbSite"].ToString());
            cOrganization.strParentOrgName = updateorg["txtParentOrgName"].ToString();
            cOrganization.strPrimaryContactName = updateorg["txtPrimaryContactName"].ToString();
            cOrganization.strPrimaryContactPhone = updateorg["txtPrimPhone"].ToString();
            cOrganization.strPrimaryContactEmail = updateorg["txtPrimEmail"].ToString();
            cOrganization.strPrimaryContactTitle = updateorg["txtPrimTitle"].ToString();
            cOrganization.intCountry_Pkey = 0;
            cOrganization.intState_Pkey = 0;

            if (updateorg["cbCountry"].ToString() != "0")
                cOrganization.intCountry_Pkey = Convert.ToInt32(updateorg["cbCountry"].ToString());

            if (!(updateorg["cbState"].ToString() == "0" || Convert.ToInt32(updateorg["cbCountry"].ToString()) > 2))
                cOrganization.intState_Pkey = Convert.ToInt32(updateorg["cbState"].ToString());

            cOrganization.intTimezone_pKey = 0;
            cOrganization.strTimezoneID = "";

            if (!(updateorg["cbTimeZone"].ToString() == "0" || updateorg["cbTimeZone"].ToString() == "--Select--"))
            {
                cOrganization.intTimezone_pKey = Convert.ToInt32(updateorg["cbTimeZone"].ToString());
                cOrganization.strTimezoneID = updateorg["hiddenTimeZone"].ToString();
            }
            cOrganization.strZip = updateorg["txtZip"].ToString();
            cOrganization.strTimezoneID = updateorg["cbTimeZone"].ToString();
            cOrganization.strEmail2 = updateorg["txtEmail2"].ToString();
            cOrganization.strEmail = updateorg["txtEmail1"].ToString();
            cOrganization.strURL = updateorg["txtURL"].ToString();

            cOrganization.strAddress1 = updateorg["txtAddress1"].ToString();
            cOrganization.strAddress2 = updateorg["txtAddress2"].ToString();
            cOrganization.strCity = updateorg["txtCity"].ToString();
            cOrganization.strCountry = updateorg["hiddenCountry"].ToString();
            string resulttext = "", errorMsg = "";
            if (!string.IsNullOrEmpty(cOrganization.strPrimaryContactEmail))
                if (!clsUtility.CheckEmailFormat(cOrganization.strPrimaryContactEmail))
                    errorMsg = "<b>Invalid Email Address: Primary Contact Email</b><br /> Enter email, address in valid format";

            if (!string.IsNullOrEmpty(cOrganization.strEmail2))
                if (!clsUtility.CheckEmailFormat(cOrganization.strEmail2))
                    errorMsg = "<b>Invalid Email Address: Email2</b><br /> Enter email, address in valid format";


            if (!string.IsNullOrEmpty(cOrganization.strEmail))
                if (!clsUtility.CheckEmailFormat(cOrganization.strEmail))
                    errorMsg = "<b>Invalid Email Address: Email1</b><br /> Enter email, address in valid format";

            try
            {
                if (errorMsg == "")
                {
                    errorMsg = "Error Occurred While Updating Organization";
                    if (cOrganization.SaveOrganization(cOrganization))
                    {
                        clsAccount cAccount = new clsAccount();
                        clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                        cAccount.sqlConn = new SqlConnection(ReadConnectionString());
                        cAccount.intAccount_PKey = data.Id;
                        cAccount.LoadAccount();

                        string orgtext = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.TEXT_HasOrg), null, null, cAccount);
                        resulttext = cOrganization.ReplaceReservedWords(orgtext);
                        errorMsg = "Success";
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            return Json(new { result = errorMsg, PageMessage = resulttext }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region MY History
        [CustomizedAuthorize]
        public ActionResult MyHistory()
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            if (User.Identity.AuthenticationType == "Forms")
            {
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.LblTitle = data.EventName + ": My Schedule - View";
                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);
                DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                ViewBag.CurrentTime = dtCurrentTime;
            }
            int SelectedPageIndex = 0;
            string strEntName = "", strchange = "";
            DateTime dtStart = DateTime.Now.Date;
            DateTime dtEnd = DateTime.Now.AddDays(1).AddSeconds(-1);
            DataTable ListTable = new SqlOperation().getListAuditEntityPage(data.Id);
            ViewBag.PageList = ListTable;
            var historytable = dba.MyHistory(dtStart, dtEnd, strchange, strEntName, SelectedPageIndex, User.Identity.Name);
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            ViewBag.SelectedPageType = 3;
            return View(historytable);
        }

        [CustomizedAuthorize]
        public ActionResult _PartialMyHistory(string Days = null, string From = null, string To = null, string ActionLike = null, string EntityName = null, string PageList = null)
        {

            int SelectedPageIndex = 0;
            int SelectedDaysIndex = Convert.ToInt32(Days);
            string strchange = ActionLike.Trim();

            if (PageList != "")
                SelectedPageIndex = Convert.ToInt32(PageList);

            DateTime dpStrat = Convert.ToDateTime(From);
            DateTime dpEnd = Convert.ToDateTime(To);
            DateTime dtStart = DateTime.Now;
            DateTime dtEnd = DateTime.Now;
            switch (SelectedDaysIndex)
            {
                case 1:
                    dtStart = DateTime.Now.Date.AddDays(-1);
                    dtEnd = DateTime.Now.Date;
                    ViewBag.SelectedPageType = 1;
                    break;
                case 2:
                    dtStart = DateTime.Now.Date.AddDays(-7);
                    dtEnd = DateTime.Now.Date;
                    ViewBag.SelectedPageType = 2;
                    break;
                case 3:
                    dtStart = dpStrat;
                    dtEnd = dpEnd.AddDays(1).AddSeconds(-1);
                    ViewBag.SelectedPageType = 3;
                    break;
                case 4:
                    dtStart = DateTime.Now.Date.AddMonths(-1);
                    dtEnd = DateTime.Now.Date;
                    ViewBag.SelectedPageType = 4;
                    break;
                case 5:
                    dtStart = DateTime.Now.Date.AddYears(-1);
                    dtEnd = DateTime.Now.Date;
                    ViewBag.SelectedPageType = 5;
                    break;
            }
            var historytable = dba.MyHistory(dtStart, dtEnd, strchange, EntityName, SelectedPageIndex, User.Identity.Name);
            return PartialView(historytable);
        }

        #endregion

        public void BindDropdown(int Account_PKEY)
        {
            DataTable ds = dba.BindSessionTracks(Account_PKEY);
            List<SelectListItem> pageList = new List<SelectListItem>();
            foreach (DataRow dr in ds.Rows)
            {
                pageList.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pKey"].ToString() });
            }
            ViewBag.PageList = pageList;

        }
        public void BindCountries()
        {
            DataTable ds = dba.BindCountry();
            List<SelectListItem> countryList = new List<SelectListItem>();
            foreach (DataRow dr in ds.Rows)
            {
                countryList.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pKey"].ToString() });
            }
            ViewBag.CountryList = countryList;
        }
        public void BindSiteType()
        {
            DataTable ds = dba.BindSiteTypes();
            List<SelectListItem> sitetypelist = new List<SelectListItem>();
            foreach (DataRow dr in ds.Rows)
            {
                sitetypelist.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pKey"].ToString() });
            }
            ViewBag.SiteTypeTable = ds;

        }
        public void BindSites()
        {
            DataTable ds = dba.BindSites();
            List<SelectListItem> sitelist = new List<SelectListItem>();
            foreach (DataRow dr in ds.Rows)
            {
                sitelist.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pKey"].ToString() });
            }

            ViewBag.SiteList = sitelist;
        }

        public JsonResult BindStates(string Country_Pkey)
        {
            DataTable ds = dba.BindStates();
            List<SelectListItem> statelist = new List<SelectListItem>();
            foreach (DataRow dr in ds.Rows)
            {
                statelist.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pKey"].ToString() });
            }

            return Json(statelist, JsonRequestBehavior.AllowGet);
        }
        public JsonResult BindTimeZones(string Country_Pkey)
        {
            DataTable ds = dba.BindTimeZones(Country_Pkey);
            List<SelectListItem> timezoneList = new List<SelectListItem>();
            foreach (DataRow dr in ds.Rows)
            {
                timezoneList.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pKey"].ToString() });
            }

            return Json(timezoneList, JsonRequestBehavior.AllowGet);
        }

        #region My Certificate
        [CustomizedAuthorize]
        public ActionResult MyCertificate()
        {
            DataTable myCerificate = new DataTable();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            if (User.Identity.AuthenticationType == "Forms")
            {
                DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);
                ViewBag.CurrentTime = dtCurrentTime;
                myCerificate = repository.GetCertificateData(data.Id.ToString());
            }
            return View(myCerificate);
        }
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult ProcessCertificate(string Event_pKey, string CertAbbrev, int CertPKey, string EarnedCEUs, int pKey, int Type, string ExamCertificateText, string CertDate, string LatestCertDate, string CRCPExpirationDate, int ExamStatus_pkey, string IsUpdatedBy)
        {
            try
            {
                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);

                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                string strFilename = "", strDisplayName = "", strDestDir = "", strEventId = "", strPendingAcctName = "", strURL = "";
                double dblTotalHrs = 0, dblHrs = 0, dblNotLiveHrs = 0, dblLiveEarned = 0; bool bUpdatedByUser = (IsUpdatedBy != "0");

                strPendingAcctName = data.FirstName + ((string.IsNullOrEmpty(data.MiddleName)) ? "" : " " + data.MiddleName) + " " + ((string.IsNullOrEmpty(data.LastName)) ? "" : " " + data.LastName);

                System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());

                clsEvent cEvt = new clsEvent() { sqlConn = conn, lblMsg = null, intEvent_PKey = Convert.ToInt32(Event_pKey) };
                clsEventAccount cEvtAcct = new clsEventAccount() { sqlConn = conn, lblMsg = null, intEventAccount_pKey = pKey };
                clsCertification c = new clsCertification() { sqlConn = conn, lblMsg = null, intCertification_PKey = CertPKey, intEventpKey = cEvt.intEvent_PKey };
                cEvt.LoadEvent();
                strEventId = cEvt.strEventID;
                strDisplayName = clsUtility.CleanFilename(strEventId + "_" + CertAbbrev + "_" + strPendingAcctName + "_" + (new DateTime().ToString("yyMMdd")) + ".pdf");
                switch (Type)
                {
                    case 0:
                        if (!(CertPKey == 3 && bUpdatedByUser))
                        {
                            strFilename = clsUtility.CleanFilename(Event_pKey.ToString() + "_" + pKey.ToString() + "_" + CertAbbrev + ".pdf");
                            strDestDir = Server.MapPath("~/UserDocuments/");
                            strURL = "~/UserDocuments/";
                            if (!clsUtility.FileExists(strDestDir + strFilename))
                            {
                                if (c.LoadCertification())
                                {
                                    if (!string.IsNullOrEmpty(EarnedCEUs))
                                    {
                                        dblHrs = Convert.ToDouble(EarnedCEUs);
                                        dblNotLiveHrs = Convert.ToDouble(EarnedCEUs);
                                    }
                                    dblLiveEarned = dblHrs - dblNotLiveHrs;
                                    dblTotalHrs = c.getTotalHoursForEvent(cEvt.intEvent_PKey);
                                    c.CreateCertificate(strFilename, strPendingAcctName, strEventId, cEvt.strLocationCity, cEvt.dtStartDate, cEvt.dtEndDate, dblHrs, dblTotalHrs, cEvt.strEventFullname, cEvt.strLocationStateCode, data.Id, dblNotLiveHrs, dblLiveEarned);
                                }
                            }
                        }
                        else
                        {
                            //Dim c As New clsCertification
                            //Dim IsOverrideHrs As Boolean = gdi.GetDataKeyValue("IsOverrideHrs")
                            //Dim dblHrsPossible As Double = c.getTotalHoursForEvent(intEventPkey)
                            //Dim dbPRACredits As Double = gdi.GetDataKeyValue("PRACredits")
                            //myVS.intPendingEventPkey = gdi.GetDataKeyValue("Event_pKey")
                            //myVS.intPendingAcctPkey = gdi.GetDataKeyValue("Account_pKey")
                            //lblCertTitle.Text = gdi.GetDataKeyValue("CertName")
                            //Me.lblLive.Text = String.Format("{0:N1}", gdi.GetDataKeyValue("EarnedCEUs") - gdi.GetDataKeyValue("NotLiveEarned"))  'Val(gdi.GetDataKeyValue("LiveEarned"))
                            //Me.txtNotLive.Text = String.Format("{0:N1}", gdi.GetDataKeyValue("NotLiveEarned"))
                            //Me.lblEarned.Text = String.Format("{0:N1}", gdi.GetDataKeyValue("EarnedCEUs"))
                            //Dim maxhrs As Double = IIf(dblHrsPossible < dbPRACredits AndAlso dblHrsPossible > 0 AndAlso Not IsOverrideHrs, dblHrsPossible, If(dbPRACredits = 0, dblHrsPossible, dbPRACredits))
                            //Me.lblMaxHours.Text = "Maximum " + maxhrs.ToString("0.0") + " Hours Attended"
                            //Me.hdnMaxHrs.Value = maxhrs
                            //Me.lblMaxNotLive.Text = "Maximum " + Val(gdi.GetDataKeyValue("NotLiveCredits")).ToString("0.0") + " Hours Watched"
                            //Me.lblMaxCombine.Text = "Maximum " + Val(gdi.GetDataKeyValue("CombinedCredits")).ToString("0.0") + " Combined Hours"
                            //Me.hdnMaxNotLive.Value = Val(gdi.GetDataKeyValue("NotLiveCredits"))
                            //Me.hdnCertificationPkey.Value = gdi.GetDataKeyValue("pKey")
                            //clsUtility.PopupRadWindow(ScriptManager.GetCurrent(Me.Page), Me.Page, rwCertUpdate)
                            //Dim script As String = "function f(){ OpenCertUpdateWindow() ; Sys.Application.remove_load(f);} Sys.Application.add_load(f);"
                            //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)
                            //Exit Select
                        }
                        break;
                    case 1:
                        cEvtAcct.LoadEventInfo();
                        strEventId = (string.IsNullOrEmpty(cEvt.strEventID) ? cLast.strActiveEvent : cEvt.strEventID);
                        strFilename = "TempAttendanceCert.pdf";
                        strURL = "~/TempDocuments/";
                        strDestDir = Server.MapPath("~/TempDocuments/");
                        if (cEvtAcct.CreateAttendanceCertificate(strDestDir + strFilename, cEvt))
                            cEvtAcct.LogAuditMessage("Download: Attendance Certificate - " + cEvt.strEventID, clsAudit.LOG_DownloadAtt);
                        break;
                    case 2:
                        cEvtAcct.LoadEventInfo();
                        strFilename = "TempAttendanceCert.pdf";
                        strURL = "~/TempDocuments/";
                        strDestDir = Server.MapPath("~/TempDocuments/");
                        if (cEvtAcct.CreateSpeakerCertificate_MVC(strDestDir + strFilename, cEvt, strPendingAcctName, data.Id))
                            cEvtAcct.LogAuditMessage("Download: Attendance Certificate - " + cEvt.strEventID, clsAudit.LOG_DownloadSpk);
                        break;
                    case 3:
                        DateTime dtExamDate = Convert.ToDateTime(CertDate);
                        DateTime dtLatestExamDate = Convert.ToDateTime(LatestCertDate);
                        DateTime dtExamExpDate = Convert.ToDateTime(CRCPExpirationDate);
                        strFilename = clsUtility.CleanFilename("CRCP_" + data.Id.ToString() + "_" + pKey.ToString() + ".pdf");
                        strDisplayName = clsUtility.CleanFilename(cLast.strActiveEvent + "_" + "CRCP_" + strPendingAcctName + "_" + (new DateTime().ToString("yyMMdd")) + ".pdf");
                        strURL = "~/UserDocuments/";
                        strDestDir = Server.MapPath("~/UserDocuments/");

                        clsExam cExam = new clsExam() { sqlConn = conn, lblMsg = null, intExam_PKey = 1 };
                        cExam.LoadExam();
                        cExam.CreateCertificate(strFilename, strPendingAcctName, dtExamDate, dtLatestExamDate, dtExamExpDate, ExamCertificateText, ExamStatus_pkey);
                        break;

                }
                if (!(CertPKey == 3 && !bUpdatedByUser))
                {
                    if (!clsUtility.FileExists(strDestDir + strFilename))
                        return Json(new { msg = "The selected file was not found. Contact MAGI to regenerate the certificate.", Destination = (strDestDir + strFilename), DisplayName = strDisplayName }, JsonRequestBehavior.AllowGet);

                    return Json(new { msg = "OK", Destination = strURL, FileName = strFilename, DisplayName = strDisplayName }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

            }
            return Json(new { msg = "Error Occurred While Processing The Selected File" }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        public FileResult DownloadCertificateFile(string Destination, string FileName, string DisplayName)
        {
            string strScheduleTargetFile = Server.MapPath(Destination + FileName.Trim());
            if (System.IO.File.Exists(strScheduleTargetFile))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(strScheduleTargetFile);
                return File(bytes, "application/pdf", DisplayName);
            }
            else
            {
                return null;
            }
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult DownloadSchedule(string strEventID, string Event_Pkey, string strCertPkey, string strSessions = "", string strCertAbbrv = "")
        {
            try
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                int EventPkey = Convert.ToInt32(Event_Pkey);
                string strScheduleFilename = "", strScheduleTargetFile = "", AccountNAme = "";

                AccountNAme = data.FirstName + ((string.IsNullOrEmpty(data.MiddleName)) ? "" : " " + data.MiddleName) + " " + ((string.IsNullOrEmpty(data.LastName)) ? "" : " " + data.LastName);

                Aspose.Pdf.Document sec = new Aspose.Pdf.Document();
                Aspose.Pdf.Page pdfnew = sec.Pages.Add();
                pdfnew.PageInfo.Width = Aspose.Pdf.PageSize.PageLetter.Width;
                pdfnew.PageInfo.Height = Aspose.Pdf.PageSize.PageLetter.Height;

                Document document = new Document();

                // Add page
                Aspose.Pdf.Page sec1 = document.Pages.Add();
                pdfnew.PageInfo.Margin.Left = 72;
                pdfnew.PageInfo.Margin.Right = 72;
                pdfnew.PageInfo.Margin.Top = 25;
                pdfnew.PageInfo.Margin.Bottom = 72;

                string s = "~/Images/HomePage2/magilogo.jpg";
                //clsUtility.AddPDFImage(pdfnew, s, "");
                s = Server.MapPath(s);
                // --heading
                if (strEventID == null)
                    pdfnew.Paragraphs.Add(clsUtility.getPDFText("MAGI's Clinical Research Conference Schedule", "Verdana", 14, false, true, true, true, true));
                else
                    pdfnew.Paragraphs.Add(clsUtility.getPDFText(strEventID, "Verdana", 14, false, true, true, true, true));

                pdfnew.Paragraphs.Add(clsUtility.getPDFText("Schedule for " + AccountNAme, "Verdana", 10, false, true, true, true));

                if (strCertAbbrv != null)
                {
                    pdfnew.Paragraphs.Add(clsUtility.getPDFText("", "Verdana", 12, false, true, true, true));
                    pdfnew.Paragraphs.Add(clsUtility.getPDFText("Sessions that qualify for " + strCertAbbrv + " contact hours", "Verdana", 10, false, true, true, true));
                }
                pdfnew.Paragraphs.Add(clsUtility.getPDFText("(as of " + DateTime.Now.ToString("MMMM dd, yyyy") + ")", "Verdana", 10, false, true, true, true));

                //  --content
                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                cEventAccount.lblMsg = null;
                cEventAccount.intEvent_pKey = EventPkey;
                cEventAccount.intAccount_pKey = data.Id;
                cEventAccount.LoadEventInfo(true);

                s = ((strSessions != "") ? cEventAccount.getHTMLSessionSchedule(strSessions, strCertPkey) : cEventAccount.getHTMLScheduledAttendanceSession());
                pdfnew.Paragraphs.Add(clsUtility.getPDFText(s, "Verdana", 10, true, false, true, false));

                strScheduleFilename = strEventID + "_" + data.Id.ToString() + "_Sched.pdf";
                strScheduleTargetFile = Server.MapPath("~/app_data/BookPrepTemp/" + strScheduleFilename);
                string strDisplayFilename = clsUtility.CleanFilename(strEventID + "_Schedule_" + AccountNAme + "_" + DateTime.Now.ToString("yyMMdd") + ".pdf");
                sec.Save(strScheduleTargetFile);
                return Json(new { msg = "OK", FileName = strScheduleFilename }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { msg = "Error while processing schedule file" }, JsonRequestBehavior.AllowGet);
            }
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        public FileResult Download(string filename)
        {
            string strScheduleTargetFile = Server.MapPath("~/app_data/BookPrepTemp/" + filename.Trim());
            if (System.IO.File.Exists(strScheduleTargetFile))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(strScheduleTargetFile);
                return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, filename);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region MyPayments

        [CustomizedAuthorize]
        [ValidateInput(true)]
        public ActionResult MyPayments(MyPaymentPage model, FormCollection fc)   //MyPayments()
        {
            ViewBag.PageTitle = "My Charges & Payments";

            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            ViewBag.ID = data.Id;
            ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
            ViewBag.EventPKey = data.EventId;
            ViewBag.EventAccountPKey = data.EventAccount_pkey;
            ViewBag.EventTypeID = data.EventTypeId;
            ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);
            DateTime dtCurrentTime = clsEvent.getEventVenueTime();
            ViewBag.CurrentTime = dtCurrentTime;


            double dblAcctBalance = 0, dblCredits = 0, dblDebits = 0;
            BindEventIds();

            DataTable dtpayment = dba.MyPayments(data.Id.ToString(), "");
            List<My_Payments> mypayments = new List<My_Payments>();
            foreach (DataRow dr in dtpayment.Rows)
            {
                int intType = (dr["TypeOfCharge"] == System.DBNull.Value ? 0 : Convert.ToInt32(dr["TypeOfCharge"].ToString()));
                double rowAmt = (dr["RowAmt"] == System.DBNull.Value ? 0 : Convert.ToDouble(dr["RowAmt"].ToString()));
                if (rowAmt > 0)
                    dblCredits += rowAmt;
                else
                    dblDebits += rowAmt;

                dblAcctBalance += rowAmt;

                double _payment = (dr["PaymentAmt"] == System.DBNull.Value ? 0 : Convert.ToDouble(dr["PaymentAmt"].ToString()));
                double _charge = (dr["ChargeAmt"] == System.DBNull.Value ? 0 : Convert.ToDouble(dr["ChargeAmt"].ToString()));
                double _balance = (dr["Balance"] == System.DBNull.Value ? 0 : Convert.ToDouble(dr["Balance"].ToString()));

                mypayments.Add(new My_Payments
                {
                    Item = dr["pKey"].ToString(),
                    Date = string.Format("{0:g}", dr["TransDate"]),
                    Event = dr["EvtID"].ToString(),
                    Transaction = dr["ChargeTypeID"].ToString(),
                    Memo = dr["Memo"].ToString(),
                    GroupCode = dr["groupDiscount"].ToString(),
                    LoggedByName = dr["LoggedBy"].ToString(),
                    Payment = _payment,
                    Status = dr["Status"].ToString(),
                    Charge = _charge,
                    Balance = dblAcctBalance,
                    Document = dr["PaymentReference"].ToString(),
                    rowAmt = rowAmt
                });
            }

            ViewBag.Credits = string.Format("{0:c}", dblCredits);
            ViewBag.Debits = string.Format("{0:c}", dblDebits);
            ViewBag.AccountBalance = string.Format("{0:c}", dblAcctBalance);
            ViewBag.VoucherVisible = false;
            ViewBag.strFinanceStartDate = cSettings.strFinanceStartDate;
            clsAccount cAccount = new clsAccount();
            cAccount.sqlConn = conn;
            cAccount.intAccount_PKey = data.Id;

            MyPaymentPage PaymentPageData = new MyPaymentPage
            {
                PaymentsTable = mypayments,
                //VoucherTable = clsVoucher.getVoucherByAccount(data.Email, intAccountPkey: data.Id),
                OtherReciptTable = dba.OtherRecipts(data.Id.ToString())
            };
            ViewData["lnkTransferVisible"] = false;
            ViewData["cmdCancelPlan"] = false;
            string intRegistrationLevel_pKey = "";
            clsEventAccount cEventAccount = new clsEventAccount();
            cEventAccount.sqlConn = conn;
            cEventAccount.intEvent_pKey = data.EventId;
            cEventAccount.intAccount_pKey = data.Id;
            ViewData["lnkTransferVisible"] = false;

            if (!cEventAccount.LoadEventInfo(true))
                ViewData["cmdCancelPlan"] = false;
            else if (cEventAccount.intParticipationStatus_pKey == clsEventAccount.PARTICIPATION_Cancelled)
                ViewData["cmdCancelPlan"] = false;
            else
                ViewData["cmdCancelPlan"] = true;

            int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);

            //if (intAttendeeStatus != clsEventAccount.PARTICIPATION_Cancelled)
            //    ViewData["cmdCancelPlan"] = true;

            double dblRefundBalance = clsEventAccount.getAccountBalance(data.Id, data.EventId);

            int cancelPopupField = 0;
            if (cEventAccount.bSpeakerAtEvent)
                cancelPopupField = 1;
            else if (cEventAccount.intParticipationStatus_pKey == clsEventAccount.PARTICIPATION_Provisional)
                cancelPopupField = 2;
            else if (cEventAccount.intParticipationStatus_pKey == clsEventAccount.PARTICIPATION_Attending && cEventAccount.dblAccountBalance == 0 && dblRefundBalance == 0)
                cancelPopupField = 3;

            ViewBag.cancelPopupField = cancelPopupField;//  cancelPopupField;

            bool CancelRegistrationVisible = true;
            if (cEventAccount.dblAccountBalance < 0)
                CancelRegistrationVisible = false;

            ViewBag.CancelRegistrationVisible = CancelRegistrationVisible; //  CancelRegistrationVisible;

            ViewBag.intParticipationStatus = cEventAccount.intParticipationStatus_pKey;
            ViewBag.lblTitleSpeaker = (cEventAccount.bSpeakerAtEvent || cEventAccount.bModtrAtEvent || cEventAccount.bChairAtEvent || cEventAccount.bLeaderAtEvent);

            var paymenttype = dba.PaymentType(data.Id, data.EventId);

            ViewBag.dblAccountBalance = cEventAccount.dblAccountBalance;

            ViewBag.DoubleEventAcouuntBalnace = cEventAccount.dblAccountBalance < 0;
            ViewBag.labeldblActBalnaceAmount = string.Format("{0:c}", Math.Abs(cEventAccount.dblAccountBalance));
            ViewBag.txtdblActBalnaceAmount = Math.Abs(cEventAccount.dblAccountBalance);
            ViewBag.PageFileName = "MAGI Charges & Payments - " + ViewBag.FullName + " - " + DateTime.Now.ToString("yMMdd");

            return View(PaymentPageData);
        }
        public PartialViewResult _PartialViewMyPayments(string EventID)
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

            DataTable dtpayment = dba.MyPayments(data.Id.ToString(), EventID);
            double dblAcctBalance = 0;
            double dblCredits = 0;
            double dblDebits = 0;
            List<My_Payments> mypayments = new List<My_Payments>();
            foreach (DataRow dr in dtpayment.Rows)
            {
                {

                    double _payment = 0;
                    string Payment = (dr["PaymentAmt"].ToString());
                    if (Payment != "")
                    {
                        _payment = Convert.ToDouble(Payment);
                    }
                    double _charge = 0;
                    string Charge = (dr["ChargeAmt"].ToString());
                    if (Charge != "")
                    {
                        _charge = Convert.ToDouble(Charge);
                    }

                    double _balance = 0;
                    string Balance = dr["Balance"].ToString();
                    if (Balance != "")
                    {
                        _balance = Convert.ToDouble(Balance);
                    }
                    double _rowAmt = 0;
                    string rowAmt = dr["RowAmt"].ToString();
                    if (rowAmt != "")
                    {
                        _rowAmt = Convert.ToDouble(rowAmt);
                    }

                    if (_rowAmt > 0)
                    {
                        dblCredits = dblCredits + _rowAmt;
                    }
                    else
                    {
                        dblDebits = dblDebits + _rowAmt;
                    }
                    dblAcctBalance = dblAcctBalance + _rowAmt;




                    mypayments.Add(new My_Payments
                    {
                        Item = dr["pKey"].ToString(),
                        Date = dr["TransDate"].ToString(),
                        Event = dr["EvtID"].ToString(),
                        Transaction = dr["ChargeTypeID"].ToString(),
                        Memo = dr["Memo"].ToString(),
                        GroupCode = dr["TransDate"].ToString(),
                        LoggedByName = dr["LoggedBy"].ToString(),
                        Payment = _payment,
                        Status = dr["Status"].ToString(),
                        Charge = _charge,
                        Balance = dblAcctBalance,
                        Document = dr["PaymentReference"].ToString(),
                        rowAmt = _rowAmt

                    });

                }
            }

            DataTable dt = new DataTable();
            clsAccount cAccount = new clsAccount();
            cAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cAccount.intAccount_PKey = data.Id;
            cAccount.LoadAccount();


            ViewBag.Credits = dblCredits;
            ViewBag.Debits = dblDebits;
            ViewBag.AccountBalance = dblAcctBalance;
            ViewBag.VoucherVisible = false;

            var table = new MyPaymentPage
            {
                PaymentsTable = mypayments,
                VoucherTable = clsVoucher.getVoucherByAccount(cAccount.strEmail, intAccountPkey: cAccount.intAccount_PKey),
                OtherReciptTable = dba.OtherRecipts(data.Id.ToString())


            };
            return PartialView(table);
        }
        private void BindEventIds()
        {
            DataTable ds = dba.BindEventID();
            List<SelectListItem> eventlist = new List<SelectListItem>();
            foreach (DataRow dr in ds.Rows)
                eventlist.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pKey"].ToString() });

            ViewBag.EventList = eventlist;
        }
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult ProcessReciptDocument(string docID)
        {
            try
            {
                string stringImage = "";
                string strlogo = "~/Images/HomePage/magilogo.jpg";
                int h = 50;
                Dictionary<int, clsImg> dct = ((Dictionary<int, clsImg>)System.Web.HttpContext.Current.Application["cImages"]);
                if (dct.ContainsKey(clsImages.IMG_6))
                {
                    strlogo = dct[clsImages.IMG_6].strPath;
                    h = dct[clsImages.IMG_6].intHeight;
                }
                int intPayMethod = clsPayment.METHOD_Credit;
                DateTime dtEarlyBirdDate = new DateTime(1980, 1, 1).Date;
                string RecieptNo = docID.Replace("R", "").Replace("N", "");
                int RNo = 0;
                if (!string.IsNullOrEmpty(RecieptNo))
                    RNo = Convert.ToInt32(RecieptNo);
                clsReceipt c = new clsReceipt() { sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString()), lblMsg = null, intReceiptNumber = RNo };
                string Label13 = "", lblReciiptTitle = "", DocName = "", tdclass = "";
                string lblReceiptNum = c.getReceiptNum();
                string lblReceiptBody = c.getPaymentLogBody_MVC(intPayMethod, dtEarlyBirdDate, ref lblReciiptTitle, ref Label13);

                if ((Label13 == "Receipt"))
                {
                    tdclass = "rcpt";
                    lblReceiptNum = "R" + lblReceiptNum;
                    DocName = "MAGI_Receipt_R" + RecieptNo + "_" + DateTime.Now.ToString("yyMMdd") + ".pdf";
                    string strStatus = c.getReciptPaidFreeStatus();
                    if (strStatus != "")
                        tdclass = (strStatus == "Free") ? "freeRcpt" : "rcpt";
                }
                else
                {
                    tdclass = "inv";
                    lblReceiptNum = "N" + lblReceiptNum;
                    DocName = "MAGI_Receipt_N" + RecieptNo + "_" + DateTime.Now.ToString("yyMMdd") + ".pdf";
                }
                return Json(new { msg = "OK", Type = Label13, InfoClass = tdclass, RecipetTitle = lblReciiptTitle, Logo = strlogo.Replace("~", ""), Height = h, ReciptNum = lblReceiptNum, RecieptBody = lblReceiptBody, DocumentName = DocName }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { msg = "Error" }, JsonRequestBehavior.AllowGet);
            }
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult GetUserVouchers()
        {
            DataTable voucherList = null;
            try
            {
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                SqlConnection Conn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                voucherList = clsVoucher.getVoucherByAccount_MVC(data.Email, intAccountPkey: data.Id);
            }
            catch
            {
            }
            return Json(new { msg = "OK", Source = JsonConvert.SerializeObject(voucherList) }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        public FileResult ExportPDF(string ReceiptNum)
        {
            try
            {
                int intPayMethod = clsPayment.METHOD_Credit;
                DateTime dtEarlyBirdDate = new DateTime(1980, 1, 1).Date;
                string RecieptNo = ReceiptNum.Replace("R", "").Replace("N", "");
                int RNo = 0;
                if (!string.IsNullOrEmpty(RecieptNo))
                    RNo = Convert.ToInt32(RecieptNo);
                clsReceipt c = new clsReceipt() { sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString()), lblMsg = null, intReceiptNumber = RNo };
                string Label13 = "", lblReciiptTitle = "";
                string lblReceiptNum = c.getReceiptNum();
                string lblReceiptBody = c.getPaymentLogBody_MVC(intPayMethod, dtEarlyBirdDate, ref lblReciiptTitle, ref Label13);

                byte[] MSbytes = new byte[2048];
                string DisplayFileName = "";
                clsReceipt.ExportReceiptPdf_MVC(lblReciiptTitle, ReceiptNum, lblReceiptBody, MSBytes: ref MSbytes, strDisplayFileName: ref DisplayFileName);
                return File(MSbytes, "application/pdf", DisplayFileName);
            }
            catch
            {
                return null;
            }
        }
        [CustomizedAuthorize]
        [ValidateInput(true)]
        public void DownloadVoucher(string ID)
        {
            try
            {
                int intpKey = 0;
                if (!string.IsNullOrEmpty(ID))
                    intpKey = Convert.ToInt32(ID);
                string Destination = "", fileName = "";
                clsVoucher.DownloadVoucher_MVC(intpKey, ref Destination, ref fileName);

                if (System.IO.File.Exists(Destination))
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(Destination);

                    Response.Clear();
                    Response.ContentType = "application/pdf";
                    Response.ClearHeaders();
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
                    Response.Buffer = true;
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.BinaryWrite(bytes);
                    Response.End();
                    Response.Close();
                    System.IO.File.Delete(Destination);
                }
                else
                {
                    Response.Clear();
                    Response.ClearHeaders();
                    Response.AddHeader("content-disposition", "attachment; filename= error.txt");
                    Response.ContentType = "text/plain";
                    Response.Write("Error Downloading ICS File");
                    Response.End();
                }
            }
            catch
            {
                Response.Clear();
                Response.ClearHeaders();
                Response.AddHeader("content-disposition", "attachment; filename= error.txt");
                Response.ContentType = "text/plain";
                Response.Write("Error Downloading ICS File");
                Response.End();
            }


        }

        [CustomizedAuthorize]
        public PartialViewResult _PartialPayCredit()
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            ViewData["phCreditAddress"] = (cSettings.bCreditCard_Address ? "" : "none");
            ViewData["phCreditCV"] = (cSettings.bCreditCard_CVCode ? "" : "none");
            ViewData["phCreditFirstLast"] = (cSettings.bCreditCard_Firstlastname ? "" : "none");
            ViewData["phCreditName"] = (cSettings.bCreditCard_Singlename ? "" : "none");
            ViewData["phCreditZipCode"] = (cSettings.bCreditCard_Zipcode ? "" : "none");
            return PartialView();
        }
        public void SetAmount(double dblAmt, bool tr)
        {
            ViewBag.lblCreditAmount = String.Format("{0:c}", Math.Abs(dblAmt));
            ViewBag.txtCreditAmount.Value = Math.Abs(dblAmt);
            ViewBag.labeldblActBalnaceAmount = Math.Abs(dblAmt);
            ViewBag.txtdblActBalnaceAmount = Math.Abs(dblAmt);
        }
        //public JsonResult BindMonth()
        //{
        //    DateTime month = Convert.ToDateTime("1/1/2000");
        //    List<SelectListItem> monthlist = new List<SelectListItem>();

        //    for (int i = 1; i <= 12; i++)
        //    {
        //        DateTime NextMont = month.AddMonths(i - 1);
        //        string month1 = NextMont.ToString("MMMM");
        //        string mnthvalue = NextMont.Month.ToString();
        //        monthlist.Add(new SelectListItem { Text = month1, Value = mnthvalue });
        //    }
        //    ViewBag.MonthList = monthlist;
        //    return Json(new { result = "OK", monthList = monthlist }, JsonRequestBehavior.AllowGet);
        //}
        //public JsonResult BindYears()
        //{
        //    int curYear = DateTime.Now.Year;
        //    List<SelectListItem> yearlist = new List<SelectListItem>();
        //    for (int i = curYear; i <= curYear + 30; i++)
        //    {
        //        yearlist.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
        //    }
        //    ViewBag.YearList = yearlist;
        //    return Json(new { result = "OK", yearlist = yearlist }, JsonRequestBehavior.AllowGet);
        //}
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        [CustomizedAuthorize]
        public JsonResult CancelRegistration(string cancelregion)
        {
            if (string.IsNullOrEmpty(cancelregion))
                return Json(new { result = "Error", msg = "Enter cancellation reason." }, JsonRequestBehavior.AllowGet);
            try
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                SqlConnection conn = new SqlConnection(ReadConnectionString());

                clsEventAccount cEA = new clsEventAccount();
                cEA.sqlConn = conn;
                cEA.intAccount_pKey = data.Id;
                cEA.intEvent_pKey = data.EventId;
                DataTable dtStatus = clsEventAccount.getEventSpeakerANDChairPerson(data.EventId, data.Id);
                if (dtStatus != null && dtStatus.Rows.Count > 0)
                {
                    if (dtStatus.Rows[0]["IsSpeaker"].ToString().ToLower() == "true")
                        return Json(new { result = "Error", msg = "Cancellation of speaker or chairperson is not allowed." }, JsonRequestBehavior.AllowGet);
                }
                string strSubject = "", strContent = "", strAddress = "";

                clsAnnouncement Ann = clsAnnouncement.getAnnouncementInfo(new System.Data.SqlClient.SqlConnection(ReadConnectionString()), null, clsAnnouncement.Registration_CancellationSpeaker);
                strSubject = Ann.strTitle;
                strContent = Ann.strHTMLText;

                clsAccount cAccount = new clsAccount();
                clsEvent cEvent = new clsEvent();
                clsVenue cVenue = new clsVenue();
                strContent = strContent.Replace("[Cancellation_Comment]", cancelregion.Trim());

                if (strContent != "")
                {

                    clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                    cAccount.sqlConn = conn;
                    cAccount.intAccount_PKey = data.Id;
                    cAccount.LoadAccount();

                    strSubject = cAccount.ReplaceReservedWords(strSubject);
                    strContent = cAccount.ReplaceReservedWords(strContent);

                    cEvent.sqlConn = conn;
                    cEvent.intEvent_PKey = data.EventId;
                    cEvent.LoadEvent();
                    strSubject = cEvent.ReplaceReservedWords(strSubject);
                    strContent = cEvent.ReplaceReservedWords(strContent);

                    cVenue.sqlConn = conn;
                    cVenue.intVenue_PKey = cEvent.intVenue_PKey;
                    strSubject = cVenue.ReplaceReservedWords(strSubject);
                    strContent = cVenue.ReplaceReservedWords(strContent);

                    strContent = strContent.Replace("[Cancellation_Comment]", cancelregion.Trim());
                    clsEmail cEmail = new clsEmail();
                    cEmail.sqlConn = conn;
                    cEmail.strSubjectLine = strSubject;
                    cEmail.strHTMLContent = strContent;
                    strAddress = new SqlOperation().MyOptionsSecurityGroupEmail();
                    cEmail.strEmailCC = strAddress != "" ? strAddress : "#";

                    clsEventAccount cEventACcount = new clsEventAccount();
                    cEventACcount.sqlConn = conn;
                    cEventACcount.DeleteRegFeedBack(data.EventId, data.Id);

                    if (!cEmail.SendEmailToAddress(cSettings.strSupportEmail.ToString(), SenderEmailID: cAccount.strEmail, SenderName: cAccount.strContactName, Event_pkey: data.EventId, Announcement_pkey: clsAnnouncement.Registration_CancellationSpeaker))
                        return Json(new { result = "OK", msg = "Email not sent" }, JsonRequestBehavior.AllowGet);


                    cEmail = null;
                    return Json(new { result = "OK", msg = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

            }
            return Json(new { result = "Error", msg = "Error while updating cancellation" }, JsonRequestBehavior.AllowGet);

        }
        [CustomizedAuthorize]
        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult CancelProvisinaolRegister(double dblAccountBalance)
        {
            try
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                SqlConnection conn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                double dblTotalBalance = dblAccountBalance;

                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.sqlConn = conn;
                cEventAccount.lblMsg = null;
                cEventAccount.intEventAccount_pKey = data.EventId;
                cEventAccount.intAccount_pKey = data.Id;
                cEventAccount.intEvent_pKey = data.EventId;

                if (Math.Abs(cEventAccount.dblAccountBalance) != Math.Abs(dblAccountBalance))
                    return Json(new { result = "Invalid Account Balance" }, JsonRequestBehavior.AllowGet);

                clsAccount cAccount = new clsAccount();
                cAccount.intAccount_PKey = data.Id;
                cAccount.sqlConn = conn;
                cAccount.LoadAccount();
                int intAccount_PKey = cAccount.intAccount_PKey;
                int intEventAccount_pKey = data.Id;
                string strContactName = cAccount.strContactName;
                clsLastUsed cLast = (clsLastUsed)Session["cLastUsed"];
                string strEventID = cLast.strActiveEvent;
                string strEmail = cAccount.strEmail;
                bool isGroupReg = false;
                clsPayment.UpdateAccountPayment(intAccount_PKey, data.EventId, Convert.ToDecimal(Math.Abs(dblTotalBalance) == 0 ? Math.Abs(dblAccountBalance) : Math.Abs(dblTotalBalance)), isGroupReg);
                clsPayment.VoucherReverseCharges(intAccount_PKey, data.EventId, cAccount.intAccount_PKey);

                // '--delete the event account  

                if (cEventAccount.DeleteAccountFromEvent(false))
                {
                    cAccount.LogAuditMessage("Cancelled " + strContactName + " From Event " + strEventID, clsAudit.LOG_CancelRegister);
                }
                clsPayment.SendRegCancellationEmailFreePass(intAccount_PKey);
                return Json(new { result = "OK" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { result = "Error Occurred While Cancelling Registration" }, JsonRequestBehavior.AllowGet);
            }
        }

        [CustomizedAuthorize]
        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult PayementSubmit(PaymentSubmit Model)
        {
            try
            {
                bool bPaymentResult = false; int intReceiptNumber = 0; string strPaymentProblem = "";
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                SqlConnection Conn = new SqlConnection(ReadConnectionString());

                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.sqlConn = Conn;
                cEventAccount.lblMsg = null;
                cEventAccount.intAccount_pKey = data.Id;
                cEventAccount.intEvent_pKey = data.EventId;
                cEventAccount.LoadEventInfo(true);

                clsEvent cEvent = new clsEvent();
                double AccountBalance = Model.txtCreditAmount;
                double dblAcBalance = Math.Abs(Model.dblAccountBalance);
                string strCharges = string.Empty;
                string strChargesPkey = string.Empty;

                DataTable dtCharges = dba.PaymentType(data.Id, data.EventId);
                if (Math.Abs(cEventAccount.dblAccountBalance) == Math.Abs(dblAcBalance))
                    strCharges += string.Join(", ", dtCharges.Rows.OfType<DataRow>().Select(r => r["ChargeType_pKey"].ToString()));
                else
                    strCharges = Model.ckPaymentType;
                if (strCharges != null)
                {
                    string[] arrCharges = strCharges.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i <= arrCharges.Length - 1; i++)
                    {
                        if (arrCharges[i] == "1")
                            arrCharges[i] = clsPayment.GetOnlyRegistrationCharge();
                    }
                    strCharges = String.Join(",", arrCharges);
                    double calculatedAmount = 0;

                    DataTable dt = clsPayment.FillAccountChargeTypePkey(strCharges, data.Id, data.EventId);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        calculatedAmount = Convert.ToDouble(dt.Compute("Sum(Amount)", ""));
                        if (Math.Abs(calculatedAmount) == AccountBalance)
                            strChargesPkey = string.Join(",", dt.AsEnumerable().Select(i => i["pKey"]).ToArray());
                    }
                    cEvent.sqlConn = Conn;
                    cEvent.lblMsg = null;
                    cEvent.intEvent_PKey = data.EventId;
                    cEvent.LoadEvent();
                    string lblcharge = strCharges, lblSelectedAcctCharges = strChargesPkey;
                    string PaymentResult = SubmitPayment(cEvent, cEventAccount, Model, true, bPaymentResult, intReceiptNumber, strPaymentProblem, AccountBalance, lblcharge, lblSelectedAcctCharges);
                    string Redirect = "";

                    if (AccountBalance == dblAcBalance)
                        Redirect = "MyPayments";

                    return Json(new { msg = PaymentResult, redirect = Redirect }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;
                clsUtility.LogErrorMessage(null, request, GetType().Name, 0, "Error while updating payments : " + ex.Message);

            }
            return Json(new { msg = "Error While Updating Payment" }, JsonRequestBehavior.AllowGet);
        }
        private string SubmitPayment(clsEvent cEvent, clsEventAccount cEventAccount, PaymentSubmit Model, bool bEditable, bool bPaymentResult, int intReceiptNumber, string strPaymentProblem, double txtCreditAmount, string lblcharge, string lblSelectedAcctCharges)
        {
            try
            {
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                SqlConnection conn = new SqlConnection(ReadConnectionString());
                clsAccount cAccount = new clsAccount();
                cAccount.intAccount_PKey = Convert.ToInt32(User.Identity.Name);
                cAccount.sqlConn = conn;
                cAccount.LoadAccount();

                //'--process the payment
                clsPayment c = new clsPayment();
                c.sqlConn = conn;
                c.lblMsg = null;

                // '--prepare
                c.intPaymentMethod_pKey = clsPayment.METHOD_Credit;
                c.intPayerAcctPKey = cAccount.intAccount_PKey;
                c.intEventPKey = cEvent.intEvent_PKey;
                c.strEventID = cEvent.strEventID;
                c.strIntendedAccounts = cEventAccount.intAccount_pKey.ToString();
                c.intLoggedByAccount_pKey = cAccount.intAccount_PKey;

                int CardType = 0;
                if (!string.IsNullOrEmpty(Model.ddCardType))
                    CardType = Convert.ToInt32(Model.ddCardType);
                //  '--credit card
                c.strCardNumber = Model.txtCCNum;
                c.intCardType = CardType;
                c.strCardLastFour = c.strCardNumber.Substring(c.strCardNumber.Length - 4, 4); // Right 4 string
                c.strCardCode = ((string.IsNullOrEmpty(Model.txtCreditCode)) ? "" : Model.txtCreditCode);
                c.strCardname = ((string.IsNullOrEmpty(Model.txtCreditName)) ? "" : Model.txtCreditName);

                c.strCardFirstname = Model.txtCreditFirstName;
                c.strCardLastname = Model.txtCreditLastname;
                c.strCardZipcode = ((string.IsNullOrEmpty(Model.txtCreditZipcode)) ? "" : Model.txtCreditZipcode);
                c.strCardAddress = ((string.IsNullOrEmpty(Model.txtCreditAddress)) ? "" : Model.txtCreditAddress);

                int Month = 0, Year = 0; string strdate = "";
                if (!string.IsNullOrEmpty(Model.ddMonth))
                    Month = Convert.ToInt32(Model.ddMonth);

                if (!string.IsNullOrEmpty(Model.ddYear))
                    Year = Convert.ToInt32(Model.ddYear);

                if (Month > 0 && Year > 0)
                {
                    strdate = Model.ddMonth + "/" + "1" + "/" + Model.ddYear + " " + DateTime.Now.ToLongTimeString();
                    c.dtCardExpiration = Convert.ToDateTime(strdate).AddMonths(1).AddDays(-1);
                }
                else
                    c.dtCardExpiration = DateTime.MinValue;

                c.dblAmount = (bEditable) ? Model.txtCreditAmount : clsUtility.CleanCurrency(Model.lblCreditAmount);
                // More information about customer

                c.strCustomerId = cAccount.intAccount_PKey.ToString();
                c.strCustomerName = cAccount.strContactName;
                c.strCustomerAddress = cAccount.strAddress1 + " " + cAccount.strAddress2;
                c.strCustomerCompany = cAccount.strOrganizationID;
                c.strCustomerZip = cAccount.strZip;
                c.strCustomerFName = cAccount.strFirstname;
                c.strCustomerLName = cAccount.strLastname;
                c.strCustomerFax = cAccount.strFax;
                c.strCustomerEmail = cAccount.strEmail;
                c.strCustomerCity = cAccount.strCity;
                c.strCustomerState = cAccount.strState;
                c.strCustomerCountry = cAccount.strCountry;
                c.strCustomerPhone = cAccount.strPhone;
                c.strSelectedCharges = lblcharge;
                c.strSelectedChargesPkey = lblSelectedAcctCharges;
                if (c.dblAmount <= 0)
                    return "The entered amount must be a valid currency and > 0. Correct and try again. If problems persist, contact MAGI.";

                nsoftware.InPay.Icharge ICharge = new nsoftware.InPay.Icharge();
                nsoftware.InPay.Cardvalidator CardValidator = new nsoftware.InPay.Cardvalidator();

                //  '--do the posting
                bPaymentResult = c.PostPayment(iCharge1: ICharge, CardValidator1: CardValidator);
                intReceiptNumber = c.intReceiptNumber;
                strPaymentProblem = String.Join(", ", c.lstErrors.ToArray());

                if (!bPaymentResult)
                {
                    string strError = "Payment was unsuccessful due to: " + strPaymentProblem.TrimEnd() + ". Correct and try again. If problems persist, <a href='" + cSettings.strWebSiteURL + "/forms/SendEmail.aspx?E=1&S=MAGI+Support'  target='_blank'>contact MAGI</a>.";
                    strError = strError.Replace("This transaction has been declined. Correct and try again", "This transaction has been declined by your credit/debit card company. Tell them to allow the charge and then try again");
                    return strError;
                }
                //  '--log the payment
                c.strMemo = "Payment made";
                //'--force the payment to be 'paid' for manual postings
                c.bPaid = true;

                // '--save the payment to the log
                if (!c.LogPayment())
                    return cSettings.getErrorcode(227);

                //'--if paid (i.e. credit card), log the payment to the attendee account
                if (c.bPaid)
                {
                    if (!c.ApplyCashToAccounts(c.dblAmount, c.dblAmount))
                        return cSettings.getErrorcode(228);
                }

                //  '--audit
                cEventAccount.LogAuditMessage("Log payment on behalf of account: " + cEventAccount.intAccount_pKey.ToString() + " for event: " + cEvent.strEventID, clsAudit.LOG_Payment);
                c = null;

                return "OK";
            }
            catch (Exception ex)
            {
                System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;
                clsUtility.LogErrorMessage(null, request, GetType().Name, 0, "Error while updating payments : " + ex.Message);
                return "Error occurred while updating payments";
            }
        }


        [CustomizedAuthorize]
        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public ActionResult ProcessRefund(int voucherCode)
        {
            try
            {
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                DataTable VoucherTable = new DataTable();
                VoucherTable = repository.GetVoucherInfoByID(voucherCode, data.Id, data.Email);
                if (VoucherTable != null)
                {
                    clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                    DataRow dr = VoucherTable.Rows[0];
                    bool lblCCRefundErrorVisible = false;
                    string intVoucher_pKey = dr["pKey"].ToString();
                    double hdnPaymentDay = Convert.ToDouble(dr["PaymentDayDiff"].ToString());
                    string hdnCCRefundCardNo = dr["CardLastFour"].ToString();
                    string hdnCCMainAmount = dr["Amount"].ToString();
                    string strLastFourDigit = dr["CardLastFour"].ToString();

                    if (strLastFourDigit == "" || hdnPaymentDay < 0 || hdnPaymentDay > cSettings.intRefundExpirationDays)
                        return Json(new { msg = "To obtain a refund, contact support@magiworld.org." }, JsonRequestBehavior.AllowGet);

                    string lblCCRefundCardTran = dr["PaymentTransAction"].ToString();
                    string lblCCRefundAmt = "$" + dr["Amount"].ToString();
                    string lblReceiptNumber = "R" + dr["ReferenceReceipt"].ToString();
                    return Json(new { msg = "OK", lblCCRefundErrorVisible = lblCCRefundErrorVisible, intVoucher_pKey = intVoucher_pKey, hdnPaymentDay = hdnPaymentDay, strLastFourDigit = strLastFourDigit, lblCCRefundCardTran = lblCCRefundCardTran, lblCCRefundAmt = lblCCRefundAmt, lblReceiptNumber = lblReceiptNumber, hdnCCRefundCardNo = hdnCCRefundCardNo, hdnCCMainAmount = hdnCCMainAmount }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

            }
            return Json(new { msg = "Error While Fetching Refund Information" }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        [AjaxValidateAntiForgeryToken]
        [CustomizedAuthorize]
        public ActionResult SubmitRefundProcess(int intVoucher_pKey, string txtCCRefundReason)
        {
            try
            {
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                DataTable VoucherTable = new DataTable();
                VoucherTable = repository.GetVoucherInfoByID(intVoucher_pKey, data.Id, data.Email);
                if (VoucherTable != null)
                {
                    DataRow dr = VoucherTable.Rows[0];

                    string lblCCRefundCardTran = "", lblCCRefundAmt = "", lblReceiptNumber = "", strLastFourDigit = "", hdnCCRefundCardNo = "", hdnCCMainAmount = "";
                    double hdnPaymentDay = 0;

                    clsSettings cSettings = ((clsSettings)Session["cSettings"]);

                    hdnPaymentDay = Convert.ToDouble(dr["PaymentDayDiff"].ToString());
                    hdnCCRefundCardNo = dr["CardLastFour"].ToString();
                    hdnCCMainAmount = dr["Amount"].ToString();
                    strLastFourDigit = dr["CardLastFour"].ToString();

                    if (strLastFourDigit == "" || hdnPaymentDay < 0 || hdnPaymentDay > cSettings.intRefundExpirationDays)
                        return Json(new { msg = "To obtain a refund, contact support@magiworld.org." }, JsonRequestBehavior.AllowGet);

                    lblCCRefundCardTran = dr["PaymentTransAction"].ToString();
                    lblCCRefundAmt = dr["Amount"].ToString();
                    lblReceiptNumber = dr["ReferenceReceipt"].ToString();

                    if (lblCCRefundCardTran != "")
                    {
                        var intReceiptNumber = Convert.ToInt32(lblReceiptNumber);

                        if (intReceiptNumber == 0)
                            return Json(new { msg = "Select receipt number." }, JsonRequestBehavior.AllowGet);
                        if (lblCCRefundCardTran == "" || lblCCRefundCardTran == "0")
                            return Json(new { msg = "Transaction id should not be blank." }, JsonRequestBehavior.AllowGet);
                        //if (txtCCRefundReason == "")  -- Not Required As Per Brijendra sir 
                        //    return Json(new { msg = "Enter comment." }, JsonRequestBehavior.AllowGet);
                        if (Convert.ToDouble(lblCCRefundAmt.Replace("$", "")) > Convert.ToDouble(hdnCCMainAmount))
                            return Json(new { msg = "Refund amount should be less than or equal to paid amount." }, JsonRequestBehavior.AllowGet);



                        SqlConnection Conn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());

                        clsPayment c = new clsPayment();
                        c.sqlConn = Conn;
                        c.lblMsg = null;
                        c.dblAmount = Double.Parse(lblCCRefundAmt);
                        c.strCardLastFour = hdnCCRefundCardNo;
                        c.strRefundCardTransactionID = lblCCRefundCardTran;
                        c.intRefundReceiptNumber = intReceiptNumber;

                        if (!c.RefundCreditCardProcessor())
                            return Json(new { msg = "Error in processing, contact support@magiworld.org." }, JsonRequestBehavior.AllowGet);
                        else
                        {
                            repository.Update_AccountPayment(c.dblAmount, data.Id, intReceiptNumber, data.EventId, c.strRefundCardTransactionID, c.intRefundReceiptNumber, c.strResponseCardType);
                            clsVoucher cVoucher = new clsVoucher();
                            cVoucher.sqlConn = Conn;
                            cVoucher.intVoucher_pKey = intVoucher_pKey;

                            var cOrig = new clsVoucher();
                            cOrig.sqlConn = Conn;
                            cOrig.lblMsg = null;
                            cOrig.intVoucher_pKey = intVoucher_pKey;
                            cOrig.LoadVoucher();

                            cVoucher.strVoucherEmail = cOrig.strVoucherEmail;
                            cVoucher.intIssuedForEvent_pKey = cOrig.intIssuedForEvent_pKey;
                            cVoucher.dtExpirationDate = cOrig.dtExpirationDate;
                            cVoucher.intCancellationReason_pKey = cOrig.intCancellationReason_pKey;
                            cVoucher.strCancellationComment = cOrig.strCancellationComment;
                            cVoucher.intAccount_pkey = data.Id;
                            cVoucher.dblAmount = cOrig.dblAmount;
                            cVoucher.dtIssuedOn = cOrig.dtIssuedOn;
                            cVoucher.intReferenceReceipt = cOrig.intReferenceReceipt;
                            cVoucher.intPaymentMethod = cOrig.intPaymentMethod;
                            cVoucher.dblUsageAmount = Convert.ToDouble(lblCCRefundAmt.Replace("$", ""));
                            cVoucher.intUsedByAccount_pkey = data.Id;
                            cVoucher.dtUsedOn = DateTime.Now;
                            cVoucher.bIsUsed = true;
                            cVoucher.UpdatedByAccount_pkey = data.Id;
                            cVoucher.strComments = "Changed Status from " + cOrig.strVoucherStatus + " to Refunded."; // Logical Issue Not Saving User Given Comments 
                            if (!cVoucher.SaveVoucher())
                                return Json(new { msg = "Error in processing, contact support@magiworld.org." }, JsonRequestBehavior.AllowGet);

                            clsVoucher.SendVoucherCCRefundEmail(data.Id, intVoucher_pKey, Convert.ToInt32(cVoucher.dblAmount), cOrig.intIssuedForEvent_pKey.ToString(), intReceiptNumber, c.strResponseCardType, c.strCardLastFour);

                            return Json(new { msg = "OK", alertMsg = "Amount has been refunded." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;
                clsUtility.LogErrorMessage(null, request, GetType().Name, 0, "Error while processing refund : " + ex.Message);
            }
            return Json(new { msg = "Error in processing refund" }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region MyBook

        private const int DOC_Faculty = -2;
        private const int DOC_Schedule = -3;
        private const int DOC_Meals = -4;
        private const int DOC_Cert = -5;
        private string ReplaceReservedWords(string strorig, int intTotalPages, clsEvent cEvent)
        {
            string s = clsReservedWords.ReplaceMyPageText(null, strorig, cEvent);
            s = s.Replace("[NumPages]", intTotalPages.ToString());
            return s;
        }

        [CustomizedAuthorize]
        public ActionResult MyConferenceBook()
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            MyConferenceBookPage MyBookPageData = new MyConferenceBookPage();
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(((FormsIdentity)User.Identity).Ticket.UserData);
            SqlConnection conn = new SqlConnection(ReadConnectionString());
            DateTime dtCurrentTime;

            if (User.Identity.AuthenticationType == "Forms")
            {
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);
            }

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = data.EventId;
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();
            dtCurrentTime = clsEvent.getEventVenueTime();
            ViewBag.CurrentTime = dtCurrentTime;
            string str = cSettings.getText(clsSettings.TEXT_MyBook);

            clsEventAccount cEventAccount = new clsEventAccount();
            cEventAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEventAccount.intEvent_pKey = data.EventId;
            cEventAccount.intAccount_pKey = data.Id;
            cEventAccount.LoadEventInfo(true);
            ViewBag.WorkshopVisible =false;
            ViewBag.PageTitle = cEvent.strEventFullname + ": My Personalized Book";
            ViewBag.lblAttName = data.FirstName + " " + data.LastName;
            ViewBag.LblInstruction = clsReservedWords.ReplaceMyPageText(null, str, cEvent: cEvent, intEvtPKey: data.EventId);
            ViewBag.bCurrentAttendee = (cEventAccount.intParticipationStatus_pKey == clsEventAccount.PARTICIPATION_Attending);
            ViewBag.bBalanceDue = (cEventAccount.dblAccountBalance < 0);
            ViewBag.dblAccountAmount = cEventAccount.dblAccountBalance;
            ViewBag.leftPanel_Visible = cEvent.bChatPanelOnOff;
            ViewBag.NotificationTips = true;
            ViewBag.bBooksAvailable = (clsEvent.getCaliforniaTime() >= cEvent.dtBookStartDate && clsEvent.getCaliforniaTime() <= cEvent.dtBookEndDate.AddDays(1).Date);

            bool showCert = cEvent.CheckValiditityOfModule(data.EventId, "ShowAttCerts");

            if (cEvent.bShowAttCerts && showCert)
                ViewBag.lblCertificateInfo = "A certificate showing your attendance With educational credits per your session feedback";
            else
                ViewBag.lblCertificateInfo = "A certificate showing your attendance with educational credits per your session feedback (not yet available)";

            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            ViewBag.lblSection = "Click Buttons to Download Individual Sessions and Workshops";
            ViewBag.lblFrontSecVisible = true;
            ViewBag.pnlBookVisible = true;
            ViewBag.lblSection1 = "Click Buttons to Download Individual Sessions and Workshops section";

            bool bAnyFound = false;

            Prepare(data.EventId, cEvent.strEventID, data.Id);

            var dtable = dba.Sessions_and_FixedHeader(data.EventId, data.Id.ToString(), 1, 1, cEventAccount.bHasSpecialMeal, cEventAccount.intEventAccount_pKey.ToString(), cEvent.intEvent_PKey.ToString(), cSettings.IsMealshow);
            var dtable1 = dba.Sessions_and_FixedHeader(data.EventId, data.Id.ToString(), 0, 1, cEventAccount.bHasSpecialMeal, cEventAccount.intEventAccount_pKey.ToString(), cEvent.intEvent_PKey.ToString(), cSettings.IsMealshow);

            string strSpeakerFilename = cEvent.strEventID + "_Faculty_Bios_Img.pdf";
            string strSpeakerTargetFile = Server.MapPath("~/Documents/" + strSpeakerFilename);
            string strScheduleFilename = cEvent.strEventID + "_" + data.Id.ToString() + "_Sched.pdf";
            string strScheduleTargetFile = Server.MapPath("~/UserDocuments/" + strScheduleFilename);
            string strMealFilename = cEvent.strEventID + "_" + data.Id.ToString() + "_Meal.pdf";
            string strMealTargetFile = Server.MapPath("~/UserDocuments/" + strMealFilename);
            string strCertFilename = cEvent.strEventID + "_" + data.Id.ToString() + "_Cert.pdf";
            string strCertTargetFile = Server.MapPath("~/UserDocuments/" + strCertFilename);

            double sngTotalSize = 0, lblSizeSpk = 0, lblSizeSched = 0, lblSizeCert = 0, lblSizeMeal = 0;
            int intTotalPages = 0;
            ViewBag.WorkshopVisible1 = false;
            foreach (DataRow dr in dtable.Rows)
            {
                int intEvtSessionPKey = Convert.ToInt32(dr["EvtSessionPKey"]);
                int intFilePages = Convert.ToInt32(dr["NumPages"]);
                double dblFileSize = Convert.ToDouble(dr["FSize"]);
                bool bAttending = (dr["IsAttending"].ToString() == "Y");
                string strSessionID = dr["SessionID"].ToString();
                string strTitle = dr["SessionTitle"].ToString();

                string strPhysical = "";

                switch (intEvtSessionPKey)
                {
                    case DOC_Faculty:
                        strPhysical = strSpeakerTargetFile;
                        dblFileSize = 1.15;
                        intFilePages = 28;
                        break;
                    case DOC_Schedule:
                        strPhysical = strScheduleTargetFile;
                        break;
                    case DOC_Meals:
                        strPhysical = strMealTargetFile;
                        dblFileSize = 0.25;
                        intFilePages = 1;
                        break;
                    case DOC_Cert:
                        strPhysical = strCertTargetFile;
                        dblFileSize = 0.25;
                        intFilePages = 1;
                        break;
                    default:
                        strPhysical = Server.MapPath("~/SessionDocuments/" + cEvent.strEventID + "_" + dr["SessionID"] + ".pdf");
                        break;
                }

                if (clsUtility.FileExists(strPhysical))
                {
                    bAnyFound = true;
                    dr["FExists"] = true;
                    dr["DisplaySize"] = String.Format("{0:N2}", dblFileSize);

                    if (intEvtSessionPKey < 0)
                    {
                        sngTotalSize = sngTotalSize + dblFileSize;
                        intTotalPages = intTotalPages + intFilePages;

                        switch (intEvtSessionPKey)
                        {
                            case DOC_Faculty:
                                lblSizeSpk = dblFileSize;
                                break;
                            case DOC_Schedule:
                                lblSizeSched = dblFileSize;
                                break;
                            case DOC_Cert:
                                lblSizeCert = dblFileSize;
                                break;
                            case DOC_Meals:
                                //lblSizeMeal = dblFileSize;
                                break;
                        }
                    }
                }

            }
            ViewBag.bShowPanel = ViewBag.bBooksAvailable;
            ViewBag.lblNotAvailable = "";
            ViewBag.cmdPayNow = false;
            ViewBag.Alert = false;
            if (data.GlobalAdmin)
                ViewBag.bShowPanel = true;
            else
            {
                if (ViewBag.bBooksAvailable)
                    ViewBag.bShowPanel = ViewBag.bBooksAvailable;
                else
                {
                    ViewBag.Alert = true;
                    ViewBag.lblNotAvailable = "";
                    ViewBag.cmdPayNow = false;
                    ViewBag.bShowPanel = false;
                }
            }
            ViewBag.phNotAvailable = !ViewBag.bShowPanel;
            ViewBag.phIsAvailable = ViewBag.bShowPanel;
            if (ViewBag.phIsAvailable)
            {
                if (!ViewBag.bBooksAvailable)
                {
                    if ((!ViewBag.bBooksAvailable) && ViewBag.bBalanceDue)
                        ViewBag.lblNotAvail = ReplaceReservedWords(cSettings.getText(clsSettings.TEXT_MyBookNotAvailableAll), intTotalPages, cEvent);
                    else if (ViewBag.bBooksAvailable && ViewBag.bBalanceDue)
                        ViewBag.lblNotAvail = ReplaceReservedWords(cSettings.getText(clsSettings.TEXT_MyBookNotAvailableBalanceDue), intTotalPages, cEvent);
                    else if ((!ViewBag.bBalanceDue) && (!ViewBag.bBooksAvailable))
                        ViewBag.lblNotAvail = ReplaceReservedWords(cSettings.getText(clsSettings.TEXT_MyBookNotAvailableNotReleased), intTotalPages, cEvent);
                    else if ((ViewBag.bBooksAvailable) && (!ViewBag.bCurrentAttendee))
                    {
                        ViewBag.phNotAvailable = false;
                        ViewBag.lblNotAvail = "You are not Attendee of this event.";
                    }
                }

                ViewBag.lblBookSize = String.Format("{0:N2}", sngTotalSize);
                ViewBag.lblSizeSpk = String.Format("{0:N2}", lblSizeSpk);
                ViewBag.lblSizeSched = String.Format("{0:N2}", lblSizeSched);
                ViewBag.lblSizeCert = String.Format("{0:N2}", lblSizeCert);
                //ViewBag.lblSizeMeal = String.Format("{0:N2}", lblSizeMeal);
                ViewBag.lblSizeMeal = lblSizeMeal;

                DataTable dtSession = new DataTable();
                DataRow[] drList = dtable.Select("EvtSessionPKey > 0");
                if (drList.Length > 0)
                {
                    dtSession = drList.OrderBy(row => row["SessionID"]).CopyToDataTable();
                    ViewBag.WorkshopVisible = true;
                }
                MyBookPageData.SessionBookTable = dtSession;

                foreach (DataRow dr in dtable1.Rows)
                {
                    int intEvtSessionPKey = Convert.ToInt32(dr["EvtSessionPKey"]);
                    int intFilePages = Convert.ToInt32(dr["NumPages"]);
                    double dblFileSize = Convert.ToDouble(dr["FSize"]);
                    bool bAttending = (dr["IsAttending"].ToString() == "Y");
                    string strSessionID = dr["SessionID"].ToString();
                    string strPhysical = "";
                    switch (intEvtSessionPKey)
                    {
                        case DOC_Faculty:
                            strPhysical = strSpeakerTargetFile;
                            dblFileSize = 1.15;
                            intFilePages = 28;
                            break;
                        default:
                            strPhysical = Server.MapPath("~/SessionDocuments/" + cEvent.strEventID + "_" + dr["SessionID"] + ".pdf");
                            break;
                    }

                    if (clsUtility.FileExists(strPhysical))
                    {
                        bAnyFound = true;
                        dr["FExists"] = true;
                        dr["DisplaySize"] = String.Format("{0:N2}", dblFileSize);

                        if (intEvtSessionPKey < 0)
                        {
                            sngTotalSize = sngTotalSize + dblFileSize;
                            intTotalPages = intTotalPages + intFilePages;
                            switch (intEvtSessionPKey)
                            {
                                case DOC_Faculty:
                                    lblSizeSpk = dblFileSize;
                                    break;
                            }
                        }
                    }

                }

                ViewBag.lblSizeSpk1 = String.Format("{0:N2}", lblSizeSpk);
                DataTable dtSession1 = new DataTable();
                DataRow[] drList1 = dtable1.Select("EvtSessionPKey > 0");
                
                if (drList1.Length > 0)
                {
                    dtSession1 = drList1.OrderBy(row => row["SessionID"]).CopyToDataTable();
                    ViewBag.WorkshopVisible1 = true;
                }
                MyBookPageData.SessionBookTableNonAttend = dtSession1;
            }
            var booklet = dba.Booklet(data.EventId);

            string intRegistrationLevel_pKey = "";
            int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);
            int RegLevelPKey = Convert.ToInt32(intRegistrationLevel_pKey);
            ViewBag.leftPanel_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
            ViewBag.VirtualDropdown_Visible = ViewBag.leftPanel_Visible;
            dtCurrentTime = clsEvent.getEventVenueTime();
            if (ViewBag.VirtualDropdown_Visible)
            {
                DateTime dtCalTime = clsEvent.getCaliforniaTime();
                string Host = HttpContext.Request.Path.ToUpper().Replace("/MYMAGI", "");
                ViewBag.ddEventVirtualData = repository.GetVirtualEventDropDownInfo(Host, data.Id, data.EventId, dtCurrentTime, dtCalTime, data.ParentOrganization_pKey, intAttendeeStatus, RegLevelPKey, data.GlobalAdmin, data.StaffMember);
                ViewBag.SelectedDropDown = GetSelectedVirtualDropDown(Host);
            }
            int IntVal = 0;
            clsJob cjob = new clsJob();
            IntVal = cjob.Job_List_Save(cEvent.intEvent_PKey, data.Id, JobTitle: ("Attending" == "Attending" ? "Rebuild Book" : "Rebuild Book" + " (non-attending)"), JobType_Pkey: ("Attending" == "Attending" ? 1 : 2), Email: data.Email.ToString(), Name: data.FirstName.ToString(), chk: true);
            if (IntVal == 1)
                ViewBag.btnBookEnabled = false;
            else
                ViewBag.btnBookEnabled = true;

            RefreshFiles("Attending", cSettings.bBookCreation, data.EventId, data.Id);
            ViewBag.CertificatebtnVisible = (cEvent.bShowAttCerts && showCert);
            ViewBag.phSpecialMeal = (cEventAccount.bHasSpecialMeal && (cSettings.IsMealshow == true));
            string s = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.TEXT_MyBookInstruct), cEvent);
            ViewBag.FormTextHelp_Ins = s;

            bool bShowSurveyQuestion = false;
            DataTable EventFeatures = repository.getDyamicEventSettings(data.EventId, "ISNULL(ShowSurveyQuestion,'0') as ShowSurveyQuestion");
            if (EventFeatures != null && EventFeatures.Rows.Count > 0)
                bShowSurveyQuestion = (EventFeatures.Rows[0]["ShowSurveyQuestion"] != System.DBNull.Value) ? Convert.ToBoolean(EventFeatures.Rows[0]["ShowSurveyQuestion"]) : false;

            int selectedEvent = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).intEventSelector;
            bool bSponsor = clsEventOrganization.CheckExhibitor(data.ParentOrganization_pKey, selectedEvent);
            ViewBag.lblRegText = "";
            ViewBag.OpenSurveyRadWindow=false;
            LoadRegistrationQuestions(data, intAttendeeStatus, bShowSurveyQuestion, bSponsor, intRegistrationLevel_pKey);

            return View(MyBookPageData);
        }

        [CustomizedAuthorize]
        public JsonResult TabChanged(string tab)
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = data.EventId;
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();

            string lblSection = "";
            int intTab = 0;
            if (tab.Trim() != "Attending")
            {
                intTab = 1;

            }

            if (intTab == 0)
            {
                lblSection = "Click Buttons to Download Individual Sessions and Workshops";

            }
            else
            {
                lblSection = "Click Buttons to Download Individual Sessions and Workshops section";

            }
            int IntVal = 0; // note- temprary set as 0  below line need to be coorect;
            clsJob cjob = new clsJob();
            IntVal = cjob.Job_List_Save(cEvent.intEvent_PKey, data.Id, JobTitle: (tab == "Attending" ? "Rebuild Book" : "Rebuild Book" + " (non-attending)"), JobType_Pkey: (tab == "Attending" ? 1 : 2), Email: data.Email.ToString(), Name: data.FirstName.ToString());
            bool btnBookEnable = true;
            if (IntVal == 1)
            {
                btnBookEnable = false;

            }

            RefreshFiles(tab, cSettings.bBookCreation, EvtPKey, data.Id);

            return Json(new { result = "OK", TextlblSection = lblSection, BtnBook = btnBookEnable }, JsonRequestBehavior.AllowGet);
        }


        [CustomizedAuthorize]
        public ActionResult DownloadBooklet(string tab)
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);

            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();

            Aspose.Pdf.Document pdfBooklet = clsUtility.CreateNewPDF("Booklet - " + EvtPKey, false);
            string strBookletPath = Server.MapPath("~/BookDocuments/" + "Booklet_" + EvtPKey + ".pdf");

            var dtBooklet = dba.Booklet(EvtPKey);

            if (dtBooklet.Rows.Count > 0)
            {
                foreach (DataRow dr in dtBooklet.Rows)
                {
                    string FileName = dr["FileName"].ToString();
                    string strPhysicalPath = Server.MapPath("~/Documents/" + FileName);
                    if (System.IO.File.Exists(strPhysicalPath) && Path.GetExtension(FileName) == ".pdf")
                    {
                        Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(strPhysicalPath);
                        pdfBooklet.Pages.Add(pdf.Pages);
                    }
                }
            }
            try
            {
                Aspose.Pdf.Document.OptimizationOptions optimizer = new Aspose.Pdf.Document.OptimizationOptions();
                optimizer.LinkDuplcateStreams = true;
                optimizer.RemoveUnusedObjects = true;
                optimizer.CompressImages = true;
                optimizer.ImageQuality = 30;
                pdfBooklet.OptimizeResources(optimizer);


            }
            catch (Exception ex)
            {

            }


            if (System.IO.File.Exists(strBookletPath))
            {
                System.IO.File.Delete(strBookletPath);
                pdfBooklet.Save(strBookletPath);
                //clsUtility.InjectAlert(ScriptManager.GetCurrent(null), null, "Event Booklet is downloading");
                string filename = "booklet";
                filename = Path.GetFileName(strBookletPath);

                return Json(new { result = "OK", fname = filename }, JsonRequestBehavior.AllowGet); ;

            }
            else
            {
                //clsUtility.InjectAlert(ScriptManager.GetCurrent(Me.Page), Me.Page, "No booklet available");
                return Json(new { result = "Error", Error = "No booklet available" }, JsonRequestBehavior.AllowGet);
            }


        }

        [CustomizedAuthorize]
        public FileResult DownloadBookletFile(string FileName)
        {
            string strScheduleTargetFile = Server.MapPath("~/BookDocuments/" + FileName.Trim());
            if (System.IO.File.Exists(strScheduleTargetFile))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(strScheduleTargetFile);
                return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, FileName);
            }
            return null;
        }

        [CustomizedAuthorize]
        public ActionResult btnDownloadBook(string tab, string btnBuildBookText)
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);

            clsEventAccount cEventAccount = new clsEventAccount();
            cEventAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEventAccount.intEvent_pKey = EvtPKey;
            cEventAccount.intAccount_pKey = data.Id;
            cEventAccount.LoadEventInfo(true);

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();

            string strTargetFilename = "";
            string strTargetFilePath = "";

            bool bAttendingBook = false; // (Me.RadTab.SelectedIndex = TAB_Attending);
            bAttendingBook = (tab.Trim() == "Attending");

            if (bAttendingBook)
            {
                strTargetFilename = "MyBook_" + "Attending_" + cEvent.intEvent_PKey.ToString() + "_" + data.Id.ToString() + ".zip";
            }
            else
            {
                strTargetFilename = "MyBook_" + cEvent.intEvent_PKey.ToString() + "_" + data.Id.ToString() + ".zip";
            }
            strTargetFilePath = Server.MapPath("~/BookDocuments/" + strTargetFilename);
            if (clsUtility.FileExists(strTargetFilePath))
            {
                if (bAttendingBook)
                {
                    cEventAccount.LogAuditMessage("Download: 'Attending' Conference Book - " + cEvent.strEventID);
                }
                else
                {
                    cEventAccount.LogAuditMessage("Download: 'Slides Only' Conference Book - " + cEvent.strEventID);
                }

                if (clsUtility.FileExists(strTargetFilePath))
                {
                    return Json(new { result = "OK", fname = strTargetFilename }, JsonRequestBehavior.AllowGet);
                }


            }
            else
            {
                string checkString = CreateBook(tab, data.Id, EvtPKey, cEvent.strEventID, cEvent.strFileGUID, cEvent.strEventFullname);
                if (checkString == "")
                {
                    RefreshFiles(tab, cSettings.bBookCreation, EvtPKey, data.Id);
                    return Json(new { result = "OK", fname = strTargetFilename }, JsonRequestBehavior.AllowGet);
                }
            }
            return null;

        }
        [CustomizedAuthorize]
        public FileResult DownloadBookFile(string FileName)
        {
            string strScheduleTargetFile = Server.MapPath("~/BookDocuments/" + FileName.Trim());
            if (System.IO.File.Exists(strScheduleTargetFile))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(strScheduleTargetFile);
                return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, FileName);
            }
            return null;
        }

        [CustomizedAuthorize]
        public ActionResult _BuildBook(string tab, string btnBuildBookText)
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);

            clsEventAccount cEventAccount = new clsEventAccount();
            cEventAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEventAccount.intEvent_pKey = EvtPKey;
            cEventAccount.intAccount_pKey = data.Id;
            cEventAccount.LoadEventInfo(true);

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();

            string strEmail = cSettings.strEmailAddressError;
            string strDisplayName = cSettings.strEmailDefaultDisplayName;
            string strSenderEmail = cSettings.strEmailDefaultMessagingAddress;

            string strTargetFilename = "";
            string strTargetFilePath = "";


            bool bAttendingBook = false;

            if (tab.Trim() == "Attending")
            {
                bAttendingBook = true;
            }

            string apiKey = (cSettings.strSendGridApiKey != "") ? cSettings.strSendGridApiKey : ConfigurationManager.AppSettings["SendGridApiKey"];
            //Me.cmdBook.Enabled = False
            //Me.cmdBook.CssClass = "btnSmall btnDisabled"

            int IntVal = 0;
            clsJob cjob = new clsJob();
            IntVal = cjob.Job_List_Save(cEvent.intEvent_PKey, data.Id, JobTitle: (tab == "Attending" ? btnBuildBookText : btnBuildBookText + " (non-attending)"), JobType_Pkey: (tab == "Attending" ? 1 : 2), Email: data.Email.ToString(), Name: data.FirstName.ToString());

            if (IntVal == 1)
            {
                //  clsUtility.InjectAlert(ScriptManager.GetCurrent(Me.Page), Me.Page, "The process of creating your personalized book is underway.");
                //  return Json(new { result = "", message = "The process of creating your personalized book is underway." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                // clsUtility.InjectAlert(ScriptManager.GetCurrent(Me.Page), Me.Page, "We will send you an email when your personalized book is ready.");              
                return Json(new { message = "We will send you an email when your personalized book is ready." }, JsonRequestBehavior.AllowGet);
            }
            if (bAttendingBook)
                strTargetFilename = "MyBook_" + "Attending_" + cEvent.intEvent_PKey.ToString() + "_" + data.Id.ToString() + ".zip";
            else
            {
                strTargetFilename = "MyBook_" + cEvent.intEvent_PKey.ToString() + "_" + data.Id.ToString() + ".zip";
            }

            //  strTargetFilePath = Server.MapPath("~/BookDocuments/" + strTargetFilename);
            // string strScheduleSessionTypes = ((clsSettings)Session["cSettings"]).strScheduleSessionTypes;
            //cjob.CreateBook(strScheduleSessionTypes, cEventAccount, cEvent, false);  //
            //string checkString = "";
            //if (tab == "Attending")
            //{
            //    checkString = cjob.CreateBook(strScheduleSessionTypes, cEventAccount, cEvent, true);
            //}
            //else
            //{

            //}
            string checkString = CreateBook(tab, data.Id, EvtPKey, cEvent.strEventID, cEvent.strFileGUID, cEvent.strEventFullname);

            if (checkString == "")
            {
                RefreshFiles(tab, cSettings.bBookCreation, EvtPKey, data.Id);
                return Json(new { message = "Your book has been built.You can now download it from your browser." }, JsonRequestBehavior.AllowGet);
                //clsUtility.InjectAlert(ScriptManager.GetCurrent(Me.Page), Me.Page, "Your book has been built. You can now download it from your browser.");
            }
            else
            {
                // clsEmail.SendErrorEmail(strSenderEmail, strDisplayName, strEmail, "Entity:" + cEventAccount.intEventAccount_pKey.ToString() + "(#" + data.Id.ToString() + "), Error Message:" + checkString, apiKey);
                return Json(new { message = "Contact support at support@magiworld.org for assistance." }, JsonRequestBehavior.AllowGet);
                //clsUtility.InjectAlert(ScriptManager.GetCurrent(Me.Page), Me.Page, "Contact support at support@magiworld.org for assistance.");

            }

        }
        int intCertificatePage = 0;
        int intEventCoverPage = 0;

        [CustomizedAuthorize]
        public string CreateBook(string tab, int AccountPkey, int EventPkey, string strEventId, string strFileGUID, string strEventFullname)
        {
            string CreateBook = "";
            //'---------------------------------
            // '--prepare TOC
            //'---------------------------------
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);


            clsEventAccount cEventAccount = new clsEventAccount();
            cEventAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEventAccount.intEvent_pKey = EvtPKey;
            cEventAccount.intAccount_pKey = data.Id;
            cEventAccount.LoadEventInfo(true);

            // string strTempFilePath = "";

            try
            {
                string strTargetFilename = "";
                string strTempFilePath = "";
                string strTargetFilePath = "";

                string strScheduleFilename = strEventId + "_" + AccountPkey.ToString() + "_Sched.pdf";
                string strScheduleTargetFile = Server.MapPath("~/UserDocuments/" + strScheduleFilename);
                string strSpeakerFilename = strEventId + "_Faculty_Bios_Img.pdf";
                string strSpeakerTargetFile = Server.MapPath("~/Documents/" + strSpeakerFilename);


                string strMealFilename = strEventId + "_" + AccountPkey.ToString() + "_Meal.pdf";
                string strMealTargetFile = Server.MapPath("~/UserDocuments/" + strMealFilename);

                string strCertFilename = strEventId + "_" + AccountPkey.ToString() + "_Cert.pdf";
                string strCertTargetFile = Server.MapPath("~/UserDocuments/" + strCertFilename);


                bool bAttendingBook = false; // (Me.RadTab.SelectedIndex = TAB_Attending)
                int tabindex = 1;
                if (tab.Trim() == "Attending")
                {
                    bAttendingBook = true;
                    tabindex = 0;
                }

                int intNextPageNum = 2; //'--assume TOC is one page so we start counting for the table of contents at page 2
                Aspose.Pdf.Table pdfTable = CreateNewTable("45 480", 95, true);


                //'--add header

                Aspose.Pdf.Row pdfTableHeader = pdfTable.Rows.Add();

                AddCellToRow(pdfTableHeader, bHighlight: true);
                AddCellToRow(pdfTableHeader, bAlignLeft: true, bHighlight: true);
                Aspose.Pdf.Text.TextFragment h1 = new Aspose.Pdf.Text.TextFragment("Contents");
                h1.TextState.FontSize = 10;
                h1.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Bold;
                h1.TextState.Font = Aspose.Pdf.Text.FontRepository.FindFont("Verdana");


                Aspose.Pdf.Text.TextFragment h2 = new Aspose.Pdf.Text.TextFragment("Page");
                h2.TextState.FontSize = 10;
                h2.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Bold;
                h2.TextState.Font = Aspose.Pdf.Text.FontRepository.FindFont("Verdana");


                pdfTableHeader.Cells[0].Paragraphs.Add(h2);
                pdfTableHeader.Cells[1].Paragraphs.Add(h1);


                //        // '------------------------------------
                //        // '--define work area
                //        // '------------------------------------
                if (bAttendingBook)
                {
                    strTargetFilename = "MyBook_" + "Attending_" + EventPkey.ToString() + "_" + AccountPkey.ToString() + ".zip";
                    strTempFilePath = Server.MapPath("~/app_data/BookPrepTemp/WorkArea/MyBook_" + "Attending_" + EventPkey.ToString() + "_" + AccountPkey.ToString() + ".pdf");
                }
                else
                {
                    strTargetFilename = "MyBook_" + EventPkey.ToString() + "_" + AccountPkey.ToString() + ".zip";
                    strTempFilePath = Server.MapPath("~/app_data/BookPrepTemp/WorkArea/MyBook_" + EventPkey.ToString() + "_" + AccountPkey.ToString() + ".pdf");
                }

                strTargetFilePath = Server.MapPath("~/BookDocuments/" + strTargetFilename);

                //    '------------------------------------
                //    '--create an empty file
                //    '------------------------------------
                Aspose.Pdf.Document pdfBook = clsUtility.CreateNewPDF("My Conference Book -" + strEventId + " " + (bAttendingBook ? "(Attending)" : "(Slides Only)"), false);

                //    '------------------------------------
                //    '--insert a coverpage
                //    '------------------------------------
                string strCoverPath = Server.MapPath("~/UserDocuments/" + strFileGUID + "_EventCoverPage.pdf");
                if (clsUtility.FileExists(strCoverPath))
                {
                    Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(strCoverPath);
                    pdfBook.Pages.Add(pdf.Pages);
                    intEventCoverPage = 1;
                    intNextPageNum = intNextPageNum + pdf.Pages.Count;
                }

                //    '------------------------------------
                //    '--insert a schedule
                //    '------------------------------------
                if (bAttendingBook)
                {
                    if (clsUtility.FileExists(strScheduleTargetFile))
                    {
                        Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(strScheduleTargetFile);
                        pdfBook.Pages.Add(pdf.Pages);
                        AddEntryToTOC(pdfTable, intNextPageNum, "Schedule");
                        intNextPageNum = intNextPageNum + pdf.Pages.Count;
                    }
                }

                //    '------------------------------------
                //    '--append the speaker list
                //    '------------------------------------

                if (clsUtility.FileExists(strSpeakerTargetFile))
                {
                    Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(strSpeakerTargetFile);
                    pdfBook.Pages.Add(pdf.Pages);
                    AddEntryToTOC(pdfTable, intNextPageNum, "Speaker Profiles");
                    intNextPageNum = intNextPageNum + pdf.Pages.Count;
                }

                //    '------------------------------------
                //    '--append the certificate
                //    '------------------------------------
                if (bAttendingBook)
                {
                    if (clsUtility.FileExists(strCertTargetFile))
                    {
                        Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(strCertTargetFile);

                        pdfBook.Pages.Add(pdf.Pages);
                        pdfBook.Pages[pdfBook.Pages.Count].PageInfo.IsLandscape = true;
                        AddEntryToTOC(pdfTable, intNextPageNum, "Certificate of Attendance");
                        intCertificatePage = intNextPageNum;
                        intNextPageNum = intNextPageNum + pdf.Pages.Count;
                    }
                }

                //    '------------------------------------
                //    '--append the meal sheet if any
                //    '------------------------------------


                if (bAttendingBook && cEventAccount.bHasSpecialMeal)
                {
                    if (clsUtility.FileExists(strMealTargetFile))
                    {
                        Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(strMealTargetFile);
                        pdfBook.Pages.Add(pdf.Pages);
                        AddEntryToTOC(pdfTable, intNextPageNum, "Special Meal Card");
                        intNextPageNum = intNextPageNum + pdf.Pages.Count;
                    }
                }

                //    '------------------------------------
                //    '--append the selected files to the first file
                //    '------------------------------------

                var dtable = dba.Sessions_and_FixedHeader(EventPkey, AccountPkey.ToString(), tabindex, 0, cEventAccount.bHasSpecialMeal, cEventAccount.intEventAccount_pKey.ToString(), EventPkey.ToString(),cSettings.IsMealshow);
                DataTable dtSession = new DataTable();
                DataRow[] drList = dtable.Select("EvtSessionPKey > 0");
                if (drList.Length > 0)
                {
                    dtSession = drList.OrderBy(row => row["SessionID"]).CopyToDataTable();
                }

                foreach (DataRow itm in dtSession.Rows)
                {
                    // Dim lblBookCert As Label = itm.FindControl("lblBookCert")

                    string strSessionTitle = itm["SessionTitle"].ToString();  // string strSessionTitle  = lblBookCert.Text
                    string strEvtSessionInfo = itm["EvtSessionInfo"].ToString();  // cmd.CommandArgument.ToString                          
                    int intPosn = strEvtSessionInfo.IndexOf("-");  //  Dim intPosn As Integer = InStr(strEvtSessionInfo, "-");

                    string strSessionID = Mid(strEvtSessionInfo, intPosn + 1);
                    string strFilename = strEventId + "_" + strSessionID + ".pdf";
                    if (strFilename != "")
                    {
                        string strPhysicalPath = Server.MapPath("~/SessionDocuments/") + strFilename;
                        if (clsUtility.FileExists(strPhysicalPath))
                        {
                            Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(strPhysicalPath);
                            pdfBook.Pages.Add(pdf.Pages);
                            AddEntryToTOC(pdfTable, intNextPageNum, "(" + strSessionID + ") " + strSessionTitle);
                            intNextPageNum = intNextPageNum + pdf.Pages.Count;
                        }
                    }
                }

                //    '------------------------------------
                //    '--insert a table of contents TOC
                //    '------------------------------------
                Aspose.Pdf.Page pdfPage = pdfBook.Pages.Insert(intEventCoverPage + 1);
                //  '--add logo
                AddLogoToPage(pdfPage, -1, 742);
                //   '--heading
                clsUtility.PDFAddTextToPage(pdfBook, pdfPage, strEventFullname, 50, 714, "Verdana", 14, System.Drawing.Color.Black, System.Drawing.Color.White, false, true, true);
                clsUtility.PDFAddTextToPage(pdfBook, pdfPage, "Table of Contents", 50, 700, "Verdana", 10, System.Drawing.Color.Black, System.Drawing.Color.White, false, true, true);
                clsUtility.PDFAddTextToPage(pdfBook, pdfPage, (bAttendingBook ? "'Attending'" : "'Slides Only'") + " as of " + String.Format("{0:MMMM dd, yyyy}", DateTime.Now), 50, 684, "Verdana", 10, System.Drawing.Color.Black, System.Drawing.Color.White, false, true, true);
                pdfPage.Paragraphs.Add(pdfTable);
                //    'pdfPage.Paragraphs.Add(img)
                //    '=====================================================
                //    '	Footer to show Page Number
                //    '=====================================================
                //    'Me.AddPageNumbers(pdfBook, intCertificatePage, intEventCoverPage)
                //    '------------------------------------
                //    '--save the file to the work area
                //    '------------------------------------

                try
                {
                    pdfBook.ProcessParagraphs();
                    pdfBook.Save(strTempFilePath);
                    pdfBook = new Aspose.Pdf.Document(strTempFilePath);
                    try
                    {
                        AddPageNumbers(pdfBook, intCertificatePage, intEventCoverPage);
                        Aspose.Pdf.Document.OptimizationOptions optimizer = new Aspose.Pdf.Document.OptimizationOptions();
                        optimizer.LinkDuplcateStreams = true;
                        optimizer.RemoveUnusedObjects = true;
                        optimizer.CompressImages = true;
                        optimizer.ImageQuality = 30;
                        pdfBook.OptimizeResources(optimizer);
                    }
                    catch (Exception ex) { }
                    // 'Exception in optimization

                    pdfBook.Save(strTempFilePath);
                    pdfBook.Dispose();
                }
                catch (Exception ex) { }

                try
                {
                    //        '------------------------------------
                    //        '--clear any existing archive
                    //        '------------------------------------
                    if (System.IO.File.Exists(strTargetFilePath))
                    {
                        if (!clsUtility.DeleteFile(strTargetFilePath, null))
                        {
                            // <===>   clsUtility.LogErrorMessage(Me.lblMsg, Me.Request, Me.GetType().Name, 0, "Error deleting old archive file");
                        }
                    }
                    //        '------------------------------------
                    //        '--create a new archive
                    //        '------------------------------------
                    ZipArchive archive = ZipFile.Open(strTargetFilePath, ZipArchiveMode.Create);
                    archive.CreateEntryFromFile(strTempFilePath, "MyConferenceBook.pdf", CompressionLevel.Fastest);
                    archive.Dispose();
                    double dblFileSize = clsUtility.getFileSize(strTargetFilePath, clsUtility.FileSize_KB);
                    CreateBook = "";
                }
                catch (Exception ex)
                {
                    CreateBook = "Error creating zip file - " + ex.Message + ex.StackTrace;
                }
            }
            catch (Exception ex)
            {
                CreateBook = "Error creating pdf file - " + ex.Message + ex.StackTrace;
            }


            return CreateBook;
        }
        public string Mid(string input, int start)
        {
            return input.Substring(Math.Min(start, input.Length));
        }

        private void AddCellToRow(Aspose.Pdf.Row Row, bool bAlignLeft = false, bool bAlignRight = false, bool bHighlight = false)
        {
            Aspose.Pdf.Cell cell = Row.Cells.Add();
            cell.VerticalAlignment = Aspose.Pdf.VerticalAlignment.Top;
            if (bAlignLeft) cell.Alignment = Aspose.Pdf.HorizontalAlignment.Left;
            if (bAlignRight) cell.Alignment = Aspose.Pdf.HorizontalAlignment.Right;
            if (bHighlight) cell.BackgroundColor = Aspose.Pdf.Color.WhiteSmoke;
            cell.IsWordWrapped = true;
        }
        private void AddEntryToTOC(Aspose.Pdf.Table pdfTable, int intNextPage, string strText)
        {
            // '--add row with two cells
            Aspose.Pdf.Row pdfRow = pdfTable.Rows.Add();
            AddCellToRow(pdfRow);
            AddCellToRow(pdfRow, bAlignLeft: true);

            //    '--add data to row
            Aspose.Pdf.Text.TextFragment t1 = new Aspose.Pdf.Text.TextFragment(strText);
            t1.TextState.FontSize = 10;

            t1.TextState.Font = Aspose.Pdf.Text.FontRepository.FindFont("Verdana");

            Aspose.Pdf.Text.TextFragment tp = new Aspose.Pdf.Text.TextFragment(intNextPage.ToString());
            tp.TextState.FontSize = 10;
            tp.TextState.Underline = true;
            tp.TextState.Font = Aspose.Pdf.Text.FontRepository.FindFont("Verdana");
            Aspose.Pdf.LocalHyperlink link = new Aspose.Pdf.LocalHyperlink();
            //    '--Set target page for link instance

            link.TargetPageNumber = intNextPage;
            //    '--Set TextFragment hyperlink
            tp.Hyperlink = link;
            //   '--Add text to paragraphs collection of Page

            pdfRow.Cells[0].Paragraphs.Add(tp);
            pdfRow.Cells[1].Paragraphs.Add(t1);
        }

        private void AddLogoToPage(Aspose.Pdf.Page pdfpage, int intX, int intY)
        {
            // '--add logo and center horizontally
            string strLogoFile = "/images/homepage/magilogoScheduler.png";

            Dictionary<int, clsImg> dct = ((Dictionary<int, clsImg>)System.Web.HttpContext.Current.Application["cImages"]);
            if (dct.ContainsKey(clsImages.IMG_12))
            {
                strLogoFile = dct[clsImages.IMG_12].strPath;
            }

            strLogoFile = Server.MapPath(strLogoFile);
            int intWidth = 250;
            int intHeight = 45;
            if (intX < 0)
            {
                intX = Convert.ToInt32(.5 * (pdfpage.PageInfo.Width - intWidth));
            }
            clsUtility.PDFAddImageToPage(pdfpage, strLogoFile, intX, intY, intX + intWidth, intY + intHeight, 50);
        }

        private void AddPageNumbers(Aspose.Pdf.Document pdfbook, int intCertificatePage, int intEventCoverPage)
        {


            // '--remove old page number stamps
            try
            {
                Aspose.Pdf.Facades.PdfContentEditor contentEditor = new Aspose.Pdf.Facades.PdfContentEditor();
                contentEditor.BindPdf(pdfbook);
                Aspose.Pdf.Facades.StampInfo[] stampInfo = contentEditor.GetStamps(1);
                contentEditor.DeleteStampByIds(new int[] { 999 });
            }
            catch (Exception ex)
            {
                // '--ignore - page may of had no stamps
                string err = ex.ToString();
            }

            //'create page number stamp
            Aspose.Pdf.PageNumberStamp pageNumberStamp = new Aspose.Pdf.PageNumberStamp();

            // 'whether the stamp is background
            pageNumberStamp.Background = false;
            pageNumberStamp.Format = "Page # of " + pdfbook.Pages.Count.ToString();
            pageNumberStamp.BottomMargin = 20;
            pageNumberStamp.RightMargin = 50;
            pageNumberStamp.HorizontalAlignment = Aspose.Pdf.HorizontalAlignment.Right;
            pageNumberStamp.StartingNumber = 1;
            //'set text properties
            pageNumberStamp.TextState.Font = Aspose.Pdf.Text.FontRepository.FindFont("Arial");
            pageNumberStamp.TextState.FontSize = 10;
            //  '--you must add StampId, so you can remove it later
            pageNumberStamp.setStampId(999);


            Aspose.Pdf.LocalHyperlink link = new Aspose.Pdf.LocalHyperlink();
            link.TargetPageNumber = 1 + intEventCoverPage;



            Aspose.Pdf.Text.TextFragment txtRTT = new Aspose.Pdf.Text.TextFragment("Return to TOC");
            txtRTT.Hyperlink = link;
            txtRTT.Margin.Top = -65;
            txtRTT.Margin.Left = -80;
            txtRTT.VerticalAlignment = Aspose.Pdf.VerticalAlignment.Top;
            txtRTT.HorizontalAlignment = Aspose.Pdf.HorizontalAlignment.Left;

            //'create copyright stamp
            Aspose.Pdf.PageNumberStamp copyright = new Aspose.Pdf.PageNumberStamp();

            //'whether the stamp is background
            copyright.Background = false;
            copyright.Format = "(c) " + DateTime.Now.Year.ToString() + " by MAGI";
            copyright.BottomMargin = 20;
            copyright.LeftMargin = 50;
            copyright.HorizontalAlignment = Aspose.Pdf.HorizontalAlignment.Left;
            //'set text properties
            copyright.TextState.Font = Aspose.Pdf.Text.FontRepository.FindFont("Arial");
            copyright.TextState.FontSize = 10;
            // '--you must add StampId, so you can remove it later
            copyright.setStampId(998);


            foreach (Aspose.Pdf.Page pg in pdfbook.Pages)
                if (pg.Number == intEventCoverPage || pg.Number == intCertificatePage)
                {

                }
                else
                {
                    pg.AddStamp(pageNumberStamp);
                    pg.AddStamp(copyright);
                    if (pg.Number != 1 + intEventCoverPage)
                    {

                        pg.Paragraphs.Add(txtRTT);

                    }
                    //switch (pg.Number)
                    //{                  
                    //    case intEventCoverPage:                      
                    //    case intCertificatePage:
                    //        {
                    //            break;
                    //        }
                    //    //  '--no numbers
                    //    default:
                    //        pg.AddStamp(pageNumberStamp);
                    //        pg.AddStamp(copyright);
                    //        if (pg.Number != 1 + intEventCoverPage) {

                    //            pg.Paragraphs.Add(txtRTT);

                    //        }
                    //        break;
                }



        }

        private Aspose.Pdf.Table CreateNewTable(string strColumns, int intMarginTop, bool bBorders)
        {
            // '-- Instantiate a table object
            Aspose.Pdf.Table table1 = new Aspose.Pdf.Table();
            table1.Margin.Top = intMarginTop;
            table1.ColumnWidths = strColumns;
            // '--Set default cell border using BorderInfo object
            Aspose.Pdf.GraphInfo g = new Aspose.Pdf.GraphInfo();
            g.LineWidth = 0.1F;
            g.Color = Aspose.Pdf.Color.WhiteSmoke;
            int intB = Convert.ToInt32(bBorders ? Aspose.Pdf.BorderSide.Bottom : Aspose.Pdf.BorderSide.None);
            table1.DefaultCellBorder = new Aspose.Pdf.BorderInfo((BorderSide)intB, g.Color);

            // '-- Create MarginInfo object And set its left, bottom, right And top margins
            Aspose.Pdf.MarginInfo margin = new Aspose.Pdf.MarginInfo();
            margin.Top = 3.0F;
            margin.Left = 5.0F;
            margin.Bottom = 3.0F;
            margin.Right = 5.0F;
            table1.DefaultCellPadding = margin;
            table1.DefaultCellTextState.FontSize = 9;
            table1.DefaultCellTextState.Font = Aspose.Pdf.Text.FontRepository.FindFont("Arial");

            return table1;
        }


        [CustomizedAuthorize]
        public ActionResult DownloadSlide(string tab)
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();

            clsEventAccount cEventAccount = new clsEventAccount();

            Aspose.Pdf.Document pdfZipSlides = clsUtility.CreateNewPDF("Slide - " + cEvent.strEventID, false);
            string strTempFilePath = Server.MapPath("~/BookDocuments/Slide_" + cEvent.intEvent_PKey.ToString() + "_" + data.Id.ToString() + ".pdf");
            string strSlidePath = Server.MapPath("~/BookDocuments/" + "Slide_" + cEvent.intEvent_PKey.ToString() + "_" + data.Id.ToString() + ".zip");

            int intCurEventPKey = 0;
            intCurEventPKey = cLast.intEventSelector;
            intCurEventPKey = cSettings.intPriorEvent_pKey;
            int _tab = 0;
            if (tab.Trim() != "Attending")
            {
                _tab = 1;
            }
            var dtnew = dba.Sessions_and_FixedHeader(intCurEventPKey, data.Id.ToString(), _tab, 0, cEventAccount.bHasSpecialMeal, cEventAccount.intEventAccount_pKey.ToString(), cEvent.intEvent_PKey.ToString(), cSettings.IsMealshow);

            if (dtnew.Rows.Count > 0 && dtnew != null)
            {
                foreach (DataRow dr in dtnew.Rows)
                {
                    int intEvtSessionPKey = Convert.ToInt32(dr["EvtSessionPKey"].ToString());
                    string strSessionID = dr["SessionID"].ToString();
                    if (intEvtSessionPKey > 0)
                    {
                        string strPhysical = Server.MapPath("~/SessionDocuments/" + cEvent.strEventID + "_" + dr["SessionID"] + ".pdf");
                        if (System.IO.File.Exists(strPhysical))
                        {
                            Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(strPhysical);
                            pdfZipSlides.Pages.Add(pdf.Pages);
                        }
                    }
                }
                try
                {
                    Aspose.Pdf.Document.OptimizationOptions optimizer = new Aspose.Pdf.Document.OptimizationOptions();
                    optimizer.LinkDuplcateStreams = true;
                    optimizer.RemoveUnusedObjects = true;
                    optimizer.CompressImages = true;
                    optimizer.ImageQuality = 30;
                    pdfZipSlides.OptimizeResources(optimizer);
                }
                catch (Exception ex)
                { }

                if (pdfZipSlides.Pages.Count > 0)
                {
                    pdfZipSlides.Save(strTempFilePath);
                    pdfZipSlides.Dispose();
                    if (System.IO.File.Exists(strSlidePath))
                    {
                        if (!clsUtility.DeleteFile(strSlidePath, null))
                        {
                            // clsUtility.LogErrorMessage(Me.lblMsg, HttpContext.Current.Request, Me.GetType().Name, 0, "Error deleting old archive file")
                            return Json(new { result = "other", message = "Error deleting old archive file" }, JsonRequestBehavior.AllowGet);
                        }
                        ZipArchive archive = ZipFile.Open(strSlidePath, ZipArchiveMode.Create);
                        archive.CreateEntryFromFile(strTempFilePath, "Slide.pdf", CompressionLevel.Optimal);
                        archive.Dispose();
                        return Json(new { result = "OK", fname = Path.GetFileName(strSlidePath) }, JsonRequestBehavior.AllowGet);
                    }
                    // clsUtility.InjectAlert(ScriptManager.GetCurrent(Me.Page), Me.Page, "No slide for this event.");
                    return Json(new { result = "other", message = "No slide for this event." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { result = "other", message = "No slide for this event." }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { result = "other", message = "No slide for this event." }, JsonRequestBehavior.AllowGet);
            }

        }


        [CustomizedAuthorize]
        public ActionResult DownloadScheduleFileClick()
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);


            clsEventAccount cEventAccount = new clsEventAccount();

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();

            string strScheduleFilename = cEvent.strEventID + "_" + data.Id.ToString() + "_Sched.pdf";
            string strScheduleTargetFile = Server.MapPath("~/UserDocuments/" + strScheduleFilename);

            try
            {
                cEventAccount.LogAuditMessage("Download: Conference Schedule - " + cEvent.strEventID);
            }
            catch (Exception ex)
            {
                // clsUtility.LogErrorMessage(null, Me.Request, Me.GetType().Name, 0, "Error downloading schedule");
                return Json(new { result = "error", message = "Error downloading schedule" }, JsonRequestBehavior.AllowGet);
            }
            //--download the speaker file
            if (clsUtility.FileExists(strScheduleTargetFile))
            {
                return Json(new { result = "OK", fname = strScheduleFilename }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //  clsUtility.LogErrorMessage(Me.lblMsg, Me.Request, Me.GetType().Name, 0, "Error downloading schedule");
                return Json(new { result = "error", message = "Error downloading schedule" }, JsonRequestBehavior.AllowGet);
            }

        }
        [CustomizedAuthorize]
        public FileResult DownloadUserScheduleFile(string FileName)
        {
            string strScheduleTargetFile = Server.MapPath("~/UserDocuments/" + FileName.Trim());

            byte[] bytes = System.IO.File.ReadAllBytes(strScheduleTargetFile);
            return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, FileName);
        }
        [CustomizedAuthorize]
        public ActionResult DownloadSpeakerFile(string tab)
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);

            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);

            clsEventAccount cEventAccount = new clsEventAccount();

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();

            string strSpeakerFilename = cEvent.strEventID + "_Faculty_Bios_Img.pdf";
            string strSpeakerTargetFile = Server.MapPath("~/Documents/" + strSpeakerFilename);

            try
            {
                cEventAccount.LogAuditMessage("Download: Faculty Biographies - " + cEvent.strEventID, clsAudit.LOG_DownloadBook);
            }
            catch (Exception ex)
            {
                // clsUtility.LogErrorMessage(null, Me.Request, Me.GetType().Name, 0, "Error downloading schedule");
                return Json(new { result = "error", message = "Error downloading Faculty Bios" }, JsonRequestBehavior.AllowGet);
            }
            if (clsUtility.FileExists(strSpeakerTargetFile))
            {
                return Json(new { result = "OK", fname = strSpeakerFilename }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //  clsUtility.LogErrorMessage(Me.lblMsg, Me.Request, Me.GetType().Name, 0, "Error downloading schedule");
                return Json(new { result = "error", message = "Error downloading Faculty Bios" }, JsonRequestBehavior.AllowGet);
            }

        }
        [CustomizedAuthorize]
        public FileResult DownloadSpeakerBookFile(string FileName)
        {
            string strScheduleTargetFile = Server.MapPath("~/Documents/" + FileName.Trim());

            byte[] bytes = System.IO.File.ReadAllBytes(strScheduleTargetFile);
            return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, FileName);
        }

        [CustomizedAuthorize]
        public ActionResult DownloadCertificateBook()
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);


            clsEventAccount cEventAccount = new clsEventAccount();


            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();

            try
            {
                cEventAccount.LogAuditMessage("Download: Attendance Certificate - " + cEvent.strEventID, clsAudit.LOG_DownloadBook);
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", message = "Error downloading attendee certificate" }, JsonRequestBehavior.AllowGet);
            }

            string strCertFilename = cEvent.strEventID + "_" + data.Id.ToString() + "_Cert.pdf";
            string strCertTargetFile = Server.MapPath("~/UserDocuments/" + strCertFilename);


            if (clsUtility.FileExists(strCertTargetFile))
            {
                return Json(new { result = "OK", fname = strCertFilename }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //  clsUtility.LogErrorMessage(Me.lblMsg, Me.Request, Me.GetType().Name, 0, "Error downloading schedule");
                return Json(new { result = "error", message = "Error downloading attendee certificate" }, JsonRequestBehavior.AllowGet);
            }
        }
        [CustomizedAuthorize]
        public ActionResult DownloadSpecialMeal()
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);

            clsEventAccount cEventAccount = new clsEventAccount();
            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();

            try
            {
                cEventAccount.LogAuditMessage("Download: Special Meal Form - " + cEvent.strEventID);
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", message = "Error downloading attendee special meals" }, JsonRequestBehavior.AllowGet);
            }
            string strMealFilename = cEvent.strEventID + "_" + data.Id.ToString() + "_Meal.pdf";
            string strCertTargetFile = Server.MapPath("~/UserDocuments/" + strMealFilename);

            if (clsUtility.FileExists(strCertTargetFile))
                return Json(new { result = "OK", fname = strMealFilename }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { result = "error", message = "Error downloading attendee special meals" }, JsonRequestBehavior.AllowGet);
        }
        private void Prepare(int EventPkey, string StrEventId, int AccountPkey)
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EventPkey;
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();

            clsEventAccount cEventAccount = new clsEventAccount();
            cEventAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEventAccount.intEvent_pKey = EventPkey;
            cEventAccount.intAccount_pKey = AccountPkey;
            cEventAccount.LoadEventInfo(true);

            //bool bBookAvailable = false, bBalanceDue = false;
            bool phSpecialMeal = false;
            string strMealFilename = "";
            string strMealTargetFile = "";

            string strScheduleFilename = "";
            string strScheduleTargetFile = "";

            string strSpeakerFilename = cEvent.strEventID + "_Faculty_Bios_Img.pdf";
            string strSpeakerTargetFile = Server.MapPath("~/Documents/" + strSpeakerFilename);

            if (clsUtility.FileExists(strSpeakerTargetFile))
            {
                DateTime dtLastUpdate = System.IO.File.GetLastWriteTime(strSpeakerTargetFile);
                if (cSettings.bAlwaysUpdateBookSpeak)
                {
                    if (cEvent.CreateSpeakerPDf(strSpeakerTargetFile) != "")
                    {
                        return;
                    }
                }
            }
            else
            {
                cEvent.bGenPDF = true;
                if (cEvent.CreateSpeakerPDf(strSpeakerTargetFile) != "")
                {
                    return;
                }
            }
            phSpecialMeal = cEventAccount.bHasSpecialMeal && (cSettings.IsMealshow = true);
            if (phSpecialMeal)
            {
                strMealFilename = cEvent.strEventID + "_" + AccountPkey.ToString() + "_Meal.pdf";
                strMealTargetFile = Server.MapPath("~/UserDocuments/" + strMealFilename);
                if (clsUtility.FileExists(strMealTargetFile))
                {
                    DateTime dtLastUpdate = System.IO.File.GetLastWriteTime(strMealTargetFile);
                    if ((cEventAccount.dtLastScheduleChange > dtLastUpdate) || (cSettings.bAlwaysUpdateBookMeals))
                    {
                        if (CreateMealPDF(strMealTargetFile, cEvent.strEventFullname, EventPkey, AccountPkey))
                            return;
                    }
                    else if (!CreateMealPDF(strMealTargetFile, cEvent.strEventFullname, EventPkey, AccountPkey))
                    {
                        return;
                    }
                }
            }
            //'--schedule file (update it if changed)
            strScheduleFilename = cEvent.strEventID + "_" + AccountPkey.ToString() + "_Sched.pdf";
            strScheduleTargetFile = Server.MapPath("~/UserDocuments/" + strScheduleFilename);
            if (clsUtility.FileExists(strScheduleTargetFile))
            {
                DateTime dtLastUpdate = System.IO.File.GetLastWriteTime(strScheduleTargetFile);
                if ((cEventAccount.dtLastScheduleChange > dtLastUpdate) || (cSettings.bAlwaysUpdateBookSched))
                {
                    if (!CreateSchedule(strScheduleTargetFile, EventPkey, StrEventId, AccountPkey))
                    {
                        return;
                    }
                }
                else if (!CreateSchedule(strScheduleTargetFile, EventPkey, StrEventId, AccountPkey))
                {
                    return;
                }

            }
            // '--certificate (update if either scheduled hours or cert itself has chaned)

            string strCertFilename = cEvent.strEventID + "_" + AccountPkey.ToString() + "_Cert.pdf";
            string strCertTargetFile = Server.MapPath("~/UserDocuments/" + strCertFilename);

            if (clsUtility.FileExists(strCertTargetFile))
            {
                DateTime dtLastUpdate = System.IO.File.GetLastWriteTime(strCertTargetFile);
                DateTime dtLastSchedUpdate = System.IO.File.GetLastWriteTime(strScheduleTargetFile);
                if ((cEventAccount.dtLastScheduleChange > dtLastUpdate) || (cEventAccount.dtLastScheduleChange > dtLastUpdate) || (cSettings.bAlwaysUpdateBookCert))
                {
                    if (!cEventAccount.CreateAttendanceCertificate(strCertTargetFile, cEvent))
                    {
                        return;
                    }
                }
                else if (!cEventAccount.CreateAttendanceCertificate(strCertTargetFile, cEvent))
                {
                    return;
                }
            }
        }

        private bool CreateMealPDF(string strTargetFile, string EvtFullName, int EvtPkey, int AccountPkey)
        {
            bool CreateMealPDF = false;

            Aspose.Pdf.Document pdfnew = new Aspose.Pdf.Document();
            pdfnew.PageInfo.Width = Aspose.Pdf.PageSize.PageLetter.Width;
            pdfnew.PageInfo.Height = Aspose.Pdf.PageSize.PageLetter.Height;

            Aspose.Pdf.Page sec1 = pdfnew.Pages.Add();
            sec1.PageInfo.Margin.Left = 72;// '--1 inch = 72px
            sec1.PageInfo.Margin.Right = 72;
            sec1.PageInfo.Margin.Top = 72;
            sec1.PageInfo.Margin.Bottom = 72;

            String s = "~/images/homepage/magilogoScheduler.png";
            Dictionary<int, clsImg> dct = ((Dictionary<int, clsImg>)System.Web.HttpContext.Current.Application["cImages"]);
            if (dct.ContainsKey(clsImages.IMG_12))
            {
                s = dct[clsImages.IMG_12].strPath;
            }
            s = Server.MapPath(s);
            clsUtility.AddPDFImage(sec1, s, "");


            //   '--heading
            sec1.Paragraphs.Add(clsUtility.getPDFText("MAGI Clinical Research Conference Special Meals", "Verdana", 14, false, true, true, true, bBold: true));
            sec1.Paragraphs.Add(clsUtility.getPDFText(EvtFullName, "Verdana", 12, false, true, true, true));
            sec1.Paragraphs.Add(clsUtility.getPDFText("(As of: " + String.Format("{0:MMMM dd, yyyy}", DateTime.Now) + ")", "Verdana", 10, false, true, true, true));
            sec1.Paragraphs.Add(clsUtility.getPDFText("", "Verdana", 10, false, true, true, true));
            sec1.Paragraphs.Add(clsUtility.getPDFText("", "Verdana", 10, false, true, true, true));

            var dt = dba.Mealtable(AccountPkey, EvtPkey);

            if (dt.Rows.Count > 0)
            {
                Aspose.Pdf.Table tab1 = new Aspose.Pdf.Table();
                tab1.ColumnWidths = "250 250";
                tab1.Border = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, Aspose.Pdf.Color.WhiteSmoke);  // 0.5 is changed to Aspose.Pdf.Color.WhiteSmoke
                tab1.DefaultCellBorder = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, Aspose.Pdf.Color.WhiteSmoke);
                tab1.DefaultCellTextState.Font = Aspose.Pdf.Text.FontRepository.FindFont("Verdana");
                tab1.DefaultCellTextState.FontSize = 14;
                tab1.DefaultCellPadding = new Aspose.Pdf.MarginInfo();
                tab1.DefaultCellPadding.Left = 3;
                tab1.DefaultCellPadding.Top = 10;


                //'--add two rows of two cells
                Aspose.Pdf.Row row1 = tab1.Rows.Add();
                Aspose.Pdf.Row row2 = tab1.Rows.Add();
                row1.Cells.Add();
                row1.Cells.Add();
                if (dt.Rows.Count > 2)
                {
                    row2.Cells.Add();
                    row2.Cells.Add();
                }
                //  '--load up to 4 meals
                int intIndex = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    intIndex = intIndex + 1;
                    if (intIndex <= 4)
                    {
                        string strDay = dr["DayName"].ToString();
                        string strSession = dr["SessionTitle"].ToString();
                        string strMeal = dr["SpecialMealID"].ToString();
                        string strImgName = dr["ImageFilename"].ToString();
                        strImgName = (strImgName == "") ? "No-Food.jpg" : strImgName;

                        //  '--load an image
                        Aspose.Pdf.Image img = new Aspose.Pdf.Image();
                        string strImagePath = "~/images/foods/" + strImgName;
                        img.File = Server.MapPath(strImagePath);
                        img.FileType = Aspose.Pdf.ImageFileType.Unknown;
                        img.FixWidth = 100;
                        img.FixHeight = 100;
                        img.HorizontalAlignment = Aspose.Pdf.HorizontalAlignment.Center;

                        //  '--load text
                        s = "<br/>" + strDay + "<br/>" + strSession + "<br/><b>" + strMeal + "</b>";
                        Aspose.Pdf.BaseParagraph t = clsUtility.getPDFText(s, "Verdana", 14, true, false, true, true);
                        Aspose.Pdf.BaseParagraph t2 = clsUtility.getPDFText("<br/>", "Verdana", 14, true, false, true, true);

                        switch (intIndex)
                        {
                            case 1:
                            case 2:
                                {
                                    row1.Cells[intIndex - 1].Paragraphs.Add(img);
                                    row1.Cells[intIndex - 1].Paragraphs.Add(t);
                                    row1.Cells[intIndex - 1].Paragraphs.Add(t2);
                                }
                                break;
                            case 3:
                            case 4:
                                {
                                    row2.Cells[intIndex - 3].Paragraphs.Add(img);
                                    row2.Cells[intIndex - 3].Paragraphs.Add(t);
                                    row2.Cells[intIndex - 3].Paragraphs.Add(t2);
                                }
                                break;
                        }

                    }
                }
                sec1.Paragraphs.Add(tab1);
                // '--save it
                pdfnew.Save(strTargetFile);

            }
            CreateMealPDF = true;
            return CreateMealPDF;
        }


        private bool CreateSchedule(string strTargetFile, int EventPkey, string strEventID, int AccountPkey)
        {
            bool CreateSchedule = false;

            clsEventAccount cEventAccount = new clsEventAccount();
            cEventAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEventAccount.intEvent_pKey = EventPkey;
            cEventAccount.intAccount_pKey = AccountPkey;
            cEventAccount.LoadEventInfo(true);

            Aspose.Pdf.Document pdfnew = new Aspose.Pdf.Document();
            pdfnew.PageInfo.Width = Aspose.Pdf.PageSize.PageLetter.Width;
            pdfnew.PageInfo.Height = Aspose.Pdf.PageSize.PageLetter.Height;


            // ' Add a section into the pdf document
            Aspose.Pdf.Page sec1 = pdfnew.Pages.Add();
            sec1.PageInfo.Margin.Left = 72;// '--1 inch = 72px
            sec1.PageInfo.Margin.Right = 72;
            sec1.PageInfo.Margin.Top = 25;// 'sec1.PageInfo.Margin.Top = 72 before change
            sec1.PageInfo.Margin.Bottom = 72;

            String s = "~/images/homepage/magilogoScheduler.png";
            Dictionary<int, clsImg> dct = ((Dictionary<int, clsImg>)System.Web.HttpContext.Current.Application["cImages"]);
            if (dct.ContainsKey(clsImages.IMG_12))
            {
                s = dct[clsImages.IMG_12].strPath;
            }
            s = Server.MapPath(s);
            try
            {
                clsUtility.AddPDFImage(sec1, s, "");
            }
            catch (Exception ex) { }


            // '--heading
            sec1.Paragraphs.Add(clsUtility.getPDFText("MAGI's Clinical Research Conference Schedule", "Verdana", 14, false, true, true, true, bBold: true));
            sec1.Paragraphs.Add(clsUtility.getPDFText(strEventID, "Verdana", 12, false, true, true, true));
            sec1.Paragraphs.Add(clsUtility.getPDFText("(as of " + String.Format("{0:MMMM dd, yyyy}", DateTime.Now) + ")", "Verdana", 10, false, true, true, true));

            // '--content
            s = cEventAccount.getHTMLSchedule();
            sec1.Paragraphs.Add(clsUtility.getPDFText(s, "Verdana", 10, true, false, true, false));

            //  '--save it
            try
            {
                pdfnew.Save(strTargetFile);

                // '--save size info
                double dblSize = clsUtility.getFileSize(strTargetFile, clsUtility.FileSize_MB);
                int intPages = pdfnew.Pages.Count;
                bool saved = dba.save_size_info(dblSize, intPages, cEventAccount.intEventAccount_pKey);
                if (saved)
                {
                    CreateSchedule = true;
                }
            }
            catch (Exception ex)
            {
                // clsUtility.LogErrorMessage(Me.lblMsg, Me.Request, Me.GetType().Name, 0, "Error creating Schedule file");
                return CreateSchedule;
            }
            return CreateSchedule;
        }


        private void RefreshFiles(string tab, bool bookcreation, int EventPkey, int AccountPkey)
        {
            bool bAttendingBook = false;
            if (tab.Trim() == "Attending")
            {
                bAttendingBook = true;
            }
            string lblBookDownload = "";
            int offSet = 0;
            bool lblBookDownloadVisible = false;
            bool btnDownloadBookEnable = false;
            string btnBuildBookText = "Build Book";
            // 'Set the time zone information to California GMT 
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time (Mexico)");
            bool bBookCreation = bookcreation;
            if (bAttendingBook)
            {
                string fileName = "MyBook_" + "Attending_" + EventPkey.ToString() + "_" + AccountPkey.ToString() + ".zip";
                string FilePath = Server.MapPath("~/BookDocuments/" + fileName);
                FileInfo fs = new FileInfo(FilePath);
                if (fs.Exists)
                {
                    lblBookDownloadVisible = true;
                    if (bBookCreation)
                        lblBookDownload = TimeZoneInfo.ConvertTime(fs.CreationTime, timeZoneInfo).ToString("hh:mm tt MM/dd/yy");
                    else
                        lblBookDownload = fs.CreationTime.ToUniversalTime().AddMinutes(-offSet).ToString("hh:mm tt MM/dd/yy");

                    ViewBag.lblBookSize = Math.Round((fs.Length / 1000000.0), 2).ToString();
                    btnDownloadBookEnable = true;
                    btnBuildBookText = "Rebuild Book";
                }
            }
            else
            {
                string fileName = "MyBook_" + EventPkey.ToString() + "_" + AccountPkey.ToString() + ".zip";
                string FilePath = Server.MapPath("~/BookDocuments/" + fileName);
                FileInfo fs = new FileInfo(FilePath);
                if (fs.Exists)
                {
                    lblBookDownloadVisible = true;
                    if (bBookCreation)
                        lblBookDownload = TimeZoneInfo.ConvertTime(fs.CreationTime, timeZoneInfo).ToString("hh:mm tt MM/dd/yy");
                    else
                        lblBookDownload = fs.CreationTime.ToUniversalTime().AddMinutes(-offSet).ToString("hh:mm tt MM/dd/yy");

                    ViewBag.lblBookSize = Math.Round((fs.Length / 1000000.0), 2).ToString();
                    btnDownloadBookEnable = true;
                    btnBuildBookText = "Rebuild Book";
                }
            }

            ViewBag.lblBookDownloadVisible = lblBookDownloadVisible;
            ViewBag.lblBookDownload = lblBookDownload;
            ViewBag.btnDownloadBookEnable = btnDownloadBookEnable;
            ViewBag.btnBuildBookText = btnBuildBookText;

        }

        [CustomizedAuthorize]
        public ActionResult SessionBook(string EvtSessionInfo)
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();

            string strEvtSessionInfo = EvtSessionInfo;
            int intPosn = strEvtSessionInfo.IndexOf("-");  // InStr(strEvtSessionInfo, "-")
            string strSessionID = Mid(strEvtSessionInfo, intPosn + 1);
            string strFilename = cEvent.strEventID + "_" + strSessionID + ".pdf";

            if (strFilename != "")
            {
                string strPhysicalPath = Server.MapPath("~/SessionDocuments/") + strFilename;
                if (clsUtility.FileExists(strPhysicalPath))
                {
                    Byte[] bts = System.IO.File.ReadAllBytes(strPhysicalPath);

                    return Json(new { result = "OK", fname = strFilename }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //clsUtility.LogErrorMessage(Me.lblMsg, Me.Request, Me.GetType().Name, 0, "Activity file not found: " + strFilename);
                    return Json(new { result = "error", message = "Activity file not found: " + strFilename }, JsonRequestBehavior.AllowGet);
                }
            }



            return null;
        }
        [CustomizedAuthorize]
        public FileResult DownloadSessionBook(string FileName)
        {
            string strScheduleTargetFile = Server.MapPath("~/SessionDocuments/" + FileName.Trim());

            byte[] bytes = System.IO.File.ReadAllBytes(strScheduleTargetFile);
            return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, FileName);

        }
        #endregion

        #region MYFAQs

        [CustomizedAuthorize]
        public ActionResult MyFAQs()
        {
            if (User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                SqlConnection conn = new SqlConnection(ReadConnectionString());
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);

                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.EventPKey =  data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);
                ViewBag.InstructionText = cSettings.getText(clsSettings.Text_FAQInstruct);

                clsEvent cEvent = new clsEvent();
                cEvent.intEvent_PKey =  data.EventId;
                cEvent.sqlConn = conn;
                cEvent.LoadEvent();

                string accountType = getMyAcctTypes(data.Id);
                string intRegistrationLevel_pKey = "";
                bool StaffMember = data.StaffMember;
                bool bSponsor = new SqlOperation().VerifyIsPartner(data.Id, data.EventId);
                int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);

                bool IsAttendee = (intAttendeeStatus != clsEventAccount.PARTICIPATION_Cancelled);
                bool bSpeaker = clsEventAccount.getSpeakerStatus(data.Id, data.EventId);
                ViewBag.leftPanel_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
                ViewBag.VirtualDropdown_Visible = ViewBag.leftPanel_Visible;
                DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                var faq = new DataTable();
                DataTable faqQuesAns = dba.MyFAQWithQuestion(data.EventId, IsAttendee, bSponsor, bSpeaker, StaffMember, accountType, cEvent.dtStartDate.ToShortDateString(), cEvent.dtEndDate.ToShortDateString());
                if (faqQuesAns!= null && faqQuesAns.Rows.Count>0)
                {
                    DataColumn newColumn = new DataColumn("Elem", typeof(string));
                    newColumn.DefaultValue = "";
                    faqQuesAns.Columns.Add(newColumn);

                    faq = faqQuesAns.DefaultView.ToTable(true, "FAQCategory_pKey", "FAQCategoryID");
                    clsVenue cVenue = new clsVenue();
                    cVenue.intVenue_PKey = cEvent.intVenue_PKey;
                    cVenue.sqlConn = conn;
                    cVenue.LoadVenue();

                    for (int rowIndex = 0; rowIndex< faqQuesAns.Rows.Count; rowIndex++)
                    {
                        DataRow row = faqQuesAns.Rows[rowIndex];
                        string s = cVenue.ReplaceReservedWords(cEvent.ReplaceReservedWords(row["FAQa"].ToString()));
                        string resulttext = clsSettings.ReplaceTermsGeneral(clsReservedWords.ReplacePriorNext(null, s));
                        row["FAQa"] = resulttext;
                        StringBuilder sb = new StringBuilder();
                        sb.Append(" <div class=\"FAQ Font10\">  <details class=\"FAQQuestn "+ ("Details_" +  row["FAQ_pKey"])+"\" style=\"padding-left:10px\"><summary id='" + ("summary_" +  row["FAQ_pKey"]) +"'><span>" + row["Question"] + "</span></summary><table width=\"100%\";><tr><td><div class='Font10' style=\"background-color:azure ;  margin: 4px 2px 2px 2px\"><span>" + resulttext + "</span></div></td></tr><tr><td> ");
                        sb.AppendLine(" <div class='Font10' style=\"display: inline-block ; margin: 4px 2px 2px 2px\"><span>Was this information helpful?</span> <input style=\"margin-left:10px\" type=\"radio\" id =\"rdYes\" name=\"" + row["FAQ_pKey"] + "rdbtn\" value =\"1\" class='me-1' /><label> Yes </label ><input style=\"margin-left:10px\" type=\"radio\" id =\"rdSomewhat\" class='me-1'  name=\"" + row["FAQ_pKey"] + "rdbtn\" value =\"2\" /><label > Somewhat </label > ");
                        sb.AppendLine(" <input style=\"margin-left:10px\" type=\"radio\" name=\"" + row["FAQ_pKey"] + "rdbtn\" id=\"rdNo\" value=\"3\" class='me-1'  /><label> No</label> </div><br /><div style=\"display: inline-block; margin: 4px 2px 6px 2px\"><span class='me-1 Font10' > Please Suggest Suggestion :</span><input type=\"text\" id=\"" + row["FAQ_pKey"] + "txtSuggestion\" style=\"width:500px ;height: 22px;\" /></div><br />  ");
                        sb.AppendLine("   <button class =\"SubmitFeedback btn\" id=\"btnSubmit\" onclick=\"SaveFAQSuggestion(" + row["FAQ_pKey"] + "," + row["FAQCategory_pKey"] + ")\" type=\"button\">OK</button></td></tr></table></details> </div>  ");
                        string htmltext = sb.ToString();
                        row["Elem"] = htmltext;
                        faqQuesAns.AcceptChanges();
                    }
                }
                ViewBag.QuesAnswer = faqQuesAns;
                if (ViewBag.VirtualDropdown_Visible)
                {
                    int RegLevelPKey = 0;
                    if (!string.IsNullOrEmpty(intRegistrationLevel_pKey))
                        RegLevelPKey = Convert.ToInt32(intRegistrationLevel_pKey);
                    DateTime dtCalTime = clsEvent.getCaliforniaTime();
                    string Host = HttpContext.Request.Path.ToUpper().Replace("/MYMAGI", "");
                    ViewBag.ddEventVirtualData = repository.GetVirtualEventDropDownInfo(Host, data.Id, data.EventId, dtCurrentTime, dtCalTime, data.ParentOrganization_pKey, intAttendeeStatus, RegLevelPKey, data.GlobalAdmin, data.StaffMember);
                    ViewBag.SelectedDropDown = GetSelectedVirtualDropDown(Host);
                }
                ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";

                bool bShowSurveyQuestion = false;
                DataTable EventFeatures = repository.getDyamicEventSettings(data.EventId, "ISNULL(ShowSurveyQuestion,'0') as ShowSurveyQuestion");
                if (EventFeatures != null && EventFeatures.Rows.Count > 0)
                    bShowSurveyQuestion = (EventFeatures.Rows[0]["ShowSurveyQuestion"] != System.DBNull.Value) ? Convert.ToBoolean(EventFeatures.Rows[0]["ShowSurveyQuestion"]) : false;
                ViewBag.lblRegText = "";
                ViewBag.OpenSurveyRadWindow=false;
                LoadRegistrationQuestions(data, intAttendeeStatus, bShowSurveyQuestion, bSponsor, intRegistrationLevel_pKey);

                return View(faq);
            }
            else
            {
                return View("~/Home/Login");
            }
        }
        [CustomizedAuthorize]
        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult GETFAQsWithQuestions()
        {
            try
            {
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(((FormsIdentity)User.Identity).Ticket.UserData);
                SqlConnection conn = new SqlConnection(ReadConnectionString());

                string intRegistrationLevel_pKey = "";
                string accountType = getMyAcctTypes(data.Id);
                bool bSpeaker = clsEventAccount.getSpeakerStatus(data.Id, data.EventId);
                bool StaffMember = data.StaffMember;
                bool bSponsor = new SqlOperation().VerifyIsPartner(data.Id, data.EventId);
                int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);
                bool IsAttendee = (intAttendeeStatus != clsEventAccount.PARTICIPATION_Cancelled);

                clsEvent cEvent = new clsEvent();
                cEvent.intEvent_PKey = data.EventId;
                cEvent.sqlConn = conn;
                cEvent.LoadEvent();

                DataColumn newColumn = new DataColumn("Elem", typeof(string));
                newColumn.DefaultValue = "";

                DataTable faqQuesAns = dba.MyFAQWithQuestion(data.EventId, IsAttendee, bSponsor, bSpeaker, StaffMember, accountType, cEvent.dtStartDate.ToShortDateString(), cEvent.dtEndDate.ToShortDateString());
                faqQuesAns.Columns.Add(newColumn);
                if (faqQuesAns!= null && faqQuesAns.Rows.Count>0)
                {
                    clsVenue cVenue = new clsVenue();
                    cVenue.intVenue_PKey = cEvent.intVenue_PKey;
                    cVenue.sqlConn = conn;
                    cVenue.LoadVenue();
                    foreach (DataRow dr in faqQuesAns.Rows)
                    {
                        string s = cVenue.ReplaceReservedWords(cEvent.ReplaceReservedWords(dr["FAQa"].ToString()));
                        string resulttext = clsSettings.ReplaceTermsGeneral(clsReservedWords.ReplacePriorNext(null, s));
                        dr["FAQa"] = resulttext;
                        StringBuilder sb = new StringBuilder();
                        sb.Append(" <div class=\"FAQ Font10\">  <details id='" + ("Details_" +  dr["FAQ_pKey"]) +"' class=\"FAQQuestn\" style=\"padding-left:10px\"><summary id='" + ("summary_" +  dr["FAQ_pKey"]) +"'><span>" + dr["Question"] + "</span></summary><table width=\"100%\";><tr><td><div class='Font10' style=\"background-color:azure ;  margin: 4px 2px 2px 2px\"><span>" + resulttext + "</span></div></td></tr><tr><td> ");
                        sb.AppendLine(" <div class='Font10' style=\"display: inline-block ; margin: 4px 2px 2px 2px\"><span>Was this information helpful?</span> <input style=\"margin-left:10px\" type=\"radio\" id =\"rdYes\" name=\"" + dr["FAQ_pKey"] + "rdbtn\" value =\"1\" class='me-1' /><label> Yes </label ><input style=\"margin-left:10px\" type=\"radio\" id =\"rdSomewhat\" class='me-1'  name=\"" + dr["FAQ_pKey"] + "rdbtn\" value =\"2\" /><label > Somewhat </label > ");
                        sb.AppendLine(" <input style=\"margin-left:10px\" type=\"radio\" name=\"" + dr["FAQ_pKey"] + "rdbtn\" id=\"rdNo\" value=\"3\" class='me-1'  /><label> No</label> </div><br /><div style=\"display: inline-block; margin: 4px 2px 6px 2px\"><span class='me-1 Font10' > Please Suggest Suggestion :</span><input type=\"text\" id=\"" + dr["FAQ_pKey"] + "txtSuggestion\" style=\"width:500px ;height: 22px;\" /></div><br />  ");
                        sb.AppendLine("   <button class =\"SubmitFeedback btn\" id=\"btnSubmit\" onclick=\"SaveFAQSuggestion(" + dr["FAQ_pKey"] + "," + dr["FAQCategory_pKey"] + ")\" type=\"button\">OK</button></td></tr></table></details> </div>  ");
                        string htmltext = sb.ToString();
                        dr["Elem"] = htmltext;
                        faqQuesAns.AcceptChanges();
                    }
                }
                var JsonResult = Json(new { result = "OK", FAQs = JsonConvert.SerializeObject(faqQuesAns) }, JsonRequestBehavior.AllowGet);
                JsonResult.MaxJsonLength = int.MaxValue;
                return JsonResult;
            }
            catch
            {

            }
            return Json(new { result = "Error While Fetching Questions" }, JsonRequestBehavior.AllowGet);
        }
        [CustomizedAuthorize]
        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult GETFAQsQuestions(int CategoryId, string searchtext = "")
        {
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(((FormsIdentity)User.Identity).Ticket.UserData);
            SqlConnection conn = new SqlConnection(ReadConnectionString());
            clsEvent cEvent = new clsEvent();

            cEvent.intEvent_PKey = data.EventId;
            cEvent.sqlConn = conn;
            cEvent.GetBasicEventInfo(cEvent.intEvent_PKey);
            string accountType = getMyAcctTypes(data.Id);

            DataTable faqQuesAns = dba.FAQsQuestionAnswer(data.EventId, CategoryId, accountType, cEvent.dtStartDate.ToShortDateString(), cEvent.dtEndDate.ToShortDateString(), searchtext);

            DataTable Info = new DataTable();
            if (faqQuesAns != null && faqQuesAns.Rows.Count>0)
                Info = faqQuesAns.DefaultView.ToTable(true, "FAQCategory_pkey", "FAQ_pKey");

            var JsonResult = Json(new { result = "OK", FAQs = JsonConvert.SerializeObject(Info) }, JsonRequestBehavior.AllowGet);
            JsonResult.MaxJsonLength = int.MaxValue;
            return JsonResult;
        }
        private string getMyAcctTypes(int accountPkey)
        {
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            clsAccount cAccount = new clsAccount();
            cAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cAccount.intAccount_PKey = accountPkey;
            cAccount.LoadAccount();

            string s = "5"; ///--general public
            if (cAccount != null)
            {

                if (cAccount.bStaffMember)
                    s = s + ",6"; /// ' --Staff
                if (cAccount.intAccount_PKey > 0)
                {
                    clsEventAccount c1 = new clsEventAccount();
                    c1.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                    c1.intAccount_pKey = cAccount.intAccount_PKey;
                    c1.intEvent_pKey = cLast.intActiveEventPkey;
                    if (c1.LoadEventInfo(true))
                    {
                        if (c1.bSpeakerAtEvent)
                            s = s + ",1,2"; /// '--speaker and attendee }
                        else
                            if (c1.intParticipationStatus_pKey != clsEventAccount.PARTICIPATION_Cancelled) { s = s + " ,1"; } ///--attending

                        if (c1.bIsExhibitor)
                            s = s + ",4";
                    }
                }
            }
            return s;
        }
        [CustomizedAuthorize]
        public ActionResult SubmitFAQs(string SelectedValue, string SuggestionText, string QuestionpKey)
        {
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            string Status = String.Empty, Suggestion = String.Empty;
            Suggestion = SuggestionText;
            if (SelectedValue == "1")
                Status = "Yes";
            else if (SelectedValue == "2")
                Status = "Somewhat";
            else if (SelectedValue == "3")
                Status = "No";
            else if (String.IsNullOrEmpty(Suggestion))
                Status = "Yes";

            var resulttext = dba.SaveFAQFeedback(Status, Suggestion, Convert.ToInt32(QuestionpKey), data.Id);

            return Json(new { result = "OK", message = resulttext }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region MyGroupChat

        [CustomizedAuthorize]
        public ActionResult MyGroupChat()
        {
            MyGroupChat groupchatpage = new MyGroupChat();
            if (User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                int AccountPkey = data.Id;
                int EventPkey = data.EventId;

                var dataset = dba.GroupChat(AccountPkey, EventPkey);
                if (dataset.Tables[0].Rows.Count > 0)
                {
                    groupchatpage.GroupChatList = dataset.Tables[0];
                    ViewBag.TitleOfPage = "My Interest Group: " + dataset.Tables[0].Rows[0]["strText"].ToString();
                    ViewBag.EventPkey = data.EventId;
                }
                else
                    return RedirectToAction("Index", "Home");

            }
            return View(groupchatpage);
        }

        public ActionResult _MyChat()
        {
            return PartialView();
        }
        #endregion

        #region MyEventSummary

        [CustomizedAuthorize]
        public ActionResult MyConference()
        {
            MyConference MyEventSummary = new MyConference();

            if (User.Identity.AuthenticationType == "Forms")
            {
                clsLastUsed cLast = (clsLastUsed)Session["cLastUsed"];
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                SqlConnection conn = new SqlConnection(ReadConnectionString());

                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                string intRegistrationLevel_pKey = "";
                int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);
                int RegistraionLevelPkey = 0;
                int.TryParse(intRegistrationLevel_pKey, out RegistraionLevelPkey);

                if (!(data.StaffMember || data.GlobalAdmin))
                    if (RegistraionLevelPkey == clsEventAccount.REGISTRATION_SlidesOnly || RegistraionLevelPkey == clsEventAccount.REGISTRATION_ExhibitOnly)
                        return RedirectToAction("Index", "Home");

                ((clsFormList)Session["cFormlist"]).LoadPage(conn, null, System.Web.HttpContext.Current.Request, "My Conference", "", Request.QueryString);

                if (Request.QueryString["PK"] != null)
                {
                    int EventpKey = 0;
                    int.TryParse(Request.QueryString["PK"], out EventpKey);
                    if (EventpKey > 0)
                        data.EventId = EventpKey;
                }
                clsEvent cEvent = new clsEvent();
                cEvent.intEvent_PKey = data.EventId;
                cEvent.sqlConn = conn;
                if (!cEvent.LoadEvent())
                    return RedirectToAction("Index", "Home");

                DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                DateTime dCalTime = clsEvent.getCaliforniaTime();
                ViewBag.dtCurrentStart = clsUtility.getStartOfDay(dtCurrentTime);
                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.intEvent_pKey = data.EventId;
                cEventAccount.sqlConn = conn;
                cEventAccount.intAccount_pKey = data.Id;
                if (!cEventAccount.LoadEventInfo(true, PartnerPanel: true))
                    return Redirect("/Registration");
                else if (cEventAccount.intParticipationStatus_pKey == clsEventAccount.PARTICIPATION_Cancelled)
                    return Redirect("/Registration");


                ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);
                ViewBag.strCurEventID = cEvent.strEventID;
                ViewBag.dtEventStart = clsUtility.getStartOfDay(cEvent.dtStartDate);

                bool showSpeakerPass = cEvent.bShowSpeakerPass && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "ShowSpeakerPass");
                ViewBag.dtEventEnd = clsUtility.getEndOfDay(cEvent.dtEndDate);
                ViewBag.strFileGUID = cEvent.strFileGUID;
                ViewBag.dtEarlyBirdDate = cEvent.dtEarlyBirdDate;
                ViewBag.dtEarlyBirdDate2 = cEvent.dtEarlyBirdDate2;
                ViewBag.dtEarlyBirdDate3 = cEvent.dtEarlyBirdDate3;
                ViewBag.bShowSpeakerPass = showSpeakerPass;
                ViewBag.lblAttName = data.FirstName + " " + data.LastName;
                ViewBag.leftPanel_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
                ViewBag.lblTitle = cEvent.strEventFullname + ": My Event Summary";
                //ViewBag.pLeftChatPanel_visible = (cEvent.bShowDemoAccount && (data.GlobalAdmin || data.IsPartner ),true,cEvent.bChatPanelOnOff)

                ViewBag.VirtualDropdown_Visible = ViewBag.leftPanel_Visible;
                int intReceiptNumber = 0;
                string strGroupCode = "", lblRegBal2 = "", lblRegBal = "";
                bool phGroupCode = false, bPaid = false, pnlTLVisible = false, pnlInvoice = false, phInvoice = false;
                double dblAccountBalance = 0, dblAccountAmount = 0, intPaidAmount = 0;
                var dt = dba.GetPaymentMethod(data.Id, data.EventId);
                if (cLast.intEventType_PKey == clsEvent.EventType_CloudConference || cLast.intEventType_PKey == clsEvent.EventType_HybridConference)
                    pnlTLVisible = false;

                var dtreceipt = clsReceipt.getAllReceipt(data.Id, data.EventId);
                MyEventSummary.tblRecipts = dtreceipt;

                if (dtreceipt.Rows.Count > 0)
                {
                    phInvoice = true;
                    ViewBag.btnReceiptText = dtreceipt.Rows[0]["ReceiptNumberText"].ToString();
                }
                else
                    phInvoice = false;

                DateTime dtReceiptDate = new DateTime(1980, 1, 1);
                if (dt != null && dt.Rows.Count > 0)
                {
                    string getPaymentMethod = dt.Rows[0]["PaymentMethod"].ToString();
                    intReceiptNumber = Convert.ToInt32(dt.Rows[0]["ReceiptNumber"].ToString());
                    strGroupCode = dt.Rows[0]["GroupCode"].ToString();
                    dblAccountBalance = Convert.ToDouble(dt.Rows[0]["Balance"].ToString());
                    dblAccountAmount = Convert.ToDouble(dt.Rows[0]["tAmount"].ToString());
                    intPaidAmount = Convert.ToDouble(dt.Rows[0]["Amount"].ToString());
                    bPaid = Convert.ToBoolean(dt.Rows[0]["Paid"].ToString());
                    dtReceiptDate = Convert.ToDateTime(dt.Rows[0]["PaymentDate"].ToString());
                }
                phGroupCode = (strGroupCode != "");
                ViewBag.ReciptNo = intReceiptNumber;
                ViewBag.phGroupCode = phGroupCode;
                ViewBag.strGroupCode = strGroupCode;
                ViewBag.lblRegDt = " on " + dtReceiptDate.ToShortDateString() + ")";
                ViewBag.DoubleEventAcouuntBalnace = dblAccountBalance;
                double balance = (dblAccountBalance == 0 ? cEventAccount.dblAccountBalance : dblAccountBalance);
                ViewBag.btnPayNowVisible = (balance < 0);

                if (bPaid)
                {
                    ViewBag.btnGetReceiptText = "R" + intReceiptNumber;
                    lblRegBal = "(Receipt ";
                    lblRegBal2 = "(Receipt ";
                }
                else
                {
                    ViewBag.btnGetReceiptText = "N" + intReceiptNumber;
                    lblRegBal = "(Invoice ";
                    lblRegBal2 = "(Invoice ";
                }

                bool bMainPayer = dba.ISMAinPayer(data.Id, data.EventId);
                bool bRegOrSpeaker = (cEventAccount.intParticipationStatus_pKey >= 0 || cEventAccount.bSpeakerAtEvent);
                ViewBag.phAttend = bRegOrSpeaker;                                                                       //phInstruct / phAttend
                ViewBag.phNotRegistered = !bRegOrSpeaker;
                ViewBag.LblInstruction = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.TEXT_MyConference), cEvent);
                ViewBag.bSpeakerAtEvent = cEventAccount.bSpeakerAtEvent;
                ViewBag.lblInstructAmbassador = cSettings.getText(clsSettings.Text_AmbassadorInstructTitle);
                clsVenue cVenue = new clsVenue();
                cVenue.sqlConn = conn;
                cVenue.intVenue_PKey = cEvent.intVenue_PKey;
                cVenue.LoadVenue();

                ViewBag.lblConference = cEvent.strEventFullname;
                ViewBag.lblLocation = cVenue.ReplaceReservedWords("[VenueName], [VenueCity], [VenueState]").TrimEnd(' ', ',');
                ViewBag.lblDates = cEvent.dtStartDate.ToShortDateString() + " - " + cEvent.dtEndDate.ToShortDateString();
                ViewBag.lblRegistrationType = cEventAccount.strRegistrationLevelID + ((cEventAccount.intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDay || cEventAccount.intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayPhysical || cEventAccount.intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayVirtual) ? " (" + cEventAccount.strOneDayName + ")" : "");

                if (cEventAccount.dtExpectedArrival.ToString("dd/MM/yyyy") == "01/01/1980" || cEventAccount.strTravelDetails.Trim() == "")
                {
                    ViewBag.btnTravelText = "Enter Travel Plans";
                    ViewBag.btnTravelUnderline = true;
                    ViewBag.lblTravelVisible = false;
                    ViewBag.btnTravelVisible = true;
                }
                else
                {
                    ViewBag.lblTravelVisible = true;
                    ViewBag.lblTravelText = cEventAccount.strTravelDetails;
                    ViewBag.btnTravelVisible = false;
                }
                if (cEventAccount.dtExpectedArrival.ToString("dd/MM/yyyy") == "01/01/1980" || cEventAccount.strLodgingDetails.Trim() == "")
                {

                    ViewBag.btnCheckInOutText = "Enter Lodging Plans";
                    ViewBag.btnCheckInOutUnderline = true;
                    ViewBag.lblLodgingVisible = false;
                }
                else
                {
                    ViewBag.lblLodgingVisible = true;
                    ViewBag.lblLodgingText = cEventAccount.strLodgingDetails;
                    ViewBag.btnCheckInOutText = " (Check In: " + cEventAccount.dtExpectedArrival.ToString("MMM dd") + " - Check Out: " + cEventAccount.dtExpectedDeparture.ToString("MMM dd") + ")";
                }

                if (cEvent.bSch)
                    cEvent.bSch = cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "MyMAGIMenu_23");

                ViewBag.btnActivityText = cEvent.bSch ? "Available" : "Not Yet Available";
                ViewBag.btnScheduleText = cEvent.bSch ? "Available" : "Not Yet Available";
                ViewBag.btnBookText = ((cEvent.dtBookStartDate <= clsUtility.getStartOfDay(clsEvent.getEventVenueTime())) && (cEvent.dtBookEndDate >= clsUtility.getStartOfDay(clsEvent.getEventVenueTime()))) ? "Available" : "Not Yet Available";
                ViewBag.btnOptionsText = (cEvent.bOptions && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "MyMAGIMenu_19")) ? "Available" : "Not Yet Available";
                ViewBag.btnActivityEnabled = cEvent.bSch;
                ViewBag.btnScheduleEnabled = cEvent.bSch;
                ViewBag.btnBookEnabled = cEvent.dtBookStartDate <= clsUtility.getStartOfDay(clsEvent.getEventVenueTime()) && cEvent.dtBookEndDate >= clsUtility.getStartOfDay(clsEvent.getEventVenueTime());
                ViewBag.btnOptionsEnabled = (cEvent.bOptions && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "MyMAGIMenu_19"));

                if (balance < 0)
                {
                    DataTable dtInv = new DataTable();
                    lblRegBal2 = "";
                    ViewBag.btnReceiptText = "";
                    ViewBag.btnReceiptVisible = false;
                    pnlInvoice = false;
                    dtInv = clsEventAccount.getInvoiceByAmount(data.Id, data.EventId, balance);
                    if (dtInv.Rows.Count > 0)
                    {
                        lblRegBal2 = "(Invoice ";
                        ViewBag.btnReceiptText = "N" + Convert.ToString(dtInv.Rows[0]["ReceiptNumber"]);
                        pnlInvoice = true;
                        ViewBag.btnReceiptVisible = true;
                    }
                }
                ViewBag.pnlInvoice = pnlInvoice;
                if ((balance < 0) && bPaid && dba.CheckPaidOrNot(intReceiptNumber.ToString()))
                {
                    ViewBag.phUnPaid = true;
                    ViewBag.phPaid = true;
                    ViewBag.lblAmt2ForeColor = "Red";
                    ViewBag.lblAmt2 = Math.Round(balance).ToString().Replace("-", "$");
                    lblRegBal2 = (balance == 0 ? lblRegBal2 : " " + lblRegBal2);
                    ViewBag.lblAmt1ForeColor = "Green";
                    ViewBag.lblAmt1 = "$" + intPaidAmount.ToString();
                    ViewBag.lblPayStatus = "Partial Balance Due";
                    lblRegBal = balance == 0 ? lblRegBal : " " + lblRegBal;
                }
                else if (balance < 0)
                {
                    ViewBag.phUnPaid = true;
                    ViewBag.phPaid = false;
                    ViewBag.lblAmt2ForeColor = "Red";
                    ViewBag.lblAmt2 = Math.Round(balance).ToString().Replace("-", "$");
                    ViewBag.lblPayStatus = "Full Balance Due";
                    lblRegBal2 = (balance == 0 ? lblRegBal2 : " " + lblRegBal2);
                }
                else if (balance == 0)
                {
                    ViewBag.phUnPaid = false;
                    ViewBag.phPaid = true;
                    ViewBag.lblAmt1ForeColor = "Green";
                    ViewBag.lblAmt1 = "$" + (bMainPayer ? Math.Round(intPaidAmount).ToString() : dblAccountAmount.ToString());
                    ViewBag.lblPayStatus = "No Balance Due";
                    lblRegBal = (balance == 0 ? lblRegBal : " " + lblRegBal);
                }
                else
                {
                    ViewBag.phUnPaid = false;
                    ViewBag.phPaid = true;
                    ViewBag.lblAmt1ForeColor = "Green";
                    ViewBag.lblAmt1 = "$" + intPaidAmount.ToString();
                    ViewBag.lblPayStatus = "Overpaid ($" + Math.Round(intPaidAmount).ToString() + ")";
                    lblRegBal = (balance == 0 ? lblRegBal : " " + lblRegBal);
                }

                if (intReceiptNumber == 0)
                    ViewBag.phPaid = false;

                ViewBag.lblRegBal = lblRegBal;
                ViewBag.lblRegBal2 = lblRegBal2;

                ViewBag.pnlTL = (cLast.intEventType_PKey != clsEvent.EventType_CloudConference && cLast.intEventType_PKey != clsEvent.EventType_HybridConference);

                double dblRefundBalance = clsEventAccount.getAccountBalance(data.Id, data.EventId);
                int cancelPopupField = 0;
                if (cEventAccount.bSpeakerAtEvent)
                    cancelPopupField = 1;
                else if (cEventAccount.intParticipationStatus_pKey == clsEventAccount.PARTICIPATION_Provisional)
                    cancelPopupField = 2;
                else if (cEventAccount.intParticipationStatus_pKey == clsEventAccount.PARTICIPATION_Attending && cEventAccount.dblAccountBalance == 0 && dblRefundBalance == 0)
                    cancelPopupField = 3;

                ViewBag.cancelPopupField = cancelPopupField;



                ViewBag.phInvoice = phInvoice;
                ViewBag.phViewSpkChr = (cEventAccount.bSpeakerAtEvent || cEventAccount.bChairAtEvent);
                ViewBag.btnInstructSpkVisible = cEventAccount.bSpeakerAtEvent;
                ViewBag.btnInstructLdrVisible = cEventAccount.bLeaderAtEvent;
                ViewBag.btnInstructModtVisible = cEventAccount.bModtrAtEvent;
                ViewBag.btnInstructChrVisible = cEventAccount.bChairAtEvent;
                ViewBag.phPartner = cEventAccount.bIsPartner;
                ViewBag.bSpeakerAtEvent = cEventAccount.bSpeakerAtEvent;
                ViewBag.btnSpkDinVisible = cEventAccount.bSpeakerAtEvent;
                ViewBag.lblSpkDinVisible = cEventAccount.bSpeakerAtEvent;
                ViewBag.btnSpkCert = cEventAccount.bSpeakerAtEvent;
                ViewBag.lblSpkCert = cEventAccount.bSpeakerAtEvent;
                ViewBag.btnSpkFeed = cEventAccount.bSpeakerAtEvent;
                ViewBag.lblSpkFeed = cEventAccount.bSpeakerAtEvent;
                ViewBag.lblSpkDin = cEventAccount.bAttendingDinner ? "Signed up for the speaker dinner" : "Sign up for the speaker dinner";
                ViewBag.lblSpkFeed = cEventAccount.bSpeakerFeedbackPosted ? "View attendee feedback for your presentations*" : "Feedback for this event is not yet available*";

                if (ViewBag.btnInstructSpkVisible)
                {
                    var dtspk = dba.RefreshSpeakerPanel((showSpeakerPass), dtCurrentTime, data.Id, data.EventId);
                    MyEventSummary.tblSessions = dtspk;
                }

                if (ViewBag.btnInstructChrVisible)
                {
                    var dtChair = dba.RefreshChairPanel(dtCurrentTime, data.Id, data.EventId);
                    MyEventSummary.tblChairs = dtChair;
                }

                if (ViewBag.phPartner)
                {
                    var _partner = dba.PartnerAlias(data.EventId, data.Id);
                    ViewData["Partners"] = _partner;
                }

                clsAccount cAccount = new clsAccount();
                cAccount.sqlConn = conn;
                cAccount.intAccount_PKey = data.Id;
                cAccount.LoadAccount();

                clsEventOrganization c = new clsEventOrganization();
                c.intOrganization_PKey = data.ParentOrganization_pKey;
                c.intEvent_PKey = data.EventId;
                c.sqlConn = conn;
                c.LoadByOrgAndEvent();

                bool trContract = (c.strContractFileName != "");
                ViewBag.trContract = trContract;
                ViewBag.btnContractNavigateUrl = "~/contractdocuments/" + c.strContractFileName;
                ViewBag.lblAmbassadorInstruction = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.Text_AmbassadorInstruct), cEvent);
                var dtSpecialEvent = dba.SpecialEventPanel(data.Id, data.EventId);

                ViewBag.phAmbassador = (cEventAccount.bIsAmbassador && bRegOrSpeaker);
                ViewBag.phSpecialEvent = dba.CheckSpecialEventParticipant(data.Id, data.EventId);
                ViewBag.tr1 = (cEvent.intEventType_PKey != clsEvent.EventType_CloudConference && cEvent.intEventType_PKey != clsEvent.EventType_HybridConference);
                ViewBag.trdinner = cEvent.bDinnerActive;

                ViewBag.trActivity = (cEvent.bSch && (intAttendeeStatus == 1 || (cEvent.intAttSch == 0 && intAttendeeStatus == 3)) && (RegistraionLevelPkey == clsEventAccount.REGISTRATION_FullReg || RegistraionLevelPkey == clsEventAccount.REGISTRATION_FullRegVirtual  || RegistraionLevelPkey == clsEventAccount.REGISTRATION_StudentOnly || RegistraionLevelPkey == clsEventAccount.REGISTRATION_OneDay|| RegistraionLevelPkey == clsEventAccount.REGISTRATION_OneDayPhysical || RegistraionLevelPkey == clsEventAccount.REGISTRATION_OneDayVirtual || cAccount.intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_SingleTrackOnly || RegistraionLevelPkey == clsEventAccount.REGISTRATION_ExhibitOnly)) || cAccount.bGlobalAdministrator || (intAttendeeStatus == 1 && cAccount.bStaffMember);
                ViewBag.trSchedule = (cEvent.bSch && (intAttendeeStatus == 1 || (cEvent.intAttSch == 0 && intAttendeeStatus == 3)) && (RegistraionLevelPkey == clsEventAccount.REGISTRATION_FullReg || RegistraionLevelPkey == clsEventAccount.REGISTRATION_FullRegVirtual  || RegistraionLevelPkey == clsEventAccount.REGISTRATION_StudentOnly || RegistraionLevelPkey == clsEventAccount.REGISTRATION_OneDay|| RegistraionLevelPkey == clsEventAccount.REGISTRATION_OneDayPhysical || RegistraionLevelPkey == clsEventAccount.REGISTRATION_OneDayVirtual || cAccount.intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_SingleTrackOnly || RegistraionLevelPkey == clsEventAccount.REGISTRATION_ExhibitOnly)) || cAccount.bGlobalAdministrator || (intAttendeeStatus == 1 && cAccount.bStaffMember);
                ViewBag.trOptions = ((intAttendeeStatus == 1 || (cEvent.intAttOptions == 0 && intAttendeeStatus == 3)) && (cEvent.bOptions && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "MyMAGIMenu_19")) && (RegistraionLevelPkey == clsEventAccount.REGISTRATION_FullReg || RegistraionLevelPkey == clsEventAccount.REGISTRATION_FullRegVirtual || RegistraionLevelPkey == clsEventAccount.REGISTRATION_StudentOnly || RegistraionLevelPkey == clsEventAccount.REGISTRATION_OneDay|| RegistraionLevelPkey == clsEventAccount.REGISTRATION_OneDayPhysical|| RegistraionLevelPkey == clsEventAccount.REGISTRATION_OneDayVirtual || cAccount.intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_SingleTrackOnly || RegistraionLevelPkey == clsEventAccount.REGISTRATION_ExhibitOnly)) || cAccount.bGlobalAdministrator || (intAttendeeStatus == 1 && cAccount.bStaffMember);

                string apppath = clsSettings.APP_URL().Replace("forms", "");
                DataSet ds = new SqlOperation().getBookLetInfo(conn, apppath, data.EventId, cEvent.bShowDemoAccount, data.GlobalAdmin, cAccount.bIsPartner, data.Id, dtCurrentTime);
                ViewBag.lblsNetworkingLevel = "";
                ViewBag.NoResult = "";
                ViewBag.dgBooklet = new DataTable();

                if (ds != null)
                {
                    if (!(ds.Tables[0].Rows.Count > 0))
                        ViewBag.NoResult = true;

                    ViewBag.dgBooklet = ds.Tables[0];
                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                    {
                        int PeopleAtNextLevel = 0;

                        string strImagePath = "/accountimages/" + data.Id.ToString() + "_img.jpg",
                                strPhysicalPathImages = Server.MapPath(strImagePath);

                        string levelName = "", netLevelKey = "", awardedBagde = "", StrNextLevel = "";
                        bool bExists = clsUtility.FileExists(strPhysicalPathImages);
                        bool bioExists = Convert.ToBoolean(ds.Tables[1].Rows[0]["hasBio"].ToString());
                        int AttainedPoints = Convert.ToInt32(ds.Tables[1].Rows[0]["AttainedPoints"].ToString());
                        int maxLimit = Convert.ToInt32(ds.Tables[1].Rows[0]["maxLimit"].ToString());

                        int decValue = (AttainedPoints / maxLimit);
                        int nextLevel = 0, overallrows = ds.Tables[1].Rows.Count;

                        for (int i = 0; i < overallrows; i++)
                        {

                            if (AttainedPoints < Convert.ToInt32(ds.Tables[1].Rows[i]["pnts"]))
                                break;
                            levelName = "";
                            nextLevel = i + 1;
                            nextLevel = ((overallrows > nextLevel) ? nextLevel : 0);
                            if (Convert.ToBoolean(ds.Tables[1].Rows[i]["imgReq"]) || Convert.ToBoolean(ds.Tables[1].Rows[i]["BioReq"]))
                            {
                                StrNextLevel = ds.Tables[1].Rows[i]["lvl"].ToString();
                                if (!bExists && !bioExists)
                                {
                                    levelName = awardedBagde + " (" + AttainedPoints.ToString() + " points, profile image and bio data required for " + StrNextLevel + " level)";
                                    break;
                                }
                                else if (!bExists)
                                {
                                    levelName = awardedBagde + " (" + AttainedPoints.ToString() + " points, profile image required for " + StrNextLevel + " level)";
                                    break;
                                }
                                else if (!bioExists)
                                {
                                    levelName = awardedBagde + " (" + AttainedPoints.ToString() + " points, bio data required for " + StrNextLevel + " level)";
                                    break;
                                }

                                awardedBagde = ds.Tables[1].Rows[i]["lvl"].ToString();
                                netLevelKey = ds.Tables[1].Rows[i]["levelKey"].ToString();
                                PeopleAtNextLevel = Convert.ToInt32(ds.Tables[1].Rows[nextLevel]["cnt"]);
                            }

                            awardedBagde = ds.Tables[1].Rows[i]["lvl"].ToString();
                            netLevelKey = ds.Tables[1].Rows[i]["levelKey"].ToString();

                            if (nextLevel != 0)
                                levelName = ds.Tables[1].Rows[i]["lvl"].ToString() + " (" + AttainedPoints.ToString() + " points, " + (Convert.ToInt32(ds.Tables[1].Rows[nextLevel]["pnts"].ToString()) - AttainedPoints).ToString() + " required for next level)";
                            else
                                levelName = ds.Tables[1].Rows[i]["lvl"].ToString() + " (" + AttainedPoints.ToString() + " points)";

                            PeopleAtNextLevel = Convert.ToInt32(ds.Tables[1].Rows[nextLevel]["cnt"]);
                        }
                        ViewBag.lblsNetworkingLevel = levelName;
                        if (nextLevel != 0)
                            ViewBag.lblsNetworkingLevel = levelName + " " + ((PeopleAtNextLevel == 0) ? "" : PeopleAtNextLevel.ToString() + " people are at next level");
                    }
                }
                ViewBag.trNetworkingLEvel = ViewBag.leftPanel_Visible;
                ViewBag.cmdConfFeedback = ((ViewBag.leftPanel_Visible) ? ((cEvent.intEventType_PKey == clsEvent.EventType_CloudConference || cEvent.intEventType_PKey == clsEvent.EventType_HybridConference) && (cAccount.bGlobalAdministrator || cAccount.bStaffMember || (dtCurrentTime >= ViewBag.dtEventStart))) : false);
                ViewBag.PaymentMethods = new SqlOperation().getPaymentMethodList();

                MyEventSummary.tblSpecialEvent = dtSpecialEvent;

                ViewBag.METHOD_Voucher = clsPayment.METHOD_Voucher;
                ViewBag.METHOD_Credit = clsPayment.METHOD_Credit;
                ViewBag.METHOD_Check = clsPayment.METHOD_Check;
                ViewBag.METHOD_Wire = clsPayment.METHOD_Wire;
                ViewBag.intTobePaidAmount = Math.Abs(cEventAccount.dblAccountBalance);
                ViewBag.CmdEventChanges = new SqlOperation().IsEventChange();
                int RegLevelPKey = Convert.ToInt32(intRegistrationLevel_pKey);
                if (ViewBag.VirtualDropdown_Visible)
                {
                    DateTime dtCalTime = clsEvent.getCaliforniaTime();
                    string Host = HttpContext.Request.Path.ToUpper().Replace("/MYMAGI", "");
                    ViewBag.ddEventVirtualData = repository.GetVirtualEventDropDownInfo(Host, data.Id, data.EventId, dtCurrentTime, dtCalTime, data.ParentOrganization_pKey, intAttendeeStatus, RegLevelPKey, data.GlobalAdmin, data.StaffMember);
                    ViewBag.SelectedDropDown = GetSelectedVirtualDropDown(Host);
                }
                ViewBag.trMyBooks = ((intAttendeeStatus == 1) && (dCalTime >= cEvent.dtBookStartDate && dCalTime < cEvent.dtBookEndDate.AddDays(1).Date) && (RegistraionLevelPkey != clsEventAccount.REGISTRATION_ExhibitOnly) && (RegistraionLevelPkey != clsEventAccount.REGISTRATION_SingleSessionOnly)) || cAccount.bGlobalAdministrator || (intAttendeeStatus == 1 && cAccount.bStaffMember);
                ViewBag.cmdBook = (cEvent.dtBookStartDate <= ViewBag.dtCurrentStart && cEvent.dtBookEndDate >= ViewBag.dtCurrentStart);
                ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).bMediaPlaying = false;
                ViewBag.HelpIconInfo = repository.PageLoadResourceData(data, "44", "14");

                bool bShowSurveyQuestion = false;
                DataTable EventFeatures = repository.getDyamicEventSettings(data.EventId, "ISNULL(ShowSurveyQuestion,'0') as ShowSurveyQuestion");
                if (EventFeatures != null && EventFeatures.Rows.Count > 0)
                    bShowSurveyQuestion = (EventFeatures.Rows[0]["ShowSurveyQuestion"] != System.DBNull.Value) ? Convert.ToBoolean(EventFeatures.Rows[0]["ShowSurveyQuestion"]) : false;

                int selectedEvent = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).intEventSelector;
                bool bSponsor = clsEventOrganization.CheckExhibitor(data.ParentOrganization_pKey, selectedEvent);
                ViewBag.lblRegText = "";
                ViewBag.OpenSurveyRadWindow=false;
                LoadRegistrationQuestions(data, intAttendeeStatus, bShowSurveyQuestion, bSponsor, intRegistrationLevel_pKey);

                return View(MyEventSummary);
            }
            return null;
        }

        [CustomizedAuthorize]
        public ActionResult GetEventHandBook()
        {
            if (User.Identity.AuthenticationType == "Forms")
            {

                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                int AccountPkey = data.Id;
                int EventPkey = data.EventId;

                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                clsEvent cEvent = new clsEvent();
                cEvent.intEvent_PKey = EventPkey;
                cEvent.sqlConn = new SqlConnection(ReadConnectionString());
                cEvent.LoadEvent();

                string strDestDir = Server.MapPath("~/Documents/");

                var dt = dba.EventHandbook(EventPkey, cEvent.strFileGUID, cSettings.UsageType_CloudEventHandbook);

                if (dt.Rows.Count > 0)
                {
                    string strDisplayName = clsUtility.CleanFilename(dt.Rows[0]["FileName"].ToString() + ".pdf");
                    strDisplayName = dt.Rows[0]["FileName"].ToString();

                    return Json(new { msg = "OK", strDisplayName, }, JsonRequestBehavior.AllowGet);
                    //Response.ClearContent();
                    //Response.Clear();
                    //Response.ContentType = "application/pdf";
                    //Response.AddHeader("Content-Disposition", "attachment; filename=""" & strDisplayName & """");
                    //Response.TransmitFile(strDestDir + strDisplayName);
                    //Response.Flush();
                    //Response.End();
                }
                else
                {
                    return Json(new { msg = "notready", descp = "Event handbook not available" }, JsonRequestBehavior.AllowGet);
                }
            }
            return null;
        }

        [CustomizedAuthorize]
        public ActionResult GetSpeakerHandBook()
        {
            if (User.Identity.AuthenticationType == "Forms")
            {

                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                int AccountPkey = data.Id;
                int EventPkey = data.EventId;

                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                clsEvent cEvent = new clsEvent();
                cEvent.intEvent_PKey = EventPkey;
                cEvent.sqlConn = new SqlConnection(ReadConnectionString());
                cEvent.LoadEvent();

                var spkdt = dba.SpeakerHandBook(cSettings.UsageType_SpeakerGuideline, EventPkey, cEvent.strFileGUID);
                string strDisplayname = "";
                if (spkdt.Rows.Count > 0)
                {
                    strDisplayname = spkdt.Rows[0]["FileName"].ToString();
                }
                else
                {
                    return Json(new { result = "ERROR", elink = "Pdf file not uploaded." }, JsonRequestBehavior.AllowGet);
                }
                string s = "frmPopupFile.aspx?EPK=" + clsUtility.TYPE_Event.ToString() + "&PK=" + EventPkey.ToString();
                s = s + "&DN=" + HttpUtility.UrlEncode(strDisplayname);
                s = s + "&N=" + HttpUtility.UrlEncode(strDisplayname);

                return Json(new { result = "OK", elink = s }, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        [CustomizedAuthorize]
        public FileResult DownloadEventHAndBook(string FileName)
        {
            string strTargetFile = Server.MapPath("~/Documents/" + FileName);
            if (System.IO.File.Exists(strTargetFile))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(strTargetFile);
                return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, FileName);

            }
            return null;

        }

        public JsonResult GetPopupText(string popupform)
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);

            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int AccountPkey = data.Id;
            int EventPkey = data.EventId;

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EventPkey;
            cEvent.sqlConn = new SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();

            if (popupform == "AttendeeHelp")
            {
                string AttendeeHelpPopText = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.TEXT_MyConferenceInstruct), cEvent);
                return Json(new { msg = "OK", AttendeeHelpPopText }, JsonRequestBehavior.AllowGet);
            }


            else if (popupform == "LeadersGuidelines")
            {
                string LeadersGuidelinesPopText = cSettings.getText(clsSettings.Text_LeaderInstruct);
                return Json(new { msg = "OK", LeadersGuidelinesPopText }, JsonRequestBehavior.AllowGet);
            }

            else if (popupform == "ModeratorsGuidelines")
            {
                string ModeratorsGuidelinesPopText = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.Text_ModeratorInstruct));
                return Json(new { msg = "OK", ModeratorsGuidelinesPopText }, JsonRequestBehavior.AllowGet);
            }
            else if (popupform == "ChairpersonsGuidelines")
            {
                string ChairpersonsGuidelinesPopText = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.TEXT_MyChairPersonInstruct), cEvent);
                return Json(new { msg = "OK", ChairpersonsGuidelinesPopText }, JsonRequestBehavior.AllowGet);

            }
            else if (popupform == "SpeakersGuidelines")
            {
                string ChairpersonsGuidelinesPopText = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.TEXT_MySpeakerInstruct), cEvent);
                return Json(new { msg = "OK", ChairpersonsGuidelinesPopText }, JsonRequestBehavior.AllowGet);
            }

            return null;
        }

        [AjaxValidateAntiForgeryToken]
        [CustomizedAuthorize]
        public JsonResult cmdVoucherValidate(string VoucherNo)
        {
            SqlConnection conn = new SqlConnection(ReadConnectionString());
            clsVoucher c = new clsVoucher();
            c.sqlConn = conn;
            int voucherpKey = 0;
            int.TryParse(VoucherNo.Replace("V", ""), out voucherpKey);
            c.intVoucher_pKey= voucherpKey;
            c.LoadVoucher();
            DateTime _date = DateTime.Now;
            int intIssuedForEvent_pKey = 0;
            string lblErrorVoucher = "", lblVoucherAmount = "", strIssuedForEvent = "", txtVoucherPin = "";
            bool VPinVisible = false, VCodeVisible = false, trVPinReqVisible = false, lblVoucherInstVisible = false;

            int AccountID = Convert.ToInt32(User.Identity.Name);
            if (!c.bIsValid)
                lblErrorVoucher = "The entered voucher code does not exist.";
            else if (c.bIsUsed)
                lblErrorVoucher = "The entered voucher code is already used.";
            else if (c.dtIssuedOn < _date && _date > c.dtExpirationDate)
                lblErrorVoucher = "The entered voucher code is expired.";
            else
            {
                if (c.intAccount_pkey == AccountID)
                    c.strVoucherCode = "";
                VPinVisible = true;
                VCodeVisible = false;
                lblErrorVoucher = "";
                lblVoucherAmount = "Voucher Amount: " + String.Format("{0:c}", c.dblAmount);
                strIssuedForEvent = c.strIssuedForEvent;
                intIssuedForEvent_pKey = c.intIssuedForEvent_pKey;

                if (c.strVoucherCode != "")
                {
                    trVPinReqVisible = true;
                    lblVoucherInstVisible = true;
                }
            }

            return Json(new
            {
                IssuedForEvent_pKey = intIssuedForEvent_pKey,
                vPinVisible = VPinVisible,
                vCodeVisible = VCodeVisible,
                sErrorVoucher = lblErrorVoucher,
                sVoucherAmount = lblVoucherAmount,
                sIssuedForEvent = strIssuedForEvent,
                trVpin = trVPinReqVisible,
                VoucherInstVisible = lblVoucherInstVisible,
                sVoucherPin = txtVoucherPin
            }, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        [CustomizedAuthorize]
        public JsonResult cmdSaveVoucher(string VoucherNo, string txtVoucherPin)
        {
            string ErrorMsg = "";
            try
            {
                if (VoucherNo == "" || VoucherNo == "0")
                    ErrorMsg = "The entered voucher code is not valid.";

                SqlConnection conn = new SqlConnection(ReadConnectionString());
                clsVoucher c = new clsVoucher();
                c.sqlConn = conn;
                c.intVoucher_pKey = Convert.ToInt32(VoucherNo.Replace("V", ""));
                c.LoadVoucher();
                DateTime _date = new DateTime();
                int intIssuedForEvent_pKey = 0, AccountID = Convert.ToInt32(User.Identity.Name), hdnVoucherPkey = 0, EventID = 0;
                string lblErrorVoucher = "", lblVoucherAmount = "", strIssuedForEvent = "", VoucherConfirm = "", hdnVoucherCode = "", hdnRemainingAmount = "", strMemo = "";
                bool VPinVisible = false, VCodeVisible = false, trVPinReqVisible = false, lblVoucherInstVisible = false, popuprwVoucherConfirm = false, bValidVoucher = false;
                double RemainingVoucherAmt = 0;
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                EventID = data.EventId;

                if (c.bIsValid)
                    bValidVoucher = true;
                else
                {
                    ErrorMsg = "The entered voucher code is not valid.";
                    goto Finish;
                }

                if (c.bIsUsed)
                {
                    ErrorMsg = "The entered voucher code is already used.";
                    goto Finish;
                }

                if (c.dtIssuedOn < _date && _date > c.dtExpirationDate)
                {
                    ErrorMsg = "The entered voucher code is expired.";
                    goto Finish;
                }

                if (c.intAccount_pkey == AccountID)
                    c.strVoucherCode = "";

                if (c.strVoucherCode != "")
                {
                    if (string.IsNullOrEmpty(txtVoucherPin))
                    {
                        ErrorMsg = "Enter voucher PIN.";
                        goto Finish;
                    }

                    if (c.strVoucherCode != txtVoucherPin.Trim())
                    {
                        ErrorMsg = "The entered voucher PIN is incorrect.";
                        goto Finish;
                    }
                }

                if (c.strVoucherCode != "" && c.strVoucherCode == txtVoucherPin.Trim())
                {
                    clsEventAccount cEventAccount = new clsEventAccount();
                    cEventAccount.intAccount_pKey = data.Id;
                    cEventAccount.intEvent_pKey = data.EventId;
                    cEventAccount.sqlConn = conn;
                    cEventAccount.LoadEventInfo(true);
                    double dblVoucherAmount = Math.Abs(cEventAccount.dblAccountBalance);

                    if (c.dblAmount > dblVoucherAmount)
                    {
                        RemainingVoucherAmt = c.dblAmount - dblVoucherAmount;
                        VoucherConfirm = "Voucher (ID: #" + VoucherNo + ") of $" + c.dblAmount.ToString() + " used for the payment of $" + dblVoucherAmount.ToString() + ". New voucher for $" + RemainingVoucherAmt.ToString() + " will be generated.";
                        if (RemainingVoucherAmt > 0)
                        {
                            hdnRemainingAmount = RemainingVoucherAmt.ToString();
                            hdnVoucherPkey = c.intVoucher_pKey;
                            hdnVoucherCode = VoucherNo.ToString();
                            popuprwVoucherConfirm = true;
                            ErrorMsg = "OK";
                            goto Finish;
                        }
                    }
                    else
                        dblVoucherAmount = c.dblAmount;

                    int intVoucher_pKey = c.intVoucher_pKey;
                    if (!bValidVoucher)
                    {
                        ErrorMsg = "The entered voucher code is not valid.";
                        goto Finish;
                    }
                    clsPayment cP = new clsPayment();
                    cP.sqlConn = conn;
                    cP.intPaymentMethod_pKey = clsPayment.METHOD_Other;
                    cP.intPayerAcctPKey = AccountID;
                    cP.intLoggedByAccount_pKey = AccountID;
                    cP.intEventPKey = EventID;
                    cP.strIntendedAccounts = AccountID.ToString();

                    string strChargesPkey = "";
                    cP.strSelectedCharges = clsPayment.getPendingCharges(AccountID, EventID, ref strChargesPkey);
                    cP.intReceiptNumber = cP.getReceipt();
                    strMemo = "applied voucher #V" + intVoucher_pKey.ToString("D5");
                    cP.bPaid = true;
                    if (string.IsNullOrEmpty(ErrorMsg))
                        ErrorMsg = "OK";

                    if (!cP.LogVoucherPayment(dblVoucherAmount, clsPayment.METHOD_Other, intVoucher_pKey, strMemo, 0))
                        goto Finish;
                    cP.strMemo = "Used Voucher: #V" + intVoucher_pKey.ToString("D5");
                    if (!cP.ApplyCashToAccounts(dblVoucherAmount, 0, ""))
                        goto Finish;

                    if (Math.Abs(cEventAccount.dblAccountBalance) == dblVoucherAmount)
                    {
                        int intreceiptnumber = 0;
                        intreceiptnumber = clsReceipt.GetAccountReceipt(AccountID, dblVoucherAmount, EventID);
                        if (intreceiptnumber > 0)
                            cP.MarkPaymentAsVoid(intreceiptnumber);
                    }
                }
                else
                    ErrorMsg = "The entered voucher PIN is incorrect.";

                Finish:
                return Json(new
                {
                    msg = ErrorMsg,
                    Memo = strMemo,
                    sVoucherConfirm = VoucherConfirm,
                    RemainingAmount = hdnRemainingAmount,
                    VoucherPkey = hdnVoucherPkey,
                    VoucherCode = hdnVoucherCode,
                    popuprwVoucherConfirm = popuprwVoucherConfirm,
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { msg = ErrorMsg }, JsonRequestBehavior.AllowGet);
            }
        }

        [CustomizedAuthorize]
        public FileResult DownloadSpkCertificate()
        {
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int AccountPkey = data.Id;
            int EventPkey = data.EventId;

            clsAccount cAccount = new clsAccount();
            cAccount.sqlConn = new SqlConnection(ReadConnectionString());
            cAccount.intAccount_PKey = AccountPkey;
            cAccount.LoadAccount();

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EventPkey;
            cEvent.sqlConn = new SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();

            clsEventAccount cEventAccount = new clsEventAccount();
            cEventAccount.intAccount_pKey = AccountPkey;
            cEventAccount.intEvent_pKey = EventPkey;
            cEventAccount.sqlConn = new SqlConnection(ReadConnectionString());
            cEventAccount.LoadEventInfo();

            string strAcctName = cAccount.strFirstname + (cAccount.strMiddlename != "" ? " " + cAccount.strMiddlename : "") + (cAccount.strLastname != "" ? " " + cAccount.strLastname : "");
            string strDestDir = Server.MapPath("~/app_data/downloadtemp/");
            string strMyCertFile = "TempSpeakerCert.pdf";
            string strDisplayName = clsUtility.CleanFilename(cEvent.strEventID + "_Speaker_" + strAcctName + "_" + DateTime.Now.ToString("yyMMdd") + ".pdf");
            if (!cEventAccount.CreateSpeakerCertificate(strDestDir + strMyCertFile, cEvent, cAccount))
            {
                return null;

            }
            //'------------------------------------
            //'--download the file
            //'------------------------------------
            cEventAccount.LogAuditMessage("Download: Speaker Certificate - " + cEvent.strEventID, clsAudit.LOG_DownloadSpk);

            string strTargetFile = Server.MapPath("~/app_data/downloadtemp/" + strMyCertFile);
            if (System.IO.File.Exists(strTargetFile))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(strTargetFile);
                return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, strDisplayName);

            }
            return null;

        }

        public ActionResult DownloadPartnerMInstruct(int intParticipationType)
        {

            if (User.Identity.AuthenticationType == "Forms")
            {

                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                int AccountPkey = data.Id;
                int EventPkey = data.EventId;

                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                clsEvent cEvent = new clsEvent();
                cEvent.intEvent_PKey = EventPkey;
                cEvent.sqlConn = new SqlConnection(ReadConnectionString());
                cEvent.LoadEvent();

                int UsageType = 0;

                if (intParticipationType == 1)
                {
                    UsageType = cSettings.UsageType_ExhibitorPartnerGuideline;
                }
                else if (intParticipationType == 2)
                {
                    UsageType = cSettings.UsageType_OtherPartnerGuideLine;
                }
                else if (intParticipationType == 3)
                {
                    UsageType = cSettings.UsageType_MediaPartnerGuideline;
                }
                else if (intParticipationType == 4)
                {
                    UsageType = cSettings.UsageType_BrandingPartnerGuideline;

                }
                else if (intParticipationType == 5)
                {
                    UsageType = cSettings.UsageType_EventSponsorPartnerGuideline;

                }

                string strDisplayname = "";
                var dt = dba.PartnerMInstruct(EventPkey, cEvent.strFileGUID, UsageType, intParticipationType);
                if (dt.Rows.Count > 0)
                {
                    strDisplayname = dt.Rows[0]["FileName"].ToString();
                }
                else
                {
                    return Json(new { result = "NotFound", msg = "Pdf file not uploaded." }, JsonRequestBehavior.AllowGet);
                }

                string s = "/frmPopupFile.aspx?EPK=" + clsUtility.TYPE_Event.ToString() + "&PK=" + EventPkey.ToString();
                s = s + "&DN=" + HttpUtility.UrlEncode(strDisplayname);
                s = s + "&N=" + HttpUtility.UrlEncode(strDisplayname);
                return Json(new { result = "OK", elink = s }, JsonRequestBehavior.AllowGet);
            }

            return null;

        }
        public ActionResult PTypeGLines(int intParticipationType)
        {
            string lblPopuptext = "";
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int AccountPkey = data.Id;
            int EventPkey = data.EventId;

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EventPkey;
            cEvent.sqlConn = new SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();
            if (intParticipationType == 1)
                lblPopuptext = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.Text_ExhibitorPartnerInstruct), cEvent);
            else if (intParticipationType == 2)
                lblPopuptext = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.Text_OtherPartnerInstruct), cEvent);
            else if (intParticipationType == 3)
                lblPopuptext = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.Text_MediaPartnerInstruct), cEvent);
            else if (intParticipationType == 4)
                lblPopuptext = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.Text_PartnerNonExhibitInfo), cEvent);
            else if (intParticipationType == 5)
                lblPopuptext = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.Text_EventSponsorInstruct), cEvent);
            else
                lblPopuptext = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.Text_PartnerNonExhibitInfo), cEvent);

            string strDisplayname = "";
            StringBuilder qry = new StringBuilder(" Select top 1 pKey,FileName from Event_Guidelines");
            qry.Append(Environment.NewLine + " where Event_pKey = " + data.EventId.ToString());

            if (intParticipationType == 1)
                qry.Append(Environment.NewLine + " And UsageType=" + cSettings.UsageType_ExhibitorPartnerGuideline.ToString());
            else if (intParticipationType == 2)
                qry.Append(Environment.NewLine + " And UsageType=" + cSettings.UsageType_OtherPartnerGuideLine.ToString());
            else if (intParticipationType == 3)
                qry.Append(Environment.NewLine + " And UsageType=" + cSettings.UsageType_MediaPartnerGuideline.ToString());
            else if (intParticipationType == 4)
                qry.Append(Environment.NewLine + " And UsageType=" + cSettings.UsageType_BrandingPartnerGuideline.ToString());
            else if (intParticipationType == 5)
                qry.Append(Environment.NewLine + " And UsageType=" + cSettings.UsageType_EventSponsorPartnerGuideline.ToString());
            else if (intParticipationType == 0)
                qry.Append(Environment.NewLine + " And UsageType=" + cSettings.UsageType_NonExhibitorPartnerGuideline.ToString());
            qry.Append(" order by UploadedOn desc");
            SqlCommand cmd = new SqlCommand(qry.ToString());
            DataTable dt = new DataTable();
            SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
            string error = "OK";
            bool cmdPdfViewVisible = false;
            if (clsUtility.GetDataTable(sqlConn, cmd, ref dt))
            {
                if (dt !=null && dt.Rows.Count>0)
                    strDisplayname = (dt.Rows[0]["FileName"] != System.DBNull.Value) ? dt.Rows[0]["FileName"].ToString() : "";
            }
            else
                error = "Error Occurred";

            string s = "";
            if (strDisplayname == "" && error != "Error Occurred")
            {
                if (intParticipationType == 1)
                    error = "Exhibitor not available.";
                else if (intParticipationType == 2)
                    error = "File not available.";
                else if (intParticipationType == 3)
                    error = "Media not available.";
                else if (intParticipationType == 4)
                    error = "Branding not available.";
                else if (intParticipationType == 5)
                    error = "Sponsor Handbook not available.";
                else if (intParticipationType == 0)
                    error = "Non-exhibitor not available.";
            }
            else
            {
                s = "/PopupFile?EPK=" + clsUtility.TYPE_Event.ToString() + "&PK=" + data.EventId.ToString()
                        + "&DN=" + HttpUtility.UrlEncode(strDisplayname)   + "&N=" + HttpUtility.UrlEncode(strDisplayname);
            }

            return Json(new { result = error, guideLineText = lblPopuptext, popFileURL = s }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PopupPdfView(string strViewPdfType, int intParticipationType)
        {
            string link = "";
            if (strViewPdfType.ToUpper() == "SPEAKER")
            {
                link = ViewPdfFile("SPEAKER");
            }
            else if (strViewPdfType.ToUpper() == "CHAIR")
            {
                link = ViewPdfFile("CHAIR");

            }
            else if (strViewPdfType.ToUpper() == "MEDIA")
            {
                link = ViewPartnerTypePdf("MEDIA", intParticipationType);
            }
            else if (strViewPdfType.ToUpper() == "OTHER")
            {
                link = ViewPartnerTypePdf("OTHER", intParticipationType);
            }
            else if (strViewPdfType.ToUpper() == "BRANDING")
            {
                link = ViewPartnerTypePdf("BRANDING", intParticipationType);
            }
            else if (strViewPdfType.ToUpper() == "EXHIBIT")
            {
                link = ViewPartnerTypePdf("EXHIBIT", intParticipationType);
            }
            else if (strViewPdfType.ToUpper() == "EVENTSPONSOR")
            {
                link = ViewPartnerTypePdf("EVENTSPONSOR", intParticipationType);
            }
            else if (strViewPdfType.ToUpper() == "NONEXHIBIT")
            {
                link = ViewPartnerTypePdf("NONEXHIBIT", intParticipationType);
            }
            if (link != "")
            {
                return Json(new { result = "OK", link }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = "ERROR", msg = "Pdf file not uploaded." }, JsonRequestBehavior.AllowGet);
            }
        }

        public string ViewPdfFile(string type)
        {
            string UsageType = "";
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int AccountPkey = data.Id;
            int EventPkey = data.EventId;

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EventPkey;
            cEvent.sqlConn = new SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();

            if (type == "CHAIR")
            {
                UsageType = cSettings.UsageType_ChairGuideline.ToString();
            }
            else
            {
                UsageType = cSettings.UsageType_SpeakerGuideline.ToString();
            }

            string strDisplayname = "";
            var dt = dba.ViewPdfFile(type, EventPkey, cEvent.strFileGUID, UsageType);
            if (dt.Rows.Count > 0)
            {
                strDisplayname = dt.Rows[0]["FileName"].ToString();
            }
            else
            {

                var dtx = dba.ViewPDFFileElsePart(UsageType);

                if (dtx.Rows.Count > 0)
                {
                    strDisplayname = dtx.Rows[0]["FileName"].ToString();
                }
                else
                {
                    return "";
                }
            }
            string s = "/frmPopupFile.aspx?EPK=" + clsUtility.TYPE_Event.ToString() + "&PK=" + EventPkey.ToString();
            s = s + "&DN=" + HttpUtility.UrlEncode(strDisplayname);
            s = s + "&N=" + HttpUtility.UrlEncode(strDisplayname);

            return s;
        }
        public string ViewPartnerTypePdf(string str, int intParticipationType)
        {
            string UsageType = "";
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int AccountPkey = data.Id;
            int EventPkey = data.EventId;

            if (intParticipationType == 1)
            {
                UsageType = cSettings.UsageType_ExhibitorPartnerGuideline.ToString();
            }
            else if (intParticipationType == 2)
            {
                UsageType = cSettings.UsageType_OtherPartnerGuideLine.ToString();
            }
            else if (intParticipationType == 3)
            {
                UsageType = cSettings.UsageType_MediaPartnerGuideline.ToString();
            }
            else if (intParticipationType == 4)
            {
                UsageType = cSettings.UsageType_BrandingPartnerGuideline.ToString();

            }
            else if (intParticipationType == 5)
            {
                UsageType = cSettings.UsageType_EventSponsorPartnerGuideline.ToString();

            }
            else
            {
                UsageType = cSettings.UsageType_NonExhibitorPartnerGuideline.ToString();
            }

            string strDisplayname = "";
            string msg = "";
            var dt = dba.ViewPartnerTypePDF(EventPkey, UsageType);
            if (dt.Rows.Count > 0)
            {
                strDisplayname = dt.Rows[0]["FileName"].ToString();
            }
            else
            {
                if (intParticipationType == 1)
                {
                    msg = "Exhibitor not available.";
                }
                else if (intParticipationType == 2)
                {
                    msg = "File not available.";
                }
                else if (intParticipationType == 3)
                {
                    msg = "Media not available.";
                }
                else if (intParticipationType == 4)
                {
                    msg = "Branding not available.";
                }
                else if (intParticipationType == 5)
                {
                    msg = " Sponsor Handbook not available.";
                }
                else if (intParticipationType == 0)
                {
                    msg = "Non-exhibitor not available.";
                }
                return ""; // (new { result = "ERROR", msg }, JsonRequestBehavior.AllowGet);

            }

            string s = "/frmPopupFile.aspx?EPK=" + clsUtility.TYPE_Event.ToString() + "&PK=" + EventPkey.ToString();
            s = s + "&DN=" + HttpUtility.UrlEncode(strDisplayname);
            s = s + "&N=" + HttpUtility.UrlEncode(strDisplayname);

            return s;
            // return Json(new { result = "OK", elink = s }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PartnerExhibitors(int intParticipationType)
        {
            string link = "";
            clsLastUsed cLast = (clsLastUsed)Session["cLastUsed"];
            if (intParticipationType == 1)
            {
                if (cLast.intEventType_PKey == clsEvent.EventType_CloudConference || cLast.intEventType_PKey == clsEvent.EventType_HybridConference)
                    link = "/Overview?M = " + clsSettings.TEXT_BeAPartner.ToString();
                else
                    link = "/Overview?M = " + clsSettings.Text_ExhibitorPInformation.ToString();
            }
            else if (intParticipationType == 2)
                link = "/Overview?M = " + clsSettings.Text_OtherPInformation.ToString();
            else if (intParticipationType == 3)
                link = "/Overview?M = " + clsSettings.Text_MediaPInformation.ToString();
            else if (intParticipationType == 4)
                link = "/Overview?M = " + clsSettings.Text_BrandingPInformation.ToString();
            else if (intParticipationType == 5)
                link = "/MyConsole";
            else
                link = "/Overview?M = " + clsSettings.Text_NonExhibitorPartnerInstruct.ToString();
            return Json(new { msg = "OK", elink = link }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AllowAttendee(int ESPK, string btntext)
        {

            int intEventSession_pkey = ESPK;
            int intStarted = (btntext == "Allow Attendee" ? 1 : 0);

            var res = dba.updateAllowAttendee(intStarted, intEventSession_pkey);
            if (res == false)
            {
                return Json(new { result = "ERROR", msg = "Failed To Process." }, JsonRequestBehavior.AllowGet);
            }
            if (intStarted == 0)
            {
                return Json(new { result = "OK", btnText = "Allow Attendee", msg = "Attendee can not attend question now." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = "OK", btnText = "Stop Attendee", msg = "Attendee can attend question now." }, JsonRequestBehavior.AllowGet);
            }

        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult QuesResponse(int ESPK)
        {
            DataTable tblQuestionResponse = null;
            try
            {
                tblQuestionResponse = dba.QuesResponse(ESPK);
            }
            catch
            {
            }
            return Json(new { msg = "OK", Source = JsonConvert.SerializeObject(tblQuestionResponse) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SpkGetStarted()
        {
            string elink = "/frmOverview.aspx?M=" + clsSettings.TEXT_SpeakerConferenceCountText.ToString();
            return Json(new { elink }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewResponseGraph(int intQues_pkey, int intEventSession_pkey)
        {

            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int AccountPkey = data.Id;
            int EventPkey = data.EventId;

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EventPkey;
            cEvent.sqlConn = new SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();

            DataTable dt = cEvent.dtActivityQuestionResponseGraph(intQues_pkey, intEventSession_pkey);
            List<string> listX = new List<string>();
            List<string> listY = new List<string>();

            if (dt.Rows.Count > 0)
            {
                var stringArr = dt.Rows[0].ItemArray.Select(x => x.ToString()).ToArray();
                foreach (DataRow dr in dt.Rows)
                {
                    listX.Add(dr["Option"].ToString());
                    listY.Add(dr["RespPerc"].ToString());
                }
            }
            String[] Xaxis = listX.ToArray();
            String[] Yaxis = listY.ToArray();

            return Json(new { msg = "OK", Xaxis, Yaxis }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult CheckSave(double Amount, string ckPaymentType, string paycheckName, string PayCheckNum, DateTime dpPayCheckExpect, double PayAmount)
        {
            PaymentResult result = new PaymentResult();
            result.ErrorMsg = "Error Occurred While Saving Check";
            try
            {
                if (Amount != Math.Abs(PayAmount))
                {
                    result.ErrorMsg = "Partial/Over payment is not allowed.";
                    return Json(new { result }, JsonRequestBehavior.AllowGet);
                }
                SqlConnection conn = new SqlConnection(ReadConnectionString());
                int Method = clsPayment.METHOD_Check;
                string strCharges = "", strChargesPkey = "";
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.intEvent_pKey = data.EventId;
                cEventAccount.sqlConn = conn;
                cEventAccount.intAccount_pKey = data.Id;
                cEventAccount.LoadEventInfo(true, PartnerPanel: true);

                DataTable dtCharges = dba.PaymentType(data.Id, data.EventId);

                if (Math.Abs(cEventAccount.dblAccountBalance) == Math.Abs(Amount))
                    strCharges += string.Join(", ", dtCharges.Rows.OfType<DataRow>().Select(r => r["ChargeType_pKey"].ToString()));
                else
                    strCharges = ckPaymentType;

                if (strCharges != null)
                {
                    string[] arrCharges = strCharges.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i <= arrCharges.Length - 1; i++)
                    {
                        if (arrCharges[i] == "1")
                            arrCharges[i] = clsPayment.GetOnlyRegistrationCharge();
                    }
                    strCharges = String.Join(",", arrCharges);
                    double calculatedAmount = 0;
                    DataTable dt = clsPayment.FillAccountChargeTypePkey(strCharges, data.Id, data.EventId);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        calculatedAmount = Convert.ToDouble(dt.Compute("Sum(Amount)", ""));
                        if (Math.Abs(calculatedAmount) == Amount)
                            strChargesPkey = string.Join(",", dt.AsEnumerable().Select(i => i["pKey"]).ToArray());
                    }
                    PaymentModel payment = new PaymentModel();
                    payment.intPayMethod = Method;
                    payment.strCharges = strCharges;
                    payment.strChargesPkey = strChargesPkey;
                    payment.paycheckName = paycheckName;
                    payment.PayCheckNum = PayCheckNum;
                    payment.dpPayCheckExpect = dpPayCheckExpect;
                    payment.CheckAmount = Amount;
                    result = ProcessPayment(conn, data, payment);
                }
            }
            catch (Exception ex)
            {
                result.ErrorMsg = ex.Message;
            }
            return Json(new { result }, JsonRequestBehavior.AllowGet);
        }
        [CustomizedAuthorize]
        private PaymentResult ProcessPayment(SqlConnection sqlConn, User_Login data, PaymentModel payment, string strComment = "")
        {
            SqlConnection conn = new SqlConnection(ReadConnectionString());
            PaymentResult Result = new PaymentResult();
            int AccountPkey = Convert.ToInt32(User.Identity.Name);
            clsPayment cp = new clsPayment();
            cp.sqlConn = sqlConn;
            cp.intPaymentMethod_pKey = payment.intPayMethod;
            cp.intPayerAcctPKey = AccountPkey;
            cp.intEventPKey = data.EventId;
            cp.strIntendedAccounts = AccountPkey.ToString();
            cp.intLoggedByAccount_pKey = AccountPkey;
            string strChargesPkey = payment.strChargesPkey;
            cp.strSelectedCharges = ((string.IsNullOrEmpty(payment.strCharges)) ? clsPayment.getPendingCharges(AccountPkey, data.EventId, ref strChargesPkey) : payment.strCharges);
            payment.strChargesPkey = strChargesPkey;
            cp.strSelectedChargesPkey = payment.strChargesPkey;

            switch (payment.intPayMethod)
            {
                case clsPayment.METHOD_Check:
                    cp.strCheckName = payment.paycheckName;
                    cp.strCheckNum = payment.PayCheckNum;
                    if (payment.dpPayCheckExpect != null)
                        cp.dtCheckExpected = payment.dpPayCheckExpect;
                    else
                        cp.dtCheckExpected = DateTime.MinValue;

                    cp.dblAmount = clsUtility.CleanCurrency(payment.CheckAmount.ToString());
                    //Me.txtPayCheckName.BackColor = IIf(.strCheckName = "", Color.LightCoral, Color.White)
                    break;
                case clsPayment.METHOD_Wire:
                    cp.dblAmount = clsUtility.CleanCurrency(payment.CheckAmount.ToString());
                    cp.strWireBank = payment.WireBank;
                    cp.strWireAccount = payment.WireAccount;
                    if (payment.dpWireDate != null)
                        cp.dtWireDate = payment.dpWireDate;
                    else
                        cp.dtWireDate = DateTime.MinValue;
                    //Me.txtWireBank.BackColor = IIf(.strWireBank = "", Color.LightCoral, Color.White)
                    //Me.txtWireAccount.BackColor = IIf(.strWireAccount = "", Color.LightCoral, Color.White)
                    break;
            }
            Result.bPaymentResult = cp.PostPayment();
            Result.intReceiptNumber = cp.intReceiptNumber;
            Result.strPaymentProblem = String.Join(", ", cp.lstErrors.ToArray());
            string ErrorMessage = "";

            if (!Result.bPaymentResult)
            {
                ErrorMessage = "Payment was unsuccessful due to: " + Result.strPaymentProblem.TrimEnd('.') + ". Correct and try again. If problems persist, contact MAGI.";
                goto Finish;
            }
            cp.strMemo = "Payment made";
            cp.strComment = "";
            clsSurrogate c1 = ((clsSurrogate)Session["Surrogate"]);
            if (c1 != null)
            {
                if (payment.intPayMethod == 2 && !c1.bPrevAdmin)
                    cp.bPaid = false;
                else
                    cp.bPaid = true;
            }
            else if (payment.intPayMethod == 2 && !data.GlobalAdmin)
                cp.bPaid = false;
            else
                cp.bPaid = true;

            if (!cp.LogPayment())
            {
                ErrorMessage = "Payment was successful but there was an error logging the payment record. Contact MAGI for assistance.";
                goto Finish;
            }
            if (cp.bPaid)
            {
                if (!cp.ApplyCashToAccounts(cp.dblAmount, cp.dblAmount))
                {
                    ErrorMessage = "Payment was successful but there was an error logging the payment record. Contact MAGI for assistance.";
                    goto Finish;
                }
            }
            clsAccount cAccount = new clsAccount();
            cAccount.sqlConn = conn;
            cAccount.lblMsg = null;
            cAccount.intAccount_PKey = data.Id;
            cAccount.LoadAccount();
            cAccount.LogAuditMessage("Log payment on behalf of account: " + data.Id.ToString() + " for event: " + data.EventId, clsAudit.LOG_Payment);

            Result.Redirect = true;
            ErrorMessage = "OK";
        Finish:
            Result.Redirect = true;
            Result.ErrorMsg = ErrorMessage;
            return Result;
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult WireSave(double Amount, string ckPaymentType, string WireBank, string WireAccount, DateTime dpWireDate, string Comment, double PayAmount)
        {
            PaymentResult result = new PaymentResult();
            result.ErrorMsg = "Error Occurred While Saving Wire";
            try
            {
                if (Amount != Math.Abs(PayAmount))
                {
                    result.ErrorMsg = "Partial/Over payment is not allowed.";
                    return Json(new { result }, JsonRequestBehavior.AllowGet);
                }

                SqlConnection conn = new SqlConnection(ReadConnectionString());
                int Method = clsPayment.METHOD_Wire;
                string strCharges = "", strChargesPkey = "";
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.intEvent_pKey = data.EventId;
                cEventAccount.sqlConn = conn;
                cEventAccount.intAccount_pKey = data.Id;
                cEventAccount.LoadEventInfo(true, PartnerPanel: true);

                DataTable dtCharges = dba.PaymentType(data.Id, data.EventId);

                if (Math.Abs(cEventAccount.dblAccountBalance) == Math.Abs(Amount))
                    strCharges += string.Join(", ", dtCharges.Rows.OfType<DataRow>().Select(r => r["ChargeType_pKey"].ToString()));
                else
                    strCharges = ckPaymentType;

                if (strCharges != null)
                {
                    string[] arrCharges = strCharges.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i <= arrCharges.Length - 1; i++)
                    {
                        if (arrCharges[i] == "1")
                            arrCharges[i] = clsPayment.GetOnlyRegistrationCharge();
                    }
                    strCharges = String.Join(",", arrCharges);
                    double calculatedAmount = 0;
                    DataTable dt = clsPayment.FillAccountChargeTypePkey(strCharges, data.Id, data.EventId);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        calculatedAmount = Convert.ToDouble(dt.Compute("Sum(Amount)", ""));
                        if (Math.Abs(calculatedAmount) == Amount)
                            strChargesPkey = string.Join(",", dt.AsEnumerable().Select(i => i["pKey"]).ToArray());
                    }


                    PaymentModel payment = new PaymentModel();
                    payment.intPayMethod = Method;
                    payment.WireBank = WireBank;
                    payment.WireAccount = WireAccount;
                    payment.dpWireDate = dpWireDate;
                    payment.CheckAmount = Amount;
                    result = ProcessPayment(conn, data, payment, Comment);
                }
            }
            catch (Exception ex)
            {
                result.ErrorMsg = ex.Message;
            }
            return Json(new { result }, JsonRequestBehavior.AllowGet);
        }
        private string CreateVoucher(double dblAmount, string strSubject, string strComment, int intAccountChargePkey, int intVoucher_pKey)
        {
            SqlConnection conn = new SqlConnection(ReadConnectionString());
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            if (dblAmount > 0)
            {
                clsVoucher cOrig = new clsVoucher();
                cOrig.sqlConn = conn;
                cOrig.intVoucher_pKey = intVoucher_pKey;
                cOrig.LoadVoucher();
                clsVoucher c = new clsVoucher();
                c.sqlConn = conn;
                c.intVoucher_pKey = 0;
                c.LoadVoucher();
                if (cOrig.intVoucher_pKey > 0)
                {
                    c.intAccount_pkey = cOrig.intAccount_pkey;
                    c.strIssuedTo = cOrig.strIssuedTo;
                    c.intIssuedForEvent_pKey = cOrig.intIssuedForEvent_pKey;
                    c.strIssuedForEvent = cOrig.strIssuedForEvent;
                    c.dtIssuedOn = cOrig.dtIssuedOn;
                    c.dtExpirationDate = cOrig.dtExpirationDate;
                    c.intPaymentMethod = cOrig.intPaymentMethod;
                    c.intReferenceReceipt = cOrig.intReferenceReceipt;
                    c.strPaymentTransAction = cOrig.strPaymentTransAction;
                    c.strCardLastFour = cOrig.strCardLastFour;
                    c.intCancellationReason_pKey = cOrig.intCancellationReason_pKey;
                    c.strVoucherEmail = cOrig.strVoucherEmail;
                }
                else
                {
                    c.intAccount_pkey = data.Id;
                    c.strIssuedTo = data.LastName + ", " + data.FirstName;
                    c.intIssuedForEvent_pKey = data.EventId;
                    c.strIssuedForEvent = data.EventName;
                    c.dtIssuedOn = DateTime.Now;
                    c.dtExpirationDate = c.dtIssuedOn.AddMonths(cSettings.intRegVoucherExpirationMonth);
                    c.strVoucherEmail = data.Email;
                }
                c.dblAmount = dblAmount;
                c.strCancellationComment = strComment;
                c.strComments = strComment;
                if (!c.SaveVoucher())
                    return "Error while saving voucher";

                if (c.intVoucher_pKey > 0)
                    clsPayment.UpdateCancellationComments(c.intAccount_pkey, data.EventId, c.intVoucher_pKey, intAccountChargePkey);

                if (c.SendVoucherEmail(clsAnnouncement.Voucher_Cancellation, strSubject))
                    return "The voucher has been created and a confirmation email has been sent.";
            }
            return "Error while saving voucher";
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult cmdVoucherConfirm(string VoucherNo, double hdnRemainingAmount, int hdnVoucherPkey)
        {
            clsPayment cP = new clsPayment();
            string ErrorMsg = "";
            try
            {
                SqlConnection conn = new SqlConnection(ReadConnectionString());
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                int AccountID = Convert.ToInt32(User.Identity.Name), EventID = 0;
                string hdnVoucherCode = "", strMemo = "";
                EventID = data.EventId;

                if (VoucherNo == "" || VoucherNo == "0")
                    ErrorMsg = "The entered voucher code is not valid.";

                clsVoucher c = new clsVoucher();
                c.sqlConn = conn;
                c.intVoucher_pKey = Convert.ToInt32(VoucherNo.Replace("V", ""));
                c.LoadVoucher();

                cP.sqlConn = conn;
                cP.intPaymentMethod_pKey = clsPayment.METHOD_Other;
                cP.intPayerAcctPKey = AccountID;
                cP.intLoggedByAccount_pKey = AccountID;
                cP.intEventPKey = EventID;
                cP.strIntendedAccounts = AccountID.ToString();
                string strChargesPkey = "";
                cP.strSelectedCharges = clsPayment.getPendingCharges(AccountID, EventID, ref strChargesPkey);
                cP.intReceiptNumber = cP.getReceipt();
                int intVoucher_pKey = c.intVoucher_pKey;
                strMemo = "applied voucher #V" + intVoucher_pKey.ToString("D5");
                cP.bPaid = true;
                cP.strOtherReference = "applied voucher #V" + intVoucher_pKey.ToString("D5");

                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.intAccount_pKey = data.Id;
                cEventAccount.intEvent_pKey = data.EventId;
                cEventAccount.sqlConn = conn;
                cEventAccount.LoadEventInfo(true);
                double dblVoucherAmount = Math.Abs(cEventAccount.dblAccountBalance);

                if (string.IsNullOrEmpty(ErrorMsg))
                    ErrorMsg = "OK";

                if (!cP.LogVoucherPayment(dblVoucherAmount, clsPayment.METHOD_Other, intVoucher_pKey, strMemo, 0))
                    goto Finish;

                cP.strMemo = "Used Voucher: #V" + intVoucher_pKey.ToString("D5");

                if (!cP.ApplyCashToAccounts(Math.Abs(dblVoucherAmount), 0, ""))
                    goto Finish;

                if (hdnRemainingAmount > 0)
                {
                    int intreceiptnumber = clsReceipt.GetAccountReceipt(data.Id, Math.Abs(cEventAccount.dblAccountBalance), data.EventId);
                    if (intreceiptnumber > 0)
                        cP.MarkPaymentAsVoid(intreceiptnumber);
                }
                ErrorMsg = CreateVoucher(hdnRemainingAmount, "MAGI Split Balance Voucher", "Balance from " + hdnVoucherCode + "", 0, intVoucher_pKey);

            Finish:
                return Json(new { msg = ErrorMsg, Memo = strMemo }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { msg = ErrorMsg }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region MyOption
        [CustomizedAuthorize]
        public ActionResult _MyOptions()
        {
            return PartialView();
        }
        public ActionResult MyOptions()
        {
            MyOptions myoptions = new MyOptions();
            ViewBag.ZoomLoginPopUp = false;
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
            {
                clsLastUsed cLast = (clsLastUsed)Session["cLastUsed"];
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                SqlConnection conn = new SqlConnection(ReadConnectionString());

                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                int AccountPkey = data.Id, EventPkey = data.EventId;

                clsAccount cAccount = new clsAccount();
                cAccount.sqlConn = conn;
                cAccount.intAccount_PKey = AccountPkey;
                cAccount.LoadAccount();

                if (cAccount.intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_SlidesOnly || cAccount.intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_ExhibitOnly)
                    return RedirectToAction("Index", "Home");


                clsEvent cEvent = new clsEvent();
                cEvent.intEvent_PKey = EventPkey;
                cEvent.sqlConn = conn;
                cEvent.LoadEvent();

                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.intEvent_pKey = EventPkey;
                cEventAccount.sqlConn = conn;
                cEventAccount.intAccount_pKey = data.Id;


                if (!cEventAccount.LoadEventInfo(true))
                    return Redirect("/Registration");
                else if (cEventAccount.intParticipationStatus_pKey == clsEventAccount.PARTICIPATION_Cancelled)
                    return Redirect("/Registration");

                bool bThirdParty = (cLast.intFeedbackAcct > 0);

                string intRegistrationLevel_pKey = "1";
                int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, EventPkey, ref intRegistrationLevel_pKey);
                string strImagePath = "~/accountimages/" + AccountPkey.ToString() + "_img.jpg";
                string strPhysicalPath = Server.MapPath(strImagePath);
                bool bExists = clsUtility.FileExists(strPhysicalPath);

                ViewBag.ID = AccountPkey;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.EventPKey = EventPkey; // data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);
                ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
                ViewBag.bLicenseUpdated = ((!string.IsNullOrEmpty(cAccount.strLicenseNumber)) ? true : false);
                ViewBag.leftPanel_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
                ViewBag.VirtualDropdown_Visible = ViewBag.leftPanel_Visible;
                ViewBag.lblTitle = cEvent.strEventFullname + ": " + (bThirdParty ? "Options for " + cAccount.strFirstname + " " + cAccount.strLastname : "My Event Options");
                ViewBag.lblInstruct = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.TEXT_MyOptions), cEvent, null, cAccount, cEventAccount);
                ViewBag.lblAcctName = cAccount.strFirstname + " " + cAccount.strLastname;
                ViewBag.lblAcctOrg = cAccount.strOrganizationID;
                ViewBag.lblAcctDept = cAccount.strDepartment;
                ViewBag.lblAcctPhone = cAccount.strPhone;
                ViewBag.lblAcctEmail = cAccount.strEmail;
                ViewBag.lblAcctTitle = cAccount.strTitle;
                ViewBag.phOverride = bThirdParty;
                ViewBag.ImageExist = bExists;
                ViewBag.ImgURL = strImagePath.Replace("~", "");
                ViewBag.lblRegType = cEventAccount.strRegistrationLevelID;
                ViewBag.lnkRegType = "(" + cEventAccount.strOneDayName + ")";
                ViewBag.ckRemindMeChecked = cEventAccount.bWantReminder;



                DateTime dtEventStart = clsUtility.getStartOfDay(cEvent.dtStartDate);
                DateTime EventStartdate = new DateTime(dtEventStart.Year, dtEventStart.Month, dtEventStart.Day);
                DateTime CurrDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                if (dtEventStart < Convert.ToDateTime("2018-05-21"))
                    cEventAccount.LoadEducationOptions();

                ViewBag.dtEventStart = clsUtility.getStartOfDay(cEvent.dtStartDate).ToString("MM/dd/y");
                ViewBag.dtEventEnd = clsUtility.getEndOfDay(cEvent.dtEndDate).ToString("MM/dd/y");

                ViewBag.strSpecial_Request = cEventAccount.strSpecial_Request;
                ViewBag.lnkRegTypeVisible = (cAccount.intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDay || cAccount.intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayPhysical || cAccount.intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayVirtual);
                ViewBag.lnkRegTypeEnabled = CurrDate < EventStartdate;
                ViewBag.lblNetworkingContacts = cEventAccount.intNetworkingLimitOverride == -1 ? cSettings.intNetLimit.ToString() : cEventAccount.intNetworkingLimitOverride.ToString();
                ViewBag.lblOpenMessageLimit = cEventAccount.intOpenMessageLmt == 0 ? cSettings.intOpenMessageLimit.ToString() : cEventAccount.intOpenMessageLmt.ToString();
                bool lblInterest = false, cmdChrPrfrnc = false;
                lblInterest = (clsEventAccount.getSpeakerStatus(AccountPkey, cEvent.intEvent_PKey) == true);
                cmdChrPrfrnc = lblInterest;
                ViewBag.lblInterest = lblInterest;
                ViewBag.cmdChrPrfrnc = cmdChrPrfrnc;
                ViewBag.lblRegMAGI = (cAccount.bIsMember ? "Yes" : "No");
                ViewBag.cmdJoinVisible = !cAccount.bIsMember;
                ViewBag.strReferralName = cEventAccount.strReferralName;
                ViewBag.strReferralEmail = cEventAccount.strReferralEmail;
                ViewBag.strReferralPhone = cEventAccount.strReferralPhone;
                ViewBag.cbReferType = cEventAccount.intReferralType_pKey;
                ViewBag.ScOpenModel =  (Request.QueryString["SC"] != null && Request.QueryString["SC"] == "1");

                bool RedeemVoucherVisible = false;
                double dblbalance = Math.Round(cEventAccount.dblAccountBalance, 2);
                if (dblbalance < 0)
                {
                    ViewBag.lblRegBal = "Balance Due " + String.Format("{0:c}", cEventAccount.dblAccountBalance);
                    ViewBag.lblRegBalColor = "Red";
                    RedeemVoucherVisible = true;
                }
                else if (dblbalance == 0)
                {
                    ViewBag.lblRegBal = "Current";
                    ViewBag.lblRegBalColor = "Green";
                }
                else
                {
                    ViewBag.lblRegBal = "Credit (" + String.Format("{0:c}", cEventAccount.dblAccountBalance) + ")";
                    ViewBag.lblRegBalColor = "Green";
                }
                ViewBag.RedeemVoucherVisible = RedeemVoucherVisible;
                ViewBag.btnPayNowVisible = (Math.Round(cEventAccount.dblAccountBalance, 2) < 0);
                var dtsession = dba.GetChairingSessionInfo(EventPkey, AccountPkey); //30817
                ViewBag.dlSessionInfo = (dtsession.Rows.Count > 0 ? true : false);
                myoptions.tblSessionInfo = dtsession;

                FillInvoicesVouchers(AccountPkey, EventPkey, "", "Invoice");   // -- Fill Invoices
                                                                               //DataTable dtReceipt = clsReceipt.getAllReceipt(AccountPkey, EventPkey);
                                                                               //ViewBag.phInvoice = (dtReceipt.Rows.Count > 0);
                myoptions.tblReceipt = (DataTable)ViewBag.dtReceipt;

                double dblRefundBalance = clsEventAccount.getAccountBalance(data.Id, data.EventId);

                int cancelPopupField = 0;
                if (cEventAccount.bSpeakerAtEvent)
                    cancelPopupField = 1;
                else if (cEventAccount.intParticipationStatus_pKey == clsEventAccount.PARTICIPATION_Provisional)
                    cancelPopupField = 2;
                else if (cEventAccount.intParticipationStatus_pKey == clsEventAccount.PARTICIPATION_Attending && cEventAccount.dblAccountBalance == 0 && dblRefundBalance == 0)
                    cancelPopupField = 3;

                ViewBag.cancelPopupField = cancelPopupField;//  cancelPopupField;
                ViewBag.SpeakerAtEvent = cEventAccount.bSpeakerAtEvent;

                DateTime d = clsEvent.getCaliforniaTime();
                ViewBag.bConferenceEnd = (d > cEvent.dtEndDate.AddDays(1).Date);
                //******************    cupons   ******************
                if (!System.IO.Directory.Exists(Server.MapPath("~/CouponFiles/")))
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/CouponFiles/"));

                string[] filePaths = Directory.GetFiles(Server.MapPath("~/CouponFiles/"), cEvent.strFileGUID + "_*", SearchOption.TopDirectoryOnly);
                List<clsOneFile> colFiles = new List<clsOneFile>();
                foreach (string filePath in filePaths)
                {
                    clsOneFile c = new clsOneFile();
                    c.dtLastUpdate = System.IO.File.GetLastWriteTime(filePath);
                    c.strFilename = Path.GetFileName(filePath);
                    colFiles.Add(c);
                    c = null;
                }
                bool DockCouponsVisible = true, bCurrentAttendee = false, bShowCoupons = false;
                string lblCoupon = "";
                ViewBag.lblCouponsTitle= "Coupons";
                bCurrentAttendee = (cEventAccount.intParticipationStatus_pKey == clsEventAccount.PARTICIPATION_Attending);
                if (cAccount.bGlobalAdministrator && !((cEvent.bDnCoupons && (cEvent.bOptions && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "MyMAGIMenu_19"))) && (((cEvent.intAttDnCoupons == clsEvent.intAttAttending && !bCurrentAttendee) || ((cEvent.intAttDnCoupons == clsEvent.intAttPaid || cEvent.intAttDnCoupons == clsEvent.intAttAsIfPaid) && cEventAccount.dblAccountBalance <= -cSettings.intAttAccessBal)) ? false : true)))
                {
                    ViewBag.lblCouponsTitle = "Coupons (Admin Access Only)";
                }
                if (!(cEvent.bDnCoupons && (cEvent.bOptions && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "MyMAGIMenu_19"))) && !cAccount.bGlobalAdministrator)
                {
                    lblCoupon = "This feature is not yet available.";
                    if ((cLast.intEventType_PKey == clsEvent.EventType_CloudConference || cLast.intEventType_PKey == clsEvent.EventType_HybridConference) && !cAccount.bGlobalAdministrator)
                        DockCouponsVisible = false;
                }
                else if ((cEvent.intAttDnCoupons == clsEvent.intAttAttending && !bCurrentAttendee) && !cAccount.bGlobalAdministrator)
                {
                    lblCoupon = "You are not Attendee of this event.";
                    if ((cLast.intEventType_PKey == clsEvent.EventType_CloudConference || cLast.intEventType_PKey == clsEvent.EventType_HybridConference) && !cAccount.bGlobalAdministrator)
                        DockCouponsVisible = false;
                }
                else
                {
                    bShowCoupons = colFiles.Count > 0;
                    lblCoupon = (bShowCoupons ? "Click the link to download a coupon (one per person only)." : "No coupons are available.");
                }
                DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                DateTime dtCalTime = clsEvent.getCaliforniaTime();
                ViewBag.VirtualDropdown_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
                string Host = HttpContext.Request.Path.ToUpper().Replace("/MYMAGI", "");
                int intRegistrationLevelpKey = 0;
                if (!string.IsNullOrEmpty(intRegistrationLevel_pKey))
                    intRegistrationLevelpKey = Convert.ToInt32(intRegistrationLevel_pKey);
                myoptions.ddEventVirtualData = repository.GetVirtualEventDropDownInfo(Host, data.Id, data.EventId, dtCurrentTime, dtCalTime, data.ParentOrganization_pKey, intAttendeeStatus, intRegistrationLevelpKey, data.GlobalAdmin, data.StaffMember);
                myoptions.HelpIconInfo = repository.PageLoadResourceData(data, "", "15");
                ViewBag.SelectedDropDown = GetSelectedVirtualDropDown(Host);

                ViewBag.lblCoupon = lblCoupon;
                ViewBag.CouponList = colFiles;
                ViewBag.DockCouponsVisible = DockCouponsVisible;
                ViewBag.bCurrentAttendee = bCurrentAttendee;
                //******************    OtherLinks   ******************

                string RoleKey = "2"; // '--attendee
                if (cEventAccount.bSpeakerAtEvent)
                    RoleKey = RoleKey + ",1";// '--speaker and attendee
                else
                {
                    if (cEventAccount.bChairAtEvent)
                        RoleKey = RoleKey + ",3"; //'--chairperson and attendee             
                }
                bool phOtherLinkVisible = true;
                var dtOtherlink = dba.GetOtherLinks(RoleKey, AccountPkey);
                myoptions.tblOtherLinks = dtOtherlink;

                ViewBag.phOtherLinkVisible = phOtherLinkVisible;

                //******************************************** CRCP ******************************************** 

                var dtEdOption = dba.GetCRCPOption(cEventAccount.bSpeakerAtEvent, EventPkey);
                myoptions.tblEdOptions = dtEdOption;

                //*************************  Offers ******************************************** 

                var dtOffers = dba.RefreshOffers(EventPkey);
                myoptions.tblOffers = dtOffers;
                ViewBag.lblOffer = (dtOffers.Rows.Count > 0 ? "Special Offers:" : "No offers are available.<br/>Check again later.");

                // ***************  Attending  ***************
                if ((cAccount.intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDay || cAccount.intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayPhysical || cAccount.intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayVirtual) && cAccount.dtOneDayReg > DateTime.MinValue)
                {
                    ViewBag.dpAttStart = cAccount.dtOneDayReg.ToString("MM/dd/y");
                    ViewBag.dpAttEnd = cAccount.dtOneDayReg.ToString("MM/dd/y");
                }
                else
                {
                    ViewBag.dpAttStart = clsUtility.getStartOfDay(cEvent.dtStartDate).ToString("MM/dd/y");
                    ViewBag.dpAttEnd = clsUtility.getEndOfDay(cEvent.dtEndDate).ToString("MM/dd/y");

                }
                // ***************  Travel and Lodging  ***************

                if (cEventAccount.dtExpectedArrival > new DateTime(2000, 1, 1))
                    ViewBag.dpAttStart = clsUtility.getStartOfDay(cEventAccount.dtExpectedArrival).ToString("MM/dd/y"); //cEventAccount.dtExpectedArrival;

                if (cEventAccount.dtExpectedDeparture > new DateTime(2000, 1, 1))
                    ViewBag.dpAttEnd = clsUtility.getStartOfDay(cEventAccount.dtExpectedDeparture).ToString("MM/dd/y");  //cEventAccount.dtExpectedDeparture;

                var ddtravel = dba.FillMyOptionsDropDowns(5);
                ViewBag.ddtravel = ddtravel;
                ViewBag.ddTravelSelectedValue = cEventAccount.intTravelStatus_pKey.ToString();
                ViewBag.ddLodgingSelectedValue = cEventAccount.intLodgingStatus_pKey.ToString();
                var ddLodging = dba.FillMyOptionsDropDowns(6);
                ViewBag.ddLodging = ddLodging;
                ViewBag.txtLodging = cEventAccount.strLodgingDetails;
                ViewBag.txtTravel = cEventAccount.strTravelDetails;
                ViewBag.intRegistrationLevel_pKey = cAccount.intRegistrationLevel_pKey;

                //************************  Badge ************************ 
                ViewBag.DockAtt = true;
                ViewBag.DockLunch = true;
                ViewBag.DockBadge = true;
                ViewBag.DockOffers = true;

                if (cLast.intEventType_PKey == clsEvent.EventType_CloudConference || cLast.intEventType_PKey == clsEvent.EventType_HybridConference)
                {
                    ViewBag.DockAtt = false;
                    ViewBag.DockLunch = false;
                    ViewBag.DockBadge = false;
                    ViewBag.DockOffers = (((cLast.intEventType_PKey == clsEvent.EventType_CloudConference || cLast.intEventType_PKey == clsEvent.EventType_HybridConference) && !cAccount.bGlobalAdministrator) ? false : true);
                }
                ViewBag.phBadgeUnLock1 = !cEventAccount.bBadgeLocked;
                ViewBag.phBadgeUnLock2 = !cEventAccount.bBadgeLocked;
                ViewBag.phBadgeLock1 = cEventAccount.bBadgeLocked;

                var dtRefreshBadge = dba.RefreshBadge(AccountPkey);
                string txtBName = "NA";
                string txtBTitle = "NA";
                string txtBOrg = "NA";
                if (dtRefreshBadge.Rows.Count > 0)
                {
                    txtBName = dtRefreshBadge.Rows[0]["BName"].ToString();
                    txtBTitle = dtRefreshBadge.Rows[0]["BTitle"].ToString();
                    txtBOrg = dtRefreshBadge.Rows[0]["BOrganizationID"].ToString();
                }
                ViewBag.txtBName = txtBName;
                ViewBag.txtBTitle = txtBTitle;
                ViewBag.txtBOrg = txtBOrg;
                ViewBag.dblAccountAmount = cEventAccount.dblAccountBalance;
                clsBadgeDesign clsBridge = new clsBadgeDesign();
                clsBridge.sqlConn = conn;
                clsBridge.intBadgeDesign_PKey = cEvent.intAttendeeBadgeDesign_pKey;
                clsBridge.LoadBadgeDesign();

                int intPosn = ViewBag.txtBName.IndexOf(" ");
                if (intPosn > 0)
                {
                    int Full = ViewBag.txtBName.Length;
                    int start = ViewBag.txtBName.Substring(0, intPosn).Length;
                    int subCount = Full - start;
                    ViewBag.htmlBName = "<b>" + ViewBag.txtBName.Substring(0, intPosn) + "</b>" + ViewBag.txtBName.Substring(start, subCount);
                }

                else
                    ViewBag.htmlBName = "<b>" + ViewBag.txtBName + "</b>";

                ViewBag.bNameClass = "Font11" + ((clsBridge.bNameReqd) ? " txtRequired" : "");
                ViewBag.bTitleClass = "Font11" + ((clsBridge.bTitleReqd) ? " txtRequired" : "");
                ViewBag.bOrgClass = "Font11" + ((clsBridge.bOrganizationReqd) ? " txtRequired" : "");
                ViewBag.TitleMax = clsBridge.intMaxLenTitle;
                ViewBag.OrganizationMax = clsBridge.intMaxLenOrg;

                ViewBag.lblBNameLen = "(" + clsBridge.intMaxLenName.ToString() + " chars)";
                ViewBag.lblBTitleLen = "(" + clsBridge.intMaxLenTitle.ToString() + " chars)";
                ViewBag.lblBOrgLen = "(" + clsBridge.intMaxLenOrg.ToString() + " chars)";

                ViewBag.BNameLen =  clsBridge.intMaxLenName;
                ViewBag.BTitleLen =  clsBridge.intMaxLenTitle;
                ViewBag.BOrgLen = clsBridge.intMaxLenOrg;


                ViewBag.BNameFore = ((ViewBag.txtBName.Length) > clsBridge.intMaxLenName) ? "red" : "black";
                ViewBag.BTitleFore = ((ViewBag.txtBTitle.Length) > clsBridge.intMaxLenName) ? "red" : "black";
                ViewBag.BOrgFore = ((ViewBag.txtBOrg.Length) > clsBridge.intMaxLenName) ? "red" : "black";
                ViewBag.imgLogoHeight = clsBridge.Badge_Logo_FrontHeight;
                ViewBag.imgLogo = clsBridge.Badge_FrontLogo.Replace("~", "");
                ViewBag.dblAccountAmount = cEventAccount.dblAccountBalance;

                // ************************ LUNCH * ***********************
                ViewBag.PaymentMethods = new SqlOperation().getPaymentMethodList();
                ViewBag.METHOD_Voucher = clsPayment.METHOD_Voucher;
                ViewBag.METHOD_Credit = clsPayment.METHOD_Credit;
                ViewBag.METHOD_Check = clsPayment.METHOD_Check;
                ViewBag.METHOD_Wire = clsPayment.METHOD_Wire;
                ViewBag.lnkTransferVoucher = false;
                ViewBag.lnkRefundVoucher = false;
                ViewBag.ddRefundVoucherEnable = false;

                if (dtEventStart < Convert.ToDateTime("2018-05-21"))
                    cEventAccount.LoadEducationOptions();

                ViewBag.bConflictResolutionModes = cEventAccount.bConflictResolutionModes;
                string SelectedCerts = "";
                if (cEventAccount.bCRCPExam)
                    SelectedCerts = SelectedCerts + "," + clsPrice.CHARGE_CRCPExam.ToString();
                if (cEventAccount.bCMECert)
                    SelectedCerts = SelectedCerts + "," + clsPrice.CHARGE_CME.ToString();
                if (cEventAccount.bCMEMDCert)
                    SelectedCerts = SelectedCerts + "," + clsPrice.CHARGE_CMEMD.ToString();
                if (cEventAccount.bCNECert)
                    SelectedCerts = SelectedCerts + "," + clsPrice.CHARGE_CNE.ToString();
                if (cEventAccount.bCCBCert)
                    SelectedCerts = SelectedCerts + "," + clsPrice.CHARGE_CCB.ToString();
                if (cEventAccount.bCLECert)
                    SelectedCerts = SelectedCerts + "," + clsPrice.CHARGE_CLE.ToString();
                if (cEventAccount.bCLECompactCert)
                    SelectedCerts = SelectedCerts + "," + clsPrice.CHARGE_CLECompactStates.ToString();
                if (cEventAccount.bCLEPACert)
                    SelectedCerts = SelectedCerts + "," + clsPrice.CHARGE_CLEPA.ToString();
                ViewBag.Selectedcerts = SelectedCerts.Trim(',');
                ViewBag.intTobePaidAmount = Math.Abs(cEventAccount.dblAccountBalance);

                if (!(cEvent.bLunch && (cEvent.bOptions && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "MyMAGIMenu_19"))))
                    ViewBag.lblNotLunch = "This feature is not yet available.";
                else if (cEvent.intAttAttendees == clsEvent.intAttAttending && !bCurrentAttendee)
                    ViewBag.lblNotLunch = "You are not Attendee of this event.";

                ViewBag.phLunch = (data.GlobalAdmin || ((cEvent.bLunch && (cEvent.bOptions && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "MyMAGIMenu_19"))) && (((cEvent.intAttLunch == clsEvent.intAttAttending && !bCurrentAttendee) || ((cEvent.intAttLunch == clsEvent.intAttPaid || cEvent.intAttLunch == clsEvent.intAttAsIfPaid) && cEventAccount.dblAccountBalance <= -cSettings.intAttAccessBal)) ? false : true)));
                FillInvoicesVouchers(0, 0, cAccount.strEmail, "Voucher");  //Fill Voucher
                ViewBag.imgPopTextURL = "/Images/miscellaneous/terms.png";
                ViewBag.lblPopTextTitle = "Badge Editing Instructions";
                ViewBag.lblPopTextContent = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.TEXT_BadgeInstructions), cEvent, null, cAccount);
                bool bShowSurveyQuestion = false;
                DataTable EventFeatures = repository.getDyamicEventSettings(data.EventId, "ISNULL(ShowSurveyQuestion,'0') as ShowSurveyQuestion");
                if (EventFeatures != null && EventFeatures.Rows.Count > 0)
                    bShowSurveyQuestion = (EventFeatures.Rows[0]["ShowSurveyQuestion"] != System.DBNull.Value) ? Convert.ToBoolean(EventFeatures.Rows[0]["ShowSurveyQuestion"]) : false;

                int selectedEvent = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).intEventSelector;
                bool bSponsor = clsEventOrganization.CheckExhibitor(data.ParentOrganization_pKey, selectedEvent);
                ViewBag.lblRegText = "";
                ViewBag.OpenSurveyRadWindow=false;
                LoadRegistrationQuestions(data, intAttendeeStatus, bShowSurveyQuestion, bSponsor, intRegistrationLevel_pKey);
                return View(myoptions);
            }
            else
            {
                ViewBag.ZoomLoginPopUp = true;
                return View(myoptions);
            }
            return RedirectToAction("Index", "Home");
        }

        private void FillInvoicesVouchers(int Id, int EvtId, string email = "", string Action = "")
        {
            switch (Action)
            {
                case "Invoice":
                    DataTable dtReceipt = clsReceipt.getAllReceipt(Id, EvtId);
                    ViewBag.phInvoice = (dtReceipt.Rows.Count > 0);
                    ViewBag.dtReceipt = dtReceipt;
                    break;
                case "Voucher":
                    DataTable dt = clsVoucher.getVoucherByAccount(email);
                    ViewBag.VoucherDT = dt;
                    DataRow[] dataRows = dt.Select(" CardLastFour <>'' and PaymentTransAction <>'' and PaymentTransAction <>'' ");
                    int length = 0;
                    if (dataRows != null)
                    {
                        length = dataRows.Length;
                        if (length > 0)
                            ViewBag.RefundVouchers = dataRows.CopyToDataTable();
                        else
                            ViewBag.RefundVouchers = null;
                        ViewBag.VoucherLength = length;
                    }

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        ViewBag.lnkTransferVoucher = true;
                        ViewBag.lnkRefundVoucher = (length > 0);
                        ViewBag.ddRefundVoucherEnable = true;
                    }
                    break;
            }
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult GetLunchInformation()
        {
            try
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);


                DataTable dtlunch = dba.RefreshLunch(data.Id, data.EventId);
                if (dtlunch != null && dtlunch.Rows.Count > 0)
                {
                    dtlunch.Columns.Add("MealList", typeof(String));
                    foreach (DataRow dr in dtlunch.Rows)
                    {
                        DataTable dtMeal = new SqlOperation().GetMealDropdownsInfo(data.EventId, dr["Mealtype"].ToString(), dr["DefaultMeal_Pkey"].ToString());
                        if (dtMeal != null)
                            dr["MealList"] = JsonConvert.SerializeObject(dtMeal);
                    }

                    dtlunch.AcceptChanges();
                }
                var jsonResult = Json(new { msg = "OK", List = JsonConvert.SerializeObject(dtlunch) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch
            {

            }
            return Json(new { msg = "Error Occurred while loading lunch info" }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult UpdateAttendLunch(int intMealPKey, int intEvtSession_pKey, bool bAttend)
        {
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            SqlConnection conn = new SqlConnection(ReadConnectionString());
            DataTable dtMeal = new DataTable();
            dtMeal = clsEventSession.getSpecialMeal(intMealPKey);

            if (dtMeal != null && dtMeal.Rows.Count > 0)
            {
                bool IsSpecial = false;
                if (dtMeal.Rows[0]["IsSpecial"] != System.DBNull.Value)
                    IsSpecial = Convert.ToBoolean(dtMeal.Rows[0]["IsSpecial"].ToString());
                if (dtMeal.Rows[0]["AdditionalCharge"].ToString() != "0")
                {
                    return Json(new { msg = "rwLunchConfirm", mealKey = intMealPKey, esKey = intEvtSession_pKey }, JsonRequestBehavior.AllowGet);
                }
                else if (IsSpecial == true)
                {
                    return Json(new { msg = "rwSpecialMeal", mealKey = intMealPKey, esKey = intEvtSession_pKey, SpecialMealRequest = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            clsEventSession c = new clsEventSession();
            c.sqlConn = conn;
            c.intEventSession_PKey = intEvtSession_pKey;
            c.intEvent_PKey = data.EventId;
            if (!c.SetAttendLunch(data.Id, bAttend, intMealPKey, data.EventAccount_pkey))
            {
                return Json(new { msg = "Error Occurred While Setting Update" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult cmdLunchSave(int intMealPKey, int intEvtSession_pKey, string specialMealText = "")
        {
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            SqlConnection conn = new SqlConnection(ReadConnectionString());
            DataTable dtMeal = new DataTable();

            clsEventSession c = new clsEventSession();
            c.sqlConn = conn;
            c.intEventSession_PKey = intEvtSession_pKey;
            c.intEvent_PKey = data.EventId;
            if (!c.SetAttendLunch(data.Id, true, intMealPKey, data.EventAccount_pkey, specialMealText))
            {
                return Json(new { msg = "Error Occurred While Setting Update" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult ddRefundVoucherChange(string SelectedValue)
        {
            try
            {
                string lblCCRefundCardTran = "";
                string lblCCRefundAmt = "";
                string lblReceiptNumber = "";
                string hdnCCRefundCardNo = "";
                string hdnCCMainAmount = "";

                DataTable dt = new SqlOperation().GetRefundVoucherSelectedData(SelectedValue);
                if (dt != null && dt.Rows.Count > 0)
                {
                    lblCCRefundCardTran = ((dt.Rows[0]["PaymentTransAction"] != System.DBNull.Value) ? dt.Rows[0]["PaymentTransAction"].ToString() : "");
                    lblCCRefundAmt = "$" + ((dt.Rows[0]["Amount"] != System.DBNull.Value) ? dt.Rows[0]["Amount"].ToString() : "");
                    lblReceiptNumber = "R" + ((dt.Rows[0]["ReferenceReceipt"] != System.DBNull.Value) ? dt.Rows[0]["ReferenceReceipt"].ToString() : "");
                    hdnCCRefundCardNo = ((dt.Rows[0]["CardLastFour"] != System.DBNull.Value) ? dt.Rows[0]["CardLastFour"].ToString() : "");
                    hdnCCMainAmount = ((dt.Rows[0]["Amount"] != System.DBNull.Value) ? dt.Rows[0]["Amount"].ToString() : "");
                }

                return Json(new
                {
                    msg = "OK",
                    lblCCRefundCardTran = lblCCRefundCardTran,
                    lblCCRefundAmt = lblCCRefundAmt,
                    lblReceiptNumber = lblReceiptNumber,
                    hdnCCRefundCardNo = hdnCCRefundCardNo,
                    hdnCCMainAmount = hdnCCMainAmount
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
            }
            return Json(new { msg = "Error Occurred while getting refund voucher change" }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public ActionResult SaveReferralOption(string txtReferPhone, string txtReferName, string txtReferEmail)
        {
            string errorMsg = "";
            try
            {
                if (txtReferName == "")
                    errorMsg = "Enter valid name.";

                if (txtReferPhone != "" && !clsUtility.isDigitsOnly(txtReferPhone))
                    errorMsg = "Enter valid phone no.";

                if (txtReferEmail != "" && !clsEmail.IsValidEmail(txtReferEmail))
                    errorMsg = "Enter valid email.";

                if (string.IsNullOrEmpty(errorMsg))
                {
                    FormsIdentity identity = (FormsIdentity)User.Identity;
                    User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                    int AccountPkey = data.Id;
                    int EventPkey = data.EventId;

                    clsEventAccount cEventAccount = new clsEventAccount();
                    cEventAccount.intEvent_pKey = EventPkey;
                    cEventAccount.sqlConn = new SqlConnection(ReadConnectionString());
                    cEventAccount.intEventAccount_pKey = data.EventAccount_pkey;
                    cEventAccount.strReferralPhone = txtReferPhone;
                    cEventAccount.strReferralName = txtReferName;
                    cEventAccount.strReferralEmail = txtReferEmail;
                    errorMsg = (((cEventAccount.UpdateAttendeeReferral())) ? "OK" : "Referrer not saved");
                }
            }
            catch (Exception ex)
            {
                errorMsg = "Error occurred while saving Referral";
            }

            return Json(new { msg = errorMsg }, JsonRequestBehavior.AllowGet);
        }
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public ActionResult ArrangeSaveEvent(string sArrangeText)
        {
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            string text = (sArrangeText.Length > 0 ? sArrangeText : "NULL");
            bool saved = new SqlOperation().ArrangeSaveEvent(text, data.Id.ToString(), data.EventId.ToString());
            return Json(new { msg = (saved ? "Saved" : "Error While Saving") }, JsonRequestBehavior.AllowGet);
        }
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public ActionResult SaveTravel(int intRegistrationLevel_pKey, string dpAtStart, string dpAtEnd, int ddTravel, int ddLodging, string txtTravel, string txtLodging, bool bReset)
        {
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int AccountPkey = data.Id, EventPkey = data.EventId;
            DateTime dpAttStart = new DateTime(), dpAttEnd = new DateTime();

            clsEventAccount cEventAccount = new clsEventAccount();
            cEventAccount.intEvent_pKey = EventPkey;
            cEventAccount.sqlConn = new SqlConnection(ReadConnectionString());
            cEventAccount.intEventAccount_pKey = data.EventAccount_pkey;

            if (!bReset)
            {
                if (string.IsNullOrEmpty(dpAtStart))
                    return Json(new { msg = "Expected date should be equal or less than event date" }, JsonRequestBehavior.AllowGet);

                if (string.IsNullOrEmpty(dpAtEnd))
                    return Json(new { msg = "Expected date should be equal or greater than event date " }, JsonRequestBehavior.AllowGet);

                dpAttStart = Convert.ToDateTime(dpAtStart);
                dpAttEnd = Convert.ToDateTime(dpAtEnd);

                if ((intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDay || intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayPhysical || intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_OneDayVirtual) && (bReset = false))
                {
                    if (cEventAccount.dtOneDayDate < dpAttStart)
                    {
                        return Json(new { msg = "Expected date should be equal or less than event date" }, JsonRequestBehavior.AllowGet);
                    }
                    if (cEventAccount.dtOneDayDate > dpAttEnd)
                    {
                        return Json(new { msg = "Expected date should be equal or greater than event date " }, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            if (bReset)
            {
                cEventAccount.ResetAttendeeArrival();
                return Json(new { result = "OK", msg = "The plan has been reset to default." }, JsonRequestBehavior.AllowGet);
            }
            else
            {

                if (dpAttStart != null)
                {
                    cEventAccount.dtExpectedArrival = dpAttStart;
                }
                else
                {
                    cEventAccount.dtExpectedArrival = DateTime.MinValue;

                }

                if (dpAttEnd != null)
                {
                    cEventAccount.dtExpectedDeparture = dpAttEnd;
                }
                else
                {
                    cEventAccount.dtExpectedDeparture = DateTime.MinValue;
                }
                cEventAccount.intTravelStatus_pKey = ddTravel;
                cEventAccount.intLodgingStatus_pKey = ddLodging;
                cEventAccount.strTravelDetails = txtTravel;
                cEventAccount.strLodgingDetails = txtLodging;

                if (cEventAccount.UpdateAttendeeArrival())
                {
                    return Json(new { result = "OK", msg = "Attendance plan updated" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { result = "ERROR", msg = "Unable to save" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [ValidateInput(true)]
        [CustomizedAuthorize]
        [AjaxValidateAntiForgeryToken]
        public ActionResult TransferRegistration(string txtTREmail, string txtTRComment, bool bTrProceed)
        {
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int AccountPkey = data.Id;
            int EventPkey = data.EventId;

            if (txtTREmail != "")
            {
                string strTargetName = "";
                int intTargetOrgPKey = 0;
                bool bMagiMember = false;
                int intTargetAccountPKey = clsUtility.getExistingAcct(txtTREmail.Trim(), null, ref strTargetName, ref intTargetOrgPKey, ref bMagiMember);
                if (intTargetAccountPKey <= 0)
                {
                    string message = clsUtility.getErrorMessage(219);
                    return Json(new { result = "ERROR", msg = message }, JsonRequestBehavior.AllowGet);
                }
                if (AccountPkey == intTargetAccountPKey)
                {
                    return Json(new { result = "ERROR", msg = "Registration cannot be transferred to self account" }, JsonRequestBehavior.AllowGet);
                }
                DataTable dtOrgStatus = new DataTable();
                dtOrgStatus = clsEventAccount.getOrgCompareStatus(AccountPkey, intTargetAccountPKey);

                if (dtOrgStatus.Rows[0]["Result"].ToString() == "False")
                {
                    return Json(new { result = "ERROR", msg = "Target person must be from same organization" }, JsonRequestBehavior.AllowGet);
                }

                if (intTargetAccountPKey > 0)
                {
                    DataTable dtStatus = new DataTable();
                    dtStatus = clsEventAccount.getRegistrationStatus(EventPkey, intTargetAccountPKey);
                    if (dtStatus.Rows.Count > 0)
                    {
                        if (dtStatus.Rows[0]["IsSpeaker"].ToString() == "1")
                        {
                            return Json(new { result = "ERROR", msg = "Target account is already speaker in current event." }, JsonRequestBehavior.AllowGet);
                        }
                        if (dtStatus.Rows[0]["IsAlreadyRegistered"].ToString() == "1")
                        {
                            return Json(new { result = "ERROR", msg = "Target account is already registered in current event." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                if (bTrProceed)
                {

                    if (!clsPayment.TransferRegistration(AccountPkey, intTargetAccountPKey, EventPkey, AccountPkey, txtTRComment))
                    {
                        return Json(new { result = "ERROR", msg = "Unable to Tranfer." });
                    }
                    // Me.SendRegTransferEmail(intTargetAccountPKey)
                    return Json(new { result = "Success", msg = "Unable to Tranfer." });
                }

                var dtlnktransfer = dba.LinkTranfer(txtTREmail);
                if (dtlnktransfer.Rows.Count > 0)
                {
                    string lblConfirms = "Transfer your registration to " + dtlnktransfer.Rows[0]["FirstName"].ToString() + " " + dtlnktransfer.Rows[0]["LastName"].ToString() + " at " + dtlnktransfer.Rows[0]["email"].ToString();
                    //clsUtility.PopupRadWindow(ScriptManager.GetCurrent(Me.Page), Me.Page, Me.rdTranserConfirm)
                    return Json(new { result = "OK", lblConfirms }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { result = "error", msg = "" }, JsonRequestBehavior.AllowGet);
                    //clsUtility.InjectAlert(ScriptManager.GetCurrent(Me.Page), Me.Page, "", 219)                   
                }
            }
            else
            {
                return Json(new { result = "error", msg = "Enter the email address of a person who has a MAGI account." }, JsonRequestBehavior.AllowGet);
            }
        }

        [ValidateInput(true)]
        [CustomizedAuthorize]
        [AjaxValidateAntiForgeryToken]
        public JsonResult Ed_OptionChanged(EdOPtionsModel OptionsModel)
        {
            try
            {
                if (OptionsModel != null && OptionsModel.EDItems != null)
                {
                    FormsIdentity identity = (FormsIdentity)User.Identity;
                    User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                    clsLastUsed cLast = (clsLastUsed)Session["cLastUsed"];
                    clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                    SqlConnection conn = new SqlConnection(ReadConnectionString());
                    clsAccount cAccount = new clsAccount();
                    string token = OptionsModel.Existing;
                    bool LicenseUpdate = true;

                    bool bPaymentRequired = false;
                    string ErrorMessage = "", strMemo = "", qry = "";
                    double dblLateCharges = 0;
                    clsEventAccount cEventAccount = new clsEventAccount();
                    cEventAccount.intAccount_pKey = data.Id;
                    cEventAccount.intEvent_pKey = data.EventId;
                    cEventAccount.sqlConn = conn;
                    cEventAccount.LoadEventInfo(true);
                    SqlCommand cmd;
                    int index = 0;
                    if (OptionsModel.EDItems.Count > 0)
                        index = OptionsModel.EDItems.Count - 1;

                    DataTable dtCharges = (dba.PaymentType(data.Id, data.EventId));
                    DataTable dtRelatedCharges = dba.GetCRCPOption(OptionsModel.bSpeakerAtEvent, data.EventId);
                    DateTime d = clsEvent.getCaliforniaTime(), dtEndDate = new DateTime();
                    DataTable EventInfo = repository.getDyamicEventSettings(data.EventId, "EndDate");
                    if (EventInfo != null && EventInfo.Rows.Count > 0)
                        dtEndDate =(EventInfo.Rows[0]["EndDate"] == DBNull.Value) ? System.DateTime.Now : Convert.ToDateTime(EventInfo.Rows[0]["EndDate"]);
                    bool bConferenceEnd = false;
                    bConferenceEnd = (d > dtEndDate.AddDays(1).Date);
                    foreach (CKEdOptions ListItem in OptionsModel.EDItems)
                    {
                        int intChargeType_pkey = Convert.ToInt32(ListItem.Values);
                        bool ChargeTypeExist = false;
                        if (!string.IsNullOrEmpty(token))
                            ChargeTypeExist = token.Contains(intChargeType_pkey.ToString());
                        string strCert = ListItem.Text;
                        string cut_at = "(";
                        //Dim x As Integer = InStr(strCert, cut_at)
                        // strCert = strCert.Substring(0, x - 2)
                        if (ListItem.CheckedState)
                        {
                            if (intChargeType_pkey == clsPrice.CHARGE_CME && !ChargeTypeExist)
                                ErrorMessage = "<b>CME Contact Hours</b><br /> CME contact hours are available only to physicians (MDs and DOs) and physician assistants. You may want to switch to CNE contact hours, which are available to everyone and do not require you to enter your contact hours on our CME partner\'s website. For information about MAGI continuing education contact hours, <a href=\'/Overview?M=32\' target=\'_blank\' >click here</a>.";
                            else if ((intChargeType_pkey == clsPrice.CHARGE_CLE || intChargeType_pkey == clsPrice.CHARGE_CLEPA || intChargeType_pkey == clsPrice.CHARGE_CLECompactStates) && !ChargeTypeExist)
                                ErrorMessage = "<b>CLE Contact Hours</b><br /> MAGI currently offers Pennsylvania CLE contact hours. If you are licensed in a different state, ask your state bar association if it accepts contact hours from this state or a MAGI attendance certificate. For information about MAGI continuing education contact hours, <a href=\'/Overview?M=32\' target=\'_blank\' >click here</a>.";

                            if (!ChargeTypeExist)
                            {
                                strMemo = "Add " + strCert + " for event: " + data.EventId + ").<br/>Via Options page";
                                bPaymentRequired = true;
                                qry = "Insert into Account_Charges(ChargeType_pKey, Account_pKey, Event_pKey, Amount,LoggedByAccount_pKey, LoggedOn, Memo)"
                                    + System.Environment.NewLine + "select t1.pKey, " + data.Id + "," + data.EventId;
                                if (cEventAccount.bSpeakerAtEvent && intChargeType_pkey != clsPrice.CHARGE_ConflictResolutionModes)
                                {
                                    switch (intChargeType_pkey)
                                    {
                                        case clsPrice.CHARGE_CRCPExam:
                                            qry = qry + System.Environment.NewLine + ",-1.0*" + cSettings.intSpkrCRCPCharge.ToString() + " as Amt";
                                            break;
                                        case clsPrice.CHARGE_CME:
                                            qry = qry + System.Environment.NewLine + ",-1.0*" + (cSettings.intSpkrCMECharge).ToString() + " as Amt";
                                            break;
                                        case clsPrice.CHARGE_CNE:
                                            qry = qry + System.Environment.NewLine + ",-1.0*" + (cSettings.intSpkrCNECharge).ToString() + " as Amt";
                                            break;
                                        case clsPrice.CHARGE_CCB:
                                            qry = qry + System.Environment.NewLine + ",-1.0*" + (cSettings.intSpkrCCBCharge).ToString() + " as Amt";
                                            break;
                                        case clsPrice.CHARGE_CLE:
                                            qry = qry + System.Environment.NewLine + ",-1.0*" + (cSettings.intSpkrCLECharge).ToString() + " as Amt";
                                            break;
                                        case clsPrice.CHARGE_CLECompactStates:
                                            qry = qry + System.Environment.NewLine + ",-1.0*" + (cSettings.intSpkrCLECompactCharge).ToString() + " as Amt";
                                            break;
                                        case clsPrice.CHARGE_CLEPA:
                                            qry = qry + System.Environment.NewLine + ",-1.0*" + (cSettings.intSpkrCLEPACharge).ToString() + " as Amt";
                                            break;
                                    }
                                }
                                else
                                    qry = qry + System.Environment.NewLine + ",-1.0*(isNull(t2.Amount,0)+iif(t1.pkey<>2," + ((bConferenceEnd) ? dblLateCharges.ToString() : "0") + ",0)) as Amt";

                                qry = qry + System.Environment.NewLine + "," + data.Id.ToString() + ",getdate(),@Memo  From sys_ChargeTypes t1";
                                qry = qry + System.Environment.NewLine + " Left Outer Join Event_Pricing t2 on t2.ChargeType_pKey = t1.pKey and t2.Event_pKey = @Event_pKey";
                                qry = qry + System.Environment.NewLine + " Where t1.pKey = @pKey";

                                cmd = new SqlCommand(qry);
                                cmd.Parameters.AddWithValue("@Memo", strMemo);
                                cmd.Parameters.AddWithValue("@pKey", intChargeType_pkey);
                                cmd.Parameters.AddWithValue("@Event_pKey", data.EventId);

                                if (!clsUtility.ExecuteQuery(cmd, null, "Manually Log New Charge"))
                                    return Json(new { msg = "Error Manually Logging New Charge", AlertMessage = ErrorMessage }, JsonRequestBehavior.AllowGet);

                                cAccount.intAccount_PKey = data.Id;
                                cAccount.sqlConn = conn;
                                cAccount.LogAuditMessage(strMemo, clsAudit.LOG_CertificationAdd);
                            }
                        }
                        else
                        {

                            if (ChargeTypeExist)
                            {
                                string text2 = "", lblSelectOptions = "";
                                bool rbOptionsVisible = false;
                                clsPrice c = new clsPrice();
                                c.sqlConn = conn;
                                int intPriorChargePKey = c.FindCharge(data.Id, data.EventId, intChargeType_pkey);

                                DataRow[] dtCurrentPrice = dtRelatedCharges.Select("ChargeType_pKey = " + intChargeType_pkey.ToString());
                                string selectedItems = string.Join(",", OptionsModel.EDItems.Where(r => r.CheckedState).Select(r => r.Values));
                                string strCharges = ((!string.IsNullOrEmpty(selectedItems)) ? selectedItems + "," + intChargeType_pkey.ToString() : intChargeType_pkey.ToString());
                                DataRow[] dtRelated = dtRelatedCharges.Select("ChargeType_pKey not in  (" + strCharges + ") and Amount=" + Convert.ToString(dtCurrentPrice[0]["Amount"]));
                                OptionsModel.strCert = strCert;
                                if (dtCharges.Rows.Count > 0)
                                {
                                    bool selectOptions = false;
                                    DataRow[] dtrows = dtCharges.Select("ChargeType_pKey in (" + intChargeType_pkey.ToString() + ")");
                                    if (!(dtrows.Length > 0))
                                    {

                                        text2 = "Create $" + Convert.ToString(dtCurrentPrice[0]["Amount"]) + " voucher that will expire on " + DateTime.Now.AddMonths(cSettings.intRegVoucherExpirationMonth).ToString("MM/dd/yy");
                                        if (dtRelated.Length > 0)
                                            rbOptionsVisible = true;
                                        else
                                        {
                                            rbOptionsVisible = false;
                                            lblSelectOptions = cSettings.getText(clsSettings.TEXT_VoucherRules).Replace("[ExpiredOn]", DateTime.Now.AddMonths(cSettings.intRegVoucherExpirationMonth).ToString("MM/dd/yy"));
                                        }

                                        return Json(new
                                        {
                                            msg = "PopSelectOptions",
                                            lblSelectOptions = lblSelectOptions,
                                            Options1Text = "Select other type of certification",
                                            Options1Val = 0,
                                            Options2Text = text2,
                                            Options2Val = 1,
                                            SelectedValue = 1,
                                            OptionsVisible = rbOptionsVisible,
                                            AlertMessage = ErrorMessage,
                                            PriorChargePKey = intPriorChargePKey,
                                            ChargeTypePKey = intChargeType_pkey,
                                            strCert = OptionsModel.strCert,
                                        }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {
                                    if (cEventAccount.dblAccountBalance >= 0)
                                    {
                                        text2 = "Create $" + Convert.ToString(dtCurrentPrice[0]["Amount"]) + " voucher that will expire on " + DateTime.Now.AddMonths(cSettings.intRegVoucherExpirationMonth).ToString("MM/dd/yy");
                                        if (dtRelated.Length > 0)
                                            rbOptionsVisible = true;
                                        else
                                        {
                                            rbOptionsVisible = false;
                                            lblSelectOptions = cSettings.getText(clsSettings.TEXT_VoucherRules).Replace("[ExpiredOn]", DateTime.Now.AddMonths(cSettings.intRegVoucherExpirationMonth).ToString("MM/dd/yy"));
                                        }

                                        return Json(new
                                        {
                                            msg = "PopSelectOptions",
                                            lblSelectOptions = lblSelectOptions,
                                            Options1Text = "Select other type of certification",
                                            Options1Val = 0,
                                            Options2Text = text2,
                                            Options2Val = 1,
                                            SelectedValue = 1,
                                            OptionsVisible = rbOptionsVisible,
                                            rwSelectOptions = true,
                                            AlertMessage = ErrorMessage,
                                            PriorChargePKey = intPriorChargePKey,
                                            ChargeTypePKey = intChargeType_pkey,
                                            strCert = OptionsModel.strCert,
                                        }, JsonRequestBehavior.AllowGet);
                                    }

                                }

                                bPaymentRequired = false;

                                strMemo = "Unchecked " + strCert + " Charge (Trans#: " + intPriorChargePKey.ToString() + ").<br/>Via Options page";
                                qry = "Update Account_Charges set Reversed = 1 Where pKey = " + intPriorChargePKey.ToString() + ";";
                                qry = qry + System.Environment.NewLine + "Update Account_Charges set IntendedCharges=REPLACE(IntendedCharges,'" + intChargeType_pkey.ToString() + "','0') Where Account_pKey =" + data.Id.ToString() + " and Event_pKey=" + data.EventId.ToString() + ";";
                                qry = qry + System.Environment.NewLine + "Insert into Account_Charges(ChargeType_pKey, Account_pKey, Event_pKey, Amount";
                                qry = qry + System.Environment.NewLine + ",LoggedByAccount_pKey, LoggedOn, Memo, ReversalReference)";
                                qry = qry + System.Environment.NewLine + "select t1.ChargeType_pKey, t1.Account_pKey, t1.Event_pKey, -1.0*t1.Amount";
                                qry = qry + System.Environment.NewLine + "," + data.Id.ToString() + ",getdate(),@Memo," + intPriorChargePKey.ToString();
                                qry = qry + System.Environment.NewLine + " From Account_Charges t1";
                                qry = qry + System.Environment.NewLine + " Where t1.pKey = " + intPriorChargePKey.ToString();
                                cmd = new SqlCommand(qry);
                                cmd.Parameters.AddWithValue("@Memo", strMemo);
                                if (!clsUtility.ExecuteQuery(cmd, null, "Log Reversal"))
                                    return Json(new { msg = "Error Occurred While Reversal", AlertMessage = ErrorMessage }, JsonRequestBehavior.AllowGet);
                                cAccount.intAccount_PKey = data.Id;
                                cAccount.sqlConn = conn;
                                cAccount.LogAuditMessage("Reverse " + strCert + " charge for event: " + data.EventId.ToString(), clsAudit.LOG_CertificationAdd);
                            }
                        }

                    }
                    if (index > 0)
                    {
                        if (cEventAccount.dblAccountBalance < 0 && OptionsModel.EDItems[index].CheckedState && Convert.ToInt32(OptionsModel.EDItems[index].Values) != clsPrice.CHARGE_CRCPExam)
                            return Json(new { msg = "OK", alert = "To make payment, see the Registration Status panel.", AlertMessage = ErrorMessage, }, JsonRequestBehavior.AllowGet);
                    }
                    else if (!bPaymentRequired && cEventAccount.dblAccountBalance > 0)
                        return Json(new { msg = "OK", alert = "Contact MAGI Support for a refund.", AlertMessage = ErrorMessage }, JsonRequestBehavior.AllowGet);

                    clsReminders cReminder = new clsReminders();
                    string ReminderSettings = clsReminders.R_EducationHour.ToString() + "," + clsReminders.R_PaymentDue.ToString();
                    cReminder.UpdateMultipleReminderStatus(data.EventId, data.Id, ReminderSettings);

                    return Json(new { msg = "OK", alert = "", AlertMessage = ErrorMessage }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {

            }
            return Json(new { msg = "Error Occurred While Updating Eduction Hours", AlertMessage = "" }, JsonRequestBehavior.AllowGet);
        }
        private string CreateVoucher(double dblAmount, string strSubject, string strComment, int intAccountChargePkey, int hdnVoucherPkey, clsSettings cSettings, User_Login data)
        {
            if (dblAmount > 0)
            {
                SqlConnection conn = new SqlConnection(ReadConnectionString());
                clsVoucher cOrig = new clsVoucher();
                cOrig.sqlConn = conn;
                cOrig.intVoucher_pKey = hdnVoucherPkey;
                cOrig.LoadVoucher();


                clsVoucher c = new clsVoucher();
                c.sqlConn=  conn;
                c.intVoucher_pKey = 0;

                if (cOrig.intVoucher_pKey > 0)
                {
                    c.intAccount_pkey = cOrig.intAccount_pkey;
                    c.strIssuedTo = cOrig.strIssuedTo;
                    c.intIssuedForEvent_pKey = cOrig.intIssuedForEvent_pKey;
                    c.strIssuedForEvent = cOrig.strIssuedForEvent;
                    c.dtIssuedOn = cOrig.dtIssuedOn;
                    c.dtExpirationDate = cOrig.dtExpirationDate;
                    c.intPaymentMethod = cOrig.intPaymentMethod;
                    c.intReferenceReceipt = cOrig.intReferenceReceipt;
                    c.strPaymentTransAction = cOrig.strPaymentTransAction;
                    c.strCardLastFour = cOrig.strCardLastFour;
                    c.intCancellationReason_pKey = cOrig.intCancellationReason_pKey;
                    c.strVoucherEmail = cOrig.strVoucherEmail;
                }
                else
                {
                    c.intAccount_pkey = data.Id;
                    c.strIssuedTo = data.LastName + ", " + data.FirstName;
                    c.intIssuedForEvent_pKey = data.EventId;
                    c.strIssuedForEvent = data.EventName;
                    c.dtIssuedOn = DateTime.Now;
                    c.dtExpirationDate = c.dtIssuedOn.AddMonths(cSettings.intRegVoucherExpirationMonth);
                    c.strVoucherEmail = data.Email;
                }

                c.dblAmount = dblAmount;
                c.strCancellationComment = strComment;
                c.strComments = strComment;
                if (!c.SaveVoucher())
                    return "Error While Saving Voucher";
                if (c.intVoucher_pKey > 0)
                    clsPayment.UpdateCancellationComments(c.intAccount_pkey, data.EventId, c.intVoucher_pKey, intAccountChargePkey);
                if (c.SendVoucherEmail(clsAnnouncement.Voucher_Cancellation, strSubject))
                    return "The voucher has been created and a confirmation email has been sent.";
                else
                    return "The voucher has been created and error while sending confirmation email";
            }
            return "OK";
        }

        [ValidateInput(true)]
        [CustomizedAuthorize]
        [AjaxValidateAntiForgeryToken]
        public JsonResult cmdOptionOKSave(EdOPtionsModel OptionsModel)
        {

            try
            {
                int intChargeType_pkey = OptionsModel.intChargeType_pkey;
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                SqlConnection conn = new SqlConnection(ReadConnectionString());
                clsLastUsed cLast = (clsLastUsed)Session["cLastUsed"];
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                clsAccount cAccount = new clsAccount();

                if (OptionsModel.rbOptionsSelected == 0)
                {

                    DataTable dtCharges = (dba.PaymentType(data.Id, data.EventId));
                    DataTable dtRelatedCharges = dba.GetCRCPOption(OptionsModel.bSpeakerAtEvent, data.EventId);

                    DataRow[] dtCurrentPrice = dtRelatedCharges.Select("ChargeType_pKey = " + intChargeType_pkey.ToString());
                    string selectedItems = string.Join(",", OptionsModel.EDItems.Where(r => r.CheckedState).Select(r => r.Values));
                    string strCharges = ((!string.IsNullOrEmpty(selectedItems)) ? selectedItems + "," + intChargeType_pkey.ToString() : intChargeType_pkey.ToString());
                    DataRow[] dtRelated = dtRelatedCharges.Select("ChargeType_pKey not in  (" + strCharges + ") and Amount=" + Convert.ToString(dtCurrentPrice[0]["Amount"]));
                    string text2 = "", lblSelectOptions = "";
                    bool rbOptionsVisible = false;
                    clsPrice c = new clsPrice();
                    c.sqlConn = conn;

                    clsEventAccount cEventAccount = new clsEventAccount();
                    cEventAccount.intAccount_pKey = data.Id;
                    cEventAccount.intEvent_pKey = data.EventId;
                    cEventAccount.sqlConn = conn;
                    cEventAccount.LoadEventInfo(true);
                    string ErrorMessage = "";
                    int intPriorChargePKey = c.FindCharge(data.Id, data.EventId, intChargeType_pkey);
                    bool pnlConfirmation = false, lblConfirmReplacement = false, popupconfirm = false;
                    DataTable dtRelatedUpdate = new DataTable();
                    string rbEdOptionsSelected = "0";
                    if (dtCharges.Rows.Count > 0)
                    {
                        DataRow[] dtrows = dtCharges.Select("ChargeType_pKey in (" + intChargeType_pkey.ToString() + ")");
                        if (!(dtrows.Length > 0))
                        {
                            if (dtRelated.Length > 0)
                            {
                                pnlConfirmation = true;
                                dtRelatedUpdate = dtRelated.CopyToDataTable();
                            }
                            popupconfirm = true;
                            return Json(new
                            {
                                msg = "PopUpConfirm",
                                pnlConfirmation = pnlConfirmation,
                                lblConfirmReplacement = lblConfirmReplacement,
                                popupconfirm = popupconfirm,
                                dtRelatedUpdate = JsonConvert.SerializeObject(dtRelatedUpdate),
                                rbEdOptionsSelected = rbEdOptionsSelected,
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (cEventAccount.dblAccountBalance >= 0)
                        {
                            if (dtRelated.Length > 0)
                            {
                                pnlConfirmation = true;
                                dtRelatedUpdate = dtRelated.CopyToDataTable();
                            }
                            return Json(new
                            {
                                msg = "PopUpConfirm",
                                pnlConfirmation = pnlConfirmation,
                                lblConfirmReplacement = lblConfirmReplacement,
                                popupconfirm = popupconfirm,
                                dtRelatedUpdate = JsonConvert.SerializeObject(dtRelatedUpdate),
                                rbEdOptionsSelected = rbEdOptionsSelected,
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    string qry = " select t1.ChargeType_pKey, -1.0*t1.Amount as Amount From Account_Charges t1 Where t1.pKey =@pKey ";
                    double dblAfter = 0;
                    SqlCommand cmd1 = new SqlCommand(qry);
                    cmd1.Parameters.AddWithValue("@pKey", OptionsModel.PriorChargePKey);
                    DataTable dt = new DataTable();
                    if (clsUtility.GetDataTable(conn, cmd1, ref dt))
                    {
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            if (dt.Rows[0]["Amount"] != System.DBNull.Value)
                                dblAfter = Convert.ToDouble(dt.Rows[0]["Amount"]);
                        }
                    }

                    string strMemo = "Unchecked " + OptionsModel.strCert + " Charge (Trans#: " + OptionsModel.PriorChargePKey.ToString() + ").<br/>Via Options page";
                    qry = qry + System.Environment.NewLine + "Update Account_Charges set Reversed = 1 Where pKey = " + OptionsModel.PriorChargePKey.ToString() + ";";
                    qry = qry + System.Environment.NewLine + "Update Account_Charges set IntendedCharges=REPLACE(IntendedCharges,'" + OptionsModel.intChargeType_pkey.ToString() + "','0') Where Account_pKey =" + data.Id.ToString() + " and Event_pKey=" + data.EventId.ToString() + ";";
                    qry = qry + System.Environment.NewLine + "Insert into Account_Charges(ChargeType_pKey, Account_pKey, Event_pKey, Amount";
                    qry = qry + System.Environment.NewLine + ",LoggedByAccount_pKey, LoggedOn, Memo, ReversalReference)";
                    qry = qry + System.Environment.NewLine + "select t1.ChargeType_pKey, t1.Account_pKey, t1.Event_pKey, -1.0*t1.Amount";
                    qry = qry + System.Environment.NewLine + "," + data.Id.ToString() + ",getdate(),@Memo," + OptionsModel.PriorChargePKey.ToString();
                    qry = qry + System.Environment.NewLine + " From Account_Charges t1";
                    qry = qry + System.Environment.NewLine + " Where t1.pKey = " + OptionsModel.PriorChargePKey.ToString();

                    SqlCommand cmd = new SqlCommand(qry);
                    cmd.Parameters.AddWithValue("@Memo", strMemo);
                    cmd.Parameters.AddWithValue("@pKey", OptionsModel.PriorChargePKey);
                    if (!clsUtility.ExecuteQuery(cmd, null, "Log Reversal"))
                        return Json(new { msg = "Error Occurred While Reversal" }, JsonRequestBehavior.AllowGet);

                    qry = "Insert into Account_Charges(ChargeType_pKey, Account_pKey, Event_pKey,Amount,LoggedByAccount_pKey, LoggedOn, Memo)";
                    qry = qry + System.Environment.NewLine + "Values(" + clsPrice.CHARGE_VoucherPurchase.ToString() + "," + data.Id.ToString() + "," + data.EventId.ToString();
                    qry = qry + System.Environment.NewLine + "," + (-1.0 * dblAfter).ToString();
                    qry = qry + System.Environment.NewLine + "," + data.Id.ToString() + ",getdate(), 'Event Voucher Purchase')";

                    SqlCommand cmd2 = new SqlCommand(qry);
                    if (!clsUtility.ExecuteQuery(cmd2, null, "Log Voucher Purchase"))
                        return Json(new { msg = "Error Occurred While Logging Voucher Purchase" }, JsonRequestBehavior.AllowGet);

                    if (string.IsNullOrEmpty(OptionsModel.strCert))
                        OptionsModel.strCert = "";

                    string msg = CreateVoucher(dblAfter, "MAGI " + OptionsModel.strCert + " Cancellation Voucher", OptionsModel.strCert + " Cancellation", OptionsModel.PriorChargePKey, OptionsModel.hdnVoucherPkey, cSettings, data);
                    return Json(new { msg = msg }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {

            }
            return Json(new { msg = "Error Occurred" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult OptionsConfirmSave(int intPriorChargePKey, int intChargeType_pkey, int SelectedValue)
        {
            try
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                SqlConnection conn = new SqlConnection(ReadConnectionString());

                string qry = "select t1.ChargeType_pKey, -1.0*t1.Amount as Amount From Account_Charges t1 Where t1.pKey = @pKey";
                double dblAfter = 0;
                SqlCommand cmd1 = new SqlCommand(qry);
                cmd1.Parameters.AddWithValue("@pKey", intPriorChargePKey);
                DataTable dt = new DataTable();
                if (clsUtility.GetDataTable(conn, cmd1, ref dt))
                {
                    if (dt != null && dt.Rows.Count > 0)
                        double.TryParse(dt.Rows[0]["Amount"].ToString(), out dblAfter);
                }
                qry = "";
                if (SelectedValue > 0)
                {
                    qry = qry + System.Environment.NewLine + "Update Account_Charges set ChargeType_pKey = @ChargeType_pKey Where pKey = @pKey;";
                    qry = qry + System.Environment.NewLine + "Update Account_Charges set IntendedCharges=REPLACE(IntendedCharges,@ChargeTypepKey,@ChargeType_pKey) Where Account_pKey =" + data.Id.ToString() + " and Event_pKey=" + data.EventId.ToString() + ";";
                    SqlCommand cmd = new SqlCommand(qry);
                    cmd.Parameters.AddWithValue("@pKey", intPriorChargePKey);
                    cmd.Parameters.AddWithValue("@ChargeTypepKey", intChargeType_pkey);
                    cmd.Parameters.AddWithValue("@ChargeType_pKey", SelectedValue);
                    if (!clsUtility.ExecuteQuery(cmd, null, "Log Reversal"))
                        return Json(new { msg = "Error Occurred While Reversal" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);

            }
            catch
            {

            }
            return Json(new { msg = "Error occurred while confirming" }, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(true)]
        [CustomizedAuthorize]
        [AjaxValidateAntiForgeryToken]
        public ActionResult SaveEditBadge(string txtBName, string txtBTitle, string txtBOrg, string hdnBName, string hdnBTitle, string hdnBOrg, double dblAccountAmount, string bCurrentAttendee, string actiontype)
        {
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int AccountPkey = data.Id;
            int EventPkey = data.EventId;

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EventPkey;
            cEvent.sqlConn = new SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();

            clsAccount cAccount = new clsAccount();
            cAccount.sqlConn = new SqlConnection(ReadConnectionString());
            cAccount.intAccount_PKey = AccountPkey;
            cAccount.LoadAccount();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);

            bool boolCurrentAttendee = false;
            if (bCurrentAttendee == "true")
                boolCurrentAttendee = true;

            if (cEvent.bEdBadge && cEvent.intAttEdBadge == clsEvent.intAttAttending && !boolCurrentAttendee && !cAccount.bGlobalAdministrator)
                return Json(new { result = "error", msg = "You are not Attendee of this event." }, JsonRequestBehavior.AllowGet);

            if (actiontype == "SaveEdits")
            {
                string strName = txtBName;
                string strTitle = txtBTitle;
                string strOrg = txtBOrg;
                if (cAccount.bGlobalAdministrator)
                {
                    clsBadgeDesign.SaveEdits(cAccount, strName, strTitle, strOrg, null, AccountPkey, "Attendee");
                    return Json(new { result = "OK", msg = "Updated" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (cEvent.bEdBadge && (cEvent.bOptions && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "MyMAGIMenu_19")))
                    {
                        if ((cEvent.intAttEdBadge > 0) && (dblAccountAmount <= -cSettings.intAttAccessBal))
                        {
                            string lblFAmsg = "To access this feature, please pay your balance due of " + String.Format("{0:c}", dblAccountAmount);  // FAccessPopup();
                            return Json(new { result = "PendingPayment", lblFAmsg }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            clsBadgeDesign.SaveEdits(cAccount, strName, strTitle, strOrg, null, AccountPkey, "Attendee");
                            if (hdnBName != txtBName || hdnBTitle != txtBTitle || hdnBOrg != txtBOrg)
                            {
                                var disapprove = dba.DisApproveOneBadge(data.EventAccount_pkey);
                                if (disapprove)
                                    return Json(new { result = "OK", msg = "Updated" }, JsonRequestBehavior.AllowGet);
                                else
                                    return Json(new { result = "Failed", msg = "Unable to Update." }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else
                        return Json(new { result = "error", msg = "This feature is not yet available." }, JsonRequestBehavior.AllowGet);
                }
            }
            else if (actiontype == "ResetBadge")
            {
                if (cEvent.bEdBadge && (cEvent.bOptions && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "MyMAGIMenu_19")))
                {
                    if ((cEvent.intAttEdBadge > 0) && dblAccountAmount <= -cSettings.intAttAccessBal)
                    {
                        string lblFAmsg = "To access this feature, please pay your balance due of " + String.Format("{0:c}", dblAccountAmount);  // FAccessPopup();
                        return Json(new { result = "PendingPayment", lblFAmsg }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var dtreset = dba.ResetBadge(AccountPkey);
                        if (dtreset.Rows.Count > 0)
                        {
                            txtBName = dtreset.Rows[0]["BName"].ToString();
                            txtBTitle = dtreset.Rows[0]["BTitle"].ToString();
                            txtBOrg = dtreset.Rows[0]["BOrganizationID"].ToString();
                        }
                        else
                        {
                            return Json(new { result = "error", msg = "Error on badge info reset." }, JsonRequestBehavior.AllowGet);
                        }
                        return Json(new { result = "OK", txtBName, txtBTitle, txtBOrg }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { result = "error", msg = "This feature is not yet available." }, JsonRequestBehavior.AllowGet);
                }
            }

            return null;
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult cmdVoucherTransferSave(string selectedValue, string VoucherTransferEmail, string comment)
        {

            string msg = "";

            if (string.IsNullOrEmpty(VoucherTransferEmail))
                msg = "Enter transferee\'s email address.";

            if (!clsEmail.IsValidEmail(VoucherTransferEmail.Trim()))
                msg = "Enter valid transferee\'s email address.";

            if (string.IsNullOrEmpty(msg))
            {
                msg = "Error Occurred While Updating Voucher";
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                bool result = new SqlOperation().TransferVoucherSave(selectedValue, VoucherTransferEmail, data.Email, comment, data.Id.ToString());
                if (result)
                {
                    string resultMsg = "Voucher #" + selectedValue + " has been transferred to " + VoucherTransferEmail.Trim() + ".";
                    clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                    if (cSettings.bSendVoucherTransferEmail)
                        SendVoucherTransferEmail(VoucherTransferEmail.Trim(), selectedValue, data.EventId, data.Id);

                    return Json(new { msg = "OK", result = resultMsg }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { msg = msg }, JsonRequestBehavior.AllowGet);
        }
        private bool SendVoucherTransferEmail(string strTargetEmail, string voucherkey, int EventID, int AccountID)
        {
            try
            {
                string strContent = "", strSubject = "", strSrcEmailAddress = "", strTrgtEmailAddress = "";

                clsEvent cevt = new clsEvent();
                clsVoucher cVoucher = new clsVoucher();
                SqlConnection conn = new SqlConnection(ReadConnectionString());
                clsAccount SrcAcct = new clsAccount();
                clsAccount TrgtAcct = new clsAccount();

                cevt.sqlConn = conn;
                cVoucher.sqlConn = conn;
                SrcAcct.sqlConn = conn;
                TrgtAcct.sqlConn = conn;

                cevt.intEvent_PKey = EventID;
                cVoucher.intVoucher_pKey = Convert.ToInt32(voucherkey);
                SrcAcct.intAccount_PKey = AccountID;
                TrgtAcct.strEmail = strTargetEmail;

                cevt.LoadEvent();
                cVoucher.LoadVoucher();
                SrcAcct.LoadAccount();
                TrgtAcct.LoadAccountByEmail();

                strSrcEmailAddress = SrcAcct.strEmail;
                strTrgtEmailAddress = TrgtAcct.strEmail;

                if (!string.IsNullOrEmpty(strSrcEmailAddress) && !string.IsNullOrEmpty(strTrgtEmailAddress))
                {
                    clsAnnouncement Ann = clsAnnouncement.getAnnouncementInfo(conn, null, clsAnnouncement.VoucherTransferEmail);
                    strSubject = Ann.strTitle;
                    strContent = Ann.strHTMLText;

                    if (!string.IsNullOrEmpty(strContent))
                    {
                        strSubject = SrcAcct.ReplaceReservedWords(strSubject);
                        strContent = SrcAcct.ReplaceReservedWords(strContent);

                        strContent = strContent.Replace("[Voucher_TransfereeName]", TrgtAcct.strFirstname + " " + TrgtAcct.strLastname);
                        strContent = strContent.Replace("[Voucher_TransferrorName]", SrcAcct.strFirstname + " " + SrcAcct.strLastname);

                        strContent = cevt.ReplaceReservedWords(strContent);
                        strSubject = cevt.ReplaceReservedWords(strSubject);

                        strSubject = cVoucher.ReplaceVoucherWords(strSubject);
                        strContent = cVoucher.ReplaceVoucherWords(strContent);

                        clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                        clsEmail cEmail = new clsEmail();
                        cEmail.sqlConn = conn;
                        cEmail.strSubjectLine = strSubject;
                        cEmail.strHTMLContent = strContent;
                        cEmail.strEmailCC = strSrcEmailAddress;
                        cEmail.strEmailBCC = cSettings.strSupportEmail.ToString();
                        if (!cEmail.SendEmailToAddress(strTrgtEmailAddress))
                            return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public string updateCkReminder(bool WantReminder)
        {
            try
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                SqlConnection conn = new SqlConnection(ReadConnectionString());

                clsEventAccount cEA = new clsEventAccount();
                cEA.sqlConn = conn;
                cEA.intAccount_pKey = data.Id;
                cEA.intEvent_pKey = data.EventId;
                if (cEA.LoadEventInfo(true))
                {
                    cEA.bWantReminder = WantReminder;
                    if (cEA.UpdateReminder())
                        return "Saved";
                }
            }
            catch (Exception ex)
            {

            }
            return "Error occurred while updating reminder";
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult cmdCCRefundSave(string lblReceiptNumber, int VoucherID, string lblCCRefundCardTran, string comment, string Amount, double hdnCCMainAmount, string hdnCCRefundCardNo)
        {
            string msg = "";
            string sReceiptNumber = lblReceiptNumber.Replace("R", "");
            if (sReceiptNumber == "0")
                msg = "Select receipt number.";

            if (string.IsNullOrEmpty(lblCCRefundCardTran))
                msg = "Transaction id should not be blank.";

            if (lblCCRefundCardTran.Trim() == "" || lblCCRefundCardTran.Trim() == "0")
                msg = "Transaction id should not be blank.";

            if (comment.Trim() == "")
                msg = "Enter comment.";

            string strAmount = Amount.Trim().Replace("$", "").Replace(",", "");

            double dAmount = 0;
            double.TryParse(strAmount, out dAmount);

            if (dAmount > hdnCCMainAmount)
                msg = "Refund amount should be less than or equal to paid amount.";

            if (string.IsNullOrEmpty(msg))
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                SqlConnection conn = new SqlConnection(ReadConnectionString());

                int RecipetNo = 0;
                int.TryParse(sReceiptNumber, out RecipetNo);

                clsPayment cp = new clsPayment();
                cp.sqlConn = conn;
                cp.dblAmount = hdnCCMainAmount;
                cp.strCardLastFour = hdnCCRefundCardNo;
                cp.strRefundCardTransactionID = lblCCRefundCardTran;
                cp.intRefundReceiptNumber = RecipetNo;
                if (!cp.RefundCreditCardProcessor())
                    msg = cp.strCardFailureReason;
                else
                {
                    clsVoucher cVoucher = new clsVoucher();
                    cVoucher.sqlConn = conn;
                    cVoucher.intVoucher_pKey = VoucherID;

                    clsVoucher cOrig = new clsVoucher();
                    cOrig.sqlConn = conn;
                    cOrig.intVoucher_pKey = VoucherID;
                    cOrig.LoadVoucher();

                    cVoucher.intAccount_pkey = data.Id;
                    cVoucher.dtUsedOn = DateTime.Now;
                    cVoucher.bIsUsed = true;
                    cVoucher.UpdatedByAccount_pkey = data.Id;
                    cVoucher.strComments = "Changed Status from " + cOrig.strVoucherStatus + " to Refunded.";
                    if (!cVoucher.SaveVoucher())
                        msg = "Error while updating voucher";
                    else
                        msg = "OK"; // Amount has been refunded.
                }
            }
            return Json(new { msg = msg }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        public FileResult ViewCoupon(string strFilename)
        {
            if (!string.IsNullOrEmpty(strFilename))
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                SqlConnection conn = new SqlConnection(ReadConnectionString());

                clsEvent cEvent = new clsEvent();
                cEvent.sqlConn = conn;
                cEvent.intEvent_PKey = data.EventId;
                cEvent.LoadEvent();
                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.sqlConn = conn;
                cEventAccount.intEventAccount_pKey = data.EventAccount_pkey;
                string strPhysicalPath = Server.MapPath("~/CouponFiles/" + cEvent.strFileGUID + "_" + strFilename);
                if (clsUtility.FileExists(strPhysicalPath))
                {
                    if (!cEventAccount.LogCouponDownload(strFilename))
                        return null;
                    Aspose.Pdf.Document pdfDocument = new Aspose.Pdf.Document(strPhysicalPath);
                    pdfDocument.Info.Title = "MAGI";
                    Aspose.Pdf.Page pdfPage = pdfDocument.Pages[1];

                    clsUtility.PDFAddTextToPage(pdfDocument, pdfPage, (data.FirstName + " " + data.LastName), 50, 655, "Verdana", 22, System.Drawing.Color.Black, System.Drawing.Color.White, false, true, true, bCenterInRange: true);

                    Random generator = new Random();
                    string strSerialNumber = "C" + generator.Next(1, 999999).ToString("D6").ToString();
                    clsUtility.PDFAddTextToPage(pdfDocument, pdfPage, strSerialNumber, 472, 410, "Verdana", 8, System.Drawing.Color.Black, System.Drawing.Color.White, false, false);
                    string DestDir = Server.MapPath("~/app_data/downloadtemp/");
                    string strDownLoadPath = Server.MapPath("~/app_data/downloadtemp/" + "Coupon_" + data.Id.ToString() + strFilename);
                    if (System.IO.File.Exists(strDownLoadPath))
                    {
                        if (!clsUtility.DeleteFile(strDownLoadPath, null))
                            return null;
                    }
                    pdfDocument.Save(strDownLoadPath);

                    if (System.IO.File.Exists(strDownLoadPath))
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(strDownLoadPath);
                        return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, strFilename);
                    }
                    return null;
                }
                else
                {
                    System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;
                    clsUtility.LogErrorMessage(null, request, this.GetType().Name, 0, "Coupon file not found: " + strFilename);
                }
            }
            return null;
        }

        public ActionResult _PartialSessionChair(bool IsPage = false)
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            SqlConnection conn = new SqlConnection(ReadConnectionString());

            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            string RegistrationLevelPkey = "";
            int intRegistrationLevel_pKey = 0, intAttendeeStatus = 0;
            intAttendeeStatus =  clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref RegistrationLevelPkey);
            intRegistrationLevel_pKey= Convert.ToInt32(RegistrationLevelPkey);

            ViewBag.ShowChairPreference=false;
            ViewBag.Redirect = "";
            if (!clsEventAccount.getSpeakerStatus(data.Id, data.EventId))
            {
                if (IsPage)
                {
                    ViewBag.Redirect  = "/Home/Index";
                    return PartialView();
                }
                else
                    return PartialView();
            }

            ViewBag.ShowChairPreference=true;
            ViewBag.ChairPreference =  new SqlOperation().getChairPreference();
            ViewBag.TrackList =  new SqlOperation().getTracksList(data.EventId);
            ViewBag.SessionCombo =  new SqlOperation().getSessionCombo(data.Id, data.EventId);

            System.Data.DataTable EventInfo = repository.getDyamicEventSettings(data.EventId, "StartDate,EndDate,SkipRegDate");
            string SkipRegDate = "";
            DateTime dtCurEventStart = DateTime.Now, dtCurEventEnd = DateTime.Now;
            if (EventInfo != null && EventInfo.Rows.Count > 0)
            {
                dtCurEventStart = (EventInfo.Rows[0]["StartDate"] != System.DBNull.Value) ? Convert.ToDateTime(EventInfo.Rows[0]["StartDate"]) : DateTime.Now;
                dtCurEventEnd = (EventInfo.Rows[0]["EndDate"] != System.DBNull.Value) ? Convert.ToDateTime(EventInfo.Rows[0]["EndDate"]) : DateTime.Now;
                SkipRegDate = (EventInfo.Rows[0]["SkipRegDate"] != System.DBNull.Value) ? Convert.ToString(EventInfo.Rows[0]["SkipRegDate"]) : "";
            }
            List<GenericListItem> values = new List<GenericListItem>();
            //DateTime d = dtCurEventStart;
            //do
            //{
            //    d = d.AddDays(1);
            //    if (!SkipRegDate.Contains(d.ToString("yyyy-MM-dd")))
            //        values.Add(new GenericListItem() { strText = d.ToString("MM/dd/yy"), value = d.ToShortDateString() });
            //} while (d <= dtCurEventEnd);
            ViewBag.ddStartSchedule =null;
            DataTable dtData = repository.GetDateList(data.EventId);
            if (dtData!= null && dtData.Rows.Count>0)
                ViewBag.ddStartSchedule = dtData.DataTableToList<GenericListItem>();

            ViewBag.lblMyPreferences = cSettings.getText(clsSettings.Text_MySessionChairPreferences);
            string time = "";
            DataTable dtPrefrenceInfo = new SqlOperation().GetSessionChairInfo(data.Id, data.EventId);
            ViewBag.ddChairPreference_Selected = "";
            ViewBag.trChairVisible = false;
            ViewBag.Selected_TrackpKey = "";
            ViewBag.Selected_SessionpKey = "";
            ViewBag.trTime_Visible = false;
            ViewBag.trTrack_Visible = false;
            ViewBag.trSession_Visible = false;
            ViewBag.cmdApply = false;
            ViewBag.rblChrSssn_Selected = "0";
            if (dtPrefrenceInfo != null && dtPrefrenceInfo.Rows.Count> 0)
            {
                string InterestinBeingaChair = (dtPrefrenceInfo.Rows[0]["InterestinBeingaChair"]!= System.DBNull.Value) ? dtPrefrenceInfo.Rows[0]["InterestinBeingaChair"].ToString() : "";
                ViewBag.rblChrSssn_Selected = ((InterestinBeingaChair.ToLower() == "true") ? "1" : "0");
                ViewBag.ddChairPreference_Selected =  (dtPrefrenceInfo.Rows[0]["SessionChairPreference"]!= System.DBNull.Value) ? dtPrefrenceInfo.Rows[0]["SessionChairPreference"].ToString() : "";

                ViewBag.trChairVisible= (ViewBag.ddChairPreference_Selected=="0") ? false : true;
                string TrackpKey = (dtPrefrenceInfo.Rows[0]["Track_pkey"]!= System.DBNull.Value) ? dtPrefrenceInfo.Rows[0]["Track_pkey"].ToString() : "";
                ViewBag.Selected_TrackpKey = TrackpKey;
                string SessionpKey = (dtPrefrenceInfo.Rows[0]["Session_pKey"]!= System.DBNull.Value) ? dtPrefrenceInfo.Rows[0]["Session_pKey"].ToString() : "";
                ViewBag.Selected_SessionpKey = SessionpKey;
                time = (dtPrefrenceInfo.Rows[0]["PreferedTime"]!= System.DBNull.Value) ? dtPrefrenceInfo.Rows[0]["PreferedTime"].ToString() : "";
            }
            ViewBag.dtlSelected = "";
            if (!string.IsNullOrEmpty(time) && time !="0")
                ViewBag.dtlSelected = time;
            return PartialView();
        }

        [CustomizedAuthorize]
        public ActionResult MySessionChair()
        {
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                int AccountPkey = data.Id, EventPkey = data.EventId;
                string RegistrationLevelPkey = "";
                int intRegistrationLevel_pKey = 0, intAttendeeStatus = 0;
                intAttendeeStatus =  clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref RegistrationLevelPkey);
                intRegistrationLevel_pKey= Convert.ToInt32(RegistrationLevelPkey);

                if (intAttendeeStatus >1)
                    return RedirectToAction("Index", "Home");

                if (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_StudentOnly || intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_SingleSessionOnly)
                    return Redirect("/SingleSession");

                if (!clsEventAccount.getSpeakerStatus(data.Id, data.EventId))
                    return RedirectToAction("Index", "Home");

                ViewBag.ID = AccountPkey;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.EventPKey = EventPkey;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);
            }
            else
                return RedirectToAction("Index", "Home");

            return View();
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public string ClearChairPreference()
        {
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            bool result = new SqlOperation().ClearSesssionChairPreference(data.Id, data.EventId);
            if (result)
                return "OK";
            else
                return "Error While Clearing Preference";
        }
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public string SaveSessionChairPreference(int rblChrSssn, int ddChairPreference, string time, string getTrackList, string getSessionList)
        {
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

            if (rblChrSssn == 1 && ddChairPreference == 0)
                return "Select any session chair preference";

            StringBuilder getSessionLists = new StringBuilder(), getTrackLists = new StringBuilder();

            foreach (string s in getSessionList.Split(','))
                getSessionLists.Append("<" + s + ">");

            foreach (string s in getTrackList.Split(','))
                getTrackLists.Append("<" + s + ">");

            bool result = new SqlOperation().SaveSessionChairPreference(data.Id, data.EventId, rblChrSssn, ddChairPreference, time, getTrackLists.ToString(), getSessionLists.ToString());
            if (result)
                return "OK";
            else
                return "Error While Saving Preference";
        }

        #endregion

        #region MyStaff Console

        [CustomizedAuthorize]
        public ActionResult MyStaffPage()
        {
            MyStaffConsole mystaffconsole = new MyStaffConsole();
            if (User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                int AccountPkey = data.Id;
                int EventPkey = data.EventId;
                clsLastUsed cLast = (clsLastUsed)Session["cLastUsed"];

                ViewBag.ID = AccountPkey;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.LblTitle = data.EventName + ": My Schedule - View";
                ViewBag.EventPKey = EventPkey; // data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);

                ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);

                clsEvent cEvent = new clsEvent();
                cEvent.intEvent_PKey = EventPkey;
                cEvent.sqlConn = new SqlConnection(ReadConnectionString());
                cEvent.LoadEvent();


                clsAccount cAccount = new clsAccount();
                cAccount.sqlConn = new SqlConnection(ReadConnectionString());
                cAccount.intAccount_PKey = AccountPkey;
                cAccount.LoadAccount();

                if (cAccount.isAllowedByPriv(clsPrivileges.CrisisAlert))
                {
                    ViewBag.scriptForCrisis = true;
                    ViewBag.notificationsOfCrisis = true;
                    ViewBag.placeHolderForCrisisWIndow = true;
                }
                else
                {
                    ViewBag.scriptForCrisis = false;
                    ViewBag.notificationsOfCrisis = false;
                    ViewBag.placeHolderForCrisisWIndow = false;
                }

                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.sqlConn = new SqlConnection(ReadConnectionString());
                cEventAccount.intEvent_pKey = data.EventId;
                cEventAccount.intAccount_pKey = data.Id;
                cEventAccount.LoadEventInfo(true);

                if (!(cAccount.bGlobalAdministrator || cAccount.bStaffMember || cLast.bSpeacialAccount) && cEvent.intEventStatus_PKey == clsEvent.STATUS_Completed)
                {
                    // clsUtility.GoHome();
                }

                ViewBag.lblTitle = cEvent.strEventID + ": My Staff Console";

                ViewBag.txtMyNote = cEventAccount.strMyNote;



                bool bVirtualEvent = false;

                if (cEvent.intEventType_PKey == 5)
                    bVirtualEvent = true;

                ViewBag.Task = true;
                ViewBag.SessionChair = true;
                ViewBag.Notes = true;
                ViewBag.Badges = true;
                ViewBag.COI = true;
                ViewBag.Travel = true;
                ViewBag.Communicator = true;
                ViewBag.RegAggregate = true;
                ViewBag.RegCumulative = true;
                ViewBag.SpeakerAssignments = true;
                ViewBag.SpeakerDinner = true;  // pending in view.
                ViewBag.Recent = true;
                ViewBag.w9Request = true;

                Microsoft.VisualBasic.Collection colDocks = cAccount.getDocksAtEvent(cLast.intEventSelector);
                if (colDocks != null)
                    colDocks.Add(0, "W0");
                int intwidgetPKey = 1;


                for (intwidgetPKey = 1; intwidgetPKey <= 13; intwidgetPKey++)
                {
                    bool bVis = colDocks.Contains("W" + intwidgetPKey.ToString());

                    switch (intwidgetPKey)
                    {
                        case 1:
                            if (!bVis)
                                ViewBag.Task = false;
                            break;
                        case 2:
                            if (!bVis)
                                ViewBag.SessionChair = false;
                            break;
                        case 3:
                            if (!bVis)
                                ViewBag.Notes = false;
                            break;
                        case 4:
                            if (!bVis)
                                ViewBag.Badges = false;
                            if (bVirtualEvent)
                                ViewBag.Badges = false;
                            break;
                        case 5:
                            if (!bVis)
                                ViewBag.COI = false;
                            break;
                        case 6:
                            if (!bVis)
                                ViewBag.Travel = false;
                            if (bVirtualEvent)
                                ViewBag.Travel = false;
                            break;
                        case 7:
                            if (!bVis)
                                ViewBag.Communicator = false;
                            break;
                        case 8:
                            if (!bVis)
                                ViewBag.RegAggregate = false;
                            break;
                        case 9:
                            if (!bVis)
                                ViewBag.RegCumulative = false;
                            break;
                        case 10:
                            if (!bVis)
                                ViewBag.SpeakerAssignments = false;
                            break;
                        case 11:
                            if (!bVis)
                                ViewBag.SpeakerDinner = false;
                            if (!cEvent.bDinnerActive)
                                ViewBag.SpeakerDinner = false;
                            break;
                        case 12:
                            if (!bVis)
                                ViewBag.Recent = false;
                            break;
                        case 13:
                            if (!bVis)
                                ViewBag.w9Request = false;
                            break;
                    }
                }
                //******************* QuickLinks**************

                var dtLinks = dba.RefreshRecent(AccountPkey, EventPkey);
                mystaffconsole.RecentLinks = dtLinks;


                //************* RefreshTasks **************
                int intTotal = 0;
                int intDue = 0;
                var dttasks = dba.RefreshTasks(AccountPkey, cLast.intActiveEventPkey, cSettings.intNextEvent_pKey, cSettings.intPrimaryEvent_pkey);
                if (dttasks.Rows.Count > 0)
                {
                    intTotal = Convert.ToInt32(dttasks.Rows[0]["TotalAssigned"].ToString());
                    intDue = Convert.ToInt32(dttasks.Rows[0]["DueNow"].ToString());
                }

                ViewBag.lblTotal = intTotal;
                ViewBag.lblDue = intDue;
                ViewBag.LblDueColor = intDue > 0 ? "red" : "black";

                //************* Refresh Chairs **************
                int intTotalChairs = 0;
                int intassignedChair = 0;
                var dtChairs = dba.RefreshChairs(EventPkey);
                if (dtChairs.Rows.Count > 0)
                {
                    intTotalChairs = Convert.ToInt32(dtChairs.Rows[0]["NumPlanned"].ToString());
                    intassignedChair = Convert.ToInt32(dtChairs.Rows[0]["NumAssigned"].ToString());
                }
                ViewBag.lblChairPlan = intTotalChairs;
                ViewBag.lblChairAss = intassignedChair;
                ViewBag.lblChairNeed = intTotalChairs - intassignedChair;
                ViewBag.lblChairneedcolor = (intTotalChairs - intassignedChair) > 0 ? "red" : "black";

                //************* Refreshw9  **************
                int intTotalW9 = 0;
                var dtW9 = dba.Refreshw9(EventPkey);
                if (dtW9.Rows.Count > 0)
                {
                    intTotalW9 = Convert.ToInt32(dtW9.Rows[0]["TotalPending"].ToString());
                }

                ViewBag.lblw9Pending = intTotalW9;
                ViewBag.lblw9PendingColor = (intTotalW9 > 0 ? "red" : "black");


                //****************Badges**************
                int intTotalBadges = 0;
                int intAwait = 0;
                int intChanged = 0;
                int intNeedReview = 0;
                var dtBadges = dba.RefreshBadges(AccountPkey, cLast.intActiveEventPkey);
                if (dtBadges.Rows.Count > 0)
                {
                    intTotalBadges = Convert.ToInt32(dtBadges.Rows[0]["TotalBadges"].ToString());
                    intAwait = Convert.ToInt32(dtBadges.Rows[0]["AwaitingApproval"].ToString());
                    intChanged = Convert.ToInt32(dtBadges.Rows[0]["ApprovedButChanged"].ToString());
                    intNeedReview = Convert.ToInt32(dtBadges.Rows[0]["NeedReview"].ToString());

                }
                ViewBag.lblBTotal = intTotalBadges;
                ViewBag.lblBAwait = intAwait;
                ViewBag.lblBAwaitColor = (intAwait > 0 ? "red" : "black");
                ViewBag.lblBChanged = intChanged;
                ViewBag.lblBChangedColor = (intChanged > 0 ? "red" : "black");
                ViewBag.lblBReview = intNeedReview;
                ViewBag.lblBReviewColor = (intNeedReview > 0 ? "red" : "black");

                //**************** COI **************
                int intConTotal = 0;
                int intConWith = 0;
                int intConWithByDef = 0;
                int intConWithout = 0;
                int intConNA = 0;

                var dtCOI = dba.RefreshCOI(EventPkey);
                if (dtCOI.Rows.Count > 0)
                {
                    intConTotal = Convert.ToInt32(dtCOI.Rows[0]["NumSpeakers"].ToString());
                    intConWith = Convert.ToInt32(dtCOI.Rows[0]["NumWith"].ToString());
                    intConWithByDef = Convert.ToInt32(dtCOI.Rows[0]["NumWithByDefault"].ToString());
                    intConWithout = Convert.ToInt32(dtCOI.Rows[0]["NumWithout"].ToString());
                    intConNA = Convert.ToInt32(dtCOI.Rows[0]["NumNA"].ToString());
                }
                ViewBag.lblConTotal = intConTotal;
                ViewBag.lblConWith = intConWith;
                ViewBag.lblConWithByDefault = intConWithByDef;
                ViewBag.lblConNone = intConWithout;
                ViewBag.lblConLate = intConNA;
                ViewBag.lblConLateColor = (intConNA > 0 ? "black" : "black");
                ViewBag.lblConWithColor = (intConWith > 0 ? "black" : "black");
                ViewBag.lblConWithByDefaultColor = (intConWithByDef > 0 ? "red" : "black");


                int intNotEntered = 0;
                var dtNotEnteredCOI = dba.RefreshNotEnteredCOI(EventPkey);
                if (dtNotEnteredCOI.Rows.Count > 0)
                {
                    intNotEntered = Convert.ToInt32(dtNotEnteredCOI.Rows[0]["TotalNotEntered"].ToString());

                }
                ViewBag.lblNotEntered = intNotEntered;


                // ***************  Travel and Lodging  ***************

                if (cEventAccount.dtExpectedArrival > new DateTime(2000, 1, 1))
                    ViewBag.dpAttStart = clsUtility.getStartOfDay(cEventAccount.dtExpectedArrival).ToString("yyyy-MM-dd");  //cEventAccount.dtExpectedArrival;

                if (cEventAccount.dtExpectedDeparture > new DateTime(2000, 1, 1))
                    ViewBag.dpAttEnd = clsUtility.getStartOfDay(cEventAccount.dtExpectedDeparture).ToString("yyyy-MM-dd");  //cEventAccount.dtExpectedDeparture;

                var ddtravel = dba.FillMyOptionsDropDowns(5);
                ViewBag.ddtravel = ddtravel;
                ViewBag.ddTravelSelectedValue = cEventAccount.intTravelStatus_pKey.ToString();
                ViewBag.ddLodgingSelectedValue = cEventAccount.intLodgingStatus_pKey.ToString();
                var ddLodging = dba.FillMyOptionsDropDowns(6);
                ViewBag.ddLodging = ddLodging;
                ViewBag.txtLodging = cEventAccount.strLodgingDetails;
                ViewBag.txtTravel = cEventAccount.strTravelDetails;
                ViewBag.intRegistrationLevel_pKey = cAccount.intRegistrationLevel_pKey;

                ViewBag.DockAtt_StaffPage = true;


                if (cLast.intEventType_PKey == clsEvent.EventType_CloudConference || cLast.intEventType_PKey == clsEvent.EventType_HybridConference)
                {
                    ViewBag.DockAtt_StaffPage = false;
                }

                //************** Communicator ***********
                int intComSpk = 0;
                int intComAtt = 0;
                int intComToday = 0;
                int intComDue = 0;
                int CountUser = 0;
                int CountEventUser = 0;

                var dtComm = dba.RefreshComm(EventPkey);
                if (dtComm.Rows.Count > 0)
                {
                    intComSpk = Convert.ToInt32(dtComm.Rows[0]["SpkCount"].ToString());
                    intComAtt = Convert.ToInt32(dtComm.Rows[0]["NonSpkCount"].ToString());
                    intComToday = Convert.ToInt32(dtComm.Rows[0]["ContactsToday"].ToString());
                    intComDue = Convert.ToInt32(dtComm.Rows[0]["Due"].ToString());
                }
                var dtCountUser = dba.CountUSer();
                if (dtCountUser.Rows.Count > 0)
                {
                    CountUser = Convert.ToInt32(dtCountUser.Rows[0]["OnlineUserCount"].ToString());
                    CountEventUser = Convert.ToInt32(dtCountUser.Rows[0]["OnlineEeventUserCount"].ToString());
                }

                ViewBag.lblComSpk = intComSpk;
                ViewBag.lblComAtt = intComAtt;
                ViewBag.lblComToday = intComToday;
                ViewBag.lblCommNeed = intComDue;
                ViewBag.lblCommNeedColor = (intComDue > 0 ? "red" : "black");

                ViewBag.lblCountUser = CountUser;
                ViewBag.lblCountEventUser = CountEventUser;

                cLast.PageAreaPage = "47";
                cLast.PageAreaTab = "";
            }

            return View(mystaffconsole);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public ActionResult SaveMyNote(string textnote)
        {
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            clsEventAccount cEventAccount = new clsEventAccount();
            cEventAccount.sqlConn = new SqlConnection(ReadConnectionString());
            cEventAccount.intEventAccount_pKey = data.EventAccount_pkey;
            cEventAccount.strMyNote = textnote;
            if (cEventAccount.UpdateMyNote())
            {
                return Json(new { result = "OK", msg = "Changes saved" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = "error", msg = "Unable to save right now." }, JsonRequestBehavior.AllowGet);
            }
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult RefreshRegChart()
        {
            try
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                clsLastUsed cLast = (clsLastUsed)Session["cLastUsed"];
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);

                clsEvent cEvent = new clsEvent();
                cEvent.intEvent_PKey = data.EventId;
                cEvent.sqlConn = new SqlConnection(ReadConnectionString());
                cEvent.LoadEvent();

                DateTime dtEventEnd = clsUtility.getEndOfDay(cEvent.dtEndDate);
                DateTime dtCurrent = clsEvent.getEventVenueTime();
                DateTime dtStart = clsUtility.getEndOfDay(cEvent.dtRegStartDate);
                DateTime dtEnd = ((dtEventEnd > dtCurrent) ? dtCurrent : dtEventEnd);

                DataTable dt = new SqlOperation().GetAttendeeRegLogs(dtStart, dtEnd, data.EventId);
                var jsonResult = Json(new { msg = "OK", data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch
            {

            }
            return Json(new { msg = "Error accessing registration date." }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult CumLativeReg()
        {
            try
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                clsLastUsed cLast = (clsLastUsed)Session["cLastUsed"];
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);

                clsEvent cEvent = new clsEvent();
                cEvent.intEvent_PKey = data.EventId;
                cEvent.sqlConn = new SqlConnection(ReadConnectionString());
                cEvent.LoadEvent();


                DataTable dt = new SqlOperation().GetCumulativeRegLogs(DateTime.Now.AddDays(-30), DateTime.Now, data.EventId, cEvent.intAttendanceGoal);
                var jsonResult = Json(new { msg = "OK", data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch
            {

            }
            return Json(new { msg = "Error accessing assignment date." }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult CumLativeSpeaker()
        {
            try
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                clsLastUsed cLast = (clsLastUsed)Session["cLastUsed"];
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);

                clsEvent cEvent = new clsEvent();
                cEvent.intEvent_PKey = data.EventId;
                cEvent.sqlConn = new SqlConnection(ReadConnectionString());
                cEvent.LoadEvent();
                //int intTotal = cEvent.getSpeakersNeeded(0);
                int intTotal = 0;
                if (!string.IsNullOrEmpty(cEvent.intSpeakerGoal))
                    int.TryParse(cEvent.intSpeakerGoal.Replace("+", ""), out intTotal);

                DateTime dtEventEnd = clsUtility.getEndOfDay(cEvent.dtEndDate);
                DateTime dtCurrent = clsEvent.getEventVenueTime();
                DateTime dtStart = clsUtility.getEndOfDay(cEvent.dtRegStartDate);
                DateTime dtEnd = ((dtEventEnd > dtCurrent) ? dtCurrent : dtEventEnd);

                DataTable dt = new SqlOperation().GetCumulativeSpeakerLogs(dtStart, dtEnd, data.EventId, intTotal);
                var jsonResult = Json(new { msg = "OK", data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch
            {

            }
            return Json(new { msg = "Error accessing assignment date." }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult SpeakerDinnerData()
        {
            try
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                DataTable dt = new SqlOperation().GetSpeakerDinner(data.EventId);
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow dr = dt.NewRow();
                    dr["ChargeTypeID"] = "Total";
                    dr[1] = dt.Compute("Sum(NumSignedUp)", "");
                    dr["ColorValue"] = "blue";
                    dt.Rows.Add(dr);
                }
                var jsonResult = Json(new { msg = "OK", data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch
            {

            }
            return Json(new { msg = "Error accessing speaker dinner info." }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region EventOnCloud 

        [CustomizedAuthorize]
        public ActionResult EventOnCloud()
        {
            return RedirectToAction("Index", "Home"); // Deleted Page
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? ((Request.UrlReferrer.PathAndQuery == "/MyMAGI/MySession") ? "/Home/Index" : Request.UrlReferrer.PathAndQuery) : "/Home/Index";
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            SqlConnection conn = new SqlConnection(ReadConnectionString());

            clsAccount cAccount = new clsAccount();
            cAccount.intAccount_PKey = data.Id;
            cAccount.sqlConn = conn;
            cAccount.LoadAccount();

            EventOnCloud EventOnCloudData = new EventOnCloud();
            DateTime dtCurrentTime = clsEvent.getEventVenueTime();
            if (User.Identity.AuthenticationType == "Forms")
            {
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;

                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);
                ViewBag.CurrentTime = dtCurrentTime;
            }

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = data.EventId;
            cEvent.sqlConn = conn;
            cEvent.LoadEvent();

            string str = cSettings.getText(clsSettings.TEXT_MyBook);

            ViewBag.SelectedDropDown = 0;
            string Host = HttpContext.Request.Path.ToUpper().Replace("/MYMAGI", "");
            DateTime dtCalTime = clsEvent.getCaliforniaTime();
            string intRegistrationLevel_pKey = cAccount.intRegistrationLevel_pKey.ToString();
            int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);
            ViewBag.VirtualDropdown_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
            ViewBag.leftPanel_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);

            int intRegistrationLevelpKey = 0;
            if (!string.IsNullOrEmpty(intRegistrationLevel_pKey))
                intRegistrationLevelpKey = Convert.ToInt32(intRegistrationLevel_pKey);
            EventOnCloudData.ddEventVirtualData = repository.GetVirtualEventDropDownInfo(Host, data.Id, data.EventId, dtCurrentTime, dtCalTime, data.ParentOrganization_pKey, intAttendeeStatus, intRegistrationLevelpKey, data.GlobalAdmin, data.StaffMember);
            EventOnCloudData.HelpIconInfo = repository.PageLoadResourceData(data, "", "53");
            ViewBag.SelectedDropDown = GetSelectedVirtualDropDown(Host);
            if (cAccount == null || cAccount.intAccount_PKey <= 0)
                return Redirect("/Home/Index");
            if (!(cAccount.bStaffMember || cAccount.bGlobalAdministrator))
            {
                if (!cAccount.bRegisteredAtCurrentEvent)
                    Response.Redirect("Registration");
                if (intAttendeeStatus != 1)
                    return Redirect("/Home/Index");

                if (cAccount.intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_StudentOnly || cAccount.intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_SingleSessionOnly)
                    Response.Redirect("frmSingleSession.aspx");
            }

            //Refresh Screen 
            if (!cAccount.bGlobalAdministrator && (cEvent.dtEndDate < clsEvent.getEventVenueTime() || cEvent.dtStartDate > clsEvent.getEventVenueTime()))
            {
                if (cEvent.bEventOpenEventSponsors && clsEventOrganization.CheckExhibitor(cAccount.intParentOrganization_pKey, cEvent.intEvent_PKey) && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "IsEventOpenForEventSponsors")) { }

                else if (cEvent.bEventOpenSpeakers && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "IsEventOpenForSpeaker") && cAccount.intNumTimesSpeakingCurEvent > 0) { }

                else if ((cEvent.bEventOpenStaff && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "IsEventOpenForStaff")) && (cAccount.bStaffForCurEvent || cAccount.bGlobalAdministrator || cAccount.bStaffMember)) { }
            }
            if (cEvent.bEventClosedAttendees && cAccount.bAttendeeAtCurrEvent && !(cAccount.bStaffForCurEvent || cAccount.bGlobalAdministrator || cAccount.bStaffMember))
                return Redirect("/Home/Index"); //   clsUtility.GoHome(Me.Response)

            cLast.intPendingEventChange = cEvent.intEvent_PKey;
            int intEventType_pkey = cEvent.intEventType_PKey;
            int intVenue_Pkey = cEvent.intVenue_PKey;

            ViewBag.PageTitle = cEvent.strEventFullname + ": Event Foyer";

            bool dvSponsorDirectory = true;
            bool dvExhibhitHall = false;
            if (cEvent.strPartnerAlias.ToUpper() == "EXHIBITOR")
            {
                dvSponsorDirectory = false;
                dvExhibhitHall = true;
            }
            ViewBag.dvSponsorDirectory = dvSponsorDirectory;
            ViewBag.dvExhibhitHall = dvExhibhitHall;
            ViewBag.leftPanel_Visible = ((cEvent.bShowDemoAccount && (cAccount.bGlobalAdministrator || cAccount.bIsPartner)) ? true : cEvent.bChatPanelOnOff);

            if (!(cAccount.bGlobalAdministrator || cAccount.bStaffMember) && cEvent.intEventStatus_PKey == clsEvent.STATUS_Completed)
                return Redirect("/Home/Index");      //    clsUtility.GoHome(Me.Response)
            ViewBag.lblpopText = cSettings.getText(clsSettings.Text_EventpopUpWelcome);

            bool ShowUpdateEventPopup = false;
            string lblEventUpdate = "";
            if (intEventType_pkey != clsEvent.EventType_CloudConference && intEventType_pkey != clsEvent.EventType_HybridConference)
                Response.Redirect("/Events/VenueInfo" + (Request.QueryString["EVT"] == null ? "" : "?EVT=" + Request.QueryString["EVT"].ToString()));

            if (cEvent.bMAGIUpdate && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "IsMAGIUpdate") && Request.UrlReferrer != null && Request.UrlReferrer.AbsoluteUri.ToString().Contains("login"))
            {
                lblEventUpdate = cSettings.getText(clsSettings.Text_EventUpdate);
                ShowUpdateEventPopup = true;
            }
            ViewBag.lblEventUpdate = lblEventUpdate;
            ViewBag.ShowUpdateEventPopup = ShowUpdateEventPopup;

            //**********************   Refresh page text  ************************

            string lblinstructions = "Event on cloud displaying";
            string lblContent = "";
            var dtText = dba.Refresh_EventOnCloud_PageText(data.EventId);
            if (dtText.Rows.Count > 0)
            {
                foreach (DataRow dr in dtText.Rows)
                {
                    if (dr["Indicator"].ToString() == "7")
                    {
                        lblinstructions = clsReservedWords.ReplaceCurrent(null, clsSettings.ReplaceTermsGeneral(dr["SectionText"].ToString()));
                    }
                    else
                    {
                        lblContent = clsReservedWords.ReplaceCurrent(null, clsSettings.ReplaceTermsGeneral(dr["SectionText"].ToString()));
                    }
                }
            }

            ViewBag.lblinstructions = lblinstructions;
            ViewBag.lblContent = lblContent;

            //Refresh Audio

            string hdfeventaudio = "";
            string hdfresource = "";
            string hdfNetworkingLounge = "";
            string hdfEducationCenter = "";

            var dtAudio = dba.RefreshEventInstructionAudio("4,8,11,12");
            foreach (DataRow row in dtAudio.Rows)
            {
                string AudioPath = "/Images/" + (row["AudioPath"].ToString() == null ? "" : row["AudioPath"].ToString());
                if (System.IO.File.Exists(Server.MapPath(AudioPath)))
                {
                    if (row["pKey"].ToString() == "4")
                        hdfeventaudio = AudioPath;
                    if (row["pKey"].ToString() == "8")
                        hdfresource = AudioPath;
                    if (row["pKey"].ToString() == "11")
                        hdfNetworkingLounge = AudioPath;
                    if (row["pKey"].ToString() == "12")
                        hdfEducationCenter = AudioPath;
                }
            }
            ViewBag.hdfeventaudio = hdfeventaudio;
            ViewBag.hdfresource = hdfresource;
            ViewBag.hdfNetworkingLounge = hdfNetworkingLounge;
            ViewBag.hdfEducationCenter = hdfEducationCenter;

            if (cSettings.bLogAttendeeDetails)
                cAccount.Attendee_EnterEvent(clsEvent.EnterEvent_Foyer, cAccount.intAccount_PKey, cLast.intActiveEventPkey);
            cLast.PageAreaPage = "53";
            cLast.PageAreaTab = "";

            return View(EventOnCloudData);
        }

        public ActionResult CheckInternetSpeed()
        {
            int AccountPkey = 0;
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
                AccountPkey = Convert.ToInt32(User.Identity.Name);
            clsAccount cAccount = new clsAccount();
            cAccount.intAccount_PKey = AccountPkey;
            cAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cAccount.LoadAccount();

            string lblDownloadSpeedDisplay = "";
            string lbldownloadSpeed = "";
            string lbluploadspeed = "";

            // ***************** Download Speed *****************

            double DisplaySpeedDownload = 0.0;

            ChecKBandWidthDownload(ref DisplaySpeedDownload);


            if (DisplaySpeedDownload > 768)
            {
                lblDownloadSpeedDisplay = "(Excellent)";
            }
            else if (DisplaySpeedDownload <= 768 & DisplaySpeedDownload >= 256)
            {
                lblDownloadSpeedDisplay = "(Good)";
            }
            else if (DisplaySpeedDownload < 256)
            {
                lblDownloadSpeedDisplay = "(Poor)";
            }
            double MbSpeed_Download = Convert.ToDouble(DisplaySpeedDownload / 1000);
            double GbSpeed_Download = Convert.ToDouble(MbSpeed_Download / 1000);

            if (DisplaySpeedDownload < 500)
            {
                if (cAccount.Is_Speaker)
                {
                    lbldownloadSpeed = String.Format("Download Speed: {0} KB/s", Convert.ToInt32(DisplaySpeedDownload));
                }
                else if (cAccount.bGlobalAdministrator)
                {
                    lbldownloadSpeed = String.Format("Download Speed: {0} KB/s", Convert.ToInt32(DisplaySpeedDownload));
                }
                else if (cAccount.bAttendeeAtCurrEvent)
                {
                    lbldownloadSpeed = String.Format("My Internet Connection Speed: {0} KB/s", Convert.ToInt32(DisplaySpeedDownload));
                }
            }

            if (DisplaySpeedDownload > 500 && DisplaySpeedDownload < 500000)
            {
                if (cAccount.Is_Speaker)
                {
                    lbldownloadSpeed = String.Format("Download Speed: {0:0.00} MB/s", MbSpeed_Download);
                }
                else if (cAccount.bGlobalAdministrator)
                {
                    lbldownloadSpeed = String.Format("Download Speed: {0:0.00} MB/s", MbSpeed_Download);
                }
                else if (cAccount.bAttendeeAtCurrEvent)
                {
                    lbldownloadSpeed = String.Format("My Internet Connection Speed: {0:0.00} MB/s", MbSpeed_Download);
                }
            }

            if (DisplaySpeedDownload > 500000)
            {
                if (cAccount.Is_Speaker)
                {
                    lbldownloadSpeed = String.Format("Download Speed: {0:0.00} GB/s", GbSpeed_Download);
                }
                else if (cAccount.bGlobalAdministrator)
                {
                    lbldownloadSpeed = String.Format("Download Speed: {0:0.00} GB/s", GbSpeed_Download);
                }
                else if (cAccount.bAttendeeAtCurrEvent)
                {
                    lbldownloadSpeed = String.Format("My Internet Connection Speed: {0:0.00} GB/s", GbSpeed_Download);
                }
            }

            // ***************** Upload Speed *****************

            double DisplaySpeed_Upload = 0.0;  // Convert.ToDouble(Convert.ToInt32(speeds.Average()));
            ChecKBandWidthUpload(ref DisplaySpeed_Upload);
            double MbSpeed_Upload = Convert.ToDouble(DisplaySpeed_Upload / 1000);
            double GbSpeed_Upload = Convert.ToDouble(MbSpeed_Upload / 1000);
            if (DisplaySpeed_Upload < 500)
            {
                if (cAccount.Is_Speaker)
                {
                    lbluploadspeed = String.Format("Upload Speed: {0} KB/s", Convert.ToInt32(DisplaySpeed_Upload));
                }
                else if (cAccount.bGlobalAdministrator)
                {
                    lbluploadspeed = String.Format("Upload Speed: {0} KB/s", Convert.ToInt32(DisplaySpeed_Upload));
                }
                else if (cAccount.bAttendeeAtCurrEvent)
                {
                    lbluploadspeed = String.Format("Upload Speed: {0} KB/s", Convert.ToInt32(DisplaySpeed_Upload));
                }
            }

            if (DisplaySpeed_Upload > 500 && DisplaySpeed_Upload < 500000)
            {
                if (cAccount.Is_Speaker)
                {
                    lbluploadspeed = String.Format("Upload Speed: {0:0.00} MB/s", MbSpeed_Upload);
                }
                else if (cAccount.bGlobalAdministrator)
                {
                    lbluploadspeed = String.Format("Upload Speed: {0:0.00} MB/s", MbSpeed_Upload);
                }
                else if (cAccount.bAttendeeAtCurrEvent)
                {
                    lbluploadspeed = String.Format("Upload Speed: {0:0.00} MB/s", MbSpeed_Upload);
                }
            }

            if (DisplaySpeed_Upload > 500000)
            {
                if (cAccount.Is_Speaker)
                {
                    lbluploadspeed = String.Format("Upload Speed: {0:0.00} GB/s", GbSpeed_Upload);
                }
                else if (cAccount.bGlobalAdministrator)
                {
                    lbluploadspeed = String.Format("Upload Speed: {0:0.00} GB/s", GbSpeed_Upload);
                }
                else if (cAccount.bAttendeeAtCurrEvent)
                {
                    lbluploadspeed = String.Format("Upload Speed: {0:0.00} GB/s", GbSpeed_Upload);
                }
            }
            return Json(new { msg = "OK", lblDownloadSpeedDisplay, lbldownloadSpeed, lbluploadspeed }, JsonRequestBehavior.AllowGet);

        }

        public void ChecKBandWidthDownload(ref double DisplaySpeed)
        {
            try
            {
                double[] speeds = new Double[4];

                for (int i = 0; i <= 4; i++)
                {
                    Random r = new Random();
                    string path = Server.MapPath("/TempDocuments/SampleData.xlsm");
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls; // || SecurityProtocolType.Tls11 || SecurityProtocolType.Tls12 ;   
                    WebClient client = new WebClient();

                    using (Stream file1 = System.IO.File.OpenWrite(path))
                    {
                        file1.Flush();
                        file1.Close();
                        file1.Dispose();
                        string domainName = Server.MapPath("~/TempDocuments/SampleData" + r.Next(10, 50).ToString() + ".xlsm");

                        DateTime startTime = DateTime.Now;
                        Byte[] bytes = System.IO.File.ReadAllBytes(path);
                        double size = bytes.Length / 1000;

                        client.Credentials = CredentialCache.DefaultCredentials;
                        client.DownloadFile(path, domainName);
                        DateTime endTime = DateTime.Now;
                        speeds[i] = Math.Round(size / (endTime - startTime).TotalSeconds);
                        client.Dispose();

                        if (System.IO.File.Exists(domainName))
                        {
                            System.IO.File.Delete(domainName);
                        }
                    }
                }

                DisplaySpeed = Convert.ToDouble(Convert.ToInt32(speeds.Average()));
            }
            catch (Exception ex)
            {

            }
        }

        public void ChecKBandWidthUpload(ref double DisplaySpeed)
        {
            try
            {
                double[] speeds = new Double[4];

                for (int i = 0; i <= 4; i++)
                {
                    Random r = new Random();
                    string path = Server.MapPath("/TempDocuments/SampleData.xlsm");
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; // SecurityProtocolType.Tls || SecurityProtocolType.Tls11 || 
                    WebClient client = new WebClient();

                    using (Stream file1 = System.IO.File.OpenWrite(path))
                    {
                        string domainName = Server.MapPath("~/TempDocuments/SampleData" + r.Next(10, 50) + ".xlsm");
                        file1.Flush();
                        file1.Close();
                        file1.Dispose();
                        DateTime startTime = DateTime.Now;
                        Byte[] bytes = System.IO.File.ReadAllBytes(path);
                        double size = bytes.Length / 1000;

                        client.Credentials = CredentialCache.DefaultCredentials;
                        client.UploadFile(path, domainName);
                        DateTime endTime = DateTime.Now;
                        speeds[i] = Math.Round(size / (endTime - startTime).TotalSeconds);
                        client.Dispose();

                        if (System.IO.File.Exists(domainName))
                        {
                            System.IO.File.Delete(domainName);
                        }
                    }

                }

                DisplaySpeed = Convert.ToDouble(Convert.ToInt32(speeds.Average()));
            }
            catch (Exception ex)
            {

            }
        }


        #endregion

        #region  Education Center

        [CustomizedAuthorize]
        public ActionResult EducationCenter()
        {
            return RedirectToAction("Index", "Home"); // Deleted Page
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? ((Request.UrlReferrer.PathAndQuery == "/MyMAGI/MySession") ? "/Home/Index" : Request.UrlReferrer.PathAndQuery) : "/Home/Index";
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);

            clsAccount cAccount = new clsAccount();
            cAccount.intAccount_PKey = data.Id;
            cAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cAccount.LoadAccount();


            EducationCenter EducationCenter = new EducationCenter();
            EducationCenter.HelpIconInfo = repository.PageLoadResourceData(data, "", "1");
            DateTime dtCurrentTime = clsEvent.getEventVenueTime();
            if (User.Identity.AuthenticationType == "Forms")
            {
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;

                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);
                ViewBag.CurrentTime = dtCurrentTime;
            }

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            //cEvent.LoadEvent();           

            //clsEventAccount cEventAccount = new clsEventAccount();
            //cEventAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            //cEventAccount.intEvent_pKey = EvtPKey;
            //cEventAccount.intAccount_pKey = data.Id;
            //cEventAccount.LoadEventInfo(true);   
            ViewBag.SelectedDropDown = 0;
            string Host = HttpContext.Request.Path.ToUpper().Replace("/MYMAGI", "");
            DateTime dtCalTime = clsEvent.getCaliforniaTime();
            string intRegistrationLevel_pKey = "";
            int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);

            int intRegistrationLevelpKey = 0;
            if (!string.IsNullOrEmpty(intRegistrationLevel_pKey))
                intRegistrationLevelpKey = Convert.ToInt32(intRegistrationLevel_pKey);
            EducationCenter.ddEventVirtualData = repository.GetVirtualEventDropDownInfo(Host, data.Id, data.EventId, dtCurrentTime, dtCalTime, data.ParentOrganization_pKey, intAttendeeStatus, intRegistrationLevelpKey, data.GlobalAdmin, data.StaffMember);
            ViewBag.SelectedDropDown = GetSelectedVirtualDropDown(Host);

            cLast.PageAreaPage = "1";
            cLast.PageAreaTab = "";

            //Refresh Screen


            if (!cEvent.LoadEvent())
            {
                // clsUtility.GoHome(Me.Response)
            }
            ViewBag.PageTitle = cEvent.strEventFullname + ": Educational Center";
            if (cSettings.bLogAttendeeDetails)
            {
                cAccount.Attendee_EnterEvent(clsEvent.EnterEvent_EducationCenter, cAccount.intAccount_PKey, cEvent.intEvent_PKey);
            }
            ViewBag.leftPanel_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
            //ViewBag.pLeftChatPanel  = cEvent.bChatPanelOnOff;
            ViewBag.VirtualDropdown_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
            string lblContent = "";
            var dtText = dba.Refresh_EducationCenter_PageText(cEvent.intEvent_PKey);
            if (dtText.Rows.Count > 0)
            {
                lblContent = clsReservedWords.ReplaceCurrent(null, clsSettings.ReplaceTermsGeneral(dtText.Rows[0]["SectionText"].ToString()));
            }
            ViewBag.lblContent = lblContent;

            //Refresh Audio 

            string hdfCreateSchedule = "";
            string hdfViewSchedule = "";
            string hdfProgram = "";
            var dtAudio = dba.RefreshEventInstructionAudio("1,2,3");
            foreach (DataRow row in dtAudio.Rows)
            {
                string AudioPath = "/Images/" + (row["AudioPath"].ToString() == null ? "" : row["AudioPath"].ToString());
                if (System.IO.File.Exists(Server.MapPath(AudioPath)))
                {
                    if (row["pKey"].ToString() == "1")
                    {
                        hdfCreateSchedule = AudioPath;
                    }
                    if (row["pKey"].ToString() == "2")
                    {
                        hdfViewSchedule = AudioPath;
                    }
                    if (row["pKey"].ToString() == "3")
                    {
                        hdfProgram = AudioPath;
                    }
                }

            }

            ViewBag.hdfCreateSchedule = hdfCreateSchedule;
            ViewBag.hdfViewSchedule = hdfViewSchedule;
            ViewBag.hdfProgram = hdfProgram;

            //var dtSpeakers = dba.RefreshSpeakers_EducationCenter(cLast.intActiveEventPkey);


            return View(EducationCenter);
        }

        public ActionResult ViewVideo_EducationCenter()
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            var dt = dba.ViewVideo_EducationCenter(EvtPKey);
            return Json(new { msg = "OK", Source = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewSpeaker_EducationCenter()
        {
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            var dt = dba.RefreshSpeakers_EducationCenter(cLast.intActiveEventPkey);
            return Json(new { msg = "OK", Source = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region NetworkingLounge

        [CustomizedAuthorize]
        public ActionResult NetworkingLounge()
        {
            return RedirectToAction("Index", "Home"); // Deleted Page
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? ((Request.UrlReferrer.PathAndQuery == "/MyMAGI/MySession") ? "/Home/Index" : Request.UrlReferrer.PathAndQuery) : "/Home/Index";
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);

            clsAccount cAccount = new clsAccount();
            cAccount.intAccount_PKey = data.Id;
            cAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cAccount.LoadAccount();


            if (cAccount == null || cAccount.intAccount_PKey == 0)
            {
                Response.Redirect("/Home/Login?LastPage_URL=" + Path.GetFileName(Request.Url.AbsoluteUri));
            }

            Event_PagesModel NetworkingLounge = new Event_PagesModel();
            DateTime dtCurrentTime = clsEvent.getEventVenueTime();
            if (User.Identity.AuthenticationType == "Forms")
            {
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;

                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);
                ViewBag.CurrentTime = dtCurrentTime;
            }
            ViewBag.SelectedDropDown = 0;
            string Host = HttpContext.Request.Path.ToUpper().Replace("/MYMAGI", "");
            DateTime dtCalTime = clsEvent.getCaliforniaTime();
            string intRegistrationLevel_pKey = "";
            int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);
            ViewBag.leftPanel_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
            ViewBag.VirtualDropdown_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
            int intRegistrationLevelpKey = 0;
            if (!string.IsNullOrEmpty(intRegistrationLevel_pKey))
                intRegistrationLevelpKey = Convert.ToInt32(intRegistrationLevel_pKey);
            NetworkingLounge.ddEventVirtualData = repository.GetVirtualEventDropDownInfo(Host, data.Id, data.EventId, dtCurrentTime, dtCalTime, data.ParentOrganization_pKey, intAttendeeStatus, intRegistrationLevelpKey, data.GlobalAdmin, data.StaffMember);
            ViewBag.SelectedDropDown = GetSelectedVirtualDropDown(Host);
            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());


            //clsUtility.BlueRibbonCheck(Response);

            cLast.PageAreaPage = "3";
            cLast.PageAreaTab = "";

            //****************** Refresh Screen ************************
            if (!cEvent.LoadEvent())
            {
                // clsUtility.GoHome(Me.Response)
            }
            ViewBag.PageTitle = cEvent.strEventFullname + ": Networking Lounge";
            if (cSettings.bLogAttendeeDetails)
            {
                cAccount.Attendee_EnterEvent(clsEvent.EnterEvent_EducationCenter, cAccount.intAccount_PKey, cEvent.intEvent_PKey);
            }

            //****************** Refresh Text ************************

            string lblContent = "";
            var dtText = dba.RefreshText_NetworkingLounge(cEvent.intEvent_PKey);
            if (dtText.Rows.Count > 0)
            {
                lblContent = clsReservedWords.ReplaceCurrent(null, clsSettings.ReplaceTermsGeneral(dtText.Rows[0]["SectionText"].ToString()));
            }
            ViewBag.lblContent = lblContent;

            //****************** Refresh Audio ************************

            string hdfConnection = "";
            string hdfCommunity = "";
            string hdfNetwork = "";
            var dtAudio = dba.RefreshEventInstructionAudio("5,6,7");
            if (dtAudio.Rows.Count > 0)
            {
                hdfConnection = "/Images/" + dtAudio.Rows[0]["AudioPath"].ToString();
                hdfCommunity = "/Images/" + dtAudio.Rows[1]["AudioPath"].ToString();
                hdfNetwork = "/Images/" + dtAudio.Rows[2]["AudioPath"].ToString();
            }
            ViewBag.hdfConnection = hdfConnection;
            ViewBag.hdfCommunity = hdfCommunity;
            ViewBag.hdfNetwork = hdfNetwork;
            NetworkingLounge.HelpIconInfo = repository.PageLoadResourceData(data, "", "3");
            if (cSettings.bLogAttendeeDetails)
                cAccount.Attendee_EnterEvent(clsEvent.EnterEvent_NetworkingLounge, cAccount.intAccount_PKey, cLast.intActiveEventPkey);


            return View(NetworkingLounge);
        }


        #endregion

        #region ScheduledEvent 
        [CustomizedAuthorize]
        public ActionResult ScheduledEvent()
        {
            return RedirectToAction("Index", "Home"); //Page Deleted
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();

            SqlConnection conn = new SqlConnection(ReadConnectionString());
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? ((Request.UrlReferrer.PathAndQuery == "/MyMAGI/MySession") ? "/Home/Index" : Request.UrlReferrer.PathAndQuery) : "/Home/Index";
            FormsIdentity identity = (FormsIdentity)User.Identity;

            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            ((clsFormList)Session["cFormlist"]).LoadPage(conn, null, System.Web.HttpContext.Current.Request, "Scheduled Event", "", Request.QueryString);

            Event_PagesModel ScheduledEvent = new Event_PagesModel();
            DateTime dtCurrentTime = clsEvent.getEventVenueTime();
            ViewBag.ID = data.Id;
            ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
            ViewBag.EventPKey = data.EventId;
            ViewBag.EventAccountPKey = data.EventAccount_pkey;
            ViewBag.EventTypeID = data.EventTypeId;
            ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);
            ViewBag.CurrentTime = dtCurrentTime;

            int intExhibitor_pKey = 0;
            if (Request.QueryString["EBPK"]!= null && Request.QueryString["EBPK"]!= "")
                intExhibitor_pKey = Convert.ToInt32(Request.QueryString["EBPK"]);
            if (intExhibitor_pKey<=0)
                intExhibitor_pKey=  new SqlOperation().FindEvent_OrgMVC(data.Id, data.EventId);

            clsAccount cAccount = new clsAccount();
            cAccount.intAccount_PKey = data.Id;
            cAccount.sqlConn = conn;
            cAccount.LoadAccount();

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.sqlConn = conn;
            cEvent.LoadEvent();

            ViewBag.PageTitle = cEvent.strEventFullname + ": Main Schedule of Events";
            ViewBag.strPartnerAlias = cEvent.strPartnerAlias;
            ViewBag.strSkipRegDate = cEvent.strSkipRegDate;
            ViewBag.dtCurEventStart = cEvent.dtStartDate;
            ViewBag.dtCurEventEnd = cEvent.dtEndDate.AddDays(1).Date;
            ViewBag.SelectedDropDown = 0;

            string Host = HttpContext.Request.Path.ToUpper().Replace("/MYMAGI", "");
            DateTime dtCalTime = clsEvent.getCaliforniaTime();
            string intRegistrationLevel_pKey = "";
            int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);
            ViewBag.leftPanel_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
            ViewBag.VirtualDropdown_Visible = ViewBag.leftPanel_Visible;
            int intRegistrationLevelpKey = 0;
            if (!string.IsNullOrEmpty(intRegistrationLevel_pKey))
                intRegistrationLevelpKey = Convert.ToInt32(intRegistrationLevel_pKey);
            ScheduledEvent.ddEventVirtualData = repository.GetVirtualEventDropDownInfo(Host, data.Id, data.EventId, dtCurrentTime, dtCalTime, data.ParentOrganization_pKey, intAttendeeStatus, intRegistrationLevelpKey, data.GlobalAdmin, data.StaffMember);
            ScheduledEvent.HelpIconInfo = repository.PageLoadResourceData(data, "", "7");
            ViewBag.SelectedDropDown = GetSelectedVirtualDropDown(Host);
            ViewBag.pLeftChatPanel = ((cEvent.bShowDemoAccount && (cAccount.bGlobalAdministrator || cAccount.bIsPartner)) ? true : cEvent.bChatPanelOnOff);

            //If Not Request.QueryString("EVT") Is Nothing Then myVS.intEventPKey = Val(Request.QueryString("EVT"))
            //If myVS.intEventPKey <= 0 Then myVS.intEventPKey = Me.cLast.intActiveEventPkey

            //Me.RefreshScheduledBooth(True)

            ViewBag.lblStaffedTitle = "Staffed Schedule";
            ViewBag.divStaffedSchedule = false;
            if (cEvent.strPartnerAlias.ToUpper() == "EXHIBITOR")
            {
                ViewBag.lblStaffedTitle = "Sponsor Booths Open & Staffed";
                ViewBag.divStaffedSchedule = false;
            }

            List<SelectListItem> ddDate = new List<SelectListItem>();
            ddDate.Add(new SelectListItem { Text = "All Days", Value = "0" });
            DateTime d = cEvent.dtStartDate;
            while (d < cEvent.dtEndDate.AddDays(1).Date)
            {
                if (cEvent.strSkipRegDate.Contains(d.ToString("yyyy-MM-dd")))
                    d = d.AddDays(1);
                else
                {
                    ddDate.Add(new SelectListItem { Text = d.ToLongDateString(), Value = d.ToShortDateString() });
                    d = d.AddDays(1);
                }
            }
            var dateSelect = clsUtility.getStartOfDay(clsEvent.getEventVenueTime()).ToShortDateString();
            d = cEvent.dtStartDate;
            if (ddDate  == null && ddDate.Count==1)
                ViewBag.ddDate_SelectedValue =0;
            else
            {
                while (d<cEvent.dtEndDate)
                    if (cEvent.strSkipRegDate.Contains(d.ToString("yyyy-MM-dd")))
                        d = d.AddDays(1);
                    else
                    {
                        if (d >= Convert.ToDateTime(dateSelect))
                        {
                            dateSelect = d.ToShortDateString();
                            break;
                        }
                        d = d.AddDays(1);
                    }
            }
            ViewBag.ddDate = ddDate;
            ViewBag.ddDate_SelectedValue = dateSelect;

            if (cSettings.bLogAttendeeDetails)
                cAccount.Attendee_EnterEvent(clsEvent.EnterEvent_EventScheduled, data.Id, data.EventId);


            return View(ScheduledEvent);
        }

        [AjaxValidateAntiForgeryToken]
        [CustomizedAuthorize]
        public ActionResult RefreshScheduledBoothTable(string dateSelected)
        {
            try
            {
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new User_Login();
                SqlConnection conn = new SqlConnection(ReadConnectionString());
                int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
                if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
                    data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                clsEvent cEvent = new clsEvent();
                cEvent.intEvent_PKey = EvtPKey;
                cEvent.sqlConn = conn;
                cEvent.LoadEvent();

                int intExhibitor_pKey = dba.FindEvent_Org(data.Id, data.EventId);
                string dateSelect = clsUtility.getStartOfDay(clsEvent.getEventVenueTime()).ToShortDateString();
                var dt = new SqlOperation().RefreshScheduledBooth(intExhibitor_pKey, data.Id, data.EventId, dateSelected, cEvent.dtStartDate, cEvent.dtEndDate.AddDays(1).Date);
                var JsonResult = Json(new { msg = "OK", Source = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
                JsonResult.MaxJsonLength= int.MaxValue;
                return JsonResult;
            }
            catch
            {

            }
            return Json(new { msg = "Error While Fetching Scheduled Events" }, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        [CustomizedAuthorize]
        public string CmdScheduleEvent(int ESPkey, string SessionSC)
        {
            string result = "OK";
            SqlConnection conn = new SqlConnection(ReadConnectionString());
            int id = Convert.ToInt32(User.Identity.Name);
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(((FormsIdentity)User.Identity).Ticket.UserData);

            clsEventSession c = new clsEventSession();
            c.lblMsg = null;
            c.sqlConn = conn;
            c.intEventSession_PKey = ESPkey;
            c.intEvent_PKey = data.EventId;
            if (SessionSC.ToUpper() == "SCHEDULE")
            {
                if (!c.SetAttend(data.Id, true, true, false, 0, data.EventId, AuditLog: true))
                    result="Error";
            }
            else if (SessionSC.ToUpper() == "UNSCHEDULE")
            {
                if (!c.SetAttend(data.Id, false, false, false, 0, data.EventId, AuditLog: true))
                    result="Error";

            }
            return result;
        }

        [ValidateInput(true)]
        [CustomizedAuthorize]
        public FileResult DownloadScheduleEventFile(ScheduleEventFile Model)
        {
            try
            {
                SqlConnection conn = new SqlConnection(ReadConnectionString());
                int id = Convert.ToInt32(User.Identity.Name);
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(((FormsIdentity)User.Identity).Ticket.UserData);
                DateTime DateTimeStart = new DateTime(), DateTimeEnd = new DateTime(), dtCurrentTime;

                clsEvent cEvent = new clsEvent();
                cEvent.intEvent_PKey = data.EventId;
                cEvent.sqlConn = conn;
                cEvent.LoadEvent();
                dtCurrentTime = clsEvent.getEventVenueTime();
                string strTimeZone = clsEvent.strStandardTimezone;
                string ShortString = Model.title.Split(' ')[0].Trim('(').Trim(')').Trim();
                new SqlOperation().GetScheduledEventData(data.EventId, Model.StartDate, Model.EndTime, ref DateTimeStart, ref DateTimeEnd);

                string FileName = (ShortString.Trim() + " Calendar File").Replace(" ", "%20") + ".ics";
                string EventTitle = Model.title, Summary = Model.title;
                string Description = ((string.IsNullOrEmpty(Model.Desc)) ? Model.title : Model.Desc);
                StringBuilder icalStringbuilder = new StringBuilder();
                icalStringbuilder.AppendLine("BEGIN:VCALENDAR");
                icalStringbuilder.AppendLine("PRODID:-//" + EventTitle + "//EN");
                icalStringbuilder.AppendLine("VERSION:2.0");
                icalStringbuilder.AppendLine("METHOD:PUBLISH");
                icalStringbuilder.AppendLine("BEGIN:VTIMEZONE");
                icalStringbuilder.AppendLine("TZID:GMT");
                icalStringbuilder.AppendLine("END:VTIMEZONE");
                icalStringbuilder.AppendLine("BEGIN:VEVENT");
                icalStringbuilder.AppendLine("SUMMARY;LANGUAGE=en-us:" + Summary);
                icalStringbuilder.AppendLine("CLASS:PUBLIC");
                icalStringbuilder.AppendLine(String.Format("CREATED:{0:yyyyMMddTHHmmssZ}", DateTime.UtcNow));
                icalStringbuilder.AppendLine("DESCRIPTION:" + Description);
                icalStringbuilder.AppendLine(String.Format("DTSTART;TZID=GMT:{0:yyyyMMddTHHmmssZ}", DateTimeStart));
                icalStringbuilder.AppendLine(String.Format("DTEND;TZID=GMT:{0:yyyyMMddTHHmmssZ}", DateTimeEnd));
                icalStringbuilder.AppendLine("SEQUENCE:0");
                icalStringbuilder.AppendLine("UID:" + Guid.NewGuid().ToString());
                icalStringbuilder.AppendLine("END:VEVENT");
                icalStringbuilder.AppendLine("END:VCALENDAR");


                byte[] bytes = Encoding.UTF8.GetBytes(icalStringbuilder.ToString());
                return File(bytes, "text/x-vCalendar", FileName);
            }
            catch
            {

            }
            return null;
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult RefreshDetailsDescription(int RKey, int MKey, int Stype, int Id, string Type, string Description)
        {
            try
            {
                DataTable dt = null;
                string Host = "";
                if (MKey > 0)
                    dt=  new SqlOperation().GetScheduleEventDetails(1, Meeting_pkey: MKey);
                else
                {
                    Host = Type.ToString();
                    if (Stype !=5)
                    {
                        if (Id>0)
                            dt=  new SqlOperation().GetScheduleEventDetails(2, ES_pKey: Id);
                        else
                            dt=  new SqlOperation().GetScheduleEventDetails(3, RPKey: RKey);
                    }
                }
                var jsonResult = Json(new { msg = "OK", Source = JsonConvert.SerializeObject(dt), TypeHost = Host }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength=int.MaxValue;
                return jsonResult;
            }
            catch
            {

            }
            return Json(new { msg = "Error Occurred While Refreshing Details Description" }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult RefreshJoinData(int SType, int MUrlKey, int MKey, int RKey, string strGroupChatLink)
        {
            SqlConnection conn = new SqlConnection(ReadConnectionString());
            User_Login data = new User_Login();
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(((FormsIdentity)User.Identity).Ticket.UserData);

            bool result = false, newTab = false;

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = data.EventId;
            cEvent.sqlConn = conn;
            cEvent.LoadEvent();

            DateTime dtCurrentTime = clsEvent.getEventVenueTime();

            if (SType == 5)
            {
                if (strGroupChatLink.Contains("LeaveChatChatChat"))
                    result = new SqlOperation().AddToMyScheduleEvent(data.Id, data.EventId, RKey, dtCurrentTime, "MYPEOPLE", true);
                else if (strGroupChatLink == "LeaveMingle")
                    result = new SqlOperation().AddToMyScheduleEvent(data.Id, data.EventId, RKey, dtCurrentTime, "MINGLE", true);
                else if (strGroupChatLink == "JoinMingle")
                    result = new SqlOperation().AddToMyScheduleEvent(data.Id, data.EventId, RKey, dtCurrentTime, "MINGLE", false);
                else if (strGroupChatLink == "JoinChatChatChat")
                    result = new SqlOperation().AddToMyScheduleEvent(data.Id, data.EventId, RKey, dtCurrentTime, "MYPEOPLE", false);
                else
                    Response.Redirect(strGroupChatLink, false);

                if (result)
                    return Json(new { msg = "OK", RefreshScheduledBooth = result, New = newTab }, JsonRequestBehavior.AllowGet);
            }
            if (MKey>0)
            {
                new SqlOperation().ScheduleEventJoinMeeting(MKey, MUrlKey, data.Id);
                newTab =true;
            }
            return Json(new { msg = "OK", RefreshScheduledBooth = result, New = newTab }, JsonRequestBehavior.AllowGet);

        }

        #endregion ScheduledEvent

        #region  ResourceSupportCenter

        [CustomizedAuthorize]
        public ActionResult ResourceSupportCenter()
        {
            return RedirectToAction("Index", "Home"); // Deleted Page
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? ((Request.UrlReferrer.PathAndQuery == "/MyMAGI/MySession") ? "/Home/Index" : Request.UrlReferrer.PathAndQuery) : "/Home/Index";
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);

            clsAccount cAccount = new clsAccount();
            cAccount.intAccount_PKey = data.Id;
            cAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cAccount.LoadAccount();


            if (cAccount == null || cAccount.intAccount_PKey == 0)
            {
                Response.Redirect("/Home/Login?LastPage_URL=" + Path.GetFileName(Request.Url.AbsoluteUri));
            }

            Event_PagesModel ResourceSupportCenter = new Event_PagesModel();
            DateTime dtCurrentTime = clsEvent.getEventVenueTime();
            if (User.Identity.AuthenticationType == "Forms")
            {
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;

                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);
                ViewBag.CurrentTime = dtCurrentTime;
            }
            ViewBag.SelectedDropDown = 0;
            string Host = HttpContext.Request.Path.ToUpper().Replace("/MYMAGI", "");
            DateTime dtCalTime = clsEvent.getCaliforniaTime();
            string intRegistrationLevel_pKey = "";
            int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);
            ViewBag.leftPanel_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
            ViewBag.VirtualDropdown_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
            int intRegistrationLevelpKey = 0;
            if (!string.IsNullOrEmpty(intRegistrationLevel_pKey))
                intRegistrationLevelpKey = Convert.ToInt32(intRegistrationLevel_pKey);
            ResourceSupportCenter.ddEventVirtualData = repository.GetVirtualEventDropDownInfo(Host, data.Id, data.EventId, dtCurrentTime, dtCalTime, data.ParentOrganization_pKey, intAttendeeStatus, intRegistrationLevelpKey, data.GlobalAdmin, data.StaffMember);
            ViewBag.SelectedDropDown = GetSelectedVirtualDropDown(Host);
            //clsUtility.BlueRibbonCheck(Response)


            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());

            if (!cEvent.LoadEvent())
            {
                // clsUtility.GoHome(Me.Response);
            }
            ViewBag.PageTitle = cEvent.strEventFullname + ": Resource & Support Center";
            //ViewBag.leftPanel_Visible = cEvent.bChatPanelOnOff;// true;//

            ViewBag.dvShowNewsDiv_Visible = (cEvent.intShowNews > 0); //true;// 

            //clsUtility.HideSidePanel(Me.lblTitle.Text, Me.Page, False, hImage:= True)
            if ((cAccount.bGlobalAdministrator || cAccount.bStaffMember) && cEvent.intEventStatus_PKey == clsEvent.STATUS_Completed)
            {
                //  clsUtility.GoHome(Me.Response)
            }

            //***************Refresh Text *******************

            string lblContent = "";
            var dtText = dba.RefreshText_ResourceSupportCenter(cEvent.intEvent_PKey);
            if (dtText.Rows.Count > 0)
            {
                lblContent = clsReservedWords.ReplaceCurrent(null, clsSettings.ReplaceTermsGeneral(dtText.Rows[0]["SectionText"].ToString()));
            }
            ViewBag.lblContent = lblContent;

            //****************** Refresh Audio ************************

            string hdfResourcesAudio = "";
            string hdFHelpDeskAudio = "";
            string hdfScheduleEventAudio = "";
            var dtAudio = dba.RefreshEventInstructionAudio("8,9,10");
            foreach (DataRow row in dtAudio.Rows)
            {
                string AudioPath = "/Images/" + (row["AudioPath"].ToString() == null ? "" : row["AudioPath"].ToString());
                if (System.IO.File.Exists(Server.MapPath(AudioPath)))
                {
                    if (row["pKey"].ToString() == "8")
                    {
                        hdfResourcesAudio = AudioPath;
                    }
                    if (row["pKey"].ToString() == "9")
                    {
                        hdFHelpDeskAudio = AudioPath;
                    }
                    if (row["pKey"].ToString() == "10")
                    {
                        hdfScheduleEventAudio = AudioPath;
                    }
                }
            }
            ViewBag.hdfResourcesAudio = hdfResourcesAudio;
            ViewBag.hdFHelpDeskAudio = hdFHelpDeskAudio;
            ViewBag.hdfScheduleEventAudio = hdfScheduleEventAudio;
            ResourceSupportCenter.HelpIconInfo = repository.PageLoadResourceData(data, "", "4");
            cLast.PageAreaPage = "4";
            cLast.PageAreaTab = "";

            if (cSettings.bLogAttendeeDetails)
            {
                cAccount.Attendee_EnterEvent(clsEvent.EnterEvent_ResourceSupportCenter, cAccount.intAccount_PKey, cLast.intActiveEventPkey);
            }

            return View(ResourceSupportCenter);
        }

        [CustomizedAuthorize]
        public ActionResult RefreshDocumentsDownload(string txtStringTitle)
        {
            int intAccount_PKey = 0;
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
                intAccount_PKey = Convert.ToInt32(User.Identity.Name);

            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);

            clsAccount cAccount = new clsAccount();
            cAccount.intAccount_PKey = intAccount_PKey;
            cAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cAccount.LoadAccount();

            var dtDoc = dba.RefreshDocuments(cAccount.bGlobalAdministrator, EvtPKey, intAccount_PKey, txtStringTitle);

            return Json(new { msg = "OK", Source = JsonConvert.SerializeObject(dtDoc) }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ShowNews
        private void PreviewGenerator(clsEvent cEvent, User_Login data, int EvtPKey, SqlConnection conn)
        {
            // --use active event
            // --get account, use self unless an event account was selected
            int intAcctPKey = data.Id;
            int intEvtPKey = EvtPKey;
            int intEvtAcctPKey = 0, intEvtSesPKey = 0;
            string strBaseTitle = "", strBaseBody = "", strReplace3 = "";

            clsAnnouncement cAnnouncement = new clsAnnouncement();
            cAnnouncement.intEvent_pkey =EvtPKey;
            cAnnouncement.sqlConn = conn;
            cAnnouncement.intAnnouncement_PKey = cEvent.intShowNews;
            cAnnouncement.LoadAnnouncement();
            strBaseTitle = cAnnouncement.strTitle;
            strBaseBody = cAnnouncement.strHTMLText;
            intEvtSesPKey = cAnnouncement.IntForActivity;

            clsReservedWords.ReplaceBase(null, intEvtPKey, cAnnouncement.strTitle, ref strBaseTitle, cAnnouncement.strHTMLText, ref strBaseBody, "", ref strReplace3);
            clsEventAccount cEventAccount = new clsEventAccount();
            {
                var withBlock = cEventAccount;
                withBlock.sqlConn = conn;
                withBlock.intAccount_pKey = intAcctPKey;
                withBlock.intEvent_pKey = intEvtPKey;
                withBlock.LoadEventInfo(true);
                intEvtAcctPKey = withBlock.intEventAccount_pKey;
            }
            // --get account info
            clsAccount cWorkAccount = new clsAccount();
            {
                var withBlock = cWorkAccount;
                withBlock.sqlConn = conn;
                withBlock.intAccount_PKey = intAcctPKey;
                withBlock.LoadAccount();
            }
            clsEventSession cEventSession = new clsEventSession();
            if (intEvtSesPKey > 0)
            {
                {
                    var withBlock = cEventSession;
                    withBlock.sqlConn = conn;
                    withBlock.intEventSession_PKey = intEvtSesPKey;
                    withBlock.LoadEventSession();
                }
            }
            if (strBaseBody.Contains("[Page_DownloadCRCPCertificate]") | strBaseBody.Contains("[AcctCRCPExpiration]"))
            {
                string qry = "select t1.pKey as AcctExamPKey, t3.CRCPExpirationDate, t1.ExamDate,t1.ExamExpDate, t1.Account_pKey, t3.Firstname,t3.Lastname, t3.email, t7.ExamName, t7.ExamAbbrev";
                qry = qry + Environment.NewLine + ",t1.ResultsSentDate, isNull(t1.ExamStatus_pKey,0) as Status_pKey,t1.ExamStatus_pKey,isNull(t3.ExamRenewalEmailSent,0) as bSent";
                qry = qry + Environment.NewLine + ",isNull(t5.ExamStatusID,'Pending') as ExamStatus,(Case when t1.ExamStatus_pKey in(" + clsExam.EXAMSTATUS_PassGroup + ") and getdate() > t3.CRCPExpirationDate Then 1 else 0 end) as IsExpired";
                qry = qry + Environment.NewLine + ",isnull((select max(examdate) from Account_ExamResults where exam_pkey = t1.exam_pkey and ExamStatus_pKey in (1,6) ";
                qry = qry + Environment.NewLine + " and account_pkey=t3.pkey),t1.ExamDate) as LatestExamDate";
                qry = qry + Environment.NewLine + " From Account_ExamResults t1";
                qry = qry + Environment.NewLine + " Inner join Account_List t3 on t3.pkey = t1.Account_pKey";
                qry = qry + Environment.NewLine + "Left outer join sys_ExamStatuses t5 On t5.pkey = t1.ExamStatus_pKey";
                qry = qry + Environment.NewLine + " Left outer join Exam_List t7 on t7.pkey = t1.Exam_pKey";
                qry = qry + Environment.NewLine + " Where t1.ExamStatus_pKey is not null and t1.ExamExpDate is not null and t1.ExamDate = (Select max(examdate) from Account_ExamResults where account_pkey = t1.Account_pKey And exam_pkey =1)";
                qry = qry + Environment.NewLine + " and t3.pkey=" + intAcctPKey.ToString();
                SqlCommand cmd = new SqlCommand(qry);
                DataTable dt = new DataTable();
                if (clsUtility.GetDataTable(conn, cmd, ref dt))
                {
                    if ((dt.Rows.Count > 0))
                    {
                        strBaseTitle = clsExam.ReplaceExamWords(dt.Rows[0], strBaseTitle);
                        strBaseBody = clsExam.ReplaceExamWords(dt.Rows[0], strBaseBody);
                    }
                }
            }
            if (strBaseBody.Contains("[TaskDetails]") || strBaseBody.Contains("[CLICK_HERE_(Tasks)]"))
            {
                clsTask cTask = new clsTask();
                strBaseBody = cTask.ReplacedReservedWords(null, strBaseBody, data.Id, EvtPKey);
            }

            // -- Title
            string strTitleToSend = cWorkAccount.ReplaceReservedWords(strBaseTitle);
            if (intEvtAcctPKey > 0)
                strTitleToSend = cEventAccount.ReplaceReservedWords(strTitleToSend);
            // -- Body Content
            string strbodyToSend = cWorkAccount.ReplaceReservedWords(strBaseBody);
            if (intEvtAcctPKey > 0)
                strbodyToSend = cEventAccount.ReplaceReservedWords(strbodyToSend);
            string strRef = string.Empty;
            clsReservedWords.ReplaceConditionals(ref strTitleToSend, ref strbodyToSend, ref strRef);

            ViewBag.PageTitle = strTitleToSend;
            ViewBag.lblContent = strbodyToSend;
        }
        [CustomizedAuthorize]
        public ActionResult ShowNews()
        {
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? ((Request.UrlReferrer.PathAndQuery == "/MyMAGI/MySession") ? "/Home/Index" : Request.UrlReferrer.PathAndQuery) : "/Home/Index";
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            int EvtPKey = data.EventId;
            if (Request.QueryString["EVT"] != null)
                int.TryParse(Request.QueryString["EVT"], out EvtPKey);
            if (EvtPKey <= 0)
                EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            SqlConnection conn = new SqlConnection(ReadConnectionString());
            if (User.Identity.AuthenticationType == "Forms")
            {
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;

                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);
            }

            clsEvent cEvent = new clsEvent();
            cEvent.sqlConn = conn;
            cEvent.intEvent_PKey = EvtPKey;
            if (!cEvent.LoadEvent())
                return RedirectToAction("Index", "Home");
            ViewBag.bShowEventPages = (cEvent.bShowEvtPages && cEvent.CheckValiditityOfModule(EvtPKey, "ShowEventPages"));
            ViewBag.intEventType_pkey = cEvent.intEventType_PKey;

            cLast.intEventType_PKey = cEvent.intEventType_PKey;
            string intRegistrationLevel_pKey = "";
            DateTime dtCurrentTime = clsEvent.getEventVenueTime();
            int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, cEvent.intEvent_PKey, ref intRegistrationLevel_pKey);
            bool bAttendeeAtCurrEvent = false;
            DataTable AccountSettings = repository.getMenuAccountSettings(data.EventId, data.Id);
            if (AccountSettings != null && AccountSettings.Rows.Count > 0)
                bAttendeeAtCurrEvent = (AccountSettings.Rows[0]["AttendeeAtCurrEvent"] != DBNull.Value) ? false : Convert.ToBoolean(AccountSettings.Rows[0]["AttendeeAtCurrEvent"].ToString());

            string Host = HttpContext.Request.Path.ToUpper().Replace("/MYMAGI", "");
            ViewBag.VirtualDropdown_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
            ViewBag.SelectedDropDown = 0;

            int intRegistrationLevelpKey = 0;
            if (!string.IsNullOrEmpty(intRegistrationLevel_pKey))
                intRegistrationLevelpKey = Convert.ToInt32(intRegistrationLevel_pKey);
            DateTime dtCalTime = clsEvent.getCaliforniaTime();
            ViewBag.ddEventVirtualData = repository.GetVirtualEventDropDownInfo(Host, data.Id, data.EventId, dtCurrentTime, dtCalTime, data.ParentOrganization_pKey, intAttendeeStatus, intRegistrationLevelpKey, data.GlobalAdmin, data.StaffMember);
            ViewBag.SelectedDropDown = GetSelectedVirtualDropDown(Host);


            bool bAttendee = (intAttendeeStatus == 1) || bAttendeeAtCurrEvent || data.GlobalAdmin || data.StaffMember;
            if (!bAttendee)
                return RedirectToAction("Index", "Home");
            if (cEvent.intShowNews > 0)
                PreviewGenerator(cEvent, data, EvtPKey, conn);
            else
            {
                ViewBag.PageTitle = "MAGI News";
                ViewBag.lblContent = "";
            }

            return View();
        }
        #endregion ShowNews

        #region SingleSession 
        public ActionResult SingleSession()
        {
            return View();
        }
        #endregion SingleSesion

    }
}
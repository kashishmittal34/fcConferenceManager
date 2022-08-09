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
    public class VirtualController : Controller
    {
        // GET: Virtual
        public ActionResult Index()
        {
            return View();
        }
        DBAccessLayer dba = new DBAccessLayer();
        static SqlOperation repository = new SqlOperation();
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

        #region EventOnCloud 

        [CustomizedAuthorize]
        public ActionResult EventOnCloud()
        {
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
            string Host = HttpContext.Request.Path.ToUpper().Replace("/VIRTUAL", "");
            DateTime dtCalTime = clsEvent.getCaliforniaTime();
            string intRegistrationLevel_pKey = cAccount.intRegistrationLevel_pKey.ToString();
            int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);
            ViewBag.VirtualDropdown_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);

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
            ViewBag.leftPanel_Visible =  ViewBag.VirtualDropdown_Visible;

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

            bool bShowSurveyQuestion = false;
            DataTable EventFeatures = repository.getDyamicEventSettings(data.EventId, "ISNULL(ShowSurveyQuestion,'0') as ShowSurveyQuestion");
            if (EventFeatures != null && EventFeatures.Rows.Count > 0)
                bShowSurveyQuestion = (EventFeatures.Rows[0]["ShowSurveyQuestion"] != System.DBNull.Value) ? Convert.ToBoolean(EventFeatures.Rows[0]["ShowSurveyQuestion"]) : false;

            int selectedEvent = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).intEventSelector;
            bool bSponsor = clsEventOrganization.CheckExhibitor(data.ParentOrganization_pKey, selectedEvent);
            ViewBag.lblRegText = "";
            ViewBag.OpenSurveyRadWindow=false;
            LoadRegistrationQuestions(data, intAttendeeStatus, bShowSurveyQuestion, bSponsor, intRegistrationLevel_pKey);

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
            string Host = HttpContext.Request.Path.ToUpper().Replace("/VIRTUAL", "");
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
            //ViewBag.pLeftChatPanel  = cEvent.bChatPanelOnOff;
            ViewBag.VirtualDropdown_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
            ViewBag.leftPanel_Visible = ViewBag.VirtualDropdown_Visible;
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
            bool bShowSurveyQuestion = false;
            DataTable EventFeatures = repository.getDyamicEventSettings(data.EventId, "ISNULL(ShowSurveyQuestion,'0') as ShowSurveyQuestion");
            if (EventFeatures != null && EventFeatures.Rows.Count > 0)
                bShowSurveyQuestion = (EventFeatures.Rows[0]["ShowSurveyQuestion"] != System.DBNull.Value) ? Convert.ToBoolean(EventFeatures.Rows[0]["ShowSurveyQuestion"]) : false;

            int selectedEvent = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).intEventSelector;
            bool bSponsor = clsEventOrganization.CheckExhibitor(data.ParentOrganization_pKey, selectedEvent);
            ViewBag.lblRegText = "";
            ViewBag.OpenSurveyRadWindow=false;
            LoadRegistrationQuestions(data, intAttendeeStatus, bShowSurveyQuestion, bSponsor, intRegistrationLevel_pKey);
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
            string Host = HttpContext.Request.Path.ToUpper().Replace("/VIRTUAL", "");
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
            ViewBag.leftPanel_Visible = cEvent.bChatPanelOnOff;

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

            if (cSettings.bLogAttendeeDetails)
                cAccount.Attendee_EnterEvent(clsEvent.EnterEvent_NetworkingLounge, cAccount.intAccount_PKey, cLast.intActiveEventPkey);

            bool bShowSurveyQuestion = false;
            DataTable EventFeatures = repository.getDyamicEventSettings(data.EventId, "ISNULL(ShowSurveyQuestion,'0') as ShowSurveyQuestion");
            if (EventFeatures != null && EventFeatures.Rows.Count > 0)
                bShowSurveyQuestion = (EventFeatures.Rows[0]["ShowSurveyQuestion"] != System.DBNull.Value) ? Convert.ToBoolean(EventFeatures.Rows[0]["ShowSurveyQuestion"]) : false;

            int selectedEvent = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).intEventSelector;
            bool bSponsor = clsEventOrganization.CheckExhibitor(data.ParentOrganization_pKey, selectedEvent);
            ViewBag.lblRegText = "";
            ViewBag.OpenSurveyRadWindow=false;
            LoadRegistrationQuestions(data, intAttendeeStatus, bShowSurveyQuestion, bSponsor, intRegistrationLevel_pKey);

            return View(NetworkingLounge);
        }


        #endregion

        #region ScheduledEvent 
        [CustomizedAuthorize]
        public ActionResult ScheduledEvent()
        {
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

            string Host = HttpContext.Request.Path.ToUpper().Replace("/VIRTUAL", "");
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

            bool bShowSurveyQuestion = false;
            DataTable EventFeatures = repository.getDyamicEventSettings(data.EventId, "ISNULL(ShowSurveyQuestion,'0') as ShowSurveyQuestion");
            if (EventFeatures != null && EventFeatures.Rows.Count > 0)
                bShowSurveyQuestion = (EventFeatures.Rows[0]["ShowSurveyQuestion"] != System.DBNull.Value) ? Convert.ToBoolean(EventFeatures.Rows[0]["ShowSurveyQuestion"]) : false;

            int selectedEvent = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).intEventSelector;
            bool bSponsor = clsEventOrganization.CheckExhibitor(data.ParentOrganization_pKey, selectedEvent);
            ViewBag.lblRegText = "";
            ViewBag.OpenSurveyRadWindow=false;
            LoadRegistrationQuestions(data, intAttendeeStatus, bShowSurveyQuestion, bSponsor, intRegistrationLevel_pKey);

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
                Model.Desc = System.Net.WebUtility.UrlDecode(Model.Desc);
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

        #endregion

        #region  ResourceSupportCenter

        [CustomizedAuthorize]
        public ActionResult ResourceSupportCenter()
        {
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
            string Host = HttpContext.Request.Path.ToUpper().Replace("/VIRTUAL", "");
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

            cLast.PageAreaPage = "4";
            cLast.PageAreaTab = "";

            if (cSettings.bLogAttendeeDetails)
            {
                cAccount.Attendee_EnterEvent(clsEvent.EnterEvent_ResourceSupportCenter, cAccount.intAccount_PKey, cLast.intActiveEventPkey);
            }
            bool bShowSurveyQuestion = false;
            DataTable EventFeatures = repository.getDyamicEventSettings(data.EventId, "ISNULL(ShowSurveyQuestion,'0') as ShowSurveyQuestion");
            if (EventFeatures != null && EventFeatures.Rows.Count > 0)
                bShowSurveyQuestion = (EventFeatures.Rows[0]["ShowSurveyQuestion"] != System.DBNull.Value) ? Convert.ToBoolean(EventFeatures.Rows[0]["ShowSurveyQuestion"]) : false;

            int selectedEvent = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).intEventSelector;
            bool bSponsor = clsEventOrganization.CheckExhibitor(data.ParentOrganization_pKey, selectedEvent);
            ViewBag.lblRegText = "";
            ViewBag.OpenSurveyRadWindow=false;
            LoadRegistrationQuestions(data, intAttendeeStatus, bShowSurveyQuestion, bSponsor, intRegistrationLevel_pKey);

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
            ViewBag.bShowEventPages = cEvent.bShowEvtPages;
            ViewBag.intEventType_pkey = cEvent.intEventType_PKey;
            ViewBag.PageTitle = cEvent.strEventFullname + " Venue & Lodging";
            cLast.intEventType_PKey = cEvent.intEventType_PKey;
            string intRegistrationLevel_pKey = "";
            DateTime dtCurrentTime = clsEvent.getEventVenueTime();
            int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, cEvent.intEvent_PKey, ref intRegistrationLevel_pKey);
            bool bAttendeeAtCurrEvent = false;
            DataTable AccountSettings = repository.getMenuAccountSettings(data.EventId, data.Id);
            if (AccountSettings != null && AccountSettings.Rows.Count > 0)
                bAttendeeAtCurrEvent = (AccountSettings.Rows[0]["AttendeeAtCurrEvent"] != DBNull.Value) ? false : Convert.ToBoolean(AccountSettings.Rows[0]["AttendeeAtCurrEvent"].ToString());

            string Host = HttpContext.Request.Path.ToUpper().Replace("/VIRTUAL", "");
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
            bool bShowSurveyQuestion = false;
            DataTable EventFeatures = repository.getDyamicEventSettings(data.EventId, "ISNULL(ShowSurveyQuestion,'0') as ShowSurveyQuestion");
            if (EventFeatures != null && EventFeatures.Rows.Count > 0)
                bShowSurveyQuestion = (EventFeatures.Rows[0]["ShowSurveyQuestion"]!= System.DBNull.Value) ? Convert.ToBoolean(EventFeatures.Rows[0]["ShowSurveyQuestion"]) : false;

            int selectedEvent = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).intEventSelector;
            bool bSponsor = clsEventOrganization.CheckExhibitor(data.ParentOrganization_pKey, selectedEvent);
            ViewBag.lblRegText = "";
            ViewBag.OpenSurveyRadWindow=false;
            LoadRegistrationQuestions(data, intAttendeeStatus, bShowSurveyQuestion, bSponsor, intRegistrationLevel_pKey);
            return View();
        }
        #endregion ShowNews

        #region VirtualSession
        private void LoadIntroVideoPlay(int IntroPlayKey)
        {
            string introURL = ""; int Width = 500, Height = 300;
            string URLstring = "", Type = "";
            DataTable dt = repository.LoadSessionIntroVideo(IntroPlayKey);
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    introURL = (dt.Rows[0]["VideoURL"] == System.DBNull.Value) ? "" : dt.Rows[0]["VideoURL"].ToString();
                    if (string.IsNullOrEmpty(introURL))
                        return;
                    if (!clsUtility.FileExists(Server.MapPath(introURL)))
                        return;
                    introURL = introURL.Replace("~", "");
                    DataTable Dimension = repository.LoadDimension();
                    if (Dimension != null)
                    {
                        if (Dimension.Rows.Count > 0)
                        {
                            foreach (DataRow dr in Dimension.Rows)
                            {
                                if (dr["Dimension"] != System.DBNull.Value)
                                {
                                    switch (dr["Dimension"].ToString().ToLower())
                                    {
                                        case "width":
                                            Width = ((dr["Size"] == System.DBNull.Value) ? 0 : Convert.ToInt32(dr["Size"].ToString()));
                                            break;
                                        case "height":
                                            Height = ((dr["Size"] == System.DBNull.Value) ? 0 : Convert.ToInt32(dr["Size"].ToString()));
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    string ExtensionString = Path.GetExtension(introURL);
                    switch (ExtensionString)
                    {
                        case ".pdf":
                            Type = "iframe";
                            URLstring = ConfigurationManager.AppSettings["AppURL"].ToString().Replace("/forms", "") + introURL;
                            URLstring = URLstring + ((HttpContext.Request.Browser.Id == "chrome") ? "#page=1&view=fitH" : "#zoom=100");
                            break;
                        case ".gif":
                            Type = "gif";
                            Bitmap bmImg = new Bitmap(Server.MapPath(introURL));
                            int iWidth = bmImg.Width, iHeight = bmImg.Height;
                            bmImg.Dispose();
                            Width = ((iWidth < 800) ? (iWidth + 50) : 800);
                            Height = ((iHeight < 500) ? (iHeight + 70) : 550);
                            URLstring = introURL;
                            break;
                        default:
                            Type = "video";
                            URLstring = introURL;
                            Width = Width - 40;
                            Height = Height + 10;
                            break;
                    }
                }
            }
            ViewData["Type"] = Type;
            ViewData["introURL"] = URLstring;
            ViewData["Width"] = Width;
            ViewData["Height"] = Height;
        }
        private void LoadVirtualSessionData(DataTable EventSesssionData, User_Login data, DateTime dtCurrentTime, clsSettings cSettings, bool bSpeaker, int intRegistrationLevel_pKey, string strRegionCode)
        {
            bool bEventSessionData = false, bWebinar = false, bWebinarWait = false, bPolling = false, PollingEnabled = false, bAttendeeQuestionLink = false, imgCopyWebinarHostkey = false, imgCopyMeetingHostkey = false,
                 ShowResult = false, IsLiveStream = false, bBreakOut = false, HallwayEnable = false, bWebinarStarted = false, bMeetingStarted = false, PollingLinkStyleEnabled = false, HallwayActive = false;
            string AttendacneLog = "", RedirectURL = "", ZoomWebinarUrl = "", ZoomMeetingUrl = "", WebinarPwd = "", HallwayPwd = "", Hallwaylink = "", TPInfo = "",
                   SessionID = "", WebinarHostKey = "", HallwayHostKey = "", LiveStreaamURL = "";
            int IntroPlayKey = 0;
            Double intStartMinut = 0, intStartMinutUTC = 0, intMaxMemberInBreakout = 0;
            ViewData["strSessionID"] = "";
            int introPlaykey = 0, intSessionPKey = 0;
            if (EventSesssionData != null && EventSesssionData.Rows.Count > 0)
            {
                bEventSessionData = true;
                IsLiveStream = ((EventSesssionData.Rows[0]["IsLiveStream"] != System.DBNull.Value) ? Convert.ToBoolean(EventSesssionData.Rows[0]["IsLiveStream"]) : false);
                intSessionPKey = ((EventSesssionData.Rows[0]["Session_pKey"] != System.DBNull.Value) ? Convert.ToInt32(EventSesssionData.Rows[0]["Session_pKey"]) : 0);
                int AttendeeLogCount = ((EventSesssionData.Rows[0]["AttendeeLogCount"] != System.DBNull.Value) ? Convert.ToInt32(EventSesssionData.Rows[0]["AttendeeLogCount"]) : 0);
                AttendacneLog = "Attendance: " + AttendeeLogCount + " of " + ViewData["TotalAttendee"] + " scheduled";
                DateTime StartTime = ((EventSesssionData.Rows[0]["StartTime"] != System.DBNull.Value) ? Convert.ToDateTime(EventSesssionData.Rows[0]["StartTime"].ToString()) : new DateTime());
                DateTime EndTime = ((EventSesssionData.Rows[0]["StartTime"] != System.DBNull.Value) ? Convert.ToDateTime(EventSesssionData.Rows[0]["StartTime"].ToString()) : new DateTime());
                intStartMinut = dtCurrentTime.Subtract(StartTime).TotalMinutes;
                intStartMinutUTC = (DateTime.UtcNow.AddHours(-4)).Subtract(StartTime).TotalMinutes;
                bWebinar = (ViewBag.IsStaff || (bSpeaker && ((intStartMinut <= cSettings.intWebinarLinkShowSpeakerBefor || intStartMinutUTC <= cSettings.intWebinarLinkShowSpeakerBefor) && dtCurrentTime <= Convert.ToDateTime(EndTime))) || ((intStartMinut <= cSettings.intWebinarLinkShowBefor || intStartMinutUTC <= cSettings.intWebinarLinkShowBefor) && dtCurrentTime <= Convert.ToDateTime(EndTime)));
                bWebinarWait = (dtCurrentTime <= Convert.ToDateTime(EndTime) && dtCurrentTime.Date == Convert.ToDateTime(EndTime).Date);
                if (!bWebinar && !bWebinarWait)
                    RedirectURL = ((intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_SingleSessionOnly) ? "/SingleSession" : "/MyMagi/MySchedule");

                int minutesbefore = 0, minutesAfter = 0;
                minutesbefore = ((bSpeaker) ? cSettings.intWebinarLinkShowSpeakerBefor : cSettings.intWebinarLinkShowBefor);
                minutesAfter  = cSettings.intWebinarLinkShowAfter;
                string stringAlertMessage = "", strType = "";
                bool bHallWay = false, Showbefore = false, ShowAfter = false;
                ViewBag.bWaitPopup = true;
                ViewBag.WaitPopUpString = "";
                if (Request.QueryString["hd"] != null)
                {
                    minutesbefore = minutesbefore + 30;
                    minutesAfter = cSettings.intWebinarLinkShowAfter + 30;
                    bHallWay = (Request.QueryString["hd"] == "1") ? (ViewBag.IsStaff || (bSpeaker && ((intStartMinut <= cSettings.intWebinarLinkShowSpeakerBefor || intStartMinutUTC <= cSettings.intWebinarLinkShowSpeakerBefor) && dtCurrentTime <= Convert.ToDateTime(EndTime).AddMinutes(cSettings.intWebinarLinkShowAfter + 30))) || ((intStartMinut <= cSettings.intWebinarLinkShowBefor || intStartMinutUTC <= cSettings.intWebinarLinkShowBefor) && dtCurrentTime <= Convert.ToDateTime(EndTime).AddMinutes(cSettings.intWebinarLinkShowAfter + 30))) : false;
                }

                if (!bWebinar && !bHallWay)
                {
                    clsLastUsed cLast = (clsLastUsed)Session["cLastUsed"];
                    Showbefore = ((intStartMinut >= minutesbefore || intStartMinutUTC >= minutesbefore)) && dtCurrentTime<StartTime.AddMinutes(-minutesbefore);
                    ShowAfter = dtCurrentTime > StartTime && ((dtCurrentTime > Convert.ToDateTime(EndTime))  || (dtCurrentTime > Convert.ToDateTime(EndTime).AddMinutes(minutesAfter)));
                    if (Showbefore && !ShowAfter)
                    {
                        strType = "1";
                        stringAlertMessage = "Session " + SessionID + " starts on " + StartTime.ToString("dddd") + " at " + StartTime.ToString("h:mm tt") + " " + strRegionCode + ". Please return on " + StartTime.ToString("dddd") + " after " + StartTime.AddMinutes(-minutesbefore).ToString("h:mm tt") + " " + strRegionCode + ".";
                    }
                    if (ShowAfter)
                    {
                        strType = "2";
                        stringAlertMessage = "Session " + SessionID + " ended at " + EndTime.ToString("h:mm tt") + " " + strRegionCode + " on Monday";
                    }
                    ViewBag.bWaitPopup = false;
                    ViewBag.WaitPopUpString = stringAlertMessage;
                }
                ViewBag.StrType =strType;

                intMaxMemberInBreakout = ((EventSesssionData.Rows[0]["MaxMemberInBreakout"] != System.DBNull.Value) ? Convert.ToInt32(EventSesssionData.Rows[0]["MaxMemberInBreakout"]) : 0);
                SessionID = ((EventSesssionData.Rows[0]["SessionID"] != System.DBNull.Value) ? Convert.ToString(EventSesssionData.Rows[0]["SessionID"]) : "");
                bAttendeeQuestionLink = ((EventSesssionData.Rows[0]["pQueCount"] != System.DBNull.Value) ? ((Convert.ToInt32(EventSesssionData.Rows[0]["pQueCount"]) > 0) ? true : false) : false);
                bPolling = ((EventSesssionData.Rows[0]["PollingCount"] != System.DBNull.Value) ? ((Convert.ToInt32(EventSesssionData.Rows[0]["PollingCount"]) > 0 && data.GlobalAdmin) ? true : false) : false);
                bBreakOut = ((EventSesssionData.Rows[0]["IsBreakOut"] != System.DBNull.Value) ? Convert.ToBoolean(EventSesssionData.Rows[0]["IsBreakOut"]) : false);
                PollingEnabled = ((EventSesssionData.Rows[0]["PollingEnabled"] != System.DBNull.Value) ? (Convert.ToInt32(EventSesssionData.Rows[0]["PollingEnabled"].ToString()) > 0) : false);
                ShowResult = ((EventSesssionData.Rows[0]["ShowResult"] != System.DBNull.Value) ? ((Convert.ToInt32(EventSesssionData.Rows[0]["ShowResult"].ToString()) > 0) ? true : false) : false);

                ZoomWebinarUrl = (EventSesssionData.Rows[0]["ZoomWebinarURL"] == System.DBNull.Value) ? "" : EventSesssionData.Rows[0]["ZoomWebinarURL"].ToString();
                ZoomMeetingUrl = (EventSesssionData.Rows[0]["ZoomMeetingURL"] == System.DBNull.Value) ? "" : EventSesssionData.Rows[0]["ZoomMeetingURL"].ToString();
                HallwayEnable = ((EventSesssionData.Rows[0]["ShowResult"] != System.DBNull.Value) ? Convert.ToBoolean(EventSesssionData.Rows[0]["HallwayActive"].ToString()) : false);
                bWebinarStarted = ((EventSesssionData.Rows[0]["WebinarStarted"] != System.DBNull.Value) ? Convert.ToBoolean(EventSesssionData.Rows[0]["WebinarStarted"].ToString()) : false);
                bMeetingStarted = ((EventSesssionData.Rows[0]["MeetingStarted"] != System.DBNull.Value) ? Convert.ToBoolean(EventSesssionData.Rows[0]["MeetingStarted"].ToString()) : false);
                WebinarPwd = ((EventSesssionData.Rows[0]["WebinarPwd"] != System.DBNull.Value) ? EventSesssionData.Rows[0]["WebinarPwd"].ToString() : "");
                HallwayPwd = ((EventSesssionData.Rows[0]["HallwayPwd"] != System.DBNull.Value) ? EventSesssionData.Rows[0]["HallwayPwd"].ToString() : "");
                introPlaykey = ((EventSesssionData.Rows[0]["SessionIntro_pkey"] != System.DBNull.Value) ? Convert.ToInt32(EventSesssionData.Rows[0]["SessionIntro_pkey"]) : 0);

                string HallWayURL = ((EventSesssionData.Rows[0]["HallwayURL"] != System.DBNull.Value) ? EventSesssionData.Rows[0]["HallwayURL"].ToString() : "");
                Hallwaylink = $"<a Class='underline-On-hover' data-url='{HallWayURL}' title='Hallway discussion' style='text-decoration:none;font-size: 12pt; text-align:center;color: navy;cursor:pointer;'>Hallway discussion</a>";

                PollingLinkStyleEnabled = (PollingEnabled || ShowResult);

                ShowResult = ((EventSesssionData.Rows[0]["PollingResultCount"] != System.DBNull.Value) ? ((Convert.ToInt32(EventSesssionData.Rows[0]["PollingResultCount"].ToString()) > 0) ? true : false) : false);
                int tp = ((EventSesssionData.Rows[0]["TP_pKey"] != System.DBNull.Value) ? Convert.ToInt32(EventSesssionData.Rows[0]["TP_pKey"].ToString()) : 0);
                if (tp > 0)
                {
                    TPInfo = $"<a id='tp_info' Class='underline-On-hover' title='Technical producer information' style='text-decoration:none;font-size: 12pt; text-align:center;color: navy;cursor:pointer;'>Technical producer information</a>";
                    if (data.Id == tp || (data.GlobalAdmin || data.StaffMember))
                    {
                        imgCopyWebinarHostkey = true;
                        imgCopyMeetingHostkey = true;
                    }
                }
                IntroPlayKey = ((EventSesssionData.Rows[0]["SessionIntro_pkey"] != System.DBNull.Value) ? Convert.ToInt32(EventSesssionData.Rows[0]["SessionIntro_pkey"].ToString()) : 0);
                WebinarHostKey = ((EventSesssionData.Rows[0]["WebinarHostKey"] != System.DBNull.Value) ? EventSesssionData.Rows[0]["WebinarHostKey"].ToString() : "");
                HallwayHostKey = ((EventSesssionData.Rows[0]["HallwayHostKey"] != System.DBNull.Value) ? EventSesssionData.Rows[0]["HallwayHostKey"].ToString() : "");
                HallwayActive = ((EventSesssionData.Rows[0]["HallwayActive"] != System.DBNull.Value) ? Convert.ToBoolean(EventSesssionData.Rows[0]["HallwayActive"].ToString()) : false);
            }
            if (IsLiveStream)
            {
                LiveStreaamURL = ZoomWebinarUrl;
                ZoomWebinarUrl = "";
            }

            ViewData["imgCopyWebinarHostkey"] = imgCopyWebinarHostkey && !IsLiveStream;
            ViewData["imgCopyMeetingHostkey"] = imgCopyMeetingHostkey;
            ViewData["AttendeeQuestionLink"] = bAttendeeQuestionLink;
            ViewData["strSessionID"] = SessionID;
            ViewData["intMaxMemberInBreakout"] = intMaxMemberInBreakout;
            ViewData["intSession_pKey"] = intSessionPKey;
            ViewData["TPlnk"] = TPInfo;
            ViewData["imgCopyHostkey"] =
            ViewData["IsBreakOut"] = bBreakOut;
            ViewData["SpeakerPollingLink"] = bPolling;
            ViewData["PollingLink"] = bPolling;
            ViewData["AttendacneLog"] = AttendacneLog;
            ViewData["EventSessionData"] = bEventSessionData;
            ViewData["RedirectURL"] = RedirectURL;
            ViewData["HallwayLink"] = Hallwaylink;
            ViewData["PollingLinkStyleEnabled"] = PollingLinkStyleEnabled;
            ViewData["imgCopyWebinarHostkey"] = imgCopyWebinarHostkey;
            ViewData["imgCopyMeetingHostkey"] = imgCopyMeetingHostkey;
            ViewData["PageMeetingName"] = data.FirstName + " " + data.LastName;
            ViewData["aWebinarURL"] = ZoomWebinarUrl;
            ViewData["aMeetingURL"] = ZoomMeetingUrl;
            ViewData["WebinarHostKey"] = WebinarHostKey;
            ViewData["WebinarURLVisible"] = (ZoomWebinarUrl != "") && !IsLiveStream;
            ViewData["MeetingVisible"] = (ZoomMeetingUrl != "");
            ViewData["AttWebinarURLVisible"] = (ZoomWebinarUrl != "") && !IsLiveStream;
            ViewData["AttHallwayURLVisible"] = (ZoomMeetingUrl != "") && !IsLiveStream;
            ViewData["WebinarPwd"] = WebinarPwd;
            ViewData["HallwayPwd"] = HallwayPwd;
            ViewData["WebinarStarted"] = bWebinarStarted && !IsLiveStream;
            ViewData["bMeetingStarted"] = bMeetingStarted;
            ViewData["HallwayHostKey"] = HallwayHostKey;
            ViewData["IsLiveStream"] = IsLiveStream;
            ViewData["IsLiveStreamSrc"] = LiveStreaamURL;
            
            RefreshHallwayDiscussionMeeting(data, bSpeaker, bWebinarStarted, bMeetingStarted, ZoomWebinarUrl, ZoomMeetingUrl, WebinarHostKey, HallwayActive);
            if (IntroPlayKey > 0)
                LoadIntroVideoPlay(IntroPlayKey);
            LoadZoomWebinar(data, bSpeaker, bWebinarStarted, bMeetingStarted, ZoomWebinarUrl, ZoomMeetingUrl, WebinarHostKey, HallwayHostKey, HallwayActive, IsLiveStream);
        }
        private void RefreshHallwayDiscussionMeeting(User_Login data, bool bSpeaker, bool bWebinarStarted, bool bMeetingStarted, string ZoomWebinarUrl, string ZoomMeetingUrl, string WebinarHostKey, bool HallwayActive)
        {
            bool IsMobileDevice = clsUtility.CheckIsMobileDevice();
            ViewData["lblHallwayURL"] = ZoomMeetingUrl;
            ViewData["ckEnableHallwaylink"] = HallwayActive;
            ViewData["hdnHallwayEnable"] = HallwayActive;
            ViewData["lnlHallway"] = ZoomMeetingUrl;

            string strZoomNumber = "", strZoomPwd = "", strZoomPassword = "", strWebinar = "", strTK = "", strTime = "", strBrowser = "chrome", strMeetingUrl = "";
            Uri strUri;
            if (!string.IsNullOrEmpty(ZoomMeetingUrl.Trim()) && ZoomMeetingUrl.Contains("https:"))
            {
                strUri = new Uri(ZoomMeetingUrl);
                strZoomNumber = strUri.AbsolutePath.Replace("/j/", "").Replace("/w/", "");
                if (ZoomMeetingUrl.Contains("pwd="))
                    strZoomPassword = HttpUtility.ParseQueryString(strUri.Query).Get("pwd");

                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
                strTime = Convert.ToInt64(ts.TotalMilliseconds).ToString();
                strMeetingUrl = string.Format("zoommtg://zoom.us/join?action=join&confno={0}&uname={1}&pwd={2}&email={3}&browser={4}&t={5}", strZoomNumber, data.FirstName + " " + data.LastName, strZoomPassword, data.Email, strBrowser, strTime);
                if (IsMobileDevice)
                    strMeetingUrl = strMeetingUrl.Replace("zoommtg://", "zoomus://");
            }
            if (!string.IsNullOrEmpty(ZoomWebinarUrl.Trim()) && ZoomWebinarUrl.Contains("https:"))
            {
                strUri = new Uri(ZoomWebinarUrl);
                strZoomNumber = strUri.AbsolutePath.Replace("/j/", "").Replace("/w/", "");
                if (ZoomWebinarUrl.Contains("pwd="))
                    strZoomPassword = HttpUtility.ParseQueryString(strUri.Query).Get("pwd");

                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
                strTime = Convert.ToInt64(ts.TotalMilliseconds).ToString();
                strWebinar = string.Format("zoommtg://zoom.us/join?action=join&confno={0}&uname={1}&pwd={2}&email={3}&browser={4}&t={5}", strZoomNumber, data.FirstName + " " + data.LastName, strZoomPassword, data.Email, strBrowser, strTime);
                strTK = HttpUtility.ParseQueryString(strUri.Query).Get("tk");
                if (!string.IsNullOrEmpty(strTK))
                    strWebinar = string.Format("zoommtg://zoom.us/join?action=join&confno={0}&uname={1}&pwd={2}&email={3}&browser={4}&t={5}&tk={6}", strZoomNumber, data.FirstName + " " + data.LastName, strZoomPassword, data.Email, strBrowser, strTime, strTK);

                if (IsMobileDevice)
                    strWebinar = strWebinar.Replace("zoommtg://", "zoomus://");
            }
            ViewData["PageMeetingTK"] = strTK;
            ViewData["aWebinarURL"] = strWebinar;
            ViewData["aMeetingURL"] = strMeetingUrl;
        }
        private void LoadZoomWebinar(User_Login data, bool bSpeaker, bool bWebinarStarted, bool bMeetingStarted, string ZoomWebinarUrl, string ZoomMeetingUrl, string WebinarHostKey, string HallwayHostKey, bool HallwayActive,bool IsLiveStream=false)
        {
            ViewData["PageMeetingKey"] = "ZBjTPERWR_SXqUjcWz0pUQ";
            ViewData["PageMeetingSecret"] = "wdLscpA3MuUgDx14PDC1t1P1uZHET4b7JSWO";
            ViewData["PageMeetingRole"] = "0";
            bool ImgPHVisible = false, MyFrameVisible = false;
            string Link = "", strZoomNumber = "", strZoomPwd = "", strZoomPassword = "", strWebinar = "";
            Uri strUri;
            if ((ViewBag.IsStaff || bSpeaker) && ViewBag.intVSEventUserType == 0)
            {
                MyFrameVisible = false;
                ImgPHVisible = true;
            }
            else
            {
                if (bWebinarStarted || bMeetingStarted)
                {
                    MyFrameVisible = true;
                    ImgPHVisible = false;
                    Link = "/MyZoom/Meeting.html";
                    if (bWebinarStarted)
                    {
                        if (!string.IsNullOrEmpty(ZoomWebinarUrl.Trim()) && ZoomWebinarUrl.Contains("https:"))
                        {
                            strUri = new Uri(ZoomWebinarUrl);
                            strZoomNumber = strUri.AbsolutePath.Replace("/j/", "").Replace("/w/", "");
                            if (ZoomWebinarUrl.Contains("pwd="))
                                strZoomPassword = HttpUtility.ParseQueryString(strUri.Query).Get("pwd");

                            strZoomPwd = ViewData["WebinarPwd"].ToString();
                        }
                        ViewData["PageMeetingEmail"] = data.Email;
                        ViewData["PageMeetingNumber"] = strZoomNumber;
                        ViewData["PageMeetingPassword"] = strZoomPwd;
                        ViewData["PageMeetingWebinar"] = "0";
                    }
                    else
                    {
                        if (bMeetingStarted)
                        {
                            if (!string.IsNullOrEmpty(ZoomMeetingUrl.Trim()) && ZoomMeetingUrl.Contains("https:"))
                            {
                                strUri = new Uri(ZoomMeetingUrl);
                                strZoomNumber = strUri.AbsolutePath.Replace("/j/", "").Replace("/w/", "");
                                if (ZoomMeetingUrl.Contains("pwd="))
                                    strZoomPassword = HttpUtility.ParseQueryString(strUri.Query).Get("pwd");

                                strZoomPwd = ViewData["HallwayPwd"].ToString();
                            }
                            ViewData["PageMeetingEmail"] = data.Email;
                            ViewData["PageMeetingNumber"] = strZoomNumber;
                            ViewData["PageMeetingPassword"] = strZoomPwd;
                            ViewData["PageMeetingWebinar"] = "1";
                        }
                    }
                }
            }

            if (IsLiveStream)
                ImgPHVisible =false;

            ViewData["ImgPHVisible"] = ImgPHVisible;
            ViewData["MyFrameVisible"] = MyFrameVisible;
            ViewData["MyFrameSrc"] = Link;
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult GetGridSpeakerBreakout(int ESPK, string GroupChatID, string S_ID)
        {
            DataTable data = repository.GetSpeakerBreakOutData(ESPK, GroupChatID, 1);
            return Json(new { msg = "OK", Source = JsonConvert.SerializeObject(data) }, JsonRequestBehavior.AllowGet);
        }
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public string UpdateFilesAttendee()
        {
            string UserFileName = "";
            try
            {
                int ESPK = 0, intUploadType = 0;
                FormsIdentity identity = (FormsIdentity)User.Identity;
                var fileContent = Request.Files[0];
                var size = fileContent.ContentLength;
                double getFileSize = fileContent.ContentLength / 1000;

                string Type = "", strPhysicalPath = "", strDirectory = "";
                UserFileName = Path.GetFileNameWithoutExtension(fileContent.FileName);
                string Extension = Path.GetExtension(fileContent.FileName);
                string strFileName = string.Format(UserFileName + "_" + ESPK.ToString() + "_" + identity.Name + "_{0: yyMMdd_HH.mm.ss}", DateTime.Now) + Extension;

                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                Type = ((string.IsNullOrEmpty(Request.Params["Type"])) ? "" : Request.Params["Type"]);
                ESPK = ((string.IsNullOrEmpty(Request.Params["ESPK"])) ? 0 : Convert.ToInt32(Request.Params["ESPK"]));
                if (Type == "0")
                {
                    intUploadType = 1;
                    strDirectory = "SpeakerDocuments/" + data.EventId;
                }
                else
                {
                    intUploadType = 2;
                    strDirectory = "AttendeeDocuments";
                }
                strPhysicalPath = Server.MapPath(Path.Combine("~/", strDirectory, strFileName));
                fileContent.SaveAs(strPhysicalPath);
                clsEventSession c = new clsEventSession();
                c.UploadAttendeeDocumentToSession(ESPK, data.Id, strFileName, UserFileName, intUploadType);
            }
            catch
            {
                System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;
                clsUtility.LogErrorMessage(null, request, this.GetType().Name, 0, "Error saving upload file: " + UserFileName);
                return "Error While Uploading File";
            }
            return "OK";
        }
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public string UpdateFilesSpeaker()
        {
            string strCheckFileName = "", Type = "", strPhysicalPath = "", strDirectory = "";
            try
            {
                int ESPK = 0, intUploadType = 0, intRev = 0;
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                var fileContent = Request.Files[0];
                var size = fileContent.ContentLength;
                double getFileSize = fileContent.ContentLength / 1000;

                string BaseFileName = Path.GetFileNameWithoutExtension(fileContent.FileName), Extension = Path.GetExtension(fileContent.FileName);
                string strFileName = string.Format(BaseFileName + "_" + ESPK.ToString() + "_" + identity.Name + "_{0: yyMMdd_HH.mm.ss}", DateTime.Now) + Extension;
                strCheckFileName = BaseFileName + Extension;
                strPhysicalPath = Server.MapPath("~/SpeakerDocuments/" + data.EventId + "/");
                double dblFSize = ((getFileSize == 0) ? clsUtility.getFileSize(strPhysicalPath + strCheckFileName, clsUtility.FileSize_KB) : getFileSize);
                while (clsUtility.FileExists(strPhysicalPath + strCheckFileName))
                {
                    intRev = intRev + 1;
                    strCheckFileName = BaseFileName + "_" + intRev.ToString() + Extension;
                }

                Type = ((string.IsNullOrEmpty(Request.Params["Type"])) ? "" : Request.Params["Type"]);
                ESPK = ((string.IsNullOrEmpty(Request.Params["ESPK"])) ? 0 : Convert.ToInt32(Request.Params["ESPK"]));
                if (Type == "0")
                {
                    intUploadType = 1;

                }
                else
                {
                    intUploadType = 2;
                }
                strPhysicalPath = strPhysicalPath + strCheckFileName;
                fileContent.SaveAs(strPhysicalPath);
                clsEventSession c = new clsEventSession();
                c.UploadSpeakerDocumentToSession(ESPK, clsEventSession.DOCTYPE_Download, data.Id, strCheckFileName, BaseFileName, intRev, clsEventSession.DOCTYPE_Download, dblFSize, clsEventSession.DOCSTATUS_New, "Uploaded new file", 0, "");
            }
            catch
            {
                System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;
                clsUtility.LogErrorMessage(null, request, this.GetType().Name, 0, "Error saving upload file: " + strCheckFileName);
                return "Error While Uploading File";
            }
            return "OK";
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public string UpdateSpeakerBreakoutLeader(int ID, bool CKValue)
        {
            return repository.UpdateSpeakerBreakoutLeader(ID, CKValue);
        }
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult RefreshLinks(int ESPK, string GroupChatID, string S_ID)
        {
            bool SpkrGroupChatVisible = false;
            string HostKey = "", SpkrGroupChatLink = "";
            DataTable RefreshLinksData = repository.GetSpeakerBreakOutData(ESPK, GroupChatID, 2);
            if (RefreshLinksData != null)
            {
                if (RefreshLinksData.Rows.Count > 0)
                {
                    string GroupLink = ((RefreshLinksData.Rows[0]["GroupLink"] != System.DBNull.Value) ? RefreshLinksData.Rows[0]["GroupLink"].ToString() : "");
                    HostKey = ((RefreshLinksData.Rows[0]["HostKey"] != System.DBNull.Value) ? RefreshLinksData.Rows[0]["HostKey"].ToString() : "");
                    if (!string.IsNullOrEmpty(GroupLink))
                    {
                        SpkrGroupChatVisible = true;
                        HostKey = "Hostkey for " + GroupChatID.Replace(S_ID, "") + ": " + HostKey;
                        SpkrGroupChatLink = GroupLink;
                    }
                }
            }
            return Json(new { msg = "OK", SpkrGroupChatVisible = SpkrGroupChatVisible, HostKey = HostKey, SpkrGroupChatLink = SpkrGroupChatLink }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult GetDocumentsVirtualSession(int ESPK)
        {
            DataTable SpeakerDocs = repository.GetVirtualSessionDocuments(ESPK, Convert.ToInt32(User.Identity.Name), "Speaker", "0");
            DataTable AttendeeDocs = repository.GetVirtualSessionDocuments(ESPK, Convert.ToInt32(User.Identity.Name), "Attendee", "1");
            return Json(new { msg = "OK", SpeakerSource = JsonConvert.SerializeObject(SpeakerDocs), AttendeeSource = JsonConvert.SerializeObject(AttendeeDocs) }, JsonRequestBehavior.AllowGet);
        }
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public string RemoveDocumentVirtualSession(int ESPK, int ID, string Tab, string DocLink)
        {
            try
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                string strDirectory = ((Tab == "1") ? "AttendeeDocuments/" : "SpeakerDocuments/" + data.EventId.ToString());
                string strPhysicalpathFile = Path.Combine(Server.MapPath("~"), strDirectory);
                clsEventSession c = new clsEventSession();
                c.lblMsg = null;
                c.sqlConn = new System.Data.SqlClient.SqlConnection(Session["sqlConn"].ToString());

                if (c.DeleteAttendeeDocument(ID) != 0 && Tab == "1")
                {
                    if (Convert.ToBoolean(clsBlob.IsBlobSet) && Tab != "0")
                    {
                        strDirectory = strDirectory.ToLower();
                        strPhysicalpathFile = Path.Combine(clsBlob.AzureBlobUrl.ToString(), strDirectory.ToLower(), DocLink);
                    }
                    else
                        strPhysicalpathFile = Path.Combine(Server.MapPath("~"), strDirectory, DocLink);

                    clsUtility.DeleteFile(strPhysicalpathFile, null);
                    return "OK";
                }
                if (c.DeleteSpeakerDocument(ID, ESPK, Convert.ToInt32(User.Identity.Name)) != 0 && Tab == "0")
                {
                    if (Convert.ToBoolean(clsBlob.IsBlobSet) && Tab != "0")
                    {
                        strDirectory = strDirectory.ToLower();
                        strPhysicalpathFile = Path.Combine(clsBlob.AzureBlobUrl.ToString(), strDirectory.ToLower(), DocLink);
                    }
                    else
                        strPhysicalpathFile = Path.Combine(Server.MapPath("~"), strDirectory, DocLink);

                    clsUtility.DeleteFile(strPhysicalpathFile, null);
                    return "OK";
                }
                return "OK";
            }
            catch (Exception ex)
            {
                return "Error Occurred While Deleting Document";
            }
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult GetSpeakerBreakOutGroup(int ESPK, string S_ID)
        {
            int TotalAttendeeInfo = 0;
            clsEventSession c = new clsEventSession();
            c.lblMsg = null;
            c.sqlConn = new System.Data.SqlClient.SqlConnection(Session["sqlConn"].ToString());
            DataTable dt = c.getAttendanceLog(ESPK, true);
            if (dt != null)
                TotalAttendeeInfo = dt.Rows.Count;

            string Title = "Breakout Group Management Session " + S_ID;
            DataTable SpeakerBreakout = repository.GetSpeakerBreakOutDropdown(ESPK);
            return Json(new { msg = "OK", TotalAttendeeInfo = TotalAttendeeInfo, Title = Title, SpeakerBreakout = JsonConvert.SerializeObject(SpeakerBreakout) }, JsonRequestBehavior.AllowGet);
        }
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public string ZoomLinkProblemClick(int ESPK)
        {
            try
            {
                if (ESPK > 0)
                {
                    FormsIdentity identity = (FormsIdentity)User.Identity;
                    User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                    clsEventSession cEventSession = new clsEventSession();
                    cEventSession.sqlConn = new System.Data.SqlClient.SqlConnection(Session["sqlConn"].ToString());
                    cEventSession.lblMsg = null;
                    cEventSession.UpdateAccessLog(ESPK, data.Id, data.EventId, clsEvent.EnterEvent_ZoomProblem, "");
                }
            }
            catch
            {

            }
            return "OK";
        }
        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult GetApplicableSponsorInfo(int EventOrgPkey, int ESPK)
        {
            string ErrorMsg = "Error Occurred While Fetching Sponsor Inf";
            try
            {
                if (ESPK > 0)
                {
                    FormsIdentity identity = (FormsIdentity)User.Identity;
                    User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                    repository.UpdateAttendeeLog(EventOrgPkey, data.EventId, data.Id, ESPK);
                    DataTable dt = repository.GetVirtualSessionBoothDataByID(data.EventId, EventOrgPkey);
                    if (dt != null)
                    {
                        if (dt.Rows.Count > 0)
                            return Json(new { msg = "OK", URL = dt.Rows[0]["URL"], Title = "Exhibitor Partners Information", ImgLogo = dt.Rows[0]["ImgLogo"], profile = dt.Rows[0]["Profile"] }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch
            {
            }
            return Json(new { msg = ErrorMsg }, JsonRequestBehavior.AllowGet);
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
        [CustomizedAuthorize]
        public ActionResult VirtualSession()
        {
            DataSet infoTables = new DataSet();
            ViewBag.LabelTitle = "Virtual Session";
            ViewData["Title"] = "VirtualSession";
            ViewBag.bWaitPopup =false;
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            if (User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.LblTitle = data.EventName + ": Virtual Session";
                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);

                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                ViewBag.PageMeetingWebinar = cLast.strPageMeetingWebinar;
                ViewBag.intVSEventUserType = cLast.intVEventUserType;

                string intRegistrationLevel_pKey = ""; int intRegistrationLevelpKey = 0, intEventSessionPKey = 0;
                int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);
                if (!string.IsNullOrEmpty(intRegistrationLevel_pKey))
                    intRegistrationLevelpKey = Convert.ToInt32(intRegistrationLevel_pKey);
                if (Request.QueryString.Count > 0)
                {
                    string ESPK = Request.QueryString["ESPK"];
                    if (ESPK != null)
                    {
                        string EVSessionPKey = clsUtility.Decrypt(ESPK);
                        intEventSessionPKey = (string.IsNullOrEmpty(EVSessionPKey)) ? 0 : Convert.ToInt32(EVSessionPKey);
                        ViewData["intESpKey"] = intEventSessionPKey;
                    }
                }
                else
                    return Redirect((intRegistrationLevelpKey == clsEventAccount.REGISTRATION_SingleSessionOnly) ? "/MyMagi/MySchedule" : "/Home/Index");

                ViewBag.leftPanel_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
                ViewBag.VirtualDropdown_Visible = ViewBag.leftPanel_Visible;
                System.Data.DataTable EventSettings = repository.getDyamicEventSettings(data.EventId, "EventFullname,EventStatus_pKey,ShowRemindersPanel,IsChatPanelOn,EndDate");
                int EventStatusPKey = 0; bool bShowRemindersPanel = false; bool bEnableChatPanel = false; DateTime endDate = DateTime.Now;
                if (EventSettings != null && EventSettings.Rows.Count > 0)
                {
                    EventStatusPKey = (EventSettings.Rows[0]["EventStatus_pKey"] == System.DBNull.Value) ? 0 : Convert.ToInt32(EventSettings.Rows[0]["EventStatus_pKey"].ToString());
                    bShowRemindersPanel = (EventSettings.Rows[0]["ShowRemindersPanel"] == System.DBNull.Value) ? false : Convert.ToBoolean(EventSettings.Rows[0]["ShowRemindersPanel"].ToString());
                    bEnableChatPanel = (EventSettings.Rows[0]["IsChatPanelOn"] == System.DBNull.Value) ? false : Convert.ToBoolean(EventSettings.Rows[0]["IsChatPanelOn"].ToString());
                    endDate = (EventSettings.Rows[0]["EndDate"] == System.DBNull.Value) ? new DateTime() : Convert.ToDateTime(EventSettings.Rows[0]["EndDate"].ToString());
                }

                clsEvent cEvent = new clsEvent();
                cEvent.sqlConn = new SqlConnection(ReadConnectionString());
                if (bEnableChatPanel)
                {
                    bEnableChatPanel = cEvent.CheckValiditityOfModule(data.EventId, "IsChatPanelOn");
                }

                endDate = endDate.AddHours(23);
                bool bEvent = (EventStatusPKey != clsEvent.STATUS_Completed), showPanelReminders = false;
                DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                bool bSingleSession = (bEvent && (intAttendeeStatus == 1 || intAttendeeStatus == 3) && (intRegistrationLevelpKey == clsEventAccount.REGISTRATION_SingleSessionOnly));
                ViewData["RelatedSessionLink"] = true;
                ViewData["ExhibitorsLink"] = false;
                ViewData["PollingLink"] = true;

                if (bSingleSession)
                {
                    showPanelReminders = false;
                    ViewData["RelatedSessionLink"] = false;
                    ViewData["PollingLink"] = false;
                    ViewData["ExhibitorsLink"] = false;
                }
                else
                {
                    showPanelReminders = bShowRemindersPanel;
                }

                ViewBag.ChatPanel_Visible = (bEvent && endDate > dtCurrentTime) ? bEnableChatPanel : false;
                ViewBag.Reminder_Visible = showPanelReminders;

                bool NotificationTips = false;
                if (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_FullReg.ToString() || (data.GlobalAdmin))
                    NotificationTips = true;

                ViewData["VisibleTips"] = false;
                if (NotificationTips)
                    LoadNotificationTips(data.EventId);

                if (!ViewBag.ChatPanel_Visible && !ViewBag.Reminder_Visible)
                    ViewBag.leftPanel_Visible = false;
                DataTable VirtualDT = null;
                if (ViewBag.VirtualDropdown_Visible)
                {
                    DateTime dtCalTime = clsEvent.getCaliforniaTime();
                    string Host = HttpContext.Request.Path.ToUpper().Replace("/MYMAGI", "");
                    VirtualDT = repository.GetVirtualEventDropDownInfo(Host, data.Id, data.EventId, dtCurrentTime, dtCalTime, data.ParentOrganization_pKey, intAttendeeStatus, intRegistrationLevelpKey, data.GlobalAdmin, data.StaffMember);
                    ViewBag.SelectedDropDown = GetSelectedVirtualDropDown(Host);
                    ViewBag.ddEventVirtualData = VirtualDT;
                }
                if (VirtualDT == null)
                    VirtualDT = new DataTable();

                infoTables.Tables.Add(VirtualDT);
                bool bSpeaker = false;

                DataTable SpeakerProfile = repository.GetSpeakerProfiles(data.EventId, intEventSessionPKey);
                if (SpeakerProfile == null)
                {
                    bSpeaker = false;
                    SpeakerProfile = new DataTable();
                }
                else
                    bSpeaker = ((SpeakerProfile.AsEnumerable().Where(i => i.Field<int>("Account_pKey") == data.Id).FirstOrDefault()) != null);
                ViewData["bSpeaker"] = bSpeaker;

                infoTables.Tables.Add(SpeakerProfile); // Speaker Profiles
                ViewData["SessionTitleHeader"] = "A104 - Conflict Resolution:Address Difficult Situation Before they Get Our of Hand";
                ViewData["TechnicalProducer"] = false;
                ViewData["PnlSpeakerVisible"] = false;
                ViewData["PnlAttendeeVisible"] = false;
                ViewData["PanelTitle"] = "";
                bool TechnicalProducer = repository.CheckTechnincalProducer(data.EventId, data.Id);
                ViewData["TechnicalProducer"] = TechnicalProducer;
                if (ViewBag.IsStaff || TechnicalProducer)
                {
                    ViewData["PnlSpeakerVisible"] = (cLast.intVEventUserType == 0);
                    ViewData["PnlAttendeeVisible"] = (cLast.intVEventUserType == 1);
                }
                else
                {
                    if (bSpeaker)
                    {
                        ViewData["PanelTitle"] = "Speaker Functions";
                        ViewData["PnlAttendeeVisible"] = false;
                        ViewData["PnlSpeakerVisible"] = true;
                    }
                    else
                    {
                        ViewData["PanelTitle"] = "Attendee Functions";
                        ViewData["PnlAttendeeVisible"] = true;
                        ViewData["PnlSpeakerVisible"] = false;
                    }
                }
                System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;
                int TotalAttedeeCount = repository.GetAttendeeCount(request, data.EventId, intEventSessionPKey);
                repository.UpdateEventSessionAccessLog(intEventSessionPKey, data.Id, "0", bSpeaker, data.EventId, dtCurrentTime, clsEventSession.Exhibit_Webinar);
                DataTable EventSesssionData = repository.GetVirtualEventSessionData(data.Email, data.Id, intEventSessionPKey);
                if (EventSesssionData == null) { EventSesssionData = new DataTable(); }

                infoTables.Tables.Add(repository.FetchSessionFilters(0, 9));
                infoTables.Tables.Add(EventSesssionData);
                infoTables.Tables.Add(repository.GetVirtualSessionBoothData(intEventSessionPKey));

                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                ViewData["TotalAttendee"] = TotalAttedeeCount;
                ViewData["ZoomProblem"] = cSettings.getText(clsSettings.Text_ZoomProblem);
                ViewData["ZoomProblemSpeaker"] = cSettings.getText(clsSettings.Text_ZoomProblemSpeaker);
                ViewData["SessionInstruction"] = cSettings.getText(clsSettings.Text_AttendeeSessionInstruction);

                cEvent.intEvent_PKey = data.EventId;
                cEvent.sqlConn= new SqlConnection(ReadConnectionString());
                cEvent.GetBasicEventInfo(data.EventId);

                LoadVirtualSessionData(EventSesssionData, data, dtCurrentTime, cSettings, bSpeaker, intRegistrationLevelpKey, cEvent.strRegionCode);
                if (!ViewBag.bWaitPopup)
                {
                    cLast.bWaitPopup =  ViewBag.bWaitPopup;
                    cLast.WaitPopUpString =  ViewBag.WaitPopUpString;

                    Session["cLastUsed"] = cLast;
                    if (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_SingleSessionOnly.ToString())
                        return Redirect("SingleSession?IsWait=1&Type=" +  ViewBag.StrType);
                    else
                        return Redirect("MySchedule?IsWait=1&Type=" +  ViewBag.StrType);
                }

                ViewData["HelpIconData"] = repository.PageLoadResourceData(data, "", "55");
                if (!String.IsNullOrEmpty(ViewData["RedirectURL"].ToString()))
                    return Redirect(ViewData["RedirectURL"].ToString());

                ViewData["CurrentTime"] = dtCurrentTime.ToString("hh:mm tt");





                if (showPanelReminders)
                    ViewData["Reminders"] = LoadReminderInformation(data);

                if (Request.QueryString.Count > 0)
                {
                    string Link = Request.QueryString["URL"];
                    if (Link != null)
                    {
                        ViewData["MyFrameVisible"] = true;
                        ViewData["ImgPHVisible"] = false;
                        ViewData["MyFrameSrc"] = Link;
                    }
                }


                bool bShowSurveyQuestion = false;
                DataTable EventFeatures = repository.getDyamicEventSettings(data.EventId, "ISNULL(ShowSurveyQuestion,'0') as ShowSurveyQuestion");
                if (EventFeatures != null && EventFeatures.Rows.Count > 0)
                    bShowSurveyQuestion = (EventFeatures.Rows[0]["ShowSurveyQuestion"]!= System.DBNull.Value) ? Convert.ToBoolean(EventFeatures.Rows[0]["ShowSurveyQuestion"]) : false;

                int selectedEvent = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).intEventSelector;
                bool bSponsor = clsEventOrganization.CheckExhibitor(data.ParentOrganization_pKey, selectedEvent);
                ViewBag.lblRegText = "";
                ViewBag.OpenSurveyRadWindow=false;
                LoadRegistrationQuestions(data, intAttendeeStatus, bShowSurveyQuestion, bSponsor, intRegistrationLevel_pKey);
                return View(infoTables);
            }
            return View();
        }
        [CustomizedAuthorize]
        [ValidateInput(true)]
        public ActionResult _PartialRelatedSessions(int S_ID)
        {
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            DataTable dt = repository.GetRelatedSession(data.EventId, S_ID, data.Id);
            return PartialView(dt);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        public ActionResult _PartialAttendeesLog(int ESPK)
        {
            clsEventSession c = new clsEventSession();
            c.lblMsg = null;
            c.sqlConn = new System.Data.SqlClient.SqlConnection(Session["sqlConn"].ToString());
            DataTable dt = c.getAttendanceLog(ESPK, false);
            return PartialView(dt);
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public string UpdateRelatedSessionData(int ESpKey, string SessionID, bool bAttend, bool bSlides, bool bWatch)
        {
            FormsIdentity identity = (FormsIdentity)User.Identity;
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

            clsEventSession c = new clsEventSession();
            c.lblMsg = null;
            c.sqlConn = new System.Data.SqlClient.SqlConnection(Session["sqlConn"].ToString());
            c.intEventSession_PKey = ESpKey;
            c.intEvent_PKey = data.EventId;
            c.strSessionID = SessionID;
            if (c.SetAttend(data.Id, bAttend, bSlides, bWatch, data.EventId, data.EventTypeId))
                return "OK";
            else
                return "Error Occurred While Updating Related Session";
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        public void DownloadVSDoc(int ESPK, string Tab, string DocLink, string DisplayFilename)
        {
            try
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                string path = ((Tab == "1") ? "AttendeeDocuments" : "SpeakerDocuments/" + data.EventId.ToString());
                string strPhysicalpathFile = Path.Combine(Server.MapPath("~"), path, DocLink);
                bool isFileExist = clsUtility.FileExists(strPhysicalpathFile);
                if (isFileExist)
                {
                    string ext = Path.GetExtension(strPhysicalpathFile);
                    if (System.IO.File.Exists(strPhysicalpathFile))
                    {
                        string contentType = "";
                        switch (ext)
                        {
                            case ".htm":
                            case ".html": contentType = "text/HTML"; break;
                            case ".txt": contentType = "text/plain"; break;
                            case ".pdf": contentType = System.Net.Mime.MediaTypeNames.Application.Pdf; break;
                            case ".doc":
                            case ".rtf":
                            case ".docx": contentType = "Application/msword"; break;
                            case ".xls":
                            case ".xlsx": contentType = "Application/x-msexcel"; break;
                            case ".jpg":
                            case ".jpeg": contentType = "image/jpeg"; break;
                            case ".gif": contentType = "image/GIF"; break;
                            default: contentType = System.Net.Mime.MediaTypeNames.Application.Octet; break;
                        }
                        Response.AddHeader("content-disposition", "attachment; filename=" + DisplayFilename + ext);
                        Response.ContentType = "text/x-vCalendar";
                        Response.TransmitFile(strPhysicalpathFile);
                        Response.End();
                    }
                    else
                    {
                        Response.AddHeader("content-disposition", "attachment; filename= error.txt");
                        Response.ContentType = "text/plain";
                        Response.Write("Error While Downloading File");
                        Response.End();
                    }
                }
                else
                {
                    Response.AddHeader("content-disposition", "attachment; filename= error.txt");
                    Response.ContentType = "text/plain";
                    Response.Write("Error While Downloading File");
                    Response.End();
                }
            }
            catch
            {
                Response.AddHeader("content-disposition", "attachment; filename= error.txt");
                Response.ContentType = "text/plain";
                Response.Write("Error While Downloading File");
                Response.End();
            }
        }

        [CustomizedAuthorize]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult SaveMemberVirtualSession(int intBreakOut, int ESPK, string SessionID, int TotalAttendee)
        {
            bool DeletedGroup = false;
            if (intBreakOut == 0)
            {
                DeletedGroup = repository.DeleteGroupVirtualSession(ESPK, Convert.ToInt32(User.Identity.Name));
                return Json(new { msg = "OK", UpdatedMessage = "Breakout Group Deleted" }, JsonRequestBehavior.AllowGet);
            }
            int intMaxMember = TotalAttendee / intBreakOut;
            if (intMaxMember == 0 || intMaxMember > TotalAttendee)
                return Json(new { msg = "Error", UpdatedMessage = "Enter a number > 0 and < " + TotalAttendee.ToString() }, JsonRequestBehavior.AllowGet);
            bool result = repository.UpdateSpeakerBreakOut(intMaxMember, ESPK, Convert.ToInt32(User.Identity.Name), intBreakOut, SessionID);
            return Json(new { msg = "OK", UpdatedMessage = "Updated", GroupCount = intBreakOut }, JsonRequestBehavior.AllowGet);
        }

        #endregion VirtualSession

        #region ZoomSession

        [CustomizedAuthorize]
        public ActionResult _PartialZoomSession()
        {
            ViewBag.bWaitPopup =false;
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            SqlConnection conn = new SqlConnection(Session["sqlConn"].ToString());

            DataSet infoTables = new DataSet();
            ViewBag.LabelTitle = "Zoom Session";
            ViewData["Title"] = "Zoom Session";
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";

            clsUtility.BlueRibbonCheck(System.Web.HttpContext.Current.Response);
            ((clsFormList)Session["cFormlist"]).LoadPage(conn, null, System.Web.HttpContext.Current.Request, "Program", "", Request.QueryString);

            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.LblTitle = data.EventName + ": Virtual Session";
                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);

                string intRegistrationLevel_pKey = ""; int intRegistrationLevelpKey = 0, intEventSessionPKey = 0;
                int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);
                ViewBag.leftPanel_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
                ViewBag.VirtualDropdown_Visible = ViewBag.leftPanel_Visible;
                int EventSessionID = 0;
                string ESPk = Request.QueryString["ESPK"];
                if (ESPk != null)
                {
                    EventSessionID = Convert.ToInt32(clsUtility.Decrypt(ESPk));
                    ViewBag.hSessionKey = EventSessionID;
                }
                if (!string.IsNullOrEmpty(intRegistrationLevel_pKey))
                    intRegistrationLevelpKey = Convert.ToInt32(intRegistrationLevel_pKey);
                if (Request.QueryString.Count > 0)
                {
                    string ESPK = Request.QueryString["ESPK"];
                    if (ESPK != null)
                    {
                        string EVSessionPKey = clsUtility.Decrypt(ESPK);
                        intEventSessionPKey = (string.IsNullOrEmpty(EVSessionPKey)) ? 0 : Convert.ToInt32(EVSessionPKey);
                        ViewData["intESpKey"] = intEventSessionPKey;
                    }
                }
                else
                    return Redirect((intRegistrationLevelpKey == clsEventAccount.REGISTRATION_SingleSessionOnly) ? "/MyMagi/MySchedule" : "/Home/Index");


                ViewBag.ESPKString = ESPk;
                ViewBag.PageMeetingWebinar = cLast.strPageMeetingWebinar;
                ViewBag.intVSEventUserType = cLast.intVEventUserType;
                ViewBag.UserTypeVisible = (data.StaffMember || data.GlobalAdmin || cLast.bEventAccess);
                ViewBag.MySessionLastURL = Request.Url.AbsoluteUri;
                ViewBag.SpeakerZoomPopup = false;
                if (HttpContext.Request.QueryString["IsPopup"] != null)
                {
                    ViewBag.hdfMeetingURL = "/Virtual/SpeakerleftPanel?ESPK=" + Convert.ToString(Request.QueryString["ESPK"]);
                    ViewBag.SpeakerZoomPopup = true;
                }
                DataTable EventSettings = repository.getDyamicEventSettings(data.EventId, "EventFullname,EventStatus_pKey,ShowRemindersPanel,IsChatPanelOn,EndDate");
                int EventStatusPKey = 0; bool bShowRemindersPanel = false; bool bEnableChatPanel = false; DateTime endDate = DateTime.Now;
                if (EventSettings != null && EventSettings.Rows.Count > 0)
                {
                    EventStatusPKey = (EventSettings.Rows[0]["EventStatus_pKey"] == System.DBNull.Value) ? 0 : Convert.ToInt32(EventSettings.Rows[0]["EventStatus_pKey"].ToString());
                    bShowRemindersPanel = (EventSettings.Rows[0]["ShowRemindersPanel"] == System.DBNull.Value) ? false : Convert.ToBoolean(EventSettings.Rows[0]["ShowRemindersPanel"].ToString());
                    bEnableChatPanel = (EventSettings.Rows[0]["IsChatPanelOn"] == System.DBNull.Value) ? false : Convert.ToBoolean(EventSettings.Rows[0]["IsChatPanelOn"].ToString());
                    endDate = (EventSettings.Rows[0]["EndDate"] == System.DBNull.Value) ? new DateTime() : Convert.ToDateTime(EventSettings.Rows[0]["EndDate"].ToString());
                }

                clsEvent cEvent = new clsEvent();
                cEvent.sqlConn = new SqlConnection(ReadConnectionString());
                if (bEnableChatPanel)
                {
                    bEnableChatPanel = cEvent.CheckValiditityOfModule(data.EventId, "IsChatPanelOn");
                }

                endDate = endDate.AddHours(23);
                bool bEvent = (EventStatusPKey != clsEvent.STATUS_Completed), showPanelReminders = false;
                DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                bool bSingleSession = (bEvent && (intAttendeeStatus == 1 || intAttendeeStatus == 3) && (intRegistrationLevelpKey == clsEventAccount.REGISTRATION_SingleSessionOnly));
                ViewData["RelatedSessionLink"] = false;
                ViewData["ExhibitorsLink"] = false;



                ViewData["PollingLink"] = true;
                if (bSingleSession)
                {
                    showPanelReminders = false;
                    ViewData["RelatedSessionLink"] = false;
                    ViewData["PollingLink"] = false;
                    ViewData["ExhibitorsLink"] = false;
                }
                else
                    showPanelReminders = bShowRemindersPanel;

                ViewBag.ChatPanel_Visible = (bEvent && endDate > dtCurrentTime) ? bEnableChatPanel : false;
                ViewBag.Reminder_Visible = showPanelReminders;
                bool NotificationTips = false;
                if (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_FullReg.ToString() || (data.GlobalAdmin))
                    NotificationTips = true;

                ViewData["VisibleTips"] = false;
                if (NotificationTips)
                    LoadNotificationTips(data.EventId);

                if (!ViewBag.ChatPanel_Visible && !ViewBag.Reminder_Visible)
                    ViewBag.leftPanel_Visible = false;
                DataTable VirtualDT = null;
                if (ViewBag.VirtualDropdown_Visible)
                {
                    DateTime dtCalTime = clsEvent.getCaliforniaTime();
                    string Host = HttpContext.Request.Path.ToUpper().Replace("/MYMAGI", "");
                    VirtualDT = repository.GetVirtualEventDropDownInfo(Host, data.Id, data.EventId, dtCurrentTime, dtCalTime, data.ParentOrganization_pKey, intAttendeeStatus, intRegistrationLevelpKey, data.GlobalAdmin, data.StaffMember);
                    ViewBag.SelectedDropDown = GetSelectedVirtualDropDown(Host);
                }
                if (VirtualDT == null)
                    VirtualDT = new DataTable();

                infoTables.Tables.Add(VirtualDT);
                bool bSpeaker = false;

                DataTable SpeakerProfile = repository.GetSpeakerProfiles(data.EventId, intEventSessionPKey);
                if (SpeakerProfile == null)
                {
                    bSpeaker = false;
                    SpeakerProfile = new DataTable();
                }
                else
                    bSpeaker = ((SpeakerProfile.AsEnumerable().Where(i => i.Field<int>("Account_pKey") == data.Id).FirstOrDefault()) != null);
                ViewData["bSpeaker"] = bSpeaker;

                infoTables.Tables.Add(SpeakerProfile); // Speaker Profiles
                ViewData["SessionTitleHeader"] = "A104 - Conflict Resolution:Address Difficult Situation Before they Get Our of Hand";
                ViewData["TechnicalProducer"] = false;
                ViewData["PnlSpeakerVisible"] = false;
                ViewData["PnlAttendeeVisible"] = false;
                ViewData["PanelTitle"] = "";
                bool TechnicalProducer = repository.CheckTechnincalProducer(data.EventId, data.Id);
                ViewData["TechnicalProducer"] = TechnicalProducer;
                if (ViewBag.IsStaff || TechnicalProducer)
                {
                    ViewData["PnlSpeakerVisible"] = (cLast.intVEventUserType == 0);
                    ViewData["PnlAttendeeVisible"] = (cLast.intVEventUserType == 1);
                }
                else
                {
                    if (bSpeaker)
                    {
                        ViewData["PanelTitle"] = "Speaker Functions";
                        ViewData["PnlAttendeeVisible"] = false;
                        ViewData["PnlSpeakerVisible"] = true;
                    }
                    else
                    {
                        ViewData["PanelTitle"] = "Attendee Functions";
                        ViewData["PnlAttendeeVisible"] = true;
                        ViewData["PnlSpeakerVisible"] = false;
                    }
                }
                HttpRequest request = System.Web.HttpContext.Current.Request;
                int TotalAttedeeCount = repository.GetAttendeeCount(request, data.EventId, intEventSessionPKey);
                repository.UpdateEventSessionAccessLog(intEventSessionPKey, data.Id, "0", bSpeaker, data.EventId, dtCurrentTime, clsEventSession.Exhibit_Webinar);
                DataTable EventSesssionData = repository.GetVirtualEventSessionData(data.Email, data.Id, intEventSessionPKey);
                if (EventSesssionData == null) { EventSesssionData = new DataTable(); }

                infoTables.Tables.Add(repository.FetchSessionFilters(0, 9));
                infoTables.Tables.Add(EventSesssionData);
                infoTables.Tables.Add(repository.GetVirtualSessionBoothData(intEventSessionPKey));

                ViewData["TotalAttendee"] = TotalAttedeeCount;
                ViewData["ZoomProblem"] = cSettings.getText(clsSettings.Text_ZoomProblem);
                ViewData["ZoomProblemSpeaker"] = cSettings.getText(clsSettings.Text_ZoomProblemSpeaker);
                ViewData["SessionInstruction"] = cSettings.getText(clsSettings.Text_AttendeeSessionInstruction);
                
                cEvent.intEvent_PKey = data.EventId;
                cEvent.sqlConn= new SqlConnection(ReadConnectionString());
                cEvent.GetBasicEventInfo(data.EventId);
                LoadVirtualSessionData(EventSesssionData, data, dtCurrentTime, cSettings, bSpeaker, intRegistrationLevelpKey, cEvent.strRegionCode);

                if (!ViewBag.bWaitPopup)
                {
                    cLast.bWaitPopup =  ViewBag.bWaitPopup;
                    cLast.WaitPopUpString =  ViewBag.WaitPopUpString;

                    Session["cLastUsed"] = cLast;
                    if (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_SingleSessionOnly.ToString())
                        return Redirect("SingleSession?IsWait=1&Type=" +  ViewBag.StrType);
                    else
                        return Redirect("MySchedule?IsWait=1&Type=" +  ViewBag.StrType);
                }

                ViewData["HelpIconData"] = repository.PageLoadResourceData(data, "", "55");
                if (!String.IsNullOrEmpty(ViewData["RedirectURL"].ToString()))
                    return Redirect(ViewData["RedirectURL"].ToString());

                ViewData["CurrentTime"] = dtCurrentTime.ToString("hh:mm tt");

                if (showPanelReminders)
                    ViewData["Reminders"] = LoadReminderInformation(data);

                if (Request.QueryString.Count > 0)
                {
                    string Link = Request.QueryString["URL"];
                    if (Link != null)
                    {
                        ViewData["MyFrameVisible"] = true;
                        ViewData["ImgPHVisible"] = false;
                        ViewData["MyFrameSrc"] = Link;
                    }

                    if (Request.QueryString["IsPopup"] != null)
                    {
                        ViewBag.HdfMeetingURL = "/Virtual/SpeakerleftPanel?ESPK=" + Convert.ToString(Request.QueryString["ESPK"]);
                    }
                    else
                    {
                        if (Request.QueryString["hd"] != null && Request.QueryString["hd"] == "1")
                            ViewBag.HdfMeetingURL = "/Virtual/SpeakerLeftPanel?ESPK=" + Convert.ToString(Request.QueryString["ESPK"]) + "&hd=1";
                        else
                            ViewBag.HdfMeetingURL = "/Virtual/SpeakerLeftPanel?ESPK=" + Convert.ToString(Request.QueryString["ESPK"]);

                        ViewBag.Link = "";
                        if (Request.QueryString["URL"] != null)
                            ViewBag.Link = Request.QueryString["URL"];

                        //RefreshBooth()

                    }
                }

                DataTable boothExhibitors = new SqlOperation().RefreshBoothExhibitors(EventSessionID);
                if (boothExhibitors != null && boothExhibitors.Rows.Count > 0)
                {
                    ViewBag.dlBooth = boothExhibitors;
                    ViewData["ExhibitorsLink"] = true;
                }

                return PartialView(infoTables);
            }

            return PartialView(infoTables);
        }

        public ActionResult ZoomSession()
        {
            DataSet infoTables = new DataSet();
            ViewBag.ZoomLoginPopUp = false;
            ViewBag.bWaitPopup =false;
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
            {
                SqlConnection conn = new SqlConnection(Session["sqlConn"].ToString());

                clsUtility.BlueRibbonCheck(System.Web.HttpContext.Current.Response);
                ((clsFormList)Session["cFormlist"]).LoadPage(conn, null, System.Web.HttpContext.Current.Request, "Program", "", Request.QueryString);

                User_Login data = new User_Login();
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);

                ViewBag.LabelTitle = "Zoom Session";
                ViewData["Title"] = "Zoom Session";
                ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";

                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.LblTitle = data.EventName + ": Virtual Session";
                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);

                string intRegistrationLevel_pKey = ""; int intRegistrationLevelpKey = 0, intEventSessionPKey = 0;
                int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);
                ViewBag.leftPanel_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
                ViewBag.VirtualDropdown_Visible = ViewBag.leftPanel_Visible;
                int EventSessionID = 0;
                string ESPk = Request.QueryString["ESPK"];
                if (ESPk != null)
                {
                    EventSessionID = Convert.ToInt32(clsUtility.Decrypt(ESPk));
                    ViewBag.hSessionKey = EventSessionID;
                }
                if (!string.IsNullOrEmpty(intRegistrationLevel_pKey))
                    intRegistrationLevelpKey = Convert.ToInt32(intRegistrationLevel_pKey);
                if (Request.QueryString.Count > 0)
                {
                    string ESPK = Request.QueryString["ESPK"];
                    if (ESPK != null)
                    {
                        string EVSessionPKey = clsUtility.Decrypt(ESPK);
                        intEventSessionPKey = (string.IsNullOrEmpty(EVSessionPKey)) ? 0 : Convert.ToInt32(EVSessionPKey);
                        ViewData["intESpKey"] = intEventSessionPKey;
                    }
                }
                else
                    return Redirect((intRegistrationLevelpKey == clsEventAccount.REGISTRATION_SingleSessionOnly) ? "/MyMagi/MySchedule" : "/Home/Index");

                ViewBag.PageMeetingWebinar = cLast.strPageMeetingWebinar;
                ViewBag.ESPKString = ESPk;
                ViewBag.intVSEventUserType = cLast.intVEventUserType;
                ViewBag.UserTypeVisible = (data.StaffMember || data.GlobalAdmin || cLast.bEventAccess);
                ViewBag.MySessionLastURL = Request.Url.AbsoluteUri;
                ViewBag.SpeakerZoomPopup = false;
                if (HttpContext.Request.QueryString["IsPopup"] != null)
                {
                    ViewBag.hdfMeetingURL = "/Virtual/SpeakerleftPanel?ESPK=" + Convert.ToString(Request.QueryString["ESPK"]);
                    ViewBag.SpeakerZoomPopup = true;
                }
                DataTable EventSettings = repository.getDyamicEventSettings(data.EventId, "EventFullname,EventStatus_pKey,ShowRemindersPanel,IsChatPanelOn,EndDate");
                int EventStatusPKey = 0; bool bShowRemindersPanel = false; bool bEnableChatPanel = false; DateTime endDate = DateTime.Now;
                if (EventSettings != null && EventSettings.Rows.Count > 0)
                {
                    EventStatusPKey = (EventSettings.Rows[0]["EventStatus_pKey"] == System.DBNull.Value) ? 0 : Convert.ToInt32(EventSettings.Rows[0]["EventStatus_pKey"].ToString());
                    bShowRemindersPanel = (EventSettings.Rows[0]["ShowRemindersPanel"] == System.DBNull.Value) ? false : Convert.ToBoolean(EventSettings.Rows[0]["ShowRemindersPanel"].ToString());
                    bEnableChatPanel = (EventSettings.Rows[0]["IsChatPanelOn"] == System.DBNull.Value) ? false : Convert.ToBoolean(EventSettings.Rows[0]["IsChatPanelOn"].ToString());
                    endDate = (EventSettings.Rows[0]["EndDate"] == System.DBNull.Value) ? new DateTime() : Convert.ToDateTime(EventSettings.Rows[0]["EndDate"].ToString());
                }

                clsEvent cEvent = new clsEvent();
                cEvent.sqlConn = new SqlConnection(ReadConnectionString());
                if (bEnableChatPanel)
                {
                    bEnableChatPanel = cEvent.CheckValiditityOfModule(data.EventId, "IsChatPanelOn");
                }

                endDate = endDate.AddHours(23);
                bool bEvent = (EventStatusPKey != clsEvent.STATUS_Completed), showPanelReminders = false;
                DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                bool bSingleSession = (bEvent && (intAttendeeStatus == 1 || intAttendeeStatus == 3) && (intRegistrationLevelpKey == clsEventAccount.REGISTRATION_SingleSessionOnly));
                ViewData["RelatedSessionLink"] = true;
                ViewData["ExhibitorsLink"] = false;



                ViewData["PollingLink"] = true;
                if (bSingleSession)
                {
                    showPanelReminders = false;
                    ViewData["RelatedSessionLink"] = false;
                    ViewData["PollingLink"] = false;
                    ViewData["ExhibitorsLink"] = false;
                }
                else
                    showPanelReminders = bShowRemindersPanel;

                ViewBag.ChatPanel_Visible = (bEvent && endDate > dtCurrentTime) ? bEnableChatPanel : false;
                ViewBag.Reminder_Visible = showPanelReminders;
                bool NotificationTips = false;
                if (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_FullReg.ToString() || (data.GlobalAdmin))
                    NotificationTips = true;

                ViewData["VisibleTips"] = false;
                if (NotificationTips)
                    LoadNotificationTips(data.EventId);

                if (!ViewBag.ChatPanel_Visible && !ViewBag.Reminder_Visible)
                    ViewBag.leftPanel_Visible = false;
                DataTable VirtualDT = null;
                if (ViewBag.VirtualDropdown_Visible)
                {
                    DateTime dtCalTime = clsEvent.getCaliforniaTime();
                    string Host = HttpContext.Request.Path.ToUpper().Replace("/MYMAGI", "");
                    VirtualDT = repository.GetVirtualEventDropDownInfo(Host, data.Id, data.EventId, dtCurrentTime, dtCalTime, data.ParentOrganization_pKey, intAttendeeStatus, intRegistrationLevelpKey, data.GlobalAdmin, data.StaffMember);
                    ViewBag.SelectedDropDown = GetSelectedVirtualDropDown(Host);
                }
                if (VirtualDT == null)
                    VirtualDT = new DataTable();

                infoTables.Tables.Add(VirtualDT);
                bool bSpeaker = false;

                DataTable SpeakerProfile = repository.GetSpeakerProfiles(data.EventId, intEventSessionPKey);
                if (SpeakerProfile == null)
                {
                    bSpeaker = false;
                    SpeakerProfile = new DataTable();
                }
                else
                    bSpeaker = ((SpeakerProfile.AsEnumerable().Where(i => i.Field<int>("Account_pKey") == data.Id).FirstOrDefault()) != null);
                ViewData["bSpeaker"] = bSpeaker;

                infoTables.Tables.Add(SpeakerProfile); // Speaker Profiles
                ViewData["SessionTitleHeader"] = "A104 - Conflict Resolution:Address Difficult Situation Before they Get Our of Hand";
                ViewData["TechnicalProducer"] = false;
                ViewData["PnlSpeakerVisible"] = false;
                ViewData["PnlAttendeeVisible"] = false;
                ViewData["PanelTitle"] = "";
                bool TechnicalProducer = repository.CheckTechnincalProducer(data.EventId, data.Id);
                ViewData["TechnicalProducer"] = TechnicalProducer;
                if (ViewBag.IsStaff || TechnicalProducer)
                {
                    ViewData["PnlSpeakerVisible"] = (cLast.intVEventUserType == 0);
                    ViewData["PnlAttendeeVisible"] = (cLast.intVEventUserType == 1);
                }
                else
                {
                    if (bSpeaker)
                    {
                        ViewData["PanelTitle"] = "Speaker Functions";
                        ViewData["PnlAttendeeVisible"] = false;
                        ViewData["PnlSpeakerVisible"] = true;
                    }
                    else
                    {
                        ViewData["PanelTitle"] = "Attendee Functions";
                        ViewData["PnlAttendeeVisible"] = true;
                        ViewData["PnlSpeakerVisible"] = false;
                    }
                }
                HttpRequest request = System.Web.HttpContext.Current.Request;
                int TotalAttedeeCount = repository.GetAttendeeCount(request, data.EventId, intEventSessionPKey);
                repository.UpdateEventSessionAccessLog(intEventSessionPKey, data.Id, "0", bSpeaker, data.EventId, dtCurrentTime, clsEventSession.Exhibit_Webinar);
                DataTable EventSesssionData = repository.GetVirtualEventSessionData(data.Email, data.Id, intEventSessionPKey);
                if (EventSesssionData == null) { EventSesssionData = new DataTable(); }

                infoTables.Tables.Add(repository.FetchSessionFilters(0, 9));
                infoTables.Tables.Add(EventSesssionData);
                infoTables.Tables.Add(repository.GetVirtualSessionBoothData(intEventSessionPKey));

                ViewData["TotalAttendee"] = TotalAttedeeCount;
                ViewData["ZoomProblem"] = cSettings.getText(clsSettings.Text_ZoomProblem);
                ViewData["ZoomProblemSpeaker"] = cSettings.getText(clsSettings.Text_ZoomProblemSpeaker);
                ViewData["SessionInstruction"] = cSettings.getText(clsSettings.Text_AttendeeSessionInstruction);

                cEvent.intEvent_PKey = data.EventId;
                cEvent.sqlConn= new SqlConnection(ReadConnectionString());
                cEvent.GetBasicEventInfo(data.EventId);
                LoadVirtualSessionData(EventSesssionData, data, dtCurrentTime, cSettings, bSpeaker, intRegistrationLevelpKey, cEvent.strRegionCode);
                if (!ViewBag.bWaitPopup)
                {
                    cLast.bWaitPopup =  ViewBag.bWaitPopup;
                    cLast.WaitPopUpString =  ViewBag.WaitPopUpString;

                    Session["cLastUsed"] = cLast;
                    if (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_SingleSessionOnly.ToString())
                        return Redirect("SingleSession?IsWait=1&Type=" +  ViewBag.StrType);
                    else
                        return Redirect("MySchedule?IsWait=1&Type=" +  ViewBag.StrType);
                }
                ViewData["HelpIconData"] = repository.PageLoadResourceData(data, "", "55");
                if (!String.IsNullOrEmpty(ViewData["RedirectURL"].ToString()))
                    return Redirect(ViewData["RedirectURL"].ToString());

                ViewData["CurrentTime"] = dtCurrentTime.ToString("hh:mm tt");

                if (showPanelReminders)
                    ViewData["Reminders"] = LoadReminderInformation(data);

                if (Request.QueryString.Count > 0)
                {
                    string Link = Request.QueryString["URL"];
                    if (Link != null)
                    {
                        ViewData["MyFrameVisible"] = true;
                        ViewData["ImgPHVisible"] = false;
                        ViewData["MyFrameSrc"] = Link;
                    }

                    if (Request.QueryString["IsPopup"] != null)
                    {
                        ViewBag.HdfMeetingURL = "/Virtual/SpeakerleftPanel?ESPK=" + Convert.ToString(Request.QueryString["ESPK"]);
                    }
                    else
                    {
                        if (Request.QueryString["hd"] != null && Request.QueryString["hd"] == "1")
                            ViewBag.HdfMeetingURL = "/Virtual/SpeakerLeftPanel?ESPK=" + Convert.ToString(Request.QueryString["ESPK"]) + "&hd=1";
                        else
                            ViewBag.HdfMeetingURL = "/Virtual/SpeakerLeftPanel?ESPK=" + Convert.ToString(Request.QueryString["ESPK"]);

                        ViewBag.Link = "";
                        if (Request.QueryString["URL"] != null)
                            ViewBag.Link = Request.QueryString["URL"];

                        //RefreshBooth()

                    }
                }

                DataTable boothExhibitors = new SqlOperation().RefreshBoothExhibitors(EventSessionID);
                if (boothExhibitors != null && boothExhibitors.Rows.Count > 0)
                {
                    ViewBag.dlBooth = boothExhibitors;
                    ViewData["ExhibitorsLink"] = true;
                }
                bool bShowSurveyQuestion = false;
                DataTable EventFeatures = repository.getDyamicEventSettings(data.EventId, "ISNULL(ShowSurveyQuestion,'0') as ShowSurveyQuestion");
                if (EventFeatures != null && EventFeatures.Rows.Count > 0)
                    bShowSurveyQuestion = (EventFeatures.Rows[0]["ShowSurveyQuestion"]!= System.DBNull.Value) ? Convert.ToBoolean(EventFeatures.Rows[0]["ShowSurveyQuestion"]) : false;

                int selectedEvent = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).intEventSelector;
                bool bSponsor = clsEventOrganization.CheckExhibitor(data.ParentOrganization_pKey, selectedEvent);
                ViewBag.lblRegText = "";
                ViewBag.OpenSurveyRadWindow=false;
                LoadRegistrationQuestions(data, intAttendeeStatus, bShowSurveyQuestion, bSponsor, intRegistrationLevel_pKey);
                return View(infoTables);
            }
            else
            {
                ViewBag.ZoomLoginPopUp = true;
            }
            return View(infoTables);
        }

        [CustomizedAuthorize]
        public ActionResult SpeakerLeftPanel()
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            SqlConnection conn = new SqlConnection(Session["sqlConn"].ToString());

            DataSet infoTables = new DataSet();
            ViewBag.LabelTitle = "Zoom Session";
            ViewData["Title"] = "Zoom Session";
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";

            clsUtility.BlueRibbonCheck(System.Web.HttpContext.Current.Response);
            ((clsFormList)Session["cFormlist"]).LoadPage(conn, null, System.Web.HttpContext.Current.Request, "Program", "", Request.QueryString);

            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.LblTitle = data.EventName + ": Virtual Session";
                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);

                string intRegistrationLevel_pKey = ""; int intRegistrationLevelpKey = 0, intEventSessionPKey = 0;
                int intAttendeeStatus = clsEventAccount.getAttendeeStatus(data.Id, data.EventId, ref intRegistrationLevel_pKey);
                ViewBag.leftPanel_Visible = (data.GlobalAdmin || data.StaffMember || intAttendeeStatus == 1);
                ViewBag.VirtualDropdown_Visible = ViewBag.leftPanel_Visible;
                int EventSessionID = 0;
                string ESPk = Request.QueryString["ESPK"];
                if (ESPk != null)
                {
                    EventSessionID = Convert.ToInt32(clsUtility.Decrypt(ESPk));
                    ViewBag.hSessionKey = EventSessionID;
                    ViewBag.hdnParentPageURL = "/Virtual/ZoomSession?ESPK=" + Request.QueryString["ESPK"] + ((Request.QueryString["hd"]!=null && Request.QueryString["hd"] =="1") ? "&hd=1" : "");
                }
                if (!string.IsNullOrEmpty(intRegistrationLevel_pKey))
                    intRegistrationLevelpKey = Convert.ToInt32(intRegistrationLevel_pKey);
                if (Request.QueryString.Count > 0)
                {
                    string ESPK = Request.QueryString["ESPK"];
                    if (ESPK != null)
                    {
                        string EVSessionPKey = clsUtility.Decrypt(ESPK);
                        intEventSessionPKey = (string.IsNullOrEmpty(EVSessionPKey)) ? 0 : Convert.ToInt32(EVSessionPKey);
                        ViewData["intESpKey"] = intEventSessionPKey;
                    }
                }
                else
                    return Redirect((intRegistrationLevelpKey == clsEventAccount.REGISTRATION_SingleSessionOnly) ? "/MyMagi/MySchedule" : "/Home/Index");

                ViewBag.PageMeetingWebinar = cLast.strPageMeetingWebinar;
                ViewBag.ESPKString = ESPk;
                ViewBag.intVSEventUserType = cLast.intVEventUserType;
                ViewBag.UserTypeVisible = (data.StaffMember || data.GlobalAdmin || cLast.bEventAccess);
                ViewBag.MySessionLastURL = Request.Url.AbsoluteUri;
                ViewBag.SpeakerZoomPopup = false;
                if (HttpContext.Request.QueryString["IsPopup"] != null)
                {
                    ViewBag.hdfMeetingURL = "/Virtual/SpeakerleftPanel?ESPK=" + Convert.ToString(Request.QueryString["ESPK"]);
                    ViewBag.SpeakerZoomPopup = true;
                }
                DataTable EventSettings = repository.getDyamicEventSettings(data.EventId, "EventFullname,EventStatus_pKey,ShowRemindersPanel,IsChatPanelOn,EndDate");
                int EventStatusPKey = 0; bool bShowRemindersPanel = false; bool bEnableChatPanel = false; DateTime endDate = DateTime.Now;
                if (EventSettings != null && EventSettings.Rows.Count > 0)
                {
                    EventStatusPKey = (EventSettings.Rows[0]["EventStatus_pKey"] == System.DBNull.Value) ? 0 : Convert.ToInt32(EventSettings.Rows[0]["EventStatus_pKey"].ToString());
                    bShowRemindersPanel = (EventSettings.Rows[0]["ShowRemindersPanel"] == System.DBNull.Value) ? false : Convert.ToBoolean(EventSettings.Rows[0]["ShowRemindersPanel"].ToString());
                    bEnableChatPanel = (EventSettings.Rows[0]["IsChatPanelOn"] == System.DBNull.Value) ? false : Convert.ToBoolean(EventSettings.Rows[0]["IsChatPanelOn"].ToString());
                    endDate = (EventSettings.Rows[0]["EndDate"] == System.DBNull.Value) ? new DateTime() : Convert.ToDateTime(EventSettings.Rows[0]["EndDate"].ToString());
                }

                clsEvent cEvent = new clsEvent();
                cEvent.sqlConn = new SqlConnection(ReadConnectionString());
                if (bEnableChatPanel)
                {
                    bEnableChatPanel = cEvent.CheckValiditityOfModule(data.EventId, "IsChatPanelOn");
                }

                endDate = endDate.AddHours(23);
                bool bEvent = (EventStatusPKey != clsEvent.STATUS_Completed), showPanelReminders = false;
                DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                bool bSingleSession = (bEvent && (intAttendeeStatus == 1 || intAttendeeStatus == 3) && (intRegistrationLevelpKey == clsEventAccount.REGISTRATION_SingleSessionOnly));
                ViewData["RelatedSessionLink"] = true;
                ViewData["ExhibitorsLink"] = false;



                ViewData["PollingLink"] = true;
                if (bSingleSession)
                {
                    showPanelReminders = false;
                    ViewData["RelatedSessionLink"] = false;
                    ViewData["PollingLink"] = false;
                    ViewData["ExhibitorsLink"] = false;
                }
                else
                    showPanelReminders = bShowRemindersPanel;

                ViewBag.ChatPanel_Visible = (bEvent && endDate > dtCurrentTime) ? bEnableChatPanel : false;
                ViewBag.Reminder_Visible = showPanelReminders;
                bool NotificationTips = false;
                if (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_FullReg.ToString() || (data.GlobalAdmin))
                    NotificationTips = true;

                ViewData["VisibleTips"] = false;
                if (NotificationTips)
                    LoadNotificationTips(data.EventId);

                if (!ViewBag.ChatPanel_Visible && !ViewBag.Reminder_Visible)
                    ViewBag.leftPanel_Visible = false;
                DataTable VirtualDT = null;
                if (ViewBag.VirtualDropdown_Visible)
                {
                    DateTime dtCalTime = clsEvent.getCaliforniaTime();
                    string Host = HttpContext.Request.Path.ToUpper().Replace("/MYMAGI", "");
                    VirtualDT = repository.GetVirtualEventDropDownInfo(Host, data.Id, data.EventId, dtCurrentTime, dtCalTime, data.ParentOrganization_pKey, intAttendeeStatus, intRegistrationLevelpKey, data.GlobalAdmin, data.StaffMember);
                    ViewBag.SelectedDropDown = GetSelectedVirtualDropDown(Host);
                }
                if (VirtualDT == null)
                    VirtualDT = new DataTable();

                infoTables.Tables.Add(VirtualDT);
                bool bSpeaker = false;

                DataTable SpeakerProfile = repository.GetSpeakerProfiles(data.EventId, intEventSessionPKey);
                if (SpeakerProfile == null)
                {
                    bSpeaker = false;
                    SpeakerProfile = new DataTable();
                }
                else
                    bSpeaker = ((SpeakerProfile.AsEnumerable().Where(i => i.Field<int>("Account_pKey") == data.Id).FirstOrDefault()) != null);
                ViewData["bSpeaker"] = bSpeaker;

                infoTables.Tables.Add(SpeakerProfile); // Speaker Profiles
                ViewData["SessionTitleHeader"] = "A104 - Conflict Resolution:Address Difficult Situation Before they Get Our of Hand";
                ViewData["TechnicalProducer"] = false;
                ViewData["PnlSpeakerVisible"] = false;
                ViewData["PnlAttendeeVisible"] = false;
                ViewData["PanelTitle"] = "";
                bool TechnicalProducer = repository.CheckTechnincalProducer(data.EventId, data.Id);
                ViewData["TechnicalProducer"] = TechnicalProducer;
                if (ViewBag.IsStaff || TechnicalProducer)
                {
                    ViewData["PnlSpeakerVisible"] = (cLast.intVEventUserType == 0);
                    ViewData["PnlAttendeeVisible"] = (cLast.intVEventUserType == 1);
                }
                else
                {
                    if (bSpeaker)
                    {
                        ViewData["PanelTitle"] = "Speaker Functions";
                        ViewData["PnlAttendeeVisible"] = false;
                        ViewData["PnlSpeakerVisible"] = true;
                    }
                    else
                    {
                        ViewData["PanelTitle"] = "Attendee Functions";
                        ViewData["PnlAttendeeVisible"] = true;
                        ViewData["PnlSpeakerVisible"] = false;
                    }
                }
                HttpRequest request = System.Web.HttpContext.Current.Request;
                int TotalAttedeeCount = repository.GetAttendeeCount(request, data.EventId, intEventSessionPKey);
                repository.UpdateEventSessionAccessLog(intEventSessionPKey, data.Id, "0", bSpeaker, data.EventId, dtCurrentTime, clsEventSession.Exhibit_Webinar);
                DataTable EventSesssionData = repository.GetVirtualEventSessionData(data.Email, data.Id, intEventSessionPKey);
                if (EventSesssionData == null) { EventSesssionData = new DataTable(); }

                infoTables.Tables.Add(repository.FetchSessionFilters(0, 9));
                infoTables.Tables.Add(EventSesssionData);
                infoTables.Tables.Add(repository.GetVirtualSessionBoothData(intEventSessionPKey));

                ViewData["TotalAttendee"] = TotalAttedeeCount;
                ViewData["ZoomProblem"] = cSettings.getText(clsSettings.Text_ZoomProblem);
                ViewData["ZoomProblemSpeaker"] = cSettings.getText(clsSettings.Text_ZoomProblemSpeaker);
                ViewData["SessionInstruction"] = cSettings.getText(clsSettings.Text_AttendeeSessionInstruction);

                
                cEvent.intEvent_PKey = data.EventId;
                cEvent.sqlConn= new SqlConnection(ReadConnectionString());
                cEvent.GetBasicEventInfo(data.EventId);
                LoadVirtualSessionData(EventSesssionData, data, dtCurrentTime, cSettings, bSpeaker, intRegistrationLevelpKey, cEvent.strRegionCode);
                if (!ViewBag.bWaitPopup)
                {
                    cLast.bWaitPopup =  ViewBag.bWaitPopup;
                    cLast.WaitPopUpString =  ViewBag.WaitPopUpString;

                    Session["cLastUsed"] = cLast;
                    if (intRegistrationLevel_pKey == clsEventAccount.REGISTRATION_SingleSessionOnly.ToString())
                        return Redirect("SingleSession?IsWait=1&Type=" +  ViewBag.StrType);
                    else
                        return Redirect("MySchedule?IsWait=1&Type=" +  ViewBag.StrType);
                }

                ViewData["HelpIconData"] = repository.PageLoadResourceData(data, "", "55");
                if (!String.IsNullOrEmpty(ViewData["RedirectURL"].ToString()))
                    return Redirect(ViewData["RedirectURL"].ToString());

                ViewData["CurrentTime"] = dtCurrentTime.ToString("hh:mm tt");

                if (showPanelReminders)
                    ViewData["Reminders"] = LoadReminderInformation(data);

                if (Request.QueryString.Count > 0)
                {
                    string Link = Request.QueryString["URL"];
                    if (Link != null)
                    {
                        ViewData["MyFrameVisible"] = true;
                        ViewData["ImgPHVisible"] = false;
                        ViewData["MyFrameSrc"] = Link;
                    }

                    if (Request.QueryString["IsPopup"] != null)
                    {
                        ViewBag.HdfMeetingURL = "/Virtual/SpeakerleftPanel?ESPK=" + Convert.ToString(Request.QueryString["ESPK"]);
                    }
                    else
                    {
                        if (Request.QueryString["hd"] != null && Request.QueryString["hd"] == "1")
                            ViewBag.HdfMeetingURL = "/Virtual/SpeakerLeftPanel?ESPK=" + Convert.ToString(Request.QueryString["ESPK"]) + "&hd=1";
                        else
                            ViewBag.HdfMeetingURL = "/Virtual/SpeakerLeftPanel?ESPK=" + Convert.ToString(Request.QueryString["ESPK"]);

                        ViewBag.Link = "";
                        if (Request.QueryString["URL"] != null)
                            ViewBag.Link = Request.QueryString["URL"];

                        //RefreshBooth()

                    }
                }

                DataTable boothExhibitors = new SqlOperation().RefreshBoothExhibitors(EventSessionID);
                if (boothExhibitors != null && boothExhibitors.Rows.Count > 0)
                {
                    ViewBag.dlBooth = boothExhibitors;
                    ViewData["ExhibitorsLink"] = true;
                }

                return View(infoTables);
            }

            return View(infoTables);
        }

        #endregion ZoomSession
        [CustomizedAuthorize]
        public JsonResult cmdSaveQClick()
        {
            User_Login data = new User_Login();
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                if (repository.UpdateSurveyClickCount(data.EventId,data.Id))
                    return Json(new {msg="OK", URL = "/SpeakerRegistrationFeedback?1=1&Req=1&APK=" + data.Id.ToString() },JsonRequestBehavior.AllowGet);
            }
            return Json(new { msg = "Error Occurred", URL = "" }, JsonRequestBehavior.AllowGet);
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
    }
}
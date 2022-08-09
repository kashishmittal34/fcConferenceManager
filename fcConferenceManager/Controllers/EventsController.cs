using fcConferenceManager.Models;
using MAGI_API.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace fcConferenceManager.Controllers
{
    [CheckActiveEventAttribute]
    public class EventsController : Controller
    {
        DBAccessLayer dba = new DBAccessLayer();
        static SqlOperation repository = new SqlOperation();
        // GET: Event
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

        private void CheckLoginType()
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
            if (User.Identity.AuthenticationType == "Forms")
            {
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);

                DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                ViewBag.CurrentTime = dtCurrentTime;
            }


        }

        //overview

        [ActionName("EventInfo")]
        public ActionResult EventsInfo()
        {
            CheckLoginType();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            int EvtPKey = 0;
            if (Request.QueryString["EVT"]!= null)
                int.TryParse(Request.QueryString["EVT"].ToString(), out EvtPKey);

            if (EvtPKey==0)
                EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);

            clsEvent cEvent = new clsEvent();
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.LoadEvent();

            clsVenue cVenue = new clsVenue();
            cVenue.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cVenue.intVenue_PKey = cEvent.intVenue_PKey;
            cVenue.LoadVenue();

            DataTable eventinfo = new DataTable();
            eventinfo = dba.FETCHEventData(EvtPKey);
            if (eventinfo.Rows.Count <= 0)
            {
                ViewBag.lblNoInfoText = cSettings.getText(clsSettings.TEXT_EventInfo);

            }
            var c = cEvent.dtStartDate.ToString("MMMM") + " " + cEvent.dtStartDate.Day.ToString() + "-" + (cEvent.dtEndDate.ToString("MMMM") != cEvent.dtStartDate.ToString("MMMM") ? cEvent.dtEndDate.ToString("MMMM") + " " : "") + cEvent.dtEndDate.Day.ToString() + ", " + cEvent.dtStartDate.ToString("yyyy");
            var currentevent = c;// dba.CurrentEventInfo(EvtPKey);
            List<EventsInfo> _eventinfo = new List<EventsInfo>();
            foreach (DataRow info in eventinfo.Rows)
            {
                string res = "";
                if (info["SectionText"].ToString() != "")
                {
                    res = clsReservedWords.ReplaceMyPageText(null, info["SectionText"].ToString(), cEvent, cVenue);
                }
                _eventinfo.Add(new EventsInfo
                {
                    SectionTitle = info["SectionTitle"].ToString(),
                    SectionText = res // info["SectionText"].ToString()
                });

            }
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            ViewBag.PageTitle = cEvent.strEventFullname;
            ViewBag.EventInfo = _eventinfo;
            ViewBag.CurrentEvent = currentevent;
        
            string formpopupISMAGIRight = "Is MAGI Right For You?";
            string formtextISRight = dba.FETCHFormData(formpopupISMAGIRight);
            string resultpopuptextISRight = "";
            if (formtextISRight != "")
            {
                resultpopuptextISRight = clsReservedWords.ReplaceMyPageText(null, formtextISRight, cEvent, cVenue);
            }
            ViewBag.PopUPText =
            ViewBag.FormText = resultpopuptextISRight;

            string popupform_TC = "Event Terms & Conditions";
            string popuptext_TC = dba.FETCHFormData(popupform_TC);
            string resultpopuptext_TC = "";
            if (popuptext_TC != "")
            {
                resultpopuptext_TC = clsReservedWords.ReplaceMyPageText(null, popuptext_TC, cEvent, cVenue);
            }
            ViewBag.FormTextT_C = resultpopuptext_TC;

            BindOverViewDropDown();

            return View("~/Views/Events/EventsInfo.cshtml");
        }

        public JsonResult AttendencePopuplink()
        {
            string s = "/frmPopupFile.aspx?EPK=" + clsUtility.TYPE_Reserved.ToString();
            s = s + "&DN=" + HttpUtility.UrlEncode("Attendance Approval Request Letter");
            s = s + "&N=" + HttpUtility.UrlEncode("MAGI_Attendance_Approval_Request_Letter.doc");
            return Json(new { result = "OK", link = s }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsConferenceRightPopUplink()
        {
            string s = "/frmPopupFile.aspx?EPK=" + clsUtility.TYPE_Reserved.ToString();
            s = s + "&DN=" + HttpUtility.UrlEncode("Is MAGI Conference Right for You?");
            s = s + "&N=" + HttpUtility.UrlEncode("Is_the_MAGI_Conference_Right_for_You.doc");
            return Json(new { result = "OK", link = s }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Program()
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            clsUtility.BlueRibbonCheck(System.Web.HttpContext.Current.Response);
            ((clsFormList)Session["cFormlist"]).LoadPage(conn, null, System.Web.HttpContext.Current.Request, "Program", "", Request.QueryString);
            User_Login data = new User_Login();
            data.EventId = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            data.Id = 0;
            if (Request.QueryString["EVT"] != null)
            {
                int EventpKey = 0;
                int.TryParse(Request.QueryString["EVT"], out EventpKey);
                if (EventpKey > 0)
                    data.EventId = EventpKey;
            }

            if (Request.QueryString["Live"] != null)
            {
                int IsLive = 0;
                int.TryParse(Request.QueryString["Live"], out IsLive);
                if (IsLive ==1)
                    cLast.IsLiveStream = true;
            }
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);
                ViewBag.EventTypePKey = cLast.intEventType_PKey;
            }
            clsEvent cEvent = new clsEvent();
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.intEvent_PKey = data.EventId;
            if (!cEvent.LoadEvent())
                return RedirectToAction("Index", "Home");
            cLast.intPendingEventChange = cEvent.intEvent_PKey;
            cLast.intEventType_PKey = cEvent.intEventType_PKey;
            ViewBag.PageTitle = cEvent.strEventFullname;

            if (cEvent.bProgramDarft)
            {
                ViewBag.intProgramToViewEvent_pkey = data.EventId;
                ViewBag.WaterMark = false;
                ViewBag.PageTitle += " <font class='ms-1 me-1' color='#ff0000'> Draft</font> Program";
            }
            else
            {
                if (data.EventId == cSettings.intNextEvent_pKey)
                {
                    ViewBag.intProgramToViewEvent_pkey = ((cEvent.bShowProgram && data.GlobalAdmin) ? data.EventId : cSettings.intPrimaryEvent_pkey);
                    ViewBag.PageTitle = ((cEvent.bShowProgram && data.GlobalAdmin) ? (cEvent.strEventFullname + " Program " + " <font class='ms-1 me-1' color='#ff0000'>(Administrator View)</font>") : (cSettings.strPrimaryEventFullName + " Program" + ((cEvent.bShowProgram) ? "" : " <font class='ms-1 me-1' color='#ff0000'> (Previous Event)</font>")));
                }
                else
                {
                    if (cEvent.bShowProgram || data.GlobalAdmin)
                        ViewBag.PageTitle += " Program <font class='ms-1 me-1' color='#ff0000'>" + ((data.EventId == cSettings.intPrimaryEvent_pkey && !cEvent.bShowProgram) ? " (Draft)" : "") + "</font>";
                    else
                        ViewBag.PageTitle = cSettings.strPriorEventFullName + " Program" + ((cEvent.bShowProgram) ? "" : " <font class='ms-1 me-1' color='#ff0000'> (Previous Event)</font>");

                    ViewBag.intProgramToViewEvent_pkey = ((cEvent.bShowProgram || data.GlobalAdmin) ? data.EventId : cSettings.intPriorEvent_pKey);
                }
                ViewBag.WaterMark = (!cEvent.bShowProgram && !data.GlobalAdmin);
            }

            ViewBag.ShowTopic = cEvent.bShowTopic;
            ViewBag.ShowTrack = cEvent.bShowTrack && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "ShowTrack");
            ViewBag.bSpkDefShow = cEvent.bShowSpeaker;
            ViewBag.bShowRelated = (cEvent.bShowRelatedActivity && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "ShowRelatedActivity"));
            ViewBag.bShowAgendaDetail = (cEvent.bShowAgendaDetail && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "ShowAgendaDetail"));

            DateTime dtCurrentTime = clsEvent.getEventVenueTime();
            ViewBag.CurrentTime = dtCurrentTime;
            ViewBag.lblInfo = "";
            if (!(cEvent.bShowProgram && data.GlobalAdmin))
                ViewBag.lblInfo = cEvent.ReplaceReservedWords(cSettings.getText(clsSettings.TEXT_ProgramNotAvailable));

            ViewBag.phNoProgram = (!cEvent.bShowProgram && !cEvent.bProgramDarft);
            ViewBag.SectionTitle = cEvent.strStandardRegion;
            ViewBag.ckShowEdu = cLast.bEduLevel;
            ViewBag.ckShowSpeak = cLast.bProgramSpeaker;
            ViewBag.ckShowRelated = cLast.bProgramRelated;
            ViewBag.ckBlurb = cLast.bBlurb;
            ViewBag.bDefaultSpkShow = (cLast.bProgramSpeaker && ViewBag.bSpkDefShow);
            ViewBag.IsSingleTrack = (cLast.intEventType_PKey == clsEvent.EventType_SingleTrack);
            ViewBag.SelectedAudience = cLast.strProgSelectionsAudience;
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            ViewBag.ckShowEdu = cLast.bEduLevel;
            ViewBag.ckShowSpeak = cLast.bProgramSpeaker;
            ViewBag.ckShowRelated = cLast.bProgramRelated;
            ViewBag.ckBlurb = cLast.bBlurb;
            ViewBag.bDefaultSpkShow = (cLast.bProgramSpeaker && ViewBag.bSpkDefShow);
            ViewBag.SelectedAudience = cLast.strProgSelectionsAudience;
            ViewBag.SelectedTopic = cLast.strProgSelections;
            ViewBag.SelectedTracks = cLast.strProgSelectionsTracks;
            ViewBag.IsLiveStream = cLast.IsLiveStream;

            BindSessionTracks(data.EventId);
            BindSessionTopics(data.EventId);
            BindSessionAudience(data.EventId);
            BindOverViewDropDown();
            ViewBag.SessionInfo = dba.SessionDetailsProcessed(data.EventId, cLast.bProgramRelated, cEvent.bShowTopic, cLast.bProgramSpeaker, "1", cLast.IsLiveStream);
            return View("~/Views/Events/Programs.cshtml");
        }
        public ActionResult _PartialProgram(bool bProgramSpeaker, bool bProgramRelated, bool bShowDesc, bool bShowEdu, bool bLiveStream)
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            User_Login data = new User_Login();
            data.EventId = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            data.Id = 0;
            if (Request.QueryString["EVT"] != null)
            {
                int EventpKey = 0;
                int.TryParse(Request.QueryString["EVT"], out EventpKey);
                if (EventpKey > 0)
                    data.EventId = EventpKey;
            }
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            }
            DataTable EventInfo = repository.getDyamicEventSettings(data.EventId, "ShowTopic");
            bool bShowTopic = false;
            if (EventInfo != null && EventInfo.Rows.Count > 0)
                bShowTopic = (EventInfo.Rows[0]["ShowTopic"] == DBNull.Value) ? false : Convert.ToBoolean(EventInfo.Rows[0]["ShowTopic"].ToString());


            ViewBag.SessionInfo = dba.SessionDetailsProcessed(data.EventId, cLast.bProgramRelated, bShowTopic, cLast.bProgramSpeaker, "1", bLiveStream);
            return PartialView();
        }
        public string UpdateSelections(bool bProgramSpeaker, bool bProgramRelated, bool bShowDesc, bool bShowEdu, bool bLiveStream, string strTopics, string strTracks, string strAudience)
        {
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            cLast.bProgramSpeaker = bProgramSpeaker;
            cLast.bProgramRelated = bProgramRelated;
            cLast.bEduLevel = bShowEdu;
            cLast.bBlurb = bShowDesc;
            cLast.strProgSelections = strTopics;
            cLast.strProgSelectionsTracks =strTracks;
            cLast.strProgSelectionsAudience = strAudience;
            cLast.IsLiveStream = bLiveStream;
            Session["cLastUsed"] = cLast;
            return "OK";
        }


        [ActionName("Speakers")]
        public ActionResult Speaker(string PostData1, string PostData2)
        {
            CheckLoginType();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);

            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            clsEvent cEvent = new clsEvent();
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.LoadEvent();

            int star = clsFeedback.RATING_EXCELLENT;
            string formname = "Speakers Page (Not Available)";
            string formtext = dba.FETCHFormData(formname);
            string resultformtext = "";

            bool chnagelabelColor = false;
            if (cEvent.bShowSpeaker)
            {
                string ss = cSettings.getText(clsSettings.TEXT_SpeakerPage);
                formtext = ss;
                chnagelabelColor = true;
            }
            if (formtext != "")
            {
                clsVenue cVenue = new clsVenue();
                cVenue.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                cVenue.intVenue_PKey = cEvent.intVenue_PKey;
                cVenue.LoadVenue();
                var res = clsReservedWords.ReplaceMyPageText(null, formtext, cEvent, cVenue);
                resultformtext = res;
            }
            cLast.bSpkrsSession = (PostData1 != null && PostData1.Trim().ToLower() == "checked") || cLast.bSpkrsSession;
            cLast.bSpkrsSort = (PostData2 != null && PostData2.Trim().ToLower() == "checked") || cLast.bSpkrsSort;

            var speakerinfo = dba.FETCHSpeaker(EvtPKey, ((cLast.bSpkrsSort) ? "Org" : ""), star);
            ViewBag.CBSessionCheck = cLast.bSpkrsSession;
            ViewBag.OrgIsCheked = cLast.bSpkrsSort;
            ViewBag.PageTitle = cEvent.strEventFullname + " Speakers";
            ViewBag.FormText = resultformtext;
            ViewBag.LabelColor = chnagelabelColor;
            Session["cLastUsed"] = cLast;
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            BindOverViewDropDown();
            return View("~/Views/Events/Speaker.cshtml", speakerinfo);
        }
        public JsonResult Spkclick(string id)
        {
            clsAccount cAccount = new clsAccount();
            cAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cAccount.intAccount_PKey = Convert.ToInt32(id);
            cAccount.LoadAccount();
            string speakername = cAccount.strFirstname + " " + cAccount.strLastname;
            string speakerInfo = cAccount.strPersonalBio;
            string speakertitle = cAccount.strTitle;
            string speakerOrganization = cAccount.strOrganizationID;

            bool imageexist = false;
            string filepath = "~/accountimages/" + id + "_img";
            if (System.IO.File.Exists(Server.MapPath("~/accountimages/" + id + "_img.jpg")))
                imageexist = true;
            return Json(new { sname = speakername, sinfo = speakerInfo, stitle = speakertitle, sOrg = speakerOrganization, imageexist }, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult _SortSpeakerPage(string PostData1, string PostData2)
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            int star = clsFeedback.RATING_EXCELLENT;
            cLast.bSpkrsSession = (PostData1 != null && PostData1.Trim().ToLower() == "checked");
            cLast.bSpkrsSort = (PostData2 != null && PostData2.Trim().ToLower() == "checked");
            var speakerinfo = dba.FETCHSpeaker(EvtPKey, ((cLast.bSpkrsSort) ? "Org" : ""), star);
            Session["cLastUsed"] = cLast;
            ViewBag.CBSessionCheck = cLast.bSpkrsSession;
            ViewBag.OrgIsCheked = cLast.bSpkrsSort;
            return PartialView("~/Views/Shared/_SortSpeakerPage.cshtml", speakerinfo);
        }


        [ActionName("BeASpeaker")]
        public ActionResult BecomeSpeaker()
        {
            CheckLoginType();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            clsEvent cEvent = new clsEvent();
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.LoadEvent();

            clsVenue cVenue = new clsVenue();
            cVenue.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cVenue.intVenue_PKey = cEvent.intVenue_PKey;
            cVenue.LoadVenue();
            string result = "";
            ViewBag.PageTitle = "Become a Speaker at a MAGI Conference";
            ViewBag.Admin= false;
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";

            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                clsAccount cAccount = new clsAccount();
                cAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                cAccount.intAccount_PKey = data.Id;
                cAccount.LoadAccount();

                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                cEventAccount.intAccount_pKey = data.Id;
                cEventAccount.intEvent_pKey = data.EventId;
                cEventAccount.LoadEventInfo();

                ViewBag.Admin= data.GlobalAdmin;
                result = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.TEXT_BeASpeaker), cEvent: cEvent,cVenue: cVenue, cAccount: cAccount,cEventAccount: cEventAccount);
            }
            else
                result = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.TEXT_BeASpeaker), cEvent: cEvent, cVenue: cVenue);

            ViewBag.FormText = result;
            BindOverViewDropDown();
            
            //string formname = "Become a Speaker at a MAGI Conference", resultformtext = "";
            //string formtext = dba.FETCHFormData(formname);
            //string res = "";
            //if (formtext != "")
            //{
            //    res = clsReservedWords.ReplaceMyPageText(null, formtext, cEvent, cVenue);
            //    resultformtext = res;
            //}
            return View("~/Views/Events/Event.cshtml");

        }

        [ActionName("EventSponsors")]
        public ActionResult Partners(int PostData1 = 0, string PostData2 = "", int aud = 0, int sol = 0)
        {
            CheckLoginType();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            clsEvent cEvent = new clsEvent();
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.LoadEvent();
            ViewBag.bShowPartner = cEvent.bShowPartner;
            ViewBag.phNoProgram = !cEvent.bShowPartner;
            ViewBag.lblInfo = "";
            if (cEvent.bShowPartner == false)
            {
                ViewBag.lblInfo = "Partners for this event are not yet available.";

                // Me.lblInfo.Text = .ReplaceReservedWords(DirectCast(Session("cSettings"), clsSettings).getText(clsSettings.Text_ParticipatinPartnersNotAvail))
                //myVS.intMode = MODE_Other;
            }
            ViewBag.lblInfo  = ViewBag.lblInfo.Replace("Partner", cEvent.strPartnerAlias);
            ViewBag.PageTitle="Partners";
            ViewBag.PageTitle  = cEvent.strEventFullname + ": " +  ViewBag.PageTitle.Replace("Partner", cEvent.strPartnerAlias);
            data.EventId = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            List<string> levels = BindLevel(PostData1);
            ViewBag.levelId = PostData1;
            ViewBag.levelName = PostData2;
            ViewBag.AudienceId = aud;
            ViewBag.SolutionId = sol;

            if (cEvent.bShowAudienceType && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "bShowAudienceType"))
                ViewBag.AudienceList = SelectOptions_AudienceTypes();

            if (cEvent.bShowSolutionType && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "bShowSolutionType"))
                ViewBag.SolutionList = SelectOptions_SolutionTypes();

            var partnerinfo = dba.EventSponsors(PostData1, EvtPKey, aud, sol);
            ViewBag.EventSponsors = partnerinfo;
            ViewBag.AllLevel = levels;
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            BindOverViewDropDown();
            return View("~/Views/Events/Partners.cshtml", partnerinfo);
        }

        public ActionResult BecomeSponsor()
        {
            CheckLoginType();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);

            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            clsEvent cEvent = new clsEvent();
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.LoadEvent();

            clsVenue cVenue = new clsVenue();
            cVenue.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cVenue.intVenue_PKey = cEvent.intVenue_PKey;
            cVenue.LoadVenue();



            string formname = "Become an Event Sponsor";
            string formtext = dba.FETCHFormData(formname);
            string resultformtext = "";
            ViewBag.PageTitle = "Become an Event Sponsor";

            string res = "";
            if (formtext != "")
            {
                res = clsReservedWords.ReplaceMyPageText(null, formtext, cEvent, cVenue);
                resultformtext = res;
            }
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            ViewBag.FormText = resultformtext;
            BindOverViewDropDown();
            return View();
            //  return View("~/Views/Events/Event.cshtml");

        }
        [ActionName("Participating")]
        public ActionResult ParticipatingOrganization()
        {
            CheckLoginType();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);

            clsUtility.BlueRibbonCheck(System.Web.HttpContext.Current.Response);
            ((clsFormList)Session["cFormlist"]).LoadPage(new System.Data.SqlClient.SqlConnection(ReadConnectionString()), null, System.Web.HttpContext.Current.Request, "Program", "", Request.QueryString);

            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            if (Request.QueryString["PK"] != null)
            {
                int EventpKey = 0;
                int.TryParse(Request.QueryString["PK"], out EventpKey);
                if (EventpKey > 0)
                    EvtPKey = EventpKey;
            }

            clsEvent cEvent = new clsEvent();
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.intEvent_PKey = EvtPKey;
            if (!cEvent.LoadEvent())
                return RedirectToAction("Index", "Home");

            int intProgramToViewEvent_pkey = 0;
            ViewBag.PageTitle = cEvent.strEventFullname + " : Participating Organizations ";
            if (EvtPKey == cSettings.intNextEvent_pKey)
            {
                intProgramToViewEvent_pkey = cEvent.bShowParticipatingOrg ? EvtPKey : cSettings.intPrimaryEvent_pkey;
                ViewBag.PageTitle = ((cEvent.bShowParticipatingOrg) ? cEvent.strEventFullname : cSettings.strPrimaryEventFullName) + " Participating Organizations" + ((cEvent.bShowParticipatingOrg) ? "" : " <font color='#ff0000'>(Previous Event)</font>");
            }
            else
            {
                intProgramToViewEvent_pkey = cEvent.bShowParticipatingOrg ? EvtPKey : cSettings.intPriorEvent_pKey;
                ViewBag.PageTitle = ((cEvent.bShowParticipatingOrg) ? cEvent.strEventFullname : cSettings.strPriorEventFullName) + " Participating Organizations" + ((cEvent.bShowParticipatingOrg) ? "" : " <font color='#ff0000'>(Previous Event)</font>");
            }

            ViewBag.phNoProgram = !cEvent.bShowParticipatingOrg;
            if (ViewBag.phNoProgram)
            {
                ViewBag.lblInfo = cEvent.ReplaceReservedWords(cSettings.getText(clsSettings.TEXT_ParticipatingNotAvail));
                ViewBag.intMode = 1;
            }

            //clsUtility.ShowSidePanel(Me.Page, myVS.intCurEventPKey, cEvent)

            var orginfo = dba.FetchOrganisation(intProgramToViewEvent_pkey);
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            ViewBag.Organization = orginfo;

            DataTable ds = dba.Organizationtype();
            List<SelectListItem> orgtypelist = new List<SelectListItem>();
            foreach (DataRow dr in ds.Rows)
                orgtypelist.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pKey"].ToString() });

            DataTable ds2 = dba.SiteOrg();
            List<SelectListItem> siteorgtypelist = new List<SelectListItem>();
            foreach (DataRow dr in ds2.Rows)
                siteorgtypelist.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pKey"].ToString() });

            ViewBag.siteOrgType = siteorgtypelist;
            ViewBag.OrgType = orgtypelist;

            return View("ParticipatingOrganization");
        }
        public ActionResult Certification()
        {

            CheckLoginType();
            int AccountPkey = 0;
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            if (User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                AccountPkey = data.Id;

            }
            ViewBag.AccountPkey = AccountPkey;
            int EventPkey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            ViewBag.PageTitle = "Clinical Research Contract Professional(CRCP) Certification";
            ViewBag.FormText = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(clsSettings.TEXT_Certification), intEvtPKey: EventPkey);
            string lblCertStatus = "";
            if (AccountPkey > 0)
            {
                clsAccount cAccount = ((clsAccount)Session["cAccount"]); // new clsAccount();
                cAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                cAccount.intAccount_PKey = AccountPkey;
                cAccount.LoadAccount();

                switch (cAccount.intCRCPStatus)
                {
                    case clsAccount.CRCP_Expired:
                        lblCertStatus = "According to our records, your CRCP certification expired on " + cAccount.dtCRCPExpirationDate.ToShortDateString();
                        break;
                    case clsAccount.CRCP_Active:
                        lblCertStatus = "According to our records, your CRCP certification is active. It will expire on " + cAccount.dtCRCPExpirationDate.ToShortDateString();
                        break;
                    default:
                        lblCertStatus = "According to our records, you have not taken the CRCP exam.";
                        break;
                }
                ViewBag.lblCertStatus = lblCertStatus;
            }
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            return View();


        }
        public ActionResult ContinueEducation()
        {
            CheckLoginType();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();

            data.EventId = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            string formname = "CRCP Certification";

            string content = clsReservedWords.ReplaceCurrent(null, clsSettings.ReplaceTermsGeneral(cSettings.getText(32)));

            var eventinfo = dba.FETCHContinueEducation(formname, data.EventId);
            ViewBag.PageTitle = "Continuing Education Contact Hours";
            ViewBag.EventInfo = content;
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            BindOverViewDropDown();
            return View("~/Views/Events/ContinueEducation.cshtml");

        }
        public ActionResult EventTermsandConditions()
        {
            CheckLoginType();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);

            string formname = "Event Terms & Conditions";
            string formtext = dba.FETCHFormData(formname);
            ViewBag.PageTitle = formname;
            ViewBag.FormText = formtext;
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            BindOverViewDropDown();
            return View();
        }

        private List<SelectListItem> SelectOptions_AudienceTypes()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            DataTable dt = new DataTable();
            dt = dba.GetAudienceTypes();
            foreach (DataRow dr in dt.Rows)
            {
                SelectListItem selectListItem = new SelectListItem() { Value = dr[0].ToString(), Text = dr[1].ToString() };
                selectListItems.Add(selectListItem);
            }
            return selectListItems;
        }

        private List<SelectListItem> SelectOptions_SolutionTypes()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            DataTable dt = new DataTable();
            dt = dba.GetSolutionTypes();
            foreach (DataRow dr in dt.Rows)
            {
                SelectListItem selectListItem = new SelectListItem() { Value = dr[0].ToString(), Text = dr[1].ToString() };
                selectListItems.Add(selectListItem);
            }
            return selectListItems;
        }

        public List<string> BindLevel(int levelSelected = 0)
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();
            data.EventId = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            string levelid = levelSelected.ToString();
            DataTable ds = dba.GetPartnersLevel(data.EventId, levelSelected);
            List<SelectListItem> levellist = new List<SelectListItem>();
            List<string> level = new List<string>();
            if (levelid == "0")
            {
                foreach (DataRow dr in ds.Rows)
                {
                    levellist.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pKey"].ToString() });
                    level.Add(dr["pKey"].ToString());
                }
            }
            else
            {
                foreach (DataRow dr in ds.Rows)
                {
                    if (dr["pKey"].ToString() == levelid)
                    {
                        levellist.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pKey"].ToString(), Selected = true });
                        level.Add(dr["pKey"].ToString());
                    }
                    else
                    {
                        levellist.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pKey"].ToString() });
                        level.Add(dr["pKey"].ToString());
                    }
                }

            }
            ViewBag.level = levellist;
            return level;
        }
        public ActionResult IsMAGIRightForYou()
        {
            CheckLoginType();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);

            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            clsEvent cEvent = new clsEvent();
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.LoadEvent();

            clsVenue cVenue = new clsVenue();
            cVenue.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cVenue.intVenue_PKey = cEvent.intVenue_PKey;
            cVenue.LoadVenue();

            string formname = "Is MAGI Right For You?";
            string formtext = dba.FETCHFormData(formname);

            string popupform = "Event Terms & Conditions";
            string popuptext = dba.FETCHFormData(popupform);

            string resultformtext = "";
            string resultpopuptext = "";

            ViewBag.PageTitle = formname;
            if (formtext != "")
            {
                resultformtext = clsReservedWords.ReplaceMyPageText(null, formtext, cEvent, cVenue);
            }
            if (popuptext != "")
            {
                resultpopuptext = clsReservedWords.ReplaceMyPageText(null, popuptext, cEvent, cVenue);
            }

            ViewBag.FormText = resultformtext;
            ViewBag.PopUpText = resultpopuptext;
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            BindOverViewDropDown();
            return View();

        }
        [ActionName("VenueInfo")]
        public ActionResult Venue()
        {
            CheckLoginType();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            clsUtility.BlueRibbonCheck(System.Web.HttpContext.Current.Response);
            ((clsFormList)Session["cFormlist"]).LoadPage(new System.Data.SqlClient.SqlConnection(ReadConnectionString()), null, System.Web.HttpContext.Current.Request, "Program", "", Request.QueryString);
            int EvtPKey = 0, intVenue_Pkey = 0;
            if (Request.QueryString["EVT"] != null)
                int.TryParse(Request.QueryString["EVT"], out EvtPKey);
            if (Request.QueryString["VPK"] != null)
                int.TryParse(Request.QueryString["VPK"], out intVenue_Pkey);

            if (EvtPKey <= 0)
                EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);

            clsEvent cEvent = new clsEvent();
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.intEvent_PKey = EvtPKey;
            if (!cEvent.LoadEvent())
                return RedirectToAction("Index", "Home");
            ViewBag.bShowEventPages = cEvent.bShowEvtPages;
            ViewBag.intEventType_pkey = cEvent.intEventType_PKey;
            ViewBag.PageTitle = cEvent.strEventFullname + " Venue & Lodging";
            if (intVenue_Pkey <= 0)
                intVenue_Pkey = cEvent.intVenue_PKey;

            clsVenue cVenue = new clsVenue();
            cVenue.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cVenue.intVenue_PKey = intVenue_Pkey;
            if (!cVenue.LoadVenue())
                return RedirectToAction("Index", "Home");

            DataTable dt = new SqlOperation().VRefreshImagesText_MVC(intVenue_Pkey, EvtPKey);

            if ((ViewBag.intEventType_pkey == clsEvent.EventType_CloudConference || ViewBag.intEventType_pkey == clsEvent.EventType_HybridConference) && ViewBag.bShowEventPages)
                return Redirect("/EventOnCloud" + ((Request.QueryString["EVT"] != null) ? "?EVT=" + Request.QueryString["EVT"] : ""));
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string res = "";
                    if (dt.Rows[i]["SectionText"] != System.DBNull.Value && dt.Rows[i]["SectionText"] != null)
                        res = clsReservedWords.ReplaceMyPageText(null, dt.Rows[i]["SectionText"].ToString(), cEvent, cVenue);
                    dt.Rows[i].SetField("SectionText", res);
                }
                dt.AcceptChanges();
            }


            ViewBag.VenuImagesText = dt;
            var venueinfo = dba.FETCHVenue();
            foreach (var venue in venueinfo)
            {
                string res = "";
                if (res != "")
                    res = clsReservedWords.ReplaceMyPageText(null, venue.SectionText, cEvent, cVenue);
                venue.SectionText = res;
            }
            ViewBag.VenueInfo = venueinfo;
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            BindOverViewDropDown();
            return View("Venue");
        }



        public ActionResult EventContact(int? M = 0, int? Q = 0)
        {
            if (M==1)
                return RedirectToAction("EventContact", "Event");

            if (Q==1)
                return RedirectToAction("FAQs", "Event");

            CheckLoginType();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);

            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            clsEvent cEvent = new clsEvent();
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.LoadEvent();
            clsVenue cVenue = new clsVenue();
            cVenue.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cVenue.intVenue_PKey = cEvent.intVenue_PKey;
            cVenue.LoadVenue();

            var contcatinfo = dba.FetchContactMAGI(EvtPKey);

            foreach (var inf in contcatinfo)
            {
                string res = clsReservedWords.ReplaceMyPageText(null, inf.Email, cEvent, cVenue);
                inf.Email = res;

            }

            string Host = "/"+ HttpContext.Request.Path.Split('/')[2].ToUpper();
            string SelectedValue = GetSelectedOverViewDropDown(Host);
            ViewBag.PageTitle = "Questions about " + cEvent.strEventFullname + " ?";
            ViewBag.ContactInfo = contcatinfo;
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            BindOverViewDropDown();
            ViewBag.OverViewDropDownSelected = SelectedValue;
            return View("~/Views/Events/ContactMAGI.cshtml");

        }
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult EmailLinkRedirect(string rolePkey, string email, string title, string tel)
        {
            try
            {
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
                clsEvent cEvent = new clsEvent();
                cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                cEvent.intEvent_PKey = EvtPKey;
                cEvent.LoadEvent();
                cLast.colNotifications.Clear();
                cLast.bNotificationSupport = true;
                clsOneNotify c = new clsOneNotify();
                c.strEmailAddress = email;
                cLast.colNotifications.Add(c);
                string strSubject = "Support for " + cEvent.strEventID;
                if (title == "Partner Relations:")
                    strSubject = "Partnering at MAGI Conference";

                string elink = "/SendEmail?SG=1&R=" + rolePkey + "&C=" + HttpUtility.UrlEncode(strSubject) + "&T=" + HttpUtility.UrlEncode(tel);
                Session["cLastUsed"] = cLast;
                c = null;
                return Json(new { msg = "OK", url = elink }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { msg = "Error Occurred", url = "" }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult SessionPage()
        {


            return View();
        }
        public ActionResult Testimonials()
        {
            CheckLoginType();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);

            var testimonials = dba.Testimonials();
            ViewBag.SessionInfo = testimonials;
            ViewBag.PageTitle = "Testimonials";
            ViewBag.Testimonials = testimonials;
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            return View("~/Views/Events/Testimonials.cshtml", testimonials);

        }
        public ActionResult Advisory(string Name, string Title, string Org)
        {
            CheckLoginType();
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            var advisoryinfo = dba.AdvisoryBoard(Name, Title, Org);
            ViewBag.AdvisoryInfo = advisoryinfo;
            ViewBag.PageTitle = "Advisory Board";
            return View("~/Views/Events/Advisory.cshtml", advisoryinfo);

        }

        public PartialViewResult _PartialFAQS(int CKC = 0, int CKM = 0, int CkID = 0)
        {
            int AccountPkey = 0, EventPkey = 0;
            User_Login data = new User_Login();
            clsAccount cAccount = new clsAccount();
            clsEvent cEvent = new clsEvent();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            EventPkey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);

            if (User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                AccountPkey = data.Id;
                EventPkey = data.EventId;
            }

            bool CKCChecked = (CKC == 1);

            cEvent.intEvent_PKey = EventPkey;
            cEvent.sqlConn =conn;
            cEvent.LoadEvent();

            string accountType = getMyAcctTypes(AccountPkey);
            string intRegistrationLevel_pKey = "";
            bool StaffMember = data.StaffMember;
            bool bSponsor = new SqlOperation().VerifyIsPartner(AccountPkey, EventPkey);
            int intAttendeeStatus = clsEventAccount.getAttendeeStatus(AccountPkey, EventPkey, ref intRegistrationLevel_pKey);
            bool IsAttendee = (intAttendeeStatus != clsEventAccount.PARTICIPATION_Cancelled);
            bool bSpeaker = clsEventAccount.getSpeakerStatus(AccountPkey, EventPkey);

            DateTime dtCurrentTime = clsEvent.getEventVenueTime();
            var faq = new DataTable();
            DataTable faqQuesAns = dba.EventFAQCategoriesWithQuestion(EventPkey, IsAttendee, bSponsor, bSpeaker, StaffMember, accountType, cEvent.dtStartDate.ToShortDateString(), cEvent.dtEndDate.ToShortDateString(), AccountPkey, CKM, CKCChecked);
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
                    string fmt = "000000.##";
                    string formatString = " {0,15:" + fmt + "}";

                    sb.Append(" <div class=\"FAQ Font10\">  <details class=\"FAQQuestn "+ ("Details_" +  row["FAQ_pKey"])+"\" style=\"padding-left:10px\"><summary id='" + ("summary_" +  row["FAQ_pKey"]) +"'><span>" + row["Question"] + "<span class='showIDs' style='display:none;'>(" + string.Format(formatString, Convert.ToInt32(row["FAQ_pKey"].ToString())) + ")</span></span></summary><table width=\"100%\";><tr><td><div class='Font10' style=\"background-color:azure ;  margin: 4px 2px 2px 2px\"><span>" + resulttext + "</span></div></td></tr><tr><td> ");
                    sb.AppendLine(" <div class='Font10' style=\"display: inline-block ; margin: 4px 2px 2px 2px\"><span>Was this information helpful?</span> <input style=\"margin-left:10px\" type=\"radio\" id =\"rdYes\" name=\"" + row["FAQ_pKey"] + "rdbtn\" value =\"1\" class='me-1' /><label> Yes </label ><input style=\"margin-left:10px\" type=\"radio\" id =\"rdSomewhat\" class='me-1'  name=\"" + row["FAQ_pKey"] + "rdbtn\" value =\"2\" /><label > Somewhat </label > ");
                    sb.AppendLine(" <input style=\"margin-left:10px\" type=\"radio\" name=\"" + row["FAQ_pKey"] + "rdbtn\" id=\"rdNo\" value=\"3\" class='me-1'  /><label> No</label> </div><br /><div style=\"display: inline-block; margin: 4px 2px 6px 2px\"><span class='me-1 Font10' > Please Suggest Suggestion :</span><input type=\"text\" id=\"" + row["FAQ_pKey"] + "txtSuggestion\" style=\"width:500px ;height: 22px;\" /></div><br />  ");
                    sb.AppendLine("   <button class =\"SubmitFeedback btn\" id=\"btnSubmit\" onclick=\"SaveFAQSuggestion(" + row["FAQ_pKey"] + "," + row["FAQCategory_pKey"] + ")\" type=\"button\">OK</button></td></tr></table></details> </div>  ");
                    string htmltext = sb.ToString();
                    row["Elem"] = htmltext;
                    faqQuesAns.AcceptChanges();
                }
            }
            ViewBag.QuesAnswer = faqQuesAns;
            return PartialView(faq);
        }

        public ActionResult FAQs()
        {
            User_Login data = new User_Login();
            string lblFAQ = "", lblFAQSignIn = "";
            bool dvFAQSignVisible = false;
            int AccountPkey = 0, EventPkey = 0;
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());

            if (User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                if (!(data.GlobalAdmin || data.StaffMember || data.bAttendeeAtCurrEvent || data.Is_Speaker))
                    return RedirectToAction("Index", "Home");

                AccountPkey = data.Id;
                EventPkey = data.EventId;

                ViewBag.ID = AccountPkey;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.EventPKey = EventPkey;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);
            }
            else
            {
                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                EventPkey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
                dvFAQSignVisible = true;
                lblFAQSignIn = cSettings.getText(clsSettings.Text_FAQSignIn);
            }

            lblFAQ = cSettings.getText(clsSettings.Text_FAQInstruct);
            ViewBag.lblFAQVisible = (!string.IsNullOrEmpty(lblFAQ));
            ViewBag.InstructionText = lblFAQ;
            ViewBag.lblFAQText = lblFAQ;
            ViewBag.lblFAQSignIn = lblFAQSignIn;
            ViewBag.dvFAQSignVisible = dvFAQSignVisible;
            ViewBag.Title = "MAGI Support";
            BindOverViewDropDown();
            return View();
        }

        public JsonResult RefreshFAQ(int CKC = 0, int CKM = 0, int CkID = 0)
        {
            int AccountPkey = 0;
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            ViewBag.InstructionText = cSettings.getText(clsSettings.Text_FAQInstruct);

            int EventPkey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            if (User.Identity.AuthenticationType == "Forms")
            {

                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                AccountPkey = data.Id;
                EventPkey = data.EventId;

            }

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EventPkey;
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();

            bool CKCChecked = false;
            if (CKC == 1)
            {
                CKCChecked = true;
            }

            clsAccount cAccount = new clsAccount();
            cAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cAccount.intAccount_PKey = AccountPkey;
            cAccount.LoadAccount();

            string accountType = getMyAcctTypes(AccountPkey);
            string intRegistrationLevel_pKey = "";
            bool StaffMember = cAccount.bStaffMember;
            bool bSponsor = cAccount.bIsPartner;
            int intAttendeeStatus = clsEventAccount.getAttendeeStatus(0, EventPkey, ref intRegistrationLevel_pKey);
            bool IsAttendee = (intAttendeeStatus != clsEventAccount.PARTICIPATION_Cancelled);
            bool bSpeaker = clsEventAccount.getSpeakerStatus(0, EventPkey);
            var faq = dba.EventFAQCategories(EventPkey, IsAttendee, bSponsor, bSpeaker, StaffMember, accountType, cEvent.dtStartDate.ToShortDateString(), cEvent.dtEndDate.ToShortDateString(), AccountPkey, CKM, CKCChecked);
            List<string> faqs = new List<string>();
            foreach (DataRow dr in faq.Rows)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<tr id=\"" + dr["FAQCategory_pKey"] + "\" onclick=\"ExpendFAQ(" + dr["FAQCategory_pKey"] + ")\"> <td> <details class=\"CategoryList\" style=\"color:black; \"> ");
                sb.AppendLine(" <summary class=\"FAQCategory\" id=\"" + dr["FAQCategory_pKey"] + "\">" + dr["FAQCategoryID"] + "</summary> ");
                sb.AppendLine(" <div style=\"margin-left:20px; background-color:whitesmoke; color: black;\">  ");
                sb.AppendLine(" <table id=\"FAQstable+" + dr["FAQCategory_pKey"] + "\" style=\"padding-left:20px\"><tr></tr></table></div></details> </td> </tr> ");
                string htmltext = sb.ToString();
                faqs.Add(htmltext);
            }
            return Json(new { result = "OK", faqtable = faqs }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GETFAQsQuestions(int CategoryId, string searchtext = "", int ckIdcCheckBox = 0, int CKM = 0, int CKC = 0)
        {
            User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(((FormsIdentity)User.Identity).Ticket.UserData);
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            clsEvent cEvent = new clsEvent();

            cEvent.intEvent_PKey = data.EventId;
            cEvent.sqlConn = conn;
            cEvent.GetBasicEventInfo(cEvent.intEvent_PKey);
            string accountType = getMyAcctTypes(data.Id);

            DataTable faqQuesAns = dba.FAQsQuestionAnswer(data.EventId, CategoryId, accountType, cEvent.dtStartDate.ToShortDateString(), cEvent.dtEndDate.ToShortDateString(), searchtext);

            DataTable Info = new DataTable();
            if (faqQuesAns != null && faqQuesAns.Rows.Count>0)
                Info = faqQuesAns.DefaultView.ToTable(true, "FAQCategory_pkey", "FAQ_pKey");

            var JsonResult = Json(new { result = "OK", FAQs = Newtonsoft.Json.JsonConvert.SerializeObject(Info) }, JsonRequestBehavior.AllowGet);
            JsonResult.MaxJsonLength = int.MaxValue;
            return JsonResult;
        }

        public ActionResult SubmitFAQs(string SelectedValue, string SuggestionText, string QuestionpKey)
        {
            int? AccountPkey = null;
            if (User.Identity.AuthenticationType == "Forms")
            {
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                AccountPkey = data.Id;
            }

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

            var resulttext = dba.SaveFAQFeedback(Status, Suggestion, Convert.ToInt32(QuestionpKey), AccountPkey);

            return Json(new { result = "OK", message = resulttext }, JsonRequestBehavior.AllowGet);
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
        public ActionResult UpcomingEvent()
        {
            CheckLoginType();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);

            var upcomingevent = dba.Upcoming_Events();
            ViewBag.PageTitle = "Upcoming Events";
            ViewBag.formText = "";
            return View("~/Views/Events/UpcomingEvent.cshtml", upcomingevent);

        }

        public List<SelectListItem> BindSessionTracks(int EventPkey)
        {
            DataTable ds = dba.BindSessionTracks(EventPkey);
            List<SelectListItem> sessionTrackList = new List<SelectListItem>();
            foreach (DataRow dr in ds.Rows)
            {
                sessionTrackList.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pKey"].ToString() });
            }
            ViewBag.TrackList = sessionTrackList;
            return sessionTrackList;
        }
        public List<SelectListItem> BindSessionTopics(int EventPkey)
        {
            DataTable ds = dba.BindSessionTopics(EventPkey);
            List<SelectListItem> sessionTopicList = new List<SelectListItem>();
            foreach (DataRow dr in ds.Rows)
            {

                sessionTopicList.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["strText"].ToString() });
            }
            ViewBag.TopicList = sessionTopicList;
            return sessionTopicList;
        }

        public List<SelectListItem> BindSessionAudience(int EventPkey)
        {
            DataTable ds = dba.BindSessionAudience(EventPkey);
            List<SelectListItem> sessionAudience = new List<SelectListItem>();
            foreach (DataRow dr in ds.Rows)
            {
                //string text = dr["strText"].ToString();
                //string result = "";
                //int pos = text.IndexOf("Audience");
                //if (pos >= 0)
                //{                  
                //    result = text.Remove(pos);
                //}
                sessionAudience.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pKey"].ToString() });
            }
            ViewBag.Audience = sessionAudience;
            return sessionAudience;
        }


        public ActionResult MySession(int ESPK, int SPK)
        {
            CheckLoginType();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            //int AccountPkey = 0;
            //User_Login data = new User_Login();
            //if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
            //{
            //    FormsIdentity identity = (FormsIdentity)User.Identity;
            //    data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            //    AccountPkey = data.Id;
            //}
            //ViewBag.AccountPkey = data.Id;
            int EventSessionPKEY = ESPK;
            int SessionPKEY = SPK;

            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);

            clsEvent cEvent = new clsEvent();
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.LoadEvent();

            clsVenue cVenue = new clsVenue();
            cVenue.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cVenue.intVenue_PKey = cEvent.intVenue_PKey;
            cVenue.LoadVenue();

            //**********************************************************
            //clsEventAccount cEventAccount = new clsEventAccount();
            //cEventAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            //cEventAccount.intEvent_pKey = EvtPKey;
            ////cEventAccount.intAccount_pKey = .cAccount.intAccount_PKey
            //cEventAccount.LoadEventInfo(true);
            string scheduledate = "", Location = "";
            clsEventSession cEventSession = new clsEventSession();
            cEventSession.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEventSession.intEventSession_PKey = EventSessionPKEY;  // Convert.ToInt32(ESPK);
            cEventSession.intEvent_PKey = EvtPKey;
            cEventSession.intSession_PKey = SessionPKEY;
            cEventSession.LoadEventSession((cEventSession.intEventSession_PKey <= 0));
            SessionPKEY = cEventSession.intSession_PKey;
            scheduledate = "Not yet scheduled";
            if (cEventSession.bIsScheduled)
                scheduledate = cEventSession.dtStartTime.ToLongDateString() + " " + cEventSession.dtStartTime.ToShortTimeString() + "-" + cEventSession.dtEndTime.ToShortTimeString();// + IIf(cEventSession.strRoomname <> "", " (Location: " + cEventSession.strRoomname + ")", "");
          
            if (cEventSession.strRoomname != "")
                Location = cEventSession.strRoomname;

            string SessionID = cEventSession.strSessionID.ToString();
            string Sessiontitle = cEventSession.strSessionTitle.ToString();
            string lblDescription = "";
            if (cEventSession.cSession.strDescription != "")
            {
                lblDescription = (!string.IsNullOrEmpty(cEventSession.cSession.strDescription) ? cEventSession.cSession.strDescription : cEventSession.strEventSpecificDescription);
            }
            string lblTopics = "";
            if (cEventSession.cSession.strProfInterests == "")
            {
                lblTopics = "N/A";
            }
            else
            {
                lblTopics = cEventSession.cSession.strProfInterests;
            }
            ViewBag.LO1  = ((cEventSession.strObjective1 == "") ? clsUtility.LineBreak(cEventSession.cSession.strObjective1) : cEventSession.strObjective1);
            ViewBag.LO2 = ((cEventSession.strObjective2 == "") ? clsUtility.LineBreak(cEventSession.cSession.strObjective2) : cEventSession.strObjective2);
            ViewBag.LO3 = ((cEventSession.strObjective3 == "") ? clsUtility.LineBreak(cEventSession.cSession.strObjective3) : cEventSession.strObjective3);

            ViewBag.strContent1  = ((cEventSession.strContent1 == "") ? clsUtility.LineBreak(cEventSession.cSession.strContent1) : cEventSession.strContent1);
            ViewBag.strContent2 = ((cEventSession.strContent2 == "") ? clsUtility.LineBreak(cEventSession.cSession.strContent2) : cEventSession.strContent2);
            ViewBag.strContent3 = ((cEventSession.strContent3 == "") ? clsUtility.LineBreak(cEventSession.cSession.strContent3) : cEventSession.strContent3);

            bool bAvailable = false, bBalanceDue = false;
            int intEventAccount_pKey = 0;
            ViewBag.cmdDownloadBookVisible =false;
            ViewBag.phNotAvailableVisible =false;
            ViewBag.lblNotAvailText = "";
            ViewBag.phLoggedInVisible= false;
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.phLoggedInVisible= true;
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                clsAccount cAccount = new clsAccount();
                cAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                cAccount.intAccount_PKey = data.Id;
                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                cEventAccount.intAccount_pKey = data.Id;
                cEventAccount.intEvent_pKey = data.EventId;
                cEventAccount.LoadEventInfo(true);
                intEventAccount_pKey = cEventAccount.intEventAccount_pKey;
                bBalanceDue = (cEventAccount.dblAccountBalance < 0);

                bAvailable =  (System.DateTime.Today >= cEventSession.cEvent.dtBookStartDate && System.DateTime.Today <= cEventSession.cEvent.dtBookEndDate);
                if (bAvailable && !bBalanceDue)
                {
                    ViewBag.cmdDownloadBookVisible =true;
                    ViewBag.phNotAvailableVisible =false;
                }
                else if (bAvailable && bBalanceDue)
                {
                    cAccount.LoadAccount();
                    ViewBag.phNotAvailableVisible =true;
                    ViewBag.lblNotAvailText =cEventSession.cEvent.ReplaceReservedWords(cAccount.ReplaceReservedWords(clsSettings.ReplaceTermsGeneral(cSettings.getText(clsSettings.TEXT_MyBookNotAvailableBalanceDue))));
                }
                else if (!bAvailable && !bBalanceDue)
                {
                    ViewBag.phNotAvailableVisible =true;
                    ViewBag.lblNotAvailText =cEventSession.cEvent.ReplaceReservedWords(cAccount.ReplaceReservedWords(clsSettings.ReplaceTermsGeneral(cSettings.getText(clsSettings.TEXT_MyBookNotAvailableNotReleased))));
                }
                else if (!bAvailable && bBalanceDue)
                {
                    ViewBag.phNotAvailableVisible =true;
                    ViewBag.lblNotAvailText = cEventSession.cEvent.ReplaceReservedWords(cAccount.ReplaceReservedWords(clsSettings.ReplaceTermsGeneral(cSettings.getText(clsSettings.TEXT_MyBookNotAvailableAll))));
                }
            }

            var relatedsessions = dba.RelatedSessionLink(SessionPKEY, EvtPKey);
            var speakerinfo = dba.RefreshSessionSpeaker(Convert.ToString(ESPK));
            ViewBag.SessionTitle = Sessiontitle;
            ViewBag.SessionId = SessionID;
            ViewBag.Description = lblDescription;
            ViewBag.EventDateTime = scheduledate;
            ViewBag.Topics = lblTopics;
            ViewBag.Speakers = speakerinfo;
            ViewBag.RelatedSessionLink = relatedsessions;
            //ViewBag.SessionInfo = sessioninfo;
            ViewBag.ESPK = 0;
            ViewBag.CurrentPageEventSessionPKEY = ESPK;

            bool btnDownloadWithSlidesVisible = (cEventSession.intEventSessionStatus_pKey == clsEventSession.STATUS_Released);

            ViewBag.btnDownloadWithSlidesVisible = btnDownloadWithSlidesVisible;
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";

            cEventSession = null;
            return View("~/Views/Events/SessionPage.cshtml");




            //********************************************************

            //  var sessioninfo = dba.MySessionDetails(ESPK, EvtPKey);
            // // var relatedSession = dba.RelatedSessionLink(ESPK,EvtPKey);
            ////  ViewBag.RelatedSession = relatedSession;
            //  foreach (var info in sessioninfo)
            //  {
            //      ViewBag.SessionTitle = info.Title;
            //      ViewBag.SessionId = info.SessionId;
            //      ViewBag.Description = info.Description;
            //      ViewBag.Speakers = info.Speakers;
            //      ViewBag.EventDateTime = info.EventDate;
            //      ViewBag.Topics = info.Topics;
            //      ViewBag.RelatedSession = info.ReletedSession;
            //      ViewBag.RelatedSessionLink = info.RelatedSessionLink;

            //  }
            //  ViewBag.ESPK = ESPK;
            //  ViewBag.SessionInfo = sessioninfo;
            //  return View("~/Views/Events/SessionPage.cshtml");
        }


        public ActionResult RelatedSession(string ESPK, int SPK)
        {
            CheckLoginType();
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
            if (User.Identity.AuthenticationType == "Forms")
            {
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);

                DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                ViewBag.CurrentTime = dtCurrentTime;
            }

            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            clsEvent cEvent = new clsEvent();
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.LoadEvent();

            clsEventSession cEventSession = new clsEventSession();
            cEventSession.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEventSession.intEventSession_PKey = Convert.ToInt32(ESPK);
            cEventSession.intEvent_PKey = EvtPKey;

            if (SPK == 0)
            {
                cEventSession.intSession_PKey = 0;
            }
            else
            {
                cEventSession.intSession_PKey = SPK;
            }
            if (cEventSession.intEventSession_PKey <= 0)
            {
                cEventSession.LoadEventSession(true);
            }
            else
            {
                cEventSession.LoadEventSession();
            }
            string scheduledate = "";
            if (cEventSession.bIsScheduled)
            {
                scheduledate = cEventSession.dtStartTime.ToLongDateString() + " " + cEventSession.dtStartTime.ToShortTimeString() + "-" + cEventSession.dtEndTime.ToShortTimeString();// + IIf(cEventSession.strRoomname <> "", " (Location: " + cEventSession.strRoomname + ")", "");
            }
            else
            {
                scheduledate = "Not yet scheduled";
            }

            if (cEventSession.strRoomname != "")
            {
                string Location = cEventSession.strRoomname;
            }

            string SessionID = cEventSession.strSessionID.ToString();
            string Sessiontitle = cEventSession.strSessionTitle.ToString();


            string lblDescription = "";
            if (cEventSession.cSession.strDescription != "")
            {

                lblDescription = cEventSession.strEventSpecificDescription + cEventSession.cSession.strDescription;
            }
            string lblTopics = "";
            if (cEventSession.cSession.strProfInterests == "")
            {
                lblTopics = "N/A";
            }
            else
            {
                lblTopics = cEventSession.cSession.strProfInterests;
            }
            var relatedsessions = dba.RelatedSessionLink(SPK, EvtPKey);
            var speakerinfo = dba.SessionSpeaker(ESPK);
            ViewBag.SessionTitle = Sessiontitle;
            ViewBag.SessionId = SessionID;
            ViewBag.Description = lblDescription;
            ViewBag.EventDateTime = scheduledate;
            ViewBag.Topics = lblTopics;
            ViewBag.Speakers = speakerinfo;
            ViewBag.RelatedSessionLink = relatedsessions;
            //ViewBag.SessionInfo = sessioninfo;
            ViewBag.ESPK = 0;

            cEventSession = null;
            return View("~/Views/Events/SessionPage.cshtml");

        }

        [CustomizedAuthorize]
        public ActionResult SynopsisCreateDownload(int intEventSession_pKey)
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);


            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);

            clsEvent cEvent = new clsEvent();
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.LoadEvent();

            clsVenue cVenue = new clsVenue();
            cVenue.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cVenue.intVenue_PKey = cEvent.intVenue_PKey;
            cVenue.LoadVenue();

            //clsEventSession cEventSession = new clsEventSession();
            //cEventSession.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            //cEventSession.intEvent_PKey = EvtPKey;
            //cEventSession.intEventSession_PKey = intEventSession_pKey;
            //cEventSession.intSession_PKey = 0;


            clsEventSession cEventSession = new clsEventSession();
            cEventSession.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEventSession.intEventSession_PKey = intEventSession_pKey;  // Convert.ToInt32(ESPK);
            cEventSession.intEvent_PKey = EvtPKey;

            // cEventSession.intSession_PKey = 0;
            // cEventSession.strSessionID = intEventSession_pKey.ToString();

            cEventSession.LoadEventSession();
            string strPhysicalPath = Server.MapPath("~/TempDocuments/TempRelease.pdf");
            string strFilename = cEventSession.strSessionID /*cEventSession.strTrackPrefix + cEventSession.cSession.strSessionID*/ + "_Synopsis_" + DateTime.Now.ToString("yyMMdd") + ".pdf";
            Aspose.Pdf.Document sessionpdf = clsUtility.CreateNewPDF(cEventSession.strSessionID, false);
            int intAttendeeSpeakerCount = dba.GetAttendeeAndPseakerCount(intEventSession_pKey, cLast.intActiveEventPkey);
            if (!cEventSession.CreateSessionSpeakerPage(sessionpdf, strPhysicalPath, false, intEventSession_pKey.ToString(), intAttendeeSpeakerCount))
            {
                return null;
            }
            if (clsUtility.FileExists(strPhysicalPath))
            {
                //  Byte[] bts = System.IO.File.ReadAllBytes(strPhysicalPath);              

                return Json(new { msg = "OK", strPhysicalPath, strFilename }, JsonRequestBehavior.AllowGet);
            }
            cEventSession = null;
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }
        [CustomizedAuthorize]
        public FileResult DownloadSynopsis(string FileName, string strPhysicalPath)
        {
            string strTargetFile = Server.MapPath("~/TempDocuments/TempRelease.pdf");
            if (System.IO.File.Exists(strTargetFile))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(strTargetFile);
                return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, FileName);

            }
            return null;

        }
        public ActionResult CreateSynopsisWithSlides(int intEventSession_pKey)
        {
            if (User.Identity.AuthenticationType == "Forms")
            {
                //  intEventSession_pKey = 14553;
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);

                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                int AccountPkey = data.Id;
                int EventPkey = data.EventId;

                clsEventSession cEventSession = new clsEventSession();
                cEventSession.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                cEventSession.intEventSession_PKey = intEventSession_pKey;  // Convert.ToInt32(ESPK);
                cEventSession.intEvent_PKey = EventPkey;
                cEventSession.LoadEventSession();

                string zipPath = Server.MapPath("~/app_data/BookPrepTemp/MSPDL_" + AccountPkey.ToString() + ".zip");
                string strSpeakerDocFolder = "~/SpeakerDocuments/" + EventPkey.ToString();
                if (clsUtility.FileExists(zipPath))
                {
                    if (!clsUtility.DeleteFile(zipPath, null))
                    {
                        return null;
                    }
                }

                int intZipNum = 0;
                ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);


                var dt = dba.GetSynopsisWithSlidesData(intEventSession_pKey);
                if (dt != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        ZipOneFile(dr, zipPath, archive, 0, strSpeakerDocFolder, false);
                    }

                }
                string strPhysicalPath = Server.MapPath("~/TempDocuments/TempRelease.pdf");
                string strFilename = cEventSession.strTrackPrefix + cEventSession.cSession.strSessionID + "_Synopsis_" + DateTime.Now.ToString("yyMMdd") + ".pdf";
                Aspose.Pdf.Document sessionpdf = clsUtility.CreateNewPDF(cEventSession.strSessionID, false);
                int intAttendeeSpeakerCount = dba.GetAttendeeAndPseakerCount(intEventSession_pKey, cLast.intActiveEventPkey); // GetAttendeeAndPseakerCount();
                if (!cEventSession.CreateSessionSpeakerPage(sessionpdf, strPhysicalPath, false, intEventSession_pKey.ToString(), intAttendeeSpeakerCount))
                {
                    return null;
                }
                if (clsUtility.FileExists(strPhysicalPath))
                {
                    archive.CreateEntryFromFile(strPhysicalPath, strFilename, CompressionLevel.Fastest);
                }
                archive.Dispose();
                string zipFilename = "MSPDL_" + AccountPkey.ToString() + ".zip";
                return Json(new { msg = "OK", zipFilename }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { msg = "ERROR" }, JsonRequestBehavior.AllowGet);


        }
        [CustomizedAuthorize]
        public FileResult DownloadSynopsisWithSlides(string FileName)
        {
            string strTargetFile = Server.MapPath("~/app_data/BookPrepTemp/" + FileName);
            if (System.IO.File.Exists(strTargetFile))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(strTargetFile);
                return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, "SynopsisWithSlides.zip");

            }
            return null;

        }
        public void ZipOneFile(DataRow drRow, string zipPath, ZipArchive archive, int intfolderSelect, string SpeakerDocfolder, bool bSynopsis = false)
        {
            string strSessionID = "";
            strSessionID = drRow["SessionID"].ToString();

            string strDisplayName = bSynopsis ? strSessionID + " Synopsis " + DateTime.Now.ToString("yyMMdd") : "MSP_" + strSessionID + "-" + drRow["DocName"].ToString();
            string strFilename = drRow["DocFileName"].ToString();
            string strPhysicalPath = "";
            if (bSynopsis)
            {
                strPhysicalPath = SpeakerDocfolder;
            }
            else
            {
                strPhysicalPath = Server.MapPath(SpeakerDocfolder + "/" + strFilename);
            }
            string fileext = Path.GetExtension(strPhysicalPath);
            strDisplayName = strDisplayName + fileext;

            switch (intfolderSelect)
            {
                case 0:
                    if (clsUtility.FileExists(strPhysicalPath))
                    {
                        archive.CreateEntryFromFile(strPhysicalPath, strDisplayName, CompressionLevel.Fastest);
                    }
                    break;
            }

        }
        public ActionResult DownloadProgram(bool ckShowSpeak, bool ckShowRelated, bool ckShowEdu, bool ckBlurb)
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);


            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            string strFinalExtension = "";
            int offSet = 0;
            //if (Request.Cookies("yjnf") != null)
            //{
            //    Int32.TryParse(Request.Cookies("yjnf").Value, offSet);
            //}
            clsEvent c = new clsEvent();
            c.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            c.lblMsg = null;
            c.intEvent_PKey = EvtPKey;//   myVS.intProgramToViewEvent_pkey
            c.LoadEvent();


            c.bFileSpeakers = ckShowSpeak;
            c.bFileRelatedSessions = ckShowRelated;
            c.bFileEdLevel = ckShowEdu;

            c.bShowDescription = ckBlurb;
            c.bShowTBD = ckShowSpeak;  // myVS.bDefaultSpkShow;
            c.bFileColumnar = true;
            c.intFileType = clsUtility.FileType_PDF;
            c.bFileLandscape = false;
            c.bShowRoom = false;

            User_Login data = new User_Login();
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            }
            clsAccount cAccount = new clsAccount();
            cAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cAccount.intAccount_PKey = data.Id;
            cAccount.LoadAccount();

            string strFilename = c.strEventID + "_" + cAccount.intAccount_PKey.ToString() + "_" + DateTime.Now.Ticks.ToString() + "_Program.pdf";
            string strDisplayFileName = c.strEventID + "_Program.pdf";
            string strTargetFile = Server.MapPath("~/App_Data/DownloadTemp/" + strFilename);
            //  if (Request.Cookies("yjnf") == null) 
            {
                if (!c.CreateStandardProgram(strTargetFile))
                {
                    return null;
                }
                else
                {
                    if (!c.CreateStandardProgramWithLocalTime(strTargetFile, (-1) * offSet))
                    {
                        return null;
                    }
                }
                c = null;
                Byte[] bts = System.IO.File.ReadAllBytes(strTargetFile);
                return Json(new { msg = "OK", strFilename, strDisplayFileName }, JsonRequestBehavior.AllowGet);
                // return File(bts, System.Net.Mime.MediaTypeNames.Application.Octet, strDisplayFileName);
            }

            return null;
        }
        public FileResult DownloadProgramFile(string FileName, string strDisplayFileName)
        {
            string strTargetFile = Server.MapPath("~/App_Data/DownloadTemp/" + FileName.Trim());
            if (System.IO.File.Exists(strTargetFile))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(strTargetFile);
                return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, strDisplayFileName);

            }
            return null;

        }
        public ActionResult FutureEvent(int Event_Pkey)
        {
            CheckLoginType();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            int EvtPKey = Event_Pkey;
            clsEvent cEvent = new clsEvent();
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.intEvent_PKey = EvtPKey;
            cEvent.LoadEvent();

            clsVenue cVenue = new clsVenue();
            cVenue.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cVenue.intVenue_PKey = cEvent.intVenue_PKey;
            cVenue.LoadVenue();

            DataTable eventinfo = new DataTable();

            eventinfo = dba.FETCHEventData(EvtPKey);
            //var currentevent =   dba.CurrentEventInfo(EvtPKey);
            var currentevent = cEvent.dtStartDate.ToString("MMMM") + " " + cEvent.dtStartDate.Day.ToString() + "-" + (cEvent.dtEndDate.ToString("MMMM") != cEvent.dtStartDate.ToString("MMMM") ? cEvent.dtEndDate.ToString("MMMM") + " " : "") + cEvent.dtEndDate.Day.ToString() + ", " + cEvent.dtStartDate.ToString("yyyy");
            if (eventinfo.Rows.Count <= 0)
            {
                ViewBag.lblNoInfoText = cSettings.getText(clsSettings.TEXT_EventInfo);

            }

            List<EventsInfo> _eventinfo = new List<EventsInfo>();
            foreach (DataRow info in eventinfo.Rows)
            {
                string res = "";
                if (info["SectionText"].ToString() != "")
                {
                    res = clsReservedWords.ReplaceMyPageText(null, info["SectionText"].ToString(), cEvent, cVenue);
                }
                _eventinfo.Add(new EventsInfo
                {
                    SectionTitle = info["SectionTitle"].ToString(),
                    SectionText = res // info["SectionText"].ToString()
                });

            }
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            ViewBag.PageTitle = cEvent.strEventFullname;
            ViewBag.EventInfo = _eventinfo;
            ViewBag.CurrentEvent = currentevent;

            string formpopupISMAGIRight = "Is MAGI Right For You?";
            string formtextISRight = dba.FETCHFormData(formpopupISMAGIRight);
            string resultpopuptextISRight = "";
            if (formtextISRight != "")
            {
                resultpopuptextISRight = clsReservedWords.ReplaceMyPageText(null, formtextISRight, cEvent, cVenue);
            }
            ViewBag.PopUPText =
            ViewBag.FormText = resultpopuptextISRight;

            string popupform_TC = "Event Terms & Conditions";
            string popuptext_TC = dba.FETCHFormData(popupform_TC);
            string resultpopuptext_TC = "";
            if (popuptext_TC != "")
            {
                resultpopuptext_TC = clsReservedWords.ReplaceMyPageText(null, popuptext_TC, cEvent, cVenue);
            }
            ViewBag.FormTextT_C = resultpopuptext_TC;
            BindOverViewDropDown();
            return View("~/Views/Events/EventsInfo.cshtml");
        }
        public void BindOverViewDropDown()
        {
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);

            var dtdd = dba.RefreshEvent(cLast.intActiveEventPkey);
            var ddOverview = dba.EventPagesDropDown();
            bool bShowEvtPages = false;
            if (dtdd.Rows.Count > 0)
            {
                bShowEvtPages = dtdd.Rows[0]["ShowEventPages"].ToString() == "1" ? true : false;
                string strPartnerAlias = dtdd.Rows[0]["PartnerAlias"].ToString();

                ddOverview.Where(p => p.Value == "4").First().Text = ddOverview.Where(p => p.Value == "4").First().Text.Replace("Partner", strPartnerAlias);
                Regex expr = new Regex(@"\b[aeiou]\w*");
                if (ddOverview.Where(p => p.Value == "5").First().Text.Contains("a Partner") && expr.IsMatch(strPartnerAlias.ToLower()))
                    ddOverview.Where(p => p.Value == "5").First().Text = ddOverview.Where(p => p.Value == "5").First().Text.Replace("a Partner", "an " + strPartnerAlias);
                else
                    ddOverview.Where(p => p.Value == "5").First().Text = ddOverview.Where(p => p.Value == "5").First().Text.Replace("Partner", strPartnerAlias);
            }
            if (cLast.intEventType_PKey == clsEvent.EventType_CloudConference || cLast.intEventType_PKey == clsEvent.EventType_HybridConference)
            {
                if (bShowEvtPages && Request.Path != "/MAGI/Overview")
                    ddOverview.Add(new SelectListItem { Text = "Enter the Event", Value = "9" });
            }
            else
                ddOverview.Add(new SelectListItem { Text = "Venue & Lodging", Value = "6" });

            string Host = "/"+ HttpContext.Request.Path.Split('/')[2].ToUpper();
            string SelectedValue = GetSelectedOverViewDropDown(Host);
            ddOverview.Where(p => p.Value == SelectedValue).First().Selected = true;
            ViewBag.OverViewDropDown = ddOverview;
            ViewBag.OverViewDropDownSelected = SelectedValue;
        }
        private string GetSelectedOverViewDropDown(string Host)
        {
            string SelectedValue = "0";
            switch (Host)
            {
                case "/EVENTINFO":
                    SelectedValue = "0";
                    break;
                case "/PROGRAM":
                    SelectedValue = "1";
                    break;
                case "/SPEAKERS":
                    SelectedValue = "2";
                    break;
                case "/BEASPEAKER":
                    SelectedValue = "3";
                    break;
                case "/EVENTSPONSORS":
                    SelectedValue = "4";
                    break;
                case "/BECOMESPONSOR":
                    SelectedValue = "5";
                    break;
                case "/VENUEINFO":
                    SelectedValue = "6";
                    break;
                case "/REGISTRATION":
                    SelectedValue = "7";
                    break;
                case "/CONTACT":
                case "/EVENTCONTACT":
                    SelectedValue = "8";
                    break;

                default:
                    {
                        SelectedValue = "0";
                        break;
                    }
            }
            return SelectedValue;
        }

    }
}
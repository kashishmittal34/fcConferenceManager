using fcConferenceManager.Models;
using MAGI_API.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace fcConferenceManager.Controllers
{
    [CheckActiveEventAttribute]
    public class MAGIController : Controller
    {
        //
        // GET: /MAGI/
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
        public ActionResult Index()
        {
            return View("TableForm");
        }

        public void CheckLoginType()
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

        [HttpGet]
        public ActionResult JoinMAGI()
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
            string formname = "Join MAGI";
            string formtext = dba.FETCHFormData(formname);
            ViewBag.PageTitle = formname;
            ViewBag.FormText = formtext;
            return View("~/Views/MAGI/MAGI.cshtml");

        }

        public ActionResult Overview(int? M = null)
        {
            switch (M)
            {
                case 2: return RedirectToAction("TermsOfUse", "MAGI");
                case 32: return RedirectToAction("ContinueEducation", "Event");
                case 30: return RedirectToAction("BecomeSponsor", "Event");
                case -1: return RedirectToAction("EventTermsandConditions", "Event");
                case 29: return RedirectToAction("IsMAGIRightForYou", "Event");
            }

            CheckLoginType();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);

            clsEvent cEvent = new clsEvent();
            cEvent.intEvent_PKey = EvtPKey;
            clsVenue cVenue = new clsVenue();
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cVenue.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();
            cVenue.LoadVenue();

            clsAccount cAccount = ((clsAccount)Session["cAccount"]);
            if (M == null)
                M = clsSettings.TEXT_OverviewPage;

         
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
            string Host = HttpContext.Request.Path.ToUpper().Replace("/MAGI", "");
            string SelectedValue = GetSelectedOverViewDropDown(Host);
            ddOverview.Where(p => p.Value == SelectedValue).First().Selected = true;
            ViewBag.OverViewDropDown = ddOverview;
            ViewBag.OverViewDropDownSelected = SelectedValue;


            ViewBag.ctlEventDropDownVisible = (Request.QueryString["A"] == null);
            ViewBag.phPRelationsVisible = false;
            bool bEventInfo = false;
            bool phPRelationsVisible = false;
            string lblTitle = "MAGI Overview";
            switch (M)
            {
                case clsSettings.TEXT_RightForYou:
                    lblTitle = "Is MAGI Right for you?";
                    break;
                case clsSettings.TEXT_BeAPartner:
                    phPRelationsVisible = false;
                    lblTitle = "Become a Partner";
                    break;
                case clsSettings.TEXT_ChairmanLetter:
                    lblTitle = "Letter from the Chairman";
                    break;
                case clsSettings.TEXT_ContactHours:
                    lblTitle = "Continuing Education Contact Hours";
                    break;
                case clsSettings.TEXT_Bio:
                    lblTitle = "Founder's Biography";
                    break;
                case clsSettings.TEXT_MoreInfo:
                    lblTitle = "More Information about MAGI";
                    break;
                case clsSettings.TEXT_MyFeedbackReference:
                    lblTitle = "Speaking at a MAGI Conference";
                    break;
                case clsSettings.Text_ExhibitorPInformation:
                    lblTitle = "Exhibitor Information";
                    bEventInfo = true;
                    break;
                case clsSettings.Text_PartnerExhibitInfo:
                    lblTitle = "Exhibitor Information";
                    bEventInfo = true;
                    break;
                case clsSettings.Text_NonExhibitorPartnerInstruct:
                    lblTitle = "Non-Exhibitor Information";
                    bEventInfo = true;
                    break;
                case clsSettings.Text_MediaPInformation:
                    lblTitle = "Media Partner Information";
                    bEventInfo = true;
                    break;
                case clsSettings.Text_OtherPInformation:
                    lblTitle = "Other Partner Information";
                    bEventInfo = true;
                    break;

                case clsSettings.Text_BrandingPInformation:
                    lblTitle = "Branding Partner Information";
                    bEventInfo = true;
                    break;
                case clsSettings.Text_RegErrorStartOver:
                    lblTitle = "Registration Instruction";
                    break;
                case clsSettings.Text_RegErrorPriorStep:
                    lblTitle = "Registration Instruction";
                    break;
                case clsSettings.Text_RegErrorPaymentStep:
                    lblTitle = "Registration Instruction";
                    break;
                case clsSettings.TEXT_MBRSAbout:
                    lblTitle = "MAGI Blue Ribbon Sites";
                    break;
                case clsSettings.TEXT_SpeakerConferenceCountText:
                    lblTitle = "Get Started";
                    break;
                case clsSettings.TEXT_PrivacyTerms:
                    lblTitle = "Terms of Use";
                    bEventInfo = true;
                    break;
                case clsSettings.TEXT_ConfTerms:
                    lblTitle = "Event Terms & Conditions";
                    bEventInfo = true;
                    break;


            }
            string cmdPMgr = "Partner Relations";
            Regex exp = new Regex(@"\b[aeiou]\w*");
            if (lblTitle.Contains("a Partner") && exp.IsMatch(cEvent.strPartnerAlias.ToLower()))
            {
                lblTitle = lblTitle.Replace("a Partner", "an " + cEvent.strPartnerAlias);
            }
            else
            {
                lblTitle = lblTitle.Replace("Partner", cEvent.strPartnerAlias);
            }
            if (phPRelationsVisible == true)
            {
                cmdPMgr = cmdPMgr.Replace("Partner", cEvent.strPartnerAlias);
            }
            ViewBag.phPRelations = phPRelationsVisible;
            ViewBag.cmdPMgr = cmdPMgr;
            string lblContent = "";
            int intMode = Convert.ToInt32(M);
            if (bEventInfo)
                lblContent = clsReservedWords.ReplaceCurrent(null, clsSettings.ReplaceTermsGeneral(cSettings.getText(intMode)));
            else
            {
                if (intMode == 46)
                    cSettings.intMagiSupportM46 = 46;
                else
                    cSettings.intMagiSupportM46 = 0;

                lblContent = clsReservedWords.ReplaceCurrent(null, clsSettings.ReplaceTermsGeneral(cSettings.getText(intMode)));
            }
            bool phGuaranteeVisible = (intMode == clsSettings.TEXT_RightForYou);
            ViewBag.phGuaranteeVisible = phGuaranteeVisible;
            ViewBag.lblPopTextTitle = "Registration Terms and Conditions";
            ViewBag.PopUpText = cSettings.getText(clsSettings.TEXT_ConfTerms);

            bool btnEditVisible = false;
            if (cAccount != null)
                btnEditVisible = cAccount.isAllowedByEntity(clsUtility.TYPE_Account, clsPrivileges.PAGE_QuickEdit) && cSettings.bShowGreenEditButton;
            ViewBag.mode = intMode;
            ViewBag.PageTitle = lblTitle;
            ViewBag.btnEditVisible = btnEditVisible;
            string formtext = lblContent;
            string resultformtext = "";

            // string formname = "Overview Page";
            // string formtext =   dba.FETCHFormData(formname);
            //string resultformtext = "";
            // ViewBag.PageTitle = "MAGI Overview";

            if (formtext != "")
            {
                resultformtext = clsReservedWords.ReplaceMyPageText(null, formtext, cEvent, cVenue);
            }
            ViewBag.FormText = resultformtext;
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            return View();

            //return View("~/Views/MAGI/TermsOfUse.cshtml");

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
            return View("~/Views/Events/Certification.cshtml");

        }
        public ActionResult UserIssue()
        {
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            CheckLoginType();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            string username = "";
            string email = "";
            int AccountPkey = 0;
            if (User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                username = data.UserName;
                email = data.Email;
                AccountPkey = data.Id;


            }
            ViewBag.UserName = username;
            ViewBag.Email = email;
            string formname = "Issue Instructions";
            string formtext = dba.FETCHFormData(formname);
            ViewBag.Message = "";
            IssueType_Bind();
            ViewBag.PageTitle = "Report an Issue";
            ViewBag.FormText = formtext;

            clsAccount cAccount = new clsAccount();
            cAccount.intAccount_PKey = AccountPkey;
            cAccount.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cAccount.LoadAccount();
            int MaxFileSize = 0;
            string lblSizeLimit = "";
            if (cAccount.bGlobalAdministrator)
            {
                MaxFileSize = 1073741824;
                lblSizeLimit = "Size limit: 1GB";
            }
            else
            {
                MaxFileSize = cSettings.intUploadSize;
                lblSizeLimit = "Size limit: " + clsSettings.FileSize(cSettings.intUploadSize);
            }
            //int MaxFileInputsCount = 999;
            //.AllowedFileExtensions = clsUtility.getAllowedGeneralandZipExtensionsArray()
            //.TargetFolder = Server.MapPath("~/UserDocuments")
            ViewBag.lblExtensions = "Allowed extensions: " + String.Join(", ", clsUtility.getAllowedGeneralandZipExtensionsArray());
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            ViewBag.lblSizeLimit = lblSizeLimit;

            return View();
            //TempData["Message"] = "You clicked Save!";
            //return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Contact()
        {
            CheckLoginType();

            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            clsEvent cEvent = new clsEvent();
            clsVenue cVenue = new clsVenue();
            cEvent.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cVenue.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cEvent.LoadEvent();
            cVenue.LoadVenue();

            string formname = "Contact MAGI";
            string formtext = dba.FETCHFormData(formname);
            string resultformtext = "";
            ViewBag.PageTitle = formname;

            if (formtext != "")
            {
                resultformtext = clsReservedWords.ReplaceMyPageText(null, formtext, cEvent, cVenue);
            }
            ViewBag.FormText = resultformtext;
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            return View("~/Views/MAGI/MAGI.cshtml");

        }

        [HttpGet]
        public ActionResult TermsOfUse()
        {
            CheckLoginType();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
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
            string Host = HttpContext.Request.Path.ToUpper().Replace("/EVENTS", "");
            string SelectedValue = GetSelectedOverViewDropDown(Host);
            ddOverview.Where(p => p.Value == SelectedValue).First().Selected = true;
            ViewBag.OverViewDropDown = ddOverview;
            ViewBag.OverViewDropDownSelected = SelectedValue;

            string formname = "Website Terms of Use";
            string formtext = dba.FETCHFormData(formname);
            ViewBag.PageTitle = "Terms of Use";
            ViewBag.FormText = formtext;
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            return View(); //"~/Views/MAGI/MAGI.cshtml"

        }

        [HttpPost]
        [ValidateInput(true)]
        //[AjaxValidateAntiForgeryToken]
        public JsonResult SubmitIssueImages()
        {
            try
            {
                string strFileGUID = Request.Form["GUID"].ToString();
                string strFolder = Server.MapPath("~/UserDocuments/");
                string strIssueFiles = "";
                if (Request.Files.Count != 0)
                {
                    foreach (string key in Request.Files.AllKeys)
                    {
                        HttpPostedFileBase fileData = Request.Files[key];
                        if (fileData != null)
                        {
                            string strPhysicalPath = strFolder + strFileGUID + "_" + fileData.FileName;
                            fileData.SaveAs(strPhysicalPath);
                            strIssueFiles = ((string.IsNullOrEmpty(strPhysicalPath)) ? strPhysicalPath : strIssueFiles + ";" + strPhysicalPath);
                        }
                    }
                    return Json(new { result = "OK", Data = strFileGUID, filePath = strIssueFiles }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { result = "Select a file and try again" }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception e)
            {

            }
            return Json(new { result = "Error occurred while updating files" }, JsonRequestBehavior.AllowGet);
        }
        private bool InsertIssue(ReportIssue ri, int AccountPkey, string strContactName, string strFolder, out int intIssuePKey)
        {
            try
            {
                intIssuePKey = 0;
                clsIssue cIssue = new clsIssue();
                cIssue.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                cIssue.lblMsg = null;
                cIssue.intIssue_PKey = ri.intIssue_PKey;
                cIssue.intIssueArea_pKey = 365;
                cIssue.strIssueName = ri.Issuetitle;
                cIssue.strFormName = ri.Issuelocation;
                cIssue.strDescription = ri.IssueDetail + "<br/> <br/>Issue:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; " + ri.Issuetitle + "<br/> Submitter: " + ri.IssueReportedbyUser + "<br/>Email:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + ri.UserEmail;
                cIssue.strPageUrl = ri.Issuelocation;
                cIssue.intIssueStatus_pkey = clsIssue.STATUS_ToDo;
                cIssue.intIssueCategory_pkey = Convert.ToInt32(ri.IssueType);
                cIssue.intIssueType_pkey = 0;
                cIssue.intIssuePriority_pkey = clsIssue.PRIORITY_Normal;
                cIssue.dtEnteredOn = DateTime.Now;
                cIssue.strFileGUID = ri.UID;
                if (AccountPkey > 0)
                {
                    cIssue.intEnteredByAccount_pkey = AccountPkey;
                    cIssue.strEnteredByAccount = strContactName;
                }

                bool res = cIssue.SaveIssue(cIssue);
                if (res)
                    intIssuePKey = cIssue.intIssue_PKey;

                return res;
            }
            catch
            {
                intIssuePKey = 0;
            }

            return false;
        }

        [HttpPost]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult SubmitIssueReport(ReportIssue ri)
        {
            try
            {
                int AccountPkey = 0;
                string strFolder = Server.MapPath("~/UserDocuments/"), username = "", lblTYMsg = "";
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                if (string.IsNullOrEmpty(ri.Issuetitle))
                    return Json(new { result = "Error", msg = "Enter issue" }, JsonRequestBehavior.AllowGet);
                if (string.IsNullOrEmpty(ri.IssueReportedbyUser))
                    return Json(new { result = "Error", msg = "Enter user name" }, JsonRequestBehavior.AllowGet);
                if (string.IsNullOrEmpty(ri.UserEmail))
                    return Json(new { result = "Error", msg = "Enter email" }, JsonRequestBehavior.AllowGet);

                if (!clsEmail.IsValidEmail(ri.UserEmail))
                    return Json(new { result = "Error", msg = "Enter valid email" }, JsonRequestBehavior.AllowGet);

                if (User.Identity.AuthenticationType == "Forms")
                {
                    FormsIdentity identity = (FormsIdentity)User.Identity;
                    User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                    AccountPkey = data.Id;
                    username = data.UserName;
                }
                ri.Issuelocation = (string.IsNullOrEmpty(ri.Issuelocation)) ? "" : ri.Issuelocation;
                ri.IssueDetail = (string.IsNullOrEmpty(ri.IssueDetail)) ? "" : ri.IssueDetail;

                int intIssuepKey = 0;
                bool insert = InsertIssue(ri, AccountPkey, username, strFolder, out intIssuepKey);

                if (insert)
                    ViewBag.Message = "Thank you for telling us about your issue so we can improve the website";
                if (ri.IssueDetail == "" && ri.files == null)
                    lblTYMsg = cSettings.getText(clsSettings.Text_IssueThankyouText);
                else if (ri.files == null)
                    lblTYMsg = cSettings.getText(clsSettings.Text_IssueThankYouURLText);
                else if (ri.IssueDetail == "")
                    lblTYMsg = "Thank you for telling us about your issue so we can improve the website. If you can enter the page URL, it would be very helpful.";
                else
                    lblTYMsg = "Thank you for telling us about your issue so we can improve the website.";

                return Json(new { result = "OK", lblTYMsg, intIssuepKey }, JsonRequestBehavior.AllowGet);
            }
            catch
            {

            }
            return Json(new { result = "Error", msg = "Error Occurred While Updating Issue" }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateInput(true)]
        [AjaxValidateAntiForgeryToken]
        public JsonResult SubmitIssueMail(ReportIssue ri)
        {
            try
            {
                clsEmail cEmail = new clsEmail();
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                cEmail.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
                cEmail.lblMsg = null;
                cEmail.strEmailUserName = ri.IssueReportedbyUser.Trim();
                cEmail.strEmailFromAddress = ri.UserEmail.Trim();

                string strSubject = ri.Issuetitle.Trim();
                if (string.IsNullOrEmpty(strSubject))
                    strSubject = "[User Issue]";

                cEmail.strSubjectLine = strSubject;
                string CC = new SqlOperation().GetSecurityGroupEmail();
                string strContent = ri.IssueDetail;
                cEmail.strEmailCC = "#";
                cEmail.strEmailBCC = ((CC != "") ? CC.Replace(",", ";") : "#");
                cEmail.strHTMLContent = ((ri.intIssue_PKey > 0) ? "#" + ri.intIssue_PKey.ToString() + "<br/>" : "") + strContent +
                                        "<br/> Submitter: " + ri.IssueReportedbyUser.Trim() + "<br/>Email:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + ri.UserEmail.Trim();
                cEmail.strAttachmentPath = ri.strIssueFiles;

                if (!cEmail.SendEmailToAddress(cSettings.strSupportEmail, bPlainText: false))
                    return Json(new { result = "Error", msg = "Error Occurred While Submitting Issue" }, JsonRequestBehavior.AllowGet);

                return Json(new { result = "OK" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {

            }
            return Json(new { result = "Error", msg = "Error Occurred While Updating Issue" }, JsonRequestBehavior.AllowGet);


        }

        public void BindTitle()
        {

        }
        public void IssueType_Bind()
        {
            DataTable ds = dba.GetIssueType();
            List<SelectListItem> issuetypelist = new List<SelectListItem>();
            foreach (DataRow dr in ds.Rows)
            {
                issuetypelist.Add(new SelectListItem { Text = dr["IssueType"].ToString(), Value = dr["pKey"].ToString() });
            }
            ViewBag.IssueType = issuetypelist;
        }

        public ActionResult ProgramManager()
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);

            int EvtPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            cLast.colNotifications.Clear();
            cLast.bNotificationSupport = true;
            clsOneNotify c = new clsOneNotify();
            c.strEmailAddress = clsEvent.getEventContactEmail(EvtPKey, 4);
            cLast.colNotifications.Add(c);
            c = null;

            bool bLoggedIn = false;
            clsAccount cAccount = ((clsAccount)Session["cAccount"]);
            if (cAccount != null)
            {
                if (cAccount.intAccount_PKey > 0)
                    bLoggedIn = true;
            }
            if (bLoggedIn)
                cLast.strNotificationContent = cAccount.ReplaceReservedWords(cSettings.getText(clsSettings.Text_PartnerInfoLoggedIn));
            else
                cLast.strNotificationContent = cSettings.getText(clsSettings.Text_PartnerInfoNotLoggedIn);

            string strSubject = "Interested in becoming a Partner";
            string url = "frmSendEmail.aspx?S=" + HttpUtility.UrlEncode(strSubject);
            return Json(new { result = "OK", url }, JsonRequestBehavior.AllowGet);
        }

        [CustomizedAuthorize]
        public ActionResult ChatModerator()
        {
            if (User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                
                if(data.GlobalAdmin)
                {
                    GroupChats groupChats = new GroupChats();
                    string MyID = data.Id.ToString();
                    string eventKey = data.EventId.ToString();

                    if(!string.IsNullOrEmpty(data.EventCodeName))
                        groupChats.DefaultEventChat = ChatOperations.getChatHistoryByPerson(MyID, null, eventKey, data.EventCodeName);

                    return View(groupChats);
                }
            }

            return RedirectToAction("Index","Home");
        }

        [ValidateInput(true)]
        public JsonResult OverViewDropdownSelected(int DropDownValue)
        {
            User_Login data = new User_Login();
            string evtString = Request.QueryString["EVT"] == null ? "" : "?EVT=" + Request.QueryString["EVT"].ToString();
            string URL = "", actionType = "Redirect";
            switch (DropDownValue)
            {
                case 0: URL = "/Events/EventInfo"; break;
                case 1: URL = "/Events/Program"; break;
                case 2: URL = "/Events/Speakers"; break;
                case 3: URL = "/Events/BeASpeaker"; break;
                case 4: URL = "/Events/EventSponsors"; break;
                case 5: URL = "/Events/BecomeSponsor"; break;
                case 6: URL = "/Events/VenueInfo"; break;
                case 7: URL = "/Registration"; break;
                case 8: URL = "/Events/EventContact"; break;
                case 9:
                    if (data.Id == 0)
                    {
                        return Json(new { msg = "Alert", ActionType = "Alert", msgTxt = "Sign in and enter the event" }, JsonRequestBehavior.AllowGet);
                    }
                    else

                        URL = "/Virtual/EventOnCloud";
                    break;
            }
            return Json(new { msg = "OK", RedirectionUrl = URL, ActionType = actionType }, JsonRequestBehavior.AllowGet);
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

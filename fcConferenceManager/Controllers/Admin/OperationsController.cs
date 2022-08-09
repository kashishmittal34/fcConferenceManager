using ClosedXML.Excel;
using fcConferenceManager.Models;
using Kendo.Mvc.UI;
using MAGI_API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace fcConferenceManager.Controllers
{
    [CheckActiveEventAttribute]
    [CustomizedAuthorize]
    public class OperationsController : Controller
    {
        private const string DATE_All = "0";
        private const string DATE_Today = "1";
        private const string DATE_ThisWeek = "2";
        private const string DATE_FPastMonth = "4";
        private const string DATE_Other = "3";
        private const string DATE_All2 = "0";
        private const string DATE_PastYear = "1";
        private const string DATE_PastMonth = "2";
        private const string DATE_PastWeek = "3";
        private const string DATE_PastDay = "4";
        private const string DATE_Other2 = "5";
        private const string DATE_Other3 = "4";
        private const string DATE_All3 = "0";
        private const string DATE_NPastMonth = "1";
        private const string DATE_NPastWeek = "2";
        private const string DATE_NPastDay = "3";
        private const string DATE_FPastDay = "5";
        private const string DATE_FPastWeek = "6";
        private const string DATE_FPastYear = "7";

        public enum PopUpType
        {
            BrowserSupport = 0,
            EventInfo = 1
        }

        static CommonOperations repository = new CommonOperations();
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
        private bool isAllowedByEntity(bool Admin, bool blueRibbonMode, int intEntity, int intFunction, DataTable dt)
        {
            bool b = Admin;
            if (!blueRibbonMode && dt != null && dt.Rows.Count>0)
                b = (Admin || dt.Select("ColKey = '" + "E" + intEntity.ToString() + "-" + intFunction.ToString() + "'").Count() >0);
            return b;
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
        private List<SelectListItem> ConvertToKendoDropDownList(DataTable dt, DataRow[] dr = null)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            try
            {
                if (dt!=null && dt.Rows.Count>0)
                    return dt.AsEnumerable().Select(x => new SelectListItem() { Text=((x["strText"] != DBNull.Value) ? x["strText"].ToString() : ""), Value=x["pKey"].ToString() }).ToList<SelectListItem>();

                if (dr!=null && dr.Length>0)
                    return dr.AsEnumerable().Select(x => new SelectListItem() { Text=((x["strText"] != DBNull.Value) ? x["strText"].ToString() : ""), Value=x["pKey"].ToString() }).ToList<SelectListItem>();
            }
            catch (Exception ex)
            {
            }
            return list;
        }
        [ValidateInput(true)]
        public JsonResult GetSpeakerFlagGrouped([DataSourceRequest] DataSourceRequest request)
        {
            DataTable dt = repository.FetchGlobalFilters(27);
            if (dt == null)
            {
                dt.Columns.Add("strText", typeof(string));
                dt.Columns.Add("pkey", typeof(int));
                dt.Columns.Add("sortorder", typeof(int));
                dt.Columns.Add("IsHeader", typeof(bool));
                dt.Columns.Add("headervalue", typeof(string));
                dt.Columns.Add("Level", typeof(string));
                dt.Columns.Add("groupId", typeof(string));
            }
            DataRow dr = dt.NewRow();
            dr["strText"] = "Any Flag"; dr["pkey"] = "-2";
            dt.Rows.InsertAt(dr, 0);
            dr = dt.NewRow();
            dr["strText"] = "None"; dr["pkey"] = "0";
            dt.Rows.InsertAt(dr, 0);
            dr = dt.NewRow();
            dr["strText"] = "Ignore"; dr["pkey"] = "-1";
            dt.Rows.InsertAt(dr, 0);
            var json = Json(dt.AsEnumerable().Select(p => new { strText = p["strText"], pKey = p["pkey"], groupId = ((p["groupId"] == DBNull.Value) ? "" : p["groupId"]) }), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength=int.MaxValue;
            return json;
        }
        // GET: Operations
        public ActionResult SpeakerManagement()
        {
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            ViewBag.PageTitle = "Speaker Management (People)";
            ViewBag.RATING_EXCELLENT =  clsFeedback.RATING_EXCELLENT;
            ViewBag.RATING_GOOD = clsFeedback.RATING_GOOD;
            ViewBag.RATING_FAIR  =clsFeedback.RATING_FAIR;
            ViewBag.RATING_POOR  =clsFeedback.RATING_POOR;
            ViewBag.RATING_AWFUL =clsFeedback.RATING_AWFUL;

            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            DataTable dt = new SqlOperation().LoadAccountPrivilages(data.EventId, data.Id);
            if (!isAllowedByPriv(false, data.GlobalAdmin, dt, "", "", clsPrivileges.PAGE_SpeakerMgt))
                return RedirectToAction("Index", "Home");
            //dpStart2.SelectedDate = cAccount.OtherDate
            //ddDateRange.SelectedValue = cAccount.IntFilterID
            ViewBag.IsCollapse =false;
            string pre = "";
            if (Request.UrlReferrer != null)
            {
                pre = Request.UrlReferrer.AbsolutePath.ToString();
                ViewBag.IsCollapse =true;
            }
            //myVS.intCurEventPKey = Me.cLast.intActiveEventPkey
            //myVS.strCurEventID = Me.cLast.strActiveEvent

            DataTable AssigmentStatus = repository.FetchGlobalFilters(1);
            ViewBag.ddCountry = ConvertToKendoDropDownList(repository.FetchGlobalFilters(23));          // All Countries Except 1,2,3
            ViewBag.ddEditStatus = ConvertToKendoDropDownList(AssigmentStatus);                         // All Assignment Statuses
            ViewBag.ddSesStatus = new List<SelectListItem>();
            ViewBag.ddStatus = new List<SelectListItem>();
            if (AssigmentStatus != null && AssigmentStatus.Rows.Count>0)
            {
                ViewBag.ddSesStatus = ConvertToKendoDropDownList(null, AssigmentStatus.Select("active =1"));
                ViewBag.ddStatus = ConvertToKendoDropDownList(null, AssigmentStatus.Select("OnSpeakerContact=1 AND active=1"));
            }

            ViewBag.ddStatus =  ConvertToKendoDropDownList(repository.FetchGlobalFilters(2));           // All Participation Statuses
            ViewBag.ddOrgType = ConvertToKendoDropDownList(repository.FetchGlobalFilters(3));           // All Organization Types
            ViewBag.ddFinalDisp = ConvertToKendoDropDownList(repository.FetchGlobalFilters(24));        // All Final Disposition
            ViewBag.ddAccStatus = ConvertToKendoDropDownList(repository.FetchGlobalFilters(4));         // All Acc Speaker Status
            ViewBag.ddlInterested =  repository.FetchGlobalFiltersByEvent(data.Id, 4);                  // All Interested Events
            ViewBag.ddTimeZone = ConvertToKendoDropDownList(repository.FetchGlobalFilters(26));         // All TimeZone List
            ViewBag.ddSpeakerStatus = ConvertToKendoDropDownList(repository.FetchGlobalFilters(8));     // All Acc Speaker Status
            ViewBag.ddResults = ConvertToKendoDropDownList(repository.FetchGlobalFilters(12));     // All Acc Speaker Status
            ViewBag.ddFollowupRights2 =repository.FetchGlobalFilters(9);
            ViewBag.ddSpeakerContact = ConvertToKendoDropDownList(repository.FetchGlobalFilters(9));     // All Speaker Contact
            SpeakerManagementOperations speakerRepository = new SpeakerManagementOperations();
            ViewBag.ddPastActivity = ConvertToKendoDropDownList(speakerRepository.FetchSpeakerFiltersByEvent(data.EventId, 1));         // All Speaker Activities
            ViewBag.ddTrack = ConvertToKendoDropDownList(repository.FetchGlobalFiltersByEvent(data.EventId, 5)); // All Educational Tracks
            ViewBag.ddParticipation = repository.FetchGlobalFiltersByEvent(data.EventId, 1); // ViewBag.ddPriorspeaker Using same query 
            ViewBag.ddEventInterest = repository.FetchGlobalFiltersByEvent(data.EventId, 2);
            ViewBag.SelectedInterest = data.EventId;
            ViewBag.CheckBoxListProblems = ConvertToKendoDropDownList(repository.FetchGlobalFilters(14)).OrderBy(s => s.Text).ToList();
            //clsUtility.BindAlphaRepeater(Me.rptAlpha)
            //Me.ddlInterested.SelectedValue = Val(myVS.intCurEventPKey)
            //clsUtility.SetCheckedItemsInSeperatedCommoas(Me.cbddlInterested, strevent)
            //Me.pnlfirst.Visible = cLast.boolSpkrMgShowFilter
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            ViewBag.ImageUrl = "/images/navigation/" + ((cLast.boolSpkrMgShowFilter) ? "downTriangle.png" : "rightTriangle.png");
            int SpkrMGrid = 0;
            int.TryParse(cLast.strSpkrMgID, out SpkrMGrid);
            if (cLast.strSpkrMgID == cLast.strSpkrMgID_Profile && SpkrMGrid>0)
            {
                cLast.strSpkrMgID = "";
                cLast.strSpkrMgID_Profile = "";
            }

            DateTime SelectionDate = new DateTime(), Last_NextConDates = new DateTime(), OtherDate = new DateTime();
            string Last_SlideDueDates = "";
            int IntFilterID = 0;
            DataTable AccountSettings = speakerRepository.getMenuAccountSettings(data.Id);
            if (AccountSettings != null && AccountSettings.Rows.Count> 0)
            {
                IntFilterID = (AccountSettings.Rows[0]["Filter_ID"] != DBNull.Value  && !string.IsNullOrEmpty(AccountSettings.Rows[0]["Filter_ID"].ToString())) ? Convert.ToInt32(AccountSettings.Rows[0]["Filter_ID"]) : 0;
                SelectionDate = (AccountSettings.Rows[0]["SelectionDate"] != DBNull.Value  && !string.IsNullOrEmpty(AccountSettings.Rows[0]["SelectionDate"].ToString())) ? Convert.ToDateTime(AccountSettings.Rows[0]["SelectionDate"]) : new DateTime();
                Last_NextConDates=  (AccountSettings.Rows[0]["Last_NextConDates"] != DBNull.Value && !string.IsNullOrEmpty(AccountSettings.Rows[0]["Last_NextConDates"].ToString())) ? Convert.ToDateTime(AccountSettings.Rows[0]["Last_NextConDates"]) : new DateTime();
                Last_SlideDueDates = (AccountSettings.Rows[0]["Last_SlideDueDates"] != DBNull.Value) ? Convert.ToString(AccountSettings.Rows[0]["Last_SlideDueDates"]) : "";
                OtherDate = (AccountSettings.Rows[0]["OtherDate"] != DBNull.Value && !string.IsNullOrEmpty(AccountSettings.Rows[0]["OtherDate"].ToString())) ? Convert.ToDateTime(AccountSettings.Rows[0]["OtherDate"]) : new DateTime();
            }

            ViewBag.OtherDate = OtherDate;
            ViewBag.chkComment_Checked = cLast.IsshowComment;
            ViewBag.chkShowNoteEFree_Checked = cLast.IsshowCommentEventFree;
            ViewBag.ddResult_SelectedValue =cLast.IntResult;
            if (cLast.strNextContactDate == "")
                cLast.strNextContactDate = Last_NextConDates.ToString();
            if (cLast.strBindDueDateDate == "none")
                cLast.strBindDueDateDate = Last_SlideDueDates.ToString();
            int eventpkey = ViewBag.SelectedInterest;
            if (eventpkey<=0)
                eventpkey = cLast.intActiveEventPkey;
            ViewBag.intSpkrCurEventPKey = eventpkey;
            ViewBag.SpeakerStats = speakerRepository.getSpeakerStats(data.EventId);
            SpeakerGridFilter Model = new SpeakerGridFilter();
            //With Me.cLast

            //    txtLName.Text = .strSpkrMLName
            //    Me.dpStart.SelectedDate = .dtSpkrMgStart
            //    Me.dpEnd.SelectedDate = .dtSpkrMgEnd
            //    Me.dpStart2.SelectedDate = cAccount.OtherDate
            //    Me.dpEnd2.SelectedDate = .dtEnd
            //    Me.dpStart3.SelectedDate = .dt_NStart
            //    Me.dpEnd3.SelectedDate = .dt_NEnd
            //    Me.ddStatus.SelectedValue = .intSpkrMgStatus.ToString
            //    Me.ddSesStatus.SelectedValue = .intSpkrMgSesStatus.ToString
            //    Me.ddOrgType.SelectedValue = .intSpkrMgOrgType.ToString
            //    Me.ddDateType.SelectedValue = .intSpkrMgDate.ToString
            //    Me.ddRating.SelectedValue = .intspkrAcctRating.ToString
            //    Me.ddDateRange.SelectedValue = cAccount.IntFilterID
            //    ddPastActivity.SelectedValue = .intSpkrPastActivity
            //    Me.ddAccStatus.SelectedValue = .intSpkrAccStatus.ToString
            //    Me.ddSpkrFlag.SelectedValue = .strSpkrFlag
            //    Me.ddSpeakerStatus.SelectedValue = .intSpeakerStatus.ToString
            //    Me.ckPriorities.Checked = .bSpkrMgPrior
            //    Me.ckAttending.Checked = .bSpkrMgAtt
            //    Me.ckFollowUp.Checked = .bSpkrMgRecent
            //    Me.ckRecentNote.Checked = .bSpkrMgNote
            //    Me.ckThumbnails.Checked = .bSpkrMgTN
            //    Me.ckFinalDisp.Checked = .bSpkrMgFD
            //    Me.ckTime.Checked = .bSpkrMgTime
            //    Me.chkAddedBy.Checked = .showAddedBy
            //    Me.chkshowOnlyNotes.Checked = .HideNotes
            //    Me.chkAnnouncementshow.Checked = .BoolShowAnnouncement
            //    Me.chkCallnotesEvent.Checked = .ShowCallnotesforEvent
            //    txtSearchNote.Text = .StrNotes
            //    Me.rbTop.SelectedIndex = 1
            //    Me.ddCountry.SelectedIndex = .intSpkrMgCountryId.ToString
            //    ddCountry_Change()
            //    Me.ddTimezone.SelectedIndex = .intSpkrMgTZ.ToString
            //    Me.ddLocalTimezone.SelectedIndex = .intSpkrMgLocalTZ.ToString
            //    Me.ddContacted.SelectedValue = .intSpkrMgContacted
            //    Me.ddlInterested.SelectedValue = .interestedEventID_Management
            //    Dim strevents As String = .interestedEventID_Management.ToString
            //    strevents = IIf(.strinterestedEventID_Management <> "", .strinterestedEventID_Management, .strinterestedEventID_Management)
            //    clsUtility.SetCheckedItemsInSeperatedCommoas(Me.cbddlInterested, strevents)
            //    ddTrack.SelectedValue = .IntSpkrtrackID.ToString
            //    clsUtility.SetCheckedItems(Me.ddFollowupRights2, .strFollowupRight_pkey)
            //    clsUtility.SetCheckedItems(Me.ddParticipation, .strSpkrParticipated)
            //    clsUtility.SetCheckedItems(Me.ddEventInterest, .strEventName)
            //    clsUtility.SetCheckedItems(Me.ddlPriorspeaker, .strPriorspeaker)
            //    clsUtility.SetCheckedItems(Me.ddlNotiner, .strnotinterestedEvId)
            //    chkComment.Checked = .IsshowComment
            //    chkShowNoteEFree.Checked = .IsshowCommentEventFree
            //    ddResult.SelectedValue = .IntResult
            //    If.strNextContactDate = "" Then.strNextContactDate = cAccount.strNextContactDate
            //    If.strBindDueDateDate = "none" Then.strBindDueDateDate = cAccount.strBindDueDateDate
            //End With
            int intMAGIContactPkey = 0;
            DataTable EventInfo = new SqlOperation().getDyamicEventSettings(data.EventId, "MagiContact_pkey");
            if (EventInfo != null && EventInfo.Rows.Count>0)
                intMAGIContactPkey = (EventInfo.Rows[0]["MagiContact_pkey"] != System.DBNull.Value && EventInfo.Rows[0]["MagiContact_pkey"].ToString()  != "") ? Convert.ToInt32(EventInfo.Rows[0]["MagiContact_pkey"]) : 0;


            ViewBag.intMAGIContactPKey = intMAGIContactPkey;
            ViewBag.txtOrganization_Text = cLast.strSpkrOrganization;
            ViewBag.txtName_Text = cLast.strSpkrMLName;
            ViewBag.txtID_Text = cLast.strSpkrMgID;
            ViewBag.strSearch_Text =cLast.strSpkrMgName;

            Model.strOrg = ViewBag.txtOrganization_Text;
            Model.strName = ViewBag.txtName_Text;
            Model.strSearch = ViewBag.strSearch_Text;
            Model.strID= ViewBag.txtID_Text;
            Model.dtStart= cLast.dtSpkrMgStart;
            Model.dtEnd =cLast.dtSpkrMgEnd;
            Model.dtPStart = SelectionDate;
            Model.dtPEnd = cLast.dtEnd;
            Model.sdtStart= cLast.dtSpkrMgStart.ToString("d");
            Model.sdtEnd =cLast.dtSpkrMgEnd.ToString("d");
            Model.sdtPStart = SelectionDate.ToString("d");
            Model.sdtPEnd = cLast.dtEnd.ToString("d");
            Model.ddStatus = cLast.intSpkrMgStatus;
            Model.ddSesStatus= cLast.intSpkrMgSesStatus;
            Model.ddOrgType = cLast.intSpkrMgOrgType;
            Model.ddDateType = cLast.intSpkrMgDate;
            Model.ddRating = cLast.intspkrAcctRating;
            Model.ddDateRange= IntFilterID;
            Model.ddPastActivity = cLast.intSpkrPastActivity;
            Model.ddAccStatus = cLast.intSpkrAccStatus;
            Model.strSpeakerFlag =cLast.strSpkrFlag;
            Model.ddSpeakerStatus = cLast.intSpeakerStatus;
            Model.chkPriorities = cLast.bSpkrMgPrior;
            Model.bSpkrMgAtt = cLast.bSpkrMgAtt;
            Model.bSpkrMgRecent= cLast.bSpkrMgRecent;
            Model.bSpkrMgNote =cLast.bSpkrMgNote;
            Model.bSpkrMgTN =cLast.bSpkrMgTN;
            Model.bSpkrMgFD = cLast.bSpkrMgFD;
            Model.bSpkrMgTime= cLast.bSpkrMgTime;
            Model.chkAddedBy= cLast.showAddedBy;
            Model.chkHideNotes = cLast.HideNotes;
            Model.chkAnnouncementshow= cLast.BoolShowAnnouncement;
            Model.chkCallnotesEvent= cLast.ShowCallnotesforEvent;
            Model.strnotes = cLast.StrNotes;
            Model.rbTop = 1;
            Model.ddCountry = cLast.intSpkrMgCountryId;
            Model.ddSpkrFlag = ((string.IsNullOrEmpty(cLast.strSpkrFlag)) ? 0 : Convert.ToInt32(cLast.strSpkrFlag));
            //ddCountry_Change();
            Model.ddTimezone = cLast.intSpkrMgTZ;
            Model.ddContacted = cLast.intSpkrMgContacted;
            Model.ddlInterested = ViewBag.intSpkrCurEventPKey;
            Model.streventInterested = (cLast.strinterestedEventID_Management != "") ? cLast.strinterestedEventID_Management : ViewBag.intSpkrCurEventPKey.ToString();
            Model.ddTrack = cLast.IntSpkrtrackID;
            Model.strPriorspeaker = cLast.strPriorspeaker;
            Model.strddlNotiner = cLast.strnotinterestedEvId;
            Model.strParticipation = cLast.strSpkrParticipated;
            Model.strevents = cLast.strEventName;
            Model.strFollowupRight2 = cLast.strFollowupRight_pkey;
            Model.chkComment = cLast.IsshowComment;
            Model.chkShowNoteEFree = cLast.IsshowCommentEventFree;
            Model.ddResult=cLast.IntResult;
            Model.cbddlInterested = Model.streventInterested;
            if (cLast.strNextContactDate == "")
                cLast.strNextContactDate = Last_NextConDates.ToString();
            if (cLast.strBindDueDateDate == "none")
                cLast.strBindDueDateDate = Last_SlideDueDates.ToString();
            Model.intSpkrCurEventPKey = eventpkey;
            //Model.intSpkrCurEventPKey = Model.intSpkrCurEventPKey.ToString();

            ViewBag.SpeakerModel = Model;
            ViewBag.FieldName = ""; ViewBag.SpkrMgSortOrder = ""; ViewBag.CurrentPageIndex = "";
            if (!string.IsNullOrEmpty(cLast.strSpkrMgSortExpression))
            {
                ViewBag.FieldName = cLast.strSpkrMgSortExpression;
                ViewBag.SpkrMgSortOrder = cLast.intSpkrMgSortOrder.ToString();
                ViewBag.CurrentPageIndex = cLast.intSpkrMgPageIndex.ToString();
            }
            ViewBag.STATUS_Cancelled = clsEventSession.STATUS_Cancelled;
            ViewBag.STATUS_Hypothetical = clsEventSession.STATUS_Hypothetical;

            //SpeakerGridData(Model);
            //Me.RefreshDateTypes()
            //Me.RefreshGrid(True)
            ViewBag.imgExport_Visible = isAllowedByPriv(false, data.GlobalAdmin, dt, "", "", clsPrivileges.PRIV_Export);  //Me.ApplySecurity()
            return View("~/Views/Admin/Operations/SpeakerManagement.cshtml");
        }

        private string CheckIcons(string speakerRating)
        {
            string result = " ";

            if (speakerRating == "")
                return " ";

            int SpeakerRating = Convert.ToInt32(speakerRating);
            if (speakerRating != "0")
            {
                if (SpeakerRating ==  clsFeedback.RATING_EXCELLENT)
                {
                    result = " ";
                }
                else if (SpeakerRating == clsFeedback.RATING_GOOD)
                {
                    result = " ";
                }
                else if (SpeakerRating == clsFeedback.RATING_FAIR)
                {
                    result = "Fair";
                }
                else if (SpeakerRating == clsFeedback.RATING_POOR)
                {
                    result = "Poor";
                }
                else if (SpeakerRating == clsFeedback.RATING_AWFUL)
                {
                    result = "Awful";
                }
                else
                    result = " ";
            }
            return result;
        }
        private string TypeNames(string PronounciationURL, string NickNames, string PhoneticName, string LastName)
        {
            if (!string.IsNullOrEmpty(PronounciationURL))
            {
                if (!string.IsNullOrEmpty(PhoneticName))
                    return "P";
                else if (!string.IsNullOrEmpty(NickNames))
                    return "N";
                else if (!string.IsNullOrEmpty(LastName))
                    return "L";
            }
            return "";
        }

        [HttpGet]
        [ValidateInput(true)]
        public virtual ActionResult Download(string fileGuid)
        {
            if (TempData[fileGuid] != null)
            {
                string FileName = String.Format("MAGI_SpeakerManagement_{0:yyMMdd_HH.mm}", DateTime.Now);
                byte[] data = TempData[fileGuid] as byte[];
                return File(data, "application/vnd.ms-excel", FileName);
            }
            else
            {
                // Problem - Log the error, generate a blank file,
                //           redirect to another controller action - whatever fits with your application
                return new EmptyResult();
            }
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult ExportData(SpeakerGridFilter Model, string FileName, string strTabName = "")
        {
            try
            {

                if (Request.Headers["Model"] != null)
                {
                    string data = Request.Headers["Model"].ToString();
                    Model  =   JsonConvert.DeserializeObject<SpeakerGridFilter>(data);
                }
                if (Model == null)
                    Model = ViewBag.SpeakerModel;
                DataTable dt = GetSpeakerGridDataTable(Model);
                if (dt!= null && dt.Rows.Count>0)
                {
                    DataTable dt2 = new DataTable();
                    dt2.Columns.Add("ID");
                    dt2.Columns.Add("Name");
                    dt2.Columns.Add("Title");
                    dt2.Columns.Add("TimeZone");
                    dt2.Columns.Add("Organization");
                    dt2.Columns.Add("Email");
                    dt2.Columns.Add("Tel#1");
                    dt2.Columns.Add("Tel#2");
                    dt2.Columns.Add("Degrees");
                    dt2.Columns.Add("Rating");
                    dt2.Columns.Add("LastSpeakerProfileUpdate");
                    dt2.Columns.Add("FollowDate");
                    foreach (DataRow dr in dt.Rows)
                    {
                        DataRow row = dt2.NewRow();
                        row["ID"] = dr["Account_pKey"];
                        row["Name"] = dr["Contactname"];
                        row["Title"] = dr["Title"];
                        row["TimeZone"]= dr["TimeZone"];
                        row["Organization"] = dr["OrganizationID"];
                        row["Email"] = dr["Email"];
                        row["Tel#1"] = dr["PhoneCall_1"];
                        row["Tel#2"] = dr["PhoneCall_2"];
                        row["Degrees"] = dr["Degrees"];
                        row["Rating"] =((dr["SpkrRating1"] != System.DBNull.Value) ? System.Text.RegularExpressions.Regex.Replace(dr["SpkrRating1"].ToString(), "<.*?>", "  ") : "");
                        row["LastSpeakerProfileUpdate"] = dr["LastSpeakerProfileUpdate"];
                        row["FollowDate"] = String.Format("{0:d}", dr["SpkrNextContact"]);
                        dt2.Rows.Add(row);
                    }
                    string tabName = FileName;
                    FileName += String.Format("MAGI_SpeakerManagement_{0:yyMMdd_HH.mm}", DateTime.Now);
                    string handle = Guid.NewGuid().ToString();
                    FileContentResult robj;
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        XLWorkbook wb = new XLWorkbook();

                        if (!string.IsNullOrEmpty(strTabName))
                            wb.Worksheets.Add(dt2, strTabName);
                        else
                            wb.Worksheets.Add(dt2, tabName);

                        wb.SaveAs(memoryStream);
                        memoryStream.Position = 0;
                        robj = File(memoryStream.ToArray(), System.Net.Mime.MediaTypeNames.Application.Octet, FileName+".xlsx");
                    }

                    var JsonResult = Json(new { msg = "OK", FN = FileName + ".xlsx", FileHandle = robj }, JsonRequestBehavior.AllowGet);
                    JsonResult.MaxJsonLength=int.MaxValue;
                    return JsonResult;
                }
            }
            catch (Exception ex)
            {
                return Json(new { msg = "Error While Exporting Data" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { msg = "Error While Exporting Data" }, JsonRequestBehavior.AllowGet);
        }

        private void getDates(DateTime dpStart, DateTime dpEnd, string ddDateType, ref DateTime dtStart, ref DateTime dtEnd)
        {
            int offSet = 0;
            bool boolcheck = false;
            if (Request.Cookies["yjnf"] == null)
                boolcheck = Int32.TryParse(Request.Cookies["yjnf"].Value, out offSet);
            DateTime currDate = (boolcheck) ? DateTime.UtcNow.AddMinutes(-offSet) : DateTime.Now;


            switch (ddDateType)
            {
                case DATE_FPastYear:
                    dtStart = clsUtility.getStartOfDay(currDate.Date.AddYears(-1));
                    dtEnd = clsUtility.getEndOfDay(currDate.Date);
                    break;
                case DATE_FPastMonth:
                    dtStart = clsUtility.getStartOfDay(currDate.Date.AddMonths(-1));
                    dtEnd = clsUtility.getEndOfDay(currDate.Date);
                    break;
                case DATE_FPastWeek:
                    dtStart = clsUtility.getStartOfDay(currDate.Date.AddDays(-7));
                    dtEnd = clsUtility.getEndOfDay(currDate.Date);
                    break;
                case DATE_FPastDay:
                    dtStart = clsUtility.getStartOfDay(currDate.Date.AddDays(1));
                    dtEnd = clsUtility.getEndOfDay(currDate.Date.AddDays(1));
                    break;
                case DATE_Today:
                    dtStart = clsUtility.getStartOfDay(currDate.Date);
                    dtEnd = clsUtility.getEndOfDay(currDate.Date);
                    break;
                case DATE_ThisWeek:
                    dtStart = clsUtility.getStartOfDay(currDate.Date);
                    dtEnd = clsUtility.getEndOfDay(currDate.Date.AddDays(+6));
                    break;
                case DATE_Other:
                    dtStart = clsUtility.getStartOfDay(dpStart);
                    dtEnd = clsUtility.getEndOfDay(dpEnd);
                    break;
                case "-2":
                case "8":
                case "9":
                    dtStart = clsUtility.getStartOfDay(currDate.Date.Date);
                    dtEnd = clsUtility.getEndOfDay(currDate.Date.Date);
                    break;
                case "10":
                    DateTime Tomorrow = currDate.Date.AddDays(+1);
                    dtStart = clsUtility.getStartOfDay(Tomorrow);
                    dtEnd = clsUtility.getEndOfDay(Tomorrow);
                    break;

            }

        }
        private void getDates2(DateTime OtherDate, DateTime dpStart, DateTime dpEnd2, string ddDateRange, ref DateTime dtStart, ref DateTime dtEnd)
        {
            int offSet = 0;
            bool boolcheck = false;
            if (Request.Cookies["yjnf"] == null)
                boolcheck = Int32.TryParse(Request.Cookies["yjnf"].Value, out offSet);
            DateTime currDate = (boolcheck) ? DateTime.UtcNow.AddMinutes(-offSet) : DateTime.Now;

            if (ddDateRange != DATE_All2)
            {
                switch (ddDateRange)
                {
                    case DATE_PastDay:
                        dtStart = clsUtility.getStartOfDay(DateTime.Now.Date.AddDays(-1));
                        dtEnd = clsUtility.getEndOfDay(DateTime.Now.Date);
                        break;
                    case DATE_PastWeek:
                        dtStart = clsUtility.getStartOfDay(DateTime.Now.Date.AddDays(-7));
                        dtEnd = clsUtility.getEndOfDay(DateTime.Now.Date);
                        break;
                    case DATE_PastMonth:
                        dtStart = clsUtility.getStartOfDay(DateTime.Now.Date.AddMonths(-1));
                        dtEnd = clsUtility.getEndOfDay(DateTime.Now.Date);
                        break;

                    case "6":
                        dtStart = clsUtility.getStartOfDay(DateTime.Now.Date.AddMonths(-1));
                        dtEnd = clsUtility.getEndOfDay(DateTime.Now.Date);
                        break;
                    case "7":
                        dtStart = clsUtility.getStartOfDay(DateTime.Now.Date.AddDays(-7));
                        dtEnd = clsUtility.getEndOfDay(DateTime.Now.Date);
                        break;
                    case "8":
                        dtStart = clsUtility.getStartOfDay(DateTime.Now.Date.AddDays(-1));
                        dtEnd = clsUtility.getEndOfDay(DateTime.Now.Date);
                        break;
                    case DATE_PastYear:
                        dtStart = clsUtility.getStartOfDay(DateTime.Now.Date.AddYears(-1));
                        dtEnd = clsUtility.getEndOfDay(DateTime.Now.Date);
                        break;
                    case DATE_Other2:
                        dtStart = clsUtility.getStartOfDay(OtherDate);
                        dtEnd = clsUtility.getEndOfDay(dpEnd2);
                        break;
                }
            }
        }
        private void getDates3(DateTime OtherDate, DateTime dpStart, DateTime dpEnd2, string ddDateRange, ref DateTime dtStart, ref DateTime dtEnd)
        {
            int offSet = 0;
            bool boolcheck = false;
            if (Request.Cookies["yjnf"] == null)
                boolcheck = Int32.TryParse(Request.Cookies["yjnf"].Value, out offSet);
            DateTime currDate = (boolcheck) ? DateTime.UtcNow.AddMinutes(-offSet) : DateTime.Now;

            if (ddDateRange != DATE_All3)
            {
                switch (ddDateRange)
                {
                    case DATE_NPastDay:
                        dtStart = clsUtility.getStartOfDay(DateTime.Now.Date.AddDays(-1));
                        dtEnd = clsUtility.getEndOfDay(DateTime.Now.Date);
                        break;
                    case DATE_NPastWeek:
                        dtStart = clsUtility.getStartOfDay(DateTime.Now.Date.AddDays(-7));
                        dtEnd = clsUtility.getEndOfDay(DateTime.Now.Date);
                        break;
                    case DATE_NPastMonth:
                        dtStart = clsUtility.getStartOfDay(DateTime.Now.Date.AddMonths(-1));
                        dtEnd = clsUtility.getEndOfDay(DateTime.Now.Date);
                        break;
                    case DATE_Other3:
                        dtStart = clsUtility.getStartOfDay(OtherDate);
                        dtEnd = clsUtility.getEndOfDay(dpEnd2);
                        break;
                }
            }
        }
        private DataTable GetSpeakerGridDataTable(SpeakerGridFilter Model)
        {

            if (String.IsNullOrEmpty(Model.streventInterested))
                Model.streventInterested = Model.intSpkrCurEventPKey.ToString();

            string SelectedInterested = "";
            if (!string.IsNullOrEmpty(Model.cbddlInterested))
                SelectedInterested = Model.cbddlInterested;

            DateTime dtStart = new DateTime(1980, 1, 1), dtEnd = new DateTime(3000, 1, 1);
            DateTime dtPStart = new DateTime(1980, 1, 1), dtPEnd = new DateTime(3000, 1, 1);
            getDates(Model.dtPStart, Model.dtPEnd, Model.ddDateType.ToString(), ref dtStart, ref dtEnd);
            getDates2(Model.OtherDate, Model.dtPStart, Model.dtPEnd, Model.ddDateRange.ToString(), ref dtPStart, ref dtPEnd);
            //get_Dates3(otherDate, Model.dtPStart, Model.dtPEnd, Model.ddDateRange.ToString(), ref dtStart, ref dtEnd);

            string qry = "DECLARE @IsInterest int = 0,@RegisterDate DateTime; SET @RegisterDate=(Select  RegStartDate  FROm event_List Where pkey= @SpkrCurEventPKey);Set @IsInterest= 0;"
               + Environment.NewLine + " SELECT isNull( t0.pkey,0) as pKey,FORMAT(t1.pkey, '00000') as PaddedID, t1.pkey as Account_pKey, t1.ContactName,t1.Firstname, t1.Lastname,t1.Title, isnull(t1.SkypeAddress,'') as SkypeAddress ,t1.Email as EmailAddress,"
               + Environment.NewLine + " ISNULL(t1.Email2,'') as Email2 , ISNULL(t1.EmailUsed,0) as EmailUsed,CASE WHEN ISNULL(t1.EmailUsed,0)=0 THEN  t1.Email WHEN ISNULL(t1.EmailUsed,0)=1 AND ISNULL(t1.Email2,'')<>'' THEN t1.Email2 ELSE t1.Email END as Email,"
               + Environment.NewLine + " (Case when t1.phone Is not null then t1.phone+ (Case When ISNULL(t1.PhoneType_pKey ,0)in (12) then ' (c)' else '' end) else '' END + CASE WHEN len(t1.Phone1Ext)>0 then ' x'+ t1.Phone1Ext else '' end   ) as phone,"
               + Environment.NewLine + " Case When len(t1.Nickname)>0 AND (Upper(t1.FirstName)<> UPPER(ISNULL(t1.NickName,''))) then  t1.Nickname Else '' END as NickName,Case When Len(t1.Degrees)>0 then ' ('+t1.Degrees +')' else '' end As Degrees,"
               + Environment.NewLine + " CASE When len(t1.PhoneticName)>0 then '\"' + t1.PhoneticName +'\"' Else '' End as PhoneticName,isnull( ' ['+  (   Select SalutationID from sys_salutations where pKey=t1.Salutation_pKey ) +']','') as Salutation,ISNULL(t1.Department,'') as Department,"
               + Environment.NewLine + " CASE WHEN ISNULL(t1.PersonalBio,'')<>'' THEN '../Images/Icons/BIOBLACK.png' ELSE '../Images/Icons/BIORED.png' END AS BIOINFO,ISNULL(dbo.getProblemType(t1.pkey),'Problem') as Problem,ISNULL(t1.PersonalBio,'') AS PersonalBio,ISNULL(t1.AboutMe,'') as AboutMe,"
               + Environment.NewLine + " CASE WHEN LEN(ISNULL(t1.ProducerReport_Selection,''))>0 THEN '../Images/Icons/PYellow.png'ELSE '../Images/Icons/PWhite.png' END as PImage,ISNULL(t1.ProducerReport_Selection,'') as ProducerReport_Selection,"
               + Environment.NewLine + " (Case when t1.phone Is not null then ( Case When Convert(varchar, t1.Phone1CC) Not IN('1','2','') then '(+'+ isnull(t1.Phone1CC,'')+')' else '' end )  + ' ' +  t1.phone+ (Case When ISNULL(t1.PhoneType_pKey ,0)in (12) then ' (c)' else '' end) else '' END + CASE WHEN len(t1.Phone1Ext)>0 then ' x'+ t1.Phone1Ext else '' end ) as PhoneCall1,"
               + Environment.NewLine + " (Case when len (t1.Phone2)>1 AND LEN(t1.Phone)>1 then + ''+ ( Case When Convert(varchar, t1.Phone2CC) Not IN('1','2','') then '(+'+ isnull(t1.Phone2CC,'')+')' else '' end ) + ' ' +  t1.Phone2 +  (Case When ISNULL(t1.PhoneType2_pKey ,0)in (12) then' (c)' else '' END) else '' END + CASE WHEN len(t1.Phone2Ext)>0 then ' x'+ t1.Phone2Ext else '' end ) as PhoneCall2,"
               + Environment.NewLine + " (Case when t1.phone Is not null then iif(ISNULL(t1.Phone1CC,'')<>'', isnull('+'+t1.Phone1CC +'-',''),'') + dbo.[Fn_GetNumeric](t1.phone) else '' end ) as PhoneCall_1,"
               + Environment.NewLine + " (case when len (t1.Phone2)>1  then IIF(ISNULL(t1.Phone2CC,'')<>'', isnull('+'+t1.Phone2CC+'-',''),'')+ dbo.[Fn_GetNumeric](t1.phone2) else '' end ) as PhoneCall_2, "
               + Environment.NewLine + " isnull([dbo].[getSpeakerRatingPercentage] (t1.pKey,'>',3),'') as SpkrRating ,isnull([dbo].[getSpeakerRatingPercentage] (t1.pKey,'>',8),'') as SpkrRating1,"
               + Environment.NewLine + " CASE WHEN LEN(LastProfileUpdate)>0 AND LEN(t1.LastSpeakerProfileUpdate)>0  THEN (CASE WHEN t1.LastProfileUpdate> t1.LastSpeakerProfileUpdate then t1.LastProfileUpdate ELSE  t1.LastSpeakerProfileUpdate END)"
               + Environment.NewLine + " WHEN LEN(LastProfileUpdate)>0 AND t1.LastSpeakerProfileUpdate IS Null THEN t1.LastProfileUpdate  WHEN LEN(LastProfileUpdate)>0 AND LEN(t1.LastSpeakerProfileUpdate)<=0 THEN t1.LastProfileUpdate else t1.LastSpeakerProfileUpdate END as lastUpdatedP_S,"
               + Environment.NewLine + " af.Account_pKey as EventAccount_pKey,case when ISNULL(af.SpkrFlag_pKey,0)>0 then (Select SpkrFlagID FROM SYS_SpkrFlags where pkey= af.SpkrFlag_pKey ) else '' end ShowSpeakerFlag,"
               + Environment.NewLine + " CASE WHEN ISNULL(PAF.CountPKey,0)>0 AND ISNULL(af.SpkrFlag_pKey,0)<=0 THEN '../Images/Icons/YellowF.png' when ISNULL(af.SpkrFlag_pKey,0)>0 THEN  '../Images/Icons/RedF.png' Else '../Images/Icons/BlackF.png' END IsShowPreFlag,"
               + Environment.NewLine + " Case WHEN isNull(af.SpkrFlag_pKey,0)>0 then (Select SpkrFlagID FROM SYS_SpkrFlags where pkey= af.SpkrFlag_pKey ) WHEN PAF.CountPKey>0 then 'None for this event' ELSE 'None' END AS FlagTooltips,"
               + Environment.NewLine + " CASE WHEN ISNULL(af.FollowupRights_Pkey,0)>0 THEN  ISNULL((Select Code FROM FollowupRights Where pkey= af.FollowupRights_Pkey), 'TBD') ELSE 'TBD' END AS FollowUpRights,"
               + Environment.NewLine + " ISNULL(af.FollowupRights_Pkey,0) as Followupright_Pkey, IIF(ISNULL(t1.PotentialSpeaker,0)>0,1,0) PotentialSpeaker, isNull(af.SpkrFlag_pKey,0) As SpkrFlag,"
               + Environment.NewLine + " Case When ISNULL(af.Counts,0)>1 then 'True' else 'False' End As IsNoteShow,ISNULL(t1.LinkedInProfile,'')  as ShowLinkedInProfile,"
               + Environment.NewLine + " Case when  CHARINDEX('HTTPS://',Upper( ISNULL(t1.LinkedInProfile,''))) > 0  then t1.LinkedInProfile when  CHARINDEX('HTTPS://',Upper( ISNULL(t1.LinkedInProfile,''))) <= 0 AND ISNULL(t1.LinkedInProfile,'')<>'' then 'https://www.'+ ISNULL(t1.LinkedInProfile,'') WHEN  ISNULL(t1.LinkedInProfile,'')='' THEN 'https://in.linkedin.com/pub/dir?firstName='+ t1.firstName+'&lastName='+t1.Lastname+'&trk=organization_guest_people-search-bar_search-submit' end as LinkedInProfile,"
               + Environment.NewLine + " dbo.getSessionPriorities_Color(t1.pkey,@SpkrCurEventPKey) AS SessionPriorities_Color,"
               + Environment.NewLine + " (select CASE WHEN  ExpressionDate IS NOT NULL THEN FORMAT(ExpressionDate,'MM/dd/yy hh:mm') ELSE null END  from Account_ExpressionProfile  where event_pkey =@SpkrCurEventPKey ANd Account_pKey=t1.pKey)  as ExpressionDate,"
               + Environment.NewLine + " (Case when isNull(t1.PhoneticName,'') <> '' Then t1.PhoneticName else Null end) as TTip, iif(ISNULL(t2.OrganizationID,'') ='','(None)',t2.OrganizationID) as OrganizationID,"
               + Environment.NewLine + " Case WHEN  ISNULL(t2.URL,'')<>'' THEN iif( CHARINDEX('HTTP',Upper( ISNULL(t2.URL,''))) > 0 ,t2.URL,'http://'+t2.URL)  WHEN ISNULL(t1.URL,'')<>'' THEN iif( CHARINDEX('HTTP',Upper( ISNULL( t1.URL,''))) > 0 , t1.URL, 'http://'+t1.URL) WHEN  ISNULL(t2.URL,'')='' AND ISNULL(t1.URL,'')=''  THEN 'https://'+ISNULL(((SELECT RIGHT(Email, LEN(Email) - CHARINDEX('@', email))   FROM   Account_List WHERE   pkey=t1.pkey AND  LEN(Email) > 0   ANd  Upper(RIGHT(Email, LEN(Email) - CHARINDEX('@', email))) NOT IN ('GMAIL.COM' ,'YAHOO.COM','HOTMAIL.COM','MAIL.COM','ATT.NET','YAHOO.COM.AU'))),'') END AS  OrgURL"
               + Environment.NewLine + " ,ISNULL(t1.City,'') + iif(ISNULL(t1.City,'')<>'' AND (IIF(ISNULL(SS.StateID,'')<>'',ISNULL(SS.StateID,''),ISNULL(t1.OtherState,'')))<>'',', ','') + IIF(ISNULL(SS.StateID,'')<>'',ISNULL(SS.StateID,''),ISNULL(t1.OtherState,'')) as TimeTooltips"
               + Environment.NewLine + " ,'('+ isNull(t9.TimeZone,'na') +')' as TimeZone"
               + Environment.NewLine + ",CASE WHEN LEN(LastProfileUpdate)>0 then '<span title=\"Last Profile Update \" style=font-size:8pt;Cursor:pointer >'+'(P) ' + CONVERT(VARCHAR(8), LastProfileUpdate, 1)  + '</span>' +'<br/>' else '' END"
               + Environment.NewLine + " + '<span  title=\"Last Speaker Profile Update \" style=font-size:8pt;cursor:pointer >' +iif(LEN(t1.LastSpeakerProfileUpdate)>0, '(S) '+ CONVERT(VARCHAR(8), LastSpeakerProfileUpdate, 1),'')  + '</span>' AS PSUpdate"
               + Environment.NewLine + " ,CASE WHEN  ISNULL((Select top 1 NoteText from Account_Flags_Notes Where Account_pkey =t1.pkey And Event_pkey=@SpkrCurEventPKey  AND type='Notes' Order by Account_Flags_Notes.pkey desc),'') <>'' THEN"
               + Environment.NewLine + " '<span style=cursor:pointer Class=NoteClassBlack title='+'\"Notes Date: '+convert(varchar,(Select top 1 NoteDate from Account_Flags_Notes Where Account_pkey =t1.pkey And Event_pkey=@SpkrCurEventPKey  AND type='Notes' Order by Account_Flags_Notes.pkey desc),1)+'\">' +"
               + Environment.NewLine + "  ISNULL((Select top 1 dbo.StripHTMLTags(NoteText) from Account_Flags_Notes Where Account_pkey =t1.pkey And Event_pkey=@SpkrCurEventPKey  AND type='Notes' Order by Account_Flags_Notes.pkey desc),'') + '</span></br>' ELSE '' END   AS OnlyNotes"
               + Environment.NewLine + " , dbo.GET_NextContact(t1.pkey,@SpkrCurEventPKey,t6.MaxPKey,1) as NextFiveNotes "
               + Environment.NewLine + " ,Case WHEN t8.CountPKey>0 THEN  'Contact Date: '+ IIF(LEN(t7.ContactDate)>0, Convert(varchar, t7.ContactDate,1),'NULL') + CHAR(13)+CHAR(10) + 'Contact Notes: ' + IIF( LEN(dbo.StripHTMLTags(t7.Comment))>0,dbo.StripHTMLTags(t7.Comment),'None') ELSE '' END AS Con_ToolTips ,'Followup Notes: '+IIF(LEN(t7.FollowupNotes)>0,t7.FollowupNotes,'None') as FollowNotesToolTip"
               + Environment.NewLine + " ,CASE WHEN LEN(CC.NextActionDate)>0 and CONVERT(Date ,CC.NextActionDate) >=Convert(Date,getdate()) then '/Images/Icons/AFblack.png' else '/Images/Icons/AFRed.png' end as AFBackColor"
               + Environment.NewLine + " ,iif( ISNULL(SC.Account_pkey,0)>0,1,0) as AFShow,LEFT(t1.Lastname,1)  AS CH"
               + Environment.NewLine + " ,( Select COUNT(0)   from account_list t1  Where  t1.pkey in((select distinct account_pkey from Account_ExpressionProfile where event_pkey =@SpkrCurEventPKey )) OR (t1.GeneralInterestInBeingSpeaker = 1) OR (ISNULL(t1.PotentialSpeaker,0)=1) Or (t1.pkey In (Select distinct account_pkey from account_sessions))) as TotalRecords"
               + Environment.NewLine + " ,CASE WHEN LEN( t7.NextActionDate)>0 THEN  t7.NextActionDate   ELSE null END AS  SpkrNextContactShort, isNull(t8.CountPKey,0) as NumContacts"
               + Environment.NewLine + " ,iif(ISNULL(af.SpkrFlag_pKey,0)<=0,CASE WHEN LEN( t7.NextActionDate)>0 THEN '<span class=bold style=font-size:8pt;>'+  ISNULL(sm.CallNextActionID,'') + ' '+ iif(ISNULL(t7.IsFollowupTime,0)=1,Convert(varchar(500),FORMAT(t7.NextActionDate,'MM/dd/yy hh:mm tt')), Convert(varchar, t7.NextActionDate,1))+'</span> <br/><span style= font-size:8pt;>'+ ISNULL(dbo.FN_ReturnActivities(t1.pkey,@SpkrCurEventPKey),'') +'</span>' ELSE '<span style= font-size:8pt;>'+ ISNULL(dbo.FN_ReturnActivities(t1.pkey,@SpkrCurEventPKey),'') +'</span>' END,'') as SpkrNextContact"
               + Environment.NewLine + ",CASE WHEN   isNull(t8.CountPKey,0)=0 THEN '<span class=PerClass title='+'\"Notes Date: '+convert(varchar,(Select top 1 NoteDate from Account_Flags_Notes Where Account_pkey =t1.pkey And Event_pkey=@SpkrCurEventPKey  AND type='Notes' Order by Account_Flags_Notes.pkey desc),1)+'\"  style= font-size:8pt;>'+"
               + Environment.NewLine + "ISNULL((Select top 1 '[' +LEFT(FirstName, 1)+LEFT(LastName,1) +'] ' from Account_Flags_Notes A0  INNER JOIN Account_List Al1 ON Al1.pkey=A0.AuthorAccount_pKey where "
               + Environment.NewLine + "Account_pkey =t1.pkey And Event_pkey=@SpkrCurEventPKey  AND type='Notes' Order by A0.pkey desc),'')+' '+"
               + Environment.NewLine + "af.NoteText+'</span>' ELSE '' END"
               + Environment.NewLine + " + (Case when t7.CallNextAction_pKey NOT  in(2,7,8) then"
               + Environment.NewLine + "iif(Len( t7.FollowupNotes)>0, ' '+'<span  Class=  style=cursor:pointer; title='+'\"Followup Date: '+IIF(LEN(t7.NextActionDate)>0, Convert(varchar, t7.NextActionDate,1),'None')+'\">'+ t7.FollowupNotes +'</span></br>','') "
               + Environment.NewLine + " When t7.CallNextAction_pKey in(2,7,8) then '<span style= font-size:8pt; title='+'\"Followup Date: '+IIF(LEN(t7.NextActionDate)>0, Convert(varchar, t7.NextActionDate,1),'None')+'\">'+ ISNULL((Select CallNextActionID from SYS_CallNextActions where pkey= t7.CallNextAction_pKey),'')+'</span></br>'"
               + Environment.NewLine + "else '' END) + (Case when len(dbo.StripHTMLTags(t7.Comment) ) >= 0 OR len(dbo.StripHTMLTags(t7.PermanentNotes) ) >= 0  then '<span style=cursor:pointer Class=NoteClass title='+'\"Contact Date: '+IIF(LEN(t7.ContactDate)>0, Convert(varchar, t7.ContactDate,1),'None')+'\">' "
               + Environment.NewLine + "+'(' +(SELECT  LEFT(FirstName, 1)+LEFT(LastName,1) FROM Account_List Where Pkey=t7.Account_pKey)+ ' ' + IIF(LEN(t7.ContactDate)>0, Convert(varchar, t7.ContactDate,1),'None')+' '+ISNULL((Select Case WHEN pkey=2 then 'Left Msg' WHEN pkey=12 then 'No Msg' ELSE ISNULL(CallOutcomeID_Futher,CallOutcomeID) END from SYS_CallOutcomes  where pkey= t7.CallOutcome_pKey),'') + iif(t7.CallNextAction_pKey=9, ' ('+ right(t1.Phone,4) +')','') +')</span>'+    iif(Len( af.NoteText)>0, ' ' +"
               + Environment.NewLine + "'<span class=PerClass title='+'\"Notes Date: '+convert(varchar,(Select top 1 NoteDate from Account_Flags_Notes Where Account_pkey =t1.pkey And Event_pkey=@SpkrCurEventPKey  AND type='Notes' Order by Account_Flags_Notes.pkey desc),1)+'\" style= font-size:8pt;>'+"
               + Environment.NewLine + "ISNULL((Select top 1  '['+LEFT(FirstName, 1)+LEFT(LastName,1)+'] ' from Account_Flags_Notes A0  INNER JOIN Account_List Al1 ON Al1.pkey=A0.AuthorAccount_pKey where "
               + Environment.NewLine + "Account_pkey =t1.pkey And Event_pkey=@SpkrCurEventPKey  AND type='Notes' Order by A0.pkey desc),'')+' '+"
               + Environment.NewLine + "af.NoteText+'</span>','')+iif(Len( dbo.StripHTMLTags(t7.Comment))>0 AND Len(dbo.StripHTMLTags(af.NoteText))>0 , ' <> ' ,'') +  iif(Len( dbo.StripHTMLTags(t7.Comment))>0, ' '  + '<span Title=\"Call Notes\" style= font-size:8pt;>'+ dbo.StripHTMLTags(t7.Comment)+'</span>','')   else '' END ) +''AS All_Notes"
               + Environment.NewLine + " ,(CASE WHEN  ISNULL((Select top 1 NoteText from Account_Flags_Notes Where Account_pkey =t1.pkey And Event_pkey=@SpkrCurEventPKey  AND type='Notes' Order by Account_Flags_Notes.pkey desc),'') <>'' THEN"
               + Environment.NewLine + " UPPER(ISNULL((Select top 1 NoteText from Account_Flags_Notes Where Account_pkey =t1.pkey And Event_pkey=@SpkrCurEventPKey   AND type='Notes' Order by Account_Flags_Notes.pkey desc),'')) ELSE '' END) AS All_Notes_Short"
               + Environment.NewLine + " ,isnull(t1.SpeakerStatus_pkey,0) as SpeakerStatus_pkey,  CASE when  t1.SpeakerStatus_pkey =1 THEN 'A' When  t1.SpeakerStatus_pkey = 2 Then  'I' When  t1.SpeakerStatus_pkey =3 Then  'R'  When  t1.SpeakerStatus_pkey =4 Then 'E' Else '' END as SpkrStatusText"
               + Environment.NewLine + " ,CASE when  t1.SpeakerStatus_pkey =1 THEN 'Green' When  t1.SpeakerStatus_pkey = 2 Then  'Gray' When  t1.SpeakerStatus_pkey =3 Then 'Red'  When  t1.SpeakerStatus_pkey =4 Then 'Black' Else 'White' END as SpkrStatusBackColor"
               + Environment.NewLine + " ,isnull(t1.AccountStatus_pkey,0) as AccountStatus_pkey,  CASE when  t1.AccountStatus_pkey =1 THEN 'A' When  t1.AccountStatus_pkey = 2 Then  'I' When  t1.AccountStatus_pkey =3 Then  'R'  When  t1.AccountStatus_pkey =4 Then 'E' Else '' END as AccStatusText"
               + Environment.NewLine + " ,CASE when  t1.AccountStatus_pkey =1 THEN 'Green' When  t1.AccountStatus_pkey = 2 Then  'Gray' When  t1.AccountStatus_pkey =3 Then 'Red'  When  t1.AccountStatus_pkey =4 Then 'Black' Else 'White' END as AccStatusBackColor"
               + Environment.NewLine + " ,isNull(t1.PrioritySpeaker,0) As PrioritySpkr, (Case When t1.PrioritySpeaker=1 Then 'Y' else Null End) as PrioritySpkrText"
               + Environment.NewLine + " ,CASE WHEN ISNULL(af.FollowupRights_Pkey,0)>0 THEN  ISNULL((Select ISNULL(Backcolor,'#FFFF00')  FROM FollowupRights Where pkey= af.FollowupRights_Pkey), '#FFFFFF') ELSE '#FFFF00' END as FBackColor"
               + Environment.NewLine + " ,ISNULL(t1.PronunciationURL,'') as PronunicationURL,iif(ISNULL(t1.WritingArticle,0) >0,1,0) as WritingArticle , ISNULL(t1.WritingArticle,0) as SetWritingArticle"
               + Environment.NewLine + " ,Case When len(t1.Nickname )>0 And (Upper(t1.FirstName)<> UPPER(ISNULL(t1.NickName,''))) then  t1.Nickname Else '' END + CASE When len(t1.PhoneticName)>0 then ' \"' + t1.PhoneticName +'\"' Else '' End as NickNames"
               + Environment.NewLine + " ,(Case when exists (select pkey from EventSession_staff where account_pkey = t1.pkey and FeedbackPosted=1 AND ISNULL(NumFeedbackResponses,0)>0 ) Then 1 else 0 End) as HasPosted"
               + Environment.NewLine + " ,ISNULL(dbo.FN_ReturnProposal(t1.pkey,@SpkrCurEventPKey),'') AS Proposal"
               + Environment.NewLine + " ,STUFF((SELECT top(5) ', ' + COALESCE(LTRIM(RTRIM(tbl2.EventID) +Case When tbl3.ParticipationStatusID <>'Attending' Then +' ('+tbl3.ParticipationStatusID +')' else '' END ), '') FROM Event_Accounts tbl1"
               + Environment.NewLine + "  Inner join Event_List tbl2 on tbl2.pkey = tbl1.Event_pKey  LEFT JOIN sys_ParticipationStatuses tbl3 ON tbl3.pkey = tbl1.ParticipationStatus_pKey Where tbl1.Account_pKey=t1.pKey  Order by tbl2.StartDate desc FOR XML PATH('') ), 1, 1, '') as Attended"
               + Environment.NewLine + " ,t1.LastSpeakerProfileUpdate,ISNULL(OType.OrganizationTypeID,'N/A') as OrganizationTypeID,ISNULL(t1.SpeakerRating ,0) as SpeakerRating ";
            if (Model.ckFinalDisp)
            {
                qry = qry + Environment.NewLine + ", CASE WHEN  ((Select Count(pkey) from EventSession_Staff where  EventSession_pkey IN(Select pkey FROM Event_Sessions where event_pkey=t0.Event_pKey) AND Account_pKey=t1.pkey ANd ISNULL(IsSpeaker,0)=1)>0) THEN 'Speaker' ELSE "
                + Environment.NewLine + " iif(t10.FinalDispositionID='Speaker','Not Set', isNull(t10.FinalDispositionID,'Not Set')) END as FinalDisp";
            }
            else
                qry = qry + Environment.NewLine + ", 'FinalDisp' as FinalDisp";

            qry = qry + Environment.NewLine + ",dbo.fn_RemoveNonASCIIChars(t1.Lastname) AS NonUnicodeLastName,ISNULL(dbo.FN_ReturnStrListMVC(t1.pkey,@SpkrCurEventPKey),'') AS List,Case when LEN(t7.NextActionDate)>0 then t7.NextActionDate else null end  as NextFollowUpdate from account_list t1"
            + Environment.NewLine + " left outer join event_accounts t0 on t0.account_pkey = t1.pkey and t0. event_pkey in( @SpkrCurEventPKey )"
            + Environment.NewLine + " Left outer join Organization_list t2 On t2.pkey = t1.ParentOrganization_pKey"
            + Environment.NewLine + " Left outer join SYS_OrganizationTypes OType On OType.pkey = t2.OrganizationType_pkey ";

            //+ Environment.NewLine + " Left outer join sys_ParticipationStatuses t3 On t3.pkey = t0.ParticipationStatus_pKey"
            //+ Environment.NewLine + " Left outer join (Select eventaccount_pKey, max(pkey) As MaxPKey from eventaccount_notes group by eventaccount_pkey) t4 On t4.eventaccount_pkey = t0.pkey"
            //+ Environment.NewLine + " Left outer join eventaccount_notes t5  On t5.pkey = t4.MaxPKey";
            string addQuery = "1=1";
            if (Model.chkAnnouncementshow)
                addQuery = "  1=1";
            else
                addQuery = "  CallOutcome_pKey <>17";

            if (Model.chkShowNoteEFree)
            {
                if (Model.chkCallnotesEvent)
                    qry = qry + Environment.NewLine + " Left outer join (Select  UpdatedforAccount_pkey, max(pkey) As MaxPKey from EventAccount_Communication Where  " + addQuery + "  AND ISNULL(AdditionalFollowup,0)=0 AND   event_pkey=@SpkrCurEventPKey  group by UpdatedforAccount_pkey ) t6 On t6.UpdatedforAccount_pkey = t1.pkey";
                else
                    qry = qry + Environment.NewLine + " Left outer join (Select  UpdatedforAccount_pkey, max(pkey) As MaxPKey from EventAccount_Communication Where " + addQuery + " AND ISNULL(AdditionalFollowup,0)=0 AND  ContactDate>=(Select RegStartDate  From event_list where pkey=@SpkrCurEventPKey)  group by UpdatedforAccount_pkey ) t6 On t6.UpdatedforAccount_pkey = t1.pkey";

                if (Model.chkCallnotesEvent)
                    qry = qry  + Environment.NewLine + "Left outer join (Select UpdatedforAccount_pkey, count(pkey) As CountPKey from EventAccount_Communication Where " + addQuery + " AND ISNULL(AdditionalFollowup,0)=0  AND   event_pkey=@SpkrCurEventPKey group by UpdatedforAccount_pkey ) t8 On t8.UpdatedforAccount_pkey = t1.pkey";
                else
                    qry = qry  + Environment.NewLine + "Left outer join (Select UpdatedforAccount_pkey, count(pkey) As CountPKey from EventAccount_Communication Where " + addQuery + " AND ISNULL(AdditionalFollowup,0)=0 AND  ContactDate>=(Select RegStartDate  From event_list where pkey=@SpkrCurEventPKey) group by UpdatedforAccount_pkey ) t8 On t8.UpdatedforAccount_pkey = t1.pkey";
            }
            else
            {
                qry = qry + Environment.NewLine + " Left outer join (Select  UpdatedforAccount_pkey,event_pkey, max(pkey) As MaxPKey from EventAccount_Communication where " + addQuery + " AND ISNULL(AdditionalFollowup,0)=0   group by UpdatedforAccount_pkey,event_pkey ) t6 On t6.UpdatedforAccount_pkey = t1.pkey AND t6.event_pkey=@SpkrCurEventPKey"
                 + Environment.NewLine + "Left outer join (Select UpdatedforAccount_pkey,event_pkey, count(pkey) As CountPKey from EventAccount_Communication where " + addQuery + "  AND ISNULL(AdditionalFollowup,0)=0  group by UpdatedforAccount_pkey,event_pkey ) t8 On t8.UpdatedforAccount_pkey = t1.pkey AND t8.event_pkey=@SpkrCurEventPKey";
            }

            qry = qry + Environment.NewLine + " Left outer join EventAccount_Communication t7  On t7.pkey = t6.MaxPKey"
                   + Environment.NewLine + "  Left outer join Sys_CallNextActions sm  On sm.pkey = t7.CallNextAction_pKey";

            qry = qry + Environment.NewLine + " Left outer join SYS_CountryTimeZone t9 on t9.pkey = t1.TimezonePKey"
            //+ Environment.NewLine + " Left outer join sys_PhoneTypes t12 on t12.pkey = t1.PhoneType2_pKey"
            //+ Environment.NewLine + " Left outer join sys_PhoneTypes t11 on t11.pkey = t1.PhoneType_pKey"
            + Environment.NewLine + " Left outer join Account_flags af on af.account_pkey = t1.pkey And af.event_pkey =@SpkrCurEventPKey ";
            if (Model.ckFinalDisp ||  Model.ddFinalDispVal > 0)
                qry = qry + Environment.NewLine + " Left outer join Sys_FinalDispositions t10 on t10.pkey = af.FinalDisposition_pKey";

            //qry = qry + Environment.NewLine + " Left outer join (select c.account_pkey, count(c.pKey) as NumCharges from account_charges c where c.event_pkey = @SpkrCurEventPKey   And IsNull(c.Reversed, 0) = 0 and c.ReversalReference is Null AND isnull(c.IsDelete,0)=0 AND ChargeType_pKey=1 Group By c.Account_pKey ) q on q.account_pkey = t1.pkey";

            qry = qry + Environment.NewLine + "Left outer join ( Select count(pkey) As CountPKey,Account_pkey from Account_flags where   ISNULL(SpkrFlag_pKey,0)>0 AND Account_flags.Event_pkey<>@SpkrCurEventPKey group by Account_pkey) PAF On PAF.account_pkey = t1.pkey "
             + Environment.NewLine + "LEFT OUTER join ( Select RealAccount_pkey , Event_pkey , max(pkey) as pkey from Common_Communication  Group by RealAccount_pkey,Event_pkey) SF on SF.RealAccount_pkey=t1.pKey and SF.event_pkey=@SpkrCurEventPKey"
             + Environment.NewLine + "LEFT OUTER  JOIN Common_Communication CC ON CC.pkey=SF.pkey"
             + Environment.NewLine + "LEFT OUTER JOIN SpeakerContact SC ON SC.Account_pkey=t1.pkey and  ISNULL(SC.Additional_FollowUp,0)=1  and SC.Event_pkey=@SpkrCurEventPKey"
             //+ Environment.NewLine + "LEFT OUTER JOIN SpeakerContact PS ON PS.Account_pkey=t1.pkey and  ISNULL(PS.Additional_FollowUp,0)=0  and PS.Event_pkey=@SpkrCurEventPKey"
             + Environment.NewLine + " LEFT OUTER JOIN SYS_States SS ON SS.pKey=t1.State_pKey"
             + Environment.NewLine + " Where 1=1";

            if (Model.chkSelectedPeople && Model.SelchkStrSpkr != "")
            {
                //qry = qry + Environment.NewLine + " AND t1.pKey IN (" + String.Join(",", SelchkStrSpkr.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).Trim(",") + ")";
                qry = qry + Environment.NewLine + " ";
            }
            else if (Model.chkSelectedPeople)
                qry = qry + Environment.NewLine + " AND t1.pKey Is NULL";


            //For Each ck As RadComboBoxItem In Me.cbddlInterested.CheckedItems
            //Select Case Val(ck.Value)
            //    Case -1 : qry = qry + vbCrLf + "And (t1.pkey in(select account_pkey from Account_ExpressionProfile where event_pkey >0 ) OR (t1.GeneralInterestInBeingSpeaker = 1) OR ((ISNULL(t1.PotentialSpeaker,0)=1 and ISNULL(SC.pkey,0)>0 ) )Or (t1.pkey In (Select account_pkey from account_sessions)))"
            //    Case -2 : qry = qry + vbCrLf + "And (t1.pkey Not in(select account_pkey from Account_ExpressionProfile where event_pkey > 0))"
            //End Select
            //Next
            if (!String.IsNullOrEmpty(SelectedInterested))
                qry = qry  + Environment.NewLine + "And t1.pkey in(select account_pkey from Account_ExpressionProfile where event_pkey in(" + SelectedInterested + " ) OR (ISNULL(t1.PotentialSpeaker,0)=1  and ISNULL(SC.pkey,0)>0  ))";
            else
                qry = qry  + Environment.NewLine + "And t1.pkey in(select account_pkey from Account_ExpressionProfile where event_pkey >0 OR (ISNULL(t1.PotentialSpeaker,0)=1  and ISNULL(SC.pkey,0)>0  ))";

            //        For Each ck As RadComboBoxItem In Me.ddlNotiner.CheckedItems
            //            If ck.Value = "0" Then
            //                bNoEvents3 = True
            //    Else
            //        If strddlNotiner <> "" Then strddlNotiner = strddlNotiner + ","
            //        strddlNotiner = strddlNotiner + ck.Value
            //    End If
            //Next
            //If strddlNotiner<> "" Then qry = qry + vbCrLf + " and not t1.pkey in(select account_pkey from Account_ExpressionProfile where event_pkey in ( " + strddlNotiner.ToString + "))"

            if (Model.ddSesStatus == -3)
            {
                qry = qry + Environment.NewLine +  " And (t1.pkey  NOT In ( "
                + Environment.NewLine +  "  Select  ttt1.Account_pKey From Account_Sessions ttt1 "
                + Environment.NewLine +  " inner Join Event_Sessions ttt3 On ttt3.Session_pKey = ttt1.Session_pKey And ttt3.Event_pKey =@Event_pkey "
                +Environment.NewLine +  "  Where ttt1.Account_pKey =t1.pkey  and isnull(ttt1.IsNotInterested,0)=0 and ttt3.Event_pKey =@Event_pkey "
                + Environment.NewLine +  ") AND (t1.GeneralInterestInBeingSpeaker = 1))";
            }

            //For Each ck As RadComboBoxItem In Me.ddlPriorspeaker.CheckedItems
            //    If ck.Value = "0" Then
            //        bNoEvents2 = True
            //    Else
            //        If strPriorspeaker <> "" Then strPriorspeaker = strPriorspeaker + ","
            //        strPriorspeaker = strPriorspeaker + ck.Value
            //    End If
            //Next

            if (!String.IsNullOrEmpty(Model.strPriorspeaker))
                qry = qry + Environment.NewLine + "and t1.pkey in(select account_pkey from EventSession_Staff where isSpeaker = 1 and ISNULL(AssignmentStatus_pKey,2) IN(19,2)  and EventSession_pkey iN(select pkey from event_sessions where event_pkey IN(" + Model.strPriorspeaker + ")))";

            if (Model.ddlInterested  > 0 && string.IsNullOrEmpty(Model.strPriorspeaker))
                qry = qry + Environment.NewLine  + " And ((t1.pkey in(select account_pkey from Account_ExpressionProfile where event_pkey in (" + Model.streventInterested + " ))) OR (t1.GeneralInterestInBeingSpeaker = 1)  OR ((ISNULL(t1.PotentialSpeaker,0)=1) and ISNULL(SC.pkey,0)>0 ) Or (t1.pkey In (Select account_pkey from account_sessions)))";

            if (Model.ddSpkrFlag != -1 && Model.ddSpkrFlag != -4)
            {
                if (Model.ddSpkrFlag == -2)
                    qry = qry + Environment.NewLine + " and af.account_pkey in (Select  account_pkey from Account_flags where  account_pkey = t1.pkey and SpkrFlag_pKey>0 AND Account_flags.event_pkey in (" + Model.streventInterested + " ))";
                else if (Model.ddSpkrFlag == 0)
                    qry = qry + Environment.NewLine + " and af.account_pkey Not in (Select  account_pkey from Account_flags where  account_pkey = t1.pkey and (SpkrFlag_pKey>0 and SpkrFlag_pkey <> 25) AND Account_flags.event_pkey in (" + Model.streventInterested + " ))";
                else
                    qry = qry + Environment.NewLine + " AND af.event_pkey=@SpkrCurEventPKey AND  af.SpkrFlag_pKey IN( " + Model.ddSpkrFlag.ToString().TrimEnd(',') + ")";
            }

            if (!string.IsNullOrEmpty(Model.strParticipation))
                qry = qry + Environment.NewLine + " And t1.pkey In(Select Account_pkey From Event_Accounts Where event_pkey IN(" + Model.strParticipation + ") GROUP BY Account_pkey) ";

            if (!string.IsNullOrEmpty(Model.strInterestedEvent))
                qry = qry + Environment.NewLine + "And t1.pkey in(select account_pkey from Account_ExpressionProfile where event_pkey in (" + Model.strInterestedEvent + "))";

            switch (Model.ddCountry)
            {
                case -1:
                    qry = qry + Environment.NewLine + "And  t1.Country_pKey in (1,2)";
                    break;
                case -2:
                    qry = qry + Environment.NewLine + "And  t1.Country_pKey not in (1)";
                    break;
            }

            if (Model.ddDateType  > 0 && Model.ddDateType != 5  && Model.ddDateType == 8  && Model.ddDateType ==9)
                qry = qry + Environment.NewLine + " And t7.NextActionDate >= @Start And t7.NextActionDate <= @End";
            if (Model.ddDateType  == -1)
                qry = qry + Environment.NewLine + " And ISNULL(t7.NextActionDate,'')='' ";
            if (Model.ddDateType  == -2)
                qry = qry + Environment.NewLine + " And ISNULL(t7.NextActionDate,'')<>'' ";
            if (Model.ddDateType  == -3)
                qry = qry + Environment.NewLine + " And ISNULL(t7.CallNextAction_pKey ,0) IN (2,7,8) ";
            if (Model.ddDateType  == -4)
                qry = qry + Environment.NewLine + " And( ISNULL(t7.CallNextAction_pKey ,0)  NOT In (2,7,8)  And ISNULL(t7.NextActionDate,'')='' )";
            if (Model.ddDateType  == 5)
                qry = qry + Environment.NewLine + "And t7.NextActionDate < @Start";
            if (Model.ddDateType  == 8)
                qry = qry + Environment.NewLine + "And t7.NextActionDate < @Start";
            if (Model.ddDateType  == 9)
                qry = qry + Environment.NewLine + "And t7.NextActionDate > @Start";

            if (Model.ddDateType == -5)
            {
                qry = qry + Environment.NewLine + " And ISNULL(t7.NextActionDate,'')='' "
                + Environment.NewLine + "AND (("
                + Environment.NewLine + "Select  Count(ISNULL(tt1.pkey,0)) From Account_List tt1 Inner Join Account_Sessions tt2 On tt2.Account_pkey = tt1.pKey Left Outer Join Account_SessionEvents tt3 On tt3.AccountSession_pkey = tt2.pKey And tt3.Event_pkey =@SpkrCurEventPKey"
                + Environment.NewLine + "Where tt3.AssignmentStatus_pKey in(2,19) and tt1.pkey=t1.pkey )=0) "
                + Environment.NewLine + "And ( ISNULL(t7.CallNextAction_pKey ,0)  NOT In (2,7,8))";
            }
            if (Model.ddStatus > 0)
                qry = qry + Environment.NewLine + " And t0.ParticipationStatus_pKey = " + Model.ddStatus.ToString();

            if (Model.ddOrgType > 0)
                qry = qry + Environment.NewLine + " And t2.OrganizationType_pKey = " + Model.ddOrgType.ToString();
            else if (Model.ddOrgType == -1)
                qry = qry + Environment.NewLine + "  and (t1.ParentOrganization_pKey in(Select Organization_pKey from Event_Organizations where   ParticipationType_pkey=5  and event_pkey=@SpkrCurEventPKey))";

            if (Model.ddCountry >0)
                qry = qry + Environment.NewLine + " And t1.Country_pKey = " + Model.ddCountry.ToString();

            if (!string.IsNullOrEmpty(Model.strSearch))
            {
                qry = qry + Environment.NewLine + " And (t1.LastName Like @Name COLLATE SQL_Latin1_General_CP1_CI_AI or t1.Firstname Like @Name COLLATE SQL_Latin1_General_CP1_CI_AI OR t1.ContactName like @Name COLLATE SQL_Latin1_General_CP1_CI_AI OR " + clsUtility.SQLNoLiterals("t1.LastName+t1.Firstname") + " Like @Name COLLATE SQL_Latin1_General_CP1_CI_AI"
                + Environment.NewLine + " OR t1.City like @Name"
                + Environment.NewLine + " OR " + clsUtility.SQLNoLiterals("t1.Title") + " like @Name"
                + Environment.NewLine + " OR t1.State_pKey in (select pKey from SYS_States where StateID like @Name)"
                + Environment.NewLine + " OR t1.OtherState Like @Name"
                + Environment.NewLine + " Or " + clsUtility.SQLNoLiterals("t1.Email") + " Like @Name"
                + Environment.NewLine + " Or " + clsUtility.SQLNoLiterals("t1.Email2") + " like @Name"
                //+ Environment.NewLine + "OR ISNULL(t12.PhoneTypeID,'Phone') Like @Name"
                //+ Environment.NewLine + "OR ISNULL(t11.PhoneTypeID,'Phone') Like @Name"
                + Environment.NewLine + " OR RIGHT(t1.Phone,4) Like @Name"
                + Environment.NewLine + " OR RIGHT(t1.Phone2,4) Like @Name)";
            }
            if (!string.IsNullOrEmpty(Model.strnotes))
            {
                qry = qry + Environment.NewLine + "AND ( (Select COUNT(isnull(UpdatedforAccount_pkey,0)) from EventAccount_Communication  where  ISNULL(AdditionalFollowup,0)=0 AND Comment Like @Notes"
                + Environment.NewLine + "And event_pkey=@SpkrCurEventPKey and UpdatedforAccount_pkey=t1.pkey Group by UpdatedforAccount_pkey)>0)"
                + Environment.NewLine + "OR  ((Select  Count(ISNULL(Account_pkey,0)) from Account_flags  where event_pkey=@SpkrCurEventPKey and account_pkey =t1.pkey and NoteText like @Notes  GROUP by Account_pkey)>0) ";
            }
            if (!string.IsNullOrEmpty(Model.strOrg))
                qry = qry + Environment.NewLine + " And t2.OrganizationID Like @Org";

            //If(cLast.strSpkrMgID_Profile <> "") Then
            //    cLast.strDemiSpkrMgID = cLast.strSpkrMgID_Profile
            //End If

            if (!string.IsNullOrEmpty(Model.strID))
                qry = qry + Environment.NewLine + " And cast(t1.pKey As varchar) Like @PK";
            else if (Model.intDemiSpkrMgID>0)
                qry = qry + Environment.NewLine + " And cast(t1.pKey As varchar) Like @PK";

            string Counrty_ID = " IN (0)";

            if (Model.ddCountry == -1)
                Counrty_ID = " IN (1,2)";
            else if (Model.ddCountry == -2)
                Counrty_ID = " Not IN (1)";
            else if (Model.ddCountry >0)
                Counrty_ID = " IN(" + Model.ddCountry.ToString() + ")";

            Model.ddTimezone  =  (Model.ddTimezone == 0) ? -14 : Model.ddTimezone;
            if (Model.ddTimezone == -12)
                qry = qry + Environment.NewLine +  " And t1.TimeZonePKey Is Null";
            else if (Model.ddTimezone == -13)
            {
                qry = qry + Environment.NewLine + " And t1.TimezonePKey Not IN( Select pkey from SYS_CountryTimeZone Where CountryCode IN( Select   CountryCode from SYS_Countries where pkey" + Counrty_ID + "))"
                + Environment.NewLine  + "AND  t1.TimezonePKey Not IN( "
                + Environment.NewLine  + " SELECT tz.pkey"
                + Environment.NewLine  + "from SYS_CountryTimeZone tz"
                + Environment.NewLine  + "inner join sys_Countries c on tz.CountryCode=c.countrycode"
                + Environment.NewLine  + "Where tz.active=1 and tz.IsInternational = 0 "
                + Environment.NewLine  + "and  c.pKey =1"
                + Environment.NewLine  + "UNION All "
                + Environment.NewLine  + "SELECT  tz.pkey"
                + Environment.NewLine  + "from SYS_CountryTimeZone tz"
                + Environment.NewLine  + "inner join sys_Countries c on tz.CountryCode=c.countrycode"
                + Environment.NewLine  + "Where tz.active=1 and tz.IsInternational = 0 "
                + Environment.NewLine  + "and  c.pKey = 2"
                + Environment.NewLine  + "UNION All"
                + Environment.NewLine  + ""
                + Environment.NewLine  + "Select   tz.pkey from SYS_CountryTimeZone tz  Where CountryCode='UA'"
                + Environment.NewLine  + "UNION ALL"
                + Environment.NewLine  + "Select   tz.pkey from SYS_CountryTimeZone tz  Where CountryCode='FR'"
                + Environment.NewLine  + "UNION ALL"
                + Environment.NewLine  + "Select  tz.pkey from SYS_CountryTimeZone tz  Where CountryCode='GB'"
                + Environment.NewLine  + ")";
            }
            else if (Model.ddTimezone != -14)
                qry = qry +  Environment.NewLine  + " And t9.UTCoffset = " + Model.ddTimezone.ToString();

            if (Model.ddDateRange> 0 && Model.ddDateRange <6 && Model.ddSpeakerStatus!= -2)
                qry = qry +  Environment.NewLine  +  " And ( t1.LastSpeakerProfileUpdate >=@PStart And t1.LastSpeakerProfileUpdate <=@PEnd   OR  t1.LastProfileUpdate >=@PStart And t1.LastProfileUpdate <=@PEnd  )";

            if (Model.ddDateRange >= 6)
                qry = qry +  Environment.NewLine  + "And ( (t1.LastSpeakerProfileUpdate < @PStart  ANd  t1.LastProfileUpdate < @PStart) )";

            if (Model.ddRating> 0)
                qry = qry + Environment.NewLine + " And t1.SpeakerRating =" +Model.ddRating.ToString();

            if (Model.ddSpeakerStatus > 0)
            {

                if (Model.strSpeakerStatus == "Special Arrangement")
                {
                    Model.bAcctSpecialArrangement = true;
                    qry = qry +  Environment.NewLine  + "And t1.SpecialArrangement is not null and t1.SpecialArrangement <> '' ";
                }
                else
                    qry = qry + Environment.NewLine  + " And isnull(t1.SpeakerStatus_pkey,0)=" + Model.ddSpeakerStatus.ToString();
            }
            else if (Model.ddSpeakerStatus ==-1)
                qry = qry + Environment.NewLine  + " And isnull(t1.PotentialSpeaker,0)=1 ";
            else if (Model.ddSpeakerStatus ==-2)
            {
                qry = qry + Environment.NewLine  + " AND  isnull(t0.ParticipationStatus_pKey,0)=2";
                if (Model.ddDateRange > 0)
                    qry = qry + Environment.NewLine  + " And t0.CancellationDate >=@PStart And t0.CancellationDate <=@PEnd";
            }
            else if (Model.ddSpeakerStatus == -3)
            {
                qry = qry + Environment.NewLine + " And (t1.pkey  NOT In ( select distinct t1.Account_pKey from Account_SessionEvents t0 INNER JOIN  Account_Sessions t1 ON t1.pkey=t0.AccountSession_pkey  and t0.Event_pKey=@SpkrCurEventPKey"
                   + Environment.NewLine + "Where t0.Event_pKey=@SpkrCurEventPKey  and ISNULL(homelession,'')<>'' ))";
            }
            switch (Model.ddAccStatus)
            {
                case 1: qry = qry + Environment.NewLine + " And t1.AccountStatus_pkey = 1"; break;
                case 2: qry = qry + Environment.NewLine + " And t1.AccountStatus_pkey = 2"; break;
                case 3: qry = qry + Environment.NewLine + " And t1.AccountStatus_pkey = 3"; break;
            }
            switch (Model.ddFinalDispVal)
            {
                case -1:
                    qry = qry + Environment.NewLine  + " And t10.pkey Is Null  AND   ((Select Count(pkey) from EventSession_Staff where  EventSession_pkey IN(Select pkey FROM Event_Sessions where event_pkey=t0.Event_pKey) AND Account_pKey=t1.pkey ANd ISNULL(IsSpeaker,0)=1)<=0) "; break;
                case -2:
                    qry = qry + Environment.NewLine  + " And  ((Select Count(pkey) from EventSession_Staff where  EventSession_pkey IN(Select pkey FROM Event_Sessions where event_pkey=t0.Event_pKey) AND Account_pKey=t1.pkey ANd ISNULL(IsSpeaker,0)=1)<=0) "; break;
                case 1:
                    qry = qry + Environment.NewLine  + " AND ((Select Count(pkey) from EventSession_Staff where  EventSession_pkey IN(Select pkey FROM Event_Sessions where event_pkey=t0.Event_pKey) AND Account_pKey=t1.pkey ANd ISNULL(IsSpeaker,0)=1)>0) AND   t0.ParticipationStatus_pKey<>2"; break;
            }
            if (Model.ddFinalDispVal>1)
                qry = qry + Environment.NewLine  + " AND ((Select Count(pkey) from EventSession_Staff where  EventSession_pkey IN(Select pkey FROM Event_Sessions where event_pkey=t0.Event_pKey) AND Account_pKey=t1.pkey ANd ISNULL(IsSpeaker,0)=1)<=0) And t10.pkey<>1 AND t10.pkey = " + Model.ddFinalDispVal.ToString();

            if (Model.ddContacted>0)
            {
                switch (Model.ddContacted)
                {
                    case 1:
                        qry = qry + Environment.NewLine + "And (isNull((Select  count(pkey) As CountPKey from EventAccount_Communication Where  UpdatedforAccount_pkey=t1.pkey AND ISNULL(AdditionalFollowup,0)=0 And event_pkey=@SpkrCurEventPKey group by UpdatedforAccount_pkey) ,0)>0"
                        + Environment.NewLine + "Or t7.ContactDate >= ( Select RegStartDate  from Event_List where pkey =@SpkrCurEventPKey ))";
                        break;
                    case 2:
                        qry = qry + Environment.NewLine  + "And (isNull((Select  count(pkey) As CountPKey from EventAccount_Communication Where UpdatedforAccount_pkey=t1.pkey AND ISNULL(AdditionalFollowup,0)=0 And ContactDate >= ( Select RegStartDate  from Event_List where pkey =@SpkrCurEventPKey ) group by UpdatedforAccount_pkey) ,0)=0)";
                        break;
                }
            }
            if (Model.ddSesStatus>0)
            {
                qry = qry + Environment.NewLine + " And t1.pkey"
                + Environment.NewLine + " In ("
                + Environment.NewLine + "Select  t1.pk "
                + Environment.NewLine + "From Account_List t1  "
                + Environment.NewLine + "Inner Join Account_Sessions t2 On t2.Account_pkey = t1.pKey "
                + Environment.NewLine + "Left Outer Join Account_SessionEvents t3 On t3.AccountSession_pkey = t2.pKey And t3.Event_pkey =@SpkrCurEventPKey"
                + Environment.NewLine + "Where ISNULL(t3.AssignmentStatus_pKey,27) = " + Model.ddSesStatus.ToString() + " Group by t1.pkey)";
            }
            else if (Model.ddSesStatus == -1)
            {
                qry = qry + Environment.NewLine + " And t1.pkey Not In  ("
                + Environment.NewLine + "Select  t1.pkey"
                + Environment.NewLine + "From Account_List t1  "
                + Environment.NewLine + "Inner Join Account_Sessions t2 On t2.Account_pkey = t1.pKey "
                + Environment.NewLine + "Left Outer Join Account_SessionEvents t3 On t3.AccountSession_pkey = t2.pKey And t3.Event_pkey =@SpkrCurEventPKey"
                + Environment.NewLine + "Where t3.AssignmentStatus_pKey  In (1,7,19,11,2) Group by t1.pkey)\" '= " + Model.ddSesStatus.ToString() + " Group by t1.pkey)";
            }
            else if (Model.ddSesStatus == -2)
            {
                qry = qry + Environment.NewLine +  " And t1.pkey  In  ( Select Account_pkey FRom EventSession_Staff t1 Where t1.EventSession_pkey in(Select pkey FRom  Event_Sessions Where Event_pKey=@SpkrCurEventPKey ) "
                 + Environment.NewLine +  "  ANd  ISNULL(IsSessionChair,0)=1  GROUP BY t1.Account_pKey)";
            }

            string strfollow = "";
            //Dim strfollow As String = ""
            //For Each ck As RadComboBoxItem In Me.ddFollowupRights2.CheckedItems
            //    If ck.Value = "0" Then
            //        bNoEvents = True
            //    Else
            //        If strfollow <> "" Then strfollow = strfollow + ","
            //        strfollow = strfollow + ck.Value
            //    End If
            //Next
            if (!string.IsNullOrEmpty(strfollow))
                qry = qry + Environment.NewLine + " AND ISNULL(af.event_pkey,@SpkrCurEventPKey)=@SpkrCurEventPKey AND   ISNULL(iif(af.FollowupRights_Pkey=0,4,af.FollowupRights_Pkey) ,4) in(  " + strfollow + ")";

            if (Model.chkAddedBy)
                qry = qry + Environment.NewLine + "And t1.pkey In(Select ISNULL(Account_pKey,0)As pkey from account_sessions Where  Event_pkey =@SpkrCurEventPKey And UpdatedBy_pKey!=Account_pKey Group by Account_pKey having Count(Account_pKey)>0)";


            //If myVS.Announcement_pkey > 0 And RdddStatus.SelectedValue = "-1" Then
            //    qry = qry + vbCrLf + "AND NOT (( Select Count(T9.pkey)  from  Email_List t9 where  ISNULL(t9.Reference_pKey,0) = @Event_pkey AND Email=t1.Email AND (t9.Sender_ID=" + myVS.Announcement_pkey.ToString + " OR  t9.Caption =@CAPTIONSTRING))>0)"
            //ElseIf myVS.Announcement_pkey > 0 And RdddStatus.SelectedValue = "-2" Then
            //    qry = qry + vbCrLf + "AND  (( Select Count(T9.pkey)  from  Email_List t9 where  ISNULL(t9.Reference_pKey,0) = @Event_pkey AND ISNULL(t9.LatestStatus,0)>=0 AND Email=t1.Email AND (t9.Sender_ID=" + myVS.Announcement_pkey.ToString + " OR  t9.Caption =@CAPTIONSTRING))>0)"
            //ElseIf myVS.Announcement_pkey > 0 And RdddStatus.SelectedValue = "-3" Then
            //    qry = qry + vbCrLf + "AND (( Select Count(T9.pkey)  from  Email_List t9 where   ISNULL(t9.Reference_pKey,0) = @Event_pkey AND Email=t1.Email AND t9.LatestStatus IN(" + myVS.Status_pkey.ToString + ") And  (t9.Sender_ID=" + myVS.Announcement_pkey.ToString + " Or t9.Caption =@CAPTIONSTRING))>0)"
            //Else
            //    If myVS.Announcement_pkey <> 0 AndAlso myVS.Status_pkey > "0" Then qry = qry + vbCrLf + "AND (( Select Count(T9.pkey)  from  Email_List t9 where   ISNULL(t9.Reference_pKey,0) = @Event_pkey AND  Email=t1.Email AND t9.LatestStatus IN(" + myVS.Status_pkey.ToString + ") And (t9.Sender_ID=" + myVS.Announcement_pkey.ToString + " Or  t9.Caption =@AnnouncementTest))>0)"
            //    If myVS.Announcement_pkey = 0 And myVS.Status_pkey <> "" AndAlso myVS.Status_pkey < "0" Then qry = qry + vbCrLf + "And ( ( Select Count(T9.pkey)  from  Email_List t9 where  ISNULL(t9.Reference_pKey,0) = @Event_pkey AND Email=t1.Email AND t9.LatestStatus<>5)>0)"
            //    If myVS.Announcement_pkey = 0 AndAlso myVS.Status_pkey > "0" Then qry = qry + vbCrLf + "And (( Select Count(T9.pkey)  from  Email_List t9 where  ISNULL(t9.Reference_pKey,0) = @Event_pkey AND Email=t1.Email AND t9.LatestStatus IN (" + myVS.Status_pkey.ToString + "))>0)"
            //    If myVS.Announcement_pkey > 0 AndAlso myVS.Status_pkey < "0" Then qry = qry + vbCrLf + "AND ( ( Select Count(T9.pkey)  from  Email_List t9 where  ISNULL(t9.Reference_pKey,0) = @Event_pkey AND Email=t1.Email AND t9.LatestStatus <>5 AND   (t9.Sender_ID=" + myVS.Announcement_pkey.ToString + " Or  t9.Caption =@AnnouncementTest)) >0)"
            //    If myVS.Announcement_pkey > 0 AndAlso myVS.Status_pkey = "0" Then qry = qry + vbCrLf + "AND ( ( Select Count(T9.pkey)  from  Email_List t9 where  ISNULL(t9.Reference_pKey,0) = @Event_pkey AND Email=t1.Email AND  (t9.Sender_ID=" + myVS.Announcement_pkey.ToString + " Or  t9.Caption =@AnnouncementTest)) >0)"
            //End If

            if (Model.ddTrack > 0)
            {
                qry = qry + Environment.NewLine + " And t1.pkey  In  ("
                + Environment.NewLine + "Select  t1.pkey From Account_List t1  Inner Join Account_Sessions t2 On t2.Account_pkey = t1.pKey INNER JOIN Session_List  t3 ON t2.Session_pKey=t3.pkey"
                + Environment.NewLine + "INNER JOIN Sys_Tracks t4 ON t3.Track_pKey=t4.pKey   "
                + Environment.NewLine + "Inner Join Event_Sessions t13 On t13.Session_pKey = t2.Session_pKey And t13.Event_pKey=@SpkrCurEventPKey"
                + Environment.NewLine + "Where t13.Track_pKey=" + Model.ddTrack.ToString() + " Group by t1.pkey)";
            }

            if (Model.ddPastActivity>0)
            {
                qry = qry + Environment.NewLine + " And t1.pkey  In  ("
                + Environment.NewLine + "Select t1.Account_pkey FROM EventSession_Staff t1 INNER JOIN Event_Sessions t2 ON t2.pkey=t1.EventSession_pkey Where (ISNULL(t1.AssignmentStatus_pKey,2) in(2,19) OR ISNULL(IsSpeaker,0) =1 OR  RatingAvg>0)  AND  t2.Session_pKey=" + Model.ddPastActivity.ToString() + " AND t2.Event_pKey<>@SpkrCurEventPKey GROUP BY t1.Account_pKey"
                + Environment.NewLine + " )";
            }
            if (Model.ddResult>0)
                qry = qry + Environment.NewLine + "  AND (ISNULL(t7.CallOutcome_pKey,0)=@Result_pkey)";

            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(qry);
            cmd.CommandTimeout = 300;

            if (!string.IsNullOrEmpty(Model.strSearch))
                cmd.Parameters.AddWithValue("@Name", Model.strSearch + "%");
            if (!string.IsNullOrEmpty(Model.strID))
                cmd.Parameters.AddWithValue("@PK", Model.strID + "%");
            else if (Convert.ToInt32(Model.strDemiSpkrMgID) > 0)
                cmd.Parameters.AddWithValue("@PK", Model.strDemiSpkrMgID + "%");

            if (!string.IsNullOrEmpty(Model.strOrg))
                cmd.Parameters.AddWithValue("@Org", "%" + Model.strOrg + "%");
            if (Model.ddDateType>0)
            {
                cmd.Parameters.AddWithValue("@Start", dtStart);
                cmd.Parameters.AddWithValue("@End", dtEnd);
            }
            if (Model.ddDateRange >0)
            {
                cmd.Parameters.AddWithValue("@PStart", dtPStart);
                cmd.Parameters.AddWithValue("@PEnd", dtPEnd);
            }
            if (!string.IsNullOrEmpty(Model.strnotes))
                cmd.Parameters.AddWithValue("@Notes", "%" + Model.strnotes + "%");
            if (Model.ddResult>0)
                cmd.Parameters.AddWithValue("@Result_pkey", Model.ddResult);

            //If myVS.strAnnouncement.ToString <> "" Then
            //    Dim strreplace As String = "(" + myVS.Announcement_pkey.ToString + ")"
            //    Dim strAnnouncement As String = myVS.strAnnouncement.Replace(strreplace, "").Trim()
            //    cmd.Parameters.AddWithValue("@CAPTIONSTRING", strAnnouncement.Trim)
            //End If

            Model.strAnnouncement =  (string.IsNullOrEmpty(Model.strAnnouncement) ? "" : Model.strAnnouncement);
            cmd.Parameters.AddWithValue("@Event_pkey", Model.intSpkrCurEventPKey);
            cmd.Parameters.AddWithValue("@SpkrCurEventPKey", Model.intSpkrCurEventPKey);
            cmd.Parameters.AddWithValue("@AnnouncementTest", Model.strAnnouncement);
            string strMsg = "";

            DataTable dt = new DataTable();
            try
            {
                string Connection = ReadConnectionString();
                SqlConnection conn = new SqlConnection(Connection);

                if (clsUtility.GetDataTable(conn, cmd, ref dt, Msg: strMsg))
                    return dt;
            }
            catch (Exception ex)
            {
            }
            return null;
        }
        private SpeakerNameColors ProcessNameColors(string list, int FlagValue)
        {
            SpeakerNameColors nameColors = new SpeakerNameColors();
            string wd = "";
            int i = 0, l = 0, r = 0, b = 0, g = 0, count = 0;

            string Colors = "Black";
            if (FlagValue>0)
                Colors = "Gray";
            else
            {
                IEnumerable<string> words = list.Split(',');

                foreach (string word in words)
                {
                    wd = word.ToUpper();
                    if (wd=="19")
                    {
                        Colors = "Green";
                        i = i + 1;
                        r = r + 1;
                        b = b + 1;
                        g = g + 1;
                    }
                    else if (i <= 0 &&  (wd=="2" || wd=="1"))
                    {
                        Colors = "LimeGreen";
                        i = i + 1;
                        r = r + 1;
                        b = b + 1;
                        g = g + 1;
                    }
                    else if (r <= 0)
                    {
                        if (wd == "12" || wd == "23" && (wd != "19" || wd != "2" || wd  != "1"))
                        {
                            Colors = "Red";
                            b = b + 1;
                            g = g + 1;
                        }
                        else if (b <= 0 && g <= 0 && wd == "8")
                        {
                            Colors = "Orange";
                            g = g + 1;
                        }
                        else if (g <= 0)
                        {
                            if (wd == "0" || wd == "" && (wd != "10" || wd != "11" || wd != "12" || wd != "23" || wd != "19" || wd != "2" || wd != "1"))
                                Colors = "Black";
                        }
                    }
                }
            }
            nameColors.NickNameForeColor = Colors;
            nameColors.LatForeColor = Colors;
            nameColors.FirstForeColor =Colors;
            nameColors.PhoneticNameForeColor = Colors;
            nameColors.SalutationForeColor  =Colors;
            return nameColors;
        }

        private int returntotalRecord(string intSpkrCurEventPKey, string streventInterested, string SelectedInterested)
        {
            int total = 0;
            string qry = "";
            SqlConnection sqlConnection = new SqlConnection(ReadConnectionString());
            try
            {
                if (SelectedInterested !="")
                    qry  = qry + Environment.NewLine + "Select COUNT(0)   from account_list t1  Where  t1.pkey in((select distinct account_pkey from Account_ExpressionProfile where event_pkey in(" + ((SelectedInterested != "") ? streventInterested.ToString() : intSpkrCurEventPKey) + ") )) OR(t1.GeneralInterestInBeingSpeaker = 1) OR(ISNULL(t1.PotentialSpeaker, 0)=1) Or(t1.pkey In(Select distinct account_pkey from account_sessions))";
                else
                    qry  = qry + Environment.NewLine + "Select COUNT(0)   from account_list t1  Where  t1.pkey in((select distinct account_pkey from Account_ExpressionProfile where event_pkey >0  )) OR (t1.GeneralInterestInBeingSpeaker = 1) OR (ISNULL(t1.PotentialSpeaker,0)=1) Or (t1.pkey In (Select distinct account_pkey from account_sessions))";

                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand(qry);
                if (clsUtility.GetDataTable(sqlConnection, cmd, ref dt))
                {
                    if (dt.Rows.Count > 0)
                        total = (dt.Rows[0][0] != DBNull.Value) ? Convert.ToInt32(dt.Rows[0][0]) : 0;
                }
            }
            catch (Exception ex)
            {
            }
            return total;
        }
        private string GetAttended(DataRow dr, string primaryevent, int currentEvent)
        {
            bool num = false;
            if (dr != null)
            {
                string Attended = ((dr["Attended"] == DBNull.Value) ? "" : dr["Attended"].ToString());
                num = new SpeakerManagementOperations().GetNumSpeakers(currentEvent, dr["Account_pKey"].ToString());
                string ReplaaceTest = Attended;
                if (Attended != "" && !num)
                    ReplaaceTest = Attended.Replace(primaryevent + ",", "").Replace(primaryevent, ""); //cSettings.strPrimaryEventID
                return ReplaaceTest.Trim(',');
            }
            return "";
        }
        private string GetPendingAccountName(DataRow dr, bool withoutnickname = false)
        {
            try
            {
                if (dr != null)
                {
                    string FirstName = ((dr["FirstName"] == DBNull.Value) ? "" : dr["FirstName"].ToString()),
                           NickName = ((dr["NickName"] == DBNull.Value) ? "" : dr["NickName"].ToString()),
                           LastName = ((dr["LastName"] == DBNull.Value) ? "" : dr["LastName"].ToString());
                    if (!withoutnickname)
                        return (!string.IsNullOrEmpty(FirstName) ? FirstName + " " : "") + (!string.IsNullOrEmpty(NickName) ? "("+  NickName + ") " : "") + LastName;
                    else
                        return (!string.IsNullOrEmpty(FirstName) ? FirstName + " " : "") + LastName;

                }
            }
            catch
            {

            }
            return "";
        }
        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult SpeakerGridData(SpeakerGridFilter Model)
        {
            if (Request.Headers["Model"] != null)
            {
                string data = Request.Headers["Model"].ToString();
                Model  =   JsonConvert.DeserializeObject<SpeakerGridFilter>(data);
            }
            if (Model == null)
                Model = ViewBag.SpeakerModel;
            DataTable dt = GetSpeakerGridDataTable(Model);
            List<SpeakerGridView> DataList = new List<SpeakerGridView>();
            int RowsCount = 0, TotalRecords = 0;
            string AlphaList = "";
            if (dt!= null && dt.Rows.Count>0)
            {
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                DataTable dataPriv = new SqlOperation().LoadAccountPrivilages(data.EventId, data.Id);
                bool EditLinkPrivilage = isAllowedByEntity(false, data.GlobalAdmin, clsUtility.TYPE_Account, clsPrivileges.FUNC_EDIT, dataPriv);

                RowsCount = dt.Rows.Count;
                TotalRecords = 0;
                if (dt.Rows[0]["TotalRecords"] != DBNull.Value)
                    TotalRecords=Convert.ToInt32(dt.Rows[0]["TotalRecords"]);

                AlphaList = string.Join(",", dt.AsEnumerable().Select(x => x["CH"]));
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);

                if (!string.IsNullOrEmpty(Model.strName))
                {
                    DataTable Gddt = new DataTable();
                    string str = Model.strName.Trim();
                    List<string> words = new List<string>();
                    DataRow[] dr;
                    string LastName = Model.strName.Trim();
                    if (words.Count>1)
                    {
                        LastName = clsUtility.NoLiterals(Model.strName.Trim());
                        if (words[0] != "")
                            dr = dt.Select("LastfirstName  like '" + LastName.Replace("'", "''").Trim() + "%'");
                        else
                            dr = dt.Select("FirstName  like '" + LastName.Replace("'", "''").Trim() + "%' OR " + "NickName  like '" + LastName.Replace("'", "''").Trim() + "%'");
                    }
                    else
                        dr = dt.Select("LastName  like '" +LastName.Replace("'", "''").Trim() + "%' OR NonUnicodeLastName  like '" + LastName.Replace("'", "''").Trim() + "%' ");

                    if (dr.Count()>0)
                    {
                        Gddt = dr.CopyToDataTable();
                        string str2 = Model.strName.Trim();
                        RowsCount = Gddt.Rows.Count;
                        TotalRecords = 0;
                        string SelectedInterested = "";
                        if (!string.IsNullOrEmpty(Model.cbddlInterested))
                            SelectedInterested = Model.cbddlInterested;

                        if (Gddt.Rows.Count > 0)
                            TotalRecords=returntotalRecord(Model.intSpkrCurEventPKey.ToString(), Model.streventInterested, SelectedInterested); //returntotalRecord()
                        //            If bExport Then
                        //                myVS.strPendingPKeys = clsUtility.getGridSelections(Me.dgAcct, "Account_pKey")
                        //                Dim items As String() = myVS.strPendingPKeys.Split(",".ToCharArray())
                        //                SelectedCount = items.Count
                        //                If SelectedCount = 1 Then
                        //                    If Me.SendAnn.SendAnnouncement(Gddt, Me.dgAcct, "Account_pKey", myVS.strPendingPKeys, IshowPreview:=True) Then Me.AnnouncementWasSent()
                        //                    Exit Sub
                        //                Else
                        //                    If Me.SendAnn.SendAnnouncement(Gddt, Me.dgAcct, "Account_pKey", myVS.strPendingPKeys) Then Me.AnnouncementWasSent()
                        //                    Exit Sub
                        //                End If
                        //            End If
                        DataList = Gddt.AsEnumerable().Select(x => new SpeakerGridView
                        {
                            pKey = ((x["pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(x["pKey"])),
                            EditLink =  EditLinkPrivilage,
                            ContactName = x["ContactName"].ToString(),
                            PaddedID = x["PaddedID"].ToString(),
                            Email = x["Email"].ToString(),
                            OrganizationID = x["OrganizationID"].ToString(),
                            Title = x["Title"].ToString(),
                            AccountPkey = x["Account_pKey"].ToString(),
                            SpkrRating = ((x["SpkrRating"] == DBNull.Value) ? "" : x["SpkrRating"].ToString()),
                            SpkrRating1 = ((x["SpkrRating1"] == DBNull.Value) ? "" : x["SpkrRating1"].ToString()),
                            PSUpdate = ((x["PSUpdate"] == DBNull.Value) ? "" : x["PSUpdate"].ToString()),
                            lastUpdatedP_S = ((x["lastUpdatedP_S"] == DBNull.Value) ? new DateTime() : Convert.ToDateTime(x["lastUpdatedP_S"])),
                            ExpressionDate = ((x["ExpressionDate"] == DBNull.Value) ? "" : Convert.ToDateTime(x["ExpressionDate"]).ToString("MM/dd/yyyy")),
                            NumContacts = ((x["NumContacts"] == DBNull.Value) ? 0 : Convert.ToInt32(x["NumContacts"])),
                            Con_ToolTips = ((x["Con_ToolTips"] == DBNull.Value) ? "" : Convert.ToString(x["Con_ToolTips"])),
                            SpkrNextContact = ((x["SpkrNextContact"] == DBNull.Value) ? "" : (Convert.ToString(x["SpkrNextContact"]).Replace("href=\"", "href=\"/"))),
                            SpkrNextContactShort = ((x["SpkrNextContactShort"] == DBNull.Value) ? "" : Convert.ToString(x["SpkrNextContactShort"])),
                            FollowNotesToolTip  = ((x["FollowNotesToolTip"] == DBNull.Value) ? "" : Convert.ToString(x["FollowNotesToolTip"])),
                            OnlyNotes = ((x["OnlyNotes"] == DBNull.Value) ? "" : Convert.ToString(x["OnlyNotes"])),
                            NextFiveNotes = ((x["NextFiveNotes"] == DBNull.Value) ? "" : Convert.ToString(x["NextFiveNotes"])),
                            All_Notes = ((x["All_Notes"] == DBNull.Value) ? "" : Convert.ToString(x["All_Notes"])),
                            All_Notes_Short = ((x["All_Notes_Short"] == DBNull.Value) ? "" : Convert.ToString(x["All_Notes_Short"])),
                            LastName = ((x["LastName"] == DBNull.Value) ? "" : Convert.ToString(x["LastName"])),
                            chkHideNotes = Model.chkHideNotes,
                            FirstName = ((x["FirstName"] == DBNull.Value) ? "" : Convert.ToString(x["FirstName"])),
                            NickName = ((x["NickName"] == DBNull.Value) ? "" : Convert.ToString((x["NickName"] != null) ? x["NickName"] : "")),
                            PhoneticName = ((x["PhoneticName"] == DBNull.Value) ? "" : Convert.ToString(x["PhoneticName"])),
                            Salutation = ((x["Salutation"] == DBNull.Value) ? "" : Convert.ToString(x["Salutation"])),
                            Department = ((x["Department"] == DBNull.Value) ? "" : Convert.ToString(x["Department"])),
                            OrganizationTypeID = ((x["OrganizationTypeID"] == DBNull.Value) ? "" : Convert.ToString(x["OrganizationTypeID"])),
                            Degrees = ((x["Degrees"] == DBNull.Value) ? "" : Convert.ToString(x["Degrees"])),
                            TTip = ((x["TTip"] == DBNull.Value) ? "" : Convert.ToString(x["TTip"])),
                            PersonalBio = ((x["PersonalBio"] == DBNull.Value) ? "" : Convert.ToString(x["PersonalBio"])),
                            AboutMe = ((x["AboutMe"] == DBNull.Value) ? "" : Convert.ToString(x["AboutMe"])),
                            OrgURL = ((x["OrgURL"] == DBNull.Value) ? "" : Convert.ToString(x["OrgURL"])),
                            PhoneCall_1 = ((x["PhoneCall_1"] == DBNull.Value) ? "" : Convert.ToString(x["PhoneCall_1"])),
                            PhoneCall1 = ((x["PhoneCall1"] == DBNull.Value) ? "" : Convert.ToString(x["PhoneCall1"])),
                            PhoneCall2 = ((x["PhoneCall2"] == DBNull.Value) ? "" : Convert.ToString(x["PhoneCall2"])),
                            PhoneCall_2 = ((x["PhoneCall_2"] == DBNull.Value) ? "" : Convert.ToString(x["PhoneCall_2"])),
                            SkypeAddress = ((x["SkypeAddress"] == DBNull.Value) ? "" : Convert.ToString(x["SkypeAddress"])),
                            EmailAddress = ((x["EmailAddress"] == DBNull.Value) ? "" : Convert.ToString(x["EmailAddress"])),
                            Phone = ((x["Phone"] == DBNull.Value) ? "" : Convert.ToString(x["Phone"])),
                            TimeTooltips = ((x["TimeTooltips"] == DBNull.Value) ? "" : Convert.ToString(x["TimeTooltips"])),
                            TimeZone = ((x["TimeZone"] == DBNull.Value) ? "" : Convert.ToString(x["TimeZone"])),
                            EventAccount_pKey = ((x["EventAccount_pKey"] == DBNull.Value) ? "" : Convert.ToString(x["EventAccount_pKey"])),
                            LinkedInProfile = ((x["LinkedInProfile"] == DBNull.Value) ? "" : Convert.ToString(x["LinkedInProfile"])),
                            SessionPriorities_Color = ((x["SessionPriorities_Color"] == DBNull.Value) ? "" : Convert.ToString(x["SessionPriorities_Color"])),
                            IsNoteShow = ((x["IsNoteShow"] == DBNull.Value) ? "" : Convert.ToString(x["IsNoteShow"])),
                            IsShowPreFlag = ((x["IsShowPreFlag"] == DBNull.Value) ? "" : Convert.ToString(x["IsShowPreFlag"])),
                            PotentialSpeaker = ((x["PotentialSpeaker"] == DBNull.Value) ? "" : Convert.ToString(x["PotentialSpeaker"])),
                            FollowUpRights = ((x["FollowUpRights"] == DBNull.Value) ? "" : Convert.ToString(x["FollowUpRights"])),
                            Followupright_Pkey = ((x["Followupright_Pkey"] == DBNull.Value) ? "" : Convert.ToString(x["Followupright_Pkey"])),
                            FlagTooltips = ((x["FlagTooltips"] == DBNull.Value) ? "" : Convert.ToString(x["FlagTooltips"])),
                            Problem = ((x["Problem"] == DBNull.Value) ? "" : Convert.ToString(x["Problem"])),
                            SpkrFlag = ((x["SpkrFlag"] == DBNull.Value) ? "" : Convert.ToString(x["SpkrFlag"])),
                            ShowLinkedInProfile = ((x["ShowLinkedInProfile"] == DBNull.Value) ? "" : Convert.ToString(x["ShowLinkedInProfile"])),
                            ShowSpeakerFlag = ((x["ShowSpeakerFlag"] == DBNull.Value) ? "" : Convert.ToString(x["ShowSpeakerFlag"])),
                            AFShow = ((x["AFShow"] == DBNull.Value) ? "" : Convert.ToString(x["AFShow"])),
                            AFBackColor = ((x["AFBackColor"] == DBNull.Value) ? "" : Convert.ToString(x["AFBackColor"])),
                            PImage = ((x["PImage"] == DBNull.Value) ? "" : Convert.ToString(x["PImage"])),
                            BIOINFO  = ((x["BIOINFO"] == DBNull.Value) ? "" : Convert.ToString(x["BIOINFO"])),
                            FinalDisp  = ((x["FinalDisp"] == DBNull.Value) ? "" : Convert.ToString(x["FinalDisp"])),
                            FinalDispVisible = Model.ckFinalDisp,
                            WritingArticle = ((x["WritingArticle"] == DBNull.Value) ? false : Convert.ToBoolean(x["WritingArticle"])),
                            strRIcon = CheckIcons(((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString())),
                            SpkrRatingVisible = (((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString()) != "0"  && ((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString()) != (clsFeedback.RATING_EXCELLENT).ToString()) ? true : false,
                            imgStarVisible  = (((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString()) != "0"  && ((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString()) == (clsFeedback.RATING_EXCELLENT).ToString()) ? true : false,
                            RedStarVisible  = (((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString()) != "0"  && ((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString()) == (clsFeedback.RATING_GOOD).ToString()) ? true : false,
                            SpkrStatusText =  ((x["SpkrStatusText"] == DBNull.Value) ? "" : x["SpkrStatusText"].ToString()),
                            SpeakerStatus_pkey =  ((x["SpeakerStatus_pkey"] == DBNull.Value) ? "" : x["SpeakerStatus_pkey"].ToString()),
                            SpkrStatusBackColor = ((x["SpkrStatusBackColor"] == DBNull.Value) ? "White" : x["SpkrStatusBackColor"].ToString()),
                            SpeakerStatusVisible = (((x["SpeakerStatus_pkey"] == DBNull.Value) ? "0" : x["SpeakerStatus_pkey"].ToString()) == "0") ? false : true,
                            SpkrStatusForeColor = (((x["SpeakerStatus_pkey"] == DBNull.Value) ? "0" : x["SpeakerStatus_pkey"].ToString()) == "0") ? "Black" : "White",
                            AccStatusText =  ((x["AccStatusText"] == DBNull.Value) ? "" : x["AccStatusText"].ToString()),
                            AccStatusBackColor = ((x["AccStatusBackColor"] == DBNull.Value) ? "White" : x["AccStatusBackColor"].ToString()),
                            AccStatusVisible = (((x["AccountStatus_pkey"] == DBNull.Value) ? "0" : x["AccountStatus_pkey"].ToString()) == "0") ? false : true,
                            AccStatusForeColor = (((x["AccountStatus_pkey"] == DBNull.Value) ? "0" : x["AccountStatus_pkey"].ToString()) == "0") ? "Black" : "White",
                            PrioritySpkr = ((x["PrioritySpkr"] == DBNull.Value) ? "" : x["PrioritySpkr"].ToString()),
                            Proposal = ((x["Proposal"] == DBNull.Value) ? "" : x["Proposal"].ToString()),
                            Attended =GetAttended(x, cSettings.strPrimaryEventID, Model.intSpkrCurEventPKey),
                            FBackColor = ((x["FBackColor"] == DBNull.Value) ? "" : Convert.ToString(x["FBackColor"])),
                            PronunicationURL = ((x["PronunicationURL"] == DBNull.Value) ? "" : Convert.ToString(x["PronunicationURL"]) + "?V=" + Guid.NewGuid().ToString()),
                            TypeNamePhonetic = TypeNames(((x["PronunicationURL"] == DBNull.Value) ? "" : Convert.ToString(x["PronunicationURL"])), ((x["NickNames"] == DBNull.Value) ? "" : Convert.ToString(x["NickNames"])), ((x["PhoneticName"] == DBNull.Value) ? "" : Convert.ToString(x["PhoneticName"])), ((x["LastName"] == DBNull.Value) ? "" : Convert.ToString(x["LastName"]))),
                            NameColors = ProcessNameColors(((x["List"] == DBNull.Value) ? "" : Convert.ToString(x["List"])), ((x["SpkrFlag"] == DBNull.Value) ? 0 : Convert.ToInt32(x["SpkrFlag"]))),
                            List = ((x["List"] == DBNull.Value) ? "" : Convert.ToString(x["List"])),
                            NextFollowUpdate = ((x["NextFollowUpdate"] == DBNull.Value) ? "" : Convert.ToString(x["NextFollowUpdate"])),
                            PendingAccountName = GetPendingAccountName(x),
                            PendingAcName = GetPendingAccountName(x, true),
                            ProducerReport = ((x["ProducerReport_Selection"] == DBNull.Value) ? "" : Convert.ToString(x["ProducerReport_Selection"])),
                            HasPosted = ((x["HasPosted"] == DBNull.Value) ? false : Convert.ToBoolean(x["HasPosted"])),
                            SpkrRatingAvailable = (((x["SpkrRating"] == DBNull.Value) ? "" : x["SpkrRating"].ToString()) != "")
                        }).ToList<SpeakerGridView>();
                    }
                    else
                        DataList = dt.AsEnumerable().Select(x => new SpeakerGridView
                        {
                            pKey = ((x["pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(x["pKey"])),
                            EditLink =  EditLinkPrivilage,
                            ContactName = x["ContactName"].ToString(),
                            PaddedID = x["PaddedID"].ToString(),
                            Email = x["Email"].ToString(),
                            OrganizationID = x["OrganizationID"].ToString(),
                            Title = x["Title"].ToString(),
                            AccountPkey = x["Account_pKey"].ToString(),
                            SpkrRating = ((x["SpkrRating"] == DBNull.Value) ? "" : x["SpkrRating"].ToString()),
                            SpkrRating1 = ((x["SpkrRating1"] == DBNull.Value) ? "" : x["SpkrRating1"].ToString()),
                            PSUpdate = ((x["PSUpdate"] == DBNull.Value) ? "" : x["PSUpdate"].ToString()),
                            lastUpdatedP_S = ((x["lastUpdatedP_S"] == DBNull.Value) ? new DateTime() : Convert.ToDateTime(x["lastUpdatedP_S"])),
                            ExpressionDate = ((x["ExpressionDate"] == DBNull.Value) ? "" : Convert.ToDateTime(x["ExpressionDate"]).ToString("MM/dd/yyyy")),
                            NumContacts = ((x["NumContacts"] == DBNull.Value) ? 0 : Convert.ToInt32(x["NumContacts"])),
                            Con_ToolTips = ((x["Con_ToolTips"] == DBNull.Value) ? "" : Convert.ToString(x["Con_ToolTips"])),
                            SpkrNextContact = ((x["SpkrNextContact"] == DBNull.Value) ? "" : (Convert.ToString(x["SpkrNextContact"]).Replace("href=\"", "href=\"/"))),
                            SpkrNextContactShort = ((x["SpkrNextContactShort"] == DBNull.Value) ? "" : Convert.ToString(x["SpkrNextContactShort"])),
                            FollowNotesToolTip  = ((x["FollowNotesToolTip"] == DBNull.Value) ? "" : Convert.ToString(x["FollowNotesToolTip"])),
                            OnlyNotes = ((x["OnlyNotes"] == DBNull.Value) ? "" : Convert.ToString(x["OnlyNotes"])),
                            NextFiveNotes = ((x["NextFiveNotes"] == DBNull.Value) ? "" : Convert.ToString(x["NextFiveNotes"])),
                            All_Notes = ((x["All_Notes"] == DBNull.Value) ? "" : Convert.ToString(x["All_Notes"])),
                            All_Notes_Short = ((x["All_Notes_Short"] == DBNull.Value) ? "" : Convert.ToString(x["All_Notes_Short"])),
                            LastName = ((x["LastName"] == DBNull.Value) ? "" : Convert.ToString(x["LastName"])),
                            chkHideNotes = Model.chkHideNotes,
                            FirstName = ((x["FirstName"] == DBNull.Value) ? "" : Convert.ToString(x["FirstName"])),
                            NickName = ((x["NickName"] == DBNull.Value) ? "" : Convert.ToString(x["NickName"])),
                            PhoneticName = ((x["PhoneticName"] == DBNull.Value) ? "" : Convert.ToString(x["PhoneticName"])),
                            Salutation = ((x["Salutation"] == DBNull.Value) ? "" : Convert.ToString(x["Salutation"])),
                            Department = ((x["Department"] == DBNull.Value) ? "" : Convert.ToString(x["Department"])),
                            OrganizationTypeID = ((x["OrganizationTypeID"] == DBNull.Value) ? "" : Convert.ToString(x["OrganizationTypeID"])),
                            Degrees = ((x["Degrees"] == DBNull.Value) ? "" : Convert.ToString(x["Degrees"])),
                            TTip = ((x["TTip"] == DBNull.Value) ? "" : Convert.ToString(x["TTip"])),
                            WritingArticle = ((x["WritingArticle"] == DBNull.Value) ? false : Convert.ToBoolean(x["WritingArticle"])),
                            PersonalBio = ((x["PersonalBio"] == DBNull.Value) ? "" : Convert.ToString(x["PersonalBio"])),
                            AboutMe = ((x["AboutMe"] == DBNull.Value) ? "" : Convert.ToString(x["AboutMe"])),
                            OrgURL = ((x["OrgURL"] == DBNull.Value) ? "" : Convert.ToString(x["OrgURL"])),
                            PhoneCall_1 = ((x["PhoneCall_1"] == DBNull.Value) ? "" : Convert.ToString(x["PhoneCall_1"])),
                            PhoneCall1 = ((x["PhoneCall1"] == DBNull.Value) ? "" : Convert.ToString(x["PhoneCall1"])),
                            PhoneCall2 = ((x["PhoneCall2"] == DBNull.Value) ? "" : Convert.ToString(x["PhoneCall2"])),
                            PhoneCall_2 = ((x["PhoneCall_2"] == DBNull.Value) ? "" : Convert.ToString(x["PhoneCall_2"])),
                            SkypeAddress = ((x["SkypeAddress"] == DBNull.Value) ? "" : Convert.ToString(x["SkypeAddress"])),
                            EmailAddress = ((x["EmailAddress"] == DBNull.Value) ? "" : Convert.ToString(x["EmailAddress"])),
                            Phone = ((x["Phone"] == DBNull.Value) ? "" : Convert.ToString(x["Phone"])),
                            TimeTooltips = ((x["TimeTooltips"] == DBNull.Value) ? "" : Convert.ToString(x["TimeTooltips"])),
                            TimeZone = ((x["TimeZone"] == DBNull.Value) ? "" : Convert.ToString(x["TimeZone"])),
                            EventAccount_pKey = ((x["EventAccount_pKey"] == DBNull.Value) ? "" : Convert.ToString(x["EventAccount_pKey"])),
                            LinkedInProfile = ((x["LinkedInProfile"] == DBNull.Value) ? "" : Convert.ToString(x["LinkedInProfile"])),
                            SessionPriorities_Color = ((x["SessionPriorities_Color"] == DBNull.Value) ? "" : Convert.ToString(x["SessionPriorities_Color"])),
                            IsNoteShow = ((x["IsNoteShow"] == DBNull.Value) ? "" : Convert.ToString(x["IsNoteShow"])),
                            IsShowPreFlag = ((x["IsShowPreFlag"] == DBNull.Value) ? "" : Convert.ToString(x["IsShowPreFlag"])),
                            PotentialSpeaker = ((x["PotentialSpeaker"] == DBNull.Value) ? "" : Convert.ToString(x["PotentialSpeaker"])),
                            FollowUpRights = ((x["FollowUpRights"] == DBNull.Value) ? "" : Convert.ToString(x["FollowUpRights"])),
                            Followupright_Pkey = ((x["Followupright_Pkey"] == DBNull.Value) ? "" : Convert.ToString(x["Followupright_Pkey"])),
                            FlagTooltips = ((x["FlagTooltips"] == DBNull.Value) ? "" : Convert.ToString(x["FlagTooltips"])),
                            Problem = ((x["Problem"] == DBNull.Value) ? "" : Convert.ToString(x["Problem"])),
                            SpkrFlag = ((x["SpkrFlag"] == DBNull.Value) ? "" : Convert.ToString(x["SpkrFlag"])),
                            ShowLinkedInProfile = ((x["ShowLinkedInProfile"] == DBNull.Value) ? "" : Convert.ToString(x["ShowLinkedInProfile"])),
                            ShowSpeakerFlag = ((x["ShowSpeakerFlag"] == DBNull.Value) ? "" : Convert.ToString(x["ShowSpeakerFlag"])),
                            AFShow = ((x["AFShow"] == DBNull.Value) ? "" : Convert.ToString(x["AFShow"])),
                            AFBackColor = ((x["AFBackColor"] == DBNull.Value) ? "" : Convert.ToString(x["AFBackColor"])),
                            PImage = ((x["PImage"] == DBNull.Value) ? "" : Convert.ToString(x["PImage"])),
                            BIOINFO  = ((x["BIOINFO"] == DBNull.Value) ? "" : Convert.ToString(x["BIOINFO"])),
                            FinalDisp  = ((x["FinalDisp"] == DBNull.Value) ? "" : Convert.ToString(x["FinalDisp"])),
                            FinalDispVisible = Model.ckFinalDisp,
                            strRIcon = CheckIcons(((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString())),
                            SpkrRatingVisible = (((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString()) != "0"  && ((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString()) != (clsFeedback.RATING_EXCELLENT).ToString()) ? true : false,
                            imgStarVisible  = (((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString()) != "0"  && ((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString()) == (clsFeedback.RATING_EXCELLENT).ToString()) ? true : false,
                            RedStarVisible  = (((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString()) != "0"  && ((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString()) == (clsFeedback.RATING_GOOD).ToString()) ? true : false,
                            SpkrStatusText =  ((x["SpkrStatusText"] == DBNull.Value) ? "" : x["SpkrStatusText"].ToString()),
                            SpeakerStatus_pkey =  ((x["SpeakerStatus_pkey"] == DBNull.Value) ? "" : x["SpeakerStatus_pkey"].ToString()),
                            SpkrStatusBackColor = ((x["SpkrStatusBackColor"] == DBNull.Value) ? "White" : x["SpkrStatusBackColor"].ToString()),
                            SpeakerStatusVisible = (((x["SpeakerStatus_pkey"] == DBNull.Value) ? "0" : x["SpeakerStatus_pkey"].ToString()) == "0") ? false : true,
                            SpkrStatusForeColor = (((x["SpeakerStatus_pkey"] == DBNull.Value) ? "0" : x["SpeakerStatus_pkey"].ToString()) == "0") ? "Black" : "White",
                            AccStatusText =  ((x["AccStatusText"] == DBNull.Value) ? "" : x["AccStatusText"].ToString()),
                            AccStatusBackColor = ((x["AccStatusBackColor"] == DBNull.Value) ? "White" : x["AccStatusBackColor"].ToString()),
                            AccStatusVisible = (((x["AccountStatus_pkey"] == DBNull.Value) ? "0" : x["AccountStatus_pkey"].ToString()) == "0") ? false : true,
                            AccStatusForeColor = (((x["AccountStatus_pkey"] == DBNull.Value) ? "0" : x["AccountStatus_pkey"].ToString()) == "0") ? "Black" : "White",
                            PrioritySpkr = ((x["PrioritySpkr"] == DBNull.Value) ? "" : x["PrioritySpkr"].ToString()),
                            Proposal = ((x["Proposal"] == DBNull.Value) ? "" : x["Proposal"].ToString()),
                            Attended =GetAttended(x, cSettings.strPrimaryEventID, Model.intSpkrCurEventPKey),
                            FBackColor = ((x["FBackColor"] == DBNull.Value) ? "" : Convert.ToString(x["FBackColor"])),
                            PronunicationURL = ((x["PronunicationURL"] == DBNull.Value) ? "" : Convert.ToString(x["PronunicationURL"]) + "?V=" + Guid.NewGuid().ToString()),
                            TypeNamePhonetic = TypeNames(((x["PronunicationURL"] == DBNull.Value) ? "" : Convert.ToString(x["PronunicationURL"])), ((x["NickNames"] == DBNull.Value) ? "" : Convert.ToString(x["NickNames"])), ((x["PhoneticName"] == DBNull.Value) ? "" : Convert.ToString(x["PhoneticName"])), ((x["LastName"] == DBNull.Value) ? "" : Convert.ToString(x["LastName"]))),
                            NameColors = ProcessNameColors(((x["List"] == DBNull.Value) ? "" : Convert.ToString(x["List"])), ((x["SpkrFlag"] == DBNull.Value) ? 0 : Convert.ToInt32(x["SpkrFlag"]))),
                            List = ((x["List"] == DBNull.Value) ? "" : Convert.ToString(x["List"])),
                            NextFollowUpdate = ((x["NextFollowUpdate"] == DBNull.Value) ? "" : Convert.ToString(x["NextFollowUpdate"])),
                            PendingAccountName = GetPendingAccountName(x),
                            PendingAcName = GetPendingAccountName(x, true),
                            ProducerReport = ((x["ProducerReport_Selection"] == DBNull.Value) ? "" : Convert.ToString(x["ProducerReport_Selection"])),
                            HasPosted = ((x["HasPosted"] == DBNull.Value) ? false : Convert.ToBoolean(x["HasPosted"])),
                            SpkrRatingAvailable = (((x["SpkrRating"] == DBNull.Value) ? "" : x["SpkrRating"].ToString()) != "")
                        }).ToList<SpeakerGridView>();
                }
                else
                    DataList = dt.AsEnumerable().Select(x => new SpeakerGridView
                    {
                        pKey = ((x["pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(x["pKey"])),
                        EditLink =  EditLinkPrivilage,
                        ContactName = x["ContactName"].ToString(),
                        PaddedID = x["PaddedID"].ToString(),
                        Email = x["Email"].ToString(),
                        OrganizationID = x["OrganizationID"].ToString(),
                        Title = x["Title"].ToString(),
                        AccountPkey = x["Account_pKey"].ToString(),
                        SpkrRating = ((x["SpkrRating"] == DBNull.Value) ? "" : x["SpkrRating"].ToString()),
                        SpkrRating1 = ((x["SpkrRating1"] == DBNull.Value) ? "" : x["SpkrRating1"].ToString()),
                        PSUpdate = ((x["PSUpdate"] == DBNull.Value) ? "" : x["PSUpdate"].ToString()),
                        lastUpdatedP_S = ((x["lastUpdatedP_S"] == DBNull.Value) ? new DateTime() : Convert.ToDateTime(x["lastUpdatedP_S"])),
                        ExpressionDate = ((x["ExpressionDate"] == DBNull.Value) ? "" : Convert.ToDateTime(x["ExpressionDate"]).ToString("MM/dd/yyyy")),
                        NumContacts = ((x["NumContacts"] == DBNull.Value) ? 0 : Convert.ToInt32(x["NumContacts"])),
                        Con_ToolTips = ((x["Con_ToolTips"] == DBNull.Value) ? "" : Convert.ToString(x["Con_ToolTips"])),
                        SpkrNextContact = ((x["SpkrNextContact"] == DBNull.Value) ? "" : (Convert.ToString(x["SpkrNextContact"]).Replace("href=\"", "href=\"/"))),
                        SpkrNextContactShort = ((x["SpkrNextContactShort"] == DBNull.Value) ? "" : Convert.ToString(x["SpkrNextContactShort"])),
                        FollowNotesToolTip  = ((x["FollowNotesToolTip"] == DBNull.Value) ? "" : Convert.ToString(x["FollowNotesToolTip"])),
                        OnlyNotes = ((x["OnlyNotes"] == DBNull.Value) ? "" : Convert.ToString(x["OnlyNotes"])),
                        NextFiveNotes = ((x["NextFiveNotes"] == DBNull.Value) ? "" : Convert.ToString(x["NextFiveNotes"])),
                        All_Notes = ((x["All_Notes"] == DBNull.Value) ? "" : Convert.ToString(x["All_Notes"])),
                        All_Notes_Short = ((x["All_Notes_Short"] == DBNull.Value) ? "" : Convert.ToString(x["All_Notes_Short"])),
                        LastName = ((x["LastName"] == DBNull.Value) ? "" : Convert.ToString(x["LastName"])),
                        chkHideNotes = Model.chkHideNotes,
                        FirstName = ((x["FirstName"] == DBNull.Value) ? "" : Convert.ToString(x["FirstName"])),
                        WritingArticle = ((x["WritingArticle"] == DBNull.Value) ? false : Convert.ToBoolean(x["WritingArticle"])),
                        NickName = ((x["NickName"] == DBNull.Value) ? "" : Convert.ToString(x["NickName"])),
                        PhoneticName = ((x["PhoneticName"] == DBNull.Value) ? "" : Convert.ToString(x["PhoneticName"])),
                        Salutation = ((x["Salutation"] == DBNull.Value) ? "" : Convert.ToString(x["Salutation"])),
                        Department = ((x["Department"] == DBNull.Value) ? "" : Convert.ToString(x["Department"])),
                        OrganizationTypeID = ((x["OrganizationTypeID"] == DBNull.Value) ? "" : Convert.ToString(x["OrganizationTypeID"])),
                        Degrees = ((x["Degrees"] == DBNull.Value) ? "" : Convert.ToString(x["Degrees"])),
                        TTip = ((x["TTip"] == DBNull.Value) ? "" : Convert.ToString(x["TTip"])),
                        PersonalBio = ((x["PersonalBio"] == DBNull.Value) ? "" : Convert.ToString(x["PersonalBio"])),
                        AboutMe = ((x["AboutMe"] == DBNull.Value) ? "" : Convert.ToString(x["AboutMe"])),
                        OrgURL = ((x["OrgURL"] == DBNull.Value) ? "" : Convert.ToString(x["OrgURL"])),
                        PhoneCall_1 = ((x["PhoneCall_1"] == DBNull.Value) ? "" : Convert.ToString(x["PhoneCall_1"])),
                        PhoneCall1 = ((x["PhoneCall1"] == DBNull.Value) ? "" : Convert.ToString(x["PhoneCall1"])),
                        PhoneCall2 = ((x["PhoneCall2"] == DBNull.Value) ? "" : Convert.ToString(x["PhoneCall2"])),
                        PhoneCall_2 = ((x["PhoneCall_2"] == DBNull.Value) ? "" : Convert.ToString(x["PhoneCall_2"])),
                        SkypeAddress = ((x["SkypeAddress"] == DBNull.Value) ? "" : Convert.ToString(x["SkypeAddress"])),
                        EmailAddress = ((x["EmailAddress"] == DBNull.Value) ? "" : Convert.ToString(x["EmailAddress"])),
                        Phone = ((x["Phone"] == DBNull.Value) ? "" : Convert.ToString(x["Phone"])),
                        TimeTooltips = ((x["TimeTooltips"] == DBNull.Value) ? "" : Convert.ToString(x["TimeTooltips"])),
                        TimeZone = ((x["TimeZone"] == DBNull.Value) ? "" : Convert.ToString(x["TimeZone"])),
                        EventAccount_pKey = ((x["EventAccount_pKey"] == DBNull.Value) ? "" : Convert.ToString(x["EventAccount_pKey"])),
                        LinkedInProfile = ((x["LinkedInProfile"] == DBNull.Value) ? "" : Convert.ToString(x["LinkedInProfile"])),
                        SessionPriorities_Color = ((x["SessionPriorities_Color"] == DBNull.Value) ? "" : Convert.ToString(x["SessionPriorities_Color"])),
                        IsNoteShow = ((x["IsNoteShow"] == DBNull.Value) ? "" : Convert.ToString(x["IsNoteShow"])),
                        IsShowPreFlag = ((x["IsShowPreFlag"] == DBNull.Value) ? "" : Convert.ToString(x["IsShowPreFlag"])),
                        PotentialSpeaker = ((x["PotentialSpeaker"] == DBNull.Value) ? "" : Convert.ToString(x["PotentialSpeaker"])),
                        FollowUpRights = ((x["FollowUpRights"] == DBNull.Value) ? "" : Convert.ToString(x["FollowUpRights"])),
                        Followupright_Pkey = ((x["Followupright_Pkey"] == DBNull.Value) ? "" : Convert.ToString(x["Followupright_Pkey"])),
                        FlagTooltips = ((x["FlagTooltips"] == DBNull.Value) ? "" : Convert.ToString(x["FlagTooltips"])),
                        Problem = ((x["Problem"] == DBNull.Value) ? "" : Convert.ToString(x["Problem"])),
                        SpkrFlag = ((x["SpkrFlag"] == DBNull.Value) ? "" : Convert.ToString(x["SpkrFlag"])),
                        ShowLinkedInProfile = ((x["ShowLinkedInProfile"] == DBNull.Value) ? "" : Convert.ToString(x["ShowLinkedInProfile"])),
                        ShowSpeakerFlag = ((x["ShowSpeakerFlag"] == DBNull.Value) ? "" : Convert.ToString(x["ShowSpeakerFlag"])),
                        AFShow = ((x["AFShow"] == DBNull.Value) ? "" : Convert.ToString(x["AFShow"])),
                        AFBackColor = ((x["AFBackColor"] == DBNull.Value) ? "" : Convert.ToString(x["AFBackColor"])),
                        PImage = ((x["PImage"] == DBNull.Value) ? "" : Convert.ToString(x["PImage"])),
                        BIOINFO  = ((x["BIOINFO"] == DBNull.Value) ? "" : Convert.ToString(x["BIOINFO"])),
                        FinalDisp  = ((x["FinalDisp"] == DBNull.Value) ? "" : Convert.ToString(x["FinalDisp"])),
                        FinalDispVisible = Model.ckFinalDisp,
                        strRIcon = CheckIcons(((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString())),
                        SpkrRatingVisible = (((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString()) != "0"  && ((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString()) != (clsFeedback.RATING_EXCELLENT).ToString()) ? true : false,
                        imgStarVisible  = (((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString()) != "0"  && ((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString()) == (clsFeedback.RATING_EXCELLENT).ToString()) ? true : false,
                        RedStarVisible  = (((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString()) != "0"  && ((x["SpeakerRating"] == DBNull.Value) ? "0" : x["SpeakerRating"].ToString()) == (clsFeedback.RATING_GOOD).ToString()) ? true : false,
                        SpkrStatusText =  ((x["SpkrStatusText"] == DBNull.Value) ? "" : x["SpkrStatusText"].ToString()),
                        SpeakerStatus_pkey =  ((x["SpeakerStatus_pkey"] == DBNull.Value) ? "" : x["SpeakerStatus_pkey"].ToString()),
                        SpkrStatusBackColor = ((x["SpkrStatusBackColor"] == DBNull.Value) ? "White" : x["SpkrStatusBackColor"].ToString()),
                        SpeakerStatusVisible = (((x["SpeakerStatus_pkey"] == DBNull.Value) ? "0" : x["SpeakerStatus_pkey"].ToString()) == "0") ? false : true,
                        SpkrStatusForeColor = (((x["SpeakerStatus_pkey"] == DBNull.Value) ? "0" : x["SpeakerStatus_pkey"].ToString()) == "0") ? "Black" : "White",
                        AccStatusText =  ((x["AccStatusText"] == DBNull.Value) ? "" : x["AccStatusText"].ToString()),
                        AccStatusBackColor = ((x["AccStatusBackColor"] == DBNull.Value) ? "White" : x["AccStatusBackColor"].ToString()),
                        AccStatusVisible = (((x["AccountStatus_pkey"] == DBNull.Value) ? "0" : x["AccountStatus_pkey"].ToString()) == "0") ? false : true,
                        AccStatusForeColor = (((x["AccountStatus_pkey"] == DBNull.Value) ? "0" : x["AccountStatus_pkey"].ToString()) == "0") ? "Black" : "White",
                        PrioritySpkr = ((x["PrioritySpkr"] == DBNull.Value) ? "" : x["PrioritySpkr"].ToString()),
                        Proposal = ((x["Proposal"] == DBNull.Value) ? "" : x["Proposal"].ToString()),
                        Attended =GetAttended(x, cSettings.strPrimaryEventID, Model.intSpkrCurEventPKey),
                        FBackColor = ((x["FBackColor"] == DBNull.Value) ? "" : Convert.ToString(x["FBackColor"])),
                        PronunicationURL = ((x["PronunicationURL"] == DBNull.Value) ? "" : Convert.ToString(x["PronunicationURL"]) + "?V=" + Guid.NewGuid().ToString()),
                        TypeNamePhonetic = TypeNames(((x["PronunicationURL"] == DBNull.Value) ? "" : Convert.ToString(x["PronunicationURL"])), ((x["NickNames"] == DBNull.Value) ? "" : Convert.ToString(x["NickNames"])), ((x["PhoneticName"] == DBNull.Value) ? "" : Convert.ToString(x["PhoneticName"])), ((x["LastName"] == DBNull.Value) ? "" : Convert.ToString(x["LastName"]))),
                        NameColors = ProcessNameColors(((x["List"] == DBNull.Value) ? "" : Convert.ToString(x["List"])), ((x["SpkrFlag"] == DBNull.Value) ? 0 : Convert.ToInt32(x["SpkrFlag"]))),
                        List = ((x["List"] == DBNull.Value) ? "" : Convert.ToString(x["List"])),
                        NextFollowUpdate = ((x["NextFollowUpdate"] == DBNull.Value) ? "" : Convert.ToString(x["NextFollowUpdate"])),
                        PendingAccountName = GetPendingAccountName(x),
                        PendingAcName = GetPendingAccountName(x, true),
                        ProducerReport = ((x["ProducerReport_Selection"] == DBNull.Value) ? "" : Convert.ToString(x["ProducerReport_Selection"])),
                        HasPosted = ((x["HasPosted"] == DBNull.Value) ? false : Convert.ToBoolean(x["HasPosted"])),
                        SpkrRatingAvailable = (((x["SpkrRating"] == DBNull.Value) ? "" : x["SpkrRating"].ToString()) != "")
                    }).ToList<SpeakerGridView>();

                //    If Me.ckThumbnails.Checked Then
                //        Dim imgPhoto As Web.UI.WebControls.Image = e.Item.FindControl("imgPhoto")
                //        clsUtility.RefreshAccountImage(intAcctPKey, imgPhoto)
                //    End If
                // Dim cmdDisp As LinkButton = DirectCast(gdi.Item("FinalDisp").Controls(0), LinkButton)
                // cmdDisp.Enabled = (intFinalDisposition_pKey <> 1) And strFinalDis.ToLower <> "speaker"

                // Dim intFinalDisposition_pKey As Integer = Val(gdi.GetDataKeyValue("FinalDisposition_pKey").ToString)
                //    Dim SpeakerFlagVal As Integer = Val(IIf(lblSpkrFlag.Text = Nothing Or lblSpkrFlag.Text = "0", "0", lblSpkrFlag.Text))
                //    If(SpeakerFlagVal = 0) Then
                //        lblSpkrFlag.Text = ""
                //    Else
                //        lblSpkrFlag.Text = "[" + d.SelectedItem.Text.ToString + "]" '"[" + Me.ddSpkrFlag.Items.Where(Function(x) x.Value = SpeakerFlagVal.ToString).FirstOrDefault().Text + "]"
                //    End If
                //    Dim NumSpk As Integer = gdi.GetDataKeyValue("NumSpk")
                //    Dim lblAttended As Label = e.Item.FindControl("lblAttended")
                //    Dim listAttended As String = lblAttended.Text.ToString()
                //    If(listAttended <> "") And(NumSpk > 0) And(r <= 0) Then
                //        lblAttended.Text = listAttended.Replace(cLast.strActiveEvent, "<span class='lblSmall' style='color:red;'> " + cSettings.strPrimaryEventID + " </span>")
                //    End If

                //    Dim ReplaaceTest As String = ""
                //    If listAttended<> "" AndAlso NumSpk = 0 Then
                //       ReplaaceTest = listAttended.Replace(cSettings.strPrimaryEventID + ",", "")
                //        ReplaaceTest = ReplaaceTest.Replace(cSettings.strPrimaryEventID, "")
                //    Else
                //        ReplaaceTest = lblAttended.Text.ToString()
                //    End If
                //    lblAttended.Text = ReplaaceTest.Trim(",")
                //    Dim Balance = gdi.GetDataKeyValue("Balance")
                //    Dim d As RadDropDownList = ph.FindControl("dFlag")
                //    Me.BindRedDropDown(d, 1)
                //    Dim S As DropDownList = ph.FindControl("ddSpeakerStatusUpdate")
                //    Me.BindDropdown(S, 2)
                //    Dim chkArticle As CheckBox = ph.FindControl("ckWritingArticle")
                //    chkArticle.Attributes.Add("style", "background-color:white;color:black")
                //    d.SelectedValue = gdi.GetDataKeyValue("SpkrFlag").ToString
                //    S.SelectedValue = gdi.GetDataKeyValue("SpeakerStatus_pkey").ToString

                //    Dim FlagValue = gdi.GetDataKeyValue("SpkrFlag")

                //    Dim count As Integer = 0
                //    lblNickName.Text = IIf(gdi.GetDataKeyValue("NickName").ToString <> "", "(" + gdi.GetDataKeyValue("NickName").ToString + ") ", "")
                //    Dim SpeakerFlagVal As Integer = Val(IIf(lblSpkrFlag.Text = Nothing Or lblSpkrFlag.Text = "0", "0", lblSpkrFlag.Text))
                //    If(SpeakerFlagVal = 0) Then
                //        lblSpkrFlag.Text = ""
                //    Else
                //        lblSpkrFlag.Text = "[" + d.SelectedItem.Text.ToString + "]" '"[" + Me.ddSpkrFlag.Items.Where(Function(x) x.Value = SpeakerFlagVal.ToString).FirstOrDefault().Text + "]"
                //    End If




                #region  DataBound Code 

                // Dim lnkNoteHistory As Web.UI.WebControls.LinkButton = e.Item.FindControl("lnkNoteHistory")
                // lnkNoteHistory.Attributes.Add("onClick", "OnValClick(" + lnkNoteHistory.ClientID.ToString + ");")

                #endregion
            }


            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            string FieldName = "", SpkrMgSortOrder = "", CurrentPageIndex = "";
            if (!string.IsNullOrEmpty(cLast.strSpkrMgSortExpression))
            {
                FieldName = cLast.strSpkrMgSortExpression;
                SpkrMgSortOrder = cLast.intSpkrMgSortOrder.ToString();
                CurrentPageIndex = cLast.intSpkrMgPageIndex.ToString();
            }

            var JsonResult = Json(new
            {
                msg = "OK",
                data = DataList,
                alpha = AlphaList,
                TotalRecords = TotalRecords,
                RowsCount = RowsCount,
                FieldName = FieldName,
                SpkrMgSortOrder = SpkrMgSortOrder,
                CurrentPageIndex = CurrentPageIndex,
            }, JsonRequestBehavior.AllowGet);
            JsonResult.MaxJsonLength=int.MaxValue;
            return JsonResult;
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult SpeakerStatus()
        {
            try
            {
                CommonOperations repos = new CommonOperations();
                DataTable dt = repos.FetchGlobalFilters(4);
                return Json(new { msg = "OK", SpeakerStatusList = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

            }
            return Json(new { msg = "Error Occurred While Fetching Details" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult GetSessionListData(int AccountID, bool ckTime, int SpeakerEvtID, int ddSessStatus)
        {
            try
            {
                SpeakerManagementOperations repos = new SpeakerManagementOperations();
                DataTable dt = repos.DataSessionList(AccountID, ckTime, SpeakerEvtID, ddSessStatus);

                return Json(new { msg = "OK", SessionList = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

            }
            return Json(new { msg = "Error Occurred While Fetching Details" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult GetSessionDetailSpeakers(int intSessionPkey, int intSpkrCurEventPKey, string ddlInterested, int ddAccStatus, int ddSpkrFlag, int ddDateRange, string dtPStart, string dtPEnd)
        {
            try
            {
                SpeakerManagementOperations repos = new SpeakerManagementOperations();
                DataTable dt = repos.getSpeakerDetailsPeopleInformation(intSpkrCurEventPKey, intSessionPkey, ddlInterested, ddAccStatus, ddSpkrFlag, ddDateRange, dtPStart, dtPEnd);
                return Json(new { msg = "OK", SpeakersList = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

            }
            return Json(new { msg = "Error Occurred While Fetching Details" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult EditSpeakerInfo(int id)
        {
            try
            {
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                DataTable dataPriv = new SqlOperation().LoadAccountPrivilages(data.EventId, data.Id);
                bool EditLinkPrivilage = isAllowedByEntity(false, data.GlobalAdmin, clsUtility.TYPE_Account, clsPrivileges.FUNC_EDIT, dataPriv);
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                if (!EditLinkPrivilage)
                {
                    string strWarning = cSettings.getText(clsSettings.Text_SecurityWarning);
                    return Json(new { msg = strWarning, url = "" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { msg = "OK", url = "/ViewAccount?PK=" + id.ToString() }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { msg = "Error occurred while performing the action", url = "/ViewAccount?PK=" + id.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }
        private ContactModel GetContactModelInfo(int SpkrCurEventPKey, int AccountID, int PendingEvtAccPKey)
        {
            ContactModel contactModel = new ContactModel();
            contactModel.intPendingAcctPKey = AccountID;
            contactModel.intPendingEventAcctPKey = PendingEvtAccPKey;
            contactModel.bSpkNotesChanged =false;
            try
            {
                SpeakerManagementOperations operations = new SpeakerManagementOperations();
                DataTable dt = operations.GetSpeakerDataByID(SpkrCurEventPKey, AccountID, PendingEvtAccPKey);
                if (dt != null && dt.Rows.Count >0)
                {
                    DataRow dr = dt.Rows[0];
                    string URLstr = "", strPhoneticIcon = "", FirstName = "", NickName = "", LastName = "", MostRecentNote = "";
                    bool bNickNameAvailable = false, bLastNameAvailable = false, bPhoneticNameAvailable = false, bSpkNotesChanged = false;
                    bNickNameAvailable = ((dr["NickNames"] != System.DBNull.Value) ? (!string.IsNullOrEmpty(dr["NickNames"].ToString())) : false);
                    bPhoneticNameAvailable = ((dr["PhoneticName"] != System.DBNull.Value) ? (!string.IsNullOrEmpty(dr["PhoneticName"].ToString())) : false);
                    bLastNameAvailable = ((dr["LastName"] != System.DBNull.Value) ? (!string.IsNullOrEmpty(dr["LastName"].ToString())) : false);
                    URLstr = ((dr["PronunicationURL"] != System.DBNull.Value) ? dr["PronunicationURL"].ToString() : "");
                    string stringOnClick = $"onclick='playSoundContact(\"{(URLstr + " ? V = " + Guid.NewGuid().ToString())}\")'";
                    if (!string.IsNullOrEmpty(URLstr) && bPhoneticNameAvailable)
                        strPhoneticIcon = $"<img src='/Images/Icons/play.png' {stringOnClick} height='16px' style='margin-Left:10px;vertical-align: middle;' /> ";
                    int FollowUpPKey = ((dr["Followupright_Pkey"] != System.DBNull.Value) ? Convert.ToInt32(dr["Followupright_Pkey"]) : 0);
                    FirstName = ((dr["FirstName"] != System.DBNull.Value) ? dr["FirstName"].ToString() : "");
                    NickName = ((dr["NickName"] != System.DBNull.Value) ? dr["NickName"].ToString() : "");
                    LastName = ((dr["LastName"] != System.DBNull.Value) ? dr["LastName"].ToString() : "");
                    MostRecentNote = ((dr["MostRecentNote"] != System.DBNull.Value) ? dr["MostRecentNote"].ToString() : "");
                    contactModel.strPhoneticIcon = strPhoneticIcon;
                    contactModel.NickName = (!string.IsNullOrEmpty(NickName)) ? NickName : FirstName;
                    contactModel.strPendingAcctName = (FirstName + " " + ((!string.IsNullOrEmpty(NickName)) ? " (" +  NickName +  ") " : "") + LastName);
                    contactModel.Flag = ((dr["SpkrFlag"] != System.DBNull.Value) ? Convert.ToInt32(dr["SpkrFlag"].ToString()) : 0);
                    contactModel.intPhoneType_pkey = ((dr["PhoneType_pKey"] != System.DBNull.Value) ? Convert.ToInt32(dr["PhoneType_pKey"]) : 0);
                    contactModel.intPhoneType2_pkey = ((dr["PhoneType2_pKey"] != System.DBNull.Value) ? Convert.ToInt32(dr["PhoneType2_pKey"]) : 0);
                    contactModel.PSAccount_pkey = ((dr["PSAccount_pkey"] != System.DBNull.Value) ? Convert.ToInt32(dr["PSAccount_pkey"]) : 0);
                    contactModel.PotentialSpeaker = ((dr["PotentialSpeaker"] != System.DBNull.Value) ? Convert.ToBoolean(dr["PotentialSpeaker"]) : false);
                    contactModel.Phone1Ext = ((dr["Phone1Ext"] != System.DBNull.Value) ? dr["Phone1Ext"].ToString() : "");
                    contactModel.Phone2Ext = ((dr["Phone2Ext"] != System.DBNull.Value) ? dr["Phone2Ext"].ToString() : "");
                    contactModel.CCode = ((dr["CCode"] != System.DBNull.Value) ? dr["CCode"].ToString() : "");
                    contactModel.CCode2 = ((dr["CCode2"] != System.DBNull.Value) ? dr["CCode2"].ToString() : "");
                    contactModel.PhoneNum = ((dr["phone"] != System.DBNull.Value) ? dr["phone"].ToString() : "");
                    contactModel.Phone2 = ((dr["phone2"] != System.DBNull.Value) ? dr["phone2"].ToString() : "");
                    contactModel.skypeAddress = ((dr["PhoneCall_1"] != System.DBNull.Value) ? dr["PhoneCall_1"].ToString() : "");
                    contactModel.skypeAddress2 = ((dr["PhoneCall_2"] != System.DBNull.Value) ? dr["PhoneCall_2"].ToString() : "");
                    contactModel.bIsPhone =true;
                    contactModel.bAllowCall =((dr["bAllowCall"] != System.DBNull.Value) ? Convert.ToBoolean(dr["bAllowCall"]) : false);
                    contactModel.MagiContact_pkey = (FollowUpPKey>0) ? FollowUpPKey : 4;
                    contactModel.Flag = ((dr["SpkrFlag"] != System.DBNull.Value) ? Convert.ToInt32(dr["SpkrFlag"]) : 0);
                    contactModel.PermanentNotes = MostRecentNote;
                    contactModel.strMobile = ((dr["PhoneCall_2"] != System.DBNull.Value) ? dr["PhoneCall_2"].ToString() : "");
                    contactModel.PhoneType = ((dr["phone_type"] != System.DBNull.Value) ? dr["phone_type"].ToString() : "");
                    contactModel.PhoneType2 = ((dr["phone_type2"] != System.DBNull.Value) ? dr["phone_type2"].ToString() : "");
                    contactModel.strMobile = ((dr["MobileSMS"] != System.DBNull.Value) ? dr["MobileSMS"].ToString() : "");
                    contactModel.strPendingEMailAddress = ((dr["Email"] != System.DBNull.Value) ? dr["Email"].ToString() : "");
                    contactModel.bPrivate = ((dr["Private2"] != System.DBNull.Value) ? Convert.ToBoolean(dr["Private2"].ToString()) : false);
                }
            }
            catch
            {
            }
            return contactModel;
        }

        [ValidateInput(true)]
        public PartialViewResult _PartialContact(int intSpkrCurEventPKey, int AccountID, int PendingEvtAccPKey, bool bSpkNotesChanged = false, bool chkCallnotesEvent = false, bool chkAnnouncementshow = false)
        {
            ViewBag.ShowModal = true;
            try
            {
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                ContactModel Model = GetContactModelInfo(intSpkrCurEventPKey, AccountID, PendingEvtAccPKey);
                Model.intAccount_pKey = data.Id;
                Model.bSpkNotesChanged=bSpkNotesChanged;
                Model.bshowAnnouncement = chkAnnouncementshow;
                Model.bEventOnly = chkAnnouncementshow;


                SqlConnection sqlConnection = new SqlConnection(ReadConnectionString());
                ViewBag.RedirectHome = false;
                DataTable dt = new SqlOperation().LoadAccountPrivilages(data.EventId, data.Id);
                if (!isAllowedByPriv(false, data.GlobalAdmin, dt, "", "", clsPrivileges.PAGE_SpeakerMgt))
                    ViewBag.RedirectHome = true;
                CommonOperations operation = new CommonOperations();
                SpeakerManagementOperations spkOperations = new SpeakerManagementOperations();
                bool IsEdit = false;
                int FilterType = (IsEdit) ? 2 : 1;
                ViewBag.ddCallNext= ConvertToKendoDropDownList(operation.FetchGlobalFilters(28));
                ViewBag.ddCallResults = ConvertToKendoDropDownList(spkOperations.FetchContactFilters(FilterType, 0));
                ViewBag.rdMagiContact = ConvertToKendoDropDownList(operation.FetchGlobalFilters(9));

                ViewBag.ddphoneType = ConvertToKendoDropDownList(operation.FetchGlobalFilters(18));
                ViewBag.ddAnnouncement =spkOperations.BindAnnouncement("ctlContact.ascx");
                ViewBag.ddActivitySelected = 0;
                int offSet = 0;
                bool boolCheck = false;
                if (Request.Cookies["yjnf"] != null)
                    boolCheck = Int32.TryParse(Request.Cookies["yjnf"].Value, out offSet);

                DateTime dateCheck = ((boolCheck) ? System.DateTime.UtcNow.AddMinutes(-offSet) : System.DateTime.Now);
                ViewBag.ddConDateMaxDate  = dateCheck.Date;
                ViewBag.ddConFolDateMinDate  = dateCheck.Date;
                ViewBag.btnSendSMSshow = "none";
                ViewBag.ContactModel = Model;
                ContactOperations operations = new ContactOperations();
                if (Model.FollowAccount_pkey>0)
                    Model.MagiContact_pkey=  operations.GetFollowUpByAccount(Model.FollowAccount_pkey.ToString());

                string Mobi = "+"+ Model.CCode + Model.PhoneNum.Replace(" (c)", "");
                Model.strMobile = (string.IsNullOrEmpty(Model.strMobile)) ? Mobi : "+" + Model.strMobile;
                Model.intpKey = 0;
                //Model.IntActivitypkey =Model.ActivityPKey;
                ViewBag.tdPossible = (Model.PSAccount_pkey  > 0 ? true : false);
                ViewBag.chkPossible = Model.PotentialSpeaker;
                if (Model.intAccount_pKey<=0)
                    Model.intAccount_pKey = Model.intPendingAcctPKey;

                Model.Event_pkey = data.EventId;
                ViewBag.IsSendEmail = false;
                ViewBag.strMessageId = "";
                ViewBag.lblMsg= "";
                ViewBag.btnCloseVisible = false;
                ViewBag.cmdConCancelVisible = true;
                if (Model.bIsCancel)
                {
                    ViewBag.btnCloseVisible = true;
                    ViewBag.cmdConCancelVisible = false;
                }
                ViewBag.ddCallNextSelectIndex = 0;
                ViewBag.ddCallResultsSelectIndex = 0;
                ViewBag.ddResponcesSelectIndex = 0;
                ViewBag.rdMagiContactSelectIndex = 0;
                ViewBag.dFlagSelectIndex = 0;
                ViewBag.ddPhoneTypeVisible=false;
                if (Model.intPhoneType_pkey >= 0 && Model.PhoneNum != "")
                {
                    ViewBag.ddPhoneTypeVisible =true;
                    ViewBag.ddPhoneTypeSelected= Model.intPhoneType_pkey;
                }
                ViewBag.lbltype = !ViewBag.ddPhoneTypeVisible;
                ViewBag.ddPhoneType2Visible=false;


                ViewBag.rdConViaSelect =0;
                ViewBag.ddCallResultsSelected = 3;
                if (Model.bIsPhone)
                {
                    ViewBag.ddCallResultsSelected = 15;
                    ViewBag.rdConViaSelected = 2;
                    ViewBag.rdConFolViaSelected = 2;
                }
                //ViewBag.trConFolEmailSubjectVisible = (Val(Me.rdConFolVia.SelectedValue) = 1)
                ViewBag.ddActivity =  operations.BindActivity(data.EventId);
                ///----RefreshSpkrLog()
                bool IsSame = (data.EventId== cLast.intActiveEventPkey);
                DataTable dtSpeakerTable = new DataTable();
                DataSet ds = operations.GetSpeakerLog(data.EventId, Model.intPendingEventAcctPKey, AccountID, IsSame, Model.Followup);
                ViewBag.ColumnSMSIDVisible =false;
                ViewBag.ColumnEmailVisible =false;
                ViewBag.trLastFollowDateVisible= false;
                ViewBag.ddConFolDateSpDaysClear =true;
                ViewBag.lblLastFollowDateText = "";
                ViewBag.colorcode= "#25a0da";
                ViewBag.NewDay = "Next Followup Date";
                ViewBag.CalendarDay= Convert.ToDateTime(dateCheck.Date);
                ViewBag.CalendarDaystring= String.Format("{0:d}", Convert.ToDateTime(dateCheck.Date));
                if (ds != null && ds.Tables.Count>0)
                {
                    dtSpeakerTable = ds.Tables[0];
                    DataRow[] drw = dtSpeakerTable.Select("SMSID <>''");
                    ViewBag.ColumnSMSIDVisible = (drw.Count()>0);
                    DataRow[] drMessage = dtSpeakerTable.Select("MessageID <>''");
                    ViewBag.ColumnEmailVisible = (drMessage.Count()>0);
                    if (ds.Tables[2].Rows.Count > 0)
                        ViewBag.strpendingMessageID= (ds.Tables[2].Rows[0][0]!= System.DBNull.Value) ? ds.Tables[2].Rows[0][0].ToString() : "";

                    if (ds.Tables[1].Rows.Count > 0 && ds.Tables[1].Rows[0][0]!= System.DBNull.Value)
                    {
                        ViewBag.trLastFollowDateVisible= true;
                        DateTime date1, date2;
                        date1 = new DateTime();
                        if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0].Rows[0]["FollowupDate"] != System.DBNull.Value)
                            date1 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FollowupDate"]);

                        date2 = dateCheck.Date;
                        ViewBag.lblLastFollowDateText =ds.Tables[1].Rows[0][0];

                        if (DateTime.Compare(date1, date2) == 0)
                        {
                            //                    If(DateTime.Compare(date1, date2) = 0) Then
                            //                        NewDay.Repeatable = RecurringEvents.DayAndMonth
                            //                        NewDay.ItemStyle.BackColor = Color.Yellow
                            //                        NewDay.ItemStyle.BorderColor = Color.White
                            //                    Else
                            //                      
                            //                        NewDay.Repeatable = RecurringEvents.DayAndMonth
                            //                        NewDay.ItemStyle.BackColor = Color.FromArgb(argb)
                            //                        NewDay.ItemStyle.BorderColor = Color.White
                            //                    End If
                        }
                    }
                }
                ///--- Content of Prep Log             //Me.PrepareLog()
                ViewBag.cmdShowmEmergVisible=false;
                ViewBag.lblType2Text = "";
                string strPhone = (Model.PhoneNum != "" ? Model.PhoneNum.Replace("(c)", "") : "");
                ViewBag.lblTypeText = (Model.PhoneType != "" ? " (" + Model.PhoneType.ToLower() + ")" : "");
                ViewBag.lblCallPhoneText = (Model.CCode != "" ? "+" + Model.CCode  + " " : "") + Model.PhoneNum;
                ViewBag.hdfskypeHref = "skype:" + Model.skypeAddress.Trim() + "?call";
                ViewBag.hdfskype2Href = "skype:" + Model.skypeAddress2.Trim() + "?call";
                ViewBag.hdfskypeVisible = (!string.IsNullOrEmpty(Model.PhoneNum));
                ViewBag.hdfskype2Visible = (!string.IsNullOrEmpty(Model.Phone2));
                //string strPhone2 = (Model.Phone2 != "" ? Model.Phone2 : "");
                bool bPhone2 = (!string.IsNullOrEmpty(Model.Phone2));
                string strP = Model.Phone2.Replace("(c)", "");
                ViewBag.tbphone2Visible = true;
                if (strPhone.Trim() == strP.Trim() && (Model.PhoneType.ToUpper() == "MOBILE" ||Model.PhoneType2.ToUpper() == "MOBILE"))
                    ViewBag.lblTypeText = " (mobile)";

                ViewBag.lblCallPhone2Text ="";
                ViewBag.cmdShowmEmergText = "";
                ViewBag.lblEmergencyVisible = false;
                ViewBag.lbljudiciouslyVisible = false;
                ViewBag.lblCallPhone2Visible=false;
                ViewBag.bPhone2Expanded=false;
                if (strP.Trim() != "")
                {
                    ViewBag.lblType2Text =(Model.PhoneType2 != "" ? " (" + Model.PhoneType2.ToLower() + ")" : "");
                    ViewBag.lblCallPhone2Text = (Model.CCode2 != "" ? "+" + Model.CCode2  + " " : "") + (Model.Phone2 != "" ? String.Format("{0:(###)###-####}", strP.ToString()) : "None");
                }
                string lblCallPhone2Text = ViewBag.lblCallPhone2Text;
                ViewBag.lblAlternateVisible = (lblCallPhone2Text.Length>0);
                ViewBag.lblCallPhone2Visible =  (lblCallPhone2Text.Length>0);

                if (bPhone2 && Model.bPrivate)
                {
                    ViewBag.cmdShowmEmergText =  (ViewBag.bPhone2Expanded ? "Hide Alternate Number" : "Show Alternate Number ");
                    ViewBag.cmdShowmEmergVisible=true;
                }


                if (Model.bAllowCall && Model.PhoneType2.ToUpper() == "MOBILE")
                    ViewBag.lbljudiciouslyVisible =true;
                else if (Model.PhoneType2.ToUpper() == "MOBILE")
                    ViewBag.lbljudiciouslyVisible =true;
                else if (Model.PhoneType2.ToUpper() == "HOME" && Model.PhoneType2.ToUpper() == "PAGER")
                    ViewBag.lblEmergencyVisible = true;

                ViewBag.lblmagiContactVisible= !Model.Followup;
                ViewBag.rdMagiContactVisible= !Model.Followup;

                ViewBag.trConEmailSubjectVisible = false;
                ViewBag.trConEmailSubjectVisible = false;
                ViewBag.dvemailVisible=false;
                if (ViewBag.ddCallResultsSelected == 3)
                    ViewBag.dvemailVisible=true;

                //Reset()
                ViewBag.rdConBySelected = "1";
                ViewBag.rdConViaSelected = "2";
                ViewBag.txtConEMailSubjText ="";
                ViewBag.txtConFolEMailSubjText = "Follow Plan";
                ViewBag.txtConFolNotesText = "";
                ViewBag.lblMsgText = "";
                //ViewBag.txtConFolEMailSubjText = "";
                ViewBag.rdProducerOnlySelected = 0;
                ViewBag.ddConFolDateSelectedDate = "";
                ViewBag.dpTimeSelectedDate = "";

                ViewBag.cmdSpkDelVisible = true;
                ViewBag.cmdConSaveVisible = true;
                ViewBag.CmdCommSaveVisible = false;
                ViewBag.dlSpeakerVisible = false;
                ViewBag.tdSpeeakerVisible = false;
                ViewBag.td1Visible = false;
                if (Model.Activity_pkey >0)
                {
                    ViewBag.RereshSpeakers = operations.getSpeakers(data.EventId, data.Id, Model.Activity_pkey);
                    ViewBag.cmdConSaveVisible = false;
                    ViewBag.CmdCommSaveVisible = true;
                    ViewBag.dlSpeakerVisible = true;
                    ViewBag.tdSpeeakerVisible = true;
                }
                ViewBag.ConBy1Text = "Us (" + (data.NickName !="" ? data.NickName : data.FirstName) + ")";
                ViewBag.ConBy2Text = "Them (" + Model.strPendingAcctName + ")";
                ViewBag.lblConTitleText = Model.strPendingAcctName + " Contact" + Model.strPhoneticIcon;
                ViewBag.ddConDateSelected = clsEvent.getCaliforniaTime().ToString("MM/dd/yy");
                SqlConnection con = new SqlConnection(ReadConnectionString());
                clsAccount cAccount = new clsAccount();
                cAccount.intAccount_PKey = data.Id;
                cAccount.sqlConn = con;
                cAccount.LoadAccount();
                string title = "", htmlText = "";
                BindContact(cAccount, data.Id, Model.intContactPkey, Model.NickName, "", ref title, ref htmlText);
                ViewBag.txtReSendEmailContent = htmlText;
                ViewBag.txtSubjectText = title;
                ViewBag.NEXTACTION_NoCall  = clsEventAccount.NEXTACTION_NoCall;
                ViewBag.NEXTACTION_NEVERCONTACT_AGAIN  = clsEventAccount.NEXTACTION_NEVERCONTACT_AGAIN;
                ViewBag.NEXTACTION_NOFOLLOWUP  = clsEventAccount.NEXTACTION_NOFOLLOWUP;


                ///-----Me.RefreshFollowup()
                ViewBag.phFollowupVisible = (ViewBag.ddCallNextSelected != clsEventAccount.NEXTACTION_NoCall &&  ViewBag.ddCallNextSelected != clsEventAccount.NEXTACTION_NEVERCONTACT_AGAIN  &&
                                            ViewBag.ddCallNextSelected != clsEventAccount.NEXTACTION_NOFOLLOWUP);
                ViewBag.txtFlagCommentsText = "";
                ViewBag.dFlagSelectedValue = Model.Flag;
                ViewBag.lblFlagCommentTitleText = "Flag for " +  Model.strPendingAcctName;
                //RdFlagComments -- if not phfollowupvisible
                ///
                ViewBag.cmdClearVisible = false;
                ViewBag.txtPermanent = Model.PermanentNotes;
                ViewBag.rdMagiContactSelectedValue=0;
                if (Model.MagiContact_pkey>0)
                    ViewBag.rdMagiContactSelected =Model.MagiContact_pkey;

                if (bPhone2)
                {
                    if (Model.bPrivate)
                    {
                        ViewBag.cmdShowmEmergText =  (ViewBag.bPhone2Expanded ? "Hide Alternate Number" : "Show Alternate Number ");
                        ViewBag.cmdShowmEmergVisible=true;
                    }
                    else
                        ViewBag.lblCallPhone2Visible=true;
                }
                if (Model.intPhoneType2_pkey>=0 && bPhone2 && ViewBag.tbphone2Visible)
                {
                    ViewBag.ddPhoneType2Visible =true;
                    ViewBag.ddPhoneType2Selected = Model.intPhoneType2_pkey;
                }
                ViewBag.lbltype2 = !ViewBag.ddPhoneType2Visible;
                ViewBag.dFlagSelected = Model.Flag;
                ViewBag.lblFlagCommentTitle = Model.strPendingAcctName;
                List<SpeakerLog> DataList = dtSpeakerTable.AsEnumerable().Select(x => new SpeakerLog()
                {
                    pKey = ((x["pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(x["pKey"])),
                    event_pkey = ((x["event_pkey"] == DBNull.Value) ? 0 : Convert.ToInt32(x["event_pkey"])),
                    Account_pKey = ((x["Account_pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(x["Account_pKey"])),
                    ShortOrder = ((x["ShortOrder"] == DBNull.Value) ? 0 : Convert.ToInt32(x["ShortOrder"])),
                    ChangeColor = ((x["ChangeColor"] == DBNull.Value) ? false : Convert.ToBoolean(x["ChangeColor"])),
                    IsSend = ((x["IsSend"] == DBNull.Value) ? "" : x["IsSend"].ToString()),
                    IsSendVisi = ((x["IsSendVisi"] == DBNull.Value) ? false : Convert.ToBoolean(x["IsSendVisi"])),
                    IsShow = ((x["IsShow"] == DBNull.Value) ? false : Convert.ToBoolean(x["IsShow"])),
                    ContactName = ((x["ContactName"] == DBNull.Value) ? "" : x["ContactName"].ToString()),
                    MessageID = ((x["MessageID"] == DBNull.Value) ? "" : x["MessageID"].ToString()),
                    ContactDate = ((x["ContactDate"] == DBNull.Value) ? "" : x["ContactDate"].ToString()),
                    ContactDateFormat = ((x["ContactDate"] == DBNull.Value) ? "" : ((!string.IsNullOrEmpty(x["ContactDate"].ToString()) ? Convert.ToDateTime(x["ContactDate"]).ToString("d") : ""))),
                    ContactMsg = ((x["ContactMsg"] == DBNull.Value) ? "" : x["ContactMsg"].ToString()),
                    FollowupNotes = ((x["FollowupNotes"] == DBNull.Value) ? "" : x["FollowupNotes"].ToString()),
                    CtBy = ((x["CtBy"] == DBNull.Value) ? "" : x["CtBy"].ToString()),
                    CtVia = ((x["CtVia"] == DBNull.Value) ? "" : x["CtVia"].ToString()),
                    FuBy = ((x["FuBy"] == DBNull.Value) ? "" : x["FuBy"].ToString()),
                    FuVia = ((x["FuVia"] == DBNull.Value) ? "" : x["FuVia"].ToString()),
                    FollowType = ((x["FollowType"] == DBNull.Value) ? "" : x["FollowType"].ToString()),
                    EventID = ((x["EventID"] == DBNull.Value) ? "" : x["EventID"].ToString()),
                    Response = ((x["Response"] == DBNull.Value) ? "" : x["Response"].ToString()),
                    PermanentNotes = ((x["PermanentNotes"] == DBNull.Value) ? "" : x["PermanentNotes"].ToString()),
                    SMSID = ((x["SMSID"] == DBNull.Value) ? "" : x["SMSID"].ToString()),
                    MessageStatus = ((x["MessageStatus"] == DBNull.Value) ? "" : x["MessageStatus"].ToString()),
                    MessageStatusTemplate = ((x["MessageStatus"] == DBNull.Value) ? "" : (x["MessageStatus"].ToString() != "") ? "<a class='CMDEmail lnkEmail' title='" + x["MessageStatus"].ToString() +"' >Email</a>" : ""),
                    FollowupDate = ((x["FollowupDate"] == DBNull.Value) ? "" : x["FollowupDate"].ToString()),
                    EditTemplate = (("<img src='/images/icons/gridgray.png' class='cmdConEdit' data-id='"+((x["pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(x["pKey"]))+"'  title='Edit' style='cursor:pointer;' />"))
                }).ToList<SpeakerLog>();

                //if(dtSpeakerTable.Rows[0]["ContactDate"] != DBNull.Value && dtSpeakerTable.Rows[0]["ContactDate"].ToString() != "")
                //    ViewBag.ddConDateSelected = String.Format("{0:MM/dd/yy}", dtSpeakerTable.Rows[0]["ContactDate"]).ToString();

                ViewBag.SpeakerDataList = DataList;

                //Dim script As String = "function f(){ if (typeof center_Window !== 'undefined') { center_Window() ; } Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
                //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)
                return PartialView("~/Views/Shared/_PartialContact.cshtml", Model);
            }
            catch (Exception ex)
            {
                ViewBag.ShowModal = false;
                return PartialView("~/Views/Shared/_PartialContact.cshtml");
            }
        }
        private void BindContact(clsAccount acc, int EventPKey, int ContactPKey, string strPendingAcctNickName, string ddAnnouncement, ref string title, ref string HtmlText, int incContactPKey = 0, bool notrebind = false)
        {
            int AnnouncmentPKey = 0;
            if (!string.IsNullOrEmpty(ddAnnouncement))
                AnnouncmentPKey = Convert.ToInt32(ddAnnouncement.Trim('_'));
            if (AnnouncmentPKey >0)
            {

                SqlConnection con = new SqlConnection(ReadConnectionString());
                string strHTMLText = "", strTitle = "";
                clsAnnouncement c = new clsAnnouncement();
                c.sqlConn = con;
                c.lblMsg = null;
                c.intAnnouncement_PKey = AnnouncmentPKey;
                c.LoadAnnouncement();
                string strtitle = c.strTitle;
                string strHtmlText = c.strHTMLText;
                string strBody = strHtmlText.Replace("[AcctNickname]", strPendingAcctNickName);
                strBody = strBody.Replace("[UserSignature]", acc.UserSignature(acc, IntContact_pkey: ContactPKey));
                int VenuePKey = 0;
                clsEvent cEvent = new clsEvent();
                clsVenue cVenue = new clsVenue();
                cEvent.sqlConn = con;
                cVenue.sqlConn = con;

                cEvent.intEvent_PKey = EventPKey;
                VenuePKey = cEvent.intVenue_PKey;
                cVenue.intVenue_PKey = VenuePKey;

                cEvent.LoadEvent();
                cVenue.LoadVenue();
                strTitle = cEvent.ReplaceReservedWords(strTitle);
                strTitle = cVenue.ReplaceReservedWords(strTitle);
                strBody = cEvent.ReplaceReservedWords(strBody);
                strBody = cVenue.ReplaceReservedWords(strBody);

                title=strTitle;
                HtmlText= strBody;
            }
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult DeleteSpeakerLog(string Selection, int eaID)
        {

            if (string.IsNullOrEmpty(Selection) || string.IsNullOrWhiteSpace(Selection))
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 100, "Delete speaker log failed as no records were selected ");
                return Json(new { msg = "Select log" }, JsonRequestBehavior.AllowGet);
            }
            int foundKey = 0;
            ContactOperations op = new ContactOperations();
            foreach (string s in Selection.Split(','))
            {
                if (int.TryParse(s, out foundKey))
                    op.DeleteSpeakerContactLog(foundKey);
            }
            op.UpdateSpeakerNextContact(eaID);
            return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult RefreshSpeakerLogs(int eaID, int PendingID, int AcID, bool followup)
        {
            List<SpeakerLog> DataList = new List<SpeakerLog>();
            try
            {
                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                DataTable dtSpeakerTable = new DataTable();
                ContactOperations operations = new ContactOperations();
                if (AcID<=0)
                    AcID= PendingID;
                bool IsSame = (data.EventId== cLast.intActiveEventPkey);
                DataSet ds = operations.GetSpeakerLog(data.EventId, eaID, AcID, IsSame, followup);
                if (ds != null && ds.Tables.Count>0)
                {
                    dtSpeakerTable = ds.Tables[0];
                }
                DataList = dtSpeakerTable.AsEnumerable().Select(x => new SpeakerLog()
                {
                    pKey = ((x["pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(x["pKey"])),
                    event_pkey = ((x["event_pkey"] == DBNull.Value) ? 0 : Convert.ToInt32(x["event_pkey"])),
                    Account_pKey = ((x["Account_pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(x["Account_pKey"])),
                    ShortOrder = ((x["ShortOrder"] == DBNull.Value) ? 0 : Convert.ToInt32(x["ShortOrder"])),
                    ChangeColor = ((x["ChangeColor"] == DBNull.Value) ? false : Convert.ToBoolean(x["ChangeColor"])),
                    IsSend = ((x["IsSend"] == DBNull.Value) ? "" : x["IsSend"].ToString()),
                    IsSendVisi = ((x["IsSendVisi"] == DBNull.Value) ? false : Convert.ToBoolean(x["IsSendVisi"])),
                    IsShow = ((x["IsShow"] == DBNull.Value) ? false : Convert.ToBoolean(x["IsShow"])),
                    ContactName = ((x["ContactName"] == DBNull.Value) ? "" : x["ContactName"].ToString()),
                    MessageID = ((x["MessageID"] == DBNull.Value) ? "" : x["MessageID"].ToString()),
                    ContactDate = ((x["ContactDate"] == DBNull.Value) ? "" : x["ContactDate"].ToString()),
                    ContactDateFormat = ((x["ContactDate"] == DBNull.Value) ? "" : ((!string.IsNullOrEmpty(x["ContactDate"].ToString()) ? Convert.ToDateTime(x["ContactDate"]).ToString("d") : ""))),
                    ContactMsg = ((x["ContactMsg"] == DBNull.Value) ? "" : x["ContactMsg"].ToString()),
                    FollowupNotes = ((x["FollowupNotes"] == DBNull.Value) ? "" : x["FollowupNotes"].ToString()),
                    CtBy = ((x["CtBy"] == DBNull.Value) ? "" : x["CtBy"].ToString()),
                    CtVia = ((x["CtVia"] == DBNull.Value) ? "" : x["CtVia"].ToString()),
                    FuBy = ((x["FuBy"] == DBNull.Value) ? "" : x["FuBy"].ToString()),
                    FuVia = ((x["FuVia"] == DBNull.Value) ? "" : x["FuVia"].ToString()),
                    FollowType = ((x["FollowType"] == DBNull.Value) ? "" : x["FollowType"].ToString()),
                    EventID = ((x["EventID"] == DBNull.Value) ? "" : x["EventID"].ToString()),
                    Response = ((x["Response"] == DBNull.Value) ? "" : x["Response"].ToString()),
                    ResponsID = ((x["ResponsID"] == DBNull.Value) ? "" : x["ResponsID"].ToString()),
                    PermanentNotes = ((x["PermanentNotes"] == DBNull.Value) ? "" : x["PermanentNotes"].ToString()),
                    SMSID = ((x["SMSID"] == DBNull.Value) ? "" : x["SMSID"].ToString()),
                    MessageStatus = ((x["MessageStatus"] == DBNull.Value) ? "" : x["MessageStatus"].ToString()),
                    MessageStatusTemplate = ((x["MessageStatus"] == DBNull.Value) ? "" : (x["MessageStatus"].ToString() != "") ? "<a class='CMDEmail lnkEmail' title='" + x["MessageStatus"].ToString() +"' >Email</a>" : ""),
                    FollowupDate = ((x["FollowupDate"] == DBNull.Value) ? "" : x["FollowupDate"].ToString()),
                    EditTemplate = (("<img src='/images/icons/gridgray.png' class='cmdConEdit' data-id='"+((x["pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(x["pKey"]))+"'  title='Edit' style='cursor:pointer;' />"))
                }).ToList<SpeakerLog>();
            }
            catch
            {

            }
            var JsonResult = Json(new { msg = "OK", data = DataList }, JsonRequestBehavior.AllowGet);
            JsonResult.MaxJsonLength=int.MaxValue;
            return JsonResult;

        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult SaveFlagComments(int SpkrFlagId, int PID, int EID, int ddCallNext, string FlagComments)
        {
            try
            {
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                ContactOperations operations = new ContactOperations();
                if (EID <=0)
                    EID = data.EventId;

                if (operations.SaveFlagComments(PID, EID, data.Id, SpkrFlagId, FlagComments))
                {
                    if (ddCallNext ==7)
                        operations.UpdateSpeakingPermission(PID);
                }
                return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
            }

            catch
            {
                return Json(new { msg = "Error Occurred While Updating Flag" }, JsonRequestBehavior.AllowGet);
            }
        }


        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult SaveFlag2Comments(int SpkrFlagId, int PID, int EID, string FlagComments)
        {
            try
            {
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                ContactOperations operations = new ContactOperations();
                if (EID <=0)
                    EID = data.EventId;

                if (operations.SaveFlagComments(PID, EID, data.Id, SpkrFlagId, FlagComments))
                    return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {

            }
            return Json(new { msg = "Error Occurred While Updating Flag" }, JsonRequestBehavior.AllowGet);
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult ConSaveClick(int eaID, int PendingID, int AcID, int ddResponce, int rdMagiContact, int ddCallResults, int rdConVia, int ddCallNext,
                                       string conby, string CtVia, string ConFolVia, string ddConFolDate, string ConFolBy, string strFolMsg, string strSubject, string txtConFolEMailSubj,
                                       string Followtype, string txtConMsg, string txtPermanent, string ddConDate, string strMessageID,
                                       string strMID, bool FollowUp, bool IsSendEmail, int rpProducerOnly, bool phFollowupVisible, bool IsPossible, int pKey = 0)
        {
            try
            {
                string strAddr = "";
                bool check = false, HasClearDate = false, ModelValid = false, IsFollowupTime = false;  //dpTime.SelectedDate is null or empty false
                int intMode = rdConVia;

                SqlConnection sqlConnection = new SqlConnection(ReadConnectionString());
                clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                DataTable dt = new SqlOperation().LoadAccountPrivilages(data.EventId, data.Id);
                if (!isAllowedByPriv(false, data.GlobalAdmin, dt, "", "", clsPrivileges.PAGE_SpeakerMgt))
                    return Json(new { msg = "Error", redirect = "home" }, JsonRequestBehavior.AllowGet);
                string strEmailBody = "", strMsg = txtConMsg;
                AcID = (AcID<=0) ? PendingID : AcID;
                strEmailBody = (intMode ==3) ? strMsg : strEmailBody;
                ContactModel cModel = GetContactModelInfo(data.EventId, AcID, eaID);
                ContactOperations operations = new ContactOperations();
                SpeakerLogUpdate Model = new SpeakerLogUpdate();
                Model.event_pkey = data.EventId;
                cModel.intAccount_pKey = data.Id;

                if (ddCallResults == 15)
                    return Json(new { msg = "Select result" }, JsonRequestBehavior.AllowGet);
                if (ddConDate == "")
                    return Json(new { msg = " Enter a contact date of today or a previous day." }, JsonRequestBehavior.AllowGet);

                if (ddConFolDate == "")
                {
                    if (ddCallResults!=7 || Followtype!= "" ||txtConMsg.ToUpper().Contains("ASSIGNED") ||txtConMsg.ToUpper().Contains("OFFER") || txtPermanent.ToUpper().Contains("ASSIGNED") ||txtPermanent.ToUpper().Contains("OFFER"))
                        check =false;
                }
                //else
                //    return Json(new { msg = " Enter a followup date of today or a future day." }, JsonRequestBehavior.AllowGet);

                HasClearDate = (ddCallNext ==  clsEventAccount.NEXTACTION_ClearDate && ddCallNext != clsEventAccount.NEXTACTION_NEVERCONTACT_AGAIN && ddCallNext != clsEventAccount.NEXTACTION_NOFOLLOWUP);

                Model.pKey =pKey;
                Model.PendingEventAccountPKey = eaID;
                Model.Account_pKey =data.Id;
                Model.bHasFollowUp =  (ddCallNext != clsEventAccount.NEXTACTION_NoCall);
                Model.ContactDate = ddConDate;
                Model.strMsg= strMsg;
                Model.HasClearDate = HasClearDate;
                Model.IsFollowupTime = IsFollowupTime;
                Model.CtVia = CtVia;
                Model.CtBy = conby;
                Model.FollowupDate =ddConFolDate; //Model.FollowupDate = IIf(ddConFolDate.SelectedDate IsNot Nothing And phFollowup.Visible, dateString, DBNull.Value);
                Model.FuVia = ConFolVia;
                Model.FuBy = ConFolBy;
                Model.FollowupMessage = ((phFollowupVisible) ? strFolMsg : "");
                Model.FallowUpEmailSubject = txtConFolEMailSubj.Replace("<", "").Replace(">", "").Trim();
                Model.CallOutcome_pKey = ddCallResults;
                Model.CallNextAction_pKey = ddCallNext;
                Model.EmailSubject = strSubject;
                Model.strEmailBody = strEmailBody;
                Model.ProducerOnly = phFollowupVisible;
                Model.rpProducerOnly =((phFollowupVisible) ? rpProducerOnly : 0);
                Model.UpdatedforAccount_pkey = PendingID;
                Model.IsSendEmail = IsSendEmail;
                Model.MessageID  = strMessageID;
                Model.ResponseID =ddResponce;
                Model.PermanentNotes= txtPermanent;
                Model.FollowType = Followtype;
                Model.strMID = strMID;
                Model.IsPossible = IsPossible;
                Model.IsPossibleUpdate = (cModel.PSAccount_pkey  > 0 ? true : false);
                Model.bFollowUp = FollowUp;
                Model.HasClearDate = HasClearDate;
                if (operations.AddSpeakerLog(Model, data.Id))
                {
                    if (ddCallNext != clsEventAccount.NEXTACTION_NoCall)
                    {
                        ModelValid = true;
                        switch (intMode)
                        {
                            case 1: strAddr = "turn on AllowEmail"; break;
                            case 2: strAddr = "turn on AllowCall"; break;
                        }
                    }
                    else
                    {
                        switch (intMode)
                        {
                            case 1: strAddr = "turn off AllowEmail"; break;
                            case 2: strAddr = "turn off AllowCall"; break;
                        }
                    }
                    if (strAddr != "")
                    {
                        operations.UpdateAllowCallEmail(PendingID, ModelValid);
                        clsEventAccount cEventAccount = new clsEventAccount();
                        cEventAccount.sqlConn= sqlConnection;
                        cEventAccount.lblMsg =   null;
                        cEventAccount.intEventAccount_pKey = PendingID;
                        cEventAccount.LogAuditMessage("Log " + ((intMode == 2) ? "Call" : "Email") + " to: " + cModel.strPendingAcctName + ((strAddr != "") ? " and " + strAddr : ""));
                    }

                    if (cModel.PermanentNotes != txtPermanent)
                        operations.Note_Insert_Update(cModel.intPendingAcctPKey, data.Id, data.Id, "Insert Permanent Notes", NoteText: txtPermanent);
                    if ((cModel.MagiContact_pkey > 0  || rdMagiContact != cModel.MagiContact_pkey) && FollowUp)
                        operations.UpdateSpeakerNextContact(eaID);
                    if (cModel.intContactPkey>0 && FollowUp)
                        operations.saveSpealerFollowup(txtPermanent, txtConMsg);
                }
                else
                    return Json(new { msg = "Error occurred while updating log" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { msg = "Error occurred while updating log" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
        }

        private bool CheckValidDueDate(string strValue)
        {
            try
            {
                if (Regex.IsMatch(strValue, @"\d") && strValue != "01/01/1900 12:00:00 AM" && strValue != "")
                {
                    DateTime DateValue = new DateTime();
                    bool IsDate = DateTime.TryParse(strValue, out DateValue);
                    return IsDate;
                }
            }
            catch
            {
            }
            return false;
        }
        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult RefreshSpkrLogByID(int pKey, bool IsEdit = false)
        {
            try
            {
                if (pKey >0)
                {
                    ContactOperations operations = new ContactOperations();
                    DataTable dt = operations.GetspeakerLogByID(pKey);
                    if (dt != null && dt.Rows.Count > 0 && IsEdit)
                    {
                        int offSet = 0;
                        bool boolCheck = false;
                        DateTime dateCheck = DateTime.Today;

                        int intVal = 0; bool checkDetails = false;
                        DateTime? SelectedDate = dateCheck;
                        string ddConFolDate = "", ddConDate = "", dpTime = "", txtConFolEMailSubj = "";

                        if (Request.Cookies["yjnf"] != null)
                            boolCheck = Int32.TryParse(Request.Cookies["yjnf"].Value, out offSet);

                        if (boolCheck)
                            dateCheck =  DateTime.UtcNow.AddMinutes(-offSet);
                        DateTime MaxDate = dateCheck.Date, MinDate = dateCheck.Date;
                        bool bNextActionDateNull = dt.Rows[0]["NextActionDate"] == System.DBNull.Value;

                        if (!bNextActionDateNull && Convert.ToDateTime(dt.Rows[0]["NextActionDate"]) >=dateCheck.Date)
                            MinDate =  dateCheck.Date;
                        else
                            MinDate = (!bNextActionDateNull) ? Convert.ToDateTime(dt.Rows[0]["NextActionDate"]) : ((dt.Rows[0]["ContactDate1"] != System.DBNull.Value) ? Convert.ToDateTime(dt.Rows[0]["ContactDate1"]) : dateCheck.Date);

                        if (!bNextActionDateNull && CheckValidDueDate(dt.Rows[0]["NextActionDate"].ToString()))
                        {
                            ddConFolDate =(!bNextActionDateNull) ? (Convert.ToDateTime(dt.Rows[0]["NextActionDate"])).ToString("d") : "";
                            bool SpeakerFollowup = (dt.Rows[0]["SpeakerFollowup"] != System.DBNull.Value) ? Convert.ToBoolean(dt.Rows[0]["SpeakerFollowup"]) : false;
                            if (!bNextActionDateNull && SpeakerFollowup)
                                dpTime = Convert.ToDateTime(dt.Rows[0]["NextActionDate"]).ToString("hh:mm tt");

                            txtConFolEMailSubj = "Follow Plan";
                            intVal = (dt.Rows[0]["Method"] != System.DBNull.Value) ? Convert.ToInt32(dt.Rows[0]["Method"]) : 0;
                            checkDetails = true;
                        }
                        ddConDate = (dt.Rows[0]["ContactDate1"] != System.DBNull.Value) ? (Convert.ToDateTime(dt.Rows[0]["ContactDate1"])).ToString("d") : "";
                        return Json(new
                        {
                            msg = "OK",
                            data = JsonConvert.SerializeObject(dt),
                            dateCheck = dateCheck.ToString("d"),
                            MinDate = MinDate,
                            MaxDate = MaxDate,
                            dpTime = dpTime,
                            checkDetails = checkDetails,
                            intVal = intVal,
                            txtConFolEMailSubj = txtConFolEMailSubj,
                            SelectedDate = ddConFolDate,
                            ddConDate = ddConDate,
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 130, "");
            }
            return Json(new { msg = "Error Occurred While Fetching Result" }, JsonRequestBehavior.AllowGet);
        }
        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public string GetSMSSignature()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("");
            int AccountID = Convert.ToInt32(User.Identity.Name);
            ContactOperations operations = new ContactOperations();
            DataTable dt = operations.SMSSignature(AccountID);
            if (dt!= null && dt.Rows.Count>0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    sb.AppendLine("DO NOT REPLY BY TEXT TO THIS MSG");
                    sb.AppendLine("" +  dr["Firstname"].ToString() + ((dr["Firstname"].ToString().Length==1) ? ". " : " ") + ((dr["MiddleName"].ToString() != "") ? dr["MiddleName"].ToString()  + ((dr["Firstname"].ToString().Length==1) ? ". " : " ") : " ") + ((dr["Lastname"].ToString() != "") ? dr["Lastname"].ToString()  + ((dr["Lastname"].ToString().Length==1) ? ". " : " ") : " ") + (dr["OrganizationID"].ToString() !="" ? ", " + dr["OrganizationID"].ToString() + ((dr["OrganizationID"].ToString().Length==1) ? ". " : " ") : ""));
                }
            }
            return sb.ToString();
        }
        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult SendSMSContact(int eaID, int PendingID, int AcID, string SMS, string Sig, string strMobile)
        {
            try
            {
                if (string.IsNullOrEmpty(SMS.Trim()))
                    return Json(new { msg = "Enter Text Message." }, JsonRequestBehavior.AllowGet);

                string phone = "";
                string strtextSMS = SMS.Trim() + "- " + Sig;
                clsSMS obj = new clsSMS();
                phone  = strMobile.Replace("-", "").Trim();
                string PhoneNumber = phone;
                string strMID = obj.ReturnMessageID(SMS, PhoneNumber, PendingID, Convert.ToInt32(User.Identity.Name));
                return Json(new { msg = "OK", MID = strMID }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 0, ex.Message.Replace("'", "").Replace("The To number", " ").ToString());
                return Json(new { msg = "Error occurred while sending sms" }, JsonRequestBehavior.AllowGet);
            }
        }
        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult UpdateSpeakerstaus(int SpeakerStatus, string Status, int PendingID, string ContactName)
        {
            try
            {

                bool bUpdated = new ContactOperations().UpdateSpeakerspeaker(SpeakerStatus, PendingID);
                if (bUpdated)
                {

                    clsAccount cAccount = new clsAccount();
                    cAccount.intAccount_PKey = Convert.ToInt32(User.Identity.Name);
                    cAccount.sqlConn = new SqlConnection(ReadConnectionString());
                    cAccount.LogAuditMessage("Speaker Status Change  " + ContactName + " as " + Status, clsAudit.LOG_AccSpkStatusChange);

                    return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
            }
            return Json(new { msg = "Error occurred while updating speaker status" }, JsonRequestBehavior.AllowGet);
        }
        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult UpdateReviewStatus(int ID, string name)
        {
            try
            {
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                bool bUpdated = new ContactOperations().MarkReviewed(14, ID, data.EventId);
                if (bUpdated)
                {
                    clsAccount cAccount = new clsAccount();
                    cAccount.intAccount_PKey = Convert.ToInt32(User.Identity.Name);
                    cAccount.sqlConn = new SqlConnection(ReadConnectionString());
                    cAccount.LogAuditMessage("Speaker Status Change  " + name + " as Reviewed", clsAudit.LOG_AccSpkStatusChange);

                    return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 100, "Error Occurred : " + ex.Message);
            }
            return Json(new { msg = "Error occurred while updating speaker status" }, JsonRequestBehavior.AllowGet);

        }
        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult ResetInterested(int ID, string name)
        {
            try
            {
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                bool bUpdated = new ContactOperations().MarkReviewed(0, ID, data.EventId);
                if (bUpdated)
                {
                    clsAccount cAccount = new clsAccount();
                    cAccount.intAccount_PKey = Convert.ToInt32(User.Identity.Name);
                    cAccount.sqlConn = new SqlConnection(ReadConnectionString());
                    cAccount.LogAuditMessage("Speaker Status Change  " + name + " as Interested", clsAudit.LOG_AccSpkStatusChange);
                    return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 100, "Error Occurred : " + ex.Message);
            }
            return Json(new { msg = "Error occurred while updating speaker status" }, JsonRequestBehavior.AllowGet);
        }
        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult lnkhistory(int AccountSession, int EID)
        {
            List<CommentsHistory> DataList = new List<CommentsHistory>();
            try
            {
                ContactOperations operations = new ContactOperations();
                DataTable dt = operations.GetSessionHistory(EID, AccountSession);
                if (dt != null && dt.Rows.Count>0)
                    DataList = dt.AsEnumerable().Select(x => new CommentsHistory()
                    {
                        pKey = ((x["pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(x["pKey"])),
                        Comments = ((x["Comments"] == DBNull.Value) ? "" : x["Comments"].ToString()),
                        AssignmentStatusID = ((x["AssignmentStatusID"] == DBNull.Value) ? "" : x["AssignmentStatusID"].ToString()),
                    }).ToList<CommentsHistory>();
            }
            catch (Exception ex)
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 100, "Error Occurred : " + ex.Message);
            }
            var JsonResult = Json(new { msg = "OK", data = DataList }, JsonRequestBehavior.AllowGet);
            JsonResult.MaxJsonLength=int.MaxValue;
            return JsonResult;
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult CommentDelete(int EventPkey, string CommentpKey, int AccountSession)
        {
            bool bFound = false;
            ContactOperations operations = new ContactOperations();
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            foreach (string s in CommentpKey.Split(','))
            {
                if (operations.CommentDelete(data.EventId, s, AccountSession))
                    bFound=true;
                else
                    break;
            }

            if (!bFound)
                clsUtility.LogErrorMessage(null, null, GetType().Name, 100, "");

            return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult GetSessionListDataByID(int AccountID, int AccountSession, int EventAcc, string NextFollowUpdate, bool ckTime, int SpeakerEvtID, int ddSessStatus)
        {
            try
            {
                SpeakerManagementOperations repos = new SpeakerManagementOperations();
                DataTable dt = repos.DataSessionList(AccountID, ckTime, SpeakerEvtID, ddSessStatus, AccountSession);
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                bool IsNextFollowDate = false, SpkNotPaidforRegi = false, phIsAssigned = false, tdDueDate = false, trrddatetimepicker = false,
                     rddatetimepicker = false;

                bool chkIsLeaderVisible = false, chkIsModuVisible = false, chkIsModuEnabled = false, chkIsLeaderEnabled = false,
                     chkIsLeaderChecked = false, chkIsModuChecked = false, chkIsSlideVisible = false, chkIsSlideChecked = false,
                     chkOfferModVisible = false, chkOfferLeaderVisible = false;

                int intPendingStatusPKey = 0, HaveSlide = 0, intPendingEventAcctPKey = 0;
                string strHomeLession = "", lblval = "", DueDate = "", dpTime = "", NextConDate = "", lblNextConDate = "", txtrddatetimepicker = "",
                       strtext = "Save", strFinish = "";
                decimal balance = 0;
                DateTime? NextFollowDate = null;
                if (dt != null && dt.Rows.Count>0)
                {
                    DataRow dr = dt.Rows[0];
                    intPendingStatusPKey = (dr["AssignmentStatus_pKey"] != System.DBNull.Value) ? Convert.ToInt32(dr["AssignmentStatus_pKey"]) : 0;
                    if (intPendingStatusPKey == 0)
                        intPendingStatusPKey = clsEventSession.ASSIGNSTATUS_Interested;

                    if (dr["AssignmentStatus_pKey"] != System.DBNull.Value)
                        IsNextFollowDate = (NextFollowUpdate != "");

                    if (NextFollowUpdate!="")
                        NextFollowDate = Convert.ToDateTime(NextFollowUpdate);
                    HaveSlide = Convert.ToInt32(dr["IsHaveSlide"]);
                    strHomeLession = dr["homelession"].ToString();
                    intPendingEventAcctPKey =EventAcc;

                    ContactOperations operations = new ContactOperations();
                    operations.GetBalanceSpeaker(data.EventId, AccountID, ref SpkNotPaidforRegi, ref balance);
                    phIsAssigned = (intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Assigned);
                    lblval =  dr["Focus_and_Comments"].ToString();
                    if (dr["DueDate"] != System.DBNull.Value)
                        DueDate =  dr["DueDate"].ToString();

                    clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                    if (intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Launched)
                    {
                        tdDueDate = (HaveSlide==1 || HaveSlide==2 ||HaveSlide==3);
                        chkIsSlideVisible = tdDueDate;
                        chkIsSlideChecked = (HaveSlide == 1 && tdDueDate);
                        trrddatetimepicker =true;
                        if (DueDate=="")
                            DueDate = cLast.strBindDueDateDate;

                        if (dr["NextConDate"] != System.DBNull.Value)
                            lblNextConDate = dr["NextConDate"].ToString();

                        if (cLast.strNextContactDate != "" && Convert.ToDateTime(cLast.strNextContactDate) > DateTime.Now)
                            txtrddatetimepicker = Convert.ToDateTime(cLast.strNextContactDate).ToString("MM/dd/yy");
                        else
                            txtrddatetimepicker = clsUtility.getStartOfDay(DateTime.Now.Date.AddDays(14)).ToString("MM/dd/yy");
                    }

                    if (intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Launched ||
                       intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Assigned ||
                       intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_OfferedNotConfirm ||
                       intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Offered ||
                       intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Cancel)
                    {
                        strtext =  "Proceed with Invitation";
                        strFinish = "Finish without Invitation";
                        if ((intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Launched ||
                                   intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Assigned ||
                                   intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Cancel))
                        {
                            strtext = "Proceed with Message";
                            strFinish = "Finish without Message";
                        }
                    }
                    int NumLeaders = 0, NumMod = 0, TotalLeaderAssigned = 0, TotalModeratorAssigned = 0;
                    bool IsSessionLeader = false, IsSessionModerator = false;
                    IsSessionLeader =  (dr["IsSessionLeader"] != System.DBNull.Value) ? Convert.ToBoolean(dr["IsSessionLeader"]) : false;
                    IsSessionModerator =  (dr["IsSessionModerator"] != System.DBNull.Value) ? Convert.ToBoolean(dr["IsSessionModerator"]) : false;
                    NumLeaders = (dr["NumLeaders"] != System.DBNull.Value) ? Convert.ToInt32(dr["NumLeaders"]) : 0;
                    NumMod = (dr["NumModerators"] != System.DBNull.Value) ? Convert.ToInt32(dr["NumModerators"]) : 0;
                    TotalLeaderAssigned = (dr["TotalLeaderAssigned"] != System.DBNull.Value) ? Convert.ToInt32(dr["TotalLeaderAssigned"]) : 0;
                    TotalModeratorAssigned = (dr["TotalModeratorAssigned"] != System.DBNull.Value) ? Convert.ToInt32(dr["TotalModeratorAssigned"]) : 0;

                    if (intPendingStatusPKey ==  clsEventSession.ASSIGNSTATUS_OfferedNotConfirm || intPendingStatusPKey ==  clsEventSession.ASSIGNSTATUS_Assigned || intPendingStatusPKey ==  clsEventSession.ASSIGNSTATUS_Launched)
                    {
                        chkIsLeaderChecked  = (IsSessionLeader || chkIsLeaderVisible);
                        chkIsModuChecked = (IsSessionModerator || chkIsModuVisible);
                        chkIsLeaderVisible = ((intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Launched || intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Assigned) && NumLeaders>0);
                        chkIsModuVisible = ((intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Launched || intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Assigned) && NumMod>0);

                        if (NumLeaders >0 && TotalLeaderAssigned < NumLeaders)
                            chkIsLeaderEnabled = !IsSessionLeader;

                        if (NumMod >0 && TotalModeratorAssigned < NumMod)
                            chkIsModuEnabled = !IsSessionModerator;
                    }
                    if ((intPendingStatusPKey == 12 || intPendingStatusPKey == 23) && (HaveSlide==1 || HaveSlide==2 ||HaveSlide==3))
                    {
                        chkOfferModVisible = (true && NumMod >0);
                        chkOfferLeaderVisible = (true && NumLeaders >0);
                    }
                    if (!chkIsLeaderEnabled && HaveSlide == 3)
                    {
                        chkIsSlideChecked = false;
                        chkIsSlideVisible =false;
                    }
                }
                return Json(new
                {
                    msg = "OK",
                    intPendingStatusPKey = intPendingStatusPKey,
                    strtext = strtext,
                    strFinish = strFinish,
                    cmdStatusSaveWithoutEmailSave = (strFinish !=""),
                    txtrddatetimepicker = txtrddatetimepicker,
                    DueDate = DueDate,
                    lblNextConDate = lblNextConDate,
                    intPendingEventAcctPKey = intPendingEventAcctPKey,
                    strHomeLesson = strHomeLession,
                    HaveSlide = HaveSlide,
                    lblval = lblval,
                    tdDueDate = tdDueDate,
                    trrddatetimepicker = trrddatetimepicker,
                    rddatetimepicker = rddatetimepicker,
                    dpTime = dpTime,
                    NextConDate = NextConDate,
                    NextFollowDate = NextFollowDate,
                    IsNextFollowDate = IsNextFollowDate,
                    phIsAssigned = phIsAssigned,
                    balance = balance,
                    SpkNotPaidforRegi = SpkNotPaidforRegi,
                    chkIsLeaderVisible = chkIsLeaderVisible,
                    chkIsModuVisible = chkIsModuVisible,
                    chkIsModuEnabled = chkIsModuEnabled,
                    chkIsLeaderEnabled = chkIsLeaderEnabled,
                    chkIsLeaderChecked = chkIsLeaderChecked,
                    chkIsModuChecked = chkIsModuChecked,
                    chkIsSlideVisible = chkIsSlideVisible,
                    chkIsSlideChecked = chkIsSlideChecked,
                    chkOfferModVisible = chkOfferModVisible,
                    chkOfferLeaderVisible = chkOfferLeaderVisible
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

            }
            return Json(new { msg = "Error Occurred While Fetching Details" }, JsonRequestBehavior.AllowGet);
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult StatusSaveClick(SessionSpeakerStatus Model)
        {
            try
            {
                bool isvalid = true; int AccountSessionHistory = 0;
                if (Model.intStatus_pkey == 19 && Model.ddSpeakerContactIndex == 0)
                {
                    isvalid = false;
                    return Json(new { msg = "Select speaker contact." }, JsonRequestBehavior.AllowGet);
                }

                if (Model.intStatus_pkey == clsEventSession.ASSIGNSTATUS_Launched && Model.chkIsSlide && Model.txtDueDateVisible)
                {
                    if (Model.strDueDate == "")
                    {
                        isvalid = false;
                        return Json(new { msg = "Fill due date for slides.", SlidesValidations = true }, JsonRequestBehavior.AllowGet);
                    }
                    else if (!CheckValidDueDate(Model.strDueDate))
                    {
                        isvalid = false;
                        return Json(new { msg = "Fill valid due date for slides i.e. mm/dd format.", SlidesValidations = true }, JsonRequestBehavior.AllowGet);
                    }
                }
                if (Model.intStatus_pkey ==clsEventSession.ASSIGNSTATUS_Launched && (Model.HaveSlide == 1 || Model.HaveSlide == 2 || Model.HaveSlide == 3))
                {
                    if (Model.txtrddatetimepicker == "")
                    {
                        isvalid = false;
                        return Json(new { msg = "Enter follow-up date.", SlidesValidations = true }, JsonRequestBehavior.AllowGet);
                    }
                    else if (CheckValidDueDate(Model.txtrddatetimepicker))
                    {
                        isvalid = false;
                        return Json(new { msg = "Fill valid Next contact date i.e. mm/dd format.", SlidesValidations = true }, JsonRequestBehavior.AllowGet);
                    }
                }

                if (isvalid)
                {
                    string result = changeActivity_Status(Model, out AccountSessionHistory);
                    if (Model.SendEmail)
                    {
                        //If Sendemail And(clsEventSession.ASSIGNSTATUS_Assigned = intStatus_pkey Or clsEventSession.ASSIGNSTATUS_Offered = intStatus_pkey Or clsEventSession.ASSIGNSTATUS_Cancel = intStatus_pkey Or clsEventSession.ASSIGNSTATUS_Launched = intStatus_pkey Or clsEventSession.ASSIGNSTATUS_OfferedNotConfirm = intStatus_pkey) Then
                        //    GetEvent_Info()
                        //    Dim IntContactpkey As Integer = 0
                        //    Dim str_MagiContactName As String = ""
                        //    Dim MagiContact As Boolean = False
                        //    Dim IsUpdateNextFollowDate As Boolean = False
                        //    Dim ISFollowuptime As Boolean = False
                        //    If myVS.intEventStatus_PKey <> 3 Then
                        //        Dim FollowupDate As String = ""
                        //        If txtrddatetimepicker.Visible And Val(Me.ddEditStatus.SelectedValue) = 19 Then
                        //            FollowupDate = IIf(txtrddatetimepicker.SelectedDate.ToString <> "", txtrddatetimepicker.SelectedDate.ToString, "")
                        //            If dpTime.SelectedDate IsNot Nothing Then
                        //                ISFollowuptime = 1
                        //                Dim s As String = ""
                        //                s = String.Format("{0:d}", txtrddatetimepicker.SelectedDate) + " " + String.Format("{0:t}", Me.dpTime.SelectedDate)
                        //                FollowupDate = CDate(s)
                        //            End If
                        //            IsUpdateNextFollowDate = radioNextFollowDate.Checked
                        //            IntContactpkey = myVS.intMagiContact_pkey
                        //            str_MagiContactName = myVS.strMagiContact_Name
                        //        End If
                        //        Me.ctlSendEmail.InitControl(
                        //        myVS.intPendingEvtSesPKey, .
                        //        IIf(intAnnStatus_pkey > 0, intAnnStatus_pkey, 0),
                        //        myVS.intPendingAcctPKey,
                        //        myVS.intPendingEventAcctPKey,
                        //        myVS.intPendingSessionPKey,
                        //        myVS.strPendingSessionName,
                        //        Me.cAccount.intAccount_PKey,
                        //        strDueDate,
                        //        IsSlideCreate:=chkIsSlide.Checked,
                        //        HaveSlide:=myVS.HaveSlide.ToString,
                        //        strNextConDate:=FollowupDate,
                        //        StrMessage:=strComment,
                        //        IntMagicontatc_pkey:=IntContactpkey,
                        //        strMagicontactName:=str_MagiContactName,
                        //        chkMagiContact:=MagiContact,
                        //        IsLeader:=chkOfferLeader.Checked.ToString,
                        //        IMod:=chkOfferMod.Checked.ToString,
                        //        IntCurrentevent_pkey:=myVS.intSpkrCurEventPKey,
                        //        IsUpdateNextFollowDate:=IsUpdateNextFollowDate,
                        //        intSpeakerContact_pkey:=Val(ddSpeakerContact.SelectedValue),
                        //        strSpeakerContact:=ddSpeakerContact.SelectedText,
                        //        ISFollowuptime:=ISFollowuptime)
                        //        clsUtility.PopupRadWindow(ScriptManager.GetCurrent(Me.Page), Me.Page, Me.rwSendEmail)
                        //        Dim script As String = "function f(){Applyconditions(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
                        //        ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)
                        //    End If
                        //End If
                    }
                    ///Code not implemented on QA ---------

                    //bool charge = Model.chkCharge;
                    //bool SpkNotPaidforRegi = false;
                    //decimal balance = 0;
                    //if (Model.chkCharge)
                    //{
                    //    ContactOperations operations = new ContactOperations();
                    //    operations.GetBalanceSpeaker(Model.intSpkrCurEventPKey,Model.intPendingAcctPKey, ref SpkNotPaidforRegi, ref balance);
                    //}

                    ///Code not implemented on QA ---------
                    return Json(new
                    {
                        msg = result,
                        AccountSessionHistory = AccountSessionHistory,
                        isvalid = isvalid,


                        ///Code not implemented on QA ---------
                        //charge= charge,
                        //SpkNotPaidforRegi= SpkNotPaidforRegi,
                        //balance= Math.Abs(balance),
                        ///Code not implemented on QA ---------
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {

            }
            return Json(new { msg = "Error Occurred While Updating Activity" }, JsonRequestBehavior.AllowGet);
        }
        private string changeActivity_Status(SessionSpeakerStatus Model, out int AccountSessionHistory)
        {
            AccountSessionHistory=0;
            int AccountSessionHistory_pkey = 0;
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            SqlConnection conn = new SqlConnection(ReadConnectionString());
            clsAccount cAccount = new clsAccount();
            cAccount.sqlConn= conn;
            cAccount.intAccount_PKey = data.Id;
            SpeakerManagementOperations SMOP = new SpeakerManagementOperations();
            SMOP.SaveHomeLesson(Model.intPendingAcctSesPKey, Model.intSpkrCurEventPKey, Model.intStatus_pkey, Model.strHomeLesson);
            string EmailSessionID = "", launchedmessgae = "", strPendingSessionName = "";
            if (Model.chkRemoveEvent || Model.chkRemoveSlide || (Model.dlOtherSpot != null && Model.dlOtherSpot.Count>0))
            {
                int intCount = 0;
                foreach (dlOtherSpot infoDataOther in Model.dlOtherSpot)
                {
                    if (infoDataOther.chkActivity)
                    {
                        if (string.IsNullOrEmpty(Model.strComment))
                            Model.strComment= "";

                        EmailSessionID = EmailSessionID + " " + infoDataOther.SessionID + ",";
                        SqlCommand sqlCmd1 = new SqlCommand("EventSession_UpdateSesAssignment", conn);
                        sqlCmd1.CommandType = System.Data.CommandType.StoredProcedure;
                        sqlCmd1.CommandTimeout = 30;
                        clsUtility.AddParameter(ref sqlCmd1, "@Account_pkey", SqlDbType.Int, ParameterDirection.Input, Model.intPendingAcctPKey);
                        clsUtility.AddParameter(ref sqlCmd1, "@Event_pkey", SqlDbType.Int, ParameterDirection.Input, Model.intSpkrCurEventPKey);
                        clsUtility.AddParameter(ref sqlCmd1, "@NewStatus_pkey", SqlDbType.Int, ParameterDirection.Input, (Model.intStatus_pkey>0, Model.intStatus_pkey, DBNull.Value));
                        clsUtility.AddParameter(ref sqlCmd1, "@AcctSespkey", SqlDbType.Int, ParameterDirection.Input, infoDataOther.AccountSession_pKey);
                        clsUtility.AddParameter(ref sqlCmd1, "@EvtSesPkey", SqlDbType.Int, ParameterDirection.Input, infoDataOther.EventSession_pkey);
                        clsUtility.AddParameter(ref sqlCmd1, "@UpdatedByAcctPKey", SqlDbType.Int, ParameterDirection.Input, data.Id);
                        clsUtility.AddParameter(ref sqlCmd1, "@Comment", SqlDbType.VarChar, ParameterDirection.Input, Model.strComment, 800);
                        clsUtility.AddParameter(ref sqlCmd1, "@SessionID", SqlDbType.VarChar, ParameterDirection.Input, infoDataOther.SessionID, 50);
                        clsUtility.AddParameter(ref sqlCmd1, "@OrigStatus_pkey", SqlDbType.Int, ParameterDirection.Input, Model.intPendingStatusPKey);
                        clsUtility.AddParameter(ref sqlCmd1, "@IsEditMode", SqlDbType.Int, ParameterDirection.Input, Model.intEditMode);
                        clsUtility.AddParameter(ref sqlCmd1, "@intCommentHpKey", SqlDbType.Int, ParameterDirection.Input, Model.intCommentHpKey);
                        clsUtility.AddParameter(ref sqlCmd1, "@SlideDueDate", SqlDbType.VarChar, ParameterDirection.Input, Model.strDueDate);
                        clsUtility.AddParameter(ref sqlCmd1, "@RemoveEvent", SqlDbType.Bit, ParameterDirection.Input, Model.chkRemoveEvent);
                        clsUtility.AddParameter(ref sqlCmd1, "@RemoveSlide", SqlDbType.Bit, ParameterDirection.Input, Model.chkRemoveSlide);
                        clsUtility.AddParameter(ref sqlCmd1, "@IsSlideCreate", SqlDbType.Bit, ParameterDirection.Input, Model.chkIsSlide);
                        clsUtility.AddParameter(ref sqlCmd1, "@CancelReg", SqlDbType.Bit, ParameterDirection.Input, Model.chkCancelRef);
                        clsUtility.AddParameter(ref sqlCmd1, "@IsLeader", SqlDbType.Bit, ParameterDirection.Input, (Model.chkIsLeader  || Model.chkOfferLeader));
                        clsUtility.AddParameter(ref sqlCmd1, "@IsModu", SqlDbType.Bit, ParameterDirection.Input, (Model.chkIsModu  || Model.chkOfferMod));
                        clsUtility.AddParameter(ref sqlCmd1, "@Error", SqlDbType.VarChar, ParameterDirection.Output, "", 1000);
                        clsUtility.AddParameter(ref sqlCmd1, "@AccountSessionHistory_pkey", SqlDbType.VarChar, ParameterDirection.Output, "", 1000);
                        if (!clsUtility.ExecuteStoredProc(sqlCmd1, null, "Error updating activity assignment"))
                            return "Error updating activity assignment";


                        if (Model.intStatus_pkey == clsEventSession.ASSIGNSTATUS_Launched)
                        {


                        }
                        if (Model.intStatus_pkey == clsEventSession.ASSIGNSTATUS_Launched)
                        {
                            launchedmessgae = "Updated assignments for " + Model.strPendingAcctName + " , Activity " + Model.strPendingSessionName + " to " +  Model.strNewStatus + ", Date for Slides: " +  Model.strDueDate + ", NEXT Follow up date: " + Model.txtrddatetimepicker + ", Speaker contact: " + Model.ddSpeakerContact;
                            cAccount.LogAuditMessage(launchedmessgae, clsAudit.LOG_EventSessionDocumentUpdate);
                        }
                        else
                            cAccount.LogAuditMessage("Updated assignments for " + Model.strPendingAcctName + " , Activity " + Model.strPendingSessionName + " to " + Model.strNewStatus, clsAudit.LOG_EventSessionDocumentUpdate);


                        clsUtility.LogToAudit(conn, null, clsUtility.TYPE_EventSession, Model.intPendingEvtSesPKey, data.Id, "Accept Assignment For " + Model.strPendingAcctName + " , Activity " +Model.strPendingSessionName, intLogType_pKey: clsAudit.LOG_AcceptAssignMent);
                        intCount = intCount + 1;
                    }
                }
                strPendingSessionName = EmailSessionID.Trim(',');
                if (intCount<=0)
                    return "refresh";
            }
            else
            {
                if (Model.dlOtherSession!=null && Model.dlOtherSession.Count>0)
                {
                    foreach (dlOtherSpot infoData in Model.dlOtherSession)
                    {
                        if (infoData.chkActivity)
                        {
                            EmailSessionID = EmailSessionID + " " + infoData.SessionID + ",";
                            SqlCommand sqlCmd1 = new SqlCommand("EventSession_UpdateSesAssignment", conn);
                            sqlCmd1.CommandType = System.Data.CommandType.StoredProcedure;
                            sqlCmd1.CommandTimeout = 30;
                            clsUtility.AddParameter(ref sqlCmd1, "@Account_pkey", SqlDbType.Int, ParameterDirection.Input, Model.intPendingAcctPKey);
                            clsUtility.AddParameter(ref sqlCmd1, "@Event_pkey", SqlDbType.Int, ParameterDirection.Input, Model.intSpkrCurEventPKey);
                            clsUtility.AddParameter(ref sqlCmd1, "@NewStatus_pkey", SqlDbType.Int, ParameterDirection.Input, clsEventSession.ASSIGNSTATUS_OtherSession);
                            clsUtility.AddParameter(ref sqlCmd1, "@AcctSespkey", SqlDbType.Int, ParameterDirection.Input, infoData.AccountSession_pKey);
                            clsUtility.AddParameter(ref sqlCmd1, "@EvtSesPkey", SqlDbType.Int, ParameterDirection.Input, infoData.EventSession_pkey);
                            clsUtility.AddParameter(ref sqlCmd1, "@UpdatedByAcctPKey", SqlDbType.Int, ParameterDirection.Input, data.Id);
                            clsUtility.AddParameter(ref sqlCmd1, "@Comment", SqlDbType.VarChar, ParameterDirection.Input, infoData.CommentsUpdate, 800);
                            clsUtility.AddParameter(ref sqlCmd1, "@SessionID", SqlDbType.VarChar, ParameterDirection.Input, infoData.SessionID, 50);
                            clsUtility.AddParameter(ref sqlCmd1, "@OrigStatus_pkey", SqlDbType.Int, ParameterDirection.Input, Model.intPendingStatusPKey);
                            clsUtility.AddParameter(ref sqlCmd1, "@IsEditMode", SqlDbType.Int, ParameterDirection.Input, Model.intEditMode);
                            clsUtility.AddParameter(ref sqlCmd1, "@intCommentHpKey", SqlDbType.Int, ParameterDirection.Input, Model.intCommentHpKey);
                            clsUtility.AddParameter(ref sqlCmd1, "@SlideDueDate", SqlDbType.VarChar, ParameterDirection.Input, Model.strDueDate);
                            clsUtility.AddParameter(ref sqlCmd1, "@RemoveEvent", SqlDbType.Bit, ParameterDirection.Input, Model.chkRemoveEvent);
                            clsUtility.AddParameter(ref sqlCmd1, "@RemoveSlide", SqlDbType.Bit, ParameterDirection.Input, Model.chkRemoveSlide);
                            clsUtility.AddParameter(ref sqlCmd1, "@IsSlideCreate", SqlDbType.Bit, ParameterDirection.Input, Model.chkIsSlide);
                            clsUtility.AddParameter(ref sqlCmd1, "@CancelReg", SqlDbType.Bit, ParameterDirection.Input, Model.chkCancelRef);
                            clsUtility.AddParameter(ref sqlCmd1, "@IsLeader", SqlDbType.Bit, ParameterDirection.Input, (Model.chkIsLeader  || Model.chkOfferLeader));
                            clsUtility.AddParameter(ref sqlCmd1, "@IsModu", SqlDbType.Bit, ParameterDirection.Input, (Model.chkIsModu  || Model.chkOfferMod));
                            clsUtility.AddParameter(ref sqlCmd1, "@Error", SqlDbType.VarChar, ParameterDirection.Output, "", 1000);
                            clsUtility.AddParameter(ref sqlCmd1, "@AccountSessionHistory_pkey", SqlDbType.VarChar, ParameterDirection.Output, "", 1000);
                            if (!clsUtility.ExecuteStoredProc(sqlCmd1, null, "Error updating activity assignment"))
                                return "Error updating activity assignment";
                        }
                    }
                }

                SqlCommand sqlCmd = new SqlCommand("EventSession_UpdateSesAssignment", conn);
                sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCmd.CommandTimeout = 30;
                clsUtility.AddParameter(ref sqlCmd, "@Account_pkey", SqlDbType.Int, ParameterDirection.Input, Model.intPendingAcctPKey);
                clsUtility.AddParameter(ref sqlCmd, "@Event_pkey", SqlDbType.Int, ParameterDirection.Input, Model.intSpkrCurEventPKey);
                clsUtility.AddParameter(ref sqlCmd, "@NewStatus_pkey", SqlDbType.Int, ParameterDirection.Input, Model.intStatus_pkey);
                clsUtility.AddParameter(ref sqlCmd, "@AcctSespkey", SqlDbType.Int, ParameterDirection.Input, Model.intPendingAcctSesPKey);
                clsUtility.AddParameter(ref sqlCmd, "@EvtSesPkey", SqlDbType.Int, ParameterDirection.Input, Model.intPendingEvtSesPKey);
                clsUtility.AddParameter(ref sqlCmd, "@UpdatedByAcctPKey", SqlDbType.Int, ParameterDirection.Input, data.Id);
                clsUtility.AddParameter(ref sqlCmd, "@Comment", SqlDbType.VarChar, ParameterDirection.Input, Model.strComment, 800);
                clsUtility.AddParameter(ref sqlCmd, "@SessionID", SqlDbType.VarChar, ParameterDirection.Input, Model.strPendingSessionName, 50);
                clsUtility.AddParameter(ref sqlCmd, "@OrigStatus_pkey", SqlDbType.Int, ParameterDirection.Input, Model.intPendingStatusPKey);
                clsUtility.AddParameter(ref sqlCmd, "@IsEditMode", SqlDbType.Int, ParameterDirection.Input, Model.intEditMode);
                clsUtility.AddParameter(ref sqlCmd, "@intCommentHpKey", SqlDbType.Int, ParameterDirection.Input, Model.intCommentHpKey);
                clsUtility.AddParameter(ref sqlCmd, "@SlideDueDate", SqlDbType.VarChar, ParameterDirection.Input, Model.strDueDate);
                clsUtility.AddParameter(ref sqlCmd, "@RemoveEvent", SqlDbType.Bit, ParameterDirection.Input, Model.chkRemoveEvent);
                clsUtility.AddParameter(ref sqlCmd, "@RemoveSlide", SqlDbType.Bit, ParameterDirection.Input, Model.chkRemoveSlide);
                clsUtility.AddParameter(ref sqlCmd, "@IsSlideCreate", SqlDbType.Bit, ParameterDirection.Input, Model.chkIsSlide);
                clsUtility.AddParameter(ref sqlCmd, "@CancelReg", SqlDbType.Bit, ParameterDirection.Input, Model.chkCancelRef);
                clsUtility.AddParameter(ref sqlCmd, "@IsLeader", SqlDbType.Bit, ParameterDirection.Input, (Model.chkIsLeader  || Model.chkOfferLeader));
                clsUtility.AddParameter(ref sqlCmd, "@IsModu", SqlDbType.Bit, ParameterDirection.Input, (Model.chkIsModu  || Model.chkOfferMod));
                clsUtility.AddParameter(ref sqlCmd, "@Error", SqlDbType.VarChar, ParameterDirection.Output, "", 1000);
                clsUtility.AddParameter(ref sqlCmd, "@AccountSessionHistory_pkey", SqlDbType.VarChar, ParameterDirection.Output, "", 1000);
                if (!clsUtility.ExecuteStoredProc(sqlCmd, null, "Error updating activity assignment"))
                    return "Error updating activity assignment";

                AccountSessionHistory_pkey = Convert.ToInt32(sqlCmd.Parameters["@AccountSessionHistory_pkey"].Value.ToString());

                if (Model.intStatus_pkey == clsEventSession.ASSIGNSTATUS_Launched)
                {
                    launchedmessgae = "Updated assignments for " + Model.strPendingAcctName + " , Activity " + Model.strPendingSessionName + " to " +  Model.strNewStatus + ", Date for Slides: " +  Model.strDueDate + ", NEXT Follow up date: " + Model.txtrddatetimepicker + ", Speaker contact: " + Model.ddSpeakerContact;
                    cAccount.LogAuditMessage(launchedmessgae, clsAudit.LOG_EventSessionDocumentUpdate);
                }
                else
                    cAccount.LogAuditMessage("Updated assignments for " + Model.strPendingAcctName + " , Activity " + Model.strPendingSessionName + " to " + Model.strNewStatus, clsAudit.LOG_EventSessionDocumentUpdate);


                clsUtility.LogToAudit(conn, null, clsUtility.TYPE_EventSession, Model.intPendingEvtSesPKey, data.Id, "Accept Assignment For " + Model.strPendingAcctName + " , Activity " +Model.strPendingSessionName, intLogType_pKey: clsAudit.LOG_AcceptAssignMent);
            }

            if (Model.intStatus_pkey == clsEventSession.ASSIGNSTATUS_Launched)
                Save_Communication_Log(Model, AccountSessionHistory_pkey: AccountSessionHistory_pkey, FollowType: Model.ddEditStatusSelected);


            if (SMOP.UpdateSpeaker_Focus(Model.intPendingAcctPKey, Model.intPendingEvtSesPKey, Model.strComment) != "OK")
                return "Error updating focus";

            bool NextCondates = false, DueDateDates = false;
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            if (Model.intPendingStatusPKey == 19)
            {
                cLast.strNextContactDate = Convert.ToDateTime(Model.txtrddatetimepicker).ToString("MM/dd/yy");
                NextCondates = true;
                if (Model.txtDueDateVisible && Model.chkIsSlide)
                {
                    cLast.strBindDueDateDate = Convert.ToDateTime(Model.strDueDate).ToString("MM/dd/yy");
                    DueDateDates =true;
                }
                SMOP.Update_Both_Dates(data.Id, NextCondates, DueDateDates);
                SMOP.UpdateSpeakerContact(0, Model.ddSpeakerContact, Model.intPendingAcctPKey, Model.intSpkrCurEventPKey, data.Id, Model.ddSpeakerContactText);
            }
            bool bChngSpkrEqlzr = false;
            if (!((Model.intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Launched && Model.intStatus_pkey == clsEventSession.ASSIGNSTATUS_Assigned)
              ||(Model.intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Assigned && Model.intStatus_pkey == clsEventSession.ASSIGNSTATUS_Launched)))
                bChngSpkrEqlzr = true;

            if (bChngSpkrEqlzr && Model.intPendingStatusPKey != Model.intStatus_pkey &&  (Model.intStatus_pkey ==  clsEventSession.ASSIGNSTATUS_Assigned
                                                                                      || Model.intStatus_pkey ==  clsEventSession.ASSIGNSTATUS_Cancel || Model.intStatus_pkey == clsEventSession.ASSIGNSTATUS_Launched
                                                                                      || Model.intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Assigned || Model.intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Launched))

                SMOP.UpdateSpeakerEqualizer(Model.Session_pKey, data.EventId);

            //RefreshComments()
            //RefreshGrid(True)
            AccountSessionHistory =AccountSessionHistory_pkey;
            return "OK";
        }
        private void Save_Communication_Log(SessionSpeakerStatus Model, int AccountSessionHistory_pkey = 0, string FollowType = "")
        {
            string s = "";
            DateTime dtStartTime = DateTime.Now;

            SqlConnection conn = new SqlConnection(ReadConnectionString());
            SqlCommand sqlCmd = new SqlCommand("EventAccount_LogContact_MVC", conn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCmd.CommandTimeout = 30;

            if (string.IsNullOrEmpty(Model.strComment))
                Model.strComment = "";

            clsUtility.AddParameter(ref sqlCmd, "@pKey", SqlDbType.Int, ParameterDirection.Input, 0);
            clsUtility.AddParameter(ref sqlCmd, "@EventAcct_pkey", SqlDbType.Int, ParameterDirection.Input, Model.intPendingEventAcctPKey);
            clsUtility.AddParameter(ref sqlCmd, "@UpdatedByAcctPKey", SqlDbType.Int, ParameterDirection.Input, User.Identity.Name);
            clsUtility.AddParameter(ref sqlCmd, "@HasFollowup", SqlDbType.Bit, ParameterDirection.Input, clsEventAccount.NEXTACTION_NoCall);
            clsUtility.AddParameter(ref sqlCmd, "@ConDate", SqlDbType.DateTime, ParameterDirection.Input, DateTime.Now);
            clsUtility.AddParameter(ref sqlCmd, "@By", SqlDbType.Int, ParameterDirection.Input, 1);
            clsUtility.AddParameter(ref sqlCmd, "@Via", SqlDbType.Int, ParameterDirection.Input, 8);
            clsUtility.AddParameter(ref sqlCmd, "@Message", SqlDbType.VarChar, ParameterDirection.Input, Model.strComment, 300);
            clsUtility.AddParameter(ref sqlCmd, "@HasClearDate", SqlDbType.Bit, ParameterDirection.Input, 0);

            if (Model.txtrddatetimepicker != null && Model.txtrddatetimepickerVisible)
            {
                if (string.IsNullOrEmpty(Model.dpTime))
                    dtStartTime = Convert.ToDateTime(Model.txtrddatetimepicker);
                else
                    s = String.Format("{0:d}", Model.txtrddatetimepicker) + " " + String.Format("{0:t}", Model.dpTime);
            }
            if (Model.dpTime != null && Model.txtrddatetimepickerVisible)
            {
                clsUtility.AddParameter(ref sqlCmd, "@FollowupDate", SqlDbType.DateTime, ParameterDirection.Input, dtStartTime);
                clsUtility.AddParameter(ref sqlCmd, "@IsFollowupTime", SqlDbType.Bit, ParameterDirection.Input, 1);
            }
            else
                clsUtility.AddParameter(ref sqlCmd, "@FollowupDate", SqlDbType.DateTime, ParameterDirection.Input, Model.txtrddatetimepicker);

            clsUtility.AddParameter(ref sqlCmd, "@FollowupBy", SqlDbType.Int, ParameterDirection.Input, 1);
            clsUtility.AddParameter(ref sqlCmd, "@FollowupVia", SqlDbType.Int, ParameterDirection.Input, 1);
            clsUtility.AddParameter(ref sqlCmd, "@FollowupMessage", SqlDbType.VarChar, ParameterDirection.Input, "", 300);
            clsUtility.AddParameter(ref sqlCmd, "@CallOutcome_pKey", SqlDbType.Int, ParameterDirection.Input, 10);
            clsUtility.AddParameter(ref sqlCmd, "@CallNextAction_pKey", SqlDbType.Int, ParameterDirection.Input, 1);
            clsUtility.AddParameter(ref sqlCmd, "@EmailSubject", SqlDbType.VarChar, ParameterDirection.Input, "", 300);
            clsUtility.AddParameter(ref sqlCmd, "@FallowUpEmailSubject", SqlDbType.VarChar, ParameterDirection.Input, "", 300);
            clsUtility.AddParameter(ref sqlCmd, "@EmailBody", SqlDbType.VarChar, ParameterDirection.Input, "", 300);
            clsUtility.AddParameter(ref sqlCmd, "@ProducerOnly", SqlDbType.Bit, ParameterDirection.Input, 0);
            clsUtility.AddParameter(ref sqlCmd, "@UpdatedforAccount_pkey", SqlDbType.Int, ParameterDirection.Input, Model.intPendingAcctPKey);
            clsUtility.AddParameter(ref sqlCmd, "@AccountSessionHistory_pkey", SqlDbType.Int, ParameterDirection.Input, AccountSessionHistory_pkey);

            if (FollowType!="")
                clsUtility.AddParameter(ref sqlCmd, "@FollowupType", SqlDbType.VarChar, ParameterDirection.Input, FollowType);

            clsUtility.AddParameter(ref sqlCmd, "@event_pkey", SqlDbType.Int, ParameterDirection.Input, Model.intSpkrCurEventPKey);
            clsUtility.AddParameter(ref sqlCmd, "@Error", SqlDbType.VarChar, ParameterDirection.Output, "", 1000);

            clsUtility.ExecuteStoredProc(sqlCmd, null, "Error logging speaker contact");
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult CallstatusFun(int ddEditStatus, int PendingSession, int HaveSlide, int AccountID, int AccountSession, bool tdDueDateVisible, bool ckTime, int SpeakerEvtID, int ddSessStatus)
        {
            try
            {

                bool RemoveOnClickSave = false, cmdStatusSaveConfirm = false, open = false, chkIsLeaderVisible = false,
                     chkIsModuVisible = false, chkIsModuEnabled = false, chkIsLeaderEnabled = false, chkIsLeaderChecked = false,
                     chkIsModuChecked = false, chkOfferModVisible = false, chkOfferLeaderVisible = false,
                     cmdYesVisible = false, chkIsSlideVisible = false, chkIsSlideChecked = false, lblSpeakerPaid = false,
                     chkRemoveSlideVisible = false, chkRemoveSlideChecked = false, chkRemoveEventChecked = false, chkCancelRefChecked = false,
                     cmdStatusSaveWithoutEmailSaveVisible = false, rwWindowOpen = false, trrddatetimepickerVisible = false,
                     chkChargeVisible = false, SpkNotPaidforRegi = false, chkChargeChecked = false, chkMagiContactChecked = false, chkRemoveEventVisible = false, rddatetimepickerVisible = false;

                decimal balance = 0;
                int valid = 0, ddSpeakerContactSelected = 0;
                DateTime? NextFollowDate = null;
                string strtext = "Save", strFinish = "", cmdStatusSaveText = "", cmdFinishText = "", cmdStatusSaveWithoutEmailSaveText = "",
                       strStatusCountwiseList = "", txtDueDate = "", dpTime = "";
                int intPendingStatusPKey = 0;
                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                SpeakerManagementOperations SMOP = new SpeakerManagementOperations();
                DataTable dt = SMOP.DataSessionList(AccountID, ckTime, SpeakerEvtID, ddSessStatus, AccountSession);
                DataTable dtOtherSpot = new DataTable();
                DataTable dlOtherSessionSpot = new DataTable();
                ContactOperations operations = new ContactOperations();
                operations.GetBalanceSpeaker(AccountID, data.EventId, ref SpkNotPaidforRegi, ref balance);
                if (dt != null && dt.Rows.Count>0)
                {
                    DataRow dr = dt.Rows[0];
                    intPendingStatusPKey = (dr["AssignmentStatus_pKey"] != System.DBNull.Value) ? Convert.ToInt32(dr["AssignmentStatus_pKey"]) : 0;
                    int NumLeaders = 0, NumMod = 0, TotalLeaderAssigned = 0, TotalModeratorAssigned = 0;
                    bool IsSessionLeader = false, IsSessionModerator = false;
                    IsSessionLeader =  (dr["IsSessionLeader"] != System.DBNull.Value) ? Convert.ToBoolean(dr["IsSessionLeader"]) : false;
                    IsSessionModerator =  (dr["IsSessionModerator"] != System.DBNull.Value) ? Convert.ToBoolean(dr["IsSessionModerator"]) : false;
                    NumLeaders = (dr["NumLeaders"] != System.DBNull.Value) ? Convert.ToInt32(dr["NumLeaders"]) : 0;
                    NumMod = (dr["NumModerators"] != System.DBNull.Value) ? Convert.ToInt32(dr["NumModerators"]) : 0;
                    TotalLeaderAssigned = (dr["TotalLeaderAssigned"] != System.DBNull.Value) ? Convert.ToInt32(dr["TotalLeaderAssigned"]) : 0;
                    TotalModeratorAssigned = (dr["TotalModeratorAssigned"] != System.DBNull.Value) ? Convert.ToInt32(dr["TotalModeratorAssigned"]) : 0;

                    if (ddEditStatus ==  clsEventSession.ASSIGNSTATUS_OfferedNotConfirm || ddEditStatus ==  clsEventSession.ASSIGNSTATUS_Assigned || ddEditStatus ==  clsEventSession.ASSIGNSTATUS_Launched)
                    {
                        chkIsLeaderChecked  = (IsSessionLeader || chkIsLeaderVisible);
                        chkIsModuChecked = (IsSessionModerator || chkIsModuVisible);
                        chkIsLeaderVisible = ((ddEditStatus == clsEventSession.ASSIGNSTATUS_Launched || ddEditStatus == clsEventSession.ASSIGNSTATUS_Assigned) && NumLeaders>0);
                        chkIsModuVisible = ((ddEditStatus == clsEventSession.ASSIGNSTATUS_Launched || ddEditStatus == clsEventSession.ASSIGNSTATUS_Assigned) && NumMod>0);

                        if (NumLeaders >0 && TotalLeaderAssigned < NumLeaders)
                            chkIsLeaderEnabled = !IsSessionLeader;

                        if (NumMod >0 && TotalModeratorAssigned < NumMod)
                            chkIsModuEnabled = !IsSessionModerator;
                    }
                    if ((ddEditStatus == 12 || ddEditStatus == 23) && (HaveSlide==1 || HaveSlide==2 ||HaveSlide==3))
                    {
                        chkOfferModVisible = (true && NumMod >0);
                        chkOfferLeaderVisible = (true && NumLeaders >0);
                    }
                    if (!chkIsLeaderEnabled && HaveSlide == 3)
                    {
                        chkIsSlideChecked = false;
                        chkIsSlideVisible =false;
                    }
                }

                if (ddEditStatus == clsEventSession.ASSIGNSTATUS_Launched || ddEditStatus == clsEventSession.ASSIGNSTATUS_Assigned || ddEditStatus ==  clsEventSession.ASSIGNSTATUS_Cancel ||
                   ddEditStatus == clsEventSession.ASSIGNSTATUS_OfferedNotConfirm || ddEditStatus == clsEventSession.ASSIGNSTATUS_Offered)
                {
                    strtext = (ddEditStatus == clsEventSession.ASSIGNSTATUS_Launched || ddEditStatus ==  clsEventSession.ASSIGNSTATUS_Assigned || ddEditStatus == clsEventSession.ASSIGNSTATUS_Cancel) ? "Proceed with Message" : "Proceed with Invitation";
                    cmdStatusSaveText =strtext;
                    cmdStatusSaveWithoutEmailSaveVisible =true;

                    strFinish = (ddEditStatus == clsEventSession.ASSIGNSTATUS_Launched || ddEditStatus ==  clsEventSession.ASSIGNSTATUS_Assigned || ddEditStatus == clsEventSession.ASSIGNSTATUS_Cancel) ? "Finish without Message" : "Finish without Invitation";
                    cmdFinishText = strFinish;
                    cmdStatusSaveWithoutEmailSaveText =strFinish;
                    cmdYesVisible =true;
                }
                else
                {
                    cmdStatusSaveText ="Save";
                    cmdStatusSaveWithoutEmailSaveVisible =false;
                    cmdFinishText = "Save";
                    cmdYesVisible =false;
                }

                if (ddEditStatus == clsEventSession.ASSIGNSTATUS_Launched && (HaveSlide ==1 || HaveSlide ==2 || HaveSlide==3))
                {
                    tdDueDateVisible =(HaveSlide ==1 || HaveSlide ==2 || HaveSlide==3);
                    if (cLast.strBindDueDateDate != "none")
                        txtDueDate = Convert.ToDateTime(cLast.strBindDueDateDate).ToString("MM/dd/yy");
                    else
                        txtDueDate = "none";
                    trrddatetimepickerVisible = (HaveSlide ==1 || HaveSlide ==2 || HaveSlide==3);
                }
                else if (ddEditStatus != clsEventSession.ASSIGNSTATUS_OfferedNotConfirm &&
                    (intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Launched || intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Assigned ||  intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Assign)
                    && (HaveSlide ==1 || HaveSlide ==2 || HaveSlide==3))
                {
                    chkRemoveEventVisible =true;
                    chkRemoveEventChecked = (intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Launched);
                    if (intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Launched && (HaveSlide ==1 || HaveSlide==3))
                    {
                        chkRemoveSlideVisible=false;
                        chkRemoveSlideChecked = false;
                        open =true;
                    }
                    if (intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Assigned)
                    {
                        chkRemoveEventVisible =true;
                        open = true;
                    }

                    if (SpkNotPaidforRegi && balance>0)
                    {
                        chkChargeVisible =true;
                        chkChargeChecked=false;
                        open = true;
                    }
                    else if (SpkNotPaidforRegi && balance == 0)
                        lblSpeakerPaid =true;
                    else
                    {
                        lblSpeakerPaid =false;
                        chkRemoveSlideChecked = false;
                        chkRemoveSlideVisible = (intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Launched);
                    }

                    if (intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Assigned && ddEditStatus == clsEventSession.ASSIGNSTATUS_Cancel && (HaveSlide ==1 || HaveSlide==3))
                    {
                        chkRemoveEventVisible =true;
                        chkRemoveEventChecked = true;
                        chkRemoveSlideChecked = false;
                        chkRemoveSlideVisible = false;
                        open = true;
                    }
                }
                else
                {
                    tdDueDateVisible = false;
                    rddatetimepickerVisible = false;
                    trrddatetimepickerVisible= false;
                    dpTime = "";
                }
                if (intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Launched && ddEditStatus != clsEventSession.ASSIGNSTATUS_Launched && ddEditStatus != clsEventSession.ASSIGNSTATUS_OfferedNotConfirm && ddEditStatus != clsEventSession.ASSIGNSTATUS_Assigned)
                {
                    chkRemoveSlideChecked = false;
                    chkRemoveSlideVisible =(HaveSlide == 1 || HaveSlide == 3);
                    chkRemoveEventVisible =true;
                    chkRemoveEventChecked =true;
                    open = true;
                }
                else if (intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Launched && ddEditStatus != clsEventSession.ASSIGNSTATUS_Launched && (ddEditStatus ==  clsEventSession.ASSIGNSTATUS_Assigned || ddEditStatus ==  clsEventSession.ASSIGNSTATUS_OfferedNotConfirm) && HaveSlide ==1)
                {
                    chkRemoveSlideChecked = true;
                    chkRemoveSlideVisible = true;
                    open =true;
                }

                if (open)
                {
                    dtOtherSpot = SMOP.GetAllOtherSession_Spot(data.EventId, data.Id, Session_pkey: PendingSession);
                    if (dtOtherSpot != null && dtOtherSpot.Rows.Count> 0)
                        rwWindowOpen =true;
                    else
                    {
                        chkRemoveSlideChecked = false;
                        chkRemoveEventChecked = false;
                        chkCancelRefChecked = false;
                        chkChargeChecked = false;
                    }
                }

                if (intPendingStatusPKey != clsEventSession.ASSIGNSTATUS_Launched && intPendingStatusPKey != clsEventSession.ASSIGNSTATUS_Assign &&
                    intPendingStatusPKey != clsEventSession.ASSIGNSTATUS_Assigned && intPendingStatusPKey != clsEventSession.ASSIGNSTATUS_Offer)
                {
                    if (intPendingStatusPKey == clsEventSession.ASSIGNSTATUS_Launched && ddEditStatus !=  clsEventSession.ASSIGNSTATUS_Cancel && (HaveSlide ==1 || HaveSlide ==2 || HaveSlide==3))
                    {
                        dtOtherSpot = SMOP.GetAllOtherSession_Spot(data.EventId, AccountID, Session_pkey: PendingSession);
                        if (dtOtherSpot != null && dtOtherSpot.Rows.Count>0)
                        {
                            chkRemoveSlideVisible = true;
                            chkRemoveSlideChecked = false;
                            rwWindowOpen =true;
                        }
                    }
                }
                chkIsSlideVisible=tdDueDateVisible;
                chkIsSlideChecked = (HaveSlide ==1 || HaveSlide==3 && tdDueDateVisible) && tdDueDateVisible;

                if (intPendingStatusPKey != clsEventSession.ASSIGNSTATUS_Launched && intPendingStatusPKey != clsEventSession.ASSIGNSTATUS_Assign &&
                   intPendingStatusPKey != clsEventSession.ASSIGNSTATUS_Assigned && intPendingStatusPKey != clsEventSession.ASSIGNSTATUS_Offer &&
                   intPendingStatusPKey != clsEventSession.ASSIGNSTATUS_Offered &&  intPendingStatusPKey != clsEventSession.ASSIGNSTATUS_OfferedNotConfirm &&
                   intPendingStatusPKey != clsEventSession.ASSIGNSTATUS_OfferPossible
                  )
                {
                    if (valid ==1 && (ddEditStatus == clsEventSession.ASSIGNSTATUS_Assign || ddEditStatus == clsEventSession.ASSIGNSTATUS_Offer ||
                        ddEditStatus == clsEventSession.ASSIGNSTATUS_Assigned || ddEditStatus == clsEventSession.ASSIGNSTATUS_Launched ||
                        ddEditStatus == clsEventSession.ASSIGNSTATUS_Offered || ddEditStatus == clsEventSession.ASSIGNSTATUS_OfferedNotConfirm ||
                        ddEditStatus == clsEventSession.ASSIGNSTATUS_OfferPossible))
                    {
                        System.Text.StringBuilder sb = new System.Text.StringBuilder("");
                        int IntSpeakerNeeded = 0;
                        if (IntSpeakerNeeded>0)
                            sb.Append("" + IntSpeakerNeeded.ToString());
                        else
                            sb.Append("This activity already has enough speakers. Continue? ");

                        sb.Append("" + strStatusCountwiseList);
                        string str = sb.ToString();
                        cmdStatusSaveConfirm =true;
                    }
                }
                else
                    RemoveOnClickSave = true;


                if ((intPendingStatusPKey != clsEventSession.ASSIGNSTATUS_Launched && intPendingStatusPKey != clsEventSession.ASSIGNSTATUS_Assign &&
                     intPendingStatusPKey != clsEventSession.ASSIGNSTATUS_Assigned && intPendingStatusPKey != clsEventSession.ASSIGNSTATUS_Offer) &&  (
                     ddEditStatus == clsEventSession.ASSIGNSTATUS_Launched ||  ddEditStatus == clsEventSession.ASSIGNSTATUS_Assign || ddEditStatus == clsEventSession.ASSIGNSTATUS_Assigned ||
                     ddEditStatus == clsEventSession.ASSIGNSTATUS_Offer || ddEditStatus == clsEventSession.ASSIGNSTATUS_OfferedNotConfirm || ddEditStatus == clsEventSession.ASSIGNSTATUS_Offered))
                    dtOtherSpot = SMOP.GetAllOtherSession_Spot(data.EventId, AccountID, Session_pkey: PendingSession);

                if (chkIsSlideChecked)
                    tdDueDateVisible= true;


                if ((intPendingStatusPKey != clsEventSession.ASSIGNSTATUS_Launched && intPendingStatusPKey != clsEventSession.ASSIGNSTATUS_Assign &&
                    intPendingStatusPKey !=  clsEventSession.ASSIGNSTATUS_Assigned && intPendingStatusPKey != clsEventSession.ASSIGNSTATUS_Offer)
                    &&
                    (ddEditStatus == clsEventSession.ASSIGNSTATUS_Launched || ddEditStatus == clsEventSession.ASSIGNSTATUS_Assign ||
                     ddEditStatus == clsEventSession.ASSIGNSTATUS_Assigned || ddEditStatus == clsEventSession.ASSIGNSTATUS_Offer ||
                     ddEditStatus == clsEventSession.ASSIGNSTATUS_OfferedNotConfirm || ddEditStatus == clsEventSession.ASSIGNSTATUS_Offered))
                    dlOtherSessionSpot =  SMOP.GetAllOtherSession_Spot(data.EventId, AccountID, Qtype: "Possible", Session_pkey: PendingSession);

                bool showPossible = (dlOtherSessionSpot != null  && dlOtherSessionSpot.Rows.Count>0);

                var jsonResult = Json(new
                {
                    msg = "OK",
                    RemoveOnClickSave = RemoveOnClickSave,
                    cmdStatusSaveConfirm = cmdStatusSaveConfirm,
                    chkIsLeaderVisible = chkIsLeaderVisible,
                    chkIsModuVisible = chkIsModuVisible,
                    chkIsModuEnabled = chkIsModuEnabled,
                    chkIsLeaderEnabled = chkIsLeaderEnabled,
                    chkIsLeaderChecked = chkIsLeaderChecked,
                    chkIsModuChecked = chkIsModuChecked,
                    chkOfferModVisible = chkOfferModVisible,
                    chkOfferLeaderVisible = chkOfferLeaderVisible,
                    cmdStatusSaveText = cmdStatusSaveText,
                    chkRemoveEventChecked = chkRemoveEventChecked,
                    cmdStatusSaveWithoutEmailSaveText = cmdStatusSaveWithoutEmailSaveText,
                    cmdStatusSaveWithoutEmailSaveVisible = cmdStatusSaveWithoutEmailSaveVisible,
                    cmdYesVisible = cmdYesVisible,
                    cmdFinishText = cmdFinishText,
                    lblSpeakerPaid = lblSpeakerPaid,
                    chkCancelRefChecked = chkCancelRefChecked,
                    strStatusCountwiseList = strStatusCountwiseList,
                    trrddatetimepickerVisible = trrddatetimepickerVisible,
                    chkChargeVisible = chkChargeVisible,
                    SpkNotPaidforRegi = SpkNotPaidforRegi,
                    chkChargeChecked = chkChargeChecked,
                    chkMagiContactChecked = chkMagiContactChecked,
                    chkRemoveEventVisible = chkRemoveEventVisible,
                    rddatetimepickerVisible = rddatetimepickerVisible,
                    balance = balance,
                    dpTime = dpTime,
                    txtDueDate = txtDueDate,
                    ddSpeakerContactSelected = ddSpeakerContactSelected,
                    open = open,
                    valid = valid,
                    chkIsSlideVisible = chkIsSlideVisible,
                    chkIsSlideChecked = chkIsSlideChecked,
                    chkRemoveSlideVisible = chkRemoveSlideVisible,
                    chkRemoveSlideChecked = chkRemoveSlideChecked,
                    rwWindowOpen = rwWindowOpen,
                    tdDueDateVisible = tdDueDateVisible,
                    dtOtherSpot = JsonConvert.SerializeObject(dtOtherSpot),
                    dlOtherSessionSpot = JsonConvert.SerializeObject(dlOtherSessionSpot),
                    showPossible = showPossible
                }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength= int.MaxValue;
                return jsonResult;
            }
            catch
            {
                return Json(new { msg = "Error Occurred while fetching details" }, JsonRequestBehavior.AllowGet);
            }
        }
        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult GetHistory(int PendingAcct, bool chkFlaghistory)
        {
            try
            {
                SpeakerManagementOperations SMOP = new SpeakerManagementOperations();
                int offset = 0;
                if (Request.Cookies["yjnf"] != null)
                    int.TryParse(Request.Cookies["yjnf"].Value.ToString(), out offset);

                DataTable dt = SMOP.RefreshHistory(offset, clsUtility.TYPE_Account, PendingAcct, GetType().Name, chkFlaghistory);
                List<SpeakerLogHistory> LogHistory = new List<SpeakerLogHistory>();
                if (dt!= null && dt.Rows.Count>0)
                {
                    LogHistory  = dt.AsEnumerable().Select(x => new SpeakerLogHistory()
                    {
                        pKey = ((x["pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(x["pKey"])),
                        UpdatedOn = ((x["UpdatedOn"] == DBNull.Value) ? "" : string.Format("{0:g}", Convert.ToDateTime(x["UpdatedOn"]))),
                        Change = ((x["Change"] == DBNull.Value) ? "" : x["Change"].ToString()),
                        UpdatedBy = ((x["UpdatedBy"] == DBNull.Value) ? "" : x["UpdatedBy"].ToString()),
                    }).ToList<SpeakerLogHistory>();
                }
                var JSonResult = Json(new { msg = "OK", data = LogHistory }, JsonRequestBehavior.AllowGet);
                JSonResult.MaxJsonLength = int.MaxValue;
                return JSonResult;
            }
            catch
            {
                return Json(new { msg = "Error Occurred While Fetching History" }, JsonRequestBehavior.AllowGet);
            }
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult GetFlagHistory(int PendingAcct)
        {
            try
            {
                SpeakerManagementOperations SMOP = new SpeakerManagementOperations();
                DataTable dt = SMOP.RefreshFlagHistory(PendingAcct);
                List<SpeakerFlagHistory> LogHistory = new List<SpeakerFlagHistory>();
                if (dt!= null && dt.Rows.Count>0)
                {
                    LogHistory  = dt.AsEnumerable().Select(x => new SpeakerFlagHistory()
                    {
                        pKey = ((x["pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(x["pKey"])),
                        EventID= ((x["EventID"] == DBNull.Value) ? "" : x["EventID"].ToString()),
                        SpkrFlagID = ((x["SpkrFlagID"] == DBNull.Value) ? "" : x["SpkrFlagID"].ToString()),
                        Comments = ((x["Comments"] == DBNull.Value) ? "" : x["Comments"].ToString()),
                    }).ToList<SpeakerFlagHistory>();
                }
                var JSonResult = Json(new { msg = "OK", data = LogHistory }, JsonRequestBehavior.AllowGet);
                JSonResult.MaxJsonLength = int.MaxValue;
                return JSonResult;
            }
            catch
            {
                return Json(new { msg = "Error Occurred While Fetching Flag History" }, JsonRequestBehavior.AllowGet);
            }
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult cmdPrReportClick(int PendingAcct, string Review)
        {
            try
            {
                SpeakerManagementOperations SMOP = new SpeakerManagementOperations();
                if (!string.IsNullOrEmpty(Review))
                    Review= "<" +Review.Replace(",", "><")+ ">";
                if (SMOP.UpdateProducerReport(PendingAcct, Review))
                    return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
            }
            return Json(new { msg = "Error Occurred While Updating Selections" }, JsonRequestBehavior.AllowGet);
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public string SpeakerFeedback(int PendingAcct)
        {
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            cLast.intFeedbackAcct = PendingAcct;
            Session["cLastUsed"] =cLast;
            return "OK";
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult ToggleFeatured(int PendingAcct, bool PrioritySpkr)
        {
            try
            {
                SpeakerManagementOperations SMOP = new SpeakerManagementOperations();
                if (SMOP.ToggleFeatured(PendingAcct, PrioritySpkr))
                    return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
            }
            return Json(new { msg = "Error Occurred While Toggling Featured" }, JsonRequestBehavior.AllowGet);
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult NotesHistoryUpdate(int PendingAcct, int EventID)
        {
            try
            {
                clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                SpeakerManagementOperations SMOP = new SpeakerManagementOperations();
                DataTable dt = SMOP.RefreshNotes(PendingAcct, EventID, cLast.IsshowCommentEventFree);
                string PreviousEdit = SMOP.BindPreviousNotes(PendingAcct, EventID);
                List<SpeakerNotes> NotesList = new List<SpeakerNotes>();
                if (dt != null && dt.Rows.Count>0)
                {
                    NotesList  = dt.AsEnumerable().Select(x => new SpeakerNotes()
                    {
                        pKey = ((x["pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(x["pKey"])),
                        NoteText = ((x["NoteText"] == DBNull.Value) ? "" : x["NoteText"].ToString()),
                        NoteBy = ((x["NoteBy"] == DBNull.Value) ? "" : x["NoteBy"].ToString()),
                        NoteDate = ((x["NoteDate"] == DBNull.Value) ? "" : x["NoteDate"].ToString()),
                        UpdateDate = ((x["UpdateDate"] == DBNull.Value) ? "" : x["UpdateDate"].ToString()),
                        UpdateBy = ((x["UpdateBy"] == DBNull.Value) ? "" : x["UpdateBy"].ToString()),
                        lblNoteText =((x["NoteText"] == DBNull.Value) ? "" : x["NoteText"].ToString()),
                        lblNoteDate = "Created by " +((x["NoteBy"] == DBNull.Value) ? "" : x["NoteBy"].ToString()) + " on " +  ((x["NoteDate"] == DBNull.Value) ? "" : x["NoteDate"].ToString()) +
                        (((x["UpdateDate"] == DBNull.Value) ? "" : x["UpdateDate"].ToString())!="" ? ". Updated by " + ((x["UpdateBy"] == DBNull.Value) ? "" : x["UpdateBy"].ToString()) + " on " +  ((x["UpdateDate"] == DBNull.Value) ? "" : x["UpdateDate"].ToString()) : ""),
                    }).ToList<SpeakerNotes>();
                }

                var JsonResult = Json(new { msg = "OK", data = NotesList, PreviousEdit = PreviousEdit }, JsonRequestBehavior.AllowGet);
                JsonResult.MaxJsonLength = int.MaxValue;
                return JsonResult;
            }
            catch
            {
                return Json(new { msg = "Error Occurred While Updating" }, JsonRequestBehavior.AllowGet);
            }
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult SaveNotes(int PendingAcct, int EventID, int PendingNotePKey = 0, string NoteText = "")
        {
            try
            {
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                SpeakerManagementOperations SMOP = new SpeakerManagementOperations();
                string Qtype = "InsertUpdateNotes", MSG = "Update Notes";
                if (PendingNotePKey>0)
                {
                    MSG = "Update Notes";
                    if (NoteText == "")
                        Qtype="DeleteNotes";
                }
                SMOP.Note_Insert_Update(PendingAcct, EventID, data.Id, MSG, Qtype: Qtype, intPendingNotePkey: PendingNotePKey, NoteText: NoteText);
                return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { msg = "Error Occurred" }, JsonRequestBehavior.AllowGet);
            }
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult DeleteSelectedNotes(int PendingAcct, string pKeys)
        {
            try
            {
                int PendingNotePKey = 0;
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                SpeakerManagementOperations SMOP = new SpeakerManagementOperations();
                foreach (string s in pKeys.Split(','))
                {
                    int.TryParse(s, out PendingNotePKey);
                    SMOP.Note_Insert_Update(PendingAcct, data.EventId, data.Id, "Update Notes", PendingNotePKey, "DeleteNotes");
                }
                return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { msg = "Error Occurred" }, JsonRequestBehavior.AllowGet);
            }
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult ImgExpressClick(int PendingAcct)
        {
            try
            {
                SpeakerManagementOperations SMOP = new SpeakerManagementOperations();
                DataTable dt = SMOP.RefreshExpressList(PendingAcct);
                List<EventList> ListEvents = new List<EventList>();
                if (dt!= null && dt.Rows.Count>0)
                {
                    ListEvents  = dt.AsEnumerable().Select(x => new EventList()
                    {
                        pKey = ((x["pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(x["pKey"])),
                        EventID= ((x["EventID"] == DBNull.Value) ? "" : x["EventID"].ToString()),
                        bChecked= ((x["bChecked"] == DBNull.Value) ? false : Convert.ToBoolean(x["bChecked"])),
                    }).ToList<EventList>();
                }
                var JSonResult = Json(new { msg = "OK", data = ListEvents }, JsonRequestBehavior.AllowGet);
                JSonResult.MaxJsonLength = int.MaxValue;
                return JSonResult;
            }
            catch
            {
                return Json(new { msg = "Error Occurred" }, JsonRequestBehavior.AllowGet);
            }
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult ImgAddInterestClick(int PendingAcct, int EventID)
        {
            try
            {
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                SpeakerManagementOperations SMOP = new SpeakerManagementOperations();
                DataTable dt = SMOP.ActivityList(PendingAcct, data.EventId);

                var JSonResult = Json(new { msg = "OK", data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
                JSonResult.MaxJsonLength = int.MaxValue;
                return JSonResult;
            }
            catch
            {
                return Json(new { msg = "Error Occurred" }, JsonRequestBehavior.AllowGet);
            }
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult cmdExpressSaveClick(int PendingID, string strPendingAcctName, bool ckPrioritySpkr, string ckExpressList)
        {
            try
            {
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                SpeakerManagementOperations SMOP = new SpeakerManagementOperations();
                foreach (string s in ckExpressList.Split(','))
                    SMOP.UpdateExpressionList(PendingID, s, (data.Id != PendingID && data.StaffMember));

                SMOP.DeleteExpressionList(PendingID, ckExpressList);
                SMOP.UpdateAccountPrioritySpeaker(PendingID, ckPrioritySpkr);

                SqlConnection conn = new SqlConnection(ReadConnectionString());
                clsAccount cAccount = new clsAccount();
                cAccount.sqlConn= conn;
                cAccount.intAccount_PKey = data.Id;
                cAccount.LogAuditMessage("Updated speaker expression profile for: " + strPendingAcctName, clsAudit.LOG_SpeakerAccountUpdate);
                return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
            }
            return Json(new { msg = "Error Occurred While Updating Express List" }, JsonRequestBehavior.AllowGet);
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult FollowRightSave(int PendingID, int EventID, int ddFollowUpAccount, string strFollowUp)
        {
            try
            {
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                SpeakerManagementOperations SMOP = new SpeakerManagementOperations();
                if (EventID != data.EventId)
                    data.EventId = EventID;
                SMOP.FollowUpUpdate(PendingID, data.EventId, data.Id, strFollowUp, ddFollowUpAccount);
                return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { msg = "Error Occurred while Update" }, JsonRequestBehavior.AllowGet);
            }


        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult GetProfileBio(int PendingID)
        {
            try
            {
                SpeakerManagementOperations SMOP = new SpeakerManagementOperations();
                DataTable dt = SMOP.GetProfileBio(PendingID);
                if (dt!= null && dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    string PersonalBioview = (dr["PersonalBio"] != System.DBNull.Value) ? dr["PersonalBio"].ToString() : "";
                    string Aboutview = (dr["AboutMe"] != System.DBNull.Value) ? dr["AboutMe"].ToString() : "";
                    string ProfileComment = (dr["Comment"] != System.DBNull.Value) ? dr["Comment"].ToString() : "";
                    string lblBios = (dr["PersonalBio"] != System.DBNull.Value) ? dr["PersonalBio"].ToString() : "";
                    bool hlCVEnabled = (dr["CVFilename"] != System.DBNull.Value) ? (!string.IsNullOrEmpty(dr["CVFilename"].ToString())) : false;
                    string hlcvNavigateUrl = "~/accountCVs/" + ((dr["CVFilename"] != System.DBNull.Value) ? dr["CVFilename"].ToString() : "");
                    return Json(new
                    {
                        msg = "OK",
                        hlCV = "CV",
                        PersonalBioview = PersonalBioview,
                        Aboutview = Aboutview,
                        ProfileComment = ProfileComment,
                        lblBios = lblBios,
                        hlCVEnabled = hlCVEnabled,
                        hlcvNavigateUrl = hlcvNavigateUrl
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
            }
            return Json(new { msg = "Error Occurred While Fetching Bio Details" }, JsonRequestBehavior.AllowGet);
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult SaveBioInfo(int PendingID, string PersonalBioview, string Aboutview, string ProfileComment)
        {
            try
            {
                SpeakerManagementOperations SMOP = new SpeakerManagementOperations();
                if (SMOP.SaveBioUpdate(PendingID, PersonalBioview, Aboutview, ProfileComment))
                    return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {

            }
            return Json(new { msg = "Error Occurred While Updating Bio" }, JsonRequestBehavior.AllowGet);
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult cmdExpressSave1Click(int PendingID, int EventID, int ddSecActivity, string ddSecActivityText, string pendingName)
        {
            try
            {
                string URLImagePriority = "";
                SpeakerManagementOperations SMOP = new SpeakerManagementOperations();
                User_Login data = new User_Login();
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                if (ddSecActivity <=0)
                    return Json(new { msg = "Activity not found" }, JsonRequestBehavior.AllowGet);

                if (SMOP.AddActivityToInterest(data.EventId, ddSecActivity, PendingID, data.Id, data.StaffMember))
                {
                    SqlConnection conn = new SqlConnection(ReadConnectionString());
                    clsAccount cAccount = new clsAccount();
                    cAccount.sqlConn =conn;
                    cAccount.intAccount_PKey = data.Id;
                    cAccount.LogAuditMessage("Updated speaker interest for " + ddSecActivityText + " for: " + pendingName, clsAudit.LOG_SpeakerAccountUpdate);
                    DataTable dt1 = SMOP.GetSessionPriorities(PendingID, data.EventId);
                    if (dt1 != null && dt1.Rows.Count>0 && dt1.Rows[0]["str"] != System.DBNull.Value)
                        URLImagePriority = dt1.Rows[0]["str"].ToString();

                    return Json(new { msg = "OK", imgPriority = URLImagePriority }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
            }
            return Json(new { msg = "Error Occurred While Updating Refresh" }, JsonRequestBehavior.AllowGet);
        }

        private void getAnnouncementPkey(int intStatus_pkey, string strSessionName)
        {
            switch (intStatus_pkey)
            {
                case clsEventSession.ASSIGNSTATUS_Assigned:
                    ViewBag.intAnnouncement_PKey = clsAnnouncement.SESSION_Assigned;
                    ViewBag.strAnnouncement = "Assigned";
                    break;
                case clsEventSession.ASSIGNSTATUS_Cancel:
                    ViewBag.intAnnouncement_PKey = clsAnnouncement.SESSION_Dropped;
                    ViewBag.strAnnouncement = "Cancel";
                    break;
                case clsEventSession.ASSIGNSTATUS_Launched:
                    ViewBag.intAnnouncement_PKey = clsAnnouncement.SESSION_Launched;
                    ViewBag.strAnnouncement = "Launched";
                    break;
                case clsEventSession.ASSIGNSTATUS_Offered:
                    ViewBag.intAnnouncement_PKey = clsAnnouncement.SESSION_Offered;
                    ViewBag.strAnnouncement = "Offered - Needs Confirmation";
                    break;
                case clsEventSession.ASSIGNSTATUS_OfferedNotConfirm:
                    ViewBag.intAnnouncement_PKey = clsAnnouncement.SESSION_OfferedNotConf;
                    ViewBag.strAnnouncement = "Offered - Does Not Need Confirmation";
                    break;
            }

            ViewBag.Link = "EditAnnouncement?PK=" + ViewBag.intAnnouncement_PKey.ToString() + "&IsTab=1";
            ViewBag.Target ="_blank";
            ViewBag.strStatus = ((strSessionName != "") ? "Activity: " +strSessionName : "") + " Announcement: " + ViewBag.strAnnouncement;

        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public PartialViewResult _PartialSendEmailToSpeaker(SessionSpeakerStatus Model)
        {
            SqlConnection conn = new SqlConnection(ReadConnectionString());
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            User_Login data = new User_Login();

            CommonOperations cop = new CommonOperations();
            SpeakerManagementOperations smop = new SpeakerManagementOperations();
            ViewBag.dScopeS =  cop.FetchGlobalFiltersByEvent(data.EventId, 6);
            ViewBag.cbAccountStaff = smop.GetcbAccountStaff(data.EventId, Model.intPendingEvtSesPKey, Model.intPendingAcctPKey, true);
            //ShowHide(True)
            //myVS.intSpeakerContact_pkey = intSpeakerContact_pkey
            //myVS.strSpeakerContact = strSpeakerContact
            //myVS.chkMagiContact = chkMagiContact
            //myVS.IsUpdateNextFollowDate = IsUpdateNextFollowDate
            //myVS.IsNotClose = isNotClose
            //myVS.Callreplace = Callreplace
            //myVS.IntMagiContact_pkey = IntMagicontatc_pkey
            //myVS.intPendingEvtSesPKey = Val(EvtSesPKey)
            //myVS.intCurEventPKey = IIf(IntCurrentevent_pkey > 0, IntCurrentevent_pkey, Me.cLast.intActiveEventPkey)
            //myVS.intStatus_pkey = Val(StatusPkey)
            //hdfAnnouncementPkey.Value = Val(StatusPkey)
            //myVS.intPendingAcctPKey = Val(PendingAcctPKey)
            //myVS.intPendingEventAcctPKey = Val(intPendingEventAcctPKey)
            //myVS.intPendingSessionPKey = Val(intPendingSessionPKey)
            //myVS.strSessionName = strSessionName
            //myVS.intAcctPKey = intAccount_PKey
            //myVS.Message = StrMessage.ToString
            //myVS.strDueDate = strDuedate
            //myVS.IsSlideCreate = IsSlideCreate
            //myVS.strEditContent = ""
            //myVS.IsLeader = IsLeader
            //myVS.IsModu = IMod
            ViewBag.cmdSendText = "Send";
            ViewBag.tdchkAllVisible = (Model.intStatus_pkey == 19);
            bool ishow = (Model.intStatus_pkey == 19 && Model.HaveSlide == 1);
            ViewBag.isShow = ishow;
            ViewBag.IsHaveSlides = ishow;
            ViewBag.hdfSlideFor = Model.HaveSlide;
            ViewBag.txtDueDateReadOnly = true;
            ViewBag.dpSlideduedateEnabled = false;
            ViewBag.PreAccountVisible = false;
            ViewBag.Model = Model;
            ViewBag.tdDateVisible = ishow;
            ViewBag.trShowVisible = ishow;
            if (Model.intPendingEvtSesPKey>0)
            {
                ViewBag.dScopeSValue = Model.intPendingEvtSesPKey;
                ViewBag.dScopeSEnabled =false;
            }
            ViewBag.chkAllChecked = false;
            ViewBag.cmdPreviewVisible = true;
            ViewBag.cmdDesignVisible = false;
            ViewBag.PreAccountVisible = false;
            ViewBag.cmdSendVisible = true;
            ViewBag.RadEditor1Enabled = true;
            ViewBag.txtBCCText = "";
            ViewBag.txtCCEmailText = "";
            ViewBag.cmdSend2Visible = (Model.intStatus_pkey == 19 && ishow);
            //If strNextConDate<> "" Then
            //    myVS.NextConDate = Convert.ToDateTime(strNextConDate).ToString("MM/dd/yy").ToString()
            //End If
            getAnnouncementPkey(Model.intStatus_pkey, Model.strPendingSessionName);

            if (ViewBag.intAnnouncement_PKey>0)
            {
                clsAnnouncement c = new clsAnnouncement();
                c.sqlConn = conn;
                c.lblMsg=null;
                c.intAnnouncement_PKey =ViewBag.intAnnouncement_PKey;
                c.LoadAnnouncement();
                ViewBag.EditorContent = c.strHTMLText;
                ViewBag.Subject =  c.strTitle;
                ViewBag.cmdDesignVisible= true;
                ViewBag.cmdPreviewVisible= true;
            }
            else
            {

            }
            DataTable dt = smop.RefreshSpeakerSendEmail(Model, ishow, Model.chkIsSlide);
            ViewBag.dtSpeaker =dt;
            if (!(dt != null && dt.Rows.Count>0))
                clsUtility.LogErrorMessage(null, null, GetType().Name, 0, "Error filling session's speaker");

            //            With cbSpeaker
            //                .DataTextField = "ContactName"
            //                .DataValueField = "Account_pkey"
            //                .DataSource = myVS.dtSpeaker
            //                .DataBind()
            //            End With
            //            dlSpeaker.DataSource = myVS.dtSpeaker
            //            dlSpeaker.DataBind()
            //            hdfCount.Value = myVS.dtSpeaker.Rows.Count
            //            myVS.EventSession_pkey = myVS.dtSpeaker.Rows(0)("eventSession_pkey").ToString
            //            ViewState(MY_VSTATE) = myVS
            //            If ishow Then
            //                Dim script As String = "function f(){CheckCount(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
            //                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)
            //            End If
            //            cbAccount.Items.Clear()
            //            If myVS.dtSpeaker.Rows.Count > 0 Then Me.FillDropdown_Preview(cbAccount, myVS.dtSpeaker) 
            //        Else
            //        End If
        
            ViewBag.cbSpeakerSelectedValue = Model.intPendingAcctPKey;
            //ViewBag.txtNextConDateVisible = IIf(strNextConDate <> "" And myVS.intStatus_pkey = 19, True, False)
            //tbNextContDate.Visible = IIf(strNextConDate <> "" And myVS.intStatus_pkey = 19, True, False)
            //If txtNextConDate.Visible And myVS.intStatus_pkey = 19 Then txtNextConDate.SelectedDate = Convert.ToDateTime(strNextConDate).ToString("MM/dd/yy")
            //lblNextConDate.Visible = txtNextConDate.Visible
            //If ISFollowuptime Then
            //    If lblNextConDate.Visible Then
            //        dpTime.SelectedDate = strNextConDate
            //    End If
            //End If
            ViewBag.chkBCCChecked = false;
            ViewBag.chkCChecked = false;
            ViewBag.trCCVisible =  "display:none";
            ViewBag.trCC1Visible =  "display:none";
            ViewBag.trBCCVisible =  "display:none";
            ViewBag.trBCC1Visible =  "display:none";
            ViewBag.lblMsgTextShowVisible = "display:none";

            return PartialView();
        }

        private PopUpType ShowOnTheBasisOfCookies()
        {
            HttpCookie reqCookies = Request.Cookies["IEX"];// System.Web.HttpContext.Current.Request.Browser.Cookies["IEX"];

            if (reqCookies != null)
            {
                if (reqCookies["name"] != null && reqCookies["name"].ToString() != "1")
                    return PopUpType.BrowserSupport;
                else
                    return PopUpType.EventInfo;
            }
            else
                return PopUpType.BrowserSupport;
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult GetBioForThisPerson(string myId, string userId, string eventKey)
        {
            try
            {
                User_Login data = new User_Login();
                if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
                {
                    DataTable table = new DataTable();
                    string Connection = ReadConnectionString();
                    SqlConnection conn = new SqlConnection(Connection);

                    int id;
                    bool success = int.TryParse(userId, out id);
                    
                    if (success)
                    {
                        SqlParameter[] sqlParameters = { new SqlParameter("@Account_pkey", id) };
                        DataTable dt = SqlHelper.ExecuteTable("API_AccountBIO_Select", System.Data.CommandType.StoredProcedure, sqlParameters);
                        if (dt.Rows.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(dt.Rows[0]["PersonalBio"].ToString()))
                            {
                                string strImagePath = "~/accountimages/" + id.ToString() + "_img.jpg";
                                string strPhysicalPath = Server.MapPath(strImagePath);
                                bool bExists = clsUtility.FileExists(strPhysicalPath);

                                if (!bExists)
                                    strImagePath = dt.Rows[0]["topImage"].ToString();

                                var jResult = new Bio()
                                {
                                    strBio = dt.Rows[0]["PersonalBio"].ToString(),
                                    strHeader = dt.Rows[0]["FirstName"].ToString() + " " + dt.Rows[0]["LastName"].ToString() + " Profile",
                                    strJobTitle = dt.Rows[0]["Title"].ToString(),
                                    strOrg = dt.Rows[0]["OrganizationID"].ToString(),
                                    topImage = dt.Rows[0]["topImage"].ToString(),
                                    strAddress = dt.Rows[0]["fullAddress"].ToString(),
                                    conStatus = ChatOperations.GetConStatusNow(Convert.ToInt32(myId), id, Convert.ToInt32(eventKey), -1)
                                };

                                var jTable = OnlineUsers.DataTableToJSON(table);
                                var jData = Json(new { notFound = false, data = jResult }, JsonRequestBehavior.AllowGet);
                                jData.MaxJsonLength = int.MaxValue;
                                return jData;
                            }
                        }
                    }
                }
            }
            catch{}
            return Json(new { notFound = true }, JsonRequestBehavior.AllowGet);
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult GetPhrases()
        {
            try
            {
                User_Login data = new User_Login();
                if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
                {
                    FormsIdentity identity = (FormsIdentity)User.Identity;
                    data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                    DataTable table = new DataTable();
                    string Connection = ReadConnectionString();
                    SqlConnection conn = new SqlConnection(Connection);
                    string qry = string.Empty;
                    qry = qry + Environment.NewLine + "select pkey,";
                    qry = qry + Environment.NewLine + "PhraseKey as [id],";
                    qry = qry + Environment.NewLine + "REPLACE(PhraseKey,'/','') as [EditId],";
                    qry = qry + Environment.NewLine + "[Text] as txt,";
                    qry = qry + Environment.NewLine + "cast(ISNULL([isForEveryone],0) as int) as [isGlobal] into #tempG";
                    qry = qry + Environment.NewLine + "from [dbo].[ChatPhrases]";
                    qry = qry + Environment.NewLine + "where ISNULL([isForEveryone],0)=1;";

                    qry = qry + Environment.NewLine + "select c.pkey,c.PhraseKey as [id],";
                    qry = qry + Environment.NewLine + "REPLACE(PhraseKey,'/','') as [EditId],";
                    qry = qry + Environment.NewLine + "c.[Text] as txt,";
                    qry = qry + Environment.NewLine + "cast(ISNULL(c.[isForEveryone],0) as int) as [isGlobal]";
                    qry = qry + Environment.NewLine + "into #myPhrases";
                    qry = qry + Environment.NewLine + "from ChatPhrases c";
                    qry = qry + Environment.NewLine + "where @accPkey = c.account_Key;";

                    qry = qry + Environment.NewLine + "select * from";
                    qry = qry + Environment.NewLine + "((select pkey,id,[EditId],txt,isGlobal from #myPhrases)";
                    qry = qry + Environment.NewLine + "union all";
                    qry = qry + Environment.NewLine + "(select pkey,id,[EditId],txt,isGlobal from #tempG g where g.id not in (select p.id from #myPhrases p))) as A";
                    qry = qry + Environment.NewLine + "where not exists(select 1 from PhrasesTrack t where A.isGlobal=1 and t.AccountKey=@accPkey and t.PhrasesKey=A.pkey)";
                    qry = qry + Environment.NewLine + "order by id;";

                    qry = qry + Environment.NewLine + "drop table #tempG,#myPhrases;";
                    SqlCommand cmd = new SqlCommand(qry);
                    cmd.Parameters.AddWithValue("@accPkey",data.Id);

                    if (
                        clsUtility.GetDataTable(conn, cmd, ref table, Msg: "Fetching the phrases")
                        && table.Rows.Count > 0
                        )
                    {
                        var jResult = table.AsEnumerable().Select(
                            x => new Phrase() 
                            { 
                                pkey = (x["pkey"] != DBNull.Value ? Convert.ToInt32(x["pkey"]) : 0),
                                id = (x["id"] != DBNull.Value ? Convert.ToString(x["id"]) : ""),
                                EditId = (x["EditId"] != DBNull.Value ? Convert.ToString(x["EditId"]) : ""),
                                txt = (x["txt"] != DBNull.Value ? Convert.ToString(x["txt"]) : ""),
                                isGlobal = (x["id"] != DBNull.Value ? Convert.ToBoolean(x["isGlobal"]) : false)
                            }
                            ).ToList<Phrase>();

                        var jTable = OnlineUsers.DataTableToJSON(table);
                        var jData = Json(new { isError = false, notFound = false, data = jResult, jsonTable = jTable }, JsonRequestBehavior.AllowGet);
                        jData.MaxJsonLength = int.MaxValue;
                        return jData;

                        //return Json(new { isError = false, notFound = false ,data = jResult }, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new { isError = false, notFound = true}, JsonRequestBehavior.AllowGet);
            }
            catch//(Exception ex)
            {
                return Json(new { isError = true, msg = "An error occurred while Fetching the phrases" }, JsonRequestBehavior.AllowGet);
            }
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult DeleteThisPhrase(string key)
        {
            try
            {
                User_Login data = new User_Login();
                if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
                {
                    FormsIdentity identity = (FormsIdentity)User.Identity;
                    data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                    DataTable table = new DataTable();
                    string Connection = ReadConnectionString();
                    SqlConnection conn = new SqlConnection(Connection);
                    string qry = string.Empty;
                    qry = "delete from dbo.ChatPhrases where pkey=@phKey and account_Key=@accPkey;";
                    qry = qry + Environment.NewLine + "IF EXISTS(select 1 from dbo.ChatPhrases WHERE isForEveryone = 1 AND pkey = @phKey)";
                    qry = qry + Environment.NewLine + "AND NOT EXISTS(SELECT 1 FROM PhrasesTrack t where [AccountKey] =@accPkey and [PhrasesKey] = @phKey)";
                    qry = qry + Environment.NewLine + "BEGIN";
                    qry = qry + Environment.NewLine + "insert into [PhrasesTrack]([AccountKey],[PhrasesKey])values";
                    qry = qry + Environment.NewLine + "(@accPkey,@phKey);";
                    qry = qry + Environment.NewLine + "END";

                    SqlCommand cmd = new SqlCommand(qry);
                    cmd.Parameters.AddWithValue("@accPkey", data.Id);
                    cmd.Parameters.AddWithValue("@phKey", key);

                    if (clsUtility.GetDataTable(conn, cmd, ref table, Msg: "Deleting the phrases"))
                    {
                        return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch{}

            return Json(new { msg = "An error occurred while Fetching the phrases" }, JsonRequestBehavior.AllowGet);
        }


        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult SavePhrase(string key, string value, string pkey, string globalPkey)
        {
            try
            {
                User_Login data = new User_Login();
                if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
                {
                    FormsIdentity identity = (FormsIdentity)User.Identity;
                    data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

                    Regex regex = new Regex("^[a-zA-Z0-9]*$");

                    if (!regex.IsMatch(key))
                    {
                        return Json(new { msg = "In code field only alphanumeric are allowed." }, JsonRequestBehavior.AllowGet);
                    }

                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                    {
                        if (!key.Contains(" "))
                        {
                            if (!(key.First(x => 1 == 1) == '/'))
                                key = "/" + key;

                            string qry = string.Empty;
                            bool isUpdate = false;

                            if (!string.IsNullOrEmpty(pkey))
                            {
                                qry = "update ChatPhrases";
                                qry = qry + Environment.NewLine + "set [PhraseKey] = @key,[Text] = @Text";
                                qry = qry + Environment.NewLine + "where pkey = @pkey and [account_Key] = @accPkey;";

                                isUpdate = true;
                            }
                            else
                            {
                                qry = "if(exists(select 1 from dbo.ChatPhrases where PhraseKey=@key COLLATE SQL_Latin1_General_CP1_CS_AS and account_Key = @accPkey))";
                                qry = qry + Environment.NewLine + "begin";
                                qry = qry + Environment.NewLine + "select 'already here';";
                                qry = qry + Environment.NewLine + "end";
                                qry = qry + Environment.NewLine + "else";
                                qry = qry + Environment.NewLine + "begin";
                                qry = qry + Environment.NewLine + "insert into ChatPhrases";
                                qry = qry + Environment.NewLine + "(account_Key,PhraseKey,[Text])";
                                qry = qry + Environment.NewLine + "values(@accPkey,@key,@Text)";
                                qry = qry + Environment.NewLine + "end";

                                if (!string.IsNullOrEmpty(globalPkey))
                                {
                                    qry = qry + Environment.NewLine + "IF EXISTS(select 1 from dbo.ChatPhrases WHERE isForEveryone = 1 AND pkey = @globalPkey)";
                                    qry = qry + Environment.NewLine + "AND NOT EXISTS(SELECT 1 FROM PhrasesTrack t where [AccountKey]=@accPkey and [PhrasesKey]=@globalPkey)";
                                    qry = qry + Environment.NewLine + "BEGIN";
                                    qry = qry + Environment.NewLine + "insert into [PhrasesTrack]([AccountKey],[PhrasesKey])values";
                                    qry = qry + Environment.NewLine + "(@accPkey,@globalPkey);";
                                    qry = qry + Environment.NewLine + "END";
                                }
                            }

                            DataTable dt = new DataTable();
                            string Connection = ReadConnectionString();
                            SqlConnection conn = new SqlConnection(Connection);
                            SqlCommand cmd = new SqlCommand(qry);
                            cmd.Parameters.AddWithValue("@accPkey", data.Id);
                            cmd.Parameters.AddWithValue("@pkey", pkey);
                            cmd.Parameters.AddWithValue("@key", key);
                            cmd.Parameters.AddWithValue("@Text", value);
                            cmd.Parameters.AddWithValue("@globalPkey", globalPkey);

                            if (clsUtility.GetDataTable(conn, cmd, ref dt, Msg: "Deleting the phrases"))
                            {
                                if (isUpdate)
                                {
                                    return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
                                }
                                else if (!isUpdate && dt.Rows.Count == 0)
                                {
                                    return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json(new { msg = "A phrase with same key already exists!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        else
                        {
                            return Json(new { msg = "Please remove space between <b>key</b> value" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { msg = "Key and Text is mandatory" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch
            {
                return Json(new { msg = "Error in saving the Phrase" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
        }

        [AjaxValidateAntiForgeryToken]
        [ValidateInput(true)]
        public JsonResult GetEventUpdates(bool IsDontShowIEPopUp_Checked)
        {
            try
            {
                string strName = System.Web.HttpContext.Current.Request.Browser.Browser.ToUpper();
                string browsersupportText = string.Empty;
                PopUpType popUpType;

                if (strName.IndexOf("FIREFOX") > -1)
                {
                    strName = "Mozilla Firefox";
                    browsersupportText = "This website does not support " + strName + " so certain functions will not operate properly. It operates best with the Chrome or Edge browsers.";
                    popUpType = ShowOnTheBasisOfCookies();
                }
                else if (strName.IndexOf("SAMSUNGBROWSER") > -1)
                {
                    strName = "Samsung Internet";
                    browsersupportText = "This website does not support " + strName + " so certain functions will not operate properly. It operates best with the Chrome or Edge browsers.";
                    popUpType = ShowOnTheBasisOfCookies();
                }
                else if (strName.IndexOf("OPERA") > -1 || strName.IndexOf("OPR") > -1)
                {
                    strName = "Opera";
                    browsersupportText = "This website does not support " + strName + " so certain functions will not operate properly. It operates best with the Chrome or Edge browsers.";
                    popUpType = ShowOnTheBasisOfCookies();
                }
                else if (strName.IndexOf("CHROME") > -1)
                {
                    strName = "Google Chrome or Chromium";
                    popUpType = PopUpType.EventInfo;
                }
                else if (strName.IndexOf("SAFARI") > -1)
                {
                    strName = "Apple Safari";
                    browsersupportText = "This website does not support " + strName + " so certain functions will not operate properly. It operates best with the Chrome or Edge browsers.";
                    popUpType = ShowOnTheBasisOfCookies();
                }
                else
                    popUpType = PopUpType.EventInfo;


                if(popUpType == PopUpType.EventInfo)
                {
                    SetBrowserPopupCookie(IsDontShowIEPopUp_Checked);
                    clsEvent cEvent = new clsEvent();
                    clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
                    clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                    clsAccount cAccount = ((clsAccount)Session["cAccount"]);
                    cEvent.intEvent_PKey = cLast.intActiveEventPkey;
                    cEvent.LoadEvent();
                    DateTime d = clsEvent.getEventVenueTime();
                    DateTime mindateTime = new DateTime(cEvent.dtStartDate.Year, cEvent.dtStartDate.Month, cEvent.dtStartDate.Day, 7, 0, 0);
                    bool bEventPopup = false;
                    string VirtualInstruction = string.Empty;

                    string EventUpdatePopUP = cSettings.getText(clsSettings.Text_EventUpdate);
                    if ((cEvent.intEventStatus_PKey == clsEvent.STATUS_Pending & d < mindateTime & (!cAccount.bGlobalAdministrator | !cAccount.bStaffMember)))
                    {
                        if (((cEvent.bEventOpenStaff && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "IsEventOpenForStaff")) & (cAccount.bStaffForCurEvent | cAccount.bGlobalAdministrator | cAccount.bStaffMember)))
                            bEventPopup = true;
                        else if (cEvent.bEventOpenEventSponsors & clsEventOrganization.CheckExhibitor(cAccount.intParentOrganization_pKey, cLast.intActiveEventPkey) && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "IsEventOpenForEventSponsors"))
                            bEventPopup = true;
                        else if ((cEvent.bEventOpenSpeakers && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "IsEventOpenForSpeaker") & cAccount.intNumTimesSpeakingCurEvent > 0))
                            bEventPopup = true;
                        else if ((cEvent.bEventClosedAttendees & cAccount.bAttendeeAtCurrEvent & !(cAccount.bStaffForCurEvent | cAccount.bGlobalAdministrator | cAccount.bStaffMember)))
                            bEventPopup = true;
                        else
                        {
                            VirtualInstruction = cEvent.strBigButtonPopupText;
                            bEventPopup = false;
                        }
                    }
                    else if (((cEvent.intEventStatus_PKey == clsEvent.STATUS_Completed & d.Date > cEvent.dtEndDate.Date) & (!cAccount.bGlobalAdministrator & !cAccount.bStaffMember)))
                    {
                        VirtualInstruction = cEvent.strEventFullname + " has closed. " + "\"" + "See" + "\"" + " you next time!";
                        bEventPopup = false;
                    }
                    else
                        bEventPopup = true;

                    if (bEventPopup)
                    {
                        if (cEvent.bMAGIUpdate && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "IsMAGIUpdate"))
                        {
                            //////OpenUpdatePopupInfo(); need to implement
                        }
                        else
                        {
                            //////redirect on event on cloud
                        }
                    }
                }
                else if(popUpType == PopUpType.BrowserSupport)
                {

                }

                var JSonResult = Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
                JSonResult.MaxJsonLength = int.MaxValue;
                return JSonResult;
            }
            catch
            {
                return Json(new { msg = "Error Occurred While Fetching History" }, JsonRequestBehavior.AllowGet);
            }
        }

        private void SetBrowserPopupCookie(bool IsDontShowIEPopUp_Checked)
        {
            HttpCookie IEX = new HttpCookie("IEX");
            if ((IsDontShowIEPopUp_Checked))
            {
                IEX["name"] = "1";
                IEX.Expires = DateTime.Today.AddDays(1);
            }
            else
            {
                IEX["name"] = "-1";
                IEX.Expires = DateTime.Today.AddDays(-1);
            }
            Response.Cookies.Add(IEX);
        }
    }
}
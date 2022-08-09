using fcConferenceManager.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Web.Security;
using System.Configuration;
using Newtonsoft.Json;
using Microsoft.VisualBasic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace fcConferenceManager.Controllers
{
    [CheckActiveEventAttribute]
    public class ResourcesController : Controller
    {
        // GET: Resources
        DBAccessLayer dba = new DBAccessLayer();
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

        #region Journal
        public ActionResult Journal()
        {
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);

            string apppath = clsSettings.APP_URL().Replace("/forms", "");
            ViewBag.apppath = apppath;

            User_Login data = new User_Login();
            clsAccount cAccount = new clsAccount();
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                int AccountPkey = Convert.ToInt32(User.Identity.Name);
                cAccount.intAccount_PKey = AccountPkey;
                cAccount.sqlConn = new SqlConnection(ReadConnectionString());
                cAccount.LoadAccount();

                ViewBag.ID = data.Id;
                ViewBag.FullName = data.FirstName + ((data.MiddleName.Trim() == "") ? " " : " " + data.MiddleName + " ") + data.LastName;
                ViewBag.EventPKey = data.EventId;
                ViewBag.EventAccountPKey = data.EventAccount_pkey;
                ViewBag.EventTypeID = data.EventTypeId;
                ViewBag.IsStaff = (data.GlobalAdmin || data.StaffMember);

                DateTime dtCurrentTime = clsEvent.getEventVenueTime();
                ViewBag.CurrentTime = dtCurrentTime;
            }
            else
            {
                data.EventId = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
                data.Id = 0;
            }
            BindMonths();
            BindYears();

            ViewBag.TitleText = (Request.QueryString["search"] != null) ? Request.QueryString["search"] : cLast.strprvsrch;
            ViewBag.lblJournalInst = cSettings.getText(clsSettings.Text_JournalInstruct);

            var count = dba.JournalCount();
            ViewBag.Count = string.Format("{0:#,###0}", count);

            if (cAccount.bGetJournal)
            {
                ViewBag.btnSubLink = "Unsubscribe from Journal Alert";
                ViewBag.lblSubLink = "Your subscription is active.";
                ViewBag.lblSubLinkVisible = true;
            }
            else
            {
                ViewBag.btnSubLink = "Subscribe to Journal Alert";
                ViewBag.lblSubLink = "";
                ViewBag.lblSubLinkVisible = false;
            }
            ViewBag.bckCrrntSrch = cLast.bckCrrntSrch;
            return View();
        }
        public void BindMonths()
        {
            DataTable dt = dba.BindJournelMonths();
            List<SelectListItem> monthList = new List<SelectListItem>();
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (DateTime.Now.Month == Convert.ToByte(dr[0]))
                        monthList.Add(new SelectListItem { Text = dr[1].ToString(), Value = dr[0].ToString() });
                    else
                        monthList.Add(new SelectListItem { Text = dr[1].ToString(), Value = dr[0].ToString() });
                }

                monthList[Convert.ToInt32(dt.Rows[0]["mnth"]) - 1].Selected = true;
            }

            ViewBag.ddMonth = monthList;
        }
        public void BindYears()
        {
            DataTable dt = dba.BindJournelYear();
            List<SelectListItem> yearList = new List<SelectListItem>();
            string SelectedYear = string.Empty;

            if(dt.Rows.Count > 0)
            {
                SelectedYear = dt.Rows[0]["selectedYear"].ToString();
            }
            foreach (DataRow dr in dt.Rows)
            {
                if(SelectedYear == dr[1].ToString())
                    yearList.Add(new SelectListItem { Text = dr[1].ToString(), Value = dr[0].ToString(), Selected = true });
                else
                    yearList.Add(new SelectListItem { Text = dr[1].ToString(), Value = dr[0].ToString() });
            }
            ViewBag.ddYear = yearList;
        }

        public JsonResult RefreshJornaltable(string txtTitle, string ddYear, string ddMonth, bool bckCurrentSrch, string rdSort, int SelInedx, int Statusvalue, string strAccountPkey, bool needSearchAmongAll = false)
        {
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            int intAccount_PKey = 0;
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
                intAccount_PKey = Convert.ToInt32(User.Identity.Name);
            
            cLast.strprvsrch = (bckCurrentSrch) ? txtTitle : "";
            cLast.bckCrrntSrch = bckCurrentSrch;
            DateTime now = DateTime.Now, _Date = DateTime.Now.Date;
            int h = now.Hour, m = now.Minute, s = now.Second, Ml = now.Millisecond;

            string startTime = now.ToString(), endTime = now.ToString();

            string apppath = clsSettings.APP_URL().Replace("/forms", ""),
               ID = "", strText = cLast.strWebLastSrch, strSrchText = txtTitle, prvSrchText = cLast.strprvsrch;

            bool bExport = false;
            ID = (!string.IsNullOrEmpty(strText)) ? strText : strSrchText;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            var dt = dba.RefreshSearchJournel(apppath, ID, bExport, ddYear, ddMonth, bckCurrentSrch, prvSrchText, Convert.ToInt32(rdSort), strSrchText, SelInedx, Statusvalue, txtTitle, intAccount_PKey, needSearchAmongAll: needSearchAmongAll);
            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            if (txtTitle != "")
            {
                startTime = now.ToString("MM/dd/yyyy HH:mm:ss.fff");
                endTime = String.Format("{0:00}/{1:00}/{2:0000} {3:00}:{4:00}:{5:00}.{6:000}", _Date.Month, _Date.Day, _Date.Year, ts.Hours + h, ts.Minutes + m, ts.Seconds + s, ts.Milliseconds / 10 + Ml);
                dba.updateFdaGcpSearch(startTime, endTime);
            }
            var jsonResult = Json(new { msg = "OK", Source = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;

            Session["cLastUsed"] = cLast;
            return jsonResult;
        }

        public ActionResult SuscribeJournal()
        {
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            clsAccount cAccount = new clsAccount();
            int AccountPkey = 0;
            if (User.Identity.AuthenticationType == "Forms")
            {
                AccountPkey = Convert.ToInt32(User.Identity.Name);
                cAccount.intAccount_PKey = AccountPkey;
                cAccount.sqlConn = new SqlConnection(ReadConnectionString());
                cAccount.LoadAccount();
            }
            if (!(AccountPkey > 0))
            {
                cLast.bRedirPending = true;
                cLast.intRedirLink = 332;
                return Json(new { res = "Redirect", link = "/Home/Login" }, JsonRequestBehavior.AllowGet);
            }

            bool chk = !cAccount.bGetJournal;
            var res = dba.UpdateJournal(chk, AccountPkey);
            if (chk)
                return Json(new { res = "OK", msg = "You have subscribed to the Journal Alert" }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { res = "OK", msg = "You have unsubscribed from the Journal Alert" }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Standards
        public ActionResult Standards()
        {
            CheckLoginType();
            clsSettings cSettings = ((clsSettings)Session["cSettings"]);
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            clsAccount cAccount = new clsAccount();
            int AccountPkey = 0;
            int EventPkey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
            if (User.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                AccountPkey = data.Id;
                cAccount.intAccount_PKey = data.Id;
                cAccount.sqlConn = new SqlConnection(ReadConnectionString());
                cAccount.LoadAccount();
            }
            bool bLoggedIn = (AccountPkey > 0);

            ViewBag.bLoggedIn = bLoggedIn;
            ViewBag.bMAGIMember = cAccount.bIsMember;
            bool btncmdMemberVisible = false;
            if (cAccount.intAccount_PKey > 0 && !cAccount.bIsMember)
            {
                btncmdMemberVisible = true;
            }
            ViewBag.btncmdMemberVisible = btncmdMemberVisible;

            int intTextBlock = (bLoggedIn ? clsSettings.TEXT_Standards_Member : clsSettings.TEXT_Standards_Nonmember);
            string lblTxt = clsReservedWords.ReplaceMyPageText(null, cSettings.getText(intTextBlock), intEvtPKey: EventPkey);
            var count = dba.StandardCount();
            string c = string.Format("{0:#,###0}", count);
            ViewBag.lblText = lblTxt.Replace("[count]", c);
            BindDropDownStandards();
            return View();

        }
        public void BindDropDownStandards()
        {
            DataTable dsCategory = dba.ddCategory();
            List<SelectListItem> CategoryList = new List<SelectListItem>();
            CategoryList.Add(new SelectListItem { Text = "All Categories", Value = "0" });
            foreach (DataRow dr in dsCategory.Rows)
            {
                CategoryList.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pKey"].ToString() });
            }
            ViewBag.ddCategory = CategoryList;

            DataTable dsStages = dba.ddStages();
            List<SelectListItem> StagesList = new List<SelectListItem>();
            StagesList.Add(new SelectListItem { Text = "All Stages", Value = "0" });
            foreach (DataRow dr in dsStages.Rows)
            {
                StagesList.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pKey"].ToString() });
            }
            ViewBag.ddPhase = StagesList;

            DataTable dsUsers = dba.ddUsers();
            List<SelectListItem> UserList = new List<SelectListItem>();
            UserList.Add(new SelectListItem { Text = "All Users", Value = "0" });
            foreach (DataRow dr in dsUsers.Rows)
            {
                UserList.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pKey"].ToString() });
            }
            ViewBag.ddUsers = UserList;

        }
        public ActionResult RefreshStandardGrid(bool bLoggedIn, int ddCategory, int ddPhase, int ddUser, string strText, string strAccountPkey)
        {
            int intAccount_PKey = 0;
            if (strAccountPkey != "")
            {
                intAccount_PKey = Convert.ToInt32(strAccountPkey);
            }
            DateTime now = DateTime.Now;
            int h = now.Hour;
            int m = now.Minute;
            int s = now.Second;
            int Ml = now.Millisecond;
            DateTime Dates = DateTime.Now.Date;

            string startTime = DateTime.Now.ToString();
            string endTime = DateTime.Now.ToString();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();


            var dt = dba.RefreshStandardGrid(bLoggedIn, ddCategory, ddPhase, ddUser, strText, intAccount_PKey);

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            if (strText != "")
            {
                startTime = now.ToString("MM/dd/yyyy HH:mm:ss.fff");
                endTime = String.Format("{0:00}/{1:00}/{2:0000} {3:00}:{4:00}:{5:00}.{6:000}", Dates.Month, Dates.Day, Dates.Year, ts.Hours + h, ts.Minutes + m, ts.Seconds + s, ts.Milliseconds / 10 + Ml);
                dba.updateFdaGcpSearch(startTime, endTime);
            }
            return Json(new { msg = "OK", Source = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateArchive(List<StandardItems> sItems)
        {

            //define work area
            string archive = Server.MapPath("~/app_data/DownloadTemp/MAGI Standards.zip");
            string tempFolder = Server.MapPath("~/app_data/DownloadTemp/WorkArea");

            //clear any existing archive

            if (System.IO.File.Exists(archive))
            {
                if (!clsUtility.DeleteFile(archive, null))
                    return Json(new { res = "error", msg = "Error deleting archive file" }, JsonRequestBehavior.AllowGet);
            }

            //empty the temp folder

            //get a list of the files, if any

            Collection colFilesToDelete = new Collection();
            string[] filePaths = Directory.GetFiles(tempFolder + "/", "*", SearchOption.TopDirectoryOnly);
            List<SelectListItem> files = new List<SelectListItem>();
            foreach (string filePath in filePaths)
            {
                string strFilename = Path.GetFileName(filePath);
                colFilesToDelete.Add(strFilename);
            }

            // delete them
            if (colFilesToDelete.Count > 0)
            {
                for (int i = 1; i <= colFilesToDelete.Count; i++)
                {
                    string strPhysicalPath = tempFolder + "/" + colFilesToDelete[i].ToString();
                    if (System.IO.File.Exists(strPhysicalPath))
                    {
                        if (!clsUtility.DeleteFile(strPhysicalPath, null))
                            return Json(new { res = "error", msg = "Error deleting temporary file" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            // copy the selected files to the temp folder

            clsAccount cAccount = new clsAccount();

            bool Checker = false;
            foreach (var item in sItems)
            {
                if (item.IsChecked)
                {
                    Checker = true;
                    int intStdPKey = item.Pkey;
                    if (item.Pkey > 0)
                    {
                        string strUserFileName = item.FileName;
                       
                        if (strUserFileName != "")
                        {
                            string strPhysicalPath = Server.MapPath("~/Standards/") + strUserFileName;
                            if (clsUtility.FileExists(strPhysicalPath))
                            {
                                // '--Delete existing file
                                if (!clsUtility.DeleteFile(tempFolder + "/" + strUserFileName, null))
                                {
                                    clsUtility.LogErrorMessage(null, null, null, 0, "Error deleting temporary file");
                                    return Json(new { res = "error", msg = "Error deleting temporary file" }, JsonRequestBehavior.AllowGet);
                                }
                                //--copy the file
                                if (strUserFileName.StartsWith(intStdPKey.ToString()))
                                    strUserFileName = strUserFileName.Substring(intStdPKey.ToString().Length + 1, strUserFileName.Length - (intStdPKey.ToString().Length + 1));
                                System.IO.File.Copy(strPhysicalPath, tempFolder + "/" + strUserFileName);
                                int AccountPkey = 0;
                                if (User.Identity.AuthenticationType == "Forms")
                                {
                                    FormsIdentity identity = (FormsIdentity)User.Identity;
                                    User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                                    AccountPkey = data.Id;
                                }
                                cAccount.intAccount_PKey = AccountPkey;
                                cAccount.sqlConn = new SqlConnection(ReadConnectionString());
                                cAccount.LoadAccount();
                                if (cAccount != null)
                                    cAccount.LogAuditMessage("Download: Standard - " + strUserFileName, clsAudit.LOG_DownloadStd);
                            }
                        }
                    }
                }

            }


            // '--create a new archive 
            if (Checker)
            {
                try
                {
                    ZipFile.CreateFromDirectory(tempFolder, archive);
                }
                catch (Exception ex)
                {
                    clsUtility.LogErrorMessage(null, null, null, 0, "Error creating zip file");
                    return Json(new { res = "error", msg = "Error creating zip file" }, JsonRequestBehavior.AllowGet);
                }




                Byte[] bts = System.IO.File.ReadAllBytes(archive);
                return Json(new { res = "OK" }, JsonRequestBehavior.AllowGet);

                // '--download the file                                      
            }
            else
            {
                return Json(new { res = "error", msg = "Select at least one or more checkbox." }, JsonRequestBehavior.AllowGet);
            }

        }

        [CustomizedAuthorize]
        public FileResult DownloadStandardZip()
        {

            string strScheduleTargetFile = Server.MapPath("~/app_data/DownloadTemp/MAGI Standards.zip");
            if (System.IO.File.Exists(strScheduleTargetFile))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(strScheduleTargetFile);
                return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, "MAGI Standards.zip");
            }
            return null;
        }

        #endregion

        #region Directories

        public ActionResult Directories()
        {
            CheckLoginType();

            var dt = dba.RefreshDirectories();
            if (dt.Rows.Count > 0)
            {
                ViewBag.btnAssociation = "Associations (" + string.Format("{0:#,###0}", dt.Rows[0]["ctn"].ToString()) + ")";
                ViewBag.btnBooks = "Bookshelf (" + string.Format("{0:#,###0}", dt.Rows[1]["ctn"].ToString()) + ")";
                ViewBag.btnGovt = "Government Organizations (" + string.Format("{0:#,###0}", dt.Rows[2]["ctn"].ToString()) + ")";
                ViewBag.btnSup = "Solution Providers (" + string.Format("{0:#,###0}", dt.Rows[7]["ctn"].ToString()) + ")";
                ViewBag.btnPub = "Publications (" + string.Format("{0:#,###0}", dt.Rows[4]["ctn"].ToString()) + ")";
                ViewBag.btnStaff = "Staffing & Employment Services (" + string.Format("{0:#,###0}", dt.Rows[6]["ctn"].ToString()) + ")";
                ViewBag.btnSpon = "Study Sponsors (" + string.Format("{0:#,###0}", dt.Rows[5]["ctn"].ToString()) + ")";
                ViewBag.btnEvent = "Training & Education Calendar (" + string.Format("{0:#,###0}", dt.Rows[9]["ctn"].ToString()) + ")";
                ViewBag.btnTrain = "Training & Education Providers (" + string.Format("{0:#,###0}", dt.Rows[8]["ctn"].ToString()) + ")";
                ViewBag.btnWeb = "Webinar Calendar (" + string.Format("{0:#,###0}", dt.Rows[3]["ctn"].ToString()) + ")";
            }
            return View();

        }

        #endregion

        #region RegDocs

        public ActionResult RegDocs()
        {
            CheckLoginType();
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";


            return View();
        }
        public ActionResult RefreshRegDocs(string txtTitle, string lkDisplay, int ddDocuments, bool ckCurrentSrch, bool ckCFR, bool ckStatutes, bool ckICH, bool chGuidances, bool ckInternational, bool ckOther,string previousSearch)
        {
            string ID = "";
            ID = txtTitle;
            string prvSrchText = previousSearch;

            int intAccount_PKey = 0;
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
                intAccount_PKey = Convert.ToInt32(User.Identity.Name);

            DateTime now = DateTime.Now;
            int h = now.Hour;
            int m = now.Minute;
            int s = now.Second;
            int Ml = now.Millisecond;
            DateTime Dates = DateTime.Now.Date;

            string startTime = DateTime.Now.ToString();
            string endTime = DateTime.Now.ToString();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var dt = dba.RefreshRegDocs(ID, lkDisplay, ddDocuments, ckCurrentSrch, prvSrchText, ckCFR, ckStatutes, ckICH, chGuidances, ckInternational, ckOther, intAccount_PKey);
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            if (txtTitle != "")
            {
                startTime = now.ToString("MM/dd/yyyy HH:mm:ss.fff");
                endTime = String.Format("{0:00}/{1:00}/{2:0000} {3:00}:{4:00}:{5:00}.{6:000}", Dates.Month, Dates.Day, Dates.Year, ts.Hours + h, ts.Minutes + m, ts.Seconds + s, ts.Milliseconds / 10 + Ml);
                dba.updateFdaGcpSearch(startTime, endTime);
            }
            return Json(new { msg = "OK", Source = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult BindddDocument(string type)
        {

            DataTable ds = dba.BindDocumentDropdown(type);
            List<SelectListItem> ddDoclist = new List<SelectListItem>();
            foreach (DataRow dr in ds.Rows)
            {
                ddDoclist.Add(new SelectListItem { Text = dr["Title"].ToString(), Value = dr["pkey"].ToString() });
            }
            return Json(new { msg = "OK", ddDoclist }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult lkDisplayLink(string linktext)
        {
            var dt = dba.lkDisplayLink(linktext);
            string aICH = "";
            string link = "";
            if (dt.Rows.Count > 0)
            {
                if (!(dt.Rows[0]["url"] == null) && dt.Rows[0]["url"].ToString() != "" && !(dt.Rows[0]["Category"] == null) && dt.Rows[0]["category"].ToString().Contains("ICH"))
                {
                    aICH = dt.Rows[0]["url"].ToString();
                    return Json(new { msg = "popup", popuplink = aICH }, JsonRequestBehavior.AllowGet);
                    //       clsUtility.PopupRadWindow(ScriptManager.GetCurrent(Me.Page), Me.Page, Me.RadWindow1)
                }
                else
                {
                    if (dt.Rows[0]["url"] != null)
                    {
                        link = "/RegDocsRecords?Pkey=" + dt.Rows[0]["pKey"].ToString();
                        return Json(new { msg = "OK", link = link }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return null;
        }
        #endregion


        #region FdaGcp

        public ActionResult FdaGcp()
        {
            CheckLoginType();
            var count = dba.FdaGcpCount();
            ViewBag.FdaGcpCount = string.Format("{0:#,###0}", count);
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            return View();
        }
        
        public ActionResult RefreshFdaGcp(bool ckCurrentSrch, int ddAnswrIndex, string ddAnswrText, string rdSor1, string txtTitle, bool ckPuncuation ,string previousSearch)
        {
            clsLastUsed cLast = ((clsLastUsed)Session["cLastUsed"]);
            cLast.strSearchFda = txtTitle;
            cLast.fdaYears = ddAnswrIndex;
            string strSrchText = (!ckPuncuation ? Filter(txtTitle) : txtTitle);
            string prvSrchText = "";
            //prvSrchText = (ckCurrentSrch ? prvSrchText + (prvSrchText != "" ? "/" : "") : "") + strSrchText;
            prvSrchText = (ckCurrentSrch ? prvSrchText + (prvSrchText != "" ? "/" : "") : "") + previousSearch;          

            DateTime now = DateTime.Now;
            int h = now.Hour;
            int m = now.Minute;
            int s = now.Second;
            int Ml = now.Millisecond;
            DateTime Dates = DateTime.Now.Date;

            string startTime = DateTime.Now.ToString();
            string endTime = DateTime.Now.ToString();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            string ID = strSrchText;

            var dt = dba.FdaGcpRefresh(ID, ckCurrentSrch, prvSrchText, ddAnswrIndex, ddAnswrText, rdSor1);

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            if (txtTitle != "")
            {
                startTime = now.ToString("MM/dd/yyyy HH:mm:ss.fff");
                endTime = String.Format("{0:00}/{1:00}/{2:0000} {3:00}:{4:00}:{5:00}.{6:000}", Dates.Month, Dates.Day, Dates.Year, ts.Hours + h, ts.Minutes + m, ts.Seconds + s, ts.Milliseconds / 10 + Ml);
                dba.updateFdaGcpSearch(startTime, endTime);
            }

            var jsonResult = Json(new { msg = "OK", Source = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        private string Filter(string str)
        {
            string tmp = str;
            tmp = tmp.Replace(",", "");
            tmp = tmp.Replace("!", "");
            tmp = tmp.Replace("^", "");
            tmp = tmp.Replace("&", "");
            tmp = tmp.Replace("*", "");
            tmp = tmp.Replace("(", "");
            tmp = tmp.Replace(")", "");
            tmp = tmp.Replace("{", "");
            tmp = tmp.Replace("}", "");
            tmp = tmp.Replace("[", "");
            tmp = tmp.Replace("]", "");
            tmp = tmp.Replace("|", "");
            tmp = tmp.Replace(";", "");
            tmp = tmp.Replace(":", "");
            tmp = tmp.Replace(".", "");
            tmp = tmp.Replace("?", "");
            tmp = tmp.Replace("`", "");
            tmp = tmp.Replace("~", "");
            tmp = tmp.Replace("<", "");
            tmp = tmp.Replace(">", "");
            tmp = tmp.Replace("+", "");
            tmp = tmp.Replace("=", "");
            tmp = tmp.Replace("-", "");
            tmp = tmp.Replace("#", "");
            tmp = tmp.Replace("\"\"", "");


            return tmp;
        }
        #endregion


        #region Glossary

        public ActionResult Glossary()
        {
            CheckLoginType();
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            return View();
        }


        public ActionResult RefreshGlossary(string txtGenSearch, bool ckCurrentSrch, bool chkDefinition , string previousSearch)
        {
            List<SelectListItem> ddDoclist = new List<SelectListItem>();
            int AccountPkey = 0;
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
                AccountPkey = Convert.ToInt32(User.Identity.Name);

            string ID = txtGenSearch;
            string prvSrchText = previousSearch;
            var dt = dba.RefreshGlossary(ID, ckCurrentSrch, prvSrchText, chkDefinition);
            string intpkey = "";
            if (txtGenSearch != "" && AccountPkey > 0)
            {
                int IntFound = 0;
                if (dt.Rows.Count > 0)
                    IntFound = 1;

                intpkey = dba.SaveSearch(txtGenSearch, IntFound, AccountPkey, clsUtility.TYPE_Glossary);
            }
            if (AccountPkey > 0)
            {
                var ds = dba.FillGlossaryDropdown(AccountPkey, clsUtility.TYPE_Glossary);
                foreach (DataRow dr in ds.Rows)
                {
                    ddDoclist.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pkey"].ToString() });
                }
            }
            return Json(new { msg = "OK", intpkey = intpkey, dropdownvalues = ddDoclist, Source = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SrchSave(int intpkey, string type)
        {
            int AccountPkey = 0;
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
                AccountPkey = Convert.ToInt32(User.Identity.Name);

            if (intpkey > 0)
            {
                var res = dba.SaveSrchSave(intpkey);
                if (res)
                {
                    List<SelectListItem> ddDoclist = new List<SelectListItem>();
                    int glossarytype = clsUtility.TYPE_Glossary;
                    if (type == "lcfGlossary")
                    {
                        glossarytype = clsUtility.TYPE_IcfGlossary;
                    }
                    var ds = dba.FillGlossaryDropdown(AccountPkey, glossarytype);
                    foreach (DataRow dr in ds.Rows)
                    {
                        ddDoclist.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pkey"].ToString() });
                    }
                    return Json(new { msg = "OK", dropdownvalues = ddDoclist, resmsg = "Saved" }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { msg = "error", resmsg = "Error updating search saved." }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { msg = "error", resmsg = "Only searched text can be saved (first search text and then click on save button)" }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult DeleteSearch(int selectedvalue)
        {
            var ds = dba.DeleteSaveSearch(selectedvalue);
            if (ds)
            {
                return Json(new { msg = "OK", resmsg = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { msg = "error", resmsg = "Error in updating filter." }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        #region lcfGlossary

        public ActionResult lcfGlossary()
        {
            CheckLoginType();
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            return View();
        }
        public ActionResult RefreshlcfGlossary(string txtGenSearch, bool ckCurrentSrch, bool chkDefinition ,string previousSearch)
        {
            List<SelectListItem> ddDoclist = new List<SelectListItem>();
            int AccountPkey = 0;
            if (User.Identity.IsAuthenticated == true && User.Identity.AuthenticationType == "Forms")
                AccountPkey = Convert.ToInt32(User.Identity.Name);
            string ID = txtGenSearch;
            string prvSrchText = previousSearch;
            var dt = dba.RefreshlcfGlossary(ID, ckCurrentSrch, prvSrchText, chkDefinition);
            string intpkey = "";
            if (txtGenSearch != "" && AccountPkey > 0)
            {
                int IntFound = 0;
                if (dt.Rows.Count > 0)
                    IntFound = 1;

                intpkey = dba.SaveSearch(txtGenSearch, IntFound, AccountPkey, clsUtility.TYPE_IcfGlossary);
            }
            if (AccountPkey > 0)
            {
                var ds = dba.FillGlossaryDropdown(AccountPkey, clsUtility.TYPE_IcfGlossary);
                foreach (DataRow dr in ds.Rows)
                {
                    ddDoclist.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pkey"].ToString() });
                }
            }
            return Json(new { msg = "OK", intpkey = intpkey, dropdownvalues = ddDoclist, Source = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Milestones

        public ActionResult Milestones()
        {
            CheckLoginType();
            var count = dba.MileStoneCount();
            ViewBag.MileStoneCount = string.Format("{0:#,###0}", count);
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            return View();

        }
        public ActionResult RefreshMileStone(string txtGenSearch, bool ckCurrentSrch, string ddSelectedValue , string previousSearch)
        {
            string ID = txtGenSearch;
            string prvSrchText = previousSearch;
            var dt = dba.RefreshMileStone(ID, Convert.ToInt32(ddSelectedValue), ckCurrentSrch, prvSrchText);
            return Json(new { msg = "OK", Source = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);


        }
        #endregion

        #region MagiNewsList

        public ActionResult MagiNewsList()
        {
            CheckLoginType();
            ViewBag.ReturnUrl = (Request.UrlReferrer != null) ? Request.UrlReferrer.PathAndQuery : "/Home/Index";
            var dt = dba.BindNewsTopic();
            List<SelectListItem> ddtopics = new List<SelectListItem>();
            foreach (DataRow dr in dt.Rows)
            {
                ddtopics.Add(new SelectListItem { Text = dr["Name"].ToString(), Value = dr["pkey"].ToString() });
            }
            ViewBag.Topics = ddtopics;

            return View();
        }

        public ActionResult RefreshMagiNews(string txtGenSearch, string txtAuthors, string txtPublication, string ddTopic, string ddYear, string strAccountPkey)
        {
            int intAccount_PKey = 0;
            if (strAccountPkey != "")
            {
                intAccount_PKey = Convert.ToInt32(strAccountPkey);
            }
            DateTime now = DateTime.Now;
            int h = now.Hour;
            int m = now.Minute;
            int s = now.Second;
            int Ml = now.Millisecond;
            DateTime Dates = DateTime.Now.Date;

            string startTime = DateTime.Now.ToString();
            string endTime = DateTime.Now.ToString();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();


            var dt = dba.RefreshMagiNews(txtGenSearch, txtAuthors, txtPublication, Convert.ToInt32(ddTopic), Convert.ToInt32(ddYear), intAccount_PKey);

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            if (txtGenSearch != "")
            {
                startTime = now.ToString("MM/dd/yyyy HH:mm:ss.fff");
                endTime = String.Format("{0:00}/{1:00}/{2:0000} {3:00}:{4:00}:{5:00}.{6:000}", Dates.Month, Dates.Day, Dates.Year, ts.Hours + h, ts.Minutes + m, ts.Seconds + s, ts.Milliseconds / 10 + Ml);
                dba.updateFdaGcpSearch(startTime, endTime);
            }
            return Json(new { msg = "OK", Source = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
        }


        #endregion

        public ActionResult SearchBar(string Title, string Topic)
        {
            Bind_Topic();
            var news = dba.MAGINews();

            ViewBag.PageTitle = "MAGI News";
            ViewBag.MAGINews = news;
            return View("~/Views/Resources/MagiNews.cshtml", news);

        }

        public void Bind_Topic()
        {
            DataTable ds = dba.GetTopics();
            List<SelectListItem> topiclist = new List<SelectListItem>();
            foreach (DataRow dr in ds.Rows)
            {
                topiclist.Add(new SelectListItem { Text = dr["Name"].ToString(), Value = dr["pkey"].ToString() });
            }
            ViewBag.Topic = topiclist;
        }
        [HttpGet]
        public JsonResult SearchByYear(string years_, int? page)
        {

            int pagesize = 30;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;

            IPagedList<Milestone> ms = null;
            var milestoneinfo = dba.Milestone(years_);
            ViewBag.PageTitle = "MAGI's History of Clinical Research Milestones";
            ViewBag.Milestone = milestoneinfo;
            ms = milestoneinfo.ToPagedList(pageIndex, pagesize);
            return Json(ms, JsonRequestBehavior.AllowGet);
            // return View("~/Views/Resources/Milestones2.cshtml", ms);

        }


    }
}
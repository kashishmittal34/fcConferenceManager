
using ClosedXML.Excel;
using Elimar.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using MAGI_API.Models;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Web.Security;
using fcConferenceManager.Models;
using MAGI_API.Security;
using System.Threading.Tasks;
using HandleMultipleButtonInMVC.CustomAttribute;
using System.IO;
using fcConferenceManager.Models.Portolo;
using System.Data.SqlClient;
using OfficeOpenXml;
using OfficeOpenXml.Table;
//using PagedList.Mvc;
//using PagedList;					

namespace fcConferenceManager.Controllers.Portolo
{
    [CheckActiveEventAttribute]
    public class PortoloController : Controller
    {
        DBAccessLayer dba = new DBAccessLayer();
        static SqlOperation repository = new SqlOperation();

        bool view;
        bool add;
        bool edit;
        bool delete;
        int account;
        public PortoloController()
        {
            loginResponse objlt = (loginResponse)System.Web.HttpContext.Current.Session["User"];

            if (objlt != null)
            {
                string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;

                string query = $"select * from SecurityGroup_Members sm join Privilage_listForPortolo pl on pl.SecurityGroupPkey = sm.SecurityGroup_pKey where sm.Account_pKey = {objlt.Id} and pl.PrivilageID = 'TaskList';";

                using (SqlConnection con = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            account = int.Parse(reader["Account_pkey"].ToString());
                            view = view || bool.Parse(reader["AllowView"].ToString());
                            add = add || bool.Parse(reader["AllowAdd"].ToString());
                            edit = edit || bool.Parse(reader["AllowEdit"].ToString());
                            delete = delete || bool.Parse(reader["AllowDelete"].ToString());
                        }
                        reader.Close();
                        con.Close();
                    }
                }
            }
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
        // GET: Portolo



        // Category List 
        private List<SelectListItem> TaskCategoriesList()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            DataTable dt = new DataTable();
            dt = dba.TaskCategories_List();

            foreach (DataRow dr in dt.Rows)
            {

                SelectListItem selectListItem = new SelectListItem() { Value = dr[0].ToString(), Text = dr[1].ToString() };
                selectListItems.Add(selectListItem);
            }
            return selectListItems;
        }

        // Task Status 
        private List<SelectListItem> TaskStatuses_Select_All()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            DataTable dt = new DataTable();
            dt = dba.TaskStatuses_Select_All();
            foreach (DataRow dr in dt.Rows)
            {
                SelectListItem selectListItem = new SelectListItem() { Value = dr[0].ToString(), Text = dr[1].ToString() };
                selectListItems.Add(selectListItem);
            }
            return selectListItems;
        }

        // Task Repeate
        private List<SelectListItem> TaskRepeat_Select_ALL()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            DataTable dt = new DataTable();
            dt = dba.TaskRepeat_Select_ALL();
            foreach (DataRow dr in dt.Rows)
            {
                SelectListItem selectListItem = new SelectListItem() { Value = dr[0].ToString(), Text = dr[1].ToString() };
                selectListItems.Add(selectListItem);
            }
            return selectListItems;
        }
        private List<SelectListItem> PortoloTaskRepeat_Select_ALL()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            DataTable dt = new DataTable();
            dt = dba.PortoloTaskRepeat_Select_ALL();
            foreach (DataRow dr in dt.Rows)
            {
                SelectListItem selectListItem = new SelectListItem() { Value = dr[0].ToString(), Text = dr[1].ToString() };
                selectListItems.Add(selectListItem);
            }
            return selectListItems;
        }



        public ActionResult TaskList(List<TaskListResponse> taskListResponse)
        {
            if (Session["User"] == null || !view)
            {

                return Redirect("~/Account/Portolo");
            }
            else
            {
                TempData.Keep("taskListResponse");
                TempData.Keep("Temprequest");
                if (TempData.Count == 0)
                {
                    TaskListRequest taskListRequest1 = new TaskListRequest();
                    TaskListfilter(taskListRequest1);
                }

                bool reload = false;
                if (TempData["Temprequest"] != null)
                {
                    reload = true;
                }
                if (TempData.Count > 0 && reload == true)
                {
                    TempData.Keep("request");
                    ViewBag.Request = (TaskListRequest)TempData["request"];
                }
                else
                {
                    //TempData.Keep("request");
                    TaskListRequest taskListRequest1 = new TaskListRequest();
                    ViewBag.Request = taskListRequest1;

                }
                //List<Response> response = (List<TaskListResponse>)TempData["taskListResponse"];


                ViewBag.BindList = (List<TaskListResponse>)TempData["taskListResponse"];//TempData["taskListResponse"];
                TaskListRequest taskListRequest = new TaskListRequest();
                ViewBag.TaskList = TaskCategoriesList();
                ViewBag.TaskStatus = TaskStatuses_Select_All();
                ViewBag.RepeatTask = TaskRepeat_Select_ALL();
                ViewBag.PortoloRepeatTask = PortoloTaskRepeat_Select_ALL();
                TempData["EdittaskListResponse"] = null;
                return View("../Portolo/Task/TaskList", taskListRequest);
            }


        }

        [HttpPost]
        [AllowMultipleButton(Name = "action", Argument = "Search")]
        public ActionResult TaskListfilter(TaskListRequest request)
        {

            if (Session["User"] == null)
            {
                Redirect("~/Account/Portolo");
            }
            TaskListRequest obj = new TaskListRequest();
            request.planType = request.intPortoloRepeat;

            if (request.active == null && request.intcategory == 0 && request.commondropdownlist == null && request.duedate == null &&
               request.editprimarykey == 0 && request.forecast == null && request.number == null && request.pKey == 0 &&
               request.planType == 0 && request.intRepeat == 0 && request.reviewed == null && request.status == 0 &&
             request.tasklistrange == null && request.title == null)
            {
                request.active = null;
                request.category = "0";
                request.commondropdownlist = null;
                request.duedate = null;
                request.editprimarykey = 0;
                request.forecast = null;
                request.number = null;
                request.pKey = 0;
                request.plandates = "0";
                request.repeat = "0";
                request.intRepeat = 0;
                request.reviewed = null;
                request.status = 0;
                request.tasklistrange = "0";
                request.title = null;
                request.planType = 0;
                request.intcategory = 0;
                request.intstatus = 0;
            }
            List<TaskListResponse> MainList = new List<TaskListResponse>();
            DataTable dt = new DataTable();
            dt = dba.TaskList_Select_All1(request);

            foreach (DataRow dr in dt.Rows)
            {
                TaskListResponse taskList = new TaskListResponse();

                taskList.title = dr["Title"].ToString();
                taskList.description = dr["Description"].ToString();
                taskList.status = int.Parse(dr["Status"].ToString());
                taskList.TaskCategory_pKey = int.Parse(dr["TaskCategory_pKey"].ToString());
                taskList.RepeatType_pKey = int.Parse(dr["RepeatType_pKey"].ToString());
                taskList.TaskCategoryID = dr["TaskCategoryID"].ToString();
                taskList.TaskStatusID = dr["TaskStatusID"].ToString();
                taskList.TaskRepeatID = dr["TaskRepeatID"].ToString();
                taskList.number = dr["Number"].ToString();
                taskList.mostrecentnote = dr["MostRecentNote"].ToString();
                taskList.name = dr["Name"].ToString();
                taskList.pKey = dr["pKey"].ToString();
                taskList.active = Boolean.Parse(dr["Active"].ToString());
                taskList.reviewed = Boolean.Parse(dr["Reviewed"].ToString());
                taskList.intcategory = Convert.ToInt32(dr["Category"].ToString());
                taskList.Tips = dr["Tips"].ToString();
                taskList.Instruction = dr["Instruction"].ToString();
                taskList.Notes = dr["Notes"].ToString();
                taskList.Resources = dr["Resources"].ToString();
                taskList.ResourcesFileName = dr["ResourcesFileName"].ToString();

                if (dr["PlanDate"].ToString() != null && dr["PlanDate"].ToString() != "")
                {
                    taskList.plan = Convert.ToDateTime(dr["PlanDate"].ToString());
                }
                if (dr["DueDate"].ToString() != null && dr["DueDate"].ToString() != "")
                {
                    taskList.duedate = Convert.ToDateTime(dr["DueDate"].ToString());
                }
                if (dr["Forecast"].ToString() != null && dr["Forecast"].ToString() != "")
                {
                    taskList.forecast = Convert.ToDateTime(dr["Forecast"].ToString());
                }

                //List<TaskListRequest> BList = TaskListAsync(request);
                MainList.Add(taskList);

                //ViewBag.BindList = dt;


            }
            ViewBag.BindList = MainList;
            ViewBag.TaskList = TaskCategoriesList();
            ViewBag.TaskStatus = TaskStatuses_Select_All();
            ViewBag.RepeatTask = TaskRepeat_Select_ALL();
            ViewBag.PortoloRepeatTask = PortoloTaskRepeat_Select_ALL();

            ViewBag.Request = obj;
            ViewBag.Baseurl = ConfigurationManager.AppSettings["AppURL"];
            TempData["taskListResponse"] = MainList;

            TempData["request"] = request;

            TempData["Temprequest"] = true;
            //return View();
            return Redirect("../Portolo/TaskList");
        }


        public ActionResult CreateTask(TaskAdd model)
        {
            TempData.Keep("EdittaskListResponse");

            TaskListResponse taskListResponse = new TaskListResponse();
            var tempData = (List<TaskListResponse>)TempData["EdittaskListResponse"];//TempData["taskListResponse"]; 
            if (tempData != null && tempData.Count > 0)
            {
                taskListResponse.title = tempData[0].title.ToString();
                taskListResponse.description = tempData[0].description.ToString();
                taskListResponse.TaskCategory_pKey = tempData[0].TaskCategory_pKey;
                taskListResponse.TaskStatusID = tempData[0].TaskStatusID.ToString();
                taskListResponse.active = tempData[0].active;
                taskListResponse.reviewed = tempData[0].reviewed;
                taskListResponse.plan = tempData[0].plan;
                taskListResponse.forecast = tempData[0].forecast;
                taskListResponse.duedate = tempData[0].duedate;
                taskListResponse.RepeatType_pKey = tempData[0].RepeatType_pKey;
                taskListResponse.status = tempData[0].status;
                taskListResponse.pKey = tempData[0].pKey;
            }
            else
            {
                taskListResponse.title = "";
                taskListResponse.description = "";
                //taskListResponse.TaskCategory_pKey = 0;
                //taskListResponse.TaskStatusID = "0";
                //taskListResponse.active = true;
                //taskListResponse.reviewed = false;

                //taskListResponse.forecast = "";
                //taskListResponse.duedate = "";
                //taskListResponse.RepeatType_pKey = 0;
                //taskListResponse.status = 0;
                //taskListResponse.pKey = 0;

            }
            model.title = "";
            model.taskListResponse = taskListResponse;
            ViewBag.Title = "Create New";
            return View();

        }

        //[HttpPost]
        //[Route("James/Del")]
        //[AjaxValidateAntiForgeryToken]
        //[ValidateInput(true)]
        //public ActionResult Delete()
        //{


        //    return RedirectToAction("~/Portolo/TaskList");
        //}

        [HttpPost]
        [AllowAnonymous]
        [AllowMultipleButton(Name = "action", Argument = "CopyTask")]
        public ActionResult CopyTask(FormCollection coll)
        {
            string msg = "Select any task and try again";
            if (coll["ID"] != null)
            {


                try

                {
                    string strpkeys = "0";
                    int i = 0;
                    string[] ids = coll["ID"].Split(new char[] { ',' });
                    foreach (string id in ids)
                    {
                        if (i <= 0)
                        {
                            strpkeys = id.ToString();
                        }
                        else
                        {
                            strpkeys = strpkeys + id.ToString() + ",";
                        }
                    }
                    msg = dba.CopyTask(strpkeys);
                    commonreload();


                }


                catch (Exception)
                {

                }
            }
            TempData["Message"] = msg;
            return Redirect("~/Portolo/TaskList");
        }


        [HttpPost]
        [AllowAnonymous]
        [AllowMultipleButton(Name = "action", Argument = "Delete")]
        public ActionResult Delete(FormCollection coll)
        {
            string msg = "Select any task and try again";
            if (coll["ID"] != null)
            {
                try



                {
                    string strpkeys = "0";
                    int i = 0;
                    string[] ids = coll["ID"].Split(new char[] { ',' });
                    foreach (string id in ids)

                    {
                        if (i <= 0)
                        {
                            strpkeys = id.ToString();
                        }
                        else
                        {
                            strpkeys = strpkeys + id.ToString() + ",";
                        }
                    }
                    msg = dba.Tasks_Detele(strpkeys);
                    commonreload();
                }






                catch (Exception)
                {

                }
            }
            TempData["Message"] = msg;
            return Redirect("~/Portolo/TaskList");

        }

        public ActionResult Topics()
        {
            if (Session["User"] == null)
            {
                return Redirect("~/Account/Portolo");
            }
            SqlConnection con = new SqlConnection(ReadConnectionString());
            DataTable dt = new DataTable();

            string dbquery = " select TopicID, Title, Description, IsActive from Portolo_Topics";
            con.Open();
            SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
            _da.Fill(dt);
            con.Close();

            ViewBag.Topics = dt;

            return View();
        }

        public ActionResult AddTopic(Topic model)
        {
            if (Session["User"] == null) return Redirect("~/Account/Portolo");
            if (model.title == null)
                return View();

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);

            string query = "";

            if (model.id == 0)
            {
                query = String.Format("INSERT INTO Portolo_Topics VALUES ('{0}', '{1}', '{2}')", model.title, model.description, model.isActive);
            }
            else
                query = String.Format("Update Portolo_Topics Set Title = '{0}', Description = '{1}', isActive = '{2}' where TopicID = {3}", model.title, model.description, model.isActive, model.id);

            SqlCommand cmd = new SqlCommand(query, conn);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            return RedirectToAction("Topics", "Portolo");
        }

        public ActionResult EditTopic(string id)
        {
            if (Session["User"] == null) return Redirect("~/Account/Portolo");
            SqlConnection con = new SqlConnection(ReadConnectionString());

            string dbquery = " select * from Portolo_Topics where TopicID = " + id;
            con.Open();

            SqlCommand cmd = new SqlCommand(dbquery, con);
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            Topic topic = new Topic();
            topic.title = reader["Title"].ToString();
            topic.description = reader["Description"].ToString();
            topic.id = (int)Convert.ToInt64(id);
            topic.isActive = (bool)reader["IsActive"];
            con.Close();

            return View("~/Views/Portolo/AddTopic.cshtml", topic);
        }

        [HttpPost]
        public ActionResult DeleteTopic(string ids)
        {
            if (Session["User"] == null) return Redirect("~/Account/Portolo");
            SqlConnection con = new SqlConnection(ReadConnectionString());

            string dbquery = String.Format("delete from Portolo_Topics where TopicID in ({0}) ", ids);
            con.Open();

            SqlCommand cmd = new SqlCommand(dbquery, con);

            cmd.ExecuteNonQuery();
            con.Close();

            return RedirectToAction("Topics", "Portolo");
        }

        [CustomizedAuthorize]
        public ActionResult UserTopics()
        {
            if (Session["User"] == null)
            {
                Redirect("~/Account/Portolo");
            }
            int Id = ((loginResponse)Session["User"]).Id;
            DataTable dt = new DataTable();

            SqlConnection con = new SqlConnection(ReadConnectionString());

            string dbquery = String.Format("Select * from Portolo_Topics where IsActive = 1");
            string selected = String.Format("Select Topic_id from Portolo_UserTopics where Registration_id = {0}", Id);
            SqlCommand cmd = new SqlCommand(selected, con);
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            List<string> ids = new List<string>();

            while (reader.Read())
            {
                ids.Add(reader["Topic_id"].ToString());
            }

            SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
            _da.Fill(dt);
            con.Close();

            ViewBag.Topics = dt;
            ViewBag.Ids = ids;

            return View();
        }

        [HttpGet]
        public ActionResult SearchTopic(string name, string description, string active, string excel)
        {
            SqlConnection con = new SqlConnection(ReadConnectionString());
            DataTable dt = new DataTable();

            string dbquery = String.Format("select TopicID, Title, Description, IsActive from Portolo_Topics where Title like '%{0}%' and Description like '%{1}%'", name.Trim(), description.Trim());
            if (active != "")
                dbquery += String.Format("and IsActive = '{0}'", active);
            con.Open();
            SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
            _da.Fill(dt);
            con.Close();

            ViewBag.Topics = dt;
            ViewBag.IsActive = active;

            if (excel == "true")
            {
                string FileName = String.Format("Topics_{0:yyMMdd_HH.mm}", DateTime.Now);
                ExportToExcel(dt, FileName);
            }

            return View("~/Views/Portolo/Topics.cshtml");

        }

        [HttpGet]

        public ActionResult UserSearchTopic(string name, string description)
        {
            if (Session["User"] == null)
            {
                Redirect("~/Account/Portolo");
            }
            int Id = ((loginResponse)Session["User"]).Id;
            SqlConnection con = new SqlConnection(ReadConnectionString());
            DataTable dt = new DataTable();

            string dbquery = String.Format("select TopicID, Title, Description from Portolo_Topics where Title like '%{0}%' and Description like '%{1}%' and IsActive = 1", name.Trim(), description.Trim());
            string selected = String.Format("Select Topic_id from Portolo_UserTopics where Registration_id = {0}", Id);
            SqlCommand cmd = new SqlCommand(selected, con);
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            List<string> ids = new List<string>();

            while (reader.Read())
            {
                ids.Add(reader["Topic_id"].ToString());
            }
            SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
            _da.Fill(dt);
            con.Close();

            ViewBag.Topics = dt;
            ViewBag.Ids = ids;

            return View("~/Views/Portolo/UserTopics.cshtml");

        }

        public ActionResult SaveUserTopic(string[] ids, string selectall = "")
        {
            if (Session["User"] == null)
            {
                Redirect("~/Account/Portolo");
            }
            int userId = ((loginResponse)Session["User"]).Id;
            SqlConnection con = new SqlConnection(ReadConnectionString());

            con.Open();
            string dbquery = "";

            if (selectall == "")
            {
                dbquery = String.Format(@"If NOT EXISTS (SELECT * FROM Portolo_UserTopics where Registration_id = {0} and Topic_id = {1} ) Insert into Portolo_UserTopics values ({0}, {1}) Else Delete from Portolo_UserTopics where Registration_id = {0} and Topic_id = {1}", userId, ids[0]);
                SqlCommand cmd = new SqlCommand(dbquery, con);
                cmd.ExecuteNonQuery();
            }

            else if (selectall == "check")
            {
                foreach (var id in ids)
                {
                    dbquery = String.Format(@"If NOT EXISTS (SELECT * FROM Portolo_UserTopics where Registration_id = {0} and Topic_id = {1} ) Insert into Portolo_UserTopics values ({0}, {1})", userId, id);

                    SqlCommand cmd = new SqlCommand(dbquery, con);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                foreach (var id in ids)
                {
                    dbquery = String.Format("If EXISTS (SELECT * FROM Portolo_UserTopics where Registration_id = {0} and Topic_id = {1} ) Delete from Portolo_UserTopics where Registration_id = {0} and Topic_id = {1}", userId, id);
                    SqlCommand cmd = new SqlCommand(dbquery, con);
                    cmd.ExecuteNonQuery();
                }
            }


            con.Close();
            return View();
        }

        public void TopicsDownloadExcel()
        {
            SqlConnection con = new SqlConnection(ReadConnectionString());
            DataTable dt = new DataTable();

            dt.Clear();

            string dbquery = "select TopicID, IsActive, Title as 'Topic Name', Description from Portolo_Topics";
            con.Open();
            SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
            _da.Fill(dt);
            con.Close();

            string FileName = String.Format("Topics_{0:yyMMdd_HH.mm}", DateTime.Now);
            ExportToExcel(dt, FileName);
        }
        [HttpPost]
        [AllowAnonymous]
        [AllowMultipleButton(Name = "action", Argument = "Reset")]
        public ActionResult commonreload()
        {

            if (Session["User"] == null)
            {
                Redirect("~/Account/Portolo");
            }
            TaskListRequest taskListRequest = new TaskListRequest();
            taskListRequest.active = null;
            taskListRequest.category = "0";
            taskListRequest.commondropdownlist = null;
            taskListRequest.duedate = null;
            taskListRequest.editprimarykey = 0;
            taskListRequest.forecast = null;
            taskListRequest.number = null;
            taskListRequest.pKey = 0;
            taskListRequest.plandates = "0";
            taskListRequest.repeat = "0";
            taskListRequest.intRepeat = 0;
            taskListRequest.reviewed = null;
            taskListRequest.status = 0;
            taskListRequest.tasklistrange = "0";
            taskListRequest.title = null;
            taskListRequest.planType = 0;
            taskListRequest.intcategory = 0;
            taskListRequest.intstatus = 0;

            TaskListfilter(taskListRequest);


            return View("~/Views/Portolo/Task/TaskList.cshtml", taskListRequest);
        }
        public ActionResult MyFiles(MyFileUpload myMyFileUpload, string search)
        {

            List<MyFileUpload> uploadlist = GetFileDetails();












            myMyFileUpload.FileList = uploadlist;

            string apppath = GetBaseUrl();
            ViewBag.apppath = apppath;
















            if (!string.IsNullOrEmpty(search))
            {
                var searchlist = (from list in uploadlist where list.FileName.StartsWith(search.Trim(), StringComparison.OrdinalIgnoreCase) select list).ToList();
                return View(searchlist);
            }


            return View(myMyFileUpload.FileList);
        }
        [HttpPost]

        public ActionResult MyFiles(HttpPostedFileBase files)
        {
            MyFileUpload model = new MyFileUpload();
            List<MyFileUpload> list = GetFileDetails();










            model.FileList = list;

            if (files != null)
            {
                var Extension = Path.GetExtension(files.FileName);
                var ResourcefileName = "my-file-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Extension;
                var filename = Path.GetFileName(files.FileName);
                string path = Path.Combine(Server.MapPath("~/PortoloDocuments/"), ResourcefileName);
                model.FileUrl = Url.Content(Path.Combine("~/PortoloDocuments/", ResourcefileName));
                model.FileName = filename;

                if (SaveFile(model))
                {
                    files.SaveAs(path);
                    TempData["AlertMessage"] = "Uploaded Successfully !!";
                    return RedirectToAction("MyFiles", "Portolo");
                }
                else
                {
                    ModelState.AddModelError("", "Error In Add File. Please Try Again !!!");
                }
            }
            else
            {
                ModelState.AddModelError("", "Please Choose Correct File Type !!");
                return View(model.FileList);
            }
            return RedirectToAction("MyFiles", "Portolo");
        }

        public ActionResult DeleteFile(int pkey)
        {
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand("DeleteFile", con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@pKey", pkey);


                    cmd.ExecuteNonQuery();
                    con.Close();

                }
            }

            return RedirectToAction("MyFiles", "Portolo");
        }
        private bool SaveFile(MyFileUpload model)
        {
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            string strQry = "INSERT INTO tblFileDetails (Resources,ResourcesFilePath) VALUES('" +
                model.FileName + "','" + model.FileUrl + "')";
            SqlConnection con = new SqlConnection(config);
            con.Open();
            SqlCommand command = new SqlCommand(strQry, con);
            int numResult = command.ExecuteNonQuery();
            con.Close();
            if (numResult > 0)
                return true;
            else
                return false;
        }
        private List<MyFileUpload> GetFileDetails()
        {
            List<MyFileUpload> uploadlist = new List<MyFileUpload>();
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            DataTable dtData = new DataTable();
            SqlConnection con = new SqlConnection(config);
            con.Open();
            SqlCommand command = new SqlCommand("getuploadfiles", con);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dtData);
            con.Close();
            foreach (DataRow dr in dtData.Rows)
            {
                uploadlist.Add(new MyFileUpload
                {
                    FileId = Convert.ToInt32(@dr["pkey"]),
                    FileName = @dr["Resources"].ToString(),
                    FileUrl = @dr["ResourcesFilePath"].ToString()

                });

            }
            return uploadlist;
        }
        public JsonResult GetFilePath(int? id)
        {
            var customer = GetFileDetails().Find(x => x.FileId.Equals(id));
            return Json(customer, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ProcessLibrary(ProcessLibrary library, string search)
        {

            List<ProcessLibrary> uploadlist = GetProcessDetails();

            library.processList = uploadlist;
            if (!string.IsNullOrEmpty(search))
            {
                ViewBag.search = search;
                var searchlist = (from list in uploadlist where list.Process.StartsWith(search.Trim(), StringComparison.OrdinalIgnoreCase) select list).ToList();
                Session["searchlist"] = searchlist;
                return View(searchlist);
            }

            library.processList = uploadlist;
            return View(library.processList);
        }

        [HttpPost]

        public ActionResult ProcessLibrary(string Process)
        {
            ProcessLibrary model = new ProcessLibrary();
            List<ProcessLibrary> librarylist = GetProcessDetails();

            model.processList = librarylist;

            if (Process != null)
            {

                model.Process = Process;
                if (SaveProcess(model))
                {

                    ViewBag.Message = "Added Successfully !!";
                    return RedirectToAction("ProcessLibrary", "Portolo");
                }
                else
                {
                    ModelState.AddModelError("", "Error In Add File. Please Try Again !!!");
                }

            }
            else
            {
                ModelState.AddModelError("", "Please Choose Correct File Type !!");
                return View(model.processList);
            }
            return RedirectToAction("ProcessLibrary", "Portolo");
        }

        private bool SaveProcess(ProcessLibrary model)
        {
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            SqlConnection con = new SqlConnection(config);
            con.Open();
            SqlCommand command = new SqlCommand("ProcessLibrary", con);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@pkey", "");
            command.Parameters.AddWithValue("@Process", model.Process.Trim());
            command.Parameters.AddWithValue("@status", "Insert");
            int numResult = command.ExecuteNonQuery();
            con.Close();
            if (numResult > 0)
                return true;
            else
                return false;
        }
        private List<ProcessLibrary> GetProcessDetails()
        {
            List<ProcessLibrary> librarylist = new List<ProcessLibrary>();
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            DataTable dtData = new DataTable();
            string query = "Select * from Sys_PortoloProcess";
            SqlConnection con = new SqlConnection(config);
            con.Open();
            SqlCommand command = new SqlCommand(query, con);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dtData);
            con.Close();
            foreach (DataRow dr in dtData.Rows)
            {
                librarylist.Add(new ProcessLibrary
                {
                    pkey = Convert.ToInt32(@dr["pkey"]),
                    Process = @dr["ProcessId"].ToString()
                });

            }
            return librarylist;
        }

        public ActionResult DeleteProcess(int pkey)
        {

            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand("ProcessLibrary", con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@pkey", pkey);
                    cmd.Parameters.AddWithValue("@Process", "");
                    cmd.Parameters.AddWithValue("@status", "Delete");
                    int result = cmd.ExecuteNonQuery();
                    con.Close();
                    if (result == 1)
                    {
                        ViewBag.Message = "Process Deleted Successfully";
                        ModelState.Clear();
                    }
                    else
                    {
                        ViewBag.Message = "Unsucessfull";
                        ModelState.Clear();
                    }

               
                    return Redirect(Request.UrlReferrer.ToString());
                }
            }


        }
        public JsonResult EditProcess(int? id)
        {
            var customer = GetProcessDetails().Find(x => x.pkey.Equals(id));
            return Json(customer, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdateProcess(ProcessLibrary library)
        {

            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand("ProcessLibrary", con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@pkey", library.pkey);
                    cmd.Parameters.AddWithValue("@Process", library.Process.Trim());
                    cmd.Parameters.AddWithValue("@status", "Update");
                    int result = cmd.ExecuteNonQuery();
                    if (result == 1)
                    {
                        ViewBag.Message = "Process Updated Successfully";
                        ModelState.Clear();
                    }
                    else
                    {
                        ViewBag.Message = "Unsucessfull";
                        ModelState.Clear();
                    }
                    con.Close();

                   
                    return Redirect(Request.UrlReferrer.ToString());




                }
            }

        }
        public ActionResult DownloadFile(string filePath)
        {

            string fullName = Server.MapPath("~" + filePath);

            var fileName = Path.GetFileName(fullName);
            byte[] fileBytes = GetFile(fullName);
            return File(
                fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        byte[] GetFile(string s)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(s);

            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
                throw new System.IO.IOException(s);
            return data;
        }
        #region ExportExcel
        public byte[] ExporttoExcel(DataTable table, string filename)
        {

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage pack = new ExcelPackage();
            ExcelWorksheet ws = pack.Workbook.Worksheets.Add(filename);
            ws.Cells["A1"].LoadFromDataTable(table, true, TableStyles.Custom);
            return pack.GetAsByteArray();

        }
        public ActionResult Download()
        {
            string reportname = $"ProcessLibrary_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";
            var list = GetProcessDetails();
            if (Session["searchlist"]!=null)
            {
                list = (List<ProcessLibrary>)Session["searchlist"];

            }
            DataTable dt = new DataTable();
            dt.Clear();
            dt.Columns.Add("S.No");
            dt.Columns.Add("Process Name");
            
             
           
            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    DataRow dataRow = dt.NewRow();
                    dataRow["S.No"] = item.pkey;
                    dataRow["Process Name"] = item.Process;
                    dt.Rows.Add(dataRow);
                }
                var exportbytes = ExporttoExcel(dt, reportname);
                return File(exportbytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportname);
            }
            else
            {
                TempData["Message"] = "No Data to Export";
                return RedirectToAction("ProcessLibrary");
            }

        }
        #endregion					   

        public void DownloadExcel()
        {
            DataTable dt = new DataTable();
            dt.Clear();
            dt.Columns.Add("Number");
            dt.Columns.Add("Name");
            dt.Columns.Add("Title");
            dt.Columns.Add("Description");
            dt.Columns.Add("plan");
            dt.Columns.Add("Due Date");
            dt.Columns.Add("Forecast");
            dt.Columns.Add("Category");
            dt.Columns.Add("Status");
            dt.Columns.Add("Most Recent Notes");

            string FileName = String.Format("PublicTasks_{0:yyMMdd_HH.mm}", DateTime.Now);
            TempData.Keep("taskListResponse");
            TaskListResponse taskListResponse = new TaskListResponse();
            var tempData = (List<TaskListResponse>)TempData["taskListResponse"];//TempData["taskListResponse"]; 

            if (tempData != null)
            {
                foreach (var item in tempData)
                {
                    DataRow exceldata = dt.NewRow();
                    exceldata["Number"] = item.number;
                    exceldata["Name"] = item.name;
                    exceldata["Title"] = item.title;
                    exceldata["Description"] = item.description;
                    exceldata["plan"] = item.plan;
                    exceldata["Due Date"] = item.duedate;
                    exceldata["Forecast"] = item.forecast;
                    exceldata["Category"] = item.TaskCategoryID;
                    exceldata["Status"] = item.TaskStatusID;
                    exceldata["Most Recent Notes"] = item.mostrecentnote;
                    dt.Rows.Add(exceldata);
                }
            }
            ExportToExcel(dt, FileName);

        }
        private void ExportToExcel(DataTable dt, string fileName)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt, "Task_List");
                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=" + fileName.ToString() + ".xlsx");
                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }
        public string GetBaseUrl()
        {
            var request = HttpContext.Request;
            var appUrl = HttpRuntime.AppDomainAppVirtualPath;
            if (appUrl != "/")
            {
                appUrl = "/" + appUrl;

            }
            var baseUrl = string.Format("{0}:{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);
            baseUrl = baseUrl.TrimEnd('/');
            return baseUrl;
        }
    }
}






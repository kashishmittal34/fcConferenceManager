﻿using ClosedXML.Excel;
using Elimar.Models;
using ExcelDataReader;
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
		SqlConnection con = new SqlConnection(ReadConnectionString());
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

				string query = $"select * from Account_List where GlobalAdministrator = 1 and pKey = {objlt.Id};";

                using (SqlConnection con = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            view = true;
                            edit = true;
                            add = true;
                            delete = true;
                            return;
                        }
                        reader.Close();
                        con.Close();
                    }
                }																								  
                 query = $"select * from SecurityGroup_Members sm join Privilage_listForPortolo pl on pl.SecurityGroupPkey = sm.SecurityGroup_pKey where sm.Account_pKey = {objlt.Id} and pl.PrivilageID = 'TaskList';";

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
            dt = TaskCategories_List();

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
            dt = TaskStatuses_Select_All1();
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
            dt = TaskRepeat_Select_ALL2();
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
            dt = PortoloTaskRepeat_Select_ALL1();
            foreach (DataRow dr in dt.Rows)
            {
                SelectListItem selectListItem = new SelectListItem() { Value = dr[0].ToString(), Text = dr[1].ToString() };
                selectListItems.Add(selectListItem);
            }
            return selectListItems;
        }
														   


        public ActionResult TaskList(List<TaskListResponse> taskListResponse)
        {
			ViewBag.notice = TempData["notice"];									
            if (Session["User"] == null)
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
                    TaskListRequest taskListRequest1 =new TaskListRequest();
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
            dt = TaskList_Select_All1(request);

			if (dt.Rows.Count == 0)
            {

                TempData["notice"] = "No Records Found";
               
            }
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
                     msg = CopyTask(strpkeys);
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
                        msg = Tasks_Detele(strpkeys);
                    }
                   
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

            string dbquery = " select TopicID, Title as 'Topic Name', Description, Cast(Case When IsActive=1 Then 'Active' Else 'Inactive' END AS varchar(10)) as Status from Portolo_Topics";
            con.Open();
            SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
            _da.Fill(dt);
            con.Close();
            ViewBag.InvalidExcel = TempData["InvalidExcel"];
            ViewBag.Topics = dt;

            return View();
        }
	[HttpPost]
       
    public ActionResult ImportExcel(HttpPostedFileBase files)
    {
        if(ModelState.IsValid)
        {
            if(files != null && files.ContentLength >  0)
            {
                Stream stream = files.InputStream;
                IExcelDataReader reader = null;
                if(files.FileName.EndsWith(".xls"))
                {
                    string filename = files.FileName;
                    string path = Server.MapPath("~/PortoloDocuments/" + filename);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                    files.SaveAs(path);

                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else if(files.FileName.EndsWith(".xlsx"))
                {
                    string filename = files.FileName;
                    string path = Server.MapPath("~/PortoloDocuments/" + filename);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                    files.SaveAs(path);
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }
                else
                {
                    ModelState.AddModelError("File", "This file format is not supported");
                    return RedirectToAction("Topics","Portolo");
                }
                int fieldCount = reader.FieldCount;
                int rowCount = reader.RowCount;
                DataTable dt = new DataTable();
                DataRow row;
                DataTable dt_ = new DataTable();
                DataSet ds = new DataSet();
                ds = reader.AsDataSet();
                try
                {
                 
                    dt_ = ds.Tables[0];
                        
                    for(int i=0; i<dt_.Columns.Count; i++)
                    {
                        dt.Columns.Add(dt_.Rows[0][i].ToString());
                    }
                    int rowCounter = 0;
                    for(int rows = 1; rows < dt_.Rows.Count; rows++)
                    {
                        row = dt.NewRow();
                        for(int col = 0; col<dt_.Columns.Count; col++)
                        {
                            row[col] = dt_.Rows[rows][col].ToString();
                            rowCounter++;
                        }
                        dt.Rows.Add(row);
						 
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("File", "Unable to upload file");
                    return RedirectToAction("Topics", "Portolo");
                }
                DataSet result = new DataSet();
                result.Tables.Add(dt);
                reader.Close();
                reader.Dispose();
                DataTable dataTable = result.Tables[0];
                    
                for (int i = 0; i < result.Tables[0].Rows.Count; i++)
                {
                    try						   
                    {
                        string numString = result.Tables[0].Rows[i][1].ToString();
                        bool number1 = false;
                        bool canConvert = bool.TryParse(numString, out number1);
                        if (canConvert == true)
                        {
                            string conn = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
                            SqlConnection con = new SqlConnection(conn);
                            string query = "Insert into Portolo_Topics(Title,Description,IsActive) Values('" + result.Tables[0].Rows[i][2].ToString() + "','" + result.Tables[0].Rows[i][3].ToString() + "','" + result.Tables[0].Rows[i][1].ToString() + "')";
                            con.Open();
                            SqlCommand cmd = new SqlCommand(query, con);
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                        else
                        {
                            TempData["InvalidExcel"] = "Invalid! Excel File";
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        TempData["InvalidExcel"] = "Invalid Excel File! Enter data in a Correct Format!!";				  
                    }
                }
                return RedirectToAction("Topics", "Portolo");
            }
            else
            {
                ModelState.AddModelError("File","Please upload your file");
            }
        }
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
        public ActionResult SearchTopic(string name, string description, string active, string search, string ids)
        {
            SqlConnection con = new SqlConnection(ReadConnectionString());
            DataTable dt = new DataTable();

            string dbquery = String.Format("select TopicID, Title as 'Topic Name', Description, Cast(Case When IsActive=1 Then 'Active' Else 'Inactive' END AS varchar(10)) as Status from Portolo_Topics where Title like '%{0}%' and Description like '%{1}%'", name.Trim(), description.Trim());
            if (active != "")
                dbquery += String.Format("and IsActive = '{0}'", active);

            if (ids != "")
                dbquery += String.Format("and TopicID in ({0})", ids);

            con.Open();
            SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
            _da.Fill(dt);
            con.Close();

            ViewBag.Topics = dt;
            ViewBag.IsActive = active;
														 

            if (search != "true")
            { 
                string FileName = String.Format("Topics_{0:yyMMdd_HH.mm}", DateTime.Now);
                ExportToExcel(dt, FileName, "Topic_List");
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
        public ActionResult MyFiles(MyFileUpload myFileUpload, string search)
        {

            List<MyFileUpload> fileslist = GetFileDetails();
            myFileUpload.FileList = fileslist;

            string apppath = GetBaseUrl();
            ViewBag.apppath = apppath;
            if (!string.IsNullOrEmpty(search))
            {
                //var searchlist = (from list in fileslist where list.FileName.Contains(search.Trim()) || list.FileName.StartsWith(search.Trim(), StringComparison.OrdinalIgnoreCase) select list).ToList();
                var searchlist = GetSearchDetails(search.Trim());
                return View(searchlist);
            }
            ViewBag.AlertMessage = TempData["AlertMessage"];

            return View(myFileUpload.FileList);
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
                    TempData["AlertMessage"] = "File is Uploaded!!";
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
            try
            {
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
                TempData["AlertMessage"] = "File is Deleted!!";
            }
            catch (Exception)
            {
                TempData["AlertMessage"] = "Unsuccessfull !!";
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
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@search","");
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
		private List<MyFileUpload> GetSearchDetails(string search)
        {
            List<MyFileUpload> uploadlist = new List<MyFileUpload>();
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            DataTable dtData = new DataTable();
            SqlConnection con = new SqlConnection(config);
            con.Open();
            SqlCommand command = new SqlCommand("getuploadfiles", con);
            command.Parameters.AddWithValue("@search", search);
            command.CommandType = CommandType.StoredProcedure;
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
        public ActionResult GetFilePath(int? id)
        {
            var customer = GetFileDetails().Find(x => x.FileId.Equals(id));
            string fullName = Server.MapPath("~" + customer.FileUrl);
            if (!System.IO.File.Exists(fullName))
            {
                TempData["AlertMessage"] = "This file does not exist!";
                return RedirectToAction("MyFiles");
            }
            return Json(customer, JsonRequestBehavior.AllowGet);
        }
      public ActionResult ProcessLibrary(ProcessLibrary library,string search)
        {

            List<ProcessLibrary> uploadlist = GetProcessDetails();

            library.processList = uploadlist;
           if (!string.IsNullOrEmpty(search))
            {
				ViewBag.search = search;						
                //var searchlist = (from list in uploadlist where list.Process.Contains(search.Trim())|| list.Process.StartsWith(search.Trim(), StringComparison.OrdinalIgnoreCase) select list).ToList();
                var searchlist = GetProcessSearchDetails(search.Trim());
                Session["searchlist"] = searchlist;
                return View(searchlist);
            }			 					
            ViewBag.Message = TempData["Message"];
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

                    TempData["Message"] = "Added new Process !!";
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
        private List<ProcessLibrary> GetProcessSearchDetails(string search)
        {
            List<ProcessLibrary> librarylist = new List<ProcessLibrary>();
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            DataTable dtData = new DataTable();
            string query = "Select * from Sys_PortoloProcess where ProcessId like '%" + search + "%' ";
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
                        TempData["Message"] = "Process Deleted!";
                        ModelState.Clear();
                    }
                    else
                    {
                        TempData["Message"] = "Unsucessfull";
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
                        TempData["Message"] = "Process Updated!";
                        ModelState.Clear();
                    }
                    else
                    {
                        TempData["Message"] = "Unsucessfull";
                        ModelState.Clear();
                    }
                    con.Close();
                   return Redirect(Request.UrlReferrer.ToString());
                }
            }
            
        }
	    public ActionResult Reports(string reportId)
        {
            if ((Session["User"] == null) || !((loginResponse)Session["User"]).IsGlobalAdmin)
                return Redirect("~/Account/Portolo");

            SqlConnection con = new SqlConnection(ReadConnectionString());

            DataTable rtable = new DataTable();
            string query = "select * from Sys_PortoloReport where IsActive = 1";
            SqlDataAdapter da = new SqlDataAdapter(query, con);
            da.Fill(rtable);

            ViewBag.Reports = rtable;

            DataTable ctable = new DataTable();
            string cquery = "select * from Sys_PortoloReportCategory";
            SqlDataAdapter dc = new SqlDataAdapter(cquery, con);
            dc.Fill(ctable);

            ViewBag.category = ctable;

            if (reportId == null) return View("~/Views/Portolo/Report/Report.cshtml");
            
																		  
            DataTable dt = new DataTable();
            string dbquery = "";
            dt.Clear();
            con.Open();
            SqlDataAdapter _db;

            if (reportId == "1")
            {
                dbquery = "select a.Firstname, a.Lastname, a.Email, a.Title, a.Phone , o.OrganizationID from Account_list a inner join Organization_List  o on o.pKey = a.ParentOrganization_pKey where PortoloUser = 1";

                _db = new SqlDataAdapter(dbquery, con);
                _db.Fill(dt);
                con.Close();
                ViewBag.Reports = dt;
                return PartialView("~/Views/Portolo/Report/_UserList.cshtml");
            }

            else if (reportId == "2")
            {
                dbquery = "select * from Portolo_ProductList";
                _db = new SqlDataAdapter(dbquery, con);
                _db.Fill(dt);
                con.Close();
                ViewBag.Reports = dt;
                return PartialView("~/Views/Portolo/Report/_ProductList.cshtml");
            }

            return null;
        }

        [HttpPost]
        public ActionResult SearchReportUser(FormCollection fields)
        {
            SqlConnection con = new SqlConnection(ReadConnectionString());
            DataTable dt = new DataTable();
            SqlDataAdapter _da;
            con.Open();

            ViewBag.name = fields["name"];
            if (fields["reportId"] == "1")
            {
                string dbquery = String.Format(@"select a.Firstname, a.Lastname, a.Email, a.Title, a.Phone , o.OrganizationID from Account_list a inner join Organization_List  o on o.pKey = a.ParentOrganization_pKey where PortoloUser = 1
                            and CONCAT(a.Firstname, ' ', a.Lastname) like '%{0}%' and a.Email like '%{1}%' and a.Title like '%{2}%' and o.OrganizationID like '%{3}%'", fields["name"].Trim(), fields["email"].Trim(), fields["titl"].Trim(), fields["org"].Trim());
                _da = new SqlDataAdapter(dbquery, con);
                _da.Fill(dt);
                con.Close();
                ViewBag.Reports = dt;
                ViewBag.email = fields["email"];
                ViewBag.titl = fields["titl"];
                ViewBag.org = fields["org"];

                return PartialView("~/Views/Portolo/Report/_UserList.cshtml");
            }

            if (fields["reportId"] == "2")
            {
                string dbquery = String.Format("select * from Portolo_ProductList where ProductName like '%{0}%' and Description like '%{1}%'", fields["name"].Trim(), fields["description"].Trim());
                _da = new SqlDataAdapter(dbquery, con);
                _da.Fill(dt);
                ViewBag.Reports = dt;
                ViewBag.description = fields["description"];
                return PartialView("~/Views/Portolo/Report/_ProductList.cshtml");
            }
            con.Close();

            return null;
        }

        [HttpGet]
        public void DownloadReport(string name, string description, string email, string titl, string org, string reportId)
        {
            SqlConnection con = new SqlConnection(ReadConnectionString());
            DataTable dt = new DataTable();
			string sname = "";				  
            SqlDataAdapter _da;
            string FileName = string.Empty;
            con.Open();

            if (reportId == "1")
            {
                string dbquery = String.Format(@"select a.Firstname, a.Lastname, a.Email, a.Title, a.Phone , o.OrganizationID from Account_list a inner join Organization_List  o on o.pKey = a.ParentOrganization_pKey where PortoloUser = 1
                            and CONCAT(a.Firstname, ' ', a.Lastname) like '%{0}%' and a.Email like '%{1}%' and a.Title like '%{2}%' and o.OrganizationID like '%{3}%'", name.Trim(), email.Trim(), titl.Trim(), org.Trim());
                _da = new SqlDataAdapter(dbquery, con);
                _da.Fill(dt);
                FileName = String.Format("Users_{0:yyMMdd_HH.mm}", DateTime.Now);
				sname = "User_List";					
            }

            if (reportId == "2")
            {
                string dbquery = String.Format("select * from Portolo_ProductList where ProductName like '%{0}%' and Description like '%{1}%'", name.Trim(), description.Trim());
                _da = new SqlDataAdapter(dbquery, con);
                _da.Fill(dt);
                FileName = String.Format("Products_{0:yyMMdd_HH.mm}", DateTime.Now);
				sname = "Product_List";					   
            }
            con.Close();


             ExportToExcel(dt, FileName, sname);
        }
												
        public ActionResult DownloadFile(string filePath)
        {

            string fullName = Server.MapPath("~" + filePath);
            if(!System.IO.File.Exists(fullName))
            {
                TempData["AlertMessage"] = "This file does not Exist!";
                return RedirectToAction("MyFiles");
            }

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
            string reportname = string.Format("ProcessLibrary_{0:yyMMdd_HH.mm}", DateTime.Now);
            var list = GetProcessDetails();
											
            DataTable dt = new DataTable();
            dt.Clear();
            dt.Columns.Add("S.No");
            dt.Columns.Add("Process Name");
            if (Session["searchlist"] != null)
            {
                list = (List<ProcessLibrary>)Session["searchlist"];
                Session.Remove("searchlist");											 
            }
            if (list.Count > 0)
            {
                foreach(var item in list)
                {
                    DataRow dataRow = dt.NewRow();
                    dataRow["S.No"] = item.pkey;
                    dataRow["Process Name"] = item.Process;
                    dt.Rows.Add(dataRow);
                }

                ExportToExcel(dt, reportname, "Portolo_ProcessLibrary");
                return Redirect(Request.UrlReferrer.ToString());
          
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
            dt.Columns.Add("Plan Date");
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
                    exceldata["Plan Date"] = item.plan;
                    exceldata["Due Date"] = item.duedate;
                    exceldata["Forecast"] = item.forecast;
                    exceldata["Category"] = item.TaskCategoryID;
                    exceldata["Status"] = item.TaskStatusID;
                    exceldata["Most Recent Notes"] = item.mostrecentnote;
                    dt.Rows.Add(exceldata);
                }
            }
             ExportToExcel(dt, FileName, "PublicTask_List");

        }
        private void ExportToExcel(DataTable dt, string fileName, string sheetname)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt, sheetname);
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
        public ActionResult Process()
        {
            return View("~/Views/Portolo/Process.cshtml");
        }

        //[HttpPost]
        //public ActionResult ProcessSubmit(Process process)
        //{
        //    string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
        //    string query = $"insert into process (name,email,telephone,subject,description) values ('{process.Name}','{process.Email}','{process.Telephone}','{process.Subject}','{process.Discription}') ;";
        //    using (SqlConnection con = new SqlConnection(config))
        //    {
        //        using (SqlCommand cmd = new SqlCommand(query, con))
        //        {
        //            con.Open();
        //            cmd.ExecuteNonQuery();
        //            con.Close();
        //        }
        //    }
        //    return Redirect($"/Portolo/Process");
        //}									 
        [AcceptVerbs(HttpVerbs.Post)]
        public string Tasks_Detele(string strpkey)
        {
            string qry = "EXEC Portolo_PublicTasks_Detele @strpkey";
            SqlCommand cmd = new SqlCommand(qry);
            cmd.CommandType = CommandType.Text;
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("@strpkey", strpkey);
            if (!clsUtility.ExecuteQuery(cmd, null, "Delete Task"))
            {
                return "Task Not Deleted";
            }
            else
            {
                return "Task Deleted";
            }
        }


        [AcceptVerbs(HttpVerbs.Post)]
        public string CopyTask(string strpkey)
        {
            string qry = "EXEC SP_CopyTask @strpkey ";
            SqlCommand cmd = new SqlCommand(qry);
            cmd.CommandType = CommandType.Text;
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("@strpkey", strpkey);
            if (!clsUtility.ExecuteQuery(cmd, null, "Delete Task"))
            {
                return "Task Not Copied ";
            }
            else
            {
                return "Task Copied sucessfully";
            }
        }

        public DataTable TaskList_Select_All1(TaskListRequest request)
        {
            int status = request.status != 0 ? Convert.ToInt32(request.status) : 0;

            SqlCommand cmd = new SqlCommand("TaskList_All", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 30;
            cmd.Parameters.AddWithValue("@planType", request.planType);
            cmd.Parameters.AddWithValue("@DueDate", request.duedate);
            cmd.Parameters.AddWithValue("@Forecast", request.forecast);
            cmd.Parameters.AddWithValue("@Number", request.number);
            cmd.Parameters.AddWithValue("@Title", request.title != null ? request.title : "");
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@TaskCategory_pKey", (request.intcategory));
            if (request.active != null)
            {
                if (Convert.ToBoolean(request.active) == true)
                {
                    cmd.Parameters.AddWithValue("@Active", 1);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Active", 0);
                }
            }
            if (request.reviewed != null)
            {
                if (Convert.ToBoolean(request.reviewed) == true)
                {
                    cmd.Parameters.AddWithValue("@Reviewed", 1);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Reviewed", 0);
                }

            }
            cmd.Parameters.AddWithValue("@TaskListRange", request.tasklistrange);
            cmd.Parameters.AddWithValue("@RepeatType_pKey", (request.intRepeat));
            cmd.Parameters.AddWithValue("@pKey", request.pKey);
            DataTable dt = new DataTable();

            if (clsUtility.GetDataTableStored(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;

        }

        public DataTable TaskRepeat_Select_ALL2()
        {
            DataTable dt = new DataTable();
            try
            {
                string dbquery = "EXEC TaskRepeat_Select_ALL";
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter(dbquery, con);
                sda.Fill(dt);
                con.Close();
            }
            catch
            {
                throw;
            }
            finally
            {
                con.Close();
            }
            return dt;
        }

        public DataTable PortoloTaskRepeat_Select_ALL1()
        {
            DataTable dt = new DataTable();
            try
            {
                string dbquery = "EXEC PortoloTaskRepeat_Select_ALL";
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter(dbquery, con);
                sda.Fill(dt);
                con.Close();
            }
            catch
            {
                throw;
            }
            finally
            {
                con.Close();
            }
            return dt;
        }

        public DataTable TaskStatuses_Select_All1()
        {
            DataTable dt = new DataTable();
            try
            {
                string dbquery = "EXEC TaskStatuses_Select_All";
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter(dbquery, con);
                sda.Fill(dt);
                con.Close();
            }
            catch
            {
                throw;
            }
            finally
            {
                con.Close();
            }
            return dt;
        }

        public DataTable TaskCategories_List()
        {
            DataTable dt = new DataTable();
            try
            {
                string dbquery = "EXEC TaskCategories_List";
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter(dbquery, con);
                sda.Fill(dt);
                con.Close();
            }
            catch
            {
                throw;
            }
            finally
            {
                con.Close();
            }
            return dt;
        }
    }
}


    



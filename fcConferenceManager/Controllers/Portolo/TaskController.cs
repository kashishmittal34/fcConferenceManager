using ClosedXML.Excel;
using Elimar.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web;

namespace fcConferenceManager.Controllers
{
    public class TaskController : Controller
    {
        private string config;
        private string baseurl;
        public TaskController()
        {
             config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
             baseurl = ConfigurationManager.AppSettings["AppURL"];
        }

																														   
					
        public ActionResult Index(List<TaskListResponse> taskListResponse)
        {
            if (Session["User"] != null)
            {
                ViewBag.Baseurl = baseurl;


                TempData.Keep("taskListResponse");
                if (TempData.Count == 0)
                {
                    TaskListRequest taskListRequest1 = new TaskListRequest();
                    TaskList(taskListRequest1);
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
                    TaskListRequest request = new TaskListRequest();
                    ViewBag.Request = request;

                }
                //List<Response> response = (List<TaskListResponse>)TempData["taskListResponse"];

                ViewBag.TaskListResponse = (List<TaskListResponse>)TempData["taskListResponse"];//TempData["taskListResponse"];




                TaskListRequest taskListRequest = new TaskListRequest();
                Common common = new Common();
                taskListRequest.commondropdownlist = common.GetDropDownList();
                return View("~/Portolo/Task/TaskList", taskListRequest);
            }
            else
            {
                return Redirect("~/Portolo/TaskList");
            }
        }
        public void Calltask()
        {
            TaskListRequest request = new TaskListRequest();
            var a = TaskList(request);

        }
        [HttpPost]
        public ActionResult TaskList(TaskListRequest request)
        {
            
            if (request.active == null && request.category == null && request.commondropdownlist ==null && request.duedate == null &&
                request.editprimarykey == 0 && request.forecast == null && request.number == null && request.pKey == 0 &&
                request.plandates == null && request.repeat == null && request.reviewed == null && request.status == null &&
                request.tasklistrange == null && request.title == null   )
            {
                request.active = null;
                request.category = "0";
                request.commondropdownlist = null;
                //request.duedate = null;
                request.editprimarykey = 0;
                //request.forecast = null;
                request.number = null;
                request.pKey = 0;
                //request.plandates = "0";
                //request.repeat = "0";
                request.reviewed = null;
                request.status = 0;
                request.tasklistrange = "0";
                request.title = null;
            }

            ViewBag.TaskListRequest = request;
            List<TaskListResponse> taskListResponse = new List<TaskListResponse>();
            try
            {


																										
                using (SqlConnection con = new SqlConnection(config))
                {

                    using (SqlCommand cmd = new SqlCommand("TaskList_All", con))
                    {
                        con.Open();
                        cmd.CommandType = CommandType.StoredProcedure;

                        //cmd.Parameters.AddWithValue("@PlanDates", request.plandates);
                        //cmd.Parameters.AddWithValue("@DueDate", request.duedate);
                        //cmd.Parameters.AddWithValue("@Forecast", request.forecast);
                        //cmd.Parameters.AddWithValue("@Number", request.number);
                        //cmd.Parameters.AddWithValue("@Title", request.title);
                        //cmd.Parameters.AddWithValue("@Status", Convert.ToInt32(request.status));
                        //cmd.Parameters.AddWithValue("@TaskCategory_pKey", int.Parse(request.category));
                        //if (request.active != "")
                        //{
                        //    cmd.Parameters.AddWithValue("@Active", Convert.ToBoolean(request.active));
                        //}
                        //if (request.reviewed != "")
                        //{
                        //    cmd.Parameters.AddWithValue("@Reviewed", Convert.ToBoolean(request.reviewed));
                        //}
                        //cmd.Parameters.AddWithValue("@TaskListRange", request.tasklistrange);
                        //cmd.Parameters.AddWithValue("@RepeatType_pKey", int.Parse(request.repeat));
                        //cmd.Parameters.AddWithValue("@pKey", request.pKey);
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            TaskListResponse taskList = new TaskListResponse();
                            //taskList.plan = DateTime.Parse(reader["PlanDate"].ToString());
                            //if (reader["PlanDate"].ToString() != "")
                            //{
                            //    taskList.plan = DateTime.Parse(reader["PlanDate"].ToString());
                            //}
                            //else
                            //{
                            //    taskList.plan = null;
                            //}
                            //if (reader["DueDate"].ToString() != "")
                            //{
                            //    taskList.duedate = DateTime.Parse(reader["DueDate"].ToString());
                            //}
                            //else
                            //{
                            //    taskList.duedate = null;
                            //}
                            //if (reader["Forecast"].ToString() != "")
                            //{
                            //    taskList.forecast = DateTime.Parse(reader["Forecast"].ToString());
                            //}
                            //else
                            //{
                            //    taskList.forecast = null;
                            //}
                            taskList.title = reader["Title"].ToString();
                            taskList.description = reader["Description"].ToString();
                            taskList.status = int.Parse(reader["Status"].ToString());
                            taskList.TaskCategory_pKey = int.Parse(reader["TaskCategory_pKey"].ToString());
                            taskList.RepeatType_pKey = int.Parse(reader["RepeatType_pKey"].ToString());
                            taskList.TaskCategoryID = reader["TaskCategoryID"].ToString();
                            taskList.TaskStatusID = reader["TaskStatusID"].ToString();
                            taskList.TaskRepeatID = reader["TaskRepeatID"].ToString();
                            taskList.number = reader["Number"].ToString();
                            taskList.mostrecentnote = reader["MostRecentNote"].ToString();
                            taskList.name = reader["Name"].ToString();
                            taskList.pKey = reader["pKey"].ToString();
                            taskList.active = Boolean.Parse(reader["Active"].ToString());
                            taskList.reviewed = Boolean.Parse(reader["Reviewed"].ToString());
                            taskList.Tips = reader["Tips"].ToString();
                            taskList.Instruction = reader["Instruction"].ToString();
                            taskList.Notes = reader["Notes"].ToString();
                            taskList.Resources = reader["Resources"].ToString();
                            taskList.ResourcesFileName = reader["ResourcesFileName"].ToString();

                            if (reader["PlanDate"].ToString() != null && reader["PlanDate"].ToString() != "")
                            {
                                taskList.plan = Convert.ToDateTime(reader["PlanDate"].ToString());
                            }
                            if (reader["DueDate"].ToString() != null && reader["DueDate"].ToString() != "")
                            {
                                taskList.duedate = Convert.ToDateTime(reader["DueDate"].ToString());
                            }
                            if (reader["Forecast"].ToString() != null && reader["Forecast"].ToString() != "")
                            {
                                taskList.forecast = Convert.ToDateTime(reader["Forecast"].ToString());
                            }

                            taskListResponse.Add(taskList);

                        }
                        reader.Close();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }

                    TempData["taskListResponse"] = taskListResponse;

                    TempData["request"] = request;

                    TempData["Temprequest"] = true;



                }
            }
            catch (Exception ex)
            {
                return Redirect("~/Portolo/TaskList");
            }


            return Redirect("~/Portolo/TaskList");
        }

        public ActionResult EditTask(int? id)
        {
			List<PublicTaskResource> publicTaskResources = new List<PublicTaskResource>();																			  
            List<TaskListResponse> taskListResponse = new List<TaskListResponse>();
            try
            {
																										
                using (SqlConnection con = new SqlConnection(config))
                {

                    using (SqlCommand cmd = new SqlCommand("SP_TaskList", con))
                    {
                        con.Open();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@pKey", id);
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            TaskListResponse taskList = new TaskListResponse();
                            //taskList.plan = DateTime.Parse(reader["PlanDate"].ToString());
                            if (reader["PlanDate"].ToString() != "")
                            {
                                taskList.plan = DateTime.Parse(reader["PlanDate"].ToString());
                            }
                            else
                            {
                                taskList.plan = null;
                            }
                            if (reader["DueDate"].ToString() != "")
                            {
                                taskList.duedate = DateTime.Parse(reader["DueDate"].ToString());
                            }
                            else
                            {
                                taskList.duedate = null;
                            }
                            if (reader["Forecast"].ToString() != "")
                            {
                                taskList.forecast = DateTime.Parse(reader["Forecast"].ToString());
                            }
                            else
                            {
                                taskList.forecast = null;
                            }
                            taskList.title = reader["Title"].ToString();
                            taskList.description = reader["Description"].ToString();
                            taskList.status = int.Parse(reader["Status"].ToString());
                            taskList.TaskCategory_pKey = int.Parse(reader["TaskCategory_pKey"].ToString());
                            taskList.RepeatType_pKey = int.Parse(reader["RepeatType_pKey"].ToString());
                            taskList.TaskCategoryID = reader["TaskCategoryID"].ToString();
                            taskList.TaskStatusID = reader["TaskStatusID"].ToString();
                            taskList.TaskRepeatID = reader["TaskRepeatID"].ToString();
                            taskList.number = reader["Number"].ToString();
                            taskList.mostrecentnote = reader["MostRecentNote"].ToString();
                            taskList.name = reader["Name"].ToString();
                            taskList.pKey = reader["pKey"].ToString();
                            taskList.Tips = reader["Tips"].ToString();
                            taskList.Instruction = reader["Instruction"].ToString();
                            taskList.Notes = reader["Notes"].ToString();
                            taskList.Resources = reader["Resources"].ToString();
                            taskList.ResourcesFileName = reader["ResourcesFileName"].ToString();
                            if (reader["reviewed"].ToString() != null)
                            {
                                taskList.reviewed = Boolean.Parse(reader["reviewed"].ToString());
                            }

                            if (reader["active"].ToString() != null)
                            {
                                taskList.active = Boolean.Parse(reader["active"].ToString());
                            }
                            taskListResponse.Add(taskList);
                        }
						reader.NextResult();
                        while (reader.Read())
                        {
                            PublicTaskResource publicTask = new PublicTaskResource();

                            publicTask.pkey = int.Parse(reader["pkey"].ToString());
                            publicTask.PublicTaskID = int.Parse(reader["PublicTaskID"].ToString());
                            publicTask.ResourcesFileName = reader["ResourcesFileName"].ToString();
                            publicTask.Resources = reader["Resources"].ToString();
                            publicTaskResources.Add(publicTask);

                        }
                        reader.Close();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }  
                    if(taskListResponse.Count()>0)
                    taskListResponse[0].publicTaskResources = publicTaskResources;
                    TempData["EdittaskListResponse"] = taskListResponse;
                    commonreload();
                    TempData["Editid"] = id;
                }
            }
            catch (Exception ex)
            {
                return Redirect("~/Portolo/TaskList");
            }


            return Redirect("~/Task/AddTask?id=" + id);
        }
        public ActionResult DeleteTask(string primarykey)
        {

            try
            {
                if (Session["User"] != null)
                {
                    List<int> primaryKeys = primarykey.Split(',').Select(int.Parse).ToList();
                    foreach (var item in primaryKeys)
                    {
																												
                         using (SqlConnection con = new SqlConnection(config))
                        {
                            using (SqlCommand cmd = new SqlCommand("SP_DeleteData", con))
                            {
                                con.Open();
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@pKey", item);


                                cmd.ExecuteNonQuery();
                                con.Close();

                            }
                        }
                    }
                    TaskListRequest request = new TaskListRequest();

                    commonreload();
                    var a = TaskList(request);
                }
                else
                {
                    Redirect("~/Account/Index");
                }
            }
            catch (Exception ex)
            {
                return Redirect("~/Portolo/TaskList");
            }
            return Redirect("~/Portolo/TaskList");


        }
        public ActionResult CopyTask(string primaryKey)
        {
            try
            {
                List<int> primaryKeys = primaryKey.Split(',').Select(int.Parse).ToList();
                foreach (var item in primaryKeys)
                {
																											
                    using (SqlConnection con = new SqlConnection(config))
                    {
                        using (SqlCommand cmd = new SqlCommand("SP_CopyTask", con))
                        {
                            con.Open();
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@pKey", item);
                            cmd.ExecuteNonQuery();
                            con.Close();

                        }
                    }
                }
                TaskListRequest request = new TaskListRequest();

                commonreload();

                var a = TaskList(request);
            }
            catch (Exception ex)
            {
                return Redirect("~/Portolo/TaskList");
            }

            return Redirect("~/Portolo/TaskList");
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase files)
        {
            // Verify that the user selected a file
            if (files != null && files.ContentLength > 0)
            {
                // extract only the filename
                var fileName = Path.GetFileName(files.FileName);
                // store the file inside ~/App_Data/uploads folder
                var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                files.SaveAs(path);
            }
            // redirect back to the index action to show the form once again
            return RedirectToAction("Index");
        }
        
        [ValidateInput(false)]
        [Route("Task/AddTask?id")]  
        public ActionResult AddTask(TaskAdd model ,HttpPostedFileBase files, string submit)
        {
            ViewBag.HeadingTitle = "Add New Task";
            TempData.Keep("EdittaskListResponse");

            if (TempData["Source"] == null)
            {
                ViewBag.Source = "Planning";
            }
            else
            {
                ViewBag.Source = TempData["Source"];

            }

            int PublicTaskID=0;
            TaskListResponse taskListResponse = new TaskListResponse();
            var tempData = (List<TaskListResponse>)TempData["EdittaskListResponse"];//TempData["taskListResponse"]; 
            if (tempData != null && tempData.Count > 0)
            {
                ViewBag.HeadingTitle = " Edit Task";
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

                taskListResponse.Tips = tempData[0].Tips;
                taskListResponse.Instruction = tempData[0].Instruction;
                taskListResponse.Notes = tempData[0].Notes;
                taskListResponse.Resources = tempData[0].Resources;
                taskListResponse.ResourcesFileName = tempData[0].ResourcesFileName;
				taskListResponse.publicTaskResources = tempData[0].publicTaskResources;
																	   

            }
            model.taskListResponse = taskListResponse;
            Common common = new Common();
            model.commondropdownlist = common.GetDropDownList();

            if (model.title != null && model.description != null)
            {


                // if (files != null && files.ContentLength > 0)
                // {

                    // //byte[] bytes;
                    // //using (BinaryReader br = new BinaryReader(files.InputStream))
                    // //{
                    // //    bytes = br.ReadBytes(files.ContentLength);
                    // //}
                    // string extension = Path.GetExtension(files.FileName);
                    // Guid guid = Guid.NewGuid();
                    // string str = guid.ToString()+ extension;
                    // var fileName = Path.GetFileName(files.FileName);
                  
                    
                    // var ffname = Path.Combine(Server.MapPath("~/PortoloDocuments/"), str);
                    // files.SaveAs(ffname);
                    // model.ResourcesFileName = str;
                    // model.Resources = fileName;
                // }
                
               
                if (submit != "Upload")
                {
                    using (SqlConnection con = new SqlConnection(config))
                    {
                        using (SqlCommand cmd = new SqlCommand("[SP_TaskAdd]", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@pkey", Convert.ToInt32(model.editprimarykey));
                            cmd.Parameters.AddWithValue("@Title", model.title);
                            cmd.Parameters.AddWithValue("@Description", model.description);

                            cmd.Parameters.AddWithValue("@Reviewed", model.reviewed);
                            cmd.Parameters.AddWithValue("@active", model.active);

                            cmd.Parameters.AddWithValue("@PlanDate", model.plandate);
                            cmd.Parameters.AddWithValue("@DueDate", model.duedate);
                            cmd.Parameters.AddWithValue("@Forecast", model.forecast);

                            if (model.RepeatType_pKey == null)
                            {
                                cmd.Parameters.AddWithValue("@RepeatType_pKey", model.taskListResponse.RepeatType_pKey);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@RepeatType_pKey", model.RepeatType_pKey);
                            }
                            if (model.TaskCategory_pKey == null)
                            {
                                cmd.Parameters.AddWithValue("@TaskCategory_pKey", model.taskListResponse.TaskCategory_pKey);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@TaskCategory_pKey", model.TaskCategory_pKey);
                            }
                            if (model.status == null)
                            {
                                cmd.Parameters.AddWithValue("@Status", model.taskListResponse.status);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@Status", model.status);
                            }
                            cmd.Parameters.AddWithValue("@Tips", model.Tips);
                            cmd.Parameters.AddWithValue("@Instruction", model.Instruction);
                            cmd.Parameters.AddWithValue("@Notes", model.Notes);
                            cmd.Parameters.AddWithValue("@Resources", "");//model.Resources
                            cmd.Parameters.AddWithValue("@ResourcesFileName", "");//model.ResourcesFileName
	  
                            con.Open();
                            SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                TaskListResponse taskList = new TaskListResponse();
                                //taskList.plan = DateTime.Parse(reader["PlanDate"].ToString());

                                PublicTaskID = Convert.ToInt32(reader["ID"].ToString());
                            }
                            reader.Close();

                            con.Close();
                        }
			
                    }
                }
                if (model.files != null)
                {
                    if (model.editprimarykey != "" && model.editprimarykey!=null)
                    {
                        PublicTaskID = Convert.ToInt32(model.editprimarykey);
                    }
                    foreach (var item in model.files)
                    {
                        if (item != null && PublicTaskID>0)
                        {
                            string extension = Path.GetExtension(item.FileName);
                            Guid guid = Guid.NewGuid();
                            string str = guid.ToString() + extension;
                            var fileName = Path.GetFileName(item.FileName);

                            var ffname = Path.Combine(Server.MapPath("~/PortoloDocuments/"), str);
                            item.SaveAs(ffname);
                            //model.ResourcesFileName = str;
                            //model.Resources = fileName;
                            using (SqlConnection con = new SqlConnection(config))
                            {
                                using (SqlCommand cmd = new SqlCommand("[SP_InsertPublicTaskResources]", con))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@PublicTaskID", PublicTaskID);                                    
                                    cmd.Parameters.AddWithValue("@Resources", fileName);//model.Resources
                                    cmd.Parameters.AddWithValue("@ResourcesFileName", str);//model.ResourcesFileName

                                    con.Open();

                                    cmd.ExecuteNonQuery();
                                    con.Close();
                                }
                                TempData["Source"] = "Source";
                            }

                                }
                            }
								 

                    //string extension = Path.GetExtension(files.FileName);
                    //Guid guid = Guid.NewGuid();
                    //string str = guid.ToString()+ extension;
                    //var fileName = Path.GetFileName(files.FileName);


                    //var ffname = Path.Combine(Server.MapPath("~/PortoloDocuments/"), str);
                    //files.SaveAs(ffname);
                    //model.ResourcesFileName = str;
                    //model.Resources = fileName;
                }


            }
            if (model.formsubmit != null && submit == null)
            {
                TempData["EdittaskListResponse"] = null;
                TempData["request"] = null;

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
                TaskList(taskListRequest);
                return Redirect("~/Portolo/TaskList");
            }
            else if (submit != null)
            {
                return RedirectToAction("EditTask", "Task", new { id = PublicTaskID });
            }
            else
            {
               
                return View("~/Views/Portolo/Task/AddTask.cshtml", model);
            }
        }
        [HttpPost]
        public ActionResult DeleteResourceFile(int pkey,string Source)
        {
																									
              using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand("DeleteResourceFile", con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@pKey", pkey);


                    cmd.ExecuteNonQuery();
                    con.Close();

                }
            }
            TempData["Source"] = "Source";

            var PublicTaskpkey = pkey;
            return Content(PublicTaskpkey.ToString());//Redirect("~/Task/EditTask/"+ PublicTaskpkey);
        }
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

        public ActionResult commonreload()
        {

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
            taskListRequest.reviewed = null;
            taskListRequest.status = 0;
            taskListRequest.tasklistrange = "0";
            taskListRequest.title = null;

            var a = TaskList(taskListRequest);

            return View("~/Views/Portolo/Task/Index.cshtml", taskListRequest);
        }



        public ActionResult DeleteImage(int id)
        {
																									
            using (SqlConnection con = new SqlConnection(config))
            {

                using (SqlCommand cmd = new SqlCommand("SP_DeleteImage", con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@pKey", id);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader["ImagesName"].ToString() != "")
                        {

                            var filePath = Server.MapPath("~/PortoloDocuments/" + reader["ImagesName"].ToString());
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);

                                EditTask(id);
                            }
                        }
                    }
                }
                return Redirect("~/Task/AddTask");
            }
        }
        public ActionResult List(string page)
        {
            var pageRoute = "~/Views/Portolo/Task/"+ page + ".html";
            var staticPageToRender = new FilePathResult(pageRoute, "text/html");
            return staticPageToRender;
        }
    }




}




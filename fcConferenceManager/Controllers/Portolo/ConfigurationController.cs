using fcConferenceManager.Models;
using fcConferenceManager.Models.Portolo;
using MAGI_API.Models;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Mvc;


namespace fcConferenceManager.Controllers.Portolo
{
    [CheckActiveEventAttribute]
    public class ConfigurationController : Controller
    {
        DBAccessLayer dba = new DBAccessLayer();
        static SqlOperation repository = new SqlOperation();
        private string config;
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
        public ConfigurationController()
        {
            config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
        }

        public ActionResult ConfigurationSettings(ApplicationSettingViewModel setting)
        {

            List<ApplicationSetting> setlist = GetSettingDetails();
            setting.SettingList = setlist;
            setting.AccountImg = GetAccountName();
            setting.OrganizationImg = GetOrgName();
            return View(setting);

        }
        [HttpPost]
        public ActionResult AccountSettings(HttpPostedFileBase AccountImg)
        {
            ApplicationSettingViewModel model = new ApplicationSettingViewModel();
            if (AccountImg != null)
            {
                var Extension = Path.GetExtension(AccountImg.FileName);
                var changeExtension = Path.ChangeExtension(Extension, ".jpg");
                var ResourcefileName = "AccountImage" + changeExtension;
                string path = Path.Combine(Server.MapPath("~/ImagesUpload/"), ResourcefileName);
                string filepath = Url.Content(Path.Combine("~/ImagesUpload/", ResourcefileName));
                if (System.IO.File.Exists(filepath))
                {
                    System.IO.File.Delete(filepath);
                }
                AccountImg.SaveAs(path);

                model.AccountImg = filepath;
            }
            else
            {
                model.AccountImg = string.Empty;
            }
            if (UploadImages(model.AccountImg))
            {

                TempData["AlertMessage"] = "Uploaded Successfully !!";
                return RedirectToAction("ConfigurationSettings", "Configuration");
            }
            else
            {
                ModelState.AddModelError("", "Error In Add File. Please Try Again !!!");
            }
            return RedirectToAction("ConfigurationSettings", "Configuration");
        }
        [HttpPost]
        public ActionResult OrganizationSettings(HttpPostedFileBase OrganizationImg)
        {
            ApplicationSettingViewModel model = new ApplicationSettingViewModel();
            if (OrganizationImg != null)
            {
                var Extension = Path.GetExtension(OrganizationImg.FileName);
                var changeExtension = Path.ChangeExtension(Extension, ".jpg");
                var ResourcefileName = "OrganizationImage" + changeExtension;
                string path = Path.Combine(Server.MapPath("~/ImagesUpload/"), ResourcefileName);
                string filepath = Url.Content(Path.Combine("~/ImagesUpload/", ResourcefileName));
                if (System.IO.File.Exists(filepath))
                {
                    System.IO.File.Delete(filepath);
                }
                OrganizationImg.SaveAs(path);
                model.OrganizationImg = filepath;
            }
            else
            {
                model.OrganizationImg = null;
            }
            if (UploadOrgImages(model.OrganizationImg))
            {

                TempData["AlertMessage"] = "Uploaded Successfully !!";
                return RedirectToAction("ConfigurationSettings", "Configuration");
            }
            else
            {
                ModelState.AddModelError("", "Error In Add File. Please Try Again !!!");
            }
            return RedirectToAction("ConfigurationSettings", "Configuration");
        }
        public string GetAccountName()
        {
            string query = string.Empty , accountname = string.Empty;
            
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            SqlConnection con = new SqlConnection(config);
            con.Open();
            query = "Select SettingValue from Portolo_ApplicationSettings where pKey = 200";

            SqlCommand command = new SqlCommand(query, con);
            if (command.ExecuteScalar() != null)
            {
                accountname = command.ExecuteScalar().ToString();
            }
            con.Close();
            return accountname;
        }
        public string GetOrgName()
        {
            string query = string.Empty, orgname = string.Empty;
            DataTable dt = new DataTable();
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            SqlConnection con = new SqlConnection(config);
            con.Open();
            query = "Select SettingValue from Portolo_ApplicationSettings where pKey = 201";
            SqlCommand command = new SqlCommand(query, con);
            if (command.ExecuteScalar() != null)
            {
                orgname = command.ExecuteScalar().ToString();
            }
            con.Close();
            return orgname;
        }
        private bool UploadImages(string model)
        {

            string query = string.Empty;
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            SqlConnection con = new SqlConnection(config);
            query = "Update Portolo_ApplicationSettings set SettingValue = '" + model + "' where pKey = 200";
            con.Open();
            SqlCommand command = new SqlCommand(query, con);
            int numResult = command.ExecuteNonQuery();
            con.Close();
            if (numResult > 0)
                return true;
            else
                return false;
        }
        private bool UploadOrgImages(string model)
        {

            string query = string.Empty;
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            SqlConnection con = new SqlConnection(config);
            query = "Update Portolo_ApplicationSettings set SettingValue = '" + model + "' where pKey = 201";
            con.Open();
            SqlCommand command = new SqlCommand(query, con);
            int numResult = command.ExecuteNonQuery();
            con.Close();
            if (numResult > 0)
                return true;
            else
                return false;
        }
        private List<ApplicationSetting> GetSettingDetails()
        {
            List<ApplicationSetting> setlist = new List<ApplicationSetting>();
            DataTable dtData = new DataTable();
            string query = "Select * from Portolo_ApplicationSettings";
            SqlConnection con = new SqlConnection(config);
            con.Open();
            SqlCommand command = new SqlCommand(query, con);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dtData);
            con.Close();
            foreach (DataRow dr in dtData.Rows)
            {
                setlist.Add(new ApplicationSetting
                {
                    pkey = Convert.ToInt32(@dr["pKey"]),
                    SettingID = @dr["SettingID"].ToString(),
                    SettingValue = dr["SettingValue"].ToString()
                });

            }
            return setlist;
        }

        public JsonResult EditSetting(int? id)
        {
            var customer = GetSettingDetails().Find(x => x.pkey.Equals(id));
            return Json(customer, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdateSetting(ApplicationSetting setting)
        {


            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ApplicationSettings", con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@pkey", setting.pkey);
                    cmd.Parameters.AddWithValue("@Value", setting.SettingValue.Trim());
                    cmd.Parameters.AddWithValue("@status", "Update");
                    int result = cmd.ExecuteNonQuery();
                    if (result == 1)
                    {
                        ViewBag.Message = "Setting Updated Successfully";
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
        public byte[] ExporttoExcel(DataTable table, string filename)
        {

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage pack = new ExcelPackage();
            ExcelWorksheet ws = pack.Workbook.Worksheets.Add(filename);
            ws.Cells["A2"].LoadFromDataTable(table, true, TableStyles.Medium12);

            return pack.GetAsByteArray();

        }
        public ActionResult Download()
        {
            string reportname = $"ConfigurationSettings_{DateTime.Now.ToString("dddd, dd MMMM yyyy")}.xlsx";
            var list = GetSettingDetails();
            DataTable dt = new DataTable();
            dt.Clear();
            dt.Columns.Add("S.No");
            dt.Columns.Add("Setting Id");
            dt.Columns.Add("Value");

            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    DataRow dataRow = dt.NewRow();
                    dataRow["S.No"] = item.pkey;
                    dataRow["Setting Id"] = item.SettingID;
                    dataRow["Value"] = item.SettingValue;
                    dt.Rows.Add(dataRow);
                }
                var exportbytes = ExporttoExcel(dt, reportname);
                return File(exportbytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportname);
            }
            else
            {
                TempData["Message"] = "No Data to Export";
                return RedirectToAction("ConfigurationSettings", "Configuration");
            }

        }
       

    }
}
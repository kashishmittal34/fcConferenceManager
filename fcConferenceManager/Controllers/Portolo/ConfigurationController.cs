﻿using ClosedXML.Excel;
using fcConferenceManager.Models;
using fcConferenceManager.Models.Portolo;
using MAGI_API.Models;
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

    public interface IConfigurationController
    {
        string GetAccountName();
    }
    public class ConfigurationController : Controller, IConfigurationController
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
            ViewBag.Message = TempData["Message"];
            return View("~/Views/Portolo/Configuration/ConfigurationSettings.cshtml", setting);

        }
        [HttpPost]
        public ActionResult AccountSettings(HttpPostedFileBase AccountImg)
        {
            ApplicationSettingViewModel model = new ApplicationSettingViewModel();
            if (AccountImg != null)
            {
                var Extension = Path.GetExtension(AccountImg.FileName);
                var changeExtension = Path.ChangeExtension(Extension, ".jpg");
                var ResourcefileName = "Portolo_AccountImage" + changeExtension;
                string path = Path.Combine(Server.MapPath("~/PortoloDocuments/"), ResourcefileName);
                string filepath = Url.Content(Path.Combine("~/PortoloDocuments/", ResourcefileName));
                if (System.IO.File.Exists(filepath))
                {
                    System.IO.File.Delete(filepath);
                }
                AccountImg.SaveAs(path);
                model.AccountImg = filepath;
                TempData["Message"] = "Account Image Uploaded!";
            }
            else
            {
                model.AccountImg = string.Empty;
            }
            if (UploadAccountImages(model.AccountImg))
            {
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
                var ResourcefileName = "Portolo_OrganizationImage" + changeExtension;
                string path = Path.Combine(Server.MapPath("~/PortoloDocuments/"), ResourcefileName);
                string filepath = Url.Content(Path.Combine("~/PortoloDocuments/", ResourcefileName));
                if (System.IO.File.Exists(filepath))
                {
                    System.IO.File.Delete(filepath);
                }
                OrganizationImg.SaveAs(path);
                model.OrganizationImg = filepath;
                TempData["Message"] = "Organization Image Uploaded!";
            }
            else
            {
                model.OrganizationImg = null;
            }
            if (UploadOrgImages(model.OrganizationImg))
            {
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
            string query = string.Empty, accountname = string.Empty;
            SqlConnection con = new SqlConnection(config);
            con.Open();
            query = "Select SettingValue from Portolo_ApplicationSettings where pKey = 1";

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
            SqlConnection con = new SqlConnection(config);
            con.Open();
            query = "Select SettingValue from Portolo_ApplicationSettings where pKey = 2";
            SqlCommand command = new SqlCommand(query, con);
            if (command.ExecuteScalar() != null)
            {
                orgname = command.ExecuteScalar().ToString();
            }
            con.Close();
            return orgname;
        }
        private bool UploadAccountImages(string model)
        {
            string query = string.Empty;
            SqlConnection con = new SqlConnection(config);
            query = "Update Portolo_ApplicationSettings set SettingValue = '" + model + "' where pKey =  '1'";
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
            SqlConnection con = new SqlConnection(config);
            query = "Update Portolo_ApplicationSettings set SettingValue = '" + model + "' where pKey = '2'";
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
            string query = "SELECT REPLICATE('0',5-LEN(RTRIM(pkey))) + RTRIM(pkey) as PrimaryKey , SettingValue,SettingID ,pKey from Portolo_ApplicationSettings ORDER BY pKey OFFSET 2 ROWS FETCH  next 2000 ROWS only";
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
                    pkey = @dr["PrimaryKey"].ToString(),
                    SettingID = @dr["SettingID"].ToString(),
                    SettingValue = dr["SettingValue"].ToString(),
                    Id = Convert.ToInt32(@dr["pKey"]),
                });

            }
            return setlist;
        }
        private List<ApplicationSetting> GetEditDetails()
        {
            List<ApplicationSetting> setlist = new List<ApplicationSetting>();
            DataTable dtData = new DataTable();
            string query = "SELECT *  from Portolo_ApplicationSettings";
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
                    Id = Convert.ToInt32(@dr["pKey"]),
                    SettingID = @dr["SettingID"].ToString(),
                    SettingValue = dr["SettingValue"].ToString()
                });

            }
            return setlist;
        }
        public JsonResult EditSetting(int id)
        {
            var customer = GetEditDetails().Find(x => x.Id.Equals(id));

            return Json(customer, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult UpdateSetting(ApplicationSetting setting)
        {
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ApplicationSettings", con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@pkey", setting.Id);
                    cmd.Parameters.AddWithValue("@Value", setting.SettingValue.Trim());
                    cmd.Parameters.AddWithValue("@status", "Update");
                    int result = cmd.ExecuteNonQuery();
                    if (result == 1)
                    {
                        TempData["Message"] = "Setting Value Updated!";
                        ModelState.Clear();
                    }
                    else
                    {
                        TempData["Message"] = "Unsucessfull";
                        ModelState.Clear();
                    }
                    con.Close();
                    if (setting.Id <= 4)
                    {
                        return RedirectToAction("ConfigurationSettings");
                    }
                    return Redirect(Request.UrlReferrer.ToString());
                }
            }
        }
        //public byte[] ExporttoExcel(DataTable table, string filename)
        //{
        //    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //    ExcelPackage pack = new ExcelPackage();
        //    ExcelWorksheet ws = pack.Workbook.Worksheets.Add(filename);
        //    ws.Cells["A2"].LoadFromDataTable(table, true, TableStyles.Medium12);
        //    return pack.GetAsByteArray();
        //}
        public ActionResult Download()
        {
            string reportname = string.Format("ConfigurationSettings_{0:yyMMdd_HH.mm}", DateTime.Now);

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
                ExportToExcel(dt, reportname, "ConfigurationSettingsList");
                return Redirect(Request.UrlReferrer.ToString());
                //return File(exportbytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportname);
            }
            else
            {
                TempData["Message"] = "No Data to Export";
                return RedirectToAction("ConfigurationSettings", "Configuration");
            }
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
        public PartialViewResult PublicPage(PublicContentPage publicContent)
        {
            publicContent.AboutUs = GetAboutUsContent();
            publicContent.TermsOfUse = GetTermsOfUseContent();
            ViewBag.content = Request.QueryString["AboutUs"];
            return PartialView("~/Views/Portolo/Configuration/PublicPage.cshtml", publicContent);
        }
        public string GetAboutUsContent()
        {
            const int aboutUsPkey = 3;
            string query = string.Empty, aboutUs = string.Empty;
            try
            {

                DataTable dt = new DataTable();
                SqlConnection con = new SqlConnection(config);
                con.Open();
                query = "Select SettingValue from Portolo_ApplicationSettings where pKey = '" + aboutUsPkey + "'";
                SqlCommand command = new SqlCommand(query, con);
                if (command.ExecuteScalar() != null)
                {
                    aboutUs = command.ExecuteScalar().ToString();
                }
                con.Close();
            }
            catch
            {
                TempData["Message"] = "There is some Error!";
            }
            return aboutUs;
        }

        public string GetTermsOfUseContent()
        {
            const int TermsPkey = 4;
            string query = string.Empty, terms = string.Empty;
            try
            {
                DataTable dt = new DataTable();
                SqlConnection con = new SqlConnection(config);
                con.Open();
                query = "Select SettingValue from Portolo_ApplicationSettings where pKey = '" + TermsPkey + "'";
                SqlCommand command = new SqlCommand(query, con);
                if (command.ExecuteScalar() != null)
                {
                    terms = command.ExecuteScalar().ToString();
                }
                con.Close();
            }
            catch
            {
                TempData["Message"] = "There is some Error!";
            }
            return terms;
        }
        public ActionResult EditConfigurationText(int? id)
        {
            var applicationSetting = GetEditDetails().Find(x => x.Id.Equals(id));
            ViewBag.Application = applicationSetting;
            ApplicationSetting application = applicationSetting;
            return View("~/Views/Portolo/EditConfigurationText.cshtml", application);
        }


    }
}
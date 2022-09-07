using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using fcConferenceManager.Models.Portolo;
using OfficeOpenXml;
using System.IO;
using System.Data;
using OfficeOpenXml.Table;
using MAGI_API.Models;

namespace fcConferenceManager.Controllers.Portolo
{

    public class MasterPageController : Controller
    {
        public ActionResult MasterPage(int tableId = 0)
        {
            List<LookUp> dataLookUp = new List<LookUp>();
            MasterPageRepo repo = new MasterPageRepo();
            ViewBag.LookUpTable = repo.getDropDownLookUp();
            List<string> tableName = new List<string>();
            if (tableId == 0)
            {
                tableName = repo.findTableLookUP(Convert.ToInt32(ViewBag.LookUpTable[0].Value));
                dataLookUp = repo.getDataLookUp(tableName[0], tableName[1]);

            }
            else
            {
                tableName = repo.findTableLookUP(tableId);
                dataLookUp = repo.getDataLookUp(tableName[0], tableName[1]);
            }
            ViewBag.SecurityRole = repo.getDropDownSecurityRole();
            List<GlobalSetting> dataGlobalSetting = repo.getDataGlobalSetting() ;
            List<RegistrationSetting> dataRegistrationSetting = repo.getDataRegistrationSetting();
            List<EventRole> dataEventRole = repo.getDataEventRole();
            MasterTable masterTable = new MasterTable()

            {
                LookUps = dataLookUp,
                GlobalSettings = dataGlobalSetting,
                RegistrationSettings = dataRegistrationSetting,
                EventRoles = dataEventRole
            };
            return View("~/Views/Portolo/MasterPage/MasterPage.cshtml", masterTable);
        }
        public ActionResult ExportToExcel(int role)
        {
            MasterPageRepo repo = new MasterPageRepo();
            ViewBag.LookUpTable = repo.getDropDownLookUp();

            List<string> tableName = repo.findTableLookUP(role);
            List<LookUp> dataLookUp = repo.getDataLookUp(tableName[0], tableName[1]);

            var gv = new System.Web.UI.WebControls.GridView();
            if (dataLookUp.Count == 0)
            {
                dataLookUp.Add(new LookUp());
            }

            gv.DataSource = dataLookUp;
            gv.DataBind();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.Cells[1, 1].LoadFromCollection(dataLookUp, true);
            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                Response.AddHeader("content-disposition", "attachment;  filename=Table_Details_" + tableName + ".xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }

            return RedirectToAction("MasterPage", new { tableId = role });
        }


        public ActionResult EditEntry(string updateName, string updateId, string updateTableId)
        {
            int tID = Convert.ToInt32(updateTableId);
            int eID = Convert.ToInt32(updateId);

            MasterPageRepo repo = new MasterPageRepo();

            List<string> tableName = repo.findTableLookUP(tID);

            bool ans = repo.EditEntryLookUp(eID, updateName, tableName[0], tableName[1]);

            if (ans)
            {
                return RedirectToAction("MasterPage", new { tableId = tID });

            }
            else
            {
                return View();
            }
        }

        public ActionResult AddNewEntry(string RoleTableId, string NewName)
        {

            int x = Convert.ToInt32(RoleTableId);


            MasterPageRepo repo = new MasterPageRepo();

            List<string> tableName = repo.findTableLookUP(x);

            bool ans = repo.AddNewDataLookUp(tableName[0], tableName[1], NewName);

            if (ans)
            {
                return RedirectToAction("MasterPage", new { tableId = x });
            }
            else
            {
                return View();
            }

        }

        [HttpPost]
        public ActionResult BatchDelete(FormCollection formCollection)
        {
            MasterPageRepo repo = new MasterPageRepo();
            var RoleTableId = Convert.ToInt32(formCollection[0]);
            List<string> tableName = repo.findTableLookUP(RoleTableId);

            var ids = formCollection[1].Split(',').ToList();
            foreach (var id in ids)
            {
                int x = Convert.ToInt32(id);
                repo.deleteLookUp(tableName[0], x);
            }

            return RedirectToAction("MasterPage", new { tableId = RoleTableId });
        }

        public ActionResult EditGlobalSetting(string id, string value)
        {

            MasterPageRepo repo = new MasterPageRepo();
            bool ans = repo.EditGlobalSetting(id, value);
            return RedirectToAction("MasterPage");
        }

        public ActionResult UpdateRegistration(int id, string Networking, bool coupon)
        {
            MasterPageRepo repo = new MasterPageRepo();
            bool ans = repo.EditRegistration(id, Networking,coupon);

            return RedirectToAction("MasterPage");
        }
        public ActionResult EditEventRole(int id,string role)
        {
            MasterPageRepo repo = new MasterPageRepo();
            bool ans = repo.EditDataEvent(id, role);

            return RedirectToAction("MasterPage");
        }
        public ActionResult DownloadExcel()
        {
            string reportname = $"ConfigurationSettings_{DateTime.Now.ToString("dddd, dd MMMM yyyy")}.xlsx";
            MasterPageRepo repo = new MasterPageRepo();
            var list = repo.getDataGlobalSetting();
            DataTable dt = new DataTable();
            dt.Clear();
            dt.Columns.Add("S.No");
            dt.Columns.Add("Setting");
            dt.Columns.Add("Value");
            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    DataRow dataRow = dt.NewRow();
                    dataRow["S.No"] = item.Id;
                    dataRow["Setting"] = item.Setting;
                    dataRow["Value"] = item.Value;
                    dt.Rows.Add(dataRow);
                }
                var exportbytes = ExportingExcel(dt, reportname);
                return File(exportbytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportname);
            }
            else
            {
                TempData["Message"] = "No Data to Export";
                return RedirectToAction("MasterPage", "MasterPage");
            }
        }
        public byte[] ExportingExcel(DataTable table, string filename)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage pack = new ExcelPackage();
            ExcelWorksheet ws = pack.Workbook.Worksheets.Add(filename);
            ws.Cells["A2"].LoadFromDataTable(table, true, TableStyles.Medium12);
            return pack.GetAsByteArray();
        }
    }
}
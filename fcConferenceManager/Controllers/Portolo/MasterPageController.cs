using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using fcConferenceManager.Models.Portolo;
using OfficeOpenXml;
using System.IO;
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
            return View(masterTable);
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

            bool ans = repo.EditEntryLookUp(eID, updateName, tableName[0]);

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

    }
}
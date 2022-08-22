using fcConferenceManager.Models.Portolo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace fcConferenceManager.Controllers.Portolo
{
    public class PortoloConfigurationController : Controller
    {
        // GET: PortoloConfiguration
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Index2()
        {
            return View();
        }

        
        public ActionResult lookupPartial(int tableId = 0)
        {
            
            List<LookUp> dataLookUp = new List<LookUp>();
            Table_list_PageRepo repo = new Table_list_PageRepo();
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
            return PartialView(dataLookUp);
        }

    }
}
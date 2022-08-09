using fcConferenceManager;
using System;
using System.Web.Mvc;

namespace Controllers
{
    public class WebsiteController : Controller
    {
        // GET: Website
        public ActionResult Index()
        {
            return View();
        }

        public string Updating()
        {
            var result = "Not Updated";
            System.Web.HttpContext.Current.Application.Lock();
            clsSettings cSettings;
            if (System.Web.HttpContext.Current.Application["cSettings"] != null)
                cSettings = (clsSettings)System.Web.HttpContext.Current.Application["cSettings"];
            else
                cSettings = new clsSettings();
            var sqlConn = clsUtility.SqlConnection();
            cSettings.LoadSettings(sqlConn.ConnectionString);
            cSettings.WebsiteUpdateTime = DateTime.Now;
            cSettings.WebsiteUpdating = true;
            System.Web.HttpContext.Current.Application["cSettings"] = cSettings;
            System.Web.HttpContext.Current.Application["IsOffline"] = false;
            System.Web.HttpContext.Current.Application.UnLock();
            result = "Application Updated";
            return result;
        }

        public string Offline()
        {
            var result = "Not Offline";
            if (System.Web.HttpContext.Current.Application["cSettings"] != null)
            {
                System.Web.HttpContext.Current.Application.Lock();
                clsSettings cSettings = (clsSettings)System.Web.HttpContext.Current.Application["cSettings"];
                cSettings.WebsiteUpdateTime = DateTime.Now.AddMinutes(-15);
                cSettings.WebsiteUpdating = true;
                System.Web.HttpContext.Current.Application["cSettings"] = cSettings;
                System.Web.HttpContext.Current.Application["IsOffline"] = true;
                System.Web.HttpContext.Current.Application.UnLock();
                result = "Application Offline";
            }
            return result;
        }

        public string Refresh()
        {
            var result = "Not Refresh";
            if (System.Web.HttpContext.Current.Application["cSettings"] != null)
            {
                System.Web.HttpContext.Current.Application.Lock();
                clsSettings cSettings = (clsSettings)System.Web.HttpContext.Current.Application["cSettings"];
                cSettings.WebsiteUpdateTime = DateTime.MinValue;
                cSettings.WebsiteUpdating = false;
                System.Web.HttpContext.Current.Application["cSettings"] = cSettings;
                System.Web.HttpContext.Current.Application["IsOffline"] = false;
                System.Web.HttpContext.Current.Application.UnLock();
                result = "Application Refreshed";
            }
            return result;
        }
        public string Flag()
        {
            var result = "Not Refresh";
            string fileLoc = System.Web.HttpContext.Current.Server.MapPath("~/Documents/MAGI_Flag.txt");
            if (System.IO.File.Exists(fileLoc))
            {
                System.IO.File.WriteAllText(fileLoc, "1");
                result = "Flag Refreshed";
            }
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace fcConferenceManager.Controllers
{
    public class HtmlController : Controller
    {
        // GET: Public
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult MySchedule()
        {
            return View();
        }

        public ActionResult MySession()
        {
            return View();
        }
        public ActionResult Test()
        {
            return View();
        }
    }
}
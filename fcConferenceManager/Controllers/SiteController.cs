using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Controllers
{
    public class SiteController : Controller
    {
        // GET: Home
        public ActionResult About()
        {
            return View("Index");
        }
    }
}
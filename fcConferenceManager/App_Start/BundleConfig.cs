using System;
using System.Web;
using System.Net;
using System.IO.Compression;
using System.Web.Optimization;

namespace fcConferenceManager.App_Start
{
    public class BundleConfig
    {
        // For more information on Bundling, visit https://go.microsoft.com/fwlink/?LinkID=303951
        public static void RegisterBundles(BundleCollection bundles)
        {
            // bundles.UseCdn = True
            bundles.Add(new ScriptBundle("~/bundles/common").Include("~/Scripts/jquery-1.10.2.min.js", "~/Scripts/Common.js", "~/Scripts/nicescroll.min.js", "~/Scripts/bootbox.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include("~/Scripts/jquery.validate*", "~/Scripts/jquery.validate.unobtrusive.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include("~/Scripts/modernizr-*"));
            bundles.Add(new ScriptBundle("~/bundles/kendo").Include("~/Scripts/kendo.all.min.js", "~/Scripts/kendo.aspnetmvc.min.js", "~/Scripts/kendo.culture.en-IN.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/jszip").Include("~/Scripts/jszip.min.js"));
            bundles.Add(new StyleBundle("~/bundles/kendo-css").Include("~/Content/kendo.material.min.css").Include("~/Content/kendo.common-material.min.css"));
            bundles.Add(new StyleBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap.min.js", "~/Scripts/app.js"));
            bundles.Add(new StyleBundle("~/bundles/common-css").Include("~/Content/bootbox.css").Include("~/Content/styles.css").Include("~/Content/bootstrap.css").Include("~/Content/core.css").Include("~/Content/components.css").Include("~/Content/colors.css"));
        }
    }
}

using System.Web.Mvc;
using System.Web.Routing;

namespace fcConferenceManager
{
    public static class RouteConfig
    {
        public static void RegisterMVCRoutes(RouteCollection routes)
        {
            // Dim settings As FriendlyUrlSettings = New FriendlyUrlSettings()
            // settings.AutoRedirectMode = RedirectMode.Permanent
            // routes.EnableFriendlyUrls(settings)
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.RouteExistingFiles = true;
            // routes.IgnoreRoute("{*allaspx}", New With {Key .allaspx = ".*\.aspx(/.*)?"})
            // routes.IgnoreRoute("forms/{file}.aspx")
            routes.MapRoute(name: "Default", url: "{controller}/{action}/{id}", defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional });       
        }
    }
}
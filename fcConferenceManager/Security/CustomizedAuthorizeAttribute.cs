using System.Web.Mvc;

namespace fcConferenceManager
{
    public class CustomizedAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                string ControllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                string AbsoluteURL = filterContext.HttpContext.Request.Url.AbsolutePath;
                string PathQuery = filterContext.HttpContext.Request.Url.PathAndQuery;
                string ActionNAME = MVCFormAction.GetActionPageName(AbsoluteURL);
                if (!string.IsNullOrEmpty(PathQuery))
                    filterContext.Result = new RedirectResult("/Home/Login?LastPage_URL=" + PathQuery.Trim());
                else
                    filterContext.Result = new RedirectResult("/Home/Login");
                return;
            }
            else
            {
                clsLastUsed cLast = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]);
                clsSettings cSettings = ((clsSettings)System.Web.HttpContext.Current.Session["cSettings"]);
                int EventPKey = 0;
                if (cLast != null && cSettings != null)
                    EventPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
                else if (cLast != null)
                    EventPKey = ((cLast.intEventSelector == -1) ? cLast.intActiveEventPkey : cLast.intEventSelector);

                System.Web.Security.FormsIdentity identity = (System.Web.Security.FormsIdentity)filterContext.HttpContext.User.Identity;
                Models.User_Login data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Models.User_Login>(identity.Ticket.UserData);
                if (data != null)
                {
                    if (data.EventId != EventPKey)
                    {
                        new MAGI_API.Models.SqlOperation().UpdateAuthEvent();
                        filterContext.Result = new RedirectResult(filterContext.HttpContext.Request.Url.PathAndQuery);
                    }
                }
                else
                {
                    //Removing Authentication
                    System.Web.Security.FormsAuthentication.SignOut();
                    filterContext.Result = new RedirectResult("/Home/Login");
                }

            }
        }
    }
}
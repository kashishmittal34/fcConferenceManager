using System.Data.SqlClient;
using System.Web.Mvc;

namespace fcConferenceManager
{
    public static class MVCFormAction
    {
        public static string GetActionPageName(string AbsolutePath)
        {
            string ActionPageName = "";
            switch (AbsolutePath)
            {
                case "/Home/Index": ActionPageName = "Home"; break;
                case "/MAGI/Overview": ActionPageName = "Overview"; break;
                case "/MAGI/Certification": ActionPageName = "CRCP Certification"; break;
                case "/MAGI/Contact": ActionPageName = "Contact MAGI"; break;
                case "/MAGI/UserIssue": ActionPageName = "Report an Issue"; break;
                case "/MAGI/TermsOfUse": ActionPageName = "Website term of use"; break;
                case "/Events/EventInfo": ActionPageName = "Overview"; break;
                case "/Events/Program": ActionPageName = "Program/Agenda"; break;
                case "/Events/Speakers": ActionPageName = "Speakers"; break;
                case "/Events/BeASpeaker": ActionPageName = "Become a Speaker"; break;
                case "/Events/EventSponsors": ActionPageName = "Event Sponsors"; break;
                case "/Events/BecomeSponsor": ActionPageName = "Become an Event Sponsor"; break;
                case "/Events/VenueInfo": ActionPageName = "Venue and Lodging"; break;
                case "/Home/Login": ActionPageName = "Sign into Event"; break;
                case "/Events/ContinueEducation": ActionPageName = "Continuing Education Hours"; break;
                case "/Events/Certification": ActionPageName = "CRCP Certification"; break;
                case "/Events/Participating": ActionPageName = "Participating Organizations"; break;
                case "/Events/IsMAGIRightForYou": ActionPageName = "Is MAGI Right for You?"; break;
                case "/Events/EventTermsandConditions": ActionPageName = "Event Terms & Conditions"; break;
                case "/Events/Advisory": ActionPageName = "Advisory Board"; break;
                case "/Events/Testimonials": ActionPageName = "Testimonials"; break;
                case "/Events/UpcomingEvent": ActionPageName = "Upcoming Events"; break;
                case "/Events/EventContact": ActionPageName = "Contact MAGI"; break;
                case "/Events/FAQs": ActionPageName = "Have a Question?"; break;
                case "/Resources/Journal": ActionPageName = "Journal of Clinical Research Best Practices"; break;
                case "/Resources/Standards": ActionPageName = "MAGI Best Practice Standards"; break;
                case "/Resources/Directories": ActionPageName = "Directories"; break;
                case "/Resources/RegDocs": ActionPageName = "Laws, Regulations & Guidelines"; break;
                case "/Resources/FdaGcp": ActionPageName = "FDA GCP Q&A"; break;
                case "/Resources/Glossary": ActionPageName = "Glossary for Clinical Research"; break;
                case "/Resources/lcfGlossary": ActionPageName = "Glossary for Informed Consent"; break;
                case "/Resources/Milestones": ActionPageName = "Clinical Research Milestones"; break;
                case "/Resources/MagiNewsList": ActionPageName = "MAGI news"; break;
                //case "/ViewAccount?PType=Self": ActionPageName = "My profile"; break;
                case "/MyMAGI/MyOrganization": ActionPageName = "My Organization"; break;
                //case "/ViewAccount?Spk=Self": ActionPageName = "My speaking interests"; break;
                case "/MyMAGI/MyHistory": ActionPageName = "My History"; break;
                case "/MyMAGI/MyPayments": ActionPageName = "My charges & payments"; break;
                case "/MyMAGI/MyCertificate": ActionPageName = "My Certificates"; break;
                case "/MyMAGI/MyStaffPage": ActionPageName = "My staff console"; break;
                //case "/MyMAGI/MyConsole": ActionPageName = "My Console"; break;
                case "/MyMAGI/MyConference": ActionPageName = "My event summary"; break;
                case "/MyMAGI/MyOptions": ActionPageName = "Conference options"; break;
                case "/MyReminders": ActionPageName = "My reminders"; break;
                case "/MyMAGI/MySchedule": ActionPageName = "My Schedule"; break;
                case "/MyMAGI/MySession": ActionPageName = "My Session"; break;
                case "/MyMAGI/MyConferenceBook": ActionPageName = "My book & materials"; break;
                case "/MyNetworking": ActionPageName = "My network "; break;
                case "/MyMAGI/MyGroupChat": ActionPageName = "My group chat"; break;
                case "/MeetingPlanner": ActionPageName = "My meetings"; break;
                case "/MyMAGI/SingleSession": ActionPageName = "My session(s)"; break;
                case "/MySpeakerDinner": ActionPageName = "My speaker dinner"; break;
                case "/MyMAGI/MyFAQs": ActionPageName = "My FAQs"; break;
                //case "/FacultyResource": ActionPageName = "Faculty resource center"; break;
                case "/Virtual/EventOnCloud": ActionPageName = "Event On Cloud"; break;
                case "/Virtual/EducationCenter": ActionPageName = "Education Center"; break;
                case "/Virtual/ResourceSupportCenter": ActionPageName = "Resource Support Center"; break;
                case "/Virtual/NetworkingLounge": ActionPageName = "Networking Lounge"; break;
                case "/Virtual/ScheduledEvent": ActionPageName = "Main Schedule Of Events"; break;
                case "/Virtual/ShowNews": ActionPageName = "Show News"; break;
                case "/Portolo/MyFiles": ActionPageName = "MyFiles"; break;
                case "/Global/AppSettings": ActionPageName = "Application Settings"; break;


            }
            return ActionPageName;
        }
        public static bool ChangedInformation(Models.User_Login data)
        {
            bool result = false;
            clsAccount cAccount = ((clsAccount)System.Web.HttpContext.Current.Session["cAccount"]);
            if (cAccount != null && cAccount.intAccount_PKey>0)
            {
                return (data.FirstName != cAccount.strFirstname |
                        data.LastName != cAccount.strLastname |
                        data.GlobalAdmin != cAccount.bGlobalAdministrator |
                        data.StaffMember != cAccount.bStaffMember |
                        data.Email != cAccount.strEmail |
                        data.Organization != cAccount.strOrganizationID |
                        data.NickName != cAccount.strNickname |
                        data.MiddleName != cAccount.strMiddlename);
            }

            return result;
        }
    }
    public class CheckActiveEventAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
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
                    bool IsChanged = MVCFormAction.ChangedInformation(data);
                    if (IsChanged)
                    {
                        new MAGI_API.Models.SqlOperation().UpdateAuthEvent();
                        filterContext.Result = new RedirectResult(filterContext.HttpContext.Request.Url.PathAndQuery);
                    }
                    else if (data.EventId != EventPKey)
                    {
                        new MAGI_API.Models.SqlOperation().UpdateAuthEvent();
                        filterContext.Result = new RedirectResult(filterContext.HttpContext.Request.Url.PathAndQuery);
                    }
                }
            }
        }
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (((ReflectedActionDescriptor)filterContext.ActionDescriptor).MethodInfo.ReturnType.Name == "ActionResult")
            {
                SqlConnection sqlConn = new SqlConnection((string)System.Web.HttpContext.Current.Session["sqlConn"]);
                string ControllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                string AbsoluteURL = filterContext.HttpContext.Request.Url.AbsolutePath;
                string ActionNAME = MVCFormAction.GetActionPageName(AbsoluteURL);
                System.Web.HttpRequest httpRequest = System.Web.HttpContext.Current.Request;
                clsFormList cFormList = ((clsFormList)System.Web.HttpContext.Current.Session["cFormlist"]);
                if (!string.IsNullOrEmpty(ActionNAME))
                    cFormList.LoadPage(sqlConn, null, httpRequest, ActionNAME, ControllerName, filterContext.HttpContext.Request.QueryString, bMVC: true, MVCUrl: AbsoluteURL);
            }
        }
    }
}
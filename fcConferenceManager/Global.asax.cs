using System;
using System.IO;
using System.Data;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Configuration;
using System.Web;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using System.Security.Principal;
using fcConferenceManager.Models;
using Microsoft.AspNet.SignalR;

namespace fcConferenceManager
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(110);
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(3600);
            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(1200);
            // Fires when the application is started
            // AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            // FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            RouteConfig.RegisterMVCRoutes(RouteTable.Routes);
            // BundleTable.EnableOptimizations = True;
            // BundleConfig.RegisterBundles(BundleTable.Bundles);
            // ASPOSE Instantiate License class and call its SetLicense method to use the license
            string strPhysicalPath = Server.MapPath("~/Licenses/Aspose.Total.lic");
            Aspose.Pdf.License plicense = new Aspose.Pdf.License();
            if (plicense != null)
                plicense.SetLicense(strPhysicalPath);
            Aspose.Words.License wlicense = new Aspose.Words.License();
            if (wlicense != null)
                wlicense.SetLicense(strPhysicalPath);
            Aspose.Slides.License slicense = new Aspose.Slides.License();
            if (slicense != null)
                slicense.SetLicense(strPhysicalPath);
            Aspose.Cells.License clicense = new Aspose.Cells.License();
            if (clicense != null)
                clicense.SetLicense(strPhysicalPath);
            InitApplicationInfo2();  //InitApplicationInfo();            
        }

        private void InitApplicationInfo2()
        {
            var ConnectionString = ReadConnectionString();
            Application.Lock();
            Application["cImages"] = clsImg.LoadImages(ConnectionString);
            Application.UnLock();
        }
		
        private void InitApplicationInfo()
        {
            Application.Lock();
            var ConnectionString = ReadConnectionString();
            Application["sqlConn"] = ConnectionString;
            Application["cImages"] = clsImg.LoadImages(ConnectionString);

            clsSettings cSettings = new clsSettings();
            clsAccount cAccount = new clsAccount();
            clsLastUsed cLastUsed = new clsLastUsed();
            clsFormList cFormList = new clsFormList();

            cSettings.LoadSettings(ConnectionString);
            //cSettings.LoadPrimarySeries(ConnectionString);
            //cSettings.LoadPrimaryEvent(ConnectionString); 
            cLastUsed.intActiveEventPkey = cSettings.intPrimaryEvent_pkey;
            cLastUsed.strinterestedEventID_Management = cSettings.intPrimaryEvent_pkey.ToString();
            cLastUsed.strActiveEvent = cSettings.strPrimaryEventID;
            cLastUsed.intEventType_PKey = cSettings.intPrimaryEventType_pkey;

            Application["cSettings"] = cSettings;
            Application["cAccount"] = cAccount;
            Application["cLastUsed"] = cLastUsed;
            Application["cFormList"] = cFormList;

            string version = clsSettings.APP_VERSION;
            Application["Version"] = version;
            bool bBlueRibbonMode = (Convert.ToInt32(ReadAppSetting("BRMode")) == 1);
            Application["BlueRibbonMode"] = bBlueRibbonMode;
            Application.UnLock();
        }

        public string ReadConnectionString()
        {
            string connString = "";
            // --read web.config connection settings
            string tempString;
            // --data source
            tempString = ReadAppSetting("AppS");
            // tempString = "Jen-PC" 'Me.ReadAppSetting("DataSource")
            // tempString = "184.168.194.77" 'Me.ReadAppSetting("DataSource")
            // tempString = "DESKTOP-41N9PO5" 'Me.ReadAppSetting("DataSource")
            if (tempString.Length == 0)
            {
                throw new Exception("Missing Connection Data Source");
            }
            connString = string.Format("Data Source={0};", tempString);
            // --user id
            tempString = ReadAppSetting("AppL");
            // tempString = "kws" 'Me.ReadAppSetting("UserID")
            if (tempString.Length == 0)
            {
                throw new Exception("Missing Connection User ID");
            }
            connString += string.Format("Uid={0};", tempString);
            // --password
            tempString = ReadAppSetting("AppP");
            // tempString = "kws" 'Me.ReadAppSetting("Password")
            // tempString = "Atbuck3385!" 'Me.ReadAppSetting("Password")
            connString += string.Format("Pwd={0};", tempString);
            tempString = ReadAppSetting("AppDB");
            if (tempString.Length == 0)
            {
                throw new Exception("Missing Connection Database Name");
            }
            connString += string.Format("Database={0};", tempString);
            // --database timeout secs
            int dbTimeoutSecs = Convert.ToInt32(ReadAppSetting("AppT"));
            if (dbTimeoutSecs <= 0)
                dbTimeoutSecs = 120;
            connString += string.Format("Connect Timeout={0};", dbTimeoutSecs);
            connString += string.Format("MultipleActiveResultSets={0};", "true");
            // --success, return connection string
            return connString;
        }

        public string ReadAppSetting(string settingKey)
        {
            // --attempt to read setting
            string settingValue = ConfigurationManager.AppSettings[settingKey].ToString();
            if (settingValue == null)
                settingValue = "";
            settingValue = settingValue.Trim();
            // --return
            return settingValue;
        }

        private void RegisterRoutes(RouteCollection routes)
        {
            routes.RouteExistingFiles = true;
            //routes.MapPageRoute("DefaultPage", "default.aspx", "~/default.aspx", true);
            var filePath = Server.MapPath("~/SiteURL.xml");
            if (!File.Exists(filePath))
            {
                DataSet ds = new DataSet();
                ds.ReadXml(filePath);
                char[] array = new char[2];
                array[0] = '/';
                array[1] = ' ';
                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        if (row["IsActive"].ToString() == "true")
                        {
                            var urlKey = row["PaddedID"].ToString();
                            var isRedirect = row["IsRedirect"].ToString();
                            var urlTitle = row["PageTitle"].ToString().TrimStart(array);
                            var oldURL = row["OldUrl"].ToString().TrimStart(array);
                            var newURL = row["RedirectUrl"].ToString().TrimStart(array);
                            if (oldURL != string.Empty && newURL != string.Empty && urlTitle != string.Empty)
                            {
                                if (isRedirect.ToLower() == "true")
                                {
                                    if (newURL.ToLower().IndexOf("http") > -1)
                                        routes.MapPageRoute("Page" + urlKey, oldURL, "~/" + oldURL, false, null, null, new RouteValueDictionary() { { "RoutePage", newURL } });
                                    else
                                        routes.MapPageRoute("Page" + urlKey, oldURL, "~/" + newURL, false, null, null, new RouteValueDictionary() { { "RoutePage", newURL } });
                                }
                                else
                                {
                                    routes.MapPageRoute("Title" + urlKey, newURL, "~/" + oldURL, true, null, null, new RouteValueDictionary() { { "RouteTitle", urlTitle } });
                                    var checkURL = oldURL.Replace("forms", "").Replace("/frm", "").Replace(".aspx", "");
                                    if (checkURL != oldURL && checkURL != newURL)
                                        routes.MapPageRoute("Page" + urlKey, checkURL, "~/" + newURL, false, null, null, new RouteValueDictionary() { { "RoutePage", newURL } });
                                }
                            }
                        }
                    }
                }
            }
            routes.Ignore("{file}.axd");
            routes.Ignore("{file}.ashx");
            routes.Ignore("{file}.aspx");
            routes.Ignore("forms/Telerik.{file}");
            routes.Ignore("forms/ChartImage.{file}");
            routes.MapPageRoute("RoutePage", "{file}", "~/forms/frm{file}.aspx", true);
            routes.MapPageRoute("FormPage", "forms/{file}.aspx", "~/forms/frm{file}.aspx", true);
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            ////////List of logged in users from dictionary
            //var loggedInUsers = (Dictionary<int, string>)Application["LoggedInUsers"];

            //if (System.Web.HttpContext.Current.Session != null)
            //{
            //    if (Session["cAccount"] != null &&
            //    ((clsAccount)System.Web.HttpContext.Current.Session["cAccount"])
            //    .intAccountStatus_pKey == 1
            //    &&
            //    (((clsAccount)System.Web.HttpContext.Current.Session["cAccount"]).intAccount_PKey>0)
            //    )
            //    {
            //        ///////////////Updating the Current Authentic user
            //        var intKey = ((clsAccount)System.Web.HttpContext.Current.Session["cAccount"]).intAccount_PKey;
            //        if (loggedInUsers != null && loggedInUsers.ContainsKey(intKey))
            //        {
            //            loggedInUsers[intKey] = Session.SessionID;
            //            Application["LoggedInUsers"] = loggedInUsers;
            //        }
            //    }                
            //}
            InitSessionInfo2();  //InitSessionInfo();            
        }

        protected void Session_End(object sender, EventArgs e)
        {
            MAGI_API.Models.OnlineUsers.RemoveBySession(Session.SessionID);

            ////////List of logged in users from dictionary
            //var loggedInUsers = (Dictionary<int, string>)Application["LoggedInUsers"];
            //if (loggedInUsers != null)
            //{
            //    foreach (var item in loggedInUsers.ToList())
            //    {
            //        //int flag = 0;
            //        //foreach(var v in Sessions)
            //        //{
            //        //    if (item.Value == v)
            //        //    {
            //        //        flag = 1;
            //        //    }
            //        //}

            //        if(item.Value == Session.SessionID)
            //        {
            //            loggedInUsers.Remove(item.Key);
            //        }
            //    }
            //    Application["LoggedInUsers"] = loggedInUsers;
            //}
            CloseSQLConnection();
        }
		
        private void InitSessionInfo2()
        {
            // --application version
            Session.Timeout = 180; // --minutes
            // --initialize the sql connection
            var ConnectionString = this.ReadConnectionString();
            Session["sqlConn"] = ConnectionString;
            // --persistant classes
            Session["cLastUsed"] = new clsLastUsed();
            Session["cAccount"] = new clsAccount();
            Session["cFormList"] = new clsFormList();
            Session["cSettings"] = new clsSettings();
            Session["Version"] = clsSettings.APP_VERSION;
            // --load settings
            ((clsSettings)Session["cSettings"]).LoadSettings(ConnectionString);
            ((clsSettings)Session["cSettings"]).LoadPrimarySeries(ConnectionString);        // --load initial series
            ((clsSettings)Session["cSettings"]).LoadPrimaryEvent(ConnectionString);        // --load initial event
            ((clsLastUsed)Session["cLastUsed"]).intActiveEventPkey = ((clsSettings)Session["cSettings"]).intPrimaryEvent_pkey;
            ((clsLastUsed)Session["cLastUsed"]).strActiveEvent = ((clsSettings)Session["cSettings"]).strPrimaryEventID;
            ((clsLastUsed)Session["cLastUsed"]).intEventType_PKey = ((clsSettings)Session["cSettings"]).intPrimaryEventType_pkey;

            bool bBlueRibbonMode = (Convert.ToInt32(ReadAppSetting("BRMode")) == 1);
            Session["BlueRibbonMode"] = bBlueRibbonMode;
        }
		
        private void InitSessionInfo()
        {
            Session.Timeout = 180;
            var ConnectionString = Application["sqlConn"].ToString();
            Session["sqlConn"] = ConnectionString;

            clsSettings cSettings = ((clsSettings)Application["cSettings"]).Copy();
            clsAccount cAccount = ((clsAccount)Application["cAccount"]).Copy();
            clsLastUsed cLastUsed = ((clsLastUsed)Application["cLastUsed"]).Copy();
            clsFormList cFormList = ((clsFormList)Application["cFormList"]).Copy();

            //cSettings.LoadSettings(ConnectionString);
            cSettings.LoadPrimarySeries(ConnectionString);        // --load initial series
            cSettings.LoadPrimaryEvent(ConnectionString);        // --load initial event
            //cLastUsed.intActiveEventPkey = cSettings.intPrimaryEvent_pkey;
            //cLastUsed.strActiveEvent = cSettings.strPrimaryEventID;
            //cLastUsed.intEventType_PKey = cSettings.intPrimaryEventType_pkey;

            Session["cSettings"] = cSettings;
            Session["cAccount"] = cAccount;
            Session["cLastUsed"] = cLastUsed;
            Session["cFormList"] = cFormList;
			
            string version = Application["Version"].ToString();
            Session["Version"] = version;
            bool bBlueRibbonMode = (bool)Application["BlueRibbonMode"];
            Session["BlueRibbonMode"] = bBlueRibbonMode;
        }

        private void CloseSQLConnection()
        {
            //
        }

        public void Application_BeginRequest(object sender, EventArgs e)
        {
            if (Request.AppRelativeCurrentExecutionFilePath == "~/")
                HttpContext.Current.Response.Redirect("~/default.aspx");
            if (Application["IsOffline"] != null && Convert.ToBoolean(Application["IsOffline"]) == true)
                HttpContext.Current.Response.Redirect("~/Offline.htm");
            string currentMethod = HttpContext.Current.Request.HttpMethod;
            if (currentMethod == "GET")
            {
                string currentRequest = HttpContext.Current.Request.Headers.Get("x-microsoftajax") ?? "";
                if (currentRequest != "Delta=true")
                {
                    string currentContent = HttpContext.Current.Request.ContentType;
                    if (currentContent != "application/json")
                    {
                        var cURL = HttpContext.Current.Request.Url.AbsolutePath;
                        string currentURL = cURL.ToLower();
                        string currentQuery = HttpContext.Current.Request.Url.Query;
                        if (currentURL.Length > 1 && currentURL != "/default.aspx")
                        {
                            if (!(currentURL.Contains("/telerik.") || currentURL.Contains("/chartimage.") || currentURL.Contains("/c/")))
                            {
                                if (currentURL.Contains("/forms") || currentURL.Contains("/frm") || currentURL.Contains(".aspx"))
                                    HttpContext.Current.Response.Redirect("~/" + cURL.Replace("/Forms", "").Replace("/forms", "").Replace("/frm", "").Replace(".aspx", "") + currentQuery);
                                else
                                {
                                    var currentContext = new HttpContextWrapper(HttpContext.Current);
                                    var RouteData = System.Web.Routing.RouteTable.Routes.GetRouteData(currentContext);
                                    if (RouteData != null && RouteData.DataTokens.Count > 0 && RouteData.DataTokens.ContainsKey("RoutePage"))
                                    {
                                        if (RouteData.DataTokens["RoutePage"].ToString().ToLower().IndexOf("http") > -1)
                                            HttpContext.Current.Response.Redirect(RouteData.DataTokens["RoutePage"].ToString() + currentQuery);
                                        else
                                            HttpContext.Current.Response.Redirect("~/" + RouteData.DataTokens["RoutePage"].ToString() + currentQuery);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            //var cURL = HttpContext.Current.Request.Url.AbsolutePath;
            //string currentURL = cURL.ToLower();
            //if (currentURL.Contains("/r/"))
            //{
            //    var strRedirect = FindTrackUrl(currentURL);
            //    if (strRedirect != "")
            //        HttpContext.Current.Response.Redirect("~/" + strRedirect);
            //}
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (authTicket != null && !authTicket.Expired)
                {
                    FormsIdentity id = (FormsIdentity)HttpContext.Current.User.Identity;
                    FormsAuthenticationTicket ticket = id.Ticket;
                    string userData = ticket.UserData;
                    string[] roles = userData.Split(',');
                    HttpContext.Current.User = new GenericPrincipal(id, roles);
                }
            }
        }

        public void Application_AcquireRequestState(object sender, EventArgs e)
        {
            var cURL = HttpContext.Current.Request.Url.AbsolutePath;
            string currentURL = cURL.ToLower();
            if (currentURL.Length > 1 && currentURL != "/default.aspx" && !currentURL.Contains("/public/") && Session["cSettings"] == null)
            {
                HttpContext.Current.Response.Redirect("~/default.aspx");
            }
            if (currentURL.Contains("/r/"))
            {
                var strRedirect = FindTrackUrl(currentURL);
                if (strRedirect != "")
                {
                    if (strRedirect.ToLower().IndexOf("http") > -1)
                        HttpContext.Current.Response.Redirect(strRedirect);
                    else
                        HttpContext.Current.Response.Redirect("~" + strRedirect);
                }
            }
            //////////////////////////////////////////////////////

            //try
            //{
            //    HttpBrowserCapabilities bc = Request.Browser;
            //    string strBrowser = bc.Browser;
            //    string strIpAddress = string.Empty;
            //    bool isloggedIn = false;
            //    string strUserEmail = string.Empty;
            //    string ConnectionString = string.Empty;

            //    if (System.Web.HttpContext.Current.Session != null)
            //    {
            //        if((Session["cAccount"] != null) &&
            //        (((clsAccount)Session["cAccount"]).intAccountStatus_pKey == 1) &&
            //        (((clsAccount)Session["cAccount"]).strEmail != ""))
            //        {
            //            isloggedIn = true;
            //            strUserEmail = ((clsAccount)Session["cAccount"]).strEmail;
            //        }

            //        if(Session["sqlConn"] != null)
            //        {
            //            ConnectionString = Session["sqlConn"].ToString();
            //        }
            //    }

            //    if(ConnectionString != "")
            //    {
            //        if (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
            //        {
            //            strIpAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
            //        }
            //        else if (HttpContext.Current.Request.UserHostAddress.Length != 0)
            //        {
            //            strIpAddress = HttpContext.Current.Request.UserHostAddress;
            //        }

            //        Visitor v = new Visitor()
            //        {
            //            broswerName = strBrowser,
            //            ipAddress = strIpAddress,
            //            isLoggedIn = isloggedIn,
            //            userID = strUserEmail,
            //            pageURL = Request.Url.ToString()
            //        };
            //        InsertVisitorLog(v, ConnectionString);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Response.Write(ex.Message);
            //}
        }
        private string FindTrackUrl(string OldURL)
        {
            string returnUrl = "";
            var UserName = "N/A";
            var strIP = "N/A";
            if (HttpContext.Current.Session["cAccount"] != null)
            {
                clsAccount cAccount = (clsAccount)HttpContext.Current.Session["cAccount"];
                if (cAccount.intAccount_PKey > 0)
                    UserName = cAccount.strFullName;
            }
            if (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
                strIP = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
            else if (HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"] != null)
                strIP = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();
            var ConnectionString = HttpContext.Current.Session["sqlConn"].ToString();
            SqlConnection sqlConn = new SqlConnection(ConnectionString);
            DataTable dtTrack = new DataTable();
            string qry = "SELECT * FROM URLREDIRECT WHERE OldUrl=@OldUrl AND ISNULL(ISTRACK,0)=1 AND ISNULL(ISACTIVE,0)=1";
            SqlCommand cmd = new SqlCommand(qry);
            cmd.Parameters.AddWithValue("@OldUrl", OldURL);
            if (clsUtility.GetDataTable(sqlConn, cmd, ref dtTrack))
            {
                if (dtTrack.Rows.Count > 0)
                {
                    var TrackType = dtTrack.Rows[0]["TrackType"].ToString();
                    clsLastUsed cLast = (clsLastUsed)HttpContext.Current.Session["cLastUsed"];
                    if (TrackType == "D")
                    {
                        var dblDiscountCode = OldURL.ToLower().Replace("/r/", "").Replace("event", "").Replace("home", "").Replace("registration", "");
                        if (dblDiscountCode.Contains("/"))
                            dblDiscountCode = dblDiscountCode.Substring(0, dblDiscountCode.IndexOf("/") - 1);
                        qry = string.Format("SELECT * FROM DISCOUNT_LIST WHERE DISCOUNTID = '{0}' AND EVENT_PKEY = {1}", dblDiscountCode, cLast.intActiveEventPkey);
                        cmd = new SqlCommand(qry);
                        DataTable dtDiscount = new DataTable();
                        if (clsUtility.GetDataTable(sqlConn, cmd, ref dtDiscount))
                        {
                            if (dtDiscount.Rows.Count > 0)
                                cLast.strDiscountCode = dblDiscountCode;
                        }
                    }
                    returnUrl = dtTrack.Rows[0]["RedirectUrl"].ToString();
                    qry = "INSERT INTO TRACK_LIST(URL ,USERNAME,IP,VISIT_DATE,Event_pkey)";
                    qry = qry + Environment.NewLine + "VALUES(@URL,@UserName,@IP,@Date,@Event_pkey)";
                    cmd = new SqlCommand(qry);
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = sqlConn;
                    cmd.Parameters.AddWithValue("@URL", OldURL);
                    cmd.Parameters.AddWithValue("@UserName", UserName);
                    cmd.Parameters.AddWithValue("@IP", strIP);
                    cmd.Parameters.AddWithValue("@Date", DateTime.UtcNow);
                    cmd.Parameters.AddWithValue("@Event_pkey", cLast.intActiveEventPkey);
                    clsUtility.ExecuteQuery(cmd, null, "");
                }
            }
            return returnUrl;
        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
        protected void Application_EndRequest()
        {

        }
        /// <summary>
        /// /////To Insert user's visits
        /// </summary>
        private void InsertVisitorLog(Visitor vs, string conStr)
        {
            try
            {
                if (vs != null)
                {
                    SqlConnection sqlConn = new SqlConnection(conStr);
                    SqlCommand cmd = new SqlCommand("InsertUserLog", sqlConn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@isLoggedIn", vs.isLoggedIn);
                    cmd.Parameters.AddWithValue("@userID", vs.userID);
                    cmd.Parameters.AddWithValue("@ipAddress", vs.ipAddress);
                    cmd.Parameters.AddWithValue("@broswerName", vs.broswerName);
                    cmd.Parameters.AddWithValue("@pageURL", vs.pageURL);
                    sqlConn.Open();
                    cmd.ExecuteNonQuery();
                    sqlConn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}
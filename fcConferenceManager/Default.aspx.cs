using System;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace fcConferenceManager
{
    public partial class DefaultE : System.Web.UI.Page
    {
        private clsSettings cSettings;
        private clsAccount cAccount;
        private SqlConnection sqlConn;
        private clsLastUsed cLast;

        protected void Page_PreInit(object sender, EventArgs e)
        {
            cmdLoginCancel.Click += cmdLoginCancel_Click;
            cmdLoginSave.Click += cmdLoginSave_Click;

        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["cSettings"] == null || Session["cAccount"]==null || Session["sqlConn"] == null ||Session["cLastUsed"] == null)
            {
                InitSessionInfo2();
                
            }
            
            cAccount = (clsAccount)Session["cAccount"];
            cSettings = (clsSettings)Session["cSettings"];
            sqlConn = new SqlConnection(Session["sqlConn"].ToString());
            cLast = (clsLastUsed)Session["cLastUsed"];
            String site = ConfigurationManager.AppSettings["AppName"];
            if (!Page.IsPostBack)
            {
                if (!site.ToUpper().Contains("ELIMAR"))
                {
                    ////if (Application["cImages"] != null && Session["cSettings"] != null)
                    ////    Response.Redirect("~/Home");
                    ////else
                        btnSubmit.Visible = true;
                    //Response.Redirect("~/Home/Index");

                }
                else
                {
                    cLast.intIssueTab = 0;

                    if (site.ToUpper().Contains("ELIMAR") && cAccount.intAccount_PKey > 0)
                    {
                        //Response.Redirect("Home");
                    }

                    if (Session["cSettings"] != null)
                    {
                        lblContent.Text = clsReservedWords.ReplaceCurrent(null, clsSettings.ReplaceTermsGeneral(cSettings.getText(210)));
                        if (Application["cImages"] != null)
                        {
                            Dictionary<int, clsImg> dct = (Dictionary<int, clsImg>)Application["cImages"];
                            if (dct.ContainsKey(clsImages.IMG_1))
                            {
                                imgLogoHeader.ImageUrl = dct[clsImages.IMG_1].strPath;
                            }
                        }
                    }

                    else
                    {
                        InitSessionInfo2();
                        cAccount = (clsAccount)Session["cAccount"];
                        cSettings = (clsSettings)Session["cSettings"];
                        sqlConn = new SqlConnection(Session["sqlConn"].ToString());

                        if (Session["cSettings"] != null)
                            lblContent.Text = clsReservedWords.ReplaceCurrent(null, clsSettings.ReplaceTermsGeneral(cSettings.getText(210)));
                        Dictionary<int, clsImg> dct = (Dictionary<int, clsImg>)Application["cImages"];
                        if (dct.ContainsKey(clsImages.IMG_1))
                        {
                            imgLogoHeader.ImageUrl = dct[clsImages.IMG_1].strPath;
                        }
                    }

                }

            }
            if (Application["cImages"] != null)
            {
                Dictionary<int, clsImg> dct = (Dictionary<int, clsImg>)Application["cImages"];
                if (dct.ContainsKey(clsImages.IMG_1))
                {
                    imgLogo.ImageUrl = dct[clsImages.IMG_1].strPath;
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (Application["cImages"] == null)
                InitApplicationInfo2();  //InitApplicationInfo
            if (Session["cSettings"] == null)
                InitSessionInfo2();  //InitSessionInfo
            //if (Application["cImages"] != null && Session["cSettings"] != null)
                //Response.Redirect("~/Login", false);
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



        protected void cmdLoginOpen_Click(object sender, EventArgs e)
        {

            PrepareLoginScreen();
            string script = "function f(){OpenLoginPopup(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);";
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, true);

        }
        private void PrepareLoginScreen()
        {
            ckRemember.Checked = true;
            lblFailureText.Visible = false;
            lblPasswordRequired.Visible = false;

            txtPassword.PasswordStrengthSettings.RequiresUpperAndLowerCaseCharacters = true;
            txtPassword.PasswordStrengthSettings.PreferredPasswordLength = cSettings.intLoginPWLength;
            txtPassword.PasswordStrengthSettings.MinimumSymbolCharacters = 1;
            txtPassword.PasswordStrengthSettings.MinimumNumericCharacters = 1;

            if (Request.QueryString["Reg"] != null)
                txtUserName.Text = clsUtility.Decrypt(Request.QueryString["Reg"]);
            else if (Request.Cookies["UserName"] != null)
                txtUserName.Text = Request.Cookies["UserName"].Value;

        }

        protected void cmdLoginSave_Click(object sender, EventArgs e)
        {
            // --reset the error flag
            bool bError = false;
            lblFailureText.Visible = false;
            cAccount.Logout();
            // --get entries
            string strUserID = txtUserName.Text.Trim();
            string strPW = txtPassword.Text.Trim();
            lblPasswordRequired.Visible = (strPW == "");

            // --if no pw or user then exit
            if (strPW == "" | strUserID == "")
                return;
            int intAccountBeingChecked = 0;
            cAccount.sqlConn = sqlConn;
            cAccount.lblMsg = lblFailureText;
            int intResult = cAccount.AuthenticateLogin(strUserID, strPW, Request, ref intAccountBeingChecked);

            switch (intResult)
            {
                case object _ when intResult > 0:
                    {
                        // --if still here, then a valid user login - now try and load
                        cAccount.intAccount_PKey = intResult;
                        if (!cAccount.LoadAccount())
                            return;

                        String site = ConfigurationManager.AppSettings["AppName"];                        
                        if (site.ToUpper().Contains("ELIMAR") && !(cAccount.bStaffMember == true || cAccount.bGlobalAdministrator == true))
                        {
                            cAccount.Logout();
                            return;
                        }
                        cAccount.LogAuditMessage("Logged in", clsAudit.LOG_Login);
                        // --save to session
                        Session["cAccount"] = cAccount;
                        // --save id in cookie
                        Response.Cookies["UserName"].Expires = DateTime.Now.AddDays((ckRemember.Checked ? 30 : -1));
                        Response.Cookies["UserName"].Value = strUserID;
                        // --initialize default event (this references lastlogin - so do this prior to setting last login date)
                        cAccount.getDefaultEvent();
                        // --log last login
                        cAccount.SetLastLogin();
                        if (cAccount.intAccountStatus_pKey != 1)
                        {
                            cAccount.intAccountStatus_pKey = 1;
                            cAccount.bGetJournal = true;
                            clsAccount.ListrackSubscription(cAccount, false, "");
                        }
                        // --success                       
                        string script = "function f(){CloseLoginPopup(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);";
                        ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, true);
                        //Response.Redirect("Home");
                        break;
                    }

                default:
                    {
                        bError = true;
                        break;
                    }
            }

            // --if here then there was an error, display message
            int intErrCode = 234; // --technical
            switch (intResult)
            {
                case 0:
                case -3 // --invalid account or password
               :
                    {
                        intErrCode = 233;
                        break;
                    }

                case -2 // --multiple accounts found
         :
                    {
                        intErrCode = 232;
                        break;
                    }

                case -4 // --inactive
         :
                    {
                        intErrCode = 236;
                        break;
                    }

                case -5 // --locked out
         :
                    {
                        intErrCode = 237;
                        break;
                    }

                case -10:
                    {

                        clsUtility.InjectAlert(ScriptManager.GetCurrent(Page), Page, "Your account has been blocked. Contact us at " + cSettings.strSupportEmail.ToString());
                        return;
                    }
            }

            string strLabel = clsUtility.getErrorMessage(intErrCode);

            // --display message
            lblFailureText.Visible = true;
            lblFailureText.Text = strLabel;
        }

        private void cmdLoginCancel_Click(object sender, EventArgs e)
        {
            string script = "function f(){CloseLoginPopup(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);";
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, true);
        }

    }
}
using System;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace fcConferenceManager
{
    public partial class DefaultM : System.Web.UI.Page
    {        
        protected void Page_PreInit(object sender, EventArgs e)
        {
            //cmdLoginCancel.Click += cmdLoginCancel_Click;
            //cmdLoginSave.Click += cmdLoginSave_Click;

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (Application["cImages"] != null && Session["cSettings"] != null)
                    Response.Redirect("~/Home");
                else
                    btnSubmit.Visible = true;
            }
        }        

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (Application["cImages"] == null)
                InitApplicationInfo2();  //InitApplicationInfo
            if (Session["cSettings"] == null)
                InitSessionInfo2();  //InitSessionInfo
            if (Application["cImages"] != null && Session["cSettings"] != null)
                Response.Redirect("~/Login", false);
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

    }
}
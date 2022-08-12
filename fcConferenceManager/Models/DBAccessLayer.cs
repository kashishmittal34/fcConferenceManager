using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using MAGI_API.Models;
using fcConferenceManager.Models.ViewModels;
using System.Threading.Tasks;
using Twilio.Http;
using Elimar.Models;

namespace fcConferenceManager.Models
{
    public class DBAccessLayer
    {

        SqlConnection con = new SqlConnection(ReadConnectionString());

        internal static string ReadConnectionString()
        {
            string connString = string.Format("Data Source={0};", ConfigurationManager.AppSettings["AppS"].ToString());
            connString += string.Format("Uid={0};", ConfigurationManager.AppSettings["AppL"].ToString());
            connString += string.Format("Pwd={0};", ConfigurationManager.AppSettings["AppP"].ToString());
            connString += string.Format("Database={0};", ConfigurationManager.AppSettings["AppDB"].ToString());
            connString += string.Format("Connect Timeout={0};", ConfigurationManager.AppSettings["AppT"].ToString());
            connString += string.Format("MultipleActiveResultSets={0};", "true");
            return connString;
        }

        public byte[] ConvertToBytes(HttpPostedFileBase image)
        {
            byte[] imageBytes = null;
            BinaryReader reader = new BinaryReader(image.InputStream);
            imageBytes = reader.ReadBytes((int)image.ContentLength);
            return imageBytes;
        }

        byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }


        #region MAGI 

        public string FETCHFormData(string formname)
        {
            string formtext = string.Empty;
            string dbquery = "Select AppTextBlock  from Application_Text where Form = @Form";
            SqlCommand cmd = new SqlCommand(dbquery, con);
            cmd.Parameters.AddWithValue("@Form", formname);
            con.Open();
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                if (oReader.Read())
                {
                    formtext = oReader["AppTextBlock"].ToString();
                }
            }
            con.Close();

            return formtext;
        }

        public DataTable GetIssueType()
        {
            DataTable _dt = new DataTable();
            try
            {
                string dbquery = "SELECT pKey,IssueCategoryID as IssueType";
                dbquery = dbquery + Environment.NewLine + "FROM Sys_IssueCategories";
                dbquery = dbquery + Environment.NewLine + "where Active = 1";
                dbquery = dbquery + Environment.NewLine + "Order by isNull(Sortorder, 999),IssueType;";
                con.Open();
                SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
                _da.Fill(_dt);
                con.Close();
            }
            catch (Exception ex)
            { }
            finally
            {
                con.Close();
            }
            return _dt;
        }

        public DataTable GetAudienceTypes()
        {
            DataTable dt = new DataTable();
            try
            {
                string dbquery = "Select PA.pKey,PA.Audience_Id as strText,PA.IsActive From SYS_PrimaryAudience PA Order by strText;";
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter(dbquery, con);
                sda.Fill(dt);
                con.Close();
            }
            catch
            {
                throw;
            }
            finally
            {
                con.Close();
            }
            return dt;
        }

        public DataTable GetSolutionTypes()
        {
            DataTable dt = new DataTable();
            try
            {
                string dbquery = "SELECT distinct t1.pKey, t1.SolutionName as strText FROM SYS_SolutionTypes t1 Order by strText";
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter(dbquery, con);
                sda.Fill(dt);
                con.Close();
            }
            catch
            {
                throw;
            }
            finally
            {
                con.Close();
            }
            return dt;
        }

        public DataTable GetPartnersLevel(int EventID, int ddLevel = 0)
        {
            DataTable _dt = new DataTable();
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select distinct t1.pKey, t1.ParticipationLevelID as strText,t1.SortOrder ");
                sb.AppendLine(" from SYS_ParticipationLevels t1");
                sb.AppendLine(" inner join Event_Organizations t2 on isnull(t2.ParticipationLevel_pkey,7) = t1.pKey");
                sb.AppendLine("  where isnull(t2.ShowOnPublicPage,0)=1 and t2.Event_pKey =" + EventID.ToString());
                //if (ddLevel> 0)
                //    sb.AppendLine(" and t1.pkey = " + Convert.ToInt32(ddLevel).ToString());
                sb.AppendLine("Order by t1.SortOrder");
                string dbquery = sb.ToString();
                con.Open();
                SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
                _da.Fill(_dt);
                con.Close();
            }
            catch (Exception ex)
            { }
            finally
            {
                con.Close();
            }
            return _dt;
        }

        public bool InsertIssue(ReportIssue ri, int AccountPkey, string strContactName, string strFolder)
        {
            string fileName = "";
            clsIssue cIssue = new clsIssue();
            cIssue.sqlConn = new System.Data.SqlClient.SqlConnection(ReadConnectionString());
            cIssue.lblMsg = null;
            cIssue.intIssue_PKey = 0;
            cIssue.intIssueArea_pKey = 365;
            cIssue.strIssueName = ri.Issuetitle;   //  Me.txtName.Text.Trim
            cIssue.strFormName = ri.Issuelocation; // Me.txtForm.Text
            cIssue.strDescription = ri.IssueDetail + "<br/> <br/>Issue:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; " + ri.Issuetitle + "<br/> Submitter: " + ri.IssueReportedbyUser + "<br/>Email:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + ri.UserEmail;
            cIssue.strPageUrl = ri.Issuelocation;
            //  cIssue.intIssueCategory_pkey = ri.IssueType;  // Val(Me.ddCategory.SelectedValue)
            cIssue.intIssueStatus_pkey = clsIssue.STATUS_ToDo;
            cIssue.intIssueType_pkey = Convert.ToInt32(ri.IssueType);  //Val(Me.ddType.SelectedValue);
            cIssue.intIssuePriority_pkey = clsIssue.PRIORITY_Normal;
            cIssue.dtEnteredOn = DateTime.Now;

            if (AccountPkey > 0)
            {
                cIssue.intEnteredByAccount_pkey = AccountPkey;
                cIssue.strEnteredByAccount = strContactName;
            }


            bool res = cIssue.SaveIssue(cIssue);

            if (ri.files != null)
            {
                string strFileGUID = clsUtility.getUniqueID();
                string strUserFileName = clsUtility.FileNewFileName(strFolder, strFileGUID, Path.GetFileNameWithoutExtension(ri.files.FileName), Path.GetExtension(ri.files.FileName));
                string strPhysicalPath = strFolder + strFileGUID + "_" + strUserFileName;

                if (!Directory.Exists(Path.Combine(strFolder)))
                {
                    Directory.CreateDirectory(strFolder);
                }
                var path = Path.Combine(strFolder, strUserFileName);
                ri.files.SaveAs(path);
            }

            return res;

        }


        #endregion

        #region EVENTS


        public DataTable FETCHEventData(int EventID)   //List<EventsInfo>
        {
            DataTable _dt = new DataTable();
            List<EventsInfo> eventinfo = new List<EventsInfo>();
            StringBuilder sb = new StringBuilder();
            sb.Append(" select * from Event_Text where event_pkey = " + EventID);
            sb.AppendLine(" and Active = 1  and sequence != 0 order by sequence ");
            string dbquery = sb.ToString();    // "select * from Event_Text where event_pkey=58  and Active=1  and sequence != 0 order by sequence";  // and Indicator=1 
            SqlCommand cmd = new SqlCommand(dbquery, con);
            con.Open();
            SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
            _da.Fill(_dt);
            con.Close();
            //using (SqlDataReader oReader = cmd.ExecuteReader())
            //{
            //    while (oReader.Read())
            //    {
            //        eventinfo.Add(new EventsInfo
            //        {
            //            SectionTitle = oReader["SectionTitle"].ToString(),
            //            SectionText = oReader["SectionText"].ToString()
            //        });

            //    }
            //}
            //con.Close();
            //return eventinfo;
            return _dt;
        }

        public List<EventsInfo> FETCHContinueEducation(string formname, int EventID)
        {
            List<EventsInfo> eventinfo = new List<EventsInfo>();
            string dbquery = "select * from Event_Text where event_pkey = " + EventID + "  and SectionTitle ='Continuing Education' and Active=1  and sequence != 0 order by sequence";
            SqlCommand cmd = new SqlCommand(dbquery, con);
            cmd.Parameters.AddWithValue("@Form", formname);
            con.Open();
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                if (oReader.Read())
                {
                    eventinfo.Add(new EventsInfo
                    {
                        SectionTitle = oReader["SectionTitle"].ToString(),
                        SectionText = oReader["SectionText"].ToString()
                    });
                }
            }
            con.Close();

            return eventinfo;
        }


        public List<VenueInfo> FETCHVenue()
        {
            DataTable dt = new DataTable();
            List<VenueInfo> venueinfo = new List<VenueInfo>();

            string dbquery = "Select * from Venue_Text where Venue_pKey = 13 and Indicator =1 order by Sequence";  // and Indicator=1 
            SqlCommand cmd = new SqlCommand(dbquery, con);
            con.Open();
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {
                    venueinfo.Add(new VenueInfo
                    {
                        SectionTitle = oReader["SectionTitle"].ToString(),
                        SectionText = oReader["SectionText"].ToString()
                    });

                }
            }
            con.Close();
            return venueinfo;
        }

        public List<ParrticipatingOrganisation> FetchOrganisation(int intProgramToViewEvent_pkey)
        {
            DataTable dt = new DataTable();
            List<ParrticipatingOrganisation> orginfo = new List<ParrticipatingOrganisation>();

            //string dbquery = "SELECt distinct(t1.OrganizationID),t1.OrganizationType_pKey,t1.SiteOrgType_Pkey  FROM Organization_List t1 where LastPartnerUpdate > '2014-03-30 18:53:42.000'";
            StringBuilder sb = new StringBuilder();
            sb.Append("select t1.OrganizationID ,t1.OrganizationType_pKey ,t1.SiteOrgType_Pkey from Organization_List t1 Inner Join Account_List t2 On t2.ParentOrganization_pkey = t1.pkey Inner Join Event_Accounts t3 on t3.account_pkey = t2.pkey");
            sb.AppendLine(" Where t3.Event_pKey =  " + intProgramToViewEvent_pkey.ToString());
            sb.AppendLine(" and t3.ParticipationStatus_pKey=" + clsEvent.PARTICIPANTSTATUS_Attending.ToString());
            sb.AppendLine("  group by t1.OrganizationType_pKey ,t1.SiteOrgType_Pkey , t1.OrganizationID having count(0) > 0  Order by t1.OrganizationID");
            string dbquery = sb.ToString();
            SqlCommand cmd = new SqlCommand(dbquery, con);
            con.Open();
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {
                    orginfo.Add(new ParrticipatingOrganisation
                    {
                        OrganizationName = oReader["OrganizationID"].ToString(),
                        OrgType = oReader["OrganizationType_pKey"].ToString(),
                        SiteOrg = oReader["SiteOrgType_Pkey"].ToString()
                    });

                }
            }
            con.Close();

            return orginfo;
        }

        public DataTable Organizationtype()
        {
            DataTable _dt = new DataTable();
            string qry = "select t1.pKey, t1.OrganizationTypeID as strText from sys_OrganizationTypes t1  Where t1.pKey NOT IN (7,8) order by t1.SortOrder";
            try
            {
                con.Open();
                SqlDataAdapter _da = new SqlDataAdapter(qry, con);
                _da.Fill(_dt);
                con.Close();
            }
            catch (Exception ex)
            { }
            finally
            {
                con.Close();
            }
            return _dt;



        }
        public DataTable SiteOrg()
        {
            DataTable _dt = new DataTable();

            string qry = "SELECT pKey, SiteOrganizationID as strText FROM Sys_SiteOrgType  Order by pKey";

            try
            {
                con.Open();
                SqlDataAdapter _da = new SqlDataAdapter(qry, con);
                _da.Fill(_dt);
                con.Close();
            }
            catch (Exception ex)
            { }
            finally
            {
                con.Close();
            }
            return _dt;
        }


        public List<ContactMAGI> FetchContactMAGI(int EventID)
        {
            List<ContactMAGI> orginfo = new List<ContactMAGI>();

            // string dbquery = "SELECT  distinct Name ,Title ,Phone ,Email FROM [MAGILOcalDB].[dbo].[Event_ContactSheet]  where Title != 'NULL' and Title != '' ";
            StringBuilder sb = new StringBuilder();
            sb.Append("select t1.pKey, (t2.ContactRoleID + ':') as RoleID, t1.Name, ('Tel# ' + t1.Phone) as Phone, t1.Email, t1.SortOrder ");
            sb.Append(" ,isnull(t3.Pkey, 0) as Account_pKey,(Case when dbo.isValidEmail(t1.Email) = 1 Then 1 else 0 end ) as bVis,t1.ContactRole_pKey ");
            sb.Append(" From Event_ContactSheet t1 ");
            sb.Append(" Left Outer Join Sys_ContactRoles t2 on t2.pkey = t1.ContactRole_pKey ");
            sb.Append(" Left Outer Join Account_List t3 on t3.Email = t1.Email ");
            sb.Append(" Where t1.Event_pkey = " + EventID + " Order by t1.SortOrder ");
            string dbquery = sb.ToString();
            SqlCommand cmd = new SqlCommand(dbquery, con);
            con.Open();
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {
                    //string id = oReader["Name"].ToString(); Account_pKey
                    string id = oReader["Account_pKey"].ToString();
                    var file = Path.Combine(id + "_img.jpg");
                    //string curFile = @"D:/Keyideas/fcManager/fcConferenceManager/Images/" + file;
                    ////string curFile = @"~\Images\"+file;
                    //bool _isexist = false;
                    //if (File.Exists(curFile))
                    //{
                    //    _isexist = true;

                    //}
                    orginfo.Add(new ContactMAGI
                    {
                        //Title = "Conference Chairman:",
                        //Name = "Norman Goldfarb",
                        //Email = "",
                        //Telephone = "Tel# +1 (650) 465-0119"
                        //ImageExist = _isexist,
                        ImagePath = file,
                        Title = oReader["RoleID"].ToString(),
                        Name = oReader["Name"].ToString(),
                        Email = oReader["Email"].ToString(),
                        Telephone = oReader["Phone"].ToString(),
                        contactRolepKey = oReader["ContactRole_pKey"].ToString()
                        //ProfilePicture = ObjectToByteArray(oReader["Img"])
                    });
                }
            }
            con.Close();

            return orginfo;
        }
        public DataTable BindSessionTracks(int EventPkey)
        {
            DataTable _dt = new DataTable();
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("  Select DISTINCT t3.pKey,'(' + t3.Prefix + ') ' + t3.TrackID as strText  From Event_Sessions t1 Inner Join Session_List t2 on t2.pkey = t1.Session_pKey  Inner Join sys_tracks t3 on t3.pkey = t1.track_pkey  Where t3.educational = 1 AND t1.IsScheduled = 1 ");
                sb.AppendLine(" AND t1.Event_pKey = " + EventPkey);
                sb.AppendLine("  Order by strText  ");


                //sb.Append("SELECT t1.pKey, t1.AuditTargetTypeID as strText ");
                //sb.AppendLine(" FROM SYS_AuditTargetTypes t1 ");
                //sb.AppendLine(" where t1.pkey in(select EntityType_pKey from Audit_Log where UpdatedByAccount_pKey = "+ AccountPkey + ")  Order by strText ");

                //sb.Append("Select DISTINCT t3.pKey,'(' + t3.Prefix + ') ' + t3.TrackID as strText  From Event_Sessions t1 Inner Join Session_List t2 on t2.pkey = t1.Session_pKey  Inner Join sys_tracks t3 on t3.pkey = t1.track_pkey  Where t3.educational = 1 AND t1.IsScheduled = 1");
                //sb.Append(" AND t1.Event_pKey = " + _Event_pkey + " Order by strText ");
                string dbquery = sb.ToString();
                con.Open();
                SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
                _da.Fill(_dt);
                con.Close();
            }
            catch (Exception ex)
            { }
            finally
            {
                con.Close();
            }
            return _dt;
        }
        public DataTable BindSessionTopics(int _Event_pkey)
        {
            DataTable _dt = new DataTable();
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(" Select DISTINCT t2.pKey, t2.ProfInterestID as strText From Event_Sessions t0 INNER Join Session_ProfInterests t1 ON t1.Session_pKey = t0.Session_pKey INNER Join SYS_ProfInterests t2 ON t2.pKey = t1.ProfInterest_pKey  ");
                sb.AppendLine(" Where isnull(t2.Enabled, 0) <> 0 AND t0.IsScheduled = 1 And t0.Event_pKey = " + _Event_pkey);
                sb.AppendLine(" Order by strText  ");

                //sb.Append(" Select DISTINCT t2.pKey, t2.ProfInterestID as strText From Event_Sessions t0 INNER Join Session_ProfInterests t1 ON t1.Session_pKey = t0.Session_pKey INNER Join SYS_ProfInterests t2 ON t2.pKey = t1.ProfInterest_pKey ");
                //sb.Append(" Where isnull(t2.Enabled, 0) <> 0 AND t0.IsScheduled = 1 And t0.Event_pKey =  " + _Event_pkey + "   Order by strText ");
                string dbquery = sb.ToString();  //  "select distinct TrackID from getFullprogram(" + _Event_pkey + ",1,1,1,1) where TrackID !='' ";
                con.Open();
                SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
                _da.Fill(_dt);
                con.Close();
            }
            catch (Exception ex)
            { }
            finally
            {
                con.Close();
            }
            return _dt;
        }
        public DataTable BindSessionAudience(int _Event_pkey)
        {
            DataTable _dt = new DataTable();
            try
            {
                string dbquery = " Select  PA.pKey, PA.Audience_Id  as strText,PA.IsActive From SYS_PrimaryAudience PA  Order by strText";
                con.Open();
                SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
                _da.Fill(_dt);
                con.Close();
            }
            catch (Exception ex)
            { }
            finally
            {
                con.Close();
            }
            return _dt;
        }

        public List<Programs> SessionDetailsProcessed(int _Event_pkey, bool ckShowRelated, bool bShowTopic, bool ckShowSpeak, string IsNew, bool ckLiveStream)
        {
            DateTime _date = DateTime.Now;
            List<Programs> sessioninfo = new List<Programs>();
            DataTable dt = new SqlOperation().GetSessionDetails(_Event_pkey, ckShowRelated, bShowTopic, ckShowSpeak, "1", ckLiveStream);
            if (dt != null && dt.Rows.Count > 0)
            {
                string _EventStartDateTime = "", _edate = "", previous_row_date = "", currentTopic = "", previousTopic = "";
                bool tablehead = false;
                int oldNum = 0, TempNum = 0;

                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    tablehead = false;
                    DataRow dr = dt.Rows[j];
                    int NextRowType = 0;
                    if (j < dt.Rows.Count - 1)
                    {
                        DataRow drNext = dt.Rows[j + 1];
                        NextRowType = (drNext["RowType"] == DBNull.Value) ? 0 : Convert.ToInt32(drNext["RowType"]);
                    }

                    string sTrackID = "", strSpkr = "", strTBDHtml = "", TimeSlot = "", TrackBG = "";
                    int RowType = 0, intNumSpeaker = 0, intSpkCount = 0, IsEducational = 0, intTrakPKey = 0, intEventType_pKey = 0, DayNum = 0, intAudiencePkey = 0, intPadValue = 0, EventSessionStatus_pkey = 0;
                    bool bNewImg = false, bLiveStream = false;

                    intTrakPKey = (dr["Track_pkey"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["Track_pkey"]);
                    bool NotAdministrative = (intTrakPKey != 10 && intTrakPKey != 11);
                    //if (intTrakPKey != 10 && intTrakPKey != 11)
                    //{
                    //-- Integer -- //
                    RowType = (dr["RowType"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["RowType"]);
                    RowType = (dr["RowType"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["RowType"]);
                    intNumSpeaker = (dr["NumSpeaker"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["NumSpeaker"]);
                    intSpkCount = (dr["SpkCount"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["SpkCount"]);
                    IsEducational = (dr["IsEducational"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["IsEducational"]);

                    intEventType_pKey = (dr["EventType_pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["EventType_pKey"]);
                    DayNum = (dr["DayNum"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["DayNum"]);
                    intAudiencePkey = (dr["Audience_pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["Audience_pKey"]);
                    EventSessionStatus_pkey = (dr["EventSessionStatus_pkey"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["EventSessionStatus_pkey"]);
                    //-- string -- //
                    sTrackID = (dr["TrackID"] == DBNull.Value) ? "" : Convert.ToString(dr["TrackID"]);
                    TrackBG = (dr["TrackBG"] == DBNull.Value) ? "" : Convert.ToString(dr["TrackBG"]);
                    strSpkr = (dr["Speakers"] == DBNull.Value) ? "" : Convert.ToString(dr["Speakers"]);
                    TimeSlot = (dr["TimeSlot"] == DBNull.Value) ? "" : Convert.ToString(dr["TimeSlot"]);
                    string strPromoLink = (dr["RecordingLink"] == DBNull.Value) ? "" : Convert.ToString(dr["RecordingLink"]).Replace("\\", "\\\\");
                    string strImg = "<img id='imgNew' src='/images/icons/icon-new.png'>";
                    bNewImg = (dr["IsNew"] == DBNull.Value) ? false : (Convert.ToInt32(dr["IsNew"]) > 0);
                    bLiveStream = (dr["IsLiveStream"] == DBNull.Value) ? false : (Convert.ToInt32(dr["IsLiveStream"]) > 0);
                    string IsLiveStream = ((bLiveStream) ? " (Live Stream)" : "");

                    bool cancelled = (EventSessionStatus_pkey == clsEventSession.STATUS_Cancelled);
                    if (oldNum == 0)
                        oldNum = DayNum;
                    else
                    {
                        if (oldNum != DayNum)
                            oldNum = DayNum;
                    }

                    string strSpeakerHTML = "";

                    if (strSpkr != "" && sTrackID != "Plenary")
                    {
                        string[] arr = strSpkr.Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries);
                        for (int intIndex = 0; intIndex <= arr.Length - 1; intIndex++)
                        {
                            string strItem = arr[intIndex];
                            if (strItem != "")
                            {
                                string[] arrSeg = strItem.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                string strName = arrSeg[0].ToString();
                                string strpkey = arrSeg[1].ToString();
                                int intRating = 0;

                                string strRating = (intRating == clsFeedback.RATING_EXCELLENT) ? "<img src='/images/icons/gold_star.png' title='Rated EXCELLENT by previous attendees'/>&nbsp;" : "";
                                strSpeakerHTML = strSpeakerHTML + "<li class='speakerListItem'>" + strRating + strName + "</li>";
                            }
                        }
                        strSpeakerHTML = "<ul class='speakerParent'>" + strSpeakerHTML + "</ul>";
                    }
                    if (IsEducational > 0 && intSpkCount < intNumSpeaker)
                    {
                        intPadValue = (intTrakPKey == 9) ? 200 : 50;
                        strTBDHtml = "<ul class='speakerParent'><li class='speakerListItem'>Speaker(s) TBD</li></ul>";
                        if (!string.IsNullOrEmpty(strTBDHtml) && strSpeakerHTML == "")
                        {
                            strSpeakerHTML = strTBDHtml;
                        }
                    }

                    TempNum = oldNum;

                    string strDescrip = "";
                    DateTime dtStart = DateTime.Now, dtEnd = DateTime.Now;
                    dtStart = (dr["StartTime"] == DBNull.Value) ? new DateTime() : Convert.ToDateTime(dr["StartTime"]);
                    strDescrip = (dr["Description"] == DBNull.Value) ? "" : Convert.ToString(dr["Description"]);

                    if (RowType == 1)
                    {
                        tablehead = true;
                        _edate = TimeSlot;
                        previous_row_date = _edate;
                    }
                    if (RowType == 3)
                    {
                        tablehead = false;
                        _edate = "";

                    }
                    _date = dtStart;
                    _EventStartDateTime = dtStart.ToString("dddd, MMMM d");

                    string _Title = (dr["Sessiontitle"] == DBNull.Value) ? "" : Convert.ToString(dr["Sessiontitle"]);
                    string _Session_pKey = (dr["EvtSession_pKey"] == DBNull.Value) ? "" : Convert.ToString(dr["EvtSession_pKey"]);
                    string _SessionId = (dr["SessionId"] == DBNull.Value) ? "" : Convert.ToString(dr["SessionId"]);
                    string _Edu = (dr["Edu"] == DBNull.Value) ? "" : Convert.ToString(dr["Edu"]);
                    string _RelatedSession = (dr["RelatedSessions"] == DBNull.Value) ? "" : Convert.ToString(dr["RelatedSessions"]);
                    string _ProfInterests = (dr["ProfInterests"] == DBNull.Value) ? "" : Convert.ToString(dr["ProfInterests"]);
                    currentTopic = (dr["TrackID"] == DBNull.Value) ? "" : Convert.ToString(dr["TrackID"]);

                    if (currentTopic == previousTopic)
                        currentTopic = "";

                    List<Session_Link> _link = new List<Session_Link>();
                    StringBuilder _relatedsession = new StringBuilder();
                    if (_RelatedSession != "")
                    {
                        string[] arr = _RelatedSession.Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries); //   Split(_RelatedSession, "^")
                        for (int i = 0; i <= arr.Length - 1; i++)
                        {
                            string arrContent = arr[i];
                            if (arrContent != "")
                            {
                                string[] rsID = arrContent.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                if (rsID.Length >= 3)
                                {
                                    string strText = rsID[0];
                                    string strpkey = rsID[1];
                                    string strTitle = rsID[2];
                                    _link.AddRange(new List<Session_Link> { new Session_Link(strText, strpkey, strTitle, _Session_pKey) });
                                    _relatedsession.Append(strText);
                                    if (i != arr.Length - 1)
                                    {
                                        _relatedsession.Append(", ");
                                    }
                                }
                            }
                        }
                    }
                    StringBuilder sbtopic = new StringBuilder();
                    if (_ProfInterests != "")
                    {
                        string[] topicsarray = _ProfInterests.Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries);
                        for (int k = 0; k <= topicsarray.Length - 1; k++)
                        {
                            string arrContent = topicsarray[k];
                            if (arrContent != "")
                            {
                                string[] tID = arrContent.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                string strTopicText = tID[0];
                                sbtopic.Append(strTopicText);
                                if (k != topicsarray.Length - 1)
                                {
                                    sbtopic.Append(", ");
                                }
                            }
                        }
                    }

                    if ((tablehead || (RowType == 3 && _EventStartDateTime == previous_row_date)) && !((RowType == NextRowType && RowType == 1) || (RowType == 1 && j == dt.Rows.Count - 1)))
                    {
                        sessioninfo.Add(new Programs
                        {
                            TableHeading = tablehead,
                            Track_pKey = intTrakPKey,
                            Audience_pKey = intAudiencePkey,
                            SessionId = ((NotAdministrative) ? _SessionId : ""),
                            Session_Pkey = _Session_pKey,
                            EventDate = TimeSlot,
                            Topic = ((NotAdministrative) ? currentTopic : ""),
                            TimeDuration = TimeSlot,
                            Title = _Title,
                            SpeakerHtml = strSpeakerHTML,
                            Edulabel = _Edu,
                            Description = strDescrip + IsLiveStream,
                            ReletedSession = _relatedsession.ToString(),
                            RelatedSessionLink = _link,
                            bNewImg = ((NotAdministrative) ? bNewImg : false),
                            Topics = sbtopic.ToString(),
                            TrackBG = TrackBG,
                            Cancelled = cancelled,
                            bLiveStream = bLiveStream,
                            RecordingLink = strPromoLink.Replace("~", "")
                        });
                    }
                    previousTopic = currentTopic;
                }
                TempNum = oldNum;
            }
            return sessioninfo;
        }
        //public List<Programs> SessionDetails(int _Event_pkey, bool ckShowRelated, bool bShowTopic, bool ckShowSpeak, string IsNew)
        //{
        //    List<Programs> sessioninfo = new List<Programs>();
        //    string dbquery = "select * from dbo.getFullprogram(" + _Event_pkey.ToString() + "," + (ckShowRelated ? "1" : "0") + ", " + (bShowTopic ? "1" : "0") + ", " + (ckShowSpeak ? "1" : "0") + ", " + IsNew + ") ORDER BY DayNum,StartTime";
        //    SqlCommand cmd = new SqlCommand(dbquery, con);
        //    DateTime _date = DateTime.Now;
        //    con.Open();
        //    string _EventStartDateTime = "", _edate = "", previous_row_date = "", currentTopic = "", previousTopic = "";
        //    bool tablehead = false;
        //    using (SqlDataReader oReader = cmd.ExecuteReader())
        //    {
        //        while (oReader.Read())
        //        {
        //            tablehead = false;
        //            string dttt = oReader["TimeSlot"].ToString();
        //            string RowType = oReader["RowType"].ToString();

        //            if (RowType == "1")
        //            {
        //                tablehead = true;
        //                _edate = dttt;
        //                previous_row_date = _edate;

        //            }
        //            if (RowType == "3")
        //            {
        //                tablehead = false;
        //                _edate = "";

        //            }
        //            string date = oReader["StartTime"].ToString();
        //            if (!string.IsNullOrEmpty(date))
        //            {
        //                _date = Convert.ToDateTime(date);
        //                _EventStartDateTime = _date.ToString("dddd, MMMM dd");
        //            }

        //            string _Title = oReader["Sessiontitle"].ToString();
        //            currentTopic = oReader["TrackID"].ToString();
        //            if (currentTopic == previousTopic)
        //            {
        //                currentTopic = "";
        //            }
        //            string _Session_pKey = oReader["EvtSession_pKey"].ToString();
        //            string _SessionId = oReader["SessionId"].ToString();
        //            string _RelatedSession = oReader["RelatedSessions"].ToString();
        //            string _ProfInterests = oReader["ProfInterests"].ToString();
        //            string strSpkr = oReader["Speakers"].ToString();
        //            List<Session_Link> _link = new List<Session_Link>();
        //            StringBuilder _relatedsession = new StringBuilder();
        //            if (_RelatedSession != "")
        //            {
        //                string[] arr = _RelatedSession.Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries); //   Split(_RelatedSession, "^")
        //                for (int i = 0; i <= arr.Length - 1; i++)
        //                {
        //                    string arrContent = arr[i];
        //                    if (arrContent != "")
        //                    {
        //                        string[] rsID = arrContent.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
        //                        if (rsID.Length >= 3)
        //                        {
        //                            string strText = rsID[0];
        //                            string strpkey = rsID[1];
        //                            string strTitle = rsID[2];
        //                            _link.AddRange(new List<Session_Link>
        //                             {
        //                              //new  Session_Link( strText,"href='/Events/MySession?ESPK='"+_Event_pkey.ToString()+"'>")
        //                              new  Session_Link( strText,  strpkey ,strTitle,_Session_pKey)
        //                             });
        //                            _relatedsession.Append(strText);
        //                            if (i != arr.Length - 1)
        //                            {
        //                                _relatedsession.Append(", ");
        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //            StringBuilder sbtopic = new StringBuilder();
        //            if (_ProfInterests != "")
        //            {
        //                string[] topicsarray = _ProfInterests.Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries);
        //                for (int k = 0; k <= topicsarray.Length - 1; k++)
        //                {
        //                    string arrContent = topicsarray[k];
        //                    if (arrContent != "")
        //                    {
        //                        string[] tID = arrContent.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
        //                        string strTopicText = tID[0];
        //                        sbtopic.Append(strTopicText);
        //                        if (k != topicsarray.Length - 1)
        //                        {
        //                            sbtopic.Append(", ");
        //                        }
        //                        //returntopic[k] = strTopicText;
        //                    }

        //                }
        //            }
        //            string strTrackID = oReader["TrackID"].ToString();
        //            string strSpeakerHTML = "";
        //            int intTrackPKey = Convert.ToInt32(oReader["Track_pkey"].ToString());
        //            if (strSpkr != "" && strTrackID != "Plenary")
        //            {

        //                int intOFs = (intTrackPKey == 9) ? 200 : 50;
        //                string[] arr = strSpkr.Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries);
        //                for (int intIndex = 0; intIndex <= arr.Length - 1; intIndex++)
        //                {
        //                    string strItem = arr[intIndex];
        //                    if (strItem != "")
        //                    {
        //                        string[] arrSeg = strItem.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
        //                        string strName = arrSeg[0].ToString();
        //                        string strpkey = arrSeg[1].ToString();
        //                        int intRating = 0; // 'Val(arrSeg(2).ToString)
        //                                           //  'strSpeakerHTML = strSpeakerHTML + IIf(intIndex = 0, "", "<br/>") + "&bull;&nbsp;<a href='frmmysessionpage.aspx? EPK = " + myVS.intEventPKey.ToString + " & SPK = " + strpkey + "'>" + strText + "</a>"
        //                        string strRating = (intRating == clsFeedback.RATING_EXCELLENT) ? "<img src='../images/icons/gold_star.png' title='Rated EXCELLENT by previous attendees'/>&nbsp;" : "";
        //                        strSpeakerHTML = strSpeakerHTML + "<tr><td style='padding-left:" + intOFs.ToString() + "px'>&bull;&nbsp;" + strRating + strName + "</td></tr>";
        //                    }
        //                }
        //                strSpeakerHTML = "<table>" + strSpeakerHTML + "</table>";
        //            }

        //            if (tablehead || RowType == "3" && _EventStartDateTime == previous_row_date)
        //            {
        //                sessioninfo.Add(new Programs
        //                {
        //                    TableHeading = tablehead,
        //                    Track_pKey = Convert.ToInt32(oReader["Track_pKey"]),
        //                    Audience_pKey = Convert.ToInt32(oReader["Audience_pKey"]),
        //                    SessionId = oReader["SessionId"].ToString(),//   _SessionId,
        //                    Session_Pkey = _Session_pKey,//  oReader["EvtSession_pKey"].ToString(),
        //                    EventDate = dttt,// _edate, //_date.ToString("dddd, dd MMMM yyyy"),
        //                    Topic = currentTopic, //  oReader["TrackID"].ToString(),
        //                    TimeDuration = oReader["TimeSlot"].ToString(),
        //                    Title = oReader["Sessiontitle"].ToString(),
        //                    SpeakerHtml = strSpeakerHTML,//  oReader["Speaker"].ToString(),
        //                    Edulabel = oReader["Edu"].ToString(),
        //                    Description = oReader["Description"].ToString(),
        //                    ReletedSession = _relatedsession.ToString(),  // oReader["RelatedSessions"].ToString()
        //                    RelatedSessionLink = _link,
        //                    Topics = sbtopic.ToString()
        //                });
        //            }
        //            previousTopic = currentTopic;


        //        }
        //    }
        //    con.Close();

        //    return sessioninfo;
        //}

        public List<SelectedSessionDetails> MySessionDetails(int ESPK, int _Event_pkey)
        {

            List<SelectedSessionDetails> sessioninfo = new List<SelectedSessionDetails>();
            List<Speakers> speakerinfo = new List<Speakers>();
            string dbquery = " select StartTime ,TimeSlot,Description ,SessionID,Sessiontitle, ProfInterests, RelatedSessions,Speakers from getFullprogram( 57 ,1,1,1,1)   where EvtSession_pKey = " + ESPK.ToString(); // "+ _Event_pkey +"         
            SqlCommand cmd = new SqlCommand(dbquery, con);
            DateTime _date = DateTime.Now;
            string _EventDate = "";
            con.Open();
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {
                    string timeslot = oReader["TimeSlot"].ToString();
                    string date = oReader["StartTime"].ToString();
                    if (!string.IsNullOrEmpty(date))
                    {
                        _date = Convert.ToDateTime(date);
                        _EventDate = _date.ToString("dddd, MMMM dd, yyyy");
                    }
                    string _RelatedSession = oReader["RelatedSessions"].ToString();
                    string _ProfInterests = oReader["ProfInterests"].ToString();
                    string _Speakers = oReader["Speakers"].ToString();

                    List<Session_Link> _link = new List<Session_Link>();
                    StringBuilder _relatedsession = new StringBuilder();
                    if (_RelatedSession != "")
                    {
                        string[] arr = _RelatedSession.Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries); //   Split(_RelatedSession, "^")
                        for (int i = 0; i <= arr.Length - 1; i++)
                        {
                            string arrContent = arr[i];
                            if (arrContent != "")
                            {
                                string[] rsID = arrContent.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                if (rsID.Length >= 3)
                                {
                                    string strText = rsID[0];
                                    string strpkey = rsID[1];
                                    string strTitle = rsID[2];
                                    _link.AddRange(new List<Session_Link>
                                     {
                                      new  Session_Link( strText,  strpkey ,strTitle,"")
                                     });
                                    _relatedsession.Append(strText);
                                    if (i != arr.Length - 1)
                                    {
                                        _relatedsession.Append(", ");
                                    }
                                }
                            }
                        }
                    }
                    StringBuilder sbtopic = new StringBuilder();
                    if (_ProfInterests != "")
                    {
                        string[] topicsarray = _ProfInterests.Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries);
                        for (int k = 0; k <= topicsarray.Length - 1; k++)
                        {
                            string arrContent = topicsarray[k];
                            if (arrContent != "")
                            {
                                string[] tID = arrContent.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                string strTopicText = tID[0];
                                sbtopic.Append(strTopicText);
                                if (k != topicsarray.Length - 1)
                                {
                                    sbtopic.Append(", ");
                                }
                            }

                        }
                    }

                    StringBuilder sbSpeaker = new StringBuilder();
                    if (_ProfInterests != "")
                    {
                        string[] speakerarray = _Speakers.Split(new[] { "^" }, StringSplitOptions.RemoveEmptyEntries);
                        for (int k = 0; k <= speakerarray.Length - 1; k++)
                        {
                            string arrContent = speakerarray[k];
                            if (arrContent != "")
                            {
                                string[] tID = arrContent.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                string strSpeaker = tID[0];
                                string[] speakerDetails = strSpeaker.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                                string spkname = speakerDetails[0];
                                string spkTitle = speakerDetails[1];
                                string spkOrg = speakerDetails[2];
                                sbSpeaker.Append(spkname);

                                speakerinfo.Add(new Speakers
                                {
                                    SpeakerName = spkname,
                                    SpeakerTitle = spkTitle,
                                    SpeakerOrganization = spkOrg,
                                    Sid = tID[1],

                                });
                            }

                        }
                    }

                    sessioninfo.Add(new SelectedSessionDetails
                    {
                        Title = oReader["Sessiontitle"].ToString(),
                        Description = oReader["Description"].ToString(),
                        Speakers = speakerinfo,
                        EventDate = _EventDate + " " + timeslot,
                        Topics = sbtopic.ToString(),
                        SessionId = oReader["SessionId"].ToString(),//   _SessionId,

                        ReletedSession = _relatedsession.ToString(), // oReader["RelatedSessions"].ToString()
                        RelatedSessionLink = _link

                    });
                }
            }
            con.Close();

            return sessioninfo;
        }
        public List<Session_Link> RelatedSessionLink(int SPK, int Event_Pkey)
        {
            string dbquery = "Select *,(Case When Row_Number() Over ( Order By SessionID ) = 1 Then '' else ',' End) as Comma  from dbo.getRelatedSessionsTable(" + SPK + ", 1," + Event_Pkey.ToString() + ")";
            SqlCommand cmd = new SqlCommand(dbquery, con);
            List<Session_Link> _link = new List<Session_Link>();
            con.Open();
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {

                    string _SessionID = oReader["SessionID"].ToString();
                    string _Session_pKey = oReader["Session_pKey"].ToString();
                    string _SessionTitle = oReader["SessionTitle"].ToString();
                    string _EventSession_pkey = oReader["EventSession_pkey"].ToString();

                    StringBuilder _relatedsession = new StringBuilder();

                    _link.AddRange(new List<Session_Link>
                                     {
                                      new  Session_Link( _SessionID,  _Session_pKey ,_SessionTitle,_EventSession_pkey)
                                     });
                }
            }
            con.Close();

            return _link;
        }
        public List<Speakers> RefreshSessionSpeaker(string ESPK)
        {
            List<Speakers> speakerinfo = new List<Speakers>();
            string qry = "Select (  isNull(t3.Lastname,'')+ ', ' + isNull(t3.Firstname,'')  ) as Name, isNull(t3.Title,'') as Title,  isNull(t4.OrganizationID,'') as Organization, t2.Account_pKey"
         + Environment.NewLine + " ,ISNULL(t1.NumSpeakers,0) as numspeaker,t2.IsSessionLeader,t2.IsSessionLeader2,t2.IsSessionModerator,t2.IsSessionModerator2"
         + Environment.NewLine + " ,CASE WHEN  ((ISNULL(t2.IsSessionLeader,0)= 1 OR ISNULL(t2.IsSessionLeader2,0) =1) AND (ISNULL(t2.IsSessionModerator,0) =1 OR ISNULL(t2.IsSessionModerator2,0) =1))"
         + Environment.NewLine + " OR (ISNULL(t2.IsSessionModerator,0) =1 OR ISNULL(t2.IsSessionModerator2,0) =1) THEN 'Moderator'"
         + Environment.NewLine + " WHEN (ISNULL(t2.IsSessionLeader,0)= 1 OR ISNULL(t2.IsSessionLeader2,0) =1)  THEN 'Leader' WHEN ISNULL(t2.IsSpeaker,0) =1 THEN '' END as SessionUserRole"
         + Environment.NewLine + " From event_sessions t1"
         + Environment.NewLine + " inner join eventsession_staff t2 On t2.EventSession_pKey = t1.pKey"
         + Environment.NewLine + " inner join account_List t3 On t3.pKey = t2.Account_pKey"
         + Environment.NewLine + " Left outer join Organization_list t4 On t4.pkey = t3.ParentOrganization_pKey"
         + Environment.NewLine + " INNER JOIN Session_List t5 ON t5.pKey=t1.Session_pKey"
         + Environment.NewLine + " Where t1.pKey = " + ESPK
         + Environment.NewLine + " and (ISNULL(t2.IsSpeaker,0) = 1 OR ISNULL(t2.IsSessionLeader,0)= 1 OR ISNULL(t2.IsSessionLeader2,0) =1 OR ISNULL(t2.IsSessionModerator,0) =1 OR ISNULL(t2.IsSessionModerator2,0) =1)"
         + Environment.NewLine + " Order by "
         + Environment.NewLine + "   CASE when t5.SessionType_pkey=6 and  t1.ISSPORDER is null   and ((ISNULL(t2.IsSessionModerator, 0) = 1 OR  "
         + Environment.NewLine + "ISNULL(t2.IsSessionModerator2, 0) = 1))  then -1  "
         + Environment.NewLine + "WHEN ((ISNULL(t2.IsSessionModerator, 0) = 1 OR  "
         + Environment.NewLine + "ISNULL(t2.IsSessionModerator2, 0) = 1) AND  "
         + Environment.NewLine + "ISNULL(t1.ISSPORDER, 0) = 1) THEN -1  ELSE 0 end"
         + Environment.NewLine + ",Name";
            SqlCommand cmd = new SqlCommand(qry, con);
            con.Open();
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {
                    string IsSessionLeader = oReader["IsSessionLeader"].ToString();
                    string IsSessionLeader2 = oReader["IsSessionLeader2"].ToString();
                    string IsSessionModerator = oReader["IsSessionModerator"].ToString();
                    string IsSessionModerator2 = oReader["IsSessionModerator2"].ToString();
                    string SessionUserRole = oReader["SessionUserRole"].ToString();
                    string role = "";

                    if (IsSessionLeader != "")
                        role = IsSessionLeader;
                    else if (IsSessionLeader2 != "")
                        role = IsSessionLeader2;
                    else if (IsSessionModerator != "")
                        role = IsSessionModerator;
                    else if (IsSessionModerator2 != "")
                        role = IsSessionModerator2;

                    speakerinfo.Add(new Speakers
                    {
                        SpeakerName = oReader["Name"].ToString(),
                        SpeakerTitle = oReader["Title"].ToString(),
                        SpeakerOrganization = oReader["Organization"].ToString(),
                        Sid = oReader["Account_pKey"].ToString(),
                        SpeakerRole = role,
                        SpeakerUserRole = SessionUserRole
                    });
                }
            }
            return speakerinfo;
        }

        public List<Speakers> SessionSpeaker(string ESPK)
        {
            List<Speakers> speakerinfo = new List<Speakers>();

            StringBuilder sb = new StringBuilder();
            sb.Append(" Select(isNull(t3.Firstname, '') + ' ' + isNull(t3.Lastname, '')) as Name, isNull(t3.Title, '') as Title,  isNull(t4.OrganizationID, '') as Organization, t2.Account_pKey ");
            sb.AppendLine(" ,ISNULL(t1.NumSpeakers, 0) as numspeaker,t2.IsSessionLeader,t2.IsSessionLeader2,t2.IsSessionModerator,t2.IsSessionModerator2  ");
            sb.AppendLine(" ,CASE WHEN((ISNULL(t2.IsSessionLeader,0)= 1 OR ISNULL(t2.IsSessionLeader2,0) = 1) AND(ISNULL(t2.IsSessionModerator, 0) = 1 OR ISNULL(t2.IsSessionModerator2, 0) = 1))  ");
            sb.AppendLine(" OR(ISNULL(t2.IsSessionModerator, 0) = 1 OR ISNULL(t2.IsSessionModerator2, 0) = 1) THEN 'Moderator'  ");
            sb.AppendLine("  WHEN(ISNULL(t2.IsSessionLeader, 0) = 1 OR ISNULL(t2.IsSessionLeader2, 0) = 1)  THEN 'Leader' WHEN ISNULL(t2.IsSpeaker,0) = 1 THEN '' END as SessionUserRole  ");
            sb.AppendLine("  From event_sessions t1 ");
            sb.AppendLine(" inner join eventsession_staff t2 On t2.EventSession_pKey = t1.pKey  ");
            sb.AppendLine("  inner join account_List t3 On t3.pKey = t2.Account_pKey  ");
            sb.AppendLine(" Left outer join Organization_list t4 On t4.pkey = t3.ParentOrganization_pKey  ");
            sb.AppendLine("   Where t1.pKey = " + ESPK);
            sb.AppendLine("  and(ISNULL(t2.IsSpeaker, 0) = 1 OR ISNULL(t2.IsSessionLeader, 0) = 1 OR ISNULL(t2.IsSessionLeader2, 0) = 1 OR ISNULL(t2.IsSessionModerator, 0) = 1 OR ISNULL(t2.IsSessionModerator2, 0) = 1) ");


            string dbquery = sb.ToString();
            SqlCommand cmd = new SqlCommand(dbquery, con);

            con.Open();
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {
                    string IsSessionLeader = oReader["IsSessionLeader"].ToString();
                    string IsSessionLeader2 = oReader["IsSessionLeader2"].ToString();
                    string IsSessionModerator = oReader["IsSessionModerator"].ToString();
                    string IsSessionModerator2 = oReader["IsSessionModerator2"].ToString();
                    string role = "";
                    if (IsSessionLeader != "")
                    {
                        role = IsSessionLeader;

                    }
                    else if (IsSessionLeader2 != "")
                    {
                        role = IsSessionLeader2;

                    }
                    else if (IsSessionModerator != "")
                    {
                        role = IsSessionModerator;
                    }
                    else if (IsSessionModerator2 != "")
                    {
                        role = IsSessionModerator2;
                    }


                    speakerinfo.Add(new Speakers
                    {
                        SpeakerName = oReader["Name"].ToString(),
                        SpeakerTitle = oReader["Title"].ToString(),
                        SpeakerOrganization = oReader["Organization"].ToString(),
                        Sid = oReader["Account_pKey"].ToString(),
                        SpeakerRole = role


                    });
                }


            }


            return speakerinfo;
        }


        public List<Programs> SessionDetailsFunction(int _Event_pkey)
        {
            List<Programs> sessioninfo = new List<Programs>();
            //int _Event_pkey = 1, _RelatedSession = 1, _speakers=1, _topic, _isnew = 0;
            //String a = "select * from dbo.getFullprogram(" + _Event_pkey + "," + IIf(_RelatedSession, "1", "0") + ", " + IIf(_topic, "1", "0") + ", " + IIf(_speakers, "1", "0") + ", " + IIf(_isnew, "1", "0") + ")";

            string dbquery = " select * from getFullprogram(" + _Event_pkey + ",1,1,1,1) "; //+ _Event_pkey +
            SqlCommand cmd = new SqlCommand(dbquery, con);
            DateTime _date = DateTime.Now;
            String _EventStartDateTime = "";
            string _edate = "";
            con.Open();
            string previous_row_date = "";
            string current_row_date = "";
            string currentTopic = "";
            string previousTopic = "";
            string dtimestring = "";
            bool tablehead = false;
            DateTime dateValue;
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {
                    // string intNumSpeaker = oReader["NumSpeaker"].ToString();
                    //int intSpkCount  = Convert.ToInt32(oReader["SpkCount"]);
                    //int IsEducational  = Convert.ToInt32(oReader["IsEducational"]);
                    //int intTrakPKey = Convert.ToInt32(oReader["Track_pkey"]);
                    //int EventType_pKey  = Convert.ToInt32(oReader["EventType_pKey"]);
                    //int DayNum   = Convert.ToInt32(oReader["DayNum"]);
                    //string strSpkr   = oReader["Speakers"].ToString();
                    //int intAudiencePkey   = Convert.ToInt32(oReader["Audience_pKey"]);

                    string dttt = oReader["TimeSlot"].ToString();

                    //bool isDateTime = DateTime.TryParseExact(dttt, "dddd, MMMM dd", out dateValue);//    DateTime.TryParse(dttt, out dateValue);

                    //DateTime dt2 = DateTime.Parse(dttt);
                    // string dt = Convert.ToDateTime(dttt).ToString("dddd, MMMM dd");
                    //DateTime eventdatetime = DateTime.Parse(dttt, new System.Globalization.CultureInfo("en-US"));
                    //CultureInfo provider = CultureInfo.InvariantCulture;
                    //bool isSuccess6 = DateTime.TryParseExact(dttt, new string[] { "dddd,MMMM dd", "MM/dd/yyyy", "MM-dd-yyyy", "MM.dd.yyyy" }, provider, DateTimeStyles.None, out dateValue);

                    //string ss = eventdatetime.ToString("dddd, MMMM dd");
                    //if (DateTime.TryParseExact(dttt, "dd,MMMM yy HH:mm:ss", new System.Globalization.CultureInfo("pt-BR"),DateTimeStyles.None,out dateValue))
                    //{
                    //    dtimestring = Convert.ToDateTime(dttt).ToString("dddd, MMMM dd");
                    //}
                    if (DateTime.TryParseExact(dttt, "dddd, MMMM dd", new System.Globalization.CultureInfo("en-US"), DateTimeStyles.None, out dateValue))
                    {
                        dtimestring = Convert.ToDateTime(dttt).ToString("dddd, MMMM dd");
                    }
                    else
                    {
                        dtimestring = "";
                    }

                    if (dttt == dtimestring)
                    {
                        tablehead = true;
                        _edate = dttt;
                        previous_row_date = _edate;
                    }
                    else
                    {
                        tablehead = false;
                        _edate = "";

                    }

                    string date = oReader["StartTime"].ToString();
                    if (!string.IsNullOrEmpty(date))
                    {
                        _date = Convert.ToDateTime(date);
                        _EventStartDateTime = _date.ToString("dddd, dd MMMM yyyy");
                    }
                    //else
                    //{
                    //     date = oReader["TimeSlot"].ToString();
                    //    _date = Convert.ToDateTime(date);
                    //    _edate = oReader["TimeSlot"].ToString(); // "";
                    //}
                    //string s = _date.ToString("MM/dd/yyyy");
                    //current_row_date = s;
                    //if (current_row_date != previous_row_date && dtimestring != "NULL")
                    //{
                    //    current_row_date = _date.ToString("MM/dd/yyyy");
                    //    current_row_date = Convert.ToDateTime(_edate).ToString("MM/dd/yyyy");
                    //}
                    //else
                    //{
                    //    current_row_date = "";
                    //    _edate = "";
                    //}
                    if (tablehead == false && _edate != "" && dttt != previous_row_date)
                    {
                        tablehead = true;
                        previous_row_date = dttt;
                    }

                    string _Title = oReader["Sessiontitle"].ToString();
                    currentTopic = oReader["TrackID"].ToString();
                    if (currentTopic == previousTopic)
                    {
                        currentTopic = "";
                    }
                    string _SessionId = oReader["SessionId"].ToString();
                    if (!tablehead && _SessionId == "")
                    {

                    }
                    else if (tablehead || _EventStartDateTime == dttt)
                    {
                        sessioninfo.Add(new Programs
                        {
                            TableHeading = tablehead,
                            SessionId = _SessionId,
                            EventDate = dttt,// _edate, //_date.ToString("dddd, dd MMMM yyyy"),
                            Topic = currentTopic, //  oReader["TrackID"].ToString(),
                            TimeDuration = oReader["TimeSlot"].ToString(),
                            Title = oReader["Sessiontitle"].ToString(),
                            //Speaker = oReader["Speaker"].ToString(),
                            Edulabel = oReader["Edu"].ToString(),
                            Description = oReader["Description"].ToString(),
                            ReletedSession = oReader["RelatedSessions"].ToString()
                        });
                    }
                    previousTopic = currentTopic;
                    //if (current_row_date != "")
                    //{
                    //    previous_row_date = current_row_date;
                    //}



                }
            }
            con.Close();

            return sessioninfo;

        }


        public int GetAttendeeAndPseakerCount(int intEventSession_pKey, int intActiveEventPkey)
        {
            int Count = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("select Count(t1.pKey) As C1");
            sb.AppendLine(" From account_List t1");
            sb.AppendLine(" inner join EventSession_Accounts t2 on t2.account_pkey = t1.pKey");
            sb.AppendLine(" Left outer join Organization_List t3 on t3.pkey = t1.parentorganization_pkey");
            sb.AppendLine(" Where t2.EventSession_pkey = " + intEventSession_pKey.ToString());
            sb.AppendLine(" and t2.Attending=1");
            sb.AppendLine("ANd t1.pkey NOT IN (Select t3.pkey");
            sb.AppendLine(" From event_sessions t1  inner Join eventsession_staff t2 On t2.EventSession_pKey = t1.pKey");
            sb.AppendLine("inner join account_List t3 On t3.pKey = t2.Account_pKey  ");
            sb.AppendLine("Left outer join Organization_list t4 On t4.pkey = t3.ParentOrganization_pKey  Where t1.pKey  = " + intEventSession_pKey.ToString() + ")  ");
            sb.AppendLine("AND t1.pKey in (select Account_pKey from Event_Accounts where isnull(ParticipationStatus_pKey,0)=1 and Event_pKey=" + intActiveEventPkey.ToString() + ")");
            sb.AppendLine("UNION ALL ");
            sb.AppendLine("Select COUNT(t3.pkey)  As C1");
            sb.AppendLine(" From event_sessions t1  inner join eventsession_staff t2 On t2.EventSession_pKey = t1.pKey");
            sb.AppendLine("inner join account_List t3 On t3.pKey = t2.Account_pKey  Left outer join Organization_List t5 on t5.pkey = t3.parentorganization_pkey ");
            sb.AppendLine("Left outer join Organization_list t4 On t4.pkey = t3.ParentOrganization_pKey  ");
            sb.AppendLine("  Where t1.pKey  = " + intEventSession_pKey.ToString());
            sb.AppendLine("AND  t3.pKey not in (select Account_pKey from Event_Accounts where isnull(ParticipationStatus_pKey,0)=2 and Event_pKey=" + intActiveEventPkey.ToString() + ")");
            sb.AppendLine(" AND (ISNULL(t2.IsSessionChair,0)<>0 OR ISNULL(t2.IsSpeaker,0)<>0)");

            string qry = sb.ToString();
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            SqlParameter[] parameters = new SqlParameter[] { };
            dt = SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
            foreach (DataRow dr in dt.Rows)
            {
                Count = Count + Convert.ToInt32(dr["C1"].ToString());
            }

            return Count;

        }

        public List<Speakers> FETCHSpeaker(string orderby)
        {

            DataTable dt = new DataTable();
            List<Speakers> speakerinfo = new List<Speakers>();

            //string dbquery = "Select pKey , Name ,Title, Organization from SpeakerContact  where Title != 'NULL' and Title !='' order by Name";  // and Indicator=1 
            StringBuilder sb = new StringBuilder();
            sb.Append(" select distinct sl.Track_Prefix+sl.SessionID as SessionID, sp.Session_pKey ,Account_pkey, Name,Title,Organization,sl.SessionTitle,sl.Description  from SpeakerContact sc ");
            sb.Append(" inner join  Session_ProfInterests sp on sp.pKey = sc.pKey ");
            sb.Append(" inner join  Session_List sl on sp.Session_pKey = sl.pKey ");
            sb.Append(" where sc.Title != 'NULL' and sc.Title != '' and sc.Account_pkey != ''  ");
            if (orderby == "")
            {
                sb.Append("order by Name");
            }
            else
            {
                sb.Append("order by Organization");

            }
            string dbquery = sb.ToString();
            SqlCommand cmd = new SqlCommand(dbquery, con);
            con.Open();
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {
                    string sessionid = oReader["SessionID"].ToString();
                    speakerinfo.Add(new Speakers
                    {
                        Sid = oReader["Account_pkey"].ToString(),
                        SpeakerName = oReader["Name"].ToString(),
                        SpeakerTitle = oReader["Title"].ToString(),
                        SpeakerOrganization = oReader["Organization"].ToString(),
                        SessionId = oReader["SessionID"].ToString(),
                        Session_pKey = Convert.ToInt32(oReader["Session_pKey"].ToString()),
                        SessionTitle = " - " + oReader["SessionTitle"].ToString(),
                        SessionDescription = oReader["Description"].ToString()
                    });

                }
            }
            con.Close();
            return speakerinfo;
        }


        public List<Speakers> FETCHSpeaker(int Evt_Pkey, string orderby, int star)
        {

            DataTable dt = new DataTable();
            List<Speakers> speakerinfo = new List<Speakers>();

            //string dbquery = "Select pKey , Name ,Title, Organization from SpeakerContact  where Title != 'NULL' and Title !='' order by Name";  // and Indicator=1 
            StringBuilder sb = new StringBuilder();
            sb.Append(" Select distinct t2.Account_pKey, t3.ContactName As Fullname, ");
            sb.AppendLine(" (Case When isNull(t3.SpeakerRating,0) = " + star + " Then 1 Else 0 End) As ShowStar  ");
            //sb.AppendLine(" --,(Case When isNull(t3.SpeakerRating,0) = " + clsFeedback.RATING_EXCELLENT.ToString + " Then 'Rated EXCELLENT by previous attendees' Else Null End) as Tooltip  ");
            sb.AppendLine(" , IIF(t3.Degrees = '' or t3.Degrees is null, '', ' - ' + t3.Degrees) as Degrees  ");
            sb.AppendLine("  , IIF(t3.Title = '' or t3.Title is null, '', ' - ' + t3.Title) as Title ");
            sb.AppendLine(" , IIF(t4.OrganizationID = '' or t4.OrganizationID is null, '', ' - ' + t4.OrganizationID) as OrganizationID  ");
            sb.AppendLine(" , t1.pKey,t1.Session_pKey ,(t1.track_prefix + t7.SessionID) as SessionID, t1.Title as SessionTitle, t7.Description ");
            sb.AppendLine("  From event_sessions t1  ");
            sb.AppendLine("   inner join eventsession_staff t2 On t2.eventsession_pKey = t1.pKey  ");
            sb.AppendLine("  inner join account_List t3 On t3.pKey = t2.Account_pKey  ");
            sb.AppendLine("  Left outer join Organization_list t4 On t4.pkey = t3.ParentOrganization_pKey ");
            sb.AppendLine("  inner join session_List t7 On t7.pKey = t1.Session_pKey ");
            sb.AppendLine("  Where(t2.IsSpeaker = 1)  ");
            sb.AppendLine(" and isNull(t2.AssignmentStatus_pkey,2) in (2, 19) ");
            sb.AppendLine("  and isNull(t3.SpecialSpeaker,0) <> 1 ");
            sb.AppendLine(" and t1.Event_pKey = " + Evt_Pkey);

            if (orderby == "")
            {
                sb.Append("order by Fullname,SessionID asc");
            }
            else
            {
                sb.Append("order by OrganizationID,SessionID asc");

            }
            string dbquery = sb.ToString();
            SqlCommand cmd = new SqlCommand(dbquery, con);
            con.Open();
            cmd.CommandTimeout = 80;
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {

                    string sessionid = oReader["SessionID"].ToString();
                    bool _showstar = false;
                    string havestar = oReader["ShowStar"].ToString();
                    if (havestar == "1")
                    {
                        _showstar = true;
                    }
                    speakerinfo.Add(new Speakers
                    {
                        Sid = oReader["Account_pkey"].ToString(),
                        SpeakerName = oReader["Fullname"].ToString(),
                        SpeakerDegree = oReader["Degrees"].ToString(),
                        SpeakerTitle = oReader["Title"].ToString(),
                        SpeakerOrganization = oReader["OrganizationID"].ToString(),
                        SessionId = oReader["SessionID"].ToString(),
                        Session_pKey = Convert.ToInt32(oReader["Session_pKey"].ToString()),
                        S_pKey = Convert.ToInt32(oReader["pKey"].ToString()),
                        SessionTitle = " - " + oReader["SessionTitle"].ToString(),
                        SessionDescription = oReader["Description"].ToString(),
                        ShowStar = _showstar

                    });

                }
            }
            con.Close();
            return speakerinfo;
        }




        public List<Advisory> AdvisoryBoard(string _name, string _title, string _organization)
        {
            DataTable dt = new DataTable();
            List<Advisory> advisoryinfo = new List<Advisory>();
            StringBuilder sb = new StringBuilder();
            sb.Append("select t1.pKey, t2.OrganizationID, t1.ContactName, t1.Firstname, t1.Lastname, t1.Title ");
            sb.AppendLine(" From Account_List t1 Left Outer Join Organization_List t2 on t2.pkey = t1.ParentOrganization_pKey");
            sb.AppendLine("Where t1.AdvisoryBoardMember = 1 AND  t1.ContactName like '%" + _name + "%' AND t1.Title like '%" + _title + "%' AND t2.OrganizationID like '%" + _organization + "%' ");
            sb.AppendLine(" Order by t1.ContactName ");
            string dbquery = sb.ToString();
            SqlCommand cmd = new SqlCommand(dbquery, con);
            con.Open();
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {
                    advisoryinfo.Add(new Advisory
                    {
                        Id = Convert.ToInt32(oReader["pKey"].ToString()),
                        Name = oReader["ContactName"].ToString(),
                        Title = oReader["Title"].ToString(),
                        Orginization = oReader["OrganizationID"].ToString(),

                    });

                }
            }
            con.Close();
            return advisoryinfo;
        }

        public List<Testimonials> Testimonials()
        {
            List<Testimonials> testimonials = new List<Testimonials>();
            string dbquery = "Select *   From Testimonial_List";
            SqlCommand cmd = new SqlCommand(dbquery, con);
            con.Open();
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {
                    testimonials.Add(new Testimonials
                    {
                        Comment = oReader["Comment"].ToString(),
                        Role = oReader["Role"].ToString()

                    });

                }
            }
            con.Close();
            return testimonials;
        }
        public List<UpcomingEvents> Upcoming_Events()
        {
            List<UpcomingEvents> upcomingevents = new List<UpcomingEvents>();
            StringBuilder sb = new StringBuilder();
            //sb.Append("select el.pKey ,el.EventFullName,el.StartDate,el.EndDate,vl.VenueID,Description  from Event_List el ");
            //sb.Append(" join Venue_List vl on vl.pKey = el.Venue_pkey Where StartDate > getdate()  ");          
            //sb.Append("order by StartDate");


            sb.Append("  select t1.StartDate,t1.pKey, t1.EventFullname, isNull(t2.VenueID, 'Venue coming soon') as Venue, t1.BannerMessage,t1.Description, t2.City, t3.StateID  ");
            sb.AppendLine("  ,iif(t2.City is null or t2.City = '', '', ', ' + t2.City + ', ' + isNull(t3.StateID, '')) as VenuePlace  ");
            sb.AppendLine(" , (datename(Month, t1.StartDate) + ' ' + datename(d, t1.StartDate) + ' - ' + datename(month, t1.EndDate) + ' ' + datename(d, t1.EndDate) + ', ' + datename(yyyy, t1.EndDate)) as Dates   ");
            sb.AppendLine(" ,iif(isNull(t1.HomeBanner, '') <> '', '../images/BannerImages/' + t1.HomeBanner, (Case when isNull(t2.ImageSmall, '') <> '' Then '../VenueDocuments/' + t2.FileGUID + '_' + t2.ImageSmall else '../VenueDocuments/DefaultOrganization.png' End)) as VenueSmall   ");
            sb.AppendLine(" from Event_List t1  ");
            sb.AppendLine("  Left outer join Venue_List t2 On t2.pkey = t1.Venue_pKey  ");
            sb.AppendLine("  Left outer join sys_states t3 On t3.pkey = t2.State_pKey  ");
            sb.AppendLine(" Where isNull(t1.AllEvents,0) = 1 And t1.EndDate > getdate()   ");
            sb.AppendLine("  And t1.EventStatus_pkey in (" + clsEvent.STATUS_Pending + ", " + clsEvent.STATUS_Started + ") ");
            sb.AppendLine("  UNION   ");
            sb.AppendLine(" Select EL.StartDate,EL.pKey,EL.EventFullName,isNull(EL.VenueID, 'Venue coming soon') as Venue,'' as BannerMessage,'' as Description,''As City,'' as StateID,'' as VenuePlace   ");
            sb.AppendLine(" , (datename(Month, EL.StartDate) + ' ' + datename(d, EL.StartDate) + ' - ' + datename(month, EL.EndDate) + ' ' + datename(d, EL.EndDate) + ', ' + datename(yyyy, EL.EndDate)) as Dates,'' as VenueSmall   ");
            sb.AppendLine(" from ExternalEvent_list EL   ");

            string dbquery = sb.ToString();
            SqlCommand cmd = new SqlCommand(dbquery, con);
            con.Open();
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {
                    //DateTime dt = Convert.ToDateTime(oReader["StartDate"].ToString());
                    //string startdate = dt.ToString("MMMM dd");                   
                    //DateTime dt2 = Convert.ToDateTime(oReader["EndDate"].ToString());
                    //string enddate = dt2.ToString("dd MMMM yyyy");
                    //string _Description = oReader["Description"].ToString();
                    //if(_Description != "")
                    //{
                    //    _Description = _Description.Substring(0, _Description.IndexOf(".") +1);
                    //}

                    upcomingevents.Add(new UpcomingEvents
                    {
                        ID = Convert.ToInt32(oReader["pKey"].ToString()),
                        Title = oReader["EventFullName"].ToString(),
                        Venue = oReader["Venue"].ToString(),
                        Place = oReader["VenuePlace"].ToString(),
                        Date = oReader["Dates"].ToString(),  //startdate +"-"+enddate,
                        Description = oReader["Description"].ToString(),
                        Image = oReader["VenueSmall"].ToString()

                    });

                }
            }
            con.Close();
            return upcomingevents;
        }

        public string CurrentEventInfo(int? pKey)
        {
            string _EventInfo = string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.Append("select el.pKey ,el.EventFullName,el.StartDate,el.EndDate,vl.VenueID,Description  from Event_List el ");
            sb.Append(" join Venue_List vl on vl.pKey = el.Venue_pkey Where   "); //StartDate > getdate() and
            if (pKey != null)
            {
                sb.Append("  el.pKey = " + pKey);
            }
            sb.Append("order by StartDate");
            string dbquery = sb.ToString();
            SqlCommand cmd = new SqlCommand(dbquery, con);
            con.Open();
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {

                    DateTime dt = Convert.ToDateTime(oReader["StartDate"].ToString()).Date;
                    string startdate = dt.ToString("MMMM dd");
                    DateTime dt2 = Convert.ToDateTime(oReader["EndDate"].ToString()).Date;
                    string enddate = dt2.Day.ToString();
                    string endyear = dt2.Year.ToString();
                    _EventInfo = startdate + "-" + enddate + ", " + endyear;

                }
            }
            con.Close();
            return _EventInfo;
        }

        public List<EventSponsors> EventSponsors(int? level, int EventID, int audType = 0, int solType = 0)
        {
            List<EventSponsors> upcomingevents = new List<EventSponsors>();
            StringBuilder sb = new StringBuilder();
            sb.Append("Select isNull(t1.pkey,0) As ipk, t1.OrganizationID ");
            sb.AppendLine(" ,iif(isnull(t1.Comment, '') = '', t1.OrganizationID, replace(replace(isNull(t1.Comment, ''), '{', '<b>'), '}', '</b>')) as Profile");
            sb.AppendLine(" , isnull(t2.ParticipationLevel_pKey, 7) as ParticipationLevel_pKey,t4.ParticipationLevelID, t2.SpecialOffer ");
            sb.AppendLine("  ,(Case when isNull(t2.SpecialOffer,'') <> '' Then 1 else 0 End) as ShowOffer ");
            sb.AppendLine("  ,(Case when t2.ParticipationType_pKey > 0 then IIF(t3.ParticipationTypeID = 'Branding','','[' + t3.ParticipationTypeID + ']')  else Null End) as PType ");
            //sb.AppendLine("  ,(Case when t2.ParticipationType_pKey > 0 and isnull(t1.Comment,'')<> '' then IIF(t3.ParticipationTypeID = 'Branding','','' + t1.OrganizationID + '')  else Null End) as PType ");
            //sb.AppendLine("  , (Case when i.pkey is Null Then (select top 1 DefaultOrganizationLogo from Application_Defaults) else i.ImageContent end) as Thumbnail ");
            sb.AppendLine(" ,'~/OrganizationDocuments/' + Convert(varchar, t1.pKey) + '_img.jpg' As ImgLogo ");
            sb.AppendLine("  , replace(replace(IIF(charindex('//', isnull(t1.url, '')) > 0, isnull(t1.url, ''), '//' + isnull(t1.url, '')), '!w', '//w'), '!h', 'h') as URL ");
            sb.AppendLine(" ,'this.onerror=null;this.src=''../OrganizationDocuments/Thumbnail/DefaultOrganization.png?' + CONVERT(varchar, DATEDIFF(second, '1970-01-01', GETDATE())) + '''' as NoImage ");
            sb.AppendLine("  ,IIF(Isnull(t2.PackageID, 0) = 0, 2, t2.PackageID) PackageID ");
            sb.AppendLine("   from Organization_List t1 ");
            sb.AppendLine("  Inner Join Event_Organizations t2 On t2.Organization_pkey = t1.pkey ");
            //sb.AppendLine(" Left outer join Image_List i On i.pkey = t1.Image_pkey ");
            sb.AppendLine(" Left outer join Sys_ParticipationTypes t3 On t3.pkey = t2.ParticipationType_pKey ");
            sb.AppendLine("  Left outer join SYS_ParticipationLevels t4 on t2.ParticipationLevel_pkey = t4.pKey ");
            sb.AppendLine(" Where isnull(t2.ParticipationStatus_pKey,0)<> 2 and isnull(t2.ShowOnPublicPage,0)= 1 ");
            sb.AppendLine(" and t2.Event_pKey = " + EventID.ToString()); // +EventKey_Value

            if (level > 0)
                sb.AppendLine(" and isnull(t2.ParticipationLevel_pKey, 7)  =  " + level);

            if (audType != 0)
                sb.AppendLine(" and " + audType.ToString() + " in (select [value] from string_split(t2.[AudienceType],','))");

            if (solType != 0)
                sb.AppendLine("and t2.Event_Solution_pkeys Like('%" + solType.ToString() + "%')");

            sb.AppendLine("order by Profile");

            string dbquery = sb.ToString();
            SqlCommand cmd = new SqlCommand(dbquery, con);
            con.Open();
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {
                    string id = oReader["ipk"].ToString();
                    var file = Path.Combine(id + "_img.jpg");
                    string curFile = System.Web.HttpContext.Current.Server.MapPath("~/OrganizationDocuments/" + file);
                    //string curFile = @"D:/Keyideas/fcManager/fcConferenceManager/OrganizationDocuments/" + file;
                    //string curFile = @"~\Images\"+file;
                    bool _isexist = false;
                    if (File.Exists(curFile))
                    {
                        _isexist = true;

                    }

                    upcomingevents.Add(new EventSponsors
                    {
                        imgpath = file,
                        ImageExist = _isexist,
                        ipk = Convert.ToInt32(oReader["ipk"].ToString()),
                        URL = oReader["URL"].ToString(),
                        Level = oReader["ParticipationLevelID"].ToString(),
                        ParticipationLevel_pkey = oReader["ParticipationLevel_pkey"].ToString(),
                        ImgLogo = oReader["ImgLogo"].ToString(),
                        Profile = oReader["Profile"].ToString()
                        //Image = ObjectToByteArray(oReader["Image"])
                    });

                }
            }
            con.Close();
            return upcomingevents;
        }

        #endregion

        #region RESOURCES

        public List<Milestone> Milestone(string years)
        {
            List<Milestone> milestone = new List<Milestone>();
            string dbquery = string.Empty;
            if (years == "" || years == null)
            {
                dbquery = "select  datex , Text  from FCR_Milestones  order by Years";
            }
            else
            {
                string[] _year = years.Split(' ');
                dbquery = "select  datex , Text  from FCR_Milestones  where Years > " + Convert.ToInt32(_year[0]) + " AND Years <= " + Convert.ToInt32(_year[1]) + " order by Years";
            }
            SqlCommand cmd = new SqlCommand(dbquery, con);
            con.Open();
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {
                    milestone.Add(new Milestone
                    {
                        Years = oReader["datex"].ToString(),
                        Title = oReader["Text"].ToString()

                    });

                }
            }
            con.Close();
            return milestone;

        }
        public List<MAGINews> MAGINews()
        {
            DataTable dt = new DataTable();
            List<MAGINews> maginews = new List<MAGINews>();

            string dbquery = "Select Title ,mnt.Name as Topic  ,Publication ,URL1 from  FCR_MagiNews  fcn join MAGINews_Topic  mnt on mnt.pkey = fcn.Topic_pkey ";
            SqlCommand cmd = new SqlCommand(dbquery, con);
            con.Open();
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {
                    maginews.Add(new MAGINews
                    {
                        Title = oReader["Title"].ToString(),
                        Topic = oReader["Topic"].ToString(),
                        Author = "n/a",// oReader["Title"].ToString(),
                        Publication = oReader["Publication"].ToString(),
                        URL = oReader["URL1"].ToString()
                    });

                }
            }
            con.Close();
            return maginews;
        }

        public DataTable GetTopics()
        {
            List<string> clist = new List<string>();
            DataTable _dt = new DataTable();
            try
            {
                string dbquery = "Select * from MAGINews_Topic ";
                con.Open();
                SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);

                _da.Fill(_dt);
                con.Close();
            }
            catch (Exception ex)
            { }
            finally
            {
                con.Close();
            }
            return _dt;

        }
        public List<MAGINews> MAGINewsSearch(string title, string topic, string author, string publication)
        {
            List<MAGINews> maginews = new List<MAGINews>();
            DataTable _dt = new DataTable();
            string dbquery = string.Empty;
            if (topic == "")
            {
                dbquery = "Select  Title, mnt.Name as Topic  ,Publication ,URL1 from  FCR_MagiNews fcn join MAGINews_Topic  mnt on mnt.pkey = fcn.Topic_pkey where  Title like '%" + title + "%' AND Publication like '%" + publication + "%'";
            }
            else
            {
                dbquery = "Select  Title, mnt.Name as Topic  ,Publication ,URL1 from  FCR_MagiNews fcn join MAGINews_Topic  mnt on mnt.pkey = fcn.Topic_pkey where  mnt.pkey =" + topic + " AND Title like '%" + title + "%' AND Publication like '%" + publication + "%'";
            }
            SqlCommand cmd = new SqlCommand(dbquery, con);
            con.Open();
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {
                    maginews.Add(new MAGINews
                    {
                        Title = oReader["Title"].ToString(),
                        Topic = oReader["Topic"].ToString(),
                        Author = "n/a",// oReader["Title"].ToString(),
                        Publication = oReader["Publication"].ToString(),
                        URL = oReader["URL1"].ToString()
                    });

                }
            }
            con.Close();
            return maginews;

        }




        #endregion


        public Byte[] FetchCV(string MainEmail)
        {
            Byte[] CVcontent = new Byte[64];
            Array.Clear(CVcontent, 0, CVcontent.Length);
            string dbquery = "Select CVFile  from CreateAccount where MainEmail = @mainEmail";
            SqlCommand cmd = new SqlCommand(dbquery, con);
            cmd.Parameters.AddWithValue("@mainEmail", MainEmail);
            con.Open();
            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                if (oReader.Read())
                {
                    CVcontent = ObjectToByteArray(oReader["CVFile"]);
                }
            }
            con.Close();

            return CVcontent;
        }


        #region MyMGI 

        public List<My_History> MyHistory(DateTime dtStart, DateTime dtEnd, string strChange, string strEntName, int SelectedPageIndex, string Account_pKey)
        {
            DataTable _dt = new DataTable();
            List<My_History> myhistory = new List<My_History>();

            string strID = "";
            string strEntID = "";
            int offSet = 0;


            StringBuilder sb = new StringBuilder();
            sb.Append(" select t1.pKey, isNull(t2.ContactName,'Not Specified') as UpdatedBy, dateadd(minute,-@Offset,t1.UpdatedOn) as UpdatedOn, REPLACE( REPLACE( t1.Change,'<b>',''),'</b>','') as  Change, t1.EntityType_pkey, t1.Entity_pkey, FORMAT(t1.pKey, '00000') as PaddedID ");
            sb.AppendLine(" ,t3.AuditTargetTypeID as EntityType, FORMAT(t1.Entity_pkey,'00000') as EntityID ");
            sb.AppendLine(" ,dbo.GetEntityName(t1.EntityType_pkey, t1.Entity_pkey) as EntityName ,t1.UpdatedOn ");
            sb.AppendLine(" From Audit_Log t1 ");
            sb.AppendLine(" inner join Sys_AuditTargetTypes t3 on t3.pkey = t1.EntityType_pKey ");
            sb.AppendLine(" Left outer join Account_List t2 on t2.pkey = t1.UpdatedByAccount_pKey ");
            sb.AppendLine(" Where t1.UpdatedOn >= @Start And t1.UpdatedOn <= @End ");
            sb.AppendLine(" and t1.UpdatedByAccount_pKey=" + Account_pKey + "and (t1.SurrogateAccount_Pkey is null or t1.SurrogateAccount_Pkey=0) ");
            sb.AppendLine("  AND ( EntityType_pKey<>12 and  AuditEntryType_pKey<>201 ) ");


            if (strChange != "")
            {
                sb.AppendLine(" And t1.Change Like @Change ");
            }
            if (strID != "")
            {
                sb.AppendLine(" And cast(t1.pKey As varchar) Like @PK   ");
            }
            if (strEntID != "")
            {
                sb.AppendLine(" And cast(t1.Entity_pkey As varchar) Like @EPK  ");
            }
            if (strEntName != "")
            {
                sb.AppendLine(" And dbo.GetEntityName(t1.EntityType_pkey, t1.Entity_pkey) Like @EName ");
            }
            if (SelectedPageIndex > 0)
            {
                sb.AppendLine(" And t1.EntityType_pkey = " + SelectedPageIndex.ToString());
            }

            sb.AppendLine(" UNION  ");

            sb.AppendLine(" select top 1 t1.pKey, isNull(t2.ContactName,'Not Specified') as UpdatedBy, dateadd(minute,-@Offset,t1.UpdatedOn) as UpdatedOn, 'Updated schedule' as  Change, t1.EntityType_pkey, t1.Entity_pkey, FORMAT(t1.pKey, '00000') as PaddedID  ");
            sb.AppendLine(" ,t3.AuditTargetTypeID as EntityType, FORMAT(t1.Entity_pkey,'00000') as EntityID  ");
            sb.AppendLine(" , dbo.GetEntityName(t1.EntityType_pkey, t1.Entity_pkey) as EntityName ,t1.UpdatedOn  ");
            sb.AppendLine(" From Audit_Log t1  ");
            sb.AppendLine(" inner join Sys_AuditTargetTypes t3 on t3.pkey = t1.EntityType_pKey ");
            sb.AppendLine(" Left outer join Account_List t2 on t2.pkey = t1.UpdatedByAccount_pKey ");
            sb.AppendLine(" Where t1.UpdatedOn >= @Start And t1.UpdatedOn <= @End  ");
            sb.AppendLine(" and t1.UpdatedByAccount_pKey=" + Account_pKey + "and (t1.SurrogateAccount_Pkey is null or t1.SurrogateAccount_Pkey=0)  ");
            sb.AppendLine("  AND ( EntityType_pKey=12 and  AuditEntryType_pKey=201 ) ");


            string strChange1 = "";

            if (strChange1 != "")
            {
                sb.AppendLine("  And t1.Change Like @Change  ");
            }
            if (strID != "")
            {
                sb.AppendLine(" And cast(t1.pKey As varchar) Like @PK  ");
            }
            if (strEntID != "")
            {
                sb.AppendLine("  And cast(t1.Entity_pkey As varchar) Like @EPK ");
            }
            if (strEntName != "")
            {
                sb.AppendLine(" And dbo.GetEntityName(t1.EntityType_pkey, t1.Entity_pkey) Like @EName ");
            }
            if (SelectedPageIndex > 0)
            {
                sb.AppendLine("   And t1.EntityType_pkey = " + SelectedPageIndex.ToString());
            }

            sb.AppendLine(" Order by t1.pkey desc  ");

            string dbquery = sb.ToString();    // "select * from Event_Text where event_pkey=58  and Active=1  and sequence != 0 order by sequence";  // and Indicator=1 
            SqlCommand cmd = new SqlCommand(dbquery, con);
            cmd.Parameters.AddWithValue("@Offset", offSet);
            cmd.Parameters.AddWithValue("@Start", dtStart);
            cmd.Parameters.AddWithValue("@End", dtEnd);
            if (strChange != "") { cmd.Parameters.AddWithValue("@Change", "%" + strChange + "%"); }

            if (strID != "")
            {
                cmd.Parameters.AddWithValue("@PK", strID + "%");
            }
            if (strEntName != "")
            {
                cmd.Parameters.AddWithValue("@EName", "%" + strEntName + "%");
            }
            if (strEntID != "")
            {
                cmd.Parameters.AddWithValue("@EPK", strEntID + "%");
            }

            con.Open();
            //_dt.Load(cmd.ExecuteReader());

            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {
                    DateTime updatedon = Convert.ToDateTime(oReader["UpdatedOn"].ToString());
                    myhistory.Add(new My_History
                    {
                        UpdatedOn = updatedon.ToString("MM/dd/yyyy"),
                        Page = oReader["EntityType"].ToString(),
                        Action = oReader["Change"].ToString(),
                        ByName = oReader["UpdatedBy"].ToString()
                    });

                }
            }

            con.Close();

            return myhistory;

        }


        public DataTable BindPageDropdown(int Account_PKey)
        {
            DataTable _dt = new DataTable();
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(" SELECT t1.pKey, t1.AuditTargetTypeID as strText ");
                sb.AppendLine(" FROM SYS_AuditTargetTypes t1 ");
                sb.AppendLine(" where t1.pkey in(select EntityType_pKey from Audit_Log where UpdatedByAccount_pKey = " + Account_PKey.ToString() + ")");
                sb.AppendLine("  Order by strText ");
                string dbquery = sb.ToString();
                con.Open();
                SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
                _da.Fill(_dt);
                con.Close();
            }
            catch (Exception ex)
            { }
            finally
            {
                con.Close();
            }
            return _dt;

        }


        public List<My_Certificates> MyCertificates(string Account_Pkey)
        {
            // Account_Pkey = "24356";
            DataTable _dt = new DataTable();
            List<My_Certificates> myCertificate = new List<My_Certificates>();

            StringBuilder sb = new StringBuilder();
            sb.Append(" select 0 as Type, t1.Comment, t1.pkey,(isNull(t1.EarnedCEUs, 0) + isNull(t1.ManualAdjustment, 0)) as EarnedCEUs, (t5.CertName + ' (' + t5.CertAbbrev + ')') as CertName, t5.CertAbbrev, t4.EventID, t4.EndDate as CertDate  ");
            sb.AppendLine(" , t6.CertStatusID, t2.Account_pKey, t2.Event_pKey, t1.CertStatus_pkey,isnull(t1.HoldReason_pKey, '') as HoldReason,isnull(dbo.GETCertificate_Links_Fn(t1.HoldReason_pKey, t2.Event_pKey, t3.pkey, t1.pkey), '') as PageLink,t5.pkey as CertPkey  ");
            sb.AppendLine("  ,al.Firstname, al.Lastname, null as CRCPExpirationDate,0 as ExamStatus_pkey,'' as ExamCertificateText, null as LatestCertDate  ");
            sb.AppendLine("  ,iif(t4.EventType_pkey in (" + clsEvent.EventType_CloudConference.ToString() + ", " + clsEvent.EventType_Webinar.ToString() + ") and isnull(t5.pkey, 0) = 3, 1, 0) as Editable  ");
            sb.AppendLine(" ,isnull(t1.LiveEarned, 0) as LiveEarned,isnull(t1.NotLiveEarned, 0) as NotLiveEarned,isnull(t7.PRACredits, 0) as PRACredits,isnull(t7.IsOverrideHrs, 0) as IsOverrideHrs,isnull(t7.NotLiveCredits, 0) as NotLiveCredits,isnull(t1.IsUpdatedByUser, 0) as IsUpdatedByUser  ");
            // sb.AppendLine(" --'qry = qry + vbCrLf + ", iif(isnull(t5.pkey, 0) = 3, isnull(t1.NotLiveEarned, 0) + isNull(t1.EarnedCEUs, 0) + isNull(t1.ManualAdjustment, 0), isNull(t1.EarnedCEUs, 0) + isNull(t1.ManualAdjustment, 0)) as TotalHrs   ");
            sb.AppendLine(" ,isnull(t7.CombinedCredits, 0) As CombinedCredits, isnull(t1.CerticationSessions, '') as CertificationSessions ,t4.EventFullName  ");
            sb.AppendLine("    From EventAccount_Certifications t1   ");
            sb.AppendLine("  inner join Event_Accounts t2 on t2.pkey = t1.EventAccount_pKey  ");
            sb.AppendLine(" Inner join Account_List t3 on t3.pkey = t2.Account_pKey   ");
            sb.AppendLine(" Inner join Event_List t4 on t4.pkey = t2.Event_pKey  ");
            sb.AppendLine("  Inner join Certification_List t5 on t5.pkey = t1.Certification_pKey ");
            sb.AppendLine(" Inner Join Sys_CertStatuses t6 on t6.pkey = t1.CertStatus_pkey  ");
            sb.AppendLine(" Left Outer join Certification_Detail t7 on t5.pKey = t7.Certification_pKey and t7.Event_pKey = t2.Event_pKey  ");
            sb.AppendLine("  Inner join Account_List al on al.pkey = t2.Account_pKey  ");
            sb.AppendLine("  Where t2.Account_pKey = " + Account_Pkey);
            sb.AppendLine("  And t6.DisplayOnAttendeeView = 1 ");
            sb.AppendLine("  And isnull(t7.ShowOnMyCertificate,1)= 1  ");
            sb.AppendLine("  And t4.EventStatus_pKey Not in (4, 5)  ");
            sb.AppendLine(" And t1.CertStatus_pkey in(" + clsCertification.CERTSTATUS_Approved.ToString() + ", " + clsCertification.CERTSTATUS_Issued.ToString() + ", " + clsCertification.CERTSTATUS_Hold.ToString() + ", " + clsCertification.CERTSTATUS_Received.ToString() + ")  ");

            sb.AppendLine(" UNION  ");

            sb.AppendLine(" select 1 as Type, 'Attendance Certificate' as Comment, t1.pkey, t1.SchedHours as EarnedCEUs, 'Attendance' as CertName  ");
            sb.AppendLine(" , 'Attendance' as CertAbbrev, t2.EventID, t2.EndDate as CertDate, 'Issued' as CertStatusID, t1.Account_pKey, t1.Event_pKey, " + clsCertification.CERTSTATUS_Issued.ToString() + " as CertStatus_pkey,'' as HoldReason,'' as PageLink,0 as CertPkey  ");
            sb.AppendLine(" ,al.Firstname, al.Lastname, null as CRCPExpirationDate,0 as ExamStatus_pkey,'' as ExamCertificateText,null as LatestCertDate,0 as Editable  ");
            sb.AppendLine(" ,0 as LiveEarned,0 as NotLiveEarned,0 as PRACredits,0 as IsOverrideHrs,0 as NotLiveCredits,0 as IsUpdatedByUser,0 as CombinedCredits,'' as CertificationSessions ,t2.EventFullName  ");
            sb.AppendLine("  From Event_Accounts t1   ");
            sb.AppendLine(" Inner join Event_List t2 on t2.pkey = t1.Event_pKey   ");
            sb.AppendLine(" Inner join Account_List al on al.pkey = t1.Account_pKey  ");
            sb.AppendLine(" Where t1.Account_pKey = " + Account_Pkey);
            sb.AppendLine("  and t1.ParticipationStatus_pKey = " + clsEventAccount.PARTICIPATION_Attending.ToString());
            sb.AppendLine(" And isnull(t2.ShowAttCerts,1)= 1  ");
            sb.AppendLine("  And t2.EndDate < Getdate()  ");
            sb.AppendLine("  And t2.EventStatus_pKey Not in (4, 5)  ");


            sb.AppendLine(" UNION  ");

            sb.AppendLine(" select 2 as Type, 'Speaker Certificate' as Comment, t1.pkey, Null as EarnedCEUs, 'Speaker' as CertName  ");
            sb.AppendLine(" , 'Speaker' as CertAbbrev, t2.EventID, t2.EndDate as CertDate, 'Issued' as CertStatusID, t1.Account_pKey, t1.Event_pKey, " + clsCertification.CERTSTATUS_Issued.ToString() + " as CertStatus_pkey,'' as HoldReason,'' as PageLink,0 as CertPkey  ");
            sb.AppendLine(" ,al.Firstname, al.Lastname, null as CRCPExpirationDate,0 as ExamStatus_pkey,'' as ExamCertificateText,null as LatestCertDate,0 as Editable  ");
            sb.AppendLine(" ,0 as LiveEarned,0 as NotLiveEarned,0 as PRACredits,0 as IsOverrideHrs,0 as NotLiveCredits,0 as IsUpdatedByUser,0 as CombinedCredits,'' as CertificationSessions ,t2.EventFullName  ");
            sb.AppendLine("  From Event_Accounts t1  ");
            sb.AppendLine(" Inner join Account_List al on al.pkey = t1.Account_pKey  ");
            sb.AppendLine(" Inner join Event_List t2 on t2.pkey = t1.Event_pKey  ");
            sb.AppendLine(" Inner join dbo.getEventSpeakers() t3 on t3.event_pkey = t1.Event_pkey and t3.account_pkey = t1.account_pkey  ");
            sb.AppendLine("  Where t1.Account_pKey = " + Account_Pkey + "AND t2.EndDate < Getdate() ");
            sb.AppendLine("  And t2.EventStatus_pKey Not in (4, 5)  ");


            sb.AppendLine(" UNION  ");


            sb.AppendLine(" select 3 as Type, 'CRCP Certificate' as Comment, t1.pkey, Null as EarnedCEUs, iif(isnull(t1.ExamStatus_pkey, 0) = 2, '(CRCP not passed)', 'CRCP') as CertName  ");
            sb.AppendLine(" , 'CRCP' as CertAbbrev, Null as EventID, t1.ExamDate as CertDate, t2.ExamStatusID as CertStatusID, t1.Account_pKey, 0 as Event_pKey, iif(isnull(t1.ExamStatus_pkey, 0) = 2, 0, 7) as CertStatus_pkey,'' as HoldReason,'' as PageLink,0 as CertPkey  ");
            sb.AppendLine(" ,al.Firstname, al.Lastname, t1.ExamExpDate as CRCPExpirationDate,t1.ExamStatus_pkey,t2.ExamCertificateText,isnull((select max(examdate) as maxexam from Account_ExamResults where exam_pkey = t1.exam_pkey and ExamStatus_pKey in (1, 6)  ");
            sb.AppendLine(" and account_pkey = t1.Account_pKey),t1.ExamDate) as LatestCertDate,0 as Editable  ");
            sb.AppendLine(" ,0 as LiveEarned,0 as NotLiveEarned,0 as PRACredits,0 as IsOverrideHrs,0 as NotLiveCredits,0 as IsUpdatedByUser,0 as CombinedCredits,'' as CertificationSessions,'' as EventFullName  ");
            sb.AppendLine(" From Account_ExamResults t1  ");
            sb.AppendLine(" Inner join sys_examStatuses t2 on t2.pkey = t1.ExamStatus_pkey  ");
            sb.AppendLine("  Inner join Account_List al on al.pkey = t1.Account_pKey ");
            sb.AppendLine(" Where t1.Account_pKey = " + Account_Pkey);
            sb.AppendLine(" And t1.ExamStatus_pkey in(" + clsExam.EXAMSTATUS_PassGroup + ", 2)  ");
            sb.AppendLine(" Order by CertDate desc, Certname  ");

            string dbquery = sb.ToString();
            SqlCommand cmd = new SqlCommand(dbquery, con);
            con.Open();
            //_dt.Load(cmd.ExecuteReader());

            using (SqlDataReader oReader = cmd.ExecuteReader())
            {
                while (oReader.Read())
                {
                    string PendingCert_pKey = oReader["EventID"].ToString();
                    string Pending_Account_pKey = oReader["Account_pKey"].ToString();
                    string Pending_Event_pKey = oReader["Event_pKey"].ToString();
                    string Pendning_CertAbbrev = oReader["CertAbbrev"].ToString();
                    string strEventId = oReader["EventID"].ToString();
                    string strPendingAcctName = oReader["Firstname"].ToString() + " " + oReader["Lastname"].ToString();

                    string filename = clsUtility.CleanFilename(Pending_Event_pKey + "_" + Pending_Account_pKey + "_" + Pendning_CertAbbrev + ".pdf");
                    string getDisplayFilename = clsUtility.CleanFilename(strEventId + "_" + Pendning_CertAbbrev + "_" + strPendingAcctName + "_" + DateTime.Now.ToString("yyMMdd") + ".pdf");

                    DateTime certDate = Convert.ToDateTime(oReader["CertDate"].ToString());
                    string EarnedCEUs = oReader["EarnedCEUs"].ToString();
                    if (EarnedCEUs == null || EarnedCEUs == "")
                    {
                        EarnedCEUs = "0.0";
                    }
                    double dEarnedCEUs = Convert.ToDouble(EarnedCEUs.ToString());
                    //double hr = Convert.ToDouble((dEarnedCEUs.ToString("N2")));//  ;
                    myCertificate.Add(new My_Certificates
                    {
                        pkey = Convert.ToInt32(oReader["pkey"].ToString()),
                        Name = strPendingAcctName,
                        Account_pkey = Account_Pkey.ToString(),
                        CertFileName = filename,
                        Date = certDate.ToString("MM/dd/yyyy"),
                        DownloadCertificate = oReader["CertName"].ToString(),    //  Pendning_CertAbbrev, //,
                        Hours = Convert.ToDouble(dEarnedCEUs.ToString("N1")),
                        Status = oReader["CertStatusID"].ToString(),
                        Comment = oReader["PageLink"].ToString(),
                        EventID = oReader["EventID"].ToString(),
                        CertPKEy = oReader["CertPkey"].ToString(),
                        EventPKey = oReader["Event_pKey"].ToString(),
                        Editable = oReader["Editable"].ToString(),
                        ISUpdatedByUSer = oReader["ISUpdatedByUSer"].ToString(),
                        Type = oReader["Type"].ToString()
                    }); ;

                }
            }

            con.Close();


            return myCertificate;
        }

        public DataTable BindCountry()
        {
            DataTable _dt = new DataTable();
            try
            {

                string dbquery = "select t1.pKey, t1.CountryID as strText from sys_Countries t1  order by isNull(t1.sortorder,999), strText ";

                con.Open();
                SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
                _da.Fill(_dt);
                con.Close();
            }
            catch (Exception ex)
            { }
            finally
            {
                con.Close();
            }
            return _dt;
        }

        public DataTable BindSites()
        {
            DataTable _dt = new DataTable();
            try
            {
                string dbquery = "SELECT pKey, SiteOrganizationID as strText FROM Sys_SiteOrgType Order by pKey ";
                con.Open();
                SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
                _da.Fill(_dt);
                con.Close();
            }
            catch (Exception ex)
            { }
            finally
            {
                con.Close();
            }
            return _dt;
        }

        public DataTable BindSiteTypes()
        {
            DataTable _dt = new DataTable();
            try
            {
                string dbquery = " select t1.pKey, t1.OrganizationTypeID as strText from sys_OrganizationTypes t1 order by SortOrder ";
                con.Open();
                SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
                _da.Fill(_dt);
                con.Close();
            }
            catch (Exception ex)
            { }
            finally
            {
                con.Close();
            }
            return _dt;
        }
        public DataTable BindStates()
        {
            DataTable _dt = new DataTable();
            try
            {
                string dbquery = " select t1.pKey, t1.StateID as strText from sys_states t1 ";
                con.Open();
                SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
                _da.Fill(_dt);
                con.Close();
            }
            catch (Exception ex)
            { }
            finally
            {
                con.Close();
            }
            return _dt;
        }

        public DataTable BindTimeZones(string country_Pkey)
        {
            DataTable _dt = new DataTable();
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("  SELECT tz.Pkey,(tz.CountryCode + ' (' + tz.TimeZone + ')' + ' UTC offset: Std ' + Convert(varchar, tz.UTCOffset) + ', Dst ' + Convert(varchar, tz.UTCDSToffset)) As strText ");
                sb.Append(" from SYS_CountryTimeZone tz ");
                sb.Append(" inner join sys_Countries c on tz.CountryCode = c.countrycode ");
                sb.Append(" where tz.active = 1 and c.pKey = " + country_Pkey);

                string dbquery = sb.ToString();
                con.Open();
                SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
                _da.Fill(_dt);
                con.Close();
            }
            catch (Exception ex)
            { }
            finally
            {
                con.Close();
            }
            return _dt;
        }


        public DataTable BindEventID()
        {
            DataTable _dt = new DataTable();
            string dbquery = "  SELECT pKey, EventID as strText  FROM Event_List where EventStatus_pKey NOT IN(4,5) ORDER BY StartDate DESC ";
            try
            {
                con.Open();
                SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
                _da.Fill(_dt);
                con.Close();
            }
            catch (Exception ex)
            { }
            finally
            {
                con.Close();
            }
            return _dt;

        }
        #endregion

        #region MyPayments

        public DataTable MyPayments(string Account_Pkey, string event_pKey)
        {
            DataTable _dt = new DataTable();
            List<My_Payments> mypayments = new List<My_Payments>();
            StringBuilder sb = new StringBuilder();
            sb.Append("   select t1.pKey, t1.ChargeType_pkey, t2.TypeOfCharge ");
            sb.AppendLine(" ,t1.LoggedOn as TransDate, isNull(t1.Reversed, 0) as IsReversed, isNull(t1.ReversalReference, 0) as RevReference  ");
            sb.AppendLine(" , isNull(t1.Event_pkey, 0) As Evt_pKey, isNull(t3.EventID, 'Not Specified') as EvtID  ");
            sb.AppendLine(" , t2.ChargeTypeID,(Case when t2.ChargeTypeID = 'Group Discount' then isnull(t1.DiscountCodeApplied,'') else '' End) as groupDiscount, t1.Memo, isNull(t1.Amount, 0) as RowAmt  ");
            sb.AppendLine(" , t1.LoggedByAccount_pKey, isNull(t4.ContactName, 'Not Specified') as LoggedBy  ");
            sb.AppendLine(" ,(Case when t2.pkey in(" + clsPrice.CHARGE_Payment.ToString() + ", " + clsPrice.CHARGE_MoveCash.ToString() + ") then isNull(t1.Amount,0) else Null End) as PaymentAmt  ");
            sb.AppendLine(" ,(Case when t2.pkey in(" + clsPrice.CHARGE_Payment.ToString() + ", " + clsPrice.CHARGE_MoveCash.ToString() + ") then Null else isNull(t1.Amount, 0) End) as ChargeAmt  ");
            sb.AppendLine("  ,0.0 as Balance ");


            sb.AppendLine(" ,(Case when t2.pkey in(" + clsPrice.CHARGE_Payment.ToString() + ", " + clsPrice.CHARGE_MoveCash.ToString() + ") then 'R' + convert(varchar, t1.ReceiptReference)  ");
            sb.AppendLine("  when(t2.pkey in (" + clsPrice.CHARGE_FullReg.ToString() + ") and(select count(1) from Account_Charges where isnull(ReceiptReference, 0) <> 0 and isNull(ReversalReference, 0) = 0 and  isNull(Reversed, 0) = 0 and isNull(IsDelete, 0) = 0  and account_pkey = t1.Account_pKey and Event_pkey = t1.Event_pkey) = 0) then  ");
            sb.AppendLine("  (select top(1) iif(isnull(Paid, 0) = 1, 'R', 'N') + convert(varchar, ReceiptNumber) from Account_Payments ap cross apply dbo.csvtonumbertable(ap.intendedaccounts, ',') x  where x.Num = " + Account_Pkey + " and isnull(ap.IsDelete, 0) = 0 and ap.Event_pkey = t1.Event_pkey order by ap.pKey desc) ");
            sb.AppendLine(" when isnull(t1.ReceiptReference,0)<> 0 then(select top(1) iif(isnull(Paid, 0) = 1, 'R', 'N') + convert(varchar, ReceiptNumber) from Account_Payments where ReceiptNumber = t1.ReceiptReference)  ");
            sb.AppendLine("  else Null End) as PaymentReference  ");

            sb.AppendLine(" ,(Case When isNull(t1.Amount,0)> 0 and iif(t2.TypeOfCharge= 3, P.ReceiptNumber, t1.ReceiptReference) > 0 and not(isNull(t1.ReversalReference,0)> 0 or isNull(t1.Reversed,0)= 1) then 'paid' when isNull(t1.Amount,0)< 0 and iif(t2.TypeOfCharge= 3, P.ReceiptNumber, t1.ReceiptReference) > 0 and not(isNull(t1.ReversalReference,0)> 0 or isNull(t1.Reversed,0)= 1) then 'Due'  else null  end) as Status  ");
            sb.AppendLine(" From Account_Charges t1  ");
            sb.AppendLine(" Cross Apply dbo.getReceiptNumber(t1.Account_pKey, t1.Event_pKey, 6) p  ");
            sb.AppendLine("  Inner Join Account_List t5 on t5.pKey = t1.Account_pkey And t5.pKey =" + Account_Pkey);
            sb.AppendLine(" Inner Join SYS_ChargeTypes t2 on t2.pKey = t1.ChargeType_pkey  ");
            sb.AppendLine("  Left Outer Join Event_List t3 on t3.pKey = t1.Event_pkey ");
            sb.AppendLine(" Left Outer Join Account_List t4 on t4.pKey = t1.LoggedByAccount_pKey  ");
            sb.AppendLine(" Where isnull(t1.IsHide,0)<> 1 and isnull(IsDelete,0)<> 1 and t1.Account_pkey = " + Account_Pkey);
            if (event_pKey != "" && event_pKey != null)
            {
                sb.AppendLine(" AND t3.pKey= " + event_pKey);
            }
            sb.AppendLine(" Order by t1.pKey desc ");
            string dbquery = sb.ToString();
            SqlCommand cmd = new SqlCommand(dbquery, con);
            con.Open();
            cmd.CommandTimeout = 150;
            _dt.Load(cmd.ExecuteReader());

            con.Close();

            return _dt;
        }

        public DataTable PaymentType(int AccountPkey, int EventPkey)
        {
            DataTable dt = new DataTable();

            StringBuilder sb = new StringBuilder();
            sb.Append(" DECLARE @Charges VARCHAR(200)  ");
            sb.AppendLine(" SELECT @Charges = COALESCE(@Charges+', ' ,'') +  ISNULL(IntendedCharges,'0')  ");
            sb.AppendLine("  from Account_Charges where ChargeType_pKey=13 and Account_pKey =@AccountPKey and Event_pKey=@EventPKey  ");
            sb.AppendLine(" and isnull(Reversed,0)=0 and isnull(ReversalReference,0)=0 and isnull(IsDelete,0)=0  ");
            sb.AppendLine(" select t1.ChargeType_pKey,t1.Pkey as AcctChargePkey  ");
            sb.AppendLine(" ,t2.ChargeTypeID+' ($'+cast(iif(t1.ChargeType_pKey=1,(select abs(sum(Amount)) from Account_Charges where Account_pKey=@AccountPKey   ");
            sb.AppendLine(" and isNull(Reversed,0) = 0 And isNull(IsDelete,0) = 0 And isNull(ReversalReference,0) = 0 and Event_pKey=@EventPKey and ChargeType_pKey in  ");
            sb.AppendLine(" (select pKey from SYS_ChargeTypes Where TypeOfCharge in (2,3) or isnull(IsOtherDiscount,0)=1 or pKey=1)),abs(t1.amount)) as varchar)+')' as ChargeTypeID  ");
            sb.AppendLine(" ,iif(t1.ChargeType_pKey=1,(select abs(sum(Amount)) from Account_Charges where Account_pKey=@AccountPKey    ");
            sb.AppendLine(" and isNull(Reversed,0) = 0 And isNull(IsDelete,0) = 0 And isNull(ReversalReference,0) = 0 and Event_pKey=@EventPKey and ChargeType_pKey in  ");
            sb.AppendLine(" (select pKey from SYS_ChargeTypes Where TypeOfCharge in (2,3) or isnull(IsOtherDiscount,0)=1 or pKey=1)),abs(t1.amount)) as Amount  ");
            sb.AppendLine(" from Account_Charges t1  ");
            sb.AppendLine(" inner join Sys_chargetypes t2 on t1.ChargeType_pKey=t2.pKey  ");
            sb.AppendLine(" where (t1.ChargeType_pKey in (1,2,3,4,5,17,18,19,20) or t2.TypeOfCharge=1) and t1.Event_pKey = @EventPKey");
            sb.AppendLine(" and Account_pKey = @AccountPKey  ");
            sb.AppendLine(" and t1.ChargeType_pKey not in (select num from dbo.csvtonumbertable(@Charges,','))   ");
            sb.AppendLine(" and isNull(t1.Reversed,0) = 0 And isNull(t1.IsDelete,0) = 0 And isNull(t1.ReversalReference,0) = 0  ");

            string dbquery = sb.ToString();

            SqlParameter[] parameters = new SqlParameter[] {
            new SqlParameter("@AccountPKey", AccountPkey.ToString()),
            new SqlParameter("@EventPKey", EventPkey.ToString()),
             };
            dt = SqlHelper.ExecuteTable(dbquery, CommandType.Text, parameters);

            return dt;
        }


        public DataTable OtherRecipts(string Account_Pkey)
        {
            DataTable dt = new DataTable();
            StringBuilder sb = new StringBuilder();
            sb.Append(" SELECT t1.pKey,t1.PaymentDate,t1.Event_pKey,t3.EventID,t2.PaymentMethodID,t1.IntendedAccounts  ");
            sb.AppendLine(" ,(Select dbo.fnGetParticipentName('' + ISNULL(t1.IntendedAccounts, 0) + '', ',')) as AttendeeName ");
            sb.AppendLine(" , (iif(t1.paid = 1, 'R', 'N') + convert(varchar(10), t1.ReceiptNumber)) as strReceipt,t1.ReceiptNumber,t1.Amount ");
            sb.AppendLine("  FROM Account_Payments t1 ");
            sb.AppendLine("  Left Outer Join SYS_PaymentMethods t2 on t2.pKey = t1.PaymentMethod_pkey ");
            sb.AppendLine("  Left Outer Join Event_List t3 on t3.pKey = t1.Event_pkey ");
            sb.AppendLine("  where t1.LoggedByAccount_pKey = " + Account_Pkey);
            sb.AppendLine("  And " + Account_Pkey + " Not In(Select Num from dbo.csvtonumbertable(isNull(t1.IntendedAccounts,''),','))  ");
            sb.AppendLine("  Order by t1.pKey desc  ");
            string dbquery = sb.ToString();
            SqlCommand cmd = new SqlCommand(dbquery, con);
            clsUtility.GetDataTable(con, cmd, ref dt);

            return dt;

        }


        public void Update_AccountPayment(double dblAmount, int intAccount_PKey, int intReceiptNumber, int intCurEventPKey, string strRefundCardTransactionID, int intRefundReceiptNumber, string strResponseCardType)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UPDATE ACCOUNT_PAYMENTS set RefundAmount=" + dblAmount.ToString());
            sb.AppendLine(",UpdatedBy=" + intAccount_PKey.ToString());
            sb.AppendLine(",UpdatedOn=GETDATE()");
            sb.AppendLine(" where ReceiptNumber=" + intReceiptNumber.ToString());
            sb.AppendLine(" and Event_Pkey=" + intCurEventPKey.ToString());
            sb.AppendLine(";UPDATE Payment_CardInfo set RefundTransactionID=" + strRefundCardTransactionID);
            sb.AppendLine(",RefundCardType=@CardType");
            sb.AppendLine(" where ReceiptNumber=" + intRefundReceiptNumber.ToString());
            string dbquery = sb.ToString();
            SqlCommand cmd = new SqlCommand(dbquery);
            cmd.Parameters.AddWithValue("@CardType", strResponseCardType);

            clsUtility.ExecuteQuery(cmd, null, "Update refund transaction id");
        }


        #endregion



        #region ConferenceBook


        public DataTable Booklet(int Event_pKey)
        {
            DataTable dt = new DataTable();
            StringBuilder sb = new StringBuilder();
            sb.Append(" select t1.pkey as pkey,t1.FileName as FileName,t2.Usage as Name from Event_GuideLines t1 ");
            sb.AppendLine(" inner join sys_GuideLineType t2 on t2.pKey = t1.UsageType  ");
            sb.AppendLine(" where t1.Status = 1 and Event_pKey = " + Event_pKey);
            sb.AppendLine("  and t1.UsageType = " + clsEvent.UsageType_Booklet.ToString() + " order by Sequence   ");

            string dbquery = sb.ToString();
            SqlCommand cmd = new SqlCommand(dbquery, con);
            clsUtility.GetDataTable(con, cmd, ref dt);

            return dt;
        }


        public DataTable Mealtable(int accountPkey, int EventPkey)
        {

            DataTable dt = new DataTable();
            StringBuilder sb = new StringBuilder();
            sb.Append(" select t1.pkey, t3.SpecialMealID, t2.Title as SessionTitle, DATENAME(dw, t2.StartTime) as DayName, t3.ImageFilename  ");
            sb.AppendLine(" From EventSession_Accounts t1  ");
            sb.AppendLine(" inner join Event_Sessions t2 on t2.pkey = t1.EventSession_pkey    ");
            sb.AppendLine(" inner join sys_specialmeals t3 on t3.pkey = t1.specialmeal_pkey  ");
            sb.AppendLine(" inner join Session_List t4 on t4.pkey = t2.Session_pKey    ");
            sb.AppendLine(" where t1.Account_pKey = " + accountPkey);
            sb.AppendLine(" and t2.Event_pKey =  " + EventPkey);
            sb.AppendLine(" order by t2.StartTime  ");

            string dbquery = sb.ToString();
            SqlCommand cmd = new SqlCommand(dbquery, con);
            clsUtility.GetDataTable(con, cmd, ref dt);

            return dt;

        }

        public DataTable Sessions_and_FixedHeader(int intCurEventPKey, string intAccount_PKey, int intTab, int TAB_Attending, bool bHasSpecialMeal, string intEventAccount_pKey, string intEvent_PKey, bool IsMealshow)
        {
            string dtCurrentTime = clsEvent.getEventVenueTime().ToString();
            int DOC_Faculty = -2;
            int DOC_Schedule = -3;
            int DOC_Meals = -4;
            int DOC_Cert = -5;
            DataTable dt = new DataTable();
            // intAccount_PKey = "24356";
            StringBuilder sb = new StringBuilder();
            sb.Append("  Select distinct 'z'+t1.SessionID as SortOrder, (t2.Track_Prefix+t1.SessionID) As SessionID, t2.Title as SessionTitle, t2.pKey as EvtSessionPKey  ");
            sb.AppendLine(" ,(cast(t2.pKey as varchar) + '-'+ t2.Track_Prefix+t1.SessionID) as EvtSessionInfo  ");
            sb.AppendLine(" ,'NA' as DisplaySize, 0 as FExists, (case when t3.Attending= 1 Then 'Y' else 'N' End) as isAttending, isNull(t2.FPageCount,0) as NumPages, isNull(t2.FSize,0) as FSize  ");
            sb.AppendLine("  ,  dbo.GetDowloads_Document(t2.pkey,t1.SessionID ,t2.Track_pKey,t2.Event_pKey) AS Downloads ");

            sb.AppendLine(" ,(case when t2.EndTime < '" + dtCurrentTime + "' Then isnull(t5.VideoURL,'') else '' End) as RecURL,isnull(t5.VideoTitle,'') as RecTitle  ");
            sb.AppendLine(" ,(case when datediff(y,t2.EndTime, '" + dtCurrentTime + "')>=7 and len((case when t2.EndTime < '" + dtCurrentTime + "' Then isnull(t5.VideoURL,'') else '' End))>0 and Isnull(t5.RecordType,0)=2 Then 'True' else 'False' End) as RecVisible ");

            sb.AppendLine("  From Session_List t1 ");
            sb.AppendLine("  Inner Join Event_Sessions t2 on t2.Session_pkey = t1.pkey ");
            sb.AppendLine(" Inner Join EventSession_Accounts t3 on t3.EventSession_pkey = t2.pkey ");
            sb.AppendLine(" Inner Join sys_Tracks t4 on t4.pkey = t1.Track_pkey ");
            sb.AppendLine(" left outer join Event_Recording t5 on t5.Session_pkey=t1.pKey and t5.Event_pkey =" + intCurEventPKey.ToString());

            sb.AppendLine(" Where t2.Event_pKey = " + intCurEventPKey.ToString());
            sb.AppendLine(" And t3.Account_pKey = " + intAccount_PKey.ToString());
            sb.AppendLine("  And t4.Educational=1  ");

            sb.AppendLine(" And t3.Slides=1 ");
            sb.AppendLine("  and isNull(t3.Attending,0) = " + (intTab == TAB_Attending ? "1" : "0"));
            if (intTab == TAB_Attending)
            {
                sb.AppendLine(" UNION Select 'a2' as sortorder, 'Schedule' as SessionID, 'My Conference Schedule' as SessionTitle, " + DOC_Schedule.ToString() + " as EvtSessionPKey, '' as EvtSessionInfo, 'NA' as DisplaySize, 0 as FExists, 'N' as isAttending, isNull(FPagesSched,0) as NumPages, isNull(FSizeSched,0) as FSize,'' as Downloads,'' as RecURL,'' as RecTitle,'' as RecVisible  from Event_Accounts where pkey = " + intEventAccount_pKey);
            }
            sb.AppendLine("  UNION Select 'a3' as sortorder, 'Faculty' as SessionID, 'Faculty Biographies' as SessionTitle, " + DOC_Faculty.ToString() + " as EvtSessionPKey, '' as EvtSessionInfo, 'NA' as DisplaySize, 0 as FExists, 'Y' as isAttending, isNull(FPagesFaculty, 0) as NumPages, isNull(FSizeFaculty, 0) as FSize, '' as Downloads, '' as RecURL, '' as RecTitle, '' as RecVisible  from Event_List where pkey = " + intEvent_PKey);

            if (intTab == TAB_Attending)
            {
                sb.AppendLine(" UNION Select 'a4' as sortorder, 'Certificate' as SessionID, 'Certificate' as SessionTitle, " + DOC_Cert.ToString() + " as EvtSessionPKey,'' as EvtSessionInfo, 'NA' as DisplaySize, 0 as FExists, 'Y' as isAttending, 0 as NumPages, 0 as FSize,'' as Downloads,'' as RecURL,'' as RecTitle,'' as RecVisible  ");
            }
            if (intTab == TAB_Attending && bHasSpecialMeal && IsMealshow)
            {
                sb.AppendLine(" UNION Select 'a5' as sortorder, 'Special Meal' as SessionID, 'Special Meals' as SessionTitle, " + DOC_Meals.ToString() + " as EvtSessionPKey,'' as EvtSessionInfo, 'NA' as DisplaySize, 0 as FExists, 'N' as isAttending, 0 as NumPages, 0 as FSize,'' as Downloads,'' as RecURL,'' as RecTitle,'' as RecVisible    ");
            }

            sb.AppendLine("  Order by SortOrder ");
            string dbquery = sb.ToString();
            SqlCommand cmd = new SqlCommand(dbquery, con);
            clsUtility.GetDataTable(con, cmd, ref dt);

            return dt;

        }


        public bool save_size_info(double dblSize, int intPages, int AccountPkey)
        {
            bool saved = false;
            string q = "Update event_accounts set FSizeSched = " + dblSize.ToString() + ", FPagesSched=" + intPages.ToString() + " where pkey = " + AccountPkey.ToString();
            SqlCommand cmd = new SqlCommand(q, con);
            if (clsUtility.ExecuteQuery(cmd, null, "Update size (Sched)"))
            {
                saved = true;
            }


            return saved;
        }


        #endregion

        #region MyFAQs

        public DataTable MyFAQWithQuestion(int CurrentEventPkey, bool IsAttendee, bool bSponsor, bool bSpeaker, bool StaffMember, string AccountType, string strEventStart, string strEventEnd)
        {
            DataTable dt = new DataTable();
            StringBuilder qry = new StringBuilder();
            string categoryFilter = "";
            categoryFilter = ((IsAttendee) ? ",1" : "") + ((bSponsor) ? ",7" : "") + ((bSpeaker) ? ",2,10" : "") + ((StaffMember) ? ",3" : "");
            qry.Append("Select distinct t1.pKey as FAQCategory_pKey, t1.FAQCategoryID, t1.SortOrder, t2.FAQq as Question ,t2.pKey As FAQ_pKey, t2.FAQa, t2.FAQLink, (Case When isNull(t2.FAQLink,'') <> '' then 1 else 0 end) as hasLink, t2.FAQCategory_pkey, t2.SortOrder From SYS_FAQCategories t1 ");
            qry.Append(" Inner Join FAQ_List t2 on (t1.pKey IN (SELECT value from string_split(t2.FAQCategory_pkey,',')))");
            qry.Append(" Inner Join FAQ_ParticipationTypes t3 on t3.FAQ_pKey = t2.pKey");
            qry.Append(" cross apply dbo.CSVToStringTable(IIF(t2.EventTypes_pKey ='' OR t2.EventTypes_pKey IS NULL,'1,4,5,3,2',t2.EventTypes_pKey),',')x ");
            qry.Append(" Where t3.AccountParticipationType_pkey in (" + AccountType + ")");
            qry.Append(" and isNull(t2.MobileOnly, 0) = 0 and isNull(t2.IsActive,0) = 1 and isNull(t2.Event_pkey," + CurrentEventPkey.ToString() + ") = " + CurrentEventPkey.ToString());
            qry.Append(" and (x.String IN (SELECT value From string_split((Select Case When EventType_pKey IN (3,5) Then '3,5' When  EventType_pKey IN (1,2,4) Then '1,2,4' ELSE '1,4,5,3,2' END from Event_List where pKey=" + CurrentEventPkey.ToString() + "),',')) OR (TRIM(t2.EventTypes_pKey)='' OR t2.EventTypes_pKey IS NULL))");
            qry.Append(" and (isNull(t2.DuringConferenceOnly,0) = 0 or (getdate() between " + strEventStart + " and " + strEventEnd + "))");
            qry.Append(" Order by t1.SortOrder, t1.FAQCategoryID");
            SqlParameter[] parameters = new SqlParameter[] { };
            string s = qry.ToString();
            dt = SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
            return dt;
        }
        public DataTable MyFAQs(int CurrentEventPkey, bool IsAttendee, bool bSponsor, bool bSpeaker, bool StaffMember, string AccountType, string strEventStart, string strEventEnd)
        {
            DataTable dt = new DataTable();
            StringBuilder qry = new StringBuilder();
            string categoryFilter = "";
            categoryFilter = ((IsAttendee) ? ",1" : "") + ((bSponsor) ? ",7" : "") + ((bSpeaker) ? ",2,10" : "") + ((StaffMember) ? ",3" : "");
            qry.Append("Select distinct t1.pKey as FAQCategory_pKey, t1.FAQCategoryID, t1.SortOrder From SYS_FAQCategories t1 ");
            qry.Append(System.Environment.NewLine + " Inner Join FAQ_List t2 on (t1.pKey IN (SELECT value from string_split(t2.FAQCategory_pkey,',')))"); //string_split
            qry.Append(System.Environment.NewLine + " Inner Join FAQ_ParticipationTypes t3 on t3.FAQ_pKey = t2.pKey");
            qry.Append(System.Environment.NewLine + " cross apply dbo.CSVToStringTable(IIF(t2.EventTypes_pKey ='' OR t2.EventTypes_pKey IS NULL,'1,4,5,3,2',t2.EventTypes_pKey),',')x ");
            qry.Append(System.Environment.NewLine + " Where t3.AccountParticipationType_pkey in (" + AccountType + ")");
            qry.Append(System.Environment.NewLine + " and isNull(t2.MobileOnly, 0) = 0 and isNull(t2.IsActive,0) = 1 and isNull(t2.Event_pkey," + CurrentEventPkey.ToString() + ") = " + CurrentEventPkey.ToString());
            qry.Append(System.Environment.NewLine + " and (x.String IN (SELECT value From string_split((Select Case When EventType_pKey IN (3,5) Then '3,5' When  EventType_pKey IN (1,2,4) Then '1,2,4' ELSE '1,4,5,3,2' END from Event_List where pKey=" + CurrentEventPkey.ToString() + "),',')) OR (TRIM(t2.EventTypes_pKey)='' OR t2.EventTypes_pKey IS NULL))");
            qry.Append(System.Environment.NewLine + " and (isNull(t2.DuringConferenceOnly,0) = 0 or (getdate() between " + strEventStart + " and " + strEventEnd + "))");
            qry.Append(System.Environment.NewLine + " Order by t1.SortOrder, t1.FAQCategoryID");


            SqlParameter[] parameters = new SqlParameter[] { };
            string s = qry.ToString();
            dt = SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
            // SqlCommand cmd = new SqlCommand(qry.ToString());
            // clsUtility.GetDataTable(con, cmd, ref dt);


            return dt;
        }
        public DataTable FAQsQuestionAnswer(int CurrentEventPkey, int intFAQCategory_pkey, string AccountType, string strEventStart, string strEventEnd, string txtSearch)
        {
            DataTable dt = new DataTable();

            string categoryFilter = "";
            StringBuilder qry = new StringBuilder("Select distinct t1.pKey As FAQ_pKey, t1.FAQa, t1.FAQLink, (Case When isNull(t1.FAQLink,'') <> '' then 1 else 0 end) as hasLink, t1.FAQCategory_pkey, t1.SortOrder , t1.FAQq as Question  From FAQ_List t1");
            qry.Append(System.Environment.NewLine + " Inner Join FAQ_ParticipationTypes t3 on t3.FAQ_pKey = t1.pKey");
            qry.Append(System.Environment.NewLine + " cross apply dbo.CSVToStringTable(IIF(TRIM(t1.EventTypes_pKey) ='' OR t1.EventTypes_pKey IS NULL,'1,4,5,3,2',t1.EventTypes_pKey),',')x");
            qry.Append(System.Environment.NewLine + " inner join Event_List EL on EL.EventStatus_pKey = x.String");
            if (intFAQCategory_pkey != 0)
            {
                //qry.Append(System.Environment.NewLine + " Where t1.FAQCategory_pKey = " + intFAQCategory_pkey.ToString());
                qry.Append(System.Environment.NewLine + " Where " + intFAQCategory_pkey.ToString() + " IN (Select value From string_split(t1.FAQCategory_pkey,','))");
                qry.Append(System.Environment.NewLine + " and t3.AccountParticipationType_pkey in (" + AccountType + ")");
            }
            else
            {
                qry.Append(System.Environment.NewLine + " Where t3.AccountParticipationType_pkey in (" + AccountType + ")");
            }

            qry.Append(System.Environment.NewLine + " and isNull(t1.Event_pkey," + CurrentEventPkey.ToString() + ") = " + CurrentEventPkey.ToString());
            qry.Append(System.Environment.NewLine + " and (x.String IN (SELECT value From string_split((Select Case When EventType_pKey IN (3,5) Then '3,5' When  EventType_pKey IN (1,2,4) Then '1,2,4' ELSE '1,4,5,3,2' END from Event_List where pKey=" + CurrentEventPkey.ToString() + "),',')) OR (TRIM(t1.EventTypes_pKey)='' OR t1.EventTypes_pKey IS NULL))");
            qry.Append(System.Environment.NewLine + " and isNull(t1.IsActive,0) = 1");
            qry.Append(System.Environment.NewLine + " and isNull(t1.MobileOnly,0) = 0"); //" and isNull(t1.MobileOnly,0) = " + IIf(Me.ckM.Checked, "1", "0")
            if (!string.IsNullOrEmpty(txtSearch.Trim()))
            {
                qry.Append(System.Environment.NewLine + " and (t1.FAQq like @Search OR t1.FAQa like @Search )");
            }
            qry.Append(System.Environment.NewLine + " and (isNull(t1.DuringConferenceOnly,0) = 0 or (getdate() between " + strEventStart + " and " + strEventEnd + "))");
            qry.Append(System.Environment.NewLine + " Order by t1.SortOrder, Question");

            if (!string.IsNullOrEmpty(txtSearch.Trim()))
            {
                SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@Search", "%" + txtSearch.Trim() + "%"),
                };
                dt = SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
            }
            else
            {
                SqlParameter[] parameters = new SqlParameter[] { };
                dt = SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
            }

            return dt;
        }


        public string SaveFAQFeedback(string Status, string Suggestion, int intFAQPKey, int? intAccount_PKey)
        {
            string qry = "Insert into FAQ_Feedbacks(Status,Suggestion,Account_Id,SuggestionDate,Feedback_pKey)";
            qry = qry + System.Environment.NewLine + "Values(@Status,@Suggestion,@AccountId,GetDate(),@FeedbackpKey)";
            SqlCommand cmd = new SqlCommand(qry);
            cmd.CommandType = CommandType.Text;
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("@Status", Status);
            cmd.Parameters.AddWithValue("@Suggestion", Suggestion);
            cmd.Parameters.AddWithValue("@FeedbackpKey", intFAQPKey);
            if (intAccount_PKey != null)
                cmd.Parameters.AddWithValue("@AccountId", intAccount_PKey);
            else
                cmd.Parameters.AddWithValue("@AccountId", DBNull.Value);

            if (!clsUtility.ExecuteQuery(cmd, null, "FAQ"))
            {
                return "Issue in submitting";
            }
            else
            {
                return "Thank you for the suggestion";
            }


        }
        #endregion

        #region MyGroupChat

        public DataSet GroupChat(int intAccount_PKey, int intActiveEventPkey)
        {
            SqlCommand cmd = new SqlCommand("SP_ShowGroupChat", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@paramEventKey", intActiveEventPkey);
            cmd.Parameters.AddWithValue("@accountkey", intAccount_PKey);
            con.Open();
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            sda.Fill(ds);
            con.Close();


            return ds;
        }
        #endregion

        public DataTable EventFAQCategoriesWithQuestion(int CurrentEventPkey, bool IsAttendee, bool bSponsor, bool bSpeaker, bool StaffMember, string AccountType, string strEventStart, string strEventEnd, int AccountPkey, int ckMChecked, bool CKCChecked)
        {

            DataTable dt = new DataTable();

            StringBuilder qry = new StringBuilder();
            string categoryFilter = "";
            categoryFilter = ((IsAttendee) ? ",1" : "") + ((bSponsor) ? ",7" : "") + ((bSpeaker) ? ",2,10" : "") + ((StaffMember) ? ",3" : "");
            qry.Append(" DECLARE @Online bit =0; ");
            if (AccountPkey > 0)
                qry.Append(" SET @Online = IIF(Exists(SELECT 1 From sys_onlinePeople where id= " + AccountPkey.ToString() + " And Cast(onlinedate as date) = Cast(CURRENT_TIMESTAMP as date)),1,0)");
            qry.Append("Select distinct t1.pKey as FAQCategory_pKey, t1.FAQCategoryID, t1.SortOrder,  t2.FAQq as Question ,t2.pKey As FAQ_pKey, t2.FAQa, t2.FAQLink, (Case When isNull(t2.FAQLink,'') <> '' then 1 else 0 end) as hasLink, t2.FAQCategory_pkey From SYS_FAQCategories t1 ");
            qry.Append(System.Environment.NewLine + " Inner Join FAQ_List t2 on (t1.pKey IN (SELECT value from string_split(t2.FAQCategory_pkey,',')))");
            qry.Append(System.Environment.NewLine + " Inner Join FAQ_ParticipationTypes t3 on t3.FAQ_pKey = t2.pKey");
            qry.Append(System.Environment.NewLine + " cross apply dbo.CSVToStringTable(IIF(t2.EventTypes_pKey ='' OR t2.EventTypes_pKey IS NULL,'1,4,5,3,2',t2.EventTypes_pKey),',')x ");
            qry.Append(System.Environment.NewLine + " Where t3.AccountParticipationType_pkey in (" + AccountType + ")");
            qry.Append(System.Environment.NewLine + " and isNull(t2.MobileOnly, 0) = " + ckMChecked + " and isNull(t2.IsActive,0) = 1 and isNull(t2.Event_pkey," + CurrentEventPkey.ToString() + ") = " + CurrentEventPkey.ToString());
            qry.Append(System.Environment.NewLine + " and (x.String IN (SELECT value From string_split((Select Case When EventType_pKey IN (3,5) Then '3,5' When  EventType_pKey IN (1,2,4) Then '1,2,4' ELSE '1,4,5,3,2' END from Event_List where pKey=" + CurrentEventPkey.ToString() + "),',')) OR (TRIM(t2.EventTypes_pKey)='' OR t2.EventTypes_pKey IS NULL))");
            qry.Append(System.Environment.NewLine + " and ((isNull(t2.OnlineOnly,0) = 1 AND @Online =1) OR ISNULL(t2.OnlineOnly,0) =0)  ");
            if (StaffMember)
            {
                if (!CKCChecked)
                    qry.Append(System.Environment.NewLine + " and (isNull(t2.DuringConferenceOnly,0) = 0 or (getdate() between " + strEventStart + " and " + strEventEnd + "))");
            }
            else
                qry.Append(System.Environment.NewLine + " and (isNull(t2.DuringConferenceOnly,0) = 0 or (getdate() between " + strEventStart + " and " + strEventEnd + "))");

            qry.Append(System.Environment.NewLine + " Order by t1.SortOrder, t1.FAQCategoryID");

            SqlParameter[] parameters = new SqlParameter[] { };
            dt = SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);

            string s = qry.ToString();
            return dt;
        }

        public DataTable EventFAQCategories(int CurrentEventPkey, bool IsAttendee, bool bSponsor, bool bSpeaker, bool StaffMember, string AccountType, string strEventStart, string strEventEnd, int AccountPkey, int ckMChecked, bool CKCChecked)
        {
            DataTable dt = new DataTable();

            StringBuilder qry = new StringBuilder();
            string categoryFilter = "";
            categoryFilter = ((IsAttendee) ? ",1" : "") + ((bSponsor) ? ",7" : "") + ((bSpeaker) ? ",2,10" : "") + ((StaffMember) ? ",3" : "");
            qry.Append(" DECLARE @Online bit =0; ");
            if (AccountPkey > 0)
                qry.Append(" SET @Online = IIF(Exists(SELECT 1 From sys_onlinePeople where id= " + AccountPkey.ToString() + " And Cast(onlinedate as date) = Cast(CURRENT_TIMESTAMP as date)),1,0)");
            qry.Append("Select distinct t1.pKey as FAQCategory_pKey, t1.FAQCategoryID, t1.SortOrder From SYS_FAQCategories t1 ");
            qry.Append(System.Environment.NewLine + " Inner Join FAQ_List t2 on (t1.pKey IN (SELECT value from string_split(t2.FAQCategory_pkey,',')))");
            qry.Append(System.Environment.NewLine + " Inner Join FAQ_ParticipationTypes t3 on t3.FAQ_pKey = t2.pKey");
            qry.Append(System.Environment.NewLine + " cross apply dbo.CSVToStringTable(IIF(t2.EventTypes_pKey ='' OR t2.EventTypes_pKey IS NULL,'1,4,5,3,2',t2.EventTypes_pKey),',')x ");
            qry.Append(System.Environment.NewLine + " Where t3.AccountParticipationType_pkey in (" + AccountType + ")");
            qry.Append(System.Environment.NewLine + " and isNull(t2.MobileOnly, 0) = " + ckMChecked + " and isNull(t2.IsActive,0) = 1 and isNull(t2.Event_pkey," + CurrentEventPkey.ToString() + ") = " + CurrentEventPkey.ToString());
            qry.Append(System.Environment.NewLine + " and (x.String IN (SELECT value From string_split((Select Case When EventType_pKey IN (3,5) Then '3,5' When  EventType_pKey IN (1,2,4) Then '1,2,4' ELSE '1,4,5,3,2' END from Event_List where pKey=" + CurrentEventPkey.ToString() + "),',')) OR (TRIM(t2.EventTypes_pKey)='' OR t2.EventTypes_pKey IS NULL))");
            qry.Append(System.Environment.NewLine + " and ((isNull(t2.OnlineOnly,0) = 1 AND @Online =1) OR ISNULL(t2.OnlineOnly,0) =0)  ");
            if (StaffMember)
            {
                if (!CKCChecked)
                    qry.Append(System.Environment.NewLine + " and (isNull(t2.DuringConferenceOnly,0) = 0 or (getdate() between " + strEventStart + " and " + strEventEnd + "))");
            }
            else
                qry.Append(System.Environment.NewLine + " and (isNull(t2.DuringConferenceOnly,0) = 0 or (getdate() between " + strEventStart + " and " + strEventEnd + "))");

            qry.Append(System.Environment.NewLine + " Order by t1.SortOrder, t1.FAQCategoryID");

            SqlParameter[] parameters = new SqlParameter[] { };
            dt = SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);

            string s = qry.ToString();
            return dt;
        }


        public DataTable EventFAQsQuestionAnswer(int CurrentEventPkey, int intFAQCategory_pkey, string AccountType, string strEventStart, string strEventEnd, string txtSearch, int AccountPkey, bool bStaffuser, int CKM, int CKC)
        {
            DataTable dt = new DataTable();

            string categoryFilter = "";
            StringBuilder qry = new StringBuilder();
            qry.Append(" DECLARE @Online bit =0; ");
            if (AccountPkey > 0)
                qry.Append(" SET @Online = IIF(Exists(SELECT 1 From sys_onlinePeople where id= " + AccountPkey.ToString() + " And Cast(onlinedate as date) = Cast(CURRENT_TIMESTAMP as date)),1,0)");

            qry.Append(System.Environment.NewLine + "Select distinct t1.pKey As FAQ_pKey, t1.FAQa, t1.FAQLink, (Case When isNull(t1.FAQLink,'') <> '' then 1 else 0 end) as hasLink, t1.FAQCategory_pkey, t1.SortOrder , t1.FAQq as Question  From FAQ_List t1");
            qry.Append(System.Environment.NewLine + " Inner Join FAQ_ParticipationTypes t3 on t3.FAQ_pKey = t1.pKey");
            qry.Append(System.Environment.NewLine + " cross apply dbo.CSVToStringTable(IIF(TRIM(t1.EventTypes_pKey) ='' OR t1.EventTypes_pKey IS NULL,'1,4,5,3,2',t1.EventTypes_pKey),',')x");
            qry.Append(System.Environment.NewLine + " inner join Event_List EL on EL.EventStatus_pKey = x.String");
            if (intFAQCategory_pkey != 0)
            {
                qry.Append(System.Environment.NewLine + " Where " + intFAQCategory_pkey.ToString() + " IN (Select value From string_split(t1.FAQCategory_pkey,','))");
                qry.Append(System.Environment.NewLine + " and t3.AccountParticipationType_pkey in (" + AccountType + ")");
            }
            else
                qry.Append(System.Environment.NewLine + " Where t3.AccountParticipationType_pkey in (" + AccountType + ")");

            qry.Append(System.Environment.NewLine + " and isNull(t1.Event_pkey," + CurrentEventPkey.ToString() + ") = " + CurrentEventPkey.ToString());
            qry.Append(System.Environment.NewLine + " and (x.String IN (SELECT value From string_split((Select Case When EventType_pKey IN (3,5) Then '3,5' When  EventType_pKey IN (1,2,4) Then '1,2,4' ELSE '1,4,5,3,2' END from Event_List where pKey=" + CurrentEventPkey.ToString() + "),',')) OR (TRIM(t1.EventTypes_pKey)='' OR t1.EventTypes_pKey IS NULL))");
            qry.Append(System.Environment.NewLine + " and isNull(t1.IsActive,0) = 1");
            qry.Append(System.Environment.NewLine + " and isNull(t1.MobileOnly,0) = " + CKM);
            if (!string.IsNullOrEmpty(txtSearch.Trim()))
                qry.Append(System.Environment.NewLine + " and (t1.FAQq like @Search OR t1.FAQa like @Search )");

            if (bStaffuser)
            {
                if (CKC == 0)
                    qry.Append(System.Environment.NewLine + " and (isNull(t1.DuringConferenceOnly,0) = 0 or (getdate() between " + strEventStart + " and " + strEventEnd + "))");
            }
            else
                qry.Append(System.Environment.NewLine + " and (isNull(t1.DuringConferenceOnly,0) = 0 or (getdate() between " + strEventStart + " and " + strEventEnd + "))");

            qry.Append(System.Environment.NewLine + " Order by t1.SortOrder, Question");

            if (!string.IsNullOrEmpty(txtSearch.Trim()))
            {
                SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@Search", "%" + txtSearch.Trim() + "%"),
                };
                dt = SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
            }
            else
            {
                SqlParameter[] parameters = new SqlParameter[] { };
                dt = SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
            }

            return dt;
        }


        public DataTable GetSynopsisWithSlidesData(int intEventSession_pKey)
        {
            DataTable dt = new DataTable();
            StringBuilder sb = new StringBuilder();
            sb.Append(" SELECT ESD.pKey as DocpKey,ESD.DocFileName,ESD.DocDisplayName as DocName,SL.SessionID From Event_Sessions ES INNER JOIN Session_List SL ON SL.pKey = ES.Session_pKey INNER JOIN  EventSession_Documents  ESD ON ESD.EventSession_pKey= ES.pKey ");
            sb.AppendLine(" Where ESD.SessionDocType_pKey=1 AND ES.pKey =  " + intEventSession_pKey.ToString());
            SqlCommand cmd = new SqlCommand(sb.ToString());

            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }


            return dt;

        }


        public DataTable GetPaymentMethod(int intAccount_PKey, int intCurEventPKey)
        {
            DataTable dt = new DataTable();
            try
            {
                StringBuilder sb = new StringBuilder("select top 1 t1.PaymentMethod_pkey as PaymentMethod, t1.ReceiptNumber, isnull(t1.PaymentDate,'') as PaymentDate,t1.Amount,t1.Paid");
                sb.AppendLine(",isnull( [dbo].[getGroupCode](convert(varchar,isnull(t1.IntendedAccounts,0))),'') as GroupCode, t2.PaymentsApplied, t2.Balance,t2.tAmount");
                sb.AppendLine(" from Account_Payments t1  ");
                sb.AppendLine(" cross apply dbo.csvtonumbertable(t1.intendedaccounts,',') x ");
                sb.AppendLine(" CROSS JOIN  getAccountBalance(" + (intAccount_PKey > 0 ? intAccount_PKey : 0).ToString() + "," + intCurEventPKey.ToString() + ") t2");
                sb.AppendLine(" Inner JOIN getPaymentDateV2(" + (intAccount_PKey > 0 ? intAccount_PKey : 0).ToString() + "," + intCurEventPKey.ToString() + ") t3 on t3.reciptNo = t1.ReceiptNumber");
                sb.AppendLine(" where isnull(t1.IsDelete,0)=0 and x.num =" + intAccount_PKey.ToString() + " and t1.Event_pKey= " + intCurEventPKey.ToString());
                sb.AppendLine(" and (t1.RefundAmount<>t1.Amount) AND t1.ReceiptNumber not in (select ReceiptReference from Account_Charges  where ReceiptReference=t1.ReceiptNumber and (isNull(Reversed,0)=1 or isNull(ReversalReference,0)>0 or ISNULL(IsDelete,0)=1))");

                SqlCommand cmd = new SqlCommand(sb.ToString());
                clsUtility.GetDataTable(con, cmd, ref dt);
            }
            catch (Exception ex)
            {
                dt = null;
            }
            return dt;
        }

        public bool ISMAinPayer(int intAccount_PKey, int intCurEventPKey)
        {

            bool bMainPayer = false;
            try
            {
                string qry1 = "select * from Account_Payments where (RefundAmount<>Amount) AND Account_pKey =  " + ((intAccount_PKey > 0) ? intAccount_PKey : 0).ToString() + " and Event_pKey = " + intCurEventPKey.ToString();
                SqlCommand cmd1 = new SqlCommand(qry1);
                DataTable dt1 = new DataTable();
                if (clsUtility.GetDataTable(con, cmd1, ref dt1))
                {
                    if (dt1 != null && dt1.Rows.Count > 0)
                        bMainPayer = true;
                }
            }
            catch (Exception Ex)
            {

            }
            return bMainPayer;
        }

        public bool CheckPaidOrNot(string intReceiptNumber)
        {
            bool CheckPaidOrNot = true;
            string qry = "select ReceiptReference from account_charges where ReceiptReference=" + intReceiptNumber + " and Reversed=1";
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                if (dt.Rows.Count > 0)
                {
                    CheckPaidOrNot = false;
                }
                else
                {
                    CheckPaidOrNot = true;
                }
            }

            return CheckPaidOrNot;
        }

        public DataTable EventHandbook(int intEventPKey, string strFileGUID, int UsageType_CloudEventHandbook)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" Select top 1 pKey,FileName from Event_Guidelines");
            sb.AppendLine(" where Event_pKey =" + intEventPKey.ToString());
            sb.AppendLine(" And FileGUID ='" + strFileGUID + "'");
            sb.AppendLine(" And UsageType = " + UsageType_CloudEventHandbook.ToString());
            SqlCommand cmd = new SqlCommand(sb.ToString());
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }


        public DataTable SpeakerHandBook(int UsageType_SpeakerGuideline, int intEventPKey, string strFileGUID)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" Select top 1 pKey,FileName from Event_Guidelines  ");
            sb.AppendLine("where UsageType = " + UsageType_SpeakerGuideline.ToString());
            sb.AppendLine(" And Event_pKey=" + intEventPKey.ToString());
            sb.AppendLine(" And FileGUID='" + strFileGUID + "'");
            sb.AppendLine(" order by UploadedOn desc");

            SqlCommand cmd = new SqlCommand(sb.ToString());
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }


        public DataTable RefreshSpeakerPanel(bool bShowSpeakerPass, DateTime dtCurrentTime, int intAccount_PKey, int intCurEventPKey)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("select t1.Track_Prefix+ t3.SessionID AS SessionID , t1.pkey as EvtSession_pKey," + (bShowSpeakerPass ? 1 : 0).ToString() + " as bShowSpeakerPass");
            sb.AppendLine(",('You are a ' + LOWER((Case when x.Roles <> '' Then x.Roles else 'Unspecified' End))) as SessionStatus");
            sb.AppendLine(",(' - Discount code for free pass to your session: ' +t4.DiscountID) as DiscountCode");
            sb.AppendLine(",IIF((Select count(*) from ActivityFeedbackForm_Questions aq cross apply dbo.CSVToNumberTable(aq.EventSession_pkeys,',') x inner join Event_Sessions es on x.num = es.pkey where es.StartTime < '" + dtCurrentTime.ToString() + "' and es.EndTime > '" + dtCurrentTime.ToString() + "' and es.pkey = t1.pkey) >0 ,1,0) ActivityStart");
            sb.AppendLine(",IIF((Select count(*) From ActivityFeedbackForm_Questions aq1 cross apply dbo.CSVToNumberTable(aq1.EventSession_pkeys,',') x where x.num = t1.pkey and Isnull(aq1.IsStarted,0) = 1)>0,'Stop Attendee','Allow Attendee') QuestionStatus");
            sb.AppendLine(" From event_sessions t1");
            sb.AppendLine(" inner join eventsession_staff t2 On t2.eventsession_pKey = t1.pKey");
            sb.AppendLine(" inner join session_List t3 On t3.pKey = t1.Session_pKey");
            sb.AppendLine(" Cross Apply dbo.getSessionRoleCross(t2.Account_pKey, t1.pKey,',',2) x");
            sb.AppendLine(" left join Discount_List t4 On t4.CreatedFor_Pkey = t2.Account_pKey and t4.Session_pkey=convert(varchar,t1.Session_pKey) And Isnull(t4.Active,0)=1");
            sb.AppendLine(" Where t2.Account_pKey = " + /*"30817"*/ intAccount_PKey.ToString());
            sb.AppendLine(" and t1.event_pkey = " + intCurEventPKey.ToString());
            sb.AppendLine(" and isNull(t2.IsSessionChair,0) <> 1");
            sb.AppendLine(" Order by SessionID ");

            SqlCommand cmd = new SqlCommand(sb.ToString());
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public DataTable RefreshChairPanel(DateTime dtCurrentTime, int intAccount_PKey, int intCurEventPKey)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select (t1.Track_Prefix+t3.SessionID) as SessionID, t1.pkey as EvtSession_pKey");
            sb.AppendLine(",('You are the ' + LOWER((Case when x.Roles <> '' Then x.Roles else 'Unspecified' End))) as SessionStatus");
            sb.AppendLine(",IIF((Select count(*) from ActivityFeedbackForm_Questions aq cross apply dbo.CSVToNumberTable(aq.EventSession_pkeys,',') x inner join Event_Sessions es on x.num = es.pkey where es.StartTime < '" + dtCurrentTime.ToString() + "' and es.EndTime > '" + dtCurrentTime.ToString() + "' and es.pkey = t1.pkey) >0 ,1,0) ActivityStart");
            sb.AppendLine(",IIF((Select count(*) From ActivityFeedbackForm_Questions aq1 cross apply dbo.CSVToNumberTable(aq1.EventSession_pkeys,',') x where x.num = t1.pkey and Isnull(aq1.IsStarted,0) = 1)>0,'Stop Attendee','Allow Attendee') QuestionStatus");
            sb.AppendLine(" From event_sessions t1");
            sb.AppendLine(" inner join eventsession_staff t2 On t2.eventsession_pKey = t1.pKey");
            sb.AppendLine(" inner join session_List t3 On t3.pKey = t1.Session_pKey");
            sb.AppendLine(" Cross Apply dbo.getSessionRoleCross(t2.Account_pKey, t1.pKey,',',1) x");
            sb.AppendLine(" Where t2.Account_pKey = " + /*"30817"*/  intAccount_PKey.ToString());
            sb.AppendLine(" and t1.event_pkey = " + intCurEventPKey.ToString());
            sb.AppendLine(" and t2.IsSessionChair = 1");
            sb.AppendLine(" Order by SessionID");
            sb.AppendLine();

            SqlCommand cmd = new SqlCommand(sb.ToString());
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public EvtSummaryPartner PartnerAlias(int EventPkey, int AccountPkey)
        {
            string lblMainTitle = "";
            string lbltext = "";
            string lblPartnerTitle = "Exhibitor Information";
            string btnPartnerMInstruct = "Handbook";
            string btnPtExh = "";
            string btnPTypeGLines = "Information for exhibitors";
            bool btnInstructPartnerVisible = false;
            bool lblPartnerTitleVisible = false;

            string qry = "Select PartnerAlias from Event_List where  pKey = " + EventPkey.ToString();

            SqlCommand myCommand = new SqlCommand(qry, con);
            con.Open();
            //SqlDataReader reader = myCommand.ExecuteReader();

            using (SqlDataReader reader = myCommand.ExecuteReader())
            {

                while (reader.Read())
                {
                    lblMainTitle = reader["PartnerAlias"].ToString();
                    if (lblMainTitle == "Exhibitor")
                        lbltext = "Your organization is an exhibitor at this event.";
                    else if (lblMainTitle == "Event Sponsor")
                        lbltext = "Your organization is an event sponsor at this event.";
                    else if (lblMainTitle == "Partner")
                        lbltext = "Your organization is a partner at this event.";
                    else if (lblMainTitle == "Solution Provider")
                        lbltext = "Your organization is a solution provider at this event.";
                }
            }
            con.Close();


            StringBuilder sb = new StringBuilder();
            sb.Append("select isNull(t1.ParticipationType_pkey,0) as pKey , t3.ParticipationTypeID from Event_Organizations t1 ");
            sb.AppendLine("left outer join Organization_list t2 on t2.pKey=t1.Organization_pKey");
            sb.AppendLine("Left outer join SYS_ParticipationTypes t3 on t3.pKey=t1.ParticipationType_pkey");
            sb.AppendLine("left outer join Account_List t4 on t4.ParentOrganization_pKey=t1.Organization_pKey and t4.ParentOrganization_pKey=t2.pKey");
            sb.AppendLine("where t4.pKey=" + AccountPkey.ToString());
            sb.AppendLine(" And t1.Event_pKey=" + EventPkey.ToString());
            sb.AppendLine();

            SqlCommand cmd = new SqlCommand(sb.ToString());
            DataTable dt = new DataTable();
            int intParticipationType = 0;
            int hdnIsExibitor = 0;
            int hdnParticipationType = 0;
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                if (dt.Rows.Count > 0)
                {
                    intParticipationType = Convert.ToInt32(dt.Rows[0]["pKey"].ToString());
                    hdnParticipationType = intParticipationType;

                    if (intParticipationType == 0)
                    {
                        lblPartnerTitleVisible = intParticipationType > 0;
                        btnInstructPartnerVisible = (intParticipationType > 0);
                    }
                    else if (intParticipationType == 1)
                    {
                        lblPartnerTitle = "Exhibitor Information";
                        btnPartnerMInstruct = "Exhibitor Guideline";
                        btnPtExh = "Exhibitor";
                        btnPTypeGLines = "Exhibitor Guidelines";
                        lblMainTitle = "Exhibiting";

                    }
                    else if (intParticipationType == 2)
                    {
                        lblPartnerTitle = "Other Partner Information";
                        btnPartnerMInstruct = "Other Guideline";
                        btnPtExh = "Other";
                        btnPTypeGLines = "Other Partner Guidelines";
                    }
                    else if (intParticipationType == 3)
                    {
                        lblPartnerTitle = "Media Partner Information";
                        btnPartnerMInstruct = "Media Guideline";
                        btnPtExh = "Media";
                        lblMainTitle = "Media Partnership";
                        btnPTypeGLines = "Media Partner Guidelines";
                    }
                    else if (intParticipationType == 4)
                    {
                        lblPartnerTitle = "Branding Partner Information";
                        btnPartnerMInstruct = "Branding Guideline";
                        btnPtExh = "Branding";
                        lblMainTitle = "Branding Partnership";
                        btnPTypeGLines = "Branding Partner Guidelines";

                    }
                    else if (intParticipationType == 5)
                    {
                        lblPartnerTitle = "Event Sponsor Information";
                        btnPartnerMInstruct = "Event Sponsor Handbook";
                        btnPtExh = "My Event Sponsor Console";
                        lblMainTitle = "Event Sponsorship";
                        btnPTypeGLines = "Event Sponsor Handbook ";

                    }

                }
                lblPartnerTitleVisible = intParticipationType > 0;
                btnInstructPartnerVisible = (intParticipationType > 0);

            }

            EvtSummaryPartner pt = new EvtSummaryPartner()
            {
                lblMainTitle = lblMainTitle,
                lbltext = lbltext,
                lblPartnerTitle = lblPartnerTitle,
                btnPartnerMInstruct = btnPartnerMInstruct,
                btnPtExh = btnPtExh,
                btnPTypeGLines = btnPTypeGLines,
                btnInstructPartnerVisible = btnInstructPartnerVisible,
                lblPartnerTitleVisible = lblPartnerTitleVisible,
                hdnParticipationType = hdnParticipationType
            };

            return pt;
        }

        public DataTable PartnerMInstruct(int EventPkey, string strFileGUID, int UsageType, int intParticipationType)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" Select top 1 pKey,FileName from Event_Guidelines");
            sb.AppendLine(" where Event_pKey =" + EventPkey.ToString());
            sb.AppendLine(" And FileGUID ='" + strFileGUID + "'");
            if (intParticipationType != 0)
            {
                sb.AppendLine(" And UsageType=" + UsageType);
            }
            sb.AppendLine(" order by UploadedOn desc");

            SqlCommand cmd = new SqlCommand(sb.ToString());
            DataTable dt = new DataTable();

            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;



        }


        public DataTable ViewPdfFile(string type, int intEventPKey, string strFileGUID, string UsagetypeGuidline)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" Select top 1 pKey,FileName from Event_Guidelines");
            sb.AppendLine(" where UsageType=" + UsagetypeGuidline);

            sb.AppendLine(" And Event_pKey=" + intEventPKey.ToString());
            sb.AppendLine(" And FileGUID='" + strFileGUID + "'");
            sb.AppendLine(" order by UploadedOn desc");
            SqlCommand cmd = new SqlCommand(sb.ToString());
            DataTable dt = new DataTable();

            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;

        }
        public DataTable ViewPDFFileElsePart(string UsagetypeGuidline)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" Select top 1 pKey,FileName from Event_Guidelines");
            sb.AppendLine(" where UsageType=" + UsagetypeGuidline);

            sb.AppendLine(" and Event_pkey = 0 Or Event_pkey is null");
            sb.AppendLine("and Status = 1");
            sb.AppendLine(" order by UploadedOn desc");
            SqlCommand cmd = new SqlCommand(sb.ToString());
            DataTable dt = new DataTable();

            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public DataTable ViewPartnerTypePDF(int intEventPKey, string UsageType_)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" Select top 1 pKey,FileName from Event_Guidelines");
            sb.AppendLine(" where Event_pKey =" + intEventPKey.ToString());
            sb.AppendLine(" And  1=1");
            sb.AppendLine("And UsageType = " + UsageType_);
            sb.AppendLine("  order by UploadedOn desc");
            SqlCommand cmd = new SqlCommand(sb.ToString());
            DataTable dt = new DataTable();

            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public DataTable SpecialEventPanel(int intAccount_PKey, int intCurEventPKey)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select (t2.Track_Prefix+t1.sessionid) as SessionID,t2.Title as Sessiontitle, Format(t2.Starttime, 'ddd, MMM dd') as mainDate");
            sb.AppendLine(",Format(t2.Starttime, 'hh:mm tt')  mainTime, isNull(t4.RoomName,'Not available') as Location");
            sb.AppendLine(" from session_list t1");
            sb.AppendLine(" inner join event_sessions t2 on t2.session_pkey = t1.pkey");
            sb.AppendLine(" inner join eventsession_accounts t3 on t3.eventsession_pkey = t2.pkey");
            sb.AppendLine(" left outer join venue_rooms t4 on t4.pkey = t2.venueroom_pkey");
            sb.AppendLine(" Where t3.account_pkey =" + intAccount_PKey.ToString());
            sb.AppendLine(" and isnull(t2.IsHideFromPublic,0) = 1");
            sb.AppendLine(" and isnull(t2.IsShowOnMyConferencePage,0) = 1");
            sb.AppendLine(" and isnull(t2.event_pkey,0) = " + intCurEventPKey.ToString());  // "54" );//
            SqlCommand cmd = new SqlCommand(sb.ToString());
            DataTable dt = new DataTable();

            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;

        }

        public bool CheckSpecialEventParticipant(int intAccount_PKey, int intCurEventPKey)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select count(t1.sessionid)");
            sb.AppendLine("from session_list t1");
            sb.AppendLine(" inner join event_sessions t2 on t2.session_pkey = t1.pkey");
            sb.AppendLine("inner join eventsession_accounts t3 on t3.eventsession_pkey = t2.pkey");
            sb.AppendLine("where t3.account_pkey =" + intAccount_PKey.ToString());
            sb.AppendLine("and t2.IsHideFromPublic = 1");
            sb.AppendLine("and t2.event_pkey = " + intCurEventPKey.ToString());
            SqlCommand cmd = new SqlCommand(sb.ToString());
            bool result = false;
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                if (dt.Rows.Count > 0 && dt.Rows[0][0].ToString() != "0")
                {
                    result = true;
                }
            }
            return result;

        }

        public bool updateAllowAttendee(int intStarted, int intEventSession_pkey)
        {
            bool result = true;
            string qry = "Update t1 Set t1.IsStarted = " + intStarted.ToString() + " From ActivityFeedbackForm_Questions t1 cross apply dbo.CSVToNumberTable(t1.EventSession_pkeys,',') x where x.num = " + intEventSession_pkey.ToString() + " and t1.IsApproved = 1";
            SqlCommand cmd = new SqlCommand(qry);
            if (!clsUtility.ExecuteQuery(cmd, null, "Update Attendee Status"))
            {
                result = false;
            }
            return result;
        }
        public DataTable QuesResponse(int intEventSession_pKey)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Select t1.pkey,t1.Question,x.num EventSession_pkey");
            sb.AppendLine(" From ActivityFeedbackForm_Questions t1");
            sb.AppendLine("cross apply dbo.CSVtoNumberTable(t1.EventSession_pkeys,',')x");
            sb.AppendLine("where x.num = " + intEventSession_pKey.ToString());
            SqlCommand cmd = new SqlCommand(sb.ToString());
            DataTable dt = new DataTable();

            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;

        }

        public DataTable GetChairingSessionInfo(int intActiveEventPkey, int intWorkAccountPKey)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" With Qry  As (SELECT * From EventSession_Staff Where ISNULL(IsSessionChair,0) = 1)");
            sb.AppendLine(" SELECT DISTINCT t1.pKey,t2.Track_Prefix + t1.SessionID AS SessionID,t1.SessionTitle,t2.Track_Prefix,ROW_NUMBER() OVER  (ORDER BY SessionID desc)  as Row_Count   From  Session_List t1");
            sb.AppendLine(" INNER JOIN event_sessions t2 on t2.Session_pkey = t1.pkey INNER JOIN  Qry t3 on t3.EventSession_pKey = t2.pkey");
            sb.AppendLine(" Where t2.Event_pKey =" + intActiveEventPkey.ToString() + " AND t3.Account_pKey =" + intWorkAccountPKey.ToString() + " and ISNULL(t2.IsScheduled,0)=1 AND isNull(t1.NumChairs,1) > 0");
            sb.AppendLine(" Order by SessionID");
            SqlCommand cmd = new SqlCommand(sb.ToString());
            DataTable dt = new DataTable();

            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public DataTable GetOtherLinks(string RoleKey, int intWorkAccountPKey)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Select Top 9 pKey, RolepKey, DisplayText, case when  LinkToPage='SpeakerRegistrationFeedback.aspx' then LinkToPage + '?APK='  + cast(" + intWorkAccountPKey.ToString() + " as varchar) else LinkToPage end as LinkToPage");
            sb.AppendLine(" From AccessPageLink t1");
            sb.AppendLine(" Where t1.RolepKey in(" + RoleKey + ")");
            sb.AppendLine(" Order by t1.pKey Desc");
            sb.AppendLine();
            SqlCommand cmd = new SqlCommand(sb.ToString());
            DataTable dt = new DataTable();

            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public DataTable GetCRCPOption(bool bSpeakerAtEvent, int intCurEventPKey)
        {
            StringBuilder sb = new StringBuilder();

            if (bSpeakerAtEvent)
            {
                sb.Append(" Select     t1.ChargeTypeID +' ('+FORMAT( ");
                sb.AppendLine(" CASE WHEN t1.pkey=2 then (Select SettingValue FROM Application_Settings Where pkey=120)");
                sb.AppendLine(" WHEN t1.pkey=43 then t2.Amount");
                sb.AppendLine(" ELSE 0 END,");
                sb.AppendLine(" 'C')+')' AS 	ChargeTypeID ,ISNULL(t1.ChargeTypeID,'') as ChargeTypeText,t1.TypeOfCharge,	t1.UserEditable	,t1.pKey	,t1.ShowOnRegistrationPage,	t1.ShowOnRegistrationPageName,	t1.RegistrationTooltip	,");
                sb.AppendLine(" t1.ShowOnRegistrationPageValue	,t1.IsOtherDiscount	,t1.SortOrder	,t2.Event_Pkey	,t2.ChargeType_pKey,	t2.Amount	,Comment	,t2.pKey As Pkeys from sys_ChargeTypes t1");
                sb.AppendLine(" Inner Join Event_Pricing t2 on t2.ChargeType_pKey = t1.pKey and isnull(t2.IsActive,0)=1 and t2.Event_pKey = " + intCurEventPKey.ToString());
                sb.AppendLine(" where t1.pkey in ( (Select t2.ChargeCode from Certification_Detail t1 INNER JOIN  Certification_List t2 ON t2.pkey=t1.Certification_pKey  where event_pkey=" + intCurEventPKey.ToString() + " ))");
                sb.AppendLine(" OR t1.pkey in(2,43)");
                sb.AppendLine(" Order by t1.pKey");
            }
            else
            {
                sb.Append(" Select t1.ChargeTypeID +' ('+FORMAT(t2.Amount, 'C')+')' AS 	ChargeTypeID,ISNULL(t1.ChargeTypeID,'') as ChargeTypeText,t1.TypeOfCharge,t1.UserEditable,t1.pKey,t1.ShowOnRegistrationPage,	t1.ShowOnRegistrationPageName,	t1.RegistrationTooltip	,");
                sb.AppendLine(" t1.ShowOnRegistrationPageValue,t1.IsOtherDiscount	,t1.SortOrder,t2.Event_Pkey	,t2.ChargeType_pKey,t2.Amount,Comment,t2.pKey as Pkeys,isnull(t2.IsActive,0) as IsActive from sys_ChargeTypes t1");
                sb.AppendLine(" Inner Join Event_Pricing t2 on t2.ChargeType_pKey = t1.pKey and isnull(t2.IsActive,0)=1 and t2.Event_pKey = " + intCurEventPKey.ToString());
                sb.AppendLine(" where t1.pkey in ((Select t2.ChargeCode from Certification_Detail t1 INNER JOIN  Certification_List t2 ON t2.pkey=t1.Certification_pKey  where event_pkey=" + intCurEventPKey.ToString() + " ))");
                sb.AppendLine(" OR t1.pkey in(2,43)");
                sb.AppendLine(" Order by t1.pKey");
            }
            SqlCommand cmd = new SqlCommand(sb.ToString());
            DataTable dt = new DataTable();

            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public DataTable RefreshOffers(int intCurEventPKey)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" Select t1.OfferID, t1.OfferDescription");
            sb.AppendLine(",(case when substring(t1.OfferLink,1,7) = 'http://' Then t1.OfferLink else 'http://'+ t1.OfferLink end) as Link");
            sb.AppendLine(",(Case when isNull(t1.OfferLink,'') <> '' Then 1 else 0 End) as LinkVis");
            sb.AppendLine(" From Event_SpecialOffers t1");
            sb.AppendLine(" Where t1.Event_pKey = " + intCurEventPKey.ToString());
            sb.AppendLine(" UNION");
            sb.AppendLine(" Select t3.OrganizationID as OfferID, t2.SpecialOffer as OfferDescription");
            sb.AppendLine(",(case when substring(t2.SpecialOfferLink, 1, 7) = 'http://' Then t2.SpecialOfferLink else 'http://'+ t2.SpecialOfferLink end) as Link");
            sb.AppendLine(",(Case When isNull(t2.SpecialOfferLink,'') <> '' Then 1 else 0 End) as LinkVis");
            sb.AppendLine(" From Event_Organizations t2");
            sb.AppendLine(" Inner Join Organization_List t3 on t3.pkey = t2.Organization_pkey");
            sb.AppendLine(" and isNull(t2.SpecialOffer,'') <> ''");
            sb.AppendLine(" and isNull(t2.ShowOffer,0) = 1");
            sb.AppendLine(" Where t2.Event_pKey = " + intCurEventPKey.ToString());
            sb.AppendLine(" Order by OfferID");
            SqlCommand cmd = new SqlCommand(sb.ToString());
            DataTable dt = new DataTable();

            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public bool ArrangeSaveEvent(string txtSpArrangementEvent, string AccountPkey, string intCurEventPKey)
        {

            string qry = "Update Event_Accounts set SpecialRequest ='" + txtSpArrangementEvent + "' Where Account_pkey = " + AccountPkey + " AND Event_pkey=" + intCurEventPKey;
            SqlCommand cmd = new SqlCommand(qry);
            bool saved = false;
            if (clsUtility.ExecuteQuery(cmd, null, "Log Special Arrangement"))
            {
                saved = true;
            }
            return saved;
        }

        public List<SelectListItem> FillMyOptionsDropDowns(int index)
        {
            string qry = "";

            switch (index)
            {
                case 2:
                    qry = "select t1.pKey, t1.ReferralTypeID as strText  from Sys_ReferralTypes t1 order by strText ";
                    break;
                case 3:
                    qry = "SELECT pKey, ParticipationStatusID as strText FROM Sys_ParticipationStatuses Order by strText ";
                    break;
                case 4:
                    qry = "SELECT pKey, RegistrationLevelID as strText FROM Sys_RegistrationLevels Order by strText ";
                    break;
                case 5:
                    qry = "select t1.pKey, t1.TravelStatusID as strText from Sys_TravelStatuses t1  order by strText ";
                    break;
                case 6:
                    qry = "select t1.pKey, t1.LodgingStatusID as strText from Sys_LodgingStatuses t1 order by strText ";
                    break;
                case 7:
                    qry = "SELECT pKey, LicenseTypeID as strText FROM SYS_LicenseType Order by LicenseTypeID ";
                    break;
            }
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            List<SelectListItem> ddList = new List<SelectListItem>();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                foreach (DataRow dr in dt.Rows)
                {
                    ddList.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pKey"].ToString() });
                }
            }
            return ddList;
        }


        public DataTable LinkTranfer(string txtTREmail)
        {
            DataTable dt = new DataTable();
            string qry = "select pKey,FirstName,LastName, Contactname,email, isNull(ParentOrganization_pKey,0) as Org_pKey,isnull(Member,0) as Member from Account_list where email = @email";
            SqlCommand cmd = new SqlCommand(qry);
            cmd.Parameters.AddWithValue("@email", txtTREmail);
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public DataTable RefreshBadge(int AccountPkey)
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Select " + "isNull(t4.EditName,isNull(t1.Firstname,'') + ' ' + isNull(t1.Lastname,''))" + " as BName, isNull(t4.EditTitle,t1.Title) as BTitle, isNull(t4.EditOrganization,t2.OrganizationID) as BOrganizationID");
            sb.AppendLine(" From account_List t1");
            sb.AppendLine(" Left outer join Organization_list t2 on t2.pkey = t1.ParentOrganization_pKey");
            sb.AppendLine(" Left outer join Account_BadgeOverrides t4 on t4.Account_pkey = t1.pKey");
            sb.AppendLine(" Where t1.pKey = " + AccountPkey.ToString());
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand(sb.ToString());
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public bool DisApproveOneBadge(int intEventAccount_pKey)
        {

            bool DisApproveOneBadge = false;
            string qry = "Update Event_Accounts Set BadgeApproved =0,BadgeApprovedByAccount_pKey=Null, BadgeApprovedDate=Null  Where Pkey = " + intEventAccount_pKey.ToString();

            SqlCommand cmd = new SqlCommand(qry);
            bool saved = false;
            if (clsUtility.ExecuteQuery(cmd, null, "Update badges from option page"))
            {
                DisApproveOneBadge = true;
            }
            return DisApproveOneBadge;
        }

        public DataTable ResetBadge(int AccountPkey)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Select (isNull(t1.Firstname,'') + ' ' + isNull(t1.Lastname,'')) as BName, t1.Title as BTitle, t2.OrganizationID as BOrganizationID");
            sb.AppendLine(" From account_List t1");
            sb.AppendLine(" Left outer join Organization_list t2 on t2.pkey = t1.ParentOrganization_pKey");
            sb.AppendLine(" Where t1.pKey = " + AccountPkey.ToString());
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand(sb.ToString());
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;

        }

        public DataTable RefreshLunch(int AccountPkey, int intCurEventPKey)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select t1.pKey as EvtSession_pKey, isNull(t3.SpecialMeal_pKey,0) as Meal_pKey,isNull(t3.Attending,0) as bAttend, isNull(t3.MealRequest,'') as MealRequest,isNull(t3.Comment,'') as Comment");
            sb.AppendLine(",isNull(t3.DateUpdated,'') as DateUpdated,isnull(Mealtype,'0') as Mealtype,isnull(t5.IsSpecial,0) as IsSpecial,isnull(DefaultMeal_Pkey,0) as DefaultMeal_Pkey,t4.SpecialMealID");
            sb.AppendLine(",(datename(WEEKDAY,t1.StartTime) + ', '+ datename(month,t1.StartTime)+ ' ' + cast(datepart(day,t1.StartTime) as varchar) +'<br/><i>'+ isNull(t1.SessionFood,'')+'</i>') as Lunch");
            sb.AppendLine(" From Event_Sessions t1");
            sb.AppendLine(" inner join Session_List t2 On t2.pkey = t1.Session_pKey");
            sb.AppendLine(" left outer join EventSession_Accounts t3 on t3.EventSession_pKey = t1.pkey and t3.Account_pKey = " + AccountPkey.ToString());
            sb.AppendLine(" left outer join SYS_SpecialMeals t4 on t4.pKey =t3.SpecialMeal_pKey");
            sb.AppendLine(" left outer join SYS_SpecialMeals t5 on t5.pKey =t3.SpecialMealRequest_pkey");
            sb.AppendLine(" Where t1.Event_pKey = " + intCurEventPKey.ToString());
            sb.AppendLine(" And t2.sessiontype_pkey In(" + clsSession.SESSIONTYPE_Lunch.ToString() + ")");
            sb.AppendLine(" and t2.pkey <> 2398");  //'special case for the 'enjoy the town lunch
            sb.AppendLine(" Order by t1.StartTime");
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand(sb.ToString());
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;

        }


        public DataTable RefreshRecent(int AccountPkey, int EventPkey)
        {
            string s = clsUtility.TYPE_Venue.ToString() + "," + clsUtility.TYPE_Event.ToString() + "," + clsUtility.TYPE_EventSession.ToString() + "," + clsUtility.TYPE_Organization.ToString() + "," + clsUtility.TYPE_Account.ToString();

            StringBuilder sb = new StringBuilder();
            sb.Append("Select Top 9 dbo.GetEntityName(t1.RecentTYpe, t1.RecentPKey) as LinkText");
            sb.AppendLine(",dbo.GetEntityLink(t1.RecentTYpe, t1.RecentPKey) as LinkURL");
            sb.AppendLine("From Recent_List t1");
            sb.AppendLine("Where t1.Account_pkey = " + AccountPkey.ToString());
            sb.AppendLine("and t1.RecentTYpe in(" + s + ")");
            sb.AppendLine("and t1.event_pkey = " + EventPkey.ToString());
            sb.AppendLine("Order by t1.pKey Desc");
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand(sb.ToString());
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;

        }


        public DataTable RefreshTasks(int AccountPkey, int intActiveEventPkey, int intNextEvent_pKey, int intPrimaryEvent_pkey)
        {
            string qry = "Select * from dbo.getTaskInfoForAccount(" + AccountPkey.ToString() + "," + intActiveEventPkey.ToString() + ",0," + intNextEvent_pKey.ToString() + "," + intPrimaryEvent_pkey.ToString() + ")";
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand(qry);
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;

        }

        public DataTable RefreshChairs(int intCurEventPKey)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" Select count(t1.NumChairs) As NumPlanned");
            sb.AppendLine(" ,sum(Case When t3.IsSessionChair =1 Then 1 Else 0 End) As NumAssigned");
            sb.AppendLine(" From Session_List t1");
            sb.AppendLine(" Inner Join event_sessions t2 On t2.Session_pkey = t1.pkey");
            sb.AppendLine(" Left OUTER JOIN EventSession_Staff t3 On t3.EventSession_pKey = t2.pkey And t3.IsSessionChair =1");
            sb.AppendLine(" Where t2.Event_pkey = " + intCurEventPKey.ToString());
            sb.AppendLine(" And (isNull(t1.NumChairs,1) > 0) And ISNULL(t2.IsScheduled,0)=1");
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand(sb.ToString());
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public DataTable Refreshw9(int intCurEventPKey)
        {

            string qry = " Select count(t0.pKey) As TotalPending from W9Request_List t0 Inner Join Account_List t1 On t1.pKey = t0.RequestByAccount_pkey Left Outer Join sys_w9statuses t2 On t2.pkey = t0.W9Status_pKey LEFT OUTER JOIN Event_List t3 On t3.pkey  = " + intCurEventPKey.ToString();
            qry = qry + "Where isNull(t2.W9StatusID,'Pending')='Pending' AND t1.Activated =1 AND (t0.RequestedOn Between t3.RegStartDate AND t3.RegEndDate)";
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand(qry);
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;

        }

        public DataTable RefreshBadges(int intAccount_PKey, int intActiveEventPkey)
        {
            string qry = "Select * from dbo.getBadgeInfoForAccount(" + intAccount_PKey.ToString() + "," + intActiveEventPkey.ToString() + ")";
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand(qry);
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public DataTable RefreshCOI(int intCurEventPKey)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Select count(x1.Account_pKey) as NumSpeakers");
            sb.AppendLine(",ISNULL(sum (case when x2.COI_SetDate is Null and x2.COI_pKey = 1 then 1 else 0 end),0) as NumWithByDefault");
            sb.AppendLine(",ISNULL(sum (case when x2.COI_SetDate is not Null and x2.COI_pKey = 1 then 1 else 0 end),0) as NumWith");
            sb.AppendLine(",ISNULL(sum (case when x2.COI_pKey = 2 then 1 else 0 end),0)  as NumWithout");
            sb.AppendLine(",ISNULL(sum (case when x2.COI_pKey is Null then 1 else 0 end),0)  as NumNA");
            sb.AppendLine("From (Select distinct t1.Account_pKey");
            sb.AppendLine("		From EventSession_Staff t1");
            sb.AppendLine("		Inner Join event_sessions t2 on t2.pkey = t1.EventSession_pkey");
            sb.AppendLine("		Where t2.Event_pkey = " + intCurEventPKey.ToString());
            sb.AppendLine("		and t1.IsSpeaker =1) x1");
            sb.AppendLine("inner join Event_Accounts x2 on x2.account_pkey = x1.account_pkey");
            sb.AppendLine("Where x2.Event_pkey = " + intCurEventPKey.ToString());
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand(sb.ToString());
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }


        public DataTable RefreshNotEnteredCOI(int intCurEventPKey)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("  SELECT Count(distinct Account_pKey) As TotalNotEntered From (SELECT  DISTINCT ESS.Account_pKey  From  EventSession_Staff ESS");
            sb.AppendLine(" INNER JOIN Event_Accounts EA ON ESS.Account_pKey = EA.Account_pKey AND EA.Event_pKey=" + intCurEventPKey.ToString());
            sb.AppendLine(" INNER JOIN event_sessions ES on ES.pkey = ESS.EventSession_pkey AND ES.Event_pKey = " + intCurEventPKey.ToString());
            sb.AppendLine(" Where ESS.IsSpeaker =1 AND ESS.Account_pKey NOT IN (Select t1.Account_pkey From Attendee_EnterWebinar t1 where t1.Event_pKey=" + intCurEventPKey.ToString() + ")) A");
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand(sb.ToString());
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public DataTable RefreshComm(int intCurEventPKey)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Select isNull(sum (Case When cast(t1.ContactDate As Date) = cast(getdate() As Date) Then 1 Else 0 End),0) As ContactsToday");
            sb.AppendLine(",isNull(sum (Case When cast(t1.ContactDate As Date) = cast(getdate() As Date) And x.account_pkey > 0 Then 1 Else 0 End),0) As SpkCount");
            sb.AppendLine(",isNull(sum (Case When cast(t1.ContactDate As Date) = cast(getdate() As Date) And isNull(x.account_pkey,0) = 0 Then 1 Else 0 End),0) As NonSpkCount");
            sb.AppendLine(",isNull(sum (Case When cast(t1.NextActionDate As Date) <= cast(getdate() As Date) And (t1.CallOutcome_pKey Is Null) Then 1 Else 0 End),0) As Due");
            sb.AppendLine("From EventAccount_Communication t1");
            sb.AppendLine("Inner Join event_accounts t2 On t2.pkey = t1.eventaccount_pkey");
            sb.AppendLine("Left outer Join (Select x1.account_pkey from EventSession_Staff x1 Inner Join Event_Sessions x2 On x2.pKey = x1.EventSession_pkey where x1.IsSpeaker = 1 And x2.Event_pKey = " + intCurEventPKey.ToString() + ") x On x.account_pkey = t2.account_pkey");
            sb.AppendLine("Where t2.Event_pkey = " + intCurEventPKey.ToString());
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand(sb.ToString());
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public DataTable CountUSer()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("With Qry As (Select *,(ROW_NUMBER() OVER (PARTITION BY ID ORDER BY onlinedate DESC)) As RowNo  From  sys_onlinePeople )");
            sb.AppendLine(" Select (Select Count(1) From Qry Where RowNo =1) As OnlineUserCount,");
            sb.AppendLine(" ISNULL((SELECT SUM(CASE When Qry.PageURL IN ('/EducationCenter','/ExhibitorDirectory','/EventOnCloud','/ScheduledEvent','/NetworkingLounge','/NetworkingModel','/CommunityShowcase','/ResourceSupportCenter',");
            sb.AppendLine(" '/MyNetworking','/VirtualEvent','/EventSponsorsDirectory','/PhotoWall') Then  1 Else 0 END) From Qry  Where RowNo =1),0) as OnlineEeventUserCount");
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand(sb.ToString());
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;

        }

        public DataTable RefreshRegChart(int intCurEventPKey, DateTime dtStart, DateTime dtEnd)
        {

            SqlCommand sqlCmd = new SqlCommand("getAttendeeRegistrationsLogs", con);
            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.CommandTimeout = 30;
            clsUtility.AddParameter(ref sqlCmd, "@Event_PKey", SqlDbType.Int, ParameterDirection.Input, intCurEventPKey);
            clsUtility.AddParameter(ref sqlCmd, "@Start", SqlDbType.DateTime, ParameterDirection.Input, dtStart);
            clsUtility.AddParameter(ref sqlCmd, "@End", SqlDbType.DateTime, ParameterDirection.Input, dtEnd);

            DataTable dt = new DataTable();

            if (clsUtility.GetDataTableStored(con, sqlCmd, ref dt))
            {
                return dt;
            }
            return dt;
        }
        public DataTable RefreshCumChart(int intCurEventPKey, DateTime dtStart, DateTime dtEnd, string intAttendanceGoal)
        {

            SqlCommand sqlCmd = new SqlCommand("getCumulativeRegistrationsInfo", con);
            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.CommandTimeout = 30;
            clsUtility.AddParameter(ref sqlCmd, "@Event_PKey", SqlDbType.Int, ParameterDirection.Input, intCurEventPKey);
            clsUtility.AddParameter(ref sqlCmd, "@Start", SqlDbType.DateTime, ParameterDirection.Input, dtStart);
            clsUtility.AddParameter(ref sqlCmd, "@End", SqlDbType.DateTime, ParameterDirection.Input, dtEnd);
            clsUtility.AddParameter(ref sqlCmd, "@intAttendanceGoal", SqlDbType.VarChar, ParameterDirection.Input, intAttendanceGoal);

            DataTable dt = new DataTable();

            if (clsUtility.GetDataTableStored(con, sqlCmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public DataTable RefreshSpkChart(int intCurEventPKey, DateTime dtStart, DateTime dtEnd, int intTotal)
        {

            SqlCommand sqlCmd = new SqlCommand("getSpeakerStatusInfo", con);
            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.CommandTimeout = 30;
            clsUtility.AddParameter(ref sqlCmd, "@Event_PKey", SqlDbType.Int, ParameterDirection.Input, intCurEventPKey);
            clsUtility.AddParameter(ref sqlCmd, "@Start", SqlDbType.DateTime, ParameterDirection.Input, dtStart);
            clsUtility.AddParameter(ref sqlCmd, "@End", SqlDbType.DateTime, ParameterDirection.Input, dtEnd);
            clsUtility.AddParameter(ref sqlCmd, "@intTotal", SqlDbType.Int, ParameterDirection.Input, intTotal);

            DataTable dt = new DataTable();

            if (clsUtility.GetDataTableStored(con, sqlCmd, ref dt))
            {
                return dt;
            }
            return dt;
        }


        #region User Resources

        #region journal
        public DataTable BindJournelYear()
        {
            string qry = string.Empty;
            qry = qry + Environment.NewLine + "declare @selectedYear int = -1;";
            qry = qry + Environment.NewLine + "set @selectedYear = (Select year(Max(JournalDate)) setDefault From FCR_Journal);";
            qry = qry + Environment.NewLine + "select ROW_NUMBER() over (order by YEAR(jDate)) as pkey,jDate,selectedYear";
            qry = qry + Environment.NewLine + "from";
            qry = qry + Environment.NewLine + "(";
            qry = qry + Environment.NewLine + "select YEAR(getdate()) as jDate,@selectedYear as selectedYear";
            qry = qry + Environment.NewLine + "union";
            qry = qry + Environment.NewLine + "select YEAR (JournalDate) as jDate,@selectedYear as selectedYear";
            qry = qry + Environment.NewLine + "from FCR_Journal group by YEAR(JournalDate)";
            qry = qry + Environment.NewLine + ") as A";
            qry = qry + Environment.NewLine + "order by jDate desc;";
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand(qry);
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public DataTable BindJournelMonths()
        {
            string qry = string.Empty;
            qry = qry + Environment.NewLine + "declare @mnth int = isnull((Select month(Max(JournalDate)) setDefault From FCR_Journal),0);";
            qry = qry + Environment.NewLine + ";WITH months(MonthNumber) AS";
            qry = qry + Environment.NewLine + "(";
            qry = qry + Environment.NewLine + "SELECT 1";
            qry = qry + Environment.NewLine + "UNION ALL";
            qry = qry + Environment.NewLine + "SELECT MonthNumber+1";
            qry = qry + Environment.NewLine + "FROM months";
            qry = qry + Environment.NewLine + "WHERE MonthNumber < 12";
            qry = qry + Environment.NewLine + ")";
            qry = qry + Environment.NewLine + "select MonthNumber as pKey,DateName(month,DateAdd(month,MonthNumber,-1)) AS strText,@mnth as mnth";
            qry = qry + Environment.NewLine + "from months;";
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand(qry);
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public DataTable RefreshSearchJournel(string apppath, string ID, bool bExport, string ddYear, string ddMonth, bool bckCurrentSrch, string prvSrchText, int rdSort, string strSrchText, int selIndex, int rdStatus, string txtTitle, int intAccount_PKey, bool needSearchAmongAll = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select t1.pKey, FORMAT(JournalDate, 'MMMM') + CHAR(13) + FORMAT(JournalDate, 'yyyy') as mnth, FilePath as pUrl, blurb, '" + apppath + "' + " + " + FilePath as CopyToClip");

            if (ID != "")
            {
                sb.AppendLine(",((len([Text])-len(REPLACE([Text],@TxtC,'')))/len(@TxtC)) AS Occur");
                if (bExport)
                {
                    sb.AppendLine(",[Title]");
                }
                else
                {
                    sb.AppendLine(",[Title], IIF(charindex(@TxtC+' ','#'+[Title])>0,1,0) srt");
                }
            }
            else
            {
                sb.AppendLine(",[Title] , '' as Occur");
            }
            sb.AppendLine(",Text from FCR_Journal t1");
            sb.AppendLine(" Where 1=1 and t1.Status=1");

            //if (rdStatus > 0)
            //{
            //    sb.AppendLine("  and t1.Status= " + rdStatus + "");
            //}

            if (!needSearchAmongAll)
            {
                if (ddYear != "All" && ddMonth != "All")
                {
                    sb.AppendLine(" and datename(m,JournalDate)+' '+cast(datepart(yyyy,JournalDate) as varchar)  = '" + ddMonth + " " + ddYear + "'");
                }
                else if (ddYear != "All")
                {
                    sb.AppendLine(" And year(JournalDate ) ='" + ddYear + "'");
                }
                else if (ddMonth != "All")
                {
                    sb.AppendLine(" And datename(m,JournalDate) ='" + ddMonth + "'");
                }
            }


            if (ID != "")
            {
                if (bckCurrentSrch)
                {
                    string[] dxt = prvSrchText.Split('/');
                    foreach (var d in dxt)
                    {
                        sb.AppendLine(" and ((Contains(t1.Title, '\"" + prvSrchText + "\"')) ");
                        sb.AppendLine("or (Contains(t1.[Text], '\"" + prvSrchText + "\"')))");
                    }
                }
                else
                {

                    //if (!Request.QueryString("NextSearch") == null)
                    //{
                    //    string[] dxt = strSrchText.Split('/');
                    //    foreach (var d in dxt)
                    //    {
                    //        sb.AppendLine(" and (Contains(t1.Title, " + (d.Contains("\"\"") ? "'\"\"\"\"" + d.Replace("'", "''") + "\"\"\"\"'" : "'\"\"" + d.Replace("'", "''") + "\"\"'") + ") or Contains(t1.Text, " + (d.Contains("\"\"") ? "'\"\"\"\"" + d.Replace("'", "''") + "\"\"\"\"'" : "'\"\"" + d.Replace("'", "''") + "\"\"'") + ")) ");
                    //    }
                    //}
                    //else
                    {
                        sb.AppendLine(" and ((t1.Title like '%'+ @Text +'%') or (t1.[Text] like '%'+ @Text +'%')) ");
                    }
                }
            }
            if (ID != "")
            {
                if (rdSort == 0)
                {
                    sb.AppendLine(" Order by Sequence,JournalDate desc ");
                }
                else
                {
                    sb.AppendLine(" Order by JournalDate desc ");
                }
            }
            else
            {
                sb.AppendLine(" Order by Sequence,JournalDate desc ");
            }

            SqlCommand cmd = new SqlCommand(sb.ToString());
            cmd.Parameters.AddWithValue("@Text", (strSrchText.Contains("\"\"") ? "\"\"\"\"" + strSrchText + "\"\"\"\"" : strSrchText));
            cmd.Parameters.AddWithValue("@TxtC", ID);

            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                if (txtTitle != "")
                    clsFCR.ResourcesSavedSearch(intAccount_PKey, clsUtility.TYPE_Journal, intAccount_PKey, txtTitle, (dt.Rows.Count > 0 ? 1 : 0));
                return dt;
            }
            return dt;
        }
        public string JournalCount()
        {
            string count = "";
            string qry = "";
            qry = "select count(*) as Count from FCR_Journal";
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                if (dt.Rows.Count > 0)
                {

                    count = (dt.Rows[0]["Count"].ToString());
                }
            }
            return count;
        }
        public int SetMonth()
        {
            string qry = "Select month(Max(JournalDate)) setDefault From FCR_Journal";
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return Convert.ToInt32(dt.Rows[0]["setDefault"].ToString());
            }
            return 0;
        }

        public bool UpdateJournal(bool chk, int AccountPkey)
        {
            bool updated = false;

            string qry = "Update Account_List  Set GetJournal = @Jrn Where pKey = @pkey";
            SqlCommand cmd = new SqlCommand(qry);
            cmd.Parameters.AddWithValue("@Jrn", chk);
            cmd.Parameters.AddWithValue("@pkey", AccountPkey);
            if (clsUtility.ExecuteQuery(cmd, null, "Update Journal Subscription Details"))
            {
                updated = true;
            }
            return updated;
        }

        #endregion


        #region Best practice standard


        public DataTable ddCategory()
        {

            string qry = "SELECT pKey, StandardCategoryID as strText FROM Sys_StandardCategories Order by strText ";

            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand(qry);
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }
        public DataTable ddStages()
        {
            string qry = "SELECT t1.pKey, t1.StudyPhaseID as strText  FROM SYS_StudyPhases t1  Order by t1.SortOrder, strText ";

            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand(qry);
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }
        public DataTable ddUsers()
        {
            string qry = "SELECT pKey, StandardUserID as strText FROM SYS_StandardUsers  Order by SortOrder, strText";

            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand(qry);
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public DataTable RefreshStandardGrid(bool bLoggedIn, int ddCategory, int ddPhase, int ddUser, string strText, int intAccount_PKey)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Select t1.pkey, ISNULL(t1.StandardTitle,'') as StandardTitle , ISNULL(t1.Description,'') as Description, IIF(ISNULL(t1.UpdateDate,'') <>'', '('+ convert(varchar,t1.UpdateDate,101) +')','') as UDate, ISNULL(t1.UserFileName,'') as UserFileName");
            sb.AppendLine("," + (bLoggedIn ? "1" : "0") + " as ShowCheck");
            sb.AppendLine("," + (bLoggedIn ? "0" : "1") + " as NoShowCheck");
            sb.AppendLine(" From standards_list t1  Where 1=1");
            if (ddCategory > 0)
            {
                sb.AppendLine(" And t1.StandardCategory_pKey = " + ddCategory.ToString());
            }
            if (ddPhase > 0)
            {
                sb.AppendLine(" And t1.pkey in(select standard_pkey from standards_studyphases where StudyPhase_pKey = " + ddPhase.ToString() + ")");
            }
            if (ddUser > 0)
            {
                sb.AppendLine(" And t1.pkey in(select standard_pkey from standards_users where StandardUser_pKey = " + ddUser.ToString() + ")");
            }
            if (strText != "")
            {
                sb.AppendLine("And t1.StandardTitle Like @Text");
            }
            sb.AppendLine(" Order by t1.StandardTitle");
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand(sb.ToString());
            if (strText != "")
            {
                cmd.Parameters.AddWithValue("@Text", "%" + strText + "%");
            }
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                if (strText != "")
                {
                    clsFCR.ResourcesSavedSearch(intAccount_PKey, clsUtility.TYPE_Standard, intAccount_PKey, strText, (dt.Rows.Count > 0 ? 1 : 0));
                }
                return dt;
            }
            return dt;
        }
        public string StandardCount()
        {
            string count = "";
            string qry = "";
            qry = "select count(*) as Count from Standards_List";
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                if (dt.Rows.Count > 0)
                {

                    count = (dt.Rows[0]["Count"].ToString());
                }
            }
            return count;
        }
        #endregion


        #region Directories

        public DataTable RefreshDirectories()
        {

            string qry = "";
            qry = qry + Environment.NewLine + "Select count(0) as ctn,1 as srt from FCR_Associations where status=1 ";
            qry = qry + Environment.NewLine + "Union								   ";
            qry = qry + Environment.NewLine + "Select count(0) as ctn,2 as srt from FCR_Books where status=1		   ";
            qry = qry + Environment.NewLine + "Union									   ";
            qry = qry + Environment.NewLine + "Select count(0) as ctn,3 as srt from FCR_Government where status=1  ";
            qry = qry + Environment.NewLine + "Union								   ";
            qry = qry + Environment.NewLine + "Select count(0) as ctn,4 as srt from FCR_Webinar where Date>DATEADD(DAY, 1, GETDATE()) and status=1	   ";
            qry = qry + Environment.NewLine + "Union									   ";
            qry = qry + Environment.NewLine + "Select count(0) as ctn,5 as srt from FCR_Publications where status=1 ";
            qry = qry + Environment.NewLine + "Union										   ";
            qry = qry + Environment.NewLine + "Select count(0) as ctn,6 as srt from FCR_Sponsors	where status=1   ";
            qry = qry + Environment.NewLine + "Union									   ";
            qry = qry + Environment.NewLine + "Select count(0) as ctn,7 as srt from FCR_Staffing	where status=1   ";
            qry = qry + Environment.NewLine + "Union									   ";
            qry = qry + Environment.NewLine + "Select count(0) as ctn,8 as srt from FCR_Suppliers where status=1	   ";
            qry = qry + Environment.NewLine + "Union								   ";
            qry = qry + Environment.NewLine + "Select count(0) as ctn,9 as srt from FCR_Training	where status=1";
            qry = qry + Environment.NewLine + "Union									   ";
            qry = qry + Environment.NewLine + "Select count(0) as ctn,10 as srt from FCR_Events  where dateEnd>=getdate() and status=1 ";
            qry = qry + Environment.NewLine + "order by srt									   ";
            var cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }
        #endregion

        #region RegDocs

        public DataTable RefreshRegDocs(string ID, string lkDisplay, int ddDocuments, bool ckCurrentSrch, string prvSrchText, bool ckCFR, bool ckStatutes, bool ckICH, bool chGuidances, bool ckInternational, bool ckOther, int intAccount_PKey)
        {
            string qry = "";
            qry = qry + Environment.NewLine + "Select t1.pKey, case when(t1.[Url] like '%www.ich.org%') then 'https://www.ich.org/page/quality-guidelines' else t1.[Url] end as [Url],t1.Category";
            qry = qry + Environment.NewLine + ",'...' + SUBSTRING(text, CHARINDEX(@TxtC, text)-50, 200) + '...' As Text1";
            qry = qry + Environment.NewLine + ",'...' + SUBSTRING(text, CHARINDEX(@TxtC, text,CHARINDEX(@TxtC, text)+1)-50, 200)+ '...' As Text2";
            qry = qry + Environment.NewLine + ",'...' + SUBSTRING(text, CHARINDEX(@TxtC, text,CHARINDEX(@TxtC, text,CHARINDEX(@Txtc,text)+1)+1)-50, 200)+ '...' As Text3";

            if (ID != "")
            {
                qry = qry + Environment.NewLine + ",((DATALENGTH([Text])-DATALENGTH(REPLACE([Text],@TxtC,'')))/DATALENGTH(@TxtC)) AS Occur,IIF(charindex(@TxtC,'#'+[Title])>0,1,0) srt";
                qry = qry + Environment.NewLine + ", [Title]";
            }
            else
            {
                qry = qry + Environment.NewLine + ",t1.Title,'' as Occur";
            }

            qry = qry + Environment.NewLine + " From FCR_RegDocs t1 Where status=1";

            if (lkDisplay != "" && ddDocuments > 0)
            {
                qry = qry + Environment.NewLine + "And (t1.[Text] like @lkTcext+'%' Or t1.Title like @lkTcext+'%')";
            }
            else if (lkDisplay != "")
            {
                qry = qry + Environment.NewLine + "And (t1.[Text] like @lkTcext+'%' Or t1.Title like @lkTcext+'%')";
            }

            if (ID != "")
            {
                if (ckCurrentSrch)
                {
                    string[] dxt = prvSrchText.Split('/');
                    foreach (var d in dxt)
                    {
                        qry = qry + Environment.NewLine + " and (Contains(t1.Title, '\"" + d.Replace("'", "''") + "\"')  or Contains(t1.Text, '\"" + d.Replace("'", "''") + "\"'))";
                    }
                }
                else
                {
                    qry = qry + Environment.NewLine + " and (contains(t1.Text,@Txt) or contains(t1.Title, @Txt))";
                }

                if (ckCFR || ckStatutes || ckICH || chGuidances || ckInternational || ckOther)
                {
                    qry = qry + Environment.NewLine + "and (";
                    string str = "";
                    if (ckCFR)
                    {
                        str += (str != "" ? "" : "") + " t1.Category ='" + "CFR" + "'";
                    }
                    if (ckStatutes)
                    {
                        str += (str != "" ? "or" : "") + "  t1.Category ='" + "STA" + "'";
                    }
                    if (ckICH)
                    {
                        str += (str != "" ? "or" : "") + "  t1.Category ='" + "ICH" + "'";
                    }
                    if (chGuidances)
                    {
                        str += (str != "" ? "or" : "") + "  t1.Category ='" + "FDA" + "'";
                    }
                    if (ckInternational)
                    {
                        str += (str != "" ? "or" : "") + "  t1.Category ='" + "INT" + "'";
                    }
                    if (ckOther)
                    {
                        str += (str != "" ? "or" : "") + "  t1.Category ='" + "OTH" + "'";
                    }

                    qry = qry + Environment.NewLine + str + " )";

                }
            }
            else
            {
                qry = qry + Environment.NewLine + " and (t1.title='')";
            }

            if (ID != "")
            {
                qry = qry + Environment.NewLine + " order by srt desc,Occur desc";
            }
            else
            {
                qry = qry + Environment.NewLine + " order by Title";
            }

            var cmd = new SqlCommand(qry);
            cmd.Parameters.AddWithValue("@Txt", ID); //("@Txt", "\"\"" + ID + "\"\"")
            cmd.Parameters.AddWithValue("@lkTcext", lkDisplay); //("@lkTcext", "\"\"" + lkDisplay + "\"\"")
            cmd.Parameters.AddWithValue("@TxtC", ID);

            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                if (ID != "")
                {
                    clsFCR.ResourcesSavedSearch(intAccount_PKey, clsUtility.TYPE_RegDocs, intAccount_PKey, ID, (dt.Rows.Count > 0 ? 1 : 0));
                }
                return dt;
            }
            return dt;

        }

        public DataTable BindDocumentDropdown(string rdSrch)
        {
            DataTable dt = new DataTable();
            if (rdSrch != "")
            {
                string qry = " select ROW_NUMBER () over (order by title) as pkey, Title from FCR_RegDocs where Category = '" + rdSrch + "'";
                qry = qry + Environment.NewLine + " group by title order by Title ";

                var cmd = new SqlCommand(qry);
                if (clsUtility.GetDataTable(con, cmd, ref dt))
                {
                    return dt;
                }
            }
            return dt;
        }

        public DataTable lkDisplayLink(string lkDisplay)
        {

            string qry = "";
            qry = "Select top 1 t1.pKey,t1.url,t1.title,t1.Category From FCR_RegDocs t1 ";
            qry = qry + Environment.NewLine + " Where 1=1 AND ";
            qry = qry + Environment.NewLine + " t1.Title = @title ";
            var cmd = new SqlCommand(qry);
            cmd.Parameters.AddWithValue("@title", lkDisplay);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }
        #endregion

        #region FdaGcp


        public DataTable FdaGcpRefresh(string ID, bool ckCurrentSrch, string prvSrchText, int ddAnswrIndex, string ddAnswrText, string rdSor1)
        {
            string qry = "";
            qry = qry + Environment.NewLine + "Select  t1.pKey, cast(format(date,'MM/dd/yyyy') as varchar(20)) as dts ,  t1.Text, t1.Url ";
            if (ID != "")
            {
                qry = qry + Environment.NewLine + ",((LEN([Text])-LEN(REPLACE([Text],@txtC,'')))/LEN(@txtC)) AS Occur, IIF(charindex(@txtC,'#'+[Title])>0,1,0) srt";
                qry = qry + Environment.NewLine + ",[Title]";
            }
            else
            {
                qry = qry + Environment.NewLine + " ,[Title] , '' as Occur";
            }

            qry = qry + Environment.NewLine + " From FCR_FdaGcp t1 Where 1=1 and status=1  "; //

            if (ID != "")
            {
                if (ckCurrentSrch)
                {
                    string[] dxt = prvSrchText.Split('/');
                    foreach (var d in dxt)
                    {
                        qry = qry + Environment.NewLine + " and (Contains(t1.Title, '\"" + d.Replace("'", "''") + "\"')  or Contains(t1.Text, '\"" + d.Replace("'", "''") + "\"'))";
                    }

                }
                else
                {
                    qry = qry + Environment.NewLine + " and (Contains(t1.Title, @txt)  or Contains(t1.Text, @txt))";
                }

            }
            else
            {
                qry = qry + Environment.NewLine + " and (t1.title='' or t1.text='')";
            }

            if (ddAnswrIndex > 0)
            {
                qry = qry + Environment.NewLine + " and (year(t1.date) >='" + ddAnswrText + "')";
            }

            if (rdSor1 == "0")
            {
                if (ID != "")
                    qry = qry + Environment.NewLine + " order by srt desc, Occur desc";
            }
            else if (rdSor1 == "1")
            {
                qry = qry + Environment.NewLine + " order by date desc";
            }
            else
            {
                qry = qry + Environment.NewLine + " order by year(date),Title";
            }

            SqlCommand cmd = new SqlCommand(qry);
            cmd.Parameters.AddWithValue("@txt", ID); //"\"\"" + ID+ "\"\""
            cmd.Parameters.AddWithValue("@txtC", ID);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }
        public string FdaGcpCount()
        {
            string count = "";
            string qry = "";
            qry = "select count(*) as Count from FCR_FdaGcp where [Status]=1";
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                if (dt.Rows.Count > 0)
                {

                    count = (dt.Rows[0]["Count"].ToString());
                }
            }
            return count;
        }
        public bool updateFdaGcpSearch(string startTime, string endTime)
        {
            bool result = false;
            string qry = "";
            qry = qry + Environment.NewLine + "update Resources_Save_Search set SearchStartTime= @startDate ,SearchEndTime= @endDate where pkey=";
            qry = qry + Environment.NewLine + "(select top 1 pkey from Resources_Save_Search order by pkey desc)";
            SqlCommand cmd = new SqlCommand(qry, con);
            cmd.Parameters.AddWithValue("@startDate", startTime);
            cmd.Parameters.AddWithValue("@endDate", endTime);
            cmd.CommandType = CommandType.Text;
            con.Open();
            int i = 0;
            try
            {
                i = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                con.Close();
            }

            if (i > 0)
            {
                result = true;
            }
            //DataTable dt = new DataTable();
            //if (clsUtility.GetDataTable(con, cmd, ref dt))
            //{
            //    result = true;
            //}
            return result;

        }
        #endregion

        #region Glossary

        public DataTable RefreshGlossary(string ID, bool ckCurrentSrch, string prvSrchText, bool chkDefinition)
        {
            ID = ID.Replace("\"\"", "");
            string qry = "";

            qry = qry + Environment.NewLine + "select t1.pKey,  t1.definition ,iif (t1.source='','','['+t1.source+']') as source  ";
            if (ID != "")
            {
                qry = qry + Environment.NewLine + " , [name],IIF(charindex(@txtC,'#'+name)>0,1,0) srt";
            }
            else
            {
                qry = qry + Environment.NewLine + " ,[name]";
            }


            qry = qry + Environment.NewLine + " From FCR_Glossary t1";
            qry = qry + Environment.NewLine + " Where 1=1 and status=1";

            if (ID != "")
            {
                if (ckCurrentSrch)
                {
                    string[] dxt = prvSrchText.Split('/');
                    foreach (var d in dxt)
                    {
                        if (chkDefinition)
                        {
                            qry = qry + Environment.NewLine + " and (Contains(t1.Name, '\"" + d.Replace("'", "''") + "\"')  or Contains(t1.definition, '\"" + d.Replace("'", "''") + "\"'))";
                        }
                        else
                        {
                            qry = qry + Environment.NewLine + " and contains(t1.Name, '\"" + d.Replace("'", "''") + "\"')";
                        }
                    }
                }
                else
                {
                    if (chkDefinition)
                    {
                        qry = qry + Environment.NewLine + "and (contains(t1.name,@Txt) or contains(t1.definition,@Txt))";
                    }
                    else
                    {
                        qry = qry + Environment.NewLine + " and contains(t1.name,@Txt) ";
                    }

                }
            }
            else
            {
                qry = qry + Environment.NewLine + " and (t1.Name ='')";
            }

            if (ID != "")
            {
                qry = qry + Environment.NewLine + " order by srt desc,Name ";
            }
            else
            {
                qry = qry + Environment.NewLine + " order by Name";
            }

            SqlCommand cmd = new SqlCommand(qry);
            cmd.Parameters.AddWithValue("@txt", ID); //"\"\"" + ID+ "\"\""
            cmd.Parameters.AddWithValue("@txtC", ID);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public string SaveSearch(string txtGenSearch, int IntFound, int intAccount_PKey, int GlossaryType)
        {
            string intpkey = "";
            string qry = "insert into Resources_Save_Search(Account_pkey,Type_pkey,UpdatedByAccount_pkey,Search,IsFound,saved,Date)";
            qry = qry + Environment.NewLine + "values(@Account_pkey,@Type_pkey,@UpdatedByAccount_pkey,@Search,@IsFound,0,getdate())";
            qry = qry + Environment.NewLine + "SELECT @@IDENTITY AS pKey";

            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand(qry);
            cmd.CommandType = CommandType.Text;
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("@Account_pkey", intAccount_PKey);
            cmd.Parameters.AddWithValue("@Type_pkey", GlossaryType);
            cmd.Parameters.AddWithValue("@UpdatedByAccount_pkey", intAccount_PKey);
            cmd.Parameters.AddWithValue("@Search", txtGenSearch);
            cmd.Parameters.AddWithValue("@IsFound", IntFound);
            try
            {
                da.SelectCommand = cmd;
                da.Fill(dt);
                intpkey = dt.Rows[0][0].ToString();
            }
            catch (Exception ex)
            {
            }
            return intpkey;
        }


        public bool SaveSrchSave(int intpkey)
        {
            bool saved = false;
            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();
            string qry = " Update Resources_Save_Search set saved=1, IsVisible = 1 where pkey=@Pkey ";

            try
            {
                SqlCommand cmd = new SqlCommand(qry);
                if (intpkey > 0)
                {
                    cmd.Parameters.AddWithValue("@Pkey", intpkey);
                }

                if (!clsUtility.ExecuteQuery(cmd, null, "Save Updated"))
                {
                    clsUtility.LogErrorMessage(null, null, null, 0, "Error updating search saved");
                    return saved = false;
                }
                return saved = true;
            }
            catch (Exception ex)
            {
                return saved = false;
            }
        }

        public DataTable FillGlossaryDropdown(int intAccount_PKey, int GlossaryType)
        {
            string qry = "SELECT t1.pKey, t1.search as strText";
            qry = qry + Environment.NewLine + " FROM Resources_Save_Search t1";
            qry = qry + Environment.NewLine + " Where t1.account_pkey = " + intAccount_PKey.ToString();
            qry = qry + Environment.NewLine + "and IsVisible=1 and saved=1 and Type_pkey=" + GlossaryType.ToString();
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public bool DeleteSaveSearch(int selctedvalue)
        {
            bool res = false;
            string qry = "";
            qry = "Update  Resources_Save_Search set isVisible=0";
            qry = qry + Environment.NewLine + " where pKey = @PK";
            SqlCommand cmd = new SqlCommand(qry);
            cmd.Parameters.AddWithValue("@PK", selctedvalue);
            if (clsUtility.ExecuteQuery(cmd, null, "Filter Updated"))
            {
                res = true;
            }
            return res;
        }
        #endregion

        #region lcfGlossary



        public DataTable RefreshlcfGlossary(string ID, bool ckCurrentSrch, string prvSrchText, bool chkDefinition)
        {
            ID = ID.Replace("\"\"", "");
            string qry = "";

            qry = qry + Environment.NewLine + "select t1.pKey,  t1.definition ,t1.source  ";
            if (ID != "")
            {
                qry = qry + Environment.NewLine + " , [name],IIF(charindex(@txtC,'#'+name)>0,1,0) srt";
            }
            else
            {
                qry = qry + Environment.NewLine + " ,[name]";
            }


            qry = qry + Environment.NewLine + " From FCR_IcfGlossary t1";
            qry = qry + Environment.NewLine + " Where 1=1 and status=1";

            if (ID != "")
            {
                if (ckCurrentSrch)
                {
                    string[] dxt = prvSrchText.Split('/');
                    foreach (var d in dxt)
                    {
                        if (chkDefinition)
                        {
                            qry = qry + Environment.NewLine + " and (Contains(t1.Name, '\"" + d.Replace("'", "''") + "\"')  or Contains(t1.definition, '\"" + d.Replace("'", "''") + "\"'))";
                        }
                        else
                        {
                            qry = qry + Environment.NewLine + " and contains(t1.Name, '\"" + d.Replace("'", "''") + "\"')";
                        }
                    }
                }
                else
                {
                    if (chkDefinition)
                    {
                        qry = qry + Environment.NewLine + "and (contains(t1.name,@Txt) or contains(t1.definition,@Txt))";
                    }
                    else
                    {
                        qry = qry + Environment.NewLine + " and contains(t1.name,@Txt) ";
                    }

                }
            }
            else
            {
                qry = qry + Environment.NewLine + " and (t1.Name ='')";
            }

            if (ID != "")
            {
                qry = qry + Environment.NewLine + " order by srt desc,Name ";
            }
            else
            {
                qry = qry + Environment.NewLine + " order by Name";
            }

            SqlCommand cmd = new SqlCommand(qry);
            cmd.Parameters.AddWithValue("@txt", ID); //"\"\"" + ID+ "\"\""
            cmd.Parameters.AddWithValue("@txtC", ID);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;


        }
        #endregion

        #region Milestone

        public DataTable RefreshMileStone(string ID, int ddDateSelectedIndex, bool ckCurrentSrch, string prvSrchText)
        {


            string qry = "";
            qry = "select  case when datex like '-%' and Approx ='on' then '~'+stuff(datex, 1, 1, '') + ' ' + 'BCE' ";
            qry = qry + Environment.NewLine + " when datex like '-%' then stuff(datex, 1, 1, '') + ' ' + 'BCE' when Approx  ='on' then '~'+datex else datex end as datex ,datex ";
            if (ID != "")
            {
                qry = qry + Environment.NewLine + " , [Text]";

            }
            else
            {
                qry = qry + Environment.NewLine + " ,[Text]";
            }
            qry = qry + Environment.NewLine + " From FCR_Milestones t1";
            qry = qry + Environment.NewLine + " Where 1=1 and status=1";

            if (ID != "")
            {
                if (ckCurrentSrch)
                {
                    string[] dxt = prvSrchText.Split('/');
                    foreach (var d in dxt)
                    {
                        qry = qry + Environment.NewLine + " and contains(t1.Text , '\"\"" + d.Replace("'", "''") + "\"\"')";
                    }
                }
                else
                {
                    qry = qry + Environment.NewLine + " and contains(t1.Text,@Txt )";

                }
            }
            if (ddDateSelectedIndex > 0)
            {
                if (ddDateSelectedIndex == 1)
                {
                    qry = qry + Environment.NewLine + " and years < 1700";
                }
                else if (ddDateSelectedIndex == 2)
                {
                    qry = qry + Environment.NewLine + " and  years >=1700 and years < 1800 ";
                }
                else if (ddDateSelectedIndex == 3)
                {
                    qry = qry + Environment.NewLine + " and   years >=1800 and years < 1900 ";
                }
                else if (ddDateSelectedIndex == 4)
                {
                    qry = qry + Environment.NewLine + " and  years >=1900 and years < 2000 ";
                }
                else if (ddDateSelectedIndex == 5)
                {
                    qry = qry + Environment.NewLine + " and years > = 2000 ";
                }
            }
            qry = qry + Environment.NewLine + " order by years";

            SqlCommand cmd = new SqlCommand(qry);
            cmd.Parameters.AddWithValue("@txt", ID); //"\"\"" + ID+ "\"\""
            cmd.Parameters.AddWithValue("@txtC", ID);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;


        }

        public string MileStoneCount()
        {
            string count = "";
            string qry = "";
            qry = "select count(*) as Count from FCR_Milestones";
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                if (dt.Rows.Count > 0)
                {

                    count = (dt.Rows[0]["Count"].ToString());
                }
            }
            return count;
        }
        #endregion

        #region MAGI News


        public DataTable RefreshMagiNews(string txtGenSearch, string txtAuthors, string txtPublication, int ddTopic, int ddYear, int intAccount_PKey)
        {
            string qry = "";
            qry = qry + Environment.NewLine + "select t1.Title,t1.SourceName,t1.Publication,t2.Name as Topic,t1.Text as Description,t1.Url1 as Url ";
            qry = qry + Environment.NewLine + "from FCR_MagiNews t1";
            qry = qry + Environment.NewLine + "left outer join MAGINews_Topic t2 on t2.pkey=t1.Topic_pkey";
            qry = qry + Environment.NewLine + "where t1.broken=1 And t1.Status!=3 ";

            if (txtGenSearch != "")
            {
                qry = qry + Environment.NewLine + "and t1.Title like @Title";
            }
            if (txtAuthors != "")
            {
                qry = qry + Environment.NewLine + "and t1.SourceName like @Authors";
            }
            if (txtPublication != "")
            {
                qry = qry + Environment.NewLine + "and t1.publication like @publication";
            }
            if (ddTopic > 0)
            {
                qry = qry + Environment.NewLine + "and t2.pkey=@Topic";
            }
            if (ddYear > 0)
            {
                qry = qry + Environment.NewLine + "and datepart(yyyy,t1.DateCreated)=@Year";
            }
            SqlCommand cmd = new SqlCommand(qry);
            cmd.Parameters.AddWithValue("@Topic", ddTopic);
            cmd.Parameters.AddWithValue("@Year", ddYear);
            cmd.Parameters.AddWithValue("@Title", "%" + txtGenSearch + "%");
            cmd.Parameters.AddWithValue("@Authors", "%" + txtAuthors + "%");
            cmd.Parameters.AddWithValue("@publication", "%" + txtPublication + "%");
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                if (txtGenSearch != "")
                {
                    clsFCR.ResourcesSavedSearch(intAccount_PKey, clsUtility.Type_MagiNews, intAccount_PKey, txtGenSearch, (dt.Rows.Count > 0 ? 1 : 0));
                }
                return dt;
            }
            return dt;
        }

        public DataTable BindNewsTopic()
        {
            string qry = "";
            qry = qry + "select pkey,Name from MAGINews_Topic order by Name";
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }
        #endregion

        #endregion




        #region Event on Cloud


        public DataTable Refresh_EventOnCloud_PageText(int intEventPKey)
        {
            string qry = "";
            qry = qry + Environment.NewLine + "select t1.pKey, t1.SectionTitle,t1.Indicator, t1.SectionText, t1.Sequence, isNull(t1.Collapsible,0) as Collapsible, isNull(t1.DefaultCollapsed,0) as DefCollapsed";
            qry = qry + Environment.NewLine + " From Event_Text t1 ";
            qry = qry + Environment.NewLine + " Where t1.Event_pKey = " + intEventPKey.ToString();
            qry = qry + Environment.NewLine + " And t1.Active=1 And Indicator in(7,3)";
            qry = qry + Environment.NewLine + " Order by t1.Sequence";

            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;

        }

        public DataTable RefreshAudio()
        {
            string qry = "Select pKey,AudioPath From EventPages_Instruction where pKey in (4,8,11,12)";
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }



        #endregion

        #region Education Center 

        public DataTable Refresh_EducationCenter_PageText(int intEvent_pKey)
        {
            string qry = "";
            qry = qry + Environment.NewLine + "select t1.pKey, t1.SectionTitle, t1.SectionText, t1.Sequence, isNull(t1.Collapsible,0) as Collapsible, isNull(t1.DefaultCollapsed,0) as DefCollapsed";
            qry = qry + Environment.NewLine + ",(Case when isNull(t1.TitleURL,'') = '' Then Null else dbo.CleanURL(t1.TitleURL) End) as TitleLink";
            qry = qry + Environment.NewLine + ",(Case when iLeft.pkey is Null Then (select top 1 EmptyImage from Application_Defaults) else iLeft.ImageContent end) as ImageLeft, t1.ImageLeft_pKey";
            qry = qry + Environment.NewLine + ",(Case when iRight.pkey is Null Then (select top 1 EmptyImage from Application_Defaults) else iRight.ImageContent end) as ImageRight, t1.ImageRight_pKey";
            qry = qry + Environment.NewLine + ",(Case when iLeft.pkey is Null Then 0 Else 1 End) as LeftVisible";
            qry = qry + Environment.NewLine + ",(Case when iRight.pkey is Null Then 0 Else 1 End) as RightVisible";
            qry = qry + Environment.NewLine + " From Event_Text t1 ";
            qry = qry + Environment.NewLine + " Left outer join Event_Images t2 On t2.pkey = t1.ImageLeft_pkey";
            qry = qry + Environment.NewLine + " Left outer join Image_List iLeft On iLeft.pkey = t2.Image_pkey";
            qry = qry + Environment.NewLine + " Left outer join Event_Images t3 On t3.pkey = t1.ImageRight_pkey";
            qry = qry + Environment.NewLine + " Left outer join Image_List iRight On iRight.pkey = t3.Image_pkey";
            qry = qry + Environment.NewLine + " Where t1.Event_pKey =  " + intEvent_pKey.ToString();
            qry = qry + Environment.NewLine + " And t1.Active=1 And Indicator = 4";
            qry = qry + Environment.NewLine + " Order by t1.Sequence";

            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;


        }

        public DataTable RefreshEducationCenterAudio()
        {
            string qry = "Select pKey,AudioPath From EventPages_Instruction where pKey in(1,2,3)";
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public DataTable ViewVideo_EducationCenter(int intEvent_pKey)
        {
            string qry = "";
            qry = qry + Environment.NewLine + " select  pKey,VideoTitle,VideoUrl from Event_Recording";
            qry = qry + Environment.NewLine + " where Event_pkey=  " + intEvent_pKey.ToString();
            qry = qry + Environment.NewLine + " order by VideoTitle";
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }


        public DataTable RefreshSpeakers_EducationCenter(int intActiveEventPkey)
        {
            string qry = "Select distinct  t1.Account_pkey";
            qry = qry + Environment.NewLine + ", (isNull(t2.Firstname,'') + ' ' +  Case When len(t2.Nickname )>0 AND (Upper(t2.FirstName)<> UPPER(ISNULL(t2.NickName,''))) then";
            qry = qry + Environment.NewLine + " iif( ISNULL(t2.NickName,'')<>'',+'\"\"'+t2.NickName +'\"\"'+  ' ','')  Else '' END + isNull(t2.Lastname,'')) as Name";
            qry = qry + Environment.NewLine + ",t2.Firstname, t2.Email ,isNull(t2.Lastname,'') as Lastname,t5.OrganizationID";
            qry = qry + Environment.NewLine + ",isnull(t2.PersonalBio,'') as PersonalBio,isnull(t2.Title,'') as Title";
            qry = qry + Environment.NewLine + "From event_accounts t1";
            qry = qry + Environment.NewLine + "inner join account_List t2 On t2.pKey = t1.Account_pKey";

            qry = qry + Environment.NewLine + "inner join eventsession_staff t3 On t3.account_pkey = t1.account_pKey";
            qry = qry + Environment.NewLine + "inner join event_sessions t4 On t4.pkey = t3.eventsession_pkey And t4.event_pkey = t1.event_pkey";
            qry = qry + Environment.NewLine + "Left outer join Organization_list t5 On t5.pkey = t2.ParentOrganization_pKey";
            qry = qry + Environment.NewLine + "Left outer join SYS_OrganizationTypes t6 on t6.pkey=t5.OrganizationType_pkey";
            qry = qry + Environment.NewLine + " Where t3.IsSpeaker=1";
            qry = qry + Environment.NewLine + " AND  t1.Event_pKey=" + intActiveEventPkey.ToString();
            qry = qry + Environment.NewLine + " Order by Name";

            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        #endregion



        #region NetworkingLounge

        public DataTable RefreshText_NetworkingLounge(int intEvent_pKey)
        {
            string qry = "";
            qry = qry + Environment.NewLine + "select t1.pKey, t1.SectionTitle, t1.SectionText, t1.Sequence, isNull(t1.Collapsible,0) as Collapsible, isNull(t1.DefaultCollapsed,0) as DefCollapsed";
            qry = qry + Environment.NewLine + ",(Case when isNull(t1.TitleURL,'') = '' Then Null else dbo.CleanURL(t1.TitleURL) End) as TitleLink";
            qry = qry + Environment.NewLine + ",(Case when iLeft.pkey is Null Then (select top 1 EmptyImage from Application_Defaults) else iLeft.ImageContent end) as ImageLeft, t1.ImageLeft_pKey";
            qry = qry + Environment.NewLine + ",(Case when iRight.pkey is Null Then (select top 1 EmptyImage from Application_Defaults) else iRight.ImageContent end) as ImageRight, t1.ImageRight_pKey";
            qry = qry + Environment.NewLine + ",(Case when iLeft.pkey is Null Then 0 Else 1 End) as LeftVisible";
            qry = qry + Environment.NewLine + ",(Case when iRight.pkey is Null Then 0 Else 1 End) as RightVisible";
            qry = qry + Environment.NewLine + " From Event_Text t1 ";
            qry = qry + Environment.NewLine + " Left outer join Event_Images t2 On t2.pkey = t1.ImageLeft_pkey";
            qry = qry + Environment.NewLine + " Left outer join Image_List iLeft On iLeft.pkey = t2.Image_pkey";
            qry = qry + Environment.NewLine + " Left outer join Event_Images t3 On t3.pkey = t1.ImageRight_pkey";
            qry = qry + Environment.NewLine + " Left outer join Image_List iRight On iRight.pkey = t3.Image_pkey";
            qry = qry + Environment.NewLine + " Where t1.Event_pKey = " + intEvent_pKey.ToString();
            qry = qry + Environment.NewLine + " And t1.Active=1 And Indicator = 5";
            qry = qry + Environment.NewLine + " Order by t1.Sequence";

            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            else
            {
                clsUtility.LogErrorMessage(null, null, "NetworkingLounge", 0, "Error accessing text block data.");
            }
            return dt;

        }


        public DataTable RefreshAudio_NetworkingLounge()
        {
            string qry = "Select AudioPath From EventPages_Instruction where pKey in(5,6,7)";
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }


        #endregion

        #region ScheduledEvent


        public int FindEvent_Org(int intAccount_PKey, int intActiveEventPkey)
        {
            string intExhibitor_pKey = "0";
            string qry = "";
            qry = qry + Environment.NewLine + "select  t3.pkey From   account_list t2 ";
            qry = qry + Environment.NewLine + " INNER JOIN Organization_List t1 ON t1.pkey=t2.ParentOrganization_pKey Inner Join Event_Organizations t3 on t3.Organization_pKey = t1.pkey";
            qry = qry + Environment.NewLine + "  where   t2.pkey=" + intAccount_PKey.ToString() + " and t3.Event_pKey=" + intActiveEventPkey.ToString();
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                if (dt.Rows.Count > 0)
                {
                    intExhibitor_pKey = dt.Rows[0]["pkey"].ToString();
                }
            }
            return Convert.ToInt32(intExhibitor_pKey);
        }

        public DataTable RefreshScheduledBooth(int intExhibitor_pKey, int intAccount_PKey, int intActiveEventPkey, DateTime ddDate2_SelectedValue, DateTime dtCurEventStart, DateTime dtCurEventEnd)
        {

            if (con.State == ConnectionState.Closed)
                con.Open();
            SqlCommand cmd = new SqlCommand("Schedule_event_select_ALL", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 60;

            cmd.Parameters.AddWithValue("@Exhibitor_pKey", intExhibitor_pKey);
            cmd.Parameters.AddWithValue("@Account_pkey", intAccount_PKey);
            cmd.Parameters.AddWithValue("@Event_pkey", intActiveEventPkey);
            cmd.Parameters.AddWithValue("@EvantTime", clsEvent.getEventVenueTime());
            cmd.Parameters.AddWithValue("@Date", ddDate2_SelectedValue);

            cmd.Parameters.AddWithValue("@dtStartDate", dtCurEventStart);
            cmd.Parameters.AddWithValue("@dtEndDate", dtCurEventEnd);

            DataTable dt = new DataTable();
            return dt;
        }

        #endregion


        #region ResourceSuppportCenter

        public DataTable RefreshText_ResourceSupportCenter(int intEvent_pKey)
        {
            string qry = "";
            qry = qry + Environment.NewLine + "select t1.pKey, t1.SectionTitle, t1.SectionText, t1.Sequence, isNull(t1.Collapsible,0) as Collapsible, isNull(t1.DefaultCollapsed,0) as DefCollapsed";
            qry = qry + Environment.NewLine + ",(Case when isNull(t1.TitleURL,'') = '' Then Null else dbo.CleanURL(t1.TitleURL) End) as TitleLink";
            qry = qry + Environment.NewLine + ",(Case when iLeft.pkey is Null Then (select top 1 EmptyImage from Application_Defaults) else iLeft.ImageContent end) as ImageLeft, t1.ImageLeft_pKey";
            qry = qry + Environment.NewLine + ",(Case when iRight.pkey is Null Then (select top 1 EmptyImage from Application_Defaults) else iRight.ImageContent end) as ImageRight, t1.ImageRight_pKey";
            qry = qry + Environment.NewLine + ",(Case when iLeft.pkey is Null Then 0 Else 1 End) as LeftVisible";
            qry = qry + Environment.NewLine + ",(Case when iRight.pkey is Null Then 0 Else 1 End) as RightVisible";
            qry = qry + Environment.NewLine + " From Event_Text t1 ";
            qry = qry + Environment.NewLine + " Left outer join Event_Images t2 On t2.pkey = t1.ImageLeft_pkey";
            qry = qry + Environment.NewLine + " Left outer join Image_List iLeft On iLeft.pkey = t2.Image_pkey";
            qry = qry + Environment.NewLine + " Left outer join Event_Images t3 On t3.pkey = t1.ImageRight_pkey";
            qry = qry + Environment.NewLine + " Left outer join Image_List iRight On iRight.pkey = t3.Image_pkey";
            qry = qry + Environment.NewLine + " Where t1.Event_pKey = " + intEvent_pKey.ToString();
            qry = qry + Environment.NewLine + " And t1.Active=1 And Indicator = 6";
            qry = qry + Environment.NewLine + " Order by t1.Sequence";


            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            else
            {
                clsUtility.LogErrorMessage(null, null, "ResourceSupportCenter", 0, "Error accessing text block data.");
            }
            return dt;
        }
        public DataTable RefreshAudio_ResourceSupportCenter()
        {
            string qry = "Select pkey,AudioPath From EventPages_Instruction Where Pkey IN (8,9,10)";
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public DataTable RefreshDocuments(bool bGlobalAdministrator, int intEvent_pKey, int intAccount_PKey, string txtStringTitle)
        {
            string GogbalAdmin = (bGlobalAdministrator ? "1" : "0");
            string qry = "";
            qry = qry + Environment.NewLine + " DECLARE @IsSpeaker bit =0,@IsEventSponsor bit=0,@IsStaff bit=0,@IsGlobalAdmin bit = " + GogbalAdmin + ";";
            qry = qry + Environment.NewLine + " SELECT Distinct @IsSpeaker = 1  From event_accounts t1 inner join eventsession_staff t3 On t3.account_pkey = t1.account_pKey";
            qry = qry + Environment.NewLine + " inner join event_sessions t4 On t4.pkey = t3.eventsession_pkey And t4.event_pkey = t1.event_pkey";
            qry = qry + Environment.NewLine + " Left outer join Speaker_Issues t14 on t14.EventAccount_pKey = t1.pKey";
            qry = qry + Environment.NewLine + " Where t1.Event_pKey = @Event_pKey  AND t1.Account_pKey = @Account_pKey AND (t3.IsSessionLeader=1 or t3.IsSessionLeader2 = 1 or t3.IsSpeaker=1 Or t3.IsSessionChair=1 Or t3.IsSessionModerator=1 or t3.IsSessionModerator2=1)";
            qry = qry + Environment.NewLine + " SELECT TOP 1 @IsStaff = 1 From Event_Staff  Where Event_Staff.Event_pKey =@Event_pKey AND Event_Staff.Account_pKey = @Account_pKey";
            qry = qry + Environment.NewLine + " SELECT @IsEventSponsor = IIF(t4.pKey IS NOT NULL,1,0) from Organization_List t1 Inner Join Event_Organizations t2 On t2.Organization_pkey = t1.pkey";
            qry = qry + Environment.NewLine + " LEFT OUTER JOIN Sys_ParticipationTypes t3 On t3.pkey = t2.ParticipationType_pKey";
            qry = qry + Environment.NewLine + " INNER JOIN Account_List t4 ON t4.ParentOrganization_pKey = t2.Organization_pKey";
            qry = qry + Environment.NewLine + " Where t2.ShowOnEventPartner=1 and isnull(t2.ParticipationStatus_pKey,0)<>2 and t2.Event_pKey = @Event_pKey And t4.pKey =@Account_pKey Order by t1.OrganizationID";
            qry = qry + Environment.NewLine + " Select DISTINCT t0.[FileName] As DisplayName,t0.FileURL As DocumentLink,FileType As [Type],t0.AudienceProperty,t0.SortOrder From Training_Resources t0";
            qry = qry + Environment.NewLine + " Where t0.Event_Pkey = @Event_pkey And ISNULL(t0.IsActive, 1) = 1 AND ISNULL(t0.IsMyConsoleOnly,0) = 0  AND (t0.FileName Like '%' + @Title + '%' OR @Title IS NULL)  AND  t0.PageArea NOT IN (34,35,36,37,38,39,40,41,42,43) AND";
            qry = qry + Environment.NewLine + " ((@IsGlobalAdmin = 1 OR @IsStaff = 1)  OR ('Public' IN (SELECT value From string_split(IIF(t0.AudienceProperty IS NULL OR t0.AudienceProperty='','Public',t0.AudienceProperty),','))) OR  ('Attendees' IN (SELECT value From string_split(IIF(t0.AudienceProperty IS NULL OR t0.AudienceProperty='','Attendees',t0.AudienceProperty),',')) OR (Case When @IsEventSponsor =1 then  'Event Sponsors' END IN (SELECT value From string_split(t0.AudienceProperty,','))) OR (Case When @IsSpeaker =1 then  'Speakers & Session Chairs' END IN (SELECT value From string_split(t0.AudienceProperty,','))) OR (Case When @IsStaff =1 then 'Staff' END IN (SELECT value From string_split(t0.AudienceProperty,',')))))";
            qry = qry + Environment.NewLine + " Order By t0.FileName ASC"; ;

            SqlCommand cmd = new SqlCommand(qry);
            cmd.Parameters.AddWithValue("@Event_pkey", intEvent_pKey.ToString());
            cmd.Parameters.AddWithValue("@Account_pKey", intAccount_PKey.ToString());
            cmd.Parameters.AddWithValue("@Title", txtStringTitle.Trim());

            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }


        #endregion



        public DataTable RefreshEvent(int intActiveEventPkey)
        {
            string qry = "select isnull(ShowEventPages,0) as ShowEventPages, isnull(PartnerAlias,'Partner') as PartnerAlias from Event_List  where pkey=" + intActiveEventPkey.ToString();

            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public List<SelectListItem> EventPagesDropDown()
        {
            List<SelectListItem> dropdown = new List<SelectListItem>();

            dropdown.Add(new SelectListItem { Text = "Overview", Value = "0" });
            dropdown.Add(new SelectListItem { Text = "Program/Agenda", Value = "1" });
            dropdown.Add(new SelectListItem { Text = "Speakers", Value = "2" });
            dropdown.Add(new SelectListItem { Text = "Become a Speaker", Value = "3" });
            dropdown.Add(new SelectListItem { Text = "Partners", Value = "4" });
            dropdown.Add(new SelectListItem { Text = "Become a Partner", Value = "5" });
            dropdown.Add(new SelectListItem { Text = "Pricing & Registration", Value = "7" });
            dropdown.Add(new SelectListItem { Text = "Contact Us", Value = "8" });
            return dropdown;
        }
        public DataTable RefreshEventInstructionAudio(string IDs)
        {
            string qry = "Select pKey,AudioPath From EventPages_Instruction where pKey in(" + IDs + ")";
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }

        public DataTable TaskCategories_List()
        {
            DataTable dt = new DataTable();
            try
            {
                string dbquery = "EXEC TaskCategories_List";
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter(dbquery, con);
                sda.Fill(dt);
                con.Close();
            }
            catch
            {
                throw;
            }
            finally
            {
                con.Close();
            }
            return dt;
        }


        public async Task<List<TaskListRequest>> TaskList_Select_All(TaskListRequest request)
        {
            Boolean active = false;
            Boolean reviewed = false;
            if (request.active != "")
            {
                active = Convert.ToBoolean(request.active);
            }
            if (request.reviewed != "")
            {
                reviewed = Convert.ToBoolean(request.reviewed);

            }
            SqlParameter[] parameters = new SqlParameter[]
            {

            new SqlParameter("@PlanDates", request.plandates),
            new SqlParameter("@DueDate", request.duedate),
            new SqlParameter("@Forecast", request.forecast),
            new SqlParameter("@Number", request.number),
            new SqlParameter("@Title", request.title),
            new SqlParameter("@Status", Convert.ToInt32(request.status)),
            new SqlParameter("@TaskCategory_pKey", Convert.ToInt32(request.category)),
            new SqlParameter("@Active", active),
                  new SqlParameter("@Reviewed", reviewed),
            new SqlParameter("@TaskListRange", request.tasklistrange),
            new SqlParameter("@RepeatType_pKey", int.Parse(request.repeat)),
            new SqlParameter("@pKey", request.pKey)

            };


            List<TaskListRequest> list = await SqlHelper.ExecuteListAsync<TaskListRequest>("SP_TaskList", CommandType.StoredProcedure, parameters);
            return list;
        }
        public DataTable TaskList_Select_All1(TaskListRequest request)
        {
            int status = request.status != null ? Convert.ToInt32(request.status) : 0;
          
            SqlCommand cmd = new SqlCommand("TaskList_All", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 30;
            cmd.Parameters.AddWithValue("@planType", request.planType);
            cmd.Parameters.AddWithValue("@DueDate", request.duedate);
            cmd.Parameters.AddWithValue("@Forecast", request.forecast);
            cmd.Parameters.AddWithValue("@Number", request.number);
            cmd.Parameters.AddWithValue("@Title", request.title != null ? request.title : "");
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@TaskCategory_pKey", (request.intcategory));
            if (request.active != null)
            {
                if (Convert.ToBoolean(request.active) == true)
                {
                    cmd.Parameters.AddWithValue("@Active", 1);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Active", 0);
                }
            }
            if (request.reviewed != null)
            {
                if (Convert.ToBoolean(request.reviewed) == true)
                {
                    cmd.Parameters.AddWithValue("@Reviewed", 1);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Reviewed", 0);
                }
                
            }
            cmd.Parameters.AddWithValue("@TaskListRange", request.tasklistrange);
            cmd.Parameters.AddWithValue("@RepeatType_pKey", (request.intRepeat));
            cmd.Parameters.AddWithValue("@pKey", request.pKey);
            DataTable dt = new DataTable();

            if (clsUtility.GetDataTableStored(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;

        }



        public DataTable TaskRepeat_Select_ALL()
        {
            DataTable dt = new DataTable();
            try
            {
                string dbquery = "EXEC TaskRepeat_Select_ALL";
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter(dbquery, con);
                sda.Fill(dt);
                con.Close();
            }
            catch
            {
                throw;
            }
            finally
            {
                con.Close();
            }
            return dt;
        }
	public DataTable PortoloTaskRepeat_Select_ALL()
        {
            DataTable dt = new DataTable();
            try
            {
                string dbquery = "EXEC PortoloTaskRepeat_Select_ALL";
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter(dbquery, con);
                sda.Fill(dt);
                con.Close();
            }
            catch
            {
                throw;
            }
            finally
            {
                con.Close();
            }
            return dt;
        }											   
        public DataTable TaskStatuses_Select_All()
        {
            DataTable dt = new DataTable();
            try
            {
                string dbquery = "EXEC TaskStatuses_Select_All";
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter(dbquery, con);
                sda.Fill(dt);
                con.Close();
            }
            catch
            {
                throw;
            }
            finally
            {
                con.Close();
            }
            return dt;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public string Tasks_Detele(string strpkey)
        {
            string qry = "EXEC PublicTasks_Detele @strpkey";
            SqlCommand cmd = new SqlCommand(qry);
            cmd.CommandType = CommandType.Text;
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("@strpkey", strpkey);
            if (!clsUtility.ExecuteQuery(cmd, null, "Delete Task"))
            {
                return "Task Not Deleted";
            }
            else
            {
                return "Task Delete";
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public string CopyTask(string strpkey)
        {
            string qry = "EXEC SP_CopyTask @strpkey";
            SqlCommand cmd = new SqlCommand(qry);
            cmd.CommandType = CommandType.Text;
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("@strpkey", strpkey);
            if (!clsUtility.ExecuteQuery(cmd, null, "Delete Task"))
            {
                return "Task Not Copied ";
            }
            else
            {
                return "Task Copied sucessfully";
            }
        }


        public DataTable TaskEdit(int pkey)
        {
            SqlCommand cmd = new SqlCommand("SP_TaskList", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 30;
          
            cmd.Parameters.AddWithValue("@pKey", pkey);
            DataTable dt = new DataTable();

            if (clsUtility.GetDataTableStored(con, cmd, ref dt))
            {
                return dt;
            }
            return dt;
        }
    }
    public class EventsInfo
    {
        public string SectionTitle { get; set; }
        public string SectionText { get; set; }
    }

    public class ReportIssue
    {
        public int intIssue_PKey { get; set; }
        public string Issuetitle { get; set; }

        public string Issuelocation { get; set; }
        public string IssueType { get; set; }
        public string IssueTypeName { get; set; }

        public string IssueReportedbyUser { get; set; }

        public string UserEmail { get; set; }

        public string IssueDetail { get; set; }
        public string UID { get; set; }
        public string strIssueFiles { get; set; }
        public Byte[] Screenshot { get; set; }
        public HttpPostedFileBase files { get; set; }
    }
    public class VenueInfo
    {
        public string SectionTitle { get; set; }
        public string SectionText { get; set; }
    }

    //public class UploadedFiles
    //{
    //    public UploadedFiles()
    //    {
    //        httpPostedFiles = new List<HttpPostedFile>();
    //    }
    //    public string FileGUID { get; set; }
    //    public List<HttpPostedFile> httpPostedFiles { get; set; }
    //}
    public class ParrticipatingOrganisation
    {
        public string OrganizationName { get; set; }
        public string OrgType { get; set; }
        public string SiteOrg { get; set; }


    }
    public class Speakers
    {
        public string Sid { get; set; }
        public int Session_pKey { get; set; }
        public int S_pKey { get; set; }
        public string SessionId { get; set; }
        public string SpeakerName { get; set; }
        public string SpeakerDegree { get; set; }
        public string SpeakerTitle { get; set; }
        public string SpeakerOrganization { get; set; }
        public string SessionTitle { get; set; }
        public string SessionDescription { get; set; }
        public string SpeakerRole { get; set; }
        public string SpeakerUserRole { get; set; }
        public bool ShowStar { get; set; }

    }

    public class ContactMAGI
    {
        public int ID { get; set; }

        public string contactRolepKey { get; set; }
        public bool ImageExist { get; set; }
        public string ImagePath { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public Byte[] ProfilePicture { get; set; }
        public string ToolTip { get; set; }
    }

    public class Programs
    {
        public int Track_pKey { get; set; }
        public string SessionId { get; set; }
        public string Session_Pkey { get; set; }
        public string EventDate { get; set; }
        public string Topic { get; set; }
        public string TimeDuration { get; set; }
        public string Title { get; set; }
        public string SpeakerHtml { get; set; }
        public List<Speakers> Speakers { get; set; }
        public string Description { get; set; }
        public string TrackBG { get; set; }
        public string Edulabel { get; set; }
        public string ReletedSession { get; set; }
        public int Audience_pKey { get; set; }
        public string Topics { get; set; }
        public bool TableHeading { get; set; }
        public bool Cancelled { get; set; }
        public string LiveStream { get; set; }
        public string RecordingLink { get; set; }
        public bool bLiveStream { get; set; }
        public List<SelectListItem> TrackList { get; set; }
        public List<Session_Link> RelatedSessionLink { get; set; }
        public bool bNewImg { get; set; }
    }
    public class SelectedSessionDetails
    {

        public string SessionId { get; set; }
        public string Session_Pkey { get; set; }
        public string EventDate { get; set; }

        public string TimeDuration { get; set; }
        public string Title { get; set; }
        public List<Speakers> Speakers { get; set; }
        public string Description { get; set; }

        public string ReletedSession { get; set; }

        public string Topics { get; set; }

        public List<Session_Link> RelatedSessionLink { get; set; }
    }

    public class Session_Link
    {
        public string _SessionId { get; set; }
        public string _SessionLink { get; set; }
        public string _SessionText { get; set; }

        public string _EventSession_pkey { get; set; }
        public Session_Link(string sid, string slink, string stext, string e_s_pkey) => (_SessionId, _SessionLink, _SessionText, _EventSession_pkey) = (sid, slink, stext, e_s_pkey);
    }

    public class MAGINews
    {
        public string Title { get; set; }
        public string Topic { get; set; }

        public string Author { get; set; }

        public string Publication { get; set; }

        public string URL { get; set; }

    }

    public class Milestone
    {
        public string Years { get; set; }
        public string Title { get; set; }
    }

    public class Advisory
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Title { get; set; }
        public string Orginization { get; set; }


    }

    public class Testimonials
    {

        public string Comment { get; set; }
        public string Role { get; set; }


    }

    public class UpcomingEvents
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Venue { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Place { get; set; }
    }

    public class EventSponsors
    {
        public string Level { get; set; }
        public string Profile { get; set; }
        public string ImgLogo { get; set; }
        public string URL { get; set; }
        public int ipk { get; set; }
        public string ParticipationLevel_pkey { get; set; }

        public bool ImageExist { get; set; }
        public string imgpath { get; set; }

    }

    public class My_History
    {
        public string UpdatedOn { get; set; }
        public string Page { get; set; }
        public string Action { get; set; }

        public string ByName { get; set; }


    }

    public class My_Certificates
    {
        public int pkey { get; set; }
        public string Name { get; set; }
        public string CertFileName { get; set; }
        public string Account_pkey { get; set; }
        public string Event { get; set; }
        public string Date { get; set; }
        public string DownloadCertificate { get; set; }
        public string DownloadSchedule { get; set; }
        public double Hours { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public string EventID { get; set; }
        public string CertPKEy { get; set; }
        public string EventPKey { get; set; }
        public string Editable { get; set; }
        public string ISUpdatedByUSer { get; set; }
        public string Type { get; set; }



    }

    public class My_Payments
    {
        public string Item { get; set; }
        public string Date { get; set; }
        public string Event { get; set; }
        public int EventPkey { get; set; }
        public string Transaction { get; set; }
        public string Memo { get; set; }
        public string GroupCode { get; set; }
        public int LoggedByID { get; set; }
        public string LoggedByName { get; set; }
        public double Payment { get; set; }
        public string Document { get; set; }
        public string Status { get; set; }
        public double Charge { get; set; }
        public double Balance { get; set; }
        public double rowAmt { get; set; }
    }


    public class EvtSummaryPartner
    {
        public string lblMainTitle { get; set; }

        public string lbltext { get; set; }

        public string lblPartnerTitle { get; set; }
        public string btnPartnerMInstruct { get; set; }

        public string btnPtExh { get; set; }
        public string btnPTypeGLines { get; set; }

        public bool btnInstructPartnerVisible { get; set; }
        public bool lblPartnerTitleVisible { get; set; }

        public int hdnParticipationType { get; set; }
    }
    public class MyPaymentPage
    {
        public IEnumerable<My_Payments> PaymentsTable { get; set; }

        public DataTable VoucherTable { get; set; }

        public DataTable OtherReciptTable { get; set; }
    }

    public class MyConferenceBookPage
    {
        public DataTable SessionBookTable { get; set; }
        public DataTable SessionBookTableNonAttend { get; set; }
        //public ChatViewModel chatViewModel { get; set; }
    }

    public class MyGroupChat
    {
        public DataTable GroupChatList { get; set; }
    }

    public class MyConference
    {
        //public ChatViewModel chatViewModel { get; set; }

        public DataTable tblRecipts { get; set; }

        public DataTable tblSessions { get; set; }

        public DataTable tblChairs { get; set; }

        public DataTable tblSpecialEvent { get; set; }
    }


    public class MyOptions
    {
        //public ChatViewModel chatViewModel { get; set; }
        public HelpIconData HelpIconInfo { get; set; }
        public DataTable ddEventVirtualData { get; set; }
        public DataTable tblSessionInfo { get; set; }
        public DataTable tblReceipt { get; set; }
        public DataTable tblOtherLinks { get; set; }
        public DataTable tblEdOptions { get; set; }
        public DataTable tblOffers { get; set; }
    }

    public class MyStaffConsole
    {
        public DataTable RecentLinks { get; set; }

    }

    public class ChartItems
    {

        public string Xaxis { get; set; }
        public int Yaxis { get; set; }
        public string Tooltip { get; set; }

    }

    public class StandardItems
    {

        public int Pkey { get; set; }
        public bool IsChecked { get; set; }
        public string FileName { get; set; }

    }
    public class EventOnCloud
    {
        //public ChatViewModel chatViewModel { get; set; }
        public DataTable ddEventVirtualData { get; set; }
        public HelpIconData HelpIconInfo { get; set; }
    }
    public class EducationCenter
    {
        //public ChatViewModel chatViewModel { get; set; }
        public DataTable ddEventVirtualData { get; set; }
        public HelpIconData HelpIconInfo { get; set; }
    }

    public class Event_PagesModel
    {
        //public ChatViewModel chatViewModel { get; set; }
        public HelpIconData HelpIconInfo { get; set; }
        public DataTable ddEventVirtualData { get; set; }
    }
}
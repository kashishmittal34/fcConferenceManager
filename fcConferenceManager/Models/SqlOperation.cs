using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using MAGI_API.Security;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using System.Net;
using System.Linq;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;
using fcConferenceManager;
using System.Globalization;
using fcConferenceManager.Models;

using System.Reflection;

namespace MAGI_API.Models
{
    public class SqlOperation
    {
        public async Task<bool> IsAllUrlVerify(string strValidateUrl)
        {
            bool isValid = true;
            try
            {
                if (!strValidateUrl.Contains("http://") & !strValidateUrl.Contains("https://"))
                    strValidateUrl = "http://" + strValidateUrl;
                // strValidateUrl = strValidateUrl.Replace("!", "")
                Uri myUri = new Uri(strValidateUrl);
                string host = myUri.Host;
                if (!host.Contains("www"))
                    host = "www." + host;
                IPAddress[] ip_Addresses = Dns.GetHostAddresses(host);
                if (ip_Addresses.Length > 0)
                    isValid = true;
            }
            catch (Exception ex)
            {
                try
                {
                    WebClient ip = new WebClient();
                    var tststrng = ip.DownloadString(strValidateUrl);
                    if (tststrng != null/* TODO Change to default(_) if this is not a reference type */ )
                        isValid = true;
                }
                catch
                {
                    try
                    {
                        Uri myUri = new Uri(strValidateUrl);
                        string host = myUri.Host;
                        if (host.Contains("www"))
                            host = host.Replace("www.", "");
                        IPAddress[] ip_Addresses = Dns.GetHostAddresses(host);
                        if (ip_Addresses.Length > 0)
                            isValid = true;
                    }
                    catch
                    {
                        isValid = false;
                    }
                }
            }
            return await Task.FromResult(isValid);
        }

        public async Task<int> PartnerStatus_Update(PartnerEventStatus Call)
        {
            SqlParameter[] parameters = new SqlParameter[]
           {
                new SqlParameter("@partner_pkey", Call.partner_pkey),
                new SqlParameter("@Status", Call.Status),


           };
            bool isUpdate = await SqlHelper.ExecuteNonQueryAsync("PartnerStatus_Update", CommandType.StoredProcedure, parameters);
            if (isUpdate)
                return 1;
            else
                return 0;
        }
        public async Task<string> Call_Recording_Save(CallRecording Call)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_pkey", Call.Account_pkey),
                new SqlParameter("@Event_pkey", Call.Event_pkey),
                new SqlParameter("@Addedby", Call.Addedby),
                new SqlParameter("@CallType", Call.CallType),
                new SqlParameter("@PhoneNo", Call.PhoneNo)
            };
            bool isInsert = await SqlHelper.ExecuteNonQueryAsync("Call_Recording_Save", CommandType.StoredProcedure, parameters);
            if (isInsert)
                return "New call recording added";
            else
                return "Call recording not added";
        }

        public async Task<string> JournalSubscriptionSave(string Account_pkey, int status)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_pkey", Account_pkey),
                new SqlParameter("@Status", Convert.ToBoolean( status))
            };
            bool isInsert = await SqlHelper.ExecuteNonQueryAsync("JournalSubscriptionSave", CommandType.StoredProcedure, parameters);
            if (isInsert)
                return "Journal Subscription Updated";
            else
                return "Journal Subscription Not Updated";
        }

        public async Task<string> Temp_log_save(CallRecording Call)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_pkey", Call.Account_pkey),
                new SqlParameter("@Event_pkey", Call.Event_pkey),
                new SqlParameter("@Addedby", Call.Addedby),
                new SqlParameter("@Type",0)
            };
            bool isInsert = await SqlHelper.ExecuteNonQueryAsync("Temp_log_save", CommandType.StoredProcedure, parameters);
            if (isInsert)
                return "New Temp Record Save";
            else
                return "Temp Record Not Save";
        }

        public async Task<int> Call_IssueList_Add(IssueItem issueItem)
        {
            DateTime dtEnteredOn = DateTime.MinValue;
            bool checker = DateTime.TryParseExact(issueItem.ItemDate + " " + issueItem.ItemTime, "MM/dd/yyyy HH:mm", null, DateTimeStyles.None, out dtEnteredOn);
            if (checker)
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Issue_pKey", issueItem.Issue_pKey),
                    new SqlParameter("@Account_pKey", issueItem.Account_pKey),
                    new SqlParameter("@Developer_pKey", issueItem.IssueDeveloper_pKey),
                    new SqlParameter("@Item_Title", issueItem.ItemTitle ?? ""),
                    new SqlParameter("@EnteredOn", dtEnteredOn.AddMinutes(-330)),
                    new SqlParameter("@Status_pKey", issueItem.IssueStatus_pKey),
                    new SqlParameter("@Duration", issueItem.Duration),
                    new SqlParameter("@Result", 0)
                };
                parameters[7].Direction = ParameterDirection.Output;
                bool isInsert = await SqlHelper.ExecuteNonQueryAsync("Call_IssueItem_Add", CommandType.StoredProcedure, parameters);
                return Convert.ToInt32(parameters[7].Value);
            }
            else
            {
                return -2;
            }
        }

        public async Task<int> Call_IssueList_Save(IssueItem issueItem)
        {
            if (issueItem.Issue_pKey > 0)
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                     new SqlParameter("@Issue_pKey", issueItem.Issue_pKey),
                     new SqlParameter("@Account_pKey", issueItem.Account_pKey),
                     new SqlParameter("@Developer_pKey", issueItem.IssueDeveloper_pKey),
                     new SqlParameter("@Status_pKey", issueItem.IssueStatus_pKey),
                     new SqlParameter("@InProgress", issueItem.InProgress),
                     new SqlParameter("@Result", 0)
                };
                parameters[5].Direction = ParameterDirection.Output;
                bool isInsert = await SqlHelper.ExecuteNonQueryAsync("Call_IssueItem_Save", CommandType.StoredProcedure, parameters);
                return Convert.ToInt32(parameters[5].Value);
            }
            else
            {
                return -2;
            }
        }

        public async Task<int> Call_IssueList_Update(IssueItem issueItem)
        {
            DateTime dtEnteredOn = DateTime.MinValue;
            bool checker = DateTime.TryParseExact(issueItem.ItemDate + " " + issueItem.ItemTime, "MM/dd/yyyy HH:mm", null, DateTimeStyles.None, out dtEnteredOn);
            if (checker)
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Issue_pKey", issueItem.Issue_pKey),
                    new SqlParameter("@EnteredOn", dtEnteredOn.AddMinutes(-330)),
                    new SqlParameter("@Duration", issueItem.Duration),
                    new SqlParameter("@Result", 0)
                };
                parameters[3].Direction = ParameterDirection.Output;
                bool isInsert = await SqlHelper.ExecuteNonQueryAsync("Call_IssueItem_Update", CommandType.StoredProcedure, parameters);
                return Convert.ToInt32(parameters[3].Value);
            }
            else
            {
                return -2;
            }
        }

        public async Task<List<IssueItem_List>> Issue_item_select(IssueItem Call)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_pkey", Call.Account_pKey),
                new SqlParameter("@Developer_pkey", Call.IssueDeveloper_pKey)
            };
            List<IssueItem_List> list = await SqlHelper.ExecuteListAsync<IssueItem_List>("Issue_item_select", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<IssueItem_List>> Issue_page_select(IssueItem Call)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_pkey", Call.Account_pKey),
                new SqlParameter("@Developer_pkey", Call.IssueDeveloper_pKey)
            };
            List<IssueItem_List> list = await SqlHelper.ExecuteListAsync<IssueItem_List>("Issue_page_select", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<IssueItem_List>> Issueitem_select(IssueItem Call)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_pkey", Call.Account_pKey),
                new SqlParameter("@Developer_pkey", Call.IssueDeveloper_pKey)
            };
            List<IssueItem_List> list = await SqlHelper.ExecuteListAsync<IssueItem_List>("Issueitem_select", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<Call_List>> Call_recording_select(CallRecording Call)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_pkey", Call.Account_pkey),
                new SqlParameter("@Event_pkey", Call.Event_pkey)
            };
            List<Call_List> list = await SqlHelper.ExecuteListAsync<Call_List>("Call_recording_select", CommandType.StoredProcedure, parameters);
            return list;
        }


        public async Task<List<Exibitor>> VirtualExibitorList(CallRecording Call)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                //new SqlParameter("@Account_pkey", Call.Account_pkey),
                new SqlParameter("@Event_pkey", Call.Event_pkey)
            };
            List<Exibitor> list = await SqlHelper.ExecuteListAsync<Exibitor>("VirtualExibitor", CommandType.StoredProcedure, parameters);
            return list;
        }
        public static string GetMD5Hash(string theInput)
        {
            StringBuilder sBuilder = new StringBuilder();
            using (MD5 hasher = MD5.Create())
            {
                byte[] dbytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(theInput));
                for (int n = 0; n <= dbytes.Length - 1; n++)
                    sBuilder.Append(dbytes[n].ToString("X2"));
            }
            return sBuilder.ToString();
        }

        public string EncryptMD5(string strPlain)
        {
            string strPassword = "";
            for (int i = 0; i <= 9; i++)
            {
                Random rnd = new Random();
                int ofs = rnd.Next(0, 2147483647);
                strPassword = strPassword + ofs.ToString();
            }
            string strSalt = GetMD5Hash(strPassword).Substring(0, 2);
            strPassword = GetMD5Hash(strSalt + strPlain) + ":" + strSalt;
            return strPassword;
        }
        public bool Validate_Password(string strplain, string strencrypted)
        {
            string[] arr = strencrypted.Split(':');
            if (arr.Length != 2)
                return false;
            string strSalt = arr[1];
            string strCalculated = GetMD5Hash(strSalt + strplain);
            return (strCalculated.ToUpper() == arr[0].ToUpper());
        }

        public async Task<IdentityUser> GetUserbyNameAndPassword(string UserName, string password)
        {
            try
            {
                var userData = new IdentityUser();
                SqlParameter[] parameters = new SqlParameter[]
                {
                       new SqlParameter("@UserName", UserName.Trim())
                };
                //string strSql = "Select a.*,s.SettingValue,e.EventFullName,e.EventType_pkey, ISNULL(t1.pkey,0) as EventAccount_pkey ,ISNULL(t1.ParticipationStatus_pKey,0) as ParticipationStatus_pKey from account_list a join Application_Settings s on s.pkey=187 " +
                //                "join Event_List e on e.pKey=s.SettingValue LEFT OUTER JOIN Event_Accounts t1 ON t1.Account_pKey=a.pKey and t1.Event_pKey=s.SettingValue where a.UL=@UserName or a.Email=@UserName";
                string strSql = " EXEC API_GetUserbyNameAndPassword  @UserName";
                var dt = await SqlHelper.ExecuteTableAsync(strSql, CommandType.Text, parameters);
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    bool val = Validate_Password(password, dr["UP"].ToString());
                    if (val == true)
                    {
                        userData.Id = Convert.ToInt32(dr["pKey"]);
                        userData.CustomerId = Convert.ToString(dr["pKey"]);
                        userData.UserId = Convert.ToString(dr["UL"]);
                        userData.UserName = Convert.ToString(dr["ContactName"]);
                        userData.FirstName = Convert.ToString(dr["Firstname"]);
                        userData.LastName = Convert.ToString(dr["Lastname"]);
                        userData.Email = Convert.ToString(dr["Email"]);
                        userData.EmailConfirmed = Convert.ToBoolean(dr["Activated"]);
                        userData.Roles = GetUserRoles(dr["pKey"].ToString());
                        userData.Claims = GetUserClaim(dr["pKey"].ToString());
                        userData.EventId = Convert.ToString(dr["SettingValue"]);
                        userData.EventName = Convert.ToString(dr["EventFullName"]);
                        userData.EventTypeId = Convert.ToString(dr["EventType_pkey"]);
                        userData.EventAccount_pkey = Convert.ToString(dr["EventAccount_pkey"]);
                        userData.EventStartDate = Convert.ToDateTime(dr["StartDate"]);
                        userData.EventEndDate = Convert.ToDateTime(dr["EndDate"]);
                        //userData.ParticipationStatus_pKey = userData.CustomerId == "20554" ? "0" : Convert.ToString(dr["ParticipationStatus_pKey"]);
                        userData.Region = Convert.ToString(dr["Region"]);
                        userData.RegionCode = Convert.ToString(dr["RegionCode"]);
                        userData.TimeOffset = Convert.ToString(dr["TimeOffset"]);
                        userData.ParticipationStatus_pKey = Convert.ToString(dr["ParticipationStatus_pKey"]);
                        userData.ISEventFeedbackResponse = Convert.ToString(dr["ISEventFeedbackResponse"]);
                        userData.LeftBlockImage = Convert.ToString(dr["LeftBlockImage"]);
                        userData.ApploginforParticipents = Convert.ToString(dr["ApploginforParticipents"]);
                        userData.LocationTimeInterval = Convert.ToString(dr["LocationTimeInterval"]);

                        userData.RegistrationLevel_Pkey = Convert.ToString(dr["RegistrationLevel_Pkey"]);
                        userData.IsLicenseNumber = Convert.ToBoolean(dr["IsLicenseNumber"]);
                        userData.IsSpeaker = Convert.ToBoolean(dr["IsSpeaker"]);
                        Boolean IsCloseEvent = Convert.ToBoolean(dr["IsCloseEvent"]);
                        if (Convert.ToInt32(dr["ApploginforParticipents"]) == 0 && Convert.ToInt32(dr["StaffMember1"]) != 1)
                        {
                            userData.IntResult = 3;
                        }
                        else if ((Convert.ToInt32(dr["ApploginforParticipents"]) == 1) && Convert.ToInt32(dr["ParticipationStatus_pKey1"]) != 1)
                        {
                            userData.IntResult = 4;
                        }
                        else if (IsCloseEvent == true && userData.IntResult==0)
                        {
                            userData.IntResult = 5;
                        }
                        else
                        {
                            userData.IntResult = 0;
                        }

                        return userData;
                    }
                    else
                    {
                        userData.IntResult = 2;
                        return userData;
                    }
                }
                else
                {
                    userData.IntResult = 1;
                    return userData;
                }
            }
            catch { }
            return null;
        }
        public async Task<IdentityUser> Apigetuserbynameandpassword_Web(string UserName, string password)
        {
            try
            {
                var userData = new IdentityUser();
                SqlParameter[] parameters = new SqlParameter[]
                {
                       new SqlParameter("@UserName", UserName.Trim())
                };
                //string strSql = "Select a.*,s.SettingValue,e.EventFullName,e.EventType_pkey, ISNULL(t1.pkey,0) as EventAccount_pkey ,ISNULL(t1.ParticipationStatus_pKey,0) as ParticipationStatus_pKey from account_list a join Application_Settings s on s.pkey=187 " +
                //                "join Event_List e on e.pKey=s.SettingValue LEFT OUTER JOIN Event_Accounts t1 ON t1.Account_pKey=a.pKey and t1.Event_pKey=s.SettingValue where a.UL=@UserName or a.Email=@UserName";
                string strSql = " EXEC Apigetuserbynameandpassword_Web  @UserName";
                var dt = await SqlHelper.ExecuteTableAsync(strSql, CommandType.Text, parameters);
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    bool val = Validate_Password(password, dr["UP"].ToString());
                    if (val == true)
                    {
                        userData.Id = Convert.ToInt32(dr["pKey"]);
                        userData.CustomerId = Convert.ToString(dr["pKey"]);
                        userData.UserId = Convert.ToString(dr["UL"]);
                        userData.UserName = Convert.ToString(dr["ContactName"]);
                        userData.FirstName = Convert.ToString(dr["Firstname"]);
                        userData.LastName = Convert.ToString(dr["Lastname"]);
                        userData.Email = Convert.ToString(dr["Email"]);
                        userData.EmailConfirmed = Convert.ToBoolean(dr["Activated"]);
                        userData.Roles = GetUserRoles(dr["pKey"].ToString());
                        userData.Claims = GetUserClaim(dr["pKey"].ToString());
                        userData.EventId = Convert.ToString(dr["SettingValue"]);
                        userData.EventName = Convert.ToString(dr["EventFullName"]);
                        userData.EventTypeId = Convert.ToString(dr["EventType_pkey"]);
                        userData.EventAccount_pkey = Convert.ToString(dr["EventAccount_pkey"]);
                        //userData.ParticipationStatus_pKey = userData.CustomerId == "20554" ? "0" : Convert.ToString(dr["ParticipationStatus_pKey"]);
                        userData.ParticipationStatus_pKey = Convert.ToString(dr["ParticipationStatus_pKey"]);
                        return userData;
                    }
                }
            }
            catch { }
            return null;
        }

        public async Task<Boolean> AllowMyboothUser(string username)
        {
            //Checking the permission in DB.
            SqlParameter[] parameters = new SqlParameter[]
            {
                    new SqlParameter("@paramUserID", username),
                    new SqlParameter("@paramEventKey", 187)
            };

            DataTable dt = await SqlHelper.ExecuteTableAsync("sp_AllowMyBoothUser", CommandType.StoredProcedure, parameters);

            if ((dt.Rows.Count > 0) && (Convert.ToBoolean(dt.Rows[0]["res"])))
            {
                return true;
            }

            return false;
        }

        private List<string> GetUserRoles(string UserId)
        {
            var roleList = new List<string>();
            roleList.Add("KeyUser");
            return roleList;
        }

        private List<Claim> GetUserClaim(string UserId)
        {
            var claimList = new List<Claim>();
            //claimList.Add(new Claim(){  } );
            return claimList;
        }

        public async Task<StatusCode> SP_ValidateUser(string UserName, string Password)
        {
            StatusCode obj = new StatusCode();
            obj.lst = new List<UserResult>();
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@LoginName", UserName),
                new SqlParameter("@Password", Password)
            };
            DataSet ds = await SqlHelper.ExecuteSetAsync("SP_ValidateUser", 2, CommandType.StoredProcedure, parameters);
            DataTable dt = ds.Tables[0];
            obj.Status = dt.Rows[0]["StatusCode"].ToString();
            obj.ErrorMsg = dt.Rows[0]["ErrorMsg"].ToString();
            if (obj.Status.ToUpper().ToString() == "OK")
            {
                dt = ds.Tables[1];
                Boolean val = Validate_Password(Password, dt.Rows[0]["UP"].ToString());
                if (val == true)
                {
                    if (dt.Rows.Count > 0)
                    {
                        string serText = JsonConvert.SerializeObject(ds.Tables[1].Rows[0].Table);
                        obj.lst = JsonConvert.DeserializeObject<List<UserResult>>(serText);
                    }
                }
                else
                {
                    obj.Status = "Not ok";
                    obj.ErrorMsg = "Password Wrong";
                }
            }
            return obj;
        }


        public async Task<List<AlltypeFiles>> Resource_select(string Type, string EventOrganizations_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@type", Type),
                new SqlParameter("@EventOrganizations_pkey", EventOrganizations_pkey)
            };
            List<AlltypeFiles> list = await SqlHelper.ExecuteListAsync<AlltypeFiles>("API_Resource_select", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<Partner>> Partnerlist_select(string EventId, string Account_pkey = "")
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Event_pkey", EventId),
                new SqlParameter("@Account_pkey", Account_pkey)
            };
            List<Partner> list = await SqlHelper.ExecuteListAsync<Partner>("API_Partnerlist_select", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<PartnerGreeting>> GETGreeting(string EventOrganizations_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@EventOrganizations_pkey", EventOrganizations_pkey)
            };
            List<PartnerGreeting> list = await SqlHelper.ExecuteListAsync<PartnerGreeting>("API_GETGreeting", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<Event_Info>> EventInfo_select(string EventId)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Event_pkey", EventId)
            };
            List<Event_Info> list = await SqlHelper.ExecuteListAsync<Event_Info>("Api_eventinfo_select", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<UserDetails>> GetUserDetail(string Account_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_pkey", Account_pkey)
            };
            List<UserDetails> list = await SqlHelper.ExecuteListAsyncAccount<UserDetails>("API_UserContact_info", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<Program>> ProgramList(string EventId, bool ShowRelated, bool ShowTopic, bool ShowSpeak, bool IsNew, string strtopic)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Event_pkey", EventId),
                new SqlParameter("@ShowRelated", ShowRelated),
                new SqlParameter("@ShowTopic", ShowTopic),
                new SqlParameter("@ShowSpeak", ShowSpeak),
                new SqlParameter("@IsNew", IsNew),
                new SqlParameter("@strTopic", strtopic)

            };
            List<Program> list = await SqlHelper.ExecuteListAsync<Program>("API_Program_all", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<Speaker>> SpeakerList(string EventId, Boolean ShortByOrg)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Event_pkey", EventId),
                new SqlParameter("@ShortbyOrganization", ShortByOrg)
            };
            List<Speaker> list = await SqlHelper.ExecuteListAsync<Speaker>("API_Speaker_Select_ALL", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<SessionDetails>> Session(string EventId, string Account_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Event_pkey", EventId),
                new SqlParameter("@Account_pkey", Account_pkey)
            };
            List<SessionDetails> list = await SqlHelper.ExecuteListAsync<SessionDetails>("API_AccountWise_Session", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<PublicSessions>> Session_Details(string EvtSession_pkey, Boolean IsShowReleated, string Account_pkey = "0", string AppURL = "")
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@EventSession_pkey", EvtSession_pkey),
                new SqlParameter("@IsShowReleated", IsShowReleated),
                new SqlParameter("@Account_pkey", Account_pkey),
                new SqlParameter("@AppURL", AppURL)
            };
            List<PublicSessions> list = await SqlHelper.ExecuteListAsync<PublicSessions>("API_GETSessionDetails", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<AccountBio>> AccountBio(string Account_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_pkey", Account_pkey)
            };
            List<AccountBio> list = await SqlHelper.ExecuteListAsyncAccount<AccountBio>("API_AccountBIO_Select", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<AccountBio>> AccountBio(string Account_pkey, string Event_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_pkey", Account_pkey),
                new SqlParameter("@Event_pkey", Event_pkey),
            };
            List<AccountBio> list = await SqlHelper.ExecuteListAsyncAccount<AccountBio>("API_AccountBIO_Select", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<ViewMySchdule>> VIEW_MySchedule(string Account_pkey, string Event_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_pkey", Account_pkey),
                new SqlParameter("@Event_pkey", Event_pkey)
            };
            List<ViewMySchdule> list = await SqlHelper.ExecuteListAsync<ViewMySchdule>("API_VIEW_MySchedule", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<ViewEventShowURL>> GetPlayLinkandURL(string EventSession_pkey, string Account_pkey, string Log_Pkey, string Event_pkey, string FileName, string IpAddress, string Access_Type)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@EventSession_pkey", EventSession_pkey),
                  new SqlParameter("@Account_pkey", Account_pkey),
                new SqlParameter("@Log_Pkey", Log_Pkey),
                new SqlParameter("@Event_pkey", Event_pkey),
                 new SqlParameter("@FileName", FileName),
                  new SqlParameter("@IpAddress", IpAddress),
                   new SqlParameter("@Access_Type", Access_Type),
                    //new SqlParameter("@Account_pkey", Account_pkey),
            };
            List<ViewEventShowURL> list = await SqlHelper.ExecuteListAsync<ViewEventShowURL>("API_GetPlayLinkandURL", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<CreateMySchdule>> Create_MySchedule(string Account_pkey, string Event_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_pkey", Account_pkey),
                new SqlParameter("@Event_pkey", Event_pkey)
            };
            List<CreateMySchdule> list = await SqlHelper.ExecuteListAsync<CreateMySchdule>("API_Create_MySchedule", CommandType.StoredProcedure, parameters);
            return list;
        }
        //public async Task<List<Program>> ProgramList(string EventId)
        //{
        //    SqlParameter[] parameters = new SqlParameter[]
        //    {
        //        new SqlParameter("@Event_pkey", EventId)
        //    };
        //    List<Program> list = await SqlHelper.ExecuteListAsync<Program>("API_Program_all", CommandType.StoredProcedure, parameters);
        //    return list;
        //}

        //public async Task<StatusCode> GetUserDetail(string Account_pkey)
        //{
        //    StatusCode obj = new StatusCode();
        //    obj.UList = new List<UserDetails>();
        //    SqlParameter[] parameters = new SqlParameter[]
        //  {
        //        new SqlParameter("@Account_pkey", Account_pkey)
        //  };
        //    List<UserDetails> list = await SqlHelper.ExecuteListAsync<UserDetails>("API_UserContact_info", CommandType.StoredProcedure, parameters);
        //    obj.UList = list;
        //    return obj;
        //}

        public async Task<string> UserUpdateDetail(UserDetails user)
        {
            StatusCode obj = new StatusCode();

            obj.UList = new List<UserDetails>();
            SqlParameter[] parameters = new SqlParameter[]
          {
                new SqlParameter("@Account_pkey", user.Account_pkey),
                new SqlParameter("@ContactName", user.ContactName),
                new SqlParameter("@Firstname", user.Firstname),
                new SqlParameter("@MiddleName", user.MiddleName),
                new SqlParameter("@Lastname", user.Lastname),
                new SqlParameter("@Suffix", user.Suffix),
                new SqlParameter("@Title", user.Title),
                new SqlParameter("@Comment", user.Comment),
                new SqlParameter("@PersonalBio", user.PersonalBio),
                new SqlParameter("@AboutMe", user.AboutMe),
                new SqlParameter("@Degrees", user.Degrees),
                new SqlParameter("@Nickname", user.Nickname),
                new SqlParameter("@Email", user.Email),
                new SqlParameter("@Email2", user.Email2),
                new SqlParameter("@Phone", Convert.ToString(user.Phone)),
                new SqlParameter("@Phone2", Convert.ToString( user.Phone2)),
                 new SqlParameter("@Address1", user.Address1),
                new SqlParameter("@Address2", user.Address2) ,
                  new SqlParameter("@ZipCode", user.ZipCode) ,

                new SqlParameter("@City", user.City),
                 new SqlParameter("@State_pKey", user.State_pKey),
                new SqlParameter("@Country_pKey", user.Country_pKey),
                 new SqlParameter("@TimezonePKey", user.TimezonePKey),
                //new SqlParameter("@ParentOrganization_pKey", user.OrganizationID),
                

                 new SqlParameter("@Department", user.Department),
                 new SqlParameter("@PhoneType_pKey", user.PhoneType_pKey) ,
                 new SqlParameter("@PhoneType2_pKey", user.PhoneType2_pKey) ,
                  new SqlParameter("@OtherState", user.OtherState) ,


                  new SqlParameter("@LicenseType", user.LicenseType) ,
                  new SqlParameter("@LicenseState", user.LicenseState) ,
                  new SqlParameter("@LicenseNumber", user.LicenseNumber) ,
                  new SqlParameter("@EmailUsed", user.EmailUsed) ,
                  new SqlParameter("@LinkedInProfile", user.LinkedInProfile) ,
                  new SqlParameter("@SpecialArrangement", user.SpecialArrangement) ,
                  new SqlParameter("@SpeakerMessage", user.SpeakerMessage) ,
                  new SqlParameter("@SpeakingPermission", user.SpeakingPermission) ,
                  new SqlParameter("@SpecialSpeaker", user.SpecialSpeaker) ,

                  new SqlParameter("@PotentialSpeaker", user.PotentialSpeaker) ,
                  new SqlParameter("@DecisionMaker", user.DecisionMaker) ,
                  new SqlParameter("@VIP", user.VIP),
                  //new SqlParameter("@Suffix_pkey", user.Suffix_pkey),
                  new SqlParameter("@Salutation_pKey", user.Salutation_pKey),
                  new SqlParameter("@ParentOrganization_pKey", user.ParentOrganization_pKey),
                  new SqlParameter("@Member", user.Member),
                  new SqlParameter("@GetJournal", user.GetJournal),
                  new SqlParameter("@SkypeAddress", user.SkypeAddress),
                  new SqlParameter("@PhoneticName", user.PhoneticName),
                  new SqlParameter("@Phone1Ext", user.Phone1Ext),
                  new SqlParameter("@Phone2Ext", user.Phone2Ext),
                  new SqlParameter("@UL", user.UL),

                 new SqlParameter("@Phone1CC", user.Phone1CC),
                  new SqlParameter("@Phone2CC", user.Phone2CC),
                  new SqlParameter("@AllowCall", user.AllowCall)
                //new SqlParameter("@AccountStatus_pkey", user.AccountStatus_pkey),

                //new SqlParameter("@AllowText", user.AllowText),
                //new SqlParameter("@AllowEmail", user.AllowEmail),
                //new SqlParameter("@AllowCall", user.AllowCall),

                //new SqlParameter("@City", user.City),

                //new SqlParameter("@ContactName", user.ContactName),
                //new SqlParameter("@ContactName", user.ContactName),
                //new SqlParameter("@ContactName", user.ContactName),
                //new SqlParameter("@ContactName", user.ContactName),
                //new SqlParameter("@ContactName", user.ContactName),

                //new SqlParameter("@ContactName", user.ContactName),
                //new SqlParameter("@ContactName", user.ContactName),

          };
            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("Api_usercontact_update", CommandType.StoredProcedure, parameters));

            if (isInsert)
                return "Updated";
            else
                return "Can't Update..";

        }

        public async Task<string> RegiQuestion_Attempt(string Account_pkey, string Event_pkey)
        {
            StatusCode obj = new StatusCode();

            obj.UList = new List<UserDetails>();
            SqlParameter[] parameters = new SqlParameter[]
            {
                    new SqlParameter("@Event_pKey", Event_pkey),
                    new SqlParameter("@Account_pkey", Account_pkey)
            };
            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_RegiQuestion_Attempt", CommandType.StoredProcedure, parameters));
            //bool isInsert = true;
            if (isInsert)
                return "Updated";
            else
                return "Can't Update..";

        }
        public async Task<string> LocationUpdate(LocationParameter postSurvey)
        {
            StatusCode obj = new StatusCode();

            obj.UList = new List<UserDetails>();
            SqlParameter[] parameters = new SqlParameter[]
            {
                    new SqlParameter("@Event_pKey", postSurvey.EventId),
                    new SqlParameter("@AppType", postSurvey.AppType),
                    new SqlParameter("@Account_pKey", postSurvey.AccountId),
                    new SqlParameter("@DeviceID", postSurvey.DeviceId),
                    new SqlParameter("@altitude", postSurvey.altitude),
                    new SqlParameter("@longitude", postSurvey.longitude),
                    new SqlParameter("@LastGeoAccessTime", DateTime.UtcNow),
            };
            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_LocationUpdate", CommandType.StoredProcedure, parameters));
            //bool isInsert = true;
            if (isInsert)
                return "Location Updated";
            else
                return "Can't Update Location..";

        }


        public async Task<string> SaveParticipantsSessionNotes(ParticipentNote postSurvey)
        {
            StatusCode obj = new StatusCode();

            obj.UList = new List<UserDetails>();
            SqlParameter[] parameters = new SqlParameter[]
            {
                    new SqlParameter("@pkey", postSurvey.pkey),
                    new SqlParameter("@EventSession_pkey", postSurvey.EventSession_pkey),
                    new SqlParameter("@SessionNote", postSurvey.SessionNote),
                    new SqlParameter("@Account_pkey", postSurvey.Account_pkey),
                    new SqlParameter("@IsActive", postSurvey.IsActive)

            };
            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_Participants_SessionNotes_Save", CommandType.StoredProcedure, parameters));
            //bool isInsert = true;
            if (isInsert)
                return "Session notes updated";
            else
                return "Can't update session notes..";

        }



        public async Task<string> POLLINGRESULTSHOWORNOT(string pkey, Boolean ShowResult, string EventSession_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                    new SqlParameter("@pkey", pkey),
                    new SqlParameter("@ShowResult", ShowResult),
                    new SqlParameter("@EventSession_pkey", EventSession_pkey)
            };
            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_POLLINGRESULTSHOWORNOT", CommandType.StoredProcedure, parameters));
            //bool isInsert = true;
            if (isInsert)
                return "Updated";
            else
                return "Can't Update";

        }


        public async Task<string> POLLINGSTARTORNOT(string pkey, Boolean IsStarted, string EventSession_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                    new SqlParameter("@pkey", pkey),
                    new SqlParameter("@IsStarted", IsStarted),
                    new SqlParameter("@EventSession_pkey", EventSession_pkey)
            };
            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_POLLINGSTARTORNOT", CommandType.StoredProcedure, parameters));
            //bool isInsert = true;
            if (isInsert)
                return "Updated";
            else
                return "Can't Update";

        }


        //public async Task<string>
        public async Task<List<UserIssue>> UserIssueSave(UserIssue user)
        {
            StatusCode obj = new StatusCode();
            obj.UList = new List<UserDetails>();
            String str = "Issue:  " + user.title.Trim()+ "<br />";
            str= str+"Submitter:" +user.username + "<br />";
            str = str + "Email: " + user.email + "<br />";
            SqlParameter[] parameters = new SqlParameter[]
          {
                new SqlParameter("@IssueName", user.title),
                new SqlParameter("@PageUrl", user.issueArea),
                new SqlParameter("@IssueType_pkey", 1),
                new SqlParameter("@Description", user.description +"<br /><br /><br />"+str ),
                new SqlParameter("@IssueCategory_pkey", user.IssueCategory_pkey),
                new SqlParameter("@EnteredByAccount_pkey", user.account_pkey)
                //new SqlParameter("@IssueName", user.account_pkey),
                //new SqlParameter("@IssueName", user.account_pkey),
                //new SqlParameter("@IssueName", user.account_pkey),
                //new SqlParameter("@IssueName", user.account_pkey),
                //new SqlParameter("@IssueName", user.account_pkey),
                //new SqlParameter("@IssueName", user.account_pkey)
          };
            List<UserIssue> list = await SqlHelper.ExecuteListAsync<UserIssue>("API_Userissue_save", CommandType.StoredProcedure, parameters);
            return list;
            //bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_Userissue_save", CommandType.StoredProcedure, parameters));
            ////bool isInsert = true;
            //if (isInsert)
            //    return "Updated";
            //else
            //    return "Can't Update..";

        }

        public async Task<List<Networking>> NetworkingList(string Account_pkey, string Event_pkey, string ParticipationStatus_pKey, string OrganizationType_pkey, String Country_pKey)
        {

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_pkey", Account_pkey),
                new SqlParameter("@Event_pkey", Event_pkey) ,
                  new SqlParameter("@ParticipationStatus_pKey", (ParticipationStatus_pKey.Trim().Length>0? ParticipationStatus_pKey :"0")),
                    new SqlParameter("@OrganizationType_pkey", (OrganizationType_pkey.Trim().Length>0?OrganizationType_pkey:"0")),
                      new SqlParameter("@Country_pKey", (Country_pKey.Trim().Length>0?Country_pKey:"0")),
                        new SqlParameter("@Search", "")

            };
            List<Networking> list = await SqlHelper.ExecuteListAsync<Networking>("API_NetWorking_Select", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<Speaker>> Api_organizationwise_attendee(string Event_pkey, string Organization_pkey)
        {

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Organization_pkey", Organization_pkey),
                new SqlParameter("@Event_pkey", Event_pkey)



            };
            List<Speaker> list = await SqlHelper.ExecuteListAsync<Speaker>("Api_organizationwise_attendee", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<Personchat>> GetChatPeople(string Account_pkey, string Event_pkey, string Search)
        {

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ParamId", Account_pkey),
                new SqlParameter("@ParamDateTime", null),
                new SqlParameter("@ParamEventKey", Event_pkey),
                new SqlParameter("@ParamSearch", (Search==null?string.Empty:Search))
            };
            List<Personchat> list = await SqlHelper.ExecuteListAsync<Personchat>("API_GetChatPeople", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<chatHistory>> ChatHistoryByPerson(string Account_pkey, string Event_pkey, string userAccount_pkey)
        {

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ParamId", Account_pkey),
                new SqlParameter("@ParamDateTime", null),
                new SqlParameter("@ParamEventKey", Event_pkey),
                 new SqlParameter("@ParamTargetId", userAccount_pkey)
            };
            List<chatHistory> list = await SqlHelper.ExecuteListAsync<chatHistory>("API_GetChatHistoryByPerson", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<NeworkingOutgoing>> NetworkingOurgoingList(string Account_pkey, string Event_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_pkey", Account_pkey),
                new SqlParameter("@Event_pkey", Event_pkey)
            };
            List<NeworkingOutgoing> list = await SqlHelper.ExecuteListAsync<NeworkingOutgoing>("API_NetworkingOutgoing_Select", CommandType.StoredProcedure, parameters);
            return list;
        }


        public async Task<List<NetworkingIncoming>> NetworkingIncomingList(string Account_pkey, string Event_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_pkey", Account_pkey),
                new SqlParameter("@Event_pkey", Event_pkey)
            };
            List<NetworkingIncoming> list = await SqlHelper.ExecuteListAsync<NetworkingIncoming>("Networking_Incoming_Select", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<string> Set_Attended(string Account_pkey, string EventSession_pkey, Boolean Attending, Boolean Slides, Boolean watch)
        {
            StatusCode obj = new StatusCode();

            obj.UList = new List<UserDetails>();
            SqlParameter[] parameters = new SqlParameter[]
          {
                new SqlParameter("@APKey", Account_pkey),

                new SqlParameter("@ESPKey", EventSession_pkey),
                new SqlParameter("@Attending", Attending),
                new SqlParameter("@Slides", Slides),
                new SqlParameter("@watch", watch)

          };
            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_SetAttend", CommandType.StoredProcedure, parameters));

            if (isInsert)
                return "Updated";
            else
                return "Can't Update..";

        }

        public async Task<List<DropdownListBind>> ParticipationType(String Event_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Event_pkey", Event_pkey)
            };
            List<DropdownListBind> list = await SqlHelper.ExecuteListAsync<DropdownListBind>("API_ParticipationType", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<DropdownListBind>> ProfInterestsList()
        {
            SqlParameter[] parameters = new SqlParameter[]
            {

            };
            List<DropdownListBind> list = await SqlHelper.ExecuteListAsync<DropdownListBind>("ddl_ProfInterests", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<DropdownListBind>> LicenseTypeList()
        {
            SqlParameter[] parameters = new SqlParameter[]
            {

            };
            List<DropdownListBind> list = await SqlHelper.ExecuteListAsync<DropdownListBind>("API_LicenseType", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<DropdownListBind>> ddlList(string Type, string Country_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Type", Type),
                new SqlParameter("@Country_pkey", Country_pkey)
            };
            List<DropdownListBind> list = await SqlHelper.ExecuteListAsync<DropdownListBind>("ddl_List", CommandType.StoredProcedure, parameters);
            return list;
        }


        public async Task<List<DropdownListBind>> ddlSpeakerAdvice()
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                // new SqlParameter("@Type", Type),
                //new SqlParameter("@Country_pkey", Country_pkey)
            };
            List<DropdownListBind> list = await SqlHelper.ExecuteListAsync<DropdownListBind>("API_ddlSpeakerAdvice", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<Organization>> Organization_Select_ALL(string Event_pkey, string Name, string pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Event_pkey", Event_pkey),
                new SqlParameter("@Name", Name),
                 new SqlParameter("@pkey", pkey),
            };
            List<Organization> list = await SqlHelper.ExecuteListAsync<Organization>("API_Organization_Select_ALL", CommandType.StoredProcedure, parameters);
            return list;
        }


        public async Task<List<FedbackReBind>> feedback_select(string Eventsession_pkey, string Account_pkey, string Event_pkey, string LogByAccount_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Eventsession_pkey", Eventsession_pkey),
                new SqlParameter("@Account_pkey", Account_pkey),
                 new SqlParameter("@Event_pkey", Event_pkey),
                  new SqlParameter("@LogByAccount_pkey", LogByAccount_pkey),
            };
            List<FedbackReBind> list = await SqlHelper.ExecuteListAsync<FedbackReBind>("Api_feedback_select", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<AttendeeQuestion>> API_AttendeeQuestion_Select(string EventSession_pkey, string Account_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@EventSession_pkey", EventSession_pkey),
                new SqlParameter("@Account_pkey", Account_pkey)
            };
            List<AttendeeQuestion> list = await SqlHelper.ExecuteListAsync<AttendeeQuestion>("API_AttendeeQuestion_Select", CommandType.StoredProcedure, parameters);
            return list;
        }


        public async Task<List<AttendeeRegistriongQues>> AttendeeRegistriongQuesSelect(string Event_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Event_pkey", Event_pkey)
                
            };
            List<AttendeeRegistriongQues> list = await SqlHelper.ExecuteListAsync<AttendeeRegistriongQues>("API_AttendeeRegistriongQuesSelect", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<Connectionstatus>> ConnectionRequest_status(string Event_pkey, string Account_pkey, string ConnectionAccount_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Event_pkey", Event_pkey),
                new SqlParameter("@Account_pkey", Account_pkey),
                new SqlParameter("@ConnectionAccount_pkey", ConnectionAccount_pkey)

            };
            List<Connectionstatus> list = await SqlHelper.ExecuteListAsync<Connectionstatus>("API_ConnectionRequest_status", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<Connectionstatus>> ConnectionStatus_Change(string Event_pkey, string Account_pkey, string ConnectionAccount_pkey, string status)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Event_pkey", Event_pkey),
                new SqlParameter("@Account_pkey", Account_pkey),
                new SqlParameter("@ConnectionAccount_pkey", ConnectionAccount_pkey),
                new SqlParameter("@Status_pkey", status)

            };
            List<Connectionstatus> list = await SqlHelper.ExecuteListAsync<Connectionstatus>("API_ConnectionStatus_Change", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<SearchAttendee>> SearchAttendeeLocationList(string Event_pkey, string CommonSearch, string Al, string Long)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Event_pkey", Event_pkey),
                new SqlParameter("@CommonSearch", CommonSearch),
                new SqlParameter("@Al", Al),
                new SqlParameter("@Long", Long)

            };
            List<SearchAttendee> list = await SqlHelper.ExecuteListAsync<SearchAttendee>("API_Attendee_LocSearch", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<EventSchedule>> EventSchedule(string Event_pkey, string Account_pkey, string EventOrganizations_pkey, string Date = null)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Event_pkey", Event_pkey),
                new SqlParameter("@Account_pkey", Account_pkey),
                new SqlParameter("@Exhibitor_pKey", EventOrganizations_pkey),
                new SqlParameter("@Date", Date)

            };
            List<EventSchedule> list = await SqlHelper.ExecuteListAsync<EventSchedule>("API_Schedule_event_select", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<ConnectPeople>> ConnectPeopleList(string Event_pkey, string Account_pkey, string EventOrganizations_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Event_pKey", Event_pkey),
                new SqlParameter("@Account_pkey", Account_pkey),
                new SqlParameter("@EventOrganization_pKey", EventOrganizations_pkey),
            };
            List<ConnectPeople> list = await SqlHelper.ExecuteListAsync<ConnectPeople>("API_ConnectPeople", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<LunchOptions>> LunchOption(string Event_pkey, string Account_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Event_pKey", Event_pkey),
                new SqlParameter("@Account_pkey", Account_pkey)
            };
            List<LunchOptions> list = await SqlHelper.ExecuteListAsync<LunchOptions>("API_MyLunchOptions", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<DropdownListBind>> MealList(string Event_pkey, string DefaultMeal, string StrMeal)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Event_pKey", Event_pkey),
                new SqlParameter("@DefaultMeal", Convert.ToInt32(DefaultMeal)),
                new SqlParameter("@STRMeal", StrMeal.Trim().ToString())
            };
            List<DropdownListBind> list = await SqlHelper.ExecuteListAsync<DropdownListBind>("API_MealList", CommandType.StoredProcedure, parameters);
            return list;
        }


        public async Task<List<AttendeeTreasureHunt>> AttendeeTreasureHuntSelect(string Event_pkey, string Account_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Event_pkey", Event_pkey),
                new SqlParameter("@Account_pkey", Account_pkey)


            };
            List<AttendeeTreasureHunt> list = await SqlHelper.ExecuteListAsync<AttendeeTreasureHunt>("API_AttendeeTreasureHunt", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<BadgeInfor>> BadgeInfo(string Account_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_pkey", Account_pkey)

            };
            List<BadgeInfor> list = await SqlHelper.ExecuteListAsync<BadgeInfor>("API_Badge_select", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<Examcharges>> Examcharges(string Event_pkey, string Account_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Event_pkey", Event_pkey),
                new SqlParameter("@Account_pkey", Account_pkey)

            };
            List<Examcharges> list = await SqlHelper.ExecuteListAsync<Examcharges>("API_Examcharges", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<BadgeDesignSetting>> BadgeDesignList(string pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@pkey", pkey)

            };
            List<BadgeDesignSetting> list = await SqlHelper.ExecuteListAsync<BadgeDesignSetting>("API_BadgeDesign_List_Select", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<AttendeeValidations>> SubmitQuestionlist(string Account_pkey, string Event_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_pkey", Account_pkey),
                new SqlParameter("@Event_pkey", Event_pkey)

            };
            List<AttendeeValidations> list = await SqlHelper.ExecuteListAsync<AttendeeValidations>("API_ExhibitorFeedbackForm_UserRespons", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<DropdownListBind>> ProfInterests_List(string Event_pkey = "0")
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Event_pkey", Event_pkey)
            };
            List<DropdownListBind> list = await SqlHelper.ExecuteListAsync<DropdownListBind>("ddl_ProfInterests_List", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<DropdownListBind>> ListIssueCategories()
        {
            SqlParameter[] parameters = new SqlParameter[]
            {

            };
            List<DropdownListBind> list = await SqlHelper.ExecuteListAsync<DropdownListBind>("API_IssueCategories_Select", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<FutureConferences>> ListFutureConferences()
        {
            SqlParameter[] parameters = new SqlParameter[]
            {

            };
            List<FutureConferences> list = await SqlHelper.ExecuteListAsync<FutureConferences>("API_FutureConferences", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<PhoneType>> PhoneTypesList()
        {
            SqlParameter[] parameters = new SqlParameter[]
            {

            };
            List<PhoneType> list = await SqlHelper.ExecuteListAsync<PhoneType>("API_PhoneTypes_Select", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<DropdownListBind>> CountryList()
        {
            SqlParameter[] parameters = new SqlParameter[]
            {

            };
            List<DropdownListBind> list = await SqlHelper.ExecuteListAsync<DropdownListBind>("API_CountryList", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<DropdownListBind>> EmailTypes()
        {
            SqlParameter[] parameters = new SqlParameter[]
            {

            };
            List<DropdownListBind> list = await SqlHelper.ExecuteListAsync<DropdownListBind>("API_EmailTypes", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<DropdownListBind>> StateList(string Country_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Country_pkey", Country_pkey),
            };
            List<DropdownListBind> list = await SqlHelper.ExecuteListAsync<DropdownListBind>("API_StateList", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<MeetingPlanneredit>> MeetingPlannerEdit(string MeetingPlanner_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@MeetingPlanner_pkey", MeetingPlanner_pkey),
            };
            List<MeetingPlanneredit> list = await SqlHelper.ExecuteListAsync<MeetingPlanneredit>("API_MeetingPlanner_Edit", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<EventAttending>> MeetingPlannerAttended_Select(string MeetingPlanner_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@MeetingPlanner_pkey", MeetingPlanner_pkey),
            };
            List<EventAttending> list = await SqlHelper.ExecuteListAsync<EventAttending>("API_MeetingPlanner_Attended_Select", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<Question_List>> QuestionList(Question_List li)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Event_pkey", li.Event_pkey),
                  new SqlParameter("@Account_pkey", li.Account_pkey),
                   new SqlParameter("@StartTime", Convert.ToDateTime( li.StartTime)),
                    new SqlParameter("@EndTime",Convert.ToDateTime( li.EndTime) ),
                 new SqlParameter("@EventSession_pkey", li.EventSession_pkey),
            };
            List<Question_List> list = await SqlHelper.ExecuteListAsync<Question_List>("API_AttendingQuesion_select", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<questionGraph>> questionGraph(string EventSession_pkey, string Question_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Question_pkey", Question_pkey),
                  new SqlParameter("@EventSession_pkey", EventSession_pkey)

            };
            List<questionGraph> list = await SqlHelper.ExecuteListAsync<questionGraph>("API_QuestionResponcforGraph", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<PoolingManagement>> PoolingManagement(string EventSession_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                  new SqlParameter("@EventSession_pkey", EventSession_pkey)

            };
            List<PoolingManagement> list = await SqlHelper.ExecuteListAsync<PoolingManagement>("API_SpeakerPollingResult", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<Form_List>> FormList()
        {
            SqlParameter[] parameters = new SqlParameter[]
            {

            };
            List<Form_List> list = await SqlHelper.ExecuteListAsync<Form_List>("Api_form_select", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<Form_List>> FeedbackForm(string Event_pkey, string Approved)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                   new SqlParameter("@Event_pkey", Event_pkey),
                      new SqlParameter("@IsApproved",Convert.ToBoolean( Approved))
            };
            List<Form_List> list = await SqlHelper.ExecuteListAsync<Form_List>("API_FeedbackForm", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<DropdownListBind>> TimeZoneList(string Country_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Country_pkey", Country_pkey),
            };
            List<DropdownListBind> list = await SqlHelper.ExecuteListAsync<DropdownListBind>("API_TimeZoneList", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<DropdownListBind>> TimeZoneListfor_Meeting(string Country_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Country_pkey", Country_pkey),
            };
            List<DropdownListBind> list = await SqlHelper.ExecuteListAsync<DropdownListBind>("API_TimeZoneListforMeeting", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<DropdownListBind>> SectorList()
        {
            SqlParameter[] parameters = new SqlParameter[]
            {

            };
            List<DropdownListBind> list = await SqlHelper.ExecuteListAsync<DropdownListBind>("API_Sector_Select", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<chatHistory>> GetChatHistory(string Account_pkey, string Event_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@ParamId", Account_pkey),
                new SqlParameter("@ParamDateTime", null),
                new SqlParameter("@ParamEventKey", Event_pkey)
            };
            List<chatHistory> list = await SqlHelper.ExecuteListAsync<chatHistory>("API_sp_GetChatHistory", CommandType.StoredProcedure, parameters);
            return list;
        }


        public async Task<List<EventSummary>> getEventSummary(string Account_pkey, string Event_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Account_pkey", Account_pkey),
                new SqlParameter("@Event_pkey", Event_pkey)
            };
            List<EventSummary> list = await SqlHelper.ExecuteListAsync<EventSummary>("API_EventSummary", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<Refundcharges>> getRefundcharges(string Account_pkey, string Event_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Account_pkey", Account_pkey),
                new SqlParameter("@Event_pkey", Event_pkey)
            };
            List<Refundcharges> list = await SqlHelper.ExecuteListAsync<Refundcharges>("API_checkRefund", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<NetworkingLevelMSG>> NetworkingLevel(string Account_pkey, string Event_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@paramAccPkey", Account_pkey),
                new SqlParameter("@paramEventKey", Event_pkey)
            };
            List<NetworkingLevelMSG> list = await SqlHelper.ExecuteListAsync<NetworkingLevelMSG>("API_sp_getpointsAndNetLevel", CommandType.StoredProcedure, parameters);
            return list;
        }
        //public async Task<Allchat> GetChatHistory(string Account_pkey, string Event_pkey)
        //{
        //    SqlParameter[] parameters = new SqlParameter[]
        //    {
        //        new SqlParameter("@ParamId", Account_pkey),
        //        new SqlParameter("@ParamDateTime", null),
        //        new SqlParameter("@ParamEventKey", Event_pkey)
        //           //new SqlParameter("@ISMobile", 1)
        //    };
        //    Allchat ob = new Allchat();
        //    DataSet ds = await SqlHelper.ExecuteSetAsync("API_sp_GetChatHistory", 2, CommandType.StoredProcedure, parameters);
        //    Allchat obj = new Allchat();
        //    obj.ListchatHistory = new List<chatHistory>();
        //    List<chatHistory> lst = new List<chatHistory>();
        //    foreach (DataTable dt in ds.Tables)
        //    {
        //        List<chatHistory> lsts = dt.DataTableToList<chatHistory>();
        //        lst.AddRange(lsts);
        //        //chatHistory ch = new chatHistory();
        //        //ch.ChtType = dt.Rows[0]["ChtType"].ToString();
        //        //ch.ID = dt.Rows[0]["ID"].ToString();
        //        //ch.img = dt.Rows[0]["img"].ToString();
        //        //ch.myID = dt.Rows[0]["myID"].ToString();
        //        //ch.topImg = dt.Rows[0]["topImg"].ToString();
        //        //ch.nm = dt.Rows[0]["nm"].ToString();
        //        //ch.topNm = dt.Rows[0]["topNm"].ToString();
        //        //ch.nickName = dt.Rows[0]["nickName"].ToString();
        //        //ch.ak = dt.Rows[0]["ak"].ToString();
        //        //ch.mid = dt.Rows[0]["mid"].ToString();
        //        //ch.mine = dt.Rows[0]["mine"].ToString();
        //        //ch.msgSt = dt.Rows[0]["msgSt"].ToString();
        //        //ch.strMsg = dt.Rows[0]["strMsg"].ToString();
        //        //ch.typeOfMsg = dt.Rows[0]["typeOfMsg"].ToString();
        //        //ch.timeOfMsg = dt.Rows[0]["timeOfMsg"].ToString();
        //        //ch.isMineNow = dt.Rows[0]["isMineNow"].ToString();
        //        //ch.DATEORDER = dt.Rows[0]["DATEORDER"].ToString();
        //        //lst.Add(ch);
        //        //obj.ListchatHistory.Add(lst1);
        //        //lst.Add(lst1);
        //        //obj.ListchatHistory.Add(List<lst>);
        //    }
        //    obj.ListchatHistory = lst;
        //    return obj;
        //}

        public async Task<ParticipatingOrganizations> ListParticipating_organizations(string Event_pkey, string ParticipationStatus_pKey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Event_pkey", Event_pkey),
                new SqlParameter("@ParticipationStatus_pKey", ParticipationStatus_pKey)
            };
            ParticipatingOrganizations ob = new ParticipatingOrganizations();
            DataSet ds = await SqlHelper.ExecuteSetAsync("API_Participating_organizations", 2, CommandType.StoredProcedure, parameters);
            ParticipatingOrganizations obj = new ParticipatingOrganizations();
            obj.ParticipatingList = new List<Participating>();
            obj.OrgTypeList = new List<DropdownListBind>();
            List<Participating> lst = ds.Tables[0].DataTableToList<Participating>();
            List<DropdownListBind> lsit = ds.Tables[1].DataTableToList<DropdownListBind>();
            obj.ParticipatingList = lst;
            obj.OrgTypeList = lsit;
            //obj.Return = "";
            //string json = JsonConvert.SerializeObject(obj); //new JavaScriptSerializer().Serialize(obj);
            //string outputjson = ""; //= Newtonsoft.Json.Linq.JObject.Parse(json);
            //outputjson= json.Replace("\"", "");
            //outputjson = json.ReplaceAll("\\r\\n", "");
            return obj;
        }

        public async Task<List<DropdownListBind>> OrganizationTypesList()
        {
            SqlParameter[] parameters = new SqlParameter[]
            {

            };
            List<DropdownListBind> list = await SqlHelper.ExecuteListAsync<DropdownListBind>("API_OrganizationTypes", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<SettingText> Setting_Text(string ID)
        {
            SettingText li = new SettingText();
            clsSettings obj = new clsSettings();


            DataTable dt = new DataTable();
            SqlParameter[] parameters = new SqlParameter[]
            {
                       new SqlParameter("@pKey", ID.Trim())
            };
            string strSql = "select AppTextBlock from Application_Text where pKey =@pKey";
            dt = await SqlHelper.ExecuteTableAsync(strSql, CommandType.Text, parameters);
            string txt = "";
            if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                txt = obj.ReplaceTermsMini(dr["AppTextBlock"].ToString());
            }
            li.AppTextBlock = txt.ToString();
            return li;
        }

        public async Task<List<OverviewMobile>> OverView_MobileSetting(string Event_pkey, string App_type, string Section_type)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Event_pkey", Event_pkey.Trim()) ,
                new SqlParameter("@AppType", App_type.Trim()),
                new SqlParameter("@SectionType", Section_type.Trim())
            };
            List<OverviewMobile> list = await SqlHelper.ExecuteListAsync<OverviewMobile>("API_Mobile_Settings_Select", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<MobileInfo>> Mobile_Info(string Event_pkey, string App_type, string Section_type)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Event_pkey", Event_pkey.Trim()) ,
                new SqlParameter("@AppType", App_type.Trim()),
                new SqlParameter("@SectionType", Section_type.Trim())
            };
            List<MobileInfo> list = await SqlHelper.ExecuteListAsync<MobileInfo>("API_Mobile_Settings_Select", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<FAQ>> FAQs_List(string account_pkey, string event_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
           {
                new SqlParameter("@Event_pkey", event_pkey.Trim()) ,
                new SqlParameter("@account_pkey", account_pkey.Trim())
           };
            List<FAQ> list = await SqlHelper.ExecuteListAsync<FAQ>("API_FAQ", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<Contacts>> Contacts_List(string event_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
           {
                new SqlParameter("@Event_pkey", event_pkey.Trim())
           };
            List<Contacts> list = await SqlHelper.ExecuteListAsync<Contacts>("API_Contacts", CommandType.StoredProcedure, parameters);
            return list;
        }


        public async Task<string> NetworkingOutgoing_Save(NeworkingOutgoing li)
        {
            StatusCode obj = new StatusCode();

            obj.UList = new List<UserDetails>();
            SqlParameter[] parameters = new SqlParameter[]
          {
                new SqlParameter("@Account_pkey", li.Account_pKey),
                new SqlParameter("@EventAccount_pKey", li.EventAccount_pKey),
                new SqlParameter("@Msg", li.Msg),
                new SqlParameter("@Event_pkey", li.Event_pkey)
                //new SqlParameter("@IncE", li.EventAccount_pKey),
                //new SqlParameter("@EventAccount_pKey", li.EventAccount_pKey),

          };
            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_Networkingoutgoing_save", CommandType.StoredProcedure, parameters));

            if (isInsert)
                return "Updated";
            else
                return "Can't Update..";

        }

        public async Task<string> Networkingincoming_Save(NetworkingIncoming li)
        {
            StatusCode obj = new StatusCode();

            obj.UList = new List<UserDetails>();
            SqlParameter[] parameters = new SqlParameter[]
          {
                new SqlParameter("@pkey", li.pKey),
                new SqlParameter("@Response", li.Responce_pkey),
                new SqlParameter("@MSG", li.Msg)
                //new SqlParameter("@IncE", li.EventAccount_pKey),
                //new SqlParameter("@EventAccount_pKey", li.EventAccount_pKey),
          };

            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("NetWorkingIncoming_Save", CommandType.StoredProcedure, parameters));
            if (isInsert)
                return "Updated";
            else
                return "Can't Update..";
        }


        public async Task<string> SaveExamcharges(string Event_pkey, string Account_pkey, string Memo, String chargeType_pkey, string Amount)
        {
            StatusCode obj = new StatusCode();

            obj.UList = new List<UserDetails>();
            SqlParameter[] parameters = new SqlParameter[]
          {
                new SqlParameter("@Account_pkey", Account_pkey),
                new SqlParameter("@Event_pkey", Event_pkey),
                new SqlParameter("@AddedBy", Account_pkey),
                new SqlParameter("@Amount", Amount),
                new SqlParameter("@Memo", Memo),
                new SqlParameter("@chargeType_pkey", chargeType_pkey),

          };

            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_SaveExamcharges", CommandType.StoredProcedure, parameters));
            if (isInsert)
                return "Updated";
            else
                return "Can't Update..";
        }

        public async Task<List<MeetingPlannerList>> MeetingPlannerList(MeetingPlannerList li)
        {
            SqlParameter[] parameters = new SqlParameter[]
           {
                new SqlParameter("@Event_pkey", li.Event_pkey),
                 new SqlParameter("@Account_pkey", li.loginAccount_pkey),
                  new SqlParameter("@Type", li.Type)
           };
            List<MeetingPlannerList> list = await SqlHelper.ExecuteListAsync<MeetingPlannerList>("API_MeetingPlanner_Select", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<string> MeetingPlannerSave(MeetingPlannerSave li)
        {
            StatusCode obj = new StatusCode();

            obj.UList = new List<UserDetails>();
            SqlParameter[] parameters = new SqlParameter[]
          {
                new SqlParameter("@pkey", li.pkey),
                new SqlParameter("@MeetingTitle", li.MeetingTitle),
                new SqlParameter("@MeetingPurpose", li.MeetingPurpose),
                new SqlParameter("@Event_pkey", li.Event_pkey),
                new SqlParameter("@HostAccount_pkey", li.HostAccount_pkey),
                new SqlParameter("@MeetingStartTime", li.MeetingStartTime),
                new SqlParameter("@MeetingEndTime", li.MeetingEndTime),
                new SqlParameter("@XMLString", li.XMLString),
                new SqlParameter("@LocationID", li.LocationID),
                new SqlParameter("@TimeZone_pkey", Convert.ToInt32( li.TimeZone_pkey.ToString())),
                new SqlParameter("@Location_Other", li.Location_Other),
                new SqlParameter("@overlapping", li.overlapping)

          };

            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_MeetingPlanner_Save", CommandType.StoredProcedure, parameters));
            if (isInsert)
                return "Created";
            else
                return "Can't Update..";
        }


        public async Task<string> MeetingPlannerUpdateAttended(MeetingPlannerSave li)
        {
            StatusCode obj = new StatusCode();

            obj.UList = new List<UserDetails>();
            SqlParameter[] parameters = new SqlParameter[]
          {
                new SqlParameter("@XMLString", li.XMLString)
          };

            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_MeetingPlanner_Update_Attended", CommandType.StoredProcedure, parameters));
            if (isInsert)
                return "Created";
            else
                return "Can't Update..";
        }

        public async Task<string> MeetingDelete(string MeetingPlanner_pkey)
        {
            StatusCode obj = new StatusCode();

            obj.UList = new List<UserDetails>();
            SqlParameter[] parameters = new SqlParameter[]
          {
                new SqlParameter("@MeetingPlanner_pkey", MeetingPlanner_pkey)
          };

            bool delete = (await SqlHelper.ExecuteNonQueryAsync("API_MeetingDelete", CommandType.StoredProcedure, parameters));
            if (delete)
                return "Deleted";
            else
                return "Can't Deleted..";
        }

        public async Task<string> DeleteAttending(string pkey)
        {
            StatusCode obj = new StatusCode();

            obj.UList = new List<UserDetails>();
            SqlParameter[] parameters = new SqlParameter[]
          {
                new SqlParameter("@pkey", pkey)
          };

            bool delete = (await SqlHelper.ExecuteNonQueryAsync("API_Delete_Attending", CommandType.StoredProcedure, parameters));
            if (delete)
                return "Deleted";
            else
                return "Can't Deleted..";
        }
        
        public async Task<List<DropdownListBind>> meetinglocationList()
        {
            SqlParameter[] parameters = new SqlParameter[]
            {

            };
            List<DropdownListBind> list = await SqlHelper.ExecuteListAsync<DropdownListBind>("API_meetinglocationList", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<DropdownListBind>> meetinglocation_List(string Event_pkey = "0")
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Event_pkey", Event_pkey)
            };
            List<DropdownListBind> list = await SqlHelper.ExecuteListAsync<DropdownListBind>("API_meetinglocation_List", CommandType.StoredProcedure, parameters);
            return list;
        }
        public async Task<List<EventAttending>> EventAttendingList(EventAttending li)
        {
            SqlParameter[] parameters = new SqlParameter[]
           {
            new SqlParameter("@Event_pkey", li.Event_pkey),
                new SqlParameter("@Account_pkey", li.Account_pkey)
           };
            List<EventAttending> list = await SqlHelper.ExecuteListAsync<EventAttending>("API_EventAttending_SelectAll", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<MeetingPlannerAllInvited>> MeetingPlannerAllInvited(MeetingPlannerAllInvited li)
        {
            SqlParameter[] parameters = new SqlParameter[]
           {
            new SqlParameter("@Event_pkey", li.Event_pkey),
                new SqlParameter("@Account_pkey", li.Account_pkey),
                   new SqlParameter("@MeetingPlanner_pkey", li.MeetingPlanner_pkey)
           };
            List<MeetingPlannerAllInvited> list = await SqlHelper.ExecuteListAsync<MeetingPlannerAllInvited>("API_MeetingPlanner_AllInvited", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<string> MeetingStatusChanges(MeetingPlannerSave li)
        {
            StatusCode obj = new StatusCode();

            obj.UList = new List<UserDetails>();
            SqlParameter[] parameters = new SqlParameter[]
          {
                new SqlParameter("@Account_pkey", li.Account_pkey),
                new SqlParameter("@MeetingPlanner_pkey", li.MeetingPlanner_pkey),
                new SqlParameter("@MeetingDetail_pkey", li.MeetingDetail_pkey),
                new SqlParameter("@Status", li.Status),
                new SqlParameter("@overlapping", li.overlapping)
          };

            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_MeetingStatus_Changes", CommandType.StoredProcedure, parameters));
            if (isInsert)
                return "changed";
            else
                return "Can't changed..";
        }

        public async Task<string> ActivityFeedbackForm_Update(Question_List li)
        {
            StatusCode obj = new StatusCode();

            obj.UList = new List<UserDetails>();
            SqlParameter[] parameters = new SqlParameter[]
          {
                new SqlParameter("@Account_pkey", li.Account_pkey),
                new SqlParameter("@pkey", li.pkey)

          };

            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_ActivityFeedbackForm_Update", CommandType.StoredProcedure, parameters));
            if (isInsert)
                return "changed";
            else
                return "Can't changed..";
        }

        public async Task<string> Attendee_Log_save(Attendee_Log li)
        {
            StatusCode obj = new StatusCode();


            SqlParameter[] parameters = new SqlParameter[]
          {
                new SqlParameter("@Account_pkey", li.Account_pkey),
                new SqlParameter("@EventOrganization_pkey", li.EventOrganization_pkey),
                new SqlParameter("@AttendeeLog", li.AttendeeLog),
                new SqlParameter("@InTime", li.InTime),
                 new SqlParameter("@Log_Pkey", li.Log_Pkey),
                new SqlParameter("@Event_Pkey", li.Event_Pkey),
                 new SqlParameter("@filename", li.filename),
                new SqlParameter("@BoothMessage", li.BoothMessage),

          };

            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_Attendee_Log", CommandType.StoredProcedure, parameters));
            if (isInsert)
                return "Log Updated";
            else
                return "Can't Update";
        }
        public async Task<string> Questions_SAVE(Question_List li)
        {
            StatusCode obj = new StatusCode();

            obj.UList = new List<UserDetails>();
            SqlParameter[] parameters = new SqlParameter[]
          {
                new SqlParameter("@EventSession_pkey", li.EventSession_pkey)
               , new SqlParameter("@Account_pkey", li.Account_pkey)
                , new SqlParameter("@XMLString", li.XMLString)

          };

            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_FeedbackForm_UserResponse_save", CommandType.StoredProcedure, parameters));
            if (isInsert)
                return "Polling Status changed  now";
            else
                return "Can't changed..";
        }

        public async Task<string> RegistrationQuestionResponce_save(RegistrationResponceSave li)
        {
            StatusCode obj = new StatusCode();

       
            SqlParameter[] parameters = new SqlParameter[]
          {
                new SqlParameter("@Account_pkey", li.Account_pkey)
               , new SqlParameter("@Event_pKey", li.Event_pKey)
                , new SqlParameter("@XMLString", li.XMLString)

          };

            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_RegistrationQuestionResponce_save", CommandType.StoredProcedure, parameters));
            if (isInsert)
                return "Registration question saved";
            else
                return "Registration question can't saved";
        }


        public async Task<List<DropdownListBind>> EventWiseActivityList(string Event_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
            new SqlParameter("@Event_pkey", Event_pkey)
            };
            List<DropdownListBind> list = await SqlHelper.ExecuteListAsync<DropdownListBind>("API_EventWiseActivity_Select", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<DropdownListBind>> EventSpeakerlist(string Event_pkey, string EventSession_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
            new SqlParameter("@Event_pkey", Event_pkey),
            new SqlParameter("@EventSession_pkey", EventSession_pkey)
            };
            List<DropdownListBind> list = await SqlHelper.ExecuteListAsync<DropdownListBind>("API_EventSpeaker_Select", CommandType.StoredProcedure, parameters);
            return list;
        }


        public async Task<string> SpeakerFeedBack(SpeakerFeedBack li)
        {
            StatusCode obj = new StatusCode();

            SqlParameter[] parameters = new SqlParameter[]
          {
                  new SqlParameter("@EventSession_pkey", li.EventSession_pKey)
                 ,new SqlParameter("@Account_pKey", li.Account_pKey)
                 ,new SqlParameter("@LogByAccount_pKey", li.LogByAccount_pkey)
                 ,new SqlParameter("@OnTopicScore", getScoreStr(li.OnTopicScore))
                 ,new SqlParameter("@ContentScore", getScoreStr(li.ContentScore))
                 ,new SqlParameter("@PresentationScore", getScoreStr(li.PresentationScore))
                 ,new SqlParameter("@SlidesScore",getScoreStr( li.SlidesScore))
                 ,new SqlParameter("@ValueScore", getScoreStr(li.ValueScore))
                 ,new SqlParameter("@LearningObjectivesMet", getYesNoStr(li.LearningObjectivesMet))
                 ,new SqlParameter("@Unbiased", getYesNoStr(li.Unbiased))
                 ,new SqlParameter("@Comment", li.Comment)
                 ,new SqlParameter("@AttendedPercent", li.AttendedPercent)
                 ,new SqlParameter("@MAGISuggestions", li.MAGISuggestions)
                 ,new SqlParameter("@SpeakerAdvice", li.SpeakerAdvice)
                 ,new SqlParameter("@GComment", li.GComment)
          };

            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_SpeakerFeedBack_Save", CommandType.StoredProcedure, parameters));
            if (isInsert)
                return "Polling Status changed  now";
            else
                return "Can't changed..";
        }
        private string getYesNoStr(string Val)
        {
            string getYesNoStr = "";
            switch (Convert.ToInt32(Val))
            {
                case 1:
                    {
                        getYesNoStr = "1";
                        break;
                    }

                case 2:
                    {
                        getYesNoStr = "0";
                        break;
                    }

                default:
                    {
                        getYesNoStr = "Null";
                        break;
                    }
            }

            return getYesNoStr;
        }
        private string getScoreStr(string txt)
        {
            string getScoreStr = "";
            if (txt != "")
                return getScoreStr = string.Format("{0:N0}", Convert.ToInt32(txt));
            else
                return getScoreStr = "Null";
        }

        //Created By :  Sagar Sharma
        #region Training_Resources
        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties by using reflection   
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names  
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
        public string SaveTrainingResource(Training_Resources Model)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@Event_Pkey", Model.Event_Pkey),
                new SqlParameter("@FileURL", Model.FileURL),
                new SqlParameter("@FileName", Model.FileName),
                new SqlParameter("@FileDescription", Model.FileDescription),
                new SqlParameter("@FileType", Model.FileType),
                new SqlParameter("@AudienceProperty", Model.AudienceProperty),
                new SqlParameter("@IsActive",Model.IsActive),
                new SqlParameter("@SortOrder",Model.SortOrder),
                new SqlParameter("@PageArea",Model.PageArea),
                new SqlParameter("@IsMyConsoleOnly",Model.IsMyConsoleOnly),
                new SqlParameter("@AutoPlay",Model.AutoPlay)
            };
            bool result = SqlHelper.ExecuteNonQuery("Save_TrainingResource", CommandType.StoredProcedure, parameters);
            if (result)
                return "Updated";
            else
                return "Title already exists";
        }
        public string UpdateTrainingResources(Training_Resources Model)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@ID", Model.ID),
                new SqlParameter("@Event_Pkey", Model.Event_Pkey),
                new SqlParameter("@FileURL", Model.FileURL),
                new SqlParameter("@FileName", Model.FileName),
                new SqlParameter("@FileDescription", Model.FileDescription),
                new SqlParameter("@FileType", Model.FileType),
                new SqlParameter("@AudienceProperty", Model.AudienceProperty),
                new SqlParameter("@IsActive",Model.IsActive),
                new SqlParameter("@SortOrder",Model.SortOrder),
                new SqlParameter("@PageArea",Model.PageArea),
                new SqlParameter("@IsMyConsoleOnly",Model.IsMyConsoleOnly),
                new SqlParameter("@AutoPlay",Model.AutoPlay)
            };
            bool result = SqlHelper.ExecuteNonQuery("Update_TrainingResource", CommandType.StoredProcedure, parameters);
            if (result)
                return "Updated";
            else
                return "Title already exists";
        }
        public string DeleteTrainingResource(string pKeys)
        {
            SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@ID", pKeys) };
            bool result = SqlHelper.ExecuteNonQuery("TrainingResource_Delete", CommandType.StoredProcedure, parameters);
            if (result)
                return "OK";
            else
                return "Some Error Occurred";
        }
        public DataTable GetResourceDataTable(string Title = null, string Type = null, string AudienceProperty = null, int? EventPkey = null)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@FileName", Title),
                new SqlParameter("@FileType", Type),
                new SqlParameter("@AudienceProperty", AudienceProperty),
                new SqlParameter("@Event_pKey", EventPkey),
            };
            return SqlHelper.ExecuteTable("getTrainingResourceList", CommandType.StoredProcedure, parameters);
        }
        public Training_Resources GetResounceInfo(int ID)
        {
            SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@ID", ID) };
            List<Training_Resources> list = SqlHelper.ExecuteList<Training_Resources>("TrainingResource_GetByID", CommandType.StoredProcedure, parameters);
            return list.FirstOrDefault();
        }
        public DataTable GetResourceData(int EventpKey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                  new SqlParameter("@Event_pKey",EventpKey),
            };
            return SqlHelper.ExecuteTable("SELECT pKey,FileName as title, FileDescription From  Training_Resources where Event_Pkey = @Event_pKey ORDER BY title ASC ", CommandType.Text, parameters);
        }
        #endregion

        #region EventStaff  
        public DataTable GetEventStaffList(int EventID, string StatusID = null, string SecurityGroupID = null, string PersonName = null, bool? Attending = null, string SecurityGroupSelected = "")
        {

            if (!string.IsNullOrEmpty(PersonName))
            {
                PersonName = PersonName.Trim();
            }
            SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@Event_pKey", EventID),
                new SqlParameter("@PersonName", PersonName),
                new SqlParameter("@StatusID", StatusID),
                new SqlParameter("@SecurityGroupID", SecurityGroupID),
                new SqlParameter("@SecurityGroup", SecurityGroupSelected),
                new SqlParameter("@ParticipationStatus",Attending), //List Of Staff Attending
            };
            return SqlHelper.ExecuteTable("getEventStaffList", CommandType.StoredProcedure, parameters);
        }
        public DataTable GetEventStaffDocumentList(int Event_pKey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                  new SqlParameter("@Event_pKey", Event_pKey),
                  new SqlParameter("@Action","List"),
            };
            return SqlHelper.ExecuteTable("EventDocuments_ExecuteAction", CommandType.StoredProcedure, parameters);
        }
        public string DeleteStaffDocumentsForEvent(string pKeys, int EventPKey, int Account_ID)
        {
            SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@Event_pKey", EventPKey),
                new SqlParameter("@Account_ID", Account_ID),
                new SqlParameter("@SelectedPKeys", pKeys),
                new SqlParameter("@Action","Delete"),
            };
            bool result = SqlHelper.ExecuteNonQuery("EventDocuments_ExecuteAction", CommandType.StoredProcedure, parameters);
            if (result)
                return "OK";
            else
                return "Some Error Occurred";
        }
        public string Insert_EventStaffDoc(StaffDocuments Model)
        {
            SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@Account_ID", Model.Account_ID),
                new SqlParameter("@Event_pKey", Model.Event_pKey),
                new SqlParameter("@Name", Model.Name),
                new SqlParameter("@Author", Model.Author),
                new SqlParameter("@EUserP", Model.EUserP),
                new SqlParameter("@LinkType", Model.LinkType),
                new SqlParameter("@LinkUrl", Model.LinkUrl),
                new SqlParameter("@FileGUID", Model.FileGUID),
                new SqlParameter("@Status", Model.Status),
                new SqlParameter("@Area_pkey", Model.Area_pkey),
                new SqlParameter("@Area", Model.Area),
                new SqlParameter("@Comments",Model.Comments),
                new SqlParameter("@Action","Insert"),
            };
            int result = SqlHelper.ExecuteScaler("EventDocuments_ExecuteAction", CommandType.StoredProcedure, parameters);
            if (result == 1)
                return "OK";
            else if (result == 0)
                return "Link Url already being used for the event";
            else
                return "Error";
        }
        public string Update_EventStaffDoc(StaffDocuments Model)
        {
            SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@Pkey", Model.Pkey),
                new SqlParameter("@Account_ID", Model.Account_ID),
                new SqlParameter("@Event_pKey", Model.Event_pKey),
                new SqlParameter("@Name", Model.Name),
                new SqlParameter("@Author", Model.Author),
                new SqlParameter("@EUserP", Model.EUserP),
                new SqlParameter("@LinkType", Model.LinkType),
                new SqlParameter("@LinkUrl", Model.LinkUrl),
                new SqlParameter("@FileGUID", Model.FileGUID),
                new SqlParameter("@Status", Model.Status),
                new SqlParameter("@Area_pkey", Model.Area_pkey),
                new SqlParameter("@Area", Model.Area),
                new SqlParameter("@Comments",Model.Comments),
                new SqlParameter("@Action","Update"),
            };
            int result = SqlHelper.ExecuteScaler("EventDocuments_ExecuteAction", CommandType.StoredProcedure, parameters);
            if (result == 1)
                return "OK";
            else if (result == 0)
                return "Link Url already being used for the event";
            else
                return "Error";
        }
        public string Insert_EventStaffRole(int Event_pKey, int ParticipantStatusPkey, int RolePKey, int StaffPKey)
        {
            SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@Event_pKey", Event_pKey),
                new SqlParameter("@ParticipantStatusPkey",ParticipantStatusPkey ),
                new SqlParameter("@RolePKey",RolePKey),
                new SqlParameter("@StaffPKey",StaffPKey),
                new SqlParameter("@Action","Insert"),
            };
            int result = SqlHelper.ExecuteScaler("EventStaff_ExecuteAction", CommandType.StoredProcedure, parameters);
            if (result == 1)
                return "OK";
            else if (result == 0)
                return "The selected combination is already assigned. Try again";
            else
                return "Error";
        }

        #endregion
        public DataTable GetAccountListWithUpdateProfileImage(string OrgID = null, string PadID = null, string Name = null, string Title = null)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@OrgID", OrgID),
                new SqlParameter("@PadID", PadID),
                new SqlParameter("@Name", Name),
                new SqlParameter("@Title", Title),
            };
            return SqlHelper.ExecuteTable("getUpdateProfileAccount", CommandType.StoredProcedure, parameters);
        }

        //public string UpdatePhotoUploadURL(string URL, int Account_pKey) 
        //{
        //    SqlParameter[] parameters = new SqlParameter[]  {
        //        new SqlParameter("@FileURL", URL),
        //        new SqlParameter("@Account_pKey", Account_pKey),
        //    };
        //    bool result = SqlHelper.ExecuteNonQuery("UpdateAccount_ProfilePhoto", CommandType.StoredProcedure, parameters);
        //    if (result)
        //        return "OK";
        //    else
        //        return "Some Error Occured";
        //}
        //public string RemovePhotoUploadURL(int Account_pKey)
        //{
        //    SqlParameter[] parameters = new SqlParameter[]  {
        //        new SqlParameter("@Account_pKey", Account_pKey),
        //    };
        //    bool result = SqlHelper.ExecuteNonQuery("UpdateAccount_RemoveProfilePhoto", CommandType.StoredProcedure, parameters);
        //    if (result)
        //        return "OK";
        //    else
        //        return "Some Error Occured";
        //}

        public string updaeAccount_UpdateAccept(int AccountpKey, int? UpdateImageAccepted = null)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@ID", AccountpKey),
                new SqlParameter("@UpdateImageAccepted", UpdateImageAccepted),
            };
            bool result = SqlHelper.ExecuteNonQuery("UpdateAccount_ImageAccepted", CommandType.StoredProcedure, parameters);
            if (result)
                return "OK";
            else
                return "Error Occurred";
        }
        public ReviewAccount GetAccountPhotoUpdateByID(int AccountPkey)
        {
            SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@ID", AccountPkey), };
            return SqlHelper.ExecuteList<ReviewAccount>("getUpdateProfileAccount_ByID", CommandType.StoredProcedure, parameters).FirstOrDefault();
        }
        public DataTable GetEventAccountsSocreList(int EventPkey, int Account_pKey, int TotalRecords)
        {
            SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@Account_pkey", Account_pKey),
                new SqlParameter("@Event_pKey", EventPkey),
                new SqlParameter("@Records", TotalRecords),
            };
            return SqlHelper.ExecuteTable("getAccountsPointsList", CommandType.StoredProcedure, parameters);
        }
        public SessionInfo GetEventSessionInfo(int SessionPKey, int EventPkey)
        {
            SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@ID", SessionPKey),
                new SqlParameter("@Event_pKey", EventPkey)
            };
            SessionInfo eventScoreList = SqlHelper.ExecuteList<SessionInfo>("getEventSessionInfoByID", CommandType.StoredProcedure, parameters).FirstOrDefault();
            return eventScoreList;
        }


        public DataTable GetEventSessionListByIDs(string SessionIDs, int EventPkey)
        {
            SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@SessionIDs", SessionIDs),
                new SqlParameter("@Event_pKey", EventPkey)
            };
            return SqlHelper.ExecuteTable("getEventSessionsInfoByIDs", CommandType.StoredProcedure, parameters);
        }

        public DataTable GetSmsDataTable(string smsid = null, string addedby = null, string SmsText = null, string Mobile = null, string Status = null, string dtFrom = null, string dtTo = null, int type = 0)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                 new SqlParameter("@SmsId", smsid),
                 new SqlParameter("@Addedby", addedby),
                new SqlParameter("@SmsText", SmsText),
                new SqlParameter("@Mobile", Mobile),
                new SqlParameter("@Status", Status),
                 new SqlParameter("@dtFrom", dtFrom),
                 new SqlParameter("@dtTo", dtTo),
                 new SqlParameter("@type", type),

            };
            DataTable dt1 = SqlHelper.ExecuteTable("GETSMSLIST", CommandType.StoredProcedure, parameters);
            return dt1;
        }

        public string UpdateSmsDataTable(SMS_List Model)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@pkey", Model.pkey),
                new SqlParameter("@SMSText", Model.SMSText),
                new SqlParameter("@MobileNumber", Model.MobileNumber),
                new SqlParameter("@Status", Model.Status),
                new SqlParameter("@SDate", Model.SentDate )

            };
            bool result = SqlHelper.ExecuteNonQuery("Update_SMSList", CommandType.StoredProcedure, parameters);
            if (result)
                return "Updated";
            else
                return "Check update data";
        }
        public string SaveSMSList(SMS_List Model)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@SMSID", Model.SMSID ),
                new SqlParameter("@SMSText", Model.SMSText ),
                new SqlParameter("@MobileNumber", Model.MobileNumber ),
                new SqlParameter("@Status", Model.Status),
                new SqlParameter("@SentDate", Convert.ToDateTime(Model.SentDate)),
                new SqlParameter("@Account_pkey", Model.Account_pkey ),
                  new SqlParameter("@AddedBy", Model.AddedBy ),

            };
            bool result = SqlHelper.ExecuteNonQuery("Save_SMSList", CommandType.StoredProcedure, parameters);
            if (result)
                return "SMS :" + Model.Status.ToString();
            else
                return "Check new data..";
        }


        public DataTable GetUserDataTable(int IsAttendee, string Id = null, string Name = null, string Title = null, string Email = null, string Mobile = null, string dtFrom = null, string dtTo = null, string Event_pkey = null, int Type = 0)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
               new SqlParameter("@IsValid",IsAttendee),
                new SqlParameter("@Id", Id),
                 new SqlParameter("@Name", Name),
                new SqlParameter("@Title", Title),
                  new SqlParameter("@Email", Email),
                new SqlParameter("@Mobile", Mobile),
                new SqlParameter("@dtFrom", dtFrom),
                 new SqlParameter("@dtTo", dtTo),
                   new SqlParameter("@Event_pkey", Event_pkey),
                  new SqlParameter("@notverify", Type)

            };

            DataTable dt1 = SqlHelper.ExecuteTable("GetUserList", CommandType.StoredProcedure, parameters);
            return dt1;

        }


        public string ValidMobile(User_List Model)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@pkey", Model.pkey),
                new SqlParameter("@IsMobile1", Model.IsMobile1),
                new SqlParameter("@IsMobile2", Model.IsMobile2),
                 new SqlParameter("@IAuto", Model.IAuto),

            };
            bool result = SqlHelper.ExecuteNonQuery("IsValidMobile", CommandType.StoredProcedure, parameters);

            if (result)
                return "Check Validation.";
            else
                return "Saved";
        }

        public string IsBothStatusUpdate(User_List Model)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@pkey", Model.pkey),
                new SqlParameter("@IsMobile1", Model.IsMobile1),
                new SqlParameter("@IsMobile2", Model.IsMobile2),
                 new SqlParameter("@IAuto", Model.IAuto),
                  new SqlParameter("@PhoneType_pkey", Model.PhoneType_pkey),
                   new SqlParameter("@PhoneType2_pkey", Model.PhoneType2_pkey),
                   new SqlParameter("@Type", Model.Type),

            };
            bool result = SqlHelper.ExecuteNonQuery("BothStatusUpdate", CommandType.StoredProcedure, parameters);

            if (result)
                return "Check Validation";
            else
                return "Saved";
        }


        public string IsBothRevierseUpdate(User_List Model)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@pkey", Model.pkey),
                new SqlParameter("@IsMobile1", Model.IsMobile1),
                new SqlParameter("@IsMobile2", Model.IsMobile2),
                 new SqlParameter("@IAuto", Model.IAuto),
                  new SqlParameter("@PhoneType_pkey", Model.PhoneType_pkey),
                   new SqlParameter("@PhoneType2_pkey", Model.PhoneType2_pkey),
                   new SqlParameter("@Type", Model.Type),

            };
            bool result = SqlHelper.ExecuteNonQuery("BothReverseStatusUpdate", CommandType.StoredProcedure, parameters);

            if (result)
                return "Check Validation";
            else
                return "Saved";
        }


        public async Task<Boolean> ConnectionrequestSave(String Event_pkey, String ConnectionAccount_pkey, string Account_pkey, string Msg, Boolean IncE, Boolean IncP, Boolean Request)
        {

            SqlParameter[] parameters = new SqlParameter[]
          {
                new SqlParameter("@ConnectionAccount_pkey", @ConnectionAccount_pkey),
                new SqlParameter("@Account_pkey",Account_pkey),
                new SqlParameter("@Msg", Msg),
                new SqlParameter("@IncE", IncE),
                new SqlParameter("@IncP", IncP),
                new SqlParameter("@Request", Request),
                new SqlParameter("@Event_pkey", Event_pkey)
                //new SqlParameter("@EventSession_pkey", EventSession_pkey)
                //new SqlParameter("@IncE", IncE)

          };
            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_ConnectionRequest_save", CommandType.StoredProcedure, parameters));

            if (isInsert)
                return true;
            else
                return false;

        }

        public async Task<string> API_Attendee_Questions_Save(string Question_pkey, string Question, string EventSession_pkey, string Account_pkey)
        {
            StatusCode obj = new StatusCode();


            SqlParameter[] parameters = new SqlParameter[]
          {
                new SqlParameter("@Question_pkey", Question_pkey),
                new SqlParameter("@Question",Question),
                new SqlParameter("@EventSession_pkey", EventSession_pkey),
                      new SqlParameter("@Account_pkey", Account_pkey)
          };

            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_Attendee_Questions_Save", CommandType.StoredProcedure, parameters));

            if (isInsert)
                return "Updated";
            else
                return "Can't Update..";
        }

        private int GetEventOrganizationPkey(int AccountPKey, int EventPKey)
        {
            string qry = "select top(1) ISNULL(t3.pKey,0)  as pKey  From account_list t2   Inner Join Event_Organizations t3 on t3.Organization_pKey = t2.ParentOrganization_pKey  "
              + System.Environment.NewLine + "where t2.pkey = " + AccountPKey.ToString() + " and t3.Event_pKey = " + EventPKey.ToString();
            DataTable dt = new DataTable();
            dt = SqlHelper.ExecuteTable(qry, CommandType.Text, null);
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                    return Convert.ToInt32(dt.Rows[0]["pKey"].ToString());
            }
            return 0;
        }
        private bool RefreshTHSettings(int EventPKey, int? EventOrg)
        {
            string qry = "select t1.pkey From Event_Organizations t1  Where t1.Event_pKey = " + EventPKey.ToString() + " and  ISNULL(t1.IsInTH,0) =1  and t1.ParticipationType_pKey in (1,5) and t1.pKey = " + EventOrg.ToString();
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            dt = SqlHelper.ExecuteTable(qry, CommandType.Text, null);
            if (dt != null)
                return (dt.Rows.Count > 0);

            return false;
        }
        public void UpdateSingleUserStampNotification(int AccountPKey, int EventPKey, int LoggedAccount = 0, int? EventOrganizationPKey = null)
        {
            clsAccount cAcc = new clsAccount();
            cAcc.sqlConn = new SqlConnection(SqlHelper.ReadConnectionString());
            cAcc.lblMsg = null;
            cAcc.intAccount_PKey = AccountPKey;

            if (EventOrganizationPKey == null)
                EventOrganizationPKey = GetEventOrganizationPkey(AccountPKey, EventPKey);

            if (RefreshTHSettings(EventPKey, EventOrganizationPKey))
            {
                if (cAcc.LoadAccount())
                {
                    string qry = "";
                    if (EventOrganizationPKey != null)
                        qry = " SELECT ISNULL(ThEmailBody,'Thank you for stamping your Treasure Hunt card.') as EmailBody From  Partner_Configuration  Where EventOrganization_pKey =" + EventOrganizationPKey.ToString();
                    else
                    {
                        qry = " select top(1)  ISNULL(t4.ThEmailBody,'Thank you for stamping your Treasure Hunt card.') as EmailBody From account_list t2  Inner Join Event_Organizations t3 on t3.Organization_pKey = t2.ParentOrganization_pKey  ";
                        qry = qry + System.Environment.NewLine + " Inner Join  Partner_Configuration t4 On t4.EventOrganization_pKey = t3.pKey where t2.pkey = " + LoggedAccount.ToString() + " and t3.Event_pKey = " + EventPKey.ToString();
                    }
                    DataTable dt = new DataTable();
                    string BodyMessage = "Thank you for stamping your Treasure Hunt card.";
                    dt = SqlHelper.ExecuteTable(qry, CommandType.Text, null);
                    if (dt != null)
                    {
                        if (dt.Rows.Count > 0)
                            BodyMessage = dt.Rows[0]["EmailBody"].ToString();
                    }
                    clsEmail cEmail = new clsEmail();
                    cEmail.sqlConn = new SqlConnection(SqlHelper.ReadConnectionString());
                    cEmail.lblMsg = null;
                    cEmail.strSubjectLine = "Thank you for stamping your Treasure Hunt card.";
                    cEmail.strHTMLContent = BodyMessage;
                    cEmail.strEmailCC = "#";
                    if (cAcc.intEmailUsed == 2)
                        cEmail.strEmailCC = cAcc.strEmail2;
                    cEmail.strEmailBCC = "#";
                    cEmail.SendEmailToAccount(cAcc, bPlainText: false);
                    cEmail = null;
                }
            }
        }


        private void NotificationUponRequestingStamp(SqlConnection sqlConn, int Account, DataTable dt)
        {
            bool ISTHSTAMPREQUEST = Convert.ToBoolean(dt.Rows[0]["IsTHRequest"].ToString());
            bool ISTHAttendee = Convert.ToBoolean(dt.Rows[0]["IsTHRequestAttendee"].ToString());

            if (ISTHAttendee || ISTHSTAMPREQUEST)
            {
                int SponsorPKey = Convert.ToInt32(dt.Rows[0]["Account_pKey"].ToString());

                clsAccount cWorkAccount = new clsAccount();
                cWorkAccount.sqlConn = sqlConn;
                cWorkAccount.lblMsg = null;
                cWorkAccount.intAccount_PKey = Account;
                cWorkAccount.LoadAccount();

                clsAccount cSponsorAccount = new clsAccount();
                cSponsorAccount.sqlConn = sqlConn;
                cSponsorAccount.lblMsg = null;
                cSponsorAccount.intAccount_PKey = SponsorPKey;
                cSponsorAccount.LoadAccount();

                clsEmail cEmail = new clsEmail();

                if (ISTHSTAMPREQUEST)
                {
                    //Sponsor Email Update
                    string SponsorMessage = "[AcctName], [Title], [AccDepartment], [AcctOrganization] has asked you to stamp their Treasure Hunt card. To stamp their card, [Link].";
                    string strSponsorSubject = "MAGI attendee has asked you to stamp their Treasure Hunt card";

                    SponsorMessage = Regex.Replace(dt.Rows[0]["RequestBody"].ToString(), @"<[^>]+>|&nbsp;", "").Trim();
                    if (string.IsNullOrEmpty(SponsorMessage.Trim()))
                        SponsorMessage = "[AcctName], [Title], [AccDepartment], [AcctOrganization] has asked you to stamp their Treasure Hunt card. To stamp their card, [Link].";

                    string SponsorHtmlText = cWorkAccount.ReplaceReservedWords(SponsorMessage);
                    SponsorHtmlText = SponsorHtmlText.Replace("[AccDepartment]", (string.IsNullOrEmpty(cWorkAccount.strDepartment) ? "" : cWorkAccount.strDepartment));
                    SponsorHtmlText = SponsorHtmlText.Replace("[Link]", "<a href='" + System.Configuration.ConfigurationManager.AppSettings["AppURL"].ToString().Replace("/forms", "") + "/MyConsole?RTab=0" + "' style='text-decoration:underline;'>click here</a>");
                    SponsorHtmlText = SponsorHtmlText.Replace(", ,", ",");

                    cEmail.lblMsg = null;
                    cEmail.strSubjectLine = strSponsorSubject;
                    cEmail.strHTMLContent = SponsorHtmlText;
                    if (!cEmail.SendEmailToAccount(cSponsorAccount, bPlainText: false))
                        return;
                }
                if (ISTHAttendee)
                {
                    string AttendeeMsg = "Thank you for your interest in [SponsorOrganizationName]. We will stamp your Treasure Hunt card. Do you have an interest in our products<br /><br /><br />[Primary_Contact_Signature]";
                    string AttendeeSubject = "Thank you for your interest in [SponsorOrganizationName]. We will stamp your Treasure Hunt card.";
                    AttendeeMsg = Regex.Replace(dt.Rows[0]["SponsorBody"].ToString(), @"<[^>]+>|&nbsp;", "").Trim();

                    if (string.IsNullOrEmpty(AttendeeMsg))
                        AttendeeMsg = "Thank you for your interest in [SponsorOrganizationName]. We will stamp your Treasure Hunt card. Do you have an interest in our products<br /><br /><br />[Primary_Contact_Signature]";

                    string SponsorSignature = cSponsorAccount.UserSignature(cSponsorAccount);
                    string SponsorOrganization = dt.Rows[0]["OrganizationID"].ToString();
                    string AttendeeMailHtml = AttendeeMsg.Replace("[SponsorOrganizationName]", SponsorOrganization);

                    AttendeeSubject = AttendeeSubject.Replace("[SponsorOrganizationName]", SponsorOrganization);
                    AttendeeMailHtml = AttendeeMailHtml.Replace("[Primary_Contact_Signature]", SponsorSignature);

                    cEmail = new clsEmail();
                    cEmail.lblMsg = null;
                    cEmail.strSubjectLine = AttendeeSubject;
                    cEmail.strHTMLContent = AttendeeMailHtml;
                    if (!cEmail.SendEmailToAccount(cWorkAccount, bPlainText: false))
                        return;
                }
            }
        }
        public void SendRequestStampNotificationMail(int AccountPKey, int EventPKey, int RequestingTo, int? EventOrganizationPKey = null)
        {
            if (EventOrganizationPKey == null)
                EventOrganizationPKey = 0;

            SqlParameter[] sqlParameters =
            {
                new SqlParameter("@RequestingTo", RequestingTo),
                new SqlParameter("@Account_pkey", AccountPKey),
                new SqlParameter("@EvtOrg_pkey", EventOrganizationPKey),
                new SqlParameter("@Event_pkey", EventPKey)
            };

            DataTable dt = SqlHelper.ExecuteTable("getStampReuqestSponsorsList", CommandType.StoredProcedure, sqlParameters);
            if (dt != null && dt.Rows.Count > 0)
            {
                NotificationUponRequestingStamp(new SqlConnection(SqlHelper.ReadConnectionString()), AccountPKey, dt);
            }
            clsReminders c = new clsReminders();
            c.UserReminderStatusUpdate(EventPKey, AccountPKey, clsReminders.R_TreasureHuntCard);
            c.UpdateReminderStatusByEvent(EventPKey, clsReminders.R_EventSponsorStamp.ToString());
        }



        public async Task<List<Meet>> Show_Sponsor_Schedule(string Event_pkey, string Account_pkey, string EventOrganizationpKey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Event_pkey", Event_pkey),
                new SqlParameter("@Account_pkey", Account_pkey),
                new SqlParameter("@EventOrganizationpKey", EventOrganizationpKey)

            };
            List<Meet> list = await SqlHelper.ExecuteListAsync<Meet>("API_Show_Sponsor_Schedule_Meeting", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<MeetingScheduleAvailable>> MeetingScheduleAvailable(string Event_pkey, string Account_pkey, string EventOrganizationpKey, string UserAccount_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@EventpKey", Event_pkey),
                new SqlParameter("@AccountpKey", Account_pkey),
                new SqlParameter("@EventOrganizationpKey", EventOrganizationpKey),
                new SqlParameter("@UserAccountpKey", UserAccount_pkey)

            };
            List<MeetingScheduleAvailable> list = await SqlHelper.ExecuteListAsync<MeetingScheduleAvailable>("AP_Show_Meeting_ScheduleAvailable", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<List<MeetingScheduleSave>> Updateeventattendee_schedule(string Event_pkey, string Account_pkey, string EventOrganizationpKey, string EventSponsorPerson_pKey, string BoothStaffSchedulepKey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Event_pKey", Event_pkey),
                new SqlParameter("@AccountpKey", Account_pkey),
                new SqlParameter("@EventOrganizationpKey", EventOrganizationpKey),
                new SqlParameter("@EventSponsorPerson_pKey", EventSponsorPerson_pKey),
                new SqlParameter("@BoothStaffSchedulepKey", BoothStaffSchedulepKey)


            };
            List<MeetingScheduleSave> list = await SqlHelper.ExecuteListAsync<MeetingScheduleSave>("API_Updateeventattendee_schedule", CommandType.StoredProcedure, parameters);
            return list;
        }

        public async Task<string> UpdateLicenseinfo(License li)
        {
            StatusCode obj = new StatusCode();


            SqlParameter[] parameters = new SqlParameter[]
            {
                    new SqlParameter("@AccountPKey", li.Account_pKey),
                    new SqlParameter("@LicenseNumber", li.LicenseNumber),
                    new SqlParameter("@LicenseType", li.LicenseType),
                    new SqlParameter("@LicenseState", li.LicenseState)

            };
            bool isInsert = (await SqlHelper.ExecuteNonQueryAsync("API_UpdateLicenseinfo", CommandType.StoredProcedure, parameters));
            //bool isInsert = true;
            if (isInsert)
                return "License Updated";
            else
                return "Can't Update License..";

        }
        public DataTable GetFormWithQuestion(int EventPkey, int AccountID)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]  {
                            new SqlParameter("@EventID",EventPkey),
                            new SqlParameter("@AccountID",AccountID),
                };
                return SqlHelper.ExecuteTable("GetRandomFormWithTestQuestions", CommandType.StoredProcedure, parameters);
            }
            catch
            {
                return null;
            }
        }
        public bool UpdateSolutions(int EventPkey, int AccountID, int FormsID, int Q_pKey, int FQ_PKey,string Response)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]  {
                  new SqlParameter("@Event_pKey",EventPkey),
                  new SqlParameter("@Account_pKey",AccountID),
                  new SqlParameter("@Forms_pKey",FormsID),
                  new SqlParameter("@Q_pKey",Q_pKey),
                  new SqlParameter("@FQ_pKey",FQ_PKey),
                  new SqlParameter("@Response",Response),
                };
                return SqlHelper.ExecuteNonQuery("UpdateExamSolutions", CommandType.StoredProcedure, parameters);
            }
            catch
            {
                return false;
            }
        }
        public DataTable CheckFormSolutionMarks(int EventPkey, int AccountID, int FormsID, int SettingMarks)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]  {
                  new SqlParameter("@AccountID",AccountID),
                  new SqlParameter("@EventID",EventPkey),
                  new SqlParameter("@FormID",FormsID),
                  new SqlParameter("@SettingMarks",SettingMarks),
                };
                return SqlHelper.ExecuteTable("FormSolutionsMarks", CommandType.StoredProcedure, parameters);
            }
            catch
            {
                return null;
            }
        }

     

        #region ReminderFunctions 
        public void UserReminderStatusUpdate(int EventPKey, int Account_pKey, int SettingValue)  //For Updating Reminder In Case Reminder Settings Are Updated / Re-updating Reminder based on account & Setting
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@Event_pKey", EventPKey),
                new SqlParameter("@Account_pKey",Account_pKey),
                new SqlParameter("@SettingValue",SettingValue),
                new SqlParameter("@Action",clsReminders.A_UpdateUserReminderStatus)
            };
            SqlHelper.ExecuteNonQuery("UpdateSavedReminder", CommandType.StoredProcedure, parameters);
        }
        #endregion 

        #region Community_Showcase
        public string SaveCommunityPost(CommunityContest Model)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@Account_pKey", Model.Account_pKey),
                new SqlParameter("@Event_pKey", Model.Event_pKey),
                new SqlParameter("@FileName", Model.FileName),
                new SqlParameter("@FileExtension", Model.FileExtension),
                new SqlParameter("@FileType", Model.FileType),
                new SqlParameter("@FileURL", Model.FileURL),
                new SqlParameter("@PostTitle", Model.PostTitle),
                new SqlParameter("@PostDescription",Model.PostDescription),
                new SqlParameter("@ContainsFile",Model.ContainsFile),
                new SqlParameter("@PostCategoryID",Model.PostCategory_pKey),
                new SqlParameter("@ActionType","Insert")
            };
            int result = SqlHelper.ExecuteScaler("Update_CommunityPost", CommandType.StoredProcedure, parameters);
            if (result == 1)
                return "OK";
            else if (result == 0)
                return "Make sure title is not Same";
            else if (result == 2)
                return "You have reached your daily limit on posts. Try again tomorrow.";
            else
                return "Error";
        }
        public string UpdateCommunityPost(CommunityContest Model)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@ID", Model.pKey),
                new SqlParameter("@FileName", Model.FileName),
                new SqlParameter("@FileExtension", Model.FileExtension),
                new SqlParameter("@FileType", Model.FileType),
                new SqlParameter("@FileURL", Model.FileURL),
                new SqlParameter("@PostTitle", Model.PostTitle),
                new SqlParameter("@PostDescription",Model.PostDescription),
                new SqlParameter("@ContainsFile",Model.ContainsFile),
                new SqlParameter("@PostCategoryID",Model.PostCategory_pKey),
                new SqlParameter("@UpdatedBy",Model.Account_pKey),
                new SqlParameter("@IsBlocked",Model.IsBlocked),
                new SqlParameter("@ActionType","Update")
            };
            int result = SqlHelper.ExecuteScaler("Update_CommunityPost", CommandType.StoredProcedure, parameters);
            if (result > 0)
                return "OK";
            else if (result == 0)
                return "Make sure title is not Same";
            else
                return "Error";
        }
        public DataTable getCommunityPostDataTable(int Account_pKey, int Event_pKey, string List_Type, int Size, int Number, string Category, int UserID, bool? BlockedStatus = null)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PageSize",Size),
                new SqlParameter("@PageNumber",Number),
                new SqlParameter("@Account_pKey",Account_pKey),
                new SqlParameter("@Event_pKey",Event_pKey),
                new SqlParameter("@ListType",List_Type),
                new SqlParameter("@ActionType","List"),
                new SqlParameter("@Category",Category),
                new SqlParameter("@UserID",UserID),
                new SqlParameter("@IsBlocked",BlockedStatus),
            };
            return SqlHelper.ExecuteTable("CommunityContest_ExecuteAction", CommandType.StoredProcedure, parameters);
        }
        public CommunityContest GetPostInfoByID(int ID, int Account_pKey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_pKey", Account_pKey),
                new SqlParameter("@ID", ID),
                new SqlParameter("@ActionType","Info"),
            };
            CommunityContest postInfo = SqlHelper.ExecuteList<CommunityContest>("CommunityContest_ExecuteAction", CommandType.StoredProcedure, parameters).FirstOrDefault();
            return postInfo;
        }
        public string DeleteCommunityPost(int pKey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
              new SqlParameter("@ID", pKey),
              new SqlParameter("@ActionType","Delete")
            };
            int result = SqlHelper.ExecuteScaler("CommunityContest_ExecuteAction", CommandType.StoredProcedure, parameters);
            if (result > 0)
                return "OK";
            else
                return "Error occurred while deleting post";
        }
        public string UpdatePostLike(int ID, int Account_pKey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ID", ID),
                new SqlParameter("@Account_pKey",Account_pKey)
            };
            int result = SqlHelper.ExecuteScaler("CommunityContest_UpdateLike", CommandType.StoredProcedure, parameters);
            if (result == 1)
                return "OK";
            else if (result == 0)
                return "You can not like your own post";
            else if (result == 2)
                return "You have reached your daily limit on liking posts. Try again tomorrow.";
            else
                return "Error occurred while liking post";
        }
        public string UpdateBlockStatus(int pKey, bool Status, int Account_pKey)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@ID", pKey),
                new SqlParameter("@UpdatedBy",Account_pKey),
                new SqlParameter("@IsBlocked",Status)
            };
            int result = SqlHelper.ExecuteScaler("Update_CommunityPostStatus", CommandType.StoredProcedure, parameters);
            if (result > 0)
                return "OK";
            else
                return "Error";
        }

        public string GetAlternateImage(int AccountPkey)
        {
            SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@ParamAccPKey", AccountPkey) };
            CommunityContest info = SqlHelper.ExecuteList<CommunityContest>("SP_GetAlternateImageByID", CommandType.StoredProcedure, parameters).FirstOrDefault();
            return info.AlternateImage;
        }
        public DataTable getLeaderBoardInformation(int Event_pKey, int Account_pKey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_pKey",Account_pKey),
                new SqlParameter("@Event_pKey",Event_pKey),
                new SqlParameter("@ActionType","LeaderBoard"),
            };
            return SqlHelper.ExecuteTable("CommunityContest_ExecuteAction", CommandType.StoredProcedure, parameters);
        }
        public string CheckPostLimit(int Account_pKey, int Event_pKey)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@Account_pKey", Account_pKey),
                new SqlParameter("@Event_pKey", Event_pKey),
                new SqlParameter("@ActionType","Check")
            };
            int result = SqlHelper.ExecuteScaler("Update_CommunityPost", CommandType.StoredProcedure, parameters);
            if (result == 1)
                return "OK";
            else if (result == 0)
                return "Make sure title is not Same";
            else if (result == 2)
                return "You have reached your daily limit on posts. Try again tomorrow.";
            else
                return "Error";
        }

        #endregion

        #region LinkEntryModule 
        public DataTable getLinkEntryList(int EventpKey, string GroupID = null, string Type = null, string URL = null, string DayFilter = null)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Event_pKey",EventpKey),
                new SqlParameter("@GroupID",GroupID),
                new SqlParameter("@Type",Type),
                new SqlParameter("@URL",URL),
                new SqlParameter("@ActionType","List"),
                new SqlParameter("@DayFilter",DayFilter),
            };
            return SqlHelper.ExecuteTable("LinkEntry_ExecuteAction", CommandType.StoredProcedure, parameters);
        }
        public int SaveLinkEntry(EventURL_List Model)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@Event_pKey",Model.EventpKey),
                new SqlParameter("@GroupID",Model.GroupID),
                new SqlParameter("@GroupLink", Model.GroupLink),
                new SqlParameter("@RecordingLink", Model.RecordingLink),
                new SqlParameter("@Type",Model.Type),
                new SqlParameter("@URL",Model.URL),
                new SqlParameter("@PrivateChatLink1",Model.PrivateChatLink1),
                new SqlParameter("@PrivateChatLink2",Model.PrivateChatLink2),
                new SqlParameter("@PrivateChatLink3",Model.PrivateChatLink3),
                new SqlParameter("@Account_pKey", Model.AccountPkey),
                new SqlParameter("@IsAvailable",Model.IsAvailable),
                new SqlParameter("@HostKey",Model.HostKey),
                new SqlParameter("@GroupTitle",Model.GroupTitle),
                new SqlParameter("@TP",Model.TP),
                new SqlParameter("@HostKey1",Model.HostKey1),
                new SqlParameter("@HostKey2",Model.HostKey2),
                new SqlParameter("@HostKey3",Model.HostKey3),
                new SqlParameter("@BaseURL",Model.BaseURL),
                new SqlParameter("@RecordType",Model.RecordType),
                new SqlParameter("@GroupLink1",Model.GroupLink1),
                new SqlParameter("@GroupLink2",Model.GroupLink2),
                new SqlParameter("@GroupLink3",Model.GroupLink3),
                new SqlParameter("@GroupLink4",Model.GroupLink4),
                new SqlParameter("@GroupLink5",Model.GroupLink5),
                new SqlParameter("@GroupLink6",Model.GroupLink6),
                new SqlParameter("@GroupLink7",Model.GroupLink7),
                new SqlParameter("@GroupLink8",Model.GroupLink8),
                new SqlParameter("@GroupLink9",Model.GroupLink9),
                new SqlParameter("@BreakOutHostKey2",Model.BreakOutHostKey2),
                new SqlParameter("@BreakOutHostKey3",Model.BreakOutHostKey3),
                new SqlParameter("@BreakOutHostKey4",Model.BreakOutHostKey4),
                new SqlParameter("@BreakOutHostKey5",Model.BreakOutHostKey5),
                new SqlParameter("@BreakOutHostKey6",Model.BreakOutHostKey6),
                new SqlParameter("@BreakOutHostKey7",Model.BreakOutHostKey7),
                new SqlParameter("@BreakOutHostKey8",Model.BreakOutHostKey8),
                new SqlParameter("@BreakOutHostKey9",Model.BreakOutHostKey9),
                new SqlParameter("@BreakOutHostKey10",Model.BreakOutHostKey10),
                new SqlParameter("@WebinarID",Model.WebinarID),
                new SqlParameter("@PIN",Model.PIN),
                new SqlParameter("@EntryKey",Model.EntryKey),
                new SqlParameter("@HallwayURL",Model.HallwayURL),
                new SqlParameter("@HallwayHostKey",Model.HallwayHostKey),
                new SqlParameter("@DiscussionStartDate",Model.DiscussionStartTime),
                new SqlParameter("@DiscussionEndDate",Model.DiscussionEndTime),
                new SqlParameter("@WebinarPwd",Model.WebinarPwd),
                new SqlParameter("@HallwayPwd",Model.HallwayPwd),
                new SqlParameter("@ActionType","Update"),
                //new SqlParameter("@WebinarStarted",Model.WebinarStarted),
                //new SqlParameter("@MeetingStarted",Model.MeetingStarted),
                //new SqlParameter("@WebinarInApp",Model.WebinarInApp),
                //new SqlParameter("@MeetingInApp",Model.MeetingInApp)
                new SqlParameter("@WebinarHost",Model.WebinarHost),
                new SqlParameter("@MeetingHost",Model.MeetingHost),
                //new SqlParameter("@LiveStreamURL",Model.LiveStreamURL),
            };
            int result = SqlHelper.ExecuteScaler("LinkEntry_ExecuteAction", CommandType.StoredProcedure, parameters);
            return result;
        }

        #endregion LinkEntryModule 

        #region  MeetingURL
        public DataTable MeetingURLS_List(int EventpKey, string Title = null, int? Type = null, int? HostType = null)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Event_pKey",EventpKey),
                new SqlParameter("@Title",Title),
                new SqlParameter("@Type",Type),
                new SqlParameter("@HostType",HostType),
                new SqlParameter("@ActionType","LIST"),
            };
            return SqlHelper.ExecuteTable("MeetingURL_ExecuteAction", CommandType.StoredProcedure, parameters);
        }
        public string MeetingURL_Add(MeetingURLS Model)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@Title",Model.Title),
                new SqlParameter("@URL",Model.URL),
                new SqlParameter("@HostKey",Model.HostKey),
                new SqlParameter("@Type",Model.Type),
                new SqlParameter("@HostType",Model.HostType),
                new SqlParameter("@IsRecurrent",Model.IsRecurrent),
                new SqlParameter("@IsScheduled",Model.IsScheduled),
                new SqlParameter("@IsActive",Model.IsActive),
                new SqlParameter("@Event_pKey",Model.Event_Pkey),
                new SqlParameter("@ActionType","Add")
            };
            int result = SqlHelper.ExecuteScaler("MeetingURL_ExecuteAction", CommandType.StoredProcedure, parameters);
            if (result > 0)
                return "OK";
            else if (result == 0)
                return "Record Already Exists With Similar Title";
            else
                return "Error";
        }
        public string MeetingURL_Update(MeetingURLS Model)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@pKey",Model.pKey),
                new SqlParameter("@Title",Model.Title),
                new SqlParameter("@URL",Model.URL),
                new SqlParameter("@HostKey",Model.HostKey),
                new SqlParameter("@Type",Model.Type),
                new SqlParameter("@HostType",Model.HostType),
                new SqlParameter("@IsRecurrent",Model.IsRecurrent),
                new SqlParameter("@IsScheduled",Model.IsScheduled),
                new SqlParameter("@IsActive",Model.IsActive),
                new SqlParameter("@Event_pKey",Model.Event_Pkey),
                new SqlParameter("@ActionType","EDIT")
            };
            int result = SqlHelper.ExecuteScaler("MeetingURL_ExecuteAction", CommandType.StoredProcedure, parameters);
            if (result > 0)
                return "OK";
            else if (result == 0)
                return "Record Already Exists With Similar Title";
            else
                return "Error";
        }
        public string MeetingURL_Delete(string pKeys)
        {
            SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@IDs", pKeys),
                new SqlParameter("@ActionType", "DELETE")
            };
            int result = SqlHelper.ExecuteScaler("MeetingURL_ExecuteAction", CommandType.StoredProcedure, parameters);
            if (result > 0)
                return "OK";
            else
                return "Some Error Occurred";
        }

        #endregion

        #region SponsorTHReport
        public DataTable GetEventSponsorTHResult(int Event_pKey, string Name = null, int? LevelID = null)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@Event_pKey",Event_pKey),
                new SqlParameter("@OrganizationID",Name),
                new SqlParameter("@Level_pKey",LevelID),
            };
            return SqlHelper.ExecuteTable("GetSponsorTHReportList", CommandType.StoredProcedure, parameters);
        }
        #endregion

        #region NetworkingReport
        public DataTable GetNetworkingReport(int Event_pKey, string Name = "", int ConnectionType = -1, string Organization = "", Boolean IsDemo = false, int OrganizationType_pkey = 0, int Page_pkey = -1, int Method_pkey = -1, int intEvtOrg_pkey = 0)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@EventpKey",Event_pKey),
                new SqlParameter("@Name",Name),
                new SqlParameter("@Organization",Organization),
                new SqlParameter("@ConnectionType",ConnectionType),
                new SqlParameter("@Date",clsEvent.getEventVenueTime()),
                new SqlParameter("@IsDemo",IsDemo),
                 new SqlParameter("@OrganizationType_pkey",OrganizationType_pkey),
                    new SqlParameter("@Page_pkey",Page_pkey),
                       new SqlParameter("@Method_pkey",Method_pkey),
                       new SqlParameter("@EvtOrg_pkey",intEvtOrg_pkey),
            };
            return SqlHelper.ExecuteTable("NetworkingReport1", CommandType.StoredProcedure, parameters);
        }

        public DataTable GetNetworkingMessage(int Event_pKey, string Name = "", string Organization = "", int Account_pkey = 0)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@Event_pkey",Event_pKey),
                new SqlParameter("@Name",Name),
                new SqlParameter("@Orgname",Organization),
                new SqlParameter("@Account_pkey",Account_pkey)

            };
            return SqlHelper.ExecuteTable("Networking_Messages_Select", CommandType.StoredProcedure, parameters);
        }

        #endregion

        #region SentinalReport
        public DataTable GetSentinalReport(int EventPkey, string ID = null, string word = null,string IDEntity = null)
        {
            int? pkey = null;
            if (!string.IsNullOrEmpty(ID))
                pkey = Convert.ToInt32(ID);

            int? EID = null;
            if (!string.IsNullOrEmpty(IDEntity))
                EID = Convert.ToInt32(IDEntity);


            if (string.IsNullOrEmpty(word))
                word = null;
            else
                word = word.Trim();
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@pKey",pkey),
                new SqlParameter("@Word",word),
                new SqlParameter("@strDescription",word),
                new SqlParameter("@ID",EID),
                new SqlParameter("@EventpKey",EventPkey)
            };
            return SqlHelper.ExecuteTable("sp_getAnnouncementResult", CommandType.StoredProcedure, parameters);
        }
        public DataTable GetSentinalRelatedAnnouncements(int EventPkey, int ID)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@EventpKey",EventPkey),
                new SqlParameter("@pKey",ID),
                new SqlParameter("@Action","GETINFO")
            };
            return SqlHelper.ExecuteTable("sp_getRelatedAnnouncement", CommandType.StoredProcedure, parameters);
        }
        public string UpdateNotesForSentinal(int ID, string Note)
        {
            SqlParameter[] parameters = new SqlParameter[]  {
                new SqlParameter("@pKey",ID),
                new SqlParameter("@Notes",Note),
                new SqlParameter("@Action","UPDATE")
            };

            int result = SqlHelper.ExecuteScaler("sp_getRelatedAnnouncement", CommandType.StoredProcedure, parameters);
            if (result > 0)
                return "OK";
            else
                return "Some Error Occurred";
        }
        #endregion

        #region ReminderSettings
        public DataTable GetReminderSettings(string EventVisibility, string EventOptions, string GenSearch)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                  new SqlParameter("@Action","List"),
                  new SqlParameter("@EventPages",EventOptions),
                  new SqlParameter("@GenSearch",GenSearch),
                  new SqlParameter("@EventVisibility",EventVisibility)
            };
            return SqlHelper.ExecuteTable("ReminderSettings_ExecuteAction", CommandType.StoredProcedure, parameters);
        }
        public string UpdateReminderSettings(Reminder_Settings Model)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                  new SqlParameter("@pKey",Model.Pkey),
                  new SqlParameter("@SettingID",Model.SettingID),
                  new SqlParameter("@Description",Model.Description),
                  new SqlParameter("@MessageText",Model.MessageText),
                  new SqlParameter("@ChairMessageText",Model.ChairMessageText),
                  new SqlParameter("@LeaderMessageText",Model.LeaderMessageText),
                  new SqlParameter("@SpeakerMessageText",Model.SpeakerMessageText),
                  new SqlParameter("@LinkText",Model.LinkText),
                  new SqlParameter("@LinkToolTip",Model.LinkToolTip),
                  new SqlParameter("@LinkURL",Model.LinkURL),
                  new SqlParameter("@ShowAt",Model.ShowAt),
                  new SqlParameter("@IsActive",Model.IsActive),
                  new SqlParameter("@IsLinkNewTab",Model.IsLinkNewTab),
                  new SqlParameter("@IsLinkNewTabOnVirtual",true),
                  new SqlParameter("@MultiText",Model.MultiText),
                  new SqlParameter("@MultiChairText",Model.MultiChairText),
                  new SqlParameter("@MultiLeaderText",Model.MultiLeaderText),
                  new SqlParameter("@MultiSpeakerText",Model.MultiSpeakerText),
                  new SqlParameter("@ShowTimes",Model.ShowTimes),
                  new SqlParameter("@IsHalfEvent",Model.IsHalfEvent),
                  new SqlParameter("@IsMobile",Model.IsMobile),
                  new SqlParameter("@Action","Update"),
            };
            int result = SqlHelper.ExecuteScaler("ReminderSettings_ExecuteAction", CommandType.StoredProcedure, parameters);
            if (result == 1)
                return "OK";
            else
                return "Error";
        }
        public string UpdateIsActiveReminderSettings(int pKey, bool IsActive, string ActionType, string SelectedpKeys = null)
        {
            string paramName = "";
            string pKeyParamName = ((ActionType == "ActivateAll") ? "@SelectedPKeys" : "@pKey");
            string pKeyValue = ((ActionType == "ActivateAll") ? SelectedpKeys : pKey.ToString());
            switch (ActionType)
            {
                case "ActivateAll":
                case "Activate":
                    paramName = "@IsActive";
                    break;
                case "NewTabVirtual":
                    paramName = "@IsLinkNewTabOnVirtual";
                    break;
                case "NewTabPage":
                    paramName = "@IsLinkNewTab";
                    break;
                case "IsMobile":
                    paramName = "@IsMobile";
                    break;
            }
            SqlParameter[] parameters = new SqlParameter[]
            {
                  new SqlParameter(pKeyParamName,pKeyValue),
                  new SqlParameter(paramName,IsActive),
                  new SqlParameter("@Action",ActionType)
            };
            int result = SqlHelper.ExecuteScaler("ReminderSettings_ExecuteAction", CommandType.StoredProcedure, parameters);
            if (result == 1)
                return "OK";
            else
                return "Error";
        }
        public DataTable GetReminderTips(int EventpKey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                  new SqlParameter("@Action","List"),
                  new SqlParameter("@Event_pKey",EventpKey),
            };
            return SqlHelper.ExecuteTable("ReminderTips_ExecuteAction", CommandType.StoredProcedure, parameters);
        }

        public string UpdateReminderTips(EventReminderTips Model, string Action)
        {
            SqlParameter[] parameters = new SqlParameter[]
           {
                  new SqlParameter("@pKey",Model.pKey),
                  new SqlParameter("@Tip",Model.Tip),
                  new SqlParameter("@ShowFrom",Model.ShowFrom),
                  new SqlParameter("@ShowTo",Model.ShowTo),
                  new SqlParameter("@Event_pKey",Model.EventpKey),
                  new SqlParameter("@Description",Model.Description),
                  new SqlParameter("@IsActive",Model.IsActive),
                  new SqlParameter("@Action",Action),
           };
            int result = SqlHelper.ExecuteScaler("ReminderTips_ExecuteAction", CommandType.StoredProcedure, parameters);
            if (result == 1)
                return "OK";
            else
                return "Error";
        }
        public string DeleteMultipleTips(string pKeys)
        {
            SqlParameter[] parameters = new SqlParameter[] {
             new SqlParameter("@ID", pKeys),
             new SqlParameter("@Action","DeleteAll")
            };
            bool result = SqlHelper.ExecuteNonQuery("ReminderTips_ExecuteAction", CommandType.StoredProcedure, parameters);
            if (result)
                return "OK";
            else
                return "Some Error Occurred";
        }
        public string DeleteTips(string pKey)
        {
            SqlParameter[] parameters = new SqlParameter[] {
             new SqlParameter("@pKey", pKey),
             new SqlParameter("@Action","Delete")
            };
            bool result = SqlHelper.ExecuteNonQuery("ReminderTips_ExecuteAction", CommandType.StoredProcedure, parameters);
            if (result)
                return "OK";
            else
                return "Some Error Occurred";
        }
        public DataTable GetReminderNotificationTips(int EventpKey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                  new SqlParameter("@Action","TipList"),
                  new SqlParameter("@Event_pKey",EventpKey),
            };
            return SqlHelper.ExecuteTable("ReminderTips_ExecuteAction", CommandType.StoredProcedure, parameters);
        }

        #endregion ReminderSettings

        #region UserAccountVerification_MVC
        private DataTable GetUserDataByAccountID(string AccountID) // AccountpKey
        {
            try
            {
                int intLockoutSecs = ((clsSettings)System.Web.HttpContext.Current.Session["cSettings"]).intLoginLockoutSecs;
                clsLastUsed cLast = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]);
                clsSettings cSettings = ((clsSettings)System.Web.HttpContext.Current.Session["cSettings"]);
                int EventPKey = 0;
                if (cLast != null && cSettings != null)
                    EventPKey = (cLast.intActiveEventPkey != 0 ? cLast.intActiveEventPkey : cSettings.intPrimaryEvent_pkey);
                else if (cLast != null)
                    EventPKey = ((cLast.intEventSelector == -1) ? cLast.intActiveEventPkey : cLast.intEventSelector);

                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@AccountID", AccountID),
                    new SqlParameter("@LockOutSecs", intLockoutSecs),
                    new SqlParameter("@EventpKey", EventPKey)
                };
                return SqlHelper.ExecuteTable("getUserDetailByAccountID_MVC", CommandType.StoredProcedure, parameters);
            }
            catch
            {
                return null;
            }
        }
        public User_Login GetUserLoginData(string AccountID)
        {
            try
            {
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(AccountID))
                {
                    dt = GetUserDataByAccountID(AccountID);

                    User_Login userinfo = new User_Login();
                    if (dt != null && dt.Rows.Count>0)
                    {
                        DataRow dr = dt.Rows[0];
                        userinfo.Result = Convert.ToInt32(AccountID);
                        userinfo.Id =Convert.ToInt32(AccountID);
                        userinfo.UserId = ((dr["UL"] == DBNull.Value) ? "" : dr["UL"].ToString());
                        userinfo.UserName = ((dr["ContactName"] == DBNull.Value) ? "" : dr["ContactName"].ToString());
                        userinfo.FirstName = ((dr["Firstname"] == DBNull.Value) ? "" : dr["Firstname"].ToString());
                        userinfo.MiddleName = ((dr["MiddleName"] == DBNull.Value) ? "" : dr["MiddleName"].ToString());
                        userinfo.LastName = ((dr["Lastname"] == DBNull.Value) ? "" : dr["Lastname"].ToString());
                        userinfo.NickName = ((dt.Rows[0]["Nickname"] == DBNull.Value) ? "" : dt.Rows[0]["Nickname"].ToString());
                        userinfo.Organization = ((dt.Rows[0]["organization"] == DBNull.Value) ? "" : dt.Rows[0]["organization"].ToString());
                        userinfo.Organization_Key = (dt.Rows[0]["organization_key"] == DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["organization_key"]);
                        userinfo.Email = ((dr["Email"] == DBNull.Value) ? "" : dr["Email"].ToString());
                        userinfo.EmailConfirmed = ((dr["Activated"] == DBNull.Value) ? false : Convert.ToBoolean(dr["Activated"]));
                        userinfo.EventId = ((dr["SettingValue"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["SettingValue"]));
                        userinfo.EventName = ((dr["EventFullName"] == DBNull.Value) ? "" : Convert.ToString(dr["EventFullName"]));
                        userinfo.EventLastDate = (dt.Rows[0]["EndDate"] == DBNull.Value) ? DateTime.Now : Convert.ToDateTime(dt.Rows[0]["EndDate"]);
                        userinfo.EventTypeId = ((dr["EventType_pkey"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["EventType_pkey"]));
                        userinfo.EventAccount_pkey = ((dr["EventAccount_pkey"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["EventAccount_pkey"]));
                        userinfo.ParticipationStatus_pKey = ((dr["ParticipationStatus_pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["ParticipationStatus_pKey"]));
                        userinfo.intLoginType = ((dr["LoginType"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["LoginType"].ToString()));
                        userinfo.GlobalAdmin = ((dr["GlobalAdministrator"] == DBNull.Value) ? false : Convert.ToBoolean(dr["GlobalAdministrator"]));
                        userinfo.StaffMember = ((dr["StaffMember"] == DBNull.Value) ? false : Convert.ToBoolean(dr["StaffMember"]));
                        userinfo.bAttendeeAtCurrEvent = ((dr["AttendeeAtCurrEvent"] == DBNull.Value) ? false : Convert.ToBoolean(dr["AttendeeAtCurrEvent"]));
                        userinfo.Is_Speaker = ((dr["Is_Speaker"] == DBNull.Value) ? false : Convert.ToBoolean(dr["Is_Speaker"]));
                        userinfo.ParentOrganization_pKey = ((dr["ParentOrganization_pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["ParentOrganization_pKey"]));
                    }
                    return userinfo;
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return null;
        }
        public void UpdateAuthEvent()
        {
            System.Web.HttpContext CurrentContext = System.Web.HttpContext.Current;
            System.Web.HttpCookie cookie = System.Web.Security.FormsAuthentication.GetAuthCookie(CurrentContext.User.Identity.Name, true);
            string AccountID = CurrentContext.User.Identity.Name;
            User_Login userinfo = null;
            if (!string.IsNullOrEmpty(AccountID))
                userinfo = GetUserLoginData(AccountID);
            if (userinfo!= null)
            {
                var ticket = System.Web.Security.FormsAuthentication.Decrypt(cookie.Value);
                string UserData = new JavaScriptSerializer().Serialize(userinfo);
                System.Web.Security.FormsAuthentication.SetAuthCookie(userinfo.Result.ToString(), true);
                var authTicket = new System.Web.Security.FormsAuthenticationTicket(1, userinfo.Result.ToString(), DateTime.Now, DateTime.MaxValue, true, UserData);
                string encryptedTicket = System.Web.Security.FormsAuthentication.Encrypt(authTicket);
                var authCookie = new System.Web.HttpCookie(System.Web.Security.FormsAuthentication.FormsCookieName, encryptedTicket);
                CurrentContext.Response.Cookies.Set(authCookie);
            }
        }

        public void LoginByAccountID(string AccountID, bool RememberMe)
        {
            User_Login userinfo = GetUserLoginData(AccountID);
            if (userinfo.Result > 1)
            {
                string UserData = new JavaScriptSerializer().Serialize(userinfo);
                System.Web.Security.FormsAuthentication.SetAuthCookie(userinfo.Result.ToString(), RememberMe);
                var authTicket = new System.Web.Security.FormsAuthenticationTicket(1, userinfo.Result.ToString(), DateTime.Now, DateTime.MaxValue, RememberMe, UserData);
                string encryptedTicket = System.Web.Security.FormsAuthentication.Encrypt(authTicket);
                var authCookie = new System.Web.HttpCookie(System.Web.Security.FormsAuthentication.FormsCookieName, encryptedTicket);
                System.Web.HttpContext.Current.Response.Cookies.Add(authCookie);
            }
        }
        public User_Login LoginByAccountIDFromCookie(string AccountID, bool RememberMe)
        {
            User_Login userinfo = GetUserLoginData(AccountID);
            if (userinfo.Result > 1)
            {
                string UserData = new JavaScriptSerializer().Serialize(userinfo);
                System.Web.Security.FormsAuthentication.SetAuthCookie(userinfo.Result.ToString(), RememberMe);
                var authTicket = new System.Web.Security.FormsAuthenticationTicket(1, userinfo.Result.ToString(), DateTime.Now, DateTime.MaxValue, RememberMe, UserData);
                string encryptedTicket = System.Web.Security.FormsAuthentication.Encrypt(authTicket);
                var authCookie = new System.Web.HttpCookie(System.Web.Security.FormsAuthentication.FormsCookieName, encryptedTicket);
                System.Web.HttpContext.Current.Response.Cookies.Add(authCookie);
                return userinfo;
            }
            return userinfo;
        }
        public void LogoutUpdateOnline(string SID)
        {
            try
            {
                string qry = "delete from sys_onlinePeople where [SessionId] = '" + SID+"';";
                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, null);
            }
            catch
            {
            }
        }
        public void LoginFromQA(string sAccountID, string sPW, bool RememberMe, System.Web.HttpRequest objRequest)
        {
            User_Login userinfo = new SqlOperation().AuthenticateLogin(sAccountID, sPW, objRequest);
            if (userinfo.Result > 1)
            {
                string UserData = new JavaScriptSerializer().Serialize(userinfo);
                System.Web.Security.FormsAuthentication.SetAuthCookie(userinfo.Result.ToString(), RememberMe);
                var authTicket = new System.Web.Security.FormsAuthenticationTicket(1, userinfo.Result.ToString(), DateTime.Now, DateTime.MaxValue, RememberMe, UserData);
                string encryptedTicket = System.Web.Security.FormsAuthentication.Encrypt(authTicket);
                var authCookie = new System.Web.HttpCookie(System.Web.Security.FormsAuthentication.FormsCookieName, encryptedTicket);
                System.Web.HttpContext.Current.Response.Cookies.Add(authCookie);
            }
        }
        private DataTable GetUserDataByEmail(string sAccountID)
        {
            try
            {
                int intLockoutSecs = ((clsSettings)System.Web.HttpContext.Current.Session["cSettings"]).intLoginLockoutSecs;
                clsLastUsed cLast = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]);
                int EventPKey = 0;
                if (cLast != null)
                    EventPKey = ((cLast.intEventSelector == -1) ? cLast.intActiveEventPkey : cLast.intEventSelector);

                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@UserName", sAccountID),
                    new SqlParameter("@LockOutSecs", intLockoutSecs),
                    new SqlParameter("@EventpKey", EventPKey)
                };
                return SqlHelper.ExecuteTable("getUserDetailByUsername", CommandType.StoredProcedure, parameters);
            }
            catch
            {
                return null;
            }
        }
        public User_Login AuthenticateLogin(string sAccountID, string sPW, System.Web.HttpRequest objRequest)
        {

            /*
                * --codes
                *--  0 =invalid password
                *-- -1=technical error
                *-- -2 duplicate references
                *-- -3 invalid account
                *-- -4 non active
                *-- -5 locked out
                *-- -6 password not set
                *-- > 0 valid login
            */
            User_Login user = new User_Login();
            user.Result = 0;
            //--make sure they entered something
            if (string.IsNullOrEmpty(sAccountID))
            {
                user.Result = -3;
                return user;
            }

            if (string.IsNullOrEmpty(sPW))
            {
                user.Result = 0;
                return user;
            }

            DataTable dt = GetUserDataByEmail(sAccountID);
            if (dt != null)
            {
                string CONNECTION_STRING = SqlHelper.ReadConnectionString();
                SqlConnection con = new SqlConnection(CONNECTION_STRING);
                int Count = dt.Rows.Count;
                if (Count == 1)
                {
                    user.intAccountBeingChecked = ((dt.Rows[0]["pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["pKey"].ToString()));
                    user.intLoginType = ((dt.Rows[0]["LoginType"] == DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["LoginType"].ToString()));
                    bool bPasswordSet = ((dt.Rows[0]["PSet"] == DBNull.Value) ? false : Convert.ToBoolean(dt.Rows[0]["PSet"].ToString()));
                    bool bAccountIsLockedOut = ((dt.Rows[0]["IsLocked"] == DBNull.Value) ? false : Convert.ToBoolean(dt.Rows[0]["IsLocked"]));
                    bool bBan = ((dt.Rows[0]["Ban"] == DBNull.Value) ? false : Convert.ToBoolean(dt.Rows[0]["Ban"]));
                    if (bBan)
                    {
                        user.Result = -10;
                        return user;
                    }
                    if (bAccountIsLockedOut)
                    {
                        user.Result = -5;
                        return user;
                    }
                    if (((dt.Rows[0]["UP"] == DBNull.Value) ? "" : dt.Rows[0]["UP"].ToString()) == "")
                    {
                        user.Result = -7;
                        return user;
                    }

                    bool valid = true;
                    if (Validate_Password(sPW, dt.Rows[0]["UP"].ToString()))
                    {
                        user.Id = user.intAccountBeingChecked;
                        user.Result = user.intAccountBeingChecked;
                        user.UserId = ((dt.Rows[0]["UL"] == DBNull.Value) ? "" : dt.Rows[0]["UL"].ToString());
                        user.UserName = ((dt.Rows[0]["ContactName"] == DBNull.Value) ? "" : dt.Rows[0]["ContactName"].ToString());
                        user.FirstName = ((dt.Rows[0]["Firstname"] == DBNull.Value) ? "" : dt.Rows[0]["Firstname"].ToString());
                        user.MiddleName = ((dt.Rows[0]["MiddleName"] == DBNull.Value) ? "" : dt.Rows[0]["MiddleName"].ToString());
                        user.LastName = ((dt.Rows[0]["Lastname"] == DBNull.Value) ? "" : dt.Rows[0]["Lastname"].ToString());
                        user.NickName = ((dt.Rows[0]["Nickname"] == DBNull.Value) ? "" : dt.Rows[0]["Nickname"].ToString());
                        user.Organization = ((dt.Rows[0]["organization"] == DBNull.Value) ? "" : dt.Rows[0]["organization"].ToString());
                        user.Email = ((dt.Rows[0]["Email"] == DBNull.Value) ? "" : dt.Rows[0]["Email"].ToString());
                        user.EmailConfirmed = ((dt.Rows[0]["Activated"] == DBNull.Value) ? false : Convert.ToBoolean(dt.Rows[0]["Activated"]));
                        user.EventId = ((dt.Rows[0]["SettingValue"] == DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["SettingValue"]));
                        user.EventCodeName = ((dt.Rows[0]["EventID"] == DBNull.Value) ? "" : dt.Rows[0]["EventID"].ToString());
                        user.EventName = ((dt.Rows[0]["EventFullName"] == DBNull.Value) ? "" : Convert.ToString(dt.Rows[0]["EventFullName"]));
                        user.EventLastDate = ((dt.Rows[0]["EndDate"] == DBNull.Value) ? new DateTime() : Convert.ToDateTime(dt.Rows[0]["EndDate"]));
                        user.EventTypeId = ((dt.Rows[0]["EventType_pkey"] == DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["EventType_pkey"]));
                        user.EventAccount_pkey = ((dt.Rows[0]["EventAccount_pkey"] == DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["EventAccount_pkey"]));
                        user.ParticipationStatus_pKey = ((dt.Rows[0]["ParticipationStatus_pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["ParticipationStatus_pKey"]));
                        user.GlobalAdmin = ((dt.Rows[0]["GlobalAdministrator"] == DBNull.Value) ? false : Convert.ToBoolean(dt.Rows[0]["GlobalAdministrator"]));
                        user.StaffMember = ((dt.Rows[0]["StaffMember"] == DBNull.Value) ? false : Convert.ToBoolean(dt.Rows[0]["StaffMember"]));
                        user.ParentOrganization_pKey = ((dt.Rows[0]["ParentOrganization_pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["ParentOrganization_pKey"]));
                    }
                    else
                    {
                        valid = false;
                        clsUtility.LogLoginFailure(con, null, clsAccount.FAILEDATTEMPT_LOGIN, objRequest, "Password mismatch", strUser: sAccountID);
                    }
                    if (!bPasswordSet && valid)
                    {
                        user.Result = -6;
                        return user;
                    }
                }
                else if (Count > 1)
                {
                    bool bAnyValid = false;
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr["UP"] != DBNull.Value)
                        {
                            if (Validate_Password(sPW, dr["UP"].ToString()))
                            {
                                user.Result = user.intAccountBeingChecked;
                                user.Id = user.intAccountBeingChecked;
                                user.Result = user.intAccountBeingChecked;
                                user.UserId = ((dr["UL"] == DBNull.Value) ? "" : dr["UL"].ToString());
                                user.UserName = ((dr["ContactName"] == DBNull.Value) ? "" : dr["ContactName"].ToString());
                                user.FirstName = ((dr["Firstname"] == DBNull.Value) ? "" : dr["Firstname"].ToString());
                                user.MiddleName = ((dr["MiddleName"] == DBNull.Value) ? "" : dr["MiddleName"].ToString());
                                user.LastName = ((dr["Lastname"] == DBNull.Value) ? "" : dr["Lastname"].ToString());
                                user.NickName = ((dt.Rows[0]["Nickname"] == DBNull.Value) ? "" : dt.Rows[0]["Nickname"].ToString());
                                user.Organization = ((dt.Rows[0]["organization"] == DBNull.Value) ? "" : dt.Rows[0]["organization"].ToString());
                                user.Email = ((dr["Email"] == DBNull.Value) ? "" : dr["Email"].ToString());
                                user.EmailConfirmed = ((dr["Activated"] == DBNull.Value) ? false : Convert.ToBoolean(dr["Activated"]));
                                user.EventId = ((dr["SettingValue"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["SettingValue"]));
                                user.EventName = ((dr["EventFullName"] == DBNull.Value) ? "" : Convert.ToString(dr["EventFullName"]));
                                user.EventTypeId = ((dr["EventType_pkey"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["EventType_pkey"]));
                                user.EventAccount_pkey = ((dr["EventAccount_pkey"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["EventAccount_pkey"]));
                                user.ParticipationStatus_pKey = ((dr["ParticipationStatus_pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["ParticipationStatus_pKey"]));
                                user.intLoginType = ((dr["LoginType"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["LoginType"].ToString()));
                                user.GlobalAdmin = ((dr["GlobalAdministrator"] == DBNull.Value) ? false : Convert.ToBoolean(dr["GlobalAdministrator"]));
                                user.StaffMember = ((dr["StaffMember"] == DBNull.Value) ? false : Convert.ToBoolean(dr["StaffMember"]));
                                user.ParentOrganization_pKey = ((dr["ParentOrganization_pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["ParentOrganization_pKey"]));
                                bAnyValid = true;
                                break;
                            }
                        }
                    }

                    if (!bAnyValid)
                    {
                        user.Result = -2;
                        return user;
                    }
                }
                else
                {
                    clsUtility.LogLoginFailure(con, null, clsAccount.FAILEDATTEMPT_LOGIN, objRequest, strUser: sAccountID);
                    user.Result = -3;
                    return user;
                }

            }
            else
            {
                user.Result = -1;
                return user;
            }

            return user;
        }
        public User_Login AuthenticatePasscode(string sAccountID, string sPasscode, System.Web.HttpRequest objRequest)
        {
            clsSettings cSettings = ((clsSettings)System.Web.HttpContext.Current.Session["cSettings"]);
            //--codes
            //--10= Passcode expires
            //-- 0=invalid password
            //-- -1=technical error
            //-- -2 duplicate references
            //-- -3 invalid account
            //-- -4 non active
            //-- -5 locked out
            //--  -6 password not set
            //-- >0 valid login
            User_Login user = new User_Login();
            user.Result = 0;
            //--make sure they entered something
            if (string.IsNullOrEmpty(sAccountID))
            {
                user.Result = -3;
                return user;
            }

            if (string.IsNullOrEmpty(sPasscode))
            {
                user.Result = 0;
                return user;
            }

            DataTable dt = GetUserDataByEmail(sAccountID);
            if (dt != null && dt.Rows.Count>0)
            {
                user.Email = ((dt.Rows[0]["Email"] == DBNull.Value) ? "" : dt.Rows[0]["Email"].ToString());
                //-make sure they entered something
                int intLockoutSecs = cSettings.intLoginLockoutSecs;
                int PasscodeExpiry = cSettings.PasscodeExpiry;
                string qry = "select t1.pkey,t1.Account_pkey,t1.Passcode,t1.CreatedTime,datediff(MINUTE,t1.CreatedTime,getdate()) As ExpMinute "
                    + System.Environment.NewLine+ " from UserLoginbyPasscode t1 where t1.UserEmail=@UserEmail";
                SqlParameter[] sqlParameters = new SqlParameter[] {
                        new SqlParameter("@UserEmail", user.Email)
                };
                DataTable dtPass = SqlHelper.ExecuteTable(qry, CommandType.Text, sqlParameters);
                if (dtPass!= null && dtPass.Rows.Count>0)
                {
                    int ExpMin = (dtPass.Rows[0]["ExpMinute"] != null) ? Convert.ToInt32(dtPass.Rows[0]["ExpMinute"]) : 0;
                    if (PasscodeExpiry < ExpMin)
                    {
                        user.Result = -10;
                        return user;
                    }
                    if (dtPass.Rows.Count == 1)
                    {
                        user.intAccountBeingChecked = ((dtPass.Rows[0]["pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["pKey"].ToString()));
                        //user.intLoginType = ((dtPass.Rows[0]["LoginType"] == DBNull.Value) ? 0 : Convert.ToInt32(dtPass.Rows[0]["LoginType"].ToString()));

                        if (((dtPass.Rows[0]["Passcode"] == DBNull.Value) ? "" : dtPass.Rows[0]["Passcode"].ToString()) == "")
                        {
                            user.Result = -7;
                            return user;
                        }
                        if (sPasscode == dtPass.Rows[0]["Passcode"].ToString())
                        {
                            int AcctPKey = (dtPass.Rows[0]["Account_pkey"] != null) ? Convert.ToInt32(dtPass.Rows[0]["Account_pkey"]) : 0;
                            user.Id = user.intAccountBeingChecked;
                            user.Result = user.intAccountBeingChecked;
                            user.UserId = ((dt.Rows[0]["UL"] == DBNull.Value) ? "" : dt.Rows[0]["UL"].ToString());
                            user.UserName = ((dt.Rows[0]["ContactName"] == DBNull.Value) ? "" : dt.Rows[0]["ContactName"].ToString());
                            user.FirstName = ((dt.Rows[0]["Firstname"] == DBNull.Value) ? "" : dt.Rows[0]["Firstname"].ToString());
                            user.MiddleName = ((dt.Rows[0]["MiddleName"] == DBNull.Value) ? "" : dt.Rows[0]["MiddleName"].ToString());
                            user.LastName = ((dt.Rows[0]["Lastname"] == DBNull.Value) ? "" : dt.Rows[0]["Lastname"].ToString());
                            user.NickName = ((dt.Rows[0]["Nickname"] == DBNull.Value) ? "" : dt.Rows[0]["Nickname"].ToString());
                            user.Organization = ((dt.Rows[0]["organization"] == DBNull.Value) ? "" : dt.Rows[0]["organization"].ToString());
                            user.Email = ((dt.Rows[0]["Email"] == DBNull.Value) ? "" : dt.Rows[0]["Email"].ToString());
                            user.EmailConfirmed = ((dt.Rows[0]["Activated"] == DBNull.Value) ? false : Convert.ToBoolean(dt.Rows[0]["Activated"]));
                            user.EventId = ((dt.Rows[0]["SettingValue"] == DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["SettingValue"]));
                            user.EventCodeName = ((dt.Rows[0]["EventID"] == DBNull.Value) ? "" : dt.Rows[0]["EventID"].ToString());
                            user.EventName = ((dt.Rows[0]["EventFullName"] == DBNull.Value) ? "" : Convert.ToString(dt.Rows[0]["EventFullName"]));
                            user.EventLastDate = ((dt.Rows[0]["EndDate"] == DBNull.Value) ? new DateTime() : Convert.ToDateTime(dt.Rows[0]["EndDate"]));
                            user.EventTypeId = ((dt.Rows[0]["EventType_pkey"] == DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["EventType_pkey"]));
                            user.EventAccount_pkey = ((dt.Rows[0]["EventAccount_pkey"] == DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["EventAccount_pkey"]));
                            user.ParticipationStatus_pKey = ((dt.Rows[0]["ParticipationStatus_pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["ParticipationStatus_pKey"]));
                            user.GlobalAdmin = ((dt.Rows[0]["GlobalAdministrator"] == DBNull.Value) ? false : Convert.ToBoolean(dt.Rows[0]["GlobalAdministrator"]));
                            user.StaffMember = ((dt.Rows[0]["StaffMember"] == DBNull.Value) ? false : Convert.ToBoolean(dt.Rows[0]["StaffMember"]));
                            user.ParentOrganization_pKey = ((dt.Rows[0]["ParentOrganization_pKey"] == DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["ParentOrganization_pKey"]));
                        }
                        else
                        {
                            string CONNECTION_STRING = SqlHelper.ReadConnectionString();
                            SqlConnection con = new SqlConnection(CONNECTION_STRING);
                            clsUtility.LogLoginFailure(con, null, clsAccount.FAILEDATTEMPT_LOGIN, objRequest, sAccountID);
                        }

                    }
                    else if (dtPass.Rows.Count>1)
                    {
                        user.Result = -2;
                        return user;
                    }
                    else
                    {
                        user.Result = -3;
                        return user;
                    }
                }
                else
                {
                    user.Result=-1;
                    return user;
                }
            }
            else
            {
                user.Result = -1;
                return user;
            }

            return user;
        }

        public string verifyLogin(string EmailID, string Password, bool RememberMe, System.Web.HttpRequest objRequest, out int Account_pKey)
        {
            Account_pKey =0;
            // --reset the error flag
            string ResultMessage = "Error Occured";
            if (string.IsNullOrEmpty(EmailID))
                return "Email Required";
            if (string.IsNullOrEmpty(Password))
                return "Password Required";

            User_Login userinfo = new SqlOperation().AuthenticateLogin(EmailID, Password, objRequest);
            int RetryCount = 0;
            if (System.Web.HttpContext.Current.Session["intRetryCount"] != null)
            {
                bool Exist = int.TryParse(System.Web.HttpContext.Current.Session["intRetryCount"].ToString(), out RetryCount);
            }
            clsAccount cAccount = new clsAccount();
            bool bError = false;
            if (userinfo.Result > 1)
            {
                string UserData = new JavaScriptSerializer().Serialize(userinfo);
                System.Web.Security.FormsAuthentication.SetAuthCookie(userinfo.Result.ToString(), RememberMe);
                var authTicket = new System.Web.Security.FormsAuthenticationTicket(1, userinfo.Result.ToString(), DateTime.Now, DateTime.MaxValue, RememberMe, UserData);
                string encryptedTicket = System.Web.Security.FormsAuthentication.Encrypt(authTicket);
                var authCookie = new System.Web.HttpCookie(System.Web.Security.FormsAuthentication.FormsCookieName, encryptedTicket);
                System.Web.HttpContext.Current.Response.Cookies.Add(authCookie);
                Account_pKey = userinfo.Id;
                return "OK";
            }
            else
            {
                bError = true;
            }

            userinfo.ErrorCode = 234; // --technical
            switch (userinfo.Result)
            {
                case 0:
                case -3: // --invalid account or password
                    userinfo.ErrorCode = 233;
                    break;
                case -2: // --multiple accounts found
                    userinfo.ErrorCode = 232;
                    break;
                case -4: // --inactive
                    userinfo.ErrorCode = 236;
                    break;
                case -5: // --locked out
                    userinfo.ErrorCode = 237;
                    break;
                case -6: return "Update Password";
                case -7: return "Create Password";
                case -10:
                    ResultMessage = "Your account has been blocked. Contact Support";
                    break;
            }
            userinfo.RetryCount = RetryCount + 1;
            string strLabel = clsUtility.getErrorMessage(userinfo.ErrorCode) + " (Try " + userinfo.RetryCount.ToString() + ")";
            System.Web.HttpContext.Current.Session["intRetryCount"] = userinfo.RetryCount;
            if (System.Configuration.ConfigurationManager.AppSettings["QAMode"].ToString() == "1")
                cAccount.LogAuditMessage("User ID: " + EmailID + " tried signing in " + userinfo.RetryCount.ToString() + " times, (Pass: " + Password + ") ", clsAudit.LOG_UserLoginAttempts);
            else
                cAccount.LogAuditMessage("User ID: " + EmailID + " tried signing in " + userinfo.RetryCount.ToString() + " times, ", clsAudit.LOG_UserLoginAttempts);

            clsSettings cSettings = ((clsSettings)System.Web.HttpContext.Current.Session["cSettings"]);
            int MaxAttemptsPasscode = ((cSettings.intLoginAttempts >= 5) ? 5 : cSettings.intLoginAttempts);
            if (userinfo.RetryCount == MaxAttemptsPasscode)
            {
                ResultMessage  = "SendCode";
                return ResultMessage;
                //int resultID = 0;
                //resultID = cAccount.InsertOneTimeLogin(EmailID, userinfo.intAccountBeingChecked);
                //String EncryptID = clsUtility.Encrypt(resultID.ToString());
                //SendOneTimeLogin(EmailID, userinfo.intAccountBeingChecked, EncryptID);
            }


            if (userinfo.RetryCount > cSettings.intLoginAttempts)
            {
                clsAccount.SetAccountLockout(userinfo.intAccountBeingChecked, null);
                ResultMessage = clsUtility.getErrorMessage(235);
            }
            ResultMessage = strLabel;
            return ResultMessage;
        }
        public string VerifyPasscodeLogin(string EmailID, string Passcode, System.Web.HttpRequest objRequest, out int Account_pKey)
        {
            Account_pKey =0;
            // --reset the error flag
            string ResultMessage = "Error Occured";
            if (string.IsNullOrEmpty(EmailID))
                return "Email Required";
            if (string.IsNullOrEmpty(Passcode))
                return "Passcode Required";

            User_Login userinfo = new SqlOperation().AuthenticatePasscode(EmailID, Passcode, objRequest);
            int RetryCount = 0;
            if (System.Web.HttpContext.Current.Session["intRetryCount"] != null)
            {
                bool Exist = int.TryParse(System.Web.HttpContext.Current.Session["intRetryCount"].ToString(), out RetryCount);
            }
            clsAccount cAccount = new clsAccount();
            bool bError = false;
            if (userinfo.Result > 1)
            {
                string UserData = new JavaScriptSerializer().Serialize(userinfo);
                System.Web.Security.FormsAuthentication.SetAuthCookie(userinfo.Result.ToString(), true);
                var authTicket = new System.Web.Security.FormsAuthenticationTicket(1, userinfo.Result.ToString(), DateTime.Now, DateTime.MaxValue, true, UserData);
                string encryptedTicket = System.Web.Security.FormsAuthentication.Encrypt(authTicket);
                var authCookie = new System.Web.HttpCookie(System.Web.Security.FormsAuthentication.FormsCookieName, encryptedTicket);
                System.Web.HttpContext.Current.Response.Cookies.Add(authCookie);
                Account_pKey = userinfo.Id;
                return "OK";
            }
            else
            {
                bError = true;
            }

            userinfo.ErrorCode = 234; // --technical
            switch (userinfo.Result)
            {
                case 0:
                case -3: // --invalid account or password
                         //userinfo.ErrorCode = 233;
                    ResultMessage = "Invalid passcode";
                    break;
                case -2: // --multiple accounts found
                    userinfo.ErrorCode = 232;
                    break;
                case -4: // --inactive
                    userinfo.ErrorCode = 236;
                    break;
                case -5: // --locked out
                    userinfo.ErrorCode = 237;
                    break;
                case -6: return "Update Password";
                case -7: return "Create Password";
                case -10:
                    ResultMessage = "Passcode has been expires.";
                    break;
            }
            userinfo.RetryCount = RetryCount + 1;
            string strLabel = clsUtility.getErrorMessage(userinfo.ErrorCode) + " (Try " + userinfo.RetryCount.ToString() + ")";
            System.Web.HttpContext.Current.Session["intRetryCount"] = userinfo.RetryCount;
            if (System.Configuration.ConfigurationManager.AppSettings["QAMode"].ToString() == "1")
                cAccount.LogAuditMessage("User ID: " + EmailID + " tried signing in " + userinfo.RetryCount.ToString() + " times, (Pass: " + Passcode + ") ", clsAudit.LOG_UserLoginAttempts);
            else
                cAccount.LogAuditMessage("User ID: " + EmailID + " tried signing in " + userinfo.RetryCount.ToString() + " times, ", clsAudit.LOG_UserLoginAttempts);

            clsSettings cSettings = ((clsSettings)System.Web.HttpContext.Current.Session["cSettings"]);
            if (userinfo.RetryCount > cSettings.intLoginAttempts && ResultMessage == "Error Occured")
            {
                clsAccount.SetAccountLockout(userinfo.intAccountBeingChecked, null);
                ResultMessage = clsUtility.getErrorMessage(235);
            }
            if (ResultMessage == "Error Occured")
                ResultMessage = strLabel;
            return ResultMessage;
        }
        private string SendOneTimeLogin(string strEMail, int Account_pKey, string ID)
        {
            // --identify person
            string CONNECTION_STRING = SqlHelper.ReadConnectionString();
            SqlConnection con = new SqlConnection(CONNECTION_STRING);
            clsAccount c = new clsAccount();
            c.sqlConn = con;
            c.lblMsg = null;
            c.intAccount_PKey = Account_pKey;
            c.strEmail = strEMail;
            if (clsUtility.CheckEmailFormat(strEMail))
                c.LoadAccountByEmail();
            else if (Account_pKey>0)
                c.LoadAccount();

            if (c.intAccount_PKey > 0)
            {
                clsAnnouncement Ann = clsAnnouncement.getAnnouncementInfo(con, null, clsAnnouncement.Text_LoginOneTIme);
                string strSubject = Ann.strTitle;
                string strContent = Ann.strHTMLText;
                strContent = strContent.Replace("[Url]", clsSettings.APP_URL().Replace("/forms", "") + "/Login?ULID=" + ID);
                strContent = strContent.Replace("[MAGI]", "MAGI");
                // --send email
                clsEmail cEmail = new clsEmail();
                cEmail.lblMsg = null;
                cEmail.strSubjectLine = strSubject;
                cEmail.strHTMLContent = strContent;
                if (!cEmail.SendEmailToAccount(c, bPlainText: false))
                    return "Error Occurred While Seding Help Mail";
                cEmail = null;
            }
            else
                return "No Account Found Related To Email To Send Login one Time";
            c = null;
            return "OK";
        }
        #endregion UserAccountVerification_MVC

        #region MySessionsPage_MVC
        public DataTable getMySessionsDataByID(int AccountPKey, int EventPKey, bool IsShowReleated, string Audience = null, string Days = null, string Tracks = null, string Levels = null, string Topics = null, string CertpKey = null)
        {
            try
            {

                Audience = (string.IsNullOrEmpty(Audience)) ? null : Audience;
                Days = (string.IsNullOrEmpty(Days)) ? null : Days;
                Tracks = (string.IsNullOrEmpty(Tracks)) ? null : Tracks;
                Levels = (string.IsNullOrEmpty(Levels)) ? null : Levels;
                Topics = (string.IsNullOrEmpty(Topics)) ? null : Topics;
                CertpKey = (string.IsNullOrEmpty(CertpKey)) ? null : CertpKey;
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Account_pKey", AccountPKey),
                    new SqlParameter("@Event_pKey", EventPKey),
                    new SqlParameter("@AudiencepKey", Audience),
                    new SqlParameter("@Days", Days),
                    new SqlParameter("@Tracks", Tracks),
                    new SqlParameter("@Levels", Levels),
                    new SqlParameter("@Topics", Topics),
                    new SqlParameter("@CertPkey",CertpKey)
                };
                return SqlHelper.ExecuteTable("getSessionDetailsByEventAccount", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
                //string message = ex.Message;
                //return null;
                throw new Exception(ex.ToString());
            }
        }
        public DataTable getEventSettingsForSessionByID(int EventPKey)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Event_pKey", EventPKey),
                };
                return SqlHelper.ExecuteTable("getEventSettingsForSessionByID", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                return null;
            }
        }
        public DateTime getEventVenueTime(string strStandardTimeZone)
        {
            TimeZoneInfo timeZoneInfo;
            if (string.IsNullOrEmpty(strStandardTimeZone))
            {
                timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time (Mexico)");
                return TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZoneInfo);
            }
            strStandardTimeZone = (strStandardTimeZone == "Eastern Daylight Time") ? "Eastern Standard Time" : strStandardTimeZone;
            timeZoneInfo = timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(strStandardTimeZone);
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZoneInfo);
        }

        //Sending Reuest For Attendee
        public string SendRequestforAttend(int pKey, int Session_pKey, string SessionID, bool CheckChanged, bool bAttend, bool bSlide, bool bWatch, DateTime ActDate, DateTime EndTime)
        {
            int EventPKey = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).intActiveEventPkey;
            int Account_pKey = 0, EventAccount_pKey = 0, intEventType_PKey = 0;

            System.Web.Security.FormsIdentity identity = (System.Web.Security.FormsIdentity)System.Web.HttpContext.Current.User.Identity;
            if (identity.AuthenticationType == "Forms")
            {
                User_Login data = new JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                Account_pKey = data.Id;
                EventAccount_pKey = data.EventAccount_pkey;
                intEventType_PKey = data.EventTypeId;
            }
            string Result = "OK";
            string ConnectionString = SqlHelper.ReadConnectionString();
            clsEventSession cEventSession = new clsEventSession();
            cEventSession.sqlConn = new SqlConnection(ConnectionString);
            cEventSession.intEventSession_PKey = pKey;
            cEventSession.intEvent_PKey = EventPKey;
            cEventSession.strSessionID = SessionID;


            if (!cEventSession.RequertForAttend(Account_pKey, bAttend, bSlide, pKey, EventAccount_pKey, intEventType_PKey, Session_pKey))
                Result = "Error";

            return Result;

        }
        public string UpdatecheckedValue(int pKey, string SessionID, bool CheckChanged, bool bAttend, bool bSlide, bool bWatch, DateTime ActDate, DateTime EndTime, bool bAttendRemote = false)
        {
            int EventPKey = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).intActiveEventPkey;
            int Account_pKey = 0, EventAccount_pKey = 0, intEventType_PKey = 0;

            System.Web.Security.FormsIdentity identity = (System.Web.Security.FormsIdentity)System.Web.HttpContext.Current.User.Identity;
            if (identity.AuthenticationType == "Forms")
            {
                User_Login data = new JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
                Account_pKey = data.Id;
                EventAccount_pKey = data.EventAccount_pkey;
                intEventType_PKey = data.EventTypeId;
                EventPKey = data.EventId;
            }
            string Result = "OK";
            string ConnectionString = SqlHelper.ReadConnectionString();
            clsEventSession cEventSession = new clsEventSession();
            cEventSession.sqlConn = new SqlConnection(ConnectionString);
            cEventSession.intEventSession_PKey = pKey;
            cEventSession.intEvent_PKey = EventPKey;
            cEventSession.strSessionID = SessionID;

            if ((bAttend || bAttendRemote) && cEventSession.CheckOverlappingSession(Account_pKey, ActDate, EndTime))
                Result = new clsSettings().getText(clsSettings.Text_OverlappingSession);

            if (!cEventSession.SetAttend(Account_pKey, bAttend, bSlide, bWatch, EventAccount_pKey, intEventType_PKey, bAttendRemote: bAttendRemote))
                Result = "Error";

            return Result;
        }
        public DataTable FetchSessionFilters(int Event_pKey, int ListType)
        {
            DataTable data = null;
            try
            {
                StringBuilder qry = new StringBuilder();
                switch (ListType)
                {
                    case 1: qry.Append("Select PA.pKey, PA.Audience_Id as strText From SYS_PrimaryAudience PA Where Isnull(IsActive,0) = 1 Order by Audience_Id"); break; //Primary Audience

                    case 2: //Certifications All
                        qry.Append(System.Environment.NewLine + "With tempQuery As (Select t0.pkey,CertAbbrev As strText, ChargeCode From Certification_List t0 INNER JOIN Certification_Detail t2 ON t2.Certification_pKey = t0.pKey Where t2.ShowOnCertifierList =1");
                        qry.Append(System.Environment.NewLine + " UNION  Select 1000 + pKey As pkey, ExamAbbrev As strText, ChargeCode From Exam_List  )");
                        qry.Append(System.Environment.NewLine + " Select  t0.pKey,t0.strText From tempQuery t0 ");
                        qry.Append(System.Environment.NewLine + " INNER Join Sys_ChargeTypes t1 ON t1.pKey = t0.ChargeCode And t1.pkey <> 9 ");
                        qry.Append(System.Environment.NewLine + " Left Outer Join Event_Pricing t2 on t2.ChargeType_pKey = t1.pKey And t2.Event_pKey = @Event_pKey");
                        qry.Append(System.Environment.NewLine + " Where t2.IsActive = 1 Order by t0.strText");
                        break;

                    case 3: // Prof Interest
                        qry.Append(" Select DISTINCT t2.pKey, t2.ProfInterestID as strText From Event_Sessions t0  INNER Join Session_ProfInterests t1 ON t1.Session_pKey= t0.Session_pKey INNER Join SYS_ProfInterests t2 ON t2.pKey = t1.ProfInterest_pKey");
                        qry.Append(System.Environment.NewLine + " Where isnull(t2.Enabled, 0) <> 0 AND t0.IsScheduled = 1 And t0.Event_pKey = @Event_pKey Order by strText");
                        break;

                    case 4: // Track Prefix 
                        qry.Append("Select DISTINCT t3.pKey,'('+  t3.Prefix + ') ' +  t3.TrackID as strText  From Event_Sessions t1  Inner Join Session_List t2 on t2.pkey = t1.Session_pKey  Inner Join sys_tracks t3 on t3.pkey = t1.track_pkey  Where  t3.educational = 1 AND t1.IsScheduled = 1");
                        qry.Append(System.Environment.NewLine + "AND  t1.Event_pKey = @Event_pKey Order by strText");
                        break;
                    case 5: qry.Append("SELECT pKey, EducationLevelID as strText FROM Sys_EducationLevels Order by strText"); break; // Education Levels

                    case 6: // Days
                        qry.Append(" DECLARE @startDate DATETIME,@endDate DATETIME,@dtSkipDate varchar(100);");
                        qry.Append(System.Environment.NewLine + " SELECT @startDate = t1.StartDate,@dtSkipDate=SkipRegDate,@endDate=EndDate FROM Event_List t1 where pkey= @Event_pKey;");
                        qry.Append(System.Environment.NewLine + " With dates As (SELECT @startdate as Date,DATENAME(Dw,@startdate) As DayName");
                        qry.Append(System.Environment.NewLine + " UNION ALL");
                        qry.Append(System.Environment.NewLine + " SELECT DATEADD(d,1,[Date]),DATENAME(Dw,DATEADD(d,1,[Date])) as DayName FROM dates WHERE DATE < @enddate )");
                        qry.Append(System.Environment.NewLine + " SELECT DayNum as pKey,lblDay as strText from dbo.getProgram(@Event_pKey)  Where lblDay Not In ");
                        qry.Append(System.Environment.NewLine + " (Select DayName + ', '+ FORMAT (Date, 'MMMM d') as lblDay from dates where Date in (Select String from CSVToStringTable(@dtSkipDate,',')))");
                        break;


                    case 7: // User Certificates Updated
                        string AccountPkey = System.Web.HttpContext.Current.User.Identity.Name;
                        qry.Append("SELECT chargeType_pkey  from Account_Charges where account_pkey = " + AccountPkey + " AND chargeType_pkey IN (2,3,17,18,19,20,41,43,49) And event_pkey = @Event_pKey");
                        qry.Append(System.Environment.NewLine + "And isNull(reversed, 0) = 0 And isNull(ReversalReference,0)=0  order by case when chargeType_pkey=17 then -1 else chargeType_pkey end");
                        break;

                    case 8: //DropDown PhoneType
                        qry.Append("select t1.pKey, t1.PhoneTypeID as strText from sys_PhoneTypes t1 order by strText");
                        break;
                    case 9: //Dial IN DropDown
                        qry.Append("Select pKey, DialInNumberID As strText FROM SYS_DialInNumber Where isnull(DialType,0)=3 ORDER BY SortOrder,DialInNumberID");
                        break;
                }
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Event_pKey", Event_pKey.ToString()),
                };
                if (!string.IsNullOrEmpty(qry.ToString()))
                {
                    data = SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
                }
            }
            catch
            {
                data = null;
            }
            return data;
        }
        public string UpdateContactHours(int EventPKey, int AccountpKey)
        {
            string result = string.Format("{0:N1}", 0);
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Account_pKey", AccountpKey),
                    new SqlParameter("@Event_pKey", EventPKey)
                };
                System.Data.DataTable dt = SqlHelper.ExecuteTable("UpdateAttendanceHours", CommandType.StoredProcedure, parameters);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        result = string.Format("{0:N1}", dt.Rows[0]["Results"]);
                    }
                    else
                    {
                        clsUtility.LogErrorMessage(null, null, null, 0, "Error recomputing att hours from create schedule.");
                    }
                }
            }
            catch
            {
                clsUtility.LogErrorMessage(null, null, null, 0, "Error recomputing att hours from create schedule.");
            }
            return result;
        }
        public int FindCertPKey(string strCertAbrv)
        {
            int result = 0;
            try
            {
                string qry = "SELECT pKey FROM Certification_List where  CertAbbrev = '" + strCertAbrv + "'";
                System.Data.DataTable dt = SqlHelper.ExecuteTable(qry, CommandType.Text, null);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        return Convert.ToInt32(dt.Rows[0][0]);
                    }
                }
            }
            catch
            {

            }
            return result;
        }
        public string RecomputeOneAccount(int intEvtAcctPKey, int intAcctpkey, int intCertPKey, int EventpKey, string strCertAbbrev)
        {
            string result = string.Format("{0:N1}", 0);
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@EvtAcctPKey", intEvtAcctPKey),
                    new SqlParameter("@AcctPKey", intAcctpkey),
                    new SqlParameter("@Event_pKey", EventpKey),
                    new SqlParameter("@Cert_pKey", intCertPKey),
                    new SqlParameter("@CertAbbrev", strCertAbbrev)
                };
                System.Data.DataTable dt = SqlHelper.ExecuteTable("UpdateCECertificationHours", CommandType.StoredProcedure, parameters);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        result = string.Format("{0:N1}", dt.Rows[0]["Results"]);
                    }
                    else
                    {
                        clsUtility.LogErrorMessage(null, null, null, 0, "Error recomputing att hours from create schedule.");
                    }
                }
            }
            catch
            {
                clsUtility.LogErrorMessage(null, null, null, 0, "Error recomputing att hours from create schedule.");
            }
            return result;
        }
        public DataTable GetOverLappingSessions(int EventpKey, int AccountpKey)
        {
            DataTable data = null;
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@AcctPKey", AccountpKey),
                    new SqlParameter("@Event_pKey", EventpKey),
                };
                return SqlHelper.ExecuteTable("MVC_GetOverLappingSessions", CommandType.StoredProcedure, parameters);
            }
            catch
            {
            }
            return data;
        }

        public DataTable QuestionExecutions(string ActionType, int? intEventSession_pkey = 0, int? AccountPKey = 0, int? QuestionID = 0)
        {
            try
            {
                string qry = "";
                switch (ActionType)
                {
                    case "RefreshQuestion":
                        qry = "Select t1.pkey,t1.Question,Case Isnull(t1.IsActive,0) When 1 Then 'Active' When 0 Then 'Inactive' Else '' End Status,Isnull(t1.Response,'') Response,Isnull(t1.IsActive,0) Active From Attendee_Questions t1"
                         + System.Environment.NewLine + "where t1.EventSession_pkey = " + intEventSession_pkey.ToString() + " and t1.Account_pkey = " + AccountPKey.ToString();
                        return SqlHelper.ExecuteTable(qry, CommandType.Text, null);
                    case "EditQuestion":
                        qry = "Select t1.Question From Attendee_Questions t1 where t1.pkey = @ID";
                        SqlParameter[] parameters = new SqlParameter[]
                        {
                            new SqlParameter("@ID", QuestionID.ToString()),
                        };
                        return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        public string SaveQuestionExecute(string Question, int? intEventSession_pkey = 0, int? AccountPKey = 0, int? QuestionID = 0, bool Schedule = false)
        {
            try
            {
                string qry = "";
                bool Executed = false;
                if (QuestionID <= 0)
                {
                    if (!Schedule)
                        qry = "Insert Into Attendee_Questions(Question,EventSession_pkey,Account_pkey) " + System.Environment.NewLine + " Values(@Question,@EventSession_pkey,@Account_pkey)";
                    else
                        qry = "Insert Into Attendee_Questions(Question,EventSession_pkey,Account_pkey,IsActive) " + System.Environment.NewLine + " Values(@Question,@EventSession_pkey,@Account_pkey,1)";

                    SqlParameter[] parameters = new SqlParameter[]
                       {
                            new SqlParameter("@Account_pkey", AccountPKey),
                            new SqlParameter("@EventSession_pkey", intEventSession_pkey),
                            new SqlParameter("@Question", Question)
                       };
                    Executed = SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
                }
                else
                {
                    qry = "Update Attendee_Questions Set Question=@Question where pkey = @QuestionID";
                    SqlParameter[] parameters = new SqlParameter[]
                       {
                           new SqlParameter("@QuestionID", QuestionID),
                           new SqlParameter("@Question", Question)
                       };
                    Executed = SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
                }
                return ((Executed) ? "OK" : "Error Occurred While Updating Question");
            }
            catch
            {
                return "Error Exception Occurred While Updating Question";
            }
        }

        public DataSet GetChatRelatedDetails(int accountKey, int eventKey)
        {
            try
            {
                string qry = string.Empty;

                qry = qry + Environment.NewLine + "select";
                qry = qry + Environment.NewLine + "isnull(e.IsDemo,0) as isDemoMode,";
                qry = qry + Environment.NewLine + "isnull(e.IsChatPanelOn,0) as IsChatPanelOn,ISNULL(IsShowChatNotification,0) AS IsShowChatNotification,";
                qry = qry + Environment.NewLine + "e.EndDate";
                qry = qry + Environment.NewLine + "from [Event_List] e where pKey = @eventKey;";

                qry = qry + Environment.NewLine + "select acc.GlobalAdministrator,";
                qry = qry + Environment.NewLine + "acc.StaffMember,";
                qry = qry + Environment.NewLine + "case when exists(select 1 from Event_Organizations eo where eo.Event_pKey=@eventKey ";
                qry = qry + Environment.NewLine + "and acc.ParentOrganization_pKey = eo.Organization_pkey) then 1 else 0 end as IsPartner";
                qry = qry + Environment.NewLine + "from Account_List acc";
                qry = qry + Environment.NewLine + "where acc.pKey = @accountKey;";

                qry = qry + Environment.NewLine + "select c.ID from sys_chats c where c.myID = @accountKey and c.Event_pkey = @eventKey and upper(c.ID) like 'ADMINGRP%'";

                SqlParameter[] parameters = new SqlParameter[]
                {
                        new SqlParameter("@accountKey", accountKey),
                        new SqlParameter("@eventKey", eventKey)
                };
                DataSet ds = new DataSet();
                ds = SqlHelper.ExecuteSet(qry, CommandType.Text, parameters);

                return ds;
            }
            catch
            {
                return null;
            }
            return null;
        }

        public Tuple<bool, DataSet> IsChatOn(int accountKey, int eventKey, bool isOnMyConsole)
        {
            try
            {
                bool Enable = false;

                string qry = string.Empty;
                qry = qry + Environment.NewLine + "select isnull((select isnull(e.Isnetworkingentirely,0) from event_accounts e";
                qry = qry + Environment.NewLine + "where e.Account_pKey = @accPkey and e.Event_pKey = @eventKey";
                qry = qry + Environment.NewLine + "and not exists(select 1 from BlockedPeopleForChat b";
                qry = qry + Environment.NewLine + "where b.AccountKey = @accPkey and b.EventKey = @eventKey";
                qry = qry + Environment.NewLine + "and isnull(b.IsBlocked,0)=1)";
                qry = qry + Environment.NewLine + "and e.ParticipationStatus_pKey=1 and (ISNULL(e.SingleSession_pKey,0)=0)),1)";
                qry = qry + Environment.NewLine + "as Isnetworkingentirely;";

                qry = qry + Environment.NewLine + "select";
                qry = qry + Environment.NewLine + "isnull(e.IsDemo,0) as isDemoMode,";
                qry = qry + Environment.NewLine + "isnull(e.showChatGadgetToStaff,0) as showChatGadgetToStaff,";
                qry = qry + Environment.NewLine + "isnull(e.IsChatOn,0) as IsChatOn,";
                qry = qry + Environment.NewLine + "e.EndDate,isnull(e.ChatPanelOnOffMyConsole,0) as ChatPanelOnOffMyConsole";
                qry = qry + Environment.NewLine + "from [Event_List] e where pKey = @eventKey;";

                qry = qry + Environment.NewLine + "select isnull(acc.GlobalAdministrator,0) as GlobalAdministrator,";
                qry = qry + Environment.NewLine + "isnull(acc.StaffMember,0) as StaffMember,";
                qry = qry + Environment.NewLine + "case when exists(select 1 from Event_Organizations eo where eo.Event_pKey=@eventKey ";
                qry = qry + Environment.NewLine + "and acc.ParentOrganization_pKey = eo.Organization_pkey) then 1 else 0 end as IsPartner";
                qry = qry + Environment.NewLine + "from Account_List acc";
                qry = qry + Environment.NewLine + "where acc.pKey = @accPkey;";

                qry = qry + Environment.NewLine + "select st.RegionCode,ISNULL(st.TimeOffset,4) as timeOffset,ISNULL(IsShowChatNotification,0) AS IsShowChatNotification from [Event_List] e inner join [StandardTime] st on st.Pkey = e.StandardTime_Pkey where e.pKey = @eventKey;";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@accPkey", accountKey),
                    new SqlParameter("@eventKey", eventKey)
                };

                DataSet ds = new DataSet();
                ds = SqlHelper.ExecuteSet(qry, CommandType.Text, parameters);

                DateTime dtEndDate = Convert.ToDateTime(ds.Tables[1].Rows[0]["EndDate"]);
                bool bShowDemoAccount = Convert.ToBoolean(ds.Tables[1].Rows[0]["isDemoMode"]);
                bool showChatGadgetToStaff = Convert.ToBoolean(ds.Tables[1].Rows[0]["showChatGadgetToStaff"]);
                bool IsChatOn = Convert.ToBoolean(ds.Tables[1].Rows[0]["IsChatOn"]);

                bool bGlobalAdministrator = Convert.ToBoolean(ds.Tables[2].Rows[0]["GlobalAdministrator"]);
                bool StaffMember = Convert.ToBoolean(ds.Tables[2].Rows[0]["StaffMember"]);
                bool IsPartner = Convert.ToBoolean(ds.Tables[2].Rows[0]["IsPartner"]);
                bool IsOptOut = Convert.ToBoolean(ds.Tables[0].Rows[0]["Isnetworkingentirely"]);
                bool bChatPanelOnOffMyConsole = Convert.ToBoolean(ds.Tables[1].Rows[0]["ChatPanelOnOffMyConsole"]);

                clsEvent cEvent = new clsEvent();
                cEvent.sqlConn = new SqlConnection(SqlHelper.ReadConnectionString());

                if (bChatPanelOnOffMyConsole)
                    bChatPanelOnOffMyConsole = cEvent.CheckValiditityOfModule(eventKey, "ChatPanelOnOffMyConsole");

                if(showChatGadgetToStaff)
                    showChatGadgetToStaff = cEvent.CheckValiditityOfModule(eventKey, "showChatGadgetToStaff");

                if (dtEndDate.AddDays(1).Date < clsEvent.getEventVenueTime()
                &&
                ((clsEvent.getEventVenueTime() - dtEndDate.AddDays(1).Date).Days > 14))
                {
                    Enable = false;
                }
                else
                {
                    Enable = (bShowDemoAccount && (bGlobalAdministrator || IsPartner) && !IsOptOut) ? true :
                             (IsPartner && bChatPanelOnOffMyConsole && isOnMyConsole && !IsOptOut) ? true :
                             (showChatGadgetToStaff && (StaffMember || bGlobalAdministrator) && !IsOptOut) ? true :
                             (IsChatOn && !IsOptOut) ? true : false;
                }

                return Tuple.Create(Enable, ds);
            }
            catch (Exception ex)
            {
                return Tuple.Create(false, new DataSet());
            }
        }

        public string[] GeNetworkingLevel(int accountKey, int eventKey)
        {
            try
            {
                string[] strResult = new string[4];
                string qry = string.Empty;
                qry = qry + Environment.NewLine + "EXEC sp_getpointsAndNetLevel  @demoMode,@accPkey,@eventKey,@currentDate;";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@demoMode", false),
                    new SqlParameter("@accPkey", accountKey),
                    new SqlParameter("@eventKey", eventKey),
                    new SqlParameter("@currentDate", DateTime.Now)
                };

                DataTable dt = new DataTable();
                dt = SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);

                if (dt.Rows.Count > 0)
                {
                    bool bExists = Convert.ToBoolean(dt.Rows[0]["hasimage"]);
                    bool bioExists = Convert.ToBoolean(dt.Rows[0]["hasBio"]);
                    float AttainedPoints = Convert.ToInt32(dt.Rows[0]["AttainedPoints"]);
                    string levelName = "";
                    string netLevelKey = "";
                    string awardedBagde = "";
                    Decimal PointsPerlevelAttained = 0, PointsOfToplevel = 0;
                    int PeopleAtNextLevel = 0;
                    string StrNextLevel = "";
                    string ToolTipForBadge = "Network with people to advance to the next level";

                    int nextLevel = 0;
                    int overallrows = dt.Rows.Count;

                    for (int i = 0; i <= overallrows - 1; i++)
                    {
                        if (AttainedPoints < Convert.ToInt32(dt.Rows[i]["pnts"]))
                            break;

                        levelName = "";
                        nextLevel = i + 1;
                        nextLevel = (overallrows > nextLevel) ? nextLevel : 0;

                        if (Convert.ToBoolean(dt.Rows[i]["imgReq"]) || Convert.ToBoolean(dt.Rows[i]["BioReq"]))
                        {
                            StrNextLevel = dt.Rows[i]["lvl"].ToString();

                            if (!bExists && !bioExists)
                            {
                                levelName = AttainedPoints.ToString() + " networking points" + "^Profile image and bio data required for " + StrNextLevel + " level";
                                break;
                            }
                            else if (!bExists)
                            {
                                levelName = AttainedPoints.ToString() + " networking points^profile image required for " + StrNextLevel + " level";
                                break;
                            }
                            else if (!bioExists)
                            {
                                levelName = AttainedPoints.ToString() + " networking points^bio data required for " + StrNextLevel + " level";
                                break;
                            }

                            awardedBagde = dt.Rows[i]["lvl"].ToString();
                            netLevelKey = dt.Rows[i]["levelKey"].ToString();
                            PeopleAtNextLevel = Convert.ToInt32(dt.Rows[nextLevel]["cnt"]);
                        }

                        PointsPerlevelAttained = Convert.ToDecimal(dt.Rows[i]["AttainedPoints"]);
                        awardedBagde = dt.Rows[i]["lvl"].ToString();
                        netLevelKey = dt.Rows[i]["levelKey"].ToString();

                        if (nextLevel != 0)
                        {
                            levelName = AttainedPoints.ToString() + " networking points";
                        }
                        else
                        {
                            ToolTipForBadge = "Congratulations! You have reached the highest networking level";
                            levelName = AttainedPoints.ToString() + " networking points^";
                        }

                        PeopleAtNextLevel = Convert.ToInt32(dt.Rows[nextLevel]["cnt"]);
                    }

                    PointsOfToplevel = Convert.ToDecimal(dt.Rows[overallrows - 1]["pnts"]);

                    int rank = Convert.ToInt32(Math.Ceiling((PointsPerlevelAttained / PointsOfToplevel) * 100));
                    rank = 100 - rank;
                    rank = (rank <= 0 ? 1 : rank);

                    if (nextLevel != 0)
                    {
                        strResult[0] = levelName + "^Top " + rank.ToString() + "%" + " of networkers^" + (PeopleAtNextLevel == 0 ? "" : (PeopleAtNextLevel.ToString() + " " + (PeopleAtNextLevel > 1 ? "people are" : "person is") + " at the next level"));
                    }
                    else
                    {
                        strResult[0] = levelName + "^Top " + rank.ToString() + "%" + " of networkers";
                    }

                    if (!string.IsNullOrEmpty(awardedBagde))
                    {
                        awardedBagde = "/images/icons/" + awardedBagde + ".png";
                        strResult[1] = awardedBagde;
                        strResult[2] = ToolTipForBadge;
                    }
                    else
                    {
                        strResult[1] = "";
                        strResult[2] = "";
                    }

                    strResult[3] = netLevelKey;
                }
                return strResult;
            }
            catch
            {
                return null;
            }
        }

        public Tuple<bool, string, DataSet> IsChatPanelON(int accountKey, int eventKey, string URL, bool bEventAccess)
        {
            try
            {
                DataSet ds = GetChatRelatedDetails(accountKey, eventKey);
                bool bShowDemoAccount = false,
                    bGlobalAdministrator = false,
                    bIsPartner = false,
                    bChatPanelOnOff = false,
                    bStaffMember = false;

                DateTime dtEndDate = new DateTime();

                string InterestBasedGroup = string.Empty;

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    bShowDemoAccount = Convert.ToBoolean(ds.Tables[0].Rows[0]["isDemoMode"]);
                    bChatPanelOnOff = Convert.ToBoolean(ds.Tables[0].Rows[0]["IsChatPanelOn"]);
                    dtEndDate = Convert.ToDateTime(ds.Tables[0].Rows[0]["EndDate"]);
                }

                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                {
                    bGlobalAdministrator = Convert.ToBoolean(ds.Tables[1].Rows[0]["GlobalAdministrator"]);
                    bStaffMember = Convert.ToBoolean(ds.Tables[1].Rows[0]["StaffMember"]);
                    bIsPartner = Convert.ToBoolean(ds.Tables[1].Rows[0]["IsPartner"]);
                }

                if (ds.Tables.Count > 2 && ds.Tables[2].Rows.Count > 0)
                {
                    InterestBasedGroup = ds.Tables[2].Rows[0]["ID"].ToString();
                }

                bool Enable = (bShowDemoAccount && (bGlobalAdministrator || bIsPartner)) ?
                true : bChatPanelOnOff;

                if (dtEndDate.AddDays(1).Date < clsEvent.getEventVenueTime()
                &&
                ((clsEvent.getEventVenueTime() - dtEndDate.AddDays(1).Date).Days > 14)
                )
                {
                    Enable = false;
                }
                else
                {
                    if ((URL.Contains("ZOOMSESSION") || URL.Contains("SPEAKERLEFTPANEL"))
                        && (bGlobalAdministrator || bStaffMember || bEventAccess))
                    {
                        Enable = true;
                    }
                }

                DataSet DataForChatPanel = new DataSet();
                DataForChatPanel = GetPeopleForChatPanel(accountKey, eventKey, bShowDemoAccount, bGlobalAdministrator, bIsPartner);
                DataForChatPanel.Tables.Add(ds.Tables[0]);
                return Tuple.Create(Enable, InterestBasedGroup, DataForChatPanel);
            }
            catch
            {
                return Tuple.Create(false, string.Empty, new DataSet());
            }
        }

        public DataSet GetPeopleForChatPanel(int accountKey, int eventKey, bool bShowDemoAccount, bool bGlobalAdministrator, bool bIsPartner)
        {
            try
            {
                string qry = "EXEC SP_GetPeopleForNotificationPanel @ParamId,@paramEvent_pkey,@paramViewStateKey,@currentDate,@isForDemo;";

                qry = qry + Environment.NewLine + "select distinct n.pKey,n.topicName as strText from";
                qry = qry + Environment.NewLine + "sys_networkingtopic n where ISNULL(n.IsActive,0) = 1 order by n.topicName;";
                qry = qry + Environment.NewLine + "select st.RegionCode,ISNULL(st.TimeOffset,4) as timeOffset from [Event_List] e inner join [StandardTime] st on st.Pkey = e.StandardTime_Pkey where e.pKey = @paramEvent_pkey;";

                SqlParameter[] parameters = new SqlParameter[]
                {
                        new SqlParameter("@ParamId", accountKey),
                        new SqlParameter("@paramEvent_pkey", eventKey),
                        new SqlParameter("@paramViewStateKey", -1),
                        new SqlParameter("@currentDate", clsEvent.getEventVenueTime()),
                        new SqlParameter("@isForDemo", (bShowDemoAccount && (bGlobalAdministrator || bIsPartner)) ? true : false)
                };
                DataSet ds = new DataSet();
                ds = SqlHelper.ExecuteSet(qry, CommandType.Text, parameters);

                return ds;
            }
            catch
            {
                return null;
            }
        }

        #endregion  MySessionsPage_MVC

        #region MySchedulePage_MVC
        public int OffsetVenue(int EventPKey)
        {
            int offsetVenue = 0;
            try
            {
                string qry = "DECLARE @TimeOffSET int = 0; SELECT @TimeOffSET  = CAST(TimeOffset as int) From  Event_List INNER JOIN StandardTime ON StandardTime.Pkey = Event_List.StandardTime_Pkey  Where Event_List.pKey = @Event_pKey; SELECT @TimeOffSET as TimeOffset ";
                SqlParameter[] parameters = new SqlParameter[]
                {
                new SqlParameter("@Event_pKey", EventPKey),
                };
                DataTable dt = SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
                if (dt!= null  && dt.Rows.Count>0)
                {
                    if (dt.Rows[0]["TimeOffset"] != System.DBNull.Value)
                        int.TryParse(dt.Rows[0]["TimeOffset"].ToString(), out offsetVenue);
                }
            }
            catch
            {
            }
            return offsetVenue;
        }
        public bool CheckCalnderIfExists(int EventpKey, DateTime CurrentTime, int AccountPKey, string ScheduleType)
        {
            try
            {
                StringBuilder q = new StringBuilder(" select a.eventsession_pkey, count(a.IsSpeaker) as NumSpk,count(a.IsSessionLeader) as numLdr1,count(a.IsSessionLeader2) as numLdr2, count(a.IsSessionChair) as NumChr");
                q.Append(System.Environment.NewLine + "From EventSession_Staff a inner join event_sessions b on b.pkey = a.eventSession_pkey");
                q.Append(System.Environment.NewLine + "where b.Event_pKey = " + EventpKey.ToString() + " And a.account_pkey =" + AccountPKey.ToString() + "Group by eventsession_pkey");

                StringBuilder qry = new StringBuilder(" SET NOCOUNT ON;" + System.Environment.NewLine + " SELECT 1 From Event_Sessions t0  left outer Join EventSession_Accounts t7 on t7.EventSession_pKey = t0.pkey and t7.Account_Pkey = " + AccountPKey.ToString());
                qry.Append(System.Environment.NewLine + " left outer Join (" + q + ") x on x.EventSession_pKey = t0.pkey");
                qry.Append(System.Environment.NewLine + " Where t0.Event_pKey = " + EventpKey.ToString() + " and isnull(t0.IsScheduled,0) = 1 and ((ISNULL(t7.Attending,0) = 1  OR ISNULL(t7.Watching,0)=1) ");
                qry.Append(System.Environment.NewLine + " Or (t0.sessionType_pkey in(" + ScheduleType + "))");
                qry.Append(System.Environment.NewLine + " Or ((x.NumSpk > 0)or (x.Numldr1 > 0) or (x.Numldr2 > 0) or (x.Numchr > 0))) and isnull(t0.IsScheduled,0) = 1 and t0.EndTime >= @CurrentTime");
                qry.Append(System.Environment.NewLine + " SET NOCOUNT OFF;");

                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@CurrentTime", CurrentTime) };

                DataTable dt = SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
                bool result = false;
                if (dt != null)
                {
                    result = (dt.Rows.Count > 0);
                }
                return result;
            }
            catch
            {
                clsUtility.LogErrorMessage(null, null, null, 0, "Error Checking Records For ICS File");
                return false;
            }
        }
        public DataTable getMyScheduleDataByID(int AccountPKey, int EventPKey, bool IsShowReleated, DateTime CurrentTime)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Account_pKey", AccountPKey),
                    new SqlParameter("@Event_pKey", EventPKey),
                    new SqlParameter("@Isshow", IsShowReleated),
                    new SqlParameter("@dtCurrentDateTime",CurrentTime)
                };
                return SqlHelper.ExecuteTable("getScheduleDetailsByEventAccount", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                return null;
            }
        }
        public DataTable GetMySessionsSettingsInfo(int Event_pKey, int Account_PKey)
        {
            DataTable data = null;
            try
            {
                StringBuilder qry = new StringBuilder();
                qry.Append("SELECT ISNULL(t1.IsSessionAlert,1) as IsSessionAlerts,CASE When (ISNULL(t3.IsMobile1,0)=1 OR ISNULL(t3.PhoneType_pKey,0)=12) and ISNULL(t3.Phone,'')<>'' Then ISNULL(t1.SendSessionReminder,1) When (ISNULL(t3.IsMobile2,0)=1 OR ISNULL(t3.PhoneType2_pKey,0)=12) and ISNULL(t3.Phone2,'')<>'' Then ISNULL(t1.SendSessionReminder,1) WHEN (ISNULL(t3.PhoneType_pKey,0)<>12 AND  ISNULL(t3.PhoneType_pKey,0)<>12)  Then 0 When (ISNULL(t3.PhoneType2_pKey,0)=12) AND  ISNULL(t3.Phone2,'')='' Then 0  ELSE ISNULL(t1.SendSessionReminder,0) END as SendSessionReminders");
                qry.Append(System.Environment.NewLine + " From Event_Accounts t1 Inner Join Account_List t3 On t3.pKey = @Account_pKey Where t1.Event_pKey = @Event_pKey AND t1.Account_pKey =@Account_pKey");
                SqlParameter[] parameters = new SqlParameter[]
                   {
                    new SqlParameter("@Account_pKey", Account_PKey),
                    new SqlParameter("@Event_pKey", Event_pKey)
                   };
                return SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
            }
            catch
            {

            }
            return data;
        }
        public DataTable GetCalnderData(int Event_pKey, int Account_pKey, DateTime CurrentTime, string ScheduleType, string DownloadType)
        {
            try
            {
                StringBuilder q = new StringBuilder("select a.eventsession_pkey, count(a.IsSpeaker) as NumSpk,count(a.IsSessionLeader) as numLdr1,count(a.IsSessionLeader2) as numLdr2, count(a.IsSessionChair) as NumChr");
                q.Append(System.Environment.NewLine + " From EventSession_Staff a inner join event_sessions b on b.pkey = a.eventSession_pkey where b.Event_pKey = " + Event_pKey.ToString());
                q.Append(System.Environment.NewLine + " And a.account_pkey =" + Account_pKey.ToString() + "Group by eventsession_pkey");

                StringBuilder qry = new StringBuilder(" SET NOCOUNT ON;" + System.Environment.NewLine + " DECLARE @TimeOffSET Decimal (5,2) = 0;");
                qry.Append(System.Environment.NewLine + " SELECT @TimeOffSET  = TimeOffset From  Event_List INNER JOIN StandardTime ON StandardTime.Pkey = Event_List.StandardTime_Pkey  Where Event_List.pKey =  " + Event_pKey.ToString());
                qry.Append(System.Environment.NewLine + " SELECT DISTINCT t0.pKey as EeventSession_pKey,t0.Title,DATEADD(HOUR,@TimeOffSET,t0.StartTime) as StartTime ,DATEADD(HOUR,@TimeOffSET,t0.EndTime) as EndTime,t0.Duration,t0.IsScheduled,t6.EventFullName,t6.EventID,t0.EventSpecificDescription, t8.EventSessionStatusID");
                qry.Append(System.Environment.NewLine + " From Event_Sessions t0 Inner Join Event_List t6 on t6.pKey = t0.Event_pkey");
                qry.Append(System.Environment.NewLine + " Inner Join Session_List t1 on t1.pKey = t0.Session_pkey");
                qry.Append(System.Environment.NewLine + " left outer join sys_SessionTypes t2 on t2.pkey = t1.SessionType_pkey");
                qry.Append(System.Environment.NewLine + " left outer join sys_Educationlevels t3 on t3.pkey = t1.Educationlevel_pkey");
                qry.Append(System.Environment.NewLine + " Left outer join sys_tracks t4 on t4.pkey = t1.Track_pkey");
                qry.Append(System.Environment.NewLine + " left outer Join EventSession_Accounts t7 on t7.EventSession_pKey = t0.pkey and t7.Account_Pkey = " + Account_pKey.ToString());
                qry.Append(System.Environment.NewLine + " Left Outer Join SYS_EventSessionStatuses t8 on t8.pkey = t0.EventSessionStatus_pkey ");
                qry.Append(System.Environment.NewLine + " left outer Join (" + q + ") x on x.EventSession_pKey = t0.pkey Where " + ((DownloadType == "All") ? "" : "ISNULL(t7.ScheduleDownloaded,0) = 0 AND ") + " t0.Event_pKey = " + Event_pKey.ToString());
                qry.Append(System.Environment.NewLine + " and ((ISNULL(t7.Attending,0) = 1  OR ISNULL(t7.Watching,0)=1) Or (t0.sessionType_pkey in(" + ScheduleType + "))");
                qry.Append(System.Environment.NewLine + " or ((x.NumSpk > 0)or (x.Numldr1 > 0) or (x.Numldr2 > 0) or (x.Numchr > 0)))");
                qry.Append(System.Environment.NewLine + " and isnull(t0.IsScheduled,0) = 1 and t0.EndTime >= @CurrentTime Order by StartTime");
                qry.Append(System.Environment.NewLine + " SET NOCOUNT OFF;");

                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@CurrentTime", CurrentTime) };
                return SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                string exception = ex.Message;
                return null;
            }
        }
        public DataTable GetVirtualEventDropDownInfo(string Host, int Account_pKey, int EventpKey, DateTime CurrentTime, DateTime CalTime, int ParentOrganizationPKey, int AttendeeStatus, int RegLevelPKey, bool GlobalAdmin, bool StaffMember)
        {
            DataTable dt = null;
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                 {
                    new SqlParameter("@Account_pKey", Account_pKey),
                    new SqlParameter("@Event_pKey", EventpKey),
                    new SqlParameter("@Host", Host),
                    new SqlParameter("@dCalTime",CalTime),
                    new SqlParameter("@CurrentTime",CurrentTime),
                    new SqlParameter("@ParentOrganizationpKey",ParentOrganizationPKey),
                    new SqlParameter("@AttendeeStatus",AttendeeStatus),
                    new SqlParameter("@RegistrationLevel_pKey",RegLevelPKey),
                    new SqlParameter("@GlobalAdmin",GlobalAdmin),
                    new SqlParameter("@StaffMember",StaffMember),
                 };
                return SqlHelper.ExecuteTable("getVirtualEventDropDown", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
                string Message = ex.Message;
            }
            return dt;
        }
        public DataTable getDyamicEventSettings(int Event_pKey, string SettingName)
        {
            try
            {
                string qry = "Select " + SettingName + " from Event_List Where pKey = @Event_pKey";
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@Event_pKey", Event_pKey) };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                return null;
            }
        }

        public DataTable getVenueBannerImage(int VenueID)
        {
            try
            {
                string qry = "SELECT isNull(t2.VenueID,'Not Specified') as VenueID ,ISNULL(t2.City,'') as LocationCity,(t2.FileGUID+'_'+t2.ImageBanner) as VenueBanner, (t2.FileGUID+'_'+t2.ImageSmall) as VenueSmall, (t2.FileGUID+'_'+t2.ImageNarrowBanner) as VenueNarrowBanner From venue_List t2 Where t2.pKey = @venue_pkey";
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@venue_pkey", VenueID) };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch
            {

            }

            return null;
        }
        public string getStandardRegion(int StandardReginpKey)
        {
            try
            {
                string qry = "SELECT ISNULL(Region,'') as Region From StandardTime Where pKey = @StandardReginpKey";
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@StandardReginpKey", StandardReginpKey) };
                DataTable dt = SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
                if (dt!=null && dt.Rows.Count>0)
                    return ((dt.Rows[0]["Region"] != System.DBNull.Value) ? dt.Rows[0]["Region"].ToString() : "");
            }
            catch
            {

            }

            return null;
        }
        public DataTable getIndexPageAccountSettings(int Event_pKey, int Account_pKey)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@Event_pKey", Event_pKey), new SqlParameter("@Account_pKey", Account_pKey) };
                return SqlHelper.ExecuteTable("getScheduleDetailsByEventAccount", CommandType.StoredProcedure, parameters);
            }
            catch
            {
                return null;
            }
        }
        public DataTable getMenuAccountSettings(int Event_pKey, int Account_pKey)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@Event_pKey", Event_pKey), new SqlParameter("@Account_pKey", Account_pKey) };
                return SqlHelper.ExecuteTable("MVC_getAccountSettingsMenu", CommandType.StoredProcedure, parameters);
            }
            catch
            {
                return null;
            }
        }
        public DataTable getTpInfo(int EventSessionpKey)
        {
            try
            {
                StringBuilder qry = new StringBuilder("Select t0.pKey,t1.ContactName As TPName, t1.Email As TPMail ,t1.Phone As TPContact, T1.Phone2 As TPAltContact");
                qry.Append(System.Environment.NewLine + ",iif(t1.PhoneType_pKey=12,'Mobile telephone','Telephone') as PhoneType1,iif(t1.PhoneType2_pKey=12,'Mobile telephone','Telephone') as PhoneType2");
                qry.Append(System.Environment.NewLine + "From Event_Sessions t0 LEFT OUTER JOIN Account_List t1 ON t1.pKey=t0.TP_pKey AND t1.Activated = 1 Where t0.pKey = @ESpKey");
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@ESpKey", EventSessionpKey) };
                return SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
            }
            catch
            {
                return null;
            }

        }
        public int getSpeakerAttendeeCount(int EventSessionpKey, int EventpKey)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
               {
                    new SqlParameter("@EventSessionPKey", EventSessionpKey),
                    new SqlParameter("@EventPKey", EventpKey),
               };
                return SqlHelper.ExecuteScaler("getAttendeeSpeakerCount", CommandType.StoredProcedure, parameters);
            }
            catch
            {
                return 0;
            }
        }
        public string HearSessionAlertUpdate(bool IsSessionAlert, int Account_PKey, int Event_pKey)
        {
            string qry = "Update Event_Accounts Set IsSessionAlert=@IsSessionAlert where Account_Pkey = @AccountpKey and Event_Pkey= @EventPKey";
            SqlParameter[] parameters = new SqlParameter[]
              {
                    new SqlParameter("@IsSessionAlert", IsSessionAlert),
                    new SqlParameter("@AccountpKey", Account_PKey),
                    new SqlParameter("@EventPKey", Event_pKey),
              };
            if (SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters))
                return "OK";
            else
                return "Error";
        }
        public string UpdateNotification(bool IsNotify, int Account_PKey, int Event_pKey)
        {
            string qry = "Update Event_Accounts Set SendSessionReminder=@SendSessionReminder where Account_Pkey = @AccountpKey and Event_Pkey= @EventPKey";
            SqlParameter[] parameters = new SqlParameter[]
            {
                    new SqlParameter("@SendSessionReminder", IsNotify),
                    new SqlParameter("@AccountpKey", Account_PKey),
                    new SqlParameter("@EventPKey", Event_pKey),
            };
            if (SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters))
                return "OK";
            else
                return "Error";
        }
        public DataTable CheckIfMobileAvailable(int intAccount)
        {
            try
            {
                StringBuilder qry = new StringBuilder("SELECT CASE When(ISNULL(t3.PhoneType_pKey, 0) = 12) and ISNULL(t3.Phone,'')<> '' Then 1 When(ISNULL(t3.PhoneType2_pKey, 0) = 12) and ISNULL(t3.Phone2,'')<> '' Then 1 ELSE 0 END as Phone1Available,");
                qry.Append(System.Environment.NewLine + "t3.Phone,t3.Phone2,t3.Phone2Ext,t3.Phone1Ext,t3.Phone1CC,t3.Phone2CC,ISNULL(t3.PhoneType_pKey, 0) as PhoneType_pKey,ISNULL(t3.PhoneType2_pKey, 0) as PhoneType2_pKey");
                qry.Append(System.Environment.NewLine + "From Account_List t3 Where  t3.Pkey = @AccountPkey");
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@AccountPkey", intAccount),
                };
                return SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
            }
            catch
            {
                return null;
            }
        }
        public DataTable UpdateContactwithNotification(int AccountPKey, int EventID, bool IsReminder, string Phone1 = "", string Phone1CC = "", string Phone1Ext = "", string PhoneType1 = "", string Phone2 = "", string Phone2CC = "", string Phone2Ext = "", string PhoneType2 = "")
        {
            try
            {
                int Phone1Type = (String.IsNullOrEmpty(PhoneType1)) ? 0 : Convert.ToInt32(PhoneType1);
                int Phone2Type = (String.IsNullOrEmpty(PhoneType2)) ? 0 : Convert.ToInt32(PhoneType2);

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Account_pkey", AccountPKey),
                    new SqlParameter("@EventPKey", EventID),
                    new SqlParameter("@Phone1", Phone1) ,
                    new SqlParameter("@Phone1Ext", Phone1Ext) ,
                    new SqlParameter("@Phone1CC", Phone1CC) ,
                    new SqlParameter("@Phone2", Phone2) ,
                    new SqlParameter("@Phone2Ext", Phone2Ext) ,
                    new SqlParameter("@Phone2CC", Phone2CC) ,
                    new SqlParameter("@Phone1Type", Phone1Type),
                    new SqlParameter("@Phone2Type", Phone2Type),
                    new SqlParameter("@IsReminder", IsReminder),
                    new SqlParameter("@ActionType", "Update"),
                };
                return SqlHelper.ExecuteTable("MVC_AccountContact_Update", CommandType.StoredProcedure, parameters);
            }
            catch
            {
                return null;
            }
        }
        public DataTable GetFeebdackInformation(int EvtSessionpKey, int EventID, int AccountPkey)
        {
            try
            {
                StringBuilder qry = new StringBuilder(" Declare @AttendedPercent int=0,@ZoomVerificationMail varchar(250) ='',@IsZoomVerification bit=0,@MaxLength int=0;");
                qry.Append(System.Environment.NewLine + " Select @MaxLength = IsNull(Maxlength,0) From ControlLength where PageName='EventFeedback'");
                qry.Append(System.Environment.NewLine + " SELECT @AttendedPercent=isnull(AttendedPercent, -1),@ZoomVerificationMail = ISNULL(ZoomVerificationMail,''),@IsZoomVerification = ISNULL(IsZoomVerification,0)  from EventSession_Accounts where EventSession_pKey = @EspKey and Account_pKey=@Account_pKey");
                qry.Append(System.Environment.NewLine + " SELECT t1.Account_pKey As Speaker_Pkey, t2.ContactName,('('+(isnull(isnull(t3.Track_Prefix,t5.Prefix),'')+t4.SessionID)+') '+isnull(t3.Title,t4.SessionTitle)) as SessionTitle");
                qry.Append(System.Environment.NewLine + " ,ISNULL(t4.IsHaveSlides,1) as IsHaveSlides,isnull(t6.OnTopicScore,-1) as OnTopicScore,isnull(t6.ContentScore,-1) as ContentScore,isnull(t6.PresentationScore,-1) PresentationScore");
                qry.Append(System.Environment.NewLine + " ,isnull(t6.SlidesScore,-1) as SlidesScore,isnull(t6.ValueScore,-1) as ValueScore,isnull(CAST(t6.LearningObjectivesMet AS INT),-1) as LearningObjectivesMet");
                qry.Append(System.Environment.NewLine + " ,isnull(CAST(t6.Unbiased AS INT),-1) as Unbiased,isnull(t8.UserComments,'') as Comment,isnull(t7.UserComments,'') as GComment,isnull(t7.MAGISuggestions,'') as MAGISuggestions");
                qry.Append(System.Environment.NewLine + " ,@AttendedPercent as AttendedPercent,t2.Title,t9.OrganizationID ,iif(ISNULL(t3.IsUnbiased,1)=1,1,0) as IsshowUnbiased,IIF(ISNULL(t3.NumObjectives,0)>0,1,0) as IsShowLOS");
                qry.Append(System.Environment.NewLine + " ,CASE when t1.Account_pKey=@Account_pKey then 0 else 1 end as disableSpeaker,IIF(ISNULL(t3.IsZoomVerification,0) = 1 OR @IsZoomVerification=1,1,0) as IsZoomVerification,@ZoomVerificationMail as  ZoomVerificationMail,ISNULL(t8.SpeakerAdvice,'') as SpeakerAdvice");
                qry.Append(System.Environment.NewLine + " ,@MaxLength as MaxLength From eventsession_staff t1 inner Join account_List t2 On t2.pKey = t1.Account_pKey");
                qry.Append(System.Environment.NewLine + " inner Join Event_Sessions t3 On t3.pkey = t1.EventSession_pKey Inner Join Session_List t4 On t4.pkey = t3.Session_pKey");
                qry.Append(System.Environment.NewLine + " Left Outer Join Sys_Tracks t5 on t5.pkey = t4.Track_pKey");
                qry.Append(System.Environment.NewLine + " left join Feedback_RawData t6 on t6.EventSession_pKey=t1.EventSession_pKey And t1.Account_pKey=t6.Account_pKey And t6.LogByAccount_pkey=@Account_pKey");
                qry.Append(System.Environment.NewLine + " left join Feedback_Comments t7 on t7.EventSession_pKey=t1.EventSession_pkey And isnull(t7.Account_pKey,0)=0 and t7.LogByAccount_pkey=@Account_pKey");
                qry.Append(System.Environment.NewLine + " left join Feedback_Comments t8 on t8.EventSession_pKey=t1.EventSession_pkey And t8.Account_pKey=t1.Account_pKey and t8.LogByAccount_pkey=@Account_pKey");
                qry.Append(System.Environment.NewLine + " left join Organization_List t9 on t2.ParentOrganization_pkey = t9.pKey");
                qry.Append(System.Environment.NewLine + " Where t1.EventSession_pKey = @EspKey And t1.IsSpeaker = 1");
                qry.Append(System.Environment.NewLine + " And  t2.pKey Not in (select Account_pKey from Event_Accounts where isnull(ParticipationStatus_pKey,0)=2 And Event_pKey=@Event_pKey)");
                qry.Append(System.Environment.NewLine + "Order by isNull(t1.SpeakingOrder, 0), t2.LastName");
                SqlParameter[] parameters = new SqlParameter[]
                {
                      new SqlParameter("@EspKey", EvtSessionpKey),
                      new SqlParameter("@Event_pKey", EventID),
                      new SqlParameter("@Account_pKey", AccountPkey),
                };
                return SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
            }
            catch
            {
                return null;
            }
        }
        public string UpdateScheduledHrsForCloud(int Account_pKey, int Event_PKey)
        {
            StringBuilder q = new StringBuilder("select SUM(IIf(isnull(AttendedPercent, -1) > 0, DateDiff(Minute, t1.StartTime, t1.EndTime), 0) / 60.0) from event_sessions t1");
            q.Append(System.Environment.NewLine + "inner join eventsession_accounts t2 on t2.eventsession_pkey = t1.pkey");
            q.Append(System.Environment.NewLine + "inner join session_list t3 On t3.pkey = t1.session_pkey Where t1.event_pkey =@EKey and t2.account_pkey =@APKey");

            StringBuilder qry = new StringBuilder("Update Event_Accounts Set LastScheduleChange = getdate(),SchedHours = isNull((" + q.ToString() + "),0) Where Event_pKey =@EKey And Account_pKey = @APKey");
            SqlParameter[] parameters = new SqlParameter[]
            {
                   new SqlParameter("@APKey", Account_pKey),
                   new SqlParameter("@EKey", Event_PKey),
            };
            if (SqlHelper.ExecuteNonQuery(qry.ToString(), CommandType.Text, parameters))
                return "OK";
            else
                return "Error Occured While Updating Scheduled Hours";
        }
        public string DeleteSpeakerFeedBack(int Account_pKey, int intEventSessionPKey)
        {
            StringBuilder qry = new StringBuilder("Delete from Feedback_RawData WHERE EventSession_pKey=@EventSession_pKey and LogByAccount_pkey=@Account_pKey");
            qry.Append(System.Environment.NewLine + " Delete from Feedback_Comments WHERE EventSession_pKey=@EventSession_pKey and LogByAccount_pkey=@Account_pKey");
            qry.Append(System.Environment.NewLine + " UPDATE EventSession_Accounts set AttendedPercent=null,FeedbackDate=getdate()");
            qry.Append(System.Environment.NewLine + " WHERE EventSession_pKey=@EventSession_pKey and Account_Pkey=@Account_pKey");
            SqlParameter[] parameters = new SqlParameter[]
            {
                   new SqlParameter("@Account_pKey", Account_pKey),
                   new SqlParameter("@EventSession_pkey", intEventSessionPKey),
            };
            if (SqlHelper.ExecuteNonQuery(qry.ToString(), CommandType.Text, parameters))
                return "OK";
            else
                return "Error withdraw speaker feedback comment";
        }
        public DataTable GetPersonalEvents(int Account_pKey, int Event_pKey, DateTime dtCurrentTime)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                   new SqlParameter("@Account_pkey", Account_pKey),
                   new SqlParameter("@Event_pkey", Event_pKey),
                   new SqlParameter("@EventTime", dtCurrentTime),
                };
                return SqlHelper.ExecuteTable("MVC_Show_Meeting_Select_ALL", CommandType.StoredProcedure, parameters);
            }
            catch
            {
                return null;
            }
        }
        public string DeletePersonalSchedule(int PersonalSchedulePKey)
        {
            string qry = "Delete From PersonalSchedule Where pKey = @PK";
            SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@PK", PersonalSchedulePKey) };
            if (SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters))
                return "OK";
            else
                return "Error Cancelling Schedule";
        }
        public DataTable GetPersonalScheduleData(int PersonalSchedulePKey, int AccountpKey)
        {
            try
            {
                StringBuilder qry = new StringBuilder("Select Account_pKey,Description,Title,Link,convert(varchar, ISNULL(DiscussionStart,GETDATE()), 101) as DiscussionStart ,DATEPART(MINUTE,ISNULL(DiscussionStart,DiscussionEnd)) As ScheduleMin ,");
                qry.Append(System.Environment.NewLine + " DATEDIFF (MINUTE,ISNULL(DiscussionStart,DiscussionEnd), ISNULL(DiscussionEnd,GETDATE())) AS Duration ,SUBSTRING(CONVERT(varchar(20), ISNULL(DiscussionStart,GETDATE()), 22), 19, 3) AS AMPM ");
                qry.Append(System.Environment.NewLine + ",case WHEN DATEPART(hour,ISNULL(DiscussionStart ,DiscussionEnd)) >13 then  DATEPART(hour,ISNULL(DiscussionStart ,DiscussionEnd)) -12 ELSE DATEPART(hour,ISNULL(DiscussionStart ,DiscussionEnd)) END As ScheduleHour");
                qry.Append(System.Environment.NewLine + " ,case when DATEPART(hour,DiscussionEnd) > 13 Then DATEPART(hour,DiscussionEnd) - 12 Else DATEPART(hour,DiscussionEnd) End EndHour");
                qry.Append(System.Environment.NewLine + " ,DATEPART(MINUTE,ISNULL(DiscussionEnd,getDate())) As EndScheduleMin,SUBSTRING(CONVERT(varchar(20), ISNULL(DiscussionEnd,GETDATE()),22),19,3) AS EndAMPM ,datediff (MINUTE,ISNULL(DiscussionStart,DiscussionEnd), ISNULL(DiscussionEnd,GETDATE())) AS Duration");
                qry.Append(System.Environment.NewLine + " ,HostName FROM PersonalSchedule Where pkey = @PK And Account_pKey = @AccountID");
                SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@PK", PersonalSchedulePKey),
                new SqlParameter("@AccountID", AccountpKey)
            };
                return SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
            }
            catch
            {
                return null;
            }
        }
        public string UpdatePersonalSchedule(int AccountpKey, int EventID, string Title, string Description, DateTime StartDate, DateTime EndDate, int PersonalSchedulePKey = 0)
        {
            StringBuilder qry = new StringBuilder();
            if (PersonalSchedulePKey == 0)
            {
                qry.Append("INSERT INTO PersonalSchedule (Account_pKey, Event_pKey, Description, Title, DiscussionStart, DiscussionEnd)");
                qry.Append(System.Environment.NewLine + "Values(@Account_pKey,@Event_pKey,@Description,@Title,@DiscussionStart,@DiscussionEnd)");
            }
            else
            {
                qry.Append("Update PersonalSchedule SET Title=@Title, DiscussionStart=@DiscussionStart, DiscussionEnd=@DiscussionEnd,Description=@Description");
                qry.Append(System.Environment.NewLine + ",Event_pKey=@Event_pKey,Account_pKey=@Account_pKey where pkey=" + PersonalSchedulePKey.ToString());
            }

            SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@Title", Title),
                new SqlParameter("@Description", Description),
                new SqlParameter("@DiscussionStart", StartDate),
                new SqlParameter("@DiscussionEnd", EndDate),
                new SqlParameter("@Account_pKey", AccountpKey),
                new SqlParameter("@Event_pKey", EventID),
            };
            if (SqlHelper.ExecuteNonQuery(qry.ToString(), CommandType.Text, parameters))
                return "OK";
            else
                return "Error While Updating Personal Schedule";
        }
        public string CheckOverlappingPersonalEvent(int AccountID, int EventID, DateTime dtStartTime, DateTime dtEndTime, int? pKey = 0)
        {
            try
            {
                StringBuilder qry = new StringBuilder("With QRY AS (SELECT t1.pkey as RoundTableSchedule_pkeys,t1.DiscussionStart as StartDate ,t1.DiscussionEnd as EndTime,t1.title FROM RoundTableSchedule t1");
                qry.Append(System.Environment.NewLine + " WHERE t1.Event_pKey = @Event_pKey AND (t1.DiscussionType <> 5 AND ISNULL(t1.Active,0)=1 AND isnull(t1.ScheduleType,1) <> 1 AND (@Account_pKey IN (SELECT Value From String_Split(ISNULL(t1.ShowOnSchedule,''),','))))");
                qry.Append(System.Environment.NewLine + " UNION SELECT t1.pkey as RoundTableSchedule_pkeys,t1.DiscussionStart as StartDate ,t1.DiscussionEnd as EndTime,t1.title FROM  RoundTableSchedule t1 ");
                qry.Append(System.Environment.NewLine + " WHERE t1.Event_pKey = @Event_pKey AND t1.DiscussionType = 5  AND ISNULL(t1.ActiveOnSchedule,0)=1 AND ISNULL(t1.Active,0)=1 AND isnull(t1.ScheduleType,1) <> 1 AND t1.ShowOnSchedule = @Account_pKey");
                qry.Append(System.Environment.NewLine + " UNION SELECT t1.pkey as RoundTableSchedule_pkeys,t1.DiscussionStart as StartDate ,t1.DiscussionEnd as EndTime,t1.title From PersonalSchedule t1 Where Account_pKey =@Account_pKey And Event_pKey=@Event_pKey AND (@pKey = '' OR t1.pKey <> @pKey)");
                qry.Append(System.Environment.NewLine + " ) SELECT * From  QRY Where  ((abs(datediff(minute,QRY.StartDate,@starttime))=0 or abs(datediff(minute,QRY.StartDate,@EndTime))=0)");
                qry.Append(System.Environment.NewLine + " or (QRY.StartDate>@starttime and QRY.EndTime<=@EndTime) or (@starttime>QRY.StartDate and @EndTime<=QRY.EndTime))");
                SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@Event_pKey", EventID),
                new SqlParameter("@StartTime", dtStartTime),
                new SqlParameter("@EndTime", dtEndTime),
                new SqlParameter("@Account_pkey", AccountID),
                new SqlParameter("@pKey", pKey),
            };
                DataTable dt = new DataTable();
                dt = SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        string image = System.Configuration.ConfigurationManager.AppSettings["AppURL"].ToString().Replace("/forms", "/") + "images/icons/OverlapSessionIcon.png";
                        return "<image src='" + image + "' height='18px'>  <b>Conflicting Times</b> <br /> This event overlaps with: " + dt.Rows[0]["Title"].ToString();
                    }
                }
                return "OK";
            }
            catch
            {
                return "Error Occured While Checking Personal Schedule";
            }
        }
        public string CancelPersonalSchedule(int ID, string strSponsor, int account, int pkey)
        {
            string qry = "DeleteEventAttendee_Schedule_Delete";
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter() { ParameterName = "@AccountpKey", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input, Value = account });
            parameters.Add(new SqlParameter() { ParameterName = "@RoundTableSchedule_pkeys", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input, Value = ID });
            parameters.Add(new SqlParameter() { ParameterName = "@pkey", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input, Value = pkey });
            parameters.Add(new SqlParameter() { ParameterName = "@Error", SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Output, Value = "" });
            if (SqlHelper.ExecuteNonQuery(qry, CommandType.StoredProcedure, parameters.ToArray()))
                return "OK";
            else
                return "Error Canceling Schedule";
        }
        public string CancelPersonalScheduleUpdate(string ID, string Type, int account, int EventID, DateTime dtCurrent)
        {
            string qry = "EXEC [SP_AddToMySchedule] @accPkey,@eventKey,@roundTableSchedule_pkey,@currentTime,@TypeOfSchedule,@Leave;";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@accPkey", account),
                 new SqlParameter("@eventKey", EventID),
                 new SqlParameter("@roundTableSchedule_pkey", ID),
                 new SqlParameter("@currentTime", dtCurrent),
                 new SqlParameter("@TypeOfSchedule", Type),
                 new SqlParameter("@Leave", true),
            };
            if (SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters))
                return "OK";
            else
                return "Error Canceling Schedule";
        }
        public string CheckSessionCountMessage(int EspKey, int AccountId, int MaxCount)
        {
            string qry = "SELECT SUM(IIf(Log_Pkey =2,1 ,0)) as CountPlay,SUM(IIf(Log_Pkey =1,1 ,0)) as CountAttend  From  Attendee_EnterWebinar Where  EventSession_pKey = @EspKey AND Account_pkey =@AccountId";
            SqlParameter[] parameters = new SqlParameter[]
              {
                new SqlParameter("@AccountId", AccountId),
                new SqlParameter("@EspKey", EspKey),
              };
            string Message = "";
            try
            {
                DataTable dt = SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        int countPlay = 0, CountAttend = 0;
                        if (dt.Rows[0]["CountPlay"] != System.DBNull.Value)
                            countPlay = Convert.ToInt32(dt.Rows[0]["CountPlay"].ToString());
                        if (dt.Rows[0]["CountAttend"] != System.DBNull.Value)
                            CountAttend = Convert.ToInt32(dt.Rows[0]["CountAttend"].ToString());

                        if (countPlay >= 1 && countPlay < MaxCount)
                            Message = "<b>Session recordings</b><br />Note: Session recordings are for registered event participants only so please do not share with others.";
                        if (countPlay >= MaxCount && CountAttend >= 0)
                            Message = "You have already watched this recording " + MaxCount.ToString() + "  times, which is the maximum allowed.";

                    }
                }
            }
            catch
            {

            }
            return Message;
        }

        public string UpdateScheduleCalendar(string pKeys, int AccountID)
        {
            try
            {
                string qry = "UPDATE EventSession_Accounts SET ScheduleDownloaded = 1 Where EventSession_pKey IN (" + pKeys.ToString().Trim(',') + ") AND Account_pKey =" + AccountID.ToString();
                if (SqlHelper.ExecuteNonQuery(qry, CommandType.Text, null))
                    return "OK";
                else
                    return "Error While Updating ICS File Setting";
            }
            catch
            {
                clsUtility.LogErrorMessage(null, null, this.GetType().Name, 0, "Error While Updating ICS File Setting");
                return "Error While Updating ICS File Setting";
            }
        }
        public DataTable GetSpeakerAdvices()
        {
            try
            {
                return SqlHelper.ExecuteTable("SELECT pKey,AdviceText as strText From Sys_Advice Where IsActive=1 ORDER BY strText ASC", CommandType.Text, null);
            }
            catch
            {
                return null;
            }
        }
        public string GetCountsOnEventbutton(int EventId, int AccountId, DateTime currentTime)
        {
            try
            {
                string qry = "SELECT SUM(IIf(Log_Pkey =2,1 ,0)) as CountPlay,SUM(IIf(Log_Pkey =1,1 ,0)) as CountAttend  From  Attendee_EnterWebinar Where  EventSession_pKey = @EspKey AND Account_pkey =@AccountId";
                SqlParameter[] parameters = new SqlParameter[]
                  {
                    new SqlParameter("@Account_pkey", AccountId),
                    new SqlParameter("@Event_pkey", EventId),
                    new SqlParameter("@EventTime", currentTime),
                  };
                DataTable dt = SqlHelper.ExecuteTable("SP_CountsOfEvent", CommandType.StoredProcedure, parameters);
                if (dt != null && dt.Rows.Count > 0)
                {
                    int Count = 0;
                    if (dt.Rows[0]["cnt"] != System.DBNull.Value)
                        Count = Convert.ToInt32(dt.Rows[0]["cnt"].ToString());

                    return "My Personal Events (" + Count.ToString() + ")";
                }
            }
            catch
            {

            }
            return "My Personal Events";
        }
        #endregion MySchedulePage_MVC

        #region HelpIcon 
        private bool CheckIfAutoPlayLogExists(string PageVal, int EventPKey, int Account_pKey)
        {
            bool result = false;
            try
            {
                string qry = "IF EXISTS(SELECT 1 From Attendee_EnterBooth Where Event_Pkey =  " + EventPKey.ToString() + " AND Log_Pkey =" + clsEventSession.Exhibit_TrainingResource_View_AutoPlay.ToString() + " AND Account_pkey =" + Account_pKey.ToString() + " AND PageAreaVal = " + PageVal + ")"
                         + System.Environment.NewLine + "SELECT 1;ELSE SELECT 0;";
                SqlCommand cmd = new SqlCommand(qry);
                DataTable dt = SqlHelper.ExecuteTable(qry, CommandType.Text, null);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        string value = (dt.Rows[0][0] != System.DBNull.Value) ? dt.Rows[0][0].ToString() : "0";
                        result = (value == "0");
                    }
                }
            }
            catch
            {

            }
            return result;
        }
        private DataTable LoadResourceHelpData(User_Login Data, string PageKey, int? EventSessionPKey = null)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                new SqlParameter("@Account_pKey", Data.Id),
                new SqlParameter("@Event_pKey", Data.EventId),
                new SqlParameter("@PageKey", PageKey),
                new SqlParameter("@IsGlobalAdmin", Data.GlobalAdmin),
                new SqlParameter("@EventSession_pKey",EventSessionPKey),
                new SqlParameter("@FetchingType","TABS"),
                new SqlParameter("@PageType","TABS"),
                };
                return SqlHelper.ExecuteTable("sp_getVideoAccordingToUserRole", CommandType.StoredProcedure, parameters);
            }
            catch
            {
                return null;
            }
        }
        public HelpIconData PageLoadResourceData(User_Login Data, string PageAreaTab, string PageAreaPage)
        {

            bool IsPage = (!string.IsNullOrEmpty(PageAreaPage));
            bool bMediaPlaying = ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).bMediaPlaying;
            HelpIconData iconData = new HelpIconData();
            string AutoPlayValue = "0";
            try
            {
                if (!string.IsNullOrEmpty(PageAreaPage))
                {
                    DataTable dtPage = LoadResourceHelpData(Data, PageAreaPage);
                    if (dtPage != null)
                    {
                        if (dtPage.Rows.Count > 0)
                        {
                            iconData.PageIconVisible = true;
                            iconData.PageFileName = (dtPage.Rows[0]["DisplayName"] == System.DBNull.Value) ? "" : dtPage.Rows[0]["DisplayName"].ToString();
                            iconData.PageDocumentLink = (dtPage.Rows[0]["DocumentLink"] == System.DBNull.Value) ? "" : dtPage.Rows[0]["DocumentLink"].ToString();
                            iconData.PageMediaType = (dtPage.Rows[0]["Type"] == System.DBNull.Value) ? "" : dtPage.Rows[0]["Type"].ToString();
                            iconData.PageValue = (dtPage.Rows[0]["PageArea"] == System.DBNull.Value) ? "" : dtPage.Rows[0]["PageArea"].ToString();
                            AutoPlayValue = (dtPage.Rows[0]["AutoPlay"] == System.DBNull.Value) ? "0" : dtPage.Rows[0]["AutoPlay"].ToString();
                            iconData.PageMime = System.Web.MimeMapping.GetMimeMapping(iconData.PageDocumentLink);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(PageAreaTab))
                {
                    DataTable dtTab = LoadResourceHelpData(Data, PageAreaTab);
                    if (dtTab != null)
                    {
                        if (dtTab.Rows.Count > 0)
                        {
                            iconData.TabIconVisible = true;
                            iconData.TabFileName = (dtTab.Rows[0]["DisplayName"] == System.DBNull.Value) ? "" : dtTab.Rows[0]["DisplayName"].ToString();
                            iconData.TabDocumentLink = (dtTab.Rows[0]["DocumentLink"] == System.DBNull.Value) ? "" : dtTab.Rows[0]["DocumentLink"].ToString();
                            iconData.TabMediaType = (dtTab.Rows[0]["Type"] == System.DBNull.Value) ? "" : dtTab.Rows[0]["Type"].ToString();
                            iconData.TabValue = (dtTab.Rows[0]["PageArea"] == System.DBNull.Value) ? "" : dtTab.Rows[0]["PageArea"].ToString();
                            AutoPlayValue = (dtTab.Rows[0]["AutoPlay"] == System.DBNull.Value) ? "0" : dtTab.Rows[0]["AutoPlay"].ToString();
                            iconData.TabToolTip = (PageAreaTab == "44") ? "How to use this panel" : "How to use this tab";
                            iconData.TabMime = System.Web.MimeMapping.GetMimeMapping(iconData.TabDocumentLink);
                        }
                    }
                }
                //Checks if AutoPlay Value Is Set Or Not
                if (AutoPlayValue != "0")
                {
                    if (!bMediaPlaying)
                    {
                        /* Cases For Auto Play Check
                         * case"1": Auto Play when user goes to page or tab.
                         * case "2": Auto Play the first time person goes to a page.
                         */
                        string PageVal = ((IsPage) ? "Page" : "Tab");
                        iconData.AutoplayType = (!string.IsNullOrEmpty(PageAreaPage)) ? "Page" : "Tab";

                        bool bAutoplay = (CheckIfAutoPlayLogExists(PageVal, Data.EventId, Data.Id) && (AutoPlayValue == "2")) || (AutoPlayValue == "1");
                        iconData.bAutoPlay = bAutoplay;
                        ((clsLastUsed)System.Web.HttpContext.Current.Session["cLastUsed"]).bMediaPlaying = true;

                    }
                }
            }
            catch (Exception ex)
            {
                return iconData;
            }
            return iconData;
        }
        public int HelpIcon_Save(int AccountID, int EventID, int type, bool bAutoPlay, string filename = "")
        {
            int Result = 0;
            int LogPKey = ((bAutoPlay) ? clsEventSession.Exhibit_TrainingResource_View_AutoPlay : clsEventSession.Exhibit_TrainingResource_View);
            DateTime dtCurrDate = clsEvent.getEventVenueTime();
            StringBuilder qry = new StringBuilder("INSERT INTO Attendee_EnterBooth (Account_pkey,InTime,EnteredOn,Organization_pkey ,Log_Pkey,AttendeeLog,BoothID,Event_Pkey,FileName,BoothMessage,PageAreaVal) ");
            qry.Append(System.Environment.NewLine + " Values (@Account_pkey,@InTime,GetDate(),iif(@Organization_pkey>0,@Organization_pkey,(Select top 1 pkey from Event_Organizations where BoothSetting_pkey=@BoothID)),@Log_Pkey,@AttendeeLog,@BoothID,@Event_Pkey,@filename,@BoothMessage,@PageAreaVal) ");
            qry.Append(System.Environment.NewLine + " SELECT SCOPE_IDENTITY() as ID;");
            SqlParameter[] parameters = new SqlParameter[]
               {
                new SqlParameter("@Account_pkey", AccountID),
                new SqlParameter("@Event_Pkey", EventID),
                new SqlParameter("@InTime", dtCurrDate),
                new SqlParameter("@filename", filename),
                new SqlParameter("@Log_Pkey",LogPKey),
                new SqlParameter("@PageAreaVal",type),
                new SqlParameter("@AttendeeLog","Resource Document Clicked"),
                new SqlParameter("@Organization_pkey", "0"),
                new SqlParameter("@BoothMessage", ""),
                new SqlParameter("@BoothID", "0"),
             };
            try
            {
                DataTable dt = SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        if (dt.Rows[0][0] != System.DBNull.Value)
                            Result = Convert.ToInt32(dt.Rows[0][0].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                string info = ex.Message;
            }
            return Result;
        }
        public string HelpIcon_Update(int ID)
        {
            string result = " Error Occured While Updating HelpIcon Log";
            string qry = " Update Attendee_EnterBooth SET EndTime = GetDate() Where pKey= @pKey";
            SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@pKey", ID) };
            try
            {
                if (SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters))
                    return "OK";
            }
            catch
            {
            }
            return result;
        }



        #endregion HelpIcon

        #region VirtualDropDown
        public bool UpdateSurveyClickCount(int EventID,int AccountID)
        {
            try
            {
                string qry = " Update Event_Accounts SET RegFeedbackClicks = (ISNULL(RegFeedbackClicks,0) + 1)  Where Event_pKey = @Event_pKey AND Account_pKey = @Account_pKey";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Account_pKey", AccountID),
                    new SqlParameter("@Event_pKey", EventID),
                };
                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool GetIsRegistered(int EventPKey,int AccountPKey)
        {
            
            try
            {
                string qry = "SELECT IIF(pkey > 0, 1, 0) as Registered  From Event_Accounts Where Event_pKey = @Event_pKey AND Account_pKey = @Account_pKey";
                DataTable dt = new DataTable();
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@Account_pKey", AccountPKey),
                    new SqlParameter("@Event_pKey", EventPKey),
                };
                dt = SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
                if (dt != null && dt.Rows.Count > 0)
                    return (dt.Rows[0][0] != System.DBNull.Value) ? Convert.ToBoolean(dt.Rows[0][0]) : false;
            }
            catch
            {
                return false;
            }
            return false;
        }
        public DataTable GetNavigationInstructionInfo(int EventPKey)
        {
            try
            {
                string qry = "select t1.pKey, t1.SectionTitle, t1.SectionText, t1.Sequence, isNull(t1.Collapsible,0) as Collapsible, isNull(t1.DefaultCollapsed,0) as DefCollapsed"
                            + Environment.NewLine + ",(Case when isNull(t1.TitleURL,'') = '' Then Null else dbo.CleanURL(t1.TitleURL) End) as TitleLink"
                            + Environment.NewLine + " From Event_Text t1 Where t1.Event_pKey = @EventpKey And t1.Active=1 And t1.Indicator = 7 Order by t1.Sequence";
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@EventpKey", EventPKey) };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch
            {
                return null;
            }
        }
        public int CheckFeedbackClickCount(int ActiveEvent, int AccountPKey)
        {
            int result = 0;
            try
            {
                string qry = "SELECT ISNULL(RegFeedbackClicks,0) as RegFeedbackClicks From Event_Accounts Where Event_pKey = @Event_pKey AND Account_pKey = @Account_pKey";
                SqlCommand cmd = new SqlCommand(qry);
                DataTable dt = new DataTable();
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@Account_pKey", AccountPKey),
                    new SqlParameter("@Event_pKey", ActiveEvent),
                };
                dt = SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
                if (dt != null && dt.Rows.Count > 0)
                    result = (dt.Rows[0][0] != System.DBNull.Value) ? Convert.ToInt32(dt.Rows[0][0]) : 0;
            }
            catch
            {
                result = 0;
            }
            return result;
        }
        public bool RegistrationFeedback(int AccountPKey,int  ActiveEvent)
        {
            try
            {
                string qry = "select t1.pkey,t3.Forms_pKey,t3.pKey as FQ_pKey, IsNull(t1.Question,'') as Ques, Isnull(t1.ResponseType_pKey,1) as Res, IsNull(t1.ResponseOptions,'') Reo, IsNull(t1.Required,0) Req";
                qry = qry + Environment.NewLine + ",IsNull(t1.Question,'')+IIF(IsNull(t1.Required,0)=1,'<br>(answer required)','<br>(answer optional)') as Question,0 As Attendee";
                qry = qry + Environment.NewLine + ",IsNull(t5.Response,'') Answer, Isnull(t5.pKey,0) Answer_pkey,0 As EventSession_pkey";
                qry = qry + Environment.NewLine + " from ExhibitorFeedbackForm_Questions t1";
                qry = qry + Environment.NewLine + " inner join Forms_Question t3 on t3.Question_pKey=t1.pKey";
                qry = qry + Environment.NewLine + " inner join ExhibitorFeedbackForm_List t4 on t4.pkey=t3.Forms_pKey";
                qry = qry + Environment.NewLine + "left outer join ExhibitorFeedbackForm_UserResponse t5 on t5.Question_pkey =  t1.pkey and t5.Event_pKey=t3.Event_pkey and t5.Account_pkey = @Account_pKey";
                qry = qry + Environment.NewLine + "Where isnull(t5.Active,1)=1 And t3.forms_pkey =" + clsExhibitorFeedbackForm.intRegistrationQuestion.ToString() + " and t3.Event_pkey= @Event_pKey";
                SqlCommand cmd = new SqlCommand(qry);
                DataTable dt = new DataTable();
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@Account_pKey", AccountPKey),
                    new SqlParameter("@Event_pKey", ActiveEvent),
                };
                dt = SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
                if (dt != null && dt.Rows.Count > 0)
                    return dt.Select().Any(x => Convert.ToBoolean(x["req"]) == true && Convert.ToInt32(x["Answer_pkey"]) == 0);
            }
            catch
            {
                
            }
            return false;
        }

        #endregion

        #region VirtualSession
        public bool CheckTechnincalProducer(int EventpKey, int AccountPKey)
        {
            bool result = false;
            try
            {
                string qry = "Select pkey from Event_Staff where account_pkey = " + AccountPKey.ToString() + " and event_pkey = " + EventpKey.ToString() + " And EventRole_pKey =15";
                DataTable DT = SqlHelper.ExecuteTable(qry, CommandType.Text, null);
                if (DT != null)
                    return (DT.Rows.Count > 0);
            }
            catch
            {
            }
            return result;
        }
        public void UpdateEventSessionAccessLog(int intEventSession_PKey, int intAcctPKey, string strGroupID, bool bSpeaker, int intEventpkey, DateTime dtCurrDate, int Exhibit_Webinar)
        {
            try
            {
                StringBuilder qry = new StringBuilder("if exists(select pkey from EventSession_AccessLog where EventSession_pKey=@EventSession_pKey and Account_Pkey=@Account_Pkey)");
                qry.Append(System.Environment.NewLine + "begin");
                qry.Append(System.Environment.NewLine + " update EventSession_AccessLog set GroupId=@GroupId,AccessTime=getdate(),IsSpeaker=@bSpeaker where EventSession_pKey=@EventSession_pKey and Account_Pkey=@Account_pKey");
                qry.Append(System.Environment.NewLine + "end");
                qry.Append(System.Environment.NewLine + "else");
                qry.Append(System.Environment.NewLine + "begin");
                qry.Append(System.Environment.NewLine + "insert into EventSession_AccessLog (EventSession_pkey,Account_pKey,AccessTime,GroupId,IsSpeaker)");
                qry.Append(System.Environment.NewLine + "values(@EventSession_pkey,@Account_pKey,getdate(),@GroupId,@bSpeaker)");
                qry.Append(System.Environment.NewLine + "end");
                qry.Append(System.Environment.NewLine + "Insert into Attendee_EnterWebinar(Account_pkey,InTime,EventSession_pKey,Log_Pkey,Event_Pkey)");
                qry.Append(System.Environment.NewLine + "values(@Account_pKey,@InTime,@EventSession_pkey,@Log_Pkey,@Event_Pkey)");
                SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@EventSession_pkey", intEventSession_PKey),
                new SqlParameter("@Account_pKey", intAcctPKey),
                new SqlParameter("@InTime", dtCurrDate),
                new SqlParameter("@GroupId", strGroupID),
                new SqlParameter("@Log_Pkey",Exhibit_Webinar ),
                new SqlParameter("@AttendeeLog", "Attendee Enter Webinar"),
                new SqlParameter("@bSpeaker", bSpeaker),
                new SqlParameter("@Event_Pkey", intEventpkey),
            };
                if (!SqlHelper.ExecuteNonQuery(qry.ToString(), CommandType.Text, parameters))
                    clsUtility.LogErrorMessage(null, null, "clsUtility", 0, "Error executing query (0)", dbError: "Error creating EventSession AccessLog", Msg: "Error creating EventSession AccessLog");
            }
            catch (Exception ex)
            {
                clsUtility.LogErrorMessage(null, null, "clsUtility", 0, "Error executing query (0)", dbError: ex.Message, Msg: ex.Message);
            }
        }
        public int GetAttendeeCount(System.Web.HttpRequest objRequest, int EventPKey, int EventSession_pKey)
        {
            int count = 0;
            try
            {

                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@Event_pKey", EventPKey),
                    new SqlParameter("@EventSession_pKey", EventSession_pKey),
                };
                DataTable dt = SqlHelper.ExecuteTable("MVC_getAttendeeCount", CommandType.StoredProcedure, parameters);
                count = (dt.Rows[0]["Total"] == System.DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["Total"].ToString());
            }
            catch (Exception ex)
            {
                count = 0;
                clsUtility.LogErrorMessage(null, objRequest, objRequest.GetType().Name, 0, "Error GetAttendee Count");
            }
            return count;
        }
        public DataTable GetVirtualEventSessionData(string speakerMail, int AccountPKey, int EventSessionPKey)
        {
            try
            {
                StringBuilder Qry = new StringBuilder("select t1.Pkey,t1.StartTime,t1.EndTime,t1.Session_pKey,(isnull(t1.Track_Prefix,'')+t2.SessionID) as SessionID,isnull(t1.Title,t2.SessionTitle) as SessionTitle");
                Qry.Append(System.Environment.NewLine + " ,0 as pQueCount,0 as PollingCount,(select count(1) from EventSession_Accounts where EventSession_pkey=t1.pKey) as AttendeeCount,ISNULL(t1.SessionIntro_pkey,0) as SessionIntro_pkey");
                Qry.Append(System.Environment.NewLine + " ,0 as PollingResultCount,(select count(1) from EventSession_AccessLog where EventSession_pkey=t1.pKey and DAY(AccessTime)=DAY(getdate())) as AttendeeLogCount");
                Qry.Append(System.Environment.NewLine + " ,iif(isnull(EventOrganizations_pkey,'')<>'',1,0) as ExhibitCount");
                Qry.Append(System.Environment.NewLine + " ,isnull(t3.PermalinkUrl,isnull(t1.LongURL,isNull(t1.WebinarLink,''))) as linkUrl,isnull(t3.MaxMemberInBreakout,0) as MaxMemberInBreakout");
                Qry.Append(System.Environment.NewLine + " ,isnull(t1.LongURL,isNull(t1.WebinarLink,'')) as SpkrUrl,isnull(t1.PIN,'') as SpkrPIN,isnull(t3.RequestVerificationToken,t1.PIN) as AttPIN");
                Qry.Append(System.Environment.NewLine + " ,0 as PollingEnabled,0 as ShowResult,isnull(t1.HostKey,'') as WebinarHostKey");
                Qry.Append(System.Environment.NewLine + " ,isnull(t1.TP_pKey,0) as TP_pKey,isnull(t1.IsBreakOut,0) IsBreakOut, isnull(t4.Link,isnull(WebinarLink,'')) as ZoomWebinarURL, isnull(HallwayURL,'') as ZoomMeetingURL");
                Qry.Append(System.Environment.NewLine + " ,isnull(t1.HallwayURL,0) as HallwayURL,isnull(t1.HallwayHostKey,'') as HallwayHostKey,isnull(t1.HallwayActive,0) as HallwayActive,isnull(IsBreakOut,0) as IsBreakOut");
                Qry.Append(System.Environment.NewLine + " ,isnull(t1.WebinarPwd,'') as WebinarPwd,isnull(t1.HallwayPwd,'') as HallwayPwd,isnull(t1.WebinarStarted,0) as WebinarStarted,isnull(t1.MeetingStarted,0) as MeetingStarted,ISNULL(t1.IsLiveStream,0) as IsLiveStream");
                Qry.Append(System.Environment.NewLine + " from Event_Sessions t1 Inner join Session_List t2 On t2.pkey = t1.Session_pKey");
                Qry.Append(System.Environment.NewLine + " Left outer join EventSession_Accounts t3 on t3.EventSession_pKey=t1.pKey And t3.Account_pKey=@Account_pKey");
                Qry.Append(System.Environment.NewLine + " Left outer join SpeakerSessionURL t4 on t4.EventSession_pkey=t3.EventSession_pKey And SpeakerEmail=@SpeakerEmail where t1.pKey=@EventSession_pkey");
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@Account_pKey", AccountPKey),
                    new SqlParameter("@EventSession_pkey", EventSessionPKey),
                    new SqlParameter("@SpeakerEmail", speakerMail),
                };
                return SqlHelper.ExecuteTable(Qry.ToString(), CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public DataTable GetSpeakerProfiles(int EventpKey, int EventSessionPKey)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Event_pKey", EventpKey),
                    new SqlParameter("@EventSession_pKey", EventSessionPKey)
                };
                return SqlHelper.ExecuteTable("MVC_getSpeakerProfiles", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
                return null;

            }
        }
        public void UpdateAttendeeLog(int EventOrganizationPKey, int EventPKey, int AccountpKey, int Espk)
        {
            try
            {
                DateTime dtCurrDate = clsEvent.getEventVenueTime();
                StringBuilder qry = new StringBuilder("Insert INTO Attendee_EnterBooth (Account_pkey	,InTime,	OutTime,	EnteredOn,	Organization_pkey ,Event_Pkey,EventSession_pKey ,AttendeeLog)");
                qry.Append(System.Environment.NewLine + " Values (@Account_pkey,@InTime,null,GetDate(),@Organization_pkey,@Event_Pkey,@EventSession_pKey,@AttendeeLog)");
                SqlParameter[] parameters = new SqlParameter[]
                {
                new SqlParameter("@Account_pkey", AccountpKey),
                new SqlParameter("@Organization_pkey", EventOrganizationPKey),
                new SqlParameter("@InTime", dtCurrDate),
                new SqlParameter("@Event_Pkey", EventPKey),
                new SqlParameter("@EventSession_pKey", Espk),
                new SqlParameter("@AttendeeLog", "Schedule clicked"),
                };
                SqlHelper.ExecuteNonQuery(qry.ToString(), CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                clsUtility.LogErrorMessage(null, null, "clsUtility", 0, "Error executing query (0)", dbError: ex.Message, Msg: ex.Message);
            }
        }
        public DataTable GetVirtualSessionBoothDataByID(int EventpKey, int BoothID)
        {
            try
            {
                StringBuilder qry = new StringBuilder("Select isNull(t1.pkey,0) As ipk, t1.OrganizationID,iif(isnull(t1.Comment,'')='',t1.OrganizationID,replace(replace(isNull(t1.Comment,''),'{','<b>'),'}','</b>')) as Profile");
                qry.Append(System.Environment.NewLine + " , t2.ParticipationLevel_pKey, t2.SpecialOffer,(Case when isNull(t2.SpecialOffer,'') <> '' Then 1 else 0 End) as ShowOffer");
                qry.Append(System.Environment.NewLine + " ,(Case when t2.ParticipationType_pKey > 0 then IIF(t3.ParticipationTypeID = 'Branding','','[' + t3.ParticipationTypeID + ']')  else Null End) as PType");
                qry.Append(System.Environment.NewLine + " ,'~/OrganizationDocuments/'+Convert(varchar,t1.pKey)+'_img.jpg' As ImgLogo");
                qry.Append(System.Environment.NewLine + " ,replace(replace(IIF(charindex('//',isnull(t1.url,''))>0 ,isnull(t1.url,'')	,'//'+ isnull(t1.url,'')),'!w','//w'),'!h','h') as URL");
                qry.Append(System.Environment.NewLine + " ,'this.onerror=null;this.src=''../OrganizationDocuments/Thumbnail/DefaultOrganization.png?'+CONVERT(varchar,DATEDIFF(second, '1970-01-01', GETDATE()))+'''' as NoImage");
                qry.Append(System.Environment.NewLine + " from Organization_List t1 Inner Join Event_Organizations t2 On t2.Organization_pkey = t1.pkey");
                qry.Append(System.Environment.NewLine + " Left outer join Sys_ParticipationTypes t3 On t3.pkey = t2.ParticipationType_pKey");
                qry.Append(System.Environment.NewLine + " Where t2.Event_pKey =@EventpKey AND  t2.pKey =@BoothKey Order by t1.OrganizationID");
                SqlParameter[] parameters = new SqlParameter[]
                {
                new SqlParameter("@EventpKey", EventpKey),
                new SqlParameter("@BoothKey", BoothID),
                };
                return SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public DataTable GetVirtualSessionBoothData(int EventSessionPKey)
        {
            try
            {
                StringBuilder qry = new StringBuilder("SELECT t1.pKey,t2.OrganizationID,IIF(isnull(t2.Comment,'')='',t2.OrganizationID,replace(replace(isNull(t2.Comment,''),'{','<b>'),'}','</b>')) as Profile");
                qry.Append(System.Environment.NewLine + ",replace(replace(IIF(charindex('//',isnull(t2.url,''))>0 ,isnull(t2.url,'')	,'//'+ isnull(t2.url,'')),'!w','//w'),'!h','h') as URL");
                qry.Append(System.Environment.NewLine + " From Event_Organizations t1 left outer join Organization_list t2 on t1.Organization_pKey=t2.pKey");
                qry.Append(System.Environment.NewLine + " where @EventSession_pKey in (select Num from dbo.[CSVToNumberTable](t1.EventSession_pkeys,',')) ORDER BY t2.OrganizationID");
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@EventSession_pKey", EventSessionPKey)
                };
                return SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
            }
            catch
            {
                return null;
            }
        }
        public DataTable GetRelatedSession(int EventPKey, int SessionId, int AccountPKey)
        {
            try
            {
                StringBuilder qry = new StringBuilder("SELECT DISTINCT t1.Session_pKey,t1.SessionID,'('+ t1.SessionID +') ' + t1.SessionTitle as SessionTitle, t0.pkey as EventSession_pKey, t0.StartTime as st, t0.EndTime as et,");
                qry.Append(System.Environment.NewLine + " isnull(t0.Track_Prefix + t1.SessionID,'') as SessionID,'('+isnull(t0.Track_Prefix + t1.SessionID,'')+') '+isnull(t0.Title,t1.SessionTitle) as SessionTitle,");
                qry.Append(System.Environment.NewLine + " FORMAT(t0.StartTime, 'hh:mm tt') as TimeStart, FORMAT(t0.EndTime, 'hh:mm tt') as TimeEnd,");
                qry.Append(System.Environment.NewLine + " (Case when t0.IsScheduled = 1 Then t0.StartTime Else '1/1/1980' End) as StartTime,");
                qry.Append(System.Environment.NewLine + " (case when t0.IsScheduled = 1 and t0.EndTime > t0.StartTime Then t0.EndTime Else dateadd(hh,1,t0.StartTime) End) as EndTime,");
                qry.Append(System.Environment.NewLine + " (case when datediff(y,t0.EndTime , getdate())>=7 Then 'True' else 'False' End) as PassedDay,");
                qry.Append(System.Environment.NewLine + " (FORMAT(t0.StartTime, 'D', 'en-US' )+'  '+FORMAT(t0.StartTime, 'hh:mm tt')+' - '+FORMAT(t0.EndTime, 'hh:mm tt')) as SessionTime,");
                qry.Append(System.Environment.NewLine + " (Case When t7.Slides > 0 Then 1 Else 0 End) As slidesChecked,(Case When t7.Attending > 0 Then 1 Else 0 End) As attendChecked,(Case When t7.Watching > 0 Then 1 Else 0 End) As watchChecked");
                qry.Append(System.Environment.NewLine + " FROM dbo.getRelatedSessionsTable(@SessionID, 1, @EventPKey) t1 Inner Join Session_List t2 on t2.pKey = t1.Session_pkey  ");
                qry.Append(System.Environment.NewLine + " INNER JOIN Event_Sessions t0 ON t0.Session_pKey  = t1.Session_pkey ");
                qry.Append(System.Environment.NewLine + " left outer Join EventSession_Accounts t7 on t7.EventSession_pKey = t0.pKey and t7.Account_Pkey = @AccountPKey");
                qry.Append(System.Environment.NewLine + " Where isnull(t0.IsScheduled,0)=1 and t0.Event_pKey = @EventPKey  AND ((Case When t7.Attending > 0 Then 1 Else 0 End) =0 AND (Case When t7.Watching > 0 Then 1 Else 0 End)= 0)");
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@EventPKey", EventPKey),
                    new SqlParameter("@AccountPKey", AccountPKey),
                    new SqlParameter("@SessionID", SessionId)
                };
                return SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public DataTable GetSpeakerBreakOutDropdown(int ESPK)
        {
            try
            {
                string qry = "Select ROW_NUMBER() OVER (ORDER BY t1.GroupId) As pKey, t1.GroupId As strText FROM EventSession_SpeakerBreakout t1 Where t1.EventSession_pKey = @ESPK Group by t1.GroupId";
                SqlParameter[] parameters = new SqlParameter[]
                {
                     new SqlParameter("@ESPK", ESPK)
                };
                return SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public DataTable GetRefreshLinks(int ESPK, string strGroupId)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                  new SqlParameter("@EventSession_pKey", ESPK),
                  new SqlParameter("@GroupId", ESPK),
                  new SqlParameter("@Type", 2),
                };
                DataSet datainfo = SqlHelper.ExecuteSet("SpeakerBreakout_List", CommandType.StoredProcedure, parameters);
                if (datainfo != null)
                    return datainfo.Tables[0];
            }
            catch
            {
            }
            return null;
        }
        public DataTable GetSpeakerBreakOutData(int ESPK, string strGroupId, int Type)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
              new SqlParameter("@EventSession_pKey", ESPK),
              new SqlParameter("@GroupId", strGroupId),
              new SqlParameter("@Type", Type),
                };
                DataSet datainfo = SqlHelper.ExecuteSet("SpeakerBreakout_List", CommandType.StoredProcedure, parameters);
                if (datainfo != null)
                    return datainfo.Tables[0];
            }
            catch
            {
            }
            return null;
        }
        public string UpdateSpeakerBreakoutLeader(int ID, bool CKValue)
        {
            string qry = "UPDATE EventSession_SpeakerBreakout Set Isleader =@Isleader where pkey=@Pkey";
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@Pkey", ID),
                 new SqlParameter("@Isleader", CKValue),
            };
            if (SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters))
                return "OK";
            else
                return "Error While Updating Enable Hallway";
        }
        public DataTable GetVirtualSessionDocuments(int ESPK, int AccountPKey, string type, string Tab)
        {
            try
            {
                StringBuilder qry = new StringBuilder();
                switch (type)
                {
                    case "Speaker":
                        qry.Append("Select t1.pKey, t1.DocFileName as DocumentLink, t1.DocDisplayName as DisplayName,'' as ContactName from EventSession_Documents t1");
                        qry.Append(System.Environment.NewLine + " WHERE t1.EventSession_pKey=@EventSession_pKey AND (t1.SessionDocType_pKey=" + clsEventSession.DOCTYPE_Download.ToString() + " or t1.SessionDocType_pKey=" + clsEventSession.DOCTYPE_Handout.ToString() + ")");
                        break;
                    case "Attendee":
                        qry.Append("Select t1.pKey, t1.DocLink as DocumentLink, t1.DocName as DisplayName " + ((Tab == "1") ? ",t2.ContactName " : "") + " from EventSession_AttendeeDocuments t1");
                        if (Tab == "1")
                            qry.Append(System.Environment.NewLine + " Left join Account_List t2 on t2.Pkey=t1.Account_Pkey");
                        qry.Append(System.Environment.NewLine + " WHERE isnull(t1.Active,0)=1 and EventSession_pKey=@EventSession_pKey and Account_pkey=@Account_PKey");

                        break;
                }
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@EventSession_pKey", ESPK),
                    new SqlParameter("@Account_PKey", AccountPKey),
                };
                return SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
            }
            catch
            {
                return null;
            }
        }
        public bool DeleteGroupVirtualSession(int ESPK, int AccountID)
        {
            bool result = false;
            try
            {
                StringBuilder qry = new StringBuilder("delete From [EventSession_SpeakerBreakout] Where [EventSession_pkey] = @EventSession_pkey;");
                qry.Append(System.Environment.NewLine + "UPDATE EventSession_Accounts Set MaxMemberInBreakout =@MaxMember where EventSession_pkey=@EventSession_pkey AND Account_pkey=@Account_pkey");
                SqlParameter[] parameters = new SqlParameter[]
                    {
                    new SqlParameter("@MaxMember", 0),
                    new SqlParameter("@EventSession_pkey", ESPK),
                    new SqlParameter("@Account_PKey", AccountID),
                    };
                if (SqlHelper.ExecuteNonQuery(qry.ToString(), CommandType.Text, parameters))
                    return true;
            }
            catch
            {

            }
            return result;
        }
        public bool UpdateSpeakerBreakOut(int intMaxMember, int ESPK, int AccountID, int intBreakoutGroupCount, string SessionID)
        {
            bool result = false;
            try
            {
                StringBuilder qry = new StringBuilder(" UPDATE EventSession_Accounts Set MaxMemberInBreakout =@MaxMember where EventSession_pkey=@EventSession_pkey AND  Account_pkey=@Account_pkey");
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaxMember", intMaxMember),
                    new SqlParameter("@EventSession_pkey", ESPK),
                    new SqlParameter("@Account_PKey", AccountID),
                };
                if (SqlHelper.ExecuteNonQuery(qry.ToString(), CommandType.Text, parameters))
                {
                    parameters = new SqlParameter[]
                    {
                        new SqlParameter("@paramEventID", ESPK),
                        new SqlParameter("@paramgrpNum", intBreakoutGroupCount),
                        new SqlParameter("@paramgrpname", SessionID),
                        new SqlParameter("@Error",SqlDbType.VarChar,1000),
                    };
                    parameters[2].Direction = ParameterDirection.Output;
                    if (SqlHelper.ExecuteNonQuery("Create_SpeakerBreakout", CommandType.StoredProcedure, parameters))
                        return true;
                }
            }
            catch
            {
            }
            return result;
        }
        public DataTable LoadSessionIntroVideo(int IntroPlaypKey)
        {
            try
            {
                string qrystring = "Select  iif(ISNULL(DocumentLink,'')<>'', '~/UserDocuments/'+DocumentLink,'') as VideoURL from PartnerBooth t1  Where  T1.FileType =9 AND  t1.pKey = @SessionIntro_pkey";
                SqlParameter[] parameters = new SqlParameter[]
                {
                   new SqlParameter("@SessionIntro_pkey", IntroPlaypKey),
                };
                DataTable dt = SqlHelper.ExecuteTable(qrystring, CommandType.Text, parameters);
                return dt;
            }
            catch
            {
                return null;
            }
        }
        public DataTable LoadDimension()
        {
            try
            {
                string qrystring = "Select Dimension, Size From Sys_IntroPopup ORDER BY Dimension ASC";
                DataTable dt = SqlHelper.ExecuteTable(qrystring, CommandType.Text, null);
                return dt;
            }
            catch
            {
                return null;
            }
        }
        #endregion VirtualSession

        #region IndexPage_MVC
        public DataTable refreshQLinks(int intNextEvent, int intEventType_PKey)
        {
            try
            {
                string qry = "select t1.LinkText, t1.LinkPage,GroupID,ISNULL(t1.LinkPageMVC,'') as LinkPageMVC,";
                qry = qry + Environment.NewLine + "iif( EXISTS (select num from dbo.csvtonumbertable(t1.Event_Pkey,',') where Num=" + intNextEvent.ToString() + "), 1 , 0) as bdisable";
                qry = qry + Environment.NewLine + " From sys_HomepageLinks t1";
                qry = qry + Environment.NewLine + "where isnull(IsActive,0)=1 and GroupID in (1,2,3) ";
                if (intEventType_PKey == clsEvent.EventType_CloudConference || intEventType_PKey == clsEvent.EventType_HybridConference)
                    qry = qry + Environment.NewLine + " and t1.Pkey<>6";
                else
                    qry = qry + Environment.NewLine + " and t1.Pkey<>14";
                qry = qry + Environment.NewLine + " Order by t1.SortOrder";
                return SqlHelper.ExecuteTable(qry, CommandType.Text, null);
            }
            catch
            {
                return null;
            }
        }
        public DataTable GetOneEventsList()
        {
            try
            {
                string qry = "Select t1.pKey, t1.EventID,t1.EventType_PKey,t1.HomelinkDisable,isNull(t2.VenueID,'Not Specified') as Venue,t1.HomeBanner";
                qry = qry + Environment.NewLine + ",t2.City as LocationCity, t3.StateID as LocationState,t1.EventFullname,t1.StartDate,t1.EndDate,(t2.FileGUID+'_'+t2.ImageSmall) as VenueSmall";
                qry = qry + Environment.NewLine + ",iif(len(isnull(t1.PublicName,t1.EventID))>23,SUBSTRING(isnull(t1.PublicName,t1.EventID), 1, 23)+'..',isnull(t1.PublicName,t1.EventID)) as strText,t1.EventStatus_pKey,t1.RegEndDate";
                qry = qry + Environment.NewLine + "From Event_List t1";
                qry = qry + Environment.NewLine + "Left Outer Join venue_List t2 On t2.pKey = t1.venue_pkey";
                qry = qry + Environment.NewLine + "Left Outer Join SYS_States t3 On t3.pkey = t2.State_pkey";
                qry = qry + Environment.NewLine + "Where t1.PublicPageStartDate<=Convert(Date, CONVERT(char(10), GetDate(),126)) and dateadd(DD,1,t1.PublicPageEndDate)>=Convert(Date, CONVERT(char(10), GetDate(),126))";
                qry = qry + Environment.NewLine + "and t1.EventStatus_pKey not in (4,5)";
                qry = qry + Environment.NewLine + "Order by t1.StartDate";
                return SqlHelper.ExecuteTable(qry, CommandType.Text, null);
            }
            catch
            {
                return null;
            }
        }
        public void ReplaceReservedWordsIndex(clsSettings cSettings, int EventID, string strOrig1, ref string strReplace1, string strOrig2, ref string strReplace2, string strOrig3, ref string strReplace3)
        {

            strReplace1 = ""; strReplace2 = ""; strReplace3 = "";
            // -- StrReplace1 
            if (!string.IsNullOrEmpty(strOrig1))
            {
                strReplace1 = clsSettings.ReplaceTermsGeneral(strOrig1);
                strReplace1 = cSettings.ReplaceTermsExternal(strReplace1);
            }
            //--replace 2
            if (!string.IsNullOrEmpty(strOrig2))
            {
                strReplace2 = clsSettings.ReplaceTermsGeneral(strOrig2);
                strReplace2 = cSettings.ReplaceTermsExternal(strReplace2);
            }
            //--replace 3
            if (!string.IsNullOrEmpty(strOrig3))
            {
                strReplace3 = clsSettings.ReplaceTermsGeneral(strOrig3);
                strReplace3 = cSettings.ReplaceTermsExternal(strReplace3);
            }

            //  Not Required For Current Event Changes

            //strReplace1 = cEvent.ReplaceReservedWords(strReplace1)
            //strReplace1 = cVenue.ReplaceReservedWords(strReplace1)
            //strReplace1 = clsReservedWords.ReplacePriorNext(lblMsg, strReplace1)

            //strReplace2 = clsSettings.ReplaceTermsGeneral(strOrig2)
            // strReplace2 = cSettings.ReplaceTermsExternal(strReplace2)
            // strReplace2 = cEvent.ReplaceReservedWords(strReplace2)
            // strReplace2 = cVenue.ReplaceReservedWords(strReplace2)
            // strReplace2 = clsReservedWords.ReplacePriorNext(lblMsg, strReplace2)

            // strReplace3 = cEvent.ReplaceReservedWords(strReplace3)
            // strReplace3 = cVenue.ReplaceReservedWords(strReplace3)
            // strReplace3 = clsReservedWords.ReplacePriorNext(lblMsg, strReplace3)



        }
        #endregion IndexPage_MVC

        #region MyCertificates 
        public DataTable GetCertificateData(string Account_Pkey)
        {
            try
            {
                StringBuilder sb = new StringBuilder(" select 0 as Type, t1.Comment, t1.pkey,(isNull(t1.EarnedCEUs, 0) + isNull(t1.ManualAdjustment, 0)) as EarnedCEUs, (t5.CertName + ' (' + t5.CertAbbrev + ')') as CertName, t5.CertAbbrev, t4.EventID, t4.EndDate as CertDate  ");
                sb.AppendLine(System.Environment.NewLine + " , t6.CertStatusID, t2.Account_pKey, t2.Event_pKey, t1.CertStatus_pkey,isnull(t1.HoldReason_pKey, '') as HoldReason,isnull(dbo.GETCertificate_Links_Fn(t1.HoldReason_pKey, t2.Event_pKey, t3.pkey, t1.pkey), '') as PageLink,t5.pkey as CertPkey  ");
                sb.AppendLine(System.Environment.NewLine + "  ,al.Firstname, al.Lastname, null as CRCPExpirationDate,0 as ExamStatus_pkey,'' as ExamCertificateText, null as LatestCertDate  ");
                sb.AppendLine(System.Environment.NewLine + "  ,iif(t4.EventType_pkey in (" + clsEvent.EventType_CloudConference.ToString() + ", "+ clsEvent.EventType_HybridConference.ToString() + ", " + clsEvent.EventType_Webinar.ToString() + ") and isnull(t5.pkey, 0) = 3, 1, 0) as Editable  ");
                sb.AppendLine(System.Environment.NewLine + " ,isnull(t1.LiveEarned, 0) as LiveEarned,isnull(t1.NotLiveEarned, 0) as NotLiveEarned,isnull(t7.PRACredits, 0) as PRACredits,isnull(t7.IsOverrideHrs, 0) as IsOverrideHrs,isnull(t7.NotLiveCredits, 0) as NotLiveCredits,isnull(t1.IsUpdatedByUser, 0) as IsUpdatedByUser  ");
                sb.AppendLine(System.Environment.NewLine + " ,isnull(t7.CombinedCredits, 0) As CombinedCredits, isnull(t1.CerticationSessions, '') as CertificationSessions ,t4.EventFullName  ");
                sb.AppendLine(System.Environment.NewLine + "    From EventAccount_Certifications t1   ");
                sb.AppendLine(System.Environment.NewLine + "  inner join Event_Accounts t2 on t2.pkey = t1.EventAccount_pKey  ");
                sb.AppendLine(System.Environment.NewLine + " Inner join Account_List t3 on t3.pkey = t2.Account_pKey   ");
                sb.AppendLine(System.Environment.NewLine + " Inner join Event_List t4 on t4.pkey = t2.Event_pKey  ");
                sb.AppendLine(System.Environment.NewLine + "  Inner join Certification_List t5 on t5.pkey = t1.Certification_pKey ");
                sb.AppendLine(System.Environment.NewLine + " Inner Join Sys_CertStatuses t6 on t6.pkey = t1.CertStatus_pkey  ");
                sb.AppendLine(System.Environment.NewLine + " Left Outer join Certification_Detail t7 on t5.pKey = t7.Certification_pKey and t7.Event_pKey = t2.Event_pKey  ");
                sb.AppendLine(System.Environment.NewLine + "  Inner join Account_List al on al.pkey = t2.Account_pKey  ");
                sb.AppendLine(System.Environment.NewLine + "  Where t2.Account_pKey = " + Account_Pkey);
                sb.AppendLine(System.Environment.NewLine + "  And t6.DisplayOnAttendeeView = 1 ");
                sb.AppendLine(System.Environment.NewLine + "  And isnull(t7.ShowOnMyCertificate,1)= 1  ");
                sb.AppendLine(System.Environment.NewLine + "  And t4.EventStatus_pKey Not in (4, 5)  ");
                sb.AppendLine(System.Environment.NewLine + " And t1.CertStatus_pkey in(" + clsCertification.CERTSTATUS_Approved.ToString() + ", " + clsCertification.CERTSTATUS_Issued.ToString() + ", " + clsCertification.CERTSTATUS_Hold.ToString() + ", " + clsCertification.CERTSTATUS_Received.ToString() + ")  ");
                sb.AppendLine(System.Environment.NewLine + " UNION  ");
                sb.AppendLine(System.Environment.NewLine + " select 1 as Type, 'Attendance Certificate' as Comment, t1.pkey, t1.SchedHours as EarnedCEUs, 'Attendance' as CertName  ");
                sb.AppendLine(System.Environment.NewLine + " , 'Attendance' as CertAbbrev, t2.EventID, t2.EndDate as CertDate, 'Issued' as CertStatusID, t1.Account_pKey, t1.Event_pKey, " + clsCertification.CERTSTATUS_Issued.ToString() + " as CertStatus_pkey,'' as HoldReason,'' as PageLink,0 as CertPkey  ");
                sb.AppendLine(System.Environment.NewLine + " ,al.Firstname, al.Lastname, null as CRCPExpirationDate,0 as ExamStatus_pkey,'' as ExamCertificateText,null as LatestCertDate,0 as Editable  ");
                sb.AppendLine(System.Environment.NewLine + " ,0 as LiveEarned,0 as NotLiveEarned,0 as PRACredits,0 as IsOverrideHrs,0 as NotLiveCredits,0 as IsUpdatedByUser,0 as CombinedCredits,'' as CertificationSessions ,t2.EventFullName  ");
                sb.AppendLine(System.Environment.NewLine + "  From Event_Accounts t1   ");
                sb.AppendLine(System.Environment.NewLine + " Inner join Event_List t2 on t2.pkey = t1.Event_pKey   ");
                sb.AppendLine(System.Environment.NewLine + " Inner join Account_List al on al.pkey = t1.Account_pKey  ");
                sb.AppendLine(System.Environment.NewLine + " Where t1.Account_pKey = " + Account_Pkey);
                sb.AppendLine(System.Environment.NewLine + "  and t1.ParticipationStatus_pKey = " + clsEventAccount.PARTICIPATION_Attending.ToString());
                sb.AppendLine(System.Environment.NewLine + " And isnull(t2.ShowAttCerts,1)= 1  ");
                sb.AppendLine(System.Environment.NewLine + "  And t2.EndDate < Getdate()  ");
                sb.AppendLine(System.Environment.NewLine + "  And t2.EventStatus_pKey Not in (4, 5)  ");
                sb.AppendLine(System.Environment.NewLine + " UNION  ");
                sb.AppendLine(System.Environment.NewLine + " select 2 as Type, 'Speaker Certificate' as Comment, t1.pkey, Null as EarnedCEUs, 'Speaker' as CertName  ");
                sb.AppendLine(System.Environment.NewLine + " , 'Speaker' as CertAbbrev, t2.EventID, t2.EndDate as CertDate, 'Issued' as CertStatusID, t1.Account_pKey, t1.Event_pKey, " + clsCertification.CERTSTATUS_Issued.ToString() + " as CertStatus_pkey,'' as HoldReason,'' as PageLink,0 as CertPkey  ");
                sb.AppendLine(System.Environment.NewLine + " ,al.Firstname, al.Lastname, null as CRCPExpirationDate,0 as ExamStatus_pkey,'' as ExamCertificateText,null as LatestCertDate,0 as Editable  ");
                sb.AppendLine(System.Environment.NewLine + " ,0 as LiveEarned,0 as NotLiveEarned,0 as PRACredits,0 as IsOverrideHrs,0 as NotLiveCredits,0 as IsUpdatedByUser,0 as CombinedCredits,'' as CertificationSessions ,t2.EventFullName  ");
                sb.AppendLine(System.Environment.NewLine + "  From Event_Accounts t1  ");
                sb.AppendLine(System.Environment.NewLine + " Inner join Account_List al on al.pkey = t1.Account_pKey  ");
                sb.AppendLine(System.Environment.NewLine + " Inner join Event_List t2 on t2.pkey = t1.Event_pKey  ");
                sb.AppendLine(System.Environment.NewLine + " Inner join dbo.getEventSpeakers() t3 on t3.event_pkey = t1.Event_pkey and t3.account_pkey = t1.account_pkey  ");
                sb.AppendLine(System.Environment.NewLine + "  Where t1.Account_pKey = " + Account_Pkey + "AND t2.EndDate < Getdate() ");
                sb.AppendLine(System.Environment.NewLine + "  And t2.EventStatus_pKey Not in (4, 5)  ");
                sb.AppendLine(System.Environment.NewLine + " UNION  ");
                sb.AppendLine(System.Environment.NewLine + " select 3 as Type, 'CRCP Certificate' as Comment, t1.pkey, Null as EarnedCEUs, iif(isnull(t1.ExamStatus_pkey, 0) = 2, '(CRCP not passed)', 'CRCP') as CertName  ");
                sb.AppendLine(System.Environment.NewLine + " , 'CRCP' as CertAbbrev, Null as EventID, t1.ExamDate as CertDate, t2.ExamStatusID as CertStatusID, t1.Account_pKey, 0 as Event_pKey, iif(isnull(t1.ExamStatus_pkey, 0) = 2, 0, 7) as CertStatus_pkey,'' as HoldReason,'' as PageLink,0 as CertPkey  ");
                sb.AppendLine(System.Environment.NewLine + " ,al.Firstname, al.Lastname, t1.ExamExpDate as CRCPExpirationDate,t1.ExamStatus_pkey,t2.ExamCertificateText,isnull((select max(examdate) as maxexam from Account_ExamResults where exam_pkey = t1.exam_pkey and ExamStatus_pKey in (1, 6)  ");
                sb.AppendLine(System.Environment.NewLine + " and account_pkey = t1.Account_pKey),t1.ExamDate) as LatestCertDate,0 as Editable  ");
                sb.AppendLine(System.Environment.NewLine + " ,0 as LiveEarned,0 as NotLiveEarned,0 as PRACredits,0 as IsOverrideHrs,0 as NotLiveCredits,0 as IsUpdatedByUser,0 as CombinedCredits,'' as CertificationSessions,'' as EventFullName  ");
                sb.AppendLine(System.Environment.NewLine + " From Account_ExamResults t1  ");
                sb.AppendLine(System.Environment.NewLine + " Inner join sys_examStatuses t2 on t2.pkey = t1.ExamStatus_pkey  ");
                sb.AppendLine(System.Environment.NewLine + "  Inner join Account_List al on al.pkey = t1.Account_pKey ");
                sb.AppendLine(System.Environment.NewLine + " Where t1.Account_pKey = " + Account_Pkey);
                sb.AppendLine(System.Environment.NewLine + " And t1.ExamStatus_pkey in(" + clsExam.EXAMSTATUS_PassGroup + ", 2)  ");
                sb.AppendLine(System.Environment.NewLine + " Order by CertDate desc, Certname  ");

                return SqlHelper.ExecuteTable(sb.ToString(), CommandType.Text, null);
            }
            catch
            {
                return null;
            }
        }
        #endregion MyCertificates

        #region MyPayments
        public DataTable GetVoucherInfoByID(int VoucherID, int AccountID, string Email)
        {
            try
            {
                StringBuilder sb = new StringBuilder("select t1.pKey,t1.VoucherCode,'V'+FORMAT(t1.pKey, '00000') as PaddedID,t1.ExpirationDate,t1.Amount,t3.VoucherStatusID,isnull(t1.Email,'') as VoucherEmail");
                sb.Append(System.Environment.NewLine + " ,t1.IssuedOn, t1.IsUsed,iif(isnull(t1.IsUsed,0)=1,'Used By: '+isnull(t2.contactname,''),'Not Used') as Status,isnull(t2.ContactName,t1.Email) as IssuedTo");
                sb.Append(System.Environment.NewLine + " ,isnull(CardLastFour,'') as CardLastFour, isnull(PaymentTransAction,'') as PaymentTransAction");
                sb.Append(System.Environment.NewLine + " ,isnull(DateDiff(Day, PaymentDate, getdate()),-1) as PaymentDayDiff,isnull(ReferenceReceipt,'0') as ReferenceReceipt");
                sb.Append(System.Environment.NewLine + " from Account_Vouchers t1 LEFT OUTER JOIN account_list t2 on t1.UsedByAccount_pkey=t2.pKey");
                sb.Append(System.Environment.NewLine + " LEFT OUTER JOIN SYS_VoucherStatuses t3 on t3.pkey = t1.VoucherStatus_pkey");
                sb.Append(System.Environment.NewLine + " LEFT OUTER JOIN account_Payments t4 on t4.ReceiptNumber = t1.ReferenceReceipt");
                sb.Append(System.Environment.NewLine + " WHERE t1.pKey = @VoucherID and (t1.Email =@Email or t1.Account_pKey= @AccountID)   and  isnull(t1.VoucherStatus_pkey,0)=" + clsVoucher.VoucherStatus_Active.ToString());
                sb.Append(System.Environment.NewLine + " order by t1.pkey Desc");
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@VoucherID", VoucherID),
                    new SqlParameter("@AccountID", AccountID),
                    new SqlParameter("@Email", Email),
                };
                return SqlHelper.ExecuteTable(sb.ToString(), CommandType.Text, parameters);
            }
            catch
            { }
            return null;
        }
        public void Update_AccountPayment(double Amount, int AccountID, int RecipetNo, int EventID, string RefundTransactionID, int RefundRecieptNumber, string strResponseCardType)
        {
            try
            {
                string qry = "UPDATE ACCOUNT_PAYMENTS set RefundAmount= @dblAmount,UpdatedBy=@AccountID Where ReceiptNumber= @RecieptNumber And Event_Pkey  = @EvtID";
                qry = qry + System.Environment.NewLine + ";UPDATE Payment_CardInfo set RefundTransactionID= @txnID,RefundCardType=@CardType where ReceiptNumber=  @RefRecieptNumber";
                SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@dblAmount", Amount),
                new SqlParameter("@AccountID", AccountID),
                new SqlParameter("@RecieptNumber", RecipetNo),
                new SqlParameter("@EvtID", EventID),
                new SqlParameter("@txnID", RefundTransactionID),
                new SqlParameter("@CardType", strResponseCardType),
                new SqlParameter("@RefRecieptNumber", RefundRecieptNumber),
            };
                if (!SqlHelper.ExecuteNonQuery(qry.ToString(), CommandType.Text, parameters))
                    clsUtility.LogErrorMessage(null, null, "clsUtility", 0, "Error executing query (0)", dbError: "Error Updating Account Payment", Msg: "Error Updating Account Payment");
            }
            catch (Exception ex)
            {
                clsUtility.LogErrorMessage(null, null, "clsUtility", 0, "Error executing query (0)", dbError: ex.Message, Msg: ex.Message);
            }
        }
        #endregion MyPayments

        #region LeftPanel

        public DataTable GetTimeZoneDataByKey(int pKey)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@pKey", pKey),
                };
                return SqlHelper.ExecuteTable("SELECT StandardTime.Region,StandardTime.RegionCode,TimeOffset From StandardTime Where pKey = @pKey", CommandType.Text, parameters);
            }
            catch
            {
                return null;
            }
        }
        public DataTable LoadListReminders(int Account_pKey, int Event_pKey, string TimeZoneID, string PageType, bool IsNewTab, DateTime dtCurrentTime, bool IsDemo, string strRegionCode, string CurrentEventDuration, DateTime dtEndDate, int intSessionSpeakerMinutes)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@Account_pKey", Account_pKey),
                    new SqlParameter("@Event_pKey", Event_pKey),
                    new SqlParameter("@TimeZone", TimeZoneID),
                    new SqlParameter("@PageType", PageType),
                    new SqlParameter("@IsNewTab", IsNewTab),
                    new SqlParameter("@CurrentTime", dtCurrentTime),
                    new SqlParameter("@IsDemo", IsDemo),
                    new SqlParameter("@RegionCode", strRegionCode),
                    new SqlParameter("@CurrentEventDuration", CurrentEventDuration),
                    new SqlParameter("@EventEndDate", dtEndDate),
                    new SqlParameter("@SessionMinuteCount", intSessionSpeakerMinutes)
                };
                return SqlHelper.ExecuteTable("sp_getDailyRemindersList", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                clsUtility.LogErrorMessage(null, null, GetType().Name, 0, "Error Accessing Notifications. " + message);
                return null;
            }
        }
        public DataTable LoadListRemindersUpdated(int Account_pKey, int Event_pKey, string TimeZoneID, string PageType, bool IsNewTab, DateTime dtCurrentTime, bool IsDemo, string strRegionCode, string CurrentEventDuration, DateTime dtEndDate, DateTime dtStartDate, int intSessionSpeakerMinutes)
        {
            try
            {

                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@Account_pKey", Account_pKey),
                    new SqlParameter("@Event_pKey", Event_pKey),
                    new SqlParameter("@TimeZone", TimeZoneID),
                    new SqlParameter("@PageType", PageType),
                    new SqlParameter("@IsNewTab", IsNewTab),
                    new SqlParameter("@CurrentTime", dtCurrentTime),
                    new SqlParameter("@IsDemo", IsDemo),
                    new SqlParameter("@RegionCode", strRegionCode),
                    new SqlParameter("@CurrentEventDuration", CurrentEventDuration),
                    new SqlParameter("@EventEndDate", dtEndDate),
                    new SqlParameter("@SessionMinuteCount", intSessionSpeakerMinutes),
                    new SqlParameter("@EventStartDate", dtStartDate),
                    new SqlParameter("@Action", clsReminders.A_GET)
                };
                return SqlHelper.ExecuteTable("UpdateSavedReminder", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                clsUtility.LogErrorMessage(null, null, GetType().Name, 0, "Error Accessing Notifications. "+ message);
                return null;
            }
        }
        public double GetAccountBalance(int EventpKey, int AccountPKey)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@UserID", AccountPKey),
                    new SqlParameter("@EventID", EventpKey),
                };
                DataTable dt = SqlHelper.ExecuteTable("SELECT isNull(Balance,0) as AccountBalance From dbo.getAccountBalance(@UserID, @EventID)", CommandType.Text, parameters);
                if (dt != null && dt.Rows.Count > 0)
                    return Convert.ToDouble(dt.Rows[0]["AccountBalance"]);
            }
            catch
            {

            }
            return 0;
        }
        public string UpdateNotificationVisibility(int EventID, int Value, int UserID, string SettingType)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter() { ParameterName = "@Account_pKey", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input, Value = UserID });
            parameters.Add(new SqlParameter() { ParameterName = "@pKey", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input, Value = Value });
            parameters.Add(new SqlParameter() { ParameterName = "@Event_pKey", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input, Value = EventID });
            parameters.Add(new SqlParameter() { ParameterName = "@SettingType", SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input, Value = SettingType });
            parameters.Add(new SqlParameter() { ParameterName = "@NotificationStatus", SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input, Value = false });
            parameters.Add(new SqlParameter() { ParameterName = "@Error", SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Output, Value = "" });
            if (SqlHelper.ExecuteNonQuery("sp_UpdateRemindersSettings", CommandType.StoredProcedure, parameters.ToArray()))
                return "OK";
            else
                return "Error Updating Reminder Settings";
        }
        #endregion LeftPanel

        #region  CommunityShowCase_LikePostAPI
        public async Task<string> UpdatePost_Likes(int PostID, int ID)
        {
            if (ID > 0)
                return new SqlOperation().UpdatePostLike(PostID, ID);
            else
                return "Kindly Login To Like/Dis-Like This Post";
        }
        #endregion  CommunityShowCase_LikePostAPI

        #region ProgramAgenda
        public DataTable GetSessionDetails(int EventID, bool ckShowRelated, bool bShowTopic, bool ckShowSpeak, string IsNew, bool bLiveStream)
        {
            try
            {
                string dbquery = "select * from dbo.getFullProgram_MVC(" + EventID.ToString() + "," + (ckShowRelated ? "1" : "0") + ", " + (bShowTopic ? "1" : "0") + ", " + (ckShowSpeak ? "1" : "0") + ", " + IsNew + ", " + (bLiveStream ? "1" : "0") +  ") ORDER BY DayNum,StartTime";
                return SqlHelper.ExecuteTable(dbquery, CommandType.Text, null);
            }
            catch
            {
                return null;
            }
        }
        #endregion ProgramAgenda

        #region VenueLodging
        public DataTable VRefreshImagesText_MVC(int VenueID, int EventID)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@VenueID", VenueID),
                    new SqlParameter("@EventID", EventID),
                    new SqlParameter("@ImageTypePKey", clsVenue.IMAGETYPE_Venue),
                };
                return SqlHelper.ExecuteTable("getLodgingTextImage_MVC", CommandType.StoredProcedure, parameters);
            }
            catch
            {
                return null;
            }
        }
        #endregion VenueLodging

        #region MyHistory
        public DataTable getListAuditEntityPage(int AccountpKey)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@AccountPkey", AccountpKey), };

                string dbquery = "SELECT t1.pKey, t1.AuditTargetTypeID as strText FROM SYS_AuditTargetTypes t1"
                + System.Environment.NewLine + " where t1.pkey in (select EntityType_pKey from Audit_Log where UpdatedByAccount_pKey = @AccountPkey ) Order by strText";

                return SqlHelper.ExecuteTable(dbquery, CommandType.Text, parameters);
            }
            catch
            {
                return null;
            }

        }
        #endregion MyHisorty

        #region EventSummary 
        public DataTable getPaymentMethodList()
        {
            try
            {
                string dbquery = " Select pKey, PaymentMethodID As strText FROM Sys_PaymentMethods Where pkey in (1,2,5) Order by sortorder";
                return SqlHelper.ExecuteTable(dbquery, CommandType.Text, null);
            }
            catch
            {
                return null;
            }
        }
        public DataSet getBookLetInfo(SqlConnection conn, string apppath, int EvtID, bool bShowDemoAccount, bool GlobalAdmin, bool IsPartner, int AccountID, DateTime dtcurrentTime)
        {
            try
            {

                StringBuilder qry = new StringBuilder("select t1.pkey as pkey,'" + apppath + "Documents/' +" + "  + t1.FileName as FileName,t2.Usage as Name from Event_GuideLines t1");
                qry.Append(" inner join sys_GuideLineType t2 on t2.pKey=t1.UsageType");
                qry.Append(" where t1.Status=1 and Event_pKey=" + EvtID.ToString() + " and Booklet=1");
                qry.Append(" order by Sequence;");

                qry.Append(" EXEC sp_getpointsAndNetLevel " + ((bShowDemoAccount && (GlobalAdmin || IsPartner)) ? "1" : "0") + "," + AccountID.ToString() + "," + EvtID.ToString() + ",'" + dtcurrentTime.ToString() + "';");
                SqlCommand cmd = new SqlCommand(qry.ToString());
                DataSet Ds = new DataSet();
                if (clsUtility.GetDataSet(conn, cmd, ref Ds))
                    return Ds;
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }
        public bool IsEventChange()
        {
            try
            {
                string qry = "select pKey from Event_changes";
                DataTable dt = SqlHelper.ExecuteTable(qry, CommandType.Text, null);
                if (dt != null && dt.Rows.Count > 0)
                    return true;
            }
            catch
            {

            }
            return false;
        }

        #endregion EventSummary

        #region  MyOptions
        public bool ArrangeSaveEvent(string txtSpArrangementEvent, string AccountPkey, string intCurEventPKey)
        {
            try
            {
                string qry = "Update Event_Accounts set SpecialRequest = @SpecialRequest Where Account_pkey = @AccountPkey  AND Event_pkey= @EventID";
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@AccountPkey", AccountPkey),
                    new SqlParameter("@EventID", intCurEventPKey),
                    new SqlParameter("@SpecialRequest",txtSpArrangementEvent),
                };
                if (SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters))
                    return true;
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public DataTable GetRefundVoucherSelectedData(string SelectedValue)
        {
            try
            {
                string qry = "select t1.pKey,t1.Account_pkey as AccPkey, t1.Amount, t1.IssuedOn,isnull(ReferenceReceipt,0) as ReferenceReceipt,isnull(PaymentMethod,0) as PaymentMethod,isnull(PaymentTransAction,'') as PaymentTransAction,"
                    + System.Environment.NewLine + " isnull(CardLastFour,'') as CardLastFour From Account_Vouchers t1 Where t1.pKey= @Selected";

                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@Selected", SelectedValue), };

                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public bool TransferVoucherSave(string selectedValue, string VoucherTransferEmail, string Email, string Comment, string AccountID)
        {
            try
            {
                Comment = "Transfer voucher from " + Email + " " + Comment.Trim();
                string qry = "Update Account_Vouchers Set CancellationComment=@CancellationComment,Email=@Email,DateUpdated=getdate(),UpdatedByAccount_pkey=@UpdatedByAccount_pkey  Where pKey =@pKey";
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@pKey", selectedValue),
                    new SqlParameter("@CancellationComment", Comment),
                    new SqlParameter("@Email",VoucherTransferEmail),
                    new SqlParameter("@UpdatedByAccount_pkey",AccountID),
                };
                if (SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters))
                    return true;
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public string MyOptionsSecurityGroupEmail()
        {
            try
            {
                string qry = "Select t2.Email,t2.Nickname,t2.pKey,t2.ContactName ,t1.SecurityGroup_pKey From SecurityGroup_Members t1 inner Join Account_List t2 on t2.pKey=t1.Account_pKey where t1.SecurityGroup_pKey = 1";
                DataTable dt = SqlHelper.ExecuteTable(qry, CommandType.Text, null);
                if (dt != null && dt.Rows.Count > 0)
                {
                    int count = 0;
                    string strAddress = "";
                    foreach (DataRow dr in dt.Rows)
                    {
                        count += 1;
                        strAddress += (count == 1) ? dr["Email"].ToString().Trim() : ";" + dr["Email"].ToString().Trim();
                    }
                    return strAddress;
                }
            }
            catch
            {

            }
            return "";
        }

        public DataTable GetMealDropdownsInfo(int EventId, string MealType, string DefaultMeal_Pkey)
        {
            try
            {

                string qry = "select t1.pKey, ISNULL(t2.SpecialMealID,t1.SpecialMealID ) as strText from Sys_SpecialMeals t1  LEFT JOIN SYS_SpecialMeals_EventWise t2 ON t1.pKey=t2.MealPkey AND t2.Event_pkey= @EventId";

                if (!string.IsNullOrEmpty(MealType) && MealType != "0")
                    qry += System.Environment.NewLine + "where t1.pkey in (" + String.Join(",", MealType.Split('<')).Replace(">", "").Trim(',') + ")";

                if (!string.IsNullOrEmpty(DefaultMeal_Pkey) && DefaultMeal_Pkey != "0")
                    qry += System.Environment.NewLine + "order by iif(t1.pKey=" + DefaultMeal_Pkey + ",0,t1.SortOrder);";
                else
                    qry +=  System.Environment.NewLine + " order by t1.SortOrder";

                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@EventId", EventId),
                };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch
            {
            }
            return null;
        }


        #endregion MyOptions

        #region  MyStaffPage
        public DataTable GetAttendeeRegLogs(DateTime RegStart, DateTime RegEnd, int EventID)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@Event_PKey", EventID),
                    new SqlParameter("@Start", RegStart),
                    new SqlParameter("@End", RegEnd),
                };
                return SqlHelper.ExecuteTable("getAttendeeRegistrationsLogs", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public DataTable GetCumulativeRegLogs(DateTime RegStart, DateTime RegEnd, int EventID, string intAttendanceGoal)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@Event_PKey", EventID),
                    new SqlParameter("@Start", RegStart),
                    new SqlParameter("@End", RegEnd),
                    new SqlParameter("@intAttendanceGoal", intAttendanceGoal),
                };
                return SqlHelper.ExecuteTable("getCumulativeRegistrationsInfo", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public DataTable GetCumulativeSpeakerLogs(DateTime RegStart, DateTime RegEnd, int EventID, int Total)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@Event_PKey", EventID),
                    new SqlParameter("@Start", RegStart),
                    new SqlParameter("@End", RegEnd),
                    new SqlParameter("@intTotal", Total),
                };
                return SqlHelper.ExecuteTable("getSpeakerStatusInfo", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public DataTable GetSpeakerDinner(int EventId)
        {
            try
            {
                string qry = "select (case t1.DinnerStatus_pkey When 4 Then 'Paid' Else 'Unpaid' End) as ChargeTypeID";
                qry = qry + System.Environment.NewLine + " ,count(t1.pkey) as NumSignedUp,(Case t1.DinnerStatus_pkey When  4 Then 'green' ELSE 'red' END) as ColorValue from event_accounts t1";
                qry = qry + System.Environment.NewLine + " inner join sys_dinnerstatuses t2 on t2.pkey = t1.DinnerStatus_pkey";
                qry = qry + System.Environment.NewLine + " where t1.event_pkey = @EventpKey and t1.DinnerStatus_pkey in(" + clsEventAccount.DINNER_AttendingPaid.ToString() + "," + clsEventAccount.DINNER_AttendingNotPaid.ToString() + ")";
                qry = qry + System.Environment.NewLine + " and Account_pKey  in( select Account_pKey from EventSession_Staff ESS inner join Event_Sessions ES ON ES.pkey=ESS.EventSession_pkey WHERE ES.Event_pKey= @EventpKey)";
                qry = qry + System.Environment.NewLine + "group by t1.DinnerStatus_pkey";

                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@EventpKey", EventId) };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        #endregion MyStaffPage


        #region IssuesReport
        public string GetSecurityGroupEmail()
        {
            string cc = "";
            try
            {
                string qry = "Select dbo.GetSecurityGroupEmail(37)";
                DataTable dt = SqlHelper.ExecuteTable(qry, CommandType.Text, null);
                if (dt != null && dt.Rows.Count > 0)
                    cc = dt.Rows[0][0].ToString();
            }
            catch
            {
            }
            return cc;
        }
        #endregion IssuesReport

        #region ZoomSession
        public DataTable RefreshBoothExhibitors(int intEventSession_pKey)
        {
            try
            {
                string qry = "SELECT t1.pKey,t2.OrganizationID,IIF(isnull(t2.Comment,'')='',t2.OrganizationID,replace(replace(isNull(t2.Comment,''),'{','<b>'),'}','</b>')) as Profile"
                + System.Environment.NewLine + " ,replace(replace(IIF(charindex('//',isnull(t2.url,''))>0 ,isnull(t2.url,'')	,'//'+ isnull(t2.url,'')),'!w','//w'),'!h','h') as URL"
                + System.Environment.NewLine + " From Event_Organizations t1"
                + System.Environment.NewLine + " left outer join Organization_list t2 on t1.Organization_pKey=t2.pKey"
                + System.Environment.NewLine + " where '" + intEventSession_pKey.ToString() + "' in (select Num from dbo.[CSVToNumberTable](t1.EventSession_pkeys,','))"
                + System.Environment.NewLine + " ORDER BY t2.OrganizationID ";

                return SqlHelper.ExecuteTable(qry, CommandType.Text, null);
            }
            catch
            {
            }
            return null;
        }
        #endregion ZoomSession

        #region  ScheduledEventMVC 
        public int FindEvent_OrgMVC(int AccountID, int EventID)
        {
            int Result = 0;
            try
            {
                string qry = "select  t3.pkey From   account_list t2 INNER JOIN Organization_List t1 ON t1.pkey=t2.ParentOrganization_pKey Inner Join Event_Organizations t3 on t3.Organization_pKey = t1.pkey"
                + System.Environment.NewLine + " where t2.pkey= @AccountID and t3.Event_pKey= @EventID";
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@EventID", EventID),
                    new SqlParameter("@AccountID", AccountID),
                };
                DataTable dt = SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
                if (dt != null && dt.Rows.Count>0)
                {
                    if (dt.Rows[0]["pkey"] != System.DBNull.Value)
                        Result = Convert.ToInt32(dt.Rows[0]["pkey"]);
                }
            }
            catch
            {
            }
            return Result;
        }
        public DataTable RefreshScheduledBooth(int intExhibitor_pKey, int intAccount_PKey, int intActiveEventPkey, string ddDate2_SelectedValue, DateTime dtCurEventStart, DateTime dtCurEventEnd)
        {
            try
            {
                SqlParameter SP = new SqlParameter("@Date", System.DBNull.Value);
                if ((ddDate2_SelectedValue != "0"))
                    SP = new SqlParameter("@Date", Convert.ToDateTime(ddDate2_SelectedValue));

                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@Exhibitor_pKey", intExhibitor_pKey),
                    new SqlParameter("@Account_pkey", intAccount_PKey),
                    new SqlParameter("@Event_pkey", intActiveEventPkey),
                    new SqlParameter("@EvantTime", clsEvent.getEventVenueTime()),
                    SP,
                    new SqlParameter("@dtStartDate", dtCurEventStart),
                    new SqlParameter("@dtEndDate", dtCurEventEnd)
                };
                return SqlHelper.ExecuteTable("Schedule_event_select_ALL", CommandType.StoredProcedure, parameters);
            }
            catch
            {

            }
            return null;
        }
        public void GetScheduledEventData(int EventId, DateTime StartDate, DateTime EndDate, ref DateTime Start, ref DateTime End)
        {
            try
            {
                string qry = "DECLARE @TimeOffSET Decimal (5,2) = 0; SELECT @TimeOffSET  = TimeOffset From  Event_List INNER JOIN StandardTime ON StandardTime.Pkey = Event_List.StandardTime_Pkey  Where Event_List.pKey = @EventID"
                + System.Environment.NewLine + " SELECT DATEADD(HOUR,@TimeOffSET,@StartDate) as StartTime ,DATEADD(HOUR,@TimeOffSET,@EndDate) as EndTime;";

                SqlParameter[] parameters = new SqlParameter[] {
                   new SqlParameter("@EventID", EventId),
                   new SqlParameter("@StartDate", StartDate),
                   new SqlParameter("@EndDate", EndDate),
               };
                DataTable dt = SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
                if (dt != null && dt.Rows.Count>0)
                {
                    if (dt.Rows[0]["StartTime"] != System.DBNull.Value)
                        Start = Convert.ToDateTime(dt.Rows[0]["StartTime"]);
                    if (dt.Rows[0]["EndTime"] != System.DBNull.Value)
                        End =  Convert.ToDateTime(dt.Rows[0]["EndTime"]);
                }
            }
            catch
            {
            }
        }
        public DataTable GetScheduleEventDetails(int SelectCase, int? Meeting_pkey = null, int? ES_pKey = null, int? RPKey = null)
        {
            try
            {
                string qry = "";
                switch (SelectCase)
                {
                    case 1:
                        qry = "SELECT ISNULL(MeetingTitle,'') as Title ,ISNULL(MeetingPurpose,'') as Description ,'' as Speaker from MeetingPlanner where pkey=@Mkey";
                        return SqlHelper.ExecuteTable(qry, CommandType.Text, new SqlParameter[] { new SqlParameter("@Mkey", Meeting_pkey) });
                    case 2:
                        qry = "SELECT t2.SessionTitle as Title,t2.Description as Description,ISNULL(dbo.getSpeakers(t1.pkey),'') as Speaker from Event_Sessions t1 Inner JOIN Session_List t2 ON t2.pkey=t1.Session_pKey "
                        + System.Environment.NewLine +" WHERE t1.pkey= @ESID";
                        return SqlHelper.ExecuteTable(qry, CommandType.Text, new SqlParameter[] { new SqlParameter("@ESID", ES_pKey) });
                    case 3:
                        qry = "SELECT ISNULL(title,'') as Title ,ISNULL(Description,'') as Description ,'' as Speaker from RoundTableSchedule where pkey= @RPKey";
                        return SqlHelper.ExecuteTable(qry, CommandType.Text, new SqlParameter[] { new SqlParameter("@RPKey", RPKey) });
                }
            }
            catch
            {
            }
            return null;
        }
        public bool AddToMyScheduleEvent(int AccountId, int EventId, int R_pkey, DateTime CurrentTime, string TypeOfSchedule, bool Leave)
        {
            bool result = true;

            try
            {
                string qry = "EXEC [SP_AddToMySchedule] @accPkey,@eventKey,@roundTableSchedule_pkey,@currentTime,@TypeOfSchedule,@Leave;";
                SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@accPkey", AccountId),
                        new SqlParameter("@eventKey",EventId),
                        new SqlParameter("@roundTableSchedule_pkey", R_pkey),
                        new SqlParameter("@currentTime", CurrentTime),
                        new SqlParameter("@TypeOfSchedule",TypeOfSchedule),
                        new SqlParameter("@Leave", Leave),
                };
                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
            }
            catch
            {
                result =false;
            }
            return result;
        }
        public void ScheduleEventJoinMeeting(int MpKey, int PendingMPKey, int AccountId)
        {
            try
            {
                string qry = " update EventUrls SET IsJoin=1 where pKey=@MpKey" +System.Environment.NewLine+ " Update MeetingPlanner SET IsJOIN=1 Where pkey=@Pkey "
               + System.Environment.NewLine + " Update MeetingPlanner_Detail set isjoin=1 where MeetingPlanner_pkey=@Pkey and Account_pkey=@account_pkey";
                SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@Pkey", PendingMPKey.ToString()),
                        new SqlParameter("@account_pkey", AccountId),
                        new SqlParameter("@MpKey", MpKey),
                };
                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
            }
            catch
            {

            }
        }
        #endregion ScheduledEventMVC


        #region  MYSessionChair
        public DataTable getChairPreference()
        {
            try
            {
                string qry = "Select Pkey,SessionChairPreference AS strText FROm sys_SessionChairPreference Where Active=1 Order by SortOrder ASC";
                return SqlHelper.ExecuteTable(qry, CommandType.Text, null);
            }
            catch
            {

            }
            return null;
        }
        public DataTable getSessionCombo(int AccountID, int EventID)
        {
            try
            {
                string qry = "With Qry  As (SELECT * From EventSession_Staff Where ISNULL(IsSessionChair,0) = 1)"
                     + System.Environment.NewLine + " SELECT DISTINCT t1.pKey,'('+ t2.Track_Prefix + t1.SessionID +') ' + t1.SessionTitle AS strText,IIF(EXISTS (SELECT 1 From Qry t3 Where t3.EventSession_pKey = t2.pkey AND t3.Account_pKey = @AccountId),1,0) as IsSessionChair"
                     + System.Environment.NewLine + " From  Session_List t1 INNER JOIN event_sessions t2 on t2.Session_pkey = t1.pkey"
                     + System.Environment.NewLine + " Where t2.Event_pKey = @EventId and ISNULL(t2.IsScheduled,0)=1 AND isNull(t1.NumChairs,1) > 0 "
                     + System.Environment.NewLine + " Order by strText";

                SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@AccountId",AccountID),
                        new SqlParameter("@EventId",EventID),
                };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch
            {

            }
            return null;
        }
        public DataTable getTracksList(int EventId)
        {
            try
            {
                string qry = "Select distinct t3.pkey,('('+ t3.Prefix +') '+ t3.TrackID)  as TrackID from Event_Sessions t1"
                    + System.Environment.NewLine + " inner join session_list t2 On t2.pkey = t1.Session_pKey"
                    + System.Environment.NewLine + " inner join sys_Tracks t3 on t3.pkey = t1.track_pKey"
                    + System.Environment.NewLine + " where t1.event_pkey = @EventId and ISNULL(t1.IsScheduled,0)=1 AND isNull(t2.NumChairs,1) > 0  "
                    + System.Environment.NewLine + " Order By TrackID";
                SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@EventId",EventId),
                };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch
            {

            }
            return null;
        }
        public DataTable GetSessionChairInfo(int AccountId, int EventId)
        {
            try
            {
                string qry = " Select Isnull(InterestinBeingaChair,0) InterestinBeingaChair,SessionChairPreference,ISNULL(Track_pkey,0) AS Track_pkey,Session_pKey,PreferedTime From Event_Accounts"
                    + System.Environment.NewLine+ " where event_pKey=@EventpKey and account_pkey=@Accountpkey";

                SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@EventpKey",EventId),
                        new SqlParameter("@Accountpkey",AccountId),
                };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch
            {

            }
            return null;
        }

        public bool ClearSesssionChairPreference(int AccountId, int EventId)
        {

            try
            {
                string qry = "Update Event_Accounts Set InterestinBeingaChair=0,SessionChairPreference=0,Track_pkey='',Session_pKey='',PreferedTime='' ,InterestAdded_by=@Accountpkey where event_pKey=@EventpKey and account_pkey=@Accountpkey";
                SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@EventpKey",EventId),
                        new SqlParameter("@Accountpkey",AccountId),
                };
                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
                return true;
            }
            catch
            {
                return false;
            }

        }
        public bool SaveSessionChairPreference(int AccountId, int EventId, int rblSessionVal, int chairPreference, string time, string getTrackList, string getSessionList)
        {
            try
            {
                string qry = "Update Event_Accounts Set InterestinBeingaChair=@InterestinBeingaChair";
                if (rblSessionVal == 0)
                    qry = qry + System.Environment.NewLine+",ChairPriority = 4";

                qry += System.Environment.NewLine+  ",SessionChairPreference=@SessionChairPreference,Track_pkey=@Track_pKey,Session_pKey=@Session_pkey,PreferedTime=@PreferedTime ,InterestAdded_by=@Accountpkey"
                    + System.Environment.NewLine+  " where event_pKey=@EventpKey and account_pkey=@Accountpkey";

                SqlParameter[] parameters = new SqlParameter[] {
                   new SqlParameter("@InterestinBeingaChair", rblSessionVal),
                   new SqlParameter("@SessionChairPreference", chairPreference),
                   new SqlParameter("@Track_pKey", getTrackList),
                   new SqlParameter("@Session_pkey", getSessionList),
                   new SqlParameter("@PreferedTime", time),
                   new SqlParameter("@EventpKey", EventId),
                   new SqlParameter("@Accountpkey", AccountId),
                };
                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
                return true;
            }
            catch
            {
                return false;
            }

        }

        #endregion MySessionChair


        #region  SharedEventDropdown
        public DataTable FillEventDropdown(int AccountId, int EventId, bool RefillEventDropDown = false, bool bSpeacialAccount = false)
        {
            DataTable dt = new DataTable();
            try
            {
                string qry = "select t1.pKey, iif(len(isnull(t1.PublicName,t1.EventID))>23,SUBSTRING(isnull(t1.PublicName,t1.EventID), 1, 23)+'..', isnull(t1.PublicName,t1.EventID)) as strText"
                    + System.Environment.NewLine + " , t1.EventFullName as strName, t1.EventStatus_pKey, isnull(t2.Account_pKey,isnull(t5.Account_pKey,0)) as Account_pKey, t1.RegEndDate,"
                    + System.Environment.NewLine + " isnull(t3.IsActive,0) as CanAccessEvent, isnull(t3.NoOfDays,0) as NoOfDays, isnull(t3.DateStarted,getdate()) as DateStarted,isnull(t1.IsDemoEvent,0) as IsDemoEvent,t1.EventID"
                    + System.Environment.NewLine + " from Event_List t1 left outer join Event_Accounts t2 on t2.Event_pKey = t1.pKey and t2.Account_pKey =  @AccountID"
                    + System.Environment.NewLine + " left outer join Event_SpecialAccounts t3 on t3.Event_pKey=t1.pKey and t3.Account_pkey = @AccountID"
                    + System.Environment.NewLine + " left outer join (EventOrganization_Staff t5 join Event_Organizations t4 on t5.EventOrganization_pkey=t4.pKey And isnull(t5.ParticipationRolesForEventManager,0)=1 And isnull(t4.ShowOnEventPartner,0)=1)"
                    + System.Environment.NewLine + " on t4.Event_pKey=t1.pkey And t5.Account_pKey= @AccountID"
                    + System.Environment.NewLine + " where ((t1.RegStartDate<=getdate() and t1.RegEndDate>=getdate()) or (t1.RegEndDate<getdate() and t1.EventStatus_pKey=1) or t1.pkey = @EventID"
                    + System.Environment.NewLine + " or (t1.RegStartDate<=getdate() and (isnull(t1.eventstatus_pkey,0)=1 or isnull(t3.IsActive,0)=1) and t2.Account_pKey = @AccountID"
                    + System.Environment.NewLine + " and isnull(t1.eventstatus_pkey,0)<>" + clsEvent.STATUS_Cancelled.ToString() + "))"
                    + System.Environment.NewLine + " order by t1.StartDate desc";

                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@EventID", EventId),
                    new SqlParameter("@AccountID", AccountId),
                };

                dt = SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
                if (dt != null && dt.Rows.Count>0)
                    return dt;
            }
            catch
            {

            }
            return null;
        }

        public DataTable GetEventRelatedData(int EventId)
        {
            try
            {
                string qry = "select t1.pKey, t1.EventFullName,ISNULL(t1.EventID,'') as EventID,isnull(t1.PublicName,t1.EventID) as PublicName,ISNULL(t1.EventType_PKey,0) as EventType_PKey from Event_List t1"
               + System.Environment.NewLine + " where pkey= @EventID order by t1.StartDate";

                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@EventID", EventId),
                };

                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch
            {

            }
            return null;
        }

        public DataTable LoadAccountPrivilages(int EventID, int AccountID)
        {
            try
            {
                string qry = "Select t1.Privilege_pKey, t1.ColKey From dbo.getSecurityPrivileges(@EvtPKey, @AcctPKey) t1";
                SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@EvtPKey", EventID),
                        new SqlParameter("@AcctPKey", AccountID),
                };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch
            {
            }
            return null;
        }

        public DataTable GetUpComingSession(int AccountID, int EventID, DateTime dtCurrentTime)
        {
            DataTable dt = new DataTable();
            try
            {
                SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@Event_PKey", EventID),
                        new SqlParameter("@Account_pKey", AccountID),
                        new SqlParameter("@CurrentTime", dtCurrentTime),
            };

                dt = SqlHelper.ExecuteTable("getUpComingSession", CommandType.StoredProcedure, parameters);
            }
            catch
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 0, "Error While Updating Next Sessions In Menu");
            }
            return dt;
        }
        public DataTable InitializeEventsCollection()
        {
            DataTable dt = new DataTable();
            if (System.Web.HttpContext.Current.Cache["HomePageMVC"] != null)
                dt = (DataTable)System.Web.HttpContext.Current.Cache["HomePageMVC"];

            if (dt!= null &&  dt.Rows.Count>0 && dt.Columns["RowIndex"] != null)
                return dt;
            else
            {
                try
                {
                    string qry = "Select t1.pKey, t1.EventID,t1.EventType_PKey,t1.HomelinkDisable,isNull(t2.VenueID,'Not Specified') as Venue,t1.HomeBanner";
                    qry = qry + Environment.NewLine + " ,t2.City as LocationCity, t3.StateID as LocationState,t1.EventFullname,t1.StartDate,t1.EndDate,(t2.FileGUID+'_'+t2.ImageSmall) as VenueSmall";
                    qry = qry + Environment.NewLine + " ,iif(len(isnull(t1.PublicName,t1.EventID))>23,SUBSTRING(isnull(t1.PublicName,t1.EventID), 1, 23)+'..',isnull(t1.PublicName,t1.EventID)) as strText,t1.EventStatus_pKey,t1.RegEndDate";
                    qry = qry + Environment.NewLine + " ,ROW_NUMBER() Over(ORDER BY t1.StartDate asc) as RowIndex From Event_List t1";
                    qry = qry + Environment.NewLine + " Left Outer Join venue_List t2 On t2.pKey = t1.venue_pkey";
                    qry = qry + Environment.NewLine + " Left Outer Join SYS_States t3 On t3.pkey = t2.State_pkey";
                    qry = qry + Environment.NewLine + " Where t1.PublicPageStartDate<=Convert(Date, CONVERT(char(10), GetDate(),126)) and dateadd(DD,1,t1.PublicPageEndDate)>=Convert(Date, CONVERT(char(10), GetDate(),126))";
                    qry = qry + Environment.NewLine + " and t1.EventStatus_pKey not in (4,5)";
                    qry = qry + Environment.NewLine + " Order by t1.StartDate";
                    dt = SqlHelper.ExecuteTable(qry, CommandType.Text, null);
                    if (dt != null && dt.Rows.Count>0)
                    {
                        System.Web.HttpContext.Current.Cache.Insert("HomePage", dt, null, DateTime.MaxValue, TimeSpan.FromMinutes(300));
                        return dt;
                    }
                }
                catch
                {
                }
            }
            return dt;
        }
        #endregion SharedEventDropdown

        #region MyFAQs
        public bool VerifyIsPartner(int AccountID, int EventID)
        {
            try
            {
                string qry = "SELECT IIF(t13.pKey > 0,1,0) as IsPartner  from account_list t1"
                + System.Environment.NewLine + "  Left outer join event_accounts t12 On t12.account_pkey = t1.pkey And t12.event_pkey = @EventID"
                + System.Environment.NewLine + " Left Outer Join Event_Organizations t13 on t13.event_pkey = t12.event_pkey and t13.organization_pkey = t1.ParentOrganization_pKey and t13.ParticipationStatus_pKey =  @ParticipationStatus"
                + System.Environment.NewLine + "  Where t1.pKey =@AccountID";

                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@EventID", AccountID),
                    new SqlParameter("@AccountID", EventID),
                    new SqlParameter("@ParticipationStatus", clsEvent.PARTICIPANTSTATUS_Attending),
                };

                int result = SqlHelper.ExecuteScaler(qry, CommandType.Text, parameters);
                if (result>0)
                    return true;
            }
            catch
            {

            }
            return false;
        }
        #endregion MYFAQs

        #region MyOrganizationMVC
        public DataTable RefreshHasEventOrg(int Event_pKey, int Account_pKey)
        {
            try
            {
                StringBuilder q = new StringBuilder("select  t3.pkey From account_list t2");
                q.Append(System.Environment.NewLine + " INNER JOIN Organization_List t1 ON t1.pkey=t2.ParentOrganization_pKey Inner Join Event_Organizations t3 on t3.Organization_pKey = t1.pkey");
                q.Append(System.Environment.NewLine + " where isnull(t3.ShowOnEventPartner,0)=1 and t2.pkey= @Account_pKey  and t3.Event_pKey= @Event_PKey  ");
                q.Append(System.Environment.NewLine + " AND t2.pkey IN (select Account_pKey from Event_Accounts where isnull(ParticipationStatus_pKey,0)=1 And Account_pKey= @Account_pKey AND Event_pKey=@Event_PKey) ");

                SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@Event_PKey", Event_pKey),
                        new SqlParameter("@Account_pKey", Account_pKey),

                };
                return SqlHelper.ExecuteTable(q.ToString(), CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                string exception = ex.Message;
                return null;
            }
        }
        public DataTable GetDateList(int Event_pKey)
        {
            try
            {
                string qry = "Select FORMAT( CAST (StartTime as date) ,'MM/dd/yy') as pkey  ,FORMAT( CAST (StartTime as date) ,'MM/dd/yy') as strtext from event_sessions t0 Inner JOIN Session_list t1 ON t1.pKey=t0.Session_pKey Where Event_pKey= @Event_Pkey ANd ISNULL(t1.NumChairs,0) >0"
                + System.Environment.NewLine + "GROUP by FORMAT( CAST (StartTime as date) ,'MM/dd/yy')";
                SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@Event_Pkey", Event_pKey),
                };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                string exception = ex.Message;
                return null;
            }
        }

        #endregion MyOrganizationMVC


    }
}
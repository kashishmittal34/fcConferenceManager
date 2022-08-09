using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using fcConferenceManager;
using MAGI_API.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace MAGI_API.SignalR
{
    public class LocalHub : Hub
    {
        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        public void Onconnected(string chatUserId, string sessionID, string name, string pageUrl, bool loadChat, string eventKey)
        {
            try
            {
                if (chatUserId != "0")
                {
                    string myIPAdress = GetIP();
                    var conId = Context.ConnectionId;
                    var usr = MAGI_API.Models.OnlineUsers.getByChatUserID(chatUserId);
                    int AccKey = Convert.ToInt32(chatUserId);
                    string strBrowser = HttpContext.Current.Request.Browser.Browser;

                    if (usr != null)
                        MAGI_API.Models.OnlineUsers.Update(AccKey, sessionID, conId, pageUrl, myIPAdress, loadChat, strBrowser);
                    else
                    {
                        MAGI_API.Models.OnlineUsers.Add(AccKey, sessionID, conId, pageUrl, myIPAdress, loadChat, strBrowser);
                        usr = MAGI_API.Models.OnlineUsers.getByChatUserID(chatUserId);
                    }

                    Groups.Add(conId, chatUserId);

                    if (loadChat)
                    {
                        Clients.Client(conId).updateSender(usr, chatUserId + "~" + conId, GetIP());

                        string[] Result = ChatOperations.CheckForAnyBroadCast(Convert.ToInt32(chatUserId), Convert.ToInt32(eventKey));

                        if (Result != null && Result.Length > 0)
                        {
                            string result = Result[0];
                            if (!string.IsNullOrEmpty(result))
                            {
                                string[] strArr = result.Split('^');
                                string strMsg = strArr[0];
                                int intKey = Convert.ToInt32(strArr[1]);
                                Clients.Group(chatUserId).adminBroadcast(strMsg, intKey);
                            }

                            //if (Result.Length > 1)
                            //{
                            //    string sessionBroadcast = Result[1];
                            //    if(!string.IsNullOrEmpty(sessionBroadcast))
                            //    {
                            //        string[] strArr = sessionBroadcast.Split('^');
                            //        if(strArr.Length > 0)
                            //        {
                            //            var strList = strArr.ToList();
                            //            strList.RemoveAt(strList.Count - 1);
                            //            foreach (string str in strList)
                            //            {
                            //                Clients.All.openThisSessionForAll(str);
                            //            }
                            //        }
                            //    }
                            //}
                        }
                    }
                }
            }
            catch (Exception e)
            {
                chatUserId = (chatUserId == null) ? "0" : chatUserId;
                Clients.Group(chatUserId).showError(e.ToString());
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            //if (!stopCalled)
            //{
                var conId = Context.ConnectionId;
                MAGI_API.Models.OnlineUsers.RemoveByChatSession(conId);
            //}
            return base.OnDisconnected(stopCalled);
        }

        public void setNotified(int myID, int eventID, int NetLevel)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@NetLevel", NetLevel),
                    new SqlParameter("@paramEvent_pkey", eventID),
                    new SqlParameter("@paramAccountKey", myID),
                    new SqlParameter("@paramDateTime", clsEvent.getEventVenueTime())
                };
                DataSet ds = new DataSet();
                ds = SqlHelper.ExecuteSet("sp_NetworkUpgradeCount", CommandType.StoredProcedure, sqlParameters);

                var conId = Context.ConnectionId;

                string strMsg = string.Empty;
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    strMsg = ds.Tables[0].Rows[0]["brightText"].ToString();
                    //Clients.Group(myID.ToString()).customMsg(strMsg);
                    //Clients.Group(myID.ToString()).celebrateSound(2);
                    //Clients.Client(conId).customMsg(strMsg);
                    Clients.Client(conId).celebrateSound(2, NetLevel, strMsg);
                }
                //if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0 && Convert.ToInt16(ds.Tables[1].Rows[0]["attempts"]) < 1)
                //{
                //    //Clients.Group(myID.ToString()).giveChanceToSpin(NetLevel);
                //    Clients.Client(conId).giveChanceToSpin(NetLevel, strMsg);
                //}
            }
            catch (Exception e)
            {
                Clients.Group(myID.ToString()).showError(e.ToString());
            }
        }

        private Tuple<string[], string> GetCrisisGroupMembers(int acc_key)
        {
            try
            {
                string qry = "select mem.Account_pKey from";
                qry = qry + Environment.NewLine + "SecurityGroup_List grp";
                qry = qry + Environment.NewLine + "inner join SecurityGroup_Members mem";
                qry = qry + Environment.NewLine + "on mem.Account_pKey = grp.pKey";
                qry = qry + Environment.NewLine + "where grp.[SecurityGroupID] = 'Crisis Alerts'";

                qry = qry + Environment.NewLine + "select (ISNULL(ac.Firstname,'') + ' ' + ISNULL(ac.Lastname,'')) as fullName,[Phone],[Email] from Account_List ac where pkey = " + acc_key.ToString();

                DataSet ds = new DataSet();
                ds = SqlHelper.ExecuteSet(qry, CommandType.Text, null);

                List<string> peopleList = ds.Tables[0].AsEnumerable()
                .Select(row => row.Field<string>(0)
                ).ToList();

                string strMsgSignature = string.Empty;
                if (ds.Tables[1].Rows.Count > 0)
                {
                    strMsgSignature += ds.Tables[1].Rows[0]["fullName"].ToString() + "<br/>";
                    strMsgSignature += ds.Tables[1].Rows[0]["Phone"].ToString() + "<br/>";
                    strMsgSignature += ds.Tables[1].Rows[0]["Email"].ToString() + "<br/>";
                }

                return Tuple.Create(peopleList.ToArray<string>(), strMsgSignature);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public void CrisisAlert(string strTitle, string strBody, int account_ID, string crisisTh, string didToWhom, int eventKey, bool replied, string strReplied, bool sendToAll)
        {
            try
            {
                DataSet peopleWithMe = ChatOperations.GetCrisisGroupMembers(account_ID);

                List<string> CrisisMembers = peopleWithMe.Tables[0].AsEnumerable()
                .Select(row => row.Field<string>(0)
                ).ToList();

                if (peopleWithMe.Tables[1].Rows.Count > 0)
                {
                    string signature = "<b>From:" + Environment.NewLine +
                        peopleWithMe.Tables[1].Rows[0]["fullName"].ToString() + Environment.NewLine +
                        (
                            !string.IsNullOrEmpty(peopleWithMe.Tables[1].Rows[0]["MobileNumber"].ToString()) ?
                            (peopleWithMe.Tables[1].Rows[0]["MobileNumber"].ToString() + Environment.NewLine) : ""
                        ) +
                        peopleWithMe.Tables[1].Rows[0]["Email"].ToString() + "</b>" + Environment.NewLine;

                    string FullBody = string.Empty;
                    if (!replied)
                    {
                        FullBody = signature + strBody;
                    }
                    else
                    {
                        FullBody = strBody + Environment.NewLine + Environment.NewLine + signature + Environment.NewLine + strReplied;
                    }

                    int id = ChatOperations.CrisisInsert(account_ID, strBody, strTitle, crisisTh, didToWhom, eventKey);
                    if (id != 0)
                    {
                        if (sendToAll)
                        {
                            if (!string.IsNullOrEmpty(didToWhom))
                                CrisisMembers.Add(didToWhom);

                            Clients.Groups(CrisisMembers).crisisAlertOnClient(strTitle, FullBody, account_ID, crisisTh, id, replied);
                            var _hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<ChatHub>();
                            _hubContext.Clients.Groups(CrisisMembers).crisisAlertOnClient(strTitle, FullBody, account_ID, crisisTh, id, replied);
                        }
                        else
                        {
                            Clients.Group(didToWhom).crisisAlertOnClient(strTitle, FullBody, account_ID, crisisTh, id, replied);
                            var _hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<ChatHub>();
                            _hubContext.Clients.Group(didToWhom).crisisAlertOnClient(strTitle, FullBody, account_ID, crisisTh, id, replied);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Clients.Group(account_ID.ToString()).showError(ex.ToString());
            }
        }

        public void crisisAcknoledged(string myID, string paraCrisisKey)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paraAccKey", myID),
                    new SqlParameter("@paraCrisisKey", paraCrisisKey)
                };
                string qry = "insert into [AcknowledgeCrisis](accountKey,[Crisis_pKey],AcknowledgeDateTime)values(@paraAccKey,@paraCrisisKey,GETDATE());";
                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, sqlParameters);
            }
            catch (Exception e)
            {
                Clients.Group(myID.ToString()).showError(e.ToString());
            }
        }

        public void updateConnectionIconsForPnl(string overAllPeopleIds)
        {
            var conId = Context.ConnectionId;
            try
            {
                DataTable dt = new DataTable();
                dt = SqlHelper.ExecuteTable(overAllPeopleIds, CommandType.Text, null);
                Clients.Client(conId).showError(dt);
            }
            catch (Exception e)
            {
                Clients.Client(conId).showError(e.ToString());
            }
        }

        public void broadcastThisText(string myID, string strMsg, bool forStaffOnly)
        {
            try
            {
                int id = BroadcastInsert(myID, strMsg, 1, forStaffOnly);
                if (id != 0)
                {
                    var conId = Context.ConnectionId;

                    if (forStaffOnly)
                    {
                        var StaffNotMe = GetStaffNotMe(myID);
                        Clients.Groups(StaffNotMe).adminBroadcast(strMsg, id);
                        var _hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<ChatHub>();
                        _hubContext.Clients.Groups(StaffNotMe).adminBroadcast(strMsg, id);
                        Clients.Client(conId).customMsg("Message broadcast for staff only");
                    }
                    else
                    {
                        string[] arStr = { conId };
                        Clients.AllExcept(arStr).adminBroadcast(strMsg, id);
                        var _hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<ChatHub>();
                        _hubContext.Clients.AllExcept(arStr).adminBroadcast(strMsg, id);
                        Clients.Client(conId).customMsg("Message broadcasted");
                    }
                }
            }
            catch (Exception e)
            {
                Clients.Group(myID.ToString()).customMsg("Error in broadcasting the message");
                Clients.Group(myID.ToString()).showError(e.ToString());
            }
        }

        public void broadcastAcknoledged(string myID, string paraBroadCastKey)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paraAccKey", myID),
                    new SqlParameter("@paraBroadCastKey", paraBroadCastKey)
                };
                string qry = "insert into [AcknowledgeBroadCast](accountKey,[BroadCast_pKey],AcknowledgeDateTime)values(@paraAccKey,@paraBroadCastKey,GETDATE());";
                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, sqlParameters);
            }
            catch (Exception e)
            {
                Clients.Group(myID.ToString()).showError(e.ToString());
            }
        }

        private int BroadcastInsert(string myID, string strMsg, int type, bool forStaffonly)
        {
            try
            {
                ////////////Insert in DB/////////////////
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paraMsg", strMsg),
                    new SqlParameter("@paraAccKey", myID),
                    new SqlParameter("@paraBroadcastType", type),
                    new SqlParameter("@paramDateTime", clsEvent.getEventVenueTime()),
                    new SqlParameter("@paramforStaffonly", forStaffonly)
                };
                string qry = "insert into dbo.[BroadCastMessage]([accountKey],[Message],[DateTime],[BroadcastType],[ForStaffOnly])values(@paraAccKey,@paraMsg,@paramDateTime,@paraBroadcastType,@paramforStaffonly);";
                qry = qry + Environment.NewLine + "SELECT isnull(SCOPE_IDENTITY(),0) AS [SCOPE_IDENTITY]";
                DataTable dt = new DataTable();
                dt = SqlHelper.ExecuteTable(qry, CommandType.Text, sqlParameters);

                if (dt.Rows.Count > 0)
                {
                    return Convert.ToInt32(dt.Rows[0]["SCOPE_IDENTITY"]);
                }
                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public void openSessionForAll(int myID, string sessionID, int duration)
        {
            try
            {
                sessionID = sessionID.ToUpper();
                int id = InsertSessionBroadCast(myID, sessionID, duration);
                if (id != 0)
                {
                    var conId = Context.ConnectionId;
                    Clients.All.openThisSessionForAll(sessionID);
                    Clients.Client(conId).customMsg("Session broadcasted");

                    var _hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<ChatHub>();
                    _hubContext.Clients.All.openThisSessionForAll(sessionID);
                }
            }
            catch (Exception e)
            {
                Clients.Group(myID.ToString()).customMsg("Error in sending this broadcast.");
                Clients.Group(myID.ToString()).showError(e.ToString());
            }
        }

        public void openVideoChat(string myID, string eventKey, string urlKey)
        {
            var conId = Context.ConnectionId;
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@parammyID", myID),
                    new SqlParameter("@parameventKey", eventKey),
                    new SqlParameter("@paramurlKey", urlKey),
                    new SqlParameter("@paramDateTime", clsEvent.getEventVenueTime())
                };

                string qry = "if(((SELECT count(1) FROM VideoChatTrack WHERE [eventKey]=@parameventKey AND [EventUrlsKey]=@paramurlKey) <= 20) and";
                qry = qry + Environment.NewLine + "(NOT EXISTS(SELECT 1 FROM VideoChatTrack WHERE [eventKey]=@parameventKey AND [accountKey]=@parammyID AND [EventUrlsKey]=@paramurlKey)))";
                qry = qry + Environment.NewLine + "BEGIN";
                qry = qry + Environment.NewLine + "insert into VideoChatTrack([eventKey],[accountKey],[EventUrlsKey],DateOfJoining) values(@parameventKey,@parammyID,@paramurlKey,@paramDateTime);";
                qry = qry + Environment.NewLine + "select top(1) ur.[URL] from EventUrls ur where ur.pKey = @paramurlKey";
                qry = qry + Environment.NewLine + "END";
                qry = qry + Environment.NewLine + "ELSE IF(EXISTS(SELECT 1 FROM VideoChatTrack WHERE [eventKey]=@parameventKey AND [accountKey]=@parammyID AND [EventUrlsKey]=@paramurlKey))";
                qry = qry + Environment.NewLine + "BEGIN";
                qry = qry + Environment.NewLine + "select top(1) ur.[URL] from EventUrls ur where ur.pKey = @paramurlKey";
                qry = qry + Environment.NewLine + "END";
                DataTable dt = new DataTable();

                dt = SqlHelper.ExecuteTable(qry, CommandType.Text, sqlParameters);
                if (dt.Rows.Count > 0)
                    Clients.Client(conId).takeToMeeting(dt.Rows[0]["URL"].ToString());
                else
                    Clients.Client(conId).customMsg("Sorry, meeting room has filled up with 20 people");
            }
            catch (Exception ex)
            {
                Clients.Group(myID).showError(ex.ToString());
            }
        }

        private int InsertSessionBroadCast(int accKey, string sessionID, int duration)
        {
            try
            {
                ////////////Insert in DB/////////////////
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paraAccKey", accKey),
                    new SqlParameter("@paramSessionID", sessionID),
                    new SqlParameter("@paramDateTime", clsEvent.getEventVenueTime()),
                    new SqlParameter("@paramOpenTill", duration)
                };
                string qry = "insert into dbo.[OpenSessionBroadCast]([account_pkey],[sessionKey],[TimeOfBroadCast],[OpenTill])values(@paraAccKey,@paramSessionID,@paramDateTime,@paramOpenTill);";
                qry = qry + Environment.NewLine + "SELECT isnull(SCOPE_IDENTITY(),0) AS [SCOPE_IDENTITY]";
                DataTable dt = new DataTable();
                dt = SqlHelper.ExecuteTable(qry, CommandType.Text, sqlParameters);

                if (dt.Rows.Count > 0)
                {
                    return Convert.ToInt32(dt.Rows[0]["SCOPE_IDENTITY"]);
                }
                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        private static string GetIP()
        {
            try
            {
                if (System.Web.HttpContext.Current != null)
                {
                    string ip = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(ip))
                    {
                        ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                    }
                    return ip.Split(',')[0].Split(':')[0];
                }
                else
                    return string.Empty;
            }
            catch { return string.Empty; }
        }

        private string[] GetStaffNotMe(string myID)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paraAccKey", myID)
                };
                string qry = "select cast(ac.pKey as varchar) as pKey from [Account_List] ac where (isnull(ac.StaffMember,0) = 1 or isnull(ac.GlobalAdministrator,0) = 1) and ac.pKey <> @paraAccKey;";
                DataTable dt = new DataTable();
                dt = SqlHelper.ExecuteTable(qry, CommandType.Text, sqlParameters);

                List<string> peopleList = dt.AsEnumerable()
                .Select(row => row.Field<string>(0)
                ).ToList();

                return peopleList.ToArray<string>();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}
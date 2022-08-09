using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using MAGI_API.Models;
using fcConferenceManager;
using System.Runtime.CompilerServices;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace MAGI_API.SignalR
{
    public class ChatHub : Hub
    {
        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
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

        public void Onconnected(string chatUserId, string sessionID, string name, string pageUrl, bool loadChat)
        {
            if (chatUserId != "0")
            {
                string myIPAdress = GetIP();
                var conId = Context.ConnectionId;
                var usr = MAGI_API.Models.OnlineUsers.getBySession(sessionID);
                int AccKey = Convert.ToInt32(chatUserId);
                string strBrowser = HttpContext.Current.Request.Browser.Browser;

                if (usr != null)
                {
                    MAGI_API.Models.OnlineUsers.Update(AccKey, sessionID, conId, pageUrl, myIPAdress, loadChat, strBrowser);
                }
                else
                {
                    MAGI_API.Models.OnlineUsers.Add(AccKey, sessionID, conId, pageUrl, myIPAdress, loadChat, strBrowser);
                    usr = MAGI_API.Models.OnlineUsers.getBySession(sessionID);
                }

                Groups.Add(conId, chatUserId);

                if (loadChat)
                    Clients.Client(conId).updateSender(usr, chatUserId + "~" + conId, myIPAdress);
            }
        }

        public void RequestForConnectByRF(int MyID, string MyName, int TargetID)
        {
            try
            {
                Clients.Group(TargetID.ToString()).confirmationForChatToTarget(MyID, MyName);
            }
            catch (Exception e)
            {
                Clients.Group(MyID.ToString()).showError(e.ToString());
            }
        }

        public void FetchRapidFirePeople(int accountKey, int eventKey, bool isDemo)
        {
            try
            {
                DataTable dt = new DataTable();
                string strQry = "sp_GetRapidFirePeople";
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paramEvent_pkey", eventKey),
                    new SqlParameter("@paramAccountKey", accountKey),
                    new SqlParameter("@IsForDemo", isDemo)
                };
                dt = SqlHelper.ExecuteTable(strQry, CommandType.StoredProcedure, sqlParameters);
                Clients.Group(accountKey.ToString()).addToRapidPeopleList(dt.AsEnumerable().ToList());
            }
            catch (Exception e)
            {
                Clients.Group(accountKey.ToString()).showError(e.ToString());
            }
        }

        public void getReceiver(string chatUserId, int ChatType, string myId, int ntfy, string topicName, string msgTxt, string eventKey)
        {
            if (ChatType == 2)
            {
                GroupOperations.Add(chatUserId, myId, eventKey);
                //var grp = GroupOperations.FirstOrDefault(chatUserId);
                //if (grp != null)
                //{
                //    bool userExists = grp.GroupUers.Exists(x => x.Equals(myId));
                //    if (!userExists)
                //    {
                //        grp.GroupUers.Add(myId);
                //    }
                //}
                //else
                //{
                //    Group newGroup = new Group()
                //    { GroupId = chatUserId, GroupUers = new List<string>() { myId } };
                //    groups.Add(newGroup);
                //}
            }

            var conId = Context.ConnectionId;
            object targetObj = null, myObj = null;
            if (ChatType == 1)
            {
                targetObj = ChatOperations.getTheName(chatUserId);
            }
            Clients.Client(conId).updateReceiver(targetObj, ChatType, chatUserId, ntfy, topicName, msgTxt);

            //if (ntfy == 1)
            //{
            //    myObj = ChatOperations.getTheName(myId);
            //    Clients.Group(chatUserId).updateReceiver(myObj, ChatType, myId, ntfy, topicName, msgTxt);
            //}
        }

        public void Send(string senderId, string receiverId, string senderName, string senderImage, string message, int ChatType, string UniqueID, string grpName = "", string nickName = "", bool isVE = false)
        {
            if (ChatType == 1)
            {
                Clients.Group(receiverId).sendAsync(senderId, senderName, senderImage, message, ChatType, UniqueID, "", nickName);
            }
            else if (ChatType == 2)
            {
                var grp = GroupOperations.FirstOrDefault(receiverId, senderId);
                if (grp != null)
                {
                    List<string> grpUsers = grp.GroupUers.ToList();
                    Clients.Groups(grpUsers).sendAsync(receiverId, senderName, senderImage, message, ChatType, UniqueID, grpName, "");
                }
            }
            else if (ChatType == 3)
            {
                Clients.Group(receiverId).sendAsync(senderId, senderName, senderImage, message, 1, UniqueID, "");
            }
        }

        /////A function only for mobile............
        public void SendFromMobile(string senderId, string receiverId, string senderName, string senderImage, string message, string UniqueID, string receiverName, int eventKey)
        {
            Clients.Group(receiverId).sendAsync(senderId, senderName, senderImage, message, 1, UniqueID, "");
            Talk talk = new Talk { ak = false, img = senderImage, name = senderName, mid = UniqueID, mine = true, msgSt = 0, strMsg = message, timeOfMsg = clsEvent.getEventVenueTime().ToString(), MsgType = 0 };
            saveChat(senderId, receiverId, 1, receiverName, talk, senderName, eventKey, true);
        }
        /////Ends here..............

        public void TypingStarts(string receiverId, string senderName, string senderId, int ChatType)
        {
            if (ChatType == 1)
                Clients.Group(receiverId).someOneIsTyping(senderName, senderId, receiverId, ChatType);
            else if (ChatType == 2)
            {
                var grp = GroupOperations.FirstOrDefault(receiverId, senderId);
                if (grp != null)
                {
                    List<string> grpUsers = grp.GroupUers.ToList();
                    Clients.Groups(grpUsers).someOneIsTyping(senderName, senderId, receiverId, ChatType);
                }
            }
            TypingStartsForVE(receiverId, senderName, senderId, ChatType);
        }

        public void TypingEnds(string receiverId, string senderId, int ChatType)
        {
            if (ChatType == 1)
                Clients.Group(receiverId).someOneIsTyped(senderId);
            else if (ChatType == 2)
            {
                var grp = GroupOperations.FirstOrDefault(receiverId, senderId);
                if (grp != null)
                {
                    List<string> grpUsers = grp.GroupUers.ToList();
                    Clients.Groups(grpUsers).someOneIsTyped(receiverId);
                }
            }
            TypingEndsForVE(receiverId, senderId, ChatType);
        }

        public void TypingStartsForVE(string receiverId, string senderName, string senderId, int ChatType)
        {
            if (ChatType == 2)
            {
                var grp = GroupOperations.FirstOrDefault(receiverId, senderId);
                if (grp != null)
                {
                    List<string> grpUsers = grp.GroupUers.ToList();
                    Clients.Groups(grpUsers).someOneIsTypingForVE(senderName, senderId, receiverId, ChatType);
                }
            }
        }

        public void TypingEndsForVE(string receiverId, string senderId, int ChatType)
        {
            if (ChatType == 1)
                Clients.Group(receiverId).someOneIsTypedForVE(senderId);
            else if (ChatType == 2)
            {
                var grp = GroupOperations.FirstOrDefault(receiverId, senderId);
                if (grp != null)
                {
                    List<string> grpUsers = grp.GroupUers.ToList();
                    Clients.Groups(grpUsers).someOneIsTypedForVE(receiverId);
                }
            }
        }



        public void ExitGroup(string groupId, string mySelf)
        {
            //var grp = GroupOperations.FirstOrDefault(groupId);
            //if (grp != null)
            //{
            deleteMyChat(mySelf, groupId);
            //}
        }

        /////////////////////////////////
        ////Acknowledgement
        public void sendAcknowledgement(string me, string to, string m_ID)
        {
            Clients.Group(to).checkRecieved(me, m_ID);
        }

        public void HideThisMessage(string myId, string msg_key,bool hide)
        {
            try
            {
                ChatOperations.HideThisChat(Convert.ToInt32(myId), msg_key, hide);
                Clients.All.hideThisMsgFromChat(msg_key, hide);
            }
            catch (Exception e)
            {
                Clients.Group(myId).showError(e.ToString());
            }
        }

        public void HideAllMessages(string myId, int Acckey, int ToPK, string Conversationdate, int eventKey, int? HiddenToAcc, int? HiddenToSecond_Acc, int typeOfPerson)
        {
            try
            {
                ChatOperations.HideTheseChatsFromPerson(myId, Acckey, ToPK, Conversationdate, eventKey, HiddenToAcc, HiddenToSecond_Acc, typeOfPerson);
            }
            catch (Exception e)
            {
                Clients.Group(myId).showError(e.ToString());
            }
        }

        public void HideAllMsgsofPerson(int myId, int Acckey, int eventKey, bool hide)
        {
            try
            {
                ChatOperations.HideAllMsgsofPerson(myId, Acckey, eventKey, hide);
            }
            catch (Exception e)
            {
                Clients.Group(myId.ToString()).showError(e.ToString());
            }
        }

        public void sendAcknowledgementForSeen(string me, string to, string m_ID)
        {
            Clients.Group(to).checkRecievedForSeen(me, m_ID);
        }
        ////////some other methods..........
        ///for personal data of a person

        public void getBio(string myId, string userId, string eventKey)
        {
            int id;
            bool success = int.TryParse(userId, out id);
            string strBio = string.Empty;
            string strHeader = string.Empty;
            string strJobTitle = string.Empty;
            string strOrg = string.Empty;
            string topImage = string.Empty;
            string strAddress = string.Empty;

            if (success)
            {
                bool nodata = false;
                SqlParameter[] sqlParameters = { new SqlParameter("@Account_pkey", id) };
                DataTable dt = SqlHelper.ExecuteTable("API_AccountBIO_Select", CommandType.StoredProcedure, sqlParameters);
                if (dt.Rows.Count > 0)
                {
                    strBio = dt.Rows[0]["PersonalBio"].ToString();

                    strHeader = dt.Rows[0]["FirstName"].ToString() + " " + dt.Rows[0]["LastName"].ToString() + " Profile";
                    strJobTitle = dt.Rows[0]["Title"].ToString();
                    strOrg = dt.Rows[0]["OrganizationID"].ToString();
                    topImage = dt.Rows[0]["topImage"].ToString();
                    strAddress = dt.Rows[0]["fullAddress"].ToString();

                    if (!string.IsNullOrEmpty(strBio))
                    {
                        string strImagePath = "~/accountimages/" + id.ToString() + "_img.jpg";
                        string strPhysicalPath = HttpContext.Current.Server.MapPath(strImagePath);
                        bool bExists = clsUtility.FileExists(strPhysicalPath);
                        if (!bExists)
                            strImagePath = topImage;

                        object conStatus = ChatOperations.GetConStatusNow(Convert.ToInt32(myId), id, Convert.ToInt32(eventKey), -1);
                        Clients.Group(myId).updateBio(strBio, strHeader, strJobTitle, strOrg, strImagePath, strAddress, conStatus);
                    }
                    else
                        nodata = true;
                }
                else
                    nodata = true;

                if (nodata)
                    Clients.Group(myId).customMsg("No bio data available for this user");
            }
        }

        public void getInfoForThisPerson(string receiverIdd)
        {
            var conId = Context.ConnectionId;
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paramId", receiverIdd)
                };

                string qry = string.Empty;
                qry = qry + Environment.NewLine + "select cast(t1.pKey as nvarchar(10)) as ID,";
                qry = qry + Environment.NewLine + "(isnull(t1.FirstName,'') +  ";
                qry = qry + Environment.NewLine + "IIF(t1.Nickname<>'' and t1.Nickname is not null,' ('+t1.Nickname+')','') +";
                qry = qry + Environment.NewLine + "IIF(t1.Lastname <>'' and t1.Lastname is not null,' '+t1.Lastname,'')) as nm,";
                qry = qry + Environment.NewLine + "(case when(t1.HasImageOnProfile = 1) then ('/accountimages/' + cast(t1.pKey as nvarchar(20)) + '_img.jpg')";
                qry = qry + Environment.NewLine + "else t1.AlternateImage end)";
                qry = qry + Environment.NewLine + "AS img,t1.Title as title,t1.PersonalBio as bio,t1.[Department] as dept,";
                qry = qry + Environment.NewLine + "isnull(o.OrganizationID,'') as org from Account_List t1 ";
                qry = qry + Environment.NewLine + "LEFT JOIN [Organization_List] o on o.pKey = t1.ParentOrganization_pKey";
                qry = qry + Environment.NewLine + "where t1.pKey = @paramId";

                DataTable dt = SqlHelper.ExecuteTable(qry, CommandType.Text, sqlParameters);
                Clients.Client(conId).addToInfoList(dt);
            }
            catch (Exception e)
            {
                Clients.Client(conId).showError(e.ToString());
            }
        }

        public void SetInSessionInAllTabs(bool InSession, string myId)
        {
            Clients.Group(myId).setInSessionByBroadcast(InSession ? "1" : "0");
        }

        public void getButtons(string myId, string userId, string EventKey, bool isDemo, bool both, int isPrtnr, int org, int activeEventKey)
        {
            int id, targetId, eventId;
            bool success = int.TryParse(myId, out id);
            success = int.TryParse(userId, out targetId);
            success = int.TryParse(EventKey, out eventId);

            if (success)
            {
                try
                {
                    ////////////////////For Stamp button
                    string IsThStamp = string.Empty;
                    int visible;
                    int intThAccountPkey = 0;

                    //bool isPartner = (isPrtnr == 1) ? true : false;

                    SqlParameter[] sqlParameters =
                    {
                        new SqlParameter("@Eventkey", eventId),
                        //new SqlParameter("@SponsorPersonPkey", (isPartner?id:targetId)),
                        //new SqlParameter("@paramAccountKey", (isPartner?targetId:id)),
                        new SqlParameter("@SponsorPersonPkey", targetId),
                        new SqlParameter("@paramAccountKey", id),
                        new SqlParameter("@IsForDemo", isDemo)
                    };

                    DataTable dt = SqlHelper.ExecuteTable("sp_BoothAtteneeForPerson", CommandType.StoredProcedure, sqlParameters);

                    if (dt.Rows.Count > 0)
                    {
                        IsThStamp = dt.Rows[0]["IsThStamp"].ToString();
                        visible = Convert.ToInt16(dt.Rows[0]["IsTh"]);
                        intThAccountPkey = Convert.ToInt32(dt.Rows[0]["Account_pkey"]);
                        bool isPartner = Convert.ToBoolean(dt.Rows[0]["isPartner"]);
                        Clients.Group(myId).createStampButton(IsThStamp, visible, intThAccountPkey, userId, isPartner);
                    }
                }
                catch (Exception e)
                {
                    Clients.Group(myId).showError(e.ToString());
                    //Clients.Group(myId).customMsg("Error in getting the treasure hunt button");
                }

                try
                {
                    ////////////////////For Connect button
                    if (both)
                    {
                        SqlParameter[] sqlParameterss =
                        {
                            new SqlParameter("@paramMyId", id),
                            new SqlParameter("@paramtargetId", targetId),
                            new SqlParameter("@paramEventKey", eventId),
                            new SqlParameter("@eventVanueTime", clsEvent.getEventVenueTime())
                        };
                        DataSet ds = SqlHelper.ExecuteSet("sp_getConnectionInfo", CommandType.StoredProcedure, sqlParameterss);

                        bool IsOnline = false;
                        if (ds.Tables.Count > 3 && ds.Tables[3].Rows.Count > 0)
                            IsOnline = Convert.ToBoolean(ds.Tables[3].Rows[0]["isOnline"]);

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            int buttonType = Convert.ToInt32(ds.Tables[0].Rows[0]["conStatus"]);
                            bool fromMe = Convert.ToBoolean(ds.Tables[0].Rows[0]["fromMe"]);
                            bool refusedByOwn = Convert.ToBoolean(ds.Tables[0].Rows[0]["refusedByOwn"]);
                            bool IsExpired = Convert.ToBoolean(ds.Tables[0].Rows[0]["IsExpired"]);
                            Clients.Group(myId).createConnectionButton(buttonType, fromMe, userId, true, refusedByOwn, IsExpired, true, IsOnline);
                        }
                        else
                        {
                            if (ds.Tables[3].Rows.Count > 0)
                            {
                                bool ReqResult = Convert.ToBoolean(ds.Tables[3].Rows[0]["ReqResult"]);
                                bool InNetworking = Convert.ToBoolean(ds.Tables[3].Rows[0]["InNetworking"]);
                                //if (ReqResult)
                                Clients.Group(myId).createConnectionButton(5, false, userId, ReqResult, false, false, InNetworking, IsOnline);
                            }
                        }

                        if (ds.Tables[1].Rows.Count > 0)
                        {
                            Clients.Group(myId).createAcceptRequest(userId, myId, EventKey, "", "2");
                        }

                        if (ds.Tables[2].Rows.Count > 0 && Convert.ToInt32(ds.Tables[2].Rows[0]["PersonalBio"]) == 1)
                        {
                            Clients.Group(myId).createBioButton();
                        }
                    }
                }
                catch (Exception e)
                {
                    Clients.Group(myId).showError(e.ToString());
                    //Clients.Group(myId).customMsg("Error in getting the connection button");
                }
            }
        }

        public void getButtonForMeet(string contextID, string myId, string userId, string EventKey, int org, int activeEventKey)
        {
            try
            {
                int id, targetId, eventId;
                bool success = int.TryParse(myId, out id);
                success = int.TryParse(userId, out targetId);
                success = int.TryParse(EventKey, out eventId);

                if (success)
                {
                    int intEventPkey = (eventId > 0 ? eventId : activeEventKey);

                    String qry = "declare @amSponser bit;declare @TargetSponser bit;declare @accKey int;declare @TargetAccKey int;";

                    qry = qry + Environment.NewLine + "set @amSponser = case when exists(Select 1 from Account_List where ";
                    qry = qry + Environment.NewLine + "ParentOrganization_pKey in (select Organization_pkey from Event_Organizations where Event_pKey=@Event_pkey)";
                    qry = qry + Environment.NewLine + "and pkey = @Account_pkey) then 1 else 0 end;";

                    qry = qry + Environment.NewLine + "set @TargetSponser = case when exists(Select 1 from Account_List where ";
                    qry = qry + Environment.NewLine + "ParentOrganization_pKey in (select Organization_pkey from Event_Organizations where Event_pKey=@Event_pkey)";
                    qry = qry + Environment.NewLine + "and pkey = @TargetAccount_pkey) then 1 else 0 end;";

                    qry = qry + Environment.NewLine + "if((@amSponser=0 AND @TargetSponser=1) OR (@amSponser=1 AND @TargetSponser=0))";
                    qry = qry + Environment.NewLine + "BEGIN";

                    qry = qry + Environment.NewLine + "SET @accKey = CASE WHEN(@amSponser = 1) THEN @Account_pkey ELSE @TargetAccount_pkey END;";
                    qry = qry + Environment.NewLine + "SET @TargetAccKey = CASE WHEN(@amSponser = 1) THEN @TargetAccount_pkey ELSE @Account_pkey END;";

                    qry = qry + Environment.NewLine + "declare @before int = (select [SettingValue] from Application_Settings s where pkey = 215);";

                    qry = qry + Environment.NewLine + "IF EXISTS(Select pkey from EventAttendee_Schedule where Account_pkey =@TargetAccKey ";
                    qry = qry + Environment.NewLine + "and SponsorAccount_pKey = @accKey ";
                    qry = qry + Environment.NewLine + "and";
                    qry = qry + Environment.NewLine + "(cast(@Date as datetime) <= Endime)";
                    qry = qry + Environment.NewLine + ")";
                    qry = qry + Environment.NewLine + "BEGIN";

                    qry = qry + Environment.NewLine + "Select t2.URL from EventAttendee_Schedule t0";
                    qry = qry + Environment.NewLine + "INNER JOIN RoundTableSchedule t1 ON t1.pkey=t0.RoundTableSchedule_pkey";
                    qry = qry + Environment.NewLine + "INNER JOIN EventUrls t2 ON t2.pkey=t1.MeetingRoomType";
                    qry = qry + Environment.NewLine + "where t0.Account_pkey = @TargetAccKey";
                    qry = qry + Environment.NewLine + "and t0.SponsorAccount_pKey=@accKey and";
                    qry = qry + Environment.NewLine + "((cast(@Date as datetime) between t0.StartTime and t0.Endime)";
                    qry = qry + Environment.NewLine + "OR";
                    qry = qry + Environment.NewLine + "(DATEADD(MINUTE,@before,cast(@Date as datetime)) between t0.StartTime and t0.Endime))";
                    qry = qry + Environment.NewLine + "END";
                    qry = qry + Environment.NewLine + "ELSE";
                    qry = qry + Environment.NewLine + "BEGIN";
                    qry = qry + Environment.NewLine + "declare @tempVar int = (select top(1) ac.[ParentOrganization_pKey] from [Account_List] ac where ac.[pKey] = @accKey);";

                    qry = qry + Environment.NewLine + "EXEC ISValidORG @tempVar, @Event_pkey ,0 ,@accKey;";

                    qry = qry + Environment.NewLine + "select 1 from Event_Accounts e where e.Account_pKey = @TargetAccKey and e.ParticipationStatus_pKey = 1 and e.Event_pKey = @Event_pkey;";

                    qry = qry + Environment.NewLine + "select eo.pkey,(isnull(acc.FirstName,'') + (case when((acc.Lastname is null) or (acc.Lastname = '')) then '' ";
                    qry = qry + Environment.NewLine + "else (' '+acc.Lastname) end)) as [rName] from dbo.account_list acc";
                    qry = qry + Environment.NewLine + "inner join dbo.Event_Organizations eo";
                    qry = qry + Environment.NewLine + "on acc.ParentOrganization_pKey = eo.organization_pkey";
                    qry = qry + Environment.NewLine + "where acc.pkey = @TargetAccKey;";
                    qry = qry + Environment.NewLine + "END";
                    qry = qry + Environment.NewLine + "END";

                    SqlParameter[] sqlParameters =
                    {
                        new SqlParameter("@OrganizationPkey", org),
                        new SqlParameter("@Event_pkey", intEventPkey),
                        new SqlParameter("@Account_pkey", id),
                        new SqlParameter("@TargetAccount_pkey", targetId),
                        new SqlParameter("@Date", clsEvent.getEventVenueTime())
                    };
                    DataSet ds = SqlHelper.ExecuteSet(qry, CommandType.Text, sqlParameters);

                    int EventOrgKey = 0;
                    string rName = string.Empty;

                    if (ds.Tables.Count == 3 && ds.Tables[2].Rows.Count > 0)
                    {
                        EventOrgKey = Convert.ToInt32(ds.Tables[2].Rows[0]["pkey"]);
                        rName = ds.Tables[2].Rows[0]["rName"].ToString();
                        if (ds.Tables[0].Rows.Count > 0 && ds.Tables[1].Rows.Count > 0)
                        {
                            Clients.Group(contextID).createMeetButton(EventOrgKey, rName);
                        }
                    }
                    else if (ds.Tables.Count == 1 && ds.Tables[0].Rows.Count > 0)
                    {
                        string urlToMove = ds.Tables[0].Rows[0]["URL"].ToString();
                        Clients.Group(contextID).createVideoButton(urlToMove);
                    }
                }
            }
            catch (Exception e)
            {
                Clients.Group(myId).showError("Error in getting Meet button :" + e.ToString());
            }
        }


        public void getVideoMeetingLink(string myId, string groupId, string EventKey, int activeEventKey)
        {
            try
            {
                int eventId;
                int.TryParse(EventKey, out eventId);
                int intEventPkey = (eventId > 0 ? eventId : activeEventKey);

                groupId = groupId.Replace("group", "");
                string qry = "declare @before int = (select [SettingValue] from Application_Settings s where pkey = 215);";
                qry = qry + Environment.NewLine + "select top(1) ur.[URL] from [SYS_ChatGroupSchedule] ch";
                qry = qry + Environment.NewLine + "INNER JOIN RoundTableSchedule r on ch.RoundTableKey = r.pKey";
                qry = qry + Environment.NewLine + "INNER JOIN EventUrls ur on ur.pkey = r.MeetingRoomType";
                qry = qry + Environment.NewLine + "where cast(ch.GroupId as nvarchar(30)) = cast(@paramGroupID as nvarchar(30)) and ch.IsTextType = 0 and ch.ScheduleType <> 1 and ((cast(@paramDate as datetime) between ch.startDate and ch.EndDate) OR (DATEADD(MINUTE,@before,cast(@paramDate as datetime)) between ch.startDate and ch.EndDate)) and ch.EventKey = @Event_pkey;";

                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paramDate", clsEvent.getEventVenueTime()),
                    new SqlParameter("@paramGroupID", groupId),
                    new SqlParameter("@Event_pkey", intEventPkey)
                };
                DataTable dt = SqlHelper.ExecuteTable(qry, CommandType.Text, sqlParameters);

                if (dt.Rows.Count > 0)
                {
                    string urlToMove = dt.Rows[0]["URL"].ToString();
                    Clients.Group(myId).createVideoButton(urlToMove);
                }
            }
            catch (Exception ex)
            {
                Clients.Group(myId).showError(ex.ToString());
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
                if(dt.Rows.Count > 0)
                    Clients.Client(conId).takeToMeeting(dt.Rows[0]["URL"].ToString());
                else
                    Clients.Client(conId).customMsg("Sorry, meeting room has filled up with 20 people");
            }
            catch (Exception ex)
            {
                Clients.Group(myID).showError(ex.ToString());
            }
        }

        public void madeRequest(string source, string target, string eventKey, string senderName, string EventOrganizationkey)
        {
            try
            {
                int id, targetId, eventId;
                bool success = int.TryParse(source, out id);
                success = int.TryParse(target, out targetId);
                success = int.TryParse(eventKey, out eventId);

                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paramSource", id),
                    new SqlParameter("@paramTarget", targetId),
                    new SqlParameter("@paramEventKey", eventId),
                    new SqlParameter("@paramEventOrganizationkey", EventOrganizationkey)
                };

                DataSet ds = new DataSet();
                ds = SqlHelper.ExecuteSet("SP_insertConnection", CommandType.StoredProcedure, sqlParameters);
                bool isonline = false;
                if (ds.Tables.Count == 1 && ds.Tables[0].Rows.Count > 0)
                {
                    isonline = Convert.ToBoolean(ds.Tables[0].Rows[0]["isOnline"]);
                }
                else if (ds.Tables.Count == 2 && ds.Tables[1].Rows.Count > 0)
                {
                    isonline = Convert.ToBoolean(ds.Tables[1].Rows[0]["isOnline"]);
                }

                Clients.Group(source).createConnectionButton(0, true, target, true, false, false, true, isonline);
                Clients.Group(target).OpenChatOverThereClientWithoutMsg(source, senderName);
                Clients.Group(target).createAcceptRequest(id, targetId, eventId, senderName, "1");

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0 && Convert.ToBoolean(ds.Tables[0].Rows[0]["bchatAutoReply"]))  ////To Check that is this request first time in this event or not   and to get chat msg
                {
                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0) ////To Check that is this target person sponsor or not
                    {
                        string msg = ds.Tables[0].Rows[0]["chatAutoReply"].ToString();
                        string targetName = ds.Tables[1].Rows[0]["targetName"].ToString();
                        string senderImage = ds.Tables[1].Rows[0]["senderImage"].ToString();
                        string nickName = ds.Tables[1].Rows[0]["nickName"].ToString();
                        senderImage = (!string.IsNullOrEmpty(senderImage) ? senderImage.Replace("~", "") : senderImage);

                        if (!string.IsNullOrEmpty(msg))
                        {
                            ///////Need to call another function here as this hub class is state less......
                            //Thread.Sleep(60000);
                            var conId = Context.ConnectionId;
                            Clients.Client(conId).replaceSentinels(msg, targetName, senderImage, nickName);
                        }
                    }
                }
                //clsReminders cReminders = new clsReminders();
                //cReminders.UserReminderStatusUpdate(eventId, targetId, clsReminders.R_TryRespondConnection);
                SqlOperation sqlOperation = new SqlOperation();
                sqlOperation.UserReminderStatusUpdate(eventId, targetId, clsReminders.R_TryRespondConnection);
            }
            catch (Exception e)
            {
                Clients.Group(source).showError(e.ToString());
                //Clients.Group(source).customMsg("An error occurred!");
            }
        }

        public void getMsgFromTarget(string source, string target, string targetName, string senderImage,
                                    string msg, string nickName, string senderName, int eventId)
        {
            try
            {
                string UniqueID = Guid.NewGuid().ToString();
                Clients.Group(source).sendAsync(target, targetName, senderImage, msg, 1, UniqueID, "", nickName);

                Talk talk = new Talk()
                {
                    ak = false,
                    img = senderImage,
                    name = targetName,
                    mid = UniqueID,
                    mine = true,
                    msgSt = 0,
                    strMsg = msg,
                    timeOfMsg = clsEvent.getEventVenueTime().ToString(),
                    MsgType = 0
                };
                saveChat(target, source, 1, senderName, talk, targetName, eventId);
            }
            catch (Exception e)
            {
                Clients.Group(source).showError(e.ToString());
                //Clients.Group(source).customMsg("An error occurred!");
            }
        }

        public void acceptRequest(string source, string target, string eventKey, string myNameOnAcc)
        {
            try
            {
                string qry = "update EventAccount_Connections";
                qry = qry + Environment.NewLine + "set [ConnectionStatus_pKey] = 1";
                qry = qry + Environment.NewLine + ",[ConnectionUpdated] = GETDATE()";
                qry = qry + Environment.NewLine + "where [Account_pkey] = @paramSource";
                qry = qry + Environment.NewLine + "and [ConnectionAccount_pkey] = @paramTarget";
                qry = qry + Environment.NewLine + "and [Event_pkey] = @paramEventKey";

                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paramSource", source),
                    new SqlParameter("@paramTarget", target),
                    new SqlParameter("@paramEventKey", eventKey)
                };

                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, sqlParameters);

                Clients.Group(source).requestAccepted(myNameOnAcc, target);
                Clients.Group(source).OpenChatOverThereClientWithoutMsg(target, myNameOnAcc);
                Clients.Group(source).ChangeButtonTo("#MainHeaderOfChat #dvForConnect input", "/Images/Icons/Connection_Accept.png", true, "Connected");
                Clients.Group(target).removeAcceptButton();
            }
            catch (Exception ex)
            {
                Clients.Group(target).showError(ex.ToString());
                //Clients.Group(target).customMsg("An error occurred!");
            }
        }

        public void rejectRequest(string source, string target, string eventKey, string nameOfSource)
        {
            try
            {
                string qry = "update EventAccount_Connections";
                qry = qry + Environment.NewLine + "set [ConnectionStatus_pKey] = 2";
                qry = qry + Environment.NewLine + ",[ConnectionUpdated] = GETDATE()";
                qry = qry + Environment.NewLine + "where [Account_pkey] = @paramSource";
                qry = qry + Environment.NewLine + "and [ConnectionAccount_pkey] = @paramTarget";
                qry = qry + Environment.NewLine + "and [Event_pkey] = @paramEventKey";

                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paramSource", source),
                    new SqlParameter("@paramTarget", target),
                    new SqlParameter("@paramEventKey", eventKey)
                };

                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, sqlParameters);
                //Clients.Group(source).requestRejected();
                //Clients.Group(source).OpenChatOverThereClientWithoutMsg(target, nameOfSource);
                Clients.Group(source).ChangeButtonTo("#MainHeaderOfChat #dvForConnect input", "/Images/Icons/Connection_Pending.png", true, "Connection Requested by Me");
                Clients.Group(target).removeAcceptButton();
            }
            catch (Exception ex)
            {
                Clients.Group(target).showError(ex.ToString());
                //Clients.Group(target).customMsg("An error occurred!");
            }
        }

        public void stampClick(int IsThStamp, string myId, string userId, string evkey, string intThAccountPkey, string myName)
        {
            try
            {
                if (intThAccountPkey == myId)
                {
                    SqlParameter[] sqlParameters =
                    {
                        new SqlParameter("@LoggedBy", userId),
                        new SqlParameter("@Account_pkey", myId),
                        new SqlParameter("@eventpKey", evkey),
                        new SqlParameter("@ThStatus", 3)
                    };
                    SqlHelper.ExecuteNonQuery("sp_createStamp", CommandType.StoredProcedure, sqlParameters);

                    Clients.Group(myId).getbuttonsAgain();
                    Clients.Group(userId).getbuttonsAgain();
                    Clients.Group(myId).customMsg("Treasure Hunt card Requested.");
                    Clients.Group(userId).OpenChatOverThereClientWithoutMsg(myId, myName);
                    Clients.Group(userId).customMsg("Treasure Hunt stamp requested by " + myName);
                }
                else
                {
                    Clients.Group(myId).treasureHuntWindow(userId);
                }
            }
            catch (Exception e)
            {
                Clients.Group(myId).showError(e.ToString());
                //Clients.Group(myId).customMsg("An error occurred!");
            }
        }

        public void changedTopic(string topicName, string changer, string listener, string changerName, object objList)
        {
            List<string> LUsers = new List<string>();
            LUsers.Add(changer);
            LUsers.Add(listener);
            Clients.Groups(LUsers).instantChatTopic(topicName, changerName, objList, changer, 1);
        }

        public void saveChat(string myID, string ID, int ChtType, string nm, Talk talk, string myName, int eventKey, bool fromMobile = false)
        {
            try
            {
                string correctName = ChatOperations.add(myID, ID, ChtType, nm, talk, myName, eventKey, fromMobile);
                if (!string.IsNullOrEmpty(correctName))
                {
                    Clients.Group(myID).updateTheCorrectName(ID, correctName);
                }
            }
            catch (Exception e)
            {
                Clients.Group(myID).showError(e.ToString());
            }
        }

        public void updateMessageStatus(string myID, string msg_ID, int status, string targetID)
        {
            try
            {
                ChatOperations.UpdateMsgStatus(myID, msg_ID, status, targetID);
            }
            catch (Exception e)
            {
                Clients.Group(myID).showError(e.ToString());
            }
        }

        public void getChat(string myID, string diff, string eventKey, string othersChat)
        {
            try
            {
                var conId = Context.ConnectionId;
                Chats chats = ChatOperations.getChatHistory((string.IsNullOrEmpty(othersChat) ? myID : othersChat), diff, eventKey);
                Clients.Client(conId).loadHistory(chats);

                if (string.IsNullOrEmpty(othersChat))
                {
                    string[] Result = ChatOperations.CheckForAnyBroadCast(Convert.ToInt32(myID), Convert.ToInt32(eventKey));
                    if (Result != null && Result.Length > 0)
                    {
                        string result = Result[0];
                        if (!string.IsNullOrEmpty(result))
                        {
                            string[] strArr = result.Split('^');
                            string strMsg = strArr[0];
                            int intKey = Convert.ToInt32(strArr[1]);
                            Clients.Group(myID).adminBroadcast(strMsg, intKey);
                        }

                        //result = Result[1];
                        //if (!string.IsNullOrEmpty(result))
                        //{
                        //    Clients.Group(myID).adminBroadcast(result, -1);
                        //}

                        //if (Result.Length > 1)
                        //{
                        //    string sessionBroadcast = Result[1];
                        //    if (!string.IsNullOrEmpty(sessionBroadcast))
                        //    {
                        //        string[] strArr = sessionBroadcast.Split('^');
                        //        if (strArr.Length > 0)
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
                //else
                //{
                //object o = ChatOperations.getChatContactList(Convert.ToInt32(othersChat), Convert.ToInt32(eventKey));
                //Clients.Group(myID).updateSponsorChat(o);
                //}

                //HR HaveAnyInterestGroup(Convert.ToInt32(myID), Convert.ToInt32(eventKey));
            }
            catch (Exception e)
            {
                Clients.Group(myID).showError(e.ToString());
            }
        }

        public void getConnectionIcon(int myID, int targetID, int eventKey)
        {
            try
            {
                object conStatus = ChatOperations.GetConStatusNow(myID, targetID, eventKey, -1);
                Clients.Group(myID.ToString()).updateConnectionIcon(targetID, ((dynamic)conStatus).connStatus, ((dynamic)conStatus).isOnline);
            }
            catch (Exception e)
            {
                Clients.Group(myID.ToString()).showError(e.ToString());
            }
        }

        public void getEventSponsorStatus(int targetID, int eventKey)
        {
            var conId = Context.ConnectionId;
            try
            {
                DataTable dt = new DataTable();
                string qry = string.Empty;
                qry = "Select 1 from Account_List ac where ";
                qry = qry + Environment.NewLine + "ac.pKey = @accKey and";
                qry = qry + Environment.NewLine + "exists(select 1 from Event_Organizations eo where eo.Event_pKey=@eventKey ";
                qry = qry + Environment.NewLine + "and eo.Organization_pkey=ac.ParentOrganization_pKey);";

                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@accKey", targetID),
                    new SqlParameter("@eventKey", eventKey)
                };
                dt = SqlHelper.ExecuteTable(qry, CommandType.Text, sqlParameters);

                if(dt.Rows.Count > 0)
                {
                    Clients.Client(conId).updateEventAttendeeStatus(targetID,"1");
                }
                else
                {
                    Clients.Client(conId).updateEventAttendeeStatus(targetID,"0");
                }
            }
            catch (Exception e)
            {
                Clients.Client(conId).showError(e.ToString());
            }
        }

        public void getConnectionIconForVE(int myID, int targetID, int eventKey, int sessionID)
        {
            object conStatus = ChatOperations.GetConStatusNow(myID, targetID, eventKey, sessionID);
            Clients.Group(myID.ToString()).updateConnectionIconForVE(targetID, conStatus);
        }

        public void deleteMyChat(string mySelf, string receiverId)
        {
            try
            {
                ChatOperations.deleteMyChat(mySelf, receiverId);
            }
            catch (Exception e)
            {
                Clients.Group(mySelf).showError(e.ToString());
            }
        }

        public void attendMeeting(dynamic d,int myID, int hisId)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@MeetingId", d.MeetingId.ToString()),
                    new SqlParameter("@Password", d.Password.ToString()),
                    new SqlParameter("@myID", myID),
                    new SqlParameter("@hisId", hisId)
                };
                string qry = "declare @currTime datetime;set @currTime = GETDATE();";
                qry = qry + Environment.NewLine + "insert into [VideoChat]";
                qry = qry + Environment.NewLine + "(MyID,Chat_ID,MeetingType,Meeting_Id,[Password],StartTime,EndTime,Recurring)";
                qry = qry + Environment.NewLine + "values(@myID,@hisId,2,@MeetingId,@Password,@currTime,null,0);";

                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, sqlParameters);
            }
            catch (Exception e)
            {
                Clients.Group(myID.ToString()).showError(e.ToString());
            }
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
                    //Clients.Group(myID.ToString()).giveChanceToSpin(NetLevel);
                    //Clients.Client(conId).giveChanceToSpin(NetLevel, strMsg);
                //}
            }
            catch (Exception e)
            {
                Clients.Group(myID.ToString()).showError(e.ToString());
            }
        }

        private string[] GetStaffNotMe(string myID)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paraAccKey", myID)
                };
                string qry = "select cast(ac.pKey as nvarchar(10)) as pKey from [Account_List] ac where (isnull(ac.StaffMember,0) = 1 or isnull(ac.GlobalAdministrator,0) = 1) and ac.pKey <> @paraAccKey;";
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

        private string[] GetAttendeeNotMe(string myID, int EventKey)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paraAccKey", myID),
                    new SqlParameter("@paraEventKey", EventKey)
                };
                string qry = string.Empty;
                qry = qry + Environment.NewLine + "select cast(ea.Account_pKey as nvarchar(10)) as pKey from";
                qry = qry + Environment.NewLine + "Event_Accounts ea";
                qry = qry + Environment.NewLine + "inner join Account_List al";
                qry = qry + Environment.NewLine + "on al.pkey = ea.Account_pKey";
                qry = qry + Environment.NewLine + "where ea.ParticipationStatus_pKey=1";
                qry = qry + Environment.NewLine + "and ea.Event_pKey = @paraEventKey";
                qry = qry + Environment.NewLine + "and ea.Account_pKey <> @paraAccKey";
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

        private string[] GetAttendeeAndStaffNotMe(string myID, int EventKey)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paraAccKey", myID),
                    new SqlParameter("@paraEventKey", EventKey)
                };
                string qry = string.Empty;
                qry = qry + Environment.NewLine + "select cast(ac.pKey as nvarchar(10)) as pKey from [Account_List] ac";
                qry = qry + Environment.NewLine + "where (isnull(ac.StaffMember,0) = 1 or isnull(ac.GlobalAdministrator,0) = 1)";
                qry = qry + Environment.NewLine + "and ac.pKey <> @paraAccKey";
                qry = qry + Environment.NewLine + "union";
                qry = qry + Environment.NewLine + "select cast(ea.Account_pKey as nvarchar(10)) as pKey from ";
                qry = qry + Environment.NewLine + "Event_Accounts ea";
                qry = qry + Environment.NewLine + "inner join Account_List al";
                qry = qry + Environment.NewLine + "on al.pkey = ea.Account_pKey";
                qry = qry + Environment.NewLine + "where ea.ParticipationStatus_pKey=1";
                qry = qry + Environment.NewLine + "and ea.Event_pKey = @paraEventKey ";
                qry = qry + Environment.NewLine + "and ea.Account_pKey <> @paraAccKey";
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

        public void broadcastThisText(string myID, string strMsg, bool forStaffonly, bool ForAttendee, int EventKey)
        {
            try
            {
                int id = BroadcastInsert(myID, strMsg, 1, forStaffonly, ForAttendee, EventKey);
                if (id != 0)
                {
                    if (ForAttendee && forStaffonly)
                    {
                        var NotMe = GetAttendeeAndStaffNotMe(myID, EventKey);
                        Clients.Groups(NotMe).adminBroadcast(strMsg, id);
                        var _hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<LocalHub>();
                        _hubContext.Clients.Groups(NotMe).adminBroadcast(strMsg, id);
                    }
                    else if (forStaffonly)
                    {
                        var StaffNotMe = GetStaffNotMe(myID);
                        Clients.Groups(StaffNotMe).adminBroadcast(strMsg, id);
                        var _hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<LocalHub>();
                        _hubContext.Clients.Groups(StaffNotMe).adminBroadcast(strMsg, id);
                    }
                    else if (ForAttendee)
                    {
                        var AttendeeNotMe = GetAttendeeNotMe(myID, EventKey);
                        Clients.Groups(AttendeeNotMe).adminBroadcast(strMsg, id);
                        var _hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<LocalHub>();
                        _hubContext.Clients.Groups(AttendeeNotMe).adminBroadcast(strMsg, id);
                    }
                    

                    //string strConfirmationMsg = string.Empty;
                    //if (forStaffonly && ForAttendee)
                    //    strConfirmationMsg = "Message broadcast for staff and attendee";
                    //else if (forStaffonly)
                    //    strConfirmationMsg = "Message broadcast for staff only";
                    //else if (ForAttendee)
                    //    strConfirmationMsg = "Message broadcast for attendees only";

                    //if(!string.IsNullOrEmpty(strConfirmationMsg))
                    //    Clients.Client(conId).customMsg(strConfirmationMsg);

                    //else
                    //{
                    //    string[] arStr = { conId };
                    //    Clients.AllExcept(arStr).adminBroadcast(strMsg, id);
                    //    var _hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<LocalHub>();
                    //    _hubContext.Clients.AllExcept(arStr).adminBroadcast(strMsg, id);
                    //    Clients.Client(conId).customMsg("Message broadcasted");
                    //}
                }
            }
            catch (Exception e)
            {
                Clients.Group(myID.ToString()).customMsg("Error in broadcasting the message");
                Clients.Group(myID.ToString()).showError(e.ToString());
            }
        }
        
        public void broadCastThisChatMsg(string myID, string strMsg, string senderName, string senderImage, string nickName, bool forStaffonly, bool ForAttendee, int eventKey)
        {
            try
            {
                int id = BroadcastInsert(myID, strMsg, 2, forStaffonly, ForAttendee, eventKey);
                if(id != 0)
                {
                    var conId = Context.ConnectionId;

                    if (ForAttendee && forStaffonly)
                    {
                        var NotMe = GetAttendeeAndStaffNotMe(myID, eventKey);
                        Clients.Groups(NotMe).sendAsync(myID, senderName, senderImage, strMsg, 1, id, "", nickName);
                        var _hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<LocalHub>();
                        _hubContext.Clients.Groups(NotMe).sendAsync(myID, senderName, senderImage, strMsg, 1, id, "", nickName);
                    }
                    else if (forStaffonly)
                    {
                        var StaffNotMe = GetStaffNotMe(myID);
                        Clients.Groups(StaffNotMe).sendAsync(myID, senderName, senderImage, strMsg, 1, id, "", nickName);
                        var _hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<LocalHub>();
                        _hubContext.Clients.Groups(StaffNotMe).sendAsync(myID, senderName, senderImage, strMsg, 1, id, "", nickName);
                    }
                    if (ForAttendee)
                    {
                        var AttendeeNotMe = GetAttendeeNotMe(myID, eventKey);
                        Clients.Groups(AttendeeNotMe).sendAsync(myID, senderName, senderImage, strMsg, 1, id, "", nickName);
                        var _hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<LocalHub>();
                        _hubContext.Clients.Groups(AttendeeNotMe).sendAsync(myID, senderName, senderImage, strMsg, 1, id, "", nickName);
                    }

                    //string strConfirmationMsg = string.Empty;
                    //if (forStaffonly && ForAttendee)
                    //    strConfirmationMsg = "Message broadcast for staff and attendee";
                    //else if (forStaffonly)
                    //    strConfirmationMsg = "Message broadcast for staff only";
                    //else if (ForAttendee)
                    //    strConfirmationMsg = "Message broadcast for attendees only";

                    //if (!string.IsNullOrEmpty(strConfirmationMsg))
                    //    Clients.Client(conId).customMsg(strConfirmationMsg);

                    //else
                    //{
                    //    string[] arStr = { conId };
                    //    Clients.AllExcept(arStr).
                    //    sendAsync(myID, senderName, senderImage, strMsg, 1, id, "", nickName);
                    //    Clients.Client(conId).customMsg("Message broadcast");
                    //}
                }
            }
            catch (Exception e)
            {
                Clients.Group(myID.ToString()).customMsg("Error in broadcasting the message");
                Clients.Group(myID.ToString()).showError(e.ToString());
            }
        }

        public void broadcastThisWelcomeText(string myID,string welcomeText, bool forStaffonly)
        {
            try
            {
                //int id = BroadcastInsert(myID, welcomeText, 3, forStaffonly);
                //if (id != 0)
                //{
                //    var conId = Context.ConnectionId;
                //    string[] arStr = { conId };

                //    if (forStaffonly)
                //        Clients.Client(conId).customMsg("Message broadcast for staff only");
                //    else
                //        Clients.Client(conId).customMsg("Message broadcast");
                //}
            }
            catch (Exception e)
            {
                Clients.Group(myID).showError(e.ToString());
            }
        }

        private int BroadcastInsert(string myID, string strMsg, int type, bool forStaffonly,bool ForAttendee, int eventKey)
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
                    new SqlParameter("@paramforStaffonly", forStaffonly),
                    new SqlParameter("@ForAttendee", ForAttendee),
                    new SqlParameter("@eventKey", eventKey)
                };
                string qry = "insert into dbo.[BroadCastMessage]";
                qry = qry + Environment.NewLine + "([accountKey],[Message],[DateTime],[BroadcastType],[ForStaffOnly],[ForAttendee],EventKey)";
                qry = qry + Environment.NewLine + "values(@paraAccKey,@paraMsg,@paramDateTime,@paraBroadcastType,@paramforStaffonly,@ForAttendee,@eventKey);";
                qry = qry + Environment.NewLine + "SELECT isnull(SCOPE_IDENTITY(),0) AS [SCOPE_IDENTITY];";
                DataTable dt = new DataTable();
                dt = SqlHelper.ExecuteTable(qry, CommandType.Text, sqlParameters);

                if (dt.Rows.Count > 0)
                {
                    return Convert.ToInt32(dt.Rows[0]["SCOPE_IDENTITY"]);
                }
                return 0;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public void getFBFriendsList(string myID, string eventKey, bool isDemo)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@intAccountKey", myID),
                    new SqlParameter("@intEventKey", eventKey),
                    new SqlParameter("@isForDemo", isDemo)
                };
                DataTable dt = new DataTable();
                dt = SqlHelper.ExecuteTable("sp_getRandomPeopleAccToAnswers", CommandType.StoredProcedure, sqlParameters);
                
                if (dt.Rows.Count > 0)
                {
                    //for (int i = 0; dt.Rows.Count > i; i++)
                    //{
                    //    dt.Rows[i]["image"] = OnlineUsers.RefreshAccountImage(Convert.ToInt32(dt.Rows[i]["id"]), dt.Rows[i]["image"].ToString());
                    //}
                    //dt.AcceptChanges();

                    List<object> dList = new List<object>();
                    foreach(DataRow dr in dt.Rows)
                    {
                        object d = new
                        {
                            id = Convert.ToInt32(dr["id"]),
                            image = dr["image"].ToString(),
                            name = dr["name"].ToString(),
                            title = dr["title"].ToString(),
                            bio = dr["bio"].ToString(),
                            dept = dr["dept"].ToString(),
                            org = dr["org"].ToString(),
                            isOnline = dr["IsOnline"].ToString()
                        };
                        dList.Add(d);
                    }
                    Clients.Group(myID.ToString()).showOrHideFBList(dList);
                }
            }
            catch (Exception e)
            {
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

        public void getChatForThisPerson(string calledFor, string myID, string diff, string eventKey, string targetID, bool isGroup, bool needToLeave)
        {
            try
            {
                if (isGroup)
                {
                    if(WasRemovedByAdmin(calledFor, eventKey, targetID))
                    {
                        var conId = Context.ConnectionId;
                        Clients.Client(conId).dontJoinTheGroup(targetID);  /// unselect and inform
                        return;
                    }
                    GroupOperations.Add(targetID, calledFor, eventKey, needToLeave);
                }

                Chats chats = ChatOperations.getChatHistoryByPerson(calledFor, diff, eventKey, targetID);
                Clients.Group(myID).loadHistoryForThisPerson(chats);
            }
            catch (Exception e)
            {
                Clients.Group(myID).showError(e.ToString());
            }
        }

        private bool WasRemovedByAdmin(string calledFor, string eventKey, string targetID)
        {
            string strQry = string.Empty;
            strQry = strQry + Environment.NewLine + "select 1 from sys_chats c";
            strQry = strQry + Environment.NewLine + "where ISNULL(c.isMineNow,0) = 0";
            strQry = strQry + Environment.NewLine + "and c.myID = @calledFor";
            strQry = strQry + Environment.NewLine + "and c.Event_pkey = @eventKey";
            strQry = strQry + Environment.NewLine + "and c.ID = @targetID";

            SqlParameter[] sqlParameters =
            {
                new SqlParameter("@calledFor", calledFor),
                new SqlParameter("@eventKey", eventKey),
                new SqlParameter("@targetID", targetID)
            };

            DataTable dt = new DataTable();
            dt = SqlHelper.ExecuteTable(strQry, CommandType.Text, sqlParameters);
            if (dt.Rows.Count > 0)
            {
                return true;
            }

            return false;
        }

        public void sendForCheckingGreetingText(string allIds, int eventkey, int myID)
        {
            var ConId = Context.ConnectionId;
            try
            {
                DataTable dt = new DataTable();
                string strQry = string.Empty;

                strQry = strQry + Environment.NewLine + "declare @adminPkey int = (select TOP(1) EventChairman_pkey from Event_list where pkey = @ParamEventKey and EventChairman_pkey in ("+ allIds + "))";
                strQry = strQry + Environment.NewLine + "DECLARE @allowThisBroadCast BIT;";
                strQry = strQry + Environment.NewLine + "SET @allowThisBroadCast = isnull((select showWelcomeTextOnLogin from [Event_List] where [pkey] = @ParamEventKey),0);  ";

                strQry = strQry + Environment.NewLine + "if(@allowThisBroadCast = 1 and @adminPkey is not null and ";
                strQry = strQry + Environment.NewLine + "not exists(select 1 from AcknowledgeBroadCast a where a.accountKey = @myID and a.BroadCast_pKey = -1))";
                strQry = strQry + Environment.NewLine + "begin";
                strQry = strQry + Environment.NewLine + "select @adminPkey as adminPkey;";
                strQry = strQry + Environment.NewLine + "insert into AcknowledgeBroadCast(accountKey,BroadCast_pKey,AcknowledgeDateTime) values(@myID,-1,getdate())";
                strQry = strQry + Environment.NewLine + "end";

                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@myID", myID),
                    new SqlParameter("@ParamEventKey", eventkey)
                };
                dt = SqlHelper.ExecuteTable(strQry, CommandType.Text, sqlParameters);

                if(dt.Rows.Count > 0)
                {
                    Clients.Client(ConId).OpenChatOverThereClientWithoutMsg(dt.Rows[0]["adminPkey"].ToString(),"");
                }
            }
            catch (Exception e)
            {
                Clients.Client(ConId).showError(e.ToString());
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
                    Clients.Client(conId).customMsg("Session broadcast");

                    var _hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<LocalHub>();
                    _hubContext.Clients.All.openThisSessionForAll(sessionID);
                }
            }
            catch (Exception e)
            {
                Clients.Group(myID.ToString()).customMsg("Error in sending this broadcast.");
                Clients.Group(myID.ToString()).showError(e.ToString());
            }
        }

        public void Testconnectivity(int myID,string testMessage)
        {
            Clients.Group(myID.ToString()).showErrorVE(testMessage);
        }

        public void SynchronizeAllSentMsgs(string clsAvatarSndr, string date, string who, string text,int ChatType,string UniqueID, object control,string currentRecieverID, string myId)
        {
            Clients.Group(myId).synchronizeSentMsgs(clsAvatarSndr, date, who, text, ChatType, UniqueID, control, currentRecieverID);
        }

        public void CheckForMingle(int myID, int eventId)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@accPkey", myID),
                    new SqlParameter("@eventKey", eventId),
                    new SqlParameter("@date", clsEvent.getEventVenueTime())
                };
                DataSet ds = new DataSet();
                ds = SqlHelper.ExecuteSet("exec sp_CheckMingleTime @accPkey,@eventKey,@date;", CommandType.Text, sqlParameters);

                if (ds.Tables.Count > 2)
                {
                    if (ds.Tables[2].Rows.Count > 0)
                    {
                        if (ds.Tables[1].Rows.Count > 0)
                            showMingleList(myID,eventId);
                        else
                        {
                            Clients.Group(myID.ToString()).ctlPanelMingleConfirmation("Please wait for the next person to join you.");
                        }
                        return;
                    }

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        string found = ds.Tables[0].Rows[0]["found"].ToString();
                        string mingleTime = ds.Tables[0].Rows[0]["MingleTime"].ToString();
                        bool allowMingle = false, showNextTimeWindow = false;

                        string windowMessage = string.Empty;

                        if (found == "1" && mingleTime == "1")
                        {
                            allowMingle = true;
                            SqlParameter[] parameters =
                            {
                                new SqlParameter("@accPkey", myID),
                                new SqlParameter("@eventKey", eventId),
                                new SqlParameter("@date", clsEvent.getEventVenueTime())
                            };
                            SqlHelper.ExecuteNonQuery("exec SP_InsertMinglePeople @accPkey,@date,@eventKey;", CommandType.Text, parameters);

                            if (ds.Tables[1].Rows.Count > 0)
                                showMingleList(myID, eventId);
                            else
                            {
                                Clients.Group(myID.ToString()).ctlPanelMingleConfirmation("Please wait for the next person to join you.");
                            }
                            //Clients.Group(myID.ToString()).ctlPanelMingleConfirmation("Please wait for the next person to join you.");
                        }
                        else if (found == "1" && mingleTime != "1")
                        {
                            windowMessage = "The next Mingle event will be at " + mingleTime + ".";
                            showNextTimeWindow = true;
                        }
                        else
                        {
                            string str = ds.Tables[0].Rows[0]["nextDate"].ToString();
                            windowMessage = "The next Mingle event will be at 10:30 AM - 11:00 AM  EDT on " + str + ".";
                            showNextTimeWindow = true;
                        }

                        if (!allowMingle && showNextTimeWindow)
                        {
                            Clients.Group(myID.ToString()).ctlPanelMingleConfirmation(windowMessage);
                            SqlParameter[] parameters =
                            {
                                new SqlParameter("@accPkey", myID),
                                new SqlParameter("@paramEventKey", eventId),
                                new SqlParameter("@date", clsEvent.getEventVenueTime())
                            };
                            SqlHelper.ExecuteNonQuery("exec SP_InsertLaterMingleRequest @accPkey,@paramEventKey,@date;", CommandType.Text, parameters);
                        }
                    }
                }
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

        public void CrisisAlert(string strTitle, string strBody, int account_ID, string crisisTh, string didToWhom, int eventKey, bool replied, string strReplied, bool sendToAll)
        {
            try
            {
                DataSet peopleWithMe = ChatOperations.GetCrisisGroupMembers(account_ID);

                List<string> CrisisMembers = peopleWithMe.Tables[0].AsEnumerable()
                .Select(row => row.Field<string>(0)
                ).ToList();

                if(peopleWithMe.Tables[1].Rows.Count > 0)
                {
                    string signature = "<b>From:" + Environment.NewLine +
                        peopleWithMe.Tables[1].Rows[0]["fullName"].ToString() + Environment.NewLine +
                        (
                            !string.IsNullOrEmpty(peopleWithMe.Tables[1].Rows[0]["MobileNumber"].ToString()) ?
                            (peopleWithMe.Tables[1].Rows[0]["MobileNumber"].ToString() + Environment.NewLine) : ""
                        ) +
                        peopleWithMe.Tables[1].Rows[0]["Email"].ToString()+ "</b>" + Environment.NewLine;

                    string FullBody = string.Empty;
                    if(!replied)
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
                        if(sendToAll)
                        {
                            if(!string.IsNullOrEmpty(didToWhom))
                                CrisisMembers.Add(didToWhom);

                            Clients.Groups(CrisisMembers).crisisAlertOnClient(strTitle, FullBody, account_ID, crisisTh, id, replied);
                            var _hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<LocalHub>();
                            _hubContext.Clients.Groups(CrisisMembers).crisisAlertOnClient(strTitle, FullBody, account_ID, crisisTh, id, replied);   
                        }
                        else
                        {
                            Clients.Group(didToWhom).crisisAlertOnClient(strTitle, FullBody, account_ID, crisisTh, id, replied);
                            var _hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<LocalHub>();
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

        public void CheckForRapidPeople(int myID, int eventId, bool isDemo)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paramEvent_pkey", eventId),
                    new SqlParameter("@paramAccountKey", myID),
                    new SqlParameter("@IsForDemo", isDemo),
                    new SqlParameter("@currentDate", clsEvent.getEventVenueTime())
                };

                DataTable dt = new DataTable();
                dt = SqlHelper.ExecuteTable("exec sp_GetRapidFirePeopleWithConSts @paramEvent_pkey, @paramAccountKey, @IsForDemo, @currentDate", CommandType.Text, sqlParameters);

                if(dt.Rows.Count > 0)
                {
                    var conId = Context.ConnectionId;
                    Clients.Client(conId).showRapidPeople(dt.AsEnumerable().ToList());
                }
                else
                {
                    Clients.Group(myID.ToString()).ctlPanelMingleConfirmation("Currently, no person available for this feature.");
                }
            }
            catch (Exception e)
            {
                Clients.Group(myID.ToString()).showError(e.ToString());
            }
        }

        private void showMingleList(int myID, int eventId)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@accPkey", myID),
                    new SqlParameter("@eventKey", eventId),
                    new SqlParameter("@date", clsEvent.getEventVenueTime())
                };
                DataTable dt = new DataTable();
                dt = SqlHelper.ExecuteTable("SP_getMinglePeople", CommandType.StoredProcedure, sqlParameters);
                Clients.Group(myID.ToString()).bindMinglers(dt.AsEnumerable().ToList());
            }
            catch (Exception e)
            {
                Clients.Group(myID.ToString()).showError(e.ToString());
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
    }
}
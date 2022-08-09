using fcConferenceManager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace MAGI_API.Models
{
    public class Chat
    {
        public Chat()
        {
            talks = new List<Talk>();
        }

        public string ID { get; set; }
        public int ChtType { get; set; }
        public string img { get; set; }
        public string nm { get; set; }
        public string nick { get; set; }

        public string JobTitle { get; set; }
        public string bio { get; set; }
        public string dept { get; set; }
        public string org { get; set; }

        public bool isMineNow { get; set; }
        public List<Talk> talks { get; set; }
    }

    public class Chats
    {
        public Chats()
        {
            chats = new List<Chat>();
        }

        public List<Chat> chats { get; set; }
    }

    public class Talk
    {
        public bool ak { get; set; }
        public string img { get; set; }
        public string name { get; set; }
        public string mid { get; set; }
        public bool mine { get; set; }
        public int msgSt { get; set; }
        public string strMsg { get; set; }
        public string timeOfMsg { get; set; }
        public Msgtypes MsgType { get; set; }
    }

    public class Phrase
    {
        public int pkey { get; set; }
        public string id { get; set; }
        public string EditId { get; set; }
        public string txt { get; set; }
        public bool isGlobal { get; set; }
    }

    public class Bio
    {
        public string strBio { get; set; }
        public string strHeader { get; set; }
        public string strJobTitle { get; set; }
        public string strOrg { get; set; }
        public string topImage { get; set; }
        public string strAddress { get; set; }
        public object conStatus { get; set; }
    }

    public enum Msgtypes
    {
        chatMsg = 0,
        infoMsg = 1
    }

    public static class ChatOperations
    {       
        public static string add(string myID, string ID, int ChtType, string nm, Talk talk, string myName, int eventKey, bool fromMobile)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@myID", myID),
                    new SqlParameter("@ID", ID),
                    new SqlParameter("@ChtType", ChtType),
                    new SqlParameter("@img", talk.img),
                    new SqlParameter("@nm", nm),
                    new SqlParameter("@ak", talk.ak),
                    new SqlParameter("@mid", talk.mid),
                    new SqlParameter("@mine", talk.mine),
                    new SqlParameter("@msgSt", talk.msgSt),
                    new SqlParameter("@strMsg", talk.strMsg),
                    new SqlParameter("@myName", myName),
                    new SqlParameter("@eventKey", eventKey),
                    new SqlParameter("@paramESTTime", clsEvent.getEventVenueTime()),
                    new SqlParameter("@fromMobile", fromMobile)
                };

                string qry = "sp_insertChat";
                DataSet ds = operation(operationType.Get, qry, sqlParameters);

                if (ds.Tables.Count == 3 && ds.Tables[2].Rows.Count > 0)
                {
                    string strPhone = ds.Tables[2].Rows[0]["Phone"].ToString();
                    string companyName = ds.Tables[2].Rows[0]["companyName"].ToString();
                    string Title = "MAGI chat Notification: " + myName + " of " + companyName;
                    string strBody = "MAGI Notification: " + myName + " of " + companyName + " would like to chat with you in the MAGI event. Do not reply to this text message. Reply through chat.";
                    bool NotifyOnMsg = Convert.ToBoolean(ds.Tables[2].Rows[0]["NotifyOnMsg"]);

                    if (NotifyOnMsg)
                    {
                        SendSMS(strBody, strPhone,  Convert.ToInt32(ID), Convert.ToInt32(myID), eventKey, 1 , Title);
                    }
                }

                string CorrectName = string.Empty;
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    if (Convert.ToBoolean(ds.Tables[0].Rows[0]["gotError"]))
                    {
                        CorrectName = ds.Tables[0].Rows[0]["CorrectName"].ToString();
                    }
                }

                if (ChtType == 1 && ds.Tables.Count > 1 && ds.Tables[1].Rows.Count == 0)
                {
                    new SqlOperation().UserReminderStatusUpdate(eventKey, Convert.ToInt32(ID), clsReminders.R_ChatContacted);
                }

                if (!string.IsNullOrEmpty(CorrectName))
                {
                    return CorrectName;
                }

                return null;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static void UpdateMsgStatus(string myID, string msg_ID, int status, string targetID)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@myID", myID),
                    new SqlParameter("@targetID", targetID),
                    new SqlParameter("@msgID", msg_ID),
                    new SqlParameter("@sts", status)
                };
                string qry = "sp_UpdateMsgStatus";
                operation(operationType.AddRemove, qry, sqlParameters);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //////Get The Name starts...............
        public static object getTheName(string ID)
        {
            try
            {
                object oName = null;
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@ID", ID)
                };
                string qry = "sp_getNameByID";
                DataSet ds = operation(operationType.Get, qry, sqlParameters);

                if (ds.Tables.Count == 1 && ds.Tables[0].Rows.Count > 0)
                {
                    oName = new
                    {
                        nick = ds.Tables[0].Rows[0]["nickname"].ToString(),
                        name = ds.Tables[0].Rows[0]["name"].ToString(),
                        //topImg = ChatOperations.ImagePath(ID, ds.Tables[0].Rows[0]["topImg"].ToString())
                        topImg = ds.Tables[0].Rows[0]["topImg"].ToString()
                    };
                }

                return oName;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //////Get The Name ends...............
        
        public static object getChatContactList(int accKey, int eventKey, bool isDemo)
        {
            try
            {
                List<object> ObjList = new List<object>();
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@ParamId", accKey),
                    new SqlParameter("@paramEvent_pkey", eventKey),
                    new SqlParameter("@paramViewStateKey", -1),
                    new SqlParameter("@currentDate", clsEvent.getEventVenueTime()),
                    new SqlParameter("@isForDemo", isDemo)
                };
                string qry = "SP_GetPeopleForNotificationPanel";
                DataSet ds = operation(operationType.Get, qry, sqlParameters);

                if(ds.Tables.Count > 0)
                {
                    foreach(DataRow dr in ds.Tables[0].Rows)
                    {
                        object obj = new {
                            dataimg = dr["image"].ToString().Replace("~", ""),
                            dataname = dr["ContactName"].ToString(),
                            datatitle = dr["title"].ToString(),
                            databio = dr["bio"].ToString(),
                            datadept = dr["dept"].ToString(),
                            dataorg = dr["org"].ToString(),
                            inmychat = dr["InMyChat"].ToString(),
                            datasort = dr["timeOfMsg"].ToString(),
                            datakey = dr["id"].ToString(),
                            searchstring = dr["searchString"].ToString(),
                            isonline = dr["isOnline"].ToString(),
                            constatusnow = dr["conStatusNow"].ToString(),
                            fromme = dr["fromMe"].ToString(),
                            refusedbyown = dr["refusedByOwn"].ToString(),
                            isexpired = dr["isExpired"].ToString(),
                            personalrole = (dr["isSpeaker"].ToString() == "1" ? "Speaker" : "")
                                           + (dr["isAttendee"].ToString() == "1" ? " Attendee" : "")
                                           + (dr["isSponser"].ToString() == "1" ? " Sponsor" : "")
                        };

                        ObjList.Add(obj);
                    }
                }

                return new { list = ObjList };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //////Get The connection status starts here...............
        public static object GetConStatusNow(int myID, int targetID, int eventKey, int sessionID)
        {
            try
            {
                object complexObject = null;
                object connSts = null;
                object roles = null;
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@myID", myID),
                    new SqlParameter("@targetID", targetID),
                    new SqlParameter("@eventKey", eventKey),
                    new SqlParameter("@sessionID", sessionID)
                };

                string qry = "sp_GetConStatusNow";
                DataSet ds = operation(operationType.Get, qry, sqlParameters);

                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        connSts = new
                        {
                            st = ds.Tables[0].Rows[0]["conStatusNow"].ToString(),
                            fromMe = Convert.ToBoolean(ds.Tables[0].Rows[0]["fromMe"]),
                            refusedByOwn = Convert.ToBoolean(ds.Tables[0].Rows[0]["refusedByOwn"]),
                            isExpired = Convert.ToBoolean(ds.Tables[0].Rows[0]["isExpired"]),
                            nick = ds.Tables[0].Rows[0]["nick"].ToString()
                        };
                    }

                    if ((ds.Tables.Count > 1) && ds.Tables[1].Rows.Count > 0)
                    {
                        roles = new
                        {
                            isSpeaker = ds.Tables[1].Rows[0]["isSpeaker"].ToString(),
                            isAttendee = ds.Tables[1].Rows[0]["isAttendee"].ToString(),
                            isSponser = ds.Tables[1].Rows[0]["isSponser"].ToString()
                        };
                    }
                }

                return complexObject = new
                {
                    connStatus = connSts ,
                    rolesOfPerson = roles ,
                    isOnline =
                            (
                                ds.Tables.Count > 1 &&
                                ds.Tables[1].Rows.Count > 0 &&
                                Convert.ToBoolean(ds.Tables[1].Rows[0]["isOnline"])
                            )
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //////Get The connection status ends here...............

        public static Chats getChatHistory(string myID, string diff, string eventKey)
        {
            try
            {
                Chats chats = new Chats();
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@ParamId", myID),
                    new SqlParameter("@ParamDateTime",clsEvent.getEventVenueTime()),
                    new SqlParameter("@ParamEventKey",eventKey)
                };
                string qry = "sp_GetChatHistory";
                DataSet ds = operation(operationType.Get, qry, sqlParameters);

                if(ds.Tables.Count > 0)
                {
                    foreach(DataTable dt in ds.Tables)
                    {
                        if (dt.Rows.Count > 0)
                        {
                            Chat chat = new Chat();
                            chat.ChtType = Convert.ToInt32(dt.Rows[0]["ChtType"]);
                            chat.ID = dt.Rows[0]["ID"].ToString();
                            //chat.img = ImagePath(dt.Rows[0]["ID"].ToString(), dt.Rows[0]["topImg"].ToString());
                            chat.img = dt.Rows[0]["topImg"].ToString();
                            chat.nm = dt.Rows[0]["topNm"].ToString();
                            chat.nick = dt.Rows[0]["nickName"].ToString();
                            chat.isMineNow = Convert.ToBoolean(dt.Rows[0]["isMineNow"]);

                            chat.JobTitle = dt.Rows[0]["title"].ToString();
                            chat.bio = dt.Rows[0]["bio"].ToString();
                            chat.dept = dt.Rows[0]["dept"].ToString();
                            chat.org = dt.Rows[0]["org"].ToString();

                            foreach (DataRow dr in dt.Rows)
                            {
                                Talk talk = new Talk();
                                talk.ak = Convert.ToBoolean(dr["ak"]);
                                //talk.img = ImagePath(dr["myID"].ToString(), dr["img"].ToString());
                                talk.img = dr["img"].ToString();
                                talk.mid = dr["mid"].ToString();
                                talk.name = dr["nm"].ToString();
                                talk.mine = Convert.ToBoolean(dr["mine"]);
                                talk.msgSt = Convert.ToInt32(dr["msgSt"]);
                                talk.strMsg = dr["strMsg"].ToString();
                                talk.timeOfMsg = dr["timeOfMsg"].ToString();
                                talk.MsgType = (Msgtypes)Convert.ToInt32(dr["typeOfMsg"]);
                                chat.talks.Add(talk);
                            }
                            chats.chats.Add(chat);
                        }
                    }
                }

                return chats;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static Chats getChatHistoryByPerson(string myID, string diff, string eventKey, string targetId)
        {
            try
            {
                targetId = targetId.Replace(" ", "_");
                Chats chats = new Chats();
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@ParamMyId", myID),
                    new SqlParameter("@ParamDateTime",clsEvent.getEventVenueTime()),
                    new SqlParameter("@ParamEventKey",eventKey),
                    new SqlParameter("@ParamTargetId",targetId)
                };
                string qry = "sp_GetChatHistoryByPerson";
                DataSet ds = operation(operationType.Get, qry, sqlParameters);

                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataTable dt in ds.Tables)
                        {
                            if (dt.Rows.Count > 0)
                            {
                                Chat chat = new Chat();
                                chat.ChtType = Convert.ToInt32(dt.Rows[0]["ChtType"]);
                                chat.ID = dt.Rows[0]["ID"].ToString();
                                //chat.img = ImagePath(dt.Rows[0]["ID"].ToString(), dt.Rows[0]["topImg"].ToString());
                                chat.img = dt.Rows[0]["topImg"].ToString();
                                chat.nm = dt.Rows[0]["topNm"].ToString();
                                chat.isMineNow = Convert.ToBoolean(dt.Rows[0]["isMineNow"]);

                                foreach (DataRow dr in dt.Rows)
                                {
                                    Talk talk = new Talk();
                                    talk.ak = Convert.ToBoolean(dr["ak"]);
                                    //talk.img = ImagePath(dr["myID"].ToString(), dr["img"].ToString());
                                    talk.img = dr["img"].ToString();
                                    talk.mid = dr["mid"].ToString();
                                    talk.name = dr["nm"].ToString();
                                    talk.mine = Convert.ToBoolean(dr["mine"]);
                                    talk.msgSt = Convert.ToInt32(dr["msgSt"]);
                                    talk.strMsg = dr["strMsg"].ToString();
                                    talk.timeOfMsg = dr["timeOfMsg"].ToString();
                                    talk.MsgType = (Msgtypes)Convert.ToInt32(dr["typeOfMsg"]);
                                    chat.talks.Add(talk);
                                }
                                chats.chats.Add(chat);
                            }
                        }
                    }
                }

                return chats;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static void deleteMyChat(string myID, string myChatId)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@myID", myID),
                    new SqlParameter("@myChatId", myChatId),
                };
                string qry = "sp_DeleteMyChat";
                operation(operationType.AddRemove, qry, sqlParameters);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static void SendSMS(string smsBody, string PhoneNumber, int Account_pkey, int AddedBy_pkey, int eventKey, int SMSType, string Title)
        {
            try
            {
                string accountSid = ConfigurationManager.AppSettings["accountSid"].ToString();
                string authToken = ConfigurationManager.AppSettings["authToken"].ToString();
                string txtFrom = ConfigurationManager.AppSettings["txtFrom"].ToString();

                TwilioClient.Init(accountSid, authToken);
                if (ConfigurationManager.AppSettings["QAMode"].ToString() == "1")
                {
                    PhoneNumber = PhoneNumber.Length>=10 ? "+919971572401" : PhoneNumber;
                }
                var Url = ConfigurationManager.AppSettings["AppURL"].Replace("/forms", "") + "/api/Twilio/post";
                var message = MessageResource.Create(body: smsBody, from: new Twilio.Types.PhoneNumber(txtFrom), statusCallback: new Uri(Url.ToString()), to: new Twilio.Types.PhoneNumber(PhoneNumber.Trim().ToString()));
                string SID = message.Sid;
                string MessageSMSID = message.MessagingServiceSid;
                string MessageSid = message.Sid;
                string MessageStatus = message.Status.ToString();

                if (!string.IsNullOrEmpty(SID))
                {
                    SqlParameter[] sqlParameters =
                    {
                        new SqlParameter("@SMSID", SID),
                        new SqlParameter("@SMSText", smsBody),
                        new SqlParameter("@MobileNumber", PhoneNumber),
                        new SqlParameter("@Status", message.Status.ToString()),
                        new SqlParameter("@Account_pkey", Convert.ToInt32(Account_pkey)),
                        new SqlParameter("@SmsMessageSid", MessageSMSID),
                        new SqlParameter("@MessageSid", MessageSid),
                        new SqlParameter("@MessageStatus", MessageStatus),
                        new SqlParameter("@AddedBy", AddedBy_pkey),
                        new SqlParameter("@pkey", "0"),
                        new SqlParameter("@Event_pkey",eventKey),
                        new SqlParameter("@SMSType",SMSType),
                        new SqlParameter("@FROMNumber",txtFrom)
                    };
                    string qry = "SMS_LIST_Save";
                    operation(operationType.AddRemove, qry, sqlParameters);
                }
            }
            catch (Exception ex)
            {

                SqlParameter[] sqlParameters1 =
                   {
                        new SqlParameter("@Account_pkey", Account_pkey),
                        new SqlParameter("@Event_pkey", eventKey),
                        new SqlParameter("@AddedBypkey", AddedBy_pkey),
                        new SqlParameter("@Title", Title.ToString()),
                        new SqlParameter("@Body",smsBody )
                    };
                string qry = "NotyficationChat_SendviaEmail";
                operation(operationType.AddRemove, qry, sqlParameters1);
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// //////////////This is for common operation
        /// </summary>
        public static DataSet operation(operationType operationType, string qry, SqlParameter[] sqlParameters)
        {
            try
            {
                if (operationType == operationType.AddRemove)
                {
                    SqlHelper.ExecuteNonQuery(qry, CommandType.StoredProcedure, sqlParameters);
                    return null;
                }
                else if (operationType.Get == operationType)
                {
                    DataSet ds = new DataSet();
                    ds = SqlHelper.ExecuteSet(qry, CommandType.StoredProcedure, sqlParameters);
                    return ds;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        //public static string ImagePath(string AccPkey, string alternateImage)
        //{
        //    string strImage = "/accountimages/" + AccPkey + "_img.jpg";
        //    string strPhysicalPath = HttpContext.Current.Server.MapPath(strImage);
        //    bool bExists = clsUtility.FileExists(strPhysicalPath);
        //    if (bExists)
        //        return strImage;
        //    return alternateImage;
        //}

        public static string[] CheckForAnyBroadCast(int AccPkey, int eventKey)
        {
            try
            {
                string qry = "select b.pkey as [id],b.[Message] as msg from dbo.[BroadCastMessage] b where ";
                qry = qry + Environment.NewLine + "(b.BroadcastType=1";
                qry = qry + Environment.NewLine + "AND DATEADD(MINUTE,60,[DateTime]) >= cast(@estTime as datetime))";
                qry = qry + Environment.NewLine + "and 1 = case when (b.ForAttendee = 1";
                qry = qry + Environment.NewLine + "and exists(";
                qry = qry + Environment.NewLine + "select top(1) 1 from Event_Accounts ea";
                qry = qry + Environment.NewLine + "inner join Account_List ac";
                qry = qry + Environment.NewLine + "on ac.pKey = ea.Account_pKey";
                qry = qry + Environment.NewLine + "where ea.Account_pKey = @myID";
                qry = qry + Environment.NewLine + "and ea.ParticipationStatus_pKey = 1";
                qry = qry + Environment.NewLine + "and ea.Event_pKey = @eventKey";
                qry = qry + Environment.NewLine + ")) then 1";
                qry = qry + Environment.NewLine + "when (b.ForStaffOnly = 1 and exists(select top(1) 1 from Account_List ac where ac.pKey=@myID and (ac.StaffMember=1 or ac.GlobalAdministrator=1))) then 1 else 0 end";
                qry = qry + Environment.NewLine + "and b.accountKey!=@myID and not exists(select 1 from [AcknowledgeBroadCast] ack where b.pkey = ack.BroadCast_pKey and ack.accountKey = @myID);";

                //qry = qry + Environment.NewLine + "if (isnull((select showWelcomeTextOnLogin from [Event_List] where [pkey] = @eventKey),0) = 1)";
                //qry = qry + Environment.NewLine + "begin";
                //qry = qry + Environment.NewLine + "select AppTextBlock from Application_Text where pkey = 204 and (not exists(select 1 from dbo.AcknowledgeBroadCast ac where ac.accountKey=@myID and cast(ac.AcknowledgeDateTime as date) = cast(@estTime as date) and ac.BroadCast_pKey = -1));";
                //qry = qry + Environment.NewLine + "end";

                //qry = qry + Environment.NewLine + "select (select trim(o.[sessionKey]) + '^' from OpenSessionBroadCast o where";
                //qry = qry + Environment.NewLine + "DATEADD(MINUTE,ISNULL(o.[OpenTill],0),o.TimeOfBroadCast) > cast(@estTime as datetime)";
                //qry = qry + Environment.NewLine + "FOR XML PATH('')) as sessionBroadcast;";

                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@myID", AccPkey),
                    new SqlParameter("@estTime", clsEvent.getEventVenueTime()),
                    new SqlParameter("@eventKey", eventKey)
                };
                DataSet ds = SqlHelper.ExecuteSet(qry, CommandType.Text, sqlParameters);
                
                if(ds.Tables.Count > 0)
                {
                    string[] arrResult = new string[2];
                    if(ds.Tables[0].Rows.Count > 0)
                        arrResult[0] = ds.Tables[0].Rows[0]["msg"].ToString() + "^" + ds.Tables[0].Rows[0]["id"].ToString();

                    //if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                    //{
                    //    arrResult[1] = ds.Tables[1].Rows[0]["AppTextBlock"].ToString();
                    //}
                    //if (ds.Tables[1].Rows.Count > 0)
                    //    arrResult[1] = ds.Tables[1].Rows[0]["sessionBroadcast"].ToString();

                    return arrResult;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static void HideThisChat(int Acckey,string msg_key,bool hide)
        {
            try
            {
                string qry = string.Empty;
                qry = qry + Environment.NewLine + "update sys_talks";
                qry = qry + Environment.NewLine + "set [HideBy_AccountKey]=@accID,";
                qry = qry + Environment.NewLine + "[HideAt]=@estTime,";
                qry = qry + Environment.NewLine + "[Hidden]=@hide";
                qry = qry + Environment.NewLine + "where [mid]=@msg_key";

                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@accID", Acckey),
                    new SqlParameter("@estTime", clsEvent.getEventVenueTime()),
                    new SqlParameter("@hide", hide),
                    new SqlParameter("@msg_key", msg_key)
                };
                SqlHelper.ExecuteSet(qry, CommandType.Text, sqlParameters);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static void HideTheseChatsFromPerson(string myId, int Acckey, int ToPK, string Conversationdate, int eventKey, int? HiddenToAcc, int? HiddenToSecond_Acc, int typeOfPerson)
        {
            try
            {
                DateTime ConversDate = Convert.ToDateTime(Conversationdate);
                string qry = "SP_hideChatFromPerson";
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@myId", myId),
                    new SqlParameter("@PK", Acckey),
                    new SqlParameter("@ToPK", ToPK),
                    new SqlParameter("@Conversationdate", ConversDate),
                    new SqlParameter("@eventKey", eventKey),
                    new SqlParameter("@HiddenToAcc", HiddenToAcc),
                    new SqlParameter("@HiddenToSecond_Acc", HiddenToSecond_Acc),
                    new SqlParameter("@typeOfPerson", typeOfPerson)
                };
                SqlHelper.ExecuteSet(qry, CommandType.StoredProcedure, sqlParameters);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static bool HideAllMsgsofPerson(int myId, int Acckey, int eventKey, bool hide)
        {
            try
            {
                string qry = string.Empty;
                qry = qry + Environment.NewLine + "update sys_talks";
                qry = qry + Environment.NewLine + "set [HideBy_AccountKey]=@myId,";
                qry = qry + Environment.NewLine + "[HideAt]=@estTime,";
                qry = qry + Environment.NewLine + "[Hidden]=@hide";
                qry = qry + Environment.NewLine + "where myID = @PKAccKey";
                qry = qry + Environment.NewLine + "and Event_pkey = @eventKey;";

                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@myId", myId),
                    new SqlParameter("@PKAccKey", Acckey),
                    new SqlParameter("@eventKey", eventKey),
                    new SqlParameter("@hide", hide),
                    new SqlParameter("@estTime", clsEvent.getEventVenueTime())
                };
                SqlHelper.ExecuteSet(qry, CommandType.Text, sqlParameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static bool HaveAnyInterestGroup(int accKey, int eventKey)
        {
            bool haveAnyGroup = false;
            string qry = string.Empty;
            qry = "select c.ID from sys_chats c where c.myID = " + accKey.ToString() + " and c.Event_pkey = " + eventKey.ToString() + " and upper(c.ID) like 'ADMINGRP%'";
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();

            SqlParameter[] sqlParameters =
            {
                new SqlParameter("@myID", accKey),
                new SqlParameter("@ParamEventKey", eventKey)
            };
            dt = SqlHelper.ExecuteTable(qry, CommandType.Text, sqlParameters);
            if (dt.Rows.Count > 0)
            {
                haveAnyGroup = true;
            }
            return haveAnyGroup;
        }

        public static DataSet GetCrisisGroupMembers(int ExceptMe)
        {
            try
            {
                string qry = string.Empty;
                qry = qry + Environment.NewLine + "select distinct cast(mem.Account_pKey as nvarchar(10)) as Account_pKey,p.SecurityGroup_pKey,p.Privilege_pkey";
                qry = qry + Environment.NewLine + ",(ISNULL(ac.Firstname,'') + ' ' + ISNULL(ac.Lastname,'')) as fullName,";
                qry = qry + Environment.NewLine + "(case when(ac.Phone is not null and ac.Phone != '') then";
                qry = qry + Environment.NewLine + "'+'+ (case";
                qry = qry + Environment.NewLine + "when (ISNULL(ac.PhoneType_pKey,0)=12) then (ISNULL(ac.Phone1CC,'') + dbo.Fn_GetNumeric(ISNULL(ac.Phone,'')))";
                qry = qry + Environment.NewLine + "when (ISNULL(ac.PhoneType2_pKey,0)=12) then (ISNULL(ac.Phone2CC,'')+ dbo.Fn_GetNumeric(case when((ac.Phone2 is null) or (ac.Phone2 = '')) then ac.Phone else ac.Phone2 end))";
                qry = qry + Environment.NewLine + "else (ISNULL(ac.Phone1CC,'') + dbo.Fn_GetNumeric(ISNULL(ac.Phone,''))) end)";
                qry = qry + Environment.NewLine + "else '' end) as MobileNumber,[Email]";
                qry = qry + Environment.NewLine + "from SecurityGroup_Members mem";
                qry = qry + Environment.NewLine + "inner join Account_List ac on mem.Account_pKey = ac.pKey";
                qry = qry + Environment.NewLine + "inner join SecurityGroup_List grp on grp.pKey = mem.SecurityGroup_pKey";
                qry = qry + Environment.NewLine + "inner join SecurityGroup_Privileges p on p.SecurityGroup_pKey = grp.pKey";
                qry = qry + Environment.NewLine + "where p.Privilege_pkey = " + clsPrivileges.CrisisAlert + " and mem.Account_pKey <> " + ExceptMe.ToString();
                qry = qry + Environment.NewLine + "order by Account_pKey;";

                qry = qry + Environment.NewLine + "select (ISNULL(ac.Firstname,'') + ' ' + ISNULL(ac.Lastname,'')) as fullName,";
                qry = qry + Environment.NewLine + "(case when(ac.Phone is not null and ac.Phone != '') then";
                qry = qry + Environment.NewLine + "'+'+ (case";
                qry = qry + Environment.NewLine + "when (ISNULL(ac.PhoneType_pKey,0)=12) then (ISNULL(ac.Phone1CC,'') + dbo.Fn_GetNumeric(ISNULL(ac.Phone,'')))";
                qry = qry + Environment.NewLine + "when (ISNULL(ac.PhoneType2_pKey,0)=12) then (ISNULL(ac.Phone2CC,'')+ dbo.Fn_GetNumeric(case when((ac.Phone2 is null) or (ac.Phone2 = '')) then ac.Phone else ac.Phone2 end))";
                qry = qry + Environment.NewLine + "else (ISNULL(ac.Phone1CC,'') + dbo.Fn_GetNumeric(ISNULL(ac.Phone,''))) end)";
                qry = qry + Environment.NewLine + "else '' end) as MobileNumber,[Email] from Account_List ac where ac.pKey = " + ExceptMe.ToString();

                DataSet ds = new DataSet();
                ds = SqlHelper.ExecuteSet(qry, CommandType.Text, null);
                return ds;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static int CrisisInsert(int sentBy, string strMsg, string strTitle, string CrisisThread, string SentTo, int Event_pkey)
        {
            try
            {
                ////////////Insert in DB/////////////////
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@sentBy", sentBy),
                    new SqlParameter("@SentDatetime", DateTime.Now),
                    new SqlParameter("@strMsg", strMsg),
                    new SqlParameter("@strTitle", strTitle),
                    new SqlParameter("@CrisisThread", CrisisThread),
                    new SqlParameter("@SendTo", SentTo),
                    new SqlParameter("@Event_pkey", Event_pkey)
                };
                string qry = string.Empty;
                qry = qry + Environment.NewLine + "INSERT INTO [CrisisAlert]";
                qry = qry + Environment.NewLine + "([SentBy_Accountpkey],[SentDatetime],[strMsg],[strTitle],[CrisisThread],[SendTo],[Event_pkey])";
                qry = qry + Environment.NewLine + "VALUES";
                qry = qry + Environment.NewLine + "(@sentBy,@SentDatetime,@strMsg,@strTitle,@CrisisThread,@SendTo,@Event_pkey);";
                qry = qry + Environment.NewLine + "SELECT isnull(SCOPE_IDENTITY(),0) AS [SCOPE_IDENTITY];";
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
    }
}
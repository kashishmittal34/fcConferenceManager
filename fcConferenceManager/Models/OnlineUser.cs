using fcConferenceManager;
using MAGI_API.SignalR;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MAGI_API.Models
{
    public enum operationType
    {
        AddRemove = 1,
        Get = 2
    }

    [Serializable]
    public class OnlineUser
    {
        public int id { get; set; }
        public string sessionId { get; set; }
        public string chatId { get; set; }
        public string chatUserId { get; set; }
        public string img { get; set; }
        public string ipAddress { get; set; }
    }

    [Serializable]
    public static class OnlineUsers
    {
        public static void Add(int id, string sessionId, string chatUserId, string PageURL, string ipAddress, bool FirstTime, string browserName)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paramId", id),
                    new SqlParameter("@paramSessionId", sessionId),
                    new SqlParameter("@paramChatId", chatUserId),
                    new SqlParameter("@paramPageURL", PageURL),
                    new SqlParameter("@paramIpAddress", ipAddress),
                    new SqlParameter("@paramFirstTime", FirstTime),
                    new SqlParameter("@parambrowserName", browserName)
                };

                string qry = string.Empty;
                qry = qry + Environment.NewLine + "IF NOT EXISTS(select 1 from dbo.sys_onlinePeople o where o.id=@paramId and o.[sessionId]=@paramSessionId and o.[chatid]=@paramChatId and o.[chatUserId]=@paramId and o.[ipAddress]=@paramIpAddress)";
                qry = qry + Environment.NewLine + "BEGIN";
                qry = qry + Environment.NewLine + "insert into sys_onlinePeople([id],[sessionId],[chatid],[chatUserId],[ipAddress],[onlinedate],[PageURL],[firstTime],Browser) values";
                qry = qry + Environment.NewLine + "(@paramId,@paramSessionId,@paramChatId,@paramId,@paramIpAddress,getdate(),@paramPageURL,@paramFirstTime,@parambrowserName)";
                qry = qry + Environment.NewLine + "END";
                operation(operationType.AddRemove, qry, sqlParameters);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        //public static void Add(string chatUserId, string chatId)
        //{
        //    try
        //    {
        //        SqlParameter[] sqlParameters =
        //        {
        //            new SqlParameter("@paramChatId", chatId),
        //            new SqlParameter("@paramChatUserId", chatUserId)
        //        };

        //        string qry = "insert into sys_onlinePeople([id],[sessionId],[chatid],[chatUserId],[onlinedate]) values (@paramChatUserId,'',@paramChatId,@paramChatUserId,getdate());";
        //        operation(operationType.AddRemove, qry, sqlParameters);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //}

        public static void Update(int id, string sessionId, string chatId, string PageURL, string ipAddress, bool FirstTime, string browserName)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paramSessionId", sessionId),
                    new SqlParameter("@paramChatId", chatId),
                    new SqlParameter("@paramPageURL", PageURL),
                    new SqlParameter("@paramIpAddress", ipAddress),
                    new SqlParameter("@paramFirstTime", FirstTime),
                    new SqlParameter("@parambrowserName", browserName)
                };

                string qry = "update sys_onlinePeople";
                qry = qry + Environment.NewLine + "set";

                if (!string.IsNullOrEmpty(chatId))
                    qry = qry + Environment.NewLine + "[chatId]=@paramChatId,";
                if (!string.IsNullOrEmpty(PageURL))
                    qry = qry + Environment.NewLine + "[PageURL]=@paramPageURL,";
                if (!string.IsNullOrEmpty(ipAddress))
                    qry = qry + Environment.NewLine + "[ipAddress]=@paramIpAddress,";
                if (!string.IsNullOrEmpty(browserName))
                    qry = qry + Environment.NewLine + "[Browser]=@parambrowserName,";

                qry = qry + Environment.NewLine + "[onlinedate] = getdate(),";
                qry = qry + Environment.NewLine + "[firstTime] = @paramFirstTime";

                qry = qry + Environment.NewLine + "where [sessionId] = @paramSessionId;";
                operation(operationType.AddRemove, qry, sqlParameters);
                //qry = qry + "update sys_onlinePeople set [sessionId] = @paramSessionId, [ipAddress]=@paramIpAddress, [onlinedate] = getdate() where [id] = @paramId;";
                //List<OnlineUser> onlineUsers = operation(operationType.Get, qry, sqlParameters);

                ////////////Log Out from other machines, if already some where.......
                //if (onlineUsers != null && onlineUsers.Count > 0)
                //{
                //    OnlineUser obj = onlineUsers.FirstOrDefault(x => x.id == id);
                //    if (obj != null)
                //    {
                //        string currentIP = obj.ipAddress;
                //        if (currentIP != ipAddress)
                //        {
                //            var _hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<ChatHub>();
                //            _hubContext.Clients.All.matchIPAdress(id, ipAddress);
                //            //In case when on another event or chat could be disabled
                //            var _hubLocalContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<LocalHub>();
                //            _hubLocalContext.Clients.All.matchIPAdress(id, ipAddress);
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        //public static void Update(string chatUserId, string chatId, string pageUrl)
        //{
        //    try
        //    {
        //        SqlParameter[] sqlParameters =
        //        {
        //            new SqlParameter("@paramChatId", chatId),
        //            new SqlParameter("@paramChatUserId", chatUserId),
        //            new SqlParameter("@paramPageURL", pageUrl)
        //        };
        //        string qry = "update sys_onlinePeople set [PageURL] = @paramPageURL,[chatid] = @paramChatId, [onlinedate] = getdate() where [chatUserId] = @paramChatUserId;";
        //        operation(operationType.AddRemove, qry, sqlParameters);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //}

        //public static void RemoveById(string id)
        //{
        //    try
        //    {
        //        SqlParameter[] sqlParameters =
        //        {
        //            new SqlParameter("@paramId", id)
        //        };
        //        string qry = "delete from sys_onlinePeople where [id] = @paramId;";
        //        operation(operationType.AddRemove, qry, sqlParameters);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //}

        //public static void RemoveByChatUserId(string chatUserId)
        //{
        //    try
        //    {
        //        SqlParameter[] sqlParameters =
        //        {
        //            new SqlParameter("@paramChatUserId", chatUserId)
        //        };
        //        string qry = "delete from sys_onlinePeople where [chatUserId] = @paramChatUserId;";
        //        operation(operationType.AddRemove, qry, sqlParameters);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //}

        public static void RemoveBySession(string sessionId)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paramSessionId", sessionId)
                };
                string qry = "delete from sys_onlinePeople where [sessionId] = @paramSessionId;";
                operation(operationType.AddRemove, qry, sqlParameters);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static void RemoveByChatSession(string chatId)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paramChatId", chatId)
                };
                string qry = "delete from sys_onlinePeople where [chatid] = @paramChatId;";
                operation(operationType.AddRemove, qry, sqlParameters);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static OnlineUser getByID(int id)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paramId", id)
                };
                List<OnlineUser> onlineUsers = null;
                string qry = "select [id], [sessionId], [chatId], [chatUserId] from sys_onlinePeople  where [id] = @paramId;";
                onlineUsers = operation(operationType.Get, qry, sqlParameters);
                if (onlineUsers.Count > 0)
                    return onlineUsers[0];
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static OnlineUser getByChatUserID(string chatUserId)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paramChatUserId", chatUserId)
                };
                List<OnlineUser> onlineUsers = null;

                string qry = "select [id], [sessionId], [chatId], [chatUserId],ipAddress,";
                qry += Environment.NewLine + "(ISNULL((select top(1) case when(al.HasImageOnProfile = 1) then ('/accountimages/' + cast(al.pKey as nvarchar(20)) + '_img.jpg') else al.AlternateImage end as [image] from Account_List al where al.pKey = @paramChatUserId),'')) as Img";
                qry += Environment.NewLine + "from sys_onlinePeople  where [chatUserId] = @paramChatUserId;";

                onlineUsers = operation(operationType.Get, qry, sqlParameters);
                if (onlineUsers.Count > 0)
                {
                    //onlineUsers[0].img = ChatOperations.ImagePath(onlineUsers[0].id.ToString(), onlineUsers[0].img);
                    return onlineUsers[0];
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static OnlineUser getBySession(string sessionId)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paramSessionId", sessionId)
                    //new SqlParameter("@paramChatUserId", sessionId)
                };
                List<OnlineUser> onlineUsers = null;
                string qry = "select [id], [sessionId], [chatId], [chatUserId],ipAddress,";
                qry += Environment.NewLine + "(ISNULL((select top(1) case when(al.HasImageOnProfile = 1) then ('/accountimages/' + cast(al.pKey as nvarchar(20)) + '_img.jpg') else al.AlternateImage end as [image] from Account_List al where al.pKey = o.id),'')) as Img";
                qry += Environment.NewLine + "from sys_onlinePeople o  where [sessionId] = @paramSessionId;";
                onlineUsers = operation(operationType.Get, qry, sqlParameters);
                if (onlineUsers.Count > 0)
                {
                    //onlineUsers[0].img = ChatOperations.ImagePath(onlineUsers[0].id.ToString(), onlineUsers[0].img);
                    return onlineUsers[0];
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static OnlineUser getByChatSession(string chatId)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paramChatId", chatId)
                };
                List<OnlineUser> onlineUsers = null;
                string qry = "select [id], [sessionId], [chatId], [chatUserId] from sys_onlinePeople  where [chatId] = @paramChatId;";
                onlineUsers = operation(operationType.Get, qry, sqlParameters);
                if (onlineUsers.Count > 0)
                    return onlineUsers[0];
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static List<OnlineUser> getList()
        {
            try
            {
                List<OnlineUser> onlineUsers = null;
                string qry = "select [id], [sessionId], [chatId], [chatUserId] from sys_onlinePeople;";
                onlineUsers = operation(operationType.Get, qry, null);

                return onlineUsers;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static bool Exists(string id)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@paramId", id)
                };
                List<OnlineUser> onlineUsers = null;
                string qry = "select [id], [sessionId], [chatId], [chatUserId] from sys_onlinePeople where [id] = @paramId;";
                onlineUsers = operation(operationType.Get, qry, sqlParameters);

                if (onlineUsers.Count > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static string GetIP()
        {
            string ip = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ip))
            {
                ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            return ip.Split(':')[0];
        }

        /// <summary>
        /// //////////////This is for common operation
        /// </summary>
        private static List<OnlineUser> operation(operationType operationType, string qry, SqlParameter[] sqlParameters)
        {
            try
            {
                if (operationType == operationType.AddRemove)
                {
                    SqlHelper.ExecuteNonQuery(qry, CommandType.Text, sqlParameters);
                    return null;
                }
                else if (operationType.Get == operationType)
                {
                    List<OnlineUser> onlineUsers = null;
                    onlineUsers = SqlHelper.ExecuteList<OnlineUser>(qry, CommandType.Text, sqlParameters);
                    return onlineUsers;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /////////////////
        ///Some extra and commonly used function on MAGI

        public static string DataTableToJSON(DataTable table)
        {
            try
            {
                string JSONString = string.Empty;
                JSONString = JsonConvert.SerializeObject(table);
                return JSONString;
            }
            catch
            {
                return null;
            }
        }

        //public static string RefreshAccountImage(int intAcctPKey, string AlternateImage)
        //{
        //    try
        //    {
        //        string strImagePath = "~/accountimages/" + intAcctPKey.ToString() + "_img.jpg";
        //        string strPhysicalPath = HttpContext.Current.Server.MapPath(strImagePath);
        //        bool bExists = clsUtility.FileExists(strPhysicalPath);

        //        if (bExists)
        //        {
        //            return strImagePath;
        //        }

        //        return AlternateImage;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
    }
}
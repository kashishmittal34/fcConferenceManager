using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MAGI_API.Models
{
    public class Group
    {
        public Group()
        {
            GroupUers = new List<string>();
        }
        public string GroupId { get; set; }
        public List<string> GroupUers { get; set; }
    }

    public static class GroupOperations
    {
        public static Group FirstOrDefault(string ID, string myID)
        {
            try
            {
                Group group = new Group();
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@ParamId", ID),
                    new SqlParameter("@MyId", myID)
                };
                string qry = "sp_getChatGroup";
                DataSet ds = ChatOperations.operation(operationType.Get, qry, sqlParameters);

                group.GroupId = ID;
                if(ds.Tables.Count > 0)
                {
                    foreach(DataRow dr in ds.Tables[0].Rows)
                    {
                        string strUser = dr["myID"].ToString();
                        group.GroupUers.Add(strUser);
                    }
                }
                return group;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static void Add(string groupID, string ID, string eventKey, bool needToLeave = false)
        {
            try
            {
                groupID = groupID.Replace(" ", "_");
                Group group = new Group();
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@groupID", groupID),
                    new SqlParameter("@ID", ID),
                    new SqlParameter("@eventKey",eventKey),
                    new SqlParameter("@needToLeave", needToLeave)
                };
                string qry = "sp_AddToGroup";
                ChatOperations.operation(operationType.AddRemove, qry, sqlParameters);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static void RemoveFromGroup(string groupID, string ID, string eventKey)
        {
            try
            {
                groupID = groupID.Replace(" ", "_");
                Group group = new Group();
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@groupID", groupID),
                    new SqlParameter("@ID", ID),
                    new SqlParameter("@eventKey",eventKey)
                };
                string qry = "Sp_RemoveFromGroup";
                ChatOperations.operation(operationType.AddRemove, qry, sqlParameters);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    public class GroupChats
    {
        public GroupChats()
        {
            GroupTypes = new Dictionary<string,string>();
            GroupTypes.Add("Event Chat", "1");
            GroupTypes.Add("Topic Chats", "2");
            //keyValuePairs.Add("Session Chats", "3");
            GetTopicChats();
        }

        public Dictionary<string,string> GroupTypes { get; set; }
        public Dictionary<string,string> GroupByTopic { get; set; }
        public Chats DefaultEventChat { get; set; }

        private void GetTopicChats()
        {
            try
            {
                GroupByTopic = new Dictionary<string, string>();

                string qry = string.Empty;
                qry = qry + Environment.NewLine + "select n.pKey,n.topicName";
                qry = qry + Environment.NewLine + "from sys_networkingtopic n where n.IsActive = 1";
                DataTable dt = SqlHelper.ExecuteTable(qry, CommandType.Text, null);

                foreach(DataRow dr in dt.Rows)
                {
                    GroupByTopic.Add(dr["topicName"].ToString(), dr["pKey"].ToString());
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}
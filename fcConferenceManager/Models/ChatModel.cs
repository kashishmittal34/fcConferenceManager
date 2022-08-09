using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace fcConferenceManager.Models
{
    public class ChatModel
    {
        public class addUsersToGroup
        {
            public class Success
            {               
                public string status { get; set; }
               
                public string message { get; set; }
            }
            public class Root
            {                           
                public Success success { get; set; }
            }
        }

        public class creategroup
        {
            public class Success
            {                
                public string status { get; set; }               
                public string message { get; set; }              
                public string guid { get; set; }
            }

            public class Root
            {              
                public Success success { get; set; }
            }
        }

        public class createuser
        {
            public class Success
            {                
                public string status { get; set; }                
                public string message { get; set; }
            }

            public class Root
            {               
                public Success success { get; set; }
            }
        }

        public class GetCallHistory
        {
            public class DataItem
            {               
                public string UID { get; set; }               
                public string call_type { get; set; }                
                public string start_time { get; set; }              
                public string end_time { get; set; }                
                public string status { get; set; }            
                public string duration { get; set; }
            }

            public class Success
            {               
                public string status { get; set; }              
                public string message { get; set; }             
                public List<DataItem> data { get; set; }
            }

            public class Root
            {               
                public Success success { get; set; }
            }
        }

        public class GetGroupMessages
        {
            public class GroupchatsItem
            {                
                public string message_id { get; set; }               
                public string guid { get; set; }               
                public string sender_uid { get; set; }              
                public string message { get; set; }              
                public string timestamp { get; set; }
            }

            public class Data
            {               
                public List<GroupchatsItem> groupchats { get; set; }
            }

            public class Success
            {               
                public string status { get; set; }               
                public string message { get; set; }               
                public Data data { get; set; }
            }

            public class Root
            {              
                public Success success { get; set; }
            }
        }

        public class GetMessages
        {
            public class One_on_oneItem
            {               
                public string message_id { get; set; }               
                public string sender_uid { get; set; }             
                public string reciever_uid { get; set; }              
                public string message { get; set; }              
                public string timestamp { get; set; }              
                public string read { get; set; }             
                public string visibility { get; set; }
            }

            public class Data
            {               
                public List<One_on_oneItem> one_on_one { get; set; }
            }

            public class Success
            {               
                public string status { get; set; }               
                public string message { get; set; }                
                public Data data { get; set; }
            }

            public class Root
            {               
                public Success success { get; set; }
            }
        }

        public class GetUnreadMessageCount
        {
            public class UsercountsItem
            {               
                public string UID { get; set; }               
                public string count { get; set; }
            }

            public class Success
            {                
                public string status { get; set; }              
                public int totalcount { get; set; }             
                public List<UsercountsItem> usercounts { get; set; }
            }

            public class Root
            {               
                public Success success { get; set; }
            }
        }

        public class GetUser
        {
            public class User
            {               
                public string uid { get; set; }               
                public string name { get; set; }              
                public string link { get; set; }             
                public string avatar { get; set; }             
                public string friends { get; set; }                
                public string role { get; set; }              
                public string credits { get; set; }               
                public string cid { get; set; }               
                public string lastactivity { get; set; }
            }

            public class Data
            {                
                public User user { get; set; }
            }

            public class Success
            {                
                public string status { get; set; }                
                public string message { get; set; }              
                public Data data { get; set; }
            }

            public class Root
            {              
                public Success success { get; set; }
            }
        }
        public class GetUserStatus
        {
            public class UIDs
            {                
                public string user { get; set; }               
                public string status { get; set; }               
                public string lastactivity { get; set; }               
                public string conversations { get; set; }
            }

            public class Success
            {
                public string UIDs { get; set; }
            }

            public class Root
            {               
                public Success success { get; set; }
            }
        }

        public class groups
        {
            public class data1
            {
                public string name { get; set; }
                public string GUID { get; set; }
                public string type { get; set; }
            }
            public class Success
            {              
                public string status { get; set; }              
                public string message { get; set; }             
                public string GUID { get; set; }
            }

            public class Root
            {                
                public Success success { get; set; }
            }
        }

        public class result
        {
            public class Success
            {                
                public string status { get; set; }                
                public string message { get; set; }
            }
            public class Failed
            {              
                public string status { get; set; }                
                public string message { get; set; }
            }
            public class Root
            {                
                public Success success { get; set; }
                public Failed failed { get; set; }
            }
        }

        public class Userlist
        {
            public class DataItem
            {                
                public string username { get; set; }               
                public string displayname { get; set; }               
                public string friends { get; set; }               
                public string uid { get; set; }              
                public string role { get; set; }             
                public string status { get; set; }               
                public string lastactivity { get; set; }
            }

            public class Success
            {               
                public string status { get; set; }               
                public string message { get; set; }              
                public List<DataItem> data { get; set; }
            }

            public class Root
            {               
                public Success success { get; set; }
            }
        }

    }
}
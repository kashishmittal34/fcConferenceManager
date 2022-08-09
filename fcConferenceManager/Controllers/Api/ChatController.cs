using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using static fcConferenceManager.Models.ChatModel;

namespace fcConferenceManager.Controllers
{
    [RoutePrefix("Chat")]
    public class ChatController : ApiController
    {
        public static string chatApiKey = "54684x195f63f59378b59ff703f0bdc3f6e250";
        [HttpPost]
        [Route("Chat/userlist")]
        public async Task<IHttpActionResult> userlist()
        {
            Userlist.Root obj = null;
            try
            {

                using (var client = new HttpClient())
                {
                    HttpContent hc = new StringContent("Content-Type", Encoding.UTF8, "application/json");
                    client.DefaultRequestHeaders.Add("api-key", chatApiKey);
                    var Res = await client.PostAsync(new Uri("https://api.cometondemand.net/api/v2/listUsers"), hc);              
                    if (Res.IsSuccessStatusCode)
                    {                        
                        var EmpResponse = Res.Content.ReadAsStringAsync().Result;
                        obj = JsonConvert.DeserializeObject<Userlist.Root>(EmpResponse);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Ok(obj);
        }

        
        [HttpPost]
        [Route("Chat/CreateUser")]
        public  async Task<IHttpActionResult> CreateUser()        
        {
            createuser.Root obj1 = null;           
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("UID", HttpContext.Current.Request.Params["UID"]));
                nvc.Add(new KeyValuePair<string, string>("name", HttpContext.Current.Request.Params["name"]));
                nvc.Add(new KeyValuePair<string, string>("role", HttpContext.Current.Request.Params["role"]));
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);               
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/createUser") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);
                if (res.IsSuccessStatusCode)
                {
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    obj1 = JsonConvert.DeserializeObject<createuser.Root>(EmpResponse);
                }
            }
            catch(Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

            return Ok(obj1);
        }

        [HttpPost]
        [Route("Chat/GetUser")]
        public async Task<IHttpActionResult> GetUser()      
        {
            GetUser.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("UID", HttpContext.Current.Request.Params["UID"]));              
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/getUser") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);
                if (res.IsSuccessStatusCode)
                {
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    obj1 = JsonConvert.DeserializeObject<GetUser.Root>(EmpResponse);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
             return Ok(obj1);
        }

        [HttpPost]
        [Route("Chat/GetUserStatus")]
        public async Task<IHttpActionResult> GetUserStatus()      
        {
            //GetUserStatus.Root obj1 = null;
            GetUserStatus.UIDs obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("UIDs", HttpContext.Current.Request.Params["UIDs"]));
                //  nvc.Add(new KeyValuePair<string, string>("name", "TEST2"));
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/getUserStatus") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);
                if (res.IsSuccessStatusCode)
                {
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                     obj1 = JsonConvert.DeserializeObject<GetUserStatus.UIDs>(EmpResponse);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
             return Ok(obj1);
        }

        [HttpPost]
        [Route("Chat/DeleteUser")]
        public async Task<IHttpActionResult> DeleteUser()
        {
            result.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("UID", HttpContext.Current.Request.Params["UID"]));
               // nvc.Add(new KeyValuePair<string, string>("name", "TEST2"));
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/deleteUser") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);
                if (res.IsSuccessStatusCode)
                {                   
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    obj1 = JsonConvert.DeserializeObject<result.Root>(EmpResponse);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

             return Ok(obj1);
        }

        [HttpPost]
        [Route("Chat/UpdateUser")]   
         public async Task<IHttpActionResult> UpdateUser()
        {
            result.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("UID", HttpContext.Current.Request.Params["UID"]));
                nvc.Add(new KeyValuePair<string, string>("name", HttpContext.Current.Request.Params["name"]));
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);
                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/updateUser") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);

                if (res.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;

                   obj1 = JsonConvert.DeserializeObject<result.Root>(EmpResponse);

                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

             return Ok(obj1);
        }

        [HttpPost]
        [Route("Chat/AddFriends")]
       // public async void AddFriends()
         public async Task<IHttpActionResult> AddFriends()
        {
            result.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("UID", HttpContext.Current.Request.Params["UID"]));
                nvc.Add(new KeyValuePair<string, string>("friendsUID", HttpContext.Current.Request.Params["friendsUID"]));
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);
                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/addFriends") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);

                if (res.IsSuccessStatusCode)
                {

                    //Storing the response details recieved from web api  
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                   obj1 = JsonConvert.DeserializeObject<result.Root>(EmpResponse);


                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

             return Ok(obj1);
        }

        [HttpPost]
        [Route("Chat/DeleteFriends")]
       // public async void DeleteFriends()
    public async Task<IHttpActionResult> DeleteFriends()
        {
            result.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("UID", HttpContext.Current.Request.Params["UID"]));
                nvc.Add(new KeyValuePair<string, string>("friendsUID", HttpContext.Current.Request.Params["friendsUID"]));
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);
                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/deleteFriends") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);

                if (res.IsSuccessStatusCode)
                {

                    //Storing the response details recieved from web api  
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;

                     obj1 = JsonConvert.DeserializeObject<result.Root>(EmpResponse);


                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

             return Ok(obj1);
        }

        [HttpPost]
        [Route("Chat/BlockUser")]
        // public async void DeleteFriends()
        public async Task<IHttpActionResult> BlockUser()
        {
            result.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("senderUID", HttpContext.Current.Request.Params["senderUID"]));
                nvc.Add(new KeyValuePair<string, string>("receiverUID", HttpContext.Current.Request.Params["receiverUID"]));
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);
                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/blockUser") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);

                if (res.IsSuccessStatusCode)
                {

                    //Storing the response details recieved from web api  

                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    obj1 = JsonConvert.DeserializeObject<result.Root>(EmpResponse);


                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

            return Ok(obj1);
        }

        [HttpPost]
        [Route("Chat/UnblockUser")]
        // public async void DeleteFriends()
        public async Task<IHttpActionResult> UnblockUser()
        {
            result.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("senderUID", HttpContext.Current.Request.Params["senderUID"]));
                nvc.Add(new KeyValuePair<string, string>("receiverUID", HttpContext.Current.Request.Params["receiverUID"]));
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);
                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/unblockUser") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);

                if (res.IsSuccessStatusCode)
                {

                    var EmpResponse = res.Content.ReadAsStringAsync().Result;

                    obj1 = JsonConvert.DeserializeObject<result.Root>(EmpResponse);


                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

            return Ok(obj1);
        }

        [HttpPost]
        [Route("Chat/CreateGroup")]      
        public async Task<IHttpActionResult> CreateGroup()
        {
            creategroup.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("GUID", HttpContext.Current.Request.Params["GUID"]));
                nvc.Add(new KeyValuePair<string, string>("name", HttpContext.Current.Request.Params["name"]));
                nvc.Add(new KeyValuePair<string, string>("type", HttpContext.Current.Request.Params["type"]));
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);
                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/createGroup") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);

                if (res.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api  
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    obj1 = JsonConvert.DeserializeObject<creategroup.Root>(EmpResponse);


                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

            return Ok(obj1);
        }

        [HttpPost]
        [Route("Chat/DeleteGroup")]
        // public async void DeleteGroup()
        public async Task<IHttpActionResult> DeleteGroup()
        {
            result.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("GUID", HttpContext.Current.Request.Params["GUID"]));
               // nvc.Add(new KeyValuePair<string, string>("name", HttpContext.Current.Request.Params["name"]));
               // nvc.Add(new KeyValuePair<string, string>("type", HttpContext.Current.Request.Params["type"]));
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);
                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/deleteGroup") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);

                if (res.IsSuccessStatusCode)
                {
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;

                    obj1 = JsonConvert.DeserializeObject<result.Root>(EmpResponse);


                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

            return Ok(obj1);
        }

        [HttpPost]
        [Route("Chat/addUsersToGroup")]
        // public async void addUsersToGroup()
        public async Task<IHttpActionResult> addUsersToGroup()
        {
            addUsersToGroup.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("GUID", HttpContext.Current.Request.Params["GUID"]));
                 nvc.Add(new KeyValuePair<string, string>("UIDs", HttpContext.Current.Request.Params["UIDs"]));
                 nvc.Add(new KeyValuePair<string, string>("clearExisting", HttpContext.Current.Request.Params["clearExisting"]));
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);
                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/addUsersToGroup") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);

                if (res.IsSuccessStatusCode)
                {

                    var EmpResponse = res.Content.ReadAsStringAsync().Result;

                    obj1 = JsonConvert.DeserializeObject<addUsersToGroup.Root>(EmpResponse);


                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

           return Ok(obj1);
        }

        [HttpPost]
        [Route("Chat/removeUsersFromGroup")]
       // public async void removeUsersFromGroup()
         public async Task<IHttpActionResult> removeUsersFromGroup()
        {
            addUsersToGroup.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("GUID", HttpContext.Current.Request.Params["GUID"]));
                nvc.Add(new KeyValuePair<string, string>("UIDs", HttpContext.Current.Request.Params["UIDs"]));
                //nvc.Add(new KeyValuePair<string, string>("clearExisting", HttpContext.Current.Request.Params["clearExisting"]));
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);
                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/removeUsersFromGroup") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);

                if (res.IsSuccessStatusCode)
                {

                    var EmpResponse = res.Content.ReadAsStringAsync().Result;

                     obj1 = JsonConvert.DeserializeObject<addUsersToGroup.Root>(EmpResponse);


                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

            return Ok(obj1);
        }

        [HttpPost]
        [Route("Chat/SendMessage")]
         //public async void SendMessage()
        public async Task<IHttpActionResult> SendMessage()
        {
            result.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("senderUID", HttpContext.Current.Request.Params["senderUID"]));
                nvc.Add(new KeyValuePair<string, string>("receiverUID", HttpContext.Current.Request.Params["receiverUID"]));
                nvc.Add(new KeyValuePair<string, string>("message", HttpContext.Current.Request.Params["message"]));
                nvc.Add(new KeyValuePair<string, string>("isGroup", HttpContext.Current.Request.Params["isGroup"]));
                nvc.Add(new KeyValuePair<string, string>("visibility", HttpContext.Current.Request.Params["visibility"]));
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);
                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/sendMessage") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);

                if (res.IsSuccessStatusCode)
                {

                    var EmpResponse = res.Content.ReadAsStringAsync().Result;

                    obj1 = JsonConvert.DeserializeObject<result.Root>(EmpResponse);


                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

            return Ok(obj1);
        }

        [HttpPost]
        [Route("Chat/GetMessages")]
        //public async void GetMessages()
        public async Task<IHttpActionResult> GetMessages()
        {
            GetMessages.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("UIDs", HttpContext.Current.Request.Params["UIDs"]));
                nvc.Add(new KeyValuePair<string, string>("offset", HttpContext.Current.Request.Params["offset"]));
                nvc.Add(new KeyValuePair<string, string>("limit", HttpContext.Current.Request.Params["limit"]));
                nvc.Add(new KeyValuePair<string, string>("withUIDs", HttpContext.Current.Request.Params["withUIDs"]));
                //nvc.Add(new KeyValuePair<string, string>("visibility", HttpContext.Current.Request.Params["visibility"]));
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);
                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/getMessages") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);

                if (res.IsSuccessStatusCode)
                {

                    var EmpResponse = res.Content.ReadAsStringAsync().Result;

                    obj1 = JsonConvert.DeserializeObject<GetMessages.Root>(EmpResponse);


                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

            return Ok(obj1);
        }

        [HttpPost]
        [Route("Chat/GetGroupMessages")]
       // public async void GetGroupMessages()
        public async Task<IHttpActionResult> GetGroupMessages()
        {
            GetGroupMessages.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("GUIDs", HttpContext.Current.Request.Params["GUIDs"]));
                nvc.Add(new KeyValuePair<string, string>("offset", HttpContext.Current.Request.Params["offset"]));
                nvc.Add(new KeyValuePair<string, string>("limit", HttpContext.Current.Request.Params["limit"]));
                //nvc.Add(new KeyValuePair<string, string>("withUIDs", HttpContext.Current.Request.Params["withUIDs"]));
                //nvc.Add(new KeyValuePair<string, string>("visibility", HttpContext.Current.Request.Params["visibility"]));
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);
                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/getGroupMessages") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);

                if (res.IsSuccessStatusCode)
                {

                    var EmpResponse = res.Content.ReadAsStringAsync().Result;

                    obj1 = JsonConvert.DeserializeObject<GetGroupMessages.Root>(EmpResponse);


                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

            return Ok(obj1);
        }

        [HttpPost]
        [Route("Chat/GetCallHistory")]
        //public async void GetCallHistory()
        public async Task<IHttpActionResult> GetCallHistory()
        {
            GetCallHistory.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("UID", HttpContext.Current.Request.Params["UID"]));
                nvc.Add(new KeyValuePair<string, string>("offset", HttpContext.Current.Request.Params["offset"]));
                nvc.Add(new KeyValuePair<string, string>("limit", HttpContext.Current.Request.Params["limit"]));
                nvc.Add(new KeyValuePair<string, string>("calltype", HttpContext.Current.Request.Params["calltype"]));
                //nvc.Add(new KeyValuePair<string, string>("visibility", HttpContext.Current.Request.Params["visibility"]));
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);
                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/getCallHistory") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);

                if (res.IsSuccessStatusCode)
                {

                    var EmpResponse = res.Content.ReadAsStringAsync().Result;

                    obj1 = JsonConvert.DeserializeObject<GetCallHistory.Root>(EmpResponse);


                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

            return Ok(obj1);
        }
        [HttpPost]
        [Route("Chat/SendBroadcastMessage")]
        
        public async Task<IHttpActionResult> SendBroadcastMessage()
        {
            result.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("senderUID", HttpContext.Current.Request.Params["senderUID"]));
                nvc.Add(new KeyValuePair<string, string>("receiverUIDs", HttpContext.Current.Request.Params["receiverUIDs"]));
                nvc.Add(new KeyValuePair<string, string>("message", HttpContext.Current.Request.Params["message"]));
                
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);
                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/sendBroadcastMessage") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);

                if (res.IsSuccessStatusCode)
                {

                    var EmpResponse = res.Content.ReadAsStringAsync().Result;

                    obj1 = JsonConvert.DeserializeObject<result.Root>(EmpResponse);


                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

            return Ok(obj1);
        }

        [HttpPost]
        [Route("Chat/GetUnreadMessageCount")]
        //public async void GetUnreadMessageCount()
        public async Task<IHttpActionResult> GetUnreadMessageCount()
        {
            GetUnreadMessageCount.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("UID", HttpContext.Current.Request.Params["UID"]));                
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/getUnreadMessageCounts") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);
                if (res.IsSuccessStatusCode)
                {
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    obj1 = JsonConvert.DeserializeObject<GetUnreadMessageCount.Root>(EmpResponse);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

            return Ok(obj1);
        }
        [HttpPost]
        [Route("Chat/GetUnreadMessageCountGroups")]        
        public async Task<IHttpActionResult> GetUnreadMessageCountGroups()
        {
            result.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("UIDs", HttpContext.Current.Request.Params["UIDs"]));
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/getUnreadMessageCountForGroups") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);
                if (res.IsSuccessStatusCode)
                {
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    obj1 = JsonConvert.DeserializeObject<result.Root>(EmpResponse);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

            return Ok(obj1);
        }


        [HttpPost]
        [Route("Chat/SendSticker")]        
        public async Task<IHttpActionResult> SendSticker()
        {
            result.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("senderUID", HttpContext.Current.Request.Params["senderUID"]));
                nvc.Add(new KeyValuePair<string, string>("receiverUIDs", HttpContext.Current.Request.Params["receiverUIDs"]));
                nvc.Add(new KeyValuePair<string, string>("isGroup", HttpContext.Current.Request.Params["isGroup"]));
                nvc.Add(new KeyValuePair<string, string>("category", HttpContext.Current.Request.Params["category"]));
                nvc.Add(new KeyValuePair<string, string>("key", HttpContext.Current.Request.Params["key"]));

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);                
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/sendSticker") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);
                if (res.IsSuccessStatusCode)
                {
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    obj1 = JsonConvert.DeserializeObject<result.Root>(EmpResponse);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

            return Ok(obj1);
        }

        [HttpPost]
        [Route("Chat/SendAnnouncement")]     
        public async Task<IHttpActionResult> SendAnnouncement()
        {
            result.Root obj1 = null;
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("announcement", HttpContext.Current.Request.Params["announcement"])); 
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", chatApiKey);               
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cometondemand.net/api/v2/sendAnnouncement") { Content = new FormUrlEncodedContent(nvc) };
                var res = await client.SendAsync(req);
                if (res.IsSuccessStatusCode)
                {
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    obj1 = JsonConvert.DeserializeObject<result.Root>(EmpResponse);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
            return Ok(obj1);
        }
    }
}


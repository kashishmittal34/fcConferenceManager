using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Models;
using Newtonsoft.Json;

namespace Controllers
{
    [RoutePrefix("api/Listrek")]
    public class ListrekController : ApiController
    {
        readonly string clientId = "v8auht0muvoby7xm2r9g";
        readonly string clientSecret = "N4oO06KETBAfWC4bDIoalb7wxfXArFGG1T9VyD+5AzU";
        private async Task<string> GetToken()
        {
            string url = "https://auth.listrak.com/OAuth2/Token";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret)
                });

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var token = await response.Content.ReadAsAsync<Token>();
                    return token.access_token;
                }
            };

            return string.Empty;
        }

        [HttpGet]
        [Route("Lists/GetAll")]
        public async Task<IHttpActionResult> GetAllLists()
        {
            string token = await GetToken();

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            
            var response = await client.GetAsync("v1/List");

            if (response.IsSuccessStatusCode)
            {
                string x = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ListsResponse>(x);
                return Ok(data);
            }

            return BadRequest();            
        }

        [HttpGet]
        [Route("List/GetListById/{listId}")]
        public async Task<IHttpActionResult> GetListById(int listId)
        {
            string token = await GetToken();

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var response = await client.GetAsync("v1/List/" + listId.ToString().Trim());

            if (response.IsSuccessStatusCode)
            {
                string x = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ListResponse>(x);
                return Ok(data);
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("Lists/Search/{email}")]
        public async Task<IHttpActionResult> GetAllListsByEmail(string email)
        {
            List<int> lstListIds = new List<int>();
            var lists = await GetAllLists();
            
            var listsResult = lists as OkNegotiatedContentResult<ListsResponse>;
            var listsResponse = listsResult.Content;

            foreach(var data in listsResponse.data)
            {
                var contacts = await GetAllContacts(data.listId);
                var contactsResult = contacts as OkNegotiatedContentResult<ListContactsResponse>;
                var contactsResponse = contactsResult.Content;

                foreach (var d in contactsResponse.data)
                {
                    if (d.emailAddress.Trim().Equals(email))
                    {
                        lstListIds.Add(data.listId);
                    }
                }
            }

            return Ok(lstListIds);
        }

        [HttpGet]
        [Route("Campaigns/GetAll/{listId}")]
        public async Task<IHttpActionResult> GetAllCampaigns(int listId)
        {
            string token = await GetToken();

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var response = await client.GetAsync("v1/List/"+ listId.ToString().Trim()
                + "/Campaign");

            if (response.IsSuccessStatusCode)
            {
                string x = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<CampaignsResponse>(x);
                return Ok(data);
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("List/{listId}/Message")]
        public async Task<IHttpActionResult> GetAllMessages(int listId)
        {
            string token = await GetToken();

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var response = await client.GetAsync("v1/List/" + listId.ToString().Trim()
                + "/Message");

            if (response.IsSuccessStatusCode)
            {
                string x = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<MessagesResponse>(x);
                return Ok(data);
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("Folders/GetAll")]
        public async Task<IHttpActionResult> GetAllFolders()
        {
            string token = await GetToken();

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var response = await client.GetAsync("v1/Folder");

            if (response.IsSuccessStatusCode)
            {
                string x = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<FoldersResponse>(x);
                return Ok(data);
            }

            return BadRequest();           
        }

        [HttpGet]
        [Route("List/Imports/{listId}")]
        public async Task<IHttpActionResult> GetListImports(int listId)
        {
            string token = await GetToken();

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var response = await client.GetAsync("v1/List/" + listId.ToString().Trim()
                + "/ListImport");

            if (response.IsSuccessStatusCode)
            {
                string x = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ListImportsResponse>(x);
                return Ok(data);
            }

            return BadRequest();
        }

        [HttpPut]
        [Route("List/Update/{updateList}")]
        public async Task<IHttpActionResult> UpdateList(CreateList updateList)
        {
            string token = await GetToken();
            var data = JsonConvert.SerializeObject(updateList);
            var stringContent = new StringContent(data, Encoding.UTF8, "application/json");

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var response = await client.PutAsJsonAsync("v1/List/361718", updateList);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<CommonRESTResponse>();
                return Ok(result);
            }

            return BadRequest();
        }

        [HttpDelete]
        [Route("List/Delete/{listId}")]
        public async Task<IHttpActionResult> DeleteList(int listId)
        {
            string token = await GetToken();
           
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var response = await client.DeleteAsync("v1/List/"+listId.ToString());

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<string>();
                return Ok(result);
            }

            return BadRequest();
        }

        [HttpPost]
        [Route("List/Create/{createList}")]
        public async Task<IHttpActionResult> CreateNewList(CreateList createList)
        {
            string token = await GetToken();
            var data = JsonConvert.SerializeObject(createList);
            var stringContent = new StringContent(data, Encoding.UTF8, "application/json");

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var response = await client.PostAsJsonAsync(client.BaseAddress.ToString(),
                    stringContent);            

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<CommonRESTResponse>();
                return Ok(result);
            }

            return BadRequest();
        }

        #region EventGroup

        [HttpPost]
        [Route("EventGroup/Create/{listId}/{createList}")]
        public async Task<IHttpActionResult> CreateNewEventGroup(EventGroup createEventGroup, int listId)
        {
            string token = await GetToken();
            var data = JsonConvert.SerializeObject(createEventGroup);
            var stringContent = new StringContent(data, Encoding.UTF8, "application/json");

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            string url = "v1/List/"+ listId.ToString() + "/EventGroup";

            var response = await client.PostAsJsonAsync(url,
                    createEventGroup);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<CommonRESTResponse>();
                return Ok(result);
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("EventGroups/GetAll/{listId}")]
        public async Task<IHttpActionResult> GetAllEventGroups(int listId)
        {
            string token = await GetToken();

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var response = await client.GetAsync("v1/List/"+ listId .ToString() +"/EventGroup");

            if (response.IsSuccessStatusCode)
            {
                string x = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<EventGroupsResponse>(x);
                return Ok(data);
            }

            return BadRequest();
        }

        [HttpPut]
        [Route("EventGroup/Update/{listId}/{updateEventGroup}")]
        public async Task<IHttpActionResult> UpdateEventGroup(EventGroup updateEventGroup, 
            int listId, int eventGroupId)
        {
            string token = await GetToken();
            var data = JsonConvert.SerializeObject(updateEventGroup);
            var stringContent = new StringContent(data, Encoding.UTF8, "application/json");

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            string url = "v1/List/" + listId.ToString() + "/EventGroup/"+ eventGroupId.ToString();

            var response = await client.PutAsJsonAsync(url,
                    updateEventGroup);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<CommonRESTResponse>();
                return Ok(result);
            }

            return BadRequest();
        }

        [HttpDelete]
        [Route("EventGroup/Delete/{listId}/{eventGroupId}")]
        public async Task<IHttpActionResult> DeleteEventGroup(int listId, int eventGroupId)
        {
            string token = await GetToken();

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var response = await client.DeleteAsync("v1/List/" +
                listId.ToString()+ "/EventGroup/"+ eventGroupId.ToString());

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<string>();
                return Ok(result);
            }

            return BadRequest();
        }
               
        #endregion


        #region Event

        [HttpPost]
        [Route("Event/Create/{listId}/{createEvent}")]
        public async Task<IHttpActionResult> CreateNewEvent(CreateEvent createEvent, int listId)
        {
            string token = await GetToken();
            var data = JsonConvert.SerializeObject(createEvent);
            var stringContent = new StringContent(data, Encoding.UTF8, "application/json");

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            string url = "v1/List/" + listId.ToString() + "/Event";

            var response = await client.PostAsJsonAsync(url,
                    createEvent);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<CommonRESTResponse>();
                return Ok(result);
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("List/{listId}/Event/GetAll")]
        public async Task<IHttpActionResult> GetAllEvents(int listId)
        {
            string token = await GetToken();

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var response = await client.GetAsync("v1/List/" + listId.ToString().Trim() + "/Event");

            if (response.IsSuccessStatusCode)
            {
                string x = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ListImportsResponse>(x);
                return Ok(data);
            }

            return BadRequest();
        }


        #endregion


        #region Contact

        [HttpPost]
        [Route("Contact/CreateOrUpdate/{status}")]       
        public async Task<IHttpActionResult> ContactCreateOrUpdate([FromUri]string status, [FromUri]string email)
        {
            string listId = ConfigurationManager.AppSettings["ListrakListKey"].ToString();           
            string token = await GetToken();
            string updateType = "Update";
            ContactCreateUpdate contact = new ContactCreateUpdate();
            contact.emailAddress = email;
            contact.subscriptionState = status;
            contact.segmentationFieldValues = null;
            //if (status == "Subscribed")
            //    contact.SegmentationFieldValues = new List<SegmentationFields>() { new SegmentationFields { segmentationFieldId = 2796425, value = accountId.ToString() } };
            var data = JsonConvert.SerializeObject(contact);
            var stringContent = new StringContent(data, Encoding.UTF8, "application/json");

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            //string url = "v1/List/" + listId.ToString() + "/Contact";
            //url += "?eventIds=12288&?overrideUnsubscribe=true&subscribedByContact=true" +
            //    "&sendDoubleOptIn=false&updateType="+ updateType +"&newEmailAddress="+ newEmailAddress;

            //string url = "v1/List/" + listId.ToString() + "/Contact";
            //url += "?eventIds=12288&?overrideUnsubscribe=true&subscribedByContact=true" +
            //    "&sendDoubleOptIn=false&updateType=" + updateType;

            string url = "v1/List/" + listId.ToString() + "/Contact";
            url += "?overrideUnsubscribe=true&subscribedByContact=true" +
                "&sendDoubleOptIn=false&updateType=" + updateType;

            var response = await client.PostAsJsonAsync(url, contact);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<CommonRESTResponse>();
                return Ok(result);
            }
            else
            {
                var result = await response.Content.ReadAsAsync<ErrorMessage>();
                return Ok(result);
                //return BadRequest();
            }
        }

        [HttpPost]
        [Route("Contact/UpdateContact/{listId}/{createOrUpdateContact}/{newEmailAddress}")]
        public async Task<IHttpActionResult> UpdateContact
           (ContactCreateUpdate createOrUpdateContact, int listId, string newEmailAddress)
        {
            string token = await GetToken();
            var data = JsonConvert.SerializeObject(createOrUpdateContact);
            var stringContent = new StringContent(data, Encoding.UTF8, "application/json");

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            string url = "v1/List/" + listId.ToString() + "/Contact";
            url += "?overrideUnsubscribe=false&subscribedByContact=true" +
                "&sendDoubleOptIn=false&updateType=" + "Update" + "&newEmailAddress=" + newEmailAddress;

            var response = await client.PostAsJsonAsync(url,
                    createOrUpdateContact);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<CommonRESTResponse>();
                return Ok(result);
            }
            else
            {
                var result = await response.Content.ReadAsAsync<ErrorMessage>();
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("List/{listId}/Contacts")]
        public async Task<IHttpActionResult> GetAllContacts(int listId)
        {
            string token = await GetToken();

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var response = await client.GetAsync("v1/List/" + listId.ToString().Trim() + "/Contact");

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    string x = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<ListContactsResponse>(x);
                    return Ok(data);
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("List/{listId}/Contact")]
        public async Task<IHttpActionResult> GetContact([FromUri]int listId, [FromUri]string email)
        {
            string token = await GetToken();

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var response = await client.GetAsync("v1/List/" + listId.ToString().Trim() + "/Contact/" + email.Trim());

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    string x = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<ContactsResponse>(x);
                    return Ok(data);
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

            return BadRequest();
        }

        #endregion

        #region ListImport

        [HttpGet]
        [Route("List/{listId}/ListImport/{importFileId}")]
        public async Task<IHttpActionResult> GetAllListImports(int listId, int importFileId)
        {
            string token = await GetToken();

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var response = await client.GetAsync("v1/List/" + listId.ToString().Trim()
                + "/ListImport/"+ importFileId.ToString());

            if (response.IsSuccessStatusCode)
            {
                string x = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ListImportsResponse>(x);
                return Ok(data);
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("List/{listId}/ListImport/All")]
        public async Task<IHttpActionResult> GetAllListImports(int listId)
        {
            string token = await GetToken();

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var response = await client.GetAsync("v1/List/" + listId.ToString().Trim()
                + "/ListImport");

            if (response.IsSuccessStatusCode)
            {
                string x = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ListImportResponse>(x);
                return Ok(data);
            }

            return BadRequest();
        }

        [HttpPost]
        [Route("List/{listId}/ListImport/{startListImport}")]
        public async Task<IHttpActionResult> StartListImport(StartListImport startListImport, int listId)
        {
            string token = await GetToken();
            var data = JsonConvert.SerializeObject(startListImport);
            var stringContent = new StringContent(data, Encoding.UTF8, "application/json");

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            string url = "v1/List/" + listId.ToString() + "/ListImport";

            var response = await client.PostAsJsonAsync(url,
                    startListImport);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<CommonRESTResponse>();
                return Ok(result);
            }
            else
            {
                var result = await response.Content.ReadAsAsync<ErrorMessage>();
                return BadRequest();
            }
        }

        #endregion

        #region Segmentation Group

        [HttpGet]
        [Route("List/{listId}/SegmentationGroups/All")]
        public async Task<IHttpActionResult> GetAllSegmentationGroups(int listId)
        {
            string token = await GetToken();

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var response = await client.GetAsync("v1/List/" + listId.ToString().Trim()
                + "/SegmentationFieldGroup");

            if (response.IsSuccessStatusCode)
            {
                string x = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<SegmentationFieldGroupResponse>(x);
                return Ok(data);
            }

            return BadRequest();
        }

        [HttpPost]
        [Route("List/SegmentationFieldGroup/Create/{segmentationFieldGroup}/{listId}")]
        public async Task<IHttpActionResult> CreateSegmentationFieldGroup
            (CreateSegmentationFieldGroup segmentationFieldGroup, int listId)
        {
            string token = await GetToken();
            var data = JsonConvert.SerializeObject(segmentationFieldGroup);
            var stringContent = new StringContent(data, Encoding.UTF8, "application/json");

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            string url = "v1/List/" + listId.ToString() + "/SegmentationFieldGroup";

            var response = await client.PostAsJsonAsync(url,
                    segmentationFieldGroup);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<CommonRESTResponse>();
                return Ok(result);
            }
            else
            {
                var result = await response.Content.ReadAsAsync<ErrorMessage>();
                return BadRequest();
            }
        }




        #endregion


        #region Segmentation Field

        [HttpPost]
        [Route("List/SegmentationField/Create/{segmentationField}/{listId}/{segmentationFieldGroupId}")]
        public async Task<IHttpActionResult> CreateSegmentationField
           (SegmentationField segmentationField, int listId, int segmentationFieldGroupId)
        {
            string token = await GetToken();
            var data = JsonConvert.SerializeObject(segmentationField);
            var stringContent = new StringContent(data, Encoding.UTF8, "application/json");

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            string url = "v1/List/" + listId.ToString() +
                "/SegmentationFieldGroup/"+ segmentationFieldGroupId.ToString()+ "/SegmentationField";

            var response = await client.PostAsJsonAsync(url,
                    segmentationField);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<CommonRESTResponse>();
                return Ok(result);
            }
            else
            {
                var result = await response.Content.ReadAsAsync<ErrorMessage>();
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("List/{listId}/SegmentationField/{segmentationFieldGroupId}/All")]
        public async Task<IHttpActionResult> GetAllSegmentationFields(int listId, int segmentationFieldGroupId)
        {
            string token = await GetToken();

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var response = await client.GetAsync("v1/List/" + listId.ToString().Trim()
                + "/SegmentationFieldGroup/"+ segmentationFieldGroupId.ToString()+ "/SegmentationField");

            if (response.IsSuccessStatusCode)
            {
                string x = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<SegmentationFieldResponse>(x);
                return Ok(data);
            }

            return BadRequest();
        }






        #endregion


    }
}

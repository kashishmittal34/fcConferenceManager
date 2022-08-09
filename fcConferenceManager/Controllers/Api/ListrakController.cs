using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Models;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using System.Configuration;

namespace Controllers
{
    [RoutePrefix("api/Listrak")]
    public class ListrakController : ApiController
    {
        private readonly string clientId = "v8auht0muvoby7xm2r9g";
        private readonly string clientSecret = "N4oO06KETBAfWC4bDIoalb7wxfXArFGG1T9VyD+5AzU";
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
            }

            return string.Empty;
        }

        [HttpGet]
        [Route("Contact/GetContact")]
        public async Task<IHttpActionResult> GetContact([FromUri] string email)
        {
            var listId = ConfigurationManager.AppSettings["ListrakListKey"].ToString();
            string token = await GetToken();
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            var response = await client.GetAsync("v1/List/" + listId.ToString().Trim() + "/Contact/" + email.Trim());
            if (response.IsSuccessStatusCode)
            {
                string x = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ContactsResponse>(x);
                return Ok(data);
            }
            else
            {
                var result = await response.Content.ReadAsAsync<ErrorMessage>();
                return Ok(result);
            }
        }

        [HttpPost]
        [Route("Contact/CreateOrUpdate/{status}")]
        public async Task<IHttpActionResult> ContactCreateOrUpdate([FromUri] string status, [FromUri] string email)
        {
            var listId = ConfigurationManager.AppSettings["ListrakListKey"].ToString();
            string token = await GetToken();
            string updateType = "Update";
            ContactCreateUpdate contact = new ContactCreateUpdate();
            contact.emailAddress = email;
            contact.subscriptionState = status;
            contact.segmentationFieldValues = null;
            var data = JsonConvert.SerializeObject(contact);
            var stringContent = new StringContent(data, Encoding.UTF8, "application/json");
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            string url = "v1/List/" + listId.ToString() + "/Contact";
            url += "?overrideUnsubscribe=true&subscribedByContact=true" + "&sendDoubleOptIn=false&updateType=" + updateType;
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
            }
        }
        [HttpPost]
        [Route("Contact/UpdateContact/{status}")]
        public async Task<IHttpActionResult> UpdateContact([FromUri] string status, [FromUri] string email, [FromUri] string newemail)
        {
            var listId = ConfigurationManager.AppSettings["ListrakListKey"].ToString();
            string token = await GetToken();
            string updateType = "Update";
            ContactCreateUpdate contactOld = new ContactCreateUpdate();
            contactOld.emailAddress = email;
            contactOld.subscriptionState = "Unsubscribed";
            contactOld.segmentationFieldValues = null;

            ContactCreateUpdate contactNew = new ContactCreateUpdate();
            contactNew.emailAddress = newemail;
            contactNew.subscriptionState = status;
            contactNew.segmentationFieldValues = null;

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            string url = "v1/List/" + listId.ToString() + "/Contact";
            url += "?overrideUnsubscribe=true&subscribedByContact=true" + "&sendDoubleOptIn=false&updateType=" + updateType;
            var response = await client.PostAsJsonAsync(url, contactOld);
            if (response.IsSuccessStatusCode)
            {
                var response2 = await client.PostAsJsonAsync(url, contactNew);
                var result = await response2.Content.ReadAsAsync<CommonRESTResponse>();
                return Ok(result);
            }
            else
            {
                var result = await response.Content.ReadAsAsync<ErrorMessage>();
                return Ok(result);
            }
        }
        [HttpPost]
        [Route("Contact/UpdateContact2/{status}")]
        public async Task<IHttpActionResult> UpdateContact2([FromUri] string status, [FromUri] string email, [FromUri] string newemail)
        {
            var listId = ConfigurationManager.AppSettings["ListrakListKey"].ToString();
            string token = await GetToken();
            string updateType = "Update";
            ContactCreateUpdate contact = new ContactCreateUpdate();
            contact.emailAddress = email;
            contact.subscriptionState = status;
            contact.segmentationFieldValues = null;
            var data = JsonConvert.SerializeObject(contact);
            var stringContent = new StringContent(data, Encoding.UTF8, "application/json");
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.listrak.com/email/");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            string url = "v1/List/" + listId.ToString() + "/Contact";
            url += "?overrideUnsubscribe=true&subscribedByContact=true" + "&sendDoubleOptIn=false&updateType=" + updateType;
            url += "&newEmailAddress=" + newemail;
            var response = await client.PostAsJsonAsync(url, data);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<CommonRESTResponse>();
                return Ok(result);
            }
            else
            {
                var result = await response.Content.ReadAsAsync<ErrorMessage>();
                return Ok(result);
            }
        }
    }
}

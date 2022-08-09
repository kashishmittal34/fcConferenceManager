using fcConferenceManager;
using MAGI_API.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Web;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Net;

namespace MAGI_API.Controllers
{
    [RoutePrefix("api/Twilio")]
    public class TwilioController : ApiController
    {
        [HttpGet]
        [Route("Get")]
        public async Task Get()
        {
            string jsonData = await Request.Content.ReadAsStringAsync();
            clsEmail.SendSMTPEmail("gaurav.sharma@keyideasglobal.com", "Twilio Get Message", jsonData);
        }
        [HttpPost]
        [Route("Post")]
        public async Task Post()
        {
            string jsonData = await Request.Content.ReadAsStringAsync();
            try
            {
                //SaveMessage(jsonData);
                //clsEmail.SendSMTPEmail("gaurav.sharma @keyideasglobal.com", "Twilio Post Message", jsonData);
                string MSID = "";
                string strBody = "";
                string Status = "";
                string MobileNum = "";
                string MobileNumFROM = "";
                string MessageSMSID = "";
                string MessageStatus = "";
                string MessageSid = "";
                var formData = HttpUtility.ParseQueryString(jsonData);                
                if (formData["SmsSid"] != null)
                    MSID = formData["SmsSid"].ToString();
                if (formData["SmsStatus"] != null)
                    Status = formData["SmsStatus"].ToString();
                if (formData["Body"] != null)
                    strBody = formData["Body"].ToString();
                if (Status == "received")
                {
                    if (formData["From"] != null)
                    {
                        MobileNum = formData["From"].Replace("%2B", "+").ToString();
                    }
                    if (formData["To"] != null)
                    {
                        MobileNumFROM = formData["To"].Replace("%2B", "+").ToString();
                    }
                }
                else
                {
                    if (formData["To"] != null)
                    {
                        MobileNum = formData["To"].Replace("%2B", "+").ToString();
                    }
                    if (formData["From"] != null)
                    {
                        MobileNumFROM = formData["From"].Replace("%2B", "+").ToString();
                    }
                }
                if (formData["SmsMessageSid"] != null)
                    MessageSMSID = formData["SmsMessageSid"].ToString();


                if (formData["MessageSid"] != null)
                    MessageSid = formData["MessageSid"].ToString();

                if (formData["MessageStatus"] != null)
                    MessageStatus = formData["MessageStatus"].ToString();

                if (MSID == "" && strBody == "" && Status == "")
                {
                    //clsEmail.SendSMTPEmail("gaurav.sharma @keyideasglobal.com", "Twilio Post Message", jsonData);
                }
                else
                {
                    string response = "";

                    string SmsSid = "";
                    string SmsStatus = "";
                    //string MessageStatus = "";
                    string To = "";
                    //string MessageSid = "";
                    string AccountSid = "";
                    string From = "";

                    if (formData["response"] != null)
                    {
                        response = formData["response"];
                    }
                    if (formData["SmsSid"] != null)
                    {
                        SmsSid = formData["SmsSid"];
                    }
                    if (formData["SmsStatus"] != null)
                    {
                        SmsStatus = formData["SmsStatus"];
                    }
                    if (formData["MessageStatus"] != null)
                    {
                        MessageStatus = formData["MessageStatus"];
                    }
                    if (formData["To"] != null)
                    {
                        To = formData["To"];
                    }
                    if (formData["MessageSid"] != null)
                    {
                        MessageSid = formData["MessageSid"];
                    }
                    if (formData["AccountSid"] != null)
                    {
                        AccountSid = formData["AccountSid"];
                    }
                    if (formData["From"] != null)
                    {
                        From = formData["From"];
                    }

                    SaveMessage_WithallParamter(response, SmsSid, SmsStatus, MessageStatus, To, MessageSid, AccountSid, From);
                    clsSMS obj = new clsSMS();
                    obj.ReplySendMessage(strBody, MSID, Status, MobileNum, MessageSMSID, 0, MessageSid, MessageStatus, MobileFROM: MobileNumFROM);
                }
            }
            catch (Exception ex)
            {
                clsEmail.SendSMTPEmail("gaurav.sharma @keyideasglobal.com", "Twilio Post Message", jsonData);                
            }
            finally
            {

            }
            // PostMessage(jsonData);
            //TwiMLResult(jsonData);
        }

        [HttpPost]
        [Route("PostVERIFY")]
        public async Task PostVERIFY()
        {
            string jsonData = await Request.Content.ReadAsStringAsync();
            try
            {
                //SaveMessage(jsonData);
                //clsEmail.SendSMTPEmail("gaurav.sharma @keyideasglobal.com", "Twilio Post Message", jsonData);
                string MSID = "";
                string strBody = "";
                string Status = "";
                string MobileNum = "";
                string MobileNumFROM = "";
                string MessageSMSID = "";
                string MessageStatus = "";
                string MessageSid = "";
                var formData = HttpUtility.ParseQueryString(jsonData);
                if (formData["SmsSid"] != null)
                    MSID = formData["SmsSid"].ToString();
                if (formData["SmsStatus"] != null)
                    Status = formData["SmsStatus"].ToString();
                if (formData["Body"] != null)
                    strBody = formData["Body"].ToString();
                if (Status == "received")
                {
                    if (formData["From"] != null)
                    {
                        MobileNum = formData["From"].Replace("%2B", "+").ToString();
                    }
                    if (formData["To"] != null)
                    {
                        MobileNumFROM = formData["To"].Replace("%2B", "+").ToString();
                    }
                }
                else
                {
                    if (formData["To"] != null)
                    {
                        MobileNum = formData["To"].Replace("%2B", "+").ToString();
                    }
                    if (formData["From"] != null)
                    {
                        MobileNumFROM = formData["From"].Replace("%2B", "+").ToString();
                    }
                }
                if (formData["SmsMessageSid"] != null)
                    MessageSMSID = formData["SmsMessageSid"].ToString();


                if (formData["MessageSid"] != null)
                    MessageSid = formData["MessageSid"].ToString();

                if (formData["MessageStatus"] != null)
                    MessageStatus = formData["MessageStatus"].ToString();
                //if (Request.Content.IsFormData())
                //{
                //    strBody = "Form Data";
                //    NameValueCollection formData = await Request.Content.ReadAsFormDataAsync();
                //    if (formData["SmsSid"] != null)
                //        MSID = formData["SmsSid"].ToString();
                //    if (formData["SmsStatus"] != null)
                //        Status = formData["SmsStatus"].ToString();
                //    if (formData["Body"] != null)
                //        strBody = formData["Body"].ToString();
                //}
                //else
                //{
                //    strBody = "JSON Data";
                //    var details = JObject.Parse(jsonData);
                //    if (details.ContainsKey("SmsSid"))
                //        MSID = details["SmsSid"].ToString();
                //    if (details.ContainsKey("SmsStatus"))
                //        Status = details["SmsStatus"].ToString();
                //    if (details.ContainsKey("Body"))
                //        strBody = details["Body"].ToString();
                //}                  
                if (MSID == "" && strBody == "" && Status == "")
                {
                    //clsEmail.SendSMTPEmail("gaurav.sharma @keyideasglobal.com", "Twilio Post Message", jsonData);
                }
                else
                {
                    clsSMS obj = new clsSMS();
                    obj.VerifyReplySendMessage(strBody, MSID, Status, MobileNum, MessageSMSID, 0, MessageSid, MessageStatus, Mobileform: MobileNumFROM);
                }
            }
            catch (Exception ex)
            {
                clsEmail.SendSMTPEmail("magi@keyideasglobal.com", "SendGrid Message", ex.Message);
            }
            finally
            {

            }
            // PostMessage(jsonData);
            //TwiMLResult(jsonData);
        }

        [HttpPost]
        [Route("Post/{AddedBy}")]
        public async Task Post(string AddedBy)
        {
            string jsonData = await Request.Content.ReadAsStringAsync();
            try
            {
                //SaveMessage(jsonData);
                //clsEmail.SendSMTPEmail("gaurav.sharma @keyideasglobal.com", "Twilio Post Message", jsonData);

                string MSID = "";
                string strBody = "";
                string Status = "";
                string MobileNum = "";
                string MobileNumFROM = "";
                string MessageSMSID = "";
                string MessageStatus = "";
                string MessageSid = "";
                var formData = HttpUtility.ParseQueryString(jsonData);
                if (formData["SmsSid"] != null)
                    MSID = formData["SmsSid"].ToString();
                if (formData["SmsStatus"] != null)
                    Status = formData["SmsStatus"].ToString();
                if (formData["Body"] != null)
                    strBody = formData["Body"].ToString();
                if (Status == "received")
                {
                    if (formData["From"] != null)
                    {
                        MobileNum = formData["From"].Replace("%2B", "+").ToString();
                    }
                    if (formData["To"] != null)
                    {
                        MobileNumFROM = formData["To"].Replace("%2B", "+").ToString();
                    }
                }
                else
                {
                    if (formData["To"] != null)
                    {
                        MobileNum = formData["To"].Replace("%2B", "+").ToString();
                    }
                    if (formData["From"] != null)
                    {
                        MobileNumFROM = formData["From"].Replace("%2B", "+").ToString();
                    }
                }
                if (formData["SmsMessageSid"] != null)
                    MessageSMSID = formData["SmsMessageSid"].ToString();

                if (formData["MessageSid"] != null)
                    MessageSid = formData["MessageSid"].ToString();

                if (formData["MessageStatus"] != null)
                    MessageStatus = formData["MessageStatus"].ToString();

                if (MSID == "" && strBody == "" && Status == "")
                {
                    //clsEmail.SendSMTPEmail("gaurav.sharma @keyideasglobal.com", "Twilio Post Message", jsonData);
                }
                else
                {
                    string response = "";

                    string SmsSid = "";
                    string SmsStatus = "";
                    //string MessageStatus = "";
                    string To = "";
                    //string MessageSid = "";
                    string AccountSid = "";
                    string From = "";

                    if (formData["response"] != null)
                    {
                        response = formData["response"];
                    }
                    if (formData["SmsSid"] != null)
                    {
                        SmsSid = formData["SmsSid"];
                    }
                    if (formData["SmsStatus"] != null)
                    {
                        SmsStatus = formData["SmsStatus"];
                    }
                    if (formData["MessageStatus"] != null)
                    {
                        MessageStatus = formData["MessageStatus"];
                    }
                    if (formData["To"] != null)
                    {
                        To = formData["To"];
                    }
                    if (formData["MessageSid"] != null)
                    {
                        MessageSid = formData["MessageSid"];
                    }
                    if (formData["AccountSid"] != null)
                    {
                        AccountSid = formData["AccountSid"];
                    }
                    if (formData["From"] != null)
                    {
                        From = formData["From"];
                    }

                    //SaveMessage_WithallParamter(response, SmsSid, SmsStatus, MessageStatus, To, MessageSid, AccountSid, From);
                    clsSMS obj = new clsSMS();
                    obj.ReplySendMessage(strBody, MSID, Status, MobileNum, MessageSMSID, 0, MessageSid, MessageStatus, AddedBy, MobileFROM: MobileNumFROM , response);
                }
            }
            catch (Exception ex)
            {
                clsEmail.SendSMTPEmail("gaurav.sharma @keyideasglobal.com", "Twilio Post Message", jsonData);
                //clsEmail.SendSMTPEmail("magi@keyideasglobal.com", "SendGrid Message", ex.Message);
            }
            finally
            {

            }
            // PostMessage(jsonData);
            //TwiMLResult(jsonData);
        }
                               
        public string TwiMLResult(String Body)
        {
            var messagingResponse = new MessagingResponse();
            messagingResponse.Message(Body);

            return TwiMLResult(messagingResponse.ToString());
        }

        [HttpPost]
        [Route("CheckPost")]
        public async Task CheckPost()
        {
            string jsonData = await Request.Content.ReadAsStringAsync();
            try
            {
                clsEmail.SendSMTPEmail("keyideas.global.general@gmail.com", "Twilio Post Message", jsonData);
                var formData = HttpUtility.ParseQueryString(jsonData);
                string response = "";
                
                string SmsSid = "";
                string  SmsStatus = "";
                string MessageStatus = "";
                string To = "";
                string MessageSid = "";
                string AccountSid = "";
                string From = "";

                if (formData["response"] != null)
                {
                    response = formData["response"];
                }
                if (formData["SmsSid"] != null)
                {
                    SmsSid = formData["SmsSid"];
                }
                if (formData["SmsStatus"] != null)
                {
                    SmsStatus = formData["SmsStatus"];
                }
                if (formData["MessageStatus"] != null)
                {
                    MessageStatus = formData["MessageStatus"];
                }
                if (formData["To"] != null)
                {
                    To = formData["To"];
                }
                if (formData["MessageSid"] != null)
                {
                    MessageSid = formData["MessageSid"];
                }
                if (formData["AccountSid"] != null)
                {
                    AccountSid = formData["AccountSid"];
                }
                if (formData["From"] != null)
                {
                    From = formData["From"];
                }
               
                SaveMessage_WithallParamter(response, SmsSid, SmsStatus, MessageStatus, To, MessageSid, AccountSid, From);
                //SaveMessage(jsonData);                
            }
            catch (Exception ex)
            {
                clsEmail.SendSMTPEmail("gaurav.sharma @keyideasglobal.com", "Twilio Post Message", jsonData);
            }
            finally
            {

            }
            // PostMessage(jsonData);
            //return TwiMLResult(string.Empty);
        }

        [HttpPost]
        [Route("CheckPostById/{AddedBy}")]
        public async Task CheckPostById(string AddedBy)
        {
            string jsonData = await Request.Content.ReadAsStringAsync();
            try
            {
                var formData = HttpUtility.ParseQueryString(jsonData);
                string response = "";

                string SmsSid = "";
                string SmsStatus = "";
                string MessageStatus = "";
                string To = "";
                string MessageSid = "";
                string AccountSid = "";
                string From = "";

                if (formData["response"] != null)
                {
                    response = formData["response"];
                }
                if (formData["SmsSid"] != null)
                {
                    SmsSid = formData["SmsSid"];
                }
                if (formData["SmsStatus"] != null)
                {
                    SmsStatus = formData["SmsStatus"];
                }
                if (formData["MessageStatus"] != null)
                {
                    MessageStatus = formData["MessageStatus"];
                }
                if (formData["To"] != null)
                {
                    To = formData["To"];
                }
                if (formData["MessageSid"] != null)
                {
                    MessageSid = formData["MessageSid"];
                }
                if (formData["AccountSid"] != null)
                {
                    AccountSid = formData["AccountSid"];
                }
                if (formData["From"] != null)
                {
                    From = formData["From"];
                }

                SaveMessage_WithallParamter(response, SmsSid, SmsStatus, MessageStatus, To, MessageSid, AccountSid, From);
                //SaveMessage(jsonData);                
            }
            catch (Exception ex)
            {
                clsEmail.SendSMTPEmail("gaurav.sharma @keyideasglobal.com", "Twilio Post Message", AddedBy + " - " + jsonData);
            }
            finally
            {

            }
            // PostMessage(jsonData);
            //TwiMLResult(jsonData);
        }

        private void SaveMessage_WithallParamter(string message, string SmsSid, string SmsStatus, string MessageStatus, string To, string MessageSid, string AccountSid, string From)
        {
            var myConnection = clsUtility.SqlConnection();
            int rows;
            SqlCommand myCommand = myConnection.CreateCommand();
            DateTime dateTime = DateTime.Now;
            try
            {
                // Dim query As String = "Update email_list set LatestStatus = @LatestStatus, LastUpdated=@LastUpdated where Email = @Email and MessageID=@MessageID; "
                string query = "";
                query = query + "EXEC textsms_response_Save @response	,@sendDate	,	@SmsSid,	@SmsStatus	,@MessageStatus	,@To,	@MessageSid	,@AccountSid	,@From";
                myConnection.Open();
                myCommand.CommandText = query;
                myCommand.Parameters.AddWithValue("@response", message);
                myCommand.Parameters.AddWithValue("@sendDate", dateTime);
                myCommand.Parameters.AddWithValue("@SmsSid", SmsSid);
                myCommand.Parameters.AddWithValue("@SmsStatus", SmsStatus);
                myCommand.Parameters.AddWithValue("@MessageStatus", MessageStatus);
                myCommand.Parameters.AddWithValue("@To", To);
                myCommand.Parameters.AddWithValue("@MessageSid", MessageSid);
                myCommand.Parameters.AddWithValue("@AccountSid", AccountSid);
                myCommand.Parameters.AddWithValue("@From", From);
              
                rows = myCommand.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                SendSMTPEmail("magi@keyideasglobal.com", "SendGrid Message", ex.Message);
            }
            finally
            {
                myConnection.Close();
            }
        }
        private void SaveMessage(string message )//--, string SmsSid ,string SmsStatus ,string MessageStatus , string To ,string MessageSid ,string AccountSid ,string From)
        {
            var myConnection = clsUtility.SqlConnection();
            int rows;
            SqlCommand myCommand = myConnection.CreateCommand();
            DateTime dateTime = DateTime.Now;
            try
            {
                // Dim query As String = "Update email_list set LatestStatus = @LatestStatus, LastUpdated=@LastUpdated where Email = @Email and MessageID=@MessageID; "
                string query = "";
                query = query + "Insert into sys_textsms_response(response,sendDate)values(@response,@sendDate);";
                myConnection.Open();
                myCommand.CommandText = query;
                myCommand.Parameters.AddWithValue("@response", message);
                myCommand.Parameters.AddWithValue("@sendDate", dateTime);               
                rows = myCommand.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                SendSMTPEmail("magi@keyideasglobal.com", "SendGrid Message", ex.Message);
            }
            finally
            {
                myConnection.Close();
            }
        }

        private bool SendSMTPEmail(string strTo, string strSubject, string strBody)
        {
            bool SentSMTPEmail = false;
            Label lblMsg = new Label();
            // --create message
            MailMessage mm = new MailMessage();
            mm.From = new MailAddress("kwsConnector@gmail.com");
            mm.Subject = strSubject;
            mm.Body = strBody;
            mm.IsBodyHtml = true;

            mm.To.Add(new MailAddress(strTo));

            // --send it
            SmtpClient smtp = new SmtpClient();
            try
            {
                // SMTP Server
                smtp.Host = "smtp.gmail.com";

                // SSL Settings depending on the Server
                smtp.EnableSsl = true;

                // Credentials for the Server
                NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                NetworkCred.UserName = "kwsConnector@gmail.com";
                NetworkCred.Password = "Atbuck3385!";
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;

                // Port No of the Server (587 for Gmail)
                smtp.Port = 587;
                smtp.Send(mm);

                SentSMTPEmail = true;
            }
            catch (Exception ex)
            {
                clsUtility.LogErrorMessage(lblMsg, null/* TODO Change to default(_) if this is not a reference type */, this.GetType().Name, 0, "Error sending SMTP email.");
            }
            finally
            {
                smtp = null;
                mm = null;
            }
            return SentSMTPEmail;
        }

        //public class TwilioMessagingRequest
        //{
        //    public string Body { get; set; }
        //}

        //public class TwilioVoiceRequest
        //{
        //    public string From { get; set; }
        //}
        //public HttpResponseMessage PostMessage([FromBody] TwilioMessagingRequest messagingRequest)
        //{
        //    var message =
        //        $"Your text to me was {messagingRequest.Body.Length} characters long. " +
        //        "Webhooks are neat :)";
        //    var response = new MessagingResponse();


        //    return ToResponseMessage(response.ToString());
        //}
        //private static HttpResponseMessage ToResponseMessage(string response)
        //{
        //    return new HttpResponseMessage
        //    {
        //        Content = new StringContent(response, Encoding.UTF8, "application/xml")
        //    };
        //}

    }

}
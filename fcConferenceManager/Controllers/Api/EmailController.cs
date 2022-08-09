using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Http;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using fcConferenceManager;
using Newtonsoft.Json.Linq;

namespace Controllers
{
    public class EmailController : ApiController
    {
        public string GetValue(int id)
        {
            return "value";
        }
        public void PostValue([FromBody()] string value)
        {
        }
        [HttpGet]
        public string Updating()
        {
            var result = "Not Updated";
            HttpContext.Current.Application.Lock();
            clsSettings cSettings;
            if (HttpContext.Current.Application["cSettings"] != null)
                cSettings = (clsSettings)HttpContext.Current.Application["cSettings"];
            else
                cSettings = new clsSettings();
            var sqlConn = clsUtility.SqlConnection();
            cSettings.LoadSettings(sqlConn.ConnectionString);
            cSettings.WebsiteUpdateTime = DateTime.Now;
            cSettings.WebsiteUpdating = true;
            HttpContext.Current.Application["cSettings"] = cSettings;
            HttpContext.Current.Application["IsOffline"] = false;
            HttpContext.Current.Application.UnLock();
            result = "Application Updated";
            return result;
        }
        [HttpGet]
        public string Offline()
        {
            var result = "Not Offline";
            if (HttpContext.Current.Application["cSettings"] != null)
            {
                HttpContext.Current.Application.Lock();
                clsSettings cSettings = (clsSettings)HttpContext.Current.Application["cSettings"];
                cSettings.WebsiteUpdateTime = DateTime.Now.AddMinutes(-15);
                cSettings.WebsiteUpdating = true;
                HttpContext.Current.Application["cSettings"] = cSettings;
                HttpContext.Current.Application["IsOffline"] = true;
                HttpContext.Current.Application.UnLock();
                result = "Application Offline";
            }
            return result;
        }
        [HttpGet]
        public string Refresh()
        {
            var result = "Not Refresh";
            if (HttpContext.Current.Application["cSettings"] != null)
            {
                HttpContext.Current.Application.Lock();
                clsSettings cSettings = (clsSettings)HttpContext.Current.Application["cSettings"];
                cSettings.WebsiteUpdateTime = DateTime.MinValue;
                cSettings.WebsiteUpdating = false;
                HttpContext.Current.Application["cSettings"] = cSettings;
                HttpContext.Current.Application["IsOffline"] = false;
                HttpContext.Current.Application.UnLock();
                result = "Application Refresh";
            }
            return result;
        }

        public async Task Post()
        {
            string jsonData = await Request.Content.ReadAsStringAsync();
            try
            {
                JArray dataList = JArray.Parse(jsonData);
                foreach (var rec in dataList)
                {
                    string eventName = System.Convert.ToString(rec["event"]) ?? "".ToLower();
                    string email = System.Convert.ToString(rec["email"]) ?? "";
                    // string category = System.Convert.ToString(rec["category"]) ?? "";
                    // string smtpid = System.Convert.ToString(rec["smtp-id"]) ?? "";
                    // string eventid = System.Convert.ToString(rec["sg_event_id"]) ?? "";
                    string messageid = System.Convert.ToString(rec["sg_message_id"]) ?? "";
                    // Int64 timestamp = 0;
                    Int64 timestamp = Int64.Parse(System.Convert.ToString(rec["timestamp"]) ?? "0");
                    int intEvent = 1;
                    if (eventName.IndexOf("process") > -1)
                        intEvent = 2;
                    else if (eventName.IndexOf("drop") > -1)
                        intEvent = 3;
                    else if (eventName.IndexOf("defer") > -1)
                        intEvent = 4;
                    else if (eventName.IndexOf("deliver") > -1)
                        intEvent = 5;
                    else if (eventName.IndexOf("bounce") > -1)
                        intEvent = 6;
                    else if (eventName.IndexOf("open") > -1)
                        intEvent = 7;
                    else if (eventName.IndexOf("click") > -1)
                        intEvent = 8;
                    DateTimeOffset dtOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                    DateTime dateTime = dtOffset.UtcDateTime;

                    if (messageid.Length > 22)
                        messageid = messageid.Substring(0, 22);

                    var myConnection = clsUtility.SqlConnection();
                    int rows;
                    SqlCommand myCommand = myConnection.CreateCommand();

                    try
                    {
                        // Dim query As String = "Update email_list set LatestStatus = @LatestStatus, LastUpdated=@LastUpdated where Email = @Email and MessageID=@MessageID; "
                        string query = "";
                        query = query + "Insert into email_list_status(Email, Name, MessageID, LatestStatus, LastUpdated)values(@Email, '', @MessageID, @LatestStatus, @LastUpdated);";
                        myConnection.Open();
                        myCommand.CommandText = query;
                        myCommand.Parameters.AddWithValue("@Email", email);
                        myCommand.Parameters.AddWithValue("@MessageID", messageid);
                        myCommand.Parameters.AddWithValue("@LastUpdated", dateTime);
                        myCommand.Parameters.AddWithValue("@LatestStatus", intEvent);
                        rows = myCommand.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        SendSMTPEmail("magi@keyideasglobal.com", "SendGrid Message", ex.Message +"___"+ jsonData.ToString());
                    }
                    finally
                    {
                        myConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                SendSMTPEmail("magi@keyideasglobal.com", "SendGrid Message", ex.Message + "___" + jsonData.ToString());
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
    }
}

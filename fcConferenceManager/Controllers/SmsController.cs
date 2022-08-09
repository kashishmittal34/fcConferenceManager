
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Twilio.Rest.Api.V2010.Account;
using System;
using System.Web.Mvc;
using Twilio;

using MessageResource = Twilio.Rest.Conversations.V1.Conversation.MessageResource;
using Twilio.Rest.Conversations.V1;

namespace fcConferenceManager.Controllers
{
    public class SmsController :Controller
    {
        string accountSid = "ACe7c904843a26c5ed14f8de553efe2a17";
        string authToken = "9232e20de513c7fe99710a1ae566bfe9";
        public ActionResult SendSms()
        {

            TwilioClient.Init(accountSid, authToken);
            var conversation = Twilio.Rest.Conversations.V1.ConversationResource.Create(
                friendlyName: "My First Chat Conversition"
            );
            AddParticeipent(conversation.Sid);
            return Content(conversation.Sid);
        }
        public void AddParticeipent(string sid)
        {

            TwilioClient.Init(accountSid, authToken);

            var participant = Twilio.Rest.Conversations.V1.Conversation.ParticipantResource.Create(
                messagingBindingAddress: "",
                messagingBindingProxyAddress: "",
                pathConversationSid: sid
            );
            CreateConversationMessage(sid);

        }
        public void CreateConversationMessage(string sid)
        {
      
            TwilioClient.Init(accountSid, authToken);

            var message = MessageResource.Create(
                author: "Gaurav",
                body: "",
                pathConversationSid: sid
            );

            Console.WriteLine(message.Sid);
            ConversionParticipentChat(sid);
        }
        public void ConversionParticipentChat(string sid)
        {
            string accountSid = "ACe7c904843a26c5ed14f8de553efe2a17";
            string authToken = "9232e20de513c7fe99710a1ae566bfe9";

            TwilioClient.Init(accountSid, authToken);

            var participant = Twilio.Rest.Conversations.V1.Conversation.ParticipantResource.Create(
                identity: "testPineapple",
                pathConversationSid: sid
            );

            Console.WriteLine(participant.Sid);

        }

        public void GetConversitrionMessages(string sid)
        {
   
            TwilioClient.Init(accountSid, authToken);
            //ConversationResource.Delete(pathSid: sid);
            var conversation = Twilio.Rest.Conversations.V1.ConversationResource.Fetch(
                pathSid: sid
            );

            Console.WriteLine(conversation.FriendlyName);

        }
        public void GetWebHookData()
        {
           
            TwilioClient.Init(accountSid, authToken);
            var webhook = WebhookResource.Fetch();
            Console.WriteLine(webhook.Method);
            clsEmail.SendSMTPEmail("gaurav.sharma@keyideasglobal.com", "Twilio Get Message", webhook.PostWebhookUrl.ToString());

        }
    }
}
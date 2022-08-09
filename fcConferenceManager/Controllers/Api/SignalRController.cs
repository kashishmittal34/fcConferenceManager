using fcConferenceManager;
using MAGI_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using MAGI_API.SignalR;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using System;

namespace MAGI_API.Controllers
{
    [RoutePrefix("api/SignalR")]
    public class SignalRController : ApiController
    {
        //[HttpGet]
        //[Route("Get")]
        //public async Task<IHttpActionResult> Get()
        //{
        //}

        [HttpPost]
        [Route("Post/{receiverId}/{senderId}/{senderName}/{nick}/{message}")]
        public async Task<IHttpActionResult> Post(string receiverId, string senderId, string senderName,string nick, string message)
        {            
            string groupName = "";
            string receiverName = "";
            int chatType = 1;
            int eventKey = 58;
            Guid guid = Guid.NewGuid();
            string time = DateTime.Now.ToLocalTime().ToString("H:mm tt");
            //string strText = getMessage(guid, senderId, senderName, time, message);
            string imgUrl = "/accountimages/" + senderId + "_img.jpg";
            Talk talk = new Talk();
            talk.ak = false;            
            talk.msgSt = 0;
            talk.mid = Guid.NewGuid().ToString();
            talk.strMsg = message;            
            ChatOperations.add(senderId, receiverId, chatType, senderName, talk, receiverName, eventKey, true);
            var _hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<ChatHub>();           
            await _hubContext.Clients.Group(receiverId).sendAsync(senderId, senderName, imgUrl, message, chatType, guid, groupName);
            return Ok();
        }

        private string getMessage(Guid guid, string senderId, string senderName, string time, string msg)
        {
            string pText = "<li style=\"width:100%;\">" +
                "<div class=\"msj-rta macro\">" +
                "<div style=\"display:inline-block;float:left;width:10%;\">" +
                string.Format("<img src=\"/accountimages/{0}_img.jpg?{1}\" class=\"clsAvatarRv\" onerror=\"this.onerror=null;this.src='/Images/Conferences/avatar.jpg';\"/>", senderId, guid.ToString()) +
                "</div>" +
                "<div class=\"text text-r\">" +
                string.Format("<p>{0}</p>", senderName) +
                string.Format("<p>Hi</p>", msg) +
                "<p>" +
                string.Format("<b id=\"{0}\" style=\"font-size: 11px;margin-right: 10px;\"></b>", guid.ToString()) +
                string.Format("<small style=\"font-size: 10px;\">{0}</small>", time) +
                "</p>" +
                "</div>" +
                "</li>";
            return pText;
        }
    }
}
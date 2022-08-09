using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace fcConferenceManager.Models.ViewModels
{
    public class ChatViewModel
    {
        public ChatViewModel()
        {
            NetworkingLevelDetails = new string[4];
        }

        public int MyID { get; set; }
        public int EventID { get; set; }
        public int ActiveEventID { get; set; }
        public DateTime LastDateOfEvent { get; set; }
        public string strActiveEventName { get; set; }
        public bool IsReceiverPartner { get; set; }
        public string MyNickName { get; set; }
        public string MyFirstName { get; set; }
        public string MyOrganization { get; set; }
        public int Organization_key { get; set; }
        public int EventOrganizationkey { get; set; }
        public bool IsDemo { get; set; }
        public string StandardTimeCode { get; set; }
        public decimal TimeZoneDiff { get; set; }
        public string Contactname { get; set; }
        public string InterestBasedGroups { get; set; }
        public Dictionary<int, string> ChatTypes { get; set; }
        public DataSet PanelSet { get; set; }
        public bool zoomSessionSilentMode { get; set; }
        public bool ChatEnabled { get; set; }
        public string[] NetworkingLevelDetails { get; set; }
    }
}
using System;

namespace Models
{
    public class Message
    {
        public int messageId { get; set; }
        public DateTime sendDate { get; set; }
        public int campaignId { get; set; }
        public string subject { get; set; }
    }
}

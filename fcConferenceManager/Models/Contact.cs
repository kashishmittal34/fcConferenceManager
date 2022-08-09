using System;
using System.Collections.Generic;

namespace Models
{
    public class Contact
    {
        public string emailAddress { get; set; }
        public string emailKey { get; set; }
        public string subscriptionState { get; set; }
        public DateTime? subscribeDate { get; set; }
        public string subscribeMethod { get; set; }
        public DateTime? unsubscribeDate { get; set; }
        public string unsubscribeMethod { get; set; }
        public List<string> segmentationFieldValues { get; set; }
    }
}

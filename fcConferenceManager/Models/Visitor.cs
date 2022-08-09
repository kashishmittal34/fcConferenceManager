using System;
using System.Collections.Generic;

namespace fcConferenceManager.Models
{
    public class Visitor
    {
        public string broswerName { get; set; }
        public string ipAddress { get; set; }
        public bool isLoggedIn { get; set; }       
        public string userID { get; set; }
        public string pageURL { get; set; }       
    }
}

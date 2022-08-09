using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace fcConferenceManager.Models.Portolo
{
    public class Study
    {
        public int pkey { get; set; }
        public int UserId { get; set; }
        public string FileTitle { get; set; }
        public string FileDescription { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
}
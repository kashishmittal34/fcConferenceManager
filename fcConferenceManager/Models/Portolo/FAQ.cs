﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace fcConferenceManager.Models.Portolo
{
    public class FAQ
    {
        public int FAQId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public bool IsActive { get; set; }
        public string category { get; set; }

    }
}
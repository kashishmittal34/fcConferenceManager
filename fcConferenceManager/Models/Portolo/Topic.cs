﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace fcConferenceManager.Models.Portolo
{
    public class Topic
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public bool isActive { get; set; }

    }
}
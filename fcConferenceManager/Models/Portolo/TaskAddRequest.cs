using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Elimar.Models
{
    public class TaskAdd
    {
        public int pkey { get; set; }
        public string title { get; set; }        
        public string description { get; set; }
        public string category { get; set; }
        public string status { get; set; }
        public bool reviewed { get; set; }
        public bool active { get; set; }        
        public string  plandate { get; set; }
        public string   duedate { get; set; }
        public string forecast { get; set; }
        public string repeat { get; set; }
        public string editprimarykey { get; set; }       
        public string TaskCategory_pKey { get; set; }
        public string RepeatType_pKey { get; set; }
        public string formsubmit { get; set; }

        public string Tips { get; set; }
        public string Instruction { get; set; }
        public string Notes { get; set; }
        public string Resources { get; set; }
        public string ResourcesFileName { get; set; }
							

        public Commondropdownlist commondropdownlist;

        public TaskListResponse taskListResponse;
        public HttpPostedFileBase[] files { get; set; }
       

    }
    public class Commondropdownlist
    {

        public List<categorydropdown> categorydropdowns;

        public List<statusdropdown> statusdropdowns;

        public List<repeatdropdown> repeatdropdowns;

    }
    public class categorydropdown
    {
        public string TaskCategoryID { get; set; }
        public int pKey { get; set; }
    }
    public class statusdropdown
    {
        public string TaskStatusID { get; set; }

        public int pKey { get; set; }

    }
    public class repeatdropdown
    {
        public string TaskRepeatID { get; set; }
        public int pKey { get; set; }
    }

}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Elimar.Models
{
    public class TaskListRequest
    {

        public int pKey { get; set; }
        public string plandates { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public string duedate { get; set; }

        public string forecast { get; set; }
        public string number { get; set; }
        public string title { get; set; }
        public int status { get; set; }
        public int intstatus { get; set; }
        public string category { get; set; }
        public string active { get; set; }
        public string reviewed { get; set; }
        public string tasklistrange { get; set; }
        public string repeat { get; set; }
        public string MostrecentNotes { get; set; }
        public int editprimarykey { get; set; }
        public int intRepeat { get; set; }
        public int intPortoloRepeat { get; set; }
        public int planType { get; set; }
        public int intcategory {get;set;}
        public string Tips { get; set; }
        public string Instruction { get; set; }
        public string Notes { get; set; }
        public string Resources { get; set; }
        public string ResourcesFileName { get; set; }
        public CommomDropdown TaskList { get; set; }
        public Commondropdownlist commondropdownlist;

    }

    public class CommomDropdown
    {
        public int pkey { get; set; }
        public string strtext { get; set; }

    }




    }
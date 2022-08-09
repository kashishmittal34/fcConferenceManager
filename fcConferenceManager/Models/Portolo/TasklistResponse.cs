using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Elimar.Models
{
    public class TaskListResponse
    {   
        
        public string pKey { get; set; }
        public string number { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string description { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? plan { get; set; }

        [DataType(DataType.Date)]

        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}", ApplyFormatInEditMode = true)]
        [RegularExpression(@"(((0|1)[0-9]|2[0-9]|3[0-1])\/(0[1-9]|1[0-2])\/((19|20)\d\d))$", ErrorMessage = "Invalid date format.")]
        public DateTime? duedate { get; set; }

        [DataType(DataType.Date)]

        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? forecast { get; set; }
        public string TaskCategoryID { get; set; }
        public int status { get; set; }

        public string mostrecentnote { get; set; }         
        public int  TaskCategory_pKey { get; set; }
        public int RepeatType_pKey { get; set; }
        public string TaskStatusID { get; set; }
        public string TaskRepeatID { get; set; }
        public bool active { get; set; }
        public int intcategory { get; set; }
        public bool reviewed { get; set; }
        public int editprimarykey { get; set; }
        public string Tips { get; set; }
        public string Instruction { get; set; }
        public string Notes { get; set; }
        public string Resources { get; set; }
        public HttpPostedFile Files { get; set; }
        public string ResourcesFileName { get; set; }
		public List<PublicTaskResource> publicTaskResources;
    } 

    public class PublicTaskResource
    {
        
        public int pkey { get; set; }
        public int PublicTaskID { get; set; }
        public string ResourcesFileName { get; set; }
        public string Resources { get; set; }
    }


}
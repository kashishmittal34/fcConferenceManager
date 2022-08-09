using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Elimar.Models
{
    public class ElimarResponse

    {
        public string  Form { get; set; }
        public string AppTextBlock { get; set; }
        public int PKey { get; set; }

        public Boolean Active { get; set; }
        public DateTime UpdatedDate { get; set; }






    }

}
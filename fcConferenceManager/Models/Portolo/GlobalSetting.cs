using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace fcConferenceManager.Models.Portolo
{
    public class GlobalSetting
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "Setting")]
        public string Setting { get; set; }

        [Display(Name = "Value")]
        public string Value { get; set; }



    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace fcConferenceManager.Models.Portolo
{
    public class RegistrationSetting
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "Registration Type")]
        public string RegistrationLevelID { get; set; }

        [Display(Name = "Networking")]
        public int Networking { get; set; }

        [Display(Name = "Coupons")]
        public string Coupons { get; set; }
    }
}
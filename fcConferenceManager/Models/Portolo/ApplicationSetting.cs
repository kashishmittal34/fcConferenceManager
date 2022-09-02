
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace fcConferenceManager.Models.Portolo
{
    public class ApplicationSetting
    {     
        public string pkey { get; set; }
        public int Id { get; set; }
        public string SettingID { get; set; }
        [AllowHtml]
        [Display(Name = "SettingValue")]
        public string SettingValue { get; set; }

    }
   
    public class ApplicationSettingViewModel
    {
        public string AccountImg { get; set; }
        public string OrganizationImg { get; set; }

        public const int OrganizationImgPkey = 1;

        public const int AccountImagePkey = 2;

        public IList<ApplicationSetting> SettingList { get; set; }
    }
    public class PublicContentPage
    {
        public string AboutUs { get; set; }
        public string TermsOfUse { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace fcConferenceManager.Models.Portolo
{
    public class ApplicationSetting
    {
        public int pkey { get; set; }
        public string SettingID { get; set; }
        public string SettingValue { get; set; }
        
    }

    public class ApplicationSettingViewModel
    {
        public string AccountImg { get; set; }
        public string AccountImgUrl { get; set; }
        public string OrganizationImg { get; set; }
        public string OrganizationImgUrl { get; set; }
        public IList<ApplicationSetting> SettingList { get; set; }
    }
}
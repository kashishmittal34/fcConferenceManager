using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace fcConferenceManager.Models.Portolo
{

    public class MasterTable
    {
        public IEnumerable<LookUp> LookUps { get; set; }
        public IEnumerable<GlobalSetting> GlobalSettings { get; set; }
        public IEnumerable<RegistrationSetting> RegistrationSettings { get; set; }

        public IEnumerable<EventRole> EventRoles { get; set; }
    }
}
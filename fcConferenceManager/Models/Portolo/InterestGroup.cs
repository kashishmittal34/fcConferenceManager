using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;

namespace Elimar.Models
{ 
    public class InterestGroup
    {
        [Key]
        public int PKey { get; set; }

        public string GroupName { get; set; }

        public int NoOfMembers { get; set; }

        public UserRequest MemberInfo { get; set; }
    }
}
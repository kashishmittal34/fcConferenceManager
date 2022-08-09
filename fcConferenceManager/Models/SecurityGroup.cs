using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;

namespace Elimar.Models
{
    public class SecurityGroup
    {
        public int SecurtiyGroupPkey { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public List<Component> ComponentList { get; set; }

        public List<SecurityGroupMember> members { get; set; }

    }
    public class Component
    {
        public int ComponentPkey { get; set; }

        public string ComponentName { get; set; }

        public int memberPkey { get; set; }
        public bool AllowView { get; set; }

        public bool AllowAdd { get; set; }

        public bool AllowEdit { get; set; }
        public bool AllowDelete { get; set; }

    }
    public class SecurityGroupMember
    {
        public int AccountID { get; set; }

        public string AccountName { get; set; }

        public bool Activated { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;

namespace Elimar.Models
{ 
    public class Help
    {
        [Key]
        public int PKey { get; set; }

        public string Name { get; set; }

        public string Telephone { get; set; }

        public string Subject { get; set; }

        public string Email { get; set; }

        public string Discription { get; set; }

        public string reply { get; set; }

    }
}
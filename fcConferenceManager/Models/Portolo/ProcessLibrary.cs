using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;

namespace fcConferenceManager.Models.Portolo
{
    public class ProcessLibrary
    {
         public int pkey { get; set; }
         [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Only alphabets!")]
         public string Process { get; set; }
        public IEnumerable<ProcessLibrary>    processList { get; set; }
    }
 }

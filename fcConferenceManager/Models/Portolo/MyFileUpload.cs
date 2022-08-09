
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace fcConferenceManager.Models.Portolo
{
    public class MyFileUpload
    {
        public int FileId { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public IEnumerable<MyFileUpload> FileList { get; set; }
    }
}
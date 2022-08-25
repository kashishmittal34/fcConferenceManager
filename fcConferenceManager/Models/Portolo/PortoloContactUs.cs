
using Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace fcConferenceManager.Models.Portolo
{
    public class PortoloContactUs
    {
        public int Id { get; set; }
        public string Role { get; set; }
		[MaxLength(100, ErrorMessage = "Maximum Length is 100")]
		public string Name { get; set; }
	
		[RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail id is not valid")]
		public string Email { get; set; }
		[DataType(DataType.PhoneNumber)]		
		[MaxLength(10, ErrorMessage = "Phone Number is 10 digits only")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Entered phone format is not valid.")]
		public  string Phone { get; set; }
        public string Title { get; set; }
        public string Department { get; set; } = "";
        public string SecurityGroup { get; set; } = "";
        public string ImageSrc { get; set; } = string.Empty;
        public HttpPostedFileBase ProfileImage { get; set; }
        public IList<PortoloContactUs> ContactList { get; set; } 
    }
}
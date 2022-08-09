using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Elimar.Models
{
    public class Login
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }

    public class loginResponse
    {
        public int Id { get; set; }
        public string salutation1 { get; set; }
        public string firstname { get; set; }
        public string middlename { get; set; }
        public string lastname { get; set; }
        public string mainemail { get; set; }
        public string status { get; set; }
        public string country { get; set; }
        public string phone1 { get; set; }
        public string skypeaddress { get; set; }
        public string city { get; set; }
        public string zipcode { get; set; }
        public string jobTitle { get; set; }
        public string organization { get; set; }
        public string department { get; set; }
		public string personalbiography { get; set; }											 
        public string Uimg { get; set; }
		public string degrees { get; set; }								   
    }

    public class UserModel
    {
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is required")]

        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class UserRequest
    {
        public int ID { get; set; }
        public string salutation1 { get; set; }
        public string firstname { get; set; }
        public string middlename { get; set; }
        public string lastname { get; set; }
        public string suffix { get; set; }
        public string nickname { get; set; }
        public string signinaccountid { get; set; }
        public string MainEmailType { get; set; }
        public string mainemail { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string SendEmailTo { get; set; }
        public string skypeaddress { get; set; }
        public string linkedinURL { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string name { get; set; }
        public string zipcode { get; set; }
        public string State { get; set; }
        public string timezone { get; set; }
        public string countrycode { get; set; }
        public string phonenumber { get; set; }
        public string extension { get; set; }
        public string email { get; set; }
        public string jobTitle { get; set; }
        public string department { get; set; }
        public string organization { get; set; }
        public string website { get; set; }
        public string degreesandcertifications { get; set; }
        public string personalbiography { get; set; }
        public string aboutmyorganizationandmyrole { get; set; }
        public string salutation2 { get; set; }
        public string phonetype1 { get; set; }
        public string phonetype2 { get; set; }
        public string phone1 { get; set; }
        public string phone1extension { get; set; }
        public string phone2 { get; set; }
        public string phone2extension { get; set; }
        public string countryCodephone1 { get; set; }
        public string countryCodephone2 { get; set; }

        [RegularExpression(@"([a-zA-Z0-9\s_\\.\-:])+(.png|.jpg|.gif)$", ErrorMessage = "Only Image files allowed.")]
        public string Uimg { get; set; }
        public string CV { get; set; }

        

    }

    public class UserResponse
    {
        public string status { get; set; }
        public int ID { get; set; }
        public string salutation1 { get; set; }
        public string firstname { get; set; }
        public string middlename { get; set; }
        public string lastname { get; set; }
        public string suffix { get; set; }
        public string nickname { get; set; }
        public string signinaccountid { get; set; }
        public string MainEmailType { get; set; }
        public string mainemail { get; set; }
        public string Password { get; set; }
        public string sendemailto { get; set; }
        public string skypeaddress { get; set; }
        public string linkedinURL { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string name { get; set; }
        public string zipcode { get; set; }
        public string State { get; set; }
        public string timezone { get; set; }
        public string countrycode { get; set; }
        public string phonenumber { get; set; }
        public string extension { get; set; }
        public string email { get; set; }
        public string jobTitle { get; set; }
        public string department { get; set; }
        public string organization { get; set; }
        public string website { get; set; }
        public string degreesandcertifications { get; set; }
        public string personalbiography { get; set; }
        public string aboutmyorganizationandmyrole { get; set; }
        public string salutation2 { get; set; }
        public string phonetype1 { get; set; }
        public string phonetype2 { get; set; }
        public string phone1 { get; set; }
        public string phone1extension { get; set; }
        public string phone2 { get; set; }
        public string phone2extension { get; set; }
        public string countryCodephone1 { get; set; }
        public string countryCodephone2 { get; set; }

        public string portoloStatus { get; set; }

        public string Uimg { get; set; }
        public string CV { get; set; }

        public string countrypkey { get; set; } 

        public string state_pkey { get; set; }

        public string salutationzID1 { get; set; }

    }

    public class getdropdownvalues
    {
        public int pkey { get; set; }
        public string portolostatus { get; set; }
    }
	 public class MyOrganisation
    {
        public int id { get; set; }
        public string organizationName { get; set; }
        public string Type { get; set; }
        public string url { get; set; }
        public string parentOrganization { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string country { get; set; }
        public string userName { get; set; }
        public string title { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public int intTimezone_pKey { get; set; }
        public string strEmail1 { get; set; }
        public string strEmail2 { get; set; }

    }						   
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace fcConferenceManager.Models
{
    

    public class Identitylogger
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }

        //[Required(ErrorMessage = "Required.")]
        //[EmailAddress(ErrorMessage = "Invalid email address.")]
        [Required]
        [Display(Name = "Account or Email:")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
        public bool EmailConfirmed { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        // public string State { get; set; }
        public int StateId { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        //  public string Country { get; set; }
        public int CountryId { get; set; }
        public string EventId { get; set; }
        public string RoleId { get; set; }
        public string CustomerId { get; set; }
        public string EventName { get; set; }
        public string EventTypeId { get; set; }
        public IList<string> Roles { get; set; }
        //public IList<Claim> Claims { get; set; }

        public string EventAccount_pkey { get; set; }
        public string ParticipationStatus_pKey { get; set; }
    }

    public class IdentityRole : Microsoft.AspNet.Identity.IRole<string>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
    }

    public class IdentityCustomer : Microsoft.AspNet.Identity.IUser<string>
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Status { get; set; }
    }

    public class Result
    {
        public bool Succeeded { get; set; }
        public string Error { get; set; }
    }

}
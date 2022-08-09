using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace MAGI_API.Security
{
    public class IdentityUser
    {
        public int IntResult { get; set; }
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
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
        public string Region { get; set; }
        public string RegionCode { get; set; }
        public string TimeOffset { get; set; }
        public IList<string> Roles { get; set; }
        public IList<Claim> Claims { get; set; }
        public string EventAccount_pkey { get; set; }
        public string ParticipationStatus_pKey { get; set; }
        public string ISEventFeedbackResponse { get; set; }
        public DateTime EventStartDate { get; set; }
        public DateTime EventEndDate { get; set; }
        public string LeftBlockImage { get; set; }

        public string ApploginforParticipents { get; set; }
        public string StaffMember { get; set; }
        public string LocationTimeInterval { get; set; }
        public string RegistrationLevel_Pkey { get; set; }
        public Boolean IsLicenseNumber { get; set; }
        public Boolean IsSpeaker { get; set; }
        

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

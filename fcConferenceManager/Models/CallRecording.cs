using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MAGI_API.Models
{
	public class VerifyURL
    {
        public string URL { get; set; }
    }

    public class IssueItem
    {
        public int Issue_pKey { get; set; }
        public int Account_pKey { get; set; }
        public int IssueDeveloper_pKey { get; set; }
        public int IssueStatus_pKey { get; set; }
        public int Duration { get; set; }
        public bool InProgress { get; set; }
        public string ItemTitle { get; set; }
        public string ItemDate { get; set; }
        public string ItemTime { get; set; }        
    }
    public class CallRecording
    {
        public int Account_pkey { get; set; }
        public int Event_pkey { get; set; }
        public int Addedby { get; set; }
        public string PhoneNo { get; set; }
        public string CallType { get; set; }
        //public DateTime CallDateTime { get; set; }
        ////public int Account_pkey { get; set; }
        //public DateTime  AddedOn { get; set; }
    }
    public class Call_List
    {
        public string Account_pkey { get; set; }
        public string Event_pkey { get; set; }
        public string Addedby { get; set; }
        public string PhoneNo { get; set; }
        public string CallType { get; set; }
        public string CallDateTime { get; set; }
        public string ContactName { get; set; }
        public string AddedOn { get; set; }
        public string AddedbyName { get; set; }
    }

    public class Exibitor
    {
        public string pkey { get; set; }
        public string OrganizationID { get; set; }
        public string Organization_pkey { get; set; }
        public string OrganizationTypeID { get; set; }
        public string Email { get; set; }
        public string EmailAddress { get; set; }
        public string Comment { get; set; }
        public string ParticipationTypeID { get; set; }
        public string Status { get; set; }
        public string Level { get; set; }
        public string StatusFG { get; set; }
        public string Alias { get; set; }
        public string ImgLogo { get; set; }
        public string CntUserByPartnerOrg { get; set; }
        public string BoothShow { get; set; }
        public string ImgLogo1 { get; set; }
    }

    public class IssueItem_List
    {
        public string pKey { get; set; }        
        public string AccountKey { get; set; }
        public string IssueKey { get; set; }
        public string IssueDate { get; set; }        
        public string IssueName { get; set; }       
        public string IssueStatusID { get; set; }
        public string ContactName { get; set; }
        public string DeveloperID { get; set; }
        public string EnteredOn { get; set; }
        public string UpdatedOn { get; set; }
        public string InProgress { get; set; }
        public string ActMins { get; set; }
        public string ActHrs { get; set; }
        public string Client { get; set; }
    }
    public class LikeUpdate 
    {
        public int AccountID { get; set; }
        public int PostID { get; set; }
    }
}
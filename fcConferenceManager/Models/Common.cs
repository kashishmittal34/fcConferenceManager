using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using fcConferenceManager.Models.ViewModels;

namespace fcConferenceManager.Models
{
    public class Training_Resources
    {
        public int ID { get; set; }
        public int Event_Pkey { get; set; }
        public int EvtOrg_PKey { get; set; }
        public bool IsActive { get; set; }
        public bool IsMyConsoleOnly { get; set; }
        public string FileURL { get; set; }
        public string FileName { get; set; }
        public string FileDescription { get; set; }
        public string FileType { get; set; }
        public string AudienceProperty { get; set; }
        public string PageArea { get; set; }
        public int SortOrder { get; set; }
        public int AutoPlay { get; set; }
    }
    public class Event_Staff
    {
        public int ID { get; set; }
        public int Event_Pkey { get; set; }
        public int SecurityGroup_pKey { get; set; }
        public int ParticipationStatus_pKey { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ContactName { get; set; }
        public int Account_pKey { get; set; }
        public string PaddedID { get; set; }
        public string RoleID { get; set; }
        public string SecurityGroupID { get; set; }
        public string ParticipationStatusID { get; set; }
        public string Forecolor { get; set; }
        public string EventStatusID { get; set; }
        public string EventFullName { get; set; }
        public string PhoneCall1 { get; set; }
    }
    public class ReviewAccount
    {
        public int pkey { get; set; }
        public int Account_pkey { get; set; }
        public string PaddedID { get; set; }
        public string Title { get; set; }
        public string Contactname { get; set; }
        public string Email { get; set; }
        public string OrganizationID { get; set; }
        public string ProfileImage { get; set; }
        public string UpdateProfileImage { get; set; }
        public bool UpdateImageAccepted { get; set; }
        public string UpdatedStatus { get; set; }
    }
    public class EventScoreList
    {
        public int ID { get; set; }
        public int Account_pKey { get; set; }
        public string ContactName { get; set; }
        public long EducationScore { get; set; }
        public long NetworkingScore { get; set; }
        public long ParticipationSCore { get; set; }
        public long TotalScore { get; set; }
        public bool IsHighlighted { get; set; }
        public string ProfileImage { get; set; }
    }
    public class SessionInfo
    {
        public string timezone { get; set; }
        public int Session_pKey { get; set; }
        public string Title { get; set; }
        public string IsScheduled { get; set; }
        public string Roomname { get; set; }
        public string EventSessionStatusID { get; set; }
        public string EventSpecificDescription { get; set; }
        public string SessionID { get; set; }
        public double Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string EventFullName { get; set; }
        public string EventID { get; set; }
    }
    public class CommunityContest
    {
        public int ID { get; set; }
        public int pKey { get; set; }
        public int TotalLikes { get; set; }
        public int TotalDislikes { get; set; }
        public int Account_pKey { get; set; }
        public int Event_pKey { get; set; }
        public int PostCategory_pKey { get; set; }
        public bool IsLiked { get; set; }
        public string PostTitle { get; set; }
        [AllowHtml]
        public string PostDescription { get; set; }
        public string FileURL { get; set; } = null;
        public string FileType { get; set; } = null;
        public string FileExtension { get; set; } = null;
        public string FileName { get; set; } = null;
        public string PostedBy { get; set; }
        public string PostCategory { get; set; }
        public DateTime PostedDate { get; set; }
        public bool ContainsFile { get; set; }
        public bool IsBlocked { get; set; }
        public int TotalCount { get; set; }
        public string ChatId { get; set; }
        public string CategoryType { get; set; }
        public string name { get; set; }
        public string ShortName { get; set; }
        public string bio { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string self { get; set; }
        public string title { get; set; }
        public string dept { get; set; }
        public string org { get; set; }
        public bool group { get; set; }
        public bool showMeOnNet { get; set; }
        public string AlternateImage { get; set; }
        public int LikeScore { get; set; }
    }
    public class EventSession_SpeakerBreakout
    {
        public string GroupID { get; set; }
        public string GroupLink { get; set; }
    }
    public class SMS_List
    {
        public string SMSID { get; set; }
        public int pkey { get; set; }
        public int EvtSms_PKey { get; set; }
        public string SMSText { get; set; }
        public string MobileNumber { get; set; }
        public string Status { get; set; }
        public int StatusID { get; set; }
        public string SentDate { get; set; }
        public int Account_pkey { get; set; }
        public int AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
    }
    public class User_List
    {
        public string PaddedID { get; set; }
        public int pkey { get; set; }
        public string Name { get; set; }
        public int EvtAccount_PKey { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public bool IsMobile1 { get; set; }
        public string Phone { get; set; }
        public string Phone2 { get; set; }
        public string Department { get; set; }
        public bool IsMobile2 { get; set; }
        public string City { get; set; }
        public int Account_pkey { get; set; }
        public string Zipcode { get; set; }
        public string ActivationDate { get; set; }

        public string PhoneType_pkey { get; set; }
        public string PhoneType2_pkey { get; set; }
        public int Type { get; set; }
        private Boolean Auto = false;
        public Boolean IAuto
        {
            get
            {
                return Auto;
            }
            set
            {
                Auto = value;
            }
        }
    }
    public class EventURL_List
    {
        public int EventpKey { get; set; }
        public string GroupID { get; set; }
        public string GroupLink { get; set; }
        public string RecordingLink { get; set; } = null;
        public string Type { get; set; } = null;
        public string PrivateChatLink1 { get; set; } = null;
        public string PrivateChatLink2 { get; set; } = null;
        public string PrivateChatLink3 { get; set; } = null;
        public int? AccountPkey { get; set; } = null;
        public string URL { get; set; } = null;
        public bool? IsAvailable { get; set; } = false;
        public string HostKey { get; set; } = null;
        public string GroupTitle { get; set; } = null;
        public string TP { get; set; } = null;
        public string HostKey1 { get; set; } = null;
        public string HostKey2 { get; set; } = null;
        public string HostKey3 { get; set; } = null;
        public string BaseURL { get; set; } = null;
        public int? RecordType { get; set; } = null;
        public string GroupLink1 { get; set; } = null;
        public string GroupLink2 { get; set; } = null;
        public string GroupLink3 { get; set; } = null;
        public string GroupLink4 { get; set; } = null;
        public string GroupLink5 { get; set; } = null;
        public string GroupLink6 { get; set; } = null;
        public string GroupLink7 { get; set; } = null;
        public string GroupLink8 { get; set; } = null;
        public string GroupLink9 { get; set; } = null;
        public string BreakOutHostKey1 { get; set; } = null;
        public string BreakOutHostKey2 { get; set; } = null;
        public string BreakOutHostKey3 { get; set; } = null;
        public string BreakOutHostKey4 { get; set; } = null;
        public string BreakOutHostKey5 { get; set; } = null;
        public string BreakOutHostKey6 { get; set; } = null;
        public string BreakOutHostKey7 { get; set; } = null;
        public string BreakOutHostKey8 { get; set; } = null;
        public string BreakOutHostKey9 { get; set; } = null;
        public string BreakOutHostKey10 { get; set; } = null;
        public string WebinarID { get; set; } = null;
        public string PIN { get; set; } = null;
        public string EntryKey { get; set; } = null;
        public string HallwayURL { get; set; } = null;
        //public string LiveStreamURL { get; set; } = null;
        public string HallwayHostKey { get; set; } = null;
        public string WebinarPwd { get; set; } = null;
        public string HallwayPwd { get; set; } = null;
        public DateTime DiscussionStartTime { get; set; } = DateTime.Now;
        public DateTime DiscussionEndTime { get; set; } = DateTime.Now;
        public Boolean WebinarStarted { get; set; }
        public Boolean MeetingStarted { get; set; }
        public Boolean WebinarInApp { get; set; }
        public Boolean MeetingInApp { get; set; }
        public string WebinarHost { get; set; } = null;
        public string MeetingHost { get; set; } = null;

    }
    public class MeetingURLS
    {
        public int pKey { get; set; }
        public int Event_Pkey { get; set; }
        public int Type { get; set; }
        public int HostType { get; set; }
        public string Title { get; set; }
        public string URL { get; set; }
        public string HostKey { get; set; }
        public bool IsRecurrent { get; set; }
        public bool IsScheduled { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateAdded { get; set; }
    }
    public class StaffDocuments
    {
        public int Pkey { get; set; }
        public int LinkType { get; set; }
        public int Account_ID { get; set; }
        public int Event_pKey { get; set; }
        public int Area_pkey { get; set; }
        public bool Status { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string Area { get; set; }
        public string FileGUID { get; set; }
        public string Comments { get; set; }
        public string EUserP { get; set; }
        public string LinkUrl { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
    public class Reminder_Settings
    {
        public int Pkey { get; set; }
        public string ShowAt { get; set; }
        public string LinkText { get; set; }
        public string LinkToolTip { get; set; }
        public string LinkURL { get; set; }
        public string MessageText { get; set; }
        public string LeaderMessageText { get; set; }
        public string ChairMessageText { get; set; }
        public string SpeakerMessageText { get; set; }
        public string SettingID { get; set; }
        public string Description { get; set; }
        public bool IsLinkNewTab { get; set; }
        public bool IsActive { get; set; }
        public string MultiText { get; set; }
        public string MultiLeaderText { get; set; }
        public string MultiChairText { get; set; }
        public string MultiSpeakerText { get; set; }
        public bool IsLinkNewTabOnVirtual { get; set; }
        public string ShowTimes { get; set; }
        public bool IsHalfEvent { get; set; }
        public bool IsMobile { get; set; }
    }
    public class User_Login
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int Result { get; set; }
        public int ErrorCode { get; set; }
        public int RetryCount { get; set; }
        public int intAccountBeingChecked { get; set; }
        public int intLoginType { get; set; }
        public bool GlobalAdmin { get; set; }
        public bool StaffMember { get; set; }
        public string PaddedID { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string Organization { get; set; }
        public int Organization_Key { get; set; }
        public string UserName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; }
        public DateTime EventLastDate { get; set; }
        public string EventCodeName { get; set; }
        public int EventTypeId { get; set; }
        public int ParticipationStatus_pKey { get; set; }
        public int ParentOrganization_pKey { get; set; }
        public int EventAccount_pkey { get; set; }
        public bool bAttendeeAtCurrEvent { get; set; }
        public bool Is_Speaker { get; set; }
    }
    public class EventDetails
    {
        public int EventPKey { get; set; }
        public string RegionCode { get; set; }
        public string StandardRegion { get; set; }
    }
    public class MySessionsPage
    {
        public HelpIconData HelpIconInfo { get; set; }
        public DataTable SessionList { get; set; }
        public DataTable cbTracks { get; set; }
        public DataTable cbTopics { get; set; }
        public DataTable cbLevels { get; set; }
        public DataTable cbDays { get; set; }
        public DataTable ddPrimaryAudience { get; set; }
        public DataTable ddCertificateHours { get; set; }
        public DataTable ddUserCertificate { get; set; }
        public DataTable ddEventVirtualData { get; set; }
        //public ChatViewModel chatViewModel { get; set; }
    }
    public class MySchedulePage
    {
        public HelpIconData HelpIconInfo { get; set; }
        public DataTable SessionList { get; set; }
        public DataTable ddEventVirtualData { get; set; }
        public DataTable ddPhonetype { get; set; }
        //public ChatViewModel chatViewModel { get; set; }
    }
    public class HelpIconData
    {
        //Tab Details 
        public bool TabIconVisible { get; set; }
        public string TabFileName { get; set; }
        public string TabDocumentLink { get; set; }
        public string TabToolTip { get; set; }
        public string TabValue { get; set; }
        public string TabMediaType { get; set; }
        public string TabMime { get; set; }
        //Page Details 
        public string PageFileName { get; set; }
        public string PageDocumentLink { get; set; }
        public bool PageIconVisible { get; set; }
        public string AutoplayType { get; set; }
        public string PageValue { get; set; }
        public string PageMediaType { get; set; }
        public string PageMime { get; set; }
        public bool bAutoPlay { get; set; }
    }
    public class GenericListItem
    {
        public int pKey { get; set; }
        public string value { get; set; }
        public string strText { get; set; }
    }
    public class SpeakerFeedback
    {
        public string speakerHidden { get; set; }
        public string ddTopic { get; set; }
        public string ddContent { get; set; }
        public string ddPresentation { get; set; }
        public string ddSlides { get; set; }
        public string ddValue { get; set; }
        public string ddUnbiased { get; set; }
        public string ddLearning { get; set; }
        public string txtAreaSpeakFeedback { get; set; }
        public string speakerAdvices { get; set; }
    }
    public class EmailContent
    {
        [AllowHtml]
        public string eContent { get; set; }
        public string StandardRegion { get; set; }
    }
    public class EventReminderTips
    {
        public string pKey { get; set; }
        public string Tip { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string ShowFrom { get; set; }
        public string ShowTo { get; set; }
        public int EventpKey { get; set; }
    }
    public class PaymentSubmit
    {
        public double txtCreditAmount { get; set; }
        public string lblCreditAmount { get; set; }
        public string ddCardType { get; set; }
        public string ddMonth { get; set; }
        public string ddYear { get; set; }
        public string txtCCNum { get; set; }
        public string txtCreditCode { get; set; }
        public string txtCreditZipcode { get; set; }
        public string txtCreditAddress { get; set; }
        public string txtCreditName { get; set; }
        public string txtCreditFirstName { get; set; }
        public string txtCreditLastname { get; set; }
        public double dblAccountBalance { get; set; }
        public string ckPaymentType { get; set; }
    }

    public class PaymentModel
    {
        public int intPayMethod { get; set; }
        public string strCharges { get; set; }
        public string strChargesPkey { get; set; }
        public string paycheckName { get; set; }
        public string PayCheckNum { get; set; }
        public string WireBank { get; set; }
        public string WireAccount { get; set; }
        public DateTime dpPayCheckExpect { get; set; }
        public DateTime dpWireDate { get; set; }
        public double CheckAmount { get; set; }

    }
    public class PaymentResult
    {
        public bool bPaymentResult { get; set; }
        public int intReceiptNumber { get; set; }
        public string strPaymentProblem { get; set; }
        public string ErrorMsg { get; set; }
        public bool Redirect { get; set; }
    }
    public class CKEdOptions
    {
        public string Text { get; set; }
        public string Values { get; set; }
        public bool CheckedState { get; set; }
    }
    public class EdOPtionsModel
    {
        public List<CKEdOptions> EDItems { get; set; }
        public string Existing { get; set; }
        public string SelectedCharges { get; set; }
        public string strCert { get; set; }
        public bool bSpeakerAtEvent { get; set; }
        public bool bConferenceEnd { get; set; }
        public int hdnVoucherPkey { get; set; }
        public int intChargeType_pkey { get; set; }
        public int PriorChargePKey { get; set; }
        public int rbOptionsSelected { get; set; }
    }
    public class ScheduleEventFile
    {
        [AllowHtml]
        public string title { get; set; }
        public string Desc { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class MVCNavMenu
    {
        public string URL { get; set; }
        public string Text { get; set; }
        public string Class { get; set; }
        public string Section { get; set; }
        public string Menu { get; set; }
        public bool Admin { get; set; }
        public bool Enable { get; set; }
        public bool Visible { get; set; }
        public int MenuType { get; set; }
        public int MenuOrder { get; set; }
    }

    public class EventFeatures
    {
        public int intAttSch { get; set; }
        public int intAttNet { get; set; }
        public int intAttDnCoupons { get; set; }
        public int intAttEdBadge { get; set; }
        public int intAttLunch { get; set; }
        public int intAttReferrer { get; set; }
        public int intAttBuy { get; set; }
        public int intAttAttendees { get; set; }
        public int intAttActivity { get; set; }
        public int intAttOptions { get; set; }
        public int intAttndeeDinnerResv { get; set; }
        public int intAttAccessBal { get; set; }
        public string strEventPages { get; set; }
        public int intMagiContact_pkey { get; set; }
        public int intMeetingplanner_Pkey { get; set; }
        public bool bSch { get; set; }
        public bool bNet { get; set; }
        public bool bDnCoupons { get; set; }
        public bool bEdBadge { get; set; }
        public bool bLunch { get; set; }
        public bool bReferrer { get; set; }
        public bool bBuy { get; set; }
        public bool bAttendees { get; set; }
        public bool bActivity { get; set; }
        public bool bOptions { get; set; }
        public bool bOpenSpeakingInterest { get; set; }
        public bool bShowConsole { get; set; }
        public bool bEventOpenStaff { get; set; }
        public bool bEventOpenEventSponsors { get; set; }
        public bool bEventOpenSpeakers { get; set; }
        public bool bEventClosedAttendees { get; set; }
        public bool bMAGIUpdate { get; set; }
        public bool bScheduleAlert { get; set; }
        public bool bShowRemindersPanel { get; set; }
        public bool bAutoJoinGroupChat { get; set; }

    }
}
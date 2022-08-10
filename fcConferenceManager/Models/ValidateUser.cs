using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using nsoftware.InPay;

namespace MAGI_API.Models
{
    public class UserInfo
    {
        public string LoginName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string resultID { get; set; }
    }

    public class UserSessionInfo
    {
        public string AccountName { get; set; }
        public string Event_pkey { get; set; }
        public string Account_pkey { get; set; }
        public string Email { get; set; }
        public string resultID { get; set; }
    }

    public class OwinError
    {
        public string error { get; set; }
        public string error_description { get; set; }
        public string resultID { get; set; }
    }

    public class OwinResult
    {
        public string resultID { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string userid { get; set; }
        public string username { get; set; }
        public string eventid { get; set; }
        public string eventname { get; set; }
        public string eventtypeid { get; set; }
        public string eventuserid { get; set; }
        public string eventuserstatusid { get; set; }
        public string role { get; set; }
        [JsonProperty(".issued")]
        public string issued { get; set; }
        [JsonProperty(".expires")]
        public string expires { get; set; }
        public DateTime EventDateTime { get; set; }

        public DateTime EventStartDate { get; set; }
        public DateTime EventEndDate { get; set; }
    }

    public class StatusCode
    {
        public string Account_pkey { get; set; }
        public int Result { get; set; }
        public bool Statusbit { get; set; }
        public string Status { get; set; }
        public string ErrorMsg { get; set; }
        public List<UserResult> lst { get; set; }
        public List<UserDetails> UList { get; set; }
    }

    public class MealSaveCondition
    {
        public int Result { get; set; }
        public bool Statusbit { get; set; }
        public string Status { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class VerifyEmail
    {
        public bool ISUsed { get; set; }
    }
    public class Instruction
    {
        public string Text { get; set; }
        public int Result { get; set; }
        public bool Statusbit { get; set; }
        public string Status { get; set; }
        public string ErrorMsg { get; set; }
    }
    public class UserResult
    {

        public string Salutation_pKey { get; set; }
        public string ContactName { get; set; }
        public string Firstname { get; set; }
        public string MiddleName { get; set; }
        public string Lastname { get; set; }
        public string Suffix { get; set; }
        public string Title { get; set; }
        public string UL { get; set; }
        public string Activated { get; set; }
        public string AccountStatus_pkey { get; set; }
        public string ActivationDate { get; set; }
        public string LastLogin { get; set; }
        public string LastViewedEvent_pKey { get; set; }
        public string Email { get; set; }
        public string Email2 { get; set; }
        public string Phone { get; set; }
        public string Phone2 { get; set; }
        public string AllowEmail { get; set; }
        public string AllowCall { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string Country_pkey { get; set; }
        public string State_pKey { get; set; }
        public string Country_pKey { get; set; }
        public string ZipCode { get; set; }
        public string Timezone_pKey { get; set; }
        public string Department { get; set; }
    }

    public class PartnerGreeting
    {
        public string OrgWebsite { get; set; }
        public string LinkURL { get; set; }
        public string PartnerAudio { get; set; }
        public string Partnerpdf { get; set; }
        public string OrgPartnerpdf { get; set; }
    }

    public class AlltypeFiles
    {
        public string Description { get; set; }
        public string VideoLink { get; set; }
        public string DocumentLink { get; set; }
        public string ORGDocumentLink { get; set; }
        public string ShowDocuments { get; set; }
        public string fileforDownload { get; set; }
        public string FileType { get; set; }
        public string Title { get; set; }
        public string FileTypeID { get; set; }
        public string ShowMp4video { get; set; }
        public string ShowFramevideo { get; set; }
        public string ShowVideoLink { get; set; }
        public string OrganizationID { get; set; }
        public string DocVideo { get; set; }
        public string ShowVideo { get; set; }
        public string ShowDocument { get; set; }

    }
    public class Partner
    {
        public string Event_pkey { get; set; }
        public string organization_pkey { get; set; }
        public string organizationid { get; set; }
        public string Profile { get; set; }
        public string participationlevel_pkey { get; set; }
        public string specialoffer { get; set; }
        public string ShowOffer { get; set; }
        public string PType { get; set; }
        public string ImgLogo { get; set; }
        public string URL { get; set; }
        public string NoImage { get; set; }
        public string pkey { get; set; }
        public string LevelID { get; set; }

        public string ShowGreeting { get; set; }
        public string eventorganizations_pkey { get; set; }

        public string ShowMeet { get; set; }
        public string ShowConnect { get; set; }
        public string ShowSessions { get; set; }
        public string ShowSeeAd { get; set; }
        public string ShowOffer1 { get; set; }
        public string ShowSponsorQuestion { get; set; }
        public string ShowDocument { get; set; }
        public string ShowVideo { get; set; }
        public string ShowTreasureHunt { get; set; }
        public string ShowWebsite { get; set; }
        public string organizationWebsite { get; set; }
        public string Advertisements { get; set; }

    }



    public class UserDetails
    {
        public string Suffix { get; set; }
        public string showJournal { get; set; }
        public string strEmailUsed { get; set; }
        public string GetJournal { get; set; }
        public string Member { get; set; }
        public string Suffix_pkey { get; set; }
        public string Salutation_pKey { get; set; }
        public string ParentOrganization_pKey { get; set; }

        public string globaladministrator { get; set; }
        public string PrioritySpeaker { get; set; }
        public string AdvisoryBoardMember { get; set; }
        public string Ambassador { get; set; }
        public string VIP { get; set; }
        public string DecisionMaker { get; set; }
        public string PotentialSpeaker { get; set; }
        public string SpecialSpeaker { get; set; }
        public string SpeakingPermission { get; set; }
        public string SpeakerMessage { get; set; }
        public string SpecialArrangement { get; set; }


        public string LinkedInProfile { get; set; }
        public string EmailUsed { get; set; }
        public string LicenseType { get; set; }
        public string LicenseNumber { get; set; }
        public string LicenseState { get; set; }
        public string LicenseTypeID { get; set; }


        public string Country_pKey { get; set; }
        public string TimeZone { get; set; }
        public string OtherState { get; set; }
        public string PhoneTypeID { get; set; }
        public string PhoneTypeID2 { get; set; }
        public string PhoneType_pKey { get; set; }
        public string PhoneType2_pKey { get; set; }

        public string Department { get; set; }
        public string Image { get; set; }
        public string Account_pkey { get; set; }
        public string ContactName { get; set; }
        public string Firstname { get; set; }
        public string MiddleName { get; set; }
        public string Lastname { get; set; }
        public string Title { get; set; }
        public string UL { get; set; }
        public string Activated { get; set; }
        public string AccountStatus_pkey { get; set; }
        public string ActivationDate { get; set; }
        public string LastLogin { get; set; }
        public string LastViewedEvent_pKey { get; set; }
        public string Email { get; set; }
        public string Email2 { get; set; }
        public string Phone { get; set; }
        public string Phone2 { get; set; }
        public string AllowText { get; set; }
        public string AllowEmail { get; set; }
        public string AllowCall { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State_pKey { get; set; }
        public string ZipCode { get; set; }
        public string Comment { get; set; }
        //public string GlobalAdministrator { get; set; }
        public string StaffMember { get; set; }
        //public string PrioritySpeaker { get; set; }
        public string GeneralInterestInBeingSpeaker { get; set; }
        public string PersonalBio { get; set; }
        public string AboutMe { get; set; }
        public string Degrees { get; set; }
        public string Nickname { get; set; }
        public string URL { get; set; }
        //public string PasswordSet { get; set; }
        public string LastProfileUpdate { get; set; }
        public string SkypeAddress { get; set; }
        public string LastSpeakerProfileUpdate { get; set; }
        public string SalutationID { get; set; }
        public string CountryID { get; set; }
        public string OrganizationID { get; set; }
        public string StateID { get; set; }
        public string accountstatusID { get; set; }
        public string PhotoApproved { get; set; }
        public string HasPhoto { get; set; }
        public string LastApprovedDate { get; set; }
        public string MostRecentExternalAccess { get; set; }
        public string OrganizationType_pKey { get; set; }
        public string OrganizationTypeID { get; set; }
        public string Phone1CC { get; set; }
        public string Phone2CC { get; set; }
        public string Phone1Ext { get; set; }
        public string Phone2Ext { get; set; }

        public string TimezonePKey { get; set; }
        //public string CVFilename { get; set; }
        public string PhoneticName { get; set; }
        public string AccountImage { get; set; }

    }

    public class UserIssue
    {
        public string event_pkey { get; set; }
        public string account_pkey { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string issueArea { get; set; }
        public string issueType { get; set; }
        public string IssueCategory_pkey { get; set; }
        public string pkey { get; set; }
        public string ContactName { get; set; }
        public string Email { get; set; }
    }

    public class Event_Info
    {
        public string pkey { get; set; }
        public string sectiontitle { get; set; }
        public string sectiontext { get; set; }
        public string sequence { get; set; }
        public string Collapsible { get; set; }
        public string DefCollapsed { get; set; }
        public string TitleLink { get; set; }
        public string ImageLeft { get; set; }
        public string imageleft_pkey { get; set; }
        public string ImageRight { get; set; }
        public string imageright_pkey { get; set; }
        public string LeftVisible { get; set; }
        public string RightVisible { get; set; }
    }

    public class Program
    {
        public string TopicList { get; set; }
        public string pKey { get; set; }
        public string RowType { get; set; }
        public string DayNum { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string TimeSlot { get; set; }
        public string Description { get; set; }
        public string HourSort { get; set; }
        public string MinSort { get; set; }
        public string TrackBG { get; set; }
        public string TrackID { get; set; }
        public string Track_pKey { get; set; }
        public string Track_Prefix { get; set; }
        public string EvtSession_pKey { get; set; }
        public string SessionTime { get; set; }
        public string SessionID { get; set; }
        public string Session_Prefix { get; set; }
        public string Sessiontitle { get; set; }
        public string RelatedSessions { get; set; }
        public string ProfInterests { get; set; }
        public string Speakers { get; set; }
        public string NumSpeaker { get; set; }
        public string Edu { get; set; }
        public string EduDetail { get; set; }
        public string IsNew { get; set; }
        public string SpkCount { get; set; }
        public string IsEducational { get; set; }
        public string EventType_pKey { get; set; }
        public string Room_Name { get; set; }
        public string strTopics_pkey { get; set; }

        public string EventTimeDayWise { get; set; }
        public string keynotes { get; set; }

    }
    public class Speaker
    {
        public string Account_pKey { get; set; }
        public string Fullname { get; set; }
        public string Nickname { get; set; }
        public string MiddleName { get; set; }
        public string Suffix { get; set; }
        public string ShowStar { get; set; }
        public string Tooltip { get; set; }
        public string Degrees { get; set; }
        public string Title { get; set; }
        public string OrganizationID { get; set; }
        public string ActivityDetails { get; set; }
        public string Department { get; set; }

        public string ChatId { get; set; }
        public string AccountImage { get; set; }
    }
    public class SessionDetails
    {
        public string pKey { get; set; }
        public string SessionID { get; set; }
        public string SessionTitle { get; set; }
        public string Description { get; set; }
        public string Account_pKey { get; set; }
        public string ShowLinkNo { get; set; }
        public string ShowLinkYes { get; set; }
    }
    public class PublicSessions
    {
        public string SessionDescription { get; set; }
        public string Event_pKey { get; set; }
        public string Session_pKey { get; set; }
        public string EventSessionStatus_pkey { get; set; }
        public string SchedulingStatus_pkey { get; set; }
        public string ScheduledByAccount_pKey { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Duration { get; set; }
        public string IsScheduled { get; set; }
        public string isLocked { get; set; }
        public string VenueRoom_pKey { get; set; }
        public string SessionFood { get; set; }
        public string pKey { get; set; }
        public string EventSpecificDescription { get; set; }
        public string AssignmentComment { get; set; }
        public string AssignmentAttention { get; set; }
        public string Objective1 { get; set; }
        public string Content1 { get; set; }
        public string Objective2 { get; set; }
        public string Content2 { get; set; }
        public string Objective3 { get; set; }
        public string Content3 { get; set; }
        public string FSize { get; set; }
        public string FPageCount { get; set; }
        public string SpeakersNotWanted { get; set; }
        public string OnRatingForm { get; set; }
        public string AttendEst { get; set; }
        public string AttendAct { get; set; }
        public string FlipCharts { get; set; }
        public string ImportID { get; set; }
        public string FoodBeverages_Pkey { get; set; }
        public string IsHideFromPublic { get; set; }
        public string IsShowOnMyConferencePage { get; set; }
        public string LastUpdated { get; set; }
        public string ProgramTrack_pKey { get; set; }
        public string Updated { get; set; }
        public string IsNew { get; set; }
        public string Track_pKey { get; set; }
        public string Track_Prefix { get; set; }
        public string IsReleased { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string NumAttendees { get; set; }
        public string Roomname { get; set; }
        public string EventSessionStatusID { get; set; }
        public string SessionID { get; set; }
        public string NumSpeakers { get; set; }
        public string LeaderPKey { get; set; }
        public string Leader2PKey { get; set; }
        public string ModeratorPKey { get; set; }
        public string Moderator2Pkey { get; set; }
        public string ChairPKey { get; set; }
        public string NumSigned { get; set; }
        public string NumSlides { get; set; }
        public string NumFeedback { get; set; }
        public string StatusBG { get; set; }
        public string StatusFG { get; set; }
        public string DefaultTrackpkey { get; set; }
        public string SpeakerEqualizer { get; set; }
        public string DaysDate { get; set; }
        public string DfltObj1 { get; set; }
        public string DfltCntnt1 { get; set; }
        public string DfltObj2 { get; set; }
        public string DfltCntnt2 { get; set; }
        public string DfltObj3 { get; set; }
        public string DfltCntnt3 { get; set; }
        public string AcceptableTimes { get; set; }
        public string IsPrivateActivity { get; set; }
        public string ActivityType { get; set; }
        public string Speaker { get; set; }
        public List<Speaker> SpeakerList { get; set; }
        public string RelatedActivity { get; set; }
        public string ProfInterests { get; set; }
        public string Sessioninfo { get; set; }
        public string DocFileName { get; set; }
        public string SessionFileName { get; set; }

    }
    public class AccountBio
    {
        public string Account_pkey { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ContactName { get; set; }
        public string NickName { get; set; }
        public string PersonalBio { get; set; }
        public string Image { get; set; }
        public string Title { get; set; }
        public string OrganizationID { get; set; }
        public string SessionList { get; set; }
        public string Department { get; set; }
        public string NetworkingLevel { get; set; }
        public string IsSpeaker { get; set; }

        public string chatId { get; set; }
        public string COI { get; set; }

    }

    public class mySchedule
    {
        public List<ViewMySchdule> lstSchedule { get; set; }
    }

    public class ViewEventShowURL
    {
        public string RecordingURL { get; set; }
        public string RecURL { get; set; }
        public string VideoPath { get; set; }
        public string ZoomWebinarURL { get; set; }
        public string ZoomMeetingURL { get; set; }
        public string MeetingStarted { get; set; }
        public string HallwayDiscussion { get; set; }
    }
    public class Personchat
    {
        public string ID { get; set; }
        public string img { get; set; }
        public string NameOfPerson { get; set; }
        public string Nickname { get; set; }
        public string ChtType { get; set; }

        public string PersonalBio { get; set; }
        public string Title { get; set; }
        public string OrganizationID { get; set; }
        public string Department { get; set; }
        public string NetworkingLevel { get; set; }
    }
    public class Allchat
    {
        public List<chatHistory> ListchatHistory { get; set; }
    }

    public class NetworkingLevelMSG
    {
        public string levelKey { get; set; }
        public string lvl { get; set; }
        public string pnts { get; set; }
        public string imgReq { get; set; }
        public string BioReq { get; set; }
        public string AttainedPoints { get; set; }
        public string hasBio { get; set; }
        public string hasimage { get; set; }
        public string maxLimit { get; set; }
        public string cnt { get; set; }
        public string percentageOfPeople { get; set; }
    }

    public class Refundcharges
    {
        public string ReceiptNumberText { get; set; }
        public string ReceiptNumber { get; set; }
        public string pKey { get; set; }
        public string Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string CardTransactionID { get; set; }
        public string CardLastFour { get; set; }
        public string PaymentMethodID { get; set; }
    }
    public class EventSummary
    {
        public string CheckPaidOrNot { get; set; }
        public string EventFullName { get; set; }
        public string VenueID { get; set; }
        public string EventstartEndDate { get; set; }
        public string RegistrationLevelID { get; set; }
        public string balance { get; set; }
        public string ReceiptNumber { get; set; }
        public string Paid { get; set; }
        public string Amount { get; set; }
        public string Myoptions { get; set; }
        public string MySchedule { get; set; }
        public string BookAvailable { get; set; }
    }
    public class chatHistory
    {
        public string ChtType { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string ID { get; set; }
        public string img { get; set; }
        public string myID { get; set; }
        public string topImg { get; set; }
        public string nm { get; set; }
        public string topNm { get; set; }
        public string nickName { get; set; }
        public string ak { get; set; }
        public string mid { get; set; }
        public string mine { get; set; }
        public string msgSt { get; set; }
        public string strMsg { get; set; }
        public string typeOfMsg { get; set; }
        public string timeOfMsg { get; set; }
        public string isMineNow { get; set; }
        public string DATEORDER { get; set; }
        public string PersonalBio { get; set; }
        public string Title { get; set; }
        public string OrganizationID { get; set; }
        public string Department { get; set; }
        public string NetworkingLevel { get; set; }

        public string bio { get; set; }

    }
    public class ViewMySchdule
    {
        public string pkey { get; set; }
        public string Session_pkey { get; set; }
        public string SessionID { get; set; }
        public string Description { get; set; }
        public string SessionTitle { get; set; }
        public string Track_pkey { get; set; }
        public string Certifications { get; set; }
        public string PaddedID { get; set; }
        public string st { get; set; }
        public string et { get; set; }
        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }
        public string Subject { get; set; }
        public string Roomname { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string sessionType_pkey { get; set; }
        public string SessiontypeID { get; set; }
        public string TrackID { get; set; }
        public string EducationlevelID { get; set; }
        public string TrackBG { get; set; }
        public string DayNum { get; set; }
        public string TimeSlot { get; set; }
        public string Prefix_track { get; set; }

        public string RecordingURL { get; set; }
        public string RecURL { get; set; }
        public string VideoPath { get; set; }
        public int CollectFeedback { get; set; }
        public int ShowFeedback { get; set; }
        public int EventShowFeedback { get; set; }
        public int EventType_pkey { get; set; }
        public int AttendedPercent { get; set; }
        public string IsShowPromoPlay { get; set; }
        public string IsshowAttendeeSession { get; set; }
        public string IsShowRecURL { get; set; }
        public string IsshowFeedback { get; set; }
        public string IsShowWaitAttendee { get; set; }
        public string WaitTooltip { get; set; }
        public string PollingEnabled { get; set; }
        public string PollingStarted { get; set; }
        public string PollingShowResult { get; set; }

        public string Spkr { get; set; }

    }
    public class CreateMySchdule
    {

        public string pKey { get; set; }
        public string Session_pKey { get; set; }
        public string SessionID { get; set; }
        public string Session_Prefix { get; set; }
        public string Sessiontitle { get; set; }
        public string Description { get; set; }
        public string EducationLevel_pkey { get; set; }
        public string Objectives { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Duration { get; set; }
        public string TrackID { get; set; }
        public string TrackBG { get; set; }
        public string Track_pKey { get; set; }
        public string Track_Prefix { get; set; }
        public string ProfInterestKeys { get; set; }
        public string SessionType_pkey { get; set; }
        public string TimeSlot { get; set; }
        public string ShowTimeslot { get; set; }
        public string EventSessionStatus_pkey { get; set; }
        public string Cancelled { get; set; }
        public string NotCancelled { get; set; }
        public string IsSlideReady { get; set; }
        public string IsNotSlides { get; set; }
        public string FSize { get; set; }
        public string ShowOnSch { get; set; }
        public string RoomName { get; set; }
        public string isEducational { get; set; }
        public string NumSpeakers { get; set; }
        public string SpkCount { get; set; }
        public string IsPrivate { get; set; }
        public string ActivityType { get; set; }
        public string sortOrder { get; set; }
        public string SessionTime { get; set; }
        public string ShowAttend { get; set; }
        public string Educ { get; set; }
        public string NoSlide { get; set; }
        public string slidesChecked { get; set; }
        public string attendChecked { get; set; }
        public string bEnabled { get; set; }
        public string LevelIconFile { get; set; }
        public string IconVisible { get; set; }
        public string LearningObjectives { get; set; }
        public string IsSHow { get; set; }
        public string Request_Status { get; set; }
        public string PrivateActivity { get; set; }
        public string Isinvitation { get; set; }
        public string DayNum { get; set; }
        public string watchChecked { get; set; }

    }

    public class receipt
    {
        public string Invoice { get; set; }
    }
    public class FAQ
    {
        public string FAQ_pKey { get; set; }
        public string FAQa { get; set; }
        public string FAQLink { get; set; }
        public string hasLink { get; set; }
        public string FAQCategory_pkey { get; set; }
        public string SortOrder { get; set; }
        public string Question { get; set; }
        public string FAQCategoryID { get; set; }
        public string SortOrders { get; set; }

    }

    public class EmailSendContent
    {
        public string Account_pkey { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string AccountEmail { get; set; }
        public string AccountContactName { get; set; }
        public string Email { get; set; }
        public string MeetingPlannerDetails_pkey { get; set; }
    }
    public class AnnouncementText
    {
        public string pKey { get; set; }
        public string Title { get; set; }
        public string Contant { get; set; }

    }
    public class Contacts
    {
        public string pKey { get; set; }
        public string RoleID { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string SortOrder { get; set; }
        public string bVis { get; set; }
        public string ContactRole_pKey { get; set; }
    }
    public class DropdownListBind
    {
        public string pKey { get; set; }
        public string strText { get; set; }
        public string UTCoffset { get; set; }
        public string sortorder { get; set; }
    }
    public class Participating
    {
        public string pkey { get; set; }
        public string OrganizationID { get; set; }
    }

    public class ParticipatingOrganizations
    {
        public List<Participating> ParticipatingList { get; set; }
        public List<DropdownListBind> OrgTypeList { get; set; }

    }
    public class SettingText
    {
        public string AppTextBlock { get; set; }
    }

    public class FedbackReBind
    {

        public string Speaker_Pkey { get; set; }
        public string ContactName { get; set; }
        public string SessionTitle { get; set; }
        public string IsHaveSlides { get; set; }
        public string OnTopicScore { get; set; }
        public string ContentScore { get; set; }
        public string PresentationScore { get; set; }
        public string SlidesScore { get; set; }
        public string ValueScore { get; set; }
        public string LearningObjectivesMet { get; set; }
        public string Unbiased { get; set; }
        public string Comment { get; set; }
        public string GComment { get; set; }
        public string MAGISuggestions { get; set; }
        public string AttendedPercent { get; set; }
        public string Title { get; set; }
        public string OrganizationID { get; set; }
        public string IsshowUnbiased { get; set; }
        public string IsShowLOS { get; set; }
        public string disableSpeaker { get; set; }
        public string IsZoomVerification { get; set; }
        public string ZoomVerificationMail { get; set; }
        public string SpeakerAdvice { get; set; }
    }

    public class ConnectPeople
    {
        public string pKey { get; set; }
        public string ChatID { get; set; }
        public string LastLogin { get; set; }
        public string Account_pKey { get; set; }
        public string ContactName { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public string Organization { get; set; }
        public string Email { get; set; }
        public string image { get; set; }
        public string CCEMAIL { get; set; }
        public string Nickname { get; set; }
    }

    public class LunchOptions
    {
        public string EvtSession_pKey { get; set; }
        public string Meal_pKey { get; set; }
        public string bAttend { get; set; }
        public string MealRequest { get; set; }
        public string Comment { get; set; }
        public string DateUpdated { get; set; }
        public string Mealtype { get; set; }
        public string IsSpecial { get; set; }
        public string DefaultMeal_Pkey { get; set; }
        public string SpecialMealID { get; set; }
        public string Lunch { get; set; }
    }

    public class FutureConferences
    {
        public string pKey { get; set; }
        public string EventFullname { get; set; }
        public string Venue { get; set; }
        public string BannerMessage { get; set; }
        public string City { get; set; }
        public string StateID { get; set; }
        public string VenuePlace { get; set; }
        public string Dates { get; set; }
        public string VenueSmall { get; set; }
        public string Image { get; set; }



    }

    public class MeetingPlanneredit
    {
        public string pkey { get; set; }
        public string MeetingTitle { get; set; }
        public string MeetingPurpose { get; set; }
        public string Event_pkey { get; set; }
        public string HostAccount_pkey { get; set; }
        public string MeetingStartdate { get; set; }
        public string Starttime { get; set; }
        public string MeetingStartTimeDate { get; set; }
        public string MeetingEndTimeDate { get; set; }
        public string MeetingEndDate { get; set; }
        public string MeetingEndTime { get; set; }
        public string CreatedDate { get; set; }
        public string MeetingStatus { get; set; }
        public string LocationID { get; set; }
        public string Address { get; set; }
        public string MeetingHour { get; set; }
        public string MeetingMin { get; set; }
        public string Duration { get; set; }
        public string AMPM { get; set; }
        public string Timezone_pkey { get; set; }
    }
    public class OverviewMobile
    {
        public string AppName { get; set; }
        public string AppIcon { get; set; }
        public string Description { get; set; }
        public string OverviewTitle { get; set; }
        public string OverviewDescription { get; set; }
        public string OverviewImage { get; set; }
        public string CurrentEventId { get; set; }
        public string APIUrl { get; set; }
        public string AppUrl { get; set; }
        public string BannerImage { get; set; }
        public string AppType { get; set; }
        public string pKey { get; set; }

        public string BannerImageSide { get; set; }
        public string OverviewImageSide { get; set; }
    }

    public class MobileInfo
    {
        public string pKey { get; set; }
        public string EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string ImageSide { get; set; }
        public string AppType { get; set; }
        public string AppTypeName { get; set; }
        public DateTime ItemDate { get; set; }

        public string EventTimeZone { get; set; }
    }


    public class Networking
    {
        public string pKey { get; set; }
        public string Account_pKey { get; set; }
        public string Title { get; set; }
        public string OrganizationID { get; set; }
        public string EMail { get; set; }
        public string PersonalBio { get; set; }
        public string AboutMe { get; set; }
        public string Match1 { get; set; }
        public string Match2 { get; set; }
        public string ContactName { get; set; }
        public string Firstname { get; set; }
        public string lastname { get; set; }
        public string RowNum { get; set; }
        public string Location { get; set; }
        public string Sector { get; set; }
        public string Greeting { get; set; }
    }


    public class NeworkingOutgoing
    {
        public string pKey { get; set; }
        public string Account_pKey { get; set; }
        public string EventAccount_pKey { get; set; }
        public string Msg { get; set; }
        public string IncE { get; set; }
        public string IncP { get; set; }
        public string ContactDate { get; set; }
        public string Title { get; set; }
        public string OrganizationID { get; set; }
        public string Message { get; set; }
        public string ResponseMessage { get; set; }
        public string Status_pKey { get; set; }
        public string ContactStatusID { get; set; }
        public string ContactName { get; set; }
        public string RowNum { get; set; }
        public string Location { get; set; }
        public string Sector { get; set; }
        public string IsGlobal { get; set; }
        public string Show_Messages { get; set; }
        public string Event_pkey { get; set; }
    }

    public class NetworkingIncoming
    {
        public string Responce_pkey { get; set; }
        public string Msg { get; set; }
        public string pKey { get; set; }
        public string Account_pKey { get; set; }
        public string ContactDate { get; set; }
        public string Title { get; set; }
        public string OrganizationID { get; set; }
        public string EMail { get; set; }
        public string Message { get; set; }
        public string ResponseMessage { get; set; }
        public string AllowEmail { get; set; }
        public string Status_pKey { get; set; }
        public string ContactStatusID { get; set; }
        public string ContactName { get; set; }
        public string RowNum { get; set; }
        public string Location { get; set; }
        public string Sector { get; set; }
        public string Show_Messages { get; set; }
    }

    public class PhoneType
    {
        public string PhoneTypeID { get; set; }
        public string pKey { get; set; }
        public string IsPrivate { get; set; }
        public string IsCell { get; set; }
        public string SortOrder { get; set; }
    }

    public class MeetingPlannerSave
    {
        public string Account_pkey { get; set; }
        public string MeetingPlanner_pkey { get; set; }
        public string MeetingDetail_pkey { get; set; }
        public string Status { get; set; }
        public string pkey { get; set; }
        public string MeetingTitle { get; set; }
        public string MeetingPurpose { get; set; }
        public string Event_pkey { get; set; }
        public string HostAccount_pkey { get; set; }
        public string MeetingStartTime { get; set; }
        public string MeetingEndTime { get; set; }
        public string XMLString { get; set; }
        public string LocationID { get; set; }
        public int TimeZone_pkey { get; set; }
        public string Location_Other { get; set; }
        public string overlapping { get; set; }
        //public string Error    { get; set; }
        //public string IsValidate { get; set; }
        //public string ReturnPkey { get; set; }
    }
    public class MeetingPlannerList
    {
        public string Event_pkey { get; set; }
        public string loginAccount_pkey { get; set; }
        public string Type { get; set; }
        public string pKey { get; set; }
        public string MeetingTitle { get; set; }
        public string MeetingPurpose { get; set; }
        public string MeetingStartTime { get; set; }
        public string MeetingEndTime { get; set; }
        public string Status { get; set; }
        public string Account_pkey { get; set; }
        public string DetailspKey { get; set; }
        public string ContactName { get; set; }
        public string NumAttended { get; set; }
        public string Location { get; set; }
        public string Att_Status { get; set; }
        public string MeetingPlanner_pkey { get; set; }
        public string HostAccount_pkey { get; set; }
        public string IsChecked { get; set; }
        public string Orderby { get; set; }
        public string GroupName { get; set; }
        public string GroupID { get; set; }
        public string TimeZone { get; set; }
        public string Countdate { get; set; }
    }

    public class EventAttending
    {
        public string Att_Status { get; set; }
        public string Name { get; set; }
        public string Account_pkey { get; set; }
        public string ID { get; set; }
        public string Event_pkey { get; set; }
        public string Title { get; set; }

        public string Org { get; set; }
        public string MeetingPlanner_pkey { get; set; }

        public string pKey { get; set; }
        public string Account_pKey { get; set; }
        //public string Title { get; set; }
        public string OrganizationID { get; set; }
        public string EMail { get; set; }
        public string PersonalBio { get; set; }
        public string AboutMe { get; set; }
        public string ContactName { get; set; }
        public string Firstname { get; set; }
        public string lastname { get; set; }
        public string Email { get; set; }
        public string RowNum { get; set; }
        public string Location { get; set; }
        public string Sector { get; set; }
        public string Greeting { get; set; }

    }

    public class MeetingPlannerAllInvited
    {
        public string MeetingPlanner_pkey { get; set; }
        public string Event_pkey { get; set; }
        public string Account_pkey { get; set; }
        public string Status { get; set; }
        public string pkey { get; set; }
        public string IsHost { get; set; }
        public string ContactName { get; set; }
        public string Title { get; set; }
        public string LastName { get; set; }
        public string Firstname { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public string Att_Status { get; set; }
        public string MeetingTitle { get; set; }
        public string InvitationStatus { get; set; }
        public string OrganizationID { get; set; }
        public string Location { get; set; }
        public string Sector { get; set; }
        public string labelStatus { get; set; }
        public string Duration { get; set; }
        public string MeetingStartTime { get; set; }

        public string MeetingTopic { get; set; }
    }

    public class Attendee_Log
    {
        public string Account_pkey { get; set; }
        public string EventOrganization_pkey { get; set; }
        public string AttendeeLog { get; set; }
        public string InTime { get; set; }
        public string Log_Pkey { get; set; }
        public string Event_Pkey { get; set; }
        public string filename { get; set; }
        public string BoothMessage { get; set; }
    }

    public class questionGraph
    {
        public string Option { get; set; }
        public string RespPerc { get; set; }
    }

    public class PoolingManagement
    {

        public string pkey { get; set; }
        public string Forms_pKey { get; set; }
        public string FQ_pKey { get; set; }
        public string Ques { get; set; }
        public string Res { get; set; }
        public string Reo { get; set; }
        public string Req { get; set; }
        public string Question { get; set; }
        public string Attendee { get; set; }
        public string IsStarted { get; set; }
        public string ShowResult { get; set; }
        public string Startedcnt { get; set; }
        public string ShowResultcnt { get; set; }
        public string Answer { get; set; }
        public string Answer_pkey { get; set; }
        public string EventSession_pkey { get; set; }
    }
    public class Question_List
    {
        public string XMLString { get; set; }
        public string pkey { get; set; }
        public string Event_pkey { get; set; }
        public string Account_pkey { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Forms_pKey { get; set; }
        public string FQ_pKey { get; set; }
        public string Ques { get; set; }
        public string Res { get; set; }
        public string Reo { get; set; }
        public string Req { get; set; }
        public string Question { get; set; }
        public string Attendee { get; set; }
        public string Required { get; set; }
        public string Answer { get; set; }
        public string Answer_pkey { get; set; }
        public string EventSession_pkey { get; set; }
        public string IsStarted { get; set; }
        public string Attendee_Question { get; set; }
        public string Attendee_Questions { get; set; }


        public string SOrder { get; set; }
        public string RT { get; set; }
        public string resPkey { get; set; }

        public string ResponseOptions { get; set; }
        public string Comment { get; set; }
        public string CreatedBy { get; set; }
        public string Activity { get; set; }

    }

    public class Form_List
    {
        public string pkey { get; set; }
        public string exhibitorfeedbackformid { get; set; }
        public string NumQ { get; set; }
        public string NumE { get; set; }
        public string event_pkey { get; set; }
        public string Q_pKey { get; set; }
        public string Question { get; set; }
        public string Required { get; set; }
    }

    public class SpeakerFeedBack
    {
        public string GComment { get; set; }
        public string AttendedPercent { get; set; }
        public string MAGISuggestions { get; set; }
        public string SpeakerAdvice { get; set; }
        public string EventSession_pKey { get; set; }
        public string Account_pKey { get; set; }
        public string OnTopicScore { get; set; }
        public string ContentScore { get; set; }
        public string PresentationScore { get; set; }
        public string SlidesScore { get; set; }
        public string ValueScore { get; set; }
        public string LearningObjectivesMet { get; set; }
        public string Unbiased { get; set; }
        public string pKey { get; set; }
        public string LogDate { get; set; }
        public string LogByAccount_pkey { get; set; }
        public string Comment { get; set; }
    }

    public class Organization
    {
        public string pKey { get; set; }
        public string OrganizationID { get; set; }
        public string Event_pkey { get; set; }
        public string Name { get; set; }
    }

    public class EventSchedule
    {

        public string EventDate { get; set; }
        public string Time { get; set; }
        public string type { get; set; }
        public string Host { get; set; }
        public string Title { get; set; }
        public string shortOrder { get; set; }
        public string ShowCondition { get; set; }
        public string Description { get; set; }
        public string ScheduleType { get; set; }
        public string WebinarLink { get; set; }
        public string RoundTableSchedule_pkey { get; set; }
        public string StartDate { get; set; }
        public string EndTime { get; set; }
        public string EventSession_pkey { get; set; }
        public string PrivateGroupChat1 { get; set; }
        public string PrivateGroupChat2 { get; set; }
        public string PrivateGroupChat3 { get; set; }
        public string GroupChatlink { get; set; }

    }
    public class SearchAttendee
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string Account_pKey { get; set; }
        public string ContactName { get; set; }
        public string OrganizationID { get; set; }
        public string longitude { get; set; }
        public string LastGeoAccessTime { get; set; }
        public string altitude { get; set; }

        public string PersonalBio { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
        public string img { get; set; }
    }
    public class Connectionstatus
    {
        public string TooltipStatus { get; set; }
        public string ShowContactName { get; set; }
        public string ShowIMAGES { get; set; }
        public string ConnectionStatus_pKey { get; set; }
        public string ownerAccount_pkey { get; set; }
    }
    public class AttendeeQuestion
    {
        public string Question_pkey { get; set; }
        public string Question { get; set; }
        public string EventSession_pkey { get; set; }
        public string Account_pkey { get; set; }

    }

    public class MeetingScheduleSave
    {
        public string ReturnMessage { get; set; }
        public int IntResult { get; set; }

    }
    public class MeetingScheduleAvailable
    {
        public string pKey { get; set; }
        public string AvailableTimes { get; set; }
        public string MeetingScheduled { get; set; }
        public string AlreadyTimeScheduled { get; set; }
        public string StartTime { get; set; }
        public string ContactName { get; set; }
        public string name { get; set; }
    }
    public class Meet
    {
        public string ContactName { get; set; }
        public string Title { get; set; }
        public string OrganizationID { get; set; }
        public string Account_pkey { get; set; }
        public string Enabel { get; set; }
    }

    public class ExamChargeupdate
    {
        public string Event_pkey { get; set; }
        public string Account_pkey { get; set; }
        public string Memo { get; set; }
        public string chargeType_pkey { get; set; }
        public string CheckedItem { get; set; }
        public string chkdorNot { get; set; }
        public string strchargeType { get; set; }
    }
    public class Examcharges
    {
        public string ChargeTypeID { get; set; }
        public string TypeOfCharge { get; set; }
        public string UserEditable { get; set; }
        public string pKey { get; set; }
        public string ShowOnRegistrationPage { get; set; }
        public string ShowOnRegistrationPageName { get; set; }
        public string RegistrationTooltip { get; set; }
        public string ShowOnRegistrationPageValue { get; set; }
        public string IsOtherDiscount { get; set; }
        public string SortOrder { get; set; }
        public string Event_Pkey { get; set; }
        public string ChargeType_pKey { get; set; }
        public string Amount { get; set; }
        public string Comment { get; set; }
        public string Pkeys { get; set; }
        public Boolean ISChecked { get; set; }
        public int Result { get; set; }
        public bool Statusbit { get; set; }
        public string Status { get; set; }
        public string ErrorMsg { get; set; }

    }

    public class RegistrationResponceSave
    {
        public string Account_pkey { get; set; }
        public string Question_pkey { get; set; }
        public string Event_pKey { get; set; }
        public string Form_pKey { get; set; }
        public string FQ_pKey { get; set; }
        public string Response { get; set; }
        public string UserComment { get; set; }
        public string XMLString { get; set; }
    }
    public class AttendeeRegistriongQues
    {
        public string pkey { get; set; }
        public string Forms_pKey { get; set; }
        public string FQ_pKey { get; set; }
        public string Ques { get; set; }
        public string Res { get; set; }
        public string Reo { get; set; }
        public string Req { get; set; }
        public string Question { get; set; }
        public string DisplayHorizontal { get; set; }
        public string ShowComment { get; set; }
        public string SelectedOptions { get; set; }
        public string Title { get; set; }

    }
    public class AttendeeValidations
    {
        public string QuestionSubmit { get; set; }
        public string NumAttempt { get; set; }
    }
    public class BadgeDesignSetting
    {
        public string BadgeDesignID { get; set; }
        public string BadgeDesignCategory_pkey { get; set; }
        public string Description { get; set; }
        public string MaxLenName { get; set; }
        public string MaxLenTitle { get; set; }
        public string MaxLenOrg { get; set; }
        public string NameReqd { get; set; }
        public string TitleReqd { get; set; }
        public string OrganizationReqd { get; set; }
        public string ShowBorder { get; set; }
        public string ShowBingo { get; set; }
        public string BlocksPerPage { get; set; }
        public string BlockHeight { get; set; }
        public string BlockWidth { get; set; }
        public string BlockBackColor { get; set; }
        public string ShowLevel { get; set; }
        public string pKey { get; set; }
        public string ShowQRCode { get; set; }
        public string PDFOffsetTopMM { get; set; }
        public string PDFOffsetLeftMM { get; set; }
        public string ShowBadgeRibbons { get; set; }
        public string ShowQRCodeFront { get; set; }
        public string ShowBackIndicator { get; set; }
        public string ShowFlag { get; set; }
        public string Flag1 { get; set; }
        public string Flag2 { get; set; }
        public string Flag3 { get; set; }
        public string ShowMeal { get; set; }
        public string FrontName_FontSize { get; set; }
        public string BackName_FontSize { get; set; }
        public string FrontName_FontBold { get; set; }
        public string BackName_FontBold { get; set; }
        public string Organization_FontSize { get; set; }
        public string Organization_FontBold { get; set; }
        public string BingoNumber_MarginLeft { get; set; }
        public string BingoNumber_FontSize { get; set; }
        public string BingoNumber_FontBold { get; set; }
        public string BarcodeBack_MarginRight { get; set; }
        public string LogoHeight_Front { get; set; }
        public string LogoHeight_Back { get; set; }
        public string ShowWcgLogo { get; set; }
        public string BadgesPadding { get; set; }
        public string TextAlignMent { get; set; }
        public string FrontLogo { get; set; }
        public string BackLogo { get; set; }
        public string FirstNameBold { get; set; }
        public string DateUpdated { get; set; }
        public string JobTitle_FontSize { get; set; }
        public string ShowLevelFront { get; set; }
    }
    public class Speakerdinnertext
    {
        public string contant { get; set; }
        public string ChangeText { get; set; }
        public string Change { get; set; }
        public Boolean bCurrentAttendee { get; set; }
        public int IntResult { get; set; }
        public string Status { get; set; }
    }

    public class chatEnabledisable
    {
        public Boolean chatallowed { get; set; }
        public int IntResult { get; set; }
        public string Status { get; set; }
    }
    public class AttendeeTreasureHunt
    {
        public string Account_pKey { get; set; }
        public string IsStamped { get; set; }
        public string Organization_pkey { get; set; }
        public string OrgName { get; set; }
        public string THResponse { get; set; }
        public string THResponseSelected { get; set; }
        public string ChatId { get; set; }
        public string EvtOrg_pkey { get; set; }
        public string MinimumToComplete { get; set; }
        public string ImgLogo { get; set; }
        public string Sponsor { get; set; }
        public string Comment { get; set; }
    }
        public class BadgeInfor
    {
        public string BName { get; set; }
        public string BTitle { get; set; }
        public string BOrganizationID { get; set; }
        public string BedgeHeadtext { get; set; }
        public string NickName { get; set; }
        public int IntResult { get; set; }
        public string Status { get; set; }
    }
    public class LocationParameter
    {
        public int EventId { get; set; }
        public int AppType { get; set; }
        public string AccountId { get; set; }
        public string DeviceId { get; set; }
        public string altitude { get; set; }
        public string longitude { get; set; }
    }

    public class License
    {
        public string Account_pKey { get; set; }
        public string LicenseNumber { get; set; }
        public string LicenseState { get; set; }
        public string LicenseType { get; set; }
    }


    public class GetWayInfo
    {
        public string GatewayURL { get; set; }
        //public string Gateway { get; set; }
        public IchargeGateways Gateway { get; set; }
        public Boolean TestMode { get; set; }
        public string AddSpecialField { get; set; }
        public string MerchantLogin { get; set; }
        public string MerchantPassword { get; set; }
        public bool bCreditCard_CVCode { get; set; }
        public bool bCreditCard_Singlename { get; set; }
        public bool bCreditCard_Firstlastname { get; set; }
        public bool bCreditCard_Zipcode { get; set; }
        public bool bCreditCard_Address { get; set; }
        public string strEventID { get; set; }
    }


    public class ReturReceiptnum
    {
        public string Receipt { get; set; }
    }


    public class SpeakerRefreshScreen
    {
        public int Result { get; set; }
        public bool Statusbit { get; set; }
        public string Status { get; set; }
        public int AttendingStatus { get; set; }
        public Double dblDinnerLateCharge { get; set; }
        public string EventAccountpKey { get; set; }
        public string Warning { get; set; }
        public Boolean Enrolled { get; set; }
        public Boolean pnlRegister { get; set; }
        public Boolean pnlNotAttending { get; set; }
        public Boolean phIsPaid { get; set; }
        public Boolean phNotPaid { get; set; }
        public Boolean cmdChange { get; set; }
        public int rdReg { get; set; }

        public string GuestMSG { get; set; }
        public string lblSpeakerCharge { get; set; }
        public string lblGuestCharge { get; set; }
        public string lblTotalCost { get; set; }

        public int intMaxDinnerGuest { get; set; }
        public Boolean trDinnerGuest { get; set; }


    }
    public class ParticipentNote
    {
        public string pkey { get; set; }
        public string EventSession_pkey { get; set; }
        public string SessionNote { get; set; }
        public string Account_pkey { get; set; }
        public string Addedby { get; set; }
        public string Addedon { get; set; }
        public string Updatedon { get; set; }
        public Boolean IsActive { get; set; }
    }
    public class SpeakerDinnerSave
    {
        public string intdinnerValue { get; set; }
        public string numDinnerGuest { get; set; }
        public string numvegetarianmeals { get; set; }
        public string numglutenmeals { get; set; }
        public Boolean paylater { get; set; }
        public string DiscountCode { get; set; }
        public string Account_pkey { get; set; }
        public string Event_pkey { get; set; }
        public string EventAccount_pkey { get; set; }
        public int Result { get; set; }
        public bool Statusbit { get; set; }
        public string Status { get; set; }

        public int MAxDinnerGust { get; set; }
        public int intDiscountPkey { get; set; }

        public string lblLater { get; set; }
    }

    public class DiscountCodeApply
    {
        public int Result { get; set; }
        public bool Statusbit { get; set; }
        public int intDiscountPkey { get; set; }
        public string Status { get; set; }

        public string strDiscountCode { get; set; }
        public int intDiscountTypePkey { get; set; }
        public string dblDiscountAmount { get; set; }

    }
    public class CommonStatus
    {
        public int Result { get; set; }
        public bool Statusbit { get; set; }
        public string Status { get; set; }
        
    }
    public class PaymentResultStatus
    {
        public int Result { get; set; }
        public bool Statusbit { get; set; }
        public string Status { get; set; }
    }
    public class PaymentUpdate
    {
        public bool bAcctPayment { get; set; }
        public int intPayment_pKey { get; set; }
        public int intPaymentMethod_pKey { get; set; }
        public double dblAmount { get; set; }
        public double dblRefundAmount { get; set; }
        public string strMemo { get; set; }
        public string strComment { get; set; }
        public DateTime dtPaymentDate { get; set; }
        public int intLoggedByAccount_pKey { get; set; }
        public DateTime dtLoggedOn { get; set; }
        public string strPaymentReference { get; set; }
        public bool bPaid { get; set; }
        public string strIntendedAccounts { get; set; }
        public bool bShowComment { get; set; }
        public string strRegistrationSessionID { get; set; }
        public int intRealPaymentMethod_pKey { get; set; }
        public bool bSuccess { get; set; }
        public int intReceiptNumber { get; set; }
        public int intRefundReceiptNumber { get; set; }
        public int intPayerAcctPKey { get; set; }
        public int intEventPKey { get; set; }
        public string strEventID { get; set; }
        public string strCardApprovalCode { get; set; }
        public string strCardTransactionID { get; set; }
        public string strCardReceiptNumber { get; set; }
        public string strCardNumber { get; set; }
        public string intCardType { get; set; }
        public string strCardCode { get; set; }
        public string strCardname { get; set; }

        public string CardExpiration { get; set; }
        public string strCardFirstName { get; set; }
        public string strCardLastName { get; set; }
        public string strCardZipcode { get; set; }
        public string strCardAddress { get; set; }


    }




}

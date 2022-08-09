using MAGI_API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace fcConferenceManager.Models
{
    public class SkypeHistory
    {
        public string ContactName { get; set; }
        public string CallDateTime { get; set; }
        public string AddedbyName { get; set; }
        public int Account_pkey { get; set; }
    }
    public class SpeakerLogHistory
    {
        public string Change { get; set; }
        public string UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public int pKey { get; set; }
    }
    public class EventList
    {
        public bool bChecked { get; set; }
        public string EventID { get; set; }
        public int pKey { get; set; }
    }
    public class SpeakerFlagHistory
    {
        public string EventID { get; set; }
        public string Comments { get; set; }
        public string SpkrFlagID { get; set; }
        public int pKey { get; set; }
    }
    public class CommentsHistory
    {
        public int pKey { get; set; }
        public string Comments { get; set; }
        public string AssignmentStatusID { get; set; }
    }
    public class SpeakerGridFilter
    {
        public int ddFinalDispVal { get; set; }
        public int ddSesStatus { get; set; }
        public int ddlInterested { get; set; }
        public int rbTop { get; set; }
        public int ddSpkrFlag { get; set; }
        public int ddCountry { get; set; }
        public int ddDateType { get; set; }
        public int ddStatus { get; set; }
        public int ddOrgType { get; set; }
        public int intDemiSpkrMgID { get; set; }
        public int ddTimezone { get; set; }
        public int ddDateRange { get; set; }
        public int ddSpeakerStatus { get; set; }
        public int ddRating { get; set; }
        public int ddAccStatus { get; set; }
        public int ddContacted { get; set; }
        public int ddTrack { get; set; }
        public int ddPastActivity { get; set; }
        public int ddResult { get; set; }
        public int intSpkrCurEventPKey { get; set; }
        public bool ckFinalDisp { get; set; }
        public bool chkComment { get; set; }
        public bool chkHideNotes { get; set; }
        public bool chkAnnouncementshow { get; set; }
        public bool chkShowNoteEFree { get; set; }
        public bool chkCallnotesEvent { get; set; }
        public bool chkSelectedPeople { get; set; }
        public bool bNoEvents { get; set; }
        public bool chkAddedBy { get; set; }
        public bool chkPriorities { get; set; }
        public bool bSpkrMgAtt { get; set; }
        public bool bSpkrMgRecent { get; set; }
        public bool bSpkrMgNote { get; set; }
        public bool bSpkrMgTN { get; set; }
        public bool bSpkrMgFD { get; set; }
        public bool bSpkrMgTime { get; set; }
        public bool bAcctSpecialArrangement { get; set; }
        public string SelchkStrSpkr { get; set; }
        public string cbddlInterested { get; set; }
        public string streventInterested { get; set; }
        public string strddlNotiner { get; set; }
        public string strPriorspeaker { get; set; }
        public string strParticipation { get; set; }
        public string strInterestedEvent { get; set; }
        public string strName { get; set; }
        public string strSearch { get; set; }
        public string strnotes { get; set; }
        public string strOrg { get; set; }
        public string strID { get; set; }
        public string strDemiSpkrMgID { get; set; }
        public string strSpeakerStatus { get; set; }
        public string strAnnouncement { get; set; }
        public string strSpeakerFlag { get; set; }
        public string strFollowupRight2 { get; set; }
        public DateTime OtherDate { get; set; }
        public string strevents { get; set; }
        public DateTime dtStart { get; set; }
        public DateTime dtEnd { get; set; }
        public DateTime dtPEnd { get; set; }
        public DateTime dtPStart { get; set; }
        public string sdtStart { get; set; }
        public string sdtEnd { get; set; }
        public string sdtPEnd { get; set; }
        public string sdtPStart { get; set; }
    }
    public class SpeakerGridView
    {
        public string Followupright_Pkey { get; set; }
        public int NumContacts { get; set; }
        public int pKey { get; set; }
        public string Con_ToolTips { get; set; }
        public string ContactName { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public string OrganizationID { get; set; }
        public string PaddedID { get; set; }
        public string AccountPkey { get; set; }
        public string SpkrRating { get; set; }
        public string SpkrRating1 { get; set; }
        public string PSUpdate { get; set; }
        public string SpkrNextContact { get; set; }
        public string FollowNotesToolTip { get; set; }
        public string SpkrNextContactShort { get; set; }
        public string OnlyNotes { get; set; }
        public string NextFiveNotes { get; set; }
        public string All_Notes { get; set; }
        public string All_Notes_Short { get; set; }
        public string ProducerReport { get; set; }
        public string LastName { get; set; }
        public bool chkHideNotes { get; set; }
        public string FirstName { get; set; }
        public string TTip { get; set; }
        public string SpkrFlag { get; set; }
        public string ExpressionDate { get; set; }
        public string PersonalBio { get; set; }
        public string NickName { get; set; }
        public string PhoneticName { get; set; }
        public string Salutation { get; set; }
        public string Degrees { get; set; }
        public string Department { get; set; }
        public string OrganizationTypeID { get; set; }
        public string AboutMe { get; set; }
        public string OrgURL { get; set; }
        public string Account_pKey { get; set; }
        public string FinalDisp { get; set; }
        public string PhoneCall_1 { get; set; }
        public string PhoneCall1 { get; set; }
        public string PhoneCall2 { get; set; }
        public string PhoneCall_2 { get; set; }
        public string SkypeAddress { get; set; }
        public string Phone { get; set; }
        public string EmailAddress { get; set; }
        public string TimeTooltips { get; set; }
        public string TimeZone { get; set; }
        public string EventAccount_pKey { get; set; }
        public string ShowLinkedInProfile { get; set; }
        public string LinkedInProfile { get; set; }
        public string SessionPriorities_Color { get; set; }
        public string Problem { get; set; }
        public string IsNoteShow { get; set; }
        public string PotentialSpeaker { get; set; }
        public string FollowUpRights { get; set; }
        public string ShowSpeakerFlag { get; set; }
        public string AFShow { get; set; }
        public string AFBackColor { get; set; }
        public string PImage { get; set; }
        public string BIOINFO { get; set; }
        public string Proposal { get; set; }
        public string Attended { get; set; }
        public string SpeakerStatus_pkey { get; set; }
        public string FlagTooltips { get; set; }
        public string IsShowPreFlag { get; set; }
        public bool FinalDispVisible { get; set; }
        public string strRIcon { get; set; }
        public string strSIcon { get; set; }
        public string PendingAcName { get; set; }
        public string SpkrStatusText { get; set; }
        public string SpkrStatusBackColor { get; set; }
        public string SpkrStatusForeColor { get; set; }
        public bool SpeakerStatusVisible { get; set; }
        public bool SpkrRatingVisible { get; set; }
        public bool HasPosted { get; set; }
        public bool SpkrRatingAvailable { get; set; }
        public string AccStatusText { get; set; }
        public string AccStatusBackColor { get; set; }
        public string AccStatusForeColor { get; set; }
        public string PrioritySpkr { get; set; }
        public string FBackColor { get; set; }
        public string PronunicationURL { get; set; }
        public string TypeNamePhonetic { get; set; }
        public string List { get; set; }
        public string PendingAccountName { get; set; }
        public string NextFollowUpdate { get; set; }
        public bool AccStatusVisible { get; set; }
        public bool RedStarVisible { get; set; }
        public bool EditLink { get; set; }
        public bool WritingArticle { get; set; }
        public bool imgStarVisible { get; set; }
        public DateTime lastUpdatedP_S { get; set; }
        public SpeakerNameColors NameColors { get; set; }
    }
    public class SpeakerNameColors
    {
        public string LatForeColor { get; set; }
        public string FirstForeColor { get; set; }
        public string NickNameForeColor { get; set; }
        public string PhoneticNameForeColor { get; set; }
        public string SalutationForeColor { get; set; }
    }
    public class ContactModel
    {
        public int intpKey { get; set; }
        public int intPendingAcctPKey { get; set; }
        public int intAccount_pKey { get; set; }
        public int Activity_pkey { get; set; }
        public int Event_pkey { get; set; }
        public int intPendingEventAcctPKey { get; set; }
        public int MagiContact_pkey { get; set; }
        public int Flag { get; set; }
        public int intContactPkey { get; set; }
        public int intPhoneType_pkey { get; set; }
        public int intPhoneType2_pkey { get; set; }
        public int PSAccount_pkey { get; set; }
        public int FollowAccount_pkey { get; set; }
        public string NickName { get; set; }
        public string PhoneNum { get; set; }
        public string CCode { get; set; }
        public string PhoneType { get; set; }
        public string Phone1Ext { get; set; }
        public string skypeAddress { get; set; }
        public string skypeAddress2 { get; set; }
        public string PermanentNotes { get; set; }
        public string CCode2 { get; set; }
        public string strPhoneticIcon { get; set; }
        public string Phone2 { get; set; }
        public string PhoneType2 { get; set; }
        public string Phone2Ext { get; set; }
        public string strMobile { get; set; }
        public string strPendingAcctName { get; set; }
        public string strPendingEMailAddress { get; set; }
        public bool bSpkNotesChanged { get; set; }
        public bool bAllowCall { get; set; }
        public bool bPrivate { get; set; }
        public bool bIsPhone { get; set; }
        public bool bIsCancel { get; set; }
        public bool Followup { get; set; }
        public bool IsPage { get; set; }
        public bool PotentialSpeaker { get; set; }
        public bool bPhone2Expanded { get; set; }
        public bool bshowAnnouncement { get; set; }
        public bool bEventOnly { get; set; }
    }
    public class SpeakerLog
    {
        public int pKey { get; set; }
        public int Account_pKey { get; set; }
        public int ShortOrder { get; set; }
        public bool ChangeColor { get; set; }
        public int event_pkey { get; set; }
        public string MessageID { get; set; }
        public string ContactName { get; set; }
        public string ContactDate { get; set; }
        public string ContactDateFormat { get; set; }
        public string MessageStatusTemplate { get; set; }
        public string EditTemplate { get; set; }
        public string ContactMsg { get; set; }
        public string FollowupDate { get; set; }
        public string FollowupNotes { get; set; }
        public string CtBy { get; set; }
        public string CtVia { get; set; }
        public string FuBy { get; set; }
        public string FuVia { get; set; }
        public string FollowType { get; set; }
        public string EventID { get; set; }
        public string Response { get; set; }
        public string ResponsID { get; set; }
        public string PermanentNotes { get; set; }
        public string SMSID { get; set; }
        public string MessageStatus { get; set; }
        public string IsSend { get; set; }
        public bool IsSendVisi { get; set; }
        public bool IsShow { get; set; }
    }
    public class SpeakerLogUpdate
    {
        public int pKey { get; set; }
        public int Account_pKey { get; set; }
        public int UpdatedforAccount_pkey { get; set; }
        public int ShortOrder { get; set; }
        public bool ChangeColor { get; set; }
        public int event_pkey { get; set; }
        public int CallOutcome_pKey { get; set; }
        public int CallNextAction_pKey { get; set; }
        public int PendingEventAccountPKey { get; set; }
        public int ResponseID { get; set; }
        public string MessageID { get; set; }
        public string ContactName { get; set; }
        public string ContactDate { get; set; }
        public string ContactDateFormat { get; set; }
        public string MessageStatusTemplate { get; set; }
        public string EditTemplate { get; set; }
        public string ContactMsg { get; set; }
        public string FollowupDate { get; set; }
        public string FollowupNotes { get; set; }
        public string CtBy { get; set; }
        public string CtVia { get; set; }
        public string FuBy { get; set; }
        public string FuVia { get; set; }
        public string FollowType { get; set; }
        public string EventID { get; set; }
        public string Response { get; set; }
        public string PermanentNotes { get; set; }
        public string SMSID { get; set; }
        public string MessageStatus { get; set; }
        public string IsSend { get; set; }
        public string EmailSubject { get; set; }
        public string strEmailBody { get; set; }
        public string FallowUpEmailSubject { get; set; }
        public string strMsg { get; set; }
        public string FollowupMessage { get; set; }
        public string FollowUp { get; set; }
        public string strMID { get; set; }
        public string strMessageID { get; set; }
        public string txtPermanent { get; set; }
        public bool IsSendVisi { get; set; }
        public bool IsShow { get; set; }
        public bool IsFollowupTime { get; set; }
        public bool HasClearDate { get; set; }
        public bool bHasFollowUp { get; set; }
        public bool IsSendEmail { get; set; }
        public bool ProducerOnly { get; set; }
        public int rpProducerOnly { get; set; }
        public bool IsPossibleUpdate { get; set; }
        public bool IsPossible { get; set; }
        public bool bFollowUp { get; set; }

    }
    public class dlOtherSpot
    {
        public int Account_pkey { get; set; }
        public int EventSession_pkey { get; set; }
        public int AccountSession_pKey { get; set; }
        public string SessionID { get; set; }
        public bool chkActivity { get; set; }
        public bool chkRemove { get; set; }
        public bool CommentsUpdate { get; set; }
    }
    public class SessionSpeakerStatus
    {
        public int intPendingAcctPKey { get; set; }
        public int intSpkrCurEventPKey { get; set; }
        public int intStatus_pkey { get; set; }
        public int intPendingAcctSesPKey { get; set; }
        public int Session_pKey { get; set; }
        public int intPendingEvtSesPKey { get; set; }
        public int intPendingEventAcctPKey { get; set; }
        public int intAccount_PKey { get; set; }
        public int intEditMode { get; set; }
        public int intCommentHpKey { get; set; }
        public int MyProperty { get; set; }
        public int intPendingStatusPKey { get; set; }
        public int ddSpeakerContactIndex { get; set; }
        public List<dlOtherSpot> dlOtherSpot { get; set; }
        public List<dlOtherSpot> dlOtherSession { get; set; }
        public int ddSpeakerContact { get; set; }
        public string ddSpeakerContactText { get; set; }
        public string txtrddatetimepicker { get; set; }
        public string strNewStatus { get; set; }
        public string strPendingAcctName { get; set; }
        public string strDueDate { get; set; }
        public string strPendingSessionName { get; set; }
        public string ddEditStatusSelected { get; set; }
        public string strComment { get; set; }
        public string strHomeLesson { get; set; }
        public string dpTime { get; set; }
        public bool SendEmail { get; set; }
        public bool chkRemoveEvent { get; set; }
        public bool txtDueDateVisible { get; set; }
        public bool txtrddatetimepickerVisible { get; set; }
        public bool chkRemoveSlide { get; set; }
        public int HaveSlide { get; set; }
        public bool chkIsSlide { get; set; }
        public bool chkCancelRef { get; set; }
        public bool chkIsLeader { get; set; }
        public bool chkOfferLeader { get; set; }
        public bool chkIsModu { get; set; }
        public bool chkOfferMod { get; set; }
        public bool chkCharge { get; set; }
    }
    public class ContactOperations
    {
        public DataTable getSpeakers(int EventPKey, int AccountPKey, int Session_pkey)
        {
            try
            {
                string qry = " Select '('+x2.Track_Prefix+Sl.SessionID +') '+ ISNULL(Al.Lastname,'')+' ' + ISNULL(Al.Firstname,'')AS ContactName  ,Al.pKey as Account_pkey "
                    + Environment.NewLine + " ,( Select pkey from Event_Accounts Where Account_pkey=Al.Pkey And Event_pKey =@Event_pkey) As EventAccount_pkey,IIF(Al.pkey=@Account_pkey,'True','False') as IsChecked ,Al.Email As Email"
                    + Environment.NewLine + " from EventSession_Staff x1 "
                    + Environment.NewLine + " Inner Join Event_Sessions x2 On x2.pKey = x1.EventSession_pkey "
                    + Environment.NewLine + " INNER JOIN Session_List Sl On x2.Session_pKey=Sl.pKey"
                    + Environment.NewLine + " INNER JOIN Account_List Al ON X1.Account_pKey=Al.pKey"
                    + Environment.NewLine + " where x1.IsSpeaker = 1 And x2.Event_pKey = @Event_pkey  ANd x2.Session_pKey=@Session_pkey";
                SqlParameter[] parameters = new SqlParameter[] {
                     new SqlParameter("@AccountPKey",AccountPKey),
                     new SqlParameter("@Event_pkey",EventPKey),
                     new SqlParameter("@Session_pkey",Session_pkey),
                };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch
            {

            }
            return null;
        }
        public int GetFollowUpByAccount(string AccountPKey)
        {
            try
            {
                string qry = "Select pkey from FollowupRights  where Account_pkey=  @AccountPKey";
                SqlParameter[] parameters = new SqlParameter[] {
                     new SqlParameter("@AccountPKey",  AccountPKey),
                 };
                DataTable dt = SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
                if (dt!= null && dt.Rows.Count>0)
                    return ((dt.Rows[0][0]!= DBNull.Value) ? Convert.ToInt32(dt.Rows[0][0]) : 0);
            }
            catch (Exception ex)
            {

            }
            return 0;
        }
        public DataTable BindActivity(int EventPKey)
        {
            try
            {
                string qry = "SELECT t0.pKey, isnull(t0.Track_Prefix,'')+isnull(t1.SessionID,'') as strText From Event_Sessions t0"
                    + Environment.NewLine + " Inner Join Session_List t1 on t1.pKey = t0.Session_pkey"
                    + Environment.NewLine + " LEFT OUTER JOIN Sys_Tracks t3 ON t3.pkey=t1.Track_pKey"
                    + Environment.NewLine + " Where  t3.Educational=1 and t0.Event_pKey = @EventPKey"
                    + Environment.NewLine + " Order by strText";

                SqlParameter[] parameters = new SqlParameter[] {
                  new SqlParameter("@EventPKey",EventPKey),
                };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);

            }
            catch
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 0, "Error loading Activity dropdown list ");
            }
            return null;
        }
        public DataSet GetSpeakerLog(int EventPKey, int EvenAccountpKey, int AccountPKey, bool IsSame, bool AdditonalFollow)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[] {
                  new SqlParameter("@Event_pkey",EventPKey),
                  new SqlParameter("@EventAccount_pKey",EvenAccountpKey),
                  new SqlParameter("@Account_pkey",AccountPKey),
                  new SqlParameter("@IsSame",IsSame),
                  new SqlParameter("@AdditionalFollow",AdditonalFollow),
            };
                return SqlHelper.ExecuteSet("SpeakerLog_SelectMVC", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 130, "");
            }
            return null;
        }
        public bool DeleteSpeakerContactLog(int pKey)
        {
            try
            {
                string qry = "Delete from EventAccount_Communication Where pKey = @PK";
                SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@PK",pKey),
            };
                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public void UpdateSpeakerNextContact(int eaID)
        {
            try
            {
                string qry = "Update Event_Accounts set SpkrNextContact =Null where pkey = @EventAccountID";
                SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@EventAccountID",eaID),
            };
                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
            }
        }
        public bool AddSpeakerLog(SpeakerLogUpdate Model, int USERID)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@pKey", Model.pKey),
                    new SqlParameter("@EventAcct_pkey", Model.PendingEventAccountPKey),
                    new SqlParameter("@UpdatedByAcctPKey", USERID),
                    new SqlParameter("@HasFollowup", Model.bHasFollowUp),
                    new SqlParameter("@ConDate", Model.ContactDate),
                    new SqlParameter("@By", Model.CtBy),
                    new SqlParameter("@Via", Model.CtVia),
                    new SqlParameter("@Message", Model.strMsg.Trim()),
                    new SqlParameter("@IsFollowupTime",  Model.IsFollowupTime),
                    new SqlParameter("@FollowupDate",  Model.FollowupDate),
                    new SqlParameter("@FollowupBy",  Model.FuBy),
                    new SqlParameter("@FollowupVia",  Model.FuVia),
                    new SqlParameter("@FollowupMessage",  Model.FollowupMessage),
                    new SqlParameter("@CallOutcome_pKey",Model.CallOutcome_pKey),
                    new SqlParameter("@CallNextAction_pKey", Model.CallNextAction_pKey),
                    new SqlParameter("@EmailSubject", Model.EmailSubject),
                    new SqlParameter("@FallowUpEmailSubject", Model.FallowUpEmailSubject),
                    new SqlParameter("@EmailBody",Model.strEmailBody),
                    new SqlParameter("@UpdatedforAccount_pkey", Model.UpdatedforAccount_pkey),
                    new SqlParameter("@event_pkey",  Model.event_pkey),
                    new SqlParameter("@MethodID",  0),
                    new SqlParameter("@EmailSend", Model.IsSendEmail),
                    new SqlParameter("@ProducerOnly", Model.ProducerOnly),
                    new SqlParameter("@MessageID", Model.strMessageID),
                    new SqlParameter("@ResponseID",  Model.Response),
                    new SqlParameter("@PermanentNotes", Model.txtPermanent),
                    new SqlParameter("@FollowupType", Model.FollowType),
                    new SqlParameter("@MSID",Model.strMID),
                    new SqlParameter("@ISpossibleUpdate", Model.IsPossibleUpdate),
                    new SqlParameter("@ISpossible",  Model.IsPossible),
                    new SqlParameter("@AdditionalFollowup", Model.bFollowUp),
                    new SqlParameter("@HasClearDate", Model.HasClearDate)
                };
                SqlHelper.ExecuteNonQuery("EventAccount_LogContact_MVC", CommandType.StoredProcedure, parameters);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool UpdateAllowCallEmail(int PendingAccountPKey, bool ModeValid)
        {
            try
            {
                string qry = "Update Account_List set AllowEmail=@AllowEMail where pkey = @pendingAccountpeky"
                    + Environment.NewLine + "Update Account_List set AllowCall=@AllowCall where pkey =  @pendingAccountpeky";

                SqlParameter[] parameters = new SqlParameter[] {
                        new SqlParameter("@pendingAccountpeky",PendingAccountPKey),
                        new SqlParameter("@AllowEMail",ModeValid),
                        new SqlParameter("@AllowCall",ModeValid),
                };

                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public bool SaveFlagComments(int PendingAccountPKey, int EventPKey, int AccountPkey, int SpkrFlag, string NoteText)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                     new SqlParameter("@Account_pkey", PendingAccountPKey),
                     new SqlParameter("@Event_pKey", EventPKey),
                     new SqlParameter("@AuthorAccount_pKey",AccountPkey),
                     new SqlParameter("@UpdateAccount_pKey",AccountPkey),
                     new SqlParameter("@NoteText",  NoteText.Trim()),
                     new SqlParameter("@SpkrFlag_pKey", SpkrFlag),
                };
                SqlHelper.ExecuteNonQuery("Update_spkrflagMVC", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public void UpdateSpeakingPermission(int PendingAccountPKey)
        {
            try
            {
                string qry = "pdate Account_List set SpeakingPermission = 1 ,IsSpeakerMessage=0 where pkey = @PendingID";
                SqlParameter[] parameters = new SqlParameter[]
                {
                      new SqlParameter("@PendingID",PendingAccountPKey),
                };

                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
            }
            catch
            {
            }
        }
        public void Note_Insert_Update(int intPendingAcctPKey, int IntCurrentevent_pkey, int intAccountPKey, string msg, string Qtype = "InsertUpdateNotes", int FinalDisposition_pkey = 0, string NoteText = "", int Followupright_Pkey = 0)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                     new SqlParameter("@Note_Pkey",0),
                     new SqlParameter("@Account_pkey", intPendingAcctPKey),
                     new SqlParameter("@Event_pKey", IntCurrentevent_pkey),
                     new SqlParameter("@AuthorAccount_pKey",intAccountPKey),
                     new SqlParameter("@UpdateAccount_pKey",intAccountPKey),
                     new SqlParameter("@NoteText",  NoteText.Trim()),
                     new SqlParameter("@QueryType", Qtype),
                     new SqlParameter("@type", "Note"),
                     new SqlParameter("@FinalDisposition_pkey",FinalDisposition_pkey),
                     new SqlParameter("@Followupright_Pkey",Followupright_Pkey)
                };
                SqlHelper.ExecuteNonQuery("Update_spkrflagMVC", CommandType.StoredProcedure, parameters);
            }
            catch
            {

            }
        }
        public void saveSpealerFollowup(string txtPermanent, string txtConMsg)
        {
            try
            {
                string qry = "Update  SpeakerContact Set Comment=@Comment,TopicSession=@TopicSession Where pkey= @ContactPkey";
                SqlParameter[] parameters = new SqlParameter[]
                {
                      new SqlParameter("@TopicSession",txtPermanent),
                      new SqlParameter("@Comment", txtConMsg),
                };

                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
            }
            catch
            {
            }
        }
        public DataTable GetspeakerLogByID(int pKey)
        {
            try
            {
                string qry = " Select EventAccount_pKey,Account_pKey,ContactDate,CallOutcome_pKey,CallNextAction_pKey,NextActionDate,Comment,EmailSubject,EmailBody,pKey,FallowUpEmailSubject,"
                 + Environment.NewLine  + " HasFollowup,ContactBy,ContactVia,FollowupBy,FollowupVia,FollowupNotes,ProducerOnly,event_pkey,UpdatedforAccount_pkey,UpdatedforAccount_pkey_old"
                 + Environment.NewLine  + " EmailSend,AccountSessionHistory_pkey,ResponseID,PermanentNotes,FollowupType,MSID,MessageID,AdditionalFollowup,IsFollowupTime,IsSMSSend,"
                 + Environment.NewLine  + " ISNULL(t1.MethodID,1) as Method ,Convert(varchar(20),t1.ContactDate,1) as ContactDate1 ,Convert(varchar(20),t1.NextActionDate,1) as NextActionDate1 ,ISNULL(t1.ResponseID,0) AS Response_ID,ISNULL(PermanentNotes,'') as Permanent_Notes"
                 + Environment.NewLine  + " ,ISNULL(FollowupType,'') as FollowupType_Options ,ISNULL(t1.IsFollowupTime,0) as SpeakerFollowup From EventAccount_Communication t1 Where t1.pkey=@pKey "
                 + Environment.NewLine  + " Order by  t1.pKey desc";

                SqlParameter[] parameters = new SqlParameter[]
                {
                   new SqlParameter("@pKey",pKey),
                };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 130, "");
            }
            return null;
        }
        public DataTable SMSSignature(int AccountID)
        {
            try
            {
                DataTable dt = new DataTable();
                string qry = "Select ISNULL(t1.Firstname,'') as Firstname,ISNULL(t1.MiddleName,'') as MiddleName,ISNULL(t1.Lastname,'') as Lastname,t1.Suffix,t1.NickName,t1.Title,	t1.Email,t1.Email2,	t1.Phone,t1.Phone2,t1.Department,t1.Phone1CC,t1.Phone2CC,ISNULL(t3.OrganizationID,'') as OrganizationID from Account_List t1"
                    + Environment.NewLine  + " Left outer join organization_list t3 On t3.pkey = t1.ParentOrganization_pKey Where t1.pkey = @pKey";
                SqlParameter[] parameters = new SqlParameter[]
                {
                   new SqlParameter("@pKey",AccountID),
                };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {


            }
            return null;
        }
        public bool UpdateSpeakerspeaker(int SpeekaerStatus, int AcID)
        {
            try
            {
                string qry = "Update Account_List set SpeakerStatus_pkey = @speakerStatus where pkey = @ACID";
                SqlParameter[] parameters = new SqlParameter[]
                {
                     new SqlParameter("@speakerStatus", SpeekaerStatus),
                     new SqlParameter("@ACID", AcID),
                };
                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;

        }
        public bool MarkReviewed(int SpeekaerStatus, int AcID, int EID)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                     new SqlParameter("@Event_pkey", EID),
                     new SqlParameter("@Account_pKey", AcID),
                     new SqlParameter("@NewStatus_pkey", SpeekaerStatus),
                };
                SqlHelper.ExecuteNonQuery("EventSession_UpdateAssignmentMVC", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;

        }
        public DataTable GetSessionHistory(int EventPKey, int AccountSessionPKey)
        {
            try
            {
                string qry = " SELECT A.AccountSession_pkey,A.Event_pKey,A.AssignmentStatus_pKey,A.pKey,A.LastUpdated,A.Comments, AssignmentStatusID  As AID,"
                            + Environment.NewLine + " 'Status : - ' + S.AssignmentStatusID +', Last updated:- ' + CONVERT(varchar(200), A.LastUpdated )  As AssignmentStatusID FROM Account_SessionHistory as A"
                            + Environment.NewLine + " LEFT OUTER JOIN SYS_AssignmentStatuses AS S ON A.AssignmentStatus_pKey = S.pKey"
                            + Environment.NewLine + " Where  AccountSession_pkey=@AccountSessionPKey And Event_pKey = @EventPKey"
                            + Environment.NewLine + " ORDER By  LastUpdated DESC";

                SqlParameter[] parameters = new SqlParameter[]
                {
                        new SqlParameter("@EventPKey", EventPKey),
                        new SqlParameter("@AccountSessionPKey", AccountSessionPKey),
                };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 130, "");
            }
            return null;
        }
        public bool CommentDelete(int EventPKey, string intCommentHpKey, int intPendingAcctSesPKey)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                     new SqlParameter("@pKey", intCommentHpKey),
                     new SqlParameter("@Event_pkey", EventPKey),
                     new SqlParameter("@AcctSespKey", intPendingAcctSesPKey),
                };
                SqlHelper.ExecuteNonQuery("CommentsDeleteMVC", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public void GetBalanceSpeaker(int EventPKey, int PendingAccount, ref bool SpkNotPaidforRegi, ref decimal balance)
        {
            try
            {
                string qry = "Select  account_pkey	,event_pkey	,chargesaccrued	,paymentsapplied,	balance	,tamount,	ptype,	options	,paymentCount	,paymentmethod	,PaymentDate from getAccountBalance(@PendingAccount,@EventpKey)";
                SqlParameter[] parameters = new SqlParameter[]
                {
                  new SqlParameter("@EventPKey", EventPKey),
                  new SqlParameter("@PendingAccount", PendingAccount)
                };
                DataTable dt = SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
                balance =0;
                SpkNotPaidforRegi = false;
                if (dt!= null && dt.Rows.Count>0)
                {
                    if (dt.Rows[0]["balance"] != System.DBNull.Value)
                        balance = Convert.ToDecimal(dt.Rows[0]["balance"]);

                    if (dt.Rows[0]["SpkNotPaidforRegi"] != System.DBNull.Value)
                        SpkNotPaidforRegi = Convert.ToBoolean(dt.Rows[0]["SpkNotPaidforRegi"]);

                    if (SpkNotPaidforRegi && balance <=0)
                        SpkNotPaidforRegi = true;
                }
            }
            catch
            {

            }
        }
    }
    public class SpeakerNotes
    {
        public int pKey { get; set; }
        public string NoteDate { get; set; }
        public string UpdateDate { get; set; }
        public string NoteBy { get; set; }
        public string UpdateBy { get; set; }
        public string NoteText { get; set; }
        public string lblNoteText { get; set; }
        public string lblNoteDate { get; set; }
    }
    public class SpeakerManagementOperations
    {
        public DataTable FetchContactFilters(int ListType, int IsOther = 0)  // Filters List Without Parameters
        {
            DataTable data = null;
            try
            {
                StringBuilder qry = new StringBuilder();
                switch (ListType)
                {
                    case 1: //
                        qry.Append("SELECT pKey, CallOutcomeID as strText ,case when pkey=15 then 1 when pkey=8 then -1 else 0 end sort , ShortOrder");
                        qry.Append(" FROM SYS_CallOutcomes Where ISNULL(Active,0)=1");
                        qry.Append(" UNION");
                        qry.Append(" SELECT pKey, CallOutcomeID as strText ,0 as sort, ShortOrder");
                        qry.Append(" FROM SYS_CallOutcomes where pkey=" + IsOther.ToString());
                        qry.Append("  Order by sort DESC ,ShortOrder asc");
                        break;
                    case 2:  //
                        qry.Append(" SELECT pKey, CallOutcomeID_futher as strText ,case when pkey=15 then 1 when pkey=8 then -1 else 0 end sort , ShortOrder");
                        qry.Append(" FROM SYS_CallOutcomes Where ISNULL(Active,0)=1");
                        qry.Append(" UNION");
                        qry.Append(" SELECT pKey, CallOutcomeID as strText ,0 as sort, ShortOrder");
                        qry.Append(" FROM SYS_CallOutcomes where pkey="+ IsOther.ToString());
                        qry.Append("  Order by sort DESC ,ShortOrder asc");
                        break;
                }
                if (!string.IsNullOrEmpty(qry.ToString()))
                    data = SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, null);
            }
            catch (Exception ex)
            {
                data = null;
            }
            return data;
        }
        public DataTable FetchSpeakerFiltersByEvent(int Event_pKey, int ListType)
        {
            DataTable data = null;
            try
            {
                StringBuilder qry = new StringBuilder();
                switch (ListType)
                {
                    case 1: // List Of Events Already Started or completed With Selected Current Event
                        qry.Append("Select t0.Session_pKey as pkey , t0.Track_Prefix+t1.SessionID as strText ,ISNULL(t1.SessionTitle ,'') as SessionTitle from Event_Sessions t0 INNER JOIN Session_List t1 ON t1.pkey=t0.Session_pKey Where event_pkey <> @Event_pKey");
                        qry.Append(Environment.NewLine + "GROUP BY  t0.Session_pKey , t0.Track_Prefix+t1.SessionID ,ISNULL(t1.SessionTitle ,'') Order by t0.Track_Prefix+t1.SessionID");
                        break;
                }
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Event_pKey", Event_pKey.ToString()),
                };
                if (!string.IsNullOrEmpty(qry.ToString()))
                {
                    data = SqlHelper.ExecuteTable(qry.ToString(), CommandType.Text, parameters);
                }
            }
            catch (Exception ex)
            {
                data = null;
            }
            return data;
        }
        public DataTable getMenuAccountSettings(int Account_pKey)
        {
            try
            {
                string qry = "SELECT Case When ISNULL(t1.OtherDate,'')<>'' then CONVERT(VARCHAR(10), t1.OtherDate, 101)  else CONVERT(VARCHAR(10), GETDATE(), 101) end as SelectionDate,"
                + Environment.NewLine + " Case When ISNULL(t1.OtherDate, '')<>'' then 1 Else 0 End AS IsHasOtherDate,ISNULL(t1.FilterID, 1) as Filter_ID,"
                + Environment.NewLine + " IIf(ISNULL(t1.Last_NextConDate, '')='', '', Convert(varchar, t1.Last_NextConDate, 101)) as Last_NextConDates,"
                + Environment.NewLine + " IIf(ISNULL(t1.Last_SlideDueDate, '')<>'', Convert(varchar, t1.Last_SlideDueDate, 101), 'none')as Last_SlideDueDates,t1.OtherDate"
                + Environment.NewLine + " from account_list t1  where t1.pKey = @Account_pKey";

                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@Account_pKey", Account_pKey) };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch
            {
                return null;
            }
        }
        public string getSpeakerStats(int EventId)
        {
            try
            {
                string qry = "Select dbo.Speakers_stats(@EventID)";
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@EventID", EventId) };
                DataTable dt = SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
                if (dt!= null && dt.Rows.Count > 0)
                    return (dt.Rows[0][0] != DBNull.Value) ? dt.Rows[0][0].ToString() : "";
            }
            catch
            {
            }
            return "";
        }
        public DataTable DataSessionList(int intAcctPKey, bool ckTime, int intSpkrCurEventPKey, int ddSesStatus, int acSessionpKey = 0)
        {
            DataTable dt = new DataTable();
            try
            {
                string qry = "Select t1.pKey As AcctSesPKey,t1.Account_pKey, t1.Session_pkey, t3.pKey As EvtSesPKey, t3.Track_Prefix + t2.SessionID as SessionID,'('+convert(varchar(10),isnull(t1.Priority,4))+')' SessionPriority,Case WHEN t1.Priority=9 AND t5.AssignmentStatus_pKey IS NULL then isNull(t6.AssignmentStatusID,'Interested?') else isNull(t6.AssignmentStatusID,'Interested') END as Status"
                    + Environment.NewLine + ", Case When t1.Approach like ('%Added by%') then '' Else ISNULL( t1.Approach,'') END AS Approach  , Case WHEN t1.Priority=9 AND t5.AssignmentStatus_pKey IS NULL then  ISNULL(t5.AssignmentStatus_pKey,27) ELSE  ISNULL(t5.AssignmentStatus_pKey,21) END as  AssignmentStatus_pKey ,t3.EventSessionStatus_pkey"
                    + Environment.NewLine + ", isnull(t1.InterestedInBeingLeader,0) as VisibleLeader, isnull(t1.InterestedInBeingModerator,0) as VisibleModerator";
                if (ckTime)
                    qry  += Environment.NewLine + ", (Case when t3.IsScheduled = 1 Then t3.Title + ' (' +   DATENAME(WEEKDAY, t3.Starttime) + ', '+  DATENAME(MONTH,t3.Starttime)   + '  '+CONVERT(VARCHAR(12), DATENAME(DAY, t3.Starttime))+', ' + format(t3.Starttime,'hh:mm tt')+' - '+format(t3.endtime,'hh:mm tt') +')'  else t3.Title end) as SessionTitle";
                else
                    qry  += Environment.NewLine + ", t3.Title as SessionTitle";

                qry  +=  Environment.NewLine + ", (case when getdate() <= el.EndDate Then 1 else 0 end) as ShowIt"
                + Environment.NewLine + ", ISNULL(t3.IsScheduled,0) as IsScheduled ,ISNULL(P.List,'') AS List,Case When ISNULL(PS.PotentialSpeaker,0) >=0 then 'Session added by '+ PS.ContactName Else '' END AS ContactName ,ISNULL(t3.numspeakers,0) as numspeakers "
                + Environment.NewLine + ", Case When LEN(ISNULL(t7.DueDate_TBD,0))>0 AND t7.DueDate_TBD <>'0' Then DueDate_TBD  Else (Case when  t7.DueDate Is Not Null then  Convert(varchar, t7.DueDate,101) else ''END) END  As DueDate"
                + Environment.NewLine + ", CASE When ISNULL(t2.IsHaveSlides,1)=1 OR ISNULL(t2.IsHaveSlides,1)=0  THEN 1 ELSE ISNULL(t2.IsHaveSlides,1)END AS IsHaveSlide"
                + Environment.NewLine + ", CASE WHEN t5.AssignmentStatus_pKey in(2,19) AND ISNULL(t2.NumLeaders,0) >0  AND  ISNULL(PLM.Leader,'TBD') <>'TBD' AND  ISNULL(PLM.Leader,'')<>'' THEN ISNULL(PLM.Leader,'TBD') ELSE '' END  As Leader,CASE WHEN t5.AssignmentStatus_pKey in(2,19) AND  ISNULL(t2.NumModerators,0) >0  AND  ISNULL(PLM.Moderator,'TBD')<>'TBD' AND  ISNULL(PLM.Moderator,'')<>''  THEN  ISNULL(PLM.Moderator,'TBD') ELSE '' END as Moderator"
                + Environment.NewLine + ", CASE WHEN t5.AssignmentStatus_pKey in(2,19) AND ISNULL(t2.NumLeaders,0) >0  AND  ISNULL(PLM.Leader,'TBD') <>'TBD' AND  ISNULL(PLM.Leader,'')<>'' then 1 Else 0 End As IsLeaderShow"
                + Environment.NewLine + ", CASE WHEN t5.AssignmentStatus_pKey in(2,19) AND ISNULL(t2.NumModerators,0) >0  AND  ISNULL(PLM.Moderator,'TBD')<>'TBD' AND  ISNULL(PLM.Moderator,'')<>''  then 1 Else 0 End As IsModShow"
                + Environment.NewLine + ", IIF(t5.AssignmentStatus_pKey in(2,19),'True','False') as Visibility ,ISNULL(t4.pKey,0) AS EventSessionStaff_Pkey ,IIF(ISNULL(t4.Comment,'')<>'' , 'True','False') as DeleteShow"
                + Environment.NewLine + ", CASE WHEN ( Select Count(ESS.pKey) From EventSession_Staff ESS inner join account_List AL On AL.pKey = ESS.Account_pKey"
                + Environment.NewLine + "  Where  ESS.EventSession_pkey IN(Select pkey from Event_Sessions Where Event_pKey=" + intSpkrCurEventPKey.ToString() + ")  AND ESS.Account_pkey=t1.Account_pKey And (ISNULL(ESS.IsSessionChair,0)=1 OR  ISNULL(ESS.IsSpeaker,0)=1) ANd ESS.EventSession_pkey<>t3.pkey"
                + Environment.NewLine + "  AND  AL.pKey not in (select distinct Account_pKey from Event_Accounts where isnull(ParticipationStatus_pKey,0)=2 and Event_pKey=" + intSpkrCurEventPKey.ToString() + "))  >0 THEN 'False' Else 'True' END AS IsCancelReg"
                + Environment.NewLine + ", CASE WHEN (Select Count(*) from EventSession_Staff Where EventSession_pkey IN(Select pkey FROM event_Sessions Where event_pkey=" + intSpkrCurEventPKey.ToString() + ") AND ISNULL(IsSessionChair,0)=1 AND Account_pKey=" + intAcctPKey.ToString() + ")>0 THEN 1 ELSE 0 END AS IsAllowCancelReg"
                + Environment.NewLine + ", TRIM(',' FROM ISNULL(p.StatusCountwiseList,'')) as StatusCountwiseList , ISNULL(p.NeededSpeaker,'') as NeededSpeaker"
                + Environment.NewLine + ", ISNULL((Select Top 1  Convert(varchar, NextActionDate,101) FROM EventAccount_Communication Where UpdatedforAccount_pkey=t1.Account_pKey AND ISNULL(AdditionalFollowup,0)=0 and event_pkey=" + intSpkrCurEventPKey.ToString() + " Order by pkey DESC),'') As NextConDate"
                + Environment.NewLine + ", ISNULL(t15.Comments,'') as Comments , case when ISNull((Select Count(pkey) from Account_SessionHistory where AccountSession_pkey=t1.pkey GROUP by AccountSession_pkey ),0)>1 then 'True' else 'False' END As IsShowHistory"
                + Environment.NewLine + ", IIF(ISNULL(t4.RemoveFocus,0)=0 AND ISNULL(t4.Comment,'')<>'' AND ISNULL(t4.Comment,'')<>ISNULL(t15.Comments,'') ,ISNULL(t4.Comment,'')+IIF(ISNULL(t15.Comments,'')<>'',', ','')+ISNULL(t15.Comments,''),ISNULL(t15.Comments,'')) as Focus_and_Comments "
                + Environment.NewLine + ", ISNULL(t2.NumLeaders ,0) as NumLeaders, ISNULL(t2.NumModerators,0)  as NumModerators,CASE WHEN ISNULL(t4.IsSessionLeader,0)=1 OR ISNULL(t4.IsSessionLeader2,0)=1 then 1 else 0 END as IsSessionLeader"
                + Environment.NewLine + ", CASE WHEN ISNULL(t4.IsSessionModerator,0)=1 OR ISNULL(t4.IsSessionModerator2,0)=1 then 1 else 0 END as IsSessionModerator"
                + Environment.NewLine + ", ISNULL((Select  Sum(CASE WHEN ISNULL(ESS.IsSessionLeader,0)>0 OR ISNULL(ESS.IsSessionLeader2,0)>0 then 1 else 0 END  ) from eventSession_Staff ESS where ESS.EventSession_pkey= t3.pKey AND ESS.account_pkey<>" + intAcctPKey.ToString() + "),0) AS TotalLeaderAssigned"
                + Environment.NewLine + ", ISNULL((Select  Sum( CASE WHEN ISNULL(ESS.IsSessionModerator,0)>0 OR ISNULL(ESS.IsSessionModerator2,0)>0 then 1 else 0 END  ) from eventSession_Staff ESS where ESS.EventSession_pkey= t3.pKey AND ESS.account_pkey<>" + intAcctPKey.ToString() + "),0) AS TotalModeratorAssigned"
                + Environment.NewLine + ", ISNULL(t3.IsHideFromPublic, 0) as IsHideFromPublic  ,ISNULL(t5.homelession,'') as homelession From account_sessions t1"
                + Environment.NewLine + "  inner join session_List t2 On t2.pKey = t1.Session_pKey"
                + Environment.NewLine + "  Cross Apply dbo.Fn_SpeakerStatus_SessionWise(t1.Session_pKey, " + intSpkrCurEventPKey.ToString() + "," + intAcctPKey.ToString() + " ) as P"
                + Environment.NewLine + "  Cross Apply dbo.fu_showsuggested(t1.Account_pKey, " + intSpkrCurEventPKey.ToString() + ", t1.Session_pKey ," + intAcctPKey.ToString() + "," + intSpkrCurEventPKey.ToString() + ") as PS"
                + Environment.NewLine + "  Inner Join Event_Sessions t3 On t3.Session_pKey = t1.Session_pKey And t3.Event_pKey = " + intSpkrCurEventPKey.ToString()
                + Environment.NewLine + "  Cross Apply dbo.getEventSession_Leader_Moderator(t3.pkey,t2.NumLeaders,t2.NumModerators) as PLM"
                + Environment.NewLine + "  Inner Join event_list el on el.pkey = t3.event_pkey"
                + Environment.NewLine + "  Left Outer Join EventSession_Staff t4 On t4.EventSession_pKey = t3.pKey And t4.Account_pKey = t1.Account_pKey"
                + Environment.NewLine + "  Left Outer Join account_sessionEvents t5 On t5.AccountSession_pKey = t1.pKey And t5.Event_pKey = " + intSpkrCurEventPKey.ToString()
                + Environment.NewLine + "  Left outer join (Select AccountSession_pkey, max(pkey) As MaxPKey from Account_SessionHistory group by AccountSession_pkey) t14 On t14.AccountSession_pkey = t1.pKey"
                + Environment.NewLine + "  Left outer join Account_SessionHistory t15  On t15.pkey = t14.MaxPKey"
                + Environment.NewLine + "  Left Outer Join Sys_AssignmentStatuses t6 On t6.pKey = t5.AssignmentStatus_pKey"
                + Environment.NewLine + "  LEFT OUTER JOIN EventSession_Documents t7 ON t7.EventSession_pKey =t3.pkey AND t7.OwnerAccount_pKey in(" + intAcctPKey.ToString() + ") AND t7.SessionDocType_pKey=1"
                + Environment.NewLine + "  Where t1.Account_pKey = " + intAcctPKey.ToString()
                + Environment.NewLine + "  AND ISNULL(t1.IsNotInterested,0)=0 and isNull(t3.EventSessionStatus_pkey,0) <> " + (clsEventSession.STATUS_Cancelled).ToString();

                switch (ddSesStatus)
                {
                    case -1: qry  +=  Environment.NewLine + " AND ISNULL(t5.AssignmentStatus_pKey,0) NOT IN (12,23,7,19,11,2)"; break;
                    case -2: qry  +=  Environment.NewLine + " And (isNull(t6.ConsideredActive,0) = 0 and t5.AssignmentStatus_pKey is not Null)"; break;
                    case clsEventSession.ASSIGNSTATUS_Interested: qry  +=  Environment.NewLine + " And (t5.AssignmentStatus_pKey is Null or t5.AssignmentStatus_pKey =  " + ddSesStatus.ToString(); break;
                }

                if (ddSesStatus>0)
                    qry  +=  Environment.NewLine + " And t5.AssignmentStatus_pKey = " +  ddSesStatus.ToString();

                if (acSessionpKey>0)
                    qry  +=  Environment.NewLine + "  And t1.pKey= " +  acSessionpKey.ToString();


                qry  +=  Environment.NewLine + " Order by SessionID";

                return SqlHelper.ExecuteTable(qry, CommandType.Text, null);
            }
            catch (Exception ex)
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 0, "Error filling datalist For speaker");
                dt =null;
            }
            return dt;
        }
        public DataTable getSpeakerDetailsInformation(int AccountID)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@AccountID", AccountID) };
                return SqlHelper.ExecuteTable("getMVCSpeakerDetails", CommandType.StoredProcedure, parameters);
            }
            catch
            {
            }
            return null;
        }
        public DataTable getSpeakerDetailsPeopleInformation(int intSpkrCurEventPKey, int intSessionPkey, string ddlInterested, int ddAccStatus, int ddSpkrFlag, int ddDateRange, string dtPStart, string dtPEnd)
        {
            DataTable dt = new DataTable();
            try
            {
                string qry = " Select t1.pKey As Account_pKey, Case WHEN t2.Priority=9 AND t3.AssignmentStatus_pkey IS NULL THEN ISNULL(t3.AssignmentStatus_pkey,27) ELSE ISNULL(t3.AssignmentStatus_pkey,21) END  as AssignmentStatus_pkey , t1.ContactName, t1.Title, iif(t4.OrganizationID is null or t4.OrganizationID='','(None)',t4.OrganizationID) as OrgID"
                    + Environment.NewLine + ", Case WHEN t2.Priority=9 AND t6.AssignmentStatusID IS NULL THEN  isNull(t6.AssignmentStatusID,'Interested?') ELSE isNull(t6.AssignmentStatusID,'Interested') END as Status,t7.pkey as EventAcct_PKey,t2.pKey as AcctSesPKey,t9.EventSessionStatus_pkey,t9.pKey as EvtSesPKey,case when isnull(t3.CommentCount,0)>1 then 'True' else 'False' END As IsShowHistory"
                    + Environment.NewLine + ", ISNULL(P.List,'') AS List,isNull(af.SpkrFlag_pKey,0) As SpkrFlag ,ISNULL(t8.NumSpeakers,0) as NumSpeakers "
                    + Environment.NewLine + ", Case When LEN(ISNULL(t17.DueDate_TBD,0))>0 AND t17.DueDate_TBD <>'0' Then DueDate_TBD  Else (Case when  t17.DueDate Is Not Null then  Convert(varchar, t17.DueDate,101) else ''END) END  As DueDate"
                    + Environment.NewLine + ", IIF(ISNULL(t16.RemoveFocus,0)=0 AND ISNULL(t16.Comment,'')<>'' AND ISNULL(t16.Comment,'')<>ISNULL(t15.Comments,'') ,ISNULL(t16.Comment,'')+ IIF(ISNULL(t15.Comments,'')<>'',', ','')+ISNULL(t15.Comments,''),ISNULL(t15.Comments,'')) as Comments "
                    + Environment.NewLine + ", ISNULL((Select Top 1  Convert(varchar, NextActionDate,101) FROM EventAccount_Communication Where UpdatedforAccount_pkey=t1.pkey AND ISNULL(AdditionalFollowup,0)=0 and event_pkey=" + intSpkrCurEventPKey.ToString() + " Order by pkey DESC),'') As NextConDate"
                    + Environment.NewLine + ", TRIM(',' FROM ISNULL(p1.StatusCountwiseList,'')) as StatusCountwiseList , ISNULL(p1.NeededSpeaker,'') as NeededSpeaker"
                    + Environment.NewLine + ", iif( len(t15.Comments) >35,LEFT(ISNULL(t15.Comments,''),35)+'...',LEFT(ISNULL(t15.Comments,''),35)) as Comment ,ISNULL(t15.Comments,'') as CompleteComm"
                    + Environment.NewLine + ", ISNULL(t8.NumLeaders ,0) as NumLeaders, ISNULL(t8.NumModerators,0)  as NumModerators"
                    + Environment.NewLine + ", CASE WHEN ISNULL(t16.IsSessionLeader,0)=1 OR ISNULL(t16.IsSessionLeader2,0)=1 then 1 else 0 END as IsSessionLeader"
                    + Environment.NewLine + ", CASE WHEN ISNULL(t16.IsSessionModerator,0)=1 OR ISNULL(t16.IsSessionModerator2,0)=1 then 1 else 0 END as IsSessionModerator"
                    + Environment.NewLine + ", ISNULL((Select  Sum(CASE WHEN ISNULL(ESS.IsSessionLeader,0)>0 OR ISNULL(ESS.IsSessionLeader2,0)>0 then 1 else 0 END  ) from eventSession_Staff ESS where ESS.EventSession_pkey= t9.pKey AND ESS.account_pkey<>t1.pkey),0) AS TotalLeaderAssigned"
                    + Environment.NewLine + ", ISNULL((Select  Sum( CASE WHEN ISNULL(ESS.IsSessionModerator,0)>0 OR ISNULL(ESS.IsSessionModerator2,0)>0 then 1 else 0 END  ) from eventSession_Staff ESS where ESS.EventSession_pkey= t9.pKey AND ESS.account_pkey<>t1.pkey),0) AS TotalModeratorAssigned"
                    + Environment.NewLine + ", ISNULL(t3.homelession,'') as homelession";

                if (ddDateRange>0)
                    qry += Environment.NewLine + ", CASE WHEN  (t1.LastSpeakerProfileUpdate >=@PStart Or  t1.LastProfileUpdate >=@PStart) And (t1.LastSpeakerProfileUpdate <=@PEnd  Or  t1.LastProfileUpdate <=@PEnd) THEN 0 ELSE 1 END As UnUpdatedSpeaker";
                else
                    qry += Environment.NewLine + ", 0 as UnUpdatedSpeaker";

                qry += Environment.NewLine + " From Account_List t1 Cross Apply dbo.Fn_SpeakerStatus(t1.pKey  , " + intSpkrCurEventPKey.ToString() + ") P   "
                    + Environment.NewLine + " Inner Join Account_Sessions t2 on t2.Account_pkey = t1.pKey      "
                    + Environment.NewLine + " Left Outer Join Account_SessionEvents t3 on t3.AccountSession_pkey = t2.pKey And t3.Event_pkey = " + intSpkrCurEventPKey.ToString()
                    + Environment.NewLine + "  Left outer Join Organization_List t4 on t4.pkey = t1.ParentOrganization_pKey"
                    + Environment.NewLine + " Left outer Join Sys_States t5 on t5.pkey = t1.State_pKey "
                    + Environment.NewLine + " Left outer Join Sys_AssignmentStatuses t6 on t6.pkey = t3.AssignmentStatus_pkey"
                    + Environment.NewLine + " Left Outer Join Event_Accounts t7 on t7.account_pkey = t1.pkey And t7.event_pkey =" + intSpkrCurEventPKey.ToString()
                    + Environment.NewLine + " Inner Join Session_List t8 on t8.pkey = t2.Session_pkey"
                    + Environment.NewLine + " Inner Join Event_Sessions t9 On t9.Session_pKey = t2.Session_pKey And t9.Event_pKey =" + intSpkrCurEventPKey.ToString()
                    + Environment.NewLine + " Left outer join Account_flags af on af.account_pkey = t1.pkey And af.event_pkey =   "+ intSpkrCurEventPKey.ToString()
                    + Environment.NewLine + " LEFT OUTER JOIN EventSession_Documents t17 ON t17.EventSession_pKey =t9.pkey AND t17.OwnerAccount_pKey=t1.pKey AND t17.SessionDocType_pKey=1 "
                    + Environment.NewLine + " Left outer join (Select AccountSession_pkey, max(pkey) As MaxPKey from Account_SessionHistory group by AccountSession_pkey) t14 On t14.AccountSession_pkey = t2.pKey"
                    + Environment.NewLine + " Left outer join Account_SessionHistory t15  On t15.pkey = t14.MaxPKey"
                    + Environment.NewLine + " Left Outer Join EventSession_Staff t16 On t16.EventSession_pKey = t9.pKey And t16.Account_pKey = t1.pkey"
                    + Environment.NewLine + " Cross Apply dbo.Fn_SpeakerStatus_SessionWise(t2.Session_pKey, " + intSpkrCurEventPKey.ToString() + ",t1.pkey ) as P1"
                    + Environment.NewLine + " Where ISNULL(t2.IsNotInterested,0)=0 AND t2.Session_pKey = " + intSessionPkey.ToString();

                if (!string.IsNullOrEmpty(ddlInterested))
                    qry +=Environment.NewLine + " AND( t1.pkey in(select account_pkey from Account_ExpressionProfile where event_pkey = " +  intSpkrCurEventPKey.ToString() + " ))";

                if (ddAccStatus >0)
                    qry +=Environment.NewLine + " AND t1.AccountStatus_pkey =" + ddAccStatus.ToString();

                if (ddSpkrFlag != -1)
                {
                    if (ddSpkrFlag == -2)
                        qry +=Environment.NewLine + " and af.account_pkey in (Select  account_pkey from Account_flags where  account_pkey = t1.pkey and SpkrFlag_pKey>0 AND Account_flags.event_pkey in (" + intSpkrCurEventPKey.ToString() + " ))";
                    else if (ddSpkrFlag ==0)
                        qry +=Environment.NewLine + "  and af.account_pkey Not in (Select  account_pkey from Account_flags where  account_pkey = t1.pkey and (SpkrFlag_pKey>0 and SpkrFlag_pkey <> 25) AND event_pkey in ( " + intSpkrCurEventPKey.ToString() + " ))";
                    else
                        qry +=Environment.NewLine + " AND af.event_pkey=" + intSpkrCurEventPKey.ToString() + " AND  af.SpkrFlag_pKey IN (" + ddSpkrFlag.ToString().TrimEnd(',') + ")";
                }
                qry +=Environment.NewLine + " Order by t1.ContactName";


                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@PStart", dtPStart), new SqlParameter("@PEnd", dtPEnd) };

                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 130, ex.Message.ToString(), Msg: ex.Message);
            }

            return dt;
        }
        public DataSet BindActivity(int intActiveEventPkey, int intPendingAcctPKey)
        {
            DataSet ds = new DataSet();
            try
            {
                StringBuilder sb = new StringBuilder("SELECT t0.pKey, isnull(t0.Track_Prefix,'')+isnull(t1.SessionID,'') as strText From Event_Sessions t0");
                sb.Append(Environment.NewLine + " Inner Join Session_List t1 on t1.pKey = t0.Session_pkey");
                sb.Append(Environment.NewLine + " LEFT OUTER JOIN Sys_Tracks t3 ON t3.pkey=t1.Track_pKey");
                sb.Append(Environment.NewLine + " Where  t3.Educational=1 and t0.Event_pKey = " + intActiveEventPkey.ToString());
                sb.Append(Environment.NewLine + " Order by strText");
                sb.Append(Environment.NewLine + " Select top 1 ISNULL(t0.EventSession_pkey,0) as EventSession_pkey  from EventSession_Staff t0");
                sb.Append(Environment.NewLine + " INNER JOIN  Event_Sessions t1 ON t1.pkey= t0.EventSession_pkey Where t1.Event_pKey=" + intActiveEventPkey.ToString() + " AND Account_pkey= " + intPendingAcctPKey.ToString());
                sb.Append(Environment.NewLine + " AND ISNULL(IsSpeaker,0)=1 ORDER by t0.pKey DESC");

                ds = SqlHelper.ExecuteSet(sb.ToString(), CommandType.Text, null);
            }
            catch (Exception ex)
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 0, "Error loading Activity dropdown list ");
            }
            return ds;
        }
        public DataTable BindAnnouncement(string Page)
        {
            try
            {
                Page = Page.Replace("ascx.forms.", "");
                string qry = "SELECT CAST(t1.pKey AS varchar) +'_'+cast(ISNULL(t1.ForActivity,0)as varchar) as pKey, '('+ CAST(t1.pKey As varchar) + ') ' + t1.Title as strText,ISNULL(ForActivity,0) as ForActivity"
                    +Environment.NewLine +"FROM Announcement_List t1"
                    +Environment.NewLine +"Where t1.AnnouncementStatus_pKey = " + clsAnnouncement.STATUS_ACTIVE.ToString()
                    +Environment.NewLine +" and (t1.pkey in(select x1.announcement_pkey from Announcement_pages x1 inner join sys_AnnouncementPages x2 on x2.pkey = x1.announcementpage_pkey where x2.AspxPage = '" + clsUtility.NoQuotes(Page) + "'))"
                    +Environment.NewLine +" Order by t1.Title";

                return SqlHelper.ExecuteTable(qry, CommandType.Text, null);
            }
            catch
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 0, "Error loading dropdown list ");
            }
            return null;
        }
        public void Note_Insert_Update(int intPendingAcctPKey, int IntCurrentevent_pkey, int intAccountPKey, string msg, int intPendingNotePkey = 0, string Qtype = "InsertUpdateNotes", int FinalDisposition_pkey = 0, string NoteText = "", int Followupright_Pkey = 0)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[] {
                     new SqlParameter("@Note_Pkey" ,  intPendingNotePkey),
                     new SqlParameter("@Account_pkey",  intPendingAcctPKey),
                     new SqlParameter("@Event_pKey", IntCurrentevent_pkey),
                     new SqlParameter("@AuthorAccount_pKey",intAccountPKey),
                     new SqlParameter("@UpdateAccount_pKey",intAccountPKey),
                     new SqlParameter("@NoteText",  NoteText.Trim()),
                     new SqlParameter("@QueryType", Qtype),
                     new SqlParameter("@type", "Note"),
                     new SqlParameter("@FinalDisposition_pkey",FinalDisposition_pkey),
                     new SqlParameter("@Followupright_Pkey",Followupright_Pkey)
                };
                SqlHelper.ExecuteNonQuery("Update_spkrflagMVC", CommandType.StoredProcedure, parameters);
            }
            catch
            {

            }
        }
        public bool GetNumSpeakers(int EventpKey, string AccountPKey)
        {
            try
            {

                string qry = "select top 1 c.pKey as NumSpeak from account_charges c  where account_pkey = @AccountPKey AND  c.event_pkey = @EventpKey   And IsNull(c.Reversed, 0) = 0 and c.ReversalReference is Null AND isnull(c.IsDelete, 0)=0 AND ChargeType_pKey = 1 Group By c.Account_pKey";
                SqlParameter[] parameters = new SqlParameter[] {
                     new SqlParameter("@AccountPKey",  AccountPKey),
                     new SqlParameter("@EventpKey", EventpKey),
            };
                DataTable dt = SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
                if (dt!= null && dt.Rows.Count>0)
                    return (dt.Rows.Count>0);
            }
            catch (Exception ex)
            {

            }
            return false;
        }
        public DataTable GetSpeakerDataByID(int SpkrCurEventPKey, int AccountID, int EventAccountpKey)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@SpkrCurEventPKey", SpkrCurEventPKey),
                    new SqlParameter("@AccountpKey", AccountID) ,
                    new SqlParameter("@EventAccountPKey", EventAccountpKey) ,
                };
                return SqlHelper.ExecuteTable("GetSpeakerDataByID_MVC", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
            }
            return null;
        }
        public string UpdateSpeaker_Focus(int PendingAccountPKey, int EventSessionPKey, string text)
        {
            try
            {
                string qry = "Update EventSession_Staff Set RemoveFocus = 1 Where eventsession_pkey = @EventSessionPKey and account_pKey = @PendingAccountPKey";
                SqlParameter[] parameters = new SqlParameter[] {
                     new SqlParameter("@PendingAccountPKey",  PendingAccountPKey),
                     new SqlParameter("@EventSessionPKey", EventSessionPKey),
                };
                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
                return "OK";
            }
            catch
            {
                return "Error updating focus";
            }
        }
        public void Update_Both_Dates(int intAccount_PKey, bool NextCondate = false, bool DueDateDate = false)
        {
            try
            {
                string qry = "Update Account_List Set Last_NextConDate= @NextConDate , Last_SlideDueDate=@dueDate where pkey =  @AccountID";
                SqlParameter[] parameters = new SqlParameter[] {
                     new SqlParameter("@NextConDate",  NextCondate),
                     new SqlParameter("@dueDate", DueDateDate),
                     new SqlParameter("@AccountID", intAccount_PKey),
                };
                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
            }
            catch
            {

            }
        }
        public bool UpdateSpeakerContact(int intPendingNotePkey, int SpeakerContact, int PendingAccountPKey, int EventPKey, int AccountPkey, string ddSpeakerContact)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Note_Pkey",intPendingNotePkey),
                     new SqlParameter("@Account_pkey", PendingAccountPKey),
                     new SqlParameter("@Event_pKey", EventPKey),
                     new SqlParameter("@AuthorAccount_pKey",AccountPkey),
                     new SqlParameter("@UpdateAccount_pKey",AccountPkey),
                     new SqlParameter("@NoteText",  ddSpeakerContact.Trim()),
                     new SqlParameter("@Followupright_Pkey", SpeakerContact),
                     new SqlParameter("@QueryType", "FOLLOWUPRIGHTS"),
                     new SqlParameter("@type",  "Rights"),
                };
                SqlHelper.ExecuteNonQuery("Update_spkrflagMVC", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public void UpdateSpeakerEqualizer(int PendingSesionPKey, int EventPKey)
        {
            try
            {
                string qry = " UPDATE ES SET ES.SpeakerEqualizer = 0 FROM Event_Sessions AS ES WHERE ES.Session_pKey=@PendingSesionPKey and ES.Event_pKey = @EventPKey";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@PendingSesionPKey",PendingSesionPKey),
                    new SqlParameter("@EventPKey", EventPKey),
                };
                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {

            }
        }
        public DataTable GetAllOtherSession_Spot(int SpkrCurEventPKey, int AccountID, int Session_pkey = 0, string Qtype = "OtherSpot")
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@Event_pkey", SpkrCurEventPKey),
                    new SqlParameter("@Account_pkey", AccountID) ,
                    new SqlParameter("@Session_pkey", Session_pkey),
                    new SqlParameter("@QueryType", Qtype),
                };
                return SqlHelper.ExecuteTable("GETOtherSession", CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
            }
            return null;
        }
        public DataTable RefreshHistory(int offSet, int intTypePKey, int intEntityPKey, string strCallingEntity, bool ShowFlagHistory = false)
        {
            try
            {
                StringBuilder sb = new StringBuilder("select t1.pKey, isNull(t2.ContactName,'Not Specified') as UpdatedBy, dateadd(minute,-@Offset,t1.UpdatedOn) as UpdatedOn,CASE WHEN t1.AuditEntryType_pKey =14 then REPLACE( replace( t1.Change,'<b>',''),'</b>','') else  t1.Change end as  Change , t1.SurrogateAccount_Pkey");
                sb.Append(Environment.NewLine +" From Audit_Log t1 Left outer join Account_List t2 on t2.pkey = t1.UpdatedByAccount_pKey");
                sb.Append(Environment.NewLine +" Where t1.EntityType_pKey = @EntityType and t1.Entity_pKey = @Entity and (t1.SurrogateAccount_Pkey is null or t1.SurrogateAccount_Pkey=0)");
                if (ShowFlagHistory)
                {
                    sb.Append(Environment.NewLine +" Union ");
                    sb.Append(Environment.NewLine +" Select PAF.pKey, isNull(t6.ContactName,'Not Specified') as UpdatedBy ,dateadd(minute,-@Offset, t5.NoteDate) as UpdatedOn,");
                    sb.Append(Environment.NewLine +" EL.EventID + ', ' + SF.SpkrFlagID + ', '+ISNULL(t5.NoteText,'') As Change  ,t5.Account_pKey as  SurrogateAccount_Pkey from Account_flags PAF");
                    sb.Append(Environment.NewLine +" LEFT JOIN SYS_SpkrFlags SF ON SF.pKey=PAF.SpkrFlag_pKey   LEFT JOIN event_List EL ON PAF.event_pkey=EL.pkey");
                    sb.Append(Environment.NewLine +" Left outer join (Select Account_pkey ,event_pkey, max(pkey) As MaxPKey from account_flags_notes Where Type='flag' group by Account_pkey, event_pkey) t4 On t4.Account_pkey = PAF.account_pkey AND t4.event_pkey=PAF.event_pkey");
                    sb.Append(Environment.NewLine +" LEFT JOIN account_flags_notes t5 ON t5.pkey=t4.MaxPKey  LEFT OUTER JOIN Account_list t6 on t6.pkey=t5.AuthorAccount_pKey");
                    sb.Append(Environment.NewLine +" where ISNULL(PAF.SpkrFlag_pKey,0)>0   AND PAF.account_pkey= @Entity");
                }
                sb.Append(Environment.NewLine +" Order by UpdatedOn desc");
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@EntityType", intTypePKey),
                    new SqlParameter("@Entity", intEntityPKey) ,
                    new SqlParameter("@Offset", offSet),
                };
                return SqlHelper.ExecuteTable(sb.ToString(), CommandType.Text, parameters);
            }
            catch
            {
                clsUtility.LogErrorMessage(null, null, strCallingEntity, 126, "");
            }
            return null;
        }
        public DataTable RefreshFlagHistory(int Account_pkey)
        {
            try
            {
                StringBuilder sb = new StringBuilder(" Select PAF.pKey, EL.EventID, SF.SpkrFlagID ,ISNULL(t5.NoteText,'') As Comments from Account_flags PAF");
                sb.Append(Environment.NewLine +" LEFT JOIN SYS_SpkrFlags SF ON SF.pKey=PAF.SpkrFlag_pKey");
                sb.Append(Environment.NewLine +" LEFT JOIN event_List EL ON PAF.event_pkey=EL.pkey");
                sb.Append(Environment.NewLine +" Left outer join (Select Account_pkey ,event_pkey, max(pkey) As MaxPKey from account_flags_notes  Where Type='flag' group by Account_pkey, event_pkey) t4 On t4.Account_pkey = PAF.account_pkey AND  t4.event_pkey=PAF.event_pkey");
                sb.Append(Environment.NewLine +" LEFT JOIN account_flags_notes t5 ON t5.pkey=t4.MaxPKey ");
                sb.Append(Environment.NewLine +" where ISNULL(PAF.SpkrFlag_pKey,0)>0   AND PAF.account_pkey= @Account_pkey");
                sb.Append(Environment.NewLine +" ORDER BY PAF.event_pkey DESC ");
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@Account_pkey", Account_pkey),
                };
                return SqlHelper.ExecuteTable(sb.ToString(), CommandType.Text, parameters);
            }
            catch
            {
            }
            return null;
        }
        public DataTable RefreshExpressList(int Account_pkey)
        {
            try
            {
                StringBuilder sb = new StringBuilder(" select t1.pKey, t1.EventID, (Case when t2.pKey > 0 Then 1 Else 0 End) as bChecked From Event_List t1");
                sb.Append(Environment.NewLine +" Left outer join Account_ExpressionProfile t2 on t2.Event_pkey = t1.pKey and t2.Account_pKey = @Account_pkey");
                sb.Append(Environment.NewLine +" Where t1.StartDate >= getdate() Order by t1.StartDate");
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@Account_pkey", Account_pkey),
                };
                return SqlHelper.ExecuteTable(sb.ToString(), CommandType.Text, parameters);
            }
            catch
            {
            }
            return null;
        }
        public bool UpdateProducerReport(int PendingID, string Report)
        {
            try
            {
                string qry = "Update Account_List set ProducerReport_Selection = @Selections where pkey = @PendingID";
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@Selections", Report),
                    new SqlParameter("@PendingID", PendingID),
                };
                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool ToggleFeatured(int PendingID, bool PrioritySpkr)
        {
            try
            {
                string qry = "Update Account_List set PrioritySpeaker =  @bFeatured where pkey = @PendingID";
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@bFeatured", ((PrioritySpkr)? 0:1)),
                    new SqlParameter("@PendingID", PendingID),
                };
                SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public DataTable RefreshNotes(int PendingID, int EventPKey, bool IsshowCommentEventFree)
        {
            try
            {
                string qry = "select n.pkey, n.NoteDate, n.NoteText, isNull(u.ContactName,'NA') as NoteBy, isNull(u2.ContactName,'NA') as UpdateBy, n.UpdateDate"
                    + Environment.NewLine + " From Account_Flags_Notes n"
                    + Environment.NewLine + " Left outer join Account_List u on u.pkey = n.AuthorAccount_pKey"
                    + Environment.NewLine + " Left outer join Account_List u2 on u2.pkey = n.UpdateAccount_pKey"
                    + Environment.NewLine + " Where n.Type='Notes' AND  n.Account_pKey = @PendingID ";

                if (!IsshowCommentEventFree)
                    qry += Environment.NewLine +  " AND n.Event_pKey=@EventID";

                qry += Environment.NewLine +  "  Order by n.NoteDate DESC";

                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@Event_pKey", EventPKey),
                    new SqlParameter("@PendingID", PendingID),
                };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 128, "");
            }
            return null;
        }
        public string BindPreviousNotes(int PendingID, int EventPKey)
        {
            string result = "";
            try
            {
                string qry = "select  top 1 n.pkey, n.NoteDate, n.NoteText, n.UpdateDate From Account_Flags_Notes n"
                    + Environment.NewLine + " Where n.Type='Notes' AND  n.Account_pKey = @PendingID AND n.Event_pKey=@EventID  Order by n.NoteDate DESC";
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@EventID", EventPKey),
                    new SqlParameter("@PendingID", PendingID),
                };
                DataTable dt = SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
                if (dt != null  && dt.Rows.Count>0 && dt.Rows[0]["NoteText"] != DBNull.Value)
                    return dt.Rows[0]["NoteText"].ToString();
            }
            catch
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 128, "");
            }
            return result;
        }
        public string UpdateExpressionList(int PendingID, string intPKey, bool Pending)
        {
            try
            {
                string qry = "if not exists(select 1 from Account_ExpressionProfile where Event_pkey = @EvtPKey and Account_pKey = @PendingID )"
                    + Environment.NewLine + " Insert Into Account_ExpressionProfile (Account_pKey, Event_pkey,ExpressionDate)"
                    + Environment.NewLine + " Values (@PendingID,@EvtPKey," + ((Pending) ? "null)" : "@Date)");

                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@EvtPKey", intPKey),
                    new SqlParameter("@PendingID", PendingID),
                    new SqlParameter("@Date", DateTime.Now.ToString()),
                };
                if (SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters))
                    return "OK";
                //If Not clsUtility.ExecuteQuery(cmd, Me.lblMsg, ) Then Exit Sub.
            }
            catch (Exception ex)
            {

            }
            return "Error Occurred Whiile Adding Event to account";
        }
        public bool DeleteExpressionList(int PendingID, string ExpressionList)
        {
            try
            {
                string qry = "Delete from Account_ExpressionProfile where Account_pKey = @ID and not Event_pKey in (@Selected)";
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@Selected", ExpressionList),
                    new SqlParameter("@ID", PendingID)
                };
                if (SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters))
                    return true;
            }
            catch
            {
            }
            return false;
        }
        public bool UpdateAccountPrioritySpeaker(int PendingID, bool PrioritySpeaker)
        {
            try
            {
                string qry = "Update Account_List Set PrioritySpeaker =  @PrioritySpeaker where pKey =  @AccountId";
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@PrioritySpeaker", PrioritySpeaker),
                    new SqlParameter("@AccountId", PendingID)
                };
                if (SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters))
                    return true;
            }
            catch
            {
            }
            return false;
        }
        public void FollowUpUpdate(int intPendingAcctPKey, int IntCurrentevent_pkey, int intAccountPKey, string NoteText, int Followupright_Pkey)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                     new SqlParameter("@Note_Pkey",0),
                     new SqlParameter("@Account_pkey", intPendingAcctPKey),
                     new SqlParameter("@Event_pKey", IntCurrentevent_pkey),
                     new SqlParameter("@AuthorAccount_pKey",intAccountPKey),
                     new SqlParameter("@UpdateAccount_pKey",intAccountPKey),
                     new SqlParameter("@SpkrFlag_pKey",0),
                     new SqlParameter("@NoteText",  NoteText.Trim()),
                     new SqlParameter("@QueryType", "FOLLOWUPRIGHTS"),
                     new SqlParameter("@type", "Rights"),
                     new SqlParameter("@FinalDisposition_pkey",0),
                     new SqlParameter("@Followupright_Pkey",Followupright_Pkey)
                };
                SqlHelper.ExecuteNonQuery("Update_spkrflagMVC", CommandType.StoredProcedure, parameters);
            }
            catch
            {

            }
        }
        public DataTable GetProfileBio(int PendingID)
        {
            try
            {
                string qry = "Select ContactName ,Firstname,Lastname,Title,PersonalBio,AboutMe,ISNULL(CVFilename,'') As CVFilename,ISNULL(Comment,'') as Comment from Account_List where pkey= @AccountID";
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@AccountID", PendingID) };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 128, "");
            }
            return null;
        }
        public bool SaveBioUpdate(int PendingID, string PersonalBioview, string Aboutview, string ProfileComment)
        {
            try
            {
                string qry = " Update Account_List set PersonalBio =@PersonalBio,AboutMe=@AboutMe,Comment=@Comment where pkey = @PendingID";
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@PendingID", PendingID),
                    new SqlParameter("@PersonalBio", PersonalBioview),
                    new SqlParameter("@AboutMe", Aboutview),
                    new SqlParameter("@Comment", ProfileComment),
                };
                return SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
            }
            catch
            {
                return false;
            }
        }
        public DataTable ActivityList(int PendingID, int EventID)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@Account_pkey", PendingID),
                    new SqlParameter("@Event_pkey", EventID),
                };
                return SqlHelper.ExecuteTable("Activity_List_MVC", CommandType.StoredProcedure, parameters);
            }
            catch
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 128, "");
            }
            return null;
        }
        public bool AddActivityToInterest(int EventID, int ddSecActivity, int PendingAccount, int AccountID, bool staffMember)
        {
            try
            {
                string qry = "if not exists(select * from Account_Sessions where Session_pkey = @SessionPKey and Account_pKey =  @PendingID)"
                    + Environment.NewLine + " BEGIN"
                    + Environment.NewLine + " Insert Into Account_Sessions (Account_pKey, Priority, Session_pKey,UpdatedBy_pKey,UpdatedBy_Date,Event_pkey)"
                    + Environment.NewLine + " Values (@PendingID, 9,@SessionPKey, @AccountID, GetDate(),@EventID)"
                    + ((!(PendingAccount != AccountID && staffMember)) ? Environment.NewLine + " Update Account_List SET LastSpeakerProfileUpdate=GETDATE() Where Pkey=@PendingID" : "")
                    + Environment.NewLine + "END "
                    + Environment.NewLine + " ELSE IF exists(select * from Account_Sessions where Session_pkey = @SessionPKey and Account_pKey = @PendingID  AND  ISNULL(IsNotInterested,0)=1) "
                    + Environment.NewLine + " BEGIN"
                    + Environment.NewLine + " Update Account_Sessions SET IsNotInterested=0 where Session_pkey = @SessionPKey and Account_pKey = @PendingID "
                    + ((!(PendingAccount != AccountID && staffMember)) ? Environment.NewLine + " Update Account_List SET LastSpeakerProfileUpdate=GETDATE() Where Pkey = @PendingID" : "")
                    + Environment.NewLine + " END";
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@PendingID", PendingAccount),
                    new SqlParameter("@SessionPKey", ddSecActivity),
                    new SqlParameter("@AccountID", PendingAccount),
                    new SqlParameter("@EventID", EventID)
                };
                return SqlHelper.ExecuteNonQuery(qry, CommandType.Text, parameters);
            }
            catch
            {
            }
            return false;
        }
        public DataTable GetSessionPriorities(int pendingID, int EventID)
        {
            try
            {
                string qry = "Select dbo.getSessionPriorities_Color(@PendingID,@EventID) as str";
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@PendingID", pendingID),
                    new SqlParameter("@EventID", EventID)
                };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch
            {
            }
            return null;
        }
        public bool SaveHomeLesson(int intPendingAcctSesPKey, int intSpkrCurEventPKey, int intPendingStatusPKey, string takehomeLession)
        {

            try
            {
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@AccountSession_pkey", intPendingAcctSesPKey),
                    new SqlParameter("@Event_pkey", intSpkrCurEventPKey),
                    new SqlParameter("@AssignmentStatus_pkey", intPendingStatusPKey),
                    new SqlParameter("@HomeLession", takehomeLession)
                };
                return SqlHelper.ExecuteNonQuery("HomeLession_Insert_Update", CommandType.StoredProcedure, parameters);
            }
            catch
            {

            }
            return false;
        }
        public DataTable GetcbAccountStaff(int EventID, int intPendingEvtSesPKey, int PendingAccountPKey, bool IsHeader)
        {
            try
            {
                string qry = "SElect '' as pkey , 'Staff' as  ContactName ,'True' as Header,-2 as shortorder  UNION ALL"
                    + Environment.NewLine + " Select  t1.Email as pkey ,t1.ContactName ,'False' as Header ,-1 as shortorder  from Event_Staff t0 INNER JOIN Account_List t1 ON t0.Account_pKey=t1.pkey Where t0.Event_pkey= @Event_pKey"
                    + Environment.NewLine + " UNION ALL SElect '' as pkey , 'Speakers' as  ContactName ,'True' as Header ,0 as shortorder  UNION ALL "
                    + Environment.NewLine + " Select t1.Email as pkey ,t1.ContactName ,'False' as Header,1 as shortorder   from EventSession_Staff t0 INNER JOIN Account_List t1 ON t1.pkey=t0.Account_pKey"
                    + Environment.NewLine + " Where t0.EventSession_pkey= @PendingEvsPKey  AND t0.Account_pKey <> @PendingAccountPKey AND ISNULL(t0.IsSpeaker,0)=1 ORDER BY shortorder";

                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@PendingEvsPKey", intPendingEvtSesPKey),
                    new SqlParameter("@PendingAccountPKey", PendingAccountPKey),
                    new SqlParameter("@Event_pKey", EventID),
                };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch
            {
            }
            return null;
        }
        public DataTable RefreshSpeakerSendEmail(SessionSpeakerStatus Model, bool ishow, bool IsSlideCreate)
        {
            try
            {
                string qry = "Select t1.pKey as Account_pkey,t1.ContactName as Name ,t2.Pkey as Account_SessionsPkey,(t1.ContactName+(Case when (t5.IsSessionModerator=1 OR t5.IsSessionModerator2=1) and (t5.IsSessionLeader=1 or t5.IsSessionLeader2=1) then ' [LM]' when (t5.IsSessionModerator=1 or t5.IsSessionModerator2=1) then ' [M]' when (t5.IsSessionLeader=1 or t5.IsSessionLeader2=1) then ' [L]' else '' end)) as ContactName"
                                     + Environment.NewLine + " ,IIF(t1.pkey = @intPendingAcctPKey,'True','False') As IsChecked ,@ishow As SlideShow, IIF((t1.pkey = @intPendingAcctPKey AND @IsSlideCreate='True'),'True','False') as IsSlideCreate"
                                     + Environment.NewLine + " ,t7.pkey as EventAccount_pKey,t8.pkey as eventSession_pkey From Account_List t1"
                                     + Environment.NewLine + " Inner Join Account_Sessions t2 On t2.Account_pkey = t1.pKey"
                                     + Environment.NewLine + " Inner Join Event_Sessions t8 On t8.Session_pKey = t2.Session_pKey And t8.Event_pKey = @intCurEventPKey"
                                     + Environment.NewLine + " Left Outer Join Account_SessionEvents t3 On t3.AccountSession_pkey = t2.pKey And t3.Event_pkey = @intCurEventPKey"
                                     + Environment.NewLine + " Left outer Join Organization_List t4 On t4.pkey = t1.ParentOrganization_pKey"
                                     + Environment.NewLine + " Left outer Join Sys_AssignmentStatuses t6 On t6.pkey = t3.AssignmentStatus_pkey"
                                     + Environment.NewLine + " Left Outer Join Event_Accounts t7 On t7.account_pkey = t1.pkey And t7.event_pkey =  @intCurEventPKey"
                                     + Environment.NewLine + " left outer join (Select account_pkey,IsSessionModerator,IsSessionModerator2,IsSessionLeader,IsSessionLeader2 from Eventsession_staff where  (ISNULL(IsSessionChair,0)<>0 OR ISNULL(IsSpeaker,0)<>0)  AND  eventsession_pkey In (Select pkey from Event_Sessions where Session_pKey= @intPendingSessionPKey"
                                     + Environment.NewLine + " And event_pkey= @intCurEventPKey))t5 On t5.account_pkey=t1.pKey"
                                     + Environment.NewLine + " Where   t2.Session_pKey = @intPendingSessionPKey";
                if (Model.intStatus_pkey == clsEventSession.ASSIGNSTATUS_Launched)
                    qry = qry + Environment.NewLine + " And (ISNULL(t3.AssignmentStatus_pkey,16)="  + clsEventSession.ASSIGNSTATUS_Assigned.ToString() + " Or (ISNULL(t3.AssignmentStatus_pkey,16)=" + clsEventSession.ASSIGNSTATUS_Launched.ToString() + "  )) AND t3.Event_pkey = @intCurEventPKey";
                else
                {
                    qry = qry + Environment.NewLine + " And t1.pkey = @intPendingAcctPKey";
                    if (Model.intStatus_pkey != 3)
                        qry = qry + Environment.NewLine + " And t3.AssignmentStatus_pkey Is Not null  AND NOT EXISTS (select Account_pKey from Event_Accounts where isnull(ParticipationStatus_pKey,0)=2 and Event_pKey= @intCurEventPKey  and Account_pKey=@intPendingAcctPKey)";
                }
                qry = qry + Environment.NewLine + "  AND t1.pkey Not in (Select  account_pkey from Account_flags where (account_pkey <> @intPendingAcctPKey and account_pkey = t1.pkey) and SpkrFlag_pKey>0 AND event_pkey= @intCurEventPKey) Order by t1.ContactName";
                SqlParameter[] parameters = new SqlParameter[] {
                    new SqlParameter("@intPendingAcctPKey", Model.intPendingAcctPKey),
                    new SqlParameter("@intPendingSessionPKey", Model.Session_pKey),
                    new SqlParameter("@intCurEventPKey", Model.intSpkrCurEventPKey),
                    new SqlParameter("@ishow", ishow),
                    new SqlParameter("@IsSlideCreate", IsSlideCreate),
                };
                return SqlHelper.ExecuteTable(qry, CommandType.Text, parameters);
            }
            catch
            {
                clsUtility.LogErrorMessage(null, null, GetType().Name, 130, "");
            }
            return null;
        }
    }
}
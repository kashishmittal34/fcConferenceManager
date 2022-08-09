using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.UI.WebControls;
using fcConferenceManager;
using fcConferenceManager.Models;
using MAGI_API.Models;
using MAGI_API.Security;
using Microsoft.VisualBasic;
using nsoftware.InPay;
using static fcConferenceManager.frmMySpeakerDinner;
using clsPayment = fcConferenceManager.clsPayment;

namespace MAGI_API.Controllers
{
    [RoutePrefix("api/WebAPI")]

    public class WEBAPIController : ApiController
    {
        static SqlOperation repository = new SqlOperation();
        public Collection colCharges = new Collection();

        [AllowAnonymous]
        [HttpPost]
        [Route("ValidateLogin", Name = "ValidateLogin")]
        public async Task<IHttpActionResult> ValidateLogin([FromBody] string username, string password, string grant_type)
        {
            try
            {
                IdentityUser user = await repository.GetUserbyNameAndPassword(username, password);
                if (user == null)
                {
                    OwinError oobj = new OwinError();
                    oobj.error = "invalid_grant";
                    oobj.error_description = "The user name or password is incorrect.";
                    return Ok(oobj);
                }

                if (!user.EmailConfirmed)
                {
                    OwinError oobj = new OwinError();
                    oobj.error = "invalid_grant";
                    oobj.error_description = "User did not confirm email.";
                    return Ok(oobj);
                }

                OwinResult obj = new OwinResult();
                obj.access_token = Guid.NewGuid().ToString();
                obj.token_type = "bearer";
                obj.expires_in = "86399";
                obj.userid = user.CustomerId;
                obj.username = user.FirstName + " " + user.LastName;
                obj.eventid = user.EventId;
                obj.eventname = user.EventName;
                obj.eventtypeid = user.EventTypeId;
                obj.eventuserid = user.EventAccount_pkey;
                obj.eventuserstatusid = user.ParticipationStatus_pKey;
                obj.role = "KeyUser";
                obj.issued = DateTime.Now.ToString("ddd, dd MMM yyyy HH:mm:ss GMT");
                obj.expires = DateTime.Now.AddDays(1).ToString("ddd, dd MMM yyyy HH:mm:ss GMT");
                obj.EventEndDate = user.EventEndDate;
                obj.EventStartDate = user.EventStartDate;

                return Ok(obj);
            }
            catch (Exception ex)
            {
                OwinError oobj = new OwinError();
                oobj.error = "exception";
                oobj.error_description = ex.Message.ToString();
                return Ok(oobj);
            }
        }


        [HttpPost]
        [Route("Validate_Login", Name = "Validate_Login")]
        public async Task<IHttpActionResult> Validate_Login(Identitylogger logger)
        {
            try
            {
                IdentityUser user = await repository.Apigetuserbynameandpassword_Web(logger.Email, logger.Password);//GetUserbyNameAndPassword
                if (user == null)
                {
                    OwinError oobj = new OwinError();
                    oobj.error = "invalid_grant";
                    oobj.error_description = "The user name or password is incorrect.";
                    oobj.resultID = "0";
                    return Ok(oobj);
                }

                if (!user.EmailConfirmed)
                {
                    OwinError oobj = new OwinError();
                    oobj.error = "invalid_grant";
                    oobj.resultID = "0";
                    oobj.error_description = "User did not confirm email.";
                    return Ok(oobj);
                }

                OwinResult obj = new OwinResult();
                obj.resultID = "1";
                obj.access_token = Guid.NewGuid().ToString();
                obj.token_type = "bearer";
                obj.expires_in = "86399";
                obj.userid = user.CustomerId;
                obj.username = user.FirstName + " " + user.LastName;
                obj.eventid = user.EventId;
                obj.eventname = user.EventName;
                obj.eventtypeid = user.EventTypeId;
                obj.eventuserid = user.EventAccount_pkey;
                obj.eventuserstatusid = user.ParticipationStatus_pKey;
                obj.role = "KeyUser";
                obj.issued = DateTime.Now.ToString("ddd, dd MMM yyyy HH:mm:ss GMT");
                obj.expires = DateTime.Now.AddDays(1).ToString("ddd, dd MMM yyyy HH:mm:ss GMT");
                obj.EventEndDate = user.EventEndDate;
                obj.EventStartDate = user.EventStartDate;
                return Ok(obj);

            }
            catch (Exception ex)
            {
                OwinError oobj = new OwinError();
                oobj.error = "exception";
                oobj.error_description = ex.Message.ToString();
                return Ok(oobj);
            }
        }



        [AllowAnonymous]
        [HttpPost]
        [Route("ValidateUser")]
        public async Task<IHttpActionResult> ValidateUser([FromBody] UserInfo login)
        {
            StatusCode obj;
            try
            {
                obj = await repository.SP_ValidateUser(login.LoginName.ToString(), login.Password.ToString());
            }
            catch (Exception ex)
            {
                obj = new StatusCode();
                obj.Result = 0;
                obj.Status = "Not Ok";
                obj.ErrorMsg = ex.Message.ToString();
            }
            return Ok(obj);
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IHttpActionResult> ForgotPassword([FromBody] UserInfo login)
        {
            try
            {
                StatusCode obj = new StatusCode();
                obj.Statusbit = true;
                obj.Status = "";

                string strEMail = login.Email.ToString();
                if ((strEMail == "") | (!clsEmail.IsValidEmail(strEMail)))
                {
                    obj.Statusbit = false;
                    obj.Status = getErrorMessage(223);
                }
                else
                {
                    SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
                    Label lblMsg = new Label();
                    clsAccount c = new clsAccount();
                    c.sqlConn = sqlConn;
                    c.lblMsg = lblMsg;
                    c.strEmail = strEMail;
                    c.LoadAccountByEmail();
                    if (c.intAccount_PKey > 0)
                    {
                        obj.Result = 1;
                        clsAnnouncement Ann = clsAnnouncement.getAnnouncementInfo(sqlConn, lblMsg, clsAnnouncement.TEXT_PassReset);
                        clsSettings cSettings = new clsSettings();
                        cSettings.sqlconn = sqlConn;
                        cSettings.lblMsg = lblMsg;
                        cSettings.LoadSettings(sqlConn.ConnectionString);
                        string strSubject = Ann.strTitle;
                        string strContent = Ann.strHTMLText;
                        strContent = Microsoft.VisualBasic.Strings.Replace(strContent, "[Name]", c.strFirstname, 1, -1);
                        strContent = Microsoft.VisualBasic.Strings.Replace(strContent, "[Url]", clsSettings.APP_URL() + "/frmResetPassword.aspx?credential=" + clsUtility.Encrypt(c.strEmail), 1, -1);
                        strContent = Microsoft.VisualBasic.Strings.Replace(strContent, "[Support Email]", cSettings.strSupportEmail, 1, -1);
                        strContent = Microsoft.VisualBasic.Strings.Replace(strContent, "[LinkName]", "click here", 1, -1);
                        // --send email
                        clsEmail cEmail = new clsEmail();
                        {
                            var withBlock = cEmail;
                            withBlock.lblMsg = lblMsg;
                            withBlock.strSubjectLine = strSubject;
                            withBlock.strHTMLContent = strContent;
                            withBlock.intAnnouncement_pKey = clsAnnouncement.TEXT_PassReset;
                            if (!cEmail.SendEmailToAccount(c, bPlainText: false))
                            {
                                obj.Statusbit = false;
                            }
                            //return false;
                        }
                        clsAccount.SetPasswordSentDate(c.intAccount_PKey, lblMsg);
                        cEmail = null;
                    }
                    else
                    {

                        obj.Status = getErrorMessage(224);
                        obj.Statusbit = false;
                        return Ok(await Task.FromResult(obj));
                    }
                }

                return Ok(await Task.FromResult(obj));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }




        //[AllowAnonymous]
        //[HttpPost]
        //[Route("ForgotPassword")]        
        //public async Task<IHttpActionResult> ForgotPassword([FromBody]UserInfo login)
        //{
        //    try
        //    {

        //        bool status = true;
        //        return Ok(await Task.FromResult(status));
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(ex.Message.ToString());
        //    }
        //}

        [HttpPost]
        [Route("PartnerList/{EventID}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> PartnerList(string EventID)
        {
            try
            {
                List<Partner> obj = await repository.Partnerlist_select(EventID);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("PartnerList/{EventID}/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> PartnerList(string EventID, string Account_pkey)
        {
            try
            {
                List<Partner> obj = await repository.Partnerlist_select(EventID, Account_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("Files_select/{Type}/{EventOrganizations_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Resource_select(string Type, string EventOrganizations_pkey)
        {
            try
            {
                List<AlltypeFiles> obj = await repository.Resource_select(Type, EventOrganizations_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("GETGreeting/{EventOrganizations_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> GETGreeting(string EventOrganizations_pkey)
        {
            try
            {
                List<PartnerGreeting> obj = await repository.GETGreeting(EventOrganizations_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("UserDetails/{AccountID}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> User_Details(string AccountID)
        {
            try
            {
                List<UserDetails> obj = await repository.GetUserDetail(AccountID);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("PhoneTypes")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> PhoneTypesList()
        {
            try
            {
                List<PhoneType> obj = await repository.PhoneTypesList();
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("Event_Info/{EventID}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Event_info(string EventID)
        {
            try
            {
                List<Event_Info> obj = await repository.EventInfo_select(EventID);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("Program/{EventID}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Program(string EventID)
        {
            try
            {
                List<Program> obj = await repository.ProgramList(EventID, false, true, true, true, "");

                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("Program/{EventID}/{ShowRelated}/{ShowTopic}/{ShowSpeak}/{IsNew}/{strtopic}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Program(string EventID, bool ShowRelated, bool ShowTopic, bool ShowSpeak, bool IsNew, string strtopic)
        {
            try
            {
                List<Program> obj = await repository.ProgramList(EventID, ShowRelated, ShowTopic, ShowSpeak, IsNew, strtopic);

                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("UserUpdate")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> User_Update([FromBody] UserDetails List)
        {
            try
            {
                return Ok(await repository.UserUpdateDetail(List));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }
        [HttpPost]
        [Route("Speakers/{EventID}/{ShortByOrg}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> GetSpeakers(string EventID, Boolean ShortByOrg)
        {
            try
            {
                List<Speaker> obj = await repository.SpeakerList(EventID, ShortByOrg);

                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("SessionDetails/{EventID}/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> GetSpeakers(string EventID, string Account_pkey)
        {
            try
            {
                List<SessionDetails> obj = await repository.Session(EventID, Account_pkey);

                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }
        [HttpPost]
        [Route("GETSessionDetails/{EvtSession_pkey}/{IsShowReleated}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> GetSessionDetails(string EvtSession_pkey, Boolean IsShowReleated)
        {
            try
            {
                //ConfigurationManager.AppSettings("AppURL").ToString().Replace("/forms", "")
                string AppURL = (ConfigurationManager.AppSettings["AppURL"].Replace("/forms", "").ToString());
                List<PublicSessions> obj = await repository.Session_Details(EvtSession_pkey, IsShowReleated, "0", AppURL);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("GETSessionDetails/{EvtSession_pkey}/{IsShowReleated}/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> GetSessionDetails(string EvtSession_pkey, Boolean IsShowReleated, string Account_pkey)
        {
            try
            {
                string AppURL = (ConfigurationManager.AppSettings["AppURL"].Replace("/forms", "").ToString());
                List<PublicSessions> obj = await repository.Session_Details(EvtSession_pkey, IsShowReleated, Account_pkey, AppURL);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("AccountBIO/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> AccountBIO_Select(string Account_pkey)
        {
            try
            {
                List<AccountBio> obj = await repository.AccountBio(Account_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("AccountBIO/{Account_pkey}/{Event_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> AccountBIO_Select(string Account_pkey, string Event_pkey)
        {
            try
            {
                List<AccountBio> obj = await repository.AccountBio(Account_pkey, Event_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("VIEWMySchedule/{Account_pkey}/{Event_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> VIEWMy_Schedule(string Account_pkey, string Event_pkey)
        {
            try
            {
                List<ViewMySchdule> obj = await repository.VIEW_MySchedule(Account_pkey, Event_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("GetPlayLinkandURL/{EventSession_pkey}/{Account_pkey}/{Log_Pkey}/{Event_pkey}/{FileName}/{IpAddress}/{Access_Type}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> GetPlayLinkandURL(string EventSession_pkey, string Account_pkey, string Log_Pkey, string Event_pkey, string FileName, string IpAddress, string Access_Type)
        {
            try
            {
                List<ViewEventShowURL> obj = await repository.GetPlayLinkandURL(EventSession_pkey, Account_pkey, Log_Pkey, Event_pkey, FileName, IpAddress, Access_Type);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("CreateMySchedule/{Account_pkey}/{Event_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> CreateMy_Schedule(string Account_pkey, string Event_pkey)
        {
            try
            {
                List<CreateMySchdule> obj = await repository.Create_MySchedule(Account_pkey, Event_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("SetAttend/{Account_pkey}/{EventSession_pkey}/{Attending}/{Slides}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Set_Attend(string Account_pkey, string EventSession_pkey, Boolean Attending, Boolean Slides)
        {
            try
            {
                return Ok(await repository.Set_Attended(Account_pkey, EventSession_pkey, Attending, Slides, false));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("SetAttend/{Account_pkey}/{EventSession_pkey}/{Attending}/{Slides}/{watch}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Set_Attend(string Account_pkey, string EventSession_pkey, Boolean Attending, Boolean Slides, Boolean watch)
        {
            try
            {
                return Ok(await repository.Set_Attended(Account_pkey, EventSession_pkey, Attending, Slides, watch));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("ParticipationType/Event_pkey")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> ProfInterestsList(String Event_pkey)
        {
            try
            {
                List<DropdownListBind> obj = await repository.ParticipationType(Event_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("ListProfInterests")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> ProfInterestsList()
        {
            try
            {
                List<DropdownListBind> obj = await repository.ProfInterestsList();
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("ListProfInterests/{Event_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> ProfInterests_List(string Event_pkey)
        {
            try
            {
                List<DropdownListBind> obj = await repository.ProfInterests_List(Event_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("ParticipationType/{Event_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> ParticipationType(string Event_pkey)
        {
            try
            {
                List<DropdownListBind> obj = await repository.ParticipationType(Event_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("ListIssueCategories")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> ListIssueCategories()
        {
            try
            {
                List<DropdownListBind> obj = await repository.ListIssueCategories();
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }
        [HttpPost]
        [Route("Participating_organizations/{Event_pkey}/{ParticipationStatus_pKey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> API_Participating_organizations(string Event_pkey, string ParticipationStatus_pKey)
        {
            try
            {
                return Ok(await repository.ListParticipating_organizations(Event_pkey, ParticipationStatus_pKey));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("GetChatHistory/{Account_pkey}/{Event_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> GetChatHistory(string Account_pkey, string Event_pkey)
        {
            try
            {
                //return Ok(await repository.GetChatHistory(Account_pkey ,Event_pkey));
                List<chatHistory> obj = await repository.GetChatHistory(Account_pkey, Event_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }



        [HttpPost]
        [Route("EventSummary/{Account_pkey}/{Event_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> EventSummary(string Account_pkey, string Event_pkey)
        {
            try
            {
                //return Ok(await repository.GetChatHistory(Account_pkey ,Event_pkey));
                List<EventSummary> obj = await repository.getEventSummary(Account_pkey, Event_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("ChargesRefund/{Account_pkey}/{Event_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> ChargesRefund(string Account_pkey, string Event_pkey)
        {
            try
            {
                //return Ok(await repository.GetChatHistory(Account_pkey ,Event_pkey));
                List<Refundcharges> obj = await repository.getRefundcharges(Account_pkey, Event_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("NetworkingLevel/{Account_pkey}/{Event_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> NetworkingLevel(string Account_pkey, string Event_pkey)
        {
            try
            {
                List<NetworkingLevelMSG> obj = await repository.NetworkingLevel(Account_pkey, Event_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("organizationwise_attendee/{Event_pkey}/{Organization_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Api_organizationwise_attendee(string Event_pkey, string Organization_pkey)
        {
            try
            {
                List<Speaker> obj = await repository.Api_organizationwise_attendee(Event_pkey, Organization_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("GetChatPeople/{Account_pkey}/{Event_pkey}/{Search}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> GetChatPeople(string Account_pkey, string Event_pkey, string Search)
        {
            try
            {
                List<Personchat> obj = await repository.GetChatPeople(Account_pkey, Event_pkey, Search);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("GetChatPeople/{Account_pkey}/{Event_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> GetChatPeople(string Account_pkey, string Event_pkey)
        {
            try
            {
                List<Personchat> obj = await repository.GetChatPeople(Account_pkey, Event_pkey, "");
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("ChatHistoryByPerson/{Account_pkey}/{Event_pkey}/{userAccount_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> ChatHistoryByPerson(string Account_pkey, string Event_pkey, string userAccount_pkey)
        {
            try
            {
                List<chatHistory> obj = await repository.ChatHistoryByPerson(Account_pkey, Event_pkey, userAccount_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("FutureConferences")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> FutureConferences()
        {
            try
            {
                List<FutureConferences> obj = await repository.ListFutureConferences();
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }




        [HttpPost]
        [Route("Networking/{Event_pkey}/{Account_pkey}/{ParticipationStatus_pKey}/{OrganizationType_pkey}/{Country_pKey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Networking(string Event_pkey, string Account_pkey, string ParticipationStatus_pKey, string OrganizationType_pkey, String Country_pKey)
        {
            try
            {
                List<Networking> obj = await repository.NetworkingList(Account_pkey, Event_pkey, ParticipationStatus_pKey, OrganizationType_pkey, Country_pKey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("NetworkingOutgoing/{Event_pkey}/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> NetworkingOutgoing(string Event_pkey, string Account_pkey)
        {
            try
            {
                List<NeworkingOutgoing> obj = await repository.NetworkingOurgoingList(Account_pkey, Event_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("NetworkingInComing/{Event_pkey}/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> NetworkingInComing(string Event_pkey, string Account_pkey)
        {
            try
            {
                List<NetworkingIncoming> obj = await repository.NetworkingIncomingList(Account_pkey, Event_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("Event_TermsandConditions/{ID}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> OverView(string ID)
        {
            try
            {
                SettingText obj = await repository.Setting_Text(ID);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("Overview/{Event_pkey}/{App_type}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> OverView_MobileSetting(string Event_pkey, string App_type)
        {
            try
            {
                List<OverviewMobile> obj = await repository.OverView_MobileSetting(Event_pkey, App_type, "0");
                if (obj != null && obj.Count > 0)
                {
                    if (HttpContext.Current.Session != null)
                    {
                        SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
                        Label lblMsg = new Label();
                        SetContext(false);
                        SetGlobal(sqlConn, lblMsg);
                        clsEvent cEvent = new clsEvent();
                        cEvent.sqlConn = sqlConn;
                        cEvent.lblMsg = lblMsg;
                        cEvent.intEvent_PKey = Convert.ToInt32(Event_pkey);
                        bool checkEvent = cEvent.LoadEvent();
                        if (checkEvent)
                        {
                            foreach (var retobj in obj)
                            {
                                string subject = retobj.OverviewTitle;
                                string body = retobj.OverviewDescription;
                                subject = cEvent.ReplaceReservedWords(subject);
                                body = cEvent.ReplaceReservedWords(body);
                                retobj.OverviewTitle = subject;
                                retobj.OverviewDescription = body;
                            }
                        }
                    }
                }
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("siteinfo/{Event_pkey}/{App_type}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Site_Info(string Event_pkey, string App_type)
        {
            try
            {
                List<MobileInfo> obj = await repository.Mobile_Info(Event_pkey, App_type, "1");
                if (obj != null && obj.Count > 0)
                {
                    if (HttpContext.Current.Session != null)
                    {
                        SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
                        Label lblMsg = new Label();
                        SetContext(false);
                        SetGlobal(sqlConn, lblMsg);
                        clsEvent cEvent = new clsEvent();
                        cEvent.sqlConn = sqlConn;
                        cEvent.lblMsg = lblMsg;
                        cEvent.intEvent_PKey = Convert.ToInt32(Event_pkey);
                        bool checkEvent = cEvent.LoadEvent();
                        if (checkEvent)
                        {
                            foreach (var retobj in obj)
                            {
                                string subject = retobj.Title;
                                string body = retobj.Description;
                                subject = cEvent.ReplaceReservedWords(subject);
                                body = cEvent.ReplaceReservedWords(body);
                                retobj.Title = subject;
                                retobj.Description = body;
                            }
                        }
                    }
                }
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("eventinfo/{Event_pkey}/{App_type}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Event_Info(string Event_pkey, string App_type)
        {
            try
            {
                List<MobileInfo> obj = await repository.Mobile_Info(Event_pkey, App_type, "2");
                if (obj != null && obj.Count > 0)
                {
                    if (HttpContext.Current.Session != null)
                    {
                        SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
                        Label lblMsg = new Label();
                        SetContext(false);
                        SetGlobal(sqlConn, lblMsg);
                        clsEvent cEvent = new clsEvent();
                        cEvent.sqlConn = sqlConn;
                        cEvent.lblMsg = lblMsg;
                        cEvent.intEvent_PKey = Convert.ToInt32(Event_pkey);
                        bool checkEvent = cEvent.LoadEvent();
                        if (checkEvent)
                        {
                            foreach (var retobj in obj)
                            {
                                string subject = retobj.Title;
                                string body = retobj.Description;
                                subject = cEvent.ReplaceReservedWords(subject);
                                body = cEvent.ReplaceReservedWords(body);
                                retobj.Title = subject;
                                retobj.Description = body;
                            }
                        }
                    }
                }
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("updates/{Event_pkey}/{App_type}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Updates(string Event_pkey, string App_type)
        {
            try
            {
                List<MobileInfo> obj = await repository.Mobile_Info(Event_pkey, App_type, "3");
                if (obj != null && obj.Count > 0)
                {
                    if (HttpContext.Current.Session != null)
                    {
                        SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
                        Label lblMsg = new Label();
                        SetContext(false);
                        SetGlobal(sqlConn, lblMsg);
                        clsEvent cEvent = new clsEvent();
                        cEvent.sqlConn = sqlConn;
                        cEvent.lblMsg = lblMsg;
                        cEvent.intEvent_PKey = Convert.ToInt32(Event_pkey);
                        bool checkEvent = cEvent.LoadEvent();
                        if (checkEvent)
                        {
                            foreach (var retobj in obj)
                            {
                                string subject = retobj.Title;
                                string body = retobj.Description;
                                subject = cEvent.ReplaceReservedWords(subject);
                                body = cEvent.ReplaceReservedWords(body);
                                retobj.Title = subject;
                                retobj.Description = body;
                            }
                        }
                    }
                }
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("aboutapp/{Event_pkey}/{App_type}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> About_App(string Event_pkey, string App_type)
        {
            try
            {
                List<MobileInfo> obj = await repository.Mobile_Info(Event_pkey, App_type, "4");
                if (obj != null && obj.Count > 0)
                {
                    if (HttpContext.Current.Session != null)
                    {
                        SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
                        Label lblMsg = new Label();
                        SetContext(false);
                        SetGlobal(sqlConn, lblMsg);
                        clsEvent cEvent = new clsEvent();
                        cEvent.sqlConn = sqlConn;
                        cEvent.lblMsg = lblMsg;
                        cEvent.intEvent_PKey = Convert.ToInt32(Event_pkey);
                        bool checkEvent = cEvent.LoadEvent();
                        if (checkEvent)
                        {
                            foreach (var retobj in obj)
                            {
                                string subject = retobj.Title;
                                string body = retobj.Description;
                                subject = cEvent.ReplaceReservedWords(subject);
                                body = cEvent.ReplaceReservedWords(body);
                                retobj.Title = subject;
                                retobj.Description = body;
                            }
                        }
                    }
                }
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        private string ReadConnectionString()
        {
            string connString = string.Format("Data Source={0};", ConfigurationManager.AppSettings["AppS"].ToString());
            connString += string.Format("Uid={0};", ConfigurationManager.AppSettings["AppL"].ToString());
            connString += string.Format("Pwd={0};", ConfigurationManager.AppSettings["AppP"].ToString());
            connString += string.Format("Database={0};", ConfigurationManager.AppSettings["AppDB"].ToString());
            connString += string.Format("Connect Timeout={0};", ConfigurationManager.AppSettings["AppT"].ToString());
            connString += string.Format("MultipleActiveResultSets={0};", "true");
            return connString;

        }

        private void SetContext(bool isProduction)
        {
            //HttpContext.Current = new HttpContext();
            //HttpContext.Current.Request = new HttpRequest();
            //HttpContext.Current.Response = new HttpResponse();
            //HttpContext.Current.Server = new HttpServerUtility(isProduction);
        }

        private void SetGlobal(SqlConnection sqlConn, Label lblMsg)
        {
            if (HttpContext.Current.Application["cImages"] == null)
                HttpContext.Current.Application.Add("cImages", clsImg.LoadImages(sqlConn.ConnectionString));
            if (HttpContext.Current.Session["sqlConn"] == null)
                HttpContext.Current.Session.Add("sqlConn", sqlConn.ConnectionString);
            if (HttpContext.Current.Session["cSettings"] == null)
            {
                clsSettings cSettings = new clsSettings();
                cSettings.sqlconn = sqlConn;
                cSettings.lblMsg = lblMsg;
                cSettings.LoadSettings(sqlConn.ConnectionString);
                //cSettings.LoadPrimaryEvent(sqlConn.ConnectionString);
                HttpContext.Current.Session.Add("cSettings", cSettings);
                clsLastUsed cLastUsed = new clsLastUsed();
                cLastUsed.intActiveEventPkey = cSettings.intPrimaryEvent_pkey;
                cLastUsed.strActiveEvent = cSettings.strPrimaryEventID;
                HttpContext.Current.Session.Add("cLastUsed", cLastUsed);
                HttpContext.Current.Session.Add("cFormList", new clsFormList());
                HttpContext.Current.Session.Add("Version", clsSettings.APP_VERSION);
                HttpContext.Current.Session.Add("BlueRibbonMode", ConfigurationManager.AppSettings["BRMode"].ToString() == "1");

                clsSurrogate cSurrogate = new clsSurrogate();
                HttpContext.Current.Session.Add("Surrogate", cSurrogate);
                HttpContext.Current.Session.Add("VeevaMode", false);
                HttpContext.Current.Session.Add("SourceMode", false);
            }
            if (HttpContext.Current.Session["cAccount"] == null)
            {
                clsAccount cAccount = new clsAccount();
                cAccount.sqlConn = sqlConn;
                cAccount.lblMsg = lblMsg;
                cAccount.intAccount_PKey = 24356;
                //cAccount.LoadAccount();
                HttpContext.Current.Session.Add("cAccount", cAccount);
            }
        }

        [HttpPost]
        [Route("ReceiptDownload/{IntReceiptNumber}/{StrReceiptNumber}/{bpaid}")]
        [Authorize(Roles = "KeyUser")]
        public byte[] MyMethod(string IntReceiptNumber, string StrReceiptNumber, Boolean bpaid)
        {
            receipt obj = new receipt();
            string strDisplayFileName = "";
            byte[] bytes = null;
            SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
            string returnstring = "";
            Label lblMsg = new Label();
            Label title = new Label();
            clsReceipt c = new clsReceipt();
            c.sqlConn = sqlConn;
            c.lblMsg = lblMsg;
            c.intReceiptNumber = Convert.ToInt32(IntReceiptNumber);
            //this.lblReceiptNum.Text = c.getReceiptNum();
            string bodyInvoice = c.APIgetReceiptBody(title);

            string strStatus = c.getReciptPaidFreeStatus();
            string textinvoice = bodyInvoice;
            //Boolean bfree = false;
            Boolean bfree = (strStatus == "Free" ? true : false);
            string name = (strStatus == "paid" ? "Receipt" : "Invoice");
            //obj.Invoice = bytes.Tostring();
            string dd = c.ExportReceiptPdf_ReturBytes(title.Text.ToString(), StrReceiptNumber, bodyInvoice, ref strDisplayFileName, ref bytes, bFree: bfree);

            //byte[] mybytearray = db.getmybytearray(ID);//working fine,returning proper result.
            return bytes;
        }

        //[HttpPost]
        //[Route("ReceiptDownload/{IntReceiptNumber}")]
        //[Authorize(Roles = "KeyUser")]
        //public async Task<IHttpActionResult> Receipt(string IntReceiptNumber)
        //{
        //    receipt obj = new receipt();
        //    string strDisplayFileName = "";
        //    byte[] bytes = null;
        //    SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
        //    string returnstring = "";
        //    Label lblMsg = new Label();
        //    Label title = new Label();
        //    clsReceipt c = new clsReceipt();
        //    c.sqlConn = sqlConn;
        //    c.lblMsg = lblMsg;
        //    c.intReceiptNumber = Convert.ToInt32(IntReceiptNumber);
        //    //this.lblReceiptNum.Text = c.getReceiptNum();
        //    string bodyInvoice = c.getReceiptBody(title);
        //    string textinvoice = bodyInvoice;
        //    //obj.Invoice = bytes.Tostring();
        //    string dd = c.ExportReceiptPdf_ReturBytes("Invoice", IntReceiptNumber, bodyInvoice, ref strDisplayFileName, ref bytes);
        //    MemoryStream dataStream = new MemoryStream(bytes);
        //    BinaryFormatter bf = new BinaryFormatter();
        //    MemoryStream ms = new MemoryStream();
        //    bf.Serialize(ms, bytes);
        //    obj.Invoice = "";
        //    return Ok(obj);
        //}

        private byte[] objecttobytearray(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();

        }
        [HttpPost]
        [Route("FAQs/{Event_pkey}/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> FAQs(string Event_pkey, string Account_pkey)
        {
            try
            {
                List<FAQ> obj = await repository.FAQs_List(Account_pkey, Event_pkey);
                if (obj != null && obj.Count > 0)
                {
                    if (HttpContext.Current.Session != null)
                    {
                        SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
                        Label lblMsg = new Label();
                        SetContext(false);
                        SetGlobal(sqlConn, lblMsg);
                        clsEvent cEvent = new clsEvent();
                        cEvent.sqlConn = sqlConn;
                        cEvent.lblMsg = lblMsg;
                        cEvent.intEvent_PKey = Convert.ToInt32(Event_pkey);
                        bool checkEvent = cEvent.LoadEvent();
                        if (checkEvent)
                        {
                            clsVenue clsVenue = new clsVenue();
                            clsVenue.sqlConn = sqlConn;
                            clsVenue.lblMsg = lblMsg;
                            clsVenue.intVenue_PKey = cEvent.intVenue_PKey;
                            bool checkVenue = clsVenue.LoadVenue();
                            foreach (var retobj in obj)
                            {
                                string FAQa = retobj.FAQa;
                                FAQa = cEvent.ReplaceReservedWords(FAQa);
                                if (checkVenue)
                                {
                                    FAQa = clsVenue.ReplaceReservedWords(FAQa);
                                    FAQa = clsReservedWords.ReplacePriorNext(lblMsg, FAQa);
                                    FAQa = clsSettings.ReplaceTermsGeneral(FAQa);
                                }
                                retobj.FAQa = FAQa;
                            }
                        }
                    }
                }

                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        /// <summary>
        ///  Not Meged this is for Examples .
        /// </summary>
        /// <param name="intStatus_pkey"></param>
        /// <param name="intActiveEventPkey"></param>
        /// 
        private string ReturnReceiptText(string IntReceiptnum)
        {
            string strDisplayFileName = "";
            byte[] bytes = null;
            SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
            string returnstring = "";
            Label lblMsg = new Label();
            Label title = new Label();
            clsReceipt c = new clsReceipt();
            c.sqlConn = sqlConn;
            c.lblMsg = lblMsg;
            c.intReceiptNumber = Convert.ToInt32(IntReceiptnum);
            //this.lblReceiptNum.Text = c.getReceiptNum();
            string bodyInvoice = c.getReceiptBody(title);

            string textinvoice = bodyInvoice;
            //obj.Invoice = bytes.Tostring();
            string dd = c.ExportReceiptPdf_ReturBytes("Invoice", IntReceiptnum, bodyInvoice, ref strDisplayFileName, ref bytes);
            returnstring = bytes.ToString();
            c = null/* TODO Change to default(_) if this is not a reference type */;
            return returnstring;
        }
        private void RefreshScreen(int intStatus_pkey, int intActiveEventPkey)
        {
            string strBody = "";
            string strbodySend = "";
            string strContent = "";
            string strtitle = "";
            Label lblMsg = new Label();
            SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
            if (intStatus_pkey > 0)
            {

                clsAnnouncement c = new clsAnnouncement();
                c.sqlConn = sqlConn;
                c.lblMsg = lblMsg;
                c.intAnnouncement_PKey = intStatus_pkey;
                c.LoadAnnouncement();
                strContent = c.strHTMLText;
                strtitle = c.strTitle;

                c = null/* TODO Change to default(_) if this is not a reference type */;
            }

        }



        public string getErrorMessage(int intCode)
        {
            string getErrorMessage = "";

            SqlConnection sqlConn = new SqlConnection(ReadConnectionString());

            string qry = "select (Case when t1.isError = 1 Then t1.ErrorCodeText  else t1.ErrorCodeText end) +' ' + ISNULL(t1.UserErrorText,'') as ErrorText From Sys_ErrorCodes t1 Where t1.pKey = " + intCode.ToString();
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            try
            {
                if (clsUtility.GetDataTable(sqlConn, cmd, ref dt))
                {
                    if (dt.Rows.Count > 0)
                    {
                        getErrorMessage = dt.Rows[0]["ErrorText"].ToString();

                    }
                }
            }
            catch (SqlException ex)
            {
            }
            // --do nothing
            finally
            {
                dt = null/* TODO Change to default(_) if this is not a reference type */;
            }

            return getErrorMessage;
        }

        [HttpPost]
        [Route("InvitedAnnouncement/{Announcement_pkey}/{Event_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> AnnouncementText(string Announcement_pkey, string Event_pkey)
        {
            Label lblMsg = new Label();
            SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
            try
            {
                AnnouncementText obj = new AnnouncementText();


                clsAnnouncement c = new clsAnnouncement();
                c.sqlConn = sqlConn;
                c.lblMsg = lblMsg;
                c.intAnnouncement_PKey = Convert.ToInt32(Announcement_pkey);
                c.LoadAnnouncement();


                obj.Contant = c.strHTMLText;
                obj.Title = c.strTitle;
                clsEvent cEvent = new clsEvent();
                cEvent.sqlConn = sqlConn;
                cEvent.lblMsg = lblMsg;
                cEvent.intEvent_PKey = Convert.ToInt32(Event_pkey);
                bool checkEvent = cEvent.LoadEvent();
                if (checkEvent)
                {

                    string subject = obj.Title;
                    string body = obj.Contant;
                    subject = cEvent.ReplaceReservedWords(subject);
                    body = cEvent.ReplaceReservedWords(body);
                    obj.Contant = c.strHTMLText;
                    obj.Title = subject;

                }
                string strBody = "";
                string strBaseBody = obj.Contant;
                string strtitle = "";
                string strReplace3 = "";
                string strBaseTitle = "";
                int event_pkey = Convert.ToInt32(Event_pkey);
                clsReservedWords.ReplaceBase(lblMsg, event_pkey, obj.Title, ref strBaseTitle, strBaseBody, ref strBody, "", ref strReplace3);

                obj.Title = strBaseTitle;
                obj.Contant = strBody;
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("SendInvitaion")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> SendInvitaion([FromBody] EmailSendContent user)
        {
            Label lblMsg = new Label();
            SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
            try
            {
                bool status = true;
                string nickname = "";
                string FirstName = "";
                int Account_pkey = Convert.ToInt32(user.Account_pkey);
                clsAccount c = new clsAccount();
                c.sqlConn = sqlConn;
                c.lblMsg = lblMsg;
                c.intAccount_PKey = Account_pkey;
                c.LoadAccountByEmail();
                nickname = c.strNickname;
                FirstName = c.strFirstname;
                string clearname = "";
                string Message = "";
                if (nickname != "")
                {
                    clearname = nickname;
                }
                else
                {
                    clearname = FirstName;
                }
                Message = Microsoft.VisualBasic.Strings.Replace(user.Message, "[Each recipient's name will display here],", clearname, 1, -1);
                Message = Microsoft.VisualBasic.Strings.Replace(user.Message, "[Each recipient's name will display here]", clearname, 1, -1);
                clsEmail cEMail = new clsEmail();
                cEMail.sqlConn = sqlConn;
                cEMail.lblMsg = lblMsg;
                cEMail.strSubjectLine = user.Subject;
                cEMail.strHTMLContent = Message;
                cEMail.strEmailFromAddress = user.AccountEmail;
                cEMail.strEmailUserName = user.AccountContactName;

                if (cEMail.SendEmailToAcctPKey(Account_pkey, user.Email))
                {
                    string qry = "";
                    qry = qry + Environment.NewLine + "UPDATE  MeetingPlanner_Detail SET IsSendInvitions=1 Where pkey=" + user.MeetingPlannerDetails_pkey.ToString();
                    SqlCommand cmd = new SqlCommand(qry);
                    if (!clsUtility.ExecuteQuery(cmd, lblMsg, "Error logging  request"))
                    {
                        status = false;
                    }

                }
                c = null;
                return Ok(await Task.FromResult(status));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("Contacts/{Event_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Contacts(string Event_pkey)
        {
            try
            {
                List<Contacts> obj = await repository.Contacts_List(Event_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("UserIssue")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> UserIssue([FromBody] UserIssue List)
        {
            try
            {
                bool status = true;
                string strEMail = List.email.ToString();
                if ((strEMail == "") | (!clsEmail.IsValidEmail(strEMail)))
                {
                    status = false;
                    return Ok(await Task.FromResult(status));
                }
                else
                {

                    List<UserIssue> obj = await repository.UserIssueSave(List);

                    //string strContent,string txtName, string txtUserName, string txtUserEmail,int intUpdatePkey         
                    SendEmail(List.description, List.title, List.username, List.email, Convert.ToInt32(obj[0].pkey));
                    return Ok("Updated".ToString());
                    //return Ok(await repository.UserIssueSave(List));
                }

            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("NetworkingOutgoingSave")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> NetworkingOutgoing_Save([FromBody] NeworkingOutgoing List)
        {
            try
            {
                return Ok(await repository.NetworkingOutgoing_Save(List));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("NetworkingInComingSave")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> NetworkingInComingSave([FromBody] NetworkingIncoming List)
        {
            try
            {
                return Ok(await repository.Networkingincoming_Save(List));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("CountryList")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Country_List()
        {
            try
            {
                List<DropdownListBind> obj = await repository.CountryList();
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("EmailTypes")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> EmailTypes()
        {
            try
            {
                List<DropdownListBind> obj = await repository.EmailTypes();
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("StateList/{Country_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> State_List(string Country_pkey)
        {
            try
            {
                List<DropdownListBind> obj = await repository.StateList(Country_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("TimeZoneList/{Country_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> TimeZoneList(string Country_pkey)
        {
            try
            {
                List<DropdownListBind> obj = await repository.TimeZoneList(Country_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("TimeZoneListforMeeting/{Country_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> TimeZoneListforMeeting(string Country_pkey)
        {
            try
            {
                List<DropdownListBind> obj = await repository.TimeZoneListfor_Meeting(Country_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("Sector")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> SectorList()
        {
            try
            {
                List<DropdownListBind> obj = await repository.SectorList();
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("MeetingPlannerList")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> MeetingPlanner_List([FromBody] MeetingPlannerList List)
        {
            try
            {
                List<MeetingPlannerList> obj = await repository.MeetingPlannerList(List);
                return Ok(obj);

            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }



        [HttpPost]
        [Route("MeetingPlannerSave")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> MeetingPlannerSave([FromBody] MeetingPlannerSave List)
        {
            try
            {
                return Ok(await repository.MeetingPlannerSave(List));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }
        [HttpPost]
        [Route("Meetinglocation")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Meetinglocation()
        {
            try
            {
                List<DropdownListBind> obj = await repository.meetinglocationList();
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("EventMeetinglocation/{Event_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> EventMeetinglocation(string Event_pkey)
        {
            try
            {
                List<DropdownListBind> obj = await repository.meetinglocation_List(Event_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("EventAttending")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> EventAttendingList([FromBody] EventAttending List)
        {
            try
            {
                List<EventAttending> obj = await repository.EventAttendingList(List);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("MeetingPlannerInvited")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> MeetingPlannerInvited([FromBody] MeetingPlannerAllInvited List)
        {
            try
            {
                List<MeetingPlannerAllInvited> obj = await repository.MeetingPlannerAllInvited(List);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("MeetingPlannerEdit/{MeetingPlanner_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> MeetingPlannerEdit(string MeetingPlanner_pkey)
        {
            try
            {
                List<MeetingPlanneredit> obj = await repository.MeetingPlannerEdit(MeetingPlanner_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }
        [HttpPost]
        [Route("MeetingPlannerAttended/{MeetingPlanner_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> MeetingPlannerAttended_Select(string MeetingPlanner_pkey)
        {
            try
            {
                List<EventAttending> obj = await repository.MeetingPlannerAttended_Select(MeetingPlanner_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("MeetingPlannerUpdateAttended")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> MeetingPlannerUpdateAttended([FromBody] MeetingPlannerSave List)
        {
            try
            {
                return Ok(await repository.MeetingPlannerUpdateAttended(List));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }
        [HttpPost]
        [Route("MeetingDelete/{MeetingPlanner_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> MeetingDelete(string MeetingPlanner_pkey)
        {
            try
            {
                return Ok(await repository.MeetingDelete(MeetingPlanner_pkey));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("DeleteAttending/{pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> DeleteAttending(string pkey)
        {
            try
            {
                return Ok(await repository.DeleteAttending(pkey));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("MeetingStatusChanges")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> MeetingStatusChanges([FromBody] MeetingPlannerSave List)
        {
            try
            {
                return Ok(await repository.MeetingStatusChanges(List));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("QuestionList")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> QuestionList([FromBody] Question_List List)
        {
            try
            {
                List<Question_List> obj = await repository.QuestionList(List);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }
        //[Route("FeedbackForm/{Event_pkey}/{Approved}")]

        [HttpPost]
        [Route("QuestionGraph/{EventSession_pkey}/{Question_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> QuestionGraph(string EventSession_pkey, string Question_pkey)
        {
            try
            {
                List<questionGraph> obj = await repository.questionGraph(EventSession_pkey, Question_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("SpeakerPollingResult/{EventSession_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> SpeakerPollingResult(string EventSession_pkey)
        {
            try
            {
                List<PoolingManagement> obj = await repository.PoolingManagement(EventSession_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("FormList")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> FormList()
        {
            try
            {
                List<Form_List> obj = await repository.FormList();
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("FeedbackForm/{Event_pkey}/{Approved}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> FeedbackForm(string Event_pkey, string Approved)
        {
            try
            {
                List<Form_List> obj = await repository.FeedbackForm(Event_pkey, Approved);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("Attendee_Log")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Attendee_Log([FromBody] Attendee_Log List)
        {
            try
            {
                return Ok(await repository.Attendee_Log_save(List));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("ActivityFeedbackFormUpdate")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> ActivityFeedbackFormUpdate([FromBody] Question_List List)
        {
            try
            {
                return Ok(await repository.ActivityFeedbackForm_Update(List));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("AttendingQuestionSave")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> AttendingQuestionSave([FromBody] Question_List List)
        {
            try
            {
                return Ok(await repository.Questions_SAVE(List));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("RegistrationQuestionResponce_save")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> AttendingQuestionSave([FromBody] RegistrationResponceSave List)
        {
            try
            {
                return Ok(await repository.RegistrationQuestionResponce_save(List));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("EventWiseActivityList/{Event_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> EventWiseActivityList(string Event_pkey)
        {
            try
            {
                List<DropdownListBind> obj = await repository.EventWiseActivityList(Event_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("EventSpeakerlist/{Event_pkey}/{EventSession_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> EventSpeakerlist(string Event_pkey, string EventSession_pkey)
        {
            try
            {
                List<DropdownListBind> obj = await repository.EventSpeakerlist(Event_pkey, EventSession_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("SpeakerFeedBackSave")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> SpeakerFeedBack([FromBody] SpeakerFeedBack List)
        {
            try
            {
                return Ok(await repository.SpeakerFeedBack(List));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        //////////////HR => 20200630

        [HttpPost]
        [Authorize(Roles = "KeyUser")]
        [Route("getFeedback/{ActiveEventPkey}/{Account_PKey}")]
        public async Task<IHttpActionResult> getFeedback(string ActiveEventPkey, string Account_PKey)
        {
            try
            {
                FeedbackOpertions o = new FeedbackOpertions();
                return Ok(await o.Feedback_question_select(ActiveEventPkey, Account_PKey));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "KeyUser")]
        [Route("postFeedBack")]
        public async Task<IHttpActionResult> postFeedBack(FeedBackList feedBackList)
        {
            try
            {
                FeedbackOpertions o = new FeedbackOpertions();
                return Ok(await o.Feedback_Submit(feedBackList));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }


        //survey......................

        [HttpPost]
        [Authorize(Roles = "KeyUser")]
        [Route("getSurvey/{accountKey}/{EventKey}/{SurveyKey}")]
        public async Task<IHttpActionResult> getSurvey(string accountKey, string EventKey, string SurveyKey)
        {
            try
            {
                SurveyOpertions o = new SurveyOpertions();
                return Ok(await o.Survey_question_select(accountKey, EventKey, SurveyKey));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "KeyUser")]
        [Route("postSurvey")]
        public async Task<IHttpActionResult> postSurvey(PostSurvey postSurvey)
        {
            try
            {
                SurveyOpertions o = new SurveyOpertions();
                return Ok(await o.Post_Survey(postSurvey));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost]
        [Route("GetVersion")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> GetVersion([FromBody] VersionParameter versionParameters)
        {
            try
            {
                VersionOperations vo = new VersionOperations();
                return Ok(await vo.getVersion(versionParameters));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        //-- -- 0 salutations ,1 Country ,2 Account Status  ,3 suffixes ,4 Country wise State(@Country_pkey) ,5 timezone (@Country_pkey) ,6 LicenseType
        [HttpPost]
        [Route("ddlList/{Type}/{Country_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> ddlList(string type, string Country_pkey)
        {
            try
            {
                List<DropdownListBind> obj = await repository.ddlList(type, Country_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("ddlSpeakerAdvice")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> ddlSpeakerAdvice()
        {
            try
            {
                List<DropdownListBind> obj = await repository.ddlSpeakerAdvice();
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("Organization/{Event_pkey}/{Name}/{pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> API_Organization_Select_ALL(string Event_pkey, string Name, string pkey)
        {
            try
            {
                List<Organization> obj = await repository.Organization_Select_ALL(Event_pkey, Name, pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("feedback_select/{Eventsession_pkey}/{Account_pkey}/{Event_pkey}/{LogByAccount_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> feedback_select(string Eventsession_pkey, string Account_pkey, string Event_pkey, string LogByAccount_pkey)
        {
            try
            {
                List<FedbackReBind> obj = await repository.feedback_select(Eventsession_pkey, Account_pkey, Event_pkey, LogByAccount_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }



        [HttpPost]
        [Route("AttendeeQuestion_Select/{EventSession_pkey}/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> API_AttendeeQuestion_Select(string EventSession_pkey, string Account_pkey)
        {
            try
            {
                List<AttendeeQuestion> obj = await repository.API_AttendeeQuestion_Select(EventSession_pkey, Account_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("AttendeeRegistriongQuesSelect/{Event_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> API_AttendeeRegistriongQuesSelect(string Event_pkey)
        {
            try
            {
                List<AttendeeRegistriongQues> obj = await repository.AttendeeRegistriongQuesSelect(Event_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("ConnectionRequest_status/{Event_pkey}/{Account_pkey}/{ConnectionAccount_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> ConnectionRequest_status(string Event_pkey, string Account_pkey, string ConnectionAccount_pkey)
        {
            try
            {
                List<Connectionstatus> obj = await repository.ConnectionRequest_status(Event_pkey, Account_pkey, ConnectionAccount_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }




        [HttpPost]
        [Route("ConnectionStatus_Change/{Event_pkey}/{Account_pkey}/{ConnectionAccount_pkey}/{Status}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> ConnectionStatus_Change(string Event_pkey, string Account_pkey, string ConnectionAccount_pkey, string Status)
        {
            try
            {
                List<Connectionstatus> obj = await repository.ConnectionStatus_Change(Event_pkey, Account_pkey, ConnectionAccount_pkey, Status);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("Attendee_Search/{Event_pkey}/{CommonSearch}/{Al}/{Long}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Attendee_Search(string Event_pkey, string CommonSearch, string Al, string Long)
        {
            try
            {
                List<SearchAttendee> obj = await repository.SearchAttendeeLocationList(Event_pkey, CommonSearch, Al, Long);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("EventSchedule/{Event_pkey}/{Account_pkey}/{EventOrganizations_pkey}/{Date}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> EventSchedule(string Event_pkey, string Account_pkey, string EventOrganizations_pkey, string Date)
        {
            try
            {
                List<EventSchedule> obj = await repository.EventSchedule(Event_pkey, Account_pkey, EventOrganizations_pkey, Date);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }
        [HttpPost]
        [Route("EventSchedule/{Event_pkey}/{Account_pkey}/{EventOrganizations_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> EventSchedule(string Event_pkey, string Account_pkey, string EventOrganizations_pkey)
        {
            try
            {
                List<EventSchedule> obj = await repository.EventSchedule(Event_pkey, Account_pkey, EventOrganizations_pkey, null);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("ConnectPeople/{Event_pkey}/{Account_pkey}/{EventOrganizations_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> ConnectPeople(string Event_pkey, string Account_pkey, string EventOrganizations_pkey)
        {
            try
            {
                List<ConnectPeople> obj = await repository.ConnectPeopleList(Event_pkey, Account_pkey, EventOrganizations_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }



        [HttpPost]
        [Route("LunchOption/{Event_pkey}/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> LunchOption(string Event_pkey, string Account_pkey)
        {
            try
            {
                List<LunchOptions> obj = await repository.LunchOption(Event_pkey, Account_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("MealList/{Event_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> MealList(string Event_pkey)
        {
            try
            {
                List<DropdownListBind> obj = await repository.MealList(Event_pkey, "0", "");
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("MealList/{Event_pkey}/{DefaultMeal}/{STRMeal}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> MealList(string Event_pkey, string DefaultMeal, string STRMeal)
        {
            try
            {
                List<DropdownListBind> obj = await repository.MealList(Event_pkey, DefaultMeal, STRMeal);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("BadgeInfo/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> BadgeInfo(string Account_pkey)
        {
            try
            {
                List<BadgeInfor> obj = await repository.BadgeInfo(Account_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("AttendeeTreasureHunt/{Event_pkey}/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> AttendeeTreasureHunt(string Event_pkey ,string Account_pkey )
        {
            try
            {
                List<AttendeeTreasureHunt> obj = await repository.AttendeeTreasureHuntSelect(Event_pkey ,Account_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("Examcharges/{Event_pkey}/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Examcharges(string Event_pkey, string Account_pkey)
        {
            try
            {
                List<Examcharges> obj = await repository.Examcharges(Event_pkey, Account_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("BadgeDesignList/{pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> BadgeDesign_List(string pkey)
        {
            try
            {
                List<BadgeDesignSetting> obj = await repository.BadgeDesignList(pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("SubmitQuestion/{Account_pkey}/{Event_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> SubmitQuestion(string Account_pkey,string  Event_pkey)
        {
            try
            {
                List<AttendeeValidations> obj = await repository.SubmitQuestionlist(Account_pkey, Event_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("RegiQuestion_Attempt/{Account_pkey}/{Event_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> RegiQuestion_Attempt(string Account_pkey, string Event_pkey)
        {
            try
            {
                return Ok(await repository.RegiQuestion_Attempt(Account_pkey, Event_pkey));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("LocaltionUpdate")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> LocationUpdate([FromBody] LocationParameter List)
        {
            try
            {
                return Ok(await repository.LocationUpdate(List));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("SaveParticipantsSessionNotes")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> SaveParticipantsSessionNotes([FromBody] ParticipentNote List)
        {
            try
            {
                return Ok(await repository.SaveParticipantsSessionNotes(List));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }




        [HttpPost]
        //[Route("SaveExamcharges/{Event_pkey}/{Account_pkey}/{Memo}/{chargeType_pkey}/{CheckedItem}/{chkdorNot}/{strchargeType}")]
        [Route("SaveExamcharges")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> SaveExamcharges([FromBody] ExamChargeupdate List)
        {

            string Event_pkey = List.Event_pkey; // "";
            string Account_pkey = List.Account_pkey.ToString();// "";
            string Memo = List.Memo.ToString();// "";
            string chargeType_pkey = List.chargeType_pkey.ToString();// "";
            string CheckedItem = List.CheckedItem.ToString();//"";
            string chkdorNot = List.chkdorNot.ToString();// "";
            string strchargeType = List.strchargeType.ToString();//"";

            string[] alreadychecked = CheckedItem.Split(',');
            Boolean CheckOldCertification = true;
            int bCMECert = 0;
            int bCNECert = 0;
            int bCCBCert = 0;
            int bCLECert = 0;
            int bCLEPACert = 0;
            int bCLECompactCert = 0;
            Boolean checkedorNot = Convert.ToInt16(chkdorNot) == 1 ? true : false;

            int intEventAccount_pKey = 0;
            Examcharges obj = new Examcharges();
            SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
            Label lblMsg = new Label();
            clsSettings cSettings = new clsSettings();
            cSettings.sqlconn = sqlConn;
            cSettings.lblMsg = lblMsg;
            cSettings.LoadSettings(sqlConn.ConnectionString);

            clsEventAccount cEventAccount = new clsEventAccount();
            cEventAccount.sqlConn = sqlConn;
            cEventAccount.lblMsg = lblMsg;
            cEventAccount.intEvent_pKey = Convert.ToInt32(Event_pkey);
            cEventAccount.intAccount_pKey = Convert.ToInt32(Account_pkey);
            cEventAccount.LoadEventInfo(true);
            intEventAccount_pKey = cEventAccount.intEventAccount_pKey;
            int ChargeType = Convert.ToInt32(chargeType_pkey);
            bool ChargeTypeExist = false;// alreadychecked.contains[]
            foreach (String val in alreadychecked)
            {
                if (val == chargeType_pkey)
                {
                    ChargeTypeExist = true;
                }
            }
            //token.Contains[chargeType_pkey.ToString()];
            if (checkedorNot == true & ChargeTypeExist == false)
            {
                DataTable dt = new DataTable();
                string qry = "SELECT ";
                qry = qry + Environment.NewLine + "(case when exists(select pkey from EventAccount_Certifications where EventAccount_pKey = @EventAccount_pKey And Certification_pKey=1) then 1 else 0 end) As bCMECert";
                qry = qry + Environment.NewLine + ",(case when exists(select pkey from EventAccount_Certifications where EventAccount_pKey = @EventAccount_pKey And Certification_pKey=2) then 1 else 0 end) As bCNECert";
                qry = qry + Environment.NewLine + ",(case when exists(select pkey from EventAccount_Certifications where EventAccount_pKey = @EventAccount_pKey And Certification_pKey=3) then 1 else 0 end) As bCCBCert";
                qry = qry + Environment.NewLine + ",(case when exists(select pkey from EventAccount_Certifications where EventAccount_pKey = @EventAccount_pKey And Certification_pKey=4) then 1 else 0 end) As bCLECert";
                qry = qry + Environment.NewLine + ",(case when exists(select pkey from EventAccount_Certifications where EventAccount_pKey = @EventAccount_pKey And Certification_pKey=5) then 1 else 0 end) As bCLECompactCert";
                qry = qry + Environment.NewLine + ",(case when exists(select pkey from EventAccount_Certifications where EventAccount_pKey = @EventAccount_pKey And Certification_pKey=6) then 1 else 0 end) As bCLEPACert";

                SqlCommand cmd = new SqlCommand(qry);
                cmd.Parameters.AddWithValue("@EventAccount_pKey", intEventAccount_pKey);
                if (!clsUtility.GetDataTable(sqlConn, cmd, ref dt))
                {
                    CheckOldCertification = false;

                }
                else if (dt.Rows.Count > 0)
                {
                    {
                        var withBlock = dt.Rows[0];
                        // --education
                        bCMECert = Convert.ToInt32(dt.Rows[0]["bCMECert"]);
                        bCNECert = Convert.ToInt32(dt.Rows[0]["bCNECert"]);
                        bCCBCert = Convert.ToInt32(dt.Rows[0]["bCCBCert"]);
                        bCLECert = Convert.ToInt32(dt.Rows[0]["bCLECert"]);
                        bCLEPACert = Convert.ToInt32(dt.Rows[0]["bCLEPACert"]);

                        bCLECompactCert = Convert.ToInt32(dt.Rows[0]["bCLECompactCert"]);
                    }
                }

                double Amount = 0;
                switch (ChargeType)
                {
                    case clsPrice.CHARGE_CRCPExam:
                        Amount = (-1.0) * Convert.ToDouble(cSettings.intSpkrCRCPCharge.ToString());
                        break;
                    case clsPrice.CHARGE_CME:
                        Amount = -1.0 * Convert.ToDouble(cSettings.intSpkrCMECharge.ToString());
                        break;
                    case clsPrice.CHARGE_CNE:
                        Amount = -1.0 * Convert.ToDouble(cSettings.intSpkrCNECharge.ToString());
                        break;
                    case clsPrice.CHARGE_CCB:
                        Amount = -1.0 * Convert.ToDouble(cSettings.intSpkrCCBCharge.ToString());
                        break;
                    case clsPrice.CHARGE_CLE:
                        Amount = -1.0 * Convert.ToDouble(cSettings.intSpkrCLECharge.ToString());
                        break;
                    case clsPrice.CHARGE_CLECompactStates:
                        Amount = -1.0 * Convert.ToDouble(cSettings.intSpkrCLECompactCharge.ToString());
                        break;
                    case clsPrice.CHARGE_CLEPA:
                        Amount = -1.0 * Convert.ToDouble(cSettings.intSpkrCLEPACharge.ToString());
                        break;

                }

                try
                {
                    double Amounts = Convert.ToDouble(Amount);
                    //return Ok(await repository.SaveExamcharges(Event_pkey, Account_pkey, Memo, chargeType_pkey, Amounts.ToString()));

                    string result = (await repository.SaveExamcharges(Event_pkey, Account_pkey, Memo, chargeType_pkey, Amounts.ToString()));
                    obj.Result = 0;
                    obj.Statusbit = true;
                    obj.ErrorMsg = "Saved sucessfully";
                    return Ok(obj);
                }
                catch (Exception ex)
                {
                    obj.Result = 4;
                    obj.Statusbit = false;
                    obj.ErrorMsg = ex.Message.ToString();
                    return Ok(obj);
                }
            }
            else if (checkedorNot == false & ChargeTypeExist == true)
            {

                clsEvent cEvent = new clsEvent();
                cEvent.lblMsg = lblMsg;
                cEvent.sqlConn = sqlConn;
                cEvent.intEvent_PKey = Convert.ToInt32(Event_pkey);
                cEvent.LoadEvent();
                clsAccount cWorkAccount = new clsAccount();
                cWorkAccount.lblMsg = lblMsg;
                cWorkAccount.sqlConn = sqlConn;
                cWorkAccount.intAccount_PKey = Convert.ToInt32(Account_pkey);
                cWorkAccount.LoadAccount();

                clsPrice cPrice = new clsPrice();
                cPrice.lblMsg = lblMsg;
                cPrice.sqlConn = sqlConn;
                int intPriorChargePKey = cPrice.FindCharge(Convert.ToInt32(Account_pkey), Convert.ToInt32(Event_pkey), Convert.ToInt32(chargeType_pkey));


                string strMemo = "Unchecked " + strchargeType.ToString() + " Charge (Trans#: " + intPriorChargePKey.ToString() + ").<br/>Via Options page";
                string qry = "Update Account_Charges set Reversed = 1 Where pKey = " + intPriorChargePKey.ToString() + ";";
                qry = qry + Environment.NewLine + "Update Account_Charges set IntendedCharges=REPLACE(IntendedCharges,'" + chargeType_pkey.ToString() + "','0') Where Account_pKey =" + Account_pkey.ToString() + " and Event_pKey=" + Event_pkey.ToString() + ";";
                qry = qry + Environment.NewLine + "Insert into Account_Charges(ChargeType_pKey, Account_pKey, Event_pKey, Amount";
                qry = qry + Environment.NewLine + ",LoggedByAccount_pKey, LoggedOn, Memo, ReversalReference)";
                qry = qry + Environment.NewLine + "select t1.ChargeType_pKey, t1.Account_pKey, t1.Event_pKey, -1.0*t1.Amount";
                qry = qry + Environment.NewLine + "," + Account_pkey.ToString() + ",getdate(),@Memo," + intPriorChargePKey.ToString();
                qry = qry + Environment.NewLine + " From Account_Charges t1";
                qry = qry + Environment.NewLine + " Where t1.pKey = " + intPriorChargePKey.ToString();
                SqlCommand cmd = new SqlCommand(qry);
                cmd.Parameters.AddWithValue("@Memo", Strings.Left(strMemo, 150));
                if (!clsUtility.ExecuteQuery(cmd, lblMsg, "Log Reversal"))
                {
                    obj.Result = 4;
                    obj.Statusbit = false;
                    obj.ErrorMsg = "Error".ToString();
                    cEvent = null;
                    cWorkAccount = null;
                    cPrice = null;
                    return Ok(obj);
                }
                else
                {
                    obj.Result = 0;
                    obj.Statusbit = true;
                    obj.ErrorMsg = "Saved sucessfully";
                    cWorkAccount.LogAuditMessage("Reverse " + strchargeType + " charge for event: " + cEvent.strEventID, clsAudit.LOG_CertificationDelete);
                    cEvent = null;
                    cWorkAccount = null;
                    cPrice = null;
                    return Ok(obj);
                }


                // --audit


            }

            obj.Result = 0;
            obj.Statusbit = true;
            obj.ErrorMsg = "Saved sucessfully".ToString();
            return Ok(obj);
        }

        [HttpPost]
        [Route("SpecialMeal/{Event_pkey}/{Meal_pkey}/{EventSession_pkey}/{Account_pkey}/{SpecialMealRequestRequest}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> SpecialMeal(string Event_pkey, string Meal_pkey, string EventSession_pkey, String Account_pkey, string SpecialMealRequestRequest)
        {
            MealSaveCondition obj = new MealSaveCondition();
            try
            {
                int intMealPKey = Convert.ToInt32(Meal_pkey);
                int intEvtSession_pKey = Convert.ToInt32(EventSession_pkey);
                SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
                Label lblMsg = new Label();
                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.sqlConn = sqlConn;
                cEventAccount.lblMsg = lblMsg;
                cEventAccount.intEvent_pKey = Convert.ToInt32(Event_pkey);
                cEventAccount.intAccount_pKey = Convert.ToInt32(Account_pkey);
                cEventAccount.LoadEventInfo(true);

                clsEventSession c = new clsEventSession();
                c.lblMsg = lblMsg;
                c.sqlConn = sqlConn;
                c.intEventSession_PKey = intEvtSession_pKey;
                c.intEvent_PKey = Convert.ToInt32(Event_pkey);
                if (!(c.SetAttendLunch(Convert.ToInt32(Account_pkey), true, intMealPKey, cEventAccount.intEventAccount_pKey, SpecialMealRequestRequest) == true))
                {
                    obj.Result = 3;
                    obj.Statusbit = false;
                    obj.ErrorMsg = "Error to save Attendee lunch Information ";
                    return Ok(obj);
                }
                else
                {
                    obj.Result = 0;
                    obj.Statusbit = true;
                    obj.ErrorMsg = "Saved sucessfully";
                    return Ok(obj);
                }
                cEventAccount = null;
                c = null/* TODO Change to default(_) if this is not a reference type */;

                //return Ok(await repository.LocationUpdate(List));
            }
            catch (Exception ex)
            {
                obj.Result = 4;
                obj.Statusbit = false;
                obj.ErrorMsg = ex.Message.ToString();
                return Ok(obj);
            }
        }


        [HttpPost]
        [Route("ConfirmLunch/{Event_pkey}/{Meal_pkey}/{EventSession_pkey}/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> ConfirmLunchs(string Event_pkey, string Meal_pkey, string EventSession_pkey, String Account_pkey)
        {
            MealSaveCondition obj = new MealSaveCondition();
            try
            {

                int intMealPKey = Convert.ToInt32(Meal_pkey);
                int intEvtSession_pKey = Convert.ToInt32(EventSession_pkey);
                SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
                Label lblMsg = new Label();
                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.sqlConn = sqlConn;
                cEventAccount.lblMsg = lblMsg;
                cEventAccount.intEvent_pKey = Convert.ToInt32(Event_pkey);
                cEventAccount.intAccount_pKey = Convert.ToInt32(Account_pkey);
                cEventAccount.LoadEventInfo(true);

                clsEventSession c = new clsEventSession();
                c.lblMsg = lblMsg;
                c.sqlConn = sqlConn;
                c.intEventSession_PKey = intEvtSession_pKey;
                c.intEvent_PKey = Convert.ToInt32(Event_pkey);
                if (!(c.SetAttendLunch(Convert.ToInt32(Account_pkey), true, intMealPKey, cEventAccount.intEventAccount_pKey) == true))
                {
                    obj.Result = 3;
                    obj.Statusbit = false;
                    obj.ErrorMsg = "Error to save Attendee lunch Information ";
                    return Ok(obj);
                }
                else
                {
                    obj.Result = 0;
                    obj.Statusbit = true;
                    obj.ErrorMsg = "Saved sucessfully";
                    return Ok(obj);
                }
                cEventAccount = null;
                c = null/* TODO Change to default(_) if this is not a reference type */;

                //return Ok(await repository.LocationUpdate(List));
            }
            catch (Exception ex)
            {
                obj.Result = 4;
                obj.Statusbit = false;
                obj.ErrorMsg = ex.Message.ToString();
                return Ok(obj);
            }
        }


        [HttpPost]
        [Route("SaveMealRequest/{Event_pkey}/{Meal_pkey}/{EventSession_pkey}/{bAttends}/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> SaveMealRequest(string Event_pkey, string Meal_pkey, string EventSession_pkey, Boolean bAttends, string Account_pkey)
        {
            MealSaveCondition obj = new MealSaveCondition();
            try
            {

                int intEvtSession_pKey = Convert.ToInt32(EventSession_pkey);
                bool bAttend = bAttends;
                int intMealPKey = Convert.ToInt32(Meal_pkey);
                //cb.Enabled = ck.Checked;

                DataTable dtMeal = clsEventSession.getSpecialMeal(intMealPKey);
                if (dtMeal.Rows.Count > 0 && bAttend)
                {
                    if (Convert.ToInt32(dtMeal.Rows[0]["AdditionalCharge"]).ToString() != "0")
                    {
                        obj.Result = 1;
                        obj.Statusbit = false;
                        obj.ErrorMsg = "Confirm lunch";
                        return Ok(obj);

                    }
                    else if (Convert.ToBoolean(dtMeal.Rows[0]["IsSpecial"]) == true)
                    {
                        obj.Result = 2;
                        obj.Statusbit = false;
                        obj.ErrorMsg = "Speacial Meal";
                        return Ok(obj);

                    }
                }
                SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
                Label lblMsg = new Label();

                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.sqlConn = sqlConn;
                cEventAccount.lblMsg = lblMsg;
                cEventAccount.intEvent_pKey = Convert.ToInt32(Event_pkey);
                cEventAccount.intAccount_pKey = Convert.ToInt32(Account_pkey);
                cEventAccount.LoadEventInfo(true);


                clsEventSession c = new clsEventSession();
                c.lblMsg = lblMsg;
                c.sqlConn = sqlConn;
                c.intEventSession_PKey = intEvtSession_pKey;
                c.intEvent_PKey = Convert.ToInt32(Event_pkey);// Event_pkey;
                if (!c.SetAttendLunch(Convert.ToInt32(Account_pkey), bAttend, intMealPKey, cEventAccount.intEventAccount_pKey))
                {
                    obj.Result = 3;
                    obj.Statusbit = false;
                    obj.ErrorMsg = "Error to save Attendee lunch Information ";
                    return Ok(obj);
                }
                else
                {
                    obj.Result = 0;
                    obj.Statusbit = true;
                    obj.ErrorMsg = "Saved sucessfully";
                    return Ok(obj);
                }
                cEventAccount = null;
                c = null/* TODO Change to default(_) if this is not a reference type */;

                //return Ok(await repository.LocationUpdate(List));
            }
            catch (Exception ex)
            {

                obj.Result = 4;
                obj.Statusbit = false;
                obj.ErrorMsg = ex.Message.ToString();
                return Ok(obj);

            }
        }


        private void SendEmail(string strContent, string txtName, string txtUserName, string txtUserEmail, int intUpdatePkey)
        {

            SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
            Label lblMsg = new Label();

            clsSettings cSettings = new clsSettings();
            cSettings.sqlconn = sqlConn;
            cSettings.lblMsg = lblMsg;
            cSettings.LoadSettings(sqlConn.ConnectionString);

            clsEmail cEmail = new clsEmail();
            cEmail.sqlConn = sqlConn;
            cEmail.lblMsg = lblMsg;
            cEmail.strEmailUserName = txtUserName.Trim();
            cEmail.strEmailFromAddress = txtUserEmail.Trim();

            // for support email from address

            // --send email
            string strSubject = txtName.Trim();
            if (strSubject == "")
                strSubject = "[User Issue]";
            cEmail.strSubjectLine = strSubject;

            string CC = "";
            string qry = "Select dbo.GetSecurityGroupEmail(37)";
            SqlCommand cmd = new SqlCommand(qry);
            DataTable dt = new DataTable();
            if (clsUtility.GetDataTable(sqlConn, cmd, ref dt))
            {
                if (dt.Rows.Count > 0)
                    CC = dt.Rows[0][0].ToString();
            }
            cEmail.strEmailCC = "#";
            //cEmail.strEmailBCC = Interaction.IIf(CC != "", CC.Replace(",", ";"), "#"); // "#" 
            cEmail.strHTMLContent = (intUpdatePkey > 0 ? "#" + intUpdatePkey.ToString() + "<br/>" : "") + strContent + "<br/> Submitter: " + txtUserName.Trim() + "<br/>Email:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + txtUserEmail.Trim();

            //cEmail.strAttachmentPath = myVS.strIssueFiles;
            if (!cEmail.SendEmailToAddress(cSettings.strSupportEmail, bPlainText: false))
                return;
            // If Not cEmail.SendUserIssueEmail(cSettings.strSupportEmail, bPlainText:=False) Then Exit Sub
            cEmail = null/* TODO Change to default(_) if this is not a reference type */;
        }


        [HttpPost]
        [Route("ConnectionrequestSave/{Event_pkey}/{ConnectionAccount_pkey}/{Account_pkey}/{Msg}/{IncE}/{IncP}/{Request}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> ConnectionrequestSave(String Event_pkey, String ConnectionAccount_pkey, string Account_pkey, string Msg, Boolean IncE, Boolean IncP, Boolean Request)
        {
            Instruction obj = new Instruction();
            SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
            Label lblMsg = new Label();
            clsSettings cSettings = new clsSettings();
            cSettings.sqlconn = sqlConn;
            cSettings.lblMsg = lblMsg;
            cSettings.LoadSettings(sqlConn.ConnectionString);

            clsAccount cAccount = new clsAccount();
            cAccount.lblMsg = lblMsg;
            cAccount.sqlConn = sqlConn;
            cAccount.intAccount_PKey = Convert.ToInt32(Account_pkey);
            cAccount.LoadAccount();

            clsAccount workAccount = new clsAccount();
            workAccount.lblMsg = lblMsg;
            workAccount.sqlConn = sqlConn;
            workAccount.intAccount_PKey = Convert.ToInt32(ConnectionAccount_pkey);
            workAccount.LoadAccount();
            //return Ok(await repository.ConnectionrequestSave(Event_pkey, ConnectionAccount_pkey, Account_pkey, Msg, IncE, IncP, Request));
            Boolean RES = false;
            try
            {

                RES = await repository.ConnectionrequestSave(Event_pkey, ConnectionAccount_pkey, Account_pkey, Msg, IncE, IncP, Request);
                if (RES == true)
                {
                    clsEmail cEMail = new clsEmail();
                    cEMail.sqlConn = sqlConn;
                    cEMail.lblMsg = lblMsg;
                    cEMail.strSubjectLine = "MAGI networking contact";
                    cEMail.strHTMLContent = Msg;
                    cEMail.strEmailFromAddress = cAccount.strEmail;
                    cEMail.strEmailUserName = cAccount.strFirstname + " " + cAccount.strLastname;

                    string SenderName = cAccount.strFirstname + " " + cAccount.strLastname;
                    string SenderEmailID = cAccount.strEmail;

                    if (cEMail.SendEmailToAcctPKey(Convert.ToInt32(Account_pkey), workAccount.strEmail, SenderEmailID: SenderEmailID, SenderName: SenderName, IsReplaceReplyaddress: true))
                    {

                    }
                }

                obj.Status = "Email Sent";
                workAccount = null;
                cAccount = null;
                return Ok(obj);

            }
            catch (Exception ex)
            {
                obj.Result = 1;
                obj.Status = ex.Message.ToString();
                return Ok(obj);
            }
        }

        [HttpPost]
        [Route("Instruction/{Event_pkey}/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Instruction(String Event_pkey, string Account_pkey)
        {
            Instruction obj = new Instruction();
            SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
            Label lblMsg = new Label();

            clsSettings cSettings = new clsSettings();
            cSettings.sqlconn = sqlConn;
            cSettings.lblMsg = lblMsg;
            cSettings.LoadSettings(sqlConn.ConnectionString);
            clsEvent cEvent = new clsEvent();
            cEvent.lblMsg = lblMsg;
            cEvent.sqlConn = sqlConn;
            cEvent.intEvent_PKey = Convert.ToInt32(Event_pkey);
            cEvent.LoadEvent();
            clsAccount cWorkAccount = new clsAccount();
            cWorkAccount.lblMsg = lblMsg;
            cWorkAccount.sqlConn = sqlConn;
            cWorkAccount.intAccount_PKey = Convert.ToInt32(Account_pkey);
            cWorkAccount.LoadAccount();
            string txt = "";
            try
            {
                txt = clsReservedWords.ReplaceMyPageText(lblMsg, cSettings.getText(clsSettings.TEXT_BadgeInstructions), cEvent, null, cWorkAccount);
                obj.Text = txt;
                obj.Result = 0;
                obj.Statusbit = true;
                obj.ErrorMsg = "";
                return Ok(obj);

            }
            catch (Exception ex)
            {

                obj.Result = 4;
                obj.Statusbit = false;
                obj.ErrorMsg = ex.Message.ToString();
                return Ok(obj);

            }
            cWorkAccount = null;
            cEvent = null;

        }
        [HttpPost]
        [Route("AttendeeQuestions_Save/{Question_pkey}/{Question}/{EventSession_pkey}/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> API_Attendee_Questions_Save(string Question_pkey, string Question, string EventSession_pkey, string Account_pkey)
        {
            try
            {
                return Ok(await repository.API_Attendee_Questions_Save(Question_pkey, Question, EventSession_pkey, Account_pkey));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }



        [HttpPost]
        [Route("IsVerifyUsedEmail/{StrEmail}/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> ValidateEmail(string StrEmail, string Account_pkey)
        {
            try
            {
                bool status = true;
                string strEMail = StrEmail;

                VerifyEmail obj = new VerifyEmail();
                if ((strEMail == "") | (!clsEmail.isAlreadyUsed(strEMail, Convert.ToInt32(Account_pkey))))
                {
                    obj.ISUsed = false;
                    return Ok(await Task.FromResult(obj));
                }
                else
                {
                    obj.ISUsed = true;
                    return Ok(await Task.FromResult(obj));
                }

            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }
        [HttpPost]
        [Route("Show_Sponsor_Schedule/{Event_pkey}/{Account_pkey}/{EventOrganizationpKey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Show_Sponsor_Schedule(string Event_pkey, string Account_pkey, string EventOrganizationpKey)
        {
            try
            {
                List<Meet> obj = await repository.Show_Sponsor_Schedule(Event_pkey, Account_pkey, EventOrganizationpKey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("Meeting_ScheduleAvailabl/{Event_pkey}/{Account_pkey}/{EventOrganizationpKey}/{UserAccount_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Meeting_ScheduleAvailable(string Event_pkey, string Account_pkey, string EventOrganizationpKey, string UserAccount_pkey)
        {
            try
            {
                List<MeetingScheduleAvailable> obj = await repository.MeetingScheduleAvailable(Event_pkey, Account_pkey, EventOrganizationpKey, UserAccount_pkey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("Updateeventattendee_schedule/{Event_pkey}/{Account_pkey}/{EventOrganizationpKey}/{EventSponsorPerson_pKey}/{BoothStaffSchedulepKey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> Updateeventattendee_schedule(string Event_pkey, string Account_pkey, string EventOrganizationpKey, string EventSponsorPerson_pKey, string BoothStaffSchedulepKey)
        {
            try
            {
                List<MeetingScheduleSave> obj = await repository.Updateeventattendee_schedule(Event_pkey, Account_pkey, EventOrganizationpKey, EventSponsorPerson_pKey, BoothStaffSchedulepKey);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("LicenseType")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> LicenseType()
        {
            try
            {
                List<DropdownListBind> obj = await repository.LicenseTypeList();
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("UpdateLicenseinfo")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> UpdateLicenseinfo([FromBody] License List)
        {
            try
            {
                return Ok(await repository.UpdateLicenseinfo(List));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }



        // public async Task<IHttpActionResult> badgeSaveEdit([FromBody] BadgeInfor List ,string Event_pkey,string Account_pkey)
        [HttpPost]
        [Route("badgeSaveEdit/{Event_pkey}/{Account_pkey}/{BName}/{BTitle}/{BOrganizationID}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> badgeSave(String Event_pkey, string Account_pkey, string BName, string BTitle, string BOrganizationID)
        {
            BadgeInfor obj = new BadgeInfor();
            try
            {
                Label lblMsg = new Label();
                SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
                SetContext(false);
                SetGlobal(sqlConn, lblMsg);

                clsSettings cSettings = new clsSettings();
                cSettings.sqlconn = sqlConn;
                cSettings.lblMsg = lblMsg;
                cSettings.LoadSettings(sqlConn.ConnectionString);

                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.sqlConn = sqlConn;
                cEventAccount.lblMsg = lblMsg;
                cEventAccount.intEvent_pKey = Convert.ToInt32(Event_pkey);
                cEventAccount.intAccount_pKey = Convert.ToInt32(Account_pkey);
                cEventAccount.LoadEventInfo(true);

                double dblAccountAmount = 0;
                dblAccountAmount = cEventAccount.dblAccountBalance;
                clsEvent cEvent = new clsEvent();
                cEvent.sqlConn = sqlConn;
                cEvent.lblMsg = lblMsg;
                cEvent.intEvent_PKey = Convert.ToInt32(Event_pkey);
                cEvent.LoadEvent();


                clsAccount cWorkAccount = new clsAccount();
                cWorkAccount.lblMsg = lblMsg;
                cWorkAccount.sqlConn = sqlConn;
                cWorkAccount.intAccount_PKey = Convert.ToInt32(Account_pkey);
                cWorkAccount.LoadAccount();

                if (cEvent.bEdBadge == true & (cEvent.bOptions && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "MyMAGIMenu_19")))
                {
                    if (((cEvent.intAttEdBadge > 0) | cEvent.intAttEdBadge > 0) & dblAccountAmount <= -cSettings.intAttAccessBal)
                    {
                        obj.IntResult = 1;
                        obj.Status = "To access this feature, please pay your balance due of " + String.Format("{0:c}", Convert.ToInt32(dblAccountAmount));
                        return Ok(obj);
                    }
                    else
                    {
                        clsBadgeDesign.SaveEdits(cWorkAccount, BName, BTitle, BOrganizationID, lblMsg, Convert.ToInt32(Account_pkey), "Attendee");
                        obj.IntResult = 0;
                        obj.Status = "Badge Saved";
                        return Ok(obj);
                    }

                }
                else
                {
                    obj.IntResult = 2;
                    obj.Status = "This feature is not yet available.";
                    return Ok(obj);
                }
                //return Ok(await repository.UpdateLicenseinfo(List));
                cWorkAccount = null;
                cEvent = null;
                cEventAccount = null;
                cSettings = null;
            }
            catch (Exception ex)
            {
                //return Ok(ex.Message.ToString());

                obj.IntResult = 3;
                obj.Status = ex.Message.ToString();
                return Ok(obj);

            }


        }



        [HttpPost]
        [Route("badgeReset/{Event_pkey}/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> badgeReset(String Event_pkey, string Account_pkey)
        {
            BadgeInfor obj = new BadgeInfor();
            try
            {
                Label lblMsg = new Label();
                SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
                SetContext(false);
                SetGlobal(sqlConn, lblMsg);

                clsSettings cSettings = new clsSettings();
                cSettings.sqlconn = sqlConn;
                cSettings.lblMsg = lblMsg;
                cSettings.LoadSettings(sqlConn.ConnectionString);

                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.sqlConn = sqlConn;
                cEventAccount.lblMsg = lblMsg;
                cEventAccount.intEvent_pKey = Convert.ToInt32(Event_pkey);
                cEventAccount.intAccount_pKey = Convert.ToInt32(Account_pkey);
                cEventAccount.LoadEventInfo(true);

                double dblAccountAmount = 0; Boolean bCurrentAttendee = false;
                dblAccountAmount = cEventAccount.dblAccountBalance;
                bCurrentAttendee = (cEventAccount.intParticipationStatus_pKey == 1) ? true : false;
                clsEvent cEvent = new clsEvent();
                cEvent.sqlConn = sqlConn;
                cEvent.lblMsg = lblMsg;
                cEvent.intEvent_PKey = Convert.ToInt32(Event_pkey);
                cEvent.LoadEvent();


                clsAccount cWorkAccount = new clsAccount();
                cWorkAccount.lblMsg = lblMsg;
                cWorkAccount.sqlConn = sqlConn;
                cWorkAccount.intAccount_PKey = Convert.ToInt32(Account_pkey);
                cWorkAccount.LoadAccount();



                if (cEvent.bEdBadge == true & (cEvent.bOptions && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "MyMAGIMenu_19")))
                {
                    if (((cEvent.intAttEdBadge > 0) | cEvent.intAttEdBadge > 0) & dblAccountAmount <= -cSettings.intAttAccessBal)
                    {
                        obj.IntResult = 1;
                        obj.Status = "To access this feature, please pay your balance due of " + String.Format("{0:c}", Convert.ToInt32(dblAccountAmount));
                        return Ok(obj);
                    }
                    else
                    {
                        DataTable dt = new DataTable();
                        string qry = "Select (isNull(t1.Firstname,'') + ' ' + isNull(t1.Lastname,'')) as BName, ISNULL(t1.Title,'') as BTitle, ISNULL(t2.OrganizationID,'') as BOrganizationID";
                        qry = qry + Environment.NewLine + " From account_List t1";
                        qry = qry + Environment.NewLine + " Left outer join Organization_list t2 on t2.pkey = t1.ParentOrganization_pKey";
                        qry = qry + Environment.NewLine + " Where t1.pKey = " + Account_pkey.ToString();

                        SqlCommand cmd = new SqlCommand(qry);
                        if (clsUtility.GetDataTable(sqlConn, cmd, ref dt))
                        {
                            obj.BName = dt.Rows[0]["BName"].ToString();
                            obj.BTitle = dt.Rows[0]["BTitle"].ToString();
                            obj.BOrganizationID = dt.Rows[0]["BOrganizationID"].ToString();
                            obj.BName = dt.Rows[0]["BName"].ToString();
                            obj.IntResult = 0;
                            obj.Status = "Reset profile info";
                            return Ok(obj);
                        }
                        else
                        {
                            obj.IntResult = 2;
                            obj.Status = "Some Error ";
                            return Ok(obj);
                        }



                    }

                }
                else
                {
                    obj.IntResult = 2;
                    obj.Status = "This feature is not yet available.";
                    return Ok(obj);
                }
                //return Ok(await repository.UpdateLicenseinfo(List));
                cWorkAccount = null;
                cEvent = null;
                cEventAccount = null;
                cSettings = null;
            }
            catch (Exception ex)
            {
                //return Ok(ex.Message.ToString());

                obj.IntResult = 3;
                obj.Status = ex.Message.ToString();
                return Ok(obj);

            }


        }

    

        [HttpPost]
        [Route("paymentGetWayInfo")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> paymentGetWayInfo()
        {
            GetWayInfo obj = new GetWayInfo();
            try
            {
                Label lblMsg = new Label();
                SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
                SetContext(false);
                SetGlobal(sqlConn, lblMsg);

                clsSettings cSettings = new clsSettings();
                cSettings.sqlconn = sqlConn;
                cSettings.lblMsg = lblMsg;
                cSettings.LoadSettings(sqlConn.ConnectionString);
                int cardtype = cSettings.intCardProcessor_pkey;
                int QAMODE = 0;
                if (ConfigurationManager.AppSettings["QAMode"].ToString() == "1")
                {
                    QAMODE = 1;
                }

                DataTable dt = new DataTable();
                string qry = "EXEC API_getPaymentDetails " + cardtype.ToString() + " , " + QAMODE.ToString();

                SqlCommand cmd = new SqlCommand(qry);
                if (clsUtility.GetDataTable(sqlConn, cmd, ref dt))
                {
                    if (dt.Rows.Count > 0)
                    {
                        obj.TestMode = Convert.ToBoolean(dt.Rows[0]["TestMode"].ToString());
                        obj.MerchantLogin = Convert.ToString(dt.Rows[0]["MerchantID"]);
                        obj.MerchantPassword = Convert.ToString(dt.Rows[0]["MerchantPW"]);
                        nsoftware.InPay.Icharge iCharge1 = new nsoftware.InPay.Icharge();
                        obj.GatewayURL = Convert.ToString(dt.Rows[0]["GatewayURL"]);
                        obj.Gateway = IchargeGateways.gwAuthorizeNet; //iCharge1.Gateway = IchargeGateways.gwAuthorizeNet
                        obj.bCreditCard_CVCode = Convert.ToBoolean(cSettings.bCreditCard_CVCode);
                        obj.bCreditCard_Singlename = Convert.ToBoolean(cSettings.bCreditCard_Singlename);
                        obj.bCreditCard_Firstlastname = Convert.ToBoolean(cSettings.bCreditCard_Firstlastname);
                        obj.bCreditCard_Zipcode = Convert.ToBoolean(cSettings.bCreditCard_Zipcode);
                        obj.bCreditCard_Address = Convert.ToBoolean(cSettings.bCreditCard_Address);
                        obj.strEventID = Convert.ToString(dt.Rows[0]["EventID"]);

                    }
                }
                //   List<GetWayInfo> obj = await repository.LicenseTypeList();
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("GetReceipt")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> GetReceipt()
        {
            try
            {
                ReturReceiptnum obj = new ReturReceiptnum();
                int Receipts = fcConferenceManager.clsPayment.ReturngetReceipt();
                obj.Receipt = Receipts.ToString();
                return Ok(obj);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }



        [HttpPost]
        [Route("PaymentUpdate")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> PaymentUpdate([FromBody] PaymentUpdate List)
        {
            PaymentResultStatus result = new PaymentResultStatus();
            result.Result = 0;
            result.Status = "Saved";
            try
            {
                Label lblMsg = new Label();
                SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
                clsSettings cSettings = new clsSettings();
                cSettings.sqlconn = sqlConn;
                cSettings.lblMsg = lblMsg;
                cSettings.LoadSettings(sqlConn.ConnectionString);

                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.sqlConn = sqlConn;
                cEventAccount.lblMsg = lblMsg;
                cEventAccount.intEvent_pKey = Convert.ToInt32(List.intEventPKey);
                cEventAccount.intAccount_pKey = Convert.ToInt32(List.intLoggedByAccount_pKey);
                cEventAccount.LoadEventInfo(true);

                double dblAccountAmount = 0;
                dblAccountAmount = cEventAccount.dblAccountBalance;
                clsEvent cEvent = new clsEvent();
                cEvent.sqlConn = sqlConn;
                cEvent.lblMsg = lblMsg;
                cEvent.intEvent_PKey = Convert.ToInt32(List.intEventPKey);
                cEvent.LoadEvent();


                clsAccount cAccount = new clsAccount();
                cAccount.lblMsg = lblMsg;
                cAccount.sqlConn = sqlConn;
                cAccount.intAccount_PKey = Convert.ToInt32(List.intLoggedByAccount_pKey);
                cAccount.LoadAccount();

                fcConferenceManager.clsPayment c = new clsPayment();
                var withBlock = c;
                withBlock.sqlConn = sqlConn;
                withBlock.lblMsg = lblMsg;

                // --prepare
                withBlock.intPaymentMethod_pKey = List.intPaymentMethod_pKey;
                withBlock.intPayerAcctPKey = cAccount.intAccount_PKey;
                withBlock.intEventPKey = cEvent.intEvent_PKey;
                withBlock.strEventID = cEvent.strEventID;
                // .strIntendedAccounts = Me.cAccount.intAccount_PKey.ToString
                withBlock.strIntendedAccounts = cEventAccount.intAccount_pKey.ToString();
                withBlock.intLoggedByAccount_pKey = cAccount.intAccount_PKey;

                // --credit card
                withBlock.strCardNumber = List.strCardNumber;
                withBlock.intCardType = Convert.ToInt32(List.intCardType);
                withBlock.strCardLastFour = List.strCardNumber.PadRight(4);
                // ToString("",PadRight(4, '0'));

                //Right[List.strCardNumber, 4];CCNo.Right(4))
                withBlock.strCardCode = List.strCardCode;
                withBlock.strCardname = List.strCardname;

                withBlock.strCardFirstname = List.strCardFirstName;
                withBlock.strCardLastname = List.strCardLastName;
                withBlock.strCardZipcode = List.strCardZipcode;
                withBlock.strCardAddress = List.strCardAddress;
                withBlock.strCardTransactionID = List.strCardTransactionID;
                // If Not Me.dpCreditDate.SelectedDate Is Nothing Then
                // .dtCardExpiration = Me.dpCreditDate.SelectedDate
                // Else
                // .dtCardExpiration = DateTime.MinValue
                // End If
                string strdate = "";
                //if (this.ddMonth.SelectedValue > 0 && this.ddYear.SelectedValue > 0)
                //{
                //    strdate = this.ddMonth.SelectedValue.ToString + "/" + "1" + "/" + this.ddYear.SelectedValue.ToString + " " + DateTime.Now.ToLongTimeString();
                //    withBlock.dtCardExpiration = Convert.ToDateTime(strdate).AddMonths(1).AddDays(-1);
                //}
                //else
                withBlock.dtCardExpiration = Convert.ToDateTime(List.CardExpiration);

                withBlock.dblAmount = List.dblAmount;

                // More information about customer
                withBlock.strCustomerId = Convert.ToInt32(cAccount.intAccount_PKey).ToString();
                withBlock.strCustomerName = cAccount.strContactName;
                withBlock.strCustomerAddress = cAccount.strAddress1 + " " + cAccount.strAddress2;
                withBlock.strCustomerCompany = cAccount.strOrganizationID;
                withBlock.strCustomerZip = cAccount.strZip;
                withBlock.strCustomerFName = cAccount.strFirstname;
                withBlock.strCustomerLName = cAccount.strLastname;
                withBlock.strCustomerFax = cAccount.strFax;
                withBlock.strCustomerEmail = cAccount.strEmail;
                withBlock.strCustomerCity = cAccount.strCity;
                withBlock.strCustomerState = cAccount.strState;
                withBlock.strCustomerCountry = cAccount.strCountry;
                withBlock.strCustomerPhone = cAccount.strPhone;
                withBlock.strSelectedCharges = "";
                withBlock.strSelectedChargesPkey = "";
                if (withBlock.dblAmount <= 0)
                {
                    //this.lblCardError.Text = "The entered amount must be a valid currency and > 0. Correct and try again. If problems persist, contact MAGI.";
                    //this.lblCardError.Visible = true;
                    //return;
                }

                //this.txtCCNum.BackColor = IIf(Len(withBlock.strCardNumber) < 16, Color.LightCoral, Color.White);
                //this.txtCreditCode.BackColor = IIf(withBlock.strCardCode == "", Color.LightCoral, Color.White);
                //this.txtCreditName.BackColor = IIf(withBlock.strCardname == "", Color.LightCoral, Color.White);
                //this.txtCreditFirstName.BackColor = IIf(withBlock.strCardFirstname == "", Color.LightCoral, Color.White);
                //this.txtCreditLastname.BackColor = IIf(withBlock.strCardLastname == "", Color.LightCoral, Color.White);
                //this.txtCreditZipcode.BackColor = IIf(withBlock.strCardZipcode == "", Color.LightCoral, Color.White);
                //this.txtCreditAddress.BackColor = IIf(withBlock.strCardAddress == "", Color.LightCoral, Color.White);
                // Me.dpCreditDate.BackColor = IIf(.dtCardExpiration = DateTime.MinValue, Color.LightCoral, Color.White)

                // --do the posting
                Boolean bPaymentResult = true;// ' withBlock.PostPayment(iCharge1: this.Icharge1, CardValidator1: this.Cardvalidator1);
                int intReceiptNumber = 0;
                withBlock.intReceiptNumber = List.intReceiptNumber;
                // strPaymentProblem = string.Join(", ", withBlock.lstErrors.ToArray);

                if (!bPaymentResult)
                {
                    //string strError = "Payment was unsuccessful due to: " + strPaymentProblem.TrimEnd(".") + ". Correct and try again. If problems persist, <a href=" + Strings.Chr(34) + cSettings.strWebSiteURL + "/forms/SendEmail.aspx?E=1&S=MAGI+Support" + Strings.Chr(34) + " target='_blank'>contact MAGI</a>.";
                    //this.lblCardError.Text = strError.Replace("This transaction has been declined. Correct and try again", "This transaction has been declined by your credit/debit card company. Tell them to allow the charge and then try again");
                    //this.lblCardError.Visible = true;
                    //return;
                }
                else
                    // this.lblCardError.Visible = false;

                    // --log the payment
                    // .strMemo = "Added by: " + Me.cAccount.strContactName
                    withBlock.strMemo = "Payment made";
                // --force the payment to be 'paid' for manual postings
                withBlock.bPaid = true;

                // --save the payment to the log
                if (!withBlock.LogPayment())
                {
                    result.Result = 1;
                    result.Status = cSettings.getErrorcode(227);
                    return Ok(result);
                }

                // --if paid (i.e. credit card), log the payment to the attendee account
                if (withBlock.bPaid)
                {
                    if (!withBlock.ApplyCashToAccounts(withBlock.dblAmount, withBlock.dblAmount))
                    {
                        result.Result = 1;
                        result.Status = cSettings.getErrorcode(228);
                        return Ok(result);
                    }
                }
                // --audit
                cEventAccount.LogAuditMessage("Log payment on behalf of account: " + cEventAccount.intAccount_pKey.ToString() + " for event: " + cEvent.strEventID, clsAudit.LOG_Payment);

                if (Convert.ToInt32(List.intReceiptNumber) > 0)
                    clsPayment.SendPaymentEmail(List.intLoggedByAccount_pKey, clsPayment.METHOD_Credit, intReceiptNumber);
                clsReminders cReminder = new clsReminders();
                cReminder.UserReminderStatusUpdate(List.intEventPKey, List.intLoggedByAccount_pKey, clsReminders.R_PaymentDue);

                c = null;
                cAccount = null;
                cEvent = null;
                cEventAccount = null;
                return Ok(result);
                //return Ok(await repository.UserUpdateDetail(List));
            }
            catch (Exception ex)
            {
                result.Result = 1;
                result.Status = ex.Message.ToString();
                return Ok(result);
            }
        }




        [HttpPost]
        [Route("SpeakerDinner/{Event_pkey}/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> SpeakerDinner(String Event_pkey, string Account_pkey)
        {
            Speakerdinnertext obj = new Speakerdinnertext();
            try
            {
                Label lblMsg = new Label();
                SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
                SetContext(false);
                SetGlobal(sqlConn, lblMsg);

                clsSettings cSettings = new clsSettings();
                cSettings.sqlconn = sqlConn;
                cSettings.lblMsg = lblMsg;
                cSettings.LoadSettings(sqlConn.ConnectionString);

                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.sqlConn = sqlConn;
                cEventAccount.lblMsg = lblMsg;
                cEventAccount.intEvent_pKey = Convert.ToInt32(Event_pkey);
                cEventAccount.intAccount_pKey = Convert.ToInt32(Account_pkey);
                cEventAccount.LoadEventInfo(true);

                double dblAccountAmount = 0; Boolean bCurrentAttendee = false;
                dblAccountAmount = cEventAccount.dblAccountBalance;
                bCurrentAttendee = (cEventAccount.intParticipationStatus_pKey == 1) ? true : false;
                clsEvent cEvent = new clsEvent();
                cEvent.sqlConn = sqlConn;
                cEvent.lblMsg = lblMsg;
                cEvent.intEvent_PKey = Convert.ToInt32(Event_pkey);
                cEvent.LoadEvent();


                clsAccount cWorkAccount = new clsAccount();
                cWorkAccount.lblMsg = lblMsg;
                cWorkAccount.sqlConn = sqlConn;
                cWorkAccount.intAccount_PKey = Convert.ToInt32(Account_pkey);
                cWorkAccount.LoadAccount();

                obj.contant = clsReservedWords.ReplaceMyPageText(lblMsg, cEvent.strSpeakerDinnerText, cEvent, null, cWorkAccount, cEventAccount);
                obj.ChangeText = cSettings.getErrorcode(229);
                obj.Change = cSettings.getErrorcode(230);
                obj.bCurrentAttendee = bCurrentAttendee;
                obj.IntResult = 0;
                obj.Status = "";
                //return Ok(await repository.UpdateLicenseinfo(List));
                cWorkAccount = null;
                cEvent = null;
                cEventAccount = null;
                cSettings = null;

                return Ok(obj);
            }
            catch (Exception ex)
            {
                //return Ok(ex.Message.ToString());

                obj.IntResult = 3;
                obj.Status = ex.Message.ToString();
                return Ok(obj);

            }


        }
     

        [HttpPost]
        [Route("ChatAllowed/{Event_pkey}/{Account_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> ChatAllowed(String Event_pkey, string Account_pkey)
        {
            chatEnabledisable obj = new chatEnabledisable();
            try
            {
                Label lblMsg = new Label();
                SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
                SetContext(false);
                SetGlobal(sqlConn, lblMsg);

                clsSettings cSettings = new clsSettings();
                cSettings.sqlconn = sqlConn;
                cSettings.lblMsg = lblMsg;
                cSettings.LoadSettings(sqlConn.ConnectionString);

                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.sqlConn = sqlConn;
                cEventAccount.lblMsg = lblMsg;
                cEventAccount.intEvent_pKey = Convert.ToInt32(Event_pkey);
                cEventAccount.intAccount_pKey = Convert.ToInt32(Account_pkey);
                cEventAccount.LoadEventInfo(true);

                double dblAccountAmount = 0; Boolean bCurrentAttendee = false;
                dblAccountAmount = cEventAccount.dblAccountBalance;
                bCurrentAttendee = (cEventAccount.intParticipationStatus_pKey == 1) ? true : false;
                clsEvent cEvent = new clsEvent();
                cEvent.sqlConn = sqlConn;
                cEvent.lblMsg = lblMsg;
                cEvent.intEvent_PKey = Convert.ToInt32(Event_pkey);
                cEvent.LoadEvent();

                Boolean ISchatallowed = false;
                clsAccount cWorkAccount = new clsAccount();
                cWorkAccount.lblMsg = lblMsg;
                cWorkAccount.sqlConn = sqlConn;
                cWorkAccount.intAccount_PKey = Convert.ToInt32(Account_pkey);
                cWorkAccount.LoadAccount();

                DateTime dt1 = Convert.ToDateTime(cEvent.dtEndDate.ToShortDateString());
                DateTime dt2 = Convert.ToDateTime(clsEvent.getEventVenueTime().ToShortDateString());  // --'DateTime.ParseExact(clsEvent.getEventVenueTime().ToString(), "MM/dd/yy", null);
                if  (dt1 >=  dt2)
                {
                    if (bCurrentAttendee == true && !(cEventAccount.Isnetworkingentirely == true))
                    {
                        bool chatON = (cEvent.bChatOnOff && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "IsChatOn"));
                        if (((cWorkAccount.bGlobalAdministrator == true || cWorkAccount.bStaffMember == true) && chatON || (cEvent.bshowChatGadgetToStaff && cEvent.CheckValiditityOfModule(cEvent.intEvent_PKey, "showChatGadgetToStaff"))))
                        {
                            ISchatallowed = true;
                        }

                        if ((bCurrentAttendee == true) && chatON)
                        {
                            ISchatallowed = true;

                        }
                        
                    }
                }
                obj.chatallowed = ISchatallowed;
                obj.IntResult = 0;
                obj.Status = "";
                //    obj.contant = clsReservedWords.ReplaceMyPageText(lblMsg, cEvent.strSpeakerDinnerText, cEvent, null, cWorkAccount, cEventAccount);
                //obj.ChangeText = cSettings.getErrorcode(229);
                //obj.Change = cSettings.getErrorcode(230);
                //obj.bCurrentAttendee = bCurrentAttendee;
                //obj.IntResult = 0;
                //obj.Status = "";
                //return Ok(await repository.UpdateLicenseinfo(List));
                cWorkAccount = null;
                cEvent = null;
                cEventAccount = null;
                cSettings = null;

                return Ok(obj);
            }
            catch (Exception ex)
            {
                //return Ok(ex.Message.ToString());

                obj.IntResult = 3;
                obj.Status = ex.Message.ToString();
                return Ok(obj);

            }


        }


        [HttpPost]
        [Route("POLLINGRESULTSHOWORNOT/{pkey}/{ShowResult}/{EventSession_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> POLLINGRESULTSHOWORNOT(string pkey, Boolean ShowResult, string EventSession_pkey)
        {
            try
            {
                return Ok(await repository.POLLINGRESULTSHOWORNOT(pkey, ShowResult, EventSession_pkey));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("POLLINGSTARTORNOT/{pkey}/{IsStarted}/{EventSession_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> POLLINGSTARTORNOT(string pkey, Boolean IsStarted, string EventSession_pkey)
        {
            try
            {
                return Ok(await repository.POLLINGSTARTORNOT(pkey, IsStarted, EventSession_pkey));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }


        [HttpPost]
        [Route("SpeakerRefreshScreen/{Account_pkey}/{Event_pkey}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> SpeakerRefreshScreen(string Account_pkey, string Event_pkey)
        {
            SpeakerRefreshScreen result = new SpeakerRefreshScreen();
            result.Result = 0;
            result.Status = "Saved";
            try
            {
                Label lblMsg = new Label();
                SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
                clsSettings cSettings = new clsSettings();
                cSettings.sqlconn = sqlConn;
                cSettings.lblMsg = lblMsg;
                cSettings.LoadSettings(sqlConn.ConnectionString);

                clsEvent cEvent = new clsEvent();
                cEvent.sqlConn = sqlConn;
                cEvent.lblMsg = lblMsg;
                cEvent.intEvent_PKey = Convert.ToInt32(Event_pkey);
                cEvent.LoadEvent();

                DateTime dtCaliforniatime = clsEvent.getCaliforniaTime();
                Boolean bAllowRegOnline = (dtCaliforniatime <= clsUtility.getEndOfDay(cEvent.dtDinnerSignupEndDate));
                result.intMaxDinnerGuest = cEvent.intMaxDinnerGuest;
                if (bAllowRegOnline == false)
                {
                    result.Warning = "Online registration for the speaker dinner is no longer available. Check at the registration desk for availability";
                }

                clsEventAccount cEventAccount = new clsEventAccount();
                cEventAccount.sqlConn = sqlConn;
                cEventAccount.lblMsg = lblMsg;
                cEventAccount.intEvent_pKey = Convert.ToInt32(Event_pkey);
                cEventAccount.intAccount_pKey = Convert.ToInt32(Account_pkey);
                cEventAccount.LoadEventInfo(true);

                result.AttendingStatus = cEventAccount.intDinnerStatus_pKey;
                result.dblDinnerLateCharge = cEventAccount.dblDinnerLateCharge;
                result.EventAccountpKey = Convert.ToInt32(cEventAccount.intEventAccount_pKey).ToString();

                result.pnlNotAttending = false;
                result.Enrolled = false;
                result.pnlRegister = false;

                switch (cEventAccount.intDinnerStatus_pKey)
                {
                    case clsEventAccount.DINNER_AttendingNotPaid:
                        {
                            result.Enrolled = bAllowRegOnline;
                            result.phNotPaid = cEventAccount.intDinnerStatus_pKey == clsEventAccount.DINNER_AttendingNotPaid ? true : false;
                            result.phIsPaid = cEventAccount.intDinnerStatus_pKey == clsEventAccount.DINNER_AttendingPaid ? true : false;
                            break;
                        }

                    case clsEventAccount.DINNER_AttendingPaid:
                        {
                            result.Enrolled = bAllowRegOnline;
                            result.phNotPaid = cEventAccount.intDinnerStatus_pKey == clsEventAccount.DINNER_AttendingNotPaid ? true : false;
                            result.phIsPaid = cEventAccount.intDinnerStatus_pKey == clsEventAccount.DINNER_AttendingPaid ? true : false;
                            break;
                        }


                    case clsEventAccount.DINNER_NotAttending:
                        {
                            result.pnlNotAttending = true;
                            result.cmdChange = bAllowRegOnline;
                            result.rdReg = 1;
                            break;
                        }
                    case clsEventAccount.DINNER_NotDecided:
                        {
                            result.pnlRegister = true;
                            result.trDinnerGuest = false;
                            result.rdReg = 0;
                            //result.cmdReg.Text = "Submit"
                            break;
                        }
                    default:
                        {
                            result.pnlRegister = bAllowRegOnline;
                            // Me.pnlRegister.Visible = True
                            result.trDinnerGuest = false;
                            //this.cmdReg.Text = "Submit";
                            break;
                        }
                }

                double dblAccountAmount = 0;
                dblAccountAmount = cEventAccount.dblAccountBalance;

                result.GuestMSG = "Dinner for " + cEventAccount.intDinnerGuests.ToString() + " Guest" + (cEventAccount.intDinnerGuests != 1 ? "s" : "");
                result.lblSpeakerCharge = string.Format("{0:c}", cEventAccount.dblDinnerSpeakerCharge);
                result.lblGuestCharge = string.Format("{0:c}", cEventAccount.dblDinnerGuestCharge);
                result.lblTotalCost = string.Format("{0:c}", cEventAccount.dblDinnerSpeakerCharge + cEventAccount.dblDinnerGuestCharge);

                //this.ckPaylater.Visible = cEvent.bSpeakerDinnerAllowLatePay;
                // Me.trDinnerGuest.Visible = (myVS.intMaxDinnerGuest <> 0 AndAlso .intDinnerStatus_pKey <> clsEventAccount.DINNER_NotDecided)
                if (result.intMaxDinnerGuest == -1)
                {
                    result.intMaxDinnerGuest = 4;
                }
                // Me.lblMaxGuests.Text = "(Maximum: " + myVS.intMaxDinnerGuest.ToString + ")"
                //this.rblDinnerGuest.Items.Clear();
                // For index As Integer = 1 To myVS.intMaxDinnerGuest
                //for (int index = 0; index <= myVS.intMaxDinnerGuest; index++)
                //    rblDinnerGuest.Items.Add(index.ToString());
                //rblDinnerGuest.SelectedIndex = 0;
                // Dim dblDinnerLateCharge As Double = Me.CheckDinnerLateCharge()
                // If dblDinnerLateCharge > 0 Then
                //trDinnerDiscount.Visible = true;

                //clsAccount cAccount = new clsAccount();
                //cAccount.lblMsg = lblMsg;
                //cAccount.sqlConn = sqlConn;
                //cAccount.intAccount_PKey = Convert.ToInt32();
                //cAccount.LoadAccount();

                cEventAccount = null;
                cEvent = null;
                cSettings = null;
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.Result = 1;
                result.Status = ex.Message.ToString();
                return Ok(result);
            }
        }


        [HttpPost]
        //[Route("SaveExamcharges/{Event_pkey}/{Account_pkey}/{Memo}/{chargeType_pkey}/{CheckedItem}/{chkdorNot}/{strchargeType}")]
        [Route("SpeakerDinnerSave")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> SaveExamcharges([FromBody] SpeakerDinnerSave List)
        {
                

        CommonStatus obj = new CommonStatus();
            obj.Status = "";
            obj.Result = 0;
            obj.Statusbit = true;

            string Event_pkey = List.Event_pkey; // "";
            string Account_pkey = List.Account_pkey.ToString();// "";
            string intdinnerValue = List.intdinnerValue.ToString();// "";
            string numDinnerGuest = List.numDinnerGuest.ToString();// "";
            string numvegetarianmeals = List.numvegetarianmeals.ToString();//"";
            string numglutenmeals = List.numglutenmeals.ToString();// "";
            Boolean paylater = Convert.ToBoolean(List.paylater.ToString());
            int MAxDinnerGust = List.MAxDinnerGust;
            string DiscountCode = List.DiscountCode.ToString();
            string EventAccount_pkey = List.EventAccount_pkey.ToString();
            int intEventAccount_pKey = 0;
            int intDiscountTypePkey = List.intDiscountPkey;
            SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
            Label lblMsg = new Label();
            clsSettings cSettings = new clsSettings();
            cSettings.sqlconn = sqlConn;
            cSettings.lblMsg = lblMsg;
            cSettings.LoadSettings(sqlConn.ConnectionString);

            clsEventAccount cEventAccount = new clsEventAccount();
            cEventAccount.sqlConn = sqlConn;
            cEventAccount.lblMsg = lblMsg;
            cEventAccount.intEvent_pKey = Convert.ToInt32(Event_pkey);
            cEventAccount.intAccount_pKey = Convert.ToInt32(Account_pkey);
            cEventAccount.LoadEventInfo(true);
            intEventAccount_pKey = cEventAccount.intEventAccount_pKey;


            clsAccount cAccount = new clsAccount();
            cAccount.lblMsg = lblMsg;
            cAccount.sqlConn = sqlConn;
            cAccount.intAccount_PKey = Convert.ToInt32(Account_pkey);
            cAccount.LoadAccount();

            clsEvent cEvent = new clsEvent();
            cEvent.lblMsg = lblMsg;
            cEvent.sqlConn = sqlConn;
            cEvent.intEvent_PKey = Convert.ToInt32(Event_pkey);
            cEvent.LoadEvent();
            //clsAccount cWorkAccount = new clsAccount();
            //cWorkAccount.lblMsg = lblMsg;
            //cWorkAccount.sqlConn = sqlConn;
            //cWorkAccount.intAccount_PKey = Convert.ToInt32(Account_pkey);
            //cWorkAccount.LoadAccount();



            try
            {
                int intSelection = Convert.ToInt32(intdinnerValue);
                int intMaxDinnerGuest = 0;
                
                double dblDiscount = 0;
                switch (intSelection)
                {
                    case 0:
                        {
                            cEventAccount.UpdateDinnerStatus(clsEventAccount.DINNER_NotDecided);
                            obj.Result = 0;
                            obj.Statusbit = true;
                            obj.Status = "Saved sucessfully";
                            return Ok(obj);
                            break;
                        }
                    case 1:
                        {
                            cEventAccount.UpdateDinnerStatus(clsEventAccount.DINNER_NotAttending);
                            obj.Result = 0;
                            obj.Statusbit = true;
                            obj.Status = "Saved sucessfully";
                            return Ok(obj);
                            break;
                        }
                    case 2:
                        {
                            double dblAmt = 0;
                            double dblDinnerLateCharge  = 0;
                            DateTime dtCaliforniatime = clsEvent.getCaliforniaTime();
                            bool bConferenceEnd = (dtCaliforniatime > cEvent.dtDinnerChargeAppliedAfter.AddDays(1).Date);
                             dblDinnerLateCharge = cEventAccount.dblDinnerLateCharge > -1 ? cEventAccount.dblDinnerLateCharge : cEvent.dblDinnerLateCharge;
                            dblDinnerLateCharge = bConferenceEnd ? dblDinnerLateCharge : 0;

                            int intGuests = Convert.ToInt32(numDinnerGuest);
                            int intVeg = Convert.ToInt32(numvegetarianmeals);
                            int intGluten = Convert.ToInt32(numglutenmeals);
                            intMaxDinnerGuest = MAxDinnerGust;

                            if (intMaxDinnerGuest > 0)
                            {
                                if (intGuests > (intMaxDinnerGuest))
                                {
                                    clsUtility.LogErrorMessage(lblMsg, null, this.GetType().Name, 0, cSettings.getText(clsSettings.Text_MaxDinnerGuestLimit) + " " + (intMaxDinnerGuest).ToString());
                                    obj.Result = 1;
                                    obj.Status = lblMsg.Text;
                                    return Ok(obj);
                                }
                            }
                            else
                            {
                                clsUtility.LogErrorMessage(lblMsg, null, this.GetType().Name, 0, cSettings.getText(clsSettings.Text_MaxDinnerGuestLimit) + " " + (intMaxDinnerGuest).ToString());
                                obj.Result = 1;
                                obj.Status = lblMsg.Text;
                                return Ok(obj);
                            }

                            if ((intVeg + intGluten) > (intGuests + 1))
                            {
                                clsUtility.LogErrorMessage(lblMsg, null, this.GetType().Name, 0, "The number of special meals cannot exceed the number attending (" + (intGuests + 1).ToString() + ")");
                                obj.Result = 1;
                                obj.Status = lblMsg.Text;
                                return Ok(obj); 
                            }


                            clsCharge c = new clsCharge();
                            c.intChargeType_pKey = clsPrice.CHARGE_SpeakerDinner;
                            // c.dblAmount = Me.cEvent.dblDinnerGuestCharge
                            c.dblAmount = cEvent.dblDinnerCharge + cEvent.dblDinnerLateCharge; // dblDinnerLateCharge
                            c.strMemo = "Added speaker dinner";
                            dblAmt = dblAmt + c.dblAmount;
                            colCharges.Add(c);
                            c = null;
                            if (List.intDiscountPkey > 0 && paylater == false)
                            {
                                c = new clsCharge();
                                c.intChargeType_pKey = clsPrice.CHARGE_Discount;
                                // c.dblAmount = Me.cEvent.dblDinnerGuestCharge
                                if (intDiscountTypePkey == clsDiscount.TYPE_FreeDinner)
                                    c.dblAmount = -1 * cEvent.dblDinnerCharge;
                                if (intDiscountTypePkey == clsDiscount.TYPE_FreeDinnerSpeakerOnly)
                                    c.dblAmount = -1 * cEvent.dblDinnerCharge;
                                if (intDiscountTypePkey == clsDiscount.TYPE_FreeDinnerSpeakerAndGuest)
                                    c.dblAmount = -1 * cEvent.dblDinnerCharge;
                                // c.dblAmount = -1 * Me.cEvent.dblDinnerCharge
                                c.strMemo = "Free Speaker Dinner";
                                c.strDiscountCodeApplied = List.DiscountCode;
                                dblAmt = dblAmt + c.dblAmount;
                                dblDiscount = cEvent.dblDinnerCharge;
                                colCharges.Add(c);
                                c = null/* TODO Change to default(_) if this is not a reference type */;
                                obj.Result = 4;
                                obj.Status = "Functionality not available in this time ";
                                return Ok(obj);
                            }


                            if (paylater == true)
                            {
                                string s = "";
                                if (cEvent.strSpeakerDinnerReceiptDueText != "")
                                {
                                    s = Microsoft.VisualBasic.Strings.Replace(cEvent.strSpeakerDinnerReceiptDueText, "[DinnerAmt]", string.Format("{0:c}", dblAmt), 1, -1, Constants.vbTextCompare);
                                    s = Microsoft.VisualBasic.Strings.Replace(s, "[DinnerName]", (cAccount.strFirstname != "" ? cAccount.strFirstname + " " : "") + (cAccount.strMiddlename != "" ? cAccount.strMiddlename + " " : "") + (cAccount.strLastname != "" ? cAccount.strLastname + " " : ""), 1, -1, Constants.vbTextCompare);
                                    s = Microsoft.VisualBasic.Strings.Replace(s, "[DinnerGuest]", intGuests > 0 ? " and " + intGuests.ToString() + Interaction.IIf(intGuests != 1, " guests", " guest") : "", 1, -1, Constants.vbTextCompare);
                                    s = clsReservedWords.ReplaceMyPageText(lblMsg, s, cEvent, null/* TODO Change to default(_) if this is not a reference type */, cAccount, cEventAccount) + "";
                                }
                                else
                                {
                                    s = "Payment of " + string.Format("{0:c}", dblAmt) + " for " + (cAccount.strFirstname != "" ? cAccount.strFirstname + " " : "") + (cAccount.strMiddlename != "" ? cAccount.strMiddlename + " " : "") + (cAccount.strLastname != "" ? cAccount.strLastname + " " : "") + "";
                                    //s = s + Convert.ToInt32(intGuests) > 0 ? " and " + intGuests.ToString() + Interaction.IIf(intGuests != 1, " guests", " guest") : "" + " Is required by " + string.Format("{0:MMMM dd, yyyy}", cEvent.dtDinnerDate) + ".";

                                    s = s + (intGuests > 0? " and " + intGuests.ToString() + (intGuests != 1 ? " guests" : " guest"): "") + " Is required by " + String.Format("{0:MMMM dd, yyyy}", cEvent.dtDinnerDate) + ".";
                                }
                                //obj.lblLater = s.ToString();
                                obj.Result = 2;
                                obj.Status = s.ToString();
                                return Ok(obj);
                                //clsUtility.PopupRadWindow(ScriptManager.GetCurrent(this.Page), this.Page, this.rwLater);

                            }
                            else
                            {
                                obj.Result = 3;
                                obj.Status = "open Payment Window";
                                return Ok(obj);
                            }
                                break;
                        }


                        obj.Result = 0;
                        obj.Status = "Saved";
                        return Ok(obj);

                }
                obj.Result = 0;
                obj.Status = "Saved";
                return Ok(obj);

                //return Ok(await repository.SaveExamcharges(Event_pkey, Account_pkey, Memo, chargeType_pkey, Amounts.ToString()));



            }
            catch (Exception ex)
            {
                obj.Result = 4;
                obj.Statusbit = false;
                obj.Status = ex.Message.ToString();
                return Ok(obj);
            }


            // --audit




            //obj.Result = 0;
            //obj.Statusbit = true;
            //obj.ErrorMsg = "Saved sucessfully".ToString();
            //return Ok(obj);
        }

        [HttpPost]
        //[Route("SaveExamcharges/{Event_pkey}/{Account_pkey}/{Memo}/{chargeType_pkey}/{CheckedItem}/{chkdorNot}/{strchargeType}")]
        [Route("DiscountCodeApply/{discountCode}/{Account_pkey}/{Event_pkey}/{intGuest}")]
        [Authorize(Roles = "KeyUser")]
        public async Task<IHttpActionResult> DiscountCodeApply(string discountCode , string  Account_pkey ,string Event_pkey ,string intGuest)
        {
            DiscountCodeApply obj = new DiscountCodeApply();
            string strCode = discountCode;
            obj.Status = "";
            obj.Result = 0;
            obj.Statusbit = true;
            SqlConnection sqlConn = new SqlConnection(ReadConnectionString());
            Label lblMsg = new Label();
            clsSettings cSettings = new clsSettings();
            cSettings.sqlconn = sqlConn;
            cSettings.lblMsg = lblMsg;
            cSettings.LoadSettings(sqlConn.ConnectionString);

            clsAccount cAccount = new clsAccount();
            cAccount.lblMsg = lblMsg;
            cAccount.sqlConn = sqlConn;
            cAccount.intAccount_PKey = Convert.ToInt32(Account_pkey);
            cAccount.LoadAccount();

            clsEvent cEvent = new clsEvent();
            cEvent.lblMsg = lblMsg;
            cEvent.sqlConn = sqlConn;
            cEvent.intEvent_PKey = Convert.ToInt32(Event_pkey);
            cEvent.LoadEvent();

            clsPrice cp = new clsPrice();
            cp.sqlConn = sqlConn;
            cp.lblMsg = lblMsg;
            cp.strDiscountCode = strCode;
            cp.intEventPKey =Convert.ToInt32(Event_pkey);
            cp.intOrganizationPKey = cAccount.intParentOrganization_pKey;
            cp.bValidDiscount = true;
            int intCodePKey = 0;
            intCodePKey = cp.isValidDiscountCode(3);

            try
            {
                if ((!cp.bValidDiscount) | (intCodePKey <= 0))
                {
                    obj.Status = "Invalid discount code";
                    obj.Result = 1;
                    obj.Statusbit = false;

                    return Ok(obj); 
                }
                cp = null/* TODO Change to default(_) if this is not a reference type */;

                //obj.Status = "";
                //obj.Result = 0;
                //obj.Statusbit = true;

                bool bFreePass = false;
                clsDiscount c = new clsDiscount();
                c.sqlConn = sqlConn;
                c.lblMsg = lblMsg;
                c.intDiscount_PKey = intCodePKey;
                c.LoadDiscount();
                double dblAmount = c.dblDiscountAmt;
                // get discount type' 
                if (c.intDiscountType_pKey == clsDiscount.TYPE_FreeDinner | c.intDiscountType_pKey == clsDiscount.TYPE_FreeDinnerSpeakerOnly | c.intDiscountType_pKey == clsDiscount.TYPE_FreeDinnerSpeakerAndGuest)
                {
                    obj.intDiscountPkey = c.intDiscount_PKey;
                    obj.strDiscountCode = strCode;
                    obj.intDiscountTypePkey = c.intDiscountType_pKey;
                    // Me.imgApplyDiscount.ImageUrl = "~/Images/Icons/greencheck.png"
                    // Me.imgApplyDiscount.Visible = True           
                    double dblDiscountAmount = 0;
                    if ( obj.intDiscountPkey > 0)
                    {
                        // c.dblAmount = Me.cEvent.dblDinnerGuestCharge
                        if (obj.intDiscountTypePkey == clsDiscount.TYPE_FreeDinner)
                            dblDiscountAmount = dblDiscountAmount + Math.Abs(cEvent.dblDinnerCharge);
                        if (obj.intDiscountTypePkey == clsDiscount.TYPE_FreeDinnerSpeakerOnly)
                            dblDiscountAmount = dblDiscountAmount + Math.Abs(cEvent.dblDinnerCharge);
                        if (obj.intDiscountTypePkey == clsDiscount.TYPE_FreeDinnerSpeakerAndGuest)
                            dblDiscountAmount = dblDiscountAmount + Math.Abs(cEvent.dblDinnerCharge);
                    }
                    // --add guests
                    int intGuests = Convert.ToInt32(intGuest);
                    if (intGuests > 0)
                    {
                        double dblAmt = 0;
                        for (int i = 1; i <= intGuests; i++)
                            dblAmt = dblAmt + cEvent.dblDinnerGuestCharge;
                        if (obj.intDiscountTypePkey == clsDiscount.TYPE_FreeDinnerSpeakerAndGuest)
                        {
                            if (intGuests >= 3)
                                dblDiscountAmount = dblDiscountAmount + Math.Abs(dblAmt);
                        }
                    }
                    //this.lblDiscountError.Text = "Discount Amount: $" + dblDiscountAmount.ToString() + "";
                    //this.lblDiscountError.ForeColor = Color.Green;

                    obj.Status = "Discount Amount: $" + dblDiscountAmount.ToString() + "";
                    obj.Result = 0;
                    obj.Statusbit = true;
                    obj.dblDiscountAmount = dblDiscountAmount.ToString();
                    return Ok(obj);

                }
                else
                {
                    obj.intDiscountPkey = 0;
                    obj.intDiscountTypePkey = 0;
                    // Me.imgApplyDiscount.ImageUrl = "~/Images/Icons/remove-icon.png"
                    // Me.imgApplyDiscount.Visible = True
                    //this.lblDiscountError.Text = "Invalid discount code";
                    //return;
                    obj.Status = "Invalid discount code";
                    obj.Result = 1;
                    obj.Statusbit = false;
                    //obj.dblDiscountAmount = dblDiscountAmount.ToString();
                    return Ok(obj);
                }
                c = null/* TODO Change to default(_) if this is not a reference type */;

            }
            catch (Exception ex)
            {
                obj.Result = 4;
                obj.Statusbit = false;
                obj.Status = ex.Message.ToString();
                return Ok(obj);
            }
        }


        }
}
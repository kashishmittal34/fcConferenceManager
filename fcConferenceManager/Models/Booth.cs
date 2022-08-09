using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace MAGI_API.Models
{
    public class MyBooth
    {
        #region Entities

        public class AttendeeLog
        {
            public int pkey { get; set; }
            public int Log_Pkey { get; set; }
            public int Account_pkey { get; set; }
            public int ChatId { get; set; }
            public string ContactName { get; set; }
            public string Title { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Organization { get; set; }
            public string Attendee_Log { get; set; }
            public int OrgTypePkey { get; set; }
            public DateTime Ttime { get; set; }
            public int Organization_pkey { get; set; }
            public DateTime InTime { get; set; }
            public int BoothID { get; set; }
            public DateTime LastLogin { get; set; }
            public string ChatColor { get; set; }
            public int rn { get; set; }
            public string UserStatus { get; set; }
            public string Comments { get; set; }
            public int Comment_Pkey { get; set; }
        }

        public class Lookup
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        public class DocVideo
        {
            public int EventOrganizations_pkey { get; set; }
            public string BoothTitle { get; set; }
            public string BoothDescription { get; set; }
            public string DocumentLink { get; set; }
            public string ChatLink { get; set; }
            public string VideoLink { get; set; }
            public string YouYoutubeChannel { get; set; }
            public int pKey { get; set; }
            public bool Active { get; set; }
            public string ORGDocumentLink { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int FileType { get; set; }
            public int Reorder { get; set; }
            public int pkey { get; set; }
            public string BoothFileName { get; set; }
            public int Type { get; set; }
            public bool IsActive { get; set; }
            public string ORGDocumentLinks { get; set; }
            public string DocumentLinks { get; set; }
            public int Reorders { get; set; }
        }

        public class Event
        {
            public int pKey { get; set; }
            public string Title { get; set; }
            public DateTime DiscussionStart { get; set; }
            public DateTime DiscussionEnd { get; set; }
            public string Description { get; set; }
            public string ShowDescription { get; set; }
            public string Status { get; set; }
            public string HostName { get; set; }
            public string DiscussionType { get; set; }
            public string ScheduleType { get; set; }
            public DateTime DiscussionDate { get; set; }
            public int ScheduleHour { get; set; }
            public int ScheduleMin { get; set; }
            public int Duration { get; set; }
            public string AMPM { get; set; }
        }

        public class StaffSchedule
        {
            public string BoothStartDay { get; set; }
            public string BoothStart_Date { get; set; }
            public DateTime BoothStartDate { get; set; }
            public DateTime BoothEndDate { get; set; }
            public int pkey { get; set; }
            public string Contactname { get; set; }
            public int Hour { get; set; }
            public DateTime StartDate { get; set; }
            public int MeetingMin { get; set; }
            public int Duration { get; set; }
            public string AMPM { get; set; }
            public DateTime DateTime { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime Closetime { get; set; }
        }

        public class Staff
        {
            public int EventOrganization_pkey { get; set; }
            public int Account_pKey { get; set; }
            public string Contactname { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Title { get; set; }
            public string ParticipationRoles { get; set; }
            public string ParticipationRolesPkey { get; set; }
            public string ChatId { get; set; }
            public int LoginTime { get; set; }
            public string ChatColor { get; set; }
            public int? Booth_pkey { get; set; }
        }

        public class StaffingSchedules
        {
            #region Contructors
            public StaffingSchedules()
            {
                staffSchedules = new List<StaffSchedule>();
                staffs = new List<Staff>();
            }
            #endregion

            public List<StaffSchedule> staffSchedules { get; set; }
            public List<Staff> staffs { get; set; }
        }

        public class ExhibtorPersonnel
        {
            public int EventOrganization_pkey { get; set; }
            public int Account_pKey { get; set; }
            public string Contactname { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Title { get; set; }
            public string ParticipationRoles { get; set; }
            public string ParticipationRolesPkey { get; set; }
            public bool ISChecked { get; set; }
            public string ChatId { get; set; }
            public int LoginTime { get; set; }
            public string ChatColor { get; set; }
        }

        public class SaveEvent
        {
            public DateTime StartSchedule { get; set; }
            public int ScheduleType { get; set; }
            public string ScheduleTitle { get; set; }
            public int RTDDuration { get; set; }
            public string StartRTD { get; set; }
            public string EndRTD { get; set; }
            public string RTDAmPm { get; set; }
        }

        #endregion

        #region Methods
        public async Task<dynamic> GetBoothLookups()
        {
            try
            {
                DataSet ds = await SqlHelper.ExecuteSetAsync("sp_getBoothLookUps",5,
                                                CommandType.StoredProcedure, null);

                var lookups = new {
                    ddfileDocType = new List<Lookup>(),
                    ddGames = new List<Lookup>(),
                    ddResponse = new List<Lookup>(),
                    ddActivityType = new List<Lookup>(),
                    ddOrgType = new List<Lookup>()                    
                };

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    lookups.ddfileDocType.Add(
                        new Lookup { Text = dr["strText"].ToString(), Value = dr["pkey"].ToString() });
                }

                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    lookups.ddGames.Add(
                        new Lookup { Text = dr["strText"].ToString(), Value = dr["pkey"].ToString() });
                }

                foreach (DataRow dr in ds.Tables[2].Rows)
                {
                    lookups.ddResponse.Add(
                        new Lookup { Text = dr["strText"].ToString(), Value = dr["pkey"].ToString() });
                }

                foreach (DataRow dr in ds.Tables[3].Rows)
                {
                    lookups.ddActivityType.Add(
                        new Lookup { Text = dr["strText"].ToString(), Value = dr["pkey"].ToString() });
                }

                foreach (DataRow dr in ds.Tables[4].Rows)
                {
                    lookups.ddOrgType.Add(
                        new Lookup { Text = dr["strText"].ToString(), Value = dr["pkey"].ToString() });
                }

                return lookups;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<AttendeeLog>> SelectAttendeeLog(string eventKey,string logType,string OrgType,string accPkey)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@EventOrgPkey",eventKey),
                    new SqlParameter("@LogType",logType),
                    new SqlParameter("@OrgType",OrgType),
                    new SqlParameter("@AccountPkey",accPkey)
                };

                List<AttendeeLog> attendeeLogs = new List<AttendeeLog>();

                attendeeLogs = await SqlHelper.ExecuteListAsync<AttendeeLog>
                                                ("BoothAttendee_Log",
                                                CommandType.StoredProcedure, parameters);
                return attendeeLogs;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Event>> SelectEvents(string EventPKey, string OrganizationPKey, string AccountPKey)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@paramEventPKey", EventPKey),
                    new SqlParameter("@paramOrganizationPKey", OrganizationPKey),
                    new SqlParameter("@paramAccountPKey", AccountPKey)
                };

                List<Event> events = new List<Event>();

                events = await SqlHelper.ExecuteListAsync<Event>
                                                ("sp_SelectEvents",
                                                CommandType.StoredProcedure, parameters);
                return events;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<DocVideo>> SelectDocVideos(string OrganizationPKey)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@paramOrganizationPKey", OrganizationPKey)
                };

                List<DocVideo> docVideos = new List<DocVideo>();

                docVideos = await SqlHelper.ExecuteListAsync<DocVideo>
                                                ("sp_SelectDocVideos",
                                                CommandType.StoredProcedure, parameters);
                return docVideos;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<StaffingSchedules> selectStaffingSchedules(string EventOrganizationPKey,string ActiveEventPkey)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@paramEventOrganizationPKey", EventOrganizationPKey),
                    new SqlParameter("@paramActiveEventPkey", ActiveEventPkey)
                };

                List<StaffSchedule> staffSchedules = new List<StaffSchedule>();
                List<Staff> staffs = new List<Staff>();
                StaffingSchedules staffingSchedules = new StaffingSchedules();
                DataSet ds = await SqlHelper.ExecuteSetAsync("sp_staffingSchedule", 2,
                                                CommandType.StoredProcedure, parameters);

                if(ds.Tables.Count == 2)
                {
                    foreach (var row in ds.Tables[0].AsEnumerable())
                    {
                        StaffSchedule obj = new StaffSchedule();
                        foreach (var prop in obj.GetType().GetProperties())
                        {
                            try
                            {
                                PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
                                propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);
                            }
                            catch
                            {
                                continue;
                            }
                        }
                        staffSchedules.Add(obj);
                    }

                    foreach (var row in ds.Tables[1].AsEnumerable())
                    {
                        Staff obj = new Staff();
                        foreach (var prop in obj.GetType().GetProperties())
                        {
                            try
                            {
                                PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
                                propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);
                            }
                            catch
                            {
                                continue;
                            }
                        }
                        staffs.Add(obj);
                    }
                }

                staffingSchedules.staffSchedules = staffSchedules;
                staffingSchedules.staffs = staffs;

                return staffingSchedules;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<ExhibtorPersonnel>> selectExhibtorPersonnels(string EventOrganizationPKey, string ActiveEventPkey)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@paramEventOrganizationPKey", EventOrganizationPKey),
                    new SqlParameter("@paramActiveEventPkey", ActiveEventPkey)
                };

                List<ExhibtorPersonnel> exhibtorPersonnels = new List<ExhibtorPersonnel>();

                exhibtorPersonnels = await SqlHelper.ExecuteListAsync<ExhibtorPersonnel>
                                                ("sp_ExhibtorPersonnel",
                                                CommandType.StoredProcedure, parameters);
                return exhibtorPersonnels;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SubmitEvent(SaveEvent saveEvent,int intRTDSchedule_Pkey, int intEventOrganizationPKey, int intDuscussionType)
        {
            try
            {
                if (saveEvent.StartSchedule == null)
                {
                    throw new Exception("Select start date");
                }

                if (saveEvent.ScheduleType <= 0)
                {
                    throw new Exception("Select any schedule type");
                }

                if (string.IsNullOrEmpty(saveEvent.ScheduleTitle.Trim()))
                {
                    throw new Exception("Enter title");
                }

                if (saveEvent.RTDDuration <= 0)
                {
                    throw new Exception("Enter time duration.");
                }

                DateTime dtTargetDateStart;
                DateTime dtTargetDateEnd;
                string strStartDate = string.Format("{0:d}", saveEvent.StartSchedule) + " " + string.Format("{0:t}", saveEvent.StartRTD + ":" + saveEvent.EndRTD + "" + saveEvent.RTDAmPm);

                dtTargetDateStart = Convert.ToDateTime(strStartDate);
                DateTime newDate = new DateTime();
                newDate = dtTargetDateStart;
                newDate = newDate.AddMinutes(Convert.ToDouble(saveEvent.RTDDuration));

                dtTargetDateEnd = (DateTime)newDate;

                string AlertMessage = "";
                string qry = "";
                if (intRTDSchedule_Pkey == 0)
                {
                    qry = "INSERT INTO RoundTableSchedule (Active,Title,DiscussionStart,DiscussionEnd         ,Event_pKey,Account_pKey,DiscussionType,ScheduleType,Exhibitor_pKey,WebinarLink,Description)";
                    qry = qry + Constants.vbCrLf + "Values(@Active,@Title,@DiscussionStart,@DiscussionEnd,@Event_pKey,@Account_pKey," + intDuscussionType.ToString();
                    qry = qry + Constants.vbCrLf + ",@ScheduleType," + intEventOrganizationPKey.ToString() + ",@WebinarLink,@Description)";
                    AlertMessage = "Event created";
                }
                else
                {
                    qry = "Update RoundTableSchedule SET Active=@Active, Title=@Title, DiscussionStart=@DiscussionStart, DiscussionEnd=@DiscussionEnd,WebinarLink=@WebinarLink,Description=@Description";
                    qry = qry + Constants.vbCrLf + ",Event_pKey=@Event_pKey,Account_pKey=@Account_pKey,DiscussionType=" + intDuscussionType.ToString();
                    qry = qry + Constants.vbCrLf + ",ScheduleType=@ScheduleType,Exhibitor_pKey=" + intEventOrganizationPKey.ToString();
                    qry = qry + Constants.vbCrLf + "where pkey=" + intRTDSchedule_Pkey.ToString();
                    AlertMessage = "Event updated";
                }

                //HR
                //SqlCommand cmd = new SqlCommand(qry);
                //cmd.Parameters.AddWithValue("@Title", saveEvent.ScheduleTitle.Trim());
                //cmd.Parameters.AddWithValue("@Description", saveEvent.RTDdescription.Trim());
                //cmd.Parameters.AddWithValue("@Event_pKey", intEventPKey.ToString());
                //cmd.Parameters.AddWithValue("@DiscussionStart", dtTargetDateStart);
                //cmd.Parameters.AddWithValue("@DiscussionEnd", dtTargetDateEnd);
                //cmd.Parameters.AddWithValue("@Account_pKey", cAccount.intAccount_PKey);
                //cmd.Parameters.AddWithValue("@Active", myVS.bActive);
                //cmd.Parameters.AddWithValue("@ScheduleType", this.ddScheduleType.SelectedValue.ToString);
                //cmd.Parameters.AddWithValue("@WebinarLink", IIf(ddScheduleType.SelectedValue == 2, this.txtlink.Text.Trim, DBNull.Value));
                //if (!clsUtility.ExecuteQuery(cmd, this.lblMsg, "Add RTD Schedule"))
                //    return;

                //if (intRTDSchedule_Pkey == 0)
                //{
                //    if (ddScheduleType.SelectedValue == 1 && myVS.strRTDTitle == "")
                //        clsChatwee.CreateGroup(txtScheduleTitle.Text.Trim);
                //}
                //else if (ddScheduleType.SelectedValue == 1 && intRTDSchedule_Pkey > 0 && myVS.strRTDTitle != txtScheduleTitle.Text.Trim && myVS.intScheduleType == 1)
                //    clsChatwee.CreateGroup(txtScheduleTitle.Text.Trim);
                //else if (ddScheduleType.SelectedValue != myVS.intScheduleType && intRTDSchedule_Pkey > 0)
                //    clsChatwee.CreateGroup(txtScheduleTitle.Text.Trim);
                //else
                //{
                //}


                //myVS.strRTDTitle = "";
                //myVS.intScheduleType = 0;
                //ViewState(MY_VSTATE) = myVS;

                //clsUtility.CloseRadWindow(ScriptManager.GetCurrent(this.Page), this.Page, this.rwCreateRTD);
                //clsUtility.InjectAlert(ScriptManager.GetCurrent(this.Page), this.Page, AlertMessage.ToString());
                //RefreshRTDSchedule();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
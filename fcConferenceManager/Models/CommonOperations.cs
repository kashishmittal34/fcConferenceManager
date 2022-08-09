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
    public class CommonOperations
    {
        private DataTable FetchWithProcedure(string qry)
        {
            DataTable data = null;
            try
            {
                if (!string.IsNullOrEmpty(qry))
                    data = SqlHelper.ExecuteTable(qry, CommandType.StoredProcedure, null);
            }
            catch
            {
            }
            return data;
        }
        public DataTable FetchGlobalFilters(int ListType)  // Filters List Without Parameters
        {
            DataTable data = null;
            try
            {
                StringBuilder qry = new StringBuilder();
                switch (ListType)
                {
                    case 1: //All Assignment Statuses
                        qry.Append(" SELECT pKey, AssignmentStatusID as strText ,active,OnSpeakerContact FROM Sys_AssignmentStatuses Order by SortOrder");
                        break;
                    case 2: //All Participation Status 
                        qry.Append("SELECT pKey, ParticipationStatusID as strText FROM Sys_ParticipationStatuses Order by strText");
                        break;
                    case 3: //All Organization Types
                        qry.Append(" SELECT pKey, OrganizationTypeID as strText FROM Sys_OrganizationTypes  Order by SortOrder");
                        break;
                    case 4: //All Account Speaker Statuses
                        qry.Append(" SELECT pKey, StatusId as strText FROM sys_AccountSpeakerStatuses Order by pkey");
                        break;
                    case 5: //All Events Started OR Completed
                        qry.Append(" select EventID AS pKey, EventID as strText FROM  Event_List  Where  StartDate <=(select CAST(GETDATE() as DATE)) Order By strText DESC");
                        break;
                    case 6: //All Events About To Start 
                        qry.Append(" select EventID AS pKey, EventID as strText FROM  Event_List  Where  StartDate >=(select CAST(GETDATE() as DATE)) Order By strText DESC");
                        break;
                    case 7: //All Mailing List
                        qry.Append("SELECT pKey, MailingListID as strText FROM Mailing_Lists Order by strText");
                        break;
                    case 8://All Account Speaker Status
                        qry.Append("SELECT pKey, StatusId as strText ,pkey as shortOrder FROM sys_AccountSpeakerStatuses Union Select -1 as pkey ,'Possible' as strText ,2 as shortOrder Union Select -2 as pkey ,'Cancelled' as strText ,5 as shortOrder Order by shortOrder");
                        break;
                    case 9://All Speaker Contact Followup  -- -rdMagiContact
                        qry.Append("Select  t1.pkey	,CASE WHEN t1.Account_pkey=-1 THEN 'To Be Determined' WHEN t1.Account_pkey=-2 THEN 'Do Not Contact' ELSE ISNULL(t2.Firstname,'')+' '+ ISNULL(t2.LastName,'') END as strText , 0  AS C from FollowupRights t1 LEFT JOIN Account_List t2 ON t1.Account_pkey=t2.pKey where t1.Account_pkey<>0  ORDER BY C,strText ASC ");
                        break;
                    case 10://All Email List status
                        qry.Append("SELECT pKey, MessageId as strText ,ISNULL(IsHeader,0)as IsHeader FROM EmailList_status  WHERE pkey !=-1 Order by Orderby");
                        break;
                    case 11://All Salutations
                        qry.Append("select t1.pKey, t1.SalutationID as strText from sys_salutations t1 where pkey<>3 and ISNULL(t1.SalutationID,'')<>'' order by strText");
                        break;
                    case 12://All Call Outcome
                        qry.Append("SELECT pKey, ISNULL(CallOutcomeID_futher,CallOutcomeID) as strText ,case when pkey=15 then 1 when pkey=8 then -1 else 0 end sort , ShortOrder FROM SYS_CallOutcomes Where ISNULL(Active,0)=1 ORDER by ShortOrder ");
                        break;
                    case 13://All Organization Types
                        qry.Append("SELECT pKey, SiteOrganizationID as strText FROM Sys_SiteOrgType  Order by pKey");
                        break;
                    case 14://All Producer Report
                        qry.Append("select t1.pKey, t1.ReportID as strText from Producer_Report t1");
                        break;
                    case 15://All States
                        qry.Append("select t1.pKey, t1.StateID as strText from Sys_States t1 order by strText");
                        break;
                    case 16://All Priorities
                        qry.Append("Select t1.pkey, t1.PriorityID As strText FROM SYS_Priorities t1");
                        break;
                    case 17://All Possible statuses
                        qry.Append("Select t1.pkey, t1.PriorityID As strText FROM Possible_statuses t1");
                        break;
                    case 18://All Phone Types - ddphoneType
                        qry.Append("select t1.pKey, t1.PhoneTypeID as strText from sys_PhoneTypes t1 where t1.pkey >0 order by strText");
                        break;
                    case 19://All Countries
                        qry.Append("select t1.pKey, t1.CountryID as strText from sys_Countries t1 order by isNull(t1.sortorder,999), strText");
                        break;
                    case 20://All Suffixes
                        qry.Append("Select t1.pkey, t1.SuffixID As strText FROM sys_suffixes t1");
                        break;
                    case 21://All TimeZone
                        qry.Append("SELECT tz.pkey,(tz.CountryCode + ' (' + tz.TimeZone+')' + ' UTC offset: Std ' +Convert(varchar,tz.UTCOffset)+', Dst '+  Convert(varchar,tz.UTCDSToffset)) As strText");
                        qry.Append(Environment.NewLine + " from SYS_CountryTimeZone tz inner join sys_Countries c on tz.CountryCode=c.countrycode Where tz.active=1 and  tz.IsInternational = 0");
                        break;
                    case 22://All Tracks
                        qry.Append("select t1.pKey, (Case when t1.Prefix = 'P' Then t1.TracKID else 'Track ' + t1.Prefix end) as strText");
                        qry.Append(Environment.NewLine + " from Sys_tracks t1 where t1.Prefix in ('A','B','C','D','E','F','W') order by t1.SortOrder, strText");
                        break;
                    case 23://All Countries Not Including 1,2,3 Key
                        qry.Append("Select  Distinct t1.pKey, t1.CountryID As strText From sys_Countries t1 Inner Join account_List t2 on t2.country_pkey = t1.pKey where  t1.pkey not in(1,2,3)  ORDEr by  t1.CountryID asc");
                        break;
                    case 24://All Final Dispositions
                        qry.Append("SELECT pKey, FinalDispositionID as strText FROM Sys_FinalDispositions Order by strText");
                        break;
                    case 26: //All Time Zone to Call
                        return FetchWithProcedure("getCountryTimezone_MVC");
                    case 27://All Speaker Flag
                        return FetchWithProcedure("getSpeakerFlags_MVC");
                    case 28: //ddCallNext
                        qry.Append("SELECT pKey, CallNextActionID as strText FROM Sys_CallNextActions where ISNULL(AbbrevCode,'')<>'INTL' AND ISNULL(IsActive,0)=1 Order by SortOrder");
                        break;
                    case 29:
                        qry.Append("Select  pKey ,	MethodName as strText  FROM sys_Method ORDER By Pkey");
                        break;
                    case 30:
                        qry.Append("WITH cte_menu (    pkey,spkrflagid,   sortorder,  IsHeader,  headervalue,     Level) AS (  ");
                        qry.Append("Select  convert(varchar ,t1.pKey)	,t1.SpkrFlagID	, t1.SortOrder	, ISNULL(t1.IsHeader,0) as IsHeader, ISNULL(t1.	HeaderValue,0) as HeaderValue  ,ISNULL(t1.HeaderValue,0)+1 as Level from sys_spkrflags t1 WHERE ISNULL( t1.headerValue,0)=0 AND ISNULL(t1.IsHeader,0)<>1");
                        qry.Append(" UNION ALL ");
                        qry.Append("Select Convert(varchar ,pKey) as Pkey,SpkrFlagID,	SortOrder,IsHeader	,pKey as HeaderValue ,pKey as Level from sys_spkrflags Where IsHeader=1");
                        qry.Append("UNION ALL Select  t1.pKey,t1.SpkrFlagID	, t1.SortOrder	, 0 as IsHeader, t1.HeaderValue ,t1.HeaderValue+1 as Level from sys_spkrflags t1 INNER JOIN  sys_spkrflags t2 ON t1.headerValue=t2.pkey");
                        qry.Append(") SELECT * FROM cte_menu ORDER BY Level,HeaderValue,SortOrder");
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
        public DataTable FetchGlobalFiltersByEvent(int Event_pKey, int ListType)
        {
            DataTable data = null;
            try
            {
                StringBuilder qry = new StringBuilder();
                switch (ListType)
                {
                    case 1: // List Of Events Already Started or completed With Selected Current Event
                        qry.Append(" select pKey AS pKey, EventID as strText FROM Event_List Where pkey=@Event_pKey OR StartDate <=(select CAST(GETDATE() as DATE)) Order By strText DESC");
                        break;
                    case 2: // List Of Events About to Start With Selected Current Event
                        qry.Append(" select pKey AS pKey, EventID as strText FROM Event_List Where pkey=@Event_pKey OR StartDate >=(select CAST(GETDATE() as DATE)) Order By strText DESC");
                        break;
                    case 3: //All Track ID
                        qry.Append("select distinct t1.pKey, t2.track_prefix+t1.SessionID as strText From Session_List t1 inner join Event_sessions t2 on t2.Session_pKey = t1.pKey   inner join  Sys_Tracks t3 on t3.pKey=t1.Track_pKey");
                        qry.Append(Environment.NewLine +" Where isNull(t1.SessionStatus_pKey,1) =" + clsSession.STATUS_Active.ToString() + " And isNull(t3.Educational,0) = 1 AND t2.Event_pkey= @Event_pKey Order by strText");
                        break;
                    case 4: //All Interested Event
                        qry.Append("Select pKey As pKey, EventID As strText ,0 as Orders  FROM  Event_List where pkey = @Event_pKey ");
                        qry.Append(Environment.NewLine + "UNION ALL Select pKey As pKey, EventID As strText,1 as Orders FROM Event_List where pkey != @Event_pKey ORDER BY Orders,strText DESC");
                        break;
                    case 5: //All Scheduled Educational Tracks By Event
                        qry.Append("Select DISTINCT t3.pKey,'('+  t3.Prefix + ') ' +  t3.TrackID  as strText  From Event_Sessions t1  Inner Join Session_List t2 on t2.pkey = t1.Session_pKey  Inner Join sys_tracks t3 on t3.pkey = t1.track_pkey  Where  t3.educational = 1 AND t1.IsScheduled = 1 AND  t1.Event_pKey = @Event_pKey Order by strText  ");
                        break;
                    case 6: //All Scopes by Event
                        qry.Append("SELECT distinct t1.pKey, t1.Track_Prefix+t2.SessionID as strText FROM Event_Sessions t1 inner join session_list t2 on t2.pkey = t1.session_pkey Where t1.event_pkey = @Event_pKey Order by strText");
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

       




    }
}
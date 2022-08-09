using fcConferenceManager.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MAGI_API.Models
{
    public static class EventPlanning
    {
        public static DataTable GetDashboardDataTable(SqlConnection sqlConn,string EventName,DateTime? startDate,DateTime? enddate,string EventType,string Brand,string Audience,string Unit,string Location,int Id,int range)
        {
            if (sqlConn.State == ConnectionState.Closed)
            {
                sqlConn.Open();
            }
            DataTable dtdata = new DataTable();
            dtdata.Clear();

            SqlCommand cmd = new SqlCommand("usp_GetDashDataByNameAndType", sqlConn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Id", Id);
            cmd.Parameters.AddWithValue("@EventName", EventName);
            cmd.Parameters.AddWithValue("@EventType", EventType);
            cmd.Parameters.AddWithValue("@Brand", Brand);
            cmd.Parameters.AddWithValue("@Audience", Audience);
            cmd.Parameters.AddWithValue("@Unit", Unit);
            cmd.Parameters.AddWithValue("@Location", Location);
            cmd.Parameters.AddWithValue("@range", range);

            if (startDate != null)
                cmd.Parameters.AddWithValue("@startDate", startDate);

            if (enddate != null)
                cmd.Parameters.AddWithValue("@endDate", enddate);

            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;
            da.Fill(dtdata);
            return dtdata;

        }

        public static DataTable GetDataDataTable(SqlConnection sqlConn, string EventName, DateTime? startDate, DateTime? enddate, string EventType, string Brand, string Audience, string Unit, string Location, int Id,int range)
        {
            if (sqlConn.State == ConnectionState.Closed)
            {
                sqlConn.Open();
            }
            DataTable dtdash = new DataTable();
            dtdash.Clear();
            SqlCommand cmd = new SqlCommand("usp_getdata", sqlConn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@EventName", EventName.Replace("'","''"));
            cmd.Parameters.AddWithValue("@EventType", EventType);
            cmd.Parameters.AddWithValue("@Brand", Brand);
            cmd.Parameters.AddWithValue("@Audience", Audience);
            cmd.Parameters.AddWithValue("@Unit", Unit);
            cmd.Parameters.AddWithValue("@Location", Location);
            cmd.Parameters.AddWithValue("@Id",Id);
            cmd.Parameters.AddWithValue("@range", range);

            if (startDate != null)
                cmd.Parameters.AddWithValue("@startDate", startDate);

            if (enddate != null)
                cmd.Parameters.AddWithValue("@endDate", enddate);

            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;
            da.Fill(dtdash);
            return dtdash;

        }

        public static DataTable GetCalendra(SqlConnection sqlConn)
        {
            if (sqlConn.State == ConnectionState.Closed)
            {
                sqlConn.Open();
            }
            DataTable dtcalendra = new DataTable();
            dtcalendra.Clear();
            SqlCommand cmd = new SqlCommand("usp_getcalendraschedulardata", sqlConn);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;
            da.Fill(dtcalendra);
            return dtcalendra;

        }
        public static DataTable GetCalendraGridViewDataTable(SqlConnection sqlConn)
        {
            if (sqlConn.State == ConnectionState.Closed)
            {
                sqlConn.Open();
            }
            DataTable dtcalendra = new DataTable();
            dtcalendra.Clear();
            SqlCommand cmd = new SqlCommand("usp_calendradata", sqlConn);

            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;
            da.Fill(dtcalendra);
            return dtcalendra;

        }

        public static DataTable GetChronologyGridViewDataTable(SqlConnection sqlConn, string EventName, DateTime? startDate,DateTime? enddate, string EventType, string Brand, string Unit, string Location, int range)
        {
            if (sqlConn.State == ConnectionState.Closed)
            {
                sqlConn.Open();
            }
            DataTable dtChronologydata = new DataTable();
            dtChronologydata.Clear();

            SqlCommand cmd = new SqlCommand("usp_getChronology", sqlConn);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter();
            cmd.Parameters.AddWithValue("@EventName", EventName);
            cmd.Parameters.AddWithValue("@EventType", EventType);
            cmd.Parameters.AddWithValue("@Brand", Brand);
            cmd.Parameters.AddWithValue("@Unit", Unit);
            cmd.Parameters.AddWithValue("@Location", Location);
            cmd.Parameters.AddWithValue("@range", range);

            if (startDate != null)
                cmd.Parameters.AddWithValue("@startDate", startDate);

            if (enddate != null)
                cmd.Parameters.AddWithValue("@endDate", enddate);

            da.SelectCommand = cmd;
            da.Fill(dtChronologydata);
            return dtChronologydata;
        }


        public static DataTable GetColorEventDataTable(SqlConnection sqlConn)
        {
            if (sqlConn.State == ConnectionState.Closed)
            {
                sqlConn.Open();
            }
            DataTable dtcolor = new DataTable();
            dtcolor.Clear();

            SqlCommand cmd = new SqlCommand("select * from tbl_eventpastelcolor", sqlConn);
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;
            da.Fill(dtcolor);
            return dtcolor;

        }


        public static DataTable GetValuePointWeighting(SqlConnection sqlConn)
        {
            if (sqlConn.State == ConnectionState.Closed)
            {
                sqlConn.Open();
            }
            DataTable dtvaluepoint = new DataTable();
            dtvaluepoint.Clear();
            SqlCommand cmd = new SqlCommand("usp_getValuePointWeighting", sqlConn);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;
            da.Fill(dtvaluepoint);
            return dtvaluepoint;

        }
        public static DataTable GetDataDashData(SqlConnection sqlConn, string Pkey, string Name,int intType)
        {
            if (sqlConn.State == ConnectionState.Closed)
            {
                sqlConn.Open();
            }
            DataTable dtDataDashData = new DataTable();
            dtDataDashData.Clear();
            SqlCommand cmd = new SqlCommand("usp_GetDataDashData", sqlConn);
            cmd.Parameters.AddWithValue("@ID",Pkey);
            cmd.Parameters.AddWithValue("@Name", Name);
            cmd.Parameters.AddWithValue("@Type", intType);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;
            da.Fill(dtDataDashData);
            return dtDataDashData;

        }

        public static DataTable GetAllData(SqlConnection sqlConn, string fromDate =null, string toDate =null)
        {
            if (sqlConn.State == ConnectionState.Closed)
            {
                sqlConn.Open();
            }
            DataTable dtAllData = new DataTable();
            dtAllData.Clear();
            SqlCommand cmd = new SqlCommand("usp_GetAllData", sqlConn);
            cmd.Parameters.AddWithValue("@dtFrom", fromDate);
            cmd.Parameters.AddWithValue("@dtTo", toDate);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;
            da.Fill(dtAllData);
            return dtAllData;
        }
        //this method is used to save excel file data of worksheet 1 in db
        public static string SaveData(SqlConnection sqlConn, DataTable dt)
        {
            string Result = "Success";
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (sqlConn.State == ConnectionState.Closed)
                    {
                        sqlConn.Open();
                    }
                    SqlCommand cmd = new SqlCommand("usp_insertdata", sqlConn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if(dt.Columns.Contains("Id"))
                    {
                        cmd.Parameters.AddWithValue("@Id", string.IsNullOrEmpty(dt.Rows[i]["Id"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Id"].ToString());
                       
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Id", 0);
                    }
                  
                    cmd.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(dt.Rows[i]["Name"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Name"].ToString());
                    cmd.Parameters.AddWithValue("@Type", string.IsNullOrEmpty(dt.Rows[i]["Type"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Type"].ToString());
                    cmd.Parameters.AddWithValue("@Brand", string.IsNullOrEmpty(dt.Rows[i]["Brand"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Brand"].ToString());
                    cmd.Parameters.AddWithValue("@TargetAudience", string.IsNullOrEmpty(dt.Rows[i]["TargetAudience"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["TargetAudience"].ToString());
                    cmd.Parameters.AddWithValue("@Unit", string.IsNullOrEmpty(dt.Rows[i]["Unit"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Unit"].ToString());
                    cmd.Parameters.AddWithValue("@Producer", string.IsNullOrEmpty(dt.Rows[i]["Producer"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Producer"].ToString());
                    cmd.Parameters.AddWithValue("@Location", string.IsNullOrEmpty(dt.Rows[i]["Location"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Location"].ToString());
                    cmd.Parameters.AddWithValue("@Venue", string.IsNullOrEmpty(dt.Rows[i]["Venue"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Venue"].ToString());
                    cmd.Parameters.AddWithValue("@Format", string.IsNullOrEmpty(dt.Rows[i]["Format"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Format"].ToString());
                    cmd.Parameters.AddWithValue("@StartDate", string.IsNullOrEmpty(dt.Rows[i]["StartDate"].ToString()) ? (object)DBNull.Value : DateTime.Parse(dt.Rows[i]["StartDate"].ToString()));
                    cmd.Parameters.AddWithValue("@EndDate", string.IsNullOrEmpty(dt.Rows[i]["EndDate"].ToString()) ? (object)DBNull.Value : DateTime.Parse(dt.Rows[i]["EndDate"].ToString()));
                    cmd.Parameters.AddWithValue("@Plan_Participants", string.IsNullOrEmpty(dt.Rows[i]["Plan_Participants"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Participants"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Participants", string.IsNullOrEmpty(dt.Rows[i]["Actual_Participants"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Participants"].ToString());
                    cmd.Parameters.AddWithValue("@Pricing", string.IsNullOrEmpty(dt.Rows[i]["Pricing"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Pricing"].ToString());
                    cmd.Parameters.AddWithValue("@Plan_Revenue", string.IsNullOrEmpty(dt.Rows[i]["Plan_Revenue"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Revenue"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Revenue", string.IsNullOrEmpty(dt.Rows[i]["Actual_Revenue"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Revenue"].ToString());
                    cmd.Parameters.AddWithValue("@Plan_Cost", string.IsNullOrEmpty(dt.Rows[i]["Plan_Cost"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Cost"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Cost", string.IsNullOrEmpty(dt.Rows[i]["Actual_Cost"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Cost"].ToString());
                    cmd.Parameters.AddWithValue("@Plan_Cost_PerPoint", string.IsNullOrEmpty(dt.Rows[i]["Plan_Cost_PerPoint"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Cost_PerPoint"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Cost_PerPoint", string.IsNullOrEmpty(dt.Rows[i]["Actual_Cost_PerPoint"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Cost_PerPoint"].ToString());
                    cmd.Parameters.AddWithValue("@Plan_Profit", string.IsNullOrEmpty(dt.Rows[i]["Plan_Profit"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Profit"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Profit", string.IsNullOrEmpty(dt.Rows[i]["Actual_Profit"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Profit"].ToString());
                    cmd.Parameters.AddWithValue("@Plan_Total", string.IsNullOrEmpty(dt.Rows[i]["Plan_Total"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Total"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Total", string.IsNullOrEmpty(dt.Rows[i]["Actual_Total"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Total"].ToString());
                    cmd.Parameters.AddWithValue("@Plan_Follow_Up", string.IsNullOrEmpty(dt.Rows[i]["Plan_Follow_Up"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Follow_Up"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Follow_Up", string.IsNullOrEmpty(dt.Rows[i]["Actual_Follow_Up"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Follow_Up"].ToString());
                    cmd.Parameters.AddWithValue("@Plan_Engagement", string.IsNullOrEmpty(dt.Rows[i]["Plan_Engagement"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Engagement"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Engagement", string.IsNullOrEmpty(dt.Rows[i]["Actual_Engagement"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Engagement"].ToString());
                    cmd.Parameters.AddWithValue("@Plan_Affinity", string.IsNullOrEmpty(dt.Rows[i]["Plan_Affinity"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Affinity"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Affinity", string.IsNullOrEmpty(dt.Rows[i]["Actual_Affinity"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Affinity"].ToString());
                    cmd.Parameters.AddWithValue("@Plan_Visibility", string.IsNullOrEmpty(dt.Rows[i]["Plan_Visibility"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Visibility"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Visibility", string.IsNullOrEmpty(dt.Rows[i]["Actual_Visibility"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Visibility"].ToString());
                    cmd.Parameters.AddWithValue("@Comments", string.IsNullOrEmpty(dt.Rows[i]["Comments"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Comments"].ToString());
                    cmd.Parameters.Add("@identity", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
            }
            catch (Exception ex)
            {
                Result = ex.Message;
            }
            return Result;
        }

        public static int DeleteData(SqlConnection sqlConn,int tabid)
        {
            int result = 1;
            try
            {
                if (sqlConn.State == ConnectionState.Closed)
                {
                    sqlConn.Open();
                }
                SqlCommand cmd = new SqlCommand("usp_deletedata", sqlConn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Tabid", tabid);
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();

            }
            catch (Exception ex)
            {
                result = 0;
            }
            return result;
        }
        public static int DeleteDatabyid(SqlConnection sqlConn, int id)
        {
            int result = 1;
            try
            {
                if (sqlConn.State == ConnectionState.Closed)
                {
                    sqlConn.Open();
                }
                SqlCommand cmd = new SqlCommand("usp_deletedatabyid", sqlConn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();

            }
            catch (Exception ex)
            {
                result = 0;
            }
            return result;
        }

        public static EventPlanResult AddUpdateData(SqlConnection sqlConn, EventPlanData obj)
        {
            EventPlanResult objresult = new EventPlanResult();
            objresult.result = "Success";
            try
            {

                if (sqlConn.State == ConnectionState.Closed)
                {
                    sqlConn.Open();
                }
                //string DATE = DateTime.ParseExact(DateString, "MM/dd/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                SqlCommand cmd = new SqlCommand("usp_insertdata", sqlConn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", obj.Id);
                cmd.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(obj.Name) ? (object)DBNull.Value : obj.Name);
                cmd.Parameters.AddWithValue("@Type", string.IsNullOrEmpty(obj.Type) ? (object)DBNull.Value : obj.Type);
                cmd.Parameters.AddWithValue("@Brand", string.IsNullOrEmpty(obj.Brand) ? (object)DBNull.Value : obj.Brand);
                cmd.Parameters.AddWithValue("@TargetAudience", string.IsNullOrEmpty(obj.TargetAudience) ? (object)DBNull.Value : obj.TargetAudience);
                cmd.Parameters.AddWithValue("@Unit", string.IsNullOrEmpty(obj.Unit) ? (object)DBNull.Value : obj.Unit);
                cmd.Parameters.AddWithValue("@Producer", string.IsNullOrEmpty(obj.Producer) ? (object)DBNull.Value : obj.Producer);
                cmd.Parameters.AddWithValue("@Location", string.IsNullOrEmpty(obj.Location) ? (object)DBNull.Value : obj.Location);
                cmd.Parameters.AddWithValue("@Venue", string.IsNullOrEmpty(obj.Venue) ? (object)DBNull.Value : obj.Venue);
                cmd.Parameters.AddWithValue("@Format", string.IsNullOrEmpty(obj.Format) ? (object)DBNull.Value : obj.Format);
                cmd.Parameters.AddWithValue("@StartDate", string.IsNullOrEmpty(obj.StartDate) ? (object)DBNull.Value : DateTime.Parse(obj.StartDate));
                cmd.Parameters.AddWithValue("@EndDate", string.IsNullOrEmpty(obj.EndDate) ? (object)DBNull.Value : DateTime.Parse(obj.EndDate));
                cmd.Parameters.AddWithValue("@Plan_Participants", string.IsNullOrEmpty(obj.Plan_Participants) ? (object)DBNull.Value : obj.Plan_Participants);
                cmd.Parameters.AddWithValue("@Actual_Participants", string.IsNullOrEmpty(obj.Actual_Participants) ? (object)DBNull.Value : obj.Actual_Participants);
                cmd.Parameters.AddWithValue("@Pricing", string.IsNullOrEmpty(obj.Pricing) ? (object)DBNull.Value : obj.Pricing);
                cmd.Parameters.AddWithValue("@Plan_Revenue", string.IsNullOrEmpty(obj.Plan_Revenue) ? (object)DBNull.Value : obj.Plan_Revenue);
                cmd.Parameters.AddWithValue("@Actual_Revenue", string.IsNullOrEmpty(obj.Actual_Revenue) ? (object)DBNull.Value : obj.Actual_Revenue);
                cmd.Parameters.AddWithValue("@Plan_Cost", string.IsNullOrEmpty(obj.Plan_Cost) ? (object)DBNull.Value : obj.Plan_Cost);
                cmd.Parameters.AddWithValue("@Actual_Cost", string.IsNullOrEmpty(obj.Actual_Cost) ? (object)DBNull.Value : obj.Actual_Cost);
                cmd.Parameters.AddWithValue("@Plan_Cost_PerPoint", string.IsNullOrEmpty(obj.Plan_Cost_PerPoint) ? (object)DBNull.Value : obj.Plan_Cost_PerPoint);
                cmd.Parameters.AddWithValue("@Actual_Cost_PerPoint", string.IsNullOrEmpty(obj.Actual_Cost_PerPoint) ? (object)DBNull.Value : obj.Actual_Cost_PerPoint);
                cmd.Parameters.AddWithValue("@Plan_Profit", string.IsNullOrEmpty(obj.Plan_Profit) ? (object)DBNull.Value : obj.Plan_Profit);
                cmd.Parameters.AddWithValue("@Actual_Profit", string.IsNullOrEmpty(obj.Actual_Profit) ? (object)DBNull.Value : obj.Actual_Profit);
                cmd.Parameters.AddWithValue("@Plan_Total", string.IsNullOrEmpty(obj.Plan_Total) ? (object)DBNull.Value : obj.Plan_Total);
                cmd.Parameters.AddWithValue("@Actual_Total", string.IsNullOrEmpty(obj.Actual_Total) ? (object)DBNull.Value : obj.Actual_Total);
                cmd.Parameters.AddWithValue("@Plan_Follow_Up", string.IsNullOrEmpty(obj.Plan_Follow_Up) ? (object)DBNull.Value : obj.Plan_Follow_Up);
                cmd.Parameters.AddWithValue("@Actual_Follow_Up", string.IsNullOrEmpty(obj.Actual_Follow_Up) ? (object)DBNull.Value : obj.Actual_Follow_Up);
                cmd.Parameters.AddWithValue("@Plan_Engagement", string.IsNullOrEmpty(obj.Plan_Engagement) ? (object)DBNull.Value : obj.Plan_Engagement);
                cmd.Parameters.AddWithValue("@Actual_Engagement", string.IsNullOrEmpty(obj.Actual_Engagement) ? (object)DBNull.Value : obj.Actual_Engagement);
                cmd.Parameters.AddWithValue("@Plan_Affinity", string.IsNullOrEmpty(obj.Plan_Affinity) ? (object)DBNull.Value : obj.Plan_Affinity);
                cmd.Parameters.AddWithValue("@Actual_Affinity", string.IsNullOrEmpty(obj.Actual_Affinity) ? (object)DBNull.Value : obj.Actual_Affinity);
                cmd.Parameters.AddWithValue("@Plan_Visibility", string.IsNullOrEmpty(obj.Plan_Visibility) ? (object)DBNull.Value : obj.Plan_Visibility);
                cmd.Parameters.AddWithValue("@Actual_Visibility", string.IsNullOrEmpty(obj.Actual_Visibility) ? (object)DBNull.Value : obj.Actual_Visibility);
                cmd.Parameters.AddWithValue("@Comments", string.IsNullOrEmpty(obj.Comments) ? (object)DBNull.Value : obj.Comments);
                cmd.Parameters.Add("@identity", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                objresult.id =Convert.ToInt32( cmd.Parameters["@identity"].Value.ToString());
                cmd.Parameters.Clear();
             
            }
            catch (Exception ex)
            {

                objresult.result = ex.Message;
            }
            return objresult;
        }


        //this method is used to save excel file data of worksheet 1 in db
        public static string SaveDashboardData(SqlConnection sqlConn,DataTable dt)
        {
            string Result = "Success";
            try
            {
              
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (sqlConn.State == ConnectionState.Closed)
                    {
                        sqlConn.Open();
                    }
                    //string DATE = DateTime.ParseExact(DateString, "MM/dd/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                    SqlCommand cmd = new SqlCommand("usp_insertdashboardata", sqlConn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (dt.Columns.Contains("Id"))
                    {
                        cmd.Parameters.AddWithValue("@Id", string.IsNullOrEmpty(dt.Rows[i]["Id"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Id"].ToString());

                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Id",0);
                    }
                    cmd.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(dt.Rows[i]["Name"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Name"].ToString());
                    cmd.Parameters.AddWithValue("@Type", string.IsNullOrEmpty(dt.Rows[i]["Type"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Type"].ToString());
                    cmd.Parameters.AddWithValue("@Brand", string.IsNullOrEmpty(dt.Rows[i]["Brand"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Brand"].ToString());
                    cmd.Parameters.AddWithValue("@TargetAudience", string.IsNullOrEmpty(dt.Rows[i]["TargetAudience"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["TargetAudience"].ToString());
                    cmd.Parameters.AddWithValue("@Unit", string.IsNullOrEmpty(dt.Rows[i]["Unit"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Unit"].ToString());
                    cmd.Parameters.AddWithValue("@Producer", string.IsNullOrEmpty(dt.Rows[i]["Producer"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Producer"].ToString());
                    cmd.Parameters.AddWithValue("@Location", string.IsNullOrEmpty(dt.Rows[i]["Location"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Location"].ToString());
                    cmd.Parameters.AddWithValue("@Venue", string.IsNullOrEmpty(dt.Rows[i]["Venue"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Venue"].ToString());
                    cmd.Parameters.AddWithValue("@Format", string.IsNullOrEmpty(dt.Rows[i]["Format"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Format"].ToString());
                    cmd.Parameters.AddWithValue("@StartDate", string.IsNullOrEmpty(dt.Rows[i]["StartDate"].ToString()) ? (object)DBNull.Value : DateTime.Parse(dt.Rows[i]["StartDate"].ToString()));
                    cmd.Parameters.AddWithValue("@EndDate", string.IsNullOrEmpty(dt.Rows[i]["EndDate"].ToString()) ? (object)DBNull.Value : DateTime.Parse(dt.Rows[i]["EndDate"].ToString()));
                    cmd.Parameters.AddWithValue("@Plan_Participants", string.IsNullOrEmpty(dt.Rows[i]["Plan_Participants"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Participants"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Participants", string.IsNullOrEmpty(dt.Rows[i]["Actual_Participants"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Participants"].ToString());
                    cmd.Parameters.AddWithValue("@Pricing", string.IsNullOrEmpty(dt.Rows[i]["Pricing"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Pricing"].ToString());
                    cmd.Parameters.AddWithValue("@Plan_Revenue", string.IsNullOrEmpty(dt.Rows[i]["Plan_Revenue"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Revenue"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Revenue", string.IsNullOrEmpty(dt.Rows[i]["Actual_Revenue"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Revenue"].ToString());
                    cmd.Parameters.AddWithValue("@Plan_Cost", string.IsNullOrEmpty(dt.Rows[i]["Plan_Cost"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Cost"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Cost", string.IsNullOrEmpty(dt.Rows[i]["Actual_Cost"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Cost"].ToString());
                    cmd.Parameters.AddWithValue("@Plan_Cost_PerPoint", string.IsNullOrEmpty(dt.Rows[i]["Plan_Cost_PerPoint"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Cost_PerPoint"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Cost_PerPoint", string.IsNullOrEmpty(dt.Rows[i]["Actual_Cost_PerPoint"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Cost_PerPoint"].ToString());
                    cmd.Parameters.AddWithValue("@Plan_Profit", string.IsNullOrEmpty(dt.Rows[i]["Plan_Profit"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Profit"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Profit", string.IsNullOrEmpty(dt.Rows[i]["Actual_Profit"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Profit"].ToString());
                    cmd.Parameters.AddWithValue("@Plan_Total", string.IsNullOrEmpty(dt.Rows[i]["Plan_Total"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Total"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Total", string.IsNullOrEmpty(dt.Rows[i]["Actual_Total"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Total"].ToString());
                    cmd.Parameters.AddWithValue("@Plan_Follow_Up", string.IsNullOrEmpty(dt.Rows[i]["Plan_Follow_Up"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Follow_Up"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Follow_Up", string.IsNullOrEmpty(dt.Rows[i]["Actual_Follow_Up"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Follow_Up"].ToString());
                    cmd.Parameters.AddWithValue("@Plan_Engagement", string.IsNullOrEmpty(dt.Rows[i]["Plan_Engagement"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Engagement"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Engagement", string.IsNullOrEmpty(dt.Rows[i]["Actual_Engagement"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Engagement"].ToString());
                    cmd.Parameters.AddWithValue("@Plan_Affinity", string.IsNullOrEmpty(dt.Rows[i]["Plan_Affinity"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Affinity"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Affinity", string.IsNullOrEmpty(dt.Rows[i]["Actual_Affinity"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Affinity"].ToString());
                    cmd.Parameters.AddWithValue("@Plan_Visibility", string.IsNullOrEmpty(dt.Rows[i]["Plan_Visibility"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Plan_Visibility"].ToString());
                    cmd.Parameters.AddWithValue("@Actual_Visibility", string.IsNullOrEmpty(dt.Rows[i]["Actual_Visibility"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Actual_Visibility"].ToString());
                    cmd.Parameters.AddWithValue("@Priority", string.IsNullOrEmpty(dt.Rows[i]["Priority"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Priority"].ToString());
                    cmd.Parameters.AddWithValue("@PointsPlanned", string.IsNullOrEmpty(dt.Rows[i]["Points Planned"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Points Planned"].ToString());
                    cmd.Parameters.AddWithValue("@PointsEarned", string.IsNullOrEmpty(dt.Rows[i]["Points Earned"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Points Earned"].ToString());
                    cmd.Parameters.Add("@identity", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();


                }
            }
            catch (Exception ex)
            {
                Result = ex.Message;
            }
            return Result;
        }

        public static EventPlanResult AddUpdateDashboardData(SqlConnection sqlConn, EventPlanData obj)
        {
            EventPlanResult objresult = new EventPlanResult();
            objresult.result = "Success";
            try
            {

                if (sqlConn.State == ConnectionState.Closed)
                {
                    sqlConn.Open();
                }
                //string DATE = DateTime.ParseExact(DateString, "MM/dd/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                SqlCommand cmd = new SqlCommand("usp_insertdashboardata", sqlConn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", obj.Id);
                cmd.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(obj.Name) ? (object)DBNull.Value : obj.Name);
                cmd.Parameters.AddWithValue("@Type", string.IsNullOrEmpty(obj.Type) ? (object)DBNull.Value : obj.Type);
                cmd.Parameters.AddWithValue("@Brand", string.IsNullOrEmpty(obj.Brand) ? (object)DBNull.Value : obj.Brand);
                cmd.Parameters.AddWithValue("@TargetAudience", string.IsNullOrEmpty(obj.TargetAudience) ? (object)DBNull.Value : obj.TargetAudience);
                cmd.Parameters.AddWithValue("@Unit", string.IsNullOrEmpty(obj.Unit) ? (object)DBNull.Value : obj.Unit);
                cmd.Parameters.AddWithValue("@Producer", string.IsNullOrEmpty(obj.Producer) ? (object)DBNull.Value : obj.Producer);
                cmd.Parameters.AddWithValue("@Location", string.IsNullOrEmpty(obj.Location) ? (object)DBNull.Value : obj.Location);
                cmd.Parameters.AddWithValue("@Venue", string.IsNullOrEmpty(obj.Venue) ? (object)DBNull.Value : obj.Venue);
                cmd.Parameters.AddWithValue("@Format", string.IsNullOrEmpty(obj.Format) ? (object)DBNull.Value : obj.Format);
                cmd.Parameters.AddWithValue("@StartDate", string.IsNullOrEmpty(obj.StartDate) ? (object)DBNull.Value : DateTime.Parse(obj.StartDate));
                cmd.Parameters.AddWithValue("@EndDate", string.IsNullOrEmpty(obj.EndDate) ? (object)DBNull.Value : DateTime.Parse(obj.EndDate));
                cmd.Parameters.AddWithValue("@Plan_Participants", string.IsNullOrEmpty(obj.Plan_Participants) ? (object)DBNull.Value : obj.Plan_Participants);
                cmd.Parameters.AddWithValue("@Actual_Participants", string.IsNullOrEmpty(obj.Actual_Participants) ? (object)DBNull.Value : obj.Actual_Participants);
                cmd.Parameters.AddWithValue("@Pricing", string.IsNullOrEmpty(obj.Pricing) ? (object)DBNull.Value : obj.Pricing);
                cmd.Parameters.AddWithValue("@Plan_Revenue", string.IsNullOrEmpty(obj.Plan_Revenue) ? (object)DBNull.Value : obj.Plan_Revenue);
                cmd.Parameters.AddWithValue("@Actual_Revenue", string.IsNullOrEmpty(obj.Actual_Revenue) ? (object)DBNull.Value : obj.Actual_Revenue);
                cmd.Parameters.AddWithValue("@Plan_Cost", string.IsNullOrEmpty(obj.Plan_Cost) ? (object)DBNull.Value : obj.Plan_Cost);
                cmd.Parameters.AddWithValue("@Actual_Cost", string.IsNullOrEmpty(obj.Actual_Cost) ? (object)DBNull.Value : obj.Actual_Cost);
                cmd.Parameters.AddWithValue("@Plan_Cost_PerPoint", string.IsNullOrEmpty(obj.Plan_Cost_PerPoint) ? (object)DBNull.Value : obj.Plan_Cost_PerPoint);
                cmd.Parameters.AddWithValue("@Actual_Cost_PerPoint", string.IsNullOrEmpty(obj.Actual_Cost_PerPoint) ? (object)DBNull.Value : obj.Actual_Cost_PerPoint);
                cmd.Parameters.AddWithValue("@Plan_Profit", string.IsNullOrEmpty(obj.Plan_Profit) ? (object)DBNull.Value : obj.Plan_Profit);
                cmd.Parameters.AddWithValue("@Actual_Profit", string.IsNullOrEmpty(obj.Actual_Profit) ? (object)DBNull.Value : obj.Actual_Profit);
                cmd.Parameters.AddWithValue("@Plan_Total", string.IsNullOrEmpty(obj.Plan_Total) ? (object)DBNull.Value : obj.Plan_Total);
                cmd.Parameters.AddWithValue("@Actual_Total", string.IsNullOrEmpty(obj.Actual_Total) ? (object)DBNull.Value : obj.Actual_Total);
                cmd.Parameters.AddWithValue("@Plan_Follow_Up", string.IsNullOrEmpty(obj.Plan_Follow_Up) ? (object)DBNull.Value : obj.Plan_Follow_Up);
                cmd.Parameters.AddWithValue("@Actual_Follow_Up", string.IsNullOrEmpty(obj.Actual_Follow_Up) ? (object)DBNull.Value : obj.Actual_Follow_Up);
                cmd.Parameters.AddWithValue("@Plan_Engagement", string.IsNullOrEmpty(obj.Plan_Engagement) ? (object)DBNull.Value : obj.Plan_Engagement);
                cmd.Parameters.AddWithValue("@Actual_Engagement", string.IsNullOrEmpty(obj.Actual_Engagement) ? (object)DBNull.Value : obj.Actual_Engagement);
                cmd.Parameters.AddWithValue("@Plan_Affinity", string.IsNullOrEmpty(obj.Plan_Affinity) ? (object)DBNull.Value : obj.Plan_Affinity);
                cmd.Parameters.AddWithValue("@Actual_Affinity", string.IsNullOrEmpty(obj.Actual_Affinity) ? (object)DBNull.Value : obj.Actual_Affinity);
                cmd.Parameters.AddWithValue("@Plan_Visibility", string.IsNullOrEmpty(obj.Plan_Visibility) ? (object)DBNull.Value : obj.Plan_Visibility);
                cmd.Parameters.AddWithValue("@Actual_Visibility", string.IsNullOrEmpty(obj.Actual_Visibility) ? (object)DBNull.Value : obj.Actual_Visibility);
                cmd.Parameters.AddWithValue("@Priority", string.IsNullOrEmpty(obj.Priority) ? (object)DBNull.Value : obj.Priority);
                cmd.Parameters.AddWithValue("@PointsPlanned", string.IsNullOrEmpty(obj.PointsPlanned) ? (object)DBNull.Value : obj.PointsPlanned);
                cmd.Parameters.AddWithValue("@PointsEarned", string.IsNullOrEmpty(obj.PointsEarned) ? (object)DBNull.Value : obj.PointsEarned);
                cmd.Parameters.Add("@identity", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                objresult.id = Convert.ToInt32(cmd.Parameters["@identity"].Value.ToString());
                cmd.Parameters.Clear();




                //   LoadData();

            }
            catch (Exception ex)
            {
                objresult.result = ex.Message;
            }
            return objresult;
        }

       

        public static void DeleteDashboardData(SqlConnection sqlConn, int id)
        {

            try
            {

                if (sqlConn.State == ConnectionState.Closed)
                {
                    sqlConn.Open();
                }
                SqlCommand cmd = new SqlCommand("usp_deletedashdata", sqlConn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }
            catch (Exception ex)
            {
                //RetunMessage = "Error In User Registration ";
            }
        }
        //this method is used to save excel file data of worksheet 2(Calendra) in db
        public static string SaveCalendraData(SqlConnection sqlConn, DataTable dt,string StartDate)
        {
            string Result = "Success";
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (sqlConn.State == ConnectionState.Closed)
                    {
                        sqlConn.Open();
                    }
                    SqlCommand cmd = new SqlCommand("usp_insertCalendraDetails", sqlConn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (dt.Columns.Contains("Id"))
                    {
                        cmd.Parameters.AddWithValue("@Id", string.IsNullOrEmpty(dt.Rows[i]["Id"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Id"].ToString());

                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Id", 0);
                    }
                    cmd.Parameters.AddWithValue("@Sun", string.IsNullOrEmpty(dt.Rows[i]["Sun"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Sun"].ToString());
                    cmd.Parameters.AddWithValue("@Mon", string.IsNullOrEmpty(dt.Rows[i]["Mon"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Mon"].ToString());
                    cmd.Parameters.AddWithValue("@Tue", string.IsNullOrEmpty(dt.Rows[i]["Tue"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Tue"].ToString());
                    cmd.Parameters.AddWithValue("@Wed", string.IsNullOrEmpty(dt.Rows[i]["Wed"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Wed"].ToString());
                    cmd.Parameters.AddWithValue("@Thu", string.IsNullOrEmpty(dt.Rows[i]["Thu"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Thu"].ToString());
                    cmd.Parameters.AddWithValue("@Fri", string.IsNullOrEmpty(dt.Rows[i]["Fri"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Fri"].ToString());
                    cmd.Parameters.AddWithValue("@Sat", string.IsNullOrEmpty(dt.Rows[i]["Sat"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Sat"].ToString());

                    cmd.Parameters.AddWithValue("@StartTime", StartDate);
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();


                }

            }
            catch (Exception ex)
            {
                Result = ex.Message;
            }
            return Result;
        }

        public static EventPlanResult UpdateCalendraData(SqlConnection sqlConn, int id, string Subject, string StartTime,string Type, string Location, string Venue, string EndTime)
        {
            EventPlanResult objres = new EventPlanResult();
            objres.result = "success";
            try
            {
                if (sqlConn.State == ConnectionState.Closed)
                {
                    sqlConn.Open();
                }
                SqlCommand cmd = new SqlCommand("usp_inserCalendraSchedularDetail", sqlConn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Subject", Subject);
                cmd.Parameters.AddWithValue("@StartTime", StartTime);

                cmd.Parameters.AddWithValue("@Type", string.IsNullOrEmpty(Type) ? (object)DBNull.Value : Type);
                cmd.Parameters.AddWithValue("@Location", string.IsNullOrEmpty(Location) ? (object)DBNull.Value :Location);
                cmd.Parameters.AddWithValue("@Venue", string.IsNullOrEmpty(Venue) ? (object)DBNull.Value : Venue);
                cmd.Parameters.AddWithValue("@EndTime", string.IsNullOrEmpty(EndTime) ? (object)DBNull.Value : DateTime.Parse(EndTime));
                
                cmd.Parameters.Add("@identity", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                objres.id = Convert.ToInt32(cmd.Parameters["@identity"].Value.ToString());
                cmd.Parameters.Clear();
            }
            catch (Exception ex)
            {
                objres.result = ex.Message;
            }
            return objres;
        }

        public static string SaveUpdateColorData(SqlConnection sqlConn, string eventtype, string color)
        {
            string result = "success";
            try
            {
                if (sqlConn.State == ConnectionState.Closed)
                {
                    sqlConn.Open();
                }
                SqlCommand cmd = new SqlCommand("usp_insertupdateeventpastelcolor", sqlConn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EventName", eventtype);
                cmd.Parameters.AddWithValue("@Color", color);
           
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();



            }
            catch (Exception ex)
            {
                result = ex.Message;
                //RetunMessage = "Error In User Registration ";
            }
            return result;
        }
        public static void DeleteCalendraData(SqlConnection sqlConn,int id)
        {

            try
            {
            
                    if (sqlConn.State == ConnectionState.Closed)
                    {
                        sqlConn.Open();
                    }
                    SqlCommand cmd = new SqlCommand("usp_deleteCalendraDetailbyid", sqlConn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
            }
            catch (Exception ex)
            {
                //RetunMessage = "Error In User Registration ";
            }
        }


        //this method is used to save excel file data of worksheet 3(Chronology) in db
        public static string SaveChronologyData(SqlConnection sqlConn, DataTable dt)
        {
            string Result = "Success";
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (sqlConn.State == ConnectionState.Closed)
                    {
                        sqlConn.Open();
                    }

                    SqlCommand cmd = new SqlCommand("usp_insertChronology", sqlConn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (dt.Columns.Contains("Id"))
                    {
                        cmd.Parameters.AddWithValue("@Id", string.IsNullOrEmpty(dt.Rows[i]["Id"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Id"].ToString());

                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Id", 0);
                    }
                    cmd.Parameters.AddWithValue("@Type", string.IsNullOrEmpty(dt.Rows[i]["Type"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Type"].ToString());
                    cmd.Parameters.AddWithValue("@Brand", string.IsNullOrEmpty(dt.Rows[i]["Brand"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Brand"].ToString());
                    cmd.Parameters.AddWithValue("@Unit", string.IsNullOrEmpty(dt.Rows[i]["Unit"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Unit"].ToString());
                    cmd.Parameters.AddWithValue("@EventName", string.IsNullOrEmpty(dt.Rows[i]["EventName"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["EventName"].ToString());
                    cmd.Parameters.AddWithValue("@Location", string.IsNullOrEmpty(dt.Rows[i]["Location"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Location"].ToString());
                    cmd.Parameters.AddWithValue("@Dates", string.IsNullOrEmpty(dt.Rows[i]["Dates"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Dates"].ToString());
                    cmd.Parameters.AddWithValue("@StartTime", string.IsNullOrEmpty(dt.Rows[i]["StartTime"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["StartTime"].ToString());
                    cmd.Parameters.AddWithValue("@EndTime", string.IsNullOrEmpty(dt.Rows[i]["EndTime"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["EndTime"].ToString());
                    cmd.Parameters.Add("@identity", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();


                }

            }
            catch (Exception ex)
            {
                Result = ex.Message;
            }
            return Result;
        }

        public static EventPlanResult UpdateChronologyData(SqlConnection sqlConn, int id, string Type, string Brand, string Unit, string EventName, string Location, string Dates,string StartTime,string EndTime)
        {
            EventPlanResult objres = new EventPlanResult();
            objres.result = "success";
            try
            {
                if (sqlConn.State == ConnectionState.Closed)
                {
                    sqlConn.Open();
                }
                SqlCommand cmd = new SqlCommand("usp_insertChronology", sqlConn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Type", Type);
                cmd.Parameters.AddWithValue("@Brand", Brand);
                cmd.Parameters.AddWithValue("@Unit", Unit);
                cmd.Parameters.AddWithValue("@EventName", EventName);
                cmd.Parameters.AddWithValue("@Location", Location);
                cmd.Parameters.AddWithValue("@Dates", Dates);
                cmd.Parameters.AddWithValue("StartTime",StartTime);
                cmd.Parameters.AddWithValue("EndTime",EndTime);
                cmd.Parameters.Add("@identity", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                objres.id = Convert.ToInt32(cmd.Parameters["@identity"].Value.ToString());
                cmd.Parameters.Clear();



            }
            catch (Exception ex)
            {
                objres.result = ex.Message;
            }
            return objres;
        }
        public static string UpdateWeightingData(SqlConnection sqlConn, int id, string txtResult, string txtWeight)
        {
            string result = "success";

            try
            {
                if (sqlConn.State == ConnectionState.Closed)
                {
                    sqlConn.Open();
                }
                SqlCommand cmd = new SqlCommand("usp_insertValuePointWeighting", sqlConn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Result", txtResult);
                cmd.Parameters.AddWithValue("@Weight", txtWeight);
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();



            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public static void DeleteChronologyData(SqlConnection sqlConn, int id)
        {

            try
            {

                if (sqlConn.State == ConnectionState.Closed)
                {
                    sqlConn.Open();
                }
                SqlCommand cmd = new SqlCommand("usp_deleteChronologyDatabyid", sqlConn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }
            catch (Exception ex)
            {
                //RetunMessage = "Error In User Registration ";
            }
        }


        public static string SaveValuePointWeighting(SqlConnection sqlConn, DataTable dt)
        {
            string Result = "Success";
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (sqlConn.State == ConnectionState.Closed)
                    {
                        sqlConn.Open();
                    }

                    SqlCommand cmd = new SqlCommand("usp_insertValuePointWeighting", sqlConn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id",0);
                    cmd.Parameters.AddWithValue("@Result", string.IsNullOrEmpty(dt.Rows[i]["Result"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Result"].ToString());
                    cmd.Parameters.AddWithValue("@Weight", string.IsNullOrEmpty(dt.Rows[i]["Weight"].ToString()) ? (object)DBNull.Value : dt.Rows[i]["Weight"].ToString());
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();


                }

            }
            catch (Exception ex)
            {
                Result = ex.Message;
            }
            return Result;
        }


        //this method is used to get event type
        public static DataTable GetEventTypes(SqlConnection sqlConn)
        {
            if (sqlConn.State == ConnectionState.Closed)
            {
                sqlConn.Open();
            }
            DataTable dt = new DataTable();
           
            SqlCommand cmd = new SqlCommand("usp_geteventtype", sqlConn);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;
            da.Fill(dt);
            return dt;

        }


        public static DataTable GetComboTypes(SqlConnection sqlConn)
        {
            if (sqlConn.State == ConnectionState.Closed)
            {
                sqlConn.Open();
            }
            DataTable dtcombo = new DataTable();

            SqlCommand cmd = new SqlCommand("select Id,ColName,ColValue from tbl_datacombocolumn", sqlConn);
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;
            da.Fill(dtcombo);
            return dtcombo;

        }
        //this method is used to get event name
        public static DataTable GetEventName(SqlConnection sqlConn)
        {

            if (sqlConn.State == ConnectionState.Closed)
            {
                sqlConn.Open();
            }
            DataTable dt = new DataTable();

            SqlCommand cmd = new SqlCommand("usp_geteventname", sqlConn);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;

            da.Fill(dt);
            return dt;

        }

    }
}
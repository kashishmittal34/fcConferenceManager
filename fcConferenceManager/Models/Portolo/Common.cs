using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Elimar.Models
{
    public class Common
    {
        public Commondropdownlist GetDropDownList()
        {

            Commondropdownlist commondropdownlist = new Commondropdownlist();
            List<categorydropdown> categorydropdownList = new List<categorydropdown>();
            List<statusdropdown> statusdropdownList = new List<statusdropdown>();
            List<repeatdropdown> repeatdropdownList = new List<repeatdropdown>();

            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand("SP_Getdropdown_List", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        categorydropdown categorydropdown = new categorydropdown();
                        categorydropdown.TaskCategoryID = reader["TaskCategoryID"].ToString();
                        categorydropdown.pKey = int.Parse(reader["pkey"].ToString());
                        categorydropdownList.Add(categorydropdown);
                    }
                    reader.NextResult();
                    while (reader.Read())
                    {
                        statusdropdown statusdropdown = new statusdropdown();
                        statusdropdown.pKey = (int)reader["pKey"];
                        statusdropdown.TaskStatusID = (string)reader["TaskStatusID"];
                        statusdropdownList.Add(statusdropdown);
                    }
                    reader.NextResult();
                    while (reader.Read())
                    {
                        repeatdropdown repeatdropdown = new repeatdropdown();
                        repeatdropdown.pKey = (int)reader["pKey"];
                        repeatdropdown.TaskRepeatID = (string)reader["TaskRepeatID"];
                        repeatdropdownList.Add(repeatdropdown);
                    }
                    commondropdownlist.categorydropdowns = categorydropdownList;
                    commondropdownlist.statusdropdowns = statusdropdownList;
                    commondropdownlist.repeatdropdowns = repeatdropdownList;
                    reader.Close();
                    //cmd.ExecuteNonQuery();
                    con.Close();
                }

            }
            return commondropdownlist;



        }
    }
}
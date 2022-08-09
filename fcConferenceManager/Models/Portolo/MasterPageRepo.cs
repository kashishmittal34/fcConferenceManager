using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace fcConferenceManager.Models.Portolo
{
    public class MasterPageRepo
    {
        private SqlConnection conn;

        public void connection()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            conn = new SqlConnection(connectionString);
        }

        public List<string> findTable(int tableId)
        {
            connection();
            SqlCommand cmd = new SqlCommand("Select * from Portolo_Table_List where Pkey = @id", conn);
            cmd.Parameters.AddWithValue("@id", tableId);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            conn.Open();
            sda.Fill(dt);
            conn.Close();
            List<string> ans = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                ans.Add(Convert.ToString(dr["TableName"]));
                ans.Add(Convert.ToString(dr["LookupField"]));
                
            }

            return ans;

        }

        public bool deleteRole(string tableName, int id)
        {
            connection();

            SqlCommand cmd = new SqlCommand("delete from " + tableName + " where pkey = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            int i = cmd.ExecuteNonQuery();
            conn.Close();

            if (i >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool EditEntry(int eID, string newName, string tableName)
        {
            connection();
            SqlCommand cmd = new SqlCommand("UPDATE " + tableName + " SET Name = @newName WHERE pkey = @id", conn);
            cmd.Parameters.AddWithValue("@newName", newName);
            cmd.Parameters.AddWithValue("@id", eID);

            conn.Open();
            int i = cmd.ExecuteNonQuery();
            conn.Close();

            if (i >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool AddNewData(string tableName,string lookUpField, string NewName)
        {
            connection();
            SqlCommand cmd = new SqlCommand("INSERT INTO " + tableName + "("+ lookUpField+")  VALUES (@newName)", conn);
            cmd.Parameters.AddWithValue("@newName", NewName);

            conn.Open();
            int i = cmd.ExecuteNonQuery();
            conn.Close();

            if (i >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }


        }

        public List<SelectListItem> getDropDown()
        {
            connection();
            List<SelectListItem> list = new List<SelectListItem>();
            SqlCommand cmd = new SqlCommand("Select * from Portolo_Table_List order by DisplayName", conn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            conn.Open();
            sda.Fill(dt);
            conn.Close();

            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new SelectListItem() { Text = Convert.ToString(dr["DisplayName"]), Value = Convert.ToString(dr["Pkey"]) });
            }

            return list;

        }
        public List<MasterTable> getData(string tableName, string lookUpField)
        {
            List<MasterTable> tables = new List<MasterTable>();

            connection();

            SqlCommand cmd = new SqlCommand("Select * from " + tableName + " order by "+ lookUpField, conn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            conn.Open();
            sda.Fill(dt);
            conn.Close();

            foreach (DataRow dr in dt.Rows)
            {
                tables.Add(
                    new MasterTable
                    {
                        Id = Convert.ToInt32(dr["pkey"]),
                        Name = Convert.ToString(dr[lookUpField])
                    }
                    );
            }
            return tables;
        }
    }
}

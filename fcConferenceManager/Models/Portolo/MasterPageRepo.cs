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

        public List<string> findTableLookUP(int tableId)
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

        public bool deleteLookUp(string tableName, int id)
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

        public bool EditEntryLookUp(int eID, string newName, string tableName)
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

        public bool AddNewDataLookUp(string tableName,string lookUpField, string NewName)
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

        public List<SelectListItem> getDropDownLookUp()
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
        public List<LookUp> getDataLookUp(string tableName, string lookUpField)
        {
            List<LookUp> tables = new List<LookUp>();

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
                    new LookUp
                    {
                        Id = Convert.ToInt32(dr["pkey"]),
                        Name = Convert.ToString(dr[lookUpField])
                    }
                    );
            }
            return tables;
        }

        public List<GlobalSetting> getDataGlobalSetting()
        {
            List<GlobalSetting> tables = new List<GlobalSetting>();

            connection();

            SqlCommand cmd = new SqlCommand("Select * from Application_Settings where UserEditable = 1 order by SettingID", conn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            conn.Open();
            sda.Fill(dt);
            conn.Close();

            foreach (DataRow dr in dt.Rows)
            {
                tables.Add(
                    new GlobalSetting
                    {
                        Id = Convert.ToInt32(dr["pkey"]),
                        Setting = Convert.ToString(dr["SettingID"]),
                        Value = Convert.ToString(dr["SettingValue"])
                    }
                    );
            }
            return tables;
        }

        public bool EditGlobalSetting(string id, string value)
        {
            connection();
            SqlCommand cmd = new SqlCommand("UPDATE Application_Settings SET SettingValue = @Value WHERE pkey = @id", conn);
            cmd.Parameters.AddWithValue("@Value", value);
            cmd.Parameters.AddWithValue("@id", Convert.ToInt32(id));
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
        
        public List<RegistrationSetting> getDataRegistrationSetting()
        {
            List<RegistrationSetting> tables = new List<RegistrationSetting>();

            connection();

            SqlCommand cmd = new SqlCommand("Select * from SYS_RegistrationLevels order by RegistrationLevelID", conn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            conn.Open();
            sda.Fill(dt);
            conn.Close();

            foreach (DataRow dr in dt.Rows)
            {
                
                string coupon = "";
                if (dr["AllowCoupons"] != DBNull.Value && Convert.ToInt32(dr["AllowCoupons"]) == 1)
                {
                    coupon = "Yes";
                }
                else
                {
                    coupon = "No";
                }
                tables.Add(
                    new RegistrationSetting
                    {
                        Id = Convert.ToInt32(dr["pkey"]),
                        RegistrationLevelID = Convert.ToString(dr["RegistrationLevelID"]),
                        Networking = Convert.ToInt32(dr["NetworkingContacts"]),
                        Coupons = coupon
                    }) ;
            }

            return tables;
        }


        public List<EventRole> getDataEventRole()
        {
            List<EventRole> tables = new List<EventRole>();

            connection();

            SqlCommand cmd = new SqlCommand("Select * from SYS_EventRoles order by RoleID", conn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            conn.Open();
            sda.Fill(dt);
            conn.Close();

            foreach (DataRow dr in dt.Rows)
            {
                int secGrp = 0;
                if(dr["SecurityGroup_pKey"] != DBNull.Value)
                {
                    secGrp = Convert.ToInt32(dr["SecurityGroup_pKey"]);
                }
                
                tables.Add(
                    new EventRole
                    {
                        Id = Convert.ToInt32(dr["pkey"]),
                        Role = Convert.ToString(dr["RoleID"]),
                        SecurityRole = secGrp
                        
                    });
            }

            return tables;

        }

        public bool EditRegistration(int id, string Networking, bool coupon)
        {

            connection();
            SqlCommand cmd = new SqlCommand("UPDATE SYS_RegistrationLevels SET NetworkingContacts = @networking, AllowCoupons = @coupon WHERE pkey = @id", conn);
            cmd.Parameters.AddWithValue("@networking", Networking);
            if (coupon)
            {
                cmd.Parameters.AddWithValue("@coupon", 1);
            }
            else
            {
                cmd.Parameters.AddWithValue("@coupon", 0);
            }
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

        public List<SelectListItem> getDropDownSecurityRole()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            
            SqlCommand cmd = new SqlCommand("SELECT * FROM SecurityGroup_List order by SecurityGroupID", conn);

            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            conn.Open();
            sda.Fill(dt);
            conn.Close();

            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new SelectListItem() { Text = Convert.ToString(dr["SecurityGroupID"]), Value = Convert.ToString(dr["Pkey"]) });
            }

            return list;
        }
    }
}

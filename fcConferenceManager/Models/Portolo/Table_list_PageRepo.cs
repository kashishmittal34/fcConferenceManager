using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace fcConferenceManager.Models.Portolo
{
    public class Table_list_PageRepo
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

        public bool EditEntryLookUp(int eID, string newName, string tableName, string lookUpField)
        {
            connection();
            SqlCommand cmd = new SqlCommand("UPDATE " + tableName + " SET "+ lookUpField + " = @newName WHERE pkey = @id", conn);
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
                        SecurityRole = secGrp,
                        Widget = getWidget(Convert.ToInt32(dr["pkey"]))

                    });
            }

            return tables;
        }

        public string getWidget(int id)
        {
            

            connection();

            SqlCommand cmd = new SqlCommand("Select * from SYS_RoleWidgets where EventRole_pKey = @id order by Widget_pKey", conn);
            cmd.Parameters.AddWithValue("@id", id);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            conn.Open();
            sda.Fill(dt);
            conn.Close();

            cmd = new SqlCommand("SELECT count(*) as count FROM SYS_Widgets", conn);
            sda = new SqlDataAdapter(cmd);
            DataTable dt2 = new DataTable();

            conn.Open();
            sda.Fill(dt2);
            conn.Close();

            int size = 0;

            foreach(DataRow dr in dt2.Rows)
            {
                size = Convert.ToInt32(dr["count"]);
            }

            char[] arr = new char[size+1];
            for (int i = 0; i <= size; i++)
            {
                arr[i] = '0';
            }
            List<char> tables = new List<char>(arr);

            foreach (DataRow dr in dt.Rows)
            {
                tables[Convert.ToInt32(dr["Widget_pKey"])] = '1';

            }

            string ans = "";
            foreach(var x in tables)
            {
                ans = ans + x;
            }

            return ans;
        }


        public List<Widget> getWidgetList()
        {
            List<Widget> tables = new List<Widget>();
            connection();

            SqlCommand cmd = new SqlCommand("SELECT * FROM SYS_Widgets order by WidgetID", conn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            conn.Open();
            sda.Fill(dt);
            conn.Close();


            foreach (DataRow dr in dt.Rows)
            {
                tables.Add(
                    new Widget()
                    {
                        id = Convert.ToInt32(dr["pkey"]),
                        WidgetID = Convert.ToString(dr["WidgetID"])
                    }
                    );
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

        public bool UpdateEventSecurity(int id, int roleSec)
        {
            connection();
            SqlCommand cmd = new SqlCommand("Update SYS_EventRoles set securityGroup_pkey = @roleSec where pkey = @id", conn);
            cmd.Parameters.AddWithValue("@roleSec", roleSec);
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

        public bool UpdateWidget(int widgetId, int id, int Value)
        {

            connection();

            SqlCommand cmd = new SqlCommand("Select * from SYS_RoleWidgets where EventRole_pKey = @id and Widget_pKey = @widgetId", conn);
            cmd.Parameters.AddWithValue("@widgetId", widgetId);
            cmd.Parameters.AddWithValue("@id", id);

            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            conn.Open();
            sda.Fill(dt);
            conn.Close();

            if (dt.Rows.Count > 0)
            {
                if (Value == 0)
                {
                    cmd = new SqlCommand("delete from SYS_RoleWidgets where EventRole_pKey = @id and Widget_pKey = @widgetId", conn);
                    cmd.Parameters.AddWithValue("@widgetId", widgetId);
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
                else
                {
                    return true;
                }

            }

            else
            {
                if(Value == 1)
                {
                    cmd = new SqlCommand("INSERT INTO SYS_RoleWidgets (Widget_pKey,EventRole_pKey) VALUES (@widgetId, @id)", conn);
                    cmd.Parameters.AddWithValue("@widgetId", widgetId);
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
                else
                {
                    return true;
                }
            }


        }


        public List<SelectListItem> getCardDetails()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            SqlCommand cmd = new SqlCommand("SELECT * FROM CardProcessor_List order by CardProcessorID", conn);

            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            conn.Open();
            sda.Fill(dt);
            conn.Close();
            list.Add(new SelectListItem() { Text = "Not Selected", Value = "" });
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new SelectListItem() { Text = Convert.ToString(dr["CardProcessorID"]), Value = Convert.ToString(dr["Pkey"]) });
            }

            return list;
        }

    }
}


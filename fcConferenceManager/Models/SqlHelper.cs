using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading.Tasks;
using System.Reflection;

namespace MAGI_API.Models
{
    public static class SqlHelper
    {
        static string CONNECTION_STRING = ReadConnectionString();
        internal static string ReadConnectionString()
        {
            string connString = string.Format("Data Source={0};", ConfigurationManager.AppSettings["AppS"].ToString());
            connString += string.Format("Uid={0};", ConfigurationManager.AppSettings["AppL"].ToString());
            connString += string.Format("Pwd={0};", ConfigurationManager.AppSettings["AppP"].ToString());
            connString += string.Format("Database={0};", ConfigurationManager.AppSettings["AppDB"].ToString());
            connString += string.Format("Connect Timeout={0};", ConfigurationManager.AppSettings["AppT"].ToString());
            connString += string.Format("MultipleActiveResultSets={0};", "true");
            return connString;
        }

        internal static DataSet ExecuteSet(string CommandName, CommandType cmdType, SqlParameter[] param = null)
        {
            DataSet set = null;
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = CommandName;
                    if (param != null && param.Length > 0)
                        cmd.Parameters.AddRange(param);
                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            con.Open();
                        }
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            set = new DataSet();
                            da.Fill(set);
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            return set;
        }        

        internal static async Task<DataSet> ExecuteSetAsync(string CommandName, int TableCount, CommandType cmdType, SqlParameter[] param = null)
        {
            DataSet set = null;
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = CommandName;
                    if (param != null && param.Length > 0)
                        cmd.Parameters.AddRange(param);
                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            await con.OpenAsync();
                        }
                        SqlDataReader sdr = await cmd.ExecuteReaderAsync();
                        set = new DataSet();
                        string[] strTables = new string[TableCount];
                        for (int i = 0; i < TableCount; i++)
                        {
                            strTables[i] = string.Format("Table{0}", (i + 1));
                        }
                        set.Load(sdr, LoadOption.PreserveChanges, strTables);
                        cmd.Parameters.Clear();
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            return set;
        }      

        internal static DataTable ExecuteTable(string CommandName, CommandType cmdType, SqlParameter[] param = null)
        {
            DataTable table = null;
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = CommandName;
                    if (param != null && param.Length > 0)
                        cmd.Parameters.AddRange(param);
                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            con.Open();
                        }
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            table = new DataTable();
                            da.Fill(table);
                        }
                        cmd.Parameters.Clear();
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return table;
        }        

        internal static async Task<DataTable> ExecuteTableAsync(string CommandName, CommandType cmdType, SqlParameter[] param = null)
        {
            DataTable table = null;
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = CommandName;
                    if (param != null && param.Length > 0)
                        cmd.Parameters.AddRange(param);
                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            await con.OpenAsync();
                        }
                        SqlDataReader sdr = await cmd.ExecuteReaderAsync();
                        table = new DataTable();
                        table.Load(sdr);
                        cmd.Parameters.Clear();
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            return table;
        }        

        internal static List<T> ExecuteList<T>(string CommandName, CommandType cmdType, SqlParameter[] param = null) where T : class, new()
        {
            List<T> list = null;
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = CommandName;
                    if (param != null && param.Length > 0)
                        cmd.Parameters.AddRange(param);
                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            con.Open();
                        }
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable table = new DataTable();
                            da.Fill(table);
                            list = new List<T>();
                            foreach (var row in table.AsEnumerable())
                            {
                                T obj = new T();
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
                                list.Add(obj);
                            }
                            cmd.Parameters.Clear();
                        }                        
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            return list;
        }        

        internal static async Task<List<T>> ExecuteListAsync<T>(string CommandName, CommandType cmdType, SqlParameter[] param = null) where T : class, new()
        {
            List<T> list = null;
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = CommandName;
                    if (param != null && param.Length > 0)
                        cmd.Parameters.AddRange(param);
                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            await con.OpenAsync();
                        }
                        SqlDataReader sdr = await cmd.ExecuteReaderAsync();
                        DataTable table = new DataTable();
                        table.Load(sdr);
                        list = new List<T>();
                        foreach (var row in table.AsEnumerable())
                        {
                            T obj = new T();
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
                            list.Add(obj);
                        }
                        cmd.Parameters.Clear();
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            return list;
        }

        internal static async Task<List<T>> ExecuteListAsyncAccount<T>(string CommandName, CommandType cmdType, SqlParameter[] param = null) where T : class, new()
        {
            List<T> list = null;
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = CommandName;
                    if (param != null && param.Length > 0)
                        cmd.Parameters.AddRange(param);
                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            await con.OpenAsync();
                        }
                        SqlDataReader sdr = await cmd.ExecuteReaderAsync();
                        DataTable table = new DataTable();
                        table.Load(sdr);
                        list = new List<T>();
                        foreach (var row in table.AsEnumerable())
                        {
                            T obj = new T();
                            foreach (var prop in obj.GetType().GetProperties())
                            {
                                try
                                {
                                    PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
                                    if (prop.Name.ToUpper() != "IMAGE")
                                    {
                                        propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);

                                    }
                                    else
                                    {
                                        string Images = "";
                                        var id = param[0].Value.ToString();
                                        string strImagePath = "~/accountimages/" + id.ToString() + "_img.jpg";
                                        string strPhysicalPath = System.Web.HttpContext.Current.Server.MapPath(strImagePath);

                                        Boolean bExists = false;   //\Images\Icons\BIOBLACK.png
                                        System.IO.FileInfo file = new System.IO.FileInfo(strPhysicalPath);
                                        bExists = (file.Exists);

                                        if (bExists == true)
                                        {
                                            Images = strImagePath.Replace("~/", "");
                                        }
                                        else
                                        {
                                            Images = "~/accountimages/no_person.jpg";
                                        }
                                        propertyInfo.SetValue(obj, Convert.ChangeType(Images, propertyInfo.PropertyType), null);
                                    }
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                            list.Add(obj);
                        }
                        cmd.Parameters.Clear();
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            return list;
        }

        internal static bool ExecuteNonQuery(string CommandName, CommandType cmdType)
        {
            int result = 0;

            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = CommandName;
                    cmd.CommandTimeout = cmd.Connection.ConnectionTimeout;
                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            con.Open();
                        }
                        result = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return (result > 0);
        }

        internal static bool ExecuteNonQuery(string CommandName, CommandType cmdType, SqlParameter[] param = null)
        {
            int result = 0;
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = CommandName;
                    if (param != null && param.Length > 0)
                        cmd.Parameters.AddRange(param);
                    cmd.CommandTimeout = cmd.Connection.ConnectionTimeout;
                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            con.Open();
                        }
                        result = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }

            return (result > 0);
        }       

        internal static async Task<bool> ExecuteNonQueryAsync(string CommandName, CommandType cmdType, SqlParameter[] param = null)
        {
            int result = 0;
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = CommandName;
                    if (param != null && param.Length > 0)
                        cmd.Parameters.AddRange(param);
                    cmd.CommandTimeout = cmd.Connection.ConnectionTimeout;
                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            await con.OpenAsync();
                        }
                        result = await cmd.ExecuteNonQueryAsync();
                        cmd.Parameters.Clear();
                    }
                    catch(Exception )
                    {
                        throw;
                    }
                }
            }
            return (result > 0);
        }     

        internal static int ExecuteScaler(string CommandName, CommandType cmdType, SqlParameter[] param = null)
        {
            int result = 0;
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = CommandName;
                    if (param != null && param.Length > 0)
                        cmd.Parameters.AddRange(param);
                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            con.Open();
                        }
                        result = Convert.ToInt32(cmd.ExecuteScalar());
                        cmd.Parameters.Clear();
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            return result;
        }        

        internal static async Task<int> ExecuteScalerAsync(string CommandName, CommandType cmdType, SqlParameter[] param = null)
        {
            int result = 0;
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = CommandName;
                    if (param != null && param.Length > 0)
                        cmd.Parameters.AddRange(param);
                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            await con.OpenAsync();
                        }
                        result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        cmd.Parameters.Clear();
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            return result;
        }

        internal static List<T> DataTableToList<T>(this DataTable table) where T : class, new()
        {
            try
            {
                List<T> list = new List<T>();
                foreach (var row in table.AsEnumerable())
                {
                    T obj = new T();
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
                    list.Add(obj);
                }
                return list;
            }
            catch
            {
                return null;
            }
        }
        internal static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);           
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {               
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {                   
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }            
            return dataTable;
        }
		internal static async Task<T> ExecuteObjectAsync<T>(string CommandName, CommandType cmdType, SqlParameter[] param = null) where T : class, new()
        {
            T objct = null;
            using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = CommandName;
                    cmd.CommandTimeout = 300;
                    if (param != null && param.Length > 0)
                        cmd.Parameters.AddRange(param);
                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            await con.OpenAsync();
                        }
                        SqlDataReader sdr = await cmd.ExecuteReaderAsync();
                        DataTable table = new DataTable();
                        table.Load(sdr);
                        objct = new T();

                        DataRow row = null;
                        if (table.Rows.Count > 0)
                            row = table.Rows[0];
                        else
                            throw new Exception("Not found");

                        T obj = new T();
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
                        objct = obj;

                        cmd.Parameters.Clear();
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
            return objct;
        }
    }
}

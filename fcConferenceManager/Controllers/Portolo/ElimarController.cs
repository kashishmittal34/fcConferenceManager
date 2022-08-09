using Elimar.Models;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace fcConferenceManager.Controllers
{
    public class ElimarController : Controller
    {
        // GET: Elimar
        public ActionResult Index()
        {
            ElimarResponse elimar = GetElimardata();
            return View("~/Views/Portolo/Elimar/Elimar.cshtml", elimar);
        }

        public ElimarResponse GetElimardata()
        {
            ElimarResponse elimarData = new ElimarResponse();
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            try
            {
                using (SqlConnection con = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand("SP_Application_Text", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows.ToString() == "True")
                        {
                            while (reader.Read())
                            {
                                elimarData.PKey = int.Parse(reader["PKey"].ToString());
                                elimarData.Form = reader["Form"].ToString();
                                elimarData.AppTextBlock = reader["AppTextBlock"].ToString();
                                elimarData.Active = Convert.ToBoolean(reader["Active"].ToString());
                                elimarData.UpdatedDate = Convert.ToDateTime(reader["UpdatedDate"].ToString());



                            }
                        }
                        reader.Close();
                        //cmd.ExecuteNonQuery();
                        con.Close();

                    }
                }

            }
            catch (Exception)
            {

            }
            return elimarData;


        }
    }
}
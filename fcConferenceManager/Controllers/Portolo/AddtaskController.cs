using Elimar.Models;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace fcConferenceManager.Controllers
{
    public class AddtaskController : Controller
    {


        // GET: Addtask
        public ActionResult Add(TaskAdd model)
        {
            Common common = new Common();
            model.commondropdownlist = common.GetDropDownList();
            if (model.title != null && model.description != null)
            {
                string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
                using (SqlConnection con = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand("[SP_TaskAdd]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Title", model.title);
                        cmd.Parameters.AddWithValue("@Description", model.description);
                        cmd.Parameters.AddWithValue("@TaskCategory_pKey", model.category);
                        cmd.Parameters.AddWithValue("@Status", model.status);
                        cmd.Parameters.AddWithValue("@Reviewed", model.reviewed);
                        cmd.Parameters.AddWithValue("@active", model.active);
                        cmd.Parameters.AddWithValue("@PlanDate", model.plandate);
                        cmd.Parameters.AddWithValue("@DueDate", model.duedate);
                        cmd.Parameters.AddWithValue("@Forecast", model.forecast);
                        cmd.Parameters.AddWithValue("@RepeatType_pKey", model.repeat);

                        cmd.Parameters.AddWithValue("@Tips", model.Tips);
                        cmd.Parameters.AddWithValue("@Instruction", model.Instruction);
                        cmd.Parameters.AddWithValue("@Notes", model.Notes);
                        cmd.Parameters.AddWithValue("@Resources", model.Resources);

                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }

                }
            }
            return View("~/Views/Portolo/AddTask/Add.cshtml", model);
        }


    }

}









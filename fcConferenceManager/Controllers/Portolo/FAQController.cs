using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using Elimar.Models;
using fcConferenceManager.Models.Portolo;

namespace fcConferenceManager.Controllers.Portolo
{
    public class FAQController : Controller
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString);

        // GET: FAQ
        public ActionResult FAQList()
        {
            if ((Session["User"] == null) || !((loginResponse)Session["User"]).IsGlobalAdmin)
                return Redirect("~/Account/Portolo");

            DataTable dt = new DataTable();

            string dbquery = "select * from Portolo_FAQ";

            con.Open();
            SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
            _da.Fill(dt);
            con.Close();

            ViewBag.FAQList = dt;

            return View("~/Views/Portolo/FAQ/FAQList.cshtml");
        }

        public ActionResult AddQuestion(FAQ model)
        {
            if ((Session["User"] == null) || !((loginResponse)Session["User"]).IsGlobalAdmin)
                return Redirect("~/Account/Portolo");

            if (model.Question == null)
                return View("~/Views/Portolo/FAQ/AddQuestion.cshtml");

            string query = "";

            if (model.FAQId == 0)
            {
                query = String.Format("INSERT INTO Portolo_FAQ VALUES ('{0}', '{1}', '{2}', '{3}')", model.Question, model.Answer, model.category, model.IsActive);
            }
            else
                query = String.Format("Update Portolo_FAQ Set Question = '{0}', Answer = '{1}', category = '{2}', isActive = '{3}' where Id = {4}", model.Question, model.Answer, model.category, model.IsActive, model.FAQId);

            SqlCommand cmd = new SqlCommand(query, con);

            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

            return RedirectToAction("FAQList", "FAQ");
        }

        public ActionResult EditQuestion(int id)
        {
            if ((Session["User"] == null) || !((loginResponse)Session["User"]).IsGlobalAdmin)
                return Redirect("~/Account/Portolo");

            string dbquery = "select * from Portolo_FAQ where Id = " + id;
            con.Open();

            SqlCommand cmd = new SqlCommand(dbquery, con);
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            FAQ question = new FAQ();
            question.Question = reader["Question"].ToString();
            question.Answer = reader["Answer"].ToString();
            question.FAQId = id;
            question.category = reader["Category"].ToString();
            question.IsActive = (bool)reader["IsActive"];
            con.Close();

            return View("~/Views/Portolo/FAQ/Addquestion.cshtml", question);
        }

        [HttpPost]
        public ActionResult DeleteQuestion(string ids)
        {
            if ((Session["User"] == null) || !((loginResponse)Session["User"]).IsGlobalAdmin)
                return Redirect("~/Account/Portolo");

            string dbquery = String.Format("delete from Portolo_FAQ where Id in ({0}) ", ids);
            con.Open();

            SqlCommand cmd = new SqlCommand(dbquery, con);

            cmd.ExecuteNonQuery();
            con.Close();

            return RedirectToAction("FAQList", "FAQ");
        }

        [HttpGet]
        public ActionResult SearchQuestion(string question, string answer, string category, string active, string search)
        {
            DataTable dt = new DataTable();

            string dbquery = String.Format("select * from Portolo_FAQ where Question like '%{0}%' and Answer like '%{1}%' and Category like '%{2}%'", question.Trim(), answer.Trim(), category.Trim());
            if (active != "")
                dbquery += String.Format("and IsActive = '{0}'", active);
            con.Open();
            SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
            _da.Fill(dt);
            con.Close();

            ViewBag.FAQList = dt;
            ViewBag.IsActive = active;
            dt.Columns["IsActive"].ColumnName = "Status";
            if (search != "true")
            {
                string FileName = String.Format("FAQList_{0:yyMMdd_HH.mm}", DateTime.Now);
                ExportToExcel(dt, FileName);
            }

            return View("~/Views/Portolo/FAQ/FAQList.cshtml");
        }

        private void ExportToExcel(DataTable dt, string fileName)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt, "FAQ_List");
                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=" + fileName.ToString() + ".xlsx");
                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        public ActionResult UserFAQ()
        {
            DataTable dt = new DataTable();

            string categoryQuery = "select distinct category from Portolo_FAQ";

            string dbquery = "Select * from Portolo_FAQ where IsActive = 1";

            SqlCommand cmd = new SqlCommand(categoryQuery, con);
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            List<string> catgories = new List<string>();

            while (reader.Read())
            {
                catgories.Add(reader["category"].ToString());
            }
            reader.Close();

            SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
            _da.Fill(dt);
            con.Close();

            ViewBag.FAQList = dt;
            ViewBag.category = catgories;

            return View("~/Views/Portolo/FAQ/UserFAQ.cshtml");
        }

        [HttpGet]
        public ActionResult UserSearchQuestion(string question, string answer)
        {
            DataTable dt = new DataTable();

            string dbquery = String.Format("select * from Portolo_FAQ where Question like '%{0}%' and Answer like '%{1}%'", question.Trim(), answer.Trim());
            string categoryQuery = String.Format("select distinct category from Portolo_FAQ where Question like '%{0}%' and Answer like '%{1}%'", question.Trim(), answer.Trim());
            con.Open();

            SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
            SqlCommand cmd = new SqlCommand(categoryQuery, con);

            SqlDataReader reader = cmd.ExecuteReader();
            List<string> catgories = new List<string>();

            while (reader.Read())
            {
                catgories.Add(reader["category"].ToString());
            }
            reader.Close();
            _da.Fill(dt);
            con.Close();

            ViewBag.FAQList = dt;
            ViewBag.category = catgories;

            return View("~/Views/Portolo/FAQ/UserFAQ.cshtml");

        }

        [HttpPost]
        public JsonResult PostQuestion(string question)
        {
            int Id = ((Elimar.Models.loginResponse)Session["User"]).Id;

            string query = $"Insert into Portolo_userFAQ values ({Id}, '{question}')";

            con.Open();

            SqlCommand cmd = new SqlCommand(query, con);

            cmd.ExecuteNonQuery();
            con.Close();
            return Json(JsonRequestBehavior.AllowGet);

        }
    }
}
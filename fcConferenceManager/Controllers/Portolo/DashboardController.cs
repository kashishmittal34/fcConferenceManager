using Elimar.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;				  
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace fcConferenceManager.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Portolo()
        {
            if (Session["User"] == null)
            {
                Redirect("~/Account/Portolo");
            }


            ViewBag.Baseurl = ConfigurationManager.AppSettings["AppURL"];


            ViewBag.userprofile = Session["User"];
            ViewBag.userprofile.Uimg = Session["AccountImage"].ToString();
            var a = Session["User"];
            return View("~/Views/Portolo/Dashboard/Portolo.cshtml");


        }

        // GET: Dashboard/Details/5
        public ActionResult Details(int id)
        {
            return View("~/Views/Portolo/Dashboard/Details.cshtml");
        }

        // GET: Dashboard/Create
        public ActionResult Create()
        {
            return View("~/Views/Portolo/Dashboard/Create.cshtml");
        }

        // POST: Dashboard/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return Redirect("~/Dashboard/Index");
            }
            catch
            {
                return View("~/Views/Portolo/Dashboard/Create.cshtml");
            }
        }

        // GET: Dashboard/Edit/5
        public ActionResult Edit(int id)
        {
            return View("~/Views/Portolo/Dashboard/Edit.cshtml");
        }

        // POST: Dashboard/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return Redirect("~/Dashboard/Index");
            }
            catch
            {
                return View("~/Views/Portolo/Dashboard/Edit.cshtml");
            }
        }

        // GET: Dashboard/Delete/5
        public ActionResult Delete(int id)
        {
            return View("~/Views/Portolo/Dashboard/Delete.cshtml");
        }

        // POST: Dashboard/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return Redirect("~/Dashboard/Index");
            }
            catch
            {
                return View("~/Views/Portolo/Dashboard/Delete.cshtml");
            }
        }
        [HttpPost]
        public ActionResult Logout()
        {
            var baseUrl = ConfigurationManager.AppSettings["AppURL"];
            ViewBag.Baseurl = baseUrl;
            Session["User"] = null;
            Session.Abandon();
            var url = baseUrl+"/account/portolo/";
            return Json(new { status = "success", redirectUrl = url });
            
        }
public ActionResult Scorecard()
        {
            List<UsersquestionResponse> getusersquestion = new List<UsersquestionResponse>();

            try
            {
                if (Session["User"] == null)
                {
                    return Redirect("~/Account/Portolo");
                }
                
               
                loginResponse objlt = (loginResponse)Session["User"];
                string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
                using (SqlConnection con = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand("GetUsersQuestion", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserId", objlt.Id);
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            UsersquestionResponse getusersquestion1 = new UsersquestionResponse();
                            getusersquestion1.pkey = int.Parse(reader["pkey"].ToString());
                            getusersquestion1.questions = (reader["Questions"].ToString());
                            getusersquestion1.ratingscore = int.Parse(reader["Ratingscore"].ToString());
                            getusersquestion.Add(getusersquestion1);

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
            return View("~/Views/Portolo/Dashboard/Scorecard.cshtml", getusersquestion);


        }

        [HttpPost]
        public List<UserRatingResponse> Insertupdatescore(UserRatingResponse user)
        {
            List<UserRatingResponse> ratingscore = new List<UserRatingResponse>();
            ViewBag.Baseurl = ConfigurationManager.AppSettings["AppURL"];

            try
            {
                loginResponse objlt = (loginResponse)Session["User"];
                string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
                using (SqlConnection con = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand("SP_ScoreRating", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Questionid", user.questions);
                        cmd.Parameters.AddWithValue("@Ratingscore", user.ratingscore);
                        cmd.Parameters.AddWithValue("@UserId", objlt.Id);
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            UserRatingResponse response = new UserRatingResponse();
                            response.ratingscore = int.Parse(reader["Ratingscore"].ToString());
                            ratingscore.Add(response);
                        }
                        reader.Close();
                        //cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }
                return ratingscore;

            }
            catch (Exception)
            {

                if (Session["User"] == null)
                {
                    RedirectToAction("~/Account/Portolo");
                }
            }
            return ratingscore;

        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using fcConferenceManager.Models.Portolo;
using System.IO;
using System.Web.Security;
using fcConferenceManager.Models;

namespace fcConferenceManager.Controllers.Portolo
{
    public class StudyController : Controller
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString);


        public ActionResult Study()
        {
            int id = ((Elimar.Models.loginResponse)Session["User"]).Id;


            DataTable dt = new DataTable();

            string dbquery = " select * from Portolo_StudyMaterials where UserId =" + id;
            con.Open();
            SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
            _da.Fill(dt);
            con.Close();

            ViewBag.Materials = dt;
            ViewBag.UserId = id;
            return View("~/Views/Portolo/Study/Study.cshtml");
        }

        public ActionResult Add(int id)
        {
            Study model = new Study();
            model.UserId = id;
            return View("~/Views/Portolo/Study/Add.cshtml", model);
        }

        public ActionResult Edit(int id)
        {
            string dbquery = " select * from Portolo_StudyMaterials where pkey = " + id;
            con.Open();

            SqlCommand cmd = new SqlCommand(dbquery, con);
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            Study model = new Study();
            model.FileTitle = reader["FileTitle"].ToString();
            model.pkey = id;
            model.FileDescription = reader["FileDescription"].ToString();
            model.FilePath = reader["FilePath"].ToString();
            model.FileName = reader["FileName"].ToString();
            model.UserId = (int)reader["UserId"];
            con.Close();

            return View("~/Views/Portolo/Study/Add.cshtml", model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(Study model, HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Server.MapPath("/StudyMaterials/"), fileName);
                file.SaveAs(path);

                model.FileName = fileName;
                model.FilePath = "/StudyMaterials/" + fileName;
            }
            string dbquery = "";

            if (model.pkey == 0)
            {
                dbquery = String.Format("INSERT INTO Portolo_StudyMaterials VALUES ({0}, '{1}', '{2}', '{3}', '{4}')", model.UserId, model.FileTitle, model.FileDescription, model.FileName, model.FilePath);
            }
            else
            {
                dbquery = String.Format("Update Portolo_StudyMaterials set FileTitle = '{0}', FileDescription = '{1}', FileName = '{2}', Filepath = '{3}' where pkey = {4}",
                model.FileTitle, model.FileDescription, model.FileName, model.FilePath, model.pkey);
            }

            con.Open();

            SqlCommand cmd = new SqlCommand(dbquery, con);

            cmd.ExecuteNonQuery();
            con.Close();

            return RedirectToAction("Study", "Study");
        }

        public ActionResult Search(int id, string name)
        {
            DataTable dt = new DataTable();
            string dbquery = String.Format("select * from Portolo_StudyMaterials where FileTitle like '%{0}%' and UserId = {1}", name.Trim(), id);
            con.Open();

            SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
            _da.Fill(dt);
            con.Close();

            ViewBag.Materials = dt;
            ViewBag.UserId = id;
            return View("~/Views/Portolo/Study/Study.cshtml");

        }

        [HttpPost]
        public ActionResult DeleteTopic(string ids)
        {
            string dbquery = String.Format("delete from Portolo_StudyMaterials where pkey in ({0}) ", ids);
            con.Open();

            SqlCommand cmd = new SqlCommand(dbquery, con);

            cmd.ExecuteNonQuery();
            con.Close();

            return RedirectToAction("Study", "Study");
        }
    }
}
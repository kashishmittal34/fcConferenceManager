using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using Elimar.Models;
using System.IO;
using System;

namespace fcConferenceManager.Controllers
{
    public class SupportController:Controller
    {
        bool view;
        bool add;
        bool edit;
        bool delete;
        int account;
        public SupportController()
        {
            loginResponse objlt = (loginResponse)System.Web.HttpContext.Current.Session["User"];
            
            if (objlt != null )
            {
                string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;

                string query = $"select * from Account_List where GlobalAdministrator = 1 and pKey = {objlt.Id};";

                using (SqlConnection con = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            view = true;
                            edit = true;
                            add = true;
                            delete = true;
                            return;
                        }
                        reader.Close();
                        con.Close();
                    }
                }

                query = $"select * from SecurityGroup_Members sm join Privilage_listForPortolo pl on pl.SecurityGroupPkey = sm.SecurityGroup_pKey where sm.Account_pKey = {objlt.Id} and pl.PrivilageID = 'SupportList';";

                using (SqlConnection con = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            account = int.Parse(reader["Account_pkey"].ToString());
                            view = view || bool.Parse(reader["AllowView"].ToString());
                            add =  add || bool.Parse(reader["AllowAdd"].ToString());
                            edit =edit ||  bool.Parse(reader["AllowEdit"].ToString());
                            delete = delete || bool.Parse(reader["AllowDelete"].ToString());
                        }
                        reader.Close();
                        con.Close();
                    }
                }
            }
        }
        public ActionResult Help(string subject)
        {
            if (!add) return Redirect("~/Account/Portolo");
            Help help = new Help();
            loginResponse objlt = (loginResponse)Session["User"];
            if(objlt == null)return Redirect("~/Account/Portolo");
            help.Name = objlt.firstname +" "+ objlt.lastname;
            help.Email = objlt.mainemail;
            help.Telephone = objlt.phone1;
            ViewBag.Subject = subject;
            return View("~/Views/Portolo/support/help.cshtml",help);
        }
        public ActionResult HelpSubmit(Help help)
        {
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand("SP_AdddDataInPortoloSupport", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@name", help.Name);
                    cmd.Parameters.AddWithValue("@email", help.Email);
                    cmd.Parameters.AddWithValue("@telephone", help.Telephone);
                    cmd.Parameters.AddWithValue("@subject", help.Subject);
                    cmd.Parameters.AddWithValue("@description", help.Discription);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            return RedirectToAction("SupportList", "Support");
        }
        public ActionResult SupportList(string nameSort, int? pageNo,string nameSearch,string emailSearch)
        {
           
            loginResponse objlt = (loginResponse)Session["User"];
            if (objlt == null) return Redirect("~/Account/Portolo");

            if (!view) return Redirect("~/Account/Portolo");

            ViewBag.AllowAdd = add;

            nameSearch = !String.IsNullOrEmpty(nameSearch) ? nameSearch.Trim() : nameSearch;
            emailSearch = !String.IsNullOrEmpty(emailSearch) ? emailSearch.Trim() : emailSearch;

            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            string query = "select * from PortoloSupport";
            ViewData["NameSortParm"] = String.IsNullOrEmpty(nameSort) ? "name_desc" : "";
            ViewData["NameFilter"] = nameSearch;
            ViewData["EmailFilter"] = emailSearch;
            if (nameSearch != null && emailSearch != null)
            {
                query = $"select * from PortoloSupport where name like '%{nameSearch}%' and email like '%{emailSearch}%'";
            }
            else if (nameSearch != null)
            {
                query = $"select * from PortoloSupport where name like '%{nameSearch}%'";
            }
            else if (emailSearch != null)
            {
                query = $"select * from PortoloSupport where email like '%{emailSearch}%'";
            }
            switch (nameSort)
            {
                case "name_desc":
                    query += " order by name desc";
                    break;
                default:
                    query += " order by name";
                    break;
            }
            
            List<Help> helpList = new List<Help>();
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Help help = new Help();
                        help.PKey = int.Parse(reader["pKey"].ToString());
                        help.Email = reader["email"].ToString();
                        help.Name = reader["name"].ToString();
                        help.Subject = reader["subject"].ToString();
                        help.Telephone = reader["telephone"].ToString();
                        help.Discription = reader["description"].ToString();
                        help.reply = reader["reply"].ToString();
                        helpList.Add(help);
                    }
                    reader.Close();
                    con.Close();
                }
            }
            //int count = 5;
            //pageNo = pageNo==null?0:pageNo;
            //helpList = helpList.GetRange((int)(pageNo * 5), count);

            ViewBag.Pages = 1;
            ViewBag.Page = pageNo == null ? 1 : pageNo;
            ViewBag.firstPage = ViewBag.Page <= 3 ? 1 : ViewBag.Page - 3;
            int count = helpList.ToList().Count;
            int noOfPages = count % 5 == 0 ? count / 5 : count / 5 + 1;
            ViewBag.lastPage = noOfPages < 5 ? noOfPages : ViewBag.firstPage + 4;


            if (count > 5)
            {
                int start = (int)(pageNo != null ? (pageNo - 1) * 5 : 0);
                int end = start + 5 > count ? count % 5 : 5;

                helpList = helpList.GetRange(start, end);
                ViewBag.Pages = noOfPages;
            }

            return View("~/Views/Portolo/support/supportList.cshtml", helpList);
        }

        [HttpPost]
        public JsonResult Reply(int id, string message)
        {
            SqlConnection config = new SqlConnection(ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString);
            string query = $"Update PortoloSupport set reply = '{message}' where Pkey = {id}";

            config.Open();

            SqlCommand cmd = new SqlCommand(query, config);

            cmd.ExecuteNonQuery();
            config.Close();
            return Json(JsonRequestBehavior.AllowGet);
        }
    }
}
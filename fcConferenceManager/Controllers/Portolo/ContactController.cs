﻿using fcConferenceManager.Models.Portolo;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace fcConferenceManager.Controllers.Portolo
{
    public class ContactController : Controller
    {
        private string config;
        public ContactController()
        {
            config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
        }
        [CheckActiveEventAttribute]
        [HttpGet]
        public ActionResult ContactUs(PortoloContactUs PortoloContactUs)
        {
            PortoloContactUs.ContactList = GetContactDetails();
            ViewBag.ContactImage =  Session["AccountImage"].ToString();           
            return View("~/Views/Portolo/Contact/ContactUs.cshtml", PortoloContactUs.ContactList);
        }
        private List<PortoloContactUs> GetContactDetails()
        {
            List<PortoloContactUs> librarylist = new List<PortoloContactUs>();
            DataTable dtData = new DataTable();
            string query = "Select * from Portolo_PublicContacts";
            SqlConnection con = new SqlConnection(config);
            con.Open();
            SqlCommand command = new SqlCommand(query, con);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dtData);
            con.Close();
            foreach (DataRow dr in dtData.Rows)
            {
                librarylist.Add(new PortoloContactUs
                {
                    Id = Convert.ToInt32(@dr["Id"]),
                    Role = @dr["Role"].ToString(),
                    Name = dr["Name"].ToString(),
                    Phone = dr["Phone"].ToString(),
                    Email = dr["Email"].ToString(),
                    Title = dr["Title"].ToString(),
                    ImageSrc = dr["ImageSource"].ToString()
                });
            }
            return librarylist;
        }
        [HttpGet]
        public ActionResult PublicStaff(PortoloContactUs PortoloContactUs)
        {
            PortoloContactUs.ContactList = GetContactDetails();
            ViewBag.Message = TempData["Message"];
            ViewBag.ContactImage = Session["AccountImage"].ToString();
            return View("~/Views/Portolo/Contact/PortoloPublicStaff.cshtml", PortoloContactUs);
        }
        [HttpPost]
        public ActionResult NewContacts(PortoloContactUs PortoloContactUs)
        {
            if (PortoloContactUs.ProfileImage != null)
            {

                PortoloContactUs.ImageSrc = GetFileName(PortoloContactUs.ProfileImage);
            }
            if (InsertContact(PortoloContactUs))
            {
                TempData["Message"] = "Added New Contact !!";
                return RedirectToAction("PublicStaff", "Contact");
            }
            else
            {
                ModelState.AddModelError("", "Error In Add File. Please Try Again !!!");
            }
            return RedirectToAction("PublicStaff", "Contact");
        }
        public bool InsertContact(PortoloContactUs cont)
        {
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand("SP_PortoloPublicStaff", con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", "");
                    cmd.Parameters.AddWithValue("@Name", cont.Name.Trim());
                    cmd.Parameters.AddWithValue("@Phone", cont.Phone.Trim());
                    cmd.Parameters.AddWithValue("@Email", cont.Email.Trim());
                    cmd.Parameters.AddWithValue("@Title", cont.Title.Trim());
                    cmd.Parameters.AddWithValue("@Role", cont.Role.Trim());
                    cmd.Parameters.AddWithValue("@ImageSource", cont.ImageSrc);
                    cmd.Parameters.AddWithValue("@status", "Insert");
                    int result = cmd.ExecuteNonQuery();
                    con.Close();
                    if (result > 0)
                        return true;
                    else
                        return false;
                }
            }
        }
        public JsonResult EditContacts(int? id)
        {
            var contact = GetContactDetails().Find(x => x.Id.Equals(id));
            return Json(contact, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdateContacts(PortoloContactUs cont)
        {
            if (cont.ImageSrc == null)
            {
                cont.ImageSrc = string.Empty;                
            }
            if (cont.ProfileImage != null)
            {
                cont.ImageSrc = GetFileName(cont.ProfileImage);
            }
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand("SP_PortoloPublicStaff", con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", cont.Id);
                    cmd.Parameters.AddWithValue("@Name", cont.Name.Trim());
                    cmd.Parameters.AddWithValue("@Phone", cont.Phone.Trim());
                    cmd.Parameters.AddWithValue("@Email", cont.Email.Trim());
                    cmd.Parameters.AddWithValue("@Title", cont.Title.Trim());
                    cmd.Parameters.AddWithValue("@Role", cont.Role.Trim());
                    cmd.Parameters.AddWithValue("@ImageSource", cont.ImageSrc);
                    cmd.Parameters.AddWithValue("@status", "Update");
                    int result = cmd.ExecuteNonQuery();
                    if (result == 1)
                    {
                        TempData["Message"] = "Contact Values Updated !";
                        ModelState.Clear();
                    }
                    else
                    {
                        TempData["Message"] = "Unsucessfull";
                        ModelState.Clear();
                    }
                    con.Close();
                    return Redirect(Request.UrlReferrer.ToString());

                }
            }

        }
        public ActionResult DeleteContact(int Id)
        {
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(config))
            {
                string query = "Delete from Portolo_PublicContacts where Id = '" + Id + "'";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();

                    int result = cmd.ExecuteNonQuery();
                    con.Close();
                    if (result == 1)
                    {
                        TempData["Message"] = "Contact  Deleted!";
                        ModelState.Clear();
                    }
                    else
                    {
                        TempData["Message"] = "Unsucessfull";
                        ModelState.Clear();
                    }


                    return Redirect(Request.UrlReferrer.ToString());
                }
            }
        }
        public string GetFileName(HttpPostedFileBase file)
        {
            var Extension = Path.GetExtension(file.FileName);
            var changeExtension = Path.ChangeExtension(Extension, ".jpg");
            var ResourcefileName = "Portolo_ContactImage_" + DateTime.Now.ToString("MMddyyyyHHmmss") + changeExtension;
            string path = Path.Combine(Server.MapPath("~/PortoloDocuments/"), ResourcefileName);
            string filepath = Url.Content(Path.Combine("~/PortoloDocuments/", ResourcefileName));
            if (System.IO.File.Exists(filepath))
            {
                System.IO.File.Delete(filepath);
            }
            file.SaveAs(path);
            return "/PortoloDocuments/" + ResourcefileName;
        }
    }
}
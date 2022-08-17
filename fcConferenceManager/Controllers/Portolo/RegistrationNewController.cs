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
using ClosedXML.Excel;			 

namespace fcConferenceManager.Controllers
{
    public class RegistrationNewController : Controller
    {
        private string baseurl;

        public RegistrationNewController()
        {
            baseurl = ConfigurationManager.AppSettings["AppURL"].Replace("/forms", ""); 
        }

        public ActionResult Registration(int? userID, int? PortoloKey)
        {
			

			loginResponse objlt = (loginResponse)Session["User"];
            //if (objlt == null) return Redirect("~/Account/Portolo");														
            ViewBag.portolodropdown = getdropdownprtlo();
            ViewBag.Country = GetCountryDropDown();
            ViewBag.organization = GetOrganizationDropDown();
            ViewBag.salutation1 = GetSalutationDropDown();
            ViewBag.state = GetStateDropDown("1");
            ViewBag.timeZone = GetStateDropDown("1");
            ViewBag.suffix = GetSuffixDropDown();
            ViewBag.phonetype1 = GetPhoneTypesDropDown();
            ViewBag.phonetype2 = GetPhoneTypesDropDown();
            List<UserResponse> userList = new List<UserResponse>();
            UserResponse user = new UserResponse();
            if (userID > 0)
            {
                userList = RegistrationList(userID, PortoloKey);
                user.ID = userList[0].ID;
                user.salutation1 = userList[0].salutation1;
                user.firstname = userList[0].firstname;
                user.middlename = userList[0].middlename;
                user.lastname = userList[0].lastname;
                user.suffix = userList[0].suffix;
                user.nickname = userList[0].nickname;
                user.signinaccountid = userList[0].signinaccountid;
                user.MainEmailType = userList[0].MainEmailType;
                user.mainemail = userList[0].mainemail;
                user.Password = userList[0].Password;
                user.sendemailto = userList[0].sendemailto;
                user.skypeaddress = userList[0].skypeaddress;
                user.linkedinURL = userList[0].linkedinURL;
                user.country = userList[0].country;
                user.address1 = userList[0].address1;
                user.address2 = userList[0].address2;
                user.city = userList[0].city;
                user.State = userList[0].State;
                user.zipcode = userList[0].zipcode;
                user.timezone = userList[0].timezone;
                user.phonetype1 = userList[0].phonetype1;
                user.phone1 = userList[0].phone1;
                user.phone1extension = userList[0].phone1extension;
                user.phonetype2 = userList[0].phonetype2;
                user.firstname = userList[0].firstname;
                user.phone2 = userList[0].phone2;
                user.phone2extension = userList[0].phone2extension;
                user.salutation2 = userList[0].salutation2;
                user.name = userList[0].name;
                user.countrycode = userList[0].countrycode;
                user.phonenumber = userList[0].phonenumber;
                user.extension = userList[0].extension;
                user.email = userList[0].email;
                user.jobTitle = userList[0].jobTitle;
                user.department = userList[0].department;
                user.organization = userList[0].organization;
                user.degreesandcertifications = userList[0].degreesandcertifications;
                user.website = userList[0].website;
                user.personalbiography = userList[0].personalbiography;
                user.aboutmyorganizationandmyrole = userList[0].aboutmyorganizationandmyrole;
                user.Uimg = userList[0].Uimg;
                user.CV = userList[0].CV;
                user.countrypkey = userList[0].countrypkey;
                user.state_pkey = userList[0].state_pkey;
                user.salutationzID1 = userList[0].salutationzID1;
                user.suffixvalue= userList[0].suffixvalue;
                ViewBag.userImage = baseurl + userList[0].Uimg;

                //user.Uimg = userList[0].portoloStatus;
                //function call to get the filename
                // user.CV = "("+ Path.GetFileName(userList[0].CV).ToString()+")";
                ViewBag.state = GetStateDropDown(user.countrypkey);
                ViewBag.timeZone = GetTimeZoneDropDown(user.countrypkey);
                return View("~/Views/Portolo/Registration/RegistrationNew.cshtml", user);
            }
            return View("~/Views/Portolo/Registration/RegistrationNew.cshtml", user);
        }
        [HttpPost]
        public ActionResult RegistrationSubmit(UserRequest request, HttpPostedFileBase file, HttpPostedFileBase CVfile,bool? save)
        {
            string base64 = Request.Form["imgCropped"];            
            byte[] bytes = base64 != "" ? Convert.FromBase64String(base64.Split(',')[1]) : null;
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand("SP_InsertRegistrationDataInAccountList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", request.ID);
                    cmd.Parameters.AddWithValue("@Salutation1", request.salutation1);
                    cmd.Parameters.AddWithValue("@FirstName", request.firstname);
                    cmd.Parameters.AddWithValue("@MiddleName", request.middlename);
                    cmd.Parameters.AddWithValue("@LastName", request.lastname);
                    cmd.Parameters.AddWithValue("@Suffix", request.suffix);
                    cmd.Parameters.AddWithValue("@NickName", request.nickname);
                    cmd.Parameters.AddWithValue("@SigninAccountID", request.signinaccountid);
                    cmd.Parameters.AddWithValue("@MainEmailType", request.MainEmailType);
                    cmd.Parameters.AddWithValue("@MainEmail", request.mainemail);
                   // cmd.Parameters.AddWithValue("@Password", request.Password);
                    cmd.Parameters.AddWithValue("@SendEmailTo", request.SendEmailTo);
                    cmd.Parameters.AddWithValue("@Skypeaddress", request.skypeaddress);
                    cmd.Parameters.AddWithValue("@LinkedinURL", request.linkedinURL);
                    cmd.Parameters.AddWithValue("@Country", request.country);
                    cmd.Parameters.AddWithValue("@Address1", request.address1);
                    cmd.Parameters.AddWithValue("@Address2", request.address2);
                    cmd.Parameters.AddWithValue("@City", request.city);
                    cmd.Parameters.AddWithValue("@State", request.State);
                    cmd.Parameters.AddWithValue("@zipcode", request.zipcode);
                    cmd.Parameters.AddWithValue("@Timezone", request.timezone);
                    cmd.Parameters.AddWithValue("@PhoneType1", request.phonetype1);
                    cmd.Parameters.AddWithValue("@PhoneType2", request.phonetype2);
                    cmd.Parameters.AddWithValue("@countryCodephone1", request.countryCodephone1 = "1");
                    cmd.Parameters.AddWithValue("@countryCodephone2", request.countryCodephone2 = "1");
                    cmd.Parameters.AddWithValue("@phone1", request.phone1);
                    cmd.Parameters.AddWithValue("@phone1extension", request.phone1extension);
                    cmd.Parameters.AddWithValue("@phone2", request.phone2);
                    cmd.Parameters.AddWithValue("@phone2extension", request.phone2extension);
                    //cmd.Parameters.AddWithValue("@Salutation2", request.salutation2);
                    //cmd.Parameters.AddWithValue("@Countrycode", request.countrycode);
                    //cmd.Parameters.AddWithValue("@Phonenumber", request.phonenumber);
                    //cmd.Parameters.AddWithValue("@Extension", request.extension);
                    //cmd.Parameters.AddWithValue("@Email", request.email);
                    //cmd.Parameters.AddWithValue("@AssistantName", request.name);
                    cmd.Parameters.AddWithValue("@JobTitle", request.jobTitle);
                    cmd.Parameters.AddWithValue("@Department", request.department);
                    cmd.Parameters.AddWithValue("@Website", request.website);
                    cmd.Parameters.AddWithValue("@Degreesandcertifications", request.degreesandcertifications);
                    cmd.Parameters.AddWithValue("@Organization", request.organization);
                    cmd.Parameters.AddWithValue("@Personalbiography", request.personalbiography);
                    cmd.Parameters.AddWithValue("@Aboutmyorganizationandmyrole", request.aboutmyorganizationandmyrole);

                    if ((file != null) && file.ContentLength > 0)
                    {
                        loginResponse objlt = (loginResponse)Session["User"];

                        int Id = objlt.Id;
                        string filename = Id + "_img.jpg";
                        //string filename = Path.GetFileName(file.FileName);
                        string imgPath = Path.Combine(Server.MapPath("/Accountimages/"), filename);
                        if (bytes != null)
                        {
                            if (System.IO.File.Exists(imgPath))
                            {
                                System.IO.File.Delete(imgPath);
                            }
                            using (FileStream stream = new FileStream(imgPath, FileMode.Create))
                            {
                                stream.Write(bytes, 0, bytes.Length);
                                stream.Flush();
                            }
                        }
                        else
                        {
                            if (System.IO.File.Exists(imgPath))
                            {
                                System.IO.File.Delete(imgPath);
                            }
                            file.SaveAs(imgPath);
                        }
                        cmd.Parameters.AddWithValue("@Uimg", "/Accountimages/" + file.FileName);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Uimg", "");
                    }

                    if ((CVfile != null) && CVfile.ContentLength > 0)

                    {
                        string filename = Path.GetFileName(CVfile.FileName);
                        string CVPath = Path.Combine(Server.MapPath("/UserDocuments/"), filename);
                        CVfile.SaveAs(CVPath);
                        cmd.Parameters.AddWithValue("@CV", "/UserDocuments/" + CVfile.FileName);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@CV", " ");
                    }

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            if(save == true) return RedirectToAction("Registration", "RegistrationNew",new { userID = request.ID });
            return RedirectToAction("ProfilePage", "Account");

        }
        public ActionResult Users(int? id, int? PortoloKey = 0)
        {
            ModelState.Clear();
            List<UserResponse> userList = RegistrationList(0, PortoloKey);
            ViewBag.userList = userList;
            ViewBag.DropdownSelected = PortoloKey;
            return View("~/Views/Portolo/Registration/Users.cshtml", userList);

        }

        public ActionResult UpdatePortoloStatus(string primaryKey, int? PortoloID = 0)
        {
            List<int> primaryKeys = primaryKey.Split(',').Select(int.Parse).ToList();
            foreach (var item in primaryKeys)
            {
                string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
                using (SqlConnection con = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand("Updateportolostatus", con))
                    {
                        con.Open();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@RegistrationKey", item);
                        cmd.Parameters.AddWithValue("@PortoloKey", PortoloID);
                        cmd.ExecuteNonQuery();
                        con.Close();

                    }
                }
            }
            return Json(1, JsonRequestBehavior.AllowGet); ;
        }

         public List<UserResponse> RegistrationList(int? id, int? PortoloKey )
        {
            if (Session["User"] == null)
            {
                Redirect("~/account/portolo");
            }
            loginResponse objlt = (loginResponse)Session["User"];
               int Id = objlt.Id;
            
            if (PortoloKey==null)
            {
                PortoloKey = 0;
            }
           
            List<UserResponse> userList = new List<UserResponse>();
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand("SP_GetUserProfileFromAccountList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", id);
                    //cmd.Parameters.AddWithValue("@PortoloKey", PortoloKey);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        UserResponse response = new UserResponse();
                        response.ID = int.Parse(reader["ID"].ToString());
                        response.salutation1 = reader["salutation1"].ToString();
                        response.firstname = reader["firstname"].ToString();
                        response.middlename = reader["middlename"].ToString();
                        response.lastname = reader["lastname"].ToString();
                        response.suffix = reader["suffix"].ToString();
                        response.nickname = reader["nickname"].ToString();
                        response.signinaccountid = reader["signinaccountid"].ToString();
                        response.MainEmailType = reader["MainEmailType"].ToString();
                        response.mainemail = reader["mainemail"].ToString();
                        //response.Password = reader["Password"].ToString();
                        response.sendemailto = reader["SendEmailTo"].ToString();
                        response.skypeaddress = reader["skypeaddress"].ToString();
                        response.linkedinURL = reader["linkedinURL"].ToString();
                        response.country = reader["Country"].ToString();
                        response.city = reader["city"].ToString();
                        response.address1 = reader["address1"].ToString();
                        response.address2 = reader["address2"].ToString();
                        response.zipcode = reader["ZipCode"].ToString();
                        response.State = reader["State"].ToString();
                        response.timezone = reader["timezone"].ToString();
                        //response.countrycode = reader["countrycode"].ToString();
                        //response.phonenumber = reader["phonenumber"].ToString();
                        //response.extension = reader["extension"].ToString();
                        //response.email = reader["email"].ToString();
                        response.jobTitle = reader["jobTitle"].ToString();
                        response.department = reader["department"].ToString();
                        response.organization = reader["ParentOrganization_pKey"].ToString();
                        response.website = reader["website"].ToString();
                        response.degreesandcertifications = reader["degreesandcertifications"].ToString();
                        response.personalbiography = reader["personalbiography"].ToString();
                        response.aboutmyorganizationandmyrole = reader["aboutmyorganizationandmyrole"].ToString();
                        //response.salutation2 = reader["salutation2"].ToString();
                        response.phonetype1 = reader["phonetype1"].ToString();
                        response.phonetype2 = reader["phonetype2"].ToString();
                        response.phone1 = reader["phone1"].ToString();
                        response.phone1extension = reader["phone1extension"].ToString();
                        response.phone2 = reader["phone2"].ToString();
                        response.phone2extension = reader["phone2extension"].ToString();
                        response.countryCodephone1 = reader["countryCodephone1"].ToString();
                        response.countryCodephone2 = reader["countryCodephone2"].ToString();
                        response.countrypkey = reader["Countrypkey"].ToString();
                        response.state_pkey = reader["state_pkey"].ToString();
                        response.salutationzID1 = reader["salutationzID1"].ToString();
                        response.suffixvalue = reader["suffixvalue"].ToString();
                        //response.portoloStatus = reader["PortoloStatus"].ToString();
                        if (reader["Uimg"].ToString() == null || reader["Uimg"].ToString() == "")
                        {
                            response.Uimg = baseurl + "/UserDocuments/emptyimage.png";//"https://localhost:44376/"+reader["Uimg"].ToString();
                        }
                        else
                        {
                            string filename = Id + "_img.jpg";
                          //  baseurl + "Accountimages/" + dr["imgpath"].ToString();
                            response.Uimg = baseurl + "/Accountimages/" + filename;
                        }
                        if (reader["CV"].ToString() == null || reader["CV"].ToString() == "")
                        {
                            //response.CV =reader["CVfile"].ToString();
                        }
                        else
                        {
                            response.CV = reader["CV"].ToString();
                        }
                        userList.Add(response);
                    }
                    reader.Close();
                    //cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            return userList;

        }

        public ActionResult updateportolo()
        {
            int registrationKey = 1;
            int PortoloKey = 2;
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand("Updateportolostatus", con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PortoloKey", PortoloKey);
                    cmd.Parameters.AddWithValue("@RegistrationKey", registrationKey);
                    cmd.ExecuteNonQuery();
                    con.Close();

                }
            }
            return View("~/Views/Portolo/Registration/updateportolo.cshtml");
        }

        [HttpPost]
        public ActionResult portolofilter(string PortoloKey)
        {

            List<int> portolofltr = PortoloKey.Split(',').Select(int.Parse).ToList();
            return View("~/Views/Portolo/Registration/portolofilter.cshtml");

        }


        public ActionResult getdropdownprtlo()
        {
            List<getdropdownvalues> getdropdownvalues = new List<getdropdownvalues>();

            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand("getdropdownvalueforportolo", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        getdropdownvalues getdropdownvalues1 = new getdropdownvalues();
                        getdropdownvalues1.pkey = int.Parse(reader["pkey"].ToString());
                        getdropdownvalues1.portolostatus = (reader["portolostatus"].ToString());
                        getdropdownvalues.Add(getdropdownvalues1);

                    }
                    reader.Close();
                    //cmd.ExecuteNonQuery();
                    con.Close();
                }
                return View("~/Views/Portolo/Registration/getdropdownprtlo.cshtml");
            }

        }

        public void DownloadExcel()
        {
            DataTable dt = new DataTable();
            dt.Clear();
            dt.Columns.Add("Profile");
            dt.Columns.Add("Salutation");
            dt.Columns.Add("First Name");
            dt.Columns.Add("Middle Name");
            dt.Columns.Add("Last Name");
            dt.Columns.Add("Main Email");
            dt.Columns.Add("Country");
            dt.Columns.Add("Job Title");
            dt.Columns.Add("Department");
            dt.Columns.Add("Organisation");
            dt.Columns.Add("Personal Biography");
            dt.Columns.Add("Portolo Status");

            string FileName = String.Format("PublicTasks_{0:yyMMdd_HH.mm}", DateTime.Now);   
            List<UserResponse> userList = RegistrationList(0, 0);//TempData["taskListResponse"]; 

            if (userList != null)
            {
                foreach (var item in userList)
                {
                    DataRow exceldata = dt.NewRow();
                    exceldata["Profile"] = item.Uimg;
                    exceldata["Salutation"] = item.salutation1;
                    exceldata["First Name"] = item.firstname;
                    exceldata["Middle Name"] = item.middlename;
                    exceldata["Last Name"] = item.lastname;
                    exceldata["Main Email"] = item.mainemail;
                    exceldata["Country"] = item.country;
                    exceldata["Job Title"] = item.jobTitle;
                    exceldata["Department"] = item.department;
                    exceldata["Organisation"] = item.organization;
                    exceldata["Personal Biography"] = item.personalbiography;
                    exceldata["Portolo Status"] = item.portoloStatus;
                    dt.Rows.Add(exceldata);
                }
            }
            ExportToExcel(dt, FileName);

        }
        private void ExportToExcel(DataTable dt, string fileName)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt, "Task_List");
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
        public List<SelectListItem> GetCountryDropDown()
        {
            List<SelectListItem> countryList = new List<SelectListItem>();
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(config))
            {
                string query = "select * from SYS_Countries order by CountryID;";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows.ToString() == "True")
                    {
                        while (reader.Read())
                        {
                            countryList.Add(new SelectListItem { Text = reader["CountryID"].ToString(), Value = reader["pKey"].ToString() });
                        }
                    }
                }
            }
            return countryList;
        }
        public List<SelectListItem> GetSalutationDropDown()
        {
            List<SelectListItem> SalutationList = new List<SelectListItem>();
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(config))
            {
                string query = "select * from SYS_Salutations;";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows.ToString() == "True")
                    {
                        while (reader.Read())
                        {
                            SalutationList.Add(new SelectListItem { Text = reader["SalutationID"].ToString(), Value = reader["pKey"].ToString() });
                        }
                    }
                }
            }
            return SalutationList;
        }

        public List<SelectListItem> GetPhoneTypesDropDown()
        {
            List<SelectListItem> PhoneTypesList = new List<SelectListItem>();
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(config))
            {
                string query = "select * from SYS_PhoneTypes;";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows.ToString() == "True")
                    {
                        while (reader.Read())
                        {
                            PhoneTypesList.Add(new SelectListItem { Text = reader["PhoneTypeID"].ToString(), Value = reader["pKey"].ToString() });
                        }
                    }
                }
            }
            return PhoneTypesList;
        }

        public List<SelectListItem> GetSuffixDropDown()
        {
            List<SelectListItem> SuffixList = new List<SelectListItem>();
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(config))
            {
                string query = "select * from SYS_Suffixes;";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows.ToString() == "True")
                    {
                        while (reader.Read())
                        {
                            SuffixList.Add(new SelectListItem { Text = reader["SuffixID"].ToString(), Value = reader["pKey"].ToString() });
                        }
                    }
                }
            }
            return SuffixList;
        }
        public List<SelectListItem> GetStateDropDown(string country_id)
        {
            country_id = country_id == null ? "1" : country_id;
            List<SelectListItem> stateList = new List<SelectListItem>();
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(config))
            {
                string query = $"select * from SYS_States where Country_pkey={country_id} and StateID!='' order by StateID ;";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows.ToString() == "True")
                    {
                        while (reader.Read())
                        {
                            stateList.Add(new SelectListItem { Text = reader["StateID"].ToString(), Value = reader["pKey"].ToString() });

                        }
                    }
                    con.Close();
                }

            }
            return stateList;
        }
        public List<SelectListItem> GetTimeZoneDropDown(string country_id)
        {
            country_id = country_id == null ? "1" : country_id;
            List<SelectListItem> timeZoneList = new List<SelectListItem>();
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(config))
            {
                string query = $"select CountryCode from SYS_Countries where pKey={country_id};";
                string countryCode = "";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows.ToString() == "True")
                    {
                        while (reader.Read())
                        {
                            //timeZoneList.Add(new SelectListItem { Text = reader["TimeZone"].ToString(), Value = reader["pKey"].ToString() });
                            countryCode = reader["CountryCode"].ToString();
                        }
                    }
                    con.Close();
                }
                query = $"select * from SYS_CountryTimeZone where CountryCode='{countryCode}' and TimeZone!='' order by TimeZone;";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows.ToString() == "True")
                    {
                        while (reader.Read())
                        {
                            timeZoneList.Add(new SelectListItem { Text = reader["TimeZone"].ToString(), Value = reader["pKey"].ToString() });
                        }
                    }
                    con.Close();
                }
            }
            return timeZoneList;
        }
        public List<SelectListItem> GetOrganizationDropDown()
        {
            List<SelectListItem> organizationList = new List<SelectListItem>();
            //organizationList.Add(new SelectListItem { Text = "<-- select organization -->", Value = "" });
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(config))
            {
                string query = "select * from Organization_List where OrganizationID != ''; ";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows.ToString() == "True")
                    {
                        while (reader.Read())
                        {
                            organizationList.Add(new SelectListItem { Text = reader["OrganizationID"].ToString(), Value = reader["pKey"].ToString() });

                        }
                    }
                    con.Close();
                }

            }
            return organizationList;
        }
        public JsonResult State_Bind(string country_id)
        {
            country_id = country_id == null ? "1" : country_id;
            List<SelectListItem> stateList = new List<SelectListItem>();
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(config))
            {
                string query = $"select * from SYS_States where Country_pkey={country_id} and StateID!='' order by StateID ;";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows.ToString() == "True")
                    {
                        while (reader.Read())
                        {
                            stateList.Add(new SelectListItem { Text = reader["StateID"].ToString(), Value = reader["pKey"].ToString() });

                        }
                    }
                    con.Close();
                }
                
            }
            return Json(stateList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult TimeZone_Bind(string country_id)
        {
            country_id = country_id == null ? "1" : country_id;
            List<SelectListItem> timeZoneList = new List<SelectListItem>();
            string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(config))
            {
                string query = $"select CountryCode from SYS_Countries where pKey={country_id};";
                string countryCode = "";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows.ToString() == "True")
                    {
                        while (reader.Read())
                        {
                            //timeZoneList.Add(new SelectListItem { Text = reader["TimeZone"].ToString(), Value = reader["pKey"].ToString() });
                            countryCode = reader["CountryCode"].ToString();
                        }
                    }
                    con.Close();
                }
                query = $"select * from SYS_CountryTimeZone where CountryCode='{countryCode}' and TimeZone!='' order by TimeZone;";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows.ToString() == "True")
                    {
                        while (reader.Read())
                        {
                            timeZoneList.Add(new SelectListItem { Text = reader["TimeZone"].ToString(), Value = reader["pKey"].ToString() });
                        }
                    }
                    con.Close();
                }
            }
            return Json(timeZoneList, JsonRequestBehavior.AllowGet);
        }
    }
}
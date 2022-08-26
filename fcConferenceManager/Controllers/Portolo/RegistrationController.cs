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
using MAGI_API.Models;			 

namespace fcConferenceManager.Controllers
{
    public class RegistrationController : Controller
    {
        private string config;
        private string baseurl;
		SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString);
							   
        public RegistrationController()
        {
            config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            baseurl = ConfigurationManager.AppSettings["BaseURL"];
        }
        public ActionResult Registration(string userID)
        {
            // var baseUrl = ConfigurationManager.AppSettings["AppURL"].Replace("/forms", "");
            // ViewBag.Baseurl = baseUrl;
            ViewBag.GlobalAdmin = ((loginResponse)Session["User"]).IsGlobalAdmin;

													 
            List<UserResponse> userList = new List<UserResponse>();
            UserResponse user = new UserResponse();
            if (!string.IsNullOrEmpty(userID))
            {
                userList = RegistrationList(userID);
                con.Open();
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
                user.organization = userList[0].orgName;
                user.degreesandcertifications = userList[0].degreesandcertifications;
                user.website = userList[0].website;
                user.personalbiography = userList[0].personalbiography;
                user.aboutmyorganizationandmyrole = userList[0].aboutmyorganizationandmyrole;
                user.Uimg = userList[0].Uimg;
                user.CV = userList[0].CV;
                user.staffmember = userList[0].staffmember;
				ViewBag.userImage = ConfigurationManager.AppSettings["AppURL"].Replace("/forms", "")+userList[0].Uimg;

														
                //function call to get the filename
                // user.CV = "("+ Path.GetFileName(userList[0].CV).ToString()+")";

                return View("~/Views/Portolo/Registration/Registration.cshtml", user);
            }
            return View("~/Views/Portolo/Registration/Registration.cshtml", user);
        }
        [HttpPost]
        public ActionResult RegistrationSubmit(UserRequest request, HttpPostedFileBase file, HttpPostedFileBase CVfile)
        {
																									  
            using (SqlConnection con = new SqlConnection(config))
            {
                string query = String.Format(@"If NOT EXISTS (select pKey from Organization_List where OrganizationID = '{0}') Insert into Organization_List (OrganizationID)
                                values ('{0}') select pkey from Organization_List where OrganizationID = '{0}'", request.organization);
                con.Open();
                SqlCommand qcmd = new SqlCommand(query, con);
                SqlDataReader reader = qcmd.ExecuteReader();
                reader.Read();

                using (SqlCommand cmd = new SqlCommand("SP_InsertRegistrationData", con))
                {
                    SqlOperation sql = new SqlOperation();
                    request.Password = sql.EncryptMD5(request.Password);
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
                    cmd.Parameters.AddWithValue("@Password", request.Password);
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
                    //cmd.Parameters.AddWithValue("@countryCodephone1", request.countryCodephone1 = "1");
                    //cmd.Parameters.AddWithValue("@countryCodephone2", request.countryCodephone2 = "1");
                    cmd.Parameters.AddWithValue("@phone1", request.phone1);
                    cmd.Parameters.AddWithValue("@phone1extension", request.phone1extension);
                    cmd.Parameters.AddWithValue("@phone2", request.phone2);
                    cmd.Parameters.AddWithValue("@phone2extension", request.phone2extension);
                    cmd.Parameters.AddWithValue("@Salutation2", request.salutation2);
                    cmd.Parameters.AddWithValue("@Countrycode", request.countrycode);
                    cmd.Parameters.AddWithValue("@Phonenumber", request.phonenumber);
                    cmd.Parameters.AddWithValue("@Extension", request.extension);
                    cmd.Parameters.AddWithValue("@Email", request.email);
                    cmd.Parameters.AddWithValue("@AssistantName", request.name);
                    cmd.Parameters.AddWithValue("@JobTitle", request.jobTitle);
                    cmd.Parameters.AddWithValue("@Department", request.department);
                    cmd.Parameters.AddWithValue("@Website", request.website);
                    cmd.Parameters.AddWithValue("@Degreesandcertifications", request.degreesandcertifications);
                    cmd.Parameters.AddWithValue("@Organization_pkey", reader["Pkey"]);
                    cmd.Parameters.AddWithValue("@Personalbiography", request.personalbiography);
                    cmd.Parameters.AddWithValue("@Aboutmyorganizationandmyrole", request.aboutmyorganizationandmyrole);
                    cmd.Parameters.AddWithValue("@StaffMember", request.staffmember);
																 
					 
																		  
																								   
											 

                    //if ((file != null) && file.ContentLength > 0)
                    //{
                    //    string filename = System.IO.Path.GetFileName(file.FileName);
                    //    string imgPath = System.IO.Path.Combine(Server.MapPath("/UserDocuments/"), filename);
                       
                    //    file.SaveAs(imgPath);

                    //    cmd.Parameters.AddWithValue("@Uimg", "/UserDocuments/" + file.FileName);
                    //}
                    //else
                    //{
                    //    cmd.Parameters.AddWithValue("@Uimg", "");
                    //}

                    if ((CVfile != null) && CVfile.ContentLength > 0)

                    {
                        string filename = System.IO.Path.GetFileName(CVfile.FileName);
                        string CVPath = System.IO.Path.Combine(Server.MapPath("/UserDocuments/"), filename);
                        CVfile.SaveAs(CVPath);
                        cmd.Parameters.AddWithValue("@CV", "/UserDocuments/" + CVfile.FileName);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@CV", " ");
                    }
                    reader.Close();

							   
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            return RedirectToAction("Profile", "Account");

        }
        public ActionResult Users()
        {
            ModelState.Clear();
																		  
										
												  
																			   

            DataTable dt = new DataTable();

            string dbquery = @"Select al.pKey, s.SalutationID, al.Firstname, al.MiddleName, al.Lastname, al.Email, c.CountryID, al.Title, al.Department, o.OrganizationID, al.PersonalBio from Account_List al
                    left join SYS_Salutations s on Al.Salutation_pKey = s.pKey left join SYS_Countries c on al.Country_pKey = c.pKey left join Organization_List o on o.pKey = al.ParentOrganization_pKey where PortoloUser = 1";
            con.Open();
            SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
            _da.Fill(dt);
            con.Close();

            ViewBag.Users = dt;
		 
																					 
											 
			 
																										  
																	 
				 
																					   
					 
								   
																	  
																			  
																			  
											  
									

            return View("~/Views/Portolo/Registration/Users.cshtml");
				 
			 
														   
        }

		[HttpGet]
        public ActionResult SearchUser(string fname, string lname, string email, string titl, string org, string search)
        {
            DataTable dt = new DataTable();

            string dbquery = String.Format(@"Select al.pKey, s.SalutationID, al.Firstname, al.MiddleName, al.Lastname, al.Email, c.CountryID, al.Title, al.Department, o.OrganizationID, al.PersonalBio from Account_List al
                    inner join SYS_Salutations s on Al.Salutation_pKey = s.pKey inner join SYS_Countries c on al.Country_pKey = c.pKey inner join Organization_List o on o.pKey = al.ParentOrganization_pKey where
                    al.Firstname like '%{0}%' and al.Lastname like '%{1}%' and al.Email like '%{2}%' and al.Title like '%{3}%' and o.OrganizationID like '%{4}%' and PortoloUser = 1", fname.Trim(), lname.Trim(), email.Trim(), titl.Trim(), org.Trim());

            con.Open();
            SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
            _da.Fill(dt);
            con.Close();

            ViewBag.Users = dt;

            if (search != "true")
		 
																					 
											 
            {
                string FileName = String.Format("Users_{0:yyMMdd_HH.mm}", DateTime.Now);
                ExportToExcel(dt, FileName);
            }
																					   
					 
								   
																	  
																			  
																			  
											  
									

            return View("~/Views/Portolo/Registration/Users.cshtml");
				 
			 
														   
        }

        [HttpPost]
        public ActionResult DeleteUser(string ids)
        {
            if ((Session["User"] == null) || !((loginResponse)Session["User"]).IsGlobalAdmin)
                return Redirect("~/Account/Portolo");

            string dbquery = String.Format("delete from Account_List where Pkey in ({0})", ids);

            con.Open();
            SqlCommand cmd = new SqlCommand(dbquery, con);
            cmd.ExecuteNonQuery();
            con.Close();

            return RedirectToActionPermanent("Users", "Registration");
        }
		 

        public ActionResult UpdatePortoloStatus(string primaryKey, int? PortoloID = 0)
        {
            List<int> primaryKeys = primaryKey.Split(',').Select(int.Parse).ToList();
            foreach (var item in primaryKeys)
            {
               // string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
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

       public List<UserResponse> RegistrationList(string id)
		 
								
         {
							   
			 
            var baseUrl = ConfigurationManager.AppSettings["AppURL"];
            List<UserResponse> userList = new List<UserResponse>();

            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetRegisteredUserList", con))
                {
                    
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", id);
																		   
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        UserResponse response = new UserResponse();
                        response.ID = Convert.ToInt32(reader["pkey"].ToString());
                        response.salutation1 = reader["Salutation_pKey"].ToString();
                        response.firstname = reader["firstname"].ToString();
                        response.middlename = reader["middlename"].ToString();
                        response.lastname = reader["lastname"].ToString();
                        response.suffix = reader["suffix"].ToString();
                        response.nickname = reader["nickname"].ToString();
                        response.signinaccountid = reader["UL"].ToString();
                        response.MainEmailType = reader["EmailSendOptions"].ToString();
                        response.mainemail = reader["Email"].ToString();
                        //response.Password = reader["Password"].ToString();
                        response.sendemailto = reader["EmailFooter"].ToString();
                        response.skypeaddress = reader["SkypeAddress"].ToString();
                        response.linkedinURL = reader["LinkedInProfile"].ToString();
                        response.country = reader["Country_pKey"].ToString();
                        response.city = reader["city"].ToString();
                        response.address1 = reader["address1"].ToString();
                        response.address2 = reader["address2"].ToString();
                        response.name = reader["ContactName"].ToString();
                        response.zipcode = reader["zipcode"].ToString();
                        response.State = reader["State_pKey"].ToString();
                        response.timezone = reader["Timezone_pKey"].ToString();
                        //response.countrycode = reader["countrycode"].ToString();
                        response.phonenumber = reader["Phone"].ToString();
                        response.extension = reader["AltExtension"].ToString();
                        response.email = reader["Alt_Email"].ToString();
                        response.jobTitle = reader["Title"].ToString();
                        response.department = reader["department"].ToString();
                        response.organization = reader["ParentOrganization_pKey"].ToString();
                        response.website = reader["url"].ToString();
                        response.degreesandcertifications = reader["Degrees"].ToString();
                        response.personalbiography = reader["PersonalBio"].ToString();
                        response.aboutmyorganizationandmyrole = reader["AboutMe"].ToString();
                        response.salutation2 = reader["AltSalutation_pKey"].ToString();
                        response.phonetype1 = reader["oldPhoneType_pKey"].ToString();
                        response.phonetype2 = reader["OldPhoneType2_pKey"].ToString();
                        response.phone1 = reader["Phone_1"].ToString();
                        response.phone1extension = reader["Phone1Ext"].ToString();
                        response.phone2 = reader["phone2"].ToString();
                        response.phone2extension = reader["Phone2Ext"].ToString();
                        response.orgName = reader["OrganizationID"].ToString();
                        response.staffmember = (reader["staffmember"].ToString() != "")?(bool)reader["staffmember"]:false;
                        //response.countryCodephone1 = reader["countryCodephone1"].ToString();
                        //response.countryCodephone2 = reader["countryCodephone2"].ToString();
                        //response.portoloStatus = reader["PortoloStatus"].ToString();
                        //if (reader["Uimg"].ToString() == null || reader["Uimg"].ToString() == "")
                        //{
                        //    response.Uimg = baseUrl + "/UserDocuments/emptyimage.png";//"https://localhost:44376/"+reader["Uimg"].ToString();
                        //}
                        //else
                        //{
                        //    response.Uimg = baseUrl + reader["Uimg"].ToString();
                        //}
                        if (reader["CVFilename"].ToString() == null || reader["CVFilename"].ToString() == "")
                        {
                            //response.CV =reader["CVfile"].ToString();
                        }
                        else
                        {
                            response.CV = reader["CVFilename"].ToString();
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
            //string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
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

           // string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
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
            List<UserResponse> userList = RegistrationList("0");//TempData["taskListResponse"]; 

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
                wb.Worksheets.Add(dt, "User_List");
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
    }
}
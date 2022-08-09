using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Web.Security;
using Elimar.Models;
using fcConferenceManager.Models;
using MAGI_API.Models;
using MAGI_API.Security;
namespace fcConferenceManager.Controllers
{
    public class AccountController : Controller
    {
        static SqlOperation repository = new SqlOperation();
        private string config;
        private string baseurl;

        public AccountController()
        {
            config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            baseurl = ConfigurationManager.AppSettings["AppURL"].Replace("/forms", "");
        }
        DBAccessLayer dba = new DBAccessLayer();
        // GET: Account
        public ActionResult Portolo()
        {
            if (Session["User"] != null)
            {
                return Redirect("~/Dashboard/Portolo");
            }
            else
            {
                ViewBag.NotValidUser = TempData["InvalidUser"];
                return View("~/Views/Portolo/Account/Login.cshtml");
            }
        }

        [HttpPost]
        public ActionResult Login(UserModel model, string redirectTo)
        {
            var Baseurl = baseurl;
            ViewBag.Baseurl = Baseurl;
            loginResponse response = new loginResponse();
            if (model.Password == null || model.UserName == null)
            {
                TempData["InvalidUser"] = "Please enter user name & password";
                return Redirect("~/Account/Portolo");
            }


            try
            {
                //IdentityUser user = repository.GetUserbyNameAndPassword(model.UserName, model.Password).Result;
                //if (user == null)
                //{
                //    OwinError oobj = new OwinError();
                //    oobj.error = "invalid_grant";
                //    oobj.error_description = "The user name or password is incorrect.";
                //    // return Ok(oobj);

                //    ModelState.AddModelError("", oobj.error_description);
                //}

                //else if (!user.EmailConfirmed)
                //{
                //    OwinError oobj = new OwinError();
                //    oobj.error = "invalid_grant";
                //    oobj.error_description = "User did not confirm email.";
                //    ModelState.AddModelError("", oobj.error_description);
                //}
                //else
                //{
                //    OwinResult obj = new OwinResult();
                //    obj.access_token = Guid.NewGuid().ToString();
                //    obj.token_type = "bearer";
                //    obj.expires_in = "86399";
                //    obj.userid = user.CustomerId;
                //    obj.username = user.FirstName + " " + user.LastName;
                //    obj.eventid = user.EventId;
                //    obj.eventname = user.EventName;
                //    obj.eventtypeid = user.EventTypeId;
                //    obj.eventuserid = user.EventAccount_pkey;
                //    obj.eventuserstatusid = user.ParticipationStatus_pKey;
                //    obj.role = "KeyUser";
                //    obj.issued = DateTime.Now.ToString("ddd, dd MMM yyyy HH:mm:ss GMT");
                //    obj.expires = DateTime.Now.AddDays(1).ToString("ddd, dd MMM yyyy HH:mm:ss GMT");
                //    // return Ok(obj);
                //    ///Context.SaveChanges();
                //}
                //return View();
                DataTable table = null;
                using (SqlConnection con = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand("[SP_Login_Page]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserName", model.UserName);

                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        //SqlDataReader sdr = await cmd.ExecuteReaderAsync();
                        table = new DataTable();
                        table.Load(reader);

                        if (table != null && table.Rows.Count > 0)
                        {
                            DataRow dr = table.Rows[0];
                            SqlOperation sql = new SqlOperation();
                            bool val = sql.Validate_Password(model.Password, dr["UP"].ToString());
                            if (val == true)
                            {
                                response.Id = Convert.ToInt32(dr["pKey"]);
                                response.salutation1 = dr["Salutation_pKey"].ToString();
                                response.firstname = dr["Firstname"].ToString();
                                response.middlename = dr["MiddleName"].ToString();
                                response.lastname = dr["Lastname"].ToString();
                                response.mainemail = dr["Email"].ToString();
                                response.country = dr["CountryID"].ToString();
                                response.phone1 = dr["Phone"].ToString();
                                response.city = dr["City"].ToString();
                                response.zipcode = dr["ZipCode"].ToString();
                                // response.organization = dr["organization"].ToString();
                                response.jobTitle = dr["Title"].ToString();
                                response.department = dr["Department"].ToString();
                                response.skypeaddress = dr["SkypeAddress"].ToString();
                                response.personalbiography = dr["PersonalBio"].ToString();
                                response.Uimg = baseurl + "/Accountimages/" + dr["imgpath"].ToString();
                                response.degrees = dr["Degrees"].ToString();
                            }
                            else
                            {
                                TempData["InvalidUser"] = "invalid username & password";
                                return Redirect("~/Account/Portolo");
                            }
                        }
                        else
                        {
                            TempData["InvalidUser"] = "invalid username & password";
                            return Redirect("~/Account/Portolo");
                        }
                        reader.Close();
                        //cmd.ExecuteNonQuery();
                        con.Close();
                        Session["User"] = response;
                        Session["FirstName"] = response.firstname;
                        Session["LastName"] = response.lastname;
                    }
                    if (redirectTo != null) return Redirect("~/" + redirectTo);

                    return Redirect("~/Dashboard/Portolo");

                }

                //var item = "1";//s.FirstOrDefault();
                //if (response.status == "Success")
                //{
                //    TempData["users"] = response;
                //    return RedirectToAction("Profile", "Account", response);

                //}
                //else if (response.status == "User Does not Exists")
                //{

                //    TempData["InvalidUser"] = "Not a valid user.";
                //    return Redirect("~/Account/Portolo");
                //}
                //else
                //{
                //    ViewBag.Failedcount = item;
                //    return Redirect("~/Account/Portolo");
                //}
                //var a = response;
            }
            catch (Exception ex)
            {

                TempData["InvalidUser"] = "Not a valid user.";
                return Redirect("~/Account/Portolo");
            }
            // return Redirect("~/Account/Portolo");
        }


        public ActionResult Index()
        {
            ViewBag.NotValidUser = TempData["InvalidUser"];
            return View("~/Views/Portolo/Account/Index.cshtml");
        }

        [HttpPost]
        // public ActionResult Index(UserModel model)
        // {
        // var baseUrl = ConfigurationManager.AppSettings["AppURL"].Replace("/forms", "");
        // ViewBag.Baseurl = baseUrl;
        // if (model.Password == null || model.UserName == null)
        // {


        // TempData["InvalidUser"] = "Please enter user name & password";
        // return Redirect("~/Account/Portolo");

        // }
        // // var s = cbe.GetCBLoginInfo(model.UserName, model.Password);
        // List<UserResponse> userList = new List<UserResponse>();

        // try
        // {
        // using (SqlConnection con = new SqlConnection(config))
        // {
        // using (SqlCommand cmd = new SqlCommand("SP_Login_Page", con))
        // {
        // cmd.CommandType = CommandType.StoredProcedure;
        // cmd.Parameters.AddWithValue("@UserName", model.UserName);
        // cmd.Parameters.AddWithValue("@Password", model.Password);
        // con.Open();
        // SqlDataReader reader = cmd.ExecuteReader();
        // if (reader.HasRows.ToString() == "True")
        // {
        // while (reader.Read())
        // {
        // UserResponse response = new UserResponse();
        // response.status = reader["Status"].ToString();
        // response.ID = int.Parse(reader["ID"].ToString());
        // response.salutation1 = reader["salutation1"].ToString();
        // response.firstname = reader["firstname"].ToString();
        // response.middlename = reader["middlename"].ToString();
        // response.lastname = reader["lastname"].ToString();
        // response.suffix = reader["suffix"].ToString();
        // response.nickname = reader["nickname"].ToString();
        // response.signinaccountid = reader["signinaccountid"].ToString();
        // response.MainEmailType = reader["MainEmailType"].ToString();
        // response.mainemail = reader["mainemail"].ToString();
        // response.Password = reader["Password"].ToString();
        // response.sendemailto = reader["SendEmailTo"].ToString();
        // response.skypeaddress = reader["skypeaddress"].ToString();
        // response.linkedinURL = reader["linkedinURL"].ToString();
        // response.country = reader["country"].ToString();
        // response.city = reader["city"].ToString();
        // response.address1 = reader["address1"].ToString();
        // response.address2 = reader["address2"].ToString();
        // response.name = reader["AssistantName"].ToString();
        // response.zipcode = double.Parse(reader["zipcode"].ToString());
        // response.State = reader["State"].ToString();
        // response.timezone = reader["timezone"].ToString();
        // response.countrycode = reader["countrycode"].ToString();
        // response.phonenumber = reader["phonenumber"].ToString();
        // response.extension = reader["extension"].ToString();
        // response.email = reader["email"].ToString();
        // response.jobTitle = reader["jobTitle"].ToString();
        // response.department = reader["department"].ToString();
        // response.organization = reader["organization"].ToString();
        // response.website = reader["website"].ToString();
        // response.degreesandcertifications = reader["degreesandcertifications"].ToString();
        // response.personalbiography = reader["personalbiography"].ToString();
        // response.aboutmyorganizationandmyrole = reader["aboutmyorganizationandmyrole"].ToString();
        // response.salutation2 = reader["salutation2"].ToString();
        // response.phonetype1 = reader["phonetype1"].ToString();
        // response.phonetype2 = reader["phonetype2"].ToString();
        // response.phone1 = reader["phone1"].ToString();
        // response.phone1extension = reader["phone1extension"].ToString();
        // response.phone2 = reader["phone2"].ToString();
        // response.phone2extension = reader["phone2extension"].ToString();
        // response.countryCodephone1 = reader["countryCodephone1"].ToString();
        // response.countryCodephone2 = reader["countryCodephone2"].ToString();
        // if (reader["Uimg"].ToString() == null || reader["Uimg"].ToString() == "")
        // {
        // response.Uimg = baseUrl + "/User-images/empty%20image.png";//"https://localhost:44376/"+reader["Uimg"].ToString();
        // }
        // else
        // {
        // response.Uimg = baseUrl + reader["Uimg"].ToString();
        // }
        // if (reader["CV"].ToString() == null || reader["CV"].ToString() == "")
        // {
        // //response.CV =reader["CVfile"].ToString();
        // }
        // else
        // {
        // response.CV = reader["CV"].ToString();
        // }
        // userList.Add(response);
        // }
        // }
        // reader.Close();
        // //cmd.ExecuteNonQuery();
        // con.Close();

        // }
        // }
        // var item = "1";//s.FirstOrDefault();
        // if (userList[0].status == "Success")
        // {
        // TempData["users"] = userList;
        // return RedirectToAction("Profile", "Account", userList);

        // }
        // else if (userList[0].status == "User Does not Exists")

        // {
        // TempData["InvalidUser"] = "Not a valid user.";
        // return Redirect("~/Account/Portolo");
        // }
        // else
        // {
        // ViewBag.Failedcount = item;
        // return Redirect("~/Account/Portolo");
        // }
        // var a = userList;
        // }
        // catch (Exception ex)
        // {
        // TempData["InvalidUser"] = "Not a valid user.";
        // return Redirect("~/Account/Portolo");
        // }
        // return Redirect("~/Account/Portolo");
        // }


        //[HttpPost]
        public List<UserResponse> GetUserProfile()
        {
            List<UserResponse> userList = new List<UserResponse>();
            try
            {
                //var baseUrl = ConfigurationManager.AppSettings["AppURL"].Replace("/forms", "");
                //ViewBag.Baseurl = baseUrl;
                loginResponse objlt = (loginResponse)Session["User"];

                int Id = objlt.Id;
                // var s = cbe.GetCBLoginInfo(model.UserName, model.Password);



                using (SqlConnection con = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand("SP_GetUserProfile", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@pKey", Id);

                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows.ToString() == "True")
                        {





                            while (reader.Read())
                            {
                                UserResponse response = new UserResponse();


                                response.ID = int.Parse(reader["pKey"].ToString());
                                response.salutation1 = reader["Salutation_pKey"].ToString();
                                response.firstname = reader["Firstname"].ToString();
                                response.middlename = reader["MiddleName"].ToString();
                                response.lastname = reader["Lastname"].ToString();
                                // response.suffix = reader["suffix"].ToString();
                                // response.nickname = reader["nickname"].ToString();
                                // response.signinaccountid = reader["signinaccountid"].ToString();
                                // response.MainEmailType = reader["MainEmailType"].ToString();
                                response.mainemail = reader["Email"].ToString();
                                // response.Password = reader["Password"].ToString();
                                // response.sendemailto = reader["SendEmailTo"].ToString();
                                response.skypeaddress = reader["SkypeAddress"].ToString();
                                //  response.linkedinURL = reader["linkedinURL"].ToString();
                                response.country = reader["CountryID"].ToString();
                                response.city = reader["City"].ToString();
                                //response.address1 = reader["address1"].ToString();
                                //response.address2 = reader["address2"].ToString();
                                //response.name = reader["AssistantName"].ToString();
                                //response.zipcode = double.Parse(reader["zipcode"].ToString());
                                //response.State = reader["State"].ToString();
                                //response.timezone = reader["timezone"].ToString();
                                //response.countrycode = reader["countrycode"].ToString();
                                //response.phonenumber = reader["phonenumber"].ToString();
                                //response.extension = reader["extension"].ToString();
                                //response.email = reader["email"].ToString();
                                //response.jobTitle = reader["jobTitle"].ToString();
                                //response.department = reader["department"].ToString();
                                //response.organization = reader["organization"].ToString();
                                //response.website = reader["website"].ToString();
                                //response.degreesandcertifications = reader["degreesandcertifications"].ToString();
                                //response.personalbiography = reader["personalbiography"].ToString();
                                //response.aboutmyorganizationandmyrole = reader["aboutmyorganizationandmyrole"].ToString();
                                //response.salutation2 = reader["salutation2"].ToString();
                                //response.phonetype1 = reader["phonetype1"].ToString();
                                //response.phonetype2 = reader["phonetype2"].ToString();
                                //response.phone1 = reader["phone1"].ToString();
                                //response.phone1extension = reader["phone1extension"].ToString();
                                //response.phone2 = reader["phone2"].ToString();
                                //response.phone2extension = reader["phone2extension"].ToString();
                                //response.countryCodephone1 = reader["countryCodephone1"].ToString();
                                //response.countryCodephone2 = reader["countryCodephone2"].ToString();
                                if (reader["imgpath"].ToString() == null || reader["imgpath"].ToString() == "")
                                {
                                    response.Uimg = baseurl + "/User-images/empty%20image.png";//"https://localhost:44376/"+reader["Uimg"].ToString();
                                }
                                else
                                {
                                    response.Uimg = baseurl + "/Accountimages/" + reader["imgpath"].ToString();
                                }
                                //if (reader["CV"].ToString() == null || reader["CV"].ToString() == "")
                                //{
                                //    //response.CV =reader["CVfile"].ToString();
                                //}
                                //else
                                //{

























































                                //    response.CV = reader["CV"].ToString();

                                //}

                                userList.Add(response);
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
                if (Session["User"] == null)
                {
                    Redirect("~/Account/Portolo");
                }

            }

            return userList;

        }

        // GET: Account/Details/5
        public ActionResult Details(int id)
        {
            return View("~/Views/Portolo/Account/Details.cshtml");
        }
        // GET: Account/Create
        public ActionResult Create()
        {
            return View("~/Views/Portolo/Account/Create.cshtml");
        }

        public ActionResult Profile()
        {

            List<UserResponse> response = GetUserProfile();

            UserResponse user = new UserResponse();
            if (response.Count > 0)
            {
                if (response[0].ID > 0)
                {

                    user.ID = response[0].ID;
                    user.salutation1 = response[0].salutation1;
                    user.firstname = response[0].firstname;
                    user.middlename = response[0].middlename;
                    user.lastname = response[0].lastname;
                    user.suffix = response[0].suffix;
                    user.nickname = response[0].nickname;
                    user.signinaccountid = response[0].signinaccountid;
                    user.MainEmailType = response[0].MainEmailType;
                    user.mainemail = response[0].mainemail;
                    user.Password = response[0].Password;
                    user.sendemailto = response[0].sendemailto;
                    user.skypeaddress = response[0].skypeaddress;
                    user.linkedinURL = response[0].linkedinURL;
                    user.country = response[0].country;
                    user.address1 = response[0].address1;
                    user.address2 = response[0].address2;
                    user.city = response[0].city;
                    user.State = response[0].State;
                    user.zipcode = response[0].zipcode;
                    user.timezone = response[0].timezone;
                    user.phonetype1 = response[0].phonetype1;
                    user.phone1 = response[0].phone1;
                    user.phone1extension = response[0].phone1extension;
                    user.phonetype2 = response[0].phonetype2;
                    user.firstname = response[0].firstname;
                    user.phone2 = response[0].phone2;
                    user.phone2extension = response[0].phone2extension;
                    user.salutation2 = response[0].salutation2;
                    user.name = response[0].name;
                    user.countrycode = response[0].countrycode;
                    user.phonenumber = response[0].phonenumber;
                    user.extension = response[0].extension;
                    user.email = response[0].email;
                    user.jobTitle = response[0].jobTitle;
                    user.department = response[0].department;
                    user.organization = response[0].organization;
                    user.degreesandcertifications = response[0].degreesandcertifications;
                    user.website = response[0].website;
                    user.personalbiography = response[0].personalbiography;
                    user.aboutmyorganizationandmyrole = response[0].aboutmyorganizationandmyrole;
                    user.Uimg = response[0].Uimg;
                    user.CV = response[0].CV;


                    return View("~/Views/Portolo/Account/Profile.cshtml", user);
                }
                return View("~/Views/Portolo/Account/Profile.cshtml", user);
            }
            else
            {
                return Redirect("~/Account/Portolo");
            }
        }
        public ActionResult ProfilePage()
























        {


























































































































































            List<UserResponse> response = GetUserProfile2();







            UserResponse user = new UserResponse();
            if (response.Count > 0)

            {
                if (response[0].ID > 0)





                {
































                    user.ID = response[0].ID;
                    user.salutation1 = response[0].salutation1;
                    user.firstname = response[0].firstname;
                    user.middlename = response[0].middlename;
                    user.lastname = response[0].lastname;
                    user.suffix = response[0].suffix;
                    user.nickname = response[0].nickname;
                    user.signinaccountid = response[0].signinaccountid;
                    user.MainEmailType = response[0].MainEmailType;
                    user.mainemail = response[0].mainemail;
                    user.Password = response[0].Password;
                    user.sendemailto = response[0].sendemailto;
                    user.skypeaddress = response[0].skypeaddress;
                    user.linkedinURL = response[0].linkedinURL;
                    user.country = response[0].country;
                    user.address1 = response[0].address1;
                    user.address2 = response[0].address2;
                    user.city = response[0].city;
                    user.State = response[0].State;
                    user.zipcode = response[0].zipcode;
                    user.timezone = response[0].timezone;
                    user.phonetype1 = response[0].phonetype1;
                    user.phone1 = response[0].phone1;
                    user.phone1extension = response[0].phone1extension;
                    user.phonetype2 = response[0].phonetype2;
                    user.firstname = response[0].firstname;
                    user.phone2 = response[0].phone2;
                    user.phone2extension = response[0].phone2extension;
                    //user.salutation2 = response[0].salutation2;
                    //user.name = response[0].name;
                    //user.countrycode = response[0].countrycode;
                    //user.phonenumber = response[0].phonenumber;
                    //user.extension = response[0].extension;
                    //user.email = response[0].email;
                    user.jobTitle = response[0].jobTitle;
                    user.department = response[0].department;
                    user.organization = response[0].organization;
                    user.degreesandcertifications = response[0].degreesandcertifications;
                    user.website = response[0].website;
                    user.personalbiography = response[0].personalbiography;
                    user.aboutmyorganizationandmyrole = response[0].aboutmyorganizationandmyrole;
                    user.Uimg = response[0].Uimg;
                    user.CV = response[0].CV;
                    user.salutationzID1 = response[0].salutationzID1;




                    return View("~/Views/Portolo/Account/ProfilePage.cshtml", user);
                }
                return View("~/Views/Portolo/Account/ProfilePage.cshtml", user);
            }
            else
            {
                return Redirect("~/Account/Portolo");
            }

        }


        [HttpPost]
        public List<UserResponse> GetUserProfile2()


        {
            List<UserResponse> userList = new List<UserResponse>();
            try


























            {
                //var baseUrl = ConfigurationManager.AppSettings["AppURL"].Replace("/forms", "");
                //ViewBag.Baseurl = baseUrl;
                loginResponse objlt = (loginResponse)Session["User"];














                int Id = objlt.Id;
                // int Id = 10324;
                // var s = cbe.GetCBLoginInfo(model.UserName, model.Password);


                UserResponse response = new UserResponse();
                using (SqlConnection con = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand("SP_GetUserProfileFromAccountList", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Id", Id);

                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows.ToString() == "True")
                        {
                            while (reader.Read())
                            {

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
                                response.country = reader["country"].ToString();
                                response.city = reader["city"].ToString();
                                response.address1 = reader["address1"].ToString();
                                response.address2 = reader["address2"].ToString();
                                response.zipcode = reader["zipcode"].ToString();
                                response.State = reader["State"].ToString();
                                response.timezone = reader["timezone"].ToString();
                                //response.countrycode = reader["countrycode"].ToString();
                                //response.phonenumber = reader["phonenumber"].ToString();
                                //response.extension = reader["extension"].ToString();
                                //response.email = reader["email"].ToString();
                                response.jobTitle = reader["jobTitle"].ToString();
                                response.department = reader["department"].ToString();
                                response.organization = reader["organization"].ToString();
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
                                response.salutationzID1 = reader["salutationzID1"].ToString();

                                if (reader["Uimg"].ToString() == null || reader["Uimg"].ToString() == "")
                                {
                                    response.Uimg = baseurl + "/User-images/empty%20image.png";//"https://localhost:44376/"+reader["Uimg"].ToString();
                                }
                                else
                                {
                                    string filename = Id + "_img.jpg";
                                    //baseurl + "Accountimages/" + dr["imgpath"].ToString();
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
                            }
                        }
                        reader.Close();
                        //cmd.ExecuteNonQuery();

                        con.Close();

                    }

                    //if (response.State == "" || response.State==null )
                    //{
                    //    response.State = "0";
                    //}
                    //  string  query = $"select * from SYS_States where pKey={int.Parse(response.State)};";
                    //    using (SqlCommand cmd = new SqlCommand(query, con))

                    //    {
                    //        con.Open();
                    //        SqlDataReader reader = cmd.ExecuteReader();
                    //        if (reader.HasRows.ToString() == "True")
                    //        {
                    //            while (reader.Read())
                    //            {
                    //                response.State = reader["stateID"].ToString();
                    //            }
                    //        }
                    //    else
                    //    {
                    //        response.State = "";
                    //    }
                    //        reader.Close();
                    //        con.Close();
                    //    }


                    //query = $"select * from SYS_Countries where pKey = {int.Parse(response.country)};";
                    //using (SqlCommand cmd = new SqlCommand(query, con))
                    //{
                    //    con.Open();
                    //    SqlDataReader reader = cmd.ExecuteReader();
                    //    if (reader.HasRows.ToString() == "True")
                    //    {
                    //        while (reader.Read())
                    //        {
                    //            response.country = reader["CountryID"].ToString();
                    //        }
                    //    }
                    //    reader.Close();
                    //    con.Close();
                    //}
                    //query = $"select * from SYS_SendEmailTo where pKey = {int.Parse(response.sendemailto)};";
                    //using (SqlCommand cmd = new SqlCommand(query, con))
                    //{
                    //    con.Open();
                    //    SqlDataReader reader = cmd.ExecuteReader();
                    //    if (reader.HasRows.ToString() == "True")
                    //    {
                    //        while (reader.Read())
                    //        {
                    //            response.sendemailto = reader["SendEmailToID"].ToString();
                    //        }
                    //    }
                    //    reader.Close();
                    //    con.Close();
                    //}
                    //query = $"select * from SYS_Salutations where pKey = {int.Parse(response.salutation1)};";
                    //using (SqlCommand cmd = new SqlCommand(query, con))
                    //{
                    //    con.Open();
                    //    SqlDataReader reader = cmd.ExecuteReader();
                    //    if (reader.HasRows.ToString() == "True")
                    //    {
                    //        while (reader.Read())
                    //        {
                    //            response.salutation1 = reader["SalutationID"].ToString();
                    //        }
                    //    }
                    //    reader.Close();
                    //    con.Close();
                    //}
                    userList.Add(response);
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
                //if (Session["User"] == null)
                //{
                //    Redirect("~/Account/Portolo");
                //}








            }

            return userList;

        }
        public JsonResult BindTimeZones(string Country_Pkey)

        {
            DataTable ds = dba.BindTimeZones(Country_Pkey);
            List<SelectListItem> timezoneList = new List<SelectListItem>();
            foreach (DataRow dr in ds.Rows)
            {
                timezoneList.Add(new SelectListItem { Text = dr["strText"].ToString(), Value = dr["pKey"].ToString() });
            }

            return Json(timezoneList, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateInput(true)]
        [CustomizedAuthorize]
        public JsonResult UpdateOrganisation(FormCollection updateorg)
        {
            User_Login data = new User_Login();
            FormsIdentity identity = (FormsIdentity)User.Identity;
            data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);

            clsOrganization cOrganization = new clsOrganization();
            cOrganization.sqlConn = new SqlConnection(ReadConnectionString());
            cOrganization.intOrganization_pKey = data.ParentOrganization_pKey;
            cOrganization.LoadOrganization();
            cOrganization.strOrganizationID = updateorg["txtOrgName"].ToString();
            cOrganization.intOrganizationType_pKey = Convert.ToInt32(updateorg["cbSiteType"].ToString());
            cOrganization.strOrganizationTypeID = updateorg["hiddencbSiteType"].ToString();
            cOrganization.strParentOrgName = updateorg["txtParentOrgName"].ToString();
            cOrganization.strPrimaryContactName = updateorg["txtPrimaryContactName"].ToString();
            cOrganization.strPrimaryContactPhone = updateorg["txtPrimPhone"].ToString();
            cOrganization.strPrimaryContactEmail = updateorg["txtPrimEmail"].ToString();
            cOrganization.strPrimaryContactTitle = updateorg["txtPrimTitle"].ToString();
            cOrganization.intCountry_Pkey = 0;
            cOrganization.intState_Pkey = 0;
            cOrganization.intTimezone_pKey = 0;
            cOrganization.strTimezoneID = "";

            if (updateorg["cbCountry"].ToString() != "0")
                cOrganization.intCountry_Pkey = Convert.ToInt32(updateorg["cbCountry"]);

            if (updateorg["cbState"].ToString() != "0")
                cOrganization.intState_Pkey = Convert.ToInt32(updateorg["cbState"]);

            if (updateorg["cbTimeZone"] != "0")
            {
                cOrganization.intTimezone_pKey = Convert.ToInt32(updateorg["cbTimeZone"]);
                cOrganization.strTimezoneID = updateorg["hiddenTimeZone"].ToString();
            }
            cOrganization.strZip = updateorg["txtZip"].ToString();
            cOrganization.strTimezoneID = updateorg["cbTimeZone"].ToString();
            cOrganization.strEmail2 = updateorg["txtEmail2"].ToString();
            cOrganization.strEmail = updateorg["txtEmail1"].ToString();
            cOrganization.strURL = updateorg["txtURL"].ToString();
            cOrganization.strAddress1 = updateorg["txtAddress1"].ToString();
            cOrganization.strAddress2 = updateorg["txtAddress2"].ToString();
            cOrganization.strCity = updateorg["txtCity"].ToString();
            cOrganization.strCountry = updateorg["hiddenCountry"].ToString();
            string errorMsg = "";
            if (!string.IsNullOrEmpty(cOrganization.strPrimaryContactEmail))
                if (!clsUtility.CheckEmailFormat(cOrganization.strPrimaryContactEmail))
                    errorMsg = "Invalid Email Address: Primary Contact Email \n Enter email, address in valid format";

            if (!string.IsNullOrEmpty(cOrganization.strEmail2))
                if (!clsUtility.CheckEmailFormat(cOrganization.strEmail2))
                    errorMsg = "Invalid Email Address: Email2 \n Enter email, address in valid format";


            if (!string.IsNullOrEmpty(cOrganization.strEmail))
                if (!clsUtility.CheckEmailFormat(cOrganization.strEmail))
                    errorMsg = "Invalid Email Address: Email1 \n Enter email, address in valid format";

            try










            {
                if (errorMsg == "")























                {
                    errorMsg = "Error Occurred While Updating Organization";
                    if (cOrganization.SaveOrganization(cOrganization))
                    {
                        clsAccount cAccount = new clsAccount();
                        clsSettings cSettings = ((clsSettings)Session["cSettings"]);
                        cAccount.sqlConn = new SqlConnection(ReadConnectionString());
                        cAccount.intAccount_PKey = data.Id;
                        cAccount.LoadAccount();
                        errorMsg = "Success";

                    }



































































































































                }
            }

            catch (Exception ex)
            {
                errorMsg = ex.Message;













            }
            return Json(new { result = errorMsg }, JsonRequestBehavior.AllowGet);


        }

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

        // GET: Account/Delete/5
        public ActionResult Delete(int id)
        {
            return View("~/Views/Portolo/Account/Delete.cshtml");
        }

        // POST: Account/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return Redirect("~/Account/Index");
            }
            catch
            {
                return View("~/Views/Portolo/Account/Delete.cshtml");
            }

        }
        [CustomizedAuthorize]
        public ActionResult MyOrganization()
        {
            FormsIdentity identity = (FormsIdentity)User.Identity;
            var data = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<User_Login>(identity.Ticket.UserData);
            clsAccount cAccount = new clsAccount();
            cAccount.sqlConn = new SqlConnection(ReadConnectionString());
            cAccount.intAccount_PKey = data.Id;
            cAccount.LoadAccount();

            clsOrganization cOrganization = new clsOrganization();
            cOrganization.sqlConn = new SqlConnection(ReadConnectionString());
            cOrganization.intOrganization_pKey = cAccount.intParentOrganization_pKey;
            cOrganization.LoadOrganization();

            MyOrganisation myOrganisation = new MyOrganisation();
            myOrganisation.organizationName = cOrganization.strOrganizationID;
            myOrganisation.id = cOrganization.intOrganization_pKey;
            myOrganisation.parentOrganization = cOrganization.strParentOrgName;
            myOrganisation.url = cOrganization.strURL;
            myOrganisation.Type = cOrganization.strOrganizationTypeID;
            myOrganisation.address1 = cOrganization.strAddress1;
            myOrganisation.address2 = cOrganization.strAddress2;
            myOrganisation.state = cOrganization.strState;
            myOrganisation.city = cOrganization.strCity;
            myOrganisation.zip = cOrganization.strZip;
            myOrganisation.country = cOrganization.strCountry;
            myOrganisation.userName = cOrganization.strPrimaryContactName;
            myOrganisation.title = cOrganization.strPrimaryContactTitle;
            myOrganisation.email = cOrganization.strPrimaryContactEmail;
            myOrganisation.phone = cOrganization.strPrimaryContactPhone;
            myOrganisation.strEmail1 = cOrganization.strEmail;
            myOrganisation.strEmail2 = cOrganization.strEmail2;
            myOrganisation.intTimezone_pKey = cOrganization.intTimezone_pKey;

            ViewBag.SiteTypeTable = dba.BindSiteTypes();
            ViewBag.CountryTable = dba.BindCountry();
            ViewBag.StateTable = dba.BindStates();
            ViewBag.TimeZoneTable = dba.BindTimeZones(cOrganization.intCountry_Pkey.ToString());

            return View("~/Views/Portolo/Account/MyOrganization.cshtml", myOrganisation);
        }

    }

}

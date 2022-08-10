using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.Mvc;
using System.Web.Security;
using ClosedXML.Excel;
using Elimar.Models;
using fcConferenceManager.Models;

namespace fcConferenceManager.Controllers
{
    public class SecurityController : Controller
    {
        private string config;
        private string baseurl;

        bool view;
        public SecurityController()
        {
            config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            baseurl = ConfigurationManager.AppSettings["BaseURL"];
            loginResponse objlt = (loginResponse)System.Web.HttpContext.Current.Session["User"];
            if (objlt != null)
            {
                string config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;

                string query = $"select * from Account_List where GlobalAdministrator = 1 and pKey = {objlt.Id};";

                using (SqlConnection con = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            view = true;
                            return;
                        }
                        reader.Close();
                        con.Close();
                    }
                }
            }
        }

        public ActionResult EditSecurityGroup(int? PK)
        {
            loginResponse objlt = (loginResponse)Session["User"];
            if (objlt == null || !view) return Redirect("~/Account/Portolo");
            PK = PK != null ? PK : 46;
            SecurityGroup securityGroup = new SecurityGroup();
            securityGroup.SecurtiyGroupPkey = (int)PK;
            string query = $"select * from SecurityGroup_List where pKey = {PK}";
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        securityGroup.Name = reader["SecurityGroupID"].ToString();
                        securityGroup.Description = reader["Description"].ToString();
                    }
                    reader.Close();
                    con.Close();
                }
            }
            List<Component>componentList = new List<Component>();
            query = $"select * from Privilage_listForPortolo where SecurityGroupPkey={PK}";
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Component component = new Component();
                        component.ComponentName = reader["PrivilageID"].ToString();
                        component.AllowView = bool.Parse(reader["AllowView"].ToString());
                        component.AllowAdd = bool.Parse(reader["AllowAdd"].ToString());
                        component.AllowEdit = bool.Parse(reader["AllowEdit"].ToString());
                        component.AllowDelete = bool.Parse(reader["AllowDelete"].ToString());
                        componentList.Add(component);
                    }
                    reader.Close();
                    con.Close();
                }
            }
            List<SecurityGroupMember> securityGroupMemberList = new List<SecurityGroupMember>();
            query = $"select * from SecurityGroup_Members SM join account_list al on SM.Account_pKey = al.pKey where SM.SecurityGroup_pKey = {PK}; ";
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        SecurityGroupMember securityGroupMember = new SecurityGroupMember();    
                        securityGroupMember.AccountName = reader["contactName"].ToString();
                        securityGroupMember.AccountID = int.Parse(reader["Account_pKey"].ToString());
                        securityGroupMember.Activated = true;
                        securityGroupMemberList.Add(securityGroupMember);
                    }
                    reader.Close();
                    con.Close();
                }
            }
            securityGroup.ComponentList = componentList;
            securityGroup.members = securityGroupMemberList;
            return View("~/Views/Portolo/Security/EditSecurityGroup.cshtml",securityGroup);
        }
        public ActionResult AddMember(int? PK ,int? pageNo, string nameSearch, string emailSearch)
        {
            loginResponse objlt = (loginResponse)Session["User"];
            if (objlt == null || !view) return Redirect("~/Account/Portolo");
            List<UserRequest> userList = new List<UserRequest>();
            string query = $"select * from Account_List a join Organization_List o on a.ParentOrganization_pKey = o.pKey where a.ContactName != '' order by a.ContactName; ";
            ViewData["NameFilter"] = nameSearch;
            ViewData["EmailFilter"] = emailSearch;
            if (nameSearch != null && emailSearch != null)
            {
                query = $"select * from Account_List a join Organization_List o on a.ParentOrganization_pKey = o.pKey where a.ContactName != '' and a.ContactName like '%{nameSearch}%' and a.email like '%{emailSearch}%' order by a.ContactName";
            }
            else if (nameSearch != null)
            {
                query = $"select * from Account_List a join Organization_List o on a.ParentOrganization_pKey = o.pKey where a.ContactName != '' and a.ContactName like '%{nameSearch}%' order by a.ContactName";
            }
            else if (emailSearch != null)
            {
                query = $"select * from Account_List a join Organization_List o on a.ParentOrganization_pKey = o.pKey where a.ContactName != '' and a.email like '%{emailSearch}%' order by a.ContactName";
            }
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        UserRequest user = new UserRequest();
                        user.name = reader["contactName"].ToString();
                        user.ID = int.Parse(reader["pKEy"].ToString());
                        user.email = reader["Email"].ToString();
                        user.organization = reader["organizationId"].ToString();
                        userList.Add(user);
                    }
                    reader.Close();
                    con.Close();
                }
            }
            ViewBag.Pages = 1;
            ViewBag.Page = pageNo == null ? 1 : pageNo;
            ViewBag.firstPage = ViewBag.Page <= 3 ? 1 : ViewBag.Page - 3;
            int count = userList.Count;
            int noOfPages = count % 40 == 0 ? count / 40 : count / 40 + 1;
            ViewBag.lastPage = noOfPages < 5 ? noOfPages : ViewBag.firstPage + 4;
            ViewBag.noOfPage = noOfPages;
            ViewBag.lastPage = ViewBag.lastPage > noOfPages ? noOfPages : ViewBag.lastPage;

            PK = PK != null?PK:46 ;
            ViewData["PK"] = PK;
            if (count > 40)
            {

                int start = (int)(pageNo != null ? (pageNo - 1) * 40 : 0);
                int end = start + 40 > count ? count % 40 : 40;

                userList = userList.GetRange(start, end);
                ViewBag.Pages = noOfPages;
            }
            return View("~/Views/Portolo/Security/AddMember.cshtml",userList);
        }
        [HttpPost]
        public ActionResult AddMember(int? PK,int[] arrayOfValues)
        {
            if(arrayOfValues != null)
            {
                foreach(var item in arrayOfValues)
                {
                    using (SqlConnection con = new SqlConnection(config))
                    {
                        string query = $"If not exists (select pKey from SecurityGroup_Members where Account_pKey={item} and SecurityGroup_pKey={PK} ) begin Insert into SecurityGroup_Members(Account_pKey, SecurityGroup_pKey) values({item}, {PK}) end; ";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }
            }
            return RedirectToAction("EditSecurityGroup", "Security");
        }
        public void RemoveMember(int? PK, int[] members)
        {
            if (members != null)
            {
                foreach (var item in members)
                {
                    using (SqlConnection con = new SqlConnection(config))
                    {
                        string query = $"delete from SecurityGroup_Members where Account_pKey ={item} and SecurityGroup_pKey={PK} ; ";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }
            }
        }
        public void ChangeComponent(int? PK, string[] components)
        {
            string query = $"update Privilage_listForPortolo set AllowView = 0, AllowAdd = 0, AllowEdit = 0,AllowDelete = 0  where SecurityGroupPkey={PK}  ";
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            if (components != null)
            {
                foreach (var item in components)
                {
                    string[] items = item.Split('+');
                    query = $"update Privilage_listForPortolo set {items[1]} = 1 where SecurityGroupPkey={PK} and PrivilageID ='{items[0]}' ";
                    using (SqlConnection con = new SqlConnection(config))
                    {
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {                            
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }
            }
        }
    }
}

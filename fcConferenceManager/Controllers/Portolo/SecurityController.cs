using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Security;
using ClosedXML.Excel;
using Elimar.Models;
using fcConferenceManager.Models;
using HttpPostAttribute = System.Web.Mvc.HttpPostAttribute;
using PagedList;
using Telerik.Web.UI;

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

                string query = $"select * from Account_List where StaffMember = 1 and pKey = {objlt.Id};";

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
        public ActionResult SecurityGroup()
        {
            loginResponse objlt = (loginResponse)Session["User"];
            if (objlt == null || !view) return Redirect("~/Account/Portolo");
            List<SecurityGroup> groupList = new List<SecurityGroup>();
            string query = $"select * from Portolo_SecurityGroup ";
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        SecurityGroup securityGroup = new SecurityGroup();
                        securityGroup.Name = reader["SecurityGroupID"].ToString();
                        securityGroup.Description = reader["Description"].ToString();
                        securityGroup.SecurtiyGroupPkey = int.Parse(reader["pKey"].ToString());
                        groupList.Add(securityGroup);
                    }
                    reader.Close();
                    con.Close();
                }
            }

            foreach (var group in groupList)
            {
                query = $"select * from Portolo_SecurityGroupMembers SM join account_list al on SM.Account_pKey = al.pKey where SM.SecurityGroup_pKey =  {group.SecurtiyGroupPkey}";
                List<SecurityGroupMember> memberList = new List<SecurityGroupMember>();
                using (SqlConnection con = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            SecurityGroupMember member = new SecurityGroupMember();
                            member.AccountID = int.Parse(reader["Account_pKey"].ToString());
                            member.AccountName = reader["FirstName"].ToString()+ " " + reader["LastName"].ToString();
                            memberList.Add(member);
                        }
                        reader.Close();
                        con.Close();
                    }
                }
                group.members = memberList;
            }

            return View("~/Views/Portolo/Security/SecurityGroup.cshtml", groupList);
        }


        public ActionResult EditSecurityGroup(int? PK)
        {
            loginResponse objlt = (loginResponse)Session["User"];
            if (objlt == null || !view) return Redirect("~/Account/Portolo");
            PK = PK != null ? PK : 46;
            SecurityGroup securityGroup = new SecurityGroup();
            securityGroup.SecurtiyGroupPkey = (int)PK;
            string query = $"select * from Portolo_SecurityGroup where pKey = {PK}";
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        securityGroup.SecurtiyGroupPkey = (int)PK;
                        securityGroup.Name = reader["SecurityGroupID"].ToString();
                        securityGroup.Description = reader["Description"].ToString();
                    }
                    reader.Close();
                    con.Close();
                }
            }
            List<Component> componentList = new List<Component>();
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
                        component.ComponentPkey = int.Parse(reader["pKey"].ToString());
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
            query = $"select * from Portolo_SecurityGroupMembers SM join account_list al on SM.Account_pKey = al.pKey where SM.SecurityGroup_pKey = {PK}; ";
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
            return View("~/Views/Portolo/Security/EditSecurityGroup.cshtml", securityGroup);
        }

        [HttpPost]
        public ActionResult EditSecurityGroup([FromBody] SecurityGroup group)
        {
            string query = $"update Portolo_SecurityGroup set SecurityGroupID = '{group.Name}' , Description = '{group.Description}' where pKey = {group.SecurtiyGroupPkey};";
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            return RedirectToAction("SecurityGroup");
        }
        public ActionResult AddMember(int? PK, int? page, string nameSearch, string emailSearch)
        {
            loginResponse objlt = (loginResponse)Session["User"];
            if (objlt == null || !view) return Redirect("~/Account/Portolo");
            nameSearch = !string.IsNullOrEmpty(nameSearch) ? nameSearch.Trim() : nameSearch;
            emailSearch = !string.IsNullOrEmpty(emailSearch) ? emailSearch.Trim() : emailSearch;
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
                    cmd.CommandTimeout = 0;
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
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            int pageSize = 40;
            IPagedList<UserRequest> users = null;
            users = userList.ToPagedList(pageIndex,pageSize);
            PK = PK != null ? PK : 46;
            ViewData["PK"] = PK;
            //ViewBag.Pages = 1;
            //ViewBag.Page = pageNo == null ? 1 : pageNo;
            //ViewBag.firstPage = ViewBag.Page <= 3 ? 1 : ViewBag.Page - 3;
            //int count = userList.Count;
            //int noOfPages = count % 40 == 0 ? count / 40 : count / 40 + 1;
            //ViewBag.lastPage = noOfPages < 5 ? noOfPages : ViewBag.firstPage + 4;
            //ViewBag.noOfPage = noOfPages;
            //ViewBag.lastPage = ViewBag.lastPage > noOfPages ? noOfPages : ViewBag.lastPage;

            //PK = PK != null ? PK : 46;
            //ViewData["PK"] = PK;
            //if (count > 40)
            //{

            //    int start = (int)(pageNo != null ? (pageNo - 1) * 40 : 0);
            //    int end = start + 40 > count ? count % 40 : 40;

            //    userList = userList.GetRange(start, end);
            //    ViewBag.Pages = noOfPages;
            //}
            return View("~/Views/Portolo/Security/AddMember.cshtml", users);
        }
        [HttpPost]
        public ActionResult AddMember(int? PK, int[] arrayOfValues)
        {
            if (arrayOfValues != null)
            {
                foreach (var item in arrayOfValues)
                {
                    using (SqlConnection con = new SqlConnection(config))
                    {
                        string query = $"If not exists (select pKey from Portolo_SecurityGroupMembers where Account_pKey={item} and SecurityGroup_pKey={PK} ) begin Insert into Portolo_SecurityGroupMembers(Account_pKey, SecurityGroup_pKey) values({item}, {PK}) end; ";
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
                        string query = $"delete from Portolo_SecurityGroupMembers where Account_pKey ={item} and SecurityGroup_pKey={PK} ; ";
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
        public JsonResult AddGroup(string groupName)
        {
            int row = 0;
            string query = $"if (not exists(select pKey from Portolo_SecurityGroup where SecurityGroupID = '{groupName}')) begin insert into Portolo_SecurityGroup (SecurityGroupID) values ('{groupName}')  select SCOPE_IDENTITY() end;";
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    row = int.Parse(cmd.ExecuteScalar().ToString());
                    con.Close();
                }
            }
            if (row > 0)
            {
                using (SqlConnection con = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand("AddSecurityPage", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id", row);
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }
                return Json(new HttpStatusCodeResult(System.Net.HttpStatusCode.OK), JsonRequestBehavior.AllowGet);
            }
            return Json(new HttpStatusCodeResult(System.Net.HttpStatusCode.NotFound), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public void RemoveGroup(string[] groups)
        {
            foreach (var group in groups)
            {
                string query = $"delete Portolo_SecurityGroup where pKey = {group}";
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

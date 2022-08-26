using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Metadata;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using Aspose.Cells;
using Elimar.Models;
using fcConferenceManager.Models;
using MAGI_API.Models;
using MAGI_API.Security;
using Microsoft.Ajax.Utilities;

namespace fcConferenceManager.Controllers
{
    public class InterestGroupController : Controller
    {
        readonly string config;

        public InterestGroupController()
        {
            config = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
        }
        public ActionResult CreateGroup(bool? membersPage , string nameSort, int? pageNo, string nameSearch, string organizationSearch,string industry ,string answerFilter,string questionFilter , string groupFilter )
        {
            ViewData["Answer"] = answerFilter;
            ViewBag.Answer = answerFilter;
            answerFilter = !string.IsNullOrEmpty(answerFilter)? answerFilter.Replace('-', ' '):answerFilter;
            membersPage = membersPage != null ? membersPage : false;
            loginResponse objlt = (loginResponse)Session["User"];
            if (objlt == null) return Redirect("~/Account/Portolo");
            ViewBag.memmberPage = membersPage;
            
            if (membersPage == true)
            {
                List<InterestGroup> groups = new List<InterestGroup>();
                string query = $"select distinct top 150  * from Account_List al join Organization_List ol on al.ParentOrganization_pKey = ol.pKey where al.ContactName != '' and al.ContactName like '%{nameSearch}%' and ol.OrganizationID like '%{organizationSearch}%'";

                nameSearch = !string.IsNullOrEmpty(nameSearch) ? nameSearch.Trim() : nameSearch;
                organizationSearch = !string.IsNullOrEmpty(organizationSearch) ? organizationSearch.Trim() : organizationSearch;


                ViewData["NameSortParm"] = String.IsNullOrEmpty(nameSort) ? "name_desc" : "";
                ViewData["NameFilter"] = nameSearch;
                ViewData["OrganizationFilter"] = organizationSearch;
                ViewData["IndustryFilter"] = industry;
                ViewData["Question"] = questionFilter;
                
                ViewData["Group"] = groupFilter;
                ViewData["NameSortParm"] = String.IsNullOrEmpty(nameSort) ? "name_desc" : "";

                if (!string.IsNullOrEmpty(industry) && industry!="0" && !string.IsNullOrEmpty(answerFilter) && answerFilter != "0" && !string.IsNullOrEmpty(groupFilter) && groupFilter!= "0")
                {
                    query = $"select distinct top 150 al.pKey , max(al.ContactName) as contactName, max(al.Title) as title,max(Department) as department,max(ol.OrganizationID) as OrganizationID from Account_List al join Organization_List ol on al.ParentOrganization_pKey = ol.pKey join (select distinct account_pkey , response from ExhibitorFeedbackForm_UserResponse) er on er.Account_pkey = al.pKey join GroupMembers gm on gm.accountPkey = al.pKey where gm.groupPkey = {groupFilter} and al.ContactName != '' and al.Email is not null and ol.organizationType_pkey = {industry} and er.Response like '{answerFilter}' and al.ContactName like '%{nameSearch}%' and ol.OrganizationID like '%{organizationSearch}%' group by al.pKey";
                }
                
                else if(!string.IsNullOrEmpty(industry) && industry != "0" && !string.IsNullOrEmpty(answerFilter) && answerFilter != "0")
                {
                    query = $"select top 150 al.pKey , max(al.ContactName) as contactName, max(al.Title) as title,max(Department) as department,max(ol.OrganizationID) as OrganizationID from Account_List al join Organization_List ol on al.ParentOrganization_pKey = ol.pKey join ExhibitorFeedbackForm_UserResponse er on er.Account_pkey = al.pKey where al.ContactName != '' and al.Email is not null and ol.organizationType_pkey = {industry} and er.Response like '{answerFilter}' and al.ContactName like '%{nameSearch}%' and ol.OrganizationID like '%{organizationSearch}%'  group by al.pKey";
                }
                else if (!string.IsNullOrEmpty(answerFilter) && answerFilter != "0" && !string.IsNullOrEmpty(groupFilter) && groupFilter != "0")
                {
                    query = $"select top 150 al.pKey , max(al.ContactName) as contactName, max(al.Title) as title,max(Department) as department,max(ol.OrganizationID) as OrganizationID from Account_List al join Organization_List ol on al.ParentOrganization_pKey = ol.pKey join ExhibitorFeedbackForm_UserResponse er on er.Account_pkey = al.pKey join GroupMembers gm on gm.accountPkey = al.pKey where gm.groupPkey = {groupFilter} and al.ContactName != '' and al.Email is not null and er.Response like '{answerFilter}' and al.ContactName like '%{nameSearch}%' and ol.OrganizationID like '%{organizationSearch}%' group by al.pKey";
                }
                else if (!string.IsNullOrEmpty(industry) && industry != "0" && !string.IsNullOrEmpty(groupFilter) && groupFilter != "0")
                {
                    query = $"select distinct top 150  * from Account_List al join Organization_List ol on al.ParentOrganization_pKey = ol.pKey join GroupMembers gm on gm.accountPkey = al.pKey where gm.groupPkey = {groupFilter} and al.ContactName != '' and al.Email is not null and ol.organizationType_pkey = {industry} and  al.ContactName like '%{nameSearch}%' and ol.OrganizationID like '%{organizationSearch}%'";
                }
                else if (!string.IsNullOrEmpty(answerFilter) && answerFilter != "0")
                {
                    query = $"select top 150 al.pKey , max(al.ContactName) as contactName, max(al.Title) as title,max(Department) as department,max(ol.OrganizationID) as OrganizationID from Account_List al join Organization_List ol on al.ParentOrganization_pKey = ol.pKey join ExhibitorFeedbackForm_UserResponse er on er.Account_pkey = al.pKey where er.response like '%{answerFilter}%' and al.ContactName != '' and al.Email is not null and al.ContactName like '%{nameSearch}%' and ol.OrganizationID like '%{organizationSearch}%' group by al.pKey";
                }
                else if (!string.IsNullOrEmpty(industry) && industry != "0" )
                {
                    query = $"select distinct top 150 * from Account_List al join Organization_List ol on al.ParentOrganization_pKey = ol.pKey where al.ContactName != '' and al.Email is not null and ol.organizationType_pkey = {industry} and al.ContactName like '%{nameSearch}%' and ol.OrganizationID like '%{organizationSearch}%'";
                }
                else if (!string.IsNullOrEmpty(groupFilter) && groupFilter != "0")
                {
                    query = $"select distinct top 150 * from Account_List al join Organization_List ol on al.ParentOrganization_pKey = ol.pKey join GroupMembers gm on gm.accountPkey = al.pKey where gm.groupPkey = {groupFilter} and al.ContactName != '' and al.Email is not null and  al.ContactName like '%{nameSearch}%' and ol.OrganizationID like '%{organizationSearch}%'";
                }
                if (!string.IsNullOrEmpty(answerFilter) && answerFilter != "0")
                {
                    switch (nameSort)
                    {
                        case "name_desc":
                            query += " order by max(al.contactname) desc";
                            break;
                        default:
                            query += " order by max(al.contactname)";
                            break;
                    }
                }
                else
                {
                    switch (nameSort)
                    {
                        case "name_desc":
                            query += " order by al.contactname desc";
                            break;
                        default:
                            query += " order by al.contactname";
                            break;
                    }
                }
                query += " option(recompile)";

                using (SqlConnection con = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        cmd.CommandTimeout = 0;
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            InterestGroup group = new InterestGroup();
                            UserRequest userRequest = new UserRequest();
                            userRequest.ID = int.Parse(reader["pKey"].ToString());
                            userRequest.name = reader["contactName"].ToString();
                            userRequest.jobTitle = reader["Title"].ToString();
                            userRequest.department = reader["Department"].ToString();
                            userRequest.organization = reader["OrganizationID"].ToString();
                            group.MemberInfo = userRequest;
                            groups.Add(group);
                        }
                    }
                }
                foreach (var item in groups)
                {
                    query = $"select *  from GroupMembers gm join TestGroup tg on gm.groupPkey = tg.pKey where gm.accountPkey = {item.MemberInfo.ID}";
                    item.GroupName = "";
                    item.NoOfMembers = 0;
                    using (SqlConnection con = new SqlConnection(config))
                    {
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            con.Open();
                            SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {                
                                item.GroupName += reader["groupName"].ToString().Trim() + ", ";
                                item.NoOfMembers += int.Parse(reader["noOfMembers"].ToString());
                            }

                        }
                    }
                    if(item.GroupName.Length != 0)item.GroupName =  item.GroupName.Substring(0, item.GroupName.Length - 2);
                }

                //paging
                ViewBag.Pages = 1;
                ViewBag.Page = pageNo == null ? 1 : pageNo;
                ViewBag.firstPage = ViewBag.Page <= 3 ? 1 : ViewBag.Page - 3;
                int count = groups.Count;
                int noOfPages = count % 25 == 0 ? count / 25 : count / 25 + 1;
                if (count < 25) noOfPages = 1;
                ViewBag.lastPage = noOfPages < 5 ? noOfPages : ViewBag.firstPage + 4;
                ViewBag.noOfPage = noOfPages;
                ViewBag.lastPage = ViewBag.lastPage > noOfPages ? noOfPages : ViewBag.lastPage;


                if (count > 25)
                {
                    int start = (int)(pageNo != null ? (pageNo - 1) * 25 : 0);
                    int end = start + 25 > count ? count % 25 : 25;

                    groups = groups.GetRange(start, end);
                    ViewBag.Pages = noOfPages;
                }

                ViewBag.Industry = Industry();
                ViewBag.QuestionFilter = Questions();
                List<SelectListItem> answer = new List<SelectListItem>() {
                    new SelectListItem { Text = "Please Select Question First", Value = "0" }
                };
                ViewBag.AnswerFilter = answer;
                ViewBag.GroupFilter = Group();
                List<SelectListItem> groupdata = Group();
                groupdata.RemoveAt(0);
                ViewBag.GroupData = groupdata;
                return View("~/Views/Portolo/createGroup/CreateGroup.cshtml", groups);
            }
            else
            {
                
                List<InterestGroup> groups = new List<InterestGroup>();
                string query = "select * from testgroup";
                using (SqlConnection con = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            InterestGroup group = new InterestGroup();
                            group.GroupName = reader["groupName"].ToString();
                            group.NoOfMembers = int.Parse(reader["noOfMembers"].ToString());
                            groups.Add(group);
                        }
                    }
                }
                return View("~/Views/Portolo/createGroup/CreateGroup.cshtml", groups);
            }
        }
        public List<SelectListItem> Industry()
        {
            List<SelectListItem> industry = new List<SelectListItem>();
            industry.Add(new SelectListItem { Text = "All", Value = "0", Selected = true });
            string query = "select * from SYS_OrganizationTypes";
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        industry.Add(new SelectListItem{ Text = reader["OrganizationTypeID"].ToString(),Value = reader["pKey"].ToString()});
                    }
                }
            }
            return industry;
        }
        public List<SelectListItem> Questions()
        {
            List<SelectListItem> question = new List<SelectListItem>();
            question.Add(new SelectListItem { Text = "Select Question", Value = "0", Selected = true });
            string query = "select top 8 * from ExhibitorFeedbackForm_Questions where ExhibitorFeedbackForm_pKey = 3 and ResponseOptions is not null order by SortOrder";
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        question.Add(new SelectListItem { Text = reader["Question"].ToString(), Value = reader["pKey"].ToString() });
                    }
                }
            }
            return question;
        }
        public List<SelectListItem> Group()
        {
            List<SelectListItem> groups = new List<SelectListItem>();
            groups.Add(new SelectListItem { Text = "All", Value = "0", Selected = true });
            string query = "select * from testgroup";
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        groups.Add(new SelectListItem { Text = reader["groupName"].ToString(), Value = reader["pKey"].ToString() });
                    }
                }
            }
            return groups;
        }
        public JsonResult Respomse_Bind(string questionID)
        {
            questionID = questionID == null ? "1" : questionID;
            List<SelectListItem> responses = new List<SelectListItem>();
            string query = $"select responseoptions from ExhibitorFeedbackForm_Questions where pKey ={questionID};";
            string response = "" ;
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        response += reader["responseoptions"].ToString();   

                    }
                }
            }
            string[] splits = response.Split(new string[] { "[{*}]" }, StringSplitOptions.None);
            foreach (var item in splits)
            {
                responses.Add(new SelectListItem { Text = item, Value = item.Replace(" ", "-")});
            }
            
            return Json(responses, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public void AddGroup(string name)
        {
            string query = $"IF NOT EXISTS (SELECT * FROM TestGroup WHERE groupName = '{name}' ) Begin insert into TestGroup (groupName,noOfMembers) values ('{name}',0) end;";
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                }
            }
        }
        public void RemoveGroup(string[] groups)
        {
            foreach (string group in groups)
            {
                string query = $"delete testGroup where groupName = '{group.Trim()}'";
                using (SqlConnection con = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                    }
                }
            }
        }
        [HttpPost]
        public ActionResult EditGroup(string previous , string current)
        {
            int row = 1;
            string query = $"IF NOT EXISTS (SELECT * FROM TestGroup WHERE groupName = '{current}' ) Begin update testGroup set groupName = '{current}' where groupName = '{previous}' end; ";
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    row = cmd.ExecuteNonQuery();
                }
            }
            if(row == -1)
            {
                return new HttpStatusCodeResult(400);
            }
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.Accepted);
        }
        public void AddToGroup(string[] accounts, string group)
        {
            foreach (var account in accounts)
            {
                string query = $"IF NOT EXISTS (SELECT * FROM GroupMembers WHERE groupPkey = {group} and accountPkey = {account} ) begin insert into GroupMembers (groupPkey , accountPkey) values ({group},{account}); update TestGroup set noOfMembers += 1 where pKey = {group}; end";
                using (SqlConnection con = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        [HttpPost]        
        public void RemoveMember(string account, string group)
        {
            string query = $"select * from testGroup where groupName = '{group.Trim()}' ";
            int groupPkey = 0;
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        groupPkey = int.Parse(reader["pKey"].ToString());
                    }
                }
            }

            query = $"IF EXISTS (SELECT * FROM GroupMembers WHERE groupPkey = {groupPkey} and accountPkey = {account} ) begin delete GroupMembers where groupPkey = '{groupPkey}' and accountPkey = {account}; update TestGroup set noOfMembers -= 1 where pKey = {groupPkey}; end";
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

}

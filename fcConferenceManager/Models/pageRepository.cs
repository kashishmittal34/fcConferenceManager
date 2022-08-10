using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using fcConferenceManager.Models;
using System.Web.Mvc;

namespace fcConferenceManager.Repository
{

    class pageRepository
    {
        private SqlConnection conn;
        private void connection()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            conn = new SqlConnection(connectionString);
        }

        public bool AddPage(page obj)
        {

            connection();

            SqlCommand cmd = new SqlCommand("AddPageDetails", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Title", obj.Title);
            cmd.Parameters.AddWithValue("@Section", obj.Section);
            cmd.Parameters.AddWithValue("@EventType", obj.EventType);
            cmd.Parameters.AddWithValue("@Status", obj.Status);
            cmd.Parameters.AddWithValue("@EventName", obj.EventName);
            cmd.Parameters.AddWithValue("@URL", obj.URL);
            cmd.Parameters.AddWithValue("@AccessFrom", obj.AccessFrom);
            cmd.Parameters.AddWithValue("@AccessTo", obj.AccessTo);
            cmd.Parameters.AddWithValue("@Notes", obj.Notes);
            cmd.Parameters.AddWithValue("@LinkDocumentation", obj.LinkDocumentation);
            cmd.Parameters.AddWithValue("@newTitle", obj.newTitle);
            cmd.Parameters.AddWithValue("@newURL", obj.newURL);
            cmd.Parameters.AddWithValue("@updated", obj.Updated);
            cmd.Parameters.AddWithValue("@tab", obj.Tab);
            cmd.Parameters.AddWithValue("@users", obj.Users);
            cmd.Parameters.AddWithValue("@pageType", obj.PageType);
            cmd.Parameters.AddWithValue("@userReq", obj.UserReq);
            cmd.Parameters.AddWithValue("@ques", obj.ques);
            conn.Open();
            int i = cmd.ExecuteNonQuery();
            conn.Close();

            if (i >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public List<page> getPageDetailForExport(string Name, string URL, string Status, string EventType, string SortOrder)
        {

            {
                List<page> pageList = new List<page>();



                connection();
                SqlCommand cmd;
                cmd = new SqlCommand("GetPageDetails", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Name", Name);
                cmd.Parameters.AddWithValue("@URL", URL);
                cmd.Parameters.AddWithValue("@Status", Status);
                cmd.Parameters.AddWithValue("@EventType", EventType);
                string SortDirection = "A";
                string x = SortOrder.Substring(SortOrder.Length - 4, 4);
                if (x == "Desc")
                {

                    SortDirection = "D";
                    SortOrder = SortOrder.Substring(0, SortOrder.Length - 4);
                }
                cmd.Parameters.AddWithValue("@SortDirection", SortDirection);
                cmd.Parameters.AddWithValue("@SortOrder", SortOrder);

                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                conn.Open();
                sda.Fill(dt);
                conn.Close();

                foreach (DataRow dr in dt.Rows)
                {

                    pageList.Add(
                        new page
                        {
                            Id = Convert.ToInt32(dr["pageId"]),
                            Title = Convert.ToString(dr["pageTitle"]),
                            newTitle = Convert.ToString(dr["pageNewName"]),
                            Section = Convert.ToString(dr["SectionID"]),
                            EventType = Convert.ToString(dr["EventTypeID"]),
                            Status = Convert.ToString(dr["StatusID"]),
                            EventName = Convert.ToString(dr["pageEventName"]),
                            URL = Convert.ToString(dr["pageURL"]),
                            newURL = Convert.ToString(dr["pageNewURL"]),
                            AccessFrom = Convert.ToString(dr["pageAccessFrom"]),
                            AccessTo = Convert.ToString(dr["pageAccessTo"]),
                            Notes = Convert.ToString(dr["pageNotes"]),
                            LinkDocumentation = Convert.ToString(dr["pageLinkDocumentation"]),
                            Users = Convert.ToString(dr["Users"]),
                            Updated = Convert.ToString(dr["Updated"]),
                            Tab = Convert.ToString(dr["Tab"]),
                            PageType = Convert.ToString(dr["PageType"]),
                            UserReq = Convert.ToString(dr["UsersReq"]),
                            ques = Convert.ToString(dr["Ques"])
                        }
                    );
                }

                return pageList;
            }
        }

        public List<page> getDetailFilterByNameURLStatusEventType(string Name, string URL, string Status, string EventType, string SortOrder)
        {
            List<page> pageList = new List<page>();
            connection();
            SqlCommand cmd;
            cmd = new SqlCommand("GetPageDetails", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Name", Name);
            cmd.Parameters.AddWithValue("@URL", URL);
            cmd.Parameters.AddWithValue("@Status", Status);
            cmd.Parameters.AddWithValue("@EventType", EventType);
            string SortDirection = "A";
            string x = SortOrder.Substring(SortOrder.Length - 4, 4);
            if (x == "Desc")
            {


                SortDirection = "D";
                SortOrder = SortOrder.Substring(0, SortOrder.Length - 4);
            }
            cmd.Parameters.AddWithValue("@SortDirection", SortDirection);
            cmd.Parameters.AddWithValue("@SortOrder", SortOrder);

            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            conn.Open();
            sda.Fill(dt);
            conn.Close();

            foreach (DataRow dr in dt.Rows)
            {
                pageList.Add(
                    new page
                    {
                        Id = Convert.ToInt32(dr["pageId"]),
                        Title = Convert.ToString(dr["pageTitle"]),
                        newTitle = Convert.ToString(dr["pageNewName"]),
                        Section = Convert.ToString(dr["SectionID"]),
                        EventType = Convert.ToString(dr["EventTypeID"]),
                        Status = Convert.ToString(dr["StatusID"]),
                        EventName = Convert.ToString(dr["pageEventName"]),
                        URL = Convert.ToString(dr["pageURL"]),
                        newURL = Convert.ToString(dr["pageNewURL"]),
                        AccessFrom = Convert.ToString(dr["pageAccessFrom"]),
                        AccessTo = Convert.ToString(dr["pageAccessTo"]),
                        Notes = Convert.ToString(dr["pageNotes"]),
                        LinkDocumentation = Convert.ToString(dr["pageLinkDocumentation"]),
                        Users = Convert.ToString(dr["Users"]),
                        Updated = Convert.ToString(dr["Updated"]),
                        Tab = Convert.ToString(dr["Tab"]),
                        PageType = Convert.ToString(dr["PageType"]),
                        UserReq = Convert.ToString(dr["UsersReq"]),
                        ques = Convert.ToString(dr["Ques"])

                    }
                );
            }

            return pageList;


        }


        public List<page> getPagesDetail(string SortOrder)
        {
            List<page> pageList = new List<page>();

            connection();
            SqlCommand cmd = new SqlCommand("GetPageDetails", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            string SortDirection = "A";
            string x = SortOrder.Substring(SortOrder.Length - 4, 4);
            if (x == "Desc")
            {

                SortDirection = "D";
                SortOrder = SortOrder.Substring(0, SortOrder.Length - 4);
            }
            cmd.Parameters.AddWithValue("@SortDirection", SortDirection);
            cmd.Parameters.AddWithValue("@SortOrder", SortOrder);

            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            conn.Open();
            sda.Fill(dt);
            conn.Close();

            foreach (DataRow dr in dt.Rows)
            {
                pageList.Add(
                    new page
                    {
                        Id = Convert.ToInt32(dr["pageId"]),
                        Title = Convert.ToString(dr["pageTitle"]),
                        newTitle = Convert.ToString(dr["pageNewName"]),
                        Section = Convert.ToString(dr["pageSection"]),
                        EventType = Convert.ToString(dr["pageEventType"]),
                        Status = Convert.ToString(dr["pageStatus"]),
                        EventName = Convert.ToString(dr["pageEventName"]),
                        URL = Convert.ToString(dr["pageURL"]),
                        newURL = Convert.ToString(dr["pageNewURL"]),
                        AccessFrom = Convert.ToString(dr["pageAccessFrom"]),
                        AccessTo = Convert.ToString(dr["pageAccessTo"]),
                        Notes = Convert.ToString(dr["pageNotes"]),
                        LinkDocumentation = Convert.ToString(dr["pageLinkDocumentation"]),
                        Users = Convert.ToString(dr["Users"]),
                        Updated = Convert.ToString(dr["Updated"]),
                        Tab = Convert.ToString(dr["Tab"]),
                        PageType = Convert.ToString(dr["PageType"]),
                        UserReq = Convert.ToString(dr["UsersReq"]),
                        ques = Convert.ToString(dr["Ques"])

                    }
                    );
            }

            return pageList;
        }


        public bool updatePage(page obj)
        {
            connection();
            SqlCommand cmd = new SqlCommand("UpdatePageDetails", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", obj.Id);
            cmd.Parameters.AddWithValue("@Title", obj.Title);
            cmd.Parameters.AddWithValue("@Section", obj.Section);
            cmd.Parameters.AddWithValue("@EventType", obj.EventType);
            cmd.Parameters.AddWithValue("@Status", obj.Status);
            cmd.Parameters.AddWithValue("@EventName", obj.EventName);
            cmd.Parameters.AddWithValue("@URL", obj.URL);
            cmd.Parameters.AddWithValue("@AccessFrom", obj.AccessFrom);
            cmd.Parameters.AddWithValue("@AccessTo", obj.AccessTo);
            cmd.Parameters.AddWithValue("@Notes", obj.Notes);
            cmd.Parameters.AddWithValue("@LinkDocumentation", obj.LinkDocumentation);
            cmd.Parameters.AddWithValue("@newTitle", obj.newTitle);
            cmd.Parameters.AddWithValue("@newURL", obj.newURL);
            cmd.Parameters.AddWithValue("@updated", obj.Updated);
            cmd.Parameters.AddWithValue("@tab", obj.Tab);
            cmd.Parameters.AddWithValue("@users", obj.Users);
            cmd.Parameters.AddWithValue("@pageType", obj.PageType);
            cmd.Parameters.AddWithValue("@userReq", obj.UserReq);
            cmd.Parameters.AddWithValue("@ques", obj.ques);

            conn.Open();
            int i = cmd.ExecuteNonQuery();
            conn.Close();
            if (i >= 1)
            {
                return true;

            }
            else { return false; }

        }


        public bool DeletePage(int id)
        {
            connection();

            SqlCommand cmd = new SqlCommand("DeletePageDetails", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            int i = cmd.ExecuteNonQuery();
            conn.Close();
            if (i >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<SelectListItem> getDropDown()
        {
            connection();
            SqlCommand cmd = new SqlCommand("SELECT pageTitle from PageInfo order by pageTitle", conn);
            List<SelectListItem> list = new List<SelectListItem>();
            conn.Open();
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            conn.Close();
            list.Add(new SelectListItem() { Text = "--Select--", Value = "" });
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new SelectListItem() { Text = Convert.ToString(dr["pageTitle"]), Value = Convert.ToString(dr["pageTitle"]) });
            }

            return list;

        }

        public List<SelectListItem> getDropDownStatus()
        {
            connection();
            SqlCommand cmd = new SqlCommand("SELECT * from SYS_Status order by StatusID", conn);
            List<SelectListItem> list = new List<SelectListItem>();
            conn.Open();
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            conn.Close();
            list.Add(new SelectListItem() { Text = "--Select Status--", Value = "" });
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new SelectListItem() { Text = Convert.ToString(dr["StatusID"]), Value = Convert.ToString(dr["pKey"]) });
            }

            return list;
        }

        public List<SelectListItem> getDropDownEventType()
        {
            connection();
            SqlCommand cmd = new SqlCommand("SELECT * from SYS_PageEventTypes order by EventTypeID", conn);
            List<SelectListItem> list = new List<SelectListItem>();
            conn.Open();
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            conn.Close();
            list.Add(new SelectListItem() { Text = "--Select Event Type--", Value = "" });
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new SelectListItem() { Text = Convert.ToString(dr["EventTypeID"]), Value = Convert.ToString(dr["pKey"]) });
            }
            return list;
        }
        public List<SelectListItem> getDropDownSection()
        {
            connection();
            SqlCommand cmd = new SqlCommand("SELECT * from SYS_PageSection order by SectionID", conn);
            List<SelectListItem> list = new List<SelectListItem>();
            conn.Open();
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            conn.Close();
            list.Add(new SelectListItem() { Text = "--Select Section--", Value = "" });
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new SelectListItem() { Text = Convert.ToString(dr["SectionID"]), Value = Convert.ToString(dr["pKey"]) });
            }

            return list;
        }

        public bool BatchUpdateStatus(int id, string status)
        {
            connection();
            SqlCommand cmd = new SqlCommand("update pageinfo set pageStatus = @status where pageId = @id", conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@status", status);

            conn.Open();
            int i = cmd.ExecuteNonQuery();
            conn.Close();
            if (i >= 1)
            {
                return true;

            }
            else { return false; }

        }

        public bool BatchUpdateSection(int id, string section)
        {
            connection();
            SqlCommand cmd = new SqlCommand("update pageinfo set pageSection = @section where pageId = @id", conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@section", section);

            conn.Open();
            int i = cmd.ExecuteNonQuery();
            conn.Close();
            if (i >= 1)
            {
                return true;

            }
            else { return false; }

        }
    }
}
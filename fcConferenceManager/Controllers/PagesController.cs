using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using fcConferenceManager.Models;
using PagedList;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;
using System.Data.OleDb;
using System.Data;
using System.Data.SqlClient;
using fcConferenceManager.Repository;
using OfficeOpenXml;

namespace fcConferenceManager.Controllers
{
    public class pagesController : Controller
    {
        public List<page> data(string FilterName, string FilterURL, string FilterStatus, string FilterEventType,string FilterType, string SortOrder = "Title")
        {
            pageRepository pgrepo = new pageRepository();
            return pgrepo.getDetailFilterByNameURLStatusEventType(FilterName, FilterURL, FilterStatus, FilterEventType, FilterType, SortOrder);

        }
        public ActionResult getAllPageDetails(string FilterName, string FilterURL, string FilterStatus,string FilterEventType,string FilterType, int? page, string SortOrder = "Title")
        {
            clsAccount cAccount = ((clsAccount)Session["cAccount"]);

            if (cAccount.intAccount_PKey <= 0)
            {

                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (FilterName != null)
                {
                    FilterName = FilterName.Trim();
                }
                if (FilterName != null)
                {
                    FilterURL = FilterURL.Trim();
                }

                if (FilterName == "")
                {
                    FilterName = null;
                }
                if (FilterURL == "")
                {
                    FilterURL = null;
                }
                if (FilterStatus == "")
                {
                    FilterStatus = null;
                }
                if (FilterEventType == "")
                {
                    FilterEventType = null;
                }
                if(FilterType == "")
            {
                FilterType = null;
            }

                ViewBag.FilterSortOrder = SortOrder;

                ViewBag.FilterName = FilterName;
                ViewBag.FilterURL = FilterURL;
                ViewBag.FilterStatus = FilterStatus;
                ViewBag.FilterEventType = FilterEventType;
                ViewBag.FilterType = FilterType;
                int pageSize = 25;

                int pageIndex = (page ?? 1);

                pageRepository pgrepo = new pageRepository();
                ViewBag.getDropDownStatus = pgrepo.getDropDownStatus();
                ViewBag.getDropDownEventType = pgrepo.getDropDownEventType();
                ViewBag.getDropDownSection = pgrepo.getDropDownSection();

                var data = this.data(FilterName, FilterURL, FilterStatus, FilterEventType, FilterType, SortOrder);
                ViewBag.data = data;
                return View(data.ToPagedList(pageIndex, pageSize));
        }
    }

        [HttpPost]
        public ActionResult ExcelImport(HttpPostedFileBase file)
        {
            string fileExtension = System.IO.Path.GetExtension(Request.Files["file"].FileName);
            
            System.Data.DataSet ds = new System.Data.DataSet();
            if (Request.Files["file"].ContentLength > 0)
            {
                
                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    string fileLocation = Server.MapPath("~/Content/") + Request.Files["file"].FileName;
                    if (System.IO.File.Exists(fileLocation))
                    {
                        System.IO.File.Delete(fileLocation);
                    }
                    Request.Files["file"].SaveAs(fileLocation);
                    string excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    

                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Open();
                    DataTable dt = new DataTable();
                    dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                    if (dt == null)
                    {
                        return null;
                    }
                    
                    

                    String[] excelSheets = new String[dt.Rows.Count];
                    int t = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[t] = row["TABLE_NAME"].ToString();
                        t++;
                    }
                    OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);

                    string query = string.Format("Select * from [{0}]", excelSheets[0]);
                    using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                    {
                        dataAdapter.Fill(ds);
                    }

                    excelConnection.Close();

                    if (ds.Tables[0].Rows.Count == 0)
                    {
                        return new ContentResult() { Content = "<script language='javascript' type='text/javascript'>alert('File Have No Entries!!!');window.location.href='/Pages/getAllPageDetails';</script>" };   
                    }
                    
                    
                    if (ds.Tables[0].Columns.Count != 11)
                    {

                        return new ContentResult() { Content = "<script language='javascript' type='text/javascript'>alert('Format of file Mismatch!!!!');window.location.href='/Pages/getAllPageDetails';</script>" };
                    }

                    
                    else
                    {
                        try {
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["myDbConnection"].ConnectionString;
                                SqlConnection conn = new SqlConnection(connectionString);
                                SqlCommand cmd = new SqlCommand("AddPageDetails", conn);
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@Title", ds.Tables[0].Rows[i][0].ToString());
                                cmd.Parameters.AddWithValue("@LinkName", ds.Tables[0].Rows[i][1].ToString());
                                cmd.Parameters.AddWithValue("@Section", ds.Tables[0].Rows[i][2].ToString());
                                cmd.Parameters.AddWithValue("@EventType", ds.Tables[0].Rows[i][3].ToString());
                                cmd.Parameters.AddWithValue("@Status", ds.Tables[0].Rows[i][4].ToString());
                                cmd.Parameters.AddWithValue("@EventName", ds.Tables[0].Rows[i][5].ToString());
                                cmd.Parameters.AddWithValue("@URL", ds.Tables[0].Rows[i][6].ToString());
                                cmd.Parameters.AddWithValue("@AccessFrom", ds.Tables[0].Rows[i][7].ToString());
                                cmd.Parameters.AddWithValue("@AccessTo", ds.Tables[0].Rows[i][8].ToString());
                                cmd.Parameters.AddWithValue("@Notes", ds.Tables[0].Rows[i][9].ToString());
                                cmd.Parameters.AddWithValue("@LinkDocumentation", ds.Tables[0].Rows[i][10].ToString());
                                conn.Open();
                                cmd.ExecuteNonQuery();
                                conn.Close();
                            }
                        }
                        catch
                        {
                            return new ContentResult() { Content = "<script language='javascript' type='text/javascript'>alert('Format of file Mismatch!!!!');window.location.href='/Pages/getAllPageDetails';</script>" };
                        }
                        
                    }
                    
                }
                


            }

                
                return RedirectToAction("getAllPageDetails");
        }


        public ActionResult ExcelExport(string FilterName, string FilterURL, string FilterStatus, string FilterEventType,string FilterType, string SortOrder = "Title")
        {
            var gv = new GridView();
            pageRepository pgrepo = new pageRepository();
            
            var data = pgrepo.getPageDetailForExport(FilterName, FilterURL, FilterStatus, FilterEventType, FilterType, SortOrder);
            if(data.Count == 0)
            {
                data.Add(new page());
            }
            gv.DataSource = data;
            gv.DataBind();

            gv.HeaderRow.Cells[0].Visible = false;

            // Loop through the rows and hide the cell in the first column
            for (int i = 0; i < gv.Rows.Count; i++)
            {
                GridViewRow row = gv.Rows[i];
                row.Cells[0].Visible = false;
            }
											 

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.Cells[1, 1].LoadFromCollection(data, true);
            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //here i have set filname as PageDetails.xlsx
                Response.AddHeader("content-disposition", "attachment;  filename=PageDetails.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
            return RedirectToAction("getAllPageDetails");

        }

        public ActionResult addPageDetails()
        {

            pageRepository pgrepo = new pageRepository();
            ViewBag.getDropDown = pgrepo.getDropDown();
            ViewBag.getDropDownStatus = pgrepo.getDropDownStatus();
            ViewBag.getDropDownEventType = pgrepo.getDropDownEventType();
            ViewBag.getDropDownSection= pgrepo.getDropDownSection();
            return View();
        }

        [HttpPost]
        public ActionResult addPageDetails(page pg)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    pageRepository pgrepo = new pageRepository();
                    if (pgrepo.AddPage(pg))
                    {
                        return RedirectToAction("getAllPageDetails");
                    }
                }
                return View();
            }
            catch
            {
                return View();
            }
        }

        public ActionResult updatePageDetails(int id, string SortOrder = "Title")
        {
            pageRepository pgrepo = new pageRepository();
            ViewBag.getDropDown = pgrepo.getDropDown();
            ViewBag.getDropDownStatus = pgrepo.getDropDownStatus();
            ViewBag.getDropDownEventType = pgrepo.getDropDownEventType();
            ViewBag.getDropDownSection= pgrepo.getDropDownSection();
            return View(pgrepo.getPagesDetail(SortOrder).Find(page => page.Id== id));
        }

        [HttpPost]
        public ActionResult updatePageDetails(int id, page obj)
        {
            try
            {
                pageRepository pgrepo = new pageRepository();
                ViewBag.getDropDown = pgrepo.getDropDown();
                ViewBag.getDropDownStatus = pgrepo.getDropDownStatus();
                ViewBag.getDropDownEventType = pgrepo.getDropDownEventType();
                ViewBag.getDropDownSection = pgrepo.getDropDownSection();
                pgrepo.updatePage(obj);
                return new ContentResult() { Content = "<script language='javascript' type='text/javascript'>alert('Page Detail Updated !!!');window.location.href='/Pages/getAllPageDetails';</script>" };
                
            }
            catch
            {
                return new ContentResult() { Content = "<script language='javascript' type='text/javascript'>alert('Data not Updated!!!');window.location.href = '/Pages/updatePageDetails/"+id+"';</script>" };
                
            }
        }

        public ActionResult deletePage(int id)
        {
            try
            {
                pageRepository pgrepo = new pageRepository();
                if (pgrepo.DeletePage(id))
                {
                    ViewBag.AlertMsg = "Page Details Deleted successfully !!!!";
                }
                return RedirectToAction("getAllPageDetails");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult BatchUpdateStatus(FormCollection formCollection)
        {
            if (formCollection.Count <= 3)
            {
                return new ContentResult() { Content = "<script language='javascript' type='text/javascript'>alert('No Row Selected!!!');window.location.href='/Pages/getAllPageDetails';</script>" };
            }
            var y = formCollection[3];
            var x = formCollection[2].Split(',').ToList();

            pageRepository pgrepo = new pageRepository();
            if(y == "Update")
            {
                if (formCollection[0] != "")
                {
                    foreach (var item in x)
                    {
                        if (item != "false")
                        {
                            pgrepo.BatchUpdateStatus(Convert.ToInt32(item), formCollection[0]);
                        }
                    }
                }
                if (formCollection[1].Trim() != "")
                {
                    foreach (var item in x)
                    {
                        if (item != "false")
                        {
                            pgrepo.BatchUpdateSection(Convert.ToInt32(item), formCollection[1]);
                        }
                    }
                }
                if (formCollection[0] == "" && formCollection[1].Trim() == "")
                {
                    return new ContentResult() { Content = "<script language='javascript' type='text/javascript'>alert('No Entry in Status or Section!!!');window.location.href='/Pages/getAllPageDetails';</script>" };
                }

            }
            else
            {
                foreach (var item in x)
                {
                    if (item != "false")
                    {
                        pgrepo.DeletePage(Convert.ToInt32(item));
                    }
                }
            }

            return RedirectToAction("getAllPageDetails");
        }

        public ActionResult BatchDeletePages(FormCollection formCollection)
        {
            if (formCollection.Count <= 2)
            {
                return new ContentResult() { Content = "<script language='javascript' type='text/javascript'>alert('No Row Selected!!!');window.location.href='/Pages/getAllPageDetails';</script>" };
            }

            return RedirectToAction("getAllPageDetails");

        }
        

    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using ExcelDataReader;
using fcConferenceManager.Models.Portolo;

namespace fcConferenceManager.Controllers.Portolo
{
    public class ProductController : Controller
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString);
        // GET: Product
        public ActionResult ProductList()
        {
            if (Session["User"] == null) return Redirect("~/Account/Portolo");

            DataTable dt = new DataTable();

            string dbquery = " select * from Portolo_ProductList";
            con.Open();
            SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
            _da.Fill(dt);
            con.Close();

            ViewBag.Products = dt;
            ViewBag.InvalidExcel = TempData["InvalidExcel"];
            return View("~/Views/Portolo/Product/Index.cshtml");
        }

        public ActionResult AddProduct(Product product)
        {
            if (Session["User"] == null) return Redirect("~/Account/Portolo");

            if (product.Name == null)
                return View("~/Views/Portolo/Product/AddProduct.cshtml");

            string query = "";

            if (product.Id == 0)
            {
                query = String.Format("INSERT INTO Portolo_ProductList VALUES ('{0}', '{1}', '{2}')", product.Name, product.description, product.Price);
            }
            else
                query = String.Format("Update Portolo_ProductList Set ProductName = '{0}', Description = '{1}', Price = '{2}' where ProductId = {3}", product.Name, product.description, product.Price, product.Id);

            SqlCommand cmd = new SqlCommand(query, con);

            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

            return RedirectToAction("ProductList", "Product");
        }

        public ActionResult Edit(int id)
        {
            if (Session["User"] == null) return Redirect("~/Account/Portolo");

            string dbquery = "select * from Portolo_ProductList where ProductId = " + id;
            con.Open();

            SqlCommand cmd = new SqlCommand(dbquery, con);
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            Product product = new Product();
            product.Name = reader["ProductName"].ToString();
            product.description = reader["Description"].ToString();
            product.Id = id;
            product.Price = (decimal)reader["Price"];
            con.Close();

            return View("~/Views/Portolo/Product/AddProduct.cshtml", product);
        }


        [HttpPost]
        public ActionResult Delete(string ids)
        {
            if (Session["User"] == null) return Redirect("~/Account/Portolo");

            string dbquery = String.Format("delete from Portolo_ProductList where ProductId in ({0}) ", ids);

            con.Open();
            SqlCommand cmd = new SqlCommand(dbquery, con);
            cmd.ExecuteNonQuery();
            con.Close();

            return RedirectToAction("ProductList", "Product");
        }

        [HttpGet]
         public ActionResult SearchProduct(string name, string description, string search)
        {
            DataTable dt = new DataTable();

            string dbquery = String.Format("select * from Portolo_ProductList where ProductName like '%{0}%' and Description like '%{1}%'", name.Trim(), description.Trim());

            con.Open();
            SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
            _da.Fill(dt);
            con.Close();

            ViewBag.Products = dt;

            if (search != "true")
            {
                string FileName = String.Format("Products_{0:yyMMdd_HH.mm}", DateTime.Now);
                ExportToExcel(dt, FileName);
            }

            return View("~/Views/Portolo/Product/Index.cshtml");

        }

        private void ExportToExcel(DataTable dt, string fileName)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt, "Product_List");
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
        #region ImportExcel

        [HttpPost]

        public ActionResult ImportFromExcel(HttpPostedFileBase files)
        {
            if (ModelState.IsValid)
            {
                if (files != null && files.ContentLength > 0)
                {
                    Stream stream = files.InputStream;
                    IExcelDataReader reader = null;
                    if (files.FileName.EndsWith(".xls"))
                    {
                        string filename = files.FileName;
                        string path = Server.MapPath("~/PortoloDocuments/" + filename);
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                        files.SaveAs(path);

                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }
                    else if (files.FileName.EndsWith(".xlsx"))
                    {
                        string filename = files.FileName;
                        string path = Server.MapPath("~/PortoloDocuments/" + filename);
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                        files.SaveAs(path);
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    }
                    else
                    {
                        ModelState.AddModelError("File", "This file format is not supported");
                        return RedirectToAction("ProductList", "Product");
                    }
                    int fieldCount = reader.FieldCount;
                    int rowCount = reader.RowCount;
                    DataTable dt = new DataTable();
                    DataRow row;
                    DataTable dt_ = new DataTable();
                    DataSet ds = new DataSet();
                    ds = reader.AsDataSet();
                    try
                    {

                        dt_ = ds.Tables[0];

                        for (int i = 0; i < dt_.Columns.Count; i++)
                        {
                            dt.Columns.Add(dt_.Rows[0][i].ToString());
                        }
                        int rowCounter = 0;
                        for (int rows = 1; rows < dt_.Rows.Count; rows++)
                        {
                            row = dt.NewRow();
                            for (int col = 0; col < dt_.Columns.Count; col++)
                            {
                                row[col] = dt_.Rows[rows][col].ToString();
                                rowCounter++;
                            }
                            dt.Rows.Add(row);
                        }
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("File", "Unable to upload file");
                        return RedirectToAction("ProductList", "Product");
                    }
                    DataSet result = new DataSet();
                    result.Tables.Add(dt);
                    reader.Close();
                    reader.Dispose();
                    DataTable dataTable = result.Tables[0];

                   
                    for (int i = 0; i < result.Tables[0].Rows.Count; i++)
                    {
                        try
                        {
                            string numString = result.Tables[0].Rows[i][3].ToString(); //"1287543.0" will return false for a long
                            int number1 = 0;
                            bool canConvert = int.TryParse(numString, out number1);
                            if (canConvert == true)
                            {
                                string conn = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
                                SqlConnection con = new SqlConnection(conn);
                                string query = "Insert into Portolo_ProductList(ProductName,Description,Price) Values('" + result.Tables[0].Rows[i][1].ToString() + "','" + result.Tables[0].Rows[i][2].ToString() + "','" + result.Tables[0].Rows[i][3].ToString() + "')";
                                con.Open();
                                SqlCommand cmd = new SqlCommand(query, con);
                                cmd.ExecuteNonQuery();
                                con.Close();
                            }
                            else
                            {
                                TempData["InvalidExcel"] = "Invalid! Excel File";
                                break;
                            }
                        }
                        catch(Exception e)
                        {
                            TempData["InvalidExcel"] = "Invalid Excel File!" + e.Message ;
                        }
                    }
                    
                    return RedirectToAction("ProductList", "Product");
                }
                else
                {
                    ModelState.AddModelError("File", "Please upload your file");
                }

            }
            return RedirectToAction("ProductList", "Product");
        }
        #endregion
    }
}

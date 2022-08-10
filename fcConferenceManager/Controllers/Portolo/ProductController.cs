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
        public ActionResult SearchProduct(string name, string description, string excel)
        {
            DataTable dt = new DataTable();

            string dbquery = String.Format("select * from Portolo_ProductList where ProductName like '%{0}%' and Description like '%{1}%'", name.Trim(), description.Trim());

            con.Open();
            SqlDataAdapter _da = new SqlDataAdapter(dbquery, con);
            _da.Fill(dt);
            con.Close();

            ViewBag.Products = dt;

            if (excel == "true")
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
    }
}

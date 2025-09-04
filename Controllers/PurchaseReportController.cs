using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Web.UI.WebControls;
using NPOI.HSSF.UserModel;
using System.IO;
using Rotativa;//----------------------For PDF
using NPOI.SS.UserModel;//-------------For Excel
using System.Net.Mail;
using System.Data.Entity.Infrastructure;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity.Validation;
using System.Collections;
using System.Globalization;
using Rotativa.Options;
using System.Data.Objects;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace XenERP.Controllers
{
    public class PurchaseReportController : Controller
    {
        InventoryEntities db = new InventoryEntities();

        XenERP.Models.Repository.TaxRepository rep = new Models.Repository.TaxRepository();


        public ActionResult Index()
        {
            return View();
        }

        #region ----Supplier Wise Purchase Stock Report --------------


        public ActionResult PurchaseSupplierStock()
        {
            return View();


        }

        [HttpGet]
        public ActionResult SupplierPurchaseStockReport(string from, string to, int id)
        {

            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? FromDate = null;
            DateTime? ToDate = null;
            Session["fdate"] = from.ToString();
            Session["tdate"] = to.ToString();
            if (from != "")
            {
                FromDate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            if (to != "")
            {
                ToDate = DateTime.ParseExact(to, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }


            Session["Custid"] = id.ToString();

            var detls = db.PurchaseInvoiceDetails.Where(d => d.PurchaseInvoice.InvoiceDate >= FromDate && d.PurchaseInvoice.InvoiceDate <= ToDate && d.PurchaseInvoice.SupplierId == id).ToList();

            return PartialView(detls);


        }

        #endregion
        #region ---- Purchase By Supplier--------------


        public ActionResult PurchaseSupplier()
        {
            return View();


        }

        [HttpGet]
        public ActionResult SupplierPurchaseReport(DateTime from, DateTime to, int id)
        {

            Session["fdate"] = from;
            Session["tdate"] = to;

            Session["Custid"] = id.ToString();

            var detls = db.PurchaseInvoices.Where(d => d.Date >= from && d.Date <= to && d.SupplierId == id).ToList();

            return PartialView(detls);


        }


        [HttpPost]
        public JsonResult getSupplier(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            var customers = db.Suppliers.Where(p => p.Name.Contains(query) && p.CompanyId == companyid).Select(p => new { Name = p.Name, Id = p.Id }).ToList();
            return Json(customers, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getSelectedSupplier(int id)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            var suppliers = db.Suppliers.Where(p => p.Id == id && p.CompanyId == companyid && p.BranchId == Branchid).Select(p => new { Name = p.Name, Id = p.Id }).FirstOrDefault();
            return Json(suppliers, JsonRequestBehavior.AllowGet);
        }




        [HttpGet]
        public ActionResult PurchasebySupplierPrint(int id)
        {
            int compid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);


            var detls = db.Companies.Where(c => c.Id == compid).FirstOrDefault();

            ViewBag.Name = detls.Name;
            ViewBag.address = detls.Address;
            ViewBag.pin = detls.Zipcode;

            try
            {
                var branch = db.BranchMasters.Where(c => c.Id == Branchid).FirstOrDefault();
                ViewBag.branchname = branch.Name;
            }
            catch { }



            // string date = "";



            string from = Session["fdate"].ToString();
            string to = Session["tdate"].ToString();

            DateTime fromDate = DateTime.Parse(Convert.ToString(from));
            DateTime toDate = DateTime.Parse(Convert.ToString(to));


            ViewBag.from = fromDate;
            ViewBag.to = toDate;


            //Get the data representing the current grid state - page, sort and filter

            var customerdetls = db.PurchaseInvoices.Where(d => d.Date >= fromDate && d.Date <= toDate && d.SupplierId == id).ToList();

            return View(customerdetls);




        }


        [HttpGet]
        public ActionResult PurchasebySupplierPDF(int id, int value, string from, string to)
        {

            int compid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);


            var detls = db.Companies.Where(c => c.Id == value).FirstOrDefault();

            ViewBag.Name = detls.Name;
            ViewBag.address = detls.Address;
            ViewBag.pin = detls.Zipcode;

            try
            {
                var branch = db.BranchMasters.Where(c => c.Id == Branchid).FirstOrDefault();
                ViewBag.branchname = branch.Name;
            }
            catch { }





            DateTime fromDate = DateTime.Parse(Convert.ToString(from));
            DateTime toDate = DateTime.Parse(Convert.ToString(to));


            ViewBag.from = fromDate;
            ViewBag.to = toDate;


            //Get the data representing the current grid state - page, sort and filter

            var customerdetls = db.PurchaseInvoices.Where(d => d.Date >= fromDate && d.Date <= toDate && d.SupplierId == id).ToList();


            return View(customerdetls);




        }



        [HttpGet]
        public ActionResult PurchasebySupplierPDFlink()
        {
            int custid = Convert.ToInt32(Session["Custid"]);

            int compid = Convert.ToInt32(Session["companyid"]);

            try
            {
                string from = Session["fdate"].ToString();
                string to = Session["tdate"].ToString();
                return new ActionAsPdf("PurchasebySupplierPDF", new { id = custid, value = compid, from = from, to = to }) { FileName = "PurchasebySupplier.pdf" };
            }
            catch
            {
                return new ActionAsPdf("PurchasebySupplierPDF", new { id = custid, value = compid, from = 0, to = 0 }) { FileName = "PurchasebySupplier.pdf" };
            }

        }






        #endregion




        #region ---- Purchase By Product--------------


        public ActionResult PurchaseProduct()
        {
            return View();


        }

        [HttpPost]
        public JsonResult getProduct(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            var customers = db.Products.Where(p => p.Name.Contains(query) &&  p.companyid == companyid).Select(p => new { Name = p.Name, Id = p.Id }).ToList();
            return Json(customers, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getSelectedProduct(int id)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            var suppliers = db.Products.Where(p => p.Id == id).Select(p => new { Name = p.Name, Id = p.Id }).FirstOrDefault();
            return Json(suppliers, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]

        public ActionResult PurchaseProductReport(int id, string from, string to)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);

            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? FromDate = null;
            DateTime? ToDate = null;
            Session["fdate"] = from.ToString();
            Session["tdate"] = to.ToString();
            if (from != "")
            {
                FromDate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            if (to != "")
            {
                ToDate = DateTime.ParseExact(to, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }

            Session["productid"] = id.ToString();
            if (id != 0)
            {

                List<PurchaseProduct_Result> sprList = new List<PurchaseProduct_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal quantitytotal = 0;
                decimal amounttotal = 0;
                var result = db.PurchaseProduct(id, FromDate, ToDate, companyid, Branchid).ToList();
                if (result.Count != 0)
                {


                    foreach (var row in result)
                    {
                        if (oldproduct != row.ItemName)
                        {
                            counter = 0;
                            quantitytotal = result.Where(r => r.ItemName == row.ItemName).Sum(r => r.Quantity);
                            amounttotal = result.Where(r => r.ItemName == row.ItemName).Sum(r => r.TotalAmount);
                            productcount = result.Where(r => r.ItemName == row.ItemName).Count() - 1;
                            if (counter == productcount)
                            {
                                if (counter == 0)
                                {
                                    var spr = new PurchaseProduct_Result();
                                    spr.ItemName = row.ItemName;
                                    spr.SupplierName = "Header";
                                    sprList.Add(spr);
                                    var spr1 = new PurchaseProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.SupplierName = row.SupplierName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    spr1.Rate = row.Rate;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchaseProduct_Result();
                                    spr2.InvoiceNO = "Grand Total:";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new PurchaseProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.SupplierName = row.SupplierName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    spr1.Rate = row.Rate;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchaseProduct_Result();
                                    spr2.InvoiceNO = "Grand Total:";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.ItemName;
                            }
                            else
                            {
                                var spr = new PurchaseProduct_Result();
                                spr.ItemName = row.ItemName;
                                spr.SupplierName = "Header";
                                sprList.Add(spr);
                                var spr1 = new PurchaseProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.SupplierName = row.SupplierName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                spr1.Rate = row.Rate;
                                sprList.Add(spr1);

                                counter++;
                                oldproduct = row.ItemName;
                            }


                        }
                        else
                        {


                            if (counter != productcount)
                            {
                                var spr1 = new PurchaseProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.SupplierName = row.SupplierName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                spr1.Rate = row.Rate;
                                sprList.Add(spr1);
                            }
                            else
                            {
                                var spr1 = new PurchaseProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.SupplierName = row.SupplierName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                spr1.Rate = row.Rate;
                                sprList.Add(spr1);
                                var spr2 = new PurchaseProduct_Result();
                                spr2.InvoiceNO = "Grand Total:";
                                spr2.Quantity = quantitytotal;
                                spr2.TotalAmount = amounttotal;
                                sprList.Add(spr2);
                            }
                            oldproduct = row.ItemName;
                            counter++;

                        }
                    }

                }




                return PartialView(sprList);
            }

            else
            {


                List<PurchaseProduct_Result> sprList = new List<PurchaseProduct_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal quantitytotal = 0;
                decimal amounttotal = 0;

                try
                {

                    var result = db.PurchaseProduct(id, FromDate, ToDate, companyid, Branchid).ToList();


                    if (result.Count != 0)
                    {

                        foreach (var row in result)
                        {
                            if (oldproduct != row.ItemName)
                            {
                                counter = 0;
                                quantitytotal = result.Where(r => r.ItemName == row.ItemName).Sum(r => r.Quantity);
                                amounttotal = result.Where(r => r.ItemName == row.ItemName).Sum(r => r.TotalAmount);
                                productcount = result.Where(r => r.ItemName == row.ItemName).Count() - 1;
                                if (counter == productcount)
                                {
                                    if (counter == 0)
                                    {
                                        var spr = new PurchaseProduct_Result();
                                        spr.ItemName = row.ItemName;
                                        spr.SupplierName = "Header";
                                        sprList.Add(spr);
                                        var spr1 = new PurchaseProduct_Result();
                                        spr1.ItemName = row.ItemName;
                                        spr1.SupplierName = row.SupplierName;
                                        spr1.Quantity = row.Quantity;
                                        spr1.TotalAmount = row.TotalAmount;
                                        spr1.InvoiceNO = row.InvoiceNO;
                                        spr1.InvoiceDate = row.InvoiceDate;
                                        spr1.Rate = row.Rate;
                                        sprList.Add(spr1);
                                        var spr2 = new PurchaseProduct_Result();
                                        //     spr2.InvoiceNO = "Grand Total:";
                                        spr2.Quantity = quantitytotal;
                                        spr2.TotalAmount = amounttotal;
                                        sprList.Add(spr2);
                                    }
                                    else
                                    {
                                        var spr1 = new PurchaseProduct_Result();
                                        spr1.ItemName = row.ItemName;
                                        spr1.SupplierName = row.SupplierName;
                                        spr1.Quantity = row.Quantity;
                                        spr1.TotalAmount = row.TotalAmount;
                                        spr1.InvoiceNO = row.InvoiceNO;
                                        spr1.InvoiceDate = row.InvoiceDate;
                                        spr1.Rate = row.Rate;
                                        sprList.Add(spr1);
                                        var spr2 = new PurchaseProduct_Result();
                                        //  spr2.InvoiceNO = "Grand Total";
                                        spr2.Quantity = quantitytotal;
                                        spr2.TotalAmount = amounttotal;
                                        sprList.Add(spr2);
                                    }

                                    oldproduct = row.ItemName;
                                }
                                else
                                {
                                    var spr = new PurchaseProduct_Result();
                                    spr.ItemName = row.ItemName;
                                    spr.SupplierName = "Header";
                                    sprList.Add(spr);
                                    var spr1 = new PurchaseProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.SupplierName = row.SupplierName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    // spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    spr1.Rate = row.Rate;
                                    sprList.Add(spr1);

                                    counter++;
                                    oldproduct = row.ItemName;
                                }


                            }
                            else
                            {


                                if (counter != productcount)
                                {
                                    var spr1 = new PurchaseProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.SupplierName = row.SupplierName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    //   spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    spr1.Rate = row.Rate;
                                    sprList.Add(spr1);
                                }
                                else
                                {
                                    var spr1 = new PurchaseProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.SupplierName = row.SupplierName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    //  spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    spr1.Rate = row.Rate;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchaseProduct_Result();
                                    spr2.InvoiceNO = "Grand Total:";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                oldproduct = row.ItemName;
                                counter++;

                            }
                        }

                    }
                }
                catch (DbEntityValidationException e) //--------Form Validation Error Throw--------//
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Response.Write("- Property:" + ve.PropertyName + ", Error: " + ve.ErrorMessage);

                        }
                    }
                    throw;
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    return RedirectToAction("ShowAllCustomer", new { Err = "Please Try Again !...." });

                }
                catch (DbUpdateException ex) //--------Databse Error Throw--------//
                {
                    UpdateException updateException = (UpdateException)ex.InnerException;
                    SqlException sqlException = (SqlException)updateException.InnerException;

                    foreach (SqlError error in sqlException.Errors)
                    {
                        Response.Write("- Property:" + error.Number + ", Error: " + error.Message);

                    }
                    return RedirectToAction("ShowAllCustomer", new { Err = "Please Try Again !...." });
                }
                catch (DataException)
                {
                    //Log the error (add a variable name after DataException)
                    ViewBag.Error = "Error:Data  not Saved Successfully.......";
                    return RedirectToAction("ShowAllCustomer", new { Err = "Please Try Again !...." });

                }
                catch
                {
                    return RedirectToAction("ShowAllCustomer", new { Err = "Please Try Again !...." });
                }




                return PartialView(sprList);
            }
        }




        [HttpGet]
        public ActionResult PurchaseProductPDF(int comp, long branchid, int id, DateTime? fromDate, DateTime? toDate)
        {

            var detls = db.Companies.Where(c => c.Id == comp).FirstOrDefault();

            ViewBag.Name = detls.Name;
            ViewBag.address = detls.Address;
            ViewBag.pin = detls.Zipcode;

            try
            {
                var branch = db.BranchMasters.Where(c => c.Id == branchid).FirstOrDefault();
                ViewBag.branchname = branch.Name;
            }
            catch { }



            // string date = "";
            if (fromDate != null)
            {
                ViewBag.from = fromDate.ToString();
                ViewBag.to = toDate.ToString();
            }

            if (id != 0)
            {

                




                List<PurchaseProduct_Result> sprList = new List<PurchaseProduct_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal quantitytotal = 0;
                decimal amounttotal = 0;
                var result = db.PurchaseProduct(id, fromDate, toDate, comp, branchid).ToList();

                if (result.Count != 0)
                {

                    foreach (var row in result)
                    {
                        if (oldproduct != row.ItemName)
                        {
                            counter = 0;
                            quantitytotal = result.Where(r => r.ItemName == row.ItemName).Sum(r => r.Quantity);
                            amounttotal = result.Where(r => r.ItemName == row.ItemName).Sum(r => r.TotalAmount);
                            productcount = result.Where(r => r.ItemName == row.ItemName).Count() - 1;
                            if (counter == productcount)
                            {
                                if (counter == 0)
                                {
                                    var spr = new PurchaseProduct_Result();
                                    spr.ItemName = row.ItemName;
                                    spr.SupplierName = "Header";
                                    sprList.Add(spr);
                                    var spr1 = new PurchaseProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.SupplierName = row.SupplierName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchaseProduct_Result();
                                    spr2.InvoiceNO = "Grand Total:";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new PurchaseProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.SupplierName = row.SupplierName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchaseProduct_Result();
                                    spr2.InvoiceNO = "Grand Total:";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.ItemName;
                            }
                            else
                            {
                                var spr = new PurchaseProduct_Result();
                                spr.ItemName = row.ItemName;
                                spr.SupplierName = "Header";
                                sprList.Add(spr);
                                var spr1 = new PurchaseProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.SupplierName = row.SupplierName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);

                                counter++;
                                oldproduct = row.ItemName;
                            }


                        }
                        else
                        {


                            if (counter != productcount)
                            {
                                var spr1 = new PurchaseProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.SupplierName = row.SupplierName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);
                            }
                            else
                            {
                                var spr1 = new PurchaseProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.SupplierName = row.SupplierName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);
                                var spr2 = new PurchaseProduct_Result();
                                spr2.InvoiceNO = "Grand Total:";
                                spr2.Quantity = quantitytotal;
                                spr2.TotalAmount = amounttotal;
                                sprList.Add(spr2);
                            }
                            oldproduct = row.ItemName;
                            counter++;

                        }
                    }

                }

                return View(sprList);




            }

            else
            {

               

                List<PurchaseProduct_Result> sprList = new List<PurchaseProduct_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal quantitytotal = 0;
                decimal amounttotal = 0;
                var result = db.PurchaseProduct(id, fromDate, toDate, comp, branchid).ToList();
                if (result.Count != 0)
                {

                    foreach (var row in result)
                    {
                        if (oldproduct != row.ItemName)
                        {
                            counter = 0;
                            quantitytotal = result.Where(r => r.ItemName == row.ItemName).Sum(r => r.Quantity);
                            amounttotal = result.Where(r => r.ItemName == row.ItemName).Sum(r => r.TotalAmount);
                            productcount = result.Where(r => r.ItemName == row.ItemName).Count() - 1;
                            if (counter == productcount)
                            {
                                if (counter == 0)
                                {
                                    var spr = new PurchaseProduct_Result();
                                    spr.ItemName = row.ItemName;
                                    spr.SupplierName = "Header";
                                    sprList.Add(spr);
                                    var spr1 = new PurchaseProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.SupplierName = row.SupplierName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchaseProduct_Result();
                                    spr2.InvoiceNO = "Grand Total:";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new PurchaseProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.SupplierName = row.SupplierName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchaseProduct_Result();
                                    spr2.InvoiceNO = "Grand Total";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.ItemName;
                            }
                            else
                            {
                                var spr = new PurchaseProduct_Result();
                                spr.ItemName = row.ItemName;
                                spr.SupplierName = "Header";
                                sprList.Add(spr);
                                var spr1 = new PurchaseProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.SupplierName = row.SupplierName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);

                                counter++;
                                oldproduct = row.ItemName;
                            }


                        }
                        else
                        {


                            if (counter != productcount)
                            {
                                var spr1 = new PurchaseProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.SupplierName = row.SupplierName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);
                            }
                            else
                            {
                                var spr1 = new PurchaseProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.SupplierName = row.SupplierName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);
                                var spr2 = new PurchaseProduct_Result();
                                spr2.InvoiceNO = "Grand Total:";
                                spr2.Quantity = quantitytotal;
                                spr2.TotalAmount = amounttotal;
                                sprList.Add(spr2);
                            }
                            oldproduct = row.ItemName;
                            counter++;

                        }
                    }

                }




                return View(sprList);
            }


        }



        [HttpGet]
        public ActionResult PurchaseProductPrint(int id)
        {

            //int id = 1; string value = "0"; string from = "0"; string to = "0";
            int compid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);


            var detls = db.Companies.Where(c => c.Id == compid).FirstOrDefault();

            ViewBag.Name = detls.Name;
            ViewBag.address = detls.Address;
            ViewBag.pin = detls.Zipcode;

            try
            {
                var branch = db.BranchMasters.Where(c => c.Id == Branchid).FirstOrDefault();
                ViewBag.branchname = branch.Name;
            }
            catch { }



            // string date = "";


            if (id != 0)
            {
                string from = Session["fdate"].ToString();
                string to = Session["tdate"].ToString();


                DateTime? fromDate = DateTime.Parse(Convert.ToString(from));
                DateTime? toDate = DateTime.Parse(Convert.ToString(to));


                ViewBag.from = fromDate;
                ViewBag.to = toDate;




                List<PurchaseProduct_Result> sprList = new List<PurchaseProduct_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal quantitytotal = 0;
                decimal amounttotal = 0;
                var result = db.PurchaseProduct(id, fromDate, toDate, compid, Branchid).ToList();

                if (result.Count != 0)
                {

                    foreach (var row in result)
                    {
                        if (oldproduct != row.ItemName)
                        {
                            counter = 0;
                            quantitytotal = result.Where(r => r.ItemName == row.ItemName).Sum(r => r.Quantity);
                            amounttotal = result.Where(r => r.ItemName == row.ItemName).Sum(r => r.TotalAmount);
                            productcount = result.Where(r => r.ItemName == row.ItemName).Count() - 1;
                            if (counter == productcount)
                            {
                                if (counter == 0)
                                {
                                    var spr = new PurchaseProduct_Result();
                                    spr.ItemName = row.ItemName;
                                    spr.SupplierName = "Header";
                                    sprList.Add(spr);
                                    var spr1 = new PurchaseProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.SupplierName = row.SupplierName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchaseProduct_Result();
                                    spr2.InvoiceNO = "Grand Total:";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new PurchaseProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.SupplierName = row.SupplierName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchaseProduct_Result();
                                    spr2.InvoiceNO = "Grand Total:";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.ItemName;
                            }
                            else
                            {
                                var spr = new PurchaseProduct_Result();
                                spr.ItemName = row.ItemName;
                                spr.SupplierName = "Header";
                                sprList.Add(spr);
                                var spr1 = new PurchaseProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.SupplierName = row.SupplierName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);

                                counter++;
                                oldproduct = row.ItemName;
                            }


                        }
                        else
                        {


                            if (counter != productcount)
                            {
                                var spr1 = new PurchaseProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.SupplierName = row.SupplierName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);
                            }
                            else
                            {
                                var spr1 = new PurchaseProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.SupplierName = row.SupplierName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);
                                var spr2 = new PurchaseProduct_Result();
                                spr2.InvoiceNO = "Grand Total:";
                                spr2.Quantity = quantitytotal;
                                spr2.TotalAmount = amounttotal;
                                sprList.Add(spr2);
                            }
                            oldproduct = row.ItemName;
                            counter++;

                        }
                    }

                }

                return View(sprList);




            }

            else
            {

                DateTime? fromDate = null; DateTime? toDate = null;

                List<PurchaseProduct_Result> sprList = new List<PurchaseProduct_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal quantitytotal = 0;
                decimal amounttotal = 0;
                var result = db.PurchaseProduct(id, fromDate, toDate, compid, Branchid).ToList();
                if (result.Count != 0)
                {

                    foreach (var row in result)
                    {
                        if (oldproduct != row.ItemName)
                        {
                            counter = 0;
                            quantitytotal = result.Where(r => r.ItemName == row.ItemName).Sum(r => r.Quantity);
                            amounttotal = result.Where(r => r.ItemName == row.ItemName).Sum(r => r.TotalAmount);
                            productcount = result.Where(r => r.ItemName == row.ItemName).Count() - 1;
                            if (counter == productcount)
                            {
                                if (counter == 0)
                                {
                                    var spr = new PurchaseProduct_Result();
                                    spr.ItemName = row.ItemName;
                                    spr.SupplierName = "Header";
                                    sprList.Add(spr);
                                    var spr1 = new PurchaseProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.SupplierName = row.SupplierName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchaseProduct_Result();
                                    spr2.InvoiceNO = "Grand Total:";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new PurchaseProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.SupplierName = row.SupplierName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchaseProduct_Result();
                                    spr2.InvoiceNO = "Grand Total";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.ItemName;
                            }
                            else
                            {
                                var spr = new PurchaseProduct_Result();
                                spr.ItemName = row.ItemName;
                                spr.SupplierName = "Header";
                                sprList.Add(spr);
                                var spr1 = new PurchaseProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.SupplierName = row.SupplierName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);

                                counter++;
                                oldproduct = row.ItemName;
                            }


                        }
                        else
                        {


                            if (counter != productcount)
                            {
                                var spr1 = new PurchaseProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.SupplierName = row.SupplierName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);
                            }
                            else
                            {
                                var spr1 = new PurchaseProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.SupplierName = row.SupplierName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);
                                var spr2 = new PurchaseProduct_Result();
                                spr2.InvoiceNO = "Grand Total:";
                                spr2.Quantity = quantitytotal;
                                spr2.TotalAmount = amounttotal;
                                sprList.Add(spr2);
                            }
                            oldproduct = row.ItemName;
                            counter++;

                        }
                    }

                }




                return View(sprList);
            }


        }





        [HttpGet]
        public ActionResult PurchasebyProductPDFlink()
        {
            int compid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? fromDate = null;
            DateTime? toDate = null;
            string from = Session["fdate"].ToString();
            string to = Session["tdate"].ToString();
            if (from != "")
            {
                fromDate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            if (to != "")
            {
                toDate = DateTime.ParseExact(to, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }

            string ProdId = Session["productid"].ToString();

            try
            {

                return new ActionAsPdf("PurchaseProductPDF", new { comp = compid, branchid = Branchid, id = ProdId, fromDate = fromDate, toDate = toDate }) { FileName = "PurchaseByProduct.pdf" };
            }
            catch
            {
                return new ActionAsPdf("PurchaseProductPDF", new { comp = compid, branchid = Branchid, id = ProdId, fromDate = fromDate, toDate = toDate }) { FileName = "PurchaseByProduct.pdf" };
            }

        }




        #endregion





        #region ---- Purchase By Date--------------

        [HttpGet]
        public ActionResult PurchasebyDate(string Msg, string Err)
        {


            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }

            return View();


        }


        [HttpGet]
        public ActionResult PurchasebyDateReport(string From, string To)
        {
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? FromDate = null;
            DateTime? ToDate = null;
            if (From != "0")
            {
                FromDate = DateTime.ParseExact(From, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            if (To != "0")
            {
                ToDate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            Session["fdate"] = FromDate;
            Session["tdate"] = ToDate;

            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);




            if (From != "0")
            {
                Session["datetime"] = 1;


                //var result = (from sale in db.SalesInvoices
                //             where  sale.Date >= From && sale.Date <= To && sale.CompanyId == companyid && sale.BranchId == Branchid).ToList();


                var result = db.PurchaseInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.Date >= FromDate && s.Date <= ToDate).ToList();


                return PartialView(result);
            }
            else
            {
                Session["datetime"] = 0;

                var result = db.PurchaseInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid).ToList();





                return PartialView(result);
            }



        }



        public FileResult PurchaseDateExport()
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();

            string date = "";

            date = Session["datetime"].ToString();
            if (date == "1")
            {


                DateTime? fromDate = (DateTime?)Session["fdate"];
                DateTime? toDate = (DateTime?)Session["tdate"];

                //Get the data representing the current grid state - page, sort and filter


                var result = db.PurchaseInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.Date >= fromDate && s.Date <= toDate).ToList();


                //Create new Excel workbook
                var workbook = new HSSFWorkbook();

                //Create new Excel sheet
                var sheet = workbook.CreateSheet();

                //(Optional) set the width of the columns
                sheet.SetColumnWidth(0, 50 * 56);
                sheet.SetColumnWidth(1, 50 * 56);
                sheet.SetColumnWidth(2, 50 * 56);
                sheet.SetColumnWidth(2, 50 * 156);
                sheet.SetColumnWidth(4, 50 * 56);
               
                //Create a header row



                // Create a font object and make it bold
                var HeaderCellStyle = workbook.CreateCellStyle();
                // HeaderCellStyle.BorderBottom = BorderStyle.THIN;
                HeaderCellStyle.Alignment = HorizontalAlignment.CENTER;
                var HeaderFont = workbook.CreateFont();
                HeaderFont.Boldweight = (short)FontBoldWeight.BOLD;
                HeaderCellStyle.SetFont(HeaderFont);



                //Create a header row
                var headerRow = sheet.CreateRow(9);
                // getting company details
                var companyDetail = db.Companies.Where(c => c.Id == companyid).FirstOrDefault();
                //Set the column names in the header row          
                int rowNumber = 1;


                //Set the column names in the header row

                var row11 = sheet.CreateRow(rowNumber++);
                var cell = row11.CreateCell(3);
                cell.SetCellValue(companyDetail.Name);

                cell.CellStyle = HeaderCellStyle;
                row11 = sheet.CreateRow(rowNumber++);
                cell = row11.CreateCell(3);
                cell.SetCellValue(companyDetail.Address);
                cell.CellStyle = HeaderCellStyle;

                row11 = sheet.CreateRow(rowNumber++);
                cell = row11.CreateCell(3);
                cell.SetCellValue(companyDetail.Zipcode);
                cell.CellStyle = HeaderCellStyle;

                row11 = sheet.CreateRow(rowNumber++);
                row11 = sheet.CreateRow(rowNumber++);

                cell = row11.CreateCell(3);
                cell.SetCellValue("Purchase By Date");
                cell.CellStyle = HeaderCellStyle;

                row11 = sheet.CreateRow(rowNumber++);
                row11 = sheet.CreateRow(rowNumber++);
                cell = row11.CreateCell(3);
                cell.SetCellValue("For " + String.Format(Session["DateFormatUpper"].ToString(), fromDate.Value.ToShortDateString()) + " To " + String.Format(Session["DateFormatUpper"].ToString(), toDate.Value.ToShortDateString()));


                row11 = sheet.CreateRow(rowNumber++);
                row11 = sheet.CreateRow(rowNumber++);

                // Create a font object and make it bold
                var detailCellStyle = workbook.CreateCellStyle();
                //detailCellStyle.BorderBottom = BorderStyle.Double;
                //detailCellStyle.BorderTop = BorderStyle.THIN;
                var detailFont = workbook.CreateFont();
                detailFont.Boldweight = (short)FontBoldWeight.BOLD;
                detailCellStyle.SetFont(detailFont);



                cell = headerRow.CreateCell(0);
                cell.SetCellValue("SL. No.");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(1);
                cell.SetCellValue("Date");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(2);
                cell.SetCellValue("Bill No.");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(3);
                cell.SetCellValue("Party Name.");
                cell.CellStyle = detailCellStyle;

               

                cell = headerRow.CreateCell(4);
                cell.SetCellValue("Net Total");
                cell.CellStyle = detailCellStyle;


                sheet.CreateFreezePane(0, 1, 0, 1);



                //Populate the sheet with values from the grid data
              
                decimal? total = 0;
                int serial = 0;
                foreach (var product in result)
                {
                   
                    var gtotal = (decimal)(product.BCGrandTotal);
                    total += gtotal;
                    serial++;

                    //Create a new row
                    var row = sheet.CreateRow(rowNumber++);
                    //   total = total + product.Amount;
                    //Set values for the cells
                    row.CreateCell(0).SetCellValue(serial);
                    row.CreateCell(1).SetCellValue(String.Format(Session["DateFormatUpper"].ToString(), product.Date));
                    row.CreateCell(2).SetCellValue(product.NO);
                    row.CreateCell(3).SetCellValue(product.Supplier.Name);
                    
                    row.CreateCell(4).SetCellValue(Math.Round((double)gtotal, 2));

                }
                var rowTotal = sheet.CreateRow(rowNumber++);

                var expense = rowTotal.CreateCell(1);
                expense.SetCellType(CellType.NUMERIC);
                expense.SetCellValue("Grand Total");
                expense.CellStyle = detailCellStyle;

                var currencyCellStyle = workbook.CreateCellStyle();
                // Right-align currency values
                currencyCellStyle.Alignment = HorizontalAlignment.RIGHT;

                var detailFonts = workbook.CreateFont();
                detailFonts.Boldweight = (short)FontBoldWeight.BOLD;
                currencyCellStyle.SetFont(detailFonts);



                // Get / create the data format string
                var formatId = HSSFDataFormat.GetBuiltinFormat("#,##0.00");
                if (formatId == -1)
                {
                    var newDataFormat = workbook.CreateDataFormat();
                    currencyCellStyle.DataFormat = newDataFormat.GetFormat("#,##0.00");
                }
                else
                    currencyCellStyle.DataFormat = formatId;

                var jan = rowTotal.CreateCell(4);
                jan.SetCellType(CellType.FORMULA);
                jan.CellFormula = "SUM(" + total + ")";
                jan.CellStyle = currencyCellStyle;
                jan.SetCellValue(Convert.ToDouble(total));
                sheet.CreateFreezePane(0, 1, 0, 1);

                //Write the workbook to a memory stream
                MemoryStream output = new MemoryStream();
                workbook.Write(output);

                //Return the result to the end user


                //Return the result to the end user

                return File(output.ToArray(),   //The binary data of the XLS file
                    "application/vnd.ms-excel", //MIME type of Excel files
                    "PurchaseByDate.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user




            }
            else
            {
                DateTime? fromDate = (DateTime?)Session["fdate"];
                DateTime? toDate = (DateTime?)Session["tdate"];

                //Get the data representing the current grid state - page, sort and filter


                var result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid).ToList();


                //Create new Excel workbook
                var workbook = new HSSFWorkbook();

                //Create new Excel sheet
                var sheet = workbook.CreateSheet();

                //(Optional) set the width of the columns
                sheet.SetColumnWidth(0, 50 * 56);
                sheet.SetColumnWidth(1, 50 * 56);
                sheet.SetColumnWidth(2, 50 * 56);
                sheet.SetColumnWidth(2, 50 * 156);
                sheet.SetColumnWidth(4, 50 * 56);
               
                //Create a header row



                // Create a font object and make it bold
                var HeaderCellStyle = workbook.CreateCellStyle();
                // HeaderCellStyle.BorderBottom = BorderStyle.THIN;
                HeaderCellStyle.Alignment = HorizontalAlignment.CENTER;
                var HeaderFont = workbook.CreateFont();
                HeaderFont.Boldweight = (short)FontBoldWeight.BOLD;
                HeaderCellStyle.SetFont(HeaderFont);



                //Create a header row
                var headerRow = sheet.CreateRow(9);
                // getting company details
                var companyDetail = db.Companies.Where(c => c.Id == companyid).FirstOrDefault();
                //Set the column names in the header row          
                int rowNumber = 1;


                //Set the column names in the header row

                var row11 = sheet.CreateRow(rowNumber++);
                var cell = row11.CreateCell(3);
                cell.SetCellValue(companyDetail.Name);

                cell.CellStyle = HeaderCellStyle;
                row11 = sheet.CreateRow(rowNumber++);
                cell = row11.CreateCell(3);
                cell.SetCellValue(companyDetail.Address);
                cell.CellStyle = HeaderCellStyle;

                row11 = sheet.CreateRow(rowNumber++);
                cell = row11.CreateCell(3);
                cell.SetCellValue(companyDetail.Zipcode);
                cell.CellStyle = HeaderCellStyle;

                row11 = sheet.CreateRow(rowNumber++);
                row11 = sheet.CreateRow(rowNumber++);

                cell = row11.CreateCell(3);
                cell.SetCellValue("Purchase Till Date");
                cell.CellStyle = HeaderCellStyle;

                row11 = sheet.CreateRow(rowNumber++);
                row11 = sheet.CreateRow(rowNumber++);
                cell = row11.CreateCell(3);
                //cell.SetCellValue("For " + String.Format(Session["DateFormatUpper"].ToString(), fromDate.Value.ToShortDateString()) + " To " + String.Format(Session["DateFormatUpper"].ToString(), toDate.Value.ToShortDateString()));


                row11 = sheet.CreateRow(rowNumber++);
                row11 = sheet.CreateRow(rowNumber++);

                // Create a font object and make it bold
                var detailCellStyle = workbook.CreateCellStyle();
                //detailCellStyle.BorderBottom = BorderStyle.Double;
                //detailCellStyle.BorderTop = BorderStyle.THIN;
                var detailFont = workbook.CreateFont();
                detailFont.Boldweight = (short)FontBoldWeight.BOLD;
                detailCellStyle.SetFont(detailFont);



                cell = headerRow.CreateCell(0);
                cell.SetCellValue("SL. No.");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(1);
                cell.SetCellValue("Date");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(2);
                cell.SetCellValue("Bill No.");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(3);
                cell.SetCellValue("Party Name.");
                cell.CellStyle = detailCellStyle;

              

                cell = headerRow.CreateCell(4);
                cell.SetCellValue("Net Total");
                cell.CellStyle = detailCellStyle;


                sheet.CreateFreezePane(0, 1, 0, 1);



                //Populate the sheet with values from the grid data
               
                decimal? total = 0;
                int serial = 0;
                foreach (var product in result)
                {
                    var sub = (decimal)(product.BCGrandTotal + product.Dis);
                    
                    var gtotal = (decimal)(product.BCGrandTotal);
                    total += gtotal;
                    serial++;

                    //Create a new row
                    var row = sheet.CreateRow(rowNumber++);
                    //   total = total + product.Amount;
                    //Set values for the cells
                    row.CreateCell(0).SetCellValue(serial);
                    row.CreateCell(1).SetCellValue(String.Format(Session["DateFormatUpper"].ToString(), product.Date));
                    row.CreateCell(2).SetCellValue(product.NO);
                    row.CreateCell(3).SetCellValue(product.Customer.Name);
                   
                    row.CreateCell(4).SetCellValue(Math.Round((double)gtotal, 2));

                }
                var rowTotal = sheet.CreateRow(rowNumber++);

                var expense = rowTotal.CreateCell(1);
                expense.SetCellType(CellType.NUMERIC);
                expense.SetCellValue("Grand Total");
                expense.CellStyle = detailCellStyle;

                var currencyCellStyle = workbook.CreateCellStyle();
                // Right-align currency values
                currencyCellStyle.Alignment = HorizontalAlignment.RIGHT;

                var detailFonts = workbook.CreateFont();
                detailFonts.Boldweight = (short)FontBoldWeight.BOLD;
                currencyCellStyle.SetFont(detailFonts);



                // Get / create the data format string
                var formatId = HSSFDataFormat.GetBuiltinFormat("#,##0.00");
                if (formatId == -1)
                {
                    var newDataFormat = workbook.CreateDataFormat();
                    currencyCellStyle.DataFormat = newDataFormat.GetFormat("#,##0.00");
                }
                else
                    currencyCellStyle.DataFormat = formatId;

                var jan = rowTotal.CreateCell(4);
                jan.SetCellType(CellType.FORMULA);
                jan.CellFormula = "SUM(" + total + ")";
                jan.CellStyle = currencyCellStyle;
                jan.SetCellValue(Convert.ToDouble(total));
                sheet.CreateFreezePane(0, 1, 0, 1);

                //Write the workbook to a memory stream
                MemoryStream output = new MemoryStream();
                workbook.Write(output);

                //Return the result to the end user

                return File(output.ToArray(),   //The binary data of the XLS file
                    "application/vnd.ms-excel", //MIME type of Excel files
                    "PurchaseByDate.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user



            }



        }



        [HttpGet]
        public ActionResult PurchasebyDatePDF(int id, string value, DateTime? From, DateTime? To)
        {

            //int id = 1; string value = "0"; string from = "0"; string to = "0";




            // string date = "";



            if (value == "1")
            {





                ViewBag.from = From;
                ViewBag.to = To;


                //Get the data representing the current grid state - page, sort and filter


                var result = db.PurchaseInvoices.Where(s => s.CompanyId == id && s.BranchId == 0 && s.Date >= From && s.Date <= To).ToList();
                return View(result);

            }

            else
            {
                var result = db.PurchaseInvoices.Where(s => s.CompanyId == id && s.BranchId == 0).ToList();
                return View(result);
            }



        }


        [HttpGet]
        public ActionResult PurchasebyDatePrint()
        {
            int compid = Convert.ToInt32(Session["companyid"]);

            string val = Session["datetime"].ToString();


            long Branchid = Convert.ToInt64(Session["BranchId"]);


            var detls = db.Companies.Where(c => c.Id == compid).FirstOrDefault();

            ViewBag.Name = detls.Name;
            ViewBag.address = detls.Address;
            ViewBag.pin = detls.Zipcode;

            try
            {
                var branch = db.BranchMasters.Where(c => c.Id == Branchid).FirstOrDefault();
                ViewBag.branchname = branch.Name;
            }
            catch { }



            // string date = "";



            if (val == "1")
            {

                string from = Session["fdate"].ToString();
                string to = Session["tdate"].ToString();


                DateTime fromDate = DateTime.Parse(Convert.ToString(from));
                DateTime toDate = DateTime.Parse(Convert.ToString(to));


                ViewBag.from = fromDate;
                ViewBag.to = toDate;


                //Get the data representing the current grid state - page, sort and filter


                var result = from sale in db.ReceiptPayments
                             where sale.transactionType == "Purchase Invoice" && sale.RPdate >= fromDate && sale.RPdate <= toDate && sale.CompanyId == compid && sale.BranchId == Branchid
                             group sale by sale.RPdate into sa

                             select new Salesdate
                             {
                                 Date = sa.Key,
                                 Amount = sa.Sum(sal => sal.TotalAmount)

                             };
                return View(result);

            }

            else
            {
                var result = from sales in db.ReceiptPayments
                             where sales.transactionType == "Purchase Invoice" && sales.CompanyId == compid && sales.BranchId == Branchid
                             group sales by sales.RPdate into s
                             select new Salesdate
                             {
                                 Date = s.Key,
                                 Amount = s.Sum(sale => sale.TotalAmount)

                             };
                return View(result);
            }



        }



        [HttpGet]
        public ActionResult PurchasebyDatePDFlink()
        {
            int compid = Convert.ToInt32(Session["companyid"]);

            string val = Session["datetime"].ToString();

            try
            {
                string from = Session["fdate"].ToString();
                string to = Session["tdate"].ToString();
                return new ActionAsPdf("PurchasebyDatePDF", new { id = compid, value = val, from = from, to = to }) { FileName = "PurchaseByDate.pdf" };
            }
            catch
            {
                return new ActionAsPdf("PurchasebyDatePDF", new { id = compid, value = val, from = 0, to = 0 }) { FileName = "PurchaseByDate.pdf" };
            }

        }




        [HttpPost]
        public ActionResult PurchasebyDateMail(string Mailto, string message, string subj, HttpPostedFileBase fileUploader)
        {
            //string from = Session["fdate"].ToString();
            //   string to = Session["tdate"].ToString();


            //DateTime fromDate = DateTime.Parse(Convert.ToString(from));
            //DateTime toDate = DateTime.Parse(Convert.ToString(to));


            int compid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);


            var detls = db.Companies.Where(c => c.Id == compid).FirstOrDefault();

            ViewBag.Name = detls.Name;
            ViewBag.address = detls.Address;
            ViewBag.pin = detls.Zipcode;

            string[] multimailId = Mailto.Split(',');




            MailMessage mail = new MailMessage();

            foreach (string mailid in multimailId)
            {
                mail.To.Add(new MailAddress(mailid));
            }
            //  mail.To.Add(Mailto);
            mail.From = new MailAddress("cabbooking7@gmail.com", "Purchase By Date Report", System.Text.Encoding.UTF8);
            mail.Subject = subj;
            mail.SubjectEncoding = System.Text.Encoding.UTF8;

            string body = "<h3>Purchase By Date Report</h3><br/>";

            int slno = 0;
            decimal? amt = 0;
            var result = from sales in db.ReceiptPayments
                         where sales.transactionType == "Purchase Invoice" && sales.CompanyId == compid && sales.BranchId == Branchid
                         group sales by sales.RPdate into s
                         select new Salesdate
                         {
                             Date = s.Key,
                             Amount = s.Sum(sale => sale.TotalAmount)

                         };

            body = body + @"<table align='center' bgcolor='#cccccc' cellpadding='0' cellspacing='0' style='width: 100%; background:#cccccc; background-color:#cccccc; margin:0; padding:0 20px;'>
	<tr>
		<td>
		<table align='center' cellpadding='0' cellspacing='0' style='width: 620px; border-collapse:collapse; text-align:left; font-family:Tahoma; font-weight:normal; font-size:12px; line-height:15pt; color:#444444; margin:0 auto;'>
			<!-- Start of logo and top links -->
			<tr>
				<td valign='top' style='height:5px;margin:0;padding:0;;line-height:0;'>
				<img alt='' height='5' src='' vspace='0' style='border:0; padding:0; margin:0; line-height:0;' width='620'/></td>
			</tr>
			<tr>
				<td style=' width:620px;' valign='top'>
					<table cellpadding='0' cellspacing='0' style='width:100%; border-collapse:collapse;font-family:Tahoma; font-weight:normal; font-size:12px; line-height:15pt; color:#444444;' >
						<tr>
							<td bgcolor='#86ac3d' style='width: 320px; padding:10px 0 10px 20px; background:#86ac3d; background-color:#86ac3d; color:#ffffff;' valign='top'>
								<span style='color:#9bc251;'> | </span><a style='color:#485c22; text-decoration:underline;' href='http://www.xenerp.xentricserver.com'>Visit our website </a><span style='color:#9bc251;'> | </span
							</td>
							<td bgcolor='#86ac3d' style='width: 300px; padding:10px 20px 10px 20px; background:#86ac3d; background-color:#86ac3d; text-align:right; color:#ffffff;' valign='top'>
								 Support:  033-568-789
							</td>
						</tr>
						<tr>
							
							<td colspan='2' bgcolor='#FFFFFF' style='width: 350px; padding:20px 20px 15px 20px; background:#ffffff; background-color:#ffffff; text-align:center; font-size:18px;' valign='middle'>
							 Purchase By DateWise Report
							</td>
						</tr>
					</table>
				</td>
			</tr>
			<tr>
				<td valign='top' style='height:5px;margin:0;padding:0;line-height:0;'>
				<img alt='' height='5' src='http://democab.xentricserver.com/Images/email/Newsletter_border_Bottom.png' vspace='0' style='border:0; padding:0; margin:0; line-height:0;' width='620'/></td>
			
            </tr>
			<tr>
			<td bgcolor='#FFFFFF' style='padding:10px 20px; background:#ffffff;background-color:#ffffff;' valign='top '>





			<table cellpadding='0' cellspacing='0' style='width: 100%; border-collapse:collapse; font-family:Tahoma; font-weight:normal; font-size:12px; line-height:15pt; color:black;'>
					
					 <thead>
	
            <tr>	

					<tr><td colspan='5' style='width:540px; padding:10px;'> 
                    <p style='padding:0; margin:0 0 11pt 0;line-height:160%; font-size:14px;'>

                            'message'
		
					</td></tr>
					<tr><th colspan='5'><center><H3>From:'+fromDate+'</H3></center><br/><br/></th></tr>	
			<tr>
            <th style='width:340px; padding:0 20px 0 0;'>
								SlNo :
							</th>
							
						
							<th style='width:340px; padding:0 20px 0 0;'>
								Date :
							</th>
							<th valign='middle' align='center' style='text-align:left; width:240px;'>
								Amount:
							</th>
						</tr>
                      </thead>
<tbody>";
            foreach (var sale in result)
            {
                slno = slno + 1;
                amt = amt + sale.Amount;

                body += "<tr><td>" + slno + "</td><td>" + String.Format(Session["DateFormatUpper"].ToString(), sale.Date) + "</td><td> " + sale.Amount + " </td></tr>";

            }
            body += "<tr><td></td><td><h4>Grand Total</h4></td><td><h4> " + amt + " </h4></td></tr>";
            body += @"</tbody></table></td>	<tr>				<td valign='top' style='height:5px;margin:0;padding:0;line-height:0;'>
				<img alt='' height='5' src='http://democab.xentricserver.com/Images/email/Newsletter_border_top.png' vspace='0' style='border:0; padding:0; margin:0; line-height:0;' width='620'/></td>
			</tr>
			<tr>
				<td bgcolor='#FFFFFF' style='padding:10px 20px; background:#ffffff;background-color:#ffffff;' valign='top'>
					<!-- <p style='padding:0; margin:0 0 11pt 0;line-height:160%; font-size:18px;'>
					 <%Name%><br>
					</p>
					<br><br>
					
					Account information</p><br>
                    Your username: <%userName%>
					<br>
					Password :  <%password%><br><br> -->

					 If you need any assistance navigating our website or have any questions please contact us at support@xentriceserver.com
                    <br/>
				</td>
			</tr>
			<tr>
				<td valign='top' style='height:5px;margin:0;padding:0;line-height:0;'>
				<img alt='' height='5' src='http://www.democab.xentricserver.com/Images/email/Newsletter_border_Bottom.png' vspace='0' style='border:0; padding:0; margin:0; line-height:0;' width='620'/></td>
			</tr>
			<!-- End of First Content -->
			
			<!-- Start of Footer -->
			<tr>
				<td valign='top' style='height:5px;margin:0;padding:0;line-height:0;'>
				<img alt='' height='5' src='http://democab.xentricserver.com/Images/email/BottomBackground_Blue_1.png' vspace='0' style='border:0; padding:0; margin:0; line-height:0;' width='620'/></td>
			</tr>
			<tr>
				<td bgcolor='#86ac3d' style='padding:0 20px 15px 20px; background-color:#86ac3d; background:#86ac3d;'>
					<table cellpadding='0' cellspacing='0' style='width: 100%; border-collapse:collapse; font-family:Tahoma; font-weight:normal; font-size:12px; line-height:15pt; color:#FFFFFF;'>
						<br />
                        <tr>
							<td style='width:340px; padding:0 20px 0 0;'>
								Kolkata, West Bengal<br>
								| <a style='color:#485c22; text-decoration:underline;' href='#'>support@xentriceserver.com</a><br/>
								Support:  033-568-789
							</td>
							<td valign='middle' align='center' style='text-align:center; width:240px;'>
								
							</td>
						</tr>
						<tr>
							<td style='padding:20px 0 0 0;' colspan='2'>
								Copyright © 2014 Xentrictechnolgies.
							</td>
						</tr>
					</table>
				</td>
			</tr>
			<tr>
				<td valign='top' style='height:5px;margin:0;padding:0 0 20px 0;line-height:0;'>
				<img alt='' height='5' src='http://democab.xentricserver.com/Images/email/BottomBackground_Blue_2.png' vspace='0' style='border:0; padding:0; margin:0; line-height:0;' width='620'/></td>
			</tr>
</tr></table>";



            mail.Body = body;
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Credentials = new System.Net.NetworkCredential("cabbooking7@gmail.com", "03041983");
            smtp.Port = 587;
            smtp.Host = "smtp.gmail.com";
            smtp.EnableSsl = true;

            try
            {
                smtp.Send(mail);

                //return RedirectToAction("SalesbyDate", new { Msg = "Email Sent Successfully...." });
                return Content("Email Sent Successfully....");
            }
            catch
            {


                return RedirectToAction("PurchasebyDate", new { Err = "Please try again...." });
            }


        }



        #endregion



        #region PurchaseTax Report

        public ActionResult PurchaseTax()
        {

            return View();
        }



        [HttpPost]
        public JsonResult getTax(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            var customers = db.Taxes.Where(p => p.Name.Contains(query) && p.CompanyId == companyid).Select(p => new { Name = p.Name, Id = p.TaxId }).ToList();
            return Json(customers, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getSelectedTax(int id)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            var suppliers = db.Taxes.Where(p => p.TaxId == id).Select(p => new { Name = p.Name, Id = p.TaxId }).FirstOrDefault();
            return Json(suppliers, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult PurchasetaxReport(int id, DateTime? from, DateTime? to)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);

            Session["fdate"] = from;
            Session["tdate"] = to;

            Session["productid"] = id.ToString();
            if (id != 0)
            {

                List<PurchasebyTax_Result> sprList = new List<PurchasebyTax_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal ratetotal = 0;
                decimal amounttotal = 0;
                var result = db.PurchasebyTax(id, from, to, companyid, Branchid).ToList();
                if (result.Count != 0)
                {


                    foreach (var row in result)
                    {
                        if (oldproduct != row.Taxname)
                        {
                            counter = 0;

                            ratetotal = result.Where(r => r.Taxname == row.Taxname).Sum(r => r.Rate);
                            amounttotal = result.Where(r => r.Taxname == row.Taxname).Sum(r => r.TaxAmount);
                            productcount = result.Where(r => r.Taxname == row.Taxname).Count() - 1;
                            if (counter == productcount)
                            {
                                if (counter == 0)
                                {
                                    var spr = new PurchasebyTax_Result();
                                    spr.Taxname = row.Taxname;
                                    spr.Item = "Header";
                                    spr.Rate = ratetotal;
                                    spr.TaxAmount = amounttotal;
                                    sprList.Add(spr);
                                    var spr1 = new PurchasebyTax_Result();
                                    spr1.Item = row.Item;
                                    spr1.Rate = row.Rate;
                                    spr1.Taxname = row.Taxname;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchasebyTax_Result();
                                    spr2.InvoiceNo = "Grand Total:";
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new PurchasebyTax_Result();
                                    spr1.Item = row.Item;
                                    spr1.Rate = row.Rate;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.Taxname = row.Taxname;

                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchasebyTax_Result();
                                    spr2.InvoiceNo = "Grand Total:";
                                    spr2.Rate = ratetotal;
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.Taxname;
                            }
                            else
                            {
                                var spr = new PurchasebyTax_Result();

                                spr.Taxname = row.Taxname;
                                spr.TaxAmount = amounttotal;
                                spr.Item = "Header";
                                spr.Rate = row.Rate;
                                sprList.Add(spr);
                                var spr1 = new PurchasebyTax_Result();
                                spr1.Item = row.Item;
                                spr1.TaxAmount = row.TaxAmount;
                                spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Date = row.Date;
                                spr1.Taxname = row.Taxname;
                                sprList.Add(spr1);

                                counter++;
                                oldproduct = row.Taxname;
                            }


                        }
                        else
                        {


                            if (counter != productcount)
                            {

                                var spr1 = new PurchasebyTax_Result();
                                spr1.Item = row.Item;
                                spr1.Taxname = row.Taxname;
                                spr1.Rate = row.Rate;
                                spr1.TaxAmount = row.TaxAmount;
                                spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Date = row.Date;
                                sprList.Add(spr1);
                            }
                            else
                            {
                                var spr1 = new PurchasebyTax_Result();
                                spr1.Item = row.Item;
                                spr1.Rate = row.Rate;
                                spr1.TaxAmount = row.TaxAmount;
                                spr1.Taxname = row.Taxname;
                                spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Date = row.Date;
                                sprList.Add(spr1);
                                var spr2 = new PurchasebyTax_Result();
                                spr2.InvoiceNo = "Grand Total:";
                                spr2.Rate = ratetotal;
                                spr2.TaxAmount = amounttotal;
                                sprList.Add(spr2);
                            }
                            oldproduct = row.Taxname;
                            counter++;

                        }
                    }

                }




                return PartialView(sprList);
            }

            else
            {



                List<PurchasebyTax_Result> sprList = new List<PurchasebyTax_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal ratetotal = 0;
                decimal amounttotal = 0;
                var result = db.PurchasebyTax(id, from, to, companyid, Branchid).ToList();




                if (result.Count != 0)
                {

                    foreach (var row in result)
                    {
                        if (oldproduct != row.Taxname)
                        {
                            counter = 0;
                            ratetotal = result.Where(r => r.Item == row.Taxname).Sum(r => r.Rate);
                            amounttotal = result.Where(r => r.Item == row.Taxname).Sum(r => r.TaxAmount);
                            productcount = result.Where(r => r.Item == row.Taxname).Count() - 1;
                            if (counter == productcount)
                            {
                                if (counter == 0)
                                {
                                    var spr = new PurchasebyTax_Result();

                                    spr.Item = "Header";
                                    spr.Taxname = row.Taxname;
                                    spr.Rate = ratetotal;
                                    spr.TaxAmount = amounttotal;
                                    sprList.Add(spr);
                                    var spr1 = new PurchasebyTax_Result();
                                    spr1.Item = row.Item;
                                    spr.Taxname = row.Taxname;
                                    spr.Rate = row.Rate;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchasebyTax_Result();
                                    //     spr2.InvoiceNo = "Grand Total:";
                                    spr2.Rate = ratetotal;
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new PurchasebyTax_Result();
                                    spr1.Item = row.Item;
                                    spr1.Rate = row.Rate;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Taxname = row.Taxname;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchasebyTax_Result();
                                    //        spr2.InvoiceNo = "Grand Total:";
                                    spr2.Rate = ratetotal;
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.Taxname;
                            }
                            else
                            {
                                var spr = new PurchasebyTax_Result();
                                spr.Item = "Header";
                                spr.Taxname = row.Taxname;
                                spr.Rate = ratetotal;
                                sprList.Add(spr);

                                var spr1 = new PurchasebyTax_Result();
                                spr1.Item = row.Item;

                                spr.Taxname = row.Taxname;
                                spr.Rate = row.Rate;

                                spr1.TaxAmount = row.TaxAmount;

                                //spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Date = row.Date;
                                sprList.Add(spr1);

                                counter++;
                                oldproduct = row.Taxname;
                            }


                        }
                        else
                        {


                            if (counter != productcount)
                            {

                                var spr1 = new PurchasebyTax_Result();
                                spr1.Item = row.Item;

                                spr1.Rate = row.Rate;
                                spr1.TaxAmount = row.TaxAmount;
                                spr1.Taxname = row.Taxname;

                                //spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Date = row.Date;
                                sprList.Add(spr1);
                            }
                            else
                            {
                                var spr1 = new PurchasebyTax_Result();
                                spr1.Item = row.Item;

                                spr1.Rate = row.Rate;
                                spr1.TaxAmount = row.TaxAmount;
                                //spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Taxname = row.Taxname;

                                spr1.Date = row.Date;
                                sprList.Add(spr1);
                                var spr2 = new PurchasebyTax_Result();
                                spr2.InvoiceNo = "Grand Total:";
                                spr2.Rate = ratetotal;
                                spr2.TaxAmount = amounttotal;
                                sprList.Add(spr2);
                            }
                            oldproduct = row.Taxname;
                            counter++;

                        }
                    }

                }




                return PartialView(sprList);
            }

        }



        [HttpGet]
        public ActionResult PurchaseTaxPDF(int comp, long branchid, int id, string from, string to)
        {

            //int id = 1; string value = "0"; string from = "0"; string to = "0";
            int compid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);


            var detls = db.Companies.Where(c => c.Id == comp).FirstOrDefault();

            ViewBag.Name = detls.Name;
            ViewBag.address = detls.Address;
            ViewBag.pin = detls.Zipcode;

            try
            {
                var branch = db.BranchMasters.Where(c => c.Id == branchid).FirstOrDefault();
                ViewBag.branchname = branch.Name;
            }
            catch { }



            int companyid = Convert.ToInt32(Session["companyid"]);


            //   long Branchid = Convert.ToInt64(Session["BranchId"]);

            Session["fdate"] = from;
            Session["tdate"] = to;

            Session["productid"] = id.ToString();
            if (id != 0)
            {


                DateTime? fromDate = DateTime.Parse(Convert.ToString(from));
                DateTime? toDate = DateTime.Parse(Convert.ToString(to));


                ViewBag.from = fromDate;
                ViewBag.to = toDate;

                List<PurchasebyTax_Result> sprList = new List<PurchasebyTax_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal ratetotal = 0;
                decimal amounttotal = 0;
                var result = db.PurchasebyTax(id, fromDate, toDate, comp, Branchid).ToList();
                if (result.Count != 0)
                {


                    foreach (var row in result)
                    {
                        if (oldproduct != row.Taxname)
                        {
                            counter = 0;

                            ratetotal = result.Where(r => r.Taxname == row.Taxname).Sum(r => r.Rate);
                            amounttotal = result.Where(r => r.Taxname == row.Taxname).Sum(r => r.TaxAmount);
                            productcount = result.Where(r => r.Taxname == row.Taxname).Count() - 1;
                            if (counter == productcount)
                            {
                                if (counter == 0)
                                {
                                    var spr = new PurchasebyTax_Result();
                                    spr.Taxname = row.Taxname;
                                    spr.Item = "Header";
                                    spr.Rate = ratetotal;
                                    spr.TaxAmount = amounttotal;
                                    sprList.Add(spr);
                                    var spr1 = new PurchasebyTax_Result();
                                    spr1.Item = row.Item;
                                    spr1.Rate = row.Rate;
                                    spr1.Taxname = row.Taxname;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchasebyTax_Result();
                                    spr2.InvoiceNo = "Grand Total:";
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new PurchasebyTax_Result();
                                    spr1.Item = row.Item;
                                    spr1.Rate = row.Rate;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.Taxname = row.Taxname;

                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchasebyTax_Result();
                                    spr2.InvoiceNo = "Grand Total:";
                                    spr2.Rate = ratetotal;
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.Taxname;
                            }
                            else
                            {
                                var spr = new PurchasebyTax_Result();

                                spr.Taxname = row.Taxname;
                                spr.TaxAmount = amounttotal;
                                spr.Item = "Header";
                                spr.Rate = row.Rate;
                                sprList.Add(spr);
                                var spr1 = new PurchasebyTax_Result();
                                spr1.Item = row.Item;
                                spr1.TaxAmount = row.TaxAmount;
                                spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Date = row.Date;
                                spr1.Taxname = row.Taxname;
                                sprList.Add(spr1);

                                counter++;
                                oldproduct = row.Taxname;
                            }


                        }
                        else
                        {


                            if (counter != productcount)
                            {

                                var spr1 = new PurchasebyTax_Result();
                                spr1.Item = row.Item;
                                spr1.Taxname = row.Taxname;
                                spr1.Rate = row.Rate;
                                spr1.TaxAmount = row.TaxAmount;
                                spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Date = row.Date;
                                sprList.Add(spr1);
                            }
                            else
                            {
                                var spr1 = new PurchasebyTax_Result();
                                spr1.Item = row.Item;
                                spr1.Rate = row.Rate;
                                spr1.TaxAmount = row.TaxAmount;
                                spr1.Taxname = row.Taxname;
                                spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Date = row.Date;
                                sprList.Add(spr1);
                                var spr2 = new PurchasebyTax_Result();
                                spr2.InvoiceNo = "Grand Total:";
                                spr2.Rate = ratetotal;
                                spr2.TaxAmount = amounttotal;
                                sprList.Add(spr2);
                            }
                            oldproduct = row.Taxname;
                            counter++;

                        }
                    }

                }




                return View(sprList);
            }

            else
            {

                DateTime? fromDate = null;
                DateTime? toDate = null;


                List<PurchasebyTax_Result> sprList = new List<PurchasebyTax_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal ratetotal = 0;
                decimal amounttotal = 0;
                var result = db.PurchasebyTax(id, fromDate, toDate, comp, Branchid).ToList();




                if (result.Count != 0)
                {

                    foreach (var row in result)
                    {
                        if (oldproduct != row.Taxname)
                        {
                            counter = 0;
                            ratetotal = result.Where(r => r.Item == row.Taxname).Sum(r => r.Rate);
                            amounttotal = result.Where(r => r.Item == row.Taxname).Sum(r => r.TaxAmount);
                            productcount = result.Where(r => r.Item == row.Taxname).Count() - 1;
                            if (counter == productcount)
                            {
                                if (counter == 0)
                                {
                                    var spr = new PurchasebyTax_Result();

                                    spr.Item = "Header";
                                    spr.Taxname = row.Taxname;
                                    spr.Rate = ratetotal;
                                    spr.TaxAmount = amounttotal;
                                    sprList.Add(spr);
                                    var spr1 = new PurchasebyTax_Result();
                                    spr1.Item = row.Item;
                                    spr.Taxname = row.Taxname;
                                    spr.Rate = row.Rate;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchasebyTax_Result();
                                    //     spr2.InvoiceNo = "Grand Total:";
                                    spr2.Rate = ratetotal;
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new PurchasebyTax_Result();
                                    spr1.Item = row.Item;
                                    spr1.Rate = row.Rate;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Taxname = row.Taxname;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchasebyTax_Result();
                                    //        spr2.InvoiceNo = "Grand Total:";
                                    spr2.Rate = ratetotal;
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.Taxname;
                            }
                            else
                            {
                                var spr = new PurchasebyTax_Result();
                                spr.Item = "Header";
                                spr.Taxname = row.Taxname;
                                spr.Rate = ratetotal;
                                sprList.Add(spr);

                                var spr1 = new PurchasebyTax_Result();
                                spr1.Item = row.Item;

                                spr.Taxname = row.Taxname;
                                spr.Rate = row.Rate;

                                spr1.TaxAmount = row.TaxAmount;

                                //spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Date = row.Date;
                                sprList.Add(spr1);

                                counter++;
                                oldproduct = row.Taxname;
                            }


                        }
                        else
                        {


                            if (counter != productcount)
                            {

                                var spr1 = new PurchasebyTax_Result();
                                spr1.Item = row.Item;

                                spr1.Rate = row.Rate;
                                spr1.TaxAmount = row.TaxAmount;
                                spr1.Taxname = row.Taxname;

                                //spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Date = row.Date;
                                sprList.Add(spr1);
                            }
                            else
                            {
                                var spr1 = new PurchasebyTax_Result();
                                spr1.Item = row.Item;

                                spr1.Rate = row.Rate;
                                spr1.TaxAmount = row.TaxAmount;
                                //spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Taxname = row.Taxname;

                                spr1.Date = row.Date;
                                sprList.Add(spr1);
                                var spr2 = new PurchasebyTax_Result();
                                spr2.InvoiceNo = "Grand Total:";
                                spr2.Rate = ratetotal;
                                spr2.TaxAmount = amounttotal;
                                sprList.Add(spr2);
                            }
                            oldproduct = row.Taxname;
                            counter++;

                        }
                    }

                }




                return View(sprList);

            }


        }




        [HttpGet]
        public ActionResult PurchasebyTaxPDFlink()
        {
            int compid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            string ProdId = Session["productid"].ToString();

            try
            {
                string from = Session["fdate"].ToString();
                string to = Session["tdate"].ToString();
                return new ActionAsPdf("PurchaseTaxPDF", new { comp = compid, branchid = Branchid, id = ProdId, from = from, to = to }) { FileName = "PurchaseTax.pdf" };
            }
            catch
            {
                return new ActionAsPdf("PurchaseTaxPDF", new { comp = compid, branchid = Branchid, id = ProdId, from = 0, to = 0 }) { FileName = "PurchaseTax.pdf" };
            }

        }


        [HttpGet]
        public ActionResult PurchaseTaxPrint(int id)
        {


            int compid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);


            var detls = db.Companies.Where(c => c.Id == compid).FirstOrDefault();

            ViewBag.Name = detls.Name;
            ViewBag.address = detls.Address;
            ViewBag.pin = detls.Zipcode;

            try
            {
                var branch = db.BranchMasters.Where(c => c.Id == Branchid).FirstOrDefault();
                ViewBag.branchname = branch.Name;
            }
            catch { }




            if (id != 0)
            {
                string from = Session["fdate"].ToString();
                string to = Session["tdate"].ToString();


                DateTime? fromDate = DateTime.Parse(Convert.ToString(from));
                DateTime? toDate = DateTime.Parse(Convert.ToString(to));


                ViewBag.from = fromDate;
                ViewBag.to = toDate;



                int companyid = Convert.ToInt32(Session["companyid"]);


                // long Branchid = Convert.ToInt64(Session["BranchId"]);

                Session["fdate"] = from;
                Session["tdate"] = to;

                Session["productid"] = id.ToString();


                List<PurchasebyTax_Result> sprList = new List<PurchasebyTax_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal ratetotal = 0;
                decimal amounttotal = 0;
                var result = db.PurchasebyTax(id, fromDate, toDate, companyid, Branchid).ToList();
                if (result.Count != 0)
                {


                    foreach (var row in result)
                    {
                        if (oldproduct != row.Taxname)
                        {
                            counter = 0;

                            ratetotal = result.Where(r => r.Taxname == row.Taxname).Sum(r => r.Rate);
                            amounttotal = result.Where(r => r.Taxname == row.Taxname).Sum(r => r.TaxAmount);
                            productcount = result.Where(r => r.Taxname == row.Taxname).Count() - 1;
                            if (counter == productcount)
                            {
                                if (counter == 0)
                                {
                                    var spr = new PurchasebyTax_Result();
                                    spr.Taxname = row.Taxname;
                                    spr.Item = "Header";
                                    spr.Rate = ratetotal;
                                    spr.TaxAmount = amounttotal;
                                    sprList.Add(spr);
                                    var spr1 = new PurchasebyTax_Result();
                                    spr1.Item = row.Item;
                                    spr1.Rate = row.Rate;
                                    spr1.Taxname = row.Taxname;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchasebyTax_Result();
                                    spr2.InvoiceNo = "Grand Total:";
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new PurchasebyTax_Result();
                                    spr1.Item = row.Item;
                                    spr1.Rate = row.Rate;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.Taxname = row.Taxname;

                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchasebyTax_Result();
                                    spr2.InvoiceNo = "Grand Total:";
                                    spr2.Rate = ratetotal;
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.Taxname;
                            }
                            else
                            {
                                var spr = new PurchasebyTax_Result();

                                spr.Taxname = row.Taxname;
                                spr.TaxAmount = amounttotal;
                                spr.Item = "Header";
                                spr.Rate = row.Rate;
                                sprList.Add(spr);
                                var spr1 = new PurchasebyTax_Result();
                                spr1.Item = row.Item;
                                spr1.TaxAmount = row.TaxAmount;
                                spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Date = row.Date;
                                spr1.Taxname = row.Taxname;
                                sprList.Add(spr1);

                                counter++;
                                oldproduct = row.Taxname;
                            }


                        }
                        else
                        {


                            if (counter != productcount)
                            {

                                var spr1 = new PurchasebyTax_Result();
                                spr1.Item = row.Item;
                                spr1.Taxname = row.Taxname;
                                spr1.Rate = row.Rate;
                                spr1.TaxAmount = row.TaxAmount;
                                spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Date = row.Date;
                                sprList.Add(spr1);
                            }
                            else
                            {
                                var spr1 = new PurchasebyTax_Result();
                                spr1.Item = row.Item;
                                spr1.Rate = row.Rate;
                                spr1.TaxAmount = row.TaxAmount;
                                spr1.Taxname = row.Taxname;
                                spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Date = row.Date;
                                sprList.Add(spr1);
                                var spr2 = new PurchasebyTax_Result();
                                spr2.InvoiceNo = "Grand Total:";
                                spr2.Rate = ratetotal;
                                spr2.TaxAmount = amounttotal;
                                sprList.Add(spr2);
                            }
                            oldproduct = row.Taxname;
                            counter++;

                        }
                    }








                }

                return View(sprList);
            }



            else
            {

                DateTime? fromDate = null; DateTime? toDate = null;

                List<PurchasebyTax_Result> sprList = new List<PurchasebyTax_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal ratetotal = 0;
                decimal amounttotal = 0;
                var result = db.PurchasebyTax(id, fromDate, toDate, compid, Branchid).ToList();




                if (result.Count != 0)
                {

                    foreach (var row in result)
                    {
                        if (oldproduct != row.Taxname)
                        {
                            counter = 0;
                            ratetotal = result.Where(r => r.Item == row.Taxname).Sum(r => r.Rate);
                            amounttotal = result.Where(r => r.Item == row.Taxname).Sum(r => r.TaxAmount);
                            productcount = result.Where(r => r.Item == row.Taxname).Count() - 1;
                            if (counter == productcount)
                            {
                                if (counter == 0)
                                {
                                    var spr = new PurchasebyTax_Result();

                                    spr.Item = "Header";
                                    spr.Taxname = row.Taxname;
                                    spr.Rate = ratetotal;
                                    spr.TaxAmount = amounttotal;
                                    sprList.Add(spr);
                                    var spr1 = new PurchasebyTax_Result();
                                    spr1.Item = row.Item;
                                    spr.Taxname = row.Taxname;
                                    spr.Rate = row.Rate;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchasebyTax_Result();
                                    //     spr2.InvoiceNo = "Grand Total:";
                                    spr2.Rate = ratetotal;
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new PurchasebyTax_Result();
                                    spr1.Item = row.Item;
                                    spr1.Rate = row.Rate;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Taxname = row.Taxname;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new PurchasebyTax_Result();
                                    //        spr2.InvoiceNo = "Grand Total:";
                                    spr2.Rate = ratetotal;
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.Taxname;
                            }
                            else
                            {
                                var spr = new PurchasebyTax_Result();
                                spr.Item = "Header";
                                spr.Taxname = row.Taxname;
                                spr.Rate = ratetotal;
                                sprList.Add(spr);

                                var spr1 = new PurchasebyTax_Result();
                                spr1.Item = row.Item;

                                spr.Taxname = row.Taxname;
                                spr.Rate = row.Rate;

                                spr1.TaxAmount = row.TaxAmount;

                                //spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Date = row.Date;
                                sprList.Add(spr1);

                                counter++;
                                oldproduct = row.Taxname;
                            }


                        }
                        else
                        {


                            if (counter != productcount)
                            {

                                var spr1 = new PurchasebyTax_Result();
                                spr1.Item = row.Item;

                                spr1.Rate = row.Rate;
                                spr1.TaxAmount = row.TaxAmount;
                                spr1.Taxname = row.Taxname;

                                //spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Date = row.Date;
                                sprList.Add(spr1);
                            }
                            else
                            {
                                var spr1 = new PurchasebyTax_Result();
                                spr1.Item = row.Item;

                                spr1.Rate = row.Rate;
                                spr1.TaxAmount = row.TaxAmount;
                                //spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Taxname = row.Taxname;

                                spr1.Date = row.Date;
                                sprList.Add(spr1);
                                var spr2 = new PurchasebyTax_Result();
                                spr2.InvoiceNo = "Grand Total:";
                                spr2.Rate = ratetotal;
                                spr2.TaxAmount = amounttotal;
                                sprList.Add(spr2);
                            }
                            oldproduct = row.Taxname;
                            counter++;

                        }
                    }


                }



                return View(sprList);


            }
        }






        #endregion

        #region Purchase Variance Report

        [HttpGet]
        public ActionResult PurchaseVariance(string Msg, string Err)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }
            return View();
        }
        public ActionResult PurchaseVarianceReport(string From, string To)
        {
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? FromDate = null;
            DateTime? ToDate = null;
            if (From != "0")
            {
                FromDate = DateTime.ParseExact(From, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            if (To != "0")
            {
                ToDate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            Session["fdate"] = FromDate;
            Session["tdate"] = ToDate;

            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            var products = //from gr in (
                                 (from po in
                                  db.PurchaseOrderDetails.Where(d => d.PurchaseOrder.Date>=FromDate && d.PurchaseOrder.Date <= ToDate && d.PurchaseOrder.Warehouse.Branchid==Branchid).GroupBy(d => new {d.PurchaseOrderId,d.PurchaseOrder.NO,d.PurchaseOrder.Date,d.PurchaseOrder.Warehouse.Name,Party=d.PurchaseOrder.Supplier.Name,d.PurchaseOrder.Status, Product= d.Product.Name, BranchId = d.PurchaseOrder.BranchId }).Select(p => new PurchaseOrderVarianceDetail {POId=p.Key.PurchaseOrderId, PONO = p.Key.NO,PODate=p.Key.Date,POStatus=p.Key.Status,Party=p.Key.Party,Warehouse=p.Key.Name ,ProductName = p.Key.Product,BranchId = p.Key.BranchId , POQuantity = p.Sum(d => d.Quantity), POPrice = p.Average(d => d.Price), PRQuantity = 0, PIQuantity = 0, PIPrice = 0 }).ToList()
                                  join pr in db.PurchaseReceiptDetails.GroupBy(d => new { d.PurchaseReceipt.ReferenceNo, d.Product.Name }).Select(p => new PurchaseOrderVarianceDetail { POId=  p.Key.ReferenceNo, ProductName = p.Key.Name, POQuantity = 0, POPrice = 0, PRQuantity = p.Sum(d => d.Quantity), PIQuantity = 0, PIPrice = 0 }).ToList()
                                  on new {po.POId, po.ProductName } equals new {pr.POId, pr.ProductName }
                                  into popr
                                  from pr in popr.DefaultIfEmpty()
                                  join pi in db.PurchaseInvoiceDetails.GroupBy(d => new { d.PurchaseInvoice.PurchaseOrderId, d.Product.Name }).Select(p => new PurchaseOrderVarianceDetail { POId = p.Key.PurchaseOrderId, ProductName = p.Key.Name, POQuantity = 0, POPrice = 0, PRQuantity = 0, PIQuantity = p.Sum(d => d.Quantity), PIPrice = p.Average(d => d.Price) }).ToList()
                                  on new { po.POId, po.ProductName } equals new { pi.POId, pi.ProductName }
                                  into popi
                                  from pi in popi.DefaultIfEmpty()
                                  select new PurchaseOrderVarianceDetail { POId = po.POId, PONO = po.PONO, PODate = po.PODate, POStatus = po.POStatus, Party = po.Party, Warehouse = po.Warehouse , ProductId = po.ProductId, ProductName = po.ProductName, POQuantity = po.POQuantity, POPrice = po.POPrice, PRQuantity = pr == null ? 0 : pr.PRQuantity, PIQuantity = pi == null ? 0 : pi.PIQuantity, PIPrice = pi == null ? 0 : pi.PIPrice }).ToList();
            // group gr by new { gr.ProductId, gr.ProductName, gr.MRP, gr.Price } into g
            //  select new PurchaseOrderVariance { ProductId = g.Key.ProductId, ProductName = g.Key.ProductName, MRP = g.Key.MRP, Price = g.Key.Price, PurchaseQuantity = g.Sum(d => d.PurchaseQuantity), SalesReturnQuantity = g.Sum(d => d.SalesReturnQuantity), SalesQuantity = g.Sum(d => d.SalesQuantity), PurchaseReturnQuantity = g.Sum(d => d.PurchaseReturnQuantity) };
            return PartialView(products);
        }

        public ActionResult PurchaseVarianceDetailReport(long id, string product)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            
            ViewBag.PurchaseOrder = db.PurchaseOrderDetails.Where(d => d.PurchaseOrderId == id && d.Product.Name==product).Select(p => new PurchaseOrderVarianceDetail { POId = p.PurchaseOrderId, PONO = p.PurchaseOrder.NO, PODate = p.PurchaseOrder.Date, POStatus = p.PurchaseOrder.Status, Party = p.PurchaseOrder.Supplier.Name, Warehouse = p.PurchaseOrder.Warehouse.Name, ProductId = p.ItemId, ProductName = p.Product.Description, POQuantity = p.Quantity, POPrice = p.Price, PRQuantity = 0, PIQuantity = 0, PIPrice = 0 }).ToList();
            ViewBag.PurchaseReceipt = db.PurchaseReceiptDetails.Where(d => d.PurchaseReceipt.ReferenceNo == id && d.Product.Name == product).Select(p => new PurchaseOrderVarianceDetail { POId = p.PurchaseReceipt.ReferenceNo, CHId = p.PurchaseReceiptId, CHNO = p.PurchaseReceipt.NO, CHDate = p.PurchaseReceipt.ReceiptDate, PartyCHNO = p.PurchaseReceipt.ReferenceChallan, TruckNO = p.PurchaseReceipt.LorryNo, CHStatus = p.PurchaseReceipt.Status, Party = p.PurchaseReceipt.Supplier.Name, Warehouse = p.PurchaseReceipt.Warehouse.Name, ProductId = p.ItemId, ProductName = p.Product.Description, POQuantity = 0, POPrice = 0, PRQuantity = p.Quantity, PIQuantity = 0, PIPrice = 0 }).ToList();
            ViewBag.PurchaseInvoice = db.PurchaseInvoiceDetails.Where(d => d.PurchaseInvoice.PurchaseOrderId == id && d.Product.Name == product).Select(p => new PurchaseOrderVarianceDetail { POId = p.PurchaseInvoice.PurchaseOrderId, InvoiceId = p.PurchaseInvoiceId, InvoiceNO = p.PurchaseInvoice.NO, InvoiceDate = p.PurchaseInvoice.InvoiceDate, PartyInvoiceNO = p.PurchaseInvoice.ReferenceInvoice, Party = p.PurchaseInvoice.Supplier.Name, ReferenceParty=p.PurchaseInvoice.Reference,Warehouse = p.PurchaseInvoice.Warehouse.Name, ProductId = p.ItemId, ProductName = p.Product.Description, POQuantity = 0, POPrice = 0, PRQuantity = 0, PIQuantity = p.Quantity, PIPrice = p.Price,SubTotal=p.PurchaseInvoice.SubTotal,TaxTotal=p.PurchaseInvoice.TaxProduct,AddedCost=p.PurchaseInvoice.TotalAddAmount,Deduction=p.PurchaseInvoice.TotalDeductAmount,RoundOff=p.PurchaseInvoice.RoundOff,GrandTotal=p.PurchaseInvoice.BCGrandTotal}).ToList();

            return View();

        }
        public ActionResult PurchaseVariancePDF(string from, string to)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            //   string val = Session["datetime"].ToString();
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? From = null;
            DateTime? To = null;
            try
            {
                From = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                To = DateTime.ParseExact(to, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                ViewBag.from =from;
                ViewBag.to =to;
                using (InventoryEntities db = new InventoryEntities())
                {
                    if (Branchid == 0)
                    {
                        var result = (from po in
                                  db.PurchaseOrderDetails.Where(d => d.PurchaseOrder.Date >= From && d.PurchaseOrder.Date <= To).GroupBy(d => new { d.PurchaseOrderId, d.PurchaseOrder.NO, d.PurchaseOrder.Date, d.PurchaseOrder.Warehouse.Name, Party = d.PurchaseOrder.Supplier.Name, d.PurchaseOrder.Status, Product = d.Product.Name }).Select(p => new PurchaseOrderVarianceDetail { POId = p.Key.PurchaseOrderId, PONO = p.Key.NO, PODate = p.Key.Date, POStatus = p.Key.Status, Party = p.Key.Party, Warehouse = p.Key.Name, ProductName = p.Key.Product, POQuantity = p.Sum(d => d.Quantity), POPrice = p.Average(d => d.Price), PRQuantity = 0, PIQuantity = 0, PIPrice = 0 }).ToList()
                                      join pr in db.PurchaseReceiptDetails.GroupBy(d => new { d.PurchaseReceipt.ReferenceNo, d.Product.Name }).Select(p => new PurchaseOrderVarianceDetail { POId = p.Key.ReferenceNo, ProductName = p.Key.Name, POQuantity = 0, POPrice = 0, PRQuantity = p.Sum(d => d.Quantity), PIQuantity = 0, PIPrice = 0 }).ToList()
                                      on new { po.POId, po.ProductName } equals new { pr.POId, pr.ProductName }
                                      into popr
                                      from pr in popr.DefaultIfEmpty()
                                      join pi in db.PurchaseInvoiceDetails.GroupBy(d => new { d.PurchaseInvoice.PurchaseOrderId, d.Product.Name }).Select(p => new PurchaseOrderVarianceDetail { POId = p.Key.PurchaseOrderId, ProductName = p.Key.Name, POQuantity = 0, POPrice = 0, PRQuantity = 0, PIQuantity = p.Sum(d => d.Quantity), PIPrice = p.Average(d => d.Price) }).ToList()
                                      on new { po.POId, po.ProductName } equals new { pi.POId, pi.ProductName }
                                      into popi
                                      from pi in popi.DefaultIfEmpty()
                                      select new PurchaseOrderVarianceDetail { POId = po.POId, PONO = po.PONO, PODate = po.PODate, POStatus = po.POStatus, Party = po.Party, Warehouse = po.Warehouse, ProductId = po.ProductId, ProductName = po.ProductName, POQuantity = po.POQuantity, POPrice = po.POPrice, PRQuantity = pr == null ? 0 : pr.PRQuantity, PIQuantity = pi == null ? 0 : pi.PIQuantity, PIPrice = pi == null ? 0 : pi.PIPrice }).ToList();
                        return new Rotativa.PartialViewAsPdf(result)
                        {
                            FileName = "PurchaseOrderVariance.pdf",
                            PageSize = Size.A4,
                            PageMargins = new Margins(10, 10, 10, 10),
                            CustomSwitches = "--footer-right [page]/[topage]"//customSwitches
                            // PageMargins = new Margins(0, 0, 0, 0),
                            //CustomSwitches = "--print-media-type  --footer-center [page] --footer-font-size 8"

                        };
                    }
                    else
                    {
                        var result = (from po in
                                  db.PurchaseOrderDetails.Where(d => d.PurchaseOrder.Date >= From && d.PurchaseOrder.Date <= To && d.PurchaseOrder.Warehouse.Branchid == Branchid).GroupBy(d => new { d.PurchaseOrderId, d.PurchaseOrder.NO, d.PurchaseOrder.Date, d.PurchaseOrder.Warehouse.Name, Party = d.PurchaseOrder.Supplier.Name, d.PurchaseOrder.Status, Product = d.Product.Name }).Select(p => new PurchaseOrderVarianceDetail { POId = p.Key.PurchaseOrderId, PONO = p.Key.NO, PODate = p.Key.Date, POStatus = p.Key.Status, Party = p.Key.Party, Warehouse = p.Key.Name, ProductName = p.Key.Product, POQuantity = p.Sum(d => d.Quantity), POPrice = p.Average(d => d.Price), PRQuantity = 0, PIQuantity = 0, PIPrice = 0 }).ToList()
                                      join pr in db.PurchaseReceiptDetails.GroupBy(d => new { d.PurchaseReceipt.ReferenceNo, d.Product.Name }).Select(p => new PurchaseOrderVarianceDetail { POId = p.Key.ReferenceNo, ProductName = p.Key.Name, POQuantity = 0, POPrice = 0, PRQuantity = p.Sum(d => d.Quantity), PIQuantity = 0, PIPrice = 0 }).ToList()
                                      on new { po.POId, po.ProductName } equals new { pr.POId, pr.ProductName }
                                      into popr
                                      from pr in popr.DefaultIfEmpty()
                                      join pi in db.PurchaseInvoiceDetails.GroupBy(d => new { d.PurchaseInvoice.PurchaseOrderId, d.Product.Name }).Select(p => new PurchaseOrderVarianceDetail { POId = p.Key.PurchaseOrderId, ProductName = p.Key.Name, POQuantity = 0, POPrice = 0, PRQuantity = 0, PIQuantity = p.Sum(d => d.Quantity), PIPrice = p.Average(d => d.Price) }).ToList()
                                      on new { po.POId, po.ProductName } equals new { pi.POId, pi.ProductName }
                                      into popi
                                      from pi in popi.DefaultIfEmpty()
                                      select new PurchaseOrderVarianceDetail { POId = po.POId, PONO = po.PONO, PODate = po.PODate, POStatus = po.POStatus, Party = po.Party, Warehouse = po.Warehouse, ProductId = po.ProductId, ProductName = po.ProductName, POQuantity = po.POQuantity, POPrice = po.POPrice, PRQuantity = pr == null ? 0 : pr.PRQuantity, PIQuantity = pi == null ? 0 : pi.PIQuantity, PIPrice = pi == null ? 0 : pi.PIPrice }).ToList();
                        return new Rotativa.PartialViewAsPdf(result)
                        {
                            FileName = "PurchaseOrderVariance.pdf",
                            PageSize = Size.A4,
                            PageMargins = new Margins(10, 10, 10, 10),
                            CustomSwitches = "--footer-right [page]/[topage]"//customSwitches
                            // PageMargins = new Margins(0, 0, 0, 0),
                            //CustomSwitches = "--print-media-type  --footer-center [page] --footer-font-size 8"

                        };
                    }
                }
            }
            catch
            {
                return new ActionAsPdf("PurchaseOrderVariance", new { id = Branchid, value = "0", From = From, To = To }) { FileName = "PurchaseOrderVariance.pdf" };
            }


        }

        [HttpGet]
        public ActionResult PurchaseUpVariance(string Msg, string Err)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }
            return View();
        }
        public ActionResult PurchaseUpVarianceReport(string From, string To)
        {
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? FromDate = null;
            DateTime? ToDate = null;
            if (From != "0")
            {
                FromDate = DateTime.ParseExact(From, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            if (To != "0")
            {
                ToDate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            Session["fdate"] = FromDate;
            Session["tdate"] = ToDate;

            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            var products = //from gr in (
                                 (from po in
                                  db.PurchaseOrderDetails.Where(d => d.PurchaseOrder.Date >= FromDate && d.PurchaseOrder.Date <= ToDate).GroupBy(d => new { d.PurchaseOrderId, d.PurchaseOrder.NO, d.PurchaseOrder.Date, d.PurchaseOrder.Warehouse.Name, Party = d.PurchaseOrder.Supplier.Name, d.PurchaseOrder.Status, Product = d.Product.Name }).Select(p => new PurchaseOrderVarianceDetail { POId = p.Key.PurchaseOrderId, PONO = p.Key.NO, PODate = p.Key.Date, POStatus = p.Key.Status, Party = p.Key.Party, Warehouse = p.Key.Name, ProductName = p.Key.Product, POQuantity = p.Sum(d => d.Quantity), POPrice = p.Average(d => d.Price), PRQuantity = 0, PIQuantity = 0, PIPrice = 0 }).ToList()
                                  join pr in db.PurchaseReceiptDetails.GroupBy(d => new { d.PurchaseReceipt.ReferenceNo, d.Product.Name }).Select(p => new PurchaseOrderVarianceDetail { POId = p.Key.ReferenceNo, ProductName = p.Key.Name, POQuantity = 0, POPrice = 0, PRQuantity = p.Sum(d => d.Quantity), PIQuantity = 0, PIPrice = 0 }).ToList()
                                  on new { po.POId, po.ProductName } equals new { pr.POId, pr.ProductName }
                                  into popr
                                  from pr in popr.DefaultIfEmpty()
                                  join pi in db.PurchaseInvoiceDetails.GroupBy(d => new { d.PurchaseInvoice.PurchaseOrderId, d.Product.Name }).Select(p => new PurchaseOrderVarianceDetail { POId = p.Key.PurchaseOrderId, ProductName = p.Key.Name, POQuantity = 0, POPrice = 0, PRQuantity = 0, PIQuantity = p.Sum(d => d.Quantity), PIPrice = p.Average(d => d.Price) }).ToList()
                                  on new { po.POId, po.ProductName } equals new { pi.POId, pi.ProductName }
                                  into popi
                                  from pi in popi.DefaultIfEmpty()
                                  where pi!=null && po.POPrice < (pi==null?0:pi.PIPrice)
                                  select new PurchaseOrderVarianceDetail { POId = po.POId, PONO = po.PONO, PODate = po.PODate, POStatus = po.POStatus, Party = po.Party, Warehouse = po.Warehouse, ProductId = po.ProductId, ProductName = po.ProductName, POQuantity = po.POQuantity, POPrice = po.POPrice, PRQuantity = pr == null ? 0 : pr.PRQuantity, PIQuantity = pi == null ? 0 : pi.PIQuantity, PIPrice = pi == null ? 0 : pi.PIPrice }).ToList();
            // group gr by new { gr.ProductId, gr.ProductName, gr.MRP, gr.Price } into g
            //  select new PurchaseOrderVariance { ProductId = g.Key.ProductId, ProductName = g.Key.ProductName, MRP = g.Key.MRP, Price = g.Key.Price, PurchaseQuantity = g.Sum(d => d.PurchaseQuantity), SalesReturnQuantity = g.Sum(d => d.SalesReturnQuantity), SalesQuantity = g.Sum(d => d.SalesQuantity), PurchaseReturnQuantity = g.Sum(d => d.PurchaseReturnQuantity) };
            return PartialView(products);
        }
        [HttpGet]
        public ActionResult PurchaseDownVariance(string Msg, string Err)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }
            return View();
        }
        public ActionResult PurchaseDownVarianceReport(string From, string To)
        {
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? FromDate = null;
            DateTime? ToDate = null;
            if (From != "0")
            {
                FromDate = DateTime.ParseExact(From, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            if (To != "0")
            {
                ToDate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            Session["fdate"] = FromDate;
            Session["tdate"] = ToDate;

            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            var products = //from gr in (
                                 (from po in
                                  db.PurchaseOrderDetails.Where(d => d.PurchaseOrder.Date >= FromDate && d.PurchaseOrder.Date <= ToDate).GroupBy(d => new { d.PurchaseOrderId, d.PurchaseOrder.NO, d.PurchaseOrder.Date, d.PurchaseOrder.Warehouse.Name, Party = d.PurchaseOrder.Supplier.Name, d.PurchaseOrder.Status, Product = d.Product.Name }).Select(p => new PurchaseOrderVarianceDetail { POId = p.Key.PurchaseOrderId, PONO = p.Key.NO, PODate = p.Key.Date, POStatus = p.Key.Status, Party = p.Key.Party, Warehouse = p.Key.Name, ProductName = p.Key.Product, POQuantity = p.Sum(d => d.Quantity), POPrice = p.Average(d => d.Price), PRQuantity = 0, PIQuantity = 0, PIPrice = 0 }).ToList()
                                  join pr in db.PurchaseReceiptDetails.GroupBy(d => new { d.PurchaseReceipt.ReferenceNo, d.Product.Name }).Select(p => new PurchaseOrderVarianceDetail { POId = p.Key.ReferenceNo, ProductName = p.Key.Name, POQuantity = 0, POPrice = 0, PRQuantity = p.Sum(d => d.Quantity), PIQuantity = 0, PIPrice = 0 }).ToList()
                                  on new { po.POId, po.ProductName } equals new { pr.POId, pr.ProductName }
                                  into popr
                                  from pr in popr.DefaultIfEmpty()
                                  join pi in db.PurchaseInvoiceDetails.GroupBy(d => new { d.PurchaseInvoice.PurchaseOrderId, d.Product.Name }).Select(p => new PurchaseOrderVarianceDetail { POId = p.Key.PurchaseOrderId, ProductName = p.Key.Name, POQuantity = 0, POPrice = 0, PRQuantity = 0, PIQuantity = p.Sum(d => d.Quantity), PIPrice = p.Average(d => d.Price) }).ToList()
                                  on new { po.POId, po.ProductName } equals new { pi.POId, pi.ProductName }
                                  into popi
                                  from pi in popi.DefaultIfEmpty()
                                  where pi != null && po.POPrice > (pi == null ? 0 : pi.PIPrice)
                                  select new PurchaseOrderVarianceDetail { POId = po.POId, PONO = po.PONO, PODate = po.PODate, POStatus = po.POStatus, Party = po.Party, Warehouse = po.Warehouse, ProductId = po.ProductId, ProductName = po.ProductName, POQuantity = po.POQuantity, POPrice = po.POPrice, PRQuantity = pr == null ? 0 : pr.PRQuantity, PIQuantity = pi == null ? 0 : pi.PIQuantity, PIPrice = pi == null ? 0 : pi.PIPrice }).ToList();
            // group gr by new { gr.ProductId, gr.ProductName, gr.MRP, gr.Price } into g
            //  select new PurchaseOrderVariance { ProductId = g.Key.ProductId, ProductName = g.Key.ProductName, MRP = g.Key.MRP, Price = g.Key.Price, PurchaseQuantity = g.Sum(d => d.PurchaseQuantity), SalesReturnQuantity = g.Sum(d => d.SalesReturnQuantity), SalesQuantity = g.Sum(d => d.SalesQuantity), PurchaseReturnQuantity = g.Sum(d => d.PurchaseReturnQuantity) };
            return PartialView(products);
        }
        #endregion

        #region Purchase Register
        public ActionResult PurchaseRegister()
        {
            return View();
        }
        public ActionResult PurchaseRegisterReport(string From, string To)
        {
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? FromDate = null;
            DateTime? ToDate = null;
            if (From != "0")
            {
                FromDate = DateTime.ParseExact(From, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            if (To != "0")
            {
                ToDate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            Session["fdate"] = FromDate;
            Session["tdate"] = ToDate;

            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            List<PurchaseReceiptDetail> result = new List<PurchaseReceiptDetail>();
            if (From == To)
            {
                ViewBag.DateRange = "For " + From;
            }
            else
            {
                ViewBag.DateRange = "From " + From + " To " + To; ;
            }
            if (From != "0")
            {
                Session["datetime"] = 1;


                //var result = (from sale in db.SalesInvoices
                //             where  sale.Date >= From && sale.Date <= To && sale.CompanyId == companyid && sale.BranchId == Branchid).ToList();

                if (Branchid == 0)
                    result = db.PurchaseReceiptDetails.Where(s => s.PurchaseReceipt.CompanyId == companyid && s.PurchaseReceipt.ReceiptDate >= FromDate && s.PurchaseReceipt.ReceiptDate <= ToDate).OrderBy(s => s.PurchaseReceipt.ReceiptDate).ToList();
                else
                    result = db.PurchaseReceiptDetails.Where(s => s.PurchaseReceipt.CompanyId == companyid && s.PurchaseReceipt.BranchId == Branchid && s.PurchaseReceipt.ReceiptDate >= FromDate && s.PurchaseReceipt.ReceiptDate <= ToDate).OrderBy(s => s.PurchaseReceipt.ReceiptDate).ToList();

                return PartialView(result);
            }
            else
            {
                Session["datetime"] = 0;

                if (Branchid == 0)
                    result = db.PurchaseReceiptDetails.Where(s => s.PurchaseReceipt.CompanyId == companyid).OrderBy(s => s.PurchaseReceipt.ReceiptDate).ToList();
                else
                    result = db.PurchaseReceiptDetails.Where(s => s.PurchaseReceipt.CompanyId == companyid && s.PurchaseReceipt.BranchId == Branchid).OrderBy(s => s.PurchaseReceipt.ReceiptDate).ToList();

                return PartialView(result);
            }

        }
        [HttpGet]
        public ActionResult PurchasePDFlink()
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            //   string val = Session["datetime"].ToString();
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? From = null;
            DateTime? To = null;
            try
            {
                From = DateTime.ParseExact(Convert.ToString(Session["fdate"]), dateFormat, CultureInfo.CreateSpecificCulture(culture));
                To = DateTime.ParseExact(Convert.ToString(Session["tdate"]), dateFormat, CultureInfo.CreateSpecificCulture(culture));
                ViewBag.from = Convert.ToString(Session["fdate"]);
                ViewBag.to = Convert.ToString(Session["tdate"]);
                using (InventoryEntities db = new InventoryEntities())
                {
                    if (Branchid == 0)
                    {
                        var result = db.SalesDeliveryDetails.Where(s => s.SalesDelivery.CompanyId == companyid && s.SalesDelivery.ReceiptDate >= From && s.SalesDelivery.ReceiptDate <= To).ToList();
                        return new Rotativa.PartialViewAsPdf("DespatchRegisterReport", result)
                        {
                            FileName = "DespatchRegisterReport.pdf",
                            PageSize = Size.A4,
                            PageMargins = new Margins(10, 10, 10, 10),
                            CustomSwitches = "--footer-right [page]/[topage]"//customSwitches
                            // PageMargins = new Margins(0, 0, 0, 0),
                            //CustomSwitches = "--print-media-type  --footer-center [page] --footer-font-size 8"

                        };
                    }
                    else
                    {
                        var result = db.SalesDeliveryDetails.Where(s => s.SalesDelivery.CompanyId == companyid && s.SalesDelivery.BranchId == Branchid && s.SalesDelivery.ReceiptDate >= From && s.SalesDelivery.ReceiptDate <= To).ToList();

                        return new Rotativa.PartialViewAsPdf("DespatchRegisterReport", result)
                        {
                            FileName = "DespatchRegisterReport.pdf",
                            PageSize = Size.A4,
                            PageMargins = new Margins(10, 10, 10, 10),
                            CustomSwitches = "--footer-right [page]/[topage]"//customSwitches
                            // PageMargins = new Margins(0, 0, 0, 0),
                            //CustomSwitches = "--print-media-type  --footer-center [page] --footer-font-size 8"

                        };
                    }
                }
            }
            catch
            {
                return new ActionAsPdf("DespatchRegisterReport", new { id = Branchid, value = "0", From = From, To = To }) { FileName = "DespatchRegisterReport.pdf" };
            }


        }
        #endregion

        #region Payment Against Monthly Purchase
        public ActionResult MonthWisePurchase()
        {
            return View();
        }


        #endregion

        #region ---- Purchase By Product Det--------------

        [HttpGet]
        public ActionResult PurchasebyProductDet(string Msg, string Err)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);

            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }
            //var registrationtype = new SelectList(new[]
            //                           {

            //                                  new{ID="All",Name="All"},
            //                                   new{ID="Registered",Name="Registered"},
            //                                  new{ID="UnRegistered",Name="UnRegistered"},


            //                              },
            //        "ID", "Name");
            //ViewData["registrationtype"] = registrationtype;
            ViewBag.Ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.groupID == 117).Select(d => new { d.LID, d.ledgerName }).ToList();
           
            return View();


        }


        [HttpGet]
        public ActionResult PurchasebyProductDetReport(string From, string To, long? ledgerId = 0)
        {
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? FromDate = null;
            DateTime? ToDate = null;
            if (From != "0")
            {
                FromDate = DateTime.ParseExact(From, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            if (To != "0")
            {
                ToDate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            Session["fdate"] = FromDate;
            Session["tdate"] = ToDate;
            Session["ledgerId"] = ledgerId;
           

            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            List<PurchaseInvoiceDetail> result = null;



            if (From != "0")
            {
                Session["datetime"] = 1;

                if (Branchid == 0)
                {

                    
                        if (ledgerId == 0)
                            result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.InvoiceDate >= FromDate && s.PurchaseInvoice.InvoiceDate <= ToDate).OrderBy(d => d.PurchaseInvoice.InvoiceDate).ToList();
                        else
                            result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.LID == ledgerId && s.PurchaseInvoice.InvoiceDate >= FromDate && s.PurchaseInvoice.InvoiceDate <= ToDate).OrderBy(d => d.PurchaseInvoice.InvoiceDate).ToList();
                    

                }
                else
                {
                   
                        if (ledgerId == 0)
                            result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.BranchId == Branchid && s.PurchaseInvoice.InvoiceDate >= FromDate && s.PurchaseInvoice.InvoiceDate <= ToDate).OrderBy(d => d.PurchaseInvoice.InvoiceDate).ToList();
                        else
                            result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.BranchId == Branchid && s.PurchaseInvoice.LID == ledgerId && s.PurchaseInvoice.InvoiceDate >= FromDate && s.PurchaseInvoice.InvoiceDate <= ToDate).OrderBy(d => d.PurchaseInvoice.InvoiceDate).ToList();
                    
                }

                return PartialView(result);
            }
            else
            {
                Session["datetime"] = 0;

                if (ledgerId == 0)
                    result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.BranchId == Branchid).OrderBy(d => d.PurchaseInvoice.InvoiceDate).ToList();
                else
                    result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.BranchId == Branchid && s.PurchaseInvoice.LID == ledgerId).OrderBy(d => d.PurchaseInvoice.InvoiceDate).ToList();





                return PartialView(result);
            }


        }



        public FileResult PurchaseByProductDetExport()
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt32(Session["companyid"]);

            List<PurchaseInvoiceDetail> result = null;
            string val = Session["datetime"].ToString();
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            //string GST = Session["GST"].ToString();
            int? ledgerId = Convert.ToInt32(Session["ledgerId"].ToString());
           
            DateTime? FromDate = null;
            DateTime? ToDate = null;

            string date = "";

            date = Session["datetime"].ToString();
            //if (date == "1")
            //{


            FromDate = DateTime.Parse(Convert.ToString(Session["fdate"]));
            ToDate = DateTime.Parse(Convert.ToString(Session["tdate"]));
            ViewBag.from = FromDate;
            ViewBag.to = ToDate;

            //Get the data representing the current grid state - page, sort and filter


            if (Branchid == 0)
            {

                
                    if (ledgerId == 0)
                        result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.InvoiceDate >= FromDate && s.PurchaseInvoice.InvoiceDate <= ToDate).OrderBy(d => d.PurchaseInvoice.InvoiceDate).ToList();
                    else
                        result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.LID == ledgerId && s.PurchaseInvoice.InvoiceDate >= FromDate && s.PurchaseInvoice.InvoiceDate <= ToDate).OrderBy(d => d.PurchaseInvoice.InvoiceDate).ToList();
  

            }
            else
            {
                
                    if (ledgerId == 0)
                        result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.BranchId == Branchid && s.PurchaseInvoice.InvoiceDate >= FromDate && s.PurchaseInvoice.InvoiceDate <= ToDate).OrderBy(d => d.PurchaseInvoice.InvoiceDate).ToList();
                    else
                        result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.BranchId == Branchid && s.PurchaseInvoice.LID == ledgerId && s.PurchaseInvoice.InvoiceDate >= FromDate && s.PurchaseInvoice.InvoiceDate <= ToDate).OrderBy(d => d.PurchaseInvoice.InvoiceDate).ToList();
                
            }


            //Create new Excel workbook
            var workbook = new HSSFWorkbook();

            //Create new Excel sheet
            var sheet = workbook.CreateSheet();

            //(Optional) set the width of the columns
            sheet.SetColumnWidth(0, 50 * 56);
            sheet.SetColumnWidth(1, 50 * 56);
            sheet.SetColumnWidth(2, 50 * 56);
            sheet.SetColumnWidth(3, 50 * 56);
            sheet.SetColumnWidth(4, 50 * 156);
            sheet.SetColumnWidth(5, 50 * 156);
            sheet.SetColumnWidth(6, 50 * 86);
            sheet.SetColumnWidth(7, 50 * 86);
            sheet.SetColumnWidth(8, 50 * 86);
            sheet.SetColumnWidth(9, 50 * 86);
            sheet.SetColumnWidth(10, 50 * 86);
            sheet.SetColumnWidth(11, 50 * 86);
            //Create a header row



            // Create a font object and make it bold
            var HeaderCellStyle = workbook.CreateCellStyle();
            // HeaderCellStyle.BorderBottom = BorderStyle.THIN;
            HeaderCellStyle.Alignment = HorizontalAlignment.CENTER;
            var HeaderFont = workbook.CreateFont();
            HeaderFont.Boldweight = (short)FontBoldWeight.BOLD;
            HeaderCellStyle.SetFont(HeaderFont);



            //Create a header row
            var headerRow = sheet.CreateRow(9);
            // getting company details
            var companyDetail = db.Companies.Where(c => c.Id == companyid).FirstOrDefault();
            //Set the column names in the header row          
            int rowNumber = 1;


            //Set the column names in the header row

            var row11 = sheet.CreateRow(rowNumber++);
            var cell = row11.CreateCell(3);
            cell.SetCellValue(companyDetail.Name);

            cell.CellStyle = HeaderCellStyle;
            row11 = sheet.CreateRow(rowNumber++);
            cell = row11.CreateCell(3);
            cell.SetCellValue(companyDetail.Address);
            cell.CellStyle = HeaderCellStyle;

            row11 = sheet.CreateRow(rowNumber++);
            cell = row11.CreateCell(3);
            cell.SetCellValue(companyDetail.Zipcode);
            cell.CellStyle = HeaderCellStyle;

            row11 = sheet.CreateRow(rowNumber++);
            row11 = sheet.CreateRow(rowNumber++);

            cell = row11.CreateCell(3);
            cell.SetCellValue("Purchase By Product Details");
            cell.CellStyle = HeaderCellStyle;

            row11 = sheet.CreateRow(rowNumber++);
            row11 = sheet.CreateRow(rowNumber++);
            cell = row11.CreateCell(3);
            cell.SetCellValue("For " + String.Format(Session["DateFormatUpper"].ToString(), FromDate.Value.ToShortDateString()) + " To " + String.Format(Session["DateFormatUpper"].ToString(), ToDate.Value.ToShortDateString()));


            row11 = sheet.CreateRow(rowNumber++);
            row11 = sheet.CreateRow(rowNumber++);

            // Create a font object and make it bold
            var detailCellStyle = workbook.CreateCellStyle();
            //detailCellStyle.BorderBottom = BorderStyle.Double;
            //detailCellStyle.BorderTop = BorderStyle.THIN;
            var detailFont = workbook.CreateFont();
            detailFont.Boldweight = (short)FontBoldWeight.BOLD;
            detailCellStyle.SetFont(detailFont);



            cell = headerRow.CreateCell(0);
            cell.SetCellValue("SL. No.");
            cell.CellStyle = detailCellStyle;

            cell = headerRow.CreateCell(1);
            cell.SetCellValue("Date");
            cell.CellStyle = detailCellStyle;

            cell = headerRow.CreateCell(2);
            cell.SetCellValue("Bill No.");
            cell.CellStyle = detailCellStyle;

            cell = headerRow.CreateCell(3);
            cell.SetCellValue("Reference Inv.");
            cell.CellStyle = detailCellStyle;

            cell = headerRow.CreateCell(4);
            cell.SetCellValue("Party Name");
            cell.CellStyle = detailCellStyle;

            cell = headerRow.CreateCell(5);
            cell.SetCellValue("Product Description");
            cell.CellStyle = detailCellStyle;

            cell = headerRow.CreateCell(6);
            cell.SetCellValue("Quantity Primary");
            cell.CellStyle = detailCellStyle;

            cell = headerRow.CreateCell(7);
            cell.SetCellValue("Primary Unit");
            cell.CellStyle = detailCellStyle;

            cell = headerRow.CreateCell(8);
            cell.SetCellValue("Quantity Secondary");
            cell.CellStyle = detailCellStyle;

            cell = headerRow.CreateCell(9);
            cell.SetCellValue("Secondary Unit");
            cell.CellStyle = detailCellStyle;

            cell = headerRow.CreateCell(10);
            cell.SetCellValue("Rate");
            cell.CellStyle = detailCellStyle;

            cell = headerRow.CreateCell(11);
            cell.SetCellValue("Taxable Value");
            cell.CellStyle = detailCellStyle;

            cell = headerRow.CreateCell(12);
            cell.SetCellValue("Tax Percentage");
            cell.CellStyle = detailCellStyle;

            cell = headerRow.CreateCell(13);
            cell.SetCellValue("SGST");
            cell.CellStyle = detailCellStyle;

            cell = headerRow.CreateCell(14);
            cell.SetCellValue("CGST");
            cell.CellStyle = detailCellStyle;

            cell = headerRow.CreateCell(15);
            cell.SetCellValue("IGST");
            cell.CellStyle = detailCellStyle;

            cell = headerRow.CreateCell(16);
            cell.SetCellValue("Total");
            cell.CellStyle = detailCellStyle;

            cell = headerRow.CreateCell(17);
            cell.SetCellValue("Branch");
            cell.CellStyle = detailCellStyle;

            sheet.CreateFreezePane(0, 1, 0, 1);



            //Populate the sheet with values from the grid data
            decimal? subtotal = 0;
            decimal? discounttotal = 0;
            decimal? total = 0;
            int serial = 0;
            foreach (var product in result)
            {

                serial++;

                //Create a new row
                var row = sheet.CreateRow(rowNumber++);
                //   total = total + product.Amount;
                //Set values for the cells
                row.CreateCell(0).SetCellValue(serial);
                row.CreateCell(1).SetCellValue(String.Format(Session["DateFormatUpper"].ToString(), product.PurchaseInvoice.InvoiceDate));
               
                row.CreateCell(2).SetCellValue(product.PurchaseInvoice.NO);
                row.CreateCell(3).SetCellValue(product.PurchaseInvoice.ReferenceInvoice);
                row.CreateCell(4).SetCellValue(product.PurchaseInvoice.Supplier.Name);
                row.CreateCell(5).SetCellValue(product.Product.Description);
                row.CreateCell(6).SetCellValue(Math.Round((double)product.Quantity, 2));
                row.CreateCell(7).SetCellValue(product.UOM1.Description);
                row.CreateCell(8).SetCellValue(Math.Round((double)product.UnitFormula, 2));
                row.CreateCell(9).SetCellValue(product.UOM.Description);
                row.CreateCell(10).SetCellValue(Math.Round((double)product.Price, 2));
                var taxAmount = product.Quantity * product.Price;
                row.CreateCell(11).SetCellValue(Math.Round((double)taxAmount, 2));
                row.CreateCell(12).SetCellValue(Math.Round((double)product.TaxPercent,2));
                if(product.Tax.Name.Substring(0,1)=="G")
                {
                    //decimal divideVal = (product.TaxPercent/2);
                    decimal sgst = product.TaxAmount / 2;
                    row.CreateCell(13).SetCellValue(Math.Round((double)sgst, 2));
                    row.CreateCell(14).SetCellValue(Math.Round((double)sgst, 2));
                    row.CreateCell(15).SetCellValue(Math.Round((double)0, 2));

                }

                else if (product.Tax.Name.Substring(0, 1) == "I")
                {
                    row.CreateCell(13).SetCellValue(Math.Round((double)0, 2));
                    row.CreateCell(14).SetCellValue(Math.Round((double)0, 2));
                    row.CreateCell(15).SetCellValue(Math.Round((double)product.TaxAmount, 2));
                }

                else
                {
                    row.CreateCell(13).SetCellValue(Math.Round((double)0, 2));
                    row.CreateCell(14).SetCellValue(Math.Round((double)0, 2));
                    row.CreateCell(15).SetCellValue(Math.Round((double)0, 2));
                }
               
                row.CreateCell(16).SetCellValue(Math.Round((double)product.TotalAmount, 2));
                row.CreateCell(17).SetCellValue(product.PurchaseInvoice.Warehouse.Name);

            }
            //var rowTotal = sheet.CreateRow(rowNumber++);

            //var expense = rowTotal.CreateCell(1);
            //expense.SetCellType(CellType.NUMERIC);
            //expense.SetCellValue("Grand Total");
            //expense.CellStyle = detailCellStyle;

            //var currencyCellStyle = workbook.CreateCellStyle();
            //// Right-align currency values
            //currencyCellStyle.Alignment = HorizontalAlignment.RIGHT;

            //var detailFonts = workbook.CreateFont();
            //detailFonts.Boldweight = (short)FontBoldWeight.BOLD;
            //currencyCellStyle.SetFont(detailFonts);



            //// Get / create the data format string
            //var formatId = HSSFDataFormat.GetBuiltinFormat("#,##0.00");
            //if (formatId == -1)
            //{
            //    var newDataFormat = workbook.CreateDataFormat();
            //    currencyCellStyle.DataFormat = newDataFormat.GetFormat("#,##0.00");
            //}
            //else
            //    currencyCellStyle.DataFormat = formatId;

            //var jan = rowTotal.CreateCell(6);
            //jan.SetCellType(CellType.FORMULA);
            //jan.CellFormula = "SUM(" + total + ")";
            //jan.CellStyle = currencyCellStyle;
            //jan.SetCellValue(Convert.ToDouble(total));
            sheet.CreateFreezePane(0, 1, 0, 1);

            //Write the workbook to a memory stream
            MemoryStream output = new MemoryStream();
            workbook.Write(output);

            //Return the result to the end user

            return File(output.ToArray(),   //The binary data of the XLS file
                "application/vnd.ms-excel", //MIME type of Excel files
                "PurchaseByProductDetails.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user




            //}
            //else
            //{
            //    DateTime? fromDate = (DateTime?)Session["fdate"];
            //    DateTime? toDate = (DateTime?)Session["tdate"];

            //    //Get the data representing the current grid state - page, sort and filter


            //    var result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid).ToList();


            //    //Create new Excel workbook
            //    var workbook = new HSSFWorkbook();

            //    //Create new Excel sheet
            //    var sheet = workbook.CreateSheet();

            //    //(Optional) set the width of the columns
            //    sheet.SetColumnWidth(0, 50 * 56);
            //    sheet.SetColumnWidth(1, 50 * 56);
            //    sheet.SetColumnWidth(2, 50 * 56);
            //    sheet.SetColumnWidth(3, 50 * 156);
            //    sheet.SetColumnWidth(4, 50 * 56);
            //    sheet.SetColumnWidth(5, 50 * 86);
            //    sheet.SetColumnWidth(6, 50 * 86);
            //    //Create a header row



            //    // Create a font object and make it bold
            //    var HeaderCellStyle = workbook.CreateCellStyle();
            //    // HeaderCellStyle.BorderBottom = BorderStyle.THIN;
            //    HeaderCellStyle.Alignment = HorizontalAlignment.CENTER;
            //    var HeaderFont = workbook.CreateFont();
            //    HeaderFont.Boldweight = (short)FontBoldWeight.BOLD;
            //    HeaderCellStyle.SetFont(HeaderFont);



            //    //Create a header row
            //    var headerRow = sheet.CreateRow(9);
            //    // getting company details
            //    var companyDetail = db.Companies.Where(c => c.Id == companyid).FirstOrDefault();
            //    //Set the column names in the header row          
            //    int rowNumber = 1;


            //    //Set the column names in the header row

            //    var row11 = sheet.CreateRow(rowNumber++);
            //    var cell = row11.CreateCell(3);
            //    cell.SetCellValue(companyDetail.Name);

            //    cell.CellStyle = HeaderCellStyle;
            //    row11 = sheet.CreateRow(rowNumber++);
            //    cell = row11.CreateCell(3);
            //    cell.SetCellValue(companyDetail.Address);
            //    cell.CellStyle = HeaderCellStyle;

            //    row11 = sheet.CreateRow(rowNumber++);
            //    cell = row11.CreateCell(3);
            //    cell.SetCellValue(companyDetail.Zipcode);
            //    cell.CellStyle = HeaderCellStyle;

            //    row11 = sheet.CreateRow(rowNumber++);
            //    row11 = sheet.CreateRow(rowNumber++);

            //    cell = row11.CreateCell(3);
            //    cell.SetCellValue("Sales Till Date");
            //    cell.CellStyle = HeaderCellStyle;

            //    row11 = sheet.CreateRow(rowNumber++);
            //    row11 = sheet.CreateRow(rowNumber++);
            //    cell = row11.CreateCell(3);
            //    //cell.SetCellValue("For " + String.Format(Session["DateFormatUpper"].ToString(), fromDate.Value.ToShortDateString()) + " To " + String.Format(Session["DateFormatUpper"].ToString(), toDate.Value.ToShortDateString()));


            //    row11 = sheet.CreateRow(rowNumber++);
            //    row11 = sheet.CreateRow(rowNumber++);

            //    // Create a font object and make it bold
            //    var detailCellStyle = workbook.CreateCellStyle();
            //    //detailCellStyle.BorderBottom = BorderStyle.Double;
            //    //detailCellStyle.BorderTop = BorderStyle.THIN;
            //    var detailFont = workbook.CreateFont();
            //    detailFont.Boldweight = (short)FontBoldWeight.BOLD;
            //    detailCellStyle.SetFont(detailFont);



            //    cell = headerRow.CreateCell(0);
            //    cell.SetCellValue("SL. No.");
            //    cell.CellStyle = detailCellStyle;

            //    cell = headerRow.CreateCell(1);
            //    cell.SetCellValue("Date");
            //    cell.CellStyle = detailCellStyle;

            //    cell = headerRow.CreateCell(2);
            //    cell.SetCellValue("Bill No.");
            //    cell.CellStyle = detailCellStyle;

            //    cell = headerRow.CreateCell(3);
            //    cell.SetCellValue("Party Name.");
            //    cell.CellStyle = detailCellStyle;

            //    cell = headerRow.CreateCell(4);
            //    cell.SetCellValue("Sub Total");
            //    cell.CellStyle = detailCellStyle;

            //    cell = headerRow.CreateCell(5);
            //    cell.SetCellValue("Discount");
            //    cell.CellStyle = detailCellStyle;

            //    cell = headerRow.CreateCell(6);
            //    cell.SetCellValue("Net Total");
            //    cell.CellStyle = detailCellStyle;


            //    sheet.CreateFreezePane(0, 1, 0, 1);



            //    //Populate the sheet with values from the grid data
            //    decimal? subtotal = 0;
            //    decimal? discounttotal = 0;
            //    decimal? total = 0;
            //    int serial = 0;
            //    foreach (var product in result)
            //    {
            //        var sub = (decimal)(product.BCGrandTotal + product.Dis);
            //        subtotal += sub;
            //        var discount = product.Dis;
            //        discounttotal += discount;
            //        var gtotal = (decimal)(product.BCGrandTotal);
            //        total += gtotal;
            //        serial++;

            //        //Create a new row
            //        var row = sheet.CreateRow(rowNumber++);
            //        //   total = total + product.Amount;
            //        //Set values for the cells
            //        row.CreateCell(0).SetCellValue(serial);
            //        row.CreateCell(1).SetCellValue(String.Format(Session["DateFormatUpper"].ToString(), product.Date));
            //        row.CreateCell(2).SetCellValue(product.NO);
            //        row.CreateCell(3).SetCellValue(product.Customer.Name);
            //        row.CreateCell(4).SetCellValue(Math.Round((double)sub, 2));
            //        row.CreateCell(5).SetCellValue(Math.Round((double)discount, 2));
            //        row.CreateCell(6).SetCellValue(Math.Round((double)gtotal, 2));

            //    }
            //    var rowTotal = sheet.CreateRow(rowNumber++);

            //    var expense = rowTotal.CreateCell(1);
            //    expense.SetCellType(CellType.NUMERIC);
            //    expense.SetCellValue("Grand Total");
            //    expense.CellStyle = detailCellStyle;

            //    var currencyCellStyle = workbook.CreateCellStyle();
            //    // Right-align currency values
            //    currencyCellStyle.Alignment = HorizontalAlignment.RIGHT;

            //    var detailFonts = workbook.CreateFont();
            //    detailFonts.Boldweight = (short)FontBoldWeight.BOLD;
            //    currencyCellStyle.SetFont(detailFonts);



            //    // Get / create the data format string
            //    var formatId = HSSFDataFormat.GetBuiltinFormat("#,##0.00");
            //    if (formatId == -1)
            //    {
            //        var newDataFormat = workbook.CreateDataFormat();
            //        currencyCellStyle.DataFormat = newDataFormat.GetFormat("#,##0.00");
            //    }
            //    else
            //        currencyCellStyle.DataFormat = formatId;

            //    var jan = rowTotal.CreateCell(6);
            //    jan.SetCellType(CellType.FORMULA);
            //    jan.CellFormula = "SUM(" + total + ")";
            //    jan.CellStyle = currencyCellStyle;
            //    jan.SetCellValue(Convert.ToDouble(total));
            //    sheet.CreateFreezePane(0, 1, 0, 1);

            //    //Write the workbook to a memory stream
            //    MemoryStream output = new MemoryStream();
            //    workbook.Write(output);

            //    //Return the result to the end user

            //    return File(output.ToArray(),   //The binary data of the XLS file
            //        "application/vnd.ms-excel", //MIME type of Excel files
            //        "SalesByDate.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user

            //}



        }


        [HttpGet]
        public ActionResult PurchasebyProductDetPDF(int id, string value, string GST, DateTime? From, DateTime? To)
        {

            //int id = 1; string value = "0"; string from = "0"; string to = "0";




            // string date = "";



            if (value == "1")
            {





                ViewBag.from = From;
                ViewBag.to = To;


                //Get the data representing the current grid state - page, sort and filter


                var result = db.SalesInvoices.Where(s => s.BranchId == id && s.Date >= From && s.Date <= To).ToList();
                return View(result);

            }

            else
            {
                var result = db.SalesInvoices.Where(s => s.BranchId == id).ToList();
                return View(result);
            }



        }

        [HttpGet]
        public ActionResult SalesbyProductDetPrint()
        {
            int compid = Convert.ToInt32(Session["companyid"]);

            string val = Session["datetime"].ToString();


            long Branchid = Convert.ToInt64(Session["BranchId"]);


            var detls = db.Companies.Where(c => c.Id == compid).FirstOrDefault();

            ViewBag.Name = detls.Name;
            ViewBag.address = detls.Address;
            ViewBag.pin = detls.Zipcode;

            try
            {
                var branch = db.BranchMasters.Where(c => c.Id == Branchid).FirstOrDefault();
                ViewBag.branchname = branch.Name;
            }
            catch { }



            // string date = "";



            if (val == "1")
            {

                string from = Session["fdate"].ToString();
                string to = Session["tdate"].ToString();


                DateTime fromDate = DateTime.Parse(Convert.ToString(from));
                DateTime toDate = DateTime.Parse(Convert.ToString(to));


                ViewBag.from = fromDate;
                ViewBag.to = toDate;


                //Get the data representing the current grid state - page, sort and filter


                var result = from sale in db.ReceiptPayments
                             where sale.transactionType == "Sales Invoice" && sale.RPdate >= fromDate && sale.RPdate <= toDate && sale.CompanyId == compid && sale.BranchId == Branchid
                             group sale by sale.RPdate into sa

                             select new Salesdate
                             {
                                 Date = sa.Key,
                                 Amount = sa.Sum(sal => sal.TotalAmount)

                             };
                return View(result);

            }

            else
            {
                var result = from sales in db.ReceiptPayments
                             where sales.transactionType == "Sales Invoice" && sales.CompanyId == compid && sales.BranchId == Branchid
                             group sales by sales.RPdate into s
                             select new Salesdate
                             {
                                 Date = s.Key,
                                 Amount = s.Sum(sale => sale.TotalAmount)

                             };
                return View(result);
            }



        }



        [HttpGet]
        public ActionResult PurchasebyProductDetPDFlink()
        {
            // int compid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt32(Session["companyid"]);

            List<PurchaseInvoiceDetail> result = null;
            string val = Session["datetime"].ToString();
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            //string GST = Session["GST"].ToString();
            int? ledgerId = Convert.ToInt32(Session["ledgerId"].ToString());
           
            DateTime? FromDate = null;
            DateTime? ToDate = null;
            try
            {
                FromDate = DateTime.Parse(Convert.ToString(Session["fdate"]));
                ToDate = DateTime.Parse(Convert.ToString(Session["tdate"]));
                ViewBag.from = FromDate;
                ViewBag.to = ToDate;


                //Get the data representing the current grid state - page, sort and filter
                if (Branchid == 0)
                {

                    
                        if (ledgerId == 0)
                            result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.InvoiceDate >= FromDate && s.PurchaseInvoice.InvoiceDate <= ToDate).OrderBy(d => d.PurchaseInvoice.InvoiceDate).ToList();
                        else
                            result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.LID == ledgerId && s.PurchaseInvoice.InvoiceDate >= FromDate && s.PurchaseInvoice.InvoiceDate <= ToDate).OrderBy(d => d.PurchaseInvoice.InvoiceDate).ToList();
                    

                }
                else
                {
                   
                        if (ledgerId == 0)
                            result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.BranchId == Branchid && s.PurchaseInvoice.InvoiceDate >= FromDate && s.PurchaseInvoice.InvoiceDate <= ToDate).OrderBy(d => d.PurchaseInvoice.InvoiceDate).ToList();
                        else
                            result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.BranchId == Branchid && s.PurchaseInvoice.LID == ledgerId && s.PurchaseInvoice.InvoiceDate >= FromDate && s.PurchaseInvoice.InvoiceDate <= ToDate).OrderBy(d => d.PurchaseInvoice.InvoiceDate).ToList();
                    
                }
                //if (Branchid == 0)
                //{
                //    if (GST == "All")
                //        result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.InvoiceDate >= FromDate && s.PurchaseInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.PurchaseInvoice.NO, s.PurchaseInvoice.InvoiceDate, s.PurchaseInvoice.Customer.Name }).Select(s => new PurchaseInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.PurchaseInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.PurchaseInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.PurchaseInvoice.BCGrandTotal), RoundOff = s.Average(d => d.PurchaseInvoice.RoundOff), TaxProduct = s.Average(d => d.PurchaseInvoice.TaxProduct) }).OrderBy(s => s.CreatedOn).ToList();
                //    else if (GST == "Registered")
                //        result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.Customer.GstVatNumber != null && s.PurchaseInvoice.InvoiceDate >= FromDate && s.PurchaseInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.PurchaseInvoice.NO, s.PurchaseInvoice.InvoiceDate, s.PurchaseInvoice.Customer.Name }).Select(s => new PurchaseInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.PurchaseInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.PurchaseInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.PurchaseInvoice.BCGrandTotal), RoundOff = s.Average(d => d.PurchaseInvoice.RoundOff), TaxProduct = s.Average(d => d.PurchaseInvoice.TaxProduct), TaxOther = s.Average(d => d.PurchaseInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                //    else
                //        result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.Customer.GstVatNumber == null && s.PurchaseInvoice.InvoiceDate >= FromDate && s.PurchaseInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.PurchaseInvoice.NO, s.PurchaseInvoice.InvoiceDate, s.PurchaseInvoice.Customer.Name }).Select(s => new PurchaseInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.PurchaseInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.PurchaseInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.PurchaseInvoice.BCGrandTotal), RoundOff = s.Average(d => d.PurchaseInvoice.RoundOff), TaxProduct = s.Average(d => d.PurchaseInvoice.TaxProduct), TaxOther = s.Average(d => d.PurchaseInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();


                //from = DateTime.Parse(Convert.ToString(Session["fdate"]));
                //to = DateTime.Parse(Convert.ToString(Session["tdate"]));

                //return new ActionAsPdf("SalesbyDatePDF", new { id = Branchid, value = "1", From = from, To = to }) { FileName = "SalesByDate.pdf" };
                return new Rotativa.PartialViewAsPdf("PurchasebyProductDetPDF", result)
                {
                    FileName = "PurchasebyProductDetPDF.pdf",
                    PageSize = Size.A4,
                    PageMargins = new Margins(10, 10, 10, 10),
                    CustomSwitches = "--footer-right [page]/[topage]"//customSwitches
                    // PageMargins = new Margins(0, 0, 0, 0),
                    //CustomSwitches = "--print-media-type  --footer-center [page] --footer-font-size 8"

                };
                //}
                //else
                //{
                //    if (GST == "All")
                //        result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.BranchId == Branchid && s.PurchaseInvoice.InvoiceDate >= FromDate && s.PurchaseInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.PurchaseInvoice.NO, s.PurchaseInvoice.InvoiceDate, s.PurchaseInvoice.Customer.Name }).Select(s => new PurchaseInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.PurchaseInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.PurchaseInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.PurchaseInvoice.BCGrandTotal), RoundOff = s.Average(d => d.PurchaseInvoice.RoundOff), TaxProduct = s.Average(d => d.PurchaseInvoice.TaxProduct), TaxOther = s.Average(d => d.PurchaseInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                //    else if (GST == "Registered")
                //        result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.BranchId == Branchid && s.PurchaseInvoice.Customer.GstVatNumber != null && s.PurchaseInvoice.InvoiceDate >= FromDate && s.PurchaseInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.PurchaseInvoice.NO, s.PurchaseInvoice.InvoiceDate, s.PurchaseInvoice.Customer.Name }).Select(s => new PurchaseInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.PurchaseInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.PurchaseInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.PurchaseInvoice.BCGrandTotal), RoundOff = s.Average(d => d.PurchaseInvoice.RoundOff), TaxProduct = s.Average(d => d.PurchaseInvoice.TaxProduct), TaxOther = s.Average(d => d.PurchaseInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                //    else
                //        result = db.PurchaseInvoiceDetails.Where(s => s.PurchaseInvoice.CompanyId == companyid && s.PurchaseInvoice.BranchId == Branchid && s.PurchaseInvoice.Customer.GstVatNumber == null && s.PurchaseInvoice.InvoiceDate >= FromDate && s.PurchaseInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.PurchaseInvoice.NO, s.PurchaseInvoice.InvoiceDate, s.PurchaseInvoice.Customer.Name }).Select(s => new PurchaseInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.PurchaseInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.PurchaseInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.PurchaseInvoice.BCGrandTotal), RoundOff = s.Average(d => d.PurchaseInvoice.RoundOff), TaxProduct = s.Average(d => d.PurchaseInvoice.TaxProduct), TaxOther = s.Average(d => d.PurchaseInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();


                //    return new Rotativa.PartialViewAsPdf("SalesbyDatePDF", result)
                //    {
                //        FileName = "SalesbyDatePDF.pdf",
                //        PageSize = Size.A4,
                //        PageMargins = new Margins(10, 10, 10, 10),
                //        CustomSwitches = "--footer-right [page]/[topage]"//customSwitches
                //                                                         // PageMargins = new Margins(0, 0, 0, 0),
                //                                                         //CustomSwitches = "--print-media-type  --footer-center [page] --footer-font-size 8"

                //    };
                //}
            }
            catch
            {
                return new ActionAsPdf("PurchasebyProductDetPDF", new { id = Branchid, value = "0", From = FromDate, To = ToDate }) { FileName = "PurchaseByProductDet.pdf" };
            }

        }




        [HttpPost]
        public ActionResult PurchasebyProductDetMail(string Mailto, string message, string subj, HttpPostedFileBase fileUploader)
        {
            //string from = Session["fdate"].ToString();
            //   string to = Session["tdate"].ToString();


            //DateTime fromDate = DateTime.Parse(Convert.ToString(from));
            //DateTime toDate = DateTime.Parse(Convert.ToString(to));


            int compid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);


            var detls = db.Companies.Where(c => c.Id == compid).FirstOrDefault();

            ViewBag.Name = detls.Name;
            ViewBag.address = detls.Address;
            ViewBag.pin = detls.Zipcode;

            string[] multimailId = Mailto.Split(',');




            MailMessage mail = new MailMessage();

            foreach (string mailid in multimailId)
            {
                mail.To.Add(new MailAddress(mailid));
            }
            //  mail.To.Add(Mailto);
            mail.From = new MailAddress("cabbooking7@gmail.com", "Sales By Date Report", System.Text.Encoding.UTF8);
            mail.Subject = subj;
            mail.SubjectEncoding = System.Text.Encoding.UTF8;

            string body = "<h3>Sales By Date Report</h3><br/>";

            int slno = 0;
            decimal? amt = 0;
            var result = from sales in db.ReceiptPayments
                         where sales.transactionType == "Sales Invoice" && sales.CompanyId == compid && sales.BranchId == Branchid
                         group sales by sales.RPdate into s
                         select new Salesdate
                         {
                             Date = s.Key,
                             Amount = s.Sum(sale => sale.TotalAmount)

                         };

            body = body + @"<table align='center' bgcolor='#cccccc' cellpadding='0' cellspacing='0' style='width: 100%; background:#cccccc; background-color:#cccccc; margin:0; padding:0 20px;'>
	<tr>
		<td>
		<table align='center' cellpadding='0' cellspacing='0' style='width: 620px; border-collapse:collapse; text-align:left; font-family:Tahoma; font-weight:normal; font-size:12px; line-height:15pt; color:#444444; margin:0 auto;'>
			<!-- Start of logo and top links -->
			<tr>
				<td valign='top' style='height:5px;margin:0;padding:0;;line-height:0;'>
				<img alt='' height='5' src='' vspace='0' style='border:0; padding:0; margin:0; line-height:0;' width='620'/></td>
			</tr>
			<tr>
				<td style=' width:620px;' valign='top'>
					<table cellpadding='0' cellspacing='0' style='width:100%; border-collapse:collapse;font-family:Tahoma; font-weight:normal; font-size:12px; line-height:15pt; color:#444444;' >
						<tr>
							<td bgcolor='#86ac3d' style='width: 320px; padding:10px 0 10px 20px; background:#86ac3d; background-color:#86ac3d; color:#ffffff;' valign='top'>
								<span style='color:#9bc251;'> | </span><a style='color:#485c22; text-decoration:underline;' href='http://www.xenerp.xentricserver.com'>Visit our website </a><span style='color:#9bc251;'> | </span
							</td>
							<td bgcolor='#86ac3d' style='width: 300px; padding:10px 20px 10px 20px; background:#86ac3d; background-color:#86ac3d; text-align:right; color:#ffffff;' valign='top'>
								 Support:  033-568-789
							</td>
						</tr>
						<tr>
							
							<td colspan='2' bgcolor='#FFFFFF' style='width: 350px; padding:20px 20px 15px 20px; background:#ffffff; background-color:#ffffff; text-align:center; font-size:18px;' valign='middle'>
							 Sales By DateWise Report
							</td>
						</tr>
					</table>
				</td>
			</tr>
			<tr>
				<td valign='top' style='height:5px;margin:0;padding:0;line-height:0;'>
				<img alt='' height='5' src='http://democab.xentricserver.com/Images/email/Newsletter_border_Bottom.png' vspace='0' style='border:0; padding:0; margin:0; line-height:0;' width='620'/></td>
			
            </tr>
			<tr>
			<td bgcolor='#FFFFFF' style='padding:10px 20px; background:#ffffff;background-color:#ffffff;' valign='top '>





			<table cellpadding='0' cellspacing='0' style='width: 100%; border-collapse:collapse; font-family:Tahoma; font-weight:normal; font-size:12px; line-height:15pt; color:black;'>
					
					 <thead>
	
            <tr>	

					<tr><td colspan='5' style='width:540px; padding:10px;'> 
                    <p style='padding:0; margin:0 0 11pt 0;line-height:160%; font-size:14px;'>

                            'message'
		
					</td></tr>
					<tr><th colspan='5'><center><H3>From:'+fromDate+'</H3></center><br/><br/></th></tr>	
			<tr>
            <th style='width:340px; padding:0 20px 0 0;'>
								SlNo :
							</th>
							
						
							<th style='width:340px; padding:0 20px 0 0;'>
								Date :
							</th>
							<th valign='middle' align='center' style='text-align:left; width:240px;'>
								Amount:
							</th>
						</tr>
                      </thead>
<tbody>";
            foreach (var sale in result)
            {
                slno = slno + 1;
                amt = amt + sale.Amount;

                body += "<tr><td>" + slno + "</td><td>" + String.Format(Session["DateFormatUpper"].ToString(), sale.Date) + "</td><td> " + sale.Amount + " </td></tr>";

            }
            body += "<tr><td></td><td><h4>Grand Total</h4></td><td><h4> " + amt + " </h4></td></tr>";
            body += @"</tbody></table></td>	<tr>				<td valign='top' style='height:5px;margin:0;padding:0;line-height:0;'>
				<img alt='' height='5' src='http://democab.xentricserver.com/Images/email/Newsletter_border_top.png' vspace='0' style='border:0; padding:0; margin:0; line-height:0;' width='620'/></td>
			</tr>
			<tr>
				<td bgcolor='#FFFFFF' style='padding:10px 20px; background:#ffffff;background-color:#ffffff;' valign='top'>
					<!-- <p style='padding:0; margin:0 0 11pt 0;line-height:160%; font-size:18px;'>
					 <%Name%><br>
					</p>
					<br><br>
					
					Account information</p><br>
                    Your username: <%userName%>
					<br>
					Password :  <%password%><br><br> -->

					 If you need any assistance navigating our website or have any questions please contact us at support@xentriceserver.com
                    <br/>
				</td>
			</tr>
			<tr>
				<td valign='top' style='height:5px;margin:0;padding:0;line-height:0;'>
				<img alt='' height='5' src='http://www.democab.xentricserver.com/Images/email/Newsletter_border_Bottom.png' vspace='0' style='border:0; padding:0; margin:0; line-height:0;' width='620'/></td>
			</tr>
			<!-- End of First Content -->
			
			<!-- Start of Footer -->
			<tr>
				<td valign='top' style='height:5px;margin:0;padding:0;line-height:0;'>
				<img alt='' height='5' src='http://democab.xentricserver.com/Images/email/BottomBackground_Blue_1.png' vspace='0' style='border:0; padding:0; margin:0; line-height:0;' width='620'/></td>
			</tr>
			<tr>
				<td bgcolor='#86ac3d' style='padding:0 20px 15px 20px; background-color:#86ac3d; background:#86ac3d;'>
					<table cellpadding='0' cellspacing='0' style='width: 100%; border-collapse:collapse; font-family:Tahoma; font-weight:normal; font-size:12px; line-height:15pt; color:#FFFFFF;'>
						<br />
                        <tr>
							<td style='width:340px; padding:0 20px 0 0;'>
								Kolkata, West Bengal<br>
								| <a style='color:#485c22; text-decoration:underline;' href='#'>support@xentriceserver.com</a><br/>
								Support:  033-568-789
							</td>
							<td valign='middle' align='center' style='text-align:center; width:240px;'>
								
							</td>
						</tr>
						<tr>
							<td style='padding:20px 0 0 0;' colspan='2'>
								Copyright © 2014 Xentrictechnolgies.
							</td>
						</tr>
					</table>
				</td>
			</tr>
			<tr>
				<td valign='top' style='height:5px;margin:0;padding:0 0 20px 0;line-height:0;'>
				<img alt='' height='5' src='http://democab.xentricserver.com/Images/email/BottomBackground_Blue_2.png' vspace='0' style='border:0; padding:0; margin:0; line-height:0;' width='620'/></td>
			</tr>
</tr></table>";



            mail.Body = body;
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Credentials = new System.Net.NetworkCredential("cabbooking7@gmail.com", "03041983");
            smtp.Port = 587;
            smtp.Host = "smtp.gmail.com";
            smtp.EnableSsl = true;

            try
            {
                smtp.Send(mail);

                //return RedirectToAction("SalesbyDate", new { Msg = "Email Sent Successfully...." });
                return Content("Email Sent Successfully....");
            }
            catch
            {


                return RedirectToAction("PurchasebyProductDetDate", new { Err = "Please try again...." });
            }


        }



        #endregion

        #region Payment Voucher Entry By User

        public ActionResult PayVoucherEntryByUser()
        {
            return View();
        }
        public ActionResult PayVoucherEntryByUserPV(string From, string To)
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            long Createdby = Convert.ToInt32(Session["Createdid"]);
            var dateFormat = Session["DateFormat"].ToString();
            var culture = Session["DateCulture"].ToString();

            DateTime? FromDate = null;
            DateTime? ToDate = null;
            if (From != "0")
            {
                FromDate = DateTime.ParseExact(From, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            if (To != "0")
            {
                ToDate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }

            var result = (from r in db.ReceiptPayments.Where(d=> (d.CompanyId == companyid && d.UserId == userid && d.CreatedBy == Createdby) && (d.transactionType == "General Payment") && (EntityFunctions.TruncateTime(d.RPDatetime) >= FromDate & EntityFunctions.TruncateTime(d.RPDatetime) <= ToDate))
                         join l in db.LedgerMasters on (r.ledgerId ?? 0) equals l.LID
                         join b in db.BranchMasters on r.BranchId equals b.Id
                         select new VoucherModelView { RPdate = r.RPdate, VoucherNo = r.VoucherNo, Prefix = r.Prefix, GeneralLedger = l.ledgerName, ModeOfPay = r.ModeOfPay, TotalAmount = (r.TotalAmount??0), branchCode = b.Name, MoneyReceiptNo = r.CreditCardNo, remarks = r.Remarks, chqDate = r.RPDatetime }).ToList();

            ViewBag.JsonData = result;
            return PartialView(result);
        }

        [HttpPost]
        public ActionResult PayVoucherEntryByUserPVPDFlink(FormCollection frm)
        {
           // var ledgerId = Convert.ToInt32(frm["LedgerId"].ToString());
            var from = frm["FromDate"];
            var To = frm["ToDate"];
            var JsonData = frm["JsonData"];

            string quoted = Regex.Unescape(JsonData);

            List<VoucherModelView> foos = JsonConvert.DeserializeObject<List<VoucherModelView>>(quoted);
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            var dateFormat = Session["DateFormat"].ToString();
            var culture = Session["DateCulture"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            string tbduration = " From  " + from + "   To  " + To;
            using (InventoryEntities db = new InventoryEntities())
            {
                ViewBag.DateRange = tbduration;

                try
                {
                    return new Rotativa.PartialViewAsPdf("PayVoucherEntryByUserPVPDFlink", foos)
                    {
                        FileName = "PaymentVoucherEntry.pdf",
                        PageSize = Size.A4,
                        PageMargins = new Margins(10, 10, 10, 10),
                        PageOrientation = Rotativa.Options.Orientation.Landscape,
                        CustomSwitches = "--footer-right [page]/[topage]"//customSwitches
                                                                         // PageMargins = new Margins(0, 0, 0, 0),
                                                                         //CustomSwitches = "--print-media-type  --footer-center [page] --footer-font-size 8"

                    };
                    //  return new ActionAsPdf("LedgerPDF", new { companyid = companyid, Branchid = Branchid, fid = fid, ledgerId = ledgerId, fdate = fdate, tdate = tdate, DateRange = tbduration, LedgerJson = LedgerJson }) { FileName = "LedgerPDF.pdf" };
                }
                catch
                {
                    return new ActionAsPdf("CashBankLedgerPDFlink", new { from = 0 }) { FileName = "LedgerPDF.pdf" };
                }
            }
        }
        #endregion

        #region Ageing Report
        public ActionResult Aging()
        {
            return View();
        }
        public ActionResult AgingReport(string To)
        {
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? AsOnDate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            //Get All Payments or adjustments for All parties

            var allPayments = (//from c in db.Suppliers.Where(d=>d.LId== 22834).ToList()
                               from c in db.LedgerMasters.Where(d => d.groupID == 104).ToList()
                               join op in db.OpeningBalances.Where(d =>  d.fYearID == (fid - 1)).GroupBy(d => d.ledgerID).Select(d => new  { LID = d.Key, InvoiceId = 0, openingBal = d.Sum(s => s.openingBal) }).ToList()
                               on c.LID equals op.LID into cop
                               from op in cop.DefaultIfEmpty()

                               join opcur in db.OpeningBalances.Where(d => d.fYearID == fid ).GroupBy(d => d.ledgerID).Select(d => new { LID = d.Key, InvoiceId = 0, openingBal = d.Sum(s => s.openingBal) }).ToList()
                               on c.LID equals opcur.LID into copcur
                               from opcur in copcur.DefaultIfEmpty()

                               join rp in db.ReceiptPayments.Where(d => (d.fYearId == (fid - 1) || d.fYearId == fid) && d.transactionType == "General Payment" && d.RPType == "Payment" && d.RPdate <= AsOnDate).GroupBy(d => d.ledgerId).Select(d => new { LId = d.Key, TotalAmount = d.Sum(l => l.TotalAmount) }).ToList()
                               on c.LID equals (int)rp.LId into crp
                               from rp in crp.DefaultIfEmpty()

                               join rp1 in db.ReceiptPayments.Where(d => (d.fYearId == (fid - 1) || d.fYearId == fid) && d.transactionType == "General Receive" && d.RPType == "Receive" && d.RPdate <= AsOnDate).GroupBy(d => d.ledgerId).Select(d => new { LId = d.Key, TotalAmount = d.Sum(l => l.TotalAmount) }).ToList()
                               on c.LID equals (int)rp1.LId into crp1
                               from rp1 in crp1.DefaultIfEmpty()

                               join sr in db.PurchaseReturns.Where(d => (d.FinancialYearId == (fid -1) || d.FinancialYearId == fid) && d.Date <= AsOnDate).GroupBy(d => d.Supplier.LId).Select(d => new { LId = d.Key, BCGrandTotal = d.Sum(l => l.BCGrandTotal) }).ToList()
                               on c.LID equals sr.LId into csr
                               from sr in csr.DefaultIfEmpty()

                               //join cn in db.CreditNotes.Where(d => d.FinancialYearId == fid && d.Date <= AsOnDate).GroupBy(d => d.Supplier.LId).Select(d => new { LId = d.Key, BCGrandTotal = d.Sum(l => l.BCGrandTotal) }).ToList()
                               //on c.LID equals cn.LId into ccn
                               //from cn in ccn.DefaultIfEmpty()

                               join jd in db.JournalDetails.Where(d => (d.Journal.FinancialYearId == (fid - 1) || d.Journal.FinancialYearId == fid) && d.Journal.JournalDate <= AsOnDate).GroupBy(d => d.LID).Select(d => new { LId = d.Key, Debit = d.Sum(l => l.Debit), Credit = d.Sum(l => l.Credit) }).ToList()
                               on c.LID equals jd.LId into cjd
                               from jd in cjd.DefaultIfEmpty()

                               select new TaxComponent { TaxId = c.LID, Amount = (-(op == null ? (opcur == null ? 0 : opcur.openingBal) : op.openingBal) + (rp == null ? 0 : (rp.TotalAmount ?? 0)) + (sr == null ? 0 : sr.BCGrandTotal) - (jd == null ? 0 : (jd.Credit ?? 0)) + (jd == null ? 0 : (jd.Debit ?? 0)) - (rp1 == null ? 0 : (rp1.TotalAmount ?? 0))) }).OrderBy(d => d.TaxId).ToList();

            //Get All This year Invoices of All Parties
            var allInvoices = db.PurchaseInvoices.Where(d => (d.FinancialYearId == (fid - 1) || d.FinancialYearId == fid) && d.Status == InventoryConst.cns_Saved).Select(d => new CustomerInvoice { LID = d.Supplier.LId, InvoiceId = d.Id, InvoiceDate = d.InvoiceDate, InvAmount = d.BCGrandTotal, MaturityDate = EntityFunctions.AddDays(d.InvoiceDate, 30), DaysDue = System.Data.Objects.EntityFunctions.DiffDays(EntityFunctions.AddDays(d.InvoiceDate, 0), AsOnDate) }).OrderBy(d => d.LID).OrderByDescending(d => d.InvoiceDate).ToList();
            //  allInvoices = allInvoices.Select(d => new CustomerInvoice { LID = d.LID, InvoiceId = d.InvoiceId, InvoiceDate = d.InvoiceDate, InvAmount = d.InvAmount, DaysDue = EntityFunctions.DiffDays(AsOnDate, d.MaturityDate) }).ToList();
            //Get All the unpaid invoices of this Year
            DateTime openingDate = (DateTime)(db.FinancialYearMasters.Where(d => d.fYearID == (fid - 1)).Select(d => d.sDate).FirstOrDefault());
           // var opening = db.OpeningBalances.Where(d => d.fYearID == fid).GroupBy(d=>d.ledgerID).Select(d => new CustomerInvoice { LID = d.Key, InvoiceId = 0, InvoiceDate = openingDate, InvAmount = d.Sum(s=>s.openingBal) }).ToList();
            List<CustomerInvoice> getUnpaidInvoices = new List<CustomerInvoice>();
            foreach (var payment in allPayments)
            {
                var eachPartyInvoices = allInvoices.Where(d => d.LID == payment.TaxId).OrderBy(d => d.InvoiceDate).ToList();
                if (eachPartyInvoices.Count() == 0 && payment.Amount != 0)
                {
                    var inv = new CustomerInvoice();
                    inv.LID = payment.TaxId;
                    inv.InvAmount = -payment.Amount;
                    inv.DaysDue = (AsOnDate - openingDate).Value.Days;
                    getUnpaidInvoices.Add(inv);
                }

                var cnte = eachPartyInvoices.Count();
                int cntr = 0;
                foreach (var eachInvoice in eachPartyInvoices)
                {
                    ++cntr;

                    if (payment.Amount >= eachInvoice.InvAmount)
                    {
                        payment.Amount -= eachInvoice.InvAmount;

                        if (payment.Amount > 0 && cnte == cntr)
                        {
                            eachInvoice.InvAmount = -payment.Amount;
                            eachInvoice.DaysDue = (AsOnDate - openingDate).Value.Days;
                            getUnpaidInvoices.Add(eachInvoice);
                        }

                    }
                    else
                    {

                        eachInvoice.InvAmount = (eachInvoice.InvAmount - payment.Amount);
                        getUnpaidInvoices.Add(eachInvoice);
                        payment.Amount = 0;
                    }
                }
            }
            List<CustomerInvoice> getGroupedInvoices = new List<CustomerInvoice>();
            //  var ceilings = new[] { 0, 30, 60, 90, 365 };
            var ceilings = new[] { 30, 60, 90, 750 };
            var groupings = getUnpaidInvoices.GroupBy(item => ceilings.First(ceiling => ceiling >= item.DaysDue)).ToList();
            foreach (var daysRange in groupings)
            {
                getGroupedInvoices.AddRange(daysRange.GroupBy(d => d.LID).Select(d => new CustomerInvoice { InvoiceId = daysRange.Key, LID = d.Key, InvAmount = d.Sum(f => f.InvAmount) }).ToList());
            }
            var orderedInvoices = getGroupedInvoices.OrderBy(d => d.LID).ToList();
            var orderedInvoiceswithParty = (from o in orderedInvoices
                                            join c in db.LedgerMasters.Where(d => d.groupID == 104)
                                            on o.LID equals c.LID
                                            select new { LID = o.LID, Party = c.ledgerName }).Distinct();
            List<Aging> agingList = new List<Aging>();
            foreach (var ordered in orderedInvoiceswithParty.OrderBy(d => d.Party))
            {
                var partyinvoices = orderedInvoices.Where(d => d.LID == ordered.LID).ToList();
                if (partyinvoices.Count() > 0)
                {
                    var aging = new Aging();
                    aging.LID = ordered.LID;
                    aging.PartyName = ordered.Party;
                    foreach (var inv in partyinvoices)
                    {

                        switch (inv.InvoiceId)
                        {
                            //case 0:
                            //    aging.Current = inv.InvAmount;
                            //    break;
                            case 30:
                                aging.Days_1_30 = inv.InvAmount;
                                break;
                            case 60:
                                aging.Days_31_60 = inv.InvAmount;
                                break;
                            case 90:
                                aging.Days_61_90 = inv.InvAmount;
                                break;
                            case 750:
                                aging.Days_91_365 = inv.InvAmount;
                                break;
                        }



                    }
                    agingList.Add(aging);
                }
            }
            return PartialView(agingList);
        }

        public ActionResult AgingReportPDFLink(string To)
        {
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? AsOnDate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            try
            {

                return new ActionAsPdf("AgingReportPDF", new { fid = fid, AsOnDate = AsOnDate })
                {
                    FileName = "AgingReport.pdf",
                    PageOrientation = Rotativa.Options.Orientation.Portrait,
                    PageSize = Rotativa.Options.Size.A4
                };
            }
            catch
            {
                return new ActionAsPdf("AgingReportPDF", new { fid = fid, AsOnDate = AsOnDate })
                {
                    FileName = "AgingReport.pdf",
                    PageOrientation = Rotativa.Options.Orientation.Portrait,
                    PageSize = Rotativa.Options.Size.A4
                };
            }
        }
        public ActionResult AgingReportPDF(int fid, DateTime? AsOnDate)
        {
            var allPayments = (//from c in db.Suppliers.Where(d=>d.LId== 22834).ToList()
                         from c in db.LedgerMasters.Where(d => d.groupID == 104).ToList()
                         join op in db.OpeningBalances.Where(d => d.fYearID == (fid - 1)).GroupBy(d => d.ledgerID).Select(d => new { LID = d.Key, InvoiceId = 0, openingBal = d.Sum(s => s.openingBal) }).ToList()
                         on c.LID equals op.LID into cop
                         from op in cop.DefaultIfEmpty()

                         join opcur in db.OpeningBalances.Where(d => d.fYearID == fid).GroupBy(d => d.ledgerID).Select(d => new { LID = d.Key, InvoiceId = 0, openingBal = d.Sum(s => s.openingBal) }).ToList()
                         on c.LID equals opcur.LID into copcur
                         from opcur in copcur.DefaultIfEmpty()

                         join rp in db.ReceiptPayments.Where(d => (d.fYearId == (fid - 1) || d.fYearId == fid) && d.transactionType == "General Payment" && d.RPType == "Payment" && d.RPdate <= AsOnDate).GroupBy(d => d.ledgerId).Select(d => new { LId = d.Key, TotalAmount = d.Sum(l => l.TotalAmount) }).ToList()
                         on c.LID equals (int)rp.LId into crp
                         from rp in crp.DefaultIfEmpty()

                         join rp1 in db.ReceiptPayments.Where(d => (d.fYearId == (fid - 1) || d.fYearId == fid) && d.transactionType == "General Receive" && d.RPType == "Receive" && d.RPdate <= AsOnDate).GroupBy(d => d.ledgerId).Select(d => new { LId = d.Key, TotalAmount = d.Sum(l => l.TotalAmount) }).ToList()
                         on c.LID equals (int)rp1.LId into crp1
                         from rp1 in crp1.DefaultIfEmpty()

                         join sr in db.PurchaseReturns.Where(d => (d.FinancialYearId == (fid - 1) || d.FinancialYearId == fid) && d.Date <= AsOnDate).GroupBy(d => d.Supplier.LId).Select(d => new { LId = d.Key, BCGrandTotal = d.Sum(l => l.BCGrandTotal) }).ToList()
                         on c.LID equals sr.LId into csr
                         from sr in csr.DefaultIfEmpty()

                             //join cn in db.CreditNotes.Where(d => d.FinancialYearId == fid && d.Date <= AsOnDate).GroupBy(d => d.Supplier.LId).Select(d => new { LId = d.Key, BCGrandTotal = d.Sum(l => l.BCGrandTotal) }).ToList()
                             //on c.LID equals cn.LId into ccn
                             //from cn in ccn.DefaultIfEmpty()

                               join jd in db.JournalDetails.Where(d => (d.Journal.FinancialYearId == (fid - 1) || d.Journal.FinancialYearId == fid) && d.Journal.JournalDate <= AsOnDate).GroupBy(d => d.LID).Select(d => new { LId = d.Key, Debit = d.Sum(l => l.Debit), Credit = d.Sum(l => l.Credit) }).ToList()
                         on c.LID equals jd.LId into cjd
                         from jd in cjd.DefaultIfEmpty()

                         select new TaxComponent { TaxId = c.LID, Amount = (-(op == null ? (opcur == null ? 0 : opcur.openingBal) : op.openingBal) + (rp == null ? 0 : (rp.TotalAmount ?? 0)) + (sr == null ? 0 : sr.BCGrandTotal) - (jd == null ? 0 : (jd.Credit ?? 0)) + (jd == null ? 0 : (jd.Debit ?? 0)) - (rp1 == null ? 0 : (rp1.TotalAmount ?? 0))) }).OrderBy(d => d.TaxId).ToList();

            //Get All This year Invoices of All Parties
            var allInvoices = db.PurchaseInvoices.Where(d => (d.FinancialYearId == (fid - 1) || d.FinancialYearId == fid) && d.Status == InventoryConst.cns_Saved).Select(d => new CustomerInvoice { LID = d.Supplier.LId, InvoiceId = d.Id, InvoiceDate = d.InvoiceDate, InvAmount = d.BCGrandTotal, MaturityDate = EntityFunctions.AddDays(d.InvoiceDate, 30), DaysDue = System.Data.Objects.EntityFunctions.DiffDays(EntityFunctions.AddDays(d.InvoiceDate, 0), AsOnDate) }).OrderBy(d => d.LID).OrderByDescending(d => d.InvoiceDate).ToList();
            //  allInvoices = allInvoices.Select(d => new CustomerInvoice { LID = d.LID, InvoiceId = d.InvoiceId, InvoiceDate = d.InvoiceDate, InvAmount = d.InvAmount, DaysDue = EntityFunctions.DiffDays(AsOnDate, d.MaturityDate) }).ToList();
            //Get All the unpaid invoices of this Year
            DateTime openingDate = (DateTime)(db.FinancialYearMasters.Where(d => d.fYearID == (fid - 1)).Select(d => d.sDate).FirstOrDefault());
            // var opening = db.OpeningBalances.Where(d => d.fYearID == fid).GroupBy(d=>d.ledgerID).Select(d => new CustomerInvoice { LID = d.Key, InvoiceId = 0, InvoiceDate = openingDate, InvAmount = d.Sum(s=>s.openingBal) }).ToList();
            List<CustomerInvoice> getUnpaidInvoices = new List<CustomerInvoice>();
            foreach (var payment in allPayments)
            {
                var eachPartyInvoices = allInvoices.Where(d => d.LID == payment.TaxId).OrderBy(d => d.InvoiceDate).ToList();
                if (eachPartyInvoices.Count() == 0 && payment.Amount != 0)
                {
                    var inv = new CustomerInvoice();
                    inv.LID = payment.TaxId;
                    inv.InvAmount = -payment.Amount;
                    inv.DaysDue = (AsOnDate - openingDate).Value.Days;
                    getUnpaidInvoices.Add(inv);
                }

                var cnte = eachPartyInvoices.Count();
                int cntr = 0;
                foreach (var eachInvoice in eachPartyInvoices)
                {
                    ++cntr;

                    if (payment.Amount >= eachInvoice.InvAmount)
                    {
                        payment.Amount -= eachInvoice.InvAmount;

                        if (payment.Amount > 0 && cnte == cntr)
                        {
                            eachInvoice.InvAmount = -payment.Amount;
                            eachInvoice.DaysDue = (AsOnDate - openingDate).Value.Days;
                            getUnpaidInvoices.Add(eachInvoice);
                        }

                    }
                    else
                    {

                        eachInvoice.InvAmount = (eachInvoice.InvAmount - payment.Amount);
                        getUnpaidInvoices.Add(eachInvoice);
                        payment.Amount = 0;
                    }
                }
            }
            List<CustomerInvoice> getGroupedInvoices = new List<CustomerInvoice>();
            //  var ceilings = new[] { 0, 30, 60, 90, 365 };
            var ceilings = new[] { 30, 60, 90, 750 };
            var groupings = getUnpaidInvoices.GroupBy(item => ceilings.First(ceiling => ceiling >= item.DaysDue)).ToList();
            foreach (var daysRange in groupings)
            {
                getGroupedInvoices.AddRange(daysRange.GroupBy(d => d.LID).Select(d => new CustomerInvoice { InvoiceId = daysRange.Key, LID = d.Key, InvAmount = d.Sum(f => f.InvAmount) }).ToList());
            }
            var orderedInvoices = getGroupedInvoices.OrderBy(d => d.LID).ToList();
            var orderedInvoiceswithParty = (from o in orderedInvoices
                                            join c in db.LedgerMasters.Where(d => d.groupID == 104)
                                            on o.LID equals c.LID
                                            select new { LID = o.LID, Party = c.ledgerName }).Distinct();
            List<Aging> agingList = new List<Aging>();
            foreach (var ordered in orderedInvoiceswithParty.OrderBy(d => d.Party))
            {
                var partyinvoices = orderedInvoices.Where(d => d.LID == ordered.LID).ToList();
                if (partyinvoices.Count() > 0)
                {
                    var aging = new Aging();
                    aging.LID = ordered.LID;
                    aging.PartyName = ordered.Party;
                    foreach (var inv in partyinvoices)
                    {

                        switch (inv.InvoiceId)
                        {
                            //case 0:
                            //    aging.Current = inv.InvAmount;
                            //    break;
                            case 30:
                                aging.Days_1_30 = inv.InvAmount;
                                break;
                            case 60:
                                aging.Days_31_60 = inv.InvAmount;
                                break;
                            case 90:
                                aging.Days_61_90 = inv.InvAmount;
                                break;
                            case 750:
                                aging.Days_91_365 = inv.InvAmount;
                                break;
                        }



                    }
                    agingList.Add(aging);
                }
            }
            ViewBag.AsOnDate = AsOnDate;
            ViewBag.Company = db.Companies.Select(d => d.Name).FirstOrDefault();
            return View(agingList);
        }

        public ActionResult AgingReportExcel(string To)
        {
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? AsOnDate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var allPayments = (//from c in db.Suppliers.Where(d=>d.LId== 22834).ToList()
                                   from c in db.LedgerMasters.Where(d => d.groupID == 104).ToList()
                                   join op in db.OpeningBalances.Where(d => d.fYearID == (fid - 1)).GroupBy(d => d.ledgerID).Select(d => new { LID = d.Key, InvoiceId = 0, openingBal = d.Sum(s => s.openingBal) }).ToList()
                                   on c.LID equals op.LID into cop
                                   from op in cop.DefaultIfEmpty()

                                   join opcur in db.OpeningBalances.Where(d => d.fYearID == fid).GroupBy(d => d.ledgerID).Select(d => new { LID = d.Key, InvoiceId = 0, openingBal = d.Sum(s => s.openingBal) }).ToList()
                                   on c.LID equals opcur.LID into copcur
                                   from opcur in copcur.DefaultIfEmpty()

                                   join rp in db.ReceiptPayments.Where(d => (d.fYearId == (fid - 1) || d.fYearId == fid) && d.transactionType == "General Payment" && d.RPType == "Payment" && d.RPdate <= AsOnDate).GroupBy(d => d.ledgerId).Select(d => new { LId = d.Key, TotalAmount = d.Sum(l => l.TotalAmount) }).ToList()
                                   on c.LID equals (int)rp.LId into crp
                                   from rp in crp.DefaultIfEmpty()

                                   join rp1 in db.ReceiptPayments.Where(d => (d.fYearId == (fid - 1) || d.fYearId == fid) && d.transactionType == "General Receive" && d.RPType == "Receive" && d.RPdate <= AsOnDate).GroupBy(d => d.ledgerId).Select(d => new { LId = d.Key, TotalAmount = d.Sum(l => l.TotalAmount) }).ToList()
                                   on c.LID equals (int)rp1.LId into crp1
                                   from rp1 in crp1.DefaultIfEmpty()

                                   join sr in db.PurchaseReturns.Where(d => (d.FinancialYearId == (fid - 1) || d.FinancialYearId == fid) && d.Date <= AsOnDate).GroupBy(d => d.Supplier.LId).Select(d => new { LId = d.Key, BCGrandTotal = d.Sum(l => l.BCGrandTotal) }).ToList()
                                   on c.LID equals sr.LId into csr
                                   from sr in csr.DefaultIfEmpty()

                                       //join cn in db.CreditNotes.Where(d => d.FinancialYearId == fid && d.Date <= AsOnDate).GroupBy(d => d.Supplier.LId).Select(d => new { LId = d.Key, BCGrandTotal = d.Sum(l => l.BCGrandTotal) }).ToList()
                                       //on c.LID equals cn.LId into ccn
                                       //from cn in ccn.DefaultIfEmpty()

                               join jd in db.JournalDetails.Where(d => (d.Journal.FinancialYearId == (fid - 1) || d.Journal.FinancialYearId == fid) && d.Journal.JournalDate <= AsOnDate).GroupBy(d => d.LID).Select(d => new { LId = d.Key, Debit = d.Sum(l => l.Debit), Credit = d.Sum(l => l.Credit) }).ToList()
                                   on c.LID equals jd.LId into cjd
                                   from jd in cjd.DefaultIfEmpty()

                                   select new TaxComponent { TaxId = c.LID, Amount = (-(op == null ? (opcur == null ? 0 : opcur.openingBal) : op.openingBal) + (rp == null ? 0 : (rp.TotalAmount ?? 0)) + (sr == null ? 0 : sr.BCGrandTotal) - (jd == null ? 0 : (jd.Credit ?? 0)) + (jd == null ? 0 : (jd.Debit ?? 0)) - (rp1 == null ? 0 : (rp1.TotalAmount ?? 0))) }).OrderBy(d => d.TaxId).ToList();

            //Get All This year Invoices of All Parties
            var allInvoices = db.PurchaseInvoices.Where(d => (d.FinancialYearId == (fid - 1) || d.FinancialYearId == fid) && d.Status == InventoryConst.cns_Saved).Select(d => new CustomerInvoice { LID = d.Supplier.LId, InvoiceId = d.Id, InvoiceDate = d.InvoiceDate, InvAmount = d.BCGrandTotal, MaturityDate = EntityFunctions.AddDays(d.InvoiceDate, 30), DaysDue = System.Data.Objects.EntityFunctions.DiffDays(EntityFunctions.AddDays(d.InvoiceDate, 0), AsOnDate) }).OrderBy(d => d.LID).OrderByDescending(d => d.InvoiceDate).ToList();
            //  allInvoices = allInvoices.Select(d => new CustomerInvoice { LID = d.LID, InvoiceId = d.InvoiceId, InvoiceDate = d.InvoiceDate, InvAmount = d.InvAmount, DaysDue = EntityFunctions.DiffDays(AsOnDate, d.MaturityDate) }).ToList();
            //Get All the unpaid invoices of this Year
            DateTime openingDate = (DateTime)(db.FinancialYearMasters.Where(d => d.fYearID == (fid - 1)).Select(d => d.sDate).FirstOrDefault());
            // var opening = db.OpeningBalances.Where(d => d.fYearID == fid).GroupBy(d=>d.ledgerID).Select(d => new CustomerInvoice { LID = d.Key, InvoiceId = 0, InvoiceDate = openingDate, InvAmount = d.Sum(s=>s.openingBal) }).ToList();
            List<CustomerInvoice> getUnpaidInvoices = new List<CustomerInvoice>();
            foreach (var payment in allPayments)
            {
                var eachPartyInvoices = allInvoices.Where(d => d.LID == payment.TaxId).OrderBy(d => d.InvoiceDate).ToList();
                if (eachPartyInvoices.Count() == 0 && payment.Amount != 0)
                {
                    var inv = new CustomerInvoice();
                    inv.LID = payment.TaxId;
                    inv.InvAmount = -payment.Amount;
                    inv.DaysDue = (AsOnDate - openingDate).Value.Days;
                    getUnpaidInvoices.Add(inv);
                }

                var cnte = eachPartyInvoices.Count();
                int cntr = 0;
                foreach (var eachInvoice in eachPartyInvoices)
                {
                    ++cntr;

                    if (payment.Amount >= eachInvoice.InvAmount)
                    {
                        payment.Amount -= eachInvoice.InvAmount;

                        if (payment.Amount > 0 && cnte == cntr)
                        {
                            eachInvoice.InvAmount = -payment.Amount;
                            eachInvoice.DaysDue = (AsOnDate - openingDate).Value.Days;
                            getUnpaidInvoices.Add(eachInvoice);
                        }

                    }
                    else
                    {

                        eachInvoice.InvAmount = (eachInvoice.InvAmount - payment.Amount);
                        getUnpaidInvoices.Add(eachInvoice);
                        payment.Amount = 0;
                    }
                }
            }
            List<CustomerInvoice> getGroupedInvoices = new List<CustomerInvoice>();
            //  var ceilings = new[] { 0, 30, 60, 90, 365 };
            var ceilings = new[] { 30, 60, 90, 750 };
            var groupings = getUnpaidInvoices.GroupBy(item => ceilings.First(ceiling => ceiling >= item.DaysDue)).ToList();
            foreach (var daysRange in groupings)
            {
                getGroupedInvoices.AddRange(daysRange.GroupBy(d => d.LID).Select(d => new CustomerInvoice { InvoiceId = daysRange.Key, LID = d.Key, InvAmount = d.Sum(f => f.InvAmount) }).ToList());
            }
            var orderedInvoices = getGroupedInvoices.OrderBy(d => d.LID).ToList();
            var orderedInvoiceswithParty = (from o in orderedInvoices
                                            join c in db.LedgerMasters.Where(d => d.groupID == 104)
                                            on o.LID equals c.LID
                                            select new { LID = o.LID, Party = c.ledgerName }).Distinct();
            List<Aging> agingList = new List<Aging>();
            foreach (var ordered in orderedInvoiceswithParty.OrderBy(d => d.Party))
            {
                var partyinvoices = orderedInvoices.Where(d => d.LID == ordered.LID).ToList();
                if (partyinvoices.Count() > 0)
                {
                    var aging = new Aging();
                    aging.LID = ordered.LID;
                    aging.PartyName = ordered.Party;
                    foreach (var inv in partyinvoices)
                    {

                        switch (inv.InvoiceId)
                        {
                            //case 0:
                            //    aging.Current = inv.InvAmount;
                            //    break;
                            case 30:
                                aging.Days_1_30 = inv.InvAmount;
                                break;
                            case 60:
                                aging.Days_31_60 = inv.InvAmount;
                                break;
                            case 90:
                                aging.Days_61_90 = inv.InvAmount;
                                break;
                            case 750:
                                aging.Days_91_365 = inv.InvAmount;
                                break;
                        }



                    }
                    agingList.Add(aging);
                }
            }
            //ViewBag.To = To;
            //ViewBag.CompanyName = db.Companies.Select(d => d.Name).FirstOrDefault();
            ViewBag.AsOnDate = AsOnDate;
            ViewBag.Company = db.Companies.Select(d => d.Name).FirstOrDefault();
            Response.AddHeader("content-disposition", "attachment; filename= Aging.xls");
            Response.ContentType = "application/ms-excel";
            return PartialView("AgingReportPDF", agingList);
        }
        #endregion

        #region Outstanding Bill Wise Details
        public ActionResult OutstandingBillWise(string Msg, string Err)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }

            return View();


        }

        [HttpGet]
        public ActionResult OutstandingBillWiseReports()
        {
            //var culture = Session["DateCulture"].ToString();
            //string dateFormat = Session["DateFormat"].ToString();
            //DateTime? FromDate = null;
            //DateTime? ToDate = null;
            //if (From != "0")
            //{
            //    FromDate = DateTime.ParseExact(From, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            //}
            //if (To != "0")
            //{
            //    ToDate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            //}
            //Session["fdate"] = FromDate;
            //Session["tdate"] = ToDate;

            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            var sundryDebtorsDet = db.LedgerMasters.Where(d => d.groupID == 104).Select(d => new { LID = d.LID, LedgerName = d.ledgerName }).ToList();
            var sundryDebtors = db.LedgerMasters.Where(d => d.groupID == 104).Select(d => d.LID).ToList();
            var opening = db.OpeningBalances.Where(d => d.fYearID == Fyid && sundryDebtors.Contains(d.ledgerID)).GroupBy(d => d.ledgerID).Select(d => new { LID = d.Key, Amount = d.Sum(q => q.openingBal) }).ToList();
            var totalRececeipts = db.ReceiptPayments.Where(d => d.fYearId == Fyid && sundryDebtors.Contains((d.ledgerId ?? 0)) && d.RPType == "Payment").GroupBy(d => d.ledgerId).Select(d => new { LID = d.Key, Amount = d.Sum(q => q.TotalAmount) }).ToList();
            var journalDetails = db.JournalDetails.Where(d => d.Journal.FinancialYearId == Fyid && sundryDebtors.Contains(d.LID)).GroupBy(d => d.LID).Select(d => new { LID = d.Key, Amount = (d.Sum(q => q.Debit - q.Credit)) }).ToList();
            var sales = db.PurchaseInvoices.Where(d => d.FinancialYearId == Fyid).GroupBy(d => d.Supplier.LId).Select(d => new { LID = d.Key, Amount = d.Sum(q => q.BCGrandTotal) }).ToList();
            var salesReturn = db.PurchaseReturns.Where(d => d.FinancialYearId == Fyid).GroupBy(d => d.Supplier.LId).Select(d => new { LID = d.Key, Amount = d.Sum(q => q.BCGrandTotal) }).ToList();
          //  var creditNote = db.CreditNotes.Where(d => d.FinancialYearId == Fyid).GroupBy(d => d.Supplier.LId).Select(d => new { LID = d.Key, Amount = d.Sum(q => q.BCGrandTotal) }).ToList();

            var result = from op in opening
                         join le in sundryDebtorsDet
                         on op.LID equals le.LID into ople
                         from le in ople.DefaultIfEmpty()
                         join rp in totalRececeipts
                         on op.LID equals (rp.LID ?? 0) into oprp
                         from rp in oprp.DefaultIfEmpty()
                         join jo in journalDetails
                         on op.LID equals jo.LID into opjo
                         from jo in opjo.DefaultIfEmpty()
                         join si in sales
                         on op.LID equals si.LID into opsi
                         from si in opsi.DefaultIfEmpty()
                         join sr in salesReturn
                         on op.LID equals sr.LID into opsr
                         from sr in opsr.DefaultIfEmpty()
                         //join cr in creditNote
                         //on op.LID equals cr.LID into opcr
                         //from cr in opcr.DefaultIfEmpty()
                         select new OutstandingInvoice
                         {
                             LID = op.LID,
                             LedgerName = le.LedgerName,
                             OpeningAdvance = op.Amount <= 0 ? op.Amount * (-1) : 0,
                             OpeningDue = op.Amount > 0 ? op.Amount : 0,
                             AdjustmentShort = jo != null ? (jo.Amount <= 0 ? jo.Amount * (-1) : 0) : 0,
                             AdjustmentExtra = jo != null ? (jo.Amount > 0 ? jo.Amount : 0) : 0,
                             Sales = si != null ? si.Amount : 0,
                             Salesreturn = sr != null ? sr.Amount : 0,
                          //   Creditnote = cr != null ? cr.Amount : 0,
                             Receipt = rp != null ? rp.Amount : 0,
                         };
            result = result.Where(d => !(d.OpeningAdvance == 0 && d.OpeningDue == 0 && d.AdjustmentShort == 0 && d.AdjustmentExtra == 0 && d.Sales == 0 && d.Salesreturn == 0  && d.Receipt == 0)).OrderBy(d => d.LedgerName).ToList();
            return View(result);
        }
        [HttpGet]
        public ActionResult OutstandingBillWiseSupplier(long LID, string lname, decimal odue, decimal oadvance, decimal ashort, decimal aextra, decimal sales, decimal preturn, decimal paymentvoucher)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            ViewBag.CustomerName = lname;
            ViewBag.Address = db.Suppliers.Where(d => d.LId == LID).Select(d => d.PoAddressName).FirstOrDefault();
            List<PurchaseInvoice> salesList = new List<PurchaseInvoice>();
            var salesinv = db.PurchaseInvoices.Where(d => d.FinancialYearId == Fyid && d.Supplier.LId == LID).OrderBy(d => d.InvoiceDate).ThenBy(d => d.Id).ToList();
            //var duefactor = sales + ashort + odue - oadvance - aextra - sreturn;
            decimal payment = 0;

            if (salesinv.Count() > 0)
            {
                payment = paymentvoucher + oadvance + aextra + preturn; //  ashort - odue;

                if (odue != 0)
                {
                    if (payment >= odue)
                    {
                        payment -= odue;
                    }
                    else
                    {
                        var item = new PurchaseInvoice();
                        item.NO = "Opening Balance As on 1st April";
                        item.BCGrandTotal = odue - payment;
                        payment = 0;
                        salesList.Add(item);

                    }
                }

                if (ashort != 0)
                {
                    if (payment >= ashort)
                    {
                        payment -= ashort;
                    }
                    else
                    {
                        var item = new PurchaseInvoice();
                        item.NO = "Journal Adjustment Short";
                        item.BCGrandTotal = ashort - payment;
                        payment = 0;
                        salesList.Add(item);

                    }
                }
            }
            else
            {
                if (odue != 0)
                {
                    var item = new PurchaseInvoice();
                    item.NO = "Opening Balance As on 1st April";
                    item.BCGrandTotal = odue;
                    payment -= odue;
                    salesList.Add(item);

                }
                if (oadvance != 0)
                {
                    var item = new PurchaseInvoice();
                    item.NO = "Advance Opening Balance As on 1st April";
                    item.BCGrandTotal = -oadvance;
                    payment += oadvance;
                    salesList.Add(item);
                }
                if (ashort != 0)
                {
                    var item = new PurchaseInvoice();
                    item.NO = "Journal Adjustment Short";
                    item.BCGrandTotal = ashort;
                    payment -= ashort;
                    salesList.Add(item);
                }
                if (preturn != 0)
                {
                    var item = new PurchaseInvoice();
                    item.NO = "Purchase Return/Debit Note";
                    item.BCGrandTotal = -preturn;
                    payment += preturn;
                    salesList.Add(item);
                }
                if (paymentvoucher != 0)
                {
                    var item = new PurchaseInvoice();
                    item.NO = "Payment";
                    item.BCGrandTotal = -paymentvoucher;
                    payment += paymentvoucher;
                    salesList.Add(item);
                }
                if (aextra != 0)
                {
                    var item = new PurchaseInvoice();
                    item.NO = "Extra from Journal Adjustment";
                    item.BCGrandTotal = -aextra;
                    payment += aextra;
                    salesList.Add(item);
                }
            }


            foreach (var sale in salesinv)
            {
                if (payment >= sale.BCGrandTotal)
                {
                    payment -= sale.BCGrandTotal;
                }
                else
                {
                    var item = new PurchaseInvoice();
                    item = sale;
                    item.BCGrandTotal = item.BCGrandTotal - payment;
                    item.CurrencyId = (DateTime.Today - sale.InvoiceDate).Days;
                    payment = 0;
                    salesList.Add(item);
                }

            }
            return View(salesList);
        }
        [HttpGet]
        public ActionResult OutstandingBillWiseSupplierPDF(long LID, string lname, decimal odue, decimal oadvance, decimal ashort, decimal aextra, decimal sales, decimal preturn, decimal paymentvoucher)
        {
            // int compid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt32(Session["companyid"]);



            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();

            //DateTime? FromDate = null;
            //DateTime? ToDate = null;
            try
            {

                // FromDate = DateTime.ParseExact(DateTime.Today, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                //ToDate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                ViewBag.from = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                //ViewBag.to = ToDate;
                var company = db.Companies.Select(d => new { d.Name, d.GST_VATNumber, d.Address }).FirstOrDefault();
                ViewBag.CompanyName = company.Name;
                ViewBag.AddressCompany = company.Address;
                ViewBag.GSTNO = company.GST_VATNumber;

                int Fyid = Convert.ToInt32(Session["fid"]);
                ViewBag.CustomerName = lname;
                ViewBag.Address = db.Suppliers.Where(d => d.LId == LID).Select(d => d.PoAddressName).FirstOrDefault();
                List<PurchaseInvoice> salesList = new List<PurchaseInvoice>();
                var salesinv = db.PurchaseInvoices.Where(d => d.FinancialYearId == Fyid && d.Supplier.LId == LID).OrderBy(d => d.InvoiceDate).ThenBy(d => d.Id).ToList();
                //var duefactor = sales + ashort + odue - oadvance - aextra - sreturn;

                decimal payment = 0;

                if (salesinv.Count() > 0)
                {
                    payment = paymentvoucher + oadvance + aextra + preturn; //  ashort - odue;

                    if (odue != 0)
                    {
                        if (payment >= odue)
                        {
                            payment -= odue;
                        }
                        else
                        {
                            var item = new PurchaseInvoice();
                            item.NO = "Opening Balance As on 1st April";
                            item.BCGrandTotal = odue - payment;
                            payment = 0;
                            salesList.Add(item);

                        }
                    }

                    if (ashort != 0)
                    {
                        if (payment >= ashort)
                        {
                            payment -= ashort;
                        }
                        else
                        {
                            var item = new PurchaseInvoice();
                            item.NO = "Journal Adjustment Short";
                            item.BCGrandTotal = ashort - payment;
                            payment = 0;
                            salesList.Add(item);

                        }
                    }
                }
                else
                {
                    if (odue != 0)
                    {
                        var item = new PurchaseInvoice();
                        item.NO = "Opening Balance As on 1st April";
                        item.BCGrandTotal = odue;
                        payment -= odue;
                        salesList.Add(item);

                    }
                    if (oadvance != 0)
                    {
                        var item = new PurchaseInvoice();
                        item.NO = "Advance Opening Balance As on 1st April";
                        item.BCGrandTotal = -oadvance;
                        payment += oadvance;
                        salesList.Add(item);
                    }
                    if (ashort != 0)
                    {
                        var item = new PurchaseInvoice();
                        item.NO = "Journal Adjustment Short";
                        item.BCGrandTotal = ashort;
                        payment -= ashort;
                        salesList.Add(item);
                    }
                    if (preturn != 0)
                    {
                        var item = new PurchaseInvoice();
                        item.NO = "Purchase Return/Debit Note";
                        item.BCGrandTotal = -preturn;
                        payment += preturn;
                        salesList.Add(item);
                    }
                    if (paymentvoucher != 0)
                    {
                        var item = new PurchaseInvoice();
                        item.NO = "Payment";
                        item.BCGrandTotal = -paymentvoucher;
                        payment += paymentvoucher;
                        salesList.Add(item);
                    }
                    if (aextra != 0)
                    {
                        var item = new PurchaseInvoice();
                        item.NO = "Extra from Journal Adjustment";
                        item.BCGrandTotal = -aextra;
                        payment += aextra;
                        salesList.Add(item);
                    }
                }


                foreach (var sale in salesinv)
                {
                    if (payment >= sale.BCGrandTotal)
                    {
                        payment -= sale.BCGrandTotal;
                    }
                    else
                    {
                        var item = new PurchaseInvoice();
                        item = sale;
                        item.BCGrandTotal = item.BCGrandTotal - payment;
                        item.CurrencyId = (DateTime.Today - sale.InvoiceDate).Days;
                        payment = 0;
                        salesList.Add(item);
                    }

                }


                if (salesList.Count > 0)
                    ViewBag.TotalDue = salesList.Sum(d => d.BCGrandTotal);
                return new Rotativa.PartialViewAsPdf("OutstandingBillWiseSupplierPDF", salesList)
                {
                    FileName = "OutstandingBillWiseSupplierPDF.pdf",
                    PageSize = Size.A4,
                    PageMargins = new Margins(10, 10, 10, 10),
                    CustomSwitches = "--footer-right [page]/[topage]"//customSwitches
                                                                     // PageMargins = new Margins(0, 0, 0, 0),
                                                                     //CustomSwitches = "--print-media-type  --footer-center [page] --footer-font-size 8"

                };
            }
            catch
            {
                return new ActionAsPdf("PurchaseGSTPDF", new { id = Branchid, value = "0" }) { FileName = "PurchaseGSTPDF.pdf" };
            }

        }
        [HttpGet]
        public ActionResult OutstandingBillWiseDetailsExcel()
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                long companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                int Fyid = Convert.ToInt32(Session["fid"]);
                var sundryDebtorsDet = db.LedgerMasters.Where(d => d.groupID == 104).Select(d => new { LID = d.LID, LedgerName = d.ledgerName }).ToList();
                var sundryDebtors = db.LedgerMasters.Where(d => d.groupID == 104).Select(d => d.LID).ToList();
                var opening = db.OpeningBalances.Where(d => d.fYearID == Fyid && sundryDebtors.Contains(d.ledgerID)).GroupBy(d => d.ledgerID).Select(d => new { LID = d.Key, Amount = d.Sum(q => q.openingBal) }).ToList();
                var totalRececeipts = db.ReceiptPayments.Where(d => d.fYearId == Fyid && sundryDebtors.Contains((d.ledgerId ?? 0)) && d.RPType == "Payment").GroupBy(d => d.ledgerId).Select(d => new { LID = d.Key, Amount = d.Sum(q => q.TotalAmount) }).ToList();
                var journalDetails = db.JournalDetails.Where(d => d.Journal.FinancialYearId == Fyid && sundryDebtors.Contains(d.LID)).GroupBy(d => d.LID).Select(d => new { LID = d.Key, Amount = (d.Sum(q => q.Debit - q.Credit)) }).ToList();
                var sales = db.PurchaseInvoices.Where(d => d.FinancialYearId == Fyid).GroupBy(d => d.Supplier.LId).Select(d => new { LID = d.Key, Amount = d.Sum(q => q.BCGrandTotal) }).ToList();
                var salesReturn = db.PurchaseReturns.Where(d => d.FinancialYearId == Fyid).GroupBy(d => d.Supplier.LId).Select(d => new { LID = d.Key, Amount = d.Sum(q => q.BCGrandTotal) }).ToList();
              //  var creditNote = db.CreditNotes.Where(d => d.FinancialYearId == Fyid).GroupBy(d => d.Supplier.LId).Select(d => new { LID = d.Key, Amount = d.Sum(q => q.BCGrandTotal) }).ToList();
                var result = (from op in opening
                              join le in sundryDebtorsDet
                              on op.LID equals le.LID into ople
                              from le in ople.DefaultIfEmpty()
                              join rp in totalRececeipts
                              on op.LID equals (rp.LID ?? 0) into oprp
                              from rp in oprp.DefaultIfEmpty()
                              join jo in journalDetails
                              on op.LID equals jo.LID into opjo
                              from jo in opjo.DefaultIfEmpty()
                              join si in sales
                              on op.LID equals si.LID into opsi
                              from si in opsi.DefaultIfEmpty()
                              join sr in salesReturn
                              on op.LID equals sr.LID into opsr
                              from sr in opsr.DefaultIfEmpty()
                              //join cr in creditNote
                              //on op.LID equals cr.LID into opcr
                              //from cr in opcr.DefaultIfEmpty()

                              select new OutstandingInvoice
                              {
                                  LID = op.LID,
                                  LedgerName = le.LedgerName,
                                  OpeningAdvance = op.Amount <= 0 ? op.Amount * (-1) : 0,
                                  OpeningDue = op.Amount > 0 ? op.Amount : 0,
                                  AdjustmentShort = jo != null ? (jo.Amount <= 0 ? jo.Amount * (-1) : 0) : 0,
                                  AdjustmentExtra = jo != null ? (jo.Amount > 0 ? jo.Amount : 0) : 0,
                                  Sales = si != null ? si.Amount : 0,
                                  Salesreturn = sr != null ? sr.Amount : 0,
                               //   Creditnote = cr != null ? cr.Amount : 0,
                                  Receipt = rp != null ? rp.Amount : 0,
                              }).OrderBy(d => d.LedgerName).ToList();

                List<PurchaseInvoice> salesList = new List<PurchaseInvoice>();

                foreach (var item in result)
                {

                    var salesinv = db.PurchaseInvoices.Where(d => d.FinancialYearId == Fyid && d.Supplier.LId == item.LID).OrderBy(d => d.InvoiceDate).ThenBy(d => d.Id).ToList();
                    //var duefactor = sales + ashort + odue - oadvance - aextra - sreturn;

                    decimal? payment = 0;

                    if (salesinv.Count() > 0)
                    {
                        payment = item.Receipt + item.OpeningAdvance + item.AdjustmentExtra + item.Salesreturn; //  ashort - odue;

                        if (item.OpeningDue != 0)
                        {
                            if (payment >= item.OpeningDue)
                            {
                                payment -= item.OpeningDue;
                            }
                            else
                            {
                                var puritem = new PurchaseInvoice();
                                puritem.DeliveryName = item.LedgerName;
                                puritem.NO = "Opening Balance As on 1st April";
                                puritem.BCGrandTotal = (item.OpeningDue - payment) ?? 0;
                                payment = 0;
                                salesList.Add(puritem);

                            }
                        }

                        if (item.AdjustmentShort != 0)
                        {
                            if (payment >= item.AdjustmentShort)
                            {
                                payment -= item.AdjustmentShort;
                            }
                            else
                            {
                                var puritem = new PurchaseInvoice();
                                puritem.DeliveryName = item.LedgerName;
                                puritem.NO = "Journal Adjustment Short";
                                puritem.BCGrandTotal = (item.AdjustmentShort - payment) ?? 0;
                                payment = 0;
                                salesList.Add(puritem);

                            }
                        }
                    }
                    else
                    {
                        if (item.OpeningDue != 0)
                        {
                            var puritem = new PurchaseInvoice();
                            puritem.DeliveryName = item.LedgerName;
                            puritem.NO = "Opening Balance As on 1st April";
                            puritem.BCGrandTotal = item.OpeningDue;
                            payment -= item.OpeningDue;
                            salesList.Add(puritem);

                        }
                        if (item.OpeningAdvance != 0)
                        {
                            var puritem = new PurchaseInvoice();
                            puritem.DeliveryName = item.LedgerName;
                            puritem.NO = "Advance Opening Balance As on 1st April";
                            puritem.BCGrandTotal = -item.OpeningAdvance;
                            payment += item.OpeningAdvance;
                            salesList.Add(puritem);
                        }
                        if (item.AdjustmentShort != 0)
                        {
                            var puritem = new PurchaseInvoice();
                            puritem.DeliveryName = item.LedgerName;
                            puritem.NO = "Journal Adjustment Short";
                            puritem.BCGrandTotal = item.AdjustmentShort ?? 0;
                            payment -= item.AdjustmentShort;
                            salesList.Add(puritem);
                        }
                        if (item.Salesreturn != 0)
                        {
                            var puritem = new PurchaseInvoice();
                            puritem.DeliveryName = item.LedgerName;
                            puritem.NO = "Purchase Return/Debit Note";
                            puritem.BCGrandTotal = -item.Salesreturn ?? 0;
                            payment += item.Salesreturn;
                            salesList.Add(puritem);
                        }
                        //if (item.Creditnote != 0)
                        //{
                        //    var puritem = new PurchaseInvoice();
                        //    puritem.DeliveryName = item.LedgerName;
                        //    puritem.NO = "Credit Note";
                        //    puritem.BCGrandTotal = item.Creditnote ?? 0;
                        //    payment += item.Creditnote;
                        //    salesList.Add(puritem);
                        //}
                        if (item.Receipt != 0)
                        {
                            var puritem = new PurchaseInvoice();
                            puritem.DeliveryName = item.LedgerName;
                            puritem.NO = "Payment";
                            puritem.BCGrandTotal = -item.Receipt ?? 0;
                            payment += item.Receipt;
                            salesList.Add(puritem);
                        }
                        if (item.AdjustmentExtra != 0)
                        {
                            var puritem = new PurchaseInvoice();
                            puritem.DeliveryName = item.LedgerName;
                            puritem.NO = "Extra from Journal Adjustment";
                            puritem.BCGrandTotal = -item.AdjustmentExtra ?? 0;
                            payment += item.AdjustmentExtra;
                            salesList.Add(puritem);
                        }
                    }


                    foreach (var sale in salesinv)
                    {
                        if (payment >= sale.BCGrandTotal)
                        {
                            payment -= sale.BCGrandTotal;
                        }
                        else
                        {
                            var puritem = new PurchaseInvoice();
                            puritem = sale;
                            puritem.DeliveryName = item.LedgerName;
                            puritem.BCGrandTotal = (puritem.BCGrandTotal - payment) ?? 0;
                            puritem.CurrencyId = (DateTime.Today - sale.InvoiceDate).Days;
                            payment = 0;
                            salesList.Add(puritem);
                        }

                    }
                }


                Response.AddHeader("content-disposition", "attachment; filename= OutstandingBillWiseDetailsExcel.xls");
                Response.ContentType = "application/ms-excel";
                return PartialView("OutstandingBillWiseDetailsExcel", salesList);
            }


        }

        #endregion


    }


}

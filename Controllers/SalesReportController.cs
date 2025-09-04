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

namespace XenERP.Controllers
{
    public class SalesReportController : Controller
    {

        InventoryEntities db = new InventoryEntities();
        
      
        XenERP.Models.Repository.TaxRepository rep = new Models.Repository.TaxRepository();
        private TransactionClasses tc = new TransactionClasses();

        public ActionResult Index()
        {
            return View();
        }


        #region ---- Sales By Customer--------------


        public ActionResult SalesCustomer()
        {
            return View();
            

        }

        [HttpGet]
        public ActionResult CustomerSalesReport(string from, string to, int id)
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? FromDate = null;
            DateTime? ToDate = null;
            if (from != "0")
            {
                FromDate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            if (to != "0")
            {
                ToDate = DateTime.ParseExact(to, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            Session["fdate"] = FromDate;
            Session["tdate"] = ToDate;

            Session["Custid"] = id.ToString();
            if (from != "0")
            {
                Session["datetime"] = 1;

                var detls = db.SalesInvoices.Where(d => d.Date >= FromDate && d.Date <= ToDate && d.BranchId==Branchid && d.CustomerId == id).ToList();
                return PartialView(detls);
            }
            else
            {
                var detls = db.SalesInvoices.Where(d => d.CustomerId == id && d.BranchId == Branchid).ToList();
                return PartialView(detls);
            }



        }


        [HttpPost]
        public JsonResult getCustomer(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            var customers = db.Customers.Where(p => p.Name.Contains(query) && p.CompanyId == companyid).Select(p => new { Name = p.Name, Id = p.Id }).ToList();
            return Json(customers, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getSelectedCustomer(int id)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            var suppliers = db.Customers.Where(p => p.Id == id && p.CompanyId == companyid && p.BranchId == Branchid).Select(p => new { Name = p.Name, Id = p.Id }).FirstOrDefault();
            return Json(suppliers, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getSundryDebtors()
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            var suppliers = db.Customers.Where(p =>  p.CompanyId == companyid && p.PId == null).Select(p => new { Name = p.Name, LedgerId = p.LId }).ToList();
            return Json(suppliers, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult SalesbyCustomerPrint(int id)
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

            var customerdetls = db.SalesInvoices.Where(d => d.Date >= fromDate && d.Date <= toDate && d.CustomerId == id).ToList();

            return View(customerdetls);




        }


        [HttpGet]
        public ActionResult SalesbyCustomerPDF(int id, int value, string from, string to)
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

            var customerdetls = db.SalesInvoices.Where(d => d.Date >= fromDate && d.Date <= toDate && d.CustomerId == id).ToList();

            return View(customerdetls);




        }



        [HttpGet]
        public ActionResult SalesbyCustomerPDFlink()
        {
            int custid = Convert.ToInt32(Session["Custid"]);

            int compid = Convert.ToInt32(Session["companyid"]);

            try
            {
                string from = Session["fdate"].ToString();
                string to = Session["tdate"].ToString();
                return new ActionAsPdf("SalesbyCustomerPDF", new { id = custid, value = compid, from = from, to = to }) { FileName = "SalesByCustomer.pdf" };
            }
            catch
            {
                return new ActionAsPdf("SalesbyCustomerPDF", new { id = custid, value = compid, from = 0, to = 0 }) { FileName = "SalesByCustomer.pdf" };
            }

        }






        #endregion




        #region ---- Sales By Product--------------


        public ActionResult SalesProduct()
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

        public ActionResult SalesProductReport(int id, string from, string to)
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

                List<SalesProduct_Result> sprList = new List<SalesProduct_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal quantitytotal = 0;
                decimal amounttotal = 0;
                var result = db.SalesProduct(id, FromDate, ToDate, companyid, Branchid).ToList();
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
                                    var spr = new SalesProduct_Result();
                                    spr.ItemName = row.ItemName;
                                    spr.CustomerName = "Header";
                                    sprList.Add(spr);
                                    var spr1 = new SalesProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.CustomerName = row.CustomerName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    spr1.Rate = row.Rate;
                                    sprList.Add(spr1);
                                    var spr2 = new SalesProduct_Result();
                                    spr2.InvoiceNO = "Grand Total:";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new SalesProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.CustomerName = row.CustomerName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    spr1.Rate = row.Rate;
                                    sprList.Add(spr1);
                                    var spr2 = new SalesProduct_Result();
                                    spr2.InvoiceNO = "Grand Total:";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.ItemName;
                            }
                            else
                            {
                                var spr = new SalesProduct_Result();
                                spr.ItemName = row.ItemName;
                                spr.CustomerName = "Header";
                                sprList.Add(spr);
                                var spr1 = new SalesProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.CustomerName = row.CustomerName;
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
                                var spr1 = new SalesProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.CustomerName = row.CustomerName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                spr1.Rate = row.Rate;
                                sprList.Add(spr1);
                            }
                            else
                            {
                                var spr1 = new SalesProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.CustomerName = row.CustomerName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                spr1.Rate = row.Rate;
                                sprList.Add(spr1);
                                var spr2 = new SalesProduct_Result();
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


                List<SalesProduct_Result> sprList = new List<SalesProduct_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal quantitytotal = 0;
                decimal amounttotal = 0;

                try
                {

                    var result = db.SalesProduct(id, FromDate, ToDate, companyid, Branchid).ToList();


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
                                        var spr = new SalesProduct_Result();
                                        spr.ItemName = row.ItemName;
                                        spr.CustomerName = "Header";
                                        sprList.Add(spr);
                                        var spr1 = new SalesProduct_Result();
                                        spr1.ItemName = row.ItemName;
                                        spr1.CustomerName = row.CustomerName;
                                        spr1.Quantity = row.Quantity;
                                        spr1.TotalAmount = row.TotalAmount;
                                        spr1.InvoiceNO = row.InvoiceNO;
                                        spr1.InvoiceDate = row.InvoiceDate;
                                        spr1.Rate = row.Rate;
                                        sprList.Add(spr1);
                                        var spr2 = new SalesProduct_Result();
                                        //     spr2.InvoiceNO = "Grand Total:";
                                        spr2.Quantity = quantitytotal;
                                        spr2.TotalAmount = amounttotal;
                                        sprList.Add(spr2);
                                    }
                                    else
                                    {
                                        var spr1 = new SalesProduct_Result();
                                        spr1.ItemName = row.ItemName;
                                        spr1.CustomerName = row.CustomerName;
                                        spr1.Quantity = row.Quantity;
                                        spr1.TotalAmount = row.TotalAmount;
                                        spr1.InvoiceNO = row.InvoiceNO;
                                        spr1.InvoiceDate = row.InvoiceDate;
                                        spr1.Rate = row.Rate;
                                        sprList.Add(spr1);
                                        var spr2 = new SalesProduct_Result();
                                        //  spr2.InvoiceNO = "Grand Total";
                                        spr2.Quantity = quantitytotal;
                                        spr2.TotalAmount = amounttotal;
                                        sprList.Add(spr2);
                                    }

                                    oldproduct = row.ItemName;
                                }
                                else
                                {
                                    var spr = new SalesProduct_Result();
                                    spr.ItemName = row.ItemName;
                                    spr.CustomerName = "Header";
                                    sprList.Add(spr);
                                    var spr1 = new SalesProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.CustomerName = row.CustomerName;
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
                                    var spr1 = new SalesProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.CustomerName = row.CustomerName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    //   spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    spr1.Rate = row.Rate;
                                    sprList.Add(spr1);
                                }
                                else
                                {
                                    var spr1 = new SalesProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.CustomerName = row.CustomerName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    //  spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    spr1.Rate = row.Rate;
                                    sprList.Add(spr1);
                                    var spr2 = new SalesProduct_Result();
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
        public ActionResult SalesProductPDF(int comp, long branchid, int id, DateTime? fromDate, DateTime? toDate)
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
           
                List<SalesProduct_Result> sprList = new List<SalesProduct_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal quantitytotal = 0;
                decimal amounttotal = 0;
                var result = db.SalesProduct(id, fromDate, toDate, comp, branchid).ToList();

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
                                    var spr = new SalesProduct_Result();
                                    spr.ItemName = row.ItemName;
                                    spr.CustomerName = "Header";
                                    sprList.Add(spr);
                                    var spr1 = new SalesProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.CustomerName = row.CustomerName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    sprList.Add(spr1);
                                    var spr2 = new SalesProduct_Result();
                                    spr2.InvoiceNO = "Grand Total:";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new SalesProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.CustomerName = row.CustomerName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    sprList.Add(spr1);
                                    var spr2 = new SalesProduct_Result();
                                    spr2.InvoiceNO = "Grand Total:";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.ItemName;
                            }
                            else
                            {
                                var spr = new SalesProduct_Result();
                                spr.ItemName = row.ItemName;
                                spr.CustomerName = "Header";
                                sprList.Add(spr);
                                var spr1 = new SalesProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.CustomerName = row.CustomerName;
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
                                var spr1 = new SalesProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.CustomerName = row.CustomerName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);
                            }
                            else
                            {
                                var spr1 = new SalesProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.CustomerName = row.CustomerName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);
                                var spr2 = new SalesProduct_Result();
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



                List<SalesProduct_Result> sprList = new List<SalesProduct_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal quantitytotal = 0;
                decimal amounttotal = 0;
                var result = db.SalesProduct(id, fromDate, toDate, comp, branchid).ToList();
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
                                    var spr = new SalesProduct_Result();
                                    spr.ItemName = row.ItemName;
                                    spr.CustomerName = "Header";
                                    sprList.Add(spr);
                                    var spr1 = new SalesProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.CustomerName = row.CustomerName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    sprList.Add(spr1);
                                    var spr2 = new SalesProduct_Result();
                                    spr2.InvoiceNO = "Grand Total:";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new SalesProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.CustomerName = row.CustomerName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    sprList.Add(spr1);
                                    var spr2 = new SalesProduct_Result();
                                    spr2.InvoiceNO = "Grand Total";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.ItemName;
                            }
                            else
                            {
                                var spr = new SalesProduct_Result();
                                spr.ItemName = row.ItemName;
                                spr.CustomerName = "Header";
                                sprList.Add(spr);
                                var spr1 = new SalesProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.CustomerName = row.CustomerName;
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
                                var spr1 = new SalesProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.CustomerName = row.CustomerName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);
                            }
                            else
                            {
                                var spr1 = new SalesProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.CustomerName = row.CustomerName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);
                                var spr2 = new SalesProduct_Result();
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
        public ActionResult SalesProductPrint(int id)
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




                List<SalesProduct_Result> sprList = new List<SalesProduct_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal quantitytotal = 0;
                decimal amounttotal = 0;
                var result = db.SalesProduct(id, fromDate, toDate, compid, Branchid).ToList();

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
                                    var spr = new SalesProduct_Result();
                                    spr.ItemName = row.ItemName;
                                    spr.CustomerName = "Header";
                                    sprList.Add(spr);
                                    var spr1 = new SalesProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.CustomerName = row.CustomerName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    sprList.Add(spr1);
                                    var spr2 = new SalesProduct_Result();
                                    spr2.InvoiceNO = "Grand Total:";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new SalesProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.CustomerName = row.CustomerName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    sprList.Add(spr1);
                                    var spr2 = new SalesProduct_Result();
                                    spr2.InvoiceNO = "Grand Total:";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.ItemName;
                            }
                            else
                            {
                                var spr = new SalesProduct_Result();
                                spr.ItemName = row.ItemName;
                                spr.CustomerName = "Header";
                                sprList.Add(spr);
                                var spr1 = new SalesProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.CustomerName = row.CustomerName;
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
                                var spr1 = new SalesProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.CustomerName = row.CustomerName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);
                            }
                            else
                            {
                                var spr1 = new SalesProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.CustomerName = row.CustomerName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);
                                var spr2 = new SalesProduct_Result();
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

                List<SalesProduct_Result> sprList = new List<SalesProduct_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal quantitytotal = 0;
                decimal amounttotal = 0;
                var result = db.SalesProduct(id, fromDate, toDate, compid, Branchid).ToList();
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
                                    var spr = new SalesProduct_Result();
                                    spr.ItemName = row.ItemName;
                                    spr.CustomerName = "Header";
                                    sprList.Add(spr);
                                    var spr1 = new SalesProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.CustomerName = row.CustomerName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    sprList.Add(spr1);
                                    var spr2 = new SalesProduct_Result();
                                    spr2.InvoiceNO = "Grand Total:";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new SalesProduct_Result();
                                    spr1.ItemName = row.ItemName;
                                    spr1.CustomerName = row.CustomerName;
                                    spr1.Quantity = row.Quantity;
                                    spr1.TotalAmount = row.TotalAmount;
                                    spr1.InvoiceNO = row.InvoiceNO;
                                    spr1.InvoiceDate = row.InvoiceDate;
                                    sprList.Add(spr1);
                                    var spr2 = new SalesProduct_Result();
                                    spr2.InvoiceNO = "Grand Total";
                                    spr2.Quantity = quantitytotal;
                                    spr2.TotalAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.ItemName;
                            }
                            else
                            {
                                var spr = new SalesProduct_Result();
                                spr.ItemName = row.ItemName;
                                spr.CustomerName = "Header";
                                sprList.Add(spr);
                                var spr1 = new SalesProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.CustomerName = row.CustomerName;
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
                                var spr1 = new SalesProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.CustomerName = row.CustomerName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);
                            }
                            else
                            {
                                var spr1 = new SalesProduct_Result();
                                spr1.ItemName = row.ItemName;
                                spr1.CustomerName = row.CustomerName;
                                spr1.Quantity = row.Quantity;
                                spr1.TotalAmount = row.TotalAmount;
                                spr1.InvoiceNO = row.InvoiceNO;
                                spr1.InvoiceDate = row.InvoiceDate;
                                sprList.Add(spr1);
                                var spr2 = new SalesProduct_Result();
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
        public ActionResult SalesbyProductPDFlink()
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

                return new ActionAsPdf("SalesProductPDF", new { comp = compid, branchid = Branchid, id = ProdId, fromDate = fromDate, toDate = toDate }) { FileName = "SalesByProduct.pdf" };
            }
            catch
            {
                return new ActionAsPdf("SalesProductPDF", new { comp = compid, branchid = Branchid, id = ProdId, fromDate = fromDate, toDate = toDate }) { FileName = "SalesByProduct.pdf" };
            }

        }




        #endregion





        #region ---- Sales By Date--------------

        [HttpGet]
        public ActionResult SalesbyDate(string Msg, string Err)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
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
            ViewBag.Ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.groupID == 113).Select(d => new { d.LID, d.ledgerName }).ToList();
            ViewBag.Customer = db.Customers.Where(d => d.CompanyId == companyid).Select(d => new { d.Id, d.Name }).OrderBy(d=>d.Name).ToList();
            ViewBag.ShedList = db.ShedMasters.Where(d => d.BranchId == Branchid).ToList();
            var mode = new SelectList(new[]
                             {
                                              new{ID=0,Name="ALL"},
                                              new{ID=2,Name="Cash"},
                                              new{ID=1,Name="Credit"},


                                          },
         "ID", "Name");
            ViewData["mod"] = mode;
            return View();


        }


        [HttpGet]
        public ActionResult SalesbyDateReport( string From, string To, long? ledgerId=0, int? Mode=0, long? customerId=0, int? shedId=0)
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
            Session["Mode"] = Mode;
            Session["customerId"] = customerId;
            Session["shedId"] = shedId;

            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            List<SalesInvoiceModelView> result = null;
            
            

            if (From != "0")
            {
                Session["datetime"] = 1;

                if (Branchid == 0)
                {

                    if (Mode == 0)
                    {
                        if (ledgerId == 0)
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct) }).OrderBy(s => s.CreatedOn).ToList();
                        else
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                    }
                    else
                    {
                        if (ledgerId == 0)
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct) }).OrderBy(s => s.CreatedOn).ToList();
                        else
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                    }
                    
                }
                else
                {
                    if (shedId == 0)
                    {
                        if (customerId == 0)
                        {
                            if (Mode == 0)
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                            else
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                        }
                        else
                        {
                            if (Mode == 0)
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.CustomerId == customerId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.CustomerId == customerId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                            else
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode && s.SalesInvoice.CustomerId == customerId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode && s.SalesInvoice.CustomerId == customerId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                            }

                        }
                    }
                    else
                    {
                        if (customerId == 0)
                        {
                            if (Mode == 0)
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.ShedId == shedId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.ShedId == shedId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                            else
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode && s.SalesInvoice.ShedId == shedId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode && s.SalesInvoice.ShedId == shedId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                        }
                        else
                        {
                            if (Mode == 0)
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.CustomerId == customerId && s.SalesInvoice.ShedId == shedId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.CustomerId == customerId && s.SalesInvoice.ShedId == shedId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                            else
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode && s.SalesInvoice.CustomerId == customerId && s.SalesInvoice.ShedId == shedId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode && s.SalesInvoice.CustomerId == customerId && s.SalesInvoice.ShedId == shedId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                            }

                        }
                    }

                }

                return PartialView(result);
            }
            else
            {
                Session["datetime"] = 0;


                if (ledgerId == 0)
                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid ).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Quantity * d.DiscountAmount), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct) }).OrderBy(s => s.CreatedOn).ToList();
                else
                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Quantity * d.DiscountAmount), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct) }).OrderBy(s => s.CreatedOn).ToList();





                return PartialView(result);
            }


        }



        public FileResult SalesDateExport()
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();

            string date = "";

            date = Session["datetime"].ToString();
            if (date == "1")
            {


                DateTime? fromDate =(DateTime?)Session["fdate"];
                DateTime? toDate = (DateTime?)Session["tdate"];

                //Get the data representing the current grid state - page, sort and filter


                var result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.Date >= fromDate && s.Date <= toDate).ToList();


                //Create new Excel workbook
                var workbook = new HSSFWorkbook();

                //Create new Excel sheet
                var sheet = workbook.CreateSheet();

                //(Optional) set the width of the columns
                sheet.SetColumnWidth(0, 50 * 56);
                sheet.SetColumnWidth(1, 50 * 56);
                sheet.SetColumnWidth(2, 50 * 56);
                sheet.SetColumnWidth(3, 50 * 156);
                sheet.SetColumnWidth(4, 50 * 56);
                sheet.SetColumnWidth(5, 50 * 86);
                sheet.SetColumnWidth(6, 50 * 86);
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
                cell.SetCellValue("Sales By Date");
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
                cell.SetCellValue("Sub Total");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(5);
                cell.SetCellValue("Discount");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(6);
                cell.SetCellValue("Net Total");
                cell.CellStyle = detailCellStyle;


                sheet.CreateFreezePane(0, 1, 0, 1);



                //Populate the sheet with values from the grid data
                decimal? subtotal = 0;
                decimal? discounttotal = 0;
                decimal? total = 0;
                int serial = 0;
                foreach (var product in result)
                {
                    var sub = (decimal)(product.BCGrandTotal + product.Dis );
                    subtotal += sub;
                    var discount = product.Dis;
                    discounttotal += discount;
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
                    row.CreateCell(4).SetCellValue(Math.Round((double)sub,2));
                    row.CreateCell(5).SetCellValue(Math.Round((double)discount, 2));
                    row.CreateCell(6).SetCellValue(Math.Round((double)gtotal, 2));

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

                var jan = rowTotal.CreateCell(6);
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
                    "SalesByDate.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user




            }
            else
            {
                DateTime? fromDate = (DateTime?)Session["fdate"];
                DateTime? toDate = (DateTime?)Session["tdate"];

                //Get the data representing the current grid state - page, sort and filter


                var result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid ).ToList();


                //Create new Excel workbook
                var workbook = new HSSFWorkbook();

                //Create new Excel sheet
                var sheet = workbook.CreateSheet();

                //(Optional) set the width of the columns
                sheet.SetColumnWidth(0, 50 * 56);
                sheet.SetColumnWidth(1, 50 * 56);
                sheet.SetColumnWidth(2, 50 * 56);
                sheet.SetColumnWidth(3, 50 * 156);
                sheet.SetColumnWidth(4, 50 * 56);
                sheet.SetColumnWidth(5, 50 * 86);
                sheet.SetColumnWidth(6, 50 * 86);
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
                cell.SetCellValue("Sales Till Date");
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
                cell.SetCellValue("Sub Total");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(5);
                cell.SetCellValue("Discount");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(6);
                cell.SetCellValue("Net Total");
                cell.CellStyle = detailCellStyle;


                sheet.CreateFreezePane(0, 1, 0, 1);



                //Populate the sheet with values from the grid data
                decimal? subtotal = 0;
                decimal? discounttotal = 0;
                decimal? total = 0;
                int serial = 0;
                foreach (var product in result)
                {
                    var sub = (decimal)(product.BCGrandTotal + product.Dis);
                    subtotal += sub;
                    var discount = product.Dis;
                    discounttotal += discount;
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
                    row.CreateCell(4).SetCellValue(Math.Round((double)sub, 2));
                    row.CreateCell(5).SetCellValue(Math.Round((double)discount, 2));
                    row.CreateCell(6).SetCellValue(Math.Round((double)gtotal, 2));

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

                var jan = rowTotal.CreateCell(6);
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
                    "SalesByDate.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user

            }



        }


        [HttpGet]
        public ActionResult SalesbyDatePDF(int id, string value, string GST, DateTime? From, DateTime? To)
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
                var result = db.SalesInvoices.Where(s => s.BranchId == id ).ToList();
                return View(result);
            }



        }

        [HttpGet]
        public ActionResult SalesbyDatePrint()
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
        public ActionResult SalesbyDatePDFlink()
        {
           // int compid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt32(Session["companyid"]);
           
            List<SalesInvoiceModelView> result = null;
            string val = Session["datetime"].ToString();
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            //string GST = Session["GST"].ToString();
            int? ledgerId = Convert.ToInt32(Session["ledgerId"].ToString());
            int? Mode = Convert.ToInt32(Session["Mode"].ToString());
            int? customerId = Convert.ToInt32(Session["customerId"].ToString());
            int? shedId = Convert.ToInt32(Session["shedId"].ToString());
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

                    if (Mode == 0)
                    {
                        if (ledgerId == 0)
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct) }).OrderBy(s => s.CreatedOn).ToList();
                        else
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                    }
                    else
                    {
                        if (ledgerId == 0)
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct) }).OrderBy(s => s.CreatedOn).ToList();
                        else
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                    }

                }
                else
                {
                    if (shedId == 0)
                    {
                        if (customerId == 0)
                        {
                            if (Mode == 0)
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                            else
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                        }
                        else
                        {
                            if (Mode == 0)
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.CustomerId == customerId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.CustomerId == customerId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                            else
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode && s.SalesInvoice.CustomerId == customerId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode && s.SalesInvoice.CustomerId == customerId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                            }

                        }
                    }
                    else
                    {
                        if (customerId == 0)
                        {
                            if (Mode == 0)
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.ShedId == shedId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.ShedId == shedId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                            else
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode && s.SalesInvoice.ShedId == shedId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode && s.SalesInvoice.ShedId == shedId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                        }
                        else
                        {
                            if (Mode == 0)
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.CustomerId == customerId && s.SalesInvoice.ShedId == shedId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.CustomerId == customerId && s.SalesInvoice.ShedId == shedId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                            else
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode && s.SalesInvoice.CustomerId == customerId && s.SalesInvoice.ShedId == shedId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode && s.SalesInvoice.CustomerId == customerId && s.SalesInvoice.ShedId == shedId).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                            }

                        }
                    }

                }
                //if (Branchid == 0)
                //{
                //    if (GST == "All")
                //        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct) }).OrderBy(s => s.CreatedOn).ToList();
                //    else if (GST == "Registered")
                //        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.Customer.GstVatNumber != null && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                //    else
                //        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.Customer.GstVatNumber == null && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();


                //from = DateTime.Parse(Convert.ToString(Session["fdate"]));
                //to = DateTime.Parse(Convert.ToString(Session["tdate"]));

                //return new ActionAsPdf("SalesbyDatePDF", new { id = Branchid, value = "1", From = from, To = to }) { FileName = "SalesByDate.pdf" };
                return new Rotativa.PartialViewAsPdf("SalesbyDatePDF", result)
                    {
                        FileName = "SalesbyDatePDF.pdf",
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
                //        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                //    else if (GST == "Registered")
                //        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.Customer.GstVatNumber != null && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                //    else
                //        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.Customer.GstVatNumber == null && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();


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
                return new ActionAsPdf("SalesbyDatePDF", new { id = Branchid, value = "0", From = FromDate, To = ToDate }) { FileName = "SalesByDate.pdf" };
            }

        }




        [HttpPost]
        public ActionResult SalesbyDateMail(string Mailto, string message, string subj, HttpPostedFileBase fileUploader)
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


                return RedirectToAction("SalesbyDate", new { Err = "Please try again...." });
            }


        }



        #endregion

        #region ---- Sales By Product Det--------------

        [HttpGet]
        public ActionResult SalesbyProductDet(string Msg, string Err)
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
            ViewBag.Ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.groupID == 113).Select(d => new { d.LID, d.ledgerName }).ToList();
            var mode = new SelectList(new[]
                             {
                                              new{ID=0,Name="ALL"},
                                              new{ID=2,Name="Cash"},
                                              new{ID=1,Name="Credit"},


                                          },
         "ID", "Name");
            ViewData["mod"] = mode;
            return View();


        }


        [HttpGet]
        public ActionResult SalesbyProductDetReport(string From, string To, long? ledgerId = 0, int? Mode = 0)
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
            Session["Mode"] = Mode;

            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            List<SalesInvoiceDetail> result = null;
            var ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.groupID == 113).Select(d => new { d.LID, d.ledgerName }).ToList();


            if (From != "0")
            {
                Session["datetime"] = 1;

                if (Branchid == 0)
                {

                    if (Mode == 0)
                    {
                        if (ledgerId == 0)
                        {
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();

                            foreach (var item in result)
                            {
                                item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                            }
                        }
                        else
                        {
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();
                            foreach (var item in result)
                            {
                                item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                            }
                        }
                    }
                    else
                    {
                        if (ledgerId == 0)
                        {
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();


                            foreach (var item in result)
                            {
                                item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                            }
                        }
                        else
                        {
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();

                            foreach (var item in result)
                            {
                                item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                            }
                        }
                    }

                }
                else
                {
                    if (Mode == 0)
                    {
                        if (ledgerId == 0)
                        {
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();


                            foreach (var item in result)
                            {
                                item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                            }
                        }
                        else
                        {
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();

                            foreach (var item in result)
                            {
                                item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                            }
                        }
                    }
                    else
                    {
                        if (ledgerId == 0)
                        {
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();

                            foreach (var item in result)
                            {
                                item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                            }
                        }
                        else
                        {
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();

                            foreach (var item in result)
                            {
                                item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                            }
                        }
                    }
                }

                return PartialView(result);
            }
            else
            {
                Session["datetime"] = 0;

                if (ledgerId == 0)
                {
                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();


                    foreach (var item in result)
                    {
                        item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                    }
                }
                else
                {
                    result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();

                    foreach (var item in result)
                    {
                        item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                    }
                }





                return PartialView(result);
            }


        }



        public FileResult SalesByProductDetExport()
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt32(Session["companyid"]);

            List<SalesInvoiceDetail> result = null;
            string val = Session["datetime"].ToString();
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            //string GST = Session["GST"].ToString();
            int? ledgerId = Convert.ToInt32(Session["ledgerId"].ToString());
            int? Mode = Convert.ToInt32(Session["Mode"].ToString());
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
            var ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.groupID == 113).Select(d => new { d.LID, d.ledgerName }).ToList();
            //Get the data representing the current grid state - page, sort and filter


            if (Branchid == 0)
            {

                if (Mode == 0)
                {
                    if (ledgerId == 0)
                    {
                        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();

                        foreach (var item in result)
                        {
                            item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                        }
                    }
                    else
                    {
                        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();
                        foreach (var item in result)
                        {
                            item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                        }
                    }
                }
                else
                {
                    if (ledgerId == 0)
                    {
                        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();


                        foreach (var item in result)
                        {
                            item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                        }
                    }
                    else
                    {
                        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();

                        foreach (var item in result)
                        {
                            item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                        }
                    }
                }

            }
            else
            {
                if (Mode == 0)
                {
                    if (ledgerId == 0)
                    {
                        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();


                        foreach (var item in result)
                        {
                            item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                        }
                    }
                    else
                    {
                        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();

                        foreach (var item in result)
                        {
                            item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                        }
                    }
                }
                else
                {
                    if (ledgerId == 0)
                    {
                        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();

                        foreach (var item in result)
                        {
                            item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                        }
                    }
                    else
                    {
                        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();

                        foreach (var item in result)
                        {
                            item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                        }
                    }
                }
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
                cell.SetCellValue("Sales By Product Details");
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
                cell.SetCellValue("Reference");
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
                cell.SetCellValue("Total");
                cell.CellStyle = detailCellStyle;


                sheet.CreateFreezePane(0, 1, 0, 1);



                //Populate the sheet with values from the grid data
                decimal? subtotal = 0;
                decimal? discounttotal = 0;
                decimal? total = 0;
                int serial = 0;
                foreach (var product in result)
                {
                    decimal secondaryQuantity = 0;
                    serial++;
                    if(product.Product.CategoryId == 13)
                    {
                        secondaryQuantity = product.UnitFormula;
                    }
                    else
                    {
                        secondaryQuantity = product.Quantity;
                    }


                //Create a new row
                var row = sheet.CreateRow(rowNumber++);
                    //   total = total + product.Amount;
                    //Set values for the cells
                    row.CreateCell(0).SetCellValue(serial);
                    row.CreateCell(1).SetCellValue(String.Format(Session["DateFormatUpper"].ToString(), product.SalesInvoice.InvoiceDate));
                    row.CreateCell(2).SetCellValue(product.SalesInvoice.NO);
                    row.CreateCell(3).SetCellValue(product.SalesInvoice.Reference);
                    row.CreateCell(4).SetCellValue(product.SalesInvoice.Customer.Name);
                    row.CreateCell(5).SetCellValue(product.Product.Description);
                    row.CreateCell(6).SetCellValue(Math.Round((double)product.Quantity, 2));
                    row.CreateCell(7).SetCellValue(product.UOM1.Description);
                    row.CreateCell(8).SetCellValue(Math.Round((double)secondaryQuantity, 2));
                    row.CreateCell(9).SetCellValue(product.UOM.Description);
                    row.CreateCell(10).SetCellValue(Math.Round((double)product.Price, 2));
                    row.CreateCell(11).SetCellValue(Math.Round((double)product.TotalAmount, 2));

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
                    "SalesByProductDetails.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user




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
        public ActionResult SalesbyProductDetPDF(int id, string value, string GST, DateTime? From, DateTime? To)
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
        public ActionResult SalesbyProductDetPDFlink()
        {
            // int compid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt32(Session["companyid"]);

            List<SalesInvoiceDetail> result = null;
            string val = Session["datetime"].ToString();
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            //string GST = Session["GST"].ToString();
            int? ledgerId = Convert.ToInt32(Session["ledgerId"].ToString());
            int? Mode = Convert.ToInt32(Session["Mode"].ToString());
            DateTime? FromDate = null;
            DateTime? ToDate = null;
            try
            {
                FromDate = DateTime.Parse(Convert.ToString(Session["fdate"]));
                ToDate = DateTime.Parse(Convert.ToString(Session["tdate"]));
                ViewBag.from = FromDate;
                ViewBag.to = ToDate;
                var ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.groupID == 113).Select(d => new { d.LID, d.ledgerName }).ToList();

                //Get the data representing the current grid state - page, sort and filter
                if (Branchid == 0)
                {

                    if (Mode == 0)
                    {
                        if (ledgerId == 0)
                        {
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();

                            foreach (var item in result)
                            {
                                item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                            }
                        }
                        else
                        {
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();
                            foreach (var item in result)
                            {
                                item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                            }
                        }
                    }
                    else
                    {
                        if (ledgerId == 0)
                        {
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();


                            foreach (var item in result)
                            {
                                item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                            }
                        }
                        else
                        {
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();

                            foreach (var item in result)
                            {
                                item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                            }
                        }
                    }

                }
                else
                {
                    if (Mode == 0)
                    {
                        if (ledgerId == 0)
                        {
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();


                            foreach (var item in result)
                            {
                                item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                            }
                        }
                        else
                        {
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();

                            foreach (var item in result)
                            {
                                item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                            }
                        }
                    }
                    else
                    {
                        if (ledgerId == 0)
                        {
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();

                            foreach (var item in result)
                            {
                                item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                            }
                        }
                        else
                        {
                            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).OrderBy(d => d.SalesInvoice.InvoiceDate).ToList();

                            foreach (var item in result)
                            {
                                item.Description = ledger.Where(d => d.LID == item.SalesInvoice.LID).Select(d => d.ledgerName).FirstOrDefault();
                            }
                        }
                    }
                }
                //if (Branchid == 0)
                //{
                //    if (GST == "All")
                //        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct) }).OrderBy(s => s.CreatedOn).ToList();
                //    else if (GST == "Registered")
                //        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.Customer.GstVatNumber != null && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                //    else
                //        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.Customer.GstVatNumber == null && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();


                //from = DateTime.Parse(Convert.ToString(Session["fdate"]));
                //to = DateTime.Parse(Convert.ToString(Session["tdate"]));

                //return new ActionAsPdf("SalesbyDatePDF", new { id = Branchid, value = "1", From = from, To = to }) { FileName = "SalesByDate.pdf" };
                return new Rotativa.PartialViewAsPdf("SalesbyProductDetPDF", result)
                {
                    FileName = "SalesbyProductDetPDF.pdf",
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
                //        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                //    else if (GST == "Registered")
                //        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.Customer.GstVatNumber != null && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                //    else
                //        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.Customer.GstVatNumber == null && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();


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
                return new ActionAsPdf("SalesbyProductDetPDF", new { id = Branchid, value = "0", From = FromDate, To = ToDate }) { FileName = "SalesByProductDet.pdf" };
            }

        }




        [HttpPost]
        public ActionResult SalesbyProductDetMail(string Mailto, string message, string subj, HttpPostedFileBase fileUploader)
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


                return RedirectToAction("SalesbyProductDetDate", new { Err = "Please try again...." });
            }


        }



        #endregion

        #region SalesTax Report

        public ActionResult SalesTax()
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

        [HttpPost]
        public ActionResult SalestaxReport(int id, DateTime from, DateTime to)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);

            Session["fdate"] = from;
            Session["tdate"] = to;

            Session["productid"] = id.ToString();
            if (id != 0)
            {

                List<SaleTax_Result> sprList = new List<SaleTax_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal ratetotal = 0;
                decimal amounttotal = 0;
                var result = db.SaleTax(id, from, to, companyid, Branchid).ToList();
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
                                    var spr = new SaleTax_Result();
                                    spr.Taxname = row.Taxname;
                                    spr.Item = "Header";
                                    spr.Rate = ratetotal;
                                    spr.TaxAmount = amounttotal;
                                    sprList.Add(spr);
                                    var spr1 = new SaleTax_Result();
                                    spr1.Item = row.Item;
                                    spr1.Rate = row.Rate;
                                    spr1.Taxname = row.Taxname;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new SaleTax_Result();
                                    spr2.InvoiceNo = "Grand Total:";
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new SaleTax_Result();
                                    spr1.Item = row.Item;
                                    spr1.Rate = row.Rate;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.Taxname = row.Taxname;

                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new SaleTax_Result();
                                    spr2.InvoiceNo = "Grand Total:";
                                    spr2.Rate = ratetotal;
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.Taxname;
                            }
                            else
                            {
                                var spr = new SaleTax_Result();

                                spr.Taxname = row.Taxname;
                                spr.TaxAmount = amounttotal;
                                spr.Item = "Header";
                                spr.Rate = row.Rate;
                                sprList.Add(spr);
                                var spr1 = new SaleTax_Result();
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

                                var spr1 = new SaleTax_Result();
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
                                var spr1 = new SaleTax_Result();
                                spr1.Item = row.Item;
                                spr1.Rate = row.Rate;
                                spr1.TaxAmount = row.TaxAmount;
                                spr1.Taxname = row.Taxname;
                                spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Date = row.Date;
                                sprList.Add(spr1);
                                var spr2 = new SaleTax_Result();
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



                List<SaleTax_Result> sprList = new List<SaleTax_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal ratetotal = 0;
                decimal amounttotal = 0;
                var result = db.SaleTax(id, from, to, companyid, Branchid).ToList();




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
                                    var spr = new SaleTax_Result();

                                    spr.Item = "Header";
                                    spr.Taxname = row.Taxname;
                                    spr.Rate = ratetotal;
                                    spr.TaxAmount = amounttotal;
                                    sprList.Add(spr);
                                    var spr1 = new SaleTax_Result();
                                    spr1.Item = row.Item;
                                    spr.Taxname = row.Taxname;
                                    spr.Rate = row.Rate;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new SaleTax_Result();
                                    //     spr2.InvoiceNo = "Grand Total:";
                                    spr2.Rate = ratetotal;
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new SaleTax_Result();
                                    spr1.Item = row.Item;
                                    spr1.Rate = row.Rate;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Taxname = row.Taxname;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new SaleTax_Result();
                                    //        spr2.InvoiceNo = "Grand Total:";
                                    spr2.Rate = ratetotal;
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.Taxname;
                            }
                            else
                            {
                                var spr = new SaleTax_Result();
                                spr.Item = "Header";
                                spr.Taxname = row.Taxname;
                                spr.Rate = ratetotal;
                                sprList.Add(spr);

                                var spr1 = new SaleTax_Result();
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

                                var spr1 = new SaleTax_Result();
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
                                var spr1 = new SaleTax_Result();
                                spr1.Item = row.Item;

                                spr1.Rate = row.Rate;
                                spr1.TaxAmount = row.TaxAmount;
                                //spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Taxname = row.Taxname;

                                spr1.Date = row.Date;
                                sprList.Add(spr1);
                                var spr2 = new SaleTax_Result();
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

        public ActionResult TestOffline()
        {

            return View();
        }

        [HttpGet]
        public ActionResult SaleTaxPDF(int comp, long branchid, int id, string from, string to)
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

                List<SaleTax_Result> sprList = new List<SaleTax_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal ratetotal = 0;
                decimal amounttotal = 0;
                var result = db.SaleTax(id, fromDate, toDate, comp, Branchid).ToList();
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
                                    var spr = new SaleTax_Result();
                                    spr.Taxname = row.Taxname;
                                    spr.Item = "Header";
                                    spr.Rate = ratetotal;
                                    spr.TaxAmount = amounttotal;
                                    sprList.Add(spr);
                                    var spr1 = new SaleTax_Result();
                                    spr1.Item = row.Item;
                                    spr1.Rate = row.Rate;
                                    spr1.Taxname = row.Taxname;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new SaleTax_Result();
                                    spr2.InvoiceNo = "Grand Total:";
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new SaleTax_Result();
                                    spr1.Item = row.Item;
                                    spr1.Rate = row.Rate;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.Taxname = row.Taxname;

                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new SaleTax_Result();
                                    spr2.InvoiceNo = "Grand Total:";
                                    spr2.Rate = ratetotal;
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.Taxname;
                            }
                            else
                            {
                                var spr = new SaleTax_Result();

                                spr.Taxname = row.Taxname;
                                spr.TaxAmount = amounttotal;
                                spr.Item = "Header";
                                spr.Rate = row.Rate;
                                sprList.Add(spr);
                                var spr1 = new SaleTax_Result();
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

                                var spr1 = new SaleTax_Result();
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
                                var spr1 = new SaleTax_Result();
                                spr1.Item = row.Item;
                                spr1.Rate = row.Rate;
                                spr1.TaxAmount = row.TaxAmount;
                                spr1.Taxname = row.Taxname;
                                spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Date = row.Date;
                                sprList.Add(spr1);
                                var spr2 = new SaleTax_Result();
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


                List<SaleTax_Result> sprList = new List<SaleTax_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal ratetotal = 0;
                decimal amounttotal = 0;
                var result = db.SaleTax(id, fromDate, toDate, comp, Branchid).ToList();




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
                                    var spr = new SaleTax_Result();

                                    spr.Item = "Header";
                                    spr.Taxname = row.Taxname;
                                    spr.Rate = ratetotal;
                                    spr.TaxAmount = amounttotal;
                                    sprList.Add(spr);
                                    var spr1 = new SaleTax_Result();
                                    spr1.Item = row.Item;
                                    spr.Taxname = row.Taxname;
                                    spr.Rate = row.Rate;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new SaleTax_Result();
                                    //     spr2.InvoiceNo = "Grand Total:";
                                    spr2.Rate = ratetotal;
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new SaleTax_Result();
                                    spr1.Item = row.Item;
                                    spr1.Rate = row.Rate;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Taxname = row.Taxname;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new SaleTax_Result();
                                    //        spr2.InvoiceNo = "Grand Total:";
                                    spr2.Rate = ratetotal;
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.Taxname;
                            }
                            else
                            {
                                var spr = new SaleTax_Result();
                                spr.Item = "Header";
                                spr.Taxname = row.Taxname;
                                spr.Rate = ratetotal;
                                sprList.Add(spr);

                                var spr1 = new SaleTax_Result();
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

                                var spr1 = new SaleTax_Result();
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
                                var spr1 = new SaleTax_Result();
                                spr1.Item = row.Item;

                                spr1.Rate = row.Rate;
                                spr1.TaxAmount = row.TaxAmount;
                                //spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Taxname = row.Taxname;

                                spr1.Date = row.Date;
                                sprList.Add(spr1);
                                var spr2 = new SaleTax_Result();
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
        public ActionResult SalesbyTaxPDFlink()
        {
            int compid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            string ProdId = Session["productid"].ToString();

            try
            {
                string from = Session["fdate"].ToString();
                string to = Session["tdate"].ToString();
                return new ActionAsPdf("SaleTaxPDF", new { comp = compid, branchid = Branchid, id = ProdId, from = from, to = to }) { FileName = "SalesTax.pdf" };
            }
            catch
            {
                return new ActionAsPdf("SaleTaxPDF", new { comp = compid, branchid = Branchid, id = ProdId, from = 0, to = 0 }) { FileName = "SalesTax.pdf" };
            }

        }

        [HttpGet]
        public ActionResult SalesTaxPrint(int id)
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


                List<SaleTax_Result> sprList = new List<SaleTax_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal ratetotal = 0;
                decimal amounttotal = 0;
                var result = db.SaleTax(id, fromDate, toDate, companyid, Branchid).ToList();
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
                                    var spr = new SaleTax_Result();
                                    spr.Taxname = row.Taxname;
                                    spr.Item = "Header";
                                    spr.Rate = ratetotal;
                                    spr.TaxAmount = amounttotal;
                                    sprList.Add(spr);
                                    var spr1 = new SaleTax_Result();
                                    spr1.Item = row.Item;
                                    spr1.Rate = row.Rate;
                                    spr1.Taxname = row.Taxname;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new SaleTax_Result();
                                    spr2.InvoiceNo = "Grand Total:";
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new SaleTax_Result();
                                    spr1.Item = row.Item;
                                    spr1.Rate = row.Rate;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.Taxname = row.Taxname;

                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new SaleTax_Result();
                                    spr2.InvoiceNo = "Grand Total:";
                                    spr2.Rate = ratetotal;
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.Taxname;
                            }
                            else
                            {
                                var spr = new SaleTax_Result();

                                spr.Taxname = row.Taxname;
                                spr.TaxAmount = amounttotal;
                                spr.Item = "Header";
                                spr.Rate = row.Rate;
                                sprList.Add(spr);
                                var spr1 = new SaleTax_Result();
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

                                var spr1 = new SaleTax_Result();
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
                                var spr1 = new SaleTax_Result();
                                spr1.Item = row.Item;
                                spr1.Rate = row.Rate;
                                spr1.TaxAmount = row.TaxAmount;
                                spr1.Taxname = row.Taxname;
                                spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Date = row.Date;
                                sprList.Add(spr1);
                                var spr2 = new SaleTax_Result();
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

                List<SaleTax_Result> sprList = new List<SaleTax_Result>();
                int counter = 0;
                int productcount = 0;
                string oldproduct = string.Empty;
                decimal ratetotal = 0;
                decimal amounttotal = 0;
                var result = db.SaleTax(id, fromDate, toDate, compid, Branchid).ToList();




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
                                    var spr = new SaleTax_Result();

                                    spr.Item = "Header";
                                    spr.Taxname = row.Taxname;
                                    spr.Rate = ratetotal;
                                    spr.TaxAmount = amounttotal;
                                    sprList.Add(spr);
                                    var spr1 = new SaleTax_Result();
                                    spr1.Item = row.Item;
                                    spr.Taxname = row.Taxname;
                                    spr.Rate = row.Rate;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new SaleTax_Result();
                                    //     spr2.InvoiceNo = "Grand Total:";
                                    spr2.Rate = ratetotal;
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }
                                else
                                {
                                    var spr1 = new SaleTax_Result();
                                    spr1.Item = row.Item;
                                    spr1.Rate = row.Rate;
                                    spr1.TaxAmount = row.TaxAmount;
                                    spr1.InvoiceNo = row.InvoiceNo;
                                    spr1.Taxname = row.Taxname;
                                    spr1.Date = row.Date;
                                    sprList.Add(spr1);
                                    var spr2 = new SaleTax_Result();
                                    //        spr2.InvoiceNo = "Grand Total:";
                                    spr2.Rate = ratetotal;
                                    spr2.TaxAmount = amounttotal;
                                    sprList.Add(spr2);
                                }

                                oldproduct = row.Taxname;
                            }
                            else
                            {
                                var spr = new SaleTax_Result();
                                spr.Item = "Header";
                                spr.Taxname = row.Taxname;
                                spr.Rate = ratetotal;
                                sprList.Add(spr);

                                var spr1 = new SaleTax_Result();
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

                                var spr1 = new SaleTax_Result();
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
                                var spr1 = new SaleTax_Result();
                                spr1.Item = row.Item;

                                spr1.Rate = row.Rate;
                                spr1.TaxAmount = row.TaxAmount;
                                //spr1.InvoiceNo = row.InvoiceNo;
                                spr1.Taxname = row.Taxname;

                                spr1.Date = row.Date;
                                sprList.Add(spr1);
                                var spr2 = new SaleTax_Result();
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

        #region Income Report

        [HttpGet]
        public ActionResult IncomeReport()
        {
            return View();
        }

        [HttpPost]
        public ActionResult IncomeReportCustomer(int? id, string name)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                Session["id"] = id;
                int? companyid = Convert.ToInt32(Session["companyid"]);
                int? Branchid = Convert.ToInt32(Session["BranchId"]);
                List<IncomeReportCustomerWiseModelView> sprList = new List<IncomeReportCustomerWiseModelView>();
                var result = db.IncomeReport_CustomerWise(companyid, Branchid, id).ToList();
                decimal? SalesPriceTotal = result.Sum(s => s.SPrice) ?? 0;
                decimal? PurchasePriceTotal = result.Sum(s => s.PPrice) ?? 0;
                decimal? ProfitTotal = (SalesPriceTotal - PurchasePriceTotal) ?? 0;
                foreach (var item in result)
                {
                    var spr = new IncomeReportCustomerWiseModelView();
                    spr.Name = item.Name;
                    spr.PQuantity = item.PQuantity;
                    spr.PPrice = item.PPrice;
                    spr.SPrice = item.SPrice;
                    spr.Profit = item.Profit;
                    spr.NO = item.NO;
                    spr.SPriceTotal = SalesPriceTotal;
                    spr.PPriceTotal = PurchasePriceTotal;
                    spr.ProfitTotal = ProfitTotal;
                    sprList.Add(spr);
                }

                return PartialView(sprList);
            }
        }

        [HttpPost]
        public JsonResult GetCustomerDetail(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            using (InventoryEntities db = new InventoryEntities())
            {
                var getCustomer = db.Customers.Where(r => r.Name.Contains(query) && r.CompanyId == companyid).Select(s => new { Name = s.Name, Id = s.Id }).ToList();
                return Json(getCustomer, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetSelectedCustomerDetail(int id)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            using (InventoryEntities db = new InventoryEntities())
            {
                var customer = db.Customers.Where(p => p.Id == id).Select(p => new { Name = p.Name, Id = p.Id }).FirstOrDefault();
                return Json(customer, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult IncomeReportCustomerPDFlink(string fdate, string tdate)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int id = Convert.ToInt32(Session["id"]);
            string From = fdate;
            string To = tdate;

            try
            {

                return new ActionAsPdf("IncomeReportCustomerPDF", new { comId = companyid, branId = Branchid, from = From, to = To, id = id }) { FileName = "IncomeReport.pdf" };
            }
            catch
            {
                return new ActionAsPdf("IncomeReportCustomerPDF", new { from = 0 }) { FileName = "IncomeReport.pdf" };
            }

        }

        [HttpGet]
        public ActionResult IncomeReportCustomerPDF(int comId, int branId, string from, string to, int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int? companyid = comId;
                int? Branchid = branId;

                var detls = db.Companies.Where(c => c.Id == companyid).FirstOrDefault();
                ViewBag.Name = detls.Name;
                ViewBag.address = detls.Address;
                ViewBag.pin = detls.Zipcode;
                ViewBag.from = from;
                ViewBag.to = to;

                List<IncomeReportCustomerWiseModelView> sprList = new List<IncomeReportCustomerWiseModelView>();
                var result = db.IncomeReport_CustomerWise(companyid, Branchid, id).ToList();
                decimal? SalesPriceTotal = result.Sum(s => s.SPrice) ?? 0;
                decimal? PurchasePriceTotal = result.Sum(s => s.PPrice) ?? 0;
                decimal? ProfitTotal = (SalesPriceTotal - PurchasePriceTotal) ?? 0;
                foreach (var item in result)
                {
                    var spr = new IncomeReportCustomerWiseModelView();
                    spr.Name = item.Name;
                    spr.PQuantity = item.PQuantity;
                    spr.PPrice = item.PPrice;
                    spr.SPrice = item.SPrice;
                    spr.Profit = item.Profit;
                    spr.NO = item.NO;
                    spr.SPriceTotal = SalesPriceTotal;
                    spr.PPriceTotal = PurchasePriceTotal;
                    spr.ProfitTotal = ProfitTotal;
                    sprList.Add(spr);
                }

                return View(sprList);
            }


        }

        #endregion

        #region Income Report By Invoice
        [HttpGet]
        public ActionResult IncomeReportInv()
        {
            return View();
        }

        [HttpPost]
        public ActionResult IncomeReportInvWise(int? id, string name)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                Session["id"] = id;
                int? companyid = Convert.ToInt32(Session["companyid"]);
                int? Branchid = Convert.ToInt32(Session["BranchId"]);
                List<IncomeReport_Result> sprList = new List<IncomeReport_Result>();
                var result = db.IncomeReport(companyid, Branchid, id).ToList();
                foreach (var item in result)
                {
                    var spr = new IncomeReport_Result();
                    spr.Name = item.Name;
                    spr.PQuantity = item.PQuantity;
                    spr.PPrice = item.PPrice;
                    spr.SPrice = item.SPrice;
                    spr.Profit = item.Profit;
                    spr.NO = item.NO;
                    sprList.Add(spr);
                }

                return PartialView(sprList);
            }
        }

        [HttpPost]
        public JsonResult getInvoice(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            using (InventoryEntities db = new InventoryEntities())
            {
                var getInvoice = db.SalesInvoices.Where(r => r.NO.Contains(query) && r.CompanyId == companyid).Select(s => new { Name = s.NO, Id = s.Id }).ToList();
                return Json(getInvoice, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getSelectedInvoice(int id)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            using (InventoryEntities db = new InventoryEntities())
            {
                var invoice = db.SalesInvoices.Where(p => p.Id == id).Select(p => new { Name = p.NO, Id = p.Id }).FirstOrDefault();
                return Json(invoice, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult IncomeReportInvoicePDFlink(string fdate, string tdate)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int id = Convert.ToInt32(Session["id"]);
            string From = fdate;
            string To = tdate;

            try
            {

                return new ActionAsPdf("IncomeReportPDF", new { comId = companyid, branId = Branchid, from = From, to = To, id = id }) { FileName = "IncomeReport.pdf" };
            }
            catch
            {
                return new ActionAsPdf("IncomeReportPDF", new { from = 0 }) { FileName = "IncomeReport.pdf" };
            }

        }

        [HttpGet]
        public ActionResult IncomeReportPDF(int comId, int branId, string from, string to, int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int? companyid = comId;
                int? Branchid = branId;

                var detls = db.Companies.Where(c => c.Id == companyid).FirstOrDefault();
                ViewBag.Name = detls.Name;
                ViewBag.address = detls.Address;
                ViewBag.pin = detls.Zipcode;
                ViewBag.from = from;
                ViewBag.to = to;

                List<IncomeReport_Result> sprList = new List<IncomeReport_Result>();
                var result = db.IncomeReport(companyid, Branchid, id).ToList();
                foreach (var item in result)
                {
                    var spr = new IncomeReport_Result();
                    spr.Name = item.Name;
                    spr.PQuantity = item.PQuantity;
                    spr.PPrice = item.PPrice;
                    spr.SPrice = item.SPrice;
                    spr.Profit = item.Profit;
                    spr.NO = item.NO;
                    sprList.Add(spr);
                }

                return View(sprList);
            }


        }

        #endregion


        #region ---- Customer Debit Credit By Date--------------

        [HttpGet]
        public ActionResult CusDCbyDate(string Msg, string Err)
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
        public ActionResult CusDCbyDateReport(string From, string To)
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


                // var result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.Date >= FromDate && s.Date <= ToDate).GroupBy(s=>s.Customer.Name).Select(s => new CustomerDebitCredit {CustomerName=s.Key,Debit=s.Sum(p=>p.BCGrandTotal) }).ToList();
                var result = db.CustomerDebitCredit(FromDate, ToDate).ToList();
                //var result=from r in  db.ReceiptPayments 
                //           join cus in db.Customers on new {ID=Convert.ToInt64(r.ledgerId)} equals new {ID=Convert.ToInt64(cus.LId )}

                //  Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.Date >= FromDate && s.Date <= ToDate)

                return PartialView(result);
            }
            else
            {
                Session["datetime"] = 0;

                var result = db.CustomerDebitCredit(FromDate, ToDate).ToList();





                return PartialView(result);
            }


        }



        public FileResult CusDCDateExport()
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);


            string date = "";

            date = Session["datetime"].ToString();
            if (date == "1")
            {


                DateTime fromDate = DateTime.Parse(Convert.ToString(Session["fdate"]));
                DateTime toDate = DateTime.Parse(Convert.ToString(Session["tdate"]));

                //Get the data representing the current grid state - page, sort and filter


                var result = from sale in db.ReceiptPayments
                             where sale.transactionType == "Sales Invoice" && sale.RPdate >= fromDate && sale.RPdate <= toDate && sale.CompanyId == companyid && sale.BranchId == Branchid
                             group sale by sale.RPdate into sa

                             select new Salesdate
                             {
                                 Date = sa.Key,
                                 Amount = sa.Sum(sal => sal.TotalAmount)

                             };


                //Create new Excel workbook
                var workbook = new HSSFWorkbook();

                //Create new Excel sheet
                var sheet = workbook.CreateSheet();

                //(Optional) set the width of the columns
                sheet.SetColumnWidth(0, 50 * 56);
                sheet.SetColumnWidth(1, 50 * 56);
                sheet.SetColumnWidth(2, 50 * 156);
                sheet.SetColumnWidth(3, 50 * 156);
                sheet.SetColumnWidth(4, 50 * 56);
                sheet.SetColumnWidth(5, 50 * 86);
                sheet.SetColumnWidth(6, 50 * 86);
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
                cell.SetCellValue("Sales By Date");
                cell.CellStyle = HeaderCellStyle;

                row11 = sheet.CreateRow(rowNumber++);
                row11 = sheet.CreateRow(rowNumber++);
                cell = row11.CreateCell(3);
                cell.SetCellValue("For " + String.Format(Session["DateFormatUpper"].ToString(), fromDate.ToShortDateString()) + " To " + String.Format(Session["DateFormatUpper"].ToString(), toDate.ToShortDateString()));


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
                cell.SetCellValue("Date");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(1);
                cell.SetCellValue("Amount");
                cell.CellStyle = detailCellStyle;
                cell = headerRow.CreateCell(3);
                sheet.CreateFreezePane(0, 1, 0, 1);



                //Populate the sheet with values from the grid data
                decimal? total = 0;

                foreach (var product in result)
                {
                    //Create a new row
                    var row = sheet.CreateRow(rowNumber++);
                    total = total + product.Amount;
                    //Set values for the cells
                    row.CreateCell(0).SetCellValue(String.Format(Session["DateFormatUpper"].ToString(), product.Date));
                    row.CreateCell(1).SetCellValue(Convert.ToInt64(product.Amount));

                }
                var rowTotal = sheet.CreateRow(rowNumber++);

                var expense = rowTotal.CreateCell(0);
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

                var jan = rowTotal.CreateCell(1);
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
                    "SalesByDate.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user




            }
            else
            {
                var result = from sales in db.ReceiptPayments
                             where sales.transactionType == "Sales Invoice" && sales.CompanyId == companyid && sales.BranchId == Branchid
                             group sales by sales.RPdate into s
                             select new Salesdate
                             {
                                 Date = s.Key,
                                 Amount = s.Sum(sale => sale.TotalAmount)

                             };
                //Create new Excel workbook
                var workbook = new HSSFWorkbook();

                //Create new Excel sheet
                var sheet = workbook.CreateSheet();

                //(Optional) set the width of the columns
                sheet.SetColumnWidth(0, 50 * 56);
                sheet.SetColumnWidth(1, 50 * 56);
                sheet.SetColumnWidth(2, 50 * 156);
                sheet.SetColumnWidth(3, 50 * 156);
                sheet.SetColumnWidth(4, 50 * 56);
                sheet.SetColumnWidth(5, 50 * 86);
                sheet.SetColumnWidth(6, 50 * 86);
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
                cell.SetCellValue("Sales By Date");
                cell.CellStyle = HeaderCellStyle;

                row11 = sheet.CreateRow(rowNumber++);
                row11 = sheet.CreateRow(rowNumber++);
                cell = row11.CreateCell(3);
                //  cell.SetCellValue("For " + fromDate.ToShortDateString() + " To " + toDate.ToShortDateString());


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
                cell.SetCellValue("Date");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(1);
                cell.SetCellValue("Amount");
                cell.CellStyle = detailCellStyle;
                cell = headerRow.CreateCell(3);
                sheet.CreateFreezePane(0, 1, 0, 1);



                //Populate the sheet with values from the grid data
                decimal? total = 0;

                foreach (var product in result)
                {
                    //Create a new row
                    var row = sheet.CreateRow(rowNumber++);
                    total = total + product.Amount;
                    //Set values for the cells
                    row.CreateCell(0).SetCellValue(String.Format(Session["DateFormatUpper"].ToString(), product.Date));
                    row.CreateCell(1).SetCellValue(Convert.ToInt64(product.Amount));

                }
                var rowTotal = sheet.CreateRow(rowNumber++);

                var expense = rowTotal.CreateCell(0);
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

                var jan = rowTotal.CreateCell(1);
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
                    "SalesByDate.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user



            }



        }



        [HttpGet]
        public ActionResult CusDCbyDatePDF(int id, string value, DateTime? From, DateTime? To)
        {

            //int id = 1; string value = "0"; string from = "0"; string to = "0";




            // string date = "";



            if (value == "1")
            {





                ViewBag.from = From;
                ViewBag.to = To;


                //Get the data representing the current grid state - page, sort and filter


                var result = db.SalesInvoices.Where(s => s.CompanyId == id && s.BranchId == 0 && s.Date >= From && s.Date <= To).ToList();
                return View(result);

            }

            else
            {
                var result = db.SalesInvoices.Where(s => s.CompanyId == id && s.BranchId == 0).ToList();
                return View(result);
            }



        }


        [HttpGet]
        public ActionResult CusDCbyDatePrint()
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
        public ActionResult CusDCbyDatePDFlink()
        {
            int compid = Convert.ToInt32(Session["companyid"]);

            string val = Session["datetime"].ToString();
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? from = null;
            DateTime? to = null;
            try
            {

                from = DateTime.Parse(Convert.ToString(Session["fdate"]));
                to = DateTime.Parse(Convert.ToString(Session["tdate"]));

                return new ActionAsPdf("SalesbyDatePDF", new { id = compid, value = "1", From = from, To = to }) { FileName = "SalesByDate.pdf" };
            }
            catch
            {
                return new ActionAsPdf("SalesbyDatePDF", new { id = compid, value = "0", From = from, To = to }) { FileName = "SalesByDate.pdf" };
            }

        }




        [HttpPost]
        public ActionResult CusDCDateMail(string Mailto, string message, string subj, HttpPostedFileBase fileUploader)
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


                return RedirectToAction("SalesbyDate", new { Err = "Please try again...." });
            }


        }



        #endregion


        #region ---- Despatch Register--------------

        [HttpGet]
        public ActionResult Despatch(string Msg, string Err)
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
        public ActionResult DespatchReport(string From, string To)
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


               // var result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.Date >= FromDate && s.Date <= ToDate).ToList();
                var despatch = db.SalesDeliveryDetails.Where(s => s.SalesDelivery.CompanyId == companyid && s.SalesDelivery.BranchId == Branchid && s.SalesDelivery.Date >= FromDate && s.SalesDelivery.Date <= ToDate)
                               .GroupBy(s=>new { s.Product.Code,s.Product.ShortName }).Select(s => new SalesInvoiceDetail { BarCode = s.Key.Code, Description = s.Key.ShortName, Quantity = s.Sum(d => d.Quantity), UnitFormula = s.Sum(d => d.Quantity * d.UnitFormula) }).ToList();

                return PartialView(despatch);
            }
            else
            {
                Session["datetime"] = 0;

                // var result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid).ToList();
                var despatch = db.SalesDeliveryDetails.Where(s => s.SalesDelivery.CompanyId == companyid && s.SalesDelivery.BranchId == Branchid && s.SalesDelivery.Date >= FromDate && s.SalesDelivery.Date <= ToDate)
                               .GroupBy(s => new { s.Product.Code, s.Product.ShortName }).Select(s => new SalesInvoiceDetail {BarCode = s.Key.Code, Description = s.Key.ShortName, Quantity = s.Sum(d => d.Quantity), UnitFormula = s.Sum(d => d.Quantity * d.UnitFormula) }).ToList();

                return PartialView(despatch);
                
            }


        }

        public FileResult DespatchExport()
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


                var result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.Date >= fromDate && s.Date <= toDate).ToList();


                //Create new Excel workbook
                var workbook = new HSSFWorkbook();

                //Create new Excel sheet
                var sheet = workbook.CreateSheet();

                //(Optional) set the width of the columns
                sheet.SetColumnWidth(0, 50 * 56);
                sheet.SetColumnWidth(1, 50 * 56);
                sheet.SetColumnWidth(2, 50 * 56);
                sheet.SetColumnWidth(3, 50 * 156);
                sheet.SetColumnWidth(4, 50 * 56);
                sheet.SetColumnWidth(5, 50 * 86);
                sheet.SetColumnWidth(6, 50 * 86);
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
                cell.SetCellValue("Sales By Date");
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
                cell.SetCellValue("Sub Total");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(5);
                cell.SetCellValue("Discount");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(6);
                cell.SetCellValue("Net Total");
                cell.CellStyle = detailCellStyle;


                sheet.CreateFreezePane(0, 1, 0, 1);



                //Populate the sheet with values from the grid data
                decimal? subtotal = 0;
                decimal? discounttotal = 0;
                decimal? total = 0;
                int serial = 0;
                foreach (var product in result)
                {
                    var sub = (decimal)(product.BCGrandTotal + product.Dis);
                    subtotal += sub;
                    var discount = product.Dis;
                    discounttotal += discount;
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
                    row.CreateCell(4).SetCellValue(Math.Round((double)sub, 2));
                    row.CreateCell(5).SetCellValue(Math.Round((double)discount, 2));
                    row.CreateCell(6).SetCellValue(Math.Round((double)gtotal, 2));

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

                var jan = rowTotal.CreateCell(6);
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
                    "SalesByDate.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user




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
                sheet.SetColumnWidth(3, 50 * 156);
                sheet.SetColumnWidth(4, 50 * 56);
                sheet.SetColumnWidth(5, 50 * 86);
                sheet.SetColumnWidth(6, 50 * 86);
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
                cell.SetCellValue("Sales Till Date");
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
                cell.SetCellValue("Sub Total");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(5);
                cell.SetCellValue("Discount");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(6);
                cell.SetCellValue("Net Total");
                cell.CellStyle = detailCellStyle;


                sheet.CreateFreezePane(0, 1, 0, 1);



                //Populate the sheet with values from the grid data
                decimal? subtotal = 0;
                decimal? discounttotal = 0;
                decimal? total = 0;
                int serial = 0;
                foreach (var product in result)
                {
                    var sub = (decimal)(product.BCGrandTotal + product.Dis);
                    subtotal += sub;
                    var discount = product.Dis;
                    discounttotal += discount;
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
                    row.CreateCell(4).SetCellValue(Math.Round((double)sub, 2));
                    row.CreateCell(5).SetCellValue(Math.Round((double)discount, 2));
                    row.CreateCell(6).SetCellValue(Math.Round((double)gtotal, 2));

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

                var jan = rowTotal.CreateCell(6);
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
                    "SalesByDate.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user

            }



        }

        [HttpGet]
        public ActionResult DespatchPDF(int id, string value, DateTime? From, DateTime? To)
        {

            //int id = 1; string value = "0"; string from = "0"; string to = "0";




            // string date = "";



            if (value == "1")
            {





                ViewBag.from = From;
                ViewBag.to = To;


                //Get the data representing the current grid state - page, sort and filter


                var result = db.SalesInvoices.Where(s => s.CompanyId == id && s.BranchId == 0 && s.Date >= From && s.Date <= To).ToList();
                return View(result);

            }

            else
            {
                var result = db.SalesInvoices.Where(s => s.CompanyId == id && s.BranchId == 0).ToList();
                return View(result);
            }



        }

        [HttpGet]
        public ActionResult DespatchPrint()
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
        public ActionResult DespatchPDFlink()
        {
            int compid = Convert.ToInt32(Session["companyid"]);

            string val = Session["datetime"].ToString();
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? from = null;
            DateTime? to = null;
            try
            {

                from = DateTime.Parse(Convert.ToString(Session["fdate"]));
                to = DateTime.Parse(Convert.ToString(Session["tdate"]));

                return new ActionAsPdf("SalesbyDatePDF", new { id = compid, value = "1", From = from, To = to }) { FileName = "SalesByDate.pdf" };
            }
            catch
            {
                return new ActionAsPdf("SalesbyDatePDF", new { id = compid, value = "0", From = from, To = to }) { FileName = "SalesByDate.pdf" };
            }

        }

        [HttpPost]
        public ActionResult DespatchMail(string Mailto, string message, string subj, HttpPostedFileBase fileUploader)
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


                return RedirectToAction("SalesbyDate", new { Err = "Please try again...." });
            }


        }

        #endregion
        #region ---- Sales By Customer Stock--------------


        public ActionResult SalesCustomerStock()
        {
            return View();


        }

        [HttpGet]
        public ActionResult SalesCustomerStockReport(string from, string to, int id)
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


            Session["idc"] = id;

            var detls = db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= FromDate && d.SalesInvoice.InvoiceDate <= ToDate && d.SalesInvoice.CustomerId == id).ToList();

            return PartialView(detls);


        }

        [HttpGet]
        public ActionResult SalesCustomerStockPDFlink()
        {
            int compid = Convert.ToInt32(Session["companyid"]);
            int? id = Convert.ToInt32(Session["idc"]);
          //  string val = Session["datetime"].ToString();
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            string fd = Session["fdate"].ToString();
            string td = Session["tdate"].ToString();
            DateTime? from = null;
            DateTime? to = null;
            try
            {
                
                from = DateTime.ParseExact(fd, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                to = DateTime.ParseExact(td, dateFormat, CultureInfo.CreateSpecificCulture(culture));

                return new ActionAsPdf("SalesCustomerStockPDF", new { id = id, companyid = compid, From = from, To = to }) { FileName = "SalesCustomerStockPDF.pdf" };
            }
            catch
            {
                return new ActionAsPdf("SalesCustomerStockPDF", new { id = id, companyid = compid, From = from, To = to }) { FileName = "SalesCustomerStockPDF.pdf" };
            }

        }
        [HttpGet]
        public ActionResult SalesCustomerStockPDF(int? id, int? companyid, DateTime? From, DateTime? To)
        {
            ViewBag.from = From;
            ViewBag.to = To;
            ViewBag.Customer = db.Customers.Where(d => d.Id == id && d.CompanyId == companyid).Select(d => d.Name).FirstOrDefault();
            var detls = db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= From && d.SalesInvoice.InvoiceDate <= To && d.SalesInvoice.CustomerId == id).ToList();

            return View(detls);
        }
        #endregion

        #region ---- Sales By  Stock Date--------------

        [HttpGet]
        public ActionResult SalesbyStockDate(string Msg, string Err)
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
            ViewBag.Customer = db.Customers.Where(d => d.CompanyId == companyid).ToList();
            return View();


        }


        [HttpGet]
        public ActionResult SalesbyStockDateReport(string From, string To,int? id)
        {
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? FromDate = null;
            DateTime? ToDate = null;
            if (From != "")
            {
                FromDate = DateTime.ParseExact(From, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            if (To != "")
            {
                ToDate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            Session["fdate"] = FromDate;
            Session["tdate"] = ToDate;
            Session["idc"] = id;
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            if (From != "")
            {
                Session["datetime"] = 1;
                if (Branchid == 0)
                {
                    if (id == null)
                    {
                        var salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate)
                             .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, TotalDeductAmount = s.SalesInvoice.TotalAddAmount, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula, TaxOther = s.UnitFormula, TaxProduct = (s.UnitIdSecondary == 1 ? (s.Quantity) : (s.Quantity * s.UnitFormula)), TotalAddAmount= s.TotalAmount, DisAmount = (s.Discount * s.Price * s.Quantity)/100, RoundOff = s.Price * s.Quantity }).OrderBy(d=>d.CreatedOn).ToList();
                        var res = (salesdetails.GroupBy(s => new {s.TaxOther, s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal,s.TotalDeductAmount })
                             .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName,TotalDeductAmount=s.Key.TotalDeductAmount, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal),TaxOther=s.Key.TaxOther,TaxProduct=s.Sum(d=>d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount), DisAmount=s.Sum(d=>d.DisAmount), RoundOff=s.Sum(d=>d.RoundOff) })).ToList();
                        return PartialView(res);
                    }
                    else
                    {
                        var salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.CustomerId == id)
                            .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, TotalDeductAmount = s.SalesInvoice.TotalAddAmount, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula, TaxOther = s.UnitFormula, TaxProduct = (s.UnitIdSecondary == 1 ? (s.Quantity) : (s.Quantity * s.UnitFormula)), TotalAddAmount = s.TotalAmount, DisAmount = (s.Discount * s.Price * s.Quantity) / 100, RoundOff = s.Price * s.Quantity }).OrderBy(d => d.CreatedOn).ToList();
                        var res = (salesdetails.GroupBy(s => new { s.TaxOther,s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal,s.TotalDeductAmount })
                             .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName,TotalDeductAmount=s.Key.TotalDeductAmount, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal), TaxOther = s.Key.TaxOther, TaxProduct = s.Sum(d => d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount), DisAmount = s.Sum(d => d.DisAmount), RoundOff = s.Sum(d => d.RoundOff) })).ToList();
                        return PartialView(res);
                    }
                }
                else
                {
                    if (id == null)
                    {
                        var salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate)
                             .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, TotalDeductAmount = s.SalesInvoice.TotalAddAmount, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula , TaxOther = s.UnitFormula, TaxProduct = (s.UnitIdSecondary == 1 ? (s.Quantity) : (s.Quantity * s.UnitFormula)), TotalAddAmount = s.TotalAmount, DisAmount = (s.Discount * s.Price * s.Quantity) / 100, RoundOff = s.Price * s.Quantity }).OrderBy(d => d.CreatedOn).ToList();
                        var res = (salesdetails.GroupBy(s => new { s.TaxOther, s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal,s.TotalDeductAmount })
                             .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName,TotalDeductAmount=s.Key.TotalDeductAmount, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal), TaxOther = s.Key.TaxOther, TaxProduct = s.Sum(d => d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount), DisAmount = s.Sum(d => d.DisAmount), RoundOff = s.Sum(d => d.RoundOff) })).ToList();
                        return PartialView(res);
                    }
                    else
                    {
                        var salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.CustomerId == id)
                            .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, TotalDeductAmount = s.SalesInvoice.TotalAddAmount, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula, TaxOther = s.UnitFormula, TaxProduct = (s.UnitIdSecondary == 1 ? (s.Quantity) : (s.Quantity * s.UnitFormula)), TotalAddAmount = s.TotalAmount, DisAmount= (s.Discount * s.Price * s.Quantity) / 100, RoundOff=s.Price*s.Quantity }).OrderBy(d => d.CreatedOn).ToList();
                        var res = (salesdetails.GroupBy(s => new { s.TaxOther, s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal,s.TotalDeductAmount })
                             .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName,TotalDeductAmount=s.Key.TotalDeductAmount, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Sum(d => d.GrandTotal), TaxOther = s.Key.TaxOther, TaxProduct = s.Sum(d => d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount), DisAmount = s.Sum(d => d.DisAmount), RoundOff = s.Sum(d => d.RoundOff) })).ToList();
                        return PartialView(res);
                    }

                }
            }
            else
            {
                Session["datetime"] = 0;

                if (id == null)
                {
                    var salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid )
                    .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula }).ToList();
                    var res = (salesdetails.GroupBy(s => new { s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal })
                         .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal) })).ToList();
                    return PartialView(res);
                }
                else
                {
                    var salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid  && s.SalesInvoice.CustomerId==id)
                   .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula, TaxOther = s.UnitFormula, TaxProduct = (s.UnitIdSecondary == 1 ? (s.Quantity) : (s.Quantity * s.UnitFormula)) }).ToList();
                    var res = (salesdetails.GroupBy(s => new { s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal })
                         .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal) })).ToList();
                    return PartialView(res);
                }
            }


        }

        public ActionResult SalesDateStockExport(string From, string To, int? id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                DateTime? FromDate = null;
                DateTime? ToDate = null;
                if (From != "")
                {
                    FromDate = DateTime.ParseExact(From, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                }
                if (To != "")
                {
                    ToDate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                }
                Session["fdate"] = FromDate;
                Session["tdate"] = ToDate;
                Session["idc"] = id;
                long companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                List<SalesInvoiceModelView> salesdetails = new List<SalesInvoiceModelView>();
                if (From != "")
                {
                    Session["datetime"] = 1;
                    if (Branchid == 0)
                    {
                        if (id == null)
                        {
                             salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.Date >= FromDate && s.SalesInvoice.Date <= ToDate)
                                 .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.SecUnitFormula ?? 0, TaxOther = s.UnitFormula, TaxProduct = s.UnitFormula * s.Quantity, TotalAddAmount = s.TotalAmount }).OrderBy(d => d.CreatedOn).ToList();
                            var res = (salesdetails.GroupBy(s => new { s.TaxOther, s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal })
                                 .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal), TaxOther = s.Key.TaxOther, TaxProduct = s.Sum(d => d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount) })).ToList();
                            
                        }
                        else
                        {
                             salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.Date >= FromDate && s.SalesInvoice.Date <= ToDate && s.SalesInvoice.CustomerId == id)
                                .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.SecUnitFormula ?? 0, TaxOther = s.UnitFormula, TaxProduct = s.UnitFormula * s.Quantity, TotalAddAmount = s.TotalAmount }).OrderBy(d => d.CreatedOn).ToList();
                            var res = (salesdetails.GroupBy(s => new { s.TaxOther, s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal })
                                 .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal), TaxOther = s.Key.TaxOther, TaxProduct = s.Sum(d => d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount) })).ToList();
                          
                        }
                    }
                    else
                    {
                        if (id == null)
                        {
                             salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.Date >= FromDate && s.SalesInvoice.Date <= ToDate)
                                 .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.SecUnitFormula ?? 0, TaxOther = s.UnitFormula, TaxProduct = s.UnitFormula * s.Quantity, TotalAddAmount = s.TotalAmount }).OrderBy(d => d.CreatedOn).ToList();
                            var res = (salesdetails.GroupBy(s => new { s.TaxOther, s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal })
                                 .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal), TaxOther = s.Key.TaxOther, TaxProduct = s.Sum(d => d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount) })).ToList();
                           
                        }
                        else
                        {
                             salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.Date >= FromDate && s.SalesInvoice.Date <= ToDate && s.SalesInvoice.CustomerId == id)
                                .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.SecUnitFormula ?? 0, TaxOther = s.UnitFormula, TaxProduct = s.UnitFormula * s.Quantity, TotalAddAmount = s.TotalAmount }).OrderBy(d => d.CreatedOn).ToList();
                            var res = (salesdetails.GroupBy(s => new { s.TaxOther, s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal })
                                 .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal), TaxOther = s.Key.TaxOther, TaxProduct = s.Sum(d => d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount) })).ToList();
                           
                        }

                    }
                }
                else
                {
                    Session["datetime"] = 0;

                    if (id == null)
                    {
                         salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid)
                        .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.SecUnitFormula ?? 0 }).ToList();
                        var res = (salesdetails.GroupBy(s => new { s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal })
                             .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal) })).ToList();
                      
                    }
                    else
                    {
                         salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.CustomerId == id)
                       .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.SecUnitFormula ?? 0 }).ToList();
                        var res = (salesdetails.GroupBy(s => new { s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal })
                             .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal) })).ToList();
                      
                    }
                }

                ViewBag.from = From;
                ViewBag.to = To;
                Response.AddHeader("content-disposition", "attachment; filename=CustomerStock.xls");
                Response.ContentType = "application/ms-excel";
                return PartialView("SalesbyDateStockPDF", salesdetails);
            }
        }


        
        [HttpGet]
        public ActionResult SalesbyStockCustomerDate(string Msg, string Err)
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
            ViewBag.Customer = db.Customers.Where(d => d.CompanyId == companyid).ToList();
            return View();


        }


        [HttpGet]
        public ActionResult SalesbyStockCustomerDateReport(string From, string To,int? id)
        {
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? FromDate = null;
            DateTime? ToDate = null;
            if (From != "")
            {
                FromDate = DateTime.ParseExact(From, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            if (To != "")
            {
                ToDate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }
            Session["fdate"] = FromDate;
            Session["tdate"] = ToDate;
            Session["idc"] = id;
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            if (From != "")
            {
                Session["datetime"] = 1;
                if (Branchid == 0)
                {
                    if (id == null)
                    {
                        var salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate)
                             .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, TotalDeductAmount = s.SalesInvoice.TotalAddAmount, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula, TaxOther = s.UnitFormula, TaxProduct = (s.UnitIdSecondary == 1 ? (s.Quantity) : (s.Quantity * s.UnitFormula)), TotalAddAmount = s.TotalAmount, DisAmount = (s.Discount * s.Price * s.Quantity) / 100, RoundOff = s.Price * s.Quantity }).OrderBy(d => d.CreatedOn).ToList();
                        var res = (salesdetails.GroupBy(s => new { s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal, s.TotalDeductAmount })
                             .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, TotalDeductAmount = s.Key.TotalDeductAmount, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal), TaxProduct = s.Sum(d => d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount), DisAmount = s.Sum(d => d.DisAmount), RoundOff = s.Sum(d => d.RoundOff) })).ToList();
                        res= (res.GroupBy(s => new {s.DeliveryName })
                             .Select(s => new SalesInvoiceModelView {  DeliveryName = s.Key.DeliveryName, TotalDeductAmount = s.Sum(d=>d.TotalDeductAmount), BCGrandTotal = s.Sum(d => d.BCGrandTotal), SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Sum(d => d.GrandTotal),  TaxProduct = s.Sum(d => d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount), DisAmount = s.Sum(d => d.DisAmount), RoundOff = s.Sum(d => d.RoundOff) })).ToList();
                        return PartialView(res);
                    }
                    else
                    {
                        var salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.CustomerId == id)
                            .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, TotalDeductAmount = s.SalesInvoice.TotalAddAmount, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula, TaxOther = s.UnitFormula, TaxProduct = (s.UnitIdSecondary == 1 ? (s.Quantity) : (s.Quantity * s.UnitFormula)), TotalAddAmount = s.TotalAmount, DisAmount = (s.Discount * s.Price * s.Quantity) / 100, RoundOff = s.Price * s.Quantity }).OrderBy(d => d.CreatedOn).ToList();
                        var res = (salesdetails.GroupBy(s => new {s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal, s.TotalDeductAmount })
                             .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, TotalDeductAmount = s.Key.TotalDeductAmount, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal), TaxProduct = s.Sum(d => d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount), DisAmount = s.Sum(d => d.DisAmount), RoundOff = s.Sum(d => d.RoundOff) })).ToList();
                        res = (res.GroupBy(s => new { s.DeliveryName })
                            .Select(s => new SalesInvoiceModelView { DeliveryName = s.Key.DeliveryName, TotalDeductAmount = s.Sum(d => d.TotalDeductAmount), BCGrandTotal = s.Sum(d => d.BCGrandTotal), SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Sum(d => d.GrandTotal), TaxProduct = s.Sum(d => d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount), DisAmount = s.Sum(d => d.DisAmount), RoundOff = s.Sum(d => d.RoundOff) })).ToList();
                        return PartialView(res);
                    }
                }
                else
                {
                    if (id == null)
                    {
                        var salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate)
                             .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, TotalDeductAmount = s.SalesInvoice.TotalAddAmount, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula, TaxOther = s.UnitFormula, TaxProduct = (s.UnitIdSecondary == 1 ? (s.Quantity) : (s.Quantity * s.UnitFormula)), TotalAddAmount = s.TotalAmount, DisAmount = (s.Discount * s.Price * s.Quantity) / 100, RoundOff = s.Price * s.Quantity }).OrderBy(d => d.CreatedOn).ToList();
                        var res = (salesdetails.GroupBy(s => new {  s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal, s.TotalDeductAmount })
                             .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, TotalDeductAmount = s.Key.TotalDeductAmount, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal), TaxProduct = s.Sum(d => d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount), DisAmount = s.Sum(d => d.DisAmount), RoundOff = s.Sum(d => d.RoundOff) })).ToList();
                        res = (res.GroupBy(s => new { s.DeliveryName })
                            .Select(s => new SalesInvoiceModelView { DeliveryName = s.Key.DeliveryName, TotalDeductAmount = s.Sum(d => d.TotalDeductAmount), BCGrandTotal = s.Sum(d => d.BCGrandTotal), SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Sum(d => d.GrandTotal), TaxProduct = s.Sum(d => d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount), DisAmount = s.Sum(d => d.DisAmount), RoundOff = s.Sum(d => d.RoundOff) })).ToList();
                        return PartialView(res);
                    }
                    else
                    {
                        var salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.CustomerId == id)
                            .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, TotalDeductAmount = s.SalesInvoice.TotalAddAmount, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula, TaxOther = s.UnitFormula, TaxProduct = (s.UnitIdSecondary == 1 ? (s.Quantity) : (s.Quantity * s.UnitFormula)), TotalAddAmount = s.TotalAmount, DisAmount = (s.Discount * s.Price * s.Quantity) / 100, RoundOff = s.Price * s.Quantity }).OrderBy(d => d.CreatedOn).ToList();
                        var res = (salesdetails.GroupBy(s => new {s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal, s.TotalDeductAmount })
                             .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, TotalDeductAmount = s.Key.TotalDeductAmount, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Sum(d => d.GrandTotal), TaxProduct = s.Sum(d => d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount), DisAmount = s.Sum(d => d.DisAmount), RoundOff = s.Sum(d => d.RoundOff) })).ToList();
                        res = (res.GroupBy(s => new { s.DeliveryName })
                            .Select(s => new SalesInvoiceModelView { DeliveryName = s.Key.DeliveryName, TotalDeductAmount = s.Sum(d => d.TotalDeductAmount), BCGrandTotal = s.Sum(d => d.BCGrandTotal), SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Sum(d => d.GrandTotal), TaxProduct = s.Sum(d => d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount), DisAmount = s.Sum(d => d.DisAmount), RoundOff = s.Sum(d => d.RoundOff) })).ToList();
                        return PartialView(res);
                    }

                }
            }
            else
            {
                Session["datetime"] = 0;

                if (id == null)
                {
                    var salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid)
                    .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula }).ToList();
                    var res = (salesdetails.GroupBy(s => new { s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal })
                         .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal) })).ToList();
                    return PartialView(res);
                }
                else
                {
                    var salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.CustomerId == id)
                   .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula, TaxOther = s.UnitFormula, TaxProduct = (s.UnitIdSecondary == 1 ? (s.Quantity) : (s.Quantity * s.UnitFormula)) }).ToList();
                    var res = (salesdetails.GroupBy(s => new { s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal })
                         .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal) })).ToList();
                    return PartialView(res);
                }
            }


        }

        //public FileResult SalesDateStockExport1()
        //{
        //    long companyid = Convert.ToInt32(Session["companyid"]);
        //    long Branchid = Convert.ToInt64(Session["BranchId"]);
        //    var culture = Session["DateCulture"].ToString();
        //    string dateFormat = Session["DateFormat"].ToString();

        //    string date = "";

        //    date = Session["datetime"].ToString();
        //    if (date == "1")
        //    {


        //        DateTime? fromDate = (DateTime?)Session["fdate"];
        //        DateTime? toDate = (DateTime?)Session["tdate"];

        //        //Get the data representing the current grid state - page, sort and filter


        //        var result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.Date >= fromDate && s.Date <= toDate).ToList();


        //        //Create new Excel workbook
        //        var workbook = new HSSFWorkbook();

        //        //Create new Excel sheet
        //        var sheet = workbook.CreateSheet();

        //        //(Optional) set the width of the columns
        //        sheet.SetColumnWidth(0, 50 * 56);
        //        sheet.SetColumnWidth(1, 50 * 56);
        //        sheet.SetColumnWidth(2, 50 * 56);
        //        sheet.SetColumnWidth(3, 50 * 156);
        //        sheet.SetColumnWidth(4, 50 * 56);
        //        sheet.SetColumnWidth(5, 50 * 86);
        //        sheet.SetColumnWidth(6, 50 * 86);
        //        sheet.SetColumnWidth(7, 50 * 86);
        //        //Create a header row



        //        // Create a font object and make it bold
        //        var HeaderCellStyle = workbook.CreateCellStyle();
        //        // HeaderCellStyle.BorderBottom = BorderStyle.THIN;
        //        HeaderCellStyle.Alignment = HorizontalAlignment.CENTER;
        //        var HeaderFont = workbook.CreateFont();
        //        HeaderFont.Boldweight = (short)FontBoldWeight.BOLD;
        //        HeaderCellStyle.SetFont(HeaderFont);



        //        //Create a header row
        //        var headerRow = sheet.CreateRow(9);
        //        // getting company details
        //        var companyDetail = db.Companies.Where(c => c.Id == companyid).FirstOrDefault();
        //        //Set the column names in the header row          
        //        int rowNumber = 1;


        //        //Set the column names in the header row

        //        var row11 = sheet.CreateRow(rowNumber++);
        //        var cell = row11.CreateCell(3);
        //        cell.SetCellValue(companyDetail.Name);

        //        cell.CellStyle = HeaderCellStyle;
        //        row11 = sheet.CreateRow(rowNumber++);
        //        cell = row11.CreateCell(3);
        //        cell.SetCellValue(companyDetail.Address);
        //        cell.CellStyle = HeaderCellStyle;

        //        row11 = sheet.CreateRow(rowNumber++);
        //        cell = row11.CreateCell(3);
        //        cell.SetCellValue(companyDetail.Zipcode);
        //        cell.CellStyle = HeaderCellStyle;

        //        row11 = sheet.CreateRow(rowNumber++);
        //        row11 = sheet.CreateRow(rowNumber++);

        //        cell = row11.CreateCell(3);
        //        cell.SetCellValue("Customer Sales By Stock");
        //        cell.CellStyle = HeaderCellStyle;

        //        row11 = sheet.CreateRow(rowNumber++);
        //        row11 = sheet.CreateRow(rowNumber++);
        //        cell = row11.CreateCell(3);
        //        cell.SetCellValue("For " + String.Format(Session["DateFormatUpper"].ToString(), fromDate.Value.ToShortDateString()) + " To " + String.Format(Session["DateFormatUpper"].ToString(), toDate.Value.ToShortDateString()));


        //        row11 = sheet.CreateRow(rowNumber++);
        //        row11 = sheet.CreateRow(rowNumber++);

        //        // Create a font object and make it bold
        //        var detailCellStyle = workbook.CreateCellStyle();
        //        //detailCellStyle.BorderBottom = BorderStyle.Double;
        //        //detailCellStyle.BorderTop = BorderStyle.THIN;
        //        var detailFont = workbook.CreateFont();
        //        detailFont.Boldweight = (short)FontBoldWeight.BOLD;
        //        detailCellStyle.SetFont(detailFont);



        //        cell = headerRow.CreateCell(0);
        //        cell.SetCellValue("SL. No.");
        //        cell.CellStyle = detailCellStyle;

        //        cell = headerRow.CreateCell(1);
        //        cell.SetCellValue("Date");
        //        cell.CellStyle = detailCellStyle;

        //        cell = headerRow.CreateCell(2);
        //        cell.SetCellValue("Bill No.");
        //        cell.CellStyle = detailCellStyle;

        //        cell = headerRow.CreateCell(3);
        //        cell.SetCellValue("Party Name.");
        //        cell.CellStyle = detailCellStyle;

        //        cell = headerRow.CreateCell(4);
        //        cell.SetCellValue("Bags");
        //        cell.CellStyle = detailCellStyle;

        //        cell = headerRow.CreateCell(5);
        //        cell.SetCellValue("Kg");
        //        cell.CellStyle = detailCellStyle;

        //        cell = headerRow.CreateCell(6);
        //        cell.SetCellValue("Total Kgs");
        //        cell.CellStyle = detailCellStyle;

        //        cell = headerRow.CreateCell(6);
        //        cell.SetCellValue("Total Amount");
        //        cell.CellStyle = detailCellStyle;

        //        sheet.CreateFreezePane(0, 1, 0, 1);



        //        //Populate the sheet with values from the grid data
        //        var SlNo = 0;
        //        decimal totalKgs = 0;
        //        decimal Bags = 0;
        //        decimal netBags = 0;
        //        decimal netKgs = 0;
        //        decimal netAmount = 0;
        //        foreach (var item in result)
        //        {
        //            SlNo = SlNo + 1;

        //            totalKgs = Math.Round(item.TaxProduct, 3);
        //            Bags = Math.Round(item.SubTotal, 0);
        //            netBags += Bags;
        //            netKgs += totalKgs;

        //            netAmount += item.TotalAddAmount;

        //            //Create a new row
        //            var row = sheet.CreateRow(rowNumber++);
        //            //   total = total + product.Amount;
        //            //Set values for the cells
        //            row.CreateCell(0).SetCellValue(SlNo);
        //            row.CreateCell(1).SetCellValue(String.Format(Session["DateFormatUpper"].ToString(), item.CreatedOn));
        //            row.CreateCell(2).SetCellValue(item.NO);
        //            row.CreateCell(3).SetCellValue(item.Customer.Name);
        //            row.CreateCell(4).SetCellValue(Math.Round((double)Bags, 2));
        //            row.CreateCell(5).SetCellValue(Math.Round((double)discount, 2));
        //            row.CreateCell(6).SetCellValue(Math.Round((double)gtotal, 2));

        //        }
        //        var rowTotal = sheet.CreateRow(rowNumber++);

        //        var expense = rowTotal.CreateCell(1);
        //        expense.SetCellType(CellType.NUMERIC);
        //        expense.SetCellValue("Grand Total");
        //        expense.CellStyle = detailCellStyle;

        //        var currencyCellStyle = workbook.CreateCellStyle();
        //        // Right-align currency values
        //        currencyCellStyle.Alignment = HorizontalAlignment.RIGHT;

        //        var detailFonts = workbook.CreateFont();
        //        detailFonts.Boldweight = (short)FontBoldWeight.BOLD;
        //        currencyCellStyle.SetFont(detailFonts);



        //        // Get / create the data format string
        //        var formatId = HSSFDataFormat.GetBuiltinFormat("#,##0.00");
        //        if (formatId == -1)
        //        {
        //            var newDataFormat = workbook.CreateDataFormat();
        //            currencyCellStyle.DataFormat = newDataFormat.GetFormat("#,##0.00");
        //        }
        //        else
        //            currencyCellStyle.DataFormat = formatId;

        //        var jan = rowTotal.CreateCell(6);
        //        jan.SetCellType(CellType.FORMULA);
        //        jan.CellFormula = "SUM(" + total + ")";
        //        jan.CellStyle = currencyCellStyle;
        //        jan.SetCellValue(Convert.ToDouble(total));
        //        sheet.CreateFreezePane(0, 1, 0, 1);

        //        //Write the workbook to a memory stream
        //        MemoryStream output = new MemoryStream();
        //        workbook.Write(output);

        //        //Return the result to the end user

        //        return File(output.ToArray(),   //The binary data of the XLS file
        //            "application/vnd.ms-excel", //MIME type of Excel files
        //            "SalesByDate.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user




        //    }
        //    else
        //    {
        //        DateTime? fromDate = (DateTime?)Session["fdate"];
        //        DateTime? toDate = (DateTime?)Session["tdate"];

        //        //Get the data representing the current grid state - page, sort and filter


        //        var result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid).ToList();


        //        //Create new Excel workbook
        //        var workbook = new HSSFWorkbook();

        //        //Create new Excel sheet
        //        var sheet = workbook.CreateSheet();

        //        //(Optional) set the width of the columns
        //        sheet.SetColumnWidth(0, 50 * 56);
        //        sheet.SetColumnWidth(1, 50 * 56);
        //        sheet.SetColumnWidth(2, 50 * 56);
        //        sheet.SetColumnWidth(3, 50 * 156);
        //        sheet.SetColumnWidth(4, 50 * 56);
        //        sheet.SetColumnWidth(5, 50 * 86);
        //        sheet.SetColumnWidth(6, 50 * 86);
        //        //Create a header row



        //        // Create a font object and make it bold
        //        var HeaderCellStyle = workbook.CreateCellStyle();
        //        // HeaderCellStyle.BorderBottom = BorderStyle.THIN;
        //        HeaderCellStyle.Alignment = HorizontalAlignment.CENTER;
        //        var HeaderFont = workbook.CreateFont();
        //        HeaderFont.Boldweight = (short)FontBoldWeight.BOLD;
        //        HeaderCellStyle.SetFont(HeaderFont);



        //        //Create a header row
        //        var headerRow = sheet.CreateRow(9);
        //        // getting company details
        //        var companyDetail = db.Companies.Where(c => c.Id == companyid).FirstOrDefault();
        //        //Set the column names in the header row          
        //        int rowNumber = 1;


        //        //Set the column names in the header row

        //        var row11 = sheet.CreateRow(rowNumber++);
        //        var cell = row11.CreateCell(3);
        //        cell.SetCellValue(companyDetail.Name);

        //        cell.CellStyle = HeaderCellStyle;
        //        row11 = sheet.CreateRow(rowNumber++);
        //        cell = row11.CreateCell(3);
        //        cell.SetCellValue(companyDetail.Address);
        //        cell.CellStyle = HeaderCellStyle;

        //        row11 = sheet.CreateRow(rowNumber++);
        //        cell = row11.CreateCell(3);
        //        cell.SetCellValue(companyDetail.Zipcode);
        //        cell.CellStyle = HeaderCellStyle;

        //        row11 = sheet.CreateRow(rowNumber++);
        //        row11 = sheet.CreateRow(rowNumber++);

        //        cell = row11.CreateCell(3);
        //        cell.SetCellValue("Sales Till Date");
        //        cell.CellStyle = HeaderCellStyle;

        //        row11 = sheet.CreateRow(rowNumber++);
        //        row11 = sheet.CreateRow(rowNumber++);
        //        cell = row11.CreateCell(3);
        //        //cell.SetCellValue("For " + String.Format(Session["DateFormatUpper"].ToString(), fromDate.Value.ToShortDateString()) + " To " + String.Format(Session["DateFormatUpper"].ToString(), toDate.Value.ToShortDateString()));


        //        row11 = sheet.CreateRow(rowNumber++);
        //        row11 = sheet.CreateRow(rowNumber++);

        //        // Create a font object and make it bold
        //        var detailCellStyle = workbook.CreateCellStyle();
        //        //detailCellStyle.BorderBottom = BorderStyle.Double;
        //        //detailCellStyle.BorderTop = BorderStyle.THIN;
        //        var detailFont = workbook.CreateFont();
        //        detailFont.Boldweight = (short)FontBoldWeight.BOLD;
        //        detailCellStyle.SetFont(detailFont);



        //        cell = headerRow.CreateCell(0);
        //        cell.SetCellValue("SL. No.");
        //        cell.CellStyle = detailCellStyle;

        //        cell = headerRow.CreateCell(1);
        //        cell.SetCellValue("Date");
        //        cell.CellStyle = detailCellStyle;

        //        cell = headerRow.CreateCell(2);
        //        cell.SetCellValue("Bill No.");
        //        cell.CellStyle = detailCellStyle;

        //        cell = headerRow.CreateCell(3);
        //        cell.SetCellValue("Party Name.");
        //        cell.CellStyle = detailCellStyle;

        //        cell = headerRow.CreateCell(4);
        //        cell.SetCellValue("Sub Total");
        //        cell.CellStyle = detailCellStyle;

        //        cell = headerRow.CreateCell(5);
        //        cell.SetCellValue("Discount");
        //        cell.CellStyle = detailCellStyle;

        //        cell = headerRow.CreateCell(6);
        //        cell.SetCellValue("Net Total");
        //        cell.CellStyle = detailCellStyle;


        //        sheet.CreateFreezePane(0, 1, 0, 1);



        //        //Populate the sheet with values from the grid data
        //        decimal? subtotal = 0;
        //        decimal? discounttotal = 0;
        //        decimal? total = 0;
        //        int serial = 0;
        //        foreach (var product in result)
        //        {
        //            var sub = (decimal)(product.BCGrandTotal + product.Dis);
        //            subtotal += sub;
        //            var discount = product.Dis;
        //            discounttotal += discount;
        //            var gtotal = (decimal)(product.BCGrandTotal);
        //            total += gtotal;
        //            serial++;

        //            //Create a new row
        //            var row = sheet.CreateRow(rowNumber++);
        //            //   total = total + product.Amount;
        //            //Set values for the cells
        //            row.CreateCell(0).SetCellValue(serial);
        //            row.CreateCell(1).SetCellValue(String.Format(Session["DateFormatUpper"].ToString(), product.Date));
        //            row.CreateCell(2).SetCellValue(product.NO);
        //            row.CreateCell(3).SetCellValue(product.Customer.Name);
        //            row.CreateCell(4).SetCellValue(Math.Round((double)sub, 2));
        //            row.CreateCell(5).SetCellValue(Math.Round((double)discount, 2));
        //            row.CreateCell(6).SetCellValue(Math.Round((double)gtotal, 2));

        //        }
        //        var rowTotal = sheet.CreateRow(rowNumber++);

        //        var expense = rowTotal.CreateCell(1);
        //        expense.SetCellType(CellType.NUMERIC);
        //        expense.SetCellValue("Grand Total");
        //        expense.CellStyle = detailCellStyle;

        //        var currencyCellStyle = workbook.CreateCellStyle();
        //        // Right-align currency values
        //        currencyCellStyle.Alignment = HorizontalAlignment.RIGHT;

        //        var detailFonts = workbook.CreateFont();
        //        detailFonts.Boldweight = (short)FontBoldWeight.BOLD;
        //        currencyCellStyle.SetFont(detailFonts);



        //        // Get / create the data format string
        //        var formatId = HSSFDataFormat.GetBuiltinFormat("#,##0.00");
        //        if (formatId == -1)
        //        {
        //            var newDataFormat = workbook.CreateDataFormat();
        //            currencyCellStyle.DataFormat = newDataFormat.GetFormat("#,##0.00");
        //        }
        //        else
        //            currencyCellStyle.DataFormat = formatId;

        //        var jan = rowTotal.CreateCell(6);
        //        jan.SetCellType(CellType.FORMULA);
        //        jan.CellFormula = "SUM(" + total + ")";
        //        jan.CellStyle = currencyCellStyle;
        //        jan.SetCellValue(Convert.ToDouble(total));
        //        sheet.CreateFreezePane(0, 1, 0, 1);

        //        //Write the workbook to a memory stream
        //        MemoryStream output = new MemoryStream();
        //        workbook.Write(output);

        //        //Return the result to the end user

        //        return File(output.ToArray(),   //The binary data of the XLS file
        //            "application/vnd.ms-excel", //MIME type of Excel files
        //            "SalesByDate.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user

        //    }



        //}


        [HttpGet]
        public ActionResult SalesbyDateStockPDF(int? id, int? companyid, int Branchid, DateTime? From, DateTime? To)
        {

            if (From != null)
            {
                ViewBag.from = From;
                ViewBag.to = To;

                Session["datetime"] = 1;
                if (Branchid == 0)
                {
                    if (id == null)
                    {
                        var salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.Date >= From && s.SalesInvoice.Date <= To)
                             .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.SecUnitFormula ?? 0, TaxOther = s.UnitFormula, TaxProduct = s.UnitFormula * s.Quantity, TotalAddAmount = s.TotalAmount }).OrderBy(d => d.CreatedOn).ToList();
                        var res = (salesdetails.GroupBy(s => new { s.TaxOther, s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal })
                             .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal), TaxOther = s.Key.TaxOther, TaxProduct = s.Sum(d => d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount) })).ToList();
                        return View(res);
                    }
                    else
                    {
                        var salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.Date >= From && s.SalesInvoice.Date <= To && s.SalesInvoice.CustomerId == id)
                            .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.SecUnitFormula ?? 0, TaxOther = s.UnitFormula, TaxProduct = s.UnitFormula * s.Quantity, TotalAddAmount = s.TotalAmount }).OrderBy(d => d.CreatedOn).ToList();
                        var res = (salesdetails.GroupBy(s => new { s.TaxOther, s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal })
                             .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal), TaxOther = s.Key.TaxOther, TaxProduct = s.Sum(d => d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount) })).ToList();
                        return View(res);
                    }
                }
                else
                {
                    if (id == null)
                    {
                        var salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.Date >= From && s.SalesInvoice.Date <= To)
                             .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.SecUnitFormula ?? 0, TaxOther = s.UnitFormula, TaxProduct = s.UnitFormula * s.Quantity, TotalAddAmount = s.TotalAmount }).OrderBy(d => d.CreatedOn).ToList();
                        var res = (salesdetails.GroupBy(s => new { s.TaxOther, s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal })
                             .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal), TaxOther = s.Key.TaxOther, TaxProduct = s.Sum(d => d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount) })).ToList();
                        return View(res);
                    }
                    else
                    {
                        var salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.Date >= From && s.SalesInvoice.Date <= To && s.SalesInvoice.CustomerId == id)
                            .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.SecUnitFormula ?? 0, TaxOther = s.UnitFormula, TaxProduct = s.UnitFormula * s.Quantity, TotalAddAmount = s.TotalAmount }).OrderBy(d => d.CreatedOn).ToList();
                        var res = (salesdetails.GroupBy(s => new { s.TaxOther, s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal })
                             .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal), TaxOther = s.Key.TaxOther, TaxProduct = s.Sum(d => d.TaxProduct), TotalAddAmount = s.Sum(d => d.TotalAddAmount) })).ToList();
                        return View(res);
                    }

                }
            }
            else
            {
                Session["datetime"] = 0;

                if (id == null)
                {
                    var salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid)
                    .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.SecUnitFormula ?? 0 }).ToList();
                    var res = (salesdetails.GroupBy(s => new { s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal })
                         .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal) })).ToList();
                    return View(res);
                }
                else
                {
                    var salesdetails = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.CustomerId == id)
                   .Select(s => new SalesInvoiceModelView { Id = s.SalesInvoice.Id, NO = s.SalesInvoice.NO, CreatedOn = s.SalesInvoice.InvoiceDate, DeliveryName = s.SalesInvoice.Customer.Name, BCGrandTotal = s.SalesInvoice.BCGrandTotal, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.SecUnitFormula ?? 0 }).ToList();
                    var res = (salesdetails.GroupBy(s => new { s.Id, s.NO, s.CreatedOn, s.DeliveryName, s.BCGrandTotal })
                         .Select(s => new SalesInvoiceModelView { Id = s.Key.Id, NO = s.Key.NO, CreatedOn = s.Key.CreatedOn, DeliveryName = s.Key.DeliveryName, BCGrandTotal = s.Key.BCGrandTotal, SubTotal = s.Sum(d => d.SubTotal), GrandTotal = s.Average(d => d.GrandTotal) })).ToList();
                    return View(res);
                }
            }
        }

        [HttpGet]
        public ActionResult SalesbyDateStockPrint()
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
        public ActionResult SalesbyDateStockPDFlink()
        {
            int compid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int? id= Convert.ToInt32(Session["idc"]);
            string val = Session["datetime"].ToString();
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            DateTime? from = null;
            DateTime? to = null;
            try
            {

                from = DateTime.Parse(Convert.ToString(Session["fdate"]));
                to = DateTime.Parse(Convert.ToString(Session["tdate"]));

                return new ActionAsPdf("SalesbyDateStockPDF", new { id = id, companyid = compid, Branchid = Branchid, From = from, To = to }) { FileName = "SalesbyDateStockPDF.pdf" };
            }
            catch
            {
                return new ActionAsPdf("SalesbyDateStockPDF", new { id = id, companyid = compid, Branchid = Branchid, From = from, To = to }) { FileName = "SalesbyDateStockPDF.pdf" };
            }

        }




        [HttpPost]
        public ActionResult SalesbyDateStockMail(string Mailto, string message, string subj, HttpPostedFileBase fileUploader)
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


                return RedirectToAction("SalesbyDate", new { Err = "Please try again...." });
            }


        }



        #endregion

        #region Despatch Register
        public ActionResult DespatchRegister()
        {
           return View();
        }
        public ActionResult DespatchRegisterReport(string From, string To)
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
            List<SalesDeliveryDetail> result = new List<SalesDeliveryDetail>();
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

                if(Branchid==0) 
                    result = db.SalesDeliveryDetails.Where(s => s.SalesDelivery.CompanyId == companyid && s.SalesDelivery.ReceiptDate >= FromDate && s.SalesDelivery.ReceiptDate <= ToDate).OrderBy(s=>s.SalesDelivery.ReceiptDate).ToList();
                else
                    result = db.SalesDeliveryDetails.Where(s => s.SalesDelivery.CompanyId == companyid && s.SalesDelivery.BranchId == Branchid && s.SalesDelivery.ReceiptDate >= FromDate && s.SalesDelivery.ReceiptDate <= ToDate).OrderBy(s => s.SalesDelivery.ReceiptDate).ToList();

                return PartialView(result);
            }
            else
            {
                Session["datetime"] = 0;

                if (Branchid == 0)
                    result = db.SalesDeliveryDetails.Where(s => s.SalesDelivery.CompanyId == companyid ).OrderBy(s => s.SalesDelivery.ReceiptDate).ToList();
                else
                    result = db.SalesDeliveryDetails.Where(s => s.SalesDelivery.CompanyId == companyid && s.SalesDelivery.BranchId == Branchid).OrderBy(s => s.SalesDelivery.ReceiptDate).ToList();

                return PartialView(result);
            }
           
        }
        [HttpGet]
        public ActionResult DespatchRegisterPDFlink(string from, string to)
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
                ViewBag.from = from;
                ViewBag.to = to;
                //using (InventoryEntities db = new InventoryEntities())
                //{
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
                //}
            }
            catch
            {
                return new ActionAsPdf("DespatchRegisterReport", new { id = Branchid, value = "0", From = From, To = To }) { FileName = "DespatchRegisterReport.pdf" };
            }


        }
        #endregion

        #region ---- Credit Note Month Wise--------------

        [HttpGet]
        public ActionResult CreditNoteMonthWise(string Msg, string Err)
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
        public ActionResult CreditNoteMonthWiseReport(string From, string To)
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

                var result = (from sc in db.SalesCostingDetails.Where(d=>d.CostingId==6).ToList()
                              join s in db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate).ToList()
                              on sc.SalesInvoiceId equals s.Id
                              select new SalesInvoice { NO = s.NO, InvoiceDate = s.InvoiceDate, DespatchNo = sc.Costing.Name, CustomerId = s.CustomerId, DespatchThrough = s.Customer.Name, GrandTotal = sc.CostAmount }).ToList();

                return PartialView(result);
            }
            else
            {
                Session["datetime"] = 0;

                var result = (from sc in db.SalesCostingDetails.Where(d => d.CostingId == 6).ToList()
                              join s in db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid).ToList()
                              on sc.SalesInvoiceId equals s.Id
                              select new SalesInvoice { NO = s.NO, InvoiceDate = s.InvoiceDate, DespatchNo = sc.Costing.Name, CustomerId = s.CustomerId, DespatchThrough = s.Customer.Name, GrandTotal = sc.CostAmount }).ToList();

                return PartialView(result);
            }


        }

        [HttpGet]
        public ActionResult CreditNoteCustomerWise(string Msg, string Err)
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
        public ActionResult CreditNoteCustomerWiseReport(string From, string To)
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

                var result = (from sc in db.SalesCostingDetails.Where(d => d.CostingId == 6).ToList()
                              join s in db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Type == null).ToList()
                              on sc.SalesInvoiceId equals s.Id
                              select new SalesInvoice { NO = s.NO, InvoiceDate = s.InvoiceDate, DespatchNo = sc.Costing.Name, CustomerId = s.CustomerId, DespatchThrough = s.Customer.Name, GrandTotal = sc.CostAmount }).ToList();
                result = result.GroupBy(s => new { CustomerId = s.CustomerId, CustomerName = s.DespatchThrough, CostName = s.DespatchNo }).Select(d => new SalesInvoice { DespatchThrough = d.Key.CustomerName, DespatchNo = d.Key.CostName, GrandTotal = d.Sum(p => p.GrandTotal) }).ToList();
                return PartialView(result);
            }
            else
            {
                Session["datetime"] = 0;

                var result = (from sc in db.SalesCostingDetails.Where(d => d.CostingId == 6).ToList()
                              join s in db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.Type == null).ToList()
                              on sc.SalesInvoiceId equals s.Id
                              select new SalesInvoice { NO = s.NO, InvoiceDate = s.InvoiceDate, DespatchNo = sc.Costing.Name, CustomerId = s.CustomerId, DespatchThrough = s.Customer.Name, GrandTotal = sc.CostAmount }).ToList();
                result = result.GroupBy(s => new { CustomerId = s.CustomerId, CustomerName = s.DespatchThrough, CostName = s.DespatchNo }).Select(d => new SalesInvoice { DespatchThrough = d.Key.CustomerName, DespatchNo = d.Key.CostName, GrandTotal = d.Sum(p => p.GrandTotal) }).ToList();
                return PartialView(result);
            }


        }

        [HttpGet]
        public ActionResult GenerateCreditNoteCustomerWise(string From, string To)
        {
            ViewBag.From = From;
            ViewBag.To = To;
            return View();
        }

        [HttpPost, ActionName("GenerateCreditNoteCustomerWise")]
        public ActionResult GenerateCreditNoteAlert(FormCollection fc)
        {
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            int Fyid = Convert.ToInt32(Session["fid"]);
            long Createdby = Convert.ToInt32(Session["Createdid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            DateTime? FromDate = null;
            DateTime? ToDate = null;
            string From = fc["From"].ToString();
            string To = fc["To"].ToString();
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
            var WarehouseId = db.Warehouses.Where(d => d.Branchid == Branchid).Select(d=>d.Id).FirstOrDefault();



            if (From != "0")
            {
                Session["datetime"] = 1;

                var result = (from sc in db.SalesCostingDetails.Where(d => d.CostingId == 6).ToList()
                              join s in db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Type == null).ToList()
                              on sc.SalesInvoiceId equals s.Id
                              join sd in db.SalesInvoiceDetails.GroupBy(d => d.SalesInvoiceId).Select(d => new { SalesInvoiceId = d.Key, Quantity = d.Sum(w => w.Quantity) }).ToList()
                              on s.Id equals sd.SalesInvoiceId
                              select new SalesInvoice { Id = s.Id, NO = s.NO, InvoiceDate = s.InvoiceDate, DespatchNo = sc.Costing.Name, CustomerId = s.CustomerId, DespatchThrough = s.Customer.Name,  SubTotal = sd.Quantity, GrandTotal = sc.CostAmount }).ToList();
                result = result.GroupBy(s => new { CustomerId = s.CustomerId, CustomerName = s.DespatchThrough, CostName = s.DespatchNo }).Select(d => new SalesInvoice { CustomerId = d.Key.CustomerId, DespatchThrough = d.Key.CustomerName, DespatchNo = d.Key.CostName, SubTotal = d.Sum(p => p.SubTotal), GrandTotal = d.Sum(p => p.GrandTotal) }).ToList();

                foreach(var pomv in result)
                {
                    int countpo = db.SalesReturns.Where(p => p.CompanyId == companyid && p.BranchId == Branchid).Count();
                    var NO = tc.GenerateCode("SR", countpo);

                    //Insert into SalesReturn table
                    SalesReturn po = new SalesReturn();
                    po.NO = NO;
                    po.CustomerId = pomv.CustomerId ?? 0;
                    po.Reference = null;
                    po.ReferenceNo = null;
                    po.WarehouseId = WarehouseId;
                    po.Date =(DateTime) ToDate;

                    po.DeliveryName = "";
                    po.StreetPoBox = "";
                    po.Suburb = "";
                    po.City = "";
                    po.StateRegion = "";
                    po.Country = "";
                    po.PostalCode = "";
                    po.CurrencyId = 84;
                    po.Currencyrate = 1;
                    po.TransactionCurrency = 84;

                    po.FinancialYearId = Fyid;
                    po.CreatedBy = Createdby;
                    po.CreatedOn = DateTime.Now;
                    po.UserId = userid;
                    po.BranchId = Branchid;
                    po.CompanyId = companyid;
                    po.Status = "Saved";
                    po.PaymentTermId = 5;
                    po.Memo = "System Generated Credit Note On FURTHER DISCOUNT OF RS. PER BAG ON " + pomv.SubTotal + " BAGS FOR THE PERIOD FROM " + From + " TO " +To + ".";
                    po.Type = "2";
                    po.TaxId = 1;
                    
                    po.TaxTotal = 0;
                    po.TotalAmount = pomv.GrandTotal;
                    po.GrandTotal = pomv.GrandTotal;
                    po.BCGrandTotal = Math.Round(pomv.GrandTotal);
                    db.SalesReturns.Add(po);
                }

                var sales = db.SalesCostingDetails.Where(d => d.CostingId == 6 && d.SalesInvoice.CompanyId == companyid && d.SalesInvoice.BranchId == Branchid && d.SalesInvoice.InvoiceDate >= FromDate && d.SalesInvoice.InvoiceDate <= ToDate).GroupBy(d => d.SalesInvoiceId).Select(d => d.Key).ToList();
                foreach(var sale in sales)
                {
                    var salesInvoice = db.SalesInvoices.Find(sale);
                    salesInvoice.Type = "Credit Note";
                }
                db.SaveChanges();
                return RedirectToAction("Index", "SalesReturn");
            }
            else
            {
                Session["datetime"] = 0;

                var result = (from sc in db.SalesCostingDetails.Where(d => d.CostingId == 6).ToList()
                              join s in db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid).ToList()
                              on sc.SalesInvoiceId equals s.Id
                              select new SalesInvoice { NO = s.NO, InvoiceDate = s.InvoiceDate, DespatchNo = sc.Costing.Name, CustomerId = s.CustomerId, DespatchThrough = s.Customer.Name, GrandTotal = sc.CostAmount }).ToList();
                result = result.GroupBy(s => new { CustomerId = s.CustomerId, CustomerName = s.DespatchThrough, CostName = s.DespatchNo }).Select(d => new SalesInvoice { DespatchThrough = d.Key.CustomerName, DespatchNo = d.Key.CostName, GrandTotal = d.Sum(p => p.GrandTotal) }).ToList();


                return PartialView(result);
            }


        }
        #endregion ---- Credit Note Month Wise--------------

        #region ---- Sales Analysis Register--------------

        [HttpGet]
        public ActionResult SalesRegister()
        {
            ViewBag.ProductCategory = db.ProductCategory_MSTR.ToList();
            return View();
        }
        public ActionResult SalesRegisterReport(string From, string To, int? CategoryId)
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
            List<SalesInvoiceModelView> result = new List<Models.SalesInvoiceModelView>();
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            if (Branchid == 0)
            {
                if (CategoryId == null)
                {
                    var salesdetails = from c in db.Customers.Select(d => new { LId = d.LId, PId = d.PId == null ? d.LId : d.PId })
                                       join s in db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= FromDate && d.SalesInvoice.InvoiceDate <= ToDate)
                                       on c.LId equals s.SalesInvoice.Customer.LId
                                       //  group s by new { s.SalesInvoice.Customer.Name } into cs
                                       select new { RecurringSalesId = c.PId, CustomerName = s.SalesInvoice.Customer.Name, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula, TaxOther = s.UnitFormula, TaxProduct = (s.UnitIdSecondary == 1 ? (s.Quantity) : (s.Quantity * s.UnitFormula)), TotalAddAmount = s.TotalAmount, DisAmount = (s.Discount * s.Price * s.Quantity) / 100, RoundOff = s.Price * s.Quantity, BCGrandTotal = s.Price };
                    result = (salesdetails.GroupBy(s => new { s.CustomerName, s.RecurringSalesId })
                   .Select(s => new SalesInvoiceModelView { RecurringSalesId = s.Key.RecurringSalesId, CustomerName = s.Key.CustomerName, SubTotal = s.Sum(d => d.SubTotal), TaxProduct = s.Sum(d => d.TaxProduct), RoundOff = s.Sum(d => d.RoundOff), BCGrandTotal = s.Sum(d => d.BCGrandTotal) })).OrderBy(d => d.RecurringSalesId).ToList();
                }
                else
                {
                    var salesdetails = from c in db.Customers.Select(d => new { LId = d.LId, PId = d.PId == null ? d.LId : d.PId })
                                       join s in db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= FromDate && d.SalesInvoice.InvoiceDate <= ToDate && d.Product.CategoryId == CategoryId)
                                       on c.LId equals s.SalesInvoice.Customer.LId
                                       //  group s by new { s.SalesInvoice.Customer.Name } into cs
                                       select new { RecurringSalesId = c.PId, CustomerName = s.SalesInvoice.Customer.Name, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula, TaxOther = s.UnitFormula, TaxProduct = (s.UnitIdSecondary == 1 ? (s.Quantity) : (s.Quantity * s.UnitFormula)), TotalAddAmount = s.TotalAmount, DisAmount = (s.Discount * s.Price * s.Quantity) / 100, RoundOff = s.Price * s.Quantity, BCGrandTotal = s.Price };
                    result = (salesdetails.GroupBy(s => new { s.CustomerName, s.RecurringSalesId })
                   .Select(s => new SalesInvoiceModelView { RecurringSalesId = s.Key.RecurringSalesId, CustomerName = s.Key.CustomerName, SubTotal = s.Sum(d => d.SubTotal), TaxProduct = s.Sum(d => d.TaxProduct), RoundOff = s.Sum(d => d.RoundOff), BCGrandTotal = s.Sum(d=>d.BCGrandTotal) })).OrderBy(d => d.RecurringSalesId).ToList();
                }
            }
            else
            {
                if (CategoryId == null)
                {
                    var salesdetails = from c in db.Customers.Select(d => new { LId = d.LId, PId = d.PId == null ? d.LId : d.PId })
                                       join s in db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= FromDate && d.SalesInvoice.InvoiceDate <= ToDate && d.SalesInvoice.BranchId==Branchid)
                                       on c.LId equals s.SalesInvoice.Customer.LId
                                       //  group s by new { s.SalesInvoice.Customer.Name } into cs
                                       select new { RecurringSalesId = c.PId, CustomerName = s.SalesInvoice.Customer.Name, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula, TaxOther = s.UnitFormula, TaxProduct = (s.UnitIdSecondary == 1 ? (s.Quantity) : (s.Quantity * s.UnitFormula)), TotalAddAmount = s.TotalAmount, DisAmount = (s.Discount * s.Price * s.Quantity) / 100, RoundOff = s.Price * s.Quantity, BCGrandTotal = s.Price };
                    result = (salesdetails.GroupBy(s => new { s.CustomerName, s.RecurringSalesId })
                   .Select(s => new SalesInvoiceModelView { RecurringSalesId = s.Key.RecurringSalesId, CustomerName = s.Key.CustomerName, SubTotal = s.Sum(d => d.SubTotal), TaxProduct = s.Sum(d => d.TaxProduct), RoundOff = s.Sum(d => d.RoundOff), BCGrandTotal = s.Sum(d => d.BCGrandTotal) })).OrderBy(d => d.RecurringSalesId).ToList();
                }
                else
                {
                    var salesdetails = from c in db.Customers.Select(d => new { LId = d.LId, PId = d.PId == null ? d.LId : d.PId })
                                       join s in db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= FromDate && d.SalesInvoice.InvoiceDate <= ToDate && d.Product.CategoryId == CategoryId && d.SalesInvoice.BranchId == Branchid)
                                       on c.LId equals s.SalesInvoice.Customer.LId
                                       //  group s by new { s.SalesInvoice.Customer.Name } into cs
                                       select new { RecurringSalesId = c.PId, CustomerName = s.SalesInvoice.Customer.Name, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula, TaxOther = s.UnitFormula, TaxProduct = (s.UnitIdSecondary == 1 ? (s.Quantity) : (s.Quantity * s.UnitFormula)), TotalAddAmount = s.TotalAmount, DisAmount = (s.Discount * s.Price * s.Quantity) / 100, RoundOff = s.Price * s.Quantity, BCGrandTotal = s.Price };
                    result = (salesdetails.GroupBy(s => new { s.CustomerName, s.RecurringSalesId })
                   .Select(s => new SalesInvoiceModelView { RecurringSalesId = s.Key.RecurringSalesId, CustomerName = s.Key.CustomerName, SubTotal = s.Sum(d => d.SubTotal), TaxProduct = s.Sum(d => d.TaxProduct), RoundOff = s.Sum(d => d.RoundOff), BCGrandTotal = s.Sum(d => d.BCGrandTotal) })).OrderBy(d => d.RecurringSalesId).ToList();
                }
            }
            return PartialView(result);
            
        }
        [HttpGet]
        public ActionResult SalesRegisterPDFlink(string from, string to, int? CategoryId)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            //   string val = Session["datetime"].ToString();
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            List<SalesInvoiceModelView> result = new List<Models.SalesInvoiceModelView>();
            DateTime? FromDate = null;
            DateTime? ToDate = null;
            try
            {
                FromDate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                ToDate = DateTime.ParseExact(to, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                ViewBag.from = from;
                ViewBag.to = to;
                //using (InventoryEntities db = new InventoryEntities())
                //{
                if (Branchid == 0)
                {
                    if (CategoryId == null)
                    {
                        var salesdetails = from c in db.Customers.Select(d => new { LId = d.LId, PId = d.PId == null ? d.LId : d.PId })
                                           join s in db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= FromDate && d.SalesInvoice.InvoiceDate <= ToDate)
                                           on c.LId equals s.SalesInvoice.Customer.LId
                                           //  group s by new { s.SalesInvoice.Customer.Name } into cs
                                           select new { RecurringSalesId = c.PId, CustomerName = s.SalesInvoice.Customer.Name, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula, TaxOther = s.UnitFormula, TaxProduct = (s.UnitIdSecondary == 1 ? (s.Quantity) : (s.Quantity * s.UnitFormula)), TotalAddAmount = s.TotalAmount, DisAmount = (s.Discount * s.Price * s.Quantity) / 100, RoundOff = s.Price * s.Quantity, BCGrandTotal = s.Price };
                        result = (salesdetails.GroupBy(s => new { s.CustomerName, s.RecurringSalesId })
                       .Select(s => new SalesInvoiceModelView { RecurringSalesId = s.Key.RecurringSalesId, CustomerName = s.Key.CustomerName, SubTotal = s.Sum(d => d.SubTotal), TaxProduct = s.Sum(d => d.TaxProduct), RoundOff = s.Sum(d => d.RoundOff), BCGrandTotal = s.Sum(d => d.BCGrandTotal) })).OrderBy(d => d.RecurringSalesId).ToList();
                    }
                    else
                    {
                        var salesdetails = from c in db.Customers.Select(d => new { LId = d.LId, PId = d.PId == null ? d.LId : d.PId })
                                           join s in db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= FromDate && d.SalesInvoice.InvoiceDate <= ToDate && d.Product.CategoryId == CategoryId)
                                           on c.LId equals s.SalesInvoice.Customer.LId
                                           //  group s by new { s.SalesInvoice.Customer.Name } into cs
                                           select new { RecurringSalesId = c.PId, CustomerName = s.SalesInvoice.Customer.Name, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula, TaxOther = s.UnitFormula, TaxProduct = (s.UnitIdSecondary == 1 ? (s.Quantity) : (s.Quantity * s.UnitFormula)), TotalAddAmount = s.TotalAmount, DisAmount = (s.Discount * s.Price * s.Quantity) / 100, RoundOff = s.Price * s.Quantity, BCGrandTotal = s.Price };
                        result = (salesdetails.GroupBy(s => new { s.CustomerName, s.RecurringSalesId })
                       .Select(s => new SalesInvoiceModelView { RecurringSalesId = s.Key.RecurringSalesId, CustomerName = s.Key.CustomerName, SubTotal = s.Sum(d => d.SubTotal), TaxProduct = s.Sum(d => d.TaxProduct), RoundOff = s.Sum(d => d.RoundOff), BCGrandTotal = s.Sum(d => d.BCGrandTotal) })).OrderBy(d => d.RecurringSalesId).ToList();
                    }
                    return new Rotativa.PartialViewAsPdf(result)
                    {
                        FileName = "SalesRegister.pdf",
                        PageSize = Size.A4,
                        PageMargins = new Margins(10, 10, 10, 10),
                        CustomSwitches = "--footer-right [page]/[topage]"//customSwitches
                                                                         // PageMargins = new Margins(0, 0, 0, 0),
                                                                         //CustomSwitches = "--print-media-type  --footer-center [page] --footer-font-size 8"

                    };
                }
                else
                {
                    if (CategoryId == null)
                    {
                        var salesdetails = from c in db.Customers.Select(d => new { LId = d.LId, PId = d.PId == null ? d.LId : d.PId })
                                           join s in db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= FromDate && d.SalesInvoice.InvoiceDate <= ToDate && d.SalesInvoice.BranchId == Branchid)
                                           on c.LId equals s.SalesInvoice.Customer.LId
                                           //  group s by new { s.SalesInvoice.Customer.Name } into cs
                                           select new { RecurringSalesId = c.PId, CustomerName = s.SalesInvoice.Customer.Name, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula, TaxOther = s.UnitFormula, TaxProduct = (s.UnitIdSecondary == 1 ? (s.Quantity) : (s.Quantity * s.UnitFormula)), TotalAddAmount = s.TotalAmount, DisAmount = (s.Discount * s.Price * s.Quantity) / 100, RoundOff = s.Price * s.Quantity, BCGrandTotal = s.Price };
                        result = (salesdetails.GroupBy(s => new { s.CustomerName, s.RecurringSalesId })
                       .Select(s => new SalesInvoiceModelView { RecurringSalesId = s.Key.RecurringSalesId, CustomerName = s.Key.CustomerName, SubTotal = s.Sum(d => d.SubTotal), TaxProduct = s.Sum(d => d.TaxProduct), RoundOff = s.Sum(d => d.RoundOff), BCGrandTotal = s.Sum(d => d.BCGrandTotal) })).OrderBy(d => d.RecurringSalesId).ToList();
                    }
                    else
                    {
                        var salesdetails = from c in db.Customers.Select(d => new { LId = d.LId, PId = d.PId == null ? d.LId : d.PId })
                                           join s in db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= FromDate && d.SalesInvoice.InvoiceDate <= ToDate && d.Product.CategoryId == CategoryId && d.SalesInvoice.BranchId == Branchid)
                                           on c.LId equals s.SalesInvoice.Customer.LId
                                           //  group s by new { s.SalesInvoice.Customer.Name } into cs
                                           select new { RecurringSalesId = c.PId, CustomerName = s.SalesInvoice.Customer.Name, SubTotal = s.UnitIdSecondary == 1 ? s.UnitFormula : s.Quantity, GrandTotal = s.UnitIdSecondary == 1 ? (s.Quantity / s.UnitFormula) : s.UnitFormula, TaxOther = s.UnitFormula, TaxProduct = (s.UnitIdSecondary == 1 ? (s.Quantity) : (s.Quantity * s.UnitFormula)), TotalAddAmount = s.TotalAmount, DisAmount = (s.Discount * s.Price * s.Quantity) / 100, RoundOff = s.Price * s.Quantity, BCGrandTotal = s.Price };
                        result = (salesdetails.GroupBy(s => new { s.CustomerName, s.RecurringSalesId })
                       .Select(s => new SalesInvoiceModelView { RecurringSalesId = s.Key.RecurringSalesId, CustomerName = s.Key.CustomerName, SubTotal = s.Sum(d => d.SubTotal), TaxProduct = s.Sum(d => d.TaxProduct), RoundOff = s.Sum(d => d.RoundOff), BCGrandTotal = s.Sum(d => d.BCGrandTotal) })).OrderBy(d => d.RecurringSalesId).ToList();
                    }

                    return new Rotativa.PartialViewAsPdf(result)
                    {
                        FileName = "SalesRegister.pdf",
                        PageSize = Size.A4,
                        PageMargins = new Margins(10, 10, 10, 10),
                        CustomSwitches = "--footer-right [page]/[topage]"//customSwitches
                                                                         // PageMargins = new Margins(0, 0, 0, 0),
                                                                         //CustomSwitches = "--print-media-type  --footer-center [page] --footer-font-size 8"

                    };
                }
                //}
            }
            catch
            {
                return new ActionAsPdf("DespatchRegisterReport", new { id = Branchid, value = "0", From = FromDate, To = ToDate }) { FileName = "SalesRegister.pdf" };
            }


        }
        #endregion ---- Sales Analysis Register--------------

        #region ---- Sales By Date Summary--------------

        [HttpGet]
        public ActionResult SalesbyDateSummary(string Msg, string Err)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
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
            ViewBag.Ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.groupID == 113).Select(d => new { d.LID, d.ledgerName }).ToList();
            ViewBag.Customer = db.Customers.Where(d => d.CompanyId == companyid).Select(d => new { d.Id, d.Name }).OrderBy(d => d.Name).ToList();
            ViewBag.ShedList = db.ShedMasters.Where(d => d.BranchId == Branchid).ToList();
            var mode = new SelectList(new[]
                             {
                                              new{ID=0,Name="ALL"},
                                              new{ID=2,Name="Cash"},
                                              new{ID=1,Name="Credit"},


                                          },
         "ID", "Name");
            ViewData["mod"] = mode;
            return View();


        }


        [HttpGet]
        public ActionResult SalesbyDateSummaryReport(string From, string To, long? ledgerId = 0, int? Mode = 0, long? customerId = 0, int? shedId = 0)
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
            Session["Mode"] = Mode;
            Session["customerId"] = customerId;
            Session["shedId"] = shedId;

            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            List<SalesInvoiceModelView> result = null;



            if (From != "0")
            {
                Session["datetime"] = 1;

                if (Branchid == 0)
                {

                    if (Mode == 0)
                    {
                        if (ledgerId == 0)
                            result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                        else
                            result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate).GroupBy(s => new {  s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                    }
                    else
                    {
                        if (ledgerId == 0)
                            result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                        else
                            result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                    }

                }
                else
                {
                    if (shedId == 0)
                    {
                        if (customerId == 0)
                        {
                            if (Mode == 0)
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                            else
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                        }
                        else
                        {
                            if (Mode == 0)
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.CustomerId == customerId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.CustomerId == customerId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                            else
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode && s.CustomerId == customerId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode && s.CustomerId == customerId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                        }
                    }
                    else
                    {
                        if (customerId == 0)
                        {
                            if (Mode == 0)
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.ShedId == shedId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.ShedId == shedId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                            else
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode && s.ShedId == shedId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode && s.ShedId == shedId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                        }
                        else
                        {
                            if (Mode == 0)
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.CustomerId == customerId && s.ShedId == shedId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.CustomerId == customerId && s.ShedId == shedId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                            else
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode && s.CustomerId == customerId && s.ShedId == shedId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode && s.CustomerId == customerId && s.ShedId == shedId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                        }
                    }
                }
                    
                return PartialView(result);
            }
            else
            {
                Session["datetime"] = 0;

                if (ledgerId == 0)
                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                else
                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.LID == ledgerId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();

                return PartialView(result);
            }


        }



        public FileResult SalesDateSummaryExport()
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


                var result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.Date >= fromDate && s.Date <= toDate).ToList();


                //Create new Excel workbook
                var workbook = new HSSFWorkbook();

                //Create new Excel sheet
                var sheet = workbook.CreateSheet();

                //(Optional) set the width of the columns
                sheet.SetColumnWidth(0, 50 * 56);
                sheet.SetColumnWidth(1, 50 * 56);
                sheet.SetColumnWidth(2, 50 * 56);
                sheet.SetColumnWidth(3, 50 * 156);
                sheet.SetColumnWidth(4, 50 * 56);
                sheet.SetColumnWidth(5, 50 * 86);
                sheet.SetColumnWidth(6, 50 * 86);
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
                cell.SetCellValue("Sales By Date");
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
                cell.SetCellValue("Sub Total");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(5);
                cell.SetCellValue("Discount");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(6);
                cell.SetCellValue("Net Total");
                cell.CellStyle = detailCellStyle;


                sheet.CreateFreezePane(0, 1, 0, 1);



                //Populate the sheet with values from the grid data
                decimal? subtotal = 0;
                decimal? discounttotal = 0;
                decimal? total = 0;
                int serial = 0;
                foreach (var product in result)
                {
                    var sub = (decimal)(product.BCGrandTotal + product.Dis);
                    subtotal += sub;
                    var discount = product.Dis;
                    discounttotal += discount;
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
                    row.CreateCell(4).SetCellValue(Math.Round((double)sub, 2));
                    row.CreateCell(5).SetCellValue(Math.Round((double)discount, 2));
                    row.CreateCell(6).SetCellValue(Math.Round((double)gtotal, 2));

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

                var jan = rowTotal.CreateCell(6);
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
                    "SalesByDate.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user




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
                sheet.SetColumnWidth(3, 50 * 156);
                sheet.SetColumnWidth(4, 50 * 56);
                sheet.SetColumnWidth(5, 50 * 86);
                sheet.SetColumnWidth(6, 50 * 86);
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
                cell.SetCellValue("Sales Till Date");
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
                cell.SetCellValue("Sub Total");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(5);
                cell.SetCellValue("Discount");
                cell.CellStyle = detailCellStyle;

                cell = headerRow.CreateCell(6);
                cell.SetCellValue("Net Total");
                cell.CellStyle = detailCellStyle;


                sheet.CreateFreezePane(0, 1, 0, 1);



                //Populate the sheet with values from the grid data
                decimal? subtotal = 0;
                decimal? discounttotal = 0;
                decimal? total = 0;
                int serial = 0;
                foreach (var product in result)
                {
                    var sub = (decimal)(product.BCGrandTotal + product.Dis);
                    subtotal += sub;
                    var discount = product.Dis;
                    discounttotal += discount;
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
                    row.CreateCell(4).SetCellValue(Math.Round((double)sub, 2));
                    row.CreateCell(5).SetCellValue(Math.Round((double)discount, 2));
                    row.CreateCell(6).SetCellValue(Math.Round((double)gtotal, 2));

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

                var jan = rowTotal.CreateCell(6);
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
                    "SalesByDate.xls");     //Suggested file name in the "Save as" dialog which will be displayed to the end user

            }



        }


        [HttpGet]
        public ActionResult SalesbyDateSummaryPDF(int id, string value, string GST, DateTime? From, DateTime? To)
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
        public ActionResult SalesbyDateSummaryPrint()
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
        public ActionResult SalesbyDateSummaryPDFlink()
        {
            // int compid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt32(Session["companyid"]);

            List<SalesInvoiceModelView> result = null;
            string val = Session["datetime"].ToString();
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            //string GST = Session["GST"].ToString();
            int? ledgerId = Convert.ToInt32(Session["ledgerId"].ToString());
            int? Mode = Convert.ToInt32(Session["Mode"].ToString());
            int? customerId = Convert.ToInt32(Session["customerId"].ToString());
            int? shedId = Convert.ToInt32(Session["shedId"].ToString());
            DateTime? FromDate = null;
            DateTime? ToDate = null;
            try
            {
                FromDate = DateTime.Parse(Convert.ToString(Session["fdate"]));
                ToDate = DateTime.Parse(Convert.ToString(Session["tdate"]));
                ViewBag.from = FromDate;
                ViewBag.to = ToDate;


                //Get the data representing the current grid state - page, sort and filter
                //if (Branchid == 0)
                //{

                //    if (Mode == 0)
                //    {
                //        if (ledgerId == 0)
                //            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct) }).OrderBy(s => s.CreatedOn).ToList();
                //        else
                //            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                //    }
                //    else
                //    {
                //        if (ledgerId == 0)
                //            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct) }).OrderBy(s => s.CreatedOn).ToList();
                //        else
                //            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                //    }

                //}
                //else
                //{
                //    if (Mode == 0)
                //    {
                //        if (ledgerId == 0)
                //            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                //        else
                //            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                //    }
                //    else
                //    {
                //        if (ledgerId == 0)
                //            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                //        else
                //            result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.LID == ledgerId && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate && s.SalesInvoice.Mode == Mode).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                //    }
                //}
                //if (Branchid == 0)
                //{
                //    if (GST == "All")
                //        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct) }).OrderBy(s => s.CreatedOn).ToList();
                //    else if (GST == "Registered")
                //        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.Customer.GstVatNumber != null && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                //    else
                //        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.Customer.GstVatNumber == null && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();


                //from = DateTime.Parse(Convert.ToString(Session["fdate"]));
                //to = DateTime.Parse(Convert.ToString(Session["tdate"]));

                //return new ActionAsPdf("SalesbyDatePDF", new { id = Branchid, value = "1", From = from, To = to }) { FileName = "SalesByDate.pdf" };
                if (Branchid == 0)
                {

                    if (Mode == 0)
                    {
                        if (ledgerId == 0)
                            result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                        else
                            result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                    }
                    else
                    {
                        if (ledgerId == 0)
                            result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                        else
                            result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                    }

                }
                else
                {
                    if (shedId == 0)
                    {
                        if (customerId == 0)
                        {
                            if (Mode == 0)
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                            else
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                        }
                        else
                        {
                            if (Mode == 0)
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.CustomerId == customerId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.CustomerId == customerId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                            else
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode && s.CustomerId == customerId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode && s.CustomerId == customerId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                        }
                    }
                    else
                    {
                        if (customerId == 0)
                        {
                            if (Mode == 0)
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.ShedId == shedId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.ShedId == shedId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                            else
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode && s.ShedId == shedId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode && s.ShedId == shedId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                        }
                        else
                        {
                            if (Mode == 0)
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.CustomerId == customerId && s.ShedId == shedId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.CustomerId == customerId && s.ShedId == shedId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                            else
                            {
                                if (ledgerId == 0)
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode && s.CustomerId == customerId && s.ShedId == shedId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                                else
                                    result = db.SalesInvoices.Where(s => s.CompanyId == companyid && s.BranchId == Branchid && s.LID == ledgerId && s.InvoiceDate >= FromDate && s.InvoiceDate <= ToDate && s.Mode == Mode && s.CustomerId == customerId && s.ShedId == shedId).GroupBy(s => new { s.InvoiceDate }).Select(s => new SalesInvoiceModelView { CreatedOn = s.Key.InvoiceDate, BCGrandTotal = s.Sum(d => d.BCGrandTotal) }).OrderBy(s => s.CreatedOn).ToList();
                            }
                        }
                    }
                }
                return new Rotativa.PartialViewAsPdf("SalesbyDateSummaryPDF", result)
                {
                    FileName = "SalesbyDateSummaryReport.pdf",
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
                //        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                //    else if (GST == "Registered")
                //        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.Customer.GstVatNumber != null && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();
                //    else
                //        result = db.SalesInvoiceDetails.Where(s => s.SalesInvoice.CompanyId == companyid && s.SalesInvoice.BranchId == Branchid && s.SalesInvoice.Customer.GstVatNumber == null && s.SalesInvoice.InvoiceDate >= FromDate && s.SalesInvoice.InvoiceDate <= ToDate).GroupBy(s => new { s.SalesInvoice.NO, s.SalesInvoice.InvoiceDate, s.SalesInvoice.Customer.Name }).Select(s => new SalesInvoiceModelView { NO = s.Key.NO, CreatedOn = s.Key.InvoiceDate, CustomerName = s.Key.Name, SubTotal = s.Sum(d => d.Quantity * d.Price), DisAmount = s.Sum(d => d.Price * d.Quantity * d.Discount / 100), TotalDeductAmount = s.Average(d => d.SalesInvoice.TotalDeductAmount), TotalAddAmount = s.Average(d => d.SalesInvoice.TotalAddAmount), BCGrandTotal = s.Average(d => d.SalesInvoice.BCGrandTotal), RoundOff = s.Average(d => d.SalesInvoice.RoundOff), TaxProduct = s.Average(d => d.SalesInvoice.TaxProduct), TaxOther = s.Average(d => d.SalesInvoice.TaxOther) }).OrderBy(s => s.CreatedOn).ToList();


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
                return new ActionAsPdf("SalesbyDateSummaryPDF", new { id = Branchid, value = "0", From = FromDate, To = ToDate }) { FileName = "SalesByDate.pdf" };
            }

        }

        #endregion

    }
}
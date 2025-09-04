using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using XenERP.Models.Repository;
using System.Transactions;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Data.Entity.Validation;

namespace XenERP.Controllers
{
    [SessionExpire]
    public class CustomerController : Controller
    {
        InventoryEntities db = new InventoryEntities();
        TaxRepository taxobj = new TaxRepository();
        public ActionResult Index()
        {
            if (Session["name"] == "" || Session["name"] == null)
            {

                return View("ShowAllCustomer");
            }

            else
            {
                return View("ShowAllCustomer");
            }
        }



        public ActionResult ShowAllCustomer(string Msg, string Err)
        {

            Session["name"] = "naresh";

            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }


            int companyid = Convert.ToInt32(Session["companyid"]);
            List<Customer> customer = new List<Customer>();
            int userid = Convert.ToInt32(Session["userid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            if (Branchid == 0)
                customer = db.Customers.Where(d => d.UserId == userid && d.CompanyId == companyid).OrderBy(d => d.Name).ToList();
            else
                customer = db.Customers.Where(d => d.UserId == userid && d.CompanyId == companyid && d.BranchId == Branchid).OrderBy(d => d.Name).ToList();
            return View(customer);

        }

        public JsonResult GetLedgerName(int type)
        {

            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);

            var group = taxobj.GetLedName(type, companyid, Branchid);
            return Json(group, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateCustomer()
        {
            int userid = Convert.ToInt32(Session["userid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int companyid = Convert.ToInt32(Session["companyid"]);

            //ViewBag.Customertype = db.CustomerTypes.ToList();

            var SalesPerson = db.Users.Where(r => r.BranchId == Branchid && r.RoleId == 11).Select(s => new { Id = s.Id, Name = s.FirstName + " " + s.LastName }).ToList();
            ViewBag.SalesPerson = SalesPerson;



            var list = new SelectList(new[]
                                          {
                                              new {ID="Wholesalers",Name="Wholesalers"},
                                              new{ID="Retailers",Name="Retailers"},
                                              new{ID="Distributors",Name="Distributors"},
                                                new {ID="Others",Name="Others"},

                                          },
                            "ID", "Name");
            ViewData["type"] = list;













            //  ViewBag.Group = db.GroupMasters.Where(d => d.CompanyId == companyid && d.UserId == userid || d.groupID == 23 || d.groupID == 24 || d.groupID == 25 || d.groupID == 26).OrderBy(d => d.groupName);

            //var currencyrate = db.CurrencyRates.Where(d => d.CompanyId == companyid && d.UserId == userid).Select(d => new { Id = d.Currency.CurrencyId, Name = d.Currency.ISO_4217 + "(" + d.Currency.Country + "," + d.Currency.Currency1 + ")" }).ToList();
            var currencyrate = db.CurrencyRates.Where(d => d.CompanyId == companyid && d.UserId == userid).Select(d => new { Id = d.Id, Name = d.Currency.ISO_4217 + "(" + d.Currency.Country + "," + d.Currency.Currency1 + ")" }).ToList();
            ViewBag.currency = currencyrate;



            // ViewBag.taxrate = db.Taxes.Where(d => d.CompanyId == companyid && d.UserId == userid && d.BranchId == Branchid || d.CompanyId == 0 && d.BranchId == 0).ToList();


            //    ViewBag.warehouse = db.Warehouses.Where(d => d.Companyid == companyid && d.Branchid == Branchid || d.Companyid == 0 && d.Branchid == 0).ToList();
            return View();
        }



        public JsonResult CheckCustomerCode(string Id, string Code)
        {

            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            int id = 0;
            string codes = Code;
            string trimcode = codes.Trim();

            if (String.IsNullOrEmpty(Id) || String.IsNullOrWhiteSpace(Id))
            {

                bool Iscustomer = db.Customers.Any(d => d.Code == trimcode && (d.CompanyId == companyid && d.BranchId == Branchid) || (d.CompanyId == companyid && d.BranchId == 0));
                return Json(!Iscustomer, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //    id = Convert.ToInt32(Id);

                bool Iscustomer = db.Customers.Where(d => d.Id != id).Any(d => d.Code == trimcode && (d.CompanyId == companyid && d.BranchId == Branchid) || (d.CompanyId == companyid && d.BranchId == 0));
                return Json(Iscustomer, JsonRequestBehavior.AllowGet);
            }
        }




        [HttpPost]
        public ActionResult CreateCustomer(Customer customer, FormCollection collection)
        {
            var scope = new TransactionScope(TransactionScopeOption.RequiresNew,
              new TransactionOptions()
              {
                  IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
              }


              );

            using (scope)
            {
                try
                {
                    int companyid = Convert.ToInt32(Session["companyid"]);
                    long Branchid = Convert.ToInt64(Session["BranchId"]);
                    int userid = Convert.ToInt32(Session["userid"]);



                    var list = new SelectList(new[]
                                         {
                                              new {ID="Wholesalers",Name="Wholesalers"},
                                              new{ID="Retailers",Name="Retailers"},
                                              new{ID="Distributors",Name="Distributors"},
                                                new {ID="Others",Name="Others"},

                                          },
                           "ID", "Name");
                    ViewData["type"] = list;

                    //   ViewBag.Group = db.GroupMasters.Where(d => d.CompanyId == companyid && d.UserId == userid || d.groupID == 23 || d.groupID == 24 || d.groupID == 25 || d.groupID == 26).OrderBy(d => d.groupName);
                    var currencyrate = db.CurrencyRates.Where(d => d.CompanyId == companyid && d.UserId == userid).Select(d => new { Id = d.Id, Name = d.Currency.ISO_4217 + "(" + d.Currency.Country + "," + d.Currency.Currency1 + ")" }).ToList();
                    ViewBag.currency = currencyrate;

                    var SalesPerson = db.Users.Where(r => r.BranchId == Branchid && r.RoleId == 11).Select(s => new { Id = s.Id, Name = s.FirstName + " " + s.LastName }).ToList();
                    ViewBag.SalesPerson = SalesPerson;
                    //  ViewBag.taxrate = db.Taxes.Where(d => d.CompanyId == companyid && d.UserId == userid && d.BranchId == Branchid || d.CompanyId == 0 && d.BranchId == 0).ToList();

                    int fYearid = Convert.ToInt32(Session["fid"]);

                    LedgerMaster ledger = new LedgerMaster();

                    int ledgeridint = 0;
                    string LedgerID = string.Empty;

                    string ledgerid = db.LedgerMasters.Where(l => l.parentID == 32 && l.CompanyId == companyid && l.BranchId == Branchid).OrderByDescending(l => l.LID).Select(l => l.ledgerID).FirstOrDefault();

                    if (ledgerid == null)
                    {
                        LedgerID = "20251001";
                    }
                    else
                    {
                        ledgeridint = Convert.ToInt32(ledgerid) + 1;
                        LedgerID = Convert.ToString(ledgeridint);
                    }

                    ledger.ledgerID = LedgerID;
                    ledger.parentID = 32;
                    ledger.ledgerName = customer.Name;
                    ledger.groupID = 108;
                    ledger.CompanyId = companyid;
                    ledger.fYearID = fYearid;
                    ledger.ledgerType = "General";
                    ledger.BranchId = Branchid;
                    ledger.UserId = userid;

                    int Createdby = Convert.ToInt32(Session["Createdid"]);
                    ledger.CreatedBy = Createdby;
                    ledger.CreatedOn = DateTime.Now;
                    db.LedgerMasters.Add(ledger);
                    db.SaveChanges();


                    //   long ledid = taxobj.GetLedgerInsertId(ledger);
                    if (customer.OpeningBal == null)
                        customer.OpeningBal = 0;
                    var openingBalance = new OpeningBalance();
                    openingBalance.BranchId = Branchid;
                    openingBalance.CompanyId = companyid;
                    openingBalance.fYearID = fYearid;
                    openingBalance.ledgerID = ledger.LID;
                    openingBalance.openingBal = (decimal)customer.OpeningBal;
                    openingBalance.UserId = userid;
                    openingBalance.CreatedBy = Createdby;
                    openingBalance.CreatedOn = DateTime.Now;
                    db.OpeningBalances.Add(openingBalance);
                    db.SaveChanges();


                    customer.LId = ledger.LID;
                    //     customer.DiscountLedger = Convert.ToInt32(collection["DiscountLedger"]);

                    customer.UserId = userid;
                    customer.CompanyId = companyid;
                    customer.BranchId = Branchid;

                    var getPrefix = db.Prefixes.Where(d => d.CompanyId == companyid && d.DefaultPrefix == "CUS").FirstOrDefault();
                    if (getPrefix.SetPrefix != null)
                        customer.Code = getPrefix.SetPrefix + customer.Code;
                    else
                        customer.Code = getPrefix.DefaultPrefix + customer.Code;

                    var checkDuplicate = db.Customers.Any(d => d.Code == customer.Code && d.CompanyId == companyid && d.BranchId == Branchid);
                    if (checkDuplicate)
                    {
                        ViewBag.Error = "Customer Code Already Exists.";
                        return View(customer);
                    }
                    //  int Createdby = Convert.ToInt32(Session["Createdid"]);

                    //try
                    //{
                    //    int maxcount = 0;
                    //    var cust = db.Customers.Where(d => d.CompanyId == companyid && d.BranchId == Branchid).Count();
                    //    if (cust > 0)
                    //    {
                    //        var getmax = db.Customers.Where(d => d.CompanyId == companyid && d.BranchId == Branchid).OrderByDescending(d => d.Id).Select(d => d.Code).FirstOrDefault();
                    //        var maxstr = getmax.Substring(getmax.Length - 4);
                    //        maxcount = Convert.ToInt32(maxstr);
                    //    }
                    //    var getPrefix = db.Prefixes.Where(d => d.CompanyId == companyid && d.BranchId == Branchid && d.DefaultPrefix == "CUS").FirstOrDefault();
                    //    if (cust == 0)
                    //    {
                    //        if(getPrefix.SetPrefix!=null)
                    //            customer.Code = getPrefix.SetPrefix+"1001";
                    //        else
                    //            customer.Code = getPrefix.DefaultPrefix + "1001";
                    //    }
                    //    else
                    //    {

                    //        maxcount = maxcount + 1;

                    //        if (getPrefix.SetPrefix != null)
                    //            customer.Code = getPrefix.SetPrefix + maxcount;
                    //        else
                    //            customer.Code = getPrefix.DefaultPrefix + maxcount;
                    //    }
                    //}
                    //catch
                    //{
                    //}
                    if (!(customer.PCode == "" || customer.PCode == null))
                    {
                        customer.PCode = customer.PCode.Trim().ToUpper();

                        if (customer.PCode == "")
                        {
                            customer.PId = null;
                        }
                        else
                        {
                            var getParentId = db.Customers.Where(d => d.CompanyId == companyid && d.BranchId == Branchid && d.Code.ToUpper() == customer.PCode).Select(d => d.Id).FirstOrDefault();
                            if (getParentId != 0)
                                customer.PId = getParentId;
                            else
                            {
                                ViewBag.Error = "Parent Code Not Found.";
                                return View(customer);
                            }
                        }
                    }
                    else
                    {
                        customer.PId = null;
                    }


                    customer.CreatedBy = Createdby;
                    customer.CreatedOn = DateTime.Now;
                    db.Customers.Add(customer);
                    db.SaveChanges();
                    scope.Complete();

                    return RedirectToAction("ShowAllCustomer", new { Msg = "Data Saved Successfully...." });
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
            }
        }




        [HttpGet]
        public ActionResult EditCustomer(int id)
        {

            var customer = db.Customers.Where(d => d.Id == id).FirstOrDefault();
            try
            {


                var list = new SelectList(new[]
                                          {
                                              new {ID="Wholesalers",Name="Wholesalers"},
                                              new{ID="Retailers",Name="Retailers"},
                                              new{ID="Distributors",Name="Distributors"},
                                                new {ID="Others",Name="Others"},

                                          },
                                "ID", "Name");
                ViewData["type"] = list;





                //ViewBag.Customertype = db.CustomerTypes.ToList();
                int companyid = Convert.ToInt32(Session["companyid"]);


                long Branchid = Convert.ToInt64(Session["BranchId"]);

                int userid = Convert.ToInt32(Session["userid"]);


                var currency = db.Companies.Where(d => d.Id == companyid).FirstOrDefault();



                var currencyrate = db.CurrencyRates.Where(d => d.CompanyId == companyid && d.UserId == userid).Select(d => new { Id = d.Id, Name = d.Currency.ISO_4217 + "(" + d.Currency.Country + "," + d.Currency.Currency1 + ")" }).ToList();
                ViewBag.currency = currencyrate;
                var SalesPerson = db.Users.Where(r => r.BranchId == Branchid && r.RoleId == 11).Select(s => new { Id = s.Id, Name = s.FirstName + " " + s.LastName }).ToList();
                ViewBag.SalesPerson = SalesPerson;
                return View(customer);
            }
            catch
            {
                return View();
            }

        }


        [HttpPost]
        public ActionResult EditCustomer(Customer customer)
        {
            try
            {


                var list = new SelectList(new[]
                                          {
                                              new {ID="Wholesalers",Name="Wholesalers"},
                                              new{ID="Retailers",Name="Retailers"},
                                              new{ID="Distributors",Name="Distributors"},
                                                new {ID="Others",Name="Others"},

                                          },
                                "ID", "Name");
                ViewData["type"] = list;






                int companyid = Convert.ToInt32(Session["companyid"]);


                long Branchid = Convert.ToInt64(Session["BranchId"]);

                int userid = Convert.ToInt32(Session["userid"]);
                int Fyid = Convert.ToInt32(Session["fid"]);

                var currency = db.Companies.Where(d => d.Id == companyid).FirstOrDefault();



                var currencyrate = db.CurrencyRates.Where(d => d.CompanyId == companyid && d.UserId == userid).Select(d => new { Id = d.Id, Name = d.Currency.ISO_4217 + "(" + d.Currency.Country + "," + d.Currency.Currency1 + ")" }).ToList();
                ViewBag.currency = currencyrate;


                int Createdby = Convert.ToInt32(Session["Createdid"]);
                var ledger = db.LedgerMasters.Find(customer.LId);
                ledger.ledgerName = customer.Name;
                ledger.ModifiedBy = Createdby;
                ledger.ModifiedOn = DateTime.Now;
                if (customer.OpeningBal == null)
                    customer.OpeningBal = 0;
                var openingBalance = db.OpeningBalances.Where(o => o.ledgerID == customer.LId && o.fYearID == Fyid).OrderByDescending(o => o.Id).FirstOrDefault();
                openingBalance.openingBal = (decimal)customer.OpeningBal;
                openingBalance.ModifiedBy = Createdby;
                openingBalance.ModifiedOn = DateTime.Now;

                var supplierdet = db.Customers.Find(customer.Id);
                if (customer.PCode != supplierdet.PCode)
                {
                    if (!(customer.PCode == "" || customer.PCode == null))
                    {
                        customer.PCode = customer.PCode.Trim().ToUpper();
                        if (customer.PCode == "")
                        {
                            customer.PId = null;
                        }
                        else
                        {
                            var getParentId = db.Customers.Where(d => d.CompanyId == companyid && d.BranchId == Branchid && d.Code.ToUpper() == customer.PCode).Select(d => d.Id).FirstOrDefault();
                            if (getParentId != 0)
                                customer.PId = getParentId;
                            else
                            {
                                ViewBag.Error = "Parent Code Not Found.";
                                return View(customer);
                            }
                        }
                    }
                    else
                    {
                        customer.PId = null;
                    }
                }

                customer.ModifiedBy = Createdby;
                customer.ModifiedOn = DateTime.Now;

                db.Entry(supplierdet).CurrentValues.SetValues(customer);
                db.SaveChanges();

                return RedirectToAction("ShowAllCustomer", new { Msg = "Data Updated Successfully...." });
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
            //catch (DataException)
            //{
            //    //Log the error (add a variable name after DataException)
            //    ViewBag.Error = "Error:Data  not Saved Successfully.......";
            //    return RedirectToAction("ShowAllCustomer", new { Err = "Please Try Again !...." });

            //}


            //catch
            //{
            //    return RedirectToAction("ShowAllCustomer", new { Err = InventoryMessage.UpdateError });
            //}
        }



        [HttpGet]
        public ActionResult DeleteCustomer(Customer customer)
        {
            try
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);

                var resultfind = db.SalesInvoices.Where(d => d.CompanyId == companyid && d.BranchId == Branchid && d.CustomerId == customer.Id).Count();
                if (resultfind == 0)
                {

                    var result = db.Customers.Where(d => d.CompanyId == companyid && d.BranchId == Branchid && d.Id == customer.Id).FirstOrDefault();

                    db.Customers.Remove(result);
                    var ledger = db.LedgerMasters.Find(result.LId);
                    db.LedgerMasters.Remove(ledger);
                    db.SaveChanges();
                    return RedirectToAction("ShowAllCustomer", new { Msg = "Row deleted Successfully...." });
                }
                else
                {
                    return RedirectToAction("ShowAllCustomer", new { Err = "Row cannot be deleted because it is used in transaction...." });
                }


            }
            catch
            {
                return RedirectToAction("ShowAllCustomer", new { Err = InventoryMessage.InsertError });
            }
        }



        [HttpGet]
        public ActionResult CustomerDetails(Customer customer)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            var result = db.Customers.Where(d => d.Id == customer.Id).FirstOrDefault();
            return View(result);

        }


        [HttpPost]
        public JsonResult getCustomer(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            // List<Customer> customers=new List<Customer>();
            if (Branchid == 0)
            {
                var customers = db.Customers.Where(p => p.Code.Contains(query) && (p.CompanyId == companyid)).Select(p => new { Code = p.Code, Id = p.Id }).ToList();
                return Json(customers, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var customers = db.Customers.Where(p => p.Code.Contains(query) && (p.CompanyId == companyid)).Select(p => new { Code = p.Code, Id = p.Id }).ToList();
                return Json(customers, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult getCustomerN(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            if (Branchid == 0)
            {
                var customers = db.Customers.Where(p => p.Name.Contains(query) && (p.CompanyId == companyid)).Select(p => new { Name = p.Name, Id = p.Id }).ToList();
                return Json(customers, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var customers = db.Customers.Where(p => p.Name.Contains(query) && (p.CompanyId == companyid)).Select(p => new { Name = p.Name, Id = p.Id }).ToList();
                return Json(customers, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getSelectedCustomer(int id)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            if (Branchid == 0)
            {
                var customers = db.Customers.Where(p => p.Id == id && (p.CompanyId == companyid)).Select(p => new { Name = p.Name, TaxId = p.TaxId, Code = p.Code }).FirstOrDefault();
                return Json(customers, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var customers = db.Customers.Where(p => p.Id == id && (p.CompanyId == companyid)).Select(p => new { Name = p.Name, TaxId = p.TaxId, Code = p.Code }).FirstOrDefault();
                return Json(customers, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult checkCustomer(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            bool DisApplicable = false;
            bool FD1Applicable = false;
            bool FD2Applicable = false;
            bool FD3Applicable = false;
            bool FD4Applicable = false;

            var getFurtherDiscount = db.FurtherDiscounts.Where(p => (p.CompanyId == companyid)).FirstOrDefault();
            if (getFurtherDiscount != null)
            {

                DisApplicable = (bool)getFurtherDiscount.Discount;
                FD1Applicable = (bool)getFurtherDiscount.FD1;
                FD2Applicable = (bool)getFurtherDiscount.FD2;
                FD3Applicable = (bool)getFurtherDiscount.FD3;
                FD4Applicable = (bool)getFurtherDiscount.FD4;

            }
            if (Branchid == 0)
            {
                var customers = db.Customers.Where(p => p.Code == query && (p.CompanyId == companyid)).Select(p => new { Id = p.Id, Name = p.Name, TaxId = p.TaxId, Code = p.Code, CurrencyId = p.CurrencyId, CurrencyPurchaseRate = p.CurrencyRate.PurchaseRate, CurrencySaleRate = p.CurrencyRate.SellRate, CurrencyCode = p.CurrencyRate.Currency.ISO_4217, CurrencyName = p.CurrencyRate.Currency.Currency1, Discount = p.Discount, DelAddressName = p.DelAddressName, DelSuburb = p.DelSuburb, DelCity = p.DelCity, DelStateRegion = p.DelStateRegion, DelCountry = p.DelCountry, DelPostalCode = p.DelPostalCode, ContactName = p.ContactName, ContactEmail = p.ContactEmail, ContactMobile = p.ContactMobile, IsFDCompounded = p.IsFDCompounded, DisApplicable = DisApplicable, FD1Applicable = FD1Applicable, FD2Applicable = FD2Applicable, FD3Applicable = FD3Applicable, FD4Applicable = FD4Applicable, FD1 = p.FD1, FD2 = p.FD2, FD3 = p.FD3, FD4 = p.FD4 }).FirstOrDefault();
                return Json(customers, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var customers = db.Customers.Where(p => p.Code == query && (p.CompanyId == companyid)).Select(p => new { Id = p.Id, Name = p.Name, TaxId = p.TaxId, Code = p.Code, CurrencyId = p.CurrencyId, CurrencyPurchaseRate = p.CurrencyRate.PurchaseRate, CurrencySaleRate = p.CurrencyRate.SellRate, CurrencyCode = p.CurrencyRate.Currency.ISO_4217, CurrencyName = p.CurrencyRate.Currency.Currency1, Discount = p.Discount, DelAddressName = p.DelAddressName, DelSuburb = p.DelSuburb, DelCity = p.DelCity, DelStateRegion = p.DelStateRegion, DelCountry = p.DelCountry, DelPostalCode = p.DelPostalCode, ContactName = p.ContactName, ContactEmail = p.ContactEmail, ContactMobile = p.ContactMobile, IsFDCompounded = p.IsFDCompounded, DisApplicable = DisApplicable, FD1Applicable = FD1Applicable, FD2Applicable = FD2Applicable, FD3Applicable = FD3Applicable, FD4Applicable = FD4Applicable, FD1 = p.FD1, FD2 = p.FD2, FD3 = p.FD3, FD4 = p.FD4 }).FirstOrDefault();
                return Json(customers, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult checkCustomerN(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            bool DisApplicable = false;
            bool FD1Applicable = false;
            bool FD2Applicable = false;
            bool FD3Applicable = false;
            bool FD4Applicable = false;

            var getFurtherDiscount = db.FurtherDiscounts.Where(p => (p.CompanyId == companyid)).FirstOrDefault();
            if (getFurtherDiscount != null)
            {

                DisApplicable = (bool)getFurtherDiscount.Discount;
                FD1Applicable = (bool)getFurtherDiscount.FD1;
                FD2Applicable = (bool)getFurtherDiscount.FD2;
                FD3Applicable = (bool)getFurtherDiscount.FD3;
                FD4Applicable = (bool)getFurtherDiscount.FD4;

            }
            if (Branchid == 0)
            {
                var customers = db.Customers.Where(p => p.Name == query && (p.CompanyId == companyid)).Select(p => new { Id = p.Id, Name = p.Name, TaxId = p.TaxId, Code = p.Code, CurrencyId = p.CurrencyId, CurrencyPurchaseRate = p.CurrencyRate.PurchaseRate, CurrencySaleRate = p.CurrencyRate.SellRate, CurrencyCode = p.CurrencyRate.Currency.ISO_4217, CurrencyName = p.CurrencyRate.Currency.Currency1, Discount = p.Discount, DelAddressName = p.DelAddressName, DelSuburb = p.DelSuburb, DelCity = p.DelCity, DelStateRegion = p.DelStateRegion, DelCountry = p.DelCountry, DelPostalCode = p.DelPostalCode, ContactName = p.ContactName, ContactEmail = p.ContactEmail, ContactMobile = p.ContactMobile, IsFDCompounded = p.IsFDCompounded, DisApplicable = DisApplicable, FD1Applicable = FD1Applicable, FD2Applicable = FD2Applicable, FD3Applicable = FD3Applicable, FD4Applicable = FD4Applicable, FD1 = p.FD1, FD2 = p.FD2, FD3 = p.FD3, FD4 = p.FD4 }).FirstOrDefault();
                return Json(customers, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var customers = db.Customers.Where(p => p.Name == query && (p.CompanyId == companyid)).Select(p => new { Id = p.Id, Name = p.Name, TaxId = p.TaxId, Code = p.Code, CurrencyId = p.CurrencyId, CurrencyPurchaseRate = p.CurrencyRate.PurchaseRate, CurrencySaleRate = p.CurrencyRate.SellRate, CurrencyCode = p.CurrencyRate.Currency.ISO_4217, CurrencyName = p.CurrencyRate.Currency.Currency1, Discount = p.Discount, DelAddressName = p.DelAddressName, DelSuburb = p.DelSuburb, DelCity = p.DelCity, DelStateRegion = p.DelStateRegion, DelCountry = p.DelCountry, DelPostalCode = p.DelPostalCode, ContactName = p.ContactName, ContactEmail = p.ContactEmail, ContactMobile = p.ContactMobile, IsFDCompounded = p.IsFDCompounded, DisApplicable = DisApplicable, FD1Applicable = FD1Applicable, FD2Applicable = FD2Applicable, FD3Applicable = FD3Applicable, FD4Applicable = FD4Applicable, FD1 = p.FD1, FD2 = p.FD2, FD3 = p.FD3, FD4 = p.FD4 }).FirstOrDefault();
                return Json(customers, JsonRequestBehavior.AllowGet);
            }
        }

    }
}

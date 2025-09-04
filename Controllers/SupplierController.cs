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
    public class SupplierController : Controller
    {
        //
        // GET: /Supplier/
        InventoryEntities db = new InventoryEntities();
        private MasterClasses mc = new MasterClasses();
        TaxRepository taxobj = new TaxRepository();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ShowAllSupplier(string Msg, string Err)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }
            int companyid = Convert.ToInt32(Session["companyid"]);
            int userid = Convert.ToInt32(Session["userid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            List<Supplier> supplier = new List<Supplier>();
            if(Branchid==0)
                 supplier = db.Suppliers.Where(d => d.UserId == userid && d.CompanyId == companyid).OrderBy(d=>d.Name).ToList();
            else
                supplier = db.Suppliers.Where(d => d.UserId == userid && d.CompanyId == companyid && d.BranchId==Branchid).OrderBy(d => d.Name).ToList();
            return View(supplier);
        }

        [HttpGet]
        public ActionResult CreateSupplier()
        {

            long companyid = Convert.ToInt32(Session["companyid"]);

            var currency = db.Companies.Where(d => d.Id == companyid).FirstOrDefault();


            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long userid = Convert.ToInt32(Session["userid"]);
            List<LedgerMaster> pendingSuppliers = new List<LedgerMaster>();

            var suppliers = db.Suppliers.Select(s => s.LId).ToList();
            var ledgerM = new LedgerMaster();
            ledgerM.LID = 0;
            ledgerM.ledgerName = "---------Select----------";

            pendingSuppliers = db.LedgerMasters.Where(d => d.parentID != null && d.groupID == 104 && !suppliers.Contains(d.LID)).ToList();
            pendingSuppliers.Add(ledgerM);
            ViewBag.pendingSuppliers = pendingSuppliers.OrderBy(d => d.LID).ToList();
            var currencyrate = db.CurrencyRates.Where(d => d.CompanyId == companyid && d.UserId == userid).Select(d => new { Id = d.Id, Name = d.Currency.ISO_4217 + "(" + d.Currency.Country + "," + d.Currency.Currency1 + ")" }).ToList();
            ViewBag.currency = currencyrate;

            ViewBag.taxrate = db.Taxes.Where(d => (d.UserId == userid && d.CompanyId == companyid) || (d.CompanyId == 0 && d.UserId == 0)).ToList();


            return View();
        }

        public JsonResult CheckSupplierCode(string Id, string Code)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            int id = 0;
            string codes = Code;
            string trimcode = codes.Trim();

            if (String.IsNullOrEmpty(Id) || String.IsNullOrWhiteSpace(Id))
            {

                bool Iscustomer = db.Suppliers.Any(d => d.Code == trimcode && d.CompanyId == companyid && d.BranchId == Branchid);
                return Json(!Iscustomer, JsonRequestBehavior.AllowGet);
            }
            else
            {

                bool Iscustomer = db.Suppliers.Where(d => d.Id != id).Any(d => d.Code == trimcode && d.CompanyId == companyid && d.BranchId == Branchid);
                return Json(Iscustomer, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateSupplier(Supplier supplier)
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
                    int Createdby = Convert.ToInt32(Session["Createdid"]);



                    int fyid = Convert.ToInt32(Session["fid"]);


                    var currencyrate = db.CurrencyRates.Where(d => d.CompanyId == companyid && d.UserId == userid).Select(d => new { Id = d.Id, Name = d.Currency.ISO_4217 + "(" + d.Currency.Country + "," + d.Currency.Currency1 + ")" }).ToList();
                    ViewBag.currency = currencyrate;

                    ViewBag.taxrate = db.Taxes.Where(d => (d.UserId == userid && d.CompanyId == companyid) || (d.CompanyId == 0 && d.UserId == 0)).ToList();
                    List<LedgerMaster> pendingSuppliers = new List<LedgerMaster>();
                    
                    var suppliers = db.Suppliers.Select(s => s.LId).ToList();
                    var ledgerM = new LedgerMaster();
                    ledgerM.LID = 0;
                    ledgerM.ledgerName = "---------Select----------";
                    
                    pendingSuppliers = db.LedgerMasters.Where(d => d.parentID != null && d.groupID == 104 && !suppliers.Contains(d.LID)).ToList();
                    pendingSuppliers.Add(ledgerM);
                    ViewBag.pendingSuppliers = pendingSuppliers.OrderBy(d=>d.LID).ToList();

                    if (supplier.LId == 0)
                    {
                        LedgerMaster ledger = new LedgerMaster();

                        int ledgeridint = 0;
                        string LedgerID = string.Empty;

                        string ledgerid = db.LedgerMasters.Where(l => l.parentID == 6 && l.CompanyId == companyid && l.BranchId == Branchid).OrderByDescending(l => l.LID).Select(l => l.ledgerID).FirstOrDefault();
                        //var sv = db.SalesInvoices.Where(s => s.CustomerId == 4).Select(s => new { s.NO, quantity = s.SalesInvoiceDetails.Sum(q => q.Quantity) }).ToList();
                        if (ledgerid == null)
                        {
                            LedgerID = "10061001";
                        }
                        else
                        {
                            ledgeridint = Convert.ToInt32(ledgerid) + 1;

                            LedgerID = Convert.ToString(ledgeridint);
                        }
                        
                        ledger.ledgerID = LedgerID;
                        ledger.parentID = 6;
                        ledger.ledgerName = supplier.Name;
                        ledger.groupID = 104;
                        ledger.CompanyId = companyid;
                        ledger.fYearID = fyid;
                        ledger.ledgerType = "General";
                        ledger.BranchId = Branchid;
                        ledger.UserId = userid;
                        ledger.CreatedBy = Createdby;
                        ledger.CreatedOn = DateTime.Now;
                        db.LedgerMasters.Add(ledger);
                        db.SaveChanges();

                        if (supplier.OpeningBalance == null)
                            supplier.OpeningBalance = 0;
                        var openingBalance = new OpeningBalance();
                        openingBalance.BranchId = Branchid;
                        openingBalance.CompanyId = companyid;
                        openingBalance.fYearID = fyid;
                        openingBalance.ledgerID = ledger.LID;
                        openingBalance.openingBal = (decimal)supplier.OpeningBalance;
                        openingBalance.UserId = userid;
                        openingBalance.CreatedBy = Createdby;
                        openingBalance.CreatedOn = DateTime.Now;
                        db.OpeningBalances.Add(openingBalance);
                        db.SaveChanges();

                        supplier.LId = ledger.LID;
                    }

                 //   long ledid = taxobj.GetLedgerInsertId(ledger);
                    var getPrefix = db.Prefixes.Where(d => d.CompanyId == companyid && d.DefaultPrefix == "SUP").FirstOrDefault();
                    if (getPrefix.SetPrefix != null)
                        supplier.Code = getPrefix.SetPrefix + supplier.Code;
                    else
                        supplier.Code = getPrefix.DefaultPrefix + supplier.Code;

                    var checkDuplicate = db.Suppliers.Any(d => d.Code == supplier.Code && d.CompanyId == companyid);
                    if (checkDuplicate)
                    {
                        ViewBag.Error = "Supplier Code Already Exists.";
                        return View(supplier);
                    }

                    var checkDuplicateName = db.Suppliers.Any(d => d.Name == supplier.Name && d.CompanyId == companyid);
                    if (checkDuplicate)
                    {
                        ViewBag.Error = "Supplier Name Already Exists.";
                        return View(supplier);
                    }
                    //try
                    //{
                    //int maxcount = 0;
                    //var cust = db.Suppliers.Where(d => d.CompanyId == companyid && d.BranchId == Branchid).Count();
                    //if (cust > 0)
                    //{
                    //    var getmax = db.Suppliers.Where(d => d.CompanyId == companyid && d.BranchId == Branchid).OrderByDescending(d => d.Id).Select(d => d.Code).FirstOrDefault();
                    //    var maxstr = getmax.Substring(getmax.Length - 4);
                    //    maxcount = Convert.ToInt32(maxstr);
                    //}
                    //var getPrefix = db.Prefixes.Where(d => d.CompanyId == companyid  && d.DefaultPrefix == "SUP").FirstOrDefault();
                    //if (cust == 0)
                    //{
                    //    if (getPrefix.SetPrefix != null)
                    //        supplier.Code = getPrefix.SetPrefix + "1001";
                    //    else
                    //        supplier.Code = getPrefix.DefaultPrefix + "1001";
                    //}
                    //else
                    //{
                    //    maxcount = maxcount + 1;
                    //    if (getPrefix.SetPrefix != null)
                    //        supplier.Code = getPrefix.SetPrefix + maxcount;
                    //    else
                    //        supplier.Code = getPrefix.DefaultPrefix + maxcount;
                    //}
                    //}
                    //catch
                    //{
                    //}
                    if (!(supplier.PCode == "" || supplier.PCode == null))
                    {
                        supplier.PCode = supplier.PCode.Trim().ToUpper();
                       
                        if (supplier.PCode == "")
                        {
                            supplier.PId = null;
                        }
                        else
                        {
                            var getParentId = db.Suppliers.Where(d => d.CompanyId == companyid && d.BranchId == Branchid && d.Code.ToUpper() == supplier.PCode).Select(d => d.Id).FirstOrDefault();
                            if (getParentId != 0)
                                supplier.PId = getParentId;
                            else
                            {
                                ViewBag.Error = "Parent Code Not Found.";
                                return View(supplier);
                            }
                        }
                    }
                    else
                    {
                        supplier.PId = null;
                    }
                    
                    supplier.UserId = userid;
                    supplier.CompanyId = companyid;
                    supplier.BranchId = Branchid;
                    supplier.CreatedBy = Createdby;
                    supplier.CreatedOn = DateTime.Now;
                    db.Suppliers.Add(supplier);
                    db.SaveChanges();
                    scope.Complete();

                    return RedirectToAction("ShowAllSupplier", new { Msg = "Data Saved Successfully...." });
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
                    return RedirectToAction("ShowAllSupplier", new { Err = "Please Try Again !...." });

                }
                catch (DbUpdateException ex) //--------Databse Error Throw--------//
                {
                    UpdateException updateException = (UpdateException)ex.InnerException;
                    SqlException sqlException = (SqlException)updateException.InnerException;

                    foreach (SqlError error in sqlException.Errors)
                    {
                        Response.Write("- Property:" + error.Number + ", Error: " + error.Message);

                    }
                    return RedirectToAction("ShowAllSupplier", new { Err = "Please Try Again !...." });
                }
                catch (DataException)
                {
                    //Log the error (add a variable name after DataException)
                    ViewBag.Error = "Error:Data  not Saved Successfully.......";
                    return RedirectToAction("ShowAllSupplier", new { Err = "Please Try Again !...." });

                }
                catch
                {
                    return RedirectToAction("ShowAllSupplier", new { Err = "Please Try Again !...." });
                }
            }
        }



        [HttpGet]
        public ActionResult EditSupplier(int id)
        {

            int companyid = Convert.ToInt32(Session["companyid"]);
            int userid = Convert.ToInt32(Session["userid"]);

            long Branchid = Convert.ToInt64(Session["BranchId"]);




            var supplier = db.Suppliers.SingleOrDefault(d => d.Id == id);
            try
            {


                var currencyrate = db.CurrencyRates.Where(d => d.CompanyId == companyid && d.UserId == userid).Select(d => new { Id = d.Id, Name = d.Currency.ISO_4217 + "(" + d.Currency.Country + "," + d.Currency.Currency1 + ")" }).ToList();
                ViewBag.currency = currencyrate;

                ViewBag.taxrate = db.Taxes.Where(d => d.CompanyId == companyid || (d.CompanyId == 0 && d.BranchId == 0)).ToList();

            }
            catch { }
            return View(supplier);
        }


        [HttpPost]
        public ActionResult EditSupplier(Supplier supplier)
        {
            try
            {
                int Createdby = Convert.ToInt32(Session["Createdid"]);
                int companyid = Convert.ToInt32(Session["companyid"]);
                int userid = Convert.ToInt32(Session["userid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                int Fyid = Convert.ToInt32(Session["fid"]);

                var currencyrate = db.CurrencyRates.Where(d => d.CompanyId == companyid && d.UserId == userid).Select(d => new { Id = d.Id, Name = d.Currency.ISO_4217 + "(" + d.Currency.Country + "," + d.Currency.Currency1 + ")" }).ToList();
                ViewBag.currency = currencyrate;

                ViewBag.taxrate = db.Taxes.Where(d => d.CompanyId == companyid  || (d.CompanyId == 0 && d.BranchId == 0)).ToList();

                var ledger = db.LedgerMasters.Find(supplier.LId);
                ledger.ledgerName = supplier.Name;
                ledger.ModifiedBy = Createdby;
                ledger.ModifiedOn = DateTime.Now;
                var supplierdet = db.Suppliers.Find(supplier.Id);
                if (supplier.PCode != supplierdet.PCode)
                {
                    if (!(supplier.PCode == "" || supplier.PCode == null))
                    {
                        supplier.PCode = supplier.PCode.Trim().ToUpper();
                        if (supplier.PCode == "")
                        {
                            supplier.PId = null;
                        }
                        else
                        {
                            var getParentId = db.Suppliers.Where(d => d.CompanyId == companyid && d.BranchId == Branchid && d.Code.ToUpper() == supplier.PCode).Select(d => d.Id).FirstOrDefault();
                            if (getParentId != 0)
                                supplier.PId = getParentId;
                            else
                            {
                                ViewBag.Error = "Parent Code Not Found.";
                                return View(supplier);
                            }
                        }
                    }
                    else
                    {
                        supplier.PId = null;
                    }
                }
                if (supplier.OpeningBalance == null)
                    supplier.OpeningBalance = 0;
                var openingBalance = db.OpeningBalances.Where(o => o.ledgerID == supplier.LId && o.fYearID == Fyid).OrderByDescending(o => o.Id).FirstOrDefault();
                openingBalance.openingBal = (decimal)supplier.OpeningBalance;
                openingBalance.ModifiedBy = Createdby;
                openingBalance.ModifiedOn = DateTime.Now;

               
                db.Entry(supplierdet).CurrentValues.SetValues(supplier);
                db.SaveChanges();

                return RedirectToAction("ShowAllSupplier", new { Msg = "Data Saved Successfully...." });
            }
            catch
            {
                return RedirectToAction("ShowAllSupplier", new { Msg = "Data cannot be saved successfully...." });
            }
        }





        [HttpGet]
        public ActionResult Canceledit()
        {
            return RedirectToAction("ShowAllSupplier");

        }


        [HttpGet]
        public ActionResult DeleteSupplier(Supplier supplier)
        {
            try
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);

                var resultfind = db.PurchaseInvoices.Where(d => d.CompanyId == companyid && d.BranchId == Branchid && d.SupplierId == supplier.Id).Count();
                if (resultfind == 0)
                {

                    var result = db.Suppliers.Where(d => d.CompanyId == companyid && d.BranchId == Branchid && d.Id == supplier.Id).FirstOrDefault();
                   
                    db.Suppliers.Remove(result);
                    var ledger = db.LedgerMasters.Find(result.LId);
                    if(ledger != null)
                        db.LedgerMasters.Remove(ledger);
                    var openingBalance = db.OpeningBalances.Where(d => d.ledgerID == result.LId).ToList();
                    foreach(var opening in openingBalance)
                    {
                        db.OpeningBalances.Remove(opening);
                    }
                    db.SaveChanges();
                    return RedirectToAction("ShowAllSupplier", new { Msg = "Row deleted Successfully...." });
                }
                else
                {
                    return RedirectToAction("ShowAllSupplier", new { Err = "Row cannot be deleted because it is used in transaction...." });
                }


            }
            catch
            {
                return RedirectToAction("ShowAllSupplier", new { Err = InventoryMessage.InsertError });
            }
        }




        [HttpGet]
        public ActionResult SupplierDetails(Supplier supplier)
        {


            int userid = Convert.ToInt32(Session["userid"]);

            var result = db.Suppliers.SingleOrDefault(d => d.UserId == userid && d.Id == supplier.Id);
            return View(result);

        }


        

        [HttpPost]
        public JsonResult getSupplier(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            if (Branchid == 0)
            {
                var suppliers = db.Suppliers.Where(p => p.Code.Contains(query) && (p.CompanyId == companyid)).Select(p => new { Code = p.Code, Id = p.Id }).ToList();
                return Json(suppliers, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var suppliers = db.Suppliers.Where(p => p.Code.Contains(query) && (p.CompanyId == companyid && p.BranchId == Branchid)).Select(p => new { Code = p.Code, Id = p.Id }).ToList();
                return Json(suppliers, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public JsonResult getSupplierN(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            if (Branchid == 0)
            {
                var suppliers = db.Suppliers.Where(p => p.Name.Contains(query) && (p.CompanyId == companyid)).Select(p => new { Name = p.Name, Id = p.Id }).ToList();
                return Json(suppliers, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var suppliers = db.Suppliers.Where(p => p.Name.Contains(query) && (p.CompanyId == companyid && p.BranchId == Branchid) ).Select(p => new { Name = p.Name, Id = p.Id }).ToList();
                return Json(suppliers, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult getSelectedSupplier(int id)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            if (Branchid == 0)
            {
                var suppliers = db.Suppliers.Where(p => p.Id == id && (p.CompanyId == companyid)).Select(p => new { Name = p.Name, TaxId = p.TaxId, Code = p.Code }).FirstOrDefault();
                return Json(suppliers, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var suppliers = db.Suppliers.Where(p => p.Id == id && (p.CompanyId == companyid && p.BranchId == Branchid) ).Select(p => new { Name = p.Name, TaxId = p.TaxId, Code = p.Code }).FirstOrDefault();
                return Json(suppliers, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult checkSupplier(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            if (Branchid == 0)
            {
                var supplier = db.Suppliers.Where(p => p.Code == query && (p.CompanyId == companyid )).Select(p => new { Name = p.Name, Code = p.Code, CurrencyId = p.CurrencyId, CurrencyPurchaseRate = p.CurrencyRate.PurchaseRate, CurrencySaleRate = p.CurrencyRate.SellRate, CurrencyCode = p.CurrencyRate.Currency.ISO_4217, CurrencyName = p.CurrencyRate.Currency.Currency1, PoAddressName = p.PoAddressName, PoCity = p.PoCity, PoSuburb = p.PoSuburb, PoStateRegion = p.PoStateRegion, PoCountry = p.PoCountry, PoPostalCode = p.PoPostalCode, ContactMobile = p.ContactMobile, ContactName = p.ContactName, ContactPhone = p.ContactPhone, ContactOfficePhone = p.ContactOfficePhone }).FirstOrDefault();
                return Json(supplier, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var supplier = db.Suppliers.Where(p => p.Code == query && (p.CompanyId == companyid && p.BranchId == Branchid)).Select(p => new { Name = p.Name, Code = p.Code, CurrencyId = p.CurrencyId, CurrencyPurchaseRate = p.CurrencyRate.PurchaseRate, CurrencySaleRate = p.CurrencyRate.SellRate, CurrencyCode = p.CurrencyRate.Currency.ISO_4217, CurrencyName = p.CurrencyRate.Currency.Currency1, PoAddressName = p.PoAddressName, PoCity = p.PoCity, PoSuburb = p.PoSuburb, PoStateRegion = p.PoStateRegion, PoCountry = p.PoCountry, PoPostalCode = p.PoPostalCode, ContactMobile = p.ContactMobile, ContactName = p.ContactName, ContactPhone = p.ContactPhone, ContactOfficePhone = p.ContactOfficePhone }).FirstOrDefault();
                return Json(supplier, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult checkSupplierN(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            if (Branchid == 0)
            {
                var supplier = db.Suppliers.Where(p => p.Name == query && (p.CompanyId == companyid )).Select(p => new { Name = p.Name, Code = p.Code, CurrencyId = p.CurrencyId, CurrencyPurchaseRate = p.CurrencyRate.PurchaseRate, CurrencySaleRate = p.CurrencyRate.SellRate, CurrencyCode = p.CurrencyRate.Currency.ISO_4217, CurrencyName = p.CurrencyRate.Currency.Currency1, PoAddressName = p.PoAddressName, PoCity = p.PoCity, PoSuburb = p.PoSuburb, PoStateRegion = p.PoStateRegion, PoCountry = p.PoCountry, PoPostalCode = p.PoPostalCode, ContactMobile = p.ContactMobile, ContactName = p.ContactName, ContactPhone = p.ContactPhone, ContactOfficePhone = p.ContactOfficePhone }).FirstOrDefault();
                return Json(supplier, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var supplier = db.Suppliers.Where(p => p.Name == query && (p.CompanyId == companyid && p.BranchId == Branchid) ).Select(p => new { Name = p.Name, Code = p.Code, CurrencyId = p.CurrencyId, CurrencyPurchaseRate = p.CurrencyRate.PurchaseRate, CurrencySaleRate = p.CurrencyRate.SellRate, CurrencyCode = p.CurrencyRate.Currency.ISO_4217, CurrencyName = p.CurrencyRate.Currency.Currency1, PoAddressName = p.PoAddressName, PoCity = p.PoCity, PoSuburb = p.PoSuburb, PoStateRegion = p.PoStateRegion, PoCountry = p.PoCountry, PoPostalCode = p.PoPostalCode, ContactMobile = p.ContactMobile, ContactName = p.ContactName, ContactPhone = p.ContactPhone, ContactOfficePhone = p.ContactOfficePhone }).FirstOrDefault();
                return Json(supplier, JsonRequestBehavior.AllowGet);
            }
        }
    }
}

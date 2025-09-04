using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Globalization;
using System.Data.SqlClient;
using System.Data;
using System.Data.Entity.Validation;
using System.Transactions;
using Rotativa;

namespace XenERP.Controllers
{
    public class SalesReceiptController : Controller
    {

        private InventoryEntities db = new InventoryEntities();
        private MasterClasses mc = new MasterClasses();
        private TransactionClasses tc = new TransactionClasses();


        #region Master



        public ActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public ActionResult CreateSalesReceipt(string Msg,string Err,long? id = 0)
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

            ViewBag.compid = companyid;
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            int Createdby = Convert.ToInt32(Session["Createdid"]);



            var CustomerList = from cus in db.Customers
                               where cus.CompanyId == companyid && cus.BranchId == Branchid && cus.UserId == userid
                               select new
                               {
                                   Id = cus.Id,
                                   Name = cus.Code + "(" + cus.Name + ")"

                               };



            ViewBag.Bank = db.Banks.Where(d => d.BranchId == Branchid && d.CompanyId == companyid).Distinct();


            ViewBag.Customer = CustomerList.ToList().OrderBy(d => d.Name);
           
                return View();          

        }


        [HttpPost]
        public ActionResult CreateSalesReceipt(ReceiptPayment receive, FormCollection collection)
        {
            var culture = Session["DateCulture"].ToString();
           System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            ReceiptPayment payment = new ReceiptPayment();

            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            int fyid = Convert.ToInt32(Session["fid"]);


            //int Invoiceid = Convert.ToInt32(collection["Invid"]);

            //string Invoiceno = collection["Invoiceno"];

            var scope = new TransactionScope(
                         TransactionScopeOption.RequiresNew,
                         new TransactionOptions()
                         {
                             IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                         }
                     );

            using (scope)
            {


                try
                {


                    var recpayment = new ReceiptPayment();

                    recpayment.RPdate = DateTime.Today;
                    recpayment.RPDatetime = DateTime.Now;

                    recpayment.fYearId = fyid;
                    recpayment.BranchId = Branchid;

                    recpayment.CompanyId = companyid;
                    recpayment.UserId = userid;

                    if (receive.RPCashAmount > 0)
                    {
                        long CashId = db.LedgerMasters.Where(l => l.ledgerName == InventoryConst.cns_PayMode_Cash && l.BranchId == Branchid && l.CompanyId == companyid).Select(l => l.LID).FirstOrDefault();

                        recpayment.RPCashId = Convert.ToInt32(CashId);
                        recpayment.RPCashAmount = receive.RPCashAmount;
                    }
                    else
                    {

                        receive.RPCashId = 0;
                        receive.RPCashAmount = 0;


                    }


                    if (receive.RPBankAmount > 0)
                    {
                        recpayment.RPBankId = receive.RPBankId;
                        recpayment.RPBankAmount = receive.RPBankAmount;
                        recpayment.chequeNo = receive.chequeNo;
                        recpayment.chequeDate = Convert.ToDateTime(collection["chequeDate"]);


                    }
                    else
                    {
                        recpayment.RPBankId = 0;
                        recpayment.RPBankAmount = 0;
                    }



                  // var sales = db.SalesInvoices.Where(d => d.Id == Invoiceid).FirstOrDefault();


                    decimal salesrate = Convert.ToInt32(collection["Currencyrate"]);

                    double cardamt = Convert.ToDouble(collection["cardamount"]);



                    if (cardamt == 0.0)
                    {

                        cardamt = 0.0;
                    }

                    if (cardamt > 0)
                    {
                        recpayment.CardName = collection["CardName"];
                        recpayment.CreditCardNo = collection["CreditCardNo"];
                        recpayment.ExpirayDate = collection["month"] + "/" + collection["year"];
                        recpayment.TotalAmount = Convert.ToDecimal(collection["cardamount"]);
                        recpayment.CreditCardAmt = Convert.ToDecimal(collection["cardamount"]);
                        recpayment.CurrencyAmount = recpayment.TotalAmount * salesrate;


                    }


                    if (collection["Totalpayment"] == "0.0")
                    {

                        recpayment.TotalAmount = 0;
                    }
                    else
                    {

                        recpayment.TotalAmount = Convert.ToInt32(collection["Totalpayment"]);
                        recpayment.CurrencyAmount = recpayment.TotalAmount * salesrate;

                    }


                    recpayment.transactionType = "Sales Invoice";

                    recpayment.RPType = "Receive";



                    var vouch = db.ReceiptPayments.Where(d => d.transactionType == recpayment.transactionType && d.CompanyId == companyid && d.UserId == userid).Max(v => (int?)v.VoucherNo) ?? 0;

                    recpayment.VoucherNo = vouch + 1;



                    int custid = Convert.ToInt32(collection["CustomerId"]);

                    var cusid = db.Customers.Where(d => d.Id == custid).FirstOrDefault();


                    int transNo = GenRandom.GetRandom();
                    recpayment.ledgerId = Convert.ToInt32(cusid.LId);
                    recpayment.transactionNo = custid.ToString();
                    recpayment.Remarks = receive.Remarks;
                    recpayment.PaymentDate = DateTime.Parse(collection["PaymentDate"]);
                    db.ReceiptPayments.Add(recpayment);
                    db.SaveChanges();
                    scope.Complete();
                    return RedirectToAction("CreateSalesReceipt", new { Msg = "   Payment Done Successfully.." });
                }




                catch (DbEntityValidationException e)
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
                    return RedirectToAction("CreateSalesReceipt", new { Err = "Error:Please Try Again!"});

                }
                catch (System.Data.DataException)
                {
                    //Log the error (add a variable name after DataException)
                    ViewBag.Error = "Error:Data  not Saved Successfully.......";
                    return RedirectToAction("CreateSalesReceipt", new { Err = "Error:Please Try Again !" });

                }
                catch (Exception exp)
                {
                    return RedirectToAction("CreateSalesReceipt", new { Err = "Error:Please Try Again !" });

                }
            }

          



        }



        [HttpPost]
        public JsonResult getCustomer(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            var customers = db.Customers.Where(p => p.Code.Contains(query) || p.Name.Contains(query)  && p.CompanyId == companyid && p.BranchId == Branchid).Select(p => new { Code = p.Code, Id = p.Id,Name=p.Name }).ToList();
            return Json(customers, JsonRequestBehavior.AllowGet);
        }

        public JsonResult checkCustomer(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            var customers = db.Customers.Where(s => (s.Code == query || s.Name==query) && s.CompanyId == companyid && s.BranchId == Branchid).Select(p => new { Name = p.Name, TaxId = p.TaxId, Code = p.Code, CurrencyId = p.CurrencyId, CurrencyPurchaseRate = p.CurrencyRate.PurchaseRate, CurrencySaleRate = p.CurrencyRate.SellRate, CurrencyCode = p.CurrencyRate.Currency.ISO_4217, CurrencyName = p.CurrencyRate.Currency.Currency1, Discount = p.Discount }).FirstOrDefault();
            return Json(customers, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getSelectedCustomer(int id)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            var customers = db.Customers.Where(p => p.Id == id && p.CompanyId == companyid && p.BranchId == Branchid).Select(p => new { Name = p.Name, City = p.City,Country=p.Country,State=p.StateRegion, Code = p.Code, CurrencyId = p.CurrencyId, CurrencyPurchaseRate = p.CurrencyRate.PurchaseRate, CurrencySaleRate = p.CurrencyRate.SellRate, CurrencyCode = p.CurrencyRate.Currency.ISO_4217, CurrencyName = p.CurrencyRate.Currency.Currency1, }).FirstOrDefault();
            return Json(customers, JsonRequestBehavior.AllowGet);
        }

         public JsonResult GetCustomerInvoice(int id,decimal amount)
        {
            decimal? remaining = 0;

             List<CustomerwiseInvoice> inv=new List<CustomerwiseInvoice>();

            string cusid = Convert.ToString(id);
            decimal? recipt = db.ReceiptPayments.Where(d => d.transactionNo == cusid).Sum(d => d.TotalAmount);
            var result = db.SalesInvoices.Where(d=>d.CustomerId==id).OrderBy(d=>d.Id).ToList();

            if (amount != 0)
            {
               remaining = recipt+amount;
            }

             else
            {
                remaining = recipt;
            
            }
            foreach (var invdetls in result)
            {
                
                if (remaining >= invdetls.BCGrandTotal)
                {
                    remaining -= invdetls.BCGrandTotal;
                    if (remaining < invdetls.BCGrandTotal)
                    {
                        var cusinv = new CustomerwiseInvoice();
                        cusinv.InvAmount = invdetls.BCGrandTotal;
                        cusinv.PaidAmount = (decimal)remaining;
                        cusinv.DueAmount = cusinv.InvAmount - cusinv.PaidAmount;
                        cusinv.Date = invdetls.Date;
                        cusinv.InvoiceNo = invdetls.NO;
                        inv.Add(cusinv);
                    }
                }
                else
                {
                    if (remaining > 0)
                    {
                        var cusinv = new CustomerwiseInvoice();
                        cusinv.InvAmount = invdetls.BCGrandTotal;
                        cusinv.PaidAmount = (decimal)remaining;
                        cusinv.DueAmount = cusinv.InvAmount - cusinv.PaidAmount;
                        cusinv.Date = invdetls.Date;
                        cusinv.InvoiceNo = invdetls.NO;
                        inv.Add(cusinv);
                        remaining = 0;
                    }
                    else
                    {
                        var cusinv = new CustomerwiseInvoice();
                        cusinv.InvAmount = invdetls.BCGrandTotal;
                        cusinv.PaidAmount = 0;
                        cusinv.DueAmount = invdetls.BCGrandTotal;
                        cusinv.Date = invdetls.Date;
                        cusinv.InvoiceNo = invdetls.NO;
                        inv.Add(cusinv);
                    }
                    
                    
                }

            }


            return Json(inv, JsonRequestBehavior.AllowGet);
        }

         public JsonResult GetCustomerpaid(string id)
         {
             var result = db.ReceiptPayments.Where(d=>d.transactionNo==id).Sum(d=>d.TotalAmount);

             return Json(result, JsonRequestBehavior.AllowGet);
         }

        // public JsonResult GetPaymentDueHistory(long id)
        //{
        //    var gettotalInvoiceAmount=db.SalesInvoices.Where(s=>s.CompanyId==
        //    var result = db.ReceiptPayments.Where(d => d.transactionNo == id).Sum(d => d.TotalAmount);

        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        #endregion



        #region PDF Email



         public ActionResult CreteSalesPDF(int id, int comids)
         {

         //    int companyid = Convert.ToInt32(Session["companyid"]);
             var company = db.Companies.Where(c => c.Id == comids).ToList();
               var companyname = db.Companies.Where(c => c.Id == comids).FirstOrDefault();
             ViewBag.company = company;

             ViewBag.companyName = companyname.Name;
             var customer = db.Customers.Where(c => c.Id == id).ToList();
             ViewBag.customer = customer;
             ViewBag.id = id;




             decimal? remaining = 0;

             List<CustomerwiseInvoice> inv = new List<CustomerwiseInvoice>();

             string cusid = Convert.ToString(id);
             decimal? recipt = db.ReceiptPayments.Where(d => d.transactionNo == cusid).Sum(d => d.TotalAmount);
             var result = db.SalesInvoices.Where(d => d.CustomerId == id).OrderBy(d => d.Id).ToList();

         
                 remaining = recipt;

          
             foreach (var invdetls in result)
             {

                 if (remaining >= invdetls.BCGrandTotal)
                 {
                     remaining -= invdetls.BCGrandTotal;
                     if (remaining < invdetls.BCGrandTotal)
                     {
                         var cusinv = new CustomerwiseInvoice();
                         cusinv.InvAmount = invdetls.BCGrandTotal;
                         cusinv.PaidAmount = (decimal)remaining;
                         cusinv.DueAmount = cusinv.InvAmount - cusinv.PaidAmount;
                         cusinv.Date = invdetls.Date;
                         cusinv.InvoiceNo = invdetls.NO;
                         inv.Add(cusinv);
                     }
                 }
                 else
                 {
                     if (remaining > 0)
                     {
                         var cusinv = new CustomerwiseInvoice();
                         cusinv.InvAmount = invdetls.BCGrandTotal;
                         cusinv.PaidAmount = (decimal)remaining;
                         cusinv.DueAmount = cusinv.InvAmount - cusinv.PaidAmount;
                         cusinv.Date = invdetls.Date;
                         cusinv.InvoiceNo = invdetls.NO;
                         inv.Add(cusinv);
                         remaining = 0;
                     }
                     else
                     {
                         var cusinv = new CustomerwiseInvoice();
                         cusinv.InvAmount = invdetls.BCGrandTotal;
                         cusinv.PaidAmount = 0;
                         cusinv.DueAmount = invdetls.BCGrandTotal;
                         cusinv.Date = invdetls.Date;
                         cusinv.InvoiceNo = invdetls.NO;
                         inv.Add(cusinv);
                     }


                 }

             }


             return View(inv);
            // return View();
         
         }



        public ActionResult PrintSalesPDF(int id,int comids)
        {


            return new ActionAsPdf(
                           "CreteSalesPDF", new { id = id, comids = comids }) { FileName = "SalesReceivePayment.pdf" }; 
        }



    

        #endregion


    }
}

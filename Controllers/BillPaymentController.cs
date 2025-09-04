using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Data.Entity.Validation;
using System.Transactions;

namespace XenERP.Controllers
{
    public class BillPaymentController : Controller
    {
        //
        // GET: /BillPayment/

        InventoryEntities db = new InventoryEntities();


        public ActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public ActionResult CreateBillPayment(string Msg, string Err,int id)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }


            try
            {

                var purchase = db.PurchaseInvoices.Where(d => d.Id == id).FirstOrDefault();


                try
                {
                    string idd = Convert.ToString(id);
                    var lesspayment = db.ReceiptPayments.Where(d => d.transactionNo == idd && d.transactionType == "Purchase Invoice").ToList();

                    ViewBag.paymentless = lesspayment;

                    decimal? amountpaid=0;

                    foreach(var paid in lesspayment)
                    {
                        amountpaid=amountpaid+paid.TotalAmount;
                    }


                    decimal? Dueamount = purchase.GrandTotal - amountpaid;

                    ViewBag.Amountdue = Dueamount;

                }
                catch { }


                ViewBag.subtotal = purchase.SubTotal;
                ViewBag.taxtotal = purchase.TaxProduct;
                ViewBag.taxtotal = purchase.TaxOther;
                ViewBag.Addition = purchase.TotalAddAmount;
                ViewBag.deduction = purchase.TotalDeductAmount;
                ViewBag.grandtotal = purchase.GrandTotal;
                var currency = db.Currencies.Where(c => c.CurrencyId == purchase.CurrencyId).FirstOrDefault();
                ViewBag.currencyname = currency.ISO_4217;


                ViewBag.id = id;
                ViewBag.invoice = purchase.NO;
                ViewBag.duedate = purchase.DueDate;
                ViewBag.invdate = purchase.InvoiceDate;
                ViewBag.Nettotal = purchase.SubTotal;


                var purchasedetails = db.PurchaseInvoiceDetails.Where(d => d.PurchaseInvoiceId == id);
                ViewBag.PurDetails = purchasedetails;




                int compid=Convert.ToInt32(purchase.CompanyId);

                var company = db.Companies.Where(c => c.Id ==compid).ToList();
                ViewBag.company = company;               
                var supplier = db.Suppliers.Where(c => c.Id == purchase.SupplierId).ToList();
                ViewBag.supplierdet = supplier;




                var paymentdone = db.ReceiptPayments.Where(d => d.transactionNo == id.ToString() && d.transactionType == "Purchase Invoice");

                ViewBag.paymentclear = paymentdone;



                int companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                int userid = Convert.ToInt32(Session["userid"]);
                int fyid = Convert.ToInt32(Session["fid"]);



                var rec = new ReceiptPayment();

                var Ledger = from grp in db.GroupMasters
                             join led in db.LedgerMasters
                             on grp.groupID equals led.groupID
                             where grp.SortOrder.Contains("003002") || grp.SortOrder.Contains("004002")
                             select new Ledger
                             {
                                 Id = led.LID,
                                 Name = led.ledgerID + "-" + led.ledgerName
                             };


                ViewBag.ledger = Ledger;

                ViewBag.Bank = db.Banks.Where(d => d.BranchId == Branchid && d.CompanyId == companyid);
            }
            catch { }



            return View();

        }


        [HttpPost]
        public ActionResult CreateBillPayment(ReceiptPayment receive, FormCollection collection)
        {
            ReceiptPayment payment = new ReceiptPayment();

            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            int fyid = Convert.ToInt32(Session["fid"]);


            int Invoiceid = Convert.ToInt32(collection["Invid"]);
            //if (Totalpayment > 0)
            //{
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
                        long CashId = db.LedgerMasters.Where(l => l.ledgerName == InventoryConst.cns_PayMode_Cash && l.BranchId == Branchid && l.CompanyId==companyid).Select(l => l.LID).FirstOrDefault();

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



                        var purchase = db.PurchaseInvoices.Where(d => d.Id ==Invoiceid).FirstOrDefault();


                        decimal supcurrate = purchase.Currencyrate;

                        double cardamt = Convert.ToDouble(collection["cardamount"]);



                        if (cardamt == 0.0)
                        {

                             cardamt = 0.0;
                        }

                        if (cardamt>0)
                        {
                            recpayment.CardName = collection["CardName"];
                            recpayment.CreditCardNo = collection["CreditCardNo"];
                            recpayment.ExpirayDate = collection["month"] + "/" + collection["year"];
                            recpayment.TotalAmount =Convert.ToDecimal(collection["cardamount"]);
                            recpayment.CreditCardAmt = Convert.ToDecimal(collection["cardamount"]);
                            recpayment.CurrencyAmount = recpayment.TotalAmount * supcurrate;
                           

                        }



                  
               
                        if (collection["Totalpayment"] == "0.0")
                        {

                            recpayment.TotalAmount = 0;
                        }
                        else
                        {
                          
                            recpayment.TotalAmount = Convert.ToInt32(collection["Totalpayment"]);
                            recpayment.CurrencyAmount = recpayment.TotalAmount * supcurrate;
                           
                        }






                        recpayment.transactionType = "Purchase Invoice";

                        recpayment.RPType = "Payment";

                       

                        var vouch = db.ReceiptPayments.Where(d => d.transactionType == recpayment.transactionType && d.CompanyId == companyid && d.UserId == userid).Max(v => (int?)v.VoucherNo) ?? 0;

                        recpayment.VoucherNo = vouch + 1;

                        int Createdby = Convert.ToInt32(Session["Createdid"]);
                        recpayment.ledgerId = receive.ledgerId;
                        recpayment.transactionNo = Invoiceid.ToString();
                        recpayment.Remarks = receive.Remarks;
                        recpayment.PaymentDate = Convert.ToDateTime(collection["PaymentDate"]);
                        recpayment.CreatedBy = Createdby;
                        db.ReceiptPayments.Add(recpayment);
                       db.SaveChanges();
                        scope.Complete();
                        return RedirectToAction("CreateBillPayment", new { Msg = "Payment Data Saved Successfully..",id=Invoiceid });
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
                        return RedirectToAction("CreateBillPayment", new { Err = "Error:Please Try Again!", id = Invoiceid });

                    }
                    catch (System.Data.DataException)
                    {
                        //Log the error (add a variable name after DataException)
                        ViewBag.Error = "Error:Data  not Saved Successfully.......";
                        return RedirectToAction("CreateBillPayment", new { Err = "Error:Please Try Again !", id = Invoiceid });

                    }
                    catch (Exception exp)
                    {
                        return RedirectToAction("CreateBillPayment", new { Err = "Error:Please Try Again !", id = Invoiceid });

                    }
                }

            //}



            //else
            //{
            //    return View("ReceiveVoucherList");

            //}



        }

       

    }
}

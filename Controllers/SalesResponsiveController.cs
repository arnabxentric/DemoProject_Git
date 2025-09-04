using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;

namespace XenERP.Controllers
{
    public class SalesResponsiveController : Controller
    {
        private InventoryEntities db = new InventoryEntities();
        //
        // GET: /SalesResponsive/

        #region SALES INVOICE

        public ActionResult SalesinvoiceList(string Msg, string Err)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }
            long Createdby = Convert.ToInt32(Session["Createdid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            int Fyid = Convert.ToInt32(Session["fid"]);
            var salesInvoice = db.SalesInvoices.Where(r => r.CreatedBy == Createdby).OrderByDescending(o=>o.InvoiceDate).Take(50).ToList();
            List<SalesInvoiceModelView> list = new List<SalesInvoiceModelView>();
            var salesDetails = db.SalesInvoiceDetails.Where(r => r.SalesInvoice.CreatedBy == Createdby).ToList();
            foreach (var item in salesInvoice)
            {
                SalesInvoiceModelView model = new SalesInvoiceModelView();
                model.CustomerName = item.Customer.Name;
                model.SalesInvoiceId = item.Id;
                model.CustomerId = item.CustomerId;
                model.InvoiceDate = Convert.ToDateTime(item.InvoiceDate).ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                model.NO = item.NO + " (" + Convert.ToString(item.ReferenceNo) + ")";
                model.ReferenceNo = item.ReferenceNo;
                model.Status = (DateTime.Today - item.InvoiceDate).Days > 1 ? "True" : "False";
                List<SalesInvoiceItemDetail> itemList = new List<SalesInvoiceItemDetail>();
                var itemDetails = salesDetails.Where(r => r.SalesInvoiceId == item.Id).ToList();
                foreach (var item1 in itemDetails)
                {
                    SalesInvoiceItemDetail detail = new SalesInvoiceItemDetail();
                    detail.ItemName = item1.Product.Name;
                    detail.ItemId = item1.ItemId;
                    detail.Quantity = item1.Quantity;
                    detail.Rate = item1.Price;
                    detail.TotalPrice = item1.TotalAmount;
                    
                    itemList.Add(detail);
                }
                model.salesInvoiceItemDetails = itemList;
                list.Add(model);
            }

            return View(list);
        }

        public ActionResult CreateSalesInvoice()
        {
            SalesInvoiceModelView model = new SalesInvoiceModelView();
            long Createdby = Convert.ToInt32(Session["Createdid"]);
            ViewBag.customer = db.Customers.Where(r => r.SalesPersonId == Createdby).Select(s => new { Text = s.Name, Value = s.Id }).ToList();
            List<SalesInvoiceItemDetail> listItem = new List<SalesInvoiceItemDetail>();
            listItem.Add(new SalesInvoiceItemDetail { ItemName = "Chicks-Broiler", ItemId = 588, Rate = 0, TotalPrice = 0 });
            listItem.Add(new SalesInvoiceItemDetail { ItemName = "Chicks-RG", ItemId = 948, Rate = 0, TotalPrice = 0 });
            listItem.Add(new SalesInvoiceItemDetail { ItemName = "Chicks-HG", ItemId = 949, Rate = 0, TotalPrice = 0 });


            var mode = new SelectList(new[]
                                         {

                                              new{ID=2,Name="Cash"},
                                              new{ID=1,Name="Credit"},


                                          },
                      "ID", "Name");
            ViewData["mod"] = mode;

            model.salesInvoiceItemDetails = listItem;
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateSalesInvoice(SalesInvoiceModelView model)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                var scope = new System.Transactions.TransactionScope(

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
                        var culture = Session["DateCulture"].ToString();
                        string dateFormat = Session["DateFormat"].ToString();
                        long Createdby = Convert.ToInt32(Session["Createdid"]);
                        long Branchid = Convert.ToInt64(Session["BranchId"]);
                        long companyid = Convert.ToInt64(Session["companyid"]);
                        long userid = Convert.ToInt32(Session["userid"]);
                        int Fyid = Convert.ToInt32(Session["fid"]);


                        string getPrefix = "";
                        int countpo = 1;
                        var fyear = db.FinancialYearMasters.FirstOrDefault(f => f.fYearID == Fyid).Year;
                        var fs = fyear.Substring(2, 2);
                        var es = fyear.Substring(7, 2);
                        fyear = fs + "-" + es;


                        if (db.SalesInvoices.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.WarehouseId == 9 && p.FinancialYearId == Fyid && p.Mode == model.Mode).Count() != 0)
                        {

                            countpo = (int)db.SalesInvoices.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.WarehouseId == 9 && p.FinancialYearId == Fyid && p.Mode == model.Mode).Max(p => p.InvoiceNo) + 1;
                        }

                        if (model.Mode == 1)
                            getPrefix = db.Prefixes.Where(p => p.DefaultPrefix == "SI" && p.CompanyId == companyid && p.BranchId == Branchid).Select(p => p.SetPrefix).FirstOrDefault();
                        else
                            getPrefix = db.Prefixes.Where(p => p.DefaultPrefix == "CS" && p.CompanyId == companyid && p.BranchId == Branchid).Select(p => p.SetPrefix).FirstOrDefault();



                        SalesInvoice invoice = new SalesInvoice();
                        invoice.CustomerId = model.CustomerId;
                        invoice.Date = DateTime.ParseExact(model.Date, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                        invoice.DueDate = DateTime.ParseExact(model.Date, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                        invoice.InvoiceDate = DateTime.ParseExact(model.Date, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                        invoice.ReferenceNo = model.ReferenceNo;
                        invoice.DespatchDate = DateTime.ParseExact(model.Date, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                        invoice.CurrencyId = 84;
                        invoice.TransactionCurrency = 84;
                        invoice.Currencyrate = 1;
                        invoice.FinancialYearId = Fyid;
                        invoice.RecurringSalesId = 0;
                        invoice.CompoundedDis = false;
                        invoice.DisApplicable = false;
                        invoice.FD1Applicable = false;
                        invoice.FD2Applicable = false;
                        invoice.FD3Applicable = false;
                        invoice.FD4Applicable = false;
                        invoice.Dis = 0;
                        invoice.FD1 = 0;
                        invoice.FD2 = 0;
                        invoice.FD3 = 0;
                        invoice.FD4 = 0;
                        invoice.DisAmount = 0;
                        invoice.FD1Amount = 0;
                        invoice.FD2Amount = 0;
                        invoice.FD3Amount = 0;
                        invoice.FD4Amount = 0;
                        invoice.DiscountPerUnit = 0;
                        invoice.SubTotal = model.GrandTotal;
                        invoice.TaxProduct = 0;
                        invoice.TaxOther = 0;
                        invoice.TotalAddAmount = 0;
                        invoice.TotalDeductAmount = 0;
                        invoice.RoundOff = 0;
                        invoice.GrandTotal = model.GrandTotal;
                        invoice.BCGrandTotal = model.GrandTotal;

                        if (getPrefix != null)
                            invoice.NO = getPrefix + "/" + fyear + "/" + countpo;//tc.GenerateCode(fyear, countpo);
                        else
                        {
                            if (model.Mode == 1)
                                invoice.NO = "SI" + "/" + fyear + "/" + countpo; //tc.GenerateCode(fyear, countpo);
                            else
                                invoice.NO = "CS" + "/" + fyear + "/" + countpo;
                        }

                        invoice.InvoiceNo = countpo;
                        invoice.WarehouseId = 9;
                        invoice.LID = 11715;
                        invoice.Mode = model.Mode;

                        tr: string TransNo = Convert.ToString(GenRandom.GetRandom());

                        var trans = db.ReceiptPayments.Where(d => d.transactionNo == TransNo).Select(d => d.Id).FirstOrDefault();

                        if (trans > 0)
                        {
                            goto tr;

                        }
                        invoice.TransNo = TransNo;
                        invoice.CreatedBy = Createdby;
                        invoice.CreatedOn = DateTime.Now;
                        invoice.Status = "Saved";
                        invoice.UserId = 1;
                        invoice.CompanyId = 1;
                        invoice.BranchId = 7;
                        invoice.IsPaid = false;
                        db.SalesInvoices.Add(invoice);
                        db.SaveChanges();

                        foreach (var item in model.salesInvoiceItemDetails)
                        {
                            if (item.ItemId == 0 || item.Quantity <= 0 || item.Rate <= 0)
                            {
                                continue; // Skip invalid items
                            }
                            SalesInvoiceDetail detail = new SalesInvoiceDetail();
                            detail.SalesInvoiceId = invoice.Id;
                            detail.ItemId = item.ItemId;
                            detail.Quantity = item.Quantity;
                            detail.UnitId = 6;
                            detail.UnitIdSecondary = 6;
                            detail.SecUnitId = 6;
                            detail.UnitFormula = 1;
                            detail.SecUnitFormula = 1;
                            detail.Price = item.Rate;
                            detail.Discount = 0;
                            detail.AccountId = 12;
                            detail.CurrencyRate = 1;
                            detail.TaxId = 1;
                            detail.Discount = 0;
                            detail.CustDiscount = 0;
                            detail.CustDiscountAmount = 0;
                            detail.TaxPercent = 0;
                            detail.TaxAmount = 0;
                            detail.TotalAmount = item.Rate * item.Quantity;
                            db.SalesInvoiceDetails.Add(detail);


                            SalesTax salesTax = new SalesTax();
                            salesTax.SalesInvoiceId = invoice.Id;
                            salesTax.ItemId = item.ItemId;
                            salesTax.TaxId = 1;
                            salesTax.Amount = 0;
                            salesTax.CurrencyRate = 1;
                            db.SalesTaxes.Add(salesTax);
                        }

                        db.SaveChanges();
                        scope.Complete();
                        return RedirectToAction("SalesinvoiceList", new { Msg = "Sales invoice saved successfully..." });
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        return RedirectToAction("SalesinvoiceList", new { Err = "Sales invoice not saved..." });
                    }
                }
            }
        }

        public ActionResult DeleteInvoiceConfirmed(int? Id)
        {
            SalesInvoice salesinvoice = db.SalesInvoices.Find(Id);
            if (salesinvoice != null)
            {
                var salesinvoicedetail = db.SalesInvoiceDetails.Where(d => d.SalesInvoiceId == Id);

                var getsalesdelivery = db.SalesDeliveries.Where(d => d.Id == salesinvoice.ReferenceNo).FirstOrDefault();
                if (getsalesdelivery != null)
                {
                    getsalesdelivery.Status = "Saved";
                }
                foreach (var sid in salesinvoicedetail)
                {
                    db.SalesInvoiceDetails.Remove(sid);
                }

                var salesTax = db.SalesTaxes.Where(d => d.SalesInvoiceId == Id);
                foreach (var sit in salesTax)
                {
                    db.SalesTaxes.Remove(sit);
                }
                var salescosting = db.SalesCostingDetails.Where(d => d.SalesInvoiceId == Id);
                foreach (var sic in salescosting)
                {
                    db.SalesCostingDetails.Remove(sic);
                }
                db.SalesInvoices.Remove(salesinvoice);
                db.SaveChanges();
            }
            return RedirectToAction("SalesinvoiceList");
        }

        #endregion


        #region Receive Voucher

        [HttpGet]
        public ActionResult ReceiveVoucherList(string Msg, string Err)
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

                List<VoucherModelView> voucher = new List<VoucherModelView>();


                long Createdby = Convert.ToInt32(Session["Createdid"]);

                int companyid = Convert.ToInt32(Session["companyid"]);


                long Branchid = Convert.ToInt64(Session["BranchId"]);

                int userid = Convert.ToInt32(Session["userid"]);

                var bankList = db.Banks.Where(d => d.CompanyId == companyid && d.BranchId == Branchid).ToList();
                int fyid = Convert.ToInt32(Session["fid"]);
                ViewBag.BranchId = Branchid;
                List<ReceiptPayment> grid = new List<ReceiptPayment>();
                var ledgerlist = db.LedgerMasters.Where(d => d.CompanyId == companyid).ToList();
                var branchList = db.BranchMasters.Where(d => d.CompanyId == companyid).ToList();
                var usersList = db.Users.Where(d => d.CompanyId == companyid).ToList();
                // var userlist = db.Users.Where(d => (d.CompanyId == companyid && d.BranchId == Branchid && d.UserId == userid) || (d.CompanyId == 0 && d.BranchId == 0 && d.UserId == userid)).ToList();
                if (Branchid == 0)
                    grid = db.ReceiptPayments.Where(d => (d.CompanyId == companyid && d.UserId == userid && d.fYearId == fyid && d.CreatedBy==Createdby) && (d.transactionType == "General Receive")).OrderByDescending(d => d.RPdate).ThenByDescending(d => d.VoucherNo).Take(100).ToList();
                else
                    grid = db.ReceiptPayments.Where(d => (d.CompanyId == companyid && d.BranchId == Branchid && d.UserId == userid && d.fYearId == fyid && d.CreatedBy==Createdby) && (d.transactionType == "General Receive")).OrderByDescending(d => d.RPdate).ThenByDescending(d => d.VoucherNo).Take(100).ToList();

                foreach (var item in grid)
                {
                    var vou = new VoucherModelView();
                    vou.Id = item.Id;
                    vou.reconst = item.ReconStatus;
                    vou.RPdate = item.RPdate;
                    vou.ledgerId = Convert.ToInt32(item.ledgerId);
                    vou.GeneralLedger = ledgerlist.Where(l => l.LID == vou.ledgerId).Select(l => l.ledgerName).FirstOrDefault();
                    vou.RPBankName = item.RPBankId == null ? "N/A" : bankList.Where(d => d.Id == item.RPBankId).Select(d => d.Name).FirstOrDefault();
                    vou.transactionType = item.transactionType;
                    vou.ModeOfPay = item.ModeOfPay;
                    vou.Prefix = item.Prefix;
                    vou.VoucherNo = item.VoucherNo;
                    vou.RPCashAmount = item.ModeOfPay == "Cash" ? Convert.ToDecimal(item.RPCashAmount) : Convert.ToDecimal(item.RPBankAmount);
                    vou.transactionNo = item.transactionNo;
                    vou.chequeNo = item.ModeOfPay == "Cheque" ? item.chequeNo : item.ModeOfPay == "NEFT" ? item.NeftRtgsNo : "N/A";
                    vou.UserName = usersList.Where(u => u.Id == item.CreatedBy).Select(u => u.UserName).FirstOrDefault();
                    vou.remarks = item.Remarks;
                    vou.branchCode = branchList.Where(d => d.Id == item.BranchId).Select(d => d.Name).FirstOrDefault();

                    vou.MoneyReceiptNo = item.MoneyReceiptNo;
                    if (item.chequeDate != null)
                    {
                        vou.chqDate = (Convert.ToDateTime(item.chequeDate));
                    }
                    else
                    {

                    }
                    vou.TotalAmount = Convert.ToDecimal(item.TotalAmount);
                    // vou.remarks = item.Remarks;
                    voucher.Add(vou);
                }
                return View(voucher.OrderBy(d => d.RPdate).ThenBy(d => d.VoucherNo));
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
                return RedirectToAction("ReceiveVoucherList", new { Err = "Data not  saved successfully.." });

            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Data  not Saved Successfully.......";
                return RedirectToAction("ReceiveVoucherList", new { Err = InventoryMessage.InsertError });

            }
        }


        public ActionResult ReceiveVoucher()
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Createdby = Convert.ToInt32(Session["Createdid"]);

            var list = new SelectList(new[]
                                   {
                                              new {ID="Cash",Name="Cash"},
                                              new{ID="Cheque",Name="Cheque"},
                                              new{ID="NEFT",Name="Neft/Rtgs"}

                                          },
                     "ID", "Name");
            ViewData["mode"] = list;

            var bankList = new List<Taxname>();
            ViewBag.customer = db.Customers.Where(r => r.SalesPersonId == Createdby).Select(s => new { Text = s.Name, Value = s.LId }).ToList();
            var blist = db.Banks.Where(d => d.CompanyId == companyid).Select(d => new Taxname { Id = d.LId, Name = d.Name });
            bankList.AddRange(blist);
            ViewBag.Bank = bankList;

            return View();
        }

        [HttpPost]
        public ActionResult ReceiveVoucher(ReceiptPaymentModelView model)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(Session["DateCulture"].ToString());
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            int Fyid = Convert.ToInt32(Session["fid"]);
            int Createdby = Convert.ToInt32(Session["Createdid"]);

            tr: string TransNo = Convert.ToString(GenRandom.GetRandom());

            var trans = db.ReceiptPayments.Where(d => d.transactionNo == TransNo).Select(d => d.Id).FirstOrDefault();

            if (trans > 0)
            {
                goto tr;

            }


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
                    ReceiptPayment payment = new ReceiptPayment();
                    payment.RPdate = DateTime.ParseExact(model.ReceiptDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    payment.RPDatetime = DateTime.Now;
                    payment.MoneyReceiptNo = model.MoneyReceiptNo;
                    payment.fYearId = Fyid;
                    payment.RPType = "Receive";

                    payment.ledgerId = model.ledgerId;
                    payment.TotalAmount = model.TotalAmount;
                    payment.IsTCS = false;
                    payment.transactionType = "General Receive";
                    if (model.ModeOfPay == "Cash")
                    {
                        payment.RPCashId = 35;
                        payment.RPCashAmount = model.TotalAmount;
                        payment.ModeOfPay = "Cash";
                    }

                    else if (model.ModeOfPay == "NEFT")
                    {
                        payment.RPBankId = model.RPBankId;
                        payment.NeftRtgsNo = model.NeftRtgsNo;
                        payment.ModeOfPay = "NEFT";
                        payment.RPBankAmount = model.TotalAmount;
                    }
                    payment.transactionNo = TransNo;
                    payment.Remarks = model.Remarks;


                    string prefix = "";
                    var fyear = db.FinancialYearMasters.Where(d => d.fYearID == Fyid).Select(d => d.Year).FirstOrDefault();
                    var fs = fyear.Substring(2, 2);
                    var es = fyear.Substring(7, 2);
                    fyear = fs + "-" + es;
                    int vouch = 1;
                    // var voucher = new Voucher();

                    var prefixList = db.Prefixes.Where(p => p.UserId == p.UserId && p.CompanyId == companyid && p.BranchId == Branchid && p.DefaultPrefix == "VR").FirstOrDefault();
                    if (prefixList != null)
                    {
                        if (prefixList.SetPrefix != null)
                        {
                            prefix = "VR/" + prefixList.SetPrefix + "/" + fyear + "/";
                        }
                        else
                        {
                            ViewBag.Error = "Please Set Prefix and then proceed.";
                            return View("ReceiveVoucher");
                        }
                    }
                    else
                    {
                        ViewBag.Error = "Please Set Prefix and then proceed.";
                        return View("ReceiveVoucher");
                    }


                    if (db.ReceiptPayments.Where(d => (d.transactionType == "General Receive" || d.transactionType == "General Payment" || d.transactionType == "Cash Invoice") && d.CompanyId == companyid && d.BranchId == Branchid && d.UserId == userid && d.fYearId == Fyid && d.Prefix == prefix).FirstOrDefault() != null)
                    {
                        vouch = (db.ReceiptPayments.Where(d => (d.transactionType == "General Receive" || d.transactionType == "General Payment" || d.transactionType == "Cash Invoice") && d.CompanyId == companyid && d.BranchId == Branchid && d.UserId == userid && d.fYearId == Fyid && d.Prefix == prefix).Max(v => (int?)v.VoucherNo) ?? 0) + 1;

                    }

                    payment.Prefix = prefix;
                    payment.VoucherNo = vouch;

                    payment.ReconStatus = false;
                    payment.CreatedBy = Createdby;
                    payment.CompanyId = companyid;
                    payment.BranchId = Branchid;
                    payment.UserId = userid;
                    payment.IsBillWise = false;
                    db.ReceiptPayments.Add(payment);
                    db.SaveChanges();
                    scope.Complete();
                    return RedirectToAction("ReceiveVoucherList", new { Msg = "Receive voucher created successfully..." });

                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    return RedirectToAction("ReceiveVoucherList", new { Err = "Error in saving voucher..." });
                }
            }

          
        }

        public ActionResult Delete(string transactionNo)
        {
            try
            {

                int userid = Convert.ToInt32(Session["userid"]);
                int companyid = Convert.ToInt32(Session["companyid"]);
                var results = db.ReceiptPayments.Where(d => d.CompanyId == companyid && d.transactionNo == transactionNo).ToList();
                var receiptId = results.FirstOrDefault().Id;
                var removeBillPayments = db.BillWiseReceives.Where(d => d.Id == receiptId).ToList();
                foreach (var removeBill in removeBillPayments)
                {
                    db.BillWiseReceives.Remove(removeBill);
                }
                // var voucher = db.Vouchers.Where(d => d.Id == result.transactionNo).FirstOrDefault();
                foreach (var result in results)
                {
                    db.ReceiptPayments.Remove(result);
                }
                db.SaveChanges();
                return RedirectToAction("ReceiveVoucherList", new { Msg = "Voucher deleted Successfully...." });
            }
            catch
            {
                return RedirectToAction("ReceiveVoucherList", new { Err = InventoryMessage.Delte });
            }

        }

        #endregion


        #region Customer 

        public ActionResult ShowAllCustomer(string Msg, string Err)
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
            int fyid = Convert.ToInt32(Session["fid"]);
            long Createdby = Convert.ToInt32(Session["Createdid"]);

            List<Customer> customer = new List<Customer>();
            List<Customer> result = new List<Customer>();
            List<BranchMaster> branchlist = new List<BranchMaster>();
            List<TaxComponent> openingList = new List<TaxComponent>();
            var branches = db.BranchMasters.Where(d => d.CompanyId == companyid).ToList();
            var br = new BranchMaster();
            br.Id = 0;
            br.Name = "Head Office";
            branchlist.Add(br);
            branchlist.AddRange(branches);
            if (Branchid == 0)
            {
                customer = db.Customers.Where(d => d.UserId == userid && d.CompanyId == companyid && d.SalesPersonId == Createdby).OrderBy(d => d.Name).ToList();
                //openingList = db.OpeningBalances.Where(d => d.CompanyId == companyid && d.fYearID == fyid).GroupBy(d => d.ledgerID).Select(d => new TaxComponent { TaxId = d.Key, Amount = d.Sum(t => t.openingBal) }).ToList();
                openingList = db.OpeningBalances.Where(d => d.CompanyId == companyid && d.fYearID == fyid && d.BranchId == 0).Select(d => new TaxComponent { TaxId = d.ledgerID, Amount = d.openingBal }).ToList();
                result = (from c in customer
                          join b in branchlist on c.BranchId equals b.Id
                          join op in openingList on c.LId equals op.TaxId into slop
                          from op in slop.DefaultIfEmpty()
                          select new Customer { Id = c.Id, Code = c.Code, Name = c.Name, PCode = c.PCode, CustomerType = c.CustomerType, BankBranch = b.Name, OpeningBal = op == null ? 0 : op.Amount }).ToList();
            }
            else
            {
                customer = db.Customers.Where(d => d.UserId == userid && d.CompanyId == companyid && d.SalesPersonId == Createdby).OrderBy(d => d.Name).ToList();
                //openingList = db.OpeningBalances.Where(d => d.CompanyId == companyid && d.fYearID == fyid).GroupBy(d => d.ledgerID).Select(d => new TaxComponent { TaxId = d.Key, Amount = d.Sum(t => t.openingBal) }).ToList();
                openingList = db.OpeningBalances.Where(d => d.CompanyId == companyid && d.fYearID == fyid && d.BranchId == 0).Select(d => new TaxComponent { TaxId = d.ledgerID, Amount = d.openingBal }).ToList();
                result = (from c in customer
                          join b in branchlist on c.BranchId equals b.Id
                          join op in openingList on c.LId equals op.TaxId into slop
                          from op in slop.DefaultIfEmpty()
                          select new Customer { Id = c.Id, Code = c.Code, Name = c.Name, PCode = c.PCode, CustomerType = c.CustomerType, BankBranch = b.Name, OpeningBal = op == null ? 0 : op.Amount }).ToList();
            }


            return View(result);

        }

        public static string GenerateRandomCode()
        {
            // Get the current timestamp as a long (number of ticks)
            long timestamp = DateTime.Now.Ticks;

            // Use a random number generator to create a 5-digit number
            Random random = new Random();
            int randomNumber = random.Next(10000, 100000); // Generates a number between 10000 and 99999

            // Combine the timestamp (truncated for simplicity) and the random number
            string code = (timestamp % 100000).ToString() + randomNumber.ToString();

            return code.Substring(0, 5); // Return only the first 5 characters
        }

        public ActionResult CreateCustomerSales()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateCustomerSales(Customer model)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                try
                {
                    long Createdby = Convert.ToInt32(Session["Createdid"]);
                    long Branchid = Convert.ToInt64(Session["BranchId"]);
                    long companyid = Convert.ToInt64(Session["companyid"]);
                    int fYearid = Convert.ToInt32(Session["fid"]);
                    int userid = Convert.ToInt32(Session["userid"]);
                    var timestamp = GenerateRandomCode();

                    var getPrefix = db.Prefixes.Where(d => d.CompanyId == companyid && d.DefaultPrefix == "CUS").FirstOrDefault();
                    if (getPrefix.SetPrefix != null)
                        model.Code = getPrefix.SetPrefix + timestamp;
                    else
                        model.Code = getPrefix.DefaultPrefix + timestamp;
                    model.CustomerType = "Wholesalers";
                    model.Discount = 0;
                    model.IsFDCompounded = false;
                    model.FD1 = 0;
                    model.FD2 = 0;
                    model.FD3 = 0;
                    model.FD4 = 0;
                    model.OpeningBal = 0;
                    model.Taxable = true;
                    model.TaxId = 1; // Assuming 1 is the default tax ID
                    model.CurrencyId = 1; // Assuming 1 is the default currency ID
                    model.UserId = userid;
                    model.CompanyId = companyid;
                    model.BranchId = Branchid;
                    model.CreatedBy = Createdby;
                    model.CreatedOn = DateTime.Now;
                    model.SalesPersonId = Convert.ToInt32(Createdby);
                    model.CreatedBy = Createdby;
                    model.CreatedOn = DateTime.Now;
                    model.CompanyId = companyid;
                    model.BranchId = Branchid;
                    db.Customers.Add(model);
                    db.SaveChanges();


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
                    ledger.ledgerName = model.Name;
                    ledger.groupID = 108;
                    ledger.CompanyId = companyid;
                    ledger.fYearID = fYearid;
                    ledger.ledgerType = "General";
                    ledger.BranchId = Branchid;
                    ledger.UserId = userid;

                    //int Createdby = Convert.ToInt32(Session["Createdid"]);
                    ledger.CreatedBy = Createdby;
                    ledger.CreatedOn = DateTime.Now;
                    db.LedgerMasters.Add(ledger);
                    db.SaveChanges();
                    model.LId = ledger.LID;

                    //   long ledid = taxobj.GetLedgerInsertId(ledger);
                    if (model.OpeningBal == null)
                        model.OpeningBal = 0;
                    var openingBalance = new OpeningBalance();
                    openingBalance.BranchId = 0;
                    openingBalance.CompanyId = companyid;
                    openingBalance.fYearID = fYearid;
                    openingBalance.ledgerID = ledger.LID;
                    //if (Branchid != 0)
                    //{
                    openingBalance.openingBal = model.OpeningBal ?? 0;
                    //}
                    //else
                    //{
                    //    openingBalance.openingBal = 0;
                    //}
                    openingBalance.UserId = userid;
                    openingBalance.CreatedBy = Createdby;
                    openingBalance.CreatedOn = DateTime.Now;
                    db.OpeningBalances.Add(openingBalance);
                    db.SaveChanges();



                    return RedirectToAction("ShowAllCustomer", new { Msg = "Customer created successfully..." });
                }
                catch (Exception ex)
                {
                    return RedirectToAction("ShowAllCustomer", new { Err = "Customer not created..." });
                }
            }

        }
        #endregion

    }
}

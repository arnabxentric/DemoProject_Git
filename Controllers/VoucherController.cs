using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Transactions;
using System.Data.Entity.Validation;
using System.Data;
using System.Globalization;
using NPOI.POIFS.Properties;

namespace XenERP.Controllers
{
    [SessionExpire]
    public class VoucherController : Controller
    {
        //
        // GET: /Voucher/

        InventoryEntities db = new InventoryEntities();
        public ActionResult Index()
        {
            return View();

        }



        //public decimal GetBankClosingAmount(string Bankid)
        //{


        //    int companyid = Convert.ToInt32(Session["companyid"]);
        //    long Branchid = Convert.ToInt64(Session["BranchId"]);
        //    int userid = Convert.ToInt32(Session["userid"]);
        //    int fyid = Convert.ToInt32(Session["fid"]);
        //    decimal amount = Convert.ToDecimal(db.BRSALLBankBookViews.Where(d => d.ledgerID == Bankid && d.CompanyId == companyid && d.BranchId == Branchid && d.fYearId == fyid).Select(d => d.closingBal).FirstOrDefault());
        //    return amount;

        //}

        #region ==================================== RECEIVED VOUCHER=================================================
        [HttpGet]
        public ActionResult ReceiveVoucherListSearch(string DateFrom, string DateTo)
        {
            var culture = Session["DateCulture"].ToString();
            var dateFormat = Session["DateFormat"].ToString();
            var DtFrm = DateTime.ParseExact(DateFrom, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var DtTo = DateTime.ParseExact(DateTo, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            try
            {

                List<VoucherModelView> voucher = new List<VoucherModelView>();




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
                    grid = db.ReceiptPayments.Where(d => (d.CompanyId == companyid && d.UserId == userid && d.fYearId == fyid && d.RPdate >= DtFrm && d.RPdate <= DtTo) && (d.transactionType == "General Receive")).ToList();
                else
                    grid = db.ReceiptPayments.Where(d => (d.CompanyId == companyid && d.BranchId == Branchid && d.UserId == userid && d.fYearId == fyid && d.RPdate >= DtFrm && d.RPdate <= DtTo) && (d.transactionType == "General Receive")).ToList();


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
                    grid = db.ReceiptPayments.Where(d => (d.CompanyId == companyid && d.UserId == userid && d.fYearId == fyid) && (d.transactionType == "General Receive")).OrderByDescending(d => d.RPdate).ThenByDescending(d => d.VoucherNo).Take(100).ToList();
                else
                    grid = db.ReceiptPayments.Where(d => (d.CompanyId == companyid && d.BranchId == Branchid && d.UserId == userid && d.fYearId == fyid) && (d.transactionType == "General Receive")).OrderByDescending(d => d.RPdate).ThenByDescending(d => d.VoucherNo).Take(100).ToList();

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


        [HttpGet]
        public ActionResult CreateReceiveVoucher(int? vno = null)
        {

            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            int fyid = Convert.ToInt32(Session["fid"]);

            var culture = Session["DateCulture"].ToString();
            var dateFormat = Session["DateFormat"].ToString();


            var rec = new ReceiptPaymentModelView();

            //  var Ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.parentID == null).Select(d => new { Id = d.LID, Name = d.ledgerID + "-" + d.ledgerName });


            //    ViewBag.ledger = Ledger;

            var bankList = new List<Taxname>();
            //var bank = new Taxname();
            //bank.Id = 36;
            //bank.Name = "RECEIVE CHEQUE/D.D.";
            //bankList.Add(bank);
            var blist = db.Banks.Where(d => d.CompanyId == companyid).Select(d => new Taxname { Id = d.LId, Name = d.Name });
            bankList.AddRange(blist);
            ViewBag.Bank = bankList;
            var branchList = db.BranchMasters.Where(d => d.CompanyId == companyid).ToList();
            ViewBag.branchList = branchList;
            //if (vno != null)
            //{
            //    var getvoucherNo = db.Vouchers.Where(d => d.Id == vno).FirstOrDefault();
            //    if (getvoucherNo != null)
            //        ViewBag.VoucherNo = getvoucherNo.Prefix + getvoucherNo.VoucherNo;
            //}
            rec.CardName = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var list = new SelectList(new[]
                                         {
                                              new {ID="Cash",Name="Cash"},
                                              new{ID="Cheque",Name="Cheque"},
                                              new{ID="NEFT",Name="Neft/Rtgs"}

                                          },
                           "ID", "Name");
            ViewData["mode"] = list;
            var list1 = new SelectList(new[]
                 {
                                              new {ID=0.075,Name=0.075},
                                              new {ID=0.75,Name=0.75}
                                          },
                  "ID", "Name");
            ViewData["tcstype"] = list1;

            rec.IsTCS = false;
            // rec.BranchId = Branchid;
            ViewBag.BranchId = Branchid;
            return View(rec);
        }

        [HttpPost]
        public ActionResult CreateReceiveVoucher(ReceiptPaymentModelView receive, FormCollection collection)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(Session["DateCulture"].ToString());
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            int Fyid = Convert.ToInt32(Session["fid"]);
            int Createdby = Convert.ToInt32(Session["Createdid"]);
            var CashId = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.ledgerID == "2040").Select(d => d.LID).FirstOrDefault();
            ViewBag.ModeOfPay = receive.ModeOfPay;
            var bankList = new List<Taxname>();
            var blist = db.Banks.Where(d => d.CompanyId == companyid).Select(d => new Taxname { Id = d.LId, Name = d.Name });
            bankList.AddRange(blist);
            ViewBag.Bank = bankList;
            var branchList = db.BranchMasters.Where(d => d.CompanyId == companyid).ToList();
            ViewBag.branchList = branchList;
            ViewBag.BranchId = Branchid;
            var list = new SelectList(new[]
                                     {
                                              new {ID="Cash",Name="Cash"},
                                              new{ID="Cheque",Name="Cheque"},
                                              new{ID="NEFT",Name="Neft/Rtgs"}

                                          },
                       "ID", "Name");
            ViewData["mode"] = list;

            var list1 = new SelectList(new[]
                 {
                                              new {ID=0.075,Name=0.075},
                                              new {ID=0.75,Name=0.75}
                                          },
       "ID", "Name");
            ViewData["tcstype"] = list1;

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

                    var date = DateTime.ParseExact(receive.CardName, dateFormat, CultureInfo.CreateSpecificCulture(culture));

                    var getDateRange = db.FinancialYearMasters.Where(d => d.fYearID == Fyid).FirstOrDefault();
                    if (!(date >= getDateRange.sDate && date <= getDateRange.eDate))
                    {
                        ViewBag.Error = "Voucher Date out of scope of " + getDateRange.Year + " Financial Year.";
                        return View(receive);
                    }
                    if (receive.ModeOfPay == "Cash")
                    {
                        if (!(receive.RPCashAmount > 0))
                        {
                            ViewBag.Error = "Cash Amount must be greater than zero.";
                            return View(receive);
                        }
                    }

                    if (receive.ModeOfPay == "Cheque")
                    {
                        if (!(receive.RPBankAmount > 0))
                        {
                            ViewBag.Error = "Bank Amount must be greater than zero.";
                            return View(receive);
                        }
                    }
                    if (receive.ModeOfPay == "NEFT")
                    {
                        if (!(receive.RPNEFTAmount > 0))
                        {
                            ViewBag.Error = "RTGS Amount must be greater than zero.";
                            return View(receive);
                        }
                    }

                    int? assignedBranch = 0;
                    if (receive.BId == null)
                        assignedBranch = Branchid;
                    else
                        assignedBranch = receive.BId;

                    string prefix = "";
                    var fyear = db.FinancialYearMasters.Where(d => d.fYearID == Fyid).Select(d => d.Year).FirstOrDefault();
                    var fs = fyear.Substring(2, 2);
                    var es = fyear.Substring(7, 2);
                    fyear = fs + "-" + es;
                    int vouch = 1;
                    // var voucher = new Voucher();


                    if (receive.RPDetails.Count() > 0)
                    {
                        foreach (var item in receive.RPDetails)
                        {
                            if (item.LineTotal > 0 && item.LID > 0)
                            {
                                //if (receive.RPCashAmount > 0)
                                //{
                                //    prefix = "CR/" + fyear + "/";
                                //}
                                //if (receive.RPBankAmount > 0 || receive.RPNEFTAmount>0)
                                //{
                                //    prefix = "BR/" + fyear + "/";
                                //}
                                //if (Fyid == 9)
                                //{
                                var prefixList = db.Prefixes.Where(p => p.UserId == p.UserId && p.CompanyId == companyid && p.BranchId == assignedBranch && p.DefaultPrefix == "VR").FirstOrDefault();
                                if (prefixList != null)
                                {
                                    if (prefixList.SetPrefix != null)
                                    {
                                        prefix = "VR/" + prefixList.SetPrefix + "/" + fyear + "/";
                                    }
                                    else
                                    {
                                        ViewBag.Error = "Please Set Prefix and then proceed.";
                                        return View(receive);
                                    }
                                }
                                else
                                {
                                    ViewBag.Error = "Please Set Prefix and then proceed.";
                                    return View(receive);
                                    //prefix =  "VR/" + fyear + "/";
                                }
                                //}
                                //else
                                //{
                                //       prefix = "VR/" + fyear + "/";
                                //}

                                if (db.ReceiptPayments.Where(d => (d.transactionType == "General Receive" || d.transactionType == "General Payment" || d.transactionType == "Cash Invoice") && d.CompanyId == companyid && d.BranchId == assignedBranch && d.UserId == userid && d.fYearId == Fyid && d.Prefix == prefix).FirstOrDefault() != null)
                                {
                                    vouch = (db.ReceiptPayments.Where(d => (d.transactionType == "General Receive" || d.transactionType == "General Payment" || d.transactionType == "Cash Invoice") && d.CompanyId == companyid && d.BranchId == assignedBranch && d.UserId == userid && d.fYearId == Fyid && d.Prefix == prefix).Max(v => (int?)v.VoucherNo) ?? 0) + 1;

                                }

                                break;
                            }
                        }
                    }
                    decimal totalcash = 0;
                    decimal totalbank = 0;
                    decimal totalrtgs = 0;
                    ReceiptPayment recpayment = null;
                    if (receive.IsTCS == false)
                    {
                        foreach (var item in receive.RPDetails)
                        {
                            if (item.LineTotal > 0 && item.LID > 0)
                            {
                                recpayment = new ReceiptPayment();
                                recpayment.IsTCS = receive.IsTCS;
                                recpayment.RPdate = date;
                                recpayment.RPDatetime = DateTime.Now;

                                recpayment.fYearId = Fyid;
                                if (receive.BId == null)
                                    recpayment.BranchId = Branchid;
                                else
                                    recpayment.BranchId = receive.BId;

                                recpayment.CompanyId = companyid;
                                recpayment.UserId = userid;
                                recpayment.ledgerId = item.LID;
                                //cusLId = item.LID;
                                if (receive.ModeOfPay == "Cash")
                                {


                                    if (receive.RPCashAmount > 0)
                                    {
                                        recpayment.RPCashId = Convert.ToInt32(CashId);
                                        recpayment.RPCashAmount = item.Cash;
                                        recpayment.TotalAmount = item.Cash;

                                        totalcash += (decimal)item.Cash;
                                    }
                                    else
                                    {

                                        recpayment.RPCashId = 0;
                                        recpayment.RPCashAmount = 0;
                                        recpayment.TotalAmount = 0;

                                        ViewBag.Error = "Please Enter Cash Amount";
                                        return View(receive);

                                    }
                                }
                                else if (receive.ModeOfPay == "Cheque" || receive.ModeOfPay == "NEFT")
                                {
                                    if (item.Bank > 0 || item.RTGS > 0)
                                    {

                                        if (receive.RPBankId == null)
                                        {
                                            ViewBag.Error = "Please Choose Bank";
                                            return View(receive);
                                        }
                                        recpayment.RPBankId = receive.RPBankId;

                                        if (receive.ModeOfPay == "Cheque")
                                        {

                                            if (receive.chequeNo == null)
                                            {
                                                ViewBag.Error = "Please Enter Cheque No";
                                                return View(receive);
                                            }

                                            recpayment.RPBankAmount = item.Bank;
                                            recpayment.TotalAmount = item.Bank;
                                            totalbank += (decimal)item.Bank;
                                            recpayment.chequeNo = receive.chequeNo;
                                            recpayment.chequeDetails = receive.chequeDetails;
                                            if (Convert.ToString(collection["cdate"]) == "")
                                            {
                                                ViewBag.Error = "Please Enter Cheque Date";
                                                return View(receive);
                                            }
                                            else
                                            {
                                                var chequedate = DateTime.ParseExact(Convert.ToString(collection["cdate"]), dateFormat, CultureInfo.CreateSpecificCulture(culture));
                                                recpayment.chequeDate = chequedate;
                                            }
                                        }
                                        else
                                        {
                                            if (receive.NeftRtgsNo == null)
                                            {
                                                ViewBag.Error = "Please Enter Neft/Rtgs No";
                                                return View(receive);
                                            }

                                            recpayment.NeftRtgsNo = receive.NeftRtgsNo;
                                            recpayment.RPBankAmount = item.RTGS;
                                            recpayment.TotalAmount = item.RTGS;
                                            totalrtgs += (decimal)item.RTGS;
                                        }
                                    }
                                    else
                                    {
                                        recpayment.RPBankId = 0;
                                        recpayment.RPBankAmount = 0;
                                        recpayment.TotalAmount = 0;
                                        ViewBag.Error = "Please Enter Amount";
                                        return View(receive);
                                    }
                                }
                                else
                                {
                                    ViewBag.Error = "Please Select A Mode of Pay";
                                    return View(receive);
                                }
                                recpayment.transactionType = InventoryConst.cns_General_Receive;
                                recpayment.RPType = InventoryConst.Cns_Receive;

                                recpayment.VoucherNo = vouch;
                                recpayment.Prefix = prefix;
                                recpayment.MoneyReceiptNo = receive.MoneyReceiptNo;
                                recpayment.ModeOfPay = receive.ModeOfPay;
                                recpayment.transactionNo = TransNo;
                                recpayment.Remarks = receive.Remarks;
                                recpayment.CreatedBy = Createdby;
                                recpayment.IsBillWise = receive.IsBillWise;
                                //if (Branchid == 0)
                                //{
                                //    recpayment.ReconStatus = false;
                                //}
                                //else
                                //{
                                //    if (checkMenu == true)
                                //    {
                                //        recpayment.ReconStatus = false;
                                //    }
                                //    else
                                //    {
                                //        recpayment.ReconStatus = true;
                                //    }
                                //}
                                db.ReceiptPayments.Add(recpayment);
                                //   receive recpayment.Id;
                                db.SaveChanges();
                                receive.Id = recpayment.Id;
                            }
                            else
                            {
                                ViewBag.Error = "Ledger Name cannot be blank in any line.";
                                return View(receive);
                            }

                        }
                    }
                    else
                    {
                        ReceiptPayment recpaymentForTCS = null;
                        decimal TCSPercent = receive.TCSType ?? 0;
                        foreach (var item in receive.RPDetails)
                        {
                            if (item.LineTotal > 0 && item.LID > 0)
                            {
                                recpayment = new ReceiptPayment();
                                recpayment.IsTCS = receive.IsTCS;
                                recpayment.TCSType = receive.TCSType;
                                recpayment.RPdate = date;
                                recpayment.RPDatetime = DateTime.Now;
                                recpayment.fYearId = Fyid;

                                if (receive.BId == null)
                                    recpayment.BranchId = Branchid;
                                else
                                    recpayment.BranchId = receive.BId;
                                recpayment.CompanyId = companyid;
                                recpayment.UserId = userid;
                                recpayment.ledgerId = item.LID;

                                recpaymentForTCS = new ReceiptPayment();
                                recpaymentForTCS.IsTCS = receive.IsTCS;
                                recpaymentForTCS.TCSType = receive.TCSType;
                                recpaymentForTCS.RPdate = date;
                                recpaymentForTCS.RPDatetime = DateTime.Now;
                                recpaymentForTCS.fYearId = Fyid;

                                recpaymentForTCS.BranchId = receive.BId;
                                if (receive.BId == null)
                                    recpaymentForTCS.BranchId = Branchid;
                                else
                                    recpaymentForTCS.BranchId = receive.BId;

                                recpaymentForTCS.CompanyId = companyid;
                                recpaymentForTCS.UserId = userid;
                                var getTCSLedger = db.LedgerMasters.Where(d => d.TagWith == item.LID).FirstOrDefault();
                                if (getTCSLedger == null)
                                {
                                    ViewBag.Error = "TCS Sub Ledger for this Party does not Exist.";
                                    return View(receive);
                                }
                                recpaymentForTCS.ledgerId = (int)getTCSLedger.LID;


                                if (receive.ModeOfPay == "Cash")
                                {


                                    if (receive.RPCashAmount > 0)
                                    {
                                        decimal TCScash = (decimal)item.Cash * TCSPercent / 100;
                                        TCScash = Math.Round(TCScash, 2);
                                        decimal PartyCash = (decimal)item.Cash - TCScash;

                                        recpayment.RPCashId = Convert.ToInt32(CashId);
                                        recpayment.RPCashAmount = PartyCash;
                                        recpayment.TotalAmount = PartyCash;


                                        recpaymentForTCS.RPCashId = Convert.ToInt32(CashId);
                                        recpaymentForTCS.RPCashAmount = TCScash;
                                        recpaymentForTCS.TotalAmount = TCScash;

                                        totalcash += (decimal)item.Cash;

                                    }
                                    else
                                    {
                                        recpayment.RPCashId = 0;
                                        recpayment.RPCashAmount = 0;
                                        recpayment.TotalAmount = 0;

                                        recpaymentForTCS.RPCashId = 0;
                                        recpaymentForTCS.RPCashAmount = 0;
                                        recpaymentForTCS.TotalAmount = 0;

                                        ViewBag.Error = "Please Enter Cash Amount";
                                        return View(receive);

                                    }
                                }
                                else if (receive.ModeOfPay == "Cheque" || receive.ModeOfPay == "NEFT")
                                {
                                    if (item.Bank > 0 || item.RTGS > 0)
                                    {

                                        if (receive.RPBankId == null)
                                        {
                                            ViewBag.Error = "Please Choose Bank";
                                            return View(receive);
                                        }
                                        recpayment.RPBankId = receive.RPBankId;
                                        recpaymentForTCS.RPBankId = receive.RPBankId;

                                        if (receive.ModeOfPay == "Cheque")
                                        {

                                            if (receive.chequeNo == null)
                                            {
                                                ViewBag.Error = "Please Enter Cheque No";
                                                return View(receive);
                                            }

                                            decimal TCSbank = (decimal)item.Bank * TCSPercent / 100;
                                            TCSbank = Math.Round(TCSbank, 2);
                                            decimal PartyBank = (decimal)item.Bank - TCSbank;

                                            recpayment.RPBankAmount = PartyBank;
                                            recpayment.TotalAmount = PartyBank;
                                            recpayment.chequeNo = receive.chequeNo;
                                            recpayment.chequeDetails = receive.chequeDetails;

                                            recpaymentForTCS.RPBankAmount = TCSbank;
                                            recpaymentForTCS.TotalAmount = TCSbank;
                                            recpaymentForTCS.chequeNo = receive.chequeNo;
                                            recpaymentForTCS.chequeDetails = receive.chequeDetails;

                                            totalbank += (decimal)item.Bank;

                                            if (Convert.ToString(collection["cdate"]) == "")
                                            {
                                                ViewBag.Error = "Please Enter Cheque Date";
                                                return View(receive);
                                            }
                                            else
                                            {
                                                var chequedate = DateTime.ParseExact(Convert.ToString(collection["cdate"]), dateFormat, CultureInfo.CreateSpecificCulture(culture));
                                                recpayment.chequeDate = chequedate;
                                                recpaymentForTCS.chequeDate = chequedate;
                                            }
                                        }
                                        else
                                        {
                                            if (receive.NeftRtgsNo == null)
                                            {
                                                ViewBag.Error = "Please Enter Neft/Rtgs No";
                                                return View(receive);
                                            }

                                            decimal TCSrtgs = (decimal)item.RTGS * TCSPercent / 100;
                                            TCSrtgs = Math.Round(TCSrtgs, 2);
                                            decimal Partyrtgs = (decimal)item.RTGS - TCSrtgs;

                                            recpayment.NeftRtgsNo = receive.NeftRtgsNo;
                                            recpayment.RPBankAmount = Partyrtgs;
                                            recpayment.TotalAmount = Partyrtgs;

                                            recpaymentForTCS.NeftRtgsNo = receive.NeftRtgsNo;
                                            recpaymentForTCS.RPBankAmount = TCSrtgs;
                                            recpaymentForTCS.TotalAmount = TCSrtgs;

                                            totalrtgs += (decimal)item.RTGS;
                                        }
                                    }
                                    else
                                    {
                                        recpayment.RPBankId = 0;
                                        recpayment.RPBankAmount = 0;
                                        recpayment.TotalAmount = 0;

                                        recpaymentForTCS.RPBankId = 0;
                                        recpaymentForTCS.RPBankAmount = 0;
                                        recpaymentForTCS.TotalAmount = 0;

                                        ViewBag.Error = "Please Enter Amount";
                                        return View(receive);
                                    }
                                }
                                else
                                {
                                    ViewBag.Error = "Please Select A Mode of Pay";
                                    return View(receive);
                                }

                                recpayment.transactionType = InventoryConst.cns_General_Receive;
                                recpayment.RPType = InventoryConst.Cns_Receive;
                                recpayment.VoucherNo = vouch;
                                recpayment.Prefix = prefix;
                                recpayment.MoneyReceiptNo = receive.MoneyReceiptNo;
                                recpayment.ModeOfPay = receive.ModeOfPay;
                                recpayment.transactionNo = TransNo;
                                recpayment.Remarks = receive.Remarks;
                                recpayment.IsBillWise = receive.IsBillWise;

                                recpayment.CreatedBy = Createdby;

                                recpaymentForTCS.transactionType = InventoryConst.cns_General_Receive;
                                recpaymentForTCS.RPType = InventoryConst.Cns_Receive;
                                recpaymentForTCS.VoucherNo = vouch;
                                recpaymentForTCS.Prefix = prefix;
                                recpaymentForTCS.MoneyReceiptNo = receive.MoneyReceiptNo;
                                recpaymentForTCS.ModeOfPay = receive.ModeOfPay;
                                recpaymentForTCS.transactionNo = TransNo;
                                recpaymentForTCS.Remarks = receive.Remarks;

                                recpaymentForTCS.CreatedBy = Createdby;


                                //if (Branchid == 0)
                                //{
                                //    recpayment.ReconStatus = false;
                                //    recpaymentForTCS.ReconStatus = false;
                                //}
                                //else
                                //{
                                //    if (checkMenu == true)
                                //    {
                                //        recpayment.ReconStatus = false;
                                //        recpaymentForTCS.ReconStatus = false;
                                //    }
                                //    else
                                //    {
                                //        recpayment.ReconStatus = true;
                                //        recpaymentForTCS.ReconStatus = true;
                                //    }
                                //}
                                db.ReceiptPayments.Add(recpayment);
                                db.ReceiptPayments.Add(recpaymentForTCS);


                                //   receive recpayment.Id;
                                db.SaveChanges();
                                receive.Id = recpayment.Id;
                            }
                            else
                            {
                                ViewBag.Error = "Ledger Name cannot be blank in any line.";
                                return View(receive);
                            }

                        }
                    }


                    if (receive.IsBillWise == true)
                    {
                        var getPayment = db.ReceiptPayments.Where(d => d.transactionNo == TransNo).FirstOrDefault();
                        var selectedBill = receive.BillList.Where(d => d.Id != 0);
                        foreach (var bill in selectedBill)
                        {
                            var billwiseReceive = new BillWiseReceive();
                            // billwiseReceive.Id = getPayment.Id;
                            billwiseReceive.Type = "S";
                            billwiseReceive.InvoiceId = bill.Id;
                            billwiseReceive.InvoiceNo = bill.Name;
                            billwiseReceive.ReceiptPaymentId = recpayment.Id;
                            billwiseReceive.BillAmount = bill.BillAmount;
                            billwiseReceive.Paid = bill.Paid;
                            billwiseReceive.Due = bill.Due;
                            db.BillWiseReceives.Add(billwiseReceive);
                            db.SaveChanges();

                            var totalPay = db.BillWiseReceives.Where(r => r.InvoiceId == bill.Id).Sum(s => s.Paid);
                            var invoice = db.SalesInvoices.Where(r => r.Id == bill.Id).FirstOrDefault();
                            if (invoice.GrandTotal == totalPay)
                            {
                                invoice.IsPaid = true;
                                db.SaveChanges();
                            }
                        }

                    }

                    //  db.SaveChanges();
                    scope.Complete();
                    // return RedirectToAction("ReceiveVoucherList", new { Msg = "Receive Voucher No." + prefix + vouch + " generated successfully." });
                    ViewBag.VoucherNo = prefix + vouch;
                    return RedirectToAction("EditReceiveVoucher", new { id = receive.Id, Msg = prefix + vouch });
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
                    return RedirectToAction("ReceiveVoucherList", new { Err = InventoryMessage.InsertError });

                }
                catch (DataException)
                {
                    //Log the error (add a variable name after DataException)
                    ViewBag.Error = "Error:Data  not Saved Successfully.......";
                    return RedirectToAction("ReceiveVoucherList", new { Err = InventoryMessage.InsertError });

                }
                catch (Exception exp)
                {
                    return RedirectToAction("ReceiveVoucherList", new { Err = InventoryMessage.InsertError });

                }
            }

        }

        [HttpGet]
        public ActionResult EditReceiveVoucher(int? id = 0, string Msg = "")
        {
            if (Msg != "")
            {
                ViewBag.VoucherMSG = Msg;
            }
            else
            {
                // ViewBag.Error = Err;
            }
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            int fyid = Convert.ToInt32(Session["fid"]);

            var culture = Session["DateCulture"].ToString();
            var dateFormat = Session["DateFormat"].ToString();

            var ledgerList = db.LedgerMasters.Where(d => d.CompanyId == companyid).Select(d => new { Id = d.LID, Name = d.ledgerName }).ToList();

            var bankList = new List<Taxname>();

            var blist = db.Banks.Where(d => d.CompanyId == companyid).Select(d => new Taxname { Id = d.LId, Name = d.Name });
            bankList.AddRange(blist);
            ViewBag.Bank = bankList;
            var branchList = db.BranchMasters.Where(d => d.CompanyId == companyid).ToList();
            ViewBag.branchList = branchList;
            ViewBag.BranchId = Branchid;
            var list = new SelectList(new[]
                                     {
                                              new {ID="Cash",Name="Cash"},
                                              new{ID="Cheque",Name="Cheque"},
                                              new{ID="NEFT",Name="Neft/Rtgs"}

                                          },
                       "ID", "Name");
            ViewData["mode"] = list;


            var list1 = new SelectList(new[]
                             {
                                              new {ID=0.075,Name=0.075},
                                              new {ID=0.75,Name=0.75}
                                          },
                   "ID", "Name");
            ViewData["tcstype"] = list1;

            var receivemv = new ReceiptPaymentModelView();
            List<ReceiptPaymentDetailModelView> receivedetailmvList = new List<ReceiptPaymentDetailModelView>();
            var rec = db.ReceiptPayments.Where(d => d.Id == id && d.CompanyId == companyid).FirstOrDefault();
            ViewBag.VoucherNo = rec.Prefix + rec.VoucherNo;
            if (rec.ModeOfPay == "Cash")
            {
                ViewBag.ModeOfPay = "Cash";
            }
            else if (rec.ModeOfPay == "Cheque")
            {
                receivemv.RPBankId = rec.RPBankId;
                receivemv.chequeNo = rec.chequeNo;
                receivemv.chequeDetails = rec.chequeDetails;
                receivemv.cdate = rec.chequeDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                ViewBag.ModeOfPay = "Cheque";
            }
            else
            {
                receivemv.RPBankId = rec.RPBankId;
                receivemv.NeftRtgsNo = rec.NeftRtgsNo;
                ViewBag.ModeOfPay = "NEFT";
            }
            receivemv.CardName = rec.RPdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
            receivemv.transactionNo = rec.transactionNo;
            receivemv.Remarks = rec.Remarks;
            receivemv.MoneyReceiptNo = rec.MoneyReceiptNo;
            var recdetails = db.ReceiptPayments.Where(d => d.transactionNo == rec.transactionNo && d.CompanyId == companyid).ToList();
            decimal totalcash = 0;
            decimal totalbank = 0;
            decimal totalrtgs = 0;

            foreach (var recdet in recdetails)
            {
                var receivedetailmv = new ReceiptPaymentDetailModelView();

                receivedetailmv.LID = (int)recdet.ledgerId;
                receivedetailmv.LName = ledgerList.Where(d => d.Id == receivedetailmv.LID).Select(d => d.Name).FirstOrDefault();
                if (recdet.ModeOfPay == "Cash")
                {
                    receivedetailmv.Cash = recdet.RPCashAmount;
                    receivedetailmv.LineTotal = recdet.RPCashAmount ?? 0;
                    totalcash += receivedetailmv.LineTotal;
                }
                else if (recdet.ModeOfPay == "Cheque")
                {
                    receivedetailmv.Bank = recdet.RPBankAmount;
                    receivedetailmv.LineTotal = recdet.RPBankAmount ?? 0;
                    totalbank += receivedetailmv.LineTotal;
                }
                else
                {
                    receivedetailmv.RTGS = recdet.RPBankAmount;
                    receivedetailmv.LineTotal = recdet.RPBankAmount ?? 0;
                    totalrtgs += receivedetailmv.LineTotal;
                }

                receivedetailmvList.Add(receivedetailmv);
            }
            if (rec.ModeOfPay == "Cash")
                receivemv.RPCashAmount = totalcash;
            if (rec.ModeOfPay == "Cheque")
                receivemv.RPBankAmount = totalbank;
            if (rec.ModeOfPay == "NEFT")
                receivemv.RPNEFTAmount = totalrtgs;

            if (rec.IsTCS == true)
            {
                receivemv.IsTCS = true;
            }
            else
            {
                receivemv.IsTCS = false;
            }

            if (rec.IsBillWise == true)
            {
                receivemv.IsBillWise = true;
            }
            else
            {
                receivemv.IsBillWise = false;
            }
            receivemv.BId = rec.BranchId;
            receivemv.RPDetails = receivedetailmvList;
            return View(receivemv);
        }


        [HttpPost]
        public ActionResult EditReceiveVoucher(ReceiptPaymentModelView receive, FormCollection collection)
        {

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(Session["DateCulture"].ToString());
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            int Createdby = Convert.ToInt32(Session["Createdid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            int Fyid = Convert.ToInt32(Session["fid"]);
            var CashId = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.ledgerID == "2040").Select(d => d.LID).FirstOrDefault();
            ViewBag.ModeOfPay = receive.ModeOfPay;

            var bankList = new List<Taxname>();

            var blist = db.Banks.Where(d => d.CompanyId == companyid).Select(d => new Taxname { Id = d.LId, Name = d.Name });
            bankList.AddRange(blist);
            ViewBag.Bank = bankList;
            var branchList = db.BranchMasters.Where(d => d.CompanyId == companyid).ToList();
            ViewBag.branchList = branchList;
            ViewBag.BranchId = Branchid;
            var list = new SelectList(new[]
                                     {
                                              new {ID="Cash",Name="Cash"},
                                              new{ID="Cheque",Name="Cheque"},
                                              new{ID="NEFT",Name="Neft/Rtgs"}

                                          },
                       "ID", "Name");
            ViewData["mode"] = list;

            var list1 = new SelectList(new[]
                             {
                                              new {ID=0.075,Name=0.075},
                                              new {ID=0.75,Name=0.75}
                                          },
                   "ID", "Name");
            ViewData["tcstype"] = list1;

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

                    var date = DateTime.ParseExact(receive.CardName, dateFormat, CultureInfo.CreateSpecificCulture(culture));

                    var getDateRange = db.FinancialYearMasters.Where(d => d.fYearID == Fyid).FirstOrDefault();
                    if (!(date >= getDateRange.sDate && date <= getDateRange.eDate))
                    {
                        ViewBag.Error = "Voucher Date out of scope of " + getDateRange.Year + " Financial Year.";
                        return View(receive);
                    }

                    var voucher = db.ReceiptPayments.Where(d => d.CompanyId == companyid && d.transactionNo == receive.transactionNo).FirstOrDefault();
                    //if (voucher.ModeOfPay == "Cash" || receive.ModeOfPay == "Cash")
                    //{
                    //    if (voucher.ModeOfPay != receive.ModeOfPay)
                    //    {
                    //        ViewBag.Error = "Mode of Payment of Voucher cannot be changed.";
                    //        return View(receive);
                    //    }
                    //}
                    if (receive.ModeOfPay == "Cash")
                    {
                        if (!(receive.RPCashAmount > 0))
                        {
                            ViewBag.Error = "Cash Amount must be greater than zero.";
                            return View(receive);
                        }
                    }

                    if (receive.ModeOfPay == "Cheque")
                    {
                        if (!(receive.RPBankAmount > 0))
                        {
                            ViewBag.Error = "Bank Amount must be greater than zero.";
                            return View(receive);
                        }
                    }
                    if (receive.ModeOfPay == "NEFT")
                    {
                        if (!(receive.RPNEFTAmount > 0))
                        {
                            ViewBag.Error = "RTGS Amount must be greater than zero.";
                            return View(receive);
                        }
                    }

                   

                    var removeoldReceipts = db.ReceiptPayments.Where(d => d.CompanyId == companyid && d.transactionNo == receive.transactionNo).ToList();
                    foreach (var removeoldReceipt in removeoldReceipts)
                    {

                        var removeBillPayments = db.BillWiseReceives.Where(d => d.ReceiptPaymentId == removeoldReceipt.Id);
                        foreach (var removeBill in removeBillPayments)
                        {
                            var invoice = db.SalesInvoices.Where(r => r.Id == removeBill.InvoiceId).FirstOrDefault();
                            if (invoice.IsPaid == true)
                            {
                                invoice.IsPaid = false;
                            }
                            db.BillWiseReceives.Remove(removeBill);
                        }
                        db.SaveChanges();

                        var removeOpeningPayments = db.OpBalancePaymentDetails.Where(d => d.ReceiptId == removeoldReceipt.Id);
                        foreach (var removeBill in removeOpeningPayments)
                        {
                            var invoice = db.OpeningBalancePayments.Where(r => r.Id == removeBill.OpBalanceId).FirstOrDefault();
                            if (invoice.IsPaid == true)
                            {
                                invoice.IsPaid = false;
                            }
                            db.OpBalancePaymentDetails.Remove(removeBill);

                        }
                        db.SaveChanges();

                        db.ReceiptPayments.Remove(removeoldReceipt);
                    }
                    db.SaveChanges();
                    int? assignedBranch = 0;
                    if (receive.BId == null)
                        assignedBranch = Branchid;
                    else
                        assignedBranch = receive.BId;

                    string prefix = voucher.Prefix;
                    int? vouch = voucher.VoucherNo;

                    if ((voucher.fYearId == Fyid) && (voucher.BranchId != assignedBranch))
                    {
                        var fyear = db.FinancialYearMasters.Where(d => d.fYearID == Fyid).Select(d => d.Year).FirstOrDefault();
                        var fs = fyear.Substring(2, 2);
                        var es = fyear.Substring(7, 2);
                        fyear = fs + "-" + es;

                        // var voucher = new Voucher();
                        if (receive.RPDetails.Count() > 0)
                        {
                            foreach (var item in receive.RPDetails)
                            {
                                if (item.LineTotal > 0 && item.LID > 0)
                                {
                                    //if (receive.RPCashAmount > 0)
                                    //{
                                    //    prefix = "CR/" + fyear + "/";
                                    //}
                                    //if (receive.RPBankAmount > 0 || receive.RPNEFTAmount > 0)
                                    //{
                                    //    prefix = "BR/" + fyear + "/";
                                    //}
                                    //if (Fyid == 9)
                                    //{
                                    var prefixList = db.Prefixes.Where(p => p.UserId == p.UserId && p.CompanyId == companyid && p.BranchId == assignedBranch && p.DefaultPrefix == "VR").FirstOrDefault();
                                    if (prefixList != null)
                                    {
                                        if (prefixList.SetPrefix != null)
                                        {
                                            prefix = "VR/" + prefixList.SetPrefix + "/" + fyear + "/";
                                        }
                                        else
                                        {
                                            ViewBag.Error = "Please Set Prefix and then proceed.";
                                            return View(receive);
                                        }
                                    }
                                    else
                                    {
                                        ViewBag.Error = "Please Set Prefix and then proceed.";
                                        return View(receive);
                                        //prefix =  "VR/" + fyear + "/";
                                    }
                                    //}
                                    //else
                                    //{
                                    //    prefix = "VR/" + fyear + "/";
                                    //}
                                    if (db.ReceiptPayments.Where(d => (d.transactionType == "General Receive" || d.transactionType == "General Payment" || d.transactionType == "Cash Invoice") && d.CompanyId == companyid && d.BranchId == assignedBranch && d.UserId == userid && d.fYearId == Fyid && d.Prefix == prefix).FirstOrDefault() != null)
                                    {
                                        vouch = (db.ReceiptPayments.Where(d => (d.transactionType == "General Receive" || d.transactionType == "General Payment" || d.transactionType == "Cash Invoice") && d.CompanyId == companyid && d.BranchId == assignedBranch && d.UserId == userid && d.fYearId == Fyid && d.Prefix == prefix).Max(v => (int?)v.VoucherNo) ?? 0) + 1;

                                    }

                                    break;
                                }
                            }
                        }
                    }




                    db.SaveChanges();

                    decimal totalcash = 0;
                    decimal totalbank = 0;
                    decimal totalrtgs = 0;
                    string voucherNo = "";
                    ReceiptPayment recpayment = null;
                    int rowcount = receive.RPDetails.Count();

                    if (receive.IsTCS == false || rowcount > 1)
                    {
                        foreach (var item in receive.RPDetails)
                        {

                            if (item.LineTotal > 0 && item.LID > 0)
                            {

                                recpayment = new ReceiptPayment();



                                recpayment.RPdate = date;
                                recpayment.RPDatetime = voucher.RPDatetime;

                                recpayment.fYearId = voucher.fYearId;
                                if (receive.BId == null)
                                    recpayment.BranchId = Branchid;
                                else
                                    recpayment.BranchId = receive.BId;
                                recpayment.CompanyId = voucher.CompanyId;


                                recpayment.UserId = voucher.UserId;

                                recpayment.ledgerId = item.LID;


                                if (receive.ModeOfPay == "Cash")
                                {


                                    if (receive.RPCashAmount > 0)
                                    {
                                        //long CashId = 35;

                                        recpayment.RPCashId = Convert.ToInt32(CashId);
                                        recpayment.RPCashAmount = item.Cash;
                                        recpayment.TotalAmount = item.Cash;

                                        totalcash += (decimal)item.Cash;
                                    }
                                    else
                                    {

                                        recpayment.RPCashId = 0;
                                        recpayment.RPCashAmount = 0;
                                        recpayment.TotalAmount = 0;

                                        ViewBag.Error = "Please Enter Cash Amount";
                                        return View(receive);

                                    }
                                }
                                else if (receive.ModeOfPay == "Cheque" || receive.ModeOfPay == "NEFT")
                                {
                                    if (item.Bank > 0 || item.RTGS > 0)
                                    {

                                        if (receive.RPBankId == null)
                                        {
                                            ViewBag.Error = "Please Choose Bank";
                                            return View(receive);
                                        }

                                        recpayment.RPBankId = receive.RPBankId;

                                        if (receive.ModeOfPay == "Cheque")
                                        {

                                            if (receive.chequeNo == null)
                                            {
                                                ViewBag.Error = "Please Enter Cheque No";
                                                return View(receive);
                                            }

                                            recpayment.RPBankAmount = item.Bank;
                                            recpayment.TotalAmount = item.Bank;
                                            totalbank += (decimal)item.Bank;
                                            recpayment.chequeNo = receive.chequeNo;
                                            recpayment.chequeDetails = receive.chequeDetails;
                                            if (Convert.ToString(collection["cdate"]) == "")
                                            {
                                                ViewBag.Error = "Please Enter Cheque Date";
                                                return View(receive);
                                            }
                                            else
                                            {
                                                var chequedate = DateTime.ParseExact(Convert.ToString(collection["cdate"]), dateFormat, CultureInfo.CreateSpecificCulture(culture));
                                                recpayment.chequeDate = chequedate;
                                            }
                                        }
                                        else
                                        {
                                            if (receive.NeftRtgsNo == null)
                                            {
                                                ViewBag.Error = "Please Enter Neft/Rtgs No";
                                                return View(receive);
                                            }

                                            recpayment.NeftRtgsNo = receive.NeftRtgsNo;
                                            recpayment.RPBankAmount = item.RTGS;
                                            recpayment.TotalAmount = item.RTGS;
                                            totalrtgs += (decimal)item.RTGS;
                                        }
                                    }
                                    else
                                    {
                                        recpayment.RPBankId = 0;
                                        recpayment.RPBankAmount = 0;
                                        recpayment.TotalAmount = 0;
                                        ViewBag.Error = "Please Enter Amount";
                                        return View(receive);
                                    }
                                }
                                else
                                {
                                    ViewBag.Error = "Please Select A Mode of Pay";
                                    return View(receive);
                                }

                                recpayment.transactionType = InventoryConst.cns_General_Receive;
                                recpayment.RPType = InventoryConst.Cns_Receive;


                                //recpayment.VoucherNo = voucher.VoucherNo;
                                //recpayment.Prefix = voucher.Prefix;
                                //voucherNo = voucher.Prefix + voucher.VoucherNo;
                                if (voucher.Prefix == prefix && voucher.VoucherNo == vouch)
                                {
                                    recpayment.VoucherNo = voucher.VoucherNo;
                                    recpayment.Prefix = voucher.Prefix;
                                    voucherNo = voucher.Prefix + voucher.VoucherNo;
                                }
                                else
                                {
                                    recpayment.VoucherNo = vouch;
                                    recpayment.Prefix = prefix;
                                    voucherNo = prefix + vouch;
                                }
                                recpayment.MoneyReceiptNo = receive.MoneyReceiptNo;
                                recpayment.ModeOfPay = receive.ModeOfPay;
                                recpayment.transactionNo = voucher.transactionNo;
                                recpayment.Remarks = receive.Remarks;

                                recpayment.IsTCS = receive.IsTCS;
                                if (rowcount > 1)
                                {
                                    receive.IsTCS = true;
                                    recpayment.IsTCS = true;
                                    recpayment.TCSType = receive.TCSType;
                                }
                                recpayment.CreatedBy = voucher.CreatedBy;

                                recpayment.ModifiedBy = Createdby;
                                recpayment.ModifiedOn = DateTime.Now;
                                //recpayment.PVId = voucher.PVId;




                                //if (Branchid == 0)
                                //{
                                //    recpayment.ReconStatus = false;
                                //}
                                //else
                                //{
                                //    if (checkMenu)
                                //    {
                                //        recpayment.ReconStatus = false;
                                //    }
                                //    else
                                //    {
                                //        recpayment.ReconStatus = true;
                                //    }
                                //}

                                db.ReceiptPayments.Add(recpayment);

                            }
                            else
                            {
                                ViewBag.Error = "Ledger Name cannot be blank in any line.";
                                return View(receive);
                            }

                        }
                        db.SaveChanges();
                    }
                    else
                    {
                        ReceiptPayment recpaymentForTCS = null;
                        decimal TCSPercent = receive.TCSType ?? 0;
                        foreach (var item in receive.RPDetails)
                        {
                            if (item.LineTotal > 0 && item.LID > 0)
                            {
                                recpayment = new ReceiptPayment();
                                recpayment.RPdate = date;
                                recpayment.RPDatetime = DateTime.Now;
                                recpayment.fYearId = Fyid;
                                if (receive.BId == null)
                                    recpayment.BranchId = Branchid;
                                else
                                    recpayment.BranchId = receive.BId;
                                recpayment.CompanyId = companyid;
                                recpayment.UserId = userid;
                                recpayment.ledgerId = item.LID;

                                recpaymentForTCS = new ReceiptPayment();
                                recpaymentForTCS.RPdate = date;
                                recpaymentForTCS.RPDatetime = DateTime.Now;
                                recpaymentForTCS.fYearId = Fyid;
                                if (receive.BId == null)
                                    recpaymentForTCS.BranchId = Branchid;
                                else
                                    recpaymentForTCS.BranchId = receive.BId;
                                recpaymentForTCS.CompanyId = companyid;
                                recpaymentForTCS.UserId = userid;
                                var getTCSLedger = db.LedgerMasters.Where(d => d.TagWith == item.LID).FirstOrDefault();
                                if (getTCSLedger == null)
                                {
                                    ViewBag.Error = "TCS Sub Ledger for this Party does not Exist.";
                                    return View(receive);
                                }
                                recpaymentForTCS.ledgerId = (int)getTCSLedger.LID;


                                if (receive.ModeOfPay == "Cash")
                                {


                                    if (receive.RPCashAmount > 0)
                                    {
                                        decimal TCScash = (decimal)item.Cash * TCSPercent / 100;
                                        TCScash = Math.Round(TCScash, 2);
                                        decimal PartyCash = (decimal)item.Cash - TCScash;

                                        recpayment.RPCashId = Convert.ToInt32(CashId);
                                        recpayment.RPCashAmount = PartyCash;
                                        recpayment.TotalAmount = PartyCash;


                                        recpaymentForTCS.RPCashId = Convert.ToInt32(CashId);
                                        recpaymentForTCS.RPCashAmount = TCScash;
                                        recpaymentForTCS.TotalAmount = TCScash;

                                        totalcash += (decimal)item.Cash;

                                    }
                                    else
                                    {
                                        recpayment.RPCashId = 0;
                                        recpayment.RPCashAmount = 0;
                                        recpayment.TotalAmount = 0;

                                        recpaymentForTCS.RPCashId = 0;
                                        recpaymentForTCS.RPCashAmount = 0;
                                        recpaymentForTCS.TotalAmount = 0;

                                        ViewBag.Error = "Please Enter Cash Amount";
                                        return View(receive);

                                    }
                                }
                                else if (receive.ModeOfPay == "Cheque" || receive.ModeOfPay == "NEFT")
                                {
                                    if (item.Bank > 0 || item.RTGS > 0)
                                    {

                                        if (receive.RPBankId == null)
                                        {
                                            ViewBag.Error = "Please Choose Bank";
                                            return View(receive);
                                        }
                                        recpayment.RPBankId = receive.RPBankId;
                                        recpaymentForTCS.RPBankId = receive.RPBankId;

                                        if (receive.ModeOfPay == "Cheque")
                                        {

                                            if (receive.chequeNo == null)
                                            {
                                                ViewBag.Error = "Please Enter Cheque No";
                                                return View(receive);
                                            }

                                            decimal TCSbank = (decimal)item.Bank * TCSPercent / 100;
                                            TCSbank = Math.Round(TCSbank, 2);
                                            decimal PartyBank = (decimal)item.Bank - TCSbank;

                                            recpayment.RPBankAmount = PartyBank;
                                            recpayment.TotalAmount = PartyBank;
                                            recpayment.chequeNo = receive.chequeNo;
                                            recpayment.chequeDetails = receive.chequeDetails;

                                            recpaymentForTCS.RPBankAmount = TCSbank;
                                            recpaymentForTCS.TotalAmount = TCSbank;
                                            recpaymentForTCS.chequeNo = receive.chequeNo;
                                            recpaymentForTCS.chequeDetails = receive.chequeDetails;

                                            totalbank += (decimal)item.Bank;

                                            if (Convert.ToString(collection["cdate"]) == "")
                                            {
                                                ViewBag.Error = "Please Enter Cheque Date";
                                                return View(receive);
                                            }
                                            else
                                            {
                                                var chequedate = DateTime.ParseExact(Convert.ToString(collection["cdate"]), dateFormat, CultureInfo.CreateSpecificCulture(culture));
                                                recpayment.chequeDate = chequedate;
                                                recpaymentForTCS.chequeDate = chequedate;
                                            }
                                        }
                                        else
                                        {
                                            if (receive.NeftRtgsNo == null)
                                            {
                                                ViewBag.Error = "Please Enter Neft/Rtgs No";
                                                return View(receive);
                                            }

                                            decimal TCSrtgs = (decimal)item.RTGS * TCSPercent / 100;
                                            TCSrtgs = Math.Round(TCSrtgs, 2);
                                            decimal Partyrtgs = (decimal)item.RTGS - TCSrtgs;

                                            recpayment.NeftRtgsNo = receive.NeftRtgsNo;
                                            recpayment.RPBankAmount = Partyrtgs;
                                            recpayment.TotalAmount = Partyrtgs;

                                            recpaymentForTCS.NeftRtgsNo = receive.NeftRtgsNo;
                                            recpaymentForTCS.RPBankAmount = TCSrtgs;
                                            recpaymentForTCS.TotalAmount = TCSrtgs;

                                            totalrtgs += (decimal)item.RTGS;
                                        }
                                    }
                                    else
                                    {
                                        recpayment.RPBankId = 0;
                                        recpayment.RPBankAmount = 0;
                                        recpayment.TotalAmount = 0;

                                        recpaymentForTCS.RPBankId = 0;
                                        recpaymentForTCS.RPBankAmount = 0;
                                        recpaymentForTCS.TotalAmount = 0;

                                        ViewBag.Error = "Please Enter Amount";
                                        return View(receive);
                                    }
                                }
                                else
                                {
                                    ViewBag.Error = "Please Select A Mode of Pay";
                                    return View(receive);
                                }


                                recpayment.transactionType = InventoryConst.cns_General_Receive;
                                recpayment.RPType = InventoryConst.Cns_Receive;
                                if (voucher.Prefix == prefix && voucher.VoucherNo == vouch)
                                {
                                    recpayment.VoucherNo = voucher.VoucherNo;
                                    recpayment.Prefix = voucher.Prefix;
                                    voucherNo = voucher.Prefix + voucher.VoucherNo;
                                }
                                else
                                {
                                    recpayment.VoucherNo = vouch;
                                    recpayment.Prefix = prefix;
                                    voucherNo = prefix + vouch;
                                }
                                recpayment.MoneyReceiptNo = receive.MoneyReceiptNo;
                                recpayment.ModeOfPay = receive.ModeOfPay;
                                recpayment.transactionNo = voucher.transactionNo;
                                recpayment.Remarks = receive.Remarks;

                                recpayment.IsTCS = receive.IsTCS;
                                recpayment.TCSType = receive.TCSType;
                                recpayment.CreatedBy = voucher.CreatedBy;
                                recpayment.ModifiedBy = Createdby;
                                recpayment.ModifiedOn = DateTime.Now;


                                recpaymentForTCS.transactionType = InventoryConst.cns_General_Receive;
                                recpaymentForTCS.RPType = InventoryConst.Cns_Receive;
                                //recpaymentForTCS.VoucherNo = voucher.VoucherNo;
                                //recpaymentForTCS.Prefix = voucher.Prefix;
                                //voucherNo = voucher.Prefix + voucher.VoucherNo;

                                if (voucher.Prefix == prefix && voucher.VoucherNo == vouch)
                                {
                                    recpaymentForTCS.VoucherNo = voucher.VoucherNo;
                                    recpaymentForTCS.Prefix = voucher.Prefix;
                                    voucherNo = voucher.Prefix + voucher.VoucherNo;
                                }
                                else
                                {
                                    recpaymentForTCS.VoucherNo = vouch;
                                    recpaymentForTCS.Prefix = prefix;
                                    voucherNo = prefix + vouch;
                                }

                                recpaymentForTCS.MoneyReceiptNo = receive.MoneyReceiptNo;
                                recpaymentForTCS.ModeOfPay = receive.ModeOfPay;
                                recpaymentForTCS.transactionNo = voucher.transactionNo;
                                recpaymentForTCS.Remarks = receive.Remarks;
                                recpaymentForTCS.IsTCS = receive.IsTCS;
                                recpaymentForTCS.TCSType = receive.TCSType;
                                recpaymentForTCS.CreatedBy = voucher.CreatedBy;
                                recpaymentForTCS.ModifiedBy = Createdby;
                                recpaymentForTCS.ModifiedOn = DateTime.Now;
                                //recpaymentForTCS.PVId = voucher.PVId;

                                //if (Branchid == 0)
                                //{
                                //    recpayment.ReconStatus = false;
                                //    recpaymentForTCS.ReconStatus = false;
                                //}
                                //else
                                //{
                                //    if (checkMenu == true)
                                //    {
                                //        recpayment.ReconStatus = false;
                                //        recpaymentForTCS.ReconStatus = false;
                                //    }
                                //    else
                                //    {
                                //        recpayment.ReconStatus = true;
                                //        recpaymentForTCS.ReconStatus = true;
                                //    }
                                //}
                                db.ReceiptPayments.Add(recpayment);
                                db.ReceiptPayments.Add(recpaymentForTCS);
                            }

                            db.SaveChanges();
                        }
                    }


                    if (receive.IsBillWise == true)
                    {
                        var getPayment = db.ReceiptPayments.Where(d => d.transactionNo == voucher.transactionNo).FirstOrDefault();
                        var selectedBill = receive.BillList.Where(d => d.Id != 0);
                        foreach (var bill in selectedBill)
                        {
                            var findDuplicate = db.BillWiseReceives.Where(d => d.InvoiceId == bill.Id && d.BillAmount == bill.BillAmount && d.Due == bill.Due).FirstOrDefault();
                            if (findDuplicate == null)
                            {
                                var billwiseReceive = new BillWiseReceive();
                                //billwiseReceive.Id = getPayment.Id;
                                billwiseReceive.Type = "S";
                                billwiseReceive.InvoiceId = bill.Id;
                                billwiseReceive.ReceiptPaymentId = recpayment.Id;
                                billwiseReceive.InvoiceNo = bill.Name;
                                billwiseReceive.BillAmount = bill.BillAmount;
                                billwiseReceive.Paid = bill.Paid;
                                billwiseReceive.Due = bill.Due;
                                db.BillWiseReceives.Add(billwiseReceive);
                                db.SaveChanges();
                                var totalPay = db.BillWiseReceives.Where(r => r.InvoiceId == bill.Id).Sum(s => s.Paid);
                                var invoice = db.SalesInvoices.Where(r => r.Id == bill.Id).FirstOrDefault();
                                if (invoice.GrandTotal == totalPay)
                                {
                                    invoice.IsPaid = true;
                                    db.SaveChanges();
                                }
                            }
                        }

                    }

                    //db.SaveChanges();
                    scope.Complete();
                    return RedirectToAction("ReceiveVoucherList", new { Msg = "Receive Voucher No." + voucherNo + " updated successfully." });
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
                    return RedirectToAction("ReceiveVoucherList", new { Err = InventoryMessage.InsertError });

                }
                catch (DataException)
                {
                    //Log the error (add a variable name after DataException)
                    ViewBag.Error = "Error:Data  not Saved Successfully.......";
                    return RedirectToAction("ReceiveVoucherList", new { Err = InventoryMessage.InsertError });

                }
                catch (Exception exp)
                {
                    return RedirectToAction("ReceiveVoucherList", new { Err = InventoryMessage.InsertError });

                }
            }









        }
        //[HttpGet]
        //public ActionResult DetailsOfReceiveVoucher(int? id = 0)
        //{
        //    int companyid = Convert.ToInt32(Session["companyid"]);
        //    long Branchid = Convert.ToInt64(Session["BranchId"]);
        //    int userid = Convert.ToInt32(Session["userid"]);
        //    int fyid = Convert.ToInt32(Session["fid"]);

        //    var culture = Session["DateCulture"].ToString();
        //    var dateFormat = Session["DateFormat"].ToString();


        //   // var vou = db.Vouchers.Find(id);


        //    var bankList = new List<Taxname>();
        //    var rec = new ReceiptPaymentModelView();

        //    var blist = db.Banks.Where(d => d.CompanyId == companyid).Select(d => new Taxname { Id = d.LId, Name = d.Name });
        //    bankList.AddRange(blist);
        //    ViewBag.Bank = bankList;

        //    rec.CardName = vou.VoucherDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
        //    if (rec.chequeDate != null)
        //    {
        //      //  rec.ExpirayDate = vou.chequeDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
        //    }

        //    if (rec.ModeOfPay == "Cash")
        //    {
        //        ViewBag.ModeOfPay = "Cash";
        //    }
        //    else if (rec.ModeOfPay == "Cheque")
        //    {
        //        ViewBag.ModeOfPay = "Cheque";
        //    }
        //    else
        //    {
        //        ViewBag.ModeOfPay = "NEFT";
        //    }
        //    return View(rec);
        //}
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


        [HttpGet]
        public ActionResult GetCustomerInvoice(int LID)
        {
            int Fyid = Convert.ToInt32(Session["fid"]);
            var salesinvoiceList = (from s in db.SalesInvoices.Where(d => d.Customer.LId == LID && d.IsPaid == false && (d.FinancialYearId == Fyid || d.FinancialYearId == (Fyid - 1))).Select(d => new Bill { InvId = d.Id, InvNo = d.NO, Date = d.InvoiceDate, BillAmount = d.BCGrandTotal }).ToList()
                                    join r in db.SalesReturns.GroupBy(d => d.ReferenceNo).Select(d => new Bill { InvId = d.Key ?? 0, InvNo = "", ReturnAmount = d.Sum(t => t.BCGrandTotal) }).ToList()
                                    on s.InvId equals r.InvId into d
                                    from sr in d.DefaultIfEmpty()
                                    join b in db.BillWiseReceives.Where(d => d.Type == "S").GroupBy(d => d.InvoiceId).Select(d => new Bill { InvId = d.Key, BillAmount = d.Average(s => s.BillAmount), Paid = d.Sum(s => s.Paid) }).ToList()
                                    on s.InvId equals b.InvId into c
                                    from sc in c.DefaultIfEmpty()
                                    where (sc == null || ((sc == null ? 0 : sc.BillAmount) > (sc == null ? 0 : sc.Paid)))
                                    select new Bill
                                    {
                                        InvId = s.InvId,
                                        InvNo = s.InvNo,
                                        Date = s.Date,
                                        BillAmount = s.BillAmount,
                                        ReturnAmount = sr == null ? 0 : sr.ReturnAmount,
                                        Amount = s.BillAmount - (sr == null ? 0 : sr.ReturnAmount),
                                        Due = sc == null ? 0 : ((sc == null ? 0 : sc.BillAmount) - (sc == null ? 0 : sc.Paid))
                                    }).OrderBy(d => d.Date).ToList();


            return Json(salesinvoiceList, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetCustomerInvoiceEdit(int LID, int RPId)
        {


            int Fyid = Convert.ToInt32(Session["fid"]);
            List<Bill> billList = new List<Bill>();
            //var paidbill = db.BillWisePayments.Where(d => d.Id == RPId).Select(d => new Bill { InvId = d.InvoiceId, InvNo = d.InvoiceNo, Status = true }).ToList();
            var salesinvoiceList = (from s in db.SalesInvoices.Where(d => d.Customer.LId == LID && d.IsPaid == false && (d.FinancialYearId == Fyid || d.FinancialYearId == (Fyid - 1))).Select(d => new Bill { InvId = d.Id, InvNo = d.NO, Date = d.InvoiceDate, BillAmount = d.BCGrandTotal }).ToList()
                                    join r in db.SalesReturns.GroupBy(d => d.ReferenceNo).Select(d => new Bill { InvId = d.Key ?? 0, InvNo = "", ReturnAmount = d.Sum(t => t.BCGrandTotal) }).ToList()
                                    on s.InvId equals r.InvId into d
                                    from sr in d.DefaultIfEmpty()
                                    join b in db.BillWiseReceives.Where(d => d.Type == "S").GroupBy(d => d.InvoiceId).Select(d => new Bill { InvId = d.Key, BillAmount = d.Average(s => s.BillAmount), Paid = d.Sum(s => s.Paid) }).ToList()
                                    on s.InvId equals b.InvId into c
                                    from sc in c.DefaultIfEmpty()
                                    where (sc == null || ((sc == null ? 0 : sc.BillAmount) > (sc == null ? 0 : sc.Paid)))
                                    select new Bill
                                    {
                                        InvId = s.InvId,
                                        InvNo = s.InvNo,
                                        Date = s.Date,
                                        Status = sc == null ? false : true,
                                        BillAmount = s.BillAmount,
                                        ReturnAmount = sr == null ? 0 : sr.ReturnAmount,
                                        Amount = s.BillAmount - (sr == null ? 0 : sr.ReturnAmount),
                                        Paid = sc == null ? 0 : sc.Paid,
                                        Due = sc == null ? 0 : ((sc == null ? 0 : sc.BillAmount) - (sc == null ? 0 : sc.Paid))
                                    }).OrderBy(d => d.Date).ToList();
            //List<long> paidBill = new List<long>();
            //paidBill = salesinvoiceList.Select(d => d.InvId).ToList();
            var paidBill = (from b in db.BillWisePayments.Where(d => d.Type == "S" && d.ReceiptPaymentId == RPId).Select(d => new { InvId = d.InvoiceId, InvNo = d.InvoiceNo, Paid = 0, BillAmount = 0, Amount = 0, Status = true })
                            join c in db.BillWisePayments.GroupBy(d => new { d.InvoiceId, d.InvoiceNo }).Select(d => new { InvId = d.Key.InvoiceId, InvNo = d.Key.InvoiceNo, Paid = d.Sum(p => p.Paid), BillAmount = d.Average(p => p.BillAmount), Amount = d.Average(p => p.BillAmount), Status = true })
                            on b.InvId equals c.InvId
                            where (c.BillAmount == c.Paid)
                            select new Bill { InvId = c.InvId, InvNo = c.InvNo, Paid = c.Paid, BillAmount = c.BillAmount, Amount = c.Amount, Due = 0, Status = true }).ToList();
            //   var fullpaidbill = paidBill.Where(d => d.BillAmount != d.Paid).ToList();

            billList.AddRange(paidBill);
            billList.AddRange(salesinvoiceList);
            return Json(billList, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region ======================================== PAYMENT VOUCHER===============================================

        [HttpGet]
        public ActionResult PaymentVoucherListSearch(string DateFrom, string DateTo)
        {
            var culture = Session["DateCulture"].ToString();
            var dateFormat = Session["DateFormat"].ToString();
            var DtFrm = DateTime.ParseExact(DateFrom, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var DtTo = DateTime.ParseExact(DateTo, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            try
            {

                List<VoucherModelView> voucher = new List<VoucherModelView>();




                int companyid = Convert.ToInt32(Session["companyid"]);


                long Branchid = Convert.ToInt64(Session["BranchId"]);

                int userid = Convert.ToInt32(Session["userid"]);
                int Createdby = Convert.ToInt32(Session["Createdid"]);

                int fyid = Convert.ToInt32(Session["fid"]);
                var bankList = db.Banks.Where(d => d.CompanyId == companyid && d.BranchId == Branchid).ToList();

                List<ReceiptPayment> grid = new List<ReceiptPayment>();
                var ledgerlist = db.LedgerMasters.Where(d => d.CompanyId == companyid).ToList();

                var branchList = db.BranchMasters.Where(d => d.CompanyId == companyid).ToList();
                var usersList = db.Users.Where(d => d.CompanyId == companyid).ToList();
                ViewBag.BranchId = Branchid;
                //     var userlist = db.Users.Where(d => (d.CompanyId == companyid && d.BranchId == Branchid && d.UserId == userid) || (d.CompanyId == 0 && d.BranchId == 0 && d.UserId == userid)).ToList();
                if (Branchid == 0 || Createdby == 14 || Createdby == 22)
                    grid = db.ReceiptPayments.Where(d => (d.CompanyId == companyid && d.UserId == userid && d.fYearId == fyid && d.RPdate >= DtFrm && d.RPdate <= DtTo) && (d.transactionType == "General Payment")).ToList();
                else
                    grid = db.ReceiptPayments.Where(d => (d.CompanyId == companyid && d.BranchId == Branchid && d.UserId == userid && d.fYearId == fyid && d.RPdate >= DtFrm && d.RPdate <= DtTo) && (d.transactionType == "General Payment")).ToList();

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
                    vou.PaidTo = item.CreditCardNo == null ? "" : item.CreditCardNo;
                    //  vou.VoucherId = (int)item.VoucherId;
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
                return RedirectToAction("PaymentVoucherList", new { Err = "Data not  saved successfully.." });

            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Data  not Saved Successfully.......";
                return RedirectToAction("PaymentVoucherList", new { Err = InventoryMessage.InsertError });

            }
        }
        [HttpGet]
        public ActionResult PaymentVoucherList(string Msg, string Err)
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




                int companyid = Convert.ToInt32(Session["companyid"]);


                long Branchid = Convert.ToInt64(Session["BranchId"]);

                int userid = Convert.ToInt32(Session["userid"]);
                int Createdby = Convert.ToInt32(Session["Createdid"]);

                int fyid = Convert.ToInt32(Session["fid"]);
                var bankList = db.Banks.Where(d => d.CompanyId == companyid && d.BranchId == Branchid).ToList();
                ViewBag.BranchId = Branchid;
                List<ReceiptPayment> grid = new List<ReceiptPayment>();
                var ledgerlist = db.LedgerMasters.Where(d => d.CompanyId == companyid).ToList();
                var branchList = db.BranchMasters.Where(d => d.CompanyId == companyid).ToList();
                var usersList = db.Users.Where(d => d.CompanyId == companyid).ToList();
                //var userlist = db.Users.Where(d => (d.CompanyId == companyid && d.BranchId == Branchid && d.UserId == userid) || (d.CompanyId == 0 && d.BranchId == 0 && d.UserId == userid)).ToList();
                if (Branchid == 0 || Createdby == 14 || Createdby == 22)
                    grid = db.ReceiptPayments.Where(d => (d.CompanyId == companyid && d.UserId == userid && d.fYearId == fyid) && (d.transactionType == "General Payment")).OrderByDescending(d => d.RPdate).ThenByDescending(d => d.VoucherNo).Take(100).ToList();
                else
                    grid = db.ReceiptPayments.Where(d => (d.CompanyId == companyid && d.BranchId == Branchid && d.UserId == userid && d.fYearId == fyid) && (d.transactionType == "General Payment")).OrderByDescending(d => d.RPdate).ThenByDescending(d => d.VoucherNo).Take(100).ToList();

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
                    vou.PaidTo = item.CreditCardNo == null ? "" : item.CreditCardNo;
                    //  vou.VoucherId = (int)item.VoucherId;
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
                return RedirectToAction("PaymentVoucherList", new { Err = "Data not  saved successfully.." });

            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Data  not Saved Successfully.......";
                return RedirectToAction("PaymentVoucherList", new { Err = InventoryMessage.InsertError });

            }
        }


        [HttpGet]
        public ActionResult CreatePaymentVoucher(string vno = null)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            int fyid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            var dateFormat = Session["DateFormat"].ToString();
            int Createdby = Convert.ToInt32(Session["Createdid"]);
            ViewBag.Createdby = Createdby;
            var rec = new ReceiptPaymentModelView();

            //   var Ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.parentID == null).Select(d => new { Id = d.LID, Name = d.ledgerID + "-" + d.ledgerName });


            //   ViewBag.ledger = Ledger;

            var bankList = new List<Taxname>();

            var blist = db.Banks.Where(d => d.CompanyId == companyid).Select(d => new Taxname { Id = d.LId, Name = d.Name });
            bankList.AddRange(blist);
            ViewBag.Bank = bankList;
            var branchList = db.BranchMasters.Where(d => d.CompanyId == companyid).ToList();
            ViewBag.branchList = branchList;
            ViewBag.BranchId = Branchid;
            //if (vno != null)
            //{
            //    var getvoucherNo = db.Vouchers.Where(d => d.Id == vno).FirstOrDefault();
            //    if (getvoucherNo != null)
            ViewBag.VoucherNo = vno;
            //}
            rec.CardName = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var list = new SelectList(new[]
                                     {
                                              new {ID="Cash",Name="Cash"},
                                              new{ID="Cheque",Name="Cheque"},
                                              new{ID="NEFT",Name="Neft/Rtgs"}

                                          },
                       "ID", "Name");
            ViewData["mode"] = list;
            var list1 = new SelectList(new[]
                             {
                                              new {ID=0.075,Name=0.075},
                                              new {ID=0.75,Name=0.75}
                                          },
                   "ID", "Name");
            ViewData["tcstype"] = list1;
            rec.IsTCS = false;
            return View(rec);
        }

        [HttpPost]
        public ActionResult CreatePaymentVoucher(ReceiptPaymentModelView receive, FormCollection collection)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(Session["DateCulture"].ToString());
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            int Fyid = Convert.ToInt32(Session["fid"]);
            int Createdby = Convert.ToInt32(Session["Createdid"]);
            ViewBag.Createdby = Createdby;
            var CashId = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.ledgerID == "2040").Select(d => d.LID).FirstOrDefault();
            ViewBag.ModeOfPay = receive.ModeOfPay;
            var bankList = new List<Taxname>();
            var blist = db.Banks.Where(d => d.CompanyId == companyid).Select(d => new Taxname { Id = d.LId, Name = d.Name });
            bankList.AddRange(blist);
            ViewBag.Bank = bankList;
            var branchList = db.BranchMasters.Where(d => d.CompanyId == companyid).ToList();
            ViewBag.branchList = branchList;
            ViewBag.BranchId = Branchid;
            var list = new SelectList(new[]
                                     {
                                              new {ID="Cash",Name="Cash"},
                                              new{ID="Cheque",Name="Cheque"},
                                              new{ID="NEFT",Name="Neft/Rtgs"}

                                          },
                       "ID", "Name");
            ViewData["mode"] = list;


            var list1 = new SelectList(new[]
                             {
                                              new {ID=0.075,Name=0.075},
                                              new {ID=0.75,Name=0.75}
                                          },
                   "ID", "Name");
            ViewData["tcstype"] = list1;


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

                    var date = DateTime.ParseExact(receive.CardName, dateFormat, CultureInfo.CreateSpecificCulture(culture));

                    var getDateRange = db.FinancialYearMasters.Where(d => d.fYearID == Fyid).FirstOrDefault();
                    if (!(date >= getDateRange.sDate && date <= getDateRange.eDate))
                    {
                        ViewBag.Error = "Voucher Date out of scope of " + getDateRange.Year + " Financial Year.";
                        return View(receive);
                    }
                    if (receive.ModeOfPay == "Cash")
                    {
                        if (!(receive.RPCashAmount > 0))
                        {
                            ViewBag.Error = "Cash Amount must be greater than zero.";
                            return View(receive);
                        }
                    }

                    if (receive.ModeOfPay == "Cheque")
                    {
                        if (!(receive.RPBankAmount > 0))
                        {
                            ViewBag.Error = "Bank Amount must be greater than zero.";
                            return View(receive);
                        }
                    }
                    if (receive.ModeOfPay == "NEFT")
                    {
                        if (!(receive.RPNEFTAmount > 0))
                        {
                            ViewBag.Error = "RTGS Amount must be greater than zero.";
                            return View(receive);
                        }
                    }

                    int? assignedBranch = 0;
                    if (receive.BId == null)
                        assignedBranch = Branchid;
                    else
                        assignedBranch = receive.BId;

                    string prefix = "";
                    var fyear = db.FinancialYearMasters.Where(d => d.fYearID == Fyid).Select(d => d.Year).FirstOrDefault();
                    var fs = fyear.Substring(2, 2);
                    var es = fyear.Substring(7, 2);
                    fyear = fs + "-" + es;
                    int vouch = 1;
                    // var voucher = new Voucher();


                    if (receive.RPDetails.Count() > 0)
                    {
                        foreach (var item in receive.RPDetails)
                        {
                            if (item.LineTotal > 0 && item.LID > 0)
                            {
                                //if (receive.RPCashAmount > 0)
                                //{
                                //    prefix = "CP/" + fyear + "/";
                                //}
                                //if (receive.RPBankAmount > 0 || receive.RPNEFTAmount > 0)
                                //{
                                //    prefix = "BP/" + fyear + "/";
                                //}
                                //if (Fyid == 9)
                                //{
                                var prefixList = db.Prefixes.Where(p => p.UserId == p.UserId && p.CompanyId == companyid && p.BranchId == assignedBranch && p.DefaultPrefix == "VP").FirstOrDefault();
                                if (prefixList != null)
                                {
                                    if (prefixList.SetPrefix != null)
                                    {
                                        prefix = "VP/" + prefixList.SetPrefix + "/" + fyear + "/";
                                    }
                                    else
                                    {
                                        ViewBag.Error = "Please Set Prefix and then proceed.";
                                        return View(receive);
                                    }
                                }
                                else
                                {
                                    ViewBag.Error = "Please Set Prefix and then proceed.";
                                    return View(receive);
                                    //prefix =  "VP/" + fyear + "/";
                                }
                                //}
                                //else
                                //{
                                //    prefix = "VP/" + fyear + "/";
                                //}
                                if (db.ReceiptPayments.Where(d => (d.transactionType == "General Receive" || d.transactionType == "General Payment" || d.transactionType == "Cash Invoice") && d.CompanyId == companyid && d.BranchId == assignedBranch && d.UserId == userid && d.fYearId == Fyid && d.Prefix == prefix).FirstOrDefault() != null)
                                {
                                    vouch = (db.ReceiptPayments.Where(d => (d.transactionType == "General Receive" || d.transactionType == "General Payment" || d.transactionType == "Cash Invoice") && d.CompanyId == companyid && d.BranchId == assignedBranch && d.UserId == userid && d.fYearId == Fyid && d.Prefix == prefix).Max(v => (int?)v.VoucherNo) ?? 0) + 1;

                                }

                                break;
                            }
                        }
                    }
                    decimal totalcash = 0;
                    decimal totalbank = 0;
                    decimal totalrtgs = 0;
                    foreach (var item in receive.RPDetails)
                    {
                        if (item.LineTotal > 0 && item.LID > 0)
                        {

                            var recpayment = new ReceiptPayment();

                            recpayment.RPdate = date;
                            recpayment.RPDatetime = DateTime.Now;

                            recpayment.fYearId = Fyid;
                            if (receive.BId == null)
                                recpayment.BranchId = Branchid;
                            else
                                recpayment.BranchId = receive.BId;

                            recpayment.CompanyId = companyid;
                            recpayment.UserId = userid;

                            recpayment.ledgerId = item.LID;


                            if (receive.ModeOfPay == "Cash")
                            {


                                if (receive.RPCashAmount > 0)
                                {
                                    recpayment.RPCashId = Convert.ToInt32(CashId);
                                    recpayment.RPCashAmount = item.Cash;
                                    recpayment.TotalAmount = item.Cash;

                                    totalcash += (decimal)item.Cash;
                                }
                                else
                                {

                                    recpayment.RPCashId = 0;
                                    recpayment.RPCashAmount = 0;
                                    recpayment.TotalAmount = 0;

                                    ViewBag.Error = "Please Enter Cash Amount";
                                    return View(receive);

                                }
                            }
                            else if (receive.ModeOfPay == "Cheque" || receive.ModeOfPay == "NEFT")
                            {
                                if (item.Bank > 0 || item.RTGS > 0)
                                {

                                    if (receive.RPBankId == null)
                                    {
                                        ViewBag.Error = "Please Choose Bank";
                                        return View(receive);
                                    }

                                    recpayment.RPBankId = receive.RPBankId;

                                    if (receive.ModeOfPay == "Cheque")
                                    {

                                        if (receive.chequeNo == null)
                                        {
                                            ViewBag.Error = "Please Enter Cheque No";
                                            return View(receive);
                                        }

                                        recpayment.RPBankAmount = item.Bank;
                                        recpayment.TotalAmount = item.Bank;
                                        totalbank += (decimal)item.Bank;
                                        recpayment.chequeNo = receive.chequeNo;
                                        recpayment.chequeDetails = receive.chequeDetails;
                                        if (Convert.ToString(collection["cdate"]) == "")
                                        {
                                            ViewBag.Error = "Please Enter Cheque Date";
                                            return View(receive);
                                        }
                                        else
                                        {
                                            var chequedate = DateTime.ParseExact(Convert.ToString(collection["cdate"]), dateFormat, CultureInfo.CreateSpecificCulture(culture));
                                            recpayment.chequeDate = chequedate;
                                        }
                                    }
                                    else
                                    {
                                        if (receive.NeftRtgsNo == null)
                                        {
                                            ViewBag.Error = "Please Enter Neft/Rtgs No";
                                            return View(receive);
                                        }

                                        recpayment.NeftRtgsNo = receive.NeftRtgsNo;
                                        recpayment.RPBankAmount = item.RTGS;
                                        recpayment.TotalAmount = item.RTGS;
                                        totalrtgs += (decimal)item.RTGS;
                                    }
                                }
                                else
                                {
                                    recpayment.RPBankId = 0;
                                    recpayment.RPBankAmount = 0;
                                    recpayment.TotalAmount = 0;
                                    ViewBag.Error = "Please Enter Amount";
                                    return View(receive);
                                }
                            }
                            else
                            {
                                ViewBag.Error = "Please Select A Mode of Pay";
                                return View(receive);
                            }

                            recpayment.transactionType = InventoryConst.cns_General_Payment;
                            recpayment.RPType = InventoryConst.Cns_Payment;


                            recpayment.VoucherNo = vouch;
                            recpayment.Prefix = prefix;
                            recpayment.ModeOfPay = receive.ModeOfPay;
                            recpayment.transactionNo = TransNo;
                            recpayment.CreditCardNo = receive.CreditCardNo;
                            recpayment.Remarks = receive.Remarks;
                            recpayment.IsBillWise = receive.IsBillWise;
                            recpayment.CreatedBy = Createdby;
                            db.ReceiptPayments.Add(recpayment);
                            db.SaveChanges();
                            receive.Id = recpayment.Id;
                        }
                        else
                        {
                            ViewBag.Error = "Ledger Name cannot be blank in any line.";
                            return View(receive);
                        }

                    }


                    if (receive.IsBillWise == true)
                    {
                        var getPayment = db.ReceiptPayments.Where(d => d.transactionNo == TransNo).FirstOrDefault();
                        var selectedBill = receive.BillList.Where(d => d.Id != 0);
                        foreach (var bill in selectedBill)
                        {
                            var billwisePayment = new BillWisePayment();
                            //billwisePayment.Id = getPayment.Id;
                            billwisePayment.Type = "P";
                            billwisePayment.InvoiceId = bill.Id;
                            billwisePayment.InvoiceNo = bill.Name;
                            billwisePayment.ReceiptPaymentId = getPayment.Id;
                            billwisePayment.BillAmount = bill.BillAmount;
                            billwisePayment.Paid = bill.Paid;
                            billwisePayment.Due = bill.Due;
                            db.BillWisePayments.Add(billwisePayment);
                            db.SaveChanges();
                            var totalPay = db.BillWisePayments.Where(r => r.InvoiceId == bill.Id).Sum(s => s.Paid);
                            var invoice = db.PurchaseInvoices.Where(r => r.Id == bill.Id).FirstOrDefault();
                            if (invoice.GrandTotal == totalPay)
                            {
                                invoice.IsPaid = true;
                                db.SaveChanges();
                            }
                        }

                    }

                    //db.SaveChanges();
                    scope.Complete();
                    return RedirectToAction("PaymentVoucherList", new { Msg = "Payment Voucher No." + prefix + vouch + " generated successfully." });
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
                    return RedirectToAction("CreatePaymentVoucher", new { Err = InventoryMessage.InsertError });

                }
                catch (DataException)
                {
                    //Log the error (add a variable name after DataException)
                    ViewBag.Error = "Error:Data  not Saved Successfully.......";
                    return RedirectToAction("CreatePaymentVoucher", new { Err = InventoryMessage.InsertError });

                }
                catch (Exception exp)
                {
                    return RedirectToAction("CreatePaymentVoucher", new { Err = InventoryMessage.InsertError });

                }
            }

        }

        [HttpGet]
        public ActionResult EditPaymentVoucher(int? id = 0)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            int fyid = Convert.ToInt32(Session["fid"]);
            int Createdby = Convert.ToInt32(Session["Createdid"]);
            ViewBag.Createdby = Createdby;

            var culture = Session["DateCulture"].ToString();
            var dateFormat = Session["DateFormat"].ToString();

            var ledgerList = db.LedgerMasters.Where(d => d.CompanyId == companyid).Select(d => new { Id = d.LID, Name = d.ledgerName }).ToList();

            var bankList = new List<Taxname>();

            var blist = db.Banks.Where(d => d.CompanyId == companyid).Select(d => new Taxname { Id = d.LId, Name = d.Name });
            bankList.AddRange(blist);
            ViewBag.Bank = bankList;
            var branchList = db.BranchMasters.Where(d => d.CompanyId == companyid).ToList();
            ViewBag.branchList = branchList;
            ViewBag.BranchId = Branchid;
            var list = new SelectList(new[]
                                     {
                                              new {ID="Cash",Name="Cash"},
                                              new{ID="Cheque",Name="Cheque"},
                                              new{ID="NEFT",Name="Neft/Rtgs"}

                                          },
                       "ID", "Name");
            ViewData["mode"] = list;

            var receivemv = new ReceiptPaymentModelView();
            List<ReceiptPaymentDetailModelView> receivedetailmvList = new List<ReceiptPaymentDetailModelView>();
            var rec = db.ReceiptPayments.Where(d => d.Id == id && d.CompanyId == companyid).FirstOrDefault();
            ViewBag.VoucherNo = rec.Prefix + rec.VoucherNo;
            if (rec.ModeOfPay == "Cash")
            {
                ViewBag.ModeOfPay = "Cash";
            }
            else if (rec.ModeOfPay == "Cheque")
            {
                receivemv.RPBankId = rec.RPBankId;
                receivemv.chequeNo = rec.chequeNo;
                receivemv.chequeDetails = rec.chequeDetails;
                receivemv.cdate = rec.chequeDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                ViewBag.ModeOfPay = "Cheque";
            }
            else
            {
                receivemv.RPBankId = rec.RPBankId;
                receivemv.NeftRtgsNo = rec.NeftRtgsNo;
                ViewBag.ModeOfPay = "NEFT";
            }
            receivemv.CardName = rec.RPdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
            receivemv.transactionNo = rec.transactionNo;
            receivemv.Remarks = rec.Remarks;
            receivemv.CreditCardNo = rec.CreditCardNo;
            var recdetails = db.ReceiptPayments.Where(d => d.transactionNo == rec.transactionNo && d.CompanyId == companyid).ToList();
            decimal totalcash = 0;
            decimal totalbank = 0;
            decimal totalrtgs = 0;

            foreach (var recdet in recdetails)
            {
                var receivedetailmv = new ReceiptPaymentDetailModelView();

                receivedetailmv.LID = (int)recdet.ledgerId;
                receivedetailmv.LName = ledgerList.Where(d => d.Id == receivedetailmv.LID).Select(d => d.Name).FirstOrDefault();
                if (recdet.ModeOfPay == "Cash")
                {
                    receivedetailmv.Cash = recdet.RPCashAmount;
                    receivedetailmv.LineTotal = recdet.RPCashAmount ?? 0;
                    totalcash += receivedetailmv.LineTotal;
                }
                else if (recdet.ModeOfPay == "Cheque")
                {
                    receivedetailmv.Bank = recdet.RPBankAmount;
                    receivedetailmv.LineTotal = recdet.RPBankAmount ?? 0;
                    totalbank += receivedetailmv.LineTotal;
                }
                else
                {
                    receivedetailmv.RTGS = recdet.RPBankAmount;
                    receivedetailmv.LineTotal = recdet.RPBankAmount ?? 0;
                    totalrtgs += receivedetailmv.LineTotal;
                }

                receivedetailmvList.Add(receivedetailmv);
            }
            if (rec.ModeOfPay == "Cash")
                receivemv.RPCashAmount = totalcash;
            if (rec.ModeOfPay == "Cheque")
                receivemv.RPBankAmount = totalbank;
            if (rec.ModeOfPay == "NEFT")
                receivemv.RPNEFTAmount = totalrtgs;

            if (rec.IsBillWise == true)
            {
                receivemv.IsBillWise = true;
            }
            else
            {
                receivemv.IsBillWise = false;
            }

            receivemv.BId = rec.BranchId;
            receivemv.RPDetails = receivedetailmvList;
            return View(receivemv);
        }

        [HttpPost]
        public ActionResult EditPaymentVoucher(ReceiptPaymentModelView receive, FormCollection collection)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(Session["DateCulture"].ToString());
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            int Fyid = Convert.ToInt32(Session["fid"]);
            int Createdby = Convert.ToInt32(Session["Createdid"]);
            ViewBag.Createdby = Createdby;
            var CashId = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.ledgerID == "2040").Select(d => d.LID).FirstOrDefault();
            ViewBag.ModeOfPay = receive.ModeOfPay;

            var bankList = new List<Taxname>();

            var blist = db.Banks.Where(d => d.CompanyId == companyid).Select(d => new Taxname { Id = d.LId, Name = d.Name });
            bankList.AddRange(blist);
            ViewBag.Bank = bankList;
            var branchList = db.BranchMasters.Where(d => d.CompanyId == companyid).ToList();
            ViewBag.branchList = branchList;
            ViewBag.BranchId = Branchid;
            var list = new SelectList(new[]
                                     {
                                              new {ID="Cash",Name="Cash"},
                                              new{ID="Cheque",Name="Cheque"},
                                              new{ID="NEFT",Name="Neft/Rtgs"}

                                          },
                       "ID", "Name");
            ViewData["mode"] = list;

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

                    var date = DateTime.ParseExact(receive.CardName, dateFormat, CultureInfo.CreateSpecificCulture(culture));

                    var getDateRange = db.FinancialYearMasters.Where(d => d.fYearID == Fyid).FirstOrDefault();
                    if (!(date >= getDateRange.sDate && date <= getDateRange.eDate))
                    {
                        ViewBag.Error = "Voucher Date out of scope of " + getDateRange.Year + " Financial Year.";
                        return View(receive);
                    }

                    var voucher = db.ReceiptPayments.Where(d => d.CompanyId == companyid && d.transactionNo == receive.transactionNo).FirstOrDefault();
                    if (voucher.ModeOfPay == "Cash" || receive.ModeOfPay == "Cash")
                    {
                        if (voucher.ModeOfPay != receive.ModeOfPay)
                        {
                            ViewBag.Error = "Mode of Payment of Voucher cannot be changed.";
                            return View(receive);
                        }
                    }
                    if (receive.ModeOfPay == "Cash")
                    {
                        if (!(receive.RPCashAmount > 0))
                        {
                            ViewBag.Error = "Cash Amount must be greater than zero.";
                            return View(receive);
                        }
                    }

                    if (receive.ModeOfPay == "Cheque")
                    {
                        if (!(receive.RPBankAmount > 0))
                        {
                            ViewBag.Error = "Bank Amount must be greater than zero.";
                            return View(receive);
                        }
                    }
                    if (receive.ModeOfPay == "NEFT")
                    {
                        if (!(receive.RPNEFTAmount > 0))
                        {
                            ViewBag.Error = "RTGS Amount must be greater than zero.";
                            return View(receive);
                        }
                    }


                    var removeBillPayments = db.BillWisePayments.Where(d => d.ReceiptPaymentId == receive.Id);
                    foreach (var removeBill in removeBillPayments)
                    {
                        var invoice = db.PurchaseInvoices.Where(r => r.Id == removeBill.InvoiceId).FirstOrDefault();
                        if (invoice.IsPaid == true)
                        {
                            invoice.IsPaid = false;
                        }
                        db.BillWisePayments.Remove(removeBill);
                       
                    }
                    db.SaveChanges();

                    var removeOpeningPayments = db.OpBalancePaymentDetails.Where(d => d.ReceiptId == receive.Id);
                    foreach (var removeBill in removeOpeningPayments)
                    {
                        var invoice = db.OpeningBalancePayments.Where(r => r.Id == removeBill.OpBalanceId).FirstOrDefault();
                        if (invoice.IsPaid == true)
                        {
                            invoice.IsPaid = false;
                        }
                        db.OpBalancePaymentDetails.Remove(removeBill);

                    }
                    db.SaveChanges();

                    var removeoldReceipts = db.ReceiptPayments.Where(d => d.CompanyId == companyid && d.transactionNo == receive.transactionNo).ToList();
                    foreach (var removeoldReceipt in removeoldReceipts)
                    {
                        db.ReceiptPayments.Remove(removeoldReceipt);
                    }
                    db.SaveChanges();
                    int? assignedBranch = 0;
                    if (receive.BId == null)
                        assignedBranch = Branchid;
                    else
                        assignedBranch = receive.BId;

                    string prefix = voucher.Prefix;
                    int? vouch = voucher.VoucherNo;
                    if ((voucher.fYearId == Fyid) && (voucher.BranchId != assignedBranch))
                    {



                        var fyear = db.FinancialYearMasters.Where(d => d.fYearID == Fyid).Select(d => d.Year).FirstOrDefault();
                        var fs = fyear.Substring(2, 2);
                        var es = fyear.Substring(7, 2);
                        fyear = fs + "-" + es;

                        // var voucher = new Voucher();


                        if (receive.RPDetails.Count() > 0)
                        {
                            foreach (var item in receive.RPDetails)
                            {
                                if (item.LineTotal > 0 && item.LID > 0)
                                {
                                    //if (receive.RPCashAmount > 0)
                                    //{
                                    //    prefix = "CP/" + fyear + "/";
                                    //}
                                    //if (receive.RPBankAmount > 0 || receive.RPNEFTAmount > 0)
                                    //{
                                    //    prefix = "BP/" + fyear + "/";
                                    //}
                                    //if (Fyid == 9)
                                    //{
                                    var prefixList = db.Prefixes.Where(p => p.UserId == p.UserId && p.CompanyId == companyid && p.BranchId == assignedBranch && p.DefaultPrefix == "VP").FirstOrDefault();
                                    if (prefixList != null)
                                    {
                                        if (prefixList.SetPrefix != null)
                                        {
                                            prefix = "VP/" + prefixList.SetPrefix + "/" + fyear + "/";
                                        }
                                        else
                                        {
                                            ViewBag.Error = "Please Set Prefix and then proceed.";
                                            return View(receive);
                                        }
                                    }
                                    else
                                    {
                                        ViewBag.Error = "Please Set Prefix and then proceed.";
                                        return View(receive);
                                        //prefix =  "VP/" + fyear + "/";
                                    }
                                    //}
                                    //else
                                    //{
                                    //    prefix = "VP/" + fyear + "/";
                                    //}
                                    //  prefix = "VP/" + fyear + "/";

                                    if (db.ReceiptPayments.Where(d => (d.transactionType == "General Receive" || d.transactionType == "General Payment" || d.transactionType == "Cash Invoice") && d.CompanyId == companyid && d.BranchId == assignedBranch && d.UserId == userid && d.fYearId == Fyid && d.Prefix == prefix).FirstOrDefault() != null)
                                    {
                                        vouch = (db.ReceiptPayments.Where(d => (d.transactionType == "General Receive" || d.transactionType == "General Payment" || d.transactionType == "Cash Invoice") && d.CompanyId == companyid && d.BranchId == assignedBranch && d.UserId == userid && d.fYearId == Fyid && d.Prefix == prefix).Max(v => (int?)v.VoucherNo) ?? 0) + 1;

                                    }

                                    break;
                                }
                            }
                        }
                    }



                    decimal totalcash = 0;
                    decimal totalbank = 0;
                    decimal totalrtgs = 0;
                    string voucherNo = "";
                    foreach (var item in receive.RPDetails)
                    {

                        if (item.LineTotal > 0 && item.LID > 0)
                        {

                            var recpayment = new ReceiptPayment();

                            recpayment.RPdate = date;
                            recpayment.RPDatetime = voucher.RPDatetime;

                            recpayment.fYearId = voucher.fYearId;
                            if (receive.BId == null)
                                recpayment.BranchId = Branchid;
                            else
                                recpayment.BranchId = receive.BId;

                            recpayment.CompanyId = voucher.CompanyId;
                            recpayment.UserId = voucher.UserId;

                            recpayment.ledgerId = item.LID;
                            recpayment.IsBillWise = receive.IsBillWise;

                            if (receive.ModeOfPay == "Cash")
                            {


                                if (receive.RPCashAmount > 0)
                                {
                                    //long CashId = 35;

                                    recpayment.RPCashId = Convert.ToInt32(CashId);
                                    recpayment.RPCashAmount = item.Cash;
                                    recpayment.TotalAmount = item.Cash;

                                    totalcash += (decimal)item.Cash;
                                }
                                else
                                {

                                    recpayment.RPCashId = 0;
                                    recpayment.RPCashAmount = 0;
                                    recpayment.TotalAmount = 0;

                                    ViewBag.Error = "Please Enter Cash Amount";
                                    return View(receive);

                                }
                            }
                            else if (receive.ModeOfPay == "Cheque" || receive.ModeOfPay == "NEFT")
                            {
                                if (item.Bank > 0 || item.RTGS > 0)
                                {

                                    if (receive.RPBankId == null)
                                    {
                                        ViewBag.Error = "Please Choose Bank";
                                        return View(receive);
                                    }

                                    recpayment.RPBankId = receive.RPBankId;

                                    if (receive.ModeOfPay == "Cheque")
                                    {

                                        if (receive.chequeNo == null)
                                        {
                                            ViewBag.Error = "Please Enter Cheque No";
                                            return View(receive);
                                        }

                                        recpayment.RPBankAmount = item.Bank;
                                        recpayment.TotalAmount = item.Bank;
                                        totalbank += (decimal)item.Bank;
                                        recpayment.chequeNo = receive.chequeNo;
                                        recpayment.chequeDetails = receive.chequeDetails;
                                        if (Convert.ToString(collection["cdate"]) == "")
                                        {
                                            ViewBag.Error = "Please Enter Cheque Date";
                                            return View(receive);
                                        }
                                        else
                                        {
                                            var chequedate = DateTime.ParseExact(Convert.ToString(collection["cdate"]), dateFormat, CultureInfo.CreateSpecificCulture(culture));
                                            recpayment.chequeDate = chequedate;
                                        }
                                    }
                                    else
                                    {
                                        if (receive.NeftRtgsNo == null)
                                        {
                                            ViewBag.Error = "Please Enter Neft/Rtgs No";
                                            return View(receive);
                                        }

                                        recpayment.NeftRtgsNo = receive.NeftRtgsNo;
                                        recpayment.RPBankAmount = item.RTGS;
                                        recpayment.TotalAmount = item.RTGS;
                                        totalrtgs += (decimal)item.RTGS;
                                    }
                                }
                                else
                                {
                                    recpayment.RPBankId = 0;
                                    recpayment.RPBankAmount = 0;
                                    recpayment.TotalAmount = 0;
                                    ViewBag.Error = "Please Enter Amount";
                                    return View(receive);
                                }
                            }
                            else
                            {
                                ViewBag.Error = "Please Select A Mode of Pay";
                                return View(receive);
                            }

                            recpayment.transactionType = InventoryConst.cns_General_Payment;
                            recpayment.RPType = InventoryConst.Cns_Payment;

                            if (voucher.Prefix == prefix && voucher.VoucherNo == vouch)
                            {
                                recpayment.VoucherNo = voucher.VoucherNo;
                                recpayment.Prefix = voucher.Prefix;
                                voucherNo = voucher.Prefix + voucher.VoucherNo;
                            }
                            else
                            {
                                recpayment.VoucherNo = vouch;
                                recpayment.Prefix = prefix;
                                voucherNo = prefix + vouch;
                            }

                            recpayment.ModeOfPay = receive.ModeOfPay;
                            recpayment.transactionNo = voucher.transactionNo;
                            recpayment.Remarks = receive.Remarks;
                            recpayment.CreditCardNo = receive.CreditCardNo;



                            recpayment.CreatedBy = voucher.CreatedBy;

                            recpayment.ModifiedBy = Createdby;
                            recpayment.ModifiedOn = DateTime.Now;
                            db.ReceiptPayments.Add(recpayment);
                            db.SaveChanges();
                        }
                        else
                        {
                            ViewBag.Error = "Ledger Name cannot be blank in any line.";
                            return View(receive);
                        }

                    }


                    if (receive.IsBillWise == true)
                    {
                        var getPayment = db.ReceiptPayments.Where(d => d.transactionNo == voucher.transactionNo).FirstOrDefault();
                        var selectedBill = receive.BillList.Where(d => d.Id != 0);
                        foreach (var bill in selectedBill)
                        {
                            var findDuplicate = db.BillWisePayments.Where(d => d.InvoiceId == bill.Id && d.BillAmount == bill.BillAmount && d.Due == bill.Due).FirstOrDefault();
                            if (findDuplicate == null)
                            {
                                var billwisePayment = new BillWisePayment();
                                //billwiseReceive.Id = getPayment.Id;
                                billwisePayment.Type = "P";
                                billwisePayment.InvoiceId = bill.Id;
                                billwisePayment.ReceiptPaymentId = getPayment.Id;
                                billwisePayment.InvoiceNo = bill.Name;
                                billwisePayment.BillAmount = bill.BillAmount;
                                billwisePayment.Paid = bill.Paid;
                                billwisePayment.Due = bill.Due;
                                db.BillWisePayments.Add(billwisePayment);
                                db.SaveChanges();
                                var totalPay = db.BillWisePayments.Where(r => r.InvoiceId == bill.Id).Sum(s => s.Paid);
                                var invoice = db.PurchaseInvoices.Where(r => r.Id == bill.Id).FirstOrDefault();
                                if (invoice.GrandTotal == totalPay)
                                {
                                    invoice.IsPaid = true;
                                    db.SaveChanges();
                                }
                            }
                        }

                    }



              

                    db.SaveChanges();
                    scope.Complete();
                    return RedirectToAction("PaymentVoucherList", new { Msg = "Payment Voucher No." + voucherNo + " updated successfully." });
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
                   // return RedirectToAction("CreatePaymentVoucher", new { Err = InventoryMessage.InsertError });
                    throw;
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    return RedirectToAction("CreatePaymentVoucher", new { Err = InventoryMessage.InsertError });

                }
                //catch (DataException Exp)
                //{
                //    //Log the error (add a variable name after DataException)
                //    ViewBag.Error = "Error:Data  not Saved Successfully.......";
                //    return RedirectToAction("CreatePaymentVoucher", new { Err = InventoryMessage.InsertError });

                //}
                //catch (Exception exp)
                //{
                //    return RedirectToAction("CreatePaymentVoucher", new { Err = InventoryMessage.InsertError });

                //}
            }

        }

        [HttpGet]
        public ActionResult PrintReceiveVoucherPDF(long? id = 0)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            int fyid = Convert.ToInt32(Session["fid"]);

            var culture = Session["DateCulture"].ToString();
            var dateFormat = Session["DateFormat"].ToString();
            var ledgerList = db.LedgerMasters.Where(d => d.CompanyId == companyid).Select(d => new { Id = d.LID, Name = d.ledgerName }).ToList();

            var receivemv = new ReceiptPaymentModelView();
            var numtowords = new NumberToEnglish();

            var rec = db.ReceiptPayments.Where(d => d.Id == id && d.CompanyId == companyid).FirstOrDefault();
            ViewBag.VoucherNo = rec.Prefix + rec.VoucherNo;
            ViewBag.LedgerName = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.LID == rec.ledgerId).Select(d => d.ledgerName).FirstOrDefault();
            ViewBag.CompanyName = db.Companies.Select(d => d.Name).FirstOrDefault();
            decimal total = db.ReceiptPayments.Where(d => d.transactionNo == rec.transactionNo && d.CompanyId == companyid).Sum(d => d.TotalAmount) ?? 0;
            ViewBag.TotalAmtInWord = numtowords.changeNumericToWords(total);
            ViewBag.TotalAmount = total;
            ViewBag.remarks = rec.Remarks;

            if (rec.ModeOfPay == "Cash")
            {
                ViewBag.ModeOfPay = "Cash";
            }
            else if (rec.ModeOfPay == "Cheque")
            {
                receivemv.chequeNo = rec.chequeNo;
                receivemv.chequeDetails = rec.chequeDetails;
                receivemv.cdate = rec.chequeDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                ViewBag.BankName = db.Banks.Where(r => r.LId == rec.RPBankId && r.CompanyId == companyid).Select(d => d.Name).FirstOrDefault();
                ViewBag.ModeOfPay = "Cheque";
            }
            else
            {
                receivemv.NeftRtgsNo = rec.NeftRtgsNo;
                ViewBag.BankName = db.Banks.Where(r => r.LId == rec.RPBankId && r.CompanyId == companyid).Select(d => d.Name).FirstOrDefault();
                ViewBag.ModeOfPay = "NEFT";
            }
            receivemv.CardName = rec.RPdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture)); //voucher date
            //var currencyDenomination = db.CurrencyDenominations.Where(d => d.transactionNo == rec.transactionNo).FirstOrDefault();
            //if (currencyDenomination != null)
            //{
            //    receivemv.TwoThousand = currencyDenomination.TwoThousand;
            //    receivemv.FiveHundred = currencyDenomination.FiveHundred;
            //    receivemv.TwoHundred = currencyDenomination.TwoHundred;
            //    receivemv.OneHundred = currencyDenomination.OneHundred;
            //    receivemv.Fifty = currencyDenomination.Fifty;
            //    receivemv.Twenty = currencyDenomination.Twenty;
            //    receivemv.Ten = currencyDenomination.Ten;
            //    receivemv.Five = currencyDenomination.Five;
            //    receivemv.Two = currencyDenomination.Two;
            //    receivemv.One = currencyDenomination.One;
            //}
            //else
            //{
            //    ViewBag.Deno = "None";
            //}
            var breakup = db.ReceiptPayments.Where(d => d.transactionNo == rec.transactionNo && d.CompanyId == companyid).OrderBy(d => d.Id).ToList();
            int c = 0;
            List<TaxComponent_Ret> breakupList = new List<TaxComponent_Ret>();
            foreach (var item in breakup)
            {
                if (c == 0)
                {
                    var l1 = new TaxComponent_Ret();
                    l1.TaxName = "Amount Credited to Sundry Debtors(Customer) A/C ";
                    l1.Amount = item.TotalAmount ?? 0;
                    breakupList.Add(l1);
                    c++;
                }
                else
                {
                    var l2 = new TaxComponent_Ret();
                    l2.TaxName = "Amount Credited to TCS (Sale of Goods) Payable A/C ";
                    l2.Amount = item.TotalAmount ?? 0;
                    breakupList.Add(l2);
                }
            }
            var l3 = new TaxComponent_Ret();
            l3.TaxName = "Total ";
            l3.Amount = total;
            breakupList.Add(l3);

            if (breakupList.Count() > 2)
            {
                ViewBag.BreakUp = breakupList;
            }
            return View(receivemv);
        }
        public ActionResult PrintPaymentVoucherPDF(long? id = 0)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            int fyid = Convert.ToInt32(Session["fid"]);

            var culture = Session["DateCulture"].ToString();
            var dateFormat = Session["DateFormat"].ToString();
            var ledgerList = db.LedgerMasters.Where(d => d.CompanyId == companyid).Select(d => new { Id = d.LID, Name = d.ledgerName }).ToList();

            var receivemv = new ReceiptPaymentModelView();
            var numtowords = new NumberToEnglish();

            var rec = db.ReceiptPayments.Where(d => d.Id == id && d.CompanyId == companyid).FirstOrDefault();
            ViewBag.VoucherNo = rec.Prefix + rec.VoucherNo;
            ViewBag.LedgerName = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.LID == rec.ledgerId).Select(d => d.ledgerName).FirstOrDefault();
            ViewBag.TotalAmtInWord = numtowords.changeNumericToWords(rec.TotalAmount.Value);
            ViewBag.TotalAmount = rec.TotalAmount;
            ViewBag.CompanyName = db.Companies.Select(d => d.Name).FirstOrDefault();

            if (rec.ModeOfPay == "Cash")
            {
                ViewBag.ModeOfPay = "Cash";
            }
            else if (rec.ModeOfPay == "Cheque")
            {
                receivemv.chequeNo = rec.chequeNo;
                receivemv.chequeDetails = rec.chequeDetails;
                receivemv.cdate = rec.chequeDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                ViewBag.BankName = db.Banks.Where(r => r.LId == rec.RPBankId && r.CompanyId == companyid).Select(d => d.Name).FirstOrDefault();
                ViewBag.ModeOfPay = "Cheque";
            }
            else
            {
                ViewBag.BankName = db.Banks.Where(r => r.LId == rec.RPBankId && r.CompanyId == companyid).Select(d => d.Name).FirstOrDefault();
                receivemv.NeftRtgsNo = rec.NeftRtgsNo;
                ViewBag.ModeOfPay = "NEFT";
            }
            if (rec.BranchId == 0)
            {
                ViewBag.branch = "Head Office";
            }
            else
            {
                var branch = db.BranchMasters.Where(r => r.Id == rec.BranchId).Select(s => s.Name).FirstOrDefault();
                ViewBag.branch = branch;
            }
            receivemv.CardName = rec.RPdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));//voucher date
            receivemv.TotalAmount = rec.TotalAmount;
            receivemv.CreditCardNo = rec.CreditCardNo;
            receivemv.Remarks = rec.Remarks;
            return View(receivemv);
        }
        public ActionResult DeletePayment(string transactionNo)
        {
            try
            {

                int userid = Convert.ToInt32(Session["userid"]);
                int companyid = Convert.ToInt32(Session["companyid"]);
                var results = db.ReceiptPayments.Where(d => d.CompanyId == companyid && d.transactionNo == transactionNo).ToList();
                var receiptId = results.FirstOrDefault().Id;
                var removeBillPayments = db.BillWisePayments.Where(d => d.Id == receiptId).ToList();
                foreach (var removeBill in removeBillPayments)
                {
                    db.BillWisePayments.Remove(removeBill);
                }
                // var voucher = db.Vouchers.Where(d => d.Id == result.transactionNo).FirstOrDefault();
                foreach (var result in results)
                {
                    db.ReceiptPayments.Remove(result);
                }

                db.SaveChanges();
                return RedirectToAction("PaymentVoucherList", new { Msg = "Voucher deleted Successfully...." });
            }
            catch
            {


                return RedirectToAction("PaymentVoucherList", new { Err = InventoryMessage.Delte });
            }

        }



        [HttpGet]
        public ActionResult GetPartyInvoice(int LID)
        {
            int Fyid = Convert.ToInt32(Session["fid"]);
            var salesinvoiceList = (from s in db.PurchaseInvoices.Where(d => d.Supplier.LId == LID && d.IsPaid == false && (d.FinancialYearId == Fyid || d.FinancialYearId == (Fyid - 1))).Select(d => new Bill { InvId = d.Id, InvNo = d.NO+"--"+d.Reference, Date = d.InvoiceDate, BillAmount = d.BCGrandTotal }).ToList()
                                        //join r in db.PurchaseReturns.GroupBy(d => d.Reference).Select(d => new Bill { InvId = d.Key ?? 0, InvNo = "", ReturnAmount = d.Sum(t => t.BCGrandTotal) }).ToList()
                                        //on s.InvId equals r.InvId into d
                                        //from sr in d.DefaultIfEmpty()
                                    join b in db.BillWisePayments.Where(d => d.Type == "P").GroupBy(d => d.InvoiceId).Select(d => new Bill { InvId = d.Key, BillAmount = d.Average(s => s.BillAmount), Paid = d.Sum(s => s.Paid) }).ToList()
                                    on s.InvId equals b.InvId into c
                                    from sc in c.DefaultIfEmpty()
                                    where (sc == null || ((sc == null ? 0 : sc.BillAmount) > (sc == null ? 0 : sc.Paid)))
                                    select new Bill
                                    {
                                        InvId = s.InvId,
                                        InvNo = s.InvNo,
                                        Date = s.Date,
                                        BillAmount = s.BillAmount,
                                        //ReturnAmount = sr == null ? 0 : sr.ReturnAmount,
                                        Amount = s.BillAmount,//- (sr == null ? 0 : sr.ReturnAmount),
                                        Due = sc == null ? 0 : ((sc == null ? 0 : sc.BillAmount) - (sc == null ? 0 : sc.Paid))
                                    }).OrderBy(d => d.Date).ToList();


            return Json(salesinvoiceList, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetPartyInvoiceEdit(int LID, int RPId)

        {


            int Fyid = Convert.ToInt32(Session["fid"]);
            List<Bill> billList = new List<Bill>();
            //var paidbill = db.BillWisePayments.Where(d => d.Id == RPId).Select(d => new Bill { InvId = d.InvoiceId, InvNo = d.InvoiceNo, Status = true }).ToList();
            var salesinvoiceList = (from s in db.PurchaseInvoices.Where(d => d.Supplier.LId == LID && d.IsPaid == false && (d.FinancialYearId == Fyid || d.FinancialYearId == (Fyid - 1))).Select(d => new Bill { InvId = d.Id, InvNo = d.NO+"--"+d.Reference, Date = d.InvoiceDate, BillAmount = d.BCGrandTotal }).ToList()
                                        //join r in db.SalesReturns.GroupBy(d => d.ReferenceNo).Select(d => new Bill { InvId = d.Key ?? 0, InvNo = "", ReturnAmount = d.Sum(t => t.BCGrandTotal) }).ToList()
                                        //on s.InvId equals r.InvId into d
                                        //from sr in d.DefaultIfEmpty()
                                    join b in db.BillWisePayments.Where(d => d.Type == "P").GroupBy(d => d.InvoiceId).Select(d => new Bill { InvId = d.Key, BillAmount = d.Average(s => s.BillAmount), Paid = d.Sum(s => s.Paid) }).ToList()
                                    on s.InvId equals b.InvId into c
                                    from sc in c.DefaultIfEmpty()
                                    where (sc == null || ((sc == null ? 0 : sc.BillAmount) > (sc == null ? 0 : sc.Paid)))
                                    select new Bill
                                    {
                                        InvId = s.InvId,
                                        InvNo = s.InvNo,
                                        Date = s.Date,
                                        Status = sc == null ? false : true,
                                        BillAmount = s.BillAmount,
                                        //ReturnAmount = sr == null ? 0 : sr.ReturnAmount,
                                        Amount = s.BillAmount,//- (sr == null ? 0 : sr.ReturnAmount),
                                        Paid = sc == null ? 0 : sc.Paid,
                                        Due = sc == null ? 0 : ((sc == null ? 0 : sc.BillAmount) - (sc == null ? 0 : sc.Paid))
                                    }).OrderBy(d => d.Date).ToList();
            //List<long> paidBill = new List<long>();
            //paidBill = salesinvoiceList.Select(d => d.InvId).ToList();
            var paidBill = (from b in db.BillWisePayments.Where(d => d.Type == "P" && d.ReceiptPaymentId == RPId).Select(d => new { InvId = d.InvoiceId, InvNo = d.InvoiceNo, Paid = 0, BillAmount = 0, Amount = 0, Status = true })
                            join c in db.BillWisePayments.GroupBy(d => new { d.InvoiceId, d.InvoiceNo }).Select(d => new { InvId = d.Key.InvoiceId, InvNo = d.Key.InvoiceNo, Paid = d.Sum(p => p.Paid), BillAmount = d.Average(p => p.BillAmount), Amount = d.Average(p => p.BillAmount), Status = true })
                            on b.InvId equals c.InvId
                            where (c.BillAmount == c.Paid)
                            select new Bill { InvId = c.InvId, InvNo = c.InvNo, Paid = c.Paid, BillAmount = c.BillAmount, Amount = c.Amount, Due = 0, Status = true }).ToList();
            //   var fullpaidbill = paidBill.Where(d => d.BillAmount != d.Paid).ToList();

            billList.AddRange(paidBill);
            billList.AddRange(salesinvoiceList);
            return Json(billList, JsonRequestBehavior.AllowGet);
        }

        #endregion



        public ActionResult UpdatePaymentVouchar(string FromDate,string DateTo)
        {

            var scope = new TransactionScope(
                        TransactionScopeOption.RequiresNew,
                        new TransactionOptions()
                        {
                            IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                        }
                    );

            using (scope)
            {
                int Fyid = Convert.ToInt32(Session["fid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                DateTime? frmDt = DateTime.ParseExact(FromDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                DateTime? toDt = DateTime.ParseExact(DateTo, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var totalPaymentVoucher = db.ReceiptPayments.Where(r => r.fYearId == Fyid && r.RPType == "Payment" && r.RPdate>= frmDt && r.RPdate<=toDt).GroupBy(r => r.ledgerId);


                // RECEIPT PAYMENT LOOP
                foreach (var item  in totalPaymentVoucher)
                {
                    var supplierId = db.Suppliers.Where(r => r.LId == item.Key).Select(s => s.Id).FirstOrDefault();

                    if (db.OpeningBalancePayments.Any(a => a.LedgerId == item.Key) == false)
                    {

                        var openingBalance = db.OpeningBalances.Where(r => r.ledgerID == item.Key && r.fYearID == Fyid).Count() > 0 ? db.OpeningBalances.Where(r => r.ledgerID == item.Key && r.fYearID == Fyid).Sum(s => s.openingBal) : 0;
                        if (openingBalance != 0)
                        {
                            if (openingBalance < 0)
                            {
                                OpeningBalancePayment paymentOp = new OpeningBalancePayment();
                                paymentOp.Fyear = Fyid;
                                paymentOp.OpeningBalance = (openingBalance * -1);
                                paymentOp.LedgerId = item.Key;
                                paymentOp.IsPaid = false;
                                db.OpeningBalancePayments.Add(paymentOp);
                            }
                            else
                            {
                                OpeningBalancePayment paymentOp = new OpeningBalancePayment();
                                paymentOp.Fyear = Fyid;
                                paymentOp.OpeningBalance = openingBalance;
                                paymentOp.LedgerId = item.Key;
                                paymentOp.IsPaid = false;
                                db.OpeningBalancePayments.Add(paymentOp);
                            }

                            db.SaveChanges();
                        }
                    }

                    // INDIVIDUAL RECEIPT PAYMENT LOOP
                    foreach (var item1 in item)
                    {
                        decimal? paidAmount = item1.TotalAmount;
                        var opBalPay = db.OpeningBalancePayments.Where(r => r.IsPaid == false && r.LedgerId == item1.ledgerId).FirstOrDefault();
                        var purchaseInvoice = db.PurchaseInvoices.Where(r => r.FinancialYearId == Fyid && r.IsPaid == false && r.SupplierId == supplierId).ToList();


                        if (opBalPay != null)
                        {
                            var totalOpeningPay = db.OpBalancePaymentDetails.Where(r => r.OpBalanceId == opBalPay.Id).Count() == 0 ? 0 : db.OpBalancePaymentDetails.Where(r => r.OpBalanceId == opBalPay.Id).Sum(s => s.PaidAmount);
                            var reminder = opBalPay.OpeningBalance - totalOpeningPay;
                            if (reminder >= paidAmount)
                            {
                                OpBalancePaymentDetail opDetail = new OpBalancePaymentDetail();
                                opDetail.Type = "P";
                                opDetail.OpBalanceId = opBalPay.Id;
                                opDetail.PaidAmount = item1.TotalAmount;
                                opDetail.ReceiptId = item1.Id;
                                db.OpBalancePaymentDetails.Add(opDetail);
                                db.SaveChanges();
                                var totalOpeningPaid = db.OpBalancePaymentDetails.Where(r => r.OpBalanceId == opBalPay.Id).Sum(s => s.PaidAmount);
                                if (totalOpeningPaid == opBalPay.OpeningBalance)
                                {
                                    opBalPay.IsPaid = true;
                                    db.SaveChanges();
                                }
                            }

                            else
                            {
                                var balanceAmount = paidAmount - reminder;
                                OpBalancePaymentDetail opDetail = new OpBalancePaymentDetail();
                                opDetail.Type = "P";
                                opDetail.OpBalanceId = opBalPay.Id;
                                opDetail.PaidAmount = reminder;
                                opDetail.ReceiptId = item1.Id;

                                db.OpBalancePaymentDetails.Add(opDetail);
                                db.SaveChanges();
                                var totalOpeningPaid = db.OpBalancePaymentDetails.Where(r => r.OpBalanceId == opBalPay.Id).Sum(s => s.PaidAmount);
                                if (totalOpeningPaid == opBalPay.OpeningBalance)
                                {
                                    opBalPay.IsPaid = true;
                                    db.SaveChanges();
                                }

                                foreach (var item2 in purchaseInvoice)
                                {
                                    if (item2.GrandTotal >= balanceAmount)
                                    {
                                        BillWisePayment payment = new BillWisePayment();
                                        payment.InvoiceId = item2.Id;
                                        payment.InvoiceNo = item2.NO;
                                        payment.ReceiptPaymentId = item1.Id;
                                        //payment.Id = item1.Id;
                                        payment.BillAmount = item2.GrandTotal;
                                        payment.Type = "P";
                                        payment.Paid = balanceAmount;
                                        db.BillWisePayments.Add(payment);
                                        db.SaveChanges();
                                        var totalpaidAmount1 = db.BillWisePayments.Where(r => r.InvoiceId == item2.Id).Sum(s => s.Paid);
                                        payment.Due = item2.GrandTotal - totalpaidAmount1;
                                        if (totalpaidAmount1 == item2.GrandTotal)
                                        {
                                            item2.IsPaid = true;
                                            db.SaveChanges();
                                        }
                                        break;
                                    }

                                    else
                                    {
                                        balanceAmount = balanceAmount - item2.GrandTotal;
                                        BillWisePayment payment = new BillWisePayment();
                                        payment.InvoiceId = item2.Id;
                                        payment.InvoiceNo = item2.NO;
                                        payment.ReceiptPaymentId = item1.Id;
                                        payment.Type = "P";
                                        //payment.Id = item1.Id;
                                        payment.BillAmount = item2.GrandTotal;
                                        payment.Paid = item2.GrandTotal;
                                        db.BillWisePayments.Add(payment);
                                        var totalpaidAmount = db.BillWisePayments.Where(r => r.InvoiceId == item2.Id).Sum(s => s.Paid);
                                        payment.Due = item2.GrandTotal - totalpaidAmount;
                                        if (totalpaidAmount == item2.GrandTotal)
                                        {
                                            item2.IsPaid = true;
                                            db.SaveChanges();
                                        }
                                    }
                                    db.SaveChanges();
                                }
                            }
                        }


                        else
                        {
                            foreach (var item2 in purchaseInvoice)
                            {
                                var totalpaidAmount = db.BillWisePayments.Where(r => r.InvoiceId == item2.Id).Sum(s => s.Paid) ?? 0;
                                var balanceAmount = item2.GrandTotal - totalpaidAmount;
                                if (balanceAmount >= paidAmount)
                                {
                                    BillWisePayment payment = new BillWisePayment();
                                    payment.InvoiceId = item2.Id;
                                    payment.InvoiceNo = item2.NO;
                                    payment.ReceiptPaymentId = item1.Id;
                                    //payment.Id= item1.Id;
                                    payment.Type = "P";
                                    payment.BillAmount = item2.GrandTotal;
                                    payment.Paid = paidAmount;
                                    db.BillWisePayments.Add(payment);
                                    db.SaveChanges();
                                    var totalpaidAmount1 = db.BillWisePayments.Where(r => r.InvoiceId == item2.Id).Sum(s => s.Paid);
                                    payment.Due = item2.GrandTotal - totalpaidAmount1;
                                    if (totalpaidAmount1 == item2.GrandTotal)
                                    {
                                        item2.IsPaid = true;
                                        db.SaveChanges();
                                    }
                                    break;
                                }

                                else
                                {
                                    paidAmount = paidAmount - balanceAmount;
                                    BillWisePayment payment = new BillWisePayment();
                                    payment.InvoiceId = item2.Id;
                                    payment.InvoiceNo = item2.NO;
                                    payment.Type = "P";
                                    payment.ReceiptPaymentId = item1.Id;
                                    //payment.Id = item1.Id;
                                    payment.BillAmount = item2.GrandTotal;
                                    payment.Paid = balanceAmount;
                                    db.BillWisePayments.Add(payment);
                                    db.SaveChanges();
                                    var totalpaidAmount1 = db.BillWisePayments.Where(r => r.InvoiceId == item2.Id).Sum(s => s.Paid);
                                    payment.Due = item2.GrandTotal - totalpaidAmount1;
                                    if (totalpaidAmount1 == item2.GrandTotal)
                                    {
                                        item2.IsPaid = true;
                                        db.SaveChanges();
                                    }
                                }
                                db.SaveChanges();

                            }
                        }


                    }

                }

                db.SaveChanges();
                scope.Complete();
                return RedirectToAction("ReceiveVoucherList");
            }
        }


        public ActionResult UpdateReceiveVouchar(string FromDate, string DateTo)
        {

            var scope = new TransactionScope(
                        TransactionScopeOption.RequiresNew,
                        new TransactionOptions()
                        {
                            IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                        }
                    );

            using (scope)
            {
                int Fyid = Convert.ToInt32(Session["fid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                DateTime? frmDt = DateTime.ParseExact(FromDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                DateTime? toDt = DateTime.ParseExact(DateTo, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var totalReceiveVoucher1 = db.ReceiptPayments.Where(r => r.RPdate >= frmDt && r.RPdate <= toDt && r.RPType == "Receive").ToList();
                var totalReceiveVoucher = totalReceiveVoucher1.GroupBy(g => g.ledgerId).ToList();
                var cust = db.Customers;
                var opBal = db.OpeningBalances.Where(r => r.fYearID == Fyid);
                // RECEIPT PAYMENT LOOP
                foreach (var item in totalReceiveVoucher)
                {
                    var customerId = cust.Where(r => r.LId == item.Key).Select(s => s.Id).FirstOrDefault();

                    var openingBalance = opBal.Where(r => r.ledgerID == item.Key).Count() > 0 ? opBal.Where(r => r.ledgerID == item.Key).Sum(s => s.openingBal) : 0;
                    if (openingBalance != 0)
                    {
                        if (openingBalance < 0)
                        {
                            OpeningBalancePayment paymentOp = new OpeningBalancePayment();
                            paymentOp.Fyear = Fyid;
                            paymentOp.OpeningBalance = (openingBalance * -1);
                            paymentOp.LedgerId = item.Key;
                            paymentOp.IsPaid = false;
                            db.OpeningBalancePayments.Add(paymentOp);
                        }
                        else
                        {
                            OpeningBalancePayment paymentOp = new OpeningBalancePayment();
                            paymentOp.Fyear = Fyid;
                            paymentOp.OpeningBalance = openingBalance;
                            paymentOp.LedgerId = item.Key;
                            paymentOp.IsPaid = false;
                            db.OpeningBalancePayments.Add(paymentOp);
                        }

                        db.SaveChanges();
                    }

                    // INDIVIDUAL RECEIPT PAYMENT LOOP
                    foreach (var item1 in item)
                    {
                        decimal? paidAmount = item1.TotalAmount;
                        var opBalPay = db.OpeningBalancePayments.Where(r => r.IsPaid == false && r.LedgerId == item1.ledgerId).FirstOrDefault();
                        var salesInvoice = db.SalesInvoices.Where(r => r.FinancialYearId == Fyid && r.IsPaid == false && r.CustomerId == customerId);


                        if (opBalPay != null)
                        {
                            var totalOpeningPay = db.OpBalancePaymentDetails.Where(r => r.OpBalanceId == opBalPay.Id).Count() == 0 ? 0 : db.OpBalancePaymentDetails.Where(r => r.OpBalanceId == opBalPay.Id).Sum(s => s.PaidAmount);
                            var reminder = opBalPay.OpeningBalance - totalOpeningPay;
                            if (reminder >= paidAmount)
                            {
                                OpBalancePaymentDetail opDetail = new OpBalancePaymentDetail();
                                opDetail.Type = "S";
                                opDetail.OpBalanceId = opBalPay.Id;
                                opDetail.PaidAmount = item1.TotalAmount;
                                opDetail.ReceiptId = item1.Id;
                                db.OpBalancePaymentDetails.Add(opDetail);
                                db.SaveChanges();
                                var totalOpeningPaid = db.OpBalancePaymentDetails.Where(r => r.OpBalanceId == opBalPay.Id).Sum(s => s.PaidAmount);
                                if (totalOpeningPaid == opBalPay.OpeningBalance)
                                {
                                    opBalPay.IsPaid = true;
                                    db.SaveChanges();
                                }
                            }

                            else
                            {
                                var balanceAmount = paidAmount - reminder;
                                OpBalancePaymentDetail opDetail = new OpBalancePaymentDetail();
                                opDetail.Type = "S";
                                opDetail.OpBalanceId = opBalPay.Id;
                                opDetail.PaidAmount = reminder;
                                opDetail.ReceiptId = item1.Id;

                                db.OpBalancePaymentDetails.Add(opDetail);
                                db.SaveChanges();
                                var totalOpeningPaid = db.OpBalancePaymentDetails.Where(r => r.OpBalanceId == opBalPay.Id).Sum(s => s.PaidAmount);
                                if (totalOpeningPaid == opBalPay.OpeningBalance)
                                {
                                    opBalPay.IsPaid = true;
                                    db.SaveChanges();
                                }

                                foreach (var item2 in salesInvoice)
                                {
                                    if (item2.GrandTotal >= balanceAmount)
                                    {
                                        BillWiseReceive receive = new BillWiseReceive();
                                        receive.InvoiceId = item2.Id;
                                        receive.InvoiceNo = item2.NO;
                                        receive.ReceiptPaymentId = item1.Id;
                                        //payment.Id = item1.Id;
                                        receive.BillAmount = item2.GrandTotal;
                                        receive.Type = "S";
                                        receive.Paid = balanceAmount;
                                        db.BillWiseReceives.Add(receive);
                                        db.SaveChanges();
                                        var totalpaidAmount1 = db.BillWiseReceives.Where(r => r.InvoiceId == item2.Id).Sum(s => s.Paid);
                                        receive.Due = item2.GrandTotal - totalpaidAmount1;
                                        if (totalpaidAmount1 == item2.GrandTotal)
                                        {
                                            item2.IsPaid = true;
                                            db.SaveChanges();
                                        }
                                        break;
                                    }

                                    else
                                    {
                                        balanceAmount = balanceAmount - item2.GrandTotal;
                                        BillWiseReceive receive = new BillWiseReceive();
                                        receive.InvoiceId = item2.Id;
                                        receive.InvoiceNo = item2.NO;
                                        receive.ReceiptPaymentId = item1.Id;
                                        receive.Type = "S";
                                        //payment.Id = item1.Id;
                                        receive.BillAmount = item2.GrandTotal;
                                        receive.Paid = item2.GrandTotal;
                                        db.BillWiseReceives.Add(receive);
                                        var totalpaidAmount = db.BillWiseReceives.Where(r => r.InvoiceId == item2.Id).Sum(s => s.Paid);
                                        receive.Due = item2.GrandTotal - totalpaidAmount;
                                        if (totalpaidAmount == item2.GrandTotal)
                                        {
                                            item2.IsPaid = true;
                                            db.SaveChanges();
                                        }
                                    }
                                    db.SaveChanges();
                                }
                            }
                        }


                        else
                        {
                            foreach (var item2 in salesInvoice)
                            {
                                var totalreceiveAmount = db.BillWiseReceives.Where(r => r.InvoiceId == item2.Id).Sum(s => s.Paid) ?? 0;
                                var balanceAmount = item2.GrandTotal - totalreceiveAmount;
                                if (balanceAmount >= paidAmount)
                                {
                                    BillWiseReceive receive = new BillWiseReceive();
                                    receive.InvoiceId = item2.Id;
                                    receive.InvoiceNo = item2.NO;
                                    receive.ReceiptPaymentId = item1.Id;
                                    //payment.Id= item1.Id;
                                    receive.Type = "S";
                                    receive.BillAmount = item2.GrandTotal;
                                    receive.Paid = paidAmount;
                                    db.BillWiseReceives.Add(receive);
                                    db.SaveChanges();
                                    var totalpaidAmount1 = db.BillWiseReceives.Where(r => r.InvoiceId == item2.Id).Sum(s => s.Paid);
                                    receive.Due = item2.GrandTotal - totalpaidAmount1;
                                    if (totalpaidAmount1 == item2.GrandTotal)
                                    {
                                        item2.IsPaid = true;
                                        db.SaveChanges();
                                    }
                                    break;
                                }

                                else
                                {
                                    paidAmount = paidAmount - balanceAmount;
                                    BillWiseReceive receive = new BillWiseReceive();
                                    receive.InvoiceId = item2.Id;
                                    receive.InvoiceNo = item2.NO;
                                    receive.Type = "S";
                                    receive.ReceiptPaymentId = item1.Id;
                                    //payment.Id = item1.Id;
                                    receive.BillAmount = item2.GrandTotal;
                                    receive.Paid = balanceAmount;
                                    db.BillWiseReceives.Add(receive);
                                    db.SaveChanges();
                                    var totalreceiveAmount1 = db.BillWiseReceives.Where(r => r.InvoiceId == item2.Id).Sum(s => s.Paid);
                                    receive.Due = item2.GrandTotal - totalreceiveAmount1;
                                    if (totalreceiveAmount1 == item2.GrandTotal)
                                    {
                                        item2.IsPaid = true;
                                        db.SaveChanges();
                                    }
                                }
                                db.SaveChanges();

                            }
                        }


                    }

                }

                db.SaveChanges();
                scope.Complete();
                return RedirectToAction("ReceiveVoucherList");
            }
        }

    }
}


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
    public class FinancialReportController : Controller
    {
        //
        // GET: /FinancialReport/

       

        #region Day Book
        [HttpGet]
        public ActionResult DayBook()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DayBookReport(string from)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                List<DayBookReportModelView> sprList = new List<DayBookReportModelView>();
                var result = db.AccountsBook_DayBook(companyid, Branchid, fid, fdate).ToList();
                decimal? debitTotal = result.Sum(r => r.Debit) ?? 0;
                decimal? creditTotal = result.Sum(r => r.Credit) ?? 0;
                foreach (var item in result)
                {
                    var spr = new DayBookReportModelView();
                    spr.Credit = item.Credit == 0 ? null : item.Credit;
                    spr.Debit = item.Debit == 0 ? null : item.Debit;
                    spr.ledgerName = item.ledgerName;
                    spr.transactionType = item.transactionType;
                    spr.VoucherNo = item.VoucherNo;
                    spr.RPDate = item.RPDate;
                    spr.DebitTotal = debitTotal;
                    spr.CreditTotal = creditTotal;
                    sprList.Add(spr);
                }

                return PartialView(sprList);
            }
        }

        [HttpGet]
        public ActionResult DayBookPDF(DateTime from, int comId, int branId, int finId)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = comId;
                int Branchid = branId;
                int fid = finId;
                var detls = db.Companies.Where(c => c.Id == companyid).FirstOrDefault();
                ViewBag.Name = detls.Name;
                ViewBag.address = detls.Address;
                ViewBag.pin = detls.Zipcode;
                ViewBag.from = from;
               
                Session["fdate"] = from;
                List<DayBookReportModelView> sprList = new List<DayBookReportModelView>();
             
                var result = db.AccountsBook_DayBook(companyid, Branchid, fid, from).ToList();
                decimal? debitTotal = result.Sum(r => r.Debit) ?? 0;
                decimal? creditTotal = result.Sum(r => r.Credit) ?? 0;
                foreach (var item in result)
                {
                    var spr = new DayBookReportModelView();
                    spr.Credit = item.Credit == 0 ? null : item.Credit;
                    spr.Debit = item.Debit == 0 ? null : item.Debit;
                    spr.ledgerName = item.ledgerName;
                    spr.transactionType = item.transactionType;
                    spr.VoucherNo = item.VoucherNo;
                    spr.RPDate = item.RPDate;
                    spr.DebitTotal = debitTotal;
                    spr.CreditTotal = creditTotal;
                    sprList.Add(spr);
                }

                return View(sprList);
            }


        }

        [HttpGet]
        public ActionResult DayBookPDFlink()
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            try
            {
                string from = Session["fdate"].ToString();
                return new ActionAsPdf("DayBookPDF", new { from = from, comId = companyid, branId = Branchid, finId = fid }) { FileName = "DayBook.pdf" };
            }
            catch
            {
                return new ActionAsPdf("DayBookPDF", new { from = 0 }) { FileName = "DayBook.pdf" };
            }

        }

        #endregion

        #region Cash Book
        [HttpGet]
        public ActionResult CashBook()
        {
            //AccountsBook_CashBook_Result spr = new AccountsBook_CashBook_Result();
            //int year = (DateTime.Today.Month >= 4) ? DateTime.Today.Year : DateTime.Today.Year - 1;
            //spr.FromDate =  new DateTime(year, 04, 01);
            //spr.ToDate = new DateTime(year + 1, 03, 31);
            return View();
        }

        [HttpPost]
        public ActionResult CashBookReport()
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                List<CashBookReportModelView> sprList = new List<CashBookReportModelView>();
                var result = db.AccountsBook_CashBook(companyid, Branchid, fid).ToList();
                decimal? debitTotal = result.Sum(r => r.Debit) ?? 0;
                decimal? creditTotal = result.Sum(r => r.Credit) ?? 0;
                foreach (var item in result)
                {
                    var spr = new CashBookReportModelView();
                    spr.Credit = item.Credit;
                    spr.Debit = item.Debit;
                    spr.ledgerName = item.ledgerName;
                    spr.transactionType = item.transactionType;
                    spr.VoucherNo = item.VoucherNo;
                    spr.RPdate = item.RPdate;
                    spr.DebitTotal = debitTotal;
                    spr.CreditTotal = creditTotal;
                    sprList.Add(spr);
                }

                return PartialView(sprList);

            }
        }

        [HttpGet]
        public ActionResult CashBookPDF(int comId, int branId, int finId, string fdate, string tdate)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = comId;
                int Branchid = branId;
                int fid = finId;
                var detls = db.Companies.Where(c => c.Id == companyid).FirstOrDefault();
                ViewBag.Name = detls.Name;
                ViewBag.address = detls.Address;
                ViewBag.pin = detls.Zipcode;
                ViewBag.from = fdate;
                ViewBag.to = tdate;

                //try
                //{
                //    var branch = db.BranchMasters.Where(c => c.Id == Branchid).FirstOrDefault();
                //    ViewBag.branchname = branch.Name;
                //}
                //catch { }


                List<CashBookReportModelView> sprList = new List<CashBookReportModelView>();
                var result = db.AccountsBook_CashBook(companyid, Branchid, fid).ToList();
                decimal? debitTotal = result.Sum(r => r.Debit) ?? 0;
                decimal? creditTotal = result.Sum(r => r.Credit) ?? 0;
                foreach (var item in result)
                {

                    var spr = new CashBookReportModelView();
                    spr.Credit = item.Credit;
                    spr.Debit = item.Debit;
                    spr.ledgerName = item.ledgerName;
                    spr.transactionType = item.transactionType;
                    spr.VoucherNo = item.VoucherNo;
                    spr.RPdate = item.RPdate;
                    spr.DebitTotal = debitTotal;
                    spr.CreditTotal = creditTotal;
                    sprList.Add(spr);
                }

                return View(sprList);
            }


        }

        [HttpGet]
        public ActionResult CashBookPDFlink(string fdate, string tdate)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            string From = fdate;
            string To = tdate;
            try
            {

                return new ActionAsPdf("CashBookPDF", new { from = From, comId = companyid, branId = Branchid, finId = fid, to = To }) { FileName = "CashBook.pdf" };
            }
            catch
            {
                return new ActionAsPdf("CashBookPDF", new { from = 0 }) { FileName = "CashBook.pdf" };
            }

        }

        #endregion

        #region Profit And Loss

        [HttpGet]
        public ActionResult ProfitLoss()
        {
            return View();
        }

        [HttpPost]
        public ActionResult profitLossReport(string from, string to)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(to, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                ViewBag.fdate = from;
                ViewBag.tdate = to;
                List<ProfitLossReportModelView> sprList = new List<ProfitLossReportModelView>();
                var result = db.AccountsBook_ProfitAndLoss(companyid, Branchid, fid).ToList();
                decimal? debitTotal = result.Sum(r => r.DebitAmount) ?? 0;
                decimal? creditTotal = result.Sum(r => r.CreditAmount) ?? 0;
                foreach (var item in result)
                {

                    int slen = 0;
                    if (item.Sortorder != null)
                    {
                        slen = item.Sortorder.Length;
                    }
                    string spc = "";
                    for (int i = 3; i < slen; i++)
                    {
                        spc += " ";
                    }
                    var spr = new ProfitLossReportModelView();
                    spr.DebitAmount = item.DebitAmount == 0 ? null : item.DebitAmount;
                    spr.CreditAmount = item.CreditAmount == 0 ? null : item.CreditAmount;
                    spr.LedgerName = item.LedgerName;
                    spr.GroupName = spc + item.GroupName;
                    spr.Sortorder = item.Sortorder;
                    spr.DebitTotal = debitTotal;
                    spr.CreditTotal = creditTotal;
                    sprList.Add(spr);
                }

                return PartialView(sprList);
            }
        }

        [HttpGet]
        public ActionResult ProfitLossPDF(int comId, int branId, int finId, string from, string to)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = comId;
                int Branchid = branId;
                int fid = finId;
                var detls = db.Companies.Where(c => c.Id == companyid).FirstOrDefault();
                ViewBag.Name = detls.Name;
                ViewBag.address = detls.Address;
                ViewBag.pin = detls.Zipcode;
                ViewBag.from = from;
                ViewBag.to = to;
                Session["fdate"] = from;
                List<ProfitLossReportModelView> sprList = new List<ProfitLossReportModelView>();
                var result = db.AccountsBook_ProfitAndLoss(companyid, Branchid, fid).ToList();
                decimal? debitTotal = result.Sum(r => r.DebitAmount) ?? 0;
                decimal? creditTotal = result.Sum(r => r.CreditAmount) ?? 0;
                foreach (var item in result)
                {

                    int slen = 0;
                    if (item.Sortorder != null)
                    {
                        slen = item.Sortorder.Length;
                    }
                    string spc = "";
                    for (int i = 3; i < slen; i++)
                    {
                        spc += " ";
                    }
                    var spr = new ProfitLossReportModelView();
                    spr.DebitAmount = item.DebitAmount == 0 ? null : item.DebitAmount;
                    spr.CreditAmount = item.CreditAmount == 0 ? null : item.CreditAmount;
                    spr.LedgerName = item.LedgerName;
                    spr.GroupName = spc + item.GroupName;
                    spr.Sortorder = item.Sortorder;
                    spr.DebitTotal = debitTotal;
                    spr.CreditTotal = creditTotal;
                    sprList.Add(spr);
                }
                return View(sprList);
            }


        }

        [HttpGet]
        public ActionResult ProfitLossPDFlink(string fdate, string tdate)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            string From = fdate;
            string To = tdate;
            try
            {
                return new ActionAsPdf("ProfitLossPDF", new { comId = companyid, branId = Branchid, finId = fid, from = From, to = To }) { FileName = "ProfitLoss.pdf" };
            }
            catch
            {
                return new ActionAsPdf("ProfitLossPDF", new { from = 0 }) { FileName = "ProfitLoss.pdf" };
            }

        }


        #endregion

        #region Balance Sheet

        public ActionResult BalanceSheet()
        {
            return View();
        }

        public ActionResult BalanceSheetReport()
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                List<BalanceSheetReportModelView> sprList = new List<BalanceSheetReportModelView>();
                var result = db.AccountsBook_BalanceSheet(companyid, Branchid, fid).ToList();
                decimal? debitTotal = result.Sum(r => r.DebitAmount) ?? 0;
                decimal? creditTotal = result.Sum(r => r.CreditAmount) ?? 0;
                foreach (var item in result)
                {
                    int slen = 0;
                    if (item.Sortorder != null)
                    {
                        slen = item.Sortorder.Length;
                    }
                    string spc = "";
                    for (int i = 3; i < slen; i++)
                    {
                        spc += "   ";
                    }
                    var spr = new BalanceSheetReportModelView();
                    spr.CreditAmount = item.CreditAmount == 0 ? '-' : item.CreditAmount;
                    spr.DebitAmount = item.DebitAmount == 0 ? '-' : item.DebitAmount;
                    spr.LedgerName = item.LedgerName;
                    spr.GroupName = spc + item.GroupName;
                    spr.Sortorder = item.Sortorder;
                    spr.DebitTotal = debitTotal;
                    spr.CreditTotal = creditTotal;
                    sprList.Add(spr);
                }

                return PartialView(sprList);

            }
        }

        [HttpGet]
        public ActionResult BalancePDF(int comId, int branId, int finId, string from, string to)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = comId;
                int Branchid = branId;
                int fid = finId;
                var detls = db.Companies.Where(c => c.Id == companyid).FirstOrDefault();
                ViewBag.Name = detls.Name;
                ViewBag.address = detls.Address;
                ViewBag.pin = detls.Zipcode;
                ViewBag.from = from;
                ViewBag.to = to;
                List<BalanceSheetReportModelView> sprList = new List<BalanceSheetReportModelView>();
                var result = db.AccountsBook_BalanceSheet(companyid, Branchid, fid).ToList();
                decimal? debitTotal = result.Sum(r => r.DebitAmount) ?? 0;
                decimal? creditTotal = result.Sum(r => r.CreditAmount) ?? 0;
                foreach (var item in result)
                {

                    int slen = 0;
                    if (item.Sortorder != null)
                    {
                        slen = item.Sortorder.Length;
                    }
                    string spc = "";
                    for (int i = 3; i < slen; i++)
                    {
                        spc += " ";
                    }
                    var spr = new BalanceSheetReportModelView();
                    spr.DebitAmount = item.DebitAmount == 0 ? null : item.DebitAmount;
                    spr.CreditAmount = item.CreditAmount == 0 ? null : item.CreditAmount;
                    spr.LedgerName = item.LedgerName;
                    spr.GroupName = spc + item.GroupName;
                    spr.Sortorder = item.Sortorder;
                    spr.DebitTotal = debitTotal;
                    spr.CreditTotal = creditTotal;
                    sprList.Add(spr);
                }
                return View(sprList);
            }


        }

        [HttpGet]
        public ActionResult BalancePDFlink(string fdate, string tdate)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            string From = fdate;
            string To = tdate;
            try
            {
                return new ActionAsPdf("BalancePDF", new { comId = companyid, branId = Branchid, finId = fid, from = From, to = To }) { FileName = "Balance.pdf" };
            }
            catch
            {
                return new ActionAsPdf("BalancePDF", new { from = 0 }) { FileName = "Balance.pdf" };
            }

        }

        #endregion

        #region Trial Balance

        public ActionResult TrialBalance()
        {
            return View();
        }

        public ActionResult TrialBalanceReport()
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                List<TrialBalanceModelView> sprList = new List<TrialBalanceModelView>();
                var result = db.AccountsBook_ChartOfAccounts(companyid, Branchid, fid).ToList();
                decimal? debitTotal = result.Sum(r => r.DebitAmount) ?? 0;
                decimal? creditTotal = result.Sum(r => r.CreditAmount) ?? 0;
                foreach (var item in result)
                {
                    int slen = 0;
                    if (item.Sortorder != null)
                    {
                        slen = item.Sortorder.Length;
                    }
                    string spc = "";
                    for (int i = 3; i < slen; i++)
                    {
                        spc += "   ";
                    }

                    var spr = new TrialBalanceModelView();
                    spr.CreditAmount = item.CreditAmount;
                    spr.DebitAmount = item.DebitAmount;
                    spr.GroupName = spc + item.GroupName;
                    spr.Sortorder = item.Sortorder;
                    spr.DebitTotal = debitTotal;
                    spr.CreditTotal = creditTotal;
                    sprList.Add(spr);
                }

                return PartialView(sprList);
            }
        }

        [HttpGet]
        public ActionResult TrialPDF(int comId, int branId, int finId, string from, string to)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = comId;
                int Branchid = branId;
                int fid = finId;
                var detls = db.Companies.Where(c => c.Id == companyid).FirstOrDefault();
                ViewBag.Name = detls.Name;
                ViewBag.address = detls.Address;
                ViewBag.pin = detls.Zipcode;
                ViewBag.from = from;
                ViewBag.to = to;
                List<TrialBalanceModelView> sprList = new List<TrialBalanceModelView>();
                var result = db.AccountsBook_ChartOfAccounts(companyid, Branchid, fid).ToList();
                decimal? debitTotal = result.Sum(r => r.DebitAmount) ?? 0;
                decimal? creditTotal = result.Sum(r => r.CreditAmount) ?? 0;
                foreach (var item in result)
                {

                    int slen = 0;
                    if (item.Sortorder != null)
                    {
                        slen = item.Sortorder.Length;
                    }
                    string spc = "";
                    for (int i = 3; i < slen; i++)
                    {
                        spc += " ";
                    }
                    var spr = new TrialBalanceModelView();
                    spr.DebitAmount = item.DebitAmount == 0 ? null : item.DebitAmount;
                    spr.CreditAmount = item.CreditAmount == 0 ? null : item.CreditAmount;
                    spr.GroupName = spc + item.GroupName;
                    spr.Sortorder = item.Sortorder;
                    spr.DebitTotal = debitTotal;
                    spr.CreditTotal = creditTotal;
                    sprList.Add(spr);
                }
                return View(sprList);
            }
        }

        [HttpGet]
        public ActionResult TrialPDFlink(string fdate, string tdate)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            string From = fdate;
            string To = tdate;
            try
            {
                return new ActionAsPdf("TrialPDF", new { comId = companyid, branId = Branchid, finId = fid, from = From, to = To }) { FileName = "Trial.pdf" };
            }
            catch
            {
                return new ActionAsPdf("TrialPDF", new { from = 0 }) { FileName = "Trial.pdf" };
            }

        }

        #endregion

        #region Ledger Report
        [HttpGet]
        public ActionResult Ledger()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LedgerReport(int ledgerId, string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                //  List<DayBookReportModelView> sprList = new List<DayBookReportModelView>();
                var result = db.AccountsBook_LedgerWise_DateRange(companyid, Branchid, fid, ledgerId, fdate, tdate).ToList();
                //decimal? debitTotal = result.Sum(r => r.Debit) ?? 0;
                //decimal? creditTotal = result.Sum(r => r.Credit) ?? 0;
                //foreach (var item in result)
                //{
                //    var spr = new DayBookReportModelView();
                //    spr.Credit = item.Credit == 0 ? null : item.Credit;
                //    spr.Debit = item.Debit == 0 ? null : item.Debit;
                //    spr.ledgerName = item.ledgerName;
                //    spr.transactionType = item.transactionType;
                //    spr.VoucherNo = item.VoucherNo;
                //    spr.RPDate = item.RPDate;
                //    spr.DebitTotal = debitTotal;
                //    spr.CreditTotal = creditTotal;
                //    sprList.Add(spr);
                //}
                var getDateRange = db.FinancialYearMasters.Where(d => d.fYearID == fid).FirstOrDefault();
                ViewBag.FirstDate = getDateRange.sDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
                ViewBag.Branch = db.BranchMasters.FirstOrDefault(d => d.Id == Branchid).Name;
                ViewBag.Company = db.Companies.FirstOrDefault(d => d.Id == companyid).Address;
                var getGroup = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.groupID).FirstOrDefault();
                if(getGroup == 104)
                {
                    var salesdetails = db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= fdate && d.SalesInvoice.InvoiceDate <= tdate && d.SalesInvoice.FinancialYearId == fid).Select(d => new SalesInvoiceDetailModelView { BarCode = d.SalesInvoice.NO, ItemName = d.Product.ShortName, Quantity = d.Quantity, UnitName = d.UOM.Code, UnitSecondaryName = d.UOM1.Code, UnitFormula = d.UnitFormula }).ToList();
                }
                return PartialView(result);
            }
        }

        [HttpGet]
        public ActionResult LedgerPDF( int companyid, int Branchid, int fid, int ledgerId, DateTime fdate, DateTime tdate,string dateRange)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                ViewBag.DateRange = dateRange;
                var result = db.AccountsBook_LedgerWise_DateRange(companyid, Branchid, fid, ledgerId, fdate, tdate).ToList();
                ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
                return View(result);
            }


        }

        [HttpGet]
        public ActionResult LedgerPDFlink(int ledgerId, string from, string To)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            string tbduration = " From  " + from + "   To  " + To;
            Session["fdate"] = fdate;
            try
            {
               
                return new ActionAsPdf("LedgerPDF", new { companyid = companyid, Branchid = Branchid, fid = fid, ledgerId = ledgerId, fdate = fdate, tdate = tdate, dateRange = tbduration }) { FileName = "LedgerPDF.pdf" };
            }
            catch
            {
                return new ActionAsPdf("LedgerPDF", new { from = 0 }) { FileName = "LedgerPDF.pdf" };
            }

        }

        #endregion

        #region Group Report
        [HttpGet]
        public ActionResult Group()
        {
            return View();
        }

        public ActionResult BranchGroupDetail(int GroupId, string GroupName, string from, string To)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            Session["fdate"] = fdate;
            ViewBag.GroupName = GroupName;
            ViewBag.GroupId = GroupId;
            ViewBag.From = from;
            ViewBag.To = To;
            using (InventoryEntities db = new InventoryEntities())
            {
                ViewBag.GroupResult = db.AccountsBook_GroupWise_DateRange(companyid,Branchid, fid, GroupId, fdate, tdate).ToList();

                return View("Group");
            }
        }

        [HttpPost]
        public ActionResult GroupReport(int GroupId,int? LedgerId ,string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                
              //  var result = db.AccountsBook_GroupWise_DateRangeSK(companyid, Branchid, fid, 0, fdate, tdate).ToList();
                var result = db.AccountsBook_GroupWise_DateRange(companyid, Branchid, fid, GroupId, fdate, tdate).Where(d=> !(d.Opening==0 && d.Debit==0 && d.Credit==0 && d.Closing==0)).ToList();
                ViewBag.GroupName = db.GroupMasters.Where(d => d.groupID == GroupId).Select(d => d.groupName).FirstOrDefault();
                string tbduration = " From  " + from + "   To  " + To;
                ViewBag.DateRange = tbduration;
                if (GroupId == 108)
                {
                    if (LedgerId == null)
                    {
                        var customers = db.Customers.Select(d => new { LId = (int?)d.LId, PId = d.PId == null ? d.LId : d.PId }).ToList();
                        var groupresult = (from r in result
                                           join c in customers
                                           on r.Lid equals c.LId
                                           //    group r by c.PId into g
                                           select new AccountsBook_GroupWise_DateRange_Result { GroupId = (int?)c.PId, Lid = r.Lid, LedgerName = r.LedgerName, DRCR = r.DRCR, Opening = r.Opening, Debit = r.Debit, Credit = r.Credit, Closing = r.Closing, ClosingDRCR = r.ClosingDRCR }).OrderBy(d => d.GroupId).ToList();
                        return PartialView("GroupReportDebtors", groupresult);
                    }
                    else
                    {
                        var customers= db.Customers.Where(d=>d.LId==LedgerId || d.PId==LedgerId).Select(d => new { LId = (int?)d.LId, PId = d.PId == null ? d.LId : d.PId }).ToList();
                        var groupresult = (from r in result
                                           join c in customers
                                           on r.Lid equals c.LId
                                           select new AccountsBook_GroupWise_DateRange_Result { GroupId = (int?)c.PId, Lid = r.Lid, LedgerName = r.LedgerName, DRCR = r.DRCR, Opening = r.Opening, Debit = r.Debit, Credit = r.Credit, Closing = r.Closing, ClosingDRCR = r.ClosingDRCR }).OrderBy(d => d.GroupId).ToList();
                        return PartialView("GroupReportDebtors", groupresult);
                    }
                }
                return PartialView(result);
            }
        }
        public ActionResult GroupBranchExport(int GroupId, string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;

                var result = db.AccountsBook_GroupWise_DateRange(companyid, Branchid, fid, GroupId, fdate, tdate).ToList();

                ViewBag.GroupName = db.GroupMasters.Where(d => d.groupID == GroupId).Select(d => d.groupName).FirstOrDefault();
            //    return PartialView(result);

                //Response.AddHeader("Content-Type", "application/vnd.ms-excel");
                Response.AddHeader("content-disposition", "attachment; filename=GroupReport.xls");
                Response.ContentType = "application/ms-excel";
                return View("GroupReport", result);
            }
           
        }
        [HttpGet]
        public ActionResult GroupPDF(DateTime from, DateTime To, int comId, int branchid, int groupid, int finId, string dateRange, int? LedgerId)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                ViewBag.DateRange = dateRange;
                ViewBag.GroupName = db.GroupMasters.Where(d => d.groupID == groupid).Select(d => d.groupName).FirstOrDefault();
                var result = db.AccountsBook_GroupWise_DateRange(comId, branchid, finId, groupid, from, To).Where(d => !(d.Opening == 0 && d.Debit == 0 && d.Credit == 0 && d.Closing == 0)).ToList();
                if (groupid == 108)
                {
                    if (LedgerId == null)
                    {
                        var customers = db.Customers.Select(d => new { LId = (int?)d.LId, PId = d.PId == null ? d.LId : d.PId }).ToList();
                        var groupresult = (from r in result
                                           join c in customers
                                           on r.Lid equals c.LId
                                           //    group r by c.PId into g
                                           select new AccountsBook_GroupWise_DateRange_Result { GroupId = (int?)c.PId, Lid = r.Lid, LedgerName = r.LedgerName, DRCR = r.DRCR, Opening = r.Opening, Debit = r.Debit, Credit = r.Credit, Closing = r.Closing, ClosingDRCR = r.ClosingDRCR }).OrderBy(d => d.GroupId).ToList();
                        return View("GroupPDFDebtors", groupresult);
                    }
                    else
                    {
                        var customers = db.Customers.Where(d => d.LId == LedgerId || d.PId == LedgerId).Select(d => new { LId = (int?)d.LId, PId = d.PId == null ? d.LId : d.PId }).ToList();
                        var groupresult = (from r in result
                                           join c in customers
                                           on r.Lid equals c.LId
                                           //    group r by c.PId into g
                                           select new AccountsBook_GroupWise_DateRange_Result { GroupId = (int?)c.PId, Lid = r.Lid, LedgerName = r.LedgerName, DRCR = r.DRCR, Opening = r.Opening, Debit = r.Debit, Credit = r.Credit, Closing = r.Closing, ClosingDRCR = r.ClosingDRCR }).OrderBy(d => d.GroupId).ToList();
                        return View("GroupPDFDebtors", groupresult);
                    }
                    

                }
                return View(result);
            }


        }

        [HttpGet]
        public ActionResult GroupPDFlink(int GroupId,int? LedgerId, string from, string To)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            Session["fdate"] = fdate;
            string tbduration = " From  " + from + "   To  " + To;
            try
            {
                //  string from = Session["fdate"].ToString();
                return new ActionAsPdf("GroupPDF", new { from = fdate, To = tdate, comId = companyid, branchid = Branchid, groupid = GroupId, finId = fid, dateRange = tbduration, LedgerId = LedgerId }) { FileName = "Group.pdf" };
            }
            catch
            {
                return new ActionAsPdf("GroupPDF", new { from = 0 }) { FileName = "Group.pdf" };
            }

        }

        public ActionResult GroupBranchExpenses()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GroupBranchExpensesReport(int GroupId, string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;

                var result = db.AccountsBook_GroupWise_DateRangeSK(companyid, Branchid, fid, 0, fdate, tdate).ToList();
               // var result = db.AccountsBook_GroupWise_DateRange(companyid, Branchid, fid, GroupId, fdate, tdate).ToList();
                ViewBag.GroupName = db.GroupMasters.Where(d => d.groupID == GroupId).Select(d => d.groupName).FirstOrDefault();
                return PartialView(result);
            }
        }

        [HttpGet]
        public ActionResult GroupBranchExpensesPDF(int GroupId, DateTime from, DateTime to, int comId, int branId, int finId)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                var result = db.AccountsBook_GroupWise_DateRangeSK(comId, branId, finId, 0, from, to).ToList();
                // var result = db.AccountsBook_GroupWise_DateRange(companyid, Branchid, fid, GroupId, fdate, tdate).ToList();
                ViewBag.GroupName = db.GroupMasters.Where(d => d.groupID == GroupId).Select(d => d.groupName).FirstOrDefault();
                return PartialView(result);
            }
        }
        [HttpGet]
        public ActionResult GroupBranchExpensesPDFlink(int GroupId, string from, string To)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            Session["fdate"] = fdate;
            try
            {
              
                return new ActionAsPdf("GroupBranchExpensesPDF", new { GroupId= GroupId,from = fdate,to=tdate, comId = companyid, branId = Branchid, finId = fid }) { FileName = "GroupBranchExpenses.pdf" };
            }
            catch
            {
                return new ActionAsPdf("DayBookPDF", new { from = 0 }) { FileName = "Group.pdf" };
            }

        }
        #endregion

        #region Ledger Date Wise Report
        [HttpGet]
        public ActionResult LedgerDateRange()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LedgerReportDateRange(int ledgerId, string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
               // int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                //  List<DayBookReportModelView> sprList = new List<DayBookReportModelView>();
                var result = db.AccountsBook_LedgerWise_DateRange_Hari_Admin(companyid, fid, ledgerId, fdate, tdate).ToList();
                //decimal? debitTotal = result.Sum(r => r.Debit) ?? 0;
                //decimal? creditTotal = result.Sum(r => r.Credit) ?? 0;
                //foreach (var item in result)
                //{
                //    var spr = new DayBookReportModelView();
                //    spr.Credit = item.Credit == 0 ? null : item.Credit;
                //    spr.Debit = item.Debit == 0 ? null : item.Debit;
                //    spr.ledgerName = item.ledgerName;
                //    spr.transactionType = item.transactionType;
                //    spr.VoucherNo = item.VoucherNo;
                //    spr.RPDate = item.RPDate;
                //    spr.DebitTotal = debitTotal;
                //    spr.CreditTotal = creditTotal;
                //    sprList.Add(spr);
                //}
                ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
                return PartialView(result);
            }
        }

        [HttpGet]
        public ActionResult LedgerDateRangePDF(int companyid, int Branchid, int fid, int ledgerId, DateTime fdate, DateTime tdate)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                var result = db.AccountsBook_LedgerWise_DateRange(companyid, Branchid, fid, ledgerId, fdate, tdate).ToList();
                ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
                return View(result);
            }


        }

        [HttpGet]
        public ActionResult LedgerDateRangePDFlink(int ledgerId, string from, string To)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            Session["fdate"] = fdate;
            try
            {

                return new ActionAsPdf("LedgerPDF", new { companyid = companyid, Branchid = Branchid, fid = fid, ledgerId = ledgerId, fdate = fdate, tdate = tdate }) { FileName = "LedgerPDF.pdf" };
            }
            catch
            {
                return new ActionAsPdf("LedgerPDF", new { from = 0 }) { FileName = "LedgerPDF.pdf" };
            }

        }

        #endregion

        #region Sub Ledger Report
        [HttpGet]
        public ActionResult SubLedger()
        {
            return View();
        }


        [HttpGet]
        public ActionResult SubLedgerDetail(int ledgerId, string ledgerName, string from, string To)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            Session["fdate"] = fdate;
            ViewBag.LedgerName = ledgerName;
            ViewBag.LedgerId = ledgerId;
            ViewBag.From = from;
            ViewBag.To = To;
            using (InventoryEntities db = new InventoryEntities())
            {
                ViewBag.GroupResult = db.AccountsBook_LedgerWise_DateRange(companyid,Branchid, fid, ledgerId, fdate, tdate).ToList();
            }
            return View("SubLedger");
        }


        [HttpPost]
        public ActionResult SubLedgerReport(int ledgerId, string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                // int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                //  List<DayBookReportModelView> sprList = new List<DayBookReportModelView>();
                var result = db.AccountsBook_LedgerWise_DateRange(companyid,Branchid ,fid, ledgerId, fdate, tdate).ToList();

                ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
                var getGroup = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.parentID).FirstOrDefault();
                if (getGroup == 6 || getGroup == 32)
                {
                    ViewBag.salesdetails = db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= fdate && d.SalesInvoice.InvoiceDate <= tdate && d.SalesInvoice.FinancialYearId == fid && d.SalesInvoice.BranchId == Branchid).Select(d => new SalesInvoiceDetailModelView { BarCode = d.SalesInvoice.NO, Description = d.Product.Group.Name, ItemName = d.Product.Name, Quantity = d.UnitIdSecondary == 1 ? d.UnitFormula : d.Quantity, UnitName = d.UnitIdSecondary == 1 ? (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM.Code) : d.UOM1.Code, UnitSecondaryName = d.UnitIdSecondary == 1 ? d.UOM1.Code : (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM.Code), UnitFormula = d.UnitIdSecondary == 1 ? d.Quantity : (d.Quantity * d.UnitFormula) }).ToList();
                }
                return PartialView(result);
            }
        }

        [HttpGet]
        public ActionResult SubLedgerPDF(int companyid, int Branchid, int fid, int ledgerId, DateTime fdate, DateTime tdate)
        {
            var culture = "es-AR";
            string dateFormat = "dd/MM/yyyy";

            using (InventoryEntities db = new InventoryEntities())
            {
                var result = db.AccountsBook_LedgerWise_DateRange(companyid,Branchid ,fid, ledgerId, fdate, tdate).ToList();
                ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
                ViewBag.Address = db.Customers.Where(d => d.LId == ledgerId).Select(d => d.AddressName + ", " + d.StateRegion + ", " + d.PostalCode).FirstOrDefault();
                ViewBag.From = fdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture)); 
                ViewBag.To = tdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var getGroup = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.parentID).FirstOrDefault();
                if (getGroup == 6 || getGroup == 32)
                {
                    ViewBag.salesdetails = db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= fdate && d.SalesInvoice.InvoiceDate <= tdate && d.SalesInvoice.FinancialYearId == fid && d.SalesInvoice.BranchId==Branchid).Select(d => new SalesInvoiceDetailModelView { BarCode = d.SalesInvoice.NO, Description = d.Product.Group.Name, ItemName = d.Product.Name, Quantity = d.UnitIdSecondary == 1 ? d.UnitFormula : d.Quantity, UnitName = d.UnitIdSecondary == 1 ? (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM.Code) : d.UOM1.Code, UnitSecondaryName = d.UnitIdSecondary == 1 ? d.UOM1.Code : (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM.Code), UnitFormula = d.UnitIdSecondary == 1 ? d.Quantity : (d.Quantity * d.UnitFormula) }).ToList();
                }
                return View(result);
            }

        }

        [AllowAnonymous]
        public ActionResult PrintHeader()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult PrintFooter()
        {
            return View();
        }
        [HttpGet]
        public ActionResult SubLedgerPDFlink(int ledgerId, string from, string To)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            Session["fdate"] = fdate;
            //string header = Server.MapPath("~/bin/PrintHeader.html");//Path of PrintHeader.html File
            //string footer = Server.MapPath("~/bin/PrintFooter.html");//Path of PrintFooter.html File

            //string customSwitches = string.Format("--header-html  \"{0}\" " +
            //                       "--header-spacing \"0\" " +
            //                       "--footer-html \"{1}\" " +
            //                       "--footer-spacing \"10\" " +
            //                       "--footer-font-size \"10\" " +
            //                       "--header-font-size \"10\" ", header, footer);
           //string customSwitches = string.Format("--header-html  \"{0}\" " +
           //                             "--header-spacing \"0\" " +
           //                             "--footer-center  Page: [page]/[toPage]\" " +
           //                             "--footer-line --footer-font-size \"9\" --footer-spacing 6 --footer-font-name \"calibri light\" ", Url.Action("PrintHeader", "FinancialReport", new { area = "" }, "http"));
            string header = Url.Action("PrintHeader", "FinancialReport", new { area = "" }, "http");
            string footer = Url.Action("PrintFooter", "FinancialReport", new { area = "" }, "http");

            string customSwitches = string.Format("--no-stop-slow-scripts --javascript-delay 1000 --footer-html {0} --footer-spacing 10", footer);
            //string customSwitches = string.Format("--header-html  \"{0}\" " +
            //                       "--header-spacing \"0\" " +
            //                       "--footer-html \"{1}\" " +
            //                       "--footer-spacing \"10\" " +
            //                       "--footer-font-size \"10\" " +
            //                       "--header-font-size \"10\" ",
            //   Url.Action("PrintHeader", "FinancialReport", new { area = "" }, "http"),Url.Action("PrintFooter", "FinancialReport", new { area = "" }, "http"));
            try
            {
                using (InventoryEntities db = new InventoryEntities())
                {
                    var result = db.AccountsBook_LedgerWise_DateRange(companyid, Branchid, fid, ledgerId, fdate, tdate).ToList();
                    ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
                    ViewBag.Address = db.Customers.Where(d => d.LId == ledgerId).Select(d => d.AddressName + ", " + d.StateRegion + ", " + d.PostalCode).FirstOrDefault();
                    ViewBag.From = fdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    ViewBag.To = tdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    var getGroup = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.parentID).FirstOrDefault();
                    if (getGroup == 6 || getGroup == 32)
                    {
                        ViewBag.salesdetails = db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= fdate && d.SalesInvoice.InvoiceDate <= tdate && d.SalesInvoice.FinancialYearId == fid && d.SalesInvoice.BranchId == Branchid).Select(d => new SalesInvoiceDetailModelView { BarCode = d.SalesInvoice.NO, Description = d.Product.Group.Name, ItemName = d.Product.Name, Quantity = d.UnitIdSecondary == 1 ? d.UnitFormula : d.Quantity, UnitName = d.UnitIdSecondary == 1 ? (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM.Code) : d.UOM1.Code, UnitSecondaryName = d.UnitIdSecondary == 1 ? d.UOM1.Code : (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM.Code), UnitFormula = d.UnitIdSecondary == 1 ? d.Quantity : (d.Quantity * d.UnitFormula) }).ToList();
                    }
                    //   return View(result);


                    return new Rotativa.PartialViewAsPdf("SubLedgerPDF", result)
                    // return new ActionAsPdf("SubLedgerPDF", new { companyid = companyid, Branchid = Branchid, fid = fid, ledgerId = ledgerId, fdate = fdate, tdate = tdate })
                    {
                        FileName = "SubLedgerPDF1.pdf",
                        PageSize = Size.A4,
                        PageMargins = new Margins(10, 10, 10, 10),
                        CustomSwitches = "--footer-right [page]/[topage]"//customSwitches
                        // PageMargins = new Margins(0, 0, 0, 0),
                        //CustomSwitches = "--print-media-type  --footer-center [page] --footer-font-size 8"

                    };
                    // "--header-center \"text\" --footer-right \"Page: [page] of [toPage]\"  --disable-smart-shrinking" };
                }
            }
            catch
            {
                return new ActionAsPdf("SubLedgerPDF", new { from = 0 }) { FileName = "SubLedgerPDF.pdf" };
            }
            //using (InventoryEntities db = new InventoryEntities())
            //{
            //    var result = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate).ToList();
            //    ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
            //    return View("AdminSubLedgerPDF", result);
            //}
        }

        #endregion

        #region ADMIN GROUP Report
        [HttpGet]
        public ActionResult AdminGroup()
        {
            return View();
        }

        public ActionResult AdminGroupDetail(int GroupId, string GroupName, string from, string To)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            Session["fdate"] = fdate;
            ViewBag.GroupName = GroupName;
            ViewBag.GroupId = GroupId;
            ViewBag.From = from;
            ViewBag.To = To;
            using (InventoryEntities db = new InventoryEntities())
            {
                ViewBag.GroupResult = db.AccountsBook_GroupWise_DateRange_Current_AllGroupAdmin(companyid, fid, GroupId, fdate, tdate).ToList();

                return View("AdminGroup");
            }
        }

        [HttpPost]
        public ActionResult AdminGroupReport(int GroupId, string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                //int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                //int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                //  List<DayBookReportModelView> sprList = new List<DayBookReportModelView>();
                var result = db.AccountsBook_GroupWise_DateRange_Current_AllGroupAdmin(companyid,fid, GroupId, fdate, tdate).ToList();
                //decimal? debitTotal = result.Sum(r => r.Debit) ?? 0;
                //decimal? creditTotal = result.Sum(r => r.Credit) ?? 0;
                //foreach (var item in result)
                //{
                //    var spr = new DayBookReportModelView();
                //    spr.Credit = item.Credit == 0 ? null : item.Credit;
                //    spr.Debit = item.Debit == 0 ? null : item.Debit;
                //    spr.ledgerName = item.ledgerName;
                //    spr.transactionType = item.transactionType;
                //    spr.VoucherNo = item.VoucherNo;
                //    spr.RPDate = item.RPDate;
                //    spr.DebitTotal = debitTotal;
                //    spr.CreditTotal = creditTotal;
                //    sprList.Add(spr);
                //}
                ViewBag.GroupName = db.GroupMasters.Where(d => d.groupID == GroupId).Select(d => d.groupName).FirstOrDefault();
                return PartialView(result);
            }
        }
        public ActionResult AdminGroupExport(int GroupId, string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;

                var result = db.AccountsBook_GroupWise_DateRange_Current_AllGroupAdmin(companyid, fid, GroupId, fdate, tdate).ToList();
                ViewBag.DateRange = " From  " + from + "   To  " + To;
                ViewBag.GroupName = db.GroupMasters.Where(d => d.groupID == GroupId).Select(d => d.groupName).FirstOrDefault();
                Response.AddHeader("content-disposition", "attachment; filename=AdminAllGroup.xls");
                Response.ContentType = "application/ms-excel";
                return PartialView("AdminGroupPDF", result);
            }

        }


        [HttpGet]
        public ActionResult AdminGroupPDF(DateTime from, DateTime To, int comId, int groupid, int finId, string dateRange)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                ViewBag.DateRange = dateRange;
                ViewBag.GroupName = db.GroupMasters.Where(d => d.groupID == groupid).Select(d => d.groupName).FirstOrDefault();
                var result = db.AccountsBook_GroupWise_DateRange_Current_AllGroupAdmin(comId, finId, groupid, from, To).ToList();

                return View(result);
            }


        }

        [HttpGet]
        public ActionResult AdminGroupPDFlink(int GroupId, string from, string To)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            Session["fdate"] = fdate;
            string tbduration = " From  " + from + "   To  " + To;
            try
            {
                //  string from = Session["fdate"].ToString();
                return new ActionAsPdf("AdminGroupPDF", new { from = fdate, To = tdate, comId = companyid, groupid = GroupId, finId = fid, dateRange = tbduration }) { FileName = "AdminGroup.pdf" };
            }
            catch
            {
                return new ActionAsPdf("AdminGroupPDF", new { from = 0 }) { FileName = "AdminGroup.pdf" };
            }

        }


        #endregion

        //#region Admin Ledger Report
        //[HttpGet]
        //public ActionResult AdminLedger()
        //{
        //    return View();
        //}


        //[HttpGet]
        //public ActionResult AdminLedgerDetail(int ledgerId, string ledgerName, string from, string To)
        //{
        //    int companyid = Convert.ToInt32(Session["companyid"]);
        //    int Branchid = Convert.ToInt32(Session["BranchId"]);
        //    int fid = Convert.ToInt32(Session["fid"]);
        //    var culture = Session["DateCulture"].ToString();
        //    string dateFormat = Session["DateFormat"].ToString();
        //    var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
        //    var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
        //    Session["fdate"] = fdate;
        //    ViewBag.LedgerName = ledgerName;
        //    ViewBag.LedgerId = ledgerId;
        //    ViewBag.From = from;
        //    ViewBag.To = To;
        //    using (InventoryEntities db = new InventoryEntities())
        //    {
        //        ViewBag.GroupResult = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate).ToList();
        //    }
        //    return View("AdminLedger");
        //}


        //[HttpPost]
        //public ActionResult AdminLedgerReport(int ledgerId, string from, string To)
        //{
        //    using (InventoryEntities db = new InventoryEntities())
        //    {
        //        int companyid = Convert.ToInt32(Session["companyid"]);
        //        int Branchid = Convert.ToInt32(Session["BranchId"]);
        //        int fid = Convert.ToInt32(Session["fid"]);
        //       // int fid = 1;
        //        var culture = Session["DateCulture"].ToString();
        //        string dateFormat = Session["DateFormat"].ToString();
        //        var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
        //        var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
        //        Session["fdate"] = fdate;
        //        //  List<DayBookReportModelView> sprList = new List<DayBookReportModelView>();
        //        var result = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate).ToList();

        //        ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
        //        var getGroup = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.groupID).FirstOrDefault();
        //        if (getGroup == 108)
        //        {
        //            var salesdetails = db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= fdate && d.SalesInvoice.InvoiceDate <= tdate && d.SalesInvoice.FinancialYearId == fid).Select(d => new SalesInvoiceDetailModelView { BarCode = d.SalesInvoice.NO, ItemName = d.Product.ShortName, Quantity = d.Quantity, UnitName = d.UOM.Code, UnitSecondaryName = d.UOM1.Code, UnitFormula = d.UnitFormula }).ToList();
        //        }
        //        return PartialView(result);
        //    }
        //}

        //[HttpGet]
        //public ActionResult AdminLedgerPDF(int companyid, int Branchid, int fid, int ledgerId, DateTime fdate, DateTime tdate)
        //{
        //    using (InventoryEntities db = new InventoryEntities())
        //    {
        //        var result = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate).ToList();
        //        ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
        //        return View(result);
        //    }


        //}

        //[HttpGet]
        //public ActionResult AdminLedgerPDFlink(int ledgerId, string from, string To)
        //{
        //    int companyid = Convert.ToInt32(Session["companyid"]);
        //    int Branchid = Convert.ToInt32(Session["BranchId"]);
        //    int fid = Convert.ToInt32(Session["fid"]);
        //    var culture = Session["DateCulture"].ToString();
        //    string dateFormat = Session["DateFormat"].ToString();
        //    var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
        //    var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
        //    Session["fdate"] = fdate;
        //    //try
        //    //{

        //    //    return new ActionAsPdf("AdminLedgerPDF", new { companyid = companyid, Branchid = Branchid, fid = fid, ledgerId = ledgerId, fdate = fdate, tdate = tdate }) { FileName = "AdminLedgerPDF.pdf" };
        //    //}
        //    //catch
        //    //{
        //    //    return new ActionAsPdf("AdminLedgerPDF", new { from = 0 }) { FileName = "AdminLedgerPDF.pdf" };
        //    //}
        //    using (InventoryEntities db = new InventoryEntities())
        //    {
        //        var result = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate).ToList();
        //        ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
        //        return View("AdminLedgerPDF",result);
        //    }
        //}
        //[HttpGet]
        //public ActionResult AdminLedgerExcel(int ledgerId, string from, string To)
        //{
        //    using (InventoryEntities db = new InventoryEntities())
        //    {
        //        int companyid = Convert.ToInt32(Session["companyid"]);
        //        int Branchid = Convert.ToInt32(Session["BranchId"]);
        //        int fid = Convert.ToInt32(Session["fid"]);
        //        //int fid = 1;
        //        var culture = Session["DateCulture"].ToString();
        //        string dateFormat = Session["DateFormat"].ToString();
        //        var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
        //        var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
        //        Session["fdate"] = fdate;
        //        var result = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate).ToList();
        //        var sdate = (DateTime)db.FinancialYearMasters.Where(d => d.fYearID == fid).Select(d => d.sDate).FirstOrDefault();
        //        // ViewBag.FinanceYear = sdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
        //        ViewBag.FinanceYear = from;
        //        ViewBag.From = from;
        //        ViewBag.To = To;
        //        ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();

        //        Response.AddHeader("content-disposition", "attachment; filename=AdminAllLedger.xls");
        //        Response.ContentType = "application/ms-excel";
        //        return PartialView("AdminLedgerPDF", result);
        //    }


        //}

        //#endregion


        #region Admin Ledger Report
        [HttpGet]
        public ActionResult AdminLedger()
        {
            return View();
        }


        [HttpGet]
        public ActionResult AdminLedgerDetail(int ledgerId, string ledgerName, string from, string To)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            Session["fdate"] = fdate;
            ViewBag.LedgerName = ledgerName;
            ViewBag.LedgerId = ledgerId;
            ViewBag.From = from;
            ViewBag.To = To;
            using (InventoryEntities db = new InventoryEntities())
            {
                //var result= db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate);

                ViewBag.GroupResult = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate).OrderBy(o=>o.CreatedOn).ToList();
                var customer = db.Customers.Where(d => d.LId == ledgerId).FirstOrDefault();
                if (customer != null)
                {
                    ViewBag.Aadhar = customer.ServiceTaxNo;
                    ViewBag.Pan = customer.PanNo;
                    ViewBag.Address = customer.PoAddressName;
                }
            }
            return View("AdminLedger");
        }


        [HttpPost]
        public ActionResult AdminLedgerReport(int ledgerId, string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                // int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                //  List<DayBookReportModelView> sprList = new List<DayBookReportModelView>();
              
                var sdate = (DateTime)db.FinancialYearMasters.Where(d => d.fYearID == fid).Select(d => d.sDate).FirstOrDefault();
                ViewBag.FinanceYear = sdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                ViewBag.From = from;
                ViewBag.To = To;
                ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
                var customer = db.Customers.Where(d => d.LId == ledgerId).FirstOrDefault();
                if (customer != null)
                {
                    ViewBag.Aadhar = customer.ServiceTaxNo;
                    ViewBag.Pan = customer.PanNo;
                    ViewBag.Address = customer.PoAddressName;
                }
                List<AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report_Result> result = new List<AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report_Result>();
                if (fid == 10)
                {
                    result = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report(companyid, fid, ledgerId, fdate, tdate).ToList();
                    return PartialView(result);
                }
                else
                {
                    List<AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Result> result1 = new List<AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Result>();

                    result = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report(companyid, fid, ledgerId, fdate, tdate).ToList();
                    //result1 = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate).ToList();
                    //result = result1.Select(s => new AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report_Result
                    //{
                    //    BranchName = s.BranchName,
                    //    Remarks = s.Remarks,
                    //    LedgerName = s.LedgerName,
                    //    CreatedOn = s.CreatedOn,
                    //    RPdate = s.RPdate,
                    //    DRCR = s.DRCR,
                    //    OpeningBalance = s.OpeningBalance,
                    //    Credit = s.Credit,
                    //    Debit = s.Debit,
                    //    Id = s.Id,
                    //    RefNo = s.RefNo,
                    //    RPType = s.RPType,
                    //    VoucherNo = s.VoucherNo,
                    //    ChequeNo = s.ChequeNo

                    //}).ToList();
                    //foreach(var item in result1)
                    //{
                    //    AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report_Result lst = new AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report_Result();
                    //    lst.BranchName = item.BranchName;   
                    //    lst.Remarks=item.Remarks;
                    //    lst.LedgerName = item.LedgerName;
                    //    lst.CreatedOn = item.CreatedOn;
                    //    lst.RPdate = item.RPdate;
                    //    lst.DRCR = item.DRCR;
                    //    lst.OpeningBalance = item.OpeningBalance;
                    //    lst.Credit = item.Credit;
                    //    lst.Debit = item.Debit;
                    //    lst.Id = item.Id;
                    //    lst.RefNo = item.RefNo;
                    //    lst.RPType = item.RPType;
                    //    lst.VoucherNo = item.VoucherNo;
                    //    lst.ChequeNo = item.ChequeNo;
                    //    result.Add(lst);
                    //}

                    return PartialView(result);

                }
                //return PartialView(result);
            }
        }

        [HttpGet]
        public ActionResult AdminLedgerPDF(int companyid, int Branchid, int fid, int ledgerId, DateTime fdate, DateTime tdate, string dateRange)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                ViewBag.DateRange = dateRange;
                //var result = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate).ToList();
               
                ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
                var customer = db.Customers.Where(d => d.LId == ledgerId).FirstOrDefault();
                if (customer != null)
                {
                    ViewBag.Aadhar = customer.ServiceTaxNo;
                    ViewBag.Pan = customer.PanNo;
                    ViewBag.Address = customer.PoAddressName;
                }

                List<AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report_Result> result = new List<AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report_Result>();
                if (fid == 10)
                {
                    result = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report(companyid, fid, ledgerId, fdate, tdate).ToList();
                    return PartialView(result);
                }
                else
                {
                    List<AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Result> result1 = new List<AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Result>();

                    result = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report(companyid, fid, ledgerId, fdate, tdate).ToList();

                    //result1 = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate).ToList();
                    //result = result1.Select(s => new AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report_Result
                    //{
                    //    BranchName = s.BranchName,
                    //    Remarks = s.Remarks,
                    //    LedgerName = s.LedgerName,
                    //    CreatedOn = s.CreatedOn,
                    //    RPdate = s.RPdate,
                    //    DRCR = s.DRCR,
                    //    OpeningBalance = s.OpeningBalance,
                    //    Credit = s.Credit,
                    //    Debit = s.Debit,
                    //    Id = s.Id,
                    //    RefNo = s.RefNo,
                    //    RPType = s.RPType,
                    //    VoucherNo = s.VoucherNo,
                    //    ChequeNo = s.ChequeNo

                    //}).ToList();
                    //foreach (var item in result1)
                    //{
                    //    AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report_Result lst = new AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report_Result();
                    //    lst.BranchName = item.BranchName;
                    //    lst.Remarks = item.Remarks;
                    //    lst.LedgerName = item.LedgerName;
                    //    lst.CreatedOn = item.CreatedOn;
                    //    lst.RPdate = item.RPdate;
                    //    lst.DRCR = item.DRCR;
                    //    lst.OpeningBalance = item.OpeningBalance;
                    //    lst.Credit = item.Credit;
                    //    lst.Debit = item.Debit;
                    //    lst.Id = item.Id;
                    //    lst.RefNo = item.RefNo;
                    //    lst.RPType = item.RPType;
                    //    lst.VoucherNo = item.VoucherNo;
                    //    lst.ChequeNo = item.ChequeNo;
                    //    result.Add(lst);
                    //}

                    return PartialView(result);

                }
                // return View(result);
            }


        }

        [HttpGet]
        public ActionResult AdminLedgerPDFlink(int ledgerId, string from, string To)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            string dateRange = " From  " + from + "   To  " + To;
            Session["fdate"] = fdate;
            try
            {
                //using (InventoryEntities db = new InventoryEntities())
                //{
                //    ViewBag.DateRange = dateRange;
                //    var result = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate).ToList();
                //    ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
                //    var customer = db.Customers.Where(d => d.LId == ledgerId).FirstOrDefault();
                //    if (customer != null)
                //    {
                //        ViewBag.Aadhar = customer.ServiceTaxNo;
                //        ViewBag.Pan = customer.PanNo;
                //        ViewBag.Address = customer.PoAddressName;
                //    }

                //    return new Rotativa.PartialViewAsPdf("AdminLedgerPDF", result)
                //   {
                //        FileName = "AdminSubLedgerPDF1.pdf",
                //        PageSize = Size.A4,
                //        PageMargins = new Margins(30, 10, 10, 10),
                //        CustomSwitches = "--print-media-type  --footer-right [page]/[topage]"//customSwitches

                //    };
                return new ActionAsPdf("AdminLedgerPDF", new { companyid = companyid, Branchid = Branchid, fid = fid, ledgerId = ledgerId, fdate = fdate, tdate = tdate, dateRange = dateRange }) { FileName = "AdminLedgerPDF.pdf" };
                // }
            }
            catch
            {
                return new ActionAsPdf("AdminLedgerPDF", new { from = 0 }) { FileName = "AdminLedgerPDF.pdf" };
            }

        }
        [HttpGet]
        public ActionResult AdminLedgerExcel(int ledgerId, string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                //int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                //var result = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate).ToList();
                List<AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report_Result> result = new List<AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report_Result>();
                var sdate = (DateTime)db.FinancialYearMasters.Where(d => d.fYearID == fid).Select(d => d.sDate).FirstOrDefault();
                if (fid == 10)
                {
                    result = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report(companyid, fid, ledgerId, fdate, tdate).ToList();
                    //return PartialView(result);
                }
                else
                {
                    List<AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Result> result1 = new List<AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Result>();
                    result = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report(companyid, fid, ledgerId, fdate, tdate).ToList();
                    //result1 = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate).ToList();
                    //result = result1.Select(s => new AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report_Result
                    //{
                    //    BranchName = s.BranchName,
                    //    Remarks = s.Remarks,
                    //    LedgerName = s.LedgerName,
                    //    CreatedOn = s.CreatedOn,
                    //    RPdate = s.RPdate,
                    //    DRCR = s.DRCR,
                    //    OpeningBalance = s.OpeningBalance,
                    //    Credit = s.Credit,
                    //    Debit = s.Debit,
                    //    Id = s.Id,
                    //    RefNo = s.RefNo,
                    //    RPType = s.RPType,
                    //    VoucherNo = s.VoucherNo,
                    //    ChequeNo = s.ChequeNo

                    //}).ToList();
                    //foreach(var item in result1)
                    //{
                    //    AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report_Result lst = new AccountsBook_LedgerWise_DateRange_AllLedgerAdmin_Report_Result();
                    //    lst.BranchName = item.BranchName;   
                    //    lst.Remarks=item.Remarks;
                    //    lst.LedgerName = item.LedgerName;
                    //    lst.CreatedOn = item.CreatedOn;
                    //    lst.RPdate = item.RPdate;
                    //    lst.DRCR = item.DRCR;
                    //    lst.OpeningBalance = item.OpeningBalance;
                    //    lst.Credit = item.Credit;
                    //    lst.Debit = item.Debit;
                    //    lst.Id = item.Id;
                    //    lst.RefNo = item.RefNo;
                    //    lst.RPType = item.RPType;
                    //    lst.VoucherNo = item.VoucherNo;
                    //    lst.ChequeNo = item.ChequeNo;
                    //    result.Add(lst);
                    //}

                    //return PartialView(result);

                }
                
                // ViewBag.FinanceYear = sdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                ViewBag.FinanceYear = from;
                ViewBag.From = from;
                ViewBag.To = To;
                ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();

                Response.AddHeader("content-disposition", "attachment; filename=AdminAllLedger.xls");
                Response.ContentType = "application/ms-excel";
                return PartialView("AdminLedgerPDF", result);
            }


        }
        #endregion

        #region Admin Sub Ledger Report
        [HttpGet]
        public ActionResult AdminSubLedger()
        {
            return View();
        }


        [HttpGet]
        public ActionResult AdminSubLedgerDetail(int ledgerId, string ledgerName, string from, string To)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            Session["fdate"] = fdate;
            ViewBag.LedgerName = ledgerName;
            ViewBag.LedgerId = ledgerId;
            ViewBag.From = from;
            ViewBag.To = To;
            using (InventoryEntities db = new InventoryEntities())
            {
                ViewBag.GroupResult = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate).ToList();
            }
            return View("AdminSubLedger");
        }


        [HttpPost]
        public ActionResult AdminSubLedgerReport(int ledgerId, string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                // int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                ViewBag.From = from;
                ViewBag.To = To;
                Session["fdate"] = fdate;
                //  List<DayBookReportModelView> sprList = new List<DayBookReportModelView>();
                
                var result1 = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate);

                var result = result1.ToList();

                ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
                var getGroup = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.parentID).FirstOrDefault();
                if ( getGroup == 32)
                {
                    ViewBag.salesdetails = db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= fdate && d.SalesInvoice.Customer.LId==ledgerId && d.SalesInvoice.InvoiceDate <= tdate && d.SalesInvoice.FinancialYearId == fid).Select(d => new SalesInvoiceDetailModelView { 
                        SalesInvoiceId=d.SalesInvoiceId, 
                        BarCode = d.SalesInvoice.NO,
                        Description = d.Product.Group.Name, 
                        ItemName = d.Product.Name,
                        Quantity =  d.Quantity, 
                        UnitName = d.UnitIdSecondary == 1 ? (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM1.Code) : d.UOM1.Code,
                        UnitSecondaryName = d.UnitIdSecondary == 1 ? d.UOM2.Code : (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM1.Code),
                        UnitFormula = d.UnitIdSecondary == 1 ? d.Quantity : (d.Quantity * d.UnitFormula), Price = d.Price }).ToList();
                }
                else if(getGroup == 6)
                {
                    ViewBag.salesdetails = db.PurchaseInvoiceDetails.Where(d => d.PurchaseInvoice.InvoiceDate >= fdate && d.PurchaseInvoice.Supplier.LId == ledgerId && d.PurchaseInvoice.InvoiceDate <= tdate && d.PurchaseInvoice.FinancialYearId == fid).Select(d => new SalesInvoiceDetailModelView { 
                        SalesInvoiceId = d.PurchaseInvoiceId, 
                        BarCode = d.PurchaseInvoice.NO,
                        Description = d.Product.Group.Name, 
                        ItemName = d.Product.Name, 
                        Quantity =  d.Quantity, 
                        UnitName = d.UnitIdSecondary == 1 ? (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM.Code) : d.UOM2.Code, 
                        UnitSecondaryName = d.UnitIdSecondary == 1 ? d.UOM1.Code : (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM2.Code), 
                        UnitFormula = d.UnitIdSecondary == 1 ? d.Quantity : (d.Quantity * d.UnitFormula), Price = d.Price }).ToList();
                }
                return PartialView(result);
            }
        }

        [HttpGet]
        public ActionResult AdminSubLedgerPDF(int companyid, int Branchid, int fid, int ledgerId, DateTime fdate, DateTime tdate)
        {
            var culture = "es-AR";
            string dateFormat = "dd/MM/yyyy";
            using (InventoryEntities db = new InventoryEntities())
            {
                var result = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate).ToList();
                ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
                ViewBag.From = fdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                ViewBag.To = tdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var getGroup = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.parentID).FirstOrDefault();
                if ( getGroup == 32)
                {
                    ViewBag.salesdetails = db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= fdate && d.SalesInvoice.InvoiceDate <= tdate && d.SalesInvoice.FinancialYearId == fid).Select(d => new SalesInvoiceDetailModelView { SalesInvoiceId = d.SalesInvoiceId, BarCode = d.SalesInvoice.NO, Description = d.Product.Group.Name, ItemName = d.Product.Name, Quantity =  d.Quantity, UnitName = d.UnitIdSecondary == 1 ? (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM.Code) : d.UOM1.Code, UnitSecondaryName = d.UnitIdSecondary == 1 ? d.UOM1.Code : (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM.Code), UnitFormula = d.UnitIdSecondary == 1 ? d.Quantity : (d.Quantity * d.UnitFormula), Price = d.Price }).ToList();
                }
                else if (getGroup == 6)
                {
                    ViewBag.salesdetails = db.PurchaseInvoiceDetails.Where(d => d.PurchaseInvoice.InvoiceDate >= fdate && d.PurchaseInvoice.InvoiceDate <= tdate && d.PurchaseInvoice.FinancialYearId == fid).Select(d => new SalesInvoiceDetailModelView { SalesInvoiceId = d.PurchaseInvoiceId, BarCode = d.PurchaseInvoice.NO, Description = d.Product.Group.Name, ItemName = d.Product.Name, Quantity =  d.Quantity, UnitName = d.UnitIdSecondary == 1 ? (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM.Code) : d.UOM1.Code, UnitSecondaryName = d.UnitIdSecondary == 1 ? d.UOM1.Code : (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM.Code), UnitFormula = d.UnitIdSecondary == 1 ? d.Quantity : (d.Quantity * d.UnitFormula), Price = d.Price }).ToList();
                }
                return View(result);
            }

        }

        [HttpGet]
        public ActionResult AdminSubLedgerPDFlink(int ledgerId, string from, string To)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            Session["fdate"] = fdate;
            //string header = Server.MapPath("~/bin/PrintHeader.html");//Path of PrintHeader.html File
            //string footer = Server.MapPath("~/bin/PrintFooter.html");//Path of PrintFooter.html File

            //string customSwitches = string.Format("--header-html  \"{0}\" " +
            //                       "--header-spacing \"0\" " +
            //                       "--footer-html \"{1}\" " +
            //                       "--footer-spacing \"10\" " +
            //                       "--footer-font-size \"10\" " +
            //                       "--header-font-size \"10\" ", header, footer);
            //string customSwitches = string.Format("--header-html  \"{0}\" " +
            //                             "--header-spacing \"0\" " +
            //                             "--footer-center  Page: [page]/[toPage]\" " +
            //                             "--footer-line --footer-font-size \"9\" --footer-spacing 6 --footer-font-name \"calibri light\" ", Url.Action("PrintHeader", "FinancialReport", new { area = "" }, "http"));
            string header = Url.Action("PrintHeader", "FinancialReport", new { area = "" }, "http");
            string footer = Url.Action("PrintFooter", "FinancialReport", new { area = "" }, "http");

            string customSwitches = string.Format("--no-stop-slow-scripts --javascript-delay 1000 --footer-html {0} --footer-spacing 10", footer);
            //string customSwitches = string.Format("--header-html  \"{0}\" " +
            //                       "--header-spacing \"0\" " +
            //                       "--footer-html \"{1}\" " +
            //                       "--footer-spacing \"10\" " +
            //                       "--footer-font-size \"10\" " +
            //                       "--header-font-size \"10\" ",
            //   Url.Action("PrintHeader", "FinancialReport", new { area = "" }, "http"),Url.Action("PrintFooter", "FinancialReport", new { area = "" }, "http"));
            try
            {
                using (InventoryEntities db = new InventoryEntities())
                {
                    var result = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate).ToList();
                    ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
                    ViewBag.Address = db.Customers.Where(d => d.LId == ledgerId).Select(d => d.AddressName + ", " + d.StateRegion + ", " + d.PostalCode).FirstOrDefault();
                    ViewBag.From = fdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    ViewBag.To = tdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    var getGroup = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.parentID).FirstOrDefault();
                    if (getGroup == 32)
                    {
                        ViewBag.salesdetails = db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= fdate && d.SalesInvoice.InvoiceDate <= tdate && d.SalesInvoice.FinancialYearId == fid).Select(d => new SalesInvoiceDetailModelView {
                            SalesInvoiceId = d.SalesInvoiceId, 
                            BarCode = d.SalesInvoice.NO, 
                            Description = d.Product.Group.Name, 
                            ItemName = d.Product.Name,
                            Quantity = d.Quantity,
                            UnitName = d.UnitIdSecondary == 1 ? (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM.Code) : d.UOM1.Code, 
                            UnitSecondaryName = d.UnitIdSecondary == 1 ? d.UOM1.Code : (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM.Code), 
                            UnitFormula = d.UnitIdSecondary == 1 ? d.Quantity : (d.Quantity * d.UnitFormula), Price = d.Price }).ToList();
                    }
                    else if (getGroup == 6)
                    {
                        ViewBag.salesdetails = db.PurchaseInvoiceDetails.Where(d => d.PurchaseInvoice.InvoiceDate >= fdate && d.PurchaseInvoice.InvoiceDate <= tdate && d.PurchaseInvoice.FinancialYearId == fid).Select(d => new SalesInvoiceDetailModelView { SalesInvoiceId = d.PurchaseInvoiceId, BarCode = d.PurchaseInvoice.NO, Description = d.Product.Group.Name, ItemName = d.Product.Name, Quantity =  d.Quantity, UnitName = d.UnitIdSecondary == 1 ? (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM.Code) : d.UOM1.Code, UnitSecondaryName = d.UnitIdSecondary == 1 ? d.UOM1.Code : (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM.Code), UnitFormula = d.UnitIdSecondary == 1 ? d.Quantity : (d.Quantity * d.UnitFormula), Price = d.Price }).ToList();
                    }
                    //   return View(result);


                    return new Rotativa.PartialViewAsPdf("AdminSubLedgerPDF", result)
                    // return new ActionAsPdf("SubLedgerPDF", new { companyid = companyid, Branchid = Branchid, fid = fid, ledgerId = ledgerId, fdate = fdate, tdate = tdate })
                    {
                        FileName = "AdminSubLedgerPDF1.pdf",
                        PageSize = Size.A4,
                        PageMargins = new Margins(10, 10, 10, 10),
                        CustomSwitches = "--footer-right [page]/[topage]"//customSwitches
                        // PageMargins = new Margins(0, 0, 0, 0),
                        //CustomSwitches = "--print-media-type  --footer-center [page] --footer-font-size 8"

                    };
                    // "--header-center \"text\" --footer-right \"Page: [page] of [toPage]\"  --disable-smart-shrinking" };
                }
            }
            catch
            {
                return new ActionAsPdf("AdminSubLedgerPDF", new { from = 0 }) { FileName = "AdminSubLedgerPDF.pdf" };
            }
            //using (InventoryEntities db = new InventoryEntities())
            //{
            //    var result = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate).ToList();
            //    ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
            //    return View("AdminSubLedgerPDF", result);
            //}
        }
        [HttpGet]
        public ActionResult AdminSubLedgerExcel(int ledgerId, string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                //int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                var result = db.AccountsBook_LedgerWise_DateRange_AllLedgerAdmin(companyid, fid, ledgerId, fdate, tdate).ToList();
                var sdate = (DateTime)db.FinancialYearMasters.Where(d => d.fYearID == fid).Select(d => d.sDate).FirstOrDefault();
                // ViewBag.FinanceYear = sdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                ViewBag.FinanceYear = from;
                ViewBag.From = from;
                ViewBag.To = To;
                ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
                ViewBag.From = fdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                ViewBag.To = tdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var getGroup = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.parentID).FirstOrDefault();
                if (getGroup == 32)
                {
                    ViewBag.salesdetails = db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= fdate && d.SalesInvoice.InvoiceDate <= tdate && d.SalesInvoice.FinancialYearId == fid).Select(d => new SalesInvoiceDetailModelView { SalesInvoiceId = d.SalesInvoiceId, BarCode = d.SalesInvoice.NO, Description = d.Product.Group.Name, ItemName = d.Product.Name, Quantity =  d.Quantity, UnitName = d.UnitIdSecondary == 1 ? (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM.Code) : d.UOM1.Code, UnitSecondaryName = d.UnitIdSecondary == 1 ? d.UOM1.Code : (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM.Code), UnitFormula = d.UnitIdSecondary == 1 ? d.Quantity : (d.Quantity * d.UnitFormula), Price = d.Price }).ToList();
                }
                else if (getGroup == 6)
                {
                    ViewBag.salesdetails = db.PurchaseInvoiceDetails.Where(d => d.PurchaseInvoice.InvoiceDate >= fdate && d.PurchaseInvoice.InvoiceDate <= tdate && d.PurchaseInvoice.FinancialYearId == fid).Select(d => new SalesInvoiceDetailModelView { SalesInvoiceId = d.PurchaseInvoiceId, BarCode = d.PurchaseInvoice.NO, Description = d.Product.Group.Name, ItemName = d.Product.Name, Quantity =  d.Quantity, UnitName = d.UnitIdSecondary == 1 ? (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM.Code) : d.UOM1.Code, UnitSecondaryName = d.UnitIdSecondary == 1 ? d.UOM1.Code : (d.UnitId == d.UnitIdSecondary ? d.UOM2.Code : d.UOM.Code), UnitFormula = d.UnitIdSecondary == 1 ? d.Quantity : (d.Quantity * d.UnitFormula), Price = d.Price }).ToList();
                }

                Response.AddHeader("content-disposition", "attachment; filename=AdminSubLedger.xls");
                Response.ContentType = "application/ms-excel";
                return PartialView("AdminSubLedgerPDF", result);
            }


        }
        #endregion


        //#region Branch Ledger Report
        //[HttpGet]
        //public ActionResult BranchLedger()
        //{
        //    return View();
        //}

        //[HttpGet]
        //public ActionResult BranchLedgerDetail(int ledgerId, string ledgerName, string from, string To)
        //{
        //    int companyid = Convert.ToInt32(Session["companyid"]);
        //    int Branchid = Convert.ToInt32(Session["BranchId"]);
        //    int fid = Convert.ToInt32(Session["fid"]);
        //    var culture = Session["DateCulture"].ToString();
        //    string dateFormat = Session["DateFormat"].ToString();
        //    var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
        //    var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
        //    Session["fdate"] = fdate;
        //    ViewBag.LedgerName = ledgerName;
        //    ViewBag.LedgerId = ledgerId;
        //    ViewBag.From = from;
        //    ViewBag.To = To;
        //    using (InventoryEntities db = new InventoryEntities())
        //    {
        //        ViewBag.GroupResult = db.AccountsBook_LedgerWise_DateRange(companyid, Branchid, fid, ledgerId, fdate, tdate).ToList();
        //    }
        //    return View("BranchLedger");
        //}

        //[HttpPost]
        //public ActionResult BranchLedgerReport(int ledgerId,string from, string To)
        //{
        //    using (InventoryEntities db = new InventoryEntities())
        //    {
        //        int companyid = Convert.ToInt32(Session["companyid"]);
        //        int Branchid = Convert.ToInt32(Session["BranchId"]);
        //        int fid = Convert.ToInt32(Session["fid"]);
        //        // int fid = 1;
        //        var culture = Session["DateCulture"].ToString();
        //        string dateFormat = Session["DateFormat"].ToString();
        //        var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
        //        var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
        //        Session["fdate"] = fdate;
        //        //  List<DayBookReportModelView> sprList = new List<DayBookReportModelView>();
        //        var result = db.AccountsBook_LedgerWise_DateRange(companyid,Branchid, fid, ledgerId, fdate, tdate).ToList();
        //        //decimal? debitTotal = result.Sum(r => r.Debit) ?? 0;
        //        //decimal? creditTotal = result.Sum(r => r.Credit) ?? 0;
        //        //foreach (var item in result)
        //        //{
        //        //    var spr = new DayBookReportModelView();
        //        //    spr.Credit = item.Credit == 0 ? null : item.Credit;
        //        //    spr.Debit = item.Debit == 0 ? null : item.Debit;
        //        //    spr.ledgerName = item.ledgerName;
        //        //    spr.transactionType = item.transactionType;
        //        //    spr.VoucherNo = item.VoucherNo;
        //        //    spr.RPDate = item.RPDate;
        //        //    spr.DebitTotal = debitTotal;
        //        //    spr.CreditTotal = creditTotal;
        //        //    sprList.Add(spr);
        //        //}
        //        ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();
        //        return PartialView(result);
        //    }
        //}

        //#endregion

        #region Branch Balance Sheet Report
        [HttpGet]
        public ActionResult BranchBS()
        {
            return View();
        }

        [HttpPost]
        public ActionResult BranchBSReport(string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                //int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                //  List<DayBookReportModelView> sprList = new List<DayBookReportModelView>();
                var result = db.AccountsBook_GroupWise_BS_Current(companyid,Branchid, fid,fdate, tdate).ToList();
                //decimal? debitTotal = result.Sum(r => r.Debit) ?? 0;
                //decimal? creditTotal = result.Sum(r => r.Credit) ?? 0;
                //foreach (var item in result)
                //{
                //    var spr = new DayBookReportModelView();
                //    spr.Credit = item.Credit == 0 ? null : item.Credit;
                //    spr.Debit = item.Debit == 0 ? null : item.Debit;
                //    spr.ledgerName = item.ledgerName;
                //    spr.transactionType = item.transactionType;
                //    spr.VoucherNo = item.VoucherNo;
                //    spr.RPDate = item.RPDate;
                //    spr.DebitTotal = debitTotal;
                //    spr.CreditTotal = creditTotal;
                //    sprList.Add(spr);
                //}
               // ViewBag.GroupName = db.GroupMasters.Where(d => d.groupID == GroupId).Select(d => d.groupName).FirstOrDefault();
                ViewBag.GroupName = db.BranchMasters.Where(d => d.BranchId == Branchid).Select(d => d.Name).FirstOrDefault();
                return PartialView(result);
            }
        }



        #endregion

        #region Branch Profit Loss Report
        [HttpGet]
        public ActionResult BranchPL()
        {
            return View();
        }

        [HttpPost]
        public ActionResult BranchPLReport(string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                //int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                //  List<DayBookReportModelView> sprList = new List<DayBookReportModelView>();
                var result = db.AccountsBook_GroupWise_PL_Current(companyid, Branchid, fid, fdate, tdate).ToList();
                //decimal? debitTotal = result.Sum(r => r.Debit) ?? 0;
                //decimal? creditTotal = result.Sum(r => r.Credit) ?? 0;
                //foreach (var item in result)
                //{
                //    var spr = new DayBookReportModelView();
                //    spr.Credit = item.Credit == 0 ? null : item.Credit;
                //    spr.Debit = item.Debit == 0 ? null : item.Debit;
                //    spr.ledgerName = item.ledgerName;
                //    spr.transactionType = item.transactionType;
                //    spr.VoucherNo = item.VoucherNo;
                //    spr.RPDate = item.RPDate;
                //    spr.DebitTotal = debitTotal;
                //    spr.CreditTotal = creditTotal;
                //    sprList.Add(spr);
                //}
                // ViewBag.GroupName = db.GroupMasters.Where(d => d.groupID == GroupId).Select(d => d.groupName).FirstOrDefault();
                ViewBag.GroupName = db.BranchMasters.Where(d => d.BranchId == Branchid).Select(d => d.Name).FirstOrDefault();
                return PartialView(result);
            }
        }



        #endregion

        #region Branch Trial Balance Report
        [HttpGet]
        public ActionResult BranchTB()
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                ViewBag.Branch = db.BranchMasters.ToList();
                return View();
            }
        }

        [HttpPost]
        public ActionResult BranchTBReport(int? Branchid, string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                //int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                //int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                if (Branchid == null)
                    Branchid = 0;
                //  List<DayBookReportModelView> sprList = new List<DayBookReportModelView>();
                var result = db.AccountsBook_GroupWise_TrialBalance_Current(companyid, Branchid, fid, fdate, tdate);
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                var resultgroup = result.GroupBy(d => new { d.GroupTypeId, d.GroupId }).Select(d => new { d.Key.GroupTypeId, d.Key.GroupId, OpeningDebit = d.Sum(p => p.OpeningDebit), OpeningCredit = d.Sum(p => p.OpeningCredit), Debit = d.Sum(p => p.Debit), Credit = d.Sum(p => p.Credit), ClosingDebit = d.Sum(p => p.ClosingDebit), ClosingCredit = d.Sum(p => p.ClosingCredit) });
                var group = db.GroupMasters.Where(d => d.CompanyId == companyid).ToList();
                foreach (var ledger in resultgroup)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = null;
                    grp.LedgerName = group.FirstOrDefault(d => d.groupID == ledger.GroupId).groupName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultList.Add(grp);
                }

                ViewBag.From = from;
                ViewBag.To = To;
                ViewBag.BranchId = Branchid;
                ViewBag.GroupName = db.BranchMasters.Where(d => d.BranchId == Branchid).Select(d => d.Name).FirstOrDefault();
                return PartialView(resultList);
            }
        }

        [HttpGet]
        public ActionResult BranchTBPDF(int companyid, int Branchid, int fid, DateTime fdate, DateTime tdate, string dateRange)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                //var result = db.AccountsBook_GroupWise_Admin_TrialBalance_Current(companyid, fid, fdate, tdate).Where(d => d.Lid != 197);
                var result = db.AccountsBook_GroupWise_TrialBalance_Current(companyid, Branchid, fid, fdate, tdate);
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                var resultgroup = result.GroupBy(d => new { d.GroupTypeId, d.GroupId }).Select(d => new { d.Key.GroupTypeId, d.Key.GroupId, OpeningDebit = d.Sum(p => p.OpeningDebit), OpeningCredit = d.Sum(p => p.OpeningCredit), Debit = d.Sum(p => p.Debit), Credit = d.Sum(p => p.Credit), ClosingDebit = d.Sum(p => p.ClosingDebit), ClosingCredit = d.Sum(p => p.ClosingCredit) });
                var group = db.GroupMasters.Where(d => d.CompanyId == companyid).ToList();
                foreach (var ledger in resultgroup)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = null;
                    grp.LedgerName = group.FirstOrDefault(d => d.groupID == ledger.GroupId).groupName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultList.Add(grp);
                }
                ViewBag.DateRange = dateRange;
                if (Branchid == 0)
                {
                    ViewBag.Branch = "Head Office";
                }
                else
                {
                    ViewBag.Branch = db.BranchMasters.Where(d => d.Id == Branchid).Select(d => d.Name).FirstOrDefault();
                }
                return View(resultList);
            }


        }

        [HttpGet]
        public ActionResult BranchTBPDFlink(int? Branchid, string from, string To)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            //  int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            Session["fdate"] = fdate;
            string tbduration = " From  " + from + "   To  " + To;
            try
            {

                return new ActionAsPdf("BranchTBPDF", new { companyid = companyid, Branchid = Branchid, fid = fid, fdate = fdate, tdate = tdate, dateRange = tbduration }) { FileName = "AdminTBPDF.pdf" };
            }
            catch
            {
                return new ActionAsPdf("BranchTBPDF", new { from = 0 }) { FileName = "BranchTBPDF.pdf" };
            }

        }
        [HttpGet]
        public ActionResult BranchTBExcel(int? Branchid, string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                //  int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                //int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                var result = db.AccountsBook_GroupWise_TrialBalance_Current(companyid, Branchid, fid, fdate, tdate);
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                var resultgroup = result.GroupBy(d => new { d.GroupTypeId, d.GroupId }).Select(d => new { d.Key.GroupTypeId, d.Key.GroupId, OpeningDebit = d.Sum(p => p.OpeningDebit), OpeningCredit = d.Sum(p => p.OpeningCredit), Debit = d.Sum(p => p.Debit), Credit = d.Sum(p => p.Credit), ClosingDebit = d.Sum(p => p.ClosingDebit), ClosingCredit = d.Sum(p => p.ClosingCredit) });
                var group = db.GroupMasters.Where(d => d.CompanyId == companyid).ToList();
                foreach (var ledger in resultgroup)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = null;
                    grp.LedgerName = group.FirstOrDefault(d => d.groupID == ledger.GroupId).groupName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultList.Add(grp);
                }
                ViewBag.DateRange = " From  " + from + "   To  " + To;
                //return View(resultList);
                Response.AddHeader("content-disposition", "attachment; filename=TrialBalance.xls");
                Response.ContentType = "application/ms-excel";
                return PartialView("BranchTBPDF", resultList);
            }


        }


        #endregion

        #region Branch Trial Balance Drill Down Report
        [HttpGet]
        public ActionResult BranchTBDrillDown()
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                ViewBag.Branch = db.BranchMasters.ToList();
                return View();
            }
        }

        [HttpPost]
        public ActionResult BranchTBDrillDownReport(int? Branchid, string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
               // int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                //int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                //  List<DayBookReportModelView> sprList = new List<DayBookReportModelView>();
                //var result = db.AccountsBook_GroupWise_Admin_TrialBalance_Current(companyid, fid, fdate, tdate).Where(d=>d.Lid!=197).ToList();
                var result = db.AccountsBook_GroupWise_TrialBalance_Current(companyid, Branchid, fid, fdate, tdate).ToList();
                TrialBalanceDrillDownModelView resultTrial = new TrialBalanceDrillDownModelView();
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultDrillList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                var resultgroup = result.GroupBy(d => new { d.GroupTypeId, d.GroupId }).Select(d => new { d.Key.GroupTypeId, d.Key.GroupId, OpeningDebit = d.Sum(p => p.OpeningDebit), OpeningCredit = d.Sum(p => p.OpeningCredit), Debit = d.Sum(p => p.Debit), Credit = d.Sum(p => p.Credit), ClosingDebit = d.Sum(p => p.ClosingDebit), ClosingCredit = d.Sum(p => p.ClosingCredit) });
                var group = db.GroupMasters.Where(d => d.CompanyId == companyid).ToList();
                var ledgerList = db.LedgerMasters.Where(d => d.CompanyId == companyid).ToList();
                foreach (var ledger in resultgroup)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = null;
                    grp.LedgerName = group.FirstOrDefault(d => d.groupID == ledger.GroupId).groupName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultList.Add(grp);

                }
                var groupDetails = result.OrderBy(d => d.LedgerName).ToList();
                foreach (var ledger in groupDetails)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = ledger.Lid;
                    grp.LedgerName = ledger.LedgerName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultDrillList.Add(grp);
                }
                resultTrial.TrialGroups = resultList;
                resultTrial.TrialLedgers = resultDrillList;
                ViewBag.CompanyName = db.Companies.Select(d => d.Name).FirstOrDefault();
                ViewBag.From = from;
                ViewBag.To = To;
                ViewBag.BranchName = db.BranchMasters.Where(d => d.Id == Branchid).Select(d => d.Name).FirstOrDefault();
                ViewBag.FinancialYear = db.FinancialYearMasters.Where(d => d.fYearID == fid && d.CompanyId == companyid).Select(d => d.Year).FirstOrDefault();
                return PartialView(resultTrial);
            }
        }

        [HttpGet]
        public ActionResult BranchTBDrillDownPDF(int companyid, int Branchid, int fid, DateTime fdate, DateTime tdate, string dateRange)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                //var result = db.AccountsBook_GroupWise_Admin_TrialBalance_Current(companyid, fid, fdate, tdate).Where(d=>d.Lid!=197).ToList();
                var result = db.AccountsBook_GroupWise_TrialBalance_Current(companyid, Branchid, fid, fdate, tdate).ToList();
                TrialBalanceDrillDownModelView resultTrial = new TrialBalanceDrillDownModelView();
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultDrillList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                var resultgroup = result.GroupBy(d => new { d.GroupTypeId, d.GroupId }).Select(d => new { d.Key.GroupTypeId, d.Key.GroupId, OpeningDebit = d.Sum(p => p.OpeningDebit), OpeningCredit = d.Sum(p => p.OpeningCredit), Debit = d.Sum(p => p.Debit), Credit = d.Sum(p => p.Credit), ClosingDebit = d.Sum(p => p.ClosingDebit), ClosingCredit = d.Sum(p => p.ClosingCredit) });
                var group = db.GroupMasters.Where(d => d.CompanyId == companyid).ToList();
                var ledgerList = db.LedgerMasters.Where(d => d.CompanyId == companyid).ToList();
                foreach (var ledger in resultgroup)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = null;
                    grp.LedgerName = group.FirstOrDefault(d => d.groupID == ledger.GroupId).groupName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultList.Add(grp);

                }
                var groupDetails = result.OrderBy(d => d.LedgerName).ToList();
                foreach (var ledger in groupDetails)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = ledger.Lid;
                    grp.LedgerName = ledger.LedgerName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultDrillList.Add(grp);
                }
                resultTrial.TrialGroups = resultList;
                resultTrial.TrialLedgers = resultDrillList;
                ViewBag.DateRange = dateRange;
                var company = db.Companies.FirstOrDefault();
                ViewBag.CompanyName = company.Name;
                ViewBag.BranchName = db.BranchMasters.Where(d => d.Id == Branchid).Select(d => d.Name).FirstOrDefault();
                ViewBag.GSTNO = company.GST_VATNumber;
                //  ViewBag.FinancialYear = db.FinancialYearMasters.Where(d => d.fYearID == fid && d.CompanyId == companyid).Select(d => d.Year).FirstOrDefault();
                return View(resultTrial);
            }


        }

        [HttpGet]
        public ActionResult BranchTBDrillDownPDFlink(int? Branchid, string from, string To)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
           // int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            Session["fdate"] = fdate;
            string tbduration = " From  " + from + "   To  " + To;
            //try
            //{
            //    return new ActionAsPdf("AdminTBDrillDownPDF", new { companyid = companyid, Branchid = Branchid, fid = fid, fdate = fdate, tdate = tdate, dateRange = tbduration }) { FileName = "AdminTBDrillDownPDF.pdf" };
            //}
            //catch
            //{
            //    return new ActionAsPdf("AdminTBDrillDownPDF", new { from = 0 }) { FileName = "AdminTBDrillDownPDF.pdf" };
            //}
            using (InventoryEntities db = new InventoryEntities())
            {
                //var result = db.AccountsBook_GroupWise_Admin_TrialBalance_Current(companyid, fid, fdate, tdate).Where(d => d.Lid != 197).ToList();
                var result = db.AccountsBook_GroupWise_TrialBalance_Current(companyid, Branchid, fid, fdate, tdate).ToList();
                TrialBalanceDrillDownModelView resultTrial = new TrialBalanceDrillDownModelView();
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultDrillList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                var resultgroup = result.GroupBy(d => new { d.GroupTypeId, d.GroupId }).Select(d => new { d.Key.GroupTypeId, d.Key.GroupId, OpeningDebit = d.Sum(p => p.OpeningDebit), OpeningCredit = d.Sum(p => p.OpeningCredit), Debit = d.Sum(p => p.Debit), Credit = d.Sum(p => p.Credit), ClosingDebit = d.Sum(p => p.ClosingDebit), ClosingCredit = d.Sum(p => p.ClosingCredit) });
                var group = db.GroupMasters.Where(d => d.CompanyId == companyid).ToList();
                var ledgerList = db.LedgerMasters.Where(d => d.CompanyId == companyid).ToList();
                foreach (var ledger in resultgroup)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = null;
                    grp.LedgerName = group.FirstOrDefault(d => d.groupID == ledger.GroupId).groupName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultList.Add(grp);

                }
                var groupDetails = result.OrderBy(d => d.LedgerName).ToList();
                foreach (var ledger in groupDetails)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = ledger.Lid;
                    grp.LedgerName = ledger.LedgerName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultDrillList.Add(grp);
                }
                resultTrial.TrialGroups = resultList;
                resultTrial.TrialLedgers = resultDrillList;
                ViewBag.DateRange = tbduration;
                ViewBag.FinancialYear = db.FinancialYearMasters.Where(d => d.fYearID == fid && d.CompanyId == companyid).Select(d => d.Year).FirstOrDefault();
                // return View(resultTrial);
                var company = db.Companies.FirstOrDefault();
                ViewBag.CompanyName = company.Name;
                ViewBag.BranchName = db.BranchMasters.Where(d => d.Id == Branchid).Select(d => d.Name).FirstOrDefault();


                return new Rotativa.PartialViewAsPdf("BranchTBDrillDownPDF", resultTrial)
                // return new ActionAsPdf("SubLedgerPDF", new { companyid = companyid, Branchid = Branchid, fid = fid, ledgerId = ledgerId, fdate = fdate, tdate = tdate })
                {
                    FileName = "BranchTBDrillDownPDF.pdf",
                    PageSize = Size.A4,
                    PageMargins = new Margins(10, 10, 10, 10),
                    CustomSwitches = "--footer-right [page]/[topage]"//customSwitches
                                                                     // PageMargins = new Margins(0, 0, 0, 0),
                                                                     //CustomSwitches = "--print-media-type  --footer-center [page] --footer-font-size 8"

                };
            }

        }
        [HttpGet]
        public ActionResult BranchTBDrillDownExcel(int? Branchid, string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
              //  int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                //int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                string tbduration = " From  " + from + "   To  " + To;
                //var result = db.AccountsBook_GroupWise_Admin_TrialBalance_Current(companyid, fid, fdate, tdate).Where(d => d.Lid != 197).ToList();
                var result = db.AccountsBook_GroupWise_TrialBalance_Current(companyid, Branchid, fid, fdate, tdate).ToList();
                TrialBalanceDrillDownModelView resultTrial = new TrialBalanceDrillDownModelView();
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultDrillList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                var resultgroup = result.GroupBy(d => new { d.GroupTypeId, d.GroupId }).Select(d => new { d.Key.GroupTypeId, d.Key.GroupId, OpeningDebit = d.Sum(p => p.OpeningDebit), OpeningCredit = d.Sum(p => p.OpeningCredit), Debit = d.Sum(p => p.Debit), Credit = d.Sum(p => p.Credit), ClosingDebit = d.Sum(p => p.ClosingDebit), ClosingCredit = d.Sum(p => p.ClosingCredit) });
                var group = db.GroupMasters.Where(d => d.CompanyId == companyid).ToList();
                var ledgerList = db.LedgerMasters.Where(d => d.CompanyId == companyid).ToList();
                foreach (var ledger in resultgroup)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = null;
                    grp.LedgerName = group.FirstOrDefault(d => d.groupID == ledger.GroupId).groupName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultList.Add(grp);

                }
                var groupDetails = result.OrderBy(d => d.LedgerName).ToList();
                foreach (var ledger in groupDetails)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = ledger.Lid;
                    grp.LedgerName = ledger.LedgerName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultDrillList.Add(grp);
                }
                resultTrial.TrialGroups = resultList;
                resultTrial.TrialLedgers = resultDrillList;
                ViewBag.DateRange = tbduration;
                ViewBag.FinancialYear = db.FinancialYearMasters.Where(d => d.fYearID == fid && d.CompanyId == companyid).Select(d => d.Year).FirstOrDefault();
                // return View(resultTrial);
                var company = db.Companies.FirstOrDefault();
                ViewBag.CompanyName = company.Name;
                ViewBag.BranchName = db.BranchMasters.Where(d => d.Id == Branchid).Select(d => d.Name).FirstOrDefault();


                Response.AddHeader("content-disposition", "attachment; filename=BranchTBDrillDown.xls");
                Response.ContentType = "application/ms-excel";
                return PartialView("BranchTBDrillDownPDF", resultTrial);
            }


        }
        [HttpGet]
        public ActionResult BranchTBExcel(string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                //int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                var result = db.AccountsBook_GroupWise_TrialBalance_Current(companyid, Branchid, fid, fdate, tdate).ToList();
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                var resultgroup = result.GroupBy(d => new { d.GroupTypeId, d.GroupId }).Select(d => new { d.Key.GroupTypeId, d.Key.GroupId, OpeningDebit = d.Sum(p => p.OpeningDebit), OpeningCredit = d.Sum(p => p.OpeningCredit), Debit = d.Sum(p => p.Debit), Credit = d.Sum(p => p.Credit), ClosingDebit = d.Sum(p => p.ClosingDebit), ClosingCredit = d.Sum(p => p.ClosingCredit) });
                var group = db.GroupMasters.Where(d => d.CompanyId == companyid).ToList();
                foreach (var ledger in resultgroup)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = null;
                    grp.LedgerName = group.FirstOrDefault(d => d.groupID == ledger.GroupId).groupName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultList.Add(grp);
                }
                ViewBag.DateRange = " From  " + from + "   To  " + To;
                ViewBag.BranchName = db.BranchMasters.Where(d => d.Id == Branchid).Select(d => d.Name).FirstOrDefault();
                //return View(resultList);
                Response.AddHeader("content-disposition", "attachment; filename=BranchTrialBalance.xls");
                Response.ContentType = "application/ms-excel";
                return PartialView("BranchTBExcel", resultList);
            }


        }
        #endregion

        #region Admin Trial Balance Report
        [HttpGet]
        public ActionResult AdminTB()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AdminTBReport(string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                //int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                //  List<DayBookReportModelView> sprList = new List<DayBookReportModelView>();
                var result = db.AccountsBook_GroupWise_Admin_TrialBalance_Current(companyid, fid, fdate, tdate);
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                var resultgroup = result.GroupBy(d => new { d.GroupTypeId, d.GroupId }).Select(d => new { d.Key.GroupTypeId, d.Key.GroupId, OpeningDebit = d.Sum(p => p.OpeningDebit), OpeningCredit = d.Sum(p => p.OpeningCredit), Debit = d.Sum(p => p.Debit), Credit = d.Sum(p => p.Credit), ClosingDebit = d.Sum(p => p.ClosingDebit), ClosingCredit = d.Sum(p => p.ClosingCredit) });
                var group = db.GroupMasters.Where(d => d.CompanyId == companyid).ToList();
                foreach (var ledger in resultgroup)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = null;
                    grp.LedgerName = group.FirstOrDefault(d => d.groupID == ledger.GroupId).groupName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultList.Add(grp);
                }

                ViewBag.From = from;
                ViewBag.To = To;
                ViewBag.GroupName = db.BranchMasters.Where(d => d.BranchId == Branchid).Select(d => d.Name).FirstOrDefault();
                return PartialView(resultList);
            }
        }


        [HttpGet]
        public ActionResult AdminTBPDF(int companyid, int Branchid, int fid, DateTime fdate, DateTime tdate,string dateRange)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                var result = db.AccountsBook_GroupWise_Admin_TrialBalance_Current(companyid, fid, fdate, tdate);
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                var resultgroup = result.GroupBy(d => new { d.GroupTypeId, d.GroupId }).Select(d => new { d.Key.GroupTypeId, d.Key.GroupId, OpeningDebit = d.Sum(p => p.OpeningDebit), OpeningCredit = d.Sum(p => p.OpeningCredit), Debit = d.Sum(p => p.Debit), Credit = d.Sum(p => p.Credit), ClosingDebit = d.Sum(p => p.ClosingDebit), ClosingCredit = d.Sum(p => p.ClosingCredit) });
                var group = db.GroupMasters.Where(d => d.CompanyId == companyid).ToList();
                foreach (var ledger in resultgroup)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = null;
                    grp.LedgerName = group.FirstOrDefault(d => d.groupID == ledger.GroupId).groupName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultList.Add(grp);
                }
                ViewBag.DateRange = dateRange;
                return View(resultList);
            }


        }

        [HttpGet]
        public ActionResult AdminTBPDFlink(string from, string To)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            Session["fdate"] = fdate;
            string tbduration = " From  " + from + "   To  " + To;
            try
            {

                return new ActionAsPdf("AdminTBPDF", new { companyid = companyid, Branchid = Branchid, fid = fid, fdate = fdate, tdate = tdate, dateRange=tbduration }) { FileName = "AdminTBPDF.pdf" };
            }
            catch
            {
                return new ActionAsPdf("AdminTBPDF", new { from = 0 }) { FileName = "AdminTBPDF.pdf" };
            }

        }
        [HttpGet]
        public ActionResult AdminTBExcel(string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                //int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                var result = db.AccountsBook_GroupWise_Admin_TrialBalance_Current(companyid, fid, fdate, tdate);
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                var resultgroup = result.GroupBy(d => new { d.GroupTypeId, d.GroupId }).Select(d => new { d.Key.GroupTypeId, d.Key.GroupId, OpeningDebit = d.Sum(p => p.OpeningDebit), OpeningCredit = d.Sum(p => p.OpeningCredit), Debit = d.Sum(p => p.Debit), Credit = d.Sum(p => p.Credit), ClosingDebit = d.Sum(p => p.ClosingDebit), ClosingCredit = d.Sum(p => p.ClosingCredit) });
                var group = db.GroupMasters.Where(d => d.CompanyId == companyid).ToList();
                foreach (var ledger in resultgroup)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = null;
                    grp.LedgerName = group.FirstOrDefault(d => d.groupID == ledger.GroupId).groupName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultList.Add(grp);
                }
                ViewBag.DateRange = " From  " + from + "   To  " + To;
                //return View(resultList);
                Response.AddHeader("content-disposition", "attachment; filename=AdminAllTrialBalance.xls");
                Response.ContentType = "application/ms-excel";
                return PartialView("AdminTBPDF", resultList);
            }


        }

        #endregion

        #region Admin Trial Balance Drill Down Report
        [HttpGet]
        public ActionResult AdminTBDrillDown()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AdminTBDrillDownReport(string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                //int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                //  List<DayBookReportModelView> sprList = new List<DayBookReportModelView>();
                var result = db.AccountsBook_GroupWise_Admin_TrialBalance_Current(companyid, fid, fdate, tdate).ToList();
                TrialBalanceDrillDownModelView resultTrial = new TrialBalanceDrillDownModelView();
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultDrillList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                var resultgroup = result.GroupBy(d => new { d.GroupTypeId, d.GroupId }).Select(d => new { d.Key.GroupTypeId, d.Key.GroupId, OpeningDebit = d.Sum(p => p.OpeningDebit), OpeningCredit = d.Sum(p => p.OpeningCredit), Debit = d.Sum(p => p.Debit), Credit = d.Sum(p => p.Credit), ClosingDebit = d.Sum(p => p.ClosingDebit), ClosingCredit = d.Sum(p => p.ClosingCredit) });
                var group = db.GroupMasters.Where(d => d.CompanyId == companyid).ToList();
                var ledgerList = db.LedgerMasters.Where(d => d.CompanyId == companyid).ToList();
                foreach (var ledger in resultgroup)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = null;
                    grp.LedgerName = group.FirstOrDefault(d => d.groupID == ledger.GroupId).groupName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultList.Add(grp);

                }
                var groupDetails = result.OrderBy(d=>d.LedgerName).ToList();
                foreach (var ledger in groupDetails)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = ledger.Lid;
                    grp.LedgerName = ledger.LedgerName;//ledgerList.FirstOrDefault(d => d.LID == ledger.Lid).ledgerName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultDrillList.Add(grp);
                }
                resultTrial.TrialGroups = resultList;
                resultTrial.TrialLedgers = resultDrillList;
                ViewBag.From = from;
                ViewBag.To = To;
                ViewBag.GroupName = db.BranchMasters.Where(d => d.BranchId == Branchid).Select(d => d.Name).FirstOrDefault();
                ViewBag.FinancialYear = db.FinancialYearMasters.Where(d => d.fYearID == fid && d.CompanyId == companyid).Select(d => d.Year).FirstOrDefault();
                return PartialView(resultTrial);
            }
        }

        [HttpGet]
        public ActionResult AdminTBDrillDownPDF(int companyid, int Branchid, int fid, DateTime fdate, DateTime tdate, string dateRange)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                var result = db.AccountsBook_GroupWise_Admin_TrialBalance_Current(companyid, fid, fdate, tdate).ToList();
                TrialBalanceDrillDownModelView resultTrial = new TrialBalanceDrillDownModelView();
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultDrillList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                var resultgroup = result.GroupBy(d => new { d.GroupTypeId, d.GroupId }).Select(d => new { d.Key.GroupTypeId, d.Key.GroupId, OpeningDebit = d.Sum(p => p.OpeningDebit), OpeningCredit = d.Sum(p => p.OpeningCredit), Debit = d.Sum(p => p.Debit), Credit = d.Sum(p => p.Credit), ClosingDebit = d.Sum(p => p.ClosingDebit), ClosingCredit = d.Sum(p => p.ClosingCredit) });
                var group = db.GroupMasters.Where(d => d.CompanyId == companyid).ToList();
                var ledgerList = db.LedgerMasters.Where(d => d.CompanyId == companyid).ToList();
                foreach (var ledger in resultgroup)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = null;
                    grp.LedgerName = group.FirstOrDefault(d => d.groupID == ledger.GroupId).groupName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultList.Add(grp);

                }
                var groupDetails = result.ToList();
                foreach (var ledger in groupDetails)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = ledger.Lid;
                    grp.LedgerName = ledgerList.FirstOrDefault(d => d.LID == ledger.Lid).ledgerName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultDrillList.Add(grp);
                }
                resultTrial.TrialGroups = resultList;
                resultTrial.TrialLedgers = resultDrillList;
                ViewBag.DateRange = dateRange;
                ViewBag.FinancialYear = db.FinancialYearMasters.Where(d => d.fYearID == fid && d.CompanyId == companyid).Select(d => d.Year).FirstOrDefault();
                return View(resultTrial);
            }


        }

        [HttpGet]
        public ActionResult AdminTBDrillDownPDFlink(string from, string To)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            Session["fdate"] = fdate;
            string tbduration = " From  " + from + "   To  " + To;
            try
            {
                return new ActionAsPdf("AdminTBDrillDownPDF", new { companyid = companyid, Branchid = Branchid, fid = fid, fdate = fdate, tdate = tdate, dateRange = tbduration }) { FileName = "AdminTBDrillDownPDF.pdf" };
            }
            catch
            {
                return new ActionAsPdf("AdminTBDrillDownPDF", new { from = 0 }) { FileName = "AdminTBDrillDownPDF.pdf" };
            }

        }

        [HttpGet]
        public ActionResult AdminTBDrillDownExcel(string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                //int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                var result = db.AccountsBook_GroupWise_Admin_TrialBalance_Current(companyid, fid, fdate, tdate).ToList();
                TrialBalanceDrillDownModelView resultTrial = new TrialBalanceDrillDownModelView();
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                List<AccountsBook_GroupWise_TrialBalance_Current_Result> resultDrillList = new List<AccountsBook_GroupWise_TrialBalance_Current_Result>();
                var resultgroup = result.GroupBy(d => new { d.GroupTypeId, d.GroupId }).Select(d => new { d.Key.GroupTypeId, d.Key.GroupId, OpeningDebit = d.Sum(p => p.OpeningDebit), OpeningCredit = d.Sum(p => p.OpeningCredit), Debit = d.Sum(p => p.Debit), Credit = d.Sum(p => p.Credit), ClosingDebit = d.Sum(p => p.ClosingDebit), ClosingCredit = d.Sum(p => p.ClosingCredit) });
                var group = db.GroupMasters.Where(d => d.CompanyId == companyid).ToList();
                var ledgerList = db.LedgerMasters.Where(d => d.CompanyId == companyid).ToList();
                foreach (var ledger in resultgroup)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = null;
                    grp.LedgerName = group.FirstOrDefault(d => d.groupID == ledger.GroupId).groupName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultList.Add(grp);

                }
                var groupDetails = result.ToList();
                foreach (var ledger in groupDetails)
                {
                    var grp = new AccountsBook_GroupWise_TrialBalance_Current_Result();
                    grp.Lid = ledger.Lid;
                    grp.LedgerName = ledgerList.FirstOrDefault(d => d.LID == ledger.Lid).ledgerName;
                    grp.GroupTypeId = ledger.GroupTypeId;
                    grp.GroupId = ledger.GroupId;
                    grp.OpeningDebit = (ledger.OpeningDebit - ledger.OpeningCredit) == 0 ? 0 : (ledger.OpeningDebit - ledger.OpeningCredit) > 0 ? (ledger.OpeningDebit - ledger.OpeningCredit) : 0;
                    grp.OpeningCredit = (ledger.OpeningCredit - ledger.OpeningDebit) == 0 ? 0 : (ledger.OpeningCredit - ledger.OpeningDebit) > 0 ? (ledger.OpeningCredit - ledger.OpeningDebit) : 0;
                    grp.Debit = ledger.Debit;
                    grp.Credit = ledger.Credit;
                    grp.ClosingDebit = (ledger.ClosingDebit - ledger.ClosingCredit) == 0 ? 0 : (ledger.ClosingDebit - ledger.ClosingCredit) > 0 ? (ledger.ClosingDebit - ledger.ClosingCredit) : 0;
                    grp.ClosingCredit = (ledger.ClosingCredit - ledger.ClosingDebit) == 0 ? 0 : (ledger.ClosingCredit - ledger.ClosingDebit) > 0 ? (ledger.ClosingCredit - ledger.ClosingDebit) : 0;
                    resultDrillList.Add(grp);
                }
                resultTrial.TrialGroups = resultList;
                resultTrial.TrialLedgers = resultDrillList;
                
                ViewBag.FinancialYear = db.FinancialYearMasters.Where(d => d.fYearID == fid && d.CompanyId == companyid).Select(d => d.Year).FirstOrDefault();
                
                ViewBag.DateRange = " From  " + from + "   To  " + To;
             //   return View(resultTrial);
                //return View(resultList);
                Response.AddHeader("content-disposition", "attachment; filename=AdminAllDrillDownTrialBalance.xls");
                Response.ContentType = "application/ms-excel";
                return PartialView("AdminTBDrillDownPDF", resultTrial);
            }


        }
        #endregion
        #region Collection
        [HttpGet]
        public ActionResult DebtorCollection()
        {
            return View();
        }
        [HttpGet]
        public ActionResult DebtorCollectionReport(string from, string To)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            //int fid = 1;
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            Session["fdate"] = from;
            Session["tdate"] = To;
            using (InventoryEntities db = new InventoryEntities())
            {
                if (Branchid == 0)
                {
                    var result = (from r in db.ReceiptPayments.Where(d => d.RPType == "Receive" && d.transactionType == "General Receive" && d.RPdate >= fdate && d.RPdate <= tdate).ToList()
                                  join l in db.LedgerMasters.Where(d => d.groupID == 108).ToList()
                                  on (int)r.ledgerId equals l.LID
                                  join b in db.LedgerMasters.Where(d => d.groupID == 109).ToList()
                                  on r.RPBankId equals (int?)b.LID into rb
                                  from b in rb.DefaultIfEmpty()
                                  join br in db.BranchMasters.ToList()
                                  on r.BranchId equals (int?)br.Id
                                  select new VoucherModelView { branchCode = br.Name, chqDate = r.RPdate, Prefix = r.Prefix + r.VoucherNo, GeneralLedger = l.ledgerName, RPCashAmount = (r.RPCashAmount ?? 0), RPBankAmount = (r.RPBankAmount ?? 0), chequeNo = r.chequeNo, NeftRtgsNo = r.NeftRtgsNo, RPdate = r.chequeDate, RPBankName = (b != null ? b.ledgerName : "") }).OrderBy(d => d.chqDate).ToList();
                    return PartialView(result);
                }
                else
                {
                    var result = (from r in db.ReceiptPayments.Where(d => d.RPType == "Receive" && d.transactionType == "General Receive" && d.RPdate >= fdate && d.RPdate <= tdate && d.BranchId == Branchid).ToList()
                                  join l in db.LedgerMasters.Where(d => d.groupID == 108).ToList()
                                  on (int)r.ledgerId equals l.LID
                                  join b in db.LedgerMasters.Where(d => d.groupID == 109).ToList()
                                  on r.RPBankId equals (int?)b.LID into rb
                                  from b in rb.DefaultIfEmpty()
                                  join br in db.BranchMasters.ToList()
                                  on r.BranchId equals (int?)br.Id
                                  select new VoucherModelView { branchCode = br.Name, chqDate = r.RPdate, Prefix = r.Prefix + r.VoucherNo, GeneralLedger = l.ledgerName, RPCashAmount = (r.RPCashAmount ?? 0), RPBankAmount = (r.RPBankAmount ?? 0), chequeNo = r.chequeNo, NeftRtgsNo = r.NeftRtgsNo, RPdate = r.chequeDate, RPBankName = (b != null ? b.ledgerName : "") }).OrderBy(d => d.chqDate).ToList();
                    return PartialView(result);
                }
            }
        }
        [HttpGet]
        public ActionResult DebtorCollectionPDF(int id, DateTime? From, DateTime? To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                if (id == 0)
                {
                    //var result = (from r in db.ReceiptPayments.Where(d => d.RPType == "Receive" && d.transactionType == "General Receive" && d.RPdate >= fdate && d.RPdate <= tdate && d.BranchId == Branchid).ToList()
                    //              join l in db.LedgerMasters.Where(d => d.groupID == 108).ToList()
                    //              on (int)r.ledgerId equals l.LID
                    //              join b in db.LedgerMasters.Where(d => d.groupID == 109).ToList()
                    //              on r.RPBankId equals (int?)b.LID into rb
                    //              from b in rb.DefaultIfEmpty()
                    //              select new VoucherModelView { chqDate = r.RPdate, Prefix = r.Prefix + r.VoucherNo, GeneralLedger = l.ledgerName, RPCashAmount = (r.RPCashAmount ?? 0), RPBankAmount = (r.RPBankAmount ?? 0), chequeNo = r.chequeNo, NeftRtgsNo = r.NeftRtgsNo, RPdate = r.chequeDate, RPBankName = (b != null ? b.ledgerName : "") }).ToList();
                    return View();
                }
                else
                {
                    //var result = (from r in db.ReceiptPayments.Where(d => d.RPType == "Receive" && d.transactionType == "General Receive" && d.RPdate >= fdate && d.RPdate <= tdate && d.BranchId == Branchid).ToList()
                    //              join l in db.LedgerMasters.Where(d => d.groupID == 108).ToList()
                    //              on (int)r.ledgerId equals l.LID
                    //              join b in db.LedgerMasters.Where(d => d.groupID == 109).ToList()
                    //              on r.RPBankId equals (int?)b.LID into rb
                    //              from b in rb.DefaultIfEmpty()
                    //              select new VoucherModelView { chqDate = r.RPdate, Prefix = r.Prefix + r.VoucherNo, GeneralLedger = l.ledgerName, RPCashAmount = (r.RPCashAmount ?? 0), RPBankAmount = (r.RPBankAmount ?? 0), chequeNo = r.chequeNo, NeftRtgsNo = r.NeftRtgsNo, RPdate = r.chequeDate, RPBankName = (b != null ? b.ledgerName : "") }).ToList();
                    return View();
                }
            }
        }

        [HttpGet]
        public ActionResult DebtorCollectionPDFlink()
        {
            // int compid = Convert.ToInt32(Session["companyid"]);
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
                        var result = (from r in db.ReceiptPayments.Where(d => d.RPType == "Receive" && d.transactionType == "General Receive" && d.RPdate >= From && d.RPdate <= To).ToList()
                                      join l in db.LedgerMasters.Where(d => d.groupID == 108).ToList()
                                      on (int)r.ledgerId equals l.LID
                                      join b in db.LedgerMasters.Where(d => d.groupID == 109).ToList()
                                      on r.RPBankId equals (int?)b.LID into rb
                                      from b in rb.DefaultIfEmpty()
                                      join br in db.BranchMasters.ToList()
                                      on r.BranchId equals (int?)br.Id
                                      select new VoucherModelView { branchCode = br.Name, chqDate = r.RPdate, Prefix = r.Prefix + r.VoucherNo, GeneralLedger = l.ledgerName, RPCashAmount = (r.RPCashAmount ?? 0), RPBankAmount = (r.RPBankAmount ?? 0), chequeNo = r.chequeNo, NeftRtgsNo = r.NeftRtgsNo, RPdate = r.chequeDate, RPBankName = (b != null ? b.ledgerName : "") }).OrderBy(d => d.chqDate).ToList();
                        return new Rotativa.PartialViewAsPdf("DebtorCollectionPDF", result)
                        {
                            FileName = "SalesbyDatePDF.pdf",
                            PageSize = Size.A4,
                            PageMargins = new Margins(10, 10, 10, 10),
                            CustomSwitches = "--footer-right [page]/[topage]"//customSwitches
                                                                             // PageMargins = new Margins(0, 0, 0, 0),
                                                                             //CustomSwitches = "--print-media-type  --footer-center [page] --footer-font-size 8"

                        };
                    }
                    else
                    {
                        var result = (from r in db.ReceiptPayments.Where(d => d.RPType == "Receive" && d.transactionType == "General Receive" && d.RPdate >= From && d.RPdate <= To && d.BranchId == Branchid).ToList()
                                      join l in db.LedgerMasters.Where(d => d.groupID == 108).ToList()
                                      on (int)r.ledgerId equals l.LID
                                      join b in db.LedgerMasters.Where(d => d.groupID == 109).ToList()
                                      on r.RPBankId equals (int?)b.LID into rb
                                      from b in rb.DefaultIfEmpty()
                                      join br in db.BranchMasters.ToList()
                                      on r.BranchId equals (int?)br.Id
                                      select new VoucherModelView { branchCode = br.Name, chqDate = r.RPdate, Prefix = r.Prefix + r.VoucherNo, GeneralLedger = l.ledgerName, RPCashAmount = (r.RPCashAmount ?? 0), RPBankAmount = (r.RPBankAmount ?? 0), chequeNo = r.chequeNo, NeftRtgsNo = r.NeftRtgsNo, RPdate = r.chequeDate, RPBankName = (b != null ? b.ledgerName : "") }).OrderBy(d => d.chqDate).ToList();

                        return new Rotativa.PartialViewAsPdf("DebtorCollectionPDF", result)
                        {
                            FileName = "DebtorCollection.pdf",
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
                return new ActionAsPdf("DebtorCollectionPDF", new { id = Branchid, value = "0", From = From, To = To }) { FileName = "DebtorCollection.pdf" };
            }
        

        }
        [HttpGet]
        public ActionResult DebtorCollectionExcel(string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                //int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                List<VoucherModelView> result = new List<VoucherModelView>();

                if (Branchid == 0)
                {
                    result = (from r in db.ReceiptPayments.Where(d => d.RPType == "Receive" && d.transactionType == "General Receive" && d.RPdate >= fdate && d.RPdate <= tdate).ToList()
                              join l in db.LedgerMasters.Where(d => d.groupID == 108).ToList()
                              on (int)r.ledgerId equals l.LID
                              join b in db.LedgerMasters.Where(d => d.groupID == 109).ToList()
                              on r.RPBankId equals (int?)b.LID into rb
                              from b in rb.DefaultIfEmpty()
                              join br in db.BranchMasters.ToList()
                              on r.BranchId equals (int?)br.Id
                              select new VoucherModelView { branchCode = br.Name, chqDate = r.RPdate, Prefix = r.Prefix + r.VoucherNo, GeneralLedger = l.ledgerName, RPCashAmount = (r.RPCashAmount ?? 0), RPBankAmount = (r.RPBankAmount ?? 0), chequeNo = r.chequeNo, NeftRtgsNo = r.NeftRtgsNo, RPdate = r.chequeDate, RPBankName = (b != null ? b.ledgerName : "") }).OrderBy(d => d.chqDate).ToList();

                }
                else
                {
                    result = (from r in db.ReceiptPayments.Where(d => d.RPType == "Receive" && d.transactionType == "General Receive" && d.RPdate >= fdate && d.RPdate <= tdate && d.BranchId == Branchid).ToList()
                              join l in db.LedgerMasters.Where(d => d.groupID == 108).ToList()
                              on (int)r.ledgerId equals l.LID
                              join b in db.LedgerMasters.Where(d => d.groupID == 109).ToList()
                              on r.RPBankId equals (int?)b.LID into rb
                              from b in rb.DefaultIfEmpty()
                              join br in db.BranchMasters.ToList()
                              on r.BranchId equals (int?)br.Id
                              select new VoucherModelView { branchCode = br.Name, chqDate = r.RPdate, Prefix = r.Prefix + r.VoucherNo, GeneralLedger = l.ledgerName, RPCashAmount = (r.RPCashAmount ?? 0), RPBankAmount = (r.RPBankAmount ?? 0), chequeNo = r.chequeNo, NeftRtgsNo = r.NeftRtgsNo, RPdate = r.chequeDate, RPBankName = (b != null ? b.ledgerName : "") }).OrderBy(d => d.chqDate).ToList();

                }

                // ViewBag.FinanceYear = sdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                ViewBag.FinanceYear = from;
                ViewBag.From = from;
                ViewBag.To = To;
                //  ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();

                Response.AddHeader("content-disposition", "attachment; filename=CashBank.xls");
                Response.ContentType = "application/ms-excel";
                return PartialView("DebtorCollectionPDF", result);
            }


        }
        #endregion
        #region Payment
        [HttpGet]
        public ActionResult Payment()
        {
            return View();
        }
        [HttpGet]
        public ActionResult PaymentReport(string from, string To)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int fid = Convert.ToInt32(Session["fid"]);
            //int fid = 1;
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            Session["fdate"] = from;
            Session["tdate"] = To;
            using (InventoryEntities db = new InventoryEntities())
            {
                if (Branchid == 0)
                {
                    var result = (from r in db.ReceiptPayments.Where(d => d.RPType == "Payment" && d.transactionType == "General Payment" && d.RPdate >= fdate && d.RPdate <= tdate).ToList()
                                  join l in db.LedgerMasters.ToList()
                                  on (int)r.ledgerId equals l.LID
                                  join b in db.LedgerMasters.Where(d => d.groupID == 109).ToList()
                                  on r.RPBankId equals (int?)b.LID into rb
                                  from b in rb.DefaultIfEmpty()
                                  select new VoucherModelView { chqDate = r.RPdate, Prefix = r.Prefix + r.VoucherNo, GeneralLedger = l.ledgerName, RPCashAmount = (r.RPCashAmount ?? 0), RPBankAmount = (r.RPBankAmount ?? 0), chequeNo = r.chequeNo, NeftRtgsNo = r.NeftRtgsNo, RPdate = r.chequeDate, RPBankName = (b != null ? b.ledgerName : "") }).OrderBy(d => d.chqDate).ToList();
                    return PartialView(result);
                }
                else
                {
                    var result = (from r in db.ReceiptPayments.Where(d => d.RPType == "Payment" && d.transactionType == "General Payment" && d.RPdate >= fdate && d.RPdate <= tdate && d.BranchId == Branchid).ToList()
                                  join l in db.LedgerMasters.ToList()
                                  on (int)r.ledgerId equals l.LID
                                  join b in db.LedgerMasters.Where(d => d.groupID == 109).ToList()
                                  on r.RPBankId equals (int?)b.LID into rb
                                  from b in rb.DefaultIfEmpty()
                                  select new VoucherModelView { chqDate = r.RPdate, Prefix = r.Prefix + r.VoucherNo, GeneralLedger = l.ledgerName, RPCashAmount = (r.RPCashAmount ?? 0), RPBankAmount = (r.RPBankAmount ?? 0), chequeNo = r.chequeNo, NeftRtgsNo = r.NeftRtgsNo, RPdate = r.chequeDate, RPBankName = (b != null ? b.ledgerName : "") }).OrderBy(d => d.chqDate).ToList();
                    return PartialView(result);
                }
            }
        }
        [HttpGet]
        public ActionResult PaymentPDF(int id, DateTime? From, DateTime? To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                if (id == 0)
                {
                    //var result = (from r in db.ReceiptPayments.Where(d => d.RPType == "Receive" && d.transactionType == "General Receive" && d.RPdate >= fdate && d.RPdate <= tdate && d.BranchId == Branchid).ToList()
                    //              join l in db.LedgerMasters.Where(d => d.groupID == 108).ToList()
                    //              on (int)r.ledgerId equals l.LID
                    //              join b in db.LedgerMasters.Where(d => d.groupID == 109).ToList()
                    //              on r.RPBankId equals (int?)b.LID into rb
                    //              from b in rb.DefaultIfEmpty()
                    //              select new VoucherModelView { chqDate = r.RPdate, Prefix = r.Prefix + r.VoucherNo, GeneralLedger = l.ledgerName, RPCashAmount = (r.RPCashAmount ?? 0), RPBankAmount = (r.RPBankAmount ?? 0), chequeNo = r.chequeNo, NeftRtgsNo = r.NeftRtgsNo, RPdate = r.chequeDate, RPBankName = (b != null ? b.ledgerName : "") }).ToList();
                    return View();
                }
                else
                {
                    //var result = (from r in db.ReceiptPayments.Where(d => d.RPType == "Receive" && d.transactionType == "General Receive" && d.RPdate >= fdate && d.RPdate <= tdate && d.BranchId == Branchid).ToList()
                    //              join l in db.LedgerMasters.Where(d => d.groupID == 108).ToList()
                    //              on (int)r.ledgerId equals l.LID
                    //              join b in db.LedgerMasters.Where(d => d.groupID == 109).ToList()
                    //              on r.RPBankId equals (int?)b.LID into rb
                    //              from b in rb.DefaultIfEmpty()
                    //              select new VoucherModelView { chqDate = r.RPdate, Prefix = r.Prefix + r.VoucherNo, GeneralLedger = l.ledgerName, RPCashAmount = (r.RPCashAmount ?? 0), RPBankAmount = (r.RPBankAmount ?? 0), chequeNo = r.chequeNo, NeftRtgsNo = r.NeftRtgsNo, RPdate = r.chequeDate, RPBankName = (b != null ? b.ledgerName : "") }).ToList();
                    return View();
                }
            }
        }

        [HttpGet]
        public ActionResult PaymentPDFlink()
        {
            // int compid = Convert.ToInt32(Session["companyid"]);
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
                    ViewBag.CompanyName = db.Companies.Select(d => d.Name).FirstOrDefault();
                    if (Branchid == 0)
                    {
                        var result = (from r in db.ReceiptPayments.Where(d => d.RPType == "Payment" && d.transactionType == "General Payment" && d.RPdate >= From && d.RPdate <= To).ToList()
                                      join l in db.LedgerMasters.ToList()
                                      on (int)r.ledgerId equals l.LID
                                      join b in db.LedgerMasters.Where(d => d.groupID == 109).ToList()
                                      on r.RPBankId equals (int?)b.LID into rb
                                      from b in rb.DefaultIfEmpty()
                                      select new VoucherModelView { chqDate = r.RPdate, Prefix = r.Prefix + r.VoucherNo, GeneralLedger = l.ledgerName, RPCashAmount = (r.RPCashAmount ?? 0), RPBankAmount = (r.RPBankAmount ?? 0), chequeNo = r.chequeNo, NeftRtgsNo = r.NeftRtgsNo, RPdate = r.chequeDate, RPBankName = (b != null ? b.ledgerName : "") }).OrderBy(d => d.chqDate).ToList();
                        return new Rotativa.PartialViewAsPdf("PaymentPDF", result)
                        {
                            FileName = "PaymentPDF.pdf",
                            PageSize = Size.A4,
                            PageMargins = new Margins(10, 10, 10, 10),
                            CustomSwitches = "--footer-right [page]/[topage]"//customSwitches
                                                                             // PageMargins = new Margins(0, 0, 0, 0),
                                                                             //CustomSwitches = "--print-media-type  --footer-center [page] --footer-font-size 8"

                        };
                    }
                    else
                    {
                        var result = (from r in db.ReceiptPayments.Where(d => d.RPType == "Payment" && d.transactionType == "General Payment" && d.RPdate >= From && d.RPdate <= To && d.BranchId == Branchid).ToList()
                                      join l in db.LedgerMasters.ToList()
                                      on (int)r.ledgerId equals l.LID
                                      join b in db.LedgerMasters.Where(d => d.groupID == 109).ToList()
                                      on r.RPBankId equals (int?)b.LID into rb
                                      from b in rb.DefaultIfEmpty()
                                      select new VoucherModelView { chqDate = r.RPdate, Prefix = r.Prefix + r.VoucherNo, GeneralLedger = l.ledgerName, RPCashAmount = (r.RPCashAmount ?? 0), RPBankAmount = (r.RPBankAmount ?? 0), chequeNo = r.chequeNo, NeftRtgsNo = r.NeftRtgsNo, RPdate = r.chequeDate, RPBankName = (b != null ? b.ledgerName : "") }).OrderBy(d => d.chqDate).ToList();

                        return new Rotativa.PartialViewAsPdf("PaymentPDF", result)
                        {
                            FileName = "Payment.pdf",
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
                return new ActionAsPdf("PaymentPDF", new { id = Branchid, value = "0", From = From, To = To }) { FileName = "Payment.pdf" };
            }


        }
        [HttpGet]
        public ActionResult PaymentExcel(string from, string To)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                //int fid = 1;
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;
                List<VoucherModelView> result = new List<VoucherModelView>();

                if (Branchid == 0)
                {
                    result = (from r in db.ReceiptPayments.Where(d => d.RPType == "Payment" && d.transactionType == "General Payment" && d.RPdate >= fdate && d.RPdate <= tdate).ToList()
                              join l in db.LedgerMasters.ToList()
                              on (int)r.ledgerId equals l.LID
                              join b in db.LedgerMasters.Where(d => d.groupID == 109).ToList()
                              on r.RPBankId equals (int?)b.LID into rb
                              from b in rb.DefaultIfEmpty()
                              select new VoucherModelView { chqDate = r.RPdate, Prefix = r.Prefix + r.VoucherNo, GeneralLedger = l.ledgerName, RPCashAmount = (r.RPCashAmount ?? 0), RPBankAmount = (r.RPBankAmount ?? 0), chequeNo = r.chequeNo, NeftRtgsNo = r.NeftRtgsNo, RPdate = r.chequeDate, RPBankName = (b != null ? b.ledgerName : "") }).OrderBy(d => d.chqDate).ToList();

                }
                else
                {
                    result = (from r in db.ReceiptPayments.Where(d => d.RPType == "Payment" && d.transactionType == "General Payment" && d.RPdate >= fdate && d.RPdate <= tdate && d.BranchId == Branchid).ToList()
                              join l in db.LedgerMasters.ToList()
                              on (int)r.ledgerId equals l.LID
                              join b in db.LedgerMasters.Where(d => d.groupID == 109).ToList()
                              on r.RPBankId equals (int?)b.LID into rb
                              from b in rb.DefaultIfEmpty()
                              select new VoucherModelView { chqDate = r.RPdate, Prefix = r.Prefix + r.VoucherNo, GeneralLedger = l.ledgerName, RPCashAmount = (r.RPCashAmount ?? 0), RPBankAmount = (r.RPBankAmount ?? 0), chequeNo = r.chequeNo, NeftRtgsNo = r.NeftRtgsNo, RPdate = r.chequeDate, RPBankName = (b != null ? b.ledgerName : "") }).OrderBy(d => d.chqDate).ToList();

                }

                // ViewBag.FinanceYear = sdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                ViewBag.FinanceYear = from;
                ViewBag.From = from;
                ViewBag.To = To;
                //  ViewBag.LedgerName = db.LedgerMasters.Where(d => d.LID == ledgerId).Select(d => d.ledgerName).FirstOrDefault();

                Response.AddHeader("content-disposition", "attachment; filename=Payment.xls");
                Response.ContentType = "application/ms-excel";
                return PartialView("PaymentPDF", result);
            }


        }
        #endregion
        #region Sales Analysis

        [HttpGet]
        public ActionResult SalesAnalysis(string Msg, string Err)
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
            using (InventoryEntities db = new InventoryEntities())
            {
                ViewBag.Customer = db.ProductCategory_MSTR.Where(d => d.CompanyId == companyid).ToList();
                return View();
            }

        }

        [HttpGet]
        public ActionResult SalesAnalysisReport( string from, string To, int? CategoryId)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var fdate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                var tdate = DateTime.ParseExact(To, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                Session["fdate"] = fdate;

                //  var result = db.AccountsBook_GroupWise_DateRangeSK(companyid, Branchid, fid, 0, fdate, tdate).ToList();
                // var result = db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= fdate && d.SalesInvoice.InvoiceDate <= tdate && d.SalesInvoice.BranchId == Branchid && d.Product.CategoryId==CategoryId).GroupBy(d=>d.SalesInvoice.Customer.LId).Select(d=> new {LID = d.Key, Quantity = d.Sum(l=>l.Quantity),  } ).ToList();
           //     ViewBag.GroupName = db.GroupMasters.Where(d => d.groupID == GroupId).Select(d => d.groupName).FirstOrDefault();


                if (CategoryId == null)
                {
                   // var customers = db.Customers.Select(d => new { LId = d.LId, PId = d.PId == null ? d.LId : d.PId, Name = d.Name }).ToList();
                    var resultview = db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= fdate && d.SalesInvoice.InvoiceDate <= tdate && d.SalesInvoice.BranchId == Branchid).GroupBy(d => new { d.SalesInvoice.Customer.Name, d.UOM1.Code, PId = (d.SalesInvoice.Customer.PId == null ? d.SalesInvoice.Customer.LId : d.SalesInvoice.Customer.PId) }).Select(d => new SalesInvoiceDetailModelView { ItemName = d.Key.Name, UnitName = d.Key.Code, ItemId = d.Key.PId ?? 0, Quantity = d.Sum(l => l.Quantity), QuantityLeft = (d.Sum(l => l.Quantity * ((l.UnitFormula == 0 || l.UnitFormula == 1) ? l.SecUnitFormula : l.UnitFormula)) ?? 0), TotalAmount = d.Sum(l => l.Quantity * l.Price) }).ToList();
                    var result = resultview.GroupBy(d => d.ItemId).Select(d => new SalesInvoiceDetailModelView { ItemId = d.Key, Quantity = d.Sum(l => l.Quantity), QuantityLeft = d.Sum(l => l.QuantityLeft), TotalAmount = d.Sum(l => l.TotalAmount) }).ToList();
                    //var groupresult = (from r in result
                    //                   join c in customers
                    //                   on r.TaxId equals c.LId
                    //                   group r by c.PId into g
                    //                   select new SalesInvoiceDetailModelView { ItemId = g.Key??0, TaxId = result., LedgerName = c.Name, DRCR = r.DRCR, Opening = r.Opening, Debit = r.Debit, Credit = r.Credit, Closing = r.Closing, ClosingDRCR = r.ClosingDRCR }).OrderBy(d => d.GroupId).ToList();
                    ViewBag.resultview = resultview;
                    return PartialView(result);
                }
                else
                {
                    var resultview = db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= fdate && d.SalesInvoice.InvoiceDate <= tdate && d.SalesInvoice.BranchId == Branchid && d.Product.CategoryId == CategoryId).GroupBy(d => new { d.SalesInvoice.Customer.Name, d.UOM1.Code, PId = (d.SalesInvoice.Customer.PId == null ? d.SalesInvoice.Customer.LId : d.SalesInvoice.Customer.PId) }).Select(d => new SalesInvoiceDetailModelView { ItemName = d.Key.Name, UnitName = d.Key.Code, ItemId = d.Key.PId ?? 0, Quantity = d.Sum(l => l.Quantity), QuantityLeft = (d.Sum(l => l.Quantity * ((l.UnitFormula == 0 || l.UnitFormula == 1) ? l.SecUnitFormula : l.UnitFormula)) ?? 0), TotalAmount=d.Sum(l=>l.Quantity*l.Price) }).ToList();
                    var result = resultview.GroupBy(d => d.ItemId).Select(d => new SalesInvoiceDetailModelView { ItemId = d.Key, Quantity = d.Sum(l => l.Quantity), QuantityLeft = d.Sum(l => l.QuantityLeft),TotalAmount=d.Sum(l=>l.TotalAmount) }).ToList();

                    //var result = db.SalesInvoiceDetails.Where(d => d.SalesInvoice.InvoiceDate >= fdate && d.SalesInvoice.InvoiceDate <= tdate && d.SalesInvoice.BranchId == Branchid && d.Product.CategoryId == CategoryId).GroupBy(d => d.SalesInvoice.Customer.LId).Select(d => new { LID = d.Key, Quantity = d.Sum(l => l.Quantity), }).ToList();
                    //var customers = db.Customers.Where(d => d.LId == LedgerId || d.PId == LedgerId).Select(d => new { LId = (int?)d.LId, PId = d.PId == null ? d.LId : d.PId }).ToList();
                    //var groupresult = (from r in result
                    //                   join c in customers
                    //                   on r.Lid equals c.LId
                    //                   select new AccountsBook_GroupWise_DateRange_Result { GroupId = (int?)c.PId, Lid = r.Lid, LedgerName = r.LedgerName, DRCR = r.DRCR, Opening = r.Opening, Debit = r.Debit, Credit = r.Credit, Closing = r.Closing, ClosingDRCR = r.ClosingDRCR }).OrderBy(d => d.GroupId).ToList();
                    ViewBag.resultview = resultview;
                    return PartialView(result);
                }


            }
        }
        #endregion

  
      
    }
}

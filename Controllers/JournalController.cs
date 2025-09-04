using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Transactions;
using System.Globalization;

namespace XenERP.Controllers
{
    [SessionExpire]
    public class JournalController : Controller
    {
        private TransactionClasses tc = new TransactionClasses();
        //
        // GET: /Journal/

        [HttpGet]
        public ActionResult CreateJournal()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateJournal(Journal model)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            int fid = Convert.ToInt32(Session["fid"]);
            string createdby = Convert.ToString(Session["Createdid"]);

            //string JDate = Convert.ToString(model.JournalDate);

            //var culture = Session["DateCulture"].ToString();
            //string dateFormat = Session["DateFormat"].ToString();
            //var fdate = DateTime.ParseExact(JDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));

            using (InventoryEntities db = new InventoryEntities())
            {
                int countpo = 1;
                if (db.Journals.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.FinancialYearId == fid).Count() != 0)
                {
                    countpo = db.Journals.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.FinancialYearId == fid).Count();
                }
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {

                        model.JournalCode = tc.GenerateCode("JOR", countpo);
                        model.BranchId = Branchid;
                        model.CompanyId = companyid;
                        model.UserId = userid;
                        model.CreatedBy = createdby;
                        model.CreatedOn = DateTime.Now;
                        //model.JournalDate = fdate;
                        db.Journals.Add(model);
                        db.SaveChanges();
                        scope.Complete();
                        long id = model.Id;
                        return RedirectToAction("ShowAllJournal", new { Msg = "New Journal has been created successfully...", JName = model.JournalName, CDate = model.JournalDate, id = id });
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        return RedirectToAction("ShowAllJournal", new { Err = "New Journal has not been created successfully..." });
                        throw ex;
                    }
                }

            }


        }

        [HttpPost]
        public JsonResult getParticulars(string query = "")
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                long companyid = Convert.ToInt64(Session["companyid"]);
                string Branchid = Convert.ToString(Session["BranchId"]);
                var particular = db.LedgerMasters.Where(p => p.ledgerName.StartsWith(query) && p.CompanyId == companyid && p.IsDeleted == null) //&& p.BranchCode == Branchid)
                                       .Select(p => new
                                       {
                                           Particular = p.ledgerName,
                                           Code = p.ledgerID,
                                           Id = p.LID,
                                       }).ToList();
                return Json(particular, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult chkParticularDetails(string query = "")
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                long companyid = Convert.ToInt64(Session["companyid"]);
                string Branchid = Convert.ToString(Session["BranchId"]);
                var particularDetails = db.LedgerMasters.Where(p => p.ledgerName == query && p.CompanyId == companyid && p.IsDeleted == null) //&& p.BranchCode == Branchid)
                                       .Select(p => new
                                       {
                                           Particular = p.ledgerName,
                                           Code = p.ledgerID,
                                           Id = p.LID,

                                       }).FirstOrDefault();
                return Json(particularDetails, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult CreateJournalDetail()
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            using (InventoryEntities db = new InventoryEntities())
            {
                JournalModelView model = new JournalModelView();
                model.JournalDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));

                var branchList = db.BranchMasters.Where(d => d.CompanyId == companyid && d.IsDeleted == false).ToList();
                ViewBag.branchList = branchList;
                ViewBag.BranchId = Branchid;
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult CreateJournalDetail(JournalModelView journalModelView, FormCollection collection)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            int fid = Convert.ToInt32(Session["fid"]);
            string createdby = Convert.ToString(Session["Createdid"]);
            int Createby = Convert.ToInt32(Session["Createdid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();

            using (InventoryEntities db = new InventoryEntities())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {

                        Journal journal = new Journal();

                        var date = DateTime.ParseExact(journalModelView.JournalDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                        var getDateRange = db.FinancialYearMasters.Where(d => d.fYearID == fid).FirstOrDefault();
                        var branchList = db.BranchMasters.Where(d => d.CompanyId == companyid && d.IsDeleted == false).ToList();
                        ViewBag.branchList = branchList;
                        ViewBag.BranchId = Branchid;
                        //if (getDateRange.fYearID == 1 || getDateRange.fYearID == 2)
                        //{
                        //    ViewBag.Error = "This Financial Year is out of scope of entry.";
                        //    return RedirectToAction("ShowAllJournal", new { Err = "This Financial Year is out of scope of entry. " });


                        //}
                        if (!(date >= getDateRange.sDate && date <= getDateRange.eDate))
                        {
                            return RedirectToAction("ShowAllJournal", new { Err = "Voucher Date out of scope of " + getDateRange.Year + " Financial Year." });

                        }
                        var checkMenu = db.MenuaccessUsers.Any(d => d.AssignedUserId == Createby && d.Name == "SpecialPriviledge");
                        //var getVoucherLock = db.LockVouchers.Where(d => d.FinancialYear == fid && d.Month == date.Month && d.BranchId == Branchid).FirstOrDefault();
                        //if (getVoucherLock != null)
                        //{
                        //    if (getVoucherLock.Journal == true)
                        //    {

                        //        return RedirectToAction("ShowAllJournal", new { Err = "The entry and update of Journal Voucher for the month of " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month) + " has been locked. Contact Head Office." });

                        //    }
                        //}

                        int countpo = db.Journals.Where(p => p.CompanyId == companyid && p.BranchId == Branchid).Count();
                        journal.JournalCode = tc.GenerateCode("JOR", countpo);
                        // journal.BranchId = Branchid;
                        if (journalModelView.BId == null)
                            journal.BranchId = Branchid;
                        else
                            journal.BranchId = journalModelView.BId;
                        journal.CompanyId = companyid;
                        journal.UserId = userid;
                        journal.CreatedBy = createdby;
                        journal.CreatedOn = DateTime.Now;
                        journal.JournalDate = DateTime.ParseExact(journalModelView.JournalDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                        journal.Narration = journalModelView.Narration;
                        journal.FinancialYearId = fid;
                        db.Journals.Add(journal);
                        db.SaveChanges();
                        //   scope.Complete();
                        string[] part = collection["txtparticular"].Split(',');
                        string[] dbt = collection["txtDebit"].Split(',');
                        string[] crt = collection["txtCredit"].Split(',');
                        string[] narr = collection["txtnarrative"].Split(',');
                        string[] lid = collection["txtLID"].Split(',');


                        int i = 0;

                        foreach (var prod in part)
                        {
                            JournalDetail model = new JournalDetail();
                            model.JournalId = Convert.ToInt64(journal.Id);
                            model.LID = Convert.ToInt64(lid[i]);
                            model.Particulars = Convert.ToString(part[i]);
                            model.Narrative = Convert.ToString(narr[i]);
                            if (crt[i] == "")
                                model.Credit = 0;
                            else
                                model.Credit = Convert.ToDecimal(crt[i]);
                            if (dbt[i] == "")
                                model.Debit = 0;
                            else
                                model.Debit = Convert.ToDecimal(dbt[i]);
                            db.JournalDetails.Add(model);

                            i++;
                        }
                        db.SaveChanges();
                        scope.Complete();

                        return RedirectToAction("ShowAllJournal", new { Msg = "Items has been saved successfully..." });
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        // throw ex;
                        return RedirectToAction("ShowAllJournal", new { Err = "Failed to save the Journal..." });
                    }
                }
            }
        }

        [HttpGet]
        public ActionResult ShowAllJournal(string Msg, string Err)
        {
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
                int userid = Convert.ToInt32(Session["userid"]);

                int companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                List<JournalModelView> journal = new List<JournalModelView>();
                List<InvoiceTotal> journaldetail = new List<InvoiceTotal>();
                var branchList = new List<Taxname>();
                ViewBag.BranchId = Branchid;
                //var branch = new Taxname();
                //branch.Id = 0;
                //branch.Name = "Head Office";
                //branchList.Add(branch);
                var branches = db.BranchMasters.Select(d => new Taxname { Id = d.Id, Name = d.Name });
                branchList.AddRange(branches);
                if (Branchid == 0)
                {
                    //  journal = db.Journals.Where(d => d.UserId == userid && d.CompanyId == companyid && d.IsDeleted == null && d.FinancialYearId == fid).OrderByDescending(d => d.Id).ToList();
                    journal = (from j in db.Journals.Where(d => d.CompanyId == companyid && d.FinancialYearId == fid && d.IsDeleted == null).OrderByDescending(o => o.JournalDate).OrderByDescending(d => d.Id).Take(200).ToList()
                               join jd in db.JournalDetails.ToList() on j.Id equals jd.JournalId
                               join b in branchList on j.BranchId equals b.Id
                               group jd by new { jd.JournalId, j.JournalCode, j.Narration, j.JournalDate, j.BranchId, b.Name } into g
                               select new JournalModelView() { Id = g.Key.JournalId, TotalDebit = g.Sum(t3 => t3.Debit), TotalCredit = g.Sum(t3 => t3.Credit), Narration = g.Key.Narration, JournalCode = g.Key.JournalCode, CreatedOn = g.Key.JournalDate, JournalName = g.Key.Name }).ToList();
                }
                else
                {
                    journal = (from j in db.Journals.Where(d => d.CompanyId == companyid && d.BranchId == Branchid && d.FinancialYearId == fid && d.IsDeleted == null).OrderByDescending(o => o.JournalDate).OrderByDescending(d => d.Id).Take(200).ToList()
                               join jd in db.JournalDetails.ToList() on j.Id equals jd.JournalId
                               join b in branchList on j.BranchId equals b.Id
                               group jd by new { jd.JournalId, j.JournalCode, j.Narration, j.JournalDate, j.BranchId, b.Name } into g
                               select new JournalModelView() { Id = g.Key.JournalId, TotalDebit = g.Sum(t3 => t3.Debit), TotalCredit = g.Sum(t3 => t3.Credit), Narration = g.Key.Narration, JournalCode = g.Key.JournalCode, CreatedOn = g.Key.JournalDate, JournalName = g.Key.Name }).ToList();
                }

                return View(journal.OrderBy(d => d.CreatedOn).ThenBy(d => d.Id).ToList());
            }

        }

        [HttpGet]
        public ActionResult ShowAllJournalSearch(string DateFrom, string DateTo)
        {
            var culture = "es-AR";
            string dateFormat = "dd/MM/yyyy";
            var DtFrm = DateTime.ParseExact(DateFrom, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var DtTo = DateTime.ParseExact(DateTo, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            using (InventoryEntities db = new InventoryEntities())
            {
                int userid = Convert.ToInt32(Session["userid"]);
                int companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                int fid = Convert.ToInt32(Session["fid"]);

                List<JournalModelView> journal = new List<JournalModelView>();
                List<InvoiceTotal> journaldetail = new List<InvoiceTotal>();
                var branchList = new List<Taxname>();
                //var branch = new Taxname();
                //branch.Id = 0;
                //branch.Name = "Head Office";
                //branchList.Add(branch);
                ViewBag.BranchId = Branchid;
                var branches = db.BranchMasters.Select(d => new Taxname { Id = d.Id, Name = d.Name });
                branchList.AddRange(branches);
                if (Branchid == 0)
                {
                    //  journal = db.Journals.Where(d => d.UserId == userid && d.CompanyId == companyid && d.IsDeleted == null && d.FinancialYearId == fid).OrderByDescending(d => d.Id).ToList();
                    journal = (from j in db.Journals.Where(d => d.CompanyId == companyid && d.FinancialYearId == fid && d.IsDeleted == null && d.JournalDate >= DtFrm && d.JournalDate <= DtTo).ToList()
                               join jd in db.JournalDetails.ToList() on j.Id equals jd.JournalId
                               join b in branchList on j.BranchId equals b.Id
                               group jd by new { jd.JournalId, j.JournalCode, j.Narration, j.JournalDate, j.BranchId, b.Name } into g
                               select new JournalModelView() { Id = g.Key.JournalId, TotalDebit = g.Sum(t3 => t3.Debit), TotalCredit = g.Sum(t3 => t3.Credit), Narration = g.Key.Narration, JournalCode = g.Key.JournalCode, CreatedOn = g.Key.JournalDate, JournalName = g.Key.Name }).OrderBy(d => d.CreatedOn).OrderBy(d => d.Id).ToList();
                }
                else
                {
                    journal = (from j in db.Journals.Where(d => d.CompanyId == companyid && d.BranchId == Branchid && d.FinancialYearId == fid && d.IsDeleted == null && d.JournalDate >= DtFrm && d.JournalDate <= DtTo).ToList()
                               join jd in db.JournalDetails.ToList() on j.Id equals jd.JournalId
                               join b in branchList on j.BranchId equals b.Id
                               group jd by new { jd.JournalId, j.JournalCode, j.Narration, j.JournalDate, j.BranchId, b.Name } into g
                               select new JournalModelView() { Id = g.Key.JournalId, TotalDebit = g.Sum(t3 => t3.Debit), TotalCredit = g.Sum(t3 => t3.Credit), Narration = g.Key.Narration, JournalCode = g.Key.JournalCode, CreatedOn = g.Key.JournalDate, JournalName = g.Key.Name }).OrderBy(d => d.CreatedOn).OrderBy(d => d.Id).ToList();
                }
                //List<JournalModelView> model = new List<JournalModelView>();
                //foreach (var item in journal)
                //{
                //    model.Add(new JournalModelView()
                //    {
                //        Narration = item.Narration,
                //        JournalCode = item.JournalCode,
                //        JournalDate = item.CreatedOn.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture)),
                //        Id = item.Id,
                //        TotalDebit=item
                //    });
                //}
                return View(journal);
            }

        }
        [HttpGet]
        public ActionResult EditJournal(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int userid = Convert.ToInt32(Session["userid"]);
                int companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                List<JournalDetailModelView> jdlist = new List<JournalDetailModelView>();
                JournalModelView model = new JournalModelView();
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var ledgermaster = db.LedgerMasters.Where(l => l.CompanyId == companyid).ToList();
                var branchList = db.BranchMasters.Where(d => d.CompanyId == companyid && d.IsDeleted == false).ToList();
                ViewBag.branchList = branchList;
                ViewBag.BranchId = Branchid;

                var journal = db.Journals.Where(d => d.Id == id && d.UserId == userid && d.CompanyId == companyid && d.IsDeleted == null).FirstOrDefault();
                var journaldetail = db.JournalDetails.Where(d => d.JournalId == id).ToList();
                model.Id = journal.Id;
                model.BId = journal.BranchId;
                model.Narration = journal.Narration;
                model.JournalCode = journal.JournalCode;
                model.JournalDate = journal.JournalDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                decimal? totaldebit = 0;
                decimal? totalcredit = 0;
                foreach (var item in journaldetail)
                {
                    var jd = new JournalDetailModelView();
                    jd.JournalId = item.JournalId;
                    jd.Particulars = item.Particulars;
                    jd.Narrative = item.Narrative;
                    jd.LedgerId = ledgermaster.FirstOrDefault(l => l.LID == item.LID).ledgerID;
                    jd.Debit = item.Debit;
                    jd.Credit = item.Credit;
                    jd.LID = item.LID;

                    jdlist.Add(jd);
                    totaldebit += jd.Debit;
                    totalcredit += jd.Credit;
                }
                model.TotalDebit = totaldebit;
                model.TotalCredit = totalcredit;
                ViewBag.JournalDetails = jdlist;
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult EditJournal(JournalModelView journalModelView, FormCollection collection)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            int fid = Convert.ToInt32(Session["fid"]);
            string createdby = Convert.ToString(Session["Createdid"]);
            int Createby = Convert.ToInt32(Session["Createdid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();

            using (InventoryEntities db = new InventoryEntities())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        var date = DateTime.ParseExact(journalModelView.JournalDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                        var getDateRange = db.FinancialYearMasters.Where(d => d.fYearID == fid).FirstOrDefault();
                        //if (getDateRange.fYearID == 1 || getDateRange.fYearID == 2)
                        //{
                        //    ViewBag.Error = "This Financial Year is out of scope of entry.";
                        //    return RedirectToAction("ShowAllJournal", new { Err = "This Financial Year is out of scope of entry. " });


                        //}
                        if (!(date >= getDateRange.sDate && date <= getDateRange.eDate))
                        {
                            return RedirectToAction("ShowAllJournal", new { Err = "Voucher Date out of scope of " + getDateRange.Year + " Financial Year." });

                        }
                        var checkMenu = db.MenuaccessUsers.Any(d => d.AssignedUserId == Createby && d.Name == "SpecialPriviledge");
                        //var getVoucherLock = db.LockVouchers.Where(d => d.FinancialYear == fid && d.Month == date.Month && d.BranchId == Branchid).FirstOrDefault();
                        //if (getVoucherLock != null)
                        //{
                        //    if (getVoucherLock.Journal == true)
                        //    {

                        //        return RedirectToAction("ShowAllJournal", new { Err = "The entry and update of Journal Voucher for the month of " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month) + " has been locked. Contact Head Office." });

                        //    }
                        //}
                        var branchList = db.BranchMasters.Where(d => d.CompanyId == companyid && d.IsDeleted == false).ToList();
                        ViewBag.branchList = branchList;
                        ViewBag.BranchId = Branchid;
                        long? assignedBranch = 0;
                        if (journalModelView.BId == null)
                            assignedBranch = Branchid;
                        else
                            assignedBranch = journalModelView.BId;


                        Journal journal = db.Journals.Find(journalModelView.Id);
                        var journaldetail = db.JournalDetails.Where(d => d.JournalId == journal.Id).ToList();
                        //   int countpo = db.Journals.Where(p => p.CompanyId == companyid && p.BranchId == Branchid).Count();

                        if ((journal.FinancialYearId == fid) && (journal.BranchId != assignedBranch))
                        {
                            int countpo = db.Journals.Where(p => p.CompanyId == companyid && p.BranchId == Branchid).Count();
                            journal.JournalCode = tc.GenerateCode("JOR", countpo);
                        }
                        //journal.JournalCode = tc.GenerateCode("JOR", countpo);
                        //journal.BranchId = Branchid;
                        //journal.CompanyId = companyid;
                        //journal.UserId = userid;
                        //journal.CreatedBy = createdby;
                        //journal.CreatedOn = DateTime.Now;
                        journal.JournalDate = DateTime.ParseExact(journalModelView.JournalDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                        journal.Narration = journalModelView.Narration;
                        if (journalModelView.BId == null)
                            journal.BranchId = Branchid;
                        else
                            journal.BranchId = journalModelView.BId;
                        //journal.FinancialYearId = fid;
                        //db.Journals.Add(journal);
                        //db.SaveChanges();
                        //   scope.Complete();
                        foreach (var journalremoveitem in journaldetail)
                        {
                            db.JournalDetails.Remove(journalremoveitem);
                        }
                        string[] part = collection["txtparticular"].Split(',');
                        string[] dbt = collection["txtDebit"].Split(',');
                        string[] crt = collection["txtCredit"].Split(',');
                        string[] narr = collection["txtnarrative"].Split(',');
                        string[] lid = collection["txtLID"].Split(',');


                        int i = 0;

                        foreach (var prod in part)
                        {
                            JournalDetail model = new JournalDetail();
                            model.JournalId = Convert.ToInt64(journal.Id);
                            model.LID = Convert.ToInt64(lid[i]);
                            model.Particulars = Convert.ToString(part[i]);
                            model.Narrative = Convert.ToString(narr[i]);
                            if (crt[i] == "")
                                model.Credit = 0;
                            else
                                model.Credit = Convert.ToDecimal(crt[i]);
                            if (dbt[i] == "")
                                model.Debit = 0;
                            else
                                model.Debit = Convert.ToDecimal(dbt[i]);
                            db.JournalDetails.Add(model);

                            i++;
                        }
                        db.SaveChanges();
                        scope.Complete();

                        return RedirectToAction("ShowAllJournal", new { Msg = "Items has been saved successfully..." });
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        //  throw ex;
                        return RedirectToAction("ShowAllJournal", new { Err = "Failed to save the Journal..." });
                    }
                }
            }
        }


        public ActionResult DeleteJournal(long id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                try
                {

                    int userid = Convert.ToInt32(Session["userid"]);
                    var result = db.Journals.Find(id);
                    var journalDetail = db.JournalDetails.Where(d => d.JournalId == id);
                    foreach (var jd in journalDetail)
                    {
                        db.JournalDetails.Remove(jd);
                    }
                    db.Journals.Remove(result);
                    db.SaveChanges();
                    return RedirectToAction("ShowAllJournal", new { Msg = "Journal deleted Successfully...." });
                }
                catch
                {


                    return RedirectToAction("ShowAllJournal", new { Err = InventoryMessage.Delte });
                }
            }
        }




        private List<JournalDetailModelView> getJournalDetails(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                List<JournalDetailModelView> journalModel = new List<JournalDetailModelView>();
                int userid = Convert.ToInt32(Session["userid"]);
                int companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                var journalDetail = db.JournalDetails.Where(r => r.JournalId == id).ToList();
                foreach (var item in journalDetail)
                {
                    journalModel.Add(new JournalDetailModelView
                    {
                        Particulars = item.Particulars,
                        Narrative = item.Narrative,
                        //LedgerId = item.le
                        Debit = item.Debit,
                        Credit = item.Credit,
                    });
                }
                return journalModel;
            }
        }
    }
}

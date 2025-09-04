using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;


namespace XenERP.Controllers
{
    
    public class CashTransferController : Controller
    {
        private InventoryEntities db = new InventoryEntities();

        //
        // GET: /CashTransfer/

        public ActionResult Index()
        {
            int Fyid = Convert.ToInt32(Session["fid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            //if (Branchid == 0)
            //{
                var cashtransfers = db.CashTransfers.Include(c => c.BranchMaster).Include(c => c.BranchMaster1).Where(d=>d.FinancialYearId==Fyid && (d.FromUnit==Branchid || d.ToUnit==Branchid));
                return View(cashtransfers.ToList());
            //}
            //else
            //{
            //    var cashtransfers = db.CashTransfers.Include(c => c.BranchMaster).Include(c => c.BranchMaster1).Where(d=>d.BranchId==Branchid);
            //    return View(cashtransfers.ToList());
            //}
          
        }

        //
        // GET: /CashTransfer/Details/5

        public ActionResult Details(long id = 0)
        {
            CashTransfer cashtransfer = db.CashTransfers.Find(id);
            if (cashtransfer == null)
            {
                return HttpNotFound();
            }
            return View(cashtransfer);
        }

        //
        // GET: /CashTransfer/Create

        public ActionResult Create()
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            
           
           
            ViewBag.ToUnit = db.BranchMasters.Where(d => d.IsDeleted == false && d.Id != Branchid);
            //if (Branchid == 0)
            //{
            ViewBag.FromUnit = db.BranchMasters.Where(d => d.Id == Branchid);
            //}
            //else
            //{
            //    ViewBag.FromUnit = new SelectList(db.BranchMasters.Where(d=>Branchid==1), "Id", "Name");
            //}
            return View();
        }

        //
        // POST: /CashTransfer/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CashTransfer cashtransfer)
        {
            long companyid = Convert.ToInt64(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            long Createdby = Convert.ToInt32(Session["Createdid"]);
            var dateFormat = Session["DateFormat"].ToString();
            var culture = Session["DateCulture"].ToString();
            var date = DateTime.ParseExact(cashtransfer.DateString, dateFormat, System.Globalization.CultureInfo.CreateSpecificCulture(culture));
            var getDateRange = db.FinancialYearMasters.Where(d => d.fYearID == Fyid).FirstOrDefault();
            if (getDateRange.fYearID == 1 || getDateRange.fYearID == 2 || getDateRange.fYearID == 3 || getDateRange.fYearID == 4)
            {
                ViewBag.Error = "This Financial Year is out of scope of entry.";
                return RedirectToAction("Index", new { Err = "This Financial Year is out of scope of entry. " });

            }
            if (!(date >= getDateRange.sDate && date <= getDateRange.eDate))
            {
                return RedirectToAction("Index", new { Err = "Cash Transfer Date out of scope of " + getDateRange.Year + " Financial Year." });

            }

            var fyear = db.FinancialYearMasters.FirstOrDefault(f => f.fYearID == Fyid).Year;
            var fs = fyear.Substring(2, 2);
            var es = fyear.Substring(7, 2);
            fyear = fs + "-" + es;
            int countpo = 1;

            //&& p.BranchId == Branchid
            if (db.CashTransfers.Where(p =>  p.FinancialYearId == Fyid).Count() != 0)
            {
                countpo = (int)db.CashTransfers.Where(p => p.FinancialYearId == Fyid).Max(p => p.Serial) + 1;
            }
            //var getPrefix = db.Prefixes.Where(p => p.DefaultPrefix == "PI" && p.CompanyId == companyid).Select(p => new { p.DefaultPrefix, p.SetPrefix }).FirstOrDefault();

            //if (getPrefix.SetPrefix != null)
            //    pomv.NO = getPrefix.SetPrefix + "/" + fyear + "/" + countpo;//tc.GenerateCode(fyear, countpo);
            //else
            cashtransfer.NO = "CT" + "/" + fyear + "/" + countpo; //tc.GenerateCode(fyear, countpo);
            cashtransfer.Serial = countpo;
            cashtransfer.CreatedBy =(int) Createdby;
            cashtransfer.CreatedOn = DateTime.Today;
            cashtransfer.BranchId =(int) Branchid;
            cashtransfer.CashLedger = 35;
            cashtransfer.FinancialYearId = Fyid;
            cashtransfer.Date = date;
            cashtransfer.CashTransferLedger = 197;
            cashtransfer.Approved = false;
            if (ModelState.IsValid)
            {
                db.CashTransfers.Add(cashtransfer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ToUnit = db.BranchMasters.Where(d => d.IsDeleted  == false && d.Id != Branchid);
            //if (Branchid == 0)
            //{
            ViewBag.FromUnit = db.BranchMasters.Where(d => d.Id == Branchid);
            //}
            //else
            //{
            //    ViewBag.FromUnit = new SelectList(db.BranchMasters.Where(d => Branchid == 1), "Id", "Name");
            //}
            return View(cashtransfer);
        }

        //
        // GET: /CashTransfer/Edit/5

        public ActionResult Edit(long id = 0)
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            
            CashTransfer cashtransfer = db.CashTransfers.Find(id);
            if (cashtransfer == null)
            {
                return HttpNotFound();
            }
            

            ViewBag.From = db.BranchMasters.Where(d => d.Id == cashtransfer.FromUnit);
            ViewBag.To = db.BranchMasters.Where(d => d.IsDeleted == false && d.Id != cashtransfer.FromUnit);
            ViewBag.Branch = Branchid;
            return View(cashtransfer);
        }

        //
        // POST: /CashTransfer/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CashTransfer cashtransfer)
        {
            int Fyid = Convert.ToInt32(Session["fid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long Createdby = Convert.ToInt32(Session["Createdid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            var date = DateTime.ParseExact(cashtransfer.DateString, dateFormat, System.Globalization.CultureInfo.CreateSpecificCulture(culture));
            var getDateRange = db.FinancialYearMasters.Where(d => d.fYearID == Fyid).FirstOrDefault();
            if (getDateRange.fYearID == 1 || getDateRange.fYearID == 2 || getDateRange.fYearID == 3 || getDateRange.fYearID == 4)
            {
                ViewBag.Error = "This Financial Year is out of scope of entry.";
                return RedirectToAction("Index", new { Err = "This Financial Year is out of scope of entry. " });

            }
            if (!(date >= getDateRange.sDate && date <= getDateRange.eDate))
            {
                return RedirectToAction("Index", new { Err = "Cash Transfer Date out of scope of " + getDateRange.Year + " Financial Year." });

            }
            if (cashtransfer.Approved == true)
            {
                db.Entry(cashtransfer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else if (cashtransfer.ToUnit == Branchid && cashtransfer.Approved == false)
            {
                cashtransfer.Approved = true;
            }
            cashtransfer.Date = date;
            cashtransfer.ModifiedBy =(int) Createdby;
            cashtransfer.ModifiedOn = DateTime.Now;
            
            if (ModelState.IsValid)
            {
                db.Entry(cashtransfer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.FromUnit = db.BranchMasters.Where(d => d.Id == cashtransfer.FromUnit);
            ViewBag.ToUnit = db.BranchMasters.Where(d => d.IsDeleted == false && d.Id != cashtransfer.FromUnit);
            return View(cashtransfer);
        }

        //
        // GET: /CashTransfer/Delete/5

        public ActionResult Delete(long id = 0)
        {
            CashTransfer cashtransfer = db.CashTransfers.Find(id);
            if (cashtransfer == null)
            {
                return HttpNotFound();
            }
            return View(cashtransfer);
        }

        //
        // POST: /CashTransfer/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            CashTransfer cashtransfer = db.CashTransfers.Find(id);
            db.CashTransfers.Remove(cashtransfer);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
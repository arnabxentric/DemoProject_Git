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
    public class CostController : Controller
    {
        private InventoryEntities db = new InventoryEntities();

        //
        // GET: /Cost/

        public ActionResult Index()
        {
            var costings = db.Costings.Include(c => c.LedgerMaster);
            return View(costings.ToList());
        }

        //
        // GET: /Cost/Details/5

        public ActionResult Details(long id = 0)
        {
            Costing costing = db.Costings.Find(id);
            if (costing == null)
            {
                return HttpNotFound();
            }
            return View(costing);
        }

        //
        // GET: /Cost/Create

        public ActionResult Create()
        {
            ViewBag.LId = new SelectList(db.LedgerMasters, "LID", "ledgerID");
            return View();
        }

        //
        // POST: /Cost/Create

        [HttpPost]
        public ActionResult Create(Costing costing)
        {
            if (ModelState.IsValid)
            {
                db.Costings.Add(costing);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.LId = new SelectList(db.LedgerMasters, "LID", "ledgerID", costing.LId);
            return View(costing);
        }

        //
        // GET: /Cost/Edit/5

        public ActionResult Edit(long id = 0)
        {
            Costing costing = db.Costings.Find(id);
            if (costing == null)
            {
                return HttpNotFound();
            }
            ViewBag.LId = new SelectList(db.LedgerMasters, "LID", "ledgerID", costing.LId);
            return View(costing);
        }

        //
        // POST: /Cost/Edit/5

        [HttpPost]
        public ActionResult Edit(Costing costing)
        {
            if (ModelState.IsValid)
            {
                db.Entry(costing).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.LId = new SelectList(db.LedgerMasters, "LID", "ledgerID", costing.LId);
            return View(costing);
        }

        //
        // GET: /Cost/Delete/5

        public ActionResult Delete(long id = 0)
        {
            Costing costing = db.Costings.Find(id);
            if (costing == null)
            {
                return HttpNotFound();
            }
            return View(costing);
        }

        //
        // POST: /Cost/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            Costing costing = db.Costings.Find(id);
            db.Costings.Remove(costing);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public JsonResult getAddPurchaseCosts()
        {
            //var username = User.Identity.Name;
            //var userinf = db.Users.Where(u => u.UserName == username).FirstOrDefault();
            // var products = db.Products.Where(p => p.Userid == userinf.UserId && p.companyid == userinf.CompanyId).ToList(); 
            var costs = db.Costings.Where(p=>p.TransactionType=="Purchase" && p.Type=="ADD").Select(p => new { Id = p.Id, Name = p.Name }).ToList();
            return Json(costs, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getDeductPurchaseCosts()
        {
            var costs = db.Costings.Where(p => p.TransactionType == "Purchase" && p.Type == "DEDUCT").Select(p => new { Id = p.Id, Name = p.Name }).ToList();
            return Json(costs, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getAddSalesCosts()
        {
           
            var costs = db.Costings.Where(p => p.TransactionType == "Sales" && p.Type == "ADD").Select(p => new { Id = p.Id, Name = p.Name }).ToList();
            return Json(costs, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getDeductSalesCosts()
        {
            var costs = db.Costings.Where(p => p.TransactionType == "Sales" && p.Type == "DEDUCT").Select(p => new { Id = p.Id, Name = p.Name }).ToList();
            return Json(costs, JsonRequestBehavior.AllowGet);
        }
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
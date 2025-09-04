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
    public class ShiftController : Controller
    {
        private InventoryEntities db = new InventoryEntities();

        //
        // GET: /Shift/

        public ActionResult Index()
        {
            return View(db.Shifts.ToList());
        }

        //
        // GET: /Shift/Details/5

        public ActionResult Details(int id = 0)
        {
            Shift shift = db.Shifts.Find(id);
            if (shift == null)
            {
                return HttpNotFound();
            }
            return View(shift);
        }

        //
        // GET: /Shift/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Shift/Create

        [HttpPost]
        public ActionResult Create(Shift shift)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);
           
            int userid = Convert.ToInt32(Session["userid"]);
           
            if (ModelState.IsValid)
            {
                
                shift.UserId = userid;
                shift.CompanyId = companyid;
                shift.BranchId = Branchid;
                shift.CreatedOn = DateTime.Now;
                db.Shifts.Add(shift);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(shift);
        }

        //
        // GET: /Shift/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Shift shift = db.Shifts.Find(id);
            if (shift == null)
            {
                return HttpNotFound();
            }
            return View(shift);
        }

        //
        // POST: /Shift/Edit/5

        [HttpPost]
        public ActionResult Edit(Shift shift)
        {
            if (ModelState.IsValid)
            {
                shift.ModifiedOn = DateTime.Now;
                db.Entry(shift).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(shift);
        }

        //
        // GET: /Shift/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Shift shift = db.Shifts.Find(id);
            if (shift == null)
            {
                return HttpNotFound();
            }
            return View(shift);
        }

        //
        // POST: /Shift/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Shift shift = db.Shifts.Find(id);
            db.Shifts.Remove(shift);
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
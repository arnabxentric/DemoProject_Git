using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Transactions;

namespace XenERP.Controllers
{
    public class ShedController : Controller
    {
        //
        // GET: /Shed/

        public ActionResult Index()
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int branchId = Convert.ToInt32(Session["BranchId"]);
                var shed = db.ShedMasters.Where(d=>d.BranchId == branchId).ToList();
                return View(shed);
            }
        }

        //
        // GET: /Shed/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Shed/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Shed/Create

        [HttpPost]
        public ActionResult Create(ShedMaster model)
        {
            int userId = Convert.ToInt32(Session["userid"]);
            int branchId = Convert.ToInt32(Session["BranchId"]);
            int companyId = Convert.ToInt32(Session["companyid"]);
            using (InventoryEntities db = new InventoryEntities())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        model.CompanyId = companyId;
                        model.BranchId = branchId;
                        model.UserId = userId;
                        db.ShedMasters.Add(model);
                        db.SaveChanges();
                        scope.Complete();
                        return RedirectToAction("Index", new { Msg = "Shed Created Successfully....." });
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        return RedirectToAction("Index", new { Msg = "Shed Creation Failed....." });
                        throw ex;
                    }
                }
            }
        }

        //
        // GET: /Shed/Edit/5

        public ActionResult Edit(int id)
        {

            return View();
        }

        //
        // POST: /Shed/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Shed/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Shed/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Transactions;

namespace XenERP.Controllers
{
    public class SalesPersonController : Controller
    {
        //
        // GET: /SalesPerson/

        [HttpGet]
        public ActionResult CreateSalesPerson()
        {
            using (InventoryEntities db = new InventoryEntities())
            {

                var list = new SelectList(new[]
                                          {
                                              new {ID="Sales",Name="Sales"},
                                              new{ID="Delivery",Name="Delivery"},
                                              new{ID="Collection",Name="Collection"}
                                           
                                          },
                                "ID", "Name");
                ViewData["type"] = list;
                int userid = Convert.ToInt32(Session["userid"]);
                var company = db.Companies.Where(d => d.Userid == userid).FirstOrDefault();
                ViewBag.company = db.Companies.Where(d => d.Userid == userid).ToList();
                ViewBag.branch = db.BranchMasters.Where(d => d.UserId == userid && d.CompanyId == company.Id).ToList();
                return View();
            }
        }

        [HttpPost]
        public ActionResult CreateSalesPerson(SalesPerson model)
        {
            string Createdby = Convert.ToString(Session["Createdid"]);
            int userId = Convert.ToInt32(Session["userid"]);
            int branchId = Convert.ToInt32(Session["BranchId"]);
            int companyId = Convert.ToInt32(Session["companyid"]);
            using (InventoryEntities db = new InventoryEntities())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        model.BranchId = branchId;
                        model.UserId = userId;
                        model.CompanyId = companyId;
                        db.SalesPersons.Add(model);
                        db.SaveChanges();
                        scope.Complete();
                        return RedirectToAction("ShowAllPerson", new { Msg = "Sales Person Created Successfully....." });
                        
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        throw ex;
                    }
                }
            }
        }

        [HttpGet]
        public ActionResult ShowAllPerson(string Msg, string Err)
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
                var person = db.SalesPersons.Where(d => d.UserId == userid && d.CompanyId == companyid && d.IsDeleted == false).ToList();
                return View(person);
            }
        }

        [HttpGet]
        public ActionResult EditSalesPerson(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {

                SalesPerson model = new SalesPerson();
                var result = db.SalesPersons.Find(id);
                model.Name = result.Name;
                model.Phone = result.Phone;
                model.Email = result.Email;
                int userid = Convert.ToInt32(Session["userid"]);
                var company = db.Companies.Where(d => d.Userid == userid).FirstOrDefault();
                ViewBag.company = new SelectList(db.Companies, "Id", "Name", model.CompanyId);
                ViewBag.branch = new SelectList(db.BranchMasters, "Id", "Name", model.BranchId);
                var list = new SelectList(new[]
                                          {
                                              new {ID="Sales",Name="Sales"},
                                              new{ID="Delivery",Name="Delivery"},
                                              new{ID="Collection",Name="Collection"}
                                           
                                          },
                               "ID", "Name");
                ViewData["type"] = list;
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult EditSalesPerson(SalesPerson model)
        {
            string Createdby = Convert.ToString(Session["Createdid"]);
            int userId = Convert.ToInt32(Session["userid"]);
            int branchId = Convert.ToInt32(Session["BranchId"]);
            int companyId = Convert.ToInt32(Session["companyid"]);
            using (InventoryEntities db = new InventoryEntities())
            {
                try
                {
                    var result = db.SalesPersons.Where(d => d.Id == model.Id && d.IsDeleted == false).FirstOrDefault();
                    result.UserId = userId;
                    result.BranchId = branchId;
                    result.CompanyId = companyId;
                    result.Name = model.Name;
                    result.Task = model.Task;
                    result.Email = model.Email;
                    result.Phone = model.Phone;
                    db.SaveChanges();
                    return RedirectToAction("ShowAllPerson", new { Msg = "Data Updated Successfully...." });
                }
                catch
                {
                    return RedirectToAction("ShowAllPerson", new { Err = "Data  not saved successfully...." });
                }
            }
        }

        [HttpGet]
        public ActionResult DeletePerson(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                try
                {
                    var result = db.SalesPersons.Where(d => d.Id == id).FirstOrDefault();
                    result.IsDeleted = true;
                    db.SaveChanges();
                    return RedirectToAction("ShowAllPerson", new { Msg = "Row Deleted Successfully...." });
                }
                catch
                {
                    return RedirectToAction("ShowAllPerson", new { Err = "Row cannot be deleted...." });
                }
            }
        }

        [HttpGet]
        public ActionResult PersonDetails(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                var person = db.SalesPersons.SingleOrDefault(d => d.Id == id);
                return View(person);
            }
        }
    }
}

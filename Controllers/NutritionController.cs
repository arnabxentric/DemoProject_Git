using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Transactions;

namespace XenERP.Controllers
{
     [SessionExpire]
    public class NutritionController : Controller
    {
        //
        // GET: /Nutrition/

        public ActionResult CreateNutrition()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateNutrition(Nutrition model)
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
                        model.NutriSpecification = model.NutriSpecification;
                        model.CompanyId = companyId;
                        model.BranchId = branchId;
                        model.UserId = userId;
                        model.CreatedOn = DateTime.Now;
                        model.CreatedBy = userId;
                        db.Nutritions.Add(model);
                        db.SaveChanges();
                        scope.Complete();
                        return RedirectToAction("ShowAllNutritionList", new { Msg = "Nutrition Created Successfully....." });
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        return RedirectToAction("ShowAllNutritionList", new { Msg = "Nutrition Creation Failes....." });
                        throw ex;
                    }
                }
            }

        }

        [HttpGet]
        public ActionResult ShowAllNutritionList(string Msg, string Err)
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
                var category = db.Nutritions.Where(d => d.UserId == userid && d.CompanyId == companyid && d.IsDeleted == false).ToList();
                return View(category);
            }
        }

        [HttpGet]
        public ActionResult EditNutrition(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                
                var result = db.Nutritions.Find(id);
                return View(result);
            }
        }

        [HttpPost]
        public ActionResult EditNutrition(Nutrition model)
        {
            string Createdby = Convert.ToString(Session["Createdid"]);
            int userId = Convert.ToInt32(Session["userid"]);
            int branchId = Convert.ToInt32(Session["BranchId"]);
            int companyId = Convert.ToInt32(Session["companyid"]);
            using (InventoryEntities db = new InventoryEntities())
            {
                try
                {
                    var result = db.Nutritions.Where(d => d.Id == model.Id && d.IsDeleted == false).FirstOrDefault();
                    model.CompanyId = companyId;
                    model.UserId = userId;
                    model.BranchId = branchId;
                    model.ModifiedBy = userId;
                    model.ModifiedOn = DateTime.Now;
                    db.Entry(result).CurrentValues.SetValues(model);

                    db.SaveChanges();

                    return RedirectToAction("ShowAllNutritionList", new { Msg = "Data Updated Successfully...." });
                }
                catch
                {
                    return RedirectToAction("ShowAllNutritionList", new { Err = "Data  not saved successfully...." });
                }
            }
        }

        [HttpGet]
        public ActionResult NutritionDetails(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                var category = db.Nutritions.SingleOrDefault(d => d.Id == id);
                return View(category);
            }
        }

        public ActionResult DeleteNutrition(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                try
                {
                    var result = db.Nutritions.Where(d => d.Id == id).FirstOrDefault();
                    result.IsDeleted = true;
                    db.SaveChanges();
                    return RedirectToAction("ShowAllNutritionList", new { Msg = "Row Deleted Successfully...." });
                }
                catch
                {
                    return RedirectToAction("ShowAllNutritionList", new { Err = "Row cannot be deleted...." });
                }
            }
        }

    }
}

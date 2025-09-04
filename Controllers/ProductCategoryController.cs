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
    public class ProductCategoryController : Controller
    {
        //
        // GET: /ProductCategory/

        public ActionResult CreateProductCategory()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateProductCategory(ProductCategory_MSTR model)
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
                        model.CategoryName = model.CategoryName;
                        model.CompanyId = companyId;
                        model.BranchId = branchId;
                        model.UserId = userId;
                        model.CreatedOn = DateTime.Now;
                        model.CreatedBy = userId;
                        db.ProductCategory_MSTR.Add(model);
                        db.SaveChanges();
                        scope.Complete();
                        return RedirectToAction("ShowAllProductCategory", new { Msg = "Product Category Created Successfully....." });
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        return RedirectToAction("ShowAllProductCategory", new { Msg = "Product Category Creation Failes....." });
                        throw ex;
                    }
                }
            }

        }

        [HttpGet]
        public ActionResult ShowAllProductCategory(string Msg, string Err)
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
                var category = db.ProductCategory_MSTR.Where(d => d.UserId == userid && d.CompanyId == companyid && d.IsDeleted == false).ToList();
                return View(category);
            }
        }

        [HttpGet]
        public ActionResult EditProductCategory(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                ProductCategory_MSTR model = new ProductCategory_MSTR();
                var result = db.ProductCategory_MSTR.Find(id);
                return View(result);
            }
        }

        [HttpPost]
        public ActionResult EditProductCategory(ProductCategory_MSTR model)
        {
            string Createdby = Convert.ToString(Session["Createdid"]);
            int userId = Convert.ToInt32(Session["userid"]);
            int branchId = Convert.ToInt32(Session["BranchId"]);
            int companyId = Convert.ToInt32(Session["companyid"]);
            using (InventoryEntities db = new InventoryEntities())
            {
                try
                {
                    var result = db.ProductCategory_MSTR.Where(d => d.Id == model.Id && d.IsDeleted == false).FirstOrDefault();
                    model.CompanyId = companyId;
                    model.UserId = userId;
                    model.BranchId = branchId;
                    model.ModifiedBy = userId;
                    model.ModifiedOn = DateTime.Now;
                    db.Entry(result).CurrentValues.SetValues(model);
                 
                    db.SaveChanges();

                    return RedirectToAction("ShowAllProductCategory", new { Msg = "Data Updated Successfully...." });
                }
                catch
                {
                    return RedirectToAction("ShowAllProductCategory", new { Err = "Data  not saved successfully...." });
                }
            }
        }

        [HttpGet]
        public ActionResult ProductCategoryDetails(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                var category = db.ProductCategory_MSTR.SingleOrDefault(d => d.Id == id);
                return View(category);
            }
        }

        public ActionResult DeleteCategory(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                try
                {
                    var result = db.ProductCategory_MSTR.Where(d => d.Id == id).FirstOrDefault();
                    result.IsDeleted = true;
                    db.SaveChanges();
                    return RedirectToAction("ShowAllProductCategory", new { Msg = "Row Deleted Successfully...." });
                }
                catch
                {
                    return RedirectToAction("ShowAllProductCategory", new { Err = "Row cannot be deleted...." });
                }
            }
        }


    }
}

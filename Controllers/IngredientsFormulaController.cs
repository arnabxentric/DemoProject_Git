using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Transactions;

namespace XenERP.Controllers
{
    public class IngredientsFormulaController : Controller
    {
        //
        // GET: /IngredientsFormula/
        [HttpGet]
        public ActionResult CreateIngredientsFormula()
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                IngredientFormulaModelView model = new IngredientFormulaModelView();
                List<nutriItem> itemList = new List<nutriItem>();
                int userId = Convert.ToInt32(Session["userid"]);
                int branchId = Convert.ToInt32(Session["BranchId"]);
                int companyId = Convert.ToInt32(Session["companyid"]);
                var medicineId = db.ProductCategory_MSTR.Where(r => r.CompanyId == companyId && r.BranchId == branchId && r.CategoryName == "Medicine").Select(s => s.Id).FirstOrDefault();
                var productList = db.Products.Where(r => r.companyid == companyId && r.Branchid == branchId && r.CategoryId == medicineId).ToList();
                var nutrition = db.Nutritions.Where(r => r.CompanyId == companyId && r.BranchId == branchId).ToList();
                ViewBag.product = productList;
                foreach (var item in nutrition)
                {
                    itemList.Add(new nutriItem
                    {
                        NutritionId = item.Id,
                        NutritionName = item.NutriSpecification,
                        NutriValue = 0
                    });
                }
                model.itemList = itemList;
                return View(model);
            }
        }

        [HttpPost]

        public ActionResult CreateIngredientsFormula(IngredientFormulaModelView model)
        {
            int userId = Convert.ToInt32(Session["userid"]);
            int branchId = Convert.ToInt32(Session["BranchId"]);
            int companyId = Convert.ToInt32(Session["companyid"]);
            using (InventoryEntities db = new InventoryEntities())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    IngredientsFormula ingredientsFormula = new IngredientsFormula();
                    try
                    {
                        foreach (var item in model.itemList)
                        {

                            ingredientsFormula.ProductId = model.ProductId;
                            ingredientsFormula.NutritionId = item.NutritionId;
                            ingredientsFormula.NutriValue = item.NutriValue;
                            ingredientsFormula.CompanyId = companyId;
                            ingredientsFormula.BranchId = branchId;
                            ingredientsFormula.UserId = userId;
                            ingredientsFormula.CreatedBy = userId;
                            ingredientsFormula.CreatedOn = DateTime.Now;
                            db.IngredientsFormulas.Add(ingredientsFormula);
                            db.SaveChanges();
                        }
                        scope.Complete();
                        return RedirectToAction("CreateIngredientsFormula");
                    }
                    catch
                    {
                        scope.Dispose();
                        return RedirectToAction("CreateIngredientsFormula");
                    }



                }

            }

        }

    }
}

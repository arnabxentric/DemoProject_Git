using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Data.Entity.Validation;

namespace XenERP.Controllers
{
    public class CategoryController : Controller
    {
        private InventoryEntities db = new InventoryEntities();
        private MasterClasses mc = new MasterClasses();
        List<Category> catlist = new List<Category>();
        //
        // GET: /Category/

        public ActionResult Index()
        {
           
            return View(db.Categories.ToList());
        }

        //
        // GET: /Category/Details/5

        public ActionResult Details(int id = 0)
        {
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        //
        // GET: /Category/Create

        public ActionResult Create(string Msg, string id)
        {
           // var categories = db.Categories.ToList();
            var category = new Category();
            if (id != null)
            {
                int Id = Convert.ToInt32(id);
                category = db.Categories.Find(Id);
            }
            var categories = mc.getDdlCategories();
            ViewBag.ddlCategories = categories;
            ViewBag.Categories = db.Categories.ToList();
            ViewBag.Message = Msg;
            return View(category);
        }

        //
        // POST: /Category/Create

        [HttpPost]
        public ActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                var categories2 = mc.getDdlCategories();
                ViewBag.ddlCategories = categories2;
                ViewBag.Categories = db.Categories.ToList();
                try
                {
                    int clevel = 0;
                    if (category.GId != 0)
                    {
                        clevel = db.Categories.FirstOrDefault(c => c.Id == category.GId).Depth;
                        category.Depth = ++clevel;
                    }

                    if (category.Id == 0)
                    {
                        var checkDuplicate = db.Categories.Any(c => c.Name == category.Name);
                        if (checkDuplicate)
                        {
                            ViewBag.Message = "Category Name cannot be duplicate.";
                            return View(category);
                        }
                        else
                        {
                            db.Categories.Add(category);
                            db.SaveChanges();
                            return RedirectToAction("Create", new { Msg = "Success" });
                        }
                    }
                    else
                    {

                         var checkDuplicate = db.Categories.Where(c=>c.Id != category.Id).Any(c => c.Name == category.Name);
                         if (checkDuplicate)
                         {
                             ViewBag.Message = "Category Name cannot be duplicate.";
                             return View(category);
                             
                         }
                         else
                         {
                             var catedit = db.Categories.Find(category.Id);
                             catedit.Id = category.Id;
                             catedit.Name = category.Name;
                             catedit.GId = category.GId;
                             catedit.Depth = category.Depth;
                             db.SaveChanges();
                             return RedirectToAction("Create", new { Msg = "Success" });

                         }
                    }
                }

                catch 
                {

                    ViewBag.Message = "Insert/Update Failed";
                    return View(category);
                    
                    
                }
            }
            //var errors = ModelState.Select(x => x.Value.Errors).Where(y => y.Count > 0).ToList();
            return View(category);
        }

        //
        // GET: /Category/Edit/5

        public ActionResult Edit(int id)
        {
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            var categories = mc.getDdlCategories();
            ViewBag.ddlCategories = categories;
            ViewBag.Categories = db.Categories.ToList();
            
            return View("Create",category);
        }

        //
        // POST: /Category/Edit/5

        [HttpPost]
        public ActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        //
        // GET: /Category/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        //
        // POST: /Category/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public JsonResult getCategory(string query="")
        {
            var categories = db.Categories.Where(p => p.Name.Contains(query)).ToList();
            return Json(categories, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Test()
        {
            catlist = db.Categories.ToList();
            int gid = 0;
            //var cat = db.Categories.Where(c => c.GId == 0);
            //var a = PrepareList(cat).ToList();
            var sortedList =GetList(gid);
            var sorted = sortedList.ToList();
            string leadingspace = "\xA0\xA0\xA0\xA0\xA0";
          
            foreach (var category in sorted)
            {
                string addleadingspace = string.Empty;
                for (int i = 1; i <= category.Depth; i++)
                {
                    addleadingspace += leadingspace;
                }
                category.Name = addleadingspace + category.Name;
               
            }
            return View(sorted);
                 
        }

        //public IEnumerable<Category> PrepareList(IEnumerable<Category> list)
        //{
        //    list = list.OrderBy(x => x.SortOrder);
        //    foreach (var item in list)
        //    {
        //        yield return item;
        //        foreach (var child in PrepareList(item.Id))
        //        {
        //            yield return child;
        //        }
        //    }
        //}
        public IEnumerable<Category> GetList(int? parentID = null)
        {

            var items = catlist.Where(x => x.GId == parentID).OrderBy(x => x.SortOrder).ToList();
            foreach (var item in items)
            {

                yield return item;

                foreach (var child in GetList(item.Id))
                {

                    yield return child;
                }

            }
        }
        
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
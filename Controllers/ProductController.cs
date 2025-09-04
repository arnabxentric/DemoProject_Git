using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.IO;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data;
using System.Data.SqlClient;
using LinqToExcel.Domain;
using Remotion.Data.Linq;

namespace XenERP.Controllers
{
   
    public class ProductController : Controller
    {
     
        InventoryEntities db = new InventoryEntities();
        private MasterClasses mc = new MasterClasses();
       

        #region Purchase Product

        public ActionResult MemberTreeStructure()
        {
            return View();
        }


        public List<getTreeStructureForProduct_Result> getTree(long? MemberID)
        {
            int? memid =(int?)MemberID ?? 0;
            var data = db.getTreeStructureForProduct(memid).ToList();

            return data;
        }




        public string getRoot(long? MemberID)
        {
            
            List<Group> gList = new List<Group>();
            var listAll = db.Groups.ToList();
            var group=listAll.Where(g=>g.Id==MemberID).FirstOrDefault();
            if (group != null)
            {
                var g = new Group();
                g.Id = 0;
                g.ParentGroupId = MemberID;
                gList.Add(g);

                gList.Add(group);
            }
            while (group.ParentGroupId != null)
            {
                group = listAll.Where(g => g.Id == group.ParentGroupId).FirstOrDefault();
                gList.Add(group);
            }
           
            var ParentId = gList[gList.Count - 1].Id;
            string buildtree = newtree(MemberID, ParentId, gList);
            return buildtree;
            //return Json("", JsonRequestBehavior.AllowGet);
        }
        public string newtree(long? MemberID, long? ParentId, List<Group> gList)
        { 
            string buildtree = "";
            Group child=new Group();
            //if (MemberID != ParentId)
            //{

            //}
                var data = gList.Where(g => g.Id == ParentId).ToList();
                //if (ParentId == MemberID)
                //    child = gList.Where(g => g.Id == ParentId).FirstOrDefault();
                //else
                    child = gList.Where(g => g.ParentGroupId == ParentId).FirstOrDefault();
                if (data[0].Id != 0)
                {

                    buildtree = "<ul>";
                    foreach (var item in data)
                    {
                        buildtree = buildtree + "<li class=parent_li><span title=" + "'" + item.Id + "'" + "><i class=icon-minus-sign></i>" + item.Code + "</span>" + newtree(MemberID, child.Id, gList) + "</li>";
                    }
                    buildtree = buildtree + "</ul>";
                }
            
            return buildtree;



            //return Json("", JsonRequestBehavior.AllowGet);
        }
        public string tree(long? MemberID)
        {
            string buildtree = "";

           var data = getTree(MemberID);
            //var data = db.getTreeStructureForProduct("51").ToList();

            if (data.Count > 0)
            {

            buildtree = "<ul>";
            foreach (var item in data)
            {
                buildtree = buildtree + "<li class=parent_li><span title=" + "'" + item.Id + "'" + "><i class=icon-minus-sign></i>" + item.Code + "</span>" + tree(item.ParentGroupId) + "</li>";
            }
            buildtree = buildtree + "</ul>";
            }

            return buildtree;

          

            //return Json("", JsonRequestBehavior.AllowGet);
        }
        public string topdowntree(long? MemberID)
        {
            string buildtree = "";

            var data = getTree(MemberID);
            //var data = db.getTreeStructureForProduct("51").ToList();

            if (data.Count > 0)
            {

                buildtree = "<ul>";
                foreach (var item in data)
                {
                    buildtree = buildtree + "<li class=parent_li><span title=" + "'" + item.Id + "'" + "><i class=icon-minus-sign></i>" + item.Code + "</span>" + tree(item.ParentGroupId) + "</li>";
                }
                buildtree = buildtree + "</ul>";
            }

            return buildtree;



            //return Json("", JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetAll()
        {
            var list=db.Groups.Where(category => category.ParentGroupId == null).ToList();
            return View(list);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult CreateProduct(string Msg, string Err, string ProdId)
        {


            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            if (ProdId != null)
            {
                ViewBag.ProductId = ProdId;
            }
            else
            {
                ViewBag.Error = Err;
            }

         

            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);

            int userid = Convert.ToInt32(Session["userid"]);



            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid || d.UserId == 0 && d.CompanyId == 0).ToList();
            ViewBag.category = db.ProductCategory_MSTR.Where(d => d.UserId == userid && d.CompanyId==companyid).ToList();
            //ViewBag.group = db.Groups.Where(d => d.UserId == userid).ToList();

         

            ViewBag.group = db.ProductGroup(userid, companyid);

         
                
               // db.Taxes.Where(d => d.UserId == userid && d.CompanyId == companyid || d.UserId == 0 && d.CompanyId == 0).ToList();

            return View();
        }


        [HttpPost]
        [SessionExpire]
        public ActionResult CreateProduct(Product product, FormCollection collection)
        {

            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);

            int userid = Convert.ToInt32(Session["userid"]);

            string ProductCode = String.Empty;
            string ProductName = String.Empty;

            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid || d.UserId == 0 && d.CompanyId == 0).ToList();
           // ViewBag.category = db.Categories.Where(d => d.CompanyId == companyid && d.UserId == userid).ToList();
            ViewBag.group = db.Groups.Where(d => d.CompanyId == companyid && d.UserId == userid).ToList();

         
            var filename = string.Empty;
            string fn = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString();

            try
            {

                //string filename = Path.GetFileName(Request.Files["productimg"].FileName.ToString());

                //if (filename != "")
                //{
                //    string extension = Path.GetExtension(filename);

                //    string[] img = { ".jpeg", ".png", ".gif", ".bmp", ".jpg" };

                //    if (img.Contains(extension))
                //    {


                //        string path = Server.MapPath("~/productimg/");

                //        Request.Files["productimg"].SaveAs(Path.Combine(path, filename));



                //    }

                //    else
                //    {
                //        ViewBag.Error = "Please select  .jpeg, .png, .gif, .bmp, .jpg Images";
                //        return View();

                //    }


                //}

                foreach (string upload in Request.Files)
                {
                    if (Request.Files[upload].ContentLength == 0) continue;
                    string pathToSave = Server.MapPath("~/productimg/");
                    filename = fn + Path.GetFileName(Request.Files[upload].FileName);
                    string uploadpath = Path.Combine(pathToSave, filename);
                    FileInfo fi1 = new FileInfo(uploadpath);
                    if (fi1.Exists)
                    {
                        System.IO.File.Delete(uploadpath);
                    }
                    Request.Files[upload].SaveAs(Path.Combine(pathToSave, filename));
                }

                int Createdby = Convert.ToInt32(Session["Createdid"]);
                product.Productimage = filename;
                product.Userid = userid;
                product.Branchid = Branchid;


                if (collection["parentcode"] == "")
                {
                    ProductCode = product.Code;
                }
                else
                {
                    ProductCode = collection["parentcode"] + "-" + product.Code;

                }


                if (collection["parentname"] == "")
                {
                    ProductName = product.Name;
                }
                else
                {
                    ProductName = collection["parentname"] + " " + product.Name;

                }
                var checkduplicateCode = db.Products.Any(p => p.companyid == companyid && p.Code == ProductCode);
                if (checkduplicateCode)
                {
                    return RedirectToAction("CreateProduct", new { Err = "Product Code already exists. " });
                }
                var checkduplicateName = db.Products.Any(p => p.companyid == companyid && p.Name == ProductName);
                if (checkduplicateName)
                {
                    return RedirectToAction("CreateProduct", new { Err = "Product Name already exists. " });
                }
                if (product.PurchasePrice == null)
                    product.PurchasePrice = 0 ;
                if (product.SalesPrice == null)
                    product.SalesPrice = 0;
                
                product.IsPurchaseProduct = true;
                product.Code = ProductCode;
                product.Name = ProductName;
                product.companyid = companyid;
                product.CreatedBy = Createdby;
                product.CreatedOn = DateTime.Now;
                db.Products.Add(product);
                db.SaveChanges();
                long id = product.Id;


                return RedirectToAction("ShowAllProduct", new { Msg ="Product Created Successfully.. "});

            }
            catch
            {


                return RedirectToAction("CreateProduct", new { Err = "Please Try Again!...." });
            }
        }


        public JsonResult GetParentgroup(string id)
        {
            try
            {
                int parentid = Convert.ToInt32(id);
                var parentgroup = db.Groups.Where(d => d.Id == parentid).Select(d => new { Id = d.Id, Code = d.Code, Name = d.Name }).FirstOrDefault();

                return Json(parentgroup, JsonRequestBehavior.AllowGet);
            }
            catch
            {

                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
         [SessionExpire]
        public ActionResult ShowAllProduct(string Msg, string Err)
        {


            int userid = Convert.ToInt32(Session["userid"]);

            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }

            int companyid = Convert.ToInt32(Session["companyid"]);


            var result = db.Products.Where(d => d.companyid == companyid && d.Userid == userid && d.IsDeleted==null).ToList();
            return View(result);
        }


        public ActionResult ProductDetails(int id)
        {
            int userid = Convert.ToInt32(Session["userid"]);
            var result = db.Products.SingleOrDefault(d => d.Id == id);
            return View(result);
        }


         [SessionExpire]
        public ActionResult DeleteProduct(Product product)
        {
            try
            {

                int userid = Convert.ToInt32(Session["userid"]);
                var result = db.Products.Where(d => d.Id == product.Id).FirstOrDefault();
                result.IsDeleted = true;
                //  db.Products.Remove(result);
                db.SaveChanges();
                return RedirectToAction("ShowAllProduct", new { Msg = "Product deleted Successfully...." });
            }
            catch
            {


                return RedirectToAction("ShowAllProduct", new { Err = "Product Already Inuse. Cannot be Deleted." });
            }

        }



        [HttpGet]
        public ActionResult EditProduct(int id)
        {

            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);

            int userid = Convert.ToInt32(Session["userid"]);


            var result = db.Products.Where(d => d.Id == id).FirstOrDefault();
            //    ViewBag.unit = new SelectList(db.UOMs, "Id", "Code", result.UnitId);
            result.Productimage = "/productimg/" + result.Productimage;
            ViewBag.group = db.Groups.Where(d => d.CompanyId == companyid && d.UserId == userid).ToList();
            var price= result.PriceLists.Where(p => p.ProductId == id).FirstOrDefault();
            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid || d.UserId == 0 && d.CompanyId == 0).ToList();
            ViewBag.category = db.ProductCategory_MSTR.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            var groupcodelength = db.Groups.Where(g => g.Id == result.GroupId).Select(g => new {Code= g.Code, Name=g.Name }).FirstOrDefault();
            int l1 = groupcodelength.Code.Length + 1;
            int l2 = groupcodelength.Name.Length + 1;
            string code = result.Code.Substring(l1);
            string name = result.Name.Substring(l2);
            result.Code = code;
            result.Name = name;
            return View(result);
        }


        [HttpPost]
        public ActionResult EditProduct(Product productview, FormCollection collection)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);

            var product = db.Products.Find(productview.Id);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            int userid = Convert.ToInt32(Session["userid"]);
            int Createdby = Convert.ToInt32(Session["Createdid"]);

            string ProductCode = String.Empty;
            string ProductName = String.Empty;

            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid || d.UserId == 0 && d.CompanyId == 0).ToList();
            // ViewBag.category = db.Categories.Where(d => d.CompanyId == companyid && d.UserId == userid).ToList();
            ViewBag.group = db.Groups.Where(d => d.CompanyId == companyid && d.UserId == userid).ToList();


            var filename = string.Empty;
            string fn = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString();

            try
            {

                

                foreach (string upload in Request.Files)
                {
                    if (Request.Files[upload].ContentLength == 0) continue;
                    string pathToSave = Server.MapPath("~/productimg/");
                    filename = fn + Path.GetFileName(Request.Files[upload].FileName);
                    string uploadpath = Path.Combine(pathToSave, filename);
                    FileInfo fi1 = new FileInfo(uploadpath);
                    if (fi1.Exists)
                    {
                        System.IO.File.Delete(uploadpath);
                    }
                    Request.Files[upload].SaveAs(Path.Combine(pathToSave, filename));
                }


                if (filename != "")
                    product.Productimage = filename;
                else
                    product.Productimage = productview.Productimage;


                product.Userid = productview.Userid;
                product.Branchid = productview.Branchid;


                if (collection["parentcode"] == "")
                {
                    ProductCode = productview.Code;
                }
                else
                {
                    ProductCode = collection["parentcode"] + "-" + productview.Code;

                }


                if (collection["parentname"] == "")
                {
                    ProductName = productview.Name;
                }
                else
                {
                    ProductName = collection["parentname"] + " " + productview.Name;

                }
                //var products=db.Products.Where(p =>p.companyid == companyid && p.Code != ProductCode)
                var checkduplicateCode = db.Products.Any(p => p.companyid == companyid && p.Code == ProductCode && p.Id != productview.Id);
                if (checkduplicateCode)
                {
                    return RedirectToAction("EditProduct", new { Err = "Product Code already exists. " });
                }
                var checkduplicateName = db.Products.Any(p => p.companyid == companyid && p.Name == ProductName && p.Id != productview.Id);
                if (checkduplicateName)
                {
                    return RedirectToAction("EditProduct", new { Err = "Product Name already exists. " });
                }
                if (productview.PurchasePrice == null)
                    productview.PurchasePrice = 0;
                if (productview.SalesPrice == null)
                    productview.SalesPrice = 0;
                product.IsPurchaseProduct = true;
                product.GroupId = productview.GroupId;
                product.CategoryId = productview.CategoryId;
                product.Code = ProductCode;
                product.Name = ProductName;
                product.HSNCode = productview.HSNCode;
                product.UnitId = productview.UnitId;
                product.UnitIdSecondary = productview.UnitIdSecondary;
                product.UnitFormula = productview.UnitFormula;
                product.Description = productview.Description;
                product.PurchasePrice = productview.PurchasePrice;
                product.SalesPrice = productview.SalesPrice;
                product.companyid = productview.companyid;
                product.CreatedBy = productview.CreatedBy;
                product.CreatedOn = productview.CreatedOn;
                product.ModifiedBy = Createdby;
                product.ModifiedOn = DateTime.Now;
                
                db.SaveChanges();
                //long id = product.Id;


                return RedirectToAction("ShowAllProduct", new { Msg = "Product Updated Successfully.. " });

            }
            catch
            {


                return RedirectToAction("EditProduct", new { Err = "Please Try Again!...." });
            }
        }
        //public ActionResult Create()
        //{
        //    int userid = Convert.ToInt32(Session["userid"]);
        //    ViewBag.unit = db.UOMs.Where(d => d.UserId == userid).ToList();
        //    ViewBag.category = db.Categories.Where(d => d.UserId == userid).ToList();
        //    ViewBag.group = db.Groups.Where(d => d.UserId == userid).ToList();
        //    return PartialView();
        //}
        [HttpPost]
        public ActionResult Create(string Code, string Description, int? UnitId, int? GroupId, decimal SalesPrice)
        {
            try
            {
                int userid = Convert.ToInt32(Session["userid"]);
                int companyid = Convert.ToInt32(Session["companyid"]);
                Code=Code.Trim();
                var checkduplicate = db.Products.Any(p => p.Code == Code && p.companyid == companyid);
                if (checkduplicate)
                {
                    return Json("Duplicate", JsonRequestBehavior.AllowGet);
                }
                var product = new Product();
                product.Code = Code;
                product.Description = Description;
                product.UnitId = UnitId;
                product.GroupId = GroupId;
               
                product.SalesPrice = SalesPrice;
                product.IsStockProduct = true;
                product.IsSalesProduct = true;
                product.IsComponentProduct = false;
                product.IsAssembledProduct = false;
                product.Userid = userid;
                product.companyid = companyid;
                db.Products.Add(product);
                db.SaveChanges();
               decimal avaiability = 100;
               ProductModelView productModelView = new ProductModelView();
                //string unitname
                productModelView.Id = product.Id;
                productModelView.Code = product.Code;
                productModelView.UnitId = product.UnitId;
                productModelView.UnitName = db.UOMs.Where(u => u.Id == product.UnitId).Select(u=>u.Description).FirstOrDefault();
                productModelView.PurchasePrice = product.PurchasePrice;
                productModelView.SalesPrice = product.SalesPrice;
                productModelView.Availability = avaiability;
                return Json(  productModelView, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("Failed", JsonRequestBehavior.AllowGet);
            }
           
        }
        public JsonResult getProducts()
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            var username=User.Identity.Name;
         //   var userinf=db.Users.Where(u=>u.UserName==username).FirstOrDefault();
           // var products = db.Products.Where(p => p.Userid == userinf.UserId && p.companyid == userinf.CompanyId).ToList(); 
            var products = db.Products.Where(p=>p.companyid==companyid && p.IsDeleted == null).Select(p => new {Id=p.Id,Code=p.Code}).ToList(); 
            return Json(products, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getSelectedProduct(int id)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            var tOut = db.Stocks.Where(s => s.ArticleID == id && s.CompanyId == companyid && s.TranCode == "OUT").Sum(s => (decimal?)s.Items) ?? 0;
            var tIn = db.Stocks.Where(s => s.ArticleID == id && s.CompanyId == companyid && s.TranCode == "IN").Sum(s => (decimal?)s.Items) ?? 0;
            decimal avaiability = tIn - tOut;
            var product = db.Products.Where(p => p.Id == id && p.companyid == companyid ).Select(s => new { Id = s.Id, Code = s.Code, Name = s.Name, UnitId = s.UnitId, UnitName = s.UOM.Code, UnitIdSecondary = s.UnitIdSecondary, UnitNameSecondary = s.UOM1.Code, UnitFormula = s.UnitFormula, PurchasePrice = s.PurchasePrice, SalesPrice = s.SalesPrice, Availability = avaiability }).FirstOrDefault();
            return Json(product, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getSelectedProductPrice(int id)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            var tOut = db.Stocks.Where(s => s.ArticleID == id && s.CompanyId == companyid && s.TranCode == "OUT").Sum(s => (decimal?)s.Items) ?? 0;
            var tIn = db.Stocks.Where(s => s.ArticleID == id && s.CompanyId == companyid && s.TranCode == "IN").Sum(s => (decimal?)s.Items) ?? 0;
            decimal avaiability = tIn-tOut;
            var product = db.Products.Where(p => p.Id == id && p.companyid == companyid).Select(s => new { Id = s.Id, Code = s.Code, Name = s.Name, UnitId = s.UnitId, UnitName = s.UOM.Code, UnitIdSecondary = s.UnitIdSecondary, UnitNameSecondary = s.UOM1.Code, UnitFormula = s.UnitFormula, PurchasePrice = s.PurchasePrice, SalesPrice = s.SalesPrice, Availability = avaiability }).FirstOrDefault();
            return Json(product, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult getProductsAuto(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
           // long Branchid = Convert.ToInt64(Session["BranchId"]);
            var products = db.Products.Where(p => p.Name.Contains(query) && p.companyid == companyid && p.IsDeleted == null).Select(p => new { Code = p.Name, Id = p.Id }).ToList();
            return Json(products, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getSelectedProductAuto(string query = "")
        {
            long id = Convert.ToInt64(query);
            long companyid = Convert.ToInt32(Session["companyid"]);
            decimal avaiability = 100;
            var product = db.Products.Where(p => p.Id == id && p.companyid == companyid && p.IsDeleted == null).Select(s => new { Id = s.Id, Code = s.Code, Name = s.Name, UnitId = s.UnitId, UnitName = s.UOM.Code, UnitIdSecondary = s.UnitIdSecondary, UnitNameSecondary = s.UOM1.Code, UnitFormula = s.UnitFormula, PurchasePrice = s.PurchasePrice, SalesPrice = s.SalesPrice, Availability = avaiability }).FirstOrDefault();
            return Json(product, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult GetProductApi()
        {
            return Json(db.Products.ToList(), JsonRequestBehavior.AllowGet);
        }

        #region Sales Product



        [HttpGet]
        public ActionResult CreateSalesProduct()
        {


            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);

            int userid = Convert.ToInt32(Session["userid"]);



            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid || d.UserId == 0 && d.CompanyId == 0).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid).ToList();
            //ViewBag.group = db.Groups.Where(d => d.UserId == userid).ToList();



            ViewBag.group = db.ProductGroup(userid, companyid);


            List<Taxname> lsttax = new List<Taxname>();
            var taxpercent = db.Taxes.Where(d => d.UserId == userid && d.CompanyId == companyid || d.UserId == 0 && d.CompanyId == 0).ToList();


            foreach (var tax in taxpercent)
            {
                Taxname tname = new Taxname();

                tname.Id = tax.TaxId;
                tname.Name = tax.Name+"("+tax.Rate+"%)" ;
                lsttax.Add(tname);

            }

            ViewBag.tax = lsttax;




            return View();
        }


        [HttpPost]
        public ActionResult CreateSalesProduct(Product product, FormCollection collection)
        {

            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);

            int userid = Convert.ToInt32(Session["userid"]);

            string ProductCode = String.Empty;

            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid || d.UserId == 0 && d.CompanyId == 0).ToList();
            ViewBag.category = db.Categories.Where(d => d.CompanyId == companyid && d.UserId == userid).ToList();
            ViewBag.group = db.Groups.Where(d => d.CompanyId == companyid && d.UserId == userid).ToList();

            ViewBag.tax = db.Taxes.Where(d => d.UserId == userid && d.CompanyId == companyid || d.UserId == 0 && d.CompanyId == 0).ToList();


            try
            {

                string filename = Path.GetFileName(Request.Files["productimg"].FileName.ToString());

                if (filename != "")
                {
                    string extension = Path.GetExtension(filename);

                    string[] img = { ".jpeg", ".png", ".gif", ".bmp", ".jpg" };

                    if (img.Contains(extension))
                    {


                        string path = Server.MapPath("~/Productimages/");

                        Request.Files["productimg"].SaveAs(Path.Combine(path, filename));



                    }

                    else
                    {
                        ViewBag.Error = "Please select  .jpeg, .png, .gif, .bmp, .jpg Images";
                        return View();

                    }


                }



                int Createdby = Convert.ToInt32(Session["Createdid"]);
                product.Productimage = filename;
                product.Userid = userid;
                product.Branchid = Branchid;


                if (collection["parentname"] == "")
                {
                    ProductCode = product.Code;
                }
                else
                {
                    ProductCode = collection["parentname"] + "-" + product.Code;

                }

                decimal Maxamt = 0;
                 decimal Minamt = 0;
                 decimal disc = 0;

                if (collection["MinDiscount"] == null || collection["MinDiscount"] == "")
                {
                    Minamt = 0;
                }
                else
                {
                    Minamt = Convert.ToDecimal(collection["MinDiscount"]);
                }

                if (collection["MaxDiscount"] == null || collection["MaxDiscount"] == "")
                {
                    Maxamt = 0;
                }
                else
                {
                    Maxamt =Convert.ToDecimal(collection["MaxDiscount"]);
                }
                if (collection["Discount"] == null || collection["Discount"] == "")
                {
                    disc = 0;
                }
                else
                {
                    disc = Convert.ToDecimal(collection["Discount"]);
                }


                product.MaxDiscount =   Math.Round(Maxamt,2);
                product.MinDiscount = Math.Round(Minamt,2) ;
                product.Discount = Math.Round(disc,2);
                product.PurchaseTax = 0;
                product.IsSalesProduct = true;
                product.PurchasePrice = 0;
                product.Code = ProductCode;
                product.companyid = companyid;
                product.CreatedBy = Createdby;
                product.CreatedOn = DateTime.Now;
                db.Products.Add(product);
               // db.SaveChanges();
                return RedirectToAction("ShowAllSalesProduct", new { Msg = "Data Saved Successfully..." });

            }

            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Response.Write("- Property:" + ve.PropertyName + ", Error: " + ve.ErrorMessage);

                    }
                }
                throw;
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                return RedirectToAction("ShowAllSalesProduct", new { Err = "Please Try Again !.." });

            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Data  not Saved Successfully.......";
                return RedirectToAction("ShowAllSalesProduct", new { Err = "Please Try Again !.." });

            }
        }


        //public JsonResult GetParentgroup(string id)
        //{
        //    try
        //    {
        //        int parentid = Convert.ToInt32(id);
        //        var parentgroup = db.Groups.FirstOrDefault(d => d.Id == parentid).Name;

        //        return Json(parentgroup, JsonRequestBehavior.AllowGet);
        //    }
        //    catch
        //    {

        //        return Json("", JsonRequestBehavior.AllowGet);
        //    }
        //}

        public ActionResult ShowAllSalesProduct(string Msg, string Err)
        {


            int userid = Convert.ToInt32(Session["userid"]);

            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }

            int companyid = Convert.ToInt32(Session["companyid"]);


            var result = db.Products.Where(d => d.companyid == companyid && d.Userid == userid && d.IsSalesProduct == true).ToList();
            return View(result);
        }






        [HttpGet]
        public ActionResult EditSalesProduct(int id)
        {

            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);

            int userid = Convert.ToInt32(Session["userid"]);


            var result = db.Products.Where(d => d.Id == id).FirstOrDefault();
            //    ViewBag.unit = new SelectList(db.UOMs, "Id", "Code", result.UnitId);

            ViewBag.group = new SelectList(db.Groups, "Id", "Name", result.CategoryId);
            ViewBag.category = new SelectList(db.Categories, "Id", "Name", result.CategoryId);
            //ViewBag.pruchsetax = new SelectList(db.Taxes, "TaxId", "Name", result.PurchaseTax);
            //ViewBag.saletax = new SelectList(db.Taxes, "TaxId", "Name", result.SalesTax);

            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid || d.UserId == 0 && d.CompanyId == 0).ToList();

            List<Taxname> lst = new List<Taxname>();

            var tax = db.Taxes.Where(d => d.UserId == userid && d.CompanyId == companyid && d.BranchId == Branchid || d.UserId == 0 && d.CompanyId == 0 && d.BranchId == 0).ToList();

            foreach (var t in tax)
            {
                var tn = new Taxname();

                tn.Id = t.TaxId;
                tn.Name = t.Name + "(" + t.Rate.ToString() + "%)";
                lst.Add(tn);

            }


            ViewBag.tax = lst;
            return View(result);
        }


        [HttpPost]
        public ActionResult EditSalesPurchaseProduct(Product product)
        {
            try
            {
                int userid = Convert.ToInt32(Session["userid"]);
                var result = db.Products.Where(d => d.Userid == userid && d.Id == product.Id).FirstOrDefault();




                string filename = Path.GetFileName(Request.Files["productimg"].FileName.ToString());

                if (filename != "")
                {
                    string extension = Path.GetExtension(filename);

                    string[] img = { ".jpeg", ".png", ".gif", ".bmp", ".jpg" };

                    if (img.Contains(extension))
                    {


                        string path = Server.MapPath("~/Productimages/");

                        Request.Files["productimg"].SaveAs(Path.Combine(path, filename));
                        product.Productimage = filename;


                    }

                    else
                    {
                        ViewBag.Error = "Please select  .jpeg, .png, .gif, .bmp, .jpg Images";
                        return View();

                    }



                }

                int Createdby = Convert.ToInt32(Session["Createdid"]);
                product.ModifiedBy = Createdby;
                product.IsSalesProduct = true;
                product.ModifiedOn = DateTime.Now;
                db.Entry(result).CurrentValues.SetValues(product);
                db.SaveChanges();


                return RedirectToAction("ShowAllSalesProduct", new { Msg = "Data updated Successfully...." });
            }

            catch (DbEntityValidationException e) //--------Form Validation Error Throw--------//
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Response.Write("- Property:" + ve.PropertyName + ", Error: " + ve.ErrorMessage);

                    }
                }
                throw;
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                return RedirectToAction("ShowAllSalesProduct", new { Err = "Please Try Again !...." });

            }
            catch (DbUpdateException ex) //--------Databse Error Throw--------//
            {
                UpdateException updateException = (UpdateException)ex.InnerException;
                SqlException sqlException = (SqlException)updateException.InnerException;

                foreach (SqlError error in sqlException.Errors)
                {
                    Response.Write("- Property:" + error.Number + ", Error: " + error.Message);

                }
                return RedirectToAction("ShowAllSalesProduct", new { Err = "Please Try Again !...." });
            }
            catch
            {


                return RedirectToAction("ShowAllSalesProduct", new { Err = "Data cannot be updated...." });
            }

        }










        [HttpGet]
        public ActionResult EditSalesPurchaseProduct(int id)
        {

            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);

            int userid = Convert.ToInt32(Session["userid"]);


            var result = db.Products.Where(d => d.Id == id).FirstOrDefault();
            //    ViewBag.unit = new SelectList(db.UOMs, "Id", "Code", result.UnitId);

            ViewBag.group = new SelectList(db.Groups, "Id", "Name", result.CategoryId);
            ViewBag.category = new SelectList(db.Categories, "Id", "Name", result.CategoryId);
            //ViewBag.pruchsetax = new SelectList(db.Taxes, "TaxId", "Name", result.PurchaseTax);
            //ViewBag.saletax = new SelectList(db.Taxes, "TaxId", "Name", result.SalesTax);

            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid || d.UserId == 0 && d.CompanyId == 0).ToList();

            List<Taxname> lst = new List<Taxname>();

            var tax = db.Taxes.Where(d => d.UserId == userid && d.CompanyId == companyid && d.BranchId == Branchid || d.UserId == 0 && d.CompanyId == 0 && d.BranchId == 0).ToList();

            foreach (var t in tax)
            {
                var tn = new Taxname();

                tn.Id = t.TaxId;
                tn.Name = t.Name + "(" + t.Rate.ToString() + "%)";
                lst.Add(tn);

            }


            ViewBag.tax = lst;
            return View(result);
        }


        [HttpPost]
        public ActionResult EditSalesProduct(Product product)
        {
            try
            {
                int userid = Convert.ToInt32(Session["userid"]);
                var result = db.Products.Where(d => d.Userid == userid && d.Id == product.Id).FirstOrDefault();




                string filename = Path.GetFileName(Request.Files["productimg"].FileName.ToString());

                if (filename != "")
                {
                    string extension = Path.GetExtension(filename);

                    string[] img = { ".jpeg", ".png", ".gif", ".bmp", ".jpg" };

                    if (img.Contains(extension))
                    {


                        string path = Server.MapPath("~/Productimages/");

                        Request.Files["productimg"].SaveAs(Path.Combine(path, filename));
                        product.Productimage = filename;


                    }

                    else
                    {
                        ViewBag.Error = "Please select  .jpeg, .png, .gif, .bmp, .jpg Images";
                        return View();

                    }



                }
                product.IsSalesProduct = true;
                int Createdby = Convert.ToInt32(Session["Createdid"]);
                result.ModifiedBy = Createdby;
                result.ModifiedOn = DateTime.Now;
                db.Entry(result).CurrentValues.SetValues(product);
                db.SaveChanges();


                return RedirectToAction("ShowAllSalesProduct", new { Msg = "Data updated Successfully...." });
            }

            catch (DbEntityValidationException e) //--------Form Validation Error Throw--------//
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Response.Write("- Property:" + ve.PropertyName + ", Error: " + ve.ErrorMessage);

                    }
                }
                throw;
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                return RedirectToAction("ShowAllSalesProduct", new { Err = "Please Try Again !...." });

            }
            catch (DbUpdateException ex) //--------Databse Error Throw--------//
            {
                UpdateException updateException = (UpdateException)ex.InnerException;
                SqlException sqlException = (SqlException)updateException.InnerException;

                foreach (SqlError error in sqlException.Errors)
                {
                    Response.Write("- Property:" + error.Number + ", Error: " + error.Message);

                }
                return RedirectToAction("ShowAllSalesProduct", new { Err = "Please Try Again !...." });
            }
            catch
            {


                return RedirectToAction("ShowAllSalesProduct", new { Err = "Data cannot be updated...." });
            }

        }









        public ActionResult ProducSalestDetails(int id)
        {
            int userid = Convert.ToInt32(Session["userid"]);
            var result = db.Products.SingleOrDefault(d => d.Id == id);
            return View(result);
        }



        public ActionResult DeleteSalesProduct(Product product)
        {
            try
            {

                int userid = Convert.ToInt32(Session["userid"]);
                var result = db.Products.SingleOrDefault(d => d.Id == product.Id);
                db.Products.Remove(result);
                db.SaveChanges();
                return RedirectToAction("ShowAllSalesProduct", new { Msg = "Product deleted Successfully...." });
            }
            catch
            {


                return RedirectToAction("ShowAllSalesProduct", new { Err = InventoryMessage.Delte });
            }

        }



        public ActionResult Upload()
        {
            return View();
        }
        public ActionResult ReadExcelProduct()
        {
            HttpPostedFileBase hpf = this.Request.Files[0];
            ImportExcelXLSProduct(hpf);
            return RedirectToAction("ShowAllProduct", "Product");
        }
        public void ImportExcelXLSProduct(HttpPostedFileBase file)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);

            int userid = Convert.ToInt32(Session["userid"]);
            var groups = db.Groups.Where(g => g.CompanyId == companyid ).ToList();
            var products = db.Products.Where(p => p.companyid == companyid).ToList();
            var units = db.UOMs.Where(p => p.CompanyId == companyid).Select(p=>new{p.Id,p.Code,p.Description}).ToList();
            string sheetname = string.Empty;
            
            var fileName = System.IO.Path.GetFileName(file.FileName);

            var path = System.IO.Path.Combine(Server.MapPath("~/App_Data/"), fileName);

            file.SaveAs(path);
            var excel = new LinqToExcel.ExcelQueryFactory(path);
            excel.DatabaseEngine = DatabaseEngine.Ace;
            var worksheetNames = excel.GetWorksheetNames();
            foreach (var worksheetName in worksheetNames)
            {
                sheetname = worksheetName;
                break;
            }
            try
            {
                var datalist = from c in excel.Worksheet<ProductExcelModelView>(sheetname) select c;


                foreach (var data in datalist)
                {
                    string gcode=string.Empty;
                    string pcode=string.Empty;
                    string punitname = string.Empty;
                    string sunitname = string.Empty;
                    long? punit = 0;
                    long? sunit = 0;
                    long? categoryid = 0;

                    if(data.GroupCode==null)
                       continue;
                     else
                    gcode=data.GroupCode.Trim();

                    if(data.Code==null)
                            continue;
                        else
                           pcode=data.Code.Trim();
                    pcode = gcode + "-" + pcode;

                    if (data.CategoryId >= 1 && data.CategoryId <= 3)
                        categoryid = data.CategoryId;
                    else
                        continue;

                    punitname = data.UnitName.Trim();
                    sunitname = data.UnitSecondaryName.Trim();

                    punit = units.Where(u => u.Description == punitname || u.Code == punitname).Select(u => u.Id).FirstOrDefault();
                    if (punit == null)
                        continue;

                    sunit = units.Where(u => u.Description == sunitname || u.Code == sunitname).Select(u => u.Id).FirstOrDefault();
                    if (sunit == null)
                        continue;
                    if (data.UnitFormula == null)
                        continue;

                    var group = groups.Where(g => g.Code == gcode).FirstOrDefault();
                    if (group == null)
                        continue;
               
                    long id = products.Where(p => p.Code == pcode).Select(p => p.Id).FirstOrDefault();
                    
                    if (id == 0)
                    {
                        
                        
                        var product = new Product();
                        product.GroupId = group.Id;
                        product.CategoryId = categoryid;
                        product.Code =group.Code +"-" + data.Code.Trim();
                        product.Name =group.Name + " " + data.Name;
                        product.Description = data.Description;
                        product.UnitId = punit;
                        product.UnitFormula = data.UnitFormula;
                        product.UnitIdSecondary = sunit;
                        product.PurchasePrice = 0;
                        product.SalesPrice = 0;
                        product.Userid = userid;
                        product.companyid = companyid;
                        product.Branchid = Branchid;
                        product.IsPurchaseProduct = true;
                        product.IsSalesProduct = true;
                        product.IsStockProduct = true;
                        product.IsComponentProduct = false;
                        db.Products.Add(product);


                    }
                    else
                    {
                        var product = db.Products.Find(id);
                        product.GroupId = group.Id;
                        product.CategoryId = categoryid;
                        product.Code = group.Code + "-" + data.Code.Trim();
                        product.Name = group.Name + " " + data.Name;
                        product.Description = data.Description;
                        product.UnitId = punit;
                        product.UnitFormula = data.UnitFormula;
                        product.UnitIdSecondary = sunit;
                        //product.PurchasePrice = 0;
                        //product.SalesPrice = 0;
                        //product.Userid = userid;
                        //product.companyid = companyid;
                        //product.Branchid = Branchid;
                        //product.IsPurchaseProduct = true;
                       // product.IsSalesProduct = true;
                        //product.IsStockProduct = true;
                        //product.IsComponentProduct = false;
                       
                        db.Entry(product).State = EntityState.Modified;
                    }
                }

                db.SaveChanges();

            

            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Response.Write("- Property:" + ve.PropertyName + ", Error: " + ve.ErrorMessage);

                    }
                }
                throw;
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
        }
        //[HttpGet]
        //public ActionResult EditProduct(int id)
        //{

        //    int companyid = Convert.ToInt32(Session["companyid"]);


        //    long Branchid = Convert.ToInt64(Session["BranchId"]);

        //    int userid = Convert.ToInt32(Session["userid"]);


        //    var result = db.Products.Where(d => d.Id == id).FirstOrDefault();
        //    //    ViewBag.unit = new SelectList(db.UOMs, "Id", "Code", result.UnitId);

        //    ViewBag.group = new SelectList(db.Groups, "Id", "Name", result.CategoryId);
        //    ViewBag.category = new SelectList(db.Categories, "Id", "Name", result.CategoryId);
        //    //ViewBag.pruchsetax = new SelectList(db.Taxes, "TaxId", "Name", result.PurchaseTax);
        //    //ViewBag.saletax = new SelectList(db.Taxes, "TaxId", "Name", result.SalesTax);

        //    ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid || d.UserId == 0 && d.CompanyId == 0).ToList();

        //    ViewBag.tax = db.Taxes.Where(d => d.UserId == userid && d.CompanyId == companyid && d.BranchId == Branchid || d.UserId == 0 && d.CompanyId == 0).ToList();

        //    return View(result);
        //}


        //[HttpPost]
        //public ActionResult EditProduct(Product product)
        //{
        //    try
        //    {
        //        int userid = Convert.ToInt32(Session["userid"]);
        //        var result = db.Products.SingleOrDefault(d => d.Userid == userid && d.Id == product.Id);




        //        string filename = Path.GetFileName(Request.Files["productimg"].FileName.ToString());

        //        if (filename != "")
        //        {
        //            string extension = Path.GetExtension(filename);

        //            string[] img = { ".jpeg", ".png", ".gif", ".bmp", ".jpg" };

        //            if (img.Contains(extension))
        //            {


        //                string path = Server.MapPath("~/Productimages/");

        //                Request.Files["productimg"].SaveAs(Path.Combine(path, filename));
        //                product.Productimage = filename;


        //            }

        //            else
        //            {
        //                ViewBag.Error = "Please select  .jpeg, .png, .gif, .bmp, .jpg Images";
        //                return View();

        //            }



        //        }

        //        int Createdby = Convert.ToInt32(Session["Createdid"]);
        //        result.ModifiedBy = Createdby;
        //        result.ModifiedOn = DateTime.Now;
        //        db.Entry(result).CurrentValues.SetValues(product);
        //        db.SaveChanges();


        //        return RedirectToAction("ShowAllProduct", new { Msg = "Data updated Successfully...." });
        //    }

        //    catch (DbEntityValidationException e) //--------Form Validation Error Throw--------//
        //    {
        //        foreach (var eve in e.EntityValidationErrors)
        //        {
        //            Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
        //            eve.Entry.Entity.GetType().Name, eve.Entry.State);
        //            foreach (var ve in eve.ValidationErrors)
        //            {
        //                Response.Write("- Property:" + ve.PropertyName + ", Error: " + ve.ErrorMessage);

        //            }
        //        }
        //        throw;
        //        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
        //        return RedirectToAction("ShowAllSupplier", new { Err = "Please Try Again !...." });

        //    }
        //    catch (DbUpdateException ex) //--------Databse Error Throw--------//
        //    {
        //        UpdateException updateException = (UpdateException)ex.InnerException;
        //        SqlException sqlException = (SqlException)updateException.InnerException;

        //        foreach (SqlError error in sqlException.Errors)
        //        {
        //            Response.Write("- Property:" + error.Number + ", Error: " + error.Message);

        //        }
        //        return RedirectToAction("ShowAllSupplier", new { Err = "Please Try Again !...." });
        //    }
        //    catch
        //    {


        //        return RedirectToAction("ShowAllProduct", new { Err = "Data cannot be updated...." });
        //    }

        //}















        #endregion


        #region Product UOM Settings
        public ActionResult ProductMultiUnit()
        {
            var products = db.Products.Select(d=> new ProductModelView { Id = d.Id, Name = d.Name, UnitIdSecondary = d.UnitIdSecondary, UnitId = d.UnitId, UnitFormula = d.UnitFormula, GroupId = d.GroupId, }).OrderBy(d=>d.GroupId).ThenBy(d=>d.Name).ToList();
            var uomList = db.UOMs.ToList();
            foreach(var product in products)
            {
                product.UnitSecondaryList = new SelectList(uomList, "Id", "Code", product.UnitIdSecondary);
                product.UnitList = new SelectList(uomList, "Id", "Code", product.UnitId);
                //  model.OrderTemplates = new SelectList(db.OrderTemplates, "OrderTemplateId", "OrderTemplateName", 1);
            }
            return View(products);
        }

        [HttpPost]
        public ActionResult UpdateProductUnit(long productId, int pu, decimal uf,int su)
        {
            var product = db.Products.Find(productId);
            if(product == null)
            {
                return Json("Failed", JsonRequestBehavior.AllowGet);
            }
            else
            {
                product.UnitIdSecondary = pu;
                product.UnitFormula = uf;
                product.UnitId = su;
                db.SaveChanges();
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteProduct(long productId)
        {
            var product = db.Products.Find(productId);
            if (product == null)
            {
                return Json("Failed", JsonRequestBehavior.AllowGet);
            }
            else
            {
                try
                {

                    db.Products.Remove(product);
                    db.SaveChanges();
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json("Failed", JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion
        #region Set Product Settings
        public ActionResult SetProductOpening()
        {
            int fyid = Convert.ToInt32(Session["fid"]);
            var products = db.Products.Select(d => new ProductModelView { Id = d.Id, Name = d.Name, UnitIdSecondary = d.UnitIdSecondary, UnitId = d.UnitId, UnitFormula = d.UnitFormula, GroupId = d.GroupId, }).OrderBy(d => d.GroupId).ThenBy(d => d.Name).ToList();
            var uomList = db.UOMs.ToList();
          
          
            var opening = db.ProductOpenings.Where(d => d.FinancialYearId == fyid).ToList();
            var productOpening = (from p in products
                                 join o in opening
                                 on p.Id equals o.ProductId into po
                                 from o in po.DefaultIfEmpty()
                                 select new ProductOpeningModelView{ ProductName = p.Name, UnitIdSecondary = p.UnitIdSecondary, UnitId = p.UnitId, OpeningSecondary =  o== null ? 0: o.Opening, OpeningPrimary = o == null ? 0 : o.Opening*p.UnitFormula }).ToList();

          
            foreach (var product in productOpening)
            {
                product.UnitSecondaryList = new SelectList(uomList, "Id", "Code", product.UnitIdSecondary);
                product.UnitList = new SelectList(uomList, "Id", "Code", product.UnitId);
                //  model.OrderTemplates = new SelectList(db.OrderTemplates, "OrderTemplateId", "OrderTemplateName", 1);
            }
            return View(productOpening);
        }
        [HttpPost]
        public ActionResult SetProductOpening(long productId, decimal pu, decimal uf, decimal su)
        {
            int fyid = Convert.ToInt32(Session["fid"]);
            var opening = db.ProductOpenings.Where(d => d.FinancialYearId == fyid).ToList();
            return View(opening);
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;

namespace XenERP.Controllers
{
    public class WarehouseController : Controller
    {
        InventoryEntities db = new InventoryEntities();

        XenERP.Models.Repository.TaxRepository rep = new Models.Repository.TaxRepository();

        public ActionResult Index()
        {
            return View();
        }

        #region  //-------------WareHouse Master---//



        [HttpGet]
        public ActionResult ShowAllWarehouse(string Msg, string Err)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }


            int userid = Convert.ToInt32(Session["userid"]);
            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);

            var ware = db.Warehouses.Where(d => d.Userid == userid && d.Companyid == companyid).ToList();


            return View(ware);

        }


        [HttpGet]
        public ActionResult CreateWarehouse(string Msg, string Err)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }




            int userid = Convert.ToInt32(Session["userid"]);


            var company = db.Companies.Where(d => d.Userid == userid).FirstOrDefault();

            ViewBag.company = db.Companies.Where(d => d.Userid == userid).ToList();


            ViewBag.branch = db.BranchMasters.Where(d => d.UserId == userid && d.CompanyId == company.Id).ToList();


            ViewBag.countrycode = db.CountryCodes.ToList().OrderBy(d => d.Country);

            ViewBag.Country = db.Countries.ToList();

            return View();

        }


        [HttpPost]
        public ActionResult CreateWarehouse(Warehouse ware)
        {
            try
            {
                int Createdby = Convert.ToInt32(Session["Createdid"]);


                int companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                int userid = Convert.ToInt32(Session["userid"]);


                ware.Userid = userid;
                ware.Companyid = companyid;
                ware.CreatedBy = Createdby.ToString();
                ware.Branchid = Branchid;
                ware.CreatedOn = DateTime.Now;
                db.Warehouses.Add(ware);
                db.SaveChanges();


                return RedirectToAction("ShowAllWarehouse", new { Msg = "Data Saved Successfully...." });
            }
            catch
            {


                return RedirectToAction("ShowAllWarehouse", new { Err = "Data  not saved successfully...." });
            }
        }

        [HttpGet]
        public ActionResult EditWarehouse(int id)
        {
            Warehouse ware = new Warehouse();
            var result = db.Warehouses.SingleOrDefault(d => d.Id == id);

            int countryid = Convert.ToInt32(result.Country);
            ViewBag.Country = new SelectList(db.Countries, "CountryId", "Country1", countryid);





            int userid = Convert.ToInt32(Session["userid"]);





            var company = db.Companies.Where(d => d.Userid == userid).FirstOrDefault();

            ViewBag.company = new SelectList(db.Companies, "Id", "Name", ware.Companyid);


            ViewBag.branch = new SelectList(db.BranchMasters, "Id", "Name", ware.Branchid);


            ViewBag.countrycode = db.CountryCodes.ToList().OrderBy(d => d.Country);

            ViewBag.Country = db.Countries.ToList();










            return View(result);

        }



         [HttpPost]
        public ActionResult EditWarehouse(Warehouse ware)
        {
            try
            {

                var result = db.Warehouses.Where(d => d.Id == ware.Id).FirstOrDefault();



                int Createdby = Convert.ToInt32(Session["Createdid"]);

                result.ModifiedOn = DateTime.Now;
                result.ModifiedBy = Createdby.ToString();
                db.Entry(result).CurrentValues.SetValues(ware);

                db.SaveChanges();


                return RedirectToAction("ShowAllWarehouse", new { Msg = "Data Updated Successfully...." });
            }
            catch
            {


                return RedirectToAction("ShowAllWarehouse", new { Err = "Data  not saved successfully...." });
            }
        }


        public JsonResult CheckWarehouseCode(string Id, string Code)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            int id = 0;
            string codes = Code;
            string trimcode = codes.Trim();

            if (String.IsNullOrEmpty(Id) || String.IsNullOrWhiteSpace(Id))
            {

                bool Iscustomer = db.Warehouses.Any(d => d.Code == trimcode && d.Companyid == companyid && d.Branchid == Branchid);
                return Json(!Iscustomer, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //    id = Convert.ToInt32(Id);

                bool Iscustomer = db.Warehouses.Where(d => d.Id != id).Any(d => d.Code == trimcode && d.Companyid == companyid && d.Branchid == Branchid);
                return Json(!Iscustomer, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Deletewarehouse(int id)
        {
            try
            {
                var result = db.Warehouses.Where(d => d.Id == id).FirstOrDefault();

                db.Warehouses.Remove(result);
                db.SaveChanges();
                return RedirectToAction("ShowAllWarehouse", new { Msg = "Row Deleted Successfully...." });
            }
            catch
            {


                return RedirectToAction("ShowAllWarehouse", new { Err = "Row cannot be deleted...." });
            }
        }

        public JsonResult Checkwarehouse(string Name, string Id, int Branchid)
        {
            int userid = Convert.ToInt32(Session["userid"]);
            int id = 0;
            if (Id == null)
            {

                bool ware = db.Warehouses.Any(d => d.Name == Name && d.Branchid == Branchid);
                return Json(!ware, JsonRequestBehavior.AllowGet);
            }
            else
            {

                bool ware = db.Warehouses.Where(d => d.Id != id).Any(d => d.Name == Name && d.Branchid == Branchid);
                return Json(ware, JsonRequestBehavior.AllowGet);

            }
        }


        [HttpGet]
        public ActionResult WarehouseDetails(int id)
        {

            var ware = db.Warehouses.SingleOrDefault(d => d.Id == id);


            return View(ware);

        }
        [HttpGet]
        public ActionResult getWarehouseDetail(int id)
        {

            var ware = db.Warehouses.Where(d => d.Id == id).FirstOrDefault();
            if (ware.Country != null)
            {
                int c1 = Convert.ToInt32(ware.Country);
                ware.Country = db.Countries.Where(c => c.CountryId == c1).Select(c => c.Country1).FirstOrDefault();
            }

            return Json(ware, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult getWarehouse(int id)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            var ware = db.Warehouses.Where(d => d.Id == id && d.Companyid == companyid).Select(w => new { AdressName = w.AdressName, Address = w.Address, Suburb = w.Suburb, Town = w.Town, State = w.State, Country = w.Country, PIN = w.PIN, ContactName = w.ContactName }).FirstOrDefault();
            var wareHouse = new Warehouse();
            wareHouse.ContactName = ware.ContactName;
            wareHouse.Address = ware.Address;
            wareHouse.Suburb = ware.Suburb;
            wareHouse.Town = ware.Town;
            wareHouse.State = ware.State;
            wareHouse.Country = ware.Country;
            wareHouse.PIN = ware.PIN;
            wareHouse.Country = ware.Country;
            if (wareHouse.Country != null)
            {
                int c1 = Convert.ToInt32(ware.Country);
                wareHouse.Country = db.Countries.Where(c => c.CountryId == c1).Select(c => c.Country1).FirstOrDefault();
            }
            return Json(wareHouse, JsonRequestBehavior.AllowGet);

        }
        #endregion






        #region  //-------------WareHouse Transfer---//

        [HttpGet]
        public ActionResult CreateWarehouseTransfer(string Msg, string Err)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }
            

            int companyid = Convert.ToInt32(Session["companyid"]);


              long Branchid = Convert.ToInt64(Session["BranchId"]);

            int userid = Convert.ToInt32(Session["userid"]);

            ViewBag.FromWarehouse = db.Warehouses.Where(d => d.Companyid == companyid && d.Branchid == Branchid && d.Userid == userid).ToList();

            ViewData["ToWarehouse"] = db.Warehouses.Where(d => d.Companyid == companyid && d.Branchid == Branchid && d.Userid == userid).ToList();

            return View();
        }


        [HttpPost]
        public ActionResult CreateWarehouseTransfer(FormCollection collection)
        {


            try
            {

                int companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                int userid = Convert.ToInt32(Session["userid"]);





                Stock stock = new Stock();

                string[] prd = collection["txtproductid"].Split(',');
                string[] qut = collection["txtquantity"].Split(',');


                int i = 0;

                foreach (var prod in prd)
                {
                    
                    //---For Item Out---//
                    stock.TranDate = Convert.ToDateTime(collection["TranDate"]);
                    stock.Items = Convert.ToDecimal(qut[i]);
                    stock.ArticleID = Convert.ToInt32(prod);
                    stock.WarehouseId = Convert.ToInt32(collection["WarehouseId"]);
                    stock.TranCode = "OUT";
                    stock.BranchId = Branchid;
                    stock.CompanyId = companyid;
                    stock.TransTag = "TRN";
                    stock.TranId = 0;
                    stock.UserId = userid;
                    stock.CreatedBy = Convert.ToInt32(Session["Createdid"]);
                    db.Stocks.Add(stock);
                    db.SaveChanges();




                    //---For Item IN---//
                  
                    stock.TranDate = Convert.ToDateTime(collection["TranDate"]);
                    stock.ArticleID = Convert.ToInt32(prod);
                    stock.WarehouseId = Convert.ToInt32(collection["WarehouseTo"]);
                    stock.TranCode = "IN";
                    stock.TransTag = "TRN";
                    stock.Items = Convert.ToDecimal(qut[i]);
                    stock.TranId = 0;
                    stock.BranchId = Branchid;
                    stock.CompanyId = companyid;
                    stock.UserId = userid;
                    stock.CreatedBy = Convert.ToInt32(Session["Createdid"]);
                    db.Stocks.Add(stock);
                    db.SaveChanges();

                    i++;

                }

                return RedirectToAction("CreateWarehouseTransfer", new { Msg = "Items has been transfer successfully..." });
            }

            catch
            {


                return RedirectToAction("CreateWarehouseTransfer", new { Err = "Data  not saved successfully...." });
            }
           
        }
           
        

        public JsonResult GetWarehouse(int type)
        {

            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            var warehouse = rep.GetWarehouse(type, companyid, userid, Branchid);
            return Json(warehouse, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult getProduct(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            var customers = db.Products.Where(p => p.Code.Contains(query) && p.companyid == companyid).Select(p => new { Name = p.Code, Id = p.Id }).ToList();
            return Json(customers, JsonRequestBehavior.AllowGet);
        }


        public JsonResult getSelectedProduct(int id, int wareid)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

    
            //decimal? ProductIN = db.Stocks.Where(p => p.ArticleID == id && p.CompanyId == companyid && p.BranchId == Branchid && p.TranCode=="IN" && p.TransTag!="TRN").Select(d=>d.Items).Sum();
            //decimal? ProductOut = db.Stocks.Where(p => p.ArticleID == id && p.CompanyId == companyid && p.BranchId == Branchid && p.TranCode == "OUT").Select(d => d.Items).Sum();

            //decimal? StockProduct = ProductIN - ProductOut;


           
            decimal ProductIN = db.Stocks.Where(p => p.ArticleID == id && p.CompanyId == companyid && p.BranchId == Branchid && p.TranCode == "IN" && p.WarehouseId==wareid).Sum(p => (decimal?)p.Items) ?? 0;




            decimal ProductOut = db.Stocks.Where(p => p.ArticleID == id && p.CompanyId == companyid && p.BranchId == Branchid && p.TranCode == "OUT" && p.WarehouseId == wareid).Sum(p => (decimal?)p.Items) ?? 0;
               


            decimal StockProduct = ProductIN - ProductOut;
            return Json(StockProduct, JsonRequestBehavior.AllowGet);
        }


        public JsonResult getSelectedProductPrice(int id)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);



            decimal? Productprice = db.Products.Where(p => p.companyid == companyid && p.Id == id).Select(p => p.SalesPrice).FirstOrDefault();




            return Json(Productprice, JsonRequestBehavior.AllowGet);
        }


        //public JsonResult getProductAvail(int id)
        //{
        //    long companyid = Convert.ToInt32(Session["companyid"]);
        //    long Branchid = Convert.ToInt64(Session["BranchId"]);
        //    var Product = db.Products.Where(p => p.Id == id && p.companyid == companyid && p.Branchid == Branchid).Select(p => new { Name = p.Description }).FirstOrDefault();
        //    return Json(Product, JsonRequestBehavior.AllowGet);
        //}

        #endregion
        #region  //-------------Item Adjustment---//

        [HttpGet]
        public ActionResult ItemAdjustment(string Msg, string Err)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }


            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);

            int userid = Convert.ToInt32(Session["userid"]);

            ViewBag.FromWarehouse = db.Warehouses.Where(d => d.Companyid == companyid && d.Branchid == Branchid && d.Userid == userid).ToList();

            ViewData["ToWarehouse"] = db.Warehouses.Where(d => d.Companyid == companyid && d.Branchid == Branchid && d.Userid == userid).ToList();

            return View();
        }


        [HttpPost]
        public ActionResult ItemAdjustment(FormCollection collection)
        {


            try
            {

                int companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                int userid = Convert.ToInt32(Session["userid"]);





                Stock stock = new Stock();

                string[] prd = collection["txtproductid"].Split(',');
                string[] qut = collection["txtquantity"].Split(',');
                string[] totval = collection["txttotalvalue"].Split(',');
                string[] adj = collection["Adjust"].Split(',');

                int i = 0;
                int j = 0;
                int k = 0;

                foreach (var prod in prd)
                {

                    //---For Item Out---//
                    stock.TranDate = Convert.ToDateTime(collection["TranDate"]);
                    stock.Items = Convert.ToDecimal(qut[i]);
                    stock.ArticleID = Convert.ToInt32(prod);
                    stock.WarehouseId = Convert.ToInt32(collection["WarehouseId"]);
                    string adjustment = adj[k];

                    if (adjustment == "INCREASE")
                    {
                        stock.TranCode = "IN";
                    }

                    if (adjustment == "DECREASE")
                    {
                        stock.TranCode = "OUT";
                    }

                    stock.Price = Convert.ToDecimal(totval[j]);

                    stock.BranchId = Branchid;
                    stock.CompanyId = companyid;
                    stock.TransTag = "ADJ";
                    stock.TranId = 0;
                    stock.UserId = userid;
                    stock.CreatedBy = Convert.ToInt32(Session["Createdid"]);
                    db.Stocks.Add(stock);
                    db.SaveChanges();

                    i++;
                    j++;
                    k++;

                }

                return RedirectToAction("ItemAdjustment", new { Msg = "Items Adjustment Successfully..." });
            }

            catch
            {


                return RedirectToAction("ItemAdjustment", new { Err = "Please Try Again...." });
            }

        }




        #endregion
    }
}

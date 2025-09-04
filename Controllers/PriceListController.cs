using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Transactions;

namespace XenERP.Controllers
{
    public class PriceListController : Controller
    {
        //
        // GET: /PriceList/
        [HttpGet]
        public ActionResult FetchPriceList(string Msg)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                List<PriceListModelView> objList = new List<PriceListModelView>();
                int userid = Convert.ToInt32(Session["userid"]);
                int companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                var existList = db.PriceLists.Where(r => r.CompanyId == companyid && r.UserId == userid).ToList();
                foreach (var item in existList)
                {
                    var obj = new PriceListModelView();
                    obj.SPTier1 = item.SPTier1;
                    obj.SPTier2 = item.SPTier2;
                    obj.SPTier3 = item.SPTier3;
                    obj.SPTier4 = item.SPTier4;
                    obj.SPTier5 = item.SPTier5;
                    obj.Code = item.Product.Code;
                    obj.Name = item.Product.Name;
                    obj.ProductId = item.Product.Id;
                    objList.Add(obj);
                }
                var priceList = db.Products.Where(d => d.Userid == userid && d.companyid == companyid && !d.PriceLists.Any(r => r.ProductId == d.Id)).ToList();
                foreach (var item in priceList)
                {
                    var obj = new PriceListModelView();
                    obj.ProductId = item.Id;
                    obj.Code = item.Code;
                    obj.Name = item.Name;
                    obj.SPTier1 = 0;
                    obj.SPTier2 = 0;
                    obj.SPTier3 = 0;
                    obj.SPTier4 = 0;
                    obj.SPTier5 = 0;
                    objList.Add(obj);
                }
                ViewBag.Message = Msg;
                return View(objList);
            }
        }

        [HttpPost]
        public ActionResult FetchPriceList(decimal? sp1, decimal? sp2, decimal? sp3, decimal? sp4, decimal? sp5, long PI)
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
                        var productId = db.PriceLists.Where(r => r.ProductId == PI).ToList();

                        if (productId.Count() > 0 &&
                            productId.FirstOrDefault().SPTier1 == sp1 &&
                            productId.FirstOrDefault().SPTier2 == sp2 &&
                            productId.FirstOrDefault().SPTier3 == sp3 &&
                            productId.FirstOrDefault().SPTier4 == sp4 &&
                            productId.FirstOrDefault().SPTier5 == sp5)
                        {
                            return Json("Success", JsonRequestBehavior.AllowGet);
                        }


                        else if (productId.Count() > 0)
                        {
                            productId.FirstOrDefault().SPTier1 = sp1;
                            productId.FirstOrDefault().SPTier2 = sp2;
                            productId.FirstOrDefault().SPTier3 = sp3;
                            productId.FirstOrDefault().SPTier4 = sp4;
                            productId.FirstOrDefault().SPTier5 = sp5;
                            productId.FirstOrDefault().CompanyId = companyId;
                            productId.FirstOrDefault().BranchId = branchId;
                            productId.FirstOrDefault().UserId = userId;
                            productId.FirstOrDefault().ModifiedBy = Createdby;
                            productId.FirstOrDefault().ModifiedOn = DateTime.Now;
                        }
                        else
                        {
                            PriceList List = new PriceList();
                            List.SPTier1 = sp1;
                            List.SPTier2 = sp2;
                            List.SPTier3 = sp3;
                            List.SPTier4 = sp4;
                            List.SPTier5 = sp5;
                            List.ProductId = PI;
                            List.CompanyId = companyId;
                            List.BranchId = branchId;
                            List.UserId = userId;
                            List.CreatedBy = Createdby;
                            List.CreatedOn = DateTime.Now;
                            db.PriceLists.Add(List);

                        }
                        PriceList_Log log = new PriceList_Log();
                        log.SPTier1 = sp1;
                        log.SPTier2 = sp2;
                        log.SPTier3 = sp3;
                        log.SPTier4 = sp4;
                        log.SPTier5 = sp5;
                        log.ProductId = PI;
                        log.CompanyId = companyId;
                        log.BranchId = branchId;
                        log.UserId = userId;
                        log.CreatedBy = Createdby;
                        log.CreatedOn = DateTime.Now;
                        db.PriceList_Log.Add(log);

                        db.SaveChanges();
                        scope.Complete();
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }

                    catch (Exception ex)
                    {
                        scope.Dispose();
                        return Json("Error", JsonRequestBehavior.AllowGet);
                        throw ex;
                    }
                }
            }

        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Transactions;
using System.Globalization;
namespace XenERP.Controllers
{
    public class SaleUSAController : Controller
    {
        private TransactionClasses tc = new TransactionClasses();
        //
        // GET: /SaleUSA/

        [HttpGet]
        public ActionResult DailySale()
        {
            List<SelectListItem> li = new List<SelectListItem>();
            li.Add(new SelectListItem { Text = "Select", Value = "0" });
            li.Add(new SelectListItem { Text = "Cash", Value = "1" });
            li.Add(new SelectListItem { Text = "Credit Card", Value = "2" });
            //ViewData["receipt"] = li;
            ViewBag.receipt = li;
            return View();
        }

        [HttpPost]
        public JsonResult getProductDetails(string query = "")
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                string companyid = Convert.ToString(Session["companyid"]);
                string Branchid = Convert.ToString(Session["BranchId"]);
                var product = db.Products.Where(p => p.Name.Contains(query))//&& p.Code == companyid) && p.BranchCode == Branchid)
                                       .Select(p => new
                                       {
                                           ProductName = p.Name,
                                           Code = p.Code,
                                           Id = p.Id,
                                       }).ToList();
                return Json(product, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult chkProductDetails(string query = "")
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                string companyid = Convert.ToString(Session["companyid"]);
                string Branchid = Convert.ToString(Session["BranchId"]);
                var productDetails = db.Products.Where(p => p.Name.Contains(query))//&& p.Code == companyid) && p.BranchCode == Branchid)
                                       .Select(p => new
                                       {
                                           ProductName = p.Name,
                                           Code = p.Code,
                                           Id = p.Id,
                                     
                                       }).FirstOrDefault();
                return Json(productDetails, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DailySale(FormCollection collection)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        int companyid = Convert.ToInt32(Session["companyid"]);
                        long Branchid = Convert.ToInt64(Session["BranchId"]);
                        int userid = Convert.ToInt32(Session["userid"]);
                        int fid = Convert.ToInt32(Session["fid"]);
                        string Createdby = Convert.ToString(Session["Createdid"]);
                        long createdby = Convert.ToInt64(Session["Createdid"]);

                        string customerName = Convert.ToString(collection["CustomerName"]);
                        string address = Convert.ToString(collection["Address"]);
                        string saleaDate = Convert.ToString(collection["saleDate"]);
                        string receiptMode = Convert.ToString(collection["ReceiptMode"]);
                        int total = Convert.ToInt32(collection["Total"]);
                        int grandTotal = Convert.ToInt32(collection["Total"]);

                        var culture = Session["DateCulture"].ToString();
                        string dateFormat = Session["DateFormat"].ToString();
                        var fdate = DateTime.ParseExact(saleaDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));

                        int countpo = db.Issues.Where(p => p.CompanyId == companyid && p.BranchId == Branchid).Count();
                        salesusa model = new salesusa();
                        {
                            model.ReceiptMode = receiptMode;
                            model.Code = tc.GenerateCode("SAL", countpo);
                            model.CompanyId = companyid;
                            model.BranchId = Branchid;
                            model.UserId = userid;
                            model.FinancialYearId = fid;
                            model.CustomerName = customerName;
                            model.Address = address;
                            model.Date = fdate;
                            model.CreatedOn = DateTime.Now;
                            model.CreatedBy = createdby;
                            model.Total = total;
                            model.GrandTotal = total;
                            db.salesusas.Add(model);
                            db.SaveChanges();

                        }

                        SalesdetailsUSA Imodel = new SalesdetailsUSA();

                        string[] prd = collection["txtItemId"].Split(',');
                        string[] QPU = collection["txtQuantity"].Split(',');
                        string[] AMT = collection["txtAmount"].Split(',');

                        int i = 0;

                        foreach (var prod in prd)
                        {
                            Imodel.SalesUSAId = model.Id;
                            Imodel.ProductId = Convert.ToInt32(prd[i]);
                            Imodel.Quantity = Convert.ToDecimal(QPU[i]);
                            Imodel.Amount = Convert.ToDecimal(AMT[i]);
                            db.SalesdetailsUSAs.Add(Imodel);
                            db.SaveChanges();
                            i++;
                        }
                      
                        scope.Complete();

                        return RedirectToAction("DailySale", new { Msg = "Items has been saved successfully..." });
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        return RedirectToAction("DailySale", new { Err = "Data  not saved successfully...." });
                        throw ex;
                    }
                }
            }
        }



    }
}

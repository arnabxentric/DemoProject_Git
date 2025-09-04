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
    public class IssueController : Controller
    {
        private TransactionClasses tc = new TransactionClasses();
        //
        // GET: /Issue/
        [HttpGet]
        public ActionResult CreateIssue(string Msg, string Err)
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
                var company = db.Companies.Where(d => d.Userid == userid).FirstOrDefault();
                ViewBag.company = db.Companies.Where(d => d.Userid == userid).ToList();
                ViewBag.branch = db.BranchMasters.Where(d => d.UserId == userid && d.CompanyId == company.Id).ToList();
                ViewBag.supplier = db.Suppliers.ToList();
                var result = new IssueModelView();
                return View(result);
            }
        }

        [HttpPost]
        public ActionResult CreateIssue(FormCollection collection)
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

                        int supplierId = Convert.ToInt32(collection["SupplierId"]);
                        string Remarks = Convert.ToString(collection["Remarks"]);
                        string IssueDate = Convert.ToString(collection["IssueDate"]);

                        var culture = Session["DateCulture"].ToString();
                        string dateFormat = Session["DateFormat"].ToString();
                        var fdate = DateTime.ParseExact(IssueDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));

                        int countpo = db.Issues.Where(p => p.CompanyId == companyid && p.BranchId == Branchid).Count();
                        Issue model = new Issue();
                        {
                            model.Code = tc.GenerateCode("ISS", countpo);
                            model.CompanyId = companyid;
                            model.BranchId = Branchid;
                            model.UserId = userid;
                            model.FinancialYearId = fid;
                            model.SupplierId = supplierId;
                            model.Remarks = Remarks;
                            model.IssueDate = fdate;
                            model.CreatedOn = DateTime.Now;
                            model.CreatedBy = Createdby;
                            db.Issues.Add(model);
                            db.SaveChanges();

                        }

                        IssueDetail Imodel = new IssueDetail();

                        string[] prd1 = collection["txtproductid"].Split(',');
                        string[] QPU1 = collection["txtquantityPU"].Split(',');
                        string[] QSU1 = collection["txtquantitySU"].Split(',');
                        string[] QFU1 = collection["txtUnitFormula"].Split(',');
                        string[] WHID = collection["txtWarehouseid"].Split(',');
                        //string[] Type = collection["txtType"].Split(',');


                        int i = 0;

                        foreach (var prod in prd1)
                        {
                            Imodel.IssueId = model.Id;
                            Imodel.ProductId = Convert.ToInt32(prd1[i]);
                            Imodel.QuantityPU = Convert.ToDecimal(QPU1[i]);
                            Imodel.QuantitySU = Convert.ToDecimal(QSU1[i]);
                            Imodel.UnitFormula = Convert.ToDecimal(QFU1[i]);
                            Imodel.WareHouseId = Convert.ToInt32(WHID[i]);
                            db.IssueDetails.Add(Imodel);
                            db.SaveChanges();
                            i++;
                        }

                        Stock Smodel = new Stock();
                        int j = 0;
                        foreach (var prod in prd1)
                        {
                            Smodel.ArticleID = Convert.ToInt32(prd1[j]);
                            Smodel.BranchId = Branchid;
                            Smodel.CompanyId = companyid;
                            Smodel.fYearID = fid;
                            Smodel.UserId = userid;
                            Smodel.TranDate = fdate;
                            Smodel.TranCode = "OUT";
                            Smodel.TranId = model.Id;
                            Smodel.Items = Convert.ToDecimal(QPU1[j]);
                            Smodel.TransTag = "ISS";
                            Smodel.WarehouseId = Convert.ToInt32(WHID[j]);
                            Smodel.CreatedBy = createdby;
                            db.Stocks.Add(Smodel);
                            db.SaveChanges();
                            j++;
                        }
                        scope.Complete();

                        return RedirectToAction("ShowAllIssue", new { Msg = "Items has been saved successfully..." });
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        return RedirectToAction("ShowAllIssue", new { Err = "Data  not saved successfully...." });
                        throw ex;
                    }
                }

            }
        }

        [HttpPost]
        public JsonResult getSupplierDetails(string query = "")
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                string companyid = Convert.ToString(Session["companyid"]);
                string Branchid = Convert.ToString(Session["BranchId"]);
                var supplier = db.Suppliers.Where(p => p.Name.Contains(query))//&& p.Code == companyid) && p.BranchCode == Branchid)
                                       .Select(p => new
                                       {
                                           SupplierName = p.Name,
                                           Code = p.Code,
                                           Id = p.Id,
                                       }).ToList();
                return Json(supplier, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult chkSupplierDetails(string query = "")
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                string companyid = Convert.ToString(Session["companyid"]);
                string Branchid = Convert.ToString(Session["BranchId"]);
                var supplierDetails = db.Suppliers.Where(p => p.Name.Contains(query))//&& p.Code == companyid) && p.BranchCode == Branchid)
                                       .Select(p => new
                                       {
                                           SupplierName = p.Name,
                                           Code = p.Code,
                                           Id = p.Id,
                                       }).FirstOrDefault();
                return Json(supplierDetails, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult chkSupplierDetailsCode(string query = "")
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                string companyid = Convert.ToString(Session["companyid"]);
                string Branchid = Convert.ToString(Session["BranchId"]);
                var supplierDetails = db.Suppliers.Where(p => p.Code.Contains(query))//&& p.Code == companyid) && p.BranchCode == Branchid)
                                       .Select(p => new
                                       {
                                           SupplierName = p.Name,
                                           Code = p.Code,
                                           Id = p.Id,
                                       }).FirstOrDefault();
                return Json(supplierDetails, JsonRequestBehavior.AllowGet);
            }
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

        public JsonResult getStockDetails(string query = "", long? warId = null)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
               
                    var companyid = Convert.ToInt64(Session["companyid"]);
                    var Branchid = Convert.ToInt64(Session["BranchId"]);
                    var id = db.Products.Where(p => p.Name.Contains(query)).Select(s => s.Id).FirstOrDefault();
                    var warehouseId = db.Stocks.Where(p => p.ArticleID == id).Select(s => s.WarehouseId).FirstOrDefault();
                    var warehouseName = db.Warehouses.Where(p => p.Id == warId).Select(s => s.Name).FirstOrDefault();
                    if (warId == null)
                    {
                        warId = warehouseId;
                        warehouseName = db.Warehouses.Where(p => p.Id == warId).Select(s => s.Name).FirstOrDefault();
                    }
                    var stocks = db.Stocks.Where(p => p.ArticleID == id && p.CompanyId == companyid && p.BranchId == Branchid && p.WarehouseId == warId).ToList();
                    var instock = stocks.Where(p => p.TranCode == "IN").Sum(s => (decimal?)s.Items) ?? 0;
                    var outstock = stocks.Where(p => p.TranCode == "OUT").Sum(s => (decimal?)s.Items) ?? 0;
                    var available = instock - outstock;
                    var stock = db.Products.Where(p => p.Id == id)
                                           .Select(p => new
                                           {
                                               productId = id,
                                               UnitFormula = p.UnitFormula,
                                               PrimaryUnit = p.UOM.Description,
                                               Secondaryunit = p.UOM1.Description,
                                               availability = available,
                                               warehouseName = warehouseName,
                                               warehouseId = p.Stocks.FirstOrDefault().WarehouseId,
                                           }).FirstOrDefault();
              
                return Json(stock, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost]
        public JsonResult getWarehouseDetails(string query = "")
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                long companyid = Convert.ToInt64(Session["companyid"]);
                string Branchid = Convert.ToString(Session["BranchId"]);
                var wareHouse = db.Warehouses.Where(p => p.Name.Contains(query) && p.Companyid == companyid) //&& p.BranchCode == Branchid)
                                       .Select(p => new
                                       {
                                           WareHouseName = p.Name,
                                           // Code = p.Code,
                                           Id = p.Id,
                                       }).ToList();
                return Json(wareHouse, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult chkWareHouseDetails(string query = "")
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                long companyid = Convert.ToInt64(Session["companyid"]);
                string Branchid = Convert.ToString(Session["BranchId"]);
                var wareHouseDetails = db.Warehouses.Where(p => p.Name.Contains(query) && p.Companyid == companyid)// && p.BranchCode == Branchid)
                                       .Select(p => new
                                       {
                                           WareHouseName = p.Name,
                                           //Code = p.Code,
                                           Id = p.Id,
                                       }).FirstOrDefault();
                return Json(wareHouseDetails, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult ShowAllIssue(string Msg, string Err)
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
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var issue = db.Issues.Where(d => d.UserId == userid && d.CompanyId == companyid && d.IsDeleted == null).ToList();
                List<IssueModelView> model = new List<IssueModelView>();
                foreach (var item in issue)
                {
                    model.Add(new IssueModelView()
                    {
                        SupplierCode = db.Suppliers.Where(r => r.Id == item.SupplierId).Select(s => s.Code).FirstOrDefault(),
                        SupplierName = db.Suppliers.Where(r => r.Id == item.SupplierId).Select(s => s.Name).FirstOrDefault(),
                        IssueDate = item.IssueDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture)),
                        Id = item.Id,
                    });
                }
                return View(model);
            }

        }

        [HttpGet]
        public ActionResult EditIssue(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int userid = Convert.ToInt32(Session["userid"]);
                int companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                List<IssueModelView> model = new List<IssueModelView>();
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                var issue = db.Issues.Where(d => d.UserId == userid && d.CompanyId == companyid && d.IsDeleted == null && d.Id == id).ToList();
                foreach (var item in issue)
                {
                    model.Add(new IssueModelView()
                    {
                        SupplierCode = db.Suppliers.Where(r => r.Id == item.SupplierId).Select(s => s.Code).FirstOrDefault(),
                        SupplierName = db.Suppliers.Where(r => r.Id == item.SupplierId).Select(s => s.Name).FirstOrDefault(),
                        Remarks = item.Remarks,
                        // IssueDate = item.IssueDate,
                        IssueDate = item.IssueDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture)),
                        Id = item.Id,
                        issueDetails = getIssueDetails(id),
                        SupplierId = item.SupplierId,

                    });
                }


                return View(model);

            }
        }

        [HttpPost]
        public ActionResult EditIssue(FormCollection collection)
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
                        int supplierId = Convert.ToInt32(collection["SupplierId"]);
                        string Remarks = Convert.ToString(collection["Remarks"]);
                        string IssueDate = Convert.ToString(collection["IssueDate"]);
                        int id = Convert.ToInt32(collection["Id"]);
                        //int productId=Convert.ToInt32(collection["txtproductid"]);
                        var culture = Session["DateCulture"].ToString();
                        string dateFormat = Session["DateFormat"].ToString();
                        var fdate = DateTime.ParseExact(IssueDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));

                        var issue = db.Issues.Where(d => d.UserId == userid && d.CompanyId == companyid && d.IsDeleted == null && d.Id == id).ToList();
                        foreach (var item in issue)
                        {
                            db.Issues.Remove(item);
                            db.SaveChanges();
                        }

                        var issueDetail = db.IssueDetails.Where(r => r.IssueId == id).ToList();
                        foreach (var item in issueDetail)
                        {
                            db.IssueDetails.Remove(item);
                            db.SaveChanges();
                        }

                        var stockDetail = db.Stocks.Where(r => r.TransTag == "ISS" && r.TranId == id).ToList();
                        foreach (var item in stockDetail)
                        {
                            db.Stocks.Remove(item);
                            db.SaveChanges();
                        }

                        int countpo = db.Issues.Where(p => p.CompanyId == companyid && p.BranchId == Branchid).Count();
                        Issue model = new Issue();
                        {
                            model.Code = tc.GenerateCode("ISS", countpo);
                            model.CompanyId = companyid;
                            model.BranchId = Branchid;
                            model.UserId = userid;
                            model.FinancialYearId = fid;
                            model.SupplierId = supplierId;
                            model.Remarks = Remarks;
                            model.IssueDate = fdate;
                            model.CreatedOn = DateTime.Now;
                            model.CreatedBy = Createdby;
                            db.Issues.Add(model);
                            db.SaveChanges();

                        }

                        IssueDetail Imodel = new IssueDetail();

                        string[] prd1 = collection["txtproductid"].Split(',');
                        string[] QPU1 = collection["txtquantityPU"].Split(',');
                        string[] QSU1 = collection["txtquantitySU"].Split(',');
                        string[] QFU1 = collection["txtUnitFormula"].Split(',');
                        string[] WHID = collection["txtWarehouseid"].Split(',');
                        //string[] Type = collection["txtType"].Split(',');


                        int i = 0;

                        foreach (var prod in prd1)
                        {
                            Imodel.IssueId = model.Id;
                            Imodel.ProductId = Convert.ToInt32(prd1[i]);
                            Imodel.QuantityPU = Convert.ToDecimal(QPU1[i]);
                            Imodel.QuantitySU = Convert.ToDecimal(QSU1[i]);
                            Imodel.UnitFormula = Convert.ToDecimal(QFU1[i]);
                            Imodel.WareHouseId = Convert.ToInt32(WHID[i]);
                            db.IssueDetails.Add(Imodel);
                            db.SaveChanges();
                            i++;
                        }

                        Stock Smodel = new Stock();
                        int j = 0;
                        foreach (var prod in prd1)
                        {
                            Smodel.ArticleID = Convert.ToInt32(prd1[j]);
                            Smodel.BranchId = Branchid;
                            Smodel.CompanyId = companyid;
                            Smodel.fYearID = fid;
                            Smodel.UserId = userid;
                            Smodel.TranDate = fdate;
                            Smodel.TranCode = "OUT";
                            Smodel.TranId = model.Id;
                            Smodel.Items = Convert.ToDecimal(QPU1[j]);
                            Smodel.TransTag = "ISS";
                            Smodel.WarehouseId = Convert.ToInt32(WHID[j]);
                            Smodel.CreatedBy = createdby;
                            db.Stocks.Add(Smodel);
                            db.SaveChanges();
                            j++;
                        }
                        scope.Complete();

                        return RedirectToAction("ShowAllIssue", new { Msg = "Items has been saved successfully..." });
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        return RedirectToAction("ShowAllIssue", new { Err = "Data  not saved successfully...." });
                        throw ex;
                    }
                }

            }
        }

        private List<IssueDetails> getIssueDetails(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                List<IssueDetails> issueModel = new List<IssueDetails>();
                int userid = Convert.ToInt32(Session["userid"]);
                int companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                var issueDetail = db.IssueDetails.Where(r => r.IssueId == id).ToList();
                foreach (var item in issueDetail)
                {
                    var stocks = db.Stocks.Where(p => p.ArticleID == item.ProductId && p.CompanyId == companyid && p.BranchId == Branchid).ToList();
                    var instock = stocks.Where(p => p.TranCode == "IN").Sum(s => (decimal?)s.Items) ?? 0;
                    var outstock = stocks.Where(p => p.TranCode == "OUT").Sum(s => (decimal?)s.Items) ?? 0;
                    var available = instock - outstock;
                    issueModel.Add(new IssueDetails
                    {
                        WareHouseId = item.WareHouseId,
                        ProductId = item.ProductId,
                        ProductName = db.Products.Where(r => r.Id == item.ProductId).Select(s => s.Name).FirstOrDefault(),
                        WarehouseName = db.Warehouses.Where(r => r.Id == item.WareHouseId).Select(s => s.Name).FirstOrDefault(),
                        AvailableItem = available + item.QuantityPU ?? 0,
                        UnitFormula = item.UnitFormula,
                        QuantityPU = item.QuantityPU,
                        QuantitySU = item.QuantitySU,
                        // primaryUnit = db.UOMs.Where(r => r.Id == db.Products.Where(q => q.Id == item.ProductId).Select(s => s.UnitId).FirstOrDefault()).Select(u => u.Description).FirstOrDefault(),
                        primaryUnit = db.Products.Where(r => r.Id == item.ProductId).FirstOrDefault().UOM.Description,
                        secondaryUnit = db.Products.Where(r => r.Id == item.ProductId).FirstOrDefault().UOM1.Description,
                    });
                }
                return issueModel;
            }
        }

        [HttpGet]
        public ActionResult DeleteIssue(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                try
                {
                    var result = db.Issues.Where(d => d.Id == id).FirstOrDefault();
                    result.IsDeleted = true;
                    //db.SaveChanges();

                    var details = db.IssueDetails.Where(r => r.IssueId == id).ToList();
                    foreach (var item in details)
                    {
                        item.IsDeleted = true;
                        // db.SaveChanges();
                    }
                    var stockDetail = db.Stocks.Where(r => r.TransTag == "ISS" && r.TranId == id).ToList();
                    foreach (var item in stockDetail)
                    {
                        db.Stocks.Remove(item);

                    }
                    db.SaveChanges();

                    return RedirectToAction("ShowAllIssue", new { Msg = "Row Deleted Successfully...." });
                }
                catch
                {
                    return RedirectToAction("ShowAllIssue", new { Err = "Row cannot be deleted...." });
                }
            }
        }

    }
}

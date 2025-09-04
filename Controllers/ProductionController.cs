using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Transactions;

namespace XenERP.Controllers
{
    public class ProductionController : Controller
    {
        //
        // GET: /Production/
        [HttpGet]
        public ActionResult ProductionDetails(string Msg, string Err)
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
                ViewBag.Product = db.Products.ToList();
                var result = new ProductionModelView();
                return View(result);
            }

        }

        [HttpPost]
        public ActionResult ProductionDetails(FormCollection collection)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                try
                {

                    int companyid = Convert.ToInt32(Session["companyid"]);
                    long Branchid = Convert.ToInt64(Session["BranchId"]);
                    int userid = Convert.ToInt32(Session["userid"]);


                    int prd = Convert.ToInt32(collection["ProductId"]);
                    int QPU = Convert.ToInt32(collection["parentname"]);
                    int QSU = Convert.ToInt32(collection["secondname"]);
                    int QFU = Convert.ToInt32(collection["UnitFormula"]);
                    int PID = Convert.ToInt32(collection["PlantId"]);


                    Production model = new Production();
                    {
                        model.ProductId = prd;
                        model.QuantityInPU = QPU;
                        model.OuantityInSU = QSU;
                        model.PlantId = PID;
                        model.UnitForm = QFU;
                        model.CompanyId = companyid;
                        model.BranchId = Branchid;
                        model.UserId = userid;
                        db.Productions.Add(model);
                        db.SaveChanges();
                    }


                    ProductionDetail pmodel = new ProductionDetail();

                    string[] prd1 = collection["txtproductid"].Split(',');
                    string[] QPU1 = collection["txtquantityPU"].Split(',');
                    string[] QSU1 = collection["txtquantitySU"].Split(',');
                    string[] QFU1 = collection["txtUnitFormula"].Split(',');

                    string[] Type = collection["txtType"].Split(',');


                    int i = 0;

                    foreach (var prod in prd1)
                    {
                        pmodel.ProductionId = model.Id;
                        pmodel.ProductId = Convert.ToInt32(prd1[i]);
                        pmodel.QuantityPU = Convert.ToInt32(QPU1[i]);
                        pmodel.QuantitySU = Convert.ToInt32(QSU1[i]);
                        pmodel.UnitFormula = Convert.ToInt32(QFU1[i]);
                        //pmodel.Type = Convert.ToInt32(Type[i]);
                        db.ProductionDetails.Add(pmodel);
                        db.SaveChanges();
                        i++;

                    }

                    return RedirectToAction("ProductionDetails", new { Msg = "Items has been transfer successfully..." });
                }

                catch
                {


                    return RedirectToAction("ProductionDetails", new { Err = "Data  not saved successfully...." });
                }
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
                                           PrimaryUnit = p.UOM.Description,
                                           Secondaryunit = p.UOM1.Description,
                                           UnitFormula = p.UnitFormula,
                                       }).FirstOrDefault();
                return Json(productDetails, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult getPlantDetails(string query = "")
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                string companyid = Convert.ToString(Session["companyid"]);
                string Branchid = Convert.ToString(Session["BranchId"]);
                var plant = db.Plants.Where(p => p.Name.Contains(query))//&& p.Code == companyid) && p.BranchCode == Branchid)
                                       .Select(p => new
                                       {
                                           PlantName = p.Name,
                                           plantCode = p.Code,
                                           Id = p.Id,
                                       }).ToList();
                return Json(plant, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult chkPlantDetails(string query = "")
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                string companyid = Convert.ToString(Session["companyid"]);
                string Branchid = Convert.ToString(Session["BranchId"]);
                var productDetails = db.Plants.Where(p => p.Name.Contains(query))//&& p.Code == companyid) && p.BranchCode == Branchid)
                                       .Select(p => new
                                       {
                                           plantName = p.Name,
                                           id = p.Id,
                                           managerName = p.Employee.FirstName + " " + p.Employee.LastName,
                                       }).FirstOrDefault();
                return Json(productDetails, JsonRequestBehavior.AllowGet);
            }
        }
    }
}

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
    public class PlantController : Controller
    {
        //
        // GET: /Plant/
        [HttpGet]
        public ActionResult CreatePlant()
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int userid = Convert.ToInt32(Session["userid"]);
                var company = db.Companies.Where(d => d.Userid == userid).FirstOrDefault();
                ViewBag.company = db.Companies.Where(d => d.Userid == userid).ToList();
                ViewBag.branch = db.BranchMasters.Where(d => d.UserId == userid && d.CompanyId == company.Id).ToList();
                ViewBag.countrycode = db.Countries.ToList().OrderBy(d => d.Country1);
                ViewBag.Country = db.Countries.ToList();
                return View();
            }
        }

        [HttpPost]
        public ActionResult CreatePlant(CreatePlantModelView plant)
        {
            string Createdby = Convert.ToString(Session["Createdid"]);
            int userId = Convert.ToInt32(Session["userid"]);
            int branchId = Convert.ToInt32(Session["BranchId"]);
            int companyId = Convert.ToInt32(Session["companyid"]);
            using (InventoryEntities db = new InventoryEntities())
            {
                using (TransactionScope scope=new TransactionScope())
                {
                    try
                    {
                        Plant model = new Plant();
                        {
                            model.Code = plant.Code;
                            model.Name = plant.Name;
                            model.Description = plant.Description;
                            model.ContactName = plant.ManagerName;
                            model.EmployeeId = plant.EmployeeId;
                            model.MobileNumber = plant.MobileNumber;
                            model.Address = plant.Address;
                            model.Town = plant.Town;
                            model.State = plant.State;
                            model.CountryId = plant.CountryId;
                            model.PIN = plant.PIN;
                            model.Userid = userId;
                            model.Branchid = branchId;
                            model.Companyid = companyId;
                            model.CreatedBy = Createdby;
                            model.CreatedOn = DateTime.Now;
                            db.Plants.Add(model);
                            db.SaveChanges();
                            scope.Complete();
                            return RedirectToAction("ShowAllPlant", new { Msg = "Plant Created Successfully....." });
                        }
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        return RedirectToAction("ShowAllPlant", new { Err = "Data  not saved successfully...." });
                        throw ex;
                    }
                }
            }
        }

        [HttpGet]
        public ActionResult ShowAllPlant(string Msg, string Err)
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
                var plant = db.Plants.Where(d => d.Userid == userid && d.Companyid == companyid && d.IsDeleted==null).ToList();
                return View(plant);
            }
        }

        [HttpGet]
        public ActionResult EditPlant( int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                CreatePlantModelView model = new CreatePlantModelView();
                var result = db.Plants.Find(id);
                model.Name = result.Name;
                model.EmployeeCode = result.Employee.Code;
                model.Code=result.Code;
                model.Description=result.Description;
                model.ManagerName=result.ContactName;
                model.MobileNumber = result.MobileNumber;
                model.Address = result.Address;
                model.Town = result.Town;
                model.State = result.State;
                model.CountryId = result.CountryId;
                model.PIN = result.PIN;
                int countryid = Convert.ToInt32(result.CountryId);
                ViewBag.Country = new SelectList(db.Countries, "CountryId", "Country1", countryid);
                int userid = Convert.ToInt32(Session["userid"]);
                var company = db.Companies.Where(d => d.Userid == userid).FirstOrDefault();
                ViewBag.company = new SelectList(db.Companies, "Id", "Name", model.Companyid);
                ViewBag.branch = new SelectList(db.BranchMasters, "Id", "Name", model.Branchid);
                ViewBag.countrycode = db.CountryCodes.ToList().OrderBy(d => d.Country);
                ViewBag.Country = db.Countries.ToList();
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult EditPlant(CreatePlantModelView model)
        {
            string Createdby = Convert.ToString(Session["Createdid"]);
            int userId = Convert.ToInt32(Session["userid"]);
            int branchId = Convert.ToInt32(Session["BranchId"]);
            int companyId = Convert.ToInt32(Session["companyid"]);
            using (InventoryEntities db = new InventoryEntities())
            {
                try
                {
                    var result = db.Plants.Where(d => d.Id == model.Id && d.IsDeleted==null).FirstOrDefault();
                    result.ModifiedOn = DateTime.Now;
                    result.ModifiedBy = Createdby;
                    result.Userid = userId;
                    result.Branchid = branchId;
                    result.Companyid = companyId;
                    result.Address = model.Address;
                    result.Code = model.Code;
                    result.CountryId = model.CountryId;
                    result.Description = model.Description;
                    result.EmployeeId = model.EmployeeId;
                    result.ContactName = model.ManagerName;
                    result.MobileNumber = model.MobileNumber;
                    result.Town = model.Town;
                    result.State = model.State;
                    result.PIN = model.PIN;
                    result.Name = model.Name;
                    db.SaveChanges();
                    return RedirectToAction("ShowAllPlant", new { Msg = "Data Updated Successfully...." });
                }
                catch
                {
                    return RedirectToAction("ShowAllPlant", new { Err = "Data  not saved successfully...." });
                }
            }
        }

        [HttpGet]
        public ActionResult DeletePlant(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                try
                {
                    var result = db.Plants.Where(d => d.Id == id).FirstOrDefault();
                    result.IsDeleted = 1;
                    db.SaveChanges();
                    return RedirectToAction("ShowAllPlant", new { Msg = "Row Deleted Successfully...." });
                }
                catch
                {
                    return RedirectToAction("ShowAllPlant", new { Err = "Row cannot be deleted...." });
                }
            }
        }

        [HttpGet]
        public ActionResult PlantDetails(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                var plant = db.Plants.SingleOrDefault(d => d.Id == id);
                return View(plant);
            }
        }

    }
}

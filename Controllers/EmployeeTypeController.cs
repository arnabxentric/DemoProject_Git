using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Transactions;
namespace XenERP.Controllers
{
    public class EmployeeTypeController : Controller
    {
        //
        // GET: /EmployeeType/
        public ActionResult CreateEmployeeType()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateEmployeeType(EmployeeType model)
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
                        model.CompanyId = companyId;
                        model.BranchId = branchId;
                        model.UserId = userId;
                        model.CreatedOn = DateTime.Now;
                        db.EmployeeTypes.Add(model);
                        db.SaveChanges();
                        scope.Complete();
                        return RedirectToAction("ShowAllEmployeeTypeList", new { Msg = "Designation Created Successfully....." });
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        return RedirectToAction("ShowAllEmployeeTypeList", new { Msg = "Designation Creation Failes....." });
                        throw ex;
                    }
                }
            }

        }

        [HttpGet]
        public ActionResult ShowAllEmployeeTypeList(string Msg, string Err)
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
                var empType = db.EmployeeTypes.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
                return View(empType);
            }
        }

        [HttpGet]
        public ActionResult EditEmployeeType(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int userId = Convert.ToInt32(Session["userid"]);
                int branchId = Convert.ToInt32(Session["BranchId"]);
                int companyId = Convert.ToInt32(Session["companyid"]);
                var result = db.EmployeeTypes.Find(id);
                return View(result);
            }
        }

        [HttpPost]
        public ActionResult EditEmployeeType(EmployeeType model)
        {
            string Createdby = Convert.ToString(Session["Createdid"]);
            int userId = Convert.ToInt32(Session["userid"]);
            int branchId = Convert.ToInt32(Session["BranchId"]);
            int companyId = Convert.ToInt32(Session["companyid"]);
            using (InventoryEntities db = new InventoryEntities())
            {
                try
                {
                    var result = db.EmployeeTypes.Find(model.Id);
                    model.CompanyId = companyId;
                    model.UserId = userId;
                    model.BranchId = branchId;
                    model.ModifiedOn = DateTime.Now;
                    db.Entry(result).CurrentValues.SetValues(model);
                    db.SaveChanges();

                    return RedirectToAction("ShowAllEmployeeTypeList", new { Msg = "Data Updated Successfully...." });
                }
                catch
                {
                    return RedirectToAction("ShowAllEmployeeTypeList", new { Err = "Data  not saved successfully...." });
                }
            }
        }

    }
}

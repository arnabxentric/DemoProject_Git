using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Transactions;

namespace XenERP.Controllers
{
    public class DepartmentController : Controller
    {
        //
        // GET: /Department/

        [HttpGet]
        public ActionResult CreateDepartment()
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int userid = Convert.ToInt32(Session["userid"]);
                int companyid = Convert.ToInt32(Session["companyid"]);
                string Branchid = Convert.ToString(Session["BranchId"]);
                string companyName = db.Companies.Where(r => r.Id == companyid).Select(s => s.Name).FirstOrDefault();
                ViewBag.branch = db.BranchMasters.Where(r => r.CompanyId == companyid).ToList();
                DepartmentModelView model = new DepartmentModelView();
                {
                    model.CompanyName = companyName;
                }
                return View(model);
            }
        }


        [HttpPost]
        public ActionResult CreateDepartment(DepartmentModelView model)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        string Createdby = Convert.ToString(Session["Createdid"]);
                        Department department = new Department();
                        {
                            department.Name = model.Name;
                            department.Code = model.Code;
                            department.BranchId = model.BranchId;
                            department.CompanyId = Convert.ToInt32(Session["companyid"]);
                            department.UserId = Convert.ToInt32(Session["userid"]);
                            department.CreatedOn = DateTime.Now;
                            department.CreatedBy = Createdby;
                            department.Head = model.Head;
                            department.Status = true;
                            department.PhoneNo = model.PhoneNo;
                            db.Departments.Add(department);
                            db.SaveChanges();
                            scope.Complete();
                            return RedirectToAction("ShowAllDepartment", new { Msg = "Data Saved Successfully...." });
                        }
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        return RedirectToAction("ShowAllDepartment", new { Err = "Data  not saved successfully...." });
                        throw ex;
                    }
                }

            }
        }

        [HttpGet]
        public ActionResult ShowAllDepartment(string Msg, string Err)
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
                var department = db.Departments.Where(d => d.UserId == userid && d.CompanyId == companyid && d.IsDeleted==null).ToList();
                List<DepartmentModelView> model = new List<DepartmentModelView>();
                foreach (var item in department)
                {
                    model.Add(new DepartmentModelView()
                        {
                            Name = item.Name,
                            Code=item.Code,
                            Id=item.Id
                            //BranchName=item.BranchMaster.Name
                            //BranchName=item.BranchMaster.Name
                        });

                }



                return View(model);
            }
        }

        [HttpGet]
        public ActionResult EditDepartment(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int userid = Convert.ToInt32(Session["userid"]);
                int companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                ViewBag.branch = db.BranchMasters.Where(r => r.CompanyId == companyid).ToList();
                string companyName = db.Companies.Where(r => r.Id == companyid).Select(s => s.Name).FirstOrDefault();
                DepartmentModelView model = new DepartmentModelView();
                var result = db.Departments.Find(id);
                model.Name = result.Name;
                model.Head = result.Head;
                model.Code = result.Code;
                model.PhoneNo = result.PhoneNo;
                model.CompanyName = companyName;
                model.BranchId = result.BranchId;
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult EditDepartment(DepartmentModelView model)
        {
            int userId = Convert.ToInt32(Session["userid"]);
            int branchId = Convert.ToInt32(Session["BranchId"]);
            int companyId = Convert.ToInt32(Session["companyid"]);
            string Createdby = Convert.ToString(Session["Createdid"]);
            using (InventoryEntities db = new InventoryEntities())
            {
                try
                {
                    var result = db.Departments.Where(d => d.Id == model.Id).FirstOrDefault();
                    result.ModifiedOn = DateTime.Now;
                    result.ModifiedBy = Createdby;
                    result.UserId = userId;
                    result.CompanyId = companyId;
                    result.Code = model.Code;
                    result.Name = model.Name;
                    result.BranchId = model.BranchId;
                    db.SaveChanges();
                    return RedirectToAction("ShowAllDepartment", new { Msg = "Data Updated Successfully...." });
                }
                catch
                {
                    return RedirectToAction("ShowAllDepartment", new { Err = "Data  not saved successfully...." });
                }
            }
        }

        [HttpGet]
        public ActionResult DeleteDepartment(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                try
                {
                    var result = db.Departments.Where(d => d.Id == id).FirstOrDefault();
                    result.IsDeleted = true;
                    db.SaveChanges();
                    return RedirectToAction("ShowAllDepartment", new { Msg = "Row Deleted Successfully...." });
                }
                catch
                {
                    return RedirectToAction("ShowAllDepartment", new { Err = "Row cannot be deleted...." });
                }
            }
        }

        [HttpGet]
        public ActionResult DepartmentDetails(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                var department = db.Departments.SingleOrDefault(d => d.Id == id);
                return View(department);
            }
        }

    }
}

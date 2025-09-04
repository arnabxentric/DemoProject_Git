using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Transactions;
using System.Data;
using System.Data.Entity.Validation;
using XenERP.Models.Repository;

namespace XenERP.Controllers
{
     [SessionExpire]
    public class BranchController : Controller
    {
        //
        // GET: /Branch/
        InventoryEntities db = new InventoryEntities();

        TaxRepository taxobj = new TaxRepository();
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult ShowAllBranch(string Msg, string Err)
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


            int userid = Convert.ToInt32(Session["userid"]);

            var result = db.BranchMasters.Where(d => d.CompanyId == companyid && d.UserId==userid).ToList();
            return View(result);
        }

        [HttpGet]
        public ActionResult CreateBranch()
        {



            int companyid = Convert.ToInt32(Session["companyid"]);


            ViewBag.company = db.Companies.Where(d => d.Id == companyid).ToList();
            ViewBag.Country = db.Countries.ToList();
            return View();
        }



        [HttpPost]
        public ActionResult CreateBranch(BranchMaster branch)
        {

           // int companyidd = Convert.ToInt32(Session["companyid"]);

            //int Branchidd = Convert.ToInt32(Session["BranchId"]);

            int companyid = Convert.ToInt32(Session["companyid"]);

            var fYear = db.FinancialYearMasters.Where(u => u.status == "Active" && u.CompanyId == companyid).FirstOrDefault();

            int fYearid = fYear.fYearID;

            try
            {
                int Createdby = Convert.ToInt32(Session["Createdid"]);
                int Branchid = Convert.ToInt32(Session["BranchId"]);
                int userid = Convert.ToInt32(Session["userid"]);


                branch.BranchId = Branchid;
                branch.CompanyId = companyid;
                branch.UserId = userid;
                branch.CreatedBy = Createdby;
                branch.CreatedDate = DateTime.Now;
                db.BranchMasters.Add(branch);
                db.SaveChanges();

            //    int branchidd = Convert.ToInt32(taxobj.InsertBranchid(branch));

         //       db.InsertLedgerBank(fYearid, companyid, branchidd, userid);
     

                return RedirectToAction("ShowAllBranch", new { Msg = "Data Save Successfully...." });
            }
            catch
            {


                return RedirectToAction("ShowAllBranch", new { Err = "Data cannot be Saved...." });
            }
        }


        public JsonResult CheckBranchCode(string Id, string Code)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            int id = 0;
            string codes = Code;
            string trimcode = codes.Trim();

            if (String.IsNullOrEmpty(Id) || String.IsNullOrWhiteSpace(Id))
            {

                bool Iscustomer = db.BranchMasters.Any(d => d.Code == trimcode && d.CompanyId == companyid && d.BranchId == Branchid);
                return Json(!Iscustomer, JsonRequestBehavior.AllowGet);
            }
            else
            {
                id = Convert.ToInt32(Id);
                bool Iscustomer = db.BranchMasters.Where(d => d.Id != id).Any(d => d.Code == trimcode && d.CompanyId == companyid && d.BranchId == Branchid);
                return Json(Iscustomer, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult EditBranch(int id)
        {


            int companyid = Convert.ToInt32(Session["companyid"]);




            var branch = db.BranchMasters.Where(d => d.Id == id).FirstOrDefault();

            ViewBag.company = db.Companies.Where(d => d.Id == companyid).ToList();
            ViewBag.Country = db.Countries.ToList();

            //ViewBag.company = new SelectList(db.Companies, "Id", "Name", branch.CompanyId);
            //ViewBag.Countryedit = new SelectList(db.Countries, "CountryId", "Country1", branch.Country);


            return View(branch);
        }





        [HttpPost]
        public ActionResult EditBranch(BranchMaster branch)
        {
            try
            {

                int Createdby = Convert.ToInt32(Session["Createdid"]);
                var details = db.BranchMasters.Where(d => d.Id == branch.Id).FirstOrDefault();

                details.ModifiedBy = Createdby;
                details.ModifiedOn = DateTime.Now;

                db.Entry(details).CurrentValues.SetValues(branch);
                db.SaveChanges();

                return RedirectToAction("ShowAllBranch", new { Msg = "Data Updated Successfully...." });
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
                return RedirectToAction("ShowAllBranch", new { Err = "Branch details  not  saved successfully.." });

            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Data  not Saved Successfully.......";
                return RedirectToAction("ShowAllBranch", new { Err = "Branch details  not  saved successfully.." });

            }
            catch (Exception exp)
            {
                return RedirectToAction("ShowAllBranch", new { Err = "Branch details  not  saved successfully.." });

            }



        }











        [HttpGet]
        public ActionResult DeleteBranch(BranchMaster branch)
        {
            try
            {

                var details = db.BranchMasters.Where(d => d.Id == branch.Id).FirstOrDefault();


                db.BranchMasters.Remove(details);

                db.SaveChanges();

                return RedirectToAction("ShowAllBranch", new { Msg = "Row Deleted Successfully...." });
            }
            catch
            {


                return RedirectToAction("ShowAllBranch", new { Err = "Row Cannot be Deleted...." });
            }
        }



    }
}

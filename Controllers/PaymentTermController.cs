using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Transactions;

namespace XenERP.Controllers
{
    public class PaymentTermController : Controller
    {
        //
        // GET: /PaymentTerm/
        [HttpGet]
        public ActionResult CreatePaymentTerm()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreatePaymentTerm(PaymentTerm model)
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
                        db.PaymentTerms.Add(model);
                        db.SaveChanges();
                        scope.Complete();
                        return RedirectToAction("ShowAllPaymentTerm", new { Msg = "Payment Term Created Successfully....." });
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        return RedirectToAction("ShowAllPaymentTerm", new { Msg = "Payment Term Creation Failes....." });
                        throw ex;
                    }
                }
            }
         
        }

        [HttpGet]
        public ActionResult ShowAllPaymentTerm(string Msg, string Err)
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
                var pay = db.PaymentTerms.Where(d => d.UserId == userid && d.CompanyId == companyid && d.IsDeleted == false).ToList();
                return View(pay);
            }
        }

        public ActionResult DeletePayTerm(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                try
                {
                    var result = db.PaymentTerms.Where(d => d.Id == id).FirstOrDefault();
                    result.IsDeleted = true;
                    db.SaveChanges();
                    return RedirectToAction("ShowAllPaymentTerm", new { Msg = "Row Deleted Successfully...." });
                }
                catch
                {
                    return RedirectToAction("ShowAllPaymentTerm", new { Err = "Row cannot be deleted...." });
                }
            }
        }

    }
}

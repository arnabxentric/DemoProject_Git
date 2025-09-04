using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;

namespace XenERP.Controllers
{
    [SessionExpire]
    public class PrefixController : Controller
    {
        private InventoryEntities db = new InventoryEntities();
        //
        // GET: /Prefix/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult SetPrefix()
        {
            
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);

            var checkPrefix = db.Prefixes.Where(p => p.UserId == userid && p.CompanyId == companyid && p.BranchId == Branchid && !(p.Name== "Receive Voucher" || p.Name == "Payment Voucher")).Count();
            if (checkPrefix == 0)
            {
                var customer = new Prefix() { Name = "Customer", DefaultPrefix = "CUS", UserId = userid, CompanyId = companyid, BranchId = Branchid };
                db.Prefixes.Add(customer);

                var supplier = new Prefix() { Name = "Supplier", DefaultPrefix = "SUP", UserId = userid, CompanyId = companyid, BranchId = Branchid };
                db.Prefixes.Add(supplier);

                var po = new Prefix() { Name = "Purchase Order", DefaultPrefix = "PO", UserId = userid, CompanyId = companyid, BranchId = Branchid };
                db.Prefixes.Add(po);

                var pc = new Prefix() { Name = "Purchase Receipt/Challan", DefaultPrefix = "PC", UserId = userid, CompanyId = companyid, BranchId = Branchid };
                db.Prefixes.Add(pc);

                var pr = new Prefix() { Name = "Purchase Return/Debit Note", DefaultPrefix = "PR", UserId = userid, CompanyId = companyid, BranchId = Branchid };
                db.Prefixes.Add(pr);

                var pi = new Prefix() { Name = "Purchase Invoice", DefaultPrefix = "PI", UserId = userid, CompanyId = companyid, BranchId = Branchid };
                db.Prefixes.Add(pi);

                var sq = new Prefix() { Name = "Sales Quote", DefaultPrefix = "SQ", UserId = userid, CompanyId = companyid, BranchId = Branchid };
                db.Prefixes.Add(sq);

                var so = new Prefix() { Name = "Sales Order", DefaultPrefix = "SO", UserId = userid, CompanyId = companyid, BranchId = Branchid };
                db.Prefixes.Add(so);

                var sd = new Prefix() { Name = "Sales Delivery", DefaultPrefix = "SD", UserId = userid, CompanyId = companyid, BranchId = Branchid };
                db.Prefixes.Add(sd);

                var sr = new Prefix() { Name = "Sales Return/Debit Note", DefaultPrefix = "SR", UserId = userid, CompanyId = companyid, BranchId = Branchid };
                db.Prefixes.Add(sr);

                var si = new Prefix() { Name = "Sales Invoice", DefaultPrefix = "SI", UserId = userid, CompanyId = companyid, BranchId = Branchid };
                db.Prefixes.Add(si);

                var cs = new Prefix() { Name = "Cash Sales", DefaultPrefix = "CS", UserId = userid, CompanyId = companyid, BranchId = Branchid };
                db.Prefixes.Add(cs);

                var vr = new Prefix() { Name = "Receive Voucher", DefaultPrefix = "VR", UserId = userid, CompanyId = companyid, BranchId = Branchid };
                db.Prefixes.Add(vr);

                var vp = new Prefix() { Name = "Payment Voucher", DefaultPrefix = "VP", UserId = userid, CompanyId = companyid, BranchId = Branchid };
                db.Prefixes.Add(vp);

                db.SaveChanges();

                var prefixList = db.Prefixes.Where(p => p.UserId == p.UserId && p.CompanyId == companyid && p.BranchId == Branchid).ToList();

                return View(prefixList);
            }
            else
            {
                var prefixList = db.Prefixes.Where(p=>p.UserId==p.UserId && p.CompanyId==companyid && p.BranchId==Branchid).ToList();
                return View(prefixList);
            }
           
        }
        [HttpPost]
        public ActionResult SetPrefix(List<Prefix> prefixList)
        {
            
            foreach (var prefix in prefixList)
            {
                var Pre = db.Prefixes.Find(prefix.Id);
                Pre.SetPrefix = prefix.SetPrefix;
            }
            db.SaveChanges();
            ViewBag.Message = "Updated Successfully!!!";
            return View(prefixList);
        }

    }
}

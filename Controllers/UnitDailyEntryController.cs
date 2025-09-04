using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;

namespace XenERP.Controllers
{
    public class UnitDailyEntryController : Controller
    {
        private InventoryEntities db = new InventoryEntities();
        //
        // GET: /UnitDailyEntry/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            short userid = Convert.ToInt16(Session["userid"]);
            short Createdby = Convert.ToInt16(Session["Createdid"]);
            short Branchid = Convert.ToInt16(Session["BranchId"]);
            short companyid = Convert.ToInt16(Session["companyid"]);
            short Fyid = Convert.ToInt16(Session["fid"]);
            var dateFormat = Session["DateFormat"].ToString();
            var culture = Session["DateCulture"].ToString();
            var fyear = db.FinancialYearMasters.FirstOrDefault(f => f.fYearID == Fyid).Year;
            var fs = fyear.Substring(2, 2);
            var es = fyear.Substring(7, 2);
            fyear = fs + "-" + es;

            var prodet = db.Products.ToList();
            return View();
        }

    }
}

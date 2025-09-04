using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models.Repository;
using XenERP.Models;

namespace XenERP.Controllers
{
    public class DashBoardController : Controller
    {
        //
        // GET: /DashBoard/

        InventoryEntities db = new InventoryEntities();
        TaxRepository taxobj = new TaxRepository();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DashBoard(int id)
        {
            //if (id != 0)
            //{
            //    int companyid = id;

            //    Session["companyid"] = companyid;

            //    int CompId = Convert.ToInt32(Session["companyid"]);


             int CompId = 1;
             Session["company"] = 1;
             var Dtformat = db.Companies.Where(d => d.Id == CompId).FirstOrDefault();
                Session["companylogo"] = Dtformat.CompanyLogo;
                Session["companyname"] = Dtformat.Name;
                var dateformat = db.DateCultureFormats.Where(d => d.Id == Dtformat.DateCultureFormatId).FirstOrDefault();
               // var fYear = db.FinancialYearMasters.Where(u => u.status == "Active" && u.CompanyId == CompId).OrderByDescending(u => u.fYearID).FirstOrDefault();
                if (Dtformat != null)
                {

                    Session["DateFormatLower"] = dateformat.ShortDatePatternLower;
                    Session["DateFormatUpper"] = "{0:" + dateformat.ShortDatePattern + "}";
                    Session["DateCulture"] = dateformat.Name;
                    Session["DateFormat"] = dateformat.ShortDatePattern;
                }

                //if (fYear != null)
                //{
                //    //int Fyid = fYear.fYearID;
                //    //Session["fid"] = Fyid;

                //    return View();
                //}

                //else
                //{
                //    return RedirectToAction("CreateFinancial", "Financial");
                //}

                long Branchid = Convert.ToInt64(id);
            Session["BranchId"] = id;
            var brnm = db.BranchMasters.Where(r => r.Id == Branchid).Select(s => s.Name).FirstOrDefault();
                Session["BranchName"] = db.BranchMasters.Where(r => r.Id == Branchid).Select(s => s.Name).FirstOrDefault();
            //}

            //else
            //{
            //    return View();
            //}
            return View();

        }


        public ActionResult UserDashBoard()
        {

            return View();
        }

    }
}

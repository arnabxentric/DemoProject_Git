using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Globalization;
using XenERP.Models.Repository;

namespace XenERP.Controllers
{
    public class FinancialController : Controller
    {
        //
        // GET: /Financial/

        InventoryEntities db = new InventoryEntities();
        TaxRepository rep = new TaxRepository();
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ShowFinancialYear(string Msg, string Err)
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



            var result = db.FinancialYearMasters.Where(d => d.CompanyId == companyid);

            return View(result);
        }

        [HttpGet]
        public ActionResult CreateFinancial(string Msg, string Err)
        {
            var culture = Session["DateCulture"].ToString();
            var dateFormat = Session["DateFormat"].ToString();

            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }
            var fym = new FinancialYearMasterModelView();
            fym.sDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
            fym.eDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
            

                return View(fym);
               
           
        }



        [HttpPost]
        public ActionResult CreateFinancial(FinancialYearMasterModelView financialyearmastermodelview)
        {


            var culture = Session["DateCulture"].ToString();
            var dateFormat = Session["DateFormat"].ToString();

           

            try
            {
                var financialyearmaster = new FinancialYearMaster();
                int companyid = Convert.ToInt32(Session["companyid"]);

                int Createdby = Convert.ToInt32(Session["Createdid"]);
                int userid = Convert.ToInt32(Session["userid"]);


                financialyearmaster.status = InventoryConst.Cns_Active;

                financialyearmaster.sDate = DateTime.ParseExact(financialyearmastermodelview.sDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                financialyearmaster.eDate = DateTime.ParseExact(financialyearmastermodelview.eDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));

                financialyearmaster.UserId = userid;
                financialyearmaster.CompanyId = companyid;
                financialyearmaster.CreatedBy = Createdby;
                financialyearmaster.CreatedOn = DateTime.Today;


                var syear = financialyearmaster.sDate.Value.Year;
                var eyear = financialyearmaster.eDate.Value.Year;
                

                financialyearmaster.Year = syear + "-" + eyear;
                var check = db.FinancialYearMasters.Any(f => f.Year == financialyearmaster.Year && f.CompanyId==companyid);
                if (check)
                {
                    ViewBag.Message = "Duplicat entry not allowed";
                    return View(financialyearmastermodelview);
                }

                if (financialyearmaster.sDate > financialyearmaster.eDate)
                {
                    ViewBag.Message = "Invalid date selection";
                    return View(financialyearmastermodelview);
                }
                if (!(DateTime.Today >= financialyearmaster.sDate && DateTime.Now <= financialyearmaster.eDate))
                {
                    ViewBag.Message = "Sorry you can create this Financial Year Now.";
                    return View(financialyearmastermodelview);
                }

                db.FinancialYearMasters.Add(financialyearmaster);
                db.SaveChanges();

               // int fyearid = rep.Insertfinancial(financialyearmaster);
                var countfyear = db.FinancialYearMasters.Where(f => f.CompanyId == companyid).OrderByDescending(f => f.fYearID).Select(f=>f.fYearID).FirstOrDefault();
               // if(countfyear==1)
                    Session["fid"] = financialyearmaster.fYearID;

                return RedirectToAction("DashBoard", "DashBoard", new { id = 0 });
            }
            catch
            {
                

                return RedirectToAction("CreateFinancial", new { Err = InventoryMessage.InsertError });
            }



        
        }

        [HttpGet]
        public ActionResult EditFinancial(int id,string Msg, string Err)
        {
            var culture = Session["DateCulture"].ToString();
            var dateFormat = Session["DateFormat"].ToString();

            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }
            var financial = db.FinancialYearMasters.Find(id);

            var fym = new FinancialYearMasterModelView();
            fym.sDate = financial.sDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
            fym.eDate = financial.sDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
            fym.CompanyId = financial.CompanyId;
            fym.fYearID = financial.fYearID;
            fym.UserId = financial.UserId;
           

            return View(fym);


        }

        [HttpPost]
        public ActionResult EditFinancial(FinancialYearMasterModelView financialyearmastermodelview)
        {


            var culture = Session["DateCulture"].ToString();
            var dateFormat = Session["DateFormat"].ToString();



            try
            {
                var financialyearmaster = db.FinancialYearMasters.Find(financialyearmastermodelview.fYearID);
               

                int Createdby = Convert.ToInt32(Session["Createdid"]);
               


               // financialyearmaster.status = InventoryConst.Cns_Active;

                financialyearmaster.sDate = DateTime.ParseExact(financialyearmastermodelview.sDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                financialyearmaster.eDate = DateTime.ParseExact(financialyearmastermodelview.eDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));

                financialyearmaster.UserId = financialyearmastermodelview.UserId;
                financialyearmaster.CompanyId = financialyearmastermodelview.CompanyId;
                financialyearmaster.ModifiedBy = Createdby;
                financialyearmaster.ModifiedOn = DateTime.Today;


                var syear = financialyearmaster.sDate.Value.Year;
                var eyear = financialyearmaster.eDate.Value.Year;
                //var syear = Convert.ToDateTime(financialyearmaster.sDate).Year;
                //var eyear = Convert.ToDateTime(financialyearmaster.eDate).Year;

                financialyearmaster.Year = syear + "-" + eyear;

                if (!(financialyearmaster.sDate >= DateTime.Today && financialyearmaster.eDate <= DateTime.Now))
                {
                    ViewBag.Message = "Sorry you can create this Financial Year Now.";
                    return View(financialyearmastermodelview);
                }

               // db.FinancialYearMasters.Add(financialyearmaster);
                db.SaveChanges();

               // int fyearid = rep.Insertfinancial(financialyearmaster);

                ViewBag.Message = "Financial Year Updated Successfully.";
                return View(financialyearmastermodelview);
            }
            catch
            {


                return RedirectToAction("EditFinancial", new { Err = InventoryMessage.InsertError });
            }




        }

        public ActionResult PreviousFinancialYear()
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            ViewBag.Financial = db.FinancialYearMasters.Where(f => f.CompanyId == companyid).Select(f => new { f.fYearID, f.Year }).ToList();
            ViewBag.FID = Session["fid"];
            return View();
        }
        [HttpPost]
        public ActionResult PreviousFinancialYear(FormCollection fc)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            int Branchid = Convert.ToInt32(Session["BranchId"]);
            if (fc["FinancialId"] != "")
            {
                var fyearid = Convert.ToInt32(fc["FinancialId"]);
                var getYear = db.FinancialYearMasters.Where(d => d.fYearID == fyearid).Select(d => d.Year).FirstOrDefault();
                Session["fid"] = fyearid;
                Session["FinYear"] = getYear;
            }
            else
            {
                ViewBag.Financial = db.FinancialYearMasters.Where(f => f.CompanyId == companyid).Select(f => new { f.fYearID, f.Year }).ToList();
                ViewBag.Message = "Please Select a Financial Year.";
                return View();
            }
            return RedirectToAction("DashBoard", "DashBoard", new { id = Branchid });
        }
    }
}


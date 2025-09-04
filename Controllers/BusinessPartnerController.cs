using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.IO;
using System.Data;
namespace XenERP.Controllers
{
    public class BusinessPartnerController : Controller
    {
        InventoryEntities db = new InventoryEntities();
        //
        // GET: /BusinessPartner/

        [HttpGet]
        public ActionResult CreateBusinessPartner()
        {

            return View();
        }
        [HttpPost]
        public ActionResult CreateBusinessPartner(BusinessPartner bp, FormCollection collection)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);

            int userid = Convert.ToInt32(Session["userid"]);
            
            var filename = string.Empty;
            int Createdby = Convert.ToInt32(Session["Createdid"]);
            string fn = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString();

            try
            {

                foreach (string upload in Request.Files)
                {
                    if (Request.Files[upload].ContentLength == 0) continue;
                    string pathToSave = Server.MapPath("~/productimg/");
                    filename = fn + Path.GetFileName(Request.Files[upload].FileName);
                    string uploadpath = Path.Combine(pathToSave, filename);
                    FileInfo fi1 = new FileInfo(uploadpath);
                    if (fi1.Exists)
                    {
                        System.IO.File.Delete(uploadpath);
                    }
                    Request.Files[upload].SaveAs(Path.Combine(pathToSave, filename));
                }
                bp.Logo = filename;
                bp.CreatedBy = Createdby;
                bp.CreatedOn = DateTime.Now;
                 bp.UserId = userid;
                bp.BranchId = Branchid;
                bp.CompanyId= companyid;
                db.BusinessPartners.Add(bp);
                db.SaveChanges();
                return RedirectToAction("ShowAllBusinessPartner", new { Msg = "Saved Successfully.!!!" });
            }
            catch
            {

                return RedirectToAction("ShowAllBusinessPartner", new { Err = "Please Try Again!...." });
            }
           
        }

        public ActionResult ShowAllBusinessPartner(string Msg, string Err)
        {


            int userid = Convert.ToInt32(Session["userid"]);

            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }

            int companyid = Convert.ToInt32(Session["companyid"]);


            var result = db.BusinessPartners.Where(d => d.CompanyId == companyid).ToList();
            return View(result);
        }

        public ActionResult EditBusinessPartner(int id)
        {

           
            int Createdby = Convert.ToInt32(Session["Createdid"]);
            var result = db.BusinessPartners.Where(d => d.Id == id).FirstOrDefault();
            //    ViewBag.unit = new SelectList(db.UOMs, "Id", "Code", result.UnitId);
            result.Logo = "/productimg/" + result.Logo;
            
            return View(result);
        }
       
        [HttpPost]
        public ActionResult EditBusinessPartner(BusinessPartner bp, FormCollection collection)
        {

            int Createdby = Convert.ToInt32(Session["Createdid"]);

            var filename = string.Empty;
            string fn = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString();

            try
            {



                foreach (string upload in Request.Files)
                {
                    if (Request.Files[upload].ContentLength == 0) continue;
                    string pathToSave = Server.MapPath("~/productimg/");
                    filename = fn + Path.GetFileName(Request.Files[upload].FileName);
                    string uploadpath = Path.Combine(pathToSave, filename);
                    FileInfo fi1 = new FileInfo(uploadpath);
                    if (fi1.Exists)
                    {
                        System.IO.File.Delete(uploadpath);
                    }
                    Request.Files[upload].SaveAs(Path.Combine(pathToSave, filename));
                }


                if (filename != "")
                    bp.Logo = filename;
                bp.ModifiedOn = DateTime.Today;
                bp.ModifiedBy = Createdby;
                db.Entry(bp).State = EntityState.Modified;
                return RedirectToAction("ShowAllBusinessPartner", new { Msg = "Updated Successfully.!!!" });
            }
            catch
            {

                return RedirectToAction("ShowAllBusinessPartner", new { Err = "Please Try Again!...." });
            }
        }
    }
}

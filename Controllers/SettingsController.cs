using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Data.Entity.Validation;
using System.Data;

namespace XenERP.Controllers
{
    public class SettingsController : Controller
    {
        //
        // GET: /Settings/


            InventoryEntities db = new InventoryEntities();
        
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult CustomerDiscount(string Msg, string Err)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }
            return View();
        }



        [HttpPost]
        public ActionResult CustomerDiscount(FurtherDiscount discount, string Fdiscount)
        {
            int Createdby = Convert.ToInt32(Session["Createdid"]);
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);


            try
            {


                var fdcount = db.FurtherDiscounts.Where(d => d.CompanyId == companyid).Count();

                if (fdcount == 0)
                {

                    string[] FD = Fdiscount.Split(',');
                    discount.Discount = false;
                    discount.FD1 = false;
                    discount.FD2 = false;
                    discount.FD3 = false;
                    discount.FD4 = false;
                    foreach (var furdiscount in FD)
                    {

                        if (furdiscount == "5")
                        {
                            discount.Discount = true;
                        }
                        if (furdiscount == "1")
                        {
                            discount.FD1 = true;
                        }

                        if (furdiscount == "2")
                        {
                            discount.FD2 = true;
                        }

                        if (furdiscount == "3")
                        {
                            discount.FD3 = true;
                        }

                        if (furdiscount == "4")
                        {
                            discount.FD4 = true;
                        }

                    }
                    discount.CreatedBy = Createdby;
                    discount.CreatedOn = DateTime.Now;
                    discount.Name = "Further Discount";
                    discount.CompanyId = companyid;
                    discount.UserId = userid;
                    db.FurtherDiscounts.Add(discount);
                    db.SaveChanges();

                }
                else
                {


                    var fdcounts = db.FurtherDiscounts.Where(d => d.CompanyId == companyid).FirstOrDefault();


                     db.FurtherDiscounts.Remove(fdcounts);
                    db.SaveChanges();



                    string[] FD = Fdiscount.Split(',');
                    discount.Discount = false;
                    discount.FD1 = false;
                    discount.FD2 = false;
                    discount.FD3 = false;
                    discount.FD4 = false;

                    foreach (var furdiscount in FD)
                    {
                        if (furdiscount == "5")
                        {
                            discount.Discount = true;
                        }

                        if (furdiscount == "1")
                        {
                            discount.FD1 = true;
                        }

                        if (furdiscount == "2")
                        {
                            discount.FD2 = true;
                        }

                        if (furdiscount == "3")
                        {
                            discount.FD3 = true;
                        }

                        if (furdiscount == "4")
                        {
                            discount.FD4 = true;
                        }

                    }
                  
                    discount.CreatedBy = Createdby;
                    discount.CreatedOn = DateTime.Now;
                    discount.Name = "Further Discount";

                    discount.CompanyId = companyid;
                    discount.UserId = userid;
                    db.FurtherDiscounts.Add(discount);
                    db.SaveChanges();
                }
                return RedirectToAction("CustomerDiscount", new { Msg = "Further Discount Saved Successfully..." });
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
                return RedirectToAction("CustomerDiscount", new { Err = InventoryMessage.InsertError });

            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Data  not Saved Successfully.......";
                return RedirectToAction("CustomerDiscount", new { Err = InventoryMessage.InsertError });

            }
            catch (Exception exp)
            {
                return RedirectToAction("CustomerDiscount", new { Err = InventoryMessage.InsertError });

            }

        }



        [HttpGet]
        public JsonResult ShowAllCusDiscount(string userid)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);

            int AssignId = Convert.ToInt32(Session["AssignId"]);

            int userids = Convert.ToInt32(userid);
            List<menuuser> user = new List<menuuser>();



            var acess = db.FurtherDiscounts.Where(d => d.CompanyId == companyid);

            return Json(acess, JsonRequestBehavior.AllowGet);
        }


    }
}

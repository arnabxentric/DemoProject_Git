using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Data.Entity.Validation;
using System.Data;
using System.IO;
using XenERP.Models;
using XenERP.Models.Repository;
using System.Transactions;

namespace XenERP.Controllers
{
     [SessionExpire]
    public class CompanyController : Controller
    {
        //
        // GET: /Company/

        InventoryEntities db = new InventoryEntities();
        TaxRepository taxobj = new TaxRepository();
        public ActionResult Index()
        {
            return View();
        }


        #region Company 

        [HttpGet]

        public ActionResult CreateCompany(string Msg, string Err)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }
           



            var list = new SelectList(new[]
                                          {
                                              new {ID="Company",Name="Company"},
                                              new{ID="Personal",Name="Personal"},
                                              new{ID="Partnership",Name="Partnership"},
                                                new {ID="SoleTrader",Name="SoleTrader"},
                                              new{ID="Trust",Name="Trust"},
                                              new{ID="Partnership",Name="Partnership"},
                                                new {ID="Charity",Name="Charity"},
                                              new{ID="Club",Name="Club"},
                                              new{ID="Society",Name="Society"},
                                          },
                            "ID", "Name");
            ViewData["list"] = list;








            var dateformat = from cur in db.DateCultureFormats.ToList()
                             where cur.IsDeleted == false
                             select new XenERP.Models.Genral.currencyratess
                             {
                                 Currencyid = cur.Id,
                                 Currencyname = cur.ShortDatePattern 

                             };

            ViewBag.dateformat = dateformat;



            var currencyy = from cur in db.Currencies.ToList()
                            select new XenERP.Models.Genral.currencyratess
                            {
                                Currencyid = cur.CurrencyId,
                                Currencyname = cur.ISO_4217 + "(" + cur.Country + "," + cur.Currency1 + ")",
                                Currencydet = cur.ISO_4217
                            };

            ViewBag.currency = currencyy.OrderBy(d => d.Currencydet);
            
            return View();

        }



        [HttpPost]
        public ActionResult CreateCompany(FormCollection collection)
        {



            int Branchid = Convert.ToInt32(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            int fyid = Convert.ToInt32(Session["fid"]);
            Company company = new XenERP.Models.Company();



            var count = db.Companies.Count();
            if (count > 0)
            {
                return RedirectToAction("CreateCompany", new { Err = "Sorry! You do not have permission to create more then one company." });
            }



            try
            {
                var filename = string.Empty;
                string fn = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString();
                //string filename = Path.GetFileName(Request.Files["productimg"].FileName.ToString());

                //if (filename != "")
                //{
                //    string extension = Path.GetExtension(filename);

                //    string[] img = { ".jpeg", ".png", ".gif", ".bmp", ".jpg" };

                //    if (img.Contains(extension))
                //    {


                //        string path = Server.MapPath("~/Companyimages/");

                //        Request.Files["productimg"].SaveAs(Path.Combine(path, filename));



                //    }

                //    else
                //    {
                //        return RedirectToAction("CreateCompany", new { Err = "Please select  .jpeg, .png, .gif, .bmp, .jpg Images" });

                //    }

                //}

                foreach (string upload in Request.Files)
                {
                    if (Request.Files[upload].ContentLength == 0) continue;
                    string pathToSave = Server.MapPath("~/Companyimages/");
                    filename = fn + Path.GetFileName(Request.Files[upload].FileName);
                    string uploadpath = Path.Combine(pathToSave, filename);
                    FileInfo fi1 = new FileInfo(uploadpath);
                    if (fi1.Exists)
                    {
                        System.IO.File.Delete(uploadpath);
                    }
                    Request.Files[upload].SaveAs(Path.Combine(pathToSave, filename));
                }



                company.Name = collection["Name"];
                company.CompanyDisplayName = collection["CompanyDisplayName"];
                company.BusinessLine = collection["BusinessLine"];
                company.CompanyType = collection["CompanyType"];
                company.CompanyDescription = collection["CompanyDescription"];
                company.DateCultureFormatId = Convert.ToInt32(collection["DateCultureFormatId"]);
                company.CurrencyId = Convert.ToInt32(collection["CurrencyId"]);
                company.CreatedDate = DateTime.Now;
                company.GST_VATNumber = collection["GST_VATNumber"];
                company.Website = collection["Website"];
                company.TANNO = collection["TANNO"];
                company.PANNO = collection["PANNO"];
                company.CountryId = collection["CountryId"];
                company.StateId = collection["StateId"];
                company.City = collection["City"];
                company.Address = collection["Address"];
                company.Zipcode = collection["Zipcode"];
                company.PostalCountryId = collection["PostalCountryId"];
                company.PostalStateId = collection["PostalStateId"];
                company.ContactNumber = collection["ContactNumber"];
                company.EmailId = collection["EmailId"];
                company.Fax = collection["Fax"];
                company.CompanyLogo = filename;

                if (collection["chkpostal"] == "on")
                {

                    company.IssamepostalAddress = true;
                }
                else
                {
                    company.IssamepostalAddress = false;
                }
                company.PurchaseEmailId = collection["PurchaseEmailId"];
                company.SalesEmailId = collection["SalesEmailId"];
                company.CompanyLogo = filename;
                company.Userid = userid;

                int Createdby = Convert.ToInt32(Session["Createdid"]);
                company.CreatedBy = Createdby;
            
                db.Companies.Add(company);
                db.SaveChanges();


                long companyid = taxobj.Insertcompanyid(company);


                int companyidledger = Convert.ToInt32(companyid);

                CurrencyRate rate = new Models.CurrencyRate();
                rate.CurrencyId = Convert.ToInt32(collection["CurrencyId"]);
                rate.PurchaseRate = 1;
                rate.SellRate = 1;
                rate.CompanyId = companyid;
                rate.UserId = userid;
                rate.IsBaseCurrency = true;
                rate.CreatedDate = DateTime.Now;
                rate.CreatedBy = Createdby;
                db.CurrencyRates.Add(rate);
                db.SaveChanges();



                long branch = Convert.ToInt64(Branchid);


                Warehouse ware = new Models.Warehouse();
                ware.Code = "Default"+companyid;
                ware.Name = "Default Warehouse";
                ware.DefaultWarehouse = true;
                ware.Companyid = companyid;
                ware.Userid = userid;
                ware.CreatedOn = DateTime.Now;
                ware.CreatedBy = Createdby.ToString();
                db.Warehouses.Add(ware);
                db.SaveChanges();


              //  db.InsertLedgerBank(fyid, companyidledger, branch, userid);


                //scope.Complete();



                return RedirectToAction("CompanyList", new { Msg = "Company Created Successfully....." });
            }




            catch (Exception exp)
            {
                return RedirectToAction("CompanyList", new { Err = "Please Try Again......." });

            }
        }


        [HttpGet]

        public ActionResult ShowAllCompany(string Msg, string Err)
        {


            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }

            //int userid = Convert.ToInt32(Session["userid"]);


            int companyid = Convert.ToInt32(Session["companyid"]);
            var result = (db.Companies.Where(d => d.Id == companyid).ToList());

            return View(result);

        }



        [HttpGet]
        public ActionResult EditCompany(int id)
        {
            var list = new SelectList(new[]
                                          {
                                              new {ID="Company",Name="Company"},
                                              new{ID="Personal",Name="Personal"},
                                              new{ID="Partnership",Name="Partnership"},
                                                new {ID="SoleTrader",Name="SoleTrader"},
                                              new{ID="Trust",Name="Trust"},
                                              new{ID="Partnership",Name="Partnership"},
                                                new {ID="Charity",Name="Charity"},
                                              new{ID="Club",Name="Club"},
                                              new{ID="Society",Name="Society"},
                                          },
                            "ID", "Name");
            ViewData["list"] = list;



            var result = db.Companies.Where(d => d.Id == id).FirstOrDefault();


            try
            {


                int idd = Convert.ToInt32(result.CurrencyId);
                var cur = db.Currencies.SingleOrDefault(d => d.CurrencyId == idd);

                ViewBag.cur = cur.ISO_4217;
                ViewBag.postal = result.IssamepostalAddress;

                ViewBag.image = result.CompanyLogo;





                var dateformat = from curr in db.DateCultureFormats
                                 where curr.IsDeleted == false
                                 select new
                                 {
                                     Id = curr.Id,
                                     Name = curr.ShortDatePattern 

                                 };

                ViewBag.dateformat = dateformat;

            }
            catch { }




            return View(result);
        }



        [HttpPost]
        public ActionResult EditCompany(FormCollection collection, int id)
        {


            var company = (db.Companies.SingleOrDefault(d => d.Id == id));

            int userid = Convert.ToInt32(Session["userid"]);


            var filename = string.Empty;
            string fn = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString();
            try
            {

                //string filename = Path.GetFileName(Request.Files["productimg"].FileName.ToString());

                //if (filename != "")
                //{
                //    string extension = Path.GetExtension(filename);

                //    string[] img = { ".jpeg", ".png", ".gif", ".bmp", ".jpg" };

                //    if (img.Contains(extension))
                //    {


                //        string path = Server.MapPath("~/Companyimages/");

                //        Request.Files["productimg"].SaveAs(Path.Combine(path, filename));



                //    }

                //    else
                //    {
                //        return RedirectToAction("CreateCompany", new { Err = "Please select  .jpeg, .png, .gif, .bmp, .jpg Images" });

                //    }

                //}

                foreach (string upload in Request.Files)
                {
                    if (Request.Files[upload].ContentLength == 0) continue;
                    string pathToSave = Server.MapPath("~/Companyimages/");
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
                    company.CompanyLogo = filename;
                


                int Createdby = Convert.ToInt32(Session["Createdid"]);



                company.Name = collection["Name"];
                company.CompanyDisplayName = collection["CompanyDisplayName"];
                company.BusinessLine = collection["BusinessLine"];
                company.CompanyType = collection["CompanyType"];
                company.CompanyDescription = collection["CompanyDescription"];
                company.DateCultureFormatId = Convert.ToInt32(collection["DateCultureFormatId"]);
                company.CurrencyId = Convert.ToInt32(collection["CurrencyId"]);
                company.ModifiedOn = DateTime.Now;
                company.ModifiedBy = Createdby;
                company.GST_VATNumber = collection["GST_VATNumber"];
                company.Website = collection["Website"];
                company.TANNO = collection["TANNO"];
                company.PANNO = collection["PANNO"];
                company.CountryId = collection["CountryId"];
                company.StateId = collection["StateId"];
                company.City = collection["City"];
                company.Address = collection["Address"];
                company.Zipcode = collection["Zipcode"];
                company.PostalCountryId = collection["PostalCountryId"];
                company.PostalStateId = collection["PostalStateId"];
                company.ContactNumber = collection["ContactNumber"];
                company.EmailId = collection["EmailId"];
                company.Fax = collection["Fax"];
                
                //   var check = collection["chkpostal"];

                if (collection["chkpostal"] == "on" || collection["chkpostal2"] == "on")
                {

                    company.IssamepostalAddress = true;
                }
                else
                {
                    company.IssamepostalAddress = false;
                }


                company.PurchaseEmailId = collection["PurchaseEmailId"];
                company.SalesEmailId = collection["SalesEmailId"];
                company.CompanyLogo = filename;
                company.Userid = userid;

                db.SaveChanges();



                return RedirectToAction("ShowAllCompany", new { Msg = "Data Saved Successfylly.." });

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
                return RedirectToAction("ShowAllCompany", new { Err = "Please Try Again !.." });

            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Data  not Saved Successfully.......";
                return RedirectToAction("ShowAllCompany", new { Err = "Please Try Again !.." });

            }
            catch (Exception exp)
            {
                return RedirectToAction("ShowAllCompany", new { Err = "Please Try Again !.." });

            }

        }





        [HttpGet]
        public ActionResult CompanyDetails(int id)
        {
            var details = db.Companies.Where(d => d.Id == id).FirstOrDefault();

            return View(details);
        }

        #endregion End Company



        #region Role

        [HttpGet]
        public ActionResult ShowAllRole(string Msg, string Err)
        {

            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }
            int userid = Convert.ToInt32(Session["userid"]);
            int companyid = Convert.ToInt32(Session["companyid"]);

            var role = db.Roles.Where(d => d.CompanyId == companyid);


            return View(role);
        }


        [HttpGet]
        public ActionResult CreateRole()
        {

            int userid = Convert.ToInt32(Session["userid"]);

            var id = db.Users.Where(d => d.Id ==userid).FirstOrDefault();

            ViewBag.companydet = db.Companies.Where(d => d.Userid==userid).ToList();
           //ViewBag.company = company.Name;

            return View();
        }



        [HttpPost]
        public ActionResult CreateRole(Role role)
        {
            int userid = Convert.ToInt32(Session["userid"]);
            int companyid = Convert.ToInt32(Session["companyid"]);

            int Createdby = Convert.ToInt32(Session["Createdid"]);
            Role roles = new Models.Role();
            try
            {
                              
                role.CompanyId = companyid;
                role.UserId = userid;
                role.CreatedBy = Createdby;
                    role.CreatedOn = DateTime.Now;
                db.Roles.Add(role);
                db.SaveChanges();

                return RedirectToAction("ShowAllRole", new { Msg = "Data Saved Successfully.." });
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
                return RedirectToAction("ShowAllRole", new { Err =InventoryMessage.InsertError });

            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Data  not Saved Successfully.......";
                return RedirectToAction("ShowAllRole", new { Err = InventoryMessage.InsertError });

            }
            catch (Exception exp)
            {
                return RedirectToAction("ShowAllRole", new { Err = InventoryMessage.InsertError });

            }

        }


        [HttpPost]
        public JsonResult CheckRole(string RoleName, string Id)
        {
            int userid = Convert.ToInt32(Session["userid"]);


            int id = 0;
            if (string.IsNullOrEmpty(Id))
            {
                bool result = db.Roles.Any(d => d.UserId==userid && d.RoleName == RoleName );

                return Json(!result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                id = Convert.ToInt32(Id);
                bool result = db.Roles.Where(d => d.Id != id).Any(d => d.UserId == userid && d.RoleName == RoleName);

                return Json(!result, JsonRequestBehavior.AllowGet);

            }

        }





        [HttpGet]
        public ActionResult EditRole(int id)
        {


            var result = db.Roles.Where(d => d.Id == id).FirstOrDefault();
            ViewBag.company = new SelectList(db.Companies, "Id", "Name", result.CompanyId);

            return View(result);
        }



        [HttpPost]
        public ActionResult EditRole(Role role)
        {
            int Createdby = Convert.ToInt32(Session["Createdid"]);
            try
            {
                var roles = db.Roles.Where(d => d.Id == role.Id).FirstOrDefault();
                roles.ModifiedBy = Createdby;
                    roles.ModifiedOn = DateTime.Now;
                db.Entry(roles).CurrentValues.SetValues(role);
                db.SaveChanges();

                return RedirectToAction("ShowAllRole", new { Msg = "Data updated successfully.." });
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
                return RedirectToAction("CreateCompany", new { Err = "Data  not  saved successfully.." });

            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Data  not Saved Successfully.......";
                return RedirectToAction("CreateCompany", new { Err = "Data  not  saved successfully.." });

            }
            catch (Exception exp)
            {
                return RedirectToAction("CreateCompany", new { Err = "Data  not  saved successfully.." });

            }

        }
        #endregion

        #region //-------------Company List----------------//



        public ActionResult CompanyListrediredct(string Msg, string Err)
        {
            
            
          return  RedirectToAction("CompanyList");
        }



        public ActionResult CompanyList(string Msg, string Err)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }

                 int userid = Convert.ToInt32(Session["userid"]);

                 var companylist = db.Companies.Where(d => d.Userid == userid).ToList();
                 //Session.Abandon();
                 //Session.Clear();
                 return View(companylist);

                 Session.Abandon();
                 Session.Clear();
        }


        #endregion


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Data.Entity.Validation;
using System.Data;
using System.IO;
using System.Transactions;
using XenERP.Models.Repository;
using System.Globalization;


namespace XenERP.Controllers
{
    public class StartController : Controller
    {
        //
        // GET: /Start/

        InventoryEntities db = new InventoryEntities();
        TaxRepository taxobj = new TaxRepository();


        public ActionResult Index()
        {
            return View();
        }


        #region  Start


        public ActionResult Next()
        {

            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);



            var set = db.CompanySetupGuides.Where(d => d.UserId == userid).FirstOrDefault();

            if (set == null)
            {


                return View();
            }
            else
            {
                return RedirectToAction("NextStep");

            }

        }


        public ActionResult NextStep()
        {

            return View();
        }



        #endregion // End Start

        #region Company Setup


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



            int userid = Convert.ToInt32(Session["userid"]);
            var result = (db.Companies.Where(d => d.Userid == userid).ToList());


            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);




            try
            {

                var set = db.CompanySetupGuides.Where(d => d.UserId == userid).FirstOrDefault();

                if (set == null)
                {
                    CompanySetupGuide setup = new CompanySetupGuide();
                    setup.Start = true;
                    setup.CompanyId = companyid;
                    setup.UserId = userid;
                    setup.BranchId = Branchid;

                    db.CompanySetupGuides.Add(setup);
                    db.SaveChanges();
                }
            }
            catch { }

            return View(result);

        }



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



            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);

            int userid = Convert.ToInt32(Session["userid"]);




            //try
            //{

            //    var set = db.CompanySetupGuides.Where(d => d.UserId == userid).FirstOrDefault();




            //    if (set == null)
            //    {
            //        CompanySetupGuide setup = new CompanySetupGuide();
            //        setup.Start = true;
            //        setup.CompanyId = companyid;
            //        setup.UserId = userid;
            //        setup.BranchId = Branchid;

            //        db.CompanySetupGuides.Add(setup);
            //        db.SaveChanges();

            //        ViewBag.set = 0;
            //    }
            //}
            //catch { }


            var dateformat = from cur in db.DateCultureFormats.ToList()
                             where cur.IsDeleted == false
                             select new XenERP.Models.Genral.currencyratess
                             {
                                 Currencyid = cur.Id,
                                 Currencyname = cur.DisplayName + "(" + cur.ShortDatePattern + ")"

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







            //var scope = new TransactionScope(TransactionScopeOption.RequiresNew,
            //  new TransactionOptions()
            //  {
            //      IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            //  }


            //  );

            //using (scope)
            //{



            try
            {

                string filename = Path.GetFileName(Request.Files["productimg"].FileName.ToString());

                if (filename != "")
                {
                    string extension = Path.GetExtension(filename);

                    string[] img = { ".jpeg", ".png", ".gif", ".bmp", ".jpg" };

                    if (img.Contains(extension))
                    {


                        string path = Server.MapPath("~/Companyimages/");

                        Request.Files["productimg"].SaveAs(Path.Combine(path, filename));



                    }

                    else
                    {
                        return RedirectToAction("CreateCompany", new { Err = "Please select  .jpeg, .png, .gif, .bmp, .jpg Images" });

                    }

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
                db.CurrencyRates.Add(rate);
                db.SaveChanges();



                long branch = Convert.ToInt64(Branchid);

                db.InsertLedgerBank(fyid, companyidledger, branch, userid);


                //scope.Complete();



                return RedirectToAction("CreateCompany", new { Msg = "Data Saved Successfully.." });
            }




            //catch (DbEntityValidationException e)
            //{
            //    foreach (var eve in e.EntityValidationErrors)
            //    {
            //        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
            //        eve.Entry.Entity.GetType().Name, eve.Entry.State);
            //        foreach (var ve in eve.ValidationErrors)
            //        {
            //            Response.Write("- Property:" + ve.PropertyName + ", Error: " + ve.ErrorMessage);

            //        }
            //    }
            //    throw;
            //    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            //    return RedirectToAction("CreateCompany", new { Err = "Company details  not  saved successfully.." });

            //}
            //catch (DataException)
            //{
            //    //Log the error (add a variable name after DataException)
            //    ViewBag.Error = "Error:Data  not Saved Successfully.......";
            //    return RedirectToAction("CreateCompany", new { Err = "Company details  not  saved successfully.." });

            //}
            catch (Exception exp)
            {
                return RedirectToAction("CreateCompany", new { Err = "Please Try Again!.." });

            }
        }



        [HttpGet]
        public ActionResult EditCompany(int id, string Msg, string Err)
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


            var date = new SelectList(new[]
                                            {  new {ID="dd/MM/yyyy",Name="dd/MM/yyyy"},
                                              new{ID="MM/dd/yyyy",Name="MM/dd/yyyy"},
                                              new{ID="dd/MMM/yyyy",Name="dd/MMM/yyyy"},
                                                new {ID="MMM/dd/yyyy",Name="MMM/dd/yyyy"},                                             
                                          },
                            "ID", "Name");
            ViewData["format"] = date;


            try
            {

                int companyid = Convert.ToInt32(Session["companyid"]);

                int userid = Convert.ToInt32(Session["userid"]);

                long Branchid = Convert.ToInt64(Session["BranchId"]);


                var result = (db.Companies.Where(d => d.Id == id).FirstOrDefault());


                try
                {

                    var set = db.CompanySetupGuides.Where(d => d.CompanyId == companyid && d.UserId == userid).FirstOrDefault();

                    if (set == null)
                    {
                        CompanySetupGuide setup = new CompanySetupGuide();
                        setup.Start = true;
                        setup.CompanyId = companyid;
                        setup.UserId = userid;
                        setup.BranchId = Branchid;

                        db.CompanySetupGuides.Add(setup);
                        db.SaveChanges();
                    }
                }
                catch { }




                int idd = Convert.ToInt32(result.CurrencyId);
                var cur = db.Currencies.SingleOrDefault(d => d.CurrencyId == idd);

                ViewBag.cur = cur.ISO_4217;
                ViewBag.postal = result.IssamepostalAddress;

                ViewBag.image = result.CompanyLogo;
            }
            catch { }





            var result2 = (db.Companies.Where(d => d.Id == id).FirstOrDefault());

            return View(result2);
        }



        [HttpPost]
        public ActionResult EditCompany(FormCollection collection, int id)
        {


            var company = (db.Companies.Where(d => d.Id == id).FirstOrDefault());

            int userid = Convert.ToInt32(Session["userid"]);


            try
            {


                company.Name = collection["Name"];
                company.CompanyDisplayName = collection["CompanyDisplayName"];
                company.BusinessLine = collection["BusinessLine"];
                company.CompanyType = collection["CompanyType"];
                company.CompanyDescription = collection["CompanyDescription"];
                company.DateCultureFormatId = Convert.ToInt32(collection["Dateformat"]);
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

                company.Userid = userid;

                db.SaveChanges();

                ViewBag.Message = "Data Saved Successfully..";



                return RedirectToAction("EditCompany", new { Msg = "Data Updated Successfully.." });

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
                ViewBag.Error = "Error:Please Try Again";
                return RedirectToAction("EditCompany", new { Err = "Error:Please Try Again" });
            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Please Try Again";
                return RedirectToAction("EditCompany");
            }
            catch (Exception exp)
            {
                ViewBag.Error = "Error:Please Try Again";
                return RedirectToAction("EditCompany", new { Msg = "Error:Please Try Again" });
            }

        }




        #endregion


        #region Financial Setup



        [HttpGet]
        public ActionResult CreateFinancial()
        {

            int userid = Convert.ToInt32(Session["userid"]);

            ViewBag.companydet = db.Companies.Where(d => d.Userid == userid).ToList();


            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);



            var fyear = db.FinancialYearMasters.Where(d => d.CompanyId == companyid && d.UserId == userid).FirstOrDefault();


            if (fyear == null)
            {
                try
                {

                    ViewBag.finyear = 0;
                    var setup = db.CompanySetupGuides.Where(d => d.UserId == userid).FirstOrDefault();

                    if (setup == null)
                    {
                        ViewBag.finyear = 0;
                        setup.Start = true;
                        setup.Organization = true;
                        ViewBag.set = 0;
                        db.SaveChanges();
                    }
                }
                catch
                {
                }
                return View();
            }
            else
            {

                //foreach(var fin in fyear)
                //{

                ViewBag.start = fyear.sDate;
                ViewBag.end = fyear.eDate;
                //}


                try
                {

                    var setup = db.CompanySetupGuides.Where(d => d.CompanyId == companyid && d.BranchId == Branchid && d.UserId == userid).FirstOrDefault();


                    setup.Start = true;
                    setup.Organization = true;

                    db.SaveChanges();
                }
                catch { }
                return View();
            }





        }
        [HttpPost]
        public ActionResult CreateFinancial(FinancialYearMaster financialyearmaster, FormCollection collection)
        {

            try
            {

                //int companyid = Convert.ToInt32(Session["companyid"]);

                int userid = Convert.ToInt32(Session["userid"]);


                var Dtformat = db.Companies.Where(d => d.Id == financialyearmaster.CompanyId).FirstOrDefault();
                
                var dateformat = db.DateCultureFormats.Where(d => d.Id == Dtformat.DateCultureFormatId).FirstOrDefault();

                Session["DateFormatLower"] = dateformat.ShortDatePatternLower;
                Session["DateFormatUpper"] = "{0:" + dateformat.ShortDatePattern + "}";
                Session["DateCulture"] = dateformat.Name;

                var culture = Session["DateCulture"].ToString();
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
                financialyearmaster.status = InventoryConst.Cns_Active;
                financialyearmaster.sDate = DateTime.Parse(financialyearmaster.sDate.ToString());
                financialyearmaster.eDate = DateTime.Parse(financialyearmaster.eDate.ToString());
                financialyearmaster.UserId = userid;
                financialyearmaster.CompanyId = financialyearmaster.CompanyId;

                try
                {

                    var maxID = db.FinancialYearMasters.Max(f => f.fYearID);

                    financialyearmaster.fYearID = maxID + 1;
                }
                catch { }
                var syear = DateTime.Parse(financialyearmaster.sDate.ToString()).Year;
                var eyear = DateTime.Parse(financialyearmaster.eDate.ToString()).Year;

                financialyearmaster.Year = syear + "-" + eyear;
                var check = db.FinancialYearMasters.Any(f => f.Year == financialyearmaster.Year && f.CompanyId == financialyearmaster.CompanyId);
                if (check)
                {
                    ViewBag.Message = "Duplicat entry not allowed";
                    return View();
                }

                if (financialyearmaster.sDate > financialyearmaster.eDate)
                {
                    ViewBag.Message = "Invalid date selection";
                    return View();
                }

                db.FinancialYearMasters.Add(financialyearmaster);
                db.SaveChanges();

                return RedirectToAction("CreateFinancial", new { Msg = InventoryMessage.Save });
            }
            catch
            {


                return RedirectToAction("CreateFinancial", new { Err = InventoryMessage.InsertError });
            }
        }




        public JsonResult GetFinancialDate(string id)
        {

            int? companyid = Convert.ToInt32(id);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            int userid = Convert.ToInt32(Session["userid"]);

            var fyear = db.FinancialYearMasters.Where(d => d.CompanyId == companyid).FirstOrDefault();

            if (fyear != null)
            {
                Financial fin = new Financial();
                fin.Startdate = fyear.sDate;
                fin.EndDate = fyear.eDate;

                int compid = Convert.ToInt32(id);

                var Dtformat = db.Companies.Where(d => d.Id == compid).FirstOrDefault();
                var dateformat = db.DateCultureFormats.Where(d => d.Id == Dtformat.DateCultureFormatId).FirstOrDefault();

                Session["DateFormatLower"] = dateformat.ShortDatePatternLower;
                Session["DateFormatUpper"] = "{0:" + dateformat.ShortDatePattern + "}";
                Session["DateCulture"] = dateformat.Name;


                return Json(fin, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }





        #endregion

        #region Cuurency Setup



        [HttpGet]

        public ActionResult ShowAllCurrencyrate(string Msg, string Err)
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


            long Branchid = Convert.ToInt64(Session["BranchId"]);

            var currency = db.CurrencyRates.Where(d => d.CompanyId == companyid && d.IsBaseCurrency == true).FirstOrDefault();
            var basecur = db.Currencies.Where(cur => cur.CurrencyId == currency.CurrencyId).FirstOrDefault();
            ViewBag.Basecurrency = basecur.ISO_4217 + "(" + basecur.Country + "," + basecur.Currency1 + ")";







            try
            {
                var result = db.CurrencyRates.Where(d => d.UserId == userid).ToList();

                var setup = db.CompanySetupGuides.Where(d => d.CompanyId == companyid && d.BranchId == Branchid && d.UserId == userid).FirstOrDefault();


                setup.Start = true;
                setup.Organization = true;


                setup.Financial = true;


                db.SaveChanges();


                return View(result);
            }
            catch
            {

                var result = db.CurrencyRates.Where(d => d.UserId == userid).ToList();

                var setup = db.CompanySetupGuides.Where(d => d.CompanyId == companyid && d.BranchId == Branchid && d.UserId == userid).FirstOrDefault();



                setup.Start = true;
                setup.Organization = true;


                setup.Financial = true;


                db.SaveChanges();
                return View(result);
            }
        }



        [HttpGet]

        public ActionResult Createcurrencyrate()
        {

            var result = from cur in db.Currencies.ToList()
                         select new XenERP.Models.Genral.currencyratess
                         {
                             Currencyid = cur.CurrencyId,
                             Currencyname = cur.ISO_4217 + "(" + cur.Country + "," + cur.Currency1 + ")",
                             Currencydet = cur.ISO_4217
                         };
            ViewBag.currency = result.OrderBy(d => d.Currencydet);


            int companyid = Convert.ToInt32(Session["companyid"]);
            var currency = db.CurrencyRates.Where(d => d.CompanyId == companyid && d.IsBaseCurrency == true).FirstOrDefault();
            var basecur = db.Currencies.Where(cur => cur.CurrencyId == currency.CurrencyId).FirstOrDefault();
            ViewBag.Basecurrency = basecur.ISO_4217 + "(" + basecur.Country + "," + basecur.Currency1 + ")";
            return View();
        }




        [HttpPost]

        public ActionResult Createcurrencyrate(FormCollection collection)
        {


            try
            {

                int companyid = Convert.ToInt32(Session["companyid"]);


                int userid = Convert.ToInt32(Session["userid"]);


                CurrencyRate rate = new CurrencyRate();
                rate.CurrencyId = Convert.ToInt32(collection["Currencyselection"]);
                rate.SellRate = Convert.ToDecimal(collection["Sellprice"]);
                rate.PurchaseRate = Convert.ToDecimal(collection["Purchaseprice"]);
                rate.UserId = userid;
                rate.CreatedDate = DateTime.Now;
                rate.IsBaseCurrency = false;
                rate.CompanyId = companyid;
                db.CurrencyRates.Add(rate);
                db.SaveChanges();

                return RedirectToAction("ShowAllCurrencyrate", new { Msg = "Currency Rate Created Successfully...." });
            }
            catch
            {


                return RedirectToAction("ShowAllCurrencyrate", new { Err = "Tax cannot created Successfully...." });
            }
        }


        public JsonResult Getcurrency(int id)
        {

            XenERP.Models.Genral.currencyratess cur = new XenERP.Models.Genral.currencyratess();

            var rate = db.Currencies.Where(d => d.CurrencyId == id).FirstOrDefault();


            cur.Currencyname = rate.ISO_4217;
            return Json(cur, JsonRequestBehavior.AllowGet);
        }



        public JsonResult CheckCurrency(int id)
        {


            int companyid = Convert.ToInt32(Session["companyid"]);

            bool currency = db.CurrencyRates.Any(d => d.CurrencyId == id && d.CompanyId == companyid);
            return Json(currency, JsonRequestBehavior.AllowGet);
        }


        #endregion


        #region Tax Setup


        public ActionResult TaxDetails(string Msg, string Err)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }


            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int fyid = Convert.ToInt32(Session["fid"]);
            int companyid = Convert.ToInt32(Session["companyid"]);

            int userid = Convert.ToInt32(Session["userid"]);
            var setup = db.CompanySetupGuides.Where(d => d.CompanyId == companyid && d.BranchId == Branchid && d.UserId == userid).FirstOrDefault();


            setup.Start = true;
            setup.Organization = true;

            setup.Financial = true;
            // setup.Currencies = true;

            db.SaveChanges();
            return View(db.Taxes.Where(d => d.UserId == userid));
        }


        [HttpGet]
        public ActionResult CreateTax()
        {
            ViewBag.subgroup = new SelectList(db.GroupMasters.Where(d => d.ParentGroupId == 21), "groupID", "groupName");
            return View();
        }


        [HttpPost]
        public ActionResult CreateTax(FormCollection collection)
        {


            int userid = Convert.ToInt32(Session["userid"]);
            var scope = new TransactionScope(TransactionScopeOption.RequiresNew,
               new TransactionOptions()
               {
                   IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
               }


               );

            using (scope)
            {


                try
                {



                    long Branchid = Convert.ToInt64(Session["BranchId"]);
                    int fyid = Convert.ToInt32(Session["fid"]);
                    int companyid = Convert.ToInt32(Session["companyid"]);


                    LedgerMaster ledger = new LedgerMaster();

                    string LedId = "LI";
                    string LedgerID = string.Empty;

                    int ledgerid = db.LedgerMasters.Where(l => l.ledgerID.Contains(LedId) && l.CompanyId == companyid && l.BranchId == Branchid).Count();

                    if (ledgerid == 0)
                    {
                        LedgerID = LedId + "-" + "10001";
                    }
                    else
                    {
                        ledgerid = ledgerid + 1;
                        string tot = "10000" + ledgerid;
                        LedgerID = (LedId + "-") + tot;
                    }

                    ledger.ledgerID = LedgerID;
                    ledger.ledgerName = collection["name"];
                    ledger.groupID = Convert.ToInt32(collection["groupID"]);
                    ledger.CompanyId = companyid;
                    ledger.fYearID = fyid;
                    ledger.ledgerType = "General";
                    ledger.BranchId = Branchid;
                    ledger.UserId = userid;

                    db.LedgerMasters.Add(ledger);
                    db.SaveChanges();


                    long ledid = taxobj.GetLedgerInsertId(ledger);






                    string[] percent = collection["txtpercentage[]"].Split(',');
                    decimal taxtotal = 0;


                    foreach (string percentage in percent)
                    {

                        taxtotal = taxtotal + Convert.ToDecimal(percentage);
                    }


                    Tax tax = new Tax();

                    tax.Name = collection["name"];
                    tax.Rate = taxtotal;
                    tax.LId = ledid;
                    tax.UserId = userid;
                    tax.Category = collection["Category"];
                    if (collection["Status"] == "Active")
                    {
                        tax.Status = true;

                    }
                    else
                    {
                        tax.Status = false;
                    }


                    tax.CompanyId = companyid;
                    tax.BranchId = Branchid;
                    db.Taxes.Add(tax);
                    db.SaveChanges();


                    long taxid = taxobj.InsertTax(tax);


                    Taxrate rate = new Taxrate();

                    string[] name = collection["txtcomponent[]"].Split(',');



                    string[] taxdet = { };
                    int i = 0;



                    foreach (string taxname in name)
                    {

                        rate.TaxName = taxname;
                        rate.Taxrate1 = Math.Round(Convert.ToDecimal(percent[i]), 2);

                        rate.UserId = userid;
                        rate.TaxId = Convert.ToInt32(taxid);
                        db.Taxrates.Add(rate);
                        db.SaveChanges();
                        i++;


                    }



                    db.SaveChanges();
                    scope.Complete();

                    return RedirectToAction("TaxDetails", new { Msg = "Tax Created Successfully...." });
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
                    return RedirectToAction("TaxDetails", new { Err = "Tax details  not  saved successfully.." });

                }
                catch (DataException)
                {
                    //Log the error (add a variable name after DataException)
                    ViewBag.Error = "Error:Data  not Saved Successfully.......";
                    return RedirectToAction("TaxDetails", new { Err = "Tax details  not  saved successfully.." });

                }
                catch (Exception exp)
                {
                    return RedirectToAction("TaxDetails", new { Err = "Tax details  not  saved successfully.." });

                }
            }
        }







        [HttpGet]
        public ActionResult TaxrateDetails(int id)
        {


            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);


            int userid = Convert.ToInt32(Session["userid"]);

            var tax = db.Taxes.SingleOrDefault(d => d.UserId == userid && d.TaxId == id);

            ViewBag.Name = tax.Name;
            var result = db.Taxrates.Where(d => d.TaxId == id);
            return View(result);
        }

        #endregion

        #region Chart of Accounts

        #region ------------Group Master--------------


        public ActionResult ShowGroupMaster(string Msg, string Err)
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
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            var result = db.GroupMasters.Where(d => d.ParentGroupId != 0 && (d.BranchId == Branchid && d.CompanyId == companyid || d.CompanyId == 0 || d.BranchId == 0));
            var setup = db.CompanySetupGuides.Where(d => d.UserId == userid).FirstOrDefault();

            if (setup == null)
            {
                setup.Start = true;
                setup.Organization = true;
                setup.Financial = true;
                ViewBag.set = 0;
                db.SaveChanges();
            }
            return View(result);
        }

        [HttpGet]
        public ActionResult CreateGroupMaster()
        {


            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);

            int userid = Convert.ToInt32(Session["userid"]);

            var setup = db.CompanySetupGuides.Where(d => d.UserId == userid).FirstOrDefault();

            if (setup == null)
            {
                setup.Start = true;
                setup.Organization = true;
                setup.Financial = true;
                //  setup.Currencies = true;
                //  setup.Tax = true;
                db.SaveChanges();
                ViewBag.set = 0;
            }

            ViewBag.Grouptype = db.GroupMasters.Where(d => d.ParentGroupId == 0);
            return View();
        }


        [HttpPost]
        public ActionResult CreateGroupMaster(GroupMaster group, FormCollection collection)
        {
            try
            {


                int companyid = Convert.ToInt32(Session["companyid"]);
                long userid = Convert.ToInt64(Session["userid"]);
                int fyid = Convert.ToInt32(Session["fid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);

                string grouptypename = collection["Grouptypename"];
                int grouptypeid = Convert.ToInt32(collection["GroupType"]);

                string gID = Convert.ToString(collection["gID"]);

                string groupname = collection["groupName"];

                var countparentid = db.GroupMasters.Where(d => d.ParentGroupId == grouptypeid).Count() + 1;

                string parentid = "00" + grouptypeid + "00" + countparentid;
                db.Insertgroupmaster(gID, groupname, grouptypename, grouptypeid, grouptypeid, parentid, fyid, companyid, userid, Branchid);

                return RedirectToAction("ShowGroupMaster", new { Msg = "Data Saved Successfully...." });
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
                return RedirectToAction("ShowGroupMaster", new { Err = "Group details  not  saved successfully.." });

            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Data  not Saved Successfully.......";
                return RedirectToAction("ShowGroupMaster", new { Err = "Group details  not  saved successfully.." });

            }
            catch (Exception exp)
            {
                return RedirectToAction("ShowGroupMaster", new { Err = "Group details  not  saved successfully.." });

            }
        }


        public JsonResult GetGrouptype(int id, int parentid)
        {

            var type = db.GroupMasters.Where(global => global.GrouptypeId == id).FirstOrDefault();
            return Json(type.GroupType, JsonRequestBehavior.AllowGet);
        }




        public JsonResult CheckgroupId(int id, int parentid)
        {

            int companyid = Convert.ToInt32(Session["companyid"]);
            int userid = Convert.ToInt32(Session["userid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);



            bool type = db.GroupMasters.Any(g => g.groupID == id && g.ParentGroupId == parentid && g.CompanyId == companyid && g.BranchId == Branchid && g.UserId == userid);
            return Json(type, JsonRequestBehavior.AllowGet);
        }



        public JsonResult Checkgroupname(string name, int parentid)
        {

            int companyid = Convert.ToInt32(Session["companyid"]);
            int userid = Convert.ToInt32(Session["userid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);



            bool type = db.GroupMasters.Any(g => g.groupName == name && g.ParentGroupId == parentid && g.CompanyId == companyid && g.BranchId == Branchid && g.UserId == userid);
            return Json(type, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

        #region Finish

        public ActionResult Finish()
        {



            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);



            var setup = db.CompanySetupGuides.Where(d => d.UserId == userid).FirstOrDefault();


            setup.Start = true;
            setup.Organization = true;


            setup.Financial = true;
            //setup.Currencies = true;

            setup.Accounts = true;
            //setup.Tax = true;

            setup.Done = true;

            db.SaveChanges();

            return View();

        }



        public ActionResult Done()
        {



            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);



            var setup = db.CompanySetupGuides.Where(d => d.CompanyId == companyid && d.BranchId == Branchid && d.UserId == userid).FirstOrDefault();


            setup.Start = true;
            setup.Organization = true;


            setup.Financial = true;
            //   setup.Currencies = true;

            setup.Accounts = true;
            //  setup.Tax = true;
            // setup.Tax = true;

            db.SaveChanges();

            return RedirectToAction("DashBoard", "DashBoard");

        }




        #endregion
    }
}

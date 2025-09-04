using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using XenERP.Models.Repository;
using System.Transactions;
using System.Data.Entity.Validation;
using System.Data;

namespace XenERP.Controllers
{
    [SessionExpire]
    public class TaxController : Controller
    {


        InventoryEntities db = new InventoryEntities();
        TaxRepository taxobj = new TaxRepository();
        private MasterClasses mc = new MasterClasses();






        #region ---------------------Taxes--------------------


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

            return View(db.Taxes.Where(d =>d.CompanyId == companyid && d.UserId == userid));
        }


        [HttpGet]
        public ActionResult CreateTax()
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int fyid = Convert.ToInt32(Session["fid"]);
            int companyid = Convert.ToInt32(Session["companyid"]);

            int userid = Convert.ToInt32(Session["userid"]);

            var list = new SelectList(new[]
            {
                new{ID="VAT",Name="VAT"},
                new{ID="CST",Name="CST"},
                new{ID="SERVICE",Name="SERVICE"},
                new{ID="OTHERS",Name="OTHERS"},
            }, "ID", "Name");


            ViewData["Categorylist"] = list;

            var Ledger = from grp in db.GroupMasters
                         join led in db.LedgerMasters
                         on grp.groupID equals led.groupID
                         where ((grp.CompanyId == companyid && grp.UserId == userid) || grp.UserId == 0)
                         select new Ledger
                         {
                             Id = led.LID,
                             Name = led.ledgerID + "-" + led.ledgerName
                         };

            ViewBag.ledger = Ledger;
            ViewBag.taxcomponent = db.Taxes.Where(d => d.CompanyId == companyid && d.BranchId == Branchid);

           // ViewBag.subgroup = new SelectList(db.GroupMasters.Where(d => d.ParentGroupId == 21), "groupID", "groupName");
            return View();
        }


        [HttpPost]
        public ActionResult CreateTax(FormCollection collection)
        {


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

                    int companyid = Convert.ToInt32(Session["companyid"]);
                    int userid = Convert.ToInt32(Session["userid"]);
                    //var fYear = db.FinancialYearMasters.Where(u => u.status == "Active" && u.CompanyId == companyid).FirstOrDefault();

                    int Fyid = Convert.ToInt32(Session["fid"]);
                    //LedgerMaster ledger = new LedgerMaster();

                    //string LedId = "LI";
                    //string LedgerID = string.Empty;

                    ////---------Ledger Master---//

                    //int ledgerid = db.LedgerMasters.Where(l => l.ledgerID.Contains(LedId) && l.CompanyId == companyid && l.BranchId == Branchid).Count();

                    //if (ledgerid == 0)
                    //{
                    //    LedgerID = LedId + "-" + "10001";
                    //}
                    //else
                    //{
                    //    ledgerid = ledgerid + 1;
                    //    string tot = "10000" + ledgerid;
                    //    LedgerID = (LedId + "-") + tot;
                    //}

                    //int Createdby = Convert.ToInt32(Session["Createdid"]);

                    //ledger.ledgerID = LedgerID;
                    //ledger.ledgerName = collection["name"];
                    //ledger.groupID = Convert.ToInt32(collection["groupID"]);
                    //ledger.CompanyId = companyid;
                    //ledger.fYearID = Fyid;
                    //ledger.ledgerType = "General";
                    //ledger.BranchId = Branchid;
                    //ledger.UserId = userid;

                    //ledger.CreatedBy = Createdby;
                    //ledger.CreatedOn = DateTime.Now;
                    //db.LedgerMasters.Add(ledger);
                    //db.SaveChanges();


                    ////----------End Ledger Master---------//


                    //long ledid = ledger.LID;

                    //var openingbalance = new OpeningBalance();
                    //openingbalance.ledgerID = ledid;
                    //openingbalance.BranchId = Branchid;
                    //openingbalance.CompanyId = companyid;
                    //openingbalance.UserId = userid;
                    //openingbalance.openingBal = 0;
                    //openingbalance.CreatedBy = Createdby;
                    //openingbalance.CreatedOn = DateTime.Now;
                    //openingbalance.fYearID = Fyid;
                    //db.OpeningBalances.Add(openingbalance);







                    decimal taxtotal = 0;


                    //------------------------Tax Master-----------//

                    Tax tax = new Tax();

                    tax.Name = collection["Name"];

                    tax.LId =Convert.ToInt32(collection["Lid"]);
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

                    if (collection["Cont"] == "1")
                    {
                        tax.IsIndividual = true;
                        tax.Rate = Convert.ToDecimal(collection["Rate"]);
                    }
                    else
                    {
                        tax.Rate = Convert.ToDecimal(collection["taxtotal"]);
                        tax.NetEffective = Convert.ToDecimal(collection["totaleff"]);

                        tax.IsIndividual = false;
                    }


                    tax.CompanyId = companyid;
                    tax.BranchId = Branchid;
                    db.Taxes.Add(tax);
                    db.SaveChanges();


                    //----------End Tax Master--------//




                  //  long taxid = taxobj.InsertTax(tax);
                    long taxid = tax.TaxId; 

                    //--------Tax Rate -------------//

                    if (collection["Cont"] == "2")
                    {


                        Taxrate rate = new Taxrate();

                        string[] taxcompid = collection["TaxcompName"].Split(',');
                        string[] taxrate = collection["txtpercentage"].Split(',');
                        string[] depend = collection["dependshide"].Split(',');
                        string[] compound = collection["compoundhide"].Split(',');
                        string[] effrate = collection["effective"].Split(',');

                        int i = 0;
                        int j = 0;
                        int k = 0;
                        int l = 0;

                        foreach (string taxname in taxcompid)
                        {
                            rate.TaxId = taxid;
                            rate.TaxCompId = Convert.ToInt32(taxname);

                            var taxnamebyid = db.Taxes.Where(d => d.TaxId == rate.TaxCompId).Select(d => d.Name).FirstOrDefault();

                            rate.TaxName = taxnamebyid;

                            rate.Taxrate1 = Math.Round(Convert.ToDecimal(taxrate[i]), 2);
                            string dependtax = depend[j];
                            string compoundtax = compound[k];

                            if (dependtax == "true")
                            {
                                rate.IsDependTax = true;
                            }
                            else
                            {
                                rate.IsDependTax = false;
                            }


                            if (compoundtax == "true")
                            {
                                rate.IsCompoundedTax = true;
                            }
                            else
                            {
                                rate.IsCompoundedTax = false;
                            }

                            rate.EffectiveTaxRate = Convert.ToDecimal(effrate[l]);
                            rate.UserId = userid;
                            rate.TaxId = Convert.ToInt32(taxid);
                            db.Taxrates.Add(rate);
                            db.SaveChanges();
                            i++;
                            j++;
                            k++;
                            l++;


                        }

                        db.SaveChanges();

                        //--------End Tax Rate--------------//
                    }
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
        public ActionResult EditTaxRate(int Id)
        {
            var list = new SelectList(new[]
            {
                new{ID="VAT",Name="VAT"},
                new{ID="CST",Name="CST"},
                new{ID="SERVICE",Name="SERVICE"},
                new{ID="OTHERS",Name="OTHERS"},
            }, "ID", "Name");

            ViewData["Categorylist"] = list;
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

                    var tax = db.Taxes.Where(d => d.TaxId == Id).FirstOrDefault();


                    ViewBag.Taxname = tax.Name;
                    ViewBag.category = tax.Category;
                    ViewBag.status = tax.Status;
                    ViewBag.taxid = tax.TaxId;




                    if (tax.IsIndividual == true)
                    {
                        ViewBag.taxtype = "single";

                    }
                    else
                    {
                        ViewBag.taxtype = "combined";
                    }



                    ViewBag.Nettaxrate = tax.Rate;
                    ViewBag.Neteffrate = tax.NetEffective;

                    var ledger = db.LedgerMasters.Where(d => d.LID == tax.LId).FirstOrDefault();
                    var result = db.GroupMasters.Where(d => d.groupID == ledger.groupID).FirstOrDefault();


                    ViewBag.subgroup = result.groupName;




                    var taxrates = db.Taxrates.Where(d => d.TaxId == Id);

                    int noofrow = taxrates.Count();

                    ViewBag.row = noofrow;


                    int companyid = Convert.ToInt32(Session["companyid"]);


                    long Branchid = Convert.ToInt64(Session["BranchId"]);

                    int userid = Convert.ToInt32(Session["userid"]);


                    var taxrate = db.Taxrates.Where(d => d.TaxId == Id).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1  }).ToList();


                    //  var taxesname = db.Taxrates.ToList();

                    ViewBag.taxnames = taxrate;




                    var taxes1 = db.Taxes.Where(d => d.CompanyId == companyid && d.BranchId == Branchid).
                        Select(t => new menuuser
                        {
                            Id2 = t.TaxId,
                            Name = t.Name
                        }).ToList();

                    ViewBag.taxes = taxes1;





                    //ViewBag.taxnames = new SelectList(db.Taxes, "TaxId", "Name", tax.TaxId);





                    //List<XenERP.Models.Genral.Awaitingpayment> await = new List<XenERP.Models.Genral.Awaitingpayment>();



                    //foreach (var name in taxrates)
                    //{

                    //    var taxname = new XenERP.Models.Genral.Awaitingpayment();

                    //    taxname.Invoiceno = name.TaxName;
                    //    taxname.Paid = name.Taxrate1;

                    //    await.Add(taxname);


                    //}
                    var Ledger = from grp in db.GroupMasters
                                 join led in db.LedgerMasters
                                 on grp.groupID equals led.groupID
                                 where ((grp.CompanyId == companyid && grp.UserId == userid) || grp.UserId == 0)
                                 select new Ledger
                                 {
                                     Id = led.LID,
                                     Name = led.ledgerID + "-" + led.ledgerName
                                 };

                    ViewBag.ledger = Ledger;


                    // return View(await);
                    return View(tax);
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
                    return RedirectToAction("ShowLedgerMaster", new { Err = "Tax details  not  saved successfully.." });

                }
                catch (DataException)
                {
                    //Log the error (add a variable name after DataException)
                    ViewBag.Error = "Error:Data  not Saved Successfully.......";
                    return RedirectToAction("ShowLedgerMaster", new { Err = "Tax details  not  saved successfully.." });

                }
                catch (Exception exp)
                {
                    return RedirectToAction("ShowLedgerMaster", new { Err = "Tax details  not  saved successfully.." });

                }
            }

        }



        [HttpPost]
        public ActionResult EditTaxRate(Tax tax1,FormCollection collection)
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
                   
                    long id = Convert.ToInt64(collection["TaxId"]);
                    var tax=db.Taxes.Where(d => d.TaxId == id).FirstOrDefault();
                    tax.Category = collection["Category"];
                   
                    if (collection["Status"] == "Active")
                    {
                        tax.Status = true;

                    }
                    else
                    {
                        tax.Status = false;
                    }

                    if (collection["Cont"] == "1")
                    {
                        tax.IsIndividual = true;
                        tax.Rate = tax1.Rate;
                    }
                    else
                    {
                        tax.Rate = Convert.ToDecimal(collection["taxtotal"]);
                        tax.NetEffective = Convert.ToDecimal(collection["totaleff"]);

                        tax.IsIndividual = false;
                    }
                    tax.LId = tax1.LId;
                    tax.Name = tax1.Name;
                    var TaxDetails = db.Taxrates.Where(d => d.TaxId == id);



                    foreach (var taxcomp in TaxDetails)
                    {

                        db.Taxrates.Remove(taxcomp);
                       
                    }


                  


                     if (collection["Cont"] == "2")
                    {

                    decimal taxtotal = 0;


                    //--------Tax Rate -------------//



                    Taxrate rate = new Taxrate();

                    string[] taxcompid = collection["TaxcompName"].Split(',');
                    string[] taxrate = collection["txtpercentage"].Split(',');
                    string[] depend = collection["dependshide"].Split(',');
                    string[] compound = collection["compoundhide"].Split(',');
                    string[] effrate = collection["effective"].Split(',');

                    int i = 0;
                    int j = 0;
                    int k = 0;
                    int l = 0;

                    foreach (string taxname in taxcompid)
                    {
                        rate.TaxId = id;
                        rate.TaxCompId = Convert.ToInt32(taxname);
                        var taxnamebyid = db.Taxes.Where(d => d.TaxId == rate.TaxCompId).Select(d => d.Name).FirstOrDefault();
                        rate.TaxName = taxnamebyid;


                        rate.Taxrate1 = Math.Round(Convert.ToDecimal(taxrate[i]), 2);
                        string dependtax = depend[j];
                        string compoundtax = compound[k];

                        if (dependtax == "true")
                        {
                            rate.IsDependTax = true;
                        }
                        else
                        {
                            rate.IsDependTax = false;
                        }


                        if (compoundtax == "true")
                        {
                            rate.IsCompoundedTax = true;
                        }
                        else
                        {
                            rate.IsCompoundedTax = false;
                        }

                        rate.EffectiveTaxRate = Convert.ToDecimal(effrate[l]);
                        rate.UserId = userid;

                        db.Taxrates.Add(rate);
                        db.SaveChanges();
                        i++;
                        j++;
                        k++;
                        l++;

                    }

                       

                        //--------End Tax Rate--------------//
                    }
                     db.SaveChanges();
                    scope.Complete();

                    return RedirectToAction("TaxDetails", new { Msg = "Tax Updated Successfully...." });
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
                    return RedirectToAction("TaxDetails", new { Err = "Please Try Again..." });

                }
                catch (DataException)
                {
                    //Log the error (add a variable name after DataException)
                    ViewBag.Error = "Error:Data  not Saved Successfully.......";
                    return RedirectToAction("TaxDetails", new { Err = "Please Try Again..." });

                }
                catch (Exception exp)
                {
                    return RedirectToAction("TaxDetails", new { Err = "Please Try Again..." });

                }
            }
        }





        public ActionResult DeleteTax(int id)
        {

            try
            {
                var tax = db.Taxes.SingleOrDefault(d => d.TaxId == id);

                db.Taxes.Remove(tax);


                var rate = db.Taxrates.Where(d => d.TaxId == id);

                foreach (var taxes in rate)
                {

                    db.Taxrates.Remove(taxes);
                }


                db.SaveChanges();
                return RedirectToAction("TaxDetails", new { Msg = "Row Deleted Successfully...." });
            }

            catch
            {


                return RedirectToAction("TaxDetails", new { Err = "Row cannot be deleted ...." });

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
            var result = db.Taxrates.Where(d => d.UserId == userid && d.TaxId == id);
            return View(result);
        }


        public JsonResult getTaxes()
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            // var taxes = mc.getDdlTaxes();
            var taxes = db.Taxes.Where(p => p.CompanyId == companyid && p.Category != "SERVICE").Select(t => new { TaxId = t.TaxId, Name = t.Name, Rate = t.Rate }).ToList();
            return Json(taxes, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getTaxerate(int id)
        {
            // var taxes = mc.getDdlTaxes();
            var taxes = db.Taxes.Where(d => d.TaxId == id).Select(t => t.Rate).FirstOrDefault();
            return Json(taxes, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTax()
        {

            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);


            int userid = Convert.ToInt32(Session["userid"]);

            var tax = db.Taxes.Where(p => p.CompanyId == companyid).Select(p => new { Id = p.TaxId, Name = p.Name }).ToList();
            return Json(tax, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]

        public JsonResult Checktaxname(string TaxId, string Name)
        {

            int taxid = 0;
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            if (TaxId == "")
            {

                bool taxcheck = db.Taxes.Any(d => d.Name == Name && d.CompanyId == companyid);
                return Json(!taxcheck, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var taxcheck = db.Taxes.Where(d => d.Name == Name && d.CompanyId == companyid);
                return Json(taxcheck, JsonRequestBehavior.AllowGet);
            }


        }











        #endregion  ----------------End Taxes----------------------------




        #region ----------------Currency------------------




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


            var currency = db.CurrencyRates.Where(d => d.CompanyId == companyid && d.IsBaseCurrency == true).FirstOrDefault();
            var basecur = db.Currencies.Where(cur => cur.CurrencyId == currency.CurrencyId).FirstOrDefault();
            ViewBag.Basecurrency = basecur.ISO_4217 + "(" + basecur.Country + "," + basecur.Currency1 + ")";







            try
            {
                var result = db.CurrencyRates.Where(d => d.CompanyId == companyid && d.UserId == userid && d.IsBaseCurrency == false).ToList();




                return View(result);
            }
            catch
            {

                var result = db.CurrencyRates.Where(d => d.CompanyId == companyid && d.UserId == userid).ToList();

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

                int Createdby = Convert.ToInt32(Session["Createdid"]);
                rate.CreatedBy = Createdby;

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





        [HttpGet]
        public ActionResult EditCurrencyRate(int id)
        {
            var currencyrate = db.CurrencyRates.Where(d => d.Id == id).FirstOrDefault();

            var result = from cur in db.Currencies
                         where cur.CurrencyId == currencyrate.CurrencyId
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


            return View(currencyrate);
        }




        [HttpPost]
        public ActionResult EditCurrencyRate(CurrencyRate rate)
        {
            try
            {
                int Createdby = Convert.ToInt32(Session["Createdid"]);
                rate.ModifiedBy = Createdby;
                rate.ModifiedOn = DateTime.Now;
                db.Entry(rate).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("ShowAllCurrencyrate", new { Msg = "Data Updated Successfylly......" });
            }
            catch
            {


                return RedirectToAction("ShowAllCurrencyrate", new { Err = "Currency rate Cannot be updated...." });
            }




        }

        #endregion



        public ActionResult Checkboxdropdown()
        {

            return View();
        }




    }
}

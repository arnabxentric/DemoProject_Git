using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Transactions;
using System.Data.Entity.Validation;
using XenERP.Models;
using XenERP.Models.Repository;
using System.Data;
using System.Globalization;

namespace XenERP.Controllers
{
    [SessionExpire]
    public class BankController : Controller
    {
        //
        // GET: /Bank/
        InventoryEntities db = new InventoryEntities();
        TaxRepository taxobj = new TaxRepository();

        public ActionResult Index()
        {
            return View();
        }


        #region Bank




        [HttpGet]
        public ActionResult ShowAllBank(string Msg, string Err)
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

            int userid = Convert.ToInt32(Session["userid"]);

            int fyid = Convert.ToInt32(Session["fid"]);

            long Branchid = Convert.ToInt64(Session["BranchId"]);
            var banks = db.Banks.Where(d => d.CompanyId == companyid && d.UserId == userid).ToList();
            var opbal = db.OpeningBalances.Where(d => d.CompanyId == companyid && d.UserId == userid && d.fYearID==fyid).ToList();
            List<Bank> bankslist = new List<Bank>();
            foreach (var bank in banks)
            {
                bank.OpeningBalance = opbal.Where(o => o.ledgerID == bank.LId).Select(o=>o.openingBal).FirstOrDefault();
                bankslist.Add(bank);
            }
            return View(bankslist);
        }

        [HttpGet]
        public ActionResult GetOpeningBalance(long lid)
        {
            var openingBalance = db.OpeningBalances.Where(o => o.ledgerID == lid).OrderByDescending(o => o.Id).Select(o => o.openingBal).FirstOrDefault();
            return Json(openingBalance, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CreateBank()
        {

            int companyid = Convert.ToInt32(Session["companyid"]);
            int userid = Convert.ToInt32(Session["userid"]);
            //var Ledger = from grp in db.GroupMasters
            //             join led in db.LedgerMasters
            //             on grp.groupID equals led.groupID
            //             where ((grp.CompanyId == companyid && grp.UserId == userid))
            //             select new Ledger
            //             {
            //                 Id = led.LID,
            //                 Name = led.ledgerID + "-" + led.ledgerName
            //             };

            var Ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.parentID == null).Select(d => new { Id = d.LID, Name = d.ledgerID + "-" + d.ledgerName });
            ViewBag.ledger = Ledger;
            return View();
        }



        [HttpPost]
        public ActionResult CreateBank(Bank bank)
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


                    int companyid = Convert.ToInt32(Session["companyid"]);


                    int Createdby = Convert.ToInt32(Session["Createdid"]);

                    long Branchid = Convert.ToInt64(Session["BranchId"]);

                    int userid = Convert.ToInt32(Session["userid"]);

                    //var fYear = db.FinancialYearMasters.Where(u => u.status == "Active" && u.CompanyId == companyid).FirstOrDefault();

                    int Fyid = Convert.ToInt32(Session["fid"]);



                    //string LedId = "AS";
                    //string LedgerID = string.Empty;

                    //int ledgerid = db.LedgerMasters.Where(l => (l.CompanyId == companyid || l.BranchId == Branchid || l.CompanyId == 0 || l.BranchId == 0) && l.ledgerID.Contains(LedId)).Count();

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

                    //LedgerMaster led = new LedgerMaster();
                    //led.ledgerName = bank.Name;
                    //led.ledgerID = LedgerID;
                    //led.groupID = 42;
                    //led.CompanyId = companyid;
                    //led.fYearID = Fyid;
                    //led.ledgerType = "General";
                    //led.BranchId = Branchid;
                    //led.UserId = userid;
                    //led.CreatedBy = Createdby;
                    //led.CreatedOn = DateTime.Now;



                    //db.LedgerMasters.Add(led);
                    //db.SaveChanges();

                    //long ledid = taxobj.GetLedgerInsertId(led);

                    
                    bank.CompanyId = companyid;


                    bank.CreatedBy = Createdby;
                    bank.CreatedOn = DateTime.Now;
                    
                    bank.BranchId = Branchid;
                    bank.UserId = userid;
                  //  bank.LId = ledid;
                    db.Banks.Add(bank);
                    db.SaveChanges();
                    scope.Complete();

                    return RedirectToAction("ShowAllBank", new { Msg = "Bank Created Successfully.." });
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
                    return RedirectToAction("ShowAllBank", new { Err = "Please Try Again...." });

                }
                catch (DataException)
                {
                    //Log the error (add a variable name after DataException)
                    ViewBag.Error = "Error:Data  not Saved Successfully.......";
                    return RedirectToAction("CreateCompany", new { Err = "Please Try Again...." });

                }
                catch (Exception exp)
                {
                    return RedirectToAction("ShowAllBank", new { Err = "Please Try Again...." });

                }
            }

        }



        [HttpGet]
        public ActionResult EditBank(int id)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
                    

            int userid = Convert.ToInt32(Session["userid"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            var bank = db.Banks.Where(d => d.Id == id).FirstOrDefault();
            //var Ledger = from grp in db.GroupMasters
            //             join led in db.LedgerMasters
            //             on grp.groupID equals led.groupID
            //             where ((grp.CompanyId == companyid && grp.UserId == userid) || grp.UserId == 0)
            //             select new Ledger
            //             {
            //                 Id = led.LID,
            //                 Name =led.ledgerID + "-" + led.ledgerName
            //             };
            bank.OpeningBalance= db.OpeningBalances.Where(d => d.CompanyId == companyid && d.UserId == userid && d.fYearID == Fyid && d.ledgerID == bank.LId).Select(d=>d.openingBal).FirstOrDefault();
            var Ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.parentID == null).Select(d => new { Id = d.LID, Name = d.ledgerID + "-" + d.ledgerName });
            ViewBag.ledger = Ledger;
            return View(bank);
        }



        [HttpPost]
        public ActionResult EditBank(Bank bank)
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

                    int Createdby = Convert.ToInt32(Session["Createdid"]);
                    //var ledger = db.LedgerMasters.Where(d => d.LID == bank.LId).FirstOrDefault();
                    //ledger.ledgerName = bank.Name;
                    //ledger.ModifiedBy = Createdby;
                    //ledger.ModifiedOn = DateTime.Now;



                    bank.ModifiedBy = Createdby;
                    bank.ModifiedOn = DateTime.Now;


                    db.Entry(bank).State = EntityState.Modified;
                    db.SaveChanges();
                    scope.Complete();

                    return RedirectToAction("ShowAllBank", new { Msg = "Data Updated Successfully.." });
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
                    return RedirectToAction("ShowAllBank", new { Err = "Please Try Again...." });

                }
                catch (DataException)
                {
                    //Log the error (add a variable name after DataException)
                    ViewBag.Error = "Error:Data  not Saved Successfully.......";
                    return RedirectToAction("ShowAllBank", new { Err = "Please Try Again...." });

                }
                catch (Exception exp)
                {
                    return RedirectToAction("ShowAllBank", new { Err = "Please Try Again...." });

                }
            
            }
        }


        #endregion End Bank





        #region Reconcilation





        [HttpGet]

        public ActionResult Reconcilation1(string Msg, string Err)
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

            int userid = Convert.ToInt32(Session["userid"]);

            long Branchid = Convert.ToInt64(Session["BranchId"]);

            ViewBag.Bank = db.Banks.Where(d => d.CompanyId == companyid && d.UserId == userid);

            return View();
        }


    [HttpPost]

        public ActionResult ReconcilationReport(string from, string to,int bankid)
        {

            int companyid = Convert.ToInt32(Session["companyid"]);



            int userid = Convert.ToInt32(Session["userid"]);


            var culture = Session["DateCulture"].ToString();

            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);

            //System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            //DateTime datetime = DateTime.Parse(date);

            DateTime fromdate = DateTime.Parse(from);
            DateTime todate = DateTime.Parse(to);

            try
            {

                ViewBag.bankid = bankid;

                var reconciliation = db.ReceiptPayments.Where(d => d.CompanyId == companyid && d.UserId == userid && d.ReconStatus == false && (d.RPBankId == bankid || d.ledgerId == bankid) && d.RPdate >= fromdate && d.RPdate <= todate);


                return PartialView(reconciliation);
            }
            catch {
                return PartialView();
            
            }
        }



    [HttpPost]
    public JsonResult ReconcilationCreate(string id, string date)
    {

        var culture = Session["DateCulture"].ToString();

        System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);


        try
        {

            string [] ids = id.Split(',');

            string[] recdate = date.Split(',');


            int Createdby = Convert.ToInt32(Session["Createdid"]);
            foreach (var recid in ids)
            {
                
                int reid=Convert.ToInt32(recid);
                var rec = db.ReceiptPayments.Where(d => d.Id == reid).FirstOrDefault();
                rec.ReconStatus = true;
                rec.CreatedBy = Createdby;
                rec.ReconDate = DateTime.Parse(recdate[0]);
       //        db.SaveChanges();
            }

            string val="insert";

            return Json(val,JsonRequestBehavior.AllowGet);
        }
        catch
        {
            string val = "Error";
            return Json(val, JsonRequestBehavior.AllowGet);

        }
    }


    public ActionResult ChequeInHand()
    {

        int companyid = Convert.ToInt32(Session["companyid"]);

            int Fyid = Convert.ToInt32(Session["fid"]);

            int userid = Convert.ToInt32(Session["userid"]);


        var culture = Session["DateCulture"].ToString();

        try
        {

          //  ViewBag.bankid = bankid;
            var Ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid).Select(d => new { Id = d.LID, Name = d.ledgerID + "-" + d.ledgerName });
            ViewBag.Bank = db.Banks.Where(d => d.UserId == userid && d.CompanyId == companyid ).Select(d => new { Id = d.LId, Name = d.Name });
            var reconciliations = db.ReceiptPayments.Where(d => d.CompanyId == companyid && d.UserId == userid && d.RPBankId == 36 && d.transactionType == "General Receive" && d.fYearId==Fyid && d.chequeNo=="1111").OrderBy(d => d.chequeDate).ToList();//.Take(50).ToList();
            foreach (var reconciliation in reconciliations)
            {
                reconciliation.transactionType = Ledger.FirstOrDefault(d=>d.Id==reconciliation.ledgerId).Name;
            }

            return View(reconciliations);
        }
        catch
        {
            return View();

        }
    }

    [HttpPost]
    public JsonResult SendForClearance(string id,string bank,string date)
    {
        int companyid = Convert.ToInt32(Session["companyid"]);
        int userid = Convert.ToInt32(Session["userid"]);
        long Branchid = Convert.ToInt64(Session["BranchId"]);
        var culture = Session["DateCulture"].ToString();
        string dateFormat = Session["DateFormat"].ToString();
        System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);


        try
        {

            string[] ids = id.Split(',');

            string[] banks = bank.Split(',');

            string[] recdate = date.Split(',');

            int Createdby = Convert.ToInt32(Session["Createdid"]);
            int i=0;
            foreach (var recid in ids)
            {

                int reid = Convert.ToInt32(recid);
                var rec = db.ReceiptPayments.Where(d => d.Id == reid).FirstOrDefault();
                rec.RPBankId = Convert.ToInt32(banks[i]);
                

                var reconcile = new Reconcilation();
                reconcile.LID = Convert.ToInt32(banks[i]);
                reconcile.RPId = rec.Id;
                reconcile.SendDate = DateTime.ParseExact(recdate[i], dateFormat, CultureInfo.CreateSpecificCulture(culture));
                reconcile.Status = "Processing";
                reconcile.CreatedBy = Createdby;
                reconcile.CreatedOn = DateTime.Now;
                reconcile.UserId = userid;
                reconcile.CompanyId = companyid;
                reconcile.BranchId = Branchid;
                db.Reconcilations.Add(reconcile);
                db.SaveChanges();

            }

            string val = "insert";

            return Json(val, JsonRequestBehavior.AllowGet);
        }
        catch
        {
            string val = "Error";
            return Json(val, JsonRequestBehavior.AllowGet);

        }
    }

    [HttpGet]
    public ActionResult Reconcilation()
    {

        int companyid = Convert.ToInt32(Session["companyid"]);
        long Branchid = Convert.ToInt64(Session["BranchId"]);


            int userid = Convert.ToInt32(Session["userid"]);


        var culture = Session["DateCulture"].ToString();
        string dateFormat = Session["DateFormat"].ToString();
        var list = new SelectList(new[]
                                          {
                                              new {ID="Cleared",Name="Cleared"},
                                              new{ID="Bounced",Name="Bounced"},
                                          
                                           
                                          },
                               "ID", "Name");
        ViewData["Status"] = list;

        try
        {

                var Ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid).Select(d => new { Id = d.LID, Name = d.ledgerID + "-" + d.ledgerName });
                var reconcile = db.Reconcilations.Where(d => d.CompanyId == companyid && d.BranchId==Branchid && d.Status== "Processing").ToList();
                List<int> RPIds = new List<int>();
                foreach(var d in reconcile)
                {
                    RPIds.Add(d.RPId);
                }
            ViewBag.Bank = db.Banks.Where(d => d.UserId == userid && d.CompanyId == companyid).Select(d => new { Id = d.LId, Name = d.Name });
            var reconciliations = db.ReceiptPayments.Where(d => d.CompanyId == companyid && d.UserId == userid && d.RPBankId != 36 && d.RPBankId!=0 && d.ReconStatus!=true && d.transactionType == "General Receive" && d.BranchId==Branchid && RPIds.Contains(d.Id)).OrderBy(d=>d.chequeDate).ToList();
            foreach (var reconciliation in reconciliations)
            {
                reconciliation.transactionType = Ledger.FirstOrDefault(d => d.Id == reconciliation.ledgerId).Name;
                reconciliation.CardName = Ledger.FirstOrDefault(d => d.Id == reconciliation.RPBankId).Name;
                reconciliation.Remarks = reconcile.FirstOrDefault(d => d.RPId == reconciliation.Id).SendDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture)); 
            }

            return View(reconciliations);
        }
        catch
        {
            return View();

        }
    }

    [HttpPost]
    public JsonResult Reconciled(string id, string bank, string date)
    {
        int companyid = Convert.ToInt32(Session["companyid"]);
        int userid = Convert.ToInt32(Session["userid"]);
        long Branchid = Convert.ToInt64(Session["BranchId"]);
        var culture = Session["DateCulture"].ToString();
        string dateFormat = Session["DateFormat"].ToString();
        System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);


        try
        {

            string[] ids = id.Split(',');

            string[] banks = bank.Split(',');

            string[] recdate = date.Split(',');

            int Createdby = Convert.ToInt32(Session["Createdid"]);
            int i = 0;
            foreach (var recid in ids)
            {

                int reid = Convert.ToInt32(recid);
                var rec = db.ReceiptPayments.Where(d => d.Id == reid).FirstOrDefault();
                if (banks[i] == "Cleared")
                {
                    rec.ReconStatus = true;
                    rec.ReconDate = DateTime.ParseExact(recdate[i], dateFormat, CultureInfo.CreateSpecificCulture(culture));
                }
                else
                {
                    rec.ReconStatus = false;
                    rec.ReconDate = DateTime.ParseExact(recdate[i], dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    rec.IsDeleted = true;
                }

                var reconcile = db.Reconcilations.Where(r => r.RPId == rec.Id).FirstOrDefault();
               
                reconcile.ClearanceDate = DateTime.ParseExact(recdate[i], dateFormat, CultureInfo.CreateSpecificCulture(culture));
                reconcile.Status = banks[i];
                reconcile.ModifiedBy = Createdby;
                reconcile.ModifiedOn = DateTime.Now;
               
             
                db.SaveChanges();

            }

            string val = "insert";

            return Json(val, JsonRequestBehavior.AllowGet);
        }
        catch
        {
            string val = "Error";
            return Json(val, JsonRequestBehavior.AllowGet);

        }
    }

    [HttpGet]
    public ActionResult ReconciledCheque()
    {

        int companyid = Convert.ToInt32(Session["companyid"]);



        int userid = Convert.ToInt32(Session["userid"]);


        var culture = Session["DateCulture"].ToString();
        string dateFormat = Session["DateFormat"].ToString();
       

        try
        {
            var Ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid).Select(d => new { Id = d.LID, Name = d.ledgerID + "-" + d.ledgerName });
            var reconciliations = db.Reconcilations.Where(d => d.CompanyId == companyid && d.ClearanceDate !=null).ToList();
            foreach (var reconciliation in reconciliations)
            {
                reconciliation.ReceiptPayment.CardName = Ledger.FirstOrDefault(d => d.Id==reconciliation.ReceiptPayment.RPBankId).Name;
                reconciliation.ReceiptPayment.transactionType = reconciliation.SendDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                reconciliation.ReceiptPayment.CreditCardNo = reconciliation.ClearanceDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
            }


            return View(reconciliations);
        }
        catch
        {
            return View();

        }
    }
        #endregion
    }
}

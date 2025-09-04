using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Data.Entity.Validation;
using System.Data;
using System.Transactions;
using System.Data.Objects.SqlClient;

namespace XenERP.Controllers
{
    [SessionExpire]
    public class LedgerMasterController : Controller
    {
        //
        // GET: /GroupMaster/
        InventoryEntities db = new InventoryEntities();
        XenERP.Models.Repository.TaxRepository rep = new Models.Repository.TaxRepository();





        #region ------------Ledger Master--------------
        public ActionResult ShowLedgerMaster(string Msg, string Err)
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
            int fyid = Convert.ToInt32(Session["fid"]);
            var groupList = db.GroupMasters.Where(d => d.CompanyId == companyid && d.UserId == userid).ToList();
            List<LedgerMaster> ledgerList = new List<LedgerMaster>();
            List<TaxComponent> openingListBranch = new List<TaxComponent>();
            List<TaxComponent> openingList = new List<TaxComponent>();
            //if (Branchid == 0)
            //{
            ledgerList = db.LedgerMasters.Where(d => d.parentID == null && d.CompanyId == companyid && d.UserId == userid).ToList();
                openingList = db.OpeningBalances.Where(d => d.CompanyId == companyid && d.fYearID == fyid).GroupBy(d => d.ledgerID).Select(d => new TaxComponent { TaxId = d.Key, Amount = d.Sum(t => t.openingBal) }).ToList();
            //}
            //else
            //{
            //    ledgerList = db.LedgerMasters.Where(d => d.parentID == null && d.CompanyId == companyid && d.UserId == userid).ToList();
                openingListBranch = db.OpeningBalances.Where(d => d.CompanyId == companyid && d.fYearID == fyid && d.BranchId == Branchid).Select(d => new TaxComponent { TaxId = d.ledgerID, Amount = d.openingBal }).ToList();
            //}

            var result = from l in groupList
                         join sl in ledgerList on l.groupID equals sl.groupID
                         join op in openingList on sl.LID equals op.TaxId into slop
                         from op in slop.DefaultIfEmpty()
                         join opb in openingListBranch on sl.LID equals opb.TaxId into slopb
                         from opb in slopb.DefaultIfEmpty()
                         select new SubLedger { LID = sl.LID, LedgerCode = sl.ledgerID, LedgerName = sl.ledgerName, ParentLedgerName = l.groupName, OpeningBalance = op == null ? 0 : op.Amount, OpeningBalanceBranch = opb == null ? 0 : opb.Amount };
            return View(result);
        }


        [HttpGet]
        public ActionResult CreateLedgerMaster()
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);

            // ViewBag.Group = db.GroupMasters.Where(d => d.CompanyId == companyid && d.UserId==userid && d.GrouptypeId!=0 && d.BranchId == 0 || d.BranchId == Branchid && d.ParentGroupId == 1 || d.ParentGroupId == 2 || d.ParentGroupId == 3 || d.ParentGroupId == 4 ).OrderBy(d => d.groupName);
            var groups = db.GroupMasters.Where(d => d.CompanyId == companyid && d.UserId == userid && d.GrouptypeId != 0 && d.BranchId == 0).OrderBy(d => d.groupID).ToList();
            foreach (var group in groups)
            {
                if (group.ParentGroupId > 5)
                {
                    var parent = groups.FirstOrDefault(p => p.groupID == group.ParentGroupId).groupName;
                    group.groupName = group.gID + "-" + group.groupName + "(" + parent + ")";
                }
                else
                {
                    group.groupName = group.gID + "-" + group.groupName;
                }
            }
            ViewBag.Group = groups;
            return View();
        }

        [HttpPost]
        public ActionResult CreateLedgerMaster(LedgerMaster ledger, FormCollection collection)
        {


            try
            {

                long Branchid = Convert.ToInt64(Session["BranchId"]);
                int companyid = Convert.ToInt32(Session["companyid"]);
                long userid = Convert.ToInt64(Session["userid"]);
                int fyid = Convert.ToInt32(Session["fid"]);



                string ledid = collection["ledgerid"];


                int subgroupid = Convert.ToInt32(collection["groupID"]);


                decimal OpBalance = Convert.ToDecimal(collection["OpeningBalance"]);


                db.InsertLedgermaster(ledid, ledger.ledgerName, subgroupid, OpBalance, "General", fyid, Branchid, companyid, userid);



                return RedirectToAction("ShowLedgerMaster", new { Msg = "Data Saved Successfully...." });
            }

            catch (Exception exp)
            {
                return RedirectToAction("ShowLedgerMaster", new { Err = "Ledger details  not  saved successfully.." });

            }

        }

        public ActionResult EditLedgerMaster(int id)
        {

            LedgerModelView model = new LedgerModelView();
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int fyid = Convert.ToInt32(Session["fid"]);
            int companyid = Convert.ToInt32(Session["companyid"]);
            long userid = Convert.ToInt64(Session["userid"]);

            var groups = db.GroupMasters.Where(d => d.CompanyId == companyid && d.UserId == userid && d.GrouptypeId != 0 && d.BranchId == 0).OrderBy(d => d.groupID).ToList();
            foreach (var group in groups)
            {
                if (group.ParentGroupId > 5)
                {
                    var parent = groups.FirstOrDefault(p => p.groupID == group.ParentGroupId).groupName;
                    group.groupName = group.gID + "-" + group.groupName + "(" + parent + ")";
                }
                else
                {
                    group.groupName = group.gID + "-" + group.groupName;
                }
            }
            ViewBag.Group = groups;

            if (Branchid == 0)
            {
                ViewBag.Branch = "Head Office";
            }
            else
            {
                ViewBag.Branch = db.BranchMasters.Where(d => d.CompanyId == companyid && d.Id == Branchid).Select(d => d.Name).FirstOrDefault();
            }

            var led = db.LedgerMasters.Where(d => d.LID == id).FirstOrDefault();

            model.Lid = led.LID;
            model.LedgerId = led.ledgerID;
            model.LedgerName = led.ledgerName;
            model.GroupId = led.groupID;
            //if (Branchid == 0)
            //    model.OpeningBalance = db.OpeningBalances.Where(r => r.ledgerID == id && r.fYearID == fyid).GroupBy(r => r.ledgerID).Select(s => s.Sum(d => d.openingBal)).FirstOrDefault();
            //else
            //    model.OpeningBalance = db.OpeningBalances.Where(r => r.ledgerID == id && r.fYearID == fyid && r.BranchId == Branchid).Select(s => s.openingBal).FirstOrDefault();

            try
            {


                var TotalBalance = db.OpeningBalances.Where(d => d.ledgerID == led.LID && d.fYearID == fyid).GroupBy(d => d.ledgerID).Select(d => d.Sum(t => t.openingBal)).FirstOrDefault();
                var OpBalance = db.OpeningBalances.Where(d => d.ledgerID == led.LID && d.BranchId == Branchid && d.fYearID == fyid).FirstOrDefault();
                if (OpBalance != null)
                {
                    ViewBag.Balance = OpBalance.openingBal;
                }
                else
                {
                    ViewBag.Balance = 0;
                }
                ViewBag.TotalBalance = TotalBalance;
            }
            catch { }
            return View(model);

        }

        [HttpPost]
        public ActionResult EditLedgerMaster(LedgerModelView model)
        {
            int Createdby = Convert.ToInt32(Session["Createdid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int fyid = Convert.ToInt32(Session["fid"]);
            int companyid = Convert.ToInt32(Session["companyid"]);
            long userid = Convert.ToInt64(Session["userid"]);

            var ledgerDetails = db.LedgerMasters.Where(r => r.LID == model.Lid).FirstOrDefault();
            ledgerDetails.groupID = model.GroupId;
            ledgerDetails.ledgerName = model.LedgerName;
            ledgerDetails.ModifiedBy = userid;
            ledgerDetails.ModifiedOn = DateTime.Now;

            var openingbal = db.OpeningBalances.Where(r => r.ledgerID == model.Lid && r.fYearID == fyid && r.BranchId == Branchid).FirstOrDefault();
            //if (model.OpeningBalance != opening.openingBal)
            //{
            //    opening.openingBal = model.OpeningBalance;

            //}
            if (openingbal == null)
            {
                var openingBalance1 = new OpeningBalance();
                openingBalance1.BranchId = Branchid;
                openingBalance1.CompanyId = companyid;
                openingBalance1.fYearID = fyid;
                openingBalance1.ledgerID = ledgerDetails.LID;
                openingBalance1.openingBal = model.OpeningBalance;
                openingBalance1.UserId = userid;
                openingBalance1.CreatedBy = Createdby;
                openingBalance1.CreatedOn = DateTime.Now;
                db.OpeningBalances.Add(openingBalance1);
            }
            else
            {
                openingbal.openingBal = model.OpeningBalance; 
            }
            db.SaveChanges();

            return RedirectToAction("ShowLedgerMaster", new { Msg = "Data Updated Successfully...." });
        }



        [HttpGet]
        public ActionResult EditLedgerMaster_Test(int id)
        {


            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int fyid = Convert.ToInt32(Session["fid"]);
            int companyid = Convert.ToInt32(Session["companyid"]);
            long userid = Convert.ToInt64(Session["userid"]);

            if (Branchid == 0)
            {
                ViewBag.Branch = "Head Office";
            }
            else
            {
                ViewBag.Branch = db.BranchMasters.Where(d => d.CompanyId == companyid && d.Id == Branchid).Select(d => d.Name).FirstOrDefault();
            }

            var led = db.LedgerMasters.Where(d => d.LID == id).FirstOrDefault();

            var subgrp = db.GroupMasters.Where(d => d.groupID == led.groupID).FirstOrDefault();

            ViewBag.Subgroup = subgrp.groupName;
            try
            {
                //if (Branchid != 0)
                //{
                //    var OpBalance = db.OpeningBalances.Where(d => d.ledgerID == led.LID && d.BranchId == Branchid && d.fYearID == fyid).FirstOrDefault();
                //    if (OpBalance != null)
                //    {
                //        ViewBag.Balance = OpBalance.openingBal;
                //    }
                //    else
                //    {
                //        ViewBag.Balance = 0;
                //    }
                //}
                //else
                //{
                var TotalBalance = db.OpeningBalances.Where(d => d.ledgerID == led.LID && d.fYearID == fyid).GroupBy(d => d.ledgerID).Select(d => d.Sum(t => t.openingBal)).FirstOrDefault();
                var OpBalance = db.OpeningBalances.Where(d => d.ledgerID == led.LID && d.BranchId == Branchid && d.fYearID == fyid).FirstOrDefault();
                if (OpBalance != null)
                {
                    ViewBag.Balance = OpBalance.openingBal;
                }
                else
                {
                    ViewBag.Balance = 0;
                }
                ViewBag.TotalBalance = TotalBalance;
                //}
            }
            catch { }
            led.BranchId = Branchid;
            return View(led);
        }



        [HttpPost]
        public ActionResult EditLedgerMaster_Test(LedgerMaster ledger, FormCollection collection)
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
                    decimal OpBalance = Convert.ToDecimal(collection["OpeningBalance"]);
                    long Branchid = Convert.ToInt64(Session["BranchId"]);
                    int fyid = Convert.ToInt32(Session["fid"]);
                    int companyid = Convert.ToInt32(Session["companyid"]);
                    long userid = Convert.ToInt64(Session["userid"]);

                    var ledg = db.LedgerMasters.Find(ledger.LID);
                    ledg.ledgerName = ledger.ledgerName;
                    ledger.ModifiedOn = DateTime.Now;
                    ledger.ModifiedBy = Createdby;

                    var openingbal = db.OpeningBalances.Where(d => d.ledgerID == ledger.LID && d.BranchId == Branchid && d.fYearID == fyid).FirstOrDefault();
                    if (openingbal == null)
                    {
                        var openingBalance1 = new OpeningBalance();
                        openingBalance1.BranchId = Branchid;
                        openingBalance1.CompanyId = companyid;
                        openingBalance1.fYearID = fyid;
                        openingBalance1.ledgerID = ledger.LID;
                        openingBalance1.openingBal = OpBalance;
                        openingBalance1.UserId = userid;
                        openingBalance1.CreatedBy = Createdby;
                        openingBalance1.CreatedOn = DateTime.Now;
                        db.OpeningBalances.Add(openingBalance1);
                    }
                    else
                    {
                        openingbal.openingBal = OpBalance;
                    }

                    db.SaveChanges();
                    scope.Complete();


                    return RedirectToAction("ShowLedgerMaster", new { Msg = "Data Updated Successfully...." });
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
                    return RedirectToAction("ShowLedgerMaster", new { Err = "Ledger details  not  update successfully.." });

                }
                catch (DataException)
                {
                    //Log the error (add a variable name after DataException)
                    ViewBag.Error = "Error:Data  not Saved Successfully.......";
                    return RedirectToAction("ShowLedgerMaster", new { Err = "Ledger details  not  update successfully.." });

                }
                catch (Exception exp)
                {
                    return RedirectToAction("ShowLedgerMaster", new { Err = "Ledger details  not  saved successfully.." });

                }
            }
        }

        #endregion



        #region ------------------SUB LEDGER MASTER-------------------

        public ActionResult ShowSubLedgerMaster(string Msg, string Err)
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
            int fyid = Convert.ToInt32(Session["fid"]);
            var parentledgerList = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.UserId == userid).ToList();
            List<LedgerMaster> ledgerList = new List<LedgerMaster>();
            List<TaxComponent> openingListBranch = new List<TaxComponent>();
            List<TaxComponent> openingList = new List<TaxComponent>();
            //if (Branchid == 0)
            //{
            ledgerList = db.LedgerMasters.Where(d => d.parentID != null && d.CompanyId == companyid && d.UserId == userid).ToList();
                openingList = db.OpeningBalances.Where(d => d.CompanyId == companyid && d.fYearID == fyid).GroupBy(d=>d.ledgerID).Select(d => new TaxComponent { TaxId = d.Key, Amount = d.Sum(t=>t.openingBal) }).ToList();
            //}
            //else
            //{
            //    ledgerList = db.LedgerMasters.Where(d => d.parentID != null && d.CompanyId == companyid && d.UserId == userid).ToList();
             openingListBranch = db.OpeningBalances.Where(d => d.CompanyId == companyid && d.fYearID == fyid && d.BranchId == Branchid).Select(d=>new TaxComponent { TaxId=d.ledgerID,Amount=d.openingBal}).ToList();
           // }
          
            var result = from l in parentledgerList 
                            join sl in ledgerList on l.LID  equals sl.parentID
                            join op in openingList on sl.LID equals op.TaxId into slop
                            from op in slop.DefaultIfEmpty()
                             join opb in openingListBranch on sl.LID equals opb.TaxId into slopb
                             from opb in slopb.DefaultIfEmpty()
                         select new SubLedger { LID = sl.LID, LedgerCode = sl.ledgerID, LedgerName = sl.ledgerName, ParentLedgerName = l.ledgerName, OpeningBalance = op == null ? 0 : op.Amount, OpeningBalanceBranch = opb == null ? 0 : opb.Amount };
            return View(result);
        }
      




     

        public ActionResult CreateSubLedger()
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);

            // ViewBag.Group = db.GroupMasters.Where(d => d.CompanyId == companyid && d.UserId==userid && d.GrouptypeId!=0 && d.BranchId == 0 || d.BranchId == Branchid && d.ParentGroupId == 1 || d.ParentGroupId == 2 || d.ParentGroupId == 3 || d.ParentGroupId == 4 ).OrderBy(d => d.groupName);
            var groups = db.GroupMasters.Where(d => d.CompanyId == companyid && d.UserId == userid && d.GrouptypeId != 0 && d.BranchId == 0).OrderBy(d => d.groupID).ToList();
            foreach (var group in groups)
            {
                if (group.ParentGroupId > 5)
                {
                    var parent = groups.FirstOrDefault(p => p.groupID == group.ParentGroupId).groupName;
                    group.groupName = group.gID + "-" + group.groupName + "(" + parent + ")";
                }
                else
                {
                    group.groupName = group.gID + "-" + group.groupName;
                }
            }

            ViewBag.Group = groups;
            return View();
        }

        [HttpPost]
        public ActionResult CreateSubLedger(LedgerMaster ledger, FormCollection collection)
        {
            
            try
            {

                long Branchid = Convert.ToInt64(Session["BranchId"]);
                int companyid = Convert.ToInt32(Session["companyid"]);
                long userid = Convert.ToInt64(Session["userid"]);
                int fyid = Convert.ToInt32(Session["fid"]);
              
                string ledid = collection["ledgerid"];


                int subgroupid = Convert.ToInt32(collection["groupID"]);
                long parentID = Convert.ToInt32(collection["parentID"]);
                var parentcode = db.LedgerMasters.FirstOrDefault(p => p.LID == parentID).ledgerID;
                ledid = parentcode + ledid;
                decimal OpBalance = Convert.ToDecimal(collection["OpeningBalance"]);
                if (Branchid == 0)
                {
                    OpBalance = 0;
                }
                db.InsertSubLedgermaster(ledid, ledger.ledgerName, subgroupid,parentID, OpBalance, "General", fyid, Branchid, companyid, userid);



                return RedirectToAction("ShowSubLedgerMaster", new { Msg = "Data Saved Successfully...." });
            }

            catch (Exception exp)
            {
                return RedirectToAction("ShowLedgerMaster", new { Err = "Ledger details  not  saved successfully.." });

            }

        }
        [HttpGet]
        public ActionResult EditSubLedgerMaster(int id)
        {


            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int fyid = Convert.ToInt32(Session["fid"]);
            int companyid = Convert.ToInt32(Session["companyid"]);
            long userid = Convert.ToInt64(Session["userid"]);
            if (Branchid == 0)
            {
                ViewBag.Branch = "Head Office";
            }
            else
            {
                ViewBag.Branch = db.BranchMasters.Where(d => d.CompanyId == companyid && d.Id == Branchid).Select(d => d.Name).FirstOrDefault();
            }

            var led = db.LedgerMasters.Where(d => d.LID == id).FirstOrDefault();

            var groups = db.GroupMasters.Where(d => d.CompanyId == companyid && d.UserId == userid && d.GrouptypeId != 0 && d.BranchId == 0).OrderBy(d => d.groupID).ToList();
            foreach (var group in groups)
            {
                if (group.ParentGroupId > 5)
                {
                    var parent = groups.FirstOrDefault(p => p.groupID == group.ParentGroupId).groupName;
                    group.groupName = group.gID + "-" + group.groupName + "(" + parent + ")";
                }
                else
                {
                    group.groupName = group.gID + "-" + group.groupName;
                }
            }

            ViewBag.Group = groups;


            var ledger = db.LedgerMasters.Where(d => d.parentID == null).Select(d=>new { Value=d.LID,Text=d.ledgerName}).ToList();
            ViewBag.Ledger = ledger;
            //var grp = db.GroupMasters.Where(d => d.ParentGroupId == subgrp.ParentGroupId).FirstOrDefault();

            //ViewBag.group = grp.groupName;


            try
            {
    

                var TotalBalance = db.OpeningBalances.Where(d => d.ledgerID == led.LID && d.fYearID == fyid).GroupBy(d => d.ledgerID).Select(d => d.Sum(t => t.openingBal)).FirstOrDefault();
                var OpBalance = db.OpeningBalances.Where(d => d.ledgerID == led.LID && d.BranchId == Branchid && d.fYearID == fyid).FirstOrDefault();
                if (OpBalance != null)
                {
                    ViewBag.Balance = OpBalance.openingBal;
                }
                else
                {
                    ViewBag.Balance = 0;
                }
                ViewBag.TotalBalance = TotalBalance;
            }
            catch { }
            led.BranchId = Branchid;
            return View(led);
        }



        [HttpPost]
        public ActionResult EditSubLedgerMaster(LedgerMaster ledger, FormCollection collection)
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
                    decimal OpBalance = Convert.ToDecimal(collection["OpeningBalance"]);
                    long Branchid = Convert.ToInt64(Session["BranchId"]);
                    int fyid = Convert.ToInt32(Session["fid"]);
                    int companyid = Convert.ToInt32(Session["companyid"]);
                    long userid = Convert.ToInt64(Session["userid"]);

                    var ledg = db.LedgerMasters.Find(ledger.LID);
                    ledg.ledgerName = ledger.ledgerName;
                    ledg.parentID = ledger.parentID;
                    ledg.groupID = ledger.groupID;
                    ledger.ModifiedOn = DateTime.Now;
                    ledger.ModifiedBy = Createdby;
                    //if (Branchid != 0)
                    //{
                    var openingbal = db.OpeningBalances.Where(d => d.ledgerID == ledger.LID && d.BranchId == Branchid && d.fYearID == fyid).FirstOrDefault();
                    if (openingbal == null)
                    {
                        var openingBalance1 = new OpeningBalance();
                        openingBalance1.BranchId = Branchid;
                        openingBalance1.CompanyId = companyid;
                        openingBalance1.fYearID = fyid;
                        openingBalance1.ledgerID = ledger.LID;
                        openingBalance1.openingBal = OpBalance;
                        openingBalance1.UserId = userid;
                        openingBalance1.CreatedBy = Createdby;
                        openingBalance1.CreatedOn = DateTime.Now;
                        db.OpeningBalances.Add(openingBalance1);
                    }
                    else
                    {
                        openingbal.openingBal = OpBalance;
                    }
                    //}
                    db.SaveChanges();
                    scope.Complete();


                    return RedirectToAction("ShowSubLedgerMaster", new { Msg = "Data Updated Successfully...." });
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
                    // throw;
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    return RedirectToAction("ShowLedgerMaster", new { Err = "Ledger details  not  update successfully.." });

                }
                catch (DataException)
                {
                    //Log the error (add a variable name after DataException)
                    ViewBag.Error = "Error:Data  not Saved Successfully.......";
                    return RedirectToAction("ShowLedgerMaster", new { Err = "Ledger details  not  update successfully.." });

                }
                catch (Exception exp)
                {
                    return RedirectToAction("ShowLedgerMaster", new { Err = "Ledger details  not  saved successfully.." });

                }
            }
        }



        #endregion



        #region -------------------AJAX CALL==================

        public JsonResult GetSubGroupName(int type)
        {

            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);

            var group = rep.GetSubgrpName(type, companyid, Branchid);
            return Json(group, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLedger(int type)
        {

            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);

            var group = db.LedgerMasters.Where(l => l.groupID == type && l.CompanyId == companyid && l.BranchId == Branchid).Select(l => new { Id = l.LID, Name = l.ledgerName }).ToList();
            //var group1 = rep.GetSubgrpName(type, companyid, Branchid);
            return Json(group, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult getLedgersByName(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            var ledgers = db.LedgerMasters.Where(p => p.ledgerName.Contains(query) && (p.CompanyId == companyid)).Select(p => new { Name = p.ledgerName, Id = p.LID }).ToList();
            return Json(ledgers, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public JsonResult getSubLedgersByName(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            var ledgers = db.LedgerMasters.Where(p => p.ledgerName.Contains(query) && (p.CompanyId == companyid) && p.parentID != null).Select(p => new { Name = p.ledgerName, Id = p.LID }).ToList();

            return Json(ledgers, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult getLedgersByNameWithLtype(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            var ledgers = db.LedgerMasters.Where(p => p.ledgerName.Contains(query) && (p.CompanyId == companyid)).Select(p => new { Name = p.ledgerName, Id = p.LID, GroupId= p.groupID }).ToList();
            var ledgerwithbank = (from l in ledgers
                          join g in db.GroupMasters
                          on l.GroupId equals g.groupID
                          join b in db.Banks
                          on l.Id equals b.LId into lb
                          from b in lb.DefaultIfEmpty()
                          select new { Id = l.Id, Name = l.Name, LGroup = b == null ? g.groupName : "Bank" }).ToList();
            return Json(ledgerwithbank, JsonRequestBehavior.AllowGet);

        }
        public JsonResult GetSubLedger(int type)
        {

            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            //if (Branchid == 0)
            //{
            var group = db.LedgerMasters.Where(l => l.parentID == type && l.CompanyId == companyid).Select(l => new { Id = l.LID, Name = l.ledgerName }).OrderBy(p => p.Name).ToList();
            return Json(group, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            //    var group = db.LedgerMasters.Where(l => l.parentID == type && l.CompanyId == companyid && l.BranchId == Branchid).Select(l => new { Id = l.LID, Name = l.ledgerName }).ToList();
            //    return Json(group, JsonRequestBehavior.AllowGet);
            //}
            //var group1 = rep.GetSubgrpName(type, companyid, Branchid);

        }
        public JsonResult CheckgroupType(int groupID, string ledgerid)
        {
            int groupid = Convert.ToInt32(groupID);
            int ledger = Convert.ToInt32(ledgerid);
            var grouptypeid = db.GroupMasters.Where(d => d.groupID == groupid).FirstOrDefault();
            string LedId = string.Empty;
            int? GtypeId = grouptypeid.GrouptypeId;
            if (GtypeId == 1)
            {
                if (ledger > 1000 && ledger < 2000)
                {
                    return Json("ok", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LedId = "Ledger Code should be between 1000 and 2000";
                    return Json(LedId, JsonRequestBehavior.AllowGet);
                }
            }
            else if (GtypeId == 2)
            {
                if (ledger > 2000 && ledger < 3000)
                {
                    return Json("ok", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LedId = "Ledger Code should be between 2000 and 3000";
                    return Json(LedId, JsonRequestBehavior.AllowGet);
                }
            }
            else if (GtypeId == 3)
            {
                if (ledger > 3000 && ledger < 4000)
                {
                    return Json("ok", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LedId = "Ledger Code should be between 3000 and 4000";
                    return Json(LedId, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (ledger > 4000 && ledger < 5000)
                {
                    return Json("ok", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LedId = "Ledger Code should be between 4000 and 5000";
                    return Json(LedId, JsonRequestBehavior.AllowGet);
                }
            }
        }


        public JsonResult GetLedgerId(int gid)
        {

            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int fyid = Convert.ToInt32(Session["fid"]);
            int companyid = Convert.ToInt32(Session["companyid"]);
            long userid = Convert.ToInt64(Session["userid"]);



            var grouptypeid = db.GroupMasters.Where(d => d.groupID == gid).FirstOrDefault();

            int? GtypeId = grouptypeid.GrouptypeId;

            string LedId = string.Empty;
            if (GtypeId == 1)
            {
                LedId = "Ledger Code should be between 1000 and 2000";
            }
            else if (GtypeId == 2)
            {
                LedId = "Ledger Code should be between 2000 and 3000";
            }
            else if (GtypeId == 3)
            {
                LedId = "Ledger Code should be between 3000 and 4000";

            }
            else if (GtypeId == 4)
            {
                LedId = "Ledger Code should be between 4000 and 5000";

            }

            //string LedgerID = string.Empty;

            //int ledgerid = db.LedgerMasters.Where(l => l.ledgerID.Contains(LedId) && l.CompanyId==companyid && l.UserId==userid && l.BranchId==Branchid || l.BranchId==Branchid || l.CompanyId==companyid).Count();

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
            return Json(LedId, JsonRequestBehavior.AllowGet);
        }


        public JsonResult CheckLedgerId(string id)
        {

            int companyid = Convert.ToInt32(Session["companyid"]);
            int userid = Convert.ToInt32(Session["userid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);



            bool type = db.LedgerMasters.Any(g => g.ledgerID == id && g.CompanyId == companyid && g.BranchId == Branchid && g.UserId == userid);
            return Json(type, JsonRequestBehavior.AllowGet);
        }



        public JsonResult CheckLedgerName(string name)
        {

            int companyid = Convert.ToInt32(Session["companyid"]);
            int userid = Convert.ToInt32(Session["userid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);



            bool type = db.LedgerMasters.Any(g => g.ledgerName == name && g.CompanyId == companyid && g.BranchId == Branchid && g.UserId == userid);
            return Json(type, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}

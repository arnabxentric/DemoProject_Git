using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;

namespace XenERP.Controllers
{
    public class DoubleEntryController : Controller
    {
        private InventoryEntities db = new InventoryEntities();
        //
        // GET: /DoubleEntry/

        //public ActionResult Index()
        //{
        //    return View();
        //}
        public ActionResult Create()
        {
            Story story = new Story
            {
                Sentences = new List<Sentence>
        {
            new Sentence { Id = 1, SentenceText = "AAA"
            , Audio = new Audio{ AudioSelected = "True"}
            , Image = new Image{ ImageSelected = "True"} },

            new Sentence { Id = 2, SentenceText = "BBB"
            , Audio = new Audio{ AudioSelected = "False"}
            , Image = new Image{ ImageSelected = "False"}},

            new Sentence { Id = 3, SentenceText = "CCC"
            , Audio = new Audio{ AudioSelected = "True"}
            , Image = new Image{ ImageSelected = "False"}}
        }
            };
            return View(story);
        }

        [HttpPost]
        public ActionResult AddSentence(Story model)
        {
            if (ModelState.IsValid)
            {
                //_db.Story.Add(model);
                //await _db.SaveChangesAsync();
                return RedirectToAction("Create");
            }
            return View(model);
        }

        public ActionResult BlankSentence()
        {
            return PartialView("_SentenceEditor", new Sentence());
        }

        public ActionResult ReceiveList()
        {
            short companyid = Convert.ToInt16(Session["companyid"]);
            var result = db.DEVouchers.Where(d=>d.CompanyId == companyid &&  d.DEVType == 1 ).OrderBy(d=>d.DEVDate).ToList();
            return View(result);
        }

        public ActionResult PaymentList()
        {
            short companyid = Convert.ToInt16(Session["companyid"]);
            var result = db.DEVouchers.Where(d => d.CompanyId == companyid && d.DEVType == 2).OrderBy(d => d.DEVDate).ToList();
            return View(result);
        }
        //
        // GET: /DoubleEntry/Details/5

        public ActionResult CreateDEV(long? Id = 0, string Msg = null)
        {
            var route = Request.Path;
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
            DoubleEntryModelView dev = new DoubleEntryModelView();
            List<DoubleEntryDetailModelView> devdetList = new List<DoubleEntryDetailModelView>();
            List<DoubleEntryBankDetailModelView> devbdList = new List<DoubleEntryBankDetailModelView>();
            dev.DEVDateString = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var branchList = db.BranchMasters.Where(d => d.CompanyId == companyid).ToList();
            ViewBag.branchList = branchList;
            var banks = db.Banks.Where(d => d.CompanyId == companyid).Select(d => new { LID = d.LId, BankName = d.Name }).ToList();
            var groups = db.GroupMasters.Where(d => d.CompanyId == companyid).Select(d => new { GroupId = d.groupID, GroupName = d.groupName }).ToList();
            var list = new SelectList(new[]
                        {
                                              new {ID="Card",Name="Card"},
                                              new{ID="Cheque",Name="Cheque"},
                                              new{ID="NEFT",Name="Neft/Rtgs"}

                                          },
          "ID", "Name");
            ViewBag.ModeOfPaymentddl = list;

            var clist = new SelectList(new[]
                        {
                                              new{ID="1",Name="1"},
                                              new{ID="2",Name="2"},
                                              new{ID="3",Name="3"},
                                              new{ID="4",Name="4"}

                                          },
          "ID", "Name");
            ViewBag.ClubList = clist;

            if (Id == 0)
            {
                if (route.Contains("/DoubleEntry/Receive"))
                {
                    var maxSerial = db.DEVouchers.Where(d => d.DEVType == 1 && d.FinancialYearId == Fyid && d.BranchId == Branchid).Select(d => d.DEVSerial).DefaultIfEmpty(0).Max();
                    dev.DEVNO = "MRV/" + fyear + "/" + (maxSerial + 1);
                    dev.DEVType = 1;
                }
                else if (route.Contains("/DoubleEntry/Payment"))
                {
                    var maxSerial = db.DEVouchers.Where(d => d.DEVType == 2 && d.FinancialYearId == Fyid && d.BranchId == Branchid).Select(d => d.DEVSerial).DefaultIfEmpty(0).Max();
                    dev.DEVNO = "MPV/" + fyear + "/" + (maxSerial + 1);
                    dev.DEVType = 2;
                }
                else
                {
                    ViewBag.Error = "Unknown Path";
                    return View();
                }
                dev.CompanyId = companyid;
                dev.FinancialYearId = Fyid;
                dev.BId = Branchid;
                dev.UserId = userid;
              
                dev.CreatedBy = Createdby;
            }
            else
            {
                var devEdit = db.DEVouchers.Find(Id);
                if (devEdit != null)
                {
                    dev.Id = devEdit.Id;
                    dev.DEVNO = devEdit.DEVNO;
                    dev.DEVSerial = devEdit.DEVSerial;
                    dev.DEVDateString = devEdit.DEVDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    dev.DEVType = devEdit.DEVType;
                    dev.Narration = devEdit.Narration;
                    dev.DebitTotal = devEdit.DEVoucherDetails.Sum(d=>d.Debit);
                    dev.CreditTotal = devEdit.DEVoucherDetails.Sum(d => d.Credit);
                    dev.FinancialYearId = devEdit.FinancialYearId;
                    dev.BId = devEdit.BranchId;
                    dev.CompanyId = devEdit.CompanyId;
                    dev.CreatedBy = devEdit.CreatedBy;
                    dev.CreatedOn = devEdit.CreatedOn;

                    foreach (var devdtEdit in devEdit.DEVoucherDetails)
                    {
                        var devdt = new DoubleEntryDetailModelView();
                        devdt.Id = devdtEdit.Id;
                        devdt.DEVId = devdtEdit.DEVId;
                        devdt.LID = devdtEdit.LID;
                        devdt.LName = devdtEdit.LedgerMaster.ledgerName;
                        devdt.Club = devdtEdit.Club;
                        devdt.Debit = devdtEdit.Debit;
                        devdt.Credit = devdtEdit.Credit;
                        var isBank = banks.Where(d => d.LID == devdtEdit.LID).FirstOrDefault();
                        if (isBank != null)
                            devdt.LGroup = "Bank";
                        else
                            devdt.LGroup = groups.Where(d => d.GroupId == devdtEdit.LedgerMaster.groupID).Select(d => d.GroupName).FirstOrDefault();
                        
                        devdetList.Add(devdt);
                    }
                    dev.DEV = devdetList;
                    foreach (var devbdEdit in devEdit.DEBankDetails)
                    {
                        var devbd = new DoubleEntryBankDetailModelView();
                        devbd.Id = devbdEdit.Id;
                        devbd.DEVId = devbdEdit.DEVId;
                        devbd.LID = devbdEdit.LID;
                        devbd.LName = devbdEdit.LedgerMaster.ledgerName;
                        devbd.modesofpay = list;
                        devbd.ModeOfPayment = devbdEdit.Mode;
                        devbd.ChequeNo = devbdEdit.ChequeNo;
                        if (devbdEdit.ChequeDate != null)
                            devbd.ChequeDateString = devbdEdit.ChequeDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                        devbd.ChequeDetail = devbd.ChequeDetail;
                        devbdList.Add(devbd);
                       
                    }
                    dev.DEVBD = devbdList;
                }
                else
                {
                    ViewBag.Error = "This voucher is either deleted or not found.";
                    return View();
                }
                if (Msg == "Success_Created")
                {
                    if (devEdit.DEVType == 1)
                        ViewBag.Message = "You have successfully Created New Multi Receive with Voucher No." + devEdit.DEVNO;
                    else if (devEdit.DEVType == 2)
                        ViewBag.Message = "You have successfully Created New Multi Payment with Voucher No." + devEdit.DEVNO;
                    else
                        ViewBag.Error = "Unknown Path";
                }
                if (Msg == "Success_Updated")
                {
                    if (devEdit.DEVType == 1)
                        ViewBag.Message = "You have successfully Updated Multi Receive Voucher No." + devEdit.DEVNO;
                    else if (devEdit.DEVType == 2)
                        ViewBag.Message = "You have successfully Updated Multi Payment Voucher No." + devEdit.DEVNO;
                    else
                        ViewBag.Error = "Unknown Path";
                }
            }
            
            return View(dev);
        }

        //
        // GET: /DoubleEntry/Create

        //public ActionResult Create()
        //{
        //    //https://stackoverflow.com/questions/14822615/how-does-mvc-4-list-model-binding-work
        //    return View();
        //}

        //
        // POST: /DoubleEntry/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[PreventDuplicateRequest]
        public ActionResult CreateDEV(DoubleEntryModelView demv)
        {
            
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt32(Session["companyid"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            var dateFormat = Session["DateFormat"].ToString();
            var culture = Session["DateCulture"].ToString();
            var branchList = db.BranchMasters.Where(d => d.CompanyId == companyid).ToList();
            ViewBag.branchList = branchList;
            var list = new SelectList(new[]
                                    {
                                              new {ID="Card",Name="Card"},
                                              new{ID="Cheque",Name="Cheque"},
                                              new{ID="NEFT",Name="Neft/Rtgs"}

                                          },
                      "ID", "Name");
            ViewBag.ModeOfPaymentddl = list;

            var clist = new SelectList(new[]
                        {
                                              new{ID="1",Name="1"},
                                              new{ID="2",Name="2"},
                                              new{ID="3",Name="3"},
                                              new{ID="4",Name="4"}

                                          },
          "ID", "Name");
            ViewBag.ClubList = clist;

            try
            {
                if (ModelState.IsValid)
                {
                    var financialYear = db.FinancialYearMasters.Where(d => d.fYearID == demv.FinancialYearId).FirstOrDefault();
                    var fyear = financialYear.Year;
                    var fs = fyear.Substring(2, 2);
                    var es = fyear.Substring(7, 2);
                    fyear = fs + "-" + es;

                    DateTime date = DateTime.ParseExact(demv.DEVDateString, dateFormat, CultureInfo.CreateSpecificCulture(culture));

                    if (!(date >= financialYear.sDate && date <= financialYear.eDate))
                    {
                        ViewBag.Error = "Multi Receive Voucher Date out of scope of " + financialYear.Year + " Financial Year.";
                        return View(demv);
                    }

                    if (demv.Id == 0)
                    {
                        DEVoucher Dev = new DEVoucher();
                        if (demv.DEVType == 1)
                        {
                            Dev.DEVSerial = db.DEVouchers.Where(d => d.DEVType == 1 && d.FinancialYearId == Fyid && d.BranchId == Branchid).Select(d => d.DEVSerial).DefaultIfEmpty(0).Max() + 1;
                            Dev.DEVNO = "MRV/" + fyear + "/" + (Dev.DEVSerial);
                        }
                        else if (demv.DEVType == 2)
                        {
                            Dev.DEVSerial = db.DEVouchers.Where(d => d.DEVType == 2 && d.FinancialYearId == Fyid && d.BranchId == Branchid).Select(d => d.DEVSerial).DefaultIfEmpty(0).Max() + 1;
                            Dev.DEVNO = "MPV/" + fyear + "/" + (Dev.DEVSerial);
                        }
                        Dev.DEVDate = date;
                        Dev.Narration = demv.Narration;
                        Dev.FinancialYearId = demv.FinancialYearId;
                        Dev.CompanyId = demv.CompanyId;
                        Dev.BranchId = demv.BId;
                        Dev.DEVType = demv.DEVType;
                        Dev.CreatedBy = demv.CreatedBy;
                        Dev.CreatedOn = DateTime.Now;

                        foreach (var devdet in demv.DEV)
                        {
                            DEVoucherDetail DevDt = new DEVoucherDetail();
                            DevDt.LID = devdet.LID;
                            DevDt.Club = devdet.Club;
                            DevDt.Debit = devdet.Debit;
                            DevDt.Credit = devdet.Credit;
                            DevDt.IsBank = devdet.LGroup == "Bank" ? true : false;

                            Dev.DEVoucherDetails.Add(DevDt);
                        }

                        if (demv.DEVBD != null)
                        {
                            foreach (var devdet in demv.DEVBD)
                            {
                                DEBankDetail DevBd = new DEBankDetail();

                                DevBd.LID = devdet.LID;
                                DevBd.Mode = devdet.ModeOfPayment;
                                DevBd.ChequeNo = devdet.ChequeNo;
                                if (devdet.ChequeDateString != null)
                                    DevBd.ChequeDate = DateTime.ParseExact(devdet.ChequeDateString, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                                DevBd.ChequeDetail = devdet.ChequeDetail;

                                Dev.DEBankDetails.Add(DevBd);
                            }
                        }

                        db.DEVouchers.Add(Dev);
                        // var a= db.DEVoucherDetails.Where(d=> d.LedgerMaster.gro)
                        db.SaveChanges();
                        demv.Id = Dev.Id;
                        if (Dev.DEVType == 1)
                        {
                            ViewBag.Message = "You have successfully created Multi Receive Voucher No." + Dev.DEVNO;
                            return RedirectToAction("Receive", new { Id = Dev.Id, Msg = "Success_Created" });
                        }
                        else if (Dev.DEVType == 2)
                        {
                            ViewBag.Message = "You have successfully created Multi Payment Voucher No." + Dev.DEVNO;
                            return RedirectToAction("Payment", new { Id = Dev.Id, Msg = "Success_Created" });
                        }
                    }
                    else
                    {
                        var Dev = db.DEVouchers.Find(demv.Id);
                        Dev.DEVDate = date;
                        Dev.Narration = demv.Narration;
                        Dev.FinancialYearId = demv.FinancialYearId;
                        Dev.CompanyId = demv.CompanyId;
                        Dev.BranchId = demv.BId;
                        Dev.DEVType = demv.DEVType;
                        Dev.ModifiedBy  = demv.CreatedBy;
                        Dev.ModifiedOn = DateTime.Now;
                        //db.Entry(Dev).CurrentValues.SetValues(Dev);
                        //UpdateModel(Dev);
                        foreach(var devdet in demv.DEV)
                        {
                            var DevDt = db.DEVoucherDetails.Find(devdet.Id);
                            if(DevDt != null)
                            {
                                DevDt.LID = devdet.LID;
                                DevDt.Club = devdet.Club;
                                DevDt.Debit = devdet.Debit;
                                DevDt.Credit = devdet.Credit;
                                DevDt.IsBank = devdet.LGroup == "Bank" ? true : false;
                            }
                            else
                            {
                                DEVoucherDetail DevDtAdd = new DEVoucherDetail();
                                DevDtAdd.Club = devdet.Club;
                                DevDtAdd.LID = devdet.LID;
                                DevDtAdd.Club = devdet.Club;
                                DevDtAdd.Debit = devdet.Debit;
                                DevDtAdd.Credit = devdet.Credit;
                                DevDtAdd.IsBank = devdet.LGroup == "Bank" ? true : false;

                                Dev.DEVoucherDetails.Add(DevDtAdd);
                                
                            }
                        }
                        var discarded = db.DEVoucherDetails.Where(d => d.DEVId == Dev.Id).ToList();
                        discarded = discarded.Where(d =>  !demv.DEV.Any(f => f.Id == d.Id) ).ToList();
                        foreach (var discard in discarded)
                        {
                            db.DEVoucherDetails.Remove(discard);
                        }

                        if (demv.DEVBD != null)
                        {
                            foreach (var devbd in demv.DEVBD)
                            {
                                var DevBDE = db.DEBankDetails.Find(devbd.Id);
                                if (DevBDE != null)
                                {
                                    DevBDE.LID = devbd.LID;
                                    DevBDE.Mode = devbd.ModeOfPayment;
                                    DevBDE.ChequeNo = devbd.ChequeNo;
                                    if (devbd.ChequeDateString != null)
                                        DevBDE.ChequeDate = DateTime.ParseExact(devbd.ChequeDateString, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                                    DevBDE.ChequeDetail = devbd.ChequeDetail;
                                   
                                }
                                else
                                {
                                    DEBankDetail DevBDA = new DEBankDetail();
                                    DevBDA.LID = devbd.LID;
                                    DevBDA.Mode = devbd.ModeOfPayment;
                                    DevBDA.ChequeNo = devbd.ChequeNo;
                                    if (devbd.ChequeDateString != null)
                                        DevBDA.ChequeDate = DateTime.ParseExact(devbd.ChequeDateString, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                                    DevBDA.ChequeDetail = devbd.ChequeDetail;
                                    Dev.DEBankDetails.Add(DevBDA);
                                }
                            }
                            var discardedBank = db.DEBankDetails.Where(d => d.DEVId == Dev.Id).ToList();
                            discardedBank = discardedBank.Where(d => !demv.DEVBD.Any(f =>f.Id == d.Id )).ToList();
                            foreach (var discard in discardedBank)
                            {
                                db.DEBankDetails.Remove(discard);
                            }
                        }
                        else
                        {
                            var discardedBank = db.DEBankDetails.Where(d => d.DEVId == Dev.Id).ToList();
                            discardedBank = discardedBank.Where(d => !demv.DEVBD.Any(f => f.Id == d.Id)).ToList();
                            foreach (var discard in discardedBank)
                            {
                                db.DEBankDetails.Remove(discard);
                            }
                        }

                        db.SaveChanges();
                        if (Dev.DEVType == 1)
                        {
                            ViewBag.Message = "You have successfully Updated Multi Receive Voucher No." + Dev.DEVNO;
                            return RedirectToAction("Receive", new { Id = Dev.Id, Msg = "Success_Updated" });
                        }
                        else if (Dev.DEVType == 2)
                        {
                            ViewBag.Message = "You have successfully Updated Multi Payment Voucher No." + Dev.DEVNO;
                            return RedirectToAction("Payment", new { Id = Dev.Id, Msg = "Success_Updated" });
                        }
                    }
                }
                //else
                //{
                //    ViewBag.Message = ModelState.Keys.
                //}
                
                return View(demv);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
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

                //ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                //var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                //ViewBag.ddlWarehouses = warehouses1;

                //ViewBag.podmvList = podmvList;

                ViewBag.Message = "Error";
                return View(demv);
            }
            catch (DataException ex)
            {
                //Log the error (add a variable name after DataException)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
    
                ViewBag.Message = "Error";
                return View(demv);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error! Please Contact Administrator.";
                return View(demv);
            }
        }

        //
        // GET: /DoubleEntry/Edit/5

        public ActionResult EditDEV(int id)
        {
            return View();
        }

        //
        // POST: /DoubleEntry/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /DoubleEntry/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /DoubleEntry/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.Entity.Validation;
using System.Globalization;

namespace XenERP.Controllers
{
    public class ContraController : Controller
    {
        //
        // GET: /Contra/

        InventoryEntities db = new InventoryEntities();
        public ActionResult Index()
        {
            return View();
        }



        #region -----------Contra Voucher-------------


        [HttpGet]
        public ActionResult ShowAllContraVoucherSearch(string DateFrom, string DateTo)
        {
            var culture = "es-AR";
            string dateFormat = "dd/MM/yyyy";
            var DtFrm = DateTime.ParseExact(DateFrom, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var DtTo = DateTime.ParseExact(DateTo, dateFormat, CultureInfo.CreateSpecificCulture(culture));


            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            int fyid = Convert.ToInt32(Session["fid"]);
            List<ReceiptPayment> grid = new List<ReceiptPayment>();
            if (Branchid == 0)
                grid = db.ReceiptPayments.Where(d => d.CompanyId == d.CompanyId && d.RPdate>=DtFrm && d.RPdate<=DtTo && d.UserId == userid && d.fYearId == fyid && (d.transactionType == "Cash To Bank" || d.transactionType == "Bank To Cash" || d.transactionType == "Bank To Bank")).OrderByDescending(d => d.Id).ToList();
            else
                grid = db.ReceiptPayments.Where(d => d.CompanyId == d.CompanyId && d.RPdate >= DtFrm && d.RPdate <= DtTo && d.BranchId == Branchid && d.UserId == userid && d.fYearId == fyid && (d.transactionType == "Cash To Bank" || d.transactionType == "Bank To Cash" || d.transactionType == "Bank To Bank")).OrderByDescending(d => d.Id).ToList();
            return View(grid);
        }


        [HttpGet]
        public ActionResult ShowAllContraVoucher(string Msg, string Err)
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
           long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            int fyid = Convert.ToInt32(Session["fid"]);
            List<ReceiptPayment> grid = new List<ReceiptPayment>();
            if(Branchid==0)
                 grid = db.ReceiptPayments.Where(d => d.CompanyId == d.CompanyId  && d.UserId == userid && d.fYearId == fyid && (d.transactionType == "Cash To Bank" || d.transactionType == "Bank To Cash" || d.transactionType == "Bank To Bank")).OrderByDescending(d => d.Id).Take(100).ToList();
            else
                 grid = db.ReceiptPayments.Where(d => d.CompanyId == d.CompanyId && d.BranchId == Branchid && d.UserId == userid && d.fYearId == fyid && (d.transactionType == "Cash To Bank" || d.transactionType == "Bank To Cash" || d.transactionType == "Bank To Bank")).OrderByDescending(d => d.Id).Take(100).ToList();
          return View(grid);
        }

        [HttpGet]
        public ActionResult CreateContra()
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
           // var fYear = db.FinancialYearMasters.Where(u => u.status == "Active" && u.CompanyId == companyid).FirstOrDefault();

            int fyid = Convert.ToInt32(Session["fid"]);



            // ViewBag.toBank = db.LedgerMasters.Where(l => l.groupID == 1  && l.BranchId == Branchid && l.CompanyId == companyid && l.UserId==userid).ToList();

            ViewBag.toBank = db.Banks.Where(l =>  l.CompanyId == companyid && l.UserId == userid).Select(l=> new {Lid=l.LId, ledgerName=l.Type==null || l.Type=="" ?l.Name : l.Name + "(" + l.Type + ")" }).ToList();
            ViewBag.FromBank = db.Banks.Where(l => l.CompanyId == companyid && l.UserId == userid).Select(l => new { Lid = l.LId, ledgerName = l.Type == null || l.Type == "" ? l.Name : l.Name + "(" + l.Type + ")" }).ToList();

            //ViewBag.FromBank = db.LedgerMasters.Where(l => l.groupID == 42 &&  l.BranchId == Branchid && l.CompanyId == companyid && l.UserId==userid).ToList();



            return View();
        }

        [HttpPost]
        public ActionResult CreateContra(FormCollection collection)
        {
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            int Createdby = Convert.ToInt32(Session["Createdid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

        l1: var transNo = GenRandom.GetRandom();

            string tran = Convert.ToString(transNo);

            var exists = db.Contras.Any(c => c.transactionNo == tran);

            if (exists)
            {
                goto l1;
            }

            try
            {
                int BranchId = Convert.ToInt32(Session["Branchid"]);
              
                  int userid = Convert.ToInt32(Session["userid"]);

                int companyid = Convert.ToInt32(Session["companyid"]);

          //      int fyid = Convert.ToInt32(Session["fid"]);
                //var fYear = db.FinancialYearMasters.Where(u => u.status == "Active" && u.CompanyId == companyid).FirstOrDefault();


                int fYears = Convert.ToInt32(Session["fid"]);
                int bankid1 = 0;
                int bankid2 = 0;
               // int CashId = 0;
                int ToId = 0;
                int FromId = 0;
         //       string ChequeNo = "";
                string entryType = "";
                var getDateRange = db.FinancialYearMasters.Where(d => d.fYearID == fYears).FirstOrDefault();
               
                var contra = new ReceiptPayment();

                contra.BranchId = BranchId;
                contra.CompanyId = companyid;
                contra.RPdate = DateTime.Today;
                contra.RPDatetime = DateTime.Now;

                var typeValue = collection["Cont"];


                if (typeValue == "1")
                {
                    var date= DateTime.ParseExact(collection["DateCTB"], dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    if (getDateRange.fYearID == 1 || getDateRange.fYearID == 2 || getDateRange.fYearID == 3 || getDateRange.fYearID == 4)
                    {
                        ViewBag.Error = "This Financial Year is out of scope of entry.";
                        return RedirectToAction("ShowAllContraVoucher", new { Err = "This Financial Year is out of scope of entry. " });


                    }


                    if (!(date >= getDateRange.sDate && date <= getDateRange.eDate))
                    {
                        ViewBag.Error = "Contra Date out of scope of " + getDateRange.Year + " Financial Year.";
                        return RedirectToAction("ShowAllContraVoucher", new { Err = "Contra Date out of scope of " + getDateRange.Year + " Financial Year." });
                       
                    }

                    //var checkMenu = db.MenuaccessUsers.Any(d => d.AssignedUserId == Createdby && d.Name == "SpecialPriviledge");
                    //var getVoucherLock = db.LockVouchers.Where(d => d.FinancialYear == fYears && d.Month == date.Month && d.BranchId == Branchid).FirstOrDefault();
                    //if (getVoucherLock != null)
                    //{
                    //    if (getVoucherLock.Contra == true)
                    //    {
                    //        ViewBag.Error = "The entry and update of Journal Voucher for the month of " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month) + " has been locked. Contact Head Office.";
                    //        return RedirectToAction("ShowAllContraVoucher", new { Err = "The entry and update of Journal Voucher for the month of " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month) + " has been locked. Contact Head Office." });

                    //    }
                    //}
                    var a = collection["CTBAmount"];
                    var b = collection["CTBAmount"].ToString();
                    var c = Convert.ToDecimal(b);
                    contra.TotalAmount = decimal.Parse(collection["CTBAmount"], CultureInfo.InvariantCulture);
                    contra.RPBankAmount = Convert.ToDecimal(collection["CTBAmount"], CultureInfo.InvariantCulture);
                    contra.RPdate = DateTime.ParseExact(collection["DateCTB"], dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    contra.RPDatetime = DateTime.ParseExact(collection["DateCTB"], dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    long CashId = db.LedgerMasters.Where(l => l.ledgerName == "CASH IN HAND").Select(l => l.LID).FirstOrDefault();

                  
           
                    FromId = Convert.ToInt32(CashId);
                    contra.ledgerId = FromId;  // From Id


                    bankid1 = Convert.ToInt32(collection["ToId"]);
                    contra.RPBankId = bankid1;  // To Id
                    contra.Remarks = Convert.ToString(collection["RemarksCTB"]);
                    entryType = "Cash To Bank";
                    contra.RPCashId = 0;
                    contra.RPCashAmount = 0;
                }
                if (typeValue == "2")
                {
                    var date = DateTime.ParseExact(collection["DateBTC"], dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    if (getDateRange.fYearID == 1 || getDateRange.fYearID == 2 || getDateRange.fYearID == 3 || getDateRange.fYearID == 4)
                    {
                        ViewBag.Error = "This Financial Year is out of scope of entry.";
                        return RedirectToAction("ShowAllContraVoucher", new { Err = "This Financial Year is out of scope of entry. "});

                      
                    }
                    if (!(date >= getDateRange.sDate && date <= getDateRange.eDate))
                    {
                        ViewBag.Error = "Contra Date out of scope of " + getDateRange.Year + " Financial Year.";
                        return RedirectToAction("ShowAllContraVoucher", new { Err = "Contra Date out of scope of " + getDateRange.Year + " Financial Year." });

                    }

                    long CashId = db.LedgerMasters.Where(l => l.ledgerName == "CASH IN HAND" ).Select(l => l.LID).FirstOrDefault();
                                  

                    bankid2 = Convert.ToInt32(collection["BankFromId"]);
                    contra.ledgerId = bankid2;   // From Id
                    contra.RPCashId = Convert.ToInt32(CashId);   //TO Id

                    contra.TotalAmount = Convert.ToDecimal(collection["BTCAmount"], CultureInfo.InvariantCulture);
                    contra.RPCashAmount = Convert.ToDecimal(collection["BTCAmount"], CultureInfo.InvariantCulture);

                    string ChequeNo = collection["chequeNoBTC"];
                    contra.chequeDate = DateTime.ParseExact(collection["chequeDateBTC"], dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    contra.RPdate = DateTime.ParseExact(collection["DateBTC"], dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    contra.RPDatetime = DateTime.ParseExact(collection["DateBTC"], dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    contra.chequeNo = ChequeNo;
                    CashId = Convert.ToInt32(InventoryLedgergroupId.Cash);
                    contra.Remarks = Convert.ToString(collection["RemarksBTC"]);
                    entryType = "Bank To Cash";

                    contra.RPBankId = 0;
                    contra.RPBankAmount = 0;

                }
                if (typeValue == "3")
                {
                    var date = DateTime.ParseExact(collection["DateBTB"], dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    if (getDateRange.fYearID == 1 || getDateRange.fYearID == 2 || getDateRange.fYearID == 3 || getDateRange.fYearID == 4)
                    {
                        ViewBag.Error = "This Financial Year is out of scope of entry.";
                        return RedirectToAction("ShowAllContraVoucher", new { Err = "This Financial Year is out of scope of entry. " });


                    }
                    if (!(date >= getDateRange.sDate && date <= getDateRange.eDate))
                    {
                        ViewBag.Error = "Contra Date out of scope of " + getDateRange.Year + " Financial Year.";
                        return RedirectToAction("ShowAllContraVoucher", new { Err = "Contra Date out of scope of " + getDateRange.Year + " Financial Year." });

                    }
                    bankid1 = Convert.ToInt32(collection["BankToFromId"]);
                    contra.ledgerId = bankid1;  // From Id

                    bankid2 = Convert.ToInt32(collection["BankCashToId"]);
                    contra.RPBankId = bankid2; //To Id


                    contra.TotalAmount = Convert.ToDecimal(collection["BTBAmount"], CultureInfo.InvariantCulture);
                    contra.RPBankAmount = Convert.ToDecimal(collection["BTBAmount"], CultureInfo.InvariantCulture);
                    string ChequeNo = collection["chequeNoBTB"];
                    contra.chequeDate = DateTime.ParseExact(collection["chequeDateBTB"], dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    contra.RPdate = DateTime.ParseExact(collection["DateBTB"], dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    contra.RPDatetime = DateTime.ParseExact(collection["DateBTB"], dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    contra.chequeNo = ChequeNo;
                    contra.Remarks = Convert.ToString(collection["RemarksBTB"]);
                    entryType = "Bank To Bank";

                    contra.RPCashId = 0;
                    contra.RPCashAmount = 0;
                }



                contra.transactionNo = transNo.ToString();

                string CTB = "Cash To Bank";

                string BTC = "Bank To Cash";

                string BTB = "Bank To Bank";


              //  contra.transactionType = InventoryConst.cns_Contra;
                var voucher = db.ReceiptPayments.Where(c =>( c.transactionType == CTB  || c.transactionType== BTC || c.transactionType== BTB) && c.CompanyId == companyid && c.BranchId == Branchid && c.UserId==userid).Max(c => (int?)c.VoucherNo) ?? 0;
                contra.VoucherNo = Convert.ToInt32(voucher) + 1;
                contra.RPType = InventoryConst.Cns_Payment;
                contra.UserId = userid;
                contra.transactionType = entryType;
                contra.fYearId = fYears;

             
                contra.CreatedBy = Createdby;
                db.ReceiptPayments.Add(contra);
                db.SaveChanges();
                return RedirectToAction("ShowAllContraVoucher", new { Msg ="Data Saved Successfully... "});
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
                return RedirectToAction("ShowAllContraVoucher", new { Err = InventoryMessage.InsertError });

            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Data  not Saved Successfully.......";
                return RedirectToAction("ShowAllContraVoucher", new { Err = InventoryMessage.InsertError });

            }
            catch (Exception exp)
            {
                return RedirectToAction("ShowAllContraVoucher", new { Err = InventoryMessage.InsertError });

            }
        }


        [HttpGet]
        public ActionResult EditContra(int id)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int userid = Convert.ToInt32(Session["userid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            //var fYear = db.FinancialYearMasters.Where(u => u.status == "Active" && u.CompanyId == companyid).FirstOrDefault();

            int fyid = Convert.ToInt32(Session["fid"]);

            string CTB = "Cash To Bank";

                string BTC = "Bank To Cash";

                string BTB = "Bank To Bank";

            var contra = db.ReceiptPayments.Where(d => d.Id == id).FirstOrDefault();
            var cmv = new ContraModelView();
            cmv.Id = contra.Id;
            if (contra.transactionType == CTB)
            {
                cmv.Cont = 1;
                cmv.CTBAmount =contra.RPBankAmount;
                cmv.dateCTB = contra.RPdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                cmv.ToCTBId = contra.RPBankId;
                cmv.FromCTBId = contra.ledgerId;
                cmv.RemarksCTB = contra.Remarks ;

            }
            if (contra.transactionType == BTC)
            {
                cmv.Cont = 2;
                cmv.BTCAmount = contra.RPCashAmount;
                cmv.dateBTC = contra.RPdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                cmv.chequeDateBTC = contra.chequeDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                cmv.chequeNoBTC = contra.chequeNo;
                cmv.FromBTCId = contra.ledgerId;
                cmv.ToBTCId = contra.RPCashId;
                cmv.RemarksBTC = contra.Remarks;

            }
            if (contra.transactionType == BTB)
            {
                cmv.Cont = 3;
                cmv.BTBAmount = contra.RPBankAmount;
                cmv.dateBTB = contra.RPdate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                cmv.chequeDateBTB = contra.chequeDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                cmv.chequeNoBTB = contra.chequeNo;
                cmv.FromBTBId = contra.ledgerId;
                cmv.ToBTBId = contra.RPBankId;
                cmv.RemarksBTB = contra.Remarks;

            }
            
            // ViewBag.toBank = db.LedgerMasters.Where(l => l.groupID == 1  && l.BranchId == Branchid && l.CompanyId == companyid && l.UserId==userid).ToList();

            ViewBag.toBank = db.Banks.Where(l => l.CompanyId == companyid && l.UserId == userid).Select(l => new { Lid = l.LId, ledgerName = l.Type == null || l.Type == "" ? l.Name : l.Name + "(" + l.Type + ")" }).ToList();
            ViewBag.FromBank = db.Banks.Where(l =>  l.CompanyId == companyid && l.UserId == userid).Select(l => new { Lid = l.LId, ledgerName = l.Type == null || l.Type == "" ? l.Name : l.Name + "(" + l.Type + ")" }).ToList();
            var branchList = new List<Taxname>();
            var branches = db.BranchMasters.Where(d => d.CompanyId == companyid).Select(d => new Taxname { Id = d.Id, Name = d.Name });
            branchList.AddRange(branches);
            ViewBag.Branch = branchList;
            //ViewBag.FromBank = db.LedgerMasters.Where(l => l.groupID == 42 &&  l.BranchId == Branchid && l.CompanyId == companyid && l.UserId==userid).ToList();
            cmv.CompanyId =(int) Branchid;
            cmv.BranchId = contra.BranchId ?? 0; 
            cmv.fYearId = contra.fYearId ??0;
            return View(cmv);
        }

        [HttpPost]
        public ActionResult EditContra(ContraModelView cmv)
        {
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            int Createdby = Convert.ToInt32(Session["Createdid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int fyid = Convert.ToInt32(Session["fid"]);
            string CTB = "Cash To Bank";

            string BTC = "Bank To Cash";

            string BTB = "Bank To Bank";
            try{
                var contra = db.ReceiptPayments.Find(cmv.Id);

                if (cmv.Cont == 1)
                {
                    contra.RPBankAmount = cmv.CTBAmount;
                    contra.RPdate = DateTime.ParseExact(cmv.dateCTB, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    contra.RPBankId = cmv.ToCTBId ;
                    contra.ledgerId = cmv.FromCTBId;
                    contra.Remarks = cmv.RemarksCTB;

                    contra.RPCashId = 0;
                    contra.RPCashAmount = 0;
                    contra.TotalAmount = cmv.CTBAmount;
                    contra.transactionType = CTB;
                }
                if (cmv.Cont == 2)
                {
                    contra.RPCashAmount = cmv.BTCAmount;
                    contra.RPdate = DateTime.ParseExact(cmv.dateBTC, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    contra.RPCashId = cmv.ToBTCId;
                    contra.ledgerId = cmv.FromBTCId;
                    contra.Remarks = cmv.RemarksBTC;
                    contra.chequeNo = cmv.chequeNoBTC;
                    contra.chequeDate = DateTime.ParseExact(cmv.chequeDateBTC, dateFormat, CultureInfo.CreateSpecificCulture(culture));

                    contra.RPBankId = 0;
                    contra.RPBankAmount = 0;
                    contra.TotalAmount = cmv.BTCAmount;
                    contra.transactionType = BTC;
                }

                if (cmv.Cont == 3)
                {
                    contra.RPBankAmount = cmv.BTBAmount;
                    contra.RPdate = DateTime.ParseExact(cmv.dateBTB, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    contra.RPBankId = cmv.ToBTBId;
                    contra.ledgerId = cmv.FromBTBId;
                    contra.Remarks = cmv.RemarksBTB;
                    contra.chequeNo = cmv.chequeNoBTB;
                    contra.chequeDate = DateTime.ParseExact(cmv.chequeDateBTB, dateFormat, CultureInfo.CreateSpecificCulture(culture));

                    contra.RPCashId = 0;
                    contra.RPCashAmount = 0;
                    contra.TotalAmount = cmv.BTBAmount;
                    contra.transactionType = BTB;
                }
                if (cmv.BranchId == null)
                {
                }
                else
                {
                    contra.BranchId = cmv.BranchId;
                }
                var getDateRange = db.FinancialYearMasters.Where(d => d.fYearID == cmv.fYearId).FirstOrDefault();
                if (getDateRange.fYearID == 1 || getDateRange.fYearID == 2 || getDateRange.fYearID == 3 || getDateRange.fYearID == 4)
                {
                    ViewBag.Error = "This Financial Year is out of scope of entry.";
                    return RedirectToAction("ShowAllContraVoucher", new { Err = "This Financial Year is out of scope of entry. " });

                }

                if (!(contra.RPdate >= getDateRange.sDate && contra.RPdate <= getDateRange.eDate))
                {
                    return RedirectToAction("ShowAllContraVoucher", new { Err = "Voucher Date out of scope of " + getDateRange.Year + " Financial Year." });

                }
                var checkMenu = db.MenuaccessUsers.Any(d => d.AssignedUserId == Createdby && d.Name == "SpecialPriviledge");
                //var getVoucherLock = db.LockVouchers.Where(d => d.FinancialYear == fyid && d.Month == contra.RPdate.Month && d.BranchId == Branchid).FirstOrDefault();
                //if (getVoucherLock != null)
                //{
                //    if (getVoucherLock.Contra == true)
                //    {
                //        ViewBag.Error = "The entry and update of Journal Voucher for the month of " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(contra.RPdate.Month) + " has been locked. Contact Head Office.";
                //        return RedirectToAction("ShowAllContraVoucher", new { Err = "The entry and update of Journal Voucher for the month of " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(contra.RPdate.Month) + " has been locked. Contact Head Office." });

                //    }
                //}

                db.SaveChanges();
                return RedirectToAction("ShowAllContraVoucher", new { Msg = "Data Saved Successfully... " });
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
                return RedirectToAction("ShowAllContraVoucher", new { Err = InventoryMessage.InsertError });

            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Data  not Saved Successfully.......";
                return RedirectToAction("ShowAllContraVoucher", new { Err = InventoryMessage.InsertError });

            }
            catch (Exception exp)
            {
                return RedirectToAction("ShowAllContraVoucher", new { Err = InventoryMessage.InsertError });

            }


        }
        public ActionResult Delete(int id)
        {
            try
            {
                int Fyid = Convert.ToInt32(Session["fid"]);
                var getDateRange = db.FinancialYearMasters.Where(d => d.fYearID == Fyid).FirstOrDefault();
                if (getDateRange.fYearID == 1 || getDateRange.fYearID == 2 || getDateRange.fYearID == 3 || getDateRange.fYearID == 4)
                {
                    ViewBag.Error = "This Financial Year is out of scope of entry.";
                    return RedirectToAction("ShowAllContraVoucher", new { Err = "This Financial Year is out of scope of Delete. " });

                }
                int userid = Convert.ToInt32(Session["userid"]);
                var result = db.ReceiptPayments.Find(id);
                db.ReceiptPayments.Remove(result);
                db.SaveChanges();
                return RedirectToAction("ShowAllContraVoucher", new { Msg = "Contra Voucher deleted Successfully...." });
            }
            catch
            {


                return RedirectToAction("ShowAllContraVoucher", new { Err = InventoryMessage.Delte });
            }

        }

        #endregion
    }
}

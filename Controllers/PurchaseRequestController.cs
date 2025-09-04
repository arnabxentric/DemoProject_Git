using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Globalization;
using System.Transactions;
namespace XenERP.Controllers
{
    public class PurchaseRequestController : Controller
    {
        private InventoryEntities db = new InventoryEntities();
        private MasterClasses mc = new MasterClasses();
        private TransactionClasses tc = new TransactionClasses();
        //
        // GET: /PurchaseOrder/
        [SessionExpire]
        //
        // GET: /PurchaseRequest/

        public ActionResult Index()
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            var branches = db.BranchMasters.Where(d => d.CompanyId == companyid).Select(d => new Taxname {Id= d.Id,Name= d.Name }).ToList();
            ViewBag.branches = branches;
            ViewBag.Branchid = Branchid;
            List<RequestModelView> requestDetail = new List<RequestModelView>();
            if (Branchid == 0)
            {
                 requestDetail = db.PurchaseRequestDetails.Where(d => d.PurchaseRequest.CompanyId == companyid && d.TaxType==1).Select(d => new RequestModelView { RequestId=d.PurchaseRequestId,RequestDetailId=d.Id, RequestNo = d.PurchaseRequest.RequestNo, RequestDate = d.PurchaseRequest.RequestDate, ExpectedDate = d.PurchaseRequest.ExpectedDate, Branch =d.PurchaseRequest.BranchId, Product = d.Product.Name, Quantity = d.Quantity, Unit = d.UOM.Description,Status=d.BarCode }).ToList();
             
            }
            else
            {
                 requestDetail = db.PurchaseRequestDetails.Where(d => d.PurchaseRequest.CompanyId == companyid && d.PurchaseRequest.BranchId == Branchid && d.TaxType == 1).Select(d => new RequestModelView { RequestId = d.PurchaseRequestId, RequestDetailId = d.Id, RequestNo = d.PurchaseRequest.RequestNo, RequestDate = d.PurchaseRequest.RequestDate, ExpectedDate = d.PurchaseRequest.ExpectedDate, Branch = d.PurchaseRequest.BranchId, Product = d.Product.Name, Quantity = d.Quantity, Unit = d.UOM.Description, Status = d.BarCode }).ToList();
                
            }
            
            return View(requestDetail);
        }

        public ActionResult HOActivityRequest()
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            var branches = db.BranchMasters.Where(d => d.CompanyId == companyid).Select(d => new Taxname { Id = d.Id, Name = d.Name }).ToList();
            ViewBag.branches = branches;
            ViewBag.Branchid = Branchid;
            List<RequestModelView> requestDetail = new List<RequestModelView>();
            if (Branchid == 0)
            {
                requestDetail = db.PurchaseRequestDetails.Where(d => d.PurchaseRequest.CompanyId == companyid && d.TaxType >= 2).Select(d => new RequestModelView { RequestId = d.PurchaseRequestId, RequestDetailId = d.Id, RequestNo = d.PurchaseRequest.RequestNo, RequestDate = d.PurchaseRequest.RequestDate, ExpectedDate = d.PurchaseRequest.ExpectedDate, Branch = d.PurchaseRequest.BranchId, Product = d.Product.Name, Quantity = d.Quantity, Unit = d.UOM.Description, Status = d.BarCode }).ToList();

            }
            else
            {
                requestDetail = db.PurchaseRequestDetails.Where(d => d.PurchaseRequest.CompanyId == companyid && d.PurchaseRequest.BranchId == Branchid && d.TaxType >= 2).Select(d => new RequestModelView { RequestId = d.PurchaseRequestId, RequestDetailId = d.Id, RequestNo = d.PurchaseRequest.RequestNo, RequestDate = d.PurchaseRequest.RequestDate, ExpectedDate = d.PurchaseRequest.ExpectedDate, Branch = d.PurchaseRequest.BranchId, Product = d.Product.Name, Quantity = d.Quantity, Unit = d.UOM.Description, Status = d.BarCode }).ToList();

            }

            return View(requestDetail);
        }
    

        [SessionExpire]
        public ActionResult Create(long? id = 0)
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            var dateFormat = Session["DateFormat"].ToString();
            var culture = Session["DateCulture"].ToString();

            ViewBag.Branch = Branchid;
            //   System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            ViewBag.ddlWarehouses = warehouses;
            var taxes = db.Taxes.Where(d => (d.CompanyId == companyid)).Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate }).ToList();
            ViewBag.TaxSingle = taxes;
            var paymentTerms = db.PaymentTerms.Where(d => (d.CompanyId == companyid)).Select(d => new { Id = d.Id, PaymentTermDescription = d.PaymentTermDescription }).ToList();
            ViewBag.PaymentTerms = paymentTerms;
            var Taxcomponents = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid)).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            ViewBag.Taxcomponents = Taxcomponents;

            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();

            if (id == 0)
            {
                PurchaseRequestModelView pomv = new PurchaseRequestModelView();
                pomv.RequestDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.ExpectedDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
             

                return View(pomv);
            }
            else
            {
                PurchaseRequest po = db.PurchaseRequests.Find(id);
                if (po == null)
                {
                    return HttpNotFound();
                }
                PurchaseRequestModelView pomv = new PurchaseRequestModelView();
                List<PurchaseRequestDetailModelView> podmvList = new List<PurchaseRequestDetailModelView>();

                pomv.Id = po.Id;
                pomv.RequestNo = po.RequestNo;
                pomv.SerialNo = po.SerialNo;
                pomv.RequestDate = po.RequestDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.ExpectedDate = po.ExpectedDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.Comments = po.Comments;
                pomv.Status = po.Status;
                pomv.FinancialYearId = po.FinancialYearId;
                pomv.UserId = po.UserId;
                pomv.BranchId = po.BranchId;
                pomv.CompanyId = po.CompanyId;
                pomv.Status = po.Status;
                pomv.CreatedBy = po.CreatedBy;
                pomv.CreatedOn = po.CreatedOn;
                pomv.ModifiedBy = po.ModifiedBy;
                pomv.ModifiedOn = po.ModifiedOn;
               
                var podlist = db.PurchaseRequestDetails.Where(p => p.PurchaseRequestId == po.Id).ToList();
                foreach (var pod in podlist)
                {
                    var podmv = new PurchaseRequestDetailModelView();
                    podmv.PurchaseOrderId = pod.PurchaseRequestId;
                    podmv.ItemId = pod.ItemId;
                    podmv.ItemCode = pod.Product.Code;
                    podmv.ItemName = pod.Product.Name;
                    podmv.BarCode = pod.BarCode;
                    podmv.Description = pod.Description;
                    podmv.Quantity = pod.Quantity;
                    podmv.AccountId = pod.AccountId;
                    podmv.UnitId = pod.UnitId;
                    podmv.UnitName = pod.UOM.Code;
                    podmv.Price = pod.Price;
                    podmv.CurrencyRate = pod.CurrencyRate;
                    podmv.TaxId = pod.TaxId;
                    podmv.TaxName = pod.Tax.Name + '(' + pod.Tax.Rate + "%)";
                    podmv.TaxPercent = pod.TaxPercent;
                    podmv.TaxAmount = pod.TaxAmount;
                    podmv.TotalAmount = pod.TotalAmount;
                    podmv.TaxType = pod.TaxType;
                    podmv.UnitIdSecondary = pod.UnitIdSecondary;
                    podmv.UnitSecondaryName = pod.UOM1.Code;
                    podmv.SecUnitId = pod.SecUnitId;
                    if (podmv.SecUnitId != null)
                        podmv.SecUnitName = pod.UOM2.Code;
                    else
                        podmv.SecUnitName = null;
                    podmv.UnitFormula = pod.UnitFormula;
                    podmv.SecUnitFormula = pod.SecUnitFormula;
                    podmv.TotalQuantity = pod.Quantity * pod.UnitFormula;
                    podmvList.Add(podmv);
                }
                ViewBag.podmvList = podmvList;
                return View(pomv);
            }
        }

        //
        // POST: /PurchaseOrder/Create
        [SessionExpire]
        [HttpPost]
        public ActionResult Create(FormCollection poCollection)
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt32(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            string dateFormat = Session["DateFormat"].ToString();
            long Createdby = Convert.ToInt32(Session["Createdid"]);
            var culture = Session["DateCulture"].ToString();
            //  System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            string currencyval = string.Empty;
          
            var paymentTerms = db.PaymentTerms.Where(d => (d.CompanyId == companyid)).Select(d => new { Id = d.Id, PaymentTermDescription = d.PaymentTermDescription }).ToList();
            ViewBag.PaymentTerms = paymentTerms;
           
            ViewBag.Branch = Branchid;

            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();

            PurchaseRequestModelView pomv = new PurchaseRequestModelView();
            List<PurchaseRequestDetailModelView> podmvList = new List<PurchaseRequestDetailModelView>();
          
            pomv.Id = Convert.ToInt64(poCollection["Id"]);
            pomv.RequestNo = poCollection["RequestNo"];
            pomv.Comments = poCollection["Comments"];
            pomv.RequestDate = Convert.ToString(poCollection["RequestDate"]);
            pomv.ExpectedDate = Convert.ToString(poCollection["ExpectedDate"]);
            pomv.Status = poCollection["Status"];
           
            var count = poCollection["producthide"].Split(',').Length;
            for (int i = 0; i < count; i++)
            {
                PurchaseRequestDetailModelView podmv = new PurchaseRequestDetailModelView();
                podmvList.Add(podmv);
            }
            foreach (var key in poCollection.AllKeys)
            {

                string[] value = poCollection[key].Split(',');
                var lt = value.Length;
                switch (key)
                {
                    case "producthide":
                        for (int i = 0; i < lt; i++)
                        {

                            podmvList[i].ItemCode = value[i];
                        }
                        break;

                    case "product":
                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].ItemId = Convert.ToInt32(value[i]);
                        }
                        break;
                    case "productcodehide":
                        for (int i = 0; i < lt; i++)
                        {

                            podmvList[i].ItemName = value[i];
                        }
                        break;
                    case "barcodehide":
                        for (int i = 0; i < lt; i++)
                        {

                            podmvList[i].BarCode = value[i];
                        }
                        break;
                
                    case "quantityhide":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].Quantity = Convert.ToDecimal(value[i]);
                        }
                        break;

                    case "unitpricehide":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].Price = Convert.ToDecimal(value[i]);
                        }
                        break;
                    
                    case "uom":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].UnitIdSecondary = Convert.ToInt32(value[i]);
                        }
                        break;
                    case "uomhide":

                        for (int i = 0; i < lt; i++)
                        {

                            podmvList[i].UnitSecondaryName = value[i];
                        }
                        break;
                    case "baseuom":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].UnitId = Convert.ToInt32(value[i]);
                        }
                        break;
                    case "baseuomhide":

                        for (int i = 0; i < lt; i++)
                        {

                            podmvList[i].UnitName = value[i];
                        }
                        break;
                    case "secuom":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].SecUnitId = Convert.ToInt32(value[i]);
                        }
                        break;
                    case "secuomhide":

                        for (int i = 0; i < lt; i++)
                        {

                            podmvList[i].SecUnitName = value[i];
                        }
                        break;
                    case "ofhide":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].UnitFormula = Convert.ToDecimal(value[i]);
                        }
                        break;
                    case "secofhide":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].SecUnitFormula = Convert.ToDecimal(value[i]);
                        }
                        break;
                   
                    case "descriptionhide":

                        for (int i = 0; i < lt; i++)
                        {

                            podmvList[i].Description = value[i];
                        }
                        break;
                  
                }
            }
           
            var requestDate = DateTime.ParseExact(pomv.RequestDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var expectedDate = DateTime.ParseExact(pomv.ExpectedDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            

            //purchase order details model view validation
            foreach (var podmv in podmvList)
            {
                if (podmv.ItemId != 0 && podmv.Quantity > 0)
                {
                    //do nothing
                }
                else
                {
                    if (!(podmv.ItemId == 0 && podmv.Quantity == 0 ))
                    {
                        var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                        ViewBag.ddlWarehouses = warehouses;

                        ViewBag.podmvList = podmvList;

                        return View(pomv);
                    }

                }
            }


            // define our transaction scope
            var scope = new System.Transactions.TransactionScope(
                // a new transaction will always be created
                TransactionScopeOption.RequiresNew,
                // we will allow volatile data to be read during transaction
                new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }
            );
            try
            {
                // use the scope we just defined
                using (scope)
                {
                    // create a new db context
                    using (var ctx = new InventoryEntities())
                    {
                        //Check Insert or Update
                        if (pomv.Id == 0)
                        {
                            //int countpo = db.PurchaseOrders.Where(p => p.CompanyId == companyid && p.BranchId == Branchid).Count();
                            //pomv.NO = tc.GenerateCode("PO", countpo);

                            var fyear = db.FinancialYearMasters.FirstOrDefault(f => f.fYearID == Fyid).Year;
                            var fs = fyear.Substring(2, 2);
                            var es = fyear.Substring(7, 2);
                            fyear = fs + "-" + es;
                            int countpo = 1;

                            //&& p.BranchId == Branchid
                            if (db.PurchaseRequests.Where(p => p.CompanyId == companyid && p.BranchId==Branchid  && p.FinancialYearId == Fyid).Count() != 0)
                            {
                                countpo = (int)db.PurchaseRequests.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.FinancialYearId == Fyid).Max(p => p.SerialNo) + 1;
                            }
                            var getBranchCode = db.BranchMasters.Where(p => p.CompanyId == companyid && p.Id == Branchid).Select(p =>  p.Code).FirstOrDefault();

                            pomv.RequestNo = getBranchCode + "/" + fyear + "/" + countpo;//tc.GenerateCode(fyear, countpo);
                           
                            //Insert into purchaseorder table
                            PurchaseRequest po = new PurchaseRequest();
                            po.RequestNo = pomv.RequestNo;
                            po.SerialNo = countpo;
                            po.RequestDate = DateTime.Today; ;
                            po.ExpectedDate = expectedDate;
                            po.Comments = pomv.Comments;
                            po.FinancialYearId = Fyid;
                            po.CreatedBy = Createdby;
                            po.CreatedOn = DateTime.Now;
                            po.UserId = userid;
                            po.BranchId = Branchid;
                            po.CompanyId = companyid;
                            po.Status = "Request";
                          
                            db.PurchaseRequests.Add(po);

                            db.SaveChanges();
                            pomv.Status = po.Status;
                            var po1 = db.PurchaseRequests.Find(po.Id);
                            pomv.Id = po.Id;

                            foreach (var podmv in podmvList)
                            {
                                if (podmv.ItemId != 0 && podmv.Quantity > 0 )
                                {


                                    PurchaseRequestDetail pod = new PurchaseRequestDetail();
                                    pod.PurchaseRequestId = po.Id;
                                    pod.ItemId = podmv.ItemId;
                                    pod.BarCode = "Request";
                                    pod.Description = podmv.Description;
                                    pod.Quantity = podmv.Quantity;
                                    pod.AccountId = 12;
                                    pod.UnitId = podmv.UnitId;
                                    pod.Price = 0;
                                    pod.CurrencyRate =1;
                                    pod.UnitIdSecondary = podmv.UnitIdSecondary;
                                    pod.SecUnitId = podmv.SecUnitId;
                                    pod.UnitFormula = podmv.UnitFormula;
                                    pod.SecUnitFormula = podmv.SecUnitFormula;
                                    pod.TaxType = 1;
                                    pod.TaxId = 1;
                                    pod.TaxPercent = 0;
                                    pod.TaxAmount = 0;
                                    pod.TotalAmount =0;
                                    db.PurchaseRequestDetails.Add(pod);
                                }
                            }
                          
                        }
                        else
                        {
                            //Update Purchase Request
                           
                            var po = db.PurchaseRequests.Find(pomv.Id);

                        // po.RequestNo = pomv.RequestNo;
                        //    po.SerialNo = countpo;
                        //    po.RequestDate = DateTime.Today; ;
                            po.ExpectedDate = expectedDate;
                            po.Comments = pomv.Comments;
                            po.ModifiedBy = Createdby;
                            po.ModifiedOn = DateTime.Now;

                            var podOldRecords = db.PurchaseRequestDetails.Where(p => p.PurchaseRequestId == po.Id).ToList();
                            foreach (var podOld in podOldRecords)
                            {
                                db.PurchaseRequestDetails.Remove(podOld);
                            }

                            foreach (var podmv in podmvList)
                            {
                                if (podmv.ItemId != 0 && podmv.Quantity > 0)
                                {

                                   
                                    PurchaseRequestDetail pod = new PurchaseRequestDetail();
                                    pod.PurchaseRequestId = po.Id;
                                    pod.ItemId = podmv.ItemId;
                                    pod.BarCode = podmv.BarCode;
                                    pod.Description = podmv.Description;
                                    pod.Quantity = podmv.Quantity;
                                    pod.AccountId = 12;
                                    pod.UnitId = podmv.UnitId;
                                    pod.Price = 0;
                                    pod.CurrencyRate = 1;
                                    pod.UnitIdSecondary = podmv.UnitIdSecondary;
                                    pod.SecUnitId = podmv.SecUnitId;
                                    pod.UnitFormula = podmv.UnitFormula;
                                    pod.SecUnitFormula = podmv.SecUnitFormula;

                                    pod.TaxId = 1;
                                    pod.TaxPercent = 0;
                                    pod.TaxAmount = 0;
                                    pod.TotalAmount = 0;
                                    db.PurchaseRequestDetails.Add(pod);
                                }
                            }

                        }

                    }
                    db.SaveChanges();
                    scope.Complete();
                }

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

                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                ViewBag.ddlWarehouses = warehouses1;

                ViewBag.podmvList = podmvList;

                ViewBag.Message = "Error";
                return View(pomv);
            }
            catch (DataException dex)
            {
                //Log the error (add a variable name after DataException)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                ViewBag.ddlWarehouses = warehouses1;

                ViewBag.podmvList = podmvList;

                ViewBag.Message = "Error";
                return View(pomv);
            }
            catch (Exception exp)
            {
                var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                ViewBag.ddlWarehouses = warehouses1;

                ViewBag.podmvList = podmvList;

                ViewBag.Message = "Error";
                return View(pomv);
            }

            var warehouses2 = mc.getDdlWarehouses(companyid, Branchid);
            ViewBag.ddlWarehouses = warehouses2;

            ViewBag.podmvList = podmvList;
            ViewBag.Message = "You have successfully saved Purchase Request " + pomv.RequestNo;

            return View(pomv);
        }


    }
}

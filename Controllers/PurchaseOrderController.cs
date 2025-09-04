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
using Rotativa;

namespace XenERP.Controllers
{
    public class PurchaseOrderController : Controller
    {
        private InventoryEntities db = new InventoryEntities();
        private MasterClasses mc = new MasterClasses();
        private TransactionClasses tc = new TransactionClasses();
        //
        // GET: /PurchaseOrder/
        [SessionExpire]
        public ActionResult Index()
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            ViewBag.BaseCurrency = basecurrency.CurrencyCode;
            if (Branchid == 0)
            {
                var dataPo = db.PurchaseOrders.Where(p => p.CompanyId == companyid && p.FinancialYearId == Fyid).OrderByDescending(d => d.Date).ThenByDescending(d => d.InvoiceNo).Take(100).OrderBy(d => d.Date).ThenBy(d => d.InvoiceNo).ToList();
                return View(dataPo);
            }
            else
                return View(db.PurchaseOrders.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.FinancialYearId == Fyid).OrderByDescending(d => d.Date).ThenByDescending(d => d.InvoiceNo).Take(100).OrderBy(d => d.Date).ThenBy(d => d.InvoiceNo).ToList());
        }

        [SessionExpire]
        public ActionResult IndexSearch(string DateFrom, string DateTo)
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            long Createdby = Convert.ToInt32(Session["Createdid"]);
            var culture = Session["DateCulture"].ToString();
            var dateFormat = Session["DateFormat"].ToString();
            var DtFrm = DateTime.ParseExact(DateFrom, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var DtTo = DateTime.ParseExact(DateTo, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            ViewBag.BaseCurrency = basecurrency.CurrencyCode;
            if (Branchid == 0)
                return View(db.PurchaseOrders.Where(p => p.CompanyId == companyid && p.FinancialYearId == Fyid && p.Date >= DtFrm && p.Date <= DtTo).OrderBy(d => d.Date).ThenBy(d => d.InvoiceNo).ToList());
            else
                return View(db.PurchaseOrders.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.FinancialYearId == Fyid && p.Date >= DtFrm && p.Date <= DtTo).OrderBy(d => d.Date).ThenBy(d => d.InvoiceNo).ToList());
        }

        [SessionExpire]
        public ActionResult ApprovedPO()
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            ViewBag.BaseCurrency = basecurrency.CurrencyCode;
            if (Branchid == 0)
                return View(db.PurchaseOrders.Where(p => p.CompanyId == companyid && p.FinancialYearId == Fyid && p.ApproveStatus==1).ToList());
            else
                return View(db.PurchaseOrders.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.FinancialYearId == Fyid && p.ApproveStatus == 1).ToList());
        }
        //
        // GET: /PurchaseOrder/Details/5
        [SessionExpire]
        public ActionResult Details(long id = 0)
        {
            PurchaseOrder purchaseorder = db.PurchaseOrders.Find(id);
            if (purchaseorder == null)
            {
                return HttpNotFound();
            }
            return View(purchaseorder);
        }


        public ActionResult CreateFromRequest(string Requests)
        {
            return View();
        }



        //
        // GET: /PurchaseOrder/Create
        [SessionExpire]
        public ActionResult Create(long? id=0, string Requests=null)
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
            var taxes = db.Taxes.Where(d => (d.CompanyId == companyid )).Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate }).ToList();
            ViewBag.TaxSingle = taxes;
            var paymentTerms = db.PaymentTerms.Where(d => (d.CompanyId == companyid )).Select(d => new { Id = d.Id, PaymentTermDescription = d.PaymentTermDescription }).ToList();
            ViewBag.PaymentTerms = paymentTerms;
            var Taxcomponents = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid)).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            ViewBag.Taxcomponents = Taxcomponents;

            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid ).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();



            if (Requests != null)
            {
                string[] value = Requests.Split(',');
                int[] myInts = Array.ConvertAll(value, int.Parse);
                var getRDetails = (from p in db.PurchaseRequestDetails
                                    where myInts.Any(val => p.Id == val)
                                    select new PurchaseRequestDetailModelView { ItemId = p.ItemId, Description = p.Description, Quantity = p.Quantity, TotalQuantity = p.Quantity, SecUnitFormula = p.SecUnitFormula, SecUnitId = p.SecUnitId, UnitFormula = p.UnitFormula, UnitId = p.UnitId, UnitIdSecondary = p.UnitIdSecondary, ItemCode = p.Product.Code, ItemName = p.Product.Name, UnitName = p.UOM.Code, UnitSecondaryName = p.UOM1.Code, SecUnitName = p.UOM2.Code, TaxId = 1, TaxName = "No Tax", Price = 0, TotalAmount = 0,BranchId=p.PurchaseRequest.BranchId,ExpectedDate=p.PurchaseRequest.ExpectedDate }).ToList();

                var getRequst = getRDetails.Select(d =>new { d.BranchId,d.ExpectedDate }).FirstOrDefault();
                var defaultwarehouse = db.Warehouses.Where(w => w.Companyid == companyid && w.Branchid == getRequst.BranchId).FirstOrDefault();

                PurchaseOrderModelView pomv = new PurchaseOrderModelView();
                List<PurchaseOrderDetailModelView> podmvList = new List<PurchaseOrderDetailModelView>();

                pomv.Date = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.DueDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.RequestDetailId = Requests;
                pomv.CurrencyId = basecurrency.CurrencyId;
                pomv.BaseCurrencyCode = basecurrency.CurrencyCode;
                pomv.Currencyrate = 1;
                pomv.TransactionCurrency = basecurrency.CurrencyId;
                pomv.TransactionCurrencyCode = basecurrency.CurrencyCode;
                pomv.Date = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.DueDate = getRequst.ExpectedDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.WarehouseId = defaultwarehouse.Id;
                pomv.DeliveryName = defaultwarehouse.ContactName;
                pomv.StreetPoBox = defaultwarehouse.Address;
                pomv.Suburb = defaultwarehouse.Suburb;
                pomv.City = defaultwarehouse.Town;
                pomv.StateRegion = defaultwarehouse.State;
                pomv.Country = "India";
                pomv.PostalCode = defaultwarehouse.PIN;

                ViewBag.Request = "Request";
                                
                foreach (var pod in getRDetails)
                {
                    var podmv = new PurchaseOrderDetailModelView();
                    podmv.PurchaseOrderId = pod.PurchaseOrderId;
                    // podmv.ItemId = pod.ItemId;
                    podmv.ItemCode = "";
                    podmv.ItemName = "";
                    podmv.BarCode = pod.BarCode;
                    podmv.Description = pod.ItemName;
                    podmv.Quantity = pod.Quantity;
                    podmv.AccountId = pod.AccountId;
                    podmv.UnitId = pod.UnitId;
                    podmv.UnitName = pod.UnitName;
                    podmv.Price = 0;
                    podmv.CurrencyRate = pod.CurrencyRate;
                    podmv.TaxId = pod.TaxId;
                    podmv.TaxName = pod.TaxName;
                    podmv.TaxPercent = pod.TaxPercent;
                    podmv.TaxAmount = pod.TaxAmount;
                    podmv.TotalAmount = pod.TotalAmount;

                    podmv.UnitIdSecondary = pod.UnitIdSecondary;
                    podmv.UnitSecondaryName = pod.UnitSecondaryName;
                    podmv.SecUnitId = pod.SecUnitId;
                    if (podmv.SecUnitId != null)
                        podmv.SecUnitName = pod.SecUnitName;
                    else
                        podmv.SecUnitName = null;
                    podmv.UnitFormula = pod.UnitFormula;
                    podmv.SecUnitFormula = pod.SecUnitFormula;
                    podmv.TotalQuantity = pod.Quantity ;
                    podmvList.Add(podmv);
                }
                ViewBag.podmvList = podmvList;
                return View(pomv);
            }


            if (id == 0)
            {
                PurchaseOrderModelView pomv = new PurchaseOrderModelView();
                pomv.Date = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.DueDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.CurrencyId = basecurrency.CurrencyId;
                pomv.BaseCurrencyCode = basecurrency.CurrencyCode;
                string countryname = string.Empty;
                var defaultwarehouse = db.Warehouses.Where(w => w.Companyid == companyid && w.Branchid == Branchid).FirstOrDefault();
                if (defaultwarehouse.Country != null)
                {
                    int c1 = Convert.ToInt32(defaultwarehouse.Country);
                    countryname = db.Countries.Where(c => c.CountryId == c1).Select(c => c.Country1).FirstOrDefault();
                }
                ViewBag.Id = defaultwarehouse.Id;
                ViewBag.ContactName = defaultwarehouse.ContactName;
                ViewBag.Address = defaultwarehouse.Address;
                ViewBag.Suburb = defaultwarehouse.Suburb;
                ViewBag.Town = defaultwarehouse.Town;
                ViewBag.State = defaultwarehouse.State;
                ViewBag.Country = countryname;
                ViewBag.PIN = defaultwarehouse.PIN;
                
                return View(pomv);
            }
            else
            {
                PurchaseOrder po = db.PurchaseOrders.Find(id);
                if (po == null)
                {
                    return HttpNotFound();
                }
                PurchaseOrderModelView pomv = new PurchaseOrderModelView();
                List<PurchaseOrderDetailModelView> podmvList = new List<PurchaseOrderDetailModelView>();
               
                pomv.Id = po.Id;
                pomv.NO = po.NO;
                pomv.RequestDetailId = po.RequestDetailId;
                pomv.SupplierCode = po.Supplier.Code;
                pomv.SupplierName =  po.Supplier.Name;
                pomv.SupplierId = po.SupplierId;
                pomv.Reference = po.Reference;
                pomv.ApproveStatus = po.ApproveStatus;
                pomv.WarehouseId = po.WarehouseId;
                pomv.CurrencyId = po.CurrencyId;
                pomv.BaseCurrencyCode = po.Currency.ISO_4217;
                pomv.Currencyrate = po.Currencyrate;
                pomv.TransactionCurrency = po.TransactionCurrency;
                pomv.TransactionCurrencyCode = po.Currency1.ISO_4217;
                pomv.Date = po.Date.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.DueDate = po.DueDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.DeliveryName = po.DeliveryName;
                pomv.StreetPoBox = po.StreetPoBox;
                pomv.Suburb = po.Suburb;
                pomv.City = po.City;
                pomv.StateRegion = po.StateRegion;
                pomv.Country = po.Country;
                pomv.PostalCode = po.PostalCode;
                pomv.TaxTotal = po.TaxTotal;
                pomv.TotalAmount = po.TotalAmount;
                
                pomv.GrandTotal = po.GrandTotal;
                pomv.BCGrandTotal = po.BCGrandTotal;
                pomv.UserId = po.UserId;
                pomv.BranchId = po.BranchId;
                pomv.CompanyId = po.CompanyId;
                pomv.Status = po.Status;
                pomv.CreatedBy = po.CreatedBy;
                pomv.CreatedOn = po.CreatedOn;
                pomv.ModifiedBy = po.ModifiedBy;
                pomv.ModifiedOn = po.ModifiedOn;
                pomv.Memo = po.Memo;
                pomv.PaymentTermId = po.PaymentTermId;
                var podlist = db.PurchaseOrderDetails.Where(p => p.PurchaseOrderId == po.Id).ToList();
                foreach (var pod in podlist)
                {
                    var podmv = new PurchaseOrderDetailModelView();
                    podmv.PurchaseOrderId = pod.PurchaseOrderId;
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
            var taxes = db.Taxes.Where(d => (d.CompanyId == companyid )).Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate, NetEffective = d.NetEffective }).ToList();
            ViewBag.TaxSingle = taxes;
            var paymentTerms = db.PaymentTerms.Where(d => (d.CompanyId == companyid)).Select(d => new { Id = d.Id, PaymentTermDescription = d.PaymentTermDescription }).ToList();
            ViewBag.PaymentTerms = paymentTerms;
            var taxCompsList = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid )).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            ViewBag.Taxcomponents = taxCompsList;
            ViewBag.Branch = Branchid;

            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();

            PurchaseOrderModelView pomv = new PurchaseOrderModelView();
            List<PurchaseOrderDetailModelView> podmvList = new List<PurchaseOrderDetailModelView>();
            var sid = poCollection["SupplierId"];
            var tid = poCollection["TaxId"];
            var wid = poCollection["WarehouseId"];
            var pti = poCollection["PaymentTermId"];
            
            
            if (sid != "")
                pomv.SupplierId = Convert.ToInt32(sid);
            
            if (wid != "")
                pomv.WarehouseId = Convert.ToInt32(wid);
            
             if (pti != "")
                 pomv.PaymentTermId = Convert.ToInt32(pti);
           
            pomv.Id =Convert.ToInt64(poCollection["Id"]);
            pomv.NO = poCollection["NO"];
            pomv.RequestDetailId = poCollection["RequestDetailId"];
            pomv.SupplierName = poCollection["SupplierName"];
            pomv.SupplierCode = poCollection["SupplierCode"];
            pomv.Reference = poCollection["Reference"];
            pomv.Date = Convert.ToString(poCollection["Date"]);
            pomv.DueDate = Convert.ToString(poCollection["DueDate"]);
            pomv.DeliveryName = poCollection["DeliveryName"];
            pomv.StreetPoBox = poCollection["StreetPoBox"];
            pomv.Suburb = poCollection["Suburb"];
            pomv.City = poCollection["City"];
            pomv.StateRegion = poCollection["StateRegion"];
            pomv.Country = poCollection["Country"];
            pomv.PostalCode = poCollection["PostalCode"];
            pomv.CurrencyId = basecurrency.CurrencyId;
            pomv.BaseCurrencyCode = poCollection["BaseCurrencyCode"];
            pomv.Currencyrate = Convert.ToDecimal(poCollection["Currencyrate"]);
            pomv.TransactionCurrency = Convert.ToInt32(poCollection["TransactionCurrency"]);
            pomv.TransactionCurrencyCode = poCollection["TransactionCurrencyCode"]; 
            pomv.Status = poCollection["Status"];
            pomv.Memo = poCollection["Memo"];
            var count = poCollection["producthide"].Split(',').Length;
            for (int i = 0; i < count; i++)
            {
                PurchaseOrderDetailModelView podmv = new PurchaseOrderDetailModelView();
                podmvList.Add(podmv);
            }
            foreach (var key in poCollection.AllKeys)
            {

                string[] value = poCollection[key].Split(',');
                var lt = value.Length;
               switch(key)
               {
                   case "producthide" :
                    for (int i = 0; i < lt; i++)
                    {

                        podmvList[i].ItemCode = value[i];
                    }
                    break;
                
                   case "product" :
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
                   case "taxhide":
                
                    for (int i = 0; i < lt; i++)
                    {

                        podmvList[i].TaxName = value[i];
                    }
                     break;
                
                   case "tax":
                
                    for (int i = 0; i < lt; i++)
                    {
                        if (!(value[i] == "" || value[i] == "null"))
                            podmvList[i].TaxId = Convert.ToInt32(value[i]);
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
                   case "amounthide":
                
                    for (int i = 0; i < lt; i++)
                    {
                        if (!(value[i] == "" || value[i] == "null"))
                            podmvList[i].TotalAmount = Convert.ToDecimal(value[i]);
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
                   case "totalquantityhide":

                    for (int i = 0; i < lt; i++)
                    {
                        if (!(value[i] == "" || value[i] == "null"))
                            podmvList[i].TotalQuantity = Convert.ToDecimal(value[i]);
                    }
                    break;
                   case "descriptionhide":
                
                    for (int i = 0; i < lt; i++)
                    {

                        podmvList[i].Description = value[i];
                    }
                    break;
                   case "availablehide":
                
                    for (int i = 0; i < lt; i++)
                    {

                        podmvList[i].Available = value[i];
                    }
                    break;
            }
            }
            var checksupplier = db.Suppliers.Where(s => s.Code == pomv.SupplierCode && s.CompanyId==companyid ).FirstOrDefault();
            var date = DateTime.ParseExact(pomv.Date, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var duedate = DateTime.ParseExact(pomv.DueDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));

            var getDateRange = db.FinancialYearMasters.Where(d => d.fYearID == Fyid).FirstOrDefault();

            if (!(date >= getDateRange.sDate && date <= getDateRange.eDate))
            {
                var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                ViewBag.ddlWarehouses = warehouses;
                ViewBag.podmvList = podmvList;
                ViewBag.Date = "Invoice Date out of scope of " + getDateRange.Year + " Financial Year.";
                return View(pomv);
            }

            if (checksupplier == null)
            {
                var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                ViewBag.ddlWarehouses = warehouses;
                
                ViewBag.podmvList = podmvList;
                ViewBag.Supply = "Supplier does not exist!";
                if (duedate < date)
                    ViewBag.Date = "Delivery Date can not be less than Order Date ";
                return View(pomv);

            }
            else if (duedate < date)
            {
                var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                ViewBag.ddlWarehouses = warehouses;
                
                ViewBag.podmvList = podmvList;
                ViewBag.Date = "Delivery Date can not be less than Order Date ";
                return View(pomv);

            }
            else
            {
                pomv.SupplierId = checksupplier.Id;

                pomv.TransactionCurrency = checksupplier.CurrencyRate.CurrencyId;
                pomv.Currencyrate = checksupplier.CurrencyRate.PurchaseRate;
                
            }

            //purchase order details model view validation
            foreach (var podmv in podmvList)
            {
                if (podmv.ItemId != 0 && podmv.Quantity > 0  && podmv.TaxId > 0 )
                {
                    //do nothing
                }
                else
                {
                    if (!(podmv.ItemId == 0 && podmv.Quantity == 0 && podmv.Price == 0))
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
                            if (db.PurchaseOrders.Where(p => p.CompanyId == companyid && p.FinancialYearId == Fyid).Count() != 0)
                            {
                                countpo = (int)db.PurchaseOrders.Where(p => p.CompanyId == companyid && p.FinancialYearId == Fyid).Max(p => p.InvoiceNo) + 1;
                            }
                            var getPrefix = db.Prefixes.Where(p => p.DefaultPrefix == "PO" && p.CompanyId == companyid).Select(p => new { p.DefaultPrefix, p.SetPrefix }).FirstOrDefault();

                            if (getPrefix.SetPrefix != null)
                                pomv.NO = getPrefix.SetPrefix + "/" + fyear + "/" + countpo;//tc.GenerateCode(fyear, countpo);
                            else
                                pomv.NO = getPrefix.DefaultPrefix + "/" + fyear + "/" + countpo; //tc.GenerateCode(fyear, countpo);

                            //Insert into purchaseorder table
                            PurchaseOrder po = new PurchaseOrder();
                            po.NO = pomv.NO;
                            po.RequestDetailId = pomv.RequestDetailId;
                            po.SupplierId = pomv.SupplierId;
                            po.Reference = pomv.Reference;
                            po.InvoiceNo = countpo;
                            po.WarehouseId = pomv.WarehouseId;
                            po.Date = date;
                            po.DueDate = duedate;
                            po.DeliveryName = pomv.DeliveryName;
                            po.StreetPoBox = pomv.StreetPoBox;
                            po.Suburb = pomv.Suburb;
                            po.City = pomv.City;
                            po.StateRegion = pomv.StateRegion;
                            po.Country = pomv.Country;
                            po.PostalCode = pomv.PostalCode;
                            po.CurrencyId = pomv.CurrencyId;
                            po.Currencyrate = pomv.Currencyrate;
                            po.TransactionCurrency = pomv.TransactionCurrency;
                            //po.TaxTotal = pomv.TaxTotal;
                            //po.TotalAmount = pomv.TotalAmount;
                            //po.GrandTotal = pomv.GrandTotal;

                            //po.BCGrandTotal = pomv.BCGrandTotal;
                            po.FinancialYearId = Fyid;
                            po.CreatedBy = Createdby;
                            po.CreatedOn = DateTime.Now;
                            po.UserId = userid;
                            //po.BranchId = Branchid;
                            //if (Branchid == 0)
                            //    po.ApproveStatus = 1;
                            po.CompanyId = companyid;
                            po.Status = pomv.Status;
                            po.PaymentTermId = pomv.PaymentTermId;
                            po.Memo = pomv.Memo;
                            db.PurchaseOrders.Add(po);

                            db.SaveChanges();
                            var po1 = db.PurchaseOrders.Find(po.Id);
                            pomv.Id = po.Id;

                            decimal? subtotal = 0;
                            decimal totaltaxonproduct = 0;

                            foreach (var podmv in podmvList)
                            {
                                if (podmv.ItemId != 0 && podmv.Quantity > 0  && podmv.TaxId != 0)
                                {

                                    decimal taxrate = taxes.FirstOrDefault(r => r.TaxId == podmv.TaxId).Rate;
                                    PurchaseOrderDetail pod = new PurchaseOrderDetail();
                                    pod.PurchaseOrderId = po.Id;
                                    pod.ItemId = podmv.ItemId;
                                    pod.BarCode = podmv.BarCode;
                                    pod.Description = podmv.Description;
                                    pod.Quantity = podmv.Quantity;
                                    pod.AccountId = 12;
                                    pod.UnitId = podmv.UnitId;
                                    pod.Price = podmv.Price;
                                    pod.CurrencyRate = po.Currencyrate;
                                    pod.UnitIdSecondary = podmv.UnitIdSecondary;
                                    pod.SecUnitId = podmv.SecUnitId;
                                    pod.UnitFormula = podmv.UnitFormula;
                                    pod.SecUnitFormula = podmv.SecUnitFormula;

                                    pod.TaxId = podmv.TaxId;
                                    decimal? compounded = podmv.Price * podmv.Quantity;
                                    subtotal += compounded;
                                    var taxComps = taxCompsList.Where(t => t.TaxId == podmv.TaxId).ToList();

                                    if (taxComps.Count != 0)
                                    {

                                        decimal? effectiveTotal = 0;
                                        decimal? parentEffectiveRate = 0;
                                        decimal? totalamount = 0;
                                        decimal? effectivetaxrate = taxes.FirstOrDefault(r => r.TaxId == podmv.TaxId).NetEffective;
                                        foreach (var taxComp in taxComps)
                                        {
                                            decimal? rate = taxComp.Taxrate1;
                                            var IsDependTax = taxComp.IsDependTax;
                                            var IsCompoundedTax = taxComp.IsCompoundedTax;
                                            decimal? amount1 = 0;

                                            if (taxComp.IsDependTax == false && taxComp.IsCompoundedTax == false)
                                            {

                                                var crate = rate;
                                                parentEffectiveRate = crate * compounded / 100;
                                                effectiveTotal += parentEffectiveRate;
                                                amount1 = parentEffectiveRate;
                                                totalamount += amount1;

                                            }
                                            if (IsDependTax == true && IsCompoundedTax == false)
                                            {
                                                var crate = parentEffectiveRate * rate / 100;
                                                effectiveTotal += crate;
                                                amount1 = crate;
                                                totalamount += amount1;
                                            }
                                            if (IsDependTax == false && IsCompoundedTax == true)
                                            {
                                                var crate = (effectiveTotal + compounded) * rate / 100;
                                                effectiveTotal += crate;
                                                parentEffectiveRate = crate;
                                                amount1 = parentEffectiveRate;
                                                totalamount += amount1;
                                            }
                                        }
                                        pod.TaxPercent = (decimal)effectivetaxrate;
                                        pod.TaxAmount = (decimal)totalamount;




                                    }
                                    else
                                    {
                                        pod.TaxPercent = (decimal)taxrate;
                                        pod.TaxAmount = (decimal)taxrate * (decimal)compounded / 100;
                                    }
                                    totaltaxonproduct += pod.TaxAmount;
                                    pod.TotalAmount = (pod.Price * pod.Quantity) + pod.TaxAmount;
                                    db.PurchaseOrderDetails.Add(pod);
                                }
                            }
                            po1.TaxTotal = totaltaxonproduct;
                            po1.TotalAmount = (decimal)subtotal;
                            po1.GrandTotal = (decimal)subtotal + totaltaxonproduct;
                            po1.BCGrandTotal = Math.Round(po1.GrandTotal * pomv.Currencyrate);

                            if (pomv.RequestDetailId != "")
                            {
                                string[] value = pomv.RequestDetailId.Split(',');
                                int[] myInts = Array.ConvertAll(value, int.Parse);
                                for (int i = 0; i < myInts.Count(); i++)
                                {
                                    var counter = myInts[i];
                                    var getRequest = db.PurchaseRequestDetails.Find(counter);
                                    getRequest.BarCode = po1.NO;
                                    getRequest.TaxType = 2;
                                }
                            }
                        }
                        else
                        {
                            //Update Purchase Order
                            if (Createdby != 1)
                            {
                                var getMenuaccess = db.MenuaccessUsers.Where(d => d.AssignedUserId == Createdby && d.Name == "ApprovedPO").FirstOrDefault();
                                if (getMenuaccess == null && pomv.Status == "Approved")
                                {
                                    ViewBag.ErrorTag = "You do not have permissions to Approve Purchase Order.";
                                    var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                                    ViewBag.ddlWarehouses = warehouses;

                                    ViewBag.podmvList = podmvList;

                                    return View(pomv);
                                }
                                if (getMenuaccess == null && pomv.ApproveStatus == 1)
                                {
                                    ViewBag.ErrorTag = "You do not have permissions to change Approved Purchase Order.";
                                    var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                                    ViewBag.ddlWarehouses = warehouses;

                                    ViewBag.podmvList = podmvList;

                                    return View(pomv);
                                }
                            }
                            var po = db.PurchaseOrders.Find(pomv.Id);

                            po.NO = pomv.NO;
                            po.SupplierId = pomv.SupplierId;
                            po.Reference = pomv.Reference;

                            po.WarehouseId = pomv.WarehouseId;
                            po.Date = date;
                            po.DueDate = duedate;
                            po.DeliveryName = pomv.DeliveryName;
                            po.StreetPoBox = pomv.StreetPoBox;
                            po.Suburb = pomv.Suburb;
                            po.City = pomv.City;
                            po.StateRegion = pomv.StateRegion;
                            po.Country = pomv.Country;
                            po.PostalCode = pomv.PostalCode;
                            po.CurrencyId = pomv.CurrencyId;
                            po.Currencyrate = pomv.Currencyrate;
                            po.TransactionCurrency = pomv.TransactionCurrency;
                            po.TaxTotal = pomv.TaxTotal;
                            po.TotalAmount = pomv.TotalAmount;
                            po.GrandTotal = pomv.GrandTotal;

                            po.BCGrandTotal = pomv.BCGrandTotal;
                            po.FinancialYearId = Fyid;
                            po.ModifiedBy = Createdby;
                            po.ModifiedOn = DateTime.Now;
                            po.UserId = userid;
                            //   po.BranchId = Branchid;
                            if (pomv.Status == "Approved")
                            {
                                po.ApproveStatus = 1;
                                po.Status = "Saved";
                                po.ApprovedBy = Createdby;
                                po.ApprovedOn = DateTime.Now;
                            }
                            else
                            {
                                po.Status = pomv.Status;
                            }
                            po.CompanyId = companyid;

                            po.PaymentTermId = pomv.PaymentTermId;
                            po.Memo = pomv.Memo;
                            var podOldRecords = db.PurchaseOrderDetails.Where(p => p.PurchaseOrderId == po.Id).ToList();
                            foreach (var podOld in podOldRecords)
                            {
                                db.PurchaseOrderDetails.Remove(podOld);
                            }

                            decimal? subtotal = 0;
                            decimal totaltaxonproduct = 0;

                            foreach (var podmv in podmvList)
                            {
                                if (podmv.ItemId != 0 && podmv.Quantity > 0  && podmv.TaxId != 0)
                                {

                                    decimal taxrate = taxes.FirstOrDefault(r => r.TaxId == podmv.TaxId).Rate;
                                    PurchaseOrderDetail pod = new PurchaseOrderDetail();
                                    pod.PurchaseOrderId = po.Id;
                                    pod.ItemId = podmv.ItemId;
                                    pod.BarCode = podmv.BarCode;
                                    pod.Description = podmv.Description;
                                    pod.Quantity = podmv.Quantity;
                                    pod.AccountId = 12;
                                    pod.UnitId = podmv.UnitId;
                                    pod.Price = podmv.Price;
                                    pod.CurrencyRate = po.Currencyrate;
                                    pod.TaxId = podmv.TaxId;
                                    pod.UnitIdSecondary = podmv.UnitIdSecondary;
                                    pod.SecUnitId = podmv.SecUnitId;
                                    pod.UnitFormula = podmv.UnitFormula;
                                    pod.SecUnitFormula = podmv.SecUnitFormula;
                                    decimal? compounded = podmv.Price * podmv.Quantity;
                                    subtotal += compounded;
                                    var taxComps = taxCompsList.Where(t => t.TaxId == podmv.TaxId).ToList();

                                    if (taxComps.Count != 0)
                                    {

                                        decimal? effectiveTotal = 0;
                                        decimal? parentEffectiveRate = 0;
                                        decimal? totalamount = 0;
                                        decimal? effectivetaxrate = taxes.FirstOrDefault(r => r.TaxId == podmv.TaxId).NetEffective;
                                        foreach (var taxComp in taxComps)
                                        {
                                            decimal? rate = taxComp.Taxrate1;
                                            var IsDependTax = taxComp.IsDependTax;
                                            var IsCompoundedTax = taxComp.IsCompoundedTax;
                                            decimal? amount1 = 0;

                                            if (taxComp.IsDependTax == false && taxComp.IsCompoundedTax == false)
                                            {

                                                var crate = rate;
                                                parentEffectiveRate = crate * compounded / 100;
                                                effectiveTotal += parentEffectiveRate;
                                                amount1 = parentEffectiveRate;
                                                totalamount += amount1;

                                            }
                                            if (IsDependTax == true && IsCompoundedTax == false)
                                            {
                                                var crate = parentEffectiveRate * rate / 100;
                                                effectiveTotal += crate;
                                                amount1 = crate;
                                                totalamount += amount1;
                                            }
                                            if (IsDependTax == false && IsCompoundedTax == true)
                                            {
                                                var crate = (effectiveTotal + compounded) * rate / 100;
                                                effectiveTotal += crate;
                                                parentEffectiveRate = crate;
                                                amount1 = parentEffectiveRate;
                                                totalamount += amount1;
                                            }
                                        }
                                        pod.TaxPercent = (decimal)effectivetaxrate;
                                        pod.TaxAmount = (decimal)totalamount;




                                    }
                                    else
                                    {
                                        pod.TaxPercent = (decimal)taxrate;
                                        pod.TaxAmount = (decimal)taxrate * (decimal)compounded / 100;
                                    }
                                    totaltaxonproduct += pod.TaxAmount;
                                    pod.TotalAmount = (pod.Price * pod.Quantity) + pod.TaxAmount;
                                    db.PurchaseOrderDetails.Add(pod);
                                }
                            }
                            po.TaxTotal = totaltaxonproduct;
                            po.TotalAmount = (decimal)subtotal;
                            po.GrandTotal = (decimal)subtotal + totaltaxonproduct;
                            po.BCGrandTotal = Math.Round(po.GrandTotal * pomv.Currencyrate);
                            pomv.ApproveStatus = po.ApproveStatus;
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
            catch (DataException)
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
            ViewBag.Message = "You have successfully "+pomv.Status+" Purchase Order "+ pomv.NO;
           
            return View(pomv);
        }

        //
        // GET: /PurchaseOrder/Edit/5

        public ActionResult Edit(long id = 0)
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            var dateFormat = Session["DateFormat"].ToString();
            var culture = Session["DateCulture"].ToString();
            //   System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            ViewBag.ddlWarehouses = warehouses;
            var taxes = mc.getDdlTaxes(userid, companyid, Branchid);
            ViewBag.ddlTaxes = taxes;

            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid || d.UserId == 0 && d.CompanyId == 0).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            PurchaseOrder po = db.PurchaseOrders.Find(id);
            if (po == null)
            {
                return HttpNotFound();
            }
            PurchaseOrderModelView pomv = new PurchaseOrderModelView();
            List<PurchaseOrderDetailModelView> podmvList = new List<PurchaseOrderDetailModelView>();
            var supplier = db.Suppliers.Where(s => s.Id == po.SupplierId).Select(s => new { Name = s.Name, Code = s.Code }).FirstOrDefault();
            pomv.NO = po.NO;
            pomv.SupplierCode = supplier.Code;
            pomv.SupplierName = supplier.Name;
            pomv.SupplierId = po.SupplierId;
            pomv.Reference = po.Reference;
           
            pomv.WarehouseId = po.WarehouseId;
            pomv.CurrencyId = po.CurrencyId;
            pomv.BaseCurrencyCode = po.Currency.ISO_4217;
            pomv.Currencyrate = po.Currencyrate;
            pomv.TransactionCurrency = po.TransactionCurrency;
            pomv.TransactionCurrencyCode = po.Currency1.ISO_4217;
            pomv.Date = po.Date.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
            pomv.DueDate = po.DueDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
            pomv.DeliveryName = po.DeliveryName;
            pomv.StreetPoBox = po.StreetPoBox;
            pomv.Suburb = po.Suburb;
            pomv.City = po.City;
            pomv.StateRegion = po.StateRegion;
            pomv.Country = po.Country;
            pomv.PostalCode = po.PostalCode;
            pomv.TaxTotal = po.TaxTotal;
            pomv.TotalAmount = po.TotalAmount;
          
            pomv.UserId = po.UserId;
            pomv.BranchId = po.BranchId;
            pomv.CompanyId = po.CompanyId;
            pomv.Status = "Parked";
            return View(po);
        }

        //
        // POST: /PurchaseOrder/Edit/5

        [HttpPost]
        public ActionResult Edit(PurchaseOrder purchaseorder)
        { //var userLanguages = Request.UserLanguages;
            //CultureInfo ci;
            //string lang = string.Empty;
            //if (userLanguages.Count() > 0)
            //{
            //    try
            //    {
            //        ci = new CultureInfo(userLanguages[0]);
            //        lang = userLanguages[0];
            //    }
            //    catch (CultureNotFoundException)
            //    {
            //        ci = CultureInfo.InvariantCulture;
            //        lang = "en-US";
            //    }
            //}
            //else
            //{
            //    ci = CultureInfo.InvariantCulture;
            //    lang = "en-US";
            //}


            //string shortUsDateFormatString = ci.DateTimeFormat.ShortDatePattern;
            //string shortUsTimeFormatString = ci.DateTimeFormat.ShortTimePattern;
            if (ModelState.IsValid)
            {
                db.Entry(purchaseorder).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(purchaseorder);
        }

        //
        // GET: /PurchaseOrder/Delete/5

        public ActionResult Delete(long id = 0)
        {
            PurchaseOrder purchaseorder = db.PurchaseOrders.Find(id);
            if (purchaseorder == null)
            {
                return HttpNotFound();
            }
            return View(purchaseorder);
        }

        //
        // POST: /PurchaseOrder/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            PurchaseOrder purchaseorder = db.PurchaseOrders.Find(id);
            db.PurchaseOrders.Remove(purchaseorder);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public JsonResult GetPODetails(long PONo)
        {
            var dateFormat = Session["DateFormat"].ToString();
            var culture = Session["DateCulture"].ToString();
            PurchaseOrderModelView po = new PurchaseOrderModelView();
            var getPODetails = db.PurchaseOrderDetails.Where(p => p.PurchaseOrderId == PONo).Select(p => new PurchaseOrderDetailModelView {ItemId= p.ItemId,Description= p.Description,Quantity= p.Quantity,TotalQuantity=p.Quantity,Price=p.Price,SecUnitFormula= p.SecUnitFormula,SecUnitId= p.SecUnitId,UnitFormula=0,UnitId= p.UnitId,UnitIdSecondary= p.UnitIdSecondary, ItemCode = p.Product.Code, ItemName = p.Product.Name, UnitName=p.UOM.Code, UnitSecondaryName=p.UOM1.Code, SecUnitName=p.UOM2.Code }).ToList();
            var getPOMain = db.PurchaseOrders.Where(p => p.Id == PONo).Select(p => new PurchaseOrderModelView { BaseCurrencyCode = p.Currency.ISO_4217, City = p.City, Country = p.Country, CurrencyId = p.CurrencyId, Currencyrate = p.Currencyrate, CreatedOn = p.Date, DeliveryName = p.DeliveryName, ModifiedOn = p.DueDate, PaymentTermId = p.PaymentTermId, PostalCode = p.PostalCode, StateRegion = p.StateRegion, StreetPoBox = p.StreetPoBox, Suburb = p.Suburb, SupplierCode = p.Supplier.Code, SupplierId = p.SupplierId, SupplierName = p.Supplier.Name, TransactionCurrency = p.TransactionCurrency, TransactionCurrencyCode = p.Currency1.ISO_4217, WarehouseId = p.WarehouseId }).FirstOrDefault();
            getPOMain.pod = getPODetails;
            po = getPOMain;
            po.Date = getPOMain.CreatedOn.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
            return Json(po, JsonRequestBehavior.AllowGet);
        }
        
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }


        #region PDF Email



        public ActionResult CreatePurchaseOrderPDF(long? id, long? Branchid, long? companyid, long? userid)
        {
            //long Branchid = Convert.ToInt64(Session["BranchId"]);
            //long companyid = Convert.ToInt64(Session["companyid"]);
            //long userid = Convert.ToInt32(Session["userid"]);
            var culture = Convert.ToString(Session["DateCulture"]);
            var dateFormat = Convert.ToString(Session["DateFormat"]);
            PurchaseOrderModelView pomv = new PurchaseOrderModelView();
            List<PurchaseOrderDetailModelView> podmvList = new List<PurchaseOrderDetailModelView>();
         //   List<PurchaseCostingDetailModelView> pcdmvList = new List<PurchaseCostingDetailModelView>();
          //  List<InvoiceTotal> invoiceTotalList = new List<InvoiceTotal>();






            var taxes = db.Taxes.Where(d => (d.CompanyId == companyid && d.BranchId == Branchid) || (d.UserId == 0 && d.CompanyId == 0)).Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate }).ToList();
            ViewBag.TaxSingle = taxes;

            var Taxcomponents = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid && d.Tax.BranchId == Branchid) || (d.Tax.CompanyId == 0 && d.Tax.BranchId == 0)).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            ViewBag.Taxcomponents = Taxcomponents;

            PurchaseOrder po = db.PurchaseOrders.Find(id);
            if (po == null)
            {
                return HttpNotFound();
            }

            var numtowords = new NumberToEnglish();
            ViewBag.BCTotalAmount = numtowords.changeNumericToWords(po.BCGrandTotal);

            //var customer = db.Customers.Where(r => r.Id == po.SupplierId).Select(s => s.Code).FirstOrDefault();
            //ViewBag.customer = customer;
            var company = db.Companies.Where(c => c.Id == po.CompanyId).FirstOrDefault();
            // var companyname = db.Companies.Where(c => c.Id == comids).FirstOrDefault();
            ViewBag.company = company;


            if (po.PaymentTermId != null)
            {
                var paymentterm = db.PaymentTerms.Where(c => c.Id == po.PaymentTermId).Select(d => new {d.PaymentTermDescription, d.Days }).FirstOrDefault();
                ViewBag.PaymentTerm = paymentterm.PaymentTermDescription;
                ViewBag.CreditDays = paymentterm.Days;
            }

            if (po.CreatedBy != null)
            {
                var prepare = db.Users.Where(d => d.Id == po.CreatedBy).Select(d => new { d.FirstName, d.LastName, d.PhoneNumber }).FirstOrDefault();
                ViewBag.PreparedBy = prepare.FirstName + " " + prepare.LastName;
                ViewBag.PreparedContact = prepare.PhoneNumber;
            }
            if (po.ApprovedBy != null)
            {
                var approve = db.Users.Where(d => d.Id == po.ApprovedBy).Select(d => new { d.FirstName, d.LastName, d.PhoneNumber }).FirstOrDefault();
                ViewBag.ApprovedBy = approve.FirstName + " " + approve.LastName;
                ViewBag.ApprovedContact = approve.PhoneNumber;
            }

           

            // List<CustomerwiseInvoice> inv = new List<CustomerwiseInvoice>();

            //string cusid = Convert.ToString(id);
            //decimal? recipt = db.ReceiptPayments.Where(d => d.transactionNo == cusid).Sum(d => d.TotalAmount);
            //var result = db.PurchaseInvoices.Where(d => d.SupplierId == id).OrderBy(d => d.Id).ToList();

            //var logo = db.BusinessPartners.Where(r => r.CompanyId == companyid).Select(s => s.Logo).ToList();
            //ViewBag.Logo = logo;


            var podlist = db.PurchaseOrderDetails.Where(p => p.PurchaseOrderId == po.Id).ToList();
            foreach (var pod in podlist)
            {
                var podmv = new PurchaseOrderDetailModelView();
                podmv.PurchaseOrderId = pod.PurchaseOrderId;
                podmv.ItemId = pod.ItemId;
                podmv.ItemName = pod.Product.Description;
                podmv.Description = pod.Description;
                podmv.Quantity = pod.Quantity;
                podmv.AccountId = pod.AccountId;
                podmv.UnitId = pod.UnitId;
                podmv.UnitName = pod.UOM.Code;
                podmv.Price = pod.Price;
                podmv.CurrencyRate = pod.CurrencyRate;
                podmv.TaxId = pod.TaxId;
                podmv.TaxName = pod.Tax.Name + '(' + pod.Tax.Rate + "%)";
                podmv.TaxAmount = pod.TaxAmount;

                podmv.TotalAmount = pod.Price * pod.Quantity;

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


            //var pcdlist = db.PurchaseCostingDetails.Where(p => p.PurchaseInvoiceId == po.Id).ToList();
            //foreach (var pcd in pcdlist)
            //{
            //    var pcdmv = new PurchaseCostingDetailModelView();
            //    pcdmv.PurchaseInvoiceId = pcd.PurchaseInvoiceId;
            //    pcdmv.CostingId = pcd.CostingId;
            //    pcdmv.CostName = pcd.Costing.Name;
            //    pcdmv.Description = pcd.Description;
            //    pcdmv.CostingType = pcd.CostingType;
            //    pcdmv.CurrencyRate = pcd.CurrencyRate;
            //    pcdmv.TaxId = pcd.TaxId;
            //    pcdmv.TaxName = pcd.Tax.Name + '(' + pcd.Tax.Rate + "%)";
            //    pcdmv.TaxAmount = pcd.TaxAmount;
            //    pcdmv.CostAmount = pcd.CostAmount;
            //    pcdmvList.Add(pcdmv);
            //}
            //ViewBag.pcdmvList = pcdmvList;



            //if (po.Status == "Saved")
            //{
            //    if (po.SubTotal > 0)
            //    {
            //        var invoiceTotal = new InvoiceTotal { Id = 1, Name = "Sub Total", Amount = po.SubTotal };
            //        invoiceTotalList.Add(invoiceTotal);
            //    }
            //    if (po.TotalAddAmount > 0)
            //    {
            //        var invoiceTotal1 = new InvoiceTotal { Id = 8, Name = "Added Cost", Amount = po.TotalAddAmount };
            //        invoiceTotalList.Add(invoiceTotal1);
            //    }
            //    if (po.TotalDeductAmount > 0)
            //    {
            //        var invoiceTotal2 = new InvoiceTotal { Id = 9, Name = "Deducted Cost", Amount = -po.TotalDeductAmount };
            //        invoiceTotalList.Add(invoiceTotal2);
            //    }
            //    if (pomv.RoundOff > 0)
            //    {
            //        var invoiceTotal = new InvoiceTotal { Id = 6, Name = "RoundOff", Amount = po.RoundOff };
            //        invoiceTotalList.Add(invoiceTotal);
            //    }
            //    var invoiceTotal4 = new InvoiceTotal { Id = 12, Name = "Grand Total In " + po.Currency1.ISO_4217, Amount = po.GrandTotal };
            //    invoiceTotalList.Add(invoiceTotal4);
            //    if (pomv.CurrencyId != pomv.TransactionCurrency)
            //    {

            //        var invoiceTotal5 = new InvoiceTotal { Id = 13, Name = "Grand Total In " + po.Currency.ISO_4217, Amount = po.BCGrandTotal };
            //        invoiceTotalList.Add(invoiceTotal5);
            //    }
            //    var alltaxes = db.SalesTaxes.Where(s => s.SalesInvoiceId == po.Id).ToList();
            //    foreach (var individualtax in alltaxes)
            //    {
            //        if (individualtax.ItemId != null)
            //        {
            //            var tname = taxes.FirstOrDefault(t => t.TaxId == individualtax.TaxId);
            //            var invoiceTotal = new InvoiceTotal { Id = 7, Name = tname.Name + '(' + tname.Rate + "%)" + " On Product", Amount = individualtax.Amount };
            //            invoiceTotalList.Add(invoiceTotal);
            //        }
            //        else
            //        {
            //            var tname = taxes.FirstOrDefault(t => t.TaxId == individualtax.TaxId);
            //            var invoiceTotal = new InvoiceTotal { Id = 10, Name = tname.Name + '(' + tname.Rate + "%)" + " On Others", Amount = individualtax.Amount };
            //            invoiceTotalList.Add(invoiceTotal);
            //        }
            //    }
            //}
            //invoiceTotalList = invoiceTotalList.OrderBy(f => f.Id).ToList();
            //var invoiceTotalList1 = invoiceTotalList.GroupBy(t => t.Name).Select(t => new InvoiceTotal { Name = t.Key, Amount = t.Sum(l => l.Amount) }).ToList();
            //ViewBag.InvoiceTotalList = invoiceTotalList1;

            //string receipts = "";
            //string pos = "";
            //if (pomv.ReceiptIds != "")
            //{
            //    string[] value = po.ReceiptIds.Split(',');
            //    int[] myInts = Array.ConvertAll(value, int.Parse);
            //    var countpr = myInts.Length;

            //    for (int i = 0; i < countpr; i++)
            //    {
            //        var prid = myInts[i];
            //        var purchaseReceipt = db.PurchaseReceipts.Where(p => p.Id == prid).Select(p => new { p.ReferenceChallan, p.ReferenceNo }).FirstOrDefault();
            //        if (purchaseReceipt != null)
            //        {

            //            receipts += purchaseReceipt.ReferenceChallan + " ";

            //            var purchaseorder = db.PurchaseOrders.Where(p => p.Id == purchaseReceipt.ReferenceNo).Select(p => p.NO).FirstOrDefault();
            //            if (purchaseorder != null)
            //                pos += purchaseorder + " ";
            //        }


            //    }
            //}
           // ViewBag.receipts = receipts;
           // ViewBag.pos = pos;
            return View(po);


        }



        public ActionResult PrintPurchaseOrderPDF(long? id)
        {
            //return RedirectToAction("CreteSalesInvoicePDF", new { id = id });
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);

            return new ActionAsPdf("CreatePurchaseOrderPDF", new { id = id, Branchid = Branchid, companyid = companyid, userid = userid }) { FileName = "PurchaseOrderPrint.pdf" };
        }





        #endregion
    }
}
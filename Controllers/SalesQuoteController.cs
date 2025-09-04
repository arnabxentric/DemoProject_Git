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
    public class SalesQuoteController : Controller
    {
        private InventoryEntities db = new InventoryEntities();
        private MasterClasses mc = new MasterClasses();
        private TransactionClasses tc = new TransactionClasses();
        //
        // GET: /SalesQuote/
        [SessionExpire]
        public ActionResult Index()
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            ViewBag.BaseCurrency = basecurrency.CurrencyCode;
            //var SalesQuotes = db.SalesQuotes.Where(p => p.CompanyId == companyid && p.BranchId == Branchid).Include(s => s.Currency1).Include(s => s.Customer).Include(s => s.Tax).Include(s => s.Warehouse);
            if (Branchid == 0)
                return View(db.SalesQuotes.Where(p => p.CompanyId == companyid && p.FinancialYearId == Fyid).ToList());
            else
                return View(db.SalesQuotes.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.FinancialYearId == Fyid).ToList());
        }

        //
        // GET: /SalesQuote/Details/5

        public ActionResult Details(long id = 0)
        {
            SalesQuote salesquote = db.SalesQuotes.Find(id);
            if (salesquote == null)
            {
                return HttpNotFound();
            }
            return View(salesquote);
        }

        //
        // GET: /SalesQuote/Create
        [SessionExpire]
        public ActionResult Create(long? id = 0)
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            var dateFormat = Session["DateFormat"].ToString();
            var culture = Session["DateCulture"].ToString();
            //    System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            ViewBag.ddlWarehouses = warehouses;
            var taxes = db.Taxes.Where(d => (d.CompanyId == companyid)).Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate }).ToList();
            ViewBag.TaxSingle = taxes;
            var paymentTerms = db.PaymentTerms.Where(d => (d.CompanyId == companyid)).Select(d => new { Id = d.Id, PaymentTermDescription = d.PaymentTermDescription }).ToList();
            ViewBag.PaymentTerms = paymentTerms;
            var Taxcomponents = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid )).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            ViewBag.Taxcomponents = Taxcomponents;
            var salesPerson = db.SalesPersons.Where(d => d.UserId == userid && d.CompanyId == companyid).Select(d => new { Id = d.Id, Name = d.Name }).ToList();
            ViewBag.ddlSalesPerson = salesPerson;
            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();


            if (id == 0)
            {
                SalesQuoteModelView pomv = new SalesQuoteModelView();
                pomv.Date = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.DueDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.CurrencyId = basecurrency.CurrencyId;
                pomv.BaseCurrencyCode = basecurrency.CurrencyCode;
               
                return View(pomv);
            }
            else
            {
                SalesQuote po = db.SalesQuotes.Find(id);
                if (po == null)
                {
                    return HttpNotFound();
                }
                SalesQuoteModelView pomv = new SalesQuoteModelView();
                List<SalesQuoteDetailModelView> podmvList = new List<SalesQuoteDetailModelView>();

                pomv.Id = po.Id;
                pomv.NO = po.NO;
                pomv.CustomerCode = po.Customer.Code;
                pomv.CustomerName = po.Customer.Name;
                pomv.CustomerId = po.CustomerId;
                pomv.Reference = po.Reference;
                pomv.SalesPerson = po.SalesPerson;

                pomv.PaymentTermId = po.PaymentTermId;
                pomv.WarehouseId = po.WarehouseId;
                pomv.CurrencyId = po.CurrencyId;
                pomv.BaseCurrencyCode = po.Currency.ISO_4217;
                pomv.Currencyrate = po.Currencyrate;
                pomv.TransactionCurrency = po.TransactionCurrency;
                pomv.TransactionCurrencyCode = po.Currency1.ISO_4217;
                pomv.Date = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.DueDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.DeliveryName = po.DeliveryName;
                pomv.StreetPoBox = po.StreetPoBox;
                pomv.Suburb = po.Suburb;
                pomv.City = po.City;
                pomv.StateRegion = po.StateRegion;
                pomv.Country = po.Country;
                pomv.PostalCode = po.PostalCode;
                pomv.TaxTotal = po.TaxTotal;
                pomv.TotalAmount = po.TotalAmount;
                pomv.Memo = po.Memo;
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

                var podlist = db.SalesQuoteDetails.Where(p => p.SalesQuoteId == po.Id).ToList();
                foreach (var pod in podlist)
                {
                    var podmv = new SalesQuoteDetailModelView();
                    podmv.SalesQuoteId = pod.SalesQuoteId;
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
                    podmv.Discount = pod.Discount;
                    podmv.DiscountAmount = pod.DiscountAmount;
                    podmv.TaxPercent = pod.TaxPercent;
                    podmv.TaxAmount = pod.TaxAmount;
                    podmv.TotalAmount = pod.Price * pod.Quantity * pod.UnitFormula - pod.DiscountAmount;

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
        // POST: /SalesQuote/Create
        [SessionExpire]
        [HttpPost]
        public ActionResult Create(FormCollection poCollection)
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt32(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            long Createdby = Convert.ToInt32(Session["Createdid"]);
            string dateFormat = Session["DateFormat"].ToString();
            var culture = Session["DateCulture"].ToString();
            //  System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            string currencyval = string.Empty;
            var salesPerson = db.SalesPersons.Where(d => (d.UserId == userid )).Select(d => new { Id = d.Id, Name = d.Name }).ToList();
            ViewBag.ddlSalesPerson = salesPerson;
            var taxes = db.Taxes.Where(d => (d.CompanyId == companyid )).Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate, NetEffective = d.NetEffective }).ToList();
            ViewBag.TaxSingle = taxes;
            var paymentTerms = db.PaymentTerms.Where(d => (d.CompanyId == companyid)).Select(d => new { Id = d.Id, PaymentTermDescription = d.PaymentTermDescription }).ToList();
            ViewBag.PaymentTerms = paymentTerms;
            var taxCompsList = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid)).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            ViewBag.Taxcomponents = taxCompsList;
            ViewBag.unit = db.UOMs.Where(d => (d.UserId == userid && d.CompanyId == companyid)).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => (d.UserId == userid && d.CompanyId == companyid)).ToList();


            SalesQuoteModelView pomv = new SalesQuoteModelView();
            List<SalesQuoteDetailModelView> podmvList = new List<SalesQuoteDetailModelView>();
            var sid = poCollection["CustomerId"];
            var tid = poCollection["PaymentTermId"];
            var wid = poCollection["WarehouseId"];
            

            if (sid != "")
                pomv.CustomerId = Convert.ToInt32(sid);
            if (tid != "")
                pomv.PaymentTermId = Convert.ToInt32(tid);
            if (wid != "")
                pomv.WarehouseId = Convert.ToInt32(wid);
           


            pomv.Id = Convert.ToInt64(poCollection["Id"]);
            pomv.NO = poCollection["NO"];
            pomv.CustomerName = poCollection["CustomerName"];
            pomv.CustomerCode = poCollection["CustomerCode"];
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

            List<InvoiceTotal> invoiceTotalList = new List<InvoiceTotal>();
            long salesp;
            if (long.TryParse(poCollection["SalesPerson"].Trim(), out salesp))
            {
                // Parse successful. value can be any integer
                pomv.SalesPerson = salesp;
            }
            else
            {
                pomv.SalesPerson = null;
                // Parse failed. value will be 0.
            }
            var count = poCollection["producthide"].Split(',').Length;
            for (int i = 0; i < count; i++)
            {
                SalesQuoteDetailModelView podmv = new SalesQuoteDetailModelView();
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

                            podmvList[i].ItemName = value[i];
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

                            podmvList[i].ItemCode = value[i];
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
                    case "discounthide":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].Discount = Convert.ToDecimal(value[i]);
                        }
                        break;
                    case "disamounthide":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].DiscountAmount = Convert.ToDecimal(value[i]);
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
            var checkCustomer = db.Customers.Where(s => s.Code == pomv.CustomerCode && s.CompanyId == companyid).FirstOrDefault();
            var date = DateTime.ParseExact(pomv.Date, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var duedate = DateTime.ParseExact(pomv.DueDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            if (checkCustomer == null)
            {
                var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                ViewBag.ddlWarehouses = warehouses;
               
                ViewBag.podmvList = podmvList;
                ViewBag.Supply = "Customer does not exist!";
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
                
                pomv.CustomerId = checkCustomer.Id;
          //      pomv.CustomerDiscountLedger = checkCustomer.DiscountLedger;
               
                pomv.TransactionCurrency = checkCustomer.CurrencyRate.CurrencyId;
                pomv.Currencyrate = checkCustomer.CurrencyRate.SellRate;
                
                pomv.BCGrandTotal = pomv.Currencyrate * pomv.GrandTotal;
            }

            //purchase order details model view validation
            foreach (var podmv in podmvList)
            {
                if (podmv.ItemId != 0 && podmv.Quantity > 0 && podmv.Price > 0 && podmv.TaxId > 0)
                {
                    //do nothing
                }
                else
                {
                    if (!(podmv.ItemId == 0 && podmv.Quantity == 0 && podmv.Price == 0 && podmv.TaxId == 0))
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
                            int countpo = db.SalesQuotes.Where(p => p.CompanyId == companyid && p.BranchId == Branchid).Count();
                            pomv.NO = tc.GenerateCode("SQ", countpo);

                            //Insert into SalesOrder table
                            SalesQuote po = new SalesQuote();
                            po.NO = pomv.NO;
                            po.CustomerId = pomv.CustomerId;
                            po.Reference = pomv.Reference;
                            po.PaymentTermId = pomv.PaymentTermId;
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
                            po.SalesPerson = pomv.SalesPerson;

                            po.TaxTotal = pomv.TaxTotal;
                            po.TotalAmount = pomv.TotalAmount;
                            po.GrandTotal = pomv.GrandTotal;

                            po.BCGrandTotal = pomv.BCGrandTotal;
                            po.FinancialYearId = Fyid;
                            po.CreatedBy = Createdby;
                            po.CreatedOn = DateTime.Now;
                            po.UserId = userid;
                            po.BranchId = Branchid;
                            po.CompanyId = companyid;
                            po.Status = pomv.Status;
                            po.Memo = pomv.Memo;
                            db.SalesQuotes.Add(po);

                            db.SaveChanges();
                            var po1 = db.SalesQuotes.Find(po.Id);
                            pomv.Id = po.Id;

                            decimal? subtotal = 0;
                            decimal totaltaxonproduct = 0;
                            foreach (var podmv in podmvList)
                            {
                                if (podmv.ItemId != 0 && podmv.Quantity > 0 && podmv.Price > 0 && podmv.TaxId != 0)
                                {
                                    decimal taxrate = taxes.FirstOrDefault(r => r.TaxId == podmv.TaxId).Rate;
                                    SalesQuoteDetail pod = new SalesQuoteDetail();
                                    pod.SalesQuoteId = po.Id;
                                    pod.ItemId = podmv.ItemId;
                                    pod.BarCode = podmv.BarCode;
                                    pod.Description = podmv.Description;
                                    pod.Quantity = podmv.Quantity;
                                    pod.AccountId = 12;
                                    pod.UnitId = podmv.UnitId;
                                    pod.Price = podmv.Price;
                                    pod.CurrencyRate = po.Currencyrate;
                                    pod.TaxId = podmv.TaxId;
                                    pod.Discount = podmv.Discount;
                                    pod.DiscountAmount = podmv.DiscountAmount;
                                    pod.UnitIdSecondary = podmv.UnitIdSecondary;
                                    pod.SecUnitId = podmv.SecUnitId;
                                    pod.UnitFormula = podmv.UnitFormula;
                                    pod.SecUnitFormula = podmv.SecUnitFormula;

                                    decimal? compounded = podmv.Price * podmv.Quantity * podmv.UnitFormula - podmv.DiscountAmount; 
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
                                    pod.TotalAmount = (pod.Price * pod.Quantity* podmv.UnitFormula) + pod.TaxAmount;
                                    db.SalesQuoteDetails.Add(pod);
                                }
                            }
                            po1.TaxTotal = totaltaxonproduct;
                            po1.TotalAmount = (decimal)subtotal;
                            po1.GrandTotal = (decimal)subtotal + totaltaxonproduct;
                            po1.BCGrandTotal = Math.Round(po1.GrandTotal * pomv.Currencyrate);
                        }
                        else
                        {
                            //Update Sales Quote
                            var po = db.SalesQuotes.Find(pomv.Id);

                            po.NO = pomv.NO;
                            po.CustomerId = pomv.CustomerId;
                            po.Reference = pomv.Reference;
                            po.PaymentTermId = pomv.PaymentTermId;
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
                            po.SalesPerson = pomv.SalesPerson;

                            po.TaxTotal = pomv.TaxTotal;
                            po.TotalAmount = pomv.TotalAmount;
                            po.GrandTotal = pomv.GrandTotal;

                            po.BCGrandTotal = pomv.BCGrandTotal;
                            po.FinancialYearId = Fyid;
                            po.ModifiedBy = Createdby;
                            po.ModifiedOn = DateTime.Now;
                            po.UserId = userid;
                            po.BranchId = Branchid;
                            po.CompanyId = companyid;
                            po.Status = pomv.Status;
                            po.Memo = pomv.Memo;
                            var podOldRecords = db.SalesQuoteDetails.Where(p => p.SalesQuoteId == po.Id).ToList();
                            foreach (var podOld in podOldRecords)
                            {
                                db.SalesQuoteDetails.Remove(podOld);
                            }

                            decimal? subtotal = 0;
                            decimal totaltaxonproduct = 0;
                            foreach (var podmv in podmvList)
                            {
                                if (podmv.ItemId != 0 && podmv.Quantity > 0 && podmv.Price > 0 && podmv.TaxId != 0)
                                {
                                    decimal taxrate = taxes.FirstOrDefault(r => r.TaxId == podmv.TaxId).Rate;
                                    SalesQuoteDetail pod = new SalesQuoteDetail();
                                    pod.SalesQuoteId = po.Id;
                                    pod.ItemId = podmv.ItemId;
                                    pod.BarCode = podmv.BarCode;
                                    pod.Description = podmv.Description;
                                    pod.Quantity = podmv.Quantity;
                                    pod.AccountId = 12;
                                    pod.UnitId = podmv.UnitId;
                                    pod.Price = podmv.Price;
                                    pod.CurrencyRate = po.Currencyrate;
                                    pod.TaxId = podmv.TaxId;
                                    pod.Discount = podmv.Discount;
                                    pod.DiscountAmount = podmv.DiscountAmount;
                                    pod.UnitIdSecondary = podmv.UnitIdSecondary;
                                    pod.SecUnitId = podmv.SecUnitId;
                                    pod.UnitFormula = podmv.UnitFormula;
                                    pod.SecUnitFormula = podmv.SecUnitFormula;



                                    decimal? compounded = podmv.Price * podmv.Quantity * podmv.UnitFormula - podmv.DiscountAmount;
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
                                    pod.TotalAmount = (pod.Price * pod.Quantity * podmv.UnitFormula) + pod.TaxAmount;
                                    db.SalesQuoteDetails.Add(pod);
                                }
                            }
                            po.TaxTotal = totaltaxonproduct;
                            po.TotalAmount = (decimal)subtotal;
                            po.GrandTotal = (decimal)subtotal + totaltaxonproduct;
                            po.BCGrandTotal = Math.Round(po.GrandTotal * pomv.Currencyrate);
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
            ViewBag.Message = "You have successfully " + pomv.Status + " Sales Quote" + pomv.NO;
            return View(pomv);
        }

        //
        // GET: /SalesQuote/Edit/5

        public ActionResult Edit(long id = 0)
        {
            SalesQuote salesquote = db.SalesQuotes.Find(id);
            if (salesquote == null)
            {
                return HttpNotFound();
            }
            ViewBag.CurrencyId = new SelectList(db.Currencies, "CurrencyId", "Country", salesquote.CurrencyId);
            ViewBag.TransactionCurrency = new SelectList(db.Currencies, "CurrencyId", "Country", salesquote.TransactionCurrency);
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "Code", salesquote.CustomerId);
           // ViewBag.TaxId = new SelectList(db.Taxes, "TaxId", "Name", salesquote.TaxId);
            ViewBag.WarehouseId = new SelectList(db.Warehouses, "Id", "Code", salesquote.WarehouseId);
            return View(salesquote);
        }

        //
        // POST: /SalesQuote/Edit/5

        [HttpPost]
        public ActionResult Edit(SalesQuote salesquote)
        {
            if (ModelState.IsValid)
            {
                db.Entry(salesquote).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CurrencyId = new SelectList(db.Currencies, "CurrencyId", "Country", salesquote.CurrencyId);
            ViewBag.TransactionCurrency = new SelectList(db.Currencies, "CurrencyId", "Country", salesquote.TransactionCurrency);
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "Code", salesquote.CustomerId);
          //  ViewBag.TaxId = new SelectList(db.Taxes, "TaxId", "Name", salesquote.TaxId);
            ViewBag.WarehouseId = new SelectList(db.Warehouses, "Id", "Code", salesquote.WarehouseId);
            return View(salesquote);
        }

        //
        // GET: /SalesQuote/Delete/5

        public ActionResult Delete(long id = 0)
        {
            SalesQuote salesquote = db.SalesQuotes.Find(id);
            if (salesquote == null)
            {
                return HttpNotFound();
            }
            return View(salesquote);
        }

        //
        // POST: /SalesQuote/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            SalesQuote salesquote = db.SalesQuotes.Find(id);
            db.SalesQuotes.Remove(salesquote);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
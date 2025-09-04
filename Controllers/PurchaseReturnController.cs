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
    public class PurchaseReturnController : Controller
    {
        private InventoryEntities db = new InventoryEntities();
        private MasterClasses mc = new MasterClasses();
        private TransactionClasses tc = new TransactionClasses();
        //
        // GET: /PurchaseReturn/
        [SessionExpire]
        public ActionResult Index()
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            ViewBag.BranchId = Branchid;
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            ViewBag.BaseCurrency = basecurrency.CurrencyCode;
            if(Branchid==0)
                return View(db.PurchaseReturns.Where(p => p.CompanyId == companyid && p.FinancialYearId == Fyid).ToList());
            else
                return View(db.PurchaseReturns.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.FinancialYearId == Fyid).ToList());
        }

        //
        // GET: /PurchaseReturn/Details/5
        [SessionExpire]
        public ActionResult Details(long id = 0)
        {
            PurchaseReturn PurchaseReturn = db.PurchaseReturns.Find(id);
            if (PurchaseReturn == null)
            {
                return HttpNotFound();
            }
            return View(PurchaseReturn);
        }

        //
        // GET: /PurchaseReturn/Create
        [SessionExpire]
        public ActionResult Create(long? id = 0)
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            var dateFormat = Session["DateFormat"].ToString();
            var culture = Session["DateCulture"].ToString();
            //   System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            ViewBag.ddlWarehouses = warehouses;
            var taxes = db.Taxes.Where(d => (d.CompanyId == companyid)).Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate }).ToList();
            ViewBag.TaxSingle = taxes;
            var paymentTerms = db.PaymentTerms.Where(d => (d.CompanyId == companyid)).Select(d => new { Id = d.Id, PaymentTermDescription = d.PaymentTermDescription }).ToList();
            ViewBag.PaymentTerms = paymentTerms;
            var Taxcomponents = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid )).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            ViewBag.Taxcomponents = Taxcomponents;
            ViewBag.Ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.groupID == 117).Select(d => new { d.LID, d.ledgerName }).ToList();
            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid ).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();

            if (id == 0)
            {
                PurchaseReturnModelView pomv = new PurchaseReturnModelView();
                pomv.Date = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.Type = "1";
                pomv.CurrencyId = basecurrency.CurrencyId;
                pomv.BaseCurrencyCode = basecurrency.CurrencyCode;
                string countryname = string.Empty;
                var defaultwarehouse = db.Warehouses.Where(w => w.DefaultWarehouse == true && w.Companyid == companyid && w.Branchid == Branchid).FirstOrDefault();
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
                PurchaseReturn po = db.PurchaseReturns.Find(id);
                if (po == null)
                {
                    return HttpNotFound();
                }
                PurchaseReturnModelView pomv = new PurchaseReturnModelView();
                List<PurchaseReturnDetailModelView> podmvList = new List<PurchaseReturnDetailModelView>();

                pomv.Id = po.Id;
                pomv.NO = po.NO;
                pomv.LID = po.LID;
                pomv.SupplierCode = po.Supplier.Code;
                pomv.SupplierName = po.Supplier.Name;
                pomv.SupplierId = po.SupplierId;
                pomv.Reference = po.Reference;
                pomv.Type = po.Type;
                pomv.WarehouseId = po.WarehouseId;
                pomv.CurrencyId = po.CurrencyId;
                pomv.BaseCurrencyCode = po.Currency.ISO_4217;
                pomv.Currencyrate = po.Currencyrate;
                pomv.TransactionCurrency = po.TransactionCurrency;
                pomv.TransactionCurrencyCode = po.Currency1.ISO_4217;
                pomv.Date = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
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
                var podlist = db.PurchaseReturnDetails.Where(p => p.PurchaseReturnId == po.Id).ToList();
                foreach (var pod in podlist)
                {
                    var podmv = new PurchaseReturnDetailModelView();
                    podmv.PurchaseReturnId = pod.PurchaseReturnId;
                    podmv.ItemId = pod.ItemId;
                    podmv.ItemName = pod.Product.Code;
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
                    //   podmv.Discount = pod.Discount;
                    podmv.TaxPercent = pod.TaxPercent;
                    podmv.TaxAmount = pod.TaxAmount;
                    podmv.TotalAmount = pod.TotalAmount;


                    podmvList.Add(podmv);
                }
                ViewBag.podmvList = podmvList;
                return View(pomv);
            }
        }

        //
        // POST: /PurchaseReturn/Create
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
            var taxes = db.Taxes.Where(d => (d.CompanyId == companyid)).Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate, NetEffective = d.NetEffective }).ToList();
            ViewBag.TaxSingle = taxes;
            var paymentTerms = db.PaymentTerms.Where(d => (d.CompanyId == companyid)).Select(d => new { Id = d.Id, PaymentTermDescription = d.PaymentTermDescription }).ToList();
            ViewBag.PaymentTerms = paymentTerms;
            var taxCompsList = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid )).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            ViewBag.Taxcomponents = taxCompsList;


            ViewBag.Ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.groupID == 117).Select(d => new { d.LID, d.ledgerName }).ToList();
            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();

            PurchaseReturnModelView pomv = new PurchaseReturnModelView();
            List<PurchaseReturnDetailModelView> podmvList = new List<PurchaseReturnDetailModelView>();
            var sid = poCollection["SupplierId"];
            var tid = poCollection["TaxId"];
            var wid = poCollection["WarehouseId"];
            var pti = poCollection["PaymentTermId"];
            var type = poCollection["Type"];
            var othertax = poCollection["TaxId"];
            var otheramount = poCollection["TotalAmount"];


            if (sid != "")
                pomv.SupplierId = Convert.ToInt32(sid);

            if (wid != "")
                pomv.WarehouseId = Convert.ToInt32(wid);

            if (pti != "")
                pomv.PaymentTermId = Convert.ToInt32(pti);
            if(othertax !="")
                pomv.TaxId = Convert.ToInt64(othertax);
            pomv.Id = Convert.ToInt64(poCollection["Id"]);
            pomv.NO = poCollection["NO"];
            pomv.SupplierName = poCollection["SupplierName"];
            pomv.SupplierCode = poCollection["SupplierCode"];
            pomv.Reference = poCollection["Reference"];
            pomv.Date = Convert.ToString(poCollection["Date"]);
           
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
            pomv.Type = type;
            pomv.LID = Convert.ToInt32(poCollection["LID"]);
            pomv.TotalAmount = Convert.ToDecimal(otheramount);

            if (pomv.Type == "1")
            {
                var count = poCollection["producthide"].Split(',').Length;
                for (int i = 0; i < count; i++)
                {
                    PurchaseReturnDetailModelView podmv = new PurchaseReturnDetailModelView();
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
                                    podmvList[i].UnitId = Convert.ToInt32(value[i]);
                            }
                            break;
                        case "uomhide":

                            for (int i = 0; i < lt; i++)
                            {

                                podmvList[i].UnitName = value[i];
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
            }
            var checksupplier = db.Suppliers.Where(s => s.Code == pomv.SupplierCode && s.CompanyId == companyid).FirstOrDefault();
            var date = DateTime.ParseExact(pomv.Date, dateFormat, CultureInfo.CreateSpecificCulture(culture));

            var getDateRange = db.FinancialYearMasters.Where(d => d.fYearID == Fyid).FirstOrDefault();

            if (!(date >= getDateRange.sDate && date <= getDateRange.eDate))
            {
                var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                ViewBag.ddlWarehouses = warehouses;
                ViewBag.podmvList = podmvList;
                ViewBag.Date = "Purchase Return Date out of scope of " + getDateRange.Year + " Financial Year.";
                return View(pomv);
            }

            if (checksupplier == null)
            {
                var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                ViewBag.ddlWarehouses = warehouses;

                ViewBag.podmvList = podmvList;
                ViewBag.Supply = "Supplier does not exist!";
               
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
                if (podmv.ItemId != 0 && podmv.Quantity > 0 && podmv.Price > 0 && podmv.TaxId > 0)
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
                            int countpo = db.PurchaseReturns.Where(p => p.CompanyId == companyid && p.BranchId == Branchid).Count();
                            pomv.NO = tc.GenerateCode("PR", countpo);

                            //Insert into PurchaseReturn table
                            PurchaseReturn po = new PurchaseReturn();
                            po.NO = pomv.NO;
                            po.SupplierId = pomv.SupplierId;
                            po.Reference = pomv.Reference;
                            po.LID = pomv.LID;
                            po.WarehouseId = pomv.WarehouseId;
                            po.Date = date;
                       
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
                           
                            po.FinancialYearId = Fyid;
                            po.CreatedBy = Createdby;
                            po.CreatedOn = DateTime.Now;
                            po.UserId = userid;
                            po.BranchId = Branchid;
                            po.CompanyId = companyid;
                            po.Status = pomv.Status;
                            po.PaymentTermId = pomv.PaymentTermId;
                            po.Memo = pomv.Memo;
                            po.Type = pomv.Type;
                            po.TaxId = pomv.TaxId;
                            po.TotalAmount = pomv.TotalAmount;
                            db.PurchaseReturns.Add(po);

                            db.SaveChanges();
                            var po1 = db.PurchaseReturns.Find(po.Id);
                            pomv.Id = po.Id;
                            if (pomv.Type == "1")
                            {
                                decimal? subtotal = 0;
                                decimal totaltaxonproduct = 0;

                                foreach (var podmv in podmvList)
                                {
                                    if (podmv.ItemId != 0 && podmv.Quantity > 0 && podmv.Price > 0 && podmv.TaxId != 0)
                                    {

                                        decimal taxrate = taxes.FirstOrDefault(r => r.TaxId == podmv.TaxId).Rate;
                                        PurchaseReturnDetail pod = new PurchaseReturnDetail();
                                        pod.PurchaseReturnId = po.Id;
                                        pod.ItemId = podmv.ItemId;
                                        pod.BarCode = podmv.BarCode;
                                        pod.Description = podmv.Description;
                                        pod.Quantity = podmv.Quantity;
                                        pod.AccountId = 12;
                                        pod.UnitId = podmv.UnitId;
                                        pod.Price = podmv.Price;
                                        pod.CurrencyRate = po.Currencyrate;
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

                                                var PurchaseReturnTax = new PurchaseReturnTax();
                                                PurchaseReturnTax.PurchaseReturnId = pomv.Id;
                                                PurchaseReturnTax.ItemId = podmv.ItemId;
                                                PurchaseReturnTax.TaxId = (long)taxComp.TaxCompId;
                                                PurchaseReturnTax.Amount = (decimal)amount1;
                                                PurchaseReturnTax.CurrencyRate = pomv.Currencyrate;
                                                if (po.Status == "Saved")
                                                {
                                                    db.PurchaseReturnTaxes.Add(PurchaseReturnTax);
                                                }
                                            }
                                            pod.TaxPercent = (decimal)effectivetaxrate;
                                            pod.TaxAmount = (decimal)totalamount;




                                        }
                                        else
                                        {
                                            pod.TaxPercent = (decimal)taxrate;
                                            pod.TaxAmount = (decimal)taxrate * (decimal)compounded / 100;
                                            var PurchaseReturnTax = new PurchaseReturnTax();
                                            PurchaseReturnTax.PurchaseReturnId = pomv.Id;
                                            PurchaseReturnTax.ItemId = podmv.ItemId;
                                            PurchaseReturnTax.TaxId = podmv.TaxId;
                                            PurchaseReturnTax.Amount = (decimal)taxrate * (decimal)compounded / 100;
                                            PurchaseReturnTax.CurrencyRate = pomv.Currencyrate;
                                            if (po.Status == "Saved")
                                            {
                                                db.PurchaseReturnTaxes.Add(PurchaseReturnTax);
                                            }
                                        }
                                        totaltaxonproduct += pod.TaxAmount;
                                        pod.TotalAmount = (pod.Price * pod.Quantity) + pod.TaxAmount;
                                        db.PurchaseReturnDetails.Add(pod);
                                        if (po.Status == "Saved")
                                        {

                                            var stock = new Stock();
                                            stock.ArticleID = podmv.ItemId;
                                            stock.Items = podmv.Quantity;
                                            // stock.Price = podmv.Price;
                                            stock.TranCode = "OUT";
                                            stock.TransTag = "PR";
                                            stock.TranDate = date;
                                            stock.TranId = po.Id;
                                            stock.WarehouseId = po.WarehouseId;
                                            stock.UserId = po.UserId;
                                            stock.CompanyId = po.CompanyId;
                                            stock.BranchId = po.BranchId;
                                            stock.CreatedBy = po.CreatedBy;
                                            db.Stocks.Add(stock);
                                        }
                                    }
                                }

                                po1.TaxTotal = totaltaxonproduct;
                                po1.TotalAmount = (decimal)subtotal;
                                po1.GrandTotal = (decimal)subtotal + totaltaxonproduct;
                                po1.BCGrandTotal = Math.Round(po1.GrandTotal * pomv.Currencyrate);
                            }
                            else
                            {
                                decimal? compounded = pomv.TotalAmount;
                                decimal? totaltaxamount = 0;
                                var taxComps = taxCompsList.Where(t => t.TaxId == pomv.TaxId).ToList();

                                if (taxComps.Count != 0)
                                {

                                    decimal? effectiveTotal = 0;
                                    decimal? parentEffectiveRate = 0;
                                    decimal? totalamount = 0;
                                    decimal? effectivetaxrate = taxes.FirstOrDefault(r => r.TaxId == pomv.TaxId).NetEffective;
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

                                        var PurchaseReturnTax = new PurchaseReturnTax();
                                        PurchaseReturnTax.PurchaseReturnId = pomv.Id;
                                        // PurchaseReturnTax.ItemId = podmv.ItemId;
                                        PurchaseReturnTax.TaxId = (long)taxComp.TaxCompId;
                                        PurchaseReturnTax.Amount = (decimal)amount1;
                                        PurchaseReturnTax.CurrencyRate = pomv.Currencyrate;
                                        if (po.Status == "Saved")
                                        {
                                            db.PurchaseReturnTaxes.Add(PurchaseReturnTax);
                                        }
                                    }
                                    totaltaxamount = totalamount;
                                }
                                else
                                {
                                    decimal taxrate = taxes.FirstOrDefault(r => r.TaxId == pomv.TaxId).Rate;
                                    totaltaxamount = compounded * taxrate / 100;
                                    var PurchaseReturnTax = new PurchaseReturnTax();
                                    PurchaseReturnTax.PurchaseReturnId = pomv.Id;
                                    // PurchaseReturnTax.ItemId = podmv.ItemId;
                                    PurchaseReturnTax.TaxId = (long)pomv.TaxId;
                                    PurchaseReturnTax.Amount = (decimal)taxrate * (decimal)compounded / 100;
                                    PurchaseReturnTax.CurrencyRate = pomv.Currencyrate;
                                    if (po.Status == "Saved")
                                    {
                                        db.PurchaseReturnTaxes.Add(PurchaseReturnTax);
                                    }
                                }

                                po1.TaxTotal = (decimal)totaltaxamount;
                                po1.TotalAmount = (decimal)compounded;
                                po1.GrandTotal = (decimal)compounded + (decimal)totaltaxamount;
                                po1.BCGrandTotal = Math.Round(po1.GrandTotal * pomv.Currencyrate);
                            }
                        }
                        else
                        {
                            //Update Purchase Order
                            var po = db.PurchaseReturns.Find(pomv.Id);

                            po.NO = pomv.NO;
                            po.SupplierId = pomv.SupplierId;
                            po.Reference = pomv.Reference;
                            po.LID = pomv.LID;
                            po.WarehouseId = pomv.WarehouseId;
                            po.Date = date;
                           
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
                            po.BranchId = Branchid;
                            po.CompanyId = companyid;
                            po.Status = pomv.Status;
                            po.PaymentTermId = pomv.PaymentTermId;
                            po.Memo = pomv.Memo;
                            var podOldRecords = db.PurchaseReturnDetails.Where(p => p.PurchaseReturnId == po.Id).ToList();
                            foreach (var podOld in podOldRecords)
                            {
                                db.PurchaseReturnDetails.Remove(podOld);
                            }
                            var PurchaseReturnTaxOldRecords = db.PurchaseReturnTaxes.Where(p => p.PurchaseReturnId == po.Id).ToList();
                            foreach (var podOld in PurchaseReturnTaxOldRecords)
                            {
                                db.PurchaseReturnTaxes.Remove(podOld);
                            }
                            var oldstocks = db.Stocks.Where(p => p.TransTag == "PR" && p.TranId == po.Id).ToList();
                            foreach (var oldstock in oldstocks)
                            {
                                db.Stocks.Remove(oldstock);
                            }
                            if (pomv.Type == "1")
                            {
                                decimal? subtotal = 0;
                                decimal totaltaxonproduct = 0;

                                foreach (var podmv in podmvList)
                                {
                                    if (podmv.ItemId != 0 && podmv.Quantity > 0 && podmv.Price > 0 && podmv.TaxId != 0)
                                    {

                                        decimal taxrate = taxes.FirstOrDefault(r => r.TaxId == podmv.TaxId).Rate;
                                        PurchaseReturnDetail pod = new PurchaseReturnDetail();
                                        pod.PurchaseReturnId = po.Id;
                                        pod.ItemId = podmv.ItemId;
                                        pod.BarCode = podmv.BarCode;
                                        pod.Description = podmv.Description;
                                        pod.Quantity = podmv.Quantity;
                                        pod.AccountId = 12;
                                        pod.UnitId = podmv.UnitId;
                                        pod.Price = podmv.Price;
                                        pod.CurrencyRate = po.Currencyrate;
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
                                                var PurchaseReturnTax = new PurchaseReturnTax();
                                                PurchaseReturnTax.PurchaseReturnId = pomv.Id;
                                                PurchaseReturnTax.ItemId = podmv.ItemId;
                                                PurchaseReturnTax.TaxId = (long)taxComp.TaxCompId;
                                                PurchaseReturnTax.Amount = (decimal)amount1;
                                                PurchaseReturnTax.CurrencyRate = pomv.Currencyrate;
                                                if (po.Status == "Saved")
                                                {
                                                    db.PurchaseReturnTaxes.Add(PurchaseReturnTax);
                                                }
                                            }
                                            pod.TaxPercent = (decimal)effectivetaxrate;
                                            pod.TaxAmount = (decimal)totalamount;




                                        }
                                        else
                                        {
                                            pod.TaxPercent = (decimal)taxrate;
                                            pod.TaxAmount = (decimal)taxrate * (decimal)compounded / 100;

                                            var PurchaseReturnTax = new PurchaseReturnTax();
                                            PurchaseReturnTax.PurchaseReturnId = pomv.Id;
                                            PurchaseReturnTax.ItemId = podmv.ItemId;
                                            PurchaseReturnTax.TaxId = podmv.TaxId;
                                            PurchaseReturnTax.Amount = (decimal)taxrate * (decimal)compounded / 100;
                                            PurchaseReturnTax.CurrencyRate = pomv.Currencyrate;
                                            if (po.Status == "Saved")
                                            {
                                                db.PurchaseReturnTaxes.Add(PurchaseReturnTax);
                                            }
                                        }
                                        totaltaxonproduct += pod.TaxAmount;
                                        pod.TotalAmount = (pod.Price * pod.Quantity) + pod.TaxAmount;
                                        db.PurchaseReturnDetails.Add(pod);
                                        if (po.Status == "Saved")
                                        {

                                            var stock = new Stock();
                                            stock.ArticleID = podmv.ItemId;
                                            stock.Items = podmv.Quantity;
                                            // stock.Price = podmv.Price;
                                            stock.TranCode = "OUT";
                                            stock.TransTag = "PR";
                                            stock.TranDate = date;
                                            stock.TranId = po.Id;
                                            stock.WarehouseId = po.WarehouseId;
                                            stock.UserId = po.UserId;
                                            stock.CompanyId = po.CompanyId;
                                            stock.BranchId = po.BranchId;
                                            stock.CreatedBy = po.CreatedBy;
                                            db.Stocks.Add(stock);
                                        }
                                    }
                                }
                                po.TaxTotal = totaltaxonproduct;
                                po.TotalAmount = (decimal)subtotal;
                                po.GrandTotal = (decimal)subtotal + totaltaxonproduct;
                                po.BCGrandTotal = Math.Round(po.GrandTotal * pomv.Currencyrate);
                            }
                            else
                            {
                                decimal? compounded = pomv.TotalAmount;
                                decimal? totaltaxamount = 0;
                                var taxComps = taxCompsList.Where(t => t.TaxId == pomv.TaxId).ToList();

                                if (taxComps.Count != 0)
                                {

                                    decimal? effectiveTotal = 0;
                                    decimal? parentEffectiveRate = 0;
                                    decimal? totalamount = 0;
                                    decimal? effectivetaxrate = taxes.FirstOrDefault(r => r.TaxId == pomv.TaxId).NetEffective;
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

                                        var PurchaseReturnTax = new PurchaseReturnTax();
                                        PurchaseReturnTax.PurchaseReturnId = pomv.Id;
                                        // PurchaseReturnTax.ItemId = podmv.ItemId;
                                        PurchaseReturnTax.TaxId = (long)taxComp.TaxCompId;
                                        PurchaseReturnTax.Amount = (decimal)amount1;
                                        PurchaseReturnTax.CurrencyRate = pomv.Currencyrate;
                                        if (po.Status == "Saved")
                                        {
                                            db.PurchaseReturnTaxes.Add(PurchaseReturnTax);
                                        }
                                    }
                                    totaltaxamount = totalamount;
                                }
                                else
                                {
                                    decimal taxrate = taxes.FirstOrDefault(r => r.TaxId == pomv.TaxId).Rate;
                                    totaltaxamount = compounded * taxrate / 100;
                                    var PurchaseReturnTax = new PurchaseReturnTax();
                                    PurchaseReturnTax.PurchaseReturnId = pomv.Id;
                                    // PurchaseReturnTax.ItemId = podmv.ItemId;
                                    PurchaseReturnTax.TaxId = (long)pomv.TaxId;
                                    PurchaseReturnTax.Amount = (decimal)taxrate * (decimal)compounded / 100;
                                    PurchaseReturnTax.CurrencyRate = pomv.Currencyrate;
                                    if (po.Status == "Saved")
                                    {
                                        db.PurchaseReturnTaxes.Add(PurchaseReturnTax);
                                    }
                                }

                                po.TaxTotal = (decimal)totaltaxamount;
                                po.TotalAmount = (decimal)compounded;
                                po.GrandTotal = (decimal)compounded + (decimal)totaltaxamount;
                                po.BCGrandTotal = Math.Round(po.GrandTotal * pomv.Currencyrate);
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
            ViewBag.Message = "You have successfully " + pomv.Status + " Purchase Return/Debit Note" + pomv.NO;

            return View(pomv);
        }

        //
        // GET: /PurchaseReturn/Edit/5

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
            PurchaseReturn po = db.PurchaseReturns.Find(id);
            if (po == null)
            {
                return HttpNotFound();
            }
            PurchaseReturnModelView pomv = new PurchaseReturnModelView();
            List<PurchaseReturnDetailModelView> podmvList = new List<PurchaseReturnDetailModelView>();
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
        // POST: /PurchaseReturn/Edit/5

        [HttpPost]
        public ActionResult Edit(PurchaseReturn PurchaseReturn)
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
                db.Entry(PurchaseReturn).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(PurchaseReturn);
        }

        //
        // GET: /PurchaseReturn/Delete/5

        public ActionResult Delete(long id = 0)
        {
            PurchaseReturn PurchaseReturn = db.PurchaseReturns.Find(id);
            if (PurchaseReturn == null)
            {
                return HttpNotFound();
            }
            return View(PurchaseReturn);
        }

        //
        // POST: /PurchaseReturn/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            PurchaseReturn PurchaseReturn = db.PurchaseReturns.Find(id);
            var purchasedetail = db.PurchaseReturnDetails.Where(d => d.PurchaseReturnId == PurchaseReturn.Id);
            foreach (var sid in purchasedetail)
            {
                db.PurchaseReturnDetails.Remove(sid);
            }
            var PurchaseTax = db.PurchaseReturnTaxes.Where(d => d.PurchaseReturnId == PurchaseReturn.Id);
            foreach (var sit in PurchaseTax)
            {
                db.PurchaseReturnTaxes.Remove(sit);
            }
            var stock = db.Stocks.Where(d => d.TranId == PurchaseReturn.Id);
            foreach (var sit in stock)
            {
                db.Stocks.Remove(sit);
            }
            db.PurchaseReturns.Remove(PurchaseReturn);
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

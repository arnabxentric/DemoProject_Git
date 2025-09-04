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
    
    public class PurchaseInvoiceController : Controller
    {
        private InventoryEntities db = new InventoryEntities();
        private MasterClasses mc = new MasterClasses();
        private TransactionClasses tc = new TransactionClasses();
        //
        // GET: /PurchaseInvoice/
        [SessionExpire]
        public ActionResult Index()
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            ViewBag.BaseCurrency = basecurrency.CurrencyCode;
            ViewBag.BranchId = Branchid;
            if (Branchid == 0)
                return View(db.PurchaseInvoices.Where(p => p.CompanyId == companyid && p.FinancialYearId == Fyid).OrderByDescending(d => d.InvoiceDate).ThenByDescending(d => d.InvoiceNo).Take(100).OrderBy(d => d.InvoiceDate).ThenBy(d => d.InvoiceNo).ToList());
            else
                return View(db.PurchaseInvoices.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.FinancialYearId == Fyid).OrderByDescending(d => d.InvoiceDate).ThenByDescending(d => d.InvoiceNo).Take(100).OrderBy(d => d.InvoiceDate).ThenBy(d => d.InvoiceNo).ToList());
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
            ViewBag.BranchId = Branchid;
            if (Branchid == 0)
                return View(db.PurchaseInvoices.Where(p => p.CompanyId == companyid && p.FinancialYearId == Fyid && p.InvoiceDate >= DtFrm && p.InvoiceDate <= DtTo).OrderBy(d => d.InvoiceDate).ThenBy(d => d.InvoiceNo).ToList());
            else
                return View(db.PurchaseInvoices.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.FinancialYearId == Fyid && p.InvoiceDate >= DtFrm && p.InvoiceDate <= DtTo).OrderBy(d => d.InvoiceDate).ThenBy(d => d.InvoiceNo).ToList());
        }
        //
        // GET: /PurchaseInvoice/Details/5
        [SessionExpire]
        public ActionResult Details(long id = 0)
        {
            PurchaseInvoice purchaseinvoice = db.PurchaseInvoices.Find(id);
            if (purchaseinvoice == null)
            {
                return HttpNotFound();
            }
            return View(purchaseinvoice);
        }

        //
        // GET: /PurchaseInvoice/Create
        [SessionExpire]
        public ActionResult Create(long? id = 0, string from = null)
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt32(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            var dateFormat = Session["DateFormat"].ToString();
            var culture = Session["DateCulture"].ToString();
            //System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            ViewBag.ddlWarehouses = warehouses;
            var taxes = db.Taxes.Where(d => (d.CompanyId == companyid )).Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate }).ToList();
            ViewBag.TaxSingle = taxes;
            var paymentTerms = db.PaymentTerms.Where(d => (d.CompanyId == companyid )).Select(d => new { Id = d.Id, PaymentTermDescription = d.PaymentTermDescription }).ToList();
            ViewBag.PaymentTerms = paymentTerms;
            var Taxcomponents = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid)).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            ViewBag.Taxcomponents = Taxcomponents;
            ViewBag.Ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.groupID == 117).Select(d => new { d.LID, d.ledgerName }).ToList();
            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();

            ViewBag.Branch = db.BranchMasters.Select(s => new { Value = s.Id, Text = s.Name }).ToList();


         //   ViewBag.ReferencePO = GetYetToBeInvoicedPR();
            if (id == 0)
            {
                PurchaseInvoiceModelView pomv = new PurchaseInvoiceModelView();
                pomv.Date = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.InvoiceDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.DespatchDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
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
                ViewBag.AdressName = defaultwarehouse.AdressName;
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
                if (from == null)
                {
                    PurchaseInvoice po = db.PurchaseInvoices.Find(id);
                    if (po == null)
                    {
                        return HttpNotFound();
                    }
                    PurchaseInvoiceModelView pomv = new PurchaseInvoiceModelView();
                    List<PurchaseInvoiceDetailModelView> podmvList = new List<PurchaseInvoiceDetailModelView>();
                    List<PurchaseCostingDetailModelView> pcdmvList = new List<PurchaseCostingDetailModelView>();
                    List<InvoiceTotal> invoiceTotalList = new List<InvoiceTotal>();
                    pomv.Id = po.Id;
                    pomv.NO = po.NO;
                    pomv.LID = po.LID;
                    pomv.ReferenceInvoice = po.ReferenceInvoice;
                    pomv.ReceiptIds = po.ReceiptIds;
                    pomv.CreatedFrom = po.CreatedFrom;
                    pomv.ReferenceNo = po.ReferenceNo;
                    pomv.OrderNo = po.OrderNo;
                    pomv.PurchaseOrderNo = po.PurchaseOrderNo;
                    pomv.PurchaseOrderId = po.PurchaseOrderId;
                    pomv.SupplierCode = po.Supplier.Code;
                    pomv.SupplierName = po.Supplier.Name;
                    pomv.SupplierId = po.SupplierId;
                    pomv.Reference = po.Reference;
                    pomv.PaymentTermId = po.PaymentTermId;
                    pomv.WarehouseId = po.WarehouseId;
                    pomv.CurrencyId = po.CurrencyId;
                    pomv.BaseCurrencyCode = po.Currency.ISO_4217;
                    pomv.Currencyrate = po.Currencyrate;
                    pomv.TransactionCurrency = po.TransactionCurrency;
                    pomv.TransactionCurrencyCode = po.Currency1.ISO_4217;
                    pomv.Date = po.Date.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DueDate = po.Date.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.InvoiceDate = po.InvoiceDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DespatchDate = po.DespatchDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DeliveryName = po.DeliveryName;
                    pomv.StreetPoBox = po.StreetPoBox;
                    pomv.Suburb = po.Suburb;
                    pomv.City = po.City;
                    pomv.StateRegion = po.StateRegion;
                    pomv.Country = po.Country;
                    pomv.PostalCode = po.PostalCode;
                    pomv.TaxProduct = po.TaxProduct;
                    pomv.TaxOther = po.TaxOther;
                    pomv.SubTotal = po.SubTotal;

                    pomv.DespatchNo = po.DespatchNo;
                    pomv.DespatchThrough = po.DespatchThrough;
                    pomv.DespatchDestination = po.DespatchDestination;

                    pomv.GrandTotal = po.GrandTotal;
                    pomv.BCGrandTotal = po.BCGrandTotal;
                    pomv.UserId = po.UserId;
                    pomv.BranchId = po.BranchId;
                    pomv.CompanyId = po.CompanyId;
                    pomv.Status = po.Status;
                    //pomv.CreatedBy = po.CreatedBy;
                    //pomv.CreatedOn = po.CreatedOn;
                    //pomv.ModifiedBy = po.ModifiedBy;
                    //pomv.ModifiedOn = po.ModifiedOn;
                    pomv.Memo = po.Memo;
                    pomv.PaymentTermId = po.PaymentTermId;

                    var podlist = db.PurchaseInvoiceDetails.Where(p => p.PurchaseInvoiceId == po.Id).ToList();
                    foreach (var pod in podlist)
                    {
                        var podmv = new PurchaseInvoiceDetailModelView();
                        podmv.PurchaseInvoiceId = pod.PurchaseInvoiceId;
                        podmv.ItemId = pod.ItemId;
                        podmv.ItemCode = pod.Product.Code;
                        podmv.ItemName = pod.Product.Name;
                        podmv.Description = pod.Description;
                        podmv.BarCode = pod.BarCode;
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
                        podmv.TotalQuantity = pod.Quantity;
                        podmvList.Add(podmv);
                    }
                    ViewBag.podmvList = podmvList;
                    var pcdlist = db.PurchaseCostingDetails.Where(p => p.PurchaseInvoiceId == po.Id).ToList();
                    foreach (var pcd in pcdlist)
                    {
                        var pcdmv = new PurchaseCostingDetailModelView();
                        pcdmv.PurchaseInvoiceId = pcd.PurchaseInvoiceId;
                        pcdmv.CostingId = pcd.CostingId;
                        pcdmv.CostName = pcd.Costing.Name;
                        pcdmv.Description = pcd.Description;
                        pcdmv.CostingType = pcd.CostingType;
                        pcdmv.CurrencyRate = pcd.CurrencyRate;
                        pcdmv.TaxId = pcd.TaxId;
                        pcdmv.TaxName = pcd.Tax.Name + '-' + pcd.Tax.TaxId;
                        pcdmv.TaxPercent = pcd.TaxPercent;
                        pcdmv.TaxAmount = pcd.TaxAmount;

                        pcdmv.CostAmount = pcd.CostAmount;

                        pcdmvList.Add(pcdmv);
                    }
                    ViewBag.pcdmvList = pcdmvList;

                    if (pomv.Status == "Saved")
                    {
                        if (pomv.SubTotal > 0)
                        {
                            var invoiceTotal = new InvoiceTotal { Id = 1, Name = "Sub Total", Amount = pomv.SubTotal };
                            invoiceTotalList.Add(invoiceTotal);
                        }
                        if (pomv.TotalAddAmount > 0)
                        {
                            var invoiceTotal1 = new InvoiceTotal { Id = 8, Name = "Added Cost", Amount = pomv.TotalAddAmount };
                            invoiceTotalList.Add(invoiceTotal1);
                        }
                        if (pomv.TotalDeductAmount > 0)
                        {
                            var invoiceTotal2 = new InvoiceTotal { Id = 9, Name = "Deducted Cost", Amount = -pomv.TotalDeductAmount };
                            invoiceTotalList.Add(invoiceTotal2);
                        }
                        if (pomv.RoundOff > 0)
                        {
                            var invoiceTotal = new InvoiceTotal { Id = 6, Name = "RoundOff", Amount = pomv.RoundOff };
                            invoiceTotalList.Add(invoiceTotal);
                        }
                        var invoiceTotal4 = new InvoiceTotal { Id = 12, Name = "Grand Total In " + pomv.TransactionCurrencyCode, Amount = pomv.GrandTotal };
                        invoiceTotalList.Add(invoiceTotal4);
                        if (pomv.CurrencyId != pomv.TransactionCurrency)
                        {

                            var invoiceTotal5 = new InvoiceTotal { Id = 13, Name = "Grand Total In " + pomv.BaseCurrencyCode, Amount = pomv.BCGrandTotal };
                            invoiceTotalList.Add(invoiceTotal5);
                        }
                        var alltaxes = db.PurchaseTaxes.Where(s => s.PurchaseInvoiceId == pomv.Id).ToList();
                        foreach (var individualtax in alltaxes)
                        {
                            if (individualtax.ItemId != null)
                            {
                                var tname = taxes.FirstOrDefault(t => t.TaxId == individualtax.TaxId);
                                var invoiceTotal = new InvoiceTotal { Id = 7, Name = tname.Name + '(' + tname.Rate + "%)" + " On Product", Amount = individualtax.Amount };
                                invoiceTotalList.Add(invoiceTotal);
                            }
                            else
                            {
                                var tname = taxes.FirstOrDefault(t => t.TaxId == individualtax.TaxId);
                                var invoiceTotal = new InvoiceTotal { Id = 10, Name = tname.Name + '(' + tname.Rate + "%)" + " On Others", Amount = individualtax.Amount };
                                invoiceTotalList.Add(invoiceTotal);
                            }
                        }
                    }
                    invoiceTotalList = invoiceTotalList.OrderBy(f => f.Id).ToList();
                    var invoiceTotalList1 = invoiceTotalList.GroupBy(t => t.Name).Select(t => new { Name = t.Key, Total = t.Sum(l => l.Amount) }).ToList();
                    ViewBag.InvoiceTotalList = invoiceTotalList1;

                    return View(pomv);
                }
                else if(from == "PO")
                {
                    PurchaseOrder po = db.PurchaseOrders.Find(id);
                    if (po == null)
                    {
                        return HttpNotFound();
                    }
                    PurchaseInvoiceModelView pomv = new PurchaseInvoiceModelView();
                    List<PurchaseInvoiceDetailModelView> podmvList = new List<PurchaseInvoiceDetailModelView>();

                    pomv.Id = 0;
                    //   pomv.NO = po.NO;
                    pomv.CreatedFrom = "Purchase Order";
                    pomv.ReferenceNo = po.Id;
                    pomv.OrderNo = po.NO;
                    pomv.SupplierCode = po.Supplier.Code;
                    pomv.SupplierName = po.Supplier.Name;
                    pomv.SupplierId = po.SupplierId;
                    pomv.Reference = po.Reference;
                    pomv.PaymentTermId = po.PaymentTermId;
                    pomv.WarehouseId = po.WarehouseId;
                    pomv.CurrencyId = po.CurrencyId;
                    pomv.BaseCurrencyCode = po.Currency.ISO_4217;
                    pomv.Currencyrate = po.Currencyrate;
                    pomv.TransactionCurrency = po.TransactionCurrency;
                    pomv.TransactionCurrencyCode = po.Currency1.ISO_4217;
                    pomv.Date = po.Date.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DueDate = po.DueDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DespatchDate = po.Date.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DeliveryName = po.DeliveryName;
                    pomv.StreetPoBox = po.StreetPoBox;
                    pomv.Suburb = po.Suburb;
                    pomv.City = po.City;
                    pomv.StateRegion = po.StateRegion;
                    pomv.Country = po.Country;
                    pomv.PostalCode = po.PostalCode;

                    //  pomv.TotalAmount = po.gra;


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

                    var podlist = db.PurchaseOrderDetails.Where(p => p.PurchaseOrderId == po.Id).ToList();
                    foreach (var pod in podlist)
                    {
                        var podmv = new PurchaseInvoiceDetailModelView();
                        // podmv.PurchaseReceiptId = pod.PurchaseReceiptId;
                        podmv.ItemId = pod.ItemId;
                        podmv.ItemCode = pod.Product.Code;
                        podmv.ItemName = pod.Product.Name;
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
                else if (from == "PR")
                {
                    PurchaseReceipt po = db.PurchaseReceipts.Find(id);
                    if (po == null)
                    {
                        return HttpNotFound();
                    }
                    PurchaseInvoiceModelView pomv = new PurchaseInvoiceModelView();
                    List<PurchaseInvoiceDetailModelView> podmvList = new List<PurchaseInvoiceDetailModelView>();

                    pomv.Id = 0;
                    //   pomv.NO = po.NO;
                    pomv.CreatedFrom = "Receipt Note";
                    pomv.ReferenceNo = po.Id;
                    pomv.OrderNo = po.NO;
                    pomv.SupplierCode = po.Supplier.Code;
                    pomv.SupplierName = po.Supplier.Name;
                    pomv.SupplierId = po.SupplierId;
                    pomv.Reference = po.Reference;
                    pomv.PaymentTermId = po.PaymentTermId;
                    pomv.WarehouseId = po.WarehouseId;
                    pomv.CurrencyId = po.CurrencyId;
                    pomv.BaseCurrencyCode = po.Currency.ISO_4217;
                    pomv.Currencyrate = po.Currencyrate;
                    pomv.TransactionCurrency = po.TransactionCurrency;
                    pomv.TransactionCurrencyCode = po.Currency1.ISO_4217;
                    pomv.Date = po.Date.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DueDate = po.DueDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DespatchDate = po.Date.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DeliveryName = po.DeliveryName;
                    pomv.StreetPoBox = po.StreetPoBox;
                    pomv.Suburb = po.Suburb;
                    pomv.City = po.City;
                    pomv.StateRegion = po.StateRegion;
                    pomv.Country = po.Country;
                    pomv.PostalCode = po.PostalCode;

                    //  pomv.TotalAmount = po.gra;


                    pomv.GrandTotal = po.GrandTotal;
                    pomv.BCGrandTotal = po.BCGrandTotal;
                    pomv.UserId = po.UserId;
                    pomv.BranchId = po.BranchId;
                    pomv.CompanyId = po.CompanyId;
                    //pomv.Status = po.Status;
                    //pomv.CreatedBy = po.CreatedBy;
                    //pomv.CreatedOn = po.CreatedOn;
                    //pomv.ModifiedBy = po.ModifiedBy;
                    //pomv.ModifiedOn = po.ModifiedOn;

                    var podlist = db.PurchaseReceiptDetails.Where(p => p.PurchaseReceiptId == po.Id).ToList();
                    var defaulttax = db.Taxes.FirstOrDefault(t => t.BranchId == 0 && t.CompanyId == 0);
                    foreach (var pod in podlist)
                    {
                        var podmv = new PurchaseInvoiceDetailModelView();
                        // podmv.PurchaseReceiptId = pod.PurchaseReceiptId;
                        podmv.ItemId = pod.ItemId;
                        podmv.ItemCode = pod.Product.Code;
                        podmv.ItemName = pod.Product.Name;
                        podmv.Description = pod.Description;
                        podmv.Quantity = pod.Quantity;
                        podmv.AccountId = pod.AccountId;
                        podmv.UnitId = pod.UnitId;
                        podmv.UnitName = pod.UOM.Code;
                        podmv.Price = pod.Price;
                        podmv.CurrencyRate = pod.CurrencyRate;
                        podmv.TaxId = defaulttax.TaxId;
                        podmv.TaxName = defaulttax.Name + '(' + defaulttax.Rate + "%)";
                        podmv.TaxPercent = defaulttax.Rate;
                        podmv.TaxAmount = defaulttax.Rate;

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
                else
                {
                    return HttpNotFound();
                }
            }
        }

        //
        // POST: /PurchaseInvoice/Create
        [SessionExpire]
        [HttpPost]
        public ActionResult Create(FormCollection poCollection)
        {
            //long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt32(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            long Createdby = Convert.ToInt32(Session["Createdid"]);
            string dateFormat = Session["DateFormat"].ToString();
            string culture = Session["DateCulture"].ToString();
            // System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            string currencyval = string.Empty;
            var taxes = db.Taxes.Where(d => (d.CompanyId == companyid)).Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate, NetEffective = d.NetEffective }).ToList();
            ViewBag.TaxSingle = taxes;
            var paymentTerms = db.PaymentTerms.Where(d => (d.CompanyId == companyid)).Select(d => new { Id = d.Id, PaymentTermDescription = d.PaymentTermDescription }).ToList();
            ViewBag.PaymentTerms = paymentTerms;
            var taxCompsList = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid)).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            ViewBag.Taxcomponents = taxCompsList;
            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.Ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.groupID == 117).Select(d => new { d.LID, d.ledgerName }).ToList();
            //   ViewBag.ReferencePO = GetYetToBeInvoicedPR();
            ViewBag.Branch = db.BranchMasters.Select(s => new { Value = s.Id, Text = s.Name }).ToList();

            PurchaseInvoiceModelView pomv = new PurchaseInvoiceModelView();
            List<PurchaseInvoiceDetailModelView> podmvList = new List<PurchaseInvoiceDetailModelView>();
            List<PurchaseCostingDetailModelView> pcdmvList = new List<PurchaseCostingDetailModelView>();
            List<InvoiceTotal> invoiceTotalList = new List<InvoiceTotal>();
            var sid = poCollection["SupplierId"];
            var tid = poCollection["TaxId"];
            var wid = poCollection["WarehouseId"];
            var pti = poCollection["PaymentTermId"];
            var rno = poCollection["ReferenceNo"];
            if (pti != "")
                pomv.PaymentTermId = Convert.ToInt32(pti);
            if (sid != "")
                pomv.SupplierId = Convert.ToInt32(sid);
            if (wid != "")
                pomv.WarehouseId = Convert.ToInt32(wid);
            if (rno != "")
                pomv.ReferenceNo = Convert.ToInt64(poCollection["ReferenceNo"]);
            pomv.CreatedFrom = poCollection["CreatedFrom"];
            pomv.NO = poCollection["NO"];
            pomv.Id = Convert.ToInt64(poCollection["Id"]);
            if (pomv.ReferenceNo != null && pomv.NO == "")
                pomv.Id = 0;
            pomv.OrderNo = poCollection["OrderNo"];
            pomv.ReceiptIds = poCollection["ReceiptIds"];
            pomv.PurchaseOrderNo = poCollection["PurchaseOrderNo"];

            if(pomv.PurchaseOrderNo != "")
                pomv.PurchaseOrderId = Convert.ToInt64(poCollection["PurchaseOrderId"]);
            
            long Branchid = Convert.ToInt64(poCollection["BranchId"]);

            pomv.SupplierName = poCollection["SupplierName"];
            pomv.SupplierCode = poCollection["SupplierCode"];
            pomv.Reference = poCollection["Reference"];
            pomv.ReferenceInvoice = poCollection["ReferenceInvoice"];
            pomv.Date = Convert.ToString(poCollection["Date"]);
            pomv.InvoiceDate = Convert.ToString(poCollection["InvoiceDate"]);
            pomv.DespatchDate = Convert.ToString(poCollection["DespatchDate"]);
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
            pomv.LID = Convert.ToInt32(poCollection["LID"]);
            pomv.DespatchNo = poCollection["DespatchNo"];
            pomv.DespatchThrough = poCollection["DespatchThrough"];
            pomv.DespatchDestination = poCollection["DespatchDestination"];

            var count = poCollection["producthide"].Split(',').Length;
            for (int i = 0; i < count; i++)
            {
                PurchaseInvoiceDetailModelView podmv = new PurchaseInvoiceDetailModelView();
                podmvList.Add(podmv);
            }
            if (poCollection["type"] != null)
            {
                var countcost = poCollection["type"].Split(',').Length;
                for (int i = 0; i < countcost; i++)
                {
                    PurchaseCostingDetailModelView pcdmv = new PurchaseCostingDetailModelView();
                    pcdmvList.Add(pcdmv);
                }
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


                    case "type":
                        for (int i = 0; i < lt; i++)
                        {

                            pcdmvList[i].CostingType = value[i];
                        }
                        break;

                    case "costhide":
                        for (int i = 0; i < lt; i++)
                        {

                            pcdmvList[i].CostName = value[i];
                        }
                        break;

                    case "cost":
                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                pcdmvList[i].CostingId = Convert.ToInt32(value[i]);
                        }
                        break;

                    case "costtaxhide":

                        for (int i = 0; i < lt; i++)
                        {

                            pcdmvList[i].TaxName = value[i];
                        }
                        break;

                    case "costtax":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                pcdmvList[i].TaxId = Convert.ToInt32(value[i]);
                        }
                        break;
                    case "costdescriptionhide":

                        for (int i = 0; i < lt; i++)
                        {

                            pcdmvList[i].Description = value[i];
                        }
                        break;
                    case "costamounthide":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                pcdmvList[i].CostAmount = Convert.ToDecimal(value[i]);
                        }
                        break;
                }
            }
            var checksupplier = db.Suppliers.Where(s => s.Code == pomv.SupplierCode && s.CompanyId == companyid).FirstOrDefault();
            var date = DateTime.ParseExact(pomv.Date, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var invoicedate = DateTime.ParseExact(pomv.InvoiceDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var despatchdate = DateTime.ParseExact(pomv.DespatchDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var getDateRange = db.FinancialYearMasters.Where(d => d.fYearID == Fyid).FirstOrDefault();

            if (!(invoicedate >= getDateRange.sDate && invoicedate <= getDateRange.eDate))
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
                //if (duedate < date)
                //    ViewBag.Date = "Delivery Date can not be less than Order Date ";
                return View(pomv);

            }
            //else if (duedate < date)
            //{
            //    var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            //    ViewBag.ddlWarehouses = warehouses;

            //    ViewBag.podmvList = podmvList;
            //    ViewBag.Date = "Delivery Date can not be less than Order Date ";
            //    return View(pomv);

            //}
            else
            {
                pomv.SupplierId = checksupplier.Id;

                pomv.TransactionCurrency = checksupplier.CurrencyRate.CurrencyId;
                pomv.Currencyrate = checksupplier.CurrencyRate.PurchaseRate;

            }

            //purchase invoice details model view validation
            // decimal quantityrequested = 0;
            foreach (var podmv in podmvList)
            {
                if (podmv.ItemId != 0 && podmv.Quantity > 0  && podmv.TaxId > 0)
                {
                    // quantityrequested += podmv.Quantity;
                    //do nothing
                }
                else
                {
                    if (!(podmv.ItemId == 0 && podmv.Quantity == 0  && podmv.TaxId == 0))
                    {
                        var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                        ViewBag.ddlWarehouses = warehouses;

                        ViewBag.podmvList = podmvList;
                        ViewBag.pcdmvList = pcdmvList;
                        return View(pomv);
                    }

                }
            }

            //purchase costing details model view validation
            foreach (var pcdmv in pcdmvList)
            {
                if (!(pcdmv.CostingId != 0 && pcdmv.CostAmount != 0 && pcdmv.TaxId != 0))
                {
                    var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                    ViewBag.ddlWarehouses = warehouses;
                    ViewBag.podmvList = podmvList;
                    ViewBag.pcdmvList = pcdmvList;
                    return View(pomv);
                }


            }

            //if (pomv.ReferenceNo != null)
            //{
            //    if (pomv.CreatedFrom == "Purchase Order")
            //    {
            //        var quantityinvoiced = db.PurchaseInvoiceDetails.Where(p => p.PurchaseInvoice.CreatedFrom == "Purchase Order" && p.PurchaseInvoice.ReferenceNo == pomv.ReferenceNo).Sum(p => (decimal?)p.Quantity) ?? 0;
            //        var quantityordered = db.PurchaseOrderDetails.Where(p => p.PurchaseOrder.Id == pomv.ReferenceNo).Sum(p => (decimal?)p.Quantity) ?? 0;
            //        if (quantityordered < quantityinvoiced + quantityrequested)
            //        {
            //            var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            //            ViewBag.ddlWarehouses = warehouses;

            //            ViewBag.podmvList = podmvList;
            //            ViewBag.ErrorTag = "Total quantity of  Invoice Items cannot exceed total qauntity ordered in Purchase Order " + pomv.OrderNo + " .";
            //            ViewBag.Message = "Error";
            //            return View(pomv);
            //        }
            //    }
            //    else
            //    {
            //        var quantityinvoiced = db.PurchaseInvoiceDetails.Where(p => p.PurchaseInvoice.CreatedFrom == "Receipt Note" && p.PurchaseInvoice.ReferenceNo == pomv.ReferenceNo).Sum(p => (decimal?)p.Quantity) ?? 0;
            //        var quantityordered = db.PurchaseReceiptDetails.Where(p => p.PurchaseReceipt.Id == pomv.ReferenceNo).Sum(p => (decimal?)p.Quantity) ?? 0;
            //        if (quantityordered < quantityinvoiced + quantityrequested)
            //        {
            //            var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            //            ViewBag.ddlWarehouses = warehouses;

            //            ViewBag.podmvList = podmvList;
            //            ViewBag.ErrorTag = "Total quantity of  Invoice Items cannot exceed total qauntity  in Receipt Note " + pomv.OrderNo + " .";
            //            ViewBag.Message = "Error";
            //            return View(pomv);
            //        }
            //    }
            //}

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

                            var fyear = db.FinancialYearMasters.FirstOrDefault(f => f.fYearID == Fyid).Year;
                            var fs = fyear.Substring(2, 2);
                            var es = fyear.Substring(7, 2);
                            fyear = fs + "-" + es;
                            int countpo = 1;

                            //&& p.BranchId == Branchid
                            if (db.PurchaseInvoices.Where(p => p.CompanyId == companyid && p.FinancialYearId == Fyid).Count() != 0)
                            {
                                countpo = (int)db.PurchaseInvoices.Where(p => p.CompanyId == companyid && p.FinancialYearId == Fyid).Max(p => p.InvoiceNo) + 1;
                            }
                            var getPrefix = db.Prefixes.Where(p => p.DefaultPrefix == "PC" && p.CompanyId == companyid).Select(p => new { p.DefaultPrefix, p.SetPrefix }).FirstOrDefault();

                            if (getPrefix.SetPrefix != null)
                                pomv.NO = getPrefix.SetPrefix + "/" + fyear + "/" + countpo;//tc.GenerateCode(fyear, countpo);
                            else
                                pomv.NO = getPrefix.DefaultPrefix + "/" + fyear + "/" + countpo; //tc.GenerateCode(fyear, countpo);


                            //if (duplicateInv)
                            //{
                            //    var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                            //    ViewBag.ddlWarehouses = warehouses1;

                            //    ViewBag.podmvList = podmvList;
                            //    ViewBag.pcdmvList = pcdmvList;
                            //    ViewBag.Message = "Purchase Invoice No Cannot be duplicate.";
                            //    return View(pomv);
                            //}

                            //Insert into purchaseorder table
                            PurchaseInvoice po = new PurchaseInvoice();
                            po.NO = pomv.NO;
                            po.ReferenceInvoice = pomv.ReferenceInvoice;
                            po.SupplierId = pomv.SupplierId;
                            po.Reference = pomv.Reference;
                            po.InvoiceNo = countpo;
                            po.OrderNo = pomv.OrderNo;
                            po.PurchaseOrderId = pomv.PurchaseOrderId;
                            po.PurchaseOrderNo = pomv.PurchaseOrderNo;
                            po.ReceiptIds = pomv.ReceiptIds;
                            po.WarehouseId = pomv.WarehouseId;
                            po.Date = date;
                            po.DueDate = date;
                            po.InvoiceDate = invoicedate;
                            po.DespatchDate = despatchdate;
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
                            po.LID = pomv.LID;
                            po.CreatedBy = Createdby;
                            po.CreatedOn = DateTime.Now;
                            po.FinancialYearId = Fyid;
                            po.UserId = userid;
                            po.BranchId = Branchid;
                            po.CompanyId = companyid;
                            po.Status = pomv.Status;
                            po.PaymentTermId = pomv.PaymentTermId;
                            po.DespatchNo = pomv.DespatchNo;
                            po.DespatchThrough = pomv.DespatchThrough;
                            po.DespatchDestination = pomv.DespatchDestination;
                            po.Memo = pomv.Memo;
                            po.IsPaid = false;
                            db.PurchaseInvoices.Add(po);

                            db.SaveChanges();
                            var po1 = db.PurchaseInvoices.Find(po.Id);
                            pomv.Id = po.Id;
                            var receiveditems = new List<Stock>();
                            //if (pomv.CreatedFrom == "Receipt Note")
                            //{
                            //    receiveditems =  db.Stocks.Where(p =>p.TransTag=="PR" &&  p.TranId == pomv.ReferenceNo ).ToList();
                            //}
                            if (pomv.ReceiptIds != "")
                            {
                                string[] value = pomv.ReceiptIds.Split(',');
                                int[] myInts = Array.ConvertAll(value, int.Parse);
                                var countpr = myInts.Length;

                                for (int i = 0; i < countpr; i++)
                                {
                                    var purchaseReceipt = db.PurchaseReceipts.Find(myInts[i]);
                                    if (purchaseReceipt != null)
                                    {
                                        purchaseReceipt.Status = InventoryConst.cns_Invoiced;


                                        var purchaseorder = db.PurchaseOrders.Where(p => p.Id == purchaseReceipt.ReferenceNo).FirstOrDefault();

                                        var getAllReceived = db.PurchaseReceipts.Where(d => d.ReferenceNo == purchaseReceipt.ReferenceNo).Select(d => new {d.Id, d.Status }).ToList();
                                        var getAllReceivedCount = getAllReceived.Count()-1;
                                        var getInvoicedReceivedCount = getAllReceived.Where(d => d.Status == "Invoiced").Count();

                                        decimal getOrderedQuantity = db.PurchaseOrderDetails.Where(d => d.PurchaseOrderId == purchaseReceipt.ReferenceNo).Sum(d => d.Quantity);



                                        decimal getReceivedQuantity = db.PurchaseReceiptDetails.Where(d => d.PurchaseReceipt.ReferenceNo == purchaseReceipt.ReferenceNo).Sum(d => d.Quantity);

                                        if (purchaseorder != null)
                                        {
                                            if (getAllReceivedCount == getInvoicedReceivedCount)
                                            {
                                                if (getReceivedQuantity <= getOrderedQuantity)
                                                {
                                                    var getShrinkage = (getOrderedQuantity - getReceivedQuantity) * 100 / getOrderedQuantity;
                                                    if (getShrinkage <= 5)
                                                        purchaseorder.Status = InventoryConst.cns_Invoiced;
                                                    else
                                                        purchaseorder.Status = InventoryConst.cns_Partially_Invoiced;
                                                }
                                                else
                                                {
                                                    purchaseorder.Status = InventoryConst.cns_Invoiced;
                                                }
                                            }
                                            else
                                            {
                                                purchaseorder.Status = InventoryConst.cns_Partially_Invoiced;
                                            }
                                        }
                                    }

                                }
                            }
                            // po1.OrderNo = preceipts;

                            decimal? subtotal = 0;
                            decimal totaltaxonproduct = 0;

                            foreach (var podmv in podmvList)
                            {
                                if (podmv.ItemId != 0 && podmv.Quantity > 0)
                                {

                                    decimal taxrate = taxes.FirstOrDefault(r => r.TaxId == podmv.TaxId).Rate;
                                    PurchaseInvoiceDetail pod = new PurchaseInvoiceDetail();
                                    pod.PurchaseInvoiceId = po.Id;
                                    pod.ItemId = podmv.ItemId;
                                    pod.Description = podmv.Description;
                                    pod.BarCode = podmv.BarCode;
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
                                                totalamount += Math.Round((decimal)amount1, 2, MidpointRounding.AwayFromZero);

                                            }
                                            if (IsDependTax == true && IsCompoundedTax == false)
                                            {
                                                var crate = parentEffectiveRate * rate / 100;
                                                effectiveTotal += crate;
                                                amount1 = crate;
                                                totalamount += Math.Round((decimal)amount1, 2, MidpointRounding.AwayFromZero);
                                            }
                                            if (IsDependTax == false && IsCompoundedTax == true)
                                            {
                                                var crate = (effectiveTotal + compounded) * rate / 100;
                                                effectiveTotal += crate;
                                                parentEffectiveRate = crate;
                                                amount1 = parentEffectiveRate;
                                                totalamount += Math.Round((decimal)amount1, 2, MidpointRounding.AwayFromZero);
                                            }
                                            var purchaseTax = new PurchaseTax();
                                            purchaseTax.PurchaseInvoiceId = pomv.Id;
                                            purchaseTax.ItemId = podmv.ItemId;
                                            purchaseTax.TaxId = (long)taxComp.TaxCompId;
                                            purchaseTax.Amount = Math.Round((decimal)amount1, 2, MidpointRounding.AwayFromZero);
                                            purchaseTax.CurrencyRate = pomv.Currencyrate;
                                            if (po.Status == "Saved")
                                            {
                                                db.PurchaseTaxes.Add(purchaseTax);
                                            }
                                            var tname = taxes.FirstOrDefault(t => t.TaxId == purchaseTax.TaxId);
                                            var invoiceTotal = new InvoiceTotal { Id = 7, Name = tname.Name + '(' + tname.Rate + "%)" + " On Product", Amount = (decimal)purchaseTax.Amount };
                                            invoiceTotalList.Add(invoiceTotal);
                                        }

                                        pod.TaxPercent = (decimal)effectivetaxrate;
                                        pod.TaxAmount = (decimal)totalamount;
                                        pod.TaxAmount = Math.Round(pod.TaxAmount, 2, MidpointRounding.AwayFromZero);



                                    }
                                    else
                                    {
                                        var purchaseTax = new PurchaseTax();
                                        purchaseTax.PurchaseInvoiceId = pomv.Id;
                                        purchaseTax.ItemId = podmv.ItemId;
                                        purchaseTax.TaxId = podmv.TaxId;
                                        decimal px = (decimal)taxrate * (decimal)compounded / 100;
                                        purchaseTax.Amount = Math.Round(px, 2, MidpointRounding.AwayFromZero);
                                        purchaseTax.CurrencyRate = pomv.Currencyrate;
                                        if (po.Status == "Saved")
                                        {
                                            db.PurchaseTaxes.Add(purchaseTax);
                                        }
                                        var tname = taxes.FirstOrDefault(t => t.TaxId == purchaseTax.TaxId);
                                        var invoiceTotal = new InvoiceTotal { Id = 7, Name = tname.Name + '(' + tname.Rate + "%)" + " On Product", Amount = (decimal)purchaseTax.Amount };
                                        invoiceTotalList.Add(invoiceTotal);
                                        pod.TaxPercent = (decimal)taxrate;
                                        pod.TaxAmount = (decimal)taxrate * (decimal)compounded / 100;
                                        pod.TaxAmount = Math.Round(pod.TaxAmount, 2, MidpointRounding.AwayFromZero);
                                    }
                                    totaltaxonproduct += pod.TaxAmount;
                                    pod.TotalAmount = (pod.Price * pod.Quantity) + pod.TaxAmount;
                                    db.PurchaseInvoiceDetails.Add(pod);

                                    if (po.Status == "Saved")
                                    {

                                        var stock = new Stock();
                                        stock.ArticleID = podmv.ItemId;
                                        stock.Items = podmv.Quantity * podmv.UnitFormula;
                                        stock.Price = podmv.Price;
                                        stock.TranCode = "IN";
                                        stock.TransTag = "PI";
                                        stock.TranDate = date;
                                        stock.TranId = po.Id;
                                        stock.WarehouseId = po.WarehouseId;
                                        stock.UserId = po.UserId;
                                        stock.CompanyId = po.CompanyId;
                                        stock.BranchId = po.BranchId;
                                        stock.CreatedBy = po.CreatedBy;
                                        db.Stocks.Add(stock);
                                        if (pomv.CreatedFrom == "Receipt Note")
                                        {

                                            if (receiveditems != null)
                                            {
                                                var receiveditem = receiveditems.Where(p => p.ArticleID == podmv.ItemId).FirstOrDefault();
                                                if (receiveditem != null)
                                                {

                                                    if (receiveditem.Items == podmv.Quantity)
                                                    {
                                                        receiveditem.Items = 0;
                                                        db.Stocks.Remove(receiveditem);
                                                        receiveditems.Remove(receiveditem);
                                                    }
                                                    //    receiveditem.Items = 0;
                                                    else if (receiveditem.Items > podmv.Quantity)
                                                    {
                                                        receiveditem.Items = receiveditem.Items - podmv.Quantity;
                                                    }
                                                    else
                                                    {
                                                        pomv.NO = "";
                                                        var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                                        ViewBag.ddlWarehouses = warehouses1;

                                                        ViewBag.podmvList = podmvList;
                                                        ViewBag.pcdmvList = pcdmvList;
                                                        ViewBag.ErrorTag = "Item " + podmv.ItemName + " cannot exceed Quantity of " + receiveditem.Items;
                                                        ViewBag.Message = "Error";
                                                        return View(pomv);
                                                    }


                                                }
                                                else
                                                {
                                                    pomv.NO = "";
                                                    var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                                    ViewBag.ddlWarehouses = warehouses1;

                                                    ViewBag.podmvList = podmvList;
                                                    ViewBag.pcdmvList = pcdmvList;
                                                    ViewBag.ErrorTag = "Item " + podmv.ItemName + " is not in Purchase Receipt " + pomv.OrderNo + "  or already invoiced.";
                                                    ViewBag.Message = "Error";
                                                    return View(pomv);
                                                }



                                            }
                                            else
                                            {
                                                pomv.NO = "";
                                                var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                                ViewBag.ddlWarehouses = warehouses1;

                                                ViewBag.podmvList = podmvList;
                                                ViewBag.pcdmvList = pcdmvList;
                                                ViewBag.ErrorTag = "Item " + podmv.ItemName + " is not in Purchase Receipt " + pomv.OrderNo + "  or already invoiced.";
                                                ViewBag.Message = "Error";
                                                return View(pomv);
                                            }
                                        }
                                        var findSupplierProductPrice = db.SupplierProductPrices.Find(po.SupplierId, podmv.ItemId, podmv.UnitFormula);
                                        if (findSupplierProductPrice == null)
                                        {
                                            var supplierProductPrice = new SupplierProductPrice();
                                            supplierProductPrice.SupplierId = (long)po.SupplierId;
                                            supplierProductPrice.ProductId = podmv.ItemId;
                                            supplierProductPrice.UnitFormula = podmv.UnitFormula;
                                            supplierProductPrice.PurchasePrice = podmv.Price;
                                            supplierProductPrice.UserId = po.UserId;
                                            supplierProductPrice.CompanyId = po.CompanyId;
                                            supplierProductPrice.BranchId = po.BranchId;
                                            db.SupplierProductPrices.Add(supplierProductPrice);
                                        }
                                        else
                                        {
                                            findSupplierProductPrice.PurchasePrice = podmv.Price;
                                        }
                                    }
                                }

                            }
                            decimal totaladdamount = 0;
                            decimal totaldeductamount = 0;
                            decimal totaltaxonother = 0;

                            foreach (var pcdmv in pcdmvList)
                            {
                                if (pcdmv.CostingId != 0 && pcdmv.TaxId > 0 && pcdmv.CostAmount > 0)
                                {
                                    decimal taxrate = taxes.FirstOrDefault(r => r.TaxId == pcdmv.TaxId).Rate;
                                    PurchaseCostingDetail pcd = new PurchaseCostingDetail();
                                    pcd.PurchaseInvoiceId = po.Id;
                                    pcd.CostingId = pcdmv.CostingId;
                                    pcd.CostingType = pcdmv.CostingType;
                                    pcd.Description = pcdmv.Description;
                                    pcd.TaxId = pcdmv.TaxId;

                                    pcd.CurrencyRate = po.Currencyrate;
                                    pcd.CostAmount = pcdmv.CostAmount;

                                    if (pcd.CostingType == "ADD")
                                    {
                                        totaladdamount += pcd.CostAmount;

                                    }
                                    else
                                    {
                                        totaldeductamount += pcd.CostAmount;

                                    }
                                    var taxComps = taxCompsList.Where(t => t.TaxId == pcdmv.TaxId).OrderBy(t => t.TaxrateId).ToList();
                                    if (taxComps.Count != 0)
                                    {
                                        decimal? effectiveTotal = 0;
                                        decimal? parentEffectiveRate = 0;
                                        decimal? totalamount = 0;
                                        decimal? effectivetaxrate = taxes.FirstOrDefault(r => r.TaxId == pcdmv.TaxId).NetEffective;
                                        foreach (var taxComp in taxComps)
                                        {
                                            decimal? rate = taxComp.Taxrate1;
                                            var IsDependTax = taxComp.IsDependTax;
                                            var IsCompoundedTax = taxComp.IsCompoundedTax;
                                            decimal? amount1 = 0;

                                            if (taxComp.IsDependTax == false && taxComp.IsCompoundedTax == false)
                                            {

                                                var crate = rate;
                                                parentEffectiveRate = crate * pcdmv.CostAmount / 100;
                                                effectiveTotal += parentEffectiveRate;
                                                amount1 = parentEffectiveRate;
                                                totalamount += Math.Round((decimal)amount1, 2, MidpointRounding.AwayFromZero);
                                            }
                                            if (IsDependTax == true && IsCompoundedTax == false)
                                            {
                                                var crate = parentEffectiveRate * rate / 100;
                                                effectiveTotal += crate;
                                                amount1 = crate;
                                                totalamount += Math.Round((decimal)amount1, 2, MidpointRounding.AwayFromZero);
                                            }
                                            if (IsDependTax == false && IsCompoundedTax == true)
                                            {
                                                var crate = (effectiveTotal + pcdmv.CostAmount) * rate / 100;
                                                effectiveTotal += crate;
                                                parentEffectiveRate = crate;
                                                amount1 = parentEffectiveRate;
                                                totalamount += Math.Round((decimal)amount1, 2, MidpointRounding.AwayFromZero);
                                            }

                                            var purchaseTax = new PurchaseTax();
                                            purchaseTax.PurchaseInvoiceId = pomv.Id;
                                            purchaseTax.CostId = pcdmv.CostingId;
                                            purchaseTax.TaxId = (long)taxComp.TaxCompId;
                                            purchaseTax.Amount = Math.Round((decimal)amount1, 2, MidpointRounding.AwayFromZero);
                                            purchaseTax.CurrencyRate = pomv.Currencyrate;

                                            db.PurchaseTaxes.Add(purchaseTax);
                                            var tname = taxes.FirstOrDefault(t => t.TaxId == purchaseTax.TaxId);
                                            var invoiceTotal = new InvoiceTotal { Id = 10, Name = tname.Name + '(' + tname.Rate + "%)" + " On Others", Amount = (decimal)purchaseTax.Amount };
                                            invoiceTotalList.Add(invoiceTotal);
                                        }
                                        pcd.TaxPercent = (decimal)effectivetaxrate;
                                        pcd.TaxAmount = (decimal)totalamount;
                                    }
                                    else
                                    {

                                        var purchaseTax = new PurchaseTax();
                                        purchaseTax.PurchaseInvoiceId = pomv.Id;
                                        purchaseTax.CostId = pcdmv.CostingId;
                                        purchaseTax.TaxId = pcdmv.TaxId;
                                        decimal px = (decimal)taxrate * (decimal)pcdmv.CostAmount / 100;
                                        purchaseTax.Amount = Math.Round(px, 2, MidpointRounding.AwayFromZero);
                                        purchaseTax.CurrencyRate = pomv.Currencyrate;

                                        db.PurchaseTaxes.Add(purchaseTax);

                                        pcd.TaxPercent = (decimal)taxrate;
                                        pcd.TaxAmount = (decimal)purchaseTax.Amount;

                                        var tname = taxes.FirstOrDefault(t => t.TaxId == purchaseTax.TaxId);
                                        var invoiceTotal = new InvoiceTotal { Id = 10, Name = tname.Name + '(' + tname.Rate + "%)" + " On Others", Amount = (decimal)purchaseTax.Amount };
                                        invoiceTotalList.Add(invoiceTotal);
                                    }
                                    totaltaxonother += pcd.TaxAmount;
                                    db.PurchaseCostingDetails.Add(pcd);

                                }

                            }
                            po1.TaxProduct = totaltaxonproduct;
                            po1.SubTotal = (decimal)subtotal;
                            if (subtotal > 0)
                            {
                                var invoiceTotal = new InvoiceTotal { Id = 1, Name = "Sub Total", Amount = (decimal)subtotal };
                                invoiceTotalList.Add(invoiceTotal);
                            }
                            var invoiceTotal1 = new InvoiceTotal { Id = 8, Name = "Added Cost", Amount = totaladdamount };
                            invoiceTotalList.Add(invoiceTotal1);
                            var invoiceTotal2 = new InvoiceTotal { Id = 9, Name = "Deducted Cost", Amount = -totaldeductamount };
                            invoiceTotalList.Add(invoiceTotal2);
                            po1.TotalAddAmount = totaladdamount;
                            po1.TotalDeductAmount = totaldeductamount;
                            po1.TaxOther = totaltaxonother;
                            decimal roundamount = 0;
                            decimal unroundedtotal = (decimal)(subtotal + totaltaxonproduct + totaladdamount - totaldeductamount + totaltaxonother);
                            //decimal roundedtotal = Math.Round(unroundedtotal);
                            //if (unroundedtotal != roundedtotal)
                            //{
                            //    roundamount = roundedtotal - unroundedtotal;
                            //    var invoiceTotal3 = new InvoiceTotal { Id = 11, Name = "Round Off", Amount = roundamount };
                            //    invoiceTotalList.Add(invoiceTotal3);

                            //}
                            //po1.RoundOff = roundamount;

                            po1.GrandTotal = unroundedtotal;
                            po1.BCGrandTotal = Math.Round(unroundedtotal * pomv.Currencyrate,2, MidpointRounding.AwayFromZero);
                            var invoiceTotal4 = new InvoiceTotal { Id = 12, Name = "Grand Total In " + pomv.TransactionCurrencyCode, Amount = unroundedtotal };
                            invoiceTotalList.Add(invoiceTotal4);
                            if (pomv.CurrencyId != pomv.TransactionCurrency)
                            {

                                var invoiceTotal5 = new InvoiceTotal { Id = 13, Name = "Grand Total In " + pomv.BaseCurrencyCode, Amount = po1.BCGrandTotal };
                                invoiceTotalList.Add(invoiceTotal5);
                            }

                            if (pomv.CreatedFrom == "Purchase Order")
                            {
                                po.CreatedFrom = "Purchase Order";
                                po.ReferenceNo = pomv.ReferenceNo;
                                po.OrderNo = pomv.OrderNo;
                                var findpo = db.PurchaseOrders.Find(pomv.ReferenceNo);
                                findpo.Status = InventoryConst.cns_Invoiced;
                            }
                            if (pomv.CreatedFrom == "Receipt Note")
                            {
                                po.CreatedFrom = "Receipt Note";
                                po.ReferenceNo = pomv.ReferenceNo;
                                po.OrderNo = pomv.OrderNo;
                                var findpo = db.PurchaseReceipts.Find(pomv.ReferenceNo);
                                findpo.Status = InventoryConst.cns_Invoiced;
                            }
                        }
                        else
                        {
                            //Update Purchase Invoice
                            var po = db.PurchaseInvoices.Find(pomv.Id);
                            // string oldStatus=po.Status;
                            //check permission
                            //if (po.Status == "Saved")
                            //{
                            //    var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                            //    ViewBag.ddlWarehouses = warehouses1;

                            //    ViewBag.podmvList = podmvList;
                            //    ViewBag.pcdmvList = pcdmvList;
                            //    ViewBag.Message = "No modification is allowed in posted Purchase Invoice.";
                            //    return View(pomv);
                            //}

                            var receiveditems = new List<Stock>();
                            var invoiceditems = new List<Stock>();
                            if (po.Status == "Saved")
                            {
                                if (pomv.CreatedFrom == "Receipt Note")
                                {
                                    receiveditems = db.Stocks.Where(p => p.TransTag == "PR" && p.TranId == pomv.ReferenceNo).ToList();
                                    invoiceditems = db.Stocks.Where(p => p.TransTag == "PI" && p.TranId == po.Id).ToList();
                                }
                                else
                                {
                                    var oldstocks = db.Stocks.Where(p => p.TransTag == "PI" && p.TranId == po.Id).ToList();
                                    foreach (var oldstock in oldstocks)
                                    {
                                        db.Stocks.Remove(oldstock);
                                    }
                                }
                            }

                            //if (po.ReceiptIds != "")
                            //{
                            //    string[] value = po.ReceiptIds.Split(',');
                            //    int[] myInts = Array.ConvertAll(value, int.Parse);
                            //    var countpr = myInts.Length;

                            //    for (int i = 0; i < countpr; i++)
                            //    {
                            //        var purchaseReceipt = db.PurchaseReceipts.Find(myInts[i]);
                            //        if (purchaseReceipt != null)
                            //        {
                            //            purchaseReceipt.Status = InventoryConst.cns_Saved;

                            //            var purchaseorder = db.PurchaseOrders.Where(p => p.Id == purchaseReceipt.ReferenceNo).FirstOrDefault();
                            //            if (purchaseorder != null)
                            //                purchaseorder.Status = InventoryConst.cns_Delivered;
                            //        }
                            //    }
                            //}
                            //if (pomv.ReceiptIds != "")
                            //{
                            //    string[] value1 = pomv.ReceiptIds.Split(',');
                            //    int[] myInts1 = Array.ConvertAll(value1, int.Parse);
                            //    var countpr1 = myInts1.Length;

                            //    for (int i = 0; i < countpr1; i++)
                            //    {
                            //        var purchaseReceipt = db.PurchaseReceipts.Find(myInts1[i]);
                            //        if (purchaseReceipt != null)
                            //        {
                            //            purchaseReceipt.Status = InventoryConst.cns_Invoiced;

                            //            var purchaseorder = db.PurchaseOrders.Where(p => p.Id == purchaseReceipt.ReferenceNo).FirstOrDefault();
                            //            if (purchaseorder != null)
                            //                purchaseorder.Status = InventoryConst.cns_Invoiced;
                            //        }
                            //    }

                            //}
                            po.NO = pomv.NO;
                            po.SupplierId = pomv.SupplierId;
                            po.Reference = pomv.Reference;
                            po.OrderNo = pomv.OrderNo;
                            po.PurchaseOrderId = pomv.PurchaseOrderId;
                            po.PurchaseOrderNo = pomv.PurchaseOrderNo;
                            po.ReferenceInvoice = pomv.ReferenceInvoice;
                            po.WarehouseId = pomv.WarehouseId;
                            po.Date = date;
                            po.DueDate = date;
                            po.InvoiceDate = invoicedate;
                            po.DespatchDate = despatchdate;
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
                            po.LID = pomv.LID;
                            po.ModifiedBy = Createdby;
                            po.ModifiedOn = DateTime.Now;
                            po.FinancialYearId = Fyid;
                            po.UserId = userid;
                            po.BranchId = Branchid;
                            po.CompanyId = companyid;
                            po.Status = pomv.Status;
                            po.PaymentTermId = pomv.PaymentTermId;
                            po.DespatchNo = pomv.DespatchNo;
                            po.DespatchThrough = pomv.DespatchThrough;
                            po.DespatchDestination = pomv.DespatchDestination;
                            po.Memo = pomv.Memo;
                            po.IsPaid = false;
                            var podOldRecords = db.PurchaseInvoiceDetails.Where(p => p.PurchaseInvoiceId == po.Id).ToList();
                            foreach (var podOld in podOldRecords)
                            {
                                db.PurchaseInvoiceDetails.Remove(podOld);
                            }

                            var purchaseTaxOldRecords = db.PurchaseTaxes.Where(p => p.PurchaseInvoiceId == po.Id).ToList();
                            foreach (var podOld in purchaseTaxOldRecords)
                            {
                                db.PurchaseTaxes.Remove(podOld);
                            }

                            decimal? subtotal = 0;
                            decimal totaltaxonproduct = 0;

                            foreach (var podmv in podmvList)
                            {
                                if (podmv.ItemId != 0 && podmv.Quantity > 0)
                                {

                                    decimal taxrate = taxes.FirstOrDefault(r => r.TaxId == podmv.TaxId).Rate;
                                    PurchaseInvoiceDetail pod = new PurchaseInvoiceDetail();
                                    pod.PurchaseInvoiceId = po.Id;
                                    pod.ItemId = podmv.ItemId;
                                    pod.Description = podmv.Description;
                                    pod.BarCode = podmv.BarCode;
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
                                                totalamount += Math.Round((decimal)amount1, 2, MidpointRounding.AwayFromZero);

                                            }
                                            if (IsDependTax == true && IsCompoundedTax == false)
                                            {
                                                var crate = parentEffectiveRate * rate / 100;
                                                effectiveTotal += crate;
                                                amount1 = crate;
                                                totalamount += Math.Round((decimal)amount1, 2, MidpointRounding.AwayFromZero);
                                            }
                                            if (IsDependTax == false && IsCompoundedTax == true)
                                            {
                                                var crate = (effectiveTotal + compounded) * rate / 100;
                                                effectiveTotal += crate;
                                                parentEffectiveRate = crate;
                                                amount1 = parentEffectiveRate;
                                                totalamount += Math.Round((decimal)amount1, 2,MidpointRounding.AwayFromZero);
                                            }
                                            var purchaseTax = new PurchaseTax();
                                            purchaseTax.PurchaseInvoiceId = pomv.Id;
                                            purchaseTax.ItemId = podmv.ItemId;
                                            purchaseTax.TaxId = (long)taxComp.TaxCompId;
                                            purchaseTax.Amount = Math.Round((decimal)amount1, 2, MidpointRounding.AwayFromZero);
                                            purchaseTax.CurrencyRate = pomv.Currencyrate;
                                            if (po.Status == "Saved")
                                            {
                                                db.PurchaseTaxes.Add(purchaseTax);
                                            }
                                            var tname = taxes.FirstOrDefault(t => t.TaxId == purchaseTax.TaxId);
                                            var invoiceTotal = new InvoiceTotal { Id = 7, Name = tname.Name + '(' + tname.Rate + "%)" + " On Product", Amount = (decimal)purchaseTax.Amount };
                                            invoiceTotalList.Add(invoiceTotal);
                                        }
                                        pod.TaxPercent = (decimal)effectivetaxrate;
                                        pod.TaxAmount = (decimal)totalamount;
                                        pod.TaxAmount = Math.Round(pod.TaxAmount, 2, MidpointRounding.AwayFromZero);



                                    }
                                    else
                                    {
                                        var purchaseTax = new PurchaseTax();
                                        purchaseTax.PurchaseInvoiceId = pomv.Id;
                                        purchaseTax.ItemId = podmv.ItemId;
                                        purchaseTax.TaxId = podmv.TaxId;
                                        decimal px = (decimal)taxrate * (decimal)compounded / 100;
                                        purchaseTax.Amount = Math.Round(px, 2, MidpointRounding.AwayFromZero);
                                        purchaseTax.CurrencyRate = pomv.Currencyrate;
                                        if (po.Status == "Saved")
                                        {
                                            db.PurchaseTaxes.Add(purchaseTax);
                                        }
                                        var tname = taxes.FirstOrDefault(t => t.TaxId == purchaseTax.TaxId);
                                        var invoiceTotal = new InvoiceTotal { Id = 7, Name = tname.Name + '(' + tname.Rate + "%)" + " On Product", Amount = (decimal)purchaseTax.Amount };
                                        invoiceTotalList.Add(invoiceTotal);
                                        pod.TaxPercent = (decimal)taxrate;
                                        pod.TaxAmount = (decimal)taxrate * (decimal)compounded / 100;
                                        pod.TaxAmount = Math.Round(pod.TaxAmount, 2, MidpointRounding.AwayFromZero);
                                    }
                                    totaltaxonproduct += pod.TaxAmount;
                                    pod.TotalAmount = (pod.Price * pod.Quantity) + pod.TaxAmount;
                                    db.PurchaseInvoiceDetails.Add(pod);

                                    if (po.Status == "Saved")
                                    {

                                        if (pomv.CreatedFrom == "Receipt Note")
                                        {
                                            if (invoiceditems != null)
                                            {

                                                var invoiceditem = invoiceditems.Where(p => p.ArticleID == podmv.ItemId).FirstOrDefault();
                                                if (invoiceditem != null)
                                                {
                                                    if (receiveditems != null)
                                                    {
                                                        var receiveditem = receiveditems.Where(p => p.ArticleID == podmv.ItemId).FirstOrDefault();
                                                        if (receiveditem != null)
                                                        {
                                                            receiveditem.Items = receiveditem.Items + invoiceditem.Items; //+ podmv.Quantity;

                                                            if (receiveditem.Items > podmv.Quantity)
                                                            {
                                                                receiveditem.Items = receiveditem.Items - podmv.Quantity;
                                                            }
                                                            else
                                                            {
                                                                var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                                                ViewBag.ddlWarehouses = warehouses1;

                                                                ViewBag.podmvList = podmvList;
                                                                ViewBag.pcdmvList = pcdmvList;
                                                                ViewBag.ErrorTag = "Item " + podmv.ItemName + " cannot exceed Quantity of " + receiveditem.Items;
                                                                ViewBag.Message = "Error";
                                                                return View(pomv);
                                                            }

                                                            invoiceditem.Items = podmv.Quantity;
                                                        }
                                                        else
                                                        {
                                                            var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                                            ViewBag.ddlWarehouses = warehouses1;

                                                            ViewBag.podmvList = podmvList;
                                                            ViewBag.pcdmvList = pcdmvList;
                                                            ViewBag.ErrorTag = "Item " + podmv.ItemName + " is not in  Recceipt Note " + pomv.OrderNo;
                                                            ViewBag.Message = "Error";
                                                            return View(pomv);
                                                        }



                                                    }
                                                    else
                                                    {
                                                        var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                                        ViewBag.ddlWarehouses = warehouses1;

                                                        ViewBag.podmvList = podmvList;
                                                        ViewBag.pcdmvList = pcdmvList;
                                                        ViewBag.ErrorTag = "Item " + podmv.ItemName + " is not in Purchase Receipt " + pomv.OrderNo;
                                                        ViewBag.Message = "Error";
                                                        return View(pomv);
                                                    }
                                                }
                                                else
                                                {
                                                    if (receiveditems != null)
                                                    {
                                                        var receiveditem = receiveditems.Where(p => p.ArticleID == podmv.ItemId).FirstOrDefault();
                                                        if (receiveditem != null)
                                                        {


                                                            if (receiveditem.Items > podmv.Quantity)
                                                            {
                                                                receiveditem.Items = receiveditem.Items - podmv.Quantity;
                                                            }
                                                            else
                                                            {
                                                                var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                                                ViewBag.ddlWarehouses = warehouses1;

                                                                ViewBag.podmvList = podmvList;
                                                                ViewBag.pcdmvList = pcdmvList;
                                                                ViewBag.ErrorTag = "Item " + podmv.ItemName + " cannot exceed Quantity of " + receiveditem.Items;
                                                                ViewBag.Message = "Error";
                                                                return View(pomv);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                                            ViewBag.ddlWarehouses = warehouses1;

                                                            ViewBag.podmvList = podmvList;
                                                            ViewBag.pcdmvList = pcdmvList;
                                                            ViewBag.ErrorTag = "Item " + podmv.ItemName + " is not in  Recceipt Note " + pomv.OrderNo;
                                                            ViewBag.Message = "Error";
                                                            return View(pomv);
                                                        }

                                                    }
                                                    var stock = new Stock();
                                                    stock.ArticleID = podmv.ItemId;
                                                    stock.Items = podmv.Quantity * podmv.UnitFormula;
                                                    stock.Price = podmv.Price;
                                                    stock.TranCode = "IN";
                                                    stock.TransTag = "PI";
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
                                            else
                                            {
                                                if (receiveditems != null)
                                                {
                                                    var receiveditem = receiveditems.Where(p => p.ArticleID == podmv.ItemId).FirstOrDefault();
                                                    if (receiveditem != null)
                                                    {

                                                        if (receiveditem.Items == podmv.Quantity)
                                                        {
                                                            receiveditem.Items = 0;
                                                            db.Stocks.Remove(receiveditem);
                                                            receiveditems.Remove(receiveditem);
                                                        }
                                                        ////    receiveditem.Items = 0;
                                                        else
                                                        if (receiveditem.Items >= podmv.Quantity)
                                                        {
                                                            receiveditem.Items = receiveditem.Items - podmv.Quantity;
                                                        }
                                                        else
                                                        {
                                                            var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                                            ViewBag.ddlWarehouses = warehouses1;

                                                            ViewBag.podmvList = podmvList;
                                                            ViewBag.pcdmvList = pcdmvList;
                                                            ViewBag.ErrorTag = "Item " + podmv.ItemName + " cannot exceed Quantity of " + receiveditem.Items;
                                                            ViewBag.Message = "Error";
                                                            return View(pomv);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                                        ViewBag.ddlWarehouses = warehouses1;

                                                        ViewBag.podmvList = podmvList;
                                                        ViewBag.pcdmvList = pcdmvList;
                                                        ViewBag.ErrorTag = "Item " + podmv.ItemName + " is not in  Receipt Note " + pomv.OrderNo;
                                                        ViewBag.Message = "Error";
                                                        return View(pomv);
                                                    }

                                                }
                                                var stock = new Stock();
                                                stock.ArticleID = podmv.ItemId;
                                                stock.Items = podmv.Quantity * podmv.UnitFormula;
                                                stock.Price = podmv.Price;
                                                stock.TranCode = "IN";
                                                stock.TransTag = "PI";
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
                                        else
                                        {
                                            var stock = new Stock();
                                            stock.ArticleID = podmv.ItemId;
                                            stock.Items = podmv.Quantity * podmv.UnitFormula;
                                            stock.Price = podmv.Price;
                                            stock.TranCode = "IN";
                                            stock.TransTag = "PI";
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
                                    if (po.Status == "Saved")
                                    {
                                        var findSupplierProductPrice = db.SupplierProductPrices.Find(po.SupplierId, podmv.ItemId, podmv.UnitFormula);
                                        if (findSupplierProductPrice == null)
                                        {
                                            var supplierProductPrice = new SupplierProductPrice();
                                            supplierProductPrice.SupplierId = (long)po.SupplierId;
                                            supplierProductPrice.ProductId = podmv.ItemId;
                                            supplierProductPrice.PurchasePrice = podmv.Price;
                                            supplierProductPrice.UnitFormula = podmv.UnitFormula;
                                            supplierProductPrice.UserId = po.UserId;
                                            supplierProductPrice.CompanyId = po.CompanyId;
                                            supplierProductPrice.BranchId = po.BranchId;
                                            db.SupplierProductPrices.Add(supplierProductPrice);
                                        }
                                        else
                                        {
                                            findSupplierProductPrice.PurchasePrice = podmv.Price;
                                        }
                                    }

                                }

                            }
                            var pcdOldRecords = db.PurchaseCostingDetails.Where(p => p.PurchaseInvoiceId == po.Id).ToList();
                            foreach (var pcdOld in pcdOldRecords)
                            {
                                db.PurchaseCostingDetails.Remove(pcdOld);
                            }
                            decimal totaladdamount = 0;
                            decimal totaldeductamount = 0;
                            decimal totaltaxonother = 0;

                            foreach (var pcdmv in pcdmvList)
                            {
                                if (pcdmv.CostingId != 0 && pcdmv.TaxId > 0 && pcdmv.CostAmount > 0)
                                {
                                    decimal taxrate = taxes.FirstOrDefault(r => r.TaxId == pcdmv.TaxId).Rate;
                                    PurchaseCostingDetail pcd = new PurchaseCostingDetail();
                                    pcd.PurchaseInvoiceId = po.Id;
                                    pcd.CostingId = pcdmv.CostingId;
                                    pcd.CostingType = pcdmv.CostingType;
                                    pcd.Description = pcdmv.Description;
                                    pcd.TaxId = pcdmv.TaxId;

                                    pcd.CurrencyRate = po.Currencyrate;
                                    pcd.CostAmount = pcdmv.CostAmount;

                                    if (pcd.CostingType == "ADD")
                                    {
                                        totaladdamount += pcd.CostAmount;

                                    }
                                    else
                                    {
                                        totaldeductamount += pcd.CostAmount;

                                    }
                                    var taxComps = taxCompsList.Where(t => t.TaxId == pcdmv.TaxId).OrderBy(t => t.TaxrateId).ToList();
                                    if (taxComps.Count != 0)
                                    {
                                        decimal? effectiveTotal = 0;
                                        decimal? parentEffectiveRate = 0;
                                        decimal? totalamount = 0;
                                        decimal? effectivetaxrate = taxes.FirstOrDefault(r => r.TaxId == pcdmv.TaxId).NetEffective;
                                        foreach (var taxComp in taxComps)
                                        {
                                            decimal? rate = taxComp.Taxrate1;
                                            var IsDependTax = taxComp.IsDependTax;
                                            var IsCompoundedTax = taxComp.IsCompoundedTax;
                                            decimal? amount1 = 0;

                                            if (taxComp.IsDependTax == false && taxComp.IsCompoundedTax == false)
                                            {

                                                var crate = rate;
                                                parentEffectiveRate = crate * pcdmv.CostAmount / 100;
                                                effectiveTotal += parentEffectiveRate;
                                                amount1 = parentEffectiveRate;
                                                totalamount += Math.Round((decimal)amount1, 2, MidpointRounding.AwayFromZero);
                                            }
                                            if (IsDependTax == true && IsCompoundedTax == false)
                                            {
                                                var crate = parentEffectiveRate * rate / 100;
                                                effectiveTotal += crate;
                                                amount1 = crate;
                                                totalamount += Math.Round((decimal)amount1, 2, MidpointRounding.AwayFromZero);
                                            }
                                            if (IsDependTax == false && IsCompoundedTax == true)
                                            {
                                                var crate = (effectiveTotal + pcdmv.CostAmount) * rate / 100;
                                                effectiveTotal += crate;
                                                parentEffectiveRate = crate;
                                                amount1 = parentEffectiveRate;
                                                totalamount += Math.Round((decimal)amount1, 2, MidpointRounding.AwayFromZero);
                                            }

                                            var purchaseTax = new PurchaseTax();
                                            purchaseTax.PurchaseInvoiceId = pomv.Id;
                                            purchaseTax.CostId = pcdmv.CostingId;
                                            purchaseTax.TaxId = (long)taxComp.TaxCompId;
                                            purchaseTax.Amount = Math.Round((decimal)amount1, 2, MidpointRounding.AwayFromZero);
                                            purchaseTax.CurrencyRate = pomv.Currencyrate;
                                            db.PurchaseTaxes.Add(purchaseTax);
                                            var tname = taxes.FirstOrDefault(t => t.TaxId == purchaseTax.TaxId);
                                            var invoiceTotal = new InvoiceTotal { Id = 10, Name = tname.Name + '(' + tname.Rate + "%)" + " On Others", Amount = (decimal)purchaseTax.Amount };
                                            invoiceTotalList.Add(invoiceTotal);
                                        }
                                        pcd.TaxPercent = (decimal)effectivetaxrate;
                                        pcd.TaxAmount = (decimal)totalamount;
                                    }
                                    else
                                    {

                                        var purchaseTax = new PurchaseTax();
                                        purchaseTax.PurchaseInvoiceId = pomv.Id;
                                        purchaseTax.CostId = pcdmv.CostingId;
                                        purchaseTax.TaxId = pcdmv.TaxId;
                                       
                                        decimal px = (decimal)taxrate * (decimal)pcdmv.CostAmount / 100;
                                        purchaseTax.Amount = Math.Round(px, 2, MidpointRounding.AwayFromZero);
                                        purchaseTax.CurrencyRate = pomv.Currencyrate;

                                        db.PurchaseTaxes.Add(purchaseTax);

                                        pcd.TaxPercent = (decimal)taxrate;
                                        pcd.TaxAmount = (decimal)purchaseTax.Amount;

                                        var tname = taxes.FirstOrDefault(t => t.TaxId == purchaseTax.TaxId);
                                        var invoiceTotal = new InvoiceTotal { Id = 10, Name = tname.Name + '(' + tname.Rate + "%)" + " On Others", Amount = (decimal)purchaseTax.Amount };
                                        invoiceTotalList.Add(invoiceTotal);
                                    }
                                    totaltaxonother += pcd.TaxAmount;
                                    db.PurchaseCostingDetails.Add(pcd);

                                }

                            }
                            po.TaxProduct = totaltaxonproduct;
                            po.SubTotal = (decimal)subtotal;
                            if (subtotal > 0)
                            {
                                var invoiceTotal = new InvoiceTotal { Id = 1, Name = "Sub Total", Amount = (decimal)subtotal };
                                invoiceTotalList.Add(invoiceTotal);
                            }
                            var invoiceTotal1 = new InvoiceTotal { Id = 8, Name = "Added Cost", Amount = totaladdamount };
                            invoiceTotalList.Add(invoiceTotal1);
                            var invoiceTotal2 = new InvoiceTotal { Id = 9, Name = "Deducted Cost", Amount = -totaldeductamount };
                            invoiceTotalList.Add(invoiceTotal2);
                            po.TotalAddAmount = totaladdamount;
                            po.TotalDeductAmount = totaldeductamount;
                            po.TaxOther = totaltaxonother;
                            decimal roundamount = 0;
                            decimal unroundedtotal = (decimal)(subtotal + totaltaxonproduct + totaladdamount - totaldeductamount + totaltaxonother);
                            //decimal roundedtotal = Math.Round(unroundedtotal);
                            //if (unroundedtotal != roundedtotal)
                            //{
                            //    roundamount = roundedtotal - unroundedtotal;
                            //    var invoiceTotal3 = new InvoiceTotal { Id = 11, Name = "Round Off", Amount = roundamount };
                            //    invoiceTotalList.Add(invoiceTotal3);

                            //}
                            //po.RoundOff = roundamount;
                            po.GrandTotal = unroundedtotal;
                            po.BCGrandTotal = Math.Round(unroundedtotal * pomv.Currencyrate,2, MidpointRounding.AwayFromZero);
                            var invoiceTotal4 = new InvoiceTotal { Id = 12, Name = "Grand Total In " + pomv.TransactionCurrencyCode, Amount = unroundedtotal };
                            invoiceTotalList.Add(invoiceTotal4);
                            if (pomv.CurrencyId != pomv.TransactionCurrency)
                            {

                                var invoiceTotal5 = new InvoiceTotal { Id = 13, Name = "Grand Total In " + pomv.BaseCurrencyCode, Amount = po.BCGrandTotal };
                                invoiceTotalList.Add(invoiceTotal5);
                            }
                        }

                    }
                    db.SaveChanges();
                    // everything good; complete
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
                ViewBag.pcdmvList = pcdmvList;
                ViewBag.ErrorTag = "Unable to save changes. Try again, and if the problem persists see your system administrator.";
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
                ViewBag.pcdmvList = pcdmvList;
                ViewBag.ErrorTag = "Unable to save changes. Try again, and if the problem persists see your system administrator.";
                ViewBag.Message = "Error";
                return View(pomv);
            }
            catch (Exception exp)
            {
                var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                ViewBag.ddlWarehouses = warehouses1;

                ViewBag.podmvList = podmvList;
                ViewBag.pcdmvList = pcdmvList;
                ViewBag.ErrorTag = "Unable to save changes. Try again, and if the problem persists see your system administrator.";
                ViewBag.Message = "Error";
                return View(pomv);
            }


            var warehouses2 = mc.getDdlWarehouses(companyid, Branchid);
            ViewBag.ddlWarehouses = warehouses2;

            ViewBag.podmvList = podmvList;
            ViewBag.pcdmvList = pcdmvList;
            ViewBag.Message = "You have successfully " + pomv.Status + " Purchase Invoice " + pomv.NO;
            return View(pomv);


        }

        //
        // GET: /PurchaseInvoice/Edit/5

        public ActionResult Edit(long id = 0)
        {
            PurchaseInvoice purchaseinvoice = db.PurchaseInvoices.Find(id);
            if (purchaseinvoice == null)
            {
                return HttpNotFound();
            }
            ViewBag.CurrencyId = new SelectList(db.Currencies, "CurrencyId", "Country", purchaseinvoice.CurrencyId);
            ViewBag.TransactionCurrency = new SelectList(db.Currencies, "CurrencyId", "Country", purchaseinvoice.TransactionCurrency);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "Id", "Code", purchaseinvoice.SupplierId);
         //   ViewBag.TaxId = new SelectList(db.Taxes, "TaxId", "Name", purchaseinvoice.TaxId);
            ViewBag.WarehouseId = new SelectList(db.Warehouses, "Id", "Code", purchaseinvoice.WarehouseId);
            return View(purchaseinvoice);
        }

        //
        // POST: /PurchaseInvoice/Edit/5

        [HttpPost]
        public ActionResult Edit(PurchaseInvoice purchaseinvoice)
        {
            if (ModelState.IsValid)
            {
                db.Entry(purchaseinvoice).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CurrencyId = new SelectList(db.Currencies, "CurrencyId", "Country", purchaseinvoice.CurrencyId);
            ViewBag.TransactionCurrency = new SelectList(db.Currencies, "CurrencyId", "Country", purchaseinvoice.TransactionCurrency);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "Id", "Code", purchaseinvoice.SupplierId);
           // ViewBag.TaxId = new SelectList(db.Taxes, "TaxId", "Name", purchaseinvoice.TaxId);
            ViewBag.WarehouseId = new SelectList(db.Warehouses, "Id", "Code", purchaseinvoice.WarehouseId);
            return View(purchaseinvoice);
        }

        //
        // GET: /PurchaseInvoice/Display/5
        public ActionResult Display(long? id = 0, string from = null)
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt32(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            var dateFormat = Session["DateFormat"].ToString();
            var culture = Session["DateCulture"].ToString();
            //System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            ViewBag.ddlWarehouses = warehouses;
            var taxes = db.Taxes.Where(d => (d.CompanyId == companyid && d.BranchId == Branchid) || (d.UserId == 0 && d.CompanyId == 0)).Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate }).ToList();
            ViewBag.TaxSingle = taxes;
            var paymentTerms = db.PaymentTerms.Where(d => (d.CompanyId == companyid && d.BranchId == Branchid) || (d.UserId == 0 && d.CompanyId == 0)).Select(d => new { Id = d.Id, PaymentTermDescription = d.PaymentTermDescription }).ToList();
            ViewBag.PaymentTerms = paymentTerms;
            var Taxcomponents = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid && d.Tax.BranchId == Branchid) || (d.Tax.CompanyId == 0 && d.Tax.BranchId == 0)).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            ViewBag.Taxcomponents = Taxcomponents;

            ViewBag.unit = db.UOMs.ToList();
            ViewBag.category = db.Categories.ToList();
            ViewBag.group = db.Groups.ToList();

            if (id == 0)
            {
                PurchaseInvoiceModelView pomv = new PurchaseInvoiceModelView();
                pomv.Date = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.DueDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.DespatchDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.CurrencyId = basecurrency.CurrencyId;
                pomv.BaseCurrencyCode = basecurrency.CurrencyCode;
                string countryname = string.Empty;
                var defaultwarehouse = db.Warehouses.Where(w =>  w.Companyid == companyid && w.Branchid == Branchid).FirstOrDefault();
                if (defaultwarehouse.Country != null)
                {
                    int c1 = Convert.ToInt32(defaultwarehouse.Country);
                    countryname = db.Countries.Where(c => c.CountryId == c1).Select(c => c.Country1).FirstOrDefault();
                }
                ViewBag.Id = defaultwarehouse.Id;
                ViewBag.AdressName = defaultwarehouse.AdressName;
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
                if (from == null)
                {
                    PurchaseInvoice po = db.PurchaseInvoices.Find(id);
                    if (po == null)
                    {
                        return HttpNotFound();
                    }
                    PurchaseInvoiceModelView pomv = new PurchaseInvoiceModelView();
                    List<PurchaseInvoiceDetailModelView> podmvList = new List<PurchaseInvoiceDetailModelView>();
                    List<PurchaseCostingDetailModelView> pcdmvList = new List<PurchaseCostingDetailModelView>();
                    List<InvoiceTotal> invoiceTotalList = new List<InvoiceTotal>();
                    pomv.Id = po.Id;
                    pomv.NO = po.NO;
                    pomv.CreatedFrom = po.CreatedFrom;
                    pomv.ReferenceNo = po.ReferenceNo;
                    pomv.OrderNo = po.OrderNo;
                    pomv.SupplierCode = po.Supplier.Code;
                    pomv.SupplierName = po.Supplier.Name;
                    pomv.SupplierId = po.SupplierId;
                    pomv.Reference = po.Reference;
                    pomv.PaymentTermId = po.PaymentTermId;
                    pomv.WarehouseId = po.WarehouseId;
                    pomv.CurrencyId = po.CurrencyId;
                    pomv.BaseCurrencyCode = po.Currency.ISO_4217;
                    pomv.Currencyrate = po.Currencyrate;
                    pomv.TransactionCurrency = po.TransactionCurrency;
                    pomv.TransactionCurrencyCode = po.Currency1.ISO_4217;
                    pomv.Date = po.Date.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DueDate = po.DueDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DespatchDate = po.DespatchDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DeliveryName = po.DeliveryName;
                    pomv.StreetPoBox = po.StreetPoBox;
                    pomv.Suburb = po.Suburb;
                    pomv.City = po.City;
                    pomv.StateRegion = po.StateRegion;
                    pomv.Country = po.Country;
                    pomv.PostalCode = po.PostalCode;
                    pomv.TaxProduct = po.TaxProduct;
                    pomv.TaxOther = po.TaxOther;
                    pomv.SubTotal = po.SubTotal;


                    pomv.GrandTotal = po.GrandTotal;
                    pomv.BCGrandTotal = po.BCGrandTotal;
                    pomv.UserId = po.UserId;
                    pomv.BranchId = po.BranchId;
                    pomv.CompanyId = po.CompanyId;
                    pomv.Status = po.Status;
                    //pomv.CreatedBy = po.CreatedBy;
                    //pomv.CreatedOn = po.CreatedOn;
                    //pomv.ModifiedBy = po.ModifiedBy;
                    //pomv.ModifiedOn = po.ModifiedOn;
                    pomv.Memo = po.Memo;
                    pomv.PaymentTermId = po.PaymentTermId;

                    var podlist = db.PurchaseInvoiceDetails.Where(p => p.PurchaseInvoiceId == po.Id).ToList();
                    foreach (var pod in podlist)
                    {
                        var podmv = new PurchaseInvoiceDetailModelView();
                        podmv.PurchaseInvoiceId = pod.PurchaseInvoiceId;
                        podmv.ItemId = pod.ItemId;
                        podmv.ItemCode = pod.Product.Code;
                        podmv.ItemName = pod.Product.Name;
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
                    var pcdlist = db.PurchaseCostingDetails.Where(p => p.PurchaseInvoiceId == po.Id).ToList();
                    foreach (var pcd in pcdlist)
                    {
                        var pcdmv = new PurchaseCostingDetailModelView();
                        pcdmv.PurchaseInvoiceId = pcd.PurchaseInvoiceId;
                        pcdmv.CostingId = pcd.CostingId;
                        pcdmv.CostName = pcd.Costing.Name;
                        pcdmv.Description = pcd.Description;
                        pcdmv.CostingType = pcd.CostingType;
                        pcdmv.CurrencyRate = pcd.CurrencyRate;
                        pcdmv.TaxId = pcd.TaxId;
                        pcdmv.TaxName = pcd.Tax.Name + '-' + pcd.Tax.TaxId;
                        pcdmv.TaxPercent = pcd.TaxPercent;
                        pcdmv.TaxAmount = pcd.TaxAmount;

                        pcdmv.TaxAmount = pcd.TaxAmount;

                        pcdmvList.Add(pcdmv);
                    }
                    ViewBag.pcdmvList = pcdmvList;

                    if (pomv.Status == "Saved")
                    {
                        if (pomv.SubTotal > 0)
                        {
                            var invoiceTotal = new InvoiceTotal { Id = 1, Name = "Sub Total", Amount = pomv.SubTotal };
                            invoiceTotalList.Add(invoiceTotal);
                        }
                        if (pomv.TotalAddAmount > 0)
                        {
                            var invoiceTotal1 = new InvoiceTotal { Id = 8, Name = "Added Cost", Amount = pomv.TotalAddAmount };
                            invoiceTotalList.Add(invoiceTotal1);
                        }
                        if (pomv.TotalDeductAmount > 0)
                        {
                            var invoiceTotal2 = new InvoiceTotal { Id = 9, Name = "Deducted Cost", Amount = -pomv.TotalDeductAmount };
                            invoiceTotalList.Add(invoiceTotal2);
                        }
                        if (pomv.RoundOff > 0)
                        {
                            var invoiceTotal = new InvoiceTotal { Id = 6, Name = "RoundOff", Amount = pomv.RoundOff };
                            invoiceTotalList.Add(invoiceTotal);
                        }
                        var invoiceTotal4 = new InvoiceTotal { Id = 12, Name = "Grand Total In " + pomv.TransactionCurrencyCode, Amount = pomv.GrandTotal };
                        invoiceTotalList.Add(invoiceTotal4);
                        if (pomv.CurrencyId != pomv.TransactionCurrency)
                        {

                            var invoiceTotal5 = new InvoiceTotal { Id = 13, Name = "Grand Total In " + pomv.BaseCurrencyCode, Amount = pomv.BCGrandTotal };
                            invoiceTotalList.Add(invoiceTotal5);
                        }
                        var alltaxes = db.PurchaseTaxes.Where(s => s.PurchaseInvoiceId == pomv.Id).ToList();
                        foreach (var individualtax in alltaxes)
                        {
                            if (individualtax.ItemId != null)
                            {
                                var tname = taxes.FirstOrDefault(t => t.TaxId == individualtax.TaxId);
                                var invoiceTotal = new InvoiceTotal { Id = 7, Name = tname.Name + '(' + tname.Rate + "%)" + " On Product", Amount = individualtax.Amount };
                                invoiceTotalList.Add(invoiceTotal);
                            }
                            else
                            {
                                var tname = taxes.FirstOrDefault(t => t.TaxId == individualtax.TaxId);
                                var invoiceTotal = new InvoiceTotal { Id = 10, Name = tname.Name + '(' + tname.Rate + "%)" + " On Others", Amount = individualtax.Amount };
                                invoiceTotalList.Add(invoiceTotal);
                            }
                        }
                    }
                    invoiceTotalList = invoiceTotalList.OrderBy(f => f.Id).ToList();
                    var invoiceTotalList1 = invoiceTotalList.GroupBy(t => t.Name).Select(t => new { Name = t.Key, Total = t.Sum(l => l.Amount) }).ToList();
                    ViewBag.InvoiceTotalList = invoiceTotalList1;

                    return View(pomv);
                }
                else if (from == "PO")
                {
                    PurchaseOrder po = db.PurchaseOrders.Find(id);
                    if (po == null)
                    {
                        return HttpNotFound();
                    }
                    PurchaseInvoiceModelView pomv = new PurchaseInvoiceModelView();
                    List<PurchaseInvoiceDetailModelView> podmvList = new List<PurchaseInvoiceDetailModelView>();

                    pomv.Id = 0;
                    //   pomv.NO = po.NO;
                    pomv.CreatedFrom = "Purchase Order";
                    pomv.ReferenceNo = po.Id;
                    pomv.OrderNo = po.NO;
                    pomv.SupplierCode = po.Supplier.Code;
                    pomv.SupplierName = po.Supplier.Name;
                    pomv.SupplierId = po.SupplierId;
                    pomv.Reference = po.Reference;
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

                    //  pomv.TotalAmount = po.gra;


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

                    var podlist = db.PurchaseOrderDetails.Where(p => p.PurchaseOrderId == po.Id).ToList();
                    foreach (var pod in podlist)
                    {
                        var podmv = new PurchaseInvoiceDetailModelView();
                        // podmv.PurchaseReceiptId = pod.PurchaseReceiptId;
                        podmv.ItemId = pod.ItemId;
                        podmv.ItemName = pod.Product.Code;
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



                        podmv.TotalAmount = pod.Price * pod.Quantity;


                        podmvList.Add(podmv);
                    }
                    ViewBag.podmvList = podmvList;
                    return View(pomv);
                }
                else if (from == "PR")
                {
                    PurchaseReceipt po = db.PurchaseReceipts.Find(id);
                    if (po == null)
                    {
                        return HttpNotFound();
                    }
                    PurchaseInvoiceModelView pomv = new PurchaseInvoiceModelView();
                    List<PurchaseInvoiceDetailModelView> podmvList = new List<PurchaseInvoiceDetailModelView>();

                    pomv.Id = 0;
                    //   pomv.NO = po.NO;
                    pomv.CreatedFrom = "Receipt Note";
                    pomv.ReferenceNo = po.Id;
                    pomv.OrderNo = po.NO;
                    pomv.SupplierCode = po.Supplier.Code;
                    pomv.SupplierName = po.Supplier.Name;
                    pomv.SupplierId = po.SupplierId;
                    pomv.Reference = po.Reference;
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

                    //  pomv.TotalAmount = po.gra;


                    pomv.GrandTotal = po.GrandTotal;
                    pomv.BCGrandTotal = po.BCGrandTotal;
                    pomv.UserId = po.UserId;
                    pomv.BranchId = po.BranchId;
                    pomv.CompanyId = po.CompanyId;
                    //pomv.Status = po.Status;
                    //pomv.CreatedBy = po.CreatedBy;
                    //pomv.CreatedOn = po.CreatedOn;
                    //pomv.ModifiedBy = po.ModifiedBy;
                    //pomv.ModifiedOn = po.ModifiedOn;

                    var podlist = db.PurchaseReceiptDetails.Where(p => p.PurchaseReceiptId == po.Id).ToList();
                    var defaulttax = db.Taxes.FirstOrDefault(t => t.BranchId == 0 && t.CompanyId == 0);
                    foreach (var pod in podlist)
                    {
                        var podmv = new PurchaseInvoiceDetailModelView();
                        // podmv.PurchaseReceiptId = pod.PurchaseReceiptId;
                        podmv.ItemId = pod.ItemId;
                        podmv.ItemName = pod.Product.Code;
                        podmv.Description = pod.Description;
                        podmv.Quantity = pod.Quantity;
                        podmv.AccountId = pod.AccountId;
                        podmv.UnitId = pod.UnitId;
                        podmv.UnitName = pod.UOM.Code;
                        podmv.Price = pod.Price;
                        podmv.CurrencyRate = pod.CurrencyRate;
                        podmv.TaxId = defaulttax.TaxId;
                        podmv.TaxName = defaulttax.Name + '(' + defaulttax.Rate + "%)";
                        podmv.TaxPercent = defaulttax.Rate;
                        podmv.TaxAmount = defaulttax.Rate;



                        podmv.TotalAmount = pod.Price * pod.Quantity;


                        podmvList.Add(podmv);
                    }
                    ViewBag.podmvList = podmvList;
                    return View(pomv);
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }
        //
        // GET: /PurchaseInvoice/Delete/5

        public ActionResult Delete(long id = 0)
        {
            PurchaseInvoice purchaseinvoice = db.PurchaseInvoices.Find(id);
            if (purchaseinvoice == null)
            {
                return HttpNotFound();
            }
            return View(purchaseinvoice);
        }

        //
        // POST: /PurchaseInvoice/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            PurchaseInvoice purchaseinvoice = db.PurchaseInvoices.Find(id);
            var purchaseinvoicedetail = db.PurchaseInvoiceDetails.Where(d => d.PurchaseInvoiceId == purchaseinvoice.Id);
            var purchasecostingdetail = db.PurchaseCostingDetails.Where(d => d.PurchaseInvoiceId == purchaseinvoice.Id);
            var purchaseinvoicetax = db.PurchaseTaxes.Where(d => d.PurchaseInvoiceId == purchaseinvoice.Id);
            
            foreach (var sid in purchaseinvoicedetail)
            {
                db.PurchaseInvoiceDetails.Remove(sid);
            }
            foreach (var sid in purchasecostingdetail)
            {
                db.PurchaseCostingDetails.Remove(sid);
            }
            foreach (var sid in purchaseinvoicetax)
            {
                db.PurchaseTaxes.Remove(sid);
            }
            db.PurchaseInvoices.Remove(purchaseinvoice);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        #region PDF Email



        public ActionResult CreatePurchaseInvoicePDF(long? id, long? Branchid, long? companyid, long? userid)
        {
            //long Branchid = Convert.ToInt64(Session["BranchId"]);
            //long companyid = Convert.ToInt64(Session["companyid"]);
            //long userid = Convert.ToInt32(Session["userid"]);
            var culture = Convert.ToString(Session["DateCulture"]);
            var dateFormat = Convert.ToString(Session["DateFormat"]);
            PurchaseInvoiceModelView pomv = new PurchaseInvoiceModelView();
            List<PurchaseInvoiceDetailModelView> podmvList = new List<PurchaseInvoiceDetailModelView>();
            List<PurchaseCostingDetailModelView> pcdmvList = new List<PurchaseCostingDetailModelView>();
            List<InvoiceTotal> invoiceTotalList = new List<InvoiceTotal>();






            var taxes = db.Taxes.Where(d => (d.CompanyId == companyid && d.BranchId == Branchid) || (d.UserId == 0 && d.CompanyId == 0)).Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate }).ToList();
            ViewBag.TaxSingle = taxes;

            var Taxcomponents = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid && d.Tax.BranchId == Branchid) || (d.Tax.CompanyId == 0 && d.Tax.BranchId == 0)).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            ViewBag.Taxcomponents = Taxcomponents;

            PurchaseInvoice po = db.PurchaseInvoices.Find(id);
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
                var paymentterm = db.PaymentTerms.FirstOrDefault(c => c.Id == po.PaymentTermId).PaymentTermDescription;
                ViewBag.PaymentTerm = paymentterm;
            }
          



           // List<CustomerwiseInvoice> inv = new List<CustomerwiseInvoice>();

            //string cusid = Convert.ToString(id);
            //decimal? recipt = db.ReceiptPayments.Where(d => d.transactionNo == cusid).Sum(d => d.TotalAmount);
            //var result = db.PurchaseInvoices.Where(d => d.SupplierId == id).OrderBy(d => d.Id).ToList();

            var logo = db.BusinessPartners.Where(r => r.CompanyId == companyid).Select(s => s.Logo).ToList();
            ViewBag.Logo = logo;


            var podlist = db.PurchaseInvoiceDetails.Where(p => p.PurchaseInvoiceId == po.Id).ToList();
            foreach (var pod in podlist)
            {
                var podmv = new PurchaseInvoiceDetailModelView();
                podmv.PurchaseInvoiceId = pod.PurchaseInvoiceId;
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

                podmv.TotalAmount = pod.Price * pod.Quantity ;

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


            var pcdlist = db.PurchaseCostingDetails.Where(p => p.PurchaseInvoiceId == po.Id).ToList();
            foreach (var pcd in pcdlist)
            {
                var pcdmv = new PurchaseCostingDetailModelView();
                pcdmv.PurchaseInvoiceId = pcd.PurchaseInvoiceId;
                pcdmv.CostingId = pcd.CostingId;
                pcdmv.CostName = pcd.Costing.Name;
                pcdmv.Description = pcd.Description;
                pcdmv.CostingType = pcd.CostingType;
                pcdmv.CurrencyRate = pcd.CurrencyRate;
                pcdmv.TaxId = pcd.TaxId;
                pcdmv.TaxName = pcd.Tax.Name + '(' + pcd.Tax.Rate + "%)";
                pcdmv.TaxAmount = pcd.TaxAmount;
                pcdmv.CostAmount = pcd.CostAmount;
                pcdmvList.Add(pcdmv);
            }
            ViewBag.pcdmvList = pcdmvList;



            if (po.Status == "Saved")
            {
                if (po.SubTotal > 0)
                {
                    var invoiceTotal = new InvoiceTotal { Id = 1, Name = "Sub Total", Amount = po.SubTotal };
                    invoiceTotalList.Add(invoiceTotal);
                }
                if (po.TotalAddAmount > 0)
                {
                    var invoiceTotal1 = new InvoiceTotal { Id = 8, Name = "Added Cost", Amount = po.TotalAddAmount };
                    invoiceTotalList.Add(invoiceTotal1);
                }
                if (po.TotalDeductAmount > 0)
                {
                    var invoiceTotal2 = new InvoiceTotal { Id = 9, Name = "Deducted Cost", Amount = -po.TotalDeductAmount };
                    invoiceTotalList.Add(invoiceTotal2);
                }
                if (pomv.RoundOff > 0)
                {
                    var invoiceTotal = new InvoiceTotal { Id = 6, Name = "RoundOff", Amount = po.RoundOff };
                    invoiceTotalList.Add(invoiceTotal);
                }
                var invoiceTotal4 = new InvoiceTotal { Id = 12, Name = "Grand Total In " + po.Currency1.ISO_4217, Amount = po.GrandTotal };
                invoiceTotalList.Add(invoiceTotal4);
                if (pomv.CurrencyId != pomv.TransactionCurrency)
                {

                    var invoiceTotal5 = new InvoiceTotal { Id = 13, Name = "Grand Total In " + po.Currency.ISO_4217, Amount = po.BCGrandTotal };
                    invoiceTotalList.Add(invoiceTotal5);
                }
                var alltaxes = db.SalesTaxes.Where(s => s.SalesInvoiceId == po.Id).ToList();
                foreach (var individualtax in alltaxes)
                {
                    if (individualtax.ItemId != null)
                    {
                        var tname = taxes.FirstOrDefault(t => t.TaxId == individualtax.TaxId);
                        var invoiceTotal = new InvoiceTotal { Id = 7, Name = tname.Name + '(' + tname.Rate + "%)" + " On Product", Amount = individualtax.Amount };
                        invoiceTotalList.Add(invoiceTotal);
                    }
                    else
                    {
                        var tname = taxes.FirstOrDefault(t => t.TaxId == individualtax.TaxId);
                        var invoiceTotal = new InvoiceTotal { Id = 10, Name = tname.Name + '(' + tname.Rate + "%)" + " On Others", Amount = individualtax.Amount };
                        invoiceTotalList.Add(invoiceTotal);
                    }
                }
            }
            invoiceTotalList = invoiceTotalList.OrderBy(f => f.Id).ToList();
            var invoiceTotalList1 = invoiceTotalList.GroupBy(t => t.Name).Select(t => new InvoiceTotal { Name = t.Key, Amount = t.Sum(l => l.Amount) }).ToList();
            ViewBag.InvoiceTotalList = invoiceTotalList1;

            string receipts = "";
            string pos = "";
            if (pomv.ReceiptIds != "")
            {
                string[] value = po.ReceiptIds.Split(',');
                int[] myInts = Array.ConvertAll(value, int.Parse);
                var countpr = myInts.Length;

                for (int i = 0; i < countpr; i++)
                {
                    var prid = myInts[i];
                    var purchaseReceipt = db.PurchaseReceipts.Where(p=>p.Id==prid).Select(p=>new{p.ReferenceChallan,p.ReferenceNo}).FirstOrDefault();
                    if (purchaseReceipt != null)
                    {
                      
                            receipts += purchaseReceipt.ReferenceChallan + " ";
                       
                        var purchaseorder = db.PurchaseOrders.Where(p => p.Id == purchaseReceipt.ReferenceNo).Select(p=>p.NO).FirstOrDefault();
                        if (purchaseorder != null)
                            pos += purchaseorder + " ";
                    }


                }
            }
            ViewBag.receipts = receipts;
            ViewBag.pos = pos;
            return View(po);


        }



        public ActionResult PrintPurchaseInvoicePDF(long? id)
        {
            //return RedirectToAction("CreteSalesInvoicePDF", new { id = id });
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);

            return new ActionAsPdf("CreatePurchaseInvoicePDF", new { id = id, Branchid = Branchid, companyid = companyid, userid = userid }) { FileName = "PurchaseInvoicePrint.pdf" };
        }





        #endregion

        public ActionResult GetYetToBeInvoicedPR(long id)
        {
            var getPoitems = db.PurchaseReceipts.Where(po =>(po.SupplierId==id) && (po.Status == InventoryConst.cns_Saved || po.Status == InventoryConst.cns_Partially_Received))
                           .Select(po => new  { po.Id, po.NO, po.OrderNo }).ToList();
            List<POGeneral> getItems = new List<POGeneral>();
            foreach(var item in getPoitems)
            {
                var general = new POGeneral();
                general.Id = item.Id;
                general.Name = item.NO + "(" + item.OrderNo + ")";
                getItems.Add(general);
            }
            return Json(getItems, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult GetYetToBeInvoicedPR()
        //{
        //    var getPoitems = db.PurchaseReceipts.Where(po =>  (po.Status == InventoryConst.cns_Saved || po.Status == InventoryConst.cns_Partially_Received))
        //                   .Select(po => new { po.Id, po.NO, po.OrderNo }).ToList();
        //    List<POGeneral> getItems = new List<POGeneral>();
        //    foreach (var item in getPoitems)
        //    {
        //        var general = new POGeneral();
        //        general.Id = item.Id;
        //        general.Name = item.NO + "(" + item.OrderNo + ")";
        //        getItems.Add(general);
        //    }
        //    return Json(getItems, JsonRequestBehavior.AllowGet);
        //}
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
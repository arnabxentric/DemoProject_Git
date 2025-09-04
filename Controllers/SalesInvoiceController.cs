using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Globalization;
using System.Transactions;
using System;
using Rotativa;
using System.Text.RegularExpressions;


namespace XenERP.Controllers
{
    public class SalesInvoiceController : Controller
    {
        private InventoryEntities db = new InventoryEntities();
        private MasterClasses mc = new MasterClasses();
        private TransactionClasses tc = new TransactionClasses();
        //
        // GET: /SalesInvoice/
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
                return View(db.SalesInvoices.Where(p => p.CompanyId == companyid && p.FinancialYearId == Fyid).OrderByDescending(d => d.InvoiceDate).ThenByDescending(d => d.InvoiceNo).Take(100).OrderBy(d => d.InvoiceDate).ThenBy(d => d.InvoiceNo).ToList());
            else
                return View(db.SalesInvoices.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.FinancialYearId == Fyid).OrderByDescending(d => d.InvoiceDate).ThenByDescending(d => d.InvoiceNo).Take(100).OrderBy(d => d.InvoiceDate).ThenBy(d => d.InvoiceNo).ToList());
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
                return View(db.SalesInvoices.Where(p => p.CompanyId == companyid && p.FinancialYearId == Fyid && p.InvoiceDate >= DtFrm && p.InvoiceDate <= DtTo).OrderBy(d => d.InvoiceDate).ThenBy(d => d.InvoiceNo).ToList());
            else
                return View(db.SalesInvoices.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.FinancialYearId == Fyid && p.InvoiceDate >= DtFrm && p.InvoiceDate <= DtTo).OrderBy(d => d.InvoiceDate).ThenBy(d => d.InvoiceNo).ToList());
        }
        //
        // GET: /SalesInvoice/Details/5

        public ActionResult Details(long id = 0)
        {
            SalesInvoice salesinvoice = db.SalesInvoices.Find(id);
            if (salesinvoice == null)
            {
                return HttpNotFound();
            }
            return View(salesinvoice);
        }

        //
        // GET: /SalesInvoice/Create

        //public ActionResult Create()
        //{
        //    ViewBag.CurrencyId = new SelectList(db.Currencies, "CurrencyId", "Country");
        //    ViewBag.TransactionCurrency = new SelectList(db.Currencies, "CurrencyId", "Country");
        //    ViewBag.CustomerId = new SelectList(db.Customers, "Id", "Code");
        //    ViewBag.TaxId = new SelectList(db.Taxes, "TaxId", "Name");
        //    ViewBag.WarehouseId = new SelectList(db.Warehouses, "Id", "Code");
        //    return View();
        //}

        //
        // POST: /SalesInvoice/Create

        //[HttpPost]
        //public ActionResult Create(SalesInvoice salesinvoice)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.SalesInvoices.Add(salesinvoice);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.CurrencyId = new SelectList(db.Currencies, "CurrencyId", "Country", salesinvoice.CurrencyId);
        //    ViewBag.TransactionCurrency = new SelectList(db.Currencies, "CurrencyId", "Country", salesinvoice.TransactionCurrency);
        //    ViewBag.CustomerId = new SelectList(db.Customers, "Id", "Code", salesinvoice.CustomerId);
        //    ViewBag.TaxId = new SelectList(db.Taxes, "TaxId", "Name", salesinvoice.TaxId);
        //    ViewBag.WarehouseId = new SelectList(db.Warehouses, "Id", "Code", salesinvoice.WarehouseId);
        //    return View(salesinvoice);
        //}
        [SessionExpire]
        public ActionResult Create(long? id = 0, string from = null)
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            var culture = Session["DateCulture"].ToString();
            var dateFormat = Session["DateFormat"].ToString();
            // System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            ViewBag.ddlWarehouses = warehouses;
            var taxes = db.Taxes.Where(d => (d.CompanyId == companyid)).Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate }).ToList();
            ViewBag.TaxSingle = taxes;
            var salesPerson = db.SalesPersons.Where(d => (d.UserId == userid && d.CompanyId == companyid) || (d.CompanyId == companyid && d.BranchId == 0)).Select(d => new { Id = d.Id, Name = d.Name }).ToList();
            ViewBag.ddlSalesPerson = salesPerson;
            var paymentTerms = db.PaymentTerms.Where(d => (d.CompanyId == companyid)).Select(d => new { Id = d.Id, PaymentTermDescription = d.PaymentTermDescription }).ToList();
            ViewBag.PaymentTerms = paymentTerms;
            var ShedList = db.ShedMasters.Where(d => (d.BranchId == Branchid)).Select(d => new { Id = d.Id, ShedNo = d.ShedNo }).ToList();
            ViewBag.ShedList = ShedList;
            var Taxcomponents = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid)).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            ViewBag.Taxcomponents = Taxcomponents;
            ViewBag.Ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.groupID == 113).Select(d => new { d.LID, d.ledgerName }).ToList();
            ViewBag.unit = db.UOMs.Where(d => (d.UserId == userid && d.CompanyId == companyid)).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => (d.UserId == userid && d.CompanyId == companyid)).ToList();
            var mode = new SelectList(new[]
                                         {

                                              new{ID=2,Name="Cash"},
                                              new{ID=1,Name="Credit"},


                                          },
                      "ID", "Name");
            ViewData["mod"] = mode;

            if (id == 0)
            {
                SalesInvoiceModelView pomv = new SalesInvoiceModelView();
                pomv.Date = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.DueDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.DespatchDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.InvoiceDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.CurrencyId = basecurrency.CurrencyId;
                pomv.BaseCurrencyCode = basecurrency.CurrencyCode;
                pomv.TransactionCurrency = basecurrency.CurrencyId;
                pomv.TransactionCurrencyCode = basecurrency.CurrencyCode;
                pomv.Currencyrate = 1;
                pomv.CompoundedDis = false;
                string countryname = string.Empty;
                return View(pomv);
            }
            else
            {
                if (from == null)
                {
                    SalesInvoice po = db.SalesInvoices.Find(id);
                    if (po == null)
                    {
                        return HttpNotFound();
                    }
                    SalesInvoiceModelView pomv = new SalesInvoiceModelView();
                    List<SalesInvoiceDetailModelView> podmvList = new List<SalesInvoiceDetailModelView>();
                    List<SalesCostingDetailModelView> pcdmvList = new List<SalesCostingDetailModelView>();
                    List<InvoiceTotal> invoiceTotalList = new List<InvoiceTotal>();
                    pomv.Id = po.Id;
                    pomv.NO = po.NO;
                    pomv.LID = po.LID;
                    pomv.OrderNo = po.OrderNo;
                    pomv.CreatedFrom = po.CreatedFrom;
                    if (po.Customer != null)
                    {
                        pomv.CustomerCode = po.Customer.Code;
                        pomv.CustomerName = po.Customer.Name;
                        pomv.CustomerId = po.CustomerId;
                    }
                    pomv.Reference = po.Reference;
                    pomv.SalesPerson = po.SalesPerson;
                    pomv.PaymentTermId = po.PaymentTermId;
                    pomv.RecurringSalesId = po.RecurringSalesId;
                    pomv.WarehouseId = po.WarehouseId;
                    pomv.CurrencyId = po.CurrencyId;
                    pomv.BaseCurrencyCode = po.Currency.ISO_4217;
                    pomv.Currencyrate = po.Currencyrate;
                    pomv.TransactionCurrency = po.TransactionCurrency;
                    pomv.TransactionCurrencyCode = po.Currency1.ISO_4217;
                    pomv.Date = po.Date.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DueDate = po.DueDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DespatchDate = po.DespatchDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.InvoiceDate = po.InvoiceDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DeliveryName = po.DeliveryName;
                    pomv.StreetPoBox = po.StreetPoBox;
                    pomv.Suburb = po.Suburb;
                    pomv.City = po.City;
                    pomv.StateRegion = po.StateRegion;
                    pomv.Country = po.Country;
                    pomv.PostalCode = po.PostalCode;
                    pomv.SalesPerson = po.SalesPerson;
                    pomv.ShedId = po.ShedId;
                    pomv.CompoundedDis = po.CompoundedDis;
                    pomv.DisApplicable = po.DisApplicable;
                    pomv.FD1Applicable = po.FD1Applicable;
                    pomv.FD2Applicable = po.FD2Applicable;
                    pomv.FD3Applicable = po.FD3Applicable;
                    pomv.FD4Applicable = po.FD4Applicable;
                    pomv.DisAmount = po.DisAmount;
                    pomv.FD1Amount = po.FD1Amount;
                    pomv.FD2Amount = po.FD2Amount;
                    pomv.FD3Amount = po.FD3Amount;
                    pomv.FD4Amount = po.FD4Amount;
                    pomv.Dis = po.Dis;
                    pomv.FD1 = po.FD1;
                    pomv.FD2 = po.FD2;
                    pomv.FD3 = po.FD3;
                    pomv.FD4 = po.FD4;
                    pomv.SubTotal = po.SubTotal;
                    pomv.TaxProduct = po.TaxProduct;
                    pomv.TotalAddAmount = po.TotalAddAmount;
                    pomv.TotalDeductAmount = po.TotalDeductAmount;
                    pomv.DiscountPerUnit = po.DiscountPerUnit;
                    pomv.TaxOther = po.TaxOther;
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
                    pomv.Mode = po.Mode;
                    pomv.TransNo = po.TransNo;
                    var podlist = db.SalesInvoiceDetails.Where(p => p.SalesInvoiceId == po.Id).ToList();
                    foreach (var pod in podlist)
                    {
                        var podmv = new SalesInvoiceDetailModelView();
                        podmv.SalesInvoiceId = pod.SalesInvoiceId;
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
                        podmv.TotalAmount = pod.Price * pod.Quantity - pod.DiscountAmount * pod.Quantity;

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
                    var pcdlist = db.SalesCostingDetails.Where(p => p.SalesInvoiceId == po.Id).ToList();
                    foreach (var pcd in pcdlist)
                    {
                        var pcdmv = new SalesCostingDetailModelView();
                        pcdmv.SalesInvoiceId = pcd.SalesInvoiceId;
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
                    if (pomv.Status == "Saved")
                    {
                        if (pomv.SubTotal > 0)
                        {
                            var invoiceTotal = new InvoiceTotal { Id = 1, Name = "Sub Total", Amount = pomv.SubTotal };
                            invoiceTotalList.Add(invoiceTotal);
                        }
                        if (pomv.Dis > 0)
                        {
                            var invoiceTotal = new InvoiceTotal { Id = 2, Name = "Discount", Amount = (decimal)-pomv.Dis };
                            invoiceTotalList.Add(invoiceTotal);
                        }
                        if (pomv.FD1 > 0)
                        {
                            var invoiceTotal = new InvoiceTotal { Id = 3, Name = "Further Discount 1", Amount = (decimal)-pomv.FD1 };
                            invoiceTotalList.Add(invoiceTotal);
                        }
                        if (pomv.FD2 > 0)
                        {
                            var invoiceTotal = new InvoiceTotal { Id = 4, Name = "Further Discount 2", Amount = (decimal)-pomv.FD2 };
                            invoiceTotalList.Add(invoiceTotal);
                        }
                        if (pomv.FD3 > 0)
                        {
                            var invoiceTotal = new InvoiceTotal { Id = 5, Name = "Further Discount 3", Amount = (decimal)-pomv.FD3 };
                            invoiceTotalList.Add(invoiceTotal);
                        }
                        if (pomv.FD4 > 0)
                        {
                            var invoiceTotal = new InvoiceTotal { Id = 6, Name = "Further Discount 4", Amount = (decimal)-pomv.FD4 };
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
                        var alltaxes = db.SalesTaxes.Where(s => s.SalesInvoiceId == pomv.Id).ToList();
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
                else if (from == "SO")
                {
                    SalesOrder po = db.SalesOrders.Find(id);
                    if (po == null)
                    {
                        return HttpNotFound();
                    }
                    SalesInvoiceModelView pomv = new SalesInvoiceModelView();
                    List<SalesInvoiceDetailModelView> podmvList = new List<SalesInvoiceDetailModelView>();

                    pomv.Id = 0;
                    pomv.CreatedFrom = "Sales Order";
                    pomv.ReferenceNo = po.Id;
                    pomv.OrderNo = po.NO;
                    pomv.SalesOrderNo = po.NO;
                    if (po.Customer != null)
                    {
                        pomv.CustomerCode = po.Customer.Code;
                        pomv.CustomerName = po.Customer.Name;
                        pomv.CustomerId = po.CustomerId;
                    }
                    pomv.Reference = po.Reference;
                    pomv.SalesPerson = po.SalesPerson;
                    pomv.PaymentTermId = po.PaymentTermId;
                    pomv.RecurringSalesId = po.RecurringSalesId;
                    pomv.WarehouseId = po.WarehouseId;
                    pomv.CurrencyId = po.CurrencyId;
                    pomv.BaseCurrencyCode = po.Currency.ISO_4217;
                    pomv.Currencyrate = po.Currencyrate;
                    pomv.TransactionCurrency = po.TransactionCurrency;
                    pomv.TransactionCurrencyCode = po.Currency1.ISO_4217;
                    pomv.Date = po.Date.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DueDate = po.DueDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DespatchDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.InvoiceDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DeliveryName = po.DeliveryName;
                    pomv.StreetPoBox = po.StreetPoBox;
                    pomv.Suburb = po.Suburb;
                    pomv.City = po.City;
                    pomv.StateRegion = po.StateRegion;
                    pomv.Country = po.Country;
                    pomv.PostalCode = po.PostalCode;
                    pomv.SalesPerson = po.SalesPerson;
                    pomv.CompoundedDis = false;
                    pomv.DisApplicable = false;
                    pomv.FD1Applicable = false;
                    pomv.FD2Applicable = false;
                    pomv.FD3Applicable = false;
                    pomv.FD4Applicable = false;
                    pomv.DisAmount = 0;
                    pomv.FD1Amount = 0;
                    pomv.FD2Amount = 0;
                    pomv.FD3Amount = 0;
                    pomv.FD4Amount = 0;
                    pomv.Dis = 0;
                    pomv.FD1 = 0;
                    pomv.FD2 = 0;
                    pomv.FD3 = 0;
                    pomv.FD4 = 0;
                    pomv.SubTotal = 0;
                    pomv.TaxProduct = 0;
                    pomv.TotalAddAmount = 0;
                    pomv.TotalDeductAmount = 0;

                    pomv.TaxOther = 0;
                    pomv.GrandTotal = 0;
                    pomv.BCGrandTotal = 0;
                    pomv.UserId = po.UserId;
                    pomv.BranchId = po.BranchId;
                    pomv.CompanyId = po.CompanyId;
                    //pomv.Status = po.Status;
                    //pomv.CreatedBy = po.CreatedBy;
                    //pomv.CreatedOn = po.CreatedOn;
                    //pomv.ModifiedBy = po.ModifiedBy;
                    //pomv.ModifiedOn = po.ModifiedOn;
                    pomv.Memo = po.Memo;
                    var podlist = db.SalesOrderDetails.Where(p => p.SalesOrderId == po.Id).ToList();
                    foreach (var pod in podlist)
                    {
                        var podmv = new SalesInvoiceDetailModelView();
                        podmv.SalesInvoiceId = pod.SalesOrderId;
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
                        podmv.TotalAmount = pod.Price * pod.Quantity - pod.DiscountAmount * pod.Quantity;
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
                    ViewBag.IgnoreZeroPrice = "1";
                    ViewBag.podmvList = podmvList;
                    return View(pomv);
                }
                else
                {
                    SalesDelivery po = db.SalesDeliveries.Find(id);
                    if (po == null)
                    {
                        return HttpNotFound();
                    }
                    SalesInvoiceModelView pomv = new SalesInvoiceModelView();
                    List<SalesInvoiceDetailModelView> podmvList = new List<SalesInvoiceDetailModelView>();

                    pomv.Id = 0;
                    pomv.CreatedFrom = "Delivery Note";
                    pomv.ReferenceNo = po.Id;
                    pomv.OrderNo = po.NO;
                    pomv.SalesOrderNo = po.OrderNo;
                    if (po.Customer != null)
                    {
                        pomv.CustomerCode = po.Customer.Code;
                        pomv.CustomerName = po.Customer.Name;
                        pomv.CustomerId = po.CustomerId;
                    }
                    pomv.Reference = po.Reference;
                    pomv.SalesPerson = po.SalesPerson;
                    pomv.PaymentTermId = po.PaymentTermId;
                    pomv.RecurringSalesId = po.RecurringSalesId;
                    pomv.WarehouseId = po.WarehouseId;
                    pomv.CurrencyId = po.CurrencyId;
                    pomv.BaseCurrencyCode = po.Currency.ISO_4217;
                    pomv.Currencyrate = po.Currencyrate;
                    pomv.TransactionCurrency = po.TransactionCurrency;
                    pomv.TransactionCurrencyCode = po.Currency1.ISO_4217;
                    pomv.Date = po.Date.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DueDate = po.DueDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DespatchDate = po.ReceiptDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.InvoiceDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DeliveryName = po.DeliveryName;
                    pomv.StreetPoBox = po.StreetPoBox;
                    pomv.Suburb = po.Suburb;
                    pomv.City = po.City;
                    pomv.StateRegion = po.StateRegion;
                    pomv.Country = po.Country;
                    pomv.PostalCode = po.PostalCode;
                    pomv.SalesPerson = po.SalesPerson;
                    pomv.CompoundedDis = false;
                    pomv.DisApplicable = false;
                    pomv.FD1Applicable = false;
                    pomv.FD2Applicable = false;
                    pomv.FD3Applicable = false;
                    pomv.FD4Applicable = false;
                    pomv.DisAmount = 0;
                    pomv.FD1Amount = 0;
                    pomv.FD2Amount = 0;
                    pomv.FD3Amount = 0;
                    pomv.FD4Amount = 0;
                    pomv.Dis = 0;
                    pomv.FD1 = 0;
                    pomv.FD2 = 0;
                    pomv.FD3 = 0;
                    pomv.FD4 = 0;
                    pomv.SubTotal = 0;
                    pomv.TaxProduct = 0;
                    pomv.TotalAddAmount = 0;
                    pomv.TotalDeductAmount = 0;

                    pomv.TaxOther = 0;
                    pomv.GrandTotal = 0;
                    pomv.BCGrandTotal = 0;
                    pomv.UserId = po.UserId;
                    pomv.BranchId = po.BranchId;
                    pomv.CompanyId = po.CompanyId;
                    //pomv.Status = po.Status;
                    //pomv.CreatedBy = po.CreatedBy;
                    //pomv.CreatedOn = po.CreatedOn;
                    //pomv.ModifiedBy = po.ModifiedBy;
                    //pomv.ModifiedOn = po.ModifiedOn;
                    pomv.Memo = po.Memo;
                    var podlist = db.SalesDeliveryDetails.Where(p => p.SalesDeliveryId == po.Id).ToList();
                    var defaulttax = db.Taxes.FirstOrDefault(t => t.BranchId == 0 && t.CompanyId == 1);
                    foreach (var pod in podlist)
                    {
                        var podmv = new SalesInvoiceDetailModelView();
                        podmv.SalesInvoiceId = pod.SalesDeliveryId;
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
                        podmv.TaxId = defaulttax.TaxId;
                        podmv.TaxName = defaulttax.Name + '(' + defaulttax.Rate + "%)";
                        podmv.TaxPercent = defaulttax.Rate;
                        podmv.TaxAmount = defaulttax.Rate;
                        podmv.Discount = pod.Discount;
                        podmv.DiscountAmount = pod.DiscountAmount;
                        podmv.TotalAmount = pod.Price * pod.Quantity - pod.DiscountAmount * pod.Quantity;
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
                    ViewBag.IgnoreZeroPrice = "1";
                    ViewBag.podmvList = podmvList;
                    return View(pomv);
                }
            }

        }
        [SessionExpire]
        [HttpPost]
        public ActionResult Create(FormCollection poCollection)
        {

            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt32(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            long Createdby = Convert.ToInt32(Session["Createdid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            //System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            string currencyval = string.Empty;
            var salesPerson = db.SalesPersons.Where(d => (d.UserId == userid && d.CompanyId == companyid)).Select(d => new { Id = d.Id, Name = d.Name }).ToList();
            ViewBag.ddlSalesPerson = salesPerson;
            var paymentTerms = db.PaymentTerms.Where(d => (d.CompanyId == companyid)).Select(d => new { Id = d.Id, PaymentTermDescription = d.PaymentTermDescription }).ToList();
            ViewBag.PaymentTerms = paymentTerms;
            var ShedList = db.ShedMasters.Where(d => (d.BranchId == Branchid)).Select(d => new { Id = d.Id, ShedNo = d.ShedNo }).ToList();
            ViewBag.ShedList = ShedList;
            ViewBag.unit = db.UOMs.Where(d => (d.UserId == userid && d.CompanyId == companyid)).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => (d.UserId == userid && d.CompanyId == companyid)).ToList();
            var taxes = db.Taxes.Where(d => (d.CompanyId == companyid)).Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate, NetEffective = d.NetEffective }).ToList();
            ViewBag.TaxSingle = taxes;
            var taxCompsList = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid)).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            ViewBag.Taxcomponents = taxCompsList;
            ViewBag.Ledger = db.LedgerMasters.Where(d => d.CompanyId == companyid && d.groupID == 113).Select(d => new { d.LID, d.ledgerName }).ToList();

            var mode = new SelectList(new[]
                                         {
                                              new{ID=2,Name="Cash"},
                                              new{ID=1,Name="Credit"},


                                          },
                     "ID", "Name");
            ViewData["mod"] = mode;

            SalesInvoiceModelView pomv = new SalesInvoiceModelView();
            List<SalesInvoiceDetailModelView> podmvList = new List<SalesInvoiceDetailModelView>();
            List<SalesCostingDetailModelView> pcdmvList = new List<SalesCostingDetailModelView>();
            var sid = poCollection["CustomerId"];
            var rno = poCollection["ReferenceNo"];
            var pti = poCollection["PaymentTermId"];
            var rsi = poCollection["RecurringSalesId"];
            var dpu = poCollection["DiscountPerUnit"];
            var mod = poCollection["Mode"];
            var sm = poCollection["ShedId"];
            if (poCollection["InvoiceNo"] != "")
                pomv.InvoiceNo = Convert.ToInt64(poCollection["InvoiceNo"]);
            if (pti != "")
                pomv.PaymentTermId = Convert.ToInt32(pti);
            if (rsi != "")
                pomv.RecurringSalesId = Convert.ToInt32(rsi);
            var wid = poCollection["WarehouseId"];
            pomv.WarehouseId = Convert.ToInt64(wid);
            if (sid != "")
                pomv.CustomerId = Convert.ToInt32(sid);
            if (mod != "")
                pomv.Mode = Convert.ToInt32(mod);

            if ((poCollection["CompoundedDis"]).ToLower() == "false")
                pomv.CompoundedDis = false;
            else
                pomv.CompoundedDis = true;

            if ((poCollection["DisApplicable"]).ToLower() == "false")
            {
                pomv.DisApplicable = false;
                pomv.DisAmount = 0;
            }
            else
            {
                pomv.DisApplicable = true;
                if (poCollection["DisAmount"] != "")
                    pomv.DisAmount = Convert.ToDecimal(poCollection["DisAmount"]);
                else
                    pomv.DisAmount = 0;
            }
            if ((poCollection["FD1Applicable"]).ToLower() == "false")
            {
                pomv.FD1Applicable = false;
                pomv.FD1Amount = 0;
            }
            else
            {
                pomv.FD1Applicable = true;
                if (poCollection["FD1Amount"] != "")
                    pomv.FD1Amount = Convert.ToDecimal(poCollection["FD1Amount"]);
                else
                    pomv.FD1Amount = 0;
            }

            if ((poCollection["FD2Applicable"]).ToLower() == "false")
            {
                pomv.FD2Applicable = false;
                pomv.FD2Amount = 0;
            }
            else
            {
                pomv.FD2Applicable = true;
                if (poCollection["FD2Amount"] != "")
                    pomv.FD2Amount = Convert.ToDecimal(poCollection["FD2Amount"]);
                else
                    pomv.FD2Amount = 0;
            }
            if ((poCollection["FD3Applicable"]).ToLower() == "false")
            {
                pomv.FD3Applicable = false;
                pomv.FD3Amount = 0;
            }
            else
            {
                pomv.FD3Applicable = true;
                if (poCollection["FD3Amount"] != "")
                    pomv.FD3Amount = Convert.ToDecimal(poCollection["FD3Amount"]);
                else
                    pomv.FD3Amount = 0;
            }
            if ((poCollection["FD4Applicable"]).ToLower() == "false")
            {
                pomv.FD4Applicable = false;
                pomv.FD4Amount = 0;
            }
            else
            {
                pomv.FD4Applicable = true;
                if (poCollection["FD4Amount"] != "")
                    pomv.FD4Amount = Convert.ToDecimal(poCollection["FD4Amount"]);
                else
                    pomv.FD4Amount = 0;
            }


            if (rno != "")
                pomv.ReferenceNo = Convert.ToInt64(poCollection["ReferenceNo"]);
            if (dpu != "")
                pomv.DiscountPerUnit = Convert.ToDecimal(poCollection["DiscountPerUnit"]);
            else
                pomv.DiscountPerUnit = 0;
            pomv.CreatedFrom = poCollection["CreatedFrom"];
            pomv.NO = poCollection["NO"];
            pomv.Id = Convert.ToInt64(poCollection["Id"]);
            if (pomv.ReferenceNo != null && pomv.NO == "")
                pomv.Id = 0;
            pomv.OrderNo = poCollection["OrderNo"];
            pomv.SalesOrderNo = poCollection["SalesOrderNo"];
            pomv.NO = poCollection["NO"];
            pomv.CustomerName = poCollection["CustomerName"];
            pomv.CustomerCode = poCollection["CustomerCode"];
            pomv.Reference = poCollection["Reference"];
            //pomv.Date = Convert.ToString(poCollection["Date"]);
            //pomv.DueDate = Convert.ToString(poCollection["DueDate"]);
            //pomv.DespatchDate = Convert.ToString(poCollection["DespatchDate"]);
            pomv.InvoiceDate = Convert.ToString(poCollection["InvoiceDate"]);
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
            pomv.Memo = poCollection["Memo"];
            pomv.DespatchNo = poCollection["DespatchNo"];
            pomv.DespatchThrough = poCollection["DespatchThrough"];
            pomv.DespatchDestination = poCollection["DespatchDestination"];
            List<InvoiceTotal> invoiceTotalList = new List<InvoiceTotal>();
            pomv.Status = poCollection["Status"];
            pomv.LID = Convert.ToInt32(poCollection["LID"]);
            if (sm != "")
                pomv.ShedId = Convert.ToInt32(poCollection["ShedId"]);

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
                SalesInvoiceDetailModelView podmv = new SalesInvoiceDetailModelView();
                podmvList.Add(podmv);
            }
            if (poCollection["type"] != null)
            {
                var countcost = poCollection["type"].Split(',').Length;
                for (int i = 0; i < countcost; i++)
                {
                    SalesCostingDetailModelView pcdmv = new SalesCostingDetailModelView();
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
            var checkCustomer = db.Customers.Where(s => s.Id == pomv.CustomerId && s.CompanyId == companyid).FirstOrDefault();
            //var date = DateTime.ParseExact(pomv.Date, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            //var duedate = DateTime.ParseExact(pomv.DueDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            //var despatchdate = DateTime.ParseExact(pomv.DespatchDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var invoicedate = DateTime.ParseExact(pomv.InvoiceDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            decimal quantityrequested = 0;

            var getDateRange = db.FinancialYearMasters.Where(d => d.fYearID == Fyid).FirstOrDefault();

            if (!(invoicedate >= getDateRange.sDate && invoicedate <= getDateRange.eDate))
            {
                var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                ViewBag.ddlWarehouses = warehouses;
                ViewBag.podmvList = podmvList;
                ViewBag.Date = "Invoice Date out of scope of " + getDateRange.Year + " Financial Year.";
                return View(pomv);
            }

            if (checkCustomer == null)
            {
                // var warehouses = mc.getDdlWarehouses( companyid, Branchid);
                // ViewBag.ddlWarehouses = warehouses;
                //// var taxesddl = mc.getDdlTaxes(userid, companyid, Branchid);
                // ViewBag.ddlTaxes = taxes;
                // ViewBag.podmvList = podmvList;
                // ViewBag.Supply = "Customer does not exist!";
                // //if (duedate < date)
                // //    ViewBag.Date = "Due Date can not be less than Order Date ";
                // return View(pomv);
                pomv.CustomerId = null;

            }
            //else if (duedate < date)
            //{
            //    var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            //    ViewBag.ddlWarehouses = warehouses;
            //    //  var taxes = mc.getDdlTaxes(userid, companyid, Branchid);
            //    ViewBag.ddlTaxes = taxes;
            //    ViewBag.podmvList = podmvList;
            //    ViewBag.Date = "Due Date can not be less than Order Date ";
            //    return View(pomv);

            //}
            else
            {
                pomv.CustomerId = checkCustomer.Id;
                pomv.TransactionCurrency = checkCustomer.CurrencyRate.CurrencyId;
                pomv.Currencyrate = checkCustomer.CurrencyRate.SellRate;

                //Sales invoice details model view validation

                foreach (var podmv in podmvList)
                {

                    if (podmv.ItemId != 0 && podmv.Quantity > 0 && podmv.TaxId > 0)
                    {
                        quantityrequested += podmv.Quantity;
                        //do nothing


                    }
                    else
                    {
                        if (!(podmv.ItemId == 0 && podmv.Quantity == 0 && podmv.TaxId == 0))
                        {
                            var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                            ViewBag.ddlWarehouses = warehouses;
                            // var taxes = mc.getDdlTaxes(userid, companyid, Branchid);
                            ViewBag.ddlTaxes = taxes;
                            ViewBag.podmvList = podmvList;
                            ViewBag.pcdmvList = pcdmvList;
                            return View(pomv);
                        }


                    }
                }
                // pomv.SubTotal=totala
                //Sales costing details model view validation
                foreach (var pcdmv in pcdmvList)
                {
                    if (!(pcdmv.CostingId != 0 && pcdmv.CostAmount != 0 && pcdmv.TaxId != 0))
                    {
                        var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                        ViewBag.ddlWarehouses = warehouses;
                        // var taxes = mc.getDdlTaxes(userid, companyid, Branchid);
                        ViewBag.ddlTaxes = taxes;

                        ViewBag.podmvList = podmvList;
                        ViewBag.pcdmvList = pcdmvList;
                        return View(pomv);
                    }
                    //else
                    //{


                    //}


                }
                // pomv.subT
            }

            //if (pomv.ReferenceNo != null)
            //{
            //    if (pomv.CreatedFrom == "Sales Order")
            //    {
            //        var quantityreceiveded = db.SalesDeliveryDetails.Where(p => p.SalesDelivery.CreatedFrom == "Sales Order" && p.SalesDelivery.ReferenceNo == pomv.ReferenceNo).Sum(p => (decimal?)p.Quantity) ?? 0;
            //        var quantityordered = db.SalesOrderDetails.Where(p => p.SalesOrder.Id == pomv.ReferenceNo).Sum(p => (decimal?)p.Quantity) ?? 0;
            //        if (quantityordered < quantityreceiveded + quantityrequested)
            //        {
            //            var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            //            ViewBag.ddlWarehouses = warehouses;

            //            ViewBag.podmvList = podmvList;
            //            ViewBag.ErrorTag = "Total quantity of  Invoice Items cannot exceed total qauntity ordered in Sales Order " + pomv.OrderNo + " .";
            //            ViewBag.Message = "Error";
            //            return View(pomv);
            //        }
            //    }
            //    else
            //    {
            //        var quantityinvoiced = db.SalesInvoiceDetails.Where(p => p.SalesInvoice.CreatedFrom == "Receipt Note" && p.SalesInvoice.ReferenceNo == pomv.ReferenceNo).Sum(p => (decimal?)p.Quantity) ?? 0;
            //        var quantityordered = db.SalesDeliveryDetails.Where(p => p.SalesDelivery.Id == pomv.ReferenceNo).Sum(p => (decimal?)p.Quantity) ?? 0;
            //        if (quantityordered < quantityinvoiced + quantityrequested)
            //        {
            //            var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            //            ViewBag.ddlWarehouses = warehouses;

            //            ViewBag.podmvList = podmvList;
            //            ViewBag.ErrorTag = "Total quantity of  Invoice Items cannot exceed total qauntity  in Delivery Note " + pomv.OrderNo + " .";
            //            ViewBag.Message = "Error";
            //            return View(pomv);
            //        }
            //    }
            //}
            if (pomv.CreatedFrom == "Delivery Note")
            {
                var checkDeliveryStatus = db.SalesDeliveries.Where(d => d.Id == pomv.ReferenceNo).Select(d => d.Status).FirstOrDefault();
                if (checkDeliveryStatus == "Invoiced")
                {
                    var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                    ViewBag.ddlWarehouses = warehouses;
                    // var taxes = mc.getDdlTaxes(userid, companyid, Branchid);
                    ViewBag.ddlTaxes = taxes;

                    ViewBag.podmvList = podmvList;
                    ViewBag.pcdmvList = pcdmvList;
                    return View(pomv);
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


                        var fyear = db.FinancialYearMasters.FirstOrDefault(f => f.fYearID == Fyid).Year;
                        var fs = fyear.Substring(2, 2);
                        var es = fyear.Substring(7, 2);
                        fyear = fs + "-" + es;

                        //Check Insert or Update
                        if (pomv.Id == 0)
                        {

                            //int countpo = db.SalesInvoices.Where(p => p.CompanyId == companyid && p.BranchId == Branchid).Count();
                            //pomv.NO = tc.GenerateCode("SI", countpo);
                            string getPrefix = "";
                            int countpo = 1;


                            if (db.SalesInvoices.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.WarehouseId == pomv.WarehouseId && p.FinancialYearId == Fyid && p.Mode == pomv.Mode).Count() != 0)
                            {

                                countpo = (int)db.SalesInvoices.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.WarehouseId == pomv.WarehouseId && p.FinancialYearId == Fyid && p.Mode == pomv.Mode).Max(p => p.InvoiceNo) + 1;
                            }

                            if (pomv.Mode == 1)
                                getPrefix = db.Prefixes.Where(p => p.DefaultPrefix == "SI" && p.CompanyId == companyid && p.BranchId == Branchid).Select(p => p.SetPrefix).FirstOrDefault();
                            else
                                getPrefix = db.Prefixes.Where(p => p.DefaultPrefix == "CS" && p.CompanyId == companyid && p.BranchId == Branchid).Select(p => p.SetPrefix).FirstOrDefault();

                            if (getPrefix != null)
                                pomv.NO = getPrefix + "/" + fyear + "/" + countpo;//tc.GenerateCode(fyear, countpo);
                            else
                            {
                                if (pomv.Mode == 1)
                                    pomv.NO = "SI" + "/" + fyear + "/" + countpo; //tc.GenerateCode(fyear, countpo);
                                else
                                    pomv.NO = "CS" + "/" + fyear + "/" + countpo;
                            }
                            //Insert into Salesorder table
                            SalesInvoice po = new SalesInvoice();
                            po.NO = pomv.NO;
                            po.InvoiceNo = countpo;
                            po.CustomerId = pomv.CustomerId;
                            po.Reference = pomv.Reference;
                            po.WarehouseId = pomv.WarehouseId;
                            //po.Date = date;
                            //po.DueDate = duedate;
                            //po.DespatchDate = despatchdate;
                            po.Date = invoicedate;
                            po.DueDate = invoicedate;
                            po.DespatchDate = invoicedate;
                            po.InvoiceDate = invoicedate;
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
                            po.CompoundedDis = pomv.CompoundedDis;
                            po.DisApplicable = pomv.DisApplicable;
                            po.FD1Applicable = pomv.FD1Applicable;
                            po.FD2Applicable = pomv.FD2Applicable;
                            po.FD3Applicable = pomv.FD3Applicable;
                            po.FD4Applicable = pomv.FD4Applicable;
                            po.DisAmount = pomv.DisAmount;
                            po.FD1Amount = pomv.FD1Amount;
                            po.FD2Amount = pomv.FD2Amount;
                            po.FD3Amount = pomv.FD3Amount;
                            po.FD4Amount = pomv.FD4Amount;
                            po.DiscountPerUnit = pomv.DiscountPerUnit;
                            po.FinancialYearId = Fyid;
                            po.CreatedBy = Createdby;
                            po.CreatedOn = DateTime.Now;
                            po.UserId = userid;
                            po.BranchId = Branchid;
                            po.CompanyId = companyid;
                            po.Status = pomv.Status;
                            po.Memo = pomv.Memo;
                            po.Mode = pomv.Mode;
                            po.SalesPerson = pomv.SalesPerson;
                            po.ShedId = pomv.ShedId;
                            po.RecurringSalesId = pomv.RecurringSalesId;
                            po.PaymentTermId = pomv.PaymentTermId;
                            po.LID = pomv.LID;
                            po.DespatchNo = pomv.DespatchNo;
                            po.DespatchThrough = pomv.DespatchThrough;
                            po.DespatchDestination = pomv.DespatchDestination;
                            po.IsPaid = false;
                            db.SalesInvoices.Add(po);

                            db.SaveChanges();
                            var po1 = db.SalesInvoices.Find(po.Id);
                            pomv.Id = po.Id;
                            var receiveditems = new List<Stock>();
                            if (pomv.CreatedFrom == "Delivery Note")
                            {

                                receiveditems = db.Stocks.Where(p => p.TransTag == "SD" && p.TranId == pomv.ReferenceNo).ToList();
                            }
                            decimal? dis = 0;
                            decimal? fd1 = 0;
                            decimal? fd2 = 0;
                            decimal? fd3 = 0;
                            decimal? fd4 = 0;

                            decimal? subtotal = 0;

                            decimal totaltaxonproduct = 0;

                            foreach (var podmv in podmvList)
                            {
                                if (podmv.ItemId != 0 && podmv.Quantity > 0)
                                {
                                    decimal taxrate = taxes.FirstOrDefault(r => r.TaxId == podmv.TaxId).Rate;
                                    SalesInvoiceDetail pod = new SalesInvoiceDetail();
                                    pod.SalesInvoiceId = po.Id;
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
                                    decimal? linetotal = podmv.Price * podmv.Quantity;
                                    decimal? disP = 0;
                                    decimal? fd1P = 0;
                                    decimal? fd2P = 0;
                                    decimal? fd3P = 0;
                                    decimal? fd4P = 0;
                                    decimal? disL = 0;
                                    decimal? fd1L = 0;
                                    decimal? fd2L = 0;
                                    decimal? fd3L = 0;
                                    decimal? fd4L = 0;
                                    //  decimal? disamt = linetotal * podmv.Discount / 100;
                                    decimal? disamt = podmv.DiscountAmount * podmv.Quantity;
                                    decimal? dislinetotal = linetotal - disamt;
                                    subtotal += dislinetotal;
                                    decimal? compounded = dislinetotal;
                                    var isComponded = pomv.CompoundedDis;

                                    if (pomv.DisApplicable == true && pomv.DisAmount > 0)
                                    {
                                        disL = dislinetotal * pomv.DisAmount / 100;
                                        dis += disL;
                                        disP = pomv.DisAmount;
                                        compounded = dislinetotal - disL;

                                    }
                                    if (pomv.FD1Applicable == true && pomv.FD1Amount > 0)
                                    {
                                        if (isComponded)
                                        {
                                            fd1L = compounded * pomv.FD1Amount / 100;
                                            fd1 += fd1L;
                                            fd1P = (100 - pomv.DisAmount) * pomv.FD1Amount / 100;
                                            compounded = compounded - fd1L;

                                        }
                                        else
                                        {
                                            fd1L = dislinetotal * pomv.FD1Amount / 100;
                                            fd1 += fd1L;
                                            fd1P = pomv.FD1Amount;
                                            compounded = compounded - fd1L;

                                        }
                                    }
                                    if (pomv.FD2Applicable == true && pomv.FD2Amount > 0)
                                    {
                                        if (isComponded)
                                        {
                                            fd2L = compounded * pomv.FD2Amount / 100;
                                            fd2 += fd2L;
                                            fd2P = (100 - pomv.DisAmount - fd1P) * pomv.FD2Amount / 100;
                                            compounded = compounded - fd2L;

                                        }
                                        else
                                        {
                                            fd2L = dislinetotal * pomv.FD2Amount / 100;
                                            fd2 += fd2L;
                                            fd2P = pomv.FD2Amount;
                                            compounded = compounded - fd2L;

                                        }
                                    }
                                    if (pomv.FD3Applicable == true && pomv.FD3Amount > 0)
                                    {
                                        if (isComponded)
                                        {
                                            fd3L = compounded * pomv.FD3Amount / 100;
                                            fd3 += fd3L;
                                            fd3P = (100 - pomv.DisAmount - fd1P - fd2P) * pomv.FD3Amount / 100;
                                            compounded = compounded - fd3L;

                                        }
                                        else
                                        {
                                            fd3L = dislinetotal * pomv.FD3Amount / 100;
                                            fd3 += fd3L;
                                            fd3P = pomv.FD3Amount;
                                            compounded = compounded - fd3L;

                                        }
                                    }
                                    if (pomv.FD4Applicable == true && pomv.FD4Amount > 0)
                                    {
                                        if (isComponded)
                                        {
                                            fd4L = compounded * pomv.FD4Amount / 100;
                                            fd4 += fd4L;
                                            fd4P = (100 - pomv.DisAmount - fd1P - fd2P - fd3P) * pomv.FD4Amount / 100;
                                            compounded = compounded - fd4L;

                                        }
                                        else
                                        {
                                            fd4L = dislinetotal * pomv.FD4Amount / 100;
                                            fd4 += fd4L;
                                            fd4P += pomv.FD4Amount;
                                            compounded = compounded - fd4L;

                                        }
                                    }

                                    //if (po.Status == "Saved")
                                    //{
                                    //    var stock = new Stock();
                                    //    stock.ArticleID = podmv.ItemId;
                                    //    stock.Items = podmv.Quantity ;
                                    //    stock.Price = compounded;
                                    //    stock.TranCode = "OUT";
                                    //    stock.TransTag = "SI";
                                    //    stock.TranDate = date;
                                    //    stock.TranId = po.Id;
                                    //    stock.WarehouseId = po.WarehouseId;
                                    //    stock.UserId = po.UserId;
                                    //    stock.CompanyId = po.CompanyId;
                                    //    stock.BranchId = po.BranchId;
                                    //    stock.CreatedBy = po.CreatedBy;
                                    //    db.Stocks.Add(stock);

                                    //    if (pomv.CreatedFrom == "Delivery Note")
                                    //    {

                                    //        if (receiveditems != null)
                                    //        {
                                    //            var receiveditem = receiveditems.Where(p => p.ArticleID == podmv.ItemId).FirstOrDefault();
                                    //            if (receiveditem != null)
                                    //            {

                                    //                if (receiveditem.Items == podmv.Quantity)
                                    //                {
                                    //                    receiveditem.Items = 0;
                                    //                    //db.Stocks.Remove(receiveditem);
                                    //                    //receiveditems.Remove(receiveditem);
                                    //                }
                                    //                //    receiveditem.Items = 0;
                                    //                else if (receiveditem.Items > podmv.Quantity)
                                    //                {
                                    //                    receiveditem.Items = receiveditem.Items - podmv.Quantity;
                                    //                }
                                    //                else
                                    //                {
                                    //                    pomv.NO = "";
                                    //                    var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                    //                    ViewBag.ddlWarehouses = warehouses1;

                                    //                    ViewBag.podmvList = podmvList;
                                    //                    ViewBag.pcdmvList = pcdmvList;
                                    //                    ViewBag.ErrorTag = "Item " + podmv.ItemName + " cannot exceed Quantity of " + receiveditem.Items;
                                    //                    ViewBag.Message = "Error";
                                    //                    return View(pomv);
                                    //                }


                                    //            }
                                    //            else
                                    //            {
                                    //                pomv.NO = "";
                                    //                var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                    //                ViewBag.ddlWarehouses = warehouses1;

                                    //                ViewBag.podmvList = podmvList;
                                    //                ViewBag.pcdmvList = pcdmvList;
                                    //                ViewBag.ErrorTag = "Item " + podmv.ItemName + " is not in Delivery Note " + pomv.OrderNo + "  or already invoiced.";
                                    //                ViewBag.Message = "Error";
                                    //                return View(pomv);
                                    //            }



                                    //        }
                                    //        else
                                    //        {
                                    //            pomv.NO = "";
                                    //            var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                    //            ViewBag.ddlWarehouses = warehouses1;

                                    //            ViewBag.podmvList = podmvList;
                                    //            ViewBag.pcdmvList = pcdmvList;
                                    //            ViewBag.ErrorTag = "Item " + podmv.ItemName + " is not in Delivery Note " + pomv.OrderNo + "  or already invoiced.";
                                    //            ViewBag.Message = "Error";
                                    //            return View(pomv);
                                    //        }
                                    //    }
                                    //}

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
                                            var salesTax = new SalesTax();
                                            salesTax.SalesInvoiceId = pomv.Id;
                                            salesTax.ItemId = podmv.ItemId;
                                            salesTax.TaxId = (long)taxComp.TaxCompId;
                                            salesTax.Amount = (decimal)amount1;
                                            salesTax.CurrencyRate = pomv.Currencyrate;
                                            if (po.Status == "Saved")
                                            {
                                                db.SalesTaxes.Add(salesTax);
                                            }
                                            var tname = taxes.FirstOrDefault(t => t.TaxId == salesTax.TaxId);
                                            var invoiceTotal = new InvoiceTotal { Id = 7, Name = tname.Name + '(' + tname.Rate + "%)" + " On Product", Amount = (decimal)salesTax.Amount };
                                            invoiceTotalList.Add(invoiceTotal);
                                        }
                                        pod.TaxPercent = (decimal)effectivetaxrate;
                                        pod.TaxAmount = (decimal)totalamount;
                                    }
                                    else
                                    {

                                        var salesTax = new SalesTax();
                                        salesTax.SalesInvoiceId = pomv.Id;
                                        salesTax.ItemId = podmv.ItemId;
                                        salesTax.TaxId = podmv.TaxId;
                                        salesTax.Amount = (decimal)taxrate * (decimal)compounded / 100;
                                        salesTax.CurrencyRate = pomv.Currencyrate;
                                        if (po.Status == "Saved")
                                        {
                                            db.SalesTaxes.Add(salesTax);
                                        }
                                        var tname = taxes.FirstOrDefault(t => t.TaxId == salesTax.TaxId);
                                        var invoiceTotal = new InvoiceTotal { Id = 7, Name = tname.Name + '(' + tname.Rate + "%)" + " On Product", Amount = (decimal)salesTax.Amount };
                                        invoiceTotalList.Add(invoiceTotal);

                                        pod.TaxPercent = (decimal)taxrate;
                                        pod.TaxAmount = (decimal)salesTax.Amount;

                                    }
                                    totaltaxonproduct += pod.TaxAmount;
                                    pod.CustDiscount = (decimal)(disP + fd1P + fd2P + fd3P + fd4P);
                                    pod.CustDiscountAmount = (decimal)(disL + fd1L + fd2L + fd3L + fd4L);
                                    pod.TotalAmount = (pod.Price * pod.Quantity) - (pod.DiscountAmount * pod.Quantity) - pod.CustDiscountAmount + pod.TaxAmount;
                                    db.SalesInvoiceDetails.Add(pod);
                                }

                            }

                            po1.Dis = dis;
                            po1.FD1 = fd1;
                            po1.FD2 = fd2;
                            po1.FD3 = fd3;
                            po1.FD4 = fd4;
                            po1.TaxProduct = totaltaxonproduct;
                            po1.SubTotal = (decimal)subtotal;
                            if (subtotal > 0)
                            {
                                var invoiceTotal = new InvoiceTotal { Id = 1, Name = "Sub Total", Amount = (decimal)subtotal };
                                invoiceTotalList.Add(invoiceTotal);
                            }
                            if (dis > 0)
                            {
                                var invoiceTotal = new InvoiceTotal { Id = 2, Name = "Discount", Amount = (decimal)-dis };
                                invoiceTotalList.Add(invoiceTotal);
                            }
                            if (fd1 > 0)
                            {
                                var invoiceTotal = new InvoiceTotal { Id = 3, Name = "Further Discount 1", Amount = (decimal)-fd1 };
                                invoiceTotalList.Add(invoiceTotal);
                            }
                            if (fd2 > 0)
                            {
                                var invoiceTotal = new InvoiceTotal { Id = 4, Name = "Further Discount 2", Amount = (decimal)-fd2 };
                                invoiceTotalList.Add(invoiceTotal);
                            }
                            if (fd3 > 0)
                            {
                                var invoiceTotal = new InvoiceTotal { Id = 5, Name = "Further Discount 3", Amount = (decimal)-fd3 };
                                invoiceTotalList.Add(invoiceTotal);
                            }
                            if (fd4 > 0)
                            {
                                var invoiceTotal = new InvoiceTotal { Id = 6, Name = "Further Discount 4", Amount = (decimal)-fd4 };
                                invoiceTotalList.Add(invoiceTotal);
                            }
                            decimal totaladdamount = 0;
                            decimal totaldeductamount = 0;
                            decimal totaltaxonother = 0;
                            foreach (var pcdmv in pcdmvList)
                            {
                                if (pcdmv.CostingId != 0 && pcdmv.TaxId > 0 && pcdmv.CostAmount > 0)
                                {
                                    decimal taxrate = taxes.FirstOrDefault(r => r.TaxId == pcdmv.TaxId).Rate;


                                    SalesCostingDetail pcd = new SalesCostingDetail();
                                    pcd.SalesInvoiceId = po.Id;
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
                                                var crate = (effectiveTotal + pcdmv.CostAmount) * rate / 100;
                                                effectiveTotal += crate;
                                                parentEffectiveRate = crate;
                                                amount1 = parentEffectiveRate;
                                                totalamount += amount1;
                                            }

                                            var salesTax = new SalesTax();
                                            salesTax.SalesInvoiceId = pomv.Id;
                                            salesTax.CostId = pcdmv.CostingId;
                                            salesTax.TaxId = (long)taxComp.TaxCompId;
                                            salesTax.Amount = (decimal)amount1;
                                            salesTax.CurrencyRate = pomv.Currencyrate;

                                            if (po.Status == "Saved")
                                            {
                                                db.SalesTaxes.Add(salesTax);
                                            }
                                            var tname = taxes.FirstOrDefault(t => t.TaxId == salesTax.TaxId);
                                            var invoiceTotal = new InvoiceTotal { Id = 10, Name = tname.Name + '(' + tname.Rate + "%)" + " On Others", Amount = (decimal)salesTax.Amount };
                                            invoiceTotalList.Add(invoiceTotal);
                                        }
                                        pcd.TaxPercent = (decimal)effectivetaxrate;
                                        pcd.TaxAmount = (decimal)totalamount;
                                    }
                                    else
                                    {

                                        var salesTax = new SalesTax();
                                        salesTax.SalesInvoiceId = pomv.Id;
                                        salesTax.CostId = pcdmv.CostingId;
                                        salesTax.TaxId = pcdmv.TaxId;
                                        salesTax.Amount = (decimal)taxrate * (decimal)pcdmv.CostAmount / 100;
                                        salesTax.CurrencyRate = pomv.Currencyrate;

                                        if (po.Status == "Saved")
                                        {
                                            db.SalesTaxes.Add(salesTax);
                                        }

                                        pcd.TaxPercent = (decimal)taxrate;
                                        pcd.TaxAmount = (decimal)salesTax.Amount;

                                        var tname = taxes.FirstOrDefault(t => t.TaxId == salesTax.TaxId);
                                        var invoiceTotal = new InvoiceTotal { Id = 10, Name = tname.Name + '(' + tname.Rate + "%)" + " On Others", Amount = (decimal)salesTax.Amount };
                                        invoiceTotalList.Add(invoiceTotal);
                                    }
                                    totaltaxonother += pcd.TaxAmount;
                                    db.SalesCostingDetails.Add(pcd);
                                }

                            }
                            var invoiceTotal1 = new InvoiceTotal { Id = 8, Name = "Added Cost", Amount = totaladdamount };
                            invoiceTotalList.Add(invoiceTotal1);
                            var invoiceTotal2 = new InvoiceTotal { Id = 9, Name = "Deducted Cost", Amount = -totaldeductamount };
                            invoiceTotalList.Add(invoiceTotal2);
                            po1.TotalAddAmount = totaladdamount;
                            po1.TotalDeductAmount = totaldeductamount;
                            po1.TaxOther = totaltaxonother;
                            decimal roundamount = 0;
                            decimal unroundedtotal = (decimal)(subtotal - (dis + fd1 + fd2 + fd3 + fd4) + totaltaxonproduct + totaladdamount - totaldeductamount + totaltaxonother);
                            //decimal roundedtotal = Math.Round(unroundedtotal);
                            //if (unroundedtotal != roundedtotal)
                            //{
                            //    roundamount = roundedtotal - unroundedtotal;
                            //    var invoiceTotal3 = new InvoiceTotal { Id = 11, Name = "Round Off", Amount = roundamount };
                            //    invoiceTotalList.Add(invoiceTotal3);

                            //}
                            //po1.RoundOff = roundamount;
                            po1.GrandTotal = unroundedtotal;
                            po1.BCGrandTotal = Math.Round(unroundedtotal * pomv.Currencyrate, 2);
                            var invoiceTotal4 = new InvoiceTotal { Id = 12, Name = "Grand Total In " + pomv.TransactionCurrencyCode, Amount = po1.GrandTotal };
                            invoiceTotalList.Add(invoiceTotal4);
                            if (pomv.CurrencyId != pomv.TransactionCurrency)
                            {

                                var invoiceTotal5 = new InvoiceTotal { Id = 13, Name = "Grand Total In " + pomv.BaseCurrencyCode, Amount = po1.BCGrandTotal };
                                invoiceTotalList.Add(invoiceTotal5);
                            }
                            if (pomv.CreatedFrom == "Sales Order")
                            {
                                po.CreatedFrom = "Sales Order";
                                po.ReferenceNo = pomv.ReferenceNo;
                                po.OrderNo = pomv.OrderNo;
                                var findpo = db.SalesOrders.Find(pomv.ReferenceNo);
                                findpo.Status = InventoryConst.cns_Invoiced;
                            }
                            if (pomv.CreatedFrom == "Delivery Note")
                            {
                                po.CreatedFrom = "Delivery Note";
                                po.ReferenceNo = pomv.ReferenceNo;
                                po.OrderNo = pomv.OrderNo;
                                po.SalesOrderNo = pomv.SalesOrderNo;
                                var finddn = db.SalesDeliveries.Find(pomv.ReferenceNo);
                                finddn.Status = InventoryConst.cns_Invoiced;
                                if (finddn.CreatedFrom == "Sales Order")
                                {
                                    var findpo = db.SalesOrders.Find(finddn.ReferenceNo);
                                    findpo.Status = InventoryConst.cns_Invoiced;
                                }

                            }

                            if (pomv.Mode == 2)
                            {
                                tr: string TransNo = Convert.ToString(GenRandom.GetRandom());

                                var trans = db.ReceiptPayments.Where(d => d.transactionNo == TransNo).Select(d => d.Id).FirstOrDefault();

                                if (trans > 0)
                                {
                                    goto tr;

                                }
                                var recpayment = new ReceiptPayment();

                                recpayment.RPdate = invoicedate;
                                recpayment.RPDatetime = DateTime.Now;

                                recpayment.fYearId = Fyid;
                                recpayment.BranchId = (int)Branchid;

                                recpayment.CompanyId = (int)companyid;
                                recpayment.UserId = (int)userid;

                                if (po.BCGrandTotal > 0)
                                {
                                    long CashId = db.LedgerMasters.Where(l => l.ledgerName == "CASH IN HAND").Select(l => l.LID).FirstOrDefault();

                                    recpayment.RPCashId = Convert.ToInt32(CashId);
                                    recpayment.RPCashAmount = po.BCGrandTotal;
                                }
                                else
                                {

                                    recpayment.RPCashId = 0;
                                    recpayment.RPCashAmount = 0;


                                }

                                recpayment.transactionType = "Cash Invoice";
                                recpayment.RPType = InventoryConst.Cns_Receive;
                                int vouch = 1;
                                string prefix = "CR/" + fyear + "/";
                                if (db.ReceiptPayments.Where(d => (d.transactionType == "General Receive" || d.transactionType == "General Payment" || d.transactionType == "Cash Invoice") && d.CompanyId == companyid && d.BranchId == Branchid && d.UserId == userid && d.fYearId == Fyid && d.Prefix == prefix).FirstOrDefault() != null)
                                {
                                    vouch = (db.ReceiptPayments.Where(d => (d.transactionType == "General Receive" || d.transactionType == "General Payment" || d.transactionType == "Cash Invoice") && d.CompanyId == companyid && d.BranchId == Branchid && d.UserId == userid && d.fYearId == Fyid && d.Prefix == prefix).Max(v => (int?)v.VoucherNo) ?? 0) + 1;

                                }
                                recpayment.VoucherNo = vouch;
                                recpayment.Prefix = prefix;


                                recpayment.TotalAmount = po.BCGrandTotal;
                                if (po1.CustomerId == null)
                                    recpayment.ledgerId = 59;
                                else
                                    recpayment.ledgerId = (int)po.Customer.LId;
                                recpayment.transactionNo = TransNo;
                                recpayment.Remarks = "Cash Invoice " + po.NO;




                                recpayment.CreatedBy = Createdby;

                                db.ReceiptPayments.Add(recpayment);
                                //Insert unique transaction no in cash invoice generated
                                po1.TransNo = TransNo;


                            }

                        }
                        else
                        {
                            //Update Sales Receipt
                            var po = db.SalesInvoices.Find(pomv.Id);
                            //if (po.Status == "Saved")
                            //{
                            //    var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                            //    ViewBag.ddlWarehouses = warehouses1;

                            //    ViewBag.podmvList = podmvList;
                            //    ViewBag.pcdmvList = pcdmvList;
                            //    ViewBag.Message = "No modification is allowed in posted Sales Invoice.";
                            //    return View(pomv);
                            //}

                            var receiveditems = new List<Stock>();
                            var invoiceditems = new List<Stock>();
                            if (po.Status == "Saved")
                            {
                                if (pomv.CreatedFrom == "Delivery Note")
                                {

                                    receiveditems = db.Stocks.Where(p => p.TransTag == "SD" && p.TranId == pomv.ReferenceNo).ToList();
                                    invoiceditems = db.Stocks.Where(p => p.TransTag == "SI" && p.TranId == po.Id).ToList();
                                }
                                else
                                {
                                    var oldstocks = db.Stocks.Where(p => p.TransTag == "SI" && p.TranId == po.Id).ToList();
                                    foreach (var oldstock in oldstocks)
                                    {
                                        db.Stocks.Remove(oldstock);
                                    }
                                }
                            }



                            if (pomv.InvoiceNo != null)
                            {

                                var replacewith = Convert.ToString(pomv.InvoiceNo);
                                // var resultString = Regex.Match(replacewith, @"\d+").Value;
                                //    var result = Int64.Parse(resultString);

                                //  pomv.NO = Regex.Replace(po.NO, @"(?<=/)(\w+?)(?=/)", replacewith);
                                var no = Regex.Match(po.NO, @"\/([0-9]+)(?=[^\/]*$)");
                                var m = Regex.Match(po.NO, @"(\d+)[^-]*$");
                                pomv.NO = Regex.Replace(po.NO, @"([0-9]+)(?=[^\/]*$)", replacewith);
                                var duplicateInv = db.SalesInvoices.Any(p => p.NO == pomv.NO && p.CompanyId == companyid);
                                if (duplicateInv)
                                {
                                    var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                    ViewBag.ddlWarehouses = warehouses1;

                                    ViewBag.podmvList = podmvList;
                                    ViewBag.pcdmvList = pcdmvList;
                                    ViewBag.Message = "Sales Invoice No Cannot be duplicate.";
                                    return View(pomv);
                                }
                                po.InvoiceNo = pomv.InvoiceNo;

                            }

                            //====================================NEW CHANGE BY ARNAB===========================================
                           
                            if (po.Mode != pomv.Mode)
                            {
                                string getPrefix = "";
                                int countpo = 1;
                                if (db.SalesInvoices.Where(p => p.CompanyId == companyid && p.BranchId == po.BranchId && p.WarehouseId == pomv.WarehouseId && p.FinancialYearId == Fyid && p.Mode == pomv.Mode).Count() != 0)
                                {

                                    countpo = (int)db.SalesInvoices.Where(p => p.CompanyId == companyid && p.BranchId == po.BranchId && p.WarehouseId == pomv.WarehouseId && p.FinancialYearId == Fyid && p.Mode == pomv.Mode).Max(p => p.InvoiceNo) + 1;
                                }

                                if (pomv.Mode == 1)
                                    getPrefix = db.Prefixes.Where(p => p.DefaultPrefix == "SI" && p.CompanyId == companyid && p.BranchId == po.BranchId).Select(p => p.SetPrefix).FirstOrDefault();
                                else
                                    getPrefix = db.Prefixes.Where(p => p.DefaultPrefix == "CS" && p.CompanyId == companyid && p.BranchId == po.BranchId).Select(p => p.SetPrefix).FirstOrDefault();

                                if (getPrefix != null)
                                    pomv.NO = getPrefix + "/" + fyear + "/" + countpo;//tc.GenerateCode(fyear, countpo);
                                else
                                {
                                    if (pomv.Mode == 1)
                                        pomv.NO = "SI" + "/" + fyear + "/" + countpo; //tc.GenerateCode(fyear, countpo);
                                    else
                                        pomv.NO = "CS" + "/" + fyear + "/" + countpo;
                                }

                                po.InvoiceNo = countpo;
                            }


                            po.NO = pomv.NO;
                          
                            po.CustomerId = pomv.CustomerId;
                            po.Reference = pomv.Reference;
                            po.WarehouseId = pomv.WarehouseId;
                            //po.Date = date;
                            //po.DueDate = duedate;
                            //po.DespatchDate = despatchdate;
                            po.Date = invoicedate;
                            po.DueDate = invoicedate;
                            po.DespatchDate = invoicedate;
                            po.InvoiceDate = invoicedate;
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
                            po.RecurringSalesId = pomv.RecurringSalesId;
                            po.CompoundedDis = pomv.CompoundedDis;
                            po.DisApplicable = pomv.DisApplicable;
                            po.FD1Applicable = pomv.FD1Applicable;
                            po.FD2Applicable = pomv.FD2Applicable;
                            po.FD3Applicable = pomv.FD3Applicable;
                            po.FD4Applicable = pomv.FD4Applicable;
                            po.DisAmount = pomv.DisAmount;
                            po.FD1Amount = pomv.FD1Amount;
                            po.FD2Amount = pomv.FD2Amount;
                            po.FD3Amount = pomv.FD3Amount;
                            po.FD4Amount = pomv.FD4Amount;
                            po.DiscountPerUnit = pomv.DiscountPerUnit;
                            po.FinancialYearId = Fyid;
                            po.CreatedBy = Createdby;
                            po.CreatedOn = DateTime.Now;
                            po.UserId = userid;
                            po.BranchId = Branchid;
                            po.CompanyId = companyid;
                            po.Status = pomv.Status;
                            po.Memo = pomv.Memo;
                            po.Mode = pomv.Mode;
                            po.PaymentTermId = pomv.PaymentTermId;
                            po.LID = pomv.LID;
                            po.ShedId = pomv.ShedId;
                            po.DespatchNo = pomv.DespatchNo;
                            po.DespatchThrough = pomv.DespatchThrough;
                            po.DespatchDestination = pomv.DespatchDestination;
                            po.IsPaid = false;
                            var podOldRecords = db.SalesInvoiceDetails.Where(p => p.SalesInvoiceId == po.Id).ToList();
                            foreach (var podOld in podOldRecords)
                            {
                                db.SalesInvoiceDetails.Remove(podOld);
                            }
                            var salesTaxOldRecords = db.SalesTaxes.Where(p => p.SalesInvoiceId == po.Id).ToList();
                            foreach (var podOld in salesTaxOldRecords)
                            {
                                db.SalesTaxes.Remove(podOld);
                            }

                            decimal? dis = 0;
                            decimal? fd1 = 0;
                            decimal? fd2 = 0;
                            decimal? fd3 = 0;
                            decimal? fd4 = 0;

                            decimal? subtotal = 0;

                            decimal totaltaxonproduct = 0;

                            foreach (var podmv in podmvList)
                            {
                                if (podmv.ItemId != 0 && podmv.Quantity > 0)
                                {
                                    decimal taxrate = taxes.FirstOrDefault(r => r.TaxId == podmv.TaxId).Rate;
                                    SalesInvoiceDetail pod = new SalesInvoiceDetail();
                                    pod.SalesInvoiceId = po.Id;
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

                                    decimal? linetotal = podmv.Price * podmv.Quantity;
                                    decimal? disP = 0;
                                    decimal? fd1P = 0;
                                    decimal? fd2P = 0;
                                    decimal? fd3P = 0;
                                    decimal? fd4P = 0;
                                    decimal? disL = 0;
                                    decimal? fd1L = 0;
                                    decimal? fd2L = 0;
                                    decimal? fd3L = 0;
                                    decimal? fd4L = 0;
                                    // decimal? disamt = linetotal * podmv.Discount / 100;
                                    decimal? disamt = podmv.DiscountAmount * podmv.Quantity;
                                    decimal? dislinetotal = linetotal - disamt;
                                    subtotal += dislinetotal;
                                    decimal? compounded = dislinetotal;
                                    var isComponded = pomv.CompoundedDis;

                                    if (pomv.DisApplicable == true && pomv.DisAmount > 0)
                                    {
                                        disL = dislinetotal * pomv.DisAmount / 100;
                                        dis += disL;
                                        disP = pomv.DisAmount;
                                        compounded = dislinetotal - disL;

                                    }
                                    if (pomv.FD1Applicable == true && pomv.FD1Amount > 0)
                                    {
                                        if (isComponded)
                                        {
                                            fd1L = compounded * pomv.FD1Amount / 100;
                                            fd1 += fd1L;
                                            fd1P = (100 - pomv.DisAmount) * pomv.FD1Amount / 100;
                                            compounded = compounded - fd1L;

                                        }
                                        else
                                        {
                                            fd1L = dislinetotal * pomv.FD1Amount / 100;
                                            fd1 += fd1L;
                                            fd1P = pomv.FD1Amount;
                                            compounded = compounded - fd1L;

                                        }
                                    }
                                    if (pomv.FD2Applicable == true && pomv.FD2Amount > 0)
                                    {
                                        if (isComponded)
                                        {
                                            fd2L = compounded * pomv.FD2Amount / 100;
                                            fd2 += fd2L;
                                            fd2P = (100 - pomv.DisAmount - fd1P) * pomv.FD2Amount / 100;
                                            compounded = compounded - fd2L;

                                        }
                                        else
                                        {
                                            fd2L = dislinetotal * pomv.FD2Amount / 100;
                                            fd2 += fd2L;
                                            fd2P = pomv.FD2Amount;
                                            compounded = compounded - fd2L;

                                        }
                                    }
                                    if (pomv.FD3Applicable == true && pomv.FD3Amount > 0)
                                    {
                                        if (isComponded)
                                        {
                                            fd3L = compounded * pomv.FD3Amount / 100;
                                            fd3 += fd3L;
                                            fd3P = (100 - pomv.DisAmount - fd1P - fd2P) * pomv.FD3Amount / 100;
                                            compounded = compounded - fd3L;

                                        }
                                        else
                                        {
                                            fd3L = dislinetotal * pomv.FD3Amount / 100;
                                            fd3 += fd3L;
                                            fd3P = pomv.FD3Amount;
                                            compounded = compounded - fd3L;

                                        }
                                    }
                                    if (pomv.FD4Applicable == true && pomv.FD4Amount > 0)
                                    {
                                        if (isComponded)
                                        {
                                            fd4L = compounded * pomv.FD4Amount / 100;
                                            fd4 += fd4L;
                                            fd4P = (100 - pomv.DisAmount - fd1P - fd2P - fd3P) * pomv.FD4Amount / 100;
                                            compounded = compounded - fd4L;

                                        }
                                        else
                                        {
                                            fd4L = dislinetotal * pomv.FD4Amount / 100;
                                            fd4 += fd4L;
                                            fd4P += pomv.FD4Amount;
                                            compounded = compounded - fd4L;

                                        }
                                    }

                                    //if (po.Status == "Saved")
                                    //{

                                    //    if (pomv.CreatedFrom == "Delivery Note")
                                    //    {
                                    //        if (invoiceditems != null)
                                    //        {

                                    //            var invoiceditem = invoiceditems.Where(p => p.ArticleID == podmv.ItemId).FirstOrDefault();
                                    //            if (invoiceditem != null)
                                    //            {
                                    //                if (receiveditems != null)
                                    //                {
                                    //                    var receiveditem = receiveditems.Where(p => p.ArticleID == podmv.ItemId).FirstOrDefault();
                                    //                    if (receiveditem != null)
                                    //                    {
                                    //                        receiveditem.Items = receiveditem.Items + invoiceditem.Items; 

                                    //                        if (receiveditem.Items >= podmv.Quantity)
                                    //                        {
                                    //                            receiveditem.Items = receiveditem.Items - podmv.Quantity;
                                    //                        }
                                    //                        else
                                    //                        {
                                    //                            var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                    //                            ViewBag.ddlWarehouses = warehouses1;

                                    //                            ViewBag.podmvList = podmvList;
                                    //                            ViewBag.pcdmvList = pcdmvList;
                                    //                            ViewBag.ErrorTag = "Item " + podmv.ItemName + " cannot exceed Quantity of " + receiveditem.Items;
                                    //                            ViewBag.Message = "Error";
                                    //                            return View(pomv);
                                    //                        }

                                    //                        invoiceditem.Items = podmv.Quantity;
                                    //                    }
                                    //                    else
                                    //                    {
                                    //                        var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                    //                        ViewBag.ddlWarehouses = warehouses1;

                                    //                        ViewBag.podmvList = podmvList;
                                    //                        ViewBag.pcdmvList = pcdmvList;
                                    //                        ViewBag.ErrorTag = "Item " + podmv.ItemName + " is not in  Delivery Note " + pomv.OrderNo;
                                    //                        ViewBag.Message = "Error";
                                    //                        return View(pomv);
                                    //                    }



                                    //                }
                                    //                else
                                    //                {
                                    //                    var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                    //                    ViewBag.ddlWarehouses = warehouses1;

                                    //                    ViewBag.podmvList = podmvList;
                                    //                    ViewBag.pcdmvList = pcdmvList;
                                    //                    ViewBag.ErrorTag = "Item " + podmv.ItemName + " is not in Delivery Receipt " + pomv.OrderNo;
                                    //                    ViewBag.Message = "Error";
                                    //                    return View(pomv);
                                    //                }
                                    //            }
                                    //            else
                                    //            {
                                    //                if (receiveditems != null)
                                    //                {
                                    //                    var receiveditem = receiveditems.Where(p => p.ArticleID == podmv.ItemId).FirstOrDefault();
                                    //                    if (receiveditem != null)
                                    //                    {

                                    //                        //if (receiveditem.Items == podmv.Quantity)
                                    //                        //{
                                    //                        //    receiveditem.Items = 0;
                                    //                        //    //db.Stocks.Remove(receiveditem);
                                    //                        //    //receiveditems.Remove(receiveditem);
                                    //                        //}
                                    //                        ////    receiveditem.Items = 0;
                                    //                        //else
                                    //                        if (receiveditem.Items > podmv.Quantity)
                                    //                        {
                                    //                            receiveditem.Items = receiveditem.Items - podmv.Quantity;
                                    //                        }
                                    //                        else
                                    //                        {
                                    //                            var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                    //                            ViewBag.ddlWarehouses = warehouses1;

                                    //                            ViewBag.podmvList = podmvList;
                                    //                            ViewBag.pcdmvList = pcdmvList;
                                    //                            ViewBag.ErrorTag = "Item " + podmv.ItemName + " cannot exceed Quantity of " + receiveditem.Items;
                                    //                            ViewBag.Message = "Error";
                                    //                            return View(pomv);
                                    //                        }
                                    //                    }
                                    //                    else
                                    //                    {
                                    //                        var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                    //                        ViewBag.ddlWarehouses = warehouses1;

                                    //                        ViewBag.podmvList = podmvList;
                                    //                        ViewBag.pcdmvList = pcdmvList;
                                    //                        ViewBag.ErrorTag = "Item " + podmv.ItemName + " is not in  Delivery Note " + pomv.OrderNo;
                                    //                        ViewBag.Message = "Error";
                                    //                        return View(pomv);
                                    //                    }

                                    //                }
                                    //                var stock = new Stock();
                                    //                stock.ArticleID = podmv.ItemId;
                                    //                stock.Items = podmv.Quantity;
                                    //                stock.Price = compounded;
                                    //                stock.TranCode = "OUT";
                                    //                stock.TransTag = "SI";
                                    //                stock.TranDate = date;
                                    //                stock.TranId = po.Id;
                                    //                stock.WarehouseId = po.WarehouseId;
                                    //                stock.UserId = po.UserId;
                                    //                stock.CompanyId = po.CompanyId;
                                    //                stock.BranchId = po.BranchId;
                                    //                stock.CreatedBy = po.CreatedBy;
                                    //                db.Stocks.Add(stock);
                                    //            }
                                    //        }
                                    //        else
                                    //        {
                                    //            if (receiveditems != null)
                                    //            {
                                    //                var receiveditem = receiveditems.Where(p => p.ArticleID == podmv.ItemId).FirstOrDefault();
                                    //                if (receiveditem != null)
                                    //                {

                                    //                    if (receiveditem.Items >= podmv.Quantity)
                                    //                    {
                                    //                        receiveditem.Items = receiveditem.Items - podmv.Quantity;
                                    //                    }
                                    //                    else
                                    //                    {
                                    //                        var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                    //                        ViewBag.ddlWarehouses = warehouses1;

                                    //                        ViewBag.podmvList = podmvList;
                                    //                        ViewBag.pcdmvList = pcdmvList;
                                    //                        ViewBag.ErrorTag = "Item " + podmv.ItemName + " cannot exceed Quantity of " + receiveditem.Items;
                                    //                        ViewBag.Message = "Error";
                                    //                        return View(pomv);
                                    //                    }
                                    //                }
                                    //                else
                                    //                {
                                    //                    var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                    //                    ViewBag.ddlWarehouses = warehouses1;

                                    //                    ViewBag.podmvList = podmvList;
                                    //                    ViewBag.pcdmvList = pcdmvList;
                                    //                    ViewBag.ErrorTag = "Item " + podmv.ItemName + " is not in  Delivery Note " + pomv.OrderNo;
                                    //                    ViewBag.Message = "Error";
                                    //                    return View(pomv);
                                    //                }

                                    //            }
                                    //            var stock = new Stock();
                                    //            stock.ArticleID = podmv.ItemId;
                                    //            stock.Items = podmv.Quantity;
                                    //            stock.Price = compounded;
                                    //            stock.TranCode = "OUT";
                                    //            stock.TransTag = "SI";
                                    //            stock.TranDate = date;
                                    //            stock.TranId = po.Id;

                                    //            stock.WarehouseId = po.WarehouseId;
                                    //            stock.UserId = po.UserId;
                                    //            stock.CompanyId = po.CompanyId;
                                    //            stock.BranchId = po.BranchId;
                                    //            stock.CreatedBy = po.CreatedBy;
                                    //            db.Stocks.Add(stock);
                                    //        }








                                    //    }
                                    //    else
                                    //    {
                                    //        var stock = new Stock();
                                    //        stock.ArticleID = podmv.ItemId;
                                    //        stock.Items = podmv.Quantity ;
                                    //        stock.Price = compounded;
                                    //        stock.TranCode = "OUT";
                                    //        stock.TransTag = "SI";
                                    //        stock.TranDate = date;
                                    //        stock.TranId = po.Id;
                                    //        stock.WarehouseId = po.WarehouseId;
                                    //        stock.UserId = po.UserId;
                                    //        stock.CompanyId = po.CompanyId;
                                    //        stock.BranchId = po.BranchId;
                                    //        stock.CreatedBy = po.CreatedBy;
                                    //        db.Stocks.Add(stock);
                                    //    }
                                    //}


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
                                            var salesTax = new SalesTax();
                                            salesTax.SalesInvoiceId = pomv.Id;
                                            salesTax.ItemId = podmv.ItemId;
                                            salesTax.TaxId = (long)taxComp.TaxCompId;
                                            salesTax.Amount = (decimal)amount1;
                                            salesTax.CurrencyRate = pomv.Currencyrate;
                                            if (po.Status == "Saved")
                                            {
                                                db.SalesTaxes.Add(salesTax);
                                            }
                                            var tname = taxes.FirstOrDefault(t => t.TaxId == salesTax.TaxId);
                                            var invoiceTotal = new InvoiceTotal { Id = 7, Name = tname.Name + '(' + tname.Rate + "%)" + " On Product", Amount = (decimal)salesTax.Amount };
                                            invoiceTotalList.Add(invoiceTotal);
                                        }
                                        pod.TaxPercent = (decimal)effectivetaxrate;
                                        pod.TaxAmount = (decimal)totalamount;
                                    }
                                    else
                                    {

                                        var salesTax = new SalesTax();
                                        salesTax.SalesInvoiceId = pomv.Id;
                                        salesTax.ItemId = podmv.ItemId;
                                        salesTax.TaxId = podmv.TaxId;
                                        salesTax.Amount = (decimal)taxrate * (decimal)compounded / 100;
                                        salesTax.CurrencyRate = pomv.Currencyrate;
                                        if (po.Status == "Saved")
                                        {
                                            db.SalesTaxes.Add(salesTax);
                                        }
                                        var tname = taxes.FirstOrDefault(t => t.TaxId == salesTax.TaxId);
                                        var invoiceTotal = new InvoiceTotal { Id = 7, Name = tname.Name + '(' + tname.Rate + "%)" + " On Product", Amount = (decimal)salesTax.Amount };
                                        invoiceTotalList.Add(invoiceTotal);

                                        pod.TaxPercent = (decimal)taxrate;
                                        pod.TaxAmount = (decimal)salesTax.Amount;

                                    }
                                    totaltaxonproduct += pod.TaxAmount;
                                    pod.CustDiscount = (decimal)(disP + fd1P + fd2P + fd3P + fd4P);
                                    pod.CustDiscountAmount = (decimal)(disL + fd1L + fd2L + fd3L + fd4L);
                                    pod.TotalAmount = (pod.Price * pod.Quantity) - (pod.DiscountAmount * pod.Quantity) - pod.CustDiscountAmount + pod.TaxAmount;
                                    db.SalesInvoiceDetails.Add(pod);
                                }

                            }

                            po.Dis = dis;
                            po.FD1 = fd1;
                            po.FD2 = fd2;
                            po.FD3 = fd3;
                            po.FD4 = fd4;
                            po.TaxProduct = totaltaxonproduct;
                            po.SubTotal = (decimal)subtotal;
                            if (subtotal > 0)
                            {
                                var invoiceTotal = new InvoiceTotal { Id = 1, Name = "Sub Total", Amount = (decimal)subtotal };
                                invoiceTotalList.Add(invoiceTotal);
                            }
                            if (dis > 0)
                            {
                                var invoiceTotal = new InvoiceTotal { Id = 2, Name = "Discount", Amount = (decimal)-dis };
                                invoiceTotalList.Add(invoiceTotal);
                            }
                            if (fd1 > 0)
                            {
                                var invoiceTotal = new InvoiceTotal { Id = 3, Name = "Further Discount 1", Amount = (decimal)-fd1 };
                                invoiceTotalList.Add(invoiceTotal);
                            }
                            if (fd2 > 0)
                            {
                                var invoiceTotal = new InvoiceTotal { Id = 4, Name = "Further Discount 2", Amount = (decimal)-fd2 };
                                invoiceTotalList.Add(invoiceTotal);
                            }
                            if (fd3 > 0)
                            {
                                var invoiceTotal = new InvoiceTotal { Id = 5, Name = "Further Discount 3", Amount = (decimal)-fd3 };
                                invoiceTotalList.Add(invoiceTotal);
                            }
                            if (fd4 > 0)
                            {
                                var invoiceTotal = new InvoiceTotal { Id = 6, Name = "Further Discount 4", Amount = (decimal)-fd4 };
                                invoiceTotalList.Add(invoiceTotal);
                            }

                            var pcdOldRecords = db.SalesCostingDetails.Where(p => p.SalesInvoiceId == po.Id).ToList();
                            foreach (var pcdOld in pcdOldRecords)
                            {
                                db.SalesCostingDetails.Remove(pcdOld);
                            }

                            decimal totaladdamount = 0;
                            decimal totaldeductamount = 0;
                            decimal totaltaxonother = 0;
                            foreach (var pcdmv in pcdmvList)
                            {
                                if (pcdmv.CostingId != 0 && pcdmv.TaxId > 0 && pcdmv.CostAmount > 0)
                                {
                                    decimal taxrate = taxes.FirstOrDefault(r => r.TaxId == pcdmv.TaxId).Rate;
                                    //var arr = pcdmv.TaxName.Split('-');
                                    //var taxamt = Convert.ToDecimal(arr[arr.Length - 1]);
                                    pcdmv.TaxPercent = taxrate;
                                    pcdmv.TaxAmount = taxrate * pcdmv.CostAmount / 100;

                                    SalesCostingDetail pcd = new SalesCostingDetail();
                                    pcd.SalesInvoiceId = po.Id;
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
                                                var crate = (effectiveTotal + pcdmv.CostAmount) * rate / 100;
                                                effectiveTotal += crate;
                                                parentEffectiveRate = crate;
                                                amount1 = parentEffectiveRate;
                                                totalamount += amount1;
                                            }

                                            var salesTax = new SalesTax();
                                            salesTax.SalesInvoiceId = pomv.Id;
                                            salesTax.CostId = pcdmv.CostingId;
                                            salesTax.TaxId = (long)taxComp.TaxCompId;
                                            salesTax.Amount = (decimal)amount1;
                                            salesTax.CurrencyRate = pomv.Currencyrate;

                                            if (po.Status == "Saved")
                                            {
                                                db.SalesTaxes.Add(salesTax);
                                            }
                                            var tname = taxes.FirstOrDefault(t => t.TaxId == salesTax.TaxId);
                                            var invoiceTotal = new InvoiceTotal { Id = 10, Name = tname.Name + '(' + tname.Rate + "%)" + " On Others", Amount = (decimal)salesTax.Amount };
                                            invoiceTotalList.Add(invoiceTotal);
                                        }
                                        pcd.TaxPercent = (decimal)effectivetaxrate;
                                        pcd.TaxAmount = (decimal)totalamount;
                                    }
                                    else
                                    {

                                        var salesTax = new SalesTax();
                                        salesTax.SalesInvoiceId = pomv.Id;
                                        salesTax.CostId = pcdmv.CostingId;
                                        salesTax.TaxId = pcdmv.TaxId;
                                        salesTax.Amount = (decimal)taxrate * (decimal)pcdmv.CostAmount / 100;
                                        salesTax.CurrencyRate = pomv.Currencyrate;

                                        if (po.Status == "Saved")
                                        {
                                            db.SalesTaxes.Add(salesTax);
                                        }

                                        pcd.TaxPercent = (decimal)taxrate;
                                        pcd.TaxAmount = (decimal)salesTax.Amount;

                                        var tname = taxes.FirstOrDefault(t => t.TaxId == salesTax.TaxId);
                                        var invoiceTotal = new InvoiceTotal { Id = 10, Name = tname.Name + '(' + tname.Rate + "%)" + " On Others", Amount = (decimal)salesTax.Amount };
                                        invoiceTotalList.Add(invoiceTotal);
                                    }
                                    totaltaxonother += pcd.TaxAmount;
                                    db.SalesCostingDetails.Add(pcd);
                                }

                            }
                            var invoiceTotal1 = new InvoiceTotal { Id = 8, Name = "Added Cost", Amount = totaladdamount };
                            invoiceTotalList.Add(invoiceTotal1);
                            var invoiceTotal2 = new InvoiceTotal { Id = 9, Name = "Deducted Cost", Amount = -totaldeductamount };
                            invoiceTotalList.Add(invoiceTotal2);
                            po.TotalAddAmount = totaladdamount;
                            po.TotalDeductAmount = totaldeductamount;
                            po.TaxOther = totaltaxonother;
                            decimal roundamount = 0;
                            decimal unroundedtotal = (decimal)(subtotal - (dis + fd1 + fd2 + fd3 + fd4) + totaltaxonproduct + totaladdamount - totaldeductamount + totaltaxonother);
                            //decimal roundedtotal = Math.Round(unroundedtotal);
                            //if (unroundedtotal != roundedtotal)
                            //{
                            //    roundamount = roundedtotal - unroundedtotal;
                            //    var invoiceTotal3 = new InvoiceTotal { Id = 11, Name = "Round Off", Amount = roundamount };
                            //    invoiceTotalList.Add(invoiceTotal3);

                            //}
                            //po.RoundOff = roundamount;
                            po.GrandTotal = unroundedtotal;
                            po.BCGrandTotal = Math.Round(unroundedtotal * pomv.Currencyrate, 2);
                            var invoiceTotal4 = new InvoiceTotal { Id = 12, Name = "Grand Total In " + pomv.TransactionCurrencyCode, Amount = po.GrandTotal };
                            invoiceTotalList.Add(invoiceTotal4);
                            if (pomv.CurrencyId != pomv.TransactionCurrency)
                            {

                                var invoiceTotal5 = new InvoiceTotal { Id = 13, Name = "Grand Total In " + pomv.BaseCurrencyCode, Amount = po.BCGrandTotal };
                                invoiceTotalList.Add(invoiceTotal5);
                            }


                            if (pomv.Mode == 2)
                            {
                                if (po.TransNo == null)
                                {
                                    tr: string TransNo = Convert.ToString(GenRandom.GetRandom());

                                    var trans = db.ReceiptPayments.Where(d => d.transactionNo == TransNo).Select(d => d.Id).FirstOrDefault();

                                    if (trans > 0)
                                    {
                                        goto tr;

                                    }
                                    var recpayment = new ReceiptPayment();

                                    recpayment.RPdate = invoicedate;
                                    recpayment.RPDatetime = DateTime.Now;

                                    recpayment.fYearId = Fyid;
                                    recpayment.BranchId = (int)Branchid;

                                    recpayment.CompanyId = (int)companyid;
                                    recpayment.UserId = (int)userid;

                                    if (po.BCGrandTotal > 0)
                                    {
                                        long CashId = db.LedgerMasters.Where(l => l.ledgerName == "CASH IN HAND").Select(l => l.LID).FirstOrDefault();

                                        recpayment.RPCashId = Convert.ToInt32(CashId);
                                        recpayment.RPCashAmount = po.BCGrandTotal;
                                    }
                                    else
                                    {

                                        recpayment.RPCashId = 0;
                                        recpayment.RPCashAmount = 0;


                                    }


                                    recpayment.transactionType = "Cash Invoice";
                                    recpayment.RPType = InventoryConst.Cns_Receive;
                                    int vouch = 1;
                                    string prefix = "CR/" + fyear + "/";
                                    if (db.ReceiptPayments.Where(d => (d.transactionType == "General Receive" || d.transactionType == "General Payment" || d.transactionType == "Cash Invoice") && d.CompanyId == companyid && d.BranchId == Branchid && d.UserId == userid && d.fYearId == Fyid && d.Prefix == prefix).FirstOrDefault() != null)
                                    {
                                        vouch = (db.ReceiptPayments.Where(d => (d.transactionType == "General Receive" || d.transactionType == "General Payment" || d.transactionType == "Cash Invoice") && d.CompanyId == companyid && d.BranchId == Branchid && d.UserId == userid && d.fYearId == Fyid && d.Prefix == prefix).Max(v => (int?)v.VoucherNo) ?? 0) + 1;

                                    }
                                    recpayment.VoucherNo = vouch;
                                    recpayment.Prefix = prefix;
                                    recpayment.TotalAmount = po.BCGrandTotal;
                                    if (po.CustomerId == null)
                                        recpayment.ledgerId = 59;
                                    else
                                        recpayment.ledgerId = (int)po.Customer.LId;
                                    recpayment.transactionNo = TransNo;
                                    recpayment.Remarks = "Cash Invoice " + po.NO;




                                    recpayment.CreatedBy = Createdby;

                                    db.ReceiptPayments.Add(recpayment);
                                    //Insert unique transaction no in cash invoice generated
                                    po.TransNo = TransNo;
                                }
                                else
                                {
                                    var recpayment = db.ReceiptPayments.Where(r => r.transactionNo == po.TransNo).FirstOrDefault();
                                    if (po.CustomerId == null)
                                        recpayment.ledgerId = 59;
                                    else
                                        recpayment.ledgerId = (int)po.Customer.LId;
                                    recpayment.RPCashAmount = po.BCGrandTotal;
                                    recpayment.TotalAmount = po.BCGrandTotal;
                                    recpayment.RPdate = invoicedate;
                                    if (po.BCGrandTotal > 0)
                                    {
                                        long CashId = db.LedgerMasters.Where(l => l.ledgerName == "CASH IN HAND").Select(l => l.LID).FirstOrDefault();

                                        recpayment.RPCashId = Convert.ToInt32(CashId);
                                        recpayment.RPCashAmount = po.BCGrandTotal;
                                    }
                                    else
                                    {
                                        recpayment.RPCashId = 0;
                                        recpayment.RPCashAmount = 0;
                                    }
                                    // recpayment.RPDatetime = date;
                                    recpayment.ModifiedBy = Createdby;
                                    recpayment.ModifiedOn = DateTime.Now;

                                }

                            }
                            else
                            {
                                if (po.TransNo != null)
                                {
                                    var recpayment = db.ReceiptPayments.Where(r => r.transactionNo == po.TransNo).FirstOrDefault();
                                    var billWisePayment = db.BillWiseReceives.Where(r => r.ReceiptPaymentId == recpayment.Id).ToList();
                                    if (billWisePayment.Count() >0)
                                    {
                                        foreach (var item in billWisePayment)
                                        {
                                            db.BillWiseReceives.Remove(item);
                                        }
                                    }
                                    if (recpayment != null)
                                    {
                                        db.ReceiptPayments.Remove(recpayment);
                                    }
                                    po.TransNo = null;
                                }
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
                var taxes1 = mc.getDdlTaxes(userid, companyid, Branchid);
                ViewBag.ddlTaxes = taxes1;
                ViewBag.podmvList = podmvList;
                ViewBag.pcdmvList = pcdmvList;
                ViewBag.ErrorTag = "Unable to save changes. Try again, and if the problem persists see your system administrator.";
                ViewBag.Message = "Error";
                return View(pomv);
            }
            //catch (DataException)
            //{
            //    //Log the error (add a variable name after DataException)
            //    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            //    var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
            //    ViewBag.ddlWarehouses = warehouses1;
            //    var taxes1 = mc.getDdlTaxes(userid, companyid, Branchid);
            //    ViewBag.ddlTaxes = taxes1;
            //    ViewBag.podmvList = podmvList;
            //    ViewBag.pcdmvList = pcdmvList;
            //    ViewBag.ErrorTag = "Unable to save changes. Try again, and if the problem persists see your system administrator.";
            //    ViewBag.Message = "Error";
            //    return View(pomv);
            //}
            catch (Exception exp)
            {
                var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                ViewBag.ddlWarehouses = warehouses1;
                var taxes1 = mc.getDdlTaxes(userid, companyid, Branchid);
                ViewBag.ddlTaxes = taxes1;
                ViewBag.podmvList = podmvList;
                ViewBag.pcdmvList = pcdmvList;
                ViewBag.ErrorTag = "Unable to save changes. Try again, and if the problem persists see your system administrator.";
                ViewBag.Message = "Error";
                return View(pomv);
            }


            var warehouses2 = mc.getDdlWarehouses(companyid, Branchid);
            ViewBag.ddlWarehouses = warehouses2;
            var taxes2 = mc.getDdlTaxes(userid, companyid, Branchid);
            ViewBag.ddlTaxes = taxes2;
            ViewBag.podmvList = podmvList;
            ViewBag.pcdmvList = pcdmvList;
            invoiceTotalList = invoiceTotalList.OrderBy(f => f.Id).ToList();
            var invoiceTotalList1 = invoiceTotalList.GroupBy(t => t.Name).Select(t => new { Name = t.Key, Total = t.Sum(l => l.Amount) }).ToList();
            ViewBag.InvoiceTotalList = invoiceTotalList1;
            //var invoiceTotal = new InvoiceTotal();
            //invoiceTotal.Id = 1;
            //invoiceTotal.Name = "Sub Total";
            //invoiceTotal.Amount=
            ViewBag.Message = "You have successfully " + pomv.Status + " Sales Invoice " + pomv.NO;
            return View(pomv);


        }


        //
        // GET: /SalesInvoice/Edit/5

        public ActionResult Edit(long id = 0)
        {
            SalesInvoice salesinvoice = db.SalesInvoices.Find(id);
            if (salesinvoice == null)
            {
                return HttpNotFound();
            }
            ViewBag.CurrencyId = new SelectList(db.Currencies, "CurrencyId", "Country", salesinvoice.CurrencyId);
            ViewBag.TransactionCurrency = new SelectList(db.Currencies, "CurrencyId", "Country", salesinvoice.TransactionCurrency);
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "Code", salesinvoice.CustomerId);
            //  ViewBag.TaxId = new SelectList(db.Taxes, "TaxId", "Name", salesinvoice.TaxId);
            ViewBag.WarehouseId = new SelectList(db.Warehouses, "Id", "Code", salesinvoice.WarehouseId);
            return View(salesinvoice);
        }

        //
        // POST: /SalesInvoice/Edit/5

        [HttpPost]
        public ActionResult Edit(SalesInvoice salesinvoice)
        {
            if (ModelState.IsValid)
            {
                db.Entry(salesinvoice).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CurrencyId = new SelectList(db.Currencies, "CurrencyId", "Country", salesinvoice.CurrencyId);
            ViewBag.TransactionCurrency = new SelectList(db.Currencies, "CurrencyId", "Country", salesinvoice.TransactionCurrency);
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "Code", salesinvoice.CustomerId);
            // ViewBag.TaxId = new SelectList(db.Taxes, "TaxId", "Name", salesinvoice.TaxId);
            ViewBag.WarehouseId = new SelectList(db.Warehouses, "Id", "Code", salesinvoice.WarehouseId);
            return View(salesinvoice);
        }

        //View the Invoice 

        public ActionResult Display(long? id = 0, string from = null)
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            var culture = Session["DateCulture"].ToString();
            var dateFormat = Session["DateFormat"].ToString();
            // System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            ViewBag.ddlWarehouses = warehouses;
            var taxes = db.Taxes.Where(d => (d.CompanyId == companyid && d.BranchId == Branchid) || (d.UserId == 0 && d.CompanyId == 0)).Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate }).ToList();
            ViewBag.TaxSingle = taxes;
            var salesPerson = db.SalesPersons.Where(d => d.UserId == userid && d.CompanyId == companyid).Select(d => new { Id = d.Id, Name = d.Name }).ToList();
            ViewBag.ddlSalesPerson = salesPerson;
            var paymentTerms = db.PaymentTerms.Where(d => (d.CompanyId == companyid && d.BranchId == Branchid) || (d.UserId == 0 && d.CompanyId == 0)).Select(d => new { Id = d.Id, PaymentTermDescription = d.PaymentTermDescription }).ToList();
            ViewBag.PaymentTerms = paymentTerms;
            var Taxcomponents = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid && d.Tax.BranchId == Branchid) || (d.Tax.CompanyId == 0 && d.Tax.BranchId == 0)).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            ViewBag.Taxcomponents = Taxcomponents;
            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid || d.UserId == 0 && d.CompanyId == 0).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();

            if (id == 0)
            {
                SalesInvoiceModelView pomv = new SalesInvoiceModelView();
                pomv.Date = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.DueDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.DespatchDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.CurrencyId = basecurrency.CurrencyId;
                pomv.BaseCurrencyCode = basecurrency.CurrencyCode;
                pomv.CompoundedDis = false;
                string countryname = string.Empty;
                return View(pomv);
            }
            else
            {
                if (from == null)
                {
                    SalesInvoice po = db.SalesInvoices.Find(id);
                    if (po == null)
                    {
                        return HttpNotFound();
                    }
                    SalesInvoiceModelView pomv = new SalesInvoiceModelView();
                    List<SalesInvoiceDetailModelView> podmvList = new List<SalesInvoiceDetailModelView>();
                    List<SalesCostingDetailModelView> pcdmvList = new List<SalesCostingDetailModelView>();
                    List<InvoiceTotal> invoiceTotalList = new List<InvoiceTotal>();
                    pomv.Id = po.Id;
                    pomv.NO = po.NO;
                    pomv.CustomerCode = po.Customer.Code;
                    pomv.CustomerName = po.Customer.Name;
                    pomv.CustomerId = po.CustomerId;
                    pomv.Reference = po.Reference;
                    pomv.SalesPerson = po.SalesPerson;
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
                    pomv.SalesPerson = po.SalesPerson;
                    pomv.CompoundedDis = po.CompoundedDis;
                    pomv.DisApplicable = po.DisApplicable;
                    pomv.FD1Applicable = po.FD1Applicable;
                    pomv.FD2Applicable = po.FD2Applicable;
                    pomv.FD3Applicable = po.FD3Applicable;
                    pomv.FD4Applicable = po.FD4Applicable;
                    pomv.DisAmount = po.DisAmount;
                    pomv.FD1Amount = po.FD1Amount;
                    pomv.FD2Amount = po.FD2Amount;
                    pomv.FD3Amount = po.FD3Amount;
                    pomv.FD4Amount = po.FD4Amount;
                    pomv.Dis = po.Dis;
                    pomv.FD1 = po.FD1;
                    pomv.FD2 = po.FD2;
                    pomv.FD3 = po.FD3;
                    pomv.FD4 = po.FD4;
                    pomv.SubTotal = po.SubTotal;
                    pomv.TaxProduct = po.TaxProduct;
                    pomv.TotalAddAmount = po.TotalAddAmount;
                    pomv.TotalDeductAmount = po.TotalDeductAmount;

                    pomv.TaxOther = po.TaxOther;
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
                    var podlist = db.SalesInvoiceDetails.Where(p => p.SalesInvoiceId == po.Id).ToList();
                    foreach (var pod in podlist)
                    {
                        var podmv = new SalesInvoiceDetailModelView();
                        podmv.SalesInvoiceId = pod.SalesInvoiceId;
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
                        podmv.Discount = pod.Discount;
                        podmv.DiscountAmount = pod.DiscountAmount;
                        podmv.TotalAmount = pod.Price * pod.Quantity - pod.DiscountAmount;

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
                    var pcdlist = db.SalesCostingDetails.Where(p => p.SalesInvoiceId == po.Id).ToList();
                    foreach (var pcd in pcdlist)
                    {
                        var pcdmv = new SalesCostingDetailModelView();
                        pcdmv.SalesInvoiceId = pcd.SalesInvoiceId;
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
                    if (pomv.Status == "Saved")
                    {
                        if (pomv.SubTotal > 0)
                        {
                            var invoiceTotal = new InvoiceTotal { Id = 1, Name = "Sub Total", Amount = pomv.SubTotal };
                            invoiceTotalList.Add(invoiceTotal);
                        }
                        if (pomv.Dis > 0)
                        {
                            var invoiceTotal = new InvoiceTotal { Id = 2, Name = "Discount", Amount = (decimal)-pomv.Dis };
                            invoiceTotalList.Add(invoiceTotal);
                        }
                        if (pomv.FD1 > 0)
                        {
                            var invoiceTotal = new InvoiceTotal { Id = 3, Name = "Further Discount 1", Amount = (decimal)-pomv.FD1 };
                            invoiceTotalList.Add(invoiceTotal);
                        }
                        if (pomv.FD2 > 0)
                        {
                            var invoiceTotal = new InvoiceTotal { Id = 4, Name = "Further Discount 2", Amount = (decimal)-pomv.FD2 };
                            invoiceTotalList.Add(invoiceTotal);
                        }
                        if (pomv.FD3 > 0)
                        {
                            var invoiceTotal = new InvoiceTotal { Id = 5, Name = "Further Discount 3", Amount = (decimal)-pomv.FD3 };
                            invoiceTotalList.Add(invoiceTotal);
                        }
                        if (pomv.FD4 > 0)
                        {
                            var invoiceTotal = new InvoiceTotal { Id = 6, Name = "Further Discount 4", Amount = (decimal)-pomv.FD4 };
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
                        var alltaxes = db.SalesTaxes.Where(s => s.SalesInvoiceId == pomv.Id).ToList();
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
                else if (from == "SO")
                {
                    SalesOrder po = db.SalesOrders.Find(id);
                    if (po == null)
                    {
                        return HttpNotFound();
                    }
                    SalesInvoiceModelView pomv = new SalesInvoiceModelView();
                    List<SalesInvoiceDetailModelView> podmvList = new List<SalesInvoiceDetailModelView>();

                    pomv.Id = 0;
                    pomv.CreatedFrom = "Sales Order";
                    pomv.ReferenceNo = po.Id;
                    pomv.OrderNo = po.NO;
                    pomv.CustomerCode = po.Customer.Code;
                    pomv.CustomerName = po.Customer.Name;
                    pomv.CustomerId = po.CustomerId;
                    pomv.Reference = po.Reference;
                    pomv.SalesPerson = po.SalesPerson;
                    pomv.WarehouseId = po.WarehouseId;
                    pomv.CurrencyId = po.CurrencyId;
                    pomv.BaseCurrencyCode = po.Currency.ISO_4217;
                    pomv.Currencyrate = po.Currencyrate;
                    pomv.TransactionCurrency = po.TransactionCurrency;
                    pomv.TransactionCurrencyCode = po.Currency1.ISO_4217;
                    pomv.Date = po.Date.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DueDate = po.DueDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DespatchDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.InvoiceDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DeliveryName = po.DeliveryName;
                    pomv.StreetPoBox = po.StreetPoBox;
                    pomv.Suburb = po.Suburb;
                    pomv.City = po.City;
                    pomv.StateRegion = po.StateRegion;
                    pomv.Country = po.Country;
                    pomv.PostalCode = po.PostalCode;
                    pomv.SalesPerson = po.SalesPerson;
                    pomv.CompoundedDis = false;
                    pomv.DisApplicable = false;
                    pomv.FD1Applicable = false;
                    pomv.FD2Applicable = false;
                    pomv.FD3Applicable = false;
                    pomv.FD4Applicable = false;
                    pomv.DisAmount = 0;
                    pomv.FD1Amount = 0;
                    pomv.FD2Amount = 0;
                    pomv.FD3Amount = 0;
                    pomv.FD4Amount = 0;
                    pomv.Dis = 0;
                    pomv.FD1 = 0;
                    pomv.FD2 = 0;
                    pomv.FD3 = 0;
                    pomv.FD4 = 0;
                    pomv.SubTotal = 0;
                    pomv.TaxProduct = 0;
                    pomv.TotalAddAmount = 0;
                    pomv.TotalDeductAmount = 0;

                    pomv.TaxOther = 0;
                    pomv.GrandTotal = 0;
                    pomv.BCGrandTotal = 0;
                    pomv.UserId = po.UserId;
                    pomv.BranchId = po.BranchId;
                    pomv.CompanyId = po.CompanyId;
                    //pomv.Status = po.Status;
                    //pomv.CreatedBy = po.CreatedBy;
                    //pomv.CreatedOn = po.CreatedOn;
                    //pomv.ModifiedBy = po.ModifiedBy;
                    //pomv.ModifiedOn = po.ModifiedOn;
                    pomv.Memo = po.Memo;
                    var podlist = db.SalesOrderDetails.Where(p => p.SalesOrderId == po.Id).ToList();
                    foreach (var pod in podlist)
                    {
                        var podmv = new SalesInvoiceDetailModelView();
                        podmv.SalesInvoiceId = pod.SalesOrderId;
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
                        podmv.Discount = pod.Discount;
                        podmv.DiscountAmount = pod.DiscountAmount;
                        podmv.TotalAmount = pod.Price * pod.Quantity - pod.DiscountAmount;
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
                    return View(pomv);
                }
                else
                {
                    SalesDelivery po = db.SalesDeliveries.Find(id);
                    if (po == null)
                    {
                        return HttpNotFound();
                    }
                    SalesInvoiceModelView pomv = new SalesInvoiceModelView();
                    List<SalesInvoiceDetailModelView> podmvList = new List<SalesInvoiceDetailModelView>();

                    pomv.Id = 0;
                    pomv.CreatedFrom = "Delivery Note";
                    pomv.ReferenceNo = po.Id;
                    pomv.OrderNo = po.NO;
                    pomv.CustomerCode = po.Customer.Code;
                    pomv.CustomerName = po.Customer.Name;
                    pomv.CustomerId = po.CustomerId;
                    pomv.Reference = po.Reference;
                    pomv.SalesPerson = po.SalesPerson;
                    pomv.WarehouseId = po.WarehouseId;
                    pomv.CurrencyId = po.CurrencyId;
                    pomv.BaseCurrencyCode = po.Currency.ISO_4217;
                    pomv.Currencyrate = po.Currencyrate;
                    pomv.TransactionCurrency = po.TransactionCurrency;
                    pomv.TransactionCurrencyCode = po.Currency1.ISO_4217;
                    pomv.Date = po.Date.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DueDate = po.DueDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DespatchDate = po.DespatchDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.InvoiceDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DeliveryName = po.DeliveryName;
                    pomv.StreetPoBox = po.StreetPoBox;
                    pomv.Suburb = po.Suburb;
                    pomv.City = po.City;
                    pomv.StateRegion = po.StateRegion;
                    pomv.Country = po.Country;
                    pomv.PostalCode = po.PostalCode;
                    pomv.SalesPerson = po.SalesPerson;
                    pomv.CompoundedDis = false;
                    pomv.DisApplicable = false;
                    pomv.FD1Applicable = false;
                    pomv.FD2Applicable = false;
                    pomv.FD3Applicable = false;
                    pomv.FD4Applicable = false;
                    pomv.DisAmount = 0;
                    pomv.FD1Amount = 0;
                    pomv.FD2Amount = 0;
                    pomv.FD3Amount = 0;
                    pomv.FD4Amount = 0;
                    pomv.Dis = 0;
                    pomv.FD1 = 0;
                    pomv.FD2 = 0;
                    pomv.FD3 = 0;
                    pomv.FD4 = 0;
                    pomv.SubTotal = 0;
                    pomv.TaxProduct = 0;
                    pomv.TotalAddAmount = 0;
                    pomv.TotalDeductAmount = 0;

                    pomv.TaxOther = 0;
                    pomv.GrandTotal = 0;
                    pomv.BCGrandTotal = 0;
                    pomv.UserId = po.UserId;
                    pomv.BranchId = po.BranchId;
                    pomv.CompanyId = po.CompanyId;
                    //pomv.Status = po.Status;
                    //pomv.CreatedBy = po.CreatedBy;
                    //pomv.CreatedOn = po.CreatedOn;
                    //pomv.ModifiedBy = po.ModifiedBy;
                    //pomv.ModifiedOn = po.ModifiedOn;
                    pomv.Memo = po.Memo;
                    var podlist = db.SalesDeliveryDetails.Where(p => p.SalesDeliveryId == po.Id).ToList();
                    var defaulttax = db.Taxes.FirstOrDefault(t => t.BranchId == 0 && t.CompanyId == 0);
                    foreach (var pod in podlist)
                    {
                        var podmv = new SalesInvoiceDetailModelView();
                        podmv.SalesInvoiceId = pod.SalesDeliveryId;
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
                        podmv.Discount = pod.Discount;
                        podmv.DiscountAmount = pod.DiscountAmount;
                        podmv.TotalAmount = pod.Price * pod.Quantity - pod.DiscountAmount;
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
                    return View(pomv);
                }
            }

        }
        //
        // GET: /SalesInvoice/Delete/5

        public ActionResult Delete(long id = 0)
        {
            SalesInvoice salesinvoice = db.SalesInvoices.Find(id);
            if (salesinvoice == null)
            {
                return HttpNotFound();
            }
            return View(salesinvoice);
        }

        //
        // POST: /SalesInvoice/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            SalesInvoice salesinvoice = db.SalesInvoices.Find(id);
            var salesinvoiceDetails = db.SalesInvoiceDetails.Where(d => d.SalesInvoiceId == salesinvoice.Id).ToList();
            var salescostingDetails = db.SalesCostingDetails.Where(d => d.SalesInvoiceId == salesinvoice.Id).ToList();
            var salesinvoiceTaxes = db.SalesTaxes.Where(d => d.SalesInvoiceId == salesinvoice.Id).ToList();
           
            foreach (var det in salesinvoiceDetails)
            {
                db.SalesInvoiceDetails.Remove(det);
            }
            foreach (var cost in salescostingDetails)
            {
                db.SalesCostingDetails.Remove(cost);
            }
            foreach (var tax in salesinvoiceTaxes)
            {
                db.SalesTaxes.Remove(tax);
            }
            db.SalesInvoices.Remove(salesinvoice);
            db.SaveChanges();
            return RedirectToAction("Index");
        }



        [HttpPost]
        public ActionResult CalculateTax(List<TaxComponent> taxComs)
        {
            List<TaxComponent_Ret> RetList = new List<TaxComponent_Ret>();
            var taxcomponents = db.Taxrates.ToList();
            foreach (var taxCom in taxComs)
            {
                var comps = taxcomponents.Where(c => c.TaxId == taxCom.TaxId);
                foreach (var comp in comps)
                {
                    var Ret = new TaxComponent_Ret();

                    Ret.TaxName = comp.TaxName;
                    Ret.Amount = (decimal)comp.Taxrate1 * taxCom.Amount;
                    RetList.Add(Ret);

                }
            }
            return Json(RetList.GroupBy(r => r.TaxName).Select(rg => new { TaxName = rg.Key, Amount = rg.Sum(rd => rd.Amount) }), JsonRequestBehavior.AllowGet);
        }

        #region PDF Email



        public ActionResult CreteSalesInvoicePDF(long? id, long? Branchid, long? companyid, long? userid)
        {
            //long Branchid = Convert.ToInt64(Session["BranchId"]);
            //long companyid = Convert.ToInt64(Session["companyid"]);
            //long userid = Convert.ToInt32(Session["userid"]);
            var culture = Convert.ToString(Session["DateCulture"]);
            var dateFormat = Convert.ToString(Session["DateFormat"]);
            SalesInvoiceModelView pomv = new SalesInvoiceModelView();
            List<SalesInvoiceDetailModelView> podmvList = new List<SalesInvoiceDetailModelView>();
            List<SalesCostingDetailModelView> pcdmvList = new List<SalesCostingDetailModelView>();
            List<InvoiceTotal> invoiceTotalList = new List<InvoiceTotal>();






            var taxes = db.Taxes.Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate }).ToList();
            ViewBag.TaxSingle = taxes;

            var Taxcomponents = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid && d.Tax.BranchId == Branchid) || (d.Tax.CompanyId == 0 && d.Tax.BranchId == 0)).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            ViewBag.Taxcomponents = Taxcomponents;

            SalesInvoice po = db.SalesInvoices.Find(id);
            if (po == null)
            {
                return HttpNotFound();
            }

            var numtowords = new NumberToEnglish();
            ViewBag.BCTotalAmount = numtowords.changeNumericToWords(po.BCGrandTotal);

            var customer = db.Customers.Where(r => r.Id == po.CustomerId).Select(s => s.Code).FirstOrDefault();
            ViewBag.customer = customer;
            
            var company = db.Companies.Where(c => c.Id == po.CompanyId).FirstOrDefault();
            // var companyname = db.Companies.Where(c => c.Id == comids).FirstOrDefault();
            ViewBag.company = company;

            ViewBag.companyName = company.Name;
            ViewBag.address = company.Address;

            if (po.PaymentTermId != null)
            {
                var paymentterm = db.PaymentTerms.FirstOrDefault(c => c.Id == po.PaymentTermId).PaymentTermDescription;
                ViewBag.PaymentTerm = paymentterm;
            }





            List<CustomerwiseInvoice> inv = new List<CustomerwiseInvoice>();



            //var logo = db.BusinessPartners.Where(r => r.CompanyId == companyid).Select(s => s.Logo).ToList();
            //ViewBag.Logo = logo; 


            var podlist = db.SalesInvoiceDetails.Where(p => p.SalesInvoiceId == po.Id).ToList();
            foreach (var pod in podlist)
            {
                var podmv = new SalesInvoiceDetailModelView();
                podmv.SalesInvoiceId = pod.SalesInvoiceId;
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
                podmv.Discount = pod.Discount;
                podmv.DiscountAmount = pod.CustDiscountAmount + pod.Discount;
                podmv.TotalAmount = pod.Price * pod.Quantity - podmv.DiscountAmount;
                podmv.SubTotal = pod.Price * pod.Quantity - podmv.DiscountAmount;

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
            //SalesInvoiceDetailModelView model = new SalesInvoiceDetailModelView();
            //{
            //    model.SubTotal = podmvList.Sum(r => (decimal?)r.SubTotal) ?? 0;
            //    model.TotalTaxAmount = podmvList.Sum(r => (decimal?)r.TaxAmount) ?? 0;
            //    model.TotalDiscountAmount = podmvList.Sum(r => (decimal?)r.DiscountAmount) ?? 0;
            //    model.TotalAmount = (model.SubTotal + model.TotalTaxAmount) - model.TotalDiscountAmount;
            //    ViewBag.subtotal = model.SubTotal;
            //    ViewBag.TotalTaxAmount = model.TotalTaxAmount;
            //    ViewBag.TotalDiscountAmount = model.TotalDiscountAmount;
            //    ViewBag.TotalAmount = model.TotalAmount;
            //}

            if (podmvList.FirstOrDefault().TaxAmount>0)
            {
                po.TaxProduct = 1;
            }
            ViewBag.podmvList = podmvList;


            var pcdlist = db.SalesCostingDetails.Where(p => p.SalesInvoiceId == po.Id).ToList();
            foreach (var pcd in pcdlist)
            {
                var pcdmv = new SalesCostingDetailModelView();
                pcdmv.SalesInvoiceId = pcd.SalesInvoiceId;
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
                if (pomv.Dis > 0)
                {
                    var invoiceTotal = new InvoiceTotal { Id = 2, Name = "Discount", Amount = (decimal)-po.Dis };
                    invoiceTotalList.Add(invoiceTotal);
                }
                if (pomv.FD1 > 0)
                {
                    var invoiceTotal = new InvoiceTotal { Id = 3, Name = "Further Discount 1", Amount = (decimal)-po.FD1 };
                    invoiceTotalList.Add(invoiceTotal);
                }
                if (pomv.FD2 > 0)
                {
                    var invoiceTotal = new InvoiceTotal { Id = 4, Name = "Further Discount 2", Amount = (decimal)-po.FD2 };
                    invoiceTotalList.Add(invoiceTotal);
                }
                if (pomv.FD3 > 0)
                {
                    var invoiceTotal = new InvoiceTotal { Id = 5, Name = "Further Discount 3", Amount = (decimal)-po.FD3 };
                    invoiceTotalList.Add(invoiceTotal);
                }
                if (pomv.FD4 > 0)
                {
                    var invoiceTotal = new InvoiceTotal { Id = 6, Name = "Further Discount 4", Amount = (decimal)-po.FD4 };
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



            return View(po);


        }

        public ActionResult CreteSalesInvoicePDF1(long? id, long? Branchid, long? companyid, long? userid)
        {
            //long Branchid = Convert.ToInt64(Session["BranchId"]);
            //long companyid = Convert.ToInt64(Session["companyid"]);
            //long userid = Convert.ToInt32(Session["userid"]);
            var culture = Convert.ToString(Session["DateCulture"]);
            var dateFormat = Convert.ToString(Session["DateFormat"]);
            SalesInvoiceModelView pomv = new SalesInvoiceModelView();
            List<SalesInvoiceDetailModelView> podmvList = new List<SalesInvoiceDetailModelView>();
            List<SalesCostingDetailModelView> pcdmvList = new List<SalesCostingDetailModelView>();
            List<InvoiceTotal> invoiceTotalList = new List<InvoiceTotal>();






            var taxes = db.Taxes.Where(d => (d.CompanyId == companyid && d.BranchId == Branchid) || (d.UserId == 0 && d.CompanyId == 0)).Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate }).ToList();
            ViewBag.TaxSingle = taxes;

            var Taxcomponents = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid && d.Tax.BranchId == Branchid) || (d.Tax.CompanyId == 0 && d.Tax.BranchId == 0)).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            ViewBag.Taxcomponents = Taxcomponents;

            SalesInvoice po = db.SalesInvoices.Find(id);
            if (po == null)
            {
                return HttpNotFound();
            }

            var numtowords = new NumberToEnglish();
            ViewBag.BCTotalAmount = numtowords.changeNumericToWords(po.BCGrandTotal);

            var customer = db.Customers.Where(r => r.Id == po.CustomerId).Select(s => s.Code).FirstOrDefault();
            ViewBag.customer = customer;
            var company = db.Companies.Where(c => c.Id == po.CompanyId).FirstOrDefault();
            // var companyname = db.Companies.Where(c => c.Id == comids).FirstOrDefault();
            ViewBag.company = company;

            ViewBag.companyName = company.Name;
            ViewBag.address = company.Address;

            if (po.PaymentTermId != null)
            {
                var paymentterm = db.PaymentTerms.FirstOrDefault(c => c.Id == po.PaymentTermId).PaymentTermDescription;
                ViewBag.PaymentTerm = paymentterm;
            }





            List<CustomerwiseInvoice> inv = new List<CustomerwiseInvoice>();



            //var logo = db.BusinessPartners.Where(r => r.CompanyId == companyid).Select(s => s.Logo).ToList();
            //ViewBag.Logo = logo; 


            var podlist = db.SalesInvoiceDetails.Where(p => p.SalesInvoiceId == po.Id).ToList();
            foreach (var pod in podlist)
            {
                var podmv = new SalesInvoiceDetailModelView();
                podmv.SalesInvoiceId = pod.SalesInvoiceId;
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
                podmv.Discount = pod.Discount;
                podmv.DiscountAmount = pod.CustDiscountAmount + pod.Discount;
                podmv.TotalAmount = pod.Price * pod.Quantity - podmv.DiscountAmount;
                podmv.SubTotal = pod.Price * pod.Quantity - podmv.DiscountAmount;

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
            //SalesInvoiceDetailModelView model = new SalesInvoiceDetailModelView();
            //{
            //    model.SubTotal = podmvList.Sum(r => (decimal?)r.SubTotal) ?? 0;
            //    model.TotalTaxAmount = podmvList.Sum(r => (decimal?)r.TaxAmount) ?? 0;
            //    model.TotalDiscountAmount = podmvList.Sum(r => (decimal?)r.DiscountAmount) ?? 0;
            //    model.TotalAmount = (model.SubTotal + model.TotalTaxAmount) - model.TotalDiscountAmount;
            //    ViewBag.subtotal = model.SubTotal;
            //    ViewBag.TotalTaxAmount = model.TotalTaxAmount;
            //    ViewBag.TotalDiscountAmount = model.TotalDiscountAmount;
            //    ViewBag.TotalAmount = model.TotalAmount;
            //}
            ViewBag.podmvList = podmvList;


            var pcdlist = db.SalesCostingDetails.Where(p => p.SalesInvoiceId == po.Id).ToList();
            foreach (var pcd in pcdlist)
            {
                var pcdmv = new SalesCostingDetailModelView();
                pcdmv.SalesInvoiceId = pcd.SalesInvoiceId;
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
                if (pomv.Dis > 0)
                {
                    var invoiceTotal = new InvoiceTotal { Id = 2, Name = "Discount", Amount = (decimal)-po.Dis };
                    invoiceTotalList.Add(invoiceTotal);
                }
                if (pomv.FD1 > 0)
                {
                    var invoiceTotal = new InvoiceTotal { Id = 3, Name = "Further Discount 1", Amount = (decimal)-po.FD1 };
                    invoiceTotalList.Add(invoiceTotal);
                }
                if (pomv.FD2 > 0)
                {
                    var invoiceTotal = new InvoiceTotal { Id = 4, Name = "Further Discount 2", Amount = (decimal)-po.FD2 };
                    invoiceTotalList.Add(invoiceTotal);
                }
                if (pomv.FD3 > 0)
                {
                    var invoiceTotal = new InvoiceTotal { Id = 5, Name = "Further Discount 3", Amount = (decimal)-po.FD3 };
                    invoiceTotalList.Add(invoiceTotal);
                }
                if (pomv.FD4 > 0)
                {
                    var invoiceTotal = new InvoiceTotal { Id = 6, Name = "Further Discount 4", Amount = (decimal)-po.FD4 };
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
                        //   invoiceTotalList.Add(invoiceTotal);
                    }
                    else
                    {
                        var tname = taxes.FirstOrDefault(t => t.TaxId == individualtax.TaxId);
                        var invoiceTotal = new InvoiceTotal { Id = 10, Name = tname.Name + '(' + tname.Rate + "%)" + " On Others", Amount = individualtax.Amount };
                        //   invoiceTotalList.Add(invoiceTotal);
                    }
                }
            }
            invoiceTotalList = invoiceTotalList.OrderBy(f => f.Id).ToList();
            var invoiceTotalList1 = invoiceTotalList.GroupBy(t => t.Name).Select(t => new InvoiceTotal { Name = t.Key, Amount = t.Sum(l => l.Amount) }).ToList();
            ViewBag.InvoiceTotalList = invoiceTotalList1;



            return View(po);


        }



        public ActionResult PrintSalesInvoicePDF(long? id)
        {
            //return RedirectToAction("CreteSalesInvoicePDF", new { id = id });
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);

            return new ActionAsPdf("CreteSalesInvoicePDF", new { id = id, Branchid = Branchid, companyid = companyid, userid = userid }) { FileName = "SalesInvoicePrint.pdf" };
        }

        public ActionResult PrintSalesInvoicePDF1(long? id)
        {
            //return RedirectToAction("CreteSalesInvoicePDF", new { id = id });
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);

            return new ActionAsPdf("CreteSalesInvoicePDF1", new { id = id, Branchid = Branchid, companyid = companyid, userid = userid }) { FileName = "SalesInvoicePrint.pdf" };
        }



        public ActionResult PrintBillOfSupply(long? id)
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            var culture = Convert.ToString(Session["DateCulture"]);
            var dateFormat = Convert.ToString(Session["DateFormat"]);
            SalesInvoiceModelView pomv = new SalesInvoiceModelView();
            List<SalesInvoiceDetailModelView> podmvList = new List<SalesInvoiceDetailModelView>();
            List<SalesCostingDetailModelView> pcdmvList = new List<SalesCostingDetailModelView>();
            List<InvoiceTotal> invoiceTotalList = new List<InvoiceTotal>();






            var taxes = db.Taxes.Where(d => (d.CompanyId == companyid && d.BranchId == Branchid) || (d.UserId == 0 && d.CompanyId == 0)).Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate }).ToList();
            ViewBag.TaxSingle = taxes;

            var Taxcomponents = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid && d.Tax.BranchId == Branchid) || (d.Tax.CompanyId == 0 && d.Tax.BranchId == 0)).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            ViewBag.Taxcomponents = Taxcomponents;

            SalesInvoice po = db.SalesInvoices.Find(id);
            if (po == null)
            {
                return HttpNotFound();
            }

            var numtowords = new NumberToEnglish();
            ViewBag.BCTotalAmount = numtowords.changeNumericToWords(po.BCGrandTotal);

            var customer = db.Customers.Where(r => r.Id == po.CustomerId).FirstOrDefault();
            ViewBag.customer = customer.Name;
            ViewBag.address = customer.PoAddressName;
            var company = db.Companies.Where(c => c.Id == po.CompanyId).FirstOrDefault();
            // var companyname = db.Companies.Where(c => c.Id == comids).FirstOrDefault();
            ViewBag.company = company;

            ViewBag.companyName = company.Name;


            if (po.PaymentTermId != null)
            {
                var paymentterm = db.PaymentTerms.FirstOrDefault(c => c.Id == po.PaymentTermId).PaymentTermDescription;
                ViewBag.PaymentTerm = paymentterm;
            }





            List<CustomerwiseInvoice> inv = new List<CustomerwiseInvoice>();



            //var logo = db.BusinessPartners.Where(r => r.CompanyId == companyid).Select(s => s.Logo).ToList();
            //ViewBag.Logo = logo; 


            var podlist = db.SalesInvoiceDetails.Where(p => p.SalesInvoiceId == po.Id).ToList();
            foreach (var pod in podlist)
            {
                var podmv = new SalesInvoiceDetailModelView();
                podmv.SalesInvoiceId = pod.SalesInvoiceId;
                podmv.ItemId = pod.ItemId;
                podmv.ItemName = pod.Product.Name;
                podmv.HsnCode = pod.Product.HSNCode == null ? "----" : pod.Product.HSNCode;
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
                podmv.Discount = pod.Discount;
                podmv.DiscountAmount = pod.CustDiscountAmount + pod.Discount;
                podmv.TotalAmount = pod.Price * pod.Quantity - podmv.DiscountAmount;
                podmv.SubTotal = pod.Price * pod.Quantity - podmv.DiscountAmount;

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
            //SalesInvoiceDetailModelView model = new SalesInvoiceDetailModelView();
            //{
            //    model.SubTotal = podmvList.Sum(r => (decimal?)r.SubTotal) ?? 0;
            //    model.TotalTaxAmount = podmvList.Sum(r => (decimal?)r.TaxAmount) ?? 0;
            //    model.TotalDiscountAmount = podmvList.Sum(r => (decimal?)r.DiscountAmount) ?? 0;
            //    model.TotalAmount = (model.SubTotal + model.TotalTaxAmount) - model.TotalDiscountAmount;
            //    ViewBag.subtotal = model.SubTotal;
            //    ViewBag.TotalTaxAmount = model.TotalTaxAmount;
            //    ViewBag.TotalDiscountAmount = model.TotalDiscountAmount;
            //    ViewBag.TotalAmount = model.TotalAmount;
            //}
            ViewBag.podmvList = podmvList;


            var pcdlist = db.SalesCostingDetails.Where(p => p.SalesInvoiceId == po.Id).ToList();
            foreach (var pcd in pcdlist)
            {
                var pcdmv = new SalesCostingDetailModelView();
                pcdmv.SalesInvoiceId = pcd.SalesInvoiceId;
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
                if (pomv.Dis > 0)
                {
                    var invoiceTotal = new InvoiceTotal { Id = 2, Name = "Discount", Amount = (decimal)-po.Dis };
                    invoiceTotalList.Add(invoiceTotal);
                }
                if (pomv.FD1 > 0)
                {
                    var invoiceTotal = new InvoiceTotal { Id = 3, Name = "Further Discount 1", Amount = (decimal)-po.FD1 };
                    invoiceTotalList.Add(invoiceTotal);
                }
                if (pomv.FD2 > 0)
                {
                    var invoiceTotal = new InvoiceTotal { Id = 4, Name = "Further Discount 2", Amount = (decimal)-po.FD2 };
                    invoiceTotalList.Add(invoiceTotal);
                }
                if (pomv.FD3 > 0)
                {
                    var invoiceTotal = new InvoiceTotal { Id = 5, Name = "Further Discount 3", Amount = (decimal)-po.FD3 };
                    invoiceTotalList.Add(invoiceTotal);
                }
                if (pomv.FD4 > 0)
                {
                    var invoiceTotal = new InvoiceTotal { Id = 6, Name = "Further Discount 4", Amount = (decimal)-po.FD4 };
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



            return View(po);
        }





        #endregion

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
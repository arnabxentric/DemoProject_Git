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
    public class PurchaseReceiptController : Controller
    {
        private InventoryEntities db = new InventoryEntities();
        private MasterClasses mc = new MasterClasses();
        private TransactionClasses tc = new TransactionClasses();
        //
        // GET: /PurchaseReceipt/
        [SessionExpire]
        public ActionResult Index()
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            ViewBag.BaseCurrency = basecurrency.CurrencyCode;
            if (Branchid == 0)
                return View(db.PurchaseReceipts.Where(p => p.CompanyId == companyid && p.FinancialYearId == Fyid).OrderByDescending(d => d.ReceiptDate).ThenByDescending(d => d.InvoiceNo).Take(100).OrderBy(d => d.ReceiptDate).ThenBy(d => d.InvoiceNo).ToList());
            else
                return View(db.PurchaseReceipts.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.FinancialYearId == Fyid).OrderByDescending(d => d.ReceiptDate).ThenByDescending(d => d.InvoiceNo).Take(100).OrderBy(d => d.ReceiptDate).ThenBy(d => d.InvoiceNo).ToList());
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
                return View(db.PurchaseReceipts.Where(p => p.CompanyId == companyid && p.FinancialYearId == Fyid && p.ReceiptDate >= DtFrm && p.ReceiptDate <= DtTo).OrderBy(d => d.ReceiptDate).ThenBy(d => d.InvoiceNo).ToList());
            else
                return View(db.PurchaseReceipts.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.FinancialYearId == Fyid && p.ReceiptDate >= DtFrm && p.ReceiptDate <= DtTo).OrderBy(d => d.ReceiptDate).ThenBy(d => d.InvoiceNo).ToList());
        }
        //
        // GET: /PurchaseReceipt/Details/5
        [SessionExpire]
        public ActionResult Details(long id = 0)
        {
            PurchaseReceipt purchasereceipt = db.PurchaseReceipts.Find(id);
            if (purchasereceipt == null)
            {
                return HttpNotFound();
            }
            return View(purchasereceipt);
        }

        //
        // GET: /PurchaseReceipt/Create
        [SessionExpire]
        public ActionResult Create(long? id = 0,string from=null)
        {
             

            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            var dateFormat = Session["DateFormat"].ToString();
            var culture = Session["DateCulture"].ToString();
     //       System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var basecurrency = db.Companies.Where(c=>c.Id==companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            ViewBag.ddlWarehouses = warehouses;
            var paymentTerms = db.PaymentTerms.Where(d => (d.CompanyId == companyid )).Select(d => new { Id = d.Id, PaymentTermDescription = d.PaymentTermDescription }).ToList();
            ViewBag.PaymentTerms = paymentTerms;

            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid ).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.ReferencePO = GetYetToBeReceivedPO();


            if (id == 0)
            {
                PurchaseReceiptModelView pomv = new PurchaseReceiptModelView();
                pomv.Date = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.ReceiptDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
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
                ViewBag.AdressName = defaultwarehouse.ContactName;
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
                    PurchaseReceipt po = db.PurchaseReceipts.Find(id);
                    if (po == null)
                    {
                        return HttpNotFound();
                    }
                    PurchaseReceiptModelView pomv = new PurchaseReceiptModelView();
                    List<PurchaseReceiptDetailModelView> podmvList = new List<PurchaseReceiptDetailModelView>();

                    pomv.Id = po.Id;
                    pomv.NO = po.NO;
                    pomv.ReferenceChallan = po.ReferenceChallan;
                    pomv.CreatedFrom = po.CreatedFrom;
                    pomv.ReferenceNo = po.ReferenceNo;
                    pomv.OrderNo = po.OrderNo;
                    pomv.SupplierCode = po.Supplier.Code;
                    pomv.SupplierName = po.Supplier.Name;
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
                    pomv.ReceiptDate = po.ReceiptDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DeliveryName = po.DeliveryName;
                    pomv.StreetPoBox = po.StreetPoBox;
                    pomv.Suburb = po.Suburb;
                    pomv.City = po.City;
                    pomv.StateRegion = po.StateRegion;
                    pomv.Country = po.Country;
                    pomv.PostalCode = po.PostalCode;
                    pomv.LorryNo = po.LorryNo;
                    pomv.TagNo = po.TagNo;
                    pomv.PaymentTermId = po.PaymentTermId;
                   // pomv.TotalAmount = po.TotalAmount;
                    pomv.TotalAmount = 0;
                   // pomv.GrandTotal = po.GrandTotal;
                  //  pomv.BCGrandTotal = po.BCGrandTotal;
                    pomv.GrandTotal = 0;
                    pomv.BCGrandTotal = 0;
                    pomv.UserId = po.UserId;
                    pomv.BranchId = po.BranchId;
                    pomv.CompanyId = po.CompanyId;
                    pomv.Status = po.Status;
                    pomv.CreatedBy = po.CreatedBy;
                    pomv.CreatedOn = po.CreatedOn;
                    pomv.ModifiedBy = po.ModifiedBy;
                    pomv.ModifiedOn = po.ModifiedOn;

                    var podlist = db.PurchaseReceiptDetails.Where(p => p.PurchaseReceiptId == po.Id).ToList();
                    foreach (var pod in podlist)
                    {
                        var podmv = new PurchaseReceiptDetailModelView();
                        podmv.PurchaseReceiptId = pod.PurchaseReceiptId;
                        podmv.ItemId = pod.ItemId;
                        podmv.ItemCode = pod.Product.Code;
                        podmv.ItemName = pod.Product.Name;
                        podmv.BarCode = pod.BarCode;
                        podmv.Description = pod.Description;
                        podmv.Quantity = pod.Quantity;
                        podmv.AccountId = pod.AccountId;
                        podmv.UnitId = pod.UnitId;
                        podmv.UnitName = pod.UOM.Code;
                      //  podmv.Price = pod.Price;
                        podmv.Price = 0;
                        podmv.CurrencyRate = pod.CurrencyRate;

                       // podmv.TotalAmount = pod.TotalAmount;
                        podmv.TotalAmount =0;
                        podmv.UnitIdSecondary = pod.UnitIdSecondary;
                        podmv.UnitSecondaryName = pod.UOM1.Code;
                        podmv.SecUnitId = pod.SecUnitId;
                        if (podmv.SecUnitId != null)
                            podmv.SecUnitName = pod.UOM2.Code;
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
                else if (from == "PO")
                {
                    PurchaseOrder po = db.PurchaseOrders.Find(id);
                    if (po == null)
                    {
                        return HttpNotFound();
                    }
                    PurchaseReceiptModelView pomv = new PurchaseReceiptModelView();
                    List<PurchaseReceiptDetailModelView> podmvList = new List<PurchaseReceiptDetailModelView>();

                    pomv.Id = 0;
                    //   pomv.NO = po.NO;
                    pomv.CreatedFrom = "Purchase Order";
                    pomv.ReferenceNo = po.Id;
                    pomv.OrderNo = po.NO;
                    pomv.SupplierCode = po.Supplier.Code;
                    pomv.SupplierName = po.Supplier.Name;
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
                    pomv.PaymentTermId = po.PaymentTermId;
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

                    var podlist = db.PurchaseOrderDetails.Where(p => p.PurchaseOrderId == po.Id).ToList();
                    foreach (var pod in podlist)
                    {
                        var podmv = new PurchaseReceiptDetailModelView();
                        // podmv.PurchaseReceiptId = pod.PurchaseReceiptId;
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
                    return View(pomv);
                }
                else
                {
                    PurchaseInvoice po = db.PurchaseInvoices.Find(id);
                    if (po == null)
                    {
                        return HttpNotFound();
                    }
                    PurchaseInvoiceModelView pomv = new PurchaseInvoiceModelView();
                    List<PurchaseInvoiceDetailModelView> podmvList = new List<PurchaseInvoiceDetailModelView>();

                    pomv.Id = 0;
                    //   pomv.NO = po.NO;
                    pomv.CreatedFrom = "Purchase Invoice";
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

                    var podlist = db.PurchaseInvoiceDetails.Where(p => p.PurchaseInvoiceId == po.Id).ToList();
                    foreach (var pod in podlist)
                    {
                        var podmv = new PurchaseInvoiceDetailModelView();
                        // podmv.PurchaseReceiptId = pod.PurchaseReceiptId;
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
                        podmv.TaxId = 1;


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
                    return View(pomv);
                }
            }
            
            
        }

        //
        // POST: /PurchaseReceipt/Create
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
            var paymentTerms = db.PaymentTerms.Where(d => (d.CompanyId == companyid )).Select(d => new { Id = d.Id, PaymentTermDescription = d.PaymentTermDescription }).ToList();
            ViewBag.PaymentTerms = paymentTerms;
            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.ReferencePO = GetYetToBeReceivedPO();
            PurchaseReceiptModelView pomv = new PurchaseReceiptModelView();
            List<PurchaseReceiptDetailModelView> podmvList = new List<PurchaseReceiptDetailModelView>();
            var sid = poCollection["SupplierId"];
            var tid = poCollection["PaymentTermId"];
            var wid = poCollection["WarehouseId"];
            var rno = poCollection["ReferenceNo"];
            var totamt = poCollection["TotalAmount"];
            var grandtot = poCollection["GrandTotal"];
            var bcgrandtot = poCollection["BCGrandTotal"];
            if (sid != "")
                pomv.SupplierId = Convert.ToInt32(sid);
            if (tid != "")
                pomv.PaymentTermId = Convert.ToInt32(tid);
            if (wid != "")
                pomv.WarehouseId = Convert.ToInt32(wid);
           
          //  if (totamt != "")
              //  pomv.TotalAmount = Convert.ToDecimal(totamt);
            //if (grandtot != "")
            //    pomv.GrandTotal = Convert.ToDecimal(grandtot);
            //if (bcgrandtot != "")
            //    pomv.BCGrandTotal = Convert.ToDecimal(bcgrandtot);
            //if(rno != "")
            pomv.TotalAmount = 0;
            pomv.GrandTotal = 0;
            pomv.BCGrandTotal = 0;
            if (rno != "")
            pomv.ReferenceNo = Convert.ToInt64(poCollection["ReferenceNo"]);
            
            pomv.Id = Convert.ToInt64(poCollection["Id"]);
            pomv.NO = poCollection["NO"];
            pomv.CreatedFrom = poCollection["CreatedFrom"];
            
            if(pomv.ReferenceNo != null && pomv.NO=="")
                pomv.Id = 0;
            pomv.OrderNo = poCollection["OrderNo"];
            pomv.SupplierName = poCollection["SupplierName"];
            pomv.SupplierCode = poCollection["SupplierCode"];
            pomv.Reference = poCollection["Reference"];
            pomv.Date = Convert.ToString(poCollection["Date"]);
            pomv.ReceiptDate = Convert.ToString(poCollection["ReceiptDate"]);
            pomv.DeliveryName = poCollection["DeliveryName"];
            pomv.StreetPoBox = poCollection["StreetPoBox"];
            pomv.Suburb = poCollection["Suburb"];
            pomv.City = poCollection["City"];
            pomv.StateRegion = poCollection["StateRegion"];
            pomv.Country = poCollection["Country"];
            pomv.PostalCode = poCollection["PostalCode"];
            pomv.LorryNo = poCollection["LorryNo"];
            pomv.TagNo = poCollection["TagNo"];
            pomv.ReferenceChallan = poCollection["ReferenceChallan"];
            pomv.CurrencyId = basecurrency.CurrencyId;
            pomv.BaseCurrencyCode = poCollection["BaseCurrencyCode"];
            pomv.Currencyrate = Convert.ToDecimal(poCollection["Currencyrate"]);
            pomv.TransactionCurrency = Convert.ToInt32(poCollection["TransactionCurrency"]);
            pomv.TransactionCurrencyCode = poCollection["TransactionCurrencyCode"]; 
            pomv.Status = poCollection["Status"];
            List<InvoiceTotal> invoiceTotalList = new List<InvoiceTotal>();
            var count = poCollection["producthide"].Split(',').Length;
            for (int i = 0; i < count; i++)
            {
                PurchaseReceiptDetailModelView podmv = new PurchaseReceiptDetailModelView();
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
                   
                    //case "unitpricehide":

                    //    for (int i = 0; i < lt; i++)
                    //    {
                    //        if (!(value[i] == "" || value[i] == "null"))
                    //            podmvList[i].Price = Convert.ToDecimal(value[i]);
                    //    }
                    //    break;
                    //case "amounthide":

                    //    for (int i = 0; i < lt; i++)
                    //    {
                    //        if (!(value[i] == "" || value[i] == "null"))
                    //            podmvList[i].TotalAmount = Convert.ToDecimal(value[i]);
                    //    }
                    //    break;

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
            var checksupplier = db.Suppliers.Where(s => s.Code == pomv.SupplierCode && s.CompanyId == companyid).FirstOrDefault();
            var date = DateTime.ParseExact(pomv.Date, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var receiptdate = DateTime.ParseExact(pomv.ReceiptDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));

            var getDateRange = db.FinancialYearMasters.Where(d => d.fYearID == Fyid).FirstOrDefault();

            if (!(receiptdate >= getDateRange.sDate && receiptdate <= getDateRange.eDate))
            {
                var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                ViewBag.ddlWarehouses = warehouses;
                ViewBag.podmvList = podmvList;
                ViewBag.Date = "Receipt Date out of scope of " + getDateRange.Year + " Financial Year.";
                return View(pomv);
            }

            //if (checksupplier == null)
            //{
            //    var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            //    ViewBag.ddlWarehouses = warehouses;
            //    var taxes = mc.getDdlTaxes(userid, companyid, Branchid);
            //    ViewBag.ddlTaxes = taxes;
            //    ViewBag.podmvList = podmvList;
            //    ViewBag.Supply = "Customer does not exist!";
            //    if (duedate < date)
            //        ViewBag.Date = "Delivery Date can not be less than Order Date ";
            //    return View(pomv);
            //}
            //else if (duedate < date)
            //{
            //    var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            //    ViewBag.ddlWarehouses = warehouses;

            //    ViewBag.podmvList = podmvList;
            //    ViewBag.Date = "Delivery Date can not be less than Order Date ";
            //    return View(pomv);

            //}
            //else
            //{
            pomv.SupplierId = checksupplier.Id;
                pomv.TransactionCurrency = checksupplier.CurrencyRate.CurrencyId;
                pomv.Currencyrate = checksupplier.CurrencyRate.PurchaseRate;
              
                           
                pomv.BCGrandTotal = pomv.Currencyrate * pomv.GrandTotal;
      //      }

            //purchase order details model view validation
            decimal quantityrequested = 0;
            foreach (var podmv in podmvList)
            {
                if (podmv.ItemId != 0 && podmv.Quantity > 0 && podmv.UnitFormula > 0)
                {
                    quantityrequested += podmv.Quantity;
                    //do nothing
                }
                else
                {
                    if (!(podmv.ItemId == 0 && podmv.Quantity == 0 && podmv.UnitFormula==0 ))
                    {
                        var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                        ViewBag.ddlWarehouses = warehouses;
                        
                        ViewBag.podmvList = podmvList;

                        return View(pomv);
                    }

                }
            }
            //List<PurchaseReceiptDetailModelView> podmvList1 = new List<PurchaseReceiptDetailModelView>();
            //foreach (var item in podmvList)
            //{
            //    var findduplicate=podmvList1.Where(p=>p.ItemId==item.ItemId).FirstOrDefault();
            //    if(findduplicate !=null)
            //    {
            //       var firstitem= podmvList1.Where(p=>p.ItemId==item.ItemId).FirstOrDefault();
            //        firstitem.Quantity += item.Quantity;
            //        firstitem.TotalQuantity += item.TotalQuantity;
            //    }
            //    podmvList1. 
            //}
            //var duplicates = podmvList.Where(d => d.ItemId != 0).GroupBy(s => s.ItemId).SelectMany(grp => grp.Skip(1));
            //if (duplicates.Count() != 0)
            //{
            //    var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            //    ViewBag.ddlWarehouses = warehouses;

            //    ViewBag.podmvList = podmvList;
               
            //    ViewBag.ErrorTag = "Duplicate Items are not allowed in the product line. Please combine it in single product line. ";
            //    ViewBag.Message = "Error";
            //    return View(pomv);
            //}
            //if (pomv.ReferenceNo != null)
            //{

            //    var quantityreceiveded = db.PurchaseReceiptDetails.Where(p => p.PurchaseReceipt.CreatedFrom == "Purchase Order" && p.PurchaseReceipt.ReferenceNo == pomv.ReferenceNo).Sum(p => (decimal?)p.Quantity) ?? 0;
            //    var quantityordered = db.PurchaseOrderDetails.Where(p => p.PurchaseOrder.Id == pomv.ReferenceNo).Sum(p => (decimal?)p.Quantity) ?? 0;
            //    if (quantityordered < quantityreceiveded + quantityrequested)
            //    {
            //        var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            //        ViewBag.ddlWarehouses = warehouses;

            //        ViewBag.podmvList = podmvList;
            //        ViewBag.ErrorTag = "Total quantity of  Received Items cannot exceed total qauntity ordered in Purchase Order " + pomv.OrderNo + " .";
            //        ViewBag.Message = "Error";
            //        return View(pomv);
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
                            if (db.PurchaseReceipts.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.FinancialYearId == Fyid).Count() != 0)
                            {
                                countpo = (int)db.PurchaseReceipts.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.FinancialYearId == Fyid).Max(p => p.InvoiceNo) + 1;
                            }
                            var getPrefix = db.Prefixes.Where(p => p.DefaultPrefix == "PC" && p.CompanyId == companyid && p.BranchId == Branchid).Select(p => new { p.DefaultPrefix, p.SetPrefix }).FirstOrDefault();

                            if (getPrefix.SetPrefix != null)
                                pomv.NO = getPrefix.SetPrefix + "/" + fyear + "/" + countpo;//tc.GenerateCode(fyear, countpo);
                            else
                                pomv.NO = getPrefix.DefaultPrefix + "/" + fyear + "/" + countpo; //tc.GenerateCode(fyear, countpo);
                            

                            //Insert into purchasereceipt table
                            PurchaseReceipt po = new PurchaseReceipt();
                            po.NO = pomv.NO;
                            po.ReferenceChallan = pomv.ReferenceChallan;
                            po.SupplierId = pomv.SupplierId;
                            po.Reference = pomv.Reference;
                            po.InvoiceNo = countpo;
                            po.WarehouseId = pomv.WarehouseId;
                            po.Date = date;
                            po.DueDate = date;
                            po.ReceiptDate = receiptdate;
                            po.DeliveryName = pomv.DeliveryName;
                            po.StreetPoBox = pomv.StreetPoBox;
                            po.Suburb = pomv.Suburb;
                            po.City = pomv.City;
                            po.StateRegion = pomv.StateRegion;
                            po.Country = pomv.Country;
                            po.PostalCode = pomv.PostalCode;
                            po.LorryNo = pomv.LorryNo;
                            po.TagNo =Convert.ToString(countpo);
                            po.CurrencyId = pomv.CurrencyId;
                            po.Currencyrate = pomv.Currencyrate;
                            po.TransactionCurrency = pomv.TransactionCurrency;
                            po.PaymentTermId = pomv.PaymentTermId;
                           // po.TotalAmount = pomv.TotalAmount;
                         //   po.GrandTotal = pomv.GrandTotal;
                            po.TotalAmount = 0;
                            po.GrandTotal = 0;

                          //  po.BCGrandTotal = pomv.BCGrandTotal;
                            po.BCGrandTotal = 0;
                            po.CreatedBy = Createdby;
                            po.CreatedOn = DateTime.Now;
                            po.FinancialYearId = Fyid;
                            po.UserId = userid;
                            po.BranchId = Branchid;
                            po.CompanyId = companyid;
                            po.Status = pomv.Status;
                            db.PurchaseReceipts.Add(po);

                            db.SaveChanges();
                            pomv.TagNo = po.TagNo;
                            pomv.Id = po.Id;

                            decimal tquantity = 0;

                            foreach (var podmv in podmvList)
                            {
                                if (podmv.ItemId != 0 && podmv.Quantity > 0 && podmv.UnitFormula > 0)
                                {


                                    PurchaseReceiptDetail pod = new PurchaseReceiptDetail();
                                    pod.PurchaseReceiptId = po.Id;
                                    pod.ItemId = podmv.ItemId;
                                    pod.BarCode = podmv.BarCode;
                                    pod.Description = podmv.Description;
                                    pod.Quantity = podmv.Quantity;
                                    pod.AccountId = 12;
                                    pod.UnitId = podmv.UnitId;
                                 //   pod.Price = podmv.Price;
                                    pod.Price = 0;
                                    pod.CurrencyRate = po.Currencyrate;
                                    pod.UnitIdSecondary = podmv.UnitIdSecondary;
                                    pod.SecUnitId = podmv.SecUnitId;
                                    pod.UnitFormula = podmv.UnitFormula;
                                    pod.SecUnitFormula = podmv.SecUnitFormula;

                                    tquantity += podmv.Quantity;
                                    //   pod.TotalAmount = podmv.Price * podmv.Quantity * podmv.UnitFormula;
                                    pod.TotalAmount = 0;

                                    db.PurchaseReceiptDetails.Add(pod);
                                    if (po.Status == "Saved")
                                    {
                                        var stock = new Stock();
                                        stock.ArticleID = podmv.ItemId;
                                        stock.Items = podmv.Quantity ;
                                        stock.Price = podmv.Price;
                                        stock.TranCode = "IN";
                                        stock.TransTag = "PR";
                                        stock.TranDate = date;
                                        stock.TranId = po.Id;
                                        // stock.Comments=
                                        stock.WarehouseId = po.WarehouseId;
                                        stock.UserId = po.UserId;
                                        stock.CompanyId = po.CompanyId;
                                        stock.BranchId = po.BranchId;
                                        stock.CreatedBy = po.CreatedBy;
                                        db.Stocks.Add(stock);
                                    }

                                }

                            }
                           // if (pomv.CreatedFrom == "Purchase Order")
                            if (pomv.ReferenceNo != null)
                            {
                                po.CreatedFrom ="Purchase Order";
                                po.ReferenceNo = pomv.ReferenceNo;
                               // po.OrderNo = pomv.OrderNo;
                                var findpo = db.PurchaseOrders.Find(pomv.ReferenceNo);
                                if (findpo != null)
                                {
                                    po.OrderNo = findpo.NO;
                                    var getOrderedQuantity = db.PurchaseOrderDetails.Where(d => d.PurchaseOrderId == findpo.Id).Sum(d => d.Quantity);

                                    decimal getPreviousReceived= db.PurchaseReceiptDetails.Where(d=>d.PurchaseReceipt.ReferenceNo== findpo.Id).Select(d => d.Quantity).DefaultIfEmpty(0).Sum();
                                    decimal totalqnty = getPreviousReceived + tquantity;

                                    if (totalqnty <= getOrderedQuantity)
                                    {
                                        var getShrinkage = (getOrderedQuantity - totalqnty) * 100 / getOrderedQuantity;
                                        if(getShrinkage <=5)
                                            findpo.Status = InventoryConst.cns_Received;
                                        else
                                            findpo.Status = InventoryConst.cns_Partially_Received;
                                    }
                                    else
                                        findpo.Status = InventoryConst.cns_Received;
                                }
                            }
                        }
                        else
                        {
                            //Update Purchase Receipt
                            var po = db.PurchaseReceipts.Find(pomv.Id);

                      //      po.NO = pomv.NO;
                            po.SupplierId = pomv.SupplierId;
                            if(pomv.Reference !=null)
                                    po.Reference = pomv.Reference;
                            po.ReferenceChallan = pomv.ReferenceChallan;
                            po.WarehouseId = pomv.WarehouseId;
                            po.Date = date;
                            po.DueDate = date;
                            po.ReceiptDate = receiptdate;
                            po.DeliveryName = pomv.DeliveryName;
                            po.StreetPoBox = pomv.StreetPoBox;
                            po.Suburb = pomv.Suburb;
                            po.City = pomv.City;
                            po.StateRegion = pomv.StateRegion;
                            po.Country = pomv.Country;
                            po.PostalCode = pomv.PostalCode;
                            po.LorryNo = pomv.LorryNo;
                          //  po.TagNo = pomv.TagNo;
                            po.CurrencyId = pomv.CurrencyId;
                            po.Currencyrate = pomv.Currencyrate;
                            po.TransactionCurrency = pomv.TransactionCurrency;
                            po.PaymentTermId = pomv.PaymentTermId;
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

                            decimal oldquantity = 0;
                            decimal newquantity = 0;
                            var podOldRecords = db.PurchaseReceiptDetails.Where(p => p.PurchaseReceiptId == po.Id).ToList();
                            foreach (var podOld in podOldRecords)
                            {
                                oldquantity += podOld.Quantity;
                                db.PurchaseReceiptDetails.Remove(podOld);
                            }
                            var oldStock = db.Stocks.Where(p => p.TransTag == "PR" && p.TranId == pomv.Id).ToList();
                            foreach (var stock in oldStock)
                            {
                                db.Stocks.Remove(stock);
                            }
                            foreach (var podmv in podmvList)
                            {
                                if (podmv.ItemId != 0 && podmv.Quantity > 0 && podmv.UnitFormula > 0)
                                {


                                    PurchaseReceiptDetail pod = new PurchaseReceiptDetail();
                                    pod.PurchaseReceiptId = po.Id;
                                    pod.ItemId = podmv.ItemId;
                                    pod.BarCode = podmv.BarCode;
                                    pod.Description = podmv.Description;
                                    pod.Quantity = podmv.Quantity;
                                    pod.AccountId = 12;
                                    pod.UnitId = podmv.UnitId;
                                   // pod.Price = podmv.Price;
                                    pod.Price =0;
                                    pod.CurrencyRate = po.Currencyrate;
                                    pod.UnitIdSecondary = podmv.UnitIdSecondary;
                                    pod.SecUnitId = podmv.SecUnitId;
                                    pod.UnitFormula = podmv.UnitFormula;
                                    pod.SecUnitFormula = podmv.SecUnitFormula;

                                    newquantity += pod.Quantity;
                                   // pod.TotalAmount = podmv.Price * podmv.Quantity * podmv.UnitFormula;
                                    pod.TotalAmount = 0;

                                    db.PurchaseReceiptDetails.Add(pod);

                                    if (po.Status == "Saved")
                                    {
                                        var stock = new Stock();
                                        stock.ArticleID = podmv.ItemId;
                                        stock.Items = podmv.Quantity;
                                        stock.Price = podmv.Price;
                                        stock.TranCode = "IN";
                                        stock.TransTag = "PC";
                                        stock.TranDate = date;
                                        stock.TranId = po.Id;
                                        // stock.Comments=
                                        stock.WarehouseId = po.WarehouseId;
                                        stock.UserId = po.UserId;
                                        stock.CompanyId = po.CompanyId;
                                        stock.BranchId = po.BranchId;
                                        stock.CreatedBy = po.CreatedBy;
                                        db.Stocks.Add(stock);
                                    }

                                }

                            }

                            if (oldquantity != newquantity)
                            {
                                po.CreatedFrom = "Purchase Order";

                                if(pomv.ReferenceNo==null)
                                   pomv.ReferenceNo = po.ReferenceNo;
                                // po.OrderNo = pomv.OrderNo;
                                var findpo = db.PurchaseOrders.Find(pomv.ReferenceNo);
                                if (findpo != null)
                                {
                                    po.OrderNo = findpo.NO;
                                    var getOrderedQuantity = db.PurchaseOrderDetails.Where(d => d.PurchaseOrderId == findpo.Id).Sum(d => d.Quantity);

                                    decimal getPreviousReceived = db.PurchaseReceiptDetails.Where(d => d.PurchaseReceipt.ReferenceNo == findpo.Id && d.PurchaseReceiptId!= po.Id).Select(d => d.Quantity).DefaultIfEmpty(0).Sum() ;
                                    decimal totalqnty =getPreviousReceived  + newquantity;

                                    if (totalqnty <= getOrderedQuantity)
                                    {
                                        var getShrinkage = (getOrderedQuantity - totalqnty) * 100 / getOrderedQuantity;
                                        if (getShrinkage <= 5)
                                            findpo.Status = InventoryConst.cns_Received;
                                        else
                                            findpo.Status = InventoryConst.cns_Partially_Received;
                                    }
                                    else
                                        findpo.Status = InventoryConst.cns_Received;
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
                ViewBag.ErrorTag = "Unable to save changes. Try again, and if the problem persists see your system administrator.";
                ViewBag.Message = "Error";
                return View(pomv);
            }
            catch (DataException Ex)
            {
                //Log the error (add a variable name after DataException)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                ViewBag.ddlWarehouses = warehouses1;

                ViewBag.podmvList = podmvList;
                ViewBag.ErrorTag = "Unable to save changes. Try again, and if the problem persists see your system administrator.";
                ViewBag.Message = "Error";
                return View(pomv);
            }
            catch (Exception exp)
            {
                var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                ViewBag.ddlWarehouses = warehouses1;

                ViewBag.podmvList = podmvList;
                ViewBag.ErrorTag = "Unable to save changes. Try again, and if the problem persists see your system administrator.";
                ViewBag.Message = "Error";
                return View(pomv);
            }
                var warehouses2 = mc.getDdlWarehouses(companyid, Branchid);
                ViewBag.ddlWarehouses = warehouses2;
                
                ViewBag.podmvList = podmvList;
                ViewBag.Message = "You have successfully "+ pomv.Status +" Purchase Receipt Note " + pomv.NO;
                return View(pomv);
            
        }

        //get
        //Create Purchase Receipt from PO
        public ActionResult CreateFromPO(long? id = 0)
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt32(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var warehouses = mc.getDdlWarehouses( companyid, Branchid);
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
            PurchaseReceiptModelView pomv = new PurchaseReceiptModelView();
            List<PurchaseReceiptDetailModelView> podmvList = new List<PurchaseReceiptDetailModelView>();

            pomv.Id = po.Id;
            pomv.NO = po.NO;
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
            pomv.Date = po.Date.ToString(dateFormat);
            pomv.DueDate = po.DueDate.ToString(dateFormat);
            pomv.DeliveryName = po.DeliveryName;
            pomv.StreetPoBox = po.StreetPoBox;
            pomv.Suburb = po.Suburb;
            pomv.City = po.City;
            pomv.StateRegion = po.StateRegion;
            pomv.Country = po.Country;
            pomv.PostalCode = po.PostalCode;
            pomv.TaxTotal = po.TaxTotal;
            pomv.TotalAmount = po.TotalAmount;
           
            
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
                var podmv = new PurchaseReceiptDetailModelView();
             //   podmv.PurchaseReceiptId = pod.PurchaseReceiptId;
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
                podmv.TaxName = pod.Tax.Name + '-' + pod.Tax.TaxId;
             //   podmv.Discount = pod.Discount;
                podmv.TaxPercent = pod.TaxPercent;
                podmv.TaxAmount = pod.TaxAmount;
                podmv.TotalAmount = pod.TotalAmount;
                podmv.TaxAmount = pod.TaxAmount;
               
                podmvList.Add(podmv);
            }
            ViewBag.podmvList = podmvList;
            return View(pomv);

        }

        //
        // POST: /PurchaseReceipt/CreateFromPO

        [HttpPost]
        public ActionResult CreateFromPO(FormCollection poCollection)
        {

            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt32(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            long Createdby = Convert.ToInt32(Session["Createdid"]);
            var culture = Session["DateCulture"].ToString();
            string dateFormat = Session["DateFormat"].ToString();
           // System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            string currencyval = string.Empty;
            PurchaseReceiptModelView pomv = new PurchaseReceiptModelView();
            List<PurchaseReceiptDetailModelView> podmvList = new List<PurchaseReceiptDetailModelView>();
            var sid = poCollection["SupplierId"];
            var tid = poCollection["TaxId"];
            var wid = poCollection["WarehouseId"];
           

            if (sid != "")
                pomv.SupplierId = Convert.ToInt32(sid);
           
            if (wid != "")
                pomv.WarehouseId = Convert.ToInt32(wid);
            

            
            pomv.Id = Convert.ToInt64(poCollection["Id"]);
            pomv.NO = poCollection["NO"];
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

            var count = poCollection["producthide"].Split(',').Length;
            for (int i = 0; i < count; i++)
            {
                PurchaseReceiptDetailModelView podmv = new PurchaseReceiptDetailModelView();
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
            var checksupplier = db.Suppliers.Where(s => s.Code == pomv.SupplierCode).FirstOrDefault();
            var date = DateTime.ParseExact(pomv.Date, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var duedate = DateTime.ParseExact(pomv.DueDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            if (checksupplier == null)
            {
                var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                ViewBag.ddlWarehouses = warehouses;
                var taxes = mc.getDdlTaxes(userid, companyid, Branchid);
                ViewBag.ddlTaxes = taxes;
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
                var taxes = mc.getDdlTaxes(userid, companyid, Branchid);
                ViewBag.ddlTaxes = taxes;
                ViewBag.podmvList = podmvList;
                ViewBag.Date = "Delivery Date can not be less than Order Date ";
                return View(pomv);

            }
            else
            {
                pomv.SupplierId = checksupplier.Id;

                pomv.TransactionCurrency = checksupplier.CurrencyRate.CurrencyId;
                pomv.Currencyrate = checksupplier.CurrencyRate.PurchaseRate;
            
                pomv.BCGrandTotal = pomv.Currencyrate * pomv.GrandTotal;
            }

            //purchase order details model view validation
            foreach (var podmv in podmvList)
            {
                if (podmv.ItemId != 0 && podmv.Quantity > 0)
                {
                    //do nothing
                }
                else
                {
                    if (!(podmv.ItemId == 0 && podmv.Quantity == 0 && podmv.Price == 0))
                    {
                        var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                        ViewBag.ddlWarehouses = warehouses;
                        var taxes = mc.getDdlTaxes(userid, companyid, Branchid);
                        ViewBag.ddlTaxes = taxes;
                        ViewBag.podmvList = podmvList;

                        return View(pomv);
                    }

                }
            }

            //Check Insert or Update
            if (pomv.Id == 0)
            {

                int countpo = db.PurchaseReceipts.Where(p => p.CompanyId == companyid && p.BranchId == Branchid).Count();
                pomv.NO = tc.GenerateCode("PR", countpo);

                //Insert into purchaseorder table
                PurchaseReceipt po = new PurchaseReceipt();
                po.NO = pomv.NO;
                po.SupplierId = pomv.SupplierId;
                po.CreatedFrom = pomv.CreatedFrom;
                po.ReferenceNo = pomv.ReferenceNo;
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
                db.PurchaseReceipts.Add(po);

                db.SaveChanges();
                pomv.Id = po.Id;
                foreach (var podmv in podmvList)
                {
                    if (podmv.ItemId != 0 && podmv.Quantity > 0 )
                    {
                        var arr = podmv.TaxName.Split('-');
                        var taxamt = Convert.ToDecimal(arr[arr.Length - 1]);
                        podmv.TaxPercent = taxamt;
                        podmv.TaxAmount = taxamt * podmv.TotalAmount / 100;
                       
                        PurchaseReceiptDetail pod = new PurchaseReceiptDetail();
                        pod.PurchaseReceiptId = po.Id;
                        pod.ItemId = podmv.ItemId;
                        pod.Description = podmv.Description;
                        pod.Quantity = podmv.Quantity;
                        pod.AccountId = 12;
                        pod.UnitId = podmv.UnitId;
                        pod.Price = podmv.Price;
                        pod.CurrencyRate = po.Currencyrate;
                     
                        
                   
                         pod.TotalAmount = podmv.TotalAmount;
                    
                        
                        db.PurchaseReceiptDetails.Add(pod);
                    
                    }

                }
            }
            else
            {
                //Update Purchase Receipt
                var po = db.PurchaseReceipts.Find(pomv.Id);

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

                var podOldRecords = db.PurchaseReceiptDetails.Where(p => p.PurchaseReceiptId == po.Id).ToList();
                foreach (var podOld in podOldRecords)
                {
                    db.PurchaseReceiptDetails.Remove(podOld);
                }

                foreach (var podmv in podmvList)
                {
                    if (podmv.ItemId != 0 && podmv.Quantity > 0 )
                    {
                        var arr = podmv.TaxName.Split('-');
                        var taxamt = Convert.ToDecimal(arr[arr.Length - 1]);
                        podmv.TaxPercent = taxamt;
                        podmv.TaxAmount = taxamt * podmv.TotalAmount / 100;
                       
                        PurchaseReceiptDetail pod = new PurchaseReceiptDetail();
                        pod.PurchaseReceiptId = po.Id;
                        pod.ItemId = podmv.ItemId;
                        pod.Description = podmv.Description;
                        pod.Quantity = podmv.Quantity;
                        pod.AccountId = 12;
                        pod.UnitId = podmv.UnitId;
                        pod.Price = podmv.Price;
                        pod.CurrencyRate = po.Currencyrate;
                        pod.TotalAmount = podmv.TotalAmount;
                        db.PurchaseReceiptDetails.Add(pod);

                    }

                }
            }

            db.SaveChanges();
            var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
            ViewBag.ddlWarehouses = warehouses1;
            var taxes1 = mc.getDdlTaxes(userid, companyid, Branchid);
            ViewBag.ddlTaxes = taxes1;
            ViewBag.podmvList = podmvList;
            ViewBag.Message = "You have successfully " + pomv.Status + " Purchase Receipt Note " + pomv.NO;
            
            return View(pomv);

        }

        //
        // GET: /PurchaseReceipt/Edit/5

        public ActionResult Edit(long id = 0)
        {
            PurchaseReceipt purchasereceipt = db.PurchaseReceipts.Find(id);
            if (purchasereceipt == null)
            {
                return HttpNotFound();
            }
            ViewBag.CurrencyId = new SelectList(db.Currencies, "CurrencyId", "Country", purchasereceipt.CurrencyId);
            ViewBag.TransactionCurrency = new SelectList(db.Currencies, "CurrencyId", "Country", purchasereceipt.TransactionCurrency);
            ViewBag.PurchaseOrderId = new SelectList(db.PurchaseOrders, "Id", "Reference", purchasereceipt.PurchaseOrderId);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "Id", "Code", purchasereceipt.SupplierId);
          //  ViewBag.TaxId = new SelectList(db.Taxes, "TaxId", "Name", purchasereceipt.TaxId);
            ViewBag.WarehouseId = new SelectList(db.Warehouses, "Id", "Code", purchasereceipt.WarehouseId);
            return View(purchasereceipt);
        }

        //
        // POST: /PurchaseReceipt/Edit/5

        [HttpPost]
        public ActionResult Edit(PurchaseReceipt purchasereceipt)
        {
            if (ModelState.IsValid)
            {
                db.Entry(purchasereceipt).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CurrencyId = new SelectList(db.Currencies, "CurrencyId", "Country", purchasereceipt.CurrencyId);
            ViewBag.TransactionCurrency = new SelectList(db.Currencies, "CurrencyId", "Country", purchasereceipt.TransactionCurrency);
            ViewBag.PurchaseOrderId = new SelectList(db.PurchaseOrders, "Id", "Reference", purchasereceipt.PurchaseOrderId);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "Id", "Code", purchasereceipt.SupplierId);
         //   ViewBag.TaxId = new SelectList(db.Taxes, "TaxId", "Name", purchasereceipt.TaxId);
            ViewBag.WarehouseId = new SelectList(db.Warehouses, "Id", "Code", purchasereceipt.WarehouseId);
            return View(purchasereceipt);
        }

        //
        // GET: /PurchaseReceipt/Delete/5

        public ActionResult Delete(long id = 0)
        {
            PurchaseReceipt purchasereceipt = db.PurchaseReceipts.Find(id);
            if (purchasereceipt == null)
            {
                return HttpNotFound();
            }
            return View(purchasereceipt);
        }

        //
        // POST: /PurchaseReceipt/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            PurchaseReceipt purchasereceipt = db.PurchaseReceipts.Find(id);
            db.PurchaseReceipts.Remove(purchasereceipt);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public List<POGeneral> GetYetToBeReceivedPO()
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt32(Session["companyid"]);
            var getwarehouseid = db.Warehouses.Where(d => d.Branchid == Branchid).Select(d=>d.Id).FirstOrDefault();
            var getPoitems = db.PurchaseOrderDetails.Where(po => (po.PurchaseOrder.Status == InventoryConst.cns_Saved || po.PurchaseOrder.Status == InventoryConst.cns_Partially_Received || po.PurchaseOrder.Status == InventoryConst.cns_Partially_Invoiced) && po.PurchaseOrder.ApproveStatus == 1)// && po.PurchaseOrder.WarehouseId == getwarehouseid)
                           .GroupBy(po=>new {po.PurchaseOrderId,po.PurchaseOrder.NO }) 
                           .Select(po => new  { Id = po.Key.PurchaseOrderId, Name = po.Key.NO,Quantity=po.Sum(d=>d.Quantity) }).ToList();
            var count = getPoitems.Count();
            long?[] myInts = new long?[count];
            for(int i=0; i< count; i++)
            {
                myInts[i] = getPoitems[i].Id;
            }
            var getPRdetails = (from p in db.PurchaseReceiptDetails
                                where myInts.Contains( p.PurchaseReceipt.ReferenceNo)
                                group p by p.PurchaseReceipt.ReferenceNo into g
                                select new  { Id = g.Key, Quantity = g.Sum(d => d.Quantity) }).ToList();

            var getPOPR = from po in getPoitems
                          from pr in getPRdetails.Where(p => p.Id == po.Id).DefaultIfEmpty()
                          select new
                          {
                              Id = po.Id,
                              Name = po.Name,
                              POQnty = po.Quantity,
                              PRQnty = pr == null ? 0 : pr.Quantity
                          };
            List<POGeneral> poList = new List<POGeneral>();
            foreach(var popr in getPOPR)
            {
                var po = new POGeneral();
                po.Id = popr.Id;
                po.Name = popr.Name + "(" + popr.POQnty + ")(" + popr.PRQnty + ")";
                poList.Add(po);
            }
              return poList;
        }
        public JsonResult GetPRDetails(string PRNo)
        {
            var dateFormat = Session["DateFormat"].ToString();
            var culture = Session["DateCulture"].ToString();
            PurchaseReceiptModelView po = new PurchaseReceiptModelView();
            string[] value = PRNo.Split(',');
            int[] myInts = Array.ConvertAll(value, int.Parse);

            List<PurchaseReceiptDetailModelView> getPRDetailsMV = null;


            var getPRDetails = (from p in db.PurchaseReceiptDetails
                          where myInts.Any(val => p.PurchaseReceiptId == val)
                                select new PurchaseReceiptDetailModelView {ItemId= p.ItemId,Description= p.Description,Quantity= p.Quantity, TotalQuantity = p.Quantity,SecUnitFormula= p.SecUnitFormula,SecUnitId= p.SecUnitId,UnitFormula= p.UnitFormula,UnitId= p.UnitId,UnitIdSecondary= p.UnitIdSecondary, ItemCode = p.Product.Code, ItemName = p.Product.Name, UnitName = p.UOM.Code, UnitSecondaryName = p.UOM1.Code, SecUnitName = p.UOM2.Code, TaxId = 1, TaxName = "No Tax", Price = 0, TotalAmount = 0 }).ToList();
            //var getPODetails = db.PurchaseReceiptDetails.Where(p => p.PurchaseReceiptId == PRNo).Select(p => new { p.ItemId, p.Description, p.Quantity, TotalQuantity = p.Quantity, p.SecUnitFormula, p.SecUnitId, p.UnitFormula, p.UnitId, p.UnitIdSecondary, ItemCode = p.Product.Code, ItemName = p.Product.Name, UnitName = p.UOM.Code, UnitSecondaryName = p.UOM1.Code, SecUnitName = p.UOM2.Code }).ToList();

            var getPOMain = db.PurchaseReceipts.Where(p => myInts.Any(val => p.Id == val)).Select(p => new PurchaseReceiptModelView { BaseCurrencyCode = p.Currency.ISO_4217, City = p.City, Country = p.Country, CurrencyId = p.CurrencyId, Currencyrate = p.Currencyrate, CreatedOn = p.Date, DeliveryName = p.DeliveryName, ModifiedOn = p.ReceiptDate, PaymentTermId = p.PaymentTermId, PostalCode = p.PostalCode, StateRegion = p.StateRegion, StreetPoBox = p.StreetPoBox, Suburb = p.Suburb, SupplierCode = p.Supplier.Code, SupplierId = p.SupplierId, SupplierName = p.Supplier.Name, TransactionCurrency = p.TransactionCurrency, TransactionCurrencyCode = p.Currency1.ISO_4217, WarehouseId = p.WarehouseId,OrderNo=p.OrderNo,ReferenceNo=p.ReferenceNo }).FirstOrDefault();
            getPOMain.pod = getPRDetails;
            po = getPOMain;
            po.Date = getPOMain.CreatedOn.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
            po.ReceiptDate = getPOMain.ModifiedOn.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));

            if (getPOMain.ReferenceNo != null)
            {
                decimal referenceNo = (decimal)getPOMain.ReferenceNo;
                var getPOdetails = db.PurchaseOrderDetails.Where(p => p.PurchaseOrderId == referenceNo).ToList();
                getPRDetailsMV = getPRDetails;
                foreach(var details in getPRDetailsMV)
                {
                    details.Price = getPOdetails.Where(d => d.ItemId == details.ItemId).Select(d => d.Price).FirstOrDefault();
                    details.TotalAmount = details.Price * details.Quantity;
                }
                getPOMain.pod = getPRDetailsMV;
                
            }
            return Json(getPOMain, JsonRequestBehavior.AllowGet);
        }

        #region PDF Email



        public ActionResult CreatePurchaseReceiptPDF(long? id, long? Branchid, long? companyid, long? userid)
        {
            //long Branchid = Convert.ToInt64(Session["BranchId"]);
            //long companyid = Convert.ToInt64(Session["companyid"]);
            //long userid = Convert.ToInt32(Session["userid"]);
            var culture = Convert.ToString(Session["DateCulture"]);
            var dateFormat = Convert.ToString(Session["DateFormat"]);
            PurchaseReceiptModelView pomv = new PurchaseReceiptModelView();
            List<PurchaseReceiptDetailModelView> podmvList = new List<PurchaseReceiptDetailModelView>();
            //   List<PurchaseCostingDetailModelView> pcdmvList = new List<PurchaseCostingDetailModelView>();
            //  List<InvoiceTotal> invoiceTotalList = new List<InvoiceTotal>();






            //var taxes = db.Taxes.Where(d => (d.CompanyId == companyid && d.BranchId == Branchid) || (d.UserId == 0 && d.CompanyId == 0)).Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate }).ToList();
            //ViewBag.TaxSingle = taxes;

            //var Taxcomponents = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid && d.Tax.BranchId == Branchid) || (d.Tax.CompanyId == 0 && d.Tax.BranchId == 0)).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            //ViewBag.Taxcomponents = Taxcomponents;

            PurchaseReceipt po = db.PurchaseReceipts.Find(id);
            if (po == null)
            {
                return HttpNotFound();
            }

            //var numtowords = new NumberToEnglish();
            //ViewBag.BCTotalAmount = numtowords.changeNumericToWords(po.BCGrandTotal);

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

            //var logo = db.BusinessPartners.Where(r => r.CompanyId == companyid).Select(s => s.Logo).ToList();
            //ViewBag.Logo = logo;


            var podlist = db.PurchaseReceiptDetails.Where(p => p.PurchaseReceiptId == po.Id).ToList();
            foreach (var pod in podlist)
            {
                var podmv = new PurchaseReceiptDetailModelView();
                podmv.PurchaseReceiptId = pod.PurchaseReceiptId;
                podmv.ItemId = pod.ItemId;
                podmv.ItemName = pod.Product.Description;
                podmv.Description = pod.Description;
                podmv.Quantity = pod.Quantity;
                podmv.AccountId = pod.AccountId;
                podmv.UnitId = pod.UnitId;
                podmv.UnitName = pod.UOM.Code;
                podmv.Price = pod.Price;
                podmv.CurrencyRate = pod.CurrencyRate;
               

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



        public ActionResult PrintPurchaseReceiptPDF(long? id)
        {
            //return RedirectToAction("CreteSalesInvoicePDF", new { id = id });
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);

            return new ActionAsPdf("CreatePurchaseReceiptPDF", new { id = id, Branchid = Branchid, companyid = companyid, userid = userid }) { FileName = "PurchaseReceiptPrint.pdf" };
        }





        #endregion
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
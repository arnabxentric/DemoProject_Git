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
using System.Text.RegularExpressions;
using Rotativa;

namespace XenERP.Controllers
{
    public class SalesDeliveryController : Controller
    {
        private InventoryEntities db = new InventoryEntities();
        private MasterClasses mc = new MasterClasses();
        private TransactionClasses tc = new TransactionClasses();
        //
        // GET: /SalesDelivery/
        [SessionExpire]
        public ActionResult Index()
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            ViewBag.BaseCurrency = basecurrency.CurrencyCode;
            if(Branchid==0)
                return View(db.SalesDeliveries.Where(p => p.CompanyId == companyid && p.FinancialYearId == Fyid).ToList());
            else
                return View(db.SalesDeliveries.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.FinancialYearId == Fyid).ToList());
        }

        //
        // GET: /SalesDelivery/Details/5

        public ActionResult Details(long id = 0)
        {
            SalesDelivery salesdelivery = db.SalesDeliveries.Find(id);
            if (salesdelivery == null)
            {
                return HttpNotFound();
            }
            return View(salesdelivery);
        }

        //
        // GET: /SalesDelivery/Create
        [SessionExpire]
        public ActionResult Create(long? id = 0, string from = null)
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            long Createdby = Convert.ToInt32(Session["Createdid"]);
            var dateFormat = Session["DateFormat"].ToString();
            var culture = Session["DateCulture"].ToString();
            //  System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            ViewBag.ddlWarehouses = warehouses;
            var paymentTerms = db.PaymentTerms.Where(d => (d.CompanyId == companyid )).Select(d => new { Id = d.Id, PaymentTermDescription = d.PaymentTermDescription }).ToList();
            ViewBag.PaymentTerms = paymentTerms;
            var salesPerson = db.SalesPersons.Where(d => (d.UserId == userid && d.CompanyId == companyid)).Select(d => new { Id = d.Id, Name = d.Name }).ToList();
            ViewBag.ddlSalesPerson = salesPerson;
            ViewBag.unit = db.UOMs.Where(d => (d.UserId == userid && d.CompanyId == companyid)).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => (d.UserId == userid && d.CompanyId == companyid)).ToList();


            if (id == 0)
            {
                SalesDeliveryModelView pomv = new SalesDeliveryModelView();
                pomv.Date = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.DueDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.ReceiptDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.DespatchDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.CurrencyId = basecurrency.CurrencyId;
                pomv.BaseCurrencyCode = basecurrency.CurrencyCode;
                pomv.TransactionCurrency = basecurrency.CurrencyId;
                pomv.TransactionCurrencyCode = basecurrency.CurrencyCode;
                pomv.Currencyrate = 1;


                return View(pomv);
            }
            else
            {
                if (from == null)
                {
                    var checkMenu = db.MenuaccessUsers.Any(d => d.AssignedUserId == Createdby && d.Name == "SpecialPriviledge");
                    if (checkMenu)
                    {
                        ViewBag.Special = "Special";
                    }

                    SalesDelivery po = db.SalesDeliveries.Find(id);
                    if (po == null)
                    {
                        return HttpNotFound();
                    }
                    SalesDeliveryModelView pomv = new SalesDeliveryModelView();
                    List<SalesDeliveryDetailModelView> podmvList = new List<SalesDeliveryDetailModelView>();

                    pomv.Id = po.Id;
                    pomv.NO = po.NO;
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
                    pomv.WarehouseId = po.WarehouseId;
                    pomv.CurrencyId = po.CurrencyId;
                    pomv.BaseCurrencyCode = po.Currency.ISO_4217;
                    pomv.Currencyrate = po.Currencyrate;
                    pomv.TransactionCurrency = po.TransactionCurrency;
                    pomv.TransactionCurrencyCode = po.Currency1.ISO_4217;
                    pomv.Date = po.Date.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DueDate = po.DueDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.ReceiptDate = po.ReceiptDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DespatchDate = po.DespatchDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DeliveryName = po.DeliveryName;
                    pomv.StreetPoBox = po.StreetPoBox;
                    pomv.Suburb = po.Suburb;
                    pomv.City = po.City;
                    pomv.StateRegion = po.StateRegion;
                    pomv.Country = po.Country;
                    pomv.PostalCode = po.PostalCode;
                    pomv.TotalAmount = po.TotalAmount;
                    pomv.RecurringSalesId = po.RecurringSalesId;
                    pomv.GrandTotal = po.GrandTotal;
                    pomv.BCGrandTotal = po.BCGrandTotal;
                    pomv.UserId = po.UserId;
                    pomv.BranchId = po.BranchId;
                    pomv.CompanyId = po.CompanyId;
                    pomv.Status = po.Status;
                    pomv.Verify = po.Verify;
                    pomv.CreatedBy = po.CreatedBy;
                    pomv.CreatedOn = po.CreatedOn;
                    pomv.ModifiedBy = po.ModifiedBy;
                    pomv.ModifiedOn = po.ModifiedOn;
                    pomv.Memo = po.Memo;
                    var podlist = db.SalesDeliveryDetails.Where(p => p.SalesDeliveryId == po.Id).ToList();
                    foreach (var pod in podlist)
                    {
                        var podmv = new SalesDeliveryDetailModelView();
                        podmv.SalesDeliveryId = pod.SalesDeliveryId;
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
                    return View(pomv);
                }

                else
                {
                    SalesOrder po = db.SalesOrders.Find(id);
                    if (po == null)
                    {
                        return HttpNotFound();
                    }
                    SalesDeliveryModelView pomv = new SalesDeliveryModelView();
                    List<SalesDeliveryDetailModelView> podmvList = new List<SalesDeliveryDetailModelView>();

                    pomv.Id = 0;
                   // pomv.NO = po.NO;
                    pomv.CreatedFrom = "Sales Order";
                    pomv.ReferenceNo = po.Id;
                    pomv.OrderNo = po.NO;
                    if (po.Customer != null)
                    {
                        pomv.CustomerCode = po.Customer.Code;
                        pomv.CustomerName = po.Customer.Name;
                        pomv.CustomerId = po.CustomerId;
                    }
                    pomv.Reference = po.Reference;
                    pomv.SalesPerson = po.SalesPerson;
                    pomv.RecurringSalesId = po.RecurringSalesId;
                    pomv.PaymentTermId = po.PaymentTermId;
                    pomv.WarehouseId = po.WarehouseId;
                    pomv.CurrencyId = po.CurrencyId;
                    pomv.BaseCurrencyCode = po.Currency.ISO_4217;
                    pomv.Currencyrate = po.Currencyrate;
                    pomv.TransactionCurrency = po.TransactionCurrency;
                    pomv.TransactionCurrencyCode = po.Currency1.ISO_4217;
                    pomv.Date = po.Date.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DueDate = po.DueDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.ReceiptDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DespatchDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.DeliveryName = po.DeliveryName;
                    pomv.StreetPoBox = po.StreetPoBox;
                    pomv.Suburb = po.Suburb;
                    pomv.City = po.City;
                    pomv.StateRegion = po.StateRegion;
                    pomv.Country = po.Country;
                    pomv.PostalCode = po.PostalCode;
                    pomv.TaxTotal = po.TaxTotal;
                    pomv.TotalAmount = po.TotalAmount;
                    pomv.RecurringSalesId = po.RecurringSalesId;
                    pomv.GrandTotal = po.GrandTotal;
                    pomv.BCGrandTotal = po.BCGrandTotal;
                    pomv.UserId = po.UserId;
                    pomv.BranchId = po.BranchId;
                    pomv.CompanyId = po.CompanyId;
                    //pomv.Status = po.Status;
                    pomv.CreatedBy = po.CreatedBy;
                    pomv.CreatedOn = po.CreatedOn;
                    pomv.ModifiedBy = po.ModifiedBy;
                    pomv.ModifiedOn = po.ModifiedOn;
                    pomv.Memo = po.Memo;
                    var podlist = db.SalesOrderDetails.Where(p => p.SalesOrderId == po.Id).ToList();
                    foreach (var pod in podlist)
                    {
                        var podmv = new SalesDeliveryDetailModelView();
                        podmv.SalesDeliveryId = pod.SalesOrderId;
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
                        podmv.TotalQuantity = pod.Quantity ;
                        podmvList.Add(podmv);
                    }
                    ViewBag.podmvList = podmvList;
                    return View(pomv);
                }

            }
        }

        //
        // POST: /SalesDelivery/Create
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
        //    System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            string currencyval = string.Empty;
            var salesPerson = db.SalesPersons.Where(d => (d.UserId == userid && d.CompanyId == companyid)).Select(d => new { Id = d.Id, Name = d.Name }).ToList();
            ViewBag.ddlSalesPerson = salesPerson;
            var paymentTerms = db.PaymentTerms.Where(d => (d.CompanyId == companyid )).Select(d => new { Id = d.Id, PaymentTermDescription = d.PaymentTermDescription }).ToList();
            ViewBag.PaymentTerms = paymentTerms;
            ViewBag.unit = db.UOMs.Where(d => (d.UserId == userid && d.CompanyId == companyid)).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => (d.UserId == userid && d.CompanyId == companyid)).ToList();


            SalesDeliveryModelView pomv = new SalesDeliveryModelView();
            List<SalesDeliveryDetailModelView> podmvList = new List<SalesDeliveryDetailModelView>();
            var sid = poCollection["CustomerId"];
            var tid = poCollection["PaymentTermId"];
            var wid = poCollection["WarehouseId"];
            var rno = poCollection["ReferenceNo"];
            var rsi = poCollection["RecurringSalesId"];
            var totamt = poCollection["TotalAmount"];
            var grandtot = poCollection["GrandTotal"];
            var bcgrandtot = poCollection["BCGrandTotal"];
            var verify = poCollection["Verify"];
            if (sid != "")
                pomv.CustomerId = Convert.ToInt32(sid);
            if (tid != "")
                pomv.PaymentTermId = Convert.ToInt32(tid);
            if (wid != "")
                pomv.WarehouseId = Convert.ToInt32(wid);
            if (rsi != "")
                pomv.RecurringSalesId = Convert.ToInt32(rsi);
            if (totamt != "")
                pomv.TotalAmount = Convert.ToDecimal(totamt);
            if (grandtot != "")
                pomv.GrandTotal = Convert.ToDecimal(grandtot);
            if (bcgrandtot != "")
                pomv.BCGrandTotal = Convert.ToDecimal(bcgrandtot);
            if (verify != "")
                pomv.Verify = Convert.ToInt32(verify);

            if (rno != "")
                pomv.ReferenceNo = Convert.ToInt64(poCollection["ReferenceNo"]);
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
            if (poCollection["InvoiceNo"] != "")
                pomv.InvoiceNo = Convert.ToInt64(poCollection["InvoiceNo"]);
            pomv.Id = Convert.ToInt64(poCollection["Id"]);
            pomv.NO = poCollection["NO"];
            pomv.CreatedFrom = poCollection["CreatedFrom"];

            if (pomv.ReferenceNo != null && pomv.NO == "")
                pomv.Id = 0;
            pomv.OrderNo = poCollection["OrderNo"];
            pomv.CustomerName = poCollection["CustomerName"];
            pomv.CustomerCode = poCollection["CustomerCode"];
            pomv.Reference = poCollection["Reference"];
            pomv.Date = Convert.ToString(poCollection["Date"]);
            pomv.DueDate = Convert.ToString(poCollection["DueDate"]);
            pomv.ReceiptDate = Convert.ToString(poCollection["ReceiptDate"]);
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
            var count = poCollection["producthide"].Split(',').Length;
            for (int i = 0; i < count; i++)
            {
                SalesDeliveryDetailModelView podmv = new SalesDeliveryDetailModelView();
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
                    case "disamounthide":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].DiscountAmount = Convert.ToDecimal(value[i]);
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
            var receiptdate = DateTime.ParseExact(pomv.ReceiptDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var despatchdate = DateTime.ParseExact(pomv.DespatchDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var getDateRange = db.FinancialYearMasters.Where(d => d.fYearID == Fyid).FirstOrDefault();

            if (!(receiptdate >= getDateRange.sDate && receiptdate <= getDateRange.eDate))
            {
                var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                ViewBag.ddlWarehouses = warehouses;
                ViewBag.podmvList = podmvList;
                ViewBag.Date = "Delivery Date out of scope of" + getDateRange.Year + "Financial Year.";
                return View(pomv);
            }
            if (checkCustomer == null)
            {
                //var warehouses = mc.getDdlWarehouses(companyid, Branchid);
                //ViewBag.ddlWarehouses = warehouses;

                //ViewBag.podmvList = podmvList;
                //ViewBag.Supply = "Customer does not exist!";
                //if (duedate < date)
                //    ViewBag.Date = "Delivery Date can not be less than Order Date ";
                //return View(pomv);
                pomv.CustomerId = null;
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
                pomv.CustomerId = checkCustomer.Id;
                pomv.TransactionCurrency = checkCustomer.CurrencyRate.CurrencyId;
                pomv.Currencyrate = checkCustomer.CurrencyRate.PurchaseRate;
               
                pomv.BCGrandTotal = pomv.Currencyrate * pomv.GrandTotal;
            }

            //purchase order details model view validation
            decimal quantityrequested = 0;
            foreach (var podmv in podmvList)
            {
                if (podmv.ItemId != 0 && podmv.Quantity > 0)
                {
                    quantityrequested += podmv.Quantity;
                    //do nothing
                }
                else
                {
                    if (!(podmv.ItemId == 0 && podmv.Quantity == 0 ))
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

            //    var quantityreceiveded = db.SalesDeliveryDetails.Where(p => p.SalesDelivery.CreatedFrom == "Sales Order" && p.SalesDelivery.ReferenceNo == pomv.ReferenceNo).Sum(p => (decimal?)p.Quantity) ?? 0;
            //    var quantityordered = db.SalesOrderDetails.Where(p => p.SalesOrder.Id == pomv.ReferenceNo).Sum(p => (decimal?)p.Quantity) ?? 0;
            //    if (quantityordered < quantityreceiveded + quantityrequested)
            //    {
            //        var warehouses = mc.getDdlWarehouses(companyid, Branchid);
            //        ViewBag.ddlWarehouses = warehouses;

            //        ViewBag.podmvList = podmvList;
            //        ViewBag.ErrorTag = "Total quantity of Items in Delivery Note cannot exceed total qauntity ordered in Sales Order " + pomv.OrderNo + " .";
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

                            //int countpo = db.SalesDeliveries.Where(p => p.CompanyId == companyid && p.BranchId == Branchid).Count();
                            //pomv.NO = tc.GenerateCode("DN", countpo);
                            var fyear = db.FinancialYearMasters.FirstOrDefault(f => f.fYearID == Fyid).Year;
                            var fs = fyear.Substring(2, 2);
                            var es = fyear.Substring(7, 2);
                            fyear = fs + "-" + es;
                            int countpo = 1;


                            if (db.SalesDeliveries.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.FinancialYearId == Fyid).Count() != 0)
                            {
                                countpo = (int)db.SalesDeliveries.Where(p => p.CompanyId == companyid && p.BranchId == Branchid && p.FinancialYearId == Fyid).Max(p => p.InvoiceNo) + 1;
                            }
                            var getPrefix = db.Prefixes.Where(p => p.DefaultPrefix == "SD" && p.CompanyId == companyid && p.BranchId == Branchid).Select(p => new { p.DefaultPrefix, p.SetPrefix }).FirstOrDefault();

                            if (getPrefix.SetPrefix != null)
                                pomv.NO = getPrefix.SetPrefix + "/" + fyear + "/" + countpo;//tc.GenerateCode(fyear, countpo);
                            else
                                pomv.NO = getPrefix.DefaultPrefix + "/" + fyear + "/" + countpo; //tc.GenerateCode(fyear, countpo);
                            //Insert into purchaseorder table
                            SalesDelivery po = new SalesDelivery();
                            po.NO = pomv.NO;
                            po.InvoiceNo = countpo;
                            po.CustomerId = pomv.CustomerId;
                            po.Reference = pomv.Reference;
                            po.PaymentTermId = pomv.PaymentTermId;
                            po.WarehouseId = pomv.WarehouseId;
                            po.Date = date;
                            po.DueDate = duedate;
                            po.ReceiptDate = receiptdate;
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
                            po.TotalAmount = pomv.TotalAmount;
                            po.GrandTotal = pomv.GrandTotal;
                            po.RecurringSalesId = pomv.RecurringSalesId;
                            po.SalesPerson = pomv.SalesPerson;
                            po.BCGrandTotal = pomv.BCGrandTotal;
                            po.FinancialYearId = Fyid;
                            po.CreatedBy = Createdby;
                            po.CreatedOn = DateTime.Now;
                            po.UserId = userid;
                            po.BranchId = Branchid;
                            po.CompanyId = companyid;
                            po.Status = pomv.Status;
                            po.Verify = 0;
                            po.Memo = pomv.Memo;
                            db.SalesDeliveries.Add(po);

                            db.SaveChanges();
                            var po1 = db.SalesDeliveries.Find(po.Id);
                            pomv.Id = po1.Id;
                            foreach (var podmv in podmvList)
                            {
                                if (podmv.ItemId != 0 && podmv.Quantity > 0)
                                {


                                    SalesDeliveryDetail pod = new SalesDeliveryDetail();
                                    pod.SalesDeliveryId = po.Id;
                                    pod.ItemId = podmv.ItemId;
                                    pod.BarCode = podmv.BarCode;
                                    pod.Description = podmv.Description;
                                    pod.Quantity = podmv.Quantity;
                                    pod.AccountId = 12;
                                    pod.UnitId = podmv.UnitId;
                                    pod.Price = podmv.Price;
                                    pod.CurrencyRate = po.Currencyrate;
                                    pod.Discount = podmv.Discount;
                                    pod.DiscountAmount = podmv.DiscountAmount;
                                    pod.UnitIdSecondary = podmv.UnitIdSecondary;
                                    pod.SecUnitId = podmv.SecUnitId;
                                    pod.UnitFormula = podmv.UnitFormula;
                                    pod.SecUnitFormula = podmv.SecUnitFormula;


                                    pod.TotalAmount = podmv.Price * podmv.Quantity - podmv.DiscountAmount * pod.Quantity;

                                    db.SalesDeliveryDetails.Add(pod);

                                    if (po.Status == "Saved")
                                    {
                                        var stock = new Stock();
                                        stock.ArticleID = podmv.ItemId;
                                        stock.Items = podmv.Quantity;
                                        // stock.Price = podmv.Price;
                                        stock.TranCode = "OUT";
                                        stock.TransTag = "SD";
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
                            if (pomv.CreatedFrom == "Sales Order")
                            {
                                po1.CreatedFrom = "Sales Order";
                                po1.ReferenceNo = pomv.ReferenceNo;
                                po1.OrderNo = pomv.OrderNo;
                                var findpo = db.SalesOrders.Find(pomv.ReferenceNo);
                                findpo.Status = InventoryConst.cns_Delivered;
                            }
                        }
                        else
                        {
                            //Update Sales Deliveries
                            var po = db.SalesDeliveries.Find(pomv.Id);
                            if (pomv.InvoiceNo != null)
                            {

                                var replacewith = Convert.ToString(pomv.InvoiceNo);
                                // var resultString = Regex.Match(replacewith, @"\d+").Value;
                                //   var result = Int64.Parse(resultString);

                                //  pomv.NO = Regex.Replace(po.NO, @"(?<=/)(\w+?)(?=/)", replacewith);
                               // var no = Regex.Match(po.NO, @"\/([0-9]+)(?=[^\/]*$)");
                               // var m = Regex.Match(po.NO, @"(\d+)[^-]*$");
                                pomv.NO = Regex.Replace(po.NO, @"([0-9]+)(?=[^\/]*$)", replacewith);
                                var duplicateInv = db.SalesDeliveries.Any(p => p.NO == pomv.NO && p.CompanyId == companyid);
                                if (duplicateInv)
                                {
                                    var warehouses1 = mc.getDdlWarehouses(companyid, Branchid);
                                    ViewBag.ddlWarehouses = warehouses1;

                                    ViewBag.podmvList = podmvList;

                                    ViewBag.Message = "Sales Challan No Cannot be duplicate.";
                                    return View(pomv);
                                }
                                po.InvoiceNo = pomv.InvoiceNo;

                            }
                            po.NO = pomv.NO;
                            po.CustomerId = pomv.CustomerId;
                            po.Reference = pomv.Reference;
                            po.PaymentTermId = pomv.PaymentTermId;
                            po.WarehouseId = pomv.WarehouseId;
                            po.Date = date;
                            po.DueDate = duedate;
                            po.ReceiptDate = receiptdate;
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
                            po.TotalAmount = pomv.TotalAmount;
                            po.GrandTotal = pomv.GrandTotal;
                            po.RecurringSalesId = pomv.RecurringSalesId;
                            po.SalesPerson = pomv.SalesPerson;
                            po.BCGrandTotal = pomv.BCGrandTotal;
                            po.FinancialYearId = Fyid;
                            po.ModifiedBy = Createdby;
                            po.ModifiedOn = DateTime.Now;
                            po.UserId = userid;
                            po.BranchId = Branchid;
                            po.CompanyId = companyid;
                            po.Status = pomv.Status;
                            po.Verify = pomv.Verify;
                            var podOldRecords = db.SalesDeliveryDetails.Where(p => p.SalesDeliveryId == po.Id).ToList();
                            foreach (var podOld in podOldRecords)
                            {
                                db.SalesDeliveryDetails.Remove(podOld);
                            }

                            foreach (var podmv in podmvList)
                            {
                                if (podmv.ItemId != 0 && podmv.Quantity > 0)
                                {


                                    SalesDeliveryDetail pod = new SalesDeliveryDetail();
                                    pod.SalesDeliveryId = po.Id;
                                    pod.ItemId = podmv.ItemId;
                                    pod.BarCode = podmv.BarCode;
                                    pod.Description = podmv.Description;
                                    pod.Quantity = podmv.Quantity;
                                    pod.AccountId = 12;
                                    pod.UnitId = podmv.UnitId;
                                    pod.Price = podmv.Price;
                                    pod.CurrencyRate = po.Currencyrate;
                                    pod.Discount = podmv.Discount;
                                    pod.DiscountAmount = podmv.DiscountAmount;
                                    pod.UnitIdSecondary = podmv.UnitIdSecondary;
                                    pod.SecUnitId = podmv.SecUnitId;
                                    pod.UnitFormula = podmv.UnitFormula;
                                    pod.SecUnitFormula = podmv.SecUnitFormula;
                                    pod.TotalAmount = podmv.Price * podmv.Quantity - podmv.DiscountAmount * pod.Quantity;
                                    db.SalesDeliveryDetails.Add(pod);

                                    if (po.Status == "Saved")
                                    {
                                        var stock = new Stock();
                                        stock.ArticleID = podmv.ItemId;
                                        stock.Items = podmv.Quantity;
                                        // stock.Price = podmv.Price;
                                        stock.TranCode = "OUT";
                                        stock.TransTag = "SD";
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
            var taxes1 = mc.getDdlTaxes(userid, companyid, Branchid);
            ViewBag.ddlTaxes = taxes1;
            ViewBag.podmvList = podmvList;
            ViewBag.Message = "You have successfully " + pomv.Status + "Delivery Note " + pomv.NO;
            return View(pomv);
            
        }

        //
        // GET: /SalesDelivery/Edit/5

        public ActionResult Edit(long id = 0)
        {
            SalesDelivery salesdelivery = db.SalesDeliveries.Find(id);
            if (salesdelivery == null)
            {
                return HttpNotFound();
            }
            ViewBag.CurrencyId = new SelectList(db.Currencies, "CurrencyId", "Country", salesdelivery.CurrencyId);
            ViewBag.TransactionCurrency = new SelectList(db.Currencies, "CurrencyId", "Country", salesdelivery.TransactionCurrency);
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "Code", salesdelivery.CustomerId);
         //   ViewBag.TaxId = new SelectList(db.Taxes, "TaxId", "Name", salesdelivery.TaxId);
            ViewBag.WarehouseId = new SelectList(db.Warehouses, "Id", "Code", salesdelivery.WarehouseId);
            return View(salesdelivery);
        }

        //
        // POST: /SalesDelivery/Edit/5

        [HttpPost]
        public ActionResult Edit(SalesDelivery salesdelivery)
        {
            if (ModelState.IsValid)
            {
                db.Entry(salesdelivery).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CurrencyId = new SelectList(db.Currencies, "CurrencyId", "Country", salesdelivery.CurrencyId);
            ViewBag.TransactionCurrency = new SelectList(db.Currencies, "CurrencyId", "Country", salesdelivery.TransactionCurrency);
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "Code", salesdelivery.CustomerId);
          //  ViewBag.TaxId = new SelectList(db.Taxes, "TaxId", "Name", salesdelivery.TaxId);
            ViewBag.WarehouseId = new SelectList(db.Warehouses, "Id", "Code", salesdelivery.WarehouseId);
            return View(salesdelivery);
        }

        //
        // GET: /SalesDelivery/Delete/5

        public ActionResult Delete(long id = 0)
        {
            SalesDelivery salesdelivery = db.SalesDeliveries.Find(id);
            if (salesdelivery == null)
            {
                return HttpNotFound();
            }
            return View(salesdelivery);
        }

        //
        // POST: /SalesDelivery/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            SalesDelivery salesdelivery = db.SalesDeliveries.Find(id);
            db.SalesDeliveries.Remove(salesdelivery);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult CreateSalesChallanPDF(long? id, long? Branchid, long? companyid, long? userid)
        {
            
            var culture = Convert.ToString(Session["DateCulture"]);
            var dateFormat = Convert.ToString(Session["DateFormat"]);
            SalesDeliveryModelView pomv = new SalesDeliveryModelView();
            List<SalesDeliveryDetailModelView> podmvList = new List<SalesDeliveryDetailModelView>();
         
            var taxes = db.Taxes.Where(d => (d.CompanyId == companyid && d.BranchId == Branchid) || (d.UserId == 0 && d.CompanyId == 0)).Select(d => new { TaxId = d.TaxId, Name = d.Name, Rate = d.Rate }).ToList();
            ViewBag.TaxSingle = taxes;

            var Taxcomponents = db.Taxrates.Where(d => (d.Tax.CompanyId == companyid && d.Tax.BranchId == Branchid) || (d.Tax.CompanyId == 0 && d.Tax.BranchId == 0)).Select(d => new { EffectiveTaxRate = d.EffectiveTaxRate, IsCompoundedTax = d.IsCompoundedTax, IsDependTax = d.IsDependTax, TaxCompId = d.TaxCompId, TaxId = d.TaxId, TaxrateId = d.TaxrateId, Taxrate1 = d.Taxrate1 }).OrderBy(t => t.TaxrateId).ToList();
            ViewBag.Taxcomponents = Taxcomponents;

            SalesDelivery po = db.SalesDeliveries.Find(id);
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

            var podlist = db.SalesDeliveryDetails.Where(p => p.SalesDeliveryId == po.Id).ToList();
            foreach (var pod in podlist)
            {
                var podmv = new SalesDeliveryDetailModelView();
                podmv.SalesDeliveryId = pod.SalesDeliveryId;
                podmv.ItemId = pod.ItemId;
                podmv.ItemName = pod.Product.Description;
                podmv.Description = pod.Description;
                podmv.Quantity = pod.Quantity;
                podmv.AccountId = pod.AccountId;
                podmv.UnitId = pod.UnitId;
                podmv.UnitName = pod.UOM.Description;
                podmv.Price = pod.Price;
                podmv.CurrencyRate = pod.CurrencyRate;
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

            return View(po);

        }



        public ActionResult PrintSalesDeliveryPDF(long? id)
        {
            //return RedirectToAction("CreteSalesInvoicePDF", new { id = id });
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);

            return new ActionAsPdf("CreateSalesChallanPDF", new { id = id, Branchid = Branchid, companyid = companyid, userid = userid }) { FileName = "SalesChallanPrint.pdf" };
        }



        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
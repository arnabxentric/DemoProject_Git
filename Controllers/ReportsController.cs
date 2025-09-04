using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;

namespace XenERP.Controllers
{
    public class ReportsController : Controller
    {
        //
        // GET: /Reports/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AllReports()
        {
            return View();
        }


        public ActionResult SalesInvoiceTrack()
        {
            return View();
        }

        public ActionResult SalesInvoiceTrackDetails(string from, string to, int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                DateTime? FromDate = null;
                DateTime? ToDate = null;
                if (from != "0")
                {
                    FromDate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                }
                if (to != "0")
                {
                    ToDate = DateTime.ParseExact(to, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                }
                var salesinvoice = db.SalesInvoices.Where(r => r.InvoiceDate >= FromDate && r.InvoiceDate <= ToDate && r.CustomerId == id).ToList();
                var cust = db.Customers.Where(r => r.Id == id).FirstOrDefault();
                var openingBalance = db.OpeningBalancePayments.Where(r => r.LedgerId == cust.LId).FirstOrDefault();

                List<SalesPurchaseReportModelView> modelList = new List<SalesPurchaseReportModelView>();
                if (openingBalance != null)
                {
                    var openingBalPay = db.OpBalancePaymentDetails.Where(r => r.OpBalanceId == openingBalance.Id).ToList();
                    decimal? PaidAmount = 0;
                    SalesPurchaseReportModelView list = new SalesPurchaseReportModelView();
                    list.Date = "01/04/2024";
                    list.InvoiceNo = "Opening";
                    list.InvoiceAmount = openingBalance.OpeningBalance;
                    List<PaymentReceiveDetails> receiveList = new List<PaymentReceiveDetails>();
                    if (openingBalPay.Count() > 0)
                    {
                        foreach (var item in openingBalPay)
                        {
                            PaymentReceiveDetails receive = new PaymentReceiveDetails();
                            receive.Date = Convert.ToDateTime(item.ReceiptPayment.RPDatetime).ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                            receive.AmountPaid = item.PaidAmount;
                            receive.InvoiceNo = item.ReceiptPayment.Prefix + Convert.ToString(item.ReceiptPayment.VoucherNo);
                            PaidAmount = PaidAmount + item.PaidAmount;
                            receiveList.Add(receive);
                        }
                    }
                    list.TotalPaidAmount = PaidAmount;
                    list.paymentReceiveDetailsList = receiveList;
                    modelList.Add(list);
                }
                foreach (var item in salesinvoice)
                {
                    decimal? PaidAmount = 0;
                    SalesPurchaseReportModelView list = new SalesPurchaseReportModelView();
                    var salesInvoPay = db.BillWiseReceives.Where(b => b.InvoiceId == item.Id).ToList();
                    list.Date = Convert.ToDateTime(item.InvoiceDate).ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    list.InvoiceNo = item.NO + "--" + item.Reference;
                    list.InvoiceAmount = item.GrandTotal;
                    List<PaymentReceiveDetails> receiveList = new List<PaymentReceiveDetails>();
                    if (salesInvoPay.Count() > 0)
                    {
                        foreach (var item1 in salesInvoPay)
                        {
                            PaymentReceiveDetails receive = new PaymentReceiveDetails();
                            receive.Date = Convert.ToDateTime(item1.ReceiptPayment.RPDatetime).ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                            receive.AmountPaid = item1.Paid;
                            receive.InvoiceNo = item1.ReceiptPayment.Prefix + Convert.ToString(item1.ReceiptPayment.VoucherNo);
                            PaidAmount = PaidAmount + item1.Paid;
                            receiveList.Add(receive);
                        }
                    }
                    list.TotalPaidAmount = PaidAmount;
                    list.paymentReceiveDetailsList = receiveList;
                    modelList.Add(list);
                }
                ViewBag.from = from;
                ViewBag.to= to;
                ViewBag.customerName = cust.Name;
                return View(modelList);
            }
        }


        public ActionResult PurchaseInvoiceTrack()
        {
            return View();
        }

        public ActionResult PurchaseInvoiceTrackDetails(string from, string to, int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                var culture = Session["DateCulture"].ToString();
                string dateFormat = Session["DateFormat"].ToString();
                DateTime? FromDate = null;
                DateTime? ToDate = null;
                if (from != "0")
                {
                    FromDate = DateTime.ParseExact(from, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                }
                if (to != "0")
                {
                    ToDate = DateTime.ParseExact(to, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                }
                var purchaseinvoice = db.PurchaseInvoices.Where(r => r.InvoiceDate >= FromDate && r.InvoiceDate <= ToDate && r.SupplierId == id).ToList();
                var sup = db.Suppliers.Where(r => r.Id == id).FirstOrDefault();
                var openingBalance = db.OpeningBalancePayments.Where(r => r.LedgerId == sup.LId).FirstOrDefault();

                List<SalesPurchaseReportModelView> modelList = new List<SalesPurchaseReportModelView>();
                if (openingBalance != null)
                {
                    var openingBalPay = db.OpBalancePaymentDetails.Where(r => r.OpBalanceId == openingBalance.Id).ToList();
                    decimal? PaidAmount = 0;
                    SalesPurchaseReportModelView list = new SalesPurchaseReportModelView();
                    list.Date = "01/04/2024";
                    list.InvoiceNo = "Opening";
                    list.InvoiceAmount = openingBalance.OpeningBalance;
                    List<PaymentReceiveDetails> receiveList = new List<PaymentReceiveDetails>();
                    if (openingBalPay.Count() > 0)
                    {
                        foreach (var item in openingBalPay)
                        {
                            PaymentReceiveDetails receive = new PaymentReceiveDetails();
                            receive.Date = Convert.ToDateTime(item.ReceiptPayment.RPDatetime).ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                            receive.AmountPaid = item.PaidAmount;
                            receive.InvoiceNo = item.ReceiptPayment.Prefix + Convert.ToString(item.ReceiptPayment.VoucherNo);
                            PaidAmount = PaidAmount + item.PaidAmount;
                            receiveList.Add(receive);
                        }
                    }
                    list.TotalPaidAmount = PaidAmount;
                    list.paymentReceiveDetailsList = receiveList;
                    modelList.Add(list);
                }
                foreach (var item in purchaseinvoice)
                {
                    decimal? PaidAmount = 0;
                    SalesPurchaseReportModelView list = new SalesPurchaseReportModelView();
                    var purchaseInvoPay = db.BillWisePayments.Where(b => b.InvoiceId == item.Id).ToList();
                    list.Date = Convert.ToDateTime(item.InvoiceDate).ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    list.InvoiceNo = item.NO + "--" + item.Reference;
                    list.InvoiceAmount = item.GrandTotal;
                    List<PaymentReceiveDetails> receiveList = new List<PaymentReceiveDetails>();
                    if (purchaseInvoPay.Count() > 0)
                    {
                        foreach (var item1 in purchaseInvoPay)
                        {
                            PaymentReceiveDetails receive = new PaymentReceiveDetails();
                            receive.Date = Convert.ToDateTime(item1.ReceiptPayment.RPDatetime).ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                            receive.AmountPaid = item1.Paid;
                            receive.InvoiceNo = item1.ReceiptPayment.Prefix + Convert.ToString(item1.ReceiptPayment.VoucherNo);
                            PaidAmount = PaidAmount + item1.Paid;
                            receiveList.Add(receive);
                        }
                    }
                    list.TotalPaidAmount = PaidAmount;
                    list.paymentReceiveDetailsList = receiveList;
                    modelList.Add(list);
                }
                ViewBag.from = from;
                ViewBag.to = to;
                ViewBag.SupplierName = sup.Name;
                return View(modelList);
            }
        }

    }
}

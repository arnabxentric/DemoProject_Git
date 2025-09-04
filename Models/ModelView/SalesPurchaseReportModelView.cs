using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XenERP.Models
{
    public class SalesPurchaseReportModelView
    {
        public string Date { get; set; }
        public string InvoiceNo { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public decimal? TotalPaidAmount { get; set; }
        public List<PaymentReceiveDetails> paymentReceiveDetailsList { get; set; }
    }

    public class PaymentReceiveDetails
    {
        public string InvoiceNo { get; set; }
        public string Date { get; set; }
        public decimal? AmountPaid { get; set; }
    }
}
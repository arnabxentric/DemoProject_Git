using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace XenERP.Models
{
    public class PurchaseCostingDetailModelView
    {
        public long Id { get; set; }
        public long PurchaseInvoiceId { get; set; }
        public long CostingId { get; set; }
        public string CostingType { get; set; }
        public string Description { get; set; }
        public long TaxId { get; set; }
        public decimal CurrencyRate { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CostAmount { get; set; }
       

        public string CostName { get; set; }
        public string TaxName { get; set; }
    }
}
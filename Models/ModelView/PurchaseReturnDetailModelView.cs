using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XenERP.Models
{
    public class PurchaseReturnDetailModelView
    {
        public long Id { get; set; }
        public long PurchaseReturnId { get; set; }
        public long ItemId { get; set; }
        public string BarCode { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public long UnitId { get; set; }
        public decimal Price { get; set; }
        public int AccountId { get; set; }
        public decimal CurrencyRate { get; set; }
        public long TaxId { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal TaxAmount { get; set; }
        public Nullable<int> TaxType { get; set; }
        public decimal TotalAmount { get; set; }
        public int FinancialYearId { get; set; }

        public string ItemName { get; set; }
        public string TaxName { get; set; }
        public string UnitName { get; set; }
        public string Available { get; set; }
    }
}
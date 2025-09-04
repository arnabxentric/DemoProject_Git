using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace XenERP.Models
{
    public class SalesOrderDetailModelView
    {
        public long Id { get; set; }
        public long SalesOrderId { get; set; }
        public long ItemId { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public long UnitId { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public int AccountId { get; set; }
        public decimal CurrencyRate { get; set; }
        public long TaxId { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal TaxAmount { get; set; }
        public int TaxType { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string BarCode { get; set; }
        public string TaxName { get; set; }
        public string UnitName { get; set; }
        public string Available { get; set; }

        public long UnitIdSecondary { get; set; }
        public Nullable<long> SecUnitId { get; set; }
        public decimal UnitFormula { get; set; }
        public Nullable<decimal> SecUnitFormula { get; set; }
        public string UnitSecondaryName { get; set; }
        public string SecUnitName { get; set; }
        public decimal TotalQuantity { get; set; }
    }
}
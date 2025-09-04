using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace XenERP.Models
{
    public class SalesInvoiceModelView
    {
        public long Id { get; set; }
        public long SalesInvoiceId { get; set; }
        [Required(ErrorMessage = "The Customer Name field is required.")]
        public long? CustomerId { get; set; }
     //   [Required(ErrorMessage = "The Customer Code field is required.")]
        public string CustomerCode { get; set; }
       
        public string CustomerName { get; set; }
        public string CreatedFrom { get; set; }
        public Nullable<long> ReferenceNo { get; set; }
        public string OrderNo { get; set; }
        public string SalesOrderNo { get; set; }
        public string Reference { get; set; }
        [Required(ErrorMessage = "Invoice date is required.")]
        public string Date { get; set; }
        public string DueDate { get; set; }
        public string InvoiceDate { get; set; }
        public string DespatchDate { get; set; }
        public string DespatchNo { get; set; }
        public string DespatchThrough { get; set; }
        public string DespatchDestination { get; set; }
        public int CurrencyId { get; set; }
        public int TransactionCurrency { get; set; }
        public string CurrencyName { get; set; }
        public string TransactionCurrencyName { get; set; }
        public string BaseCurrencyCode { get; set; }
        public string TransactionCurrencyCode { get; set; }
        public decimal Currencyrate { get; set; }
        public int FinancialYearId { get; set; }
        public long? RecurringSalesId { get; set; }
        public int ApproveStatus { get; set; }
        public Nullable<long> PaymentTermId { get; set; }
        public bool CompoundedDis { get; set; }
        public Nullable<bool> DisApplicable { get; set; }
        public Nullable<bool> FD1Applicable { get; set; }
        public Nullable<bool> FD2Applicable { get; set; }
        public Nullable<bool> FD3Applicable { get; set; }
        public Nullable<bool> FD4Applicable { get; set; }
        public Nullable<decimal> Dis { get; set; }
        public Nullable<decimal> FD1 { get; set; }
        public Nullable<decimal> FD2 { get; set; }
        public Nullable<decimal> FD3 { get; set; }
        public Nullable<decimal> FD4 { get; set; }
        public Nullable<decimal> DisAmount { get; set; }

        public Nullable<long> InvoiceNo { get; set; }
        public Nullable<decimal> FD1Amount { get; set; }
        public Nullable<decimal> FD2Amount { get; set; }
        public Nullable<decimal> FD3Amount { get; set; }
        public Nullable<decimal> FD4Amount { get; set; }
        public Nullable<decimal> DiscountPerUnit { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxProduct { get; set; }
        public decimal TaxOther { get; set; }
        public decimal TotalAddAmount { get; set; }
        public decimal TotalDeductAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal BCGrandTotal { get; set; }
        public Nullable<long> LID { get; set; }
        public long TaxId { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public string NO { get; set; }
        public string Memo { get; set; }
        public string Type { get; set; }
        [Required(ErrorMessage = "Please choose a Warehouse field is required.")]
        public long WarehouseId { get; set; }
        public string DeliveryName { get; set; }
        public string StreetPoBox { get; set; }
        public string Suburb { get; set; }
        public string City { get; set; }
        public string StateRegion { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public Nullable<int> ShedId { get; set; }
        
        public Nullable<int> Mode { get; set; }
        public string TransNo { get; set; }
        public Nullable<long> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<long> SalesPerson { get; set; }
        public string Status { get; set; }
        public long UserId { get; set; }
        public long CompanyId { get; set; }
        public long BranchId { get; set; }
        public List<SalesInvoiceItemDetail>salesInvoiceItemDetails { get; set; }
    }


    public class SalesInvoiceItemDetail
    {
        public string ItemName { get; set; }
        public long ItemId { get; set; }
        public decimal Rate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal FreeQuantity { get; set; }
    }
}
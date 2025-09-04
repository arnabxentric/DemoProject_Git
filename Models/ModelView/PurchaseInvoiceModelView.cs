using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace XenERP.Models
{
    public class PurchaseInvoiceModelView
    {
        public long Id { get; set; }
        public long PurchaseInvoiceId { get; set; }
        public long? SupplierId { get; set; }
        [Required(ErrorMessage = "The Supplier Code field is required.")]
        public string SupplierCode { get; set; }
        [Required(ErrorMessage = "The Supplier Name field is required.")]
        public string SupplierName { get; set; }
        public string CreatedFrom { get; set; }
        public Nullable<long> ReferenceNo { get; set; }


        public Nullable<long> PurchaseOrderId { get; set; }

        public string PurchaseOrderNo { get; set; }
        public string OrderNo { get; set; }
        public string ReceiptIds { get; set; }
        public string Reference { get; set; }
        public string Date { get; set; }
        public string DueDate { get; set; }
        public long LID { get; set; }
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
        public int RecurringSalesId { get; set; }
        public int ApproveStatus { get; set; }
        public Nullable<long> PaymentTermId { get; set; }
        public decimal TaxProduct { get; set; }
        public decimal TaxOther { get; set; }

        public decimal RoundOff { get; set; }

        public decimal SubTotal { get; set; }
        public decimal TotalAddAmount { get; set; }
        public decimal TotalDeductAmount { get; set; }
       
        public decimal GrandTotal { get; set; }
        public decimal BCGrandTotal { get; set; }
        
        public Nullable<bool> IsDeleted { get; set; }
        [Required(ErrorMessage = "The Invoice No field is required.")]
        public string NO { get; set; }
        public string ReferenceInvoice { get; set; }
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
        public Nullable<long> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public string Status { get; set; }
        public long UserId { get; set; }
        public long CompanyId { get; set; }
        public long BranchId { get; set; }
    }
}
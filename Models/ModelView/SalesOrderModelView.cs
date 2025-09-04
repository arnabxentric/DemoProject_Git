using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace XenERP.Models
{
    public class SalesOrderModelView
    {
        public long Id { get; set; }
        public long? CustomerId { get; set; }
      //  [Required(ErrorMessage = "The Customer Code field is required.")]
        public string CustomerCode { get; set; }
     //   [Required(ErrorMessage = "The Customer Name field is required.")]
        public string CustomerName { get; set; }
        public string CreatedFrom { get; set; }
        public Nullable<long> ReferenceNo { get; set; }
        public string OrderNo { get; set; }
        public string Reference { get; set; }
        public string Date { get; set; }
        public string DueDate { get; set; }
        public int CurrencyId { get; set; }
        public int TransactionCurrency { get; set; }
        public string TransactionCurrencyCode { get; set; }
        public string BaseCurrencyCode { get; set; }
        public decimal Currencyrate { get; set; }
        public int FinancialYearId { get; set; }
        public long? RecurringSalesId { get; set; }
        public int ApproveStatus { get; set; }
        
        public decimal TaxTotal { get; set; }
        
        public decimal TotalAmount { get; set; }
        public Nullable<long> InvoiceNo { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal BCGrandTotal { get; set; }
        public Nullable<long> PaymentTermId { get; set; }
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
        public Nullable<long> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<long> SalesPerson { get; set; }
        public string Status { get; set; }
        public long UserId { get; set; }
        public long CompanyId { get; set; }
        public long BranchId { get; set; }
    }
}
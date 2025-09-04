using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using XenERP.Models;

namespace XenERP.Models
{
    public class SalesReceiveModelView
    {
        public long Id { get; set; }
        public long SalesInvoiceId { get; set; }
        public long CustomerId { get; set; }
   
        [Required(ErrorMessage = "The Customer Code field is required.")]
        public string CustomerCode { get; set; }
        [Required(ErrorMessage = "The Customer Name field is required.")]
        public string CustomerName { get; set; }
        public string Reference { get; set; }
        public string Date { get; set; }
        public string DueDate { get; set; }
        public System.DateTime InvoiceDate { get; set; }
        public int CurrencyId { get; set; }
        public int TransactionCurrency { get; set; }

        public string PANNo { get; set; }
        public string CurrencyName { get; set; }
        public string TransactionCurrencyName { get; set; }
        public string BaseCurrencyCode { get; set; }
        public string TransactionCurrencyCode { get; set; }
        public decimal Currencyrate { get; set; }
        public int FinancialYearId { get; set; }
        public int RecurringSalesId { get; set; }
        public int ApproveStatus { get; set; }
        public long CustomerDiscountLedger { get; set; }
   
        public decimal CustomerDiscountPercent { get; set; }
        public decimal CustomerDiscountAmount { get; set; }
        public decimal BCCustomerDiscountAmount { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal AddTaxTotal { get; set; }
        public decimal DeductTaxTotal { get; set; }

        public decimal BCTaxTotal { get; set; }
        public decimal BCAddTaxTotal { get; set; }
        public decimal BCDeductTaxTotal { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAddAmount { get; set; }
        public decimal TotalDeductAmount { get; set; }
        public decimal BCTotalAmount { get; set; }
        public decimal BCTotalAddAmount { get; set; }
        public decimal BCTotalDeductAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal BCGrandTotal { get; set; }
        public long TaxId { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public string NO { get; set; }
        public string Memo { get; set; }
        public string Type { get; set; }
     
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







    
        public System.DateTime RPdate { get; set; }
        public System.DateTime RPDatetime { get; set; }
        public Nullable<int> fYearId { get; set; }
        public string RPType { get; set; }
        public Nullable<int> RPCashId { get; set; }
        public Nullable<int> ledgerId { get; set; }
        public string chequeNo { get; set; }
        public Nullable<System.DateTime> chequeDate { get; set; }
     
        public string transactionType { get; set; }
        public Nullable<decimal> RPCashAmount { get; set; }
        public Nullable<int> RPBankId { get; set; }
        public Nullable<decimal> RPBankAmount { get; set; }
        public string transactionNo { get; set; }
        public string Remarks { get; set; }
        public Nullable<int> VoucherNo { get; set; }
        public string CreditCardNo { get; set; }
        public string ExpirayDate { get; set; }
        public string CardName { get; set; }
        public Nullable<decimal> CreditCardAmt { get; set; }
        public Nullable<System.DateTime> PaymentDate { get; set; }
        public Nullable<decimal> CurrencyAmount { get; set; }
        public bool ReconStatus { get; set; }
        public Nullable<System.DateTime> ReconDate { get; set; }
    










    }
}
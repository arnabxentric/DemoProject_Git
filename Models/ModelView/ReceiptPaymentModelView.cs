using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace XenERP.Models
{
    public class ReceiptPaymentModelView
    {
        public int Id { get; set; }
        public Nullable<int> VoucherId { get; set; }
        public System.DateTime RPdate { get; set; }
        [Required(ErrorMessage = "Please enter money receipt number")]
        public string MoneyReceiptNo { get; set; }
        public System.DateTime RPDatetime { get; set; }
        public Nullable<int> fYearId { get; set; }
        public string RPType { get; set; }
        public Nullable<int> RPCashId { get; set; }
       
        public string chequeNo { get; set; }
        public Nullable<System.DateTime> chequeDate { get; set; }

        public string cdate { get; set; }
        public string chequeDetails { get; set; }
        [Required(ErrorMessage = "Please Enter Receive Amount.")]
        public Nullable<decimal> TotalAmount { get; set; }
        public Nullable<bool> IsTCS { get; set; }
        public Nullable<decimal> TCSType { get; set; }
        public string transactionType { get; set; }
        public Nullable<decimal> RPCashAmount { get; set; }
        [Required(ErrorMessage = "Bank name is Mandatory.")]
        public Nullable<int> RPBankId { get; set; }
        public Nullable<decimal> RPBankAmount { get; set; }
        public Nullable<decimal> RPNEFTAmount { get; set; }
        [Required(ErrorMessage = "The Mode of Pay field is Mandatory.")]
        public string ModeOfPay { get; set; }
        public string transactionNo { get; set; }
        public string Remarks { get; set; }
        public bool IsBillWise { get; set; }
        public string Prefix { get; set; }
        public Nullable<int> VoucherNo { get; set; }
        public string CreditCardNo { get; set; }
        public string ExpirayDate { get; set; }
        public string CardName { get; set; }
        public Nullable<decimal> CreditCardAmt { get; set; }
        public Nullable<System.DateTime> PaymentDate { get; set; }
        public string NeftRtgsNo { get; set; }
        public Nullable<decimal> CurrencyAmount { get; set; }
        public bool ReconStatus { get; set; }
        public Nullable<System.DateTime> ReconDate { get; set; }
        public Nullable<int> CompanyId { get; set; }
        [Required(ErrorMessage = "The Branch field is Mandatory.")]
        public Nullable<int> BId { get; set; }
        //public Nullable<int> BranchId { get; set; }
        public Nullable<int> UserId { get; set; }

        public Nullable<long> CreatedBy { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<bool> IsDeleted { get; set; }

        public List<ReceiptPaymentDetailModelView> RPDetails { get; set; }
        public List<InvoiceDue> BillList { get; set; }

        [Required(ErrorMessage = "Receive Date Is Required.")]
        public string ReceiptDate { get; set; }
        [Required(ErrorMessage = "Customer Name Is Required.")]
        public int? ledgerId { get; set; }
        public string CustomerName { get; set; }
    }
}
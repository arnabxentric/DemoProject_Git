using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace XenERP.Models
{
    public class Genral
    {
        public class Awaitingpayment
        {
            public string Salesinvoiceid { get; set; }
            public string Invoiceno { get; set; }
            public long customername { get; set; }
            public DateTime? Date { get; set; }
            public DateTime? DueDate { get; set; }

            public Decimal? Paid { get; set; }


            public Decimal? Due { get; set; }


        }


        public class ProfileViewModel
        {
            [UIHint("ProfileImage")]
            public string ImageUrl { get; set; }
        }


        public class currencyratess
        {
            [Required(ErrorMessage = "Please select a currency")]
            public string Currencyselection { get; set; }
            public int Currencyid { get; set; }
            public string Currencyname { get; set; }
            public string Currencydet { get; set; }


            public decimal Sellprice { get; set; }

            public decimal Purchaseprice { get; set; }

        }


        public class Rates
        {

            public string code { get; set; }
            public int countrycode { get; set; }
            public int description { get; set; }

        }


        public class Login
        {

            [Required(ErrorMessage = "User ID is required")]
            [Remote("CheckUserLogin", "Home", HttpMethod = "Post", ErrorMessage = "User ID not found", AdditionalFields = "Id")]
            public string UserName { get; set; }




            [Required(ErrorMessage = "Password is required")]
            [Remote("CheckPassword", "Home", HttpMethod = "Post", ErrorMessage = "Password not found", AdditionalFields = "UserName")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

        }

    }
    public class menuuser
    {

        public string Name { get; set; }
        public long Id { get; set; }
        public long Id2 { get; set; }

        public int? AccessMenuHeaderId { get; set; }
    }


    public static class GenRandom
    {

        public static int GetRandom()
        {
            Random rn = new Random();
            return rn.Next(1000000, 9999999);
        }

    }
    public class Taxname
    {
        public long Id { get; set; }
        public string Name { get; set; }


    }

    public class DateCulture
    {
        public string Name { get; set; }
        public string ShortDatePattern { get; set; }
        public string DisplayName { get; set; }
        public string ShortTimePattern { get; set; }
    }
    public static class InventoryMessage
    {
        public const string Save = "Data Saved Successfully..";
        public const string Delte = "Row Deleted Successfully..";
        public const string Edit = "Row Updated Successfully..";
        public const string UpdateError = "An error occurred when saving changes.Please Try Again..";
        public const string InsertError = "An error occurred saving data.Please Try Again..";

    }
    public static class InventoryConst
    {
        public const string Cns_Payment = "Payment";
        public const string Cns_Receive = "Receive";
        public const string Cns_Active = "Active";
        public const string Cns_Inactive = "Inactive";
        public const string cns_Contra = "Contra";
        public const string cns_PayMode_Cash = "Cash";
        public const string cns_Saved_As_Draft = "Saved As Draft";
        public const string cns_Saved = "Saved";
        public const string cns_Partially_Received = "Partially Received";
        public const string cns_Received = "Received";
        public const string cns_Delivered = "Delivered";
        public const string cns_Received_Invoiced = "Received & Invoiced";
        public const string cns_Delivered_Invoiced = "Delivered & Invoiced";
        public const string cns_Invoiced = "Invoiced";
        public const string cns_Partially_Invoiced = "Partially Invoiced";
        public const string cns_Ordered = "Ordered";
        public const string cns_General_Receive = "General Receive";
        public const string cns_General_Payment = "General Payment";
    }


    public enum InventoryLedgergroupId
    {

        Member = 14, // ledger group id 
        Client = 12,// ledger group id 
        OpenBalanc = 0,
        GLCustomer = 16,// ledger group id 
        Cash = 3, // cash ledger id
        Interest = 4, // Interest receive id 
        Salary = 7, // Salary account ledger 


    }
    public class Ledger
    {

        public string Name { get; set; }
        public long Id { get; set; }

    }

    public class Financial
    {

        public DateTime? Startdate { get; set; }
        public DateTime? EndDate { get; set; }

    }
    public class TaxComponent
    {
        public long TaxId { get; set; }
        public decimal Amount { get; set; }


    }
    public class TaxComponent_Ret
    {
        public string TaxName { get; set; }
        public decimal Amount { get; set; }


    }
    public class InvoiceTotal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }
    public class CustomerwiseInvoice
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string InvoiceNo { get; set; }
        public int? VoucherNo { get; set; }
        public decimal InvAmount { get; set; }
        public decimal DueAmount { get; set; }
        public decimal PaidAmount { get; set; }

        public decimal Advance { get; set; }

    }
    public class Salesdate
    {

        public DateTime Date { get; set; }
        public decimal? Amount { get; set; }


    }
    public class POGeneral
    {

        public string Name { get; set; }
        public long? Id { get; set; }

    }
    public class PurchaseOrderVariance
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal POQuantity { get; set; }
        public decimal POPrice { get; set; }
        public decimal PRQuantity { get; set; }
        public decimal PIQuantity { get; set; }
        public decimal PIPrice { get; set; }

    }
    public class PurchaseOrderVarianceDetail
    {
        public long? POId { get; set; }
        public string PONO { get; set; }
        public DateTime? PODate { get; set; }
        public string POStatus { get; set; }
        public long? CHId { get; set; }
        public string CHNO { get; set; }
        public DateTime? CHDate { get; set; }
        public string PartyCHNO { get; set; }
        public string CHStatus { get; set; }
        public string TruckNO { get; set; }
        public long? InvoiceId { get; set; }
        public string InvoiceNO { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string PartyInvoiceNO { get; set; }
        public string Party { get; set; }
        public string ReferenceParty { get; set; }
        public string Warehouse { get; set; }
        public long BranchId { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal POQuantity { get; set; }
        public decimal POPrice { get; set; }
        public decimal PRQuantity { get; set; }
        public decimal PIQuantity { get; set; }
        public decimal PIPrice { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal AddedCost { get; set; }
        public decimal Deduction { get; set; }
        public decimal RoundOff { get; set; }
        public decimal GrandTotal { get; set; }

    }
    public class SubLedger
    {
        public long LID { get; set; }
        public string LedgerName { get; set; }
        public string ParentLedgerName { get; set; }
        public string LedgerCode { get; set; }

        public decimal OpeningBalanceBranch { get; set; }
        public decimal OpeningBalance { get; set; }

    }
    public class CollectionExpense
    {
        public int LID { get; set; }
        public string LedgerName { get; set; }
        public string BranchName { get; set; }
        public decimal Amount { get; set; }
        public decimal Opening { get; set; }
        public decimal PurchaseAmount { get; set; }
        public decimal CashCollection { get; set; }
        public decimal BankCollection { get; set; }
        public string PaymentType { get; set; }
        public DateTime? Date { get; set; }

    }

    public class LoginModelView
    {

        public long? BranchId { get; set; }
        public string BranchName { get; set; }


        public List<BranchList> BranchListShow { get; set; }


    }
    public class BranchList
    {
        public string Name { get; set; }
        public long? Id { get; set; }
    }
    public class Story
    {
        public int StoryId { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }

        public virtual IEnumerable<Sentence> Sentences { get; set; } // one to many Story-Sentence
    }

    public class Sentence
    {
        public int Id { get; set; }
        public string SentenceText { get; set; }

        public virtual Audio Audio { get; set; } // one to one Sentence-Audio
        public virtual Image Image { get; set; } // one to one Sentence-Image
    }
    public class Image
    {
        [Key]

        public int Id { get; set; }
        public string ImageSelected { get; set; }
        public virtual Sentence Sentence { get; set; }
    }
    public class Audio
    {
        [Key]

        public int Id { get; set; }
        public string AudioSelected { get; set; }
        public virtual Sentence Sentence { get; set; }
    }
    public class OutstandingInvoice
    {
        public long LID { get; set; }
        public string LedgerName { get; set; }
        public decimal OpeningDue { get; set; }
        public decimal OpeningAdvance { get; set; }
        public decimal? AdjustmentShort { get; set; }
        public decimal? AdjustmentExtra { get; set; }
        public decimal Sales { get; set; }
        public decimal? Salesreturn { get; set; }
        public decimal? Creditnote { get; set; }
        public decimal? Receipt { get; set; }
        public decimal? Payment { get; set; }
    }

    public class Aging
    {

        public long LID { get; set; }
        public string PartyName { get; set; }
        public decimal Current { get; set; }
        public decimal Days_1_30 { get; set; }
        public decimal Days_31_60 { get; set; }
        public decimal Days_61_90 { get; set; }
        public decimal Days_91_365 { get; set; }

    }

    public class CustomerInvoice
    {

        public DateTime InvoiceDate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public long LID { get; set; }

        public long InvoiceId { get; set; }
        public int? DaysDue { get; set; }
        public decimal InvAmount { get; set; }


    }

    public class Bill
    {
        public long? InvId { get; set; }
        public string InvNo { get; set; }
        public DateTime? Date { get; set; }
        public bool Status { get; set; }
        public decimal? BillAmount { get; set; }
        public decimal ReturnAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Paid { get; set; }
        public decimal? Due { get; set; }

    }

    public class InvoiceDue
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal BillAmount { get; set; }
        public decimal Paid { get; set; }
        public decimal Due { get; set; }

    }
}
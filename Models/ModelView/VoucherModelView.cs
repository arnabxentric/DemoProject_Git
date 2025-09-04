using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XenERP.Models
{
    public class VoucherModelView
    {
        
        public int Id { get; set; }
        public DateTime? RPdate { get; set; }
        public int fYearId { get; set; }
        public string branchCode { get; set; }
        public string RPType { get; set; }
        public string ModeOfPay { get; set; }
        public int RPCashId { get; set; }
        public int ledgerId { get; set; }
        public string chequeNo { get; set; }
        public DateTime chqDate { get; set; }
        public string PaidTo { get; set; }

        public string NeftRtgsNo { get; set; }
        public decimal TotalAmount { get; set; }
        public string transactionType { get; set; }
        public decimal RPCashAmount { get; set; }
        public int RPBankId { get; set; }
        public decimal RPBankAmount { get; set; }
        public string RPBankName { get; set; }
        public string transactionNo { get; set; }
        public string remarks { get; set; }
        public string cashLedger { get; set; }
        public string Prefix { get; set; }
        public int? VoucherNo { get; set; }
        public bool reconst { get; set; }
        public string bankLedger { get; set; }
        public string GeneralLedger { get; set; }
        public int VoucherId { get; set; }
        public string MoneyReceiptNo { get; set; }
        public string UserName { get; set; }

    }
}
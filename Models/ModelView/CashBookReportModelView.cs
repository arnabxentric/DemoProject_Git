using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace XenERP.Models
{
    public class CashBookReportModelView
    {
        public System.DateTime RPdate { get; set; }
        public string ledgerName { get; set; }
        public Nullable<long> Lid { get; set; }
        public string transactionType { get; set; }
        public string RPType { get; set; }
        public Nullable<int> VoucherNo { get; set; }
        public Nullable<decimal> TotalAmount { get; set; }
        public Nullable<decimal> Debit { get; set; }
        public Nullable<decimal> Credit { get; set; }
        public Nullable<decimal> OpeningBalance { get; set; }
        public string DRCR { get; set; }
        public Nullable<decimal> DebitTotal { get; set; }
        public Nullable<decimal> CreditTotal { get; set; }
    }
}
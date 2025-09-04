using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace XenERP.Models
{
    public class DayBookReportModelView
    {
        public Nullable<System.DateTime> RPDate { get; set; }
        public string ledgerName { get; set; }
        public Nullable<long> Lid { get; set; }
        public Nullable<int> GroupTypeId { get; set; }
        public string transactionType { get; set; }
        public Nullable<decimal> Debit { get; set; }
        public Nullable<decimal> Credit { get; set; }
        public Nullable<long> VoucherNo { get; set; }
        public Nullable<decimal> DebitTotal { get; set; }
        public Nullable<decimal> CreditTotal { get; set; }
    }
}
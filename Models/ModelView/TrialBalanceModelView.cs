using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace XenERP.Models
{
    public class TrialBalanceModelView
    {
        public string Sortorder { get; set; }
        public Nullable<int> GroupId { get; set; }
        public Nullable<int> ParentGroupId { get; set; }
        public string GroupName { get; set; }
        public Nullable<int> LedgerID { get; set; }
        public string LedgerName { get; set; }
        public Nullable<decimal> DebitAmount { get; set; }
        public Nullable<decimal> CreditAmount { get; set; }
         public Nullable<decimal> DebitTotal { get; set; }
        public Nullable<decimal> CreditTotal { get; set; }
    }
}
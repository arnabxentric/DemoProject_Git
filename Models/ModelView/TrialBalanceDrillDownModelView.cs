using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XenERP.Models
{
    public class TrialBalanceDrillDownModelView
    {
        public List<AccountsBook_GroupWise_TrialBalance_Current_Result> TrialGroups { get; set; }
        public List<AccountsBook_GroupWise_TrialBalance_Current_Result> TrialLedgers { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XenERP.Models
{
    public class LedgerModelView
    {
        public long? Lid { get; set; } 
        public string LedgerName { get; set; }
        public string LedgerId { get; set; }
        public int GroupId { get; set; }
        public string LedgerType { get; set; }
        public int? FyearId { get; set; }
        public int? CompanyId { get; set; }
        public int? BranchId { get; set; }
        public int? UserId { get; set; }
        public decimal OpeningBalance { get; set; }
    }
}
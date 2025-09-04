using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XenERP.Models
{
    public class PurchaseRequestModelView
    {
        public long Id { get; set; }
        public string RequestNo { get; set; }

        public Nullable<long> SerialNo { get; set; }
        public string RequestDate { get; set; }
        public string ExpectedDate { get; set; }
        public int FinancialYearId { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }
        public long UserId { get; set; }
        public long CompanyId { get; set; }
        public long BranchId { get; set; }

        public Nullable<long> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
    }
}
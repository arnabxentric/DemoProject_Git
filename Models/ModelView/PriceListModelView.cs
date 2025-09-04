using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace XenERP.Models
{
    public class PriceListModelView
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public Nullable<decimal> SPTier1 { get; set; }
        public Nullable<decimal> SPTier2 { get; set; }
        public Nullable<decimal> SPTier3 { get; set; }
        public Nullable<decimal> SPTier4 { get; set; }
        public Nullable<decimal> SPTier5 { get; set; }
        public string BranchId { get; set; }
        public string CompanyId { get; set; }
        public string UserId { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public virtual Product Product { get; set; }
    }
}
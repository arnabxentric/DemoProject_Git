using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace XenERP.Models
{
    public partial class ProductOpeningModelView
    {
        public long Id { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public string DateString { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public Nullable<long> UnitId { get; set; }
        public string UnitName { get; set; }
        public Nullable<long> UnitIdSecondary { get; set; }
        public string UnitSecondaryName { get; set; }
        public Nullable<decimal> UOM_Size { get; set; }
        public decimal? OpeningPrimary { get; set; }
        public decimal OpeningSecondary { get; set; }
        public Nullable<decimal> Price { get; set; }
        public long UserId { get; set; }
        public long Branchid { get; set; }
        public long Companyid { get; set; }
        public int FinancialYearId { get; set; }

        public SelectList UnitSecondaryList { get; set; }
        public SelectList UnitList { get; set; }

    }

}

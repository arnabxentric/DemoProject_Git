using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XenERP.Models
{
    public class ProductExcelModelView
    {
      //  public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string GroupCode { get; set; }
        public string Description { get; set; }
        public Nullable<long> UnitId { get; set; }
        public string UnitName { get; set; }
        public Nullable<decimal> UnitFormula { get; set; }
        public Nullable<long> UnitIdSecondary { get; set; }
        public string UnitSecondaryName { get; set; }
        public Nullable<long> GroupId { get; set; }
        public Nullable<long> CategoryId { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? SalesPrice { get; set; }
    }
}
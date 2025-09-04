using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace XenERP.Models
{
    public class ProductModelView
    {

        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string GroupCode { get; set; }
        public string Description { get; set; }
        public Nullable<long> UnitId { get; set; }
        public string UnitName { get; set; }
        public Nullable<decimal> UnitFormula { get; set; }
        public Nullable<long> UnitIdSecondary { get; set; }
        public decimal Availability { get; set; }
        public Nullable<long> GroupId { get; set; }
        public Nullable<long> CategoryId { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? SalesPrice { get; set; }
        public Nullable<long> PurchaseTax { get; set; }
        public Nullable<long> SalesTax { get; set; }
        public bool IsStockProduct { get; set; }
        public bool IsAssembledProduct { get; set; }
        public bool IsComponentProduct { get; set; }
        public string PickOutProperty { get; set; }
        public string Productimage { get; set; }
        public Nullable<decimal> Weight { get; set; }
        public Nullable<decimal> Width { get; set; }
        public Nullable<decimal> Height { get; set; }
        public string Colour { get; set; }
        public Nullable<decimal> Depth { get; set; }
        public Nullable<decimal> CubicTotal { get; set; }
        public long Userid { get; set; }
        public long Branchid { get; set; }
        public long companyid { get; set; }
        public Nullable<long> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<bool> IsDeleted { get; set; }

        public SelectList UnitSecondaryList { get; set; }
        public SelectList UnitList { get; set; }
    }
}
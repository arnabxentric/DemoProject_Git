using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace XenERP.Models
{
    public class IssueModelView
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public long SupplierId { get; set; }
        //public System.DateTime IssueDate { get; set; }
        public string IssueDate { get; set; }
        public string Remarks { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public long FinancialYearId { get; set; }
        public long CompanyId { get; set; }
        public long UserId { get; set; }
        public Nullable<long> BranchId { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        [Required(ErrorMessage = "The Supplier Name field is required.")]
        public string SupplierName { get; set; }
         [Required(ErrorMessage = "The Supplier Code field is required.")]
        public string SupplierCode { get; set; }


        public List<IssueDetails> issueDetails { get; set; }
         
    }

    public class IssueDetails
    {
        public long IssueId { get; set; }
         public long WareHouseId { get; set; }
         public long ProductId { get; set; }
         public Nullable<decimal> QuantityPU { get; set; }
         public Nullable<decimal> QuantitySU { get; set; }
         public decimal UnitFormula { get; set; }
         public string ProductName { get; set; }
         public string WarehouseName { get; set; }
         public decimal AvailableItem { get; set; }
         public string primaryUnit { get; set; }
         public string secondaryUnit { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace XenERP.Models
{
    public class ProductionModelView
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public Nullable<long> QuantityInPU { get; set; }
        public Nullable<long> OuantityInSU { get; set; }
        public Nullable<decimal> UnitForm { get; set; }
        public Nullable<long> PlantId { get; set; }
        public Nullable<int> ShiftId { get; set; }
        public long CompanyId { get; set; }
        public long UserId { get; set; }
        public Nullable<long> BranchId { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }

        [Required(ErrorMessage = "The Product Name field is required.")]
        public string productName { get; set; }

        public string PrimaryUnit { get; set; }

        public string SecondaryUnit { get; set; }
        public long QuantityPU { get; set; }
        public long QuantitySU { get; set; }
        [Required(ErrorMessage = "The Plant Name field is required.")]
        public string PlantName { get; set; }
        public string PlantMnager { get; set; }
        public string productCode { get; set; }
        // [Required(ErrorMessage = "The Primary Unit field is required.")]
        public string primary { get; set; }
        //[Required(ErrorMessage = "The Secondary Unit field is required.")]
        public string secondary { get; set; }
        [Required(ErrorMessage = "The Unit Formula field is required.")]
        public Nullable<decimal> UnitFormula { get; set; }


    }
}
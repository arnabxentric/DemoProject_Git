using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataAnnotationsExtensions.ClientValidation;
using XenERP.Models;
using  System.ComponentModel.DataAnnotations;

namespace XenERP.Models
{

    [MetadataType(typeof(MetaDataProduct))]


    public  partial class Product
    {


    }

    public partial class MetaDataProduct
    {

        [Required(ErrorMessage = "Code Field is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "2-100 characters allowed")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Name Field is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "2-100 characters allowed")]
        public string Name { get; set; }

        //[Required(ErrorMessage = "Description Field is required")]
        //[StringLength(200, MinimumLength = 2, ErrorMessage = "2-200 characters allowed")]
        //public string Description { get; set; }
         

        [RegularExpression(@"^[+-]?(\d+|\.\d+|\d+\.\d+)$", ErrorMessage = "The field PurchasePrice must be a number.")]
        public decimal  PurchasePrice { get; set; }

       
        [RegularExpression(@"^[+-]?(\d+|\.\d+|\d+\.\d+)$", ErrorMessage = "The field PurchasePrice must be a number.")]
        public decimal SalesPrice { get; set; }

      
        public Nullable<long> CategoryId { get; set; }

        [Required(ErrorMessage = "Group Field Required")]
        public Nullable<long> GroupId { get; set; }

        [Required(ErrorMessage = "Unit Field Required")]
        public Nullable<long> UnitId { get; set; }

        [Required(ErrorMessage = "Unit Formula Field Required")]
        public Nullable<decimal> UnitFormula { get; set; }

        [Required(ErrorMessage = "Primary Unit Field Required")]
        public Nullable<long> UnitIdSecondary { get; set; }

        
       

    }
}
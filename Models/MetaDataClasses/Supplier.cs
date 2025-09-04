using System;

using System.Collections.Generic;
using System.Linq;
using System.Web;
using XenERP.Models;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions.ClientValidation;
using System.Web.Mvc;

namespace XenERP.Models
{
    [MetadataType(typeof(SupplierMetaData))]

    public partial class Supplier
    {
    }
    public partial class SupplierMetaData
    {
        [Required(ErrorMessage = "The Supplier Code field is required.")]
      //  [Remote("CheckSupplierCode", "Supplier", ErrorMessage = "Supplier Code already exists", HttpMethod = "post", AdditionalFields = "Id")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "1-8 characters allowed")]
        public string Code { get; set; }



        [Required(ErrorMessage = "The Supplier Name field is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "2-100 characters allowed")]
        public string Name { get; set; }




    }
}
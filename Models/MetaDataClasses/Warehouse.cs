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
    [MetadataType(typeof(WarehouseMetaData))]

    public partial class Warehouse
    {
    }
    public partial class WarehouseMetaData
    {
        [Required(ErrorMessage = "The Warehouse Name field is required.")]        
        [StringLength(100,MinimumLength=2,ErrorMessage="2-100 characters allowed")]
        public string Name { get; set; }


        [Required(ErrorMessage = "The Warehouse Code field is required.")]
        [Remote("CheckWarehouseCode", "Warehouse", HttpMethod = "post", ErrorMessage = "Warehouse Code already exists", AdditionalFields = "Id")]    
        [StringLength(100,MinimumLength=2,ErrorMessage="1-8 characters allowed")]
          public string Code { get; set; }



    }
}
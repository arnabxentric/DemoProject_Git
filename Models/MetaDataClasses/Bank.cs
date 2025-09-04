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
    [MetadataType(typeof(BankMetaData))]

    public partial class Bank
    {
    }
    public partial class BankMetaData
    {
        //[Required(ErrorMessage = "The Warehouse Code field is required.")]
        //[Remote("CheckWarehouseCode", "Warehouse", HttpMethod = "post", ErrorMessage = "Warehouse Code already exists", AdditionalFields = "Id")]
        //[StringLength(100, MinimumLength = 2, ErrorMessage = "1-8 characters allowed")]
        //public string Code { get; set; }



        [Required(ErrorMessage = "The Bank Name field is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "2-100 characters allowed")]
        public string Name { get; set; }




        [Required(ErrorMessage = "The Opening Balance field is required.")]
        public decimal OpeningBalance { get; set; }





    }
}
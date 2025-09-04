using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataAnnotationsExtensions.ClientValidation;
using XenERP.Models;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using System.Web.Mvc;

namespace XenERP.Models
{
    [MetadataType(typeof(MetaDataCustomer))]


    public partial class Customer
    {


    }

    public partial class MetaDataCustomer
    {

        [Required(ErrorMessage = "Customer Code is required")]
    //    [Remote("CheckCustomerCode", "Customer", ErrorMessage = "Customer Code already exists", HttpMethod = "post",AdditionalFields="Id")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "2-50 characters allowed")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Customer Name is required")]
        [RegularExpression("^[A-Za-z ]*$", ErrorMessage = "Customer Name is invalid")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "2-150 characters allowed")]
        public string Name { get; set; }

        public int? SalesPersonId { get; set; }

    }

}
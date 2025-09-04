using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XenERP.Models;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions.ClientValidation;

namespace XenERP.Models
{

    [MetadataType(typeof(MetaDataCompany))]



    public partial class Company
    {

    }


    public partial class MetaDataCompany
    {

        [Required(ErrorMessage = "Company Name is required")]    
          public string Name { get; set; }


        [Required(ErrorMessage = "Base Currency is required")]
        public int CurrencyId { get; set; }

        

    }
}
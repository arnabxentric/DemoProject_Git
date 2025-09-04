using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace XenERP.Models
{
    [MetadataType(typeof(BusinessPartnerMetaData))]
    public partial class BusinessPartner
    {
    }
    public class BusinessPartnerMetaData
    {
        [Required(ErrorMessage = "The Name field is required.")]
        public string Name { get; set; }
    }
}
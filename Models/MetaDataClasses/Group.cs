using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace XenERP.Models
{
    [MetadataType(typeof(GroupMetaData))]
    public partial class Group
    {
    }
    public partial class GroupMetaData
    {
        [Required(ErrorMessage = "Mandatory Field.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Mandatory Field.")]
        public string Code { get; set; }
       // public int GId { get; set; }
    }
}
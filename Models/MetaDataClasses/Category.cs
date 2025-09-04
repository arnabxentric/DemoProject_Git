using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace XenERP.Models
{
    [MetadataType(typeof(CategoryMetaData))]
    public partial class Category
    {
    }
    public partial class CategoryMetaData
    {
        [Required (ErrorMessage="The Category Name field is required.")]
        public string Name { get; set; }
        public int GId { get; set; }
    }
}
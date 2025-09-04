using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using DataAnnotationsExtensions.ClientValidation;
using System.Web.Mvc;
using System.Collections.Generic;


namespace XenERP.Models
{
    [MetadataType(typeof(RoleMetaData))]

    public partial class Role
    {
    }
    public partial class RoleMetaData
    {
      

        [Required(ErrorMessage = "The Role field is required.")]
        [Remote("CheckRole", "Company", HttpMethod = "Post", ErrorMessage = "Role already exist")]
        public string RoleName { get; set; }
    }
}
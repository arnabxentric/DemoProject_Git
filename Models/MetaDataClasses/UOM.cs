using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using DataAnnotationsExtensions.ClientValidation;
using System.Web.Mvc;
using System.Collections.Generic;


namespace XenERP.Models
{
    [MetadataType(typeof(UOMMetaData))]

    public partial class UOM
    {
    }
    public partial class UOMMetaData
    {
        [Required(ErrorMessage = "The Symbol field is required.")]
        [Remote("CheckUnit","Home",HttpMethod="Post",ErrorMessage="Symbol already exist",AdditionalFields="Id")]

        public string Code { get; set; }


        [Required(ErrorMessage = "The Description field is required.")]
        public string Description { get; set; }
    }
}
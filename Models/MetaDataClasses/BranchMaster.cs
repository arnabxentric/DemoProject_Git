using System;
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
    [MetadataType(typeof(BranchMetaData))]

    public partial class BranchMaster
    {
    }
    public partial class BranchMetaData
    {
        //[Required(ErrorMessage = "The Company Name field is required.")]
        //public string CompanyId { get; set; }




        [Required(ErrorMessage = "The Branch Code field is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "2-200 characters allowed")]
        [Remote("CheckBranchCode", "Branch", HttpMethod = "post", ErrorMessage = "Branch Code already exists", AdditionalFields = "Id")]    

        public string Code { get; set; }


        [Required(ErrorMessage = "The Branch Name field is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "2-200 characters allowed")]
        public string Name { get; set; }





        [RegularExpression("^[+0-9]*$", ErrorMessage = "Not a Valid number")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "10-20 numbers allowed")]
        public string ContactNumber { get; set; }


        
        [RegularExpression("^([0-9a-zA-Z]([-.\\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9})$", ErrorMessage = "Not a valid Email Id")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "2-100 characters allowed")]
        public string EmailId { get; set; }

        [StringLength(500, MinimumLength = 2, ErrorMessage = "2-500 characters allowed")]
        public string Address { get; set; }

        
        

        [StringLength(100, MinimumLength = 2, ErrorMessage = "2-100 character are allowed")]
         public string State { get; set; }



        [StringLength(100, MinimumLength = 2, ErrorMessage = "2-100 character are allowed")]
        public string City { get; set; }



        [StringLength(20, MinimumLength = 6, ErrorMessage = "6-20 numbers allowed")]
        public string ZipCode { get; set; }


    }
}
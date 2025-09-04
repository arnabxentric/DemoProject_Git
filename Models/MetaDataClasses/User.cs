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
    [MetadataType(typeof(MetaDataUser))]


    public partial class User
    {


    }

    public partial class MetaDataUser
    {


        [Required(ErrorMessage = "Branch Name is required")]
        public int BranchId { get; set; }
   
        [Required(ErrorMessage = "First Name is required")]
        [RegularExpression("^[A-Za-z ]*$", ErrorMessage = "First Name is invalid")]
        [StringLength(250, MinimumLength = 2, ErrorMessage = "2-250 characters allowed")]
        public string FirstName { get; set; }


        [Required(ErrorMessage = "Last Name is required")]
        [RegularExpression("^[A-Za-z ]*$", ErrorMessage = "Last Name is invalid")]
        [StringLength(250, MinimumLength = 2, ErrorMessage = "2-250 characters allowed")]
        public string LastName { get; set; }


          [Required(ErrorMessage = "Email ID is required")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "2-100 characters allowed")]
        public string UserEmailAddress { get; set; }


          [Required(ErrorMessage = "Mobile number is required")]
        //[RegularExpression("^[+0-9]*$", ErrorMessage = "Invalid Mobile number")]
          [StringLength(15, MinimumLength = 10, ErrorMessage = "Enter Mobile number between 10 to 15")]
          //[Range(10, 20, ErrorMessage = "Enter Mobile number between 10 to 20")]
        public string PhoneNumber { get; set; }



        [Required(ErrorMessage = "User ID is required")]     
        [StringLength(50, MinimumLength = 5, ErrorMessage = "5-50 characters allowed")]
        [Remote("Checkuserid", "User", HttpMethod = "Post", ErrorMessage = "User ID already exists", AdditionalFields = "Id")]
        public string UserName { get; set; }




        [Required(ErrorMessage = "Password is required")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "5-50 characters allowed")]
        [DataType(DataType.Password)]
        public string Password { get; set; }


    }

}
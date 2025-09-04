using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace XenERP.Models
{
    public class CreatePlantModelView
    {
        public long Id { get; set; }
        [Required(ErrorMessage = "The Plant Code field is required.")]
        public string Code { get; set; }
        [Required(ErrorMessage = "The Plant Name field is required.")]
        public string Name { get; set; }
        public string Description { get; set; }
        public string ContactName { get; set; }
        public string PhoneNumber { get; set; }
        public string MobileNumber { get; set; }
        public string AdressName { get; set; }
        public string Address { get; set; }
        public string Suburb { get; set; }
        public string Town { get; set; }
        public string State { get; set; }
       // public virtual Country Country { get; set; }
        public string CountryName { get; set; }
        public string PIN { get; set; }
        [Required(ErrorMessage = "The Manager Code field is required.")]
        public string EmployeeCode { get; set; }
        public int EmployeeId { get; set; }
        [Required(ErrorMessage = "The Manager Name field is required.")]
        public string ManagerName { get; set; }
        public string LastName { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public long Userid { get; set; }
        public long Branchid { get; set; }
        public long Companyid { get; set; }
        public Nullable<long> CountryId { get; set; }
       
    }
}
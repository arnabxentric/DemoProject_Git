using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace XenERP.Models
{
    public class DepartmentModelView
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Nullable<int> BranchId { get; set; }
        public string Head { get; set; }
        public string PhoneNo { get; set; }
        public bool Status { get; set; }
        public long UserId { get; set; }
        public long CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string BranchName { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
    }
}
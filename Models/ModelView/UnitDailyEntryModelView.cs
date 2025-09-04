using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace XenERP.Models
{
    public class UnitDailyEntryModelView
    {
        public long Id { get; set; }
        public System.DateTime EntryDate { get; set; }
        public string EntryDateString { get; set; }
        public long Serial { get; set; }
        public string No { get; set; }
        public string Remarks { get; set; }
        public int FinancialYearid { get; set; }
        public int BranchId { get; set; }
        public int CompanyId { get; set; }
        public Nullable<long> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }

        public List<UnitDailyEntryDetailModelView> DEV { get; set; }
       

    }

}
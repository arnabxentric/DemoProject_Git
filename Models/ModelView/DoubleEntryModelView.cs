using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace XenERP.Models
{
    public class DoubleEntryModelView
    {
        public long Id { get; set; }

        [Required]
        public string DEVNO { get; set; }
        public long DEVSerial { get; set; }
        public short DEVType { get; set; }

       
        public string Narration { get; set; }
        public System.DateTime DEVDate { get; set; }

        [Required]
        public string DEVDateString { get; set; }
        public decimal DebitTotal { get; set; }

        [Compare("DebitTotal")]
        public decimal CreditTotal { get; set; }
        public short FinancialYearId { get; set; }
        public short CompanyId { get; set; }
        public short UserId { get; set; }
        public int BId { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<short> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<short> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }

        public List<DoubleEntryDetailModelView> DEV { get; set; }
        public List<DoubleEntryBankDetailModelView> DEVBD { get; set; }

    }

}
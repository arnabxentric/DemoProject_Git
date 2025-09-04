using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace XenERP.Models
{
    public class JournalModelView
    {
        public long Id { get; set; }
        public string JournalCode { get; set; }
        public string Category { get; set; }
        public string JournalName { get; set; }
        public string Narration { get; set; }
        public long FinancialYearId { get; set; }
        public long CompanyId { get; set; }
        public long UserId { get; set; }
        public Nullable<long> BranchId { get; set; }

        [Required(ErrorMessage = "The Branch field is Mandatory.")]
        public Nullable<long> BId { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public string JournalDate { get; set; }
        public Nullable<decimal> TotalDebit { get; set; }
        public Nullable<decimal> TotalCredit { get; set; }

        public List<JournalDetailModelView> journalDetails { get; set; }

    }

    public class JournalDetailModelView
    {
        public long JournalId { get; set; }
        public string Particulars { get; set; }
        public string Narrative { get; set; }
        public Nullable<decimal> Debit { get; set; }
        public Nullable<decimal> Credit { get; set; }

        public string LedgerId { get; set; }
        public long LID { get; set; }
    }
}
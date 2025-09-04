using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace XenERP.Models
{
    public class DoubleEntryBankDetailModelView
    {
        public long? Id { get; set; }
        public long? DEVId { get; set; }
        public long LID { get; set; }
        public string LName { get; set; }

        [Required]
        public string ModeOfPayment { get; set; }

        public IEnumerable<SelectListItem> modesofpay { get; set; }
        public string ChequeNo { get; set; }
        public Nullable<System.DateTime> ChequeDate { get; set; }
        public string ChequeDateString { get; set; }
        public string ChequeDetail { get; set; }
    }
}
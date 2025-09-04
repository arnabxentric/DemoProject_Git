using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace XenERP.Models
{
    public class ReceiptPaymentDetailModelView
    {
        public int LID { get; set; }

        [Required(ErrorMessage = "The Ledger Name field is required.")]
        public string LName { get; set; }
        public Nullable<decimal> Cash { get; set; }
        public Nullable<decimal> Bank { get; set; }
        public Nullable<decimal> RTGS { get; set; }
        [Required(ErrorMessage = "The Line Total field is required.")]
        public decimal LineTotal { get; set; }
    }
}
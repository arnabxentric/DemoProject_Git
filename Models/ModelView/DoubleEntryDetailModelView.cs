using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace XenERP.Models
{
    public class DoubleEntryDetailModelView
    {
        public long? Id { get; set; }
        public long? DEVId { get; set; }
        public int Club { get; set; }
        public long LID { get; set; }

        [Required]
        public string LName { get; set; }

        [Range(0.0, float.MaxValue, ErrorMessage = "Only positive number allowed")]
        [Required]
      
        public decimal Debit { get; set; }

        [Range(0.0, float.MaxValue, ErrorMessage = "Only positive number allowed")]
        [Required]
        [NotEqualTo("Debit", ErrorMessage = "These fields cannot match")]
        //[Compare("Debit")]
        public decimal Credit { get; set; }
        public string LGroup { get; set; }
    }
}
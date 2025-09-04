using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XenERP.Models
{
    public class RequestModelView
    {
        public long RequestId { get; set; }
        public long RequestDetailId { get; set; }

        public string RequestNo { get; set; }
        public System.DateTime RequestDate { get; set; }
        public System.DateTime ExpectedDate { get; set; }
        public long Branch { get; set; }
        public string Product { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }

        public string Status { get; set; }

    }
}
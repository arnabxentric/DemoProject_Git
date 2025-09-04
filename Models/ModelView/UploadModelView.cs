using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XenERP.Models
{
    public class UploadModelView
    {
        public DateTime Date { get; set; }
        public string PurchaseOrder { get; set; }
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ReffInvoiceNo { get; set; }
        public long? ProductId { get; set; }
        public long? SupplierId { get; set; }
        public long? CustomerId { get; set; }
    }
}
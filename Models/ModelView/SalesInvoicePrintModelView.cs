using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XenERP.Models
{
    public class SalesInvoicePrintModelView
    {
        
     
        public string CustomerName { get; set; }
      
        public string Date { get; set; }
       
        public string InvoiceDate { get; set; }
        public string InvoiceNO { get; set; }
        public decimal TotalAddAmount { get; set; }
        public decimal TotalDeductAmount { get; set; }
        
        public decimal BCGrandTotal { get; set; }
        public string GrandTotalInWords { get; set; }
        public string SalesPerson { get; set; }
        public string ContactNo { get; set; }
        public string Address { get; set; }
        public string Suburb { get; set; }
        public string City { get; set; }
        public string StateRegion { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public List<SalesInvoiceDetailModelView> salesInvoiceDetails { get; set; }
    }
}
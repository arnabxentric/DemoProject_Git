using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace XenERP.Models
{
    public class IncomeReportCustomerWiseModelView
    {
        public string NO { get; set; }
        public Nullable<int> ItemID { get; set; }
        public string Name { get; set; }
        public Nullable<int> PQuantity { get; set; }
        public Nullable<decimal> PRate { get; set; }
        public Nullable<decimal> SRate { get; set; }
        public Nullable<decimal> PPrice { get; set; }
        public Nullable<decimal> SPrice { get; set; }
        public Nullable<decimal> Profit { get; set; }
        public Nullable<int> InvoiceNo { get; set; }
        public Nullable<decimal> SPriceTotal { get; set; }
        public Nullable<decimal> PPriceTotal { get; set; }
        public Nullable<decimal> ProfitTotal { get; set; }

    }
}
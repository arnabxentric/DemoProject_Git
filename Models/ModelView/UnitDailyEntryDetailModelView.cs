using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace XenERP.Models
{
    public class UnitDailyEntryDetailModelView
    {
        public long Id { get; set; }
        public long UnitDailyEntryId { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public Nullable<decimal> ProductSize { get; set; }
        public Nullable<decimal> OpeningQuantity { get; set; }
        public Nullable<decimal> LeftQuantity { get; set; }
        public Nullable<decimal> ConsumedQuantity { get; set; }

    }
}
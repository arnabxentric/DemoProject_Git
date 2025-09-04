using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XenERP.Models
{
    public class TransferDeliveryDetailModelView
    {
        public long Id { get; set; }
        public long TransferDeliveryReceiveId { get; set; }
        public long ItemId { get; set; }
        public string BarCode { get; set; }
        public string Description { get; set; }
        public decimal OrderQuantity { get; set; }
        public decimal TransferQuantity { get; set; }
        public decimal ReceiveQuantity { get; set; }
        public long UnitId { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string UnitName { get; set; }
        public long UnitIdSecondary { get; set; }
        public Nullable<long> SecUnitId { get; set; }
        public decimal UnitFormula { get; set; }
        public Nullable<decimal> SecUnitFormula { get; set; }
        public string UnitSecondaryName { get; set; }
        public string SecUnitName { get; set; }

    }
}
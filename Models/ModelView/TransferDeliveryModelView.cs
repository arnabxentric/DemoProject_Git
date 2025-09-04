using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace XenERP.Models
{
    public class TransferDeliveryModelView
    {
        public long Id { get; set; }
        public string TransferDeliveryNo { get; set; }
        public string TransferOrderNo { get; set; }
        public long TransferOrderId { get; set; }
        public string TransferOrderDate { get; set; }
        public string ExpectedDate { get; set; }
        public string TransferDeliveryDate { get; set; }
        public string TransferReceiveDate { get; set; }
        public long FromBranchId { get; set; }
        public long ToBranchId { get; set; }

        [Required(ErrorMessage = "Please choose a From Warehouse field is required.")]
        public long FromWareHouseId { get; set; }

        [Required(ErrorMessage = "Please choose a To Warehouse field is required.")]
        public long ToWareHouseId { get; set; }
        public string LorryNo { get; set; }
        public string Transporter { get; set; }
        public string TransporterContact { get; set; }
        public string TagNo { get; set; }
        public int FinancialYearId { get; set; }
        public string Comments { get; set; }
        public Nullable<long> SerialNo { get; set; }
        public Nullable<long> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<long> ReceivedBy { get; set; }
        public Nullable<System.DateTime> ReceivedOn { get; set; }
        public Nullable<long> ReceivedModifiedBy { get; set; }
        public Nullable<System.DateTime> ReceivedModifiedOn { get; set; }
        public string Status { get; set; }
        public long UserId { get; set; }
        public long CompanyId { get; set; }
        public long BranchId { get; set; }
    }
}
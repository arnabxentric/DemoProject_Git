using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XenERP.Models
{
    public class ContraModelView
    {
        public int Id { get; set; }
        public string dateCTB { get; set; }
        public string dateBTC { get; set; }
        public string dateBTB { get; set; }
        public int fYearId { get; set; }
        public int BranchId { get; set; }
        public int CompanyId { get; set; }
        public string Type { get; set; }
        public int? ToCTBId { get; set; }
        public int? FromCTBId { get; set; }
        public int? ToBTCId { get; set; }
        public int? FromBTCId { get; set; }
        public int? ToBTBId { get; set; }
        public int? FromBTBId { get; set; }
        public string chequeNoBTC { get; set; }
        public string chequeNoBTB { get; set; }
        public string chequeDateBTC { get; set; }
        public string chequeDateBTB { get; set; }
        public decimal? TotalAmount { get; set; }
        public string transactionType { get; set; }
        public string transactionNo { get; set; }
        public string Remarks { get; set; }
        public int VoucherNo { get; set; }
       
        
        public int Cont { get; set; }


        public string RemarksCTB { get; set; }
        public string RemarksBTC { get; set; }
        public string RemarksBTB { get; set; }

        public decimal? CTBAmount { get; set; }
        public decimal? BTCAmount { get; set; }
        public decimal? BTBAmount { get; set; }


    }
}
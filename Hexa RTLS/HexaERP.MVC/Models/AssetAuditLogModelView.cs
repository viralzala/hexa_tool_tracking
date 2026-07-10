using System;

namespace HexaERP.MVC.Models
{
    public class AssetAuditLogModelView
    {
        public long mSMTProductId { get; set; }
        public string OldRowData { get; set; }
        public string NewRowData { get; set; }
        public string DmlType { get; set; }
        public DateTime DmlTimestamp { get; set; }
        public string DmlCreatedBy { get; set; }
        public DateTime TrxTimestamp { get; set; }
    }
}
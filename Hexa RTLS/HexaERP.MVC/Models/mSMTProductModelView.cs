using System;

namespace HexaERP.MVC.Models
{
    public class mSMTProductModelView
    {
        public int mSMTProductId { get; set; }
        public Nullable<int> Id { get; set; }
        public string SerialNumber { get; set; }
        public Nullable<int> PalletId { get; set; }
        public string Quantity { get; set; }
        public Nullable<System.DateTime> DateAndTime { get; set; }
        public Nullable<int> StatusId { get; set; }
        public string Status { get; set; }
        public string CustomerCode { get; set; }
        public Nullable<int> ContainerId { get; set; }
        public Nullable<int> CustomerId { get; set; }
        public string Lot { get; set; }
        public Nullable<int> PartId { get; set; }
        public string PartNumber { get; set; }
        public string QADLine { get; set; }
        public Nullable<int> ShiftId { get; set; }
        public string Station { get; set; }
        public string Ble { get; set; }
        public Nullable<int> mZoneId { get; set; }
        public string ShelfName { get; set; }
        public string Remark { get; set; }
        public string Comment { get; set; }
        public bool IsNormalReturn { get; set; }
        public bool IsQualityReturn { get; set; }
        public bool IsPutaway { get; set; }
        public bool IsTakeaway { get; set; }
        public bool IsAssembly { get; set; }
        public bool IsMaster { get; set; }
        public bool IsApprove { get; set; }
        public Nullable<System.DateTime> LastSeenTime { get; set; }
        public bool IsAction { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }


        public string LotName { get; set; }
    }
}
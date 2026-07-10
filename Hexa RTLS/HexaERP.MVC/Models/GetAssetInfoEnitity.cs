using System;

namespace HexaERP.MVC.Models
{
    public partial class GetAssetInfoEnitity
    {
        public int tAssetTagId { get; set; }
        public string IteamName { get; set; }
        public string Model { get; set; }
        public string ModelNo { get; set; }
        public string SerialNo { get; set; }
        public string Manufacturer { get; set; }
        public string BarCode { get; set; }
        public string RFID { get; set; }
        public Nullable<decimal> PurchaseCost { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string InvNo { get; set; }
        public Nullable<int> Depreciation { get; set; }
        public string Receivedby { get; set; }
        public Nullable<decimal> bStock { get; set; }
        public Nullable<int> DefaultWarranty { get; set; }
        public string IteamDescription { get; set; }
        public string VendorName { get; set; }
        public string GroupName { get; set; }
        public string IteamType { get; set; }
        public string UnitName { get; set; }
        public string StatusName { get; set; }

        public string EmployeeName { get; set; }
        public string EmployeeId { get; set; }
        public string ContactNo { get; set; }
        public string EmailId { get; set; }
        public Nullable<int> tEmployeeTagId { get; set; }

        public Nullable<System.DateTime> IssueDate { get; set; }
        public Nullable<System.DateTime> ReturnDate { get; set; }
        public Nullable<int> EngDays { get; set; }

        public string Site { get; set; }
        public string Zone { get; set; }
        public string FloorName { get; set; }
        public string RoomName { get; set; }
        public string img { get; set; }

    }
}
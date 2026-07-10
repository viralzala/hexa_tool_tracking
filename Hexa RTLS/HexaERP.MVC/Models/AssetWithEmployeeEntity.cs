using System;

namespace HexaERP.MVC.Models
{
    public class AssetWithEmployeeEntity
    {
        public string IteamName { get; set; }
        public string Model { get; set; }
        public string ModelNo { get; set; }
        public string SerialNo { get; set; }
        public string Manufacturer { get; set; }
        public string BarCode { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeId { get; set; }
        public string EmailId { get; set; }
        public string ContactNo { get; set; }
        public Nullable<System.DateTime> IssueDate { get; set; }
        public Nullable<System.DateTime> ReturnDate { get; set; }
        public string InvNo { get; set; }
        public string Receivedby { get; set; }
        public Nullable<int> DefaultWarranty { get; set; }
        public Nullable<decimal> bStock { get; set; }
        public string IteamDescription { get; set; }
        public string UID { get; set; }
        public string UnitName { get; set; }
        public string Site { get; set; }
        public string Zone { get; set; }
        public string Rack { get; set; }
        public string Shelf { get; set; }
        public string AssetCategory { get; set; }
        public string AssetSubCategory { get; set; }
        public string AssetType { get; set; }
        public string VendorName { get; set; }
        public string StatusName { get; set; }
        public Nullable<int> NoOfDayLeft { get; set; }

    }
}
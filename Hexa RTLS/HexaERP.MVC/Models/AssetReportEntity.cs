using System;

namespace HexaERP.MVC.Models
{
    public class AssetReportEntity
    {
        public string IteamName { get; set; }
        public string Model { get; set; }
        public string ModelNo { get; set; }
        public string SerialNo { get; set; }
        public string Manufacturer { get; set; }
        public string BarCode { get; set; }
        public Nullable<decimal> PurchaseCost { get; set; }
        public Nullable<System.DateTime> PurchaseDate { get; set; }
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
    }
}
using System;

namespace HexaERP.MVC.Models
{
    public partial class IndorAstEntity
    {
        public string Asset { get; set; }
        public string IteamName { get; set; }
        public string IteamCode { get; set; }
        public string IteamDescription { get; set; }
        public string Epc { get; set; }
        public Nullable<DateTime> tDate { get; set; }
        public string Rack { get; set; }
        public string Shelf { get; set; }
        public string img { get; set; }
    }
}
namespace HexaERP.MVC.Models
{
    public partial class ReaderInfoEntity
    {
        public string Address { get; set; }
        public string Name { get; set; }
        public int PortNumber { get; set; }
        public bool IsPortConnected { get; set; }
        public bool IsConnected { get; set; }
        public int HubId { get; set; }
        public string Fault { get; set; }
        public bool State { get; set; }
        public string sDate { get; set; }

        //

        public decimal Readertemperature { get; set; }
        public bool Issingulating { get; set; }
        public bool Isconnected { get; set; }
    }
}
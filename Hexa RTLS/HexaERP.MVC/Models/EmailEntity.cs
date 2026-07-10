namespace HexaERP.MVC.Models
{
    using System;

    public partial class EmailEntity
    {
        public string MessageID { get; set; }
        public string Subject { get; set; }
        public string TimeOfDay { get; set; }
        public DateTime Date { get; set; }
        public string Html { get; set; }
        public string HtmlDataText { get; set; }
        public string HtmlDataString { get; set; }
        public string SenderAddress { get; set; }
        public string SenderDomainPart { get; set; }
        public string SenderLocalPart { get; set; }
        public string SenderName { get; set; }
        public string emailText { get; set; }
        public string emailTextDataString { get; set; }


    }
}
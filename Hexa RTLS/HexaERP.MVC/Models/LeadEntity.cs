namespace HexaERP.MVC.Models
{
    using System;

    public partial class LeadEntity
    {
        //Customer Entity
        public int TC_CustomerId { get; set; }
        public string Uid { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string EmailId { get; set; }
        public string Contact { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public Nullable<int> PinCode { get; set; }
        public string CompanyName { get; set; }
        public string Designation { get; set; }
        //Lead Ids Data
        public int? TC_LeadTypeId { get; set; }
        public int? TC_InquiryTypeId { get; set; }
        public int? TC_LeadDispositionId { get; set; }
        public int? TC_InquirySourceId { get; set; }

    }
}
using System;

namespace HexaERP.MVC.Models
{
    public partial class AppointmentEntity
    {

        public Nullable<long> TC_LeadId { get; set; }
        public Nullable<int> OrgInfoID { get; set; }
        public string AppointmentDate { get; set; }
    }
}
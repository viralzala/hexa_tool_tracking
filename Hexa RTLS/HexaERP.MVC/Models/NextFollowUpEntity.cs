using System;

namespace HexaERP.MVC.Models
{
    public partial class NextFollowUpEntity
    {
        public long TC_LeadId { get; set; }

        public Nullable<int> TC_ActionId { get; set; }
        public string NextFollowUpDate { get; set; }
        public string NextFollowUpAssinged { get; set; }
        public Nullable<bool> IsAction { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<int> OrgInfoID { get; set; }
    }
}
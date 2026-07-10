using System;

namespace HexaERP.MVC.Models
{
    public partial class LeadFollowUpEntity
    {
        public long TC_LeadFollowUpId { get; set; }
        public Nullable<int> BranchId { get; set; }
        public Nullable<long> TC_LeadId { get; set; }
        public Nullable<int> TC_ActionId { get; set; }
        public string PostActionCall { get; set; }
        public string FollowUpDate { get; set; }
        public string Title { get; set; }
        public string Comments { get; set; }
        public string FollowUpAssinged { get; set; }
        public Nullable<bool> IsAction { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<int> OrgInfoID { get; set; }
    }
}
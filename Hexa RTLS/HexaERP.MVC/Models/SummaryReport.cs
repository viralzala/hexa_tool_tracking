using System;

namespace HexaERP.MVC.Models
{
    public partial class SummaryReport
    {
        public Nullable<int> mSiteMasterId { get; set; }
        public Nullable<int> mZoneId { get; set; }
        public Nullable<int> mFloorMasterId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeId { get; set; }
        public Nullable<int> mAgencyId { get; set; }
        public Nullable<int> mDesignationId { get; set; }
        public Nullable<int> mSkillCategoryId { get; set; }
        public Nullable<int> mWorkCategoryId { get; set; }
        public Nullable<int> mActivityId { get; set; }
        public string fromDate { get; set; }
        public string toDate { get; set; }
        // public string RFID { get; set; }      

    }
}
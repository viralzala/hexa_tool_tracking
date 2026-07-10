using System;

namespace HexaERP.MVC.Models
{

    public partial class EmployeeDetailLogEntity
    {

        public string RFID { get; set; }
        public Nullable<System.DateTime> tDate { get; set; }
        public string Zone { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeId { get; set; }
        public string Site { get; set; }

        public string Agency { get; set; }
        public string Designation { get; set; }
        public string SkillCategory { get; set; }
        public string WorkCategory { get; set; }
        public string Activity { get; set; }
    }
}
using System;

namespace HexaERP.MVC.Models
{
    public partial class IndoEmpEntity
    {
        public string Agency { get; set; }
        public string Designation { get; set; }
        public string SkillCategory { get; set; }
        public string WorkCategory { get; set; }
        public string Activity { get; set; }
        public string Name { get; set; }
        public string EmployeeId { get; set; }
        public string Epc { get; set; }
        public Nullable<DateTime> tDate { get; set; }
    }
}
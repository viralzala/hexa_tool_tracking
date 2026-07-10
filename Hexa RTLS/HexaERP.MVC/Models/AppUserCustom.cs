using System;

namespace HexaERP.MVC.Models
{
    public class AppUserCustom
    {

        public string EMail { get; set; }
        public string AppUserName { get; set; }
        public Nullable<int> AppRoleId { get; set; }
        public string Password { get; set; }
        public Nullable<int> DepartmentId { get; set; }
        public Nullable<bool> IsAction { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<int> OrgInfoId { get; set; }
        public string Mobile { get; set; }
        public string Sex { get; set; }
        public string Address { get; set; }
        public Nullable<System.DateTime> DOJ { get; set; }
        public Nullable<System.DateTime> DOB { get; set; }
    }
}
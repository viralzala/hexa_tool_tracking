using System;

namespace HexaERP.MVC.Models
{
    public class ToolsTrackReportModel
    {
        public string ToolName { get; set; }
        public string FloorName { get; set; }
        public Nullable<int> FloorNo { get; set; }
        public string RoomName { get; set; }
        public Nullable<int> RoomNo { get; set; }
        public string Persent { get; set; }
        public string drawer { get; set; }
        public string drawerrack { get; set; }
        public Nullable<System.DateTime> tDate { get; set; }
        public Nullable<bool> Type { get; set; }
    }
}
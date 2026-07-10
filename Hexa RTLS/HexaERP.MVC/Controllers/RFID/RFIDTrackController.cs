using HexaERP.MVC.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class RFIDTrackController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();
        // GET: RFIDTrack
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public JsonResult MonitoringData()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            //var ObjData = (from tm in db.toMonitors
            //               join fm in db.mFloorMasters  on tm.mFloorMasterId equals fm.mFloorMasterId
            //               join trm in db.mRoomMasters on tm.mFloorMasterId equals trm.mFloorMasterId // ()
            //               join rft in db.tRFIDTypes  on tm.RFID equals rft.RFID
            //               join getl in db.mRoomMasters  on rft.LocationId equals getl.mRoomMasterId
            //               join getfl in db.mFloorMasters on getl.mFloorMasterId equals getfl.mFloorMasterId
            //               where tm.mAttPortId == trm.RoomNo 
            //               select new {
            //                   tm.RFID, INFloorName=fm.FloorName,
            //                   INFloorNo = fm.FloorNo,
            //                   INRoomName = trm.RoomName,
            //                   INRoomNo=trm.RoomNo,
            //                   rft.Name,
            //                   Types = rft.Type,
            //                   MYRoomName = getl.RoomName,
            //                   MYRoomNo = getl.RoomNo,
            //                   MYFloorName = getfl.FloorName,
            //                   MYFloorNo = getfl.FloorNo,
            //                   Color = ((getl.RoomNo) - (trm.RoomNo) + (getfl.FloorNo) - (fm.FloorNo)) }).ToList();
            //Convert List Data to The Json Array          
            return Json("", JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult LocationData()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = (from rm in db.mRoomMasters
                           join fm in db.mFloorMasters on rm.mFloorMasterId equals fm.mFloorMasterId
                           where fm.IsAction == true
                           select new
                           {
                               rm.mRoomMasterId,
                               rm.RoomName,
                               rm.RoomNo,
                               fm.mFloorMasterId,
                               fm.FloorName,
                               fm.FloorNo

                           }).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
    }
}
using HexaERP.MVC.Models;
using PusherServer;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.Tools
{
    public class ToolsTrackReportController : Controller
    {
        // GET: ToolsTrackReport
        private ERPdbEntities db = new ERPdbEntities();
        public ActionResult Index()
        {
            return View();
        }
        //
        [HttpGet]
        public ActionResult getToolsReport()
        {
            //            Select tg.ToolName, fm.FloorName,fm.FloorNo,rm.RoomName,rm.RoomNo,
            //ar.RoomName,tg.drawer,tg.drawerrack,tfo.tDate,typ.Type
            //from toTrackInfo as tfo

            //Left join toTooltag as tg on tfo.RFID = tg.RFID
            //Left join toRFIDType as typ on tg.toTooltagId = typ.RerfrenceId

            //Left join mRoomMaster as ar on tg.mRoomMasterId = ar.mRoomMasterId
            //Left join mFloorMaster as fm on tfo.mFloorMasterId = fm.mFloorMasterId
            //Left join mRoomMaster as rm on tfo.mRoomMasterId = rm.mRoomMasterId

            //Get Organization Id From Session Variable
            //int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            //ExampleEntities db = new ExampleEntities();
            //SqlParameter param1 = new SqlParameter("@id", id);
            //var ObjData = "";
            //try
            //{
            //    DateTime toDate = Convert.ToDateTime("2017 - 04 - 13 13:42:32.217");
            //    DateTime fromoDate = Convert.ToDateTime("2017 - 04 - 13 13:42:32.217");

            //var authors = db.Database.SqlQuery<dynamic>("usp_GetAuthorByName @AuthorName",
            //                     new SqlParameter("@toDate", toDate, "@fromDate", fromoDate));

            //var ObjData = db.proToolsTrackReport(toDate, fromoDate);
            var ObjData = (from tfo in db.toTrackInfoes

                           from prm in db.toTooltags

                           join tg in db.toTooltags on tfo.RFID equals tg.RFID into tg_ob

                           join typ in db.toRFIDTypes on prm.toTooltagId equals typ.RerfrenceId into typ_ob
                           join ar in db.mRoomMasters on prm.mRoomMasterId equals ar.mRoomMasterId into ar_ob

                           join fm in db.mFloorMasters on tfo.mFloorMasterId equals fm.mFloorMasterId into fm_ob
                           join rm in db.mRoomMasters on tfo.mRoomMasterId equals rm.mRoomMasterId into rm_ob

                           from tg_obs in tg_ob.DefaultIfEmpty()
                           from typ_obs in typ_ob.DefaultIfEmpty()
                           from ar_obs in ar_ob.DefaultIfEmpty()
                           from fm_obs in fm_ob.DefaultIfEmpty()
                           from rm_obs in rm_ob.DefaultIfEmpty()
                               //where (tg_ob.IsAction == true)
                           select new
                           {
                               // tg_obs.toTooltagId,
                               tfo.RFID,
                               tg_obs.ToolName,
                               fm_obs.FloorName,
                               fm_obs.FloorNo,

                               //Tracked
                               ar_obs.RoomName,
                               ar_obs.RoomNo,

                               tfo.mAttPortId,
                               Persent = rm_obs.RoomName,
                               tg_obs.drawer,
                               tg_obs.drawerrack,
                               tfo.tDate,
                               typ_obs.Type
                           }).Take(300).ToList();
            //Convert List Data to The Json Array     

            var jsonResult = Json(ObjData, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //return Json(jsonResult, JsonRequestBehavior.AllowGet);
            return jsonResult;

            //return Json(ObjData, JsonRequestBehavior.AllowGet);
            //}
            //catch (DbEntityValidationException ex)
            //{
            //    foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
            //    {
            //        // Get entry
            //        DbEntityEntry entry = item.Entry;
            //        string entityTypeName = entry.Entity.GetType().Name;
            //        // Display or log error messages
            //        foreach (DbValidationError subItem in item.ValidationErrors)
            //        {
            //            string message = string.Format("Error '{0}' occurred in {1} at {2}",
            //                     subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
            //            Console.WriteLine(message);
            //        }
            //    }
            //}
            //return Json(ObjData, JsonRequestBehavior.AllowGet);
        }

        //
        [HttpGet]
        public JsonResult getTools()
        {
            var dobj = db.toTooltags.Where(o => o.IsAction == true).Select(o => new { o.toTooltagId, o.ToolName }).ToList();
            return Json(dobj, JsonRequestBehavior.AllowGet);
        }
        //
        [HttpGet]
        public JsonResult getEmplTrackList(int toTooltagId, string tDate)
        {

            DateTime Td;
            Td = Convert.ToDateTime(tDate);
            var ObjData = (from tfo in db.toTrackInfoes

                           from prm in db.toTooltags

                               //from emp in db.tEmployeeTags

                           join tg in db.toTooltags on tfo.RFID equals tg.RFID into tg_ob

                           join temp in db.tEmployeeTags on tfo.RFID equals temp.RFID into temp_ob

                           join typ in db.toRFIDTypes on tfo.RFID equals typ.RFID into typ_ob

                           join ar in db.mRoomMasters on prm.mRoomMasterId equals ar.mRoomMasterId into ar_ob

                           join fm in db.mFloorMasters on tfo.mFloorMasterId equals fm.mFloorMasterId into fm_ob
                           join rm in db.mRoomMasters on tfo.mRoomMasterId equals rm.mRoomMasterId into rm_ob

                           from tg_obs in tg_ob.DefaultIfEmpty()
                           from typ_obs in typ_ob.DefaultIfEmpty()
                           from ar_obs in ar_ob.DefaultIfEmpty()
                           from fm_obs in fm_ob.DefaultIfEmpty()
                           from rm_obs in rm_ob.DefaultIfEmpty()

                           from temp_obs in temp_ob.DefaultIfEmpty()

                           where (prm.toTooltagId == toTooltagId && DbFunctions.TruncateTime(tfo.tDate) == DbFunctions.TruncateTime(Td) && typ_obs.Type != null)
                           select new
                           {
                               // tg_obs.toTooltagId,
                               tfo.RFID,
                               tg_obs.ToolName,
                               fm_obs.FloorName,
                               fm_obs.FloorNo,
                               //Tracked
                               ar_obs.RoomName,
                               ar_obs.RoomNo,

                               tfo.mAttPortId,
                               Persent = rm_obs.RoomName,
                               tg_obs.drawer,
                               tg_obs.drawerrack,
                               tfo.tDate,
                               typ_obs.Type,

                               //
                               temp_obs.EmployeeName
                           }).OrderBy(o => o.tDate).ToList();

            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> pusher()
        {
            var options = new PusherOptions { Encrypted = true };

            var pusher = new Pusher(
              "342645",
              "9ceb80a9736c2001c8d1",
              "778e6b02ed1873a8b15b",
              options);

            var result = await pusher.TriggerAsync(
              "my-channel",
              "my-event",
              new { message = "hello world" });

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
using HexaERP.MVC.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.SMT
{
    public class TVDisplayDashboardController : Controller
    {
        // GET: TVDisplayDashboard
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> GetNotifications()
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {
                using (var context = new ERPdbEntities())
                {
                    var _ReaderStatus = @"select DISTINCT r.ReaderNo,a.RoomName, r.OrgInfoId
from toTrackInfo as r
left join mRoomMaster as a on r.mRoomMasterId = a.mRoomMasterId
where r.UID = 'Alert'";

                    var ReaderStatus = await context.Database.SqlQuery<ReaderStatusModelView>(_ReaderStatus).ToListAsync();

                    result = this.Json(new { Flag = true, ReaderStatus, message = "Data Found" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = string.Empty;
                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    // Get entry
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;
                    // Display or log error messages
                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        message = string.Format("Error '{0}' occurred in {1} at {2}",
                                 subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                        Console.WriteLine(message);
                    }
                }
                result = this.Json(new { message, Flag = false }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }

        public class PartNumberModelView
        {
            public int Q { get; set; }
            public string PartNumber { get; set; }
            public string Status { get; set; }
        }

        public class ReaderStatusModelView
        {
            public string ReaderNo { get; set; }
            public string RoomName { get; set; }
            //public int OrgInfoId { get; set; }
        }

        [HttpGet]
        public async Task<ActionResult> PartNumberProcess()
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {
                using (var context = new ERPdbEntities())
                {
                    var _partNumber = @"select SUM(Quantity) As Q , PartNumber 
from mSMTProduct
where IsTakeaway=0 AND IsAssembly=0
--where CAST(CreatedDate as DATE) >= CONVERT(date, GETDATE())  AND CAST(CreatedDate as DATE) <= CONVERT(date,GETDATE())
Group By PartNumber
order by PartNumber";

                    var _partNumberData = await context.Database.SqlQuery<PartNumberModelView>(_partNumber).ToListAsync();

                    result = this.Json(new { Flag = true, _partNumberData, message = "Data Found" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = string.Empty;
                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    // Get entry
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;
                    // Display or log error messages
                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        message = string.Format("Error '{0}' occurred in {1} at {2}",
                                 subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                        Console.WriteLine(message);
                    }
                }
                result = this.Json(new { message, Flag = false }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }

        [HttpGet]
        public async Task<JsonResult> GetCounts()
        {

            JsonResult result = new JsonResult();
            try
            {
                var dateLimit = DateTime.Now;

                using (var context = new ERPdbEntities())
                {

                    var _shelf = @"select COUNT(*) As TotalShelf,
(select COUNT(*) from mShelf where IsAction = 0) AS AvailableShelf,
(select COUNT(*) from mShelf where IsAction = 1) AS AllocatedShelf
from mShelf";

                    var _shelfData = await context.Database.SqlQuery<ShelfModelView>(_shelf).FirstOrDefaultAsync();

                    var _smtProdcut = @"select COUNT(*) As TotalProduct,
(select COUNT(*) from mSMTProduct where IsPutaway = 1) AS TotalPutaway,
(select COUNT(*) from mSMTProduct where IsTakeaway = 1) AS TotalTakeaway,
(select COUNT(*) from mSMTProduct where IsAssembly = 1) AS TotalAssembly,
(select COUNT(*) from mSMTProduct where IsMaster = 1) AS TotalMaster
from mSMTProduct";
                    var _smtProcutData = await context.Database.SqlQuery<SmtProcutModelView>(_smtProdcut).FirstOrDefaultAsync();


                    //                    var _ReaderStatus = @"select r.ReaderNo,m.LastSeenTime,rm.RoomName
                    //from mReaderSettup as r
                    //LEFT JOIN toMonitor AS m on r.ReaderNo = m.Name
                    //LEFT JOIN mRoomMaster as rm on rm.mRoomMasterId = r.mRoomMasterId
                    //group by r.ReaderNo,m.LastSeenTime,rm.RoomName";

                    //                    var _ReaderStatusData = await context.Database.SqlQuery<ReadersStatusModelView>(_ReaderStatus).ToListAsync();

                    //var ReaderStatus = context.toMonitors
                    //    .OrderBy(d => d.tDate)
                    //    .Select(s => new { s.Name })
                    //    .Distinct()
                    //    .Select(s => new
                    //    {
                    //        s.Name,
                    //        tm = DbFunctions.DiffSeconds(s.tDate, dateLimit) > 120 ? "OFF" : "ON"
                    //    }).ToList();

                    var ReaderStatus = context
                        .toMonitors
                        .OrderBy(d => d.tDate)
                        .GroupBy(n => new { n.Name })
                        .Select(g => new
                        {
                            Name = g.Key.Name,
                            tm = g.Select(t => new
                            {
                                t = t.tDate,
                                st = DbFunctions.DiffSeconds(t.tDate, dateLimit) > 120 ? "OFF" : "ON"
                            }).OrderByDescending(od => od.t)
                        }).ToList();

                    result = Json(new { _shelfData, _smtProcutData, ReaderStatus }, JsonRequestBehavior.AllowGet);
                    result.MaxJsonLength = int.MaxValue;
                }
            }
            catch (Exception) { }
            return result;
        }



        [HttpGet]
        public async Task<JsonResult> BeaconBattery()
        {

            JsonResult result = new JsonResult();
            try
            {
                using (var context = new ERPdbEntities())
                {
                    var _BeaconsAlgorithem = @"select * from toMonitor where AntennaPortNumber <= 2750 AND AntennaPortNumber != 0";
                    var _BeaconsData = await context.Database.SqlQuery<toMonitor>(_BeaconsAlgorithem).ToListAsync();
                    result = Json(new { _BeaconsData }, JsonRequestBehavior.AllowGet);
                    result.MaxJsonLength = int.MaxValue;
                }
            }
            catch (Exception) { }
            return result;
        }
        public class ReadersStatusModelView
        {
            public string RoomName { get; set; }
            public string ReaderNo { get; set; }
            public string LastSeenTime { get; set; }
        }

        public class ShelfModelView
        {
            public int TotalShelf { get; set; }
            public int AvailableShelf { get; set; }
            public int AllocatedShelf { get; set; }
        }
        public class SmtProcutModelView
        {
            public int TotalPutaway { get; set; }
            public int TotalTakeaway { get; set; }
            public int TotalAssembly { get; set; }
            public int TotalMaster { get; set; }
            public int TotalProduct { get; set; }
        }
    }
}
using HexaERP.MVC.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class HexaRTLSController : Controller
    {
        static List<tToolTrackDemo> tagsTrack = new List<tToolTrackDemo>();

        [Authorize(Roles = "AD,PAK,AAD,SA")]
        public ActionResult Index()
        {
            try
            {
                HttpCookie cookieObject = Request.Cookies["HexaCookie"];
                if (cookieObject != null)
                {
                    ViewBag.LogedIn = cookieObject["AppUserName"];
                }
                else { return RedirectToAction("Index", "AppUser"); }
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "AppUser");
            }
            return View();
        }

        [HttpGet]
        public JsonResult Details()
        {
            using (StreamReader sr = new StreamReader(Server.MapPath("~/Content/EmployeeTag.json")))
            {
                var _tEmployeeTag = JsonConvert.DeserializeObject<List<tEmployeeTag>>(sr.ReadToEnd());
                return Json(_tEmployeeTag.ToArray(), JsonRequestBehavior.AllowGet);
            }
        }

        //HexaRTLS/getlocationdata  
        [HttpGet]
        public JsonResult getlocationdata()
        {
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            using (var db = new ERPdbEntities())
            {
                var ZoneData = (from Dis in db.mZones
                                where (Dis.IsAction == true)
                                select new { Dis.mZoneId, Dis.Zone }).ToList();

                var subZoneData = (from Dis in db.mFloorMasters
                                   join F in db.mZones on Dis.mZoneId equals F.mZoneId
                                   where (Dis.IsAction == true)
                                   select new { Dis.mFloorMasterId, Dis.FloorName, F.mZoneId, F.Zone, Dis.FloorNo }).ToList();

                var FloorData = (from Dis in db.mFloorMasters
                                 where (Dis.IsAction == true)
                                 select new { Dis.mFloorMasterId, Dis.FloorName, Dis.FloorNo }).ToList();

                var RoomData = (from Dis in db.mRoomMasters
                                join F in db.mFloorMasters on Dis.mFloorMasterId equals F.mFloorMasterId
                                where (Dis.IsAction == true)
                                select new { Dis.mRoomMasterId, Dis.RoomName, F.FloorName, Dis.mFloorMasterId }).ToList();

                var PortsData = (from Dis in db.mReaderSettups
                                 join zn in db.mZones on Dis.mZoneId equals zn.mZoneId
                                 join fm in db.mFloorMasters on Dis.mFloorMasterId equals fm.mFloorMasterId
                                 join rm in db.mRoomMasters on Dis.mRoomMasterId equals rm.mRoomMasterId
                                 where (Dis.IsAction == true)
                                 select new { Dis.mReaderSettupId, Dis.ReaderNo, Dis.AttPortId, rm.RoomName, Dis.mRoomMasterId, zn.mZoneId, zn.Zone, fm.mFloorMasterId, fm.FloorName }).ToList();

                var objText = (from tm in db.mShelves
                               join p in db.mSMTProducts on tm.Barcode equals p.Ble into p
                               from _p in p.DefaultIfEmpty()
                               select new
                               {
                                   _p.Status,
                                   _p.PartNumber,
                                   _p.SerialNumber,
                                   tm.mShelfId,
                                   tm.mZoneId,
                                   tm.Barcode,
                                   tm.CreatedBy,
                                   tm.CreatedDate,
                                   tm.IsAction,
                                   tm.ModifiedBy,
                                   tm.ModifiedDate,
                                   tm.ShelfName
                               }).ToList();

                // FIXED: Load actual assets from tAssetTag table with Zone and Floor references
                // Each asset in tAssetTag has mZoneId and mFloorMasterId columns
                // We join with mZones and mFloorMasters to get the names
                var AssetTagData = (from ast in db.tAssetTags
                                    join zn in db.mZones on ast.mZoneId equals zn.mZoneId into znJoin
                                    from zn in znJoin.DefaultIfEmpty()
                                    join fl in db.mFloorMasters on ast.mFloorMasterId equals fl.mFloorMasterId into flJoin
                                    from fl in flJoin.DefaultIfEmpty()
                                    where ast.IsAction == true || ast.IsAction == null
                                    select new
                                    {
                                        ast.tAssetTagId,
                                        ast.IteamName,
                                        ast.IteamCode,
                                        ast.AssetID,
                                        ast.RFID,
                                        ast.BarCode,
                                        ast.SerialNo,
                                        ast.Model,
                                        ast.ModelNo,
                                        ast.mZoneId,
                                        ast.mFloorMasterId,
                                        ast.mRoomMasterId,
                                        ZoneName = zn.Zone ?? "",
                                        FloorName = fl.FloorName ?? "",
                                        ast.IsAction,
                                        ast.mStatusMasterId,
                                        ast.ModifiedDate
                                    }).ToList();

                return Json(new { 
                    IFloorData = FloorData, 
                    IObjData = RoomData, 
                    IPortsData = PortsData, 
                    IZoneData = ZoneData, 
                    IsubZoneData = subZoneData, 
                    objText,
                    AssetTagData // NEW: Actual assets from database with zone/floor references
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // HEXARTLS: NEW - Get Assets By Floor
        // Returns all assets that belong to a specific floor
        [HttpGet]
        public JsonResult GetAssetsByFloor(int mFloorMasterId)
        {
            using (var db = new ERPdbEntities())
            {
                var assets = (from ast in db.tAssetTags
                              join zn in db.mZones on ast.mZoneId equals zn.mZoneId into znJoin
                              from zn in znJoin.DefaultIfEmpty()
                              join fl in db.mFloorMasters on ast.mFloorMasterId equals fl.mFloorMasterId into flJoin
                              from fl in flJoin.DefaultIfEmpty()
                              where ast.mFloorMasterId == mFloorMasterId
                              select new
                              {
                                  ast.tAssetTagId,
                                  ast.IteamName,
                                  ast.IteamCode,
                                  ast.AssetID,
                                  ast.RFID,
                                  ast.BarCode,
                                  ast.SerialNo,
                                  ast.Model,
                                  ast.ModelNo,
                                  ast.mZoneId,
                                  ast.mFloorMasterId,
                                  ast.mRoomMasterId,
                                  ZoneName = zn.Zone ?? "",
                                  FloorName = fl.FloorName ?? "",
                                  ast.IsAction,
                                  ast.mStatusMasterId,
                                  ast.ModifiedDate,
                                  ast.IteamDescription
                              }).ToList();

                return Json(new { assets }, JsonRequestBehavior.AllowGet);
            }
        }

        // HEXARTLS: NEW - Get Assets By Zone
        // Returns all assets that belong to a specific zone/location
        [HttpGet]
        public JsonResult GetAssetsByZone(int mZoneId)
        {
            using (var db = new ERPdbEntities())
            {
                var assets = (from ast in db.tAssetTags
                              join zn in db.mZones on ast.mZoneId equals zn.mZoneId into znJoin
                              from zn in znJoin.DefaultIfEmpty()
                              join fl in db.mFloorMasters on ast.mFloorMasterId equals fl.mFloorMasterId into flJoin
                              from fl in flJoin.DefaultIfEmpty()
                              where ast.mZoneId == mZoneId
                              select new
                              {
                                  ast.tAssetTagId,
                                  ast.IteamName,
                                  ast.IteamCode,
                                  ast.AssetID,
                                  ast.RFID,
                                  ast.BarCode,
                                  ast.SerialNo,
                                  ast.Model,
                                  ast.ModelNo,
                                  ast.mZoneId,
                                  ast.mFloorMasterId,
                                  ast.mRoomMasterId,
                                  ZoneName = zn.Zone ?? "",
                                  FloorName = fl.FloorName ?? "",
                                  ast.IsAction,
                                  ast.mStatusMasterId,
                                  ast.ModifiedDate,
                                  ast.IteamDescription
                              }).ToList();

                return Json(new { assets }, JsonRequestBehavior.AllowGet);
            }
        }

        //HexaRTLS/getGetToTrackData
        [HttpGet]
        public async Task<ActionResult> GetTrackData(int mZoneId)
        {
            JsonResult result = new JsonResult();
            try
            {
                using (var db = new ERPdbEntities())
                {
                    var objText = await (from tm in db.mShelves
                                         where tm.mZoneId == mZoneId
                                         select new
                                         {
                                             tm.mShelfId,
                                             tm.mZoneId,
                                             tm.Barcode,
                                             tm.CreatedBy,
                                             tm.CreatedDate,
                                             tm.IsAction,
                                             tm.ModifiedBy,
                                             tm.ModifiedDate,
                                             tm.ShelfName
                                         }).ToListAsync();

                    result = this.Json(new { objText }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = string.Empty;
                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;
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
    }
}
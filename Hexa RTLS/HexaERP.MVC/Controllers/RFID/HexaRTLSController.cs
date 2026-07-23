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

        // HEXARTLS: NEW - Search Asset by ID, RFID, Barcode, or Name
        // Returns matching assets with their latest location information
        [HttpGet]
        public JsonResult SearchAsset(string searchTerm)
        {
            using (var db = new ERPdbEntities())
            {
                var searchLower = searchTerm.ToLower();
                
                var assets = (from ast in db.tAssetTags
                              join zn in db.mZones on ast.mZoneId equals zn.mZoneId into znJoin
                              from zn in znJoin.DefaultIfEmpty()
                              join fl in db.mFloorMasters on ast.mFloorMasterId equals fl.mFloorMasterId into flJoin
                              from fl in flJoin.DefaultIfEmpty()
                              join rm in db.mRoomMasters on ast.mRoomMasterId equals rm.mRoomMasterId into rmJoin
                              from rm in rmJoin.DefaultIfEmpty()
                             where (ast.IsAction == true || ast.IsAction == null)
                                 && ((ast.AssetID ?? "").ToLower().Contains(searchLower)
                                     || (ast.RFID ?? "").ToLower().Contains(searchLower)
                                     || (ast.BarCode ?? "").ToLower().Contains(searchLower)
                                     || (ast.IteamName ?? "").ToLower().Contains(searchLower))
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
                                  RoomName = rm.RoomName ?? "",
                                  ast.IsAction,
                                  ast.mStatusMasterId,
                                  ast.ModifiedDate,
                                  ast.IteamDescription
                              }).ToList();

                return Json(new { assets }, JsonRequestBehavior.AllowGet);
            }
        }

        // HEXARTLS: NEW - Search Asset and Get Latest Location
        // Searches asset by ID, RFID, Barcode, or Name and returns latest location with coordinates
        // Data Flow: Asset (tAssetTag) -> toMonitor (latest RFID scan) -> mReaderSettups (coordinates)
        // Returns: Asset details, FloorId, RoomId, ReaderId, X/Y coordinates
        [HttpGet]
        public JsonResult SearchAssetAndGetLocation(string searchTerm)
        {
            using (var db = new ERPdbEntities())
            {
                var searchLower = searchTerm.ToLower();
                
                // Step 1: Find the asset
                var asset = (from ast in db.tAssetTags
                             join zn in db.mZones on ast.mZoneId equals zn.mZoneId into znJoin
                             from zn in znJoin.DefaultIfEmpty()
                             join fl in db.mFloorMasters on ast.mFloorMasterId equals fl.mFloorMasterId into flJoin
                             from fl in flJoin.DefaultIfEmpty()
                             join rm in db.mRoomMasters on ast.mRoomMasterId equals rm.mRoomMasterId into rmJoin
                             from rm in rmJoin.DefaultIfEmpty()
                             where (ast.IsAction == true || ast.IsAction == null)
                                 && ((ast.AssetID ?? "").ToLower().Contains(searchLower)
                                     || (ast.RFID ?? "").ToLower().Contains(searchLower)
                                     || (ast.BarCode ?? "").ToLower().Contains(searchLower)
                                     || (ast.IteamName ?? "").ToLower().Contains(searchLower))
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
                                 RoomName = rm.RoomName ?? "",
                                 ast.IsAction,
                                 ast.mStatusMasterId,
                                 ast.ModifiedDate,
                                 ast.IteamDescription
                             }).FirstOrDefault();

                if (asset == null)
                {
                    return Json(new { 
                        success = false, 
                        message = "Asset not found" 
                    }, JsonRequestBehavior.AllowGet);
                }

                // Step 2: Get latest tracking from toMonitor (live tracking table)
                // Join toMonitor with mReaderSettups to get coordinates (Xaxis, Yaxis)
                var latestTrack = (from tm in db.toMonitors
                                   join rst in db.mReaderSettups on tm.Name equals rst.ReaderNo
                                   where tm.Epc == asset.RFID
                                   orderby tm.tDate descending
                                   select new
                                   {
                                       tm.Epc,
                                       tm.tDate,
                                       rst.mReaderSettupId,
                                       rst.mFloorMasterId,
                                       rst.mRoomMasterId,
                                       rst.mIndooMapsId,
                                       rst.Xaxis,
                                       rst.Yaxis,
                                       rst.ReaderNo,
                                       rst.AttPortId,
                                       FloorName = db.mFloorMasters
                                           .Where(f => f.mFloorMasterId == rst.mFloorMasterId)
                                           .Select(f => f.FloorName)
                                           .FirstOrDefault() ?? "",
                                       RoomName = db.mRoomMasters
                                           .Where(r => r.mRoomMasterId == rst.mRoomMasterId)
                                           .Select(r => r.RoomName)
                                           .FirstOrDefault() ?? "",
                                       ZoneName = db.mZones
                                           .Where(z => z.mZoneId == rst.mZoneId)
                                           .Select(z => z.Zone)
                                           .FirstOrDefault() ?? ""
                                   }).FirstOrDefault();

                if (latestTrack != null)
                {
                    return Json(new { 
                        success = true, 
                        asset = asset,
                        location = latestTrack
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { 
                        success = true, 
                        asset = asset,
                        location = (object)null,
                        message = "Asset found but no tracking data available"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        // HEXARTLS: NEW - Get Asset Latest Location with Coordinates
        // Returns the latest tracked position (X/Y) for an asset from toTrackInfo
        [HttpGet]
        public JsonResult GetAssetLatestLocation(string rfid)
        {
            using (var db = new ERPdbEntities())
            {
                // Get the latest tracking record for this RFID from toTrackInfo
                var latestTrack = (from track in db.toTrackInfoes
                                   join rst in db.mReaderSettups on track.mAttPortId equals rst.AttPortId
                                   where track.RFID == rfid
                                   orderby track.tDate descending
                                   select new
                                   {
                                       track.RFID,
                                       track.tDate,
                                       rst.mFloorMasterId,
                                       rst.mIndooMapsId,
                                       rst.Xaxis,
                                       rst.Yaxis,
                                       rst.ReaderNo,
                                       FloorName = db.mFloorMasters
                                           .Where(f => f.mFloorMasterId == rst.mFloorMasterId)
                                           .Select(f => f.FloorName)
                                           .FirstOrDefault() ?? "",
                                       ZoneName = db.mZones
                                           .Where(z => z.mZoneId == rst.mZoneId)
                                           .Select(z => z.Zone)
                                           .FirstOrDefault() ?? ""
                                   }).FirstOrDefault();

                if (latestTrack != null)
                {
                    return Json(new { 
                        success = true, 
                        location = latestTrack 
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { 
                        success = false, 
                        message = "No tracking data found for this asset" 
                    }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        // HEXARTLS: NEW - Get Multiple Floor Maps Data
        // Returns map data for multiple selected floors
        [HttpGet]
        public JsonResult GetMultiFloorMaps(List<int> floorIds)
        {
            using (var db = new ERPdbEntities())
            {
                var floorMaps = (from flr in db.mFloorMasters
                                 join rst in db.mReaderSettups on flr.mFloorMasterId equals rst.mFloorMasterId into rstJoin
                                 from rst in rstJoin.DefaultIfEmpty()
                                 join ind in db.mIndooMaps on rst.mIndooMapsId equals ind.mIndooMapsId into indJoin
                                 from ind in indJoin.DefaultIfEmpty()
                                 where floorIds.Contains(flr.mFloorMasterId) && flr.IsAction == true
                                 select new
                                 {
                                     flr.mFloorMasterId,
                                     flr.FloorName,
                                     flr.FloorNo,
                                     flr.mZoneId,
                                     ZoneName = db.mZones
                                         .Where(z => z.mZoneId == flr.mZoneId)
                                         .Select(z => z.Zone)
                                         .FirstOrDefault() ?? "",
                                     MapId = ind != null ? ind.mIndooMapsId : (int?)null,
                                     MapPath = ind != null ? ind.ImgPath : "",
                                     MapUID = ind != null ? ind.UID : ""
                                 }).ToList();

                // Get reader positions for these floors
                var readerPositions = (from rst in db.mReaderSettups
                                       where floorIds.Contains(rst.mFloorMasterId ?? 0) && rst.IsAction == true
                                       select new
                                       {
                                           rst.mReaderSettupId,
                                           rst.mFloorMasterId,
                                           rst.mIndooMapsId,
                                           rst.Xaxis,
                                           rst.Yaxis,
                                           rst.ReaderNo,
                                           rst.AttPortId
                                       }).ToList();

                return Json(new { 
                    floorMaps = floorMaps,
                    readerPositions = readerPositions
                }, JsonRequestBehavior.AllowGet);
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
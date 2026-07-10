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

        //private ERPdbEntities db = new ERPdbEntities();
        // GET: HexaRTLS
        [Authorize(Roles = "AD,PAK,AAD,SA")]
        public ActionResult Index()
        {
            try
            {
                //--- Get cookie Collection.
                HttpCookie cookieObject = Request.Cookies["HexaCookie"];

                //--- Check for null 
                if (cookieObject != null)
                {
                    ViewBag.LogedIn = cookieObject["AppUserName"];
                    //--- To read values from cookie collection we will use Keys used while creating cookie.                   
                    // string AppUserName = cookieObject["AppUserName"];
                    //string UniqueId = cookieObject["UniqueId"];
                    //string OrgInfoId = cookieObject["OrgInfoId"];
                    //string SortCode = cookieObject["SortCode"];

                    //System.IO.File.WriteAllText(Server.MapPath("~/Content/EmployeeTag.json"), null);

                }
                else { return RedirectToAction("Index", "AppUser"); }

                //if (Session["UniqueId"].ToString() != "" && Session["OrgInfoId"].ToString() != "" && Session["AppUserName"].ToString() != "")
                //{

                //}
                //else
                //{
                //    return RedirectToAction("Index", "AppUser");
                //}
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "AppUser");
            }
            return View();
        }

        // GET: HexaRTLS/Details
        [HttpGet]
        public JsonResult Details()
        {
            using (StreamReader sr = new StreamReader(Server.MapPath("~/Content/EmployeeTag.json")))
            {
                var _tEmployeeTag = JsonConvert.DeserializeObject<List<tEmployeeTag>>(sr.ReadToEnd());
                return Json(_tEmployeeTag.ToArray(), JsonRequestBehavior.AllowGet);
            }
            //return streturn;
        }

        //HexaRTLS/getlocationdata  
        [HttpGet]
        public JsonResult getlocationdata()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
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
                                //join R in db.mReaderSettups on Dis.mRoomMasterId equals R.mRoomMasterId
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

                //Convert List Data to The Json Array                     
                return Json(new { IFloorData = FloorData, IObjData = RoomData, IPortsData = PortsData, IZoneData = ZoneData, IsubZoneData = subZoneData, objText }, JsonRequestBehavior.AllowGet);
            }
        }

        //HexaRTLS/getGetToTrackData
        [HttpGet]
        public async Task<ActionResult> GetTrackData(int mZoneId)
        {
            // Initialization.    
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
                // t = DateTime.Now - Convert.ToDateTime(tm.tDate)
                //days =(DateTime.Now - tm.tDate).TotalDays
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

            //string source = "";
            ////var path = System.IO.File.ReadAllText(Server.MapPath("~/Content/EmployeeTag.json"));
            //var path = Server.MapPath("~/Content/EmployeeTag.json");
            //using (StreamReader SourceReaderr = new StreamReader(path))
            //{
            //    source = await SourceReaderr.ReadToEndAsync();
            //}
            ////var path = System.IO.File.ReadAllText(Server.MapPath("~/Content/EmployeeTag.json"));
            //if (source != null)
            //{
            //    List<tToolTrackDemo> tagsTrack = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<tToolTrackDemo>>(source));
            //    //List<tToolTrackDemo> tagsTrack = JsonConvert.DeserializeObject<List<tToolTrackDemo>>(path);
            //    try
            //    {
            //        var ObjDatass = (from tm in tagsTrack
            //                         join emp in db.tEmployeeTags on tm.Epc equals emp.RFID
            //                         join ag in db.mAgencies on emp.mAgencyId equals ag?.mAgencyId into ag
            //                         join ds in db.mDesignations on emp.mDesignationId equals ds?.mDesignationId into ds
            //                         join sk in db.mSkillCategories on emp.mSkillCategoryId equals sk?.mSkillCategoryId into sk
            //                         join wk in db.mWorkCategories on emp.mWorkCategoryId equals wk?.mWorkCategoryId into wk
            //                         join ac in db.mActivities on emp.mActivityId equals ac?.mActivityId into ac
            //                         join rst in db.mReaderSettups on tm.Reader equals rst.ReaderNo
            //                         where tm.PortId == rst.AttPortId
            //                         from _ag in ag.DefaultIfEmpty(new mAgency { Agency = "Not Assinged" })
            //                         from _ds in ds.DefaultIfEmpty(new mDesignation { Designation = "Not Assinged" })
            //                         from _sk in sk.DefaultIfEmpty(new mSkillCategory { SkillCategory = "Not Assinged" })
            //                         from _wk in wk.DefaultIfEmpty(new mWorkCategory { WorkCategory = "Not Assinged" })
            //                         from _ac in ac.DefaultIfEmpty(new mActivity { Activity = "Not Assinged" })
            //                         select new
            //                         {
            //                             Agency = _ag.Agency ?? string.Empty,
            //                             Designation = _ds.Designation ?? string.Empty,
            //                             SkillCategory = _sk.SkillCategory ?? string.Empty,
            //                             WorkCategory = _wk.WorkCategory ?? string.Empty,
            //                             Activity = _ac.Activity ?? string.Empty,
            //                             Name = emp.EmployeeName ?? string.Empty,
            //                             EmployeeId = emp.EmployeeId ?? string.Empty,
            //                             tm.Epc,
            //                             tm.tDate,
            //                             rst.mFloorMasterId,
            //                             rst.mZoneId,
            //                             eZoneId = emp.mZoneId,
            //                             IsAE = true
            //                         }
            //                     ).ToList();

            //        var objAsset = (from tm in tagsTrack
            //                        join ast in db.tAssetTags on tm.Epc equals ast.RFID
            //                        join emp in db.tEmployeeTags on ast.tEmployeeTagId equals (int?)emp.tEmployeeTagId into empe
            //                        join itm in db.mIteamMasters on ast.mIteamMasterId equals itm?.mIteamMasterId into itm
            //                        join msit in db.mSiteMasters on ast.mSiteMasterId equals msit?.mSiteMasterId into msit
            //                        join mzon in db.mZones on ast.mZoneId equals mzon?.mZoneId
            //                        join mflr in db.mFloorMasters on ast.mFloorMasterId equals mflr?.mFloorMasterId into mflr
            //                        join mrom in db.mRoomMasters on ast.mRoomMasterId equals mrom?.mRoomMasterId into mrom
            //                        join rst in db.mReaderSettups on tm.Reader equals rst.ReaderNo
            //                        where tm.PortId == rst.AttPortId
            //                        from _itm in itm.DefaultIfEmpty(new mIteamMaster { IteamName = "Not Yet Assinged" })
            //                        from _msit in msit.DefaultIfEmpty(new mSiteMaster { Site = "Not Yet Assinged" })
            //                        from _mflr in mflr.DefaultIfEmpty(new mFloorMaster { FloorName = "Not Yet Assinged" })
            //                        from _mrom in mrom.DefaultIfEmpty(new mRoomMaster { RoomName = "Not Yet Assinged" })
            //                        from _emp in empe.DefaultIfEmpty(new tEmployeeTag { EmployeeName = "Not Yet Assinged" })
            //                        select new
            //                        {
            //                            EmployeeId = _emp.EmployeeId ?? string.Empty,
            //                            EmployeeName = _emp.EmployeeName ?? string.Empty,
            //                            EmailId = _emp.EmailId ?? string.Empty,
            //                            ContactNo = _emp.ContactNo ?? string.Empty,
            //                            Model = ast.Model ?? string.Empty,
            //                            ModelNo = ast.ModelNo ?? string.Empty,
            //                            img = ast.img ?? string.Empty,
            //                            Rack = _mflr.FloorName ?? string.Empty,
            //                            Shelf = _mrom.RoomName ?? string.Empty,
            //                            Asset = _itm.IteamName ?? string.Empty,
            //                            IteamName = ast.IteamName ?? string.Empty,
            //                            IteamCode = ast.IteamCode ?? string.Empty,
            //                            IteamDescription = ast.IteamDescription ?? string.Empty,
            //                            ast.bStock,
            //                            tm.Epc,
            //                            tm.tDate,
            //                            rst.mFloorMasterId,
            //                            rst.mZoneId,
            //                            eZoneId = ast.mZoneId,
            //                            IsAE = false,
            //                            _msit.Site,
            //                            mzon.Zone
            //                        }
            //                    ).ToList();

            //        result = this.Json(new { ObjDatass = ObjDatass.ToArray(), objAsset = objAsset.ToArray() }, JsonRequestBehavior.AllowGet);
            //        // t = DateTime.Now - Convert.ToDateTime(tm.tDate)
            //        //days =(DateTime.Now - tm.tDate).TotalDays
            //    }
            //    catch (DbEntityValidationException ex)
            //    {
            //        var message = string.Empty;
            //        foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
            //        {
            //            // Get entry
            //            DbEntityEntry entry = item.Entry;
            //            string entityTypeName = entry.Entity.GetType().Name;
            //            // Display or log error messages
            //            foreach (DbValidationError subItem in item.ValidationErrors)
            //            {
            //                message = string.Format("Error '{0}' occurred in {1} at {2}",
            //                         subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
            //                Console.WriteLine(message);
            //            }
            //        }
            //        result = this.Json(new { message, Flag = false }, JsonRequestBehavior.AllowGet);
            //    }
            //    catch (Exception ex)
            //    {
            //        result = this.Json(new { ex.InnerException.Message, Flag = false, Exceptions = "Exception" }, JsonRequestBehavior.AllowGet);
            //    }

            //}
            //else
            //{
            //    result = this.Json(new { Message = "Null Data", Flag = false }, JsonRequestBehavior.AllowGet);
            //}
            return result;
        }
    }
}

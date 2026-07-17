using HexaERP.MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HexaERP.MVC.Controllers.Report
{
    public class AssetsReportController : Controller
    {
        //private ERPdbEntities db = new ERPdbEntities();

        // GET: AssetsReport
        [Authorize(Roles = "AD,AAD,SA")]
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
                }
                else { return RedirectToAction("Index", "AppUser"); }

            }
            catch (Exception)
            {
                return RedirectToAction("Index", "AppUser");
            }
            return View();
        }

        //AssetsReport /Get All Master
        [HttpGet]
        public JsonResult getMasterData()
        {
            JsonResult result = new JsonResult();

            try
            {
                using (var db = new ERPdbEntities())
                {
                    var mSite = db.mSiteMasters
                        .Where(x => x.IsAction == true)
                        .Select(s => new { SiteId = s.mSiteMasterId, SiteName = s.Site })
                        .ToList();

                    var mststu = db.mStatusMasters
                        .Where(x => x.IsAction == true)
                        .Select(s => new { StatusId = s.mStatusMasterId, StatusName = s.StatusName })
                        .ToList();

                    var mGroup = db.mGroupMasters
                        .Where(x => x.IsAction == true)
                        .Select(s => new { GroupId = s.mGroupMasterId, GroupName = s.GroupName })
                        .ToList();

                    var mIteam = db.mIteamMasters
                        .Where(x => x.IsAction == true)
                        .Select(s => new { ItemId = s.mIteamMasterId, ItemName = s.IteamName })
                        .ToList();

                    var mIteamType = db.mIteamTypeMasters
                        .Where(x => x.IsAction == true)
                        .Select(s => new { ItemTypeId = s.mIteamTypeMasterId, ItemType = s.IteamType })
                        .ToList();

                    var mVendor = db.mVendors
                        .Where(x => x.IsAction == true)
                        .Select(s => new { VendorId = s.mVendorId, VendorName = s.VendorName })
                        .ToList();

                    result = this.Json(new { Flag = true, mSite, mststu, mGroup, mIteam, mIteamType, mVendor }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Flag = false,
                    Message = ex.Message,
                    Inner = ex.InnerException != null ? ex.InnerException.Message : ""
                }, JsonRequestBehavior.AllowGet);
            }
            //Convert List Data to The Json Array          
            return result;
        }
        // GET: AssetsReport/getZones
        [HttpGet]
        public JsonResult getZones(int id)
        {
            JsonResult result = new JsonResult();
            try
            {
                using (var db = new ERPdbEntities())
                {
                    var getZone = db.mZones.Where(x => x.IsAction == true && x.mSiteMasterId == id).ToList();
                    return Json(new { Flag = true, Message = "Data Loaded Sucessfully", DZone = getZone.ToArray() }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                result = this.Json(new { Flag = false, Message = ex.Message, Inner = ex.InnerException != null ? ex.InnerException.Message : "" }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }
        // GET: AssetsReport/getSubZones
        [HttpGet]
        public JsonResult getSubZones(int id)
        {
            try
            {
                using (var db = new ERPdbEntities())
                {
                    var getSubZone = db.mFloorMasters.Where(x => x.IsAction == true && x.mZoneId == id).ToList();
                    return Json(new { Flag = true, Message = "Data Loaded Sucessfully", DZone = getSubZone.ToArray() }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        // GET: AssetsReport/Area
        [HttpGet]
        public JsonResult getArea(int id)
        {
            try
            {
                using (var db = new ERPdbEntities())
                {
                    var mroom = db.mRoomMasters.Where(x => x.IsAction == true && x.mFloorMasterId == id).ToList();
                    return Json(new { Flag = true, Message = "Data Loaded Sucessfully", DArea = mroom.ToArray() }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        //
        public static List<string> InQueryMaker(string arrayString)
        {
            try
            {
                return arrayString.Split(',').ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }
        //
        public static List<int> IntQueryMaker(string arrayString)
        {
            try
            {
                return arrayString.Split(',').Select(x => x.Trim()).Select(x => Int32.Parse(x)).ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }
        public DateTime ConvertToDateTime(string strDateTime)
        {
            DateTime dtFinaldate; string sDateTime;
            try { dtFinaldate = Convert.ToDateTime(strDateTime); }
            catch (Exception)
            {
                string[] sDate = strDateTime.Split('/');
                sDateTime = sDate[1] + '/' + sDate[0] + '/' + sDate[2];
                dtFinaldate = Convert.ToDateTime(sDateTime);
            }
            return dtFinaldate;
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> GetAssetReport(string Barcode, string UniqueID, string dateFrom, string toDate, string Location, string Zone, string Floor, string AssetCategory, string AssetSubCategory, string AssetType, string Vendor, string Status, int PageIndex = 1, int PageSize = 10)
        {
            JsonResult result = new JsonResult();
            try
            {
                DateTime _fDate;
                DateTime tDate;
                List<string> BarcodeFilter = null;
                List<string> UniqueIDFilter = null;

                IQueryable<tAssetTag> FiltterDataList = null;

                using (var o = new ERPdbEntities())
                {
                    FiltterDataList = (from t in o.tAssetTags
                                       where t.IsAction == true
                                       select t);

                    if (!string.IsNullOrWhiteSpace(dateFrom) && !string.IsNullOrWhiteSpace(toDate))
                    {
                        _fDate = ConvertToDateTime(dateFrom);
                        tDate = ConvertToDateTime(toDate);
                        FiltterDataList = FiltterDataList.Where(t => t.CreatedDate >= _fDate && t.CreatedDate <= tDate);
                    }

                    if (!string.IsNullOrEmpty(Barcode) && Barcode != "undefined")
                    {
                        BarcodeFilter = InQueryMaker(Barcode.ToLower());
                        FiltterDataList = FiltterDataList.Where(x => BarcodeFilter.Contains(x.BarCode.ToLower()));
                    }

                    if (!string.IsNullOrEmpty(UniqueID) && UniqueID != "undefined")
                    {
                        UniqueIDFilter = InQueryMaker(UniqueID.ToLower());
                        FiltterDataList = FiltterDataList.Where(x => UniqueIDFilter.Contains(x.UID.ToLower()));
                    }

                    if (!string.IsNullOrEmpty(Location) && Location != "undefined" && Location != "ALL")
                    {
                        int siteId = int.Parse(Location);
                        FiltterDataList = FiltterDataList.Where(x => x.mSiteMasterId == siteId);
                    }

                    if (!string.IsNullOrEmpty(Zone) && Zone != "undefined" && Zone != "ALL")
                    {
                        int zoneId = int.Parse(Zone);
                        FiltterDataList = FiltterDataList.Where(x => x.mZoneId == zoneId);
                    }

                    if (!string.IsNullOrEmpty(Floor) && Floor != "undefined" && Floor != "ALL")
                    {
                        int floorId = int.Parse(Floor);
                        FiltterDataList = FiltterDataList.Where(x => x.mFloorMasterId == floorId);
                    }

                    if (!string.IsNullOrEmpty(AssetCategory) && AssetCategory != "undefined" && AssetCategory != "ALL")
                    {
                        FiltterDataList = FiltterDataList.Where(x => x.mGroupMasterId.ToString() == AssetCategory);
                    }

                    if (!string.IsNullOrEmpty(AssetSubCategory) && AssetSubCategory != "undefined" && AssetSubCategory != "ALL")
                    {
                        FiltterDataList = FiltterDataList.Where(x => x.mIteamMasterId.ToString() == AssetSubCategory);
                    }

                    if (!string.IsNullOrEmpty(AssetType) && AssetType != "undefined" && AssetType != "ALL")
                    {
                        FiltterDataList = FiltterDataList.Where(x => x.mIteamTypeMasterId.ToString() == AssetType);
                    }

                    if (!string.IsNullOrEmpty(Vendor) && Vendor != "undefined" && Vendor != "ALL")
                    {
                        int vendorId = int.Parse(Vendor);
                        FiltterDataList = FiltterDataList.Where(x => x.mVendorId == vendorId);
                    }

                    if (!string.IsNullOrEmpty(Status) && Status != "undefined" && Status != "ALL")
                    {
                        int statusId = int.Parse(Status);
                        FiltterDataList = FiltterDataList.Where(x => x.mStatusMasterId == statusId);
                    }

                    var Result = FiltterDataList.OrderBy(x => x.CreatedDate)
                                    .Skip((PageIndex - 1) * PageSize)
                                    .Take(PageSize)
                                    .ToList();

                    int TotalRecords = FiltterDataList.Count();

                    if (PageIndex == 1)
                        TempData["AssetList"] = FiltterDataList.ToList();

                    result = this.Json(new
                    {
                        TotalRecords,
                        Flag = true,
                        Result,
                        Message = TotalRecords == 0 ? "No Assets Available for this Floor." : String.Format("PageIndex :{0} PageSize:{1}", PageIndex, PageSize),
                        PageIndex,
                        PageSize
                    }, JsonRequestBehavior.AllowGet);
                    result.MaxJsonLength = int.MaxValue;

                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = new List<string>();
                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;
                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        message.Add(string.Format($"Error {subItem.ErrorMessage} occurred in {entityTypeName} at {subItem.PropertyName}"));
                    }
                }
                return Json(new { message, Flag = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                result = this.Json(new { ex.Message, Flag = false, Exceptions = "Exception" }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }

        //GET:AssetsReport
        public async Task<ActionResult> ExportExcelAsset()
        {
            try
            {
                var ExcelExport = (List<tAssetTag>)TempData["AssetList"];

                if (ExcelExport != null)
                {
                    using (var db = new ERPdbEntities())
                    {
                        var assetIds = ExcelExport.Select(x => x.tAssetTagId).ToList();

                        var l = (
                            from a in db.tAssetTags
                            join g in db.mGroupMasters on a.mGroupMasterId equals g.mGroupMasterId into gg
                            from g in gg.DefaultIfEmpty()
                            join t in db.mIteamTypeMasters on a.mIteamTypeMasterId equals t.mIteamTypeMasterId into tt
                            from t in tt.DefaultIfEmpty()
                            join v in db.mVendors on a.mVendorId equals v.mVendorId into vv
                            from v in vv.DefaultIfEmpty()
                            join s in db.mStatusMasters on a.mStatusMasterId equals s.mStatusMasterId into ss
                            from s in ss.DefaultIfEmpty()
                            join z in db.mZones on a.mZoneId equals z.mZoneId into zz
                            from z in zz.DefaultIfEmpty()
                            join f in db.mFloorMasters on a.mFloorMasterId equals f.mFloorMasterId into ff
                            from f in ff.DefaultIfEmpty()
                            where assetIds.Contains(a.tAssetTagId)
                            select new
                            {
                                AssetName = a.IteamName,
                                AssetNumber = a.AssetID,
                                BarCode = a.BarCode,
                                SerialNo = a.SerialNo,
                                Category = g == null ? "" : g.GroupName,
                                AssetType = t == null ? "" : t.IteamType,
                                Vendor = v == null ? "" : v.VendorName,
                                Status = s == null ? "" : s.StatusName,
                                Zone = z == null ? "" : z.Zone,
                                Floor = f == null ? "" : f.FloorName,
                                CurrentLocation = a.CurrentLocation,
                                CreatedDate = a.CreatedDate,
                                CreatedBy = a.CreatedBy,
                                ModifiedDate = a.ModifiedDate,
                                ModifiedBy = a.ModifiedBy
                            }
                        ).ToList();

                        var grid = new GridView();
                        grid.DataSource = l;
                        grid.DataBind();
                        var FileName = "Asset-Report" + DateTime.Now.ToString("s") + "_.xls";
                        Response.ClearContent();
                        Response.AddHeader("content-disposition", "attachement; filename=" + FileName + "");
                        Response.ContentType = "application/excel";
                        StringWriter sw = new StringWriter();
                        HtmlTextWriter htw = new HtmlTextWriter(sw);
                        grid.RenderControl(htw);
                        Response.Output.Write(sw.ToString());
                        Response.Flush();
                        Response.End();
                    }
                }
                else
                {
                    return new EmptyResult();
                }
            }
            catch (Exception ex)
            {
                return new EmptyResult();
            }
            return View();
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> GetAssetMoreDetailsReport(string searchValue, int id, int PageIndex = 1, int PageSize = 10)
        {
            JsonResult result = new JsonResult();
            try
            {

                // IQueryable<SmtProductAuditLog> FiltterDataList = null;

                using (var o = new ERPdbEntities())
                {
                    var Result = (from t in o.SmtProductAuditLogs
                                  where t.mSMTProductId == id
                                  select t).OrderBy(x => x.TrxTimestamp)
                                     .Skip((PageIndex - 1) * PageSize)
                                     .Take(PageSize)
                                     .ToList();


                    //var Result = FiltterDataList.OrderBy(x => x.TrxTimestamp)
                    //                .Skip((PageIndex - 1) * PageSize)
                    //                .Take(PageSize)
                    //                .ToList();

                    int TotalRecords = Result.Count();

                    result = this.Json(new
                    {
                        keys = new AssetAuditDisplayModelview(),
                        TotalRecords,
                        Flag = true,
                        Result,
                        Message = String.Format("PageIndex :{0} PageSize:{1}", PageIndex, PageSize),
                        PageIndex,
                        PageSize
                    }, JsonRequestBehavior.AllowGet);
                    result.MaxJsonLength = int.MaxValue;
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = new List<string>();
                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    // Get entry
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;
                    // Display or log error messages
                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        message.Add(string.Format($"Error {subItem.ErrorMessage} occurred in {entityTypeName} at {subItem.PropertyName}"));
                    }
                }
                return Json(new { message, Flag = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                result = this.Json(new { ex.Message, Flag = false, Exceptions = "Exception" }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }
    }
}

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
                    //var UserName = Session["AppUserName"];
                    ////Get Organization Id From Session Variable
                    //int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                    //Get Selected Data Accourding to Org Id

                    var mSite = db.mSiteMasters.Where(x => x.IsAction == true).ToList();
                    var mststu = db.mStatusMasters.Where(x => x.IsAction == true).ToList();
                    result = this.Json(new { Flag = true, mSite, mststu }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                result = this.Json(new { Flag = false, Message = ex.InnerException.Message }, JsonRequestBehavior.AllowGet);
            }
            //Convert List Data to The Json Array          
            return result;
        }
        // GET: AssetsReport/getZones
        [HttpGet]
        public JsonResult getZones(int id)
        {
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
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

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
        public async Task<JsonResult> GetAssetReport(string SerialNumber, string Ble, string dateFrom, string toDate, String Lot, string Status, string PartNumber, int PageIndex = 1, int PageSize = 10)
        {
            JsonResult result = new JsonResult();
            try
            {
                DateTime _fDate;
                DateTime tDate;
                List<string> BleFilter = null;
                List<string> SerialNumberFilter = null;
                List<string> PartNumberFilter = null;
                List<string> LotNumberFilter = null;

                IQueryable<mSMTProduct> FiltterDataList = null;

                using (var o = new ERPdbEntities())
                {
                    if (!string.IsNullOrWhiteSpace(dateFrom) && !string.IsNullOrWhiteSpace(toDate))
                    {
                        _fDate = ConvertToDateTime(dateFrom);
                        tDate = ConvertToDateTime(toDate);
                        FiltterDataList = (from t in o.mSMTProducts where t.IsAction == true && t.CreatedDate >= _fDate && t.CreatedDate <= tDate select t);
                    }
                    else
                        FiltterDataList = (from t in o.mSMTProducts where t.IsAction == true select t);

                    if (!string.IsNullOrEmpty(Ble) && Ble != "undefined")
                    {
                        BleFilter = InQueryMaker(Ble.ToLower());
                        FiltterDataList = FiltterDataList.Where(x => BleFilter.Contains(x.Ble.ToLower()));
                    }

                    if (!string.IsNullOrEmpty(SerialNumber) && SerialNumber != "undefined")
                    {
                        SerialNumberFilter = InQueryMaker(SerialNumber.ToLower());
                        FiltterDataList = FiltterDataList.Where(x => SerialNumberFilter.Contains(x.SerialNumber.ToLower()));
                    }

                    if (!string.IsNullOrEmpty(PartNumber) && PartNumber != "undefined")
                    {
                        PartNumberFilter = InQueryMaker(PartNumber.ToLower());
                        FiltterDataList = FiltterDataList.Where(x => PartNumberFilter.Contains(x.PartNumber.ToLower()));
                    }

                    if (!string.IsNullOrEmpty(Lot) && Lot != "undefined")
                    {
                        LotNumberFilter = InQueryMaker(Lot.ToLower());
                        FiltterDataList = FiltterDataList.Where(x => LotNumberFilter.Contains(x.Lot.ToLower()));
                    }

                    if (!string.IsNullOrEmpty(Status) && Status != "undefined" && Status != "ALL")
                    {
                        FiltterDataList = FiltterDataList.Where(x => Status.Equals(x.Status, StringComparison.OrdinalIgnoreCase));
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

        //GET:AssetsReport
        public async Task<ActionResult> ExportExcelAsset()
        {
            try
            {
                var ExcelExport = (List<mSMTProduct>)TempData["AssetList"];
                if (ExcelExport != null)
                {
                    var l = ExcelExport.Select(s =>
                   new
                   {
                       SerialNumber = s.SerialNumber,
                       PalletId = s.PalletId,
                       Quantity = s.Quantity,
                       s.DateAndTime,
                       s.StatusId,
                       s.Status,
                       s.CustomerCode,
                       s.ContainerId,
                       s.CustomerId,
                       s.Lot,
                       s.PartId,
                       s.PartNumber,
                       s.QADLine,
                       s.ShiftId,
                       s.Station,
                       s.Ble,
                       s.mZoneId,
                       s.ShelfName,
                       s.Remark,
                       s.Comment,
                       s.IsNormalReturn,
                       s.IsQualityReturn,
                       s.IsPutaway,
                       s.IsTakeaway,
                       s.IsAssembly,
                       s.IsMaster,
                       s.IsApprove,
                       s.LastSeenTime,
                       s.IsAction,
                       s.CreatedDate,
                       s.CreatedBy,
                       s.ModifiedDate,
                       s.ModifiedBy
                   }).ToList();

                    var grid = new GridView();
                    grid.DataSource = l;
                    grid.DataBind();
                    var FileName = "SMT-Report" + DateTime.Now.ToString("s") + "_.xls";
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

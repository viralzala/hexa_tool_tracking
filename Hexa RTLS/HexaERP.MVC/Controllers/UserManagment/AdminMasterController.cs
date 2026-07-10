using HexaERP.MVC.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace HexaERP.MVC.Controllers.UserManagment
{
    public class AdminMasterController : Controller
    {
        //Database Connection
        private ERPdbEntities db = new ERPdbEntities();

        public static string AppUserName, UniqueId;
        // GET: AdminMaster
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
                }
                // else { return RedirectToAction("Index", "AppUser"); }

            }
            catch (Exception)
            {
                return RedirectToAction("Index", "AppUser");
            }
            return View();
        }

        public JsonResult getCookie()
        {
            HttpCookie cookieObject = Request.Cookies["HexaCookie"];
            //--- Check for null           
            var AppUserName = "";
            if (cookieObject != null)
            {                //--- To read values from cookie collection we will use Keys used while creating cookie.                   
                AppUserName = cookieObject["AppUserName"];
            }
            return Json(new { LogedUser = AppUserName }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getallmodules()
        {
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                DateTime date = System.DateTime.Now;
                int orginf = Convert.ToInt32(Session["OrgInfoId"]);
                var listofmodules = (from rmodule in HexaErpobj.Rolemodules
                                     from modu in HexaErpobj.HexaModules
                                     where rmodule.moduleID == modu.moduleID &&
                                     rmodule.OrgInfoId == orginf && rmodule.IsAllowed == true
                                     //&& rmodule.Datefrom <= DateTime.Now.Date
                                     //&& rmodule.Dateto >= DateTime.Now.Date
                                     select new { RolemoduleId = rmodule.RolemoduleId, moduleName = modu.moduleName }).ToList();
                return Json(listofmodules, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getallwindows(int moduleid)
        {
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                int orginf = Convert.ToInt32(Session["OrgInfoId"]);
                var listofmenus = (from rmenu in HexaErpobj.RoleMenus
                                   from menu in HexaErpobj.AppMenus
                                   where rmenu.RolemoduleId == moduleid && rmenu.AppMenuId == menu.AppMenuId &&
                                   rmenu.OrgInfoId == orginf
                                   select new { RoleMenuId = rmenu.RoleMenuId, MenuName = menu.MenuName, MenuUrl = menu.MenuUrl, rmenu.RolemoduleId }).ToList();
                return Json(listofmenus, JsonRequestBehavior.AllowGet);
            }
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
where r.UID = 'Reader'";

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
            public int OrgInfoId { get; set; }
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
where IsTakeaway = 0 AND IsAssembly=0
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

        //
        [HttpGet]
        public async Task<JsonResult> GetCounts()
        {

            JsonResult result = new JsonResult();
            try
            {
                using (var context = new ERPdbEntities())
                {

                    var _shelf = @"select COUNT(*) As TotalShelf,
(select COUNT(*) from mShelf where IsAction = 0) AS AvailableShelf,
(select COUNT(*) from mShelf where IsAction = 1) AS AllocatedShelf
from mShelf";

                    var _shelfData = await context.Database.SqlQuery<ShelfModelView>(_shelf).FirstOrDefaultAsync();

                    var _smtProdcut = @"select COUNT(*) As TotalProduct,
(select COUNT(*) from mSMTProduct where IsPutaway = 1 AND CAST(CreatedDate as date) >= CAST(CreatedDate as date) AND CAST(CreatedDate as date) <= CAST(CreatedDate as date)) AS TotalPutaway,
(select COUNT(*) from mSMTProduct where IsTakeaway = 1 AND CAST(CreatedDate as date) >= CAST(CreatedDate as date) AND CAST(CreatedDate as date) <= CAST(CreatedDate as date)) AS TotalTakeaway,
(select COUNT(*) from mSMTProduct where IsAssembly = 1 AND CAST(CreatedDate as date) >= CAST(CreatedDate as date) AND CAST(CreatedDate as date) <= CAST(CreatedDate as date)) AS TotalAssembly,
(select COUNT(*) from mSMTProduct where IsMaster = 1 AND CAST(CreatedDate as date) >= CAST(CreatedDate as date) AND CAST(CreatedDate as date) <= CAST(CreatedDate as date)) AS TotalMaster
from mSMTProduct";
                    var _smtProcutData = await context.Database.SqlQuery<SmtProcutModelView>(_smtProdcut).FirstOrDefaultAsync();

                    //int orginf = Convert.ToInt32(Session["OrgInfoId"]);

                    //var locationCount = db.mSiteMasters.Where(w => w.IsAction == true && w.OrgInfoId == orginf).Count();
                    //var ZoneCount = db.mZones.Where(w => w.IsAction == true && w.OrgInfoId == orginf).Count();

                    //var FloorCount = db.mFloorMasters.Where(w => w.IsAction == true && w.OrgInfoId == orginf).Count();
                    //var RoomCount = db.mRoomMasters.Where(w => w.IsAction == true && w.OrgInfoId == orginf).Count();
                    //var orders = await db.mReaderSettups.AsNoTracking()
                    //          .GroupBy(x => new { x.ReaderNo, x.OrgInfoId })
                    //          .Where(x => x.Key.OrgInfoId == orginf)
                    //          .ToDictionaryAsync(g => g.Key, g => g.Count());
                    //var att = db.mReaderSettups.Where(w => w.IsAction == true && w.OrgInfoId == orginf).Count();
                    //AsNoTracking(x => x.IsAction == true)
                    //.ToDictionaryAsync(g => g.Key, g => g.Count());
                    result = Json(new { _shelfData, _smtProcutData }, JsonRequestBehavior.AllowGet);
                    result.MaxJsonLength = int.MaxValue;
                }
            }
            catch (Exception) { }
            return result;
        }

        [HttpGet]
        public async Task<JsonResult> getGetToTrackData()
        {
            int orginf = Convert.ToInt32(Session["OrgInfoId"]);
            // Initialization.    
            JsonResult result = new JsonResult();
            string source = "";
            //var path = System.IO.File.ReadAllText(Server.MapPath("~/Content/EmployeeTag.json"));
            var path = Server.MapPath("~/Content/EmployeeTag.json");
            using (StreamReader SourceReaderr = new StreamReader(path))
            {
                source = await SourceReaderr.ReadToEndAsync();
            }

            if (source != null)
            {
                List<tToolTrackDemo> tagsTrack = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<tToolTrackDemo>>(source));
                try
                {
                    var ObjDatass = (from tm in tagsTrack
                                     join emp in db.tEmployeeTags on tm.Epc equals emp.RFID
                                     join ag in db.mAgencies on emp.mAgencyId equals ag.mAgencyId
                                     join ds in db.mDesignations on emp.mDesignationId equals ds.mDesignationId
                                     join sk in db.mSkillCategories on emp.mSkillCategoryId equals sk.mSkillCategoryId
                                     join wk in db.mWorkCategories on emp.mWorkCategoryId equals wk.mWorkCategoryId
                                     join ac in db.mActivities on emp.mActivityId equals ac.mActivityId
                                     join rst in db.mReaderSettups on tm.Reader equals rst.ReaderNo
                                     join zon in db.mZones on rst.mZoneId equals zon.mZoneId
                                     join Szon in db.mFloorMasters on rst.mFloorMasterId equals Szon.mFloorMasterId
                                     where tm.PortId == rst.AttPortId && zon.IsAction == true && Szon.IsAction == true
                                     select new
                                     {
                                         Agency = ag.Agency ?? string.Empty,
                                         ag.mAgencyId,

                                         Designation = ds.Designation ?? string.Empty,
                                         ds.mDesignationId,

                                         SkillCategory = sk.SkillCategory ?? string.Empty,
                                         sk.mSkillCategoryId,

                                         WorkCategory = wk.WorkCategory ?? string.Empty,
                                         wk.mWorkCategoryId,

                                         Activity = ac.Activity ?? string.Empty,
                                         ac.mActivityId,

                                         Name = emp.EmployeeName ?? string.Empty,
                                         EmployeeId = emp.EmployeeId ?? string.Empty,

                                         tm.Epc,
                                         tm.tDate,
                                         rst.mFloorMasterId,
                                         Szon.FloorName,

                                         rst.mZoneId,
                                         zon.Zone,

                                         rst.mSiteMasterId
                                     }
                                 ).ToList();

                    var ComWise = ObjDatass.GroupBy(n => n.Zone)
                                .Select(n => new
                                {
                                    name = n.Key,
                                    y = n.Count()
                                }).OrderBy(n => n.name);

                    var AgenWise = ObjDatass.GroupBy(n => n.Agency)
                                .Select(n => new
                                {
                                    name = n.Key,
                                    y = n.Count()
                                }).OrderBy(n => n.name);

                    //var DeckWise = ObjDatass.GroupBy(n => n.FloorName)
                    //           .Select(n => new
                    //           {
                    //               name = n.Key,
                    //               y = n.Count()
                    //           }).OrderBy(n => n.name);

                    var WorkCatWise = ObjDatass.GroupBy(n => n.WorkCategory)
                            .Select(n => new
                            {
                                name = n.Key,
                                y = n.Count()
                            }).OrderBy(n => n.name);

                    var DesCatWise = ObjDatass.GroupBy(n => n.Designation)
                         .Select(n => new
                         {
                             name = n.Key,
                             y = n.Count()
                         }).OrderBy(n => n.name);

                    result = this.Json(new { ComWise, AgenWise, WorkCatWise, DesCatWise }, JsonRequestBehavior.AllowGet);

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
                    //_Flag = false; Message = message;
                }
                //Convert List Data to The Json Array             
                return result;
            }
            //var orders = await db.mReaderSettups.AsNoTracking()
            //          .GroupBy(x => new { x.ReaderNo, x.OrgInfoId })
            //          .Where(x => x.Key.OrgInfoId == orginf)
            //          .ToDictionaryAsync(g => g.Key, g => g.Count());
            //var att = db.mReaderSettups.Where(w => w.IsAction == true && w.OrgInfoId == orginf).Count();

            //return Json("", JsonRequestBehavior.AllowGet);
            return result;
        }

        [HttpGet]
        public JsonResult getHourTrend()
        {
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                List<spTrendsHour> objadd = new List<spTrendsHour>();
                spTrendsHour obj = new spTrendsHour();
                try
                {
                    int orginf = Convert.ToInt32(Session["OrgInfoId"]);
                    var Trend = db.Database.SqlQuery<spTrendsHour_Result>("spTrendsHour").ToList();
                    foreach (var it in Trend)
                    {
                        objadd.Add(new spTrendsHour
                        {
                            Attendance = it.Attendance,
                            XHour = Convert.ToDateTime(it.XHour),
                            YHour = it.XHour.Value.AddHours(1),
                        });
                    }
                }
                catch (Exception) { }

                var ReaderLog = db.tReaderLogs.Where(s => DbFunctions.TruncateTime(s.CreatedDate) == DbFunctions.TruncateTime(DateTime.Today)).OrderByDescending(e => e.CreatedDate).Take(6).ToList();
                var portLog = db.tPortChangeLogs.Where(s => DbFunctions.TruncateTime(s.CreatedDate) == DbFunctions.TruncateTime(DateTime.Today)).OrderByDescending(e => e.CreatedDate).Take(6).ToList();
                //
                var _CheckOuts = db.tAssetCheckOuts.Where(p => p.IssueDate < DateTime.Now)
                    .Where(p => (int)SqlFunctions.DateDiff("day", p.IssueDate, DateTime.Now) <= 60).OrderByDescending(e => e.IssueDate).ToList();

                var _IssueDate_ = _CheckOuts.GroupBy(n => n.IssueDate)
                              .Select(n => new
                              {
                                  IssueDate = ((DateTime)n.Key).ToShortDateString(),
                                  x = n.Count()
                              }).OrderBy(n => n.IssueDate).ToList();
                var _IssueDate = _IssueDate_.Select(x => new string[] { x.IssueDate, x.x.ToString() }).ToList();

                //
                var _AssetLists = db.Database.SqlQuery<AssetReportEntity>("spAssetReport {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}",
               new object[] { null, null, null, null, null, null, null, null, null, null, null }).ToList();

                var _AssetList_ = _AssetLists.GroupBy(n => n.IteamName)
                          .Select(n => new
                          {
                              IteamName = n.Key,
                              x = n.Count()
                          }).OrderBy(n => n.IteamName).ToList();
                var _AssetList = _AssetList_.Select(x => new string[] { x.IteamName, x.x.ToString() }).ToList();
                //
                var _AssetEmpLists = db.Database.SqlQuery<AssetWithEmployeeEntity>("spAssetWithEmployee {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}",
                new object[] { null, null, null, null, null, null, null, null, null, null, null, null }).ToList();
                var noOfdaysLeft = _AssetEmpLists.Select(x => new string[] { x.Model, x.NoOfDayLeft.ToString() }).ToList();

                //
                var _AssetDeprectList = db.Database.SqlQuery<AssetDepreciationEntity>("spAssetDepreciation {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}",
              new object[] { null, null, null, null, null, null, null, null, null, null, null }).ToList();
                var _AssetDep = _AssetDeprectList.Select(x => new string[] { x.Model, x.LeftMonth.ToString() }).ToList();
                //
                var _asst = db.tAssetTags.Where(x => x.IsAction == true).Count();
                var _emp = db.tEmployeeTags.Where(x => x.IsAction == true).Count();
                ArrayList assetemplCount = new ArrayList();
                string[] _assts = new string[] { "Asset", _asst.ToString() };
                string[] _emps = new string[] { "Employee", _emp.ToString() };
                assetemplCount.Add(_assts);
                assetemplCount.Add(_emps);

                return Json(new { objadd, ReaderLog, portLog, assetemplCount = assetemplCount.ToArray(), _IssueDate, _AssetList, _AssetEmpLists, noOfdaysLeft = noOfdaysLeft.ToArray(), _AssetDep = _AssetDep.ToArray() }, JsonRequestBehavior.AllowGet);
            }
        }


        public int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
        //
        [HttpGet]
        public ActionResult GetDashData()
        {
            ArrayList xValue = new ArrayList();
            ArrayList yValue = new ArrayList();

            int orginf = Convert.ToInt32(Session["OrgInfoId"]);

            var _asst = db.mSMTProducts.Where(x => x.IsAction == true).Count();
            var _emp = db.tEmployeeTags.Where(x => x.IsAction == true).Count();

            ArrayList _ast = new ArrayList { "SMT Product", _asst };
            ArrayList _empA = new ArrayList { "Employee", _emp };

            //AsNoTracking(x => x.IsAction == true)
            //.ToDictionaryAsync(g => g.Key, g => g.Count());
            return Json(new { _ast, _empA }, JsonRequestBehavior.AllowGet);
        }

        public partial class spTrendsHour
        {
            public Nullable<System.DateTime> XHour { get; set; }
            public Nullable<System.DateTime> YHour { get; set; }
            public Nullable<int> Attendance { get; set; }
        }
    }
}
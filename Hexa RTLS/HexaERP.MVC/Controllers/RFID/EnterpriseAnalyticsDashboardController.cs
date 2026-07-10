using HexaERP.MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class EnterpriseAnalyticsDashboardController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();

        // GET: EnterpriseAnalyticsDashboard
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

                if (Session["UniqueId"].ToString() != "" && Session["OrgInfoId"].ToString() != "" && Session["AppUserName"].ToString() != "")
                {
                    //string Page_Name = Path.GetFileName(Request.Path);
                    if (!new string[] { "AD", "SA" }.Contains(Convert.ToString(Session["SortCode"])))
                    {
                        return RedirectToAction("Index", "AppUser");
                    }
                }
                else
                {
                    return RedirectToAction("Index", "AppUser");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "AppUser");
            }

            ViewBag.UserName = Session["AppUserName"].ToString();
            ViewBag.OrgName = "Hexa ERP";
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);

            var org = db.OrgInfoes.FirstOrDefault(o => o.OrgInfoId == orgId);

            ViewBag.Organization = org != null ? org.OrgInfoName : "Hexa ERP";
            return View();
        }

        [HttpGet]
        public JsonResult GetDashboardData(int? page = 1, int? pageSize = 10, string search = "", string sortBy = "CreatedDate", bool sortDesc = true)
        {
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                var now = DateTime.Now;
                var today = DateTime.Today;

                // ===== KPI Counts (All Dynamic) =====
                var totalAssets = db.tAssetTags.Count(x => x.OrgInfoId == orgId && x.IsAction == true);
                var assetsIssued = db.tAssetTags.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.tEmployeeTagId != null);
                var assetsAvailable = totalAssets - assetsIssued;
                var activeAssets = totalAssets; // All active assets

                // Calibration Due - count pending calibrations
                var calibrationDue = db.tAssetCalibrations.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.NextDueDate != null && x.NextDueDate >= today);
                var calibrationOverdue = db.tAssetCalibrations.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.NextDueDate != null && x.NextDueDate < today);

                // Inspection Due
                var inspectionDue = db.tAssetInspections.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.InspectionDate >= today);
                var inspectionOverdue = db.tAssetInspections.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.InspectionDate < today);

                // Maintenance Due
                var maintenanceDue = db.tMaintenances.Count(x => x.OrgInfoId == orgId && x.IsAction == true && (x.EndDate >= today || x.EndDate == null));
                var maintenanceOverdue = db.tMaintenances.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.EndDate.HasValue && x.EndDate.Value < today);

                // Expired Assets (Phase Out Date passed)
                var expiredAssets = db.tAssetTags.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.PhaseOutDate.HasValue && x.PhaseOutDate.Value < today);

                // Due Today
                var dueToday = db.tAssetCalibrations.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.NextDueDate.HasValue && DbFunctions.TruncateTime(x.NextDueDate.Value) == DbFunctions.TruncateTime(today)) +
                                db.tAssetInspections.Count(x => x.OrgInfoId == orgId && x.IsAction == true && DbFunctions.TruncateTime(x.InspectionDate) == DbFunctions.TruncateTime(today)) +
                                db.tMaintenances.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.EndDate.HasValue && DbFunctions.TruncateTime(x.EndDate.Value) == DbFunctions.TruncateTime(today));

                // ===== Recent Assets =====
                var latestAssetsRaw = db.tAssetTags
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true)
                    .OrderByDescending(x => x.CreatedDate)
                    .Take(10)
                    .Select(x => new
                    {
                        x.IteamName,
                        x.ModelNo,
                        x.SerialNo,
                        x.RFID,
                        x.BarCode,
                        x.OwnerDepartment,
                        x.PlantName,
                        x.BuildingName,
                        x.CreatedDate,
                        x.CreatedBy
                    })
                    .ToList();
                var latestAssets = latestAssetsRaw.Select(x => new
                {
                    x.IteamName,
                    x.ModelNo,
                    x.SerialNo,
                    x.RFID,
                    x.BarCode,
                    x.OwnerDepartment,
                    x.PlantName,
                    x.BuildingName,
                    CreatedDateDisplay = x.CreatedDate.HasValue ? x.CreatedDate.Value.ToString("yyyy-MM-dd") : "",
                    CreatedBy = x.CreatedBy ?? ""
                }).ToList();

                // ===== Latest Calibrations =====
                var latestCalibrationsRaw = db.tAssetCalibrations
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true)
                    .OrderByDescending(x => x.CreatedDate)
                    .Take(10)
                    .Select(x => new
                    {
                        x.CertificateNo,
                        x.AssetName,
                        x.CalibrationDate,
                        x.Result,
                        x.CreatedBy
                    })
                    .ToList();
                var latestCalibrations = latestCalibrationsRaw.Select(x => new
                {
                    x.CertificateNo,
                    x.AssetName,
                    CalibrationDateDisplay = x.CalibrationDate.ToString("yyyy-MM-dd"),
                    x.Result,
                    x.CreatedBy
                }).ToList();

                // ===== Latest Inspections =====
                var latestInspectionsRaw = db.tAssetInspections
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true)
                    .OrderByDescending(x => x.InspectionDate)
                    .Take(10)
                    .Select(x => new
                    {
                        x.InspectionNo,
                        x.AssetName,
                        x.InspectionDate,
                        x.Status,
                        x.Inspector,
                        x.CreatedBy
                    })
                    .ToList();
                var latestInspections = latestInspectionsRaw.Select(x => new
                {
                    x.InspectionNo,
                    x.AssetName,
                    InspectionDateDisplay = x.InspectionDate.ToString("yyyy-MM-dd"),
                    x.Status,
                    x.Inspector,
                    x.CreatedBy
                }).ToList();

                // ===== Latest Maintenance =====
                var latestMaintenanceRaw = db.tMaintenances
                     .Where(x => x.OrgInfoId == orgId && x.IsAction == true)
                     .OrderByDescending(x => x.CreatedDate)
                     .Take(10)
                     .Select(x => new
                     {
                         x.Title,
                         x.Cost,
                         x.Maintby_,
                         x.StartDate,
                         x.EndDate,
                         x.CreatedDate,
                         x.CreatedBy
                     })
                     .ToList();
                var latestMaintenance = latestMaintenanceRaw.Select(x => new
                {
                    Title = x.Title,
                    Cost = x.Cost ?? 0,
                    MaintainedBy = x.Maintby_,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    CreatedDateDisplay = x.CreatedDate.HasValue
                        ? x.CreatedDate.Value.ToString("yyyy-MM-dd")
                        : "",
                    CreatedBy = x.CreatedBy ?? ""
                }).ToList();

                // ===== Upcoming Alerts =====
                var upcomingCalibrationsRaw = db.tAssetCalibrations
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.NextDueDate != null && x.NextDueDate > today)
                    .OrderBy(x => x.NextDueDate)
                    .Take(10)
                    .Select(x => new
                    {
                        x.CertificateNo,
                        x.AssetName,
                        x.NextDueDate,
                        x.Remarks
                    })
                    .ToList();
                var upcomingCalibrations = upcomingCalibrationsRaw.Select(x => new
                {
                    x.CertificateNo,
                    x.AssetName,
                    DueDateDisplay = x.NextDueDate.HasValue ? x.NextDueDate.Value.ToString("yyyy-MM-dd") : "",
                    x.Remarks
                }).ToList();

                var upcomingInspectionsRaw = db.tAssetInspections
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.InspectionDate > today)
                    .OrderBy(x => x.InspectionDate)
                    .Take(10)
                    .Select(x => new
                    {
                        x.InspectionNo,
                        x.AssetName,
                        x.InspectionDate,
                        x.Status
                    })
                    .ToList();
                var upcomingInspections = upcomingInspectionsRaw.Select(x => new
                {
                    x.InspectionNo,
                    x.AssetName,
                    InspectionDateDisplay = x.InspectionDate.ToString("yyyy-MM-dd"),
                    x.Status
                }).ToList();

                var upcomingMaintenanceRaw = db.tMaintenances
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.EndDate != null && x.EndDate > today)
                    .OrderBy(x => x.EndDate)
                    .Take(10)
                    .Select(x => new
                    {
                        x.Title,
                        x.EndDate,
                        x.Maintby_,
                        x.Cost
                    })
                    .ToList();
                var upcomingMaintenance = upcomingMaintenanceRaw.Select(x => new
                {
                    Title = x.Title,
                    DueDateDisplay = x.EndDate.HasValue
                        ? x.EndDate.Value.ToString("yyyy-MM-dd")
                        : "",
                    MaintainedBy = x.Maintby_,
                    Cost = x.Cost ?? 0
                }).ToList();

                // ===== Overdue Items =====
                var overdueCalibrationsRaw = db.tAssetCalibrations
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.NextDueDate != null && x.NextDueDate < today)
                    .OrderByDescending(x => x.NextDueDate)
                    .Take(10)
                    .Select(x => new
                    {
                        x.CertificateNo,
                        x.AssetName,
                        x.NextDueDate
                    })
                    .ToList();
                var overdueCalibrations = overdueCalibrationsRaw.Select(x => new
                {
                    x.CertificateNo,
                    x.AssetName,
                    DueDateDisplay = x.NextDueDate.Value.ToString("yyyy-MM-dd"),
                    DaysOverdue = (today - x.NextDueDate.Value).Days
                }).ToList();

                var overdueInspectionsRaw = db.tAssetInspections
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.InspectionDate < today)
                    .OrderByDescending(x => x.InspectionDate)
                    .Take(10)
                    .Select(x => new
                    {
                        x.InspectionNo,
                        x.AssetName,
                        x.InspectionDate
                    })
                    .ToList();
                var overdueInspections = overdueInspectionsRaw.Select(x => new
                {
                    x.InspectionNo,
                    x.AssetName,
                    InspectionDateDisplay = x.InspectionDate.ToString("yyyy-MM-dd"),
                    DaysOverdue = (today - x.InspectionDate).Days
                }).ToList();

                // ===== Monthly Trends (12 months) =====
                var monthLabels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                var assetByMonth = new List<int>();
                var calByMonth = new List<int>();
                var inspByMonth = new List<int>();
                var maintByMonth = new List<int>();

                for (int i = 1; i <= 12; i++)
                {
                    assetByMonth.Add(db.tAssetTags.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.CreatedDate.HasValue && x.CreatedDate.Value.Year == now.Year && x.CreatedDate.Value.Month == i));
                    calByMonth.Add(db.tAssetCalibrations.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.CalibrationDate.Year == now.Year && x.CalibrationDate.Month == i));
                    inspByMonth.Add(db.tAssetInspections.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.InspectionDate.Year == now.Year && x.InspectionDate.Month == i));
                    maintByMonth.Add(db.tMaintenances.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.CreatedDate.HasValue && x.CreatedDate.Value.Year == now.Year && x.CreatedDate.Value.Month == i));
                }

                // ===== Weekly Trends (last 7 days) =====
                var weekDays = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
                var assetByWeek = new List<int>();
                var calByWeek = new List<int>();

                // Fetch all assets and calibrations first, then filter in memory to avoid DayOfWeek LINQ to Entities issue
                var assetsForWeek = db.tAssetTags.Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.CreatedDate.HasValue).ToList();
                var calsForWeek = db.tAssetCalibrations.Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.CreatedDate.HasValue).ToList();

                for (int i = 0; i < 7; i++)
                {
                    assetByWeek.Add(assetsForWeek.Count(x => x.CreatedDate.Value.DayOfWeek == (DayOfWeek)i));
                    calByWeek.Add(calsForWeek.Count(x => x.CreatedDate.Value.DayOfWeek == (DayOfWeek)i));
                }

                // ===== Yearly Trends =====
                var years = db.tAssetTags
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.CreatedDate != null)
                    .Select(x => x.CreatedDate.Value.Year)
                    .Distinct()
                    .OrderBy(y => y)
                    .ToList();

                if (!years.Any()) years = new List<int> { now.Year };
                var yearlyAssets = years.Select(y => db.tAssetTags.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.CreatedDate.HasValue && x.CreatedDate.Value.Year == y)).ToList();

                // ===== By Department =====
                var assetsByDepartment = db.tAssetTags
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && !string.IsNullOrEmpty(x.OwnerDepartment))
                    .GroupBy(x => x.OwnerDepartment)
                    .Select(g => new { label = g.Key, value = g.Count() })
                    .ToList();

                // ===== By Status =====
                var assetStatus = db.tAssetTags
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.mStatusMasterId != null)
                    .GroupBy(x => x.mStatusMasterId)
                    .Select(g => new { label = "Status " + g.Key, value = g.Count() })
                    .ToList();

                // ===== Maintenance by Type =====
                var maintByTypeLabels = db.mMaintenanceTypes
                    .Where(x => x.OrgInfoId == orgId)
                    .Select(x => x.MaintenanceName)
                    .ToList();
                
                if (!maintByTypeLabels.Any()) maintByTypeLabels = new List<string> { "Preventive", "Corrective", "Emergency", "Scheduled" };
                var maintByType = maintByTypeLabels.Select((t, index) => 
                    db.tMaintenances.Count(x => x.OrgInfoId == orgId && x.IsAction == true && (x.mMaintenanceTypeId == index + 1 || (index == maintByTypeLabels.Count - 1 && maintByTypeLabels.Count > 4)))).ToList();

                // ===== Recent Transactions (with pagination) =====
                var transactionsRaw = db.tAssetCheckOuts
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true);
                
                if (!string.IsNullOrEmpty(search))
                {
                    transactionsRaw = transactionsRaw.Where(x => x.CreatedBy.Contains(search));
                }

                if (sortDesc)
                {
                    transactionsRaw = transactionsRaw.OrderByDescending(x => x.CreatedDate);
                }
                else
                {
                    transactionsRaw = transactionsRaw.OrderBy(x => x.CreatedDate);
                }

                var totalTxns = transactionsRaw.Count();
                var transactionsData = transactionsRaw.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value)
                    .Select(x => new
                    {
                        x.tAssetCheckOutId,
                        x.IssueDate,
                        x.ReturnDate,
                        x.CreatedBy
                    })
                    .ToList();
                var transactions = transactionsData.Select(x => new
                {
                    x.tAssetCheckOutId,
                    IssueDateDisplay = x.IssueDate.HasValue ? x.IssueDate.Value.ToString("yyyy-MM-dd") : "",
                    ReturnDateDisplay = x.ReturnDate.HasValue ? x.ReturnDate.Value.ToString("yyyy-MM-dd") : "",
                    x.CreatedBy
                }).ToList();

                // ===== Due Today Items =====
                var dueTodayCalibrations = db.tAssetCalibrations
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.NextDueDate.HasValue && DbFunctions.TruncateTime(x.NextDueDate.Value) == DbFunctions.TruncateTime(today))
                    .Select(x => new { Type = "Calibration", Name = x.AssetName, DateRaw = x.NextDueDate })
                    .ToList();
                var dueTodayInspections = db.tAssetInspections
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && DbFunctions.TruncateTime(x.InspectionDate) == DbFunctions.TruncateTime(today))
                    .Select(x => new { Type = "Inspection", Name = x.AssetName, DateRaw = (DateTime?)x.InspectionDate })
                    .ToList();
                var dueTodayMaintenances = db.tMaintenances
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.EndDate.HasValue && DbFunctions.TruncateTime(x.EndDate.Value) == DbFunctions.TruncateTime(today))
                    .Select(x => new { Type = "Maintenance", Name = x.Title, DateRaw = x.EndDate })
                    .ToList();
                
                var dueTodayItemsRaw = dueTodayCalibrations
                    .Union(dueTodayInspections)
                    .Union(dueTodayMaintenances)
                    .Take(10)
                    .ToList();
                var dueTodayItems = dueTodayItemsRaw.Select(x => new
                {
                    x.Type,
                    x.Name,
                    Date = x.DateRaw.HasValue ? x.DateRaw.Value.ToString("yyyy-MM-dd") : ""
                }).ToList();

                // ===== Recent Login Users =====
                var recentLoginsRaw = db.AppUsers
                    .Where(x => x.OrgInfoId == orgId)
                    .OrderByDescending(x => x.AppUserId)
                    .Take(10)
                    .Select(x => new
                    {
                        x.AppUserName,
                        EMail = x.EMail,
                        x.CreatedDate
                    })
                    .ToList();
                var recentLogins = recentLoginsRaw.Select(x => new
                {
                    x.AppUserName,
                    Email = x.EMail,
                    LastLogin = x.CreatedDate.HasValue ? x.CreatedDate.Value.ToString("yyyy-MM-dd") : ""
                }).ToList();

                // ===== Assets by Site =====
                var assetsBySite = db.tAssetTags
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.mSiteMasterId != null)
                    .GroupBy(x => x.mSiteMasterId)
                    .Select(g => new { label = "Site " + g.Key, value = g.Count() })
                    .ToList();

                // ===== Assets by Zone =====
                var assetsByZone = db.tAssetTags
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.mZoneId != null)
                    .GroupBy(x => x.mZoneId)
                    .Select(g => new { label = "Zone " + g.Key, value = g.Count() })
                    .ToList();

                // ===== Cost Distribution =====
                var costDistributionRaw = db.tAssetTags
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.PurchaseCost.HasValue)
                    .GroupBy(x => x.mGroupMasterId)
                    .Select(g => new { Key = g.Key, Value = g.Sum(a => a.PurchaseCost ?? 0) })
                    .ToList();
                var costDistribution = costDistributionRaw.Select(x => new { label = "Group " + (x.Key.HasValue ? x.Key.ToString() : "Unknown"), value = x.Value }).ToList();

                return Json(new
                {
                    success = true,
                    // KPI
                    totalAssets,
                    activeAssets,
                    calibrationDue,
                    calibrationOverdue,
                    inspectionDue,
                    inspectionOverdue,
                    maintenanceDue,
                    maintenanceOverdue,
                    assetsIssued,
                    assetsAvailable,
                    expiredAssets,
                    dueToday,

                    // Recent
                    latestAssets,
                    latestCalibrations,
                    latestInspections,
                    latestMaintenance,

                    // Alerts
                    upcomingCalibrations,
                    upcomingInspections,
                    upcomingMaintenance,

                    // Overdue
                    overdueCalibrations,
                    overdueInspections,

                    // Trends
                    monthLabels,
                    assetByMonth,
                    calByMonth,
                    inspByMonth,
                    maintByMonth,
                    weekDays,
                    assetByWeek,
                    calByWeek,
                    years,
                    yearlyAssets,

                    // Distributions
                    assetsByDepartment,
                    assetStatus,
                    maintByTypeLabels,
                    maintByType,
                    assetsBySite,
                    assetsByZone,
                    costDistribution,

                    // Transactions
                    transactions,
                    totalTxns,
                    page,
                    pageSize,

                    // Due Today
                    dueTodayItems,

                    // Recent Logins
                    recentLogins
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    totalAssets = 0, activeAssets = 0, calibrationDue = 0, calibrationOverdue = 0,
                    inspectionDue = 0, inspectionOverdue = 0, maintenanceDue = 0, maintenanceOverdue = 0,
                    assetsIssued = 0, assetsAvailable = 0, expiredAssets = 0, dueToday = 0,
                    latestAssets = new object[0], latestCalibrations = new object[0],
                    latestInspections = new object[0], latestMaintenance = new object[0],
                    upcomingCalibrations = new object[0], upcomingInspections = new object[0], upcomingMaintenance = new object[0],
                    overdueCalibrations = new object[0], overdueInspections = new object[0],
                    monthLabels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" },
                    assetByMonth = new int[12], calByMonth = new int[12], inspByMonth = new int[12], maintByMonth = new int[12],
                    weekDays = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" },
                    assetByWeek = new int[7], calByWeek = new int[7],
                    years = new[] { DateTime.Now.Year }, yearlyAssets = new[] { 0 },
                    assetsByDepartment = new object[0], assetStatus = new object[0],
                    maintByTypeLabels = new[] { "Preventive", "Corrective", "Emergency", "Scheduled" }, maintByType = new int[4],
                    assetsBySite = new object[0], assetsByZone = new object[0], costDistribution = new object[0],
                    transactions = new object[0], totalTxns = 0, page = 1, pageSize = 10,
                    dueTodayItems = new object[0], recentLogins = new object[0]
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult ExportExcel(string type = "transactions")
        {
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                
                switch (type.ToLower())
                {
                    case "transactions":
                        var txns = db.tAssetCheckOuts.Where(x => x.OrgInfoId == orgId && x.IsAction == true)
                            .Select(x => new { x.IssueDate, x.ReturnDate, x.CreatedBy })
                            .ToList();
                        return Json(new { success = true, data = txns }, JsonRequestBehavior.AllowGet);
                    
                    case "assets":
                        var assets = db.tAssetTags.Where(x => x.OrgInfoId == orgId && x.IsAction == true)
                            .Select(x => new { x.IteamName, x.ModelNo, x.SerialNo})
                            .ToList();
                        return Json(new { success = true, data = assets }, JsonRequestBehavior.AllowGet);
                    
                    default:
                        return Json(new { success = false, error = "Invalid export type" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
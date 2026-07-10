using HexaERP.MVC.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class NewDashboardController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();

        // GET: NewDashboard
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

                if (Session["UniqueId"] != null && Session["OrgInfoId"] != null && Session["AppUserName"] != null)
                {
                    if (new string[] { "AD", "SA" }.Contains(Convert.ToString(Session["SortCode"])))
                    {
                        ViewBag.UserName = Session["AppUserName"].ToString();
                        ViewBag.OrgName = "Hexa ERP";
                        return View();
                    }
                    return RedirectToAction("Index", "AppUser");
                }
                return RedirectToAction("Index", "AppUser");
            }
            catch
            {
                return RedirectToAction("Index", "AppUser");
            }
        }

        [HttpGet]
        public JsonResult GetDashboardData()
        {
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);

                // ===== KPI Counts =====
                // tAssetTags has IsAction, OrgInfoId
                var totalAssets = db.tAssetTags.Count(x => x.OrgInfoId == orgId && x.IsAction == true);
                var assetsIssued = db.tAssetTags.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.tEmployeeTagId != null);
                var assetsAvailable = totalAssets - assetsIssued;

                // AssetCalibration - NO IsAction field, uses OrgInfoId
                var totalCalibrations = db.AssetCalibrations.Count(x => x.OrgInfoId == orgId);

                // tAssetInspections has IsAction, OrgInfoId
                var totalInspections = db.tAssetInspections.Count(x => x.OrgInfoId == orgId && x.IsAction == true);

                // tMaintenances has IsAction, OrgInfoId
                var totalMaintenance = db.tMaintenances.Count(x => x.OrgInfoId == orgId && x.IsAction == true);

                // ===== Recent Activities =====
                var latestAssets = db.tAssetTags
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true)
                    .OrderByDescending(x => x.CreatedDate)
                    .Take(5)
                    .Select(x => new { x.IteamName, x.ModelNo, x.SerialNo, x.CreatedBy, CreatedDate = x.CreatedDate ?? DateTime.Now })
                    .ToList();

                // AssetCalibration uses CreatedAt (DateTime) not CreatedDate
                var latestCalibrations = db.AssetCalibrations
                    .Where(x => x.OrgInfoId == orgId)
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(5)
                    .Select(x => new { x.CalibrationDate, x.CertificateNo, x.Remarks, x.CreatedBy })
                    .ToList();

                var latestInspections = db.tAssetInspections
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true)
                    .OrderByDescending(x => x.InspectionDate)
                    .Take(5)
                    .Select(x => new { x.InspectionNo, InspectionDate = (DateTime?)x.InspectionDate, x.Inspector, x.Status, x.CreatedBy })
                    .ToList();

                var latestMaintenance = db.tMaintenances
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true)
                    .OrderByDescending(x => x.CreatedDate)
                    .Take(5)
                    .Select(x => new { x.Title, Cost = x.Cost ?? 0, x.CreatedBy, x.CreatedDate })
                    .ToList();

                // ===== Upcoming Alerts =====
                var upcomingCalibrations = db.AssetCalibrations
                    .Where(x => x.OrgInfoId == orgId && x.NextDueDate != null)
                    .OrderBy(x => x.NextDueDate)
                    .Take(5)
                    .Select(x => new { x.CertificateNo, DueDate = x.NextDueDate ?? DateTime.Now, x.Remarks })
                    .ToList();

                var upcomingInspections = db.tAssetInspections
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true)
                    .OrderBy(x => x.InspectionDate)
                    .Take(5)
                    .Select(x => new { x.InspectionNo, InspectionDate = (DateTime?)x.InspectionDate, x.Status })
                    .ToList();

                // ===== Chart Data =====
                var assetsByDepartment = db.tAssetTags
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.OwnerDepartment != null)
                    .GroupBy(x => x.OwnerDepartment)
                    .Select(g => new { Department = g.Key, Count = g.Count() })
                    .ToList();

                var monthlyCalibrations = db.AssetCalibrations
                    .Where(x => x.OrgInfoId == orgId && x.CalibrationDate != null)
                    .GroupBy(x => new { Year = x.CalibrationDate.Value.Year, Month = x.CalibrationDate.Value.Month })
                    .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToList();

                var maintenanceByType = db.tMaintenances
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true)
                    .GroupBy(x => x.mMaintenanceTypeId)
                    .Select(g => new { TypeId = g.Key, Count = g.Count() })
                    .ToList();

                var assetStatus = db.tAssetTags
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true)
                    .GroupBy(x => x.mStatusMasterId)
                    .Select(g => new { StatusId = g.Key, Count = g.Count() })
                    .ToList();

                // ===== Recent Transactions =====
                var recentTransactions = db.tAssetCheckOuts
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true)
                    .OrderByDescending(x => x.CreatedDate)
                    .Take(10)
                    .Select(x => new { x.IssueDate, x.ReturnDate, x.CreatedBy, TransactionType = "Issue/Return" })
                    .ToList();

                return Json(new
                {
                    // KPI
                    TotalAssets = totalAssets,
                    ActiveAssets = totalAssets,
                    CalibrationDue = totalCalibrations,
                    InspectionDue = totalInspections,
                    MaintenanceDue = totalMaintenance,
                    AssetsIssued = assetsIssued,
                    AssetsAvailable = assetsAvailable,
                    ExpiredAssets = 0,

                    // Recent
                    LatestAssets = latestAssets,
                    LatestCalibrations = latestCalibrations,
                    LatestInspections = latestInspections,
                    LatestMaintenance = latestMaintenance,

                    // Alerts
                    UpcomingCalibrations = upcomingCalibrations,
                    UpcomingInspections = upcomingInspections,

                    // Charts
                    AssetsByDepartment = assetsByDepartment,
                    MonthlyCalibrations = monthlyCalibrations,
                    MaintenanceByType = maintenanceByType,
                    AssetStatus = assetStatus,

                    // Transactions
                    RecentTransactions = recentTransactions
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    TotalAssets = 0, ActiveAssets = 0, CalibrationDue = 0, InspectionDue = 0,
                    MaintenanceDue = 0, AssetsIssued = 0, AssetsAvailable = 0, ExpiredAssets = 0,
                    LatestAssets = new object[0], LatestCalibrations = new object[0],
                    LatestInspections = new object[0], LatestMaintenance = new object[0],
                    UpcomingCalibrations = new object[0], UpcomingInspections = new object[0],
                    AssetsByDepartment = new object[0], MonthlyCalibrations = new object[0],
                    MaintenanceByType = new object[0], AssetStatus = new object[0],
                    RecentTransactions = new object[0],
                    Error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
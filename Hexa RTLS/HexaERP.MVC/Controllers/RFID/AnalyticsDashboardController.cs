using HexaERP.MVC.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class AnalyticsDashboardController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();

        public ActionResult Index()
        {
            try
            {
                HttpCookie cookieObject = Request.Cookies["HexaCookie"];
                if (cookieObject != null) { ViewBag.LogedIn = cookieObject["AppUserName"]; }
                else { return RedirectToAction("Index", "AppUser"); }

                if (Session["UniqueId"] != null && Session["OrgInfoId"] != null && Session["AppUserName"] != null)
                {
                    if (new string[] { "AD", "SA" }.Contains(Convert.ToString(Session["SortCode"])))
                    {
                        ViewBag.UserName = Session["AppUserName"];
                        ViewBag.OrgName = "Hexa ERP";
                        return View();
                    }
                    return RedirectToAction("Index", "AppUser");
                }
                return RedirectToAction("Index", "AppUser");
            }
            catch { return RedirectToAction("Index", "AppUser"); }
        }

        [HttpGet]
        public JsonResult GetDashboardData()
        {
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                var now = DateTime.Now;

                // ===== KPI =====
                var totalAssets = db.tAssetTags.Count(x => x.OrgInfoId == orgId && x.IsAction == true);
                var issued = db.tAssetTags.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.tEmployeeTagId != null);
                var available = totalAssets - issued;
                var calibrations = db.AssetCalibrations.Count(x => x.OrgInfoId == orgId);
                var inspections = db.tAssetInspections.Count(x => x.OrgInfoId == orgId && x.IsAction == true);
                var maintenances = db.tMaintenances.Count(x => x.OrgInfoId == orgId && x.IsAction == true);

                // ===== Monthly Trends (12 months) =====
                var monthLabels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                var assetByMonth = monthLabels.Select((m, i) =>
                    db.tAssetTags.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.CreatedDate.HasValue && x.CreatedDate.Value.Year == now.Year && x.CreatedDate.Value.Month == i + 1)).ToArray();

                var calByMonth = monthLabels.Select((m, i) =>
                    db.AssetCalibrations.Count(x => x.OrgInfoId == orgId && x.CalibrationDate.HasValue && x.CalibrationDate.Value.Year == now.Year && x.CalibrationDate.Value.Month == i + 1)).ToArray();

                var inspByMonth = monthLabels.Select((m, i) =>
                    db.tAssetInspections.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.InspectionDate.Year == now.Year && x.InspectionDate.Month == i + 1)).ToArray();

                var maintByMonth = monthLabels.Select((m, i) =>
                    db.tMaintenances.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.CreatedDate.HasValue && x.CreatedDate.Value.Year == now.Year && x.CreatedDate.Value.Month == i + 1)).ToArray();

                // ===== Weekly Trends (last 7 days) =====
                var weekDays = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
                var assetByWeek = weekDays.Select((d, i) =>
                    db.tAssetTags.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.CreatedDate.HasValue && x.CreatedDate.Value.DayOfWeek == (DayOfWeek)i)).ToArray();

                // ===== Yearly =====
                var years = db.tAssetTags.Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.CreatedDate != null)
                    .Select(x => x.CreatedDate.Value.Year).Distinct().OrderBy(y => y).ToList();
                if (!years.Any()) years = new System.Collections.Generic.List<int> { now.Year };
                var yearlyAssets = years.Select(y => db.tAssetTags.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.CreatedDate.HasValue && x.CreatedDate.Value.Year == y)).ToArray();

                // ===== By Department =====
                var deptData = db.tAssetTags.Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.OwnerDepartment != null)
                    .GroupBy(x => x.OwnerDepartment).Select(g => new { label = g.Key, value = g.Count() }).ToList();

                // ===== Status Distribution =====
                var statusData = db.tAssetTags.Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.mStatusMasterId != null)
                    .GroupBy(x => x.mStatusMasterId).Select(g => new { label = "Status " + g.Key, value = g.Count() }).ToList();

                // ===== Recent =====
                var latestAssets = db.tAssetTags.Where(x => x.OrgInfoId == orgId && x.IsAction == true)
                    .OrderByDescending(x => x.CreatedDate).Take(10)
                    .Select(x => new { x.IteamName, x.ModelNo, x.SerialNo, x.CreatedBy, CreatedDate = x.CreatedDate ?? now }).ToList();
                var latestCal = db.AssetCalibrations.Where(x => x.OrgInfoId == orgId)
                    .OrderByDescending(x => x.CreatedAt).Take(10)
                    .Select(x => new { x.CertificateNo, x.CalibrationDate, x.CreatedBy }).ToList();
                var latestInsp = db.tAssetInspections.Where(x => x.OrgInfoId == orgId && x.IsAction == true)
                    .OrderByDescending(x => x.InspectionDate).Take(10)
                    .Select(x => new { x.InspectionNo, x.InspectionDate, x.Status, x.CreatedBy }).ToList();
                var latestMaint = db.tMaintenances.Where(x => x.OrgInfoId == orgId && x.IsAction == true)
                    .OrderByDescending(x => x.CreatedDate).Take(10)
                    .Select(x => new { x.Title, x.Cost, x.CreatedBy }).ToList();

                // ===== Transactions =====
                var transactions = db.tAssetCheckOuts.Where(x => x.OrgInfoId == orgId && x.IsAction == true)
                    .OrderByDescending(x => x.CreatedDate).Take(20)
                    .Select(x => new { x.IssueDate, x.ReturnDate, x.CreatedBy }).ToList();

                // ===== Alerts =====
                var upcomingCal = db.AssetCalibrations.Where(x => x.OrgInfoId == orgId && x.NextDueDate != null)
                    .OrderBy(x => x.NextDueDate).Take(10)
                    .Select(x => new { x.CertificateNo, DueDate = x.NextDueDate ?? now }).ToList();

                // ===== Maintenance by Type =====
                var maintByTypeLabels = new[] { "Preventive", "Corrective", "Emergency", "Scheduled" };
                var maintByType = maintByTypeLabels.Select(t => db.tMaintenances.Count(x => x.OrgInfoId == orgId && x.IsAction == true)).ToArray();

                return Json(new
                {
                    // KPI
                    totalAssets, issued, available, calibrations, inspections, maintenances,
                    // Trends
                    monthLabels, assetByMonth, calByMonth, inspByMonth, maintByMonth,
                    weekDays, assetByWeek,
                    years, yearlyAssets,
                    // Distributions
                    deptData, statusData,
                    maintByTypeLabels, maintByType,
                    // Recent
                    latestAssets, latestCal, latestInsp, latestMaint,
                    transactions, upcomingCal,
                    now = now.ToString("yyyy-MM-dd HH:mm:ss")
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
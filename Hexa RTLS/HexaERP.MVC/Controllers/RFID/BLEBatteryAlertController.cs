using HexaERP.MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class BLEBatteryAlertController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();

        // GET: BLEBatteryAlert
        public ActionResult Index()
        {
            try
            {
                // Get cookie Collection.
                HttpCookie cookieObject = Request.Cookies["HexaCookie"];
                // Check for null 
                if (cookieObject != null)
                {
                    ViewBag.LogedIn = cookieObject["AppUserName"];
                }
                else { return RedirectToAction("Index", "AppUser"); }

                if (Session["UniqueId"] != null &&
                     Session["OrgInfoId"] != null &&
                     Session["AppUserName"] != null)
                {
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

            return View();
        }

        // GET: BLEBatteryAlert/GetDashboard
        [HttpGet]
        public JsonResult GetDashboard()
        {
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);

                // Get all BLE tags from tAssetTag that have RFID (BLE ID)
                var assetTags = db.tAssetTags
                    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && !string.IsNullOrEmpty(x.RFID))
                    .Select(x => x.RFID)
                    .Distinct()
                    .ToList();

                int total = assetTags.Count;

                // Get only the latest battery record for each BLE Tag (EPC)
                var latestMonitorData = db.toMonitors
                    .Where(x => assetTags.Contains(x.Epc) && !string.IsNullOrEmpty(x.BatteryLevel))
                    .GroupBy(x => x.Epc)
                    .Select(g => g.OrderByDescending(x => x.tDate).ThenByDescending(x => x.Id).FirstOrDefault())
                    .Where(x => x != null)
                    .ToList();

                int healthy = 0, medium = 0, low = 0;

                foreach (var item in latestMonitorData)
                {
                    if (int.TryParse(item.BatteryLevel.Replace("%", ""), out int batteryLevel))
                    {
                        if (batteryLevel >= 80)
                        {
                            healthy++;
                        }
                        else if (batteryLevel >= 40 && batteryLevel <= 79)
                        {
                            medium++;
                        }
                        else if (batteryLevel < 40)
                        {
                            low++;
                        }
                    }
                }

                return Json(new { Total = total, Healthy = healthy, Medium = medium, Low = low }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Total = 0, Healthy = 0, Medium = 0, Low = 0, Error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: BLEBatteryAlert/GetData
        [HttpGet]
        public JsonResult GetData(string bleId = "", string assetName = "")
        {
            try
            {
                // TEMPORARY LOGGING FOR DEBUGGING
                System.Diagnostics.Debug.WriteLine("=== GetData() Debug Log ===");
                
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                System.Diagnostics.Debug.WriteLine("OrgId: " + orgId);

                // STEP 1: Match working SQL query exactly - Get all asset tags with RFID
                var assetQuery = db.tAssetTags
                    .Where(x => x.OrgInfoId == orgId && 
                                x.IsAction == true && 
                                !string.IsNullOrEmpty(x.RFID));

                // Apply filters if provided
                if (!string.IsNullOrEmpty(bleId))
                {
                    assetQuery = assetQuery.Where(x => x.RFID.Contains(bleId));
                }

                if (!string.IsNullOrEmpty(assetName))
                {
                    assetQuery = assetQuery.Where(x => x.IteamName.Contains(assetName));
                }

                var assetTags = assetQuery
                    .Select(x => new { x.RFID, x.IteamName })
                    .Distinct()
                    .ToList();

                System.Diagnostics.Debug.WriteLine("Asset Tags Count: " + assetTags.Count);
                if (assetTags.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("First 3 Asset Tags:");
                    for (int i = 0; i < Math.Min(3, assetTags.Count); i++)
                    {
                        System.Diagnostics.Debug.WriteLine("  " + assetTags[i].RFID + " - " + assetTags[i].IteamName);
                    }
                }

                // STEP 2: Get all monitor data - matching SQL INNER JOIN
                var assetRfids = assetTags.Select(a => a.RFID).ToList();
                System.Diagnostics.Debug.WriteLine("Asset RFID List Count: " + assetRfids.Count);

                var allMonitorData = db.toMonitors
                    .Where(x => assetRfids.Contains(x.Epc) && !string.IsNullOrEmpty(x.BatteryLevel))
                    .ToList();

                System.Diagnostics.Debug.WriteLine("All Monitor Data Count: " + allMonitorData.Count);
                if (allMonitorData.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("First 3 Monitor Records:");
                    for (int i = 0; i < Math.Min(3, allMonitorData.Count); i++)
                    {
                        System.Diagnostics.Debug.WriteLine("  " + allMonitorData[i].Epc + " - " + allMonitorData[i].BatteryLevel + " - " + allMonitorData[i].tDate + " - " + allMonitorData[i].Id);
                    }
                }

                // STEP 3: Get only the latest battery record for each BLE Tag (EPC) using in-memory grouping
                var latestMonitorData = allMonitorData
                    .GroupBy(x => x.Epc)
                    .Select(g => g.OrderByDescending(x => x.tDate).ThenByDescending(x => x.Id).FirstOrDefault())
                    .Where(x => x != null)
                    .ToList();

                System.Diagnostics.Debug.WriteLine("Latest Monitor Data Count (after grouping): " + latestMonitorData.Count);
                if (latestMonitorData.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("First 3 Latest Monitor Records:");
                    for (int i = 0; i < Math.Min(3, latestMonitorData.Count); i++)
                    {
                        System.Diagnostics.Debug.WriteLine("  " + latestMonitorData[i].Epc + " - " + latestMonitorData[i].BatteryLevel + " - " + latestMonitorData[i].tDate + " - " + latestMonitorData[i].Id);
                    }
                }

                // STEP 4: Join and create result (INNER JOIN like SQL)
                var result = (from asset in assetTags
                              join monitor in latestMonitorData on asset.RFID equals monitor.Epc
                              select new
                              {
                                  BLEId = asset.RFID,
                                  AssetName = asset.IteamName,
                                  BatteryLevel = monitor.BatteryLevel
                              }).ToList();

                System.Diagnostics.Debug.WriteLine("Final Result Count: " + result.Count);
                if (result.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("First 3 Results:");
                    for (int i = 0; i < Math.Min(3, result.Count); i++)
                    {
                        System.Diagnostics.Debug.WriteLine("  " + result[i].BLEId + " - " + result[i].AssetName + " - " + result[i].BatteryLevel);
                    }
                }
                System.Diagnostics.Debug.WriteLine("=== End Debug Log ===");

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR in GetData(): " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack Trace: " + ex.StackTrace);
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }
    }
}
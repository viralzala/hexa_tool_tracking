using HexaERP.MVC.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;
using System.Web;
using System.Data.Entity.Validation;

namespace HexaERP.MVC.Controllers.RFID
{
    public class AssetInspectionController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();

        // GET: AssetInspection
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

        [HttpPost]
        public ActionResult Create(tAssetTag obj)
        {
            JsonResult result = new JsonResult();
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);

                if (string.IsNullOrEmpty(obj.RFID) && string.IsNullOrEmpty(obj.BarCode) && string.IsNullOrEmpty(obj.UID))
                {
                    return Json(new
                    {
                        Flag = false,
                        Message = "Please fill the filter parameter"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    GetAssetInfoEnitity _AssetList = db.Database.SqlQuery<GetAssetInfoEnitity>("spGetAssetInfo {0}, {1}, {2}, {3}, {4}, {5}",
                    new object[] { obj.RFID, obj.BarCode, obj.IteamName, obj.Model, obj.ModelNo, obj.UID }).ToList().FirstOrDefault();

                    if (_AssetList != null)
                    {
                        // Get the mIteamMasterId from tAssetTag
                        var assetTag = db.tAssetTags.Find(_AssetList.tAssetTagId);
                        int mIteamMasterId = assetTag != null ? assetTag.mIteamMasterId ?? 0 : 0;
                        
                        var InspData = (from ins in db.tAssetInspections
                                        where ((ins.OrgInfoId == orgId && ins.IsAction == true) && ins.AssetId == mIteamMasterId)
                                        select new
                                        {
                                            ins.AssetInspectionId,
                                            ins.InspectionNo,
                                            ins.InspectionDate,
                                            ins.Inspector,
                                            ins.PhysicalCondition,
                                            ins.SafetyLabels,
                                            ins.FitForUse,
                                            ins.Observation,
                                            ins.Status,
                                            ins.CreatedBy
                                        }).ToList();

                        result = this.Json(new
                        {
                            Flag = true,
                            Message = "Record Found!!",
                            _AssetList,
                            InspData
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        result = this.Json(new
                        {
                            Flag = false,
                            Message = "Record Not Found"
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (DbEntityValidationException ex)
            {
                var Message = string.Empty;
                foreach (var item in ex.EntityValidationErrors)
                {
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;
                    foreach (var subItem in item.ValidationErrors)
                    {
                        Message = string.Format("Error '{0}' occurred in {1} at {2}",
                                 subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                    }
                }
                result = this.Json(new { Message, Flag = false }, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        [HttpPost]
        public ActionResult CreateInspection(tAssetInspection obj)
        {
            JsonResult result = new JsonResult();
            try
            {
                var UserName = Session["AppUserName"];
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);

                if (obj.AssetId <= 0)
                {
                    return Json(new { Flag = false, Message = "AssetId Missing" });
                }

                if (string.IsNullOrWhiteSpace(obj.InspectionNo))
                {
                    return Json(new { Flag = false, Message = "InspectionNo Missing" });
                }

                if (obj.InspectionDate == null)
                {
                    return Json(new { Flag = false, Message = "InspectionDate Missing" });
                }
                else
                {
                    if (!string.IsNullOrEmpty(obj.InspectionDate.ToString()))
                    {
                        obj.InspectionDate = Convert.ToDateTime(obj.InspectionDate);
                    }
                    obj.IsAction = true;
                    obj.OrgInfoId = orgId;
                    obj.CreatedBy = UserName.ToString();
                    obj.CreatedDate = DateTime.Now;

                    db.tAssetInspections.Add(obj);
                    System.Diagnostics.Debug.WriteLine("AssetId : " + obj.AssetId);
                    System.Diagnostics.Debug.WriteLine("----------------");
                    System.Diagnostics.Debug.WriteLine("AssetId : " + obj.AssetId);
                    System.Diagnostics.Debug.WriteLine("InspectionNo : " + obj.InspectionNo);
                    System.Diagnostics.Debug.WriteLine("InspectionDate : " + obj.InspectionDate);
                    System.Diagnostics.Debug.WriteLine("----------------");
                    db.SaveChanges();

                    result = this.Json(new { Message = "Successfully Added!!", Flag = true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var Message = string.Empty;
                foreach (var item in ex.EntityValidationErrors)
                {
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;
                    foreach (var subItem in item.ValidationErrors)
                    {
                        Message = string.Format("Error '{0}' occurred in {1} at {2}",
                                 subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                    }
                }
                result = this.Json(new { Message, Flag = false }, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        [HttpGet]
        public ActionResult InitData()
        {
            JsonResult result = new JsonResult();
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);

                var AssetList = db.mIteamMasters.Where(x => x.OrgInfoId == orgId && x.IsAction == true)
                    .Select(x => new { x.mIteamMasterId, x.IteamName }).ToList();

                result = this.Json(new
                {
                    Flag = true,
                    Message = "Record Not Found",
                    AssetList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                var Message = string.Empty;
                foreach (var item in ex.EntityValidationErrors)
                {
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;
                    foreach (var subItem in item.ValidationErrors)
                    {
                        Message = string.Format("Error '{0}' occurred in {1} at {2}",
                                 subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                    }
                }
                result = this.Json(new { Message, Flag = false }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }

        [HttpGet]
        public JsonResult GetStatistics()
        {
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                
                var totalInspections = db.tAssetInspections.Count(x => x.OrgInfoId == orgId && x.IsAction == true);
                
                // Passed: inspections where Status indicates passed/success
                var passedInspections = db.tAssetInspections.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.Status == "Passed");
                
                // Pending: inspections where Status indicates pending/in progress
                var pendingInspections = db.tAssetInspections.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.Status == "Pending");
                
                // Failed: inspections where Status indicates failed
                var failedInspections = db.tAssetInspections.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.Status == "Failed");

                return Json(new { Total = totalInspections, Passed = passedInspections, Pending = pendingInspections, Failed = failedInspections }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Total = 0, Passed = 0, Pending = 0, Failed = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetData()
        {
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);

                var data = (from ins in db.tAssetInspections
                            join itm in db.mIteamMasters on ins.AssetId equals itm.mIteamMasterId
                            where ins.OrgInfoId == orgId && ins.IsAction == true
                            orderby ins.AssetInspectionId descending
                            select new
                            {
                                ins.AssetInspectionId,
                                AssetName = itm.IteamName,
                                ins.InspectionNo,
                                ins.InspectionDate,
                                ins.Inspector,
                                ins.PhysicalCondition,
                                ins.SafetyLabels,
                                ins.FitForUse,
                                ins.Observation,
                                ins.Status,
                                ins.CreatedBy
                            }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult Edit(int id)
        {
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);

                var data = (from ins in db.tAssetInspections
                            join itm in db.mIteamMasters on ins.AssetId equals itm.mIteamMasterId
                            where ins.AssetInspectionId == id && ins.OrgInfoId == orgId && ins.IsAction == true
                            select new
                            {
                                ins.AssetInspectionId,
                                ins.AssetId,
                                AssetName = itm.IteamName,
                                ins.InspectionNo,
                                ins.InspectionDate,
                                ins.Inspector,
                                ins.PhysicalCondition,
                                ins.SafetyLabels,
                                ins.FitForUse,
                                ins.Observation,
                                ins.Status,
                                ins.CreatedBy
                            }).FirstOrDefault();

                if (data != null)
                {
                    return Json(new { Flag = true, Idata = data }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Flag = false, Message = "Record not found" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult DeleteData(int ID)
        {
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                var rec = db.tAssetInspections.FirstOrDefault(x => x.AssetInspectionId == ID && x.OrgInfoId == orgId);

                if (rec != null)
                {
                    rec.IsAction = false;
                    rec.ModifiedDate = DateTime.Now;
                    rec.ModifiedBy = Convert.ToString(Session["AppUserName"]);
                    db.SaveChanges();

                    return Json(new { Flag = true, Message = "Inspection record deleted successfully." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Flag = false, Message = "Record not found." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}

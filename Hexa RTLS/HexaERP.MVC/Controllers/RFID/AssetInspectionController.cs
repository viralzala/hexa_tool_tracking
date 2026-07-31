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
                    GetAssetInfoEnitity _AssetList = (from tag in db.tAssetTags
                        join vendor in db.mVendors on tag.mVendorId equals vendor.mVendorId into vendorJoin
                        from vendor in vendorJoin.DefaultIfEmpty()
                        join groupMaster in db.mGroupMasters on tag.mGroupMasterId equals groupMaster.mGroupMasterId into groupJoin
                        from groupMaster in groupJoin.DefaultIfEmpty()
                        join iteamType in db.mIteamTypeMasters on tag.mIteamTypeMasterId equals iteamType.mIteamTypeMasterId into typeJoin
                        from iteamType in typeJoin.DefaultIfEmpty()
                        join unit in db.mUnitMasters on tag.mUnitMasterId equals unit.mUnitMasterId into unitJoin
                        from unit in unitJoin.DefaultIfEmpty()
                        join status in db.mStatusMasters on tag.mStatusMasterId equals status.mStatusMasterId into statusJoin
                        from status in statusJoin.DefaultIfEmpty()
                        join empTag in db.tEmployeeTags on tag.tEmployeeTagId equals empTag.tEmployeeTagId into empJoin
                        from empTag in empJoin.DefaultIfEmpty()
                        join site in db.mSiteMasters on tag.mSiteMasterId equals site.mSiteMasterId into siteJoin
                        from site in siteJoin.DefaultIfEmpty()
                        join zone in db.mZones on tag.mZoneId equals zone.mZoneId into zoneJoin
                        from zone in zoneJoin.DefaultIfEmpty()
                        join floor in db.mFloorMasters on tag.mFloorMasterId equals floor.mFloorMasterId into floorJoin
                        from floor in floorJoin.DefaultIfEmpty()
                        join room in db.mRoomMasters on tag.mRoomMasterId equals room.mRoomMasterId into roomJoin
                        from room in roomJoin.DefaultIfEmpty()
                        where (!string.IsNullOrEmpty(obj.RFID) && tag.RFID == obj.RFID)
                           || (!string.IsNullOrEmpty(obj.BarCode) && tag.BarCode == obj.BarCode)
                           || (!string.IsNullOrEmpty(obj.IteamName) && tag.IteamName.Contains(obj.IteamName))
                           || (!string.IsNullOrEmpty(obj.Model) && tag.Model.Contains(obj.Model))
                           || (!string.IsNullOrEmpty(obj.ModelNo) && tag.ModelNo.Contains(obj.ModelNo))
                           || (!string.IsNullOrEmpty(obj.UID) && tag.UID == obj.UID)
                        select new GetAssetInfoEnitity
                        {
                            tAssetTagId = tag.tAssetTagId,
                            IteamName = tag.IteamName,
                            Model = tag.Model,
                            ModelNo = tag.ModelNo,
                            SerialNo = tag.SerialNo,
                            Manufacturer = tag.Manufacturer,
                            BarCode = tag.BarCode,
                            RFID = tag.RFID,
                            PurchaseCost = tag.PurchaseCost,
                            CreatedDate = tag.CreatedDate,
                            InvNo = tag.InvNo,
                            Depreciation = tag.Depreciation,
                            Receivedby = tag.Receivedby,
                            bStock = tag.bStock,
                            DefaultWarranty = tag.DefaultWarranty,
                            IteamDescription = tag.IteamDescription,
                            VendorName = vendor.VendorName,
                            GroupName = groupMaster.GroupName,
                            IteamType = iteamType.IteamType,
                            UnitName = unit.UnitName,
                            StatusName = status.StatusName,
                            EmployeeName = empTag.EmployeeName,
                            EmployeeId = empTag.EmployeeId,
                            ContactNo = empTag.ContactNo,
                            EmailId = empTag.EmailId,
                            tEmployeeTagId = empTag.tEmployeeTagId,
                            IssueDate = tag.IssueDate,
                            ReturnDate = tag.ReturnDate,
                            Site = site.Site,
                            Zone = zone.Zone,
                            FloorName = floor.FloorName,
                            RoomName = room.RoomName,
                            img = tag.img
                        }).FirstOrDefault();

                    if (_AssetList != null)
                    {
                        _AssetList.EngDays = _AssetList.IssueDate.HasValue && _AssetList.ReturnDate.HasValue
                            ? (int?)(_AssetList.ReturnDate.Value - _AssetList.IssueDate.Value).Days
                            : null;

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

using HexaERP.MVC.EmailConfig;
using HexaERP.MVC.Models;
using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class MoreAssetDetailController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();

        // GET: MoreAssetDetail
        public ActionResult Index()
        {

            try
            {
                //--- Get cookie Collection.
                HttpCookie cookieObject = Request.Cookies["HexaCookie"];
                //--- Check for null 
                if (cookieObject != null)
                {
                    // Check if CarryParam exists before accessing it (null-safe)
                    var carryParam = TempData["CarryParam"] as string;
                    if (!string.IsNullOrEmpty(carryParam))
                    {
                        TempData["_CarryParam"] = carryParam;
                    }

                    ViewBag.LogedIn = cookieObject["AppUserName"];
                }
                else { return RedirectToAction("Index", "AppUser"); }

                if (Session["UniqueId"].ToString() != "" && Session["OrgInfoId"].ToString() != "" && Session["AppUserName"].ToString() != "")
                {
                    //string Page_Name = Path.GetFileName(Request.Path);
                    var sortCode = Convert.ToString(Session["SortCode"]);

                    if (sortCode != "AD" && sortCode != "SA")
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

        [HttpGet]
        public JsonResult GetStatistics()
        {
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                var today = DateTime.Today;

                var totalMaintenance = db.tMaintenances.Count(x => x.OrgInfoId == orgId && x.IsAction == true);
                
                // Completed: maintenance records where EndDate is in the past (work completed)
                var completedMaintenance = db.tMaintenances.Count(x => x.OrgInfoId == orgId && x.IsAction == true && 
                                                                      x.EndDate.HasValue && x.EndDate.Value < today);
                
                // In Progress: maintenance records where EndDate is null or in the future (still ongoing)
                var inProgressMaintenance = db.tMaintenances.Count(x => x.OrgInfoId == orgId && x.IsAction == true && 
                                                                       (!x.EndDate.HasValue || x.EndDate.Value >= today));
                
                // Overdue: maintenance records that have passed their EndDate and are not completed
                // (EndDate < today indicates the maintenance period is over and should have been completed)
                var overdueMaintenance = db.tMaintenances.Count(x => x.OrgInfoId == orgId && x.IsAction == true && 
                                                                     x.EndDate.HasValue && x.EndDate.Value < today);

                return Json(new { Total = totalMaintenance, Completed = completedMaintenance, InProgress = inProgressMaintenance, Overdue = overdueMaintenance }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Total = 0, Completed = 0, InProgress = 0, Overdue = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: MoreAssetDetail/Create
        [HttpPost]
        public JsonResult CreateSatatus(tAssetTag obj)
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                if (string.IsNullOrEmpty(obj.tAssetTagId.ToString()) || string.IsNullOrEmpty(obj.mStatusMasterId.ToString()))
                {
                    return Json(new
                    {
                        Flag = false,
                        Message = "Please select status"
                    }, JsonRequestBehavior.AllowGet);
                }
                else if (obj.tAssetTagId == 0 || obj.mStatusMasterId == 0)
                {
                    return Json(new
                    {
                        Flag = false,
                        Message = "Please Search Asset"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tAssetTag _Atags = db.tAssetTags.Find(obj.tAssetTagId);
                    _Atags.mStatusMasterId = obj.mStatusMasterId;
                    _Atags.OrgInfoId = orgId;
                    _Atags.CreatedBy = UserName.ToString();
                    _Atags.CreatedDate = DateTime.Now;
                    //db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();
                    result = this.Json(new { Message = "Successfully Updated!!", Flag = true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var Message = string.Empty;
                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    // Get entry
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;
                    // Display or log error messages
                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        Message = string.Format("Error '{0}' occurred in {1} at {2}",
                                 subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                    }
                }
                result = this.Json(new { Message, Flag = false }, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        // POST: MoreAssetDetail/CreateMaint
        [HttpPost]
        public ActionResult CreateMaint(tMaintenance obj)
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                if (string.IsNullOrEmpty(obj.mMaintenanceTypeId.ToString()) && string.IsNullOrEmpty(obj.Title))
                {
                    return Json(new
                    {
                        Flag = false,
                        Message = "Please fill the required parameter"
                    }, JsonRequestBehavior.AllowGet);
                }
                else if (string.IsNullOrEmpty(obj.tAssetTagId.ToString()))
                {
                    return Json(new
                    {
                        Flag = false,
                        Message = "Please select Asset for adding maintainance detail"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (!string.IsNullOrEmpty(obj.StartDate.ToString()))
                    {
                        DateTime sDate = Convert.ToDateTime(obj.StartDate);
                        obj.StartDate = sDate;
                    }
                    if (!string.IsNullOrEmpty(obj.EndDate.ToString()))
                    {
                        DateTime EDate = Convert.ToDateTime(obj.EndDate);
                        obj.EndDate = EDate;
                    }
                    if (!string.IsNullOrEmpty(obj.IsWarranty.ToString()))
                    {
                        obj.IsWarranty = true;
                    }
                    obj.IsAction = true;
                    obj.IsAction = true; obj.OrgInfoId = orgId; obj.CreatedBy = UserName.ToString();
                    obj.CreatedDate = DateTime.Now;
                    db.tMaintenances.Add(obj);
                    db.SaveChanges();
                    result = this.Json(new { Message = "Successfully Added!!", Flag = true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var Message = string.Empty;
                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    // Get entry
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;
                    // Display or log error messages
                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        Message = string.Format("Error '{0}' occurred in {1} at {2}",
                                 subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                    }
                }
                result = this.Json(new { Message, Flag = false }, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        // POST: MoreAssetDetail/Create
        [HttpPost]
        public ActionResult Create(tAssetTag obj)
        {
            // Initialization.
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

                        var MaintData = (from main in db.tMaintenances
                                         join mainty in db.mMaintenanceTypes on main.mMaintenanceTypeId equals mainty.mMaintenanceTypeId
                                         where ((main.OrgInfoId == orgId && main.IsAction == true) && main.tAssetTagId == _AssetList.tAssetTagId)
                                         select new
                                         {
                                             main.tMaintenanceId,
                                             main.Title,
                                             main.MaintenanPart,
                                             main.Cost,
                                             main.Note,
                                             main.IsWarranty,
                                             main.AdditionalPart,
                                             main.CreatedBy,
                                             main.StartDate,
                                             main.EndDate,
                                             main.Maintby_,
                                             mainty.MaintenanceName
                                         }).ToList();

                        var inoutHitor = (from aco in db.tAssetCheckOuts
                                          join emp in db.tEmployeeTags on aco.tEmployeeTagId equals emp.tEmployeeTagId
                                          join tas in db.tAssetTags on aco.tAssetTagId equals tas.tAssetTagId
                                          where ((aco.OrgInfoId == orgId && aco.IsAction == true) && aco.tAssetTagId == _AssetList.tAssetTagId)
                                          select new
                                          {
                                              aco.IssueDate,
                                              aco.ReturnDate,
                                              aco.CreatedBy,
                                              emp.EmployeeName,
                                              emp.EmployeeId,
                                              tas.IteamName,
                                              tas.Model,
                                              tas.ModelNo

                                          }).ToList();



                        result = this.Json(new
                        {
                            Flag = true,
                            Message = "Record Found!!",
                            _AssetList,
                            MaintData,
                            inoutHitor
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

                    //TempData["Asset"] = null;
                    //TempData["Asset"] = _LogList;
                }
            }
            catch (DbEntityValidationException ex)
            {
                var Message = string.Empty;
                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    // Get entry
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;
                    // Display or log error messages
                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        Message = string.Format("Error '{0}' occurred in {1} at {2}",
                                 subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                    }
                }
                result = this.Json(new { Message, Flag = false }, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        // POST: MoreAssetDetail/InitData
        [HttpGet]
        public ActionResult InitData()
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                var MaintType = db.mMaintenanceTypes.Where(x => x.OrgInfoId == orgId && x.IsAction == true).ToList();
                var statusType = db.mStatusMasters.Where(x => x.OrgInfoId == orgId && x.IsAction == true).ToList();

                var EmpList = db.tEmployeeTags.Where(x => x.OrgInfoId == orgId && x.IsAction == true).Select(x => new { x.tEmployeeTagId, x.EmployeeName }).ToList();

                result = this.Json(new
                {
                    Flag = true,
                    Message = "Record Not Found",
                    MaintType,
                    statusType,
                    EmpList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                var Message = string.Empty;
                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    // Get entry
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;
                    // Display or log error messages
                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        Message = string.Format("Error '{0}' occurred in {1} at {2}",
                                 subItem.ErrorMessage, entityTypeName, subItem.PropertyName);

                    }
                }
                result = this.Json(new { Message, Flag = false }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }

        // POST: MoreAssetDetail/CheckOutAsset
        [HttpPost]
        public ActionResult CheckOutAsset(tAssetTag obj)
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);

                if (string.IsNullOrEmpty(obj.tAssetTagId.ToString()) || string.IsNullOrEmpty(obj.tEmployeeTagId.ToString()))
                {
                    return Json(new
                    {
                        Flag = false,
                        Message = "Please Enter Employee and Issue Detail"
                    }, JsonRequestBehavior.AllowGet);
                }
                else if (obj.tAssetTagId == 0)
                {
                    return Json(new
                    {
                        Flag = false,
                        Message = "Please Enter Employee and Issue Detail"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tAssetCheckOut ckinout = new tAssetCheckOut();
                    tAssetTag _Atags = db.tAssetTags.Find(obj.tAssetTagId);
                    _Atags.tEmployeeTagId = obj.tEmployeeTagId;
                    ckinout.tEmployeeTagId = obj.tEmployeeTagId;
                    if (!string.IsNullOrEmpty(obj.IssueDate.ToString()))
                    {
                        DateTime sDate = Convert.ToDateTime(obj.IssueDate);
                        _Atags.IssueDate = sDate;
                        ckinout.IssueDate = sDate;
                    }
                    if (!string.IsNullOrEmpty(obj.ReturnDate.ToString()))
                    {
                        DateTime EDate = Convert.ToDateTime(obj.ReturnDate);
                        _Atags.ReturnDate = EDate;
                        ckinout.ReturnDate = EDate;
                    }
                    _Atags.OrgInfoId = orgId;
                    _Atags.CreatedBy = UserName.ToString();
                    _Atags.CreatedDate = DateTime.Now;
                    ckinout.tAssetTagId = obj.tAssetTagId;
                    ckinout.IsAction = true;
                    ckinout.OrgInfoId = orgId;
                    ckinout.CreatedBy = UserName.ToString();
                    ckinout.CreatedDate = DateTime.Now;
                    //db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();
                    db.tAssetCheckOuts.Add(ckinout);
                    db.SaveChanges();

                    //
                    tEmployeeTag _Temp = db.tEmployeeTags.Find(obj.tEmployeeTagId);
                    NotifyMail.TrasactionNotify(_Atags.IteamName, _Atags.ModelNo, _Atags.SerialNo, _Temp.EmployeeName, _Temp.EmployeeId, UserName.ToString(), _Atags.IssueDate, _Atags.ReturnDate, "Asset Issued To Employee");

                    result = this.Json(new { Message = "Successfully Issued!!", Flag = true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var Message = string.Empty;
                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    // Get entry
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;
                    // Display or log error messages
                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        Message = string.Format("Error '{0}' occurred in {1} at {2}",
                                 subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                    }
                }
                result = this.Json(new { Message, Flag = false }, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        // POST: MoreAssetDetail/CheckIn
        [HttpGet]
        public ActionResult CheckIn(int eId, int aId)
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);

                if (string.IsNullOrEmpty(eId.ToString()) || string.IsNullOrEmpty(aId.ToString()))
                {
                    return Json(new
                    {
                        Flag = false,
                        Message = "Something went wrong try again"
                    }, JsonRequestBehavior.AllowGet);
                }
                else if (eId == 0 || aId == 0)
                {
                    return Json(new
                    {
                        Flag = false,
                        Message = "Something went wrong try again"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {


                    //tAssetCheckOut ckinout = new tAssetCheckOut();
                    tAssetTag _Atags = db.tAssetTags.Find(aId);

                    if (_Atags.tEmployeeTagId == eId)
                    {

                        DateTime? _tempIssued;

                        _tempIssued = _Atags.IssueDate;

                        _Atags.tEmployeeTagId = null;
                        _Atags.IssueDate = null;
                        _Atags.ReturnDate = DateTime.Now;

                        _Atags.OrgInfoId = orgId;
                        _Atags.ModifiedBy = UserName.ToString();
                        _Atags.ModifiedDate = DateTime.Now;


                        db.SaveChanges();
                        tAssetCheckOut ckinout = db.tAssetCheckOuts.Where(x => x.tAssetTagId == aId && x.tEmployeeTagId == eId).FirstOrDefault();
                        ckinout.ReturnDate = DateTime.Now;
                        db.SaveChanges();

                        tEmployeeTag _Temp = db.tEmployeeTags.Find(eId);
                        NotifyMail.TrasactionNotify(_Atags.IteamName, _Atags.ModelNo, _Atags.SerialNo, _Temp.EmployeeName, _Temp.EmployeeId, UserName.ToString(), _tempIssued, DateTime.Now, "Asset Returned From Employee");
                        result = this.Json(new { Message = "Successfully Checking Asset!!", Flag = true }, JsonRequestBehavior.AllowGet);
                    }
                    else { result = this.Json(new { Message = "No such employee found belongs to asset", Flag = false }, JsonRequestBehavior.AllowGet); }
                }
            }
            catch (DbEntityValidationException ex)
            {
                var Message = string.Empty;
                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    // Get entry
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;
                    // Display or log error messages
                    foreach (DbValidationError subItem in item.ValidationErrors)
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
        public JsonResult GetData()
        {
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);

                var data = (from main in db.tMaintenances
                            join tag in db.tAssetTags on main.tAssetTagId equals tag.tAssetTagId
                            join mty in db.mMaintenanceTypes on main.mMaintenanceTypeId equals mty.mMaintenanceTypeId
                            where main.OrgInfoId == orgId && main.IsAction == true
                            orderby main.tMaintenanceId descending
                            select new
                            {
                                main.tMaintenanceId,
                                AssetName = tag.IteamName,
                                main.Title,
                                MaintenanceName = mty.MaintenanceName,
                                main.MaintenanPart,
                                main.Cost,
                                main.IsWarranty,
                                main.AdditionalPart,
                                main.CreatedBy,
                                main.StartDate,
                                main.EndDate,
                                main.Note
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

                var data = (from main in db.tMaintenances
                            join tag in db.tAssetTags on main.tAssetTagId equals tag.tAssetTagId
                            join mty in db.mMaintenanceTypes on main.mMaintenanceTypeId equals mty.mMaintenanceTypeId
                            where main.tMaintenanceId == id && main.OrgInfoId == orgId && main.IsAction == true
                            select new
                            {
                                main.tMaintenanceId,
                                main.tAssetTagId,
                                AssetName = tag.IteamName,
                                main.Title,
                                main.mMaintenanceTypeId,
                                MaintenanceName = mty.MaintenanceName,
                                main.MaintenanPart,
                                main.Cost,
                                main.IsWarranty,
                                main.AdditionalPart,
                                main.CreatedBy,
                                main.StartDate,
                                main.EndDate,
                                main.Note
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
                var rec = db.tMaintenances.FirstOrDefault(x => x.tMaintenanceId == ID && x.OrgInfoId == orgId);

                if (rec != null)
                {
                    rec.IsAction = false;
                    rec.ModifiedDate = DateTime.Now;
                    rec.ModifiedBy = Convert.ToString(Session["AppUserName"]);
                    db.SaveChanges();

                    return Json(new { Flag = true, Message = "Maintenance record deleted successfully." }, JsonRequestBehavior.AllowGet);
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

        //
        [HttpPost]
        public JsonResult uploadImg(tAssetTag _obj, HttpPostedFileBase file)
        {

            if (file != null)
            {

                try
                {
                    var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                    if (string.IsNullOrEmpty(_obj.tAssetTagId.ToString()))
                    {
                        return Json(new { result = false, message = "Something went wrong refresh page" });

                    }
                    else if (_obj.tAssetTagId == 0)
                    {
                        return Json(new { result = false, message = "Something went wrong refresh page" });


                    }
                    else
                    {
                        tAssetTag obj = db.tAssetTags.Find(_obj.tAssetTagId);

                        //string Uid = DateTime.Now.ToString().GetHashCode().ToString("x");

                        string appPath = Request.PhysicalApplicationPath;
                        //string fullPath = appPath + obj.UID;

                        string extension = Path.GetExtension(file.FileName);
                        string fileName = obj.UID + extension;
                        string path = Path.Combine(Server.MapPath("Files/AssetsImg"), fileName);
                        string _path = "Files/AssetsImg/" + fileName;
                        obj.img = _path;
                        file.SaveAs(appPath + _path);

                        obj.OrgInfoId = orgId;
                        obj.ModifiedBy = UserName.ToString();
                        obj.ModifiedDate = DateTime.Now;
                        db.SaveChanges();
                        //file is uploaded

                        // save the image path path to the database or you can send image 
                        // directly to database
                        // in-case if you want to store byte[] ie. for DB
                        //using (MemoryStream ms = new MemoryStream())
                        //{
                        //    file.InputStream.CopyTo(ms);
                        //    byte[] array = ms.GetBuffer();
                        //}
                        return Json(new { result = true, message = "Document uploaded successfully" });
                        //ModelState.AddModelError(String.Empty, "Map uploaded successfully.");
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { result = false, message = ex.InnerException.Message });
                }
            }
            else
            {
                return Json(new { result = false, message = "Please select Image to upload." });
            }
        }
    }
}
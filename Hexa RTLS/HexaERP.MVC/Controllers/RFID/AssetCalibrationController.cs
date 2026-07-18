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
    public class AssetCalibrationController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();

        // GET: AssetCalibration
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
                        var CalibData = (from cal in db.tAssetCalibrations
                                         join itm in db.mIteamMasters on cal.AssetId equals itm.mIteamMasterId
                                         where ((cal.OrgInfoId == orgId && cal.IsAction == true) && cal.AssetId == _AssetList.tAssetTagId)
                                         select new
                                         {
                                             cal.AssetCalibrationId,
                                             cal.CalibrationDate,
                                             cal.NextDueDate,
                                             cal.CertificateNo,
                                             cal.Remarks,
                                             cal.CreatedBy,
                                             itm.IteamName
                                         }).ToList();

                        result = this.Json(new
                        {
                            Flag = true,
                            Message = "Record Found!!",
                            _AssetList,
                            CalibData
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
        public ActionResult CreateCalib(tAssetCalibration obj)
        {
            JsonResult result = new JsonResult();

            try
            {
                var userName = Convert.ToString(Session["AppUserName"]);
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);

                if (obj == null)
                {
                    return Json(new
                    {
                        Flag = false,
                        Message = "No data received."
                    }, JsonRequestBehavior.AllowGet);
                }

                if (obj.AssetId == 0)
                {
                    return Json(new
                    {
                        Flag = false,
                        Message = "Asset is required."
                    }, JsonRequestBehavior.AllowGet);
                }

                if (obj.CalibrationDate == null || obj.CalibrationDate == default(DateTime))
                {
                    return Json(new
                    {
                        Flag = false,
                        Message = "Calibration Date is required."
                    }, JsonRequestBehavior.AllowGet);
                }

                // Required fields
                obj.OrgInfoId = orgId;
                obj.CreatedBy = userName;
                obj.CreatedDate = DateTime.Now;
                obj.IsAction = true;
                obj.Result = "Pass";

                db.tAssetCalibrations.Add(obj);
                db.SaveChanges();

                result = Json(new
                {
                    Flag = true,
                    Message = "Calibration saved successfully."
                }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                string message = "";

                foreach (var eve in ex.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        message += ve.PropertyName + " : " + ve.ErrorMessage + Environment.NewLine;
                    }
                }

                result = Json(new
                {
                    Flag = false,
                    Message = message
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                result = Json(new
                {
                    Flag = false,
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                }, JsonRequestBehavior.AllowGet);
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
                var today = DateTime.Today;

                // Use tAssetCalibrations for proper filtering with IsAction
                // Note: AssetCalibrations table lacks IsAction field, using tAssetCalibrations instead
                var totalCalibrations = db.tAssetCalibrations
                                            .Where(x => x.OrgInfoId == orgId && x.IsAction == true)
                                            .Count();

                // Completed: calibrations where Result indicates pass/completion
                var completedCalibrations = db.tAssetCalibrations
                                            .Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.Result == "Pass")
                                            .Count();

                // Pending: calibrations with NextDueDate today or future (or null - not yet due)
                var pendingCalibrations = db.tAssetCalibrations
                                            .Where(x => x.OrgInfoId == orgId && x.IsAction == true && 
                                                       (x.NextDueDate == null || x.NextDueDate >= today))
                                            .Count();

                // Expired: calibrations where NextDueDate has passed
                var expiredCalibrations = db.tAssetCalibrations
                                            .Where(x => x.OrgInfoId == orgId && x.IsAction == true && 
                                                         x.NextDueDate.HasValue && x.NextDueDate.Value < today)
                                            .Count();

                return Json(new
                {
                    Total = totalCalibrations,
                    Completed = completedCalibrations,
                    Pending = pendingCalibrations,
                    Expired = expiredCalibrations
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new
                {
                    Total = 0,
                    Completed = 0,
                    Pending = 0,
                    Expired = 0
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetData()
        {
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);

                var data = (from cal in db.tAssetCalibrations
                            join itm in db.mIteamMasters on cal.AssetId equals itm.mIteamMasterId
                            join ag in db.mAgencies on cal.Agency equals ag.mAgencyId.ToString() into agJoin
                            from ag in agJoin.DefaultIfEmpty()
                            where cal.OrgInfoId == orgId && cal.IsAction == true
                            orderby cal.AssetCalibrationId descending
                            select new
                            {
                                cal.AssetCalibrationId,
                                AssetName = itm.IteamName,
                                cal.CertificateNo,
                                cal.CalibrationDate,
                                cal.NextDueDate,
                                cal.Result,
                                //Agency = ag != null ? ag.AgencyName : cal.Agency,
                                cal.Remarks,
                                cal.CreatedBy
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

                var data = (from cal in db.tAssetCalibrations
                            join itm in db.mIteamMasters on cal.AssetId equals itm.mIteamMasterId
                            where cal.AssetCalibrationId == id && cal.OrgInfoId == orgId && cal.IsAction == true
                            select new
                            {
                                cal.AssetCalibrationId,
                                cal.AssetId,
                                AssetName = itm.IteamName,
                                cal.CertificateNo,
                                cal.CalibrationDate,
                                cal.NextDueDate,
                                cal.Result,
                                cal.Agency,
                                cal.Remarks,
                                cal.CreatedBy
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
                var rec = db.tAssetCalibrations.FirstOrDefault(x => x.AssetCalibrationId == ID && x.OrgInfoId == orgId);

                if (rec != null)
                {
                    rec.IsAction = false;
                    rec.ModifiedDate = DateTime.Now;
                    rec.ModifiedBy = Convert.ToString(Session["AppUserName"]);
                    db.SaveChanges();

                    return Json(new { Flag = true, Message = "Calibration record deleted successfully." }, JsonRequestBehavior.AllowGet);
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
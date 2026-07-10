using HexaERP.MVC.Models;
using HexaERP.MVC.Service;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.SMT
{
    public class TakeAwaySMTBinController : Controller
    {
        // GET: TakeAwaySMTBin
        public static string UserName = string.Empty;

        [Authorize(Roles = "AD,PAK,AAD")]
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
                    UserName = cookieObject["AppUserName"];
                }
                else { return RedirectToAction("Index", "AppUser"); }
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "AppUser");
            }
            return View();
        }

        [HttpGet]
        public ActionResult GetTakeAwaySMTBin(string PartNumber, int Quantity)
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {
                var _qty = Quantity;

                using (var context = new ERPdbEntities())
                {
                    var TakewayList = context
                        .mSMTProducts
                        .Where(x => x.IsAction == true && x.IsPutaway == true && x.IsTakeaway == false && x.PartNumber.Trim() == PartNumber.Trim())
                        .OrderBy(p => p.CreatedDate);

                    List<mSMTProduct> _TakewayList = new List<mSMTProduct>();

                    var _avialbeQuantity = TakewayList.Sum(s => s.Quantity);

                    //var m = Math.Ceiling(Convert.ToDecimal(Quantity) / 15) * 1;
                    //var q = m * 15;

                    if (_qty > _avialbeQuantity)
                        this.Json(new { Flag = false, message = "QTY ENTERED IS HIGHER THAN AVAILABLE STOCK", _avialbeQuantity }, JsonRequestBehavior.AllowGet);


                    int counter = Quantity;

                    foreach (var i in TakewayList)
                    {
                        if (counter > 0)
                        {
                            _TakewayList.Add(new mSMTProduct
                            {
                                mSMTProductId = i.mSMTProductId,
                                Id = i.Id,
                                SerialNumber = i.SerialNumber,
                                PalletId = i.PalletId,
                                Quantity = i.Quantity,
                                DateAndTime = i.DateAndTime,
                                StatusId = i.StatusId,
                                Status = i.Status,
                                CustomerCode = i.CustomerCode,
                                ContainerId = i.ContainerId,
                                CustomerId = i.CustomerId,
                                Lot = i.Lot,
                                PartId = i.PartId,
                                PartNumber = i.PartNumber,
                                QADLine = i.QADLine,
                                ShiftId = i.ShiftId,
                                Station = i.Station,
                                Ble = i.Ble,
                                mZoneId = i.mZoneId,
                                ShelfName = i.ShelfName,
                                Remark = i.Remark,
                                Comment = i.Comment,
                                IsNormalReturn = i.IsNormalReturn,
                                IsQualityReturn = i.IsQualityReturn,
                                IsPutaway = i.IsPutaway,
                                IsTakeaway = i.IsTakeaway,
                                IsApprove = i.IsApprove,
                                LastSeenTime = i.LastSeenTime,
                                IsAction = i.IsAction,
                                CreatedDate = i.CreatedDate,
                                CreatedBy = i.CreatedBy,
                                ModifiedDate = i.ModifiedDate,
                                ModifiedBy = i.ModifiedBy

                            });
                            counter = counter - i.Quantity;
                        }
                    }

                    //var findEqual = TakewayList.Where(f => f.Quantity == Quantity).ToList().Take(1);
                    //if (findEqual != null)
                    //    return this.Json(new { Flag = true, message = "Record Found", _TakewayList = findEqual, _avialbeQuantity }, JsonRequestBehavior.AllowGet);

                    //var findLess = TakewayList.Where(f => f.Quantity < Quantity).FirstOrDefault();
                    //if (findLess != null)
                    //{
                    //}

                    //var _TakewayList = context.Database.SqlQuery<mSMTProduct>(@"SELECT * 
                    //FROM (SELECT o.*, SUM(Quantity) OVER (ORDER BY CreatedDate, Lot ASC) as total_qty
                    //FROM mSMTProduct o
                    //WHERE PartNumber = {0} AND IsTakeaway = 0 AND IsPutaway = 1) o
                    //WHERE o.total_qty < {1} OR o.total_qty > {1};",
                    //new object[] { PartNumber, _qty }).ToList();

                    if (_TakewayList.Count() > 0)
                    {
                        if (Quantity > _avialbeQuantity)
                            result = this.Json(new { Flag = false, message = "QTY ENTERED IS HIGHER THAN AVAILABLE STOCK", _avialbeQuantity }, JsonRequestBehavior.AllowGet);
                        else
                            result = this.Json(new { Flag = true, message = "Record Found", _TakewayList, _avialbeQuantity }, JsonRequestBehavior.AllowGet);
                    }
                    else
                        result = this.Json(new { Flag = false, message = "Record Not Found", _TakewayList }, JsonRequestBehavior.AllowGet);

                    //var _TakewayList = context.Database.SqlQuery<mSMTProduct>(@"SELECT * 
                    //FROM (SELECT o.*, SUM(Quantity) OVER (ORDER BY Quantity ASC) as total_qty
                    //FROM mSMTProduct o
                    //WHERE PartNumber = {0} AND IsTakeaway = 0 AND IsPutaway = 1) o
                    //WHERE o.total_qty - o.Quantity < {1};",
                    //                    new object[] { PartNumber, _qty }).ToList();

                    //                    if (_TakewayList.Count() > 0)
                    //                        result = this.Json(new { Flag = true, message = "Record Found", _TakewayList }, JsonRequestBehavior.AllowGet);
                    //                    else
                    //                        result = this.Json(new { Flag = false, message = "Record Not Found" }, JsonRequestBehavior.AllowGet);

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

        [HttpPost]
        public ActionResult SubmitBinOut(string SMTProductIds, string Ble, string ModifiedBy)
        {
            JsonResult result = new JsonResult();
            try
            {
                if (string.IsNullOrWhiteSpace(Ble) || Ble == "undefined")
                    return this.Json(new { Flag = false, Message = $"Scan Ble" }, JsonRequestBehavior.AllowGet);

                if (string.IsNullOrWhiteSpace(ModifiedBy) || ModifiedBy == "undefined")
                    return this.Json(new { Flag = false, Message = $"Employee id is required" }, JsonRequestBehavior.AllowGet);

                if (string.IsNullOrWhiteSpace(SMTProductIds) || SMTProductIds == "undefined")
                    return this.Json(new { Flag = false, Message = $"Select Product" }, JsonRequestBehavior.AllowGet);



                using (var context = new ERPdbEntities())
                {
                    if (SMTProductIds.Equals(Ble, StringComparison.Ordinal))
                    {
                        var _r = context.mSMTProducts.Where(b => b.Ble == SMTProductIds).FirstOrDefault();
                        if (_r != null)
                        {
                            var _findShelf = context.mShelves.Where(a => a.Barcode == _r.Ble).FirstOrDefault();
                            if (_findShelf != null)
                            {
                                _findShelf.IsAction = false;
                                _findShelf.Barcode = null;
                            }

                            if (_r.IsPutaway)
                            {
                                _r.Status = JobStatus.Takeaway;
                                _r.mZoneId = null;
                                _r.ShelfName = null;

                                _r.IsTakeaway = true;
                                _r.ModifiedDate = DateTime.Now;
                                _r.ModifiedBy = ModifiedBy;

                                context.Entry(_r).State = EntityState.Modified;
                                context.SaveChanges();

                                result = this.Json(new { Flag = true, Message = $"Takeaway done for {_r.PartNumber} - {_r.Ble}", Less = _r.Quantity }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else
                    {
                        return this.Json(new { Flag = false, Message = $"{SMTProductIds} ${Ble} not matching FIFO basis." }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex) { result = this.Json(new { Flag = false, Message = ex.InnerException.Message }, JsonRequestBehavior.AllowGet); }
            return result;
        }
        [HttpPost]
        public ActionResult SubmitTakeAway(string SMTProductIds, string ModifiedBy)
        {
            JsonResult result = new JsonResult();
            try
            {
                if (string.IsNullOrWhiteSpace(ModifiedBy) || ModifiedBy == "undefined")
                    return this.Json(new { Flag = false, Message = $"Employee id is required" }, JsonRequestBehavior.AllowGet);

                if (string.IsNullOrWhiteSpace(SMTProductIds))
                    return this.Json(new { Flag = false, Message = $"Select Product" }, JsonRequestBehavior.AllowGet);

                List<int> _SMTProductIds = null;
                _SMTProductIds = ConList(SMTProductIds);

                int RecordJob = 0;
                _SMTProductIds.ForEach(o =>
                {
                    using (var context = new ERPdbEntities())
                    {
                        var _r = context.mSMTProducts.Find(o);
                        if (_r != null)
                        {
                            var _findShelf = context.mShelves.Where(a => a.Barcode == _r.Ble).FirstOrDefault();
                            if (_findShelf != null)
                            {
                                _findShelf.IsAction = false;
                                _findShelf.Barcode = null;
                            }

                            if (_r.IsPutaway)
                            {
                                _r.Status = JobStatus.Takeaway;
                                _r.mZoneId = null;
                                _r.ShelfName = null;

                                _r.IsTakeaway = true;
                                _r.ModifiedDate = DateTime.Now;
                                _r.ModifiedBy = ModifiedBy;

                                context.Entry(_r).State = EntityState.Modified;
                                context.SaveChanges();
                                RecordJob = RecordJob + 1;
                            }
                        }
                    }
                });

                if (RecordJob > 0)
                    result = this.Json(new { Flag = true, Message = $"Takeaway done for {RecordJob} record", }, JsonRequestBehavior.AllowGet);
                else
                    result = this.Json(new { Flag = false, Message = $"Takeaway not done becouse record not found" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                result = this.Json(new { Flag = false, Message = ex.InnerException.Message }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }

        public static List<int> ConList(string a)
        {
            try
            {
                return a.Split(',').Select(x => x.Trim()).Select(x => Int32.Parse(x)).ToList();
            }
            catch (Exception) { return null; }
        }
    }
}
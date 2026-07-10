using HexaERP.MVC.Models;
using HexaERP.MVC.Service;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.SMT
{
    public class PutAwaySMTBinController : Controller
    {
        public static string UserName = string.Empty;
        // GET: PutAwaySMTBin
        [Authorize(Roles = "AD,PAK")]
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
        public ActionResult GetBinPosition(string LocatonBleMac, string _ProductShelf)
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {
                if (string.IsNullOrWhiteSpace(LocatonBleMac))
                    return this.Json(new { message = $"Location information is missing", Flag = false }, JsonRequestBehavior.AllowGet);

                if (string.IsNullOrWhiteSpace(_ProductShelf))
                    return this.Json(new { message = $"Product information is missing", Flag = false }, JsonRequestBehavior.AllowGet);

                using (var context = new ERPdbEntities())
                {
                    var _product = context.mSMTProducts.Where(b => b.Ble == _ProductShelf).FirstOrDefault();
                    if (_product == null)
                        return this.Json(new { message = $"Product not found", Flag = false }, JsonRequestBehavior.AllowGet);

                    var _location = context.mShelves.Where(b => b.ShelfName == LocatonBleMac)
                        .FirstOrDefault();
                    if (_location == null)
                        return this.Json(new { message = $"Shelf not found", Flag = false }, JsonRequestBehavior.AllowGet);

                    if (_product.ShelfName == _location.ShelfName)
                    {
                        return this.Json(new { message = $"location Matched!", Flag = true, _location }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        try
                        {
                            var t = new toTrackInfo
                            {
                                RFID = $"{LocatonBleMac} - {_ProductShelf} Scaned Shelf Mismatched!",
                                AppUserName = UserName,
                                //ReaderNo = ins.f.Name,
                                UID = $"BIN IN",
                                tDate = DateTime.Now
                            };
                            using (var c = new ERPdbEntities())
                            {
                                c.toTrackInfoes.Add(t);
                                c.SaveChanges();
                            }
                        }
                        catch (Exception) { }

                        return this.Json(new { Flag = false, message = "location is not matched" }, JsonRequestBehavior.AllowGet);
                    }
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

        [HttpGet]
        public ActionResult GetBinInfo(string BinNumber)
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {
                if (string.IsNullOrWhiteSpace(BinNumber))
                    result = this.Json(new { Flag = false, message = "Scan Ble Number" }, JsonRequestBehavior.AllowGet);

                string _BinNumber = BinNumber;
                using (var context = new ERPdbEntities())
                {

                    //var _BinInfo = context.Database.SqlQuery<SMTSearchModelView>(@"select p.*,m.* from mSMTProduct as p left join toMonitor as m on p.Ble = m.Epc where p.Ble = {0} AND p.IsPutaway = 0",
                    //    new object[] { _BinNumber }).FirstOrDefault();

                    var _BinInfo = context.Database.SqlQuery<SMTSearchModelView>(@"select z.Zone,p.*,m.*
from mSMTProduct as p
left join toMonitor as m on p.Ble = m.Epc 
left join mReaderSettup as r on m.Name = r.ReaderNo
left join mZone as z on r.mZoneId = z.mZoneId 
where p.Ble = {0} AND p.IsPutaway = 0",
                      new object[] { _BinNumber }).FirstOrDefault();

                    if (_BinInfo != null)
                    {
                        if (_BinInfo.IsPutaway)
                            result = this.Json(new { message = $"Putaway done for this product", Flag = false }, JsonRequestBehavior.AllowGet);
                        else
                            result = this.Json(new { message = $"Success", Flag = true, _BinInfo }, JsonRequestBehavior.AllowGet);
                    }
                    else
                        result = this.Json(new { Flag = false, message = "Record not found" }, JsonRequestBehavior.AllowGet);
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

        [HttpGet]
        public ActionResult SubmitPutAwaySMTBin(string ModifiedBy, int mSMTProductId)
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {
                if (string.IsNullOrWhiteSpace(ModifiedBy))
                    return this.Json(new { message = $"Employee Id is required", Flag = false }, JsonRequestBehavior.AllowGet);

                if (mSMTProductId == null)
                    return this.Json(new { message = $"SMT Product not found for putaway", Flag = false }, JsonRequestBehavior.AllowGet);

                using (var context = new ERPdbEntities())
                {
                    var _product = context.mSMTProducts.Find(mSMTProductId);

                    if (_product.IsPutaway)
                        return this.Json(new { message = $"Putaway already done to this device", Flag = false }, JsonRequestBehavior.AllowGet);


                    _product.ModifiedBy = ModifiedBy;
                    _product.ModifiedDate = DateTime.Now;
                    _product.IsPutaway = true;
                    _product.Status = JobStatus.Putaway;

                    context.Entry(_product).State = EntityState.Modified;
                    context.SaveChanges();

                    result = this.Json(new { Flag = true, message = $"Successully Putway done for : {_product.PartNumber}" }, JsonRequestBehavior.AllowGet);
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
    }
}
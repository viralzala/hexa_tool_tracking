using HexaERP.MVC.Models;
using HexaERP.MVC.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.SMT
{
    public class TagBLEtoBinController : Controller
    {
        // private ERPdbEntities db = new ERPdbEntities();

        public static string UserName = string.Empty;

        // GET: TagBLEtoBin
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
                    UserName = cookieObject["AppUserName"];
                    ViewBag.LogedIn = cookieObject["AppUserName"];
                }
                else { return RedirectToAction("Index", "AppUser"); }
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "AppUser");
            }
            return View();
        }

        //TagBLEtoBin/Create
        [HttpPost]
        public ActionResult Create(tAssetTag collection)
        {
            JsonResult result = new JsonResult();

            try
            {
                var UserName = Session["AppUserName"];
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);

                if (string.IsNullOrEmpty(collection.RFID))
                {
                    return this.Json(new { Message = $"BLE Tag Should Not be Empty", Flag = false }, JsonRequestBehavior.AllowGet);
                }
                else if (string.IsNullOrEmpty(collection.ModelNo))
                {
                    return this.Json(new { Message = $"Enter Part Number", Flag = false }, JsonRequestBehavior.AllowGet);


                }
                else if (string.IsNullOrEmpty(Convert.ToString(collection.Stock)))
                {
                    return this.Json(new { Message = $"Missing Stock Information", Flag = false }, JsonRequestBehavior.AllowGet);

                }
                else if (string.IsNullOrEmpty(Convert.ToString(collection.PurchaseDate)))
                {
                    return this.Json(new { Message = $"Enter Pro Date", Flag = false }, JsonRequestBehavior.AllowGet);

                }
                else if (string.IsNullOrEmpty(Convert.ToString(collection.SerialNo)))
                {
                    return this.Json(new { Message = $"Enter SerialNo/Lot", Flag = false }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    using (var db = new ERPdbEntities())
                    {
                        if (db.tAssetTags.Any(o => o.RFID == collection.RFID && o.IsAction == true))
                        {
                            return this.Json(new { Message = $"{collection.RFID} Same BLE Tag Alerdy In Use", Flag = false }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var _findShelf = db.mShelves.Where(a => a.IsAction == false).FirstOrDefault();

                            if (_findShelf == null)
                                return this.Json(new { message = $"Shel is full! or not found", Flag = false }, JsonRequestBehavior.AllowGet);

                            collection.mZoneId = _findShelf.mZoneId;
                            collection.IteamCode = _findShelf.ShelfName;
                            //
                            String Uid = DateTime.Now.ToString().GetHashCode().ToString("x");
                            collection.UID = Uid;
                            collection.bStock = collection.Stock;
                            collection.OrgInfoId = orgId;
                            //                    
                            collection.CreatedDate = DateTime.Now;
                            collection.CreatedBy = UserName.ToString();
                            collection.IsAction = true;

                            db.tAssetTags.Add(collection);
                            db.SaveChanges();

                            _findShelf.IsAction = true;
                            _findShelf.Barcode = collection.RFID;
                            db.Entry(_findShelf).State = EntityState.Modified;
                            db.SaveChanges();

                            return this.Json(new { message = $"Asset Record Added Successfully", Flag = true }, JsonRequestBehavior.AllowGet);
                        }
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
                return this.Json(new { message = message, Flag = false }, JsonRequestBehavior.AllowGet);
            }
        }

        private static string[] GetFileNames(string path, string filter)
        {
            string[] files = Directory.GetFiles(path, filter);
            for (int i = 0; i < files.Length; i++)
                files[i] = Path.GetFileName(files[i]);
            return files;
        }

        [HttpGet]
        public ActionResult ReadFileFromLocation()
        {
            JsonResult result = new JsonResult();
            // var BaseFolder = "C:\\Database";
            var BaseFolder = Convert.ToString(ConfigurationManager.AppSettings["BaseFolder"]);

            //  string[] pickFile = GetFileNames($"{BaseFolder}", "*.json");
            string[] pickFile = GetFileNames($"{BaseFolder}", "*");

            if (pickFile.Length <= 0)
                return this.Json(new { message = $"Json File not found", Flag = false, t = DateTime.Now.ToString("dddd, dd MMMM yyyy") }, JsonRequestBehavior.AllowGet);

            var FileName = pickFile[0];
            var BaseLocation = $"{BaseFolder}\\{FileName}";

            try
            {
                using (StreamReader r = new StreamReader($"{BaseLocation}"))
                {
                    string json = r.ReadToEnd();

                    string cleanData = ReplaceFlowerBracket(json);

                    List<mSMTProductModelView> items = JsonConvert.DeserializeObject<List<mSMTProductModelView>>(cleanData);

                    if (items.Count > 0)
                        result = this.Json(new { Flag = true, items, message = "Record Found" }, JsonRequestBehavior.AllowGet);
                    else
                        result = this.Json(new { Flag = false, items, message = "Record not Found" }, JsonRequestBehavior.AllowGet);
                }

                //if (Directory.Exists(BaseFolder))
                //{
                //    foreach (var file in new DirectoryInfo(BaseFolder).GetFiles())
                //    {
                //        if (FileName == file.Name)
                //            file.MoveTo($@"{BaseFolder}\\Task\\{file.Name}");
                //    }
                //}

            }
            catch (Exception e)
            {
                result = this.Json(new { Flag = false, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }

        [HttpGet]
        public ActionResult PushFileTODB(string Ble, string _CreatedBy, bool IsMaster)

        {
            JsonResult result = new JsonResult();

            if (string.IsNullOrWhiteSpace(Ble))
                return this.Json(new { message = $"Scan Ble", Flag = false, t = DateTime.Now.ToString("dddd, dd MMMM yyyy") }, JsonRequestBehavior.AllowGet);

            if (string.IsNullOrWhiteSpace(_CreatedBy))
                return this.Json(new { message = $"Scan Employee Id", Flag = false, t = DateTime.Now.ToString("dddd, dd MMMM yyyy") }, JsonRequestBehavior.AllowGet);

            // var BaseFolder = "C:\\Database";
            var BaseFolder = Convert.ToString(ConfigurationManager.AppSettings["BaseFolder"]);

            //  string[] pickFile = GetFileNames($"{BaseFolder}", "*.json");
            string[] pickFile = GetFileNames($"{BaseFolder}", "*");

            if (pickFile.Length <= 0)
                return this.Json(new { message = $"Json File not found", Flag = false, t = DateTime.Now.ToString("dddd, dd MMMM yyyy") }, JsonRequestBehavior.AllowGet);

            var FileName = pickFile[0];
            var BaseLocation = $"{BaseFolder}\\{FileName}";

            try
            {
                int _zoneId = 0; string _shelfName = $"Empty";
                using (StreamReader r = new StreamReader($"{BaseLocation}"))
                {
                    string json = r.ReadToEnd();

                    string cleanData = ReplaceFlowerBracket(json);

                    List<mSMTProductModelView> items = JsonConvert.DeserializeObject<List<mSMTProductModelView>>(cleanData);

                    using (var ERPdbEntities = new ERPdbEntities())
                    {
                        foreach (var insert in items)
                        {
                            var _bleUse = ERPdbEntities.mSMTProducts.Where(s => s.Ble.Equals(Ble, StringComparison.OrdinalIgnoreCase) && s.IsAction == true).FirstOrDefault();
                            if (_bleUse != null)
                            {
                                try
                                {
                                    var t = new toTrackInfo
                                    {
                                        RFID = $"{Ble} Ble address already in use",
                                        AppUserName = UserName,
                                        //ReaderNo = ins.f.Name,
                                        UID = $"Bin Pack",
                                        tDate = DateTime.Now
                                    };
                                    using (var c = new ERPdbEntities())
                                    {
                                        c.toTrackInfoes.Add(t);
                                        c.SaveChanges();
                                    }
                                }
                                catch (Exception) { }
                                return this.Json(new { message = $"Ble address already in use for :{_bleUse.Ble}", Flag = false, t = DateTime.Now.ToString("dddd, dd MMMM yyyy") }, JsonRequestBehavior.AllowGet);
                            }

                            //var Dublicate = ERPdbEntities.mSMTProducts.Where(s => s.SerialNumber == insert.SerialNumber.Trim()).FirstOrDefault();
                            //if (Dublicate != null)
                            //    return this.Json(new { Flag = false, message = "Serial number already exist" }, JsonRequestBehavior.AllowGet);


                            if (IsMaster)
                            {
                                insert.IsPutaway = true;
                                insert.IsTakeaway = true;
                                insert.Status = JobStatus.Takeaway;
                            }
                            else
                            {
                                var _findShelf = ERPdbEntities.mShelves.Where(a => a.Barcode != null && a.IsAction == true).OrderByDescending(o => o.mShelfId).FirstOrDefault();
                                if (_findShelf == null)
                                {
                                    var _f = ERPdbEntities.mShelves.Where(a => a.IsAction == false).FirstOrDefault();
                                    if (_f == null)
                                        return this.Json(new { message = $"Shel is full! or not found", Flag = false, t = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") }, JsonRequestBehavior.AllowGet);

                                    insert.mZoneId = _f.mZoneId;
                                    insert.ShelfName = _f.ShelfName;

                                    _f.IsAction = true;
                                    _f.Barcode = Ble;
                                    _f.CreatedDate = DateTime.Now;
                                    _f.CreatedBy = _CreatedBy;

                                    ERPdbEntities.Entry(_f).State = EntityState.Modified;
                                    ERPdbEntities.SaveChanges();
                                }
                                else
                                {
                                    var _id = _findShelf.mShelfId + 1;

                                    var _s = ERPdbEntities.mShelves.Where(a => a.mShelfId == _id && a.IsAction == false).FirstOrDefault();
                                    if (_s != null)
                                    {
                                        insert.mZoneId = _s.mZoneId;
                                        insert.ShelfName = _s.ShelfName;

                                        _s.IsAction = true;
                                        _s.Barcode = Ble;
                                        _s.CreatedDate = DateTime.Now;
                                        _s.CreatedBy = _CreatedBy;

                                        ERPdbEntities.Entry(_s).State = EntityState.Modified;
                                        ERPdbEntities.SaveChanges();
                                    }
                                    else
                                    {

                                        var _el = ERPdbEntities.mShelves.Where(a => a.Barcode == null && a.IsAction == false).FirstOrDefault();
                                        if (_el != null)
                                        {
                                            insert.mZoneId = _el.mZoneId;
                                            insert.ShelfName = _el.ShelfName;

                                            _el.IsAction = true;
                                            _el.Barcode = Ble;
                                            _el.CreatedDate = DateTime.Now;
                                            _el.CreatedBy = _CreatedBy;

                                            ERPdbEntities.Entry(_el).State = EntityState.Modified;
                                            ERPdbEntities.SaveChanges();
                                        }
                                        else
                                            return this.Json(new { message = $"Location not found", Flag = false, t = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }


                            insert.Quantity = RemoveNonNumeric(insert.Quantity);
                            //insert.PartNumber = RemovePreFixChar(insert.PartNumber);

                            insert.IsAction = true;
                            insert.CreatedDate = DateTime.Now;
                            // _mSMTProduct.CreatedBy = UserName;
                            insert.Status = JobStatus.SMTMarketPlace;

                            var smtP = new mSMTProduct
                            {
                                Ble = Ble,
                                Id = insert.Id,
                                SerialNumber = insert.SerialNumber,
                                PalletId = insert.PalletId,
                                Quantity = Convert.ToInt32(insert.Quantity),
                                DateAndTime = insert.DateAndTime,
                                StatusId = insert.StatusId,
                                Status = insert.Status,
                                CustomerCode = insert.CustomerCode,
                                ContainerId = insert.ContainerId,
                                CustomerId = insert.CustomerId,
                                Lot = insert.LotName,
                                PartId = insert.PartId,
                                PartNumber = insert.PartNumber,
                                QADLine = insert.QADLine,
                                ShiftId = insert.ShiftId,
                                Station = insert.Station,
                                mZoneId = insert.mZoneId,
                                ShelfName = insert.ShelfName,
                                Remark = insert.Remark,
                                Comment = insert.Comment,
                                IsNormalReturn = insert.IsNormalReturn,
                                IsQualityReturn = insert.IsQualityReturn,
                                IsPutaway = insert.IsPutaway,
                                IsTakeaway = insert.IsTakeaway,
                                IsAssembly = insert.IsAssembly,
                                IsMaster = insert.IsMaster,
                                IsApprove = insert.IsApprove,
                                LastSeenTime = insert.LastSeenTime,
                                IsAction = insert.IsAction,
                                CreatedDate = insert.CreatedDate,
                                CreatedBy = _CreatedBy,
                                ModifiedDate = insert.ModifiedDate,
                                ModifiedBy = insert.ModifiedBy
                            };

                            ERPdbEntities.mSMTProducts.Add(smtP);
                            ERPdbEntities.SaveChanges();
                        }
                    }
                }

                if (Directory.Exists(BaseFolder))
                {
                    foreach (var file in new DirectoryInfo(BaseFolder).GetFiles())
                    {
                        if (FileName == file.Name)
                            file.MoveTo($@"{BaseFolder}\\Task\\{file.Name}");
                    }
                }

                result = this.Json(new { Flag = true, message = "Successfull Added", t = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                result = this.Json(new { Flag = false, message = e.Message, t = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }

        private string ReplaceFlowerBracket(string input)
        {
            return Regex.Replace(input, "{", "[{").Replace("}", "}]");
            // return input.Replace("{", "[{").Replace("}", "]}");
        }

        [HttpPost]
        public ActionResult SubmitSMTProduct(mSMTProductModelView insert)
        {
            JsonResult result = new JsonResult();

            if (string.IsNullOrWhiteSpace(insert.Ble))
                return this.Json(new { message = $"Scan Ble", Flag = false, t = DateTime.Now.ToString("dddd, dd MMMM yyyy") }, JsonRequestBehavior.AllowGet);

            if (string.IsNullOrWhiteSpace(insert.CreatedBy))
                return this.Json(new { message = $"Scan Employee Id", Flag = false, t = DateTime.Now.ToString("dddd, dd MMMM yyyy") }, JsonRequestBehavior.AllowGet);


            try
            {
                if (string.IsNullOrEmpty(insert.PartNumber))
                    return this.Json(new { message = $"Enter Part Number", Flag = false }, JsonRequestBehavior.AllowGet);

                if (string.IsNullOrEmpty(Convert.ToString(insert.Quantity)))
                    return this.Json(new { message = $"Missing Stock Information", Flag = false }, JsonRequestBehavior.AllowGet);

                //if (string.IsNullOrEmpty(Convert.ToString(insert.DateAndTime)))
                //    return this.Json(new { message = $"Enter Pro Date", Flag = false }, JsonRequestBehavior.AllowGet);


                Boolean fileAction = false;

                string _shelfName = $"Empty";

                using (var ERPdbEntities = new ERPdbEntities())
                {
                    var _bleUse = ERPdbEntities.mSMTProducts.Where(s => s.Ble.Equals(insert.Ble, StringComparison.OrdinalIgnoreCase) && s.IsAction == true).FirstOrDefault();
                    if (_bleUse != null)
                    {
                        try
                        {
                            var t = new toTrackInfo
                            {
                                RFID = $"{insert.Ble} Ble address already in use",
                                AppUserName = UserName,
                                //ReaderNo = ins.f.Name,
                                UID = $"Bin Pack",
                                tDate = DateTime.Now
                            };
                            using (var c = new ERPdbEntities())
                            {
                                c.toTrackInfoes.Add(t);
                                c.SaveChanges();
                            }
                        }
                        catch (Exception) { }
                        return this.Json(new { message = $"Ble address already in use for :{_bleUse.Ble}", Flag = false, t = DateTime.Now.ToString("dddd, dd MMMM yyyy") }, JsonRequestBehavior.AllowGet);
                    }

                    if (insert.IsMaster)
                    {
                        insert.IsPutaway = true;
                        insert.IsTakeaway = true;
                        insert.Status = JobStatus.Takeaway;
                    }
                    else
                    {
                        var _findShelf = ERPdbEntities.mShelves.Where(a => a.Barcode != null && a.IsAction == true).OrderByDescending(o => o.mShelfId).FirstOrDefault();
                        if (_findShelf == null)
                        {
                            var _f = ERPdbEntities.mShelves.Where(a => a.IsAction == false).FirstOrDefault();
                            if (_f == null)
                                return this.Json(new { message = $"Shel is full! or not found", Flag = false, t = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") }, JsonRequestBehavior.AllowGet);

                            insert.mZoneId = _f.mZoneId;
                            insert.ShelfName = _f.ShelfName;

                            _f.IsAction = true;
                            _f.Barcode = insert.Ble;
                            _f.CreatedDate = DateTime.Now;
                            _f.CreatedBy = insert.CreatedBy;

                            ERPdbEntities.Entry(_f).State = EntityState.Modified;
                            ERPdbEntities.SaveChanges();
                        }
                        else
                        {
                            var _id = _findShelf.mShelfId + 1;

                            var _s = ERPdbEntities.mShelves.Where(a => a.mShelfId == _id && a.IsAction == false).FirstOrDefault();
                            if (_s != null)
                            {
                                insert.mZoneId = _s.mZoneId;
                                insert.ShelfName = _s.ShelfName;

                                _s.IsAction = true;
                                _s.Barcode = insert.Ble;
                                _s.CreatedDate = DateTime.Now;
                                _s.CreatedBy = insert.CreatedBy;

                                ERPdbEntities.Entry(_s).State = EntityState.Modified;
                                ERPdbEntities.SaveChanges();
                            }
                            else
                            {

                                var _el = ERPdbEntities.mShelves.Where(a => a.Barcode == null && a.IsAction == false).FirstOrDefault();
                                if (_el != null)
                                {
                                    insert.mZoneId = _el.mZoneId;
                                    insert.ShelfName = _el.ShelfName;

                                    _el.IsAction = true;
                                    _el.Barcode = insert.Ble;
                                    _el.CreatedDate = DateTime.Now;
                                    _el.CreatedBy = insert.CreatedBy;

                                    ERPdbEntities.Entry(_el).State = EntityState.Modified;
                                    ERPdbEntities.SaveChanges();
                                }
                                else
                                    return this.Json(new { message = $"Location not found", Flag = false, t = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }


                    insert.Quantity = RemoveNonNumeric(insert.Quantity);
                    insert.PartNumber = RemovePreFixChar(insert.PartNumber);
                    insert.Lot = RemovePreFixChar(insert.Lot);


                    insert.IsAction = true;
                    insert.CreatedDate = DateTime.Now;
                    // _mSMTProduct.CreatedBy = UserName;
                    insert.Status = JobStatus.SMTMarketPlace;

                    var smtP = new mSMTProduct
                    {
                        Ble = insert.Ble,
                        Id = insert.Id,
                        SerialNumber = insert.SerialNumber,
                        PalletId = insert.PalletId,
                        Quantity = Convert.ToInt32(insert.Quantity),
                        DateAndTime = insert.DateAndTime,
                        StatusId = insert.StatusId,
                        Status = insert.Status,
                        CustomerCode = insert.CustomerCode,
                        ContainerId = insert.ContainerId,
                        CustomerId = insert.CustomerId,
                        Lot = insert.Lot,
                        PartId = insert.PartId,
                        PartNumber = insert.PartNumber,
                        QADLine = insert.QADLine,
                        ShiftId = insert.ShiftId,
                        Station = insert.Station,
                        mZoneId = insert.mZoneId,
                        ShelfName = insert.ShelfName,
                        Remark = insert.Remark,
                        Comment = insert.Comment,
                        IsNormalReturn = insert.IsNormalReturn,
                        IsQualityReturn = insert.IsQualityReturn,
                        IsPutaway = insert.IsPutaway,
                        IsTakeaway = insert.IsTakeaway,
                        IsAssembly = insert.IsAssembly,
                        IsMaster = insert.IsMaster,
                        IsApprove = insert.IsApprove,
                        LastSeenTime = insert.LastSeenTime,
                        IsAction = insert.IsAction,
                        CreatedDate = insert.CreatedDate,
                        CreatedBy = insert.CreatedBy,
                        ModifiedDate = insert.ModifiedDate,
                        ModifiedBy = insert.ModifiedBy
                    };

                    ERPdbEntities.mSMTProducts.Add(smtP);
                    ERPdbEntities.SaveChanges();

                    fileAction = true;
                }
                result = this.Json(new { Flag = true, message = "Successfull Added", t = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                result = this.Json(new { Flag = false, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }


        static string RemoveNonNumeric(string input)
        {
            return Regex.Replace(input, "[^0-9]", "");
        }
        static string RemovePreFixChar(string input)
        {
            return input.Substring(1);
        }
    }
}
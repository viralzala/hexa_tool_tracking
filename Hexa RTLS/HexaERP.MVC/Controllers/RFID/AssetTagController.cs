using HexaERP.MVC.EmailConfig;
using HexaERP.MVC.Hubs;
using HexaERP.MVC.Models;

using Impinj.OctaneSdk;
using J_RFID;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class AssetTagController : Controller
    {

        private static IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<GetTags>();

        private ERPdbEntities db = new ERPdbEntities();

        RFIDAPI NFC_API = new RFIDAPI();
        int Err = 0;

        string[] UID_ID = new string[500];
        ulong[] UID_Count = new ulong[500];

        //string EPC = "", Cardtype = "0";
        //ulong EPC_Count = 0;
        //int Tim4out = 0;

        static string outstr = "", test = "", Times = "100";
        //static string Str = ""; static bool Flag = false;
        static Dictionary<string, DateTime> TagObj = new Dictionary<string, DateTime>();

        static List<GetReaderObj> TagColl = new List<GetReaderObj>();

        //******************
        //Author: Mudassar I     
        //******************
        // Create an instance of the ImpinjReader class.
        static ImpinjReader reader = new ImpinjReader();

        // Create a Dictionary to store the tags we've read.
        static Dictionary<string, Tag> tagsRead = new Dictionary<string, Tag>();
        static List<entitys> tags = new List<entitys>();

        // GET: AssetTag
        public ActionResult Index()
        {
            try
            {
                TagObj.Clear();
                TagColl.Clear();

                //--- Get cookie Collection.
                HttpCookie cookieObject = Request.Cookies["HexaCookie"];
                //--- Check for null 
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

        //Get All Master
        [HttpGet]
        public JsonResult getMasterData()
        {
            var UserName = Session["AppUserName"];
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjReaderIP = db.mReaderSettups.Where(j => j.IsAction == true && j.OrgInfoId == orgId).Select(c => new { c.ReaderIP, c.ReaderNo }).Distinct().ToList();
            var ObjUnit = (from Dis in db.mUnitMasters
                           where (Dis.OrgInfoId == orgId && Dis.IsAction == true)
                           select new { Dis.mUnitMasterId, Dis.UnitName }).ToList();
            var ObjIteam = (from Dis in db.mIteamMasters
                            where (Dis.OrgInfoId == orgId && Dis.IsAction == true)
                            select new { Dis.mIteamMasterId, Dis.IteamName, Dis.IteamCode }).ToList();
            var ObjGroup = (from Dis in db.mGroupMasters
                            where (Dis.OrgInfoId == orgId && Dis.IsAction == true)
                            select new { Dis.mGroupMasterId, Dis.GroupName }).ToList();
            var ObjIteamType = (from Dis in db.mIteamTypeMasters
                                where (Dis.OrgInfoId == orgId && Dis.IsAction == true)
                                select new { Dis.mIteamTypeMasterId, Dis.IteamType }).ToList();

            var mSite = db.mSiteMasters
                .Where(x => x.IsAction == true && x.OrgInfoId == orgId)
                .Select(x => new { x.mSiteMasterId, x.Site })
                .ToList();

            var vendor = db.mVendors
                .Where(x => x.IsAction == true && x.OrgInfoId == orgId)
                .Select(x => new { x.mVendorId, x.VendorName })
                .ToList();

            //Convert List Data to The Json Array       
            //Convert List Data to The Json Array          
            return Json(new { ObjReaderIP, ObjUnit, ObjIteam, ObjGroup, ObjIteamType, mSite, vendor }, JsonRequestBehavior.AllowGet);
        }

        // GET: AssetTag/getSubCategory2 (cascading by Asset Sub Category)
        [HttpGet]
        public JsonResult getSubCategory2(int subCategoryId)
        {
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                var data = db.mAssetSubCategory2
                    .Where(x => x.AssetSubCategoryId == subCategoryId && x.IsActive == true)
                    .Select(c => new { c.AssetSubCategory2Id, c.AssetSubCategory2Name })
                    .ToList();
                return Json(new { Flag = true, Message = "Data Loaded Successfully", DSubCategory2 = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message, DSubCategory2 = (object)null }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: AssetTag/getSubCategory1 (cascading by Asset Category)
        [HttpGet]
        public JsonResult getSubCategory1(int categoryId)
        {
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                var data = db.mIteamTypeMasters
                    .Where(x => x.mGroupMasterId == categoryId && x.IsAction == true)
                    .Select(c => new { c.mIteamTypeMasterId, c.IteamType })
                    .ToList();
                return Json(new { Flag = true, Message = "Data Loaded Successfully", DSubCategory1 = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message, DSubCategory1 = (object)null }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: AssetTag/getZones
        [HttpGet]
        public JsonResult getZones(int id)
        {
            try
            {
                var getZone = db.mZones.Where(x => x.IsAction == true && x.mSiteMasterId == id).ToList();
                return Json(new { Flag = true, Message = "Data Loaded Sucessfully", DZone = getZone.ToArray() }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        // GET: AssetTag/getSubZones
        [HttpGet]
        public JsonResult getSubZones(int id)
        {
            try
            {
                var getSubZone = db.mFloorMasters.Where(x => x.IsAction == true && x.mZoneId == id).ToList();
                return Json(new { Flag = true, Message = "Data Loaded Sucessfully", DZone = getSubZone.ToArray() }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        // GET: AssetTag/Area
        [HttpGet]
        public JsonResult getArea(int id)
        {
            try
            {
                var mroom = db.mRoomMasters.Where(x => x.IsAction == true && x.mFloorMasterId == id).ToList();
                return Json(new { Flag = true, Message = "Data Loaded Sucessfully", DArea = mroom.ToArray() }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        // GET: 
        [HttpGet]
        public JsonResult GetPorts()
        {
            string[] strAryCom = null;

            Dictionary<string, int> dObj = new Dictionary<string, int>();

            strAryCom = SerialPort.GetPortNames();

            for (int i = 0; i < strAryCom.Length; i++)
            {
                dObj.Add(strAryCom[i], i);
                //ComBxTm_COM2.Items.Add((object)strAryCom[i]);
            }
            return Json(dObj.ToArray(), JsonRequestBehavior.AllowGet);
        }

        // GET: 
        [HttpGet]
        public JsonResult PutStart(string _Port)
        {
            string Str = ""; //bool Flag = false;

            if (_Port == "" || _Port == null)
            {
                return Json(new { Flag = false, Msg = "Port Null" }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                Err = NFC_API.UHC_OpenReader(_Port);
                if (Err != 0)
                {
                    Str = ("Open COM Err " + Err);
                    return Json(new { Flag = false, Msg = "Open COM Err " + Err }, JsonRequestBehavior.AllowGet);
                }
                Err = NFC_API.UHF_FwVersion(out Str);
                Err = NFC_API.UHF_ReaderID(out Str);


                System.Timers.Timer timer = new System.Timers.Timer();
                timer.Interval = 1000;
                timer.Elapsed += timer_Elapsed;
                timer.Start();

                return Json(new { Flag = true, Msg = "Reader Started Tap the Tag" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                NFC_API.UHF_CloseReader(_Port);
                Str = "Error In Port:" + ex;
                return Json(new { Flag = false, Msg = Str }, JsonRequestBehavior.AllowGet);
            }

            //return Json(new { Flag = Flag, Msg = Str }, JsonRequestBehavior.AllowGet);
        }
        //Timer Method
        private void timer_Elapsed(object sender, EventArgs e)
        {
            try
            {
                Err = NFC_API.UHF_GetEPC(Times, out outstr);
                if (Err != 1)
                {
                    test = Regex.Replace(outstr, ".{4}", "$0 ").Trim();
                    _hubContext.Clients.All.getrfid(test.ToString(), test.ToString());
                }
            }
            catch (Exception) { }
        }

        // GET: 
        [HttpGet]
        public JsonResult getDataTap()
        {

            try
            {
                Err = NFC_API.UHF_GetEPC(Times, out outstr);

                if (Err != 1)
                {
                    test = Regex.Replace(outstr, ".{4}", "$0 ").Trim();
                    _hubContext.Clients.All.getrfid(test.ToString(), test.ToString());
                    if (!TagObj.ContainsKey(test))
                    {
                        GetReaderObj obj = new GetReaderObj();
                        obj.RFID = test; obj.tDate = DateTime.Now;
                        // Add this tag to the list of tags we've read.                       
                        TagObj.Add(test, DateTime.Now);
                        TagColl.Add(obj);
                    }
                    return Json(new { Flag = true, Msg = "New Tag", Datas = TagColl }, JsonRequestBehavior.AllowGet);
                    //Flag = true; Str = "New Tag";
                }
                //else {
                //    //return Json(new { Flag = false, Msg = "Error Failed to connect the reader" }, JsonRequestBehavior.AllowGet);
                //}

            }
            catch (Exception ex) { return Json(new { Flag = false, Msg = "Error:" + ex }, JsonRequestBehavior.AllowGet); }

            return Json(new { Flag = false, Msg = "Reader Ideal" }, JsonRequestBehavior.AllowGet);
            //return Json(TagColl, JsonRequestBehavior.AllowGet);
        }

        public partial class GetReaderObj
        {
            public string RFID { get; set; }
            public DateTime tDate { get; set; }
        }

        [HttpGet]
        public string ReaderInit(string Reader)
        {
            string msg = "";

            if (Reader == "")
            {
                return msg = "Please Select Reader IP Address To Connect";
            }
            try
            {

                // Connect to the reader.
                // Change the ReaderHostname constant in SolutionConstants.cs 
                // to the IP address or hostname of your reader.
                reader.Connect(Reader);

                Status status = reader.QueryStatus();

                try
                {
                    // Don't call the Stop method if the
                    // reader is already stopped.
                    if (reader.QueryStatus().IsSingulating)
                    {
                        reader.Stop();
                    }
                }
                catch (OctaneSdkException ex)
                {
                    // An Octane SDK exception occurred. Handle it here.                  
                    return msg = "('An Octane SDK exception has occurred : {0}', " + ex.Message + ")";
                }
                catch (Exception ex)
                {
                    // A general exception occurred. Handle it here.                    
                    return msg = "('An Octane SDK exception has occurred : {0}', " + ex.Message + ")";
                }
                // Get the default settings
                // We'll use these as a starting point
                // and then modify the settings we're 
                // interested in.
                Settings settings = reader.QueryDefaultSettings();

                // Tell the reader to include the TID
                // in all tag reports. We will use FastID
                // to do this. FastID is supported
                // by Impinj Monza 4 and later tags.
                settings.Report.IncludeFastId = true;
                settings.Report.IncludeAntennaPortNumber = true;
                // Apply the newly modified settings.
                reader.ApplySettings(settings);
                // Assign the TagsReported event handler.
                // This specifies which method to call
                // when tags reports are available.
                reader.TagsReported += OnTagsReported;

                // Start reading.
                reader.Start();

                msg = "Reader started...";
                //// Wait for the user to press enter.
                //Console.WriteLine("Press enter to exit.");
                //Console.ReadLine();

                //// Stop reading.
                //reader.Stop();

                //// Disconnect from the reader.
                //reader.Disconnect();
            }
            catch (OctaneSdkException e)
            {
                // Handle Octane SDK errors.                
                msg = e.Message.ToString();
            }
            catch (Exception e)
            {
                // Handle other .NET errors.               
                msg = e.Message.ToString();
            }
            return msg;
        }
        //
        public void OnTagsReported(ImpinjReader sender, TagReport report)
        {
            entitys on = new entitys();
            // This event handler is called asynchronously 
            // when tag reports are available.
            // Loop through each tag in the report 
            // and print the data.
            foreach (Tag tag in report)
            {
                // If this tag hasn't been read before, print out the EPC and TID
                if (!tagsRead.ContainsKey(tag.Epc.ToString()))
                {
                    _hubContext.Clients.All.GetRFID(tag.Epc.ToString(), tag.Epc.ToString());
                    // Add this tag to the list of tags we've read.
                    tagsRead.Add(tag.Epc.ToString(), tag);
                    on.RFID = tag.Epc.ToString(); on.PORTID = tag.AntennaPortNumber;
                    tags.Add(on);
                    GetIds();
                }
            }

        }
        public class entitys
        {
            public string RFID;
            public int PORTID;
        }
        //
        [HttpGet]
        public JsonResult GetIds()
        {

            if (tags.Count > 0)
            {
                return Json(tags, JsonRequestBehavior.AllowGet);
            }
            else
            {
            }
            return Json(tags, JsonRequestBehavior.AllowGet);
            //tagsRead.ToList();
            //return Session["rfidStored"].ToString();
        }
        //
        [HttpGet]
        public string ReaderClear()
        {
            string msg = "";
            try
            {
                tags.Clear();
                tagsRead.Clear();
                msg = "Data Cleared..";
            }
            catch (Exception ex)
            {
                msg = ex.Message.ToString();
            }
            return msg;
        }
        //
        [HttpGet]
        public string StopReaders()
        {
            string msg = "";
            try
            {
                // Don't call the Stop method if the
                // reader is already stopped.
                if (reader.QueryStatus().IsSingulating)
                {
                    reader.Stop();
                    msg = "Reader stopped..";
                }
            }
            catch (OctaneSdkException ex)
            {
                // An Octane SDK exception occurred. Handle it here.
                msg = ex.Message.ToString();
            }
            catch (Exception ex)
            {
                // A general exception occurred. Handle it here.
                msg = ex.Message.ToString();
            }
            return msg;
        }

        //AssetTag/getGetReadersData
        [HttpGet]
        public JsonResult getGetReadersData()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = db.mReaderSettups.Where(j => j.IsAction == true && j.OrgInfoId == orgId).Select(c => new { c.ReaderIP, c.ReaderNo }).Distinct().ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }


        //AssetTag/Create
        [HttpPost]
        public JsonResult Create(tAssetTag collection)
        {
            bool _Flag = false; string Message = string.Empty; string _redirectUrl = string.Empty;
            try
            {
                var UserName = Session["AppUserName"];
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);

                if (string.IsNullOrEmpty(collection.RFID))
                {
                    _Flag = false; Message = "RFID Should Not be Empty";
                }
                // else if (string.IsNullOrEmpty(collection.IteamName))
                // {
                //     _Flag = false; Message = "Enter Asset Name";
                // }
                else if (collection.mIteamMasterId == null || collection.mGroupMasterId == null || collection.mIteamTypeMasterId == null)
                {
                    _Flag = false; Message = "Missing master mandatory Data /Asset Information";
                }
                else if (string.IsNullOrEmpty(Convert.ToString(collection.Stock)) || string.IsNullOrEmpty(Convert.ToString(collection.mUnitMasterId)))
                {
                    _Flag = false; Message = "Missing Stock Information";
                }

                else if (string.IsNullOrEmpty(Convert.ToString(collection.PurchaseDate)))
                {
                    _Flag = false; Message = "Missing Purchase Date";
                }
                else
                {
                    if (db.tAssetTags.Any(o => o.RFID == collection.RFID && o.OrgInfoId == orgId && o.IsAction == true))
                    {
                        _Flag = false; Message = "Same RFID Alerdy Exist";
                    }
                    else
                    {
                        tAssetStockIn Sin = new tAssetStockIn();
                        tRFIDType Rtype = new tRFIDType();
                        String Uid = DateTime.Now.ToString().GetHashCode().ToString("x");
                        collection.UID = Uid;
                        collection.bStock = collection.Stock;
                        collection.OrgInfoId = orgId;
                        //                    
                        collection.CreatedDate = DateTime.Now; collection.CreatedBy = UserName.ToString(); collection.IsAction = true;
                        db.tAssetTags.Add(collection);
                        db.SaveChanges();

                        //
                        Sin.UID = Uid;
                        Sin.OrgInfoId = orgId;
                        Sin.tAssetTagId = collection.tAssetTagId;
                        Sin.RFID = collection.RFID;
                        Sin.Stock = collection.Stock;
                        Sin.bStock = collection.Stock;
                        Sin.CreatedDate = DateTime.Now; Sin.CreatedBy = UserName.ToString(); Sin.IsAction = true;
                        db.tAssetStockIns.Add(Sin);
                        db.SaveChanges();

                        //
                        Rtype.RerfrenceId = collection.tAssetTagId;
                        Rtype.RFID = collection.RFID;
                        Rtype.Name = collection.IteamName;
                        Rtype.LocationId = collection.mRoomMasterId;
                        Rtype.Type = false;
                        Rtype.IsAction = true;
                        db.tRFIDTypes.Add(Rtype);
                        db.SaveChanges();

                        NotifyMail.AddNotify(collection.IteamName, collection.SerialNo, UserName.ToString(), DateTime.Now, "New Asset Record Added");
                        _Flag = true; Message = "Asset Record Added Successfully"; _redirectUrl = "AssetTag";
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
                _Flag = false; Message = message;
            }
            return Json(new { Flag = _Flag, Message, redirectUrl = _redirectUrl }, JsonRequestBehavior.AllowGet);
        }


        // POST: AssetTag/Edit
        [HttpPost]
        public JsonResult Edit(tAssetTag collection)
        {
            bool _Flag = false; string Message = string.Empty;
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);

                if (string.IsNullOrEmpty(Convert.ToString(collection.tAssetTagId)))
                {
                    _Flag = false; Message = "Incorrect Data please refresh you application";
                }
                if (string.IsNullOrEmpty(collection.RFID))
                {
                    _Flag = false; Message = "RFID Should Not be Empty";
                }
                // else if (string.IsNullOrEmpty(collection.IteamName))
                // {
                //     _Flag = false; Message = "Enter Asset Name";
                // }
                else if (collection.mIteamMasterId == null || collection.mGroupMasterId == null || collection.mIteamTypeMasterId == null)
                {
                    _Flag = false; Message = "Missing master mandatory Data /Asset Information";
                }

                else if (string.IsNullOrEmpty(collection.PurchaseDate.ToString()))
                {
                    _Flag = false; Message = "Missing Purchase Date";
                }

                //else if (string.IsNullOrEmpty(Convert.ToString(collection.Stock)) || string.IsNullOrEmpty(Convert.ToString(collection.mUnitMasterId)))
                //{
                //    _Flag = false; Message = "Missing Stock Information";
                //}

                else
                {
                    if (db.tAssetTags.Any(o => o.tAssetTagId == collection.tAssetTagId && o.OrgInfoId == orgId))
                    {
                        tAssetTag EditObj = db.tAssetTags.Find(collection.tAssetTagId);

                        tAssetStockIn stockInObj = db.tAssetStockIns.FirstOrDefault(x => x.tAssetTagId == collection.tAssetTagId);

                        EditObj.mIteamMasterId = collection.mIteamMasterId;
                        EditObj.IteamName = collection.IteamName;
                        EditObj.BarCode = collection.BarCode;
                        EditObj.RFID = collection.RFID;
                        EditObj.IteamDescription = collection.IteamDescription;

                        EditObj.Model = collection.Model;
                        EditObj.ModelNo = collection.ModelNo;
                        EditObj.SerialNo = collection.SerialNo;
                        EditObj.Manufacturer = collection.Manufacturer;
                        EditObj.PurchaseCost = collection.PurchaseCost;
                        EditObj.mVendorId = collection.mVendorId;

                        EditObj.mUnitMasterId = collection.mUnitMasterId;
                        EditObj.mGroupMasterId = collection.mGroupMasterId;
                        EditObj.mIteamTypeMasterId = collection.mIteamTypeMasterId;

                        EditObj.Depreciation = collection.Depreciation;
                        EditObj.Receivedby = collection.Receivedby;
                        EditObj.DefaultWarranty = collection.DefaultWarranty;



                        //EditObj.mSiteMasterId = collection.mSiteMasterId;
                        //EditObj.mZoneId = collection.mZoneId;
                        //EditObj.mFloorMasterId = collection.mFloorMasterId;
                        //EditObj.mRoomMasterId = collection.mRoomMasterId;

                        EditObj.mSiteMasterId = collection.mSiteMasterId;
                        EditObj.mZoneId = collection.mZoneId;
                        EditObj.mFloorMasterId = collection.mFloorMasterId;
                        EditObj.mRoomMasterId = collection.mRoomMasterId;

                        EditObj.PurchaseDate = collection.PurchaseDate;
                        EditObj.SlNo = collection.SlNo;
                        EditObj.BLETAGNo = collection.BLETAGNo;
                        EditObj.BatteryLevel = collection.BatteryLevel;
                        EditObj.Length = collection.Length;
                        EditObj.Width = collection.Width;
                        EditObj.Height = collection.Height;
                        EditObj.Weight = collection.Weight;
                        EditObj.SetNo = collection.SetNo;
                        EditObj.PlantName = collection.PlantName;
                        EditObj.OwnerDepartment = collection.OwnerDepartment;
                        EditObj.POC = collection.POC;
                        EditObj.Program = collection.Program;
                        EditObj.Module = collection.Module;
                        EditObj.BuildingName = collection.BuildingName;
                        EditObj.StorageLocation = collection.StorageLocation;
                        EditObj.Number = collection.Number;
                        EditObj.SubCategory1 = collection.SubCategory1;
                        EditObj.SubCategory2 = collection.SubCategory2;
                        EditObj.Material = collection.Material;
                        EditObj.CurrentLocation = collection.CurrentLocation;
                        EditObj.PhaseInDate = collection.PhaseInDate;
                        EditObj.PhaseOutDate = collection.PhaseOutDate;
                        EditObj.CalibrationDate = collection.CalibrationDate;
                        EditObj.PreventiveMaintenanceDate = collection.PreventiveMaintenanceDate;
                        EditObj.PreventiveMaintenanceDueDate = collection.PreventiveMaintenanceDueDate;
                        
                        // MIS Information fields
                        EditObj.WorkOrderNumber = collection.WorkOrderNumber;
                        EditObj.MISNumber = collection.MISNumber;
                        EditObj.PartNumber = collection.PartNumber;
                        EditObj.PartName = collection.PartName;
                        EditObj.LaunchDate = collection.LaunchDate;
                        EditObj.EndDate = collection.EndDate;

                        EditObj.OrgInfoId = orgId; EditObj.ModifiedBy = UserName.ToString();
                        EditObj.ModifiedDate = DateTime.Now;
                        db.Entry(EditObj).State = EntityState.Modified;
                        db.SaveChanges();

                        stockInObj.OrgInfoId = orgId; stockInObj.ModifiedBy = UserName.ToString();
                        stockInObj.ModifiedDate = DateTime.Now;
                        db.Entry(EditObj).State = EntityState.Modified;
                        db.SaveChanges();

                        NotifyMail.ModifyNotify(collection.IteamName, collection.SerialNo, UserName.ToString(), DateTime.Now, "Asset Record Modified");
                        _Flag = true; Message = "Asset Record Updated Successfully";
                    }
                    else
                    {
                        _Flag = false; Message = "Record Not Found ";
                    }
                }

            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }

            return Json(new { Flag = _Flag, Message }, JsonRequestBehavior.AllowGet);
        }

        // GET: AssetTag/Edit/5
        [HttpGet]
        public JsonResult Edit(int id)
        {
            bool _Flag = false; string Message = string.Empty;
            if (string.IsNullOrEmpty(Convert.ToString(id)))
            {
                _Flag = false; Message = "Id Could Not Found";
                return Json(new { Flag = _Flag, Message }, JsonRequestBehavior.AllowGet);
            }
            var EditData = db.tAssetTags.Find(id);
            if (EditData == null)
            {
                _Flag = false; Message = "Data Could Not Found";
                return Json(new { Flag = _Flag, Message }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                _Flag = true; Message = "Successfully";
                return Json(new { Flag = _Flag, Message, Idata = EditData }, JsonRequestBehavior.AllowGet);
            }
        }

        //
        [HttpGet]
        public JsonResult getWarehousesData()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = (from Dis in db.mWarehouseMasters
                           where (Dis.OrgInfoId == orgId && Dis.IsAction == true)
                           select new { Dis.mWarehouseMasterId, Dis.WarehouseName }).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //
        [HttpGet]
        public JsonResult getLocatorsData(int WarehId)
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = (from Dis in db.mLocatorMasters
                           where (Dis.OrgInfoId == orgId && Dis.IsAction == true && Dis.mWarehouseMasterId == WarehId)
                           select new { Dis.mLocatorMasterId, Dis.RackNo }).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //
        [HttpGet]
        public JsonResult getLoadData()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = db.TC_LeadType.Where(o => o.OrgInfoId == orgId).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //Get All DAta
        [HttpGet]
        public JsonResult getData()
        {
            var UserName = Session["AppUserName"];
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = (from tAs in db.tAssetTags
                           join Im in db.mIteamMasters on tAs.mIteamMasterId equals Im.mIteamMasterId into Im
                           join Um in db.mUnitMasters on tAs.mUnitMasterId equals Um.mUnitMasterId into Um
                           join Gm in db.mGroupMasters on tAs.mGroupMasterId equals Gm.mGroupMasterId into Gm
                           join Itm in db.mIteamTypeMasters on tAs.mIteamTypeMasterId equals Itm.mIteamTypeMasterId into Itm
                           join Rm in db.mRoomMasters on tAs.mRoomMasterId equals Rm.mRoomMasterId into Rm
                           join Fm in db.mFloorMasters on tAs.mFloorMasterId equals Fm.mFloorMasterId into Fm
                           //join Lm in db.mLocatorMasters on tAs.mLocatorMasterId equals Lm.mLocatorMasterId
                           //join Wr in db.mWarehouseMasters on Lm.mWarehouseMasterId equals Wr.mWarehouseMasterId
                           where (tAs.OrgInfoId == orgId && tAs.IsAction == true)

                           from _Im in Im.DefaultIfEmpty()
                           from _Um in Um.DefaultIfEmpty()
                           from _Gm in Gm.DefaultIfEmpty()
                           from _Itm in Itm.DefaultIfEmpty()
                           from _Rm in Rm.DefaultIfEmpty()
                           from _Fm in Fm.DefaultIfEmpty()
                           select new
                           {
                               tAs.Model,
                               tAs.ModelNo,
                               tAs.InvNo,
                               tAs.BarCode,
                               tAs.tAssetTagId,
                               tAs.IteamName,
                               tAs.IteamCode,
                               tAs.IteamDescription,
                               tAs.UID,
                               tAs.RFID,
                               tAs.Stock,
                               tAs.bStock,
                               AssetName = _Im.IteamName,
                               _Um.UnitName,
                               _Gm.GroupName,
                               _Itm.IteamType,
                               _Fm.FloorName,
                               _Fm.FloorNo,
                               _Rm.RoomName,
                               _Rm.RoomNo
                           }).ToList();
            //Convert List Data to The Json Array       
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //Save       
        public string SaveData(String JsonD)
        {
            var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            string msg = "";
            try
            {
                var objd = JsonConvert.DeserializeObject<tAssetTag>(JsonD);

                if (objd.RFID == null || objd.RFID == "")
                {
                    return msg = "RFID SHOULD NOT BE NULL";
                }
                if (db.tAssetTags.Any(o => o.RFID == objd.RFID && o.IsAction == true))
                {
                    return msg = "Same Name Alerdy Exist";
                }
                else
                {
                    tAssetStockIn Sin = new tAssetStockIn();
                    tRFIDType Rtype = new tRFIDType();
                    String Uid = DateTime.Now.ToString().GetHashCode().ToString("x");
                    objd.UID = Uid;
                    objd.bStock = objd.Stock;
                    objd.OrgInfoId = orgId;
                    //                    
                    objd.CreatedDate = DateTime.Now; objd.CreatedBy = UserName.ToString(); objd.IsAction = true;
                    db.tAssetTags.Add(objd);
                    db.SaveChanges();

                    //
                    Sin.UID = Uid;
                    Sin.OrgInfoId = orgId;
                    Sin.tAssetTagId = objd.tAssetTagId;
                    Sin.RFID = objd.RFID;
                    Sin.Stock = objd.Stock;
                    Sin.bStock = objd.Stock;
                    Sin.CreatedDate = DateTime.Now; Sin.CreatedBy = UserName.ToString(); Sin.IsAction = true;
                    db.tAssetStockIns.Add(Sin);
                    db.SaveChanges();

                    //
                    Rtype.RerfrenceId = objd.tAssetTagId;
                    Rtype.RFID = objd.RFID;
                    Rtype.Name = objd.IteamName;
                    Rtype.LocationId = objd.mRoomMasterId;
                    Rtype.Type = false;
                    Rtype.IsAction = true;
                    db.tRFIDTypes.Add(Rtype);
                    db.SaveChanges();

                    msg = "Data Saved";
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message.ToString();
            }
            return msg;
        }

        //
        [HttpGet]
        public string DeleteData(int ID)
        {
            string msg = "";

            int orgId = Convert.ToInt32(Session["OrgInfoId"]); var UserName = Session["AppUserName"];
            var original = db.tAssetTags.FirstOrDefault(b => b.tAssetTagId == ID);
            if (original != null)
            {
                var tAss = db.tAssetStockIns.FirstOrDefault(b => b.tAssetTagId == ID);
                tAss.IsAction = false;
                tAss.ModifiedBy = UserName.ToString();
                tAss.ModifiedDate = DateTime.Now;
                //
                original.IsAction = false;
                original.ModifiedBy = UserName.ToString();
                original.ModifiedDate = DateTime.Now;
                //
                //var rType = db.tRFIDTypes.FirstOrDefault(b => b.RerfrenceId == ID);
                //rType.IsAction = false;

                db.SaveChanges();
                //
                NotifyMail.DeleteNotify(original.IteamName, original.SerialNo, UserName.ToString(), DateTime.Now, "Asset Record Deleted");
                msg = "Data Deleted";
            }
            else
            {
                return msg = "Data is Not Deleted";
            }
            return msg;
        }

        //Update QTY
        [HttpGet]
        public JsonResult setQTYData(int? tAssetTagId, string UID, decimal? qty)
        {
            bool _Flag = false; string Message = string.Empty;

            if (tAssetTagId == null || UID == null || qty == null)
            {
                Message = "you are missing data"; _Flag = false;
            }
            else
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]); var UserName = Session["AppUserName"];
                var original = db.tAssetTags.FirstOrDefault(b => b.tAssetTagId == tAssetTagId && b.UID == UID && b.IsAction == true);
                if (original != null)
                {
                    tAssetStockIn Sin = new tAssetStockIn();

                    original.Stock = (qty);
                    original.bStock = (original.bStock + qty);
                    original.ModifiedBy = UserName.ToString();
                    original.ModifiedDate = DateTime.Now;
                    db.SaveChanges();

                    //
                    Sin.UID = original.UID;
                    Sin.OrgInfoId = original.OrgInfoId;
                    Sin.tAssetTagId = original.tAssetTagId;
                    Sin.RFID = original.RFID;

                    Sin.Stock = qty;
                    Sin.bStock = original.bStock;

                    Sin.CreatedDate = DateTime.Now; Sin.CreatedBy = UserName.ToString(); Sin.IsAction = true;
                    db.tAssetStockIns.Add(Sin);
                    db.SaveChanges();
                    Message = "Data Updated Successfully"; _Flag = true;
                }
                else
                {
                    Message = "Data is Not updated"; _Flag = false;
                }
            }
            return Json(new { Flag = _Flag, Message }, JsonRequestBehavior.AllowGet);
        }


        // GET: AssetTag/Edit/5
        [HttpGet]
        public ActionResult CarryParam(string id)
        {
            TempData["CarryParam"] = id;
            return RedirectToAction("", "MoreAssetDetail");
        }

        // GET: AssetTag/GetStatistics
        [HttpGet]
        public JsonResult GetStatistics()
        {
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                
                var totalAssets = db.tAssetTags.Count(x => x.OrgInfoId == orgId && x.IsAction == true);
                var activeAssets = db.tAssetTags.Count(x => x.OrgInfoId == orgId && x.IsAction == true);
                var pendingAssets = db.tAssetTags.Count(x => x.OrgInfoId == orgId && x.IsAction == true);
                var underMaintenance = db.tAssetTags.Count(x => x.OrgInfoId == orgId && x.IsAction == true);

                return Json(new { Total = totalAssets, Active = activeAssets, Pending = pendingAssets, UnderMaintenance = underMaintenance }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Total = 0, Active = 0, Pending = 0, UnderMaintenance = 0 }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}

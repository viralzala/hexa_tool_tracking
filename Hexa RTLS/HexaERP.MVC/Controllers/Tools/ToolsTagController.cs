using HexaERP.MVC.Models;
using Impinj.OctaneSdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.Tools
{
    public class ToolsTagController : Controller
    {
        //******************
        //Author: Mudassar I
        //Date: 24/02/2017
        //
        //******************
        // Create an instance of the ImpinjReader class.
        static ImpinjReader reader = new ImpinjReader();

        // Create a Dictionary to store the tags we've read.
        static Dictionary<string, Tag> tagsRead = new Dictionary<string, Tag>();
        static List<entitys> tags = new List<entitys>();

        private ERPdbEntities db = new ERPdbEntities();
        // GET: 
        public ActionResult Index()
        {
            return View();
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
                    // Add this tag to the list of tags we've read.
                    tagsRead.Add(tag.Epc.ToString(), tag);
                    on.RFID = tag.Epc.ToString(); on.PORTID = tag.AntennaPortNumber;
                    tags.Add(on);
                    GetIds();
                }
            }

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
        public class entitys
        {
            public string RFID;
            public int PORTID;
        }
        //
        [HttpGet]
        public JsonResult getGetReadersData()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = (from Dis in db.mReaders
                           where (Dis.OrgInfoId == orgId && Dis.IsAction == true && Dis.ReaderIP != null)
                           select new { Dis.ReaderIP }).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
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
        public JsonResult getGetUnitsData()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = (from Dis in db.mUnitMasters
                           where (Dis.OrgInfoId == orgId && Dis.IsAction == true)
                           select new { Dis.mUnitMasterId, Dis.UnitName }).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //
        [HttpGet]
        public JsonResult getIteamsData()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = (from Dis in db.mIteamMasters
                           where (Dis.OrgInfoId == orgId && Dis.IsAction == true)
                           select new { Dis.mIteamMasterId, Dis.IteamName, Dis.IteamCode }).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //
        [HttpGet]
        public JsonResult getGroupsData()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = (from Dis in db.mGroupMasters
                           where (Dis.OrgInfoId == orgId && Dis.IsAction == true)
                           select new { Dis.mGroupMasterId, Dis.GroupName }).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //
        [HttpGet]
        public JsonResult getTypesData()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = (from Dis in db.mIteamTypeMasters
                           where (Dis.OrgInfoId == orgId && Dis.IsAction == true)
                           select new { Dis.mIteamTypeMasterId, Dis.IteamType }).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
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
            var ObjData = (from Tooltag in db.toTooltags
                           join Rm in db.mRoomMasters on Tooltag.mRoomMasterId equals Rm.mRoomMasterId into RMTooltag
                           join Um in db.mUnitMasters on Tooltag.mUnitMasterId equals Um.mUnitMasterId into UmTooltag
                           join Itm in db.mIteamTypeMasters on Tooltag.mIteamTypeMasterId equals Itm.mIteamTypeMasterId into ItmTooltag
                           join Sup in db.toSuppliers on Tooltag.toSupplierId equals Sup.toSupplierId into SupTooltag
                           where (Tooltag.OrgInfoId == orgId && Tooltag.IsAction == true)

                           from Rm in RMTooltag.DefaultIfEmpty()
                           from Um in UmTooltag.DefaultIfEmpty()
                           from Itm in ItmTooltag.DefaultIfEmpty()
                           from Sup in SupTooltag.DefaultIfEmpty()

                           select new
                           {
                               Tooltag.toTooltagId,
                               Tooltag.ToolName,
                               Tooltag.Code,
                               Tooltag.RFID,
                               Tooltag.UID,
                               Tooltag.Manufacturer,
                               Tooltag.Model,
                               Tooltag.ModelNo,
                               Tooltag.Serial,
                               Tooltag.Description,
                               Tooltag.Condition,
                               Tooltag.IteamStatus,
                               Tooltag.drawer,
                               Tooltag.drawerrack,
                               Tooltag.Size,

                               Rm.RoomName,
                               Rm.RoomNo,
                               Um.UnitName,
                               Itm.IteamType,

                               Tooltag.Price,
                               Tooltag.Stock,
                               Tooltag.bStock,
                               Sup.SupplierName

                           }).ToList();

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
                var objd = JsonConvert.DeserializeObject<toTooltag>(JsonD);

                if (objd.RFID == null || objd.RFID == "")
                {
                    return msg = "RFID SHOULD NOT BE NULL";
                }
                else if (objd.ToolName == null || objd.ToolName == "" || objd.mRoomMasterId == null)
                {
                    return msg = "Fill Mandatory filds";
                }
                if (db.toTooltags.Any(o => o.RFID == objd.RFID && o.IsAction == true))
                {
                    return msg = "Alerdy Exist";
                }
                else
                {
                    toToolStockIn Sin = new toToolStockIn();
                    toRFIDType Rtype = new toRFIDType();
                    String Uid = DateTime.Now.ToString().GetHashCode().ToString("x");
                    objd.UID = Uid;
                    objd.bStock = objd.Stock;
                    objd.OrgInfoId = orgId;

                    //Tag Tools With RFID     
                    objd.CreatedDate = DateTime.Now; objd.CreatedBy = UserName.ToString(); objd.IsAction = true;
                    db.toTooltags.Add(objd);
                    db.SaveChanges();

                    //Stock In Record
                    Sin.UID = Uid;
                    Sin.OrgInfoId = orgId;
                    Sin.toTooltagId = objd.toTooltagId;
                    Sin.RFID = objd.RFID;
                    Sin.Stock = objd.Stock;
                    Sin.bStock = objd.Stock;
                    Sin.CreatedDate = DateTime.Now; Sin.CreatedBy = UserName.ToString(); Sin.IsAction = true;
                    db.toToolStockIns.Add(Sin);
                    db.SaveChanges();

                    //RFID Type Data
                    Rtype.RerfrenceId = objd.toTooltagId;
                    Rtype.RFID = objd.RFID;
                    Rtype.Name = objd.ToolName;
                    Rtype.LocationId = objd.mRoomMasterId;

                    Rtype.Type = false;
                    Rtype.IsAction = true;
                    db.toRFIDTypes.Add(Rtype);
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
        //Get Dept By Id
        [HttpGet]
        public JsonResult getDataWithId(int ID)
        {
            var datas = db.mRoomMasters.Where(o => o.mRoomMasterId == ID).ToList();
            return Json(datas, JsonRequestBehavior.AllowGet);
        }
        ////Update Department
        //[HttpGet]
        //public string UpdateData(int _FloorId, string RoomName, int RoomNo, int ID)
        //{
        //    string msg = "";
        //    int orgId = Convert.ToInt32(Session["OrgInfoId"]); var UserName = Session["AppUserName"];
        //    var original = db.mRoomMasters.FirstOrDefault(b => b.mRoomMasterId == ID);
        //    if (original != null)
        //    {
        //        original.mFloorMasterId = _FloorId;
        //        original.RoomName = RoomName;
        //        original.RoomNo = RoomNo;
        //        original.ModifiedBy = UserName.ToString();
        //        original.ModifiedDate = DateTime.Now;
        //        db.SaveChanges();
        //        msg = "Data Updated";
        //    }
        //    else
        //    {
        //        return msg = "Data is Not updated";
        //    }
        //    return msg;
        //}
        //Delete Department
        [HttpGet]
        public string DeleteData(int ID)
        {
            string msg = "";

            int orgId = Convert.ToInt32(Session["OrgInfoId"]); var UserName = Session["AppUserName"];
            var original = db.toTooltags.FirstOrDefault(b => b.toTooltagId == ID);
            if (original != null)
            {
                var tAss = db.toToolStockIns.FirstOrDefault(b => b.toTooltagId == ID);
                if (tAss != null)
                {
                    tAss.IsAction = false;
                    tAss.ModifiedBy = UserName.ToString();
                    tAss.ModifiedDate = DateTime.Now;
                }

                //
                original.IsAction = false;
                original.ModifiedBy = UserName.ToString();
                original.ModifiedDate = DateTime.Now;

                //
                var rType = db.toRFIDTypes.FirstOrDefault(b => b.RerfrenceId == ID);
                rType.IsAction = false;

                db.SaveChanges();
                //


                msg = "Data Deleted";
            }
            else
            {
                return msg = "Data is Not Deleted";
            }
            return msg;
        }
        //
        [HttpGet]
        public JsonResult getGetFloorsData()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = (from Dis in db.mFloorMasters
                           where (Dis.OrgInfoId == orgId && Dis.IsAction == true)
                           select new { Dis.mFloorMasterId, Dis.FloorName }).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //
        [HttpGet]
        public JsonResult getGetRoomsData(int FloorId)
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = (from Dis in db.mRoomMasters
                           where (Dis.OrgInfoId == orgId && Dis.mFloorMasterId == FloorId && Dis.IsAction == true)
                           select new { Dis.mRoomMasterId, Dis.RoomName }).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //
        //Update Department
        [HttpGet]
        public string setQTYData(int toTooltagId, string UID, string RFID, decimal qty)
        {
            string msg = "";

            if (string.IsNullOrEmpty(Convert.ToString(qty)) || string.IsNullOrEmpty(RFID.Trim()) || string.IsNullOrEmpty(Convert.ToString(toTooltagId)) || qty <= 0)
            {
                return msg = "Enter Quatity";
            }
            int orgId = Convert.ToInt32(Session["OrgInfoId"]); var UserName = Session["AppUserName"];
            var original = db.toTooltags.FirstOrDefault(b => b.toTooltagId == toTooltagId && b.UID == UID && b.RFID == RFID && b.IsAction == true);
            if (original != null)
            {
                toToolStockIn Sin = new toToolStockIn();

                original.Stock = (qty);
                original.bStock = (original.bStock + qty);
                original.ModifiedBy = UserName.ToString();
                original.ModifiedDate = DateTime.Now;
                db.SaveChanges();

                //
                Sin.UID = original.UID;
                Sin.OrgInfoId = original.OrgInfoId;
                Sin.toTooltagId = original.toTooltagId;
                Sin.RFID = original.RFID;

                Sin.Stock = qty;
                Sin.bStock = original.bStock;

                Sin.CreatedDate = DateTime.Now; Sin.CreatedBy = UserName.ToString(); Sin.IsAction = true;
                db.toToolStockIns.Add(Sin);
                db.SaveChanges();

                msg = "Data Updated";
            }
            else
            {
                return msg = "Data is Not updated";
            }
            return msg;
        }
    }
}
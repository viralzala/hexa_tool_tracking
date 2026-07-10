using HexaERP.MVC.EmailConfig;
using HexaERP.MVC.Hubs;
using HexaERP.MVC.Models;
using HexaERP.Services.ActiveDirectory;
using Impinj.OctaneSdk;
using J_RFID;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;


namespace HexaERP.MVC.Controllers.RFID
{
    public class EmployeeTagController : Controller
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
        private const string SPLIT_1 = "\\";
        //******************
        //Author: Mudassar I
        //Date: 02/03/2017
        //
        //******************
        // Create an instance of the ImpinjReader class.
        static ImpinjReader reader = new ImpinjReader();

        // Create a Dictionary to store the tags we've read.
        static Dictionary<string, Tag> tagsRead = new Dictionary<string, Tag>();
        static List<entitys> tags = new List<entitys>();

        ActiveDirectoryHelper adh = new ActiveDirectoryHelper();
        // GET: 
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

                if (Session["UniqueId"].ToString() != "" && Session["OrgInfoId"].ToString() != "" && Session["AppUserName"].ToString() != "")
                {
                    //string Page_Name = Path.GetFileName(Request.Path);
                    if (Convert.ToString(Session["SortCode"]) != "AD")
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
            var ObjData = db.mReaderSettups.Where(j => j.IsAction == true && j.OrgInfoId == orgId).Select(c => new { c.ReaderIP, c.ReaderNo }).Distinct().ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }

        //EmployeeTag/Create
        [HttpPost]
        public JsonResult Create(tEmployeeTag collection)
        {
            bool _Flag = false; string Message = string.Empty; string _redirectUrl = string.Empty;
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);

                if (string.IsNullOrEmpty(collection.RFID))
                {
                    _Flag = false; Message = "RFID Should Not be Empty";
                }
                else if (string.IsNullOrEmpty(collection.EmployeeName))
                {
                    _Flag = false; Message = "Enter Employee Name";
                }
                else if (collection.mAgencyId == null || collection.mDesignationId == null || collection.mSkillCategoryId == null)
                {
                    _Flag = false; Message = "Missing master mandatory Data";
                }

                //else if (collection.mSiteMasterId == null || collection.mZoneId == null || collection.mFloorMasterId == null || collection.mRoomMasterId == null)
                //{
                //    _Flag = false; Message = "Missing Work Loaction Data";
                //}

                //else if (string.IsNullOrEmpty(collection.ContactNo))
                //{
                //    _Flag = false; Message = "Contact Number required";
                //}

                else
                {
                    if (db.tEmployeeTags.Any(o => o.RFID == collection.RFID && o.OrgInfoId == orgId && o.IsAction == true))
                    {
                        _Flag = false; Message = "Same RFID Alerdy Exist";
                    }
                    else
                    {
                        collection.IsAction = true;
                        collection.OrgInfoId = orgId;
                        collection.CreatedBy = UserName.ToString();
                        collection.CreatedDate = DateTime.Now;
                        collection.ModifiedDate = DateTime.Now;
                        db.tEmployeeTags.Add(collection);
                        db.SaveChanges();
                        NotifyMail.AddNotify(collection.EmployeeName, collection.EmployeeId, UserName.ToString(), DateTime.Now, "New Employee Record Added");
                        _Flag = true; Message = "Employee Record Added Successfully"; _redirectUrl = "EmployeeTag";

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
            return Json(new { Flag = _Flag, Message = Message, redirectUrl = _redirectUrl }, JsonRequestBehavior.AllowGet);
        }




        // GET: EmployeeTag/Edit/5
        [HttpGet]
        public JsonResult Edit(int id)
        {
            bool _Flag = false; string Message = string.Empty;
            if (string.IsNullOrEmpty(Convert.ToString(id)))
            {
                _Flag = false; Message = "Id Could Not Found";
                return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
            }

            var EditData = db.tEmployeeTags.Find(id);

            if (EditData == null)
            {
                _Flag = false; Message = "Data Could Not Found";
                return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                _Flag = true; Message = "Successfully";
                return Json(new { Flag = _Flag, Message = Message, Idata = EditData }, JsonRequestBehavior.AllowGet);
            }
        }
        //Save       
        public string SaveData(String JsonD)
        {
            var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            string msg = "";
            try
            {

                var objd = JsonConvert.DeserializeObject<tEmployeeTag>(JsonD);
                if (objd.RFID == null || objd.RFID == "")
                {
                    return msg = "RFID SHOULD NOT BE NULL";
                }
                if (db.tEmployeeTags.Any(o => o.RFID == objd.RFID && o.IsAction == true))
                {
                    return msg = "Same Name Alerdy Exist";
                }
                else
                {

                    tRFIDType Rtype = new tRFIDType();
                    //String Uid = DateTime.Now.ToString().GetHashCode().ToString("x");                   
                    //                    
                    objd.CreatedDate = DateTime.Now; objd.CreatedBy = UserName.ToString(); objd.OrgInfoId = orgId; objd.IsAction = true;
                    db.tEmployeeTags.Add(objd);
                    db.SaveChanges();

                    //
                    Rtype.RerfrenceId = objd.tEmployeeTagId;
                    Rtype.RFID = objd.RFID;
                    Rtype.Name = objd.EmployeeName;
                    Rtype.LocationId = objd.mRoomMasterId;
                    Rtype.Type = true;
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
        //Get All DAta
        [HttpGet]
        public JsonResult getData()
        {
            var UserName = Session["AppUserName"];
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = (from tAs in db.tEmployeeTags
                           join ag in db.mAgencies on tAs.mAgencyId equals ag.mAgencyId into _ag

                           join ds in db.mDesignations on tAs.mDesignationId equals ds.mDesignationId into _ds
                           join sk in db.mSkillCategories on tAs.mSkillCategoryId equals sk.mSkillCategoryId into _sk
                           join wk in db.mWorkCategories on tAs.mWorkCategoryId equals wk.mWorkCategoryId into _wk
                           join ac in db.mActivities on tAs.mActivityId equals ac.mActivityId into _ac
                           join shf in db.mShifts on tAs.mShiftId equals shf.mShiftId into _shf

                           join sit in db.mSiteMasters on tAs.mSiteMasterId equals sit.mSiteMasterId into _sit
                           join zon in db.mZones on tAs.mZoneId equals zon.mZoneId into _zon
                           join subzon in db.mFloorMasters on tAs.mFloorMasterId equals subzon.mFloorMasterId into _subzon

                           where (tAs.OrgInfoId == orgId && tAs.IsAction == true)

                           from _agd in _ag.DefaultIfEmpty()
                           from _dsd in _ds.DefaultIfEmpty()
                           from _skd in _sk.DefaultIfEmpty()
                           from _wkd in _wk.DefaultIfEmpty()
                           from _acd in _ac.DefaultIfEmpty()
                           from _shfd in _shf.DefaultIfEmpty()
                           from _sitd in _sit.DefaultIfEmpty()
                           from _zond in _zon.DefaultIfEmpty()
                           from _subzond in _subzon.DefaultIfEmpty()

                           select new
                           {
                               tAs.tEmployeeTagId,
                               tAs.RFID,
                               tAs.EmployeeName,
                               EmployeeId = tAs.EmployeeId ?? string.Empty,
                               Agency = _agd.Agency ?? string.Empty,
                               Designation = _dsd.Designation ?? string.Empty,
                               SkillCategory = _skd.SkillCategory ?? string.Empty,
                               Activity = _acd.Activity ?? string.Empty,
                               Shift = _shfd.Shift ?? string.Empty,
                               Site = _sitd.Site ?? string.Empty,
                               Zone = _zond.Zone ?? string.Empty,
                               FloorName = _subzond.FloorName ?? string.Empty

                           }).ToList();
            //Convert List Data to The Json Array       
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }

        // GET: EmployeeTag/Delete/5
        [HttpGet]
        public JsonResult Delete(int id)
        {
            bool _Flag = false; string Message = string.Empty;
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                if (string.IsNullOrEmpty(Convert.ToString(id)))
                {
                    _Flag = false; Message = "Error While Deleting Record";
                }
                else
                {
                    tEmployeeTag delObj = db.tEmployeeTags.Find(id);
                    if (delObj == null)
                    {
                        _Flag = false; Message = "Data Not Found";
                    }
                    else
                    {
                        db.tEmployeeTags.Remove(delObj);
                        db.SaveChanges();
                        NotifyMail.AddNotify(delObj.EmployeeName, delObj.EmployeeId, UserName.ToString(), DateTime.Now, "Employee Record Deleted");
                        _Flag = true; Message = "Record Deleted Successfully :" + delObj.EmployeeName;
                    }
                }
            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }

        //Get All Master
        [HttpGet]
        public JsonResult getMasterData()
        {
            var UserName = Session["AppUserName"];
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var mAgency = db.mAgencies.Where(x => x.IsAction == true && x.OrgInfoId == orgId).ToList();
            var mDesignation = db.mDesignations.Where(x => x.IsAction == true && x.OrgInfoId == orgId).ToList();
            var mSkillCategory = db.mSkillCategories.Where(x => x.IsAction == true && x.OrgInfoId == orgId).ToList();
            var mWorkCategory = db.mWorkCategories.Where(x => x.IsAction == true && x.OrgInfoId == orgId).ToList();

            var mActivity = db.mActivities.Where(x => x.IsAction == true && x.OrgInfoId == orgId).ToList();
            var mShift = db.mShifts.Where(x => x.IsAction == true && x.OrgInfoId == orgId).ToList();

            var mSite = db.mSiteMasters.Where(x => x.IsAction == true && x.OrgInfoId == orgId).ToList();
            //Convert List Data to The Json Array       
            //Convert List Data to The Json Array          
            return Json(new { mAgency, mDesignation, mSkillCategory, mWorkCategory, mActivity, mShift, mSite }, JsonRequestBehavior.AllowGet);
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

        // GET: EmployeeTag/getZones
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
        // GET: EmployeeTag/getSubZones
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
        // GET: EmployeeTag/Area
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

        // POST: EmployeeTag/Edit/5
        [HttpPost]
        public JsonResult Edit(tEmployeeTag collection)
        {
            bool _Flag = false; string Message = string.Empty;
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);

                if (string.IsNullOrEmpty(Convert.ToString(collection.tEmployeeTagId)))
                {
                    _Flag = false; Message = "Incorrect Data please refresh you application";
                }
                if (string.IsNullOrEmpty(collection.EmployeeName) | string.IsNullOrEmpty(collection.RFID))
                {
                    _Flag = false; Message = "You are missing employee basic details";
                }
                else
                {

                    if (db.tEmployeeTags.Any(o => o.tEmployeeTagId == collection.tEmployeeTagId && o.OrgInfoId == orgId))
                    {

                        tEmployeeTag EditObj = db.tEmployeeTags.Find(collection.tEmployeeTagId);

                        EditObj.EmployeeName = collection.EmployeeName;
                        EditObj.tEmployeeTagId = collection.tEmployeeTagId;
                        EditObj.EmployeeId = collection.EmployeeId;
                        EditObj.RFID = collection.RFID;

                        EditObj.EmailId = collection.EmailId;
                        EditObj.ContactNo = collection.ContactNo;
                        //EditObj.Address = collection.Address;

                        EditObj.mAgencyId = collection.mAgencyId;
                        EditObj.mDesignationId = collection.mDesignationId;
                        EditObj.mSkillCategoryId = collection.mSkillCategoryId;
                        EditObj.mWorkCategoryId = collection.mWorkCategoryId;
                        EditObj.mActivityId = collection.mActivityId;
                        EditObj.mShiftId = collection.mShiftId;

                        EditObj.mSiteMasterId = collection.mSiteMasterId;
                        EditObj.mZoneId = collection.mZoneId;
                        EditObj.mFloorMasterId = collection.mFloorMasterId;
                        EditObj.mRoomMasterId = collection.mRoomMasterId;

                        EditObj.OrgInfoId = orgId; EditObj.ModifiedBy = UserName.ToString();
                        EditObj.ModifiedDate = DateTime.Now;
                        db.Entry(EditObj).State = EntityState.Modified;
                        db.SaveChanges();
                        NotifyMail.AddNotify(EditObj.EmployeeName, EditObj.EmployeeId, UserName.ToString(), DateTime.Now, "Employee Record Modified");
                        _Flag = true; Message = "Employee Record Updated Successfully";
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
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult getADusers()
        {
            JsonResult _json = new JsonResult();
            string _redir = string.Empty, _Domain = string.Empty, _UserName = string.Empty;
            try
            {
                string _userSes = Convert.ToString(Session["adUser"]);
                string _adPass = Convert.ToString(Session["adpass"]);

                if (string.IsNullOrEmpty(_userSes) || string.IsNullOrEmpty(_adPass))
                    return Json(new { Message = "Null AD", False = false }, JsonRequestBehavior.AllowGet);

                if (_userSes.IndexOf("\\") != -1)
                {
                    string[] arrT = _userSes.Split(SPLIT_1[0]);
                    _Domain = arrT[0];
                    _UserName = arrT[1];
                }
                PrincipalContext _conte = new PrincipalContext(ContextType.Domain, _Domain, _UserName, _adPass);
                //PrincipalSearcher ffff = new PrincipalSearcher(new ComputerPrincipal(_conte));

                PrincipalSearcher _searcher = new PrincipalSearcher(new UserPrincipal(_conte));
                var _adusers = _searcher.FindAll().Select(p => p as UserPrincipal).Select(x => new { x.Name, x.DisplayName, x.EmailAddress, x.EmployeeId, x.GivenName, x.MiddleName, x.SamAccountName, x.Surname, x.VoiceTelephoneNumber }).ToList();
                var newJson = JsonConvert.SerializeObject(_adusers.AsQueryable());
                System.IO.File.WriteAllText(Server.MapPath("~/Content/ADusers.json"), newJson);
                _json = this.Json(new { User = _adusers.ToArray(), Message = "Successfully", False = true }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex) { _json = this.Json(new { Message = ex.Message, False = false }, JsonRequestBehavior.AllowGet); }

            return _json;
        }


        private static string GetProperty(SearchResult searchResult, string PropertyName)
        {
            if (searchResult.Properties.Contains(PropertyName))
            {
                return searchResult.Properties[PropertyName][0].ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
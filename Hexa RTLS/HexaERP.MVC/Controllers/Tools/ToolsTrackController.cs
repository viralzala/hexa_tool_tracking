using HexaERP.MVC.Models;
using Impinj.OctaneSdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.Tools
{
    public class ToolsTrackController : Controller
    {
        //******************
        //Author: Mudassar I
        //Date: 02/03/2017
        //FloorMasterController
        //******************
        // Create an instance of the ImpinjReader class.
        static ImpinjReader reader = new ImpinjReader();

        // Create a Dictionary to store the tags we've read.
        private Dictionary<string, int> tagsRead = new Dictionary<string, int>();

        private Dictionary<string, int> tagsMontor = new Dictionary<string, int>();
        static List<toMonitor> tagsTrack = new List<toMonitor>();
        private Dictionary<string, int> ListtagsRead = new Dictionary<string, int>();
        private Dictionary<string, int> ListtagsMontor = new Dictionary<string, int>();

        private ERPdbEntities db = new ERPdbEntities();
        public static String UserNameTrack; static int orgIdTrack; static string ReaderMac; static string Uid;
        // GET: ToolsTrack
        public ActionResult Index()
        {
            return View();
        }
        //
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
                //reader.Disconnect();
                //
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

                ReaderMac = GetClientMac(Reader);
                reader.Start();
                msg = "Reader started...";
                //// Wait for the user to press enter.
                //// Stop reading.
                //reader.Stop();
                //// Disconnect from the reader.
                //reader.Disconnect();
                UserNameTrack = Session["AppUserName"].ToString();
                //Get Organization Id From Session Variable
                orgIdTrack = Convert.ToInt32(Session["OrgInfoId"]);
                //Get Selected Data Accourding to Org Id
                Uid = DateTime.Now.ToString().GetHashCode().ToString("x");



                System.Timers.Timer timer = new System.Timers.Timer();
                timer.Interval = 30000;
                timer.Elapsed += timer_Elapsed;
                timer.Start();

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
                    InsertTrack(tag.Epc.ToString(), tag.AntennaPortNumber);
                    tagsRead.Add(tag.Epc.ToString(), tag.AntennaPortNumber);

                }
                if (!tagsMontor.ContainsKey(tag.Epc.ToString()))
                {
                    tagsTrack.Add(new toMonitor
                    {
                        Epc = tag.Epc.ToString(),
                        Name = ReaderMac,
                        AntennaPortNumber = tag.AntennaPortNumber
                    });

                    tagsMontor.Add(tag.Epc.ToString(), tag.AntennaPortNumber);
                }
            }
        }
        //Track RFID
        private void InsertTrack(string Key, int Val)
        {
            try
            {
                toTrackInfo Obje = new toTrackInfo();
                Obje.RFID = Key;
                Obje.mAttPortId = Val;
                Obje.ReaderNo = ReaderMac;
                Obje.OrgInfoId = orgIdTrack;
                Obje.tDate = DateTime.Now;
                Obje.UID = Uid;
                db.toTrackInfoes.Add(Obje);
                db.SaveChanges();
            }
            catch (Exception)
            {

            }
        }
        //Monitor RFID
        private void InsertMonitor()
        {
            try
            {
                using (var ctx = new ERPdbEntities())
                {
                    // Delete Record
                    var Flags = ctx.Database.ExecuteSqlCommand("DELETE FROM toMonitor WHERE ReaderNo='" + ReaderMac + "'");
                    // Do not use the ChangeTracker or require to add the list in the DbSet
                    ctx.toMonitors.AddRange(tagsTrack);
                    ctx.SaveChanges();
                }
            }
            catch (Exception)
            {

            }
            TrackMonitor();
        }

        //Track & Monitor RFID
        [HttpGet]
        public JsonResult TrackMonitor()
        {
            var ObjData = (from Tooltag in db.toTooltags
                           join Mon in db.toMonitors on Tooltag.RFID equals Mon.Epc into Mon_OBJ
                           where (Tooltag.OrgInfoId == orgIdTrack && Tooltag.IsAction == true)
                           from Mon in Mon_OBJ.DefaultIfEmpty()
                           select new
                           {
                               Tooltag.toTooltagId,
                               RFIDt = Tooltag.RFID,
                               Mon.Epc

                           }).ToList();

            foreach (var gObj in ObjData)
            {
                if (gObj.Epc == "" || gObj.Epc == null)
                {
                    try
                    {
                        var original = db.toTrackInfoes.FirstOrDefault(b => b.RFID == gObj.RFIDt && b.UID == Uid && b.mAttPortId == 0);
                        if (original != null)
                        {

                        }
                        else
                        {
                            InsertTrack(gObj.RFIDt.ToString(), 0);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //
        [HttpGet]
        public JsonResult GetAllCount()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = db.toTooltags
              .Where(o => o.IsAction == true && o.OrgInfoId == orgIdTrack)
              .Count();
            //Convert List Data to The Json Array         

            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //
        [HttpGet]
        public JsonResult GetTrackCount()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            //            select m.*
            //from toTooltag t
            //INNER JOIN toMonitor m on t.RFID = m.RFID

            //var ObjData = (from Tooltag in db.toTooltags
            //               join Mon in db.toMonitors on Tooltag.RFID equals Mon.RFID into Mon_OBJ
            //               where (Tooltag.OrgInfoId == orgIdTrack && Tooltag.IsAction == true)
            //               select new
            //               {
            //                   Tooltag.toTooltagId,
            //               }).Count();


            var ObjData = db.toMonitors
              .Where(o => o.Name == ReaderMac)
              .Count();


            //Convert List Data to The Json Array         

            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //
        [HttpGet]
        public JsonResult GetListtagsMontor()
        {
            return Json(tagsMontor.ToArray(), JsonRequestBehavior.AllowGet);
        }
        //Not Used
        private void GetListtagsOparationMethod()
        {

            tagsTrack.Clear();
            ListtagsRead.Clear();
            ListtagsMontor.Clear();

            ListtagsRead = tagsRead;
            ListtagsMontor = tagsMontor;

            try
            {
                foreach (var InObj in ListtagsRead)
                {
                    if (!ListtagsMontor.ContainsKey(InObj.Key))
                    {
                        tagsTrack.Add(new toMonitor
                        {
                            Epc = InObj.Key
                        });
                    }
                }
            }
            catch (Exception)
            {

            }



            if (tagsTrack.Count != 0)
            {
                try
                {
                    using (var ctx = new ERPdbEntities())
                    {
                        // Do not use the ChangeTracker or require to add the list in the DbSet
                        ctx.toMonitors.AddRange(tagsTrack);

                        ctx.SaveChanges();
                    }
                }
                catch (Exception)
                {

                }
            }


        }
        //Not Used
        [HttpGet]
        public JsonResult GetListtagsOparation()
        {
            tagsTrack.Clear();
            ListtagsRead.Clear();
            ListtagsMontor.Clear();

            ListtagsRead = tagsRead;
            ListtagsMontor = tagsMontor;

            try
            {
                foreach (var InObj in ListtagsRead)
                {
                    if (!ListtagsMontor.ContainsKey(InObj.Key))
                    {
                        tagsTrack.Add(new toMonitor
                        {
                            Epc = InObj.Key
                        });
                    }
                }

            }
            catch (Exception)
            {

            }

            if (tagsTrack.Count != 0)
            {
                try
                {
                    using (var ctx = new ERPdbEntities())
                    {
                        // Do not use the ChangeTracker or require to add the list in the DbSet
                        ctx.toMonitors.AddRange(tagsTrack);
                        ctx.SaveChanges();
                        tagsRead.Clear();
                    }
                }
                catch (Exception)
                {

                }
            }

            return Json(tagsTrack.ToArray(), JsonRequestBehavior.AllowGet);
        }


        //
        [HttpGet]
        public JsonResult getGetToTrackData()
        {
            var ObjData = (db.toTrackInfoes.Where(o => o.OrgInfoId == orgIdTrack && o.ReaderNo == ReaderMac && o.UID == Uid).ToList());
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //Timer Method
        private void timer_Elapsed(object sender, EventArgs e)
        {
            InsertMonitor();
            tagsTrack.Clear();
            tagsMontor.Clear();
        }
        //
        [HttpGet]
        public string ReaderClear()
        {
            string msg = "";
            try
            {
                tagsRead.Clear();
                tagsMontor.Clear();
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
        //
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
                           select new { Dis.ReaderIP }).Distinct().ToList();
            //Convert List Data to The Json Array         

            return Json(ObjData, JsonRequestBehavior.AllowGet);
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
                           join Lm in db.mRoomMasters on tAs.mRoomMasterId equals Lm.mRoomMasterId
                           where (tAs.OrgInfoId == orgId && tAs.IsAction == true)
                           select new { tAs.tEmployeeTagId, tAs.RFID, tAs.EmployeeName, tAs.EmployeeId, Lm.RoomName, Lm.RoomNo, tAs.EmailId, tAs.ContactNo }).ToList();
            //Convert List Data to The Json Array       
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //
        [HttpGet]
        public string DeleteData(int ID)
        {
            string msg = "";

            int orgId = Convert.ToInt32(Session["OrgInfoId"]); var UserName = Session["AppUserName"];
            var original = db.tEmployeeTags.FirstOrDefault(b => b.tEmployeeTagId == ID);
            if (original != null)
            {

                //
                original.IsAction = false;
                original.ModifiedBy = UserName.ToString();
                original.ModifiedDate = DateTime.Now;

                //
                var rType = db.tRFIDTypes.FirstOrDefault(b => b.RerfrenceId == ID);
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
        private String GetClientMac(string IPAddress)
        {
            /* There is a good chance that the server does not have an "arp" record for a given IP address, */
            /* so to create an arp entry we first send a ping to the IP Address*/
            string mac = string.Empty;

            try
            {
                //Sending ping:
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "ping";
                psi.CreateNoWindow = true;
                psi.RedirectStandardInput = false;
                psi.RedirectStandardOutput = false;
                psi.Arguments = IPAddress;
                psi.UseShellExecute = false;

                Process process = Process.Start(psi);
                process.WaitForExit();

                //Now run arp command:
                psi = new ProcessStartInfo();
                psi.FileName = "arp";
                psi.CreateNoWindow = false;
                psi.RedirectStandardInput = false;
                psi.RedirectStandardOutput = true;
                psi.Arguments = "-a " + IPAddress;
                psi.UseShellExecute = false;

                process = Process.Start(psi);
                string arpInfo = string.Empty;
                while (!process.StandardOutput.EndOfStream)
                {
                    arpInfo += process.StandardOutput.ReadLine();
                }

                process.WaitForExit();

                //Remove all white space from ARP result:
                arpInfo = arpInfo.Replace(" ", "");
                //Strip MAC from ARP result
                mac = arpInfo.Substring((arpInfo.IndexOf(IPAddress) + IPAddress.Length), 17);
            }
            catch (Exception)
            {
                //Error code here....
            }
            return mac.ToUpper();
        }
        //     
        public static string ConvertHextoAscii(string HexString)
        {
            string asciiString = "";
            try
            {
                for (int i = 0; i < HexString.Length; i += 2)
                {
                    if (HexString.Length >= i + 2)
                    {
                        String hs = HexString.Substring(i, 2);
                        asciiString = asciiString + System.Convert.ToChar(System.Convert.ToUInt32(HexString.Substring(i, 2), 16)).ToString();
                    }
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.Message.ToString());
            }
            return asciiString;
        }
    }
}
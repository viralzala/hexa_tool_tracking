using HexaERP.MVC.Models;
using Impinj.OctaneSdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class ManageReaderController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();
        // Create a collection to hold all the ImpinjReader instances.
        static List<ImpinjReader> readers = new List<ImpinjReader>();

        // Create a Dictionary to store the tags we've read.
        static List<tToolTrackDemo> tagsTrack = new List<tToolTrackDemo>();
        static Dictionary<string, int> tagsRead = new Dictionary<string, int>();
        static Dictionary<string, int> tagsMontor = new Dictionary<string, int>();

        static Dictionary<bool, string> errormsg = new Dictionary<bool, string>();
        //
        public static String UserNameTrack;
        //static int orgIdTrack;
        //static string ReaderMac;
        static string Uid;

        static int OrgInfoId;
        // GET: ToolsTrackDemo
        public ActionResult Index()
        {
            try
            {
                //--- Get cookie Collection.
                HttpCookie cookieObject = Request.Cookies["HexaCookie"];
                //--- Check for null 
                if (cookieObject != null)
                {
                    tagsRead.Clear();
                    tagsTrack.Clear();
                    tagsMontor.Clear();
                    readers.Clear();
                    errormsg.Clear();

                    OrgInfoId = Convert.ToInt32(cookieObject["OrgInfoId"]);
                    UserNameTrack = cookieObject["AppUserName"];

                }
                else { return RedirectToAction("Index", "AppUser"); }

                if (Session["UniqueId"].ToString() != "" && Session["OrgInfoId"].ToString() != "" && Session["AppUserName"].ToString() != "")
                {
                    //string Page_Name = Path.GetFileName(Request.Path);                   
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
        //
        [HttpGet]
        public JsonResult ReaderInit()
        {
            string msg = "", readerName = "", readerIp = ""; bool Flag = false;

            //List<ReaderConnections> rConn = new List<ReaderConnections>();
            //ReaderConnections rconE = new ReaderConnections();

            try
            {
                readers.Clear();
                //errormsg.Clear();
                var _rdeaders = db.mReaderSettups.Where(j => j.IsAction == true && j.OrgInfoId == OrgInfoId).Select(c => new { c.ReaderIP, c.ReaderNo }).Distinct().ToList();

                foreach (var _r in _rdeaders)
                {
                    readers.Add(
                        new ImpinjReader
                        (_r.ReaderIP, _r.ReaderNo));
                }

                // Create two reader instances and add them to the List of readers.
                //readers.Add(new ImpinjReader("172.31.99.201", "00-16-25-12-42-F7"));
                //readers.Add(new ImpinjReader("172.31.99.210", "00-16-25-12-42-EF"));
                //readers.Add(new ImpinjReader("172.31.99.211", "00-16-25-12-42-F4"));
                // Change "SpeedwayR-10-27-52" to the IP address 
                // or hostname of the second reader.
                //readers.Add(new ImpinjReader("172.31.99.210", "00-16-25-12-42-EF"));
                // Loop through the List of readers to configure and start them.
                foreach (ImpinjReader reader in readers)
                {
                    // Don't call the Start method if the
                    // reader is already running.
                    //if (!reader.QueryStatus().IsSingulating)
                    //{

                    //rconE.readerName = reader.Name;
                    //rconE.readerIp = reader.Address;

                    // Connect to the reader
                    reader.Connect();

                    // Get the default settings
                    // We'll use these as a starting point
                    // and then modify the settings we're 
                    // interested in.
                    Settings settings = reader.QueryDefaultSettings();

                    // Tell the reader to include the antenna number
                    // and TID (using FastID) in all tag reports. 
                    // FastID is available on Impinj Monza 4 and later chips.
                    settings.Report.IncludeAntennaPortNumber = true;
                    // settings.Report.IncludeFastId = true;

                    //// Enable keepalives.
                    //settings.Keepalives.Enabled = true;
                    //settings.Keepalives.PeriodInMs = 30000;

                    //// Enable link monitor mode.
                    //// If our application fails to reply to
                    //// five consecutive keepalive messages,
                    //// the reader will close the network connection.
                    //settings.Keepalives.EnableLinkMonitorMode = true;
                    //settings.Keepalives.LinkDownThreshold = 5;

                    //// Assign an event handler that will be called
                    //// when keepalive messages are received.
                    //reader.KeepaliveReceived += OnKeepaliveReceived;

                    //// Assign an event handler that will be called
                    //// if the reader stops sending keepalives.
                    //reader.ConnectionLost += OnConnectionLost;


                    //// Send a tag report every time the reader stops (period is over).
                    //settings.Report.Mode = ReportMode.BatchAfterStop;

                    //// Reading tags for 10 seconds every 20 seconds
                    //settings.AutoStart.Mode = AutoStartMode.Periodic;
                    //settings.AutoStart.PeriodInMs = 20000;
                    //settings.AutoStop.Mode = AutoStopMode.Duration;
                    //settings.AutoStop.DurationInMs = 10000;

                    // Apply the newly modified settings.
                    reader.ApplySettings(settings);

                    // Assign the TagsReported event handler.
                    // This specifies which method to call
                    // when tags reports are available.
                    reader.TagsReported += OnTagsReported;

                    //rconE.Flag = true; rconE.msg = "Reader started...";


                    // Start reading.
                    reader.Start();

                    Flag = true; msg = "Reader started...";

                    //}
                }

                // Stop all the readers and disconnect from them.
                //foreach (ImpinjReader reader in readers)
                //{
                //    // Stop reading.
                //    reader.Stop();

                //    // Disconnect from the reader.
                //    reader.Disconnect();
                //}
            }
            catch (OctaneSdkException e)
            {
                // Handle Octane SDK errors.                
                // rconE.Flag = false; rconE.msg = "('Octane SDK exception: {0}'," + e.Message + ")";
                msg = "('Octane SDK exception: {0}'," + e.Message + ")";
                return Json(new { Flag = Flag, Message = msg, rName = readerName, rIp = readerIp }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                // Handle other .NET errors.                         
                //rconE.Flag = false; rconE.msg = "('Exception : {0}'," + e.Message + ")";
                msg = "('Exception : {0}'," + e.Message + ")";
                return Json(new { Flag = Flag, Message = msg, rName = readerName, rIp = readerIp }, JsonRequestBehavior.AllowGet);
            }

            Uid = DateTime.Now.ToString().GetHashCode().ToString("x");

            //System.Timers.Timer timer2 = new System.Timers.Timer();
            //timer2.Interval = 9000000;
            //timer2.Elapsed += timer_Triger;
            //timer2.Start();

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 20000;
            timer.Elapsed += timer_Elapsed;
            timer.Start();

            // rConn.Add(rconE);

            //return Json(rConn, JsonRequestBehavior.AllowGet);
            return Json(new { Flag = Flag, Message = msg, rName = readerName, rIp = readerIp }, JsonRequestBehavior.AllowGet);
        }

        //Timer Method
        //private void timer_Triger(object sender, EventArgs e)
        //{
        //    tagsRead.Clear();
        //}
        //
        public void OnTagsReported(ImpinjReader sender, TagReport report)
        {
            String Key; int PortId; String EpcKey;
            // This event handler is called asynchronously 
            // when tag reports are available.
            // Loop through each tag in the report 
            // and print the data.
            foreach (Tag tag in report)
            {
                Key = tag.Epc.ToString(); PortId = tag.AntennaPortNumber;
                EpcKey = Key + "-" + sender.Name + "-" + PortId;

                if (!tagsRead.ContainsKey(EpcKey))
                {
                    // Add this tag to the list of tags we've read.
                    InsertTrack(tag.Epc.ToString(), tag.AntennaPortNumber, sender.Name, sender.Address);
                    tagsRead.Add(EpcKey, tag.AntennaPortNumber);

                }

                // If this tag hasn't been read before, print out the EPC and TID
                if (!tagsMontor.ContainsKey(EpcKey))
                {
                    // Add this tag to the list of tags we've read.                   
                    tagsMontor.Add(EpcKey, tag.AntennaPortNumber);
                    tagsTrack.Add(new tToolTrackDemo
                    {
                        Epc = tag.Epc.ToString(),
                        Reader = sender.Name,
                        PortId = tag.AntennaPortNumber,
                        tDate = DateTime.Now
                    });
                }
            }
        }
        //Track RFID
        private void InsertTrack(string Key, int Val, string Mac, string ip)
        {
            try
            {
                toTrackInfo Obje = new toTrackInfo();
                Obje.RFID = Key;
                Obje.mAttPortId = Val;
                Obje.ReaderNo = Mac;
                Obje.ReaderIP = ip;
                Obje.tDate = DateTime.Now;
                Obje.OrgInfoId = OrgInfoId;
                Obje.UID = Uid;
                Obje.AppUserName = UserNameTrack;
                db.toTrackInfoes.Add(Obje);
                db.SaveChanges();
            }
            catch (Exception)
            {

            }
        }
        //Timer Method
        private void timer_Elapsed(object sender, EventArgs e)
        {
            try
            {
                var newJson = JsonConvert.SerializeObject(tagsTrack);
                System.IO.File.WriteAllText(Server.MapPath("~/Content/EmployeeTag.json"), newJson);
                tagsTrack.Clear();
                tagsMontor.Clear();
            }
            catch (Exception)
            {

            }
            //GetReaderStatus();
        }
        //
        [HttpGet]
        public JsonResult getGetToTrackData()
        {
            //var ObjData = tagsTrack;
            var ObjData = (from tm in tagsTrack
                           join rft in db.tEmployeeTags on tm.Epc equals rft.RFID
                           join getl in db.mRoomMasters on rft.mRoomMasterId equals getl.mRoomMasterId
                           join getfl in db.mFloorMasters on getl.mFloorMasterId equals getfl.mFloorMasterId
                           join trm in db.mReaderSettups on rft.mRoomMasterId equals trm.mRoomMasterId
                           //where tm.mAttPortId == trm.RoomNo
                           select new
                           {
                               tm.Epc,
                               Name = rft.EmployeeName,
                               //Types = rft.GetType,
                               MyRoom = getl.RoomName,
                               //MyRoomNo = getl.RoomNo,
                               //MyFloor = getfl.FloorName,
                               //MyFloorNo = getfl.FloorNo,
                               MasteReader = trm.ReaderNo,
                               MastePort = trm.AttPortId,

                               IsTrack = ((tm.PortId) - (trm.AttPortId)),
                               Reader = tm.Reader,
                               PortId = tm.PortId

                           }).ToList();

            //Convert List Data to The Json Array             
            return Json(ObjData.ToArray(), JsonRequestBehavior.AllowGet);
        }
        //
        [HttpGet]
        public JsonResult getlocationdata()
        {
            //Get Organization Id From Session Variable
            //int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var FloorData = (from Dis in db.mFloorMasters
                             where (Dis.IsAction == true && Dis.OrgInfoId == OrgInfoId)
                             select new { Dis.mFloorMasterId, Dis.FloorName }).ToList();

            var RoomData = (from Dis in db.mRoomMasters
                            join F in db.mFloorMasters on Dis.mFloorMasterId equals F.mFloorMasterId
                            //join R in db.mReaderSettups on Dis.mRoomMasterId equals R.mRoomMasterId
                            where (Dis.IsAction == true && Dis.OrgInfoId == OrgInfoId)
                            select new { Dis.mRoomMasterId, Dis.RoomName, F.FloorName, Dis.mFloorMasterId }).ToList();

            var PortsData = (from Dis in db.mReaderSettups
                             join rm in db.mRoomMasters on Dis.mRoomMasterId equals rm.mRoomMasterId
                             where (Dis.IsAction == true && Dis.OrgInfoId == OrgInfoId)
                             select new { Dis.mReaderSettupId, Dis.ReaderNo, Dis.AttPortId, rm.RoomName, Dis.mRoomMasterId }).ToList();

            //Convert List Data to The Json Array                     
            return Json(new { IFloorData = FloorData, IObjData = RoomData, IPortsData = PortsData }, JsonRequestBehavior.AllowGet);
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
        //
        [HttpGet]
        public JsonResult StopReaders()
        {
            string msg = ""; bool Flag = false;
            try
            {
                // Stop all the readers and disconnect from them.
                foreach (ImpinjReader reader in readers)
                {
                    // Don't call the Stop method if the
                    // reader is already stopped.
                    if (reader.QueryStatus().IsSingulating)
                    {
                        // Stop reading.
                        reader.Stop();

                        // Disconnect from the reader.
                        reader.Disconnect();

                        Flag = true;
                        msg = "Reader stopped..";
                    }

                }

            }
            catch (OctaneSdkException ex)
            {
                // An Octane SDK exception occurred. Handle it here.
                msg = ex.Message.ToString();
                Flag = false;
            }
            catch (Exception ex)
            {
                // A general exception occurred. Handle it here.
                msg = ex.Message.ToString();
                Flag = false;
            }

            return Json(new { Flag = Flag, Message = msg }, JsonRequestBehavior.AllowGet);
        }
        //
        [HttpGet]
        public JsonResult ReaderStatusDetail()
        {
            //Convert List Data to The Json Array
            return Json(errormsg, JsonRequestBehavior.AllowGet);
        }
        //
        [HttpGet]
        public JsonResult ReaderClear()
        {
            string msg = ""; bool Flag = false;

            //Convert List Data to The Json Array          
            try
            {
                tagsMontor.Clear();
                tagsRead.Clear();
                Flag = true;
                msg = "Data Cleared..";
            }
            catch (Exception ex)
            {
                msg = ex.Message.ToString();
                Flag = false;
            }
            return Json(new { Flag = Flag, Message = msg }, JsonRequestBehavior.AllowGet);
        }
        //       
        static void OnConnectionLost(ImpinjReader reader)
        {
            // This event handler is called if the reader  
            // stops sending keepalive messages.

            // Get the reader status.
            Status status = reader.QueryStatus();
            errormsg.Add(status.IsConnected, reader.Name);
            ///Console.WriteLine("Connection lost : {0} ({1})", reader.Name, reader.Address);

        }
        //
        static void OnKeepaliveReceived(ImpinjReader reader)
        {
            // This event handler is called when a keepalive 
            // message is received from the reader.


            // Get the reader status.
            Status status = reader.QueryStatus();

            // = status.IsConnected.ToString();
            // = reader.QueryStatus().IsSingulating;

            errormsg.Add(status.IsConnected, reader.Name);

            //LICon.Content = status.IsConnected.ToString();
            //lIso.Content = reader.QueryStatus().IsSingulating;

            //Console.WriteLine("Keepalive received from {0} ({1})", reader.Name, reader.Address);
        }
        //
        public void GetReaderStatus()
        {
            foreach (ImpinjReader reader in readers)
            {
                // Get the reader status.
                Status status = reader.QueryStatus();

            }

        }
    }
}
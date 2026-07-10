using HexaERP.MVC.Models;
using Impinj.OctaneSdk;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.Tools
{
    public class ToolsTrackDemoController : Controller
    {

        private static IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<ReaderStatusHub>();

        private static IHubContext _hubAttena = GlobalHost.ConnectionManager.GetHubContext<ReaderStatusHub>();

        //private static IHubContext _hubMonitor = GlobalHost.ConnectionManager.GetHubContext<ReaderStatusHub>();


        private ERPdbEntities db = new ERPdbEntities();
        // Create a collection to hold all the ImpinjReader instances.
        static List<ImpinjReader> readers = new List<ImpinjReader>();

        // Create a Dictionary to store the tags we've read.
        static List<tToolTrackDemo> tagsTrack = new List<tToolTrackDemo>();
        static Dictionary<string, int> tagsRead = new Dictionary<string, int>();

        static Dictionary<string, int> tagsMontor = new Dictionary<string, int>();

        static Dictionary<string, int> tagsMap = new Dictionary<string, int>();
        static List<tToolTrackDemo> tagsTrackMap = new List<tToolTrackDemo>();

        static List<ReaderInfoEntity> RdrInfo = new List<ReaderInfoEntity>();

        //
        public static String UserNameTrack;
        //static int orgIdTrack;
        //static string ReaderMac;
        static string Uid;

        static int OrgInfoId; static double RssiVal = 50;

        static Dictionary<string, bool> ReaderConLost = new Dictionary<string, bool>();
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
                    ViewBag.LogedIn = cookieObject["AppUserName"];
                    OrgInfoId = Convert.ToInt32(cookieObject["OrgInfoId"]);
                    UserNameTrack = cookieObject["AppUserName"];

                    // System.IO.File.WriteAllText(Server.MapPath("~/Content/EmployeeTag.json"), null);

                    tagsRead.Clear();
                    tagsTrack.Clear();
                    //tagsMontor.Clear();
                    readers.Clear();
                    RdrInfo.Clear();
                    ReaderConLost.Clear();
                    //tagsTrackMap.Clear();
                    //tagsMap.Clear();


                }
                else { return RedirectToAction("Index", "AppUser"); }

                if (Session["UniqueId"].ToString() != "" && Session["OrgInfoId"].ToString() != "" && Session["AppUserName"].ToString() != "")
                {
                    //string Page_Name = Path.GetFileName(Request.Path);
                    if (Convert.ToString(Session["SortCode"]) != "SA")
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
        //
        [HttpGet]
        public JsonResult ReaderInit(double _Rssival, double TxVal, bool _isXspan)
        {
            string msg = "", readerName = "", readerIp = ""; bool Flag = false;
            Uid = DateTime.Now.ToString().GetHashCode().ToString("x");
            ReaderConLost.Clear();

            try
            {
                readers.Clear();
                //RssiVal = 80;
                RssiVal = _Rssival;

                var _rdeaders = db.mReaderSettups.Where(j => j.IsAction == true && j.OrgInfoId == OrgInfoId).Select(c => new { c.ReaderIP, c.ReaderNo }).Distinct().ToList();

                if (_rdeaders == null)
                {
                    return Json(new { Flag, Message = "Readers not found" }, JsonRequestBehavior.AllowGet);
                }
                else if (_rdeaders.Count > 4)
                {
                    return Json(new { Flag, Message = "More then #4 reader found please contact your service pervider" }, JsonRequestBehavior.AllowGet);
                }

                foreach (var _r in _rdeaders)
                {
                    try
                    {
                        var ping = GetClientMac(_r.ReaderIP);

                        if (string.IsNullOrEmpty(ping))
                        {
                            return Json(new { Flag = Flag, Message = "Readers IP not Pinging " + _r.ReaderIP }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception)
                    {
                        return Json(new { Flag = Flag, Message = "Readers IP not Pinging " + _r.ReaderIP }, JsonRequestBehavior.AllowGet);
                    }
                }


                foreach (var _r in _rdeaders)
                {
                    readers.Add(
                        new ImpinjReader
                        (_r.ReaderIP, _r.ReaderNo));
                }

                foreach (ImpinjReader reader in readers)
                {
                    if (!reader.IsConnected)
                    {
                        reader.Connect();
                    }

                    Settings settings = reader.QueryDefaultSettings();
                    //settings.AutoStart.Mode = AutoStartMode.Immediate;
                    settings.Report.IncludePeakRssi = true;
                    settings.Report.IncludeAntennaPortNumber = true;
                    settings.Keepalives.Enabled = true;
                    settings.Keepalives.PeriodInMs = 15000;

                    settings.ReaderMode = ReaderMode.AutoSetDenseReaderDeepScan;
                    settings.SearchMode = SearchMode.DualTarget;
                    settings.Session = 2;

                    if (reader.IsXArray)
                    {
                        settings.Antennas.DisableAll();
                        settings.Antennas.EnableById(new ushort[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52 });
                    }
                    else if (reader.IsXSpan)
                    {
                        settings.Antennas.DisableAll();
                        settings.Antennas.EnableById(new ushort[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 });
                    }

                    if (!reader.IsXArray && !reader.IsXSpan)
                    {
                        FeatureSet features = reader.QueryFeatureSet();
                        foreach (TxPowerTableEntry tx in features.TxPowers)
                        {

                            settings.Antennas.GetAntenna(1).TxPowerInDbm = TxVal;
                            settings.Antennas.GetAntenna(2).TxPowerInDbm = TxVal;
                            settings.Antennas.GetAntenna(3).TxPowerInDbm = TxVal;
                            settings.Antennas.GetAntenna(4).TxPowerInDbm = TxVal;
                        }
                    }

                    reader.ApplySettings(settings);
                    //Thread.Sleep(2000);
                    reader.KeepaliveReceived += OnKeepaliveReceived;
                    reader.ConnectionLost += OnConnectionLost;
                    reader.AntennaChanged += OnAntennaEvent;
                    reader.ReaderStarted += OnReaderStarted;
                    reader.TagsReported += OnTagsReported;
                    reader.Start();
                    Flag = true; msg = "Reader started...";
                }

            }
            catch (OctaneSdkException e)
            {
                foreach (ImpinjReader reader in readers)
                {
                    reader.Stop();
                    reader.Disconnect();
                }
                msg = e.Message;
                return Json(new { Flag = Flag, Message = msg, rName = readerName, rIp = readerIp }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                msg = "('Exception : {0}'," + e.Message + ")";
                return Json(new { Flag = Flag, Message = msg, rName = readerName, rIp = readerIp }, JsonRequestBehavior.AllowGet);
            }




            //System.Timers.Timer timermap = new System.Timers.Timer();
            //timermap.Interval = 600000;            
            //timermap.Elapsed += timer_Trigermap;
            //timermap.Start();

            System.Timers.Timer timer2 = new System.Timers.Timer();
            timer2.Interval = 300000;
            timer2.Elapsed += timer_Triger;
            timer2.Start();

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 5000;
            timer.Elapsed += timer_Elapsed;
            timer.Start();

            // rConn.Add(rconE);
            //return Json(rConn, JsonRequestBehavior.AllowGet);
            return Json(new { Flag = Flag, Message = msg, rName = readerName, rIp = readerIp }, JsonRequestBehavior.AllowGet);
        }


        //Timer Method
        private void timer_Triger(object sender, EventArgs e)
        {
            tagsRead.Clear();
        }

        //Timer Method
        //private void timer_Trigermap(object sender, EventArgs e)
        //{
        //    tagsTrackMap.Clear();
        //    tagsMap.Clear();
        //}

        //
        public void OnTagsReported(ImpinjReader sender, TagReport report)
        {
            //String Key;
            //int PortId;
            String EpcKey;
            //String EpcKeyt;
            // This event handler is called asynchronously 
            // when tag reports are available.
            // Loop through each tag in the report 
            // and print the data.
            foreach (Tag tag in report)
            {
                // Key = tag.Epc.ToString(); PortId = tag.AntennaPortNumber;
                EpcKey = tag.Epc + "-" + sender.Name;
                // EpcKeyt = tag.Epc + "-" + sender.Name + "-" + tag.AntennaPortNumber;
                double Rssi = Math.Abs(tag.PeakRssiInDbm);

                if (!tagsRead.ContainsKey(EpcKey))
                {
                    // Add this tag to the list of tags we've read.
                    if (Rssi <= RssiVal)
                    {
                        InsertTrack(tag.Epc.ToString(), tag.AntennaPortNumber, sender.Name, sender.Address);
                        tagsRead.Add(EpcKey, tag.AntennaPortNumber);
                    }

                }

                if (!tagsMap.ContainsKey(tag.Epc.ToString()))
                {
                    // Add this tag to the list of tags we've read.
                    tagsMap.Add(tag.Epc.ToString(), tag.AntennaPortNumber);
                    tagsTrackMap.Add(new tToolTrackDemo
                    {
                        Epc = tag.Epc.ToString(),
                        Reader = sender.Name,
                        PortId = tag.AntennaPortNumber,
                        tDate = DateTime.Now
                    });
                }

                // If this tag hasn't been read before, print out the EPC and TID
                if (!tagsMontor.ContainsKey(tag.Epc.ToString()))
                {
                    // Add this tag to the list of tags we've read.  
                    if (Rssi <= RssiVal)
                    {
                        tagsMontor.Add(tag.Epc.ToString(), tag.AntennaPortNumber);
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
        //
        public class ProductComparer : IEqualityComparer<tToolTrackDemo>
        {

            public bool Equals(tToolTrackDemo x, tToolTrackDemo y)
            {
                //Check whether the objects are the same object. 
                if (Object.ReferenceEquals(x, y))
                    return true;

                //Check whether the products' properties are equal. 
                return x != null && y != null && x.Epc == y.Epc;
            }

            public int GetHashCode(tToolTrackDemo obj)
            {
                //Get hash code for the Name field if it is not null. 
                int hashProductName = obj.Epc == null ? 0 : obj.Epc.GetHashCode();

                //Calculate the hash code for the product. 
                return hashProductName;
            }
        }

        //
        public class GenericCompare<T> : IEqualityComparer<T> where T : class
        {
            private Func<T, object> _expr { get; set; }
            public GenericCompare(Func<T, object> expr)
            {
                this._expr = expr;
            }
            public bool Equals(T x, T y)
            {
                var first = _expr.Invoke(x);
                var sec = _expr.Invoke(y);
                if (first != null && first.Equals(sec))
                    return true;
                else
                    return false;
            }
            public int GetHashCode(T obj)
            {
                return obj.GetHashCode();
            }
        }


        //Timer Method
        private void timer_Elapsed(object sender, EventArgs e)
        {
            try
            {
                List<tToolTrackDemo> convertedList = tagsTrack.ToList();

                convertedList.ForEach(sc =>
                {
                    tToolTrackDemo original = tagsTrackMap.FirstOrDefault(obj => obj.Epc == sc.Epc);
                    if (original != null)
                    {
                        original.Epc = sc.Epc;
                        original.Reader = sc.Reader;
                        original.PortId = sc.PortId;
                        original.tDate = sc.tDate;
                    }
                });

                //var baseList = tagsTrackMap.AsQueryable();
                var newJson = JsonConvert.SerializeObject(tagsTrackMap.AsQueryable());
                System.IO.File.WriteAllText(Server.MapPath("~/Content/EmployeeTag.json"), newJson);

                //_hubMonitor.Clients.All.readerstatus("Impinj R420 Monitor Data", newJson);

                tagsTrack.Clear();
                tagsMontor.Clear();
            }
            catch (Exception)
            {

            }
        }
        //
        [HttpGet]
        public JsonResult getGetToTrackData()
        {
            // Initialization.    
            JsonResult result = new JsonResult();
            try
            {
                var ObjDatass = (from tm in tagsTrack
                                 join emp in db.tEmployeeTags on tm.Epc equals emp.RFID
                                 join ag in db.mAgencies on emp.mAgencyId equals ag.mAgencyId
                                 join ds in db.mDesignations on emp.mDesignationId equals ds.mDesignationId
                                 join sk in db.mSkillCategories on emp.mSkillCategoryId equals sk.mSkillCategoryId
                                 join wk in db.mWorkCategories on emp.mWorkCategoryId equals wk.mWorkCategoryId
                                 join ac in db.mActivities on emp.mActivityId equals ac.mActivityId

                                 join rst in db.mReaderSettups on tm.Reader equals rst.ReaderNo
                                 where tm.PortId == rst.AttPortId
                                 select new
                                 {
                                     Agency = ag.Agency ?? string.Empty,
                                     Designation = ds.Designation ?? string.Empty,
                                     SkillCategory = sk.SkillCategory ?? string.Empty,
                                     WorkCategory = wk.WorkCategory ?? string.Empty,
                                     Activity = ac.Activity ?? string.Empty,
                                     Name = emp.EmployeeName ?? string.Empty,
                                     tm.Epc,
                                     rst.mFloorMasterId
                                 }
                                ).ToList();

                result = this.Json(ObjDatass, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                var Err = ex.InnerException.Message.ToString();
            }
            return result;
            //return Json(ObjDatas, JsonRequestBehavior.AllowGet);
        }
        //
        [HttpGet]
        public JsonResult getlocationdata()
        {
            //Get Organization Id From Session Variable
            //int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id

            var ZoneData = (from Dis in db.mZones
                            where (Dis.IsAction == true && Dis.OrgInfoId == OrgInfoId)
                            select new { Dis.mZoneId, Dis.Zone }).ToList();

            var subZoneData = (from Dis in db.mFloorMasters
                               join F in db.mZones on Dis.mZoneId equals F.mZoneId
                               where (Dis.IsAction == true && Dis.OrgInfoId == OrgInfoId)
                               select new { Dis.mFloorMasterId, Dis.FloorName, F.mZoneId, F.Zone }).ToList();

            var FloorData = (from Dis in db.mFloorMasters
                             where (Dis.IsAction == true && Dis.OrgInfoId == OrgInfoId)
                             select new { Dis.mFloorMasterId, Dis.FloorName }).ToList();

            var RoomData = (from Dis in db.mRoomMasters
                            join F in db.mFloorMasters on Dis.mFloorMasterId equals F.mFloorMasterId
                            //join R in db.mReaderSettups on Dis.mRoomMasterId equals R.mRoomMasterId
                            where (Dis.IsAction == true && Dis.OrgInfoId == OrgInfoId)
                            select new { Dis.mRoomMasterId, Dis.RoomName, F.FloorName, Dis.mFloorMasterId }).ToList();

            var PortsData = (from Dis in db.mReaderSettups
                             join zn in db.mZones on Dis.mZoneId equals zn.mZoneId
                             join fm in db.mFloorMasters on Dis.mFloorMasterId equals fm.mFloorMasterId
                             join rm in db.mRoomMasters on Dis.mRoomMasterId equals rm.mRoomMasterId
                             where (Dis.OrgInfoId == OrgInfoId)
                             select new { Dis.mReaderSettupId, Dis.ReaderNo, Dis.AttPortId, rm.RoomName, Dis.mRoomMasterId, zn.mZoneId, zn.Zone, fm.mFloorMasterId, fm.FloorName }).ToList();


            //Convert List Data to The Json Array                     
            return Json(new { IFloorData = FloorData, IObjData = RoomData, IPortsData = PortsData, IZoneData = ZoneData, IsubZoneData = subZoneData }, JsonRequestBehavior.AllowGet);
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
                    if (!ReaderConLost.ContainsKey(reader.Name))
                    {
                        try
                        {
                            tReaderLog obj = new tReaderLog();
                            obj.address = reader.Address;
                            obj.name = reader.Name;
                            obj.Status = false;
                            obj.Reason = "Reader stopped by application";
                            obj.IsAction = true;
                            obj.OrgInfoId = OrgInfoId;
                            obj.CreatedDate = DateTime.Now;
                            obj.CreatedBy = UserNameTrack;
                            db.tReaderLogs.Add(obj);
                            db.SaveChanges();
                            ReaderConLost.Add(reader.Name, reader.IsConnected);
                        }
                        catch (Exception ex)
                        {
                            string[] errorlines = { ex.InnerException.Message, Convert.ToString(DateTime.Now) };
                            System.IO.File.WriteAllLines(@"C:\Windows\Temp\errorlog.txt", errorlines);
                        }
                    }

                    // Don't call the Stop method if the
                    // reader is already stopped.
                    if (reader.QueryStatus().IsSingulating)
                    {

                        // Stop reading.
                        reader.Stop();

                        // Disconnect from the reader.
                        reader.Disconnect();
                        RdrInfo.Clear();
                        _hubContext.Clients.All.readerstatus("Impinj R420", RdrInfo);
                        Flag = true;
                        msg = "Reader stopped..";
                    }



                    tagsRead.Clear();
                    tagsTrack.Clear();
                    //tagsMontor.Clear();
                    //RdrInfo.Clear();
                    //tagsTrackMap.Clear();
                    //tagsMap.Clear();

                    System.IO.File.WriteAllText(Server.MapPath("~/Content/EmployeeTag.json"), null);

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
            return Json(RdrInfo, JsonRequestBehavior.AllowGet);
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
                tagsTrackMap.Clear();
                tagsMap.Clear();
                System.IO.File.WriteAllText(Server.MapPath("~/Content/EmployeeTag.json"), null);
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
        public void OnConnectionLost(ImpinjReader reader)
        {
            //RdrInfo.Clear();

            try
            {
                if (!ReaderConLost.ContainsKey(reader.Name))
                {
                    try
                    {
                        tReaderLog obj = new tReaderLog();
                        obj.address = reader.Address;
                        obj.name = reader.Name;
                        obj.Status = false;
                        obj.Reason = "Reader stopped by problem connection/power lost";
                        obj.IsAction = true;
                        obj.OrgInfoId = OrgInfoId;
                        obj.CreatedDate = DateTime.Now;
                        obj.CreatedBy = UserNameTrack;
                        db.tReaderLogs.Add(obj);
                        db.SaveChanges();
                        ReaderConLost.Add(reader.Name, reader.IsConnected);
                    }
                    catch (Exception ex)
                    {
                        string[] errorlines = { ex.InnerException.Message, Convert.ToString(DateTime.Now) };
                        System.IO.File.WriteAllLines(@"C:\Windows\Temp\errorlog.txt", errorlines);
                    }
                }




                List<ReaderInfoEntity> readerList = RdrInfo.Where(x => x.Name == reader.Name).ToList();
                readerList.ForEach(rl =>
                {

                    rl.IsConnected = false;
                    rl.IsPortConnected = false;
                    rl.sDate = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                    rl.Issingulating = false;
                    rl.Isconnected = false;
                });


                //foreach (ImpinjReader readere in readers)
                //{
                //    var ping = GetClientMac(readere.Address);

                //    if (string.IsNullOrEmpty(ping))
                //    {
                //        RdrInfo.Add(new ReaderInfoEntity
                //        {
                //            Name = readere.Name,
                //            Address = readere.Address,
                //            IsConnected = false,
                //            sDate = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                //        });
                //    }
                //    else
                //    {
                //        RdrInfo.Add(new ReaderInfoEntity
                //        {
                //            Name = readere.Name,
                //            Address = readere.Address,
                //            IsConnected = true,
                //            sDate = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                //        });
                //    }

                //}
                _hubContext.Clients.All.readerstatus("Impinj R420", RdrInfo);

                //SendMail("Reader Disconnection", "Reader Disconnection & Details Of reader", RdrInfo);
            }
            catch (Exception)
            {
                // SendMail("Reader Disconnection", ex.InnerException.Message, RdrInfo);
            }
        }
        //
        static void OnKeepaliveReceived(ImpinjReader reader)
        {
            RdrInfo.Clear();
            try
            {
                // strong type instance 
                //var _jsonObject = new JObject();

                foreach (ImpinjReader readere in readers)
                {
                    Status status = readere.QueryStatus();
                    // Antenna status               
                    foreach (AntennaStatus antStatus in status.Antennas)
                    {
                        RdrInfo.Add(new ReaderInfoEntity
                        {
                            Name = readere.Name,
                            Address = readere.Address,
                            IsConnected = status.IsConnected,
                            PortNumber = antStatus.PortNumber,
                            IsPortConnected = antStatus.IsConnected,
                            sDate = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                            Readertemperature = status.TemperatureInCelsius,
                            Issingulating = status.IsSingulating,
                            Isconnected = status.IsConnected
                        });
                    }

                    //_hubContext.Clients.All.readerstatus("Impinj R420", RdrInfo);

                    // Antenna Hub status
                    //foreach (AntennaHubStatus hubStatus in status.AntennaHubs)
                    //{
                    //    RdrInfo.Add(new ReaderInfoEntity
                    //    {
                    //        HubId = hubStatus.HubId,
                    //        Fault = hubStatus.Fault.ToString()
                    //    });
                    //}
                }

                //var _rdjson = JsonConvert.SerializeObject(RdrInfo);
                _hubContext.Clients.All.readerstatus("Impinj R420", RdrInfo);
            }
            catch (Exception)
            {

            }
        }
        //
        // This event handler gets called when an antenna event occurs.
        public void OnAntennaEvent(ImpinjReader sender, AntennaEvent e)
        {
            try
            {
                tPortChangeLog obj = new tPortChangeLog();
                obj.address = sender.Address;
                obj.name = sender.Name;
                obj.PortNumber = e.PortNumber;
                obj.ChangeStatus = e.State.ToString();
                obj.IsAction = true;
                obj.OrgInfoId = OrgInfoId;
                obj.CreatedDate = DateTime.Now;
                obj.CreatedBy = UserNameTrack;
                db.tPortChangeLogs.Add(obj);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                string[] errorlines = { ex.InnerException.Message, Convert.ToString(DateTime.Now) };
                System.IO.File.WriteAllLines(@"C:\errorlog.txt", errorlines);
            }

            var _attenaStatus = sender.Name + sender.Address + "Port : " + e.PortNumber + " State : " + e.State + "\n";
            _hubContext.Clients.All._attenaEvents("Impinj R420", _attenaStatus);

        }
        //
        public bool SendEMail(string recipient, string subject, string message)
        {

            bool isMessageSent = false;
            ////Intialise Parameters  
            //System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient("smtp@gmail.com");
            //client.Port = 587;
            //client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            //client.UseDefaultCredentials = false;
            //System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("mudassar@hexahash.com", "blore@123");
            //client.EnableSsl = true;
            //client.Credentials = credentials;
            //try
            //{
            //    var mail = new System.Net.Mail.MailMessage(senderAddress.Trim(), recipient.Trim());
            //    mail.Subject = subject;
            //    mail.Body = message;
            //    mail.IsBodyHtml = true;
            //    //System.Net.Mail.Attachment attachment;  
            //    //attachment = new Attachment(@"C:\Users\XXX\XXX\XXX.jpg");  
            //    //mail.Attachments.Add(attachment);  
            //    client.Send(mail);
            //    isMessageSent = true;
            //}
            //catch (Exception ex)
            //{
            //    isMessageSent = false;
            //}
            return isMessageSent;
        }
        //
        public string SendMail(string strSubject, string ToDisplayName, List<ReaderInfoEntity> ot)
        {
            //string Tomail = "";

            StringBuilder strBody = new StringBuilder();
            strBody.Append("<html xmlns=\"http://www.w3.org/1999/xhtml/\">");
            strBody.Append("<head><title>" + strSubject + "</title><style>td{font-weight:bold;} </style></head>");
            strBody.Append("<body style=\"font-size: 12pt; font-family: Times New Roman;\">");
            strBody.Append("<br /><font face=\"Verdana\" size=\"3\">Dear " + ToDisplayName + ",</font><br /><br />");
            strBody.Append("<table border='1' width=\"100%\" align=\"center\" style=\"font-family: sans-serif; -webkit-font-smoothing: antialiased;font-size: 12px;overflow: auto;text-align: left; border: 2px solid Gray;\">");
            strBody.Append("<tr style='background-color: #99ccff;");
            strBody.Append(" color: black;padding: 6px 10px;font-weight: bold; border-right-color: Black;border-right-width: 1px;'>");
            strBody.Append("<td><p><font face=\"Verdana\" size=\"6\">&nbsp;&nbsp;Raeder MAC</font></p>");
            strBody.Append("</td>");
            strBody.Append("<td><p><font face=\"Verdana\" size=\"6\">&nbsp;&nbsp;Raeder IP Address</font></p>");
            strBody.Append("</td>");
            strBody.Append("<td><p><font face=\"Verdana\" size=\"6\">&nbsp;&nbsp;Is Connected</font></p>");
            strBody.Append("</td>");
            strBody.Append("<td><p><font face=\"Verdana\" size=\"6\">&nbsp;&nbsp;Date</font></p>");
            strBody.Append("</td>");
            strBody.Append("</tr>");
            strBody.Append("<tr style='height: 25px;'>");

            foreach (var otd in ot)
            {
                strBody.Append("<td style='padding: 5px 10px 5px 5px;'>" + otd.Name + "</td>");
                strBody.Append("<td style='padding: 5px 10px 5px 5px;'>" + otd.Address + "</td>");
                strBody.Append("<td style='padding: 5px 10px 5px 5px;'>" + otd.IsConnected + "</td>");
                strBody.Append("<td style='padding: 5px 10px 5px 5px;'>" + otd.sDate + "</td>");
            }

            strBody.Append("</tr>");
            strBody.Append("</table><br /><br />");
            strBody.Append("</body></html>");

            try
            {
                MailMessage msg = new MailMessage();
                msg.From = new MailAddress("RFID.helpdesk@lntv.com", "RFID Notification Alert Email");
                msg.To.Add(new MailAddress("20094872@lntv.com"));
                msg.CC.Add(new MailAddress("926185@lntv.com"));
                msg.Subject = strSubject;
                msg.Body = strBody.ToString();
                //sendMail(msg);
                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        // This event handler gets called when the reader is started.
        public void OnReaderStarted(ImpinjReader reader, ReaderStartedEvent e)
        {
            try
            {
                tReaderLog obj = new tReaderLog();
                obj.address = reader.Address;
                obj.name = reader.Name;
                obj.Status = true;
                obj.Reason = "Reader started by application";
                obj.IsAction = true;
                obj.OrgInfoId = OrgInfoId;
                obj.CreatedDate = DateTime.Now;
                obj.CreatedBy = UserNameTrack;
                db.tReaderLogs.Add(obj);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                string[] errorlines = { ex.InnerException.Message, Convert.ToString(DateTime.Now) };
                System.IO.File.WriteAllLines(@"C:\errorlog.txt", errorlines);
            }

        }
        // This event handler gets called when the reader is stopped.
        public void OnReaderStopped(ImpinjReader reader, ReaderStoppedEvent e)
        {

        }
    }
}
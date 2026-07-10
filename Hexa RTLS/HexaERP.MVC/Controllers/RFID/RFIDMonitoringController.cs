using HexaERP.MVC.Models;
using Impinj.OctaneSdk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class RFIDMonitoringController : Controller
    {
        /// <summary>
        /// Author: Mudassar I
        /// </summary>
        /// <returns></returns>

        // Create an instance of the ImpinjReader class.
        static ImpinjReader reader = new ImpinjReader();

        // Create a Dictionary to store the tags we've read.
        static Dictionary<string, int> tagsRead = new Dictionary<string, int>();
        //
        static Dictionary<string, int> tagsMontor = new Dictionary<string, int>();
        static List<toMonitor> tagsTrack = new List<toMonitor>();
        //
        private ERPdbEntities db = new ERPdbEntities();
        public static String UserNameTrack; static int orgIdTrack; static string ReaderMac, _ReaderIP; static string Uid;

        // GET: RFIDMonitoring
        public ActionResult Index()
        {
            //--- Get cookie Collection.
            HttpCookie cookieObj = Request.Cookies["HexaCookie"];
            UserNameTrack = cookieObj["AppUserName"].ToString();
            orgIdTrack = Convert.ToInt32(cookieObj["OrgInfoId"]);
            tagsRead.Clear();
            tagsMontor.Clear();
            return View();
        }

        //
        [HttpGet]
        public JsonResult ReaderInit(string Reader)
        {
            string msg = ""; bool Flag = false;

            if (Reader == "")
            {
                msg = "Please Select Reader IP Address To Connect";
                Flag = false;
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
                    msg = "('An Octane SDK exception has occurred : {0}', " + ex.Message + ")";
                    Flag = false;
                }
                catch (Exception ex)
                {
                    // A general exception occurred. Handle it here.                    
                    msg = "('An Octane SDK exception has occurred : {0}', " + ex.Message + ")";
                    Flag = false;
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

                //Assin Ip
                _ReaderIP = Reader;

                Flag = true;
                msg = "Reader started...";
                //// Wait for the user to press enter.
                //// Stop reading.
                //reader.Stop();
                //// Disconnect from the reader.
                //reader.Disconnect();

                // UserNameTrack = Session["AppUserName"].ToString();
                //Get Organization Id From Session Variable
                //orgIdTrack = Convert.ToInt32(Session["OrgInfoId"]);
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
                Flag = false;
            }
            catch (Exception e)
            {
                // Handle other .NET errors.               
                msg = e.Message.ToString();
                Flag = false;
            }

            return Json(new { Flag = Flag, Message = msg, User = UserNameTrack, IP = Reader }, JsonRequestBehavior.AllowGet);
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
                    //
                    //tagsTrack.Add(new toMonitor
                    //{
                    //    RFID = tag.Epc.ToString(),
                    //    ReaderNo = ReaderMac,
                    //    mAttPortId = tag.AntennaPortNumber,
                    //    UID = Uid
                    //});
                    //
                    tagsRead.Add(tag.Epc.ToString(), tag.AntennaPortNumber);
                }

                if (!tagsMontor.ContainsKey(tag.Epc.ToString()))
                {
                    tagsMontor.Add(tag.Epc.ToString(), tag.AntennaPortNumber);
                }
            }
        }

        //Timer Method
        private void timer_Elapsed(object sender, EventArgs e)
        {
            tagsMontor.Clear();
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
                Obje.ReaderIP = _ReaderIP;
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

        //
        [HttpGet]
        public JsonResult getInvDatas()
        {

            //Get Organization Id From Session Variable
            // int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id

            var ObjReaderData = db.mReaderSettups.Where(j => j.IsAction == true).Select(c => new { c.ReaderIP, c.ReaderNo }).Distinct().ToList();
            //Convert List Data to The Json Array        

            var ObjInvData = (from Dis in db.tAssetTags
                              where (Dis.OrgInfoId == orgIdTrack && Dis.IsAction == true)
                              select new { Dis.RFID, Dis.IteamName }).ToList();
            //Convert List Data to The Json Array         

            return Json(new { ReaderData = ObjReaderData, InvData = ObjInvData }, JsonRequestBehavior.AllowGet);
        }

        //
        [HttpGet]
        public JsonResult getMonitorData()
        {

            //Get Organization Id From Session Variable
            // int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id 

            return Json(new { TodayInv = tagsRead.ToArray(), MonitorInv = tagsMontor.ToArray() }, JsonRequestBehavior.AllowGet);
        }

        //
        [HttpGet]
        public JsonResult StopReaders()
        {
            string msg = ""; bool Flag = false;
            try
            {

                // Don't call the Stop method if the
                // reader is already stopped.
                if (reader.QueryStatus().IsSingulating)
                {
                    reader.Stop();
                    Flag = true;
                    msg = "Reader stopped..";
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

            return Json(new { Flag = Flag, Message = msg, User = UserNameTrack }, JsonRequestBehavior.AllowGet);
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
            return Json(new { Flag = Flag, Message = msg, User = UserNameTrack }, JsonRequestBehavior.AllowGet);
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
    }
}
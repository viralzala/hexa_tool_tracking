using HexaERP.MVC.Models;
using Impinj.OctaneSdk;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class SpeedCalculatorController : Controller
    {
        // Create an instance of the ImpinjReader class.
        static ImpinjReader reader = new ImpinjReader();
        // Create a Dictionary to store the tags we've read.
        static Dictionary<string, int> tagsRead = new Dictionary<string, int>();

        private ERPdbEntities db = new ERPdbEntities();
        public static String UserNameTrack;
        //static int orgIdTrack;
        static string ReaderMac;
        static string Uid;
        // GET: SpeedCalculator
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
            String Key; int PortId; String EpcKey;
            // This event handler is called asynchronously 
            // when tag reports are available.
            // Loop through each tag in the report 
            // and print the data.
            foreach (Tag tag in report)
            {
                Key = tag.Epc.ToString(); PortId = tag.AntennaPortNumber;
                EpcKey = Key + "-" + PortId;
                // If this tag hasn't been read before, print out the EPC and TID
                if (!tagsRead.ContainsKey(EpcKey))
                {
                    // Add this tag to the list of tags we've read.                    
                    tagsRead.Add(EpcKey, tag.AntennaPortNumber);
                    InsertTrack(tag.Epc.ToString(), tag.AntennaPortNumber);

                }
            }
        }
        //Track RFID
        private void InsertTrack(string Key, int Val)
        {
            try
            {
                tSpeedTrack Obje = new tSpeedTrack();
                Obje.Epc = Key;
                Obje.PortId = Val;
                Obje.tDate = DateTime.Now;
                db.tSpeedTracks.Add(Obje);
                db.SaveChanges();
            }
            catch (Exception)
            {

            }


        }
        //
        [HttpGet]
        public JsonResult getGetToTrackData(int StartPort, int EndPort)
        {
            var idParam = new SqlParameter
            {
                ParameterName = "StartPort",
                Value = StartPort
            };
            var idParam1 = new SqlParameter
            {
                ParameterName = "EndPort",
                Value = EndPort
            };

            var ObjData = db.Database.SqlQuery<tSpeedTrackModel>("exec proDistance @StartPort,@EndPort ", idParam, idParam1).ToList<tSpeedTrackModel>();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //
        [HttpGet]
        public string ReaderClear()
        {
            string msg = "";

            //Convert List Data to The Json Array          
            try
            {
                tagsRead.Clear();
                //tagsMontor.Clear();
                db.Database.ExecuteSqlCommand("DELETE FROM tSpeedTrack");
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
        //Timer Method
        private void timer_Elapsed(object sender, EventArgs e)
        {
            //InsertMonitor();
            //tagsTrack.Clear();
            //tagsMontor.Clear();
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
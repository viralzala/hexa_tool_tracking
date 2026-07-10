using Impinj.OctaneSdk;
using Log.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.ManagedClient;
using Newtonsoft.Json;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Log
{
    class Program
    {
        private static IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<ReaderStatusHub>();

        static Dictionary<string, string> readersList = new Dictionary<string, string>();
        static List<ImpinjReader> readers = new List<ImpinjReader>();
        static string EpcKey, Gateway, Uid, AppUserName;
        //        
        //static List<toMonitor> tagsTrack = new List<toMonitor>();
        //static Dictionary<string, int> tagsRead = new Dictionary<string, int>();
        //static Dictionary<string, int> tagsMap = new Dictionary<string, int>();
        //static List<tToolTrackDemo> tagsTrackMap = new List<tToolTrackDemo>();

        static Dictionary<string, toMonitor> tagsMontor = new Dictionary<string, toMonitor>();
        static Dictionary<string, ushort> tagMontor = new Dictionary<string, ushort>();

        static int OrgInfoId; static double RssiVal = 90;
        static double TimerIntervalInMilliseconds, TimerIntervalInMillisecondsRefresh, TimerIntervalInMinutesMissing;
        private static int _theshholdvalue = 130;

        private static Dictionary<string, string> dicEntery
            = new Dictionary<string, string>();

        private static Dictionary<string, string> dicExit
          = new Dictionary<string, string>();

        private static string KeyPair, _epc, tempVal;

        public static Program Instance { get; } = new Program();

        static bool isReading = false;

        static object initLock = new object();

        static void OnTagsReported(ImpinjReader sender, TagReport report)
        {
            try
            {
                foreach (Tag tag in report)
                {
                    //EpcMoni = tag.Epc + "-" + sender.Name;
                    // EpcKeyt = tag.Epc + "-" + sender.Name + "-" + tag.AntennaPortNumber;
                    EpcKey = tag.Epc.ToString();

                    //double Rssi = Math.Abs(tag.PeakRssiInDbm);
                    //if (!tagsRead.ContainsKey(EpcKey))
                    //{
                    //    if (Rssi <= RssiVal)
                    //    {
                    //        tagsRead.Add(EpcKey, tag.AntennaPortNumber);
                    //    }
                    //}

                    if (!tagsMontor.ContainsKey(EpcKey))
                    {
                        tagsMontor.Add(tag.Epc.ToString(), new toMonitor
                        {
                            Epc = tag.Epc.ToString(),
                            Name = sender.Name,
                            AntennaPortNumber = Convert.ToInt32(tag.AntennaPortNumber),
                            tDate = DateTime.Now,
                            RSSI = Convert.ToString(tag.PeakRssiInDbm),
                            LastSeenTime = Convert.ToString(tag.LastSeenTime)
                        });
                    }

                    if (!tagMontor.ContainsKey(EpcKey))
                    {
                        new Thread(new ThreadStart(() =>
                        {
                            using (var db = new EricssonDBEntities())
                            {
                                db.Database
                                .ExecuteSqlCommand(@"INSERT INTO toMonitor(Epc,AntennaPortNumber,Name,Address,tDate,RSSI,LastSeenTime)Values({0},{1},{2},{3},{4},{5},{6})",
                                Convert.ToString(tag.Epc),
                                Convert.ToInt32(tag.AntennaPortNumber),
                                               sender.Name,
                                               sender.Address,
                                               DateTime.Now,
                                               Convert.ToString(tag.PeakRssiInDbm),
                                               Convert.ToString(tag.LastSeenTime));

                                db.Database
                                .ExecuteSqlCommand(@"INSERT INTO toTrackInfo(RFID,UID,AppUserName,mAttPortId,ReaderIP,ReaderNo,OrgInfoId,tDate,RSSI)Values({0},{1},{2},{3},{4},{5},{6},{7},{8})",
                                Convert.ToString(tag.Epc),
                                Uid,
                                AppUserName,
                                Convert.ToInt32(tag.AntennaPortNumber),
                                sender.Address,
                                               sender.Name,
                                               OrgInfoId,
                                               DateTime.Now,
                                               Convert.ToString(tag.PeakRssiInDbm));


                            }
                        })).Start();

                        tagMontor
                            .Add(EpcKey, tag.AntennaPortNumber);
                    }
                    //else
                    //{
                    //    if (tagMontor.TryGetValue(EpcKey, out string val))
                    //    {
                    //        //Console.WriteLine(tag.LastSeenTime);
                    //        if (sender.Address != val)
                    //        {
                    //            new Thread(new ThreadStart(() =>
                    //            {
                    //                using (var db = new EricssonDBEntities())
                    //                {
                    //                    db.Database.ExecuteSqlCommand("DELETE FROM toMonitor where Epc='{0}'", Convert.ToString(tag.Epc));
                    //                    db.Database.ExecuteSqlCommand(@"INSERT INTO toMonitor(Epc,AntennaPortNumber,Name,Address,tDate,RSSI,LastSeenTime)Values({0},{1},{2},{3},{4},{5},{6})",
                    //                            Convert.ToString(tag.Epc),
                    //                            Convert.ToInt32(tag.AntennaPortNumber),
                    //                            sender.Name,
                    //                            sender.Address,
                    //                            DateTime.Now,
                    //                            Convert.ToString(tag.PeakRssiInDbm),
                    //                            Convert.ToString(tag.LastSeenTime));


                    //                    db.Database.ExecuteSqlCommand(@"INSERT INTO toTrackInfo(RFID,UID,AppUserName,mAttPortId,ReaderIP,ReaderNo,OrgInfoId,tDate,RSSI)Values({0},{1},{2},{3},{4},{5},{6},{7},{8})",
                    //                    Convert.ToString(tag.Epc),
                    //                    Uid,
                    //                    AppUserName,
                    //                    Convert.ToInt32(tag.AntennaPortNumber),
                    //                    sender.Address,
                    //                                   sender.Name,
                    //                                   OrgInfoId,
                    //                                   DateTime.Now,
                    //                                   Convert.ToString(tag.PeakRssiInDbm));
                    //                    Console.WriteLine("Location Chnage {0}", DateTime.Now);

                    //                }
                    //            })).Start();

                    //            tagMontor.Remove(EpcKey);
                    //            tagMontor.Add(EpcKey, sender.Name);
                    //        }
                    //    }
                    //}
                    //if (!tagsMap.ContainsKey(EpcKey))
                    //{
                    //    tagsMap.Add(tag.Epc.ToString(), tag.AntennaPortNumber);
                    //    tagsTrackMap.Add(new tToolTrackDemo
                    //    {
                    //        Epc = tag.Epc.ToString(),
                    //        Reader = sender.Name,
                    //        PortId = tag.AntennaPortNumber,
                    //        tDate = DateTime.Now
                    //    });
                    //}


                }
            }
            catch (Exception ex)
            {
                tagMontor.Remove(EpcKey);
                Console.WriteLine("Exception {0} : {1}", "OnTagsReported", ex.Message);
            }
        }
        //var hubConnection = new HubConnection("http://localhost:54828/");
        private static void DisplayMenu()
        {
            Console.Clear();
            Console.WriteLine("-------Select Action--------:");
            Console.WriteLine("1) Impinj RFID");
            Console.WriteLine("2) BLE Beacons");
            Console.WriteLine("3) Hand Sanitizer");
            Console.WriteLine("4) Niruha");
            Console.WriteLine("5) Reboot");
            Console.WriteLine("6) Clear");
            Console.WriteLine("7) Exit");
            Console.Write("\r\nSelect an option: ");
        }
        static void Main(string[] args)
        {
            try
            {
                Console.Clear();
                int menuchoice = 0;
                while (true)
                {
                    DisplayMenu();

                    menuchoice = int.Parse(Console.ReadLine());

                    if (menuchoice == null)
                    {
                        Console.WriteLine("Sorry, invalid selection");
                    }

                    switch (menuchoice)
                    {
                        case 1:
                            Settings();
                            OnConnectAsync();
                            break;
                        case 2:
                            BleMonitoringAsync().Wait();
                            break;
                        case 3:
                            InitiateBleAsync().Wait();
                            HandSanitizer();
                            break;
                        case 4:
                            BleMonitoringniruha().Wait();
                            break;
                        case 5:
                            RebootReaders();
                            break;
                        case 6:
                            Console.Clear();
                            DisplayMenu();
                            break;
                        case 7:
                            break;
                        default:
                            Console.WriteLine("Sorry, invalid selection");
                            DisplayMenu();
                            break;
                    }
                    Console.ReadLine();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception {0} : {1}", "Main", ex.Message);
            }
        }
        private static async Task InitiateBleAsync()
        {
            try
            {
                Uid = DateTime.Now.ToString().GetHashCode().ToString("x");
                OrgInfoId = 1037;
                AppUserName = "BLE App";
                Console.WriteLine(string.Format("SESSION UID :{0}", Uid));
                _theshholdvalue = Convert.ToInt32(ConfigurationManager.AppSettings["Mqtt:Thresholdvalue"]);
                var _test = ConfigurationManager.AppSettings["Mqtt:Server"];
                var options = new ManagedMqttClientOptionsBuilder()
                        .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                        .WithClientOptions(new MqttClientOptionsBuilder()
                            .WithClientId(Guid.NewGuid().ToString())
                             .WithTcpServer(ConfigurationManager.AppSettings["Mqtt:Server"], Convert.ToInt32(ConfigurationManager.AppSettings["Mqtt:Port"]))
                              .WithCredentials(ConfigurationManager.AppSettings["Mqtt:Username"], ConfigurationManager.AppSettings["Mqtt:Password"])
                            .Build())
                        .Build();

                var mqttClient = new MqttFactory().CreateManagedMqttClient();
                string[] _topiclist
                   = ConfigurationManager.AppSettings["Mqtt:Topics"].Split(',');

                foreach (var topic in _topiclist)
                {
                    await mqttClient.SubscribeAsync(new TopicFilterBuilder()
                  .WithTopic(topic).Build());
                }
                mqttClient.Connected +=
                    (s, e) => Console.WriteLine($"{DateTime.Now.ToShortTimeString()} connected.");
                mqttClient.Disconnected +=
                    (s, e) => Console.WriteLine($"{DateTime.Now.ToShortTimeString()} disconnected");

                //mqttClient.ApplicationMessageReceived +=
                //    (s, e) => ApplicationPlayLoadUpdate(Encoding.UTF8.GetString(e.ApplicationMessage.Payload)); //Debug.WriteLine($"{DateTime.Now.ToShortTimeString()} received: {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");

                await mqttClient.StartAsync(options);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Exception: {0} InnerException :{1}",
                    ex.Message,
                    ex.InnerException.Message));
            }
        }
        private static void HandSanitizer()
        {
            try
            {
                using (EricssonDBEntities contx = new EricssonDBEntities())
                {
                    readersList = contx.mReaderSettups.Where(j => j.IsAction == true)
                             .Select(x => new { x.ReaderIP, x.ReaderNo }).Distinct().ToDictionary(x => x.ReaderIP, x => x.ReaderNo);
                    Console.WriteLine("    (Reader)\n");
                    Console.WriteLine(contx.mReaderSettups.Distinct().Select(s => new
                    {
                        s.ReaderIP,
                        s.ReaderNo
                    }).ToMarkdownTable());

                    try
                    {
                        if (readersList != null)
                        {
                            foreach (KeyValuePair<string, string> entry in readersList)
                                readers.Add(new ImpinjReader(entry.Key, entry.Value));
                            foreach (ImpinjReader reader in readers)
                            {

                                ConnectToReader(reader);
                                ConfigureReader(reader);
                            }

                            Thread.Sleep(1000);
                            Console.WriteLine("Triger: {0}", DateTime.Now);

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception {0} : {1}", "OnConnectAsync", ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Exception: {0} InnerException :{1}",
                    ex.Message,
                    ex.InnerException.Message));
            }
        }
        private static void ConfigureReader(ImpinjReader reader)
        {
            try
            {

                // Get the default settings.
                // We'll use these as a starting point
                // and then modify the settings we're 
                // interested in.
                Settings settings = reader.QueryDefaultSettings();

                // Start the reader as soon as it's configured.
                // This will allow it to run without a client connected.
                settings.AutoStart.Mode = AutoStartMode.Immediate;
                settings.AutoStop.Mode = AutoStopMode.None;

                settings.Report.IncludePeakRssi = true;
                settings.Report.IncludeAntennaPortNumber = true;
                settings.Report.IncludeLastSeenTime = true;

                //settings.ReaderMode = ReaderMode.AutoSetDenseReaderDeepScan;
                //settings.SearchMode = SearchMode.DualTarget;
                //settings.Session = 2;

                settings.ReaderMode = ReaderMode.AutoSetDenseReader;
                settings.SearchMode = SearchMode.DualTarget;
                settings.Session = 2;

                // Tell the reader to include the timestamp in all tag reports.
                //settings.Report.IncludeFirstSeenTime = true;
                //settings.Report.IncludeLastSeenTime = true;
                //settings.Report.IncludeSeenCount = true;

                // If this application disconnects from the 
                // reader, hold all tag reports and events.
                settings.HoldReportsOnDisconnect = true;

                // Enable keepalives.
                settings.Keepalives.Enabled = true;
                settings.Keepalives.PeriodInMs = 60000;

                // Enable link monitor mode.
                // If our application fails to reply to
                // five consecutive keepalive messages,
                // the reader will close the network connection.
                settings.Keepalives.EnableLinkMonitorMode = true;
                settings.Keepalives.LinkDownThreshold = 5;

                // Assign an event handler that will be called
                // when keepalive messages are received.
                reader.KeepaliveReceived += OnKeepaliveReceived;

                // Assign an event handler that will be called
                // if the reader stops sending keepalives.
                reader.ConnectionLost += OnConnectionLost;

                // Apply the newly modified settings.
                reader.ApplySettings(settings);

                // Save the settings to the reader's 
                // non-volatile memory. This will
                // allow the settings to persist
                // through a power cycle.
                //reader.SaveSettings();

                reader.ApplySettings(settings);
                Console.WriteLine("Applying Settings...");
                Thread.Sleep(1000);
                Console.ForegroundColor = reader.IsConnected ? ConsoleColor.DarkGreen : ConsoleColor.Red;
                Console.WriteLine("Applyed: {0}", DateTime.Now);
                Console.ResetColor();

                // Assign the TagsReported event handler.
                // This specifies which method to call
                // when tags reports are available.
                reader.TagsReported += OnTagsReported_st;


            }
            catch (OctaneSdkException e)
            {
                // Handle Octane SDK errors.
                Console.WriteLine("Octane SDK exception: {0}", e.Message);
            }
            catch (Exception e)
            {
                // Handle other .NET errors.
                Console.WriteLine("Exception : {0}", e.Message);
            }
        }
        static void OnTagsReported_st(ImpinjReader sender, TagReport report)
        {
            foreach (Tag tag in report)
            {
                EpcKey = tag.Epc.ToString();
                if (!tagMontor.ContainsKey(EpcKey))
                {
                    using (var db = new EricssonDBEntities())
                    {
                        string emp = db.Database.SqlQuery<string>("select * from tEmployeeTag where RFID={0}", EpcKey)
                             .FirstOrDefault();
                        if (emp == null)
                        {

                        }
                    }
                    tagMontor
                           .Add(EpcKey, tag.AntennaPortNumber);
                }
                Console.WriteLine("EPC : {0} Timestamp : {1}", tag.Epc, tag.LastSeenTime);
            }
        }
        static void OnConnectionLost(ImpinjReader reader)
        {
            // This event handler is called if the reader  
            // stops sending keepalive messages (connection lost).
            Console.WriteLine("Connection lost : {0} ({1})", reader.Name, reader.Address);

            // Cleanup
            reader.Disconnect();

            // Try reconnecting to the reader
            ConnectToReader(reader);
        }

        static void ConnectToReader(ImpinjReader reader)
        {
            try
            {
                Console.WriteLine("Attempting to connect to {0} ({1}).",
                    reader.Name, reader.Address);

                // The maximum number of connection attempts
                // before throwing an exception.
                reader.MaxConnectionAttempts = 15;
                // Number of milliseconds before a 
                // connection attempt times out.
                reader.ConnectTimeout = 6000;
                // Connect to the reader.
                // Change the ReaderHostname constant in SolutionConstants.cs 
                // to the IP address or hostname of your reader.
                reader.Connect(reader.Address);
                Console.WriteLine("Successfully connected.");

                // Tell the reader to send us any tag reports and 
                // events we missed while we were disconnected.
                reader.ResumeEventsAndReports();
            }
            catch (OctaneSdkException e)
            {
                Console.WriteLine("Failed to connect.");
                throw e;
            }
        }
        private static async Task BleMonitoringniruha()
        {
            Uid = DateTime.Now.ToString().GetHashCode().ToString("x");
            OrgInfoId = 1037;
            AppUserName = "BLE App";
            Console.WriteLine(string.Format("SESSION UID :{0}", Uid));
            _theshholdvalue = Convert.ToInt32(ConfigurationManager.AppSettings["Mqtt:Thresholdvalue"]);
            var _test = ConfigurationManager.AppSettings["Mqtt:Server"];
            var options = new ManagedMqttClientOptionsBuilder()
                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                    .WithClientOptions(new MqttClientOptionsBuilder()
                        .WithClientId(Guid.NewGuid().ToString())
                         .WithTcpServer(ConfigurationManager.AppSettings["Mqtt:Server"], Convert.ToInt32(ConfigurationManager.AppSettings["Mqtt:Port"]))
                          .WithCredentials(ConfigurationManager.AppSettings["Mqtt:Username"], ConfigurationManager.AppSettings["Mqtt:Password"])
                        .Build())
                    .Build();

            var mqttClient = new MqttFactory().CreateManagedMqttClient();
            string[] _topiclist
               = ConfigurationManager.AppSettings["Mqtt:Topics"].Split(',');
            foreach (var topic in _topiclist)
            {
                await mqttClient.SubscribeAsync(new TopicFilterBuilder()
              .WithTopic(topic).Build());
            }
            mqttClient.Connected +=
                (s, e) => Console.WriteLine($"{DateTime.Now.ToShortTimeString()} connected.");
            mqttClient.Disconnected +=
                (s, e) => Console.WriteLine($"{DateTime.Now.ToShortTimeString()} disconnected");
            mqttClient.ApplicationMessageReceived +=
                (s, e) => ApplicationPlayLoadNirua(Encoding.UTF8.GetString(e.ApplicationMessage.Payload)); //Debug.WriteLine($"{DateTime.Now.ToShortTimeString()} received: {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            await mqttClient.StartAsync(options);

            TimerIntervalInMilliseconds =
                     Convert.ToDouble(ConfigurationManager.AppSettings["TimerIntervalInMilliseconds"]);
            TimerIntervalInMillisecondsRefresh =
                Convert.ToDouble(ConfigurationManager.AppSettings["TimerIntervalInMillisecondsRefresh"]);
            TimerIntervalInMinutesMissing =
                Convert.ToDouble(ConfigurationManager.AppSettings["TimerIntervalInMinutesMissing"]);

            //System.Timers.Timer timer2 = new System.Timers.Timer();
            //timer2.Interval = TimerIntervalInMilliseconds;
            //timer2.Elapsed += bletimer_Triger;
            //timer2.Start();
        }
        private static async Task BleMonitoringAsync()
        {
            Uid = DateTime.Now.ToString().GetHashCode().ToString("x");
            OrgInfoId = 1037;
            AppUserName = "BLE App";
            Console.WriteLine(string.Format("SESSION UID :{0}", Uid));
            _theshholdvalue = Convert.ToInt32(ConfigurationManager.AppSettings["Mqtt:Thresholdvalue"]);
            var _test = ConfigurationManager.AppSettings["Mqtt:Server"];
            var options = new ManagedMqttClientOptionsBuilder()
                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                    .WithClientOptions(new MqttClientOptionsBuilder()
                        .WithClientId(Guid.NewGuid().ToString())
                         .WithTcpServer(ConfigurationManager.AppSettings["Mqtt:Server"], Convert.ToInt32(ConfigurationManager.AppSettings["Mqtt:Port"]))
                          .WithCredentials(ConfigurationManager.AppSettings["Mqtt:Username"], ConfigurationManager.AppSettings["Mqtt:Password"])
                        .Build())
                    .Build();

            var mqttClient = new MqttFactory().CreateManagedMqttClient();
            string[] _topiclist
               = ConfigurationManager.AppSettings["Mqtt:Topics"].Split(',');
            foreach (var topic in _topiclist)
            {
                await mqttClient.SubscribeAsync(new TopicFilterBuilder()
              .WithTopic(topic).Build());
            }
            mqttClient.Connected +=
                (s, e) => Console.WriteLine($"{DateTime.Now.ToShortTimeString()} connected.");
            mqttClient.Disconnected +=
                (s, e) => Console.WriteLine($"{DateTime.Now.ToShortTimeString()} disconnected");
            mqttClient.ApplicationMessageReceived +=
                (s, e) => ApplicationPayLoad(Encoding.UTF8.GetString(e.ApplicationMessage.Payload)); //Debug.WriteLine($"{DateTime.Now.ToShortTimeString()} received: {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            await mqttClient.StartAsync(options);

            TimerIntervalInMilliseconds =
                     Convert.ToDouble(ConfigurationManager.AppSettings["TimerIntervalInMilliseconds"]);
            TimerIntervalInMillisecondsRefresh =
                Convert.ToDouble(ConfigurationManager.AppSettings["TimerIntervalInMillisecondsRefresh"]);
            TimerIntervalInMinutesMissing =
                Convert.ToDouble(ConfigurationManager.AppSettings["TimerIntervalInMinutesMissing"]);

            System.Timers.Timer timer2 = new System.Timers.Timer();
            timer2.Interval = TimerIntervalInMilliseconds;
            timer2.Elapsed += bletimer_Triger;
            timer2.Start();
        }

        private static void ApplicationPayLoad(string log)
        {
            try
            {
                if (!isReading)
                    lock (initLock)
                        if (!isReading)
                        {
                            var configuration = JsonConvert.DeserializeObject<MqttTopicModel>(log, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                            if (configuration != null && configuration.obj != null)
                            {
                                foreach (var i in configuration.obj)
                                {
                                    Gateway = configuration.gmac;

                                    KeyPair = $"{i.dmac} {configuration.gmac}";

                                    new Thread(new ThreadStart(() =>
                                    {
                                        try
                                        {
                                            _ = PushDataSqlight(Gateway, i, "Live");
                                        }
                                        catch (Exception) { }

                                    })).Start();

                                    //if (!dicEntery.ContainsValue(KeyPair))
                                    //{
                                    //    if (dicEntery.ContainsKey(i.dmac))
                                    //        dicEntery.Remove(i.dmac);

                                    //    dicEntery.Add(i.dmac, KeyPair);


                                    //}
                                }
                            }
                        }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //
        private static void ApplicationPlayLoadUpdate(string log)
        {
            try
            {
                Console.WriteLine(log);

                string[] eachObj = log.Trim().Split(',');
                tempVal = Convert.ToString(eachObj[4]);
                _epc = Convert.ToString(eachObj[1]);
                KeyPair = eachObj[1] + eachObj[2];
                Console.WriteLine(eachObj[1]);
                //if (Math.Abs(Convert.ToInt32(eachObj[3])) <= _theshholdvalue)
                //{
                if (!dicEntery.ContainsValue(KeyPair))
                {
                    if (dicEntery.ContainsKey(_epc))
                        dicEntery.Remove(_epc);

                    //if (dicExit.ContainsKey(_epc))
                    //    dicExit.Remove(_epc);

                    dicEntery.Add(_epc, KeyPair);

                    new Thread(new ThreadStart(() =>
                    {
                        try
                        {
                            ///pushDataSqlight(eachObj);
                        }
                        catch (Exception) { }

                    })).Start();


                    //dicExit.Add(_epc, tempVal);
                    //if (dicExit.Count > 0)
                    //{
                    //    pushDataSqlight(eachObj);
                    //}
                }
                else
                {
                    updateDataSqlight(eachObj).Wait();
                }
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void ApplicationPlayLoadNirua(string log)
        {
            try
            {


                var jsonObj = JsonConvert.DeserializeObject<NiruaData>(log);
                //Console.WriteLine(jsonObj.scnItms);
                foreach (var listI in jsonObj.scnItms)
                {
                    //string[] eachObj = log.Trim().Split(',');
                    // tempVal = Convert.ToString(eachObj[4]);
                    _epc = Convert.ToString(listI.mac);
                    KeyPair = listI.mac + jsonObj.dev;
                    Console.WriteLine(listI.mac);
                    if (Math.Abs(Convert.ToInt32(listI.rssi)) <= _theshholdvalue)
                    {
                        if (!dicEntery.ContainsValue(KeyPair))
                        {
                            if (dicEntery.ContainsKey(_epc))
                                dicEntery.Remove(_epc);

                            //if (dicExit.ContainsKey(_epc))
                            //    dicExit.Remove(_epc);

                            dicEntery.Add(_epc, KeyPair);
                            pushDataSqlightNirua(listI, jsonObj.dev);
                            //dicExit.Add(_epc, tempVal);
                            //if (dicExit.Count > 0)
                            //{
                            //    pushDataSqlight(eachObj);
                            //}
                        }
                        else
                        {
                            updateDataSqlightNirua(listI, jsonObj.dev).Wait();
                        }
                    }
                }
                ;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        //

        public class NiruaData
        {
            public string subID { get; set; }
            public string tkn { get; set; }
            public string pri { get; set; }
            public string dev { get; set; }
            public string evt { get; set; }
            public string time { get; set; }
            public List<scnItms> scnItms { get; set; }

        }
        public class scnItms
        {
            public string mac { get; set; }
            public string adv { get; set; }
            public decimal rssi { get; set; }
            public string scnTime { get; set; }
        }
        private static async Task PushDataSqlight(string _gateway, Obj obj, string status)
        {
            try
            {
                var readersql = @"UPDATE toMonitor SET LastSeenTime=@LastSeenTime where Name=@Name";
                var sql = @"UPDATE toMonitor SET AntennaPortNumber=@AntennaPortNumber,Name=@Name,tDate=GETDATE(),RSSI=@RSSI,LastSeenTime=@LastSeenTime where Epc=@Epc";


                using (var db = new EricssonDBEntities())
                {

                    object[] par = {
                        new SqlParameter("@Epc",obj.dmac),
                        new SqlParameter("@AntennaPortNumber", obj.vbatt),
                        new SqlParameter("@Name",_gateway),
                        new SqlParameter("@Address","-"),
                        new SqlParameter("@RSSI",Convert.ToString(obj.rssi)),
                        new SqlParameter("@LastSeenTime", status)
                    };

                    await db.Database.ExecuteSqlCommandAsync(readersql, par);
                    var ins = await db.Database.ExecuteSqlCommandAsync(sql, par);
                    if (ins == 0)
                    {
                        var usql = @"INSERT INTO toMonitor(Epc,AntennaPortNumber,Name,tDate,RSSI,LastSeenTime)Values(@Epc,@AntennaPortNumber,@Name,GETDATE(),@RSSI,@LastSeenTime)";
                        var _ = await db.Database.ExecuteSqlCommandAsync(usql, par);
                    }
                    try
                    {
                        object[] dpar = { new SqlParameter("@Epc", obj.dmac) };
                        var dAlert = $"DELETE FROM toTrackInfo where RFID=@Epc;";
                        await db.Database.ExecuteSqlCommandAsync(dAlert, dpar);
                    }
                    catch (Exception) { }

                    //string p1 = db.Database
                    //    .SqlQuery<string>("SELECT Epc FROM toMonitor Where Epc={0}", ary[1])
                    //    .FirstOrDefault();

                    //if (p1 == null)
                    //{
                    //    Console.WriteLine(string.Format(@"Device :{0} Address:{1} RSSI: {2} Datetime:{3}", ary[2], ary[1], ary[3], DateTime.Now));
                    //    db.Database.ExecuteSqlCommand(@"INSERT INTO toMonitor(Epc,AntennaPortNumber,Name,tDate,RSSI)Values({0},{1},{2},{3},{4})",
                    //                  Convert.ToString(ary[1]),
                    //                  Convert.ToInt32(0),
                    //                                 ary[2],
                    //                                 DateTime.Now,
                    //                                 Convert.ToString(ary[3]));

                    //    db.Database.ExecuteSqlCommand(@"INSERT INTO toTrackInfo(RFID,UID,AppUserName,mAttPortId,ReaderNo,OrgInfoId,tDate,RSSI)Values({0},{1},{2},{3},{4},{5},{6},{7})",
                    //    Convert.ToString(ary[1]),
                    //    Uid,
                    //   AppUserName,
                    //    Convert.ToInt32(0),
                    //                   ary[2],
                    //                   OrgInfoId,
                    //                   DateTime.Now,
                    //                   Convert.ToString(ary[3]));
                    //}

                    //else
                    //{
                    //    updateDataSqlight(ary);
                    //}
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        //
        private static async Task updateDataSqlight(string[] ary)
        {
            try
            {
                //Console.WriteLine(string.Format(@"Device :{0} Address:{1} RSSI: {2} Datetime:{3}", ary[2], ary[1], ary[3], DateTime.Now));
                using (var db = new EricssonDBEntities())
                {
                    await db.Database.ExecuteSqlCommandAsync(@"UPDATE toMonitor SET AntennaPortNumber={0},Name={1},tDate={2},RSSI={3} where Epc={4}",
                                    0,
                                    ary[2],
                                    DateTime.Now,
                                    Convert.ToString(ary[3]),
                                    ary[1]);
                    //Console.WriteLine("Updated {0} : {1}", DateTime.Now, _flag);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void pushDataSqlightNirua(scnItms scnItms, string dev)
        {
            try
            {
                using (var db = new EricssonDBEntities())
                {
                    string p1 = db.Database.SqlQuery<string>("SELECT Epc FROM toMonitor Where Epc={0}", scnItms.mac)
                           .FirstOrDefault();
                    if (p1 == null)
                    {
                        Console.WriteLine(string.Format(@"Device :{0} Address:{1} RSSI: {2} Datetime:{3}", dev, scnItms.mac, scnItms.rssi, DateTime.Now));
                        db.Database.ExecuteSqlCommand(@"INSERT INTO toMonitor(Epc,AntennaPortNumber,Name,tDate,RSSI)Values({0},{1},{2},{3},{4})",
                                      Convert.ToString(scnItms.mac),
                                      Convert.ToInt32(0),
                                                     dev,
                                                     DateTime.Now,
                                                     Convert.ToString(scnItms.rssi));

                        db.Database.ExecuteSqlCommand(@"INSERT INTO toTrackInfo(RFID,UID,AppUserName,mAttPortId,ReaderNo,OrgInfoId,tDate,RSSI)Values({0},{1},{2},{3},{4},{5},{6},{7})",
                        Convert.ToString(scnItms.mac),
                        Uid,
                       AppUserName,
                        Convert.ToInt32(0),
                                       dev,
                                       OrgInfoId,
                                       DateTime.Now,
                                       Convert.ToString(scnItms.rssi));
                    }
                    //else
                    //{
                    //    updateDataSqlight(ary);
                    //}
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        //
        private static async Task updateDataSqlightNirua(scnItms scnItms, string dev)
        {
            try
            {
                //Console.WriteLine(string.Format(@"Device :{0} Address:{1} RSSI: {2} Datetime:{3}", ary[2], ary[1], ary[3], DateTime.Now));
                using (var db = new EricssonDBEntities())
                {
                    await db.Database.ExecuteSqlCommandAsync(@"UPDATE toMonitor SET AntennaPortNumber={0},Name={1},tDate={2},RSSI={3} where Epc={4}",
                                    0,
                                    dev,
                                    DateTime.Now,
                                    Convert.ToString(scnItms.rssi),
                                    scnItms.mac);
                    //Console.WriteLine("Updated {0} : {1}", DateTime.Now, _flag);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        //
        private static void bletimer_Triger(object sender, ElapsedEventArgs e)
        {
            try
            {
                var _reader = ConfigurationManager.AppSettings["Smt:SearchReader"];

                isReading = true;

                try
                {
                    new Thread(new ThreadStart(() =>
                    {
                        dicEntery.ToList().ForEach(m =>
                        {
                            using (var db = new EricssonDBEntities())
                            {
                                db.Database.ExecuteSqlCommand("DELETE FROM toMonitor Where Name={0}", new object[] { _reader });
                                //db.Database.ExecuteSqlCommand("DEL toMonitor SET LastSeenTime='Off' where Name = @p0", _reader);
                            }
                        });
                    })).Start();

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception {0} : {1}", "timer_Triger", ex.Message);
                }




                //using (var db = new EricssonDBEntities())
                //{
                //    DateTime TimeNow = DateTime.Now;
                //    db.toMonitors.ToList().ForEach(m =>
                //    {
                //        TimeSpan interval = TimeNow - m.tDate;
                //        if (interval.Minutes >= TimerIntervalInMinutesMissing)
                //        {
                //            //var _asset = db.tAssetTags
                //            // .Where(x =>
                //            // x.IsAction == true
                //            // && x.IsAllocated == false
                //            // && x.IsAssetRefresh == false
                //            // && x.IsAssetTransfer == false
                //            // && x.IsBreakFix == false
                //            // && x.IsLoan == false
                //            // && x.IsMail == false
                //            // && x.Address == m.Epc
                //            // ).FirstOrDefault();
                //            db.Database.ExecuteSqlCommand(@"DELETE FROM toMonitor where Epc={0}", m.Epc);
                //            Console.WriteLine(String.Format("Asset Missing {0} DateTime :{1}", m.Epc, DateTime.Now));
                //            // Debug.WriteLine(String.Format("Asset Missing {0}", m.Epc));
                //            dicEntery.Remove(m.Epc);
                //            //dicExit.Remove(m.Epc);
                //            //if (_asset != null)
                //            //{
                //            //    MissingAsset(_asset, m.tDate);
                //            //    db.Database.ExecuteSqlCommand(@"UPDATE tAssetTag SET IsMail={0} where Address={1}", true, m.Address);
                //            //}
                //        }
                //    });
                //}                //using (var db = new EricssonDBEntities())
                //{
                //    DateTime TimeNow = DateTime.Now;
                //    db.toMonitors.ToList().ForEach(m =>
                //    {
                //        TimeSpan interval = TimeNow - m.tDate;
                //        if (interval.Minutes >= TimerIntervalInMinutesMissing)
                //        {
                //            //var _asset = db.tAssetTags
                //            // .Where(x =>
                //            // x.IsAction == true
                //            // && x.IsAllocated == false
                //            // && x.IsAssetRefresh == false
                //            // && x.IsAssetTransfer == false
                //            // && x.IsBreakFix == false
                //            // && x.IsLoan == false
                //            // && x.IsMail == false
                //            // && x.Address == m.Epc
                //            // ).FirstOrDefault();
                //            db.Database.ExecuteSqlCommand(@"DELETE FROM toMonitor where Epc={0}", m.Epc);
                //            Console.WriteLine(String.Format("Asset Missing {0} DateTime :{1}", m.Epc, DateTime.Now));
                //            // Debug.WriteLine(String.Format("Asset Missing {0}", m.Epc));
                //            dicEntery.Remove(m.Epc);
                //            //dicExit.Remove(m.Epc);
                //            //if (_asset != null)
                //            //{
                //            //    MissingAsset(_asset, m.tDate);
                //            //    db.Database.ExecuteSqlCommand(@"UPDATE tAssetTag SET IsMail={0} where Address={1}", true, m.Address);
                //            //}
                //        }
                //    });
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                dicEntery.Clear();
                isReading = false;
            }
        }
        //
        public void SaveOrUpdate(string[] ary)
        {
            using (var db = new EricssonDBEntities())
            {
                var sql = @"MERGE INTO MyEntity
                USING 
                (
                   SELECT   @id as Id
                            @myField AS MyField
                ) AS entity
                ON  MyEntity.Id = entity.Id
                WHEN MATCHED THEN
                    UPDATE 
                    SET     Id = @id
                            MyField = @myField
                WHEN NOT MATCHED THEN
                    INSERT (Id, MyField)
                    VALUES (@Id, @myField);";

                object[] parameters = {
        new SqlParameter("@id", ary[0]),
        new SqlParameter("@myField", ary[1])    };
                db.Database.ExecuteSqlCommand(sql, parameters);
            }
        }
        //
        private static void ApplicationPlayLoad(string log)
        {
            try
            {
                string[] eachObj = log.Trim().Split(',');
                tempVal = Convert.ToString(eachObj[4]);
                _epc = Convert.ToString(eachObj[1]);
                KeyPair = $"{eachObj[1]}_{eachObj[2]}";
                //Console.WriteLine(string.Format(@"Counts :{0}", log.Trim().Split(' ').Length));

                if (Math.Abs(Convert.ToInt32(eachObj[3])) <= _theshholdvalue)
                {
                    if (!dicEntery.ContainsValue(KeyPair))
                    {
                        //Console.WriteLine(KeyPair);
                        if (dicEntery.ContainsKey(_epc))
                            dicEntery.Remove(_epc);

                        if (dicExit.ContainsKey(_epc))
                            dicExit.Remove(_epc);

                        dicEntery.Add(_epc, KeyPair);
                        dicExit.Add(_epc, tempVal);

                        if (dicExit.Count > 0)
                        {
                            ///pushDataSqlight(eachObj);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        class Startup
        {
            public void Configuration(IAppBuilder app)
            {
                app.UseCors(CorsOptions.AllowAll);
                app.MapSignalR();
            }
        }
        public class ReaderStatusHub : Hub
        {
            //public void _li(string name, string message)
            //{
            //    Clients.All.livetimer(name, message);
            //}
        }
        static void Settings()
        {
            try
            {
                OrgInfoId = 1037;
                AppUserName = "Console App";
                Uid = DateTime.Now.ToString().GetHashCode().ToString("x");

                using (EricssonDBEntities contx = new EricssonDBEntities())
                {
                    Console.WriteLine("Clean Up Tags {0}", contx.Database.ExecuteSqlCommand("DELETE FROM toMonitor"));

                    readersList = contx.mReaderSettups.Where(j => j.IsAction == true)
                             .Select(x => new { x.ReaderIP, x.ReaderNo }).Distinct().ToDictionary(x => x.ReaderIP, x => x.ReaderNo);
                    //readersList.Add("192.168.1.10", "R420");
                    //readersList.Add("192.168.1.11", "XSpan");
                    foreach (KeyValuePair<string, string> entry in readersList)
                        Console.WriteLine("Active Reader List {0} : {1}", entry.Value, entry.Key);
                }

                TimerIntervalInMilliseconds =
                    Convert.ToDouble(ConfigurationManager.AppSettings["TimerIntervalInMilliseconds"]);
                TimerIntervalInMillisecondsRefresh =
                    Convert.ToDouble(ConfigurationManager.AppSettings["TimerIntervalInMillisecondsRefresh"]);
                TimerIntervalInMinutesMissing =
                    Convert.ToDouble(ConfigurationManager.AppSettings["TimerIntervalInMinutesMissing"]);
                //_path = @ConfigurationManager.AppSettings["filePath"];

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception {0} : {1}", "Settings", ex.Message);
            }
        }
        static void OnConnectAsync()
        {
            try
            {
                if (readersList != null)
                {
                    //reader.ConnectAsyncComplete += OnConnectAsyncComplete;
                    //reader.TagsReported += OnTagsReported;
                    foreach (KeyValuePair<string, string> entry in readersList)
                        readers.Add(new ImpinjReader(entry.Key, entry.Value));
                    foreach (ImpinjReader reader in readers)
                    {
                        reader.Connect();
                        Settings settings = reader.QueryDefaultSettings();
                        settings.AutoStart.Mode = AutoStartMode.Immediate;
                        settings.Report.IncludePeakRssi = true;
                        settings.Report.IncludeAntennaPortNumber = true;
                        settings.Report.IncludeLastSeenTime = true;

                        //settings.ReaderMode = ReaderMode.AutoSetDenseReaderDeepScan;
                        //settings.SearchMode = SearchMode.DualTarget;
                        //settings.Session = 2;

                        settings.ReaderMode = ReaderMode.AutoSetDenseReader;
                        settings.SearchMode = SearchMode.DualTarget;
                        settings.Session = 2;

                        settings.Keepalives.Enabled = true;
                        settings.Keepalives.PeriodInMs = 60000;
                        settings.Keepalives.EnableLinkMonitorMode = true;
                        settings.Keepalives.LinkDownThreshold = 5;

                        reader.ApplySettings(settings);
                        Console.WriteLine("Applying Settings...");
                        Thread.Sleep(1000);
                        Console.ForegroundColor = reader.IsConnected ? ConsoleColor.DarkGreen : ConsoleColor.Red;
                        Console.WriteLine("Applyed: {0}", DateTime.Now);
                        Console.ResetColor();
                        reader.TagsReported += OnTagsReported;
                        reader.KeepaliveReceived += OnKeepaliveReceived;

                    }

                    Thread.Sleep(1000);
                    Console.WriteLine("Triger: {0}", DateTime.Now);

                    //*%%*
                    System.Timers.Timer timer2 = new System.Timers.Timer();
                    timer2.Interval = TimerIntervalInMilliseconds;
                    timer2.Elapsed += timer_Triger;
                    timer2.Start();

                    System.Timers.Timer timer = new System.Timers.Timer();
                    timer.Interval = TimerIntervalInMillisecondsRefresh;
                    timer.Elapsed += timer_Elapsed;
                    timer.Start();
                    //DisplayCurrentSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception {0} : {1}", "OnConnectAsync", ex.Message);
            }
        }
        private static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (tagsMontor.ToList().Count() > 0)
                {
                    tagsMontor.ToList().ForEach(x =>
                    {
                        using (var db = new EricssonDBEntities())
                        {
                            db.Database.ExecuteSqlCommand(@"UPDATE toMonitor SET AntennaPortNumber={0},Name={1},Address={2},tDate={3},LastSeenTime={4} where Epc={5}",
                                            x.Value.AntennaPortNumber,
                                            x.Value.Name,
                                            x.Value.Address,
                                            x.Value.tDate,
                                            x.Value.LastSeenTime,
                                            x.Value.Epc);
                            //Console.WriteLine("Updated {0} : {1}", DateTime.Now, _flag);
                        }
                    });
                    tagsMontor.Clear();
                }
                //new Thread(new ThreadStart(() =>
                //{                    
                //})).Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception {0} : {1}", "timer_Elapsed", ex.Message);
            }
        }
        private static void timer_Triger(object sender, ElapsedEventArgs e)
        {
            try
            {
                new Thread(new ThreadStart(() =>
                {
                    using (var db = new EricssonDBEntities())
                    {
                        DateTime TimeNow = DateTime.Now;
                        db.toMonitors.ToList().ForEach(m =>
                        {
                            TimeSpan interval = TimeNow - m.tDate;
                            if (interval.Minutes >= TimerIntervalInMinutesMissing)
                            {
                                //var _asset = db.tAssetTags
                                // .Where(x =>
                                // x.IsAction == true
                                // && x.IsAllocated == false
                                // && x.IsAssetRefresh == false
                                // && x.IsAssetTransfer == false
                                // && x.IsBreakFix == false
                                // && x.IsLoan == false
                                // && x.IsMail == false
                                // && x.RFID == m.Epc).FirstOrDefault();
                                //db.Database.ExecuteSqlCommand(@"DELETE FROM toMonitor where Epc={0}", m.Epc);
                                //Console.WriteLine("Asset Missing {0}", m.Epc);
                                //if (_asset != null)
                                //{
                                //    MissingAsset(_asset, m.tDate);
                                //    db.Database.ExecuteSqlCommand(@"UPDATE tAssetTag SET IsMail={0} where RFID={1}", true, m.Epc);
                                //    tagMontor.Remove(m.Epc);
                                //}
                                //else { tagMontor.Remove(m.Epc); }
                                tagMontor.Remove(m.Epc);
                            }
                        });
                    }
                })).Start();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception {0} : {1}", "timer_Triger", ex.Message);
            }
        }
        //static void OnConnectAsyncComplete(ImpinjReader reader, ConnectAsyncResult result, string errorMessage)
        //{
        //    try
        //    {
        //        if (result == ConnectAsyncResult.Success)
        //        {
        //            Settings settings = reader.QueryDefaultSettings();
        //            settings.AutoStart.Mode = AutoStartMode.Immediate;
        //            settings.Report.IncludePeakRssi = true;
        //            settings.Report.IncludeAntennaPortNumber = true;
        //            settings.Keepalives.Enabled = true;
        //            settings.Keepalives.PeriodInMs = 15000;
        //            settings.ReaderMode = ReaderMode.AutoSetDenseReaderDeepScan;
        //            settings.SearchMode = SearchMode.DualTarget;
        //            settings.Session = 2;
        //            settings.Keepalives.Enabled = true;
        //            settings.Keepalives.PeriodInMs = 5000;
        //            settings.Keepalives.EnableLinkMonitorMode = true;
        //            settings.Keepalives.LinkDownThreshold = 5;
        //            reader.KeepaliveReceived += OnKeepaliveReceived;
        //            if (reader.IsXArray)
        //            {
        //                settings.Antennas.DisableAll();
        //                settings.Antennas.EnableById(new ushort[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52 });
        //            }
        //            else if (reader.IsXSpan)
        //            {
        //                settings.Antennas.DisableAll();
        //                settings.Antennas.EnableById(new ushort[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 });
        //            }
        //            reader.ApplySettings(settings);
        //            Console.WriteLine("Starting reader...");

        //            if (!reader.IsConnected)
        //                reader.Start();
        //            //Console.WriteLine("Stopping reader in 5 seconds...");
        //            //Thread.Sleep(5000);
        //            //reader.Stop();
        //            //reader.Disconnect();
        //            //Console.WriteLine("Reader stopped. Press enter to exit.");
        //        }
        //        else
        //        {
        //            // Failed to connect to the reader
        //            Console.WriteLine("Failure while connecting to {0} : {1}", reader.Address, errorMessage);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Exception {0} : {1}", "OnConnectAsyncComplete", ex.Message);
        //    }
        //}
        static void OnKeepaliveReceived(ImpinjReader _reader)
        {
            try
            {
                DisplayCurrentSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception {0} : {1}", "OnKeepaliveReceived", ex.Message);
            }
        }
        static void DisplayCurrentSettings()
        {
            try
            {
                Console.Clear();
                Thread.Sleep(1000);
                foreach (ImpinjReader reader in readers)
                {
                    if (reader.IsConnected)
                    {
                        Console.WriteLine("Reader Features");
                        Console.WriteLine("---------------");
                        FeatureSet features = reader.QueryFeatureSet();
                        Console.WriteLine("Model name : {0}", features.ModelName);
                        Console.WriteLine("Mac: {0} IP: ({1})", reader.Name, reader.Address);
                        Console.WriteLine("Model number : {0}", features.ModelNumber);
                        //Console.WriteLine("Reader model : {0}", features.ReaderModel.ToString());
                        Console.WriteLine("Firmware version : {0}", features.FirmwareVersion);
                        Console.WriteLine("Antenna count : {0}\n", features.AntennaCount);

                        //features.Save("features.xml");
                        Console.WriteLine("Reader Status");
                        Console.WriteLine("---------------");
                        Status status = reader.QueryStatus();
                        Console.ForegroundColor = status.IsConnected ? ConsoleColor.DarkGreen : ConsoleColor.Red;
                        Console.WriteLine("Is connected : {0}", status.IsConnected);
                        Console.ForegroundColor = status.IsSingulating ? ConsoleColor.DarkGreen : ConsoleColor.Red;
                        Console.WriteLine("Is singulating : {0}", status.IsSingulating);
                        Console.ForegroundColor = status.TemperatureInCelsius < 50 ? ConsoleColor.DarkGreen : ConsoleColor.Red;
                        Console.WriteLine("Temperature : {0}° C\n", status.TemperatureInCelsius);
                        Console.ResetColor();

                        //
                        //Console.WriteLine("Reader Settings");
                        //Console.WriteLine("---------------");
                        //Settings settings = reader.QuerySettings();
                        //Console.WriteLine("Reader mode : {0}", settings.ReaderMode);
                        //Console.WriteLine("Search mode : {0}", settings.SearchMode);
                        //Console.WriteLine("Session : {0}\n", settings.Session);
                    }
                }
                Console.WriteLine("");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception {0} : {1}", "DisplayCurrentSettings", ex.Message);
            }
        }
        static void ApplyDefaultSettings()
        {

        }
        public static class SolutionConstants
        {
            public const string ReaderHostname = "speedwayr-11-XX-XX.example.com";
        }
        static void RebootReaders()
        {
            try
            {
                foreach (ImpinjReader reader in readers)
                {
                    string reply;
                    reader.RShell.Open(reader.Address, "root", "impinj", 5000);
                    RShellCmdStatus status = reader.RShell.Send("show network summary", out reply);
                    if (status == RShellCmdStatus.Success)
                    {
                        Console.WriteLine("RShell command executed successfully.\n");
                    }
                    else
                    {
                        Console.WriteLine("RShell command failed to execute.\n");
                    }
                    Console.WriteLine("RShell command reply : \n\n" + reply + "\n");
                    reader.RShell.Close();
                }
            }
            catch (OctaneSdkException e)
            {
                Console.WriteLine("Exception {0} : {1}", "RebootReaders", e);
            }
        }
        static void MissingAsset(tAssetTag ticket, DateTime _lastSeen)
        {
            try
            {
                String text, strSubject;
                strSubject = "Asset Missing";
                StringBuilder strBody = new StringBuilder();
                strBody.Append("<html xmlns=\"http://www.w3.org/1999/xhtml/\">");
                strBody.Append("<head><title>" + strSubject + "</title><style>td{font-weight:bold;} </style></head>");
                strBody.Append("<body style=\"font-size: 12pt; font-family: Courier New;\">");
                strBody.Append("<br /><font face=\"Courier New\" size=\"3\">ITAM Notification</font><br /><br />");
                strBody.Append("<table border='1' width=\"100%\" align=\"center\" style=\"font-family: Courier New; -webkit-font-smoothing: antialiased;font-size: 12px;overflow: auto;text-align: left; border: 2px solid Gray;\">");
                strBody.Append("<tr style='background-color: #99ccff;");
                strBody.Append(" color: black;padding: 6px 10px;font-weight: bold; border-right-color: Black;border-right-width: 1px;'>");
                strBody.Append("<td><p><font face=\"Courier New\" size=\"2\">&nbsp;&nbsp;Asset Name</font></p>");
                strBody.Append("</td>");
                strBody.Append("<td><p><font face=\"Courier New\" size=\"2\">&nbsp;&nbsp;Serial Name</font></p>");
                strBody.Append("</td>");
                strBody.Append("<td><p><font face=\"Courier New\" size=\"2\">&nbsp;&nbsp;RFID</font></p>");
                strBody.Append("</td>");
                strBody.Append("<td><p><font face=\"Courier New\" size=\"2\">&nbsp;&nbsp;Last Seen</font></p>");
                strBody.Append("</td>");
                strBody.Append("</tr>");
                strBody.Append("<tr style='height: 25px;'>");
                strBody.Append("<td style='padding: 5px 10px 5px 5px;'>" + ticket.IteamName + "</td>");
                strBody.Append("<td style='padding: 5px 10px 5px 5px;'>" + ticket.SerialNo + "</td>");
                strBody.Append("<td style='padding: 5px 10px 5px 5px;'>" + ticket.RFID + "</td>");
                strBody.Append("<td style='padding: 5px 10px 5px 5px;'>" + _lastSeen + "</td>");
                strBody.Append("</tr>");
                strBody.Append("</table><br /><br />");
                strBody.Append("</body></html>");

                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                message.From = new MailAddress(ConfigurationManager.AppSettings["EmailId"].ToString());
                string[] missingTo = ConfigurationManager.AppSettings["missingTo"].Split(';');
                string[] missingCC = ConfigurationManager.AppSettings["missingCC"].Split(';');

                if (missingCC.Length > 0)
                {
                    foreach (var _eid in missingCC)
                    {
                        message.CC.Add(new MailAddress(_eid));
                    }
                }
                if (missingTo.Length > 0)
                {
                    foreach (var _eid in missingTo)
                    {
                        message.To.Add(new MailAddress(_eid));
                    }
                }

                //string htmlBody;
                //htmlBody = strBody;
                string returnUrl = strBody.ToString();
                message.Body = returnUrl;
                message.Subject = ticket.SerialNo + ": Asset Missing Alert";
                message.IsBodyHtml = true;
                message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                ///smtp server and port configured at registry
                SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings["EmailServer"], Convert.ToInt32(ConfigurationManager.AppSettings["Port"]));
                ///enable ssl is required for secure connection.It is must be true for gmail server and false for other servers.
                smtpClient.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["IsSSL"]);
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = true;
                //smtpClient.Credentials = new NetworkCredential(WebConfigurationManager.AppSettings["EmailId"], WebConfigurationManager.AppSettings["Password"]);
                smtpClient.Send(message);

                //NotifyMail.FireAndForgetTaskAsync(async () =>
                //{
                //});

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception {0} : {1}", "MissingAsset", ex.Message);
            }
            finally
            {
                //Console.WriteLine("Executing finally block.");
            }
        }
    }
}

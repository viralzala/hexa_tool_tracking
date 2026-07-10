using HexaERP.MVC.Controllers;
using HexaERP.Services;
using HexaPatch;
using System;
using System.Timers;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace HexaERP.MVC
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static MqttService MqttService { get; private set; }

        private static double TimerIntervalInMilliseconds =
           Convert.ToDouble(WebConfigurationManager.AppSettings["TimerIntervalInMilliseconds"]);

        EncryptionDecryption Sec = new EncryptionDecryption();
        HexaPatchInclude lobj = new HexaPatchInclude();

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register); // 
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            MqttService = new MqttService();

            //var PostSetting = ConfigurationManager.GetSection("threshold") as NameValueCollection;
            //if (PostSetting.Count == 0)
            //{
            //    Console.WriteLine("Post Settings are not defined");
            //}
            //else
            //{
            //    foreach (var key in PostSetting.AllKeys)
            //    {
            //        Console.WriteLine(key + " = " + PostSetting[key]);
            //    }
            //}

            //
            HexaApiController.thresholdlevels();

            //string pcMac = Sec.MachineMac();
            //var mac = lobj.Platforms();
            //bool has = mac.Any(cus => cus.Mac == pcMac);
            bool has = true;
            if (has)
            {

                ///Debug.WriteLine(string.Concat("Application_Start Called: ", DateTime.Now.ToString()));
                // This will raise the Elapsed event every 'x' millisceonds (whatever you set in the
                // Web.Config file for the added TimerIntervalInMilliseconds AppSetting
                Timer timer = new Timer(TimerIntervalInMilliseconds);
                timer.Enabled = true;
                // Setup Event Handler for Timer Elapsed Event
                timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                timer.Start();
                //AddModelError("", "This hosted server are not licenced. please contact your service pervider");
            }


        }
        // Added the following procedure:
        static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            HexaApiController.Callback(DateTime.Now);
            //// Get the TimerStartTime web.config value
            //DateTime MyScheduledRunTime = DateTime.Parse(WebConfigurationManager.AppSettings["TimerStartTime"]);
            //// Get the current system time
            //DateTime CurrentSystemTime = DateTime.Now;
            //Debug.WriteLine(string.Concat("Timer Event Handler Called: ", CurrentSystemTime.ToString()));

            //// This makes sure your code will only run once within the time frame of (Start Time) to
            //// (Start Time+Interval). The timer's interval and this (Start Time+Interval) must stay in sync
            //// or your code may not run, could run once, or may run multiple times per day.
            //DateTime LatestRunTime = MyScheduledRunTime.AddMilliseconds(TimerIntervalInMilliseconds);

            //// If within the (Start Time) to (Start Time+Interval) time frame - run the processes
            //if ((CurrentSystemTime.CompareTo(MyScheduledRunTime) >= 0) && (CurrentSystemTime.CompareTo(LatestRunTime) <= 0))
            //{
            //    Debug.WriteLine(String.Concat("Timer Event Handling MyScheduledRunTime Actions: ", DateTime.Now.ToString()));
            //    // RUN YOUR PROCESSES HERE
            //}
        }
        public partial class _Platforms
        {
            public string Mac { get; set; }
        }
    }
}

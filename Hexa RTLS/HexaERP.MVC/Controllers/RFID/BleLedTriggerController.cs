using HexaERP.MVC.Models;
using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class BleLedTriggerController : Controller
    {
        private readonly MqttService _mqttService;

        public static string UserName = string.Empty;

        public BleLedTriggerController()
        {
            _mqttService = MvcApplication.MqttService; // Retrieve the MQTT service instance
        }

        // GET: BleLedTrigger
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
                    UserName = cookieObject["AppUserName"];
                }
                else { return RedirectToAction("Index", "AppUser"); }
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "AppUser");
            }
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> SearchProductByBle(string Search)
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {

                string _serach = Search.Trim();

                using (var context = new ERPdbEntities())
                {
                    context.mReaderSettups
                        .Where(ip => ip.ReaderIP == null)
                        .Select(reader => reader.ReaderNo).Distinct()
                        .ToList().ForEach(i =>
                        {
                            string msg = "dData";
                            string mac = $"{Search}";
                            int seq = 7716;
                            string auth1 = "0000000000000000";
                            string dType = "json";
                            int mode = 0;
                            string dataMsg = "ring";
                            int ringType = 2;
                            int ringTime = 2000;
                            int ledOn = 500;
                            int ledOff = 1500;
                            string jsonString = $"{{\"msg\":\"{msg}\",\"mac\":\"{mac}\",\"seq\":{seq},\"auth1\":\"{auth1}\",\"dType\":\"{dType}\",\"mode\":{mode},\"data\":{{\"msg\":\"{dataMsg}\",\"ringType\":{ringType},\"ringTime\":{ringTime},\"ledOn\":{ledOn},\"ledOff\":{ledOff}}}}}";
                            //string jsonPayload = "{\"msg\":\"dData\",\"mac\":\"BC572903D80A\",\"seq\":7716,\"auth1\":\"0000000000000000\",\"dType\":\"json\",\"mode\":0,\"data\":{\"msg\":\"ring\",\"ringType\":2,\"ringTime\":2000,\"ledOn\":500,\"ledOff\":1500}}";

                            _mqttService.PublishAsync($"kbeacon/pubaction/{i}", jsonString);
                            _mqttService.PublishAsync($"kbeacon/subaction/{i}", jsonString);
                        });
                    result = this.Json(new { Flag = false, message = "" }, JsonRequestBehavior.AllowGet);
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
                result = this.Json(new { message, Flag = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

            }
            return result;
        }
    }
}
using HexaERP.MVC.Models;
using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.SMT
{
    public class SearchSMTBinController : Controller
    {
        private readonly MqttService _mqttService;

        public static string UserName = string.Empty;

        public SearchSMTBinController()
        {
            _mqttService = MvcApplication.MqttService; // Retrieve the MQTT service instance
        }
        // GET: SearchSMTBin
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
        public async Task<ActionResult> GetSMTProdcutInfo(string Search)
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {
                //if (Search.Length < 3)
                //    return this.Json(new { message = "Add at least 3 words for search", Flag = false }, JsonRequestBehavior.AllowGet);

                string _serach = Search.Trim();

                using (var context = new ERPdbEntities())
                {
                    //                    var _product = context.Database.SqlQuery<SMTSearchModelView>(@"select z.RoomName as Zone,p.*,m.*
                    //from mSMTProduct as p
                    //left join toMonitor as m on p.Ble = m.Epc 
                    //left join mReaderSettup as r on m.Name = r.ReaderNo
                    //left join mRoomMaster as z on r.mRoomMasterId = z.mRoomMasterId where
                    //  ({0} IS NULL OR p.PartNumber LIKE '%' + {0} + '%')
                    //OR ({0} IS NULL OR p.Status LIKE '%' + {0} + '%')
                    //OR ({0} IS NULL OR p.SerialNumber LIKE '%' + {0} + '%')
                    //OR ({0} IS NULL OR p.CustomerCode LIKE '%' + {0} + '%')
                    //OR ({0} IS NULL OR p.Lot LIKE '%' + {0} + '%')
                    //OR ({0} IS NULL OR p.Ble LIKE '%' + {0} + '%')
                    //OR ({0} IS NULL OR p.ShelfName LIKE '%' + {0} + '%')
                    //OR ({0} IS NULL OR p.ModifiedBy LIKE '%' + {0} + '%')",
                    //                                        new object[] { Search }).ToList();

                    //object[] parameters = { _serach };
                    string jsonPayload = "{\"msg\":\"dData\",\"mac\":\"BC572903D80A\",\"seq\":7716,\"auth1\":\"0000000000000000\",\"dType\":\"json\",\"mode\":0,\"data\":{\"msg\":\"ring\",\"ringType\":2,\"ringTime\":2000,\"ledOn\":500,\"ledOff\":1500}}";

                    await _mqttService.PublishAsync("kbeacon/subaction/94A408B61ED8", jsonPayload);

                    var _product = context.Database.SqlQuery<SMTSearchModelView>("select z.RoomName as Zone,p.*,m.* from mSMTProduct as p left join toMonitor as m on p.Ble = m.Epc left join mReaderSettup as r on m.Name = r.ReaderNo left join mRoomMaster as z on r.mRoomMasterId = z.mRoomMasterId where p.ShelfName={0} OR p.PartNumber={0} OR p.Ble ={0}",
                                                            new object[] { _serach }).ToList();

                    if (_product.Count() > 0)
                        result = this.Json(new { Flag = true, message = "Record Found", _product }, JsonRequestBehavior.AllowGet);
                    else
                        result = this.Json(new { Flag = false, message = "Record Not Found" }, JsonRequestBehavior.AllowGet);
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
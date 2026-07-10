using HexaERP.MVC.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class GoogleMapLocatorController : Controller
    {
        // static List<tToolTrackDemo> tagsTrack = new List<tToolTrackDemo>();

        private ERPdbEntities db = new ERPdbEntities();
        // GET: GoogleMapLocator
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


                }
                else { return RedirectToAction("Index", "AppUser"); }

            }
            catch (Exception)
            {
                return RedirectToAction("Index", "AppUser");
            }
            return View();
        }

        //GoogleMapLocator/getGetToTrackData
        [HttpGet]
        public async Task<ActionResult> getGetToTrackData()
        {
            // Initialization.    
            JsonResult result = new JsonResult();
            string source = "";
            //var path = System.IO.File.ReadAllText(Server.MapPath("~/Content/EmployeeTag.json"));
            var path = Server.MapPath("~/Content/EmployeeTag.json");
            using (StreamReader SourceReaderr = new StreamReader(path))
            {
                source = await SourceReaderr.ReadToEndAsync();
            }
            //var path = System.IO.File.ReadAllText(Server.MapPath("~/Content/EmployeeTag.json"));
            if (source != null)
            {
                List<tToolTrackDemo> tagsTrack = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<tToolTrackDemo>>(source));
                //List<tToolTrackDemo> tagsTrack = JsonConvert.DeserializeObject<List<tToolTrackDemo>>(path);
                try
                {

                    var tEmp = (from tm in tagsTrack
                                join emp in db.tEmployeeTags on tm.Epc equals emp.RFID

                                join ag in db.mAgencies on emp.mAgencyId equals ag?.mAgencyId
                                join ds in db.mDesignations on emp.mDesignationId equals ds?.mDesignationId
                                join rst in db.mReaderSettups on tm.Reader equals rst.ReaderNo
                                join tzn in db.mZones on rst.mZoneId equals tzn.mZoneId
                                //join sel in db.mFloorMasters on rst.mFloorMasterId equals sel?.mFloorMasterId
                                //join rec in db.mRoomMasters on rst.mRoomMasterId equals rec?.mRoomMasterId
                                where tm.PortId == rst.AttPortId
                                select new
                                {
                                    rst.lat,
                                    rst.lng,
                                    description = tzn.Zone ?? string.Empty,
                                    title = (emp.EmployeeName + "</br>" + tm.tDate.ToString())
                                }).ToList();

                    var tAsset = (from tm in tagsTrack
                                  join ast in db.tAssetTags on tm.Epc equals ast.RFID
                                  join itm in db.mIteamMasters on ast.mIteamMasterId equals itm?.mIteamMasterId
                                  join msit in db.mSiteMasters on ast.mSiteMasterId equals msit?.mSiteMasterId
                                  join mzon in db.mZones on ast.mZoneId equals mzon?.mZoneId
                                  join mflr in db.mFloorMasters on ast.mFloorMasterId equals mflr?.mFloorMasterId
                                  join mrom in db.mRoomMasters on ast.mRoomMasterId equals mrom?.mRoomMasterId
                                  join rst in db.mReaderSettups on tm.Reader equals rst.ReaderNo
                                  join tzn in db.mZones on rst.mZoneId equals tzn.mZoneId
                                  join sel in db.mFloorMasters on rst.mFloorMasterId equals sel.mFloorMasterId
                                  join rec in db.mRoomMasters on rst.mRoomMasterId equals rec.mRoomMasterId
                                  where tm.PortId == rst.AttPortId
                                  select new
                                  {
                                      rst.lat,
                                      rst.lng,
                                      description = tzn.Zone ?? string.Empty + tm.tDate.ToString(),
                                      title = (ast.Model + "</br>" + tm.tDate.ToString())
                                  }).ToList();

                    var ObjDatass = tEmp.Concat(tAsset).ToList();

                    var query = (from t in ObjDatass
                                 group t by new { lat = t.lat, lng = t.lng } into g
                                 select new
                                 {
                                     lat = g.Key.lat,
                                     lng = g.Key.lng,
                                     //title = String.Join(",", g),
                                     //title = g.title,
                                     Data = g.AsEnumerable(),
                                 }).ToList().Select(q => new { type = "circle", circle_options = "{radius: 60}", draggable = false, zoom = 11, icon = "Content/assets/img/md-images/ic_place_red_48dp.png", lat = q.lat, lon = q.lng, title = q.Data.Aggregate("", (acc, t) => t.description), html = q.Data.Aggregate("", (acc, t) => (acc) + "<div class='map-info-window'><p>" + t.title + "</p></div>") });


                    result = this.Json(new { ObjDatass, query }, JsonRequestBehavior.AllowGet);
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

            }
            else
            {
                result = this.Json(new { Message = "Null Data", Flag = false }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }
    }
}
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
    public class EmployeeMapController : Controller
    {
        static List<tToolTrackDemo> tagsTrack = new List<tToolTrackDemo>();

        private ERPdbEntities db = new ERPdbEntities();
        // GET: EmployeeMap
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
                    //--- To read values from cookie collection we will use Keys used while creating cookie.                   
                    // string AppUserName = cookieObject["AppUserName"];
                    //string UniqueId = cookieObject["UniqueId"];
                    //string OrgInfoId = cookieObject["OrgInfoId"];
                    //string SortCode = cookieObject["SortCode"];

                    //System.IO.File.WriteAllText(Server.MapPath("~/Content/EmployeeTag.json"), null);
                }
                else { return RedirectToAction("Index", "AppUser"); }

                if (Session["UniqueId"].ToString() != "" && Session["OrgInfoId"].ToString() != "" && Session["AppUserName"].ToString() != "")
                {

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

        // GET: EmployeeMap/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: EmployeeMap/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EmployeeMap/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: EmployeeMap/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }
        //
        [HttpGet]
        public JsonResult getlocationdata()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id

            var ZoneData = (from Dis in db.mZones
                            where (Dis.IsAction == true && Dis.OrgInfoId == orgId)
                            select new { Dis.mZoneId, Dis.Zone }).ToList();

            var subZoneData = (from Dis in db.mFloorMasters
                               join F in db.mZones on Dis.mZoneId equals F.mZoneId
                               where (Dis.IsAction == true && Dis.OrgInfoId == orgId)
                               select new { Dis.mFloorMasterId, Dis.FloorName, F.mZoneId, F.Zone }).ToList();

            var FloorData = (from Dis in db.mFloorMasters
                             where (Dis.IsAction == true && Dis.OrgInfoId == orgId)
                             select new { Dis.mFloorMasterId, Dis.FloorName }).ToList();

            var RoomData = (from Dis in db.mRoomMasters
                            join F in db.mFloorMasters on Dis.mFloorMasterId equals F.mFloorMasterId
                            //join R in db.mReaderSettups on Dis.mRoomMasterId equals R.mRoomMasterId
                            where (Dis.IsAction == true && Dis.OrgInfoId == orgId)
                            select new { Dis.mRoomMasterId, Dis.RoomName, F.FloorName, Dis.mFloorMasterId }).ToList();

            var PortsData = (from Dis in db.mReaderSettups
                             join zn in db.mZones on Dis.mZoneId equals zn.mZoneId
                             join fm in db.mFloorMasters on Dis.mFloorMasterId equals fm.mFloorMasterId
                             join rm in db.mRoomMasters on Dis.mRoomMasterId equals rm.mRoomMasterId
                             where (Dis.IsAction == true && Dis.OrgInfoId == orgId)
                             select new { Dis.mReaderSettupId, Dis.ReaderNo, Dis.AttPortId, rm.RoomName, Dis.mRoomMasterId, zn.mZoneId, zn.Zone, fm.mFloorMasterId, fm.FloorName }).ToList();


            //Convert List Data to The Json Array                     
            return Json(new { IFloorData = FloorData, IObjData = RoomData, IPortsData = PortsData, IZoneData = ZoneData, IsubZoneData = subZoneData }, JsonRequestBehavior.AllowGet);
        }
        // POST: EmployeeMap/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: EmployeeMap/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: EmployeeMap/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //EmployeeLocator/getGetToTrackData
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
                                         EmployeeId = emp.EmployeeId ?? string.Empty,
                                         tm.Epc,
                                         tm.tDate,
                                         rst.mFloorMasterId,
                                         rst.mZoneId,
                                         eZoneId = emp.mZoneId
                                     }
                                 ).ToList();


                    result = this.Json(ObjDatass.ToArray(), JsonRequestBehavior.AllowGet);

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

using HexaERP.MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class AssignWorkController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();

        // GET: AssignWork
        public ActionResult Index()
        {
            return View();
        }

        // GET: AssignWork/getZones
        [HttpGet]
        public JsonResult getZones()
        {
            try
            {
                var getZone = db.mZones.Where(x => x.IsAction == true).ToList();
                return Json(new { Flag = true, Message = "Data Loaded Sucessfully", DZone = getZone.ToArray() }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        public static List<int> ConList(string a)
        {
            try
            {
                return a.Split(',').Select(x => x.Trim()).Select(x => Int32.Parse(x)).ToList();
            }
            catch (Exception) { return null; }
        }

        [HttpPost]
        public JsonResult AssigWorkZone(string EmployeeIds, string ZoneIds, int mShiftId)
        {
            JsonResult result = new JsonResult();
            try
            {
                List<int> _empList = null;
                List<int> _zoneList = null;
                _empList = ConList(EmployeeIds);
                _zoneList = ConList(ZoneIds);

                result = this.Json(new { Flag = true, Message = "Employee record found", EmployeeIds, ZoneIds }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                result = this.Json(new { Flag = false, Message = ex.InnerException.Message }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }


        // POST: SiteMaster/Create
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult Create(tEmployeeTag collection)
        {
            JsonResult result = new JsonResult();
            try
            {
                if (string.IsNullOrEmpty(collection.EmployeeName) && string.IsNullOrEmpty(collection.EmployeeId))
                {
                    result = this.Json(new { Flag = false, Message = "Enter Employee Name/ID" }, JsonRequestBehavior.AllowGet);
                }
                var _courseList = db.Database.SqlQuery<GetEmployeeForWork>("spGetEmployeeForWork {0}, {1}",
                new object[] { collection.EmployeeName, collection.EmployeeId }).ToList();
                result = this.Json(new { Flag = true, Message = "Employee record found", _courseList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                result = this.Json(new { Flag = false, Message = ex.InnerException.Message }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }

        public class GetEmployeeForWork
        {
            public Nullable<int> tEmployeeTagId { get; set; }

            public string RFID { get; set; }
            public string EmployeeName { get; set; }
            public string EmployeeId { get; set; }
            public string Site { get; set; }
            public Nullable<int> mSiteMasterId { get; set; }
            public string Zone { get; set; }
            public Nullable<int> mZoneId { get; set; }
            public Nullable<int> mFloorMasterId { get; set; }
            public Nullable<int> mRoomMasterId { get; set; }


            public string Agency { get; set; }
            public string Designation { get; set; }
            public string SkillCategory { get; set; }

            public string WorkCategory { get; set; }
            public Nullable<int> mWorkCategoryId { get; set; }
            public string Activity { get; set; }
            public Nullable<int> mActivityId { get; set; }
            public string Shift { get; set; }
            public Nullable<int> mShiftId { get; set; }
        }
    }
}
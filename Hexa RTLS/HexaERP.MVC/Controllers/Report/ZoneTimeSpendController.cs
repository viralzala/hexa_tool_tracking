using HexaERP.MVC.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HexaERP.MVC.Controllers.Report
{
    public class ZoneTimeSpendController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();
        static List<spSiteTimeSpend_Result> ExcelExport = new List<spSiteTimeSpend_Result>();

        // GET: ZoneTimeSpend
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

        public DateTime ConvertToDateTime(string strDateTime)
        {
            DateTime dtFinaldate; string sDateTime;
            try { dtFinaldate = Convert.ToDateTime(strDateTime); }
            catch (Exception)
            {
                string[] sDate = strDateTime.Split('/');
                sDateTime = sDate[1] + '/' + sDate[0] + '/' + sDate[2];
                dtFinaldate = Convert.ToDateTime(sDateTime);
            }
            return dtFinaldate;
        }
        // POST: ZoneTimeSpend/Create
        [HttpPost]
        public ActionResult CreateZone(SummaryReport obj)
        {

            // Initialization.    
            JsonResult result = new JsonResult();

            try
            {
                DateTime fDate = ConvertToDateTime(obj.fromDate);
                DateTime tDate = ConvertToDateTime(obj.toDate);

                if (obj == null || obj == null)
                {
                    return Json(new { Flag = false, Message = "Between Date Require" }, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    var _dataList = GetTimeSpent(fDate, tDate, obj.mSiteMasterId, obj.mZoneId, obj.mFloorMasterId, obj.mAgencyId, obj.mDesignationId, obj.mSkillCategoryId, obj.mWorkCategoryId, obj.mActivityId, obj.EmployeeName, obj.EmployeeId);

                    if (_dataList != null)
                    {

                        var mData = (from objI in _dataList
                                     join emp in db.tEmployeeTags on objI.RFID equals emp.RFID
                                     join ag in db.mAgencies on emp.mAgencyId equals ag.mAgencyId
                                     join ds in db.mDesignations on emp.mDesignationId equals ds.mDesignationId
                                     join sk in db.mSkillCategories on emp.mSkillCategoryId equals sk.mSkillCategoryId
                                     join wk in db.mWorkCategories on emp.mWorkCategoryId equals wk.mWorkCategoryId
                                     join ac in db.mActivities on emp.mActivityId equals ac.mActivityId
                                     join ezone in db.mZones on emp.mZoneId equals ezone.mZoneId
                                     select new
                                     {
                                         EmployeeId = emp.EmployeeId ?? string.Empty,
                                         EmployeeName = emp.EmployeeName ?? string.Empty,
                                         Agency = ag.Agency ?? string.Empty,
                                         Designation = ds.Designation ?? string.Empty,
                                         SkillCategory = sk.SkillCategory ?? string.Empty,
                                         WorkCategory = wk.WorkCategory ?? string.Empty,
                                         Activity = ac.Activity ?? string.Empty,
                                         WorkedZone = objI.CompartmentName ?? string.Empty,
                                         Hours = objI.TotalSpentTime.Hours,
                                         Minutes = objI.TotalSpentTime.Minutes,
                                         EmployeeZone = ezone.Zone
                                     }).ToList();

                        TempData["ZoneTimeSpend"] = mData;

                        result = this.Json(new { Flag = true, Message = "Suceess Data", data = mData }, JsonRequestBehavior.AllowGet);
                        result.MaxJsonLength = int.MaxValue;

                    }
                }

            }
            catch (Exception ex)
            {
                result = this.Json(new { Flag = false, Message = ex.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return result;
        }


        //Get All Master
        [HttpGet]
        public JsonResult getMasterData()
        {
            var UserName = Session["AppUserName"];
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var mAgency = db.mAgencies.Where(x => x.IsAction == true && x.OrgInfoId == orgId).ToList();
            var mDesignation = db.mDesignations.Where(x => x.IsAction == true && x.OrgInfoId == orgId).ToList();
            var mSkillCategory = db.mSkillCategories.Where(x => x.IsAction == true && x.OrgInfoId == orgId).ToList();
            var mWorkCategory = db.mWorkCategories.Where(x => x.IsAction == true && x.OrgInfoId == orgId).ToList();

            var mActivity = db.mActivities.Where(x => x.IsAction == true && x.OrgInfoId == orgId).ToList();
            var mShift = db.mShifts.Where(x => x.IsAction == true && x.OrgInfoId == orgId).ToList();

            var mSite = db.mSiteMasters.Where(x => x.IsAction == true && x.OrgInfoId == orgId).ToList();
            //Convert List Data to The Json Array       
            //Convert List Data to The Json Array          
            return Json(new { mAgency = mAgency, mDesignation = mDesignation, mSkillCategory = mSkillCategory, mWorkCategory = mWorkCategory, mActivity = mActivity, mShift = mShift, mSite = mSite }, JsonRequestBehavior.AllowGet);
        }
        // GET: EmployeeSummary/getZones
        [HttpGet]
        public JsonResult getZones(int id)
        {
            try
            {
                var getZone = db.mZones.Where(x => x.IsAction == true && x.mSiteMasterId == id).ToList();
                return Json(new { Flag = true, Message = "Data Loaded Sucessfully", DZone = getZone.ToArray() }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        // GET: EmployeeSummary/getSubZones
        [HttpGet]
        public JsonResult getSubZones(int id)
        {
            try
            {
                var getSubZone = db.mFloorMasters.Where(x => x.IsAction == true && x.mZoneId == id).ToList();
                return Json(new { Flag = true, Message = "Data Loaded Sucessfully", DZone = getSubZone.ToArray() }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        // GET: EmployeeSummary/Area
        [HttpGet]
        public JsonResult getArea(int id)
        {
            try
            {
                var mroom = db.mRoomMasters.Where(x => x.IsAction == true && x.mFloorMasterId == id).ToList();
                return Json(new { Flag = true, Message = "Data Loaded Sucessfully", DArea = mroom.ToArray() }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        //
        public ActionResult ExportExcel()
        {
            var ExcelExport = TempData["ZoneTimeSpend"];

            if (ExcelExport != null)
            {
                var grid = new GridView();
                grid.DataSource = ExcelExport;
                grid.DataBind();
                var FileName = "TimeSpendOnZone_" + DateTime.Now.ToString("s") + "_.xls";
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachement; filename=" + FileName + "");
                Response.ContentType = "application/excel";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                grid.RenderControl(htw);
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
                return View();
            }
            else { return File("", "application/ms-excel", ""); }
        }


        public static List<CompartmentwiseTimeSpenditure> GetTimeSpent(DateTime startDt, DateTime endDt, int? mSiteMasterId, int? mZoneId, int? mFloorMasterId, int? mAgencyId, int? mDesignationId, int? mSkillCategoryId, int? mWorkCategoryId, int? mActivityId, string EmployeeName, string EmployeeId)
        {
            var logData = GetAllLogData(startDt, endDt, mSiteMasterId, mZoneId, mFloorMasterId, mAgencyId, mDesignationId, mSkillCategoryId, mWorkCategoryId, mActivityId, EmployeeName, EmployeeId);
            var startDate = Convert.ToDateTime(startDt);
            var endDate = Convert.ToDateTime(endDt);
            var lstCompartments = logData.Select(i => i.Zone).Distinct().ToList();
            var lstRFIDwiseLogdata = lstRFIDwiseLogData(logData);
            var totalTimeSpend = new List<CompartmentwiseTimeSpenditure>();
            foreach (var rfIDwiseData in lstRFIDwiseLogdata)
            {
                var compartmentWiseTimeSpendData = FilterCompartmentWiseTimeSpend(rfIDwiseData, lstCompartments, startDate, endDate);
                totalTimeSpend.AddRange(compartmentWiseTimeSpendData);
            }
            return totalTimeSpend;
        }

        public static List<ExportData> GetAllLogData(DateTime _startDt, DateTime _endDt, int? mSiteMasterId, int? mZoneId, int? mFloorMasterId, int? mAgencyId, int? mDesignationId, int? mSkillCategoryId, int? mWorkCategoryId, int? mActivityId, string EmployeeName, string EmployeeId)
        {
            using (var ctx = new ERPdbEntities())
            {

                var _courseList = ctx.Database.SqlQuery<ExportData>("spZoneTimeSpend {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}",
                new object[] { _startDt, _endDt, mSiteMasterId, mZoneId, mFloorMasterId, mAgencyId, mDesignationId, mSkillCategoryId, mWorkCategoryId, mActivityId, EmployeeName, EmployeeId }).ToList();
                return _courseList;
            }
        }
        public static List<RFIDwiseLogData> lstRFIDwiseLogData(List<ExportData> logData)
        {
            var lstRFID = logData.Select(i => i.RFID).Distinct().ToList();
            var lstRFIDwiseLogData = new List<RFIDwiseLogData>();
            foreach (var rfid in lstRFID)
            {
                var logdataFilterByRFID = logData.Where(x => x.RFID == rfid).ToList();
                var logDataByTimeStamp = logdataFilterByRFID.GroupBy(x => x.TtimeStamp).Select(g => g.First()).ToList();
                lstRFIDwiseLogData.Add(new RFIDwiseLogData() { RFID = rfid, LogData = logDataByTimeStamp });
            }
            return lstRFIDwiseLogData;
        }
        public static List<CompartmentwiseTimeSpenditure> FilterCompartmentWiseTimeSpend(RFIDwiseLogData rfidWiseLogData, List<string> lstCompartments, DateTime startDate, DateTime endDate)
        {

            var compartmentwiseData = new List<CompartmentwiseTimeSpenditure>();
            var groupByDate = GroupByDate(rfidWiseLogData.LogData, startDate, endDate);
            foreach (var CompartmentName in lstCompartments)
            {
                var totalTimeSpendData = CalculateTotalTimeSpent(CompartmentName, groupByDate, rfidWiseLogData);
                compartmentwiseData.AddRange(totalTimeSpendData);
            }


            return compartmentwiseData;
        }

        public static List<DateTime> GroupByDate(List<ExportData> logData, DateTime startDate, DateTime endDate)
        {
            var groupByDate = new List<DateTime>();
            if (startDate.TimeOfDay == TimeSpan.Parse("00:00:00") && endDate.TimeOfDay == TimeSpan.Parse("00:00:00"))
            {
                groupByDate = logData.Where(x => x.TtimeStamp.Date >= startDate && x.TtimeStamp.Date <= endDate).Select(x => x.TtimeStamp.Date).Distinct().ToList();
            }
            else
            {
                groupByDate = logData.Where(x => x.TtimeStamp >= startDate && x.TtimeStamp <= endDate).Select(x => x.TtimeStamp.Date).Distinct().ToList();
            }
            return groupByDate;
        }

        public static List<CompartmentwiseTimeSpenditure> CalculateTotalTimeSpent(string Compartment, List<DateTime> GroupByDate, RFIDwiseLogData rfidWiseLogData)
        {
            var totalSpendTimeSortByCompartment = new List<CompartmentwiseTimeSpenditure>();
            foreach (var data in GroupByDate)
            {
                TimeSpan totalTimeSpent = TimeSpan.Parse("0:00:00");
                var logIndex = 1;
                foreach (var logDatum in rfidWiseLogData.LogData)
                {
                    if (data == logDatum.TtimeStamp.Date && logDatum.Zone == Compartment && logIndex < rfidWiseLogData.LogData.Count && rfidWiseLogData.LogData[logIndex].TtimeStamp.Date == data)
                    {
                        // Logic to determine the total time spent by the RFID Tag in the compartment => log change between two adjacent compartment time diff
                        var nextlogData = rfidWiseLogData.LogData[logIndex];//logData.Where(x => x.TLogId == compData.TLogId + 1 && x.TtimeStamp.Date == data).FirstOrDefault();
                        if (nextlogData != null)
                        {
                            var time = nextlogData.TtimeStamp - logDatum.TtimeStamp;
                            totalTimeSpent = totalTimeSpent + time;
                        }
                    }
                    logIndex = logIndex + 1;
                }
                totalSpendTimeSortByCompartment.Add(new CompartmentwiseTimeSpenditure() { RFID = rfidWiseLogData.RFID, CompartmentName = Compartment, Date = data, TotalSpentTime = totalTimeSpent });

            }
            return totalSpendTimeSortByCompartment;
        }

        public class RFIDwiseLogData
        {
            public string RFID { get; set; }
            public List<ExportData> LogData { get; set; }
        }
        public class CompartmentwiseLogData
        {
            public string CompartmentName { get; set; }
            public List<ExportData> LogData { get; set; }
        }
        public class CompartmentwiseTimeSpenditure
        {
            public string RFID { get; set; }
            public string CompartmentName { get; set; }
            public DateTime Date { get; set; }
            public TimeSpan TotalSpentTime { get; set; }
        }

        public class ExportData
        {
            public string RFID { get; set; }
            public string EmployeeName { get; set; }
            public string EmployeeId { get; set; }
            public string Site { get; set; }
            public string Zone { get; set; }
            public DateTime TtimeStamp { get; set; }

            public string Agency { get; set; }
            public string Designation { get; set; }
            public string SkillCategory { get; set; }
            public string WorkCategory { get; set; }
            public string Activity { get; set; }


        }
    }
}
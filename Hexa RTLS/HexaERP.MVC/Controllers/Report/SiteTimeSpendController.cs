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
    public class SiteTimeSpendController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();
        static List<spSiteTimeSpend_Result> ExcelExport = new List<spSiteTimeSpend_Result>();
        // GET: SiteTimeSpend
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
        // POST: SiteTimeSpend/Create
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
                    var Idata = db.spSiteTimeSpend(fDate, tDate, obj.mSiteMasterId, obj.mZoneId, obj.mFloorMasterId, obj.mAgencyId, obj.mDesignationId, obj.mSkillCategoryId, obj.mWorkCategoryId, obj.mActivityId, obj.EmployeeName, obj.EmployeeId).ToList();
                    if (Idata != null)
                    {

                        ExcelExport.Clear();
                        ExcelExport = Idata;

                        var mData = (from objI in Idata
                                     join emp in db.tEmployeeTags on objI.RFID equals emp.RFID
                                     join ag in db.mAgencies on emp.mAgencyId equals ag.mAgencyId
                                     join ds in db.mDesignations on emp.mDesignationId equals ds.mDesignationId
                                     join sk in db.mSkillCategories on emp.mSkillCategoryId equals sk.mSkillCategoryId
                                     join wk in db.mWorkCategories on emp.mWorkCategoryId equals wk.mWorkCategoryId
                                     join ac in db.mActivities on emp.mActivityId equals ac.mActivityId
                                     join esite in db.mSiteMasters on emp.mSiteMasterId equals esite.mSiteMasterId
                                     select new
                                     {
                                         EmployeeId = emp.EmployeeId ?? string.Empty,
                                         EmployeeName = emp.EmployeeName ?? string.Empty,
                                         Agency = ag.Agency ?? string.Empty,
                                         Designation = ds.Designation ?? string.Empty,
                                         SkillCategory = sk.SkillCategory ?? string.Empty,
                                         WorkCategory = wk.WorkCategory ?? string.Empty,
                                         Activity = ac.Activity ?? string.Empty,
                                         WorkedSite = objI.Site ?? string.Empty,
                                         InTime = objI.InTime,
                                         OutTime = objI.OutTime,
                                         TimeSpend = objI.TimeSpend,
                                         EmployeeSite = esite.Site
                                     }).ToList();

                        result = this.Json(new { Flag = true, Message = "Suceess Data", data = Idata }, JsonRequestBehavior.AllowGet);

                    }

                    else
                    {
                        result = this.Json(new { Flag = false, Message = "Data Not Found", data = Idata }, JsonRequestBehavior.AllowGet);
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
            // var dataTrackSummary = (spTrackSummary_Result)TempData["ExcelData"];
            if (ExcelExport != null)
            {
                var grid = new GridView();
                grid.DataSource = ExcelExport;
                grid.DataBind();
                var FileName = "TimeSpendOnSite_" + DateTime.Now.ToString("s") + "_.xls";
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
        //
        public class ExportData
        {
            public string EmployeeName { get; set; }
            public string EmployeeId { get; set; }
            public string TrackSite { get; set; }
            public DateTime? InTime { get; set; }
            public DateTime? OutTime { get; set; }
            public string TimeSpend { get; set; }
            public string Agency { get; set; }
            public string Designation { get; set; }
            public string SkillCategory { get; set; }
            public string WorkCategory { get; set; }
            public string Activity { get; set; }
            public string Name { get; set; }
            public string TimeSpendZone { get; set; }
        }
    }
}
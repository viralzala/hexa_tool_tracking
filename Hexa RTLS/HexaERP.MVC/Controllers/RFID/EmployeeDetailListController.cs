using HexaERP.MVC.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HexaERP.MVC.Controllers.RFID
{
    public class EmployeeDetailListController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();
        //public static List<EmployeeDetails> ExcelExport = new List<EmployeeDetails>();
        // GET: EmployeeDetailList
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
                    //string Page_Name = Path.GetFileName(Request.Path);
                    if (Convert.ToString(Session["SortCode"]) != "AD")
                    {
                        return RedirectToAction("Index", "AppUser");
                    }



                    //AppUserName = Session["AppUserName"].ToString(); UniqueId = Session["UniqueId"].ToString();
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
        [HttpGet]
        public JsonResult GetEmployeeTrack()
        {

            var UserName = Session["AppUserName"];
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = (from tAs in db.tEmployeeTags
                           join ag in db.mAgencies on tAs.mAgencyId equals ag.mAgencyId into _ag

                           join ds in db.mDesignations on tAs.mDesignationId equals ds.mDesignationId into _ds
                           join sk in db.mSkillCategories on tAs.mSkillCategoryId equals sk.mSkillCategoryId into _sk
                           join wk in db.mWorkCategories on tAs.mWorkCategoryId equals wk.mWorkCategoryId into _wk
                           join ac in db.mActivities on tAs.mActivityId equals ac.mActivityId into _ac
                           join shf in db.mShifts on tAs.mShiftId equals shf.mShiftId into _shf

                           join sit in db.mSiteMasters on tAs.mSiteMasterId equals sit.mSiteMasterId into _sit
                           join zon in db.mZones on tAs.mZoneId equals zon.mZoneId into _zon
                           join subzon in db.mFloorMasters on tAs.mFloorMasterId equals subzon.mFloorMasterId into _subzon

                           where (tAs.OrgInfoId == orgId && tAs.IsAction == true)

                           from _agd in _ag.DefaultIfEmpty()
                           from _dsd in _ds.DefaultIfEmpty()
                           from _skd in _sk.DefaultIfEmpty()
                           from _wkd in _wk.DefaultIfEmpty()
                           from _acd in _ac.DefaultIfEmpty()
                           from _shfd in _shf.DefaultIfEmpty()
                           from _sitd in _sit.DefaultIfEmpty()
                           from _zond in _zon.DefaultIfEmpty()
                           from _subzond in _subzon.DefaultIfEmpty()

                           select new
                           {
                               tAs.tEmployeeTagId,
                               tAs.RFID,
                               tAs.EmployeeName,
                               tAs.EmployeeId,

                               _agd.Agency,
                               _dsd.Designation,
                               _skd.SkillCategory,
                               _wkd.WorkCategory,
                               _acd.Activity,
                               _shfd.Shift,

                               _sitd.Site,
                               _zond.Zone,
                               _subzond.FloorName

                           }).ToList();
            //Convert List Data to The Json Array       
            //Convert List Data to The Json Array  
            TempData["EmployeeDetails"] = ObjData;
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }

        //
        public ActionResult ExportExcel()
        {
            var ExcelExport = TempData["EmployeeDetails"];
            // var dataTrackSummary = (spTrackSummary_Result)TempData["ExcelData"];
            if (ExcelExport != null)
            {
                var grid = new GridView();
                grid.DataSource = ExcelExport;
                grid.DataBind();
                var FileName = "EmployeeDetailList_" + DateTime.Now.ToString("s") + "_.xls";
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
        public partial class EmployeeDetails
        {
            public string EmployeeId { get; set; }
            public string EmployeeName { get; set; }
            public string Agency { get; set; }
            public string Designation { get; set; }
            public string SkillCategory { get; set; }
            public string WorkCategory { get; set; }
            public string Activity { get; set; }
            public string Shift { get; set; }
            public string Site { get; set; }

        }
    }
}
using HexaERP.MVC.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HexaERP.MVC.Controllers.Report
{
    public class EmployeeTrackSummaryController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();
        private List<EmployeeDetailLogEntity> ExcelExportEmp = new List<EmployeeDetailLogEntity>();
        // GET: EmployeeTrackSummary
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
            return Json(new { mAgency, mDesignation, mSkillCategory, mWorkCategory, mActivity, mShift, mSite }, JsonRequestBehavior.AllowGet);
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

        // POST: EmployeeSummary/Create
        [HttpPost]
        public ActionResult Create(SummaryReport obj)
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {
                //DateTime date = DateTime.ParseExact(obj.fromDate,  "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);


                if (obj.fromDate == null || obj.toDate == null)
                {
                    return Json(new
                    {
                        Flag = false,
                        Message = "Between Date Require"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    //string[] sDate = obj.toDate.Split('/');
                    //string sDateTime = sDate[1] + '/' + sDate[0] + '/' + sDate[2];
                    //DateTime tDate = Convert.ToDateTime(sDateTime);

                    DateTime fDate = ConvertToDateTime(obj.fromDate);
                    DateTime tDate = ConvertToDateTime(obj.toDate);

                    // var mData = db.spTrackSummary(fDate, tDate,
                    //obj.mSiteMasterId, obj.mZoneId, obj.mFloorMasterId, obj.mAgencyId,
                    //obj.mDesignationId, obj.mSkillCategoryId, obj.mWorkCategoryId,
                    //obj.mActivityId, obj.EmployeeName, obj.EmployeeId).ToList();

                    var _LogList = db.Database.SqlQuery<EmployeeDetailLogEntity>("spTrackSummary {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}",
                    new object[] { fDate, tDate, obj.mSiteMasterId, obj.mZoneId, obj.mAgencyId, obj.mDesignationId, obj.mSkillCategoryId, obj.mWorkCategoryId, obj.mActivityId, obj.EmployeeName, obj.EmployeeId }).ToList();
                    result = this.Json(new
                    {
                        Flag = true,
                        Message = "Suceess Data",
                        data = _LogList
                    }, JsonRequestBehavior.AllowGet);

                    TempData["EmployeeLogDetail"] = null;
                    TempData["EmployeeLogDetail"] = _LogList;
                }
            }
            catch (Exception ex)
            {
                result = this.Json(new
                {
                    Flag = false,
                    Message =
                        ex.InnerException.Message.ToString()
                }, JsonRequestBehavior.AllowGet);
            }

            return result;
        }
        // GET: EmployeeSummary/getSubZones
        [HttpGet]
        public JsonResult GerEmployess(string fDate, string toDate)
        {
            // Initialization.    
            JsonResult result = new JsonResult();

            try
            {
                //DateTime date = DateTime.ParseExact(obj.fromDate, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(fDate) || string.IsNullOrEmpty(toDate))
                {
                    return Json(new { Flag = false, Message = "Between Date Require" }, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    // DateTime _tDate = ConvertToDateTime(tDate);
                    //string[] sDate = toDate.Split('/');
                    //string sDateTime = sDate[1] + '/' + sDate[0] + '/' + sDate[2];
                    //DateTime tDate = Convert.ToDateTime(sDateTime);

                    //DateTime _fDate = ConvertToDateTime(fDate);

                    DateTime _fDate = ConvertToDateTime(fDate);
                    DateTime tDate = ConvertToDateTime(toDate);

                    var mData = (from objI in db.toTrackInfoes
                                 join emp in db.tEmployeeTags on objI.RFID equals emp.RFID
                                 join ag in db.mAgencies on emp.mAgencyId equals ag.mAgencyId
                                 join ds in db.mDesignations on emp.mDesignationId equals ds.mDesignationId
                                 join sk in db.mSkillCategories on emp.mSkillCategoryId equals sk.mSkillCategoryId
                                 join wk in db.mWorkCategories on emp.mWorkCategoryId equals wk.mWorkCategoryId
                                 join ac in db.mActivities on emp.mActivityId equals ac.mActivityId

                                 join tin in db.mReaderSettups on objI.ReaderNo equals tin.ReaderNo
                                 join sm in db.mSiteMasters on tin.mSiteMasterId equals sm.mSiteMasterId
                                 join tzon in db.mZones on tin.mZoneId equals tzon.mZoneId
                                 join tszon in db.mFloorMasters on tin.mFloorMasterId equals tszon.mFloorMasterId
                                 where objI.tDate >= _fDate && objI.tDate <= tDate && objI.mAttPortId == tin.AttPortId

                                 select new
                                 {
                                     sm.Site,
                                     tzon.Zone,
                                     tszon.FloorName,
                                     EmployeeId = emp.EmployeeId ?? string.Empty,
                                     EmployeeName = emp.EmployeeName ?? string.Empty,
                                     Agency = ag.Agency ?? string.Empty,
                                     Designation = ds.Designation ?? string.Empty,
                                     SkillCategory = sk.SkillCategory ?? string.Empty,
                                     WorkCategory = wk.WorkCategory ?? string.Empty,
                                     Activity = ac.Activity ?? string.Empty,
                                     Name = emp.EmployeeName ?? string.Empty,
                                     objI.tDate
                                 }

                               ).Distinct().ToList();

                    var _Excdel = mData.GroupBy(n => new { n.EmployeeId, n.Name, n.Agency, n.Designation, n.WorkCategory, n.Activity, n.Site, n.Zone }, (key, group)
                        => new
                        {
                            key.EmployeeId,
                            key.Name,
                            key.Agency,
                            key.Designation,
                            key.WorkCategory,
                            key.Activity,
                            key.Site,
                            key.Zone
                        }).OrderBy(n => n.Name);

                    TempData["EmployeeWise"] = _Excdel;
                    result = this.Json(new { Flag = true, Message = "Suceess Data", data = mData }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return result;

        }
        //
        public ActionResult ExportExcel()
        {
            //var ExcelExportEmp = TempData["EmployeeLogDetail"] as List<EmployeeDetailLogEntity>;

            var ExcelExportEmp = TempData["EmployeeLogDetail"];// as List<EmployeeDetailLogEntity>;


            if (ExcelExportEmp != null)
            {
                var grid = new GridView();
                grid.DataSource = ExcelExportEmp;
                grid.DataBind();
                var FileName = "EmployeeDetailLog_" + DateTime.Now.ToString("s") + "_.xls";
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
                //string xml = String.Empty;
                //XmlDocument xmlDoc = new XmlDocument();
                //XmlSerializer xmlSerializer = new XmlSerializer(ExcelExport.GetType());
                //using (MemoryStream xmlStream = new MemoryStream())
                //{
                //    xmlSerializer.Serialize(xmlStream, ExcelExport);
                //    xmlStream.Position = 0;
                //    xmlDoc.Load(xmlStream);
                //    xml = xmlDoc.InnerXml;
                //}
                //var fName = string.Format("TrackSummary-{0}", DateTime.Now.ToString("s"));
                //byte[] fileContents = Encoding.UTF8.GetBytes(xml);
                //return File(fileContents, "application/ms-excel", fName);
            }
            else { return File("", "application/ms-excel", ""); }
        }
        //
        public ActionResult EmpWiseExportExcel()
        {
            var ExcelExport = TempData["EmployeeWise"];
            if (ExcelExport != null)
            {
                var grid = new GridView();
                grid.DataSource = ExcelExport;
                grid.DataBind();
                var FileName = "EmployeeTracked_" + DateTime.Now.ToString("s") + "_.xls";
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
        public async System.Threading.Tasks.Task<ActionResult> SafetyExportExcelAsync()
        {
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

                var ObjDatass = (from tm in tagsTrack
                                 join emp in db.tEmployeeTags on tm.Epc equals emp.RFID
                                 join ag in db.mAgencies on emp.mAgencyId equals ag.mAgencyId
                                 join ds in db.mDesignations on emp.mDesignationId equals ds.mDesignationId
                                 join sk in db.mSkillCategories on emp.mSkillCategoryId equals sk.mSkillCategoryId
                                 join wk in db.mWorkCategories on emp.mWorkCategoryId equals wk.mWorkCategoryId
                                 join ac in db.mActivities on emp.mActivityId equals ac.mActivityId
                                 join rst in db.mReaderSettups on tm.Reader equals rst.ReaderNo

                                 join msit in db.mSiteMasters on rst.mSiteMasterId equals msit.mSiteMasterId
                                 join mz in db.mZones on rst.mZoneId equals mz.mZoneId

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

                                     msit.Site,
                                     mz.Zone
                                 }
                             ).ToList();


                var objAsset = (from tm in tagsTrack
                                join ast in db.tAssetTags on tm.Epc equals ast.RFID

                                join itm in db.mIteamMasters on ast.mIteamMasterId equals itm.mIteamMasterId
                                join msit in db.mSiteMasters on ast.mSiteMasterId equals msit.mSiteMasterId
                                join mzon in db.mZones on ast.mZoneId equals mzon.mZoneId
                                join mflr in db.mFloorMasters on ast.mFloorMasterId equals mflr.mFloorMasterId
                                join mrom in db.mRoomMasters on ast.mRoomMasterId equals mrom.mRoomMasterId

                                join rst in db.mReaderSettups on tm.Reader equals rst.ReaderNo
                                where tm.PortId == rst.AttPortId

                                select new
                                {
                                    Asset = itm.IteamName,
                                    IteamName = ast.IteamName ?? string.Empty,
                                    IteamCode = ast.IteamCode ?? string.Empty,
                                    IteamDescription = ast.IteamDescription ?? string.Empty,
                                    ast.bStock,

                                    tm.Epc,
                                    tm.tDate,
                                    rst.mFloorMasterId,
                                    rst.mZoneId,
                                    eZoneId = ast.mZoneId,
                                    IsAE = false,

                                    msit.Site,
                                    mzon.Zone,
                                    Rack = mflr.FloorName,
                                    Shelf = mrom.RoomName
                                }
                            ).ToList();

                var q = (from c1 in ObjDatass
                         from c2 in objAsset
                         select new
                         {
                             ObjDatass = c1,
                             objAsset = c2
                         }).ToList();
                //var finalPO = objAsset.Union(ObjDatass).ToList();


                if (ObjDatass != null)
                {
                    var grid = new GridView();
                    grid.DataSource = ObjDatass;
                    grid.DataBind();
                    var FileName = "SafteyReportEmployee_" + DateTime.Now.ToString("s") + "_.xls";
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

                //if (objAsset != null)
                //{
                //    var grid = new GridView();
                //    grid.DataSource = objAsset;
                //    grid.DataBind();
                //    var FileName = "SafteyReportAsset_" + DateTime.Now.ToString("s") + "_.xls";
                //    Response.ClearContent();
                //    Response.AddHeader("content-disposition", "attachement; filename=" + FileName + "");
                //    Response.ContentType = "application/excel";
                //    StringWriter sw = new StringWriter();
                //    HtmlTextWriter htw = new HtmlTextWriter(sw);
                //    grid.RenderControl(htw);
                //    Response.Output.Write(sw.ToString());
                //    Response.Flush();
                //    Response.End();
                //    return View();

                //}
                else { return File("", "application/ms-excel", ""); }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }
        //
        public partial class spEmployeeAttendance_Result
        {
            public Nullable<System.DateTime> InTime { get; set; }
            public Nullable<System.DateTime> OutTime { get; set; }
            public string RFID { get; set; }
            public string ReaderNo { get; set; }
            public Nullable<int> Years { get; set; }
            public Nullable<int> Months { get; set; }
            public Nullable<int> Days { get; set; }
            public Nullable<int> Hours { get; set; }
            public Nullable<int> Minutes { get; set; }
            public Nullable<int> Seconds { get; set; }
            public string TimeSpend { get; set; }
            public Nullable<System.DateTime> at_Date { get; set; }
        }
    }
}
using HexaERP.MVC.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HexaERP.MVC.Controllers.Report
{
    public class EmployeeSummaryController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();
        static List<ExportData> EData = new List<ExportData>();
        // GET: EmployeeSummary
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


        public void ExportToExcel()
        {
            var gv = new GridView();
            DataTable dt = ToDataTable(EData);
            dt.Columns[2].ColumnName = "Location";
            dt.Columns[3].ColumnName = "Zone";
            dt.Columns[4].ColumnName = "Sub Zone";
            dt.Columns[5].ColumnName = "Time In";
            dt.Columns[6].ColumnName = "Time Out";
            dt.Columns[8].ColumnName = "Agency Name";
            dt.Columns[7].ColumnName = "Total Time Spent";
            dt.Columns.RemoveAt(13);
            dt.Columns["EmployeeId"].SetOrdinal(0);
            dt.Columns["EmployeeName"].SetOrdinal(1);
            dt.Columns["Agency Name"].SetOrdinal(2);
            dt.Columns["Designation"].SetOrdinal(3);
            dt.Columns["SkillCategory"].SetOrdinal(4);
            dt.Columns["WorkCategory"].SetOrdinal(5);
            dt.Columns["Activity"].SetOrdinal(6);
            dt.Columns["Location"].SetOrdinal(7);
            dt.Columns["Zone"].SetOrdinal(8);
            dt.Columns["Sub Zone"].SetOrdinal(9);
            dt.Columns["Time In"].SetOrdinal(10);
            dt.Columns["Time Out"].SetOrdinal(11);
            dt.Columns["Time Out"].SetOrdinal(11);
            dt.Columns["Total Time Spent"].SetOrdinal(12);
            gv.DataSource = dt;
            gv.DataBind();


            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=Employee Summary.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter objStringWriter = new StringWriter();
            HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            gv.RenderControl(objHtmlTextWriter);
            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();
        }
        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties by using reflection   
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names  
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {

                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        // GET: EmployeeSummary/Details/5
        //public ActionResult Details(int id)
        //{
        //    return View();
        //}
        // GET: EmployeeSummary/Details
        public JsonResult Details()
        {
            var jdata = db.tEmployeeTags.ToList();
            return Json(jdata, JsonRequestBehavior.AllowGet);
        }

        // GET: EmployeeSummary/Create
        public ActionResult Create()
        {
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



        // POST: EmployeeSummary/Create
        [HttpPost]
        public ActionResult CreateSubZone(SummaryReport obj)
        {

            // Initialization.    
            JsonResult result = new JsonResult();

            try
            {


                //DateTime tDate = ConvertToDateTime(obj.toDate);
                //string[] sDate = obj.toDate.Split('/');
                //string sDateTime = sDate[1] + '/' + sDate[0] + '/' + sDate[2];
                //DateTime tDate = Convert.ToDateTime(sDateTime);

                DateTime fDate = ConvertToDateTime(obj.fromDate);
                DateTime tDate = ConvertToDateTime(obj.toDate);

                if (obj == null || obj == null)
                {
                    return Json(new { Flag = false, Message = "Between Date Require" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var Idata = db.spNewReportSummary(fDate, tDate, obj.mSiteMasterId, obj.mZoneId, obj.mFloorMasterId, obj.mAgencyId, obj.mDesignationId, obj.mSkillCategoryId, obj.mWorkCategoryId, obj.mActivityId, obj.EmployeeName, obj.EmployeeId).ToList();
                    if (Idata != null)
                    {

                        List<ExportData> mData = (from objI in Idata
                                                  join emp in db.tEmployeeTags on objI.RFID equals emp.RFID
                                                  join ag in db.mAgencies on emp.mAgencyId equals ag.mAgencyId
                                                  join ds in db.mDesignations on emp.mDesignationId equals ds.mDesignationId
                                                  join sk in db.mSkillCategories on emp.mSkillCategoryId equals sk.mSkillCategoryId
                                                  join wk in db.mWorkCategories on emp.mWorkCategoryId equals wk.mWorkCategoryId
                                                  join ac in db.mActivities on emp.mActivityId equals ac.mActivityId
                                                  select new ExportData
                                                  {
                                                      EmployeeId = objI.EmployeeId ?? string.Empty,
                                                      EmployeeName = objI.EmployeeName ?? string.Empty,
                                                      Agency = ag.Agency ?? string.Empty,
                                                      Designation = ds.Designation ?? string.Empty,
                                                      SkillCategory = sk.SkillCategory ?? string.Empty,
                                                      WorkCategory = wk.WorkCategory ?? string.Empty,
                                                      Activity = ac.Activity ?? string.Empty,
                                                      TrackSite = objI.TrackSite ?? string.Empty,
                                                      TrackZone = objI.TrackZone ?? string.Empty,
                                                      TrackSubZone = objI.TrackSubZone ?? string.Empty,
                                                      InTime = objI.InTime,
                                                      OutTime = objI.OutTime,
                                                      TimeSpend = objI.TimeSpend ?? string.Empty,
                                                      //TimeSpendZone=objI.TimeSpendZone,
                                                      Name = emp.EmployeeName ?? string.Empty
                                                  }

                          ).ToList();
                        EData = mData as List<ExportData>;
                        result = this.Json(new { Flag = true, Message = "Suceess Data", data = mData }, JsonRequestBehavior.AllowGet);

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


        // POST: EmployeeSummary/Create
        [HttpPost]
        public ActionResult CreateZone(SummaryReport obj)
        {

            // Initialization.    
            JsonResult result = new JsonResult();

            try
            {



                //string[] sDate = obj.toDate.Split('/');
                //string sDateTime = sDate[1] + '/' + sDate[0] + '/' + sDate[2];
                //DateTime tDate = Convert.ToDateTime(sDateTime);


                DateTime fDate = ConvertToDateTime(obj.fromDate);
                DateTime tDate = ConvertToDateTime(obj.toDate);

                if (obj == null || obj == null)
                {
                    return Json(new { Flag = false, Message = "Between Date Require" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var Idata = db.spNewZoneReportSummary(fDate, tDate, obj.mSiteMasterId, obj.mZoneId, obj.mFloorMasterId, obj.mAgencyId, obj.mDesignationId, obj.mSkillCategoryId, obj.mWorkCategoryId, obj.mActivityId, obj.EmployeeName, obj.EmployeeId).ToList();
                    if (Idata != null)
                    {

                        List<ExportData> mData = (from objI in Idata
                                                  join emp in db.tEmployeeTags on objI.RFID equals emp.RFID
                                                  join ag in db.mAgencies on emp.mAgencyId equals ag.mAgencyId
                                                  join ds in db.mDesignations on emp.mDesignationId equals ds.mDesignationId
                                                  join sk in db.mSkillCategories on emp.mSkillCategoryId equals sk.mSkillCategoryId
                                                  join wk in db.mWorkCategories on emp.mWorkCategoryId equals wk.mWorkCategoryId
                                                  join ac in db.mActivities on emp.mActivityId equals ac.mActivityId
                                                  select new ExportData
                                                  {
                                                      EmployeeId = objI.EmployeeId ?? string.Empty,
                                                      EmployeeName = objI.EmployeeName ?? string.Empty,
                                                      Agency = ag.Agency ?? string.Empty,
                                                      Designation = ds.Designation ?? string.Empty,
                                                      SkillCategory = sk.SkillCategory ?? string.Empty,
                                                      WorkCategory = wk.WorkCategory ?? string.Empty,
                                                      Activity = ac.Activity ?? string.Empty,
                                                      TrackSite = objI.TrackSite ?? string.Empty,
                                                      TrackZone = objI.TrackZone ?? string.Empty,
                                                      InTime = objI.InTime,
                                                      OutTime = objI.OutTime,
                                                      TimeSpend = objI.TimeSpend ?? string.Empty,
                                                      //TimeSpendZone=objI.TimeSpendZone,
                                                      Name = emp.EmployeeName ?? string.Empty
                                                  }

                          ).ToList();
                        EData = mData as List<ExportData>;
                        result = this.Json(new { Flag = true, Message = "Suceess Data", data = mData }, JsonRequestBehavior.AllowGet);

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

        // GET: EmployeeSummary/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: EmployeeSummary/Edit/5
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

        // GET: EmployeeSummary/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: EmployeeSummary/Delete/5
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

    }
    public class ExportData
    {
        public string EmployeeName { get; set; }
        public string EmployeeId { get; set; }
        public string TrackSite { get; set; }
        public string TrackZone { get; set; }
        public string TrackSubZone { get; set; }
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

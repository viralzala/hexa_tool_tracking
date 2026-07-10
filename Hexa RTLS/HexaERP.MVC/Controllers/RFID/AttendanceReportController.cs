using HexaERP.MVC.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class AttendanceReportController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();
        // GET: AttendanceReport
        public ActionResult Index()
        {
            try
            {
                //--- Get cookie Collection.
                HttpCookie cookieObject = Request.Cookies["HexaCookie"];
                //--- Check for null 
                if (cookieObject != null)
                {

                }
                else { return RedirectToAction("Index", "AppUser"); }

                if (Session["UniqueId"].ToString() != "" && Session["OrgInfoId"].ToString() != "" && Session["AppUserName"].ToString() != "")
                {
                    //string Page_Name = Path.GetFileName(Request.Path);
                    if (Convert.ToString(Session["SortCode"]) != "AD")
                    {
                        return RedirectToAction("Index", "AppUser");
                    }
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

        // 
        [HttpGet]
        public JsonResult GetEmployeeAtt(string FromDate, string EndDate)
        {
            if (FromDate == null || FromDate == "" || EndDate == null || EndDate == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            DateTime sDate = Convert.ToDateTime(FromDate); DateTime EDate = Convert.ToDateTime(EndDate);

            string startDate = ((DateTime)sDate).ToString("yyyy-MM-dd"); string endDate = ((DateTime)EDate).ToString("yyyy-MM-dd");

            var ObjD = db.spEmployeeAtt(sDate, EDate).ToList();
            return Json(ObjD, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetData(string FromDate, string EndDate)
        {
            // Initialization.    
            JsonResult result = new JsonResult();
            try
            {
                if (FromDate == null || FromDate == "" || EndDate == null || EndDate == "")
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }

                DateTime sDate = Convert.ToDateTime(FromDate); DateTime EDate = Convert.ToDateTime(EndDate);

                string startDate = ((DateTime)sDate).ToString("yyyy-MM-dd"); string endDate = ((DateTime)EDate).ToString("yyyy-MM-dd");

                var ObjD = await db.Database.SqlQuery<spEmployeeAtt_Result>("spEmployeeAtt {0}, {1}",
               new object[] { sDate, EDate }).ToListAsync();

                //var ObjD = db.spEmployeeAtt(sDate, EDate).ToList();
                //return Json(ObjD, JsonRequestBehavior.AllowGet);
                result = this.Json(new { data = ObjD }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Info    
                Console.Write(ex);
            }
            // Return info.    
            return result;
        }

    }
}
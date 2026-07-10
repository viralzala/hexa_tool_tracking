using HexaERP.MVC.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class EmployeeTrackReportController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();
        // GET: EmployeeTrackReport
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
        public JsonResult GetEmployeeTrack(string FromDate, string EndDate)
        {
            if (FromDate == null || FromDate == "" || EndDate == null || EndDate == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            DateTime sDate = Convert.ToDateTime(FromDate); DateTime EDate = Convert.ToDateTime(EndDate);
            var ObjD = db.spEmployeeTrackReport(sDate, EDate).ToList();
            return Json(ObjD, JsonRequestBehavior.AllowGet);
        }
    }
}
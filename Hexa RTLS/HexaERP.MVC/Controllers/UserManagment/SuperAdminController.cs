using System;
using System.ServiceProcess;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.UserManagment
{
    public class SuperAdminController : Controller
    {


        public static string AppUserName, UniqueId;
        // GET: SuperAdmin
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
                    var dd = cookieObject["AppUserName"];
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
                    if (Convert.ToString(Session["SortCode"]) != "SA")
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

        //
        [HttpGet]
        public ActionResult StartService()
        {
            JsonResult result = new JsonResult();
            try
            {
                ServiceController myService = new ServiceController("HexaService");

                if (myService.Status == ServiceControllerStatus.Stopped)
                {
                    myService.Start();
                    myService.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10.0));

                    result = this.Json(new { Flag = true, Message = "HexaService Service Started Successfully" }, JsonRequestBehavior.AllowGet);
                }
                else if (myService.Status == ServiceControllerStatus.Running)
                {
                    result = this.Json(new { Flag = false, Message = "HexaService Service Is Running" }, JsonRequestBehavior.AllowGet);
                }
                else if (myService.Status == ServiceControllerStatus.Paused)
                {
                    result = this.Json(new { Flag = false, Message = "HexaService Service Is Paused" }, JsonRequestBehavior.AllowGet);
                }
                else if (myService.Status == ServiceControllerStatus.StopPending)
                {
                    result = this.Json(new { Flag = false, Message = "HexaService Service Is StopPending" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex) { result = this.Json(new { Flag = false, Message = ex.InnerException.Message }, JsonRequestBehavior.AllowGet); }
            return result;
        }

        //
        [HttpGet]
        public ActionResult StopService()
        {
            JsonResult result = new JsonResult();
            try
            {
                ServiceController myService = new ServiceController("HexaService");
                if (myService.Status == ServiceControllerStatus.Running)
                {
                    myService.Stop();
                    myService.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10.0));
                    result = this.Json(new { Flag = true, Message = "HexaService Service Stoped Successfully" }, JsonRequestBehavior.AllowGet);
                }
                else if (myService.Status == ServiceControllerStatus.Paused)
                {
                    result = this.Json(new { Flag = false, Message = "HexaService Service Is Paused" }, JsonRequestBehavior.AllowGet);
                }
                else if (myService.Status == ServiceControllerStatus.StopPending)
                {
                    result = this.Json(new { Flag = false, Message = "HexaService Service Is StopPending" }, JsonRequestBehavior.AllowGet);
                }
                else if (myService.Status == ServiceControllerStatus.Stopped)
                {
                    result = this.Json(new { Flag = false, Message = "HexaService Service Is Alredy Stopped" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex) { result = this.Json(new { Flag = false, Message = ex.InnerException.Message }, JsonRequestBehavior.AllowGet); }
            return result;
        }

        //
        [HttpGet]
        public ActionResult HexaServiceStatus()
        {
            JsonResult result = new JsonResult();
            try
            {
                ServiceController myService = new ServiceController("HexaService");

                if (myService.Status == ServiceControllerStatus.Stopped)
                {
                    result = this.Json(new { Flag = false, Message = "HexaService Service Is Stopped" }, JsonRequestBehavior.AllowGet);
                }
                else if (myService.Status == ServiceControllerStatus.Running)
                {
                    result = this.Json(new { Flag = true, Message = "HexaService Service Is Running" }, JsonRequestBehavior.AllowGet);
                }
                else if (myService.Status == ServiceControllerStatus.Paused)
                {
                    result = this.Json(new { Flag = false, Message = "HexaService Service Is Paused" }, JsonRequestBehavior.AllowGet);
                }
                else if (myService.Status == ServiceControllerStatus.StopPending)
                {
                    result = this.Json(new { Flag = false, Message = "HexaService Service Is Stop Pending" }, JsonRequestBehavior.AllowGet);
                }
                else if (myService.Status == ServiceControllerStatus.PausePending)
                {
                    result = this.Json(new { Flag = false, Message = "HexaService Service Is Pause Pending" }, JsonRequestBehavior.AllowGet);
                }
                else if (myService.Status == ServiceControllerStatus.ContinuePending)
                {
                    result = this.Json(new { Flag = false, Message = "HexaService Service Is Continue Pending" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex) { result = this.Json(new { Flag = false, Message = ex.InnerException.Message }, JsonRequestBehavior.AllowGet); }
            return result;
        }
    }
}
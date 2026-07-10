using HexaERP.MVC.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.UserManagment
{
    public class UserHomeController : Controller
    {
        // GET: UserHome
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
                }
                else { return RedirectToAction("Index", "AppUser"); }

            }
            catch (Exception)
            {
                return RedirectToAction("Index", "AppUser");
            }
            return View();
        }

        public JsonResult getCookie()
        {
            HttpCookie cookieObject = Request.Cookies["HexaCookie"];
            //--- Check for null           
            var AppUserName = "";
            if (cookieObject != null)
            {                //--- To read values from cookie collection we will use Keys used while creating cookie.                   
                AppUserName = cookieObject["AppUserName"];
            }
            return Json(new { LogedUser = AppUserName }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllMenus()
        {
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                int orginf = Convert.ToInt32(Session["OrgInfoId"]);
                var MenusData = (from am in HexaErpobj.AppMenus
                                 join md in HexaErpobj.HexaModules on am.ModuleID equals md.moduleID
                                 join ra in HexaErpobj.RoleAcesses on am.AppMenuId equals ra.AppMenuId
                                 join au in HexaErpobj.AppUsers on ra.AppUserId equals au.AppUserId
                                 where (ra.OrgInfoId == orginf)
                                 select new
                                 {
                                     MenuName = am.MenuName,
                                     MenuUrl = am.MenuUrl,
                                     PageName = am.PageName,
                                     moduleName = md.moduleName,
                                     modulepath = md.modulepath,
                                     IsAllowed = ra.IsAllowed,
                                     IsRead = ra.IsRead,
                                     Iswrite = ra.Iswrite,
                                     IsSuperUser = ra.IsSuperUser,
                                     AppUserId = ra.AppUserId,
                                     AppUserName = au.AppUserName
                                 }).ToList();
                return Json(MenusData, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
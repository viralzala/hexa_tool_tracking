using HexaERP.MVC.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Mvc;


namespace HexaERP.MVC.Controllers.UserManagment
{
    public class WindowManagementController : Controller
    {
        // GET: WindowManagement
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult getmodulename()
        {
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                //var moduleslist = HexaErpobj.AppMenus.ToList();
                var moduleslist = (from p in HexaErpobj.HexaModules
                                   join c in HexaErpobj.AppMenus on p.moduleID equals c.ModuleID into g
                                   select new { moduleID = p.moduleID, modulename = p.moduleName }).ToList();
                return Json(moduleslist, JsonRequestBehavior.AllowGet);
            }
        }
        public string SaveWindow(string JsonData)
        {
            var obj = JsonConvert.DeserializeObject<AppMenu>(JsonData);
            string stringassigned = "";
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                obj.CreatedBy = Session["AppUserName"].ToString();
                obj.CreatedDate = System.DateTime.Now;
                HexaErpobj.AppMenus.Add(obj);
                HexaErpobj.SaveChanges();
            }
            return stringassigned;
        }
        public JsonResult BindWindowDatatable()
        {
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                var ListofWindows = (from s in HexaErpobj.AppMenus
                                     from t in HexaErpobj.HexaModules
                                     where s.ModuleID == t.moduleID
                                     select new { AppMenuId = s.AppMenuId, PageName = s.PageName, moduleName = t.moduleName, MenuName = s.MenuName, MenuUrl = s.MenuUrl }).ToList();
                return Json(ListofWindows, JsonRequestBehavior.AllowGet);
            }
        }
        public string DeleteWindowData(int WindowId)
        {
            string i = "";
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                HexaErpobj.AppMenus.RemoveRange(HexaErpobj.AppMenus.Where(u => u.AppMenuId == WindowId));
                HexaErpobj.SaveChanges();
                i = "Module record Deleted";
                return i;
            }
        }
        public JsonResult getwindowdata(int windowid)
        {
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                var listofdata = HexaErpobj.AppMenus.Where(u => u.AppMenuId == windowid).ToList();
                return Json(listofdata, JsonRequestBehavior.AllowGet);
            }
        }
        public string UpdateWindow(string JsonData, string AppMenuIdhidden)
        {
            string i = "";
            if (JsonData != null)
            {
                using (ERPdbEntities HexaErpobj = new ERPdbEntities())
                {
                    var obj = JsonConvert.DeserializeObject<AppMenu>(JsonData);
                    int windowid = Convert.ToInt32(AppMenuIdhidden);
                    AppMenu databasetablelist = HexaErpobj.AppMenus.Where(b => b.AppMenuId == windowid).FirstOrDefault();
                    databasetablelist.MenuName = obj.MenuName;
                    databasetablelist.Description = obj.Description;
                    databasetablelist.MenuUrl = obj.MenuUrl;
                    databasetablelist.PageName = obj.PageName;
                    databasetablelist.ModuleID = obj.ModuleID;
                    HexaErpobj.SaveChanges();
                    i = "Updated Successfully";
                    return i;
                }
            }
            else
            {
                return i = "Data is Not updated";
            }
        }
    }
}
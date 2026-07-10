using HexaERP.MVC.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.UserManagment
{
    public class ModuleManagementController : Controller
    {
        // GET: ModuleManagement
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetAllIcons()
        {
            //if (Session["UserID"] == null) { return Json(""); }
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                var IconsList = HexaErpobj.Icons.ToList();
                return Json(IconsList, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult BindModuleDatatable()
        {
            //if (Session["UserID"] == null) { return Json(""); }
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                // int uid = Convert.ToInt32(Session["UserID"]);
                var ListofModules = (from u in HexaErpobj.HexaModules select u).ToList();
                return Json(ListofModules, JsonRequestBehavior.AllowGet);
            }
        }
        public string SaveModuledata(string icon, string description, string moduleName)
        {
            string stringassigned = "";
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                HexaModule obj = new HexaModule();
                obj.moduleName = moduleName;
                obj.description = description;
                obj.IsAction = true;
                obj.icon = "&" + icon;
                HexaErpobj.HexaModules.Add(obj);
                HexaErpobj.SaveChanges();
            }
            return stringassigned;
        }
        public string UpdateModule(string icon, string description, string moduleName, string ModuleIDFOREDITINGHIDDEN)
        {
            string i = "";
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                int moduleid = Convert.ToInt32(ModuleIDFOREDITINGHIDDEN);
                HexaModule databasetablelist = HexaErpobj.HexaModules.Where(b => b.moduleID == moduleid).FirstOrDefault();
                databasetablelist.moduleName = moduleName;
                databasetablelist.description = description;
                databasetablelist.icon = "&" + icon;
                HexaErpobj.SaveChanges();
                i = "Updated Successfully";
                return i;
            }

        }
        public string DeleteModuleData(int ModuleId)
        {
            string i = "";
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                HexaErpobj.HexaModules.RemoveRange(HexaErpobj.HexaModules.Where(x => x.moduleID == ModuleId));
                HexaErpobj.SaveChanges();
                i = "Module record Deleted";
                return i;
            }
        }
    }
}
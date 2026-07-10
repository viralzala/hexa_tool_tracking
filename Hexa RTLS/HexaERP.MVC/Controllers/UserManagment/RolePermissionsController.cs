using HexaERP.MVC.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.UserManagment
{
    public class RolePermissionsController : Controller
    {
        // GET: RolePermissions
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult getusername()
        {
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                int orgid = Convert.ToInt32(Session["OrgInfoId"]);
                var listofusers = HexaErpobj.AppUsers.Where(b => b.OrgInfoId == orgid && b.AppRoleId == 3).ToList();
                return Json(listofusers, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getmodulename()
        {
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {

                int orgid = Convert.ToInt32(Session["OrgInfoId"]);

                var moduleslist = (from p in HexaErpobj.Rolemodules
                                   join c in HexaErpobj.HexaModules on p.moduleID equals c.moduleID
                                   where p.OrgInfoId == orgid
                                   select new { RolemoduleId = p.RolemoduleId, moduleName = c.moduleName }).ToList();
                return Json(moduleslist, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getwindowsnamesaccordingtomodule(int Rolemoduleid)
        {
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                int orgid = Convert.ToInt32(Session["OrgInfoId"]);
                var listofwindows = (from t in HexaErpobj.RoleMenus
                                     join p in HexaErpobj.AppMenus on t.AppMenuId equals p.AppMenuId
                                     where t.RolemoduleId == Rolemoduleid
                                     select new { RoleMenuId = t.RoleMenuId, AppMenuId = t.AppMenuId, MenuName = p.MenuName }).ToList();
                return Json(listofwindows, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public string SaveRolePermissions(string formdata, string str2, string IsRead, string IsWrite)
        {
            string stringassigned = "";
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                var obj = JsonConvert.DeserializeObject<RoleAcess>(formdata);

                string pattern = "!";
                string replacement = "-";
                string result = Regex.Replace(str2, pattern, replacement);
                int gg = result.Split(',').Count();
                for (int i = 0; i < gg; i++)
                {
                    string value = result.Split(',')[i];
                    int val = value.Split('-').Count();
                    int AppMenuId = Convert.ToInt32(value.Split('-')[0]);
                    int Rolemenuid = Convert.ToInt32(value.Split('-')[1]);
                    obj.AppMenuId = AppMenuId;
                    obj.RoleMenuId = Rolemenuid;
                    if (IsRead == "IsRead")
                    {
                        obj.IsRead = true;
                    }
                    else
                    {
                        obj.IsRead = false;
                    }
                    if (IsWrite == "IsWrite")
                    {
                        obj.Iswrite = true;
                    }
                    else
                    {
                        obj.Iswrite = false;
                    }
                    obj.IsAllowed = true;
                    obj.IsModifiy = true;
                    obj.IsSuperUser = false;
                    obj.CreatedBy = Session["AppUserName"].ToString();
                    obj.CreatedDate = System.DateTime.Now;
                    obj.OrgInfoId = Convert.ToInt32(Session["OrgInfoId"]);
                    HexaErpobj.RoleAcesses.Add(obj);
                    HexaErpobj.SaveChanges();

                }
            }
            return stringassigned;
        }
        public string Deletebtnassign(int raccessid, int RMenuid)
        {
            string i = "";
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                HexaErpobj.RoleAcesses.RemoveRange(HexaErpobj.RoleAcesses.Where(u => u.RoleAcessId == raccessid));
                HexaErpobj.SaveChanges();
                i = "Access Removed";
                return i;
            }
        }
        public JsonResult BindRolesDatatable()
        {
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                var data = (from a in HexaErpobj.RoleAcesses
                            from t in HexaErpobj.OrgInfoes
                            where a.OrgInfoId == t.OrgInfoId
                            from u in HexaErpobj.AppUsers
                            where a.AppUserId == u.AppUserId
                            from me in HexaErpobj.AppMenus
                            where a.AppMenuId == me.AppMenuId
                            from rmen in HexaErpobj.RoleMenus
                            where a.AppMenuId == rmen.AppMenuId
                            from rmod in HexaErpobj.Rolemodules
                            where rmen.RolemoduleId == rmod.RolemoduleId
                            from mod in HexaErpobj.HexaModules
                            where mod.moduleID == rmod.moduleID
                            select new
                            {
                                RoleAcessId = a.RoleAcessId,
                                RoleMenuId = a.RoleMenuId,
                                OrgInfoName = t.OrgInfoName,
                                AppUserName = u.AppUserName,
                                MenuName = me.MenuName,
                                moduleName = mod.moduleName,
                                IsRead = a.IsRead,
                                Iswrite = a.Iswrite
                            }).ToList();
                // var data = HexaErpobj.RoleAcesses


                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
    }

}
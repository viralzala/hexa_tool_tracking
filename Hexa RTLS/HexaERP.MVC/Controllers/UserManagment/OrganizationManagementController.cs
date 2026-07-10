using HexaERP.MVC.Models;
using HexaERP.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.UserManagment
{
    public class OrganizationManagementController : Controller
    {
        EncryptionDecryption Seq = new EncryptionDecryption();
        // GET: OrganizationManagement
        public ActionResult Index()
        {
            try
            {
                if (Session["UniqueId"].ToString() != "" && Session["OrgInfoId"].ToString() != "" && Session["AppUserName"].ToString() != "")
                {
                    // string Page_Name = Path.GetFileName(Request.Path);
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
        public string SaveOrganization(string JsonData)
        {
            var obj = JsonConvert.DeserializeObject<OrgInfo>(JsonData);
            string stringassigned = "";
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                obj.CreatedBy = Session["AppUserName"].ToString();
                obj.CreatedDate = System.DateTime.Now;
                HexaErpobj.OrgInfoes.Add(obj);
                HexaErpobj.SaveChanges();


                string Uid = Guid.NewGuid().ToString().GetHashCode().ToString("X");

                string AppUserName = "admin";
                string Password = Seq.Encrypt(Uid);
                int AppRoleId = 1;
                var UserName = Session["AppUserName"].ToString();
                var cFlag = HexaErpobj.Database.ExecuteSqlCommand(
                          "INSERT INTO AppUser(AppUserName,AppRoleId,Password,IsAction,CreatedDate,CreatedBy,OrgInfoId,IsAllowed)" +
                          "VALUES('" + AppUserName + "','" + AppRoleId + "','" + Password + "','" + true + "',GETDATE(),'" + UserName + "','" + obj.OrgInfoId + "','" + true + "')");



                ////
                //int _OrgInfoId = obj.OrgInfoId;
                //try
                //{
                //    var cFlag = HexaErpobj.Database.ExecuteSqlCommand("INSERT INTO ConfigureModels (moduleID,OrgInfoId) SELECT moduleID,'" + _OrgInfoId + "' FROM HexaModule WHERE IsAction = 1");
                //    if (cFlag >= 1)
                //    {
                //        string CreatedBy = Session["AppUserName"].ToString();
                //        HexaErpobj.Database.ExecuteSqlCommand("INSERT INTO Rolemodule (moduleID,OrgInfoId) SELECT moduleID,'" + _OrgInfoId + "' FROM HexaModule WHERE IsAction = 1");
                //        HexaErpobj.Database.ExecuteSqlCommand("INSERT INTO RoleMenu (RolemoduleId,AppMenuId,OrgInfoId,CreatedDate,CreatedBy) SELECT RM.RolemoduleId, AM.AppMenuId,'" + _OrgInfoId + "',GETDATE(),'" + CreatedBy + "' FROM Rolemodule RM LEFT JOIN AppMenu AM ON RM.moduleID = AM.moduleID");
                //    }
                //}
                //catch (Exception ex) {
                //}

            }
            return stringassigned;
        }
        [HttpGet]
        public string btnassignwindowssave(string Iswrite, string IsRead, string Datefrom, string Dateto, string orgid, string moduleid, string windowid)
        {
            string stringassigned = "";
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                Rolemodule objrolemodule = new Rolemodule();
                RoleMenu objrolemenu = new RoleMenu();
                objrolemodule.moduleID = Convert.ToInt32(moduleid);
                objrolemodule.OrgInfoId = Convert.ToInt32(orgid);
                objrolemodule.Datefrom = Convert.ToDateTime(Datefrom);
                objrolemodule.Dateto = Convert.ToDateTime(Dateto);
                objrolemodule.IsAllowed = true;
                HexaErpobj.Rolemodules.Add(objrolemodule);
                HexaErpobj.SaveChanges();
                string pattern = "!";
                string replacement = "'";
                string input = windowid;
                string result = Regex.Replace(input, pattern, replacement);

                int gg = result.Split(',').Count();

                for (int i = 0; i < gg; i++)
                {
                    string value = result.Split(',')[i];

                    int id = objrolemodule.RolemoduleId;
                    objrolemenu.AppMenuId = Convert.ToInt32(value);
                    objrolemenu.OrgInfoId = Convert.ToInt32(orgid);
                    objrolemenu.RolemoduleId = id;
                    objrolemenu.IsAllowed = true;
                    objrolemenu.CreatedBy = Session["AppUserName"].ToString();
                    objrolemenu.CreatedDate = System.DateTime.Now;

                    if (IsRead == "Read")
                    {
                        objrolemenu.IsRead = true;
                    }
                    else { objrolemenu.IsRead = false; }
                    if (Iswrite == "Write")
                    {
                        objrolemenu.Iswrite = true;
                    }
                    else { objrolemenu.Iswrite = false; }
                    objrolemenu.IsAllowed = true;

                    HexaErpobj.RoleMenus.Add(objrolemenu);
                    HexaErpobj.SaveChanges();
                }



            }
            return stringassigned;
        }
        public JsonResult BindOrganizationDatatable()
        {
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                var listoforganizations = HexaErpobj.OrgInfoes.ToList();
                return Json(listoforganizations, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult BindmodulesandwindowsassignedDatatable()
        {
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                var listofboth = (from p in HexaErpobj.Rolemodules
                                  join c in HexaErpobj.RoleMenus
                                  on p.RolemoduleId equals c.RolemoduleId
                                  join modules in HexaErpobj.HexaModules
                                  on p.moduleID equals modules.moduleID
                                  join menus in HexaErpobj.AppMenus
                                  on c.AppMenuId equals menus.AppMenuId
                                  join orgg in HexaErpobj.OrgInfoes
                                  on c.OrgInfoId equals orgg.OrgInfoId
                                  select new
                                  {
                                      RolemoduleId = p.RolemoduleId,
                                      RoleMenuId = c.RoleMenuId,
                                      OrgInfoName = orgg.OrgInfoName,
                                      moduleName = modules.moduleName,
                                      MenuName = menus.MenuName,
                                      IsRead = c.IsRead,
                                      IsWrite = c.Iswrite
                                  }).ToList();
                return Json(listofboth, JsonRequestBehavior.AllowGet);
            }
        }
        public string Deleteorganization(int organizationid)
        {
            string i = "";
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                HexaErpobj.OrgInfoes.RemoveRange(HexaErpobj.OrgInfoes.Where(u => u.OrgInfoId == organizationid));
                HexaErpobj.SaveChanges();

                HexaErpobj.AppUsers.RemoveRange(HexaErpobj.AppUsers.Where(u => u.OrgInfoId == organizationid && u.AppRoleId == 1));
                HexaErpobj.SaveChanges();

                i = "Organization record Deleted";
                return i;
            }
        }
        public string Deletebtnassign(int RModuleid, int RMenuid)
        {
            string i = "";
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                HexaErpobj.RoleMenus.RemoveRange(HexaErpobj.RoleMenus.Where(u => u.RoleMenuId == RMenuid));
                HexaErpobj.SaveChanges();
                i = "Window Removed";
                return i;
            }
        }
        public JsonResult getorganization(int organizationid)
        {
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                var listofdata = HexaErpobj.OrgInfoes.Where(u => u.OrgInfoId == organizationid).ToList();
                return Json(listofdata, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getOrganizationname()
        {
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                var listoforganizationslist = HexaErpobj.OrgInfoes.ToList();
                return Json(listoforganizationslist, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getwindowsnamesaccordingtomodule(int moduleid, int orgid)
        {
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                //var listofwindows = HexaErpobj.AppMenus.Where(b => b.moduleID == moduleid).ToList();
                var listofwindows = (from t in HexaErpobj.AppMenus
                                     where t.ModuleID == moduleid && !!
                                             (from m in HexaErpobj.RoleMenus where m.OrgInfoId == orgid select m.AppMenuId)
                                         .Contains(t.AppMenuId)
                                     select t).ToList();
                return Json(listofwindows, JsonRequestBehavior.AllowGet);
            }
        }
        public string btnupdatewindowsupdate(string JsonData, string rolemodulehidden, string rolemenuhidden)
        {
            string i = "";
            if (JsonData != null)
            {
                using (ERPdbEntities HexaErpobj = new ERPdbEntities())
                {
                    var Rolemoduleobj = JsonConvert.DeserializeObject<Rolemodule>(JsonData);
                    var RoleMenuobj = JsonConvert.DeserializeObject<RoleMenu>(JsonData);

                    int rolemodulehiddenint = Convert.ToInt32(rolemodulehidden);
                    Rolemodule databasetablelist = HexaErpobj.Rolemodules.Where(b => b.RolemoduleId == rolemodulehiddenint).FirstOrDefault();
                    databasetablelist.Datefrom = Rolemoduleobj.Datefrom;
                    databasetablelist.Dateto = Rolemoduleobj.Dateto;
                    databasetablelist.moduleID = Rolemoduleobj.moduleID;
                    databasetablelist.OrgInfoId = Rolemoduleobj.moduleID;
                    HexaErpobj.SaveChanges();


                    int rolemenuhiddenint = Convert.ToInt32(rolemenuhidden);
                    RoleMenu databasetablelistmenu = HexaErpobj.RoleMenus.Where(b => b.RoleMenuId == rolemenuhiddenint).FirstOrDefault();
                    databasetablelistmenu.OrgInfoId = RoleMenuobj.OrgInfoId;
                    databasetablelistmenu.RolemoduleId = rolemodulehiddenint;
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
        public string UpdateOrganization(string JsonData, string orghidenid)
        {
            string i = "";
            if (JsonData != null)
            {
                using (ERPdbEntities HexaErpobj = new ERPdbEntities())
                {
                    var obj = JsonConvert.DeserializeObject<OrgInfo>(JsonData);

                    int organizationid = Convert.ToInt32(orghidenid);
                    OrgInfo databasetablelist = HexaErpobj.OrgInfoes.Where(b => b.OrgInfoId == organizationid).FirstOrDefault();
                    databasetablelist.OrgInfoName = obj.OrgInfoName;
                    databasetablelist.ProductService = obj.ProductService;
                    databasetablelist.Adress = obj.Adress;
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
        public JsonResult getorganizationPass(int OrgId)
        {
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                //var obj = (from Obj in HexaErpobj.AppUsers
                //           where (Obj.OrgInfoId == OrgId && Obj.IsAction == true && Obj.AppRoleId == 1)
                //           select new
                //           {
                //               Obj.Password
                //           }).ToList();

                var obj = HexaErpobj.AppUsers.Where(Obj => Obj.OrgInfoId == OrgId && Obj.IsAction == true && Obj.AppRoleId == 1).ToList();
                AppUser _obj = new AppUser();
                List<AppUser> eList = new List<AppUser>();
                foreach (var item in obj)
                {
                    eList.Add(new AppUser
                    {
                        Password = Seq.Decrypt(item.Password)
                    });
                }

                return Json(eList, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
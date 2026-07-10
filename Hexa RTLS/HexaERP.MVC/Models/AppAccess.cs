using System;
using System.Linq;

namespace HexaERP.MVC.Models
{

    public class AppAccess
    {
        private ERPdbEntities db = new ERPdbEntities();
        public string GetViewPermission()
        {
            var cFlag = db.Database.ExecuteSqlCommand("SELECT AppUser.AppUserName,AppRole.SortCode,AppUser.IsAllowed,AppMenu.MenuName,AppMenu.MenuUrl,RoleAcess.IsRead,RoleAcess.Iswrite,RoleAcess.IsModifiy FROM AppUser LEFT JOIN AppRole ON AppUser.AppRoleId = AppRole.AppRoleId LEFT JOIN RoleAcess ON AppUser.AppUserId = RoleAcess.AppUserId LEFT JOIN AppMenu ON AppMenu.AppMenuId = RoleAcess.AppMenuId Where AppUser.AppUserName = 'mudassar' AND AppMenu.MenuUrl = 'Users.aspx'");

            return "Mudassar";
        }
        public bool GetViewPermission(string _User, string _PageURL, string _Action)
        {
            bool FlagReturn = false;
            using (ERPdbEntities db = new ERPdbEntities())
            {
                try
                {
                    //Get Organization Id From Session Variable
                    //int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                    //Get Selected Data Accourding to Org Id
                    var ObjData = (from AppUser in db.AppUsers
                                   join AppRole in db.AppRoles on AppUser.AppRoleId equals AppRole.AppRoleId
                                   join RoleAc in db.RoleAcesses on AppUser.AppUserId equals RoleAc.AppUserId // ()
                                   join AppMe in db.AppMenus on RoleAc.AppMenuId equals AppMe.AppMenuId
                                   where (AppUser.AppUserName == _User && AppMe.MenuUrl == _PageURL)
                                   select new
                                   {
                                       AppUser.AppUserName,
                                       AppRole.SortCode,
                                       AppUser.IsAllowed,
                                       AppMe.MenuName,
                                       AppMe.MenuUrl,
                                       RoleAc.IsRead,
                                       RoleAc.Iswrite,
                                       RoleAc.IsModifiy
                                   }).ToList();
                    //Convert List Data to The Json Array          
                    //return Json(ObjData, JsonRequestBehavior.AllowGet);
                    if (ObjData != null)
                    {
                        foreach (var objs in ObjData)
                        {
                            if (objs.SortCode == "SA")
                            {
                                FlagReturn = true;
                                // return ("Sucessfully Login !");
                            }
                            else if (objs.SortCode == "AD")
                            {
                                FlagReturn = true;
                                //return ("Sucessfully Login !");
                            }
                            else if (objs.SortCode == "OT")
                            {
                                if (_Action == "read")
                                {

                                    if (objs.IsRead == true)
                                    {
                                        FlagReturn = true;
                                    }
                                    else
                                    {
                                        FlagReturn = false;
                                    }

                                }
                                else if (_Action == "modify")
                                {
                                    if (objs.IsModifiy == true)
                                    {
                                        FlagReturn = true;
                                    }
                                    else
                                    {
                                        FlagReturn = false;
                                    }

                                }
                                else if (_Action == "write")
                                {
                                    if (objs.Iswrite == true)
                                    {
                                        FlagReturn = true;
                                    }
                                    else
                                    {
                                        FlagReturn = false;
                                    }

                                }

                            }
                            else
                            {
                                FlagReturn = false;
                            }

                        }
                    }
                }
                catch (Exception)
                {
                    // Response.Write("<script>alert(" + ex + ")</script>");
                }
            }
            return FlagReturn;
        }
    }
}
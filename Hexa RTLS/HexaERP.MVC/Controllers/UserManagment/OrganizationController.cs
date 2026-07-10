using System;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.UserManagment
{
    public class OrganizationController : Controller
    {
        // GET: Organization
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
    }
}
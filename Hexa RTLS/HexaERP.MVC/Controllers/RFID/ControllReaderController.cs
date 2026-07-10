using HexaERP.MVC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class ControllReaderController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();
        // GET: ControllReader
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

        [HttpGet]
        public async Task<JsonResult> GetReaderData()
        {
            var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            var _rdeaders = await db.mReaderSettups.Select(c => new { c.ReaderIP, c.ReaderNo, c.mReaderSettupId, c.IsAction, c.OrgInfoId }).Where(x => x.OrgInfoId == orgId).Distinct().ToListAsync();
            return Json(_rdeaders, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> setReaderContrl(int ID, bool status)
        {
            if (string.IsNullOrEmpty(Convert.ToString(ID)) || string.IsNullOrEmpty(Convert.ToString(status)))
            {
                return Json(new { Flag = false, Msg = "Error" }, JsonRequestBehavior.AllowGet);
            }
            var obj = await db.mReaderSettups.FindAsync(ID);
            obj.IsAction = status;
            db.SaveChanges();
            return Json(new { Flag = true, Msg = "Sucess" }, JsonRequestBehavior.AllowGet);
        }
    }
}
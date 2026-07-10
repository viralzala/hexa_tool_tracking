using HexaERP.MVC.Models;
using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.SMT
{
    public class VisteonAlertsController : Controller
    {
        public static string UserName = string.Empty;
        // GET: VisteonAlerts
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
                    UserName = cookieObject["AppUserName"];
                }
                else { return RedirectToAction("Index", "AppUser"); }
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "AppUser");
            }
            return View();
        }


        [HttpGet]
        public async Task<ActionResult> GetAllAlert()
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {


                using (var context = new ERPdbEntities())
                {
                    var _product = context.Database.SqlQuery<toTrackInfo>(@"select * from  toTrackInfo where UID = 'Alert' order by tDate desc").ToList();

                    if (_product.Count() > 0)
                        result = this.Json(new { Flag = true, message = "Record Found", _product }, JsonRequestBehavior.AllowGet);
                    else
                        result = this.Json(new { Flag = false, message = "Record Not Found" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = string.Empty;
                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    // Get entry
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;
                    // Display or log error messages
                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        message = string.Format("Error '{0}' occurred in {1} at {2}",
                                 subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                        Console.WriteLine(message);
                    }
                }
                result = this.Json(new { message, Flag = false }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }


        [HttpGet]
        public async Task<ActionResult> DeleteAlert(int id)
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {
                using (var context = new ERPdbEntities())
                {
                    var delObj = context.toTrackInfoes.Find(id);
                    if (delObj == null)
                    {
                        return this.Json(new { message = $"Record Not Found", Flag = false }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        context.toTrackInfoes.Remove(delObj);
                        context.SaveChanges();
                        return this.Json(new { message = $"Record Not deleted", Flag = true }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = string.Empty;
                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    // Get entry
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;
                    // Display or log error messages
                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        message = string.Format("Error '{0}' occurred in {1} at {2}",
                                 subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                        Console.WriteLine(message);
                    }
                }
                result = this.Json(new { message, Flag = false }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }
    }
}
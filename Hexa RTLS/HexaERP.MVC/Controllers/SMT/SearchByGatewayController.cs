using HexaERP.MVC.Models;
using System;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.SMT
{
    public class SearchByGatewayController : Controller
    {
        public static string UserName = string.Empty;
        // GET: SearchByGateway
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
        public async Task<ActionResult> CleanData()
        {
            JsonResult result = new JsonResult();
            try
            {
                using (var context = new ERPdbEntities())
                {
                    var _reader = ConfigurationManager.AppSettings["Smt:SearchReader"];
                    var clean = $"DELETE FROM toMonitor Where Name ={_reader}";
                    context.Database.ExecuteSqlCommand(clean);
                    result = this.Json(new { Flag = true, message = "Record Found", }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception) { }
            return result;
        }
        [HttpGet]
        public async Task<ActionResult> GatewayConnect()
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {
                using (var context = new ERPdbEntities())
                {
                    var _reader = ConfigurationManager.AppSettings["Smt:SearchReader"];
                    var _product = context.Database.SqlQuery<SMTSearchModelView>(@"select p.*,m.* from mSMTProduct as p left join toMonitor as m on p.Ble = m.Epc where m.Name = {0} ",
                                    new object[] { _reader }).ToList();

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
            catch (Exception e)
            {
            }
            return result;
        }


        [HttpGet]
        public async Task<ActionResult> GetSMTProdcutInfo(string Search)
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {
                var _reader = ConfigurationManager.AppSettings["Smt:SearchReader"];

                string _serach = Search.Trim();

                using (var context = new ERPdbEntities())
                {
                    var _product = context.Database.SqlQuery<SMTSearchModelView>("select * from toMonitor where Epc={0} AND Name={1}",
                        new object[] { _serach, _reader }).ToList();

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
            catch (Exception e)
            {

            }
            return result;
        }
    }
}
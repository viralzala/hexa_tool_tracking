using HexaERP.MVC.Models;
using HexaERP.MVC.Service;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.SMT
{
    public class ReturnApproveSMTBinController : Controller
    {
        public static string UserName = string.Empty;

        // GET: ReturnApproveSMTBin
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
        public async Task<ActionResult> GetReturnApproveSMTBin()
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {
                using (var context = new ERPdbEntities())
                {
                    var _product = await context.mSMTProducts.Where(f => f.IsNormalReturn == true).ToListAsync();
                    if (_product != null)
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

        [HttpPost]
        public ActionResult ReturnSMTBinRequest(string SMTProductIds, string Remark)
        {
            JsonResult result = new JsonResult();
            try
            {
                if (string.IsNullOrWhiteSpace(SMTProductIds))
                    return this.Json(new { Flag = false, Message = $"Select Product" }, JsonRequestBehavior.AllowGet);

                if (string.IsNullOrWhiteSpace(Remark))
                    return this.Json(new { Flag = false, Message = $"Enter Remark" }, JsonRequestBehavior.AllowGet);

                List<int> _SMTProductIds = null;
                _SMTProductIds = ConList(SMTProductIds);

                int RecordJob = 0;
                using (var context = new ERPdbEntities())
                {
                    _SMTProductIds.ForEach(o =>
                    {
                        var _r = context.mSMTProducts.Find(o);

                        if (_r != null)
                        {
                            if (_r.IsNormalReturn)
                            {
                                var _findShelf = context.mShelves.Where(a => a.IsAction == false).FirstOrDefault();
                                if (_findShelf != null)
                                {
                                    _r.mZoneId = _findShelf.mZoneId;
                                    _r.ShelfName = _findShelf.ShelfName;
                                    _findShelf.IsAction = true;
                                    _findShelf.Barcode = _r.Ble;
                                    context.Entry(_findShelf).State = EntityState.Modified;
                                    context.SaveChanges();
                                }

                                _r.Remark = Remark;
                                _r.Status = JobStatus.ReturnRequestApproved;
                                _r.IsNormalReturn = false;
                                _r.IsPutaway = false;
                                _r.IsTakeaway = false;
                                _r.IsAssembly = false;
                                _r.ModifiedDate = DateTime.Now;
                                _r.ModifiedBy = UserName;

                                context.Entry(_r).State = EntityState.Modified;
                                context.SaveChanges();

                                RecordJob = RecordJob + 1;
                            }
                        }

                    });
                }
                if (RecordJob > 0)
                    result = this.Json(new { Flag = true, Message = $"{RecordJob} request approved succssfully", }, JsonRequestBehavior.AllowGet);
                else
                    result = this.Json(new { Flag = false, Message = $"request not not found" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                result = this.Json(new { Flag = false, Message = ex.InnerException.Message }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }

        public static List<int> ConList(string a)
        {
            try
            {
                return a.Split(',').Select(x => x.Trim()).Select(x => Int32.Parse(x)).ToList();
            }
            catch (Exception) { return null; }
        }
    }
}
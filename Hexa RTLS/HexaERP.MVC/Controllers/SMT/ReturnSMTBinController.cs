using HexaERP.MVC.Models;
using HexaERP.MVC.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.SMT
{
    public class ReturnSMTBinController : Controller
    {
        public static string UserName = string.Empty;

        // GET: ReturnSMTBin
        [Authorize(Roles = "AD,AAD")]
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
        public async Task<ActionResult> GetReturnSMTBin(string Search)
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {
                string _serach = Search.Trim();

                using (var context = new ERPdbEntities())
                {
                    var _product = context.Database.SqlQuery<mSMTProduct>(@"select * 
from mSMTProduct
where p.IsPutaway = 1 AND p.IsTakeaway = 1 AND
  ({0} IS NULL OR PartNumber LIKE '%' + {0} + '%')
OR ({0} IS NULL OR Status LIKE '%' + {0} + '%')
OR ({0} IS NULL OR SerialNumber LIKE '%' + {0} + '%')
OR ({0} IS NULL OR CustomerCode LIKE '%' + {0} + '%')
OR ({0} IS NULL OR Lot LIKE '%' + {0} + '%')
OR ({0} IS NULL OR Ble LIKE '%' + {0} + '%')
OR ({0} IS NULL OR ShelfName LIKE '%' + {0} + '%')
OR ({0} IS NULL OR ModifiedBy LIKE '%' + {0} + '%')",
                    new object[] { Search }).ToList();

                    if (_product.Count > 0)
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

        public class ReturnListModelView
        {
            public int RowNumber { get; set; }
            public int NumberControlValue { get; set; }
        }
        [HttpPost]
        public ActionResult ReturnSMTBinRequest(string data, string ModifiedBy, string Remark, string Comment)
        {
            JsonResult result = new JsonResult();
            try
            {
                if (string.IsNullOrWhiteSpace(ModifiedBy) || ModifiedBy == "undefined")
                    return this.Json(new { Flag = false, Message = $"Employee id is required" }, JsonRequestBehavior.AllowGet);

                //if ())
                //    return this.Json(new { Flag = false, Message = $"Select Product" }, JsonRequestBehavior.AllowGet);

                if (string.IsNullOrWhiteSpace(Remark))
                    return this.Json(new { Flag = false, Message = $"Enter Remark" }, JsonRequestBehavior.AllowGet);

                List<int> _SMTProductIds = null;

                //_SMTProductIds = ConList(SMTProductIds);
                string _ble = string.Empty;
                var l = JsonConvert.DeserializeObject<List<ReturnListModelView>>(data);

                int RecordJob = 0;
                l.ForEach(o =>
                {
                    using (var context = new ERPdbEntities())
                    {
                        var _r = context.mSMTProducts.Find(o.RowNumber);
                        if (_r != null)
                        {
                            try
                            {
                                var _b = context.SmtProductAuditLogs.Where(p => p.mSMTProductId == _r.mSMTProductId).OrderByDescending(d => d.TrxTimestamp).FirstOrDefault().OldRowData;
                                if (_b != null)
                                {
                                    _ble = JsonConvert.DeserializeObject<mSMTProduct>(_b).Ble;
                                }
                            }
                            catch (Exception ex)
                            {
                                WriteError.WriteErrorLog($"{MethodBase.GetCurrentMethod().DeclaringType.FullName} :{DateTime.Now} Exception :{ex.Message} |  InnerException :{ex.InnerException?.Message}");
                            }
                            if (o.NumberControlValue <= _r.Quantity)
                                if (_r.IsTakeaway == true && _r.IsNormalReturn == false)
                                {
                                    _r.Ble = _ble;
                                    _r.Status = JobStatus.ReturnRequest;
                                    _r.IsNormalReturn = true;
                                    _r.ModifiedDate = DateTime.Now;
                                    _r.ModifiedBy = ModifiedBy;
                                    _r.Remark = Remark;
                                    _r.Comment = Comment;
                                    _r.Quantity = o.NumberControlValue;
                                    context.Entry(_r).State = EntityState.Modified;
                                    context.SaveChanges();
                                    RecordJob = RecordJob + 1;
                                }
                                else
                                    WriteError.WriteErrorLog($"{MethodBase.GetCurrentMethod().DeclaringType.FullName} :{DateTime.Now} Exception :entered quantity more then existing");
                        }
                    }
                });

                if (RecordJob > 0)
                    result = this.Json(new { Flag = true, Message = $"{RecordJob} request submited succssfully", }, JsonRequestBehavior.AllowGet);
                else
                    result = this.Json(new { Flag = false, Message = $"request not submited becouse record not found" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                WriteError.WriteErrorLog($"{MethodBase.GetCurrentMethod().DeclaringType.FullName} :{DateTime.Now} Exception :{ex.Message} |  InnerException :{ex.InnerException?.Message}");
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
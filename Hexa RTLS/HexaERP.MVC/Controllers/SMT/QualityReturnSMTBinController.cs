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
    public class QualityReturnSMTBinController : Controller
    {
        public static string UserName = string.Empty;
        // GET: QualityReturnSMTBin
        [Authorize(Roles = "AD,AAD,QAD")]
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
        public async Task<ActionResult> GetQualityReturnSMTBin(string Search)
        {
            // Initialization.
            JsonResult result = new JsonResult();
            try
            {
                string _serach = Search.Trim();

                using (var context = new ERPdbEntities())
                {
                    var _product = context.Database.SqlQuery<SMTSearchModelView>(@"select p.*,m.*
from mSMTProduct as p
left join toMonitor as m on p.Ble = m.Epc
where p.IsPutaway = 1 AND p.IsTakeaway = 1 AND
  ({0} IS NULL OR p.PartNumber LIKE '%' + {0} + '%')
OR ({0} IS NULL OR Status LIKE '%' + {0} + '%')
OR ({0} IS NULL OR p.SerialNumber LIKE '%' + {0} + '%')
OR ({0} IS NULL OR p.CustomerCode LIKE '%' + {0} + '%')
OR ({0} IS NULL OR p.Lot LIKE '%' + {0} + '%')
OR ({0} IS NULL OR p.Ble LIKE '%' + {0} + '%')
OR ({0} IS NULL OR p.ShelfName LIKE '%' + {0} + '%')
OR ({0} IS NULL OR p.ModifiedBy LIKE '%' + {0} + '%')",
                    new object[] { Search }).ToList();

                    //                    var _product = context.Database.SqlQuery<mSMTProduct>(@"select * 
                    //from mSMTProduct
                    //where IsTakeaway = 1 AND
                    //  ({0} IS NULL OR PartNumber LIKE '%' + {0} + '%')
                    //OR ({0} IS NULL OR Status LIKE '%' + {0} + '%')
                    //OR ({0} IS NULL OR SerialNumber LIKE '%' + {0} + '%')
                    //OR ({0} IS NULL OR CustomerCode LIKE '%' + {0} + '%')
                    //OR ({0} IS NULL OR Lot LIKE '%' + {0} + '%')
                    //OR ({0} IS NULL OR Ble LIKE '%' + {0} + '%')
                    //OR ({0} IS NULL OR ShelfName LIKE '%' + {0} + '%')
                    //OR ({0} IS NULL OR ModifiedBy LIKE '%' + {0} + '%')",
                    //                    new object[] { Search }).ToList();

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

        [HttpPost]
        public ActionResult QualityReturnSMTBinRequest(string SMTProductIds, string ModifiedBy, string Remark, string Comment)
        {
            JsonResult result = new JsonResult();
            try
            {
                if (string.IsNullOrWhiteSpace(ModifiedBy) || ModifiedBy == "undefined")
                    return this.Json(new { Flag = false, Message = $"Employee id is required" }, JsonRequestBehavior.AllowGet);

                if (string.IsNullOrWhiteSpace(SMTProductIds))
                    return this.Json(new { Flag = false, Message = $"Select Product" }, JsonRequestBehavior.AllowGet);

                if (string.IsNullOrWhiteSpace(Comment))
                    return this.Json(new { Flag = false, Message = $"Enter Comment" }, JsonRequestBehavior.AllowGet);

                List<int> _SMTProductIds = null;
                _SMTProductIds = ConList(SMTProductIds);
                string _ble = string.Empty;

                int RecordJob = 0;
                _SMTProductIds.ForEach(o =>
                {
                    using (var context = new ERPdbEntities())
                    {
                        var _r = context.mSMTProducts.Find(o);
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

                            if (_r.IsTakeaway == true && _r.IsQualityReturn == false)
                            {
                                _r.Ble = _ble;
                                _r.Status = JobStatus.QualityReturnRequest;
                                _r.IsQualityReturn = true;
                                _r.ModifiedDate = DateTime.Now;
                                _r.ModifiedBy = ModifiedBy;
                                _r.Remark = Remark;
                                _r.Comment = Comment;


                                context.Entry(_r).State = EntityState.Modified;
                                context.SaveChanges();
                                RecordJob = RecordJob + 1;
                            }
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
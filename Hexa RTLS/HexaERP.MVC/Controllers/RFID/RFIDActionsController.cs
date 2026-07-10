using HexaERP.MVC.EmailConfig;
using HexaERP.MVC.Models;
using Newtonsoft.Json;
using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    [Authorize]
    public class RFIDActionsController : Controller
    {
        [HttpGet]
        [Route("api/AssetCheckIn")]
        public HttpResponseMessage AssetCheckIn(int EmployeeId, int AssetId, string UserName, int orgId)
        {
            try
            {
                if (string.IsNullOrEmpty(EmployeeId.ToString())
                    || string.IsNullOrEmpty(AssetId.ToString()))
                {
                    var content = new { status = 0, message = "Something went wrong try again" };
                    return new HttpResponseMessage
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                    };

                }
                else
                {
                    using (var contx = new ERPdbEntities())
                    {
                        try
                        {
                            var _Atags = contx.tAssetTags.Find(AssetId);
                            if (_Atags.tEmployeeTagId == EmployeeId)
                            {
                                DateTime? _tempIssued;
                                _tempIssued = _Atags.IssueDate;
                                _Atags.tEmployeeTagId = null;
                                _Atags.IssueDate = null;
                                _Atags.ReturnDate = DateTime.Now;

                                _Atags.OrgInfoId = orgId;
                                _Atags.ModifiedBy = UserName.ToString();
                                _Atags.ModifiedDate = DateTime.Now;


                                contx.SaveChanges();
                                var ckinout = contx.tAssetCheckOuts
                                    .Where(x => x.tAssetTagId == AssetId && x.tEmployeeTagId == EmployeeId)
                                    .FirstOrDefault();

                                ckinout.ReturnDate = DateTime.Now;
                                contx.SaveChanges();

                                tEmployeeTag _Temp = contx.tEmployeeTags.Find(EmployeeId);

                                NotifyMail.TrasactionNotify(_Atags.IteamName, _Atags.ModelNo, _Atags.SerialNo, _Temp.EmployeeName, _Temp.EmployeeId, UserName.ToString(), _tempIssued, DateTime.Now, "Asset Returned From Employee");
                                var content = new
                                {
                                    status = 1,
                                    message = "Successfully Checking Asset!!",
                                    content = _Atags
                                };
                                return new HttpResponseMessage
                                {
                                    Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                                };
                            }
                            else
                            {
                                var content = new { status = 0, message = "No such employee found belongs to asset" };
                                return new HttpResponseMessage
                                {
                                    Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                                };
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
                            var content = new { status = 0, message };
                            return new HttpResponseMessage
                            {
                                Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                            };
                        }
                        catch (Exception ex)
                        {
                            var content = new { status = 0, message = ex.Message };
                            return new HttpResponseMessage
                            {
                                Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                            };
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                var content = new { status = 0, message = ex.Message };
                return new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                };
            }
        }

        //
        [HttpPost]
        [Route("api/AssetCheckOut")]
        public HttpResponseMessage AssetCheckOut(tAssetTag obj)
        {
            try
            {
                if (string.IsNullOrEmpty(obj.tEmployeeTagId.ToString())
                    || string.IsNullOrEmpty(obj.tAssetTagId.ToString()))
                {
                    var content = new { status = 0, message = "Something went wrong try again" };
                    return new HttpResponseMessage
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                    };

                }
                else
                {
                    using (var contx = new ERPdbEntities())
                    {
                        try
                        {

                            var _Atags = contx.tAssetTags.Find(obj.tAssetTagId);

                            if (_Atags.tEmployeeTagId != null)
                            {

                                var ckinout = new tAssetCheckOut();
                                _Atags.tEmployeeTagId = obj.tEmployeeTagId;
                                ckinout.tEmployeeTagId = obj.tEmployeeTagId;
                                if (!string.IsNullOrEmpty(obj.IssueDate.ToString()))
                                {
                                    DateTime sDate = Convert.ToDateTime(obj.IssueDate);
                                    _Atags.IssueDate = sDate;
                                    ckinout.IssueDate = sDate;
                                }
                                if (!string.IsNullOrEmpty(obj.ReturnDate.ToString()))
                                {
                                    DateTime EDate = Convert.ToDateTime(obj.ReturnDate);
                                    _Atags.ReturnDate = EDate;
                                    ckinout.ReturnDate = EDate;
                                }

                                _Atags.OrgInfoId = obj.OrgInfoId;
                                _Atags.CreatedBy = obj.CreatedBy;
                                _Atags.CreatedDate = DateTime.Now;
                                ckinout.tAssetTagId = obj.tAssetTagId;
                                ckinout.IsAction = true;
                                ckinout.OrgInfoId = obj.OrgInfoId;
                                ckinout.CreatedBy = obj.CreatedBy;
                                ckinout.CreatedDate = DateTime.Now;
                                //db.Entry(obj).State = EntityState.Modified;
                                contx.SaveChanges();
                                contx.tAssetCheckOuts.Add(ckinout);
                                contx.SaveChanges();
                                //
                                var _Temp = contx.tEmployeeTags.Find(obj.tEmployeeTagId);
                                NotifyMail.TrasactionNotify(_Atags.IteamName, _Atags.ModelNo, _Atags.SerialNo, _Temp.EmployeeName, _Temp.EmployeeId, obj.CreatedBy, _Atags.IssueDate, _Atags.ReturnDate, "Asset Issued To Employee");
                                var content = new
                                {
                                    status = 1,
                                    message = "Successfully Checking Asset!!",
                                    content = _Atags
                                };
                                return new HttpResponseMessage
                                {
                                    Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                                };
                            }
                            else
                            {
                                var content = new { status = 0, message = "Asset is already assigned" };
                                return new HttpResponseMessage
                                {
                                    Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                                };
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
                            var content = new { status = 0, message };
                            return new HttpResponseMessage
                            {
                                Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                            };
                        }
                        catch (Exception ex)
                        {
                            var content = new { status = 0, message = ex.Message };
                            return new HttpResponseMessage
                            {
                                Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                            };
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                var content = new { status = 0, message = ex.Message };
                return new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                };
            }
        }

        //
        [HttpGet]
        [Route("api/RFIDBullkScanning")]
        public HttpResponseMessage RFIDBullkScanning(string RFID)
        {
            try
            {
                if (string.IsNullOrEmpty(RFID))
                {
                    var content = new { status = 0, message = "RFID Missing" };
                    return new HttpResponseMessage
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                    };

                }
                else
                {
                    //string[] m_separators = new string[] { "," };
                    //string[] m_stringarray = RFID.Split(m_separators, StringSplitOptions.RemoveEmptyEntries);
                    //string s = string.Format("'{0}'", string.Join("','", RFID));
                    using (var contx = new ERPdbEntities())
                    {
                        try
                        {
                            var _AssetEmpList = contx.Database.SqlQuery<AssetWithEmployeeEntity>("spBulkScan {0}",
                                new object[] { RFID })
                                .ToList();

                            if (_AssetEmpList != null)
                            {
                                var content = new
                                {
                                    status = 1,
                                    message = "Successfully",
                                    content = _AssetEmpList
                                };
                                return new HttpResponseMessage
                                {
                                    Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                                };
                            }
                            else
                            {
                                var content = new { status = 0, message = "Record Not Found" };
                                return new HttpResponseMessage
                                {
                                    Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                                };
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
                            var content = new { status = 0, message };
                            return new HttpResponseMessage
                            {
                                Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                            };
                        }
                        catch (Exception ex)
                        {
                            var content = new { status = 0, message = ex.Message };
                            return new HttpResponseMessage
                            {
                                Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                            };
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                var content = new { status = 0, message = ex.Message };
                return new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                };
            }
        }
    }
}

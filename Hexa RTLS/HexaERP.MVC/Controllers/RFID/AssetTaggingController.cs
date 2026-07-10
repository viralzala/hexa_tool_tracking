using HexaERP.MVC.EmailConfig;
using HexaERP.MVC.Models;
using Newtonsoft.Json;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    [Authorize]
    public class AssetTaggingController : Controller
    {
        [HttpPost]
        [Route("api/AssetInfoTag")]
        public HttpResponseMessage AssetInfoTag(tAssetTag collection)
        {
            try
            {
                using (var contx = new ERPdbEntities())
                {
                    try
                    {

                        if (string.IsNullOrEmpty(collection.RFID))
                        {
                            var content = new { status = 0, message = "RFID Should Not be Empty" };
                            return new HttpResponseMessage
                            {
                                Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                            };
                        }
                        else if (string.IsNullOrEmpty(collection.IteamName))
                        {
                            var content = new { status = 0, message = "Enter Asset Name" };
                            return new HttpResponseMessage
                            {
                                Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                            };
                        }

                        else if (collection.mIteamMasterId == null || collection.mGroupMasterId == null || collection.mIteamTypeMasterId == null)
                        {
                            var content = new { status = 0, message = "Missing master mandatory Data /Asset Information" };
                            return new HttpResponseMessage
                            {
                                Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                            };

                        }
                        else if (string.IsNullOrEmpty(Convert.ToString(collection.Stock)) || string.IsNullOrEmpty(Convert.ToString(collection.mUnitMasterId)))
                        {
                            var content = new { status = 0, message = "Missing Stock Information" };

                            return new HttpResponseMessage
                            {
                                Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                            };
                        }

                        else if (string.IsNullOrEmpty(Convert.ToString(collection.PurchaseDate)))
                        {
                            var content = new { status = 0, message = "Missing Purchase Date" };
                            return new HttpResponseMessage
                            {
                                Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                            };
                        }
                        else
                        {
                            if (contx.tAssetTags.Any(o => o.RFID == collection.RFID && o.OrgInfoId == collection.OrgInfoId && o.IsAction == true))
                            {
                                var content = new { status = 0, message = "Same RFID Alerdy Exist" };
                                return new HttpResponseMessage
                                {
                                    Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                                };
                            }
                            else
                            {
                                tAssetStockIn Sin = new tAssetStockIn();
                                tRFIDType Rtype = new tRFIDType();
                                String Uid = DateTime.Now.ToString().GetHashCode().ToString("x");
                                collection.UID = Uid;
                                collection.bStock = collection.Stock;
                                collection.OrgInfoId = collection.OrgInfoId;
                                //                    
                                collection.CreatedDate = DateTime.Now;
                                collection.IsAction = true;
                                contx.tAssetTags.Add(collection);
                                contx.SaveChanges();

                                //
                                Sin.UID = Uid;
                                Sin.OrgInfoId = collection.OrgInfoId;
                                Sin.tAssetTagId = collection.tAssetTagId;
                                Sin.RFID = collection.RFID;
                                Sin.Stock = collection.Stock;
                                Sin.bStock = collection.Stock;
                                Sin.CreatedDate = DateTime.Now;
                                Sin.CreatedBy = collection.CreatedBy;
                                Sin.IsAction = true;
                                contx.tAssetStockIns.Add(Sin);
                                contx.SaveChanges();

                                //
                                Rtype.RerfrenceId = collection.tAssetTagId;
                                Rtype.RFID = collection.RFID;
                                Rtype.Name = collection.IteamName;
                                Rtype.LocationId = collection.mRoomMasterId;
                                Rtype.Type = false;
                                Rtype.IsAction = true;
                                contx.tRFIDTypes.Add(Rtype);
                                contx.SaveChanges();

                                NotifyMail
                                    .AddNotify(collection.IteamName, collection.SerialNo, collection.CreatedBy, DateTime.Now, "New Asset Record Added");

                                var content = new
                                {
                                    status = 1,
                                    message = "Asset Record Added Successfully",
                                    content = collection
                                };
                                return new HttpResponseMessage
                                {
                                    Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                                };
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
                        var content = new { status = 0, message };
                        return new HttpResponseMessage
                        {
                            Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                        };
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
        // POST: api/AssetInfoTagEdit
        [HttpPost]
        [Route("api/AssetInfoTagEdit")]
        public HttpResponseMessage AssetInfoTagEdit(tAssetTag collection)
        {
            try
            {
                using (var contx = new ERPdbEntities())
                {
                    try
                    {

                        if (string.IsNullOrEmpty(Convert.ToString(collection.tAssetTagId)))
                        {
                            var content = new { status = 0, message = "Incorrect Data please refresh you application" };
                            return new HttpResponseMessage
                            {
                                Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                            };
                        }
                        if (string.IsNullOrEmpty(collection.RFID))
                        {
                            var content = new { status = 0, message = "RFID Should Not be Empty" };
                            return new HttpResponseMessage
                            {
                                Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                            };
                        }
                        else if (string.IsNullOrEmpty(collection.IteamName))
                        {
                            var content = new { status = 0, message = "Enter Asset Name" };
                            return new HttpResponseMessage
                            {
                                Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                            };

                        }
                        else if (collection.mIteamMasterId == null || collection.mGroupMasterId == null || collection.mIteamTypeMasterId == null)
                        {
                            var content = new { status = 0, message = "Missing master mandatory Data /Asset Information" };
                            return new HttpResponseMessage
                            {
                                Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                            };

                        }
                        else if (string.IsNullOrEmpty(collection.PurchaseDate.ToString()))
                        {
                            var content = new { status = 0, message = "Missing Purchase Date" };
                            return new HttpResponseMessage
                            {
                                Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                            };
                        }
                        else
                        {
                            if (contx.tAssetTags.Any(o => o.tAssetTagId == collection.tAssetTagId && o.OrgInfoId == collection.OrgInfoId))
                            {
                                tAssetTag EditObj = contx.tAssetTags.Find(collection.tAssetTagId);

                                tAssetStockIn stockInObj = contx.tAssetStockIns.FirstOrDefault(x => x.tAssetTagId == collection.tAssetTagId);

                                EditObj.mIteamMasterId = collection.mIteamMasterId;
                                EditObj.IteamName = collection.IteamName;
                                EditObj.BarCode = collection.BarCode;
                                EditObj.RFID = collection.RFID;
                                EditObj.IteamDescription = collection.IteamDescription;

                                EditObj.Model = collection.Model;
                                EditObj.ModelNo = collection.ModelNo;
                                EditObj.SerialNo = collection.SerialNo;
                                EditObj.Manufacturer = collection.Manufacturer;
                                EditObj.PurchaseCost = collection.PurchaseCost;
                                EditObj.mVendorId = collection.mVendorId;

                                EditObj.mUnitMasterId = collection.mUnitMasterId;
                                EditObj.mGroupMasterId = collection.mGroupMasterId;
                                EditObj.mIteamTypeMasterId = collection.mIteamTypeMasterId;

                                EditObj.Depreciation = collection.Depreciation;
                                EditObj.Receivedby = collection.Receivedby;
                                EditObj.DefaultWarranty = collection.DefaultWarranty;


                                EditObj.mSiteMasterId = collection.mSiteMasterId;
                                EditObj.mZoneId = collection.mZoneId;
                                EditObj.mFloorMasterId = collection.mFloorMasterId;
                                EditObj.mRoomMasterId = collection.mRoomMasterId;

                                EditObj.mSiteMasterId = collection.mSiteMasterId;
                                EditObj.mZoneId = collection.mZoneId;
                                EditObj.mFloorMasterId = collection.mFloorMasterId;
                                EditObj.mRoomMasterId = collection.mRoomMasterId;

                                EditObj.PurchaseDate = collection.PurchaseDate;

                                EditObj.OrgInfoId = collection.OrgInfoId;
                                EditObj.ModifiedBy = collection.ModifiedBy;

                                EditObj.ModifiedDate = DateTime.Now;
                                contx.Entry(EditObj).State = EntityState.Modified;
                                contx.SaveChanges();

                                stockInObj.OrgInfoId = collection.OrgInfoId;
                                stockInObj.ModifiedBy = collection.ModifiedBy;

                                stockInObj.ModifiedDate = DateTime.Now;
                                contx.Entry(EditObj).State = EntityState.Modified;
                                contx.SaveChanges();

                                NotifyMail.ModifyNotify(collection.IteamName, collection.SerialNo, collection.ModifiedBy, DateTime.Now, "Asset Record Modified");
                                var content = new
                                {
                                    status = 1,
                                    message = "Asset Record Updated Successfully",
                                    content = collection
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
            catch (Exception ex)
            {
                var content = new { status = 0, message = ex.Message };
                return new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                };
            }
        }

        // GET: api/AssetInfoTagEdit
        [HttpGet]
        [Route("api/AssetInfoTagEditData")]
        public HttpResponseMessage AssetInfoTagEditData(int id)
        {
            try
            {
                if (string.IsNullOrEmpty(Convert.ToString(id)))
                {
                    var content = new { status = 0, message = "Id Could Not Found" };
                    return new HttpResponseMessage
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                    };
                }
                using (var contx = new ERPdbEntities())
                {
                    try
                    {
                        var EditData = contx.tAssetTags.Find(id);
                        if (EditData == null)
                        {
                            var content = new { status = 0, message = "Data Could Not Found" };
                            return new HttpResponseMessage
                            {
                                Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                            };
                        }
                        else
                        {
                            var content = new
                            {
                                status = 1,
                                message = "Asset Record Updated Successfully",
                                content = EditData
                            };
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
        [Route("api/AssetInfoTagDelete")]
        public HttpResponseMessage AssetInfoTagDelete(int id)
        {

            try
            {
                if (string.IsNullOrEmpty(Convert.ToString(id)))
                {
                    var content = new { status = 0, message = "Id Could Not Found" };
                    return new HttpResponseMessage
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                    };
                }
                using (var contx = new ERPdbEntities())
                {
                    try
                    {
                        var _original = contx.tAssetTags
                .FirstOrDefault(b => b.tAssetTagId == id);
                        if (_original == null)
                        {
                            var content = new { status = 0, message = "Data is Not Deleted" };
                            return new HttpResponseMessage
                            {
                                Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                            };
                        }
                        else
                        {
                            var tAss = contx.tAssetStockIns
                                .FirstOrDefault(b => b.tAssetTagId == id);

                            contx.Entry(tAss).State = EntityState.Deleted;
                            contx.SaveChanges();
                            var _msgData = new
                            {
                                IteamName = _original.IteamName,
                                SerialNo = _original.SerialNo,
                                ModifiedBy = _original.ModifiedBy
                            };

                            contx.Entry(_original).State = EntityState.Deleted;
                            contx.SaveChanges();

                            NotifyMail.DeleteNotify(_msgData.IteamName, _msgData.SerialNo, _msgData.ModifiedBy, DateTime.Now, "Asset Record Deleted");
                            var content = new
                            {
                                status = 1,
                                message = "Data Deleted",
                                content = _original
                            };
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

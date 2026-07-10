using HexaERP.MVC.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace HexaERP.MVC.Controllers.RFID
{

    public class MasterDropdownController : ApiController
    {
        [HttpGet]
        [Route("api/Master/AssetCategoryList")]
        public HttpResponseMessage AssetCategoryList()
        {
            try
            {

                using (var contx = new ERPdbEntities())
                {
                    var AssetCategoryList = contx.mGroupMasters
                        .Where(x => x.IsAction == true)
                        .ToList();
                    if (AssetCategoryList != null)
                    {
                        var content = new { status = 1, message = "Successfully", content = AssetCategoryList };
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
        [Route("api/Master/AssetSubCategoryList")]
        public HttpResponseMessage AssetSubCategoryList()
        {
            try
            {

                using (var contx = new ERPdbEntities())
                {
                    var AssetSubCategoryList = contx.mIteamTypeMasters
                        .Where(x => x.IsAction == true)
                        .ToList();
                    if (AssetSubCategoryList != null)
                    {
                        var content = new { status = 1, message = "Successfully", content = AssetSubCategoryList };
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
        [Route("api/Master/AssetTypeList")]
        public HttpResponseMessage AssetTypeList()
        {
            try
            {

                using (var contx = new ERPdbEntities())
                {
                    var AssetTypeList = contx.mIteamMasters
                        .Where(x => x.IsAction == true)
                        .ToList();
                    if (AssetTypeList != null)
                    {
                        var content = new { status = 1, message = "Successfully", content = AssetTypeList };
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
        [Route("api/Master/VendorList")]
        public HttpResponseMessage VendorList()
        {
            try
            {

                using (var contx = new ERPdbEntities())
                {
                    var VendorList = contx.mVendors
                        .Where(x => x.IsAction == true)
                        .ToList();
                    if (VendorList != null)
                    {
                        var content = new { status = 1, message = "Successfully", content = VendorList };
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
        [Route("api/Master/UnitList")]
        public HttpResponseMessage UnitList()
        {
            try
            {

                using (var contx = new ERPdbEntities())
                {
                    var UnitList = contx.mVendors
                        .Where(x => x.IsAction == true)
                        .ToList();
                    if (UnitList != null)
                    {
                        var content = new { status = 1, message = "Successfully", content = UnitList };
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
        [Route("api/Master/LocationList")]
        public HttpResponseMessage LocationList()
        {
            try
            {
                using (var contx = new ERPdbEntities())
                {
                    var LocationList = contx.mSiteMasters
                        .Where(x => x.IsAction == true)
                        .ToList();
                    if (LocationList != null)
                    {
                        var content = new { status = 1, message = "Successfully", content = LocationList };
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
        [Route("api/Master/ZoneList")]
        public HttpResponseMessage ZoneList()
        {
            try
            {
                using (var contx = new ERPdbEntities())
                {
                    var ZoneList = contx.mZones
                        .Where(x => x.IsAction == true)
                        .ToList();
                    if (ZoneList != null)
                    {
                        var content = new { status = 1, message = "Successfully", content = ZoneList };
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
        [Route("api/Master/ShelfList")]
        public HttpResponseMessage ShelfList()
        {
            try
            {
                using (var contx = new ERPdbEntities())
                {
                    var ShelfList = contx.mFloorMasters
                        .Where(x => x.IsAction == true)
                        .ToList();
                    if (ShelfList != null)
                    {
                        var content = new { status = 1, message = "Successfully", content = ShelfList };
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
        [Route("api/Master/RackList")]
        public HttpResponseMessage RackList()
        {
            try
            {
                using (var contx = new ERPdbEntities())
                {
                    var RackList = contx.mRoomMasters
                        .Where(x => x.IsAction == true)
                        .ToList();
                    if (RackList != null)
                    {
                        var content = new { status = 1, message = "Successfully", content = RackList };
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
        [Route("api/Master/EmployeeList")]
        public HttpResponseMessage EmployeeList()
        {
            try
            {
                using (var contx = new ERPdbEntities())
                {
                    var EmployeeList = contx.tEmployeeTags
                        .Where(x => x.IsAction == true)
                        .ToList();
                    if (EmployeeList != null)
                    {
                        var content = new { status = 1, message = "Successfully", content = EmployeeList };
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
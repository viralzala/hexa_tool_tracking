using HexaERP.MVC.Models;
using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{

    public class IndoorMapController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();

        //public static List<IndoEmpEntity> empList = new List<IndoEmpEntity>();
        // GET: IndoorMap
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
                }
                else { return RedirectToAction("Index", "AppUser"); }

            }
            catch (Exception)
            {
                return RedirectToAction("Index", "AppUser");
            }
            return View();
        }

        //IndoorMap/getGetToTrackData
        [HttpGet]
        public async Task<ActionResult> getGetToTrackData()
        {
            // Initialization.    
            JsonResult result = new JsonResult();
            try
            {

                var tEmp = (from tm in db.toMonitors
                            join emp in db.tEmployeeTags on tm.Epc equals emp.RFID

                            join ag in db.mAgencies on emp.mAgencyId equals ag.mAgencyId
                            join ds in db.mDesignations on emp.mDesignationId equals ds.mDesignationId

                            join rst in db.mReaderSettups on tm.Name equals rst.ReaderNo
                            join tzn in db.mZones on rst.mZoneId equals tzn.mZoneId
                            join sel in db.mFloorMasters on rst.mFloorMasterId equals sel.mFloorMasterId
                            join rec in db.mRoomMasters on rst.mRoomMasterId equals rec.mRoomMasterId
                            where tm.AntennaPortNumber == rst.AttPortId

                            select new
                            {
                                EmailId = emp.EmailId ?? string.Empty,
                                ContactNo = emp.ContactNo ?? string.Empty,

                                Agency = ag.Agency ?? string.Empty,
                                Designation = ds.Designation ?? string.Empty,
                                Name = emp.EmployeeName ?? string.Empty,
                                EmployeeId = emp.EmployeeId ?? string.Empty,
                                tm.Epc,
                                tm.tDate,

                                Zone = tzn.Zone ?? string.Empty,
                                FloorName = sel.FloorName ?? string.Empty,
                                RoomName = rec.RoomName ?? string.Empty,


                                rst.mIndooMapsId,
                                rst.Xaxis,
                                rst.Yaxis
                            }
                             ).ToList();

                var tAsset = (from tm in db.toMonitors
                              join ast in db.tAssetTags on tm.Epc equals ast.RFID
                              join itm in db.mIteamMasters on ast.mIteamMasterId equals itm.mIteamMasterId
                              join msit in db.mSiteMasters on ast.mSiteMasterId equals msit.mSiteMasterId
                              join mzon in db.mZones on ast.mZoneId equals mzon.mZoneId
                              join mflr in db.mFloorMasters on ast.mFloorMasterId equals mflr.mFloorMasterId
                              join mrom in db.mRoomMasters on ast.mRoomMasterId equals mrom.mRoomMasterId
                              join rst in db.mReaderSettups on tm.Name equals rst.ReaderNo

                              join tzn in db.mZones on rst.mZoneId equals tzn.mZoneId
                              join sel in db.mFloorMasters on rst.mFloorMasterId equals sel.mFloorMasterId
                              join rec in db.mRoomMasters on rst.mRoomMasterId equals rec.mRoomMasterId

                              where tm.AntennaPortNumber == rst.AttPortId
                              select new
                              {
                                  img = ast.img ?? string.Empty,
                                  Asset = itm.IteamName,
                                  IteamName = ast.IteamName ?? string.Empty,
                                  Model = ast.Model ?? string.Empty,
                                  ModelNo = ast.ModelNo ?? string.Empty,

                                  IteamDescription = ast.IteamDescription ?? string.Empty,

                                  tm.Epc,
                                  tm.tDate,

                                  Site = msit.Site ?? string.Empty,
                                  Zone = mzon.Zone ?? string.Empty,
                                  FloorName = mflr.FloorName ?? string.Empty,
                                  RoomName = mrom.RoomName ?? string.Empty,

                                  tZone = tzn.Zone ?? string.Empty,
                                  tFloorName = sel.FloorName ?? string.Empty,
                                  tRoomName = rec.RoomName ?? string.Empty,


                                  rst.mIndooMapsId,
                                  rst.Xaxis,
                                  rst.Yaxis
                              }
                            ).ToList();

                var ObjEmp = (from tm in db.toMonitors
                              join emp in db.tEmployeeTags on tm.Epc equals emp.RFID
                              join ag in db.mAgencies on emp.mAgencyId equals ag.mAgencyId
                              join ds in db.mDesignations on emp.mDesignationId equals ds.mDesignationId
                              join sk in db.mSkillCategories on emp.mSkillCategoryId equals sk.mSkillCategoryId
                              join wk in db.mWorkCategories on emp.mWorkCategoryId equals wk.mWorkCategoryId
                              join ac in db.mActivities on emp.mActivityId equals ac.mActivityId

                              join rst in db.mReaderSettups on tm.Name equals rst.ReaderNo
                              where tm.AntennaPortNumber == rst.AttPortId

                              select new
                              {
                                  IsAE = true,
                                  rst.mReaderSettupId,
                                  rst.mIndooMapsId,
                                  rst.Xaxis,
                                  rst.Yaxis
                              }
                             ).ToList();

                var objAsset = (from tm in db.toMonitors
                                join ast in db.tAssetTags on tm.Epc equals ast.RFID
                                join itm in db.mIteamMasters on ast.mIteamMasterId equals itm.mIteamMasterId
                                join msit in db.mSiteMasters on ast.mSiteMasterId equals msit.mSiteMasterId
                                join mzon in db.mZones on ast.mZoneId equals mzon.mZoneId
                                join mflr in db.mFloorMasters on ast.mFloorMasterId equals mflr.mFloorMasterId
                                join mrom in db.mRoomMasters on ast.mRoomMasterId equals mrom.mRoomMasterId
                                join rst in db.mReaderSettups on tm.Name equals rst.ReaderNo
                                where tm.AntennaPortNumber == rst.AttPortId
                                select new
                                {
                                    IsAE = false,
                                    rst.mReaderSettupId,
                                    rst.mIndooMapsId,
                                    rst.Xaxis,
                                    rst.Yaxis
                                }
                            ).ToList();

                var comxresult = objAsset.Concat(ObjEmp).ToList();

                var totals = from e in comxresult
                             group e by new { e.mReaderSettupId, e.mIndooMapsId, e.Xaxis, e.Yaxis } into eg
                             select new { eg.Key.mReaderSettupId, eg.Key.mIndooMapsId, eg.Key.Xaxis, eg.Key.Yaxis, Count = eg.Count() };

                result = this.Json(new { ObjEmp = ObjEmp.ToArray(), objAsset = objAsset.ToArray(), totals, tEmp, tAsset }, JsonRequestBehavior.AllowGet);

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
            //string source = "";
            ////var path = System.IO.File.ReadAllText(Server.MapPath("~/Content/EmployeeTag.json"));
            //var path = Server.MapPath("~/Content/EmployeeTag.json");
            //using (StreamReader SourceReaderr = new StreamReader(path))
            //{
            //    source = await SourceReaderr.ReadToEndAsync();
            //}
            ////var path = System.IO.File.ReadAllText(Server.MapPath("~/Content/EmployeeTag.json"));
            //if (source != null)
            //{
            //    List<tToolTrackDemo> tagsTrack = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<tToolTrackDemo>>(source));
            //    //List<tToolTrackDemo> tagsTrack = JsonConvert.DeserializeObject<List<tToolTrackDemo>>(path);
            //    try
            //    {

            //        var tEmp = (from tm in tagsTrack
            //                    join emp in db.tEmployeeTags on tm.Epc equals emp.RFID

            //                    join ag in db.mAgencies on emp.mAgencyId equals ag?.mAgencyId
            //                    join ds in db.mDesignations on emp.mDesignationId equals ds?.mDesignationId                            

            //                    join rst in db.mReaderSettups on tm.Reader equals rst.ReaderNo
            //                    join tzn in db.mZones on rst.mZoneId equals tzn.mZoneId
            //                    join sel in db.mFloorMasters on rst.mFloorMasterId equals sel.mFloorMasterId
            //                    join rec in db.mRoomMasters on rst.mRoomMasterId equals rec.mRoomMasterId
            //                    where tm.PortId == rst.AttPortId
            //                    select new
            //                    {
            //                        EmailId = emp.EmailId ?? string.Empty,
            //                        ContactNo = emp.ContactNo ?? string.Empty,

            //                        Agency = ag.Agency ?? string.Empty,
            //                        Designation = ds.Designation ?? string.Empty,                                    
            //                        Name = emp.EmployeeName ?? string.Empty,
            //                        EmployeeId = emp.EmployeeId ?? string.Empty,
            //                        tm.Epc,
            //                        tm.tDate,

            //                        Zone = tzn.Zone ?? string.Empty,
            //                        FloorName = sel.FloorName ?? string.Empty,
            //                        RoomName = rec.RoomName ?? string.Empty,


            //                        rst.mIndooMapsId,
            //                        rst.Xaxis,
            //                        rst.Yaxis
            //                    }
            //                     ).ToList();

            //        var tAsset = (from tm in tagsTrack
            //                      join ast in db.tAssetTags on tm.Epc equals ast.RFID
            //                      join itm in db.mIteamMasters on ast.mIteamMasterId equals itm?.mIteamMasterId
            //                      join msit in db.mSiteMasters on ast.mSiteMasterId equals msit?.mSiteMasterId
            //                      join mzon in db.mZones on ast.mZoneId equals mzon?.mZoneId
            //                      join mflr in db.mFloorMasters on ast.mFloorMasterId equals mflr?.mFloorMasterId
            //                      join mrom in db.mRoomMasters on ast.mRoomMasterId equals mrom?.mRoomMasterId
            //                      join rst in db.mReaderSettups on tm.Reader equals rst.ReaderNo

            //                      join tzn in db.mZones on rst.mZoneId equals tzn.mZoneId
            //                      join sel in db.mFloorMasters on rst.mFloorMasterId equals sel?.mFloorMasterId
            //                      join rec in db.mRoomMasters on rst.mRoomMasterId equals rec?.mRoomMasterId

            //                      where tm.PortId == rst.AttPortId
            //                      select new
            //                      {
            //                          img = ast.img ?? string.Empty,
            //                          Asset = itm.IteamName,
            //                          IteamName = ast.IteamName ?? string.Empty,
            //                          Model = ast.Model ?? string.Empty,
            //                          ModelNo = ast.ModelNo ?? string.Empty,

            //                          IteamDescription = ast.IteamDescription ?? string.Empty,

            //                          tm.Epc,
            //                          tm.tDate,

            //                          Site = msit.Site?? string.Empty,
            //                          Zone = mzon.Zone ?? string.Empty,
            //                          FloorName = mflr.FloorName ?? string.Empty,
            //                          RoomName = mrom.RoomName ?? string.Empty,

            //                          tZone = tzn.Zone ?? string.Empty,
            //                          tFloorName = sel.FloorName ?? string.Empty,
            //                          tRoomName = rec.RoomName ?? string.Empty,


            //                          rst.mIndooMapsId,
            //                          rst.Xaxis,
            //                          rst.Yaxis
            //                      }
            //                    ).ToList();

            //        var ObjEmp = (from tm in tagsTrack
            //                      join emp in db.tEmployeeTags on tm.Epc equals emp.RFID
            //                      join ag in db.mAgencies on emp.mAgencyId equals ag.mAgencyId
            //                      join ds in db.mDesignations on emp.mDesignationId equals ds.mDesignationId
            //                      join sk in db.mSkillCategories on emp.mSkillCategoryId equals sk.mSkillCategoryId
            //                      join wk in db.mWorkCategories on emp.mWorkCategoryId equals wk.mWorkCategoryId
            //                      join ac in db.mActivities on emp.mActivityId equals ac.mActivityId

            //                      join rst in db.mReaderSettups on tm.Reader equals rst.ReaderNo
            //                      where tm.PortId == rst.AttPortId

            //                      select new
            //                      {
            //                          IsAE = true,
            //                          rst.mReaderSettupId,
            //                          rst.mIndooMapsId,
            //                          rst.Xaxis,
            //                          rst.Yaxis
            //                      }
            //                     ).ToList();

            //        var objAsset = (from tm in tagsTrack
            //                        join ast in db.tAssetTags on tm.Epc equals ast.RFID
            //                        join itm in db.mIteamMasters on ast.mIteamMasterId equals itm.mIteamMasterId
            //                        join msit in db.mSiteMasters on ast.mSiteMasterId equals msit.mSiteMasterId
            //                        join mzon in db.mZones on ast.mZoneId equals mzon.mZoneId
            //                        join mflr in db.mFloorMasters on ast.mFloorMasterId equals mflr.mFloorMasterId
            //                        join mrom in db.mRoomMasters on ast.mRoomMasterId equals mrom.mRoomMasterId
            //                        join rst in db.mReaderSettups on tm.Reader equals rst.ReaderNo
            //                        where tm.PortId == rst.AttPortId
            //                        select new
            //                        {
            //                            IsAE = false,
            //                            rst.mReaderSettupId,
            //                            rst.mIndooMapsId,
            //                            rst.Xaxis,
            //                            rst.Yaxis
            //                        }
            //                    ).ToList();

            //        var comxresult = objAsset.Concat(ObjEmp).ToList();

            //        var totals = from e in comxresult
            //                     group e by new { e.mReaderSettupId, e.mIndooMapsId, e.Xaxis, e.Yaxis } into eg
            //                     select new { eg.Key.mReaderSettupId, eg.Key.mIndooMapsId, eg.Key.Xaxis, eg.Key.Yaxis, Count = eg.Count() };

            //        result = this.Json(new { ObjEmp = ObjEmp.ToArray(), objAsset = objAsset.ToArray(), totals, tEmp, tAsset }, JsonRequestBehavior.AllowGet);

            //    }
            //    catch (DbEntityValidationException ex)
            //    {
            //        var message = string.Empty;
            //        foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
            //        {
            //            // Get entry
            //            DbEntityEntry entry = item.Entry;
            //            string entityTypeName = entry.Entity.GetType().Name;
            //            // Display or log error messages
            //            foreach (DbValidationError subItem in item.ValidationErrors)
            //            {
            //                message = string.Format("Error '{0}' occurred in {1} at {2}",
            //                         subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
            //                Console.WriteLine(message);
            //            }
            //        }
            //        result = this.Json(new { message, Flag = false }, JsonRequestBehavior.AllowGet);
            //    }

            //}
            //else
            //{
            //    result = this.Json(new { Message = "Null Data", Flag = false }, JsonRequestBehavior.AllowGet);
            //}
            return result;
        }

        // POST: IndoorMap/GetIndoorMaps
        [HttpGet]
        public JsonResult GetIndoorMaps()
        {
            // Initialization.    
            JsonResult result = new JsonResult();
            //var UserName = Session["AppUserName"];
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            try
            {
                var mData = db.mIndooMaps.Where(x => x.OrgInfoId == orgId && x.IsAction == true).ToList();
                var ObjData = (from mRd in db.mReaderSettups
                               join flr in db.mFloorMasters on mRd.mFloorMasterId equals flr.mFloorMasterId into Flr_
                               join mRm in db.mRoomMasters on mRd.mRoomMasterId equals mRm.mRoomMasterId into CUT_RMS
                               join ind in db.mIndooMaps on mRd.mIndooMapsId equals ind.mIndooMapsId into Ind_
                               where (mRd.OrgInfoId == orgId)
                               from RdData in CUT_RMS.DefaultIfEmpty()
                               from _Ind in Ind_.DefaultIfEmpty()
                               from _Flr in Flr_.DefaultIfEmpty()
                               select new
                               {
                                   subZone = _Flr.FloorName,
                                   mRd.mReaderSettupId,
                                   _Ind.FloorName,
                                   _Ind.FloorNo,
                                   _Ind.UID,
                                   _Ind.ImgPath,
                                   RdData.RoomName

                               }).ToList();
                result = this.Json(new { Flag = true, Message = "Suceess Data", mData, ObjData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                result = this.Json(new { Flag = false, Message = ex.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        // POST: IndoorMap/GetIndoorMaps
        [HttpGet]
        public JsonResult GetAttenaLoc()
        {
            // Initialization.    
            JsonResult result = new JsonResult();
            //var UserName = Session["AppUserName"];
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            try
            {
                var mData = (from mRd in db.mReaderSettups
                             join flr in db.mFloorMasters on mRd.mFloorMasterId equals flr.mFloorMasterId into Flr_
                             join mRm in db.mRoomMasters on mRd.mRoomMasterId equals mRm.mRoomMasterId into CUT_RMS
                             where (mRd.OrgInfoId == orgId && mRd.IsAction == true)
                             from RdData in CUT_RMS.DefaultIfEmpty()
                             from _Flr in Flr_.DefaultIfEmpty()
                             select new
                             {

                                 subZone = _Flr.FloorName,
                                 AttLoc = RdData.RoomName,
                                 mRd.ReaderNo,
                                 mRd.AttPortId,
                                 mRd.ReaderIP,
                                 mRd.mReaderSettupId,
                                 mRd.mIndooMapsId,
                                 mRd.Xaxis,
                                 mRd.Yaxis
                             }).ToList();

                result = this.Json(new { Flag = true, Message = "Suceess Data", mData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                result = this.Json(new { Flag = false, Message = ex.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }

    }
}
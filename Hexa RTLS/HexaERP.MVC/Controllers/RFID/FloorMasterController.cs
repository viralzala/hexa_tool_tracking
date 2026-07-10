using HexaERP.MVC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.UserManagment
{
    public class FloorMasterController : Controller
    {
        //******************
        //Author: Mudassar I
        //Date: 24/02/2017
        //FloorMasterController
        //******************

        private ERPdbEntities db = new ERPdbEntities();
        // GET: FloorMaster
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

                if (Session["UniqueId"].ToString() != "" && Session["OrgInfoId"].ToString() != "" && Session["AppUserName"].ToString() != "")
                {
                    //string Page_Name = Path.GetFileName(Request.Path);
                    if (!new string[] { "AD", "SA" }.Contains(Convert.ToString(Session["SortCode"])))
                    {
                        return RedirectToAction("Index", "AppUser");
                    }
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



        // GET: FloorMaster/Details
        [HttpGet]
        public JsonResult GetCollData()
        {
            try
            {
                using (ERPdbEntities bbObj = new ERPdbEntities())
                {
                    var getDatas = (from sz in bbObj.mFloorMasters
                                    join st in bbObj.mSiteMasters on sz.mSiteMasterId equals st.mSiteMasterId into stsn
                                    join zn in bbObj.mZones on sz.mZoneId equals zn.mZoneId into znsz

                                    from stsnd in stsn
                                    from znszd in znsz
                                    select new { sz.mFloorMasterId, stsnd.Site, znszd.Zone, sz.FloorName }
                                 ).ToList();
                    return Json(new { Flag = true, Message = "Data Loaded Sucessfully", IData = getDatas }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        // POST: FloorMaster/Create
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult Create(mFloorMaster collection)
        {
            bool _Flag = false; string Message = string.Empty;
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                if (string.IsNullOrEmpty(collection.FloorName))
                {
                    _Flag = false; Message = "Sub Zone Should Not be Empty";
                }
                else if (collection.mSiteMasterId == null || collection.mZoneId == null)
                {
                    _Flag = false; Message = "Missing Location/Zone Should Not be Empty";
                }
                else
                {
                    if (db.mFloorMasters.Any(o => o.FloorName == collection.FloorName && o.OrgInfoId == orgId))
                    {
                        _Flag = false; Message = "Same Data Alerdy Exist";
                    }
                    else
                    {
                        collection.IsAction = true; collection.OrgInfoId = orgId; collection.CreatedBy = UserName.ToString();
                        collection.CreatedDate = DateTime.Now;
                        db.mFloorMasters.Add(collection);
                        db.SaveChanges();
                        _Flag = true; Message = "Sub Zone Added Successfully";
                    }
                }

            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }

        // GET: FloorMaster/Edit/5
        [HttpGet]
        public JsonResult Edit(int id)
        {
            bool _Flag = false; string Message = string.Empty;
            if (string.IsNullOrEmpty(Convert.ToString(id)))
            {
                _Flag = false; Message = "Id Could Not Found";
                return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
            }

            var EditData = db.mFloorMasters.Find(id);

            if (EditData == null)
            {
                _Flag = false; Message = "Data Could Not Found";
                return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                _Flag = true; Message = "Successfully";
                return Json(new { Flag = _Flag, Message = Message, Idata = EditData }, JsonRequestBehavior.AllowGet);
            }

        }

        // POST: FloorMaster/Edit/5
        [HttpPost]
        public JsonResult Edit(mFloorMaster collection)
        {
            bool _Flag = false; string Message = string.Empty;
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                if (string.IsNullOrEmpty(collection.FloorName))
                {
                    _Flag = false; Message = "Sub Zone Should Not be Empty";
                }
                else
                {

                    if (db.mFloorMasters.Any(o => o.FloorName == collection.FloorName && o.OrgInfoId == orgId))
                    {
                        _Flag = false; Message = "Same Data Alerdy Exist please choose different";
                    }
                    else
                    {
                        mFloorMaster EditObj = db.mFloorMasters.Find(collection.mFloorMasterId);
                        EditObj.FloorName = collection.FloorName;
                        EditObj.OrgInfoId = orgId; EditObj.ModifiedBy = UserName.ToString();
                        EditObj.ModifiedDate = DateTime.Now;
                        db.Entry(EditObj).State = EntityState.Modified;
                        db.SaveChanges();
                        _Flag = true; Message = "Sub Zone Updated Successfully";
                    }
                }

            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }
        // GET: FloorMaster/Delete/5
        [HttpGet]
        public JsonResult Delete(int id)
        {
            bool _Flag = false; string Message = string.Empty;
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                if (string.IsNullOrEmpty(Convert.ToString(id)))
                {
                    _Flag = false; Message = "Error While Deleting Record";
                }
                else
                {
                    mFloorMaster delObj = db.mFloorMasters.Find(id);
                    if (delObj == null)
                    {
                        _Flag = false; Message = "Data Not Found";
                    }
                    else
                    {
                        var _mRoomMaster = $"DELETE FROM mRoomMaster Where mZoneId = {delObj.mZoneId}";
                        db.Database.ExecuteSqlCommand(_mRoomMaster);


                        var _mReaderSettup = $"DELETE FROM mReaderSettup Where mZoneId = {delObj.mZoneId}";
                        db.Database.ExecuteSqlCommand(_mReaderSettup);

                        db.mFloorMasters.Remove(delObj);
                        db.SaveChanges();
                        _Flag = true; Message = "Record Deleted Successfully :" + delObj.FloorName;
                    }
                }
            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }

        // GET: FloorMaster/SiteDataColl
        [HttpGet]
        public JsonResult setDropData()
        {
            try
            {
                var getSite = db.mSiteMasters.Where(x => x.IsAction == true).ToList();
                return Json(new { Flag = true, Message = "Data Loaded Sucessfully", DSite = getSite.ToArray() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: FloorMaster/SiteDataColl
        [HttpGet]
        public JsonResult getZones(int id)
        {
            try
            {
                var getZone = db.mZones.Where(x => x.IsAction == true && x.mSiteMasterId == id).ToList();
                return Json(new { Flag = true, Message = "Data Loaded Sucessfully", DZone = getZone.ToArray() }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}

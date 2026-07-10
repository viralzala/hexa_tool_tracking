using HexaERP.MVC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class RoomMasterController : Controller
    {
        //******************
        //Author: Mudassar I
        //Date: 24/02/2017
        //RoomMasterController
        //******************

        private ERPdbEntities db = new ERPdbEntities();
        // GET: RoomMaster
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


        // GET: RoomMaster/Details
        [HttpGet]
        public JsonResult GetCollData()
        {
            try
            {
                using (ERPdbEntities bbObj = new ERPdbEntities())
                {
                    var getDatas = (from sz in bbObj.mRoomMasters
                                    join st in bbObj.mSiteMasters on sz.mSiteMasterId equals st.mSiteMasterId into stsn
                                    join zn in bbObj.mZones on sz.mZoneId equals zn.mZoneId into znsz
                                    join sbz in bbObj.mFloorMasters on sz.mFloorMasterId equals sbz.mFloorMasterId into szsbz

                                    from stsnd in stsn
                                    from znszd in znsz
                                    from szsbzd in szsbz
                                    select new { sz.mRoomMasterId, stsnd.Site, znszd.Zone, szsbzd.FloorName, sz.RoomName }
                                 ).ToList();
                    return Json(new { Flag = true, Message = "Data Loaded Sucessfully", IData = getDatas }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        // POST: RoomMaster/Create
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult Create(mRoomMaster collection)
        {
            bool _Flag = false; string Message = string.Empty;
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                if (string.IsNullOrEmpty(collection.RoomName))
                {
                    _Flag = false; Message = "Area Should Not be Empty";
                }
                else if (collection.mSiteMasterId == null || collection.mZoneId == null || collection.mFloorMasterId == null)
                {
                    _Flag = false; Message = "Missing Location/Zone/Sub Zone Should Not be Empty";
                }
                else
                {
                    if (db.mRoomMasters.Any(o => o.RoomName == collection.RoomName && o.OrgInfoId == orgId))
                    {
                        _Flag = false; Message = "Same Data Alerdy Exist";
                    }
                    else
                    {
                        collection.IsAction = true; collection.OrgInfoId = orgId; collection.CreatedBy = UserName.ToString();
                        collection.CreatedDate = DateTime.Now;
                        db.mRoomMasters.Add(collection);
                        db.SaveChanges();
                        _Flag = true; Message = "Antenna Location Added Successfully";
                    }
                }

            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }

        // GET: RoomMaster/Edit/5
        [HttpGet]
        public JsonResult Edit(int id)
        {
            bool _Flag = false; string Message = string.Empty;
            if (string.IsNullOrEmpty(Convert.ToString(id)))
            {
                _Flag = false; Message = "Id Could Not Found";
                return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
            }

            var EditData = db.mRoomMasters.Find(id);

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

        // POST: RoomMaster/Edit/5
        [HttpPost]
        public JsonResult Edit(mRoomMaster collection)
        {
            bool _Flag = false; string Message = string.Empty;
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                if (string.IsNullOrEmpty(collection.RoomName))
                {
                    _Flag = false; Message = "Area Should Not be Empty";
                }
                else
                {
                    if (db.mRoomMasters.Any(o => o.RoomName == collection.RoomName && o.OrgInfoId == orgId))
                    {
                        _Flag = false; Message = "Same Data Alerdy Exist please choose different";
                    }
                    else
                    {
                        mRoomMaster EditObj = db.mRoomMasters.Find(collection.mRoomMasterId);
                        EditObj.RoomName = collection.RoomName;
                        EditObj.OrgInfoId = orgId; EditObj.ModifiedBy = UserName.ToString();
                        EditObj.ModifiedDate = DateTime.Now;
                        db.Entry(EditObj).State = EntityState.Modified;
                        db.SaveChanges();
                        _Flag = true; Message = "Antenna Location Updated Successfully";
                    }
                }
            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }
        // GET: RoomMaster/Delete/5
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
                    mRoomMaster delObj = db.mRoomMasters.Find(id);





                    if (delObj == null)
                    {
                        _Flag = false; Message = "Data Not Found";
                    }
                    else
                    {

                        db.mRoomMasters.Remove(delObj);
                        db.SaveChanges();

                        var mReaderSettup = db.mReaderSettups.SingleOrDefault(x => x.mRoomMasterId == id); //returns a single item.
                        if (mReaderSettup != null)
                        {
                            db.mReaderSettups.Remove(mReaderSettup);
                            db.SaveChanges();
                        }

                        _Flag = true; Message = "Record Deleted Successfully :" + delObj.RoomName;
                    }
                }
            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }

        // GET: RoomMaster/SiteDataColl
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

        // GET: RoomMaster/getZones
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

        // GET: RoomMaster/getSubZones
        [HttpGet]
        public JsonResult getSubZones(int id)
        {
            try
            {
                var getSubZone = db.mFloorMasters.Where(x => x.IsAction == true && x.mZoneId == id).ToList();
                return Json(new { Flag = true, Message = "Data Loaded Sucessfully", DZone = getSubZone.ToArray() }, JsonRequestBehavior.AllowGet);

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

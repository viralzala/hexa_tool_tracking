using HexaERP.MVC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class ZoneController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();
        // GET: Zone
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
                    //--- To read values from cookie collection we will use Keys used while creating cookie.                   
                    // string AppUserName = cookieObject["AppUserName"];
                    //string UniqueId = cookieObject["UniqueId"];
                    //string OrgInfoId = cookieObject["OrgInfoId"];
                    //string SortCode = cookieObject["SortCode"];
                }
                else { return RedirectToAction("Index", "AppUser"); }

                if (Session["UniqueId"].ToString() != "" && Session["OrgInfoId"].ToString() != "" && Session["AppUserName"].ToString() != "")
                {
                    //string Page_Name = Path.GetFileName(Request.Path);
                    if (!new string[] { "AD", "SA" }.Contains(Convert.ToString(Session["SortCode"])))
                    {
                        return RedirectToAction("Index", "AppUser");
                    }



                    //AppUserName = Session["AppUserName"].ToString(); UniqueId = Session["UniqueId"].ToString();
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

        // GET: Zone/Details
        [HttpGet]
        public JsonResult GetCollData()
        {
            try
            {
                using (ERPdbEntities bbObj = new ERPdbEntities())
                {
                    var getDatas = (from zn in bbObj.mZones
                                    join st in bbObj.mSiteMasters on zn.mSiteMasterId equals st.mSiteMasterId
                                    select new { zn.mZoneId, zn.Zone, st.Site }
                                 ).ToList();
                    return Json(new { Flag = true, Message = "Data Loaded Sucessfully", IData = getDatas }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        // POST: Zone/Create
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult Create(mZone collection)
        {
            bool _Flag = false; string Message = string.Empty;
            try
            {
                var UserName = Session["AppUserName"];
                // int orgId = Convert.ToInt32(Session["OrgInfoId"]);

                if (string.IsNullOrEmpty(collection.Zone))
                    return Json(new { Flag = false, Message = $"Zone Should Not be Empty" }, JsonRequestBehavior.AllowGet);

                if (collection.mSiteMasterId == null)
                    return Json(new { Flag = false, Message = $"Location Should Not be Empty" }, JsonRequestBehavior.AllowGet);

                if (collection.IsAction ?? false)
                {
                    if (string.IsNullOrWhiteSpace(collection.CreatedBy))
                        return Json(new { Flag = false, Message = $"Please enter Prefix" }, JsonRequestBehavior.AllowGet);

                    if (collection.OrgInfoId == null || collection.OrgInfoId == 0)
                        return Json(new { Flag = false, Message = $"Please select number of row" }, JsonRequestBehavior.AllowGet);
                }

                //if (db.mZones.Count() > 5)
                //    return Json(new { Flag = false, Message = "Only 5 zones are allowed" }, JsonRequestBehavior.AllowGet);

                if (db.mZones.Any(o => o.Zone == collection.Zone))
                    return Json(new { Flag = false, Message = $"Same Data Alerdy Exist" }, JsonRequestBehavior.AllowGet);
                else
                {
                    string _prefix = string.Empty;

                    if (!string.IsNullOrWhiteSpace(collection.CreatedBy))
                        _prefix = collection.CreatedBy.Trim();

                    collection.IsAction = true;
                    collection.CreatedBy = UserName.ToString();
                    collection.CreatedDate = DateTime.Now;

                    db.mZones.Add(collection);
                    db.SaveChanges();


                    if (collection.IsAction ?? false)
                    {
                        var _checkForExist = db.mShelves.Where(m => m.mZoneId == collection.mZoneId).FirstOrDefault();
                        if (_checkForExist == null)
                        {
                            //string[] _Alpha = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T" };
                            //foreach (var variableName in _Alpha)
                            //{                                   
                            //}
                            var _rows = Convert.ToInt32(collection.OrgInfoId);

                            for (var i = 1; i <= _rows; i++)
                            {
                                db.mShelves.Add(new mShelf
                                {
                                    mZoneId = collection.mZoneId,
                                    CreatedBy = UserName.ToString(),
                                    CreatedDate = DateTime.Now,
                                    IsAction = false,
                                    ShelfName = $"{_prefix}{i}"
                                });
                                db.SaveChanges();
                            }
                        }
                    }

                    //collection.IsAction = true;
                    //collection.OrgInfoId = orgId;
                    //collection.CreatedBy = UserName.ToString();
                    //collection.CreatedDate = DateTime.Now;
                    //collection.ModifiedBy = UserName.ToString();


                    _Flag = true; Message = "Zone Added Successfully";
                }
            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }

        // GET: Zone/Edit/5
        [HttpGet]
        public JsonResult Edit(int id)
        {
            bool _Flag = false; string Message = string.Empty;
            if (string.IsNullOrEmpty(Convert.ToString(id)))
            {
                _Flag = false; Message = "Id Could Not Found";
                return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
            }

            var EditData = db.mZones.Find(id);

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

        // POST: Zone/Edit/5
        [HttpPost]
        public JsonResult Edit(mZone collection)
        {
            bool _Flag = false; string Message = string.Empty;
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                if (string.IsNullOrEmpty(collection.Zone))
                {
                    _Flag = false; Message = "Zone Should Not be Empty";
                }
                else
                {

                    if (db.mZones.Any(o => o.Zone == collection.Zone && o.OrgInfoId == orgId))
                    {
                        _Flag = false; Message = "Same Data Alerdy Exist please choose different";
                    }
                    else
                    {
                        mZone EditObj = db.mZones.Find(collection.mZoneId);
                        EditObj.Zone = collection.Zone;
                        EditObj.OrgInfoId = orgId; EditObj.ModifiedBy = UserName.ToString();
                        EditObj.ModifiedDate = DateTime.Now;
                        db.Entry(EditObj).State = EntityState.Modified;
                        db.SaveChanges();
                        _Flag = true; Message = "Zone Updated Successfully";
                    }
                }

            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }
        // GET: Zone/Delete/5
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
                    mZone delObj = db.mZones.Find(id);
                    if (delObj == null)
                    {
                        _Flag = false; Message = "Data Not Found";
                    }
                    else
                    {
                        var _mShelf = $"DELETE FROM mShelf Where mZoneId ={delObj.mZoneId}";
                        db.Database.ExecuteSqlCommand(_mShelf);

                        var _mFloorMaster = $"DELETE FROM mFloorMaster Where mZoneId ={delObj.mZoneId}";
                        db.Database.ExecuteSqlCommand(_mFloorMaster);

                        var _mRoomMaster = $"DELETE FROM mRoomMaster Where mZoneId = {delObj.mZoneId}";
                        db.Database.ExecuteSqlCommand(_mRoomMaster);


                        var _mReaderSettup = $"DELETE FROM mReaderSettup Where mZoneId = {delObj.mZoneId}";
                        db.Database.ExecuteSqlCommand(_mReaderSettup);

                        db.mZones.Remove(delObj);
                        db.SaveChanges();
                        _Flag = true; Message = "Record Deleted Successfully :" + delObj.Zone;
                    }
                }
            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }

        // GET: Site/SiteDataColl
        [HttpGet]
        public JsonResult SiteDataColl()
        {
            try
            {
                using (ERPdbEntities bbObj = new ERPdbEntities())
                {

                    var getDatas = bbObj.mSiteMasters.Where(x => x.IsAction == true).ToList();
                    return Json(new { Flag = true, Message = "Data Loaded Sucessfully", IData = getDatas }, JsonRequestBehavior.AllowGet);
                }
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

using EntityFramework.Extensions;
using HexaERP.MVC.Models;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class UploadMapController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();
        // GET: UploadMap
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

        //
        public ActionResult FileUpload(HttpPostedFileBase file)
        {
            if (file != null)
            {
                string pic = System.IO.Path.GetFileName(file.FileName);
                string path = System.IO.Path.Combine(Server.MapPath("~/images/profile"), pic);
                // file is uploaded
                //file.SaveAs(path);

                // save the image path path to the database or you can send image 
                // directly to database
                // in-case if you want to store byte[] ie. for DB
                using (MemoryStream ms = new MemoryStream())
                {
                    file.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }

            }
            // after successfully uploading redirect the user
            return RedirectToAction("actionname", "controller name");
        }
        //
        [HttpPost]
        public ActionResult Index(mIndooMap obj, HttpPostedFileBase file, string actionType)
        {
            if (file != null)
            {
                if (actionType == "Upload")
                {
                    try
                    {
                        var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                        if (string.IsNullOrEmpty(obj.FloorName) || string.IsNullOrEmpty(obj.FloorNo))
                        {
                            ModelState.AddModelError(String.Empty, "Enter Map Details"); return View(obj);
                        }
                        else if (db.mIndooMaps.Any(o => o.FloorName == obj.FloorName && o.OrgInfoId == orgId))
                        {
                            ModelState.AddModelError("", "Same Data Alerdy Exist");
                            return View(obj);
                        }
                        else
                        {
                            string Uid = DateTime.Now.ToString().GetHashCode().ToString("x"); string extension = Path.GetExtension(file.FileName);
                            string fileName = Uid + extension;
                            string path = Path.Combine(Server.MapPath("Files/Maps"), fileName);
                            string _path = "Files/Maps/" + fileName;
                            obj.ImgPath = _path;
                            obj.UID = Uid;
                            obj.IsAction = true; obj.OrgInfoId = orgId; obj.CreatedBy = UserName.ToString();
                            obj.CreatedDate = DateTime.Now;
                            db.mIndooMaps.Add(obj);
                            db.SaveChanges();
                            //file is uploaded
                            file.SaveAs(path);
                            // save the image path path to the database or you can send image 
                            // directly to database
                            // in-case if you want to store byte[] ie. for DB
                            //using (MemoryStream ms = new MemoryStream())
                            //{
                            //    file.InputStream.CopyTo(ms);
                            //    byte[] array = ms.GetBuffer();
                            //}
                            ModelState.AddModelError(String.Empty, "Map uploaded successfully.");
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", ex.Message.ToString());
                        return View(obj);
                    }
                }
            }
            else { ModelState.AddModelError(String.Empty, "You must select floor map"); return View(obj); }
            // photo.SaveAs(path);
            return View(obj);
        }

        //
        [HttpGet]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteIndMap(int _id)
        {
            // Initialization.    
            JsonResult result = new JsonResult();
            //var UserName = Session["AppUserName"];
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            try
            {
                //var _seNull = db.mReaderSettups.Where(x => x.mIndooMapsId == _id).ToList();
                db.mReaderSettups.Where(c => c.mIndooMapsId == _id).Update(c => new mReaderSettup()
                {
                    Xaxis = null,
                    Yaxis = null,
                    mIndooMapsId = null
                });

                try
                {
                    mIndooMap mIndr = db.mIndooMaps.Find(_id);
                    string appPath = Request.PhysicalApplicationPath;
                    string fullPath = appPath + mIndr.ImgPath;
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }

                    db.mIndooMaps.Remove(mIndr);
                    db.SaveChanges();

                    result = this.Json(new { Flag = true, Message = "Suceess Data" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    result = this.Json(new { Flag = false, Message = ex.InnerException.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                result = this.Json(new { Flag = false, Message = ex.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }


        // GET: UploadMap/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: UploadMap/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }



        // POST: UploadMap/GetIndoorMaps
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

        // POST: UploadMap/GetIndoorMaps
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
                                 mRd.mReaderSettupId,
                                 mRd.mIndooMapsId,
                                 mRd.Xaxis,
                                 mRd.Yaxis,
                                 subZone = _Flr.FloorName,
                                 AttLoc = RdData.RoomName,
                                 mRd.ReaderNo,
                                 mRd.AttPortId,
                                 mRd.ReaderIP
                             }).ToList();

                result = this.Json(new { Flag = true, Message = "Suceess Data", mData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                result = this.Json(new { Flag = false, Message = ex.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }

        // POST: UploadMap/GetIndoorMaps
        [HttpGet]
        public JsonResult SetAxisMaps(int Xaxi, int Yaxi, int mIndoMapId, int mAtteId)
        {
            // Initialization.    
            JsonResult result = new JsonResult();
            //var UserName = Session["AppUserName"];
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            try
            {
                if (string.IsNullOrEmpty(Xaxi.ToString()) || string.IsNullOrEmpty(Yaxi.ToString()))
                {
                    result = this.Json(new { Flag = false, Message = "Map Pixcel are missing" }, JsonRequestBehavior.AllowGet);
                }
                else if (string.IsNullOrEmpty(mIndoMapId.ToString()) || string.IsNullOrEmpty(mAtteId.ToString()))
                {
                    result = this.Json(new { Flag = false, Message = "somrthing went wrong please refresh the application." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var mData = db.mReaderSettups.Find(mAtteId);
                    if (mData != null)
                    {
                        if (mData.mIndooMapsId == null)
                        {
                            mData.mIndooMapsId = mIndoMapId;
                            mData.Xaxis = Xaxi;
                            mData.Yaxis = Yaxi;
                            db.Entry(mData).State = EntityState.Modified;
                            db.SaveChanges();
                            result = this.Json(new { Flag = true, Message = "Location assigned", mData }, JsonRequestBehavior.AllowGet);
                        }
                        else if (mData.mIndooMapsId != null)
                        {
                            mData.mIndooMapsId = mIndoMapId;
                            mData.Xaxis = Xaxi;
                            mData.Yaxis = Yaxi;
                            db.Entry(mData).State = EntityState.Modified;
                            db.SaveChanges();
                            result = this.Json(new { Flag = true, Message = "Location Updated", mData }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = this.Json(new { Flag = false, Message = ex.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
            return result;
        }
    }
}

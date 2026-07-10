using HexaERP.MVC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class WorkCategoryController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();
        // GET: WorkCategory
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


        // POST: WorkCategory/Create
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult Create(mWorkCategory collection)
        {
            bool _Flag = false; string Message = string.Empty;
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                if (string.IsNullOrEmpty(collection.WorkCategory))
                {
                    _Flag = false; Message = "Work Category Should Not be Empty";
                }
                else
                {
                    if (db.mWorkCategories.Any(o => o.WorkCategory == collection.WorkCategory && o.OrgInfoId == orgId))
                    {
                        _Flag = false; Message = "Same Designation Alerdy Exist";
                    }
                    else
                    {
                        collection.IsAction = true; collection.OrgInfoId = orgId; collection.CreatedBy = UserName.ToString();
                        collection.CreatedDate = DateTime.Now;
                        db.mWorkCategories.Add(collection);
                        db.SaveChanges();
                        _Flag = true; Message = "Work Category Added Successfully";
                    }
                }

            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }

        // GET: WorkCategory/Edit/5
        [HttpGet]
        public JsonResult Edit(int id)
        {
            bool _Flag = false; string Message = string.Empty;

            if (string.IsNullOrEmpty(Convert.ToString(id)))
            {
                _Flag = false; Message = "Id Could Not Found";
                return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
            }

            var EditData = db.mWorkCategories.Find(id);

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

        // POST: WorkCategory/Edit/5
        [HttpPost]
        public JsonResult Edit(mWorkCategory collection)
        {
            bool _Flag = false; string Message = string.Empty;
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                if (string.IsNullOrEmpty(collection.WorkCategory))
                {
                    _Flag = false; Message = "Work Category Should Not be Empty";
                }
                else
                {

                    if (db.mWorkCategories.Any(o => o.WorkCategory == collection.WorkCategory && o.OrgInfoId == orgId))
                    {
                        _Flag = false; Message = "Same Data Alerdy Exist please choose different";
                    }
                    else
                    {
                        mWorkCategory EditObj = db.mWorkCategories.Find(collection.mWorkCategoryId);
                        EditObj.WorkCategory = collection.WorkCategory;
                        EditObj.OrgInfoId = orgId; EditObj.ModifiedBy = UserName.ToString();
                        EditObj.ModifiedDate = DateTime.Now;
                        db.Entry(EditObj).State = EntityState.Modified;
                        db.SaveChanges();
                        _Flag = true; Message = "Work Category Updated Successfully";
                    }
                }

            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }

        // GET: WorkCategory/Delete/5
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
                    mWorkCategory delObj = db.mWorkCategories.Find(id);
                    if (delObj == null)
                    {
                        _Flag = false; Message = "Data Not Found";
                    }
                    else
                    {
                        db.mWorkCategories.Remove(delObj);
                        db.SaveChanges();
                        _Flag = true; Message = "Record Deleted Successfully :" + delObj.WorkCategory;
                    }
                }
            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }

        //GET: WorkCategory/GetCollData
        [HttpGet]
        public JsonResult GetCollData()
        {
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                using (ERPdbEntities bbObj = new ERPdbEntities())
                {
                    var getDatas = bbObj.mWorkCategories.Where(x => x.IsAction == true && x.OrgInfoId == orgId).ToList();
                    return Json(new { Flag = true, Message = "Data Loaded Sucessfully", IData = getDatas }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        //Dispose
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


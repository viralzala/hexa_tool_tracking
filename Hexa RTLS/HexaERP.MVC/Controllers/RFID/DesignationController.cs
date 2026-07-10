using HexaERP.MVC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class DesignationController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();
        // GET: Designation
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

        // POST: Designation/Create
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult Create(mDesignation collection)
        {
            bool _Flag = false; string Message = string.Empty;
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                if (string.IsNullOrEmpty(collection.Designation))
                {
                    _Flag = false; Message = "Designation Should Not be Empty";
                }
                else
                {
                    if (db.mDesignations.Any(o => o.Designation == collection.Designation && o.OrgInfoId == orgId))
                    {
                        _Flag = false; Message = "Same Designation Alerdy Exist";
                    }
                    else
                    {
                        collection.IsAction = true; collection.OrgInfoId = orgId; collection.CreatedBy = UserName.ToString();
                        collection.CreatedDate = DateTime.Now;
                        db.mDesignations.Add(collection);
                        db.SaveChanges();
                        _Flag = true; Message = "Designation Added Successfully";
                    }
                }

            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message }, JsonRequestBehavior.AllowGet);
        }

        // GET: Designation/Edit/5
        [HttpGet]
        public JsonResult Edit(int id)
        {
            bool _Flag = false; string Message = string.Empty;
            if (string.IsNullOrEmpty(Convert.ToString(id)))
            {
                _Flag = false; Message = "Id Could Not Found";
                return Json(new { Flag = _Flag, Message }, JsonRequestBehavior.AllowGet);
            }

            var EditData = db.mDesignations.Find(id);

            if (EditData == null)
            {
                _Flag = false; Message = "Data Could Not Found";
                return Json(new { Flag = _Flag, Message }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                _Flag = true; Message = "Successfully";
                return Json(new { Flag = _Flag, Message, Idata = EditData }, JsonRequestBehavior.AllowGet);
            }

        }

        // POST: Designation/Edit/5
        [HttpPost]
        public JsonResult Edit(mDesignation collection)
        {
            bool _Flag = false; string Message = string.Empty;
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                if (string.IsNullOrEmpty(collection.Designation))
                {
                    _Flag = false; Message = "Designation Should Not be Empty";
                }
                else
                {

                    if (db.mDesignations.Any(o => o.Designation == collection.Designation && o.OrgInfoId == orgId))
                    {
                        _Flag = false; Message = "Same Data Alerdy Exist please choose different";
                    }
                    else
                    {
                        mDesignation EditObj = db.mDesignations.Find(collection.mDesignationId);
                        EditObj.Designation = collection.Designation;
                        EditObj.OrgInfoId = orgId; EditObj.ModifiedBy = UserName.ToString();
                        EditObj.ModifiedDate = DateTime.Now;
                        db.Entry(EditObj).State = EntityState.Modified;
                        db.SaveChanges();
                        _Flag = true; Message = "Designation Updated Successfully";
                    }
                }

            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message }, JsonRequestBehavior.AllowGet);
        }

        // GET: Designation/Delete/5
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
                    mDesignation delObj = db.mDesignations.Find(id);
                    if (delObj == null)
                    {
                        _Flag = false; Message = "Data Not Found";
                    }
                    else
                    {
                        db.mDesignations.Remove(delObj);
                        db.SaveChanges();
                        _Flag = true; Message = "Record Deleted Successfully :" + delObj.Designation;
                    }
                }
            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message }, JsonRequestBehavior.AllowGet);
        }

        //GET: Designation/GetCollData
        [HttpGet]
        public JsonResult GetCollData()
        {
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                using (ERPdbEntities bbObj = new ERPdbEntities())
                {
                    var getDatas = bbObj.mDesignations.Where(x => x.IsAction == true && x.OrgInfoId == orgId).ToList();
                    return Json(new { Flag = true, Message = "Data Loaded Sucessfully", IData = getDatas }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, ex.Message }, JsonRequestBehavior.AllowGet);
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


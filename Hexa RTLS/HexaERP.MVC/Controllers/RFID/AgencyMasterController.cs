using HexaERP.MVC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class AgencyMasterController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();
        // GET: AgencyMaster
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

        // POST: AgencyMaster/Create
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult Create(mAgency collection)
        {
            bool _Flag = false; string Message = string.Empty;
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                if (string.IsNullOrEmpty(collection.Agency))
                {
                    _Flag = false; Message = "Agency Should Not be Empty";
                }
                else
                {
                    if (db.mAgencies.Any(o => o.Agency == collection.Agency && o.OrgInfoId == orgId))
                    {
                        _Flag = false; Message = "Same Data Alerdy Exist";
                    }
                    else
                    {
                        collection.IsAction = true; collection.OrgInfoId = orgId; collection.CreatedBy = UserName.ToString();
                        collection.CreatedDate = DateTime.Now;
                        db.mAgencies.Add(collection);
                        db.SaveChanges();
                        _Flag = true; Message = "Agency Added Successfully";
                    }
                }

            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }

        // GET: AgencyMaster/Edit/5
        [HttpGet]
        public JsonResult Edit(int id)
        {
            bool _Flag = false; string Message = string.Empty;
            if (string.IsNullOrEmpty(Convert.ToString(id)))
            {
                _Flag = false; Message = "Id Could Not Found";
                return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
            }

            var EditData = db.mAgencies.Find(id);

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

        // POST: AgencyMaster/Edit/5
        [HttpPost]
        public JsonResult Edit(mAgency collection)
        {
            bool _Flag = false; string Message = string.Empty;
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                if (string.IsNullOrEmpty(collection.Agency))
                {
                    _Flag = false; Message = "Agency Should Not be Empty";
                }
                else
                {

                    if (db.mAgencies.Any(o => o.Agency == collection.Agency && o.OrgInfoId == orgId))
                    {
                        _Flag = false; Message = "Same Data Alerdy Exist please choose different";
                    }
                    else
                    {
                        mAgency magenObj = db.mAgencies.Find(collection.mAgencyId);
                        magenObj.Agency = collection.Agency;
                        magenObj.OrgInfoId = orgId; magenObj.ModifiedBy = UserName.ToString();
                        magenObj.ModifiedDate = DateTime.Now;
                        db.Entry(magenObj).State = EntityState.Modified;
                        db.SaveChanges();
                        _Flag = true; Message = "Agency Updated Successfully";
                    }
                }

            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }

        // GET: AgencyMaster/Delete/5
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
                    mAgency mAgencyobj = db.mAgencies.Find(id);
                    if (mAgencyobj == null)
                    {
                        _Flag = false; Message = "Data Not Found";
                    }
                    else
                    {
                        db.mAgencies.Remove(mAgencyobj);
                        db.SaveChanges();
                        _Flag = true; Message = "Record Deleted Successfully :" + mAgencyobj.Agency;
                    }
                }
            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }

        //
        [HttpGet]
        public JsonResult GetAllAgency()
        {
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                using (ERPdbEntities bbObj = new ERPdbEntities())
                {
                    var Ags = bbObj.mAgencies.Where(x => x.IsAction == true && x.OrgInfoId == orgId).ToList();
                    return Json(new { Flag = true, Message = "Data Loaded Sucessfully", IData = Ags }, JsonRequestBehavior.AllowGet);
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

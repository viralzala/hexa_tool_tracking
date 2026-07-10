using HexaERP.MVC.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class AssetCategoryController : Controller
    {
        //******************
        //Author: Mudassar I
        //Date: 03/05/2017
        //FloorMasterController
        //******************

        private ERPdbEntities db = new ERPdbEntities();
        // GET: LeadDispositionMaster
        public ActionResult Index()
        {
            try
            {
                //--- Get cookie Collection.
                HttpCookie cookieObject = Request.Cookies["HexaCookie"];
                //--- Check for null 
                if (cookieObject != null)
                {

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
        [HttpGet]
        public JsonResult getLoadData()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = db.mGroupMasters.Where(o => o.OrgInfoId == orgId).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //Get All Department
        [HttpGet]
        public JsonResult getData()
        {
            var UserName = Session["AppUserName"];
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = db.mGroupMasters.Where(o => o.OrgInfoId == orgId && o.IsAction == true).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //Save New Departmnt
        [HttpPost]
        public string SaveData(string _GroupName)
        {
            var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            mGroupMaster obj = new mGroupMaster();
            string msg = "";
            try
            {
                if (db.mGroupMasters.Any(o => o.GroupName == _GroupName && o.OrgInfoId == orgId))
                {
                    return msg = "Same Name Alerdy Exist";
                }
                else
                {
                    obj.OrgInfoId = orgId;
                    obj.GroupName = _GroupName;
                    obj.CreatedDate = DateTime.Now; obj.CreatedBy = UserName.ToString(); obj.IsAction = true;
                    db.mGroupMasters.Add(obj);
                    db.SaveChanges();
                    msg = "Data Saved";
                }
            }
            catch (Exception)
            {
            }
            return msg;
        }
        //Get Dept By Id
        [HttpGet]
        public JsonResult getDataWithId(int ID)
        {
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            var datas = db.mGroupMasters.Where(o => o.mGroupMasterId == ID && o.OrgInfoId == orgId).ToList();
            return Json(datas, JsonRequestBehavior.AllowGet);
        }
        //Update Department
        [HttpGet]
        public string UpdateData(string _GroupName, int ID)
        {
            string msg = "";
            int orgId = Convert.ToInt32(Session["OrgInfoId"]); var UserName = Session["AppUserName"];
            var original = db.mGroupMasters.FirstOrDefault(b => b.mGroupMasterId == ID);
            if (original != null)
            {
                original.GroupName = _GroupName; original.ModifiedBy = UserName.ToString(); original.ModifiedDate = DateTime.Now;
                db.SaveChanges();
                msg = "Data Updated";
            }
            else
            {
                return msg = "Data is Not updated";
            }
            return msg;
        }
        //Delete Department
        [HttpGet]
        public string DeleteData(int ID)
        {
            string msg = "";
            mGroupMaster removeData = db.mGroupMasters.Find(ID);
            if (removeData != null)
            {
                db.mGroupMasters.Remove(removeData);
                db.SaveChanges();
                msg = "Deleted";
            }
            else
            {
                msg = "Unable to Deleted";
            }
            return msg;
        }
    }
}